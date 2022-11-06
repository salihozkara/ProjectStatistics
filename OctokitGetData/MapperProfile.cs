using AutoMapper;
using AutoMapper.Internal;
using Octokit;

namespace OctokitGetData;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // this.Internal().MethodMappingEnabled = false;
        // this.Internal().EnableNullPropagationForQueryMapping = false;
        // CreateMap<AccountType, Shared.AccountType>();
        // CreateMap<User, Shared.Owner>();
        // CreateMap<LicenseMetadata, Shared.License>();
        // CreateMap<License, Shared.License>();
        // CreateMap<Repository, Shared.Repository>();
    }
}