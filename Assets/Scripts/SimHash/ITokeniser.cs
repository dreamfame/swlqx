using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.SimHash
{
	public interface ITokeniser
	{
        IEnumerable<string> Tokenise(string input);
	}
}
