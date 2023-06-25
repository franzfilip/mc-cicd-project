using ConsumerSimulator;
using ConsumerSimulator.GraphQLClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureAppConfiguration((hostingContext, config) => {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Console.WriteLine("CURRENTLY IN THE ENVINROMENT " + env.EnvironmentName);
        });

        builder.ConfigureServices((context, services) => {
            IConfiguration configuration = context.Configuration;
            string graphqlApiUrl = configuration.GetValue<string>("GRAPHQL_API_URL");
            string httpGraphQlApiUrl = $"http://{graphqlApiUrl}";
            string webSocketsGraphQLApiUrl = $"ws://{graphqlApiUrl}";

            services.AddConsumerSimulator()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(httpGraphQlApiUrl))
                .ConfigureWebSocketClient(c => c.Uri = new Uri(webSocketsGraphQLApiUrl));

            services.AddHostedService<Startup>();
        });

        await builder.RunConsoleAsync();
    }

    public class Startup : IHostedService
    {
        private readonly IConsumerSimulator _client;
        public Startup(IConsumerSimulator client)
        {
            _client = client;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Simulator simulator = new Simulator(_client);

            await simulator.StartSimulationAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
