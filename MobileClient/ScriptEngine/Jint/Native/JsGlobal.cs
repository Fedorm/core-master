using BitMobile.Application.Exceptions;
using BitMobile.Application.Extensions;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;
using Jint.Expressions;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.Native
{
    [Serializable]
    public class JsGlobal : JsObject, IGlobal
    {
        /// <summary>
        /// Useful for eval()
        /// </summary>
        public IJintVisitor Visitor { get; set; }

        public Options Options { get; set; }

        public JsGlobal(IJintVisitor visitor, Options options)
        {
            this.Options = options;
            this.Visitor = visitor;

            this["null"] = JsNull.Instance;

            #region Global Classes
            this["Object"] = ObjectClass = new JsObjectConstructor(this);
            this["Function"] = FunctionClass = new JsFunctionConstructor(this);
            this["Array"] = ArrayClass = new JsArrayConstructor(this);
            this["Boolean"] = BooleanClass = new JsBooleanConstructor(this);
            this["Date"] = DateClass = new JsDateConstructor(this); // overriten by 1C function

            this["Error"] = ErrorClass = new JsErrorConstructor(this, "Error");
            this["EvalError"] = EvalErrorClass = new JsErrorConstructor(this, "EvalError");
            this["RangeError"] = RangeErrorClass = new JsErrorConstructor(this, "RangeError");
            this["ReferenceError"] = ReferenceErrorClass = new JsErrorConstructor(this, "ReferenceError");
            this["SyntaxError"] = SyntaxErrorClass = new JsErrorConstructor(this, "SyntaxError");
            this["TypeError"] = TypeErrorClass = new JsErrorConstructor(this, "TypeError");
            this["URIError"] = URIErrorClass = new JsErrorConstructor(this, "URIError");

            this["Number"] = NumberClass = new JsNumberConstructor(this);
            this["RegExp"] = RegExpClass = new JsRegExpConstructor(this);
            this["String"] = StringClass = new JsStringConstructor(this);
            this["Math"] = MathClass = new JsMathConstructor(this);
            this.Prototype = ObjectClass.Prototype;
            #endregion


            MathClass.Prototype = ObjectClass.Prototype;

            foreach (JsInstance c in this.GetValues())
            {
                if (c is JsConstructor)
                {
                    ((JsConstructor)c).InitPrototype(this);
                }
            }

            #region Global Properties
            this["NaN"] = NumberClass["NaN"];  // 15.1.1.1
            this["Infinity"] = NumberClass["POSITIVE_INFINITY"]; // // 15.1.1.2
            this["undefined"] = JsUndefined.Instance; // 15.1.1.3
            this[JsInstance.THIS] = this;
            #endregion

            #region Global Functions
            this["eval"] = new JsFunctionWrapper(Eval); // 15.1.2.1
            this["parseInt"] = new JsFunctionWrapper(ParseInt); // 15.1.2.2
            this["parseFloat"] = new JsFunctionWrapper(ParseFloat); // 15.1.2.3
            this["getType"] = new JsFunctionWrapper(GetType); // AVKugushev
            this["isDefault"] = new JsFunctionWrapper(IsDefault); // AVKugushev
            this["enumToString"] = new JsFunctionWrapper(EnumToString); // AVKugushev
            this["enumToInt"] = new JsFunctionWrapper(EnumToInt); // AVKugushev
            this["validate"] = new JsFunctionWrapper(Validate); // AVKugushev
            this["isNaN"] = new JsFunctionWrapper(IsNaN);
            this["isFinite"] = new JsFunctionWrapper(isFinite);
            this["decodeURI"] = new JsFunctionWrapper(DecodeURI);
            this["encodeURI"] = new JsFunctionWrapper(EncodeURI);
            this["decodeURIComponent"] = new JsFunctionWrapper(DecodeURIComponent);
            this["encodeURIComponent"] = new JsFunctionWrapper(EncodeURIComponent);

            // 1C functions
            // Functions for working with String type values
            this["StrLen"] = new JsFunctionWrapper(StrLen);
            this["TrimL"] = new JsFunctionWrapper(TrimL);
            this["TrimR"] = new JsFunctionWrapper(TrimR);
            this["TrimAll"] = new JsFunctionWrapper(TrimAll);
            this["Left"] = new JsFunctionWrapper(Left);
            this["Right"] = new JsFunctionWrapper(Right);
            this["Mid"] = new JsFunctionWrapper(Mid);
            this["Find"] = new JsFunctionWrapper(Find);
            this["Upper"] = new JsFunctionWrapper(Upper);
            this["Lower"] = new JsFunctionWrapper(Lower);
            this["Char"] = new JsFunctionWrapper(Char);
            this["CharCode"] = new JsFunctionWrapper(CharCode);
            this["IsBlankString"] = new JsFunctionWrapper(IsBlankString);
            this["StrReplace"] = new JsFunctionWrapper(StrReplace);
            this["StrLineCount"] = new JsFunctionWrapper(StrLineCount);
            this["StrGetLine"] = new JsFunctionWrapper(StrGetLine);
            this["StrOccurrenceCount"] = new JsFunctionWrapper(StrOccurrenceCount);
            this["Title"] = new JsFunctionWrapper(Title);
            // Functions for working with Number type values
            this["Int"] = new JsFunctionWrapper(Int);
            this["Round"] = new JsFunctionWrapper(Round);
            this["Log"] = new JsFunctionWrapper(Log);
            this["Log10"] = new JsFunctionWrapper(Log10);
            this["Sin"] = new JsFunctionWrapper(Sin);
            this["Cos"] = new JsFunctionWrapper(Cos);
            this["Tan"] = new JsFunctionWrapper(Tan);
            this["ASin"] = new JsFunctionWrapper(ASin);
            this["ACos"] = new JsFunctionWrapper(ACos);
            this["ATan"] = new JsFunctionWrapper(ATan);
            this["Exp"] = new JsFunctionWrapper(Exp);
            this["Pow"] = new JsFunctionWrapper(Pow);
            this["Sqrt"] = new JsFunctionWrapper(Sqrt);
            // Functions for working with Date type values
            this["Year"] = new JsFunctionWrapper(Year);
            this["Month"] = new JsFunctionWrapper(Month);
            this["Day"] = new JsFunctionWrapper(Day);
            this["Hour"] = new JsFunctionWrapper(Hour);
            this["Minute"] = new JsFunctionWrapper(Minute);
            this["Second"] = new JsFunctionWrapper(Second);
            this["BegOfYear"] = new JsFunctionWrapper(BegOfYear);
            this["BegOfQuarter"] = new JsFunctionWrapper(BegOfQuarter);
            this["BegOfMonth"] = new JsFunctionWrapper(BegOfMonth);
            this["BegOfWeek"] = new JsFunctionWrapper(BegOfWeek);
            this["BegOfDay"] = new JsFunctionWrapper(BegOfDay);
            this["BegOfHour"] = new JsFunctionWrapper(BegOfHour);
            this["BegOfMinute"] = new JsFunctionWrapper(BegOfMinute);
            this["EndOfYear"] = new JsFunctionWrapper(EndOfYear);
            this["EndOfQuarter"] = new JsFunctionWrapper(EndOfQuarter);
            this["EndOfMonth"] = new JsFunctionWrapper(EndOfMonth);
            this["EndOfWeek"] = new JsFunctionWrapper(EndOfWeek);
            this["EndOfDay"] = new JsFunctionWrapper(EndOfDay);
            this["EndOfHour"] = new JsFunctionWrapper(EndOfHour);
            this["EndOfMinute"] = new JsFunctionWrapper(EndOfMinute);
            this["WeekOfYear"] = new JsFunctionWrapper(WeekOfYear);
            this["DayOfYear"] = new JsFunctionWrapper(DayOfYear);
            this["WeekDay"] = new JsFunctionWrapper(WeekDay);
            this["AddMonth"] = new JsFunctionWrapper(AddMonth);
            this["CurrentDate"] = new JsFunctionWrapper(CurrentDate);
            // Value conversion functions
            this["Boolean"] = new JsFunctionWrapper(Boolean);
            this["Number"] = new JsFunctionWrapper(Number);
            this["String"] = new JsFunctionWrapper(String);
            this["Date"] = new JsFunctionWrapper(Date);
            // Formatting functions
            this["Format"] = new JsFunctionWrapper(Format);
            // Others
            this["Type"] = new JsFunctionWrapper(Type);
            this["TypeOf"] = new JsFunctionWrapper(TypeOf);
            this["Min"] = new JsFunctionWrapper(Min);
            this["Max"] = new JsFunctionWrapper(Max);
            this["ErrorDescription"] = new JsFunctionWrapper(ErrorDescription);
            this["Eval"] = new JsFunctionWrapper(Eval1C);
            this["ErrorInfo"] = new JsFunctionWrapper(ErrorInfo);
            this["ToString"] = new JsFunctionWrapper(String);
            #endregion
        }


        #region Global Functions

        public JsObjectConstructor ObjectClass { get; private set; }
        public JsFunctionConstructor FunctionClass { get; private set; }
        public JsArrayConstructor ArrayClass { get; private set; }
        public JsBooleanConstructor BooleanClass { get; private set; }
        public JsDateConstructor DateClass { get; private set; }
        public JsErrorConstructor ErrorClass { get; private set; }
        public JsErrorConstructor EvalErrorClass { get; private set; }
        public JsErrorConstructor RangeErrorClass { get; private set; }
        public JsErrorConstructor ReferenceErrorClass { get; private set; }
        public JsErrorConstructor SyntaxErrorClass { get; private set; }
        public JsErrorConstructor TypeErrorClass { get; private set; }
        public JsErrorConstructor URIErrorClass { get; private set; }

        public JsMathConstructor MathClass { get; private set; }
        public JsNumberConstructor NumberClass { get; private set; }
        public JsRegExpConstructor RegExpClass { get; private set; }
        public JsStringConstructor StringClass { get; private set; }

        /// <summary>
        /// 15.1.2.1
        /// </summary>
        public JsInstance Eval(JsInstance[] arguments)
        {
            if (JsString.TYPEOF != arguments[0].Class)
            {
                return arguments[0];
            }

            Program p;

            try
            {
                p = JintEngine.Compile(arguments[0].ToString(), Visitor.DebugMode);
            }
            catch (Exception e)
            {
                throw new JsException(this.SyntaxErrorClass.New(e.Message));
            }

            try
            {
                p.Accept((IStatementVisitor)Visitor);
            }
            catch (Exception e)
            {
                throw new JsException(this.EvalErrorClass.New(e.Message));
            }

            return Visitor.Result;
        }

        /// <summary>
        /// 15.1.2.2
        /// </summary>
        public JsInstance ParseInt(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsUndefined.Instance;
            }

            //in case of an enum, just cast it to an integer
            if (arguments[0].IsClr && arguments[0].Value.GetType().IsEnum)
                return NumberClass.New((int)arguments[0].Value);

            string number = arguments[0].ToString().Trim();
            int sign = 1;
            int radix = 10;

            if (number == string.Empty)
            {
                return this["NaN"];
            }

            if (number.StartsWith("-"))
            {
                number = number.Substring(1);
                sign = -1;
            }
            else if (number.StartsWith("+"))
            {
                number = number.Substring(1);
            }

            if (arguments.Length >= 2)
            {
                if (arguments[1] != JsUndefined.Instance && !0.Equals(arguments[1]))
                {
                    radix = Convert.ToInt32(arguments[1].Value);
                }
            }

            if (radix == 0)
            {
                radix = 10;
            }
            else if (radix < 2 || radix > 36)
            {
                return this["NaN"];
            }

            if (number.ToLower().StartsWith("0x"))
            {
                radix = 16;
            }

            try
            {
                return NumberClass.New(sign * Convert.ToInt32(number, radix));
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(number))
                    return NumberClass.New(0);

                return this["NaN"];
            }
        }

        /// <summary>
        /// 15.1.2.3
        /// </summary>
        public JsInstance ParseFloat(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsUndefined.Instance;
            }

            string number = arguments[0].ToString().Trim().Replace(" ", "").Replace(Environment.NewLine, "");

            double result;
            if (double.TryParse(number, NumberStyles.Float, new CultureInfo("en-US"), out result))
            {
                return new JsNumber(result);
            }
            if (double.TryParse(number, NumberStyles.Float, new CultureInfo("ru-RU"), out result))
            {
                return new JsNumber(result);
            }
            if (number.Trim() == "-" || number.Trim() == ".")
            {
                return new JsNumber(0);
            }
            return this["NaN"];
        }

        public JsInstance GetType(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsUndefined.Instance;
            }

            object val = arguments[0].Value;
            string result = val != null ? val.GetType().ToString() : "object";

            var entity = val as IEntity;
            if (entity != null)
                result = entity.EntityType.TypeName;

            return new JsString(result);
        }

        public JsInstance IsDefault(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsUndefined.Instance;
            }

            Type type = arguments[0].Value.GetType();

            object defaultValue;

            if (type.IsValueType)
                defaultValue = Activator.CreateInstance(type);
            else
                defaultValue = null;

            bool result = arguments[0].Value.Equals(defaultValue);

            return new JsBoolean(result);
        }

        public JsInstance EnumToString(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
                return JsUndefined.Instance;

            if (!(arguments[0].Value is Enum))
                return JsUndefined.Instance;

            return new JsString(arguments[0].Value.ToString());
        }

        public JsInstance EnumToInt(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
                return JsUndefined.Instance;

            if (!(arguments[0].Value is Enum))
                return JsUndefined.Instance;

            return new JsNumber((int)arguments[0].Value);
        }

        /// <summary>
        /// 15.1.2.4
        /// </summary>
        public JsInstance IsNaN(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsBoolean.False;
            }

            return new JsBoolean(double.NaN.Equals(arguments[0].ToNumber()));
        }

        /// <summary>
        /// 15.1.2.5
        /// </summary>
        protected JsInstance isFinite(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return JsBoolean.False;
            }

            var value = arguments[0];
            return new JsBoolean(value != NumberClass["NaN"]
                && value != NumberClass["POSITIVE_INFINITY"]
                && value != NumberClass["NEGATIVE_INFINITY"]);
        }

        protected JsInstance DecodeURI(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return StringClass.New();
            }

            return this.StringClass.New(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
        }

        private static char[] reservedEncoded = new char[] { ';', ',', '/', '?', ':', '@', '&', '=', '+', '$', '#' };
        private static char[] reservedEncodedComponent = new char[] { '-', '_', '.', '!', '~', '*', '\'', '(', ')', '[', ']' };

        protected JsInstance EncodeURI(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return this.StringClass.New();
            }

            string encoded = Uri.EscapeDataString(arguments[0].ToString());

            foreach (char c in reservedEncoded)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
            }

            foreach (char c in reservedEncodedComponent)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
            }

            return this.StringClass.New(encoded.ToUpper());
        }

        protected JsInstance DecodeURIComponent(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return this.StringClass.New();
            }

            return this.StringClass.New(Uri.UnescapeDataString(arguments[0].ToString().Replace("+", " ")));
        }

        protected JsInstance EncodeURIComponent(JsInstance[] arguments)
        {
            if (arguments.Length < 1 || arguments[0] == JsUndefined.Instance)
            {
                return this.StringClass.New();
            }

            string encoded = Uri.EscapeDataString(arguments[0].ToString());

            foreach (char c in reservedEncodedComponent)
            {
                encoded = encoded.Replace(Uri.EscapeDataString(c.ToString()), c.ToString());
            }

            return this.StringClass.New(encoded.ToUpper());
        }

        public JsInstance Validate(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string input = ParseStr(arg[0]);
            string pattern = string.Format("^{0}$", ParseStr(arg[1]));

            Regex regex = new Regex(pattern);
            bool result = regex.IsMatch(input);
            return new JsBoolean(result);
        }

        #endregion

        #region 1C functions

        #region Functions for working with String type values

        public JsInstance Title(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string str = ParseStr(arg[0]);

            string[] split = str.Split(' ');

            StringBuilder builder = new StringBuilder(split.Length);
            for (int i = 0; i < split.Length; i++)
            {
                char[] word = split[i].ToLower().Trim().ToCharArray();
                if (word.Length > 0)
                {
                    word[0] = char.ToUpper(word[0]);
                    builder.Append(word);
                    if (i != split.Length - 1)
                        builder.Append(' ');
                }
            }

            return new JsString(builder.ToString());
        }

        public JsInstance StrOccurrenceCount(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);
            string search = ParseStr(arg[1]);

            int result = Regex.Matches(line, search).Count;
            return new JsNumber(result);
        }

        public JsInstance StrGetLine(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);
            int lineNumber = ParseInt(arg[1]);
            lineNumber--; // because line number starts from 1

            string[] split = line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            string result = "";

            if (lineNumber < split.Length && lineNumber > 0)
                result = split[lineNumber];

            return new JsString(result);
        }

        public JsInstance StrLineCount(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            string[] split = line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            return new JsNumber(split.Length);
        }

        public JsInstance StrReplace(JsInstance[] arg)
        {
            CheckArg(arg, 3);
            string line = ParseStr(arg[0]);
            string search = ParseStr(arg[1]);
            string replace = ParseStr(arg[2]);

            string result = line;
            if (search.Length > 0)
                result = line.Replace(search, replace);

            return new JsString(result);
        }

        public JsInstance IsBlankString(JsInstance[] arg)
        {
            CheckArg(arg, 1, false);
            string line = ParseStr(arg[0]);

            bool result = string.IsNullOrWhiteSpace(line);
            return new JsBoolean(result);
        }

        public JsInstance CharCode(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);
            int index = 1;
            if (arg.Length > 1 && arg[1] != null)
                index = ParseInt(arg[1]);

            index--; // because index starts from 1

            int result = 0;
            if (index < line.Length && index >= 0)
                result = (int)line[index];
            return new JsNumber(result);
        }

        public JsInstance Char(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            int index = ParseInt(arg[0]);

            string result = string.Empty;

            if (index >= char.MinValue && index <= char.MaxValue)
                result = Convert.ToChar(index).ToString();
            return new JsString(result);
        }

        public JsInstance Lower(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            return new JsString(line.ToLower());
        }

        public JsInstance Upper(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            return new JsString(line.ToUpper());
        }

        public JsInstance Find(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);
            string search = ParseStr(arg[1]);

            int result = line.IndexOf(search);
            result++; // because search starts from 1
            return new JsNumber(result);
        }

        public JsInstance Mid(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);

            int initial = ParseInt(arg[1]);
            if (initial <= 0)
                initial = 1; // because if initial is less or equal to zero, then it is set to 1.
            initial--; // because initial starts from 1

            int count = line.Length - initial;
            if (arg.Length > 2 && arg[2] != null)
                count = ParseInt(arg[2]);

            string result = string.Empty;
            if (initial < line.Length)
            {
                if ((long)initial + (long)count > (long)line.Length)
                    count = line.Length - initial;

                result = line.Substring(initial, count);
            }

            return new JsString(result);
        }

        public JsInstance Right(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);
            int count = ParseInt(arg[1]);

            if (count > line.Length)
                count = line.Length;

            string result = line.Substring(line.Length - count);
            return new JsString(result);
        }

        public JsInstance Left(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            string line = ParseStr(arg[0]);
            int count = ParseInt(arg[1]);

            if (count > line.Length)
                count = line.Length;

            string result = line.Substring(0, count);
            return new JsString(result);
        }

        public JsInstance TrimAll(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            string result = line.Trim();
            return new JsString(result);
        }

        public JsInstance TrimR(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            string result = line.TrimEnd();
            return new JsString(result);
        }

        public JsInstance TrimL(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            string result = line.TrimStart();
            return new JsString(result);
        }

        public JsInstance StrLen(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            string line = ParseStr(arg[0]);

            int result = line.Length;
            return new JsNumber(result);
        }

        #endregion

        #region Functions for working with Number type values

        public JsInstance Sqrt(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            if (num < 0)
                return JsUndefined.Instance;

            double result = Math.Sqrt(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Pow(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            double b = ParseDouble(arg[0]);
            double f = ParseDouble(arg[1]);

            double result = Math.Pow(b, f);

            return NumberOrUndefined(result);
        }

        public JsInstance Exp(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Exp(num);

            return NumberOrUndefined(result);
        }

        public JsInstance ATan(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Atan(num);

            return NumberOrUndefined(result);
        }

        public JsInstance ACos(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Acos(num);

            return NumberOrUndefined(result);
        }

        public JsInstance ASin(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Asin(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Tan(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Tan(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Cos(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Cos(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Sin(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Sin(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Log10(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Log10(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Log(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = Math.Log(num);

            return NumberOrUndefined(result);
        }

        public JsInstance Round(JsInstance[] arg)
        {
            // TODO: Реализовать параметр RoutingMode
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            int capacity = 0;
            if (arg.Length > 1 && arg[1] != null)
                capacity = ParseInt(arg[1]);

            double scale = Math.Pow(10, -1 * capacity);
            double result = Math.Round(num / scale, 0, MidpointRounding.AwayFromZero);
            result *= scale;

            return NumberOrUndefined(result);
        }

        public JsInstance Int(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            double num = ParseDouble(arg[0]);

            double result = num > 0 ? Math.Floor(num) : Math.Ceiling(num);

            return NumberOrUndefined(result);
        }

        #endregion

        #region Functions for working with Date type values

        public JsInstance CurrentDate(JsInstance[] arg)
        {
            CheckArg(arg, 0);

            return new JsClr(Visitor, DateTime.Now);
        }

        public JsInstance AddMonth(JsInstance[] arg)
        {
            CheckArg(arg, 2);
            DateTime date = ParseDate(arg[0]);
            int count = ParseInt(arg[1]);

            DateTime result = date.AddMonths(count);
            return new JsClr(Visitor, result);
        }

        public JsInstance WeekDay(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = (int)date.DayOfWeek;
            if (result == 0)
                result = 1;// because week in the west starts from sunday
            return new JsNumber(result);
        }

        public JsInstance DayOfYear(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = (int)date.DayOfYear;
            return new JsNumber(result);
        }

        public JsInstance WeekOfYear(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            int result = cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            return new JsNumber(result);
        }

        public JsInstance EndOfMinute(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfHour(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfDay(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfWeek(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int lastDayNum = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - 1;
            if (lastDayNum < 0)
                lastDayNum = 6;
            DayOfWeek lastDay = (DayOfWeek)lastDayNum;

            DateTime day = date.Date;
            while (day.DayOfWeek != lastDay)
                day = day.AddDays(1);

            DateTime result = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfMonth(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int day = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime result = new DateTime(date.Year, date.Month, day, 23, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfQuarter(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int quarterNum = (date.Month - 1) / 3 + 1;
            int month = quarterNum * 3;
            int day = DateTime.DaysInMonth(date.Year, month);

            DateTime result = new DateTime(date.Year, month, day, 23, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance EndOfYear(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, 12, 31, 23, 59, 59);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfMinute(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfHour(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfDay(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfWeek(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            DateTime day = date.Date;
            while (day.DayOfWeek != firstDay)
                day = day.AddDays(-1);

            DateTime result = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfMonth(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfQuarter(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int quarterNum = (date.Month - 1) / 3 + 1;
            int month = quarterNum * 3 - 2;

            DateTime result = new DateTime(date.Year, month, 1, 0, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance BegOfYear(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            DateTime result = new DateTime(date.Year, 1, 1, 0, 0, 0);
            return new JsClr(Visitor, result);
        }

        public JsInstance Second(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Second;
            return new JsNumber(result);
        }

        public JsInstance Minute(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Minute;
            return new JsNumber(result);
        }

        public JsInstance Hour(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Hour;
            return new JsNumber(result);
        }

        public JsInstance Day(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Day;
            return new JsNumber(result);
        }

        public JsInstance Month(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Month;
            return new JsNumber(result);
        }

        public JsInstance Year(JsInstance[] arg)
        {
            CheckArg(arg, 1);
            DateTime date = ParseDate(arg[0]);

            int result = date.Year;
            return new JsNumber(result);
        }

        #endregion

        #region Value conversion functions

        public JsInstance Date(JsInstance[] arg)
        {
            CheckArg(arg, 1);

            object a = arg[0].ToObject();
            DateTime result;
            if (a is string) // dafault input
            {
                if (!TryParseDate(arg[0], out result))
                    return JsUndefined.Instance;
            }
            else if (a is DateTime)
            {
                result = (DateTime)a;
            }
            else if (a is JsDate)
            {
                result = (DateTime)((JsDate)a).ToObject();
            }
            else
            {
                CheckArg(arg, 3); // by component input
                int year = ParseInt(arg[0]);
                int month = ParseInt(arg[1]);
                int day = ParseInt(arg[2]);

                int hour, min, sec;
                hour = min = sec = 0;
                if (arg.Length >= 6 && arg[3] != null && arg[4] != null && arg[5] != null)
                {
                    hour = ParseInt(arg[3]);
                    min = ParseInt(arg[4]);
                    sec = ParseInt(arg[5]);
                }

                try
                {
                    result = new DateTime(year, month, day, hour, min, sec);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return JsUndefined.Instance;
                }
            }
            return new JsClr(Visitor, result);
        }

        public JsInstance String(JsInstance[] arg)
        {
            CheckArg(arg, 1, false);
            object obj = arg[0].ToObject();

            string result = obj != null ? obj.ToString() : JsNull.Instance.ToString();

            return new JsString(result);
        }

        public JsInstance Number(JsInstance[] arg)
        {
            CheckArg(arg, 1);

            double result;
            if (!TryParseNum(arg[0], out result))
                return JsUndefined.Instance;
            return new JsNumber(result);
        }

        public JsInstance Boolean(JsInstance[] arg)
        {
            CheckArg(arg, 1);

            bool result;
            if (!TryParseBool(arg[0], out result))
                return JsUndefined.Instance;
            return new JsBoolean(result);
        }

        #endregion

        #region Formatting functions

        public JsInstance Format(JsInstance[] arg)
        {
            CheckArg(arg, 1, false);

            if (arg[0].Value == null)
                return null;

            var args = new object[arg.Length - 1];
            for (int i = 1; i < arg.Length; i++)
                args[i - 1] = arg[i].Value;

            string result = string.Format(ParseStr(arg[0]), args);
            return new JsString(result);
        }
        #endregion

        #region Others

        public JsInstance ErrorInfo(JsInstance[] arg)
        {
            throw new JsGlobalException("Function ErrorInfo not implemented");
        }

        public JsInstance Eval1C(JsInstance[] arg)
        {
            return Eval(arg);
        }

        public JsInstance ErrorDescription(JsInstance[] arg)
        {
            throw new JsGlobalException("Function ErrorDescription not implemented");
        }

        public JsInstance Max(JsInstance[] arg)
        {
            if (arg.Length == 0)
                return JsUndefined.Instance;

            bool hasValue;

            bool[] bools;
            DateTime[] dates;
            double[] nums;
            string[] strings;
            PrepareArrays(arg, out hasValue, out bools, out dates, out nums, out strings);

            if (!hasValue)
                return JsUndefined.Instance;

            if (bools != null)
                return new JsBoolean(bools.Max());

            if (dates != null)
                return new JsClr(Visitor, dates.Max());

            if (nums != null)
                return new JsNumber(nums.Max());

            return new JsString(strings.Max());
        }

        public JsInstance Min(JsInstance[] arg)
        {
            if (arg.Length == 0)
                return JsUndefined.Instance;

            bool hasValue;

            bool[] bools;
            DateTime[] dates;
            double[] nums;
            string[] strings;
            PrepareArrays(arg, out hasValue, out bools, out dates, out nums, out strings);

            if (!hasValue)
                return JsUndefined.Instance;

            if (bools != null)
                return new JsBoolean(bools.Min());

            if (dates != null)
                return new JsClr(Visitor, dates.Min());

            if (nums != null)
                return new JsNumber(nums.Min());

            return new JsString(strings.Min());
        }

        static void PrepareArrays(JsInstance[] arg, out bool hasValue, out bool[] bools, out DateTime[] dates, out double[] nums, out string[] strings)
        {
            hasValue = false;

            bools = new bool[arg.Length];
            dates = new DateTime[arg.Length];
            nums = new double[arg.Length];
            strings = new string[arg.Length];


            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == null || arg[i].Value == null)
                    continue;

                if (bools != null)
                {
                    bool r;
                    if (TryParseBool(arg[i], out r))
                        bools[i] = r;
                    else
                        bools = null;
                }

                if (dates != null)
                {
                    DateTime r;
                    if (TryParseDate(arg[i], out r))
                        dates[i] = r;
                    else
                        dates = null;
                }

                if (nums != null)
                {
                    double r;
                    if (TryParseNum(arg[i], out r))
                        nums[i] = r;
                    else
                        nums = null;
                }

                string s;
                if (!TryParseString(arg[i], out s))
                    s = arg[i].ToString();
                strings[i] = s;

                hasValue = true;
            }
        }

        public JsInstance TypeOf(JsInstance[] arg)
        {
            CheckArg(arg, 1, false);

            object obj = arg[0].ToObject();
            if (obj != null)
                return new JsObject(obj.GetType());
            return JsUndefined.Instance;
        }

        public JsInstance Type(JsInstance[] arg)
        {
            CheckArg(arg, 1, false);

            string input = ParseStr(arg[0]);

            Type result;

            result = this.GetType().Assembly.GetType(input, false, true);
            if (result == null)
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    result = a.GetType(input, false, true);
                    if (result != null)
                        break;
                }

            if (result != null)
                return new JsObject(result);
            return JsUndefined.Instance;
        }
        #endregion

        #endregion

        #region Helpers

        static void CheckArg(JsInstance[] arg, int expectedCount, bool throwIfNull = true)
        {
            if (arg.Length < expectedCount)
                throw new JsGlobalException("Incorrect argument count in function");

            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == JsUndefined.Instance)
                    throw new JsGlobalException("Argument at index {0} has undefined type".Formatting(i));

                if (throwIfNull)
                    if (arg[i].Value == null)
                        throw new JsGlobalException("Argument at index {0} is null".Formatting(i));
            }
        }

        static bool TryParseBool(JsInstance arg, out bool result)
        {
            object obj = arg.ToObject();
            if (obj is string)
            {
                string input = (string)obj;

                if (!bool.TryParse(input, out result))
                    return false;
            }
            else if (obj is bool)
            {
                result = (bool)obj;
            }
            else
            {
                double input = ParseDouble(arg);
                if (input < 0)
                {
                    result = false;
                    return false;
                }

                result = input != 0;
            }
            return true;
        }

        static string ParseStr(JsInstance obj)
        {
            string result;

            if (!TryParseString(obj, out result))
                throw new JsGlobalException("Invalid cast '{0}' to 'String'".Formatting(obj));
            return result;
        }

        static bool TryParseString(JsInstance obj, out string result)
        {
            result = obj.ToObject() as string;
            return result != null;
        }

        static int ParseInt(JsInstance obj)
        {
            double result;
            if (!TryParseNum(obj, out result))
                throw new JsGlobalException(
                    "Invalid cast '{0}' to 'Int'. Js object: {1}".Formatting(obj.ToObject(), obj));
            return (int)result;
        }

        static double ParseDouble(JsInstance obj)
        {
            double result;
            if (!TryParseNum(obj, out result))
                throw new JsGlobalException(
                    "Invalid cast '{0}' to 'Int'. Js object: {1}".Formatting(obj.ToObject(), obj));

            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new JsGlobalException("Argument is NaN or Infinity");
            return result;
        }

        static JsInstance NumberOrUndefined(double num)
        {
            if (double.IsNaN(num) || double.IsInfinity(num))
                return JsUndefined.Instance;
            return new JsNumber(num);
        }

        static bool TryParseNum(JsInstance arg, out double result)
        {
            object obj = arg.ToObject();
            if (obj is string)
            {
                var input = (string)obj;

                if (input.Trim() == "-" || input.Trim() == ".")
                {
                    result = 0;
                    return true;
                }

                if (!double.TryParse(input, out result))
                    if (!double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                        return false;
                return true;
            }
            
            if (obj is bool)
            {
                result = (bool)obj ? 1 : 0;
                return true;
            }
            
            try
            {
                result = (double)Convert.ChangeType(obj, typeof(double));
                return true;
            }
            catch (InvalidCastException)
            {
                result = double.NaN;
                return false;
            }
        }

        static DateTime ParseDate(JsInstance obj)
        {
            DateTime result;
            if (!TryParseDate(obj, out result))
                throw new JsGlobalException(
                    "Invalid cast '{0}' to 'DateTime'. Js object: {1}".Formatting(obj.ToObject(), obj));
            return result;
        }

        static bool TryParseDate(JsInstance arg, out DateTime result)
        {
            object obj = arg.ToObject();
            string input = obj as string;
            if (input != null)
            {
                if (!DateTime.TryParse(input, out result))
                    if (!DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        if (!DateTime.TryParseExact(input, "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                            if (!DateTime.TryParseExact(input, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                                return false;
                return true;
            }
            else if (obj is DateTime)
            {
                result = (DateTime)obj;
                return true;
            }

            result = DateTime.MinValue;
            return false;
        }

        #endregion

        public JsObject Wrap(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                    return BooleanClass.New((bool)value);
                case TypeCode.Char:
                case TypeCode.String:
                    return StringClass.New(Convert.ToString(value));
                case TypeCode.DateTime:
                    return DateClass.New((DateTime)value);
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return NumberClass.New(Convert.ToDouble(value));
                case TypeCode.Object:
                    return ObjectClass.New(value);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    throw new ArgumentNullException("value");
            }
        }

        public JsClr WrapClr(object value)
        {
            if (value == null)
            {
                return new JsClr(Visitor, null);
            }

            JsClr clr = new JsClr(Visitor, value);

            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Boolean:
                    clr.Prototype = BooleanClass.Prototype;
                    break;
                case TypeCode.Char:
                case TypeCode.String:
                    clr.Prototype = StringClass.Prototype;
                    break;
                case TypeCode.DateTime:
                    clr.Prototype = DateClass.Prototype;
                    break;
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    clr.Prototype = NumberClass.Prototype;
                    break;
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Empty:
                default:
                    if (value is System.Collections.IEnumerable)
                        clr.Prototype = ArrayClass.Prototype;
                    else
                        clr.Prototype = ObjectClass.Prototype;
                    break;
            }

            return clr;
        }

        public bool HasOption(Options options)
        {
            return (Options & options) == options;
        }

        #region IGlobal Members


        public JsInstance NaN
        {
            get { return this["NaN"]; }
        }

        #endregion
    }
}
