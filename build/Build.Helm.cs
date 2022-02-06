using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Helm;
using static Nuke.Common.Tools.Helm.HelmTasks;

namespace CloudBuild
{
    public partial class Build
    {
        AbsolutePath ChartsDirectory => RootDirectory / "charts";

        Target ChartBuild => _ => _
            .DependsOn(DockerPush)
            .Executes(() =>
            {
                var di = new DirectoryInfo(ChartsDirectory);
                var charts = di.GetDirectories();

                foreach (var chart in charts)
                    HelmTemplate(settings =>
                        settings
                            .SetProcessWorkingDirectory(di.FullName)
                            .SetChart(chart.Name)
                            .SetNameTemplate(chart.Name)
                            .SetOutputDir($"{OutputDirectory}/{di.Name}")
                    );
            });
    }
}