using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Serilog;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;

    [Parameter]
    AbsolutePath TestResultDirectory = RootDirectory + "/.nuke/Artifacts/Test-Results/";

    Target LogInformation => _ => _
        .Executes(() =>
        {
            Log.Information($"Solution path : {Solution}");
            Log.Information($"Solution directory : {Solution.Directory}");
            Log.Information($"Configuration : {Configuration}");
            Log.Information($"TestResultDirectory : {TestResultDirectory}");
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
                .SetConfiguration(Configuration));
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
