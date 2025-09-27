namespace CantineBack.Models
{
    public class Article
    {
        public Article() 
        {
            LigneCommandesNavigation = new HashSet<LigneCommande>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PrixDeVente { get; set; }
        public int? PrixDachat { get; set; }
        public int QuantiteStock { get; set; }
        public byte[]? Image { get; set; }
        public int CategorieId { get; set; }
        public bool IsArticleOnMenu { get; set; }
        public bool Actif { get; set; }
        public bool IsApproved { get; set; }

        public bool ControlStockQuantity { get; set; } = true;
        public int EntrepriseId { get; set; }
        public virtual Entreprise? Entreprise { get; set; }


        public virtual ICollection<LigneCommande>? LigneCommandesNavigation { get; set; }
        public virtual Categorie? CategorieNavigation { get; set; }

    }
}
