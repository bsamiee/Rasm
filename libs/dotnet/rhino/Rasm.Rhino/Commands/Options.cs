using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Persistence.Settings;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LabelPair(string Off, string On) {
    public static readonly LabelPair NoYes = new("No", "Yes");
    public static readonly LabelPair OffOn = new("Off", "On");
}

public sealed record AddedOption<T>(int Index, Option<IDisposable> Holder, Func<CommandLineOption, IO<T>> Read) {
    public AddedOption<TNext> Bind<TNext>(Func<T, IO<TNext>> next) => new(Index, Holder, option => Read(option).Bind(next));
}

public sealed record OptionType<TValue>(Func<GetBaseClass, LocalizeStringPair, IPlugInSink, TValue, IO<AddedOption<TValue>>> Add);

public static class OptionType {
    // --- [ROWS]
    public static readonly OptionType<System.Drawing.Color> Color = new(static (getter, name, _, initial) => Held(
        () => new OptionColor(initial), holder => getter.AddOptionColor(name, ref holder), nameof(GetBaseClass.AddOptionColor), static holder => Fin.Succ(holder.CurrentValue)));

    public static OptionType<bool> Toggle(LabelPair labels) =>
        new((getter, name, plugIn, initial) =>
            from offName in Named(labels.Off, plugIn)
            from onName in Named(labels.On, plugIn)
            from added in Held(() => new OptionToggle(initial, offName, onName), holder => getter.AddOptionToggle(name, ref holder), nameof(GetBaseClass.AddOptionToggle), static holder => Fin.Succ(holder.CurrentValue))
            select added);

    public static OptionType<TValue> Number<TValue, TRaw, TError>()
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IFloatingPointIeee754<TRaw>
        where TError : Error, IValidationError<TError> =>
        new(static (getter, name, _, initial) => Held(
            () => new OptionDouble(double.CreateChecked(initial.ToValue()), double.CreateChecked(TValue.MinValue.ToValue()), double.CreateChecked(TValue.MaxValue.ToValue())),
            holder => getter.AddOptionDouble(name, ref holder),
            nameof(GetBaseClass.AddOptionDouble),
            static holder => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(holder.CurrentValue))));

    public static OptionType<TValue> Integer<TValue, TRaw, TError>()
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IBinaryInteger<TRaw>
        where TError : Error, IValidationError<TError> =>
        new(static (getter, name, _, initial) => Held(
            () => new OptionInteger(int.CreateSaturating(initial.ToValue()), int.CreateSaturating(TValue.MinValue.ToValue()), int.CreateSaturating(TValue.MaxValue.ToValue())),
            holder => getter.AddOptionInteger(name, ref holder),
            nameof(GetBaseClass.AddOptionInteger),
            static holder => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(holder.CurrentValue))));

    public static OptionType<TValue> Distance<TValue, TError>(LengthUnit spaceUnit)
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, System.Numerics.IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        new((getter, name, _, initial) => Held(
            () => new OptionDouble(Quantities.As(initial.ToValue(), spaceUnit), Quantities.As(TValue.MinValue.ToValue(), spaceUnit), Quantities.As(TValue.MaxValue.ToValue(), spaceUnit)),
            holder => getter.AddOptionDouble(name, ref holder),
            nameof(GetBaseClass.AddOptionDouble),
            holder => Conversions.Validated<TValue, Length, TError>(Quantities.From(holder.CurrentValue, spaceUnit))));

    public static OptionType<TValue> Text<TValue, TError>()
        where TValue : IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        new(static (getter, name, _, initial) => Held(
            () => new OptionString(initial.ToValue(), allowEmptyString: Conversions.Validated<TValue, string, TError>("").IsSucc),
            holder => getter.AddOptionString(name, ref holder),
            nameof(GetBaseClass.AddOptionString),
            static holder => Conversions.Validated<TValue, string, TError>(holder.CurrentValue)));

    public static OptionType<TItem> List<TItem>(Seq<TItem> items, Func<TItem, string> caption) where TItem : notnull =>
        new((getter, name, plugIn, initial) =>
            from values in items.TraverseM(item => Named(caption(item), plugIn)).As()
            from index in Indexed(
                () => getter.AddOptionList(name, values, items.TakeWhile(item => !EqualityComparer<TItem>.Default.Equals(item, initial)).Count),
                nameof(GetBaseClass.AddOptionList))
            select new AddedOption<TItem>(index, None, option => IO.lift(() =>
                items.At(option.CurrentListOptionIndex).ToFin(new InvalidAnswer(nameof(CommandLineOption.CurrentListOptionIndex))))));

    // --- [NAMES]
    internal static IO<LocalizeStringPair> Named(string english, IPlugInSink plugIn) =>
        IO.lift(() => Missing.Unless(RhinoGet.StringToCommandOptionName(english, RowText.Localize(english, table: Some<object>(plugIn)).Local), nameof(RhinoGet.StringToCommandOptionName)));

    // --- [ADDERS]
    internal static IO<int> Indexed(Func<int> add, string member) =>
        IO.lift(add).Bind(index => IO.lift(Refused.Unless(index > 0, index, member)));

    private static IO<AddedOption<TValue>> Held<THolder, TValue>(Func<THolder> create, Func<THolder, int> add, string member, Func<THolder, Fin<TValue>> current)
        where THolder : IDisposable =>
        from holder in IO.lift(create)
        from index in DisposalOps.OnFailure(Indexed(() => add(holder), member), IO.lift(holder.Dispose))
        select new AddedOption<TValue>(index, Some<IDisposable>(holder), _ => IO.lift(() => current(holder)));
}

public sealed record OptionSpec<T>(string Caption, Func<GetBaseClass, LocalizeStringPair, IPlugInSink, IO<AddedOption<T>>> Add) {
    public bool Varies { get; init; }
}

public static class OptionSpec {
    // --- [OPTIONS]
    public static OptionSpec<T> Plain<T>(string caption, Option<string> value, bool hidden, IO<T> chosen) =>
        new(caption, (getter, name, _) => OptionType.Indexed(() => getter.AddOption(name, value.Map(static text => new LocalizeStringPair(text, text)).ValueUnsafe(), hidden), nameof(GetBaseClass.AddOption))
            .Map(index => new AddedOption<T>(index, None, _ => chosen)));

    public static OptionSpec<T> Of<T, TValue>(string caption, OptionType<TValue> type, TValue initial, Func<TValue, IO<T>> chosen) =>
        new(caption, (getter, name, plugIn) => type.Add(getter, name, plugIn, initial).Map(added => added.Bind(chosen)));

    // --- [SEEDS]
    public static IO<OptionSpec<T>> Seeded<T, TValue, TRaw, TError>(Command command, PlugInSetting<TValue, TRaw, TError> setting, OptionType<TValue> type, Func<TValue, IO<T>> chosen)
        where TValue : notnull
        where TRaw : notnull =>
        new SettingsNode(command) switch {
            var node => PlugInSettings.Current(node, setting)
                .Map(held => Of(setting.Value.Caption, type, held, value => PlugInSettings.Store(node, setting).Write(Some(value)).Bind(_ => chosen(value)))),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CommandOptions {
    public static IO<TResult> Using<T, TResult>(GetBaseClass getter, IPlugInSink plugIn, Seq<OptionSpec<T>> options, Func<IO<T>, IO<TResult>> body) =>
        DisposalOps.AcquireAll(options.Map(option => Register(getter, plugIn, option)), Release)
            .Bracket(Use: added => body(Chosen(getter, toHashMap(added))), Fin: Release);

    private static IO<(string Name, AddedOption<T> Option)> Register<T>(GetBaseClass getter, IPlugInSink plugIn, OptionSpec<T> option) =>
        from name in OptionType.Named(option.Caption, plugIn)
        from added in option.Add(getter, name, plugIn)
        from varied in DisposalOps.OnFailure(
            when(option.Varies, IO.lift(() => getter.SetOptionVaries(added.Index, varies: true))).As(),
            DisposalOps.Release(added.Holder.ToSeq()))
        select (name.English, added);

    private static IO<T> Chosen<T>(GetBaseClass getter, HashMap<string, AddedOption<T>> added) =>
        IO.lift(() =>
                from option in Missing.Unless(getter.Option(), nameof(GetBaseClass.Option))
                from row in added.Find(option.EnglishName).ToFin(new InvalidAnswer(nameof(CommandLineOption.EnglishName)))
                select row.Read(option))
            .Flatten();

    private static IO<Unit> Release<T>(Seq<(string Name, AddedOption<T> Option)> added) =>
        DisposalOps.Release(added.Choose(static row => row.Option.Holder));
}
