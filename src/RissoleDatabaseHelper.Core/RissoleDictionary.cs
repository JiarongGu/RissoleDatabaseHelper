using RissoleDatabaseHelper.Core.Enums;
using RissoleDatabaseHelper.Core.Exceptions;
using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    /// <summary>
    /// static dictionary class to convert constent values
    /// </summary>
    internal static class RissoleDictionary
    {
        public static readonly Dictionary<Type, DbType> DbTypeMap = new Dictionary<Type, DbType>()
        {
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {typeof(DateTimeOffset?), DbType.DateTimeOffset}
        };

        public static object NodeTypeToString(ExpressionType nodeType, bool isNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add: return "+";
                case ExpressionType.And: return "&";
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.Convert: return isNull ? "NULL" : "";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Equal: return isNull ? "IS" : "=";
                case ExpressionType.ExclusiveOr: return "^";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Negate: return "-";
                case ExpressionType.Not: return "NOT";
                case ExpressionType.NotEqual: return isNull ? "IS NOT" : "<>";
                case ExpressionType.Or: return "|";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Subtract: return "-";
                default: throw new Exception($"Unsupported node type: {nodeType}");
            }
        }

        public static bool ValueTypeHasQuote(Type type)
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

        public static List<RissoleReferencedScript> GetReferenceScripts(ReferencedScriptType referencedScriptType)
        {
            switch (referencedScriptType)
            {
                case ReferencedScriptType.GetLastInsert: return LastInsertScripts;
                default: throw new RissoleException($"no script defined for {referencedScriptType.ToString()}");
            }
        }

        private static readonly List<RissoleReferencedScript> LastInsertScripts = new List<RissoleReferencedScript>()
        {
            new RissoleReferencedScript("SELECT LAST_INSERT_ID();"),
            new RissoleReferencedScript("SELECT SCOPE_IDENTITY();"),
            new RissoleReferencedScript("SELECT lastval();"),
            new RissoleReferencedScript("SELECT SEQNAME.CURRVAL FROM DUAL;")
        };
    }
}
