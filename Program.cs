using HFilesBackend.Data;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add local settings file for development
if (builder.Environment.IsDevelopment())
{
  builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Azure
var azureConnectionString = builder.Configuration["AzureStorage:ConnectionString"];
if (!string.IsNullOrEmpty(azureConnectionString))
{
  builder.Services.AddSingleton(new BlobServiceClient(azureConnectionString));
}

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
  opt.Cookie.HttpOnly = true;
  opt.Cookie.IsEssential = true;
  opt.IdleTimeout = TimeSpan.FromHours(8);
  if (builder.Environment.IsProduction())
  {
    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
    opt.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
  }
  else
  {
    opt.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
  }
});

builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(corsBuilder =>
  {
    corsBuilder.WithOrigins("http://localhost:3000", "https://localhost:3000")
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials();
  });
});

var app = builder.Build();

// Add error handling for production
if (app.Environment.IsProduction())
{
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}
else
{
  app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();  // Add this to serve static files
if (!app.Environment.IsDevelopment())
{
  app.UseHttpsRedirection();
}
app.MapGet("/", () => "Hello from Medical Records API!");

app.UseCors();  // Enable CORS
app.UseRouting();
app.UseSession();
app.MapControllers();

app.Run();
