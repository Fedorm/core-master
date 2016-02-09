using System;
using System.Text.RegularExpressions;

namespace Telegram.Translation
{
    internal class Lexer
    {
        private readonly string _input;
        private int _offset;
        private int _start;

        public Lexer(string input)
        {
            _input = input;
        }

        public string Next(string regexp)
        {
            string result = NextTerminal(regexp, ref _offset);
            if (result == null)
                throw new Exception("Cannot find " + regexp + " in " + _input.Substring(_start));

            _start = _offset;
            return result;
        }

        public bool Look(string regexp)
        {
            int offset = _offset;
            return NextTerminal(regexp, ref offset) != null;
        }

        private string NextTerminal(string regexp, ref int offset)
        {
            int current = offset;
            var r = new Regex(string.Format("^{0}$", regexp));

            string last = null;
            while (true)
            {
                if (++current > _input.Length)
                    return last;

                string substring = _input.Substring(_start, current - _start);
                string trim = substring.TrimStart();

                if (r.IsMatch(substring))
                    last = substring;
                else if (r.IsMatch(trim))
                    last = trim;
                else if (last != null)
                    return last;

                offset = current;
            }
        }
    }
}