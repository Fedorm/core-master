namespace Telegram.Service
{
    /// <summary>
    ///     RPC ответ
    /// </summary>
    class RpcAnswer
    {
        public RpcAnswer(long sessionId, Combinator answer)
            : this(answer)
        {
            SessionId = sessionId;
        }

        public RpcAnswer(Combinator answer)
        {
            Success = answer.Name != "rpc_error";
            if (Success)
                Combinator = answer;
            else
                Error = new RpcError(answer.Get<int>("error_code"), answer.Get<string>("error_message"));
        }

        public RpcAnswer(RpcError error)
        {
            Success = false;
            Error = error;
        }

        public long SessionId { get; set; }

        /// <summary>
        ///     Результат запроса
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        ///     Ответ
        /// </summary>
        public Combinator Combinator { get; private set; }

        /// <summary>
        ///     Ошиюка
        /// </summary>
        public RpcError Error { get; private set; }

        public override string ToString()
        {
            return Success ? Combinator.ToString() : Error.ToString();
        }
    }
}