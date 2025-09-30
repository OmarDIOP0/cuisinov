using System.ComponentModel.DataAnnotations;

namespace CantineFront.Models.Request
{
    public class ArticleCURequest
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public int PrixDeVente { get; set; }
        public int? PrixDachat { get; set; }
        [Required]
        public int QuantiteStock { get; set; }
        public byte[]? Image { get; set; }

        public string? ImageBase64 { get; set; }

        [Required]
        public int? CategorieId { get; set; }
        public bool IsArticleOnMenu { get; set; }
        public bool IsApproved { get; set; }
        public bool Actif { get; set; }

        public bool ControlStockQuantity { get; set; } = true;

        public int EntrepriseId { get; set; }
    }
}
