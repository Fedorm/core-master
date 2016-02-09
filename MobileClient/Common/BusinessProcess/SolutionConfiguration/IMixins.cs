namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IMixins
    {
        void AddChild(object obj);
        object[] Controls { get; }
        object GetControl(int index);
    }
}
