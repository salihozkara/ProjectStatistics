using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProjectStatistics.Helpers;
using ProjectStatistics.LanguageStatistics;
using Shared;
using Sharprompt;
using Sharprompt.Fluent;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class CliService : ISingletonDependency
{
    private readonly ConcurrentBag<Repository> _errorRepositories = new();
    private readonly LanguageStatisticsFactory _languageStatisticsFactory;

    public CliService(LanguageStatisticsFactory languageStatisticsFactory, ILogger<CliService> logger)
    {
        _languageStatisticsFactory = languageStatisticsFactory;
        Logger = logger;
    }

    private ILogger<CliService> Logger { get; }

    public async Task RunAsync(string[] args)
    {
        Args arg;
        if(args.Length == 0)
        {
            arg = new Args
            {
                ComputerCount = Prompt.Input<int>("Enter computer count")
            };
            if (arg.ComputerCount > 0)
            {
                arg.ComputerIndex = Prompt.Select<int>(o=>o
                    .WithMessage("Select computer index")
                    .WithItems(Enumerable.Range(0, arg.ComputerCount).ToArray())
                    .WithDefaultValue(0)
                );
            }else
            {
                arg = new Args();
            }
        }else{
            arg = ArgsHelper.Parse(args);
        }

        var languages = _languageStatisticsFactory.GetSupportedLanguages();
        

        var repositories = Resources.RepositoriesJson
            .Where(r => languages.Contains(r.Language))
            .OrderByDescending(x => x.Size)
            .Where((r,i) => i % arg.ComputerCount == arg.ComputerIndex)
            .Where(r=>!File.Exists(Path.Combine(r.Language,"Reports", r.Name + ".xml")))
            .ToArray();
        

        var count = repositories.Length;
        var totalSize = repositories.Sum(x => x.Size);
        var totalSizeString = ToSizeString(totalSize);
        
        Logger.LogInformation($"Total repositories: {count}");
        Logger.LogInformation($"Total size: {totalSizeString}");
        
        var tasks = repositories.Select(r => ProcessRepository(r)).OrderBy(_ => Guid.NewGuid()).ToArray();

        await RunTask(tasks);

        var tryCount = 0;
        while (_errorRepositories.Any() && tryCount < 3)
        {
            tryCount++;
            Logger.LogInformation($"Try {tryCount} to process {_errorRepositories.Count} repositories");
            tasks = _errorRepositories.Select(r => ProcessRepository(r)).ToArray();
            _errorRepositories.Clear();
            await RunTask(tasks);
        }

        Logger.LogInformation("Done");

        if (_errorRepositories.Any()) Logger.LogError($"Failed to process {_errorRepositories.Count} repositories");
        
        _errorRepositories.ToJsonFile();
        
    }
    private string ToSizeString(long totalSize)
    {
        var size = totalSize;
        var sizeString = "KB";
        if (size > 1024)
        {
            size /= 1024;
            sizeString = "MB";
        }

        if (size > 1024)
        {
            size /= 1024;
            sizeString = "GB";
        }

        if (size > 1024)
        {
            size /= 1024;
            sizeString = "TB";
        }

        return $"{size} {sizeString}";
    }
    private Task ProcessRepository(Repository repository, CancellationToken token = default)
    {
        var languageStatistics = _languageStatisticsFactory.GetLanguageStatistics(repository.Language);
        var task = Task.Run(async () =>
        {
            try
            {
                if (CliConsts.IsStop)
                {
                    return;
                }
                await languageStatistics.GetStatisticsAsync(repository, token);
                try
                {
                    await DeleteDirectory(Path.Combine(repository.Language, "Repositories", repository.Name));
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Failed to delete {repository.Name}");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while processing {Repository}", repository);
                _errorRepositories.Add(repository);
            }
        }, token);

        return task;
    }

    private static Task DeleteDirectory(string path)
    {
        if (!Directory.Exists(path)) return Task.CompletedTask;

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        var directories = Directory.GetDirectories(path);
        foreach (var directory in directories) DeleteDirectory(directory);

        Directory.Delete(path, false);

        return Task.CompletedTask;
    }

    public Task RunTask(IEnumerable<Task> tasks)
    {
        var taskGrouping = tasks.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / CliConsts.MaxConcurrentThreads)
            .Select(x => x.Select(v => v.Value));
        return RunTask(taskGrouping);
    }
    
    public async Task RunTask(IEnumerable<IEnumerable<Task>> taskList)
    {
        using var concurrencySemaphore = new SemaphoreSlim(CliConsts.MaxConcurrentThreads);
        foreach (var tasks in taskList)
        {
            try
            {
                await concurrencySemaphore.WaitAsync();
                using var concurrencySemaphore2 = new SemaphoreSlim(CliConsts.MaxConcurrentThreads);
                var tasksToRun = tasks.Select(async task =>
                {
                    await concurrencySemaphore2.WaitAsync();
                    try
                    {
                        await task;
                    }
                    finally
                    {
                        concurrencySemaphore2.Release();
                    }
                }).ToList();
                await Task.WhenAll(tasksToRun);
                Logger.LogInformation(CliConsts.MaxConcurrentThreads + " tasks completed");
            }
            finally
            { 
                concurrencySemaphore.Release();
            }
        }
    }
}