namespace ChatApp.Backend.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? SenderName { get; set; } // Göndərən
        public string? Content { get; set; }    // Mesajın mətni
        public DateTime Timestamp { get; set; } = DateTime.Now; // Vaxt

        // Foreign Key - Mesajı kim göndərib?
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
