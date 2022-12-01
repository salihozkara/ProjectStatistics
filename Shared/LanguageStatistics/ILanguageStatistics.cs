using Volo.Abp.DependencyInjection;

namespace ProjectStatistics.LanguageStatistics;

public interface ILanguageStatistics : ISingletonDependency
{
    Task GetStatisticsAsync(Repository repository, CancellationToken token = default);
}