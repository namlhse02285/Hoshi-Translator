using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Hoshi_Translator.PjProcessor
{
    class ChaosProcessor : AbstractPjProcessor
    {
        private int SIZE_OF_CHARACTER_I;

        public static readonly string INFO_MODE_HEAD = "Mode";
        public static readonly string INFO_MODE_HOVER = "hover";
        public static readonly string INFO_LINE_HEAD = "Line";

        private List<char> listCharVi = new List<char>()
        {
            'á','à','ả','ã','ạ','â','ấ','ầ','ẩ','ẫ','ậ','ă','ắ','ằ','ẳ','ẵ','ặ','é','è','ẻ','ẽ','ẹ','ê','ế','ề','ể','ễ','ệ','í','ì','ỉ','ĩ','ị','ú','ù','ủ','ũ','ụ','ư','ứ','ừ','ử','ữ','ự','ó','ò','ỏ','õ','ọ','ô','ố','ồ','ổ','ỗ','ộ','ơ','ớ','ờ','ở','ỡ','ợ','ý','ỳ','ỷ','ỹ','ỵ','đ','Á','À','Ả','Ã','Ạ','Â','Ấ','Ầ','Ẩ','Ẫ','Ậ','Ă','Ắ','Ằ','Ẳ','Ẵ','Ặ','É','È','Ẻ','Ẽ','Ẹ','Ê','Ế','Ề','Ể','Ễ','Ệ','Í','Ì','Ỉ','Ĩ','Ị','Ú','Ù','Ủ','Ũ','Ụ','Ư','Ứ','Ừ','Ử','Ữ','Ự','Ó','Ò','Ỏ','Õ','Ọ','Ô','Ố','Ồ','Ổ','Ỗ','Ộ','Ơ','Ớ','Ờ','Ở','Ỡ','Ợ','Ý','Ỳ','Ỷ','Ỹ','Ỵ','Đ','ü','ä','ö','ß'
        };
        private List<char> listCharJp = new List<char>()
        {
            'ぁ','あ','ぃ','い','ぅ','う','ぇ','え','ぉ','お','か','が','き','ぎ','く','ぐ','け','げ','こ','ご','さ','ざ','し','じ','す','ず','せ','ぜ','そ','ぞ','た','だ','ち','ぢ','っ','つ','づ','て','で','と','ど','な','に','ぬ','ね','の','は','ば','ぱ','ひ','び','ぴ','ふ','ぶ','ぷ','へ','べ','ぺ','ほ','ぼ','ぽ','ま','み','む','め','も','ゃ','ァ','ア','ィ','イ','ゥ','ウ','ェ','エ','ォ','オ','カ','ガ','キ','ギ','ク','グ','ケ','ゲ','コ','ゴ','サ','ザ','シ','ジ','ス','ズ','セ','ゼ','ソ','ゾ','タ','ダ','チ','ヂ','ッ','ツ','ヅ','テ','デ','ト','ド','ナ','ニ','ヌ','ネ','ノ','ハ','バ','パ','ヒ','ビ','ピ','フ','ブ','プ','ヘ','ベ','ペ','ホ','ボ','ポ','マ','ミ','ム','メ','モ','ャ','や','ゅ','ゆ','ょ'
        };
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("shift-jis");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8-bom");
            aOutputEncoding = BuCommon.getEncodingFromString("shift-jis");
            aWrapFont = new Font("Noto Sans VN Mod", 40);
            SIZE_OF_CHARACTER_I = TextRenderer.MeasureText("iiiiiii", aWrapFont).Width
                 - TextRenderer.MeasureText("iiiiii", aWrapFont).Width;
            aMaxWrap = SIZE_OF_CHARACTER_I * 133;
            aWrapString = "<br>";
            base.loadDefault(forceReload);
        }

        public void concat(string inputDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);

                List<List<string>> fileBlocks = TransCommon.getBlockText(inputFileArr);
                List<List<string>> newfileBlocks= new List<List<string>>();
                int delta = 0;
                for (int i = 0; i < fileBlocks.Count; i++)
                {
                    newfileBlocks.Add(fileBlocks[i]);
                    if (delta > 0)
                    {
                        newfileBlocks[i][2] = TransCommon.TRANSLATED_LINE_HEAD;
                        delta--;
                        continue;
                    }
                    Dictionary<string, string> info = TransCommon.getInfoFromString(fileBlocks[i][0]);
                    if (info[INFO_MODE_HEAD] == TransCommon.INFO_MODE_CHARACTER_NAME) { continue; }
                    if (info[INFO_MODE_HEAD] == INFO_MODE_HOVER) { continue; }
                    if (info[INFO_MODE_HEAD].EndsWith("wnd_comment")) { continue; }
                    int curLineCount = Int32.Parse(info[INFO_LINE_HEAD]);
                    string curLineContent = TransCommon.getBlockSingleText(
                        fileBlocks[i], TransCommon.TRANSLATED_LINE_HEAD, false);
                    if (curLineContent.EndsWith(";")) { continue; }

                    if (curLineContent.EndsWith("<br>"))
                    {
                        newfileBlocks[i][2] = newfileBlocks[i][2].Substring(0, newfileBlocks[i][2].Length - 4);
                        continue;
                    }
                    do
                    {
                        delta++;
                        if (i + delta >= fileBlocks.Count) { delta--; break; }
                        Dictionary<string, string> nextInfo =
                            TransCommon.getInfoFromString(fileBlocks[i+ delta][0]);
                        int nextLineCount = Int32.Parse(nextInfo[INFO_LINE_HEAD]);
                        if(nextLineCount- curLineCount== delta)
                        {
                            string nextLineContent= TransCommon.getBlockSingleText(
                                fileBlocks[i + delta], TransCommon.TRANSLATED_LINE_HEAD, false);
                            curLineContent += " " + nextLineContent;
                        }
                        else
                        {
                            delta--;
                            break;
                        }
                    } while (true);
                    newfileBlocks[i][2] = TransCommon.TRANSLATED_LINE_HEAD+ curLineContent;
                }

                string newFileContent = "";
                foreach (List<string> newfileBlock in newfileBlocks)
                {
                    newFileContent += TransCommon.blockToString(newfileBlock);
                }
                File.WriteAllText(toFilePath, newFileContent, aMediateEncoding);
            }
        }

        public void wrap(string inputDir, string outputDir, string wrapReplaceFilePath)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                string newFileContent = "";

                List<List<string>> fileBlocks = TransCommon.getBlockText(inputFileArr);
                foreach(List<string> aBlock in fileBlocks)
                {
                    int wrapMax = -1;
                    Dictionary<string, string> info = TransCommon.getInfoFromString(aBlock[0]);
                    List<string> newBlockContent = aBlock;
                    switch (info[INFO_MODE_HEAD])
                    {
                        case "box01":
                        case "@box01":
                            wrapMax = 1950;
                            break;
                        case "box02":
                        case "@box02":
                            wrapMax = 2060;//140
                            break;
                        case "box03":
                        case "@box03":
                            wrapMax = 225 * SIZE_OF_CHARACTER_I;//195
                            break;
                        case "box04":
                        case "@box04":
                            wrapMax = 1670;//112
                            break;
                        case "wnd_comment":
                        case "@wnd_comment":
                            wrapMax = 1516;//104
                            break;
                        case "character_name":
                            wrapMax = -1;
                            break;
                        default:
                            wrapMax = 1970;
                            break;
                    }
                    for (int i = 0; i < aBlock.Count; i++)
                    {
                        if (aBlock[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                        {
                            string line = aBlock[i].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                            string lineTrim = line.Trim();
                            if (lineTrim.EndsWith(";"))
                            {
                                MatchCollection stringMatch = Regex.Matches(line, "\".*?\"");
                                foreach (Match m in stringMatch)
                                {
                                    string sentence = m.Value.Substring(1, m.Value.Length- 2);
                                    sentence= textSizeWrap(sentence, aWrapFont, wrapMax, aWrapString, wrapReplaceFilePath, out _);
                                    line = line.Replace(m.Value, "\""+ sentence + "\"");
                                }
                                newBlockContent[i] = TransCommon.TRANSLATED_LINE_HEAD + line;
                            }
                            else
                            {
                                newBlockContent[i] = TransCommon.TRANSLATED_LINE_HEAD +
                                    textSizeWrap(line, aWrapFont, wrapMax, aWrapString, wrapReplaceFilePath, out _);
                            }
                        }
                        if (!aBlock[i].StartsWith("<"))
                        {
                            newBlockContent[i] = textSizeWrap(aBlock[i], aWrapFont, wrapMax, aWrapString, wrapReplaceFilePath, out _);
                        }
                    }
                    newFileContent += TransCommon.blockToString(newBlockContent);
                }

                File.WriteAllText(toFilePath, newFileContent, aMediateEncoding);
            }

        }

        public void import(string inputDir, string orgDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string orgFilePath= orgDir;
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
                            orgLine = Int32.Parse(info[INFO_LINE_HEAD])- 1;
                            continue;
                        }
                        if (aBlock[i].StartsWith(TransCommon.ORIGINAL_LINE_HEAD)) { continue; }
                        string sentence = "";
                        if (aBlock[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                        {
                            toFileLines[orgLine] = "";
                            sentence = aBlock[i].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                        }
                        else
                        {
                            toFileLines[orgLine] += Environment.NewLine+ Environment.NewLine;
                            sentence = aBlock[i];
                        }
                        sentence = base.convertCharacter(sentence, listCharVi, listCharJp);
                        if (!sentence.ToLower().Contains("setbacklog(")
                            && !sentence.ToLower().Contains("setfont("))
                        {
                            sentence = sentence.Replace(".", "&.").Replace(",", "&,");
                        }
                        toFileLines[orgLine] += sentence;
                    }
                }

                File.WriteAllLines(toFilePath, toFileLines, aOutputEncoding);
            }
        }


    }
}
