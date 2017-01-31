namespace CommunityCoffeeImport.OutputWriter
{
	interface IOutputWriter
	{
		string TableName { set; }
		string TableDefinition { set; }
		string CreateScript { set; }
		string Format { set; }
		string BulkInsertScript { set; }

		void Write();
	}
}
