// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#r "FAKE\FakeLib.dll"
#r "System.Xml.Linq"
#r "FAKE\15below.VersionUpdater.dll"
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
let pushUsingNugetWebService = environVarOrDefault "nuget_web_service_push" "true"
let nugetWebserviceDirectory = environVarOrDefault "nuget_repository_path" ""
let nugetWebService = environVarOrDefault "nuget_repository" "http://btn-tc01:8083"
let tempNuspecDir = currentDirectory @@ "NuSpecsTemp"
let outputDirectory = currentDirectory @@ "NugetFeed"
let toolPath = currentDirectory @@ "FakeBuild" @@ "NuGet.exe"

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
        match pushUsingNugetWebService with
        | "true" -> ActivateFinalTarget "PushNugetsAndArtifacts"
        |_ -> ActivateFinalTarget "FileCopyNugetsAndArtifacts"


//    // This will only do anything if the build is running on TeamCity
//    // TODO: Submit patch with spelling correction... :)
//    PublishArticfact (sprintf "%s => Nuget" outputDirectory)
    ReportProgressFinish "Create Nugets"
)

Target "RunUnitTests" (fun _ ->
    ReportProgressStart "Run Unit Tests"
    !+ (sprintf @"**\bin\%s\**\*.Tests.dll" configuration)
    |> Scan
    |> NUnit (fun defaults -> { defaults with
                                    ExcludeCategory = "Integration,IntegrationTest,IntegrationsTest,IntegrationTests,IntegrationsTests,Integration Test,Integration Tests,Integrations Tests,Approval Tests"
                                    ToolPath = currentDirectory @@ "FakeBuild" @@ "NUnit" @@ "net-2.0"
                                    DisableShadowCopy = true
                                    ShowLabels = false
                                    TimeOut = TimeSpan.FromMinutes 5.
                                    OutputFile = "TestResults.xml"
                                    })
    ReportProgressFinish "Run Unit Tests"
)

let getPackageName (nuspecFileName : string) =
    let postfix = sprintf ".%s.nupkg" (environVar "NugetVersion")
    nuspecFileName.Replace(postfix, "")

let MoveNuget (info:FileInfo) name =
    let directory = sprintf "%s\%s" nugetWebserviceDirectory name
    if (not (Directory.Exists(directory))) then 
        Directory.CreateDirectory(directory) |> ignore
    let file = info.CopyTo(sprintf "%s\%s" directory info.Name, true)
    sprintf "Pushed File: %s to: %s" info.Name directory    
    
FinalTarget "FileCopyNugetsAndArtifacts" (fun _ ->
    ReportProgressStart "Push Nugets"
    !! (outputDirectory @@ "**\*.nupkg")
    |> Seq.map (fun fileName -> new FileInfo(fileName))
    |> Seq.map (fun info -> info, (getPackageName info.Name))
    |> Seq.map (fun (info, name) -> MoveNuget info name)
    |> Log ""

    ReportProgressFinish "Push Nugets"
    PublishArticfact (outputDirectory @@ "Ensconce.*.nupkg")  
    )

FinalTarget "PushNugetsAndArtifacts" (fun _ ->
    ReportProgressStart "Push Nugets"
    !! (outputDirectory @@ "**\*.nupkg")
    |> Seq.map (fun fileName -> new FileInfo(fileName))
    |> Seq.map (fun info -> info, (getPackageName info.Name))
    |> doParallelWithThrottle 4 (fun (info, name) -> 
            let args = sprintf @"push ""%s"" -ApiKey ""%s"" -Source %s" (info.FullName) "ATest" nugetWebService
            let result =
                    ExecProcess (fun info ->
                                    info.FileName <- toolPath
                                    info.Arguments <- args) (TimeSpan.FromMinutes 20.)
            if result <> 0 then failwithf "Error during Nuget push. %s %s" toolPath args 
            sprintf "%s" info.FullName)
    |> Log "Pushing file: "

    ReportProgressFinish "Push Nugets"
    PublishArticfact (outputDirectory @@ "Ensconce.*.nupkg")
    )

Target "Default" DoNothing

"UpdateVersions"
    ==> "BuildSolution"
    ==> "RunUnitTests"
    ==> "CreateNugets"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"

