﻿using AuthenNet8.API.Middleware;
using AuthenNet8.Services.Repositories.Users;
using AuthenNet8.Services.Services.Auth;
using AuthenNet8.Services.Services.Email;
using AuthenNet8.Services.Services.Token;
using MicroServiceNet8.Entity.DBContext;
using MicroServiceNet8.Services.Repositories.Users.Interfaces;
using MicroServiceNet8.Services.Services.Auth.Interfaces;
using MicroServiceNet8.Services.Services.Email.Interfaces;
using MicroServiceNet8.Services.Services.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Life cycle DI: AddSingleton(), AddTransient(), AddScoped()
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<GoogleLoginService>();
#endregion Life cycle DI: AddSingleton(), AddTransient(), AddScoped()

builder.Services.AddHttpContextAccessor();

// Khởi tạo conection string
builder.Services.AddDbContext<DBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DBPostgres16"));
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

#region Cấu hình xác thực JWT Bearer với Google - xác thực token đăng nhập Google được client (frontend, mobile app, Postman…) gửi lên
// Khi người dùng login with Google thành công, bạn sẽ nhận được id_token từ phía client, rồi gửi token này đến backend .NET,  middleware .NET sẽ
// -> Xác thực token (qua Google Authority)
// -> Giải mã token
// - Gắn thông tin người dùng vào HttpContext.User (tức là ClaimsPrincipal)
builder.Services.AddAuthentication(options =>
{
    // Thiết lập hệ thống xác thực mặc định là JWT Bearer (tức là request sẽ mang theo token dạng Authorization: Bearer <token>).
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Nơi token được phát hành — Google's OAuth 2.0 server.
    // Authority sẽ được dùng để lấy metadata (public key, issuer, v.v...) nhằm xác thực token do Google phát.
    options.Authority = "https://accounts.google.com";
    //  Client ID của bạn (ứng dụng đã đăng ký với Google).
    // Token phải có aud (audience) trùng khớp với Client ID này.
    options.Audience = builder.Configuration["Jwt:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Bắt buộc token phải đến từ https://accounts.google.com
        ValidIssuer = "https://accounts.google.com", 
        ValidateAudience = true, // Bắt buộc aud trong token phải đúng với Audience bạn khai báo
        ValidateLifetime = true, // Không chấp nhận token đã hết hạn
        ValidateIssuerSigningKey = true // Kiểm tra chữ ký token có hợp lệ không (Google sẽ cung cấp public key để kiểm chứng)
    };
});
#endregion Cấu hình xác thực JWT Bearer với Google - xác thực token đăng nhập Google được client (frontend, mobile app, Postman…) gửi lên

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Middleware
// Anti XSS
app.UseMiddleware<AntiXssMiddleware>();
// Exception Handler
app.UseMiddleware<ExceptionHandler>();
#endregion Middleware

// Ánh xạ các route vào Controller
app.UseAuthorization();

app.MapControllers();

app.Run();
