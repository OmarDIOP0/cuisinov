using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;
namespace CantineBack.Profiles
{
    public class EntreprisesProfile : Profile
    {
        public EntreprisesProfile()
        {
            CreateMap<Entreprise, EntrepriseReadDto>();

        }
    }
}
