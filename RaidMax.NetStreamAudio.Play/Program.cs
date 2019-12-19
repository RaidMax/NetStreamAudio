using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaidMax.NetStreamAudio.Core;
using RaidMax.NetStreamAudio.Core.Players;
using RaidMax.NetStreamAudio.Shared;
using RaidMax.NetStreamAudio.Shared.Configuration;
using RaidMax.NetStreamAudio.Shared.Enumerations;
using RaidMax.NetStreamAudio.Shared.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaidMax.NetStreamAudio.Play
{
    class Program
    {
        private static IAudioPlayer audioPlayer;
        private static CancellationTokenSource cancellationSource;

        public static async Task Main(string[] args)
        {
            ILogger fallbackLogger = null;
            IStopResult stopResult = null;

            try
            {
                var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(Utilities.IsDevelopment ? "appsettings.Development.json" : "appsettings.json")
                        .Build();

                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection, configuration);

                using var serviceProvider = serviceCollection.BuildServiceProvider();
                cancellationSource = new CancellationTokenSource();
                audioPlayer = serviceProvider.GetRequiredService<IAudioPlayer>();
                fallbackLogger = serviceProvider.GetRequiredService<ILogger<Program>>();

                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                Console.CancelKeyPress += OnProcessExit;

                stopResult = await audioPlayer.Start(cancellationSource.Token);
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

                if (stopResult != null)
                {
                    stopResult.ResultType = StopResultType.Unexpected;
                }

                OnProcessExit(null, null);
            }

            finally
            {
                if ((stopResult == null || stopResult.ResultType == StopResultType.Unexpected) && !Utilities.IsDevelopment)
                {
                    fallbackLogger?.LogInformation("Press any key to exit...");
                    Console.ReadKey();
                }

                if (stopResult != null)
                {
                    audioPlayer.StopFinished.Wait();
                }

                cancellationSource.Dispose();
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
            if (!audioPlayer.StopFinished.IsSet)
            {
                cancellationSource.Cancel();
            }
        }

        /// <summary>
        /// Sets up the dependency injection container
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="config">appsettings configuration</param>
        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            var mainConfigInstance = new NetStreamAudioConfiguration();
            config.Bind(mainConfigInstance);

            services.AddSingleton<IAudioClient, UdpAudioClient>()
                .AddSingleton<IAudioPlayer, AudioPlayer>()
                .AddSingleton<Func<string, AudioClientConfiguration>>(_serviceProvider => key => mainConfigInstance.ClientTypes[key])
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
