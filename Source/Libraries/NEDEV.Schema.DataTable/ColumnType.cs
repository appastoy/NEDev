namespace NEDev.Schema.DataTable
{
    public enum ColumnType
    {
        Invalid,
        
        Bool,
        String,
        Enum,
        Custom,

        Int8,
        SByte = Int8,
        Int16,
        Short = Int16,
        Int32,
        Int = Int32,
        Int64,
        Long = Int64,

        UInt8,
        Byte = UInt8,
        UInt16,
        UShort = UInt16,
        UInt32,
        UInt = UInt32,
        UInt64,
        ULong = UInt64,

        Float32,
        Float = Float32,
        Float64,
        Double = Float64,
        Fixed128,
        Decimal = Fixed128,

        Date,
        Time,
        DateTime,
        TimeSpan
    }
}
