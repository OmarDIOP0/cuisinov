using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class PostLigneCommandDto
    {
        public int ArticleId { get; set; }
        public int Quantite { get; set; }
        public int PrixTotal { get; set; }
    }
}
