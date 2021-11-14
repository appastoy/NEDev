using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("NEDev.Schema.DataTable.Tests")]
namespace NEDev.Schema.DataTable
{
    internal class ColumnSchemaParser
    {
        static readonly char[] typeSplitChars = { ':' };
        static readonly char[] defaultValueSplitChars = { '=' };
        static readonly char[] dateTimeSplitChars = { '@' };
        static readonly char[] arraySplitChars = { '|' };
        static readonly Regex validNameRE = new Regex(@"^[_a-zA-Z][_a-zA-Z0-9]*$", RegexOptions.Compiled | RegexOptions.Multiline);
        static readonly Regex datetimeFormatEscapeCharRE = new Regex(@"[^ a-zA-Z0-9]", RegexOptions.Compiled);

        public static bool TryParse(string sheetName, string columnName, string columnType, string columnComment, out ColumnSchema columnSchema)
        {
            columnSchema = null;
            var safeColumnName = columnName?.Trim() ?? string.Empty;
            var safeColumnType = columnType?.Trim() ?? string.Empty;
            if (!validNameRE.IsMatch(safeColumnName))
                return false;

            var typeInfo = ParseType(sheetName, safeColumnName, safeColumnType);
            columnSchema = new ColumnSchema(safeColumnName, 
                                            safeColumnType, 
                                            typeInfo.Type, 
                                            typeInfo.TypeName, 
                                            typeInfo.IsArray, 
                                            typeInfo.DateTimeFormat,
                                            typeInfo.DefaultValue, 
                                            columnComment);
            return true;
        }

        static (ColumnType Type, string TypeName, bool IsArray, string DateTimeFormat, string DefaultValue) ParseType(string sheetName, string columnName, string rawType)
        {
            ColumnType type = ColumnType.Invalid;
            string defaultValue = null;
            string datetimeFormat = null;

            // Parse DefaultValue.
            var defaultValueTokens = rawType.Split(defaultValueSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (defaultValueTokens.Length > 1)
            {
                defaultValue = string.Join(string.Empty, defaultValueTokens.Skip(1)).Trim();
                rawType = defaultValueTokens[0].Trim();
            }
            else
            {
                rawType = defaultValueTokens[0];
            }

            // Parse DateTime or TimeSpan
            var dateTimeTokens = rawType.Split(dateTimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (dateTimeTokens.Length > 1)
            {
                datetimeFormat = string.Join(string.Empty, dateTimeTokens.Skip(1)).Trim();
                rawType = dateTimeTokens[0].Trim();
            }
            else
            {
                rawType = dateTimeTokens[0];
            }

            // Parse Enum, Custom type.
            var typeTokens = rawType.Split(typeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (typeTokens.Length > 1)
            {
                switch (typeTokens[0].Trim())
                {
                    case "c": case "C": type = ColumnType.Custom; break;
                    case "e": case "E": type = ColumnType.Enum; break;
                    default:  throw new DataTableException(sheetName, columnName, $"Unsupported Type ({rawType}).");
                }
                rawType = string.Join(string.Empty, typeTokens.Skip(1)).Trim();
            }
            else
            {
                rawType = typeTokens[0];
            }

            // Parse IsArray.
            var isArray = rawType.EndsWith("[]");
            if (isArray)
                rawType = rawType.Substring(0, rawType.Length - 2);

            // Parse Type.
            if (type == ColumnType.Invalid)
            {
                if (!Enum.TryParse(rawType, true, out type) || type == ColumnType.Invalid)
                    throw new DataTableException(sheetName, columnName, $"Unsupported Type ({rawType}).");
            }

            // Validate default value.
            if (!string.IsNullOrEmpty(defaultValue))
            {
                if (isArray)
                {
                    foreach (var defaultValueItem in defaultValue.Split(arraySplitChars, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var refValue = defaultValueItem?.Trim() ?? string.Empty;
                        ValidateDefaultValue(sheetName, columnName, type, datetimeFormat, ref refValue);
                    }
                }
                else
                {
                    ValidateDefaultValue(sheetName, columnName, type, datetimeFormat, ref defaultValue);
                }
            }

            return (type, rawType, isArray, datetimeFormat, defaultValue);
        }

        static void ValidateDefaultValue(string sheetName, string columnName, ColumnType type, string datetimeFormat, ref string defaultValue)
        {
            switch (type)
            {
                case ColumnType.Bool:
                    {
                        var defaultValueLower = defaultValue.ToLower();
                        if (defaultValueLower != "true" && 
                            defaultValueLower != "1" &&
                            defaultValueLower != "false" &&
                            defaultValueLower != "0")
                            throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). true or 1, false or 0 are valid value.");
                    }
                    break;

                case ColumnType.String:
                    defaultValue = defaultValue.Trim('"');
                    break;

                case ColumnType.Int8:
                    if (!sbyte.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {sbyte.MinValue} ~ {sbyte.MaxValue} are valid value.");
                    break;

                case ColumnType.Int16:
                    if (!short.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {short.MinValue} ~ {short.MaxValue} are valid value.");
                    break;

                case ColumnType.Int32:
                    if (!int.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {int.MinValue} ~ {int.MaxValue} are valid value.");
                    break;

                case ColumnType.Int64:
                    if (!long.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {long.MinValue} ~ {long.MaxValue} are valid value.");
                    break;

                case ColumnType.UInt8:
                    if (!byte.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {byte.MinValue} ~ {byte.MaxValue} are valid value.");
                    break;

                case ColumnType.UInt16:
                    if (!ushort.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {ushort.MinValue} ~ {ushort.MaxValue} are valid value.");
                    break;

                case ColumnType.UInt32:
                    if (!uint.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {uint.MinValue} ~ {uint.MaxValue} are valid value.");
                    break;

                case ColumnType.UInt64:
                    if (!ulong.TryParse(defaultValue, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {ulong.MinValue} ~ {ulong.MaxValue} are valid value.");
                    break;

                case ColumnType.Float32:
                    if (!float.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {float.MinValue} ~ {float.MaxValue} are valid value.");
                    break;

                case ColumnType.Float64:
                    if (!double.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {double.MinValue} ~ {double.MaxValue} are valid value.");
                    break;

                case ColumnType.Fixed128:
                    if (!decimal.TryParse(defaultValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {decimal.MinValue} ~ {decimal.MaxValue} are valid value.");
                    break;

                case ColumnType.Date:
                    {
                        var currentFormat = string.IsNullOrEmpty(datetimeFormat) ? DataTableSchema.DefaultDateFormat : datetimeFormatEscapeCharRE.Replace(datetimeFormat, m=>$@"\{m.Value}");
                        if (!DateTime.TryParseExact(defaultValue, currentFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {currentFormat} is valid value format.");
                    }
                    break;

                case ColumnType.Time:
                    {
                        var currentFormat = string.IsNullOrEmpty(datetimeFormat) ? DataTableSchema.DefaultTimeFormat : datetimeFormatEscapeCharRE.Replace(datetimeFormat, m => $@"\{m.Value}");
                        if (!DateTime.TryParseExact(defaultValue, currentFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {currentFormat} is valid value format.");
                    }
                    break;

                case ColumnType.DateTime:
                    {
                        var currentFormat = string.IsNullOrEmpty(datetimeFormat) ? DataTableSchema.DefaultDateTimeFormat : datetimeFormatEscapeCharRE.Replace(datetimeFormat, m => $@"\{m.Value}");
                        if (!DateTime.TryParseExact(defaultValue, currentFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {currentFormat} is valid value format.");
                    }
                    break;

                case ColumnType.TimeSpan:
                    {
                        var currentFormat = string.IsNullOrEmpty(datetimeFormat) ? DataTableSchema.DefaultTimeSpanFormat : datetimeFormatEscapeCharRE.Replace(datetimeFormat, m => $@"\{m.Value}");
                        if (!TimeSpan.TryParseExact(defaultValue, currentFormat, CultureInfo.InvariantCulture, TimeSpanStyles.None, out _))
                            throw new DataTableException(sheetName, columnName, $"Invalid {type} default value ({defaultValue}). {currentFormat} is valid value format.");
                    }
                    break;
            }
        }
    }

    public sealed class DataTableException : Exception
    {
        public string Sheet { get; }
        public string Column { get; }

        public DataTableException(string sheet, string column, string message) : base($"{sheet} Sheet, {column} Column: {message}")
        {
            Sheet = sheet;
            Column = column;
        }
    }
}
