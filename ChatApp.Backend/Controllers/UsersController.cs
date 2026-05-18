using ChatApp.Backend.Data;
using ChatApp.Backend.Hubs;
using ChatApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

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

                // DƏYİŞDİ: Login olanda istifadəçini online edirik
                existingUser.IsOnline = true;
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("UserStatusChanged");
                return Ok(existingUser);
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password,
                Email = $"{request.Username}@chat.com",
                IsOnline = true // DƏYİŞDİ: Yeni qeydiyyat da birbaşa online sayılır
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UserStatusChanged");
            return Ok(newUser);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            // DƏYİŞDİ: Artıq sol paneldə YALNIZ hal-hazırda tətbiqdə aktiv (Online) olanlar görünəcək
            var users = await _context.Users
                .Where(u => u.IsOnline)
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