using CantineBack.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class SoldAmountRequest
    {

      public  List<PaymentMethodsEnum>? PaymentMethods { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }


        public bool UseDateTimeFilter { get; set; }

    }
}
