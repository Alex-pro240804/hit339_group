using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models.ViewModels
{
    public class EmailBlastVM
    {
        [Required] public string Target { get; set; } = "All"; // All, Bronze, Silver, Gold, Platinum
        [Required, StringLength(120)] public string Subject { get; set; } = string.Empty;
        [Required] public string Body { get; set; } = string.Empty;

        // Output/preview
        public int RecipientCount { get; set; }
        public List<string> Recipients { get; set; } = new();
    }
}
