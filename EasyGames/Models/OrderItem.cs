using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(0, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }      //  selling price snapshot

        [Range(0, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitBuyPrice { get; set; }   //  cost snapshot

        [Range(1, 9999)]
        public int Quantity { get; set; }

        [NotMapped] public decimal LineTotal => UnitPrice * Quantity;
        [NotMapped] public decimal LineProfit => (UnitPrice - UnitBuyPrice) * Quantity;

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
