using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet.Cache
{
    [DebuggerDisplay("{_partsString}")]
    struct StyleKey : IStyleKey
    {
        private readonly IStyleKeyPart[] _parts;
        private readonly int _hash;
#if DEBUG
        private readonly string _partsString;
#endif

        public StyleKey(ILayoutable control)
            : this()
        {
            int hash;
            _parts = GetKey(control, out hash);
            _hash = hash;
#if DEBUG
            _partsString = _parts.Select(val => val.ToString()).Aggregate((seq, next) => seq + "->" + next);
#endif
        }

        public IStyleKeyPart[] Parts
        {
            get { return _parts; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StyleKey && _parts.SequenceEqual(((StyleKey)obj)._parts);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        private static IStyleKeyPart[] GetKey(ILayoutable control, out int hash)
        {
            hash = control.Name.GetHashCode();
            List<IStyleKeyPart> key;
            var parent = control.Parent as ILayoutable;
            if (parent != null)
            {
                int parentHash;
                IStyleKeyPart[] parentParts;

                if (parent.StyleKey != null)
                {
                    IStyleKey parentKey = parent.StyleKey;
                    parentParts = parentKey.Parts;
                    parentHash = parentKey.GetHashCode();
                }
                else
                    parentParts = GetKey(parent, out parentHash);
                key = new List<IStyleKeyPart>(parentParts.Length + 1);
                key.AddRange(parentParts);
                hash ^= parentHash;
            }
            else
                key = new List<IStyleKeyPart>(1);

            string cssClass = control.CssClass;
            if (!string.IsNullOrEmpty(cssClass))
            {
                cssClass = cssClass.ToLower();
                hash ^= cssClass.GetHashCode();
            }

            key.Add(!string.IsNullOrEmpty(cssClass)
                ? new Part(control.Name.ToLower(), cssClass)
                : new Part(control.Name.ToLower()));

            return key.ToArray();
        }

        [DebuggerDisplay("{_type} {_cssClass}")]
        struct Part : IStyleKeyPart
        {
#if DEBUG
            private readonly string _type;
            private readonly string _cssClass;
#endif
            public Part(string type, string cssClass)
                : this()
            {
#if DEBUG
                _type = type;
                _cssClass = cssClass;
#endif
                Type = StyleSheetCache.GetStringKey(type);
                CssClass = StyleSheetCache.GetStringKey(cssClass);
            }

            public Part(string type)
                : this()
            {
                Type = StyleSheetCache.GetStringKey(type);
                CssClass = 0;
            }

            public uint Type { get; private set; }

            public uint CssClass { get; private set; }

#if DEBUG
            public override string ToString()
            {
                return string.Format("{0} {1}", _type, _cssClass);
            }
#endif
        }
    }
}
