using Microsoft.AspNetCore.SignalR;
using ChatApp.Backend.Data;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int userId, string message)
        {
            // 1. Mesajı bazaya yadda saxla
            var newMessage = new Message
            {
                UserId = userId,
                Content = message,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // 2. Mesajı bütün qoşulmuş istifadəçilərə göndər
            // İstifadəçi adını bazadan tapıb göndərmək daha yaxşı olar
            var user = await _context.Users.FindAsync(userId);
            string username = user?.Username ?? "Anonim";

            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}