using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.Execution;

namespace CloudBuild
{
    [PublicAPI]
    public class UseUserSecrets : BuildExtensionAttributeBase, IOnBuildCreated
    {
        public void OnBuildCreated(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> executableTargets)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Build>()
                .Build();

            foreach (var config in configuration.GetChildren())
                Environment.SetEnvironmentVariable(config.Key, config.Value);
        }
    }
}