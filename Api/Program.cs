using Application.Interfaces;
using Domain.Enums;
using DomainUser = Domain.Entities.User;
using Infrastructure.Configuration;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Keep a stable local URL/port during development runs.
builder.WebHost.UseUrls("http://localhost:5000");

var jwtKey = builder.Configuration["JwtSettings:Key"]
             ?? "DevOnly_ChangeThisJwtKey_AtLeast32Characters!";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ApiUsers";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "ApiUsersClient";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var mongoOptions = builder.Configuration.GetSection("Mongo").Get<MongoOptions>() ?? new MongoOptions();

var mongoConnectionString = mongoOptions.ConnectionString;
var mongoDatabaseName = mongoOptions.DatabaseName;
var usersCollectionName = mongoOptions.GetCollectionName("Users");
var tasksCollectionName = mongoOptions.GetCollectionName("Tasks");

builder.Services.AddScoped<IUserService>(sp =>
    new MongoUserService(mongoConnectionString, mongoDatabaseName, usersCollectionName));

var app = builder.Build();

await EnsureRootUserAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference(options =>
        options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (IHostEnvironment env) =>
    env.IsDevelopment()
        ? Results.Redirect("/scalar")
        : Results.Ok(new { message = "API running" }));

app.MapControllers();

app.Run();

static async Task EnsureRootUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    var users = await userService.UserGetAll();
    if (users.Any(u => u.Role == Role.Root))
    {
        Console.WriteLine("[Startup] Ya existe un usuario con rol Root.");
        return;
    }

    Console.WriteLine("[Startup] No existe usuario Root. Se iniciará creación por consola.");

    var nombres = ReadRequiredValue("Nombres");
    var apellidos = ReadRequiredValue("Apellidos");
    var cedula = ReadRequiredLong("Cédula (mínimo 7 dígitos)", 1_000_000);
    var telefono = ReadRequiredLong("Teléfono (mínimo 10 dígitos)", 1_000_000_000);
    var email = ReadRequiredValue("Email");
    var password = ReadRequiredValue("Password (mínimo 5 caracteres)");

    var root = DomainUser.CreateWithPassword(
        nombres,
        apellidos,
        cedula,
        telefono,
        email,
        Role.Root,
        password);

    await userService.UserRegister(root);
    Console.WriteLine("[Startup] Usuario Root creado correctamente.");
}

static string ReadRequiredValue(string label)
{
    while (true)
    {
        Console.Write($"> {label}: ");
        var input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("Valor obligatorio. Intenta nuevamente.");
    }
}

static long ReadRequiredLong(string label, long minValue)
{
    while (true)
    {
        var valueText = ReadRequiredValue(label);
        if (long.TryParse(valueText, out var value) && value >= minValue)
        {
            return value;
        }

        Console.WriteLine($"Debe ser un número válido mayor o igual a {minValue}.");
    }
}