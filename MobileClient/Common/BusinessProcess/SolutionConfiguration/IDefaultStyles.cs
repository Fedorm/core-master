namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IDefaultStyles
    {
        void AddChild(object obj);
        object[] Controls { get; }
        object GetControl(int index);
    }
}