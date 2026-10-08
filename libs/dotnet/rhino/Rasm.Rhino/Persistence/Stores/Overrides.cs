namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Override<T> where T : notnull {
    public Option<T> Applied(Option<T> held) =>
        Switch(held, keep: static (current, _) => current, set: static (_, value) => Some(value.Value), clear: static (_, _) => Option<T>.None);

    public bool Changes => this is not Keep;

    public sealed record Keep : Override<T>;
    public sealed record Set(T Value) : Override<T>;
    public sealed record Clear : Override<T>;
}
