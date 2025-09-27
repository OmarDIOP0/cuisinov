namespace CantineBack.Models
{
    public class LigneCommande
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int CommandeId { get; set; }
        public int Quantite { get; set; }
        public int PrixTotal { get; set; }

        public virtual Article? ArticleNavigation { get; set; }
        public virtual Commande? CommandeNavigation { get; set; }
    }
}
