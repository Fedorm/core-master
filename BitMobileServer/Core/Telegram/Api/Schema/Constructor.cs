namespace Telegram.Schema
{
    // ReSharper disable InconsistentNaming
    public class Constructor : SchemaCombinator
    {
        public string predicate { get; set; }

        public override string GetName()
        {
            return predicate;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        protected override string ToString(bool needId)
        {
            return predicate + base.ToString(needId);
        }
    }
}
