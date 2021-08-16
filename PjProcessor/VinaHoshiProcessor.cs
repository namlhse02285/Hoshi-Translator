using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class VinaHoshiProcessor
    {
		public static readonly string INFO_SEPARATOR = ";";
		public void addVideoInfoToFileNam(string ffmpegPath, string rootDir, string outputDir)
        {
			using (Process ffmpeg = new Process())
			{
				ffmpeg.StartInfo.UseShellExecute = false;
				ffmpeg.StartInfo.ErrorDialog = false;
				ffmpeg.StartInfo.RedirectStandardError = true;
				ffmpeg.StartInfo.CreateNoWindow = true;
				ffmpeg.StartInfo.FileName = ffmpegPath;
				//Directory.CreateDirectory(outputDir);
				foreach (string filePath in BuCommon.listFiles(rootDir))
                {
					ffmpeg.StartInfo.Arguments = "-i \""+ filePath+ "\"";
					ffmpeg.Start();
					ffmpeg.WaitForExit();
					string result = ffmpeg.StandardError.ReadToEnd();

					int duration = 0;
					Match durationMatches = Regex.Match(result, @"(?<=Duration: )[^Nn].{10}");
					if(durationMatches.Success)
                    {
						duration = BuCommon.timeStringToMs(durationMatches.Value);
					}

					string width = "", height= "";
					Match sizeMatches = Regex.Match(result, @"Stream #0:0.+?,.+?, (\d+)x(\d+) ");
					if (sizeMatches.Success)
					{
						width = sizeMatches.Groups[1].Value;
						height = sizeMatches.Groups[2].Value;
					}

					string mediaInfo = "";
					if (duration > 0)
					{
						mediaInfo += INFO_SEPARATOR + "t=" + duration;
					}
					if (width.Length > 0 && height.Length> 0)
					{
						mediaInfo += INFO_SEPARATOR + "w=" + width;
						mediaInfo += INFO_SEPARATOR + "h=" + height;
					}

					string newDir = Path.GetDirectoryName(filePath).Replace(rootDir, outputDir);
					string newFileName = Path.GetFileNameWithoutExtension(filePath).IndexOf(INFO_SEPARATOR) > 0
						? Path.GetFileNameWithoutExtension(filePath)
						  .Substring(0, Path.GetFileNameWithoutExtension(filePath).IndexOf(INFO_SEPARATOR))
						: Path.GetFileNameWithoutExtension(filePath);
					string newFilePath
						= newDir + "\\"
						+ newFileName
						+ mediaInfo
						+ Path.GetExtension(filePath);
					if (!Directory.Exists(newDir)) { Directory.CreateDirectory(newDir); }
					Console.WriteLine(newFilePath);
					File.Copy(filePath, newFilePath);
				}
			}
		}

		public void addImageInfoToFileNam(string rootDir, string outputDir)
		{
			foreach (string filePath in BuCommon.listFiles(rootDir))
            {
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					try
					{
						Image image = Image.FromStream(fileStream, false, false);
					}
					catch (Exception e) { continue; }
					using (Image image = Image.FromStream(fileStream, false, false))
					{
						int height = image.Height;
						int width = image.Width;
						string newDir = Path.GetDirectoryName(filePath).Replace(rootDir, outputDir);
						string newFileName = Path.GetFileNameWithoutExtension(filePath).IndexOf(INFO_SEPARATOR) > 0
							? Path.GetFileNameWithoutExtension(filePath)
							  .Substring(0, Path.GetFileNameWithoutExtension(filePath).IndexOf(INFO_SEPARATOR))
							: Path.GetFileNameWithoutExtension(filePath);
						string newFilePath
							= newDir + "\\"
							+ newFileName
							+ INFO_SEPARATOR + "w=" + width
							+ INFO_SEPARATOR + "h=" + height
							+ Path.GetExtension(filePath);
						if (!Directory.Exists(newDir)) { Directory.CreateDirectory(newDir); }
						Console.WriteLine(newFilePath);
						File.Copy(filePath, newFilePath);
					}
				}
			}
		}

		public void generateMagicPath(string rootDir)
        {
			Encoding outputEncoding = BuCommon.getEncodingFromString("utf-8-bom");
			File.WriteAllText(AppConst.OUTPUT_FILE, "", outputEncoding);
			foreach (string filePath in BuCommon.listFiles(rootDir))
            {
				int separatorIndex = filePath.LastIndexOf("\\")
					+ filePath.Substring(filePath.LastIndexOf("\\")).IndexOf(";");
				if (separatorIndex <= filePath.LastIndexOf("\\"))
				{
					separatorIndex = filePath.LastIndexOf(".");
				}
				string rootParent = rootDir.Substring(0, rootDir.LastIndexOf("\\")+ 1);
				File.AppendAllText(AppConst.OUTPUT_FILE, String.Format("{0}|=|{1}"
					, filePath.Substring(rootParent.Length, separatorIndex- rootParent.Length).Replace("\\", "/")
					, filePath.Substring(rootParent.Length).Replace("\\", "/")) + Environment.NewLine
					, outputEncoding);
			}

		}


	}
}
