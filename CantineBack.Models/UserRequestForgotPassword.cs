using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models
{
    public class UserRequestForgotPassword
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;

        public string ConfirmPassword { get; set; }

    }
}
