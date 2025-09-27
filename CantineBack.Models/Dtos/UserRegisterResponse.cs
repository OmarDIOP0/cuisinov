using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class UserRegisterResponse
    {
        public string? Token { get; set; }
        public DateTime TokenExpireAt { get; set; }
        public string? RefreshToken { get; set; }
        public UserReadDto? User { get; set; }
    }
}
