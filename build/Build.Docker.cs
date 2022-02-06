using System.IO;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

namespace CloudBuild
{
    public partial class Build
    {
        [Secret]
        [Parameter("Docker access token for pushing image.")]
        readonly string DockerAccessToken;

        [Parameter("Publish main images to repository")]
        readonly string DockerRepositoryNamespace;

        [Parameter("Docker username for pushing image.")]
        readonly string DockerUsername;

        [Parameter("Image Tag")]
        readonly string ImageTag = "dev";

        Target DockerTemplate => _ => _
            .DependsOn(Publish)
            .Executes(() =>
            {
                var template = File.ReadAllText("Dockerfile.template");

                void ProcessDockerTemplate(string projectName)
                {
                    var rendered = template
                        .Replace("{ASPNET_VERSION}", AspNetVersion)
                        .Replace("{APPNAME}", projectName);

                    File.WriteAllText(Path.Combine(OutputDirectory, projectName, "Dockerfile"), rendered);
                }

                foreach (var p in MainProjects) ProcessDockerTemplate(p);
            });

        Target Dockerize => _ => _
            .DependsOn(DockerTemplate)
            .Executes(() =>
            {
                foreach (var p in MainProjects)
                    DockerBuild(s => s
                        .SetPath(".")
                        .SetProcessWorkingDirectory(Path.Combine(OutputDirectory, p))
                        .SetTag($"{p}:{ImageTag}".ToLower()));
            });

        Target DockerLogin => _ => _
            .DependsOn(Dockerize)
            .Requires(() => DockerUsername)
            .Requires(() => DockerAccessToken)
            .Executes(() =>
            {
                Docker($"login -u {DockerUsername} -p {DockerAccessToken}");
            });

        Target DockerPush => _ => _
            .DependsOn(DockerLogin)
            .Requires(() => DockerRepositoryNamespace)
            .Executes(() =>
            {
                foreach (var p in MainProjects)
                {
                    var sourceTag = $"{p.ToLower()}:{ImageTag}";
                    var targetTag = $"{DockerRepositoryNamespace}/{p.ToLower()}:{ImageTag}";

                    DockerTag(s => s
                        .SetSourceImage(sourceTag)
                        .SetTargetImage(targetTag));

                    DockerImagePush(s => s
                        .SetName(targetTag));
                }
            });
    }
}