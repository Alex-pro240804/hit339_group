using System.Diagnostics;

namespace EasyGames.Services
{
    public class ConsoleEmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string htmlBody)
        {
            Console.WriteLine($"[EMAIL] To:{to} | Subj:{subject}\n{htmlBody}\n---");
            Debug.WriteLine($"[EMAIL] To:{to} | Subj:{subject}");
            return Task.CompletedTask;
        }
    }
}
