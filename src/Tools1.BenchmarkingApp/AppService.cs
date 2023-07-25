using System.Reflection;
using Tools1.Benchmarking;

namespace Tools1.BenchmarkingApp
{
	public class AppService
	{
		public AppService(TextWriter outputWriter)
		{
			_outputWriter = outputWriter;
		}

		private readonly TextWriter _outputWriter;

		private readonly AppOptions _options = new AppOptions();
		private const string _defaultBanchmarkClassName = "Benchmarks";

		public int Run()
		{
			if (_options.AssemblyFilePath is null)
			{
				WriteLine("error: Mandatory argument 'assembly' not set");
				return -1;
			}
			if (!File.Exists(_options.AssemblyFilePath))
			{
				WriteLine("error: Assembly does not exists");
				return -1;
			}

			int passes = _options.Passes > 0 ? _options.Passes : 1;
			string className = _options.ClassName ?? _defaultBanchmarkClassName;
			Assembly assembly = Assembly.LoadFrom(_options.AssemblyFilePath);
			Type? benchType = assembly.ExportedTypes.FirstOrDefault(x => x.Name == className);

			if (benchType is null)
			{
				WriteLine("error: Class not found");
				return -1;
			}

			try
			{
				IDictionary<string, List<long>> result = Benchmarker.Run(benchType, passes);
				IDictionary<string, int> resultAvg = result.ToDictionary(x=> x.Key, x => (int)Math.Round(x.Value.Average()));
				WriteResult(resultAvg);
			}
			catch (Exception ex)
			{
				WriteLine($"error: {ex.Message}");
				return -1;
			}
			return 0;
		}

		public void SetOptionsFromArguments(string[] arguments)
		{
			for (int i = 0; i < arguments.Length; i += 2)
			{
				switch (arguments[i])
				{
					case "-a":
						_options.AssemblyFilePath = arguments[i + 1];
						break;
					case "-c":
						_options.ClassName = arguments[i + 1];
						break;
					case "-p":
						int passes;
						if (int.TryParse(arguments[i + 1], out passes)) _options.Passes = passes;
						break;
					case "-o":
						_options.OutputType = arguments[i + 1];
						break;
				}
			}
		}

		public void WriteHelp()
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{GetType().Namespace}.help.txt")!;
			using StreamReader reader = new StreamReader(stream);
			_outputWriter.Write(reader.ReadToEnd());
		}

		public void WriteLine(string line)
		{
			_outputWriter.WriteLine(line);
		}

		public void WriteResult(IDictionary<string, int> result)
		{
			switch (_options.OutputType)
			{
				case "json":
					_outputWriter.WriteLine(GetJsonResult(result));
					break;
				default:
					_outputWriter.WriteLine(GetTextResult(result));
					break;
			}
		}

		private string GetTextResult(IDictionary<string, int> result)
		{
			return string.Join("\r\n", result.Select(x => $"{x.Key}\t{x.Value}"));
		}

		private string GetJsonResult(IDictionary<string, int> result)
		{
			return "{\r\n" + string.Join(",\r\n", result.Select(x => $"\t\"{x.Key}\":{x.Value}")) + "\r\n}";
		}
	}
}
