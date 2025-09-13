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
      if (await _db.Users.AnyAsync(user => user.Email == dto.Email))
      {
        return BadRequest(new { error = "Email already exists" });//400
      }

      var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
      var user = new User
      {
        FullName = dto.FullName,
        Email = dto.Email,
        Phone = dto.phone,
        PasswordHash = hashed,
        Gender = dto.Gender,
        ProfileImageUrl = dto.ProfileImageUrl ?? "https://via.placeholder.com/150x150/cccccc/666666?text=User"
      };

      _db.Users.Add(user);
      await _db.SaveChangesAsync();
      return Ok(new { message = "Registered" }); //200
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