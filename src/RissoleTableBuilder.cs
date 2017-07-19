using RissoleDatabaseHelper.Attributes;
using RissoleDatabaseHelper.Enums;
using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Internals
{
    /// <summary>
    /// helper class build table definition based on given type
    /// </summary>
    internal class RissoleTableBuilder
    {
        private List<RissoleColumn> BuildColumns(Type model)
        {
            var columns = new List<RissoleColumn>();

            PropertyInfo[] properties = model.GetProperties();

            foreach (var property in properties)
            {
                //create column based on column attribute
                RissoleColumn column = BuildColumn(property);
                columns.Add(column);
            }

            return columns;
        }

        private RissoleColumn BuildColumn(PropertyInfo property)
        {
            CustomAttributeData columnAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute));

            if (columnAttribute == null)
                return null;

            //Default Data Definition
            RissoleColumn column = new RissoleColumn(property);

            //Update By Attribute if exists
            foreach (var value in columnAttribute.NamedArguments)
            {
                switch (value.MemberName)
                {
                    case "Name": column.Name = (string)value.TypedValue.Value; break;
                    case "DataType": column.DataType = (Type)value.TypedValue.Value;  break;
                    case "IsGenerated": column.IsGenerated = (bool)value.TypedValue.Value; break;
                    case "IsComputed": column.IsComputed = (bool)value.TypedValue.Value; break;
                    default: throw new Exception("Unknow Attribute: " + value.MemberName);
                }
            }
            
            //create keys based on key attribute
            column.Keys = BuildKeys(property);

            return column;
        }

        private List<RissoleKey> BuildKeys(PropertyInfo property)
        {
            List<CustomAttributeData> keyAttributes = property.CustomAttributes.Where(x => x.AttributeType == typeof(KeyAttribute)).ToList();

            if(keyAttributes.Count == 0)
                return null;

            List<RissoleKey> keys = new List<RissoleKey>();

            foreach (var foreignKeyAttribute in keyAttributes)
            {
                RissoleKey key = new RissoleKey();

                foreach (var value in foreignKeyAttribute.NamedArguments)
                {
                    switch (value.MemberName)
                    {
                        case "Table": key.TableName = (string)value.TypedValue.Value; break;
                        case "Column": key.ColumnName = (string)value.TypedValue.Value; break;
                        case "Type": key.Type = (KeyType)value.TypedValue.Value; break;
                        default: throw new Exception("Unknow Attribute: " + value.MemberName);
                    }
                }

                keys.Add(key);
            }

            return keys;
        }

        public RissoleTable BuildTable(Type model)
        {
            // in this function table attribute should never be null
            var tableAttribute = model.GetTypeInfo().GetCustomAttributes<TableAttribute>().First();

            // define new table, default as class name
            var table = new RissoleTable(model.GetTypeInfo().Name);

            // override table name if attribute defines
            if (tableAttribute.Name != null)
            {
                table.Name = tableAttribute.Name;
            }

            // create table columns
            table.Columns = BuildColumns(model);
            return table;
        }
    }
}
