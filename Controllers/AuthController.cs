using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ecoa.Core.Entities;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(AuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    public record RegisterRequest(string Name, string Email, string Cpf, string Password);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cpf) || request.Cpf.Length != 11 || !request.Cpf.All(char.IsDigit))
            return BadRequest(new { error = "CPF inválido. Informe 11 dígitos numéricos." });

        var result = await _authService.RegisterAsync(request.Name, request.Email, request.Cpf, request.Password);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var token = GenerateJwt(result.Value!);
        return Ok(new { token, user = new { result.Value!.Id, result.Value.Name, result.Value.Email, result.Value.Cpf, result.Value.Role } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        var token = GenerateJwt(result.Value!);
        return Ok(new { token, user = new { result.Value!.Id, result.Value.Name, result.Value.Email, result.Value.Cpf, result.Value.Role } });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _authService.GetUserByIdAsync(userId.Value);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(new
        {
            result.Value!.Id,
            result.Value.Name,
            result.Value.Email,
            result.Value.Cpf,
            result.Value.Role,
            result.Value.StellarPublicKey
        });
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private uint? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (uint.TryParse(claim, out var id))
            return id;
        return null;
    }
}
