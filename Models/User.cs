using System.ComponentModel.DataAnnotations;

namespace HFilesBackend.Models
{
  public class User
  {
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = "";
    [Required] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    [Required] public string PasswordHash { get; set; } = "";
    [Required]
    [RegularExpression("(?i)^(Male|Female)$", ErrorMessage = "Gender must be 'Male' or 'Female'.")]
    public string Gender { get; set; } = "Male";
    public string ProfileImageUrl { get; set; } = "https://via.placeholder.com/150x150/cccccc/666666?text=User";
  }
}