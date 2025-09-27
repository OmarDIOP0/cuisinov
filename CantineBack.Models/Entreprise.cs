using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models
{
    public class Entreprise
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Addresse { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public bool Actif { get; set; }

        public virtual ICollection<Department>? Departments { get; set; } = new HashSet<Department>();
        public virtual ICollection<User>? Users { get; set; } = new HashSet<User>();
        public virtual ICollection<Article>? Articles { get; set; } = new HashSet<Article>();
        public virtual ICollection<Commande>? Commandes { get; set; } = new HashSet<Commande>();
    }
}
