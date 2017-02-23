using System.Collections.Generic;

namespace CommunityCoffeeImport.TableDataSource
{
	interface ITableDataSource
	{
		bool IsQuoteEnclosed { get; }

		bool UsesColumn(ColumnDefinition columnName);

		string GetNextLine();

		string[] GetNextLineCells();

		List<ColumnDefinition> Reorder(List<ColumnDefinition> columnDefinitions);
	}
}
