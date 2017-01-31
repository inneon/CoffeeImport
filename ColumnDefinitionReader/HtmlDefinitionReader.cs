using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace CommunityCoffeeImport.ColumnDefinitionReader
{
	class HtmlDefinitionReader : IDefinitionReader
	{
		public List<ColumnDefinition> LoadFromContent(string content)
		{
			List<ColumnDefinition> result = new List<ColumnDefinition>();
			Regex decimalMatcher = new Regex("(\\d+)\\((\\d+)\\)");

			XmlDocument document = new XmlDocument();
			document.LoadXml(content);
			foreach (XmlNode column in document["tbody"].ChildNodes) {
				XmlElement columnName = column["td"]["a"];
				if (columnName != null) {
					bool keyColumn = column.Attributes["class"].InnerText == "keyField";
					string decimalsAndLength = column.ChildNodes[3]?.InnerText ?? "";
					Match match = decimalMatcher.Match(decimalsAndLength);
					string length = decimalsAndLength;
					string decimals = "";
					if (match.Success) {
						length = match.Groups[1].Value;
						decimals = match.Groups[2].Value;
					}
					ColumnDefinition definition = new ColumnDefinition {
						Field = columnName.InnerText,
						DataElement = column.ChildNodes[1]["a"].InnerText,
						DataType = column.ChildNodes[2].InnerText,
						IsKey = keyColumn,
						Decimals = string.IsNullOrEmpty(decimals) ? 0 : int.Parse(decimals),
						Length = int.Parse(length)
					};
					result.Add(definition);
				}
			}

			return result;
		} 
	}
}
