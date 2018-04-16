using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SimHash
{
	public interface IAnalyser
	{
        float GetLikenessValue(string needle, string haystack);
	}
}
