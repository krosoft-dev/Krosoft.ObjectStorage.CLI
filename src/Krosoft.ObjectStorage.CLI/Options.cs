using CommandLine;

namespace Krosoft.ObjectStorage.CLI;

internal static class Options
{
    [Verb("info", HelpText = "Affiche les informations du stockage objet (S3/MinIO).")]
    internal class InfoOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;
    }

    [Verb("list", HelpText = "Liste les fichiers.")]
    internal class ListOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;

        [Option('d', "path", Required = true, HelpText = "Nom de la file à parcourir.")]
        public string Path { get; set; } = string.Empty;
    }
}