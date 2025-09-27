using CantineBack.Models;
using AutoMapper;
using CantineBack.Models.Dtos;

namespace CantineBack.Profiles
{
    public class CommandesProfile : Profile
    {
        public CommandesProfile()
        {
            CreateMap<Commande, CommandeReadDto>()
                .ForMember(dest => dest.UserNavigation, opt => opt.MapFrom(src => src.UserNavigation))
                .ForMember(dest => dest.LigneCommandesNavigation, opt => opt.MapFrom(src => src.LigneCommandesNavigation))
                
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;
   
        }
    }
}
