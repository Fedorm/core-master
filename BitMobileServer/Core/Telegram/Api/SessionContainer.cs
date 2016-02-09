namespace Telegram
{
    internal class SessionContainer
    {
        public SessionContainer(long sessionId, byte[] raw)
            : this(sessionId)
        {
            Combinator = new Combinator(raw, null);
        }

        public SessionContainer(long sessionId, Combinator combinator)
            : this(sessionId)
        {
            Combinator = combinator;
        }

        private SessionContainer(long sessionId)
        {
            SessionId = sessionId;
        }

        public long SessionId { get; private set; }

        public Combinator Combinator { get; private set; }

        public override string ToString()
        {
            return string.Format("{1}; #{0}", SessionId, Combinator);
        }
    }
}