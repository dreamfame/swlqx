using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SimHash
{
	public class OverlappingStringTokeniser : ITokeniser
{
           
    private readonly ushort chunkSize = 4;
    private readonly ushort overlapSize = 3;
 
    public OverlappingStringTokeniser(ushort chunkSize, ushort overlapSize)
    {
        if (chunkSize <= overlapSize)
        {
            throw new ArgumentException("Chunck 必须大于 overlap");
        }
        this.overlapSize = overlapSize;
        this.chunkSize = chunkSize;
    }
 
    public IEnumerable<string> Tokenise(string input)
    {
        var result = new List<string>();
        int position = 0;
        while (position < input.Length - this.chunkSize)
        {
            result.Add(input.Substring(position, this.chunkSize));
            position += this.chunkSize - this.overlapSize;
        }
        return result;
    }
 
 
}
}
