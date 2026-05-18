using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; } // YENİ CƏDVƏL
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Message -> User Əlaqəsi (Bir istifadəçinin çoxlu mesajı ola bilər)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict); // İstifadəçi silinəndə mesajlar səhvə düşməsin

            // 2. Message -> ChatRoom Əlaqəsi (Bir otağın çoxlu mesajı ola bilər - YENİ)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatRoom)
                .WithMany(r => r.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade); // Otaq silinsə, içindəki mesajlar da silinsin
        }
    }
}