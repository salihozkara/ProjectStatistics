using System.Text.Json;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class CliService : ISingletonDependency
{
    private readonly LanguageStatisticsFactory _languageStatisticsFactory;
    private ILogger<CliService> Logger { get; }

    public CliService(LanguageStatisticsFactory languageStatisticsFactory, ILogger<CliService> logger)
    {
        _languageStatisticsFactory = languageStatisticsFactory;
        Logger = logger;
    }

    public async Task RunAsync(string[] args)
    {
        var repositories =
            JsonSerializer.Deserialize<Repository[]>(await File.ReadAllTextAsync(CliConsts.RepositoriesPath)) ??
            Array.Empty<Repository>();

        var languages = _languageStatisticsFactory.GetSupportedLanguages();

        repositories = repositories
            .Where(r => languages.Contains(r.Language))
#if DEBUG
            .GroupBy(r => r.Language)
            .SelectMany(g => g.Take(1))
#endif
            .ToArray();


        var tasks = repositories.Select(r => Task.Run(() => ProcessRepository(r)));
        await Task.WhenAll(tasks);
    }

    private Task ProcessRepository(Repository repository)
    {
        var languageStatistics = _languageStatisticsFactory.GetLanguageStatistics(repository.Language);
        return languageStatistics.GetLanguageStatisticsAsync(repository);
    }
}