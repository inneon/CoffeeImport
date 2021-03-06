using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommunityCoffeeImport.TableDataSource;

namespace CommunityCoffeeImport
{
	class InlineInsertGenerator : IInsertGenerator
	{
		private string tableName;
		private ITableDataSource tableDataSource;
		private readonly List<ColumnDefinition> columnDefinitions;
		private string insertColumnNames;

		public InlineInsertGenerator(string tableName, ITableDataSource tableDataSource, List<ColumnDefinition> columnDefinitions, List<ColumnDefinition> sourceOrderedColumnDefinitions)
		{
			this.tableName = tableName;
			this.tableDataSource = tableDataSource;
			this.columnDefinitions = columnDefinitions;
			insertColumnNames = string.Join(",", sourceOrderedColumnDefinitions.Select(col => col.Field.Replace("/", "_")));
		}
		
		public string BuildInsert()
		{
			StringBuilder insertBuilder = new StringBuilder();
			
			bool read = true;
			IEnumerable<string> line;

			while (read) {
				line = tableDataSource.GetNextLineCells();
				read = line != null;

				if (read) {
					insertBuilder.AppendLine($"INSERT INTO {tableName} ({insertColumnNames}) VALUES (");
					if (line.Count() != columnDefinitions.Count()) {
						throw new NotSupportedException($"Row has {line.Count()} entries, but there are {columnDefinitions.Count} columns");
					}

					Regex quoteEscape = new Regex("(^|[^'])'([^']|$)");

					string values = string.Join(", ", line.Zip(columnDefinitions, (val, def) => new {val, def})
						.Select(pair =>
						{
							string res = pair.val;
							res = quoteEscape.Replace(res, "$1''$2");
							if (pair.def.SqlDataType != SqlType.@int && pair.def.SqlDataType != SqlType.@decimal) {
								res = $"'{res}'";
							}
							return res;
						}));
					insertBuilder.AppendLine(values);
					insertBuilder.AppendLine(");");

				}
			}
			
			return insertBuilder.ToString();
		}
	}
}