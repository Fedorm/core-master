using System.Linq;

namespace BitMobile.Application.Translator
{
    public class Translator
    {
        const string DefaultLanguage = "en";

        static readonly string[] SupportedLanguages = { "ru", "en" };

        public string CurrentLanguge { get; private set; }

        public Translator(string language)
        {
            CurrentLanguge = CheckLanguage(language);
        }

        public string Choice(string english, string russian)
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
            foreach (var lang in SupportedLanguages)
                if (language.Contains(lang))
                    return lang;
            return DefaultLanguage;
        }
    }
}