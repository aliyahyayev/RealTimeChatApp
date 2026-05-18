using ChatApp.Backend.Data;
using ChatApp.Backend.Hubs;
using ChatApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // Bu namespace-i əlavə et
using Microsoft.EntityFrameworkCore;
// using ChatApp.Backend.Hubs; // ChatHub hansı qovluqdadırsa, bura daxil et

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext; // SignalR Context-i əlavə edirik

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        // Konstruktorda həm context-i, həm de hubContext-i qəbul edirik
        public UsersController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("login-or-register")]
        public async Task<IActionResult> LoginOrRegister([FromBody] LoginRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                if (existingUser.PasswordHash != request.Password)
                {
                    return BadRequest("Səhv Giriş Kodu (Şifrə)!");
                }

                // Mövcud istifadəçi daxil olduqda da siyahını yeniləyə bilərik
                await _hubContext.Clients.All.SendAsync("UserStatusChanged");

                return Ok(existingUser);
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = $"{request.Username}@chat.com"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // CANLI BİLDİRİŞ: Yeni istifadəçi yarandıqda, sistemdəki hər kəsə siqnal göndərilir
            await _hubContext.Clients.All.SendAsync("UserStatusChanged");

            return Ok(newUser);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("messages/{roomName}")]
        public async Task<IActionResult> GetRoomMessages(string roomName)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoom.RoomName == roomName)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { senderUsername = m.SenderName, m.Content, m.Timestamp })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpDelete("messages/clear/{roomName}")]
        public async Task<IActionResult> ClearRoomMessages(string roomName)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ChatRoom.RoomName == roomName)
                    .ToListAsync();

                if (messages.Any())
                {
                    _context.Messages.RemoveRange(messages);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = $"'{roomName}' otağının tarixçəsi uğurla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Silinmə zamanı daxili xəta baş verdi: {ex.Message}");
            }
        }
    }
}