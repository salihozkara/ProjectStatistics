using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics.LanguageStatistics;

public class LanguageStatisticsFactory : ISingletonDependency
{
    private readonly ILogger<LanguageStatisticsFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LanguageStatisticsFactory(IServiceProvider serviceProvider, ILogger<LanguageStatisticsFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public ILanguageStatistics GetLanguageStatistics(string language)
    {
        var languageStatistics = _serviceProvider.GetRequiredService<IEnumerable<ILanguageStatistics>>()
            .SingleOrDefault(t =>
                t.GetType().GetCustomAttribute<LanguageAttribute>()?.Languages.Contains(language) ?? false);

        if (languageStatistics == null)
        {
            _logger.LogWarning($"No language statistics found for language {language}");
            return _serviceProvider.GetRequiredService<NullLanguageStatistics>();
        }

        return languageStatistics;
    }

    public string[] GetSupportedLanguages()
    {
        return _serviceProvider.GetRequiredService<IEnumerable<ILanguageStatistics>>()
            .SelectMany(t => t.GetType().GetCustomAttribute<LanguageAttribute>()?.Languages ?? Array.Empty<string>())
            .Distinct()
            .ToArray();
    }
}