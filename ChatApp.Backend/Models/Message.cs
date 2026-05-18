using System;

namespace ChatApp.Backend.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? SenderName { get; set; } // Göndərən şəxsin adı (ekranda birbaşa göstərmək üçün)
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // 1. Foreign Key - Mesajı kim göndərib?
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // 2. Foreign Key - Mesaj hansı otağa göndərilib? (YENİ ƏLAVƏ)
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
    }
}