using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NEDev.Schema.DataTable
{
    public interface IDataTableSchema : IEquatable<IDataTableSchema>
    {
        IReadOnlyList<ISheetSchema> Sheets { get; }
        string ToJson();
    }

    public sealed class DataTableSchema : IDataTableSchema
    {
        public static readonly string DefaultDateFormat = @"yyyy\-MM\-dd";
        public static readonly string DefaultTimeFormat = @"HH\:mm\:ss";
        public static readonly string DefaultDateTimeFormat = $"{DefaultDateFormat} {DefaultTimeFormat}";
        public static readonly string DefaultTimeSpanFormat = @"d\.hh\:mm\:ss\.FFF";

        static readonly Regex validNameRE = new Regex(@"^[_A-Z][_a-zA-Z0-9]*$", RegexOptions.Compiled | RegexOptions.Multiline);

        public static IDataTableSchema LoadFromExcelFile(string filePath) => ExcelFileLoader.LoadFromExcelFile(filePath);
        public static IDataTableSchema LoadFromExcelFile(byte[] bytes) => ExcelFileLoader.LoadFromExcelFile(bytes);
        public static async Task<IDataTableSchema> LoadFromExcelFileAsync(string filePath) => await ExcelFileLoader.LoadFromExcelFileAsync(filePath);
        public static bool IsValidName(string name) => validNameRE.IsMatch(name);
        public static IDataTableSchema FromJson(string json) => JsonConvert.DeserializeObject<DataTableSchema>(json);

        public SheetSchema[] Sheets { get; set; }

        IReadOnlyList<ISheetSchema> IDataTableSchema.Sheets => Sheets;

        public DataTableSchema()
        {

        }

        internal DataTableSchema(SheetSchema[] sheets)
        {
            Sheets = sheets ?? throw new ArgumentNullException(nameof(sheets));
        }

        public string ToJson() => JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        });

        public bool Equals(IDataTableSchema other)
        {
            return Sheets.SequenceEqual(other.Sheets);
        }
    }
}