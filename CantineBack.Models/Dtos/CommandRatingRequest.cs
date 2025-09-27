using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class CommandRatingRequest
    {
        public string CustomerFeedback { get; set; }
        public int? Rate { get; set; }
        public int Id { get; set; }
    }
}
