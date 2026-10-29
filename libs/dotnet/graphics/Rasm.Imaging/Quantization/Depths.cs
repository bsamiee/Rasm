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
    private Quantizer(int levels) => MaxCode = levels - 1;

    public float MaxCode { get; }

    public LevelRange Clipping => new(0.5f / MaxCode, (MaxCode - 0.5f) / MaxCode);

    public static Fin<Quantizer> Validated(int levels) => ImagingFault.Unless(levels >= 2, ImagingReason.Range, levels).Map(_ => new Quantizer(levels));

    public Vector4 Round(Vector4 code) => Vector4.Truncate(Vector4.Clamp(code + new Vector4(0.5f), Vector4.Zero, new Vector4(MaxCode)));

    internal void Coded(ReadOnlySpan<float> values, Span<float> codes) {
        TensorPrimitives.Multiply(values, MaxCode, codes);
        TensorPrimitives.Add(codes, 0.5f, codes);
        TensorPrimitives.Clamp(codes, 0f, MaxCode, codes);
        TensorPrimitives.Truncate(codes, codes);
    }
}

[SmartEnum<string>]
public sealed partial class OutputDepth {
    public static readonly OutputDepth UInt8 = Integer<byte>(8);
    public static readonly OutputDepth UInt10 = Integer<ushort>(10);
    public static readonly OutputDepth UInt12 = Integer<ushort>(12);
    public static readonly OutputDepth UInt14 = Integer<ushort>(14);
    public static readonly OutputDepth UInt16 = Integer<ushort>(16);
    public static readonly OutputDepth Float16 = Floating<Half>();
    public static readonly OutputDepth Float32 = Floating<float>();

    public int Bits { get; }

    public int SampleBytes { get; }

    public Option<Quantizer> Levels { get; }

    private readonly Action<ReadOnlySpan<float>, Span<byte>> store;

    public long RowBytes(int width, bool alpha) => (((long)width * (alpha ? 4 : 3) * Bits) + 7) / 8;

    internal Fin<Unit> Buffered(int width, bool alpha) => ImagingFault.Unless(RowBytes(width, alpha) <= System.Array.MaxLength, ImagingReason.Oversize, (width, alpha));

    public void Encode(ReadOnlySpan<Vector4> row, bool alpha, Span<byte> destination) {
        int channels = alpha ? 4 : 3;
        using SpanOwner<float> samples = SpanOwner<float>.Allocate(channels * row.Length);
        MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4)[.., ..channels].CopyTo(samples.Span.AsSpan2D(row.Length, channels));
        foreach (Quantizer levels in Levels.ToSpan())
            levels.Coded(samples.Span, samples.Span);
        store(samples.Span, destination);
    }

    public void ToBigEndian(Span<byte> words) {
        if (Bits == 8 * SampleBytes && SampleBytes is 2)
            BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(words), MemoryMarshal.Cast<byte, ushort>(words));
        else if (Bits == 8 * SampleBytes && SampleBytes is 4)
            BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, uint>(words), MemoryMarshal.Cast<byte, uint>(words));
    }

    private static OutputDepth Integer<T>(int bits) where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
        new(string.Create(CultureInfo.InvariantCulture, $"uint{bits}"), bits, Unsafe.SizeOf<T>(), Quantizer.Validated(1 << bits).ToOption(),
            bits == 8 * Unsafe.SizeOf<T>() ? static (codes, destination) => TensorPrimitives.ConvertSaturating(codes, MemoryMarshal.Cast<byte, T>(destination)) : (codes, destination) => Packed(codes, destination, bits));

    private static OutputDepth Floating<T>() where T : unmanaged, IFloatingPointIeee754<T> =>
        new(string.Create(CultureInfo.InvariantCulture, $"float{8 * Unsafe.SizeOf<T>()}"), 8 * Unsafe.SizeOf<T>(), Unsafe.SizeOf<T>(), None,
            static (samples, destination) => TensorPrimitives.ConvertSaturating(samples, MemoryMarshal.Cast<byte, T>(destination)));

    private static void Packed(ReadOnlySpan<float> codes, Span<byte> destination, int bits) {
        (ulong held, int filled, int at) = (0UL, 0, 0);
        foreach (float code in codes) {
            held = (held << bits) | (ulong)code;
            filled += bits;
            BinaryPrimitives.WriteUInt16BigEndian(destination[at..], (ushort)(((held << 16) >> filled) & ushort.MaxValue));
            (at, filled) = (at + (filled >> 3), filled & 7);
        }
        if (filled > 0)
            destination[at] = (byte)((held << (8 - filled)) & byte.MaxValue);
    }
}
