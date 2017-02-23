﻿using System;

namespace CommunityCoffeeImport
{
	[Serializable]
	public class Parameters
	{
		public static Parameters Singleton { get; set; }

		public string DataFolder { get; set; }

		public string TableDefinitionFolder { get; set; }

		public string MasterDataFolder { get; set; }

		public string OutputFolder { get; set; }

		public string NetworkLocation { get; set; }

		public string CreateFileName { get; set; }

		public string InsertFileName { get; set; }

		public string[] Tables { get; set; }
	}
}