using Microsoft.AspNetCore.Mvc;

namespace HFilesBackend.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class HealthController : ControllerBase
  {
    private readonly IConfiguration _configuration;

    public HealthController(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
      var dbConnection = _configuration.GetConnectionString("DefaultConnection");
      var azureConnection = _configuration["AzureStorage:ConnectionString"];
      var azureContainer = _configuration["AzureStorage:ContainerName"];

      return Ok(new
      {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        version = "1.0.0",
        configuration = new
        {
          hasDbConnection = !string.IsNullOrEmpty(dbConnection),
          dbConnectionLength = dbConnection?.Length ?? 0,
          hasAzureConnection = !string.IsNullOrEmpty(azureConnection),
          azureConnectionLength = azureConnection?.Length ?? 0,
          azureContainer = azureContainer,
          // First few characters for debugging (safe)
          dbPrefix = dbConnection?.Substring(0, Math.Min(20, dbConnection.Length)) ?? "null",
          azurePrefix = azureConnection?.Substring(0, Math.Min(20, azureConnection.Length)) ?? "null"
        }
      });
    }
  }
}