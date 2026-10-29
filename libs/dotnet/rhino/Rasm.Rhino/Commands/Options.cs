using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LabelPair(string Off, string On) {
    public static readonly LabelPair NoYes = new("No", "Yes");
    public static readonly LabelPair OffOn = new("Off", "On");
}

public sealed record AddedOption<T>(int Index, Func<CommandLineOption, IO<T>> Read);

public sealed record OptionSpec<T>(string Caption, Func<GetBaseClass, LocalizeStringPair, IO<AddedOption<T>>> Add) {
    public bool Varies { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class OptionType {
    // --- [ROWS]
    public static readonly Func<GetBaseClass, LocalizeStringPair, Color, IO<AddedOption<Color>>> Swatch =
        static (getter, name, initial) => Held(() => new OptionColor(initial), holder => getter.AddOptionColor(name, ref holder), static holder => Fin.Succ(holder.CurrentValue));

    public static Func<GetBaseClass, LocalizeStringPair, bool, IO<AddedOption<bool>>> Toggle(LabelPair labels) =>
        (getter, name, initial) =>
            from offName in Named(labels.Off)
            from onName in Named(labels.On)
            from added in Held(() => new OptionToggle(initial, offName, onName), holder => getter.AddOptionToggle(name, ref holder), static holder => Fin.Succ(holder.CurrentValue))
            select added;

    public static Func<GetBaseClass, LocalizeStringPair, double, IO<AddedOption<double>>> Number(Option<double> lower, Option<double> upper) =>
        (getter, name, initial) => Held(
            () => (lower.Case, upper.Case) switch {
                (double minimum, double maximum) => new OptionDouble(initial, minimum, maximum),
                (double minimum, _) => new OptionDouble(initial, setLowerLimit: true, minimum),
                (_, double maximum) => new OptionDouble(initial, setLowerLimit: false, maximum),
                _ => new OptionDouble(initial),
            },
            holder => getter.AddOptionDouble(name, ref holder),
            static holder => Fin.Succ(holder.CurrentValue));

    public static Func<GetBaseClass, LocalizeStringPair, int, IO<AddedOption<int>>> Integer(Option<int> lower, Option<int> upper) =>
        (getter, name, initial) => Held(
            () => (lower.Case, upper.Case) switch {
                (int minimum, int maximum) => new OptionInteger(initial, minimum, maximum),
                (int minimum, _) => new OptionInteger(initial, setLowerLimit: true, minimum),
                (_, int maximum) => new OptionInteger(initial, setLowerLimit: false, maximum),
                _ => new OptionInteger(initial),
            },
            holder => getter.AddOptionInteger(name, ref holder),
            static holder => Fin.Succ(holder.CurrentValue));

    public static Func<GetBaseClass, LocalizeStringPair, Length, IO<AddedOption<Length>>> Distance(LengthUnit spaceUnit) =>
        (getter, name, initial) => Held(
            () => new OptionDouble(Quantities.As(initial, spaceUnit)),
            holder => getter.AddOptionDouble(name, ref holder),
            holder => Fin.Succ(Quantities.From(holder.CurrentValue, spaceUnit)));

    public static Func<GetBaseClass, LocalizeStringPair, string, IO<AddedOption<string>>> Text(bool allowEmptyString) =>
        (getter, name, initial) => Held(() => new OptionString(initial, allowEmptyString), holder => getter.AddOptionString(name, ref holder), static holder => Fin.Succ(holder.CurrentValue));

    public static Func<GetBaseClass, LocalizeStringPair, TItem, IO<AddedOption<TItem>>> List<TItem>(Seq<TItem> items, Func<TItem, string> caption) where TItem : notnull =>
        (getter, name, initial) =>
            from values in items.TraverseM(item => Named(caption(item))).As()
            from index in IO.lift(() => Indexed(getter.AddOptionList(name, values, items.TakeWhile(item => !EqualityComparer<TItem>.Default.Equals(item, initial)).Count)))
            select new AddedOption<TItem>(index, option => IO.lift(() => Conversions.Required(items.At(option.CurrentListOptionIndex))));

    // --- [NAMES]
    internal static IO<LocalizeStringPair> Named(string english) =>
        IO.lift(() => Conversions.Required(RhinoGet.StringToCommandOptionName(english, LocalText.Of(english).Local)));

    // --- [ADDERS]
    internal static Fin<int> Indexed(int index, [CallerArgumentExpression(nameof(index))] string call = "") =>
        RhinoFault.Unless(index > 0, RhinoReason.Declined, index, names: call).Map(_ => index);

    private static IO<AddedOption<TValue>> Held<THolder, TValue>(
        Func<THolder> create, Func<THolder, int> add, Func<THolder, Fin<TValue>> current, [CallerArgumentExpression(nameof(add))] string call = "") where THolder : class, IDisposable =>
        from holder in use(create)
        from index in IO.lift(() => Indexed(add(holder), call))
        select new AddedOption<TValue>(index, _ => IO.lift(() => current(holder)));
}

public static class OptionSpecs {
    public static OptionSpec<T> Plain<T>(string caption, Option<string> value, bool hidden, Func<CommandLineOption, IO<T>> chosen) =>
        new(caption, (getter, name) => IO.lift(() => OptionType.Indexed(getter.AddOption(name, value.Map(static text => new LocalizeStringPair(text, text)).ValueUnsafe(), hidden)))
            .Map(index => new AddedOption<T>(index, chosen)));

    public static OptionSpec<T> Of<T, TValue>(string caption, Func<GetBaseClass, LocalizeStringPair, TValue, IO<AddedOption<TValue>>> add, TValue initial, Func<TValue, IO<T>> chosen) =>
        new(caption, (getter, name) => add(getter, name, initial).Map(added => new AddedOption<T>(added.Index, option => added.Read(option).Bind(chosen))));

    public static IO<OptionSpec<T>> Seeded<T, TValue>(string caption, Preference<TValue> preference, Func<GetBaseClass, LocalizeStringPair, TValue, IO<AddedOption<TValue>>> add, Func<TValue, IO<T>> chosen) =>
        preference.Read.Map(held => Of(caption, add, held, value => preference.Write(value) >> chosen(value)));
}

public static class CommandOptions {
    public static IO<TResult> Using<T, TResult>(GetBaseClass getter, Seq<OptionSpec<T>> options, Func<IO<T>, IO<TResult>> body) =>
        (from added in options.TraverseM(option => Register(getter, option)).As()
         from answer in body(Chosen(getter, toHashMap(added.Map(static row => (row.Index, row)))))
         select answer).Bracket();

    private static IO<AddedOption<T>> Register<T>(GetBaseClass getter, OptionSpec<T> option) =>
        from name in OptionType.Named(option.Caption)
        from added in option.Add(getter, name)
        from varied in IO.lift(() => getter.SetOptionVaries(added.Index, option.Varies))
        select added;

    private static IO<T> Chosen<T>(GetBaseClass getter, HashMap<int, AddedOption<T>> added) =>
        from option in IO.lift(() => Conversions.Required(getter.Option()))
        from row in IO.lift(Conversions.Required(added.Find(option.Index)))
        from value in row.Read(option)
        select value;
}
