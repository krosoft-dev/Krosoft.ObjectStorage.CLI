namespace Krosoft.ObjectStorage.CLI.Interfaces;

internal interface IObjectStorageManager
{
    Task<int> Info(string profilePath);
    Task<int> List(string profilePath, string path);
}
