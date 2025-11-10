using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
