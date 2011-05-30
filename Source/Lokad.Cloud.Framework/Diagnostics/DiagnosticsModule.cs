﻿#region Copyright (c) Lokad 2009-2011
// This code is released under the terms of the new BSD licence.
// URL: http://www.lokad.com/
#endregion

using Autofac;
using Lokad.Cloud.Diagnostics.Persistence;
using Lokad.Cloud.Storage;
using Lokad.Cloud.Storage.Shared.Logging;
using Microsoft.WindowsAzure;

namespace Lokad.Cloud.Diagnostics
{
    /// <summary>Cloud Diagnostics IoC Module.</summary>
    public class DiagnosticsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CloudLogger).As<ILog>();
            builder.Register(CloudLogProvider).As<ILogProvider>();

            // Cloud Monitoring
            builder.RegisterType<BlobDiagnosticsRepository>().As<ICloudDiagnosticsRepository>().PreserveExistingDefaults();
            builder.RegisterType<ServiceMonitor>().As<IServiceMonitor>();
            builder.RegisterType<DiagnosticsAcquisition>()
                .PropertiesAutowired(true)
                .InstancePerDependency();

            // TODO (ruegg, 2011-05-30): Observer that logs system events to the log: temporary! to keep old logging behavior for now
            builder.RegisterType<CloudStorageLogger>().As<IStartable>().SingleInstance();
        }

        static CloudLogger CloudLogger(IComponentContext c)
        {
            return new CloudLogger(BlobStorageForDiagnostics(c), string.Empty);
        }

        static CloudLogProvider CloudLogProvider(IComponentContext c)
        {
            // TODO (ruegg, 2011-05-26): Looks like legacy code, verify and remove
            return new CloudLogProvider(BlobStorageForDiagnostics(c));
        }

        static IBlobStorageProvider BlobStorageForDiagnostics(IComponentContext c)
        {
            // Neither log nor observers are provided since the providers
            // used for logging obviously can't log themselves (cyclic dependency)

            // We also always use the CloudFormatter, so this is equivalent
            // to the RuntimeProvider, for the same reasons.

            return CloudStorage
                .ForAzureAccount(c.Resolve<CloudStorageAccount>())
                .WithDataSerializer(new CloudFormatter())
                .BuildBlobStorage();
        }
    }
}
