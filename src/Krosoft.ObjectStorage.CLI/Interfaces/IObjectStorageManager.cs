namespace Krosoft.ObjectStorage.CLI.Interfaces;

internal interface IObjectStorageManager
{
    Task<int> Info(string profilePath);
    Task<int> Download(string profilePath, string path, string? outputPath, bool decodeBase64 = false, bool formatXml = false);
    Task<int> List(string profilePath, string path);
}
