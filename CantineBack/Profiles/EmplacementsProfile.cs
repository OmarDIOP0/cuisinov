using AutoMapper;
using CantineBack.Models.Dtos;
using CantineBack.Models;

namespace CantineBack.Profiles
{
    public class EmplacementsProfile : Profile
    {
        public EmplacementsProfile()
        {
            CreateMap<Emplacement, EmplacementReadDto>();

            ;
        }
    }
}
