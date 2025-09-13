namespace HFilesBackend.DTOs
{
  public record RegisterDto(string FullName, string Email, string phone, string Password, string Gender, string? ProfileImageUrl = null);
}