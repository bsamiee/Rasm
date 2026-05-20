using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Falloff {
    public static readonly Falloff Constant = new(key: 0, weight: static (_, _) => Fin.Succ(1.0));
    public static readonly Falloff Inverse = new(key: 1, weight: static (distance, op) => distance > RhinoMath.ZeroTolerance ? Fin.Succ(1.0 / distance) : Fin.Fail<double>(op.InvalidInput()));
    public static readonly Falloff InverseSquare = new(key: 2, weight: static (distance, op) => distance > RhinoMath.ZeroTolerance ? Fin.Succ(1.0 / (distance * distance)) : Fin.Fail<double>(op.InvalidInput()));
    [UseDelegateFromConstructor] internal partial Fin<double> Weight(double distance, Op key);
}

[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static spans => 1.0 / spans.Count);
    internal Fin<VectorSpan> Combine(Seq<VectorSpan> spans, Point3d sample, Context context, Op key) =>
        spans switch {
            Seq<VectorSpan> values when !values.IsEmpty => VectorSpan.Of(
                anchor: sample,
                vector: values.Map(static span => span.Value).Fold(initialState: Vector3d.Zero, f: static (sum, vector) => sum + vector) * Scale(values),
                context: context,
                key: key),
            _ => Fin.Fail<VectorSpan>(error: key.InvalidResult()),
        };
    [UseDelegateFromConstructor] private partial double Scale(Seq<VectorSpan> spans);
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RayPolicy {
    private RayPolicy(BoundarySense sense, Option<double> length) {
        Sense = sense;
        Length = length;
    }
    public BoundarySense Sense { get; }
    public Option<double> Length { get; }
    public static RayPolicy Forward => new(sense: BoundarySense.Toward, length: Option<double>.None);
    public static RayPolicy Reverse => new(sense: BoundarySense.Away, length: Option<double>.None);
    public static Fin<RayPolicy> Segment(double length, BoundarySense? sense = null, Op? key = null) =>
        (RhinoMath.IsValidDouble(x: length), length > RhinoMath.ZeroTolerance, key ?? Op.Of(name: nameof(RayPolicy))) switch {
            (true, true, Op op) => Fin.Succ(new RayPolicy(sense: sense ?? BoundarySense.Toward, length: Some(length))),
            (_, _, Op op) => Fin.Fail<RayPolicy>(error: new VectorFault.Invalid(Key: op, Requirement: "positive finite ray segment length")),
        };
    internal Fin<Ray3d> Ray(Point3d origin, Direction direction, Op key) {
        Vector3d vector = direction.Value * Sense.Sign;
        return key.AcceptValue(value: origin).Map(point => new Ray3d(position: point, direction: vector));
    }
    internal Fin<Line> Line(Point3d origin, Direction direction, Op key) {
        Option<double> length = Length;
        Vector3d vector = direction.Value * Sense.Sign;
        return from span in length.ToFin(Fail: new VectorFault.Invalid(Key: key, Requirement: "finite ray segment length"))
               from point in key.AcceptValue(value: origin)
               from line in key.AcceptValue(value: new Line(start: point, direction: vector, length: span))
               select line;
    }
    internal Fin<VectorSpan> Span(Point3d origin, Direction direction, Context context, Op key) {
        Option<double> length = Length;
        Vector3d vector = direction.Value * Sense.Sign;
        return from span in length.ToFin(Fail: new VectorFault.Invalid(Key: key, Requirement: "finite ray segment length"))
               from value in VectorSpan.Of(anchor: origin, vector: vector * span, context: context, key: key)
               select value;
    }
}

[Union]
public partial record VectorField {
    public sealed record ConstantCase(Vector3d Value) : VectorField;
    public sealed record InfluenceCase(SupportSpace Source, Falloff Falloff, BoundarySense Sense) : VectorField;
    public sealed record NormalCase(SupportSpace Source, BoundarySense Sense) : VectorField;
    public sealed record BlendCase(Seq<VectorField> Fields, FieldBlend Mode) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Influence(SupportSpace source, Falloff? falloff = null, BoundarySense? sense = null) =>
        new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Inverse, Sense: sense ?? BoundarySense.Toward);
    public static VectorField Normal(SupportSpace source, BoundarySense? sense = null) =>
        new NormalCase(Source: source, Sense: sense ?? BoundarySense.Toward);
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    internal Fin<VectorSpan> Sample(Point3d sample, Context context, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Sample));
        return this switch {
            ConstantCase c => VectorSpan.Of(anchor: sample, vector: c.Value, context: context, key: op),
            InfluenceCase c => from hit in c.Source.Closest(sample: sample, key: op)
                               from distance in hit.Distance.ToFin(Fail: op.InvalidResult())
                               from weight in c.Falloff.Weight(distance: distance, key: op)
                               from direction in Direction.Of(value: c.Sense.Sign * (hit.Point - sample), context: context, key: op)
                               from span in VectorSpan.Of(anchor: sample, direction: direction, magnitude: weight, key: op)
                               select span,
            NormalCase c => from hit in c.Source.Closest(sample: sample, key: op)
                            from normal in hit.Normal.ToFin(Fail: op.InvalidResult())
                            from direction in Direction.Of(value: c.Sense.Sign * normal, context: context, key: op)
                            from span in VectorSpan.Of(anchor: sample, direction: direction, magnitude: 1.0, key: op)
                            select span,
            BlendCase c => c.Fields.TraverseM(field => field.Sample(sample: sample, context: context, key: op)).As()
                .Bind(spans => c.Mode.Combine(spans: spans, sample: sample, context: context, key: op)),
            _ => Fin.Fail<VectorSpan>(error: new VectorFault.Unsupported(Key: op, Source: GetType(), Output: typeof(VectorSpan))),
        };
    }
}
