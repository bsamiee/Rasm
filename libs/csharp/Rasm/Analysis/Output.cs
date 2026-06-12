using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AnalysisOutput<TOut>(Op Key) {
    public Fin<Seq<TOut>> One<TValue>(TValue value) =>
        Many(values: Seq(value));

    public Fin<Seq<TOut>> Many<TValue>(Seq<TValue> values) {
        Op key = Key;
        return ProjectMany(key: key, values: values);
    }

    public Fin<Seq<TOut>> Many<TValue>(IEnumerable<TValue> values) {
        Op key = Key;
        return Optional(values)
            .ToFin(key.InvalidResult())
            .Bind(found => ProjectMany(key: key, values: found.AsIterable().ToSeq()));
    }

    public Fin<Seq<TOut>> Unsupported<TValue>() =>
        Fin.Fail<Seq<TOut>>(Key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut)));

    private static Fin<Seq<TOut>> ProjectMany<TValue>(Op key, Seq<TValue> values) =>
        typeof(TOut) == typeof(TValue)
            ? values.TraverseM(value => AnalysisAcceptance.AcceptValue(key: key, value: value)).As()
                .Map(static admitted => admitted.Map(static value => (TOut)(object)value!))
            : Fin.Fail<Seq<TOut>>(key.Unsupported(geometryType: typeof(TValue), outputType: typeof(TOut)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class AnalysisAcceptance {
    internal static Fin<TValue> AcceptValue<TValue>(Op key, TValue value) =>
        value switch {
            null => Fin.Fail<TValue>(key.InvalidResult()),
            Enum => Fin.Succ(value),
            _ => ValidityOf(source: value!).Case switch {
                bool ok => ok ? Fin.Succ(value) : Fin.Fail<TValue>(key.InvalidResult()),
                _ => key.AcceptValue(value: value),
            },
        };

    internal static Option<bool> ValidityOf(object? source) =>
        source switch {
            null => Option<bool>.None,
            IntersectionHit hit => Some(hit.IsValid),
            CurveDeviation deviation => Some(deviation.IsValid),
            ResidualSample sample => Some(sample.IsValid),
            SpatialHit hit => Some(hit.IsValid),
            SpatialPair pair => Some(pair.IsValid),
            RayQuery ray => Some(ray.IsValid),
            _ => OpAcceptance.ValidityOf(source: source),
        };
}
