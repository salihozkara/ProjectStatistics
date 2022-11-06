using Microsoft.Extensions.DependencyInjection;
using ProjectStatistics;
using Serilog;
using Serilog.Events;
using Volo.Abp;

Console.OutputEncoding = System.Text.Encoding.UTF8;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "Logs/logs.txt"))
    .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
    .CreateLogger();


using var application = AbpApplicationFactory.Create<ProjectStatisticsModule>(
    options =>
    {
        options.UseAutofac();
        options.Services.AddLogging(c => c.AddSerilog());
    });
application.Initialize();


var cliService = application.ServiceProvider
    .GetRequiredService<CliService>();


Console.CancelKeyPress += (_, eventArgs) =>
{
    try
    {
        var scope = application.ServiceProvider.CreateScope();
        var processHelper = scope.ServiceProvider.GetRequiredService<ProcessHelper>();
        processHelper.AllProcessKill();
        Log.Information("Application is shutting down...");
        eventArgs.Cancel = true;
    }
    catch (Exception e)
    {
        Log.Fatal(e, "Application shutdown failed!");
    }
};

await cliService.RunAsync(args);
application.Shutdown();



