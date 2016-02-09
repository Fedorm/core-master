using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.StyleSheet.Cache;

namespace BitMobile.StyleSheet
{
    public class StyleSheet : IStyleSheet
    {
        private readonly Dictionary<StyleSelector, List<IStyle>> _styles;
        private readonly Dictionary<IStyleKey, Dictionary<Type, IStyle>> _stylesByKey;
        private long _stylesCount;
        private IDisposable _cache;
        private bool _disposed;

        public StyleSheet()
        {
            Helper = new StyleSheetHelper(GetStyles);
            _styles = new Dictionary<StyleSelector, List<IStyle>>();
            _stylesByKey = new Dictionary<IStyleKey, Dictionary<Type, IStyle>>();
        }

        #region IStyleSheet

        public IStyleSheetHelper Helper { get; private set; }

        public IDictionary<Type, IStyle> GetStyles(object obj)
        {
            var control = obj as ILayoutable;
            if (control != null)
                return control.Styles;
            return new Dictionary<Type, IStyle>();
        }

        public void Assign(ILayoutable root)
        {
            AssignControl(root);
        }

        public void Load(Stream stream)
        {
            string styleSheet = ReadStream(stream);
            string[] expressions = styleSheet.Split('}');
            foreach (string expD in expressions)
            {
                string exp = expD.Trim();
                if (!string.IsNullOrEmpty(exp))
                {
                    string[] arr = exp.Split('{');
                    if (arr.Length != 2)
                        throw new Exception(string.Format("Css error: {0}", exp));

                    var selector = new StyleSelector(arr[0].Trim().ToLower());

                    foreach (string styleD in arr[1].Split(';'))
                    {
                        string style = styleD.Trim();
                        if (!string.IsNullOrEmpty(style))
                        {
                            string[] styleSplit = style.Split(':');
                            if (styleSplit.Length != 2)
                                throw new Exception(string.Format("Css error: {0}", style));
                            string attr = styleSplit[0].Trim();
                            string value = styleSplit[1].Trim();

                            if (!StyleSheetCache.StyleNames.ContainsKey(attr))
                                throw new Exception(string.Format("Style {0} is not found", attr));
                            Type attrType = StyleSheetCache.StyleNames[attr];

                            // ReSharper disable once PossibleNullReferenceException
                            var s = (IStyle)attrType.GetConstructor(new[] { typeof(long) })
                                .Invoke(new object[] { _stylesCount++ });
                            s.FromString(value);

                            if (!_styles.ContainsKey(selector))
                                _styles.Add(selector, new List<IStyle>());
                            _styles[selector].Add(s);
                        }
                    }
                }
            }
        }

        public IDictionary<Type, IStyle> StylesIntersection(IDictionary<Type, IStyle> oldStyles, IDictionary<Type, IStyle> newStyles)
        {
            var result = new Dictionary<Type, IStyle>(newStyles.Count);
            if (oldStyles != null && newStyles != oldStyles)
                foreach (var pair in newStyles)
                {
                    IStyle old;
                    if (oldStyles.TryGetValue(pair.Key, out old))
                    {
                        if (!old.Equals(pair.Value))
                            result.Add(pair.Key, pair.Value);
                    }
                    else
                        result.Add(pair.Key, pair.Value);
                }
            return result;
        }

        public T GetCache<T>() where T : class, IDisposable
        {
            if (_cache == null)
                return null;
            return (T)_cache;
        }

        public void SetCache<T>(T cache) where T : class, IDisposable
        {
            RemoveCache();
            _cache = cache;
        }

        public void RemoveCache()
        {
            if (_cache != null)
                _cache.Dispose();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StyleSheet()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Helper = null;

                if (_cache != null)
                    _cache.Dispose();
                _cache = null;

                _disposed = true;
            }
        }
        #endregion

        private static string ReadStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            var chars = new List<char>((int)stream.Length);
            bool inCommentary = false;

            while (!reader.EndOfStream)
            {
                var c = (char)reader.Read();
                if (inCommentary)
                {
                    if (c == '*')
                    {
                        var nextChar = (char)reader.Read();
                        if (nextChar == '/')
                            inCommentary = false;
                    }
                }
                else if (c == '/')
                {
                    var nextChar = (char)reader.Read();
                    if (nextChar == '*')
                        inCommentary = true;
                    else
                    {
                        chars.Add(c);
                        chars.Add(nextChar);
                    }
                }
                else
                    chars.Add(c);
            }

            return new string(chars.ToArray());
        }

        private void AssignControl(ILayoutable control)
        {
            if (control == null)
                throw new ArgumentException("Cannot assign this control");

            IStyleKey key = new StyleKey(control);

            Dictionary<Type, IStyle> styles;
            if (!_stylesByKey.TryGetValue(key, out styles))
            {
                styles = new Dictionary<Type, IStyle>();
                foreach (var item in _styles)
                {
                    StyleSelector selector = item.Key;
                    uint[] styleParts = selector.Parts;
                    int j = styleParts.Length - 1;
                    if (j >= 0)
                    {
                        bool allowed = false;
                        for (int i = key.Parts.Length - 1; i >= 0; i--)
                        {
                            IStyleKeyPart keyPart = key.Parts[i];
                            uint styleItem = styleParts[j];
                            if (styleItem == keyPart.Type || styleItem == keyPart.CssClass)
                            {
                                allowed = true;
                                j--;
                                if (j < 0)
                                    break;
                            }
                            else
                            {
                                allowed = false;
                                break;
                            }
                        }

                        if (allowed)
                            foreach (IStyle style in item.Value)
                            {
                                styles.Remove(style.GetType());
                                styles.Add(style.GetType(), style);
                            }
                    }
                }

                _stylesByKey.Add(key, styles);
            }

            control.StyleKey = key;
            control.Styles = styles;

            var container = control as IContainer;
            if (container != null)
                foreach (object child in container.Controls)
                    AssignControl(child as ILayoutable);
        }
    }
}

