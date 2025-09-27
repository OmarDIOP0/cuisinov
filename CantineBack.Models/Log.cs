using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models
{
    public class Log
    {

        public int Id { get; set; }
        public DateTime Date { get; set; }= DateTime.Now;
        [MaxLength(1000)]
        public string Comment { get; set; } = null!;
        [MaxLength(100)]
        public string Entity { get; set; } = null!;
        [MaxLength(100)]
        public string ActionType { get; set; } = null!;
        [MaxLength(100)]
        public string ActionUser { get; set; } = null!;
        [MaxLength(100)]
        public string ActionProfil { get; set; } = null!;
    }
}
