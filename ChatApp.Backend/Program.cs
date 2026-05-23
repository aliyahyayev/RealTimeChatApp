using Microsoft.EntityFrameworkCore;
using ChatApp.Backend.Data;
using ChatApp.Backend.Hubs;

// .NET-ə deyirik ki, Docker daxilində həm HTTP (8080), həm HTTPS (8081) portlarını aktiv etsin
Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://+:8080;https://+:8081");

// --- 1. PORT AYARI (Railway üçün mütləqdir) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

var builder = WebApplication.CreateBuilder(args);

// .NET-ə Railway-in verdiyi portu dinləməsini əmr edirik
builder.WebHost.UseUrls($"http://*:{port}");

// Xidmətlərin əlavə edilməsi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

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

// Canlıda (Railway-də) Swagger-i görmək və test etmək üçün bu şərti ləğv edirik
app.UseSwagger();
app.UseSwaggerUI();

// Railway HTTP sorğularını özü idarə etdiyi üçün canlıda bu bəzən xətaya səbəb olur,
// müvəqqəti olaraq şərhə (comment) alırıq:
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();