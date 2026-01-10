var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseStaticFiles(); // для index.html

app.MapPost("/upload", async (IFormFileCollection files, IConfiguration config) =>
{
    var uploadPath = config["TargetFolder"] ?? Path.Combine(@"C:\FileGet", "Files");

    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

    foreach (var file in files)
    {
        var filePath = Path.Combine(uploadPath, file.Name);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
    }

    return Results.Ok(new { message = "Файлы успешно отправлены" });
}).DisableAntiforgery();

app.Run();  