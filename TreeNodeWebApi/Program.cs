using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TreeNodeWebApi.Data;
using TreeNodeWebApi.Interfaces;
using TreeNodeWebApi.Midllewares;
using TreeNodeWebApi.Models.Entities;
using TreeNodeWebApi.Repositories;
using TreeNodeWebApi.Services;
using TreeNodeWebApi.Services.Auth;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Вставьте JWT токен следующим образом: Bearer {ваш_токен}",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasherService, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITreeService, TreeService>();
builder.Services.AddScoped<ITreeRepository, TreeNodeRepository>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString.StartsWith("Data Source="))
{
    var relativePath = connectionString.Substring("Data Source=".Length);
    var fullPath = Path.GetFullPath(relativePath);

    var directory = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }

    connectionString = $"Data Source={fullPath}";
    Console.WriteLine($"Полный путь к БД: {fullPath}");
}

SQLitePCL.Batteries.Init();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    await Task.Delay(1000);

    var serviceProvider = scope.ServiceProvider;

    await SeedAdmin(serviceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedAdmin(IServiceProvider serviceProvider)
{
    var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasherService>();

    var adminEmail = "admin@myapp.ru";
    var adminPassword = "AdminSecurePassword123!";

    var existingAdmin = await userRepository.FindByEmail(adminEmail);

    if (existingAdmin == null)
    {
        string passwordHash = passwordHasher.HashPassword(adminPassword);

        var admin = new User
        {
            UserName = "Administrator",
            Email = adminEmail,
            PasswordHash = passwordHash,
            Role = "Admin",
        };

        await userRepository.AddUser(admin);

        Console.WriteLine("Администратор создан успешно");
        Console.WriteLine($"Email: {adminEmail}");
        Console.WriteLine($"Пароль: {adminPassword}");
    }
    else
    {
        Console.WriteLine("Администратор уже существует");
    }
}