using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityCoffeeImport.ColumnDefinitionReader;
using CommunityCoffeeImport.OutputWriter;
using CommunityCoffeeImport.TableDataSource;
using Newtonsoft.Json;

namespace CommunityCoffeeImport
{
	class Program
	{
		public static void Main(string[] args)
		{
			try {
				if (!ReadSettings(args)) {
					Console.ReadKey();
					return;
				}

				string[] tableNames = Parameters.Singleton.Tables;
					/*new[] {
					"ce1bp01.2016", "ce1bp01.2017", "ce2bp01.2016", "ce2bp01.2017", "cobk.2016_001", "cobk.2017_001", "coep.2016_001", "coep.2017_001",
					"faglflexa.2016_001", "faglflexa.2017_001", "faglflext.2016", "faglflext.2017",
					"tvkot", "tvtwt", "tvkbt", "tvgrt", "t171t", "tbrct", "t179t", "t001w", "csks", "cskst", "t001", "t001b", "cska", "csku", "skat",
					"cepct", "setheader", "setheadert", "setnode", "setleaf", "fagl_011pc", "fagl_011zc", "fagl_011sc", "fagl_011qt", "aufk", "mara"
				};*/
				IOutputWriter writer = new SingleFileWriter(Parameters.Singleton.CreateFileName, Parameters.Singleton.InsertFileName);
				foreach (string tableName in tableNames) {
					Console.WriteLine($"Processing file {tableName}");
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
			Console.WriteLine("Completed all tables successfully");
		}

		private static bool ReadSettings(string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine("Please supply a command line parameter for the input file.");
				return false;
			}

			string parametersFile = args[0];
			if (!File.Exists(parametersFile)) {
				Console.WriteLine($"Cannot find the specified input file: {parametersFile}.");
				return false;
			}

			var serialiser = JsonSerializer.Create();
			try {
				using (StreamReader reader = new StreamReader(args[0])) {
					Parameters.Singleton = (Parameters) serialiser.Deserialize(reader, typeof(Parameters));
				}
			} catch (JsonReaderException e) {
				if (e.Message.StartsWith("Bad JSON escape sequence: \\")) {
					Console.WriteLine(
						$"Remember: the input file needs to have the slashes escaped (replace '\\' with '\\\\'). It looks like you have not escaped a slash on line {e.LineNumber}.");
				} else {
					Console.WriteLine("Something went wrong when attempting to access the contents of the input file. The file could be opened, but something else went wrong:");
					Console.WriteLine(e.Message);
				}
				return false;
			} catch (Exception e) {
				Console.WriteLine($"Something went wrong when attempting to access the input file:{Environment.NewLine}{e.Message}");
				return false;
			}

			if (!Directory.Exists(Parameters.Singleton.DataFolder)) {
				Console.WriteLine($"The input file specifies the data folder {Parameters.Singleton.DataFolder}, but this does not exist.");
				return false;
			}
			if (!Directory.Exists(Parameters.Singleton.TableDefinitionFolder)) {
				Console.WriteLine($"The input file specifies the table definition folder {Parameters.Singleton.TableDefinitionFolder}, but this does not exist.");
				return false;
			}
			if (!Directory.Exists(Parameters.Singleton.OutputFolder)) {
				Console.WriteLine($"The input file specifies the output folder {Parameters.Singleton.OutputFolder}, but this does not exist.");
				return false;
			}
			/*if (!Directory.Exists(Parameters.Singleton.BulkInsertFolder)) {
				Console.WriteLine($"The input file specifies the bulk insert folder {Parameters.Singleton.BulkInsertFolder}, but this does not exist.");
				return false;
			}*/
			string createFileWritable = CheckFileWritable(Parameters.Singleton.CreateFileName);
			if (!string.IsNullOrEmpty(createFileWritable)) {
				Console.WriteLine($"The specified file for the script creating tables ({Parameters.Singleton.CreateFileName}) could not be written. The error was:");
				Console.WriteLine(createFileWritable);
				return false;
			}
			string insertFileWritable = CheckFileWritable(Parameters.Singleton.InsertFileName);
			if (!string.IsNullOrEmpty(insertFileWritable)) {
				Console.WriteLine($"The specified file for the script creating tables ({Parameters.Singleton.InsertFileName}) could not be written. The error was:");
				Console.WriteLine(insertFileWritable);
				return false;
			}

			return true;
		}

		private static string CheckFileWritable(string fileName)
		{
			fileName = Path.Combine(Parameters.Singleton.OutputFolder, fileName);
			if (File.Exists(fileName)) {
				try {
					File.Delete(fileName);
				} catch (Exception e) {
					return e.Message;
				}
			}

			try {
				File.Create(fileName);
			} catch (Exception e) {
				return e.Message;
			}

			return string.Empty;
		}

		private static List<ColumnDefinition> ColumnDefinitions(string tableDefinition)
		{
			string content;
			Dictionary<string, IDefinitionReader> readersForExtension = new Dictionary<string, IDefinitionReader> {
				{"rgx", new RegexDefinitionReader() },
				{"xml", new HtmlDefinitionReader() },
			};
			List<ColumnDefinition> result = null;

			foreach (KeyValuePair<string, IDefinitionReader> definitionReader in readersForExtension) {
				string path = Path.Combine(Parameters.Singleton.TableDefinitionFolder, $"{tableDefinition}.{definitionReader.Key}");
				if (File.Exists(path)) {

					using (StreamReader reader = new StreamReader(path)) {
						content = reader.ReadToEnd();
					}
					result = definitionReader.Value.LoadFromContent(content);
					break;
				}
			}
			if (result == null) {
				throw new NotSupportedException($"No metadata file was found for table {tableDefinition}");
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
