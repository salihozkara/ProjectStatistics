using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ProjectStatistics;
using Serilog;
using Serilog.Events;
using Shared.DependencyProcesses;
using Shared.Helpers;
using Volo.Abp;

Console.OutputEncoding = Encoding.UTF8;


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

if (await application.ServiceProvider.DependencyProcessesCheck())
{
    Console.WriteLine("All dependencies are installed");
}
else
{
    Console.WriteLine("Some dependencies are not installed");
    Environment.Exit(1);
}

var cliService = application.ServiceProvider
    .GetRequiredService<CliService>();


Console.CancelKeyPress += (_, eventArgs) =>
{
    try
    {
        CliConsts.IsStop = true;
        ProcessHelper.IsStopRequested = true;
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