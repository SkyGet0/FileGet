var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Row matters!
app.UseDefaultFiles();     // / → /index.html
app.UseStaticFiles();      // wwwroot files

app.MapPost("/upload", async (IFormFileCollection files, IConfiguration config) =>
{
    var uploadPath = config["TargetFolder"] ?? @"C:\FileGet\Files";
    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

    foreach (var file in files)
    {
        var filePath = Path.Combine(uploadPath, file.FileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
    }
    return Results.Ok(new { message = "Файлы успешно отправлены" });
}).DisableAntiforgery();

app.Run("http://localhost:5233");