using Api.DTOs;
using Application.Interfaces;
using Domain.Enums;
using DomainUser = Domain.Entities.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    // Inyección de dependencias del servicio de usuarios
    // El controlador se comunica con la capa de aplicación a través de IUserService
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public UsersController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<ApiMessageResponse>> Register([FromBody] RegisterUserRequest request)
    {
        if (User.IsInRole(Role.Admin.ToString()) && request.Role == Role.Root)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiMessageResponse { Message = "Un usuario Admin no puede crear usuarios con rol Root." });
        }

        try
        {
            var user = DomainUser.CreateWithPassword(
                request.Nombres,
                request.Apellidos,
                request.Cedula,
                request.Telefono,
                request.Email,
                request.Role,
                request.Password);

            await _userService.UserRegister(user);

            return CreatedAtAction(
                nameof(FindUser),
                new { cedula = user.Cedula },
                new ApiMessageResponse { Message = "Usuario registrado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiMessageResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiMessageResponse { Message = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var authenticated = await _userService.UserAuthenticate(request.Email, request.Password);
        if (!authenticated)
        {
            return Unauthorized(new LoginResponse
            {
                Authenticated = false,
                Message = "Credenciales inválidas.",
                Token = null,
                Role = null
            });
        }

        var user = await _userService.UserFindByCedulaOrEmail(null, request.Email);
        if (user == null)
        {
            return Unauthorized(new LoginResponse
            {
                Authenticated = false,
                Message = "Credenciales inválidas.",
                Token = null,
                Role = null
            });
        }

        var token = GenerateJwtToken(user);

        return Ok(new LoginResponse
        {
            Authenticated = true,
            Message = "Autenticación correcta.",
            Token = token,
            Role = user.Role.ToString()
        });
    }

    [HttpGet]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> ListUsers()
    {
        var users = await _userService.UserGetAll();
        return Ok(users.Select(MapUser));
    }

    [HttpGet("search")]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<UserResponse>> FindUser([FromQuery] long? cedula, [FromQuery] string? email)
    {
        if (!cedula.HasValue && string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ApiMessageResponse
            {
                Message = "Debe enviar cedula o email para buscar el usuario."
            });
        }

        var user = await _userService.UserFindByCedulaOrEmail(cedula, email);
        if (user == null)
        {
            return NotFound(new ApiMessageResponse { Message = "Usuario no encontrado." });
        }

        return Ok(MapUser(user));
    }

    [HttpPut]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<ApiMessageResponse>> UpdateUser(
        [FromQuery] long? cedula,
        [FromQuery] string? email,
        [FromBody] UpdateUserRequest request)
    {
        if (User.IsInRole(Role.Admin.ToString()) && request.Role == Role.Root)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiMessageResponse { Message = "Un usuario Admin no puede asignar el rol Root." });
        }

        if (!cedula.HasValue && string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ApiMessageResponse
            {
                Message = "Debe enviar cedula o email para editar el usuario."
            });
        }

        var existing = await _userService.UserFindByCedulaOrEmail(cedula, email);
        if (existing == null)
        {
            return NotFound(new ApiMessageResponse { Message = "Usuario no encontrado." });
        }

        try
        {
            var nombres = string.IsNullOrWhiteSpace(request.Nombres) ? existing.Nombres : request.Nombres.Trim();
            var apellidos = string.IsNullOrWhiteSpace(request.Apellidos) ? existing.Apellidos : request.Apellidos.Trim();
            var newCedula = request.Cedula ?? existing.Cedula;
            var telefono = request.Telefono ?? existing.Telefono;
            var newEmail = string.IsNullOrWhiteSpace(request.Email) ? existing.Email : request.Email.Trim();
            var role = request.Role ?? existing.Role;

            var updatedUser = new DomainUser(
                nombres,
                apellidos,
                newCedula,
                telefono,
                newEmail,
                role,
                existing.PasswordHash);

            await _userService.UserUpdateByCedulaOrEmail(cedula, email, updatedUser);
            return Ok(new ApiMessageResponse { Message = "Usuario actualizado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiMessageResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiMessageResponse { Message = ex.Message });
        }
    }

    [HttpDelete]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<ApiMessageResponse>> DeleteUser([FromQuery] long? cedula, [FromQuery] string? email)
    {
        if (!cedula.HasValue && string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ApiMessageResponse
            {
                Message = "Debe enviar cedula o email para borrar el usuario."
            });
        }

        try
        {
            await _userService.UserDeleteByCedulaOrEmail(cedula, email);
            return Ok(new ApiMessageResponse { Message = "Usuario borrado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiMessageResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiMessageResponse { Message = ex.Message });
        }
    }

    [HttpPut("password")]
    public async Task<ActionResult<ApiMessageResponse>> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                    ?? User.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new ApiMessageResponse { Message = "No se pudo identificar el usuario autenticado." });
        }

        if (string.Equals(request.CurrentPassword, request.NewPassword, StringComparison.Ordinal))
        {
            return BadRequest(new ApiMessageResponse
            {
                Message = "La nueva contraseña debe ser diferente a la actual."
            });
        }

        var authenticated = await _userService.UserAuthenticate(email, request.CurrentPassword);
        if (!authenticated)
        {
            return BadRequest(new ApiMessageResponse { Message = "La contraseña actual no es correcta." });
        }

        var existing = await _userService.UserFindByCedulaOrEmail(null, email);
        if (existing == null)
        {
            return NotFound(new ApiMessageResponse { Message = "Usuario no encontrado." });
        }

        try
        {
            var updatedUser = DomainUser.CreateWithPassword(
                existing.Nombres,
                existing.Apellidos,
                existing.Cedula,
                existing.Telefono,
                existing.Email,
                existing.Role,
                request.NewPassword);

            await _userService.UserUpdateByCedulaOrEmail(existing.Cedula, null, updatedUser);

            return Ok(new ApiMessageResponse { Message = "Contraseña actualizada correctamente." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiMessageResponse { Message = ex.Message });
        }
    }

    [HttpPut("password/admin")]
    [Authorize(Roles = "Root,Admin")]
    public async Task<ActionResult<ApiMessageResponse>> AdminChangeUserPassword([FromBody] AdminChangeUserPasswordRequest request)
    {
        var hasCedula = request.Cedula.HasValue;
        var hasEmail = !string.IsNullOrWhiteSpace(request.Email);

        if (!hasCedula && !hasEmail)
        {
            return BadRequest(new ApiMessageResponse
            {
                Message = "Debe enviar cedula o email para identificar el usuario."
            });
        }

        // If both are provided, prioritize email as requested.
        var lookupCedula = hasEmail ? null : request.Cedula;
        var lookupEmail = hasEmail ? request.Email : null;

        var targetUser = await _userService.UserFindByCedulaOrEmail(lookupCedula, lookupEmail);
        if (targetUser == null)
        {
            return NotFound(new ApiMessageResponse { Message = "Usuario no encontrado." });
        }

        if (User.IsInRole(Role.Admin.ToString())
            && (targetUser.Role == Role.Root || targetUser.Role == Role.Admin))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiMessageResponse
                {
                    Message = "Un usuario Admin no puede cambiar la contraseña de cuentas Root o Admin."
                });
        }

        try
        {
            var updatedUser = DomainUser.CreateWithPassword(
                targetUser.Nombres,
                targetUser.Apellidos,
                targetUser.Cedula,
                targetUser.Telefono,
                targetUser.Email,
                targetUser.Role,
                request.NewPassword);

            await _userService.UserUpdateByCedulaOrEmail(targetUser.Cedula, null, updatedUser);

            return Ok(new ApiMessageResponse { Message = "Contraseña actualizada correctamente." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiMessageResponse { Message = ex.Message });
        }
    }

    private static UserResponse MapUser(DomainUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Cedula = user.Cedula,
            Telefono = user.Telefono,
            Email = user.Email,
            Role = user.Role,
            PhotoUrl = null
        };
    }

    private string GenerateJwtToken(DomainUser user)
    {
        var jwtKey = _configuration["JwtSettings:Key"]
                     ?? "DevOnly_ChangeThisJwtKey_AtLeast32Characters!";
        var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "ApiUsers";
        var jwtAudience = _configuration["JwtSettings:Audience"] ?? "ApiUsersClient";

        var expireMinutes = 60;
        var configuredExpire = _configuration["JwtSettings:ExpireMinutes"];
        if (!string.IsNullOrWhiteSpace(configuredExpire) && int.TryParse(configuredExpire, out var parsedExpire) && parsedExpire > 0)
        {
            expireMinutes = parsedExpire;
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id ?? user.Cedula.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("cedula", user.Cedula.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}