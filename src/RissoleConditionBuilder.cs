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

            var rssioleScript = ResolveScript(expression.Body, parameters, 0);

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

        private RissoleScript ResolveScript(Expression expression, Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            if (expression is UnaryExpression) return ResolveUnaryExpression((UnaryExpression)expression, parameters, stack++);

            if (expression is BinaryExpression) return ResolveBinaryExpression((BinaryExpression)expression, parameters, stack++);

            if (expression is ConstantExpression) return ResolveConstantExpression((ConstantExpression)expression, stack++);

            if (expression is MemberExpression) return ResolveMemberExpression((MemberExpression)expression, parameters, stack++);

            if (expression is MethodCallExpression) return ResolveMethodCallExpression((MethodCallExpression)expression, parameters, stack++);

            if (expression is ParameterExpression) return ResolveTypeAccessException((ParameterExpression)expression, parameters, stack++);

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private RissoleScript ResolveTypeAccessException(ParameterExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            var table = parameters[expression];
            var script = string.Join(", ", table.Columns.Select(x => $"{table.Name}.{x.Name}").ToList());

            var rissoleScript = new RissoleScript(script);

            return rissoleScript;
        }

        private RissoleScript ResolveUnaryExpression(UnaryExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            var right = ResolveScript(expression.Operand, parameters, stack++);
            var node = NodeTypeToString(expression.NodeType, right.Parameters.FirstOrDefault().Value == null);

            var script = $"({node} {right.Script})";
            var rissoleScript = new RissoleScript(script, right.Parameters);
            
            return rissoleScript;
        }

        private RissoleScript ResolveBinaryExpression(BinaryExpression expression,
            Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            var left = ResolveScript(expression.Left, parameters, stack++);
            var right = ResolveScript(expression.Right, parameters, stack++);
            var isNull = right.Parameters.Count > 0 && right.Parameters.First().Value == null;
            var node = NodeTypeToString(expression.NodeType, isNull);

            var script = $"({left.Script} {node} {right.Script})";
            var rissoleScript = new RissoleScript(script, left.Parameters, right.Parameters);

            return rissoleScript;
        }

        private RissoleScript ResolveConstantExpression(ConstantExpression expression, int stack)
        {
            return ValueToRissoleScript(expression.Value, stack);
        }

        private RissoleScript ResolveMemberExpression(MemberExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            if (expression.Member is PropertyInfo)
            {
                if (expression.Expression is ParameterExpression)
                {
                    var property = (PropertyInfo)expression.Member;
                    var table = parameters[(ParameterExpression)expression.Expression];
                    var columnName = table.Columns.First(x => x.Property == property).Name;

                    var script = $"{table.Name}.{columnName}";
                    var rissoleScript = new RissoleScript(script);

                    return rissoleScript;
                }
                return ValueToRissoleScript(GetValue(expression), stack);
            }

            if (expression.Member is FieldInfo)
            {
                return ValueToRissoleScript(GetValue(expression), stack);
            }

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }

        private RissoleScript ResolveMethodCallExpression(MethodCallExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int stack)
        {
            // LIKE queries:
            if (expression.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                var script = "(" + ResolveScript(expression.Object, parameters, stack++) + " LIKE '%"
                    + ResolveScript(expression.Arguments[0], parameters, stack++) + "%')";
                return new RissoleScript(script);
            }
            if (expression.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                var script = "(" + ResolveScript(expression.Object, parameters, stack++) + " LIKE '" +
                    ResolveScript(expression.Arguments[0], parameters, stack++) + "%')";
                return new RissoleScript(script);
            }
            if (expression.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                var script = "(" + ResolveScript(expression.Object, parameters, stack++) + " LIKE '%" +
                    ResolveScript(expression.Arguments[0], parameters, stack++) + "')";
                return new RissoleScript(script);
            }

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
                    return ValueToRissoleScript(false, stack);
                }

                var script = "(" + ResolveScript(property, parameters, stack++) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";

                return new RissoleScript(script);
            }

            object value = Expression.Lambda(expression).Compile().DynamicInvoke();
            return ValueToRissoleScript(value, stack);

            throw new Exception("Unsupported method call: " + expression.Method.Name);
        }

        public RissoleScript ValueToRissoleScript(object value, int stack)
        {
            var parameterName = $"{value.GetType().Name}_{stack}";

            var rissoleScript = new RissoleScript();
            rissoleScript.Parameters.Add(parameterName, value);
            rissoleScript.Script = $"@{parameterName}";

            return rissoleScript;
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
