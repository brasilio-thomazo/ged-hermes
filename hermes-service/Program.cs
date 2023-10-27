using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace br.dev.optimus.hermes.service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DotEnv.Load();
            var host = Host.CreateDefaultBuilder(args).ConfigureServices(ConfigurationService).Build();
            host.Run();
        }

        static void ConfigurationService(HostBuilderContext context, IServiceCollection services)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            services.AddSingleton(config);
            services.AddHostedService<App>();
        }

    }
}