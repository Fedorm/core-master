using BitMobile.Application;
using BitMobile.ValueStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.ClientModel
{
    public class Variables : IIndexedProperty, BitMobile.ValueStack.IEvaluator
    {
        IApplicationContext _context;
        public IApplicationContext GetContext()
        {
            return _context;
        }

        string[] _forbiddenKeys = { "common", "context", "dao", "activity" };

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
            this.Add(name, new CustomDictionary());
        }

        public void CreateCollectionGlobal(string name)
        {
            this.AddGlobal(name, new CustomDictionary());
        }

        public bool Exists(string key)
        {            
            return _context.ValueStack.Values.ContainsKey(key);
        }

        bool Validate(string key)
        {
            if (_forbiddenKeys.Contains(key))
                throw new ArgumentException(string.Format("Cannot complete operation. {0} is forbidden keyword", key));
            else
                return true;
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

        //-------------------------------IEvaluator-----------------------------------------------------

        public object Evaluate(string expression, object root)
        {
            return _context.ValueStack.Evaluate(expression, root);
        }
    }
}
