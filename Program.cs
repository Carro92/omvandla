using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Den senaste konfigurationen för Azure Functions Worker
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService(); // Lägg till om du använder Application Insights
    })
    .Build();

host.Run();


