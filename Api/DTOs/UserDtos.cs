using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Api.DTOs;

public class RegisterUserRequest
{
    [Required]
    [MinLength(1)]
    public string Nombres { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public string Apellidos { get; set; } = null!;

    [Required]
    [Range(1000000, long.MaxValue)]
    public long Cedula { get; set; }

    [Required]
    [Range(1000000000, long.MaxValue)]
    public long Telefono { get; set; }

    [Required]
    [EmailAddress]
    [MinLength(1)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = null!;

    public Role Role { get; set; } = Role.User;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public string Password { get; set; } = null!;
}

public class UpdateUserRequest
{
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public long? Cedula { get; set; }
    public long? Telefono { get; set; }
    public string? Email { get; set; }
    public Role? Role { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    [MinLength(1)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(5)]
    public string NewPassword { get; set; } = null!;
}

public class AdminChangeUserPasswordRequest
{
    public long? Cedula { get; set; }
    public string? Email { get; set; }

    [Required]
    [MinLength(5)]
    public string NewPassword { get; set; } = null!;
}

public class UploadUserPhotoRequest
{
    [Required]
    public IFormFile Photo { get; set; } = null!;
}

public class UserResponse
{
    public string? Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public long Cedula { get; set; }
    public long Telefono { get; set; }
    public string Email { get; set; } = null!;
    public Role Role { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ApiMessageResponse
{
    public string Message { get; set; } = null!;
}

public class LoginResponse
{
    public bool Authenticated { get; set; }
    public string Message { get; set; } = null!;
    public string? Token { get; set; }
    public string? Role { get; set; }
}