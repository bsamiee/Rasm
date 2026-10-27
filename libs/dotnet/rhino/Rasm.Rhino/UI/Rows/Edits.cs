namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class EditPhase {
    public static readonly EditPhase Preview = new(applies: false);
    public static readonly EditPhase Commit = new(applies: true);

    public bool Applies { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Edit<T> where T : notnull {
    public Option<T> Held => Switch(changed: static held => Some(held.Value), cancel: static _ => Option<T>.None);

    public Option<T> Applied => Switch(changed: static held => Some(held.Value).Filter(_ => held.Phase.Applies), cancel: static _ => Option<T>.None);

    public Edit<TOut> Mapped<TOut>(Func<T, TOut> into) where TOut : notnull =>
        Switch<Edit<TOut>>(changed: held => new Edit<TOut>.Changed(held.Phase, into(held.Value)), cancel: static _ => new Edit<TOut>.Cancel());

    public sealed record Changed(EditPhase Phase, T Value) : Edit<T>;

    public sealed record Cancel : Edit<T>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Edits {
    public static Edit<T> Preview<T>(T value) where T : notnull => new Edit<T>.Changed(EditPhase.Preview, value);

    public static Edit<T> Commit<T>(T value) where T : notnull => new Edit<T>.Changed(EditPhase.Commit, value);

    public static Edit<T> Cancel<T>() where T : notnull => new Edit<T>.Cancel();
}
