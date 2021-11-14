using NUnit.Framework;
using System.Threading.Tasks;

namespace NEDev.Schema.DataTable.Tests
{
    public class JsonTests
    {
        string testFilePath = "DataTableSchema.xlsx";

        [Test]
        public async Task ToJson_FromJson()
        {
            var bytes = await FileAsync.ReadAllBytesAsync(testFilePath);
            var schema = DataTableSchema.LoadFromExcelFile(bytes);
            
            var json = schema.ToJson();
            var loadedSchema = DataTableSchema.FromJson(json);
            Assert.That(json, Is.Not.Empty);
            Assert.That(loadedSchema, Is.Not.Null);
            Assert.That(schema, Is.EqualTo(loadedSchema));
        }
    }
}
