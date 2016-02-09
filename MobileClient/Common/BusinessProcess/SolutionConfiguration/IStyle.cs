namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IStyle
    {
        IDefaultStyles DefaultStyles { get; set; }
        object[] Controls { get; }
        void AddChild(object obj);
        object GetControl(int index);
    }
}
