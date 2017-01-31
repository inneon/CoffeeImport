using System.Collections.Generic;

namespace CommunityCoffeeImport
{
	class ColumnDefinition
	{
		private readonly Dictionary<string, SqlType> typeMapping = new Dictionary<string, SqlType> {
			{ "CLNT", SqlType.varchar },
			{ "NUMC", SqlType.@int },
			{ "INT4", SqlType.@int },
			{ "CUKY", SqlType.varchar },
			{ "CHAR", SqlType.varchar },
			{ "UNIT", SqlType.varchar },
			{ "CURR", SqlType.@decimal },
			{ "QUAN", SqlType.@decimal },
			{ "DEC", SqlType.@decimal },
			{ "TIMESTAMP",SqlType.bigint },
			{ "DATS",SqlType.varchar },
			{ "TIMS",SqlType.varchar },
			{ "LANG",SqlType.varchar }
		};

		public string Field { get; set; }
		public string DataElement { get; set; }
		public string DataType { get; set; }
		public int Length { get; set; }
		public int Decimals { get; set; }
		public bool IsKey { get; set; }

		public SqlType SqlDataType => typeMapping[DataType];
	}
}