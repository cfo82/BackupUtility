using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.BuildInstaller);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution? Solution;

    [GitVersion]
    readonly GitVersion? GitVersion;

    GitHubActions? GitHubActions => GitHubActions.Instance;

    Project? InstallerProject => Solution?.Projects.FirstOrDefault(p => p.Name == "BackupUtility.Installer");

    AbsolutePath BuildDirectory => RootDirectory / "Build";

    [Parameter]
    AbsolutePath TestResultDirectory = RootDirectory + "/.nuke/Artifacts/Test-Results/";

    [Parameter]
    AbsolutePath PublishDirectory = RootDirectory + "/.nuke/Artifacts/Publish/";

    [Parameter]
    AbsolutePath InstallerDirectory = RootDirectory + "/.nuke/Artifacts/Installer/";

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
            InstallerDirectory.CreateOrCleanDirectory();
            TestResultDirectory.CreateOrCleanDirectory();
            PublishDirectory.CreateOrCleanDirectory();

            Log.Information($"Build dir {BuildDirectory}");

            RootDirectory.GlobDirectories("**/bin")
                .Where(d => !d.ToString().StartsWith(BuildDirectory))
                .ForEach(d =>
                {
                    Log.Information($"Delete directory {d}");
                    d.DeleteDirectory();
                });
            RootDirectory.GlobDirectories("**/obj")
                .Where(d => !d.ToString().StartsWith(BuildDirectory))
                .ForEach(d =>
                {
                    Log.Information($"Delete directory {d}");
                    d.DeleteDirectory();
                });

            DotNetTasks.DotNetClean(_ => _
                .SetProject(Solution));
            DotNetTasks.DotNetClean(_ => _
                .SetProject(InstallerProject));
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(Solution)
                .SetRuntime("win-x64"));
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

    Target Publish => _ => _
        .DependsOn(Test)
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            var wpfProject = Solution?.Projects.FirstOrDefault(p => p.Name == "BackupUtility.Wpf");
            Assert.NotNull(wpfProject);

            DotNetTasks.DotNetPublish(_ => _
                .SetProject(wpfProject)
                .SetNoRestore(true)
                .SetNoBuild(true)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory)
                .SetSelfContained(false)
                .SetRuntime("win-x64")
                .SetVersion(GitVersion?.AssemblySemVer)
                .SetAssemblyVersion(GitVersion?.AssemblySemVer)
                .SetPublishTrimmed(false)
                .SetPublishReadyToRun(false));
        });

    Target BuildInstaller => _ => _
        .DependsOn(Publish)
        .Produces(RootDirectory / "BackupUtility.Installer" / "bin" / "x64" / "Release")
        .OnlyWhenDynamic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            Assert.NotNull(Solution);
            Assert.NotNull(InstallerProject);

            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(InstallerProject)
                .SetRuntime("win-x64"));

            DotNetTasks.DotNetBuild(_ => _
                .SetNoRestore(true)
                .SetProjectFile(InstallerProject)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion?.AssemblySemVer)
                .SetAssemblyVersion(GitVersion?.AssemblySemVer)
                .SetProperty("SolutionDir", Solution?.Path.Parent));

            var installerBinDir = RootDirectory / "BackupUtility.Installer" / "bin" / Configuration / "en-US";

            Log.Information($"Copy installer files from {installerBinDir} to {InstallerDirectory}");

            System.IO.Directory.GetFiles(installerBinDir)
                .ForEach(f => System.IO.File.Copy(f, System.IO.Path.Combine(InstallerDirectory, System.IO.Path.GetFileName(f))));
        });
}
