using System;
namespace afi.datetime2image
{
	public class TerminalOptions
	{
		public TerminalOptions()
		{
		}

		[CommandLine.Option('d', "debug", HelpText = "Show debug info", Required =false, Default = false )]
		public bool Debug { get; set; }
	}
}
