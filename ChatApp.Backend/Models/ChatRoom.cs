namespace ChatApp.Backend.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }

        // Otağın unikal mətni (Məsələn frontend-də işlətdiyimiz: "1_2" və ya "Ali_Ayxan" forması üçün)
        public string RoomName { get; set; } = string.Empty;

        // Otaqdakı 1-ci istifadəçi
        public int User1Id { get; set; }

        // Otaqdakı 2-ci istifadəçi
        public int User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Bu otağa aid olan mesajların siyahısı
        public List<Message> Messages { get; set; } = new();
    }
}