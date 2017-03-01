using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace CommunityCoffeeImport.TableDataSource
{
	class ExcelTableDataSource : ITableDataSource
	{
		private int row = 1; // columns in the 0th row
		private int maxRows;
		private object[,] grid;
		private List<string> columnNames;
		private Dictionary<string, ColumnDefinition> columnDefinitions = new Dictionary<string, ColumnDefinition>();
		private ExcelRange excelRange;

		public ExcelTableDataSource(ExcelWorksheet excelWorksheet)
		{
			excelRange = excelWorksheet.Cells;
			grid = (object[,])excelRange.Value;

			columnNames = ReadColumnNames();
			maxRows = grid.GetLength(0);
		}

		public bool IsQuoteEnclosed => true;

		public bool UsesColumn(ColumnDefinition column)
		{
			bool result = columnNames.Contains(column.Field);
			if (result) {
				columnDefinitions[column.Field] = column;
			}
			return result;
		}

		public string GetNextLine()
		{
			string result = null;
			if (row < maxRows) {
				result = string.Join(",", GetRowValues(true));
				row++;
			}
			return result;
		}

		public string[] GetNextLineCells()
		{
			string[] result = null;
			if (row < maxRows) {
				result = GetRowValues(false).ToArray();
				row++;
			}
			return result;
		}

		public List<ColumnDefinition> Reorder(List<ColumnDefinition> cols)
		{
			List<ColumnDefinition> result = columnNames.Select(name =>
			{
				try {
					return cols.Single(col => col.Field == name);
				} catch (InvalidOperationException e) {
					throw new NotSupportedException($"Column \"{name}\" in the source data cannot be matched to a column in the metadata.", e);
				}
			}).ToList();
			return result;
		}

		private IEnumerable<string> GetRowValues(bool requote)
		{
			for (int i = 0; i < grid.GetLength(1); i++) {
				string item = excelRange[row+1, i+1].Text;

				int number;
				ColumnDefinition definition = columnDefinitions[columnNames[i]];
				DateTime dateTime;
				
				Regex timeMatcher = new Regex("(\\d\\d):(\\d\\d):(\\d\\d)");
				Match match = timeMatcher.Match(item);
				if (match.Success) {
					item = match.Groups[1].Value + match.Groups[2].Value + match.Groups[3].Value;
				} else if (DateTime.TryParse(item, out dateTime)) {
					item = dateTime.ToString("yyyyMMdd");
				} else if (int.TryParse(item, out number)) {
					if (definition.DataType.ToLower() == "char") {
						int length = definition.Length;
						item = item.PadLeft(length, '0');
					}
				}
				if (definition.DataType == "LANG" && item.Length > 1) {
					// hack - but whatevs, right?
					item = item.Substring(0, 1);
				} else if (definition.DataElement == "SCOPE_CV" && item.Length > 2) {
					item = item.Substring(0, 2);
				}

				if ((definition.SqlDataType == SqlType.@int || definition.SqlDataType == SqlType.@decimal) && item.ToString() == string.Empty) {
					item = "null";
				} else if ((definition.SqlDataType == SqlType.varchar || definition.DataType == "NUMC") && requote) {
					item = $"\"{item}\"";
				}
				

				yield return item.ToString();
			} 
		}

		private List<string> ReadColumnNames()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < grid.GetLength(1); i++) {
				result.Add(grid[0, i].ToString());
			}
			return result;
		} 
	}
}