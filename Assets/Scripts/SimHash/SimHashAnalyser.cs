using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SimHash
{
	
public class SimHashAnalyser : IAnalyser
{
 
    private const int HashSize = 32;
 
    public float GetLikenessValue(string needle, string haystack)
    {
        var needleSimHash = this.DoCalculateSimHash(needle);
        var hayStackSimHash = this.DoCalculateSimHash(haystack);
        return (HashSize - GetHammingDistance(needleSimHash, hayStackSimHash)) / (float)HashSize;
    }
 
    private static IEnumerable<int> DoHashTokens(IEnumerable<string> tokens)
    {
        var hashedTokens = new List<int>();
        foreach (string token in tokens)
        {
            hashedTokens.Add(token.GetHashCode());
        }
        return hashedTokens;
    }
 
    private static int GetHammingDistance(int firstValue, int secondValue)
    {
        var hammingBits = firstValue ^ secondValue;
        var hammingValue = 0;
        for (int i = 0; i < 32; i++)
        {
            if (IsBitSet(hammingBits, i))
            {
                hammingValue += 1;
            }
        }
        return hammingValue;
    }
 
    private static bool IsBitSet(int b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
 
    private int DoCalculateSimHash(string input)
    {
        ITokeniser tokeniser = new OverlappingStringTokeniser(4, 3);
        var hashedtokens = DoHashTokens(tokeniser.Tokenise(input));
        var vector = new int[HashSize];
        for (var i = 0; i < HashSize; i++)
        {
            vector[i] = 0;
        }
 
        foreach (var value in hashedtokens)
        {
            for (var j = 0; j < HashSize; j++)
            {
                if (IsBitSet(value, j))
                {
                    vector[j] += 1;
                }
                else
                {
                    vector[j] -= 1;
                }
            }
        }
 
        var fingerprint = 0;
        for (var i = 0; i < HashSize; i++)
        {
            if (vector[i] > 0)
            {
                fingerprint += 1 << i;
            }
        }
        return fingerprint;
    }
 
 
}
}
