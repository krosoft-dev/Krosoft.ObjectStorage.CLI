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

    [Verb("download", HelpText = "Télécharge un fichier depuis le stockage objet.")]
    internal class DownloadOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;

        [Option('d', "path", Required = true, HelpText = "Chemin du fichier à télécharger (bucket/clé).")]
        public string Path { get; set; } = string.Empty;

        [Option('o', "output", Required = false, HelpText = "Chemin local de destination (optionnel, défaut : répertoire courant).")]
        public string? Output { get; set; }

        [Option('b', "decode-base64", Required = false, Default = false, HelpText = "Décode le contenu du fichier depuis Base64 après téléchargement.")]
        public bool DecodeBase64 { get; set; }

        [Option('x', "format-xml", Required = false, Default = false, HelpText = "Formate le contenu en XML indenté après décodage Base64 (nécessite --decode-base64).")]
        public bool FormatXml { get; set; }
    }

    [Verb("upload", HelpText = "Envoie un fichier local vers le stockage objet.")]
    internal class UploadOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;

        [Option('d', "path", Required = true, HelpText = "Destination distante (bucket/clé).")]
        public string Path { get; set; } = string.Empty;

        [Option('i', "input", Required = true, HelpText = "Chemin du fichier local à envoyer.")]
        public string Input { get; set; } = string.Empty;

        [Option('x', "format-xml", Required = false, Default = false, HelpText = "Normalise le contenu XML sur une seule ligne (inline) avant l'envoi.")]
        public bool FormatXml { get; set; }

        [Option('b', "encode-base64", Required = false, Default = false, HelpText = "Encode le contenu en Base64 avant l'envoi (appliqué après --format-xml).")]
        public bool EncodeBase64 { get; set; }
    }
}