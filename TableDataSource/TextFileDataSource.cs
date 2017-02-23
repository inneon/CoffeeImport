using System;
using System.Collections.Generic;
using System.IO;

namespace CommunityCoffeeImport.TableDataSource
{
	class TextFileDataSource : ITableDataSource, IDisposable
	{
		private StreamReader fileStreamReader;

		public TextFileDataSource(string fileName)
		{
			fileStreamReader = new StreamReader(fileName);
		}

		public bool IsQuoteEnclosed => true;

		public bool UsesColumn(ColumnDefinition columnName)
		{
			return true;
		}

		public string GetNextLine()
		{
			string result = fileStreamReader.ReadLine();
			return result;
		}

		public List<ColumnDefinition> Reorder(List<ColumnDefinition> columnDefinitions)
		{
			return columnDefinitions;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				if (fileStreamReader != null) {
					fileStreamReader.Dispose();
					fileStreamReader = null;
				}
			}
		}

		public string[] GetNextLineCells()
		{
			throw new NotImplementedException();
		}
	}
}