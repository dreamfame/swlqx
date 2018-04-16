using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SimHash
{
	public class FixedSizeStringTokeniser : ITokeniser
{
    private readonly ushort tokensize = 5;
    public FixedSizeStringTokeniser(ushort tokenSize)
    {
        if (tokenSize < 2 || tokenSize > 127)
        {
            throw new ArgumentException("Token 不能超出范围");
        }
        this.tokensize = tokenSize;
    }
 
    public IEnumerable<string> Tokenise(string input)
    {
        var chunks = new List<string>();
        int offset = 0;
        while (offset < input.Length)
        {
            chunks.Add(new string(input.Skip(offset).Take(this.tokensize).ToArray()));
            offset += this.tokensize;
        }
        return chunks;
    }
 
}

}
