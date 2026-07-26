using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Models;
using DataAccess.Models;
using DataAccess;
namespace BusinessLogic.Profiles;
public class MetronomeCollectProfile : Profile
{
    public MetronomeCollectProfile()
    {
        CreateMap<MetronomeCollectionName, CollectionNameReadDto>();
        CreateMap<CollectionNameCreateDto, MetronomeCollectionName>();
        CreateMap<CollectionNameUpdateDto, MetronomeCollectionName>();

        CreateMap<IsAuthenticateDto, AuthenticateResponse>();
    }
}