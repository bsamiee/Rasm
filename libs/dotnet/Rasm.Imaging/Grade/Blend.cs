using System.Buffers.Binary;
using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IBlendKernel {
    public static abstract Vector3 Apply(Vector3 a, Vector3 b);
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
            weights[x] = float.Clamp(Vector3.Dot(luminance, row[x].AsVector3()) * row[x].W, 0f, 1f);
    });
    public static readonly MatteChannel Alpha = new("alpha", static (row, _, weights) => {
        for (int x = 0; x < row.Length; x++)
            weights[x] = float.Clamp(row[x].W, 0f, 1f);
    });

    [UseDelegateFromConstructor]
    public partial void Read(ReadOnlySpan<Vector4> row, Vector3 luminance, Span<float> weights);
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

file readonly record struct Hsv(float Hue, float Saturation, float Value) {
    public static Hsv From(Vector3 color) =>
        (color.Y < color.Z ? (Green: color.Z, Blue: color.Y, Offset: -1f) : (Green: color.Y, Blue: color.Z, Offset: 0f)) switch {
            var (green, blue, offset) when color.X < green => Of(green, color.X, blue, (-2f / 6f) - offset, float.Min(color.X, blue)),
            var (green, blue, offset) => Of(color.X, green, blue, offset, blue),
        };

    public Vector3 ToRgb() =>
        (((Vector3.Clamp(new Vector3(float.Abs((Hue * 6f) - 3f) - 1f, 2f - float.Abs((Hue * 6f) - 2f), 2f - float.Abs((Hue * 6f) - 4f)), Vector3.Zero, Vector3.One) - Vector3.One) * Saturation) + Vector3.One) * Value;

    private static Hsv Of(float red, float green, float blue, float offset, float least) =>
        new(float.Abs(offset + ((green - blue) / ((6f * (red - least)) + 1e-20f))), (red - least) / (red + 1e-20f), red);
}

file readonly struct MixKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => b;
}

file readonly struct DarkenKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.Min(a, b);
}

file readonly struct MultiplyKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => a * b;
}

file readonly struct BurnKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        Vector3.ConditionalSelect(Vector3.LessThanOrEqual(b, Vector3.Zero), Vector3.Zero, Vector3.Clamp(Vector3.One - ((Vector3.One - a) / b), Vector3.Zero, Vector3.One));
}

file readonly struct LightenKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.Max(a, b);
}

file readonly struct ScreenKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.One - ((Vector3.One - b) * (Vector3.One - a));
}

file readonly struct DodgeKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        (Vector3.One - b) switch {
            var t => Vector3.ConditionalSelect(
                Vector3.Equals(a, Vector3.Zero),
                a,
                Vector3.ConditionalSelect(Vector3.LessThanOrEqual(t, Vector3.Zero), Vector3.One, Vector3.Min(a / t, Vector3.One))),
        };
}

file readonly struct AddKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => a + b;
}

file readonly struct OverlayKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        Vector3.ConditionalSelect(Vector3.LessThan(a, new Vector3(0.5f)), 2f * a * b, Vector3.One - (2f * (Vector3.One - b) * (Vector3.One - a)));
}

file readonly struct SoftLightKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => (a * a) + (2f * a * b * (Vector3.One - a));
}

file readonly struct LinearLightKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => a + (2f * b) - Vector3.One;
}

file readonly struct DifferenceKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.Abs(a - b);
}

file readonly struct ExclusionKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.Max(a + b - (2f * a * b), Vector3.Zero);
}

file readonly struct SubtractKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => a - b;
}

file readonly struct DivideKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => Vector3.ConditionalSelect(Vector3.Equals(b, Vector3.Zero), a, a / b);
}

file readonly struct HueKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        Hsv.From(b) switch {
            { Saturation: 0f } => a,
            var layer => (Hsv.From(a) with { Hue = layer.Hue }).ToRgb(),
        };
}

file readonly struct SaturationKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        Hsv.From(a) switch {
            { Saturation: 0f } => a,
            var @base => (@base with { Saturation = Hsv.From(b).Saturation }).ToRgb(),
        };
}

file readonly struct ColorKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) =>
        Hsv.From(b) switch {
            { Saturation: 0f } => a,
            var layer => (Hsv.From(a) with { Hue = layer.Hue, Saturation = layer.Saturation }).ToRgb(),
        };
}

file readonly struct ValueKernel : IBlendKernel {
    public static Vector3 Apply(Vector3 a, Vector3 b) => (Hsv.From(a) with { Value = Hsv.From(b).Value }).ToRgb();
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

    public abstract void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights);

    public abstract void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights);

    private sealed class Mode<TKernel> : BlendingMode where TKernel : IBlendKernel {
        public Mode(string key, ReadOnlySpan<byte> code) : base(key, BinaryPrimitives.ReadUInt32BigEndian(code)) { }

        public override void Composite(Span<Vector4> backdrop, ReadOnlySpan<Vector4> layer, ReadOnlySpan<float> weights) {
            for (int x = 0; x < backdrop.Length; x++) {
                Vector4 under = backdrop[x];
                Vector3 source = layer[x].AsVector3();
                float covered = weights[x] * layer[x].W;
                Vector3 blended = Vector3.Lerp(source, TKernel.Apply(under.W > 0f ? under.AsVector3() / under.W : Vector3.Zero, source), under.W);
                backdrop[x] = new Vector4((covered * blended) + ((1f - covered) * under.AsVector3()), covered + ((1f - covered) * under.W));
            }
        }

        public override void Mixed(ReadOnlySpan<Vector4> input, Span<Vector4> graded, ReadOnlySpan<float> weights) {
            for (int x = 0; x < graded.Length; x++)
                graded[x] = new Vector4(Vector3.Lerp(input[x].AsVector3(), TKernel.Apply(input[x].AsVector3(), graded[x].AsVector3()), weights[x]), input[x].W);
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Coverage {
    public abstract void Weights(ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights);

    public sealed record Qualified(HueRange Range, bool Inverted) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = Range.Membership(Hsy.From(row[x]));
            if (Inverted)
                TensorPrimitives.Subtract(1f, weights, weights);
        }
    }

    public sealed record Matte(MatteChannel Channel, PixelFrame Frame, Gamut Gamut, bool Inverted) : Coverage {
        private readonly Vector3 luminance = Gamut.Luminance;

        public override void Weights(ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights) {
            Channel.Read(Frame.Row(line).Slice(column - Frame.Origin.X, row.Length), luminance, weights);
            if (Inverted)
                TensorPrimitives.Subtract(1f, weights, weights);
        }
    }

    public sealed record Luma(Gamut Gamut, BinRange Bins) : Coverage {
        private readonly Vector3 luminance = Gamut.Luminance;

        public override void Weights(ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f && Bins.Holds(Vector3.Dot(luminance, row[x].AsVector3())) ? 1f : 0f;
        }
    }

    public sealed record Clipped(ClipLevels Levels, ClipEnd End) : Coverage {
        public override void Weights(ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights) {
            for (int x = 0; x < row.Length; x++)
                weights[x] = row[x].W > 0f
                    && ((End.Above && float.Max(row[x].X, float.Max(row[x].Y, row[x].Z)) >= Levels.High)
                        || (End.Below && float.Min(row[x].X, float.Min(row[x].Y, row[x].Z)) < Levels.Low)) ? 1f : 0f;
        }
    }
}

public sealed record Mask(MaskOperation Operation, Coverage Key);

file readonly record struct Blending(BlendingMode Mode, float Amount, Seq<Mask> Masks, PixelExtent Extent) {
    public void Weigh(ReadOnlySpan<Vector4> input, int column, int line, Span<float> weights) {
        Blend.Weights(Masks, input, column, line, Extent, weights);
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

file readonly struct LayerAction(Memory2D<Vector4> rows, Memory2D<Vector4> layer, Blending blending, PixelFrame frame) : IAction {
    public void Invoke(int i) {
        Span<Vector4> row = rows.Span.GetRowSpan(i);
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        blending.Weigh(row, frame.Origin.X, frame.Line(i), weights.Span);
        blending.Mode.Composite(row, layer.Span.GetRowSpan(i), weights.Span);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Blend {
    public static PixelPass Mixed(PixelPass stage, BlendingMode mode, Mix mix, Seq<Mask> masks, PixelExtent extent) =>
        mode == BlendingMode.Mix && mix == Mix.Full && masks.IsEmpty
            ? stage
            : stage.Switch<Blending, PixelPass>(
                new Blending(mode, mix, masks, extent),
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

    public static PixelFrame Composite(PixelFrame @base, PixelFrame layer, BlendingMode mode, Mix opacity, Seq<Mask> masks) =>
        new(@base.Origin, @base.Size, @base.Extent, block => {
            @base.Block.CopyTo(block);
            ParallelHelper.For(0, @base.Size.Height, new LayerAction(
                block.AsMemory().Cast<float, Vector4>().AsMemory2D(@base.Size.Height, @base.Size.Width), layer.View, new Blending(mode, opacity, masks, @base.Extent), @base));
        });

    public static void Weights(Seq<Mask> masks, ReadOnlySpan<Vector4> row, int column, int line, PixelExtent frame, Span<float> weights) {
        weights.Fill(masks.Head.Map(static mask => mask.Operation.Start).IfNone(1f));
        using SpanOwner<float> key = SpanOwner<float>.Allocate(weights.Length);
        foreach (Mask mask in masks) {
            mask.Key.Weights(row, column, line, frame, key.Span);
            mask.Operation.Join(weights, key.Span);
        }
    }
}
