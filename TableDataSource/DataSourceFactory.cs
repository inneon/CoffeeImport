using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace CommunityCoffeeImport.TableDataSource
{
	class DataSourceFactory : IDisposable
	{
		List<IDisposable> disposables = new List<IDisposable>();

		public ITableDataSource GetDataSourceForTable(string tableName)
		{
			ITableDataSource result = null;

			result = TryLoadTextFile(tableName);

			if (result == null)
				result = TryLoadExcelFile(tableName);

			if (result == null) {
				throw new ArgumentException($"Cannot find any file for table {tableName}");
			}

			return result;
		}

		private ITableDataSource TryLoadExcelFile(string tableName)
		{
			ExcelTableDataSource result = null;
			
			if (!string.IsNullOrEmpty(Parameters.Singleton.MasterDataFile)) {
				string fileName = Path.Combine(Parameters.Singleton.DataFolder, Parameters.Singleton.MasterDataFile);
				byte[] file = File.ReadAllBytes(fileName);
				MemoryStream stream = null;
				ExcelPackage package = null;
				try {
					stream = new MemoryStream(file);
					package = new ExcelPackage(stream);
					ExcelWorksheet sheet = package.Workbook.Worksheets[tableName];
					if (sheet != null) {
						result = new ExcelTableDataSource(sheet);
						disposables.Add(package);
						disposables.Add(stream);
					}
				} finally {
					if (result == null) {
						stream?.Dispose();
						package?.Dispose();
					}
				}
			}

			return result;
		}

		private ITableDataSource TryLoadTextFile(string tableName)
		{
			TextFileDataSource result = null;
			string fileName = Path.Combine(Parameters.Singleton.DataFolder, $"{tableName}.txt");
			if (File.Exists(fileName)) {
				result = new TextFileDataSource(fileName);
				disposables.Add(result);
			}
			return result;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				foreach (IDisposable disposable in disposables) {
					disposable?.Dispose();
				}
				disposables.Clear();
				disposables = null;
			}
		}
	}
}
