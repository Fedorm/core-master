namespace BitMobile.BusinessProcess.ClientModel
{
    public class Error
    {
        public Error(string name, string message)
        {
            Name = name;
            Message = message;
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable MemberCanBePrivate.Global
        public string Name { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return Message;
        }
    }
}