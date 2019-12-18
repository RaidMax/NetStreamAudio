using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaidMax.NetStreamAudio.Core;
using RaidMax.NetStreamAudio.Core.Servers;
using RaidMax.NetStreamAudio.Shared;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Capture
{
    class Program
    {
        private static IAudioCapture audioCapture;
        private static CancellationTokenSource cancellationSource;

        public static async Task Main(string[] args)
        {
            ILogger fallbackLogger = null;

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(Utilities.IsDevelopment ? "appsettings.Development.json" : "appsettings.json")
                    .Build();

                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection, configuration);

                using var serviceProvider = serviceCollection.BuildServiceProvider();
                audioCapture = serviceProvider.GetRequiredService<IAudioCapture>();
                cancellationSource = new CancellationTokenSource();
                fallbackLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                Console.CancelKeyPress += OnProcessExit;

                await audioCapture.Start(cancellationSource.Token);
            }

            catch (Exception e)
            {
                if (fallbackLogger != null)
                {
                    fallbackLogger.LogError(e, "Uncaught exception ocurred");
                }

                else
                {
                    Console.WriteLine("Uncaught exception ocurred");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

                if (Utilities.IsDevelopment)
                {
                    throw e;
                }

                if (!Utilities.IsDevelopment)
                {
                    fallbackLogger?.LogInformation("Press any key to exit...");
                    Console.ReadKey();
                }

                OnProcessExit(null, null);
            }

            audioCapture.StopFinished.Wait();

            if (Utilities.IsDevelopment)
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Event callback executed when the process exits
        /// Performs any cleanup logic
        /// </summary>
        /// <param name="sender">source of the event</param>
        /// <param name="e">event arguments</param>
        private static void OnProcessExit(object sender, EventArgs e)
        {
            if (!cancellationSource.IsCancellationRequested)
            {
                AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                Console.CancelKeyPress -= OnProcessExit;

                cancellationSource.Cancel();
                cancellationSource.Dispose();
            }
        }

        /// <summary>
        /// Sets up the dependency injection container
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="config">appsettings configuration</param>
        static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var mainConfigInstance = new NetStreamAudioConfiguration();
            config.Bind(mainConfigInstance);

            services.AddSingleton<IAudioCapture, AudioCapture>()
                .AddSingleton<IAudioServer, UdpAudioServer>()
                .AddSingleton<IDateTimeProvider, DateTimeProvider>()
                .AddSingleton<Func<string, AudioServerConfiguration>>(_serviceProvider => key => mainConfigInstance.ServerTypes[key])
                .AddSingleton(_serviceProvicer => mainConfigInstance)
                .AddLogging(_builder =>
                {
                    _builder.ClearProviders()
                        .AddConsole()
                        .AddFilter((level) => level >= (LogLevel)Enum.Parse(typeof(LogLevel), mainConfigInstance.LogLevel));
                });
        }
    }
}
