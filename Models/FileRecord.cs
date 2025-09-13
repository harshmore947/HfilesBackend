using Azure.Storage.Blobs.Models;

namespace HFilesBackend.Models
{
  public class FileRecord
  {
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = "";
    public string BlobUrl { get; set; } = "";
    public DateTime UploadedAt { get; set; }
  }
}