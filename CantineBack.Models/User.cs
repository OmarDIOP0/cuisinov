namespace CantineBack.Models
{
    public class User
    {
        public User()
        {
            CommandeNavigation = new HashSet<Commande>();
        }
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Matricule { get; set; }
        public string? Login { get; set; }

        public string? Email { get; set; }
        public bool? ResetPassword { get; set; }
        public string? Password { get; set; }
        public string? Telephone { get; set; }
        public string? QrCode { get; set; }
        public string? Guid { get; set; }
        public int Solde { get; set; }
        public string? Profile { get; set; }
        public int? DepartmentId { get; set; }
        public bool Actif { get; set; }
        public bool? IsInterimEmployee { get; set; }
        public DateTime? LastRechargeDate { get; set; }
        public int? LastRechargeAmount { get; set; }
        public int? EntrepriseId { get; set; }
        public string? Bureau { get; set; }
        public string? EntrepriseCode { get; set; }
        public virtual Entreprise? Entreprise { get; set; }
        public virtual Department? Department { get; set; }
        public virtual IEnumerable<Commande>? CommandeNavigation { get; set; }


    }
}
