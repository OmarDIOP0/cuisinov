using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Dtos
{
    public class ArticleCommandReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PrixDeVente { get; set; }
        public int? PrixDachat { get; set; }
        public int QuantiteStock { get; set; }
        public String Image { get; set; }
        public int CategorieId { get; set; }
        public bool IsArticleOnMenu { get; set; }
        public bool IsApproved { get; set; }
    }
}
