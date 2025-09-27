namespace CantineBack.Models
{
    public class Emplacement
    {
        public Emplacement() 
        { 
           CommandesNavigation = new HashSet<Commande>(); 
        }
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool Actif { get; set; }
        public int EntrepriseId { get; set; }
        public virtual Entreprise? Entreprise{ get; set; }
        public virtual IEnumerable<Commande>? CommandesNavigation { get; set; }
    }
}
