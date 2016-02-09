using System;
using System.Linq;
using BitMobile.Application.ValueStack;
using BitMobile.Common.Application;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Variables : IIndexedProperty
    {
        private readonly IApplicationContext _context;
        private readonly string[] _forbiddenKeys = { "common", "context", "dao", "activity" };

        // Не надо впихивать Constructor Injection, так как ValueStack каждый раз новый, при открытии экрана
        public Variables(IApplicationContext context)
        {
            _context = context;
        }

        public object this[string index]
        {
            get
            {
                return _context.ValueStack.Values[index];
            }
            set
            {
                Validate(index);

                _context.ValueStack.Values[index] = value;
            }
        }


        public IApplicationContext GetContext()
        {
            return _context;
        }

        public void Add(string name, object value)
        {
            Validate(name);
            _context.ValueStack.Push(name, value);
        }

        public void AddGlobal(string name, object value)
        {
            Add(name, value);
            _context.GlobalVariables.Add(name, value);
        }

        public void Remove(string name)
        {
            Validate(name);
            if (_context.GlobalVariables.ContainsKey(name))
                _context.GlobalVariables.Remove(name);

            _context.ValueStack.Pull(name);
        }

        public void CreateCollection(string name)
        {
            Add(name, ValueStackContext.Current.CreateDictionary());
        }

        public void CreateCollectionGlobal(string name)
        {
            AddGlobal(name, ValueStackContext.Current.CreateDictionary());
        }

        public bool Exists(string key)
        {
            return _context.ValueStack.Values.ContainsKey(key);
        }

        void Validate(string key)
        {
            if (_forbiddenKeys.Contains(key))
                throw new ArgumentException(string.Format("Cannot complete operation. {0} is forbidden keyword", key));
        }

        //-------------------------------IIndexedProperty-----------------------------------------------------

        public object GetValue(string propertyName)
        {
            return _context.ValueStack.Values[propertyName];
        }

        public bool HasProperty(string propertyName)
        {
            return _context.ValueStack.Values.ContainsKey(propertyName);
        }
    }
}
