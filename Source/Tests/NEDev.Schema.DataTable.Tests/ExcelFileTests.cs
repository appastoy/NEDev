using NUnit.Framework;
using System.Threading.Tasks;

namespace NEDev.Schema.DataTable.Tests
{
    public class ExcelFileTests
    {
        string testFilePath = "DataTableSchema.xlsx";
        byte[] bytes = System.Array.Empty<byte>();

        [OneTimeSetUp]
        public async Task SetUp()
        {
            bytes = await FileAsync.ReadAllBytesAsync(testFilePath);
        }

        [Test]
        public void LoadFromExcelFile()
        {
            DataTableSchema.LoadFromExcelFile(testFilePath);
        }

        [Test]
        public void LoadFromExcelFileBytes()
        {
            DataTableSchema.LoadFromExcelFile(bytes);
        }

        [Test]
        public async Task LoadFromExcelFileAsync()
        {
            await DataTableSchema.LoadFromExcelFileAsync(testFilePath);
        }
    }
}