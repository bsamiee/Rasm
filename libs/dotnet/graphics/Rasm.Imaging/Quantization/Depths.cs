using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Quantization;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Quantizer {
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

    internal void Coded(ReadOnlySpan<float> values, Span<float> codes) {
        TensorPrimitives.Multiply(values, MaxCode, codes);
        TensorPrimitives.Add(codes, 0.5f, codes);
        TensorPrimitives.Clamp(codes, 0f, MaxCode, codes);
        TensorPrimitives.Truncate(codes, codes);
    }

    internal void Quantize(Span<Vector4> row, long column, long line, Func<long, long, float> draw) {
        for (int x = 0; x < row.Length; x++) {
            Vector4 pixel = row[x];
            Vector4 code = MaxCode * pixel;
            row[x] = Vector4.Create((Round(code + Offset(draw(column + x, line), code)) / MaxCode).AsVector3(), pixel.W);
        }
    }
}

[SmartEnum<string>]
public abstract partial class OutputDepth {
    public static readonly OutputDepth UInt8 = new Integer<byte>(8);
    public static readonly OutputDepth UInt10 = new Integer<ushort>(10);
    public static readonly OutputDepth UInt12 = new Integer<ushort>(12);
    public static readonly OutputDepth UInt14 = new Integer<ushort>(14);
    public static readonly OutputDepth UInt16 = new Integer<ushort>(16);
    public static readonly OutputDepth Float16 = new Floating<Half>();
    public static readonly OutputDepth Float32 = new Floating<float>();

    public int Bits { get; }

    public int SampleBytes { get; }

    public abstract Option<Quantizer> Levels { get; }

    public static Seq<OutputDepth> Where<T>(Func<OutputDepth, Option<T>> table) => toSeq(Items).Filter(depth => table(depth).IsSome);

    internal static Fin<T> Sampled<T>(OutputDepth depth, Func<OutputDepth, Option<T>> table, [CallerArgumentExpression(nameof(table))] string format = "") =>
        table(depth).ToFin(new ImagingFault($"{format} {depth} samples beside {Where(table)}", ImagingReason.Unsupported));

    public long RowBytes(int width, bool alpha) => (((long)width * (alpha ? 4 : 3) * Bits) + 7) / 8;

    internal Fin<Unit> Buffered(int width, bool alpha) => ImagingFault.Unless(RowBytes(width, alpha) <= System.Array.MaxLength, ImagingReason.Oversize, (width, alpha));

    public void Encode(ReadOnlySpan<Vector4> row, bool alpha, Span<byte> destination) {
        int channels = alpha ? 4 : 3;
        using SpanOwner<float> samples = SpanOwner<float>.Allocate(channels * row.Length);
        MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4)[.., ..channels].CopyTo(samples.Span.AsSpan2D(row.Length, channels));
        Store(samples.Span, destination);
    }

    public void ToBigEndian(Span<byte> words) {
        (int Bytes, bool Whole) sample = (SampleBytes, Bits == 8 * SampleBytes);
        if (sample is (2, true))
            BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(words), MemoryMarshal.Cast<byte, ushort>(words));
        else if (sample is (4, true))
            BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, uint>(words), MemoryMarshal.Cast<byte, uint>(words));
    }

    private protected abstract void Store(Span<float> samples, Span<byte> destination);

    private sealed class Integer<T>(int bits) : OutputDepth(string.Create(CultureInfo.InvariantCulture, $"uint{bits}"), bits, Unsafe.SizeOf<T>()) where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> {
        private readonly Quantizer levels = new(bits);

        public override Option<Quantizer> Levels => Some(levels);

        private protected override void Store(Span<float> samples, Span<byte> destination) {
            levels.Coded(samples, samples);
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

    private sealed class Floating<T>() : OutputDepth(string.Create(CultureInfo.InvariantCulture, $"float{8 * Unsafe.SizeOf<T>()}"), 8 * Unsafe.SizeOf<T>(), Unsafe.SizeOf<T>())
        where T : unmanaged, IFloatingPointIeee754<T> {
        public override Option<Quantizer> Levels => None;

        private protected override void Store(Span<float> samples, Span<byte> destination) =>
            TensorPrimitives.ConvertSaturating(samples, MemoryMarshal.Cast<byte, T>(destination));
    }
}
