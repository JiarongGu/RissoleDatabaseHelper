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
        public RissoleScript RissoleScript(LambdaExpression expression, ICollection<RissoleTable> rissoleTables)
        {
            var parameters = ResolveParameters(expression, rissoleTables);

            var script = ResolveScript(expression.Body, parameters);

            var rssioleScript = new RissoleScript();
            rssioleScript.Script = script;

            return rssioleScript;
        }

        private Dictionary<ParameterExpression, RissoleTable> ResolveParameters(LambdaExpression expression, ICollection<RissoleTable> rissoleTables)
        {
            Dictionary<ParameterExpression, RissoleTable> parameters = new Dictionary<ParameterExpression, RissoleTable>();

            foreach (var rissoleTable in rissoleTables)
            {
                var lambdaParameter = expression.Parameters.First(x => x.Type == rissoleTable.ReferenceType);
                parameters.Add(lambdaParameter, rissoleTable);
            }

            return parameters;
        }

        private string ResolveScript(Expression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            if (expression is UnaryExpression) return ResolveUnaryExpression((UnaryExpression)expression, parameters);

            if (expression is BinaryExpression) return ResolveBinaryExpression((BinaryExpression)expression, parameters);

            if (expression is ConstantExpression) return ResolveConstantExpression((ConstantExpression)expression);

            if (expression is MemberExpression) return ResolveMemberExpression((MemberExpression)expression, parameters);

            if (expression is MethodCallExpression) return ResolveMethodCallExpression((MethodCallExpression)expression, parameters);

            if (expression is ParameterExpression) return ResolveTypeAccessException((ParameterExpression)expression, parameters);

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private string ResolveTypeAccessException(ParameterExpression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            var table = parameters[expression];
            var columnName = string.Join(", ", table.Columns.Select(x => $"{table.Name}.{x.Name}").ToList());
            return columnName;
        }

        private string ResolveUnaryExpression(UnaryExpression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            var right = ResolveScript(expression.Operand, parameters);
            var node = NodeTypeToString(expression.NodeType, right == "NULL");
            return $"({node} {right})";
        }

        private string ResolveBinaryExpression(BinaryExpression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            var left = ResolveScript(expression.Left, parameters);
            var right = ResolveScript(expression.Right, parameters);
            var node = NodeTypeToString(expression.NodeType, right == "NULL");

            return $"({left} {node} {right})";
        }

        private string ResolveConstantExpression(ConstantExpression expression)
        {
            return ValueToString(expression.Value);
        }

        private string ResolveMemberExpression(MemberExpression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            if (expression.Member is PropertyInfo)
            {
                if (expression.Expression is ParameterExpression)
                {
                    var property = (PropertyInfo)expression.Member;
                    var table = parameters[(ParameterExpression)expression.Expression];
                    var columnName = table.Columns.First(x => x.Property == property).Name;
                    
                    return $"{table.Name}.{columnName}";
                }

                return ValueToString(GetValue(expression));
            }

            if (expression.Member is FieldInfo)
            {
                return ValueToString(GetValue(expression));
            }

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }

        private string ResolveMethodCallExpression(MethodCallExpression expression, Dictionary<ParameterExpression, RissoleTable> parameters)
        {
            // LIKE queries:
            if (expression.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                return "(" + ResolveScript(expression.Object, parameters) + " LIKE '%" + ResolveScript(expression.Arguments[0], parameters) + "%')";
            }
            if (expression.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                return "(" + ResolveScript(expression.Object, parameters) + " LIKE '" + ResolveScript(expression.Arguments[0], parameters) + "%')";
            }
            if (expression.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                return "(" + ResolveScript(expression.Object, parameters) + " LIKE '%" + ResolveScript(expression.Arguments[0], parameters) + "')";
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
                return "(" + ResolveScript(property, parameters) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
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
