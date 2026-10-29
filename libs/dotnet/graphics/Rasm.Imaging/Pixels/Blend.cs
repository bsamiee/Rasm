using System.Buffers.Binary;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IBlend {
    public static abstract Vector4 Blended(Vector4 backdrop, Vector4 source);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class MaskOperation {
    public static readonly MaskOperation Add = new("add", 0f, static (mask, key) => TensorPrimitives.Max(mask, key, mask));
    public static readonly MaskOperation Subtract = new("subtract", 1f, static (mask, key) => {
        TensorPrimitives.Subtract(mask, key, mask);
        TensorPrimitives.Max(mask, 0f, mask);
    });
    public static readonly MaskOperation Multiply = new("multiply", 1f, static (mask, key) => TensorPrimitives.Multiply(mask, key, mask));

    public float Start { get; }
    public Action<Span<float>, ReadOnlySpan<float>> Join { get; }
}

public sealed class Mask(MaskOperation operation, Action<ReadOnlySpan<Vector4>, Span<float>> key, Option<PixelFrame> matte = default, bool inverted = false) {
    public static Action<ReadOnlySpan<Vector4>, Span<float>> Alpha { get; } = static (row, weights) => {
        MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4).GetColumn(3).CopyTo(weights);
        TensorPrimitives.Max(weights, 0f, weights);
        TensorPrimitives.Min(weights, 1f, weights);
    };

    public MaskOperation Operation { get; } = operation;
    public Action<ReadOnlySpan<Vector4>, Span<float>> Key { get; } = key;
    public Option<PixelFrame> Matte { get; } = matte;
    public bool Inverted { get; } = inverted;

    public static Action<ReadOnlySpan<Vector4>, Span<float>> Lightness(Vector3 luminance) => (row, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = float.Clamp(Vector3.Dot(luminance, row[x].AsVector3()) * row[x].W, 0f, 1f);
    };

    public static Action<ReadOnlySpan<Vector4>, Span<float>> Luminance(Vector3 luminance, LevelRange levels) => (row, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = row[x].W > 0f && Vector3.Dot(luminance, row[x].AsVector3()) is var level && level >= levels.Low && level <= levels.High ? 1f : 0f;
    };

    public static Action<ReadOnlySpan<Vector4>, Span<float>> Clipping(LevelRange levels) => (row, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = row[x].W > 0f && levels.Clips(row[x]) ? 1f : 0f;
    };

    public static void Weights(Seq<Mask> masks, ReadOnlySpan<Vector4> row, int x, int y, Span<float> weights) {
        weights.Fill(masks.Head.Map(static mask => mask.Operation.Start).IfNone(1f));
        using SpanOwner<float> key = SpanOwner<float>.Allocate(weights.Length);
        foreach (Mask mask in masks) {
            mask.Key(mask.Matte.Case is PixelFrame frame ? frame.View.Span.GetRowSpan(y - frame.Origin.Y).Slice(x - frame.Origin.X, row.Length) : row, key.Span);
            if (mask.Inverted)
                TensorPrimitives.Subtract(1f, key.Span, key.Span);
            mask.Operation.Join(weights, key.Span);
        }
    }
}

[SmartEnum<string>]
public abstract partial class BlendingMode {
    public static readonly BlendingMode Normal = new Kernel<Kernels.Normal>("normal", "norm"u8);
    public static readonly BlendingMode Multiply = new Kernel<Kernels.Multiply>("multiply", "mul "u8);
    public static readonly BlendingMode Screen = new Kernel<Kernels.Screen>("screen", "scrn"u8);
    public static readonly BlendingMode Overlay = new Kernel<Kernels.Overlay>("overlay", "over"u8);
    public static readonly BlendingMode Darken = new Kernel<Kernels.Darken>("darken", "dark"u8);
    public static readonly BlendingMode Lighten = new Kernel<Kernels.Lighten>("lighten", "lite"u8);
    public static readonly BlendingMode ColorDodge = new Kernel<Kernels.ColorDodge>("color-dodge", "div "u8);
    public static readonly BlendingMode ColorBurn = new Kernel<Kernels.ColorBurn>("color-burn", "idiv"u8);
    public static readonly BlendingMode HardLight = new Kernel<Kernels.HardLight>("hard-light", "hLit"u8);
    public static readonly BlendingMode SoftLight = new Kernel<Kernels.SoftLight>("soft-light", "sLit"u8);
    public static readonly BlendingMode Difference = new Kernel<Kernels.Difference>("difference", "diff"u8);
    public static readonly BlendingMode Exclusion = new Kernel<Kernels.Exclusion>("exclusion", "smud"u8);
    public static readonly BlendingMode Hue = new Kernel<Kernels.Hue>("hue", "hue "u8);
    public static readonly BlendingMode Saturation = new Kernel<Kernels.Saturation>("saturation", "sat "u8);
    public static readonly BlendingMode Color = new Kernel<Kernels.Color>("color", "colr"u8);
    public static readonly BlendingMode Luminosity = new Kernel<Kernels.Luminosity>("luminosity", "lum "u8);
    public static readonly BlendingMode Add = new Kernel<Kernels.Add>("add", "lddg"u8);
    public static readonly BlendingMode Subtract = new Kernel<Kernels.Subtract>("subtract", "fsub"u8);
    public static readonly BlendingMode Divide = new Kernel<Kernels.Divide>("divide", "fdiv"u8);
    public static readonly BlendingMode LinearLight = new Kernel<Kernels.LinearLight>("linear-light", "lLit"u8);

    public uint BlendModeKey { get; }

    public abstract void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> source, ReadOnlySpan<float> amount, PixelAlphaCompositionMode composition = PixelAlphaCompositionMode.SrcOver);

    public abstract void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights);

    public PixelFrame Composite(PixelFrame backdrop, PixelFrame layer, float opacity, Seq<Mask> masks, PixelAlphaRepresentation representation, PixelAlphaCompositionMode composition = PixelAlphaCompositionMode.SrcOver) {
        PixelFrame composite = backdrop.Copy();
        composite.Rows((row, x, y) => {
            using SpanOwner<Vector4> source = SpanOwner<Vector4>.Allocate(row.Length, AllocationMode.Clear);
            using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
            System.Drawing.Rectangle covered = System.Drawing.Rectangle.Intersect(layer.Window, new(x, y, row.Length, 1));
            if (!covered.IsEmpty)
                layer.View.Span.GetRowSpan(y - layer.Origin.Y).Slice(covered.Left - layer.Origin.X, covered.Width).CopyTo(source.Span[(covered.Left - x)..]);
            Straightened(row, representation);
            Straightened(source.Span, representation);
            Mask.Weights(masks, row, x, y, weights.Span);
            TensorPrimitives.Multiply(weights.Span, opacity, weights.Span);
            weights.Span[..int.Clamp(covered.Left - x, 0, row.Length)].Clear();
            weights.Span[int.Clamp(covered.Right - x, 0, row.Length)..].Clear();
            Composite(row, source.Span, weights.Span, composition);
            if (representation is PixelAlphaRepresentation.Associated)
                PixelFrame.Associated(row, row);
        });
        return composite;
    }

    public Option<PixelPass> Mixed(PixelPass stage, float mix, Seq<Mask> masks) =>
        mix is 0f ? None
        : this == Normal && mix is 1f && masks.IsEmpty ? stage
        : stage.Switch<(BlendingMode Mode, float Mix, Seq<Mask> Masks), PixelPass>((this, mix, masks),
            point: static (state, point) => state.Masks.IsEmpty
                ? new PixelPass.Point(row => state.Mode.Graded(row, point.Row, state.Mix, state.Masks, 0, 0))
                : new PixelPass.Positional((row, x, y) => state.Mode.Graded(row, point.Row, state.Mix, state.Masks, x, y)),
            positional: static (state, positional) => new PixelPass.Positional((row, x, y) => state.Mode.Graded(row, graded => positional.Row(graded, x, y), state.Mix, state.Masks, x, y)),
            frame: static (state, frame) => new PixelPass.Frame((target, progress) => target.Copy() switch {
                var staged => frame.Kernel(staged, progress).Bind(_ =>
                    new PixelPass.Positional((row, x, y) => state.Mode.Graded(row, graded => staged.View.Span.GetRowSpan(y - staged.Origin.Y).CopyTo(graded), state.Mix, state.Masks, x, y)).Run(target, progress)),
            }));

    private void Graded(Span<Vector4> row, Action<Span<Vector4>> stage, float mix, Seq<Mask> masks, int x, int y) {
        using SpanOwner<Vector4> input = SpanOwner<Vector4>.Allocate(row.Length);
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        row.CopyTo(input.Span);
        Mask.Weights(masks, row, x, y, weights.Span);
        TensorPrimitives.Multiply(weights.Span, mix, weights.Span);
        stage(row);
        Mixed(input.Span, row, weights.Span);
    }

    private static void Straightened(Span<Vector4> row, PixelAlphaRepresentation representation) {
        if (representation is PixelAlphaRepresentation.Associated)
            PixelFrame.Unassociated(row);
        else if (representation is PixelAlphaRepresentation.None)
            MemoryMarshal.Cast<Vector4, float>(row).AsSpan2D(row.Length, 4).GetColumn(3).Fill(1f);
    }

    private sealed class Kernel<T>(string key, ReadOnlySpan<byte> code) : BlendingMode(key, BinaryPrimitives.ReadUInt32BigEndian(code)) where T : struct, IBlend {
        public override void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> source, ReadOnlySpan<float> amount, PixelAlphaCompositionMode composition = PixelAlphaCompositionMode.SrcOver) {
            Vector4 factors = Factors(composition);
            for (int x = 0; x < backdrop.Length; x++) {
                (Vector4 under, Vector4 over) = (backdrop[x], source[x]);
                (float below, float above) = (under.W, over.W * amount[x]);
                (float sourceShare, float backdropShare) = (above * (factors.X + (factors.Y * below)), below * (factors.Z + (factors.W * above)));
                float alpha = float.Min(sourceShare + backdropShare, 1f);
                Vector4 color = ((sourceShare * Vector4.Lerp(over, T.Blended(under, over), below)) + (backdropShare * under)) / float.Max(alpha, float.Epsilon);
                backdrop[x] = new(color.AsVector3(), alpha);
            }
        }

        public override void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights) {
            for (int x = 0; x < graded.Length; x++)
                graded[x] = new(Vector3.Lerp(input[x].AsVector3(), T.Blended(input[x], graded[x]).AsVector3(), weights[x]), input[x].W);
        }
    }

    private static Vector4 Factors(PixelAlphaCompositionMode composition) =>
            composition switch {
                PixelAlphaCompositionMode.Src => new(1f, 0f, 0f, 0f),
                PixelAlphaCompositionMode.SrcOver => new(1f, 0f, 1f, -1f),
                PixelAlphaCompositionMode.SrcIn => new(0f, 1f, 0f, 0f),
                PixelAlphaCompositionMode.SrcOut => new(1f, -1f, 0f, 0f),
                PixelAlphaCompositionMode.SrcAtop => new(0f, 1f, 1f, -1f),
                PixelAlphaCompositionMode.Dest => new(0f, 0f, 1f, 0f),
                PixelAlphaCompositionMode.DestOver => new(1f, -1f, 1f, 0f),
                PixelAlphaCompositionMode.DestIn => new(0f, 0f, 0f, 1f),
                PixelAlphaCompositionMode.DestOut => new(0f, 0f, 1f, -1f),
                PixelAlphaCompositionMode.DestAtop => new(1f, -1f, 0f, 1f),
                PixelAlphaCompositionMode.Xor => new(1f, -1f, 1f, -1f),
                PixelAlphaCompositionMode.Plus => new(1f, 0f, 1f, 0f),
                _ => Vector4.Zero,
            };
}

file static class Kernels {
    private static readonly Vector4 Luma = new(0.3f, 0.59f, 0.11f, 0f);
    private static readonly Vector4 Half = new(0.5f);

    internal readonly struct Normal : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => source; }
    internal readonly struct Multiply : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => backdrop * source; }
    internal readonly struct Screen : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Screened(backdrop, source); }
    internal readonly struct Overlay : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Hard(source, backdrop); }
    internal readonly struct Darken : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Vector4.Min(backdrop, source); }
    internal readonly struct Lighten : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Vector4.Max(backdrop, source); }
    internal readonly struct ColorDodge : IBlend {
        public static Vector4 Blended(Vector4 backdrop, Vector4 source) =>
            Vector4.ConditionalSelect(Vector4.IsZero(backdrop), Vector4.Zero, Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(source, Vector4.One), Vector4.One, Vector4.Min(Vector4.One, backdrop / (Vector4.One - source))));
    }
    internal readonly struct ColorBurn : IBlend {
        public static Vector4 Blended(Vector4 backdrop, Vector4 source) =>
            Vector4.ConditionalSelect(Vector4.GreaterThanOrEqual(backdrop, Vector4.One), Vector4.One, Vector4.ConditionalSelect(Vector4.LessThanOrEqual(source, Vector4.Zero), Vector4.Zero, Vector4.One - Vector4.Min(Vector4.One, (Vector4.One - backdrop) / source)));
    }
    internal readonly struct HardLight : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Hard(backdrop, source); }
    internal readonly struct SoftLight : IBlend {
        public static Vector4 Blended(Vector4 backdrop, Vector4 source) =>
            Vector4.ConditionalSelect(Vector4.LessThanOrEqual(source, Half),
                backdrop - ((Vector4.One - (2f * source)) * backdrop * (Vector4.One - backdrop)),
                backdrop + (((2f * source) - Vector4.One) * (Vector4.ConditionalSelect(Vector4.LessThanOrEqual(backdrop, Half * Half), ((((16f * backdrop) - new Vector4(12f)) * backdrop) + new Vector4(4f)) * backdrop, Vector4.SquareRoot(backdrop)) - backdrop)));
    }
    internal readonly struct Difference : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Vector4.Abs(backdrop - source); }
    internal readonly struct Exclusion : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => backdrop + source - (2f * backdrop * source); }
    internal readonly struct Hue : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => SetLum(SetSat(source, Sat(backdrop)), Vector4.Dot(backdrop, Luma)); }
    internal readonly struct Saturation : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => SetLum(SetSat(backdrop, Sat(source)), Vector4.Dot(backdrop, Luma)); }
    internal readonly struct Color : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => SetLum(source, Vector4.Dot(backdrop, Luma)); }
    internal readonly struct Luminosity : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => SetLum(backdrop, Vector4.Dot(source, Luma)); }
    internal readonly struct Add : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => backdrop + source; }
    internal readonly struct Subtract : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => backdrop - source; }
    internal readonly struct Divide : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => Vector4.ConditionalSelect(Vector4.IsZero(source), backdrop, backdrop / source); }
    internal readonly struct LinearLight : IBlend { public static Vector4 Blended(Vector4 backdrop, Vector4 source) => backdrop + (2f * source) - Vector4.One; }

    private static Vector4 Screened(Vector4 backdrop, Vector4 source) => backdrop + source - (backdrop * source);

    private static Vector4 Hard(Vector4 backdrop, Vector4 source) =>
        Vector4.ConditionalSelect(Vector4.LessThanOrEqual(source, Half), 2f * backdrop * source, Screened(backdrop, (2f * source) - Vector4.One));

    private static float Least(Vector4 color) => float.Min(color.X, float.Min(color.Y, color.Z));

    private static float Most(Vector4 color) => float.Max(color.X, float.Max(color.Y, color.Z));

    private static float Sat(Vector4 color) => Most(color) - Least(color);

    private static Vector4 SetSat(Vector4 color, float saturation) =>
        Sat(color) is > 0f and var range ? (color - new Vector4(Least(color))) * (saturation / range) : Vector4.Zero;

    private static Vector4 SetLum(Vector4 color, float level) =>
        (color + new Vector4(level - Vector4.Dot(color, Luma))) switch {
            var shifted => (Least(shifted), Most(shifted)) switch {
                var (least, most) => (least < 0f ? level / (level - least) : 1f) switch {
                    var lifted => Vector4.Lerp(new Vector4(level), shifted, level + (lifted * (most - level)) > 1f ? (1f - level) / (most - level) : lifted),
                },
            },
        };
}
