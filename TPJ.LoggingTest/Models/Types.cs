namespace TPJ.LoggingTest.Models;

class Types
{
    public string String { get; set; }
    public string StringNull { get; set; }
    public bool Bool { get; set; }
    public bool? BoolNull { get; set; }
    public byte Byte { get; set; }
    public byte? ByteNull { get; set; }
    public char Char { get; set; }
    public char? CharNull { get; set; }
    public decimal Decimal { get; set; }
    public decimal? DecimalNull { get; set; }
    public double Double { get; set; }
    public double? DoubleNull { get; set; }
    public float Float { get; set; }
    public float? FloatNull { get; set; }
    public int Int { get; set; }
    public int? IntNull { get; set; }
    public long Long { get; set; }
    public long? LongNull { get; set; }
    public sbyte Sbyte { get; set; }
    public sbyte? SbyteNull { get; set; }
    public short Short { get; set; }
    public short? ShortNull { get; set; }
    public uint Uint { get; set; }
    public uint? UintNull { get; set; }
    public ulong Ulong { get; set; }
    public ulong? UlongNull { get; set; }
    public ushort Ushort { get; set; }
    public ushort? UshortNull { get; set; }

    public Types NestedClass { get; set; }

    public IEnumerable<Types> ListOfTypes { get; set; }

    public Types()
    {
        Bool = true;
        BoolNull = null;
        Byte = 255;
        ByteNull = null;
        Char = 'A';
        CharNull = null;
        Decimal = 1.12m;
        DecimalNull = null;
        Double = 2.23;
        DoubleNull = null;
        Float = 151.3245f;
        FloatNull = null;
        Int = 2147483647;
        IntNull = null;
        Long = 9223372036854775807;
        LongNull = null;
        Sbyte = 127;
        SbyteNull = null;
        Short = 32767;
        ShortNull = null;
        String = "Test";
        StringNull = null;
        Uint = 4294967290;
        UintNull = null;
        Ulong = 9223372036854775808;
        UlongNull = null;
        Ushort = 65535;
        UshortNull = null;
    }
}
