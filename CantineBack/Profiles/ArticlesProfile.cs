using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;

namespace CantineBack.Profiles
{
    public class ArticlesProfile : Profile
    {
        public ArticlesProfile()
        {
            CreateMap<Article, ArticleCommandReadDto>()
              
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => String.Empty))
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;
            CreateMap<Article, ArticleReadDto>()

        
           .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Article, ArticleImageDto>()


.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;
            CreateMap<Article, Article>()
                .ForMember(dest => dest.CategorieNavigation, opt => opt.MapFrom(src => src.CategorieNavigation))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => String.Empty))
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;

        }
    }
}
