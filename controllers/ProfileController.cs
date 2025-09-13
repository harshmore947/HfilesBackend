using Microsoft.AspNetCore.Mvc;
using HFilesBackend.Data;
using HFilesBackend.DTOs;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;

namespace HFilesBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProfileController : ControllerBase
  {
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly BlobServiceClient _blobServiceClient;

    public ProfileController(AppDbContext db, IWebHostEnvironment env, BlobServiceClient blobServiceClient)
    {
      _db = db;
      _env = env;
      _blobServiceClient = blobServiceClient;
    }

    [HttpGet]
    public IActionResult GetProfile()
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      var user = _db.Users.Find(userId.Value);
      if (user == null) return NotFound();

      return Ok(new { user.Id, user.FullName, user.Email, user.Phone, user.Gender, user.ProfileImageUrl });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      var user = await _db.Users.FindAsync(userId.Value);
      if (user == null) return NotFound();

      if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
      if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;
      if (!string.IsNullOrEmpty(dto.Gender) && (dto.Gender == "Male" || dto.Gender == "Female")) user.Gender = dto.Gender;
      if (!string.IsNullOrEmpty(dto.ProfileImageUrl)) user.ProfileImageUrl = dto.ProfileImageUrl;

      await _db.SaveChangesAsync();
      return Ok(new { message = "Profile updated successfully" });
    }

    [HttpPost("upload-profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      var user = await _db.Users.FindAsync(userId.Value);
      if (user == null) return NotFound();

      if (file == null || file.Length == 0) return BadRequest("No file uploaded");

      // Validate file type (images only)
      var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
      var extension = Path.GetExtension(file.FileName).ToLower();
      if (!allowedExtensions.Contains(extension)) return BadRequest("Invalid file type");

      // Create unique filename
      var containerClient = _blobServiceClient.GetBlobContainerClient("hfilestest");
      await containerClient.CreateIfNotExistsAsync();

      var blobName = $"profiles/{userId}_{Guid.NewGuid()}{extension}";
      var blobClient = containerClient.GetBlobClient(blobName);

      using (var stream = file.OpenReadStream())
      {
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
      }

      // Update user's profile image URL
      user.ProfileImageUrl = blobClient.Uri.ToString();
      await _db.SaveChangesAsync();

      return Ok(new { message = "Profile image uploaded", imageUrl = user.ProfileImageUrl });
    }
  }
}