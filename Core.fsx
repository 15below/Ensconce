#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"
#load "./Packaging.fsx"
#load "./PackagingDeploy.fsx"
#load "./Versioning.fsx"
#load "./Solution.fsx"
#load "./Test.fsx"
#load "./Specflow.fsx"

open System.IO
open Fake
open Utils

let config =
    Map.ofList [
        "build:configuration",          environVarOrDefault "configuration"         "Release"
        "build:solution",               environVar          "solution"
        "core:tools",                   environVar          "tools"
        "packaging:output",             environVarOrDefault "output"                (sprintf "%s\output" (Path.GetFullPath(".")))
        "packaging:deployoutput",       environVarOrDefault "deployoutput"          (sprintf "%s\deploy" (Path.GetFullPath(".")))
        "packaging:outputsubdirs",      environVarOrDefault "outputsubdirs"         "true"
        "packaging:updateid",           environVarOrDefault "updateid"              ""
        "packaging:pushto",             environVarOrDefault "pushto"                ""
        "packaging:pushendpoint",       environVarOrDefault "pushendpoint"          ""
        "packaging:pushurl",            environVarOrDefault "pushurl"               ""
        "packaging:apikey",             environVarOrDefault "apikey"                ""
        "packaging:deploypushto",       environVarOrDefault "deploypushto"          ""
        "packaging:deploypushdir",      environVarOrDefault "deploypushdir"         ""
        "packaging:deploypushurl",      environVarOrDefault "deploypushurl"         ""
        "packaging:deployapikey",       environVarOrDefault "deployapikey"          ""
        "packaging:packages",           environVarOrDefault "packages"              ""
        "versioning:build",             environVarOrDefault "build_number"          "0"
        "versioning:branch",            match environVar "teamcity_build_branch" with
                                        | "<default>" -> environVar "vcsroot_branch"
                                        | _ -> environVar "teamcity_build_branch"
        "vs:version",                   environVarOrDefault "vs_version"            "11.0" ]

// Target definitions
Target "Default"                  <| DoNothing
Target "PackagingDeploy:Package"  <| PackagingDeploy.package config
Target "PackagingDeploy:Push"     <| PackagingDeploy.push config
Target "Solution:Build"           <| Solution.build config
Target "Solution:Clean"           <| Solution.clean config
Target "Versioning:Update"             <| Versioning.update config
Target "Versioning:UpdateDeployNuspec" <| Versioning.updateDeploy config
Target "Test:Run"                      <| Test.run config
Target "SpecFlow:Run"                  <| Specflow.run config

// Build order
"Solution:Clean"
    ==> "Versioning:Update"
    =?> ("Versioning:UpdateDeployNuspec", not isLocalBuild)
    ==> "Solution:Build"
    ==> "PackagingDeploy:Package"
    ==> "SpecFlow:Run"
    ==> "Test:Run"
    =?> ("PackagingDeploy:Push", not isLocalBuild)
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"