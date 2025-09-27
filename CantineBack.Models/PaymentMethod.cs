namespace CantineBack.Models
{
    public class PaymentMethod
    {
        public PaymentMethod() 
        {
            CommandesNavigation = new HashSet<Commande>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Actif { get; set; } 

        public virtual ICollection<Commande>? CommandesNavigation { get; set; }
    }
}
