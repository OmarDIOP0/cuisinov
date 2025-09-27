using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class LigneCommandeReadDto
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int CommandeId { get; set; }
        public int Quantite { get; set; }
        public int PrixTotal { get; set; }

        public ArticleCommandReadDto? ArticleNavigation { get; set; }
    }
}
