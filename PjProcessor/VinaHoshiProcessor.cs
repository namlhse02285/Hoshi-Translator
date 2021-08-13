using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class VinaHoshiProcessor
    {

        public void addMediaInfoToFileNam(string ffmpegPath, string rootDir, string separator, string outputDir)
        {
			using (Process ffmpeg = new Process())
			{
				String result;  // temp variable holding a string representation of our video's duration
				StreamReader errorreader;  // StringWriter to hold output from ffmpeg

				ffmpeg.StartInfo.UseShellExecute = false;
				ffmpeg.StartInfo.ErrorDialog = false;
				ffmpeg.StartInfo.RedirectStandardError = true;
				ffmpeg.StartInfo.FileName = ffmpegPath;
				//Directory.CreateDirectory(outputDir);
				foreach (string filePath in BuCommon.listFiles(rootDir))
                {
					ffmpeg.StartInfo.Arguments = "-i \""+ filePath+ "\"";
					ffmpeg.Start();
					errorreader = ffmpeg.StandardError;
					ffmpeg.WaitForExit();
					result = errorreader.ReadToEnd();

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
						mediaInfo += separator + "t=" + duration;
					}
					if (width.Length > 0 && height.Length> 0)
					{
						mediaInfo += separator + "w=" + width;
						mediaInfo += separator + "h=" + height;
					}

					Console.WriteLine(mediaInfo);
				}
			}
		}
    }
}
