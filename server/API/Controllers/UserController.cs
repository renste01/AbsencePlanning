using System.ComponentModel.DataAnnotations;
using Infra.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(UserAccountService userAccountService) : ControllerBase
{
    public record UserResponse(
        int UserAccountId,
        int EmployeeId,
        string RoleName,
        string Email,
        string UserName);

    public record CreateUserRequest(
        [Required] int EmployeeId,
        [Required] string RoleName,
        [Required, EmailAddress] string Email,
        [Required, MinLength(1)] string UserName,
        [Required, MinLength(8)] string Password);

    public record UpdateUserRequest(
        [Required] int EmployeeId,
        [Required] string RoleName,
        [Required, EmailAddress] string Email,
        [Required, MinLength(1)] string UserName,
        string Password);

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await userAccountService.GetAllAsync();
        return Ok(users.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await userAccountService.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var user = await userAccountService.CreateAsync(
            request.EmployeeId,
            request.RoleName,
            request.Email,
            request.UserName,
            request.Password);

        if (user is null)
            return Conflict("Email or username is already in use.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.UserAccountId },
            ToResponse(user));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(
        int id,
        UpdateUserRequest request)
    {
        var user = await userAccountService.UpdateAsync(
            id,
            request.EmployeeId,
            request.RoleName,
            request.Email,
            request.UserName,
            request.Password);

        if (user is null)
        {
            var existingUser = await userAccountService.GetByIdAsync(id);
            if (existingUser is null)
                return NotFound();

            return Conflict("Email or username is already in use.");
        }

        return Ok(ToResponse(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await userAccountService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static UserResponse ToResponse(UserAccount user)
    {
        return new UserResponse(
            user.UserAccountId,
            user.EmployeeId,
            user.RoleName,
            user.Email,
            user.UserName);
    }
}
