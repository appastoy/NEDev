using System;
using System.Collections.Generic;
using System.Linq;

namespace NEDev.Schema.DataTable
{
    public interface ISheetSchema : IEquatable<ISheetSchema>
    {
        string Name { get; }
        IReadOnlyList<IColumnSchema> Columns { get; }
    }

    public sealed class SheetSchema : ISheetSchema
    {
        public string Name { get; set; }
        public ColumnSchema[] Columns { get; set; }

        IReadOnlyList<IColumnSchema> ISheetSchema.Columns => Columns;

        internal SheetSchema()
        { 
        }

        internal SheetSchema(string name, ColumnSchema[] columns)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public bool Equals(ISheetSchema other)
        {
            return Name == other.Name &&
                Columns.SequenceEqual(other.Columns);
        }
    }
}
