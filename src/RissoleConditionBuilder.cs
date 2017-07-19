using RissoleDatabaseHelper.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace RissoleDatabaseHelper
{
    /// <summary>
    /// Build sql where condition from exepression tree
    /// </summary>
    internal class RissoleConditionBuilder
    {
        private readonly IRissoleProvider _rissoleProvider;
        private RissoleTable _rissoleTable;

        public RissoleConditionBuilder(IRissoleProvider provider)
        {
            _rissoleProvider = provider;
        }

        public string ToSql<T>(Expression<Func<T, bool>> expression)
        {
            _rissoleTable = _rissoleProvider.GetRissoleTable<T>();
            return Resolve(expression.Body);
        }

        private string Resolve(Expression expression, bool right = false)
        {
            if (expression is UnaryExpression) return ResolveUnaryExpression((UnaryExpression)expression);

            if (expression is BinaryExpression) return ResolveBinaryExpression((BinaryExpression)expression);

            if (expression is ConstantExpression) return ResolveConstantExpression((ConstantExpression)expression);

            if (expression is MemberExpression) return ResolveMemberExpression((MemberExpression)expression, right);

            if (expression is MethodCallExpression) return ResolveMethodCallExpression((MethodCallExpression)expression);

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private string ResolveUnaryExpression(UnaryExpression expression)
        {
            var right = Resolve(expression.Operand);
            var node = NodeTypeToString(expression.NodeType, right == "NULL");

            return $"({node} {right})";
        }

        private string ResolveBinaryExpression(BinaryExpression expression)
        {
            var left = Resolve(expression.Left);
            var right = Resolve(expression.Right, true);
            var node = NodeTypeToString(expression.NodeType, right == "NULL");

            return $"({left} {node} {right})";
        }

        private string ResolveConstantExpression(ConstantExpression expression)
        {
            return ValueToString(expression.Value);
        }

        private string ResolveMemberExpression(MemberExpression expression, bool right)
        {
            if (expression.Member is PropertyInfo)
            {
                var property = (PropertyInfo)expression.Member;

                if (right)
                {
                    var value = Expression.Lambda(expression).Compile().DynamicInvoke();
                    return ValueToString(value);
                }

                var columnName = _rissoleTable.GetColumnByPropertyName(property.Name).Name;
                return "[" + columnName + "]";
            }

            if (expression.Member is FieldInfo)
            {
                return ValueToString(GetValue(expression));
            }

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }

        private string ResolveMethodCallExpression(MethodCallExpression expression)
        {
            // LIKE queries:
            if (expression.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                return "(" + Resolve(expression.Object) + " LIKE '%" + Resolve(expression.Arguments[0]) + "%')";
            }
            if (expression.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                return "(" + Resolve(expression.Object) + " LIKE '" + Resolve(expression.Arguments[0]) + "%')";
            }
            if (expression.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                return "(" + Resolve(expression.Object) + " LIKE '%" + Resolve(expression.Arguments[0]) + "')";
            }
            // IN queries:
            if (expression.Method.Name == "Contains")
            {
                Expression collection;
                Expression property;
                if (expression.Method.IsDefined(typeof(ExtensionAttribute)) && expression.Arguments.Count == 2)
                {
                    collection = expression.Arguments[0];
                    property = expression.Arguments[1];
                }
                else if (!expression.Method.IsDefined(typeof(ExtensionAttribute)) && expression.Arguments.Count == 1)
                {
                    collection = expression.Object;
                    property = expression.Arguments[0];
                }
                else
                {
                    throw new Exception("Unsupported method call: " + expression.Method.Name);
                }
                var values = (IEnumerable)GetValue(collection);
                var concated = "";
                foreach (var e in values)
                {
                    concated += ValueToString(e) + ", ";
                }
                if (concated == "")
                {
                    return ValueToString(false);
                }
                return "(" + Resolve(property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
            }

            object value = Expression.Lambda(expression).Compile().DynamicInvoke();
            return value.ToString();

            throw new Exception("Unsupported method call: " + expression.Method.Name);
        }
        
        public string ValueToString(object value)
        {
            var quote = ValueTypeHasQuote(value.GetType());
            string convert = string.Empty;

            if (value is bool)
            {
                convert = (bool)value ? "1" : "0";
            }
            else
            {
                convert = value == null ? "NULL" : value.ToString();
            }

            return quote ? $"'{convert}'" : convert;
        }

        private static bool IsEnumerableType(Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericParameter && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static object NodeTypeToString(ExpressionType nodeType, bool isNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:                return "+";
                case ExpressionType.And:                return "&";
                case ExpressionType.AndAlso:            return "AND";
                case ExpressionType.Convert:            return isNull ? "NULL": "";
                case ExpressionType.Divide:             return "/";
                case ExpressionType.Equal:              return isNull ? "IS" : "=";
                case ExpressionType.ExclusiveOr:        return "^";
                case ExpressionType.GreaterThan:        return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan:           return "<";
                case ExpressionType.LessThanOrEqual:    return "<=";
                case ExpressionType.Modulo:             return "%";
                case ExpressionType.Multiply:           return "*";
                case ExpressionType.Negate:             return "-";
                case ExpressionType.Not:                return "NOT";
                case ExpressionType.NotEqual:           return isNull ? "IS NOT" : "<>";
                case ExpressionType.Or:                 return "|";
                case ExpressionType.OrElse:             return "OR";
                case ExpressionType.Subtract:           return "-";
                default: throw new Exception($"Unsupported node type: {nodeType}");
            }
        }

        private static bool ValueTypeHasQuote(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Boolean:
                    return false;
                default:
                    return true;
            }
        }
    }
}
