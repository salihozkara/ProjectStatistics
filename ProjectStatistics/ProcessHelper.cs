using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class ProcessHelper : ISingletonDependency
{
    private List<Process> Processes { get; } = new();
    private ILogger<ProcessHelper> Logger { get; }

    private readonly ConcurrentDictionary<Process, Logger> _processLoggers = new();

    public ProcessHelper(ILogger<ProcessHelper> logger)
    {
        Logger = logger;
    }

    private Logger CreateProcessLogger(int processId)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), $"Logs/Process_{processId}.txt"))
            .CreateLogger();
    }

    public async Task<bool> RunAsync(string fileName, string arguments, string? workingDirectory = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        
        process.Exited += (sender, args) =>
        {
            var p = (Process) sender!;
            Logger.LogInformation($"Process {p.Id} exited with code {p.ExitCode}");
            
            if(_processLoggers.TryGetValue(p, out var logger))
            {
                try
                {
                    logger.Dispose();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error disposing logger");
                }
                _processLoggers.TryRemove(p, out _);
            }
        };

        try
        {
            process.ErrorDataReceived += ErrorDataReceived;
            process.OutputDataReceived += OutputDataReceived;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Processes.Add(process);

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception e)
        {
            Logger.LogException(e);

            return false;
        }
    }

    private void ProcessKill(Process process)
    {
        try
        {
            process.Kill(true);
            Logger.LogWarning($"Process id: {process.Id} was killed.");
        }
        catch (Exception e)
        {
            Logger.LogError("Error while killing process.");
        }
    }

    public void AllProcessKill()
    {
        Logger.LogInformation("Killing all processes.");
        try
        {
            Processes.ForEach(ProcessKill);
            Processes.Clear();
        }
        catch (Exception e)
        {
            Logger.LogError("Error while killing all processes.");
        }
    }

    private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (sender is not Process process)
        {
            return;
        }

        var logger = TryGetOrAddLogger(process);

        logger.Error(e.Data);
    }

    private Logger TryGetOrAddLogger(Process process)
    {
        if (_processLoggers.TryGetValue(process, out var logger)) return logger;
        logger = CreateProcessLogger(process.Id);
        logger.Information("Process started.");
        logger.Information($"arguments: {process.StartInfo.Arguments}");
        _processLoggers.TryAdd(process, logger);

        return logger;
    }

    private void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (sender is not Process process)
        {
            return;
        }

        var logger = TryGetOrAddLogger(process);

        logger.Information(e.Data);
    }
}