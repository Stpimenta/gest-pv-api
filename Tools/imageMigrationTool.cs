using System.Collections.Concurrent;
using c___Api_Example.Application.Services.Gdrive;
using c___Api_Example.data;
using c___Api_Example.Domain.Models;
using c___Api_Example.Models;
using Microsoft.EntityFrameworkCore;
using Minio;

namespace c___Api_Example.Tools;

public class imageMigrationTool
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GdriveService _gdriveService;
    private readonly MinioService _minioService;

    public imageMigrationTool(IbpvDataBaseContext dbContext, GdriveService gdriveService, MinioService minioService, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory =  scopeFactory;
        _gdriveService = gdriveService;
        _minioService = minioService;
    }


//
    
    public async Task<List<int>> RunGastoAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IbpvDataBaseContext>();

        var gastos = await db.Gasto
            .Where(g => g.UrlComprovante != null)
            .Select(g => new { g.Id, g.UrlComprovante })
            .ToListAsync();

        return await RunMigrationAsync(
            gastos,
            g => g.UrlComprovante!,
            (id, objectName, ctx) =>
            {
                ctx.ExpenseImages.Add(new ExpenseImageModel
                {
                    ExpenseId = id,
                    Url = objectName,
                    CreatedAt = DateTime.UtcNow
                });
            });
    }

    public async Task<List<int>> RunContribuicaoAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IbpvDataBaseContext>();

        var contribuicoes = await db.Contribuicao
            .Where(c => c.UrlEnvelope != null)
            .Select(c => new { c.Id, c.UrlEnvelope })
            .ToListAsync();

        return await RunMigrationAsync(
            contribuicoes,
            c => c.UrlEnvelope!,
            (id, objectName, ctx) =>
            {
                ctx.ContributionImages.Add(new ContributionImageModel
                {
                    ContributionId = id,
                    Url = objectName,
                    CreatedAt = DateTime.UtcNow
                });
            });
    }

    // === Método genérico para migração ===
    private async Task<List<int>> RunMigrationAsync<T>(
        List<T> items,
        Func<T, string> urlSelector,
        Action<int, string, IbpvDataBaseContext> addImageAction
    )
    {
        int maxDegreeOfParallelism = 30;
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
        var erros = new ConcurrentBag<int>();

        var tasks = items.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IbpvDataBaseContext>();

                var id = (int)item!.GetType().GetProperty("Id")!.GetValue(item)!;
                Console.WriteLine($"⬇ Baixando Id: {id}");

                var fileId = urlSelector(item);
                var (stream, mimeType) = await _gdriveService.DownloadAsync(fileId);

                if (stream.Length == 0)
                {
                    Console.WriteLine($"❌ Stream vazia | Id {id}");
                    erros.Add(id);
                    return;
                }

                var extension = mimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "application/pdf" => ".pdf",
                    _ => ".bin"
                };

                var objectName = $"{Guid.NewGuid()}{extension}";

                await _minioService.UploadAsync(stream, objectName, "financial");

           
                addImageAction(id, objectName, db);
                await db.SaveChangesAsync();

                Console.WriteLine($"⬆ Upload OK | {objectName}");
            }
            catch (Exception ex)
            {
                var id = (int)item!.GetType().GetProperty("Id")!.GetValue(item)!;
                Console.WriteLine($"❌ ERRO | Id {id} | {ex.Message}");
                erros.Add(id);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        Console.WriteLine("IDs com erro:");
        foreach (var id in erros)
            Console.WriteLine($" - {id}");

        return erros.ToList();
    }
    
}
    