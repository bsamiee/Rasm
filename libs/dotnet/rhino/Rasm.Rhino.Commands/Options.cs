using Rasm.Rhino.Document;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Registered<T>(int Index, Option<IDisposable> Holder, Func<CommandLineOption, IO<T>> Chosen);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionSpec<T> {
    public abstract LocalizeStringPair Name { get; init; }

    public bool Varies { get; init; }

    internal abstract IO<Registered<T>> Add(GetBaseClass getter);

    public sealed record Simple(LocalizeStringPair Name, Option<LocalizeStringPair> Value, bool Hidden, IO<T> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            Held(getter, None, () => getter.AddOption(Name, Value.ValueUnsafe(), Hidden), _ => Chosen);
    }

    public sealed record Toggle(LocalizeStringPair Name, bool Initial, LocalizeStringPair Off, LocalizeStringPair On, Func<bool, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            IO.lift(() => new OptionToggle(Initial, Off, On))
                .Bind(holder => Held(getter, holder, () => getter.AddOptionToggle(Name, ref holder), _ => IO.lift(() => holder.CurrentValue).Bind(Chosen)));
    }

    public sealed record Number(LocalizeStringPair Name, double Initial, Option<double> Lower, Option<double> Upper, Option<string> Prompt, Func<double, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            IO.lift(() => Bounded(Lower, Upper, (lower, upper) => new OptionDouble(Initial, lower, upper), (setLowerLimit, limit) => new OptionDouble(Initial, setLowerLimit, limit), () => new OptionDouble(Initial)))
                .Bind(holder => Held(getter, holder, () => getter.AddOptionDouble(Name, ref holder, Prompt.ValueUnsafe()), _ => IO.lift(() => holder.CurrentValue).Bind(Chosen)));
    }

    public sealed record Integer(LocalizeStringPair Name, int Initial, Option<int> Lower, Option<int> Upper, Option<string> Prompt, Func<int, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            IO.lift(() => Bounded(Lower, Upper, (lower, upper) => new OptionInteger(Initial, lower, upper), (setLowerLimit, limit) => new OptionInteger(Initial, setLowerLimit, limit), () => new OptionInteger(Initial)))
                .Bind(holder => Held(getter, holder, () => getter.AddOptionInteger(Name, ref holder, Prompt.ValueUnsafe()), _ => IO.lift(() => holder.CurrentValue).Bind(Chosen)));
    }

    public sealed record String(LocalizeStringPair Name, string Initial, bool AllowEmpty, Option<string> Prompt, Func<string, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            IO.lift(() => new OptionString(Initial, AllowEmpty))
                .Bind(holder => Held(getter, holder, () => getter.AddOptionString(Name, ref holder, Prompt.ValueUnsafe()), _ => IO.lift(() => holder.CurrentValue).Bind(Chosen)));
    }

    public sealed record Color(LocalizeStringPair Name, System.Drawing.Color Initial, Option<string> Prompt, Func<System.Drawing.Color, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            IO.lift(() => new OptionColor(Initial))
                .Bind(holder => Held(getter, holder, () => getter.AddOptionColor(Name, ref holder, Prompt.ValueUnsafe()), _ => IO.lift(() => holder.CurrentValue).Bind(Chosen)));
    }

    public sealed record List(LocalizeStringPair Name, Seq<LocalizeStringPair> Values, int Current, Func<int, IO<T>> Chosen) : OptionSpec<T> {
        internal override IO<Registered<T>> Add(GetBaseClass getter) =>
            Held(getter, None, () => getter.AddOptionList(Name, Values, Current), option => Chosen(option.CurrentListOptionIndex));
    }

    private IO<Registered<T>> Held(GetBaseClass getter, Option<IDisposable> holder, Func<int> add, Func<CommandLineOption, IO<T>> chosen) =>
        DisposalOps.OnFailure(
            from index in IO.lift(add)
            from added in IO.lift(OptionNotAdded.Unless(index > 0, index, Name.English))
            from varied in when(Varies, IO.lift(() => getter.SetOptionVaries(added, varies: true))).As()
            select new Registered<T>(added, holder, chosen),
            DisposalOps.Release(holder.ToSeq()));

    private static THolder Bounded<TValue, THolder>(Option<TValue> lower, Option<TValue> upper, Func<TValue, TValue, THolder> both, Func<bool, TValue, THolder> one, Func<THolder> none) where TValue : struct =>
        (lower.Case, upper.Case) switch {
            (TValue low, TValue high) => both(low, high),
            (TValue low, _) => one(true, low),
            (_, TValue high) => one(false, high),
            _ => none(),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandOptions {
    public static IO<TValue> Using<T, TValue>(GetBaseClass getter, Seq<OptionSpec<T>> specs, Func<IO<T>, IO<TValue>> body) =>
        DisposalOps.AcquireAll(specs.Map(spec => spec.Add(getter)), Release).Bracket(
            Use: registered => body(IO.lift(() =>
                    from option in Missing.Unless(getter.Option(), nameof(GetBaseClass.Option))
                    from held in registered.Find(row => row.Index == option.Index).ToFin(new Missing(nameof(CommandLineOption.Index)))
                    select held.Chosen(option))
                .Flatten()),
            Fin: Release);

    private static IO<Unit> Release<T>(Seq<Registered<T>> registered) =>
        DisposalOps.Release(registered.Choose(static row => row.Holder));
}
