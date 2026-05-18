using System;
using System.Collections.Generic;

namespace ChatApp.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Frontend-dən gələn sadə şifrəni tutmaq və ya hash-ləmək üçün
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relation: İstifadəçinin göndərdiyi mesajlar
        public List<Message> Messages { get; set; } = new();
    }
}