using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class PropellerProcessor : AbstractPjProcessor
    {
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("shift-jis");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("shift-jis");
            aWrapFont = new Font("MotoyaLMaru", 16);
            aMaxWrap = 63;
            aWrapString = "_r";
            base.loadDefault(forceReload);
        }

        public void simpleExport(string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string[] inputs = File.ReadAllLines(filePath, aInputEncoding);
                string output = "";
                for (int i = 0; i < inputs.Length; i++)
                {
                    string filteredLine = inputs[i];
                    if (filteredLine.Length == 0) { continue; }
                    Match txtMatch = Regex.Match(filteredLine, "05 00 \\[.+?\"(.+?)\"");
                    if (!txtMatch.Success) { continue; }

                    string txt = txtMatch.Groups[1].Value;
                    string charName = Regex.Match(txt, "【.+】").Value;
                    if(charName.Length> 0)
                    {
                        output += charName + Environment.NewLine;
                        output += txt.Substring(charName.Length) + Environment.NewLine + Environment.NewLine;
                    }
                    else
                    {
                        output += txt + Environment.NewLine + Environment.NewLine;
                    }
                }
                if (output.Length > 0)
                {
                    output += Environment.NewLine + Environment.NewLine;
                    String outputFile = outputDir + "\\" + Path.GetFileName(filePath);
                    File.WriteAllText(outputFile, output, aMediateEncoding);
                }
            }
        }
    }
}
