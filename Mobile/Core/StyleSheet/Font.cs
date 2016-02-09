using System;
using System.Globalization;

namespace BitMobile.Controls.StyleSheet
{
    public class Font : Style
    {
        private float size;

        public float Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }

        private Measure measure;

        public Measure Measure
        {
            get
            {
                return measure;
            }
            set
            {
                measure = value;
            }
        }

        private String name;

        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public override Style FromString(string stringValue)
        {
            stringValue = stringValue.Trim();

            String[] arr = stringValue.Split(' ');
            if (arr.Length != 2)
                throw new Exception("Invalid style value: " + stringValue);

            name = arr[1];
            String s = arr[0];

            try
            {
                if (s.Contains("px"))
                {
                    String v = s.Replace("px", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Pixels;
                }
                else if (s.Contains("%"))
                {
                    String v = s.Replace("%", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Percent;
                }
                else if (s.Contains("sp"))
                {
                    String v = s.Replace("sp", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.ScreenPercent;
                }
                else if (s.Contains("mm"))
                {
                    String v = s.Replace("mm", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Millimetre;
                }
                else
                    throw new Exception("Unknown measure");

                return this;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid style value: " + stringValue, e);
            }
        }

        public override int GetHashCode()
        {
            return size.GetHashCode() ^ measure.GetHashCode() ^ name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            if (!(obj is Font))
                return false;

            Font f = (Font)obj;

            bool result = size.Equals(f.size);
            result &= measure.Equals(f.measure);
            result &= name.Equals(f.name);

            return result;
        }
    }

	[Synonym("text-format")]
	public class TextFormat: Style 
	{
		public Format Value { get; private set; }

		public override Style FromString(string s)
		{
			s = s.Trim();

			Format result;
			if (!Enum.TryParse<Format>(s, true, out result))
				throw new Exception("Invalid text-format value: " + s);

			Value = result;
			return this;
		}

		public enum Format
		{
			Text,
			Html
		}
	}

	// TODO: add handler to IOS and Android
//    [Synonym("font-weight")]
//    public class FontWeight : Style
//    {
//        private String value;
//
//        public String Value
//        {
//            get
//            {
//                return this.value;
//            }
//            set
//            {
//                this.value = value;
//            }
//        }
//
//        public override Style FromString(string s)
//        {
//            s = s.Trim().ToLower();
//            if (!(s.Equals("normal") || s.Equals("bold")))
//                throw new Exception("Invalid font-weight value");
//            this.value = s;
//            return this;
//        }
//    }
}

