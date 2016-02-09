using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("background-image")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class BackgroundImage : Style<BackgroundImage>, IBackgroundImage
    {
        public BackgroundImage(long depth)
            : base(depth)
        {
        }

        public string Path { get; private set; }

        public override void FromString(string s)
        {
            s = s.Trim();
            var reg = new System.Text.RegularExpressions.Regex(@"url\((?<value>.+)\)");
            System.Text.RegularExpressions.Match match = reg.Match(s);
            if (match.Success)
                Path = match.Groups["value"].Value;
        }

        protected override bool Equals(BackgroundImage other)
        {
            return other.Path == Path;
        }

        protected override int GenerateHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }
    }
}
