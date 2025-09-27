using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class CommandeReadDto
    {
        public CommandeReadDto()
        {
            LigneCommandesNavigation = new HashSet<LigneCommandeReadDto>();
        }
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public int? UserId { get; set; }
        public int Montant { get; set; }
        public bool IsDelivered { get; set; }
        public bool CommandeADistance { get; set; }
        public int EmplacementId { get; set; }
        public int PaymentMethodId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRejected { get; set; }

        public string RejectComment { get; set; }

        public string CustomerFeedback { get; set; }
        public string? Comment { get; set; }
        public bool IsRating { get; set; }
        public int? Rate { get; set; }
        public  UserReadDto? UserNavigation { get; set; }
        public  EmplacementReadDto? EmplacementNavigation { get; set; }
        public  PaymentMethodReadDto? PaymentMethodNavigation { get; set; }
        public  ICollection<LigneCommandeReadDto>? LigneCommandesNavigation { get; set; }
    }
}
