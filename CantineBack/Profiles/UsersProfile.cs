using AutoMapper;
using CantineBack.Models;
using CantineBack.Models.Dtos;

namespace CantineBack.Profiles
{
    public class UsersProfile : Profile
    {
        public UsersProfile()
        {
            CreateMap<User, UserReadDto>();
               
            ;
          
        }
    }
}
