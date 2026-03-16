using hotelier_core_app.Domain.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace hotelier_core_app.Domain.SqlGenerator
{
    internal static class ExpressionHelper
    {
        public static string GetPropertyName<TSource, TField>(Expression<Func<TSource, TField>> field)
        {
            if (object.Equals(field, null))
            {
                throw new ArgumentNullException("field", "field can't be null");
            }

            Expression body = field.Body;
            MemberExpression memberExpression2;
            if (body is not MemberExpression memberExpression)
            {
                if (body is not UnaryExpression unaryExpression)
                {
                    throw new ArgumentException("Expression field isn't supported", "field");
                }

                memberExpression2 = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression2 = memberExpression;
            }

            return memberExpression2.Member.Name;
        }

        public static Func<PropertyInfo, bool> GetPrimitivePropertiesPredicate()
        {
            return (PropertyInfo p) => p.CanWrite && (p.PropertyType.IsValueType || p.GetCustomAttributes<ColumnAttribute>().Any() || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[]));
        }

        public static object GetValue(Expression member)
        {
            return GetValue(member, out _);
        }

        private static object GetValue(Expression member, out string parameterName)
        {
            parameterName = null;
            if (member == null)
            {
                return null;
            }

            if (member is not MemberExpression memberExpression)
            {
                if (member is ConstantExpression constantExpression)
                {
                    return constantExpression.Value;
                }

                if (member is not MethodCallExpression methodCallExpression)
                {
                    if (member is UnaryExpression unaryExpression && (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked) && unaryExpression.Type.UnwrapNullableType() == unaryExpression.Operand.Type)
                    {
                        return GetValue(unaryExpression.Operand, out parameterName);
                    }
                }
                else
                {
                    parameterName = methodCallExpression.Method.Name;
                }
            }
            else
            {
                object value = GetValue(memberExpression.Expression, out parameterName);
                try
                {
                    MemberInfo member2 = memberExpression.Member;
                    if (member2 is FieldInfo fieldInfo)
                    {
                        parameterName = ((parameterName != null) ? (parameterName + "_") : "") + fieldInfo.Name;
                        return fieldInfo.GetValue(value);
                    }

                    if (member2 is PropertyInfo propertyInfo)
                    {
                        parameterName = ((parameterName != null) ? (parameterName + "_") : "") + propertyInfo.Name;
                        return propertyInfo.GetValue(value);
                    }
                }
                catch
                {
                }
            }

            return Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object)), Array.Empty<ParameterExpression>()).Compile()();
        }

        public static string GetSqlOperator(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal or ExpressionType.MemberAccess or ExpressionType.Not => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.And or ExpressionType.AndAlso => "AND",
                ExpressionType.Or or ExpressionType.OrElse => "OR",
                ExpressionType.Default => string.Empty,
                _ => throw new NotSupportedException($"{type} isn't supported"),
            };
        }

        public static string GetSqlLikeValue(string methodName, object value)
        {
            value ??= string.Empty;

            switch (methodName)
            {
                case "CompareString":
                case "Equals":
                    return value.ToString();
                case "StartsWith":
                    return $"{value}%";
                case "EndsWith":
                    return $"%{value}";
                case "StringContains":
                    return $"%{value}%";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetMethodCallSqlOperator(string methodName, bool isNotUnary = false)
        {
            switch (methodName)
            {
                case "EndsWith":
                case "StartsWith":
                case "StringContains":
                    if (!isNotUnary)
                    {
                        return "LIKE";
                    }

                    return "NOT LIKE";
                case "Contains":
                    if (!isNotUnary)
                    {
                        return "IN";
                    }

                    return "NOT IN";
                case "Equals":
                case "CompareString":
                    if (!isNotUnary)
                    {
                        return "=";
                    }

                    return "!=";
                case "All":
                case "Any":
                    return methodName.ToUpperInvariant();
                default:
                    throw new NotSupportedException(methodName + " isn't supported");
            }
        }

        public static object GetValuesFromStringMethod(MethodCallExpression callExpr)
        {
            return GetValue(callExpr.Method.IsStatic ? callExpr.Arguments[1] : callExpr.Arguments[0]);
        }

        public static object GetValuesFromCollection(MethodCallExpression callExpr)
        {
            MemberExpression member = (callExpr.Method.IsStatic ? callExpr.Arguments.First() : callExpr.Object) as MemberExpression;
            try
            {
                return GetValue(member);
            }
            catch
            {
                throw new NotSupportedException(callExpr.Method.Name + " isn't supported");
            }
        }

        public static MemberExpression GetMemberExpression(Expression expression)
        {
            if (!(expression is MethodCallExpression methodCallExpression))
            {
                if (!(expression is MemberExpression result))
                {
                    if (!(expression is UnaryExpression unaryExpression))
                    {
                        if (!(expression is BinaryExpression binaryExpression))
                        {
                            if (expression is LambdaExpression lambdaExpression)
                            {
                                Expression body = lambdaExpression.Body;
                                if (body is MemberExpression result2)
                                {
                                    return result2;
                                }

                                if (body is UnaryExpression unaryExpression2)
                                {
                                    return (MemberExpression)unaryExpression2.Operand;
                                }
                            }

                            return null;
                        }

                        BinaryExpression binaryExpression2 = binaryExpression;
                        if (binaryExpression2.Left is UnaryExpression unaryExpression3)
                        {
                            return (MemberExpression)unaryExpression3.Operand;
                        }

                        return (MemberExpression)binaryExpression2.Left;
                    }

                    return (MemberExpression)unaryExpression.Operand;
                }

                return result;
            }

            if (methodCallExpression.Method.IsStatic)
            {
                return (MemberExpression)methodCallExpression.Arguments.Last((Expression x) => x.NodeType == ExpressionType.MemberAccess);
            }

            return (MemberExpression)methodCallExpression.Arguments[0];
        }

        public static string GetPropertyNamePath(Expression expr, out bool nested)
        {
            StringBuilder stringBuilder = new StringBuilder();
            MemberExpression memberExpression = GetMemberExpression(expr);
            int num = 0;
            do
            {
                num++;
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Insert(0, "");
                }

                stringBuilder.Insert(0, memberExpression.Member.Name);
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }
            while (memberExpression != null);
            if (num > 2)
            {
                throw new ArgumentException("Only one degree of nesting is supported");
            }

            nested = num == 2;
            return stringBuilder.ToString();
        }
    }
}
