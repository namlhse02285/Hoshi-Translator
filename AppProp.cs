using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hoshi_Translator
{
    class AppProp
    {
        public const string APP_LOCATION_X = "app_location_x";
        public const string APP_LOCATION_Y = "app_location_y";
        public const string INIT_SIZE_X = "init_size_x";
        public const string INIT_SIZE_Y = "init_size_y";
        public const string LAST_COMMAND = "last_command";


        public static void saveProp(string key, string value)
        {
            Properties.Settings.Default[key] = value;
            Properties.Settings.Default.Save();
        }

        public static string getProp(string key)
        {
            return Properties.Settings.Default[key].ToString();
        }

        public static string getProp(string key, string defaultValue)
        {
            string ret = Properties.Settings.Default[key].ToString();
            if (ret == null || ret.Length < 1) { return defaultValue; }
            return ret;
        }

        public static int getIntProp(string key)
        {
            int ret = Int32.MinValue;
            try
            {
                ret = Int32.Parse(Properties.Settings.Default[key].ToString());
            }
            catch { }
            return ret;
        }
        public static int getIntProp(string key, int defaultValue)
        {
            int ret = defaultValue;
            try
            {
                ret = Int32.Parse(Properties.Settings.Default[key].ToString());
            }
            catch { }
            return ret;
        }
    }
}
