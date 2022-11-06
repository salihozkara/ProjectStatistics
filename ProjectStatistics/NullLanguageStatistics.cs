using Microsoft.Extensions.Logging;

namespace ProjectStatistics;

public class NullLanguageStatistics : ILanguageStatistics
{
    private readonly ILogger<NullLanguageStatistics> _logger;

    public NullLanguageStatistics(ILogger<NullLanguageStatistics> logger)
    {
        _logger = logger;
    }

    public Task GetLanguageStatisticsAsync(Repository repository)
    {
        _logger.LogInformation("No language statistics available for {Repository}", repository.Name);
        return Task.CompletedTask;
    }
}