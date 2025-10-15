using System.Linq; // for Sum
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public enum OrderStatus { Pending = 0, Paid = 1 }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public List<OrderItem> Items { get; set; } = new();

        [NotMapped]
        public decimal Profit => Items?.Sum(i => (i.UnitPrice - i.UnitBuyPrice) * i.Quantity) ?? 0m;

        [NotMapped]
        public decimal TotalRevenue => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0m;

    }
}
