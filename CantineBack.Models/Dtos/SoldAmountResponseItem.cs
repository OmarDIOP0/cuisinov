using CantineBack.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class SoldAmountResponseItem
    {
        public PaymentMethodsEnum PaymentMethod { get; set; }
        public string Code { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Amount { get; set; }
    }
}
