using Color = System.Drawing.Color;

namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandOptionValue(
    int Index,
    string Name,
    Option<object> Value,
    Option<int> ListIndex);

public abstract record CommandOption {
    private CommandOption(string name) => Name = name;

    private delegate int RefOptionBinder<TNative>(GetBaseClass getter, string name, ref TNative native, Option<string> prompt) where TNative : IDisposable;
    private delegate int RefOptionPlainBinder<TNative>(GetBaseClass getter, string name, ref TNative native) where TNative : IDisposable;
    private delegate int RefOptionPromptBinder<TNative>(GetBaseClass getter, string name, ref TNative native, string prompt) where TNative : IDisposable;
    private enum EnumOptionMode { List, Selection }

    public string Name { get; }

    public static CommandOption Named(string name, string? value = null, bool hidden = false) => new NamedCase(Name: name, Value: value, Hidden: hidden);
    public static CommandOption Toggle(string name, bool initial, string off = "No", string on = "Yes") =>
        new RefOptionCase<OptionToggle, bool>(
            Name: name,
            CreateNative: () => (ValidValue(value: off), ValidValue(value: on)).Apply((disabled, enabled) => new OptionToggle(initial, disabled, enabled)).As(),
            Prompt: Option<string>.None,
            BindNative: Plain(bind: static (GetBaseClass getter, string name, ref OptionToggle native) => getter.AddOptionToggle(name, ref native)),
            Current: static native => native.CurrentValue);
    public static CommandOption Number(string name, double initial, Option<double> lower = default, Option<double> upper = default, string? prompt = null) =>
        new RefOptionCase<OptionDouble, double>(
            Name: name,
            CreateNative: () => Fin.Succ(value: BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (double value) => new OptionDouble(value), single: static (double value, bool isLower, double bound) => new OptionDouble(value, isLower, bound), bounded: static (double value, double lo, double hi) => new OptionDouble(value, lo, hi))),
            Prompt: Optional(prompt),
            BindNative: Prompted(
                plain: static (GetBaseClass getter, string name, ref OptionDouble native) => getter.AddOptionDouble(name, ref native),
                prompted: static (GetBaseClass getter, string name, ref OptionDouble native, string label) => getter.AddOptionDouble(name, ref native, label)),
            Current: static native => native.CurrentValue);
    public static CommandOption Integer(string name, int initial, Option<int> lower = default, Option<int> upper = default, string? prompt = null) =>
        new RefOptionCase<OptionInteger, int>(
            Name: name,
            CreateNative: () => Fin.Succ(value: BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (int value) => new OptionInteger(value), single: static (int value, bool isLower, int bound) => new OptionInteger(value, isLower, bound), bounded: static (int value, int lo, int hi) => new OptionInteger(value, lo, hi))),
            Prompt: Optional(prompt),
            BindNative: Prompted(
                plain: static (GetBaseClass getter, string name, ref OptionInteger native) => getter.AddOptionInteger(name, ref native),
                prompted: static (GetBaseClass getter, string name, ref OptionInteger native, string label) => getter.AddOptionInteger(name, ref native, label)),
            Current: static native => native.CurrentValue);
    public static CommandOption Text(string name, string initial = "", bool allowEmpty = false, string? prompt = null) =>
        new RefOptionCase<OptionString, string>(
            Name: name,
            CreateNative: () => Fin.Succ(value: new OptionString(initial, allowEmpty)),
            Prompt: Optional(prompt),
            BindNative: Prompted(
                plain: static (GetBaseClass getter, string name, ref OptionString native) => getter.AddOptionString(name, ref native),
                prompted: static (GetBaseClass getter, string name, ref OptionString native, string label) => getter.AddOptionString(name, ref native, label)),
            Current: static native => native.CurrentValue);
    public static CommandOption Color(string name, Color initial, string? prompt = null) =>
        new RefOptionCase<OptionColor, Color>(
            Name: name,
            CreateNative: () => Fin.Succ(value: new OptionColor(initial)),
            Prompt: Optional(prompt),
            BindNative: Prompted(
                plain: static (GetBaseClass getter, string name, ref OptionColor native) => getter.AddOptionColor(name, ref native),
                prompted: static (GetBaseClass getter, string name, ref OptionColor native, string label) => getter.AddOptionColor(name, ref native, label)),
            Current: static native => native.CurrentValue);
    public static CommandOption List(string name, Seq<string> values, int current = 0) => new ListCase(Name: name, Values: values, Current: current);
    public static CommandOption EnumList<TEnum>(string name, TEnum initial, Seq<TEnum> values = default) where TEnum : struct, Enum =>
        new EnumCase<TEnum>(Name: name, Initial: Some(initial), Values: values, Current: 0, Mode: EnumOptionMode.List);
    public static CommandOption EnumSelection<TEnum>(string name, Seq<TEnum> values, int current = 0) where TEnum : struct, Enum =>
        new EnumCase<TEnum>(Name: name, Initial: Option<TEnum>.None, Values: values, Current: current, Mode: EnumOptionMode.Selection);

    internal abstract Fin<Bound> Add(GetBaseClass getter);

    internal sealed record Bound(int Index, IDisposable? Native, Func<GetBaseClass, Fin<CommandOptionValue>> Snapshot) {
        internal Unit Dispose() {
            Native?.Dispose();
            return unit;
        }
    }

    internal static Fin<Scope> Bind(Seq<CommandOption> options, GetBaseClass getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(g => options.TraverseM(option => option.Add(getter: g)).As())
            .Map(static bounds => new Scope(bounds: bounds));

    private static TNative BoundedOption<TValue, TNative>(
        TValue initial,
        Option<TValue> lower,
        Option<TValue> upper,
        Func<TValue, TNative> unconstrained,
        Func<TValue, bool, TValue, TNative> single,
        Func<TValue, TValue, TValue, TNative> bounded) =>
        (lower, upper) switch {
            ( { IsSome: true } lo, { IsSome: true } hi) => bounded(arg1: initial, arg2: lo.IfNone(initial), arg3: hi.IfNone(initial)),
            ( { IsSome: true } lo, _) => single(arg1: initial, arg2: true, arg3: lo.IfNone(initial)),
            (_, { IsSome: true } hi) => single(arg1: initial, arg2: false, arg3: hi.IfNone(initial)),
            _ => unconstrained(arg: initial),
        };

    private static RefOptionBinder<TNative> Plain<TNative>(RefOptionPlainBinder<TNative> bind) where TNative : IDisposable =>
        (GetBaseClass getter, string name, ref TNative native, Option<string> _) => bind(getter, name, ref native);

    private static RefOptionBinder<TNative> Prompted<TNative>(RefOptionPlainBinder<TNative> plain, RefOptionPromptBinder<TNative> prompted) where TNative : IDisposable =>
        (GetBaseClass getter, string name, ref TNative native, Option<string> prompt) => prompt.Case switch {
            string label => prompted(getter, name, ref native, label),
            _ => plain(getter, name, ref native),
        };

    private static Fin<string> Valid(string name) =>
        guard(CommandLineOption.IsValidOptionName(optionName: name), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: name));

    private static Fin<string> ValidValue(string value) =>
        guard(CommandLineOption.IsValidOptionValueName(optionValue: value), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: value));

    private static Fin<Bound> Added(int index, IDisposable? native, Func<GetBaseClass, Fin<CommandOptionValue>> snapshot) {
        Bound bound = new(Index: index, Native: native, Snapshot: snapshot);
        return index switch {
            > 0 => Fin.Succ(value: bound),
            _ => bound.Dispose() switch {
                _ => Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidResult()),
            },
        };
    }

    private static CommandOptionValue Snapshot(string name, GetBaseClass getter, Option<object> value, Option<int> listIndex) =>
        new(Index: getter.OptionIndex(), Name: name, Value: value, ListIndex: listIndex);

    private static Fin<CommandOptionValue> SnapshotFin(string name, GetBaseClass getter, Option<object> value, Option<int> listIndex) =>
        Fin.Succ(value: Snapshot(name: name, getter: getter, value: value, listIndex: listIndex));

    private static Fin<Seq<TValue>> NonEmpty<TValue>(Seq<TValue> values) =>
        values.IsEmpty switch {
            true => Fin.Fail<Seq<TValue>>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
            false => Fin.Succ(value: values),
        };

    private static Fin<int> ValidIndex(int index, int count) =>
        index switch {
            >= 0 when index < count => Fin.Succ(value: index),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(CommandOption)).InvalidResult()),
        };

    private static Option<int> EnumIndex<TEnum>(Seq<TEnum> values, TEnum value) where TEnum : struct, Enum =>
        toSeq(Enumerable.Range(start: 0, count: values.Count))
            .Find(index => EqualityComparer<TEnum>.Default.Equals(x: values[index], y: value));

    private sealed record RefOptionCase<TNative, TValue>(
        string Name,
        Func<Fin<TNative>> CreateNative,
        Option<string> Prompt,
        RefOptionBinder<TNative> BindNative,
        Func<TNative, TValue> Current) : CommandOption(name: Name) where TNative : IDisposable {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from bound in AddNative(getter: getter, name: name)
            select bound;

        private Fin<Bound> AddNative(GetBaseClass getter, string name) =>
            from native in CreateNative()
            from bound in AddNative(getter: getter, name: name, native: native)
            select bound;

        private Fin<Bound> AddNative(GetBaseClass getter, string name, TNative native) {
            int index = BindNative(getter, name, ref native, Prompt);
            return Added(
                index: index,
                native: native,
                snapshot: g => SnapshotFin(name: name, getter: g, value: Some((object)Current(native)!), listIndex: Option<int>.None));
        }
    }

    private sealed record NamedCase(string Name, string? Value, bool Hidden) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from bound in Value switch {
                string value => ValidValue(value: value).Bind(valid => Added(
                    index: getter.AddOption(name, valid, Hidden),
                    native: null,
                    snapshot: g => SnapshotFin(name: name, getter: g, value: Some((object)valid), listIndex: Option<int>.None))),
                _ => Added(
                    index: getter.AddOption(name),
                    native: null,
                    snapshot: g => SnapshotFin(name: name, getter: g, value: Option<object>.None, listIndex: Option<int>.None)),
            }
            select bound;
    }

    private sealed record ListCase(string Name, Seq<string> Values, int Current) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from values in Values.TraverseM(ValidValue).As()
            from nonEmpty in NonEmpty(values: values)
            from current in ValidIndex(index: Current, count: nonEmpty.Count)
            from bound in Added(
                index: getter.AddOptionList(name, nonEmpty.AsIterable(), current),
                native: null,
                snapshot: g => SnapshotAt(name: name, getter: g, values: nonEmpty))
            select bound;

        private static Fin<CommandOptionValue> SnapshotAt(string name, GetBaseClass getter, Seq<string> values) =>
            from index in ValidIndex(index: getter.Option().CurrentListOptionIndex, count: values.Count)
            select Snapshot(name: name, getter: getter, value: Some((object)values[index]), listIndex: Some(index));
    }

    private sealed record EnumCase<TEnum>(string Name, Option<TEnum> Initial, Seq<TEnum> Values, int Current, EnumOptionMode Mode) : CommandOption(name: Name) where TEnum : struct, Enum {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from values in Mode switch {
                EnumOptionMode.List => Fin.Succ(value: Values.IsEmpty switch { true => toSeq(Enum.GetValues<TEnum>()), false => Values }),
                _ => NonEmpty(values: Values),
            }
            from current in Mode switch { EnumOptionMode.List => Fin.Succ(value: Current), _ => ValidIndex(index: Current, count: values.Count) }
            from initial in Initial.Case switch {
                TEnum value => from _ in EnumIndex(values: values, value: value).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
                               select value,
                _ => Fin.Succ(value: values[0]),
            }
            from bound in Mode switch {
                EnumOptionMode.List => Added(
                    index: Values.IsEmpty ? getter.AddOptionEnumList(name, initial) : getter.AddOptionEnumList(name, initial, [.. values]),
                    native: null,
                    snapshot: g => SnapshotAt(name: name, getter: g, values: values, mode: Mode)),
                _ => Added(
                    index: getter.AddOptionEnumSelectionList(name, values.AsIterable(), current),
                    native: null,
                    snapshot: g => SnapshotAt(name: name, getter: g, values: values, mode: Mode)),
            }
            select bound;

        private static Fin<CommandOptionValue> SnapshotAt(string name, GetBaseClass getter, Seq<TEnum> values, EnumOptionMode mode) =>
            (mode switch {
                EnumOptionMode.List => Try.lift<TEnum>(f: getter.GetSelectedEnumValue<TEnum>).Run(),
                EnumOptionMode.Selection => Try.lift<TEnum>(f: () => getter.GetSelectedEnumValueFromSelectionList(values.AsIterable())).Run(),
                _ => Fin.Fail<TEnum>(error: Op.Of(name: nameof(CommandOption)).InvalidResult()),
            })
                .MapFail(static _ => Op.Of(name: nameof(CommandOption)).InvalidResult())
                .Map(selected => Snapshot(
                    name: name,
                    getter: getter,
                    value: Some((object)selected),
                    listIndex: EnumIndex(values: values, value: selected)));
    }

    internal sealed class Scope : IDisposable {
        private bool disposed;

        internal Scope(Seq<CommandOption.Bound> bounds) => Bounds = bounds;

        internal Seq<CommandOption.Bound> Bounds { get; }

        internal Fin<CommandOptionValue> Snapshot(GetBaseClass getter) =>
            Bounds
                .Find(bound => bound.Index == getter.OptionIndex())
                .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidResult())
                .Bind(bound => bound.Snapshot(getter));

        public void Dispose() {
            _ = disposed switch {
                true => unit,
                false => Bounds.Iter(static bound => bound.Dispose()),
            };
            disposed = true;
            GC.SuppressFinalize(obj: this);
        }
    }
}
