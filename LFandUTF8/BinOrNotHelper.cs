using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LFandUTF8
{
	public class BinOrNotHelper
	{
		public BinOrNotHelper(string filename)
		{
			this.filename = filename;

			try
			{
				var json = File.ReadAllText(filename);
				Model = JsonConvert.DeserializeObject<BinOrNotModel>(json);
				if(Model == null || Model.BinExtensions == null || Model.TextExtensions == null)
					throw new Exception(string.Format("Invalid contents in {0}", filename));
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception when initializing BinOrNotHelper({0}). Recreating settings.", filename);
				Model = new BinOrNotModel
				{
					BinExtensions = new List<string>(),
					TextExtensions = new List<string>()
				};
			}
		}

		public bool IsText(FileInfo file)
		{
			// try to find among known extensions
			if (Model.TextExtensions.Contains(file.Extension))
				return true;
			if (Model.BinExtensions.Contains(file.Extension))
				return false;

			// or learn and add to database
			while (true)
			{
				Console.Write("{0}\n\tis it text or binary file? Press T/B ", file.FullName);
				var key = Console.ReadKey();
				Console.WriteLine();
				var input = char.ToUpper(key.KeyChar);
				switch (input)
				{
					case 'T':
						Learn(file.Extension, true);
						return true;
					case 'B':
						Learn(file.Extension, false);
						return false;
					default:
						continue;
				}
			}
		}

		private void Learn(string extension, bool isText)
		{
			if (string.IsNullOrEmpty(extension))
				return;
			if (isText)
			{
				if(Model.BinExtensions.Contains(extension))
					throw new InvalidOperationException(string.Format("Extension {0} is already learned as binary", extension));
				if(!Model.TextExtensions.Contains(extension))
					Model.TextExtensions.Add(extension);
			}
			else
			{
				if (Model.TextExtensions.Contains(extension))
					throw new InvalidOperationException(string.Format("Extension {0} is already learned as text", extension));
				if (!Model.BinExtensions.Contains(extension))
					Model.BinExtensions.Add(extension);
			}
		}

		public void Save()
		{
			Model.BinExtensions.Sort();
			Model.TextExtensions.Sort();
			var json = JsonConvert.SerializeObject(Model, Formatting.Indented);
			File.WriteAllText(filename, json);  // uses UTF-8 without BOM
			Console.WriteLine("Known file extensions saved to {0}", filename);
		}

		public BinOrNotModel Model { get; private set; }

		private readonly string filename;
	}
}
