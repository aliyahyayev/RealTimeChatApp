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
            // 1. Göndərən istifadəçini yoxlayırıq
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null) return;

            // 2. Əgər ümumi otaqdırsa, otaq yaratma məntiqini keçirik
            if (roomName == "global")
            {
                // Ümumi otaq üçün verilənlər bazasında xüsusi bir ID təyin edə bilərik (məsələn: 0)
                var globalRoom = await _context.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == "global");
                if (globalRoom == null)
                {
                    globalRoom = new ChatRoom { RoomName = "global", User1Id = 0, User2Id = 0 };
                    _context.ChatRooms.Add(globalRoom);
                    await _context.SaveChangesAsync();
                }

                var globalMessage = new Message
                {
                    ChatRoomId = globalRoom.Id,
                    UserId = senderId,
                    SenderName = sender.Username,
                    Content = messageContent,
                    Timestamp = DateTime.Now
                };
                _context.Messages.Add(globalMessage);
                await _context.SaveChangesAsync();

                await Clients.Group(roomName).SendAsync("ReceivePrivateMessage", sender.Username, messageContent, roomName);
                return;
            }

            // 3. Əgər özəl otaqdırsa (Məsələn: "1_2")
            var room = await _context.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
            if (room == null)
            {
                try
                {
                    var ids = roomName.Split('_');
                    if (ids.Length == 2 && int.TryParse(ids[0], out int u1) && int.TryParse(ids[1], out int u2))
                    {
                        room = new ChatRoom
                        {
                            RoomName = roomName,
                            User1Id = u1,
                            User2Id = u2
                        };
                        _context.ChatRooms.Add(room);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        throw new Exception("Otaq adı formatı düzgün deyil!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Otaq yaradılarkən xəta: {ex.Message}");
                    return; // Serverin çökməsinin qarşısını alırıq
                }
            }

            // 4. Mesajı bazaya yazırıq
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

            // 5. Yalnız otaqdakı insanlara göndəririk
            await Clients.Group(roomName).SendAsync("ReceivePrivateMessage", sender.Username, messageContent, roomName);
        }
    }
}