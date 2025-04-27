using AuthenNet8.API.Middleware;
using AuthenNet8.Auth.Services.Token;
using MicroServiceNet8.Auth.Services.Auth;
using MicroServiceNet8.Auth.Services.Auth.Interfaces;
using MicroServiceNet8.Auth.Services.Email;
using MicroServiceNet8.Auth.Services.Email.Interfaces;
using MicroServiceNet8.Auth.Services.Google;
using MicroServiceNet8.Auth.Services.Token.Interfaces;
using MicroServiceNet8.Auth.Services.User;
using MicroServiceNet8.Auth.Services.User.Interfaces;
using MicroServiceNet8.Entity.DBContext;
using MicroServiceNet8.Repositories.Users;
using MicroServiceNet8.Repositories.Users.Interfaces;
using MicroServiceNet8.Services.Services.User;
using MicroServiceNet8.Services.Services.User.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Cấu hình sử dụng Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập 'Bearer {token}' vào đây",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

#region Life cycle DI: AddSingleton(), AddTransient(), AddScoped()
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<GoogleLoginService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
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
    //options.Authority = "https://accounts.google.com";
    //  Client ID của bạn (ứng dụng đã đăng ký với Google).
    // Token phải có aud (audience) trùng khớp với Client ID này.
    options.Audience = builder.Configuration["Jwt:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Kiểm tra iss claim trong token phải trùng với cấu hình (Jwt:Issuer)
        //ValidIssuer = "https://accounts.google.com", 
        ValidateAudience = true, // Kiểm tra aud claim trong token phải đúng (Jwt:Audience)
        ValidateLifetime = true, // Không chấp nhận token đã hết hạn
        ValidateIssuerSigningKey = true, // Xác minh token có đúng chữ ký không
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:SecretKey"] ?? string.Empty)),
        ClockSkew = TimeSpan.Zero,

        NameClaimType = builder.Configuration["Jwt:userID"],
    };
    options.Events = new JwtBearerEvents
    {
        // Events
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
// Custom xác thực JWT token & gán ClaimsPrincipal vào HttpContext.User
//app.UseMiddleware<JwtMiddleware>();
// Exception Handler
app.UseMiddleware<ExceptionHandler>();
#endregion Middleware

// Nếu dùng UseAuthentication() thì không cần middleware JwtMiddleware vì UseAuthentication() đã tự động gán ClaimsPrincipal vào HttpContext.User
// Authen là xác thực danh tính từ token
app.UseAuthentication();
// Author dùng xác thực quyền của user từ HttpContext.User sau khi pass Authen
app.UseAuthorization();

app.MapControllers();

app.Run();
