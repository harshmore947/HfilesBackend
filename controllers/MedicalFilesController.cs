using Microsoft.AspNetCore.Mvc;
using HFilesBackend.Data;
using HFilesBackend.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Linq;

namespace HFilesBackend.Controllers
{
  [ApiController]
  [Route("api/medical-files")]
  public class MedicalFilesController : ControllerBase
  {
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly BlobServiceClient _blobServiceClient;

    public MedicalFilesController(AppDbContext db, IWebHostEnvironment env, BlobServiceClient blobServiceClient)
    {
      _db = db;
      _env = env;
      _blobServiceClient = blobServiceClient;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedicalFile([FromForm] string fileType, [FromForm] string fileName, [FromForm] IFormFile file)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      if (string.IsNullOrEmpty(fileType) || string.IsNullOrEmpty(fileName) || file == null)
        return BadRequest("File type, name, and file are required");

      var allowedTypes = new[] { "Lab Report", "Prescription", "X-Ray", "Blood Report", "MRI Scan", "CT Scan" };
      if (!allowedTypes.Contains(fileType)) return BadRequest("Invalid file type");

      var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };
      var extension = Path.GetExtension(file.FileName).ToLower();
      if (!allowedExtensions.Contains(extension)) return BadRequest("Invalid file format. Only PDFs and images allowed");

      var containerClient = _blobServiceClient.GetBlobContainerClient("hfilestest");
      await containerClient.CreateIfNotExistsAsync();

      var blobName = $"medical/{userId}_{Guid.NewGuid()}{extension}";
      var blobClient = containerClient.GetBlobClient(blobName);

      using (var stream = file.OpenReadStream())
      {
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
      }

      // var uniqueFileName = $"{userId}_{Guid.NewGuid()}{extension}";

      // var filePath = Path.Combine(_env.WebRootPath, "files", "medical", uniqueFileName);

      // using (var stream = new FileStream(filePath, FileMode.Create))
      // {
      //   await file.CopyToAsync(stream);
      // }

      var fileRecord = new FileRecord
      {
        OwnerUserId = userId.Value,
        FileName = fileName,
        FileType = fileType,
        BlobUrl = blobClient.Uri.ToString(),
        UploadedAt = DateTime.UtcNow
      };

      _db.FileRecords.Add(fileRecord);
      await _db.SaveChangesAsync();

      return Ok(new { message = "File uploaded successfully", fileId = fileRecord.Id });
    }

    [HttpGet]
    public IActionResult GetMedicalFiles()
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      var files = _db.FileRecords
        .Where(f => f.OwnerUserId == userId.Value)
        .OrderByDescending(f => f.UploadedAt)
        .Select(f => new
        {
          f.Id,
          f.FileName,
          f.FileType,
          f.BlobUrl,
          f.UploadedAt
        })
        .ToList();

      return Ok(files);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicalFile(int id)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null) return Unauthorized();

      var fileRecord = await _db.FileRecords.FindAsync(id);
      if (fileRecord == null || fileRecord.OwnerUserId != userId.Value)
        return NotFound();

      var containerClient = _blobServiceClient.GetBlobContainerClient("hfilestest");
      var blobName = fileRecord.BlobUrl.Split('/').Last();
      var blobClient = containerClient.GetBlobClient(blobName);
      await blobClient.DeleteIfExistsAsync();

      // Delete physical file
      // var filePath = Path.Combine(_env.WebRootPath, fileRecord.BlobUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
      // if (System.IO.File.Exists(filePath))
      // {
      //   System.IO.File.Delete(filePath);
      // }

      _db.FileRecords.Remove(fileRecord);
      await _db.SaveChangesAsync();

      return Ok(new { message = "File deleted successfully" });
    }
  }
}