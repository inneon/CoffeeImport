using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunityCoffeeImport
{
	internal class CreateGenerator
	{
		
		private readonly Dictionary<SqlType, string> formatMapping = new Dictionary<SqlType, string> {
			//{ SqlType.bigint, "SQLINT" },
			//{ SqlType.@int, "SQLINT" },
			//{ SqlType.@decimal, "SQLDECIMAL" },
			//{ SqlType.varchar, "SQLCHAR" }
			{ SqlType.bigint, "SQLCHAR" },
			{ SqlType.@int, "SQLCHAR" },
			{ SqlType.@decimal, "SQLCHAR" },
			{ SqlType.varchar, "SQLCHAR" }
		};

		private bool leadingQuote;
		private bool quoteEnclosed;
		
		private readonly string tableName;
		private readonly List<ColumnDefinition> columnDefinitions;

		public CreateGenerator(string tableName, List<ColumnDefinition> columnDefinitions)
		{
			this.tableName = tableName;
			this.columnDefinitions = columnDefinitions;
		}

		public string BuildCreate(out string formatFile, bool isQuoteEnclosed)
		{
			quoteEnclosed = isQuoteEnclosed;
			StringBuilder insertBuilder = new StringBuilder();
			StringBuilder formatBuilder = new StringBuilder();

			insertBuilder.AppendLine($"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL{Environment.NewLine}  DROP TABLE {tableName};");
			insertBuilder.AppendLine($"CREATE TABLE {tableName} (");

			formatBuilder.AppendLine("11.0");
			DetermineLeadingQuote(columnDefinitions[0]);
			int formatRows = columnDefinitions.Count();
			if (leadingQuote) {
				formatRows++;
			}
			formatBuilder.AppendLine(formatRows.ToString());
			if (leadingQuote) {
				formatBuilder.AppendLine("1 SQLCHAR 0 0 \"\\\"\" 0 FIRST_QUOTE Latin1_General_CI_AS");
			}
			
			insertBuilder.Append(string.Join($",{Environment.NewLine}", columnDefinitions.Select(ToColumnDefinition)));
			insertBuilder.AppendLine($", PRIMARY KEY ({string.Join(", ", columnDefinitions.Where(col => col.IsKey).Select(col => col.Field))})");
			formatBuilder.Append(string.Join($"{Environment.NewLine}", columnDefinitions.Select(ToFormat)));

			insertBuilder.AppendLine(");");

			formatFile = formatBuilder.ToString();
			return insertBuilder.ToString();
		}

		private string ToColumnDefinition(ColumnDefinition arg)
		{
			string name = arg.Field.Replace("/", "_");
			string dataType = arg.DataType;
			int length = arg.Length;
			int decimals = arg.Decimals;
			string lengthString = string.Empty;
			SqlType sqlType = arg.SqlDataType;

			dataType = sqlType.ToString();
			if (dataType == "decimal") {
				if (decimals != 0) {
					lengthString = $"{length},{decimals}";
				} else {
					lengthString = length.ToString();
				}
				lengthString = $"({lengthString})";
			} else if (dataType == "int") {
				lengthString = "";
			} else if (dataType == "varchar") {
				lengthString = $"({length})";
			}

			string notNull = string.Empty;
			if (arg.IsKey) {
				notNull = "NOT NULL";
			}
			return $"  {name} {dataType} {lengthString} {notNull}";
		}

		private string ToFormat(ColumnDefinition arg, int columnIndex)
		{
			string name = arg.Field;
			string dataType = arg.DataType;
			string length = arg.Length.ToString();
			string delimeter = ",";
			string caseSensitive = "\"\"";
			SqlType sqlType = arg.SqlDataType;

			if (columnIndex == columnDefinitions.Count() - 1) {
				delimeter = Parameters.Singleton.ClosingComma ? ",\\r\\n" : "\\r\\n";
			}
			
			bool quoteDelimited = IsColumnQuoteDelimited(arg);
			dataType = formatMapping[sqlType];
			if (dataType == formatMapping[SqlType.@decimal]) {
				length = (arg.Length + arg.Decimals).ToString();
			}

			if (quoteDelimited && quoteEnclosed) {
				delimeter = $"\\\"{delimeter}";
				caseSensitive = "Latin1_General_CI_AS";
			}
			if (columnIndex + 1 < columnDefinitions.Count()) {
				ColumnDefinition nextArg = columnDefinitions[columnIndex + 1];
				if (IsColumnQuoteDelimited(nextArg) && quoteEnclosed) {
					delimeter = $"{delimeter}\\\"";
				}
			}
			delimeter = $"\"{delimeter}\"";

			int lineNumber = leadingQuote ? (columnIndex + 2) : (columnIndex + 1);
			string result = $"{lineNumber} {dataType} 0 {length} {delimeter} {columnIndex+1} {name} {caseSensitive}";
			return result;
		}

		private void DetermineLeadingQuote(ColumnDefinition firstColumnDefinition)
		{
			leadingQuote = IsColumnQuoteDelimited(firstColumnDefinition) && quoteEnclosed;
		}

		private bool IsColumnQuoteDelimited(ColumnDefinition col)
		{
			bool result = col.SqlDataType == SqlType.varchar || col.DataType == "NUMC" || Parameters.Singleton.NumericsAreQuoted;
			return result;
		}

	}
}
