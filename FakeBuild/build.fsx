// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#r "FAKE\FakeLib.dll"
#r "System.Xml.Linq"
#r "15below.VersionUpdater.dll"
open System
open System.IO
open System.Text.RegularExpressions
open Fake
open FifteenBelow.VersionUpdater

let configuration = "Release"
let core_version_number = environVarOrDefault "core_version_number" "0.0.0"
let build_number = environVarOrDefault "build_number" "0"
let teamcity_build_branch = environVarOrDefault "teamcity_build_branch" (sprintf "%s/release" core_version_number)
let vcsroot_branch = environVarOrDefault "vcsroot_branch" "development"
let nugetWebserviceDirectory = environVarOrDefault "nuget_repository_path" ""
let tempNuspecDir = currentDirectory @@ "NuSpecsTemp"
let outputDirectory = currentDirectory @@ "NugetFeed"

Target "UpdateVersions" (fun _ ->
    ReportProgressStart "Update Versions"
    let assemblyInfos = !! (sprintf "%s/**/AssemblyInfo.*" currentDirectory)
                        |> Seq.map (fun name -> new FileInfo(name))

    let returns = Update.Do(teamcity_build_branch, vcsroot_branch, core_version_number, build_number, true, assemblyInfos)
    setEnvironVar "AssemblyVersion" returns.AssemblyVersion
    setEnvironVar "NugetVersion" returns.NugetVersion
    setEnvironVar "SafeBranchName" returns.SafeBranchName
    SetTeamCityParameter "safe_branch" returns.SafeBranchName
    SetTeamCityParameter "nuget_version" returns.NugetVersion

    // Now we need to update the nuspec files as well...
    let updateNuspec nuspec =
        let versionRegex = new Regex("version>([^<]*)</version", RegexOptions.Compiled)
        let copyrightRegex = new Regex("copyright>([^<]*)</copyright", RegexOptions.Compiled)
        let fi = new FileInfo(nuspec)
        fi.ReadAll()
        |> fun (fileContents, info) -> versionRegex.Replace(fileContents, sprintf "version>%s</version" returns.NugetVersion), info
        |> fun (fileContents, info) -> copyrightRegex.Replace(fileContents, sprintf "copyright>Copyright 2004-%d</copyright" DateTime.Now.Year), info
        |> fun (fileContents, info) -> 
            info.Delete ()
            use writer = new StreamWriter(info.OpenWrite())
            writer.Write(fileContents)
    CleanDir tempNuspecDir
    !! "NuSpecs/*.nuspec"
    |> CopyTo tempNuspecDir
    
    !! (sprintf "%s/*.nuspec" tempNuspecDir)
    |> Seq.iter updateNuspec
)

Target "BuildSolution" (fun _ ->
    let setSolutionParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Properties = [
                            "Optimize", "True"
                            "DebugSymbols", "True"
                            "Configuration", configuration]
        }
    build setSolutionParams "Ensconce.sln"
)

Target "CreateNugets" (fun _ ->
    ReportProgressStart "Create Nugets"
    // Not using Fake's Nuget Helpers as they assume
    // you are using Nuget files as designed, not
    // building octopus packages.

    let toolPath = currentDirectory @@ "FakeBuild" @@ "NuGet.exe"
    let nuspecDirectory = currentDirectory @@ "NuSpecsTemp"
    let nuspecs = !! (sprintf @"%s\*.nuspec" nuspecDirectory)
    let timeOut = TimeSpan.FromMinutes 5.
    CleanDir outputDirectory

    let packNuspec nuspec = async {
        let args = sprintf @"pack ""%s"" -OutputDirectory ""%s"" -Properties Configuration=%s -NoPackageAnalysis" nuspec outputDirectory configuration
        let! result = async {
            return ExecProcess (fun info ->
                            info.FileName <- toolPath
                            info.WorkingDirectory <- nuspecDirectory
                            info.Arguments <- args) timeOut }
        if result <> 0 then failwithf "Error during Nuget creation. %s %s" toolPath args }

    nuspecs |> Seq.map packNuspec |> Async.Parallel |> Async.RunSynchronously |> ignore

    if buildServer = TeamCity then
        ActivateFinalTarget "PushNugetsAndArtifacts"


//    // This will only do anything if the build is running on TeamCity
//    // TODO: Submit patch with spelling correction... :)
//    PublishArticfact (sprintf "%s => Nuget" outputDirectory)
    ReportProgressFinish "Create Nugets"
)

FinalTarget "PushNugetsAndArtifacts" (fun _ ->
    let getPackageName (nuspecFileName : string) =
        let postfix = sprintf ".%s.nupkg" (environVar "NugetVersion")
        nuspecFileName.Replace(postfix, "")

    ReportProgressStart "Push Nugets"
    !! (outputDirectory @@ "*.nupkg")
    |> Seq.map (fun fileName -> new FileInfo(fileName))
    |> Seq.map (fun info -> info, (getPackageName info.Name))
    |> Seq.map (fun (info, name) -> 
        let fromPath = info.FullName
        let toPath = (environVar "nuget_repository_path") @@ name @@ info.Name
        File.Copy(fromPath, toPath)
        sprintf "%s --> %s" fromPath toPath)
    |> Log "Pushing file: "

    ReportProgressFinish "Push Nugets"
)

Target "Default" DoNothing

"UpdateVersions"
    ==> "BuildSolution"
    ==> "CreateNugets"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"

