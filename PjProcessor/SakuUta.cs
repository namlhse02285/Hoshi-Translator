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
    class SakuUta : AbstractPjProcessor
    {
        private readonly string SLASH_REPLACE_STRING = "†";
        private readonly string NEW_TEXT_BOX_HEADER = "　";
        private readonly string ORG_LINE_BREAK = "\\n";
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8");
            aWrapFont = new Font("MotoyaLMaru", 16);
            aMaxWrap = 620;
            aWrapString = "†n";
            base.loadDefault(forceReload);
        }
        public void cuuVan(string orgFile, string failFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            string[] orgFileLines = File.ReadAllLines(orgFile, aInputEncoding);
            string[] failFileLines = File.ReadAllLines(failFile, aInputEncoding);
            int orgBlockCount = 0;
            for (int i = 0; i < orgFileLines.Length; i++)
            {
                string orgLine = orgFileLines[i];
                if (orgLine.StartsWith("//TEXT"))
                {
                    string failLine = failFileLines[orgBlockCount];
                    if (failLine.StartsWith(NEW_TEXT_BOX_HEADER))
                    {
                        failLine = failLine.Substring(NEW_TEXT_BOX_HEADER.Length);
                    }
                    orgBlockCount++;
                    
                    Match header = Regex.Match(orgFileLines[i + 2], @"^<.+?>");
                    //string jpText = orgFileLines[i + 2].Substring(header.Length);
                    orgFileLines[i + 2] = header.Value + failLine;
                }
            }

            string outputPath = String.Format("{0}\\{1}"
                        , outputDir, Path.GetFileName(orgFile));
            File.WriteAllLines(outputPath, orgFileLines, aMediateEncoding);
        }

        public void import(string transDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(transDir))
            {
                string[] fileLines = File.ReadAllLines(filePath, aInputEncoding);
                for (int i = 0; i < fileLines.Length; i++)
                {
                    string orgLine = fileLines[i];
                    if (orgLine.StartsWith("//TEXT"))
                    {
                        Match jpHeader = Regex.Match(fileLines[i + 1], @"^<.+?>");
                        string jpTxt = fileLines[i + 1].Substring(jpHeader.Length);
                        Match viHeader = Regex.Match(fileLines[i + 2], @"^<.+?>");
                        string viTxt = fileLines[i + 2].Substring(viHeader.Length);
                        if (jpTxt.StartsWith(NEW_TEXT_BOX_HEADER))
                        {
                            viTxt = NEW_TEXT_BOX_HEADER + viTxt;
                        }
                        if (jpTxt.EndsWith(ORG_LINE_BREAK))
                        {
                            viTxt += ORG_LINE_BREAK;
                        }
                        viTxt = viTxt.Replace(SLASH_REPLACE_STRING, "\\");
                        fileLines[i + 2] = viHeader.Value + viTxt;
                    }
                }

                string outputPath = String.Format("{0}\\{1}"
                        , outputDir, Path.GetFileName(filePath));
                File.WriteAllLines(outputPath, fileLines, aOutputEncoding);
            }
        }
    }
}
