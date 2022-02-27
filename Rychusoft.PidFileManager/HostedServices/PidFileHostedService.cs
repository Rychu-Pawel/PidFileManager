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
        private readonly PidFileOptions options;

        private bool isPidFileCreated = false;

        public PidFileHostedService(ILogger<PidFileHostedService> logger,
            IOptions<PidFileOptions> pidFileOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = pidFileOptions?.Value ?? throw new ArgumentNullException(nameof(pidFileOptions));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!ValidateOptionsWithLogging())
                    return;

                await WritePidFile();

                isPidFileCreated = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unhandled exception when starting {nameof(PidFileHostedService)}", null);
            }
        }

        private bool ValidateOptionsWithLogging()
        {
            if (options.IsDisabled)
            {
                logger.LogInformation($"Skipping PID file creation due to {nameof(PidFileOptions.IsDisabled)} flag set to true.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(options.PidFilePath))
            {
                logger.LogWarning($"Skipping PID file creation because of empty {nameof(PidFileOptions.PidFilePath)} property. Are {nameof(PidFileOptions)} configured correctly?");
                return false;
            }

            return true;
        }

        private async Task WritePidFile()
        {
#if NETCOREAPP3_1
            var processId = Process.GetCurrentProcess().Id.ToString();
#else
            var processId = Environment.ProcessId.ToString();
#endif
            await File.WriteAllTextAsync(options.PidFilePath, processId);

            logger.LogInformation("PID file created successfully");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                RemovePidFileIfCreated();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when deleting PID file", null);
            }

            return Task.CompletedTask;
        }

        private void RemovePidFileIfCreated()
        {
            if (!isPidFileCreated)
            {
                logger.LogInformation("Skipping PID file removal because it was not created during start");
                return;
            }

            File.Delete(options.PidFilePath);

            logger.LogInformation("PID file deleted successfully");
        }
    }
}