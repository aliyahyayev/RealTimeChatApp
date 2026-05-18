using Microsoft.AspNetCore.SignalR;
using ChatApp.Backend.Data;
using ChatApp.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        // YENİ: Hansı connectionId-nin hansı UserId-yə aid olduğunu yadda saxlayırıq
        private static readonly Dictionary<string, int> UserConnections = new Dictionary<string, int>();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // YENİ: İstifadəçi SignalR-a qoşulanda işə düşür
        public async Task RegisterConnection(int userId)
        {
            UserConnections[Context.ConnectionId] = userId;

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = true;
                await _context.SaveChangesAsync();
            }

            // Hər kəsə sol paneli yeniləməsi üçün siqnal göndər
            await Clients.All.SendAsync("UserStatusChanged");
        }

        // YENİ: İstifadəçi brauzeri bağlayanda və ya connection.stop() edəndə avtomatik işə düşür
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (UserConnections.TryGetValue(Context.ConnectionId, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    await _context.SaveChangesAsync();
                }
                UserConnections.Remove(Context.ConnectionId);
            }

            // Hər kəsə istifadəçinin çıxdığını anında xəbər ver
            await Clients.All.SendAsync("UserStatusChanged");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task SendPrivateMessage(string roomName, int senderId, string messageContent)
        {
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null) return;

            if (roomName == "global")
            {
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

            var room = await _context.ChatRooms.FirstOrDefaultAsync(r => r.RoomName == roomName);
            if (room == null)
            {
                try
                {
                    var ids = roomName.Split('_');
                    if (ids.Length == 2 && int.TryParse(ids[0], out int u1) && int.TryParse(ids[1], out int u2))
                    {
                        room = new ChatRoom { RoomName = roomName, User1Id = u1, User2Id = u2 };
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
                    return;
                }
            }

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

            await Clients.Group(roomName).SendAsync("ReceivePrivateMessage", sender.Username, messageContent, roomName);
        }
    }
}