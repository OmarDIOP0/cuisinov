using CantineBack.Models;

namespace CantineFront.Models.Request
{
    public class UserCURequest
    {
      
        public int Id { get; set; }
        public string? Nom { get; set; } = null!;
        public string? Prenom { get; set; } = null!;
        public string? Matricule { get; set; } 
        public string? Email { get; set; } 
        public string Login { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string? QrCode { get; set; }
        public string? Guid { get; set; }
        public int Solde { get; set; }
        public string? Profile { get; set; } = null!;
        public int? DepartmentId { get; set; }
        public bool Actif { get; set; } = true;
        public int EntrepriseId { get; set; }

        public bool? ResetPassword { get; set; } = true;
        public string? Password { get; set; }

        public bool? IsInterimEmployee { get; set; }
        public bool? UseActiveDirectoryAuth { get; set; }
        public string? EntrepriseCode { get; set; }
        public string? Bureau { get; set; }
    }
}
