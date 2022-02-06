using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable UnusedMember.Local

namespace CloudBuild
{
    [CheckBuildProjectConfigurations]
    [ShutdownDotNetAfterServerBuild]
    [UseUserSecrets(Priority = 300)]
    partial class Build : NukeBuild
    {
        [Parameter("ASPNET Version")]
        readonly string AspNetVersion = "6.0";

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        // [GitRepository] readonly GitRepository GitRepository;
        [GitVersion]
        readonly GitVersion GitVersion;

        readonly IEnumerable<string> MainProjects = new List<string> { "Silo", "MultiTenantApplication" }.AsReadOnly();

        [Solution]
        readonly Solution Solution;

        AbsolutePath SourceDirectory => RootDirectory / "src";
        AbsolutePath TestsDirectory => RootDirectory / "tests";
        AbsolutePath OutputDirectory => RootDirectory / "output";

        Target Clean => _ => _
            .Before(Restore)
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(OutputDirectory);
            });

        Target Restore => _ => _
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution));
            });

        Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .EnableNoRestore());
            });

        Target Test => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                DotNetTest(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableNoBuild());
            });

        Target Publish => _ => _
            .DependsOn(Test)
            .Executes(() =>
            {
                void Publish(string projectName)
                {
                    var siloProject = Solution.GetProject(projectName)!;

                    DotNetPublish(s => s
                        .SetProject(siloProject.Path)
                        .SetConfiguration(Configuration)
                        .SetOutput(Path.Combine(OutputDirectory, projectName))
                        .EnableNoRestore()
                        .EnableNoBuild());
                }

                foreach (var p in MainProjects) Publish(p);
            });

        /// Support plugins are available for:
        /// - JetBrains ReSharper        https://nuke.build/resharper
        /// - JetBrains Rider            https://nuke.build/rider
        /// - Microsoft VisualStudio     https://nuke.build/visualstudio
        /// - Microsoft VSCode           https://nuke.build/vscode
        public static int Main() => Execute<Build>(x => x.Compile);
    }
}