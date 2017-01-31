using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommunityCoffeeImport.ColumnDefinitionReader
{
	class RegexDefinitionReader : IDefinitionReader
	{
		Regex parser = new Regex("\\s*([A-Z0-9_]+)\\s+(X?)\\s+([A-Z]+)\\s+(\\d+)\\s+([A-Z0-9_]+)");

		public List<ColumnDefinition> LoadFromContent(string content)
		{
			List<ColumnDefinition> result = new List<ColumnDefinition>();
			foreach (Match match in parser.Matches(content)) {
				ColumnDefinition toAdd = new ColumnDefinition {
					Field = match.Groups[1].Value,
					IsKey = match.Groups[2].Value != string.Empty,
					DataType = match.Groups[3].Value,
					Length = int.Parse(match.Groups[4].Value),
					DataElement = match.Groups[5].Value
				};
				result.Add(toAdd);
			}
			return result;
		}
	}
}