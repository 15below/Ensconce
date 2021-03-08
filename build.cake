var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var baseVersion = "1.7.0";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Versioning")
    .Does(() =>
{
    var subVersion = "";
    var subVersionNumber = "";

    if (BuildSystem.TeamCity.IsRunningOnTeamCity)
    {
        var branchName = BuildSystem.TeamCity.Environment.Build.BranchName;
        var buildNumber = BuildSystem.TeamCity.Environment.Build.Number;

        if(branchName == "master")
        {
            subVersion = $".{buildNumber}";
            subVersionNumber = $".{buildNumber}";
        }
        else
        {
            var versionBranch = branchName.Replace("feature/", "").Replace("pull/","pull-");
            versionBranch = Regex.Replace(preReleaseTag, @"[^0-9A-Za-z-]", "-").ToLower();
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

    Information($"Full version number: {fullVersion}");
    Information($"Package version number: {packageVersion}");

    var files = GetFiles("./src/**/*.*proj");
    foreach(var file in files)
    {
        Information("File: {0}", file);
        XmlPoke(file, "/Project/PropertyGroup/Version", packageVersion);
        XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", fullVersion);
        XmlPoke(file, "/Project/PropertyGroup/FileVersion", fullVersion);
    }
});

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean("./src/Ensconce.sln", new DotNetCoreCleanSettings
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Versioning")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./src/Ensconce.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./src/Ensconce.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetCorePack("./src/Ensconce.sln", new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = "./output/",
    });
});

Task("Default")
    .IsDependentOn("Pack")
    .Does(() => {});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
