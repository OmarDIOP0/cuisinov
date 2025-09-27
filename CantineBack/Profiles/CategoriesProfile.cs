using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;

namespace CantineBack.Profiles
{
    public class CategoriesProfile : Profile
    {
        public CategoriesProfile()
        {
            CreateMap<Categorie, CategorieReadDto>();
              
            ;
        }
    }
}
