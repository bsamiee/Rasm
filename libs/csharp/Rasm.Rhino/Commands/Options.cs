using Color = System.Drawing.Color;

namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandOptionValue(
    int Index,
    string Name,
    Option<object> Value,
    Option<int> ListIndex,
    CommandLineOptionType OptionType,
    Option<string> EnglishName,
    Option<string> LocalName,
    Option<string> StringValue,
    int CurrentListIndex,
    Option<bool> CurrentToggleValue,
    double CurrentNumericValue) {
    public Option<T> As<T>() => Value.Bind(static value => value is T typed ? Some(typed) : Option<T>.None);
    public Fin<T> Require<T>() => As<T>().ToFin(Fail: Op.Of(name: nameof(CommandOptionValue)).InvalidResult());
}

public readonly record struct CommandOptionPolicy(
    string? ValueName = null,
    string? LocalName = null,
    string? LocalValueName = null,
    bool Hidden = false,
    bool Varies = false,
    string? Prompt = null,
    string Off = "No",
    string On = "Yes",
    Option<double> Lower = default,
    Option<double> Upper = default,
    bool AllowEmpty = false,
    int Current = 0);

public abstract record CommandOption {
    private CommandOption(string name) => Name = name;

    private delegate int RefOptionBinder<TNative>(GetBaseClass getter, string name, ref TNative native, Option<string> prompt) where TNative : IDisposable;
    private delegate int RefOptionPlainBinder<TNative>(GetBaseClass getter, string name, ref TNative native) where TNative : IDisposable;
    private delegate int RefOptionPromptBinder<TNative>(GetBaseClass getter, string name, ref TNative native, string prompt) where TNative : IDisposable;

    public string Name { get; }

    public static CommandOption Of<T>(string name, T value, CommandOptionPolicy policy = default) =>
        value switch {
            bool toggle => Toggle(name: name, initial: toggle, off: policy.Off, on: policy.On, varies: policy.Varies),
            double number => Number(name: name, initial: number, lower: policy.Lower, upper: policy.Upper, prompt: policy.Prompt, varies: policy.Varies),
            int integer => Integer(name: name, initial: integer, lower: policy.Lower.Map(static bound => (int)bound), upper: policy.Upper.Map(static bound => (int)bound), prompt: policy.Prompt, varies: policy.Varies),
            string text => Text(name: name, initial: text, allowEmpty: policy.AllowEmpty, prompt: policy.Prompt, varies: policy.Varies),
            Color color => Color(name: name, initial: color, prompt: policy.Prompt, varies: policy.Varies),
            IEnumerable<string> values => List(name: name, values: toSeq(values), current: policy.Current, varies: policy.Varies),
            _ => Invalid(name: name),
        };

    public static CommandOption Named(string name, CommandOptionPolicy policy = default) =>
        NamedOption(name: name, value: policy.ValueName, hidden: policy.Hidden, varies: policy.Varies, localName: Optional(policy.LocalName), localValue: Optional(policy.LocalValueName));

    public static CommandOption Enum<TEnum>(string name, TEnum value, CommandOptionPolicy policy = default) where TEnum : struct, Enum =>
        EnumList(name: name, initial: value, values: Seq<TEnum>(), varies: policy.Varies);

    public static CommandOption Enum<TEnum>(string name, IEnumerable<TEnum> values, TEnum selected, CommandOptionPolicy policy = default) where TEnum : struct, Enum =>
        EnumOption(name: name, values: toSeq(values), initial: Some(selected), current: Option<int>.None, varies: policy.Varies);

    public static CommandOption EnumSelection<TEnum>(string name, IEnumerable<TEnum> values, CommandOptionPolicy policy = default) where TEnum : struct, Enum =>
        EnumSelection(name: name, values: toSeq(values), current: policy.Current, varies: policy.Varies);

    public static CommandOption List(string name, IEnumerable<string> values, string selected, CommandOptionPolicy policy = default) {
        Seq<string> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<string>());
        Option<int> index = toSeq(Enumerable.Range(start: 0, count: source.Count))
            .Find(i => StringComparer.Ordinal.Equals(x: source[i], y: selected));
        return index.Case switch {
            int current => List(name: name, values: source, current: current, varies: policy.Varies),
            _ => Invalid(name: name),
        };
    }

    public static CommandOption List<T>(string name, IEnumerable<T> values, Func<T, string> label, T selected, CommandOptionPolicy policy = default) {
        Seq<T> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<T>());
        Option<int> index = toSeq(Enumerable.Range(start: 0, count: source.Count))
            .Find(i => EqualityComparer<T>.Default.Equals(x: source[i], y: selected));
        return (Optional(label).Case, index.Case) switch {
            (Func<T, string> project, int current) => List(name: name, values: source, label: project, current: current, varies: policy.Varies),
            _ => Invalid(name: name),
        };
    }

    private static Case NamedOption(string name, string? value = null, bool hidden = false, bool varies = false, Option<string> localName = default, Option<string> localValue = default) =>
        new(Name: name, AddToGetter: (getter, validName) => value switch {
            string v => ValidValue(value: v).Bind(valid => Added(getter: getter, index: (localName.Case, localValue.Case) switch {
                (string ln, string lv) => getter.AddOption(new global::Rhino.UI.LocalizeStringPair(english: validName, local: ln), new global::Rhino.UI.LocalizeStringPair(english: valid, local: lv), hidden),
                _ => getter.AddOption(validName, valid, hidden),
            }, native: null, snapshot: g => Fin.Succ(value: Snapshot(name: validName, getter: g, value: Some((object)valid), listIndex: Option<int>.None)), varies: varies)),
            _ => Added(getter: getter, index: localName.Case switch {
                string local => getter.AddOption(new global::Rhino.UI.LocalizeStringPair(english: validName, local: local)),
                _ => getter.AddOption(validName),
            }, native: null, snapshot: g => Fin.Succ(value: Snapshot(name: validName, getter: g, value: Option<object>.None, listIndex: Option<int>.None)), varies: varies),
        });

    private static Case Invalid(string name) =>
        new(Name: name, AddToGetter: static (_, _) => Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()));

    private static Case Toggle(string name, bool initial, string off = "No", string on = "Yes", bool varies = false) =>
        Ref(name: name, create: () => (ValidValue(value: off), ValidValue(value: on)).Apply((disabled, enabled) => new OptionToggle(initial, disabled, enabled)).As(), prompt: Option<string>.None, bind: static (GetBaseClass getter, string name, ref OptionToggle native, Option<string> _) => getter.AddOptionToggle(name, ref native), current: static native => native.CurrentValue, varies: varies);
    private static Case Number(string name, double initial, Option<double> lower = default, Option<double> upper = default, string? prompt = null, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (double value) => new OptionDouble(value), single: static (double value, bool isLower, double bound) => new OptionDouble(value, isLower, bound), bounded: static (double value, double lo, double hi) => new OptionDouble(value, lo, hi))), prompt: Optional(prompt), bind: Prompted(plain: static (GetBaseClass getter, string name, ref OptionDouble native) => getter.AddOptionDouble(name, ref native), prompted: static (GetBaseClass getter, string name, ref OptionDouble native, string label) => getter.AddOptionDouble(name, ref native, label)), current: static native => native.CurrentValue, varies: varies);
    private static Case Integer(string name, int initial, Option<int> lower = default, Option<int> upper = default, string? prompt = null, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (int value) => new OptionInteger(value), single: static (int value, bool isLower, int bound) => new OptionInteger(value, isLower, bound), bounded: static (int value, int lo, int hi) => new OptionInteger(value, lo, hi))), prompt: Optional(prompt), bind: Prompted(plain: static (GetBaseClass getter, string name, ref OptionInteger native) => getter.AddOptionInteger(name, ref native), prompted: static (GetBaseClass getter, string name, ref OptionInteger native, string label) => getter.AddOptionInteger(name, ref native, label)), current: static native => native.CurrentValue, varies: varies);
    private static Case Text(string name, string initial = "", bool allowEmpty = false, string? prompt = null, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionString(initial, allowEmpty)), prompt: Optional(prompt), bind: Prompted(plain: static (GetBaseClass getter, string name, ref OptionString native) => getter.AddOptionString(name, ref native), prompted: static (GetBaseClass getter, string name, ref OptionString native, string label) => getter.AddOptionString(name, ref native, label)), current: static native => native.CurrentValue, varies: varies);
    private static Case Color(string name, Color initial, string? prompt = null, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionColor(initial)), prompt: Optional(prompt), bind: Prompted(plain: static (GetBaseClass getter, string name, ref OptionColor native) => getter.AddOptionColor(name, ref native), prompted: static (GetBaseClass getter, string name, ref OptionColor native, string label) => getter.AddOptionColor(name, ref native, label)), current: static native => native.CurrentValue, varies: varies);
    private static Case List(string name, Seq<string> values, int current = 0, bool varies = false) =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from valid in values.TraverseM(ValidValue).As()
            from nonEmpty in NonEmpty(values: valid)
            from index in ValidIndex(index: current, count: nonEmpty.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput())
            from bound in Added(getter: getter, index: getter.AddOptionList(validName, nonEmpty.AsIterable(), index), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: nonEmpty), varies: varies)
            select bound);
    private static Case List<T>(string name, Seq<T> values, Func<T, string> label, int current = 0, bool varies = false) =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from items in values.TraverseM(value => Optional(value).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())).As()
            from labels in items.TraverseM(value => Optional(label(arg: value)).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput()).Bind(ValidValue)).As()
            from nonEmpty in NonEmpty(values: items)
            from index in ValidIndex(index: current, count: items.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput())
            from bound in Added(getter: getter, index: getter.AddOptionList(validName, labels.AsIterable(), index), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: nonEmpty), varies: varies)
            select bound);
    private static Case EnumList<TEnum>(string name, TEnum initial, Seq<TEnum> values = default, bool varies = false) where TEnum : struct, Enum =>
        EnumOption(name: name, values: values, initial: Some(initial), current: Option<int>.None, varies: varies);
    private static Case EnumSelection<TEnum>(string name, Seq<TEnum> values, int current = 0, bool varies = false) where TEnum : struct, Enum =>
        EnumOption<TEnum>(name: name, values: values, initial: Option<TEnum>.None, current: Some(current), varies: varies);

    private static Case Ref<TNative, TValue>(string name, Func<Fin<TNative>> create, Option<string> prompt, RefOptionBinder<TNative> bind, Func<TNative, TValue> current, bool varies) where TNative : IDisposable =>
        new(Name: name, AddToGetter: (getter, validName) => create().Bind(native => {
            int index = bind(getter, validName, ref native, prompt);
            return Added(getter: getter, index: index, native: native, snapshot: g => Fin.Succ(value: Snapshot(name: validName, getter: g, value: Some((object)current(arg: native)!), listIndex: Option<int>.None)), varies: varies);
        }));

    private static Case EnumOption<TEnum>(string name, Seq<TEnum> values, Option<TEnum> initial, Option<int> current, bool varies) where TEnum : struct, Enum =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from options in current.Case switch {
                int => NonEmpty(values: values),
                _ => Fin.Succ(value: values.IsEmpty switch { true => toSeq(global::System.Enum.GetValues<TEnum>()), false => values }),
            }
            from selected in current.Case switch {
                int index => ValidIndex(index: index, count: options.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
                _ => Fin.Succ(value: 0),
            }
            from seed in initial.Case switch {
                TEnum value => from _ in EnumIndex(values: options, value: value).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
                               select value,
                _ => Fin.Succ(value: options[0]),
            }
            from bound in current.Case switch {
                int => Added(getter: getter, index: getter.AddOptionEnumSelectionList(validName, options.AsIterable(), selected), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: options, selection: true), varies: varies),
                _ => Added(getter: getter, index: values.IsEmpty ? getter.AddOptionEnumList(validName, seed) : getter.AddOptionEnumList(validName, seed, [.. options]), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: options, selection: false), varies: varies),
            }
            select bound);

    internal abstract Fin<Bound> Add(GetBaseClass getter);

    private sealed record Case(string Name, Func<GetBaseClass, string, Fin<Bound>> AddToGetter) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in guard(CommandLineOption.IsValidOptionName(optionName: Name), Op.Of(name: nameof(CommandOption)).InvalidInput())
                .Bind(_ => Fin.Succ(value: Name))
            from bound in AddToGetter(arg1: getter, arg2: name)
            select bound;
    }

    internal sealed record Bound(int Index, IDisposable? Native, Func<GetBaseClass, Fin<CommandOptionValue>> Capture) {
        internal Unit Release() {
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

    private static RefOptionBinder<TNative> Prompted<TNative>(RefOptionPlainBinder<TNative> plain, RefOptionPromptBinder<TNative> prompted) where TNative : IDisposable =>
        (GetBaseClass getter, string name, ref TNative native, Option<string> prompt) => prompt.Case switch {
            string label => prompted(getter, name, ref native, label),
            _ => plain(getter, name, ref native),
        };

    private static Fin<string> ValidValue(string value) =>
        guard(CommandLineOption.IsValidOptionValueName(optionValue: value), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: value));

    private static Fin<Bound> Added(GetBaseClass getter, int index, IDisposable? native, Func<GetBaseClass, Fin<CommandOptionValue>> snapshot, bool varies = false) {
        Bound bound = new(Index: index, Native: native, Capture: snapshot);
        return index switch {
            > 0 => Optional(getter)
                .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
                .Map(valid => {
                    valid.SetOptionVaries(optionIndex: index, varies: varies);
                    return bound;
                }),
            _ => bound.Release() switch {
                _ => Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidResult()),
            },
        };
    }

    private static CommandOptionValue Snapshot(string name, GetBaseClass getter, Option<object> value, Option<int> listIndex) {
        CommandLineOption option = getter.Option();
        return new(
            Index: getter.OptionIndex(),
            Name: name,
            Value: value,
            ListIndex: listIndex,
            OptionType: option.OptionType,
            EnglishName: Optional(option.EnglishName),
            LocalName: Optional(option.LocalName),
            StringValue: Optional(option.StringOptionValue),
            CurrentListIndex: option.CurrentListOptionIndex,
            CurrentToggleValue: Optional(option.CurrentToggleValue),
            CurrentNumericValue: option.CurrentNumericValue);
    }

    private static Fin<CommandOptionValue> SnapshotAt(string name, GetBaseClass getter, Seq<string> values) =>
        from index in ValidIndex(index: getter.Option().CurrentListOptionIndex, count: values.Count, error: Op.Of(name: nameof(CommandOption)).InvalidResult())
        select Snapshot(name: name, getter: getter, value: Some((object)values[index]), listIndex: Some(index));

    private static Fin<CommandOptionValue> SnapshotAt<T>(string name, GetBaseClass getter, Seq<T> values) =>
        from index in ValidIndex(index: getter.Option().CurrentListOptionIndex, count: values.Count, error: Op.Of(name: nameof(CommandOption)).InvalidResult())
        select Snapshot(name: name, getter: getter, value: Some((object)values[index]!), listIndex: Some(index));

    private static Fin<CommandOptionValue> SnapshotAt<TEnum>(string name, GetBaseClass getter, Seq<TEnum> values, bool selection) where TEnum : struct, Enum =>
        (selection switch {
            false => Try.lift<TEnum>(f: getter.GetSelectedEnumValue<TEnum>).Run(),
            true => Try.lift<TEnum>(f: () => getter.GetSelectedEnumValueFromSelectionList(values.AsIterable())).Run(),
        })
            .MapFail(static _ => Op.Of(name: nameof(CommandOption)).InvalidResult())
            .Map(selected => Snapshot(
                name: name,
                getter: getter,
                value: Some((object)selected),
                listIndex: EnumIndex(values: values, value: selected)));

    private static Fin<Seq<TValue>> NonEmpty<TValue>(Seq<TValue> values) =>
        values.IsEmpty switch {
            true => Fin.Fail<Seq<TValue>>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
            false => Fin.Succ(value: values),
        };

    private static Fin<int> ValidIndex(int index, int count, Error error) =>
        index switch {
            >= 0 when index < count => Fin.Succ(value: index),
            _ => Fin.Fail<int>(error: error),
        };

    private static Option<int> EnumIndex<TEnum>(Seq<TEnum> values, TEnum value) where TEnum : struct, Enum =>
        toSeq(Enumerable.Range(start: 0, count: values.Count))
            .Find(index => EqualityComparer<TEnum>.Default.Equals(x: values[index], y: value));

    internal sealed class Scope : IDisposable {
        private bool disposed;

        internal Scope(Seq<CommandOption.Bound> bounds) => Bounds = bounds;

        internal Seq<CommandOption.Bound> Bounds { get; }

        internal Fin<CommandOptionValue> Selected(GetBaseClass getter) =>
            Bounds
                .Find(bound => bound.Index == getter.OptionIndex())
                .ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidResult())
                .Bind(bound => bound.Capture(getter));

        public void Dispose() {
            _ = disposed switch {
                true => unit,
                false => Bounds.Iter(static bound => bound.Release()),
            };
            disposed = true;
            GC.SuppressFinalize(obj: this);
        }
    }
}
