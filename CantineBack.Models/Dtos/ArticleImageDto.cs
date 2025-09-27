using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class ArticleImageDto
    {
        public int Id { get; set; }
      
        public byte[]? Image { get; set; }
    }
}
