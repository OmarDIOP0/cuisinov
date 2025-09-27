using AutoMapper;
using CantineBack.Models;
using CantineFront.Models;

namespace CantineFront.Profiles
{
    public class ArticlesProfile : Profile
    {
        public ArticlesProfile()
        {
            CreateMap<Article, ArticleBooking>()
                .ForMember(dest=>dest.Categorie,opt=>opt.MapFrom(src=>src.CategorieNavigation))
                .ForMember(dest=>dest.Image,opt=>opt.MapFrom(src=> Convert.ToBase64String( src.Image!)))
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;
            CreateMap<Categorie, Categorie>()
                 .ForMember(dest => dest.ArticlesNavigation, opt => opt.Ignore());
        }
    }
}
