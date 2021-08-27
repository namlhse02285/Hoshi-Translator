using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class KirikiriProcessor : AbstractPjProcessor
    {
        public static readonly string INFO_MODE_ADV = "adv";
        public static readonly string INFO_MODE_NOVEl = "novel";

        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("shift-jis");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aOutputEncoding = BuCommon.getEncodingFromString("ucs-2");
            aWrapFont = new Font("MontserratViJp", 16);
            aMaxWrap = 620;
            aWrapString = "\r\n";
            base.loadDefault(forceReload);
        }

        public void export(string dataDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(dataDir))
            {
                string exportContent = "";
                string[] fileContent = File.ReadAllLines(filePath, aInputEncoding);
                string mode = "";
                for (int i = 0; i < fileContent.Length; i++) {
                    string lineToProcess = fileContent[i].Trim();
                    lineToProcess = Regex.Replace(lineToProcess, @";.*", "");
                    lineToProcess = Regex.Replace(lineToProcess, @"^@.*", "");
                    lineToProcess = Regex.Replace(lineToProcess, @"^\*.*", "");
                    if (lineToProcess.Length == 0) { continue; }

                    Dictionary<string, string> headerInfo = new Dictionary<string, string>();
                    headerInfo.Add(TransCommon.INFO_LINE_HEAD, (i + 1).ToString());

                    if (Regex.IsMatch(lineToProcess, @"^\[[^\[\]]+?\]$"))
                    {
                        if ("[ノベルモード]".Equals(lineToProcess))
                        {
                            mode = INFO_MODE_NOVEl;
                        }
                        if ("[通常モード]".Equals(lineToProcess))
                        {
                            mode = "";
                        }
                        continue;
                    }
                    if (mode.Length> 0)
                    {
                        headerInfo.Add(TransCommon.INFO_MODE_HEAD, mode);
                    }
                    exportContent += genOneBlock(headerInfo, fileContent[i]);
                }
                if (exportContent.Length > 0)
                {
                    string outputPath = String.Format("{0}\\{1}{2}"
                        , outputDir, Path.GetFileNameWithoutExtension(filePath), TransCommon.EXPORT_FILE_EXTENSION);
                    File.WriteAllText(outputPath, exportContent, aMediateEncoding);
                }
            }
        }
        private string genOneBlock(Dictionary<string, string> headerInfo, string orgText)
        {
            string ret = "";
            List<string> tempBlock = new List<string>();

            tempBlock.Add(TransCommon.genInfoString(headerInfo));
            tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + orgText);
            tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD);
            tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD);
            tempBlock.Add("");

            foreach (string blockLine in tempBlock)
            {
                ret += blockLine + Environment.NewLine;
            }

            return ret;
        }

        public void concat(string inputDir, string outputDir, string[] charNamePattern)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                    {
                        Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(inputFileArr[i]);
                        int fileLine = Int32.Parse(headerInfo[TransCommon.INFO_LINE_HEAD]);
                        string orgTxt = inputFileArr[i + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                        orgTxt = removeCodeFromText(orgTxt);
                        inputFileArr[i + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;

                        //Char name detect, ignore concat sentence if detected
                        bool isCharNameLine = false;
                        foreach (string pattern in charNamePattern)
                        {
                            if (Regex.IsMatch(orgTxt, pattern))
                            {
                                inputFileArr[i + 3] = TransCommon.TRANSLATED_LINE_HEAD + orgTxt;
                                isCharNameLine = true;
                                break;
                            }
                        }
                        if (isCharNameLine){continue;}
                        //Char name detect, END

                        int concated = 0;
                        for (int j = i + 1; j < inputFileArr.Length; j++)
                        {
                            if (inputFileArr[j].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                            {
                                Dictionary<string, string> headerInfo2 = TransCommon.getInfoFromString(inputFileArr[j]);
                                int fileNextLine = Int32.Parse(headerInfo2[TransCommon.INFO_LINE_HEAD]);
                                if (fileNextLine - fileLine > 1) { break; }
                                fileLine = fileNextLine;
                                inputFileArr[j + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD;

                                string nextOrgTxt = inputFileArr[j + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                                nextOrgTxt = removeCodeFromText(nextOrgTxt);
                                inputFileArr[i + 2] += nextOrgTxt;
                                concated = j+1;
                            }
                        }
                        if (concated> 0) { i = concated; }
                    }
                }

                //if (changed)
                //{
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, inputFileArr, aMediateEncoding);
                //}
            }
        }

        public void concat2(string inputDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                    {
                        Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(inputFileArr[i]);
                        int fileLine = Int32.Parse(headerInfo[TransCommon.INFO_LINE_HEAD]);
                        string orgTxt = inputFileArr[i + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                        string charName = "";
                        Match charNameMatch = Regex.Match(orgTxt, @"\[nm [^\[\]]+?\]");
                        if (charNameMatch.Success)
                        {
                            charName = TransCommon.htmlStyleGetProperty(charNameMatch.Value, "t");
                        }

                        orgTxt = removeCodeFromText(orgTxt);
                        inputFileArr[i + 2] = (charName.Length== 0 ? ""
                            : (TransCommon.CHAR_NAME_LINE_HEAD+ charName+ Environment.NewLine))
                            + TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;
                    }
                }

                //if (changed)
                //{
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, inputFileArr, aMediateEncoding);
                //}
            }
        }
        private string removeCodeFromText(string input)
        {
            string ret = "";
            MatchCollection allCode = Regex.Matches(input, @"\[.+?\]");
            if (allCode.Count == 0) { return input; }
            List<Match> sortMatches = new List<Match>();

            foreach(Match oneMatch in allCode) { sortMatches.Add(oneMatch); }
            sortMatches.Sort(delegate (Match x, Match y){
                return x.Index.CompareTo(y.Index);
            });
            int lastIndex = 0;
            foreach(Match oneMatch in sortMatches)
            {
                ret += input.Substring(lastIndex, oneMatch.Index- lastIndex);
                lastIndex = oneMatch.Index + oneMatch.Length;
                Match matchFurigana1 = Regex.Match(oneMatch.Value, @"\[(.+?)\(.+?\)\]");
                if (matchFurigana1.Success)
                {
                    ret += matchFurigana1.Groups[1].Value;
                    continue;
                }
                Match matchFurigana2 = Regex.Match(oneMatch.Value, @"\[(.+?)'.+?\]");
                if (matchFurigana2.Success)
                {
                    ret += matchFurigana2.Groups[1].Value;
                    continue;
                }
                Match matchFurigana3 = Regex.Match(oneMatch.Value, @"\[LRB .+?\]");
                if (matchFurigana3.Success)
                {
                    ret += matchFurigana3.Value;
                    continue;
                }
            }
            ret += input.Substring(lastIndex);
            return ret;
        }


    }
}
