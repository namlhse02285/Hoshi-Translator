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
                    Match headerMatch = Regex.Match(inputs[i], @"^<\d+>");
                    if (headerMatch.Success)
                    {
                        string orgText = inputs[i].Substring(headerMatch.Length+ 1);

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
                    sentence = textSizeWrap(sentence, aWrapFont, aMaxWrap, aWrapString, out _);
                    fileContent[i] = lineHead + sentence;
                }
                string outputFilePath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(filePath));
                File.WriteAllLines(outputFilePath, fileContent, aMediateEncoding);
            }
        }


    }
}
