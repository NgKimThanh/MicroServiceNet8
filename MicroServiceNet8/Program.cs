using AuthenNet8.Middleware;
using AuthenNet8.Repositories.Users;
using AuthenNet8.Services.Auth;
using AuthenNet8.Services.Email;
using AuthenNet8.Services.Token;
using MicroServiceNet8.Entities.Repositories.Users.Interfaces;
using MicroServiceNet8.Entity.DBContext;
using MicroServiceNet8.Services.Services.Auth.Interfaces;
using MicroServiceNet8.Services.Services.Email.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Life cycle DI: AddSingleton(), AddTransient(), AddScoped()
//builder.Services.AddScoped<IGroupOfProductService, GroupOfProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

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
