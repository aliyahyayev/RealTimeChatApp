using Microsoft.AspNetCore.SignalR;
using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // İstifadəçini fərdi otaq qrupuna daxil edən metod
        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        // Özəl mesajı həm bazaya yazan, həm də otağa ötürən metod
        public async Task SendPrivateMessage(string roomName, int senderId, string messageContent)
        {
            // 1. Göndərən istifadəçini bazadan tapırıq
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null) return;

            // 2. Bu adda otaq bazada varmı? Yoxdursa yaradırıq
            var room = await _context.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
            if (room == null)
            {
                // "1_2" formatındakı mətndən istifadəçi ID-lərini ayırırıq
                var ids = roomName.Split('_');
                int u1 = int.Parse(ids[0]);
                int u2 = int.Parse(ids[1]);

                room = new ChatRoom
                {
                    RoomName = roomName,
                    User1Id = u1,
                    User2Id = u2
                };
                _context.ChatRooms.Add(room);
                await _context.SaveChangesAsync(); // Otaq ID-si generasiya olunsun
            }

            // 3. Mesajı yaradırıq və otağa bağlayırıq
            var message = new Message
            {
                ChatRoomId = room.Id,
                UserId = senderId,
                SenderName = sender.Username,
                Content = messageContent,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // 4. Canlı olaraq mesajı YALNIZ bu otaq qrupuna göndəririk
            await Clients.Group(roomName).SendAsync("ReceivePrivateMessage", sender.Username, messageContent, roomName);
        }
    }
}