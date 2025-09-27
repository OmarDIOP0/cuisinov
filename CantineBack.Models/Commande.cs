using System.ComponentModel.DataAnnotations;

namespace CantineBack.Models
{
    public class Commande
    {
        public Commande() 
        {
            LigneCommandesNavigation = new HashSet<LigneCommande>();
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

        [MaxLength(500)]
        public string? CustomerFeedback { get; set; }
        public bool IsRating { get; set; }
        public int? Rate { get; set; }

        [MaxLength(500)]
        public string? RejectComment { get; set; }

        public virtual User? UserNavigation { get; set; }
        public virtual Emplacement? EmplacementNavigation { get; set; }
        public virtual PaymentMethod? PaymentMethodNavigation { get; set; }
        public virtual ICollection<LigneCommande>? LigneCommandesNavigation { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
