using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models
{
    public class QrCodeStaff
    {
        public string Matricule { get; set; }=null!; 
        public string? Departement { get; set; }
        public string? FullName { get; set; }
    }
}
