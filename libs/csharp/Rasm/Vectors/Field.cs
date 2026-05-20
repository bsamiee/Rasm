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
    private RayPolicy(BoundarySense sense, Option<PositiveMagnitude> length) {
        Sense = sense;
        Length = length;
    }
    public BoundarySense Sense { get; }
    public Option<PositiveMagnitude> Length { get; }
    public static RayPolicy Forward => new(sense: BoundarySense.Toward, length: Option<PositiveMagnitude>.None);
    public static RayPolicy Reverse => new(sense: BoundarySense.Away, length: Option<PositiveMagnitude>.None);
    public static Fin<RayPolicy> Segment(double length, BoundarySense? sense = null, Op? key = null) =>
        length.TryCreateValidated<PositiveMagnitude>().ToFin()
            .Map(span => new RayPolicy(sense: sense ?? BoundarySense.Toward, length: Some(span)));
    internal Fin<Ray3d> Ray(Point3d origin, Direction direction, Op key) {
        Vector3d vector = direction.Value * Sense.Sign;
        return key.AcceptValue(value: origin).Map(point => new Ray3d(position: point, direction: vector));
    }
    internal Fin<Line> Line(Point3d origin, Direction direction, Op key) {
        Option<PositiveMagnitude> length = Length;
        Vector3d vector = direction.Value * Sense.Sign;
        return from span in length.ToFin(Fail: key.InvalidInput())
               from point in key.AcceptValue(value: origin)
               from line in key.AcceptValue(value: new Line(start: point, direction: vector, length: span.Value))
               select line;
    }
    internal Fin<VectorSpan> Span(Point3d origin, Direction direction, Context context, Op key) {
        Option<PositiveMagnitude> length = Length;
        Vector3d vector = direction.Value * Sense.Sign;
        return from span in length.ToFin(Fail: key.InvalidInput())
               from value in VectorSpan.Of(anchor: origin, vector: vector * span.Value, context: context, key: key)
               select value;
    }
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key) {
        RayPolicy policy = this;
        Vector3d vector = direction.Value * policy.Sense.Sign;
        return typeof(TOut) switch {
            Type t when t == typeof(Ray3d) => policy.Ray(origin: origin, direction: direction, key: key)
                .Bind(ray => key.AcceptValue(value: ray).Map(static value => (TOut)(object)value)),
            Type t when t == typeof(Direction) => Direction.Of(value: vector, context: context, key: key)
                .Bind(active => active.Project<TOut>(key: key)),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: vector).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Line) => policy.Line(origin: origin, direction: direction, key: key)
                .Bind(line => key.AcceptValue(value: line).Map(static value => (TOut)(object)value)),
            Type t when t == typeof(VectorSpan) => policy.Span(origin: origin, direction: direction, context: context, key: key)
                .Bind(span => span.Project<TOut>(key: key)),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut))),
        };
    }
}

[Union]
public partial record VectorField {
    public sealed record ConstantCase(Vector3d Value) : VectorField;
    public sealed record InfluenceCase(SupportSpace Source, Falloff Falloff, BoundarySense Sense, Option<PositiveMagnitude> Radius) : VectorField;
    public sealed record NormalCase(SupportSpace Source, BoundarySense Sense) : VectorField;
    public sealed record BlendCase(Seq<VectorField> Fields, FieldBlend Mode) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Influence(SupportSpace source, Falloff? falloff = null, BoundarySense? sense = null) =>
        new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Inverse, Sense: sense ?? BoundarySense.Toward, Radius: Option<PositiveMagnitude>.None);
    public static VectorField Normal(SupportSpace source, BoundarySense? sense = null) =>
        new NormalCase(Source: source, Sense: sense ?? BoundarySense.Toward);
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null) =>
        radius.TryCreateValidated<PositiveMagnitude>().ToFin()
            .Map(value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)));
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    internal Fin<VectorSpan> Sample(Point3d sample, Context context, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Sample));
        return Switch(
            state: (Sample: sample, Context: context, Key: op),
            constantCase: static (state, c) => VectorSpan.Of(anchor: state.Sample, vector: c.Value, context: state.Context, key: state.Key),
            influenceCase: static (state, c) => from hit in c.Source.Closest(sample: state.Sample, key: state.Key)
                                                from distance in hit.Distance.ToFin(Fail: state.Key.InvalidResult())
                                                let residual = c.Radius.Map(radius => Math.Abs(distance - radius.Value)).IfNone(distance)
                                                let shellSign = c.Radius.Map(radius => distance >= radius.Value ? 1.0 : -1.0).IfNone(1.0)
                                                from weight in c.Falloff.Weight(distance: residual, key: state.Key)
                                                from direction in Direction.Of(value: c.Sense.Sign * shellSign * (hit.Point - state.Sample), context: state.Context, key: state.Key)
                                                from span in VectorSpan.Of(anchor: state.Sample, direction: direction, magnitude: c.Radius.IsSome ? residual * weight : weight, key: state.Key)
                                                select span,
            normalCase: static (state, c) => from hit in c.Source.Closest(sample: state.Sample, key: state.Key)
                                             from normal in hit.Normal.ToFin(Fail: state.Key.InvalidResult())
                                             from direction in Direction.Of(value: c.Sense.Sign * normal, context: state.Context, key: state.Key)
                                             from span in VectorSpan.Of(anchor: state.Sample, direction: direction, magnitude: 1.0, key: state.Key)
                                             select span,
            blendCase: static (state, c) => c.Fields.TraverseM(field => field.Sample(sample: state.Sample, context: state.Context, key: state.Key)).As()
                .Bind(spans => c.Mode.Combine(spans: spans, sample: state.Sample, context: state.Context, key: state.Key)));
    }
}
