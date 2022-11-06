using JsonMerge;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace OctokitGetData;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(JsonMergeModule)
    )]
public class OctokitGetDataModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
    }
}