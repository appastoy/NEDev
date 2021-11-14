using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NEDev.Schema.DataTable
{
    internal static class ExcelFileLoader
    {
        public static DataTableSchema LoadFromExcelFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            return LoadFromExcelFile(bytes);
        }

        public static async Task<DataTableSchema> LoadFromExcelFileAsync(string filePath)
        {
            var bytes = await FileAsync.ReadAllBytesAsync(filePath);
            return LoadFromExcelFile(bytes);
        }

        public static DataTableSchema LoadFromExcelFile(byte[] bytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var stream = new MemoryStream(bytes))
            using (var package = new ExcelPackage(stream))
            {
                var sheetSchemas = LoadSheets(package.Workbook.Worksheets);
                return new DataTableSchema(sheetSchemas);
            }
        }

        static SheetSchema[] LoadSheets(ExcelWorksheets sheets)
        {
            return sheets.AsParallel()
                         .WithDegreeOfParallelism(Math.Min(sheets.Count, Environment.ProcessorCount))
                         .Select(LoadSheet)
                         .Where(s => s != null)
                         .ToArray();
        }

        static SheetSchema LoadSheet(ExcelWorksheet sheet)
        {
            var sheetName = sheet.Name.Trim();
            if (!DataTableSchema.IsValidName(sheetName))
                return null;

            var dimension = sheet.Dimension;
            var start = dimension.Start;
            var end = dimension.End;
            var cells = sheet.Cells;

            var columns = new List<ColumnSchema>();
            for (int i = start.Column; i <= end.Column; i++)
            {
                if (TryLoadColumn(sheetName, cells, i, out var columnSchema))
                    columns.Add(columnSchema);
            }

            return new SheetSchema(sheet.Name, columns.ToArray());
        }

        static bool TryLoadColumn(string sheetName, ExcelRange cells, int columnIndex, out ColumnSchema columnSchema)
        {
            var columnComment = cells[1, columnIndex]?.Value?.ToString()?.Trim() ?? string.Empty;
            var columnType = cells[2, columnIndex]?.Value?.ToString()?.Trim() ?? string.Empty;
            var columnName = cells[3, columnIndex]?.Value?.ToString()?.Trim() ?? string.Empty;
            return ColumnSchemaParser.TryParse(sheetName, columnName, columnType, columnComment, out columnSchema);
        }
    }
}
