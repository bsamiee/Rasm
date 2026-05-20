namespace Rasm.Vectors;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Vector {
    public static Fin<TOut> Project<TOut>(VectorIntent intent, Context context, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(Project));
        return from active in Optional(intent).ToFin(op.InvalidInput())
               from model in Optional(context).ToFin(op.MissingContext())
               from result in active.Switch(
                   state: (Context: model, Key: op),
                   betweenCase: static (state, c) => from hit in c.Target.Closest(sample: c.Origin, key: state.Key)
                                                     from span in VectorSpan.Of(anchor: c.Origin, vector: c.Sense.Sign * (hit.Point - c.Origin), context: state.Context, key: state.Key)
                                                     from output in span.Project<TOut>(key: state.Key)
                                                     select output,
                   axisCase: static (state, c) => from direction in Direction.Of(value: c.Value.Of(frame: c.Frame), context: state.Context, key: state.Key)
                                                  from output in direction.Project<TOut>(key: state.Key)
                                                  select output,
                   angularCase: static (state, c) => from angle in VectorAngle.Of(a: c.A, b: c.B, frame: c.Frame, key: state.Key)
                                                     from output in angle.Project<TOut>(key: state.Key)
                                                     select output,
                   supportCase: static (state, c) => from hit in c.Space.Closest(sample: c.Sample, key: state.Key)
                                                     from output in c.Projection.Project<TOut>(space: c.Space, hit: hit, sample: c.Sample, context: state.Context, key: state.Key)
                                                     select output,
                   fieldCase: static (state, c) => c.Value.Project<TOut>(sample: c.Sample, context: state.Context, key: state.Key),
                   rayCase: static (state, c) => c.Policy.Project<TOut>(origin: c.Origin, direction: c.Direction, context: state.Context, key: state.Key),
                   ringCase: static (state, c) => from ring in VectorRing.Of(points: c.Points, key: state.Key)
                                                  from output in c.Metric.Project<TOut>(ring: ring, key: state.Key)
                                                  select output,
                   componentsCase: static (state, c) => from span in VectorSpan.Of(anchor: c.Anchor, vector: c.Value, context: state.Context, key: state.Key)
                                                        from components in span.Components(frame: c.Frame, key: state.Key)
                                                        from output in typeof(TOut) == typeof(ValueTuple<double, double>)
                                                            ? Fin.Succ((TOut)(object)components)
                                                            : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(VectorSpan), outputType: typeof(TOut)))
                                                        select output,
                   relationCase: static (state, c) => from relation in VectorRelation.Of(a: c.A, b: c.B, context: state.Context, key: state.Key)
                                                      from output in relation.Project<TOut>(key: state.Key)
                                                      select output)
               select result;
    }
}
