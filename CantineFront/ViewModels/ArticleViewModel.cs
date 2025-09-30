using CantineBack.Models;

namespace CantineFront.ViewModels
{
    public class ArticleViewModel
    {
        public List<Categorie>? Categories {get;set;}
        public Article Article {get;set;}
        public string ImageBase64 { get;set;}

        public List<Entreprise>? Entreprises { get; set; }

    }
}
