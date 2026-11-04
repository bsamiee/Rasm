using System.Buffers.Binary;
using System.IO.Hashing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace Rasm.Rhino;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKeys {
    // --- [KEYS]
    public static UInt128 Of<TOwner>(Func<XxHash128, XxHash128> fields) =>
        fields(new XxHash128().Id(typeof(TOwner).GUID)).GetCurrentHashAsUInt128();

    extension(UInt128 value) {
        public Guid Id {
            get {
                Span<byte> bytes = stackalloc byte[Unsafe.SizeOf<Guid>()];
                BinaryPrimitives.WriteUInt128BigEndian(bytes, value);
                return new Guid(bytes, bigEndian: true);
            }
        }
    }

    // --- [FIELDS]
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

        public XxHash128 Text(string value) {
            using SpanOwner<byte> field = SpanOwner<byte>.Allocate(Encoding.UTF8.GetByteCount(value));
            _ = Encoding.UTF8.GetBytes(value, field.Span);
            return accumulator.Bytes(field.Span);
        }

        public XxHash128 Id(Guid value) {
            Span<byte> field = stackalloc byte[Unsafe.SizeOf<Guid>()];
            _ = value.TryWriteBytes(field, bigEndian: true, out _);
            return accumulator.Integer(BinaryPrimitives.ReadUInt128BigEndian(field));
        }

        public XxHash128 Number(double value) => accumulator.Integer(BitConverter.DoubleToInt64Bits(double.IsNaN(value) ? double.NaN : value is 0d ? 0d : value));

        public XxHash128 Rows<T>(Seq<T> rows, Func<XxHash128, T, XxHash128> row) => rows.Fold(accumulator.Integer(rows.Count), row);
    }
}
