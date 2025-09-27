using CantineBack.Models;

namespace CantineFront.Models
{
    public class ArticleBooking
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PrixDeVente { get; set; }
        public int PrixDachat { get; set; }
        public int QuantiteStock { get; set; }
        public string Image { get; set; }
        public int CategorieId { get; set; }
        public bool IsArticleOnMenu { get; set; }


        /*Par défaut est Nombre de fois que l'article a été selectionné */
        public int Quantite { get; set; }
        public  Categorie? Categorie { get; set; }
    }
}
