using System;
using System.Collections.Generic;

namespace ChatApp.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // YENİ: İstifadəçinin online/offline vəziyyətini yadda saxlayır
        public bool IsOnline { get; set; } = false;

        public List<Message> Messages { get; set; } = new();
    }
}