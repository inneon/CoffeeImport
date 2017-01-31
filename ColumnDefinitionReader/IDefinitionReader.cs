using System.Collections.Generic;

namespace CommunityCoffeeImport.ColumnDefinitionReader
{
	internal interface IDefinitionReader
	{
		List<ColumnDefinition> LoadFromContent(string content);
	}
}