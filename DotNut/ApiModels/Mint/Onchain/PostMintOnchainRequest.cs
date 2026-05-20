using System.Text.Json.Serialization;

namespace DotNut.ApiModels.Onchain;

public class PostMintOnchainRequest
{
// "quote": <str>,
// "outputs": <Array[BlindedMessage]>,
// "signature": <str>
    [JsonPropertyName("quote")]
    public string Quote { get; set; }
    
    [JsonPropertyName("outputs")]
    public BlindedMessage[] Outputs { get; set; }
    
    [JsonPropertyName("signature")]
    public string Signture { get; set; }
}