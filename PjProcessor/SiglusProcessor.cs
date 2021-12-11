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
        public static string getConvertedCharName(string japName)
        {
            switch (japName)
            {
                case "羽依里": return "Hairi";
                case "うみ": return "Umi";
                case "天善": return "Tenzen";
                case "蒼": return "Ao";
                case "鴎": return "Kamome";
                case "紬": return "Tsumugi";
                case "美希": return "Miki";
                case "鏡子": return "Kyouko";
                case "しろは": return "Shiroha";
                case "三谷": return "Mitani";
                case "良一": return "Ryouichi";
                case "静久": return "Shizuku";
                case "イナリ": return "Inari";
                case "徳田": return "Tokuda";
                case "駄菓子屋のおばーちゃん": return "Bà lão cửa hàng kẹo";
                case "食堂のおっさん": return "Bác chủ quán ăn";
                case "声": return "Giọng nói";
                case "恵": return "Megumi";
                case "女の子": return "Người con gái";
                case "女の子Ａ": return "Người con gái A";
                case "女の子Ｂ": return "Người con gái B";
                case "おっぱいさん": return "Ngực-san";
                case "しろはのおじーさん": return "Ông của Shiroha";
                case "小鳩": return "Kobato";
                case "堀田": return "Hotta";
                case "水織先輩": return "Mizuori-senpai";
                case "鷺": return "Sagi";
                case "羽依里手紙": return "Bức thư của Hairi";
                case "七海": return "Nanami";
                case "藍": return "Ai";
                case "？？？": return "???";
                case "……": return "......";
                case "「……」": return "「......」";
                case "『……』": return "『......』";
                case "（……）": return "（......）";
                case "………": return ".........";
                case "「………」": return "「.........」";
                case "『………』": return "『.........』";
                case "（………）": return "（.........）";
                case "…………": return "............";
                case "「…………」": return "「............」";
                case "『…………』": return "『............』";
                case "（…………）": return "（............）";
                case "「？」": return "「?」";
                case "『？』": return "『?』";
                case "「！？」": return "「!?」";
                case "『！？』": return "『!?』";
                case "「……あ」": return "「...a.」";
                case "「あ……」": return "「A...」";
                case "「ふーん」": return "「Hmm.」";
                case "「うーん」": return "「Ừmm.」";
                case "「ふむ」": return "「Hm.」";
                case "「ふぅ」": return "「Phùù.」";
                case "「……はぁ」": return "「...hàà.」";
                case "「へぇ」": return "「Hêê.」";
                case "「ん？」": return "「Hm?」";
                case "「？？？」": return "「???」";
                case "「……？」": return "「......?」";
                case "「……ん？」": return "「......hm？」";

                case "島の子供Ａ": return "Trẻ con trên đảo A";
                case "島の子供Ｂ": return "Trẻ con trên đảo B";
                case "島の子供Ｃ": return "Trẻ con trên đảo C";
                case "半裸男": return "Thằng đực cởi trần";
                case "女の子達": return "Các bé gái";
                case "島の少女Ａ": return "Bé gái trên đảo A";
                case "島の少女Ｂ": return "Bé gái trên đảo B";
                case "島の少女Ｃ": return "Bé gái trên đảo C";
                case "小学生女子Ａ": return "Nữ học sinh tiểu học A";
                case "小学生女子Ｂ": return "Nữ học sinh tiểu học B";
                case "小学生女子Ｃ": return "Nữ học sinh tiểu học C";
                case "羽依里（小学生）": return "Hairi(tiểu học)";
                case "少女Ａ": return "Người con gái A";
                case "少女Ｂ": return "Người con gái B";
                case "ハヤト": return "Hayato";
                case "タカ": return "Taka";
                case "カモメ": return "Kamome";
                case "スズメ": return "Suzume";
                case "ツバメ": return "Shibame";
                case "タカマガハラ": return "Taka-mega-hara";
                case "タカシャルハラスメント": return "Taka quấy rối con nhà người ta";
                case "毎度ありー": return "Ngàn lần cảm tạ";
                case "蒼・羽依里": return "Ao - Hairi";
                case "女優": return "Nữ diễn viên";
                case "俳優": return "Nam diễn viên";
                case "玄武": return "Huyền Vũ";
                case "朱雀": return "Chu Tước";
                case "青龍": return "Thanh Long";
                case "白虎": return "Bạch Hổ";
                case "玄武？": return "Huyền Vũ?";
                case "屋台のおっさん": return "Bác chủ xe hàng";
                case "良一妹": return "Em gái Ryouichi";
                case "ギャラリー": return "Quan sát viên";
                case "小学校女教師": return "Nữ giáo viên trường tiểu học";
                case "頑強なおじーちゃん": return "Ông lão rắn rỏi";
                case "役場の職員": return "Nhân viên hành chính";
                case "男": return "Cậu trai";
                case "警官": return "Bảo vệ";
                case "男性": return "Anh trai trẻ";
                case "蒼の父": return "Cha Ao";
                case "蒼の母": return "Mẹ Ao";
                case "おっさん": return "Ông bác";
                case "配達員": return "Nhân viên giao hàng";
                case "眼鏡の少年": return "Cậu bé đeo kính";
                case "栗毛の女の子": return "Cô bé tóc màu hạt dẻ";
                case "背の低い少女": return "Cô bé hơi lùn";
                case "テレビ": return "Tivi";
                case "──": return "──";
                case "島民": return "Người dân";
                case "大男": return "Người đàn ông";
                case "少女": return "Bé gái";
                case "船員": return "Thuyền viên";
                case "医者": return "Bác sĩ";
                case "瞳": return "Hitomi";
                case "しろはの親戚": return "Người nhà Shiroha";
                case "羽未": return "Umi";
                case "堀田父": return "Bố Hotta";
                case "ツムギ": return "TsuMuGi";

                case "ハイドロなんとか": return "Thủy lực gì gì đó";
                case "ハイドロ": return "Cô gái thủy lực";
                case "おばーちゃん": return "Bà lão";
                case "女性": return "Người phụ nữ";
                case "ジャージ男": return "Cậu trai mặc đồ thể thao";
                case "観光客の女性": return "Nữ khách tham quan";
                case "鶏": return "Gà";
                case "謎声": return "Giọng trầm bí ẩn";
                case "野生の猪": return "Lợn rừng hoang";
                case "猪": return "Lợn lòi";
                case "髭のおじーちゃん": return "Ông lão râu rậm";
                case "主婦Ａ": return "Cô nội trợ A";
                case "主婦Ｂ": return "Cô nội trợ B";
                case "主婦Ｃ": return "Cô nội trợ C";
                case "中学時代の友人Ａ": return "Bạn trung học A";
                case "中学時代の友人Ｂ": return "Bạn trung học B";
                case "ハチ駆除業者Ａ": return "Cao thủ diệt ong A";
                case "ハチ駆除業者Ｂ": return "Cao thủ diệt ong B";
                case "良一・天善": return "Ryouichi - Tenzen";
                case "羽依里・うみ": return "Hairi - Umi";
                case "紬・静久": return "Tsumugi - Shizuku";
                case "ちんまい少女": return "Bé con";
                case "アナウンサー": return "Phát thanh viên";
                case "野良猫": return "Mèo hoang";
                case "猫": return "Mèo";
                case "鳥": return "Chim";
                case "船内アナウンス": return "Thông báo trong thuyền";
                case "青年団団長": return "Hội trưởng đoàn thanh niên";
                case "青年団副長": return "Hội phó đoàn thanh niên";
                case "鷹原・パイリ・おっぱい": return "Takahara có hai ti";
                case "子供達": return "Đám trẻ con";
                case "漁師Ａ": return "Ngư dân A";
                case "漁師Ｂ": return "Ngư dân B";
                case "漁師Ｃ": return "Ngư dân C";
                case "女性？": return "Cô gái?";
                case "羽依里・駄菓子屋のおばーちゃん": return "Hairi - Bà lão cửa hàng kẹo";
                case "羽依里・鴎": return "Hairi - Kamome";
                case "やさしげなおじーさん": return "Ông lão có vẻ hiền từ";
                case "老犬": return "Chú chó già";
                case "羽依里・女の子": return "Hairi - Cô gái";
                case "羽依里・蒼": return "Hairi - Ao";
                case "羽依里・良一": return "Hairi - Ryouichi";
                case "良一・羽依里": return "Ryouichi - Hairi";
                case "羽依里・しろは": return "Hairi - Shiroha";
                case "ひとりぼっちの漁師": return "Ngư dân bơ vơ";
                case "美希・良一": return "Miki - Ryouichi";
                case "蒼・美希": return "Ao - Miki";
                case "美希・少女達": return "Miki - Các bé gái";
                case "小鳩・──": return "Kobato - ──";
                case "七海・小鳩": return "Nanami - Kobato";
                case "しろは・七海": return "Shiroha - Nanami";
                case "チームメイト": return "Thành viên trong đội";
                case "ラジオ体操大好きさん": return "Người yêu thể thao radio";
                case "堀田のじーちゃん": return "Ông lão nhà Hotta";
                case "亀": return "Rùa";
                case "一同": return "Mọi người";
                case "手紙": return "Nội dung thư";
                case "ロリコンライダーＡ": return "Lolicon rider A";
                case "ロリコンライダーＢ": return "Lolicon rider B";
                case "ロリコンライダーＣ": return "Lolicon rider C";
                case "カエル": return "Con ếch";
                case "白": return "白";
                case "黒": return "黒";
                case "半裸の男": return "Cậu trai cởi trần";
                case "半裸の男・ジャージ男": return "Hai người con trai";

            }

            return null;
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
                    sentence = TransCommon.quoteSentenceBaseOnOrg(
                        fileContent[i- 1].Substring(lineHead.Length+ 2), sentence);
                    if (!sentence.Contains(aWrapString))
                    {
                        sentence = textSizeWrap(sentence, aWrapFont, aMaxWrap, aWrapString, null, out _);
                    }
                    fileContent[i] = lineHead + sentence;
                }
                string outputFilePath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(filePath));
                File.WriteAllLines(outputFilePath, fileContent, aMediateEncoding);
            }
        }


    }
}
