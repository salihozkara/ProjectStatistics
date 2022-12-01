using Shared;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace OctokitGetData;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SharedModule)
)]
public class OctokitGetDataModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}