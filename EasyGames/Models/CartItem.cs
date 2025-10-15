using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // IdentityUser.Id

        [Required]
        public int ProductId { get; set; }

        [Range(1, 9999)]
        public int Quantity { get; set; } = 1;

        // navigation (optional for now)
        public Product? Product { get; set; }
    }
}
