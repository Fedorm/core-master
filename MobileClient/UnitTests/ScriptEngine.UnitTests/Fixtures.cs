using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using Jint;
using Jint.Debugger;
using Jint.Expressions;
using Jint.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.ScriptEngine.UnitTests
{
    [TestClass]
    public class Fixtures
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        protected object Test(Options options, params string[] scripts)
        {
            JintEngine jint = CreateJintEngine()
                //.SetDebugMode(true)
                .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
                .SetFunction("istrue", new Action<bool>(Assert.IsTrue))
                .SetFunction("isfalse", new Action<bool>(Assert.IsFalse))
                // .SetFunction("alert", new Func<string, System.Windows.Forms.DialogResult>(System.Windows.Forms.MessageBox.Show))
                .SetFunction("print", new Action<string>(Console.WriteLine))
                .SetFunction("alert", new Action<string>(Console.WriteLine))
                .SetFunction("loadAssembly", new Action<string>(assemblyName => Assembly.Load(assemblyName)))
                .DisableSecurity();
            //jint.BreakPoints.Add(new BreakPoint(3741, 9));
            //jint.Break += new EventHandler<DebugInformation>(jint_Break);

            object result = null;

            var sw = new Stopwatch();
            sw.Start();

            foreach (string script in scripts)
                result = jint.Run(script);

            Console.WriteLine(sw.Elapsed);

            return result;
        }

        private void ExecuteEmbededScript(string scriptName)
        {
            const string prefix = "BitMobile.ScriptEngine.UnitTests.Scripts.";
            var script = prefix + scriptName;

            var assembly = Assembly.GetExecutingAssembly();
            var program = new StreamReader(assembly.GetManifestResourceStream(script)).ReadToEnd();
            Test(program);
        }

        protected object Test(params string[] scripts)
        {
            return Test(Options.Ecmascript3 | Options.Strict, scripts);
        }

        [TestMethod]
        public void ShouldHandleDictionaryObjects()
        {
            var dic = new JsObject();
            dic["prop1"] = new JsNumber(1);
            Assert.IsTrue(dic.HasProperty(new JsString("prop1")));
            Assert.IsTrue(dic.HasProperty("prop1"));
            Assert.AreEqual(1, dic["prop1"].ToNumber());
        }

        [TestMethod]
        public void ShouldRunInRun()
        {
            var filename = Path.GetTempFileName();
            File.WriteAllText(filename, "a='bar'");

            var engine = CreateJintEngine().AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            engine.SetFunction("load", new Action<string>(delegate(string fileName) { using (var reader = File.OpenText(fileName)) { engine.Run(reader); } }));
            engine.SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run("var a='foo'; load('" + JintEngine.EscapteStringLiteral(filename) + "'); print(a);");

            File.Delete(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Security.SecurityException))]
        [Ignore]
        public void ShouldNotRunInRun()
        {
            var filename = Path.GetTempFileName();
            File.WriteAllText(filename, "a='bar'");

            var engine = CreateJintEngine().AddPermission(new FileIOPermission(PermissionState.None));
            engine.SetFunction("load", new Action<string>(delegate(string fileName) { using (var reader = File.OpenText(fileName)) { engine.Run(reader); } }));
            engine.SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run("var a='foo'; load('" + JintEngine.EscapteStringLiteral(filename) + "'); print(a);");
        }

        [TestMethod]
        public void ShouldSupportCasting()
        {
            const string script = @";
                var value = Number(3);
                assert('number', typeof value);
                value = String(value); // casting
                assert('string', typeof value);
                assert('3', value);
            ";

            Test(script);

        }

        [TestMethod]
        public void ShouldCompareNullValues()
        {
            const string script = @";
                if(null == 1) 
                    assert(true, false); 

                if(null != null) 
                    assert(true, false); 
                
                assert(true, true);
            ";

            Test(script);
        }


        [TestMethod]
        public void ShouldModifyIteratedCollection()
        {
            const string script = @";
                var values = [ 0, 1, 2 ];

                for (var v in values)
                {
                    values[v] = v * v;
                }

                assert(0, values[0]);
                assert(1, values[1]);
                assert(4, values[2]);
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldHandleTheMostSimple()
        {
            Test("var i = 1; assert(1, i);");
        }

        [TestMethod]
        public void ShouldHandleAnonymousFunctions()
        {
            const string script = @"
                function oksa(x, y) { return x + y; }
                assert(3, oksa(1, 2));
            ";

            Test(script);
        }

        [TestMethod]
        [Ignore]
        public void ShouldSupportUtf8VariableNames()
        {
            const string script = @"
                var 経済協力開発機構 = 'a strange variable';
                var Sébastien = 'a strange variable';
                assert('a strange variable', 経済協力開発機構);
                assert('a strange variable', Sébastien);
                assert('undefined', typeof sébastien);
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldHandleReturnAsSeparator()
        {
            Test(@" var i = 1; assert(1, i) ");
        }

        [TestMethod]
        public void ShouldHandleAssignment()
        {
            Test("var i; i = 1; assert(1, i);");
            Test("var i = 1; i = i + 1; assert(2, i);");
        }

        [TestMethod]
        public void ShouldHandleEmptyStatement()
        {
            Assert.AreEqual(1d, CreateJintEngine().Run(";;;;var i = 1;;;;;;;; return i;;;;;"));
        }

        [TestMethod]
        public void ShouldHandleFor()
        {
            Assert.AreEqual(9d, CreateJintEngine().Run("var j = 0; for(i = 1; i < 10; i = i + 1) { j = j + 1; } return j;"));
        }

        [TestMethod]
        public void ShouldHandleSwitch()
        {
            Assert.AreEqual(1d, CreateJintEngine().Run("var j = 0; switch(j) { case 0 : j = 1; break; case 1 : j = 0; break; } return j;"));
            Assert.AreEqual(2d, CreateJintEngine().Run("var j = -1; switch(j) { case 0 : j = 1; break; case 1 : j = 0; break; default : j = 2; } return j;"));
        }

        [TestMethod]
        [Ignore]
        public void SwitchShouldFallBackWhenNoBreak()
        {
            Test(@"
                function doSwitch(input) {
                    var result = 0;
                    switch(input) {
                         case 'a':
                         case 'b':
                             result = 2;
                             break;
                          case 'c':
                              result = 3;
                             break;
                          case 'd':
                               result = 4;
                               break;
                          default:
                               break;
                    }
                    return result;
                }

                assert(2, doSwitch('a'));
                assert(0, doSwitch('z'));
                assert(2, doSwitch('b'));
                assert(3, doSwitch('c'));
                assert(4, doSwitch('d'));
            ");
        }
        [TestMethod]
        public void ShouldHandleVariableDeclaration()
        {
            Assert.AreEqual(null, CreateJintEngine().Run("var i; return i;"));
            Assert.AreEqual(1d, CreateJintEngine().Run("var i = 1; return i;"));
            Assert.AreEqual(2d, CreateJintEngine().Run("var i = 1 + 1; return i;"));
            Assert.AreEqual(3d, CreateJintEngine().Run("var i = 1 + 1; var j = i + 1; return j;"));
        }

        [TestMethod]
        public void ShouldHandleUndeclaredVariable()
        {
            Assert.AreEqual(1d, CreateJintEngine().Run("i = 1; return i;"));
            Assert.AreEqual(2d, CreateJintEngine().Run("i = 1 + 1; return i;"));
            Assert.AreEqual(3d, CreateJintEngine().Run("i = 1 + 1; j = i + 1; return j;"));
        }

        [TestMethod]
        public void ShouldHandleStrings()
        {
            Assert.AreEqual("hello", CreateJintEngine().Run("return \"hello\";"));
            Assert.AreEqual("hello", CreateJintEngine().Run("return 'hello';"));

            Assert.AreEqual("hel'lo", CreateJintEngine().Run("return \"hel'lo\";"));
            Assert.AreEqual("hel\"lo", CreateJintEngine().Run("return 'hel\"lo';"));

            Assert.AreEqual("hel\"lo", CreateJintEngine().Run("return \"hel\\\"lo\";"));
            Assert.AreEqual("hel'lo", CreateJintEngine().Run("return 'hel\\'lo';"));

            Assert.AreEqual("hel\tlo", CreateJintEngine().Run("return 'hel\tlo';"));
            Assert.AreEqual("hel/lo", CreateJintEngine().Run("return 'hel/lo';"));
            Assert.AreEqual("hel//lo", CreateJintEngine().Run("return 'hel//lo';"));
            Assert.AreEqual("/*hello*/", CreateJintEngine().Run("return '/*hello*/';"));
            Assert.AreEqual("/hello/", CreateJintEngine().Run("return '/hello/';"));
        }

        [TestMethod]
        public void ShouldHandleExternalObject()
        {
            Assert.AreEqual(3d,
                CreateJintEngine()
                .SetParameter("i", 1)
                .SetParameter("j", 2)
                .Run("return i + j;"));
        }

        public bool ShouldBeCalledWithBoolean(TypeCode tc)
        {
            return tc == TypeCode.Boolean;
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleEnums()
        {
            Assert.AreEqual(TypeCode.Boolean,
                CreateJintEngine()
                .Run("System.TypeCode.Boolean"));

            Assert.AreEqual(true,
                CreateJintEngine()
                .SetParameter("clr", this)
                .Run("clr.ShouldBeCalledWithBoolean(System.TypeCode.Boolean)"));

        }

        [TestMethod]
        public void ShouldHandleNetObjects()
        {
            Assert.AreEqual("1",
                CreateJintEngine() // call Int32.ToString() 
                .SetParameter("i", 1)
                .Run("return i.ToString();"));
        }

        [TestMethod]
        public void ShouldReturnDelegateForFunctions()
        {
            const string script = "ccat=function (arg1,arg2){ return arg1+' '+arg2; }";
            JintEngine engine = CreateJintEngine().SetFunction("print", new Action<string>(Console.WriteLine));
            engine.Run(script);
            Assert.AreEqual("Nicolas Penin", engine.CallFunction("ccat", "Nicolas", "Penin"));
        }

        [TestMethod]
        public void ShouldHandleFunctions()
        {
            const string square = @"function square(x) { return x * x; } return square(2);";
            const string fibonacci = @"function fibonacci(n) { if (n == 0) return 0; else return n + fibonacci(n - 1); } return fibonacci(10); ";

            Assert.AreEqual(4d, CreateJintEngine().Run(square));
            Assert.AreEqual(55d, CreateJintEngine().Run(fibonacci));
        }

        [TestMethod]
        [Ignore]
        public void ShouldCreateExternalTypes()
        {
            const string script = @"
                var sb = new System.Text.StringBuilder();
                sb.Append('hi, mom');
                sb.Append(3);	
                sb.Append(true);
                return sb.ToString();
                ";

            Assert.AreEqual("hi, mom3True", CreateJintEngine().Run(script));
        }

        [TestMethod]
        [ExpectedException(typeof(System.Security.SecurityException))]
        public void ShouldNotAccessClr()
        {
            const string script = @"
                var sb = new System.Text.StringBuilder();
                sb.Append('hi, mom');
                sb.Append(3);	
                sb.Append(true);
                return sb.ToString();
                ";
            var engine = CreateJintEngine();
            engine.AllowClr = false;
            Assert.AreEqual("hi, mom3True", engine.Run(script));
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleStaticMethods()
        {
            const string script = @"
                var a = System.Int32.Parse('1');
                assert(1, ToDouble(a));
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldParseMultilineStrings()
        {
            const string script = @"
                assert('foobar', 'foo\
bar');
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldEvaluateConsecutiveIfStatements()
        {
            const string script = @"
                var a = 0;
                
                if(a > 0)
                    a = -1;
                else
                    a = 0;

                if(a > 1)
                    a = -1;
                else
                    a = 1;

                if(a > 2)
                    a = -1;
                else
                    a = 2;

                assert(2, a);
            ";

            Test(script);
        }

        private static JsString GiveMeJavascript(JsNumber number, JsInstance instance)
        {
            return new JsString(number + instance.ToString());
        }

        [TestMethod]
        public void ShouldNotWrapJsInstancesIfExpected()
        {
            var engine = CreateJintEngine()
            .SetFunction("evaluate", new Func<JsNumber, JsInstance, JsString>(GiveMeJavascript));

            const string script = @"
                var r = evaluate(3, [1,2]);
                return r;
            ";

            var r = engine.Run(script, false);

            Assert.IsTrue(r is JsString);
            Assert.AreEqual("31,2", r.ToString());
        }

        [TestMethod]
        public void ShouldAssignBooleanValue()
        {
            const string script = @"
                function check(x) {
                    assert(false, x);    
                }

                var a = false;
                check(a);                
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldEvaluateFunctionDeclarationsFirst()
        {
            const string script = @"
                var a = false;
                assert(false, a);
                test();
                assert(true, a);
                
                function test() {
                    a = true;
                }
            ";

            Test(script);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Security.SecurityException))]
        [Ignore]
        public void ShouldRunInLowTrustMode()
        {
            const string script = @"
                var a = System.Convert.ToInt32(1);
                var b = System.IO.Directory.GetFiles('c:');
            ";

            CreateJintEngine()
                .Run(script);
        }

        [TestMethod]
        [Ignore]
        public void ShouldAllowSecuritySandBox()
        {
            var userDirectory = Path.GetTempPath();

            const string script = @"
                var b = System.IO.Directory.GetFiles(userDir);
            ";

            CreateJintEngine()
                .SetParameter("userDir", userDirectory)
                .AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery, userDirectory))
                .Run(script);
        }


        [TestMethod]
        [Ignore]
        public void ShouldSetClrProperties()
        {
            // Ensure assembly is loaded
            var a = typeof(StringBuilder);
            var b = a.Assembly; // Force loading in Release mode, otherwise code is optimized
            const string script = @"
                var s = new System.Text.StringBuilder();
                s.Append('Test');
                return frm.ToString(); 
            ";

            var result = CreateJintEngine()
                .AddPermission(new UIPermission(PermissionState.Unrestricted))
                .Run(script);

            Assert.AreEqual("Test", result.ToString());
        }

        [TestMethod]
        public void ShouldHandleCustomMethods()
        {
            Assert.AreEqual(9d, CreateJintEngine()
                .SetFunction("square", new Func<double, double>(a => a * a))
                .Run("return square(3);"));

            CreateJintEngine()
                .SetFunction("print", new Action<string>(Console.Write))
                .Run("print('hello');");

            const string script = @"
                function square(x) { 
                    return multiply(x, x); 
                }; 

                return square(4);
            ";

            var result =
                CreateJintEngine()
                .SetFunction("multiply", new Func<double, double, double>((x, y) => x * y))
                .Run(script);

            Assert.AreEqual(16d, result);
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleDirectNewInvocation()
        {
            Assert.AreEqual("c", CreateJintEngine()
                .Run("return new System.Text.StringBuilder('c').ToString();"));
        }

        [TestMethod]
        public void ShouldHandleGlobalVariables()
        {
            const string program = @"
                var i = 3;
                function calculate() {
                    return i*i;
                }
                return calculate();
            ";

            Assert.AreEqual(9d, CreateJintEngine()
                .Run(program));
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleObjectClass()
        {
            const string program = @"
                var userObject = new Object();
                userObject.lastLoginTime = new Date();
                return userObject.lastLoginTime;
            ";

            object result = CreateJintEngine().Run(program);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleIndexedProperties()
        {
            const string program = @"
                var userObject = { };
                userObject['lastLoginTime'] = new Date();
                return userObject.lastLoginTime;
            ";

            object result = CreateJintEngine().Run(program);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void ShouldAssignProperties()
        {
            const string script = @"
                function sayHi(x) {
                    alert('Hi, ' + x + '!');
                }

                sayHi.text = 'Hello World!';
                sayHi['text2'] = 'Hello World... again.';

                assert('Hello World!', sayHi['text']); 
                assert('Hello World... again.', sayHi.text2); 
                ";

            Test(script);
        }

        [TestMethod]
        public void ShouldStoreFunctionsInArray()
        {
            const string script = @"

                // functions stored as array elements
                var arr = [];
                arr[0] = function(x) { return x * x; };
                arr[1] = arr[0](2);
                arr[2] = arr[0](arr[1]);
                arr[3] = arr[0](arr[2]);
                
                // displays 256
                assert(256, arr[3]);
            ";

            Test(script);
        }

        [TestMethod]
        [Ignore]
        public void ShouldNotConflictWithClrMethods()
        {
            const string script = @"
                assert(true, System.Math.Max(1, 2) == 2);
                assert(true, System.Math.Min(1, 2) == 1);
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldCreateObjectLiterals()
        {
            const string script = @"
                var myDog = {
                    'name' : 'Spot',
                    'bark' : function() { return 'Woof!'; },
                    'displayFullName' : function() {
                        return this.name + ' The Alpha Dog';
                    },
                    'chaseMrPostman' : function() { 
                        // implementation beyond the scope of this article 
                    }    
                };
                assert('Spot The Alpha Dog', myDog.displayFullName()); 
                assert('Woof!', myDog.bark()); // Woof!
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldHandleFunctionsAsObjects()
        {
            const string script = @"
                // assign an anonymous function to a variable
                var greet = function(x) {
                    return 'Hello, ' + x;
                };

                assert('Hello, MSDN readers', greet('MSDN readers'));

                // passing a function as an argument to another
                function square(x) {
                    return x * x;
                }
                function operateOn(num, func) {
                    return func(num);
                }
                // displays 256
                assert(256, operateOn(16, square));

                // functions as return values
                function makeIncrementer() {
                    return function(x) { return x + 1; };
                }
                var inc = makeIncrementer();
                // displays 8
                assert(8, inc(7));
                ";

            Test(script);

            Test(@"var Test = {};
Test.FakeButton = function() { };
Test.FakeButton.prototype = {};
var fakeButton = new Test.FakeButton();");
        }

        [TestMethod]
        public void ShouldOverrideDefaultFunction()
        {
            const string script = @"

                // functions as object properties
                var obj = { 'toString' : function() { return 'This is an object.'; } };
                // calls obj.toString()
                assert('This is an object.', obj.toString());
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldHandleFunctionConstructor()
        {
            const string script = @"
                var func = new Function('x', 'return x * x;');
                var r = func(3);
                assert(9, r);
            ";

            Test(script);
        }

        [TestMethod]
        public void ShouldContinueAfterFunctionCall()
        {
            const string script = @"
                function fib(x) {
                    if (x==0) return 0;
                    if (x==1) return 1;
                    if (x==2) return 2;
                    return fib(x-1) + fib(x-2);
                }

                var x = fib(0);
                
                return 'beacon';
                ";

            Assert.AreEqual("beacon", Test(script).ToString());
        }

        [TestMethod]
        public void ShouldRetainGlobalsThroughRuns()
        {
            var jint = CreateJintEngine();

            jint.Run("i = 3; function square(x) { return x*x; }");

            Assert.AreEqual(3d, jint.Run("return i;"));
            Assert.AreEqual(9d, jint.Run("return square(i);"));
        }

        [TestMethod]
        public void ShouldDebugScripts()
        {
            var jint = CreateJintEngine()
            .SetDebugMode(true);
            jint.BreakPoints.Add(new BreakPoint(4, 22)); // return x*x;

            jint.Step += (sender, info) => Assert.IsNotNull(info.CurrentStatement);

            bool brokeOnReturn = false;

            jint.Break += (sender, info) =>
            {
                Assert.IsNotNull(info.CurrentStatement);
                Assert.IsTrue(info.CurrentStatement is ReturnStatement);
                Assert.AreEqual(3, Convert.ToInt32(info.Locals["x"].Value));

                brokeOnReturn = true;
            };

            jint.Run(@"
                var i = 3; 
                function square(x) { 
                    return x*x; 
                }

                return square(i);
            ");

            Assert.IsTrue(brokeOnReturn);

        }

        [TestMethod]
        [Ignore]
        public void ShouldBreakInLoops()
        {
            var jint = CreateJintEngine()
                .SetDebugMode(true);
            jint.BreakPoints.Add(new BreakPoint(4, 22)); // x += 1;

            jint.Step += (sender, info) => Assert.IsNotNull(info.CurrentStatement);

            bool brokeInLoop = false;

            jint.Break += (sender, info) =>
            {
                Assert.IsNotNull(info.CurrentStatement);
                Assert.IsTrue(info.CurrentStatement is ExpressionStatement);
                Assert.AreEqual(7, Convert.ToInt32(info.Locals["x"].Value));

                brokeInLoop = true;
            };

            jint.Run(@"
                var x = 7;
                for(i=0; i<3; i++) { 
                    x += i; 
                    return;
                }
            ");

            Assert.IsTrue(brokeInLoop);
        }

        [TestMethod]
        [Ignore]
        public void ShouldBreakOnCondition()
        {
            JintEngine jint = CreateJintEngine()
            .SetDebugMode(true);
            jint.BreakPoints.Add(new BreakPoint(4, 22, "x == 2;")); // return x*x;

            jint.Step += (sender, info) => Assert.IsNotNull(info.CurrentStatement);

            bool brokeOnReturn = false;

            jint.Break += (sender, info) =>
            {
                Assert.IsNotNull(info.CurrentStatement);
                Assert.IsTrue(info.CurrentStatement is ReturnStatement);
                Assert.AreEqual(2, Convert.ToInt32(info.Locals["x"].Value));

                brokeOnReturn = true;
            };

            jint.Run(@"
                var i = 3; 
                function square(x) { 
                    return x*x; 
                }
                
                square(1);
                square(2);
                square(3);
            ");

            Assert.IsTrue(brokeOnReturn);
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleInlineCLRMethodCalls()
        {
            string script = @"
                var box = new BitMobile.ScriptEngine.UnitTests.Box();
                box.SetSize(ToInt32(100), ToInt32(100));
                assert(100, Number(box.Width));
                assert(100, Number(box.Height));
            ";
            Test(script);
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleStructs()
        {
            const string script = @"
                var size = new BitMobile.ScriptEngine.UnitTests.Size();
                size.Width = 10;
                assert(10, Number(size.Width));
                assert(0, Number(size.Height));
            ";
            Test(script);
        }

        [TestMethod]
        public void ShouldHandleFunctionScopes()
        {
            const string script = @"
                var success = false;
                $ = {};

                (function () { 
                    
                    function a(x) {
                        success = x;                                   
                    }
                    
                    $.b = function () {
                        a(true);
                    }

                }());
                
                $.b();

                ";

            Test(script);
        }

        [TestMethod]
        public void ShouldHandleLoopScopes()
        {
            const string script = @"
                f = function() { var i = 10; }
                for(var i=0; i<3; i++) { f(); }
                assert(3, i);

                f = function() { i = 10; }
                for(i=0; i<3; i++) { f(); }
                assert(11, i);

                f = function() { var i = 10; }
                for(i=0; i<3; i++) { f(); }
                assert(3, i);

                f = function() { i = 10; }
                for(var i=0; i<3; i++) { f(); }
                assert(11, i);
                ";

            Test(script);
        }

        [TestMethod]
        [Ignore]
        public void ShouldExecuteSingleScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var program = new StreamReader(assembly.GetManifestResourceStream("BitMobile.ScriptEngine.UnitTests.Scripts.Date.js")).ReadToEnd();
            Test(program);
        }

        [TestMethod]
        public void ShouldCascadeEquals()
        {
            Test("a=b=1; assert(1,a);assert(1,b);");
        }

        [TestMethod]
        public void ShouldParseScripts()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resx in assembly.GetManifestResourceNames())
            {
                // Ignore scripts not in /Scripts
                if (!resx.Contains(".Parse"))
                {
                    continue;
                }

                var program = new StreamReader(assembly.GetManifestResourceStream(resx)).ReadToEnd();
                if (program.Trim() == String.Empty)
                {
                    continue;
                }
                Trace.WriteLine(Path.GetFileNameWithoutExtension(resx));
                JintEngine.Compile(program, true);
            }
        }

        [TestMethod]
        public void ShouldHandleNativeTypes()
        {

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
            .SetFunction("print", new Action<string>(System.Console.WriteLine))
            .SetParameter("foo", "native string");

            jint.Run(@"
                assert(7, foo.indexOf('string'));            
            ");
        }

        [TestMethod]
        [Ignore]
        public void ClrNullShouldBeConverted()
        {

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
            .SetFunction("print", new Action<string>(System.Console.WriteLine))
            .SetParameter("foo", null);

            // strict equlity ecma 262.3 11.9.6 x === y: If type of (x) is null return true.
            jint.Run(@"
                assert(true, foo == null);
                assert(true, foo === null);
            ");
        }

        public void RunMozillaTests(string folder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var shell = new StreamReader(assembly.GetManifestResourceStream("BitMobile.ScriptEngine.UnitTests.shell.js")).ReadToEnd();
            var extensions = new StreamReader(assembly.GetManifestResourceStream("BitMobile.ScriptEngine.UnitTests.extensions.js")).ReadToEnd();

            var resources = new List<string>();
            foreach (var resx in assembly.GetManifestResourceNames())
            {
                // Ignore scripts not in /Scripts
                if (!resx.Contains(".ecma_3.") || !resx.Contains(folder))
                {
                    continue;
                }

                resources.Add(resx);
            }

            resources.Sort();

            //Run the shell first if defined
            string additionalShell = null;
            if (resources[resources.Count - 1].EndsWith("shell.js"))
            {
                additionalShell = resources[resources.Count - 1];
                resources.RemoveAt(resources.Count - 1);
                additionalShell = new StreamReader(assembly.GetManifestResourceStream(additionalShell)).ReadToEnd();
            }

            foreach (var resx in resources)
            {
                var program = new StreamReader(assembly.GetManifestResourceStream(resx)).ReadToEnd();
                Console.WriteLine(Path.GetFileNameWithoutExtension(resx));

                StringBuilder output = new StringBuilder();
                StringWriter sw = new StringWriter(output);

                var jint = CreateJintEngine()
                .SetDebugMode(true)
                .SetFunction("print", new Action<string>(sw.WriteLine));

                jint.Run(extensions);
                jint.Run(shell);
                jint.Run("test = _test;");
                if (additionalShell != null)
                {
                    jint.Run(additionalShell);
                }

                try
                {
                    jint.Run(program);
                    string result = sw.ToString();
                    if (result.Contains("FAILED"))
                    {
                        Assert.Fail(result);
                    }
                }
                catch (Exception e)
                {
                    jint.Run("print('Error in : ' + gTestfile)");
                    Assert.Fail(e.Message);
                }
            }
        }

        [TestMethod]
        [Ignore]// уже был заигнорен
        public void ShouldExecuteEcmascript5TestsScripts()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var extensions = new StreamReader(assembly.GetManifestResourceStream("BitMobile.ScriptEngine.UnitTests.extensions.js")).ReadToEnd();

            var resources = new List<string>();
            foreach (var resx in assembly.GetManifestResourceNames())
            {
                // Ignore scripts not in /Scripts
                if (!resx.Contains(".ecma_5.") || resx.Contains(".Scripts."))
                {
                    continue;
                }

                resources.Add(resx);
            }

            resources.Sort();

            //Run the shell first if defined
            string additionalShell = null;
            if (resources[resources.Count - 1].EndsWith("shell.js"))
            {
                additionalShell = resources[resources.Count - 1];
                resources.RemoveAt(resources.Count - 1);
                additionalShell = new StreamReader(assembly.GetManifestResourceStream(additionalShell)).ReadToEnd();
            }

            foreach (var resx in resources)
            {
                var program = new StreamReader(assembly.GetManifestResourceStream(resx)).ReadToEnd();
                Console.WriteLine(Path.GetFileNameWithoutExtension(resx));

                var jint = CreateJintEngine()
                .SetDebugMode(true)
                .SetFunction("print", new Action<string>(System.Console.WriteLine));

                jint.Run(extensions);
                //jint.Run(shell);
                jint.Run("test = _test;");
                if (additionalShell != null)
                {
                    jint.Run(additionalShell);
                }

                try
                {
                    jint.Run(program);
                }
                catch (Exception e)
                {
                    jint.Run("print('Error in : ' + gTestfile)");
                    Console.WriteLine(e.Message);
                }
            }
        }

        public List<int> FindAll(List<int> source, Predicate<int> predicate)
        {
            var result = new List<int>();

            foreach (var i in source)
            {
                var obj = predicate(i);

                if (obj)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        [TestMethod]
        public void ShouldHandleStrictMode()
        {
            //Strict mode enabled
            var engine = CreateJintEngine(Options.Strict)
            .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
            ;
            engine.Run(@"
            try{
                var test1=function(eval){}
                //should not execute the next statement
                assert(true, false);
            }
            catch(e){
                assert(true, true);
            }
            try{
                function test2(eval){}
                //should not execute the next statement
                assert(true, false);
            }
            catch(e){
                assert(true, true);
            }");

            //Strict mode disabled
            engine = CreateJintEngine(Options.Ecmascript3)
            .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
            ;
            engine.Run(@"
            try{
                var test1=function(eval){}
                assert(true, true);
            }
            catch(e){
                assert(true, false);
            }
            try{
                function test2(eval){}
                assert(true, true);
            }
            catch(e){
                assert(true, false);
            }");
        }

        [TestMethod]
        public void ShouldHandleMultipleRunsInSameScope()
        {
            var jint = CreateJintEngine()
                .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
                .SetFunction("print", new Action<string>(System.Console.WriteLine));

            jint.Run(@" var g = []; function foo() { assert(0, g.length); }");
            jint.Run(@" foo();");
        }

        [TestMethod]
        public void ShouldHandleClrArrays()
        {
            var values = new int[] { 2, 3, 4, 5, 6, 7 };
            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetParameter("a", values);

            Assert.AreEqual(3, jint.Run("a[1];"));
            jint.Run("a[1] = 4");
            Assert.AreEqual(4, jint.Run("a[1];"));
            Assert.AreEqual(4, values[1]);

        }

        [TestMethod]
        public void ShouldHandleClrDictionaries()
        {
            var dic = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetParameter("dic", dic);

            Assert.AreEqual(1, jint.Run("return dic['a'];"));
            jint.Run("dic['a'] = 4");
            Assert.AreEqual(4, jint.Run("return dic['a'];"));
            Assert.AreEqual(4, dic["a"]);
        }

        [TestMethod]
        public void ShouldEvaluateIndexersAsClrProperties()
        {
            var box = new Box { Width = 10, Height = 20 };

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetParameter("box", box);

            Assert.AreEqual(10, jint.Run("return box.Width"));
            Assert.AreEqual(10, jint.Run("return box['Width']"));
            jint.Run("box['Height'] = 30;");

            Assert.AreEqual(30, box.Height);

            jint.Run("box.Height = 18;");
            Assert.AreEqual(18, box.Height);
        }

        [TestMethod]
        public void ShouldEvaluateIndexersAsClrFields()
        {
            var box = new Box { width = 10, height = 20 };

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetParameter("box", box);

            Assert.AreEqual(10, jint.Run("return box.width"));
            Assert.AreEqual(10, jint.Run("return box['width']"));
            jint.Run("box['height'] = 30;");

            Assert.AreEqual(30, box.height);

            jint.Run("box.height = 18;");

            Assert.AreEqual(18, box.height);

        }

        [TestMethod]
        public void ShouldFindOverloadWithNullParam()
        {
            var box = new Box { Width = 10, Height = 20 };

            var jint = CreateJintEngine()
            .SetDebugMode(true)
            .SetFunction("assert", new Action<object, object>(Assert.AreEqual))
            .SetParameter("box", box);

            jint.Run(@"
                assert(1, Number(box.Foo(1)));
                assert(2, Number(box.Foo(2, null)));    
            ");
        }

        [TestMethod]
        public void ShouldHandlePropertiesOnFunctions()
        {
            Test(@"
                HelloWorld.webCallable = 'GET';
                function HelloWorld()
                {
                    return 'Hello from Javascript!';
                }
                
                assert('GET', HelloWorld.webCallable);
            ");

        }

        [TestMethod]
        [Ignore]
        public void ShouldCatchNotDefinedVariable()
        {
            Test(@"
                try {
                    a = b;
                    assert(true, false);
                } 
                catch(e) {
                }

                assert('undefined', typeof foo);
                
                try {
                    var y;
                    assert(false, y instanceof Foo);
                    assert(true, false);
                } 
                catch(e) {
                }                
            ");
        }

        [TestMethod]
        [Ignore]
        public void ShouldNotThrowOverflowExpcetion()
        {
            var jint = CreateJintEngine();
            jint.SetParameter("box", new Box());
            jint.Run("box.Write(new Date);");

        }

        [TestMethod]
        public void ShouldNotReproduceBug85418()
        {
            var engine = CreateJintEngine();
            engine.SetParameter("a", 4);
            Assert.AreEqual(4, engine.Run("a"));
            Assert.AreEqual(4d, engine.Run("4"));
            Assert.AreEqual(true, engine.Run("a == 4"));
            Assert.AreEqual(true, engine.Run("4 == 4"));
            Assert.AreEqual(true, engine.Run("a == a"));
        }

        [TestMethod]
        public void ShouldShortCircuitBooleanOperators()
        {
            Test(@"
                var called = false;
                function dontcallme() {
                    called = true;
                }
                
                assert(true, true || dontcallme());
                assert(false, called);

                assert(false, false && dontcallme());
                assert(false, called);

                ");
        }

        [TestMethod]
        public void UndefinedEqualsToNullShouldBeTrue()
        {
            Test(@"
                assert(true, undefined == null);
                assert(false, undefined === null);
                ");
        }

        [TestMethod]
        public void NumbersShouldEqualTheirStrings()
        {
            Test(@"
                assert(true, 5 == '5');
                assert(true, 5.1 == '5.1');
                assert(false, 5 === '5');
                ");
        }

        [TestMethod]
        [Ignore]
        public void AccessorsScriptShouldPassTests()
        {
            ExecuteEmbededScript("Accessors.js");
        }

        [TestMethod]
        public void ArgumentsScriptShouldPassTests()
        {
            ExecuteEmbededScript("Arguments.js");
        }

        [TestMethod]
        public void ArraysScriptShouldPassTests()
        {
            ExecuteEmbededScript("Arrays.js");
        }


        [TestMethod]
        public void BlocksScriptShouldPassTests()
        {
            ExecuteEmbededScript("Blocks.js");
        }

        [TestMethod]
        [Ignore]
        public void BooleanScriptShouldPassTests()
        {
            ExecuteEmbededScript("Boolean.js");
        }

        [TestMethod]
        public void ChainConstructorsScriptShouldPassTests()
        {
            ExecuteEmbededScript("ChainConstructors.js");
        }

        [TestMethod]
        public void ClosuresScriptShouldPassTests()
        {
            ExecuteEmbededScript("Closures.js");
        }

        [TestMethod]
        [Ignore]
        public void ClrScriptShouldPassTests()
        {
            ExecuteEmbededScript("Clr.js");
        }

        [TestMethod]
        public void CommentsScriptShouldPassTests()
        {
            ExecuteEmbededScript("Comments.js");
        }

        [TestMethod]
        [Ignore]
        public void DateScriptShouldPassTests()
        {
            ExecuteEmbededScript("Date.js");
        }

        [TestMethod]
        [Ignore]
        public void FunctionScriptShouldPassTests()
        {
            ExecuteEmbededScript("Function.js");
        }

        [TestMethod]
        public void FunctionAsConstrutorScriptShouldPassTests()
        {
            ExecuteEmbededScript("FunctionAsConstructor.js");
        }

        [TestMethod]
        [Ignore]
        public void GlobalScriptShouldPassTests()
        {
            ExecuteEmbededScript("Global.js");
        }

        [TestMethod]
        [Ignore]
        public void InOperatorScriptShouldPassTests()
        {
            ExecuteEmbededScript("InOperator.js");
        }

        [TestMethod]
        [Ignore]
        public void JsonScriptShouldPassTests()
        {
            ExecuteEmbededScript("Json.js");
        }

        [TestMethod]
        public void LoopsScriptShouldPassTests()
        {
            ExecuteEmbededScript("Loops.js");
        }

        [TestMethod]
        public void MathScriptShouldPassTests()
        {
            ExecuteEmbededScript("Math.js");
        }

        [TestMethod]
        public void NumberScriptShouldPassTests()
        {
            ExecuteEmbededScript("Number.js");
        }

        [TestMethod]
        public void ObjectScriptShouldPassTests()
        {
            ExecuteEmbededScript("Object.js");
        }

        [TestMethod]
        public void OperatorsScriptShouldPassTests()
        {
            ExecuteEmbededScript("Operators.js");
        }

        [TestMethod]
        public void PrivateMembersScriptShouldPassTests()
        {
            ExecuteEmbededScript("PrivateMembers.js");
        }

        [TestMethod]
        public void PrototypeInheritanceScriptShouldPassTests()
        {
            ExecuteEmbededScript("PrototypeInheritance.js");
        }

        [TestMethod]
        [Ignore]
        public void RegExpScriptShouldPassTests()
        {
            ExecuteEmbededScript("RegExp.js");
        }

        [TestMethod]
        public void SimpleClassScriptShouldPassTests()
        {
            ExecuteEmbededScript("SimpleClass.js");
        }

        [TestMethod]
        [Ignore]
        public void StaticMethodsScriptShouldPassTests()
        {
            ExecuteEmbededScript("StaticMethods.js");
        }

        [TestMethod]
        [Ignore]
        public void StringScriptShouldPassTests()
        {
            ExecuteEmbededScript("String.js");
        }

        [TestMethod]
        public void TernaryScriptShouldPassTests()
        {
            ExecuteEmbededScript("Ternary.js");
        }

        [TestMethod]
        public void ThisInDifferentScopesScriptShouldPassTests()
        {
            ExecuteEmbededScript("ThisInDifferentScopes.js");
        }

        [TestMethod]
        [Ignore]
        public void TryCatchScriptShouldPassTests()
        {
            ExecuteEmbededScript("TryCatch.js");
        }

        [TestMethod]
        [Ignore]
        public void TypeofScriptShouldPassTests()
        {
            ExecuteEmbededScript("typeof.js");
        }

        [TestMethod]
        public void WithScriptShouldPassTests()
        {
            ExecuteEmbededScript("With.js");
        }

        [TestMethod]
        [Ignore]
        public void InstanceOfScriptShouldPassTests()
        {
            ExecuteEmbededScript("instanceOf.js");
        }

        [TestMethod]
        public void RandomValuesShouldNotRepeat()
        {
            Test(@"
                for(var i=0; i<100; i++){
                    assert(false, Math.random() == Math.random());
                }
            ");
        }

        [TestMethod]
        [Ignore]// уже был заигнорен
        public void MaxRecursionsShouldBeDetected()
        {
            Test(@"
                function doSomething(){
                    doSomethingElse();
                }

                function doSomethingElse(){
                    doSomething();
                }

                try {
                    doSomething();
                    assert(true, false);
                }
                catch (e){
                    return;                
                }
                ");
        }

        [TestMethod]
        public void ObjectShouldBePassedToDelegates()
        {
            var engine = CreateJintEngine();
            engine.SetFunction("render", new Action<object>(s => Console.WriteLine(s)));

            const string script =
                @"
                var contact = {
                    'Name': 'John Doe',
                    'PhoneNumbers': [ 
                    {
                       'Location': 'Home',
                       'Number': '555-555-1234'
                    },
                    {
                        'Location': 'Work',
                        'Number': '555-555-9999 Ext. 123'
                    }
                    ]
                };

                render(contact.Name);
                render(contact.toString());
                render(contact);
            ";

            engine.Run(script);
        }

        [TestMethod]
        [Ignore]
        public void IndexerShouldBeEvaluatedBeforeUsed()
        {
            Test(@"
                var cat = {
                    name : 'mega cat',
                    prop: 'name',
                    hates: 'dog'
                };

                var prop = 'hates';
                assert('dog', cat[prop]);

                ");
        }

        [TestMethod]
        [Ignore]
        public void ShouldParseCoffeeScript()
        {
            Test(@"
                xhr = new (String || Number)('123');
                var type = String || Number;
                var x = new type('123');
                assert('123', x);
            ");
        }

        [TestMethod]
        [Ignore]
        public void StaticMemberAfterUndefinedReference()
        {
            var engine = CreateJintEngine();

            Assert.AreEqual(System.String.Format("{0}", 1), engine.Run("System.String.Format('{0}', 1)"));
            Assert.AreEqual("undefined", engine.Run("typeof thisIsNotDefined"));
            Assert.AreEqual(System.String.Format("{0}", 1), engine.Run("System.String.Format('{0}', 1)"));
        }

        [TestMethod]
        [Ignore]
        public void MozillaNumber()
        {
            RunMozillaTests("Number");
        }

        [TestMethod]
        public void CheckingErrorsShouldNotThrow()
        {
            string errors;
            Assert.IsTrue(JintEngine.HasErrors("var s = @string?;", out errors));
        }

        [TestMethod]
        [Ignore]
        public void ShouldHandleBadEnums()
        {
            Test(@"
                assert('Name', BitMobile.ScriptEngine.UnitTests.FooEnum.Name.toString());
                assert('GetType', BitMobile.ScriptEngine.UnitTests.FooEnum.GetType.toString());
                assert('IsEnum', BitMobile.ScriptEngine.UnitTests.FooEnum.IsEnum.toString());
                assert('System', BitMobile.ScriptEngine.UnitTests.FooEnum.System.toString());

                // still can access hidden Type properties
                assert('FooEnum',BitMobile.ScriptEngine.UnitTests.FooEnum.get_Name());
            ");
        }

        [TestMethod]
        [ExpectedException(typeof(JintException))]
        [Ignore]
        public void RunningInvalidScriptSourceShouldThrow()
        {
            CreateJintEngine().Run("var s = @string?;");
        }

        public static JintEngine CreateJintEngine()
        {
            return new Script.ScriptEngine("Test", null);
        }

        public static JintEngine CreateJintEngine(Options options)
        {
            return new Script.ScriptEngine("Test", null, options);
        }
    }

    public struct Size
    {
        public int Width;
        public int Height;
    }

    public enum FooEnum
    {
        Name = 1,
        GetType = 2,
        IsEnum = 3,
        System = 4
    }

    public class Box
    {
        // public fields
        public int width;
        public int height;

        // public properties
        public int Width { get; set; }
        public int Height { get; set; }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Foo(int a, object b)
        {
            return a;
        }

        public int Foo(int a)
        {
            return a;
        }

        public void Write(object value)
        {
            Console.WriteLine(value);
        }
    }
}