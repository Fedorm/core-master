using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BitMobile.Common.Entites;
using BitMobile.ValueStack;
using BitMobile.DataAccessLayer;
using BitMobile.Script;
using BitMobile.Application;
using BitMobile.Controls;
using BitMobile.DbEngine;

namespace BitMobile.ClientModel
{
    public class DB
    {
        ScriptEngine _scriptEngine;
        IApplicationContext _context;

        IJSExecutable _handler;
        object _state;

        public DB(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public string LastError { get; private set; }

        public object Current
        {
            get
            {
                return _context.DAL.DAO;
            }
        }

        public void Save()
        {
            _context.DAL.SaveChanges();
        }

        public void Sync()
        {
            LastError = null;

            ActionHandler.Busy = true;
            ActionHandlerEx.Busy = true;

            _context.DAL.RefreshAsync(SyncComplete);
        }

        public void Sync(IJSExecutable handler)
        {
            _handler = handler;
            Sync();
        }

        public void Sync(IJSExecutable handler, object state)
        {
            _state = state;
            Sync(handler);
        }

        public void CreateTable(String tableName, String[] columns)
        {
            Database.Current.CreateUserTable(tableName, columns);
        }

        public void DropTable(String tableName)
        {
            Database.Current.DropUserTable(tableName);
        }

        public void TruncateTable(String tableName)
        {
            Database.Current.TruncateUserTable(tableName);
        }

        public object Create(String name)
        {
            String[] arr = name.Split('.');
            if (arr.Length == 2)
                return EntityFactory.CreateEntity(name);

            throw new Exception(String.Format("Cant create instance of type '{0}'", name));
        }

        public object SelectById(String name, String guid)
        {
            String[] arr = name.Split('.');
            if (arr.Length == 2)
            {
                EntityType t = EntityFactory.FindType(name);
                return Database.Current.SelectById(t, guid);
            }
            throw new Exception(String.Format("Cant create instance of type '{0}'", name));
        }

        public void Delete(BitMobile.DbEngine.IDbRef obj)
        {
            if (obj != null)
                Database.Current.Delete(obj, true);
        }

        public void Delete(BitMobile.DbEngine.IDbRef obj, bool inTran)
        {
            if (obj != null)
                Database.Current.Delete(obj, inTran);
        }

        public DbEngine.DbRef EmptyRef(String tableName)
        {
            return DbRef.CreateInstance(tableName, Guid.Empty);
        }

        public Guid AsGuid(String guid)
        {
            return Guid.Parse(guid);
        }

        public void Commit()
        {
            Database.Current.CommitTransaction();
        }

        public void Rollback()
        {
            Database.Current.RollbackTransaction();
        }

        public DateTime LastSyncTime
        {
            get
            {
                return Database.Current.LastSyncTime;
            }
        }

        public bool SuccessSync
        {
            get
            {
                return Database.Current.SuccessSync;
            }
        }

        void SyncComplete(object sender, SyncEventArgs e)
        {
            ActionHandler.Busy = false;
            ActionHandlerEx.Busy = false;

            CommonData common = (CommonData)_context.ValueStack.Values["common"];
            common.SyncIsOK = e.OK;

            DbEngine.Database.Current.SyncComplete(e.OK);

            if (_handler != null)
                _context.InvokeOnMainThread(() =>
                {
                    _handler.ExecuteStandalone(_scriptEngine.Visitor, new object[] { _state });
                    _state = null;
                    _handler = null;
                });

            if (!e.OK)
            {
                LastError = e.Exception.Message;
                _context.HandleException(e.Exception);
            }
        }
    }
}

