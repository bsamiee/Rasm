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
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) =>
        vectors switch {
            Seq<Vector3d> values when !values.IsEmpty => key.AcceptValue(value: values.Fold(initialState: Vector3d.Zero, f: static (sum, vector) => sum + vector) * Scale(values)),
            _ => Fin.Fail<Vector3d>(error: key.InvalidResult()),
        };
    [UseDelegateFromConstructor] private partial double Scale(Seq<Vector3d> vectors);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record RayPolicy {
    private RayPolicy() { }
    public sealed record InfiniteCase(BoundarySense Sense) : RayPolicy;
    public sealed record SegmentCase(BoundarySense Sense, PositiveMagnitude Length) : RayPolicy;
    public static RayPolicy Forward => new InfiniteCase(Sense: BoundarySense.Toward);
    public static RayPolicy Reverse => new InfiniteCase(Sense: BoundarySense.Away);
    public static Fin<RayPolicy> Segment(double length, BoundarySense? sense = null, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Segment));
        return length.TryCreateValidated<PositiveMagnitude>()
            .ToFin()
            .BindFail(_ => Fin.Fail<PositiveMagnitude>(error: op.InvalidInput()))
            .Map(value => (RayPolicy)new SegmentCase(Sense: sense ?? BoundarySense.Toward, Length: value));
    }
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key) =>
        from point in key.AcceptValue(value: origin)
        let policy = Switch(
            state: direction.Value,
            infiniteCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Option<PositiveMagnitude>.None),
            segmentCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Some(c.Length)))
        from output in typeof(TOut) switch {
            Type t when t == typeof(Ray3d) => key.AcceptValue(value: new Ray3d(position: point, direction: policy.Vector)).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Direction) => Direction.Of(value: policy.Vector, context: context, key: key).Bind(active => active.Project<TOut>(key: key)),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: policy.Vector).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Line) => policy.Length.ToFin(key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut)))
                .Bind(length => key.AcceptValue(value: new Line(start: point, direction: policy.Vector, length: length.Value)))
                .Map(static value => (TOut)(object)value),
            Type t when t == typeof(VectorSpan) => policy.Length.ToFin(key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut)))
                .Bind(length => VectorSpan.Of(anchor: point, vector: policy.Vector * length.Value, context: context, key: key))
                .Bind(span => span.Project<TOut>(key: key)),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut))),
        }
        select output;
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
    public static Fin<VectorField> Normal(SupportSpace source, BoundarySense? sense = null, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Normal));
        return from active in Optional(source).ToFin(op.InvalidInput())
               from field in active.CanClosestNormal switch {
                   true => Fin.Succ((VectorField)new NormalCase(Source: active, Sense: sense ?? BoundarySense.Toward)),
                   false => Fin.Fail<VectorField>(error: op.Unsupported(active.SourceType, typeof(Vector3d))),
               }
               select field;
    }
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Shell));
        return radius.TryCreateValidated<PositiveMagnitude>().ToFin().BindFail(_ => Fin.Fail<PositiveMagnitude>(error: op.InvalidInput()))
            .Map(value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)));
    }
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Project));
        return from point in op.AcceptValue(value: sample)
               from vector in SampleVector(sample: point, context: context, key: op)
               from output in typeof(TOut) switch {
                   Type t when t == typeof(Vector3d) => op.AcceptValue(value: vector).Map(static value => (TOut)(object)value),
                   Type t when t == typeof(double) => op.AcceptValue(value: vector.Length).Map(static value => (TOut)(object)value),
                   _ => VectorSpan.Of(anchor: point, vector: vector, context: context, key: op).Bind(span => span.Project<TOut>(key: op)),
               }
               select output;
    }
    private Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) =>
        Switch(
            state: (Sample: sample, Context: context, Key: key),
            constantCase: static (state, c) => state.Key.AcceptValue(value: c.Value),
            influenceCase: static (state, c) => from hit in c.Source.Closest(sample: state.Sample, key: state.Key)
                                                from distance in hit.Distance.ToFin(Fail: state.Key.InvalidResult())
                                                let residual = c.Radius.Map(radius => Math.Abs(distance - radius.Value)).IfNone(distance)
                                                let shellSign = c.Radius.Map(radius => distance >= radius.Value ? 1.0 : -1.0).IfNone(1.0)
                                                from weight in c.Falloff.Weight(distance: residual, key: state.Key)
                                                from direction in Direction.Of(value: c.Sense.Sign * shellSign * (hit.Point - state.Sample), context: state.Context, key: state.Key)
                                                from vector in state.Key.AcceptValue(value: direction.Value * (c.Radius.IsSome ? residual * weight : weight))
                                                select vector,
            normalCase: static (state, c) => from _ in guard(c.Source.CanClosestNormal, state.Key.Unsupported(c.Source.SourceType, typeof(Vector3d)))
                                             from hit in c.Source.Closest(sample: state.Sample, key: state.Key)
                                             from normal in hit.Normal.ToFin(Fail: state.Key.InvalidResult())
                                             from direction in Direction.Of(value: c.Sense.Sign * normal, context: state.Context, key: state.Key)
                                             from vector in state.Key.AcceptValue(value: direction.Value)
                                             select vector,
            blendCase: static (state, c) => c.Fields.TraverseM(field => field.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)).As()
                .Bind(vectors => c.Mode.Combine(vectors: vectors, key: state.Key)));
}
