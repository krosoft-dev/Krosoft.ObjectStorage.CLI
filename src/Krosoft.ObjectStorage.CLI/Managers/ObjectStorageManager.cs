using System.Diagnostics;
using Amazon.S3;
using Amazon.S3.Model;
using Krosoft.ObjectStorage.CLI.Helpers;
using Krosoft.ObjectStorage.CLI.Interfaces;
using Krosoft.ObjectStorage.CLI.Models;

namespace Krosoft.ObjectStorage.CLI.Managers;

internal class ObjectStorageManager : IObjectStorageManager
{
    public async Task<int> Info(string profilePath)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return HandleError(error!);
        }

        if (profile.ObjectStorage is null)
        {
            return HandleError("Le profil ne contient pas de section 'objectStorage'.");
        }

        var settings = profile.ObjectStorage;

        DisplayHeader($"INFORMATIONS OBJECT STORAGE — {profile.Name}");

        try
        {
            using var client = CreateClient(settings);

            var sw = Stopwatch.StartNew();
            var response = await client.ListBucketsAsync(new ListBucketsRequest());
            sw.Stop();

            var buckets = response.Buckets ?? [];

            Console.WriteLine($"Endpoint         : {settings.Endpoint}");
            Console.WriteLine($"SSL              : {settings.UseSSL}");
            Console.WriteLine($"Timeout          : {settings.Timeout}s");
            Console.WriteLine($"Buckets          : {buckets.Count}");
            Console.WriteLine($"Temps de réponse : {sw.ElapsedMilliseconds:N0} ms");

            if (buckets.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{"#",-4} {"Bucket",-40} Création");
                Console.WriteLine(new string('─', 80));

                var index = 1;
                foreach (var bucket in buckets)
                {
                    var created = bucket.CreationDate is { } date
                        ? date.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                        : "—";
                    Console.WriteLine($"{index,-4} {bucket.BucketName,-40} {created}");
                    index++;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            return HandleError($"Impossible de récupérer les informations de l'object storage : {ex.Message}");
        }
    }

    public Task<int> List(string profilePath, string path) => throw new NotImplementedException();

    // Client S3 compatible MinIO : path-style forcé et schéma dérivé du profil.
    private static IAmazonS3 CreateClient(ObjectStorageProfile settings)
    {
        if (string.IsNullOrEmpty(settings.Endpoint))
        {
            throw new InvalidOperationException("'endpoint' non renseigné dans le profil.");
        }

        var config = new AmazonS3Config
        {
            ForcePathStyle = true,
            Timeout = TimeSpan.FromSeconds(settings.Timeout),
            MaxErrorRetry = 3
        };

        var scheme = settings.UseSSL ? "https" : "http";
        config.ServiceURL = settings.Endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? settings.Endpoint
            : $"{scheme}://{settings.Endpoint}";

        return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
    }

    private static void DisplayHeader(string title)
    {
        const int totalWidth = 100;
        var paddingWidth = (totalWidth - title.Length) / 2;
        var padding = new string(' ', paddingWidth);
        var border = new string('═', totalWidth);

        WriteColoredLine(ConsoleColor.Green, $"╔{border}╗");
        WriteColoredLine(ConsoleColor.Green, title.Length % 2 != 0
                             ? $"║{padding} {title}{padding}║"
                             : $"║{padding}{title}{padding}║");
        WriteColoredLine(ConsoleColor.Green, $"╚{border}╝\n");
    }

    private static void WriteColoredLine(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static int HandleError(string message)
    {
        WriteColoredLine(ConsoleColor.Red, message);
        return 1;
    }
}