using Krosoft.ObjectStorage.CLI.Interfaces;
using Krosoft.ObjectStorage.CLI.Managers;

namespace Krosoft.ObjectStorage.CLI;

internal static class ProgramObjectStorage
{
    public static Task<int> Info(Options.InfoOptions opts) =>
        GetManager().Info(opts.Profile);

    public static Task<int> List(Options.ListOptions opts)
        => GetManager().List(opts.Profile, opts.Path);

    private static IObjectStorageManager GetManager() => new ObjectStorageManager();
}