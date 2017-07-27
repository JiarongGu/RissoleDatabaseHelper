using RissoleDatabaseHelper.Core.Attributes;
using RissoleDatabaseHelper.Core.Enums;
using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    /// <summary>
    /// helper class build table definition based on given type
    /// </summary>
    internal class RissoleDefinitionBuilder
    {
        public List<RissoleColumn> BuildColumns(Type model)
        {
            var columns = new List<RissoleColumn>();

            PropertyInfo[] properties = model.GetProperties();

            foreach (var property in properties)
            {
                //create column based on column attribute
                RissoleColumn column = BuildRissoleColumn(property);
                if(column != null)
                    columns.Add(column);
            }

            return columns;
        }

        public RissoleColumn BuildRissoleColumn(PropertyInfo property)
        {
            ColumnAttribute attribute = property.GetCustomAttribute<ColumnAttribute>();

            if (attribute == null) return null;

            //Default Data Definition
            RissoleColumn column = new RissoleColumn(property);

            if (!string.IsNullOrEmpty(attribute.Name))
                column.Name = attribute.Name;

            if (attribute.DataType != null)
                column.DataType = attribute.DataType;

            column.IsGenerated = attribute.IsGenerated;
            column.IsComputed = attribute.IsComputed;

            //create keys based on key attribute
            column.Keys = BuildRissoleKeys(property);

            return column;
        }

        public List<RissoleKey> BuildRissoleKeys(PropertyInfo property)
        {
            List<RissoleKey> keys = new List<RissoleKey>();
            PrimaryKeyAttribute primaryKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
            if(primaryKeyAttribute != null)
                keys.Add(new RissoleKey(primaryKeyAttribute));

            PrimaryKeyAttribute foreignKeyKeyAttribute = property.GetCustomAttribute<PrimaryKeyAttribute>();
            if (foreignKeyKeyAttribute != null)
                keys.Add(new RissoleKey(foreignKeyKeyAttribute));
            
            return keys;
        }

        public RissoleTable BuildRissoleTable(Type type)
        {
            // in this function table attribute should never be null
            var tableAttribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            // define new table, default as class name
            var table = new RissoleTable(type);

            // override table name if attribute defines
            if (tableAttribute.Name != null)
                table.Name = tableAttribute.Name;

            // create table columns
            table.Columns = BuildColumns(type);
            return table;
        }
    }
}
