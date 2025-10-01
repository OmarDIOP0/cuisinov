using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Matricule { get; set; }
        public string? Login { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public bool? ResetPassword { get; set; } = true;
        //public string? Password { get; set; }
        public string? QrCode { get; set; }
        public string? Guid { get; set; }
        public int Solde { get; set; }
        public string? Profile { get; set; }
        public int? DepartmentId { get; set; }
        public bool Actif { get; set; }
        public bool? IsInterimEmployee { get; set; }
        public int? EntrepriseId { get; set; }
        public string? Bureau { get; set; }
        //public bool? UseActiveDirectoryAuth { get; set; }
        public DateTime? LastRechargeDate { get; set; }
        public int? LastRechargeAmount { get; set; }
        public DepartmentReadDto? Department { get; set; }
        public Entreprise? Entreprise { get; set; }
    }
}
