using HFilesBackend.Data;
using Microsoft.AspNetCore.Mvc;
using HFilesBackend.DTOs;
using HFilesBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HFilesBackend.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase
  {
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
      _db = db;
    }

    //Register User
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      try
      {
        // Test database connection first
        try
        {
          await _db.Database.CanConnectAsync();
        }
        catch (Exception dbEx)
        {
          return StatusCode(500, new { error = "Database connection failed", details = dbEx.Message });
        }

        if (await _db.Users.AnyAsync(user => user.Email == dto.Email))
        {
          return BadRequest(new { error = "Email already exists" });//400
        }

        // Normalize gender input
        var normalizedGender = string.IsNullOrEmpty(dto.Gender) ? "Male" :
                              char.ToUpper(dto.Gender[0]) + dto.Gender.Substring(1).ToLower();

        var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new User
        {
          FullName = dto.FullName,
          Email = dto.Email,
          Phone = dto.phone,
          PasswordHash = hashed,
          Gender = normalizedGender,
          ProfileImageUrl = dto.ProfileImageUrl ?? "https://via.placeholder.com/150x150/cccccc/666666?text=User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registered" }); //200
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = "Registration failed", details = ex.Message });
      }
    }

    [HttpPost("login")]
    //login user
    public Task<IActionResult> Login(LoginDto dto)
    {
      var user = _db.Users.FirstOrDefault(user => user.Email == dto.Email);
      if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
      {
        return Task.FromResult<IActionResult>(Unauthorized(new { error = "Invalid credentials" }));//401
      }

      HttpContext.Session.SetInt32("UserId", user.Id);

      return Task.FromResult<IActionResult>(Ok(new { message = "Logged in" }));//200
    }

    [HttpPost("logout")]
    //user log out
    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      return Ok(new { message = "Logged out" });//200
    }
  }
}