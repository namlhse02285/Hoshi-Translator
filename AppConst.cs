using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hoshi_Translator
{
    class AppConst
    {
        public static readonly string CONFIG_FILE = Environment.CurrentDirectory + "\\config.txt";
        public static readonly string OUTPUT_FILE = Environment.CurrentDirectory + "\\output.txt";
        public static readonly string GUIDE_FILE = Environment.CurrentDirectory + "\\guide.xlsx";
        public static readonly string OUTPUT_DIR = Environment.CurrentDirectory + "\\output";
        public static readonly string OUTPUT_DIR_ = Environment.CurrentDirectory + "\\output\\";
        public static readonly string THIRD_PARTY_LIB_DIR_ = Environment.CurrentDirectory + "\\third_party_library\\";
        public static readonly string RPGM_TRANS_DIR_ = THIRD_PARTY_LIB_DIR_ + "ruby_rpgm_translator\\";
        public static readonly string REPORT_FILE = Environment.CurrentDirectory + "\\report.txt";
        public static readonly string REPLACE_SEPARATOR = "|replace_to|";
        public static readonly string REPLACE_FILE = Environment.CurrentDirectory + "\\replacer.txt";
        public static readonly string REPLACE_WHEN_WRAP_FILE = Environment.CurrentDirectory + "\\replace_when_wrap.txt";
    }
}
