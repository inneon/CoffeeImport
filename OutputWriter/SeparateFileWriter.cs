using System.IO;

namespace CommunityCoffeeImport.OutputWriter
{
	class SeparateFileWriter : IOutputWriter
	{
		public string TableName { get; set; }
		public string TableDefinition { get; set; }
		public string CreateScript { get; set; }
		public string Format { get; set; }
		public string BulkInsertScript { get; set; }

		public void Write()
		{
			string outputFile = $"Data{Path.DirectorySeparatorChar}Output{Path.DirectorySeparatorChar}{TableName}.sql";
			WriteFile(outputFile, CreateScript);
			outputFile = $"Data{Path.DirectorySeparatorChar}Output{Path.DirectorySeparatorChar}{TableDefinition}.fmt";
			WriteFile(outputFile, Format);
			outputFile = $"Data{Path.DirectorySeparatorChar}Output{Path.DirectorySeparatorChar}{TableName}Insert.sql";
			WriteFile(outputFile, BulkInsertScript);
		}
		
		private static void WriteFile(string outputFile, string content)
		{
			if (File.Exists(outputFile)) {
				File.Delete(outputFile);
			}
			using (StreamWriter writer = new StreamWriter(outputFile)) {
				writer.WriteLine(content);
			}
		}

	}
}