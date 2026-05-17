namespace ChatApp.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Şifrəni açıq saxlamırıq!
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İstifadəçinin göndərdiyi mesajların siyahısı (Relation)
        public List<Message> Messages { get; set; } = new();
    }
}
