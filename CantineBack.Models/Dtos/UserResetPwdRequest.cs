using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class UserResetPwdRequest
    {

        public int Id { get; set; }

        [Required]
        public string Login { get; set; } = null!;

        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;
       
        public string ConfirmPassword { get; set; }
  
    }
}
