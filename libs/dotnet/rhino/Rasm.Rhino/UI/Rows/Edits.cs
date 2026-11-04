namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Edit<T> where T : notnull {
    public Option<T> Held => Switch(changed: static held => Some(held.Value), cancel: static _ => Option<T>.None);

    public Option<T> Applied => Switch(changed: static held => held.Applies ? Some(held.Value) : None, cancel: static _ => Option<T>.None);

    public Edit<TOut> Mapped<TOut>(Func<T, TOut> into) where TOut : notnull =>
        Switch<Func<T, TOut>, Edit<TOut>>(into, changed: static (map, held) => new Edit<TOut>.Changed(held.Applies, map(held.Value)), cancel: static (_, _) => new Edit<TOut>.Cancel());

    public sealed record Changed(bool Applies, T Value) : Edit<T>;

    public sealed record Cancel : Edit<T>;
}
