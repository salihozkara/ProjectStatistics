using Microsoft.Extensions.DependencyInjection;
using OctokitGetData;
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

using var application = AbpApplicationFactory.Create<OctokitGetDataModule>(
    options =>
    {
        options.UseAutofac();
        options.Services.AddLogging(c => c.AddSerilog());
    });
application.Initialize();


var cliService = application.ServiceProvider
    .GetRequiredService<CliService>();

await cliService.RunAsync(args);
application.Shutdown();