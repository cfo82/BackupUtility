using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution? Solution;

    [GitVersion]
    readonly GitVersion? GitVersion;

    GitHubActions? GitHubActions => GitHubActions.Instance;

    [Parameter]
    AbsolutePath TestResultDirectory = RootDirectory + "/.nuke/Artifacts/Test-Results/";

    Target LogInformation => _ => _
        .Executes(() =>
        {
            Assert.NotNull(Solution);
            Assert.NotNull(GitVersion);

            Log.Information($"Solution path : {Solution}");
            Log.Information($"Solution directory : {Solution?.Directory}");
            Log.Information($"Configuration : {Configuration}");
            Log.Information($"TestResultDirectory : {TestResultDirectory}");

            Log.Information($"Version:");
            Log.Information($"  Major:                {GitVersion?.Major}");
            Log.Information($"  Minor:                {GitVersion?.Minor}");
            Log.Information($"  Patch:                {GitVersion?.Patch}");
            Log.Information($"  Version:              {GitVersion?.MajorMinorPatch}");
            Log.Information($"  SemVer:               {GitVersion?.SemVer}");
            Log.Information($"  Full SemVer:          {GitVersion?.FullSemVer}");
            Log.Information($"  Assembly SemVer:      {GitVersion?.AssemblySemVer}");
            Log.Information($"  Assembly File SemVer: {GitVersion?.AssemblySemFileVer}");
            Log.Information($"  Legacy SemVer Padded: {GitVersion?.LegacySemVerPadded}");
            Log.Information($"  Nuget Version:        {GitVersion?.NuGetVersionV2}");
            Log.Information($"  Custom Nuget Version: {GitVersion?.FullSemVer}");

            if (GitHubActions != null)
            {
                Log.Information($"Github Actions");
                Log.Information($" Run: {GitHubActions.RunId}");
            }
        });

    Target Clean => _ => _
        .DependsOn(LogInformation)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean();
            TestResultDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(_ => _
                .SetNoRestore(true)
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion?.AssemblySemVer)
                .SetAssemblyVersion(GitVersion?.AssemblySemVer));
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(_ => _
                .SetConfiguration(Configuration)
                .SetNoRestore(true)
                .SetNoBuild(true)
                .SetResultsDirectory(TestResultDirectory)
                .EnableCollectCoverage());
        });
}
