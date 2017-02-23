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
			string outputFile = Path.Combine(Parameters.Singleton.OutputFolder, $"{TableName}.sql");
			WriteFile(outputFile, CreateScript);
			outputFile = Path.Combine(Parameters.Singleton.OutputFolder, $"{TableDefinition}.sql");
			WriteFile(outputFile, Format);
			outputFile = Path.Combine(Parameters.Singleton.OutputFolder, $"{TableName}Insert.sql");
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