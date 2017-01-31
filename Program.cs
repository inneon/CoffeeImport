using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityCoffeeImport.ColumnDefinitionReader;
using CommunityCoffeeImport.OutputWriter;
using CommunityCoffeeImport.TableDataSource;

namespace CommunityCoffeeImport
{
	class Program
	{
		private static void Main(string[] args)
		{
			try {
				string[] tableNames = new[] {
					"tvkot", "tvtwt", "tvkbt", "t171t", "tbrct", "t179t", "t001w", "tvgrt", "cskst", "t001b"
					, "cska", "cepct", "setheader", "setnode", "setleaf", "setheadert", "aufk"
				};
				IOutputWriter writer = new SingleFileWriter();
				foreach (string tableName in tableNames) {
					string tableDefinition = CreateTableDefinition(tableName);
					string formatFile;
					string bulkInsert;
					string create;
					using (DataSourceFactory factory = new DataSourceFactory()) {
						ITableDataSource source = factory.GetDataSourceForTable(tableName);
						List<ColumnDefinition> colDefinition = ColumnDefinitions(tableDefinition)
							.Where(col => source.UsesColumn(col)).ToList();
						List<ColumnDefinition> sourceOrderedColumnDefinitions = source.Reorder(colDefinition);
						CreateGenerator createGenerator = new CreateGenerator(tableDefinition, colDefinition);
						create = createGenerator.BuildCreate(out formatFile, source.IsQuoteEnclosed);
						IInsertGenerator insertGenerator = new BulkInsertGenerator(tableDefinition, source, tableName);
						if (source is ExcelTableDataSource) {
							insertGenerator = new InlineInsertGenerator(tableDefinition, source, colDefinition, sourceOrderedColumnDefinitions);
						}
						bulkInsert = insertGenerator.BuildInsert();
					}

					writer.TableName = tableName;
					writer.TableDefinition = tableDefinition;
					writer.CreateScript = create;
					writer.Format = formatFile;
					writer.BulkInsertScript = bulkInsert;
					writer.Write();
				}
			} catch (Exception e) {
				Console.WriteLine(e);
				Console.ReadKey();
			}
		}

		private static List<ColumnDefinition> ColumnDefinitions(string tableDefinition)
		{
			string content;
			Dictionary<string, IDefinitionReader> readersForExtension = new Dictionary<string, IDefinitionReader> {
				{"rgx", new RegexDefinitionReader() },
				{"xml", new HtmlDefinitionReader() },
			};
			string path;
			List<ColumnDefinition> result = null;

			foreach (KeyValuePair<string, IDefinitionReader> definitionReader in readersForExtension) {
				path = $"Data{Path.DirectorySeparatorChar}TableDefinitions{Path.DirectorySeparatorChar}{tableDefinition}.{definitionReader.Key}";
				if (File.Exists(path)) {

					using (StreamReader reader = new StreamReader(path)) {
						content = reader.ReadToEnd();
					}
					result = definitionReader.Value.LoadFromContent(content);
					break;
				}
			}
			if (result == null) {
				throw new NotSupportedException("No file found for table");
			}

			return result;
		}

		private static void WriteFile(string outputFile, string content)
		{
			if (File.Exists(outputFile)) {
				File.Delete(outputFile);
			}
			using (StreamWriter writer = new StreamWriter(outputFile)) {
				writer.WriteLine(content);
			}
		}

		private static List<string> ReadLinesFromFile(string path)
		{
			List<string> lines = new List<string>();
			using (StreamReader streamReader = new StreamReader(path)) {
				string line;
				while ((line = streamReader.ReadLine()) != null) {
					lines.Add(line);
				}
			}
			return lines;
		}

		private static string CreateTableDefinition(string tableName)
		{
			string result;
			Regex regex = new Regex("(.+)\\.(.+)");
			var match = regex.Match(tableName);
			if (match.Success) {
				result = match.Groups[1].Value;
			} else {
				result = tableName;
			}
			return result;
		}
	}
}
