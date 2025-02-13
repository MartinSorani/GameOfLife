using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string TempFilePath { get; private set; } = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Generate a unique temporary file path.
        TempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.json");

        var inMemorySettings = new Dictionary<string, string?>
        {
            // Assuming your repository uses this configuration key.
            { "FileBoardRepository:FilePath", TempFilePath }
        };

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(inMemorySettings);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && !string.IsNullOrEmpty(TempFilePath) && File.Exists(TempFilePath))
        {
            File.Delete(TempFilePath);
        }
    }
}
