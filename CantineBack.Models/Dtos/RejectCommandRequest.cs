using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class RejectCommandRequest
    {
        public string Reason { get; set; }
        public int Id { get; set; }
    }
}
