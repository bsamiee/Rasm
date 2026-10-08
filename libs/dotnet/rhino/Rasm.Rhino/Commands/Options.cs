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

public sealed record AddedOption<T>(int Index, Option<IDisposable> Holder, Func<CommandLineOption, IO<T>> Read);

public sealed record OptionSpec<T>(string Caption, Func<GetBaseClass, LocalizeStringPair, IPlugInSink, IO<AddedOption<T>>> Add) {
    public bool Varies { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class OptionType {
    // --- [ROWS]
    public static readonly Func<GetBaseClass, LocalizeStringPair, IPlugInSink, System.Drawing.Color, IO<AddedOption<System.Drawing.Color>>> Color =
        static (getter, name, _, initial) => Held(IO.lift(() => new OptionColor(initial)), holder => getter.AddOptionColor(name, ref holder), nameof(GetBaseClass.AddOptionColor), static holder => Fin.Succ(holder.CurrentValue));

    public static IO<AddedOption<bool>> Toggle(LabelPair labels, GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, bool initial) =>
        from offName in Named(labels.Off, plugIn)
        from onName in Named(labels.On, plugIn)
        from added in Held(IO.lift(() => new OptionToggle(initial, offName, onName)), holder => getter.AddOptionToggle(name, ref holder), nameof(GetBaseClass.AddOptionToggle), static holder => Fin.Succ(holder.CurrentValue))
        select added;

    public static IO<AddedOption<TValue>> Number<TValue, TRaw, TError>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TValue initial)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IFloatingPointIeee754<TRaw>
        where TError : Error, IValidationError<TError> =>
        Held(IO.lift(() => new OptionDouble(double.CreateChecked(initial.ToValue()), double.CreateChecked(TValue.MinValue.ToValue()), double.CreateChecked(TValue.MaxValue.ToValue()))),
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            static holder => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(holder.CurrentValue)));

    public static IO<AddedOption<TValue>> Integer<TValue, TRaw, TError>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TValue initial)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IBinaryInteger<TRaw>
        where TError : Error, IValidationError<TError> =>
        Held(IO.lift(() => Callbacks.Thrown<OverflowException, OptionInteger>(
                () => new OptionInteger(int.CreateChecked(initial.ToValue()), int.CreateSaturating(TValue.MinValue.ToValue()), int.CreateSaturating(TValue.MaxValue.ToValue())), nameof(OptionInteger))),
            holder => getter.AddOptionInteger(name, ref holder), nameof(GetBaseClass.AddOptionInteger),
            static holder => Conversions.Validated<TValue, TRaw, TError>(TRaw.CreateChecked(holder.CurrentValue)));

    public static IO<AddedOption<TValue>> Distance<TValue, TError>(LengthUnit spaceUnit, GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TValue initial)
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, System.Numerics.IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        Held(IO.lift(() => new OptionDouble(Quantities.As(initial.ToValue(), spaceUnit), Quantities.As(TValue.MinValue.ToValue(), spaceUnit), Quantities.As(TValue.MaxValue.ToValue(), spaceUnit))),
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            holder => Conversions.Validated<TValue, Length, TError>(Quantities.From(holder.CurrentValue, spaceUnit)));

    public static IO<AddedOption<TValue>> Text<TValue, TError>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TValue initial)
        where TValue : IObjectFactory<TValue, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        Held(IO.lift(() => new OptionString(initial.ToValue(), allowEmptyString: Conversions.Validated<TValue, string, TError>("").IsSucc)),
            holder => getter.AddOptionString(name, ref holder), nameof(GetBaseClass.AddOptionString),
            static holder => Conversions.Validated<TValue, string, TError>(holder.CurrentValue));

    public static IO<AddedOption<TItem>> List<TItem>(Seq<TItem> items, Func<TItem, string> caption, GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TItem initial) where TItem : notnull =>
        from values in items.TraverseM(item => Named(caption(item), plugIn)).As()
        from index in Indexed(() => getter.AddOptionList(name, values, items.TakeWhile(item => !EqualityComparer<TItem>.Default.Equals(item, initial)).Count), nameof(GetBaseClass.AddOptionList))
        select new AddedOption<TItem>(index, None, option => IO.lift(() => items.At(option.CurrentListOptionIndex).ToFin(new InvalidAnswer(nameof(CommandLineOption.CurrentListOptionIndex)))));

    // --- [NAMES]
    internal static IO<LocalizeStringPair> Named(string english, IPlugInSink plugIn) =>
        IO.lift(() => Missing.Unless(RhinoGet.StringToCommandOptionName(english, RowText.Localize(english, table: Some<object>(plugIn)).Local), nameof(RhinoGet.StringToCommandOptionName)));

    // --- [ADDERS]
    internal static IO<int> Indexed(Func<int> add, string member) =>
        from index in IO.lift(add)
        from accepted in IO.lift(Refused.Unless(index > 0, index, member))
        select accepted;

    private static IO<AddedOption<TValue>> Held<THolder, TValue>(IO<THolder> create, Func<THolder, int> add, string member, Func<THolder, Fin<TValue>> current) where THolder : IDisposable =>
        from holder in create
        from index in DisposalOps.OnFailure(Indexed(() => add(holder), member), IO.lift(holder.Dispose))
        select new AddedOption<TValue>(index, Some<IDisposable>(holder), _ => IO.lift(() => current(holder)));
}

public static class OptionSpec {
    public static OptionSpec<T> Plain<T>(string caption, Option<string> value, bool hidden, Func<CommandLineOption, IO<T>> chosen) =>
        new(caption, (getter, name, _) => OptionType.Indexed(() => getter.AddOption(name, value.Map(static text => new LocalizeStringPair(text, text)).ValueUnsafe(), hidden), nameof(GetBaseClass.AddOption))
            .Map(index => new AddedOption<T>(index, None, chosen)));

    public static OptionSpec<T> Of<T, TValue>(string caption, Func<GetBaseClass, LocalizeStringPair, IPlugInSink, TValue, IO<AddedOption<TValue>>> add, TValue initial, Func<TValue, IO<T>> chosen) =>
        new(caption, (getter, name, plugIn) => add(getter, name, plugIn, initial).Map(added => new AddedOption<T>(added.Index, added.Holder, option => added.Read(option).Bind(chosen))));

    public static IO<OptionSpec<T>> Seeded<T, TValue, TRaw, TError>(Command command, PlugInSetting<TValue, TRaw, TError> setting, Func<GetBaseClass, LocalizeStringPair, IPlugInSink, TValue, IO<AddedOption<TValue>>> add, Func<TValue, IO<T>> chosen)
        where TValue : notnull
        where TRaw : notnull =>
        from store in IO.pure(PlugInSettings.Store(new SettingsNode(command), setting))
        from held in store.Read
        select Of(setting.Value.Caption, add, held.IfNone(setting.Value.Default), value => store.Write(Some(value)).Map(_ => value).Bind(chosen));
}

public static class CommandOptions {
    public static IO<TResult> Using<T, TResult>(GetBaseClass getter, IPlugInSink plugIn, Seq<OptionSpec<T>> options, Func<IO<T>, IO<TResult>> body) =>
        DisposalOps.AcquireAll(options.Map(option => Register(getter, plugIn, option)), Release)
            .Bracket(Use: added => IO.pure(Chosen(getter, toHashMap(added.Map(static row => (row.Index, row))))).Bind(body), Fin: Release);

    private static IO<AddedOption<T>> Register<T>(GetBaseClass getter, IPlugInSink plugIn, OptionSpec<T> option) =>
        from name in OptionType.Named(option.Caption, plugIn)
        from added in option.Add(getter, name, plugIn)
        from varied in DisposalOps.OnFailure(IO.lift(() => getter.SetOptionVaries(added.Index, option.Varies)), DisposalOps.Release(added.Holder.ToSeq()))
        select added;

    private static IO<T> Chosen<T>(GetBaseClass getter, HashMap<int, AddedOption<T>> added) =>
        from option in IO.lift(() => Missing.Unless(getter.Option(), nameof(GetBaseClass.Option)))
        from row in IO.lift(added.Find(option.Index).ToFin(new InvalidAnswer(nameof(CommandLineOption.Index))))
        from value in row.Read(option)
        select value;

    private static IO<Unit> Release<T>(Seq<AddedOption<T>> added) => DisposalOps.Release(added.Choose(static row => row.Holder));
}
