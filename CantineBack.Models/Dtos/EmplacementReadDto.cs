using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class EmplacementReadDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? EntrepriseId { get; set;  }
        public bool Actif { get; set; }
        public Entreprise? Entreprise { get; set; }
    }
}
