using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class VinaHoshiProcessor
    {
		public static readonly string INFO_SEPARATOR = ";";
		public static readonly string BETWEEN_SCRIPT_COMMAND_SEPARATOR = " -> ";
		public static readonly Encoding COMMON_ENCODING = BuCommon.getEncodingFromString("utf-8");
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

		private static string storageToBody(string storage)
        {
			string ret = storage.Substring(storage.LastIndexOf("/") + 1);
			ret = ret.Substring(0, ret.LastIndexOf("."));
			return ret.ToLower();
		}
		public void parseOneNightCrossScript(string inputDir, string outputDir)
        {
			Directory.CreateDirectory(outputDir);
			foreach (string filePath in BuCommon.listFiles(inputDir))
            {
				string[] fileContent = File.ReadAllLines(filePath, COMMON_ENCODING);
				string parseContent = ""; string tempCharName = "";
				List<string> charOnScene = new List<string>();

				for (int i = 0; i < fileContent.Length; i++)
                {
					string line = fileContent[i].Trim();
					string pureTxt = Regex.Replace(line, @"\[.+?\]", "");
                    if (line.StartsWith("#"))
                    {
						string newName = line.Substring(1);
                        if (!newName.Equals(tempCharName)
							&& tempCharName.Length> 0
							&& charOnScene.Contains(tempCharName))
                        {
							parseContent += Environment.NewLine;
							parseContent += "img; name=" + tempCharName
								+ "; action=animate; type=filter; color=80000000; time=500";
						}
						tempCharName = newName;
						continue;
					}

					MatchCollection allCodeMatch = Regex.Matches(line, @"\[.+?\]");
					foreach(Match commandMatch in allCodeMatch)
                    {
						string command = commandMatch.Value.Substring(1, commandMatch.Length- 2);
						if (command.StartsWith("tb_show_message_window"))
						{
							parseContent += Environment.NewLine;
							parseContent += "txt; action=animate; type=fade; alpha=1; time=500";
							parseContent += Environment.NewLine;
							parseContent += "txt; action=animate; type=move; bottom=0; time=500";
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("tb_hide_message_window"))
						{
							parseContent += Environment.NewLine;
							parseContent += "txt; action=animate; type=fade; alpha=0; time=500";
							parseContent += Environment.NewLine;
							parseContent += "txt; action=animate; type=move; bottom=-20; time=500";
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("mask_off"))
						{
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							parseContent += Environment.NewLine;
							parseContent += "img; name=bover; action=animate; type=fade; alpha=0; time=" + time;
							parseContent += " -> img; name=bover; action=remove";
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("mask "))
						{
							parseContent += Environment.NewLine;
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							parseContent += "img; action=new; name=bover; type=back; layer=over; path=black";
							parseContent += " -> img; name=bover; action=animate; type=fade; alpha=1; time=" + time;
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("chara_show"))
						{
							parseContent += Environment.NewLine;
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							string name = TransCommon.htmlStyleGetProperty(line, "name");
							string body = TransCommon.htmlStyleGetProperty(line, "storage");
							body = storageToBody(body);
							string path = body.Substring(0, body.IndexOf("_"));
							int width = TransCommon.htmlStyleGetIntProperty(line, "width");
							int left = TransCommon.htmlStyleGetIntProperty(line, "left");
							left = (int)Math.Round((left + width / 2) * 100.0 / 1280);
							parseContent += String.Format("img; action=new; name={0}; type=char; path={1}; body={2}; left={3}; bottom=-10; color=80000000"
								, name, path, body, left);
							parseContent += String.Format(" -> img; name={0}; action=animate; type=fade; alpha=1; time={1}"
								, name, time);
							parseContent += Environment.NewLine;
							charOnScene.Add(name);
						}
						else if (command.StartsWith("chara_mod"))
						{
							parseContent += Environment.NewLine;
							string name = TransCommon.htmlStyleGetProperty(line, "name");
							string body = TransCommon.htmlStyleGetProperty(line, "storage");
							body = storageToBody(body);
							parseContent += String.Format("img; action=mod; name={0}; body={1}"
								, name, body);
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("chara_move"))
						{
							parseContent += Environment.NewLine;
							string name = TransCommon.htmlStyleGetProperty(line, "name");
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							int width = TransCommon.htmlStyleGetIntProperty(line, "width");
							int left = TransCommon.htmlStyleGetIntProperty(line, "left");
							string curve = TransCommon.htmlStyleGetProperty(line, "effect");
							left = (int)Math.Round((left + width / 2) * 100.0 / 1280);
							parseContent += String.Format("img; action=animate; name={0}; type=move; left={1}; time={2}; curve={3}"
								, name, left, time, curve);
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("chara_hide_all"))
						{
							parseContent += Environment.NewLine;
							parseContent += String.Format("layer; name=char; action=clear");
							parseContent += Environment.NewLine;
							charOnScene.Clear();
						}
						else if (command.StartsWith("chara_hide"))
						{
							parseContent += Environment.NewLine;
							string name = TransCommon.htmlStyleGetProperty(line, "name");
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							parseContent += String.Format("img; action=animate; type=fade; name={0}; alpha=0; time={1}"
								, name, time);
							parseContent += String.Format(" -> img; name={0}; action=remove"
								, name);
							parseContent += Environment.NewLine;
							charOnScene.Remove(name);
						}
						else if (command.StartsWith("jump "))
						{
							parseContent += Environment.NewLine;
							string file = TransCommon.htmlStyleGetProperty(line, "storage");
							if(file.Length> 0)
                            {
								file = file.Substring(0, file.LastIndexOf("."));
							}
							parseContent += String.Format("label; action=jump; file={0}", file);
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("stopbgm "))
						{
							parseContent += Environment.NewLine;
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							parseContent += String.Format("sound; type=back; name=bgm; path=");
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("playbgm "))
						{
							parseContent += Environment.NewLine;
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							string path = TransCommon.htmlStyleGetProperty(line, "storage");
							path = path.Substring(0, path.LastIndexOf("."));
							parseContent += String.Format("sound; name=bgm; type=back; path=" + path);
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("playse "))
						{
							parseContent += Environment.NewLine;
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							string path = TransCommon.htmlStyleGetProperty(line, "storage");
							path = path.Substring(0, path.LastIndexOf("."));
							parseContent += String.Format("sound; type=se; path=" + path);
							parseContent += Environment.NewLine;
						}
						else if (command.StartsWith("bg "))
						{
							parseContent += Environment.NewLine;
							string path = TransCommon.htmlStyleGetProperty(line, "storage");
							path = path.Substring(0, path.LastIndexOf("."));
							string time = TransCommon.htmlStyleGetProperty(line, "time");
							parseContent += String.Format("img; action=new; type=back; name=bgover; path={0}"
								, path);
							parseContent += String.Format(" -> img; action=animate; type=fade; name=bgover; alpha=1; time={0}"
								, time);
							parseContent += String.Format(" -> img; action=mod; name=bg; path={0}"
								, path);
							parseContent += " -> img; name=bgover; action=remove";
							parseContent += Environment.NewLine;
						}
					}
					if (pureTxt.Length > 0)
					{
						//Dim/undim character
						if (tempCharName.Length > 0 && charOnScene.Contains(tempCharName))
						{
							parseContent += Environment.NewLine;
							parseContent += "img; name=" + tempCharName
								+ "; action=animate; type=filter; color=00000000; time=500";
						}
						//Character say
						parseContent += Environment.NewLine;
						parseContent += "txt; ";
						if(tempCharName.Length> 0)
                        {
							parseContent += "name=" + tempCharName + "; ";
						}
						string convertTxt = line
							.Replace("[font color=red]", "<color=CB0C00>")
							.Replace("[resetfont]", "</color>");
						convertTxt = Regex.Replace(convertTxt, @"\[.+?\]", "");
						parseContent += "jp=" + convertTxt + ";vi=";
						parseContent += Environment.NewLine;
					}
				}

				if (parseContent.Length > 0)
				{
					string outputPath = String.Format("{0}\\{1}{2}"
						, outputDir, Path.GetFileNameWithoutExtension(filePath), TransCommon.EXPORT_FILE_EXTENSION);
					File.WriteAllText(outputPath, parseContent, COMMON_ENCODING);
				}
			}
		}

		private string getKanaFromHtml(string htmlRespond)
        {
			MatchCollection matchCollection = Regex.Matches(
				htmlRespond, @">[^<> 【】]+? 【[^【】<>]+?】");
			int lastIndex = 0;
			string ret = "";
			List<string> kanjiList = new List<string>();
			List<string> hiraOfKanjiList = new List<string>();
			List<bool> isStartKanjiBlockTextList = new List<bool>();
			foreach (Match match in matchCollection)
			{
				isStartKanjiBlockTextList.Add(htmlRespond
					.Substring(lastIndex, match.Index - lastIndex)
					.Contains("class=\"gloss-rtext\""));
				kanjiList.Add(match.Value.Substring(1, match.Value.IndexOf("【")- 1).Trim());
				hiraOfKanjiList.Add(Regex.Match(match.Value, @"(?<=【).+?(?=】)").Value.Trim());
				lastIndex = match.Index + match.Length;
			}
			for (int i = 0; i < kanjiList.Count; i++)
			{
				if(i+ 1 < kanjiList.Count && kanjiList[i+ 1].Substring(0, 1)
					.Equals(kanjiList[i].Substring(0, 1)))
				{
					ret += kanjiList[i] + ": " + hiraOfKanjiList[i]+ " | ";
				}
                else
                {
					ret += "<click=copy>" + kanjiList[i] + "</click>: " + hiraOfKanjiList[i] + "<br>";
				}
			}

			return ret;
		}
		public void addKana(string inputDir, string outputDir)
        {
			Directory.CreateDirectory(outputDir);
			foreach (string filePath in BuCommon.listFiles(inputDir))
            {
				string[] fileContent = File.ReadAllLines(filePath, COMMON_ENCODING);
				string parseContent = "";

				for (int i = 0; i < fileContent.Length; i++)
                {
					string line = fileContent[i].Trim();
					line = Regex.Replace(line, @"\/\/.+", "");
					if (line.Length == 0) { continue; }

					string[] commandInLine = Regex.Split(line, BETWEEN_SCRIPT_COMMAND_SEPARATOR);
					for (int j = 0; j < commandInLine.Length; j++)
					{
						if (commandInLine[j].StartsWith("txt"))
						{
							Match jpTextMatch = Regex.Match(commandInLine[j], @"(?<=jp=)[^;]+?(?=;)");
							string jpText = jpTextMatch.Value;
							jpText = Regex.Replace(jpText, "<[^<>]+?>", "");

							WebRequest request = WebRequest.Create("https://ichi.moe/cl/qr/?q=" + jpText + "&r=htr");
							request.Credentials = CredentialCache.DefaultCredentials;
							WebResponse response = request.GetResponse();
							string responseFromServer = "";

							using (Stream dataStream = response.GetResponseStream())
							{
								StreamReader reader = new StreamReader(dataStream);
								responseFromServer = reader.ReadToEnd();

							}
							response.Close();

                            if (commandInLine[j].Contains(";vi="))
                            {
								commandInLine[j] =
									commandInLine[j].Substring(0, commandInLine[j].IndexOf(";vi="))+
									";kana="+ getKanaFromHtml(responseFromServer)+
									commandInLine[j].Substring(commandInLine[j].IndexOf(";vi="));
                            }
                            else
                            {
								commandInLine[j] += ";kana=" + getKanaFromHtml(responseFromServer);
							}
						}
					}
					fileContent[i] = String.Join(BETWEEN_SCRIPT_COMMAND_SEPARATOR, commandInLine);
				}

				string outputPath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(filePath));
				File.WriteAllLines(outputPath, fileContent, COMMON_ENCODING);
			}

		}
		private string genKanaBlock(Dictionary<string, string> headerInfo, string splittedJpText)
        {
			if (splittedJpText.Length == 0) { return ""; }
			string ret = "";
			List<string> tempBlock = new List<string>();

			tempBlock.Add(TransCommon.genInfoString(headerInfo));
			tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + splittedJpText);
			tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD);
			tempBlock.Add("");

			foreach (string blockLine in tempBlock)
			{
				ret += blockLine + Environment.NewLine;
			}

			return ret;
		}


	}
}
