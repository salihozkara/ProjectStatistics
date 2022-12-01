using Shared;
using Volo.Abp.Modularity;

namespace CloneAllRepository;

[DependsOn(
    typeof(SharedModule)
)]
public class CloneAllRepositoryModule : AbpModule
{
}