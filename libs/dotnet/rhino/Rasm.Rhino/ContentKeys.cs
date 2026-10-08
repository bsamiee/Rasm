using System.Buffers.Binary;
using System.IO.Hashing;
using System.Numerics;
using System.Text;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public enum KeyDomain {
    Definition = 0,
    Artifact = 1,
    HistorySlot = 2,
    Scene = 3,
    QueueEntry = 4,
    RenderMeshes = 5,
    Tile = 6,
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentKeys {
    public static UInt128 Of(KeyDomain domain, Func<XxHash128, XxHash128> fields) =>
        fields(new XxHash128().Integer((int)domain)).GetCurrentHashAsUInt128();

    extension(XxHash128 accumulator) {
        public XxHash128 Integer<T>(T value) where T : unmanaged, IBinaryInteger<T> {
            Span<byte> field = stackalloc byte[value.GetByteCount()];
            _ = value.WriteLittleEndian(field);
            accumulator.Append(field);
            return accumulator;
        }

        public XxHash128 Flag(bool value) => accumulator.Integer(Convert.ToInt32(value));

        public XxHash128 Text(string value) {
            byte[] field = Encoding.UTF8.GetBytes(value);
            accumulator.Integer(field.Length).Append(field);
            return accumulator;
        }

        public XxHash128 Id(Guid value) => accumulator.Integer(BinaryPrimitives.ReadUInt128BigEndian(value.ToByteArray(bigEndian: true)));

        public XxHash128 Number(double value) => accumulator.Integer(BitConverter.DoubleToInt64Bits(double.IsNaN(value) ? double.NaN : value == 0d ? 0d : value));

        public XxHash128 Rows<T>(Seq<T> rows, Func<XxHash128, T, XxHash128> row) => rows.Fold(accumulator.Integer(rows.Count), row);
    }
}
