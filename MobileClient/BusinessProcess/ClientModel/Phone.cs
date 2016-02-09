using BitMobile.Common.Application;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    class Phone
    {
        readonly IApplicationContext _context;

        public Phone(IApplicationContext context)
        {
            _context = context;
        }

        public void Call(string number)
        {
            _context.PhoneCall(number);
        }
    }
}
