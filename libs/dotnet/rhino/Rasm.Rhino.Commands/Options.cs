using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionSpec<T> {
    public abstract LocalizeStringPair Name { get; init; }

    public required T Key { get; init; }

    public bool Varies { get; init; }

    public sealed record Simple(LocalizeStringPair Name, Option<LocalizeStringPair> Value, bool Hidden) : OptionSpec<T>;

    public sealed record Toggle(LocalizeStringPair Name, bool Initial, LocalizeStringPair Off, LocalizeStringPair On) : OptionSpec<T>;

    public sealed record Number(LocalizeStringPair Name, double Initial, Limits<double> Limits, Option<string> Prompt) : OptionSpec<T>;

    public sealed record Integer(LocalizeStringPair Name, int Initial, Limits<int> Limits, Option<string> Prompt) : OptionSpec<T>;

    public sealed record String(LocalizeStringPair Name, string Initial, bool AllowEmpty, Option<string> Prompt) : OptionSpec<T>;

    public sealed record Color(LocalizeStringPair Name, System.Drawing.Color Initial, Option<string> Prompt) : OptionSpec<T>;

    public sealed record List(LocalizeStringPair Name, Seq<LocalizeStringPair> Values, int Current) : OptionSpec<T>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class OptionHolders<T>(Seq<IDisposable> holders, Map<int, (T Key, Option<Func<OptionState>> Read)> bound) : IDisposable {
    private readonly Disposal disposal = new(() => Disposal.Release(holders).Run());

    public Option<(T Key, Option<OptionState> CurrentValue)> Bound(int index) =>
        (disposal.IsDisposed ? Option<(T Key, Option<Func<OptionState>> Read)>.None : bound.Find(index))
            .Map(static row => (row.Key, row.Read.Map(static read => read())));

    public void Dispose() => disposal.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandOptions {
    // --- [REGISTRATION]
    public static IO<OptionHolders<T>> Register<T>(GetBaseClass getter, Seq<OptionSpec<T>> specs) =>
        from named in IO.lift(() => specs.Bind(Names).Traverse(static check => check).As().ToFin())
        from registered in specs.Fold(
            IO.pure((Holders: Seq<IDisposable>(), Bound: Map<int, (T Key, Option<Func<OptionState>> Read)>())),
            (held, spec) => held.Bind(earlier => GeometryOps.OnFailure(IO.lift(() => Add(getter, spec, earlier)), Disposal.Release(earlier.Holders))))
        select new OptionHolders<T>(registered.Holders, registered.Bound);

    private static Seq<Validation<Error, Unit>> Names<T>(OptionSpec<T> spec) =>
        Named(spec.Name, CommandLineOption.IsValidOptionName) + spec.Switch(
            simple: static simple => simple.Value.ToSeq().Bind(static value => Named(value, CommandLineOption.IsValidOptionValueName)),
            toggle: static toggle => Named(toggle.Off, CommandLineOption.IsValidOptionValueName) + Named(toggle.On, CommandLineOption.IsValidOptionValueName),
            number: static _ => Seq<Validation<Error, Unit>>(),
            integer: static _ => Seq<Validation<Error, Unit>>(),
            @string: static _ => Seq<Validation<Error, Unit>>(),
            color: static _ => Seq<Validation<Error, Unit>>(),
            list: static list => list.Values.Bind(static value => Named(value, CommandLineOption.IsValidOptionValueName)));

    private static Seq<Validation<Error, Unit>> Named(LocalizeStringPair name, Func<string, bool> valid) =>
        Seq(name.English, name.Local).Map(text => InvalidOptionName.Unless(valid(text), text).ToValidation());

    private static Fin<(Seq<IDisposable> Holders, Map<int, (T Key, Option<Func<OptionState>> Read)> Bound)> Add<T>(
        GetBaseClass getter,
        OptionSpec<T> spec,
        (Seq<IDisposable> Holders, Map<int, (T Key, Option<Func<OptionState>> Read)> Bound) registered) =>
        spec.Switch(
            (Getter: getter, Registered: registered),
            simple: static (state, simple) => Held(state.Getter, state.Getter.AddOption(simple.Name, simple.Value.ValueUnsafe(), simple.Hidden), simple, Option<(IDisposable Holder, Func<OptionState> Read)>.None, state.Registered),
            toggle: static (state, toggle) => {
                OptionToggle holder = new(toggle.Initial, toggle.Off, toggle.On);
                return Held(state.Getter, state.Getter.AddOptionToggle(toggle.Name, ref holder), toggle, Some<(IDisposable Holder, Func<OptionState> Read)>((holder, () => new OptionState.Toggle(holder.CurrentValue))), state.Registered);
            },
            number: static (state, number) => number.Limits.Inclusive(nameof(OptionDouble)).Bind(limits => {
                OptionDouble holder = limits.Fold<OptionDouble>(
                    both: (lower, upper) => new(number.Initial, lower.Value, upper.Value),
                    lower: lower => new(number.Initial, setLowerLimit: true, lower.Value),
                    upper: upper => new(number.Initial, setLowerLimit: false, upper.Value),
                    none: () => new(number.Initial));
                return Held(state.Getter, state.Getter.AddOptionDouble(number.Name, ref holder, number.Prompt.ValueUnsafe()), number, Some<(IDisposable Holder, Func<OptionState> Read)>((holder, () => new OptionState.Number(holder.CurrentValue))), state.Registered);
            }),
            integer: static (state, integer) => integer.Limits.Inclusive(nameof(OptionInteger)).Bind(limits => {
                OptionInteger holder = limits.Fold<OptionInteger>(
                    both: (lower, upper) => new(integer.Initial, lower.Value, upper.Value),
                    lower: lower => new(integer.Initial, setLowerLimit: true, lower.Value),
                    upper: upper => new(integer.Initial, setLowerLimit: false, upper.Value),
                    none: () => new(integer.Initial));
                return Held(state.Getter, state.Getter.AddOptionInteger(integer.Name, ref holder, integer.Prompt.ValueUnsafe()), integer, Some<(IDisposable Holder, Func<OptionState> Read)>((holder, () => new OptionState.Integer(holder.CurrentValue))), state.Registered);
            }),
            @string: static (state, text) => {
                OptionString holder = new(text.Initial, text.AllowEmpty);
                return Held(state.Getter, state.Getter.AddOptionString(text.Name, ref holder, text.Prompt.ValueUnsafe()), text, Some<(IDisposable Holder, Func<OptionState> Read)>((holder, () => new OptionState.String(holder.CurrentValue))), state.Registered);
            },
            color: static (state, color) => {
                OptionColor holder = new(color.Initial);
                return Held(state.Getter, state.Getter.AddOptionColor(color.Name, ref holder, color.Prompt.ValueUnsafe()), color, Some<(IDisposable Holder, Func<OptionState> Read)>((holder, () => new OptionState.Color(holder.CurrentValue))), state.Registered);
            },
            list: static (state, list) => Held(state.Getter, state.Getter.AddOptionList(list.Name, list.Values, list.Current), list, Option<(IDisposable Holder, Func<OptionState> Read)>.None, state.Registered));

    private static Fin<(Seq<IDisposable> Holders, Map<int, (T Key, Option<Func<OptionState>> Read)> Bound)> Held<T>(
        GetBaseClass getter,
        int index,
        OptionSpec<T> spec,
        Option<(IDisposable Holder, Func<OptionState> Read)> holder,
        (Seq<IDisposable> Holders, Map<int, (T Key, Option<Func<OptionState>> Read)> Bound) registered) {
        if (index == 0) {
            _ = holder.Iter(static held => held.Holder.Dispose());
            return new OptionNotAdded(spec.Name.English);
        }
        if (spec.Varies)
            getter.SetOptionVaries(index, varies: true);
        return (
            holder.Match(Some: held => registered.Holders.Add(held.Holder), None: () => registered.Holders),
            registered.Bound.Add(index, (spec.Key, holder.Map(static held => held.Read))));
    }

    // --- [READS]
    public static IO<(OptionSelection Selection, T Key)> Selected<T>(GetBaseClass getter, OptionHolders<T> holders) =>
        IO.lift(() =>
            from option in Missing.Unless(getter.Option(), nameof(GetBaseClass.Option))
            from row in holders.Bound(option.Index).ToFin(new Missing(nameof(OptionHolders<>.Bound)))
            select (OptionSelection.Read(option, row.CurrentValue), row.Key));
}
