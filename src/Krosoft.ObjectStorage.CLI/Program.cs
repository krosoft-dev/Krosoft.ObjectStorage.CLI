using CommandLine;

namespace Krosoft.ObjectStorage.CLI;

internal static class Program
{
    private static async Task<int> Main(params string[] args)
    {
        PrintBanner();
        return await Parser.Default.ParseArguments<Options.InfoOptions,
                               Options.ListOptions>(args)
                           .MapResult(
                                      (Options.InfoOptions opts) => ProgramObjectStorage.Info(opts),
                                      (Options.ListOptions opts) => ProgramObjectStorage.List(opts),
                                      _ => Task.FromResult(-1));
    }

    private static void PrintBanner()
    {
        const string banner = """

                                _  __                     __ _   
                               | |/ /                    / _| |  
                               | ' / _ __ ___  ___  ___ | |_| |_ 
                               |  < | '__/ _ \/ __|/ _ \|  _| __|
                               | . \| | | (_) \__ \ (_) | | | |_ 
                               |_|\_\_|  \___/|___/\___/|_|  \__| 
                               
                               Object Storage CLI Tool

                              """;
        Console.WriteLine(banner);
    }
}