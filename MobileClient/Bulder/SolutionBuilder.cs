using BitMobile.Application;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Controls;
using BitMobile.Application.DataAccessLayer;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Debugger;
using BitMobile.Application.Entites;
using BitMobile.Application.ExpressionEvaluator;
using BitMobile.Application.IO;
using BitMobile.Application.Log;
using BitMobile.Application.ScriptEngine;
using BitMobile.Application.StyleSheet;
using BitMobile.Application.SyncLibrary;
using BitMobile.Application.ValueStack;
using BitMobile.Common.Application;
using BitMobile.Common.Log;
using BitMobile.Log;
using BitMobile.SyncLibrary.BitMobile;

namespace BitMobile.Bulder
{
    public class SolutionBuilder
    {
        private readonly IApplicationContext _applicationContext;

        public SolutionBuilder(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Build()
        {
            ApplicationContext.Init(_applicationContext);
            BusinessProcessContext.Init(new BusinessProcess.BusinessProcessContext());
            IOContext.Init(new IO.IOContext(_applicationContext));
            ControlsContext.Init(new Controls.ControlsContext());
            DbContext.Init(new DbEngine.DbContext());
            ExpressionContext.Init(new ExpressionEvaluator.ExpressionContext(_applicationContext));
            ValueStackContext.Init(new ValueStack.ValueStackContext(DbContext.Current, ExpressionContext.Current));
            ScriptEngineContext.Init(new ScriptEngine.ScriptEngineContext());
            InitLogManager();
            DebugContext.Init(new Debugger.DebugContext());
            SyncContext.Init(new SyncLibrary.SyncContext());
            DalContext.Init(new DataAccessLayer.DalContext());
            StyleSheetContext.Init(new StyleSheet.StyleSheetContext());
            InitEntityFactory();
        }

        private void InitLogManager()
        {
            IReportSender reportSender;
            if (string.IsNullOrWhiteSpace(_applicationContext.Settings.DevelopersEmail))
                reportSender = new ZendeskReportSender();
            else
                reportSender = _applicationContext.EmailProvider;

            LogManager.Init(new Logger(_applicationContext.Settings.LogLifetime, _applicationContext.Settings.LogMinCount)
                , new Reporter(reportSender));
        }

        private void InitEntityFactory()
        {
            EntityFactory.CreateInstance = Entity.CreateInstance;
            EntityFactory.DbRefFactory = DbContext.Current.CreateDbRef;
            EntityFactory.CustomDictionaryFactory = ValueStackContext.Current.CreateDictionary;
        }
    }
}
