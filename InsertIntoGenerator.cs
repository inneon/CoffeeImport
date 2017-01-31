using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityCoffeeImport
{
	class InsertIntoGenerator
	{
		private readonly string tableName;
		private readonly IEnumerable<ColumnDefinition> columnDefinitions;
		private readonly IEnumerable<string> rows;

		public InsertIntoGenerator(string tableName, IEnumerable<ColumnDefinition> columnDefinitions, IEnumerable<string> rows)
		{
			this.tableName = tableName;
			this.columnDefinitions = columnDefinitions;
			this.rows = rows;
		}

		public string GenerateInserts()
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine(string.Join($"{Environment.NewLine}", rows.Select(CreateInsert)));

			return builder.ToString();
		}

		private string CreateInsert(string arg)
		{
			string[] cells = LineParser.Parse(arg, LineParser.ParseOptions.RemoveQuotes).ToArray();

			if (cells.Length != columnDefinitions.Count()) {
				throw new NotSupportedException($"This row does not have the same number of cells as there should be columns. Row: {arg}");
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"INSERT INTO {tableName} VALUES (");
			builder.AppendLine(string.Join(", ", cells.Zip(columnDefinitions, CreateValue)));
			builder.AppendLine(")");
			return builder.ToString();
		}
		
		private string CreateValue(string cellValue, ColumnDefinition columnDefinition)
		{
			string result;
			switch (columnDefinition.Type) {
				case SqlType.bigint:
					result = cellValue;
					break;
				case SqlType.@decimal:
					result = cellValue;
					break;
				case SqlType.@int:
					result = cellValue;
					break;
				case SqlType.varchar:
					result = cellValue.Trim("\"".ToCharArray());
					result = $"'{result}'";
				break;
			default:
					throw new NotSupportedException($"Unrecognised type {columnDefinition.Type}");
			}
			return result;
		}
	}
}
