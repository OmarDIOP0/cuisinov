namespace CantineBack.Models
{
    public class Categorie
    {
        public Categorie() 
        {
            ArticlesNavigation = new HashSet<Article>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Actif { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public IEnumerable<Article>? ArticlesNavigation { get; set; }
    }
}
