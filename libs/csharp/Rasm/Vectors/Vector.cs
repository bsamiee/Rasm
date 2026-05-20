namespace Rasm.Vectors;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Vector {
    public static Fin<TOut> Project<TOut>(VectorIntent intent, Context context) {
        Op key = Op.Of(name: nameof(Project));
        return from active in Optional(intent).ToFin(key.InvalidInput())
               from model in Optional(context).ToFin(key.MissingContext())
               from result in active switch {
                   VectorIntent.BetweenCase c => from hit in c.Target.Closest(sample: c.Origin, key: key)
                                                 from span in VectorSpan.Of(anchor: c.Origin, vector: c.Sense.Sign * (hit.Point - c.Origin), context: model, key: key)
                                                 from result in ProjectSpan<TOut>(span: span, key: key)
                                                 select result,
                   VectorIntent.AxisCase c => from direction in Direction.Of(value: c.Value.Of(frame: c.Frame), context: model, key: key)
                                              from result in ProjectDirection<TOut>(direction: direction, key: key)
                                              select result,
                   VectorIntent.AngularCase c => from angle in AngleOf(a: c.A, b: c.B, frame: c.Frame)
                                                 from result in ProjectAngle<TOut>(angle: angle, key: key)
                                                 select result,
                   VectorIntent.SupportCase c => from hit in c.Space.Closest(sample: c.Sample, key: key)
                                                 from result in ProjectSupport<TOut>(projection: c.Projection, hit: hit, sample: c.Sample, context: model, key: key)
                                                 select result,
                   VectorIntent.FieldCase c => from span in c.Value.Sample(sample: c.Sample, context: model, key: key)
                                               from result in ProjectSpan<TOut>(span: span, key: key)
                                               select result,
                   VectorIntent.RayCase c => from ray in c.Policy.Ray(origin: c.Origin, direction: c.Direction, key: key)
                                             from result in ProjectRay<TOut>(ray: ray, origin: c.Origin, policy: c.Policy, context: model, key: key)
                                             select result,
                   _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: active.GetType(), Output: typeof(TOut))),
               }
               select result;
    }
    private static Fin<VectorAngle> AngleOf(Direction a, Direction b, Option<Plane> frame) =>
        frame.Map(plane => Vector3d.VectorAngle(a: a.Value, b: b.Value, plane: plane))
            .IfNone(Vector3d.VectorAngle(a: a.Value, b: b.Value))
            .TryCreateValidated<VectorAngle>()
            .ToFin();
    private static Fin<TOut> ProjectDirection<TOut>(Direction direction, Op key) => typeof(TOut) switch {
        Type t when t == typeof(Direction) => Fin.Succ((TOut)(object)direction),
        Type t when t == typeof(Vector3d) => key.AcceptValue(value: direction.Value).Map(static value => (TOut)(object)value),
        _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: typeof(Direction), Output: typeof(TOut))),
    };
    private static Fin<TOut> ProjectAngle<TOut>(VectorAngle angle, Op key) => typeof(TOut) switch {
        Type t when t == typeof(VectorAngle) => Fin.Succ((TOut)(object)angle),
        Type t when t == typeof(double) => key.AcceptValue(value: angle.Value).Map(static value => (TOut)(object)value),
        _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: typeof(VectorAngle), Output: typeof(TOut))),
    };
    private static Fin<TOut> ProjectSpan<TOut>(VectorSpan span, Op key) => typeof(TOut) switch {
        Type t when t == typeof(VectorSpan) => Fin.Succ((TOut)(object)span),
        Type t when t == typeof(Direction) => ProjectDirection<TOut>(direction: span.Direction, key: key),
        Type t when t == typeof(Vector3d) => key.AcceptValue(value: span.Value).Map(static value => (TOut)(object)value),
        Type t when t == typeof(Line) => key.AcceptValue(value: span.Axis).Map(static value => (TOut)(object)value),
        _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: typeof(VectorSpan), Output: typeof(TOut))),
    };
    private static Fin<TOut> ProjectRay<TOut>(Ray3d ray, Point3d origin, RayPolicy policy, Context context, Op key) => typeof(TOut) switch {
        Type t when t == typeof(Ray3d) => key.AcceptValue(value: ray).Map(static value => (TOut)(object)value),
        Type t when t == typeof(Direction) => Direction.Of(value: ray.Direction, context: context, key: key).Bind(direction => ProjectDirection<TOut>(direction: direction, key: key)),
        Type t when t == typeof(Vector3d) => key.AcceptValue(value: ray.Direction).Map(static value => (TOut)(object)value),
        Type t when t == typeof(Line) => Direction.Of(value: ray.Direction, context: context, key: key).Bind(direction => policy.Line(origin: origin, direction: direction, key: key)).Bind(line => key.AcceptValue(value: line).Map(static value => (TOut)(object)value)),
        Type t when t == typeof(VectorSpan) => Direction.Of(value: ray.Direction, context: context, key: key).Bind(direction => policy.Span(origin: origin, direction: direction, context: context, key: key)).Bind(span => ProjectSpan<TOut>(span: span, key: key)),
        _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: typeof(Ray3d), Output: typeof(TOut))),
    };
    private static Fin<TOut> ProjectHit<TOut>(ClosestHit hit, Op key, bool parameterMode = false) =>
        hit.Project<TOut>(key: key, parameterMode: parameterMode).Bind(values => values.Head.ToFin(key.InvalidResult()));
    private static Fin<TOut> ProjectSupport<TOut>(SupportProjection projection, ClosestHit hit, Point3d sample, Context context, Op key) =>
        projection switch {
            SupportProjection p when p.Equals(SupportProjection.Closest) => ProjectHit<TOut>(hit: hit, key: key),
            SupportProjection p when p.Equals(SupportProjection.Direction) => Direction.Of(value: hit.Point - sample, context: context, key: key).Bind(direction => ProjectDirection<TOut>(direction: direction, key: key)),
            SupportProjection p when p.Equals(SupportProjection.Span) => VectorSpan.Of(anchor: sample, vector: hit.Point - sample, context: context, key: key).Bind(span => ProjectSpan<TOut>(span: span, key: key)),
            SupportProjection p when p.Equals(SupportProjection.Normal) => hit.Normal.ToFin(Fail: key.InvalidResult()).Bind(normal => Direction.Of(value: normal, context: context, key: key)).Bind(direction => ProjectDirection<TOut>(direction: direction, key: key)),
            SupportProjection p when p.Equals(SupportProjection.Distance) => ProjectHit<TOut>(hit: hit, key: key),
            SupportProjection p when p.Equals(SupportProjection.Parameter) => ProjectHit<TOut>(hit: hit, key: key, parameterMode: true),
            SupportProjection p when p.Equals(SupportProjection.Uv) => ProjectHit<TOut>(hit: hit, key: key),
            SupportProjection p when p.Equals(SupportProjection.Component) => ProjectHit<TOut>(hit: hit, key: key),
            SupportProjection p when p.Equals(SupportProjection.MeshPoint) => ProjectHit<TOut>(hit: hit, key: key),
            _ => Fin.Fail<TOut>(error: new VectorFault.Unsupported(Key: key, Source: projection.GetType(), Output: typeof(TOut))),
        };
}
