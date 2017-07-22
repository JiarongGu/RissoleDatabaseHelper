using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    /// <summary>
    /// Build sql where condition from exepression tree
    /// </summary>
    internal class RissoleConditionBuilder
    {
        public RissoleScript RissoleScript(LambdaExpression expression, ICollection<RissoleTable> rissoleTables, int commandStack)
        {
            var parameters = ResolveParameters(expression, rissoleTables);

            var rssioleScript = ResolveScript(expression.Body, parameters, commandStack, 0);

            return rssioleScript;
        }

        private Dictionary<ParameterExpression, RissoleTable> ResolveParameters(LambdaExpression expression, 
            ICollection<RissoleTable> rissoleTables)
        {
            Dictionary<ParameterExpression, RissoleTable> parameters = new Dictionary<ParameterExpression, RissoleTable>();

            foreach (var rissoleTable in rissoleTables)
            {
                var lambdaParameter = expression.Parameters.First(x => x.Type == rissoleTable.ReferenceType);
                parameters.Add(lambdaParameter, rissoleTable);
            }

            return parameters;
        }

        public RissoleScript ResolveScript(Expression expression, Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            if (expression is UnaryExpression)
                return ResolveUnaryExpression((UnaryExpression)expression, parameters, commandStack, ++stack);

            if (expression is BinaryExpression)
                return ResolveBinaryExpression((BinaryExpression)expression, parameters, commandStack, ++stack);

            if (expression is ConstantExpression)
                return ResolveConstantExpression((ConstantExpression)expression, commandStack, ++stack);

            if (expression is MemberExpression)
                return ResolveMemberExpression((MemberExpression)expression, parameters, commandStack, ++stack);

            if (expression is MethodCallExpression)
                return ResolveMethodCallExpression((MethodCallExpression)expression, parameters, commandStack, ++stack);

            if (expression is ParameterExpression)
                return ResolveParameterException((ParameterExpression)expression, parameters, commandStack, ++stack);

            if (expression is NewExpression)
                return ResolveNewException((NewExpression)expression, parameters, commandStack, ++stack);

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private RissoleScript ResolveParameterException(ParameterExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            var table = parameters[expression];
            var script = string.Join(", ", table.Columns.Select(x => $"{table.Name}.{x.Name}").ToList());

            var rissoleScript = new RissoleScript(script);

            return rissoleScript;
        }

        private RissoleScript ResolveUnaryExpression(UnaryExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            var right = ResolveScript(expression.Operand, parameters, commandStack, ++stack);
            var isNull = right.Parameters.FirstOrDefault().Value == null;
            var node = RissoleQueryDictionary.NodeTypeToString(expression.NodeType, isNull);

            var script = $"({node} {right.Script})";
            var rissoleScript = new RissoleScript(script, right.Parameters);
            
            return rissoleScript;
        }

        private RissoleScript ResolveBinaryExpression(BinaryExpression expression,
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            var left = ResolveScript(expression.Left, parameters, commandStack, ++stack);
            var right = ResolveScript(expression.Right, parameters, commandStack, ++stack);
            var isNull = right.Parameters.Count > 0 && right.Parameters.First().Value == null;
            var node = RissoleQueryDictionary.NodeTypeToString(expression.NodeType, isNull);

            var script = $"({left.Script} {node} {right.Script})";
            var rissoleScript = new RissoleScript(script, left.Parameters, right.Parameters);

            return rissoleScript;
        }

        private RissoleScript ResolveConstantExpression(ConstantExpression expression, int commandStack, int stack)
        {
            return ValueToRissoleScript(expression.Value, commandStack, stack);
        }

        private RissoleScript ResolveMemberExpression(MemberExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
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
                return ValueToRissoleScript(GetValue(expression), commandStack, stack);
            }

            if (expression.Member is FieldInfo)
            {
                return ValueToRissoleScript(GetValue(expression), commandStack, stack);
            }

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }

        private RissoleScript ResolveNewException(NewExpression expression,
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            var values = new List<RissoleScript>();
            foreach (var argument in expression.Arguments)
            {
                values.Add(ResolveScript(argument, parameters, commandStack, ++stack));
            }

            var script = string.Join(", ", values.Select(x => x.Script).ToList());

            return new RissoleScript(script);
        }

        private RissoleScript ResolveMethodCallExpression(MethodCallExpression expression, 
            Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            switch (expression.Method.Name)
            {
                case "Contains": return ResolveContainsMethod(expression, parameters, commandStack, ++stack);
            }

            if (expression.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                var left = ResolveScript(expression.Object, parameters, commandStack, ++stack);
                var right = ResolveScript(expression.Arguments[0], parameters, commandStack, ++stack);
                var script = $"({left.Script} LIKE '{right.Script}%')";

                return new RissoleScript(script, left.Parameters, right.Parameters);
            }

            if (expression.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                var left = ResolveScript(expression.Object, parameters, commandStack, ++stack);
                var right = ResolveScript(expression.Arguments[0], parameters, commandStack, ++stack);
                var script = $"({left.Script} LIKE '%{right.Script}')";

                return new RissoleScript(script, left.Parameters, right.Parameters);
            }

            if (expression.Object is MemberExpression)
            {
                return ResolveScript(expression.Object, parameters, commandStack, ++stack);
            }

            object value = Expression.Lambda(expression).Compile().DynamicInvoke();
            return ValueToRissoleScript(value, commandStack, stack);

            throw new Exception("Unsupported method call: " + expression.Method.Name);
        }

        private RissoleScript ResolveContainsMethod(MethodCallExpression expression,
    Dictionary<ParameterExpression, RissoleTable> parameters, int commandStack, int stack)
        {
            // LIKE queries:
            if (expression.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                var left = ResolveScript(expression.Object, parameters, commandStack, ++stack);
                var right = ResolveScript(expression.Arguments[0], parameters, commandStack, ++stack);
                var script = $"({left.Script} LIKE '%{right.Script}%')";

                return new RissoleScript(script, left.Parameters, right.Parameters);
            }
            else
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
                    throw new Exception($"Unsupported method call: {expression.Method.Name}, Args: {expression.Arguments.Count}");
                }

                var values = (IEnumerable)GetValue(collection);
                var left = ResolveScript(property, parameters, commandStack, ++stack);

                List<RissoleScript> subScripts = new List<RissoleScript>();

                foreach (var value in values)
                {
                    subScripts.Add(ValueToRissoleScript(value, commandStack, ++stack));
                }

                if (subScripts.Count == 0)
                {
                    return ValueToRissoleScript(false, commandStack, ++stack);
                }

                var script = $"({left} IN ({string.Join(", ", subScripts.Select(x => x.Script).ToList())})";

                return new RissoleScript(script, subScripts.Select(x => x.Parameters).ToList());
            }
        }

        public RissoleScript ValueToRissoleScript(object value, int commandStack, int stack)
        {
            var valueName = value == null ? "NULL" : value.GetType().Name;
            var parameterName = $"{valueName}_{commandStack}_{stack}";

            var rissoleScript = new RissoleScript();
            rissoleScript.Parameters.Add(parameterName, value);
            rissoleScript.Script = $"@{parameterName}";

            return rissoleScript;
        }

        public object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(objectMember);
            return lambda.Compile().DynamicInvoke();
        }
    }
}
