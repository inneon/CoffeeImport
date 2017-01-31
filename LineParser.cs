using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.String;

namespace CommunityCoffeeImport
{
	class LineParser
	{
		public enum ParseOptions
		{
			RemoveQuotes,
			KeepQuotes
		}

		public static IEnumerable<string> Parse(string line, ParseOptions options)
		{
			char[] cs = line.ToCharArray();
			char? delimiter = null;
			string currentWord = Empty;

			for (int i = 0; i < cs.Length; i++) {
				char c = cs[i];
				if (delimiter == null) {
					if (c == '"') {
						delimiter = '"';
					} else {
						delimiter = ',';
						currentWord = c.ToString();
					}
				} else if (c == delimiter.Value) {
					if (delimiter.Value == '"' && options == ParseOptions.KeepQuotes) {
						currentWord = $"\"{currentWord}\"";
					}
					yield return currentWord;
					currentWord = Empty;
					if (delimiter.Value == '"') {
						i++;
					}
					delimiter = null;
				} else {
					currentWord += c;
				}
			}

			if (!IsNullOrEmpty(currentWord)) {
				if (delimiter.HasValue && delimiter.Value == '"' && options == ParseOptions.KeepQuotes) {
					currentWord = $"\"{currentWord}\"";
				}
				yield return currentWord;
			}
		}
	}
}
