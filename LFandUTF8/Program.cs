using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LFandUTF8
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.TreatControlCAsInput = false;
			Console.CancelKeyPress += OnCtrlC;
			try
			{
				Start(args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
#if DEBUG
			Console.ReadKey();
#endif
		}

		private static void OnCtrlC(object sender, ConsoleCancelEventArgs e)
		{
			if (helper == null)
				return;
			Console.WriteLine("Catched ctrl-c, saving knowledge..");
			helper.Save();
#if DEBUG
			Console.ReadKey();
#endif
		}

		static void Start(string[] args)
		{
			var program = new FileInfo(Environment.GetCommandLineArgs()[0]);

			if (args.Length != 1)
			{
				Console.WriteLine("Usage: {0} <repository_to_convert>", program.Name);
				return;
			}

			var path = args[0];
			if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
			{
				Console.WriteLine("{0} is not a directory.", path);
				return;
				
			}

			helper = new BinOrNotHelper("settings.json");
			try
			{
				foreach (var file in ListRepositoryFiles(path))
				{
					WorkFile(file, helper);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception while iterating over files: {0}", e);
			}
			finally
			{
				helper.Save();
			}
		}

		private static IEnumerable<FileInfo> ListRepositoryFiles(string path)
		{
			var gitDir = Path.Combine(path, ".git");
			var gitArgs = string.Format("--git-dir={0} --work-tree={1} ls-files --full-name -z", gitDir, path);

			var git = new Process
			{
				StartInfo =
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					FileName = "git",
					Arguments = gitArgs,
					StandardOutputEncoding = Encoding.UTF8
				}
			};
			
			git.Start();
			var fileList = git.StandardOutput.ReadToEnd();
			git.WaitForExit();
			return fileList.Split(new []{'\0'}, StringSplitOptions.RemoveEmptyEntries).Select(filename => new FileInfo(Path.Combine(path, filename)));
		}


		private static void WorkFile(FileInfo file, BinOrNotHelper helper)
		{
			try
			{
				if (!file.Exists)
				{
					Console.WriteLine("{0} does not exist.", file.FullName);
					return;
				}
				if (helper.IsText(file))
				{
					CreateBackup(file);
					ConvertFile(file);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Error at file {0}:\n{1}", file.FullName, e);
			}
		}

		private static BinOrNotHelper helper;

		private static void CreateBackup(FileInfo file)
		{
			var backupFilename = string.Format("{0}.{1}.bak", file.FullName, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ff"));
			Console.WriteLine("Backup: {0} -> {1}", file.FullName, backupFilename);
			file.CopyTo(backupFilename, true);
		}

		private static void ConvertFile(FileInfo file)
		{
			var content = File.ReadAllBytes(file.FullName);
			var clean = TextTools.GetCleanString(content);

			var utf8NoBom = new UTF8Encoding(false);  // UTF8 encoder without BOM
			File.WriteAllText(file.FullName, clean, utf8NoBom);
			Console.WriteLine("Done!");
		}
	}
}
