using System.Security.Cryptography;
using System.Text.Json;
using DotNut;
using DotNut.Abstractions;
using DotNut.Api;

namespace ConsoleApp6;

class Program
{
    static async Task Main(string[] args)
    { 
        string MintUrl = "http://localhost:3338/";
        
        var wallet = Wallet
            .Create()
            .WithMint(MintUrl);
         
        var privKeyBob = new PrivKey("9fc13272461bc2ea1b42d141119db1930e466ffcfe6d2528d7ca15827f58c9b8");
        var pubkeyBob = privKeyBob.Key.CreatePubKey();
        
        var preimage = "0000000000000000000000000000000000000000000000000000000000000001";
        var hashLock = Convert.ToHexString(SHA256.HashData(Convert.FromHexString(preimage))).ToLowerInvariant();
        var builder = new HTLCBuilder()
        {
            HashLock = hashLock,
            Pubkeys = [pubkeyBob]
        };
        
        var quote = await wallet.CreateMintQuote()
            .WithAmount(4)
            .WithHTLCLock(builder)
            .ProcessAsyncBolt11();
        
        // pay invoice by sleeping
        await Task.Delay(3500);
        
        // mint the quote
        var htlcProofs = await quote.Mint();
        
        // here's a htlc bug. why????????
        var proofs = await wallet.Swap()
            .FromInputs(htlcProofs)
            .WithPrivkeys([privKeyBob])
            .WithHtlcPreimage(preimage)
            .ProcessAsync();
            
        
       var token =  CashuTokenHelper.Encode(new CashuToken()
        {
            Tokens = new List<CashuToken.Token>()
            { 
                new CashuToken.Token()
                {
                    Proofs = htlcProofs,
                    Mint = "https://testnut.cashu.space/",
                },
            },
            Unit = "sat"
        });
       
        Console.WriteLine(token);
    }
}