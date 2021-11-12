using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class SonohanaProcessor : AbstractPjProcessor
    {
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aWrapFont = new Font("UVN Cat Bien", 16);
            aMaxWrap = 620;
            aWrapString = "†n";
            base.loadDefault(forceReload);
        }

        public void export(string scriptDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(scriptDir))
            {
                string[] fileLines = File.ReadAllLines(filePath, aInputEncoding);
                string orgTempText = "";
                string fileContent = "";
                for (int i = 0; i < fileLines.Length; i++)
                {
                    string tempLine = fileLines[i];
                    if(tempLine.Length== 0) { continue; }
                    if(tempLine.StartsWith("    # voice")) { continue; }
                    if(tempLine.StartsWith("    voice")) { continue; }
                    if(!tempLine.StartsWith("    ")) { continue; }
                    if (tempLine.StartsWith("    #"))
                    {
                        if (tempLine.IndexOf("\"") < 0) { continue; }
                        orgTempText = tempLine.Substring(tempLine.IndexOf("\"")+ 1);
                        orgTempText = orgTempText.Substring(0, orgTempText.Length - 1);
                        continue;
                    }
                    if (tempLine.StartsWith("    old "))
                    {
                        if (tempLine.IndexOf("\"") < 0) { continue; }
                        orgTempText = tempLine.Substring(tempLine.IndexOf("\"") + 1);
                        orgTempText = orgTempText.Substring(0, orgTempText.Length - 1);
                        continue;
                    }
                    if (tempLine.Equals("    \"\""))
                    {
                        fileContent += TransCommon.TRANS_BLOCK_INFO_HEADER
                            + TransCommon.makeOneInfoStr(false, TransCommon.INFO_LINE_HEAD, (i+ 1).ToString());
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.CHAR_NAME_LINE_HEAD;
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.ORIGINAL_LINE_HEAD + orgTempText;
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.TRANSLATED_LINE_HEAD;
                        fileContent += Environment.NewLine;
                        fileContent += Environment.NewLine;
                    }else if (tempLine.EndsWith("\"\""))
                    {
                        fileContent += TransCommon.TRANS_BLOCK_INFO_HEADER;
                        fileContent += TransCommon.makeOneInfoStr(false, TransCommon.INFO_LINE_HEAD, (i + 1).ToString());
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.CHAR_NAME_LINE_HEAD + tempLine.Substring(0, tempLine.IndexOf("\"")).Trim();
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.ORIGINAL_LINE_HEAD + orgTempText;
                        fileContent += Environment.NewLine;
                        fileContent += TransCommon.TRANSLATED_LINE_HEAD;
                        fileContent += Environment.NewLine;
                        fileContent += Environment.NewLine;
                    }
                }
                string outputPath = Path.Combine(outputDir,
                    Path.GetFileNameWithoutExtension(filePath)
                    + TransCommon.EXPORT_FILE_EXTENSION);
                File.WriteAllText(outputPath, fileContent, aMediateEncoding);
            }
        }

        public void export2(string scriptDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(scriptDir))
            {
                string[] fileLines = File.ReadAllLines(filePath, aInputEncoding);
                string fileContent = "";
                for (int i = 0; i < fileLines.Length; i++)
                {
                    string tempLine = fileLines[i];
                    tempLine = tempLine.Replace("\\\"", "†");
                    if (tempLine.Length == 0) { continue; }
                    Match match1= Regex.Match(tempLine, "    \"(.+?)\"");
                    Match match2= Regex.Match(tempLine, "    ([^ ]+?) \"(.+?)\"");
                    if (!match1.Success && !match2.Success) { continue; }
                    if(match2.Success && match2.Groups[1].Value.ToLower().Equals("voice")) { continue; }

                    fileContent += TransCommon.TRANS_BLOCK_INFO_HEADER;
                    fileContent += TransCommon.makeOneInfoStr(false, TransCommon.INFO_LINE_HEAD, (i + 1).ToString());
                    fileContent += Environment.NewLine;
                    fileContent += TransCommon.CHAR_NAME_LINE_HEAD;
                    if (match2.Success)
                    {
                        fileContent += match2.Groups[1].Value;
                    }
                    fileContent += Environment.NewLine;
                    fileContent += TransCommon.ORIGINAL_LINE_HEAD
                        + (match2.Success
                        ? match2.Groups[2].Value.Replace("†", "\\\"")
                        : match1.Groups[1].Value.Replace("†", "\\\""));
                    fileContent += Environment.NewLine;
                    fileContent += TransCommon.TRANSLATED_LINE_HEAD;
                    fileContent += Environment.NewLine;
                    fileContent += Environment.NewLine;
                }
                string outputPath = String.Format("{0}\\{1}{2}"
                        , outputDir, Path.GetFileNameWithoutExtension(filePath), TransCommon.EXPORT_FILE_EXTENSION);
                File.WriteAllText(outputPath, fileContent, aMediateEncoding);
            }
        }

        public void import(string transFiles, string orgFiles, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(transFiles))
            {
                string orgFilePath = BuCommon.searchOrgFilePath(filePath, orgFiles);

                string[] orgFileContent = File.ReadAllLines(orgFilePath, aInputEncoding);
                List<List<string>> allTransBlock = TransCommon.getBlockText(
                    File.ReadAllLines(filePath, aMediateEncoding));
                foreach (List<string> oneBlock in allTransBlock)
                {
                    Dictionary<string, string> transInfo
                        = TransCommon.getInfoFromString(oneBlock[0]);
                    string transSentence = TransCommon.getBlockSingleText(oneBlock,
                        TransCommon.TRANSLATED_LINE_HEAD, false);
                    int line = int.Parse(transInfo[TransCommon.INFO_LINE_HEAD])- 1;
                    Match orgSentenceMatch = Regex.Match(orgFileContent[line],
                        "(?<!\\\\)\".*?(?<!\\\\)\"");
                    orgFileContent[line]
                        = orgFileContent[line].Substring(0, orgSentenceMatch.Index+ 1)
                        + transSentence
                        + orgFileContent[line].Substring(
                            orgSentenceMatch.Index+ orgSentenceMatch.Length- 1);
                }
                string outputPath = Path.Combine(outputDir, Path.GetFileName(orgFilePath));
                File.WriteAllLines(outputPath, orgFileContent, aOutputEncoding);
            }
        }


    }
}
