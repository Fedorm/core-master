using BitMobile.Common.Application;

namespace BitMobile.BusinessProcess.ClientModel
{
    class Clipboard
    {
        private readonly IApplicationContext _context;

        public Clipboard(IApplicationContext context)
        {
            _context = context;
        }

        public bool HasStringValue
        {
            get { return _context.ClipboardProvider.HasStringValue; }
        }

        public bool SetString(string text)
        {
            return _context.ClipboardProvider.SetString(text);
        }

        public string GetString()
        {
            return _context.ClipboardProvider.GetString();
        }
    }
}
