using Minio.DataModel.Args;

namespace c___Api_Example.Application.Services.Gdrive;
using Minio;
using Minio.DataModel;
using System.IO;


public class MinioService
{
    private readonly IMinioClient _client;

    public MinioService(IMinioClient client)
    {
        _client = client;
    }
    
    private string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".jpn" => "image/jpn",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    public async Task UploadAsync(MemoryStream stream, string objectName, string bucket)
    {
        bool found = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket)
        );
        
        
        if (!found)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket)
            );
        }

        stream.Position = 0;
        
        //get type
        var extension = Path.GetExtension(objectName);
        var contentType = GetContentTypeFromExtension(extension);
        
        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType)
        );

      
    }

    public async Task DeleteAsync(string objectName, string bucket)
    {
        bool found = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket)
        );
        if (!found)
            throw new InvalidOperationException($"Bucket '{bucket}' not found.");
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
        );
    }
    
    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiresInSeconds = 60)
    {
        var url = await _client.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expiresInSeconds) 
        );

        return url;
    }
    
   
}



