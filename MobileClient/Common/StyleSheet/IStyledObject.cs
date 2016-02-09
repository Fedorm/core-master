namespace BitMobile.Common.StyleSheet
{
    public interface IStyledObject
    {
        string Name { get; }
        string CssClass { get; set; }
        IBound ApplyStyles(IStyleSheet stylesheet, IBound styleBound, IBound maxBound);
    }
}

