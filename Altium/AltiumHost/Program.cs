using System;
using System.Numerics;
using System.Threading.Tasks;
using AltiumHost.Generator;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AltiumHost
{
    internal class Program
    {
        private static readonly string Txtprocessorwriterthreshold = "txtprocessorwriterthreshold";
        private static readonly string Maxdifferentwords = "maxdifferentwords";
        private static readonly string Targetfilename = "targetfilename";
        private static readonly string GenerateCommand = "generate";

        private static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false);
            var configuration = builder.Build();

            var sourceFileName = configuration.GetValue<string>(Targetfilename);

            Log.Debug("File Generator\\Sorting Utility.");

            try
            {
                if (args.Length != 2) return 1;

                using (var container = BuildContainer(configuration))
                {
                    if (args[0] == GenerateCommand)
                    {
                        if (BigInteger.TryParse(args[1], out var fileSize) == false) return 1;

                        Log.Debug($"The new file will be generated with size about {fileSize / 1024 / 1024} megabytes");

                        await container.Resolve<IFileGenerator>().GenerateAsync(sourceFileName, fileSize);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return 1;
            }
        }

        public static IContainer BuildContainer(IConfigurationRoot configuration)
        {
            var txtprocessorwriterthreshold = configuration.GetValue<int>(Txtprocessorwriterthreshold);
            var maxdifferentwords = configuration.GetValue<int>(Maxdifferentwords);

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<MagicFileGenerator>()
                .As<IFileGenerator>().WithParameter("maxDifferentWords", maxdifferentwords)
                .WithParameter("txtprocessorwriterthreshold", txtprocessorwriterthreshold).SingleInstance();

            return containerBuilder.Build();
        }
    }
}