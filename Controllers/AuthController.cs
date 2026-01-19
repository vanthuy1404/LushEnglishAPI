// 2025
// DANGTHUY

using LushEnglishAPI.Data;
using LushEnglishAPI.DTOs;
using LushEnglishAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LushEnglishAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController(LushEnglishDbContext context) : ControllerBase
{
   

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO? request)
    {
        //check request
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Invalid request, check again");

            //valid user
            var userGetByRequest = await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (userGetByRequest == null) return BadRequest("Wrong email or password, check again");

            // valid password
            if (userGetByRequest.Password != request.Password) return BadRequest("Invalid request, check again");
            var loginSession = Guid.CreateVersion7().ToString();
            userGetByRequest.LoginSession = loginSession;
            context.Users.Update(userGetByRequest);
            await context.SaveChangesAsync();

            var objectResult = new
            {
                userId = userGetByRequest.Id,
                sessionId = loginSession,
                fullName = userGetByRequest.FullName,
                avatarUrl = userGetByRequest.AvatarUrl,
                isAdmin = userGetByRequest.IsAdmin,
            };

            return Ok(objectResult);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO? request)
    {
        //check request
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest("Invalid request, check again");
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Password not equal to ConfirmPassword, check again");

            }
            //valid user
            var userGetByRequest = await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (userGetByRequest != null) return BadRequest("User exists, check again");

            var userCreate = new User()
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password,
                AvatarUrl = "",
                CreatedAt = DateTime.UtcNow.AddHours(7),
                LoginSession = Guid.CreateVersion7().ToString()
            };

            context.Users.Add(userCreate);
            await context.SaveChangesAsync();

            return Ok("Register successful");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "Internal server error");
        }
    }
}