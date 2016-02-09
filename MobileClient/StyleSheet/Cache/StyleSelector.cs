using System;
using System.Linq;

namespace BitMobile.StyleSheet.Cache
{
    struct StyleSelector
    {
        private readonly int _hash;

        private readonly uint[] _parts;

        public StyleSelector(string selector)
        {
            _hash = selector.GetHashCode();

            string[] split = selector.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var parts = new uint[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                string part = split[i].Trim();
                if (!string.IsNullOrEmpty(part))
                    if (part[0] == '.')
                        part = part.Substring(1);
                    else if (StyleSheetCache.TagNames.ContainsKey(part))
                        part = StyleSheetCache.TagNames[part].Name.ToLower();

                parts[i] = StyleSheetCache.GetStringKey(part);
            }
            _parts = parts;
        }

        public uint[] Parts
        {
            get { return _parts; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleSelector && _parts.SequenceEqual(((StyleSelector)obj)._parts);
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}
