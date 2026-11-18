using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]

//authenticates log ins and registers of users. May need changes.
public class AuthController : ControllerBase
{
    private readonly Authservice _authservice;
    private readonly JwtTokenService _jwttokenservice;

    public AuthController(
        Authservice authservice,
        JwtTokenService jwttokenservice)
    {
        _authservice = authservice;
        _jwttokenservice = jwttokenservice;
    }

    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required, MinLength(1)] string UserName,
        [Required, MinLength(8)] string Role);

    public record LoginRequest(
        [Required] string UserName,
        [Required] string Password);

    public record AuthResponse(
        int Id,
        string Email,
        string UserName,
        string Token);

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = await _authservice.RegisterAsync(
            request.Email,
            request.Password,
            request.UserName,
            request.Role);

        if (user is null)
            return Conflict("An account with that email already exists.");

        var token = _jwttokenservice.CreateToken(user);

        return Ok(new AuthResponse(
            user.UserAccountId,
            user.Email,
            user.UserName,
            user.RoleName));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _authservice.LoginAsync(
            request.UserName,
            request.Password);
        
        if (user is null)
            return Unauthorized("Invalid email or password.");

        var token = _jwttokenservice.CreateToken(user);
        
        return Ok(new AuthResponse(
            user.UserAccountId,
            user.Email,
            user.UserName,
            user.RoleName));
    }
}