namespace CommunityCoffeeImport.LineMangler
{
	class SkatManger : ILineMangler
	{
		public string Mangle(string line)
		{
			const string language = "\"EN";
			if (line.StartsWith(language)) {
				line = $"\"E{line.Substring(language.Length)}";
			}
			return line;
		}
	}
}