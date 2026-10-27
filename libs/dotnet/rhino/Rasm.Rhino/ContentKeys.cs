using System.Buffers.Binary;
using System.IO.Hashing;
using System.Numerics;
using System.Text;

namespace Rasm.RhinoCommon;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<UInt128>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public sealed partial class ContentKey {
    public static ContentKey Of<TOwner>(Func<XxHash128, XxHash128> fields) =>
        new(fields(new XxHash128().Id(typeof(TOwner).GUID)).GetCurrentHashAsUInt128());

    public Guid Id {
        get {
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128BigEndian(bytes, _value);
            return new Guid(bytes, bigEndian: true);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKeys {
    extension(XxHash128 accumulator) {
        public XxHash128 Integer<T>(T value) where T : unmanaged, IBinaryInteger<T> {
            Span<byte> field = stackalloc byte[value.GetByteCount()];
            _ = value.WriteLittleEndian(field);
            accumulator.Append(field);
            return accumulator;
        }

        public XxHash128 Bytes(ReadOnlySpan<byte> value) {
            accumulator.Integer(value.Length).Append(value);
            return accumulator;
        }

        public XxHash128 Text(string value) => accumulator.Bytes(Encoding.UTF8.GetBytes(value));

        public XxHash128 Id(Guid value) => accumulator.Integer(BinaryPrimitives.ReadUInt128BigEndian(value.ToByteArray(bigEndian: true)));

        public XxHash128 Number(double value) => accumulator.Integer(BitConverter.DoubleToInt64Bits(double.IsNaN(value) ? double.NaN : value is 0d ? 0d : value));

        public XxHash128 Rows<T>(Seq<T> rows, Func<XxHash128, T, XxHash128> row) => rows.Fold(accumulator.Integer(rows.Count), row);
    }
}
