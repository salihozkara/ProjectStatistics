using Volo.Abp.DependencyInjection;

namespace Shared.LanguageStatistics;

public interface ILanguageStatistics : ISingletonDependency
{
    Task GetStatisticsAsync(Repository repository, CancellationToken token = default);
}