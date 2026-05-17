using Microsoft.EntityFrameworkCore; 
using ChatApp.Backend.Data;          
using ChatApp.Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Xidmətlərin əlavə edilməsi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Köhnə UseSqlServer sətrini silirik və bunu yazırıq:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ChatAppMemoryDB"));

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

var app = builder.Build();

// --- 2. Middleware-ləri (App pipeline) bura əlavə et ---

// İnkişaf mərhələsində Swagger-i aktiv edirik
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Bu kod .NET-ə wwwroot içindəki HTML-i oxumağa icazə verir

// CORS-u aktiv edirik (Həmişə Authorization-dan əvvəl gəlməlidir)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// SignalR Hub-ın yolunu (route) təyin edirik
app.MapHub<ChatHub>("/chatHub");

app.Run();