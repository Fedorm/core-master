namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IGlobalModules
    {
        void AddChild(object obj);
        object[] Controls { get; }
        object GetControl(int index);
    }
}
