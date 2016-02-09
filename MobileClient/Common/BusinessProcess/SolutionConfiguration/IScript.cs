namespace BitMobile.Common.BusinessProcess.SolutionConfiguration
{
    public interface IScript
    {
        IGlobalModules GlobalModules { get; set; }
        IMixins Mixins { get; set; }
        IGlobalEvents GlobalEvents { get; set; }
        IWarmupActions WarmupActions { get; set; }
        object[] Controls { get; }
        void AddChild(object obj);
        object GetControl(int index);
    }
}