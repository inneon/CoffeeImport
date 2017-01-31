using System.Collections.Generic;

namespace CommunityCoffeeImport.TableDataSource
{
	interface ITableDataSource
	{
		bool IsQuoteEnclosed { get; }

		bool UsesColumn(ColumnDefinition columnName);

		string GetNextLine();

		List<ColumnDefinition> Reorder(List<ColumnDefinition> columnDefinitions);
	}
}
