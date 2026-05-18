using Color = System.Drawing.Color;

namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandOptionValue(
    int Index,
    string Name,
    Option<object> Value,
    Option<int> ListIndex);

public abstract record CommandOption {
    private CommandOption(string name) => Name = name;

    private delegate int RefOptionBinder<TNative>(GetBaseClass getter, string name, ref TNative native, Option<string> prompt) where TNative : IDisposable;

    public string Name { get; }

    public static CommandOption Named(string name, string? value = null, bool hidden = false) => new NamedCase(Name: name, Value: value, Hidden: hidden);
    public static CommandOption Toggle(string name, bool initial, string off = "No", string on = "Yes") =>
        new RefOptionCase<OptionToggle, bool>(
            Name: name,
            CreateNative: () => (ValidValue(value: off), ValidValue(value: on)).Apply((disabled, enabled) => new OptionToggle(initial, disabled, enabled)).As(),
            Prompt: Option<string>.None,
            BindNative: AddToggle,
            Current: static native => native.CurrentValue);
    public static CommandOption Number(string name, double initial, Option<double> lower = default, Option<double> upper = default, string? prompt = null) =>
        new RefOptionCase<OptionDouble, double>(
            Name: name,
            CreateNative: () => Fin.Succ(value: NumberOption(initial: initial, lower: lower, upper: upper)),
            Prompt: Optional(prompt),
            BindNative: AddDouble,
            Current: static native => native.CurrentValue);
    public static CommandOption Integer(string name, int initial, Option<int> lower = default, Option<int> upper = default, string? prompt = null) =>
        new RefOptionCase<OptionInteger, int>(
            Name: name,
            CreateNative: () => Fin.Succ(value: IntegerOption(initial: initial, lower: lower, upper: upper)),
            Prompt: Optional(prompt),
            BindNative: AddInteger,
            Current: static native => native.CurrentValue);
    public static CommandOption Text(string name, string initial = "", bool allowEmpty = false, string? prompt = null) =>
        new RefOptionCase<OptionString, string>(
            Name: name,
            CreateNative: () => Fin.Succ(value: new OptionString(initial, allowEmpty)),
            Prompt: Optional(prompt),
            BindNative: AddString,
            Current: static native => native.CurrentValue);
    public static CommandOption Color(string name, Color initial, string? prompt = null) =>
        new RefOptionCase<OptionColor, Color>(
            Name: name,
            CreateNative: () => Fin.Succ(value: new OptionColor(initial)),
            Prompt: Optional(prompt),
            BindNative: AddColor,
            Current: static native => native.CurrentValue);
    public static CommandOption List(string name, Seq<string> values, int current = 0) => new ListCase(Name: name, Values: values, Current: current);
    public static CommandOption EnumList<TEnum>(string name, TEnum initial, Seq<TEnum> values = default) where TEnum : struct, Enum => new EnumListCase<TEnum>(Name: name, Initial: initial, Values: values);
    public static CommandOption EnumSelection<TEnum>(string name, Seq<TEnum> values, int current = 0) where TEnum : struct, Enum => new EnumSelectionCase<TEnum>(Name: name, Values: values, Current: current);

    internal abstract Fin<Bound> Add(GetBaseClass getter);

    internal sealed record Bound(int Index, IDisposable? Native, Func<GetBaseClass, CommandOptionValue> Snapshot);

    internal static Fin<CommandOptionScope> Bind(Seq<CommandOption> options, GetBaseClass getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(g => options.TraverseM(option => option.Add(getter: g)).As())
            .Map(static bounds => new CommandOptionScope(bounds: bounds));

    private static OptionDouble NumberOption(double initial, Option<double> lower, Option<double> upper) =>
        (lower, upper) switch {
            ( { IsSome: true } lo, { IsSome: true } hi) => new OptionDouble(initial, lo.IfNone(initial), hi.IfNone(initial)),
            ( { IsSome: true } lo, _) => new OptionDouble(initial, true, lo.IfNone(initial)),
            (_, { IsSome: true } hi) => new OptionDouble(initial, false, hi.IfNone(initial)),
            _ => new OptionDouble(initial),
        };

    private static OptionInteger IntegerOption(int initial, Option<int> lower, Option<int> upper) =>
        (lower, upper) switch {
            ( { IsSome: true } lo, { IsSome: true } hi) => new OptionInteger(initial, lo.IfNone(initial), hi.IfNone(initial)),
            ( { IsSome: true } lo, _) => new OptionInteger(initial, true, lo.IfNone(initial)),
            (_, { IsSome: true } hi) => new OptionInteger(initial, false, hi.IfNone(initial)),
            _ => new OptionInteger(initial),
        };

    private static int AddDouble(GetBaseClass getter, string name, ref OptionDouble native, Option<string> prompt) =>
        prompt.Case switch {
            string label => getter.AddOptionDouble(name, ref native, label),
            _ => getter.AddOptionDouble(name, ref native),
        };

    private static int AddInteger(GetBaseClass getter, string name, ref OptionInteger native, Option<string> prompt) =>
        prompt.Case switch {
            string label => getter.AddOptionInteger(name, ref native, label),
            _ => getter.AddOptionInteger(name, ref native),
        };

    private static int AddString(GetBaseClass getter, string name, ref OptionString native, Option<string> prompt) =>
        prompt.Case switch {
            string label => getter.AddOptionString(name, ref native, label),
            _ => getter.AddOptionString(name, ref native),
        };

    private static int AddColor(GetBaseClass getter, string name, ref OptionColor native, Option<string> prompt) =>
        prompt.Case switch {
            string label => getter.AddOptionColor(name, ref native, label),
            _ => getter.AddOptionColor(name, ref native),
        };

    private static int AddToggle(GetBaseClass getter, string name, ref OptionToggle native, Option<string> _) =>
        getter.AddOptionToggle(name, ref native);

    private static Fin<string> Valid(string name) =>
        guard(CommandLineOption.IsValidOptionName(optionName: name), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: name));

    private static Fin<string> ValidValue(string value) =>
        guard(CommandLineOption.IsValidOptionValueName(optionValue: value), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: value));

    private static Fin<Bound> Added(int index, IDisposable? native, Func<GetBaseClass, CommandOptionValue> snapshot) =>
        index switch {
            > 0 => Fin.Succ(value: new Bound(Index: index, Native: native, Snapshot: snapshot)),
            _ => native switch {
                IDisposable disposable => FailAfterDispose(disposable: disposable),
                _ => Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidResult()),
            },
        };

    private static Fin<Bound> FailAfterDispose(IDisposable disposable) {
        disposable.Dispose();
        return Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidResult());
    }

    private static CommandOptionValue Snapshot(string name, GetBaseClass getter, Option<object> value, Option<int> listIndex) =>
        new(Index: getter.OptionIndex(), Name: name, Value: value, ListIndex: listIndex);

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
                snapshot: g => Snapshot(name: name, getter: g, value: Some((object)Current(native)!), listIndex: Option<int>.None));
        }
    }

    private sealed record NamedCase(string Name, string? Value, bool Hidden) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from bound in Value switch {
                string value => ValidValue(value: value).Bind(valid => Added(
                    index: getter.AddOption(name, valid, Hidden),
                    native: null,
                    snapshot: g => Snapshot(name: name, getter: g, value: Some((object)valid), listIndex: Option<int>.None))),
                _ => Added(
                    index: getter.AddOption(name),
                    native: null,
                    snapshot: g => Snapshot(name: name, getter: g, value: Option<object>.None, listIndex: Option<int>.None)),
            }
            select bound;
    }

    private sealed record ListCase(string Name, Seq<string> Values, int Current) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from values in Values.TraverseM(ValidValue).As()
            from bound in Added(
                index: getter.AddOptionList(name, values.AsIterable(), Current),
                native: null,
                snapshot: g => Snapshot(name: name, getter: g, value: ValueAt(index: g.Option().CurrentListOptionIndex), listIndex: Some(g.Option().CurrentListOptionIndex)))
            select bound;

        private Option<object> ValueAt(int index) =>
            index switch {
                >= 0 when index < Values.Count => Some((object)Values[index]),
                _ => Option<object>.None,
            };
    }

    private sealed record EnumListCase<TEnum>(string Name, TEnum Initial, Seq<TEnum> Values) : CommandOption(name: Name) where TEnum : struct, Enum {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from bound in Added(
                index: Values.IsEmpty
                    ? getter.AddOptionEnumList(name, Initial)
                    : getter.AddOptionEnumList(name, Initial, [.. Values]),
                native: null,
                snapshot: g => Snapshot(name: name, getter: g, value: Some((object)g.GetSelectedEnumValue<TEnum>()), listIndex: Some(g.Option().CurrentListOptionIndex)))
            select bound;
    }

    private sealed record EnumSelectionCase<TEnum>(string Name, Seq<TEnum> Values, int Current) : CommandOption(name: Name) where TEnum : struct, Enum {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in Valid(name: Name)
            from bound in Added(
                index: getter.AddOptionEnumSelectionList(name, Values.AsIterable(), Current),
                native: null,
                snapshot: g => Snapshot(name: name, getter: g, value: Some((object)g.GetSelectedEnumValueFromSelectionList(Values.AsIterable())), listIndex: Some(g.Option().CurrentListOptionIndex)))
            select bound;
    }
}

internal sealed class CommandOptionScope : IDisposable {
    private bool disposed;

    internal CommandOptionScope(Seq<CommandOption.Bound> bounds) => Bounds = bounds;

    internal Seq<CommandOption.Bound> Bounds { get; }

    internal Fin<CommandOptionValue> Snapshot(GetBaseClass getter) =>
        Bounds
            .Find(bound => bound.Index == getter.OptionIndex())
            .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidResult())
            .Map(bound => bound.Snapshot(getter));

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => Bounds.Iter(static bound => Dispose(native: bound.Native)),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    private static Unit Dispose(IDisposable? native) =>
        native switch {
            IDisposable disposable => Effect(action: disposable.Dispose),
            _ => unit,
        };

    private static Unit Effect(Action action) {
        action();
        return unit;
    }
}
