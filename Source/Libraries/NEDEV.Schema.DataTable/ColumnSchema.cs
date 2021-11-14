using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace NEDev.Schema.DataTable
{
    public interface IColumnSchema : IEquatable<IColumnSchema>
    {
        string Name { get; }
        ColumnType Type { get; }
        string RawType { get; }
        string TypeName { get; }
        bool IsArray { get; }
        string DateTimeFormat { get; }
        string DefaultValue { get; }
        string Comment { get; }
    }

    public sealed class ColumnSchema : IColumnSchema
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ColumnType Type { get; set; }

        // for debug
        [JsonIgnore]
        public string RawType { get; set; }
        
        public string TypeName { get; set; }
        public bool IsArray { get; set; }
        public string DateTimeFormat { get; set; }
        public string DefaultValue { get; set; }
        public string Comment { get; set; }

        internal ColumnSchema()
        {

        }

        internal ColumnSchema(string name, string rawType, ColumnType type, string typeName, bool isArray, string dateTimeFormat = null, string defaultValue = null, string comment = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RawType = rawType ?? throw new ArgumentNullException(nameof(rawType));
            Type = type != ColumnType.Invalid ? type : throw new ArgumentException(nameof(type));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            IsArray = isArray;
            DateTimeFormat = dateTimeFormat;
            DefaultValue = defaultValue;
            Comment = comment;
        }

        public bool Equals(IColumnSchema other)
        {
            return Name == other.Name &&
                Type == other.Type &&
                TypeName == other.TypeName &&
                IsArray == other.IsArray &&
                DateTimeFormat == other.DateTimeFormat &&
                DefaultValue == other.DefaultValue &&
                Comment == other.Comment;
        }
    }
}
