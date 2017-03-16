using System;
using System.Collections.Generic;
using System.IO;

namespace CommunityCoffeeImport.ColumnDefinitionReader
{
	internal static class ColumnDefinitionFactory
	{
		public static List<ColumnDefinition> GetColumnDefinitionsForTable(string tableDefinition)
		{
			string content;
			List<ReaderConfiguration> readers = new List<ReaderConfiguration> {
				new ReaderConfiguration("sql", Db2LookReader.IsCreateable, new Db2LookReader()),
				new ReaderConfiguration("rgx", new RegexDefinitionReader()),
				new ReaderConfiguration("xml", new HtmlDefinitionReader()),
			};
			List<ColumnDefinition> result = null;

			foreach (ReaderConfiguration definitionReader in readers) {
				string path = Path.Combine(Parameters.Singleton.TableDefinitionFolder, $"{tableDefinition}.{definitionReader.FileExtension}");
				if (definitionReader.IsCreateableUsingFile(path)) {

					using (StreamReader reader = new StreamReader(path)) {
						content = reader.ReadToEnd();
					}
					result = definitionReader.DefinitionReader.LoadFromContent(content);
					break;
				}
			}
			if (result == null) {
				throw new NotSupportedException($"No metadata file was found for table {tableDefinition}");
			}

			return result;
		}

		private struct ReaderConfiguration
		{
			public ReaderConfiguration(string fileExtension, IDefinitionReader definitionReader) : this(fileExtension, File.Exists, definitionReader)
			{
			}

			public ReaderConfiguration(string fileExtension, Func<string, bool> isCreateableUsingFile, IDefinitionReader definitionReader)
			{
				FileExtension = fileExtension;
				IsCreateableUsingFile = isCreateableUsingFile;
				DefinitionReader = definitionReader;
			}

			/// <summary>The file extension that the IDefintionReader uses.</summary>
			public string FileExtension { get; }
			/// <summary>Gets whether it is possible to use the IDefintionReader given the file name.</summary>
			public Func<string, bool> IsCreateableUsingFile { get; }
			/// <summary>The defition reader for this configuration.</summary>
			public IDefinitionReader DefinitionReader { get; }

			
		}
	}
}
