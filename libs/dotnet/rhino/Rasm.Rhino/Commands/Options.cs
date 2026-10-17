using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence.Settings;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<AddedOption<TValue>> OptionAdder<TValue>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink plugIn, TValue initial, Action<IDisposable> retain);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LabelPair(string Off, string On) {
    public static readonly LabelPair NoYes = new("No", "Yes");
    public static readonly LabelPair OffOn = new("Off", "On");
}

public sealed record AddedOption<T>(int Index, Func<CommandLineOption, IO<T>> Read);

public sealed record OptionSpec<T>(string Caption, Func<GetBaseClass, LocalizeStringPair, IPlugInSink, Action<IDisposable>, IO<AddedOption<T>>> Add) {
    public bool Varies { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class OptionType {
    // --- [ROWS]
    public static readonly OptionAdder<System.Drawing.Color> Color =
        static (getter, name, _, initial, retain) => Held(() => new OptionColor(initial), holder => getter.AddOptionColor(name, ref holder), nameof(GetBaseClass.AddOptionColor), static holder => Fin.Succ(holder.CurrentValue), retain);

    public static OptionAdder<bool> Toggle(LabelPair labels) =>
        (getter, name, plugIn, initial, retain) =>
            from offName in Named(labels.Off, plugIn)
            from onName in Named(labels.On, plugIn)
            from added in Held(() => new OptionToggle(initial, offName, onName), holder => getter.AddOptionToggle(name, ref holder), nameof(GetBaseClass.AddOptionToggle), static holder => Fin.Succ(holder.CurrentValue), retain)
            select added;

    public static IO<AddedOption<TValue>> Number<TValue, TRaw>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink _, TValue initial, Action<IDisposable> retain)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IFloatingPointIeee754<TRaw> =>
        Held(() => new OptionDouble(double.CreateChecked(initial.ToValue()), double.CreateChecked(TValue.MinValue.ToValue()), double.CreateChecked(TValue.MaxValue.ToValue())),
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            static holder => Admission.Validated<TValue, TRaw>(TRaw.CreateChecked(holder.CurrentValue)), retain);

    public static IO<AddedOption<TValue>> Integer<TValue, TRaw>(GetBaseClass getter, LocalizeStringPair name, IPlugInSink _, TValue initial, Action<IDisposable> retain)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>, System.Numerics.IMinMaxValue<TValue>
        where TRaw : struct, System.Numerics.IBinaryInteger<TRaw> =>
        Held(() => new OptionInteger(int.CreateChecked(initial.ToValue()), int.CreateSaturating(TValue.MinValue.ToValue()), int.CreateSaturating(TValue.MaxValue.ToValue())),
            holder => getter.AddOptionInteger(name, ref holder), nameof(GetBaseClass.AddOptionInteger),
            static holder => Admission.Validated<TValue, TRaw>(TRaw.CreateChecked(holder.CurrentValue)), retain);

    public static OptionAdder<int> Integer(Option<int> lower, Option<int> upper) =>
        (getter, name, _, initial, retain) => Held(() => (lower.Case, upper.Case) switch {
            (int minimum, int maximum) => new OptionInteger(initial, minimum, maximum),
            (int minimum, _) => new OptionInteger(initial, setLowerLimit: true, minimum),
            (_, int maximum) => new OptionInteger(initial, setLowerLimit: false, maximum),
            _ => new OptionInteger(initial),
        },
            holder => getter.AddOptionInteger(name, ref holder), nameof(GetBaseClass.AddOptionInteger),
            static holder => Fin.Succ(holder.CurrentValue), retain);

    public static OptionAdder<TValue> Distance<TValue>(LengthUnit spaceUnit)
        where TValue : IObjectFactory<TValue, Length, ValidationError>, IConvertible<Length>, System.Numerics.IMinMaxValue<TValue> =>
        (getter, name, _, initial, retain) => Held(() => new OptionDouble(Quantities.As(initial.ToValue(), spaceUnit), Quantities.As(TValue.MinValue.ToValue(), spaceUnit), Quantities.As(TValue.MaxValue.ToValue(), spaceUnit)),
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            holder => Admission.Validated<TValue, Length>(Quantities.From(holder.CurrentValue, spaceUnit)), retain);

    public static OptionAdder<TValue> Text<TValue>(bool allowEmptyString)
        where TValue : IObjectFactory<TValue, string, ValidationError>, IConvertible<string> =>
        (getter, name, _, initial, retain) => Held(() => new OptionString(initial.ToValue(), allowEmptyString),
            holder => getter.AddOptionString(name, ref holder), nameof(GetBaseClass.AddOptionString),
            static holder => Admission.Validated<TValue, string>(holder.CurrentValue), retain);

    public static OptionAdder<TItem> List<TItem>(Seq<TItem> items, Func<TItem, string> caption) where TItem : notnull =>
        (getter, name, plugIn, initial, _) =>
            from values in items.TraverseM(item => Named(caption(item), plugIn)).As()
            from index in Indexed(() => getter.AddOptionList(name, values, items.TakeWhile(item => !EqualityComparer<TItem>.Default.Equals(item, initial)).Count), nameof(GetBaseClass.AddOptionList))
            select new AddedOption<TItem>(index, option => IO.lift(() => items.At(option.CurrentListOptionIndex).ToFin(new Refused(nameof(CommandLineOption.CurrentListOptionIndex), Refused.Invalid))));

    // --- [NAMES]
    internal static IO<LocalizeStringPair> Named(string english, IPlugInSink plugIn) =>
        IO.lift(() => Refused.UnlessFound(RhinoGet.StringToCommandOptionName(english, RowText.Localize(english, plugIn).Local), nameof(RhinoGet.StringToCommandOptionName)));

    // --- [ADDERS]
    internal static IO<int> Indexed(Func<int> add, string member) =>
        from index in IO.lift(add)
        from accepted in IO.lift(Refused.Unless(index > 0, index, member))
        select accepted;

    private static IO<AddedOption<TValue>> Held<THolder, TValue>(Func<THolder> create, Func<THolder, int> add, string member, Func<THolder, Fin<TValue>> current, Action<IDisposable> retain) where THolder : class, IDisposable =>
        from holder in DisposalOps.Acquire(IO.lift(create), retain)
        from index in Indexed(() => add(holder), member)
        select new AddedOption<TValue>(index, _ => IO.lift(() => current(holder)));
}

public static class OptionSpec {
    public static OptionSpec<T> Plain<T>(string caption, Option<string> value, bool hidden, Func<CommandLineOption, IO<T>> chosen) =>
        new(caption, (getter, name, _, _) => OptionType.Indexed(() => getter.AddOption(name, value.Map(static text => new LocalizeStringPair(text, text)).ValueUnsafe(), hidden), nameof(GetBaseClass.AddOption))
            .Map(index => new AddedOption<T>(index, chosen)));

    public static OptionSpec<T> Of<T, TValue>(string caption, OptionAdder<TValue> add, TValue initial, Func<TValue, IO<T>> chosen) =>
        new(caption, (getter, name, plugIn, retain) => add(getter, name, plugIn, initial, retain).Map(added => new AddedOption<T>(added.Index, option => added.Read(option).Bind(chosen))));

    public static IO<OptionSpec<T>> Seeded<T, TValue, TRaw>(Command command, PlugInSetting<TValue, TRaw> setting, OptionAdder<TValue> add, Func<TValue, IO<T>> chosen)
        where TValue : notnull
        where TRaw : notnull =>
        from store in IO.pure(PlugInSettings.Store(new SettingsNode(command), setting))
        from held in store.Read
        select Of(setting.Value.Caption, add, held.IfNone(setting.Value.Default), value => store.Write(Some(value)).Map(value).Bind(chosen));
}

public static class CommandOptions {
    public static IO<TResult> Using<T, TResult>(GetBaseClass getter, IPlugInSink plugIn, Seq<OptionSpec<T>> options, Func<IO<T>, IO<TResult>> body) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            from added in options.TraverseM(option => Register(getter, plugIn, option, retain)).As()
            from answer in body(Chosen(getter, toHashMap(added.Map(static row => (row.Index, row)))))
            select answer, DisposalOps.Release);

    private static IO<AddedOption<T>> Register<T>(GetBaseClass getter, IPlugInSink plugIn, OptionSpec<T> option, Action<IDisposable> retain) =>
        from name in OptionType.Named(option.Caption, plugIn)
        from added in option.Add(getter, name, plugIn, retain)
        from varied in IO.lift(() => getter.SetOptionVaries(added.Index, option.Varies))
        select added;

    private static IO<T> Chosen<T>(GetBaseClass getter, HashMap<int, AddedOption<T>> added) =>
        from option in IO.lift(() => Refused.UnlessFound(getter.Option(), nameof(GetBaseClass.Option)))
        from row in IO.lift(added.Find(option.Index).ToFin(new Refused(nameof(CommandLineOption.Index), Refused.Invalid)))
        from value in row.Read(option)
        select value;
}
