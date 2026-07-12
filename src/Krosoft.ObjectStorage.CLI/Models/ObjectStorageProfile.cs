using System.Text.Json.Serialization;

namespace Krosoft.ObjectStorage.CLI.Models;

internal record ObjectStorageProfile(
    [property: JsonPropertyName("endpoint")]
    string Endpoint,
    [property: JsonPropertyName("accessKey")]
    string AccessKey,
    [property: JsonPropertyName("secretKey")]
    string SecretKey,
    [property: JsonPropertyName("useSSL")] bool UseSSL = false,
    [property: JsonPropertyName("timeout")]
    int Timeout = 10);