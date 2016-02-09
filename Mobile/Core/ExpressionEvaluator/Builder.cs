using BitMobile.ExpressionEvaluator.Expressions;
using BitMobile.ExpressionEvaluator.Expressions.MemberExpression;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BitMobile.DbEngine;
using BitMobile.ValueStack;

namespace BitMobile.ExpressionEvaluator
{
    static class Builder
    {
        public static IExpression<T> BuildBlockExpression<T>(string str, ExpressionQueue<T> root)
        {
            ExpressionQueue<T> result = root;

            bool firstExpressionCreated = false;

            int bracketsCount = 0;

            int lastIndex = 0;

            for (int i = 0; i < str.Length; i++)
            {
                char current = str[i];

                bool isEOF = str.Length == i + 1;

                if (current == '(')
                {
                    bracketsCount++;
                }
                else if (current == ')')
                {
                    bracketsCount--;
                }
                if (bracketsCount == 0 && (isEOF || result.IsExpressionEdge(str, i)))
                {
                    string substring = str.Substring(lastIndex, i + 1 - lastIndex);

                    lastIndex = i + 1;
                    if (!firstExpressionCreated)
                    {
                        result.Handle(substring);

                        firstExpressionCreated = true;
                    }
                    else
                    {
                        substring = substring.TrimStart(' ');

                        result.Handle(substring);
                    }
                }
            }

            return result;
        }

        public static IExpression<T> BuildValueExpression<T>(string str, ExpressionFactory factory)
        {
            IExpression<T> result;

            if (!TryBuildParameter(str, factory, out result))
                if (!TryBuildConst(str, out result))
                    if (!TryBuildMembers(str, factory, out result))
                        throw new Exception("Cannot parse expression: " + str);

            return result;
        }

        static bool TryBuildParameter<T>(string str, ExpressionFactory factory, out IExpression<T> result)
        {
            result = null;

            if (str[0] == '@' || str[0] == '$')
            {
                string[] splitted = str.Split('.');

                IDictionary<string, object> parameters;
                if (str[0] == '@')
                    parameters = factory.Parameters;
                else
                    parameters = factory.ValueStack;

                object value;
                if (parameters.TryGetValue(splitted[0].Substring(1), out value))
                {
                    if (splitted.Length > 1)
                    {
                        string s = str.Substring(splitted[0].Length + 1);
                        IExpression<object> exp;
                        if (Builder.TryBuildMembers<object>(s, factory, value.GetType(), value, out exp))
                            value = exp.Evaluate(value);
                        else
                            return false;
                    }

                    result = new ObjectExpression<T>(value, str);

                    return true;
                }
            }

            return false;
        }

        static bool TryBuildConst<T>(string str, out IExpression<T> result)
        {
            result = null;

            // null
            if (string.Equals(str, "null", StringComparison.CurrentCultureIgnoreCase))
            {
                result = new ObjectExpression<T>(null, str);
                return true;
            }

            // string
            if (str[0] == '\'' && str[str.Length - 1] == '\'')
            {
                result = new ObjectExpression<T>(str.Substring(1, str.Length - 2), str);
                return true;
            }

            // bool
            bool valueBool;
            if (bool.TryParse(str, out valueBool))
            {
                result = new ObjectExpression<T>(valueBool, str);
                return true;
            }

            // guid
            Guid valueGuid;
            if (Guid.TryParse(str, out valueGuid))
            {
                result = new ObjectExpression<T>(valueGuid, str);
                return true;
            }

            // integer
            int valueInt;
            if (int.TryParse(str, out valueInt))
            {
                result = new ObjectExpression<T>(valueInt, str);
                return true;
            }

            // decimal
            decimal valueDecimal;
            if (decimal.TryParse(str, out valueDecimal))
            {
                result = new ObjectExpression<T>(valueDecimal, str);
                return true;
            }
            else
                if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out valueDecimal))
                {
                    result = new ObjectExpression<T>(valueDecimal, str);
                    return true;
                }

            // DateTime
            DateTime valueDateTime;
            if (DateTime.TryParse(str, out valueDateTime))
            {
                result = new ObjectExpression<T>(valueDateTime, str);
                return true;
            }
            else
                if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out valueDateTime))
                {
                    result = new ObjectExpression<T>(valueDateTime, str);
                    return true;
                }

            return false;
        }

        static bool TryBuildMembers<T>(string str, ExpressionFactory factory, out IExpression<T> result)
        {
            return TryBuildMembers<T>(str, factory, factory.BaseType, null, out result);
        }

        static bool TryBuildMembers<T>(string str
            , ExpressionFactory factory
            , Type baseType
            , object baseObject
            , out IExpression<T> result)
        {
            result = null;
            MemberExpression<T> block = new MemberExpression<T>(str);

            string[] members = str.Split('.');

            for (int i = 0; i < members.Length; i++)
            {
                string member = members[i].Trim();

                if (i == 0 && baseType.GetInterfaces().Contains(typeof(IDbRecordset)))
                {
                    block.Add(new DataReaderMember(member, member));
                    baseType = typeof(object);
                    continue;
                }

                if (baseType == typeof(DbRef))
                {
                    block.Add(new DbRefMember(member, member));
                    baseType = typeof(object);
                    continue;
                }

                if (baseType == typeof(IDbRef))
                {
                    block.Add(new DbRefMember(member, member));
                    baseType = typeof(object);
                    continue;
                }

                if (member.Contains('(') && member.Last() == ')')
                {
                    string[] details = member.Split('(', ')');

                    IExpression<object>[] p = ParseMethodParameters(factory, details);

                    // Check helper
                    MethodInfo helperMI = typeof(Helper).GetMethod(details[0]);
                    if (helperMI != null)
                    {
                        block.Add(new HelperMember(helperMI, p, member));
                        baseType = helperMI.ReturnType;
                    }
                    else
                    {
                        MethodInfo mi = baseType.GetMethod(details[0]);
                        if (mi != null)
                        {
                            block.Add(new MethodMember(mi, p, details[0]));
                            baseType = mi.ReturnType;
                        }
                        else
                            return false;
                    }

                }
                else if (member.Contains('[') && member.Last() == ']')
                {
                    // TODO: ¬крутить парсинг индексаторов
                    throw new NotSupportedException("Indexers are not supported :_(");
                }
                else
                {
                    // Check helper
                    MethodInfo helperMI = typeof(Helper).GetMethod(member);
                    if (helperMI != null)
                    {
                        block.Add(new HelperMember(helperMI, new IExpression<object>[0], member));
                        baseType = helperMI.ReturnType;
                    }
                    else
                    {
                        if (baseObject is IEntity)
                        {
                            block.Add(new EntityMember(member));
                            baseType = typeof(object);
                        }
                        else
                        {
                            string property = members[i].Trim();
                            PropertyInfo pi = baseType.GetProperty(property);
                            if (pi != null)
                            {
                                block.Add(new PropertyMember(pi, member));

                                baseType = pi.PropertyType;
                            }
                            else
                            {
                                // Dirty hack ("workflow.order")
                                if (baseType != null &&
                                    (baseType.ToString().Contains("BitMobile.ValueStack.CustomDictionary")
                                     || baseType.ToString().Contains("BitMobile.DbEngine.DbRef")))
                                {
                                    MethodInfo mi = baseType.GetMethod("GetValue");
                                    object itemValue = mi.Invoke(baseObject, new object[] { member });

                                    block.Add(new ValueMember(itemValue, member));

                                    if (itemValue != null)
                                        baseType = itemValue.GetType();
                                    else
                                        baseType = typeof(object);
                                }
                                else
                                    return false;
                            }
                        }
                    }
                }
            }

            result = block;
            return true;
        }

        static IExpression<object>[] ParseMethodParameters(ExpressionFactory factory, string[] details)
        {
            IExpression<object>[] p;

            if (!string.IsNullOrWhiteSpace(details[1]))
            {
                string[] methodParams = details[1].Split();
                p = new IExpression<object>[methodParams.Length];

                for (int j = 0; j < methodParams.Length; j++)
                {
                    IExpression<object> exp = Builder.BuildValueExpression<object>(methodParams[j].Trim(), factory);
                    p[j] = exp;
                }
            }
            else
                p = new IExpression<object>[0];
            return p;
        }

    }
}