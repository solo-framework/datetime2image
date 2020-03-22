using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using CommandLine;
using CommandLine.Text;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;

namespace afi.datetime2image
{
    class MainClass
    {
		protected static bool IsDebug = false;

		protected static string GetDateTime(string file)
		{
			try
			{
				Debug($"Метаданные:");

				var directories = ImageMetadataReader.ReadMetadata(file);

				foreach (var directory in directories)
				{
					if (directory.Name != "File")
						continue;

					foreach (var tag in directory.Tags)
					{

						Debug($"tag: type {tag.Type}, name {tag.Name}, desc {tag.Description}");
						if (tag.Type == FileMetadataDirectory.TagFileModifiedDate)
						{
							//Console.WriteLine($"[{directory.Name}] {tag.Name} [{tag.Type}] = {tag.Description}");
							// Fri Mar 01 13:53:08 +03:00 2019

							if (DateTime.TryParseExact(tag.Description, datePatterns, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out DateTime dt))
								return dt.ToString("dd/MM/yyyy HH:mm:ss");
							else
								return null;
						}
					}

					if (directory.HasError)
					{
						StringBuilder sb = new StringBuilder();
						foreach (var error in directory.Errors)
							sb.AppendLine(error);
						throw new ImageProcessingException(sb.ToString());														

					}
				}
			}
			catch (ImageProcessingException ipe)
			{
				Info($"Ошибка обработки файла [{file}]: {ipe.Message}");
			}


			return null;
		}

		public static void Run(TerminalOptions options)
		{
			IsDebug = options.Debug;
			HandleImages();
		}

		private static void HandleParseError(IEnumerable<Error> errs)
		{
			
			if (errs.IsHelp())
			{
				Console.WriteLine("Help Request");
				return;
			}
			//Console.WriteLine("Некорректные аргументы");
		}

		private static void HandleImages()
		{ 
			try
			{
				if (!System.IO.Directory.Exists("./output"))
				{
					System.IO.Directory.CreateDirectory("./output");
				}

				string[] files = System.IO.Directory.GetFiles("./input/");
				foreach (string file in files)
				{
					Debug();
					Info($"Обработка файла {file}");

					string dateTime = GetDateTime(file);
					if (dateTime == null)
					{
						Info($"Пропуск {file}");
					}
					else
					{
						Debug($"{file}: {dateTime}");
						//byte[] f = File.ReadAllBytes(file);
						using (Image img = Image.FromFile(file))
						{
							Debug($"image size: {img.Size}");

							int fontSize = 10 * (img.Size.Width / 200);
							Debug($"font size: {fontSize}");

							using (var graphic = Graphics.FromImage(img))
							{
								var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
								var color = Color.White;
								var brush = new SolidBrush(color);

								string outText = $"{dateTime}";

								// size of string
								SizeF stringSize = new SizeF();
								stringSize = graphic.MeasureString(outText, font);
								Debug($"string size: {stringSize}");

								int textWidth = (int)(img.Width - stringSize.Width);
								int textHeight = (int)(img.Height - stringSize.Height);
								Debug($"text position: {textWidth} x {textHeight}");

								var point = new Point(textWidth - 30, textHeight - 30);

								SolidBrush solidBush = new SolidBrush(Color.Black);
								graphic.FillRectangle(solidBush, new Rectangle(textWidth - 30, textHeight - 30, (int)stringSize.Width, (int)stringSize.Height));

								graphic.DrawString(outText, font, brush, point);
								FileInfo fi = new FileInfo(file);

								img.Save($"./output/{fi.Name}");
							}
						}
					}
				}

				Console.WriteLine("Обработка успешно завершена. Нажмите любую клавишу");
				Console.ReadKey();

			}
			catch (Exception e)
			{
				Info($"Ошибка: {e.Message}");
			}
		}

		public static void Main(string[] args)
        {
			TerminalOptions opts = new TerminalOptions();
			var parser = new CommandLine.Parser(with => with.HelpWriter = null);
			var parserResult = parser.ParseArguments<TerminalOptions>(args);
			parserResult.WithParsed(Run).WithNotParsed(errs => DisplayHelp(parserResult, errs));		
        }

		private static void DisplayHelp(ParserResult<TerminalOptions> parserResult, IEnumerable<Error> errs)
		{

			HelpText helpText = null;

			if (errs.IsVersion())
			{
				helpText = HelpText.AutoBuild(parserResult);
			}
			else
			{
				helpText = HelpText.AutoBuild(parserResult, h => {
					h.AdditionalNewLineAfterOption = false;
					h.Heading = "Утилита добавляет на фото дату и время его создания"; 
					return HelpText.DefaultParsingErrorsHandler(parserResult, h);
				}, e => e);
			}
			Console.WriteLine(helpText);
		}

		private static void drawText(Graphics g)
		{ 
			SolidBrush myBrush = new SolidBrush(Color.Red);
			g.FillRectangle(myBrush, new Rectangle(0, 0, 200, 300));
		}

		private static void Debug(string str = "")
		{
			if (IsDebug)
				Console.WriteLine(str);
		}

		private static void Info(string str)
		{
			Console.WriteLine(str);
		}

		private static readonly string[] datePatterns =
		{
			"ddd MMM dd HH:mm:ss zzz yyyy",
			"yyyy:MM:dd HH:mm:ss.fff",
			"yyyy:MM:dd HH:mm:ss",
			"yyyy:MM:dd HH:mm",
			"yyyy-MM-dd HH:mm:ss",
			"yyyy-MM-dd HH:mm",
			"yyyy.MM.dd HH:mm:ss",
			"yyyy.MM.dd HH:mm",
			"yyyy-MM-ddTHH:mm:ss.fff",
			"yyyy-MM-ddTHH:mm:ss.ff",
			"yyyy-MM-ddTHH:mm:ss.f",
			"yyyy-MM-ddTHH:mm:ss",
			"yyyy-MM-ddTHH:mm.fff",
			"yyyy-MM-ddTHH:mm.ff",
			"yyyy-MM-ddTHH:mm.f",
			"yyyy-MM-ddTHH:mm",
			"yyyy:MM:dd",
			"yyyy-MM-dd",
			"yyyy-MM",
			"yyyyMMdd", // as used in IPTC data
            "yyyy"
};
	}
}
