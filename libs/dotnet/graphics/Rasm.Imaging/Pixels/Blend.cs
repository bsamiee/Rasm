using System.Buffers.Binary;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Pixels;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IBlendKernel { internal static abstract Vector256<double> Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma); }

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class MatteChannel {
    public static readonly MatteChannel Lightness = new("lightness", static (row, luminance, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = row[x].W is 0f ? 0f : (float)double.Clamp(Vector256.Dot(luminance, row[x].Wide) * row[x].W, 0d, 1d);
    });
    public static readonly MatteChannel Alpha = new("alpha", static (row, _, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = float.Clamp(row[x].W, 0f, 1f);
    });

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, Vector256<double> luminance, Span<float> weights);
}

[SmartEnum<string>]
public sealed partial class MaskOperation {
    public static readonly MaskOperation Add = new("add", 0f, static (mask, key) => TensorPrimitives.Max(mask, key, mask));
    public static readonly MaskOperation Subtract = new("subtract", 1f, static (mask, key) => {
        TensorPrimitives.Subtract(mask, key, mask);
        TensorPrimitives.Max(mask, 0f, mask);
    });
    public static readonly MaskOperation Multiply = new("multiply", 1f, static (mask, key) => TensorPrimitives.Multiply(mask, key, mask));

    public float Start { get; }

    [UseDelegateFromConstructor]
    public partial void Join(Span<float> mask, ReadOnlySpan<float> key);
}

[SmartEnum<string>]
public abstract partial class BlendingMode {
    public static readonly BlendingMode Mix = new Blended<Kernels.Mix>("mix", "norm"u8);
    public static readonly BlendingMode Darken = new Blended<Kernels.Darken>("darken", "dark"u8);
    public static readonly BlendingMode Multiply = new Blended<Kernels.Multiply>("multiply", "mul "u8);
    public static readonly BlendingMode Burn = new Blended<Kernels.Burn>("burn", "idiv"u8);
    public static readonly BlendingMode Lighten = new Blended<Kernels.Lighten>("lighten", "lite"u8);
    public static readonly BlendingMode Screen = new Blended<Kernels.Screen>("screen", "scrn"u8);
    public static readonly BlendingMode Dodge = new Blended<Kernels.Dodge>("dodge", "div "u8);
    public static readonly BlendingMode Add = new Blended<Kernels.Add>("add", "lddg"u8);
    public static readonly BlendingMode Overlay = new Blended<Kernels.Overlay>("overlay", "over"u8);
    public static readonly BlendingMode SoftLight = new Blended<Kernels.SoftLight>("soft-light", "sLit"u8);
    public static readonly BlendingMode LinearLight = new Blended<Kernels.LinearLight>("linear-light", "lLit"u8);
    public static readonly BlendingMode Difference = new Blended<Kernels.Difference>("difference", "diff"u8);
    public static readonly BlendingMode Exclusion = new Blended<Kernels.Exclusion>("exclusion", "smud"u8);
    public static readonly BlendingMode Subtract = new Blended<Kernels.Subtract>("subtract", "fsub"u8);
    public static readonly BlendingMode Divide = new Blended<Kernels.Divide>("divide", "fdiv"u8);
    public static readonly BlendingMode Hue = new Blended<Kernels.Hue>("hue", "hue "u8);
    public static readonly BlendingMode Saturation = new Blended<Kernels.Saturation>("saturation", "sat "u8);
    public static readonly BlendingMode Color = new Blended<Kernels.Color>("color", "colr"u8);
    public static readonly BlendingMode Value = new Blended<Kernels.Value>("value", "lum "u8);

    public uint BlendModeKey { get; }

    public abstract void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, PixelAlphaRepresentation representation, Gamut gamut, PixelAlphaRepresentation sourceRepresentation = PixelAlphaRepresentation.Unassociated);

    public abstract void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights, Gamut gamut);

    private sealed class Blended<TKernel>(string key, ReadOnlySpan<byte> code) : BlendingMode(key, BinaryPrimitives.ReadUInt32BigEndian(code)) where TKernel : struct, IBlendKernel {
        public override void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, PixelAlphaRepresentation representation, Gamut gamut, PixelAlphaRepresentation sourceRepresentation = PixelAlphaRepresentation.Unassociated) {
            Vector256<double> luma = gamut.Luminance;
            for (int x = 0; x < backdrop.Length; x++) {
                if (weights[x] is 0f || (sourceRepresentation is PixelAlphaRepresentation.Unassociated && layer[x].W is 0f) || (sourceRepresentation is PixelAlphaRepresentation.Associated && layer[x] == Vector4.Zero))
                    continue;
                double sourceAlpha = sourceRepresentation is PixelAlphaRepresentation.None ? 1d : layer[x].W;
                double covered = weights[x] * sourceAlpha;
                Vector4 under = backdrop[x];
                Vector256<double> source = Vector256.Create(layer[x].X, layer[x].Y, layer[x].Z, 0d);
                double alpha = representation is PixelAlphaRepresentation.None ? 1d : under.W;
                Vector256<double> color = Vector256.Create(under.X, under.Y, under.Z, 0d);
                Vector256<double> associated = representation is PixelAlphaRepresentation.Associated || alpha is 1d ? color
                    : alpha is 0d ? Vector256<double>.Zero : alpha * color;
                color = representation is PixelAlphaRepresentation.Associated && alpha is not (0d or 1d) ? color / alpha : color;
                Vector256<double> straight = sourceRepresentation is PixelAlphaRepresentation.Associated && sourceAlpha is not 0d ? source / sourceAlpha : source;
                Vector256<double> blended = alpha is 0d ? straight
                    : alpha is 1d ? TKernel.Blended(color, straight, luma)
                    : Vector256.Lerp(straight, TKernel.Blended(color, straight, luma), Vector256.Create(alpha));
                double outputAlpha = covered is 1d || alpha is 1d ? 1d : alpha is 0d ? covered : covered + ((1d - covered) * alpha);
                Vector256<double> output = sourceAlpha is 0d ? associated + (weights[x] * source)
                    : covered is 1d ? blended : (covered * blended) + ((1d - covered) * associated);
                output = representation is PixelAlphaRepresentation.Unassociated && outputAlpha is not (0d or 1d) ? output / outputAlpha : output;
                backdrop[x] = new Vector4((float)output[0], (float)output[1], (float)output[2], (float)outputAlpha);
            }
        }

        public override void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights, Gamut gamut) {
            Vector256<double> luma = gamut.Luminance;
            for (int x = 0; x < graded.Length; x++) {
                if (weights[x] is 0f)
                    graded[x] = input[x];
                else {
                    Vector256<double> original = Vector256.Create(input[x].X, input[x].Y, input[x].Z, 0d);
                    Vector256<double> applied = TKernel.Blended(original, Vector256.Create(graded[x].X, graded[x].Y, graded[x].Z, 0d), luma);
                    Vector256<double> output = weights[x] is 1f ? applied : Vector256.Lerp(original, applied, Vector256.Create((double)weights[x]));
                    graded[x] = new Vector4((float)output[0], (float)output[1], (float)output[2], input[x].W);
                }
            }
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Coverage {
    public abstract void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights);

    public sealed record Keyed(Func<Vector4, float> Membership, bool Inverted) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = Membership(row[x]);
            if (Inverted)
                TensorPrimitives.Subtract(1f, weights, weights);
        }
    }

    public sealed record Matte(MatteChannel Channel, PixelFrame Frame, Gamut Gamut, bool Inverted) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            Channel.Read(Frame.Row(line).Slice((int)(column - Frame.Origin.X), row.Length), Gamut.Luminance, weights);
            if (Inverted)
                TensorPrimitives.Subtract(1f, weights, weights);
        }
    }

    public sealed record Luma(Gamut Gamut, BinRange Bins) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            Vector256<double> luminance = Gamut.Luminance;
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f && Bins.Holds(Vector256.Dot(luminance, row[x].Wide)) ? 1f : 0f;
        }
    }

    public sealed record Clipped(ClipLevels Levels, ClipEnd End) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f && End.Clips(row[x], Levels) ? 1f : 0f;
        }
    }
}

public sealed record Mask(MaskOperation Operation, Coverage Key);

file sealed record Blending(BlendingMode Mode, float Amount, Seq<Mask> Masks, Gamut Gamut) {
    public void Weigh(ReadOnlySpan<Vector4> input, long column, long line, Span<float> weights) {
        Blend.Weights(Masks, input, column, line, weights);
        TensorPrimitives.Multiply(weights, Amount, weights);
    }

    public void Graded(Span<Vector4> row, Action<Span<Vector4>> stage, Action<ReadOnlySpan<Vector4>, Span<float>> weigh) {
        using SpanOwner<Vector4> input = SpanOwner<Vector4>.Allocate(row.Length);
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        row.CopyTo(input.Span);
        stage(row);
        weigh(input.Span, weights.Span);
        Mode.Mixed(input.Span, row, weights.Span, Gamut);
    }

    public void Layered(Span<Vector4> row, ReadOnlySpan<Vector4> layer, long column, long line, PixelAlphaRepresentation representation) {
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        if (representation is PixelAlphaRepresentation.Unassociated || Masks.IsEmpty)
            Weigh(row, column, line, weights.Span);
        else {
            using SpanOwner<Vector4> input = SpanOwner<Vector4>.Allocate(row.Length);
            for (int x = 0; x < row.Length; x++)
                input.Span[x] = representation is PixelAlphaRepresentation.None ? new Vector4(row[x].AsVector3(), 1f)
                    : row[x].W is 0f or 1f ? row[x] : new Vector4(row[x].AsVector3() / row[x].W, row[x].W);
            Weigh(input.Span, column, line, weights.Span);
        }
        Mode.Composite(row, layer, weights.Span, representation, Gamut);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Blend {
    internal static void Associated(ReadOnlySpan<Vector4> row, Span<Vector4> into) =>
        PixelOperations<RgbaVector>.Instance.ToVector4(Configuration.Default, MemoryMarshal.Cast<Vector4, RgbaVector>(row), into, PixelConversionModifiers.Premultiply);

    extension(PixelFrame frame) {
        internal Mat Premultiplied() {
            Mat associated = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            Associated(MemoryMarshal.Cast<float, Vector4>(frame.Block), associated.GetSpan<Vector4>());
            return associated;
        }

        public unsafe Fin<Unit> Mutate(Action<Image<RgbaVector>> edit) {
            Span2D<float> samples = new((void*)frame.Address, frame.Size.Width * frame.Size.Height, 4, 0);
            using SpanOwner<float> alpha = SpanOwner<float>.Allocate(samples.Height);
            samples.GetColumn(3).CopyTo(alpha.Span);
            samples.GetColumn(3).Fill(1f);
            Fin<Unit> edited = Try.lift(() => {
                using Image<RgbaVector> image = Image.WrapMemory<RgbaVector>((void*)frame.Address, frame.Block.Length * sizeof(float), frame.Size.Width, frame.Size.Height);
                edit(image);
            }).Run();
            alpha.Span.CopyTo(samples.GetColumn(3));
            return edited;
        }
    }

    public static Option<PixelPass> Mixed(PixelPass stage, BlendingMode mode, float mix, Seq<Mask> masks, Gamut gamut) =>
        mix is 0f
            ? None
            : mode == BlendingMode.Mix && mix is 1f && masks.IsEmpty
            ? stage
            : stage.Switch<Blending, PixelPass>(
                new Blending(mode, mix, masks, gamut),
                uniform: static (blending, uniform) => blending.Masks.IsEmpty
                    ? new PixelPass.Uniform(row => blending.Graded(row, uniform.Row, (_, weights) => weights.Fill(blending.Amount)))
                    : new PixelPass.Pointwise((row, column, line) => blending.Graded(row, uniform.Row, (input, weights) => blending.Weigh(input, column, line, weights))),
                pointwise: static (blending, pointwise) => new PixelPass.Pointwise((row, column, line) =>
                    blending.Graded(row, graded => pointwise.Row(graded, column, line), (input, weights) => blending.Weigh(input, column, line, weights))),
                frame: static (blending, frame) => new PixelPass.Frame((target, progress) => target.Copy() switch {
                    var staged => frame.Kernel(staged, progress).Bind(_ => new PixelPass.Pointwise((row, column, line) =>
                        blending.Graded(row, graded => staged.Row(line).CopyTo(graded), (input, weights) => blending.Weigh(input, column, line, weights))).Run(target, new Progress<int>())),
                }));

    public static PixelFrame Composite(PixelFrame @base, PixelFrame layer, BlendingMode mode, float opacity, Seq<Mask> masks, PixelAlphaRepresentation representation, Gamut gamut) {
        PixelFrame composite = @base.Copy();
        Blending blending = new(mode, opacity, masks, gamut);
        if (opacity is not 0f)
            _ = new PixelPass.Pointwise((row, column, line) => blending.Layered(row, layer.View.Span.GetRowSpan(composite.RowIndex(line)), column, line, representation))
                .Run(composite, new Progress<int>());
        return composite;
    }

    public static void Weights(Seq<Mask> masks, ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
        weights.Fill(masks.Head.Map(static mask => mask.Operation.Start).IfNone(1f));
        using SpanOwner<float> key = SpanOwner<float>.Allocate(weights.Length);
        foreach (Mask mask in masks) {
            mask.Key.Weights(row, column, line, key.Span);
            mask.Operation.Join(weights, key.Span);
        }
    }
}

file static class Kernels {
    internal readonly struct Mix : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => over; }
    internal readonly struct Darken : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.Min(under, over); }
    internal readonly struct Multiply : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => under * over; }
    internal readonly struct Burn : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.ConditionalSelect(Vector256.LessThanOrEqual(over, Vector256<double>.Zero), Vector256<double>.Zero, Vector256.Clamp(Vector256<double>.One - ((Vector256<double>.One - under) / over), Vector256<double>.Zero, Vector256<double>.One)); }
    internal readonly struct Lighten : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.Max(under, over); }
    internal readonly struct Screen : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256<double>.One - ((Vector256<double>.One - over) * (Vector256<double>.One - under)); }
    internal readonly struct Dodge : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => (Vector256<double>.One - over) switch { var rest => Vector256.ConditionalSelect(Vector256.IsZero(under), under, Vector256.ConditionalSelect(Vector256.LessThanOrEqual(rest, Vector256<double>.Zero), Vector256<double>.One, Vector256.Min(under / rest, Vector256<double>.One))) }; }
    internal readonly struct Add : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => under + over; }
    internal readonly struct Overlay : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.ConditionalSelect(Vector256.LessThan(under, Vector256.Create(0.5d)), 2d * under * over, Vector256<double>.One - (2d * (Vector256<double>.One - over) * (Vector256<double>.One - under))); }
    internal readonly struct SoftLight : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => (under * under) + (2d * under * over * (Vector256<double>.One - under)); }
    internal readonly struct LinearLight : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => under + (2d * over) - Vector256<double>.One; }
    internal readonly struct Difference : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.Abs(under - over); }
    internal readonly struct Exclusion : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.Max(under + over - (2d * under * over), Vector256<double>.Zero); }
    internal readonly struct Subtract : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => under - over; }
    internal readonly struct Divide : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Vector256.ConditionalSelect(Vector256.IsZero(over), under, under / over); }
    internal readonly struct Hue : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Lit(Saturated(over, Chroma(under)), Vector256.Dot(under, luma), luma); }
    internal readonly struct Saturation : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Lit(Saturated(under, Chroma(over)), Vector256.Dot(under, luma), luma); }
    internal readonly struct Color : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Lit(over, Vector256.Dot(under, luma), luma); }
    internal readonly struct Value : IBlendKernel { static Vector256<double> IBlendKernel.Blended(Vector256<double> under, Vector256<double> over, Vector256<double> luma) => Lit(under, Vector256.Dot(over, luma), luma); }

    private static (double Least, double Most) Extremes(Vector256<double> color) => (double.Min(double.Min(color[0], color[1]), color[2]), double.Max(double.Max(color[0], color[1]), color[2]));

    private static double Chroma(Vector256<double> color) => double.Max(double.Max(color[0], color[1]), color[2]) - double.Min(double.Min(color[0], color[1]), color[2]);

    private static Vector256<double> Saturated(Vector256<double> color, double chroma) => Extremes(color) switch { (var least, var most) when most > least => (color - Vector256.Create(least)) * (chroma / (most - least)), _ => Vector256<double>.Zero };

    private static Vector256<double> Lit(Vector256<double> color, double level, Vector256<double> luma) =>
        (color + Vector256.Create(level - Vector256.Dot(color, luma))) switch {
            var shifted => (Vector256.Dot(shifted, luma), Extremes(shifted)) switch {
                (var lum, (var least, var most)) when least < 0d || most > 1d => Vector256.Lerp(Vector256.Create(lum), shifted,
                    Vector256.Create((least < 0d ? lum / (lum - least) : 1d) * (most > 1d ? (1d - lum) / (most - lum) : 1d))),
                _ => shifted,
            },
        };
}
