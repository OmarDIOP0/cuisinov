using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class PostCommandDto
    {
        public int? UserId { get; set; }
        public bool CommandeADistance { get; set; }
        public string? Comment { get; set; }
        public int EmplacementId { get; set; }
        public int PaymentMethodId { get; set; }

        public IEnumerable<PostLigneCommandDto>? LigneCommands { get; set;}
    }
}
