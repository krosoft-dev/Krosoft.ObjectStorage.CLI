using System.Text.Json.Serialization;

namespace Krosoft.ObjectStorage.CLI.Models;

internal record Profile(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("objectStorage")]
    ObjectStorageProfile? ObjectStorage = null);