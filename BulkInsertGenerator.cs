﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
			string currentNetworkFolder = Parameters.Singleton.BulkInsertPublicLocation;

			List<string> fragmentFiles = new List<string>();
			bool read = true;
			int fileNumber = 0;

			// sometimes we get numbers exported like 12.34- this turns it into -12.34 and copes with quotes and integers too
			Regex negativeReplacer = new Regex("((?:,|^)\"?)(\\d+(?:\\.\\d+)?)-(\"?(?:,|$))");
			// Capture group 1 is the first comma (or start of line) and an optional quote
			// Capture group 2 is the number without the negative sign
			// Capture group 3 is the second comma (or end of line) and an optional quote
			const string negativeReplacement = "$1-$2$3";

			while (read) {
				string fragmentName = $"{importName}fragment{fileNumber}.csv";
				fragmentFiles.Add(fragmentName);
				using (StreamWriter writer = new StreamWriter(Path.Combine(Parameters.Singleton.OutputFolder, fragmentName))) {
					for (int i = 0; (i < MaxBulkInsertRows) && read; i++) {
						string line = tableDataSource.GetNextLine();
						read = line != null;
						if (read) {
							ILineMangler mangler;
							// Apply it twice to match where 2 subsequent numbers are negative and the first capture overlaps the second
							line = negativeReplacer.Replace(line, negativeReplacement);
							line = negativeReplacer.Replace(line, negativeReplacement);
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
				bulkInsertBuilder.AppendLine($"FROM '{currentNetworkFolder}{Path.DirectorySeparatorChar}{fragmentFile}'");
				bulkInsertBuilder.AppendLine("WITH (");
				bulkInsertBuilder.AppendLine($"  FORMATFILE = '{currentNetworkFolder}{Path.DirectorySeparatorChar}{tableName}.fmt'");
				bulkInsertBuilder.AppendLine($"  --,ERRORFILE  = '{fragmentFile}.log'");
				bulkInsertBuilder.AppendLine(");");
			}

			return bulkInsertBuilder.ToString();
		}
	}
}
