using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class SiglusProcessor : AbstractPjProcessor
    {
        private const string OUTPUT_FILE_EXTENSION = ".txt";
        private const string SS_TRANS_LINE_HEAD_REGEX = @"^<\d+?> ";
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aWrapFont = new Font("MotoyaLMaru", 16);
            aMaxWrap = 700;
            aWrapString = "\\n";
            base.loadDefault(forceReload);
        }

        public void simpleExport(string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            string exp = "[\uFF01-\uFF9F\u2000-\u206F\u2600-\u26FF\u3000-\u9fff\uFF00-\uFFEF]+";

            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string[] inputs = File.ReadAllLines(filePath, aInputEncoding);
                string output = "";
                bool isConcat = false;
                bool isFirst = true;
                for (int i = 0; i < inputs.Length; i++)
                {
                    Match headerMatch = Regex.Match(inputs[i], SS_TRANS_LINE_HEAD_REGEX);
                    if (headerMatch.Success)
                    {
                        string orgText = inputs[i].Substring(headerMatch.Length);

                        Match match = Regex.Match(orgText.Trim(), exp);
                        if (match.Length == 0 || match.Length < orgText.Trim().Length)
                        {
                            isConcat = false;
                            continue;
                        }

                        if (orgText.StartsWith("「") || orgText.StartsWith("『") || orgText.StartsWith("（"))
                        {
                            output += Environment.NewLine;
                        }
                        else if (!isConcat)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                output += Environment.NewLine+ Environment.NewLine+ Environment.NewLine;
                            }
                        }

                        output += orgText;

                        isConcat = inputs[i + 1].Contains("} 3e ");
                    }
                }
                if (output.Length > 0)
                {
                    output += Environment.NewLine+ Environment.NewLine;
                    String outputFile = outputDir + "\\" + Path.GetFileName(filePath);
                    File.WriteAllText(outputFile, output, aMediateEncoding);
                }
            }
        }

        public void export(string inputFile, string outputDir, string lang)
        {
            string jpCharExp = "[\uFF01-\uFF9F\u2000-\u206F\u2600-\u26FF\u3000-\u9fff\uFF00-\uFFEF]+";
            string engExp = "[.,!?\"]$";
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string[] fileContent = File.ReadAllLines(filePath, aInputEncoding);
                string exportContent = "";
                for (int i = 0; i < fileContent.Length; i++)
                {
                    Match sentenceMatch = Regex.Match(fileContent[i], SS_TRANS_LINE_HEAD_REGEX);
                    if(sentenceMatch.Success)
                    {
                        string lineHead = sentenceMatch.Value;
                        string sentence = fileContent[i].Substring(lineHead.Length);
                        if (lang.Equals("en"))
                        {
                            if (!Regex.IsMatch(sentence, engExp)) { continue; }
                        }
                        if (lang.Equals("jp"))
                        {
                            Match match = Regex.Match(sentence.Trim(), jpCharExp);
                            if (match.Length == 0 || match.Length < sentence.Trim().Length) { continue; }
                        }
                        List<string> tempBlock = new List<string>();
                        tempBlock.Add(TransCommon.TRANS_BLOCK_INFO_HEADER
                            + TransCommon.makeOneInfoStr(false, TransCommon.INFO_LINE_HEAD, (i + 1).ToString()));
                        tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + sentence);
                        tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD);
                        exportContent += TransCommon.blockToString(tempBlock);
                    }
                }
                if (exportContent.Length > 0)
                {
                    string outputPath = String.Format("{0}\\{1}"
                        , outputDir, Path.GetFileName(filePath));
                    File.WriteAllText(outputPath, exportContent, aMediateEncoding);
                }
            }
        }

        public void import(string inputDir, string orgDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string orgFilePath = orgDir;
                if (File.GetAttributes(@orgDir).HasFlag(FileAttributes.Directory))
                {
                    orgFilePath += "\\" + Path.GetFileName(fromFilePath);
                }
                string[] toFileLines = File.ReadAllLines(orgFilePath, aInputEncoding);
                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                List<List<string>> allBlocks =
                    TransCommon.getBlockText(File.ReadAllLines(fromFilePath, aMediateEncoding));
                foreach (List<string> aBlock in allBlocks)
                {
                    int orgLine = -1;
                    for (int i = 0; i < aBlock.Count; i++)
                    {
                        if (aBlock[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                        {
                            Dictionary<string, string> info = TransCommon.getInfoFromString(aBlock[i]);
                            orgLine = Int32.Parse(info[TransCommon.INFO_LINE_HEAD]) - 1;
                            continue;
                        }
                        if (aBlock[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                        {
                            string sentence = aBlock[i].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                            Match sentenceMatch = Regex.Match(toFileLines[orgLine], SS_TRANS_LINE_HEAD_REGEX);
                            toFileLines[orgLine] = sentenceMatch.Value + sentence;
                        }
                    }
                }
                File.WriteAllLines(toFilePath, toFileLines, aOutputEncoding);
            }
        }

        public void ssWrap(string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string[] fileContent = File.ReadAllLines(filePath, aMediateEncoding);
                Match tempMatch;
                for (int i = 0; i < fileContent.Length; i++)
                {
                    tempMatch = Regex.Match(fileContent[i], @"^<\d+?> ");
                    if (!tempMatch.Success) { continue; }
                    string lineHead = tempMatch.Value;
                    string sentence = fileContent[i].Substring(lineHead.Length);
                    sentence = TransCommon.formatJpSentence(sentence,
                        fileContent[i- 1].Substring(lineHead.Length+ 2));
                    sentence = textSizeWrap(sentence, aWrapFont, aMaxWrap, aWrapString, null, out _);
                    fileContent[i] = lineHead + sentence;
                }
                string outputFilePath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(filePath));
                File.WriteAllLines(outputFilePath, fileContent, aMediateEncoding);
            }
        }


    }
}
