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

    public async Task<int> Download(string profilePath, string path, string? outputPath, bool decodeBase64 = false, bool formatXml = false)
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

        // Parse path : "bucket/key"
        var slashIndex = path.IndexOf('/');
        if (slashIndex < 0)
        {
            return HandleError("Le chemin doit être au format 'bucket/chemin/du/fichier'.");
        }

        var bucketName = path[..slashIndex];
        var key = path[(slashIndex + 1)..];

        if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(key))
        {
            return HandleError("Le bucket ou la clé du fichier est vide.");
        }

        var fileName = Path.GetFileName(key);
        var destination = string.IsNullOrWhiteSpace(outputPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), fileName)
            : outputPath;

        var settings = profile.ObjectStorage;

        DisplayHeader($"TÉLÉCHARGEMENT — {path}");
        Console.WriteLine($"Destination : {destination}");
        Console.WriteLine();

        try
        {
            using var client = CreateClient(settings);

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await client.GetObjectAsync(request);

            var totalBytes = response.ContentLength;
            var destinationDir = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var sw = Stopwatch.StartNew();
            await using (var responseStream = response.ResponseStream)
            await using (var fileStream = File.Create(destination))
            {
                var buffer = new byte[81920];
                long bytesRead = 0;
                int read;

                while ((read = await responseStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    bytesRead += read;

                    if (totalBytes > 0)
                    {
                        var pct = (int)(bytesRead * 100 / totalBytes);
                        Console.Write($"\r  Progression : {pct,3}%  ({FormatSize(bytesRead)} / {FormatSize(totalBytes)})");
                    }
                }
            }

            sw.Stop();
            Console.WriteLine();
            WriteColoredLine(ConsoleColor.Green, $"\n  Fichier téléchargé en {sw.ElapsedMilliseconds:N0} ms → {destination}");

            if (decodeBase64)
            {
                Console.WriteLine("  Décodage Base64 en cours...");
                try
                {
                    var encoded = await File.ReadAllTextAsync(destination);
                    var decoded = Convert.FromBase64String(encoded.Trim());

                    // Strip .b64 extension si présente, sinon garde le même chemin
                    var decodedDestination = destination.EndsWith(".b64", StringComparison.OrdinalIgnoreCase)
                        ? destination[..^4]
                        : destination;

                    await File.WriteAllBytesAsync(decodedDestination, decoded);

                    // Supprime le fichier encodé si le chemin a changé
                    if (!string.Equals(destination, decodedDestination, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(destination);
                    }

                    WriteColoredLine(ConsoleColor.Green, $"  Décodé ({FormatSize(decoded.LongLength)}) → {decodedDestination}");

                    if (formatXml)
                    {
                        Console.WriteLine("  Formatage XML en cours...");
                        try
                        {
                            var rawXml = await File.ReadAllTextAsync(decodedDestination);
                            var doc = new System.Xml.XmlDocument();
                            doc.LoadXml(rawXml);

                            var xmlSettings = new System.Xml.XmlWriterSettings
                            {
                                Async = true,
                                Indent = true,
                                IndentChars = "  ",
                                NewLineChars = "\n",
                                Encoding = new System.Text.UTF8Encoding(false)
                            };

                            await using var writer = System.Xml.XmlWriter.Create(decodedDestination, xmlSettings);
                            doc.Save(writer);

                            var formattedSize = new FileInfo(decodedDestination).Length;
                            WriteColoredLine(ConsoleColor.Green, $"  XML formaté ({FormatSize(formattedSize)}) → {decodedDestination}");
                        }
                        catch (System.Xml.XmlException ex)
                        {
                            WriteColoredLine(ConsoleColor.Yellow, $"  Avertissement : le contenu décodé n'est pas un XML valide ({ex.Message}).");
                        }
                    }
                }
                catch (FormatException)
                {
                    return HandleError("Le contenu du fichier n'est pas un Base64 valide.");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            return HandleError($"Impossible de télécharger le fichier : {ex.Message}");
        }
    }

    public async Task<int> List(string profilePath, string path)
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

        // Parse path : "bucket" ou "bucket/prefix/..."
        var slashIndex = path.IndexOf('/');
        var bucketName = slashIndex >= 0 ? path[..slashIndex] : path;
        var prefix = slashIndex >= 0 ? path[(slashIndex + 1)..] : string.Empty;

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return HandleError("Le chemin doit contenir au minimum un nom de bucket.");
        }

        var settings = profile.ObjectStorage;
        var displayPath = string.IsNullOrEmpty(prefix) ? bucketName : $"{bucketName}/{prefix}";

        DisplayHeader($"LISTE DES FICHIERS — {displayPath}");

        try
        {
            using var client = CreateClient(settings);

            var objects = new List<S3Object>();
            string? continuationToken = null;

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                };

                var response = await client.ListObjectsV2Async(request);
                objects.AddRange(response.S3Objects);
                continuationToken = response.IsTruncated.HasValue && response.IsTruncated.Value ? response.NextContinuationToken : null;
            } while (continuationToken is not null);

            Console.WriteLine($"Bucket  : {bucketName}");
            Console.WriteLine($"Préfixe : {(string.IsNullOrEmpty(prefix) ? "(racine)" : prefix)}");
            Console.WriteLine($"Fichiers: {objects.Count}");
            Console.WriteLine();

            if (objects.Count > 0)
            {
                Console.WriteLine($"{"#",-5} {"Nom",-60} {"Taille",12}  Modifié");
                Console.WriteLine(new string('─', 100));

                var index = 1;
                foreach (var obj in objects)
                {
                    var name = string.IsNullOrEmpty(prefix) ? obj.Key : obj.Key[prefix.Length..];
                    var size = FormatSize(obj.Size);
                    var modified = obj.LastModified?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "—";
                    Console.WriteLine($"{index,-5} {name,-60} {size,12}  {modified}");
                    index++;
                }
            }
            else
            {
                WriteColoredLine(ConsoleColor.Yellow, "Aucun fichier trouvé pour ce chemin.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            return HandleError($"Impossible de lister les fichiers : {ex.Message}");
        }
    }

    private static string FormatSize(long? bytes) => bytes switch
    {
        null => "—",
        < 1024 => $"{bytes.Value} B",
        < 1024 * 1024 => $"{bytes.Value / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes.Value / (1024.0 * 1024):F1} MB",
        _ => $"{bytes.Value / (1024.0 * 1024 * 1024):F2} GB"
    };

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