using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionSpec<T> {
    public abstract LocalizeStringPair Name { get; init; }

    public bool Varies { get; init; }

    public sealed record Simple(LocalizeStringPair Name, Option<LocalizeStringPair> Value, bool Hidden, IO<T> Chosen) : OptionSpec<T>;

    public sealed record Toggle(LocalizeStringPair Name, bool Initial, LocalizeStringPair Off, LocalizeStringPair On, Func<bool, IO<T>> Chosen) : OptionSpec<T>;

    public sealed record Number(LocalizeStringPair Name, double Initial, Limits<double> Limits, Option<string> Prompt, Func<double, IO<T>> Chosen) : OptionSpec<T>;

    public sealed record Integer(LocalizeStringPair Name, int Initial, Limits<int> Limits, Option<string> Prompt, Func<int, IO<T>> Chosen) : OptionSpec<T>;

    public sealed record String(LocalizeStringPair Name, string Initial, bool AllowEmpty, Option<string> Prompt, Func<string, IO<T>> Chosen) : OptionSpec<T>;

    public sealed record Color(LocalizeStringPair Name, System.Drawing.Color Initial, Option<string> Prompt, Func<System.Drawing.Color, IO<T>> Chosen) : OptionSpec<T>;

    public sealed record List(LocalizeStringPair Name, Seq<LocalizeStringPair> Values, int Current, Func<int, IO<T>> Chosen) : OptionSpec<T>;
}

public sealed record Registered<T>(Seq<IDisposable> Holders, Map<int, Func<CommandLineOption, IO<T>>> Bound);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandOptions {
    // --- [REGISTRATION]
    public static IO<Registered<T>> Register<T>(GetBaseClass getter, Seq<OptionSpec<T>> specs) =>
        from named in IO.lift(() => specs.Bind(Names).Traverse(static check => check).As().ToFin())
        from registered in specs.Fold(
            IO.pure(new Registered<T>(Seq<IDisposable>(), Map<int, Func<CommandLineOption, IO<T>>>())),
            (held, spec) => held.Bind(earlier => GeometryOps.OnFailure(IO.lift(() => Add(getter, spec, earlier)), Disposal.Release(earlier.Holders))))
        select registered;

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

    private static Fin<Registered<T>> Add<T>(GetBaseClass getter, OptionSpec<T> spec, Registered<T> registered) =>
        spec.Switch(
            (Getter: getter, Registered: registered),
            simple: static (state, simple) => Held(state.Getter, state.Getter.AddOption(simple.Name, simple.Value.ValueUnsafe(), simple.Hidden), simple, None, _ => simple.Chosen, state.Registered),
            toggle: static (state, toggle) => {
                OptionToggle holder = new(toggle.Initial, toggle.Off, toggle.On);
                return Held(state.Getter, state.Getter.AddOptionToggle(toggle.Name, ref holder), toggle, holder, Current(() => holder.CurrentValue, toggle.Chosen), state.Registered);
            },
            number: static (state, number) => number.Limits.Inclusive(nameof(OptionDouble)).Bind(limits => {
                OptionDouble holder = limits.Fold<OptionDouble>(
                    both: (lower, upper) => new(number.Initial, lower.Value, upper.Value),
                    lower: lower => new(number.Initial, setLowerLimit: true, lower.Value),
                    upper: upper => new(number.Initial, setLowerLimit: false, upper.Value),
                    none: () => new(number.Initial));
                return Held(state.Getter, state.Getter.AddOptionDouble(number.Name, ref holder, number.Prompt.ValueUnsafe()), number, holder, Current(() => holder.CurrentValue, number.Chosen), state.Registered);
            }),
            integer: static (state, integer) => integer.Limits.Inclusive(nameof(OptionInteger)).Bind(limits => {
                OptionInteger holder = limits.Fold<OptionInteger>(
                    both: (lower, upper) => new(integer.Initial, lower.Value, upper.Value),
                    lower: lower => new(integer.Initial, setLowerLimit: true, lower.Value),
                    upper: upper => new(integer.Initial, setLowerLimit: false, upper.Value),
                    none: () => new(integer.Initial));
                return Held(state.Getter, state.Getter.AddOptionInteger(integer.Name, ref holder, integer.Prompt.ValueUnsafe()), integer, holder, Current(() => holder.CurrentValue, integer.Chosen), state.Registered);
            }),
            @string: static (state, text) => {
                OptionString holder = new(text.Initial, text.AllowEmpty);
                return Held(state.Getter, state.Getter.AddOptionString(text.Name, ref holder, text.Prompt.ValueUnsafe()), text, holder, Current(() => holder.CurrentValue, text.Chosen), state.Registered);
            },
            color: static (state, color) => {
                OptionColor holder = new(color.Initial);
                return Held(state.Getter, state.Getter.AddOptionColor(color.Name, ref holder, color.Prompt.ValueUnsafe()), color, holder, Current(() => holder.CurrentValue, color.Chosen), state.Registered);
            },
            list: static (state, list) => Held(state.Getter, state.Getter.AddOptionList(list.Name, list.Values, list.Current), list, None, option => list.Chosen(option.CurrentListOptionIndex), state.Registered));

    private static Func<CommandLineOption, IO<T>> Current<TValue, T>(Func<TValue> read, Func<TValue, IO<T>> chosen) =>
        _ => IO.lift(read).Bind(chosen);

    private static Fin<Registered<T>> Held<T>(GetBaseClass getter, int index, OptionSpec<T> spec, Option<IDisposable> holder, Func<CommandLineOption, IO<T>> chosen, Registered<T> registered) {
        if (index == 0) {
            _ = holder.Iter(static held => held.Dispose());
            return new OptionNotAdded(spec.Name.English);
        }
        if (spec.Varies)
            getter.SetOptionVaries(index, varies: true);
        return new Registered<T>(registered.Holders + holder.ToSeq(), registered.Bound.Add(index, chosen));
    }

    // --- [READS]
    public static IO<T> Chosen<T>(GetBaseClass getter, Map<int, Func<CommandLineOption, IO<T>>> bound) =>
        IO.lift(() =>
            from option in Missing.Unless(getter.Option(), nameof(GetBaseClass.Option))
            from then in bound.Find(option.Index).ToFin(new Missing(nameof(CommandLineOption.Index)))
            select then(option))
            .Flatten();
}
