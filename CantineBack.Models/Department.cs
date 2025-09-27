namespace CantineBack.Models
{
    public class Department
    {
        public Department() 
        {
            UsersNavigation = new HashSet<User>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Actif { get; set; }
        public int EntrepriseId { get; set; }
        public virtual Entreprise? Entreprise { get; set; }
        public IEnumerable<User>? UsersNavigation { get; set; }
    }
}
