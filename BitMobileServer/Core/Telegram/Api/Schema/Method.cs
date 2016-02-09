namespace Telegram.Schema
{
    // ReSharper disable InconsistentNaming
    public class Method : SchemaCombinator
    {
        public string method { get; set; }

        public override string GetName()
        {
            return method;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        protected override string ToString(bool needId)
        {
            return method + base.ToString(needId);
        }
    }
}
