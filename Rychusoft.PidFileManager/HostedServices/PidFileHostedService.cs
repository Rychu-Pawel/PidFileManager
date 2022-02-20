using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rychusoft.PidFileManager.HostedServices
{
    public class PidFileHostedService : IHostedService
    {
        private readonly ILogger<PidFileHostedService> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly PidFileOptions options;

        public PidFileHostedService(ILogger<PidFileHostedService> logger,
            IHostApplicationLifetime appLifetime,
            IOptions<PidFileOptions> pidFileOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            this.options = pidFileOptions?.Value ?? throw new ArgumentNullException(nameof(pidFileOptions));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!options.IsEnabled || string.IsNullOrWhiteSpace(options.PidFilePath))
                {
                    logger.LogWarning($"Skipping PID file creation due to {nameof(PidFileOptions.IsEnabled)} flag set to false or because of empty {nameof(PidFileOptions.PidFilePath)} property.");
                    return;
                }

                await WritePidFile();

                appLifetime.ApplicationStopped.Register(() => RemovePidFile());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unhandled exception when starting {nameof(PidFileHostedService)}", null);
            }
        }

        private async Task WritePidFile()
        {
#if NETCOREAPP3_1
            var processId = Process.GetCurrentProcess().Id.ToString();
#else
            var processId = Environment.ProcessId.ToString();
#endif
            await File.WriteAllTextAsync(options.PidFilePath, processId);
        }

        private void RemovePidFile()
        {
            try
            {
                File.Delete(options.PidFilePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when deleting PID file", null);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}