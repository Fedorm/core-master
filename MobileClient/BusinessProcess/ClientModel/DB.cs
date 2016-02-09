using System;
using System.Collections;
using System.IO;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Entites;
using BitMobile.Application.Exceptions;
using BitMobile.Common.Application;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.DbEngine;
using BitMobile.Common.ScriptEngine;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Db
    {
        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _context;

        IJsExecutable _handler;
        object _state;

        public Db(IScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public string LastError { get; private set; }

        public object Current
        {
            get
            {
                return _context.Dal.Dao;
            }
        }

        public int Size
        {
            get
            {
                using (Stream stream = DbContext.Current.GetDatabaseStream())
                    return (int)stream.Length / 1000;
            }
        }

        public void Save()
        {
            _context.Dal.SaveChanges();
        }

        public void Sync()
        {
            LastError = null;

            ControlsContext.Current.ActionHandlerLocker.Acquire();

            _context.Dal.RefreshAsync(SyncComplete);
        }

        public void Sync(IJsExecutable handler)
        {
            _handler = handler;
            Sync();
        }

        public void Sync(IJsExecutable handler, object state)
        {
            _state = state;
            Sync(handler);
        }

        public void CreateTable(string tableName, ArrayList columns)
        {
            DbContext.Current.Database.CreateUserTable(tableName, columns.OfType<string>().ToArray());
        }

        public void DropTable(string tableName)
        {
            DbContext.Current.Database.DropUserTable(tableName);
        }

        public void TruncateTable(string tableName)
        {
            DbContext.Current.Database.TruncateUserTable(tableName);
        }

        public object Create(string name)
        {
            string[] arr = name.Split('.');
            if (arr.Length == 2)
                return EntityFactory.CreateEntity(name);

            throw new Exception(String.Format("Cant create instance of type '{0}'", name));
        }

        public object SelectById(string name, string guid)
        {
            String[] arr = name.Split('.');
            if (arr.Length == 2)
            {
                EntityType t = EntityFactory.FindType(name);
                return DbContext.Current.Database.SelectById(t, guid);
            }
            throw new Exception(String.Format("Cant create instance of type '{0}'", name));
        }

        public void Delete(IDbRef obj)
        {
            Delete(obj, true);
        }

        public void Delete(IDbRef obj, bool inTran)
        {
            if (obj != null)
            {
                DbContext.Current.Database.Delete(obj, inTran);
                if (obj.HasCache)
                {
                    var entity = (ISqliteEntity)obj.GetObject();
                    entity.IsTombstone = true;
                }
            }
        }

        public IDbRef EmptyRef(string tableName)
        {
            return DbContext.Current.CreateDbRef(tableName, Guid.Empty);
        }

        public IDbRef CreateRef(string tableName, string guidString)
        {
            Guid guid;
            if (Guid.TryParse(guidString, out guid))
                return DbContext.Current.CreateDbRef(tableName, Guid.Empty);
            throw _scriptEngine.CreateException(new Error("DBException", "guid is empty"));
        }

        public Guid AsGuid(string guid)
        {
            return Guid.Parse(guid);
        }

        public void Commit()
        {
            DbContext.Current.Database.CommitTransaction();
        }

        public void Rollback()
        {
            DbContext.Current.Database.RollbackTransaction();
        }

        public DateTime LastSyncTime
        {
            get
            {
                return DbContext.Current.Database.LastSyncTime;
            }
        }

        public bool SuccessSync
        {
            get
            {
                return DbContext.Current.Database.SuccessSync;
            }
        }

        void SyncComplete(object sender, ISyncEventArgs e)
        {
            ControlsContext.Current.ActionHandlerLocker.Release();

            var common = (ICommonData)_context.ValueStack.Values["common"];
            common.SyncIsOK = e.Ok;

            DbContext.Current.Database.SyncComplete(e.Ok);

            if (_handler != null)
                _context.InvokeOnMainThread(() =>
                {
                    _handler.ExecuteCallback(_scriptEngine.Visitor, _state, null);
                    _state = null;
                    _handler = null;
                });

            if (!e.Ok)
            {
                LastError = e.Exception.Message;
                _context.HandleException(e.Exception);
            }
        }
    }
}

