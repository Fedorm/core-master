using BitMobile.Common.Log;

namespace BitMobile.Application.Log
{
    public static class LogManager
    {
        public static ILogger Logger { get; private set; }

        public static IReporter Reporter { get; private set; }

        public static void Init(ILogger logger, IReporter reporter)
        {
            Logger = logger;
            Reporter = reporter;
        }
    }
}
