using System.IO;
using System.Text;

namespace CommunityCoffeeImport.OutputWriter
{
	class SingleFileWriter : IOutputWriter
	{
		public SingleFileWriter(string createFileName, string insertFileName)
		{
			this.createFileName = Path.Combine(Parameters.Singleton.OutputFolder, createFileName);
			this.insertFileName = Path.Combine(Parameters.Singleton.OutputFolder, insertFileName);

		}

		private readonly string createFileName;
		private readonly string insertFileName;
		public string TableName { get; set; }
		public string TableDefinition { get; set; }
		public string CreateScript { get; set; }
		public string Format { get; set; }
		public string BulkInsertScript { get; set; }

		public void Write()
		{
			using (StreamWriter writer = new StreamWriter(createFileName, true, Encoding.UTF32)) {
				writer.WriteLine($"-- {TableDefinition}");
				writer.WriteLine(CreateScript);
				writer.WriteLine();
			}
			using (StreamWriter writer = new StreamWriter(insertFileName, true, Encoding.UTF32)) {
				writer.WriteLine($"-- {TableDefinition}");
				writer.WriteLine($"print('{TableName}')");
				writer.WriteLine(BulkInsertScript);
				writer.WriteLine();
			}
			using (StreamWriter writer = new StreamWriter(Path.Combine(Parameters.Singleton.OutputFolder, $"{TableDefinition}.fmt"), false)) {
				writer.WriteLine(Format);
			}
		}
	}
}