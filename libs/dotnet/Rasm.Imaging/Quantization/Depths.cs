using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Quantization;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Quantizer {
    internal Quantizer(int bits) => MaxCode = (1 << bits) - 1;

    public float MaxCode { get; }

    public ClipLevels Clipping => new(0.5f / MaxCode, (MaxCode - 0.5f) / MaxCode);

    public Vector4 Round(Vector4 code) =>
        Vector4.Truncate(Vector4.Clamp(code + Vector4.Create(0.5f), Vector4.Zero, Vector4.Create(MaxCode)));

    internal Vector4 Offset(float draw, Vector4 code) {
        float signed = (2f * draw) - 1f;
        float rectangular = draw - 0.5f;
        return Vector4.Create(rectangular)
            + (Vector4.Clamp(2f * Vector4.Min(code, Vector4.Create(MaxCode) - code), Vector4.Zero, Vector4.One) * (MathF.CopySign(1f - MathF.Sqrt(1f - MathF.Abs(signed)), signed) - rectangular));
    }

    internal void Quantize<TField>(Span<Vector4> row, int column, int line) where TField : IOffsetField {
        for (int x = 0; x < row.Length; x++) {
            Vector4 pixel = row[x];
            Vector4 code = MaxCode * pixel;
            row[x] = Vector4.Create((Round(code + Offset(TField.Draw(column + x, line), code)) / MaxCode).AsVector3(), pixel.W);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidQuantization>]
public abstract partial class OutputDepth {
    public static readonly OutputDepth UInt8 = new Integer<byte>(8);
    public static readonly OutputDepth UInt10 = new Integer<ushort>(10);
    public static readonly OutputDepth UInt12 = new Integer<ushort>(12);
    public static readonly OutputDepth UInt14 = new Integer<ushort>(14);
    public static readonly OutputDepth UInt16 = new Integer<ushort>(16);
    public static readonly OutputDepth Float16 = new Floating<Half>();
    public static readonly OutputDepth Float32 = new Floating<float>();

    public int Bits { get; }

    internal int SampleBytes { get; }

    public abstract Option<Quantizer> Levels { get; }

    public int RowBytes(int width, bool alpha) => ((width * (alpha ? 4 : 3) * Bits) + 7) / 8;

    internal void Encode(ReadOnlySpan<Vector4> row, bool alpha, Span<byte> destination) {
        int lanes = alpha ? 4 : 3;
        using SpanOwner<float> samples = SpanOwner<float>.Allocate((lanes * row.Length) + 4 - lanes);
        for (int x = 0; x < row.Length; x++)
            Sample(row[x]).CopyTo(samples.Span[(lanes * x)..]);
        Store(samples.Span[..(lanes * row.Length)], destination);
    }

    internal void ToBigEndian(Span<byte> words) {
        for (int at = 0; at < words.Length; at += SampleBytes)
            words.Slice(at, SampleBytes).Reverse();
    }

    private protected abstract Vector4 Sample(Vector4 pixel);

    private protected abstract void Store(ReadOnlySpan<float> samples, Span<byte> destination);

    private sealed class Integer<T>(int bits) : OutputDepth(string.Create(CultureInfo.InvariantCulture, $"uint{bits}"), bits, Unsafe.SizeOf<T>()) where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> {
        private readonly Quantizer levels = new(bits);

        public override Option<Quantizer> Levels => Some(levels);

        private protected override Vector4 Sample(Vector4 pixel) => levels.Round(levels.MaxCode * pixel);

        private protected override void Store(ReadOnlySpan<float> samples, Span<byte> destination) {
            using SpanOwner<T> words = SpanOwner<T>.Allocate(samples.Length);
            TensorPrimitives.ConvertSaturating(samples, words.Span);
            if (Bits == 8 * SampleBytes) {
                MemoryMarshal.AsBytes(words.Span).CopyTo(destination);
                return;
            }
            (ulong held, int filled, int at) = (0UL, 0, 0);
            foreach (T word in words.Span) {
                held = (held << Bits) | ulong.CreateTruncating(word);
                filled += Bits;
                BinaryPrimitives.WriteUInt16BigEndian(destination[at..], (ushort)(((held << 16) >> filled) & ushort.MaxValue));
                (at, filled) = (at + (filled >> 3), filled & 7);
            }
            if (filled > 0)
                destination[at] = (byte)((held << (8 - filled)) & byte.MaxValue);
        }
    }

    private sealed class Floating<T> : OutputDepth where T : unmanaged, IFloatingPointIeee754<T> {
        public Floating() : this(Unsafe.SizeOf<T>()) { }

        private Floating(int bytes) : base(string.Create(CultureInfo.InvariantCulture, $"float{8 * bytes}"), 8 * bytes, bytes) { }

        public override Option<Quantizer> Levels => None;

        private protected override Vector4 Sample(Vector4 pixel) => pixel;

        private protected override void Store(ReadOnlySpan<float> samples, Span<byte> destination) =>
            TensorPrimitives.ConvertSaturating(samples, MemoryMarshal.Cast<byte, T>(destination));
    }
}
