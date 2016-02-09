using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    struct ColorInfo : IColorInfo
    {
		public ColorInfo(int red, int green, int blue, string hex)
            : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
			Hex = hex;
        }

        public int Red { get; private set; }
        public int Green { get; private set; }
        public int Blue { get; private set; }
		public string Hex { get; private set; }

		public override string ToString ()
		{
			return Hex;
		}
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ColorInfo && Equals((ColorInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Red;
                hashCode = (hashCode*397) ^ Green;
                hashCode = (hashCode*397) ^ Blue;
                return hashCode;
            }
        }

        private bool Equals(ColorInfo other)
        {
            return Red == other.Red && Green == other.Green && Blue == other.Blue;
        }
    }
}
