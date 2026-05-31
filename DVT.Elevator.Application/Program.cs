using DVT.Elevator.Application.Services;
using DVT.Elevator.Application.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DVT.Elevator.Application;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7083";

                // HTTP Client for API calls
                services.AddHttpClient<IElevatorApiClient, ElevatorApiClient>(client =>
                {
                    client.BaseAddress = new Uri(apiBaseUrl);
                });

                // SignalR real-time service
                services.AddSingleton<ElevatorSignalRService>();

                // Console UI Service
                services.AddHostedService<ElevatorConsoleUI>();
            })
            .Build();

        await host.RunAsync();
    }
}
