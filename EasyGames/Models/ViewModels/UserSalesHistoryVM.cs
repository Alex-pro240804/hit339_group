namespace EasyGames.Models.ViewModels
{
    public class UserSalesHistoryVM
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public string Tier { get; set; } = "Bronze";
        public List<Order> Orders { get; set; } = new();
    }
}
