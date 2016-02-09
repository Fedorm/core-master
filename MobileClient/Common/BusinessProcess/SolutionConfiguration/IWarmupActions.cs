namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IWarmupActions
    {
        void AddChild(object obj);
        object[] Controls { get; }
        object GetControl(int index);
    }
}
