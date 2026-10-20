using System.Buffers.Binary;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using SixLabors.ImageSharp.PixelFormats;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IBlendKernel {
    public static abstract Vector256<double> Apply(Vector256<double> a, Vector256<double> b);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MatteChannel {
    public static readonly MatteChannel Lightness = new("lightness", static (row, luminance, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = row[x].W is 0f ? 0f : (float)double.Clamp(LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance) * row[x].W, 0d, 1d);
    });
    public static readonly MatteChannel Alpha = new("alpha", static (row, _, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = float.Clamp(row[x].W, 0f, 1f);
    });

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, (double X, double Y, double Z) luminance, Span<float> weights);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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

file readonly struct MixKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => b;
}

file readonly struct DarkenKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.Min(a, b);
}

file readonly struct MultiplyKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => a * b;
}

file readonly struct BurnKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        Vector256.ConditionalSelect(Vector256.LessThanOrEqual(b, Vector256<double>.Zero), Vector256<double>.Zero, Vector256.Clamp(Vector256<double>.One - ((Vector256<double>.One - a) / b), Vector256<double>.Zero, Vector256<double>.One));
}

file readonly struct LightenKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.Max(a, b);
}

file readonly struct ScreenKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256<double>.One - ((Vector256<double>.One - b) * (Vector256<double>.One - a));
}

file readonly struct DodgeKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        (Vector256<double>.One - b) switch {
            var t => Vector256.ConditionalSelect(
                Vector256.IsZero(a),
                a,
                Vector256.ConditionalSelect(Vector256.LessThanOrEqual(t, Vector256<double>.Zero), Vector256<double>.One, Vector256.Min(a / t, Vector256<double>.One))),
        };
}

file readonly struct AddKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => a + b;
}

file readonly struct OverlayKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        Vector256.ConditionalSelect(Vector256.LessThan(a, Vector256.Create(0.5d)), 2d * a * b, Vector256<double>.One - (2d * (Vector256<double>.One - b) * (Vector256<double>.One - a)));
}

file readonly struct SoftLightKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => (a * a) + (2d * a * b * (Vector256<double>.One - a));
}

file readonly struct LinearLightKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => a + (2d * b) - Vector256<double>.One;
}

file readonly struct DifferenceKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.Abs(a - b);
}

file readonly struct ExclusionKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.Max(a + b - (2d * a * b), Vector256<double>.Zero);
}

file readonly struct SubtractKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => a - b;
}

file readonly struct DivideKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.ConditionalSelect(Vector256.IsZero(b), a, a / b);
}

file static class Shade {
    public static Hsb Of(Vector256<double> rgb) => new Unicolour(ColourSpace.Rgb, (rgb[0], rgb[1], rgb[2])).Hsb;

    public static Vector256<double> Rgb(double hue, double saturation, double brightness) =>
        new Unicolour(ColourSpace.Hsb, (hue, saturation, brightness)).Rgb switch {
            var rgb => Vector256.Create(rgb.R, rgb.G, rgb.B, 0d),
        };
}

file readonly struct HueKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        (Shade.Of(a), Shade.Of(b)) switch {
            (_, { S: 0d }) => a,
            var (under, layer) => Shade.Rgb(layer.H, under.S, under.B),
        };
}

file readonly struct SaturationKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        (Shade.Of(a), Shade.Of(b)) switch {
            ( { S: 0d }, _) => a,
            var (under, layer) => Shade.Rgb(under.H, layer.S, under.B),
        };
}

file readonly struct ColorKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        (Shade.Of(a), Shade.Of(b)) switch {
            (_, { S: 0d }) => a,
            var (under, layer) => Shade.Rgb(layer.H, layer.S, under.B),
        };
}

file readonly struct ValueKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        (Shade.Of(a), Shade.Of(b)) switch {
            var (under, layer) => Shade.Rgb(under.H, under.S, layer.B),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class BlendingMode : ICaptioned {
    public static readonly BlendingMode Mix = new Mode<MixKernel>("mix", "norm"u8, "Mix");
    public static readonly BlendingMode Darken = new Mode<DarkenKernel>("darken", "dark"u8, "Darken");
    public static readonly BlendingMode Multiply = new Mode<MultiplyKernel>("multiply", "mul "u8, "Multiply");
    public static readonly BlendingMode Burn = new Mode<BurnKernel>("burn", "idiv"u8, "Color burn");
    public static readonly BlendingMode Lighten = new Mode<LightenKernel>("lighten", "lite"u8, "Lighten");
    public static readonly BlendingMode Screen = new Mode<ScreenKernel>("screen", "scrn"u8, "Screen");
    public static readonly BlendingMode Dodge = new Mode<DodgeKernel>("dodge", "div "u8, "Color dodge");
    public static readonly BlendingMode Add = new Mode<AddKernel>("add", "lddg"u8, "Add");
    public static readonly BlendingMode Overlay = new Mode<OverlayKernel>("overlay", "over"u8, "Overlay");
    public static readonly BlendingMode SoftLight = new Mode<SoftLightKernel>("soft-light", "sLit"u8, "Soft light");
    public static readonly BlendingMode LinearLight = new Mode<LinearLightKernel>("linear-light", "lLit"u8, "Linear light");
    public static readonly BlendingMode Difference = new Mode<DifferenceKernel>("difference", "diff"u8, "Difference");
    public static readonly BlendingMode Exclusion = new Mode<ExclusionKernel>("exclusion", "smud"u8, "Exclusion");
    public static readonly BlendingMode Subtract = new Mode<SubtractKernel>("subtract", "fsub"u8, "Subtract");
    public static readonly BlendingMode Divide = new Mode<DivideKernel>("divide", "fdiv"u8, "Divide");
    public static readonly BlendingMode Hue = new Mode<HueKernel>("hue", "hue "u8, "Hue");
    public static readonly BlendingMode Saturation = new Mode<SaturationKernel>("saturation", "sat "u8, "Saturation");
    public static readonly BlendingMode Color = new Mode<ColorKernel>("color", "colr"u8, "Color");
    public static readonly BlendingMode Value = new Mode<ValueKernel>("value", "lum "u8, "Value");

    public uint BlendModeKey { get; }
    public string Caption { get; }

    public abstract void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, PixelAlphaRepresentation representation, PixelAlphaRepresentation sourceRepresentation = PixelAlphaRepresentation.Unassociated);

    public abstract void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights);

    private sealed class Mode<TKernel> : BlendingMode where TKernel : IBlendKernel {
        public Mode(string key, ReadOnlySpan<byte> code, string caption) : base(key, BinaryPrimitives.ReadUInt32BigEndian(code), caption) { }

        public override void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, PixelAlphaRepresentation representation, PixelAlphaRepresentation sourceRepresentation = PixelAlphaRepresentation.Unassociated) {
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
                Vector256<double> blended = alpha is 0d || this == Mix ? straight
                    : alpha is 1d ? TKernel.Apply(color, straight)
                    : Vector256.Lerp(straight, TKernel.Apply(color, straight), Vector256.Create(alpha));
                double outputAlpha = covered is 1d || alpha is 1d ? 1d : alpha is 0d ? covered : covered + ((1d - covered) * alpha);
                Vector256<double> output = sourceAlpha is 0d ? associated + (weights[x] * source)
                    : covered is 1d ? blended : (covered * blended) + ((1d - covered) * associated);
                output = representation is PixelAlphaRepresentation.Unassociated && outputAlpha is not (0d or 1d) ? output / outputAlpha : output;
                backdrop[x] = new Vector4((float)output[0], (float)output[1], (float)output[2], (float)outputAlpha);
            }
        }

        public override void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights) {
            for (int x = 0; x < graded.Length; x++) {
                if (weights[x] is 0f)
                    graded[x] = input[x];
                else {
                    Vector256<double> original = Vector256.Create(input[x].X, input[x].Y, input[x].Z, 0d);
                    Vector256<double> applied = TKernel.Apply(original, Vector256.Create(graded[x].X, graded[x].Y, graded[x].Z, 0d));
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

    public sealed record Qualified(HueRange Range, bool Inverted) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = Range.Membership(Hsy.From(row[x]));
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
            (double X, double Y, double Z) luminance = Gamut.Luminance;
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f && Bins.Holds(LuminanceSamples.Luminance(row[x].X, row[x].Y, row[x].Z, luminance)) ? 1f : 0f;
        }
    }

    public sealed record Clipped(ClipLevels Levels, ClipEnd End) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f
                    && ((End.Above && float.Max(row[x].X, float.Max(row[x].Y, row[x].Z)) >= Levels.High)
                        || (End.Below && float.Min(row[x].X, float.Min(row[x].Y, row[x].Z)) < Levels.Low)) ? 1f : 0f;
        }
    }
}

public sealed record Mask(MaskOperation Operation, Coverage Key);

file readonly record struct Blending(BlendingMode Mode, float Amount, Seq<Mask> Masks) {
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
        Mode.Mixed(input.Span, row, weights.Span);
    }
}

file readonly struct LayerAction(Memory2D<Vector4> rows, Memory2D<Vector4> layer, Blending blending, PixelFrame frame, PixelAlphaRepresentation representation) : IAction {
    public void Invoke(int i) {
        Span<Vector4> row = rows.Span.GetRowSpan(i);
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        if (representation is PixelAlphaRepresentation.Unassociated || blending.Masks.IsEmpty)
            blending.Weigh(row, frame.Origin.X, frame.Line(i), weights.Span);
        else {
            using SpanOwner<Vector4> input = SpanOwner<Vector4>.Allocate(row.Length);
            for (int x = 0; x < row.Length; x++)
                input.Span[x] = representation is PixelAlphaRepresentation.None ? new Vector4(row[x].AsVector3(), 1f)
                    : row[x].W is 0f or 1f ? row[x] : new Vector4(row[x].AsVector3() / row[x].W, row[x].W);
            blending.Weigh(input.Span, frame.Origin.X, frame.Line(i), weights.Span);
        }
        blending.Mode.Composite(row, layer.Span.GetRowSpan(i), weights.Span, representation);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Blend {
    public static Option<PixelPass> Mixed(PixelPass stage, BlendingMode mode, AxisFraction mix, Seq<Mask> masks) =>
        mix == AxisFraction.MinValue
            ? None
            : mode == BlendingMode.Mix && mix == AxisFraction.MaxValue && masks.IsEmpty
            ? stage
            : stage.Switch<Blending, PixelPass>(
                new Blending(mode, mix, masks),
                color: static (blending, color) => blending.Masks.IsEmpty
                    ? new PixelPass.Color(row => blending.Graded(row, color.Row, (_, weights) => weights.Fill(blending.Amount)))
                    : new PixelPass.Pointwise((row, column, line) => blending.Graded(row, color.Row, (input, weights) => blending.Weigh(input, column, line, weights))),
                pointwise: static (blending, pointwise) => new PixelPass.Pointwise((row, column, line) =>
                    blending.Graded(row, graded => pointwise.Row(graded, column, line), (input, weights) => blending.Weigh(input, column, line, weights))),
                frame: static (blending, frame) => new PixelPass.Frame((target, progress) => target.Copy() switch {
                    var staged => frame.Kernel(staged, progress).Bind(_ => new PixelPass.Pointwise((row, column, line) =>
                        blending.Graded(row, graded => staged.Row(line).CopyTo(graded), (input, weights) => blending.Weigh(input, column, line, weights))).Run(target, new Progress<int>())),
                }));

    public static PixelFrame Composite(PixelFrame @base, PixelFrame layer, BlendingMode mode, AxisFraction opacity, Seq<Mask> masks, PixelAlphaRepresentation representation) =>
        opacity == AxisFraction.MinValue ? @base.Copy() : new(@base.Origin, @base.Size, @base.Extent, block => {
            @base.Block.CopyTo(block);
            ParallelHelper.For(0, @base.Size.Height, new LayerAction(
                block.AsMemory().Cast<float, Vector4>().AsMemory2D(@base.Size.Height, @base.Size.Width), layer.View, new Blending(mode, opacity, masks), @base, representation));
        });

    public static void Weights(Seq<Mask> masks, ReadOnlySpan<Vector4> row, long column, long line, Span<float> weights) {
        weights.Fill(masks.Head.Map(static mask => mask.Operation.Start).IfNone(1f));
        using SpanOwner<float> key = SpanOwner<float>.Allocate(weights.Length);
        foreach (Mask mask in masks) {
            mask.Key.Weights(row, column, line, key.Span);
            mask.Operation.Join(weights, key.Span);
        }
    }
}
