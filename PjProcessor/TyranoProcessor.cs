using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class TyranoProcessor : AbstractPjProcessor
    {
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8");
            aWrapFont = new Font("MotoyaLMaru", 16);
            aMaxWrap = 63;
            aWrapString = "[r]\n";
            base.loadDefault(forceReload);
        }

        public void simpleExport(string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string[] inputs = File.ReadAllLines(filePath, aInputEncoding);
                string output = "";
                bool isConcat = false;
                for (int i = 0; i < inputs.Length; i++)
                {
                    string filteredLine = Regex.Replace(inputs[i], @"\[.+?\]","").Trim();
                    if (filteredLine.Length== 0) { continue; }
                    if (filteredLine.StartsWith("*")) { continue; }
                    if (filteredLine.StartsWith(";")) { continue; }
                    if (filteredLine.StartsWith("$")) { continue; }
                    if (filteredLine.StartsWith("#"))
                    {
                        string charName = filteredLine.Substring(1);
                        if(charName.Length> 0)
                        {
                            output += charName+ Environment.NewLine;
                        }
                        continue;
                    }
                    if (inputs[i].EndsWith("[r]"))
                    {
                        isConcat = true;
                    }
                    if (inputs[i].EndsWith("[p]"))
                    {
                        isConcat = false;
                    }
                    if(filteredLine.StartsWith("_　"))
                    {
                        filteredLine = filteredLine.Substring("_　".Length);
                    }

                    output += filteredLine + (isConcat ? "" : (Environment.NewLine+ Environment.NewLine));
                }
                if (output.Length > 0)
                {
                    output += Environment.NewLine + Environment.NewLine;
                    String outputFile = outputDir + "\\" + Path.GetFileNameWithoutExtension(filePath)+ ".txt";
                    File.WriteAllText(outputFile, output, aMediateEncoding);
                }
            }
        }
    }
}
