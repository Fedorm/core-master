﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;


namespace BitMobile.ValueStack
{
    #region " Operators "

    internal interface IOperator
    {
        string value { get; set; }
        int precedence { get; set; }
        int arguments { get; set; }
        bool leftassoc { get; set; }
    }

    internal abstract class Operator<T> : IOperator
    {
        public T func;

        public string value { get; set; }
        public int precedence { get; set; }
        public int arguments { get; set; }
        public bool leftassoc { get; set; }

        public Operator(string value, int precedence, bool leftassoc, T func)
        {
            this.value = value;
            this.precedence = precedence;
            this.leftassoc = leftassoc;
            this.func = func;
        }

    }

    internal class MethodOperator : Operator<Func<Expression, string, List<Expression>, Dictionary<string, object>, Expression>>
    {
        public MethodOperator(string value, int precedence, bool leftassoc, Func<Expression, string, List<Expression>, Dictionary<string, object>, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
        }
    }

    internal class BinaryOperator : Operator<Func<Expression, Expression, Expression>>
    {
        public BinaryOperator(string value, int precedence, bool leftassoc, Func<Expression, Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 2;
        }
    }

    internal class UnaryOperator : Operator<Func<Expression, UnaryExpression>>
    {
        public UnaryOperator(string value, int precedence, bool leftassoc, Func<Expression, UnaryExpression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 1;
        }
    }

    internal class TypeOperator : Operator<Func<Expression, Type, UnaryExpression>>
    {
        public TypeOperator(string value, int precedence, bool leftassoc, Func<Expression, Type, UnaryExpression> func)
            : base(value, precedence, leftassoc, func)
        {
            arguments = 1;
        }
    }

    #endregion

    #region " Custom Operator Expressions "

    internal class OperatorCustomExpressions
    {
        /// <summary>
        /// Returns an Expression that accesses a member on an Expression
        /// </summary>
        /// <param name="le">The expression that contains the member to be accessed</param>
        /// <param name="membername">The name of the member to access</param>
        /// <param name="args">Optional list of arguments to be passed if the member is a method</param>
        /// <returns></returns>
        public static Expression MemberAccess(Expression le, string membername, List<Expression> args, Dictionary<string, object> stack)
        {
            List<Type> argTypes = new List<Type>();
            args.ForEach(x => argTypes.Add(x.Type));

            Expression instance = null;
            Type type = null;
            if (le.Type.Name == "RuntimeType")
            {
                type = ((Type)((ConstantExpression)le).Value);
            }
            else
            {
                type = le.Type;
                instance = le;
            }

            MethodInfo mi = typeof(ExpressionHelper).GetMethod(membername, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (mi != null)
            {
                ParameterInfo[] pi = mi.GetParameters();
                for (int i = 1; i < pi.Length; i++)
                {
                    args[i - 1] = TypeConversion.Convert(args[i - 1], pi[i].ParameterType);
                }
                args.Insert(0, instance);
                return Expression.Call(null, mi, args);
            }
            else
            {
                mi = type.GetMethod(membername, argTypes.ToArray());
                if (mi != null)
                {
                    ParameterInfo[] pi = mi.GetParameters();
                    for (int i = 0; i < pi.Length; i++)
                    {
                        args[i] = TypeConversion.Convert(args[i], pi[i].ParameterType);
                    }
                    return Expression.Call(instance, mi, args);
                }
                else
                {
                    Expression exp = null;

                    PropertyInfo pi = type.GetProperty(membername);
                    if (pi != null)
                    {
                        var getter = pi.GetGetMethod(true);
                        if (getter != null)
                            instance = getter.IsStatic ? null : instance;
                        exp = Expression.Property(instance, pi);
                    }
                    else
                    {
                        FieldInfo fi = type.GetField(membername);
                        if (fi != null)
                        {
                            exp = Expression.Field(instance, fi);
                        }
                        else
                        {
                            //value from Dictionary ?
                            if (type.FullName.Equals("BitMobile.ValueStack.CustomDictionary"))//StartsWith("System.Collections.Generic.Dictionary"))
                            {
                                CustomDictionary d = (CustomDictionary)((ConstantExpression)le).Value;
                                object v = d.GetValue(membername);
                                if (!d.ContainsKey(membername))
                                {
                                    //return Expression.Constant(null);
                                    throw new NotInDictionaryExeception(Expression.Constant(null));
                                }
                                else
                                {
                                    MethodInfo indexAccessor = type.GetMethod("GetValue");
                                    return Expression.Convert(
                                        Expression.Call(instance, indexAccessor, new Expression[] { Expression.Constant(membername) }), v.GetType());
                                }

                                /*
                                System.Reflection.MethodInfo indexAccessor = type.GetMethod("GetValue");
                                if (indexAccessor != null)
                                {
                                    System.Reflection.MethodInfo miExists = type.GetMethod("ContainsKey");

                                    return Expression.Condition(
                                        Expression.Call(instance, miExists, new Expression[] { Expression.Constant(membername) }),
                                        Expression.Call(instance, indexAccessor, new Expression[] { Expression.Constant(membername) }), 
                                        Expression.Constant(null));

                                    return Expression.Call(instance, indexAccessor, new Expression[] { Expression.Constant(membername) });
                                }
                                */

                            }
                            else
                            {
                                if (stack != null)
                                {
                                    mi = type.GetMethod(membername, new Type[] { stack["dao"].GetType() });
                                    if (mi != null)
                                        return Expression.Call(instance, mi, new Expression[] { Expression.Constant(stack["dao"]) });
                                }

                            }
                        }
                    }

                    if (exp != null)
                    {
                        if (args.Count > 0)
                        {
                            //return Expression.ArrayAccess(exp, args);
                            throw new NotSupportedException();
                        }
                        else
                        {
                            return exp;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Member not found: {0}.{1}", le.Type.Name, membername));
                    }
                }
            }
        }

        /// <summary>
        /// Extends the Add Expression handler to handle string concatenation
        /// </summary>
        /// <param name="le"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static Expression Add(Expression le, Expression re)
        {
            if (le.Type == typeof(string) && re.Type == typeof(string))
            {
                return Expression.Add(le, re, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
            }
            else
            {
                return Expression.Add(le, re);
            }
        }

        /// <summary>
        /// Returns an Expression that access a 1-dimensional index on an Array expression 
        /// </summary>
        /// <param name="le"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static Expression ArrayAccess(Expression le, Expression re)
        {
            throw new NotSupportedException();
            /*
            var indexes = new List<Expression>();
            indexes.Add(re);
            return Expression.ArrayAccess(le, indexes);
            */
        }

    }

    #endregion

    #region " Operator Collection "

    internal class OperatorCollection : Dictionary<string, IOperator>
    {
        List<char> firstlookup = new List<char>();

        public new void Add(string key, IOperator op)
        {
            firstlookup.Add(key[0]);
            base.Add(key, op);
        }

        public bool ContainsFirstKey(char key)
        {
            return firstlookup.Contains(key);
        }
    }

    #endregion

    #region " Operator Function Service Locator "

    internal delegate Expression OpFuncDelegate(
        OpFuncArgs args
    );

    internal class OpFuncServiceLocator
    {
        static OpFuncServiceLocator instance = new OpFuncServiceLocator();

        OpFuncServiceLocator()
        {
            typeActions.Add(typeof(MethodOperator), OpFuncServiceProviders.MethodOperatorFunc);
            typeActions.Add(typeof(TypeOperator), OpFuncServiceProviders.TypeOperatorFunc);
            typeActions.Add(typeof(UnaryOperator), OpFuncServiceProviders.UnaryOperatorFunc);
            typeActions.Add(typeof(BinaryOperator), OpFuncServiceProviders.BinaryOperatorFunc);
        }

        public static OpFuncDelegate Resolve(Type type)
        {
            return instance.ResolveType(type);
        }

        OpFuncDelegate ResolveType(Type type)
        {
            return typeActions[type];
        }

        Dictionary<Type, OpFuncDelegate>
            typeActions = new Dictionary<Type, OpFuncDelegate>();
    }

    #endregion

    #region " Operator Function Service Providers "

    internal class OpFuncArgs
    {
        public Queue<Token> tempQueue;
        public Stack<Expression> exprStack;
        //public Stack<String> literalStack;
        public Token t;
        public IOperator op;
        public List<Expression> args;
        public Dictionary<String, object> stack = null;
    }

    internal class OpFuncServiceProviders
    {
        public static Expression MethodOperatorFunc(
            OpFuncArgs args
          )
        {
            //var nextToken = args.literalStack.Pop();
            var nextToken = ((MemberToken)args.t).name;
            Expression le = args.exprStack.Pop();

            Expression result = ((MethodOperator)args.op).func(le, nextToken, args.args, args.stack);

            return result;
        }

        public static Expression TypeOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.exprStack.Pop();
            return ((TypeOperator)args.op).func(le, args.t.type);
        }

        public static Expression UnaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression le = args.exprStack.Pop();
            return ((UnaryOperator)args.op).func(le);
        }

        public static Expression BinaryOperatorFunc(
            OpFuncArgs args
            )
        {
            Expression re = args.exprStack.Pop();
            Expression le = args.exprStack.Pop();
            // perform implicit conversion on known types
            TypeConversion.Convert(ref le, ref re);
            return ((BinaryOperator)args.op).func(le, re);
        }


    }
    #endregion


    class TypeConversion
    {
        Dictionary<Type, int> typePrecedence = null;
        static TypeConversion instance = new TypeConversion();
        /// <summary>
        /// Performs implicit conversion between two expressions depending on their type precedence
        /// </summary>
        /// <param name="le"></param>
        /// <param name="re"></param>
        internal static void Convert(ref Expression le, ref Expression re)
        {
            if (instance.typePrecedence.ContainsKey(le.Type) && instance.typePrecedence.ContainsKey(re.Type))
            {
                if (instance.typePrecedence[le.Type] > instance.typePrecedence[re.Type]) re = Expression.Convert(re, le.Type);
                if (instance.typePrecedence[le.Type] < instance.typePrecedence[re.Type]) le = Expression.Convert(le, re.Type);
            }
            else
            {
                if (IsNullableType(le.Type) && !IsNullableType(re.Type))
                    re = Expression.Convert(re, le.Type);
                else if (!IsNullableType(le.Type) && IsNullableType(re.Type))
                    le = Expression.Convert(le, re.Type);
            }
        }

        static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Performs implicit conversion on an expression against a specified type
        /// </summary>
        /// <param name="le"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Expression Convert(Expression le, Type type)
        {
            if (instance.typePrecedence.ContainsKey(le.Type) && instance.typePrecedence.ContainsKey(type))
            {
                if (instance.typePrecedence[le.Type] < instance.typePrecedence[type]) return Expression.Convert(le, type);
            }
            return le;
        }

        TypeConversion()
        {
            typePrecedence = new Dictionary<Type, int>();
            typePrecedence.Add(typeof(byte), 0);
            typePrecedence.Add(typeof(int), 1);
            typePrecedence.Add(typeof(float), 2);
            typePrecedence.Add(typeof(double), 3);
            typePrecedence.Add(typeof(decimal), 4);
        }
    }
}