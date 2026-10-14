using System.Buffers.Binary;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [TYPES] ---------------------------------------------------------------------------
public enum AlphaRepresentation {
    None,
    Associated,
    Unassociated,
}

internal interface IBlendKernel {
    public static abstract Vector256<double> Apply(Vector256<double> a, Vector256<double> b);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct Mix : IMinMaxValue<Mix> {
    public static Mix MinValue { get; } = new(0f);
    public static Mix MaxValue => Full;
    public static Mix Full { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MatteChannel {
    public static readonly MatteChannel Lightness = new("lightness", static (row, luminance, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = row[x].W == 0f ? 0f : (float)double.Clamp(((luminance.X * row[x].X) + (luminance.Y * row[x].Y) + (luminance.Z * row[x].Z)) * row[x].W, 0d, 1d);
    });
    public static readonly MatteChannel Alpha = new("alpha", static (row, _, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = float.Clamp(row[x].W, 0f, 1f);
    });

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, (double X, double Y, double Z) luminance, Span<float> weights);
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
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
                Vector256.Equals(a, Vector256<double>.Zero),
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
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) => Vector256.ConditionalSelect(Vector256.Equals(b, Vector256<double>.Zero), a, a / b);
}

file readonly struct HueKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        new Unicolour(ColourSpace.Rgb, (b[0], b[1], b[2])).Hsb switch {
            { S: 0d } => a,
            var layer => new Unicolour(ColourSpace.Rgb, (a[0], a[1], a[2])).Hsb switch {
                var under => new Unicolour(ColourSpace.Hsb, (layer.H, under.S, under.B)).Rgb switch {
                    var rgb => Vector256.Create(rgb.R, rgb.G, rgb.B, 0d),
                },
            },
        };
}

file readonly struct SaturationKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        new Unicolour(ColourSpace.Rgb, (a[0], a[1], a[2])).Hsb switch {
            { S: 0d } => a,
            var under => new Unicolour(ColourSpace.Hsb, (under.H, new Unicolour(ColourSpace.Rgb, (b[0], b[1], b[2])).Hsb.S, under.B)).Rgb switch {
                var rgb => Vector256.Create(rgb.R, rgb.G, rgb.B, 0d),
            },
        };
}

file readonly struct ColorKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        new Unicolour(ColourSpace.Rgb, (b[0], b[1], b[2])).Hsb switch {
            { S: 0d } => a,
            var layer => new Unicolour(ColourSpace.Hsb, (layer.H, layer.S, new Unicolour(ColourSpace.Rgb, (a[0], a[1], a[2])).Hsb.B)).Rgb switch {
                var rgb => Vector256.Create(rgb.R, rgb.G, rgb.B, 0d),
            },
        };
}

file readonly struct ValueKernel : IBlendKernel {
    public static Vector256<double> Apply(Vector256<double> a, Vector256<double> b) =>
        new Unicolour(ColourSpace.Rgb, (a[0], a[1], a[2])).Hsb switch {
            var under => new Unicolour(ColourSpace.Hsb, (under.H, under.S, new Unicolour(ColourSpace.Rgb, (b[0], b[1], b[2])).Hsb.B)).Rgb switch {
                var rgb => Vector256.Create(rgb.R, rgb.G, rgb.B, 0d),
            },
        };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class BlendingMode {
    public static readonly BlendingMode Mix = new Mode<MixKernel>("mix", "norm"u8);
    public static readonly BlendingMode Darken = new Mode<DarkenKernel>("darken", "dark"u8);
    public static readonly BlendingMode Multiply = new Mode<MultiplyKernel>("multiply", "mul "u8);
    public static readonly BlendingMode Burn = new Mode<BurnKernel>("burn", "idiv"u8);
    public static readonly BlendingMode Lighten = new Mode<LightenKernel>("lighten", "lite"u8);
    public static readonly BlendingMode Screen = new Mode<ScreenKernel>("screen", "scrn"u8);
    public static readonly BlendingMode Dodge = new Mode<DodgeKernel>("dodge", "div "u8);
    public static readonly BlendingMode Add = new Mode<AddKernel>("add", "lddg"u8);
    public static readonly BlendingMode Overlay = new Mode<OverlayKernel>("overlay", "over"u8);
    public static readonly BlendingMode SoftLight = new Mode<SoftLightKernel>("soft-light", "sLit"u8);
    public static readonly BlendingMode LinearLight = new Mode<LinearLightKernel>("linear-light", "lLit"u8);
    public static readonly BlendingMode Difference = new Mode<DifferenceKernel>("difference", "diff"u8);
    public static readonly BlendingMode Exclusion = new Mode<ExclusionKernel>("exclusion", "smud"u8);
    public static readonly BlendingMode Subtract = new Mode<SubtractKernel>("subtract", "fsub"u8);
    public static readonly BlendingMode Divide = new Mode<DivideKernel>("divide", "fdiv"u8);
    public static readonly BlendingMode Hue = new Mode<HueKernel>("hue", "hue "u8);
    public static readonly BlendingMode Saturation = new Mode<SaturationKernel>("saturation", "sat "u8);
    public static readonly BlendingMode Color = new Mode<ColorKernel>("color", "colr"u8);
    public static readonly BlendingMode Value = new Mode<ValueKernel>("value", "lum "u8);

    public uint BlendModeKey { get; }

    public abstract void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, AlphaRepresentation representation, AlphaRepresentation sourceRepresentation = AlphaRepresentation.Unassociated);

    public abstract void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights);

    private sealed class Mode<TKernel> : BlendingMode where TKernel : IBlendKernel {
        public Mode(string key, ReadOnlySpan<byte> code) : base(key, BinaryPrimitives.ReadUInt32BigEndian(code)) { }

        public override void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights, AlphaRepresentation representation, AlphaRepresentation sourceRepresentation = AlphaRepresentation.Unassociated) {
            for (int x = 0; x < backdrop.Length; x++) {
                if (weights[x] == 0f || (sourceRepresentation == AlphaRepresentation.Unassociated && layer[x].W == 0f) || (sourceRepresentation == AlphaRepresentation.Associated && layer[x] == Vector4.Zero))
                    continue;
                double sourceAlpha = sourceRepresentation == AlphaRepresentation.None ? 1d : layer[x].W;
                double covered = weights[x] * sourceAlpha;
                Vector4 under = backdrop[x];
                Vector256<double> source = Vector256.Create(layer[x].X, layer[x].Y, layer[x].Z, 0d);
                double alpha = representation == AlphaRepresentation.None ? 1d : under.W;
                Vector256<double> color = Vector256.Create(under.X, under.Y, under.Z, 0d);
                Vector256<double> associated = representation == AlphaRepresentation.Associated || alpha == 1d ? color
                    : alpha == 0d ? Vector256<double>.Zero : alpha * color;
                color = representation == AlphaRepresentation.Associated && alpha is not (0d or 1d) ? color / alpha : color;
                Vector256<double> straight = sourceRepresentation == AlphaRepresentation.Associated && sourceAlpha != 0d ? source / sourceAlpha : source;
                Vector256<double> blended = alpha == 0d || this == Mix ? straight
                    : alpha == 1d ? TKernel.Apply(color, straight)
                    : Vector256.Lerp(straight, TKernel.Apply(color, straight), Vector256.Create(alpha));
                double outputAlpha = covered == 1d || alpha == 1d ? 1d : alpha == 0d ? covered : covered + ((1d - covered) * alpha);
                Vector256<double> output = sourceAlpha == 0d ? associated + (weights[x] * source)
                    : covered == 1d ? blended : (covered * blended) + ((1d - covered) * associated);
                output = representation == AlphaRepresentation.Unassociated && outputAlpha is not (0d or 1d) ? output / outputAlpha : output;
                backdrop[x] = new Vector4((float)output[0], (float)output[1], (float)output[2], (float)outputAlpha);
            }
        }

        public override void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights) {
            for (int x = 0; x < graded.Length; x++) {
                if (weights[x] == 0f)
                    graded[x] = input[x];
                else {
                    Vector256<double> original = Vector256.Create(input[x].X, input[x].Y, input[x].Z, 0d);
                    Vector256<double> applied = TKernel.Apply(original, Vector256.Create(graded[x].X, graded[x].Y, graded[x].Z, 0d));
                    Vector256<double> output = weights[x] == 1f ? applied : Vector256.Lerp(original, applied, Vector256.Create((double)weights[x]));
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
            (double red, double green, double blue) = Gamut.Luminance;
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f && Bins.Holds((red * row[x].X) + (green * row[x].Y) + (blue * row[x].Z)) ? 1f : 0f;
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

file readonly struct LayerAction(Memory2D<Vector4> rows, Memory2D<Vector4> layer, Blending blending, PixelFrame frame, AlphaRepresentation representation) : IAction {
    public void Invoke(int i) {
        Span<Vector4> row = rows.Span.GetRowSpan(i);
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        if (representation == AlphaRepresentation.Unassociated || blending.Masks.IsEmpty)
            blending.Weigh(row, frame.Origin.X, frame.Line(i), weights.Span);
        else {
            using SpanOwner<Vector4> input = SpanOwner<Vector4>.Allocate(row.Length);
            for (int x = 0; x < row.Length; x++)
                input.Span[x] = representation == AlphaRepresentation.None ? new Vector4(row[x].AsVector3(), 1f)
                    : row[x].W is 0f or 1f ? row[x] : new Vector4(row[x].AsVector3() / row[x].W, row[x].W);
            blending.Weigh(input.Span, frame.Origin.X, frame.Line(i), weights.Span);
        }
        blending.Mode.Composite(row, layer.Span.GetRowSpan(i), weights.Span, representation);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Blend {
    public static PixelPass Mixed(PixelPass stage, BlendingMode mode, Mix mix, Seq<Mask> masks) =>
        mix == Mix.MinValue
            ? new PixelPass.Color(static _ => { })
            : mode == BlendingMode.Mix && mix == Mix.Full && masks.IsEmpty
            ? stage
            : stage.Switch<Blending, PixelPass>(
                new Blending(mode, mix, masks),
                color: static (blending, color) => blending.Masks.IsEmpty
                    ? new PixelPass.Color(row => blending.Graded(row, color.Row, (_, weights) => weights.Fill(blending.Amount)))
                    : new PixelPass.Pointwise((row, column, line) => blending.Graded(row, color.Row, (input, weights) => blending.Weigh(input, column, line, weights))),
                pointwise: static (blending, pointwise) => new PixelPass.Pointwise((row, column, line) =>
                    blending.Graded(row, graded => pointwise.Row(graded, column, line), (input, weights) => blending.Weigh(input, column, line, weights))),
                frame: static (blending, frame) => new PixelPass.Frame((target, progress) => {
                    using MemoryOwner<Vector4> input = MemoryOwner<Vector4>.Allocate(target.Size.Width * target.Size.Height);
                    target.View.Span.CopyTo(input.Span.AsSpan2D(target.Size.Height, target.Size.Width));
                    return frame.Kernel(target, progress).Map(_ => {
                        Span2D<Vector4> rows = target.View.Span;
                        Span2D<Vector4> copy = input.Span.AsSpan2D(target.Size.Height, target.Size.Width);
                        using SpanOwner<float> weights = SpanOwner<float>.Allocate(target.Size.Width);
                        for (int i = 0; i < target.Size.Height; i++) {
                            blending.Weigh(copy.GetRowSpan(i), target.Origin.X, target.Line(i), weights.Span);
                            blending.Mode.Mixed(copy.GetRowSpan(i), rows.GetRowSpan(i), weights.Span);
                        }
                        return unit;
                    });
                }));

    public static PixelFrame Composite(PixelFrame @base, PixelFrame layer, BlendingMode mode, Mix opacity, Seq<Mask> masks, AlphaRepresentation representation) =>
        opacity == Mix.MinValue ? @base.Copy() : new(@base.Origin, @base.Size, @base.Extent, block => {
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
