#module nuget:?package=Cake.BuildSystems.Module&version=5.0.0
#tool "nuget:?package=OctopusTools&version=9.1.7"

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var baseVersion = "1.22.1";
var subVersion = "";
var subVersionNumber = "";
var isMasterOrDevelop = false;
var isLocal = BuildSystem.IsLocalBuild;

if (BuildSystem.TeamCity.IsRunningOnTeamCity)
{
    var branchName = BuildSystem.TeamCity.Environment.Build.BranchName;
    var buildNumber = BuildSystem.TeamCity.Environment.Build.Number;

    if(branchName == "master")
    {
        subVersion = $".{buildNumber}";
        subVersionNumber = $".{buildNumber}";
        isMasterOrDevelop = true;
    }
    else if(branchName == "develop")
    {
        subVersion = $"-develop-{buildNumber}";
        subVersionNumber = $".{buildNumber}";
        isMasterOrDevelop = true;
    }
    else
    {
        var versionBranch = branchName.Replace("feature/", "").Replace("pull/","pull-").ToLower();
        versionBranch = Regex.Replace(versionBranch, @"[^0-9a-z-]", "-");
        subVersion = $"-{versionBranch}-{buildNumber}";
        subVersionNumber = $".{buildNumber}";
    }
}
else
{
    Information("Not running on TeamCity");
    baseVersion = "0.0.0";
    subVersion = "-local-0";
    subVersionNumber = ".0";
}

var fullVersion = $"{baseVersion}{subVersionNumber}";
var packageVersion = $"{baseVersion}{subVersion}";


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Versioning")
    .Does(() =>
{
    Information($"Full version number: {fullVersion}");
    Information($"Package version number: {packageVersion}");

    var files = GetFiles("./src/**/*.*proj");
    foreach(var file in files)
    {
        Information($"Updating version in {file.FullPath}");
        XmlPoke(file, "/Project/PropertyGroup/Version", packageVersion);
        XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", fullVersion);
        XmlPoke(file, "/Project/PropertyGroup/FileVersion", fullVersion);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./output");

    DotNetClean("./src/Ensconce.sln", new DotNetCleanSettings
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Versioning")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./src/Ensconce.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./src/Ensconce.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Pack-Binary")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPack("./src/Ensconce.sln", new DotNetPackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = "./output/binaries",
    });
});

Task("Push-Binary-Internal")
    .WithCriteria(!isLocal)
    .IsDependentOn("Pack-Binary")
    .Does(() =>
{
    var apiKey = BuildSystem.TeamCity.Environment.Build.ConfigProperties["nuget.apiKey.binaries"];
    var url = BuildSystem.TeamCity.Environment.Build.ConfigProperties["nuget.url.binaries"];
    var endpoint = BuildSystem.TeamCity.Environment.Build.ConfigProperties["nuget.endpoint.binaries"];

    var files = GetFiles("./output/binaries/*.nupkg");
    foreach(var file in files)
    {
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            ApiKey = apiKey,
            Source = $"{url}{endpoint}",
            SymbolApiKey = apiKey,
            SymbolSource = $"{url}{endpoint}",
            NoServiceEndpoint = true,
            SkipDuplicate = true,
            Timeout = 600
        });
    }
});

Task("Push-Binary-Public")
    .WithCriteria(!isLocal)
    .WithCriteria(isMasterOrDevelop)
    .IsDependentOn("Pack-Binary")
    .Does(() =>
{
    var apiKey = BuildSystem.TeamCity.Environment.Build.ConfigProperties["nuget.apiKey.nugetorg"];

    var files = GetFiles("./output/binaries/*.nupkg");
    foreach(var file in files)
    {
        DotNetNuGetPush(file.FullPath, new DotNetNuGetPushSettings
        {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = apiKey,
            SkipDuplicate = true,
            Timeout = 600
        });
    }
});

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
{
    CreateDirectory("./output/publish");
    CreateDirectory("./output/publish/Content");
    CreateDirectory("./output/publish/Content/Tools");
    CreateDirectory("./output/publish/Content/Tools/Ensconce");

    CopyFiles("./src/Scripts/*.ps1", "./output/publish/Content");
    CopyFiles("./src/Deploy/*.ps1", "./output/publish");
    CopyFiles("./src/Deploy/*.xml", "./output/publish");


    DotNetPublish("./src/Ensconce.Console/Ensconce.Console.csproj", new DotNetPublishSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = "./output/publish/Content/Tools/Ensconce",
    });
});

Task("Pack-Deploy")
    .IsDependentOn("Publish")
    .Does(() =>
{
    CreateDirectory("./output/deployment");

    OctoPack("Ensconce", new OctopusPackSettings
    {
        Format = OctopusPackFormat.Zip,
        OutFolder = "./output/deployment",
        Title = "Ensconce",
        Version = packageVersion,
        BasePath = "./output/publish"
    });
});

Task("Push-Deploy")
    .WithCriteria(!isLocal)
    .IsDependentOn("Pack-Deploy")
    .Does(() =>
{
    var serverUrl = BuildSystem.TeamCity.Environment.Build.ConfigProperties["octopus.url"];
    var apiKey = BuildSystem.TeamCity.Environment.Build.ConfigProperties["octopus.apikey"];

    var files = GetFiles("./output/deployment/*.zip");
    foreach(var file in files)
    {
        OctoPush(serverUrl, apiKey, file.FullPath, new OctopusPushSettings
        {
            ReplaceExisting = false,
        });
    }
});

Task("Default")
    .IsDependentOn("Push-Binary-Internal")
    .IsDependentOn("Push-Binary-Public")
    .IsDependentOn("Push-Deploy")
    .Does(() => {});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
