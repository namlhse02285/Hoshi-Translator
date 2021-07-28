using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hoshi_Translator
{
    class Property
    {
        public class Common
        {
            public static readonly string INPUT_ENCODING = "INPUT_ENCODING";
            public static readonly string MEDIATE_ENCODING = "MEDIATE_ENCODING";
            public static readonly string OUTPUT_ENCODING = "OUTPUT_ENCODING";
            public static readonly string WRAP_FONT_NAME = "WRAP_FONT_NAME";
            public static readonly string WRAP_FONT_SIZE = "WRAP_FONT_SIZE";
            public static readonly string WRAP_MAX = "WRAP_MAX";
            public static readonly string WRAP_STRING = "WRAP_STRING";
        }
        public class RPGProp
        {
            public static readonly string RPG_FACE_WRAP_MAX = "RPG_FACE_WRAP_MAX";
            public static readonly string RPG_NONE_CODE_WRAP_MAX = "RPG_NONE_CODE_WRAP_MAX";
        }

        private Dictionary<String, String> list;
        private String filename;

        public Property(String _fileName)
        {
            filename = _fileName;
        }

        public String get(String field, String defValue)
        {
            return (get(field) == null) ? (defValue) : (get(field));
        }
        public String get(String field)
        {
            return (list.ContainsKey(field)) ? (list[field]) : (null);
        }
        public bool getBool(String field)
        {
            if (list.ContainsKey(field))
            {
                return (list[field]).ToLower().Equals("true");
            }
            return false;
        }

        public void set(String field, Object value)
        {
            if (!list.ContainsKey(field))
                list.Add(field, value.ToString());
            else
                list[field] = value.ToString();
        }

        public void Save()
        {
            using (StreamWriter fileWriter = File.CreateText(filename))
            {
                foreach (String prop in list.Keys.ToArray())
                    //if (!String.IsNullOrWhiteSpace(list[prop]))
                    fileWriter.WriteLine(prop + "=" + list[prop]);

                fileWriter.Close();
            }
        }

        public bool reload()
        {
            list = new Dictionary<String, String>();

            if (System.IO.File.Exists(filename))
            {
                loadFromFile();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void loadFromFile()
        {
            foreach (String line in System.IO.File.ReadAllLines(filename))
            {
                if ((!String.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (!line.StartsWith("//")) &&
                    (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    String key = line.Substring(0, index).Trim();
                    String value = line.Substring(index + 1);

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    try
                    {
                        //ignore dublicates
                        list.Add(key, value);
                    }
                    catch { }
                }
            }
        }
    }
}
