using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Data;
using ChatApp.Backend.Models;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Frontend-dən gələn məlumatları qarşılamaq üçün kiçik model (DTO)
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Giriş və ya Qeydiyyat (POST: api/users/login-or-register)
        [HttpPost("login-or-register")]
        public async Task<IActionResult> LoginOrRegister([FromBody] LoginRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                // İstifadəçi tapıldı, şifrəni yoxlayırıq
                if (existingUser.PasswordHash != request.Password)
                {
                    return BadRequest("Səhv Giriş Kodu (Şifrə)!");
                }
                return Ok(existingUser); // Giriş uğurludur
            }

            // İstifadəçi yoxdursa, yeni hesab yaradırıq
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = $"{request.Username}@chat.com"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(newUser);
        }

        // 2. Çat siyahısı üçün digər bütün istifadəçiləri gətirmək (GET: api/users/all)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username }) // Şifrələri frontenda göndərmirik (Təhlükəsizlik)
                .ToListAsync();

            return Ok(users);
        }

        // 3. Konkret bir özəl otağın mesaj tarixçəsini gətirmək (GET: api/users/messages/{roomName})
        [HttpGet("messages/{roomName}")]
        public async Task<IActionResult> GetRoomMessages(string roomName)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatRoom.RoomName == roomName)
                .OrderBy(m => m.Timestamp)
                // senderUsername olaraq anonim obyekt yaradırıq ki, frontend tam oxuya bilsin
                .Select(m => new { senderUsername = m.SenderName, m.Content, m.Timestamp })
                .ToListAsync();

            return Ok(messages);
        }

        // 4. BİR OTAĞIN BÜTÜN MESAJ TARİXÇƏSİNİ SİLMƏK (DELETE: api/users/messages/clear/{roomName})
        [HttpDelete("messages/clear/{roomName}")]
        public async Task<IActionResult> ClearRoomMessages(string roomName)
        {
            try
            {
                // Həmin otağa aid olan bütün mesajları bazadan tapırıq
                var messages = await _context.Messages
                    .Where(m => m.ChatRoom.RoomName == roomName)
                    .ToListAsync();

                if (messages.Any())
                {
                    // Tapılan bütün mesajları toplu şəkildə silirik
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