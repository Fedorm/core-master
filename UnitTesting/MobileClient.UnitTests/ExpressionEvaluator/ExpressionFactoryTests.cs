using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BitMobile.ExpressionEvaluator;
using System.Collections;

namespace MoblieClient.Tests.ExpressionEvaluator
{
    [TestClass]
    public class ExpressionFactoryTests
    {
        List<FakeClass> _collectionMock;
        ExpressionFactory _factory;

        [TestInitialize]
        public void Setup()
        {
            _collectionMock = new List<FakeClass>();
            _collectionMock.Add(new FakeClass(0, "Alex", new DateTime(1989, 11, 14), 99.9));
            _collectionMock.Add(new FakeClass(1, "Sasha", new DateTime(1989, 11, 14), 42.3));

            for (int i = 0; i < 10000; i++)
            {
                _collectionMock.Add(new FakeClass(i + 2, Guid.NewGuid().ToString(), DateTime.Now, i + 42));
            }

            Dictionary<string, object> valueStack = new Dictionary<string, object>();
            BitMobile.ValueStack.CustomDictionary workflow = new BitMobile.ValueStack.CustomDictionary();
            workflow.Add("name", "main");
            workflow.Add("null", null);
            valueStack.Add("workflow", workflow);

            _factory = new ExpressionFactory(valueStack, typeof(FakeClass));
            _factory.AddParameter("name", "Alex");
            _factory.AddParameter("birth", new DateTime(1989, 11, 14));
            _factory.AddParameter("money", 42.3);
            _factory.AddParameter("id", 1);
            _factory.AddParameter("collection", new string[] { "Alex", "sasha" });
        }
        
        [TestMethod]
        public void LogicalExpression_StandartInput_ReturnsFiltered()
        {
            List<FakeClass> expected = _collectionMock.Where((val) =>
                {
                    return val.BirthDate.ToUniversalTime().Date.AddYears(1) == DateTime.Parse("1990.11.13")
                         && (val.Money > 42.3 || val.Name != "Alex" || val.Id >= 1)
                         && ((string)((IDictionary<string, object>)_factory.ValueStack["workflow"])["name"]).Length == 4
                         && ((IDictionary<string, object>)_factory.ValueStack["workflow"])["null"] == null
                         && true
                         && ((ICollection<string>)_factory.Parameters["collection"]).Contains(val.Name);
                }).ToList();


            Func<object, bool> func = _factory.BuildLogicalExpression(@"
BirthDate.ToUniversalTime().Date.AddYears(1) == 1990.11.13 
&& (Money > 42.3 || Name != @name || Id >= @id)
&& $workflow.name.Length == 4
&& $workflow.null == nuLL
&& TruE
&& Name IN @collection
");
            List<object> actual = _collectionMock.Where(func).ToList();

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void ArithmeticExpression_StandartInput_ReturnsSum()
        {
            decimal expected = _collectionMock.Sum(val => val.Age * (10 + val.BirthDate.AddYears(val.Age).Year));

            Func<object, decimal> func = _factory.BuildArithmeticExpression(@"
Age*(10 + BirthDate.AddYears(Age).Year ) 
");
            decimal actual = _collectionMock.Sum(func);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ValueExpression_StandartInput_ReturnsValue()
        {
            List<int> expected = _collectionMock.Select(val => val.Age).ToList();

            Func<object, object> func = _factory.BuildValueExpression(@"
Age 
");
            List<object> actual = _collectionMock.Select(func).ToList();

            Assert.AreEqual(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void LogicalExpression_InputOnlyFomValueStackAndParameters_ReturnsBool()
        {
            Dictionary<string, object> valueStack = new Dictionary<string, object>();
            valueStack.Add("str", "test");
            valueStack.Add("int", 5);
            valueStack.Add("date", new DateTime(1989, 11, 14));

            ExpressionFactory factory = new ExpressionFactory(valueStack);
            factory.AddParameter("date", new DateTime(2012, 1, 1));
            factory.AddParameter("null", null);

            bool expected = (string)factory.ValueStack["str"] == "test"
                && ((int)factory.ValueStack["int"] >= 5 || (DateTime)factory.ValueStack["date"] != new DateTime(1989, 11, 14)
                && ((DateTime)factory.Parameters["date"]).AddYears(1).Year < 2000)
                && ((string)factory.ValueStack["str"]).Contains("na");

            var func = factory.BuildLogicalExpression(@"
$str == 'test' 
&& (  $int >= 5 || $date != 1989.11.14) 
&& @date.AddYears(1).Year > 2000
&& $str.Contains('na')
&& @null == NuLL");

            bool actual = func(null);

            Assert.AreEqual(expected, actual);
        }

        class FakeClass
        {
            public FakeClass(int id, string name, DateTime birthDate, double money)
            {
                Id = id;
                Name = name;
                BirthDate = birthDate;
                Money = money;
            }

            public int Id { get; private set; }
            public string Name { get; private set; }
            public DateTime BirthDate { get; private set; }
            public int Age
            {
                get
                {
                    return (DateTime.MinValue + (DateTime.Now - BirthDate)).Year - 1;
                }
            }
            public double Money { get; private set; }

            public override string ToString()
            {
                return Name
                    + "\t" + BirthDate.ToString()
                    + "\t" + Age.ToString()
                    + "\t" + Money.ToString();
            }
        }
    }
}


namespace BitMobile.ValueStack
{
    class CustomDictionary : Dictionary<string, object>
    {
        public object GetValue(String key)
        {
            object result = null;
            if (TryGetValue(key, out result))
                return result;
            else
                return null;
        }
    }
}