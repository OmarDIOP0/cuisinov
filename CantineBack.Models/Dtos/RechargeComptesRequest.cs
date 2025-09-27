using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class RechargeComptesRequest
    {
       
        public int Montant { set; get; }    
        public List<int>? UserIds { set; get; }    
    }
}
