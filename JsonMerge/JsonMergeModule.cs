using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace JsonMerge;

[DependsOn(typeof(AbpAutofacModule))]
public class JsonMergeModule : AbpModule
{
    
}