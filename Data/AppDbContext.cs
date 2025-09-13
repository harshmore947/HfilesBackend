using HFilesBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace HFilesBackend.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<FileRecord> FileRecords{ get; set; }
  }
}