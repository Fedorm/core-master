using BitMobile.ExpressionEvaluator;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BitMobile.SyncLibrary
{
    public class ProxyCollection<T> : IEnumerable<T>, IEnumerable
    {
        IEnumerable<T> _collection;
        [NonSerialized]
        IsolatedStorageOfflineContext _context;

        IEnumerator<T> _enumerator;

        public ProxyCollection(IEnumerable<T> source, IsolatedStorageOfflineContext context)
        {
            _collection = source;
            _context = context;
        }

        public ProxyCollection<T> Where(string predicate)
        {
            return Where(predicate, new object[0]);
        }

        public ProxyCollection<T> Where(string predicate, IEnumerable conditionParameters)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int i = 1;
            foreach (var parameter in conditionParameters)
            {
                object p = parameter;

                // it is need for calculating linq
                IEnumerable enumerable = parameter as IEnumerable;
                if (enumerable != null && !(parameter is string))
                {
                    ArrayList list = new ArrayList();
                    foreach (var item in enumerable)
                        list.Add(item);
                    p = list;
                }

                parameters.Add(string.Format("p{0}", i), p);
                i++;
            }

            ExpressionFactory factory = new ExpressionFactory(_context, typeof(T), parameters);
            Func<object, bool> func = factory.BuildLogicalExpression(predicate);

            IEnumerable<T> c = _collection.Where<T>(val => func(val));

            return new ProxyCollection<T>(c, _context);
        }

        public ProxyCollection<object> Distinct(string field)
        {
            ExpressionFactory factory = new ExpressionFactory(_context, typeof(T));
            Func<object, object> func = factory.BuildValueExpression(field);

            IEnumerable<object> c = _collection.Select(val => func(val)).Distinct();
            return new ProxyCollection<object>(c, _context);
        }

        public ProxyCollection<T> Union(ProxyCollection<T> collection)
        {
            IEnumerable<T> c = _collection.Intersect(collection);
            return new ProxyCollection<T>(c, _context);
        }

        public ProxyCollection<T> UnionAll(ProxyCollection<T> collection)
        {
            IEnumerable<T> c = _collection.Union(collection);
            return new ProxyCollection<T>(c, _context);
        }

        public ProxyCollection<T> OrderBy(string field)
        {
            return OrderBy(field, false);
        }

        public ProxyCollection<T> OrderBy(string field, bool desc)
        {
            ExpressionFactory factory = new ExpressionFactory(_context, typeof(T));
            Func<object, object> func = factory.BuildValueExpression(field);

            IEnumerable<T> c;
            if (!desc)
                c = _collection.OrderBy(val => func(val));
            else
                c = _collection.OrderByDescending(val => func(val));

            return new ProxyCollection<T>(c, _context);
        }

        public ProxyCollection<T> Top(int count)
        {
            if (count > 0)
            {
                IEnumerable<T> c = _collection.Take(count);
                return new ProxyCollection<T>(c, _context);
            }
            else
                return this;
        }

        public decimal Sum(string predicate)
        {
            return Sum(predicate, new object[0]);
        }

        public decimal Sum(string predicate, IEnumerable conditionParameters)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int i = 1;
            foreach (var parameter in conditionParameters)
            {
                object p = parameter;

                // it is need for calculating linq
                IEnumerable enumerable = parameter as IEnumerable;
                if (enumerable != null && !(parameter is string))
                {
                    ArrayList list = new ArrayList();
                    foreach (var item in enumerable)
                        list.Add(item);
                    p = list;
                }

                parameters.Add(string.Format("p{0}", i), p);
                i++;
            }

            ExpressionFactory factory = new ExpressionFactory(_context, typeof(T), parameters);
            Func<object, decimal> func = factory.BuildArithmeticExpression(predicate);

            decimal result = _collection.Sum(val => func(val));

            return result;
        }

        public T First()
        {
            return _collection.FirstOrDefault();
        }

        public int Count()
        {
            return _collection.Count();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
