using AutoMapper;
using CantineBack.Models.Dtos;
using CantineBack.Models;

namespace CantineBack.Profiles
{
    public class DepartementsProfile : Profile
    {
        public DepartementsProfile()
        {
            CreateMap<Department, DepartmentReadDto>();

            ;
        }
    }
}
