using AutoMapper;
using BusinessLogic.DTOs;
using DataAccess.Models;

namespace BusinessLogic.Profiles;

public class MetronomeSettingsProfile : Profile
{
    public MetronomeSettingsProfile()
    {
        CreateMap<MetronomeSettings, MetronomeSettingsReadDto>();
        CreateMap<MetronomeSettings, MetronomeSettingsListItemDto>();
        CreateMap<MetronomeSettingsCreateDto, MetronomeSettings>();
        CreateMap<MetronomeSettingsUpdateDto, MetronomeSettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore the Id property (Usually the Id cannot be changed)
    }
}