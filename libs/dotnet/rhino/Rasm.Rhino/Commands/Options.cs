using System.Drawing;
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
    public static readonly OptionAdder<Color> Swatch =
        static (getter, name, _, initial, retain) => Held(() => new OptionColor(initial), holder => getter.AddOptionColor(name, ref holder), nameof(GetBaseClass.AddOptionColor), static holder => Fin.Succ(holder.CurrentValue), retain);

    public static OptionAdder<bool> Toggle(LabelPair labels) =>
        (getter, name, plugIn, initial, retain) =>
            from offName in Named(labels.Off, plugIn)
            from onName in Named(labels.On, plugIn)
            from added in Held(() => new OptionToggle(initial, offName, onName), holder => getter.AddOptionToggle(name, ref holder), nameof(GetBaseClass.AddOptionToggle), static holder => Fin.Succ(holder.CurrentValue), retain)
            select added;

    public static OptionAdder<double> Number(Option<double> lower, Option<double> upper) =>
        (getter, name, _, initial, retain) => Held(() => (lower.Case, upper.Case) switch {
            (double minimum, double maximum) => new OptionDouble(initial, minimum, maximum),
            (double minimum, _) => new OptionDouble(initial, setLowerLimit: true, minimum),
            (_, double maximum) => new OptionDouble(initial, setLowerLimit: false, maximum),
            _ => new OptionDouble(initial),
        },
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            static holder => Fin.Succ(holder.CurrentValue), retain);

    public static OptionAdder<int> Integer(Option<int> lower, Option<int> upper) =>
        (getter, name, _, initial, retain) => Held(() => (lower.Case, upper.Case) switch {
            (int minimum, int maximum) => new OptionInteger(initial, minimum, maximum),
            (int minimum, _) => new OptionInteger(initial, setLowerLimit: true, minimum),
            (_, int maximum) => new OptionInteger(initial, setLowerLimit: false, maximum),
            _ => new OptionInteger(initial),
        },
            holder => getter.AddOptionInteger(name, ref holder), nameof(GetBaseClass.AddOptionInteger),
            static holder => Fin.Succ(holder.CurrentValue), retain);

    public static OptionAdder<Length> Distance(LengthUnit spaceUnit) =>
        (getter, name, _, initial, retain) => Held(() => new OptionDouble(Quantities.As(initial, spaceUnit)),
            holder => getter.AddOptionDouble(name, ref holder), nameof(GetBaseClass.AddOptionDouble),
            holder => Fin.Succ(Quantities.From(holder.CurrentValue, spaceUnit)), retain);

    public static OptionAdder<string> Text(bool allowEmptyString) =>
        (getter, name, _, initial, retain) => Held(() => new OptionString(initial, allowEmptyString),
            holder => getter.AddOptionString(name, ref holder), nameof(GetBaseClass.AddOptionString),
            static holder => Fin.Succ(holder.CurrentValue), retain);

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

public static class OptionSpecs {
    public static OptionSpec<T> Plain<T>(string caption, Option<string> value, bool hidden, Func<CommandLineOption, IO<T>> chosen) =>
        new(caption, (getter, name, _, _) => OptionType.Indexed(() => getter.AddOption(name, value.Map(static text => new LocalizeStringPair(text, text)).ValueUnsafe(), hidden), nameof(GetBaseClass.AddOption))
            .Map(index => new AddedOption<T>(index, chosen)));

    public static OptionSpec<T> Of<T, TValue>(string caption, OptionAdder<TValue> add, TValue initial, Func<TValue, IO<T>> chosen) =>
        new(caption, (getter, name, plugIn, retain) => add(getter, name, plugIn, initial, retain).Map(added => new AddedOption<T>(added.Index, option => added.Read(option).Bind(chosen))));

    public static IO<OptionSpec<T>> Seeded<T, TValue>(Command command, PlugInSetting<TValue> setting, OptionAdder<TValue> add, Func<TValue, IO<T>> chosen) where TValue : notnull =>
        PlugInSettings.Store(new SettingsNode(command), setting) switch {
            var store => store.Read.Map(held => Of(setting.Key, add, held.IfNone(setting.Default), value => store.Write(Some(value)).Map(value).Bind(chosen))),
        };
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
