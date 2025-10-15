using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public enum ProductCategory { Book = 0, Game = 1, Toy = 2 }

    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ProductCategory Category { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        //supplier / source
        [StringLength(120)]
        public string? Source { get; set; }

        //cost price (money type)
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999)]
        public decimal BuyPrice { get; set; }

        //sell price (money type)
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int StockQty { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [ScaffoldColumn(false)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ScaffoldColumn(false)]
        public DateTime? UpdatedAt { get; set; }

        // Read-only helpers (not stored in DB)
        [NotMapped] public decimal MarginPerUnit => Price - BuyPrice;
        [NotMapped] public decimal MarginPercent => Price == 0 ? 0 : (Price - BuyPrice) / Price * 100m;
    }
}
