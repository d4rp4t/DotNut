using System.Text.Json.Serialization;

namespace DotNut.ApiModels.Onchain;

public class PostMintQuoteOnchainResponse
{
    [JsonPropertyName("quote")]
    public string Quote { get; set; }
    
    [JsonPropertyName("request")]
    public string Request { get; set; }
    
    [JsonPropertyName("unit")]
    public string Unit { get; set; }
    
    [JsonPropertyName("expiry")]
    public ulong? Expiry { get; set; }
    
    [JsonPropertyName("pubkey")]
    public string PubKey { get; set; }
    
    [JsonPropertyName("amount_paid")]
    public ulong AmountPaid { get; set; }
    
    [JsonPropertyName("amount_issued")]
    public ulong AmountIssued { get; set; }
}