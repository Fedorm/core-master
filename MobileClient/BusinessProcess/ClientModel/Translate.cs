using BitMobile.Common.Application;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Translate
    {
        readonly IApplicationContext _context;

        public Translate(IApplicationContext context)
        {
            _context = context;
        }

        public object this[string key]
        {
            get
            {
                return _context.Dal.TranslateString(key);
            }
        }
    }
}
