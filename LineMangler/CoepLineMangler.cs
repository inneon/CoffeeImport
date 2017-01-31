using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityCoffeeImport.LineMangler
{
	public class CoepLineMangler : ILineMangler {
		public string Mangle(string line)
		{
			if (line.Contains("\"\"\"")) {
				line = line.Replace("\"\"\"", "\"\"");
			}
			return $"{line},\" \"";
		}
	}
}