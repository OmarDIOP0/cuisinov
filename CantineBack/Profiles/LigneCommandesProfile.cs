using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;

namespace CantineBack.Profiles
{
    public class LigneCommandesProfile : Profile
    {
        public LigneCommandesProfile()
        {
            CreateMap<LigneCommande, LigneCommandeReadDto>()
                .ForMember(dest => dest.ArticleNavigation, opt => opt.MapFrom(src => src.ArticleNavigation))
               
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;

        }
    }
}
