using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Shared;

[DependsOn(
    typeof(AbpAutofacModule)
)]
public class SharedModule : AbpModule
{
}