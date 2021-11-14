using NUnit.Framework;

namespace NEDev.Schema.DataTable.Tests
{
    public class ColumnTests
    {
        [TestCase("_")]
        [TestCase("_C")]
        [TestCase("C")]
        [TestCase("Class")]
        [TestCase(" \t\r\nClass\t\r\n ")]
        public void ValidColumnName(string name)
        {
            Assert.That(ColumnSchemaParser.TryParse("", name, "int", null, out var schema), Is.True);
            Assert.That(schema.Name, Is.EqualTo(name?.Trim() ?? string.Empty));
        }

        [TestCase("")]
        [TestCase("1_")]
        [TestCase("/class")]
        [TestCase("~class")]
        [TestCase("class~")]
        public void InvalidColumnName(string name)
        {
            Assert.That(ColumnSchemaParser.TryParse("", name, "int", null, out _), Is.False);
        }

        [Test]
        public void SingleColumnType()
        {
            foreach (ColumnType type in System.Enum.GetValues<ColumnType>())
            {
                if (type == ColumnType.Invalid ||
                    type == ColumnType.Enum ||
                    type == ColumnType.Custom)
                    continue;

                var typeVariants = new string[]
                {
                    type.ToString(),
                    type.ToString().ToLower(),
                    type.ToString().ToUpper()
                };
                foreach (var currentType in typeVariants)
                {
                    Assert.That(ColumnSchemaParser.TryParse("", "column", currentType, null, out var schema), Is.True);
                    Assert.That(schema.Type, Is.EqualTo(type));
                }
            }
        }

        [Test]
        public void ArrayColumnType()
        {
            Assert.That(ColumnSchemaParser.TryParse("", "column", "int[]", null, out var schema), Is.True);
            Assert.That(schema.Type, Is.EqualTo(ColumnType.Int32));
            Assert.That(schema.IsArray, Is.EqualTo(true));
        }

        [TestCase("int", "", false, true)]
        [TestCase("int=", "", false, true)]
        [TestCase("int=0", "0", false, true)]
        [TestCase("int = 1", "1", false, true)]
        [TestCase("int[]=2", "2", true, true)]
        [TestCase("int[] = 3|4", "3|4", true, true)]
        [TestCase("int[] = 5 | 6", "5 | 6", true, true)]
        [TestCase("int=asdf", null, false, false)]
        [TestCase("int[] = asdf", null, false, false)]
        [TestCase("int[] = 1|asdf", null, false, false)]
        public void ColumnDefaultValue(string columnType, string defaultValue, bool isArray, bool isValid)
        {
            if (isValid)
            {
                Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
                Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
                Assert.That(schema.IsArray, Is.EqualTo(isArray));
            }
            else
            {
                Assert.That(() => ColumnSchemaParser.TryParse("", "column", columnType, null, out _), Throws.Exception.TypeOf<DataTableException>());
            }
        }

        [TestCase("e:Test", "Test", false, "")]
        [TestCase("e: Test", "Test", false, "")]
        [TestCase("E:Test", "Test", false, "")]
        [TestCase("e:Test=TEST", "Test", false, "TEST")]
        [TestCase("e:Test = TEST", "Test", false, "TEST")]
        [TestCase("e:Test[]", "Test", true, "")]
        [TestCase("e:Test[]=TEST", "Test", true, "TEST")]
        [TestCase("e:Test[] = TEST|TEST2", "Test", true, "TEST|TEST2")]
        public void EnumColumnType(string columnType, string typeName, bool isArray, string defaultValue)
        {
            Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
            Assert.That(schema.Type, Is.EqualTo(ColumnType.Enum));
            Assert.That(schema.TypeName, Is.EqualTo(typeName));
            Assert.That(schema.IsArray, Is.EqualTo(isArray));
            Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
        }

        [TestCase("c:Test", "Test", false, "")]
        [TestCase("c: Test", "Test", false, "")]
        [TestCase("C:Test", "Test", false, "")]
        [TestCase("c:Test=TEST", "Test", false, "TEST")]
        [TestCase("c:Test = TEST", "Test", false, "TEST")]
        [TestCase("c:Test[]", "Test", true, "")]
        [TestCase("c:Test[]=TEST", "Test", true, "TEST")]
        [TestCase("c:Test[] = TEST|TEST2", "Test", true, "TEST|TEST2")]
        public void CustomColumnType(string columnType, string typeName, bool isArray, string defaultValue)
        {
            Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
            Assert.That(schema.Type, Is.EqualTo(ColumnType.Custom));
            Assert.That(schema.TypeName, Is.EqualTo(typeName));
            Assert.That(schema.IsArray, Is.EqualTo(isArray));
            Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
        }

        [TestCase("Date = 2021-11-14", false, "", "2021-11-14", true)]
        [TestCase("Date = asdf", false, "", "", false)]
        [TestCase("Date@yyyy/MM", false, "yyyy/MM", "", true)]
        [TestCase("Date @ yyyy/MM", false, "yyyy/MM", "", true)]
        [TestCase("Date@yyyy/MM = 2021/12", false, "yyyy/MM", "2021/12", true)]
        [TestCase("Date@yyyy/MM = asdf", false, "", "", false)]
        [TestCase("Date[]@yyyy/MM", true, "yyyy/MM", "", true)]
        [TestCase("Date[] @ yyyy/MM", true, "yyyy/MM", "", true)]
        [TestCase("Date[]@yyyy/MM = 2021/12", true, "yyyy/MM", "2021/12", true)]
        [TestCase("Date[] @ yyyy/MM = 2021/12|2022/11", true, "yyyy/MM", "2021/12|2022/11", true)]
        [TestCase("Date[]@yyyy/MM = 2021/12 | 2022/11", true, "yyyy/MM", "2021/12 | 2022/11", true)]
        [TestCase("Date[]@yyyy/MM = 2021/12|asdf", true, "", "", false)]
        public void DateColumnType(string columnType, bool isArray, string datetimeFormat, string defaultValue, bool isValid)
        {
            if (isValid)
            {
                Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
                Assert.That(schema.Type, Is.EqualTo(ColumnType.Date));
                Assert.That(schema.IsArray, Is.EqualTo(isArray));
                Assert.That(schema.DateTimeFormat, Is.EqualTo(datetimeFormat));
                Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
            }
            else
            {
                Assert.That(() => ColumnSchemaParser.TryParse("", "column", columnType, null, out _), Throws.Exception.TypeOf<DataTableException>());
            }
        }

        [TestCase("Time = 23:59:59", false, "", "23:59:59", true)]
        [TestCase("Time = asdf", false, "", "", false)]
        [TestCase("Time@HH:mm", false, "HH:mm", "", true)]
        [TestCase("Time @ HH:mm", false, "HH:mm", "", true)]
        [TestCase("Time@HH:mm = 23:59", false, "HH:mm", "23:59", true)]
        [TestCase("Time@HH:mm = asdf", false, "", "", false)]
        [TestCase("Time[]@HH:mm", true, "HH:mm", "", true)]
        [TestCase("Time[] @ HH:mm", true, "HH:mm", "", true)]
        [TestCase("Time[]@HH:mm = 23:59", true, "HH:mm", "23:59", true)]
        [TestCase("Time[] @ HH:mm = 23:59|22:58", true, "HH:mm", "23:59|22:58", true)]
        [TestCase("Time[]@HH:mm = 23:59 | 22:58", true, "HH:mm", "23:59 | 22:58", true)]
        [TestCase("Time[]@HH:mm = 23:59|asdf", true, "", "", false)]
        public void TimeColumnType(string columnType, bool isArray, string datetimeFormat, string defaultValue, bool isValid)
        {
            if (isValid)
            {
                Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
                Assert.That(schema.Type, Is.EqualTo(ColumnType.Time));
                Assert.That(schema.IsArray, Is.EqualTo(isArray));
                Assert.That(schema.DateTimeFormat, Is.EqualTo(datetimeFormat));
                Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
            }
            else
            {
                Assert.That(() => ColumnSchemaParser.TryParse("", "column", columnType, null, out _), Throws.Exception.TypeOf<DataTableException>());
            }
        }

        [TestCase("DateTime = 2021-11-14 23:59:59", false, "", "2021-11-14 23:59:59", true)]
        [TestCase("DateTime = asdf", false, "", "", false)]
        [TestCase("DateTime@yyyy-HH", false, "yyyy-HH", "", true)]
        [TestCase("DateTime @ yyyy-HH", false, "yyyy-HH", "", true)]
        [TestCase("DateTime@yyyy-HH = 2021-23", false, "yyyy-HH", "2021-23", true)]
        [TestCase("DateTime@yyyy-HH = asdf", false, "", "", false)]
        [TestCase("DateTime[]@yyyy-HH", true, "yyyy-HH", "", true)]
        [TestCase("DateTime[] @ yyyy-HH", true, "yyyy-HH", "", true)]
        [TestCase("DateTime[]@yyyy-HH = 2021-23", true, "yyyy-HH", "2021-23", true)]
        [TestCase("DateTime[] @ yyyy-HH = 2021-23|2021-22", true, "yyyy-HH", "2021-23|2021-22", true)]
        [TestCase("DateTime[]@yyyy-HH = 2021-23 | 2021-22", true, "yyyy-HH", "2021-23 | 2021-22", true)]
        [TestCase("DateTime[]@yyyy-HH = 2021-23|asdf", true, "", "", false)]
        public void DateTimeColumnType(string columnType, bool isArray, string datetimeFormat, string defaultValue, bool isValid)
        {
            if (isValid)
            {
                Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
                Assert.That(schema.Type, Is.EqualTo(ColumnType.DateTime));
                Assert.That(schema.IsArray, Is.EqualTo(isArray));
                Assert.That(schema.DateTimeFormat, Is.EqualTo(datetimeFormat));
                Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
            }
            else
            {
                Assert.That(() => ColumnSchemaParser.TryParse("", "column", columnType, null, out _), Throws.Exception.TypeOf<DataTableException>());
            }
        }

        [TestCase("TimeSpan = 999.23:59:59.999", false, "", "999.23:59:59.999", true)]
        [TestCase("TimeSpan = asdf", false, "", "", false)]
        [TestCase("TimeSpan@s.fff", false, "s.fff", "", true)]
        [TestCase("TimeSpan @ s.fff", false, "s.fff", "", true)]
        [TestCase("TimeSpan@s.fff = 1.234", false, "s.fff", "1.234", true)]
        [TestCase("TimeSpan@s.fff = asdf", false, "", "", false)]
        [TestCase("TimeSpan[]@s.fff", true, "s.fff", "", true)]
        [TestCase("TimeSpan[] @ s.fff", true, "s.fff", "", true)]
        [TestCase("TimeSpan[]@s.fff = 1.234", true, "s.fff", "1.234", true)]
        [TestCase("TimeSpan[] @ s.fff = 1.234|1.234", true, "s.fff", "1.234|1.234", true)]
        [TestCase("TimeSpan[]@s.fff = 1.234 | 1.234", true, "s.fff", "1.234 | 1.234", true)]
        [TestCase("TimeSpan[]@s.fff = 1.234|asdf", true, "", "", false)]
        public void TimeSpanColumnType(string columnType, bool isArray, string datetimeFormat, string defaultValue, bool isValid)
        {
            if (isValid)
            {
                Assert.That(ColumnSchemaParser.TryParse("", "column", columnType, null, out var schema), Is.True);
                Assert.That(schema.Type, Is.EqualTo(ColumnType.TimeSpan));
                Assert.That(schema.IsArray, Is.EqualTo(isArray));
                Assert.That(schema.DateTimeFormat, Is.EqualTo(datetimeFormat));
                Assert.That(schema.DefaultValue, Is.EqualTo(defaultValue));
            }
            else
            {
                Assert.That(() => ColumnSchemaParser.TryParse("", "column", columnType, null, out _), Throws.Exception.TypeOf<DataTableException>());
            }
        }
    }
}
