using JsonMerge;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace OctokitGetData;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(JsonMergeModule),
    typeof(AbpAutoMapperModule)
    )]
public class OctokitGetDataModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //Use AutoMapper for MyModule
        context.Services.AddAutoMapperObjectMapper<OctokitGetDataModule>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<OctokitGetDataModule>(validate: false);
        });
    }
}