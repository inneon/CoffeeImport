using System.IO;
using System.Text;

namespace CommunityCoffeeImport.OutputWriter
{
	class SingleFileWriter : IOutputWriter
	{
		public SingleFileWriter()
		{
			fileName = Path.Combine("Data", "Output", "Script.sql");

			if (File.Exists(fileName)) {
				File.Delete(fileName);
			}
			File.Create(fileName);
		}

		private string fileName;
		public string TableName { get; set; }
		public string TableDefinition { get; set; }
		public string CreateScript { get; set; }
		public string Format { get; set; }
		public string BulkInsertScript { get; set; }

		public void Write()
		{
			using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF32)) {
				writer.WriteLine($"-- {TableDefinition}");
				writer.WriteLine(CreateScript);
				writer.WriteLine();
				writer.WriteLine(BulkInsertScript);
				writer.WriteLine();
			}
			using (StreamWriter writer = new StreamWriter(Path.Combine("Data", "Output", $"{TableDefinition}.fmt"), false)) {
				writer.WriteLine(Format);
			}
		}
	}
}