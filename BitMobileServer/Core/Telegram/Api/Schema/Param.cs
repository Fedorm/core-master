namespace Telegram.Schema
{
    // ReSharper disable InconsistentNaming
    public class Param
    {
        public string name { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            return name + ":" + type;
        }
    }
}
