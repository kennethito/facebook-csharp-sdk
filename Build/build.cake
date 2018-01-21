var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
var version = EnvironmentVariable("version") ?? "0.0.1";
var build = EnvironmentVariable("build") ?? "1";

var fullVersion = $"{version}.{build}";
var semVersion = EnvironmentVariable("tag") ?? $"{version}-beta{build}";

Information($"");
Information($"============================");
Information($"Version {version}");
Information($"Build {build}");
Information($"SemVersion {semVersion}");
Information($"============================");

Task("Clean")
    .Does(() => 
    {
        DotNetCoreClean("../Source/Facebook.sln");
        CleanDirectory("../artifacts");
    });

Task("Restore")
    .Does(() => 
    {
        DotNetCoreRestore("../Source/Facebook.sln");
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration
        };

        DotNetCoreBuild("../Source/Facebook.sln", settings);
    });

Task("Test")
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            NoBuild = true
        };

        var projectFiles = GetFiles("../Source/**/*.Tests.csproj");
        foreach(var file in projectFiles)
        {
            Information($"Testing {file}");
            DotNetCoreTest(file.FullPath, settings);
        }
    });

Task("Pack")
    .Does(() =>
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings();
        msBuildSettings.SetVersion(semVersion);

        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = "../artifacts",
            MSBuildSettings = msBuildSettings,
            NoBuild = true
        };

        DotNetCorePack("../Source/Facebook/Facebook.csproj", settings);
    });    

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        // Get the path to the package.
        var package = $"../artifacts/Facebook-NetStandard.{semVersion}.nupkg";

        // Push the package.
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = nugetApiKey
        });
    });    

RunTarget(target);