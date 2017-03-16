using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Int32;

namespace CommunityCoffeeImport.ColumnDefinitionReader
{
	internal class Db2LookReader : IDefinitionReader
	{
		public List<ColumnDefinition> LoadFromContent(string content)
		{
			List<ColumnDefinition> result = ColumnDefinitions(content).ToList();
			return result;
		}

		private static IEnumerable<ColumnDefinition> ColumnDefinitions(string content)
		{
			Regex createFinder = new Regex("CREATE TABLE [^\\(]+\\(([^;]+);", RegexOptions.Multiline);
			Regex columnDefitionMatcher = new Regex("^\\s*\"([^\\\"]+)\" (\\w+)(\\([^\\)]+\\))?");
			Match match;
			Capture capture;

			List<string> primaryKeyColumns = FindPrimaryKeyColumns(content);

			match = createFinder.Match(content);
			capture = match.Groups[1];

			foreach (string columnCreateDefinition in capture.Value.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)) {
				match = columnDefitionMatcher.Match(columnCreateDefinition);
				if (match.Success) {
					string columnName = match.Groups[1].Value;
					string columnType = match.Groups[2].Value;
					int length = 8;
					int decimals = 0;
					if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[3].Value)) {
						// something of the form "(10 OCTETS)" or "(8,2)"
						string lengthText = match.Groups[3].Value.Trim(new[] {'(', ')'});
						string[] lengthComponents = lengthText.Split(new[] {' ', ','});
						length = Parse(lengthComponents[0]);
						if (lengthText.Contains(",")) {
							decimals = Parse(lengthComponents[1]);
						}
					}

					ColumnDefinition result = new ColumnDefinition {
						Field = columnName,
						DataType = GuessDataElement(columnType),
						Length = length,
						Decimals = decimals,
						IsKey = primaryKeyColumns.Contains(columnName),
						DataElement = ""
					};

					yield return result;
				}
			}
		}

		private static string GuessDataElement(string columnType)
		{
			switch (columnType.ToUpper()) {
				case "VARCHAR":
					return "CHAR";
				case "INTEGER":
					return "INT2";
				case "DECIMAL":
					return "DEC";
				case "BIGINTEGER":
					return "TIMESTAMP";
				case "SMALLINT":
					return "INT2";
			}
			return "";
		}

		private static List<string> FindPrimaryKeyColumns(string content)
		{
			List<string> primaryKeyColumns = new List<string>();
			Regex primaryKeyFinder = new Regex("ADD CONSTRAINT \"[^\"]+\" PRIMARY KEY\\s*\\(([^\\)]+)\\);", RegexOptions.Multiline);
			Regex primarkKeyColumnMatcher = new Regex("\"([^\"]+)\"");

			Match match = primaryKeyFinder.Match(content);
			Capture capture = match.Groups[1];

			foreach (string primaryKeyDefinition in capture.Value.Split(new[] {','})) {
				match = primarkKeyColumnMatcher.Match(primaryKeyDefinition);
				Capture keyCapture = match.Groups[1];
				primaryKeyColumns.Add(keyCapture.Value);
			}
			return primaryKeyColumns;
		}

		internal static bool IsCreateable(string filename)
		{
			if (!filename.EndsWith(".sql"))
				filename = $"{filename}.sql";

			bool result = false;
			try {
				if (File.Exists(filename)) {
					using (StreamReader reader = new StreamReader(filename)) {
						string firstLine = reader.ReadLine();
						if (firstLine != null &&
							firstLine.StartsWith("-- This CLP file was created using DB2LOOK")) {
							result = true;
						}
					}
				}
			} catch (Exception) {
				result = false;
			}

			return result;
		}
	}
}
