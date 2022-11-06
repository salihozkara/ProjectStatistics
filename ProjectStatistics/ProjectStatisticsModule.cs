using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ProjectStatistics;

[DependsOn(
    typeof(AbpAutofacModule)
)]
public class ProjectStatisticsModule : AbpModule
{
    
}