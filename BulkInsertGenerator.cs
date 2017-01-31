using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityCoffeeImport.LineMangler;
using CommunityCoffeeImport.TableDataSource;

namespace CommunityCoffeeImport
{
	class BulkInsertGenerator : IInsertGenerator
	{
		private const int MaxBulkInsertRows = 30000;

		private readonly string tableName;
		private readonly ITableDataSource tableDataSource;
		private readonly string importName;

		private Dictionary<string, ILineMangler> lineManglers = new Dictionary<string, ILineMangler> {
			{ "coep", new CoepLineMangler() },
			{ "skat", new SkatManger() }
		};

		public BulkInsertGenerator(string tableName, ITableDataSource tableDataSource, string importName)
		{
			this.tableName = tableName;
			this.tableDataSource = tableDataSource;
			this.importName = importName;
		}

		public string BuildInsert()
		{
			StringBuilder bulkInsertBuilder = new StringBuilder();

			string currentLocal = Directory.GetCurrentDirectory();
			Regex folderMatcher = new Regex($"^[A-Z]{Path.VolumeSeparatorChar}\\{Path.DirectorySeparatorChar}(.*)");
			var match = folderMatcher.Match(currentLocal);
			if (!match.Success || match.Groups.Count <= 1) {
				throw new NotSupportedException();
			}
			string folder = match.Groups[1].Value;
			string currentNetworkFolder = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}DWDL-JONNY01{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Output";

			List<string> fragmentFiles = new List<string>();
			bool read = true;
			int fileNumber = 0;
			while (read) {
				string fragmentName = $"{currentNetworkFolder}{Path.DirectorySeparatorChar}{importName}fragment{fileNumber}.csv";
				fragmentFiles.Add(fragmentName);
				using (StreamWriter writer = new StreamWriter(fragmentName)) {
					for (int i = 0; (i < MaxBulkInsertRows) && read; i++) {
						string line = tableDataSource.GetNextLine();
						read = line != null;
						if (read) {
							ILineMangler mangler;
							if (lineManglers.TryGetValue(tableName, out mangler)) {
								line = mangler.Mangle(line);
							}
							writer.WriteLine(line);
						}
					}
					fileNumber++;
				}
			}

			foreach (string fragmentFile in fragmentFiles) {
				bulkInsertBuilder.AppendLine($"BULK INSERT {tableName}");
				bulkInsertBuilder.AppendLine($"FROM '{fragmentFile}'");
				bulkInsertBuilder.AppendLine("WITH (");
				bulkInsertBuilder.AppendLine($"  FORMATFILE = '{currentNetworkFolder}{Path.DirectorySeparatorChar}{tableName}.fmt'");
				//bulkInsertBuilder.AppendLine($"  ,ERRORFILE  = '{fragmentFile}.log'");
				bulkInsertBuilder.AppendLine(");");
			}

			return bulkInsertBuilder.ToString();
		}
	}
}
