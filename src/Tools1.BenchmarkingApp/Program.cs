

using Tools1.BenchmarkingApp;

public partial class Program
{
	public static int Main(string[] args)
	{
		AppService appService = new AppService(Console.Out);
		if (args.Length == 0 || args[0] == "?" || args[0] == "-h")
		{
			appService.WriteHelp();
			return 0;
		}

		appService.SetOptionsFromArguments(args);
		return appService.Run();
	}
}
