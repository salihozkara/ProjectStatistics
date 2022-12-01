using Shared;
using Volo.Abp.Modularity;

namespace ProjectStatistics;

[DependsOn(
    typeof(SharedModule)
)]
public class ProjectStatisticsModule : AbpModule
{
}