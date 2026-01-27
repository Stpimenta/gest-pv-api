namespace c___Api_Example.Application.Services.Gdrive;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

public class GdriveService
{
    private readonly DriveService _drive;
    
    
    public GdriveService()
    {
        var credential = GoogleCredential
            .FromFile("googleDriveToken/modern-optics-427420-b0-f6ced981b26b.json")
            .CreateScoped(DriveService.Scope.DriveReadonly);

        _drive = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ImageMigration"
        });
    }
    
    public async Task<(MemoryStream stream, string mimeType)> DownloadAsync(string fileId)
    {
        // pega metadata do arquivo
        var file = await _drive.Files.Get(fileId).ExecuteAsync();
    
        var stream = new MemoryStream();
        await _drive.Files.Get(fileId).DownloadAsync(stream);
        stream.Position = 0;

        return (stream, file.MimeType); // retorna o MIME type
    }
    
}