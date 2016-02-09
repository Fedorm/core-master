using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Utilities.Translator
{
    public class Translator
    {
        const string DEFAULT_LANGUAGE = "en";

        static readonly string[] SUPPORTED_LANGUAGES = { "en", "ru" };

        public string CurrentLanguge { get; private set; }

        public Translator(string language)
        {
            CurrentLanguge = CheckLanguage(language);
        }

        public string R(string english, string russian)
        {
            string result;
            switch (CurrentLanguge)
            {
                case "en":
                    result = english;
                    break;
                case "ru":
                    result = russian;
                    break;
                default:
                    result = english;
                    break;
            }

            return result;
        }

        public static string CheckLanguage(string language)
        {
            if (SUPPORTED_LANGUAGES.Contains(language))
                return language;
            else
                return DEFAULT_LANGUAGE;
        }
    }
}