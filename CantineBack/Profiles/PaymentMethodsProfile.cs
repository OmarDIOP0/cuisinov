using AutoMapper;
using CantineBack.Models.Dtos;
using CantineBack.Models;

namespace CantineBack.Profiles
{
    public class PaymentMethodsProfile : Profile
    {
        public PaymentMethodsProfile()
        {
            CreateMap<PaymentMethod, PaymentMethodReadDto>();

            ;
        }
    }
}
