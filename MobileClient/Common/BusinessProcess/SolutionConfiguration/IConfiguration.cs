namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IConfiguration
    {
        IBusinessProcess BusinessProcess { get; set; }
        IScript Script { get; set; }
        IStyle Style { get; set; }
        object[] Controls { get; }
        void AddChild(object obj);
        object GetControl(int index);
    }
}