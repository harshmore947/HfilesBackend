using Microsoft.AspNetCore.Mvc;

namespace HFilesBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class HealthController : ControllerBase
  {
    [HttpGet]
    public IActionResult Get()
    {
      return Ok(new
      {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        version = "1.0.0"
      });
    }
  }
}