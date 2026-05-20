using Color = System.Drawing.Color;

namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandOptionValue {
    internal CommandOptionValue(
        int index,
        string key,
        string name,
        Option<object> value,
        Option<int> listIndex,
        CommandLineOptionType optionType,
        Option<string> englishName,
        Option<string> localName,
        Option<string> stringValue) {
        Index = index;
        Key = key;
        Name = name;
        Value = value;
        ListIndex = listIndex;
        OptionType = optionType;
        EnglishName = englishName;
        LocalName = localName;
        StringValue = stringValue;
    }

    internal int Index { get; }
    public string Key { get; }
    public string Name { get; }
    public Option<object> Value { get; }
    public Option<int> ListIndex { get; }
    public CommandLineOptionType OptionType { get; }
    public Option<string> EnglishName { get; }
    public Option<string> LocalName { get; }
    public Option<string> StringValue { get; }

    public Option<T> As<T>() => Value.Bind(static value => value is T typed ? Some(typed) : Option<T>.None);
    public Option<T> As<T>(string key) => Is(key: key) ? As<T>() : Option<T>.None;
    public Fin<T> Require<T>() => As<T>().ToFin(Fail: Op.Of(name: nameof(CommandOptionValue)).InvalidResult());
    public Fin<T> Require<T>(string key) => As<T>(key: key).ToFin(Fail: Op.Of(name: nameof(CommandOptionValue)).InvalidResult());
    public bool Is(string key) => string.Equals(a: Key, b: key, comparisonType: StringComparison.Ordinal);
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

    private delegate int RefOptionBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native, Option<string> prompt) where TNative : IDisposable;
    private delegate int RefOptionPlainBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native) where TNative : IDisposable;
    private delegate int RefOptionPromptBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native, string prompt) where TNative : IDisposable;

    public string Name { get; }

    public static CommandOption Of<T>(string name, T value, CommandOptionPolicy policy = default) =>
        value switch {
            bool toggle => Toggle(name: name, initial: toggle, off: policy.Off, on: policy.On, localName: Optional(policy.LocalName), varies: policy.Varies),
            double number => Number(name: name, initial: number, lower: policy.Lower, upper: policy.Upper, prompt: policy.Prompt, localName: Optional(policy.LocalName), varies: policy.Varies),
            int integer => IntegerBounds(lower: policy.Lower, upper: policy.Upper).Case switch {
                (Option<int> lo, Option<int> hi) => Integer(name: name, initial: integer, lower: lo, upper: hi, prompt: policy.Prompt, localName: Optional(policy.LocalName), varies: policy.Varies),
                _ => Invalid(name: name),
            },
            string text => Text(name: name, initial: text, allowEmpty: policy.AllowEmpty, prompt: policy.Prompt, localName: Optional(policy.LocalName), varies: policy.Varies),
            Color color => Color(name: name, initial: color, prompt: policy.Prompt, localName: Optional(policy.LocalName), varies: policy.Varies),
            IEnumerable<string> values => List(name: name, values: toSeq(values), current: policy.Current, localName: Optional(policy.LocalName), varies: policy.Varies),
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

    public static CommandOption EnumSelection<TEnum>(string name, IEnumerable<TEnum> values, TEnum selected, CommandOptionPolicy policy = default) where TEnum : struct, Enum {
        Seq<TEnum> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<TEnum>());
        return EnumIndex(values: source, value: selected).Case switch {
            int current => EnumSelection(name: name, values: source, current: current, varies: policy.Varies),
            _ => Invalid(name: name),
        };
    }

    public static CommandOption List(string name, IEnumerable<string> values, string selected, CommandOptionPolicy policy = default) {
        Seq<string> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<string>());
        return IndexOf(values: source, selected: selected, same: StringComparer.Ordinal.Equals).Case switch {
            int current => List(name: name, values: source, current: current, localName: Optional(policy.LocalName), varies: policy.Varies),
            _ => Invalid(name: name),
        };
    }

    public static CommandOption List<T>(string name, IEnumerable<T> values, Func<T, string> label, T selected, CommandOptionPolicy policy = default) {
        Seq<T> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<T>());
        Option<int> index = IndexOf(values: source, selected: selected, same: EqualityComparer<T>.Default.Equals);
        return (Optional(label).Case, index.Case) switch {
            (Func<T, string> project, int current) => List(name: name, values: source, label: project, current: current, localName: Optional(policy.LocalName), varies: policy.Varies),
            _ => Invalid(name: name),
        };
    }

    private static Case NamedOption(string name, string? value = null, bool hidden = false, bool varies = false, Option<string> localName = default, Option<string> localValue = default) =>
        new(Name: name, AddToGetter: (getter, validName) => value switch {
            string v => ValidValue(value: v).Bind(valid => Added(getter: getter, index: (localName.Case, localValue.Case) switch {
                (string ln, string lv) => getter.AddOption(new global::Rhino.UI.LocalizeStringPair(english: validName, local: ln), new global::Rhino.UI.LocalizeStringPair(english: valid, local: lv), hidden),
                _ => getter.AddOption(validName, valid, hidden),
            }, native: null, snapshot: g => Fin.Succ(value: Snapshot(key: validName, name: validName, getter: g, value: Some((object)valid), listIndex: Option<int>.None)), varies: varies)),
            _ => Added(getter: getter, index: localName.Case switch {
                string local => getter.AddOption(new global::Rhino.UI.LocalizeStringPair(english: validName, local: local)),
                _ => getter.AddOption(validName),
            }, native: null, snapshot: g => Fin.Succ(value: Snapshot(key: validName, name: validName, getter: g, value: Option<object>.None, listIndex: Option<int>.None)), varies: varies),
        }, ScriptToken: token =>
            ScriptPair(name: name, token: token).Map(pair => Scripted(key: name, value: Optional(value).Map(static valid => (object)valid), listIndex: Option<int>.None, optionType: value is null ? CommandLineOptionType.Simple : CommandLineOptionType.Hidden, stringValue: Some(pair.Value)))
            | ScriptName(name: name, token: token).Map(static key => Scripted(key: key, value: Option<object>.None, listIndex: Option<int>.None, optionType: CommandLineOptionType.Simple, stringValue: Option<string>.None)));

    private static Case Invalid(string name) =>
        new(Name: name, AddToGetter: static (_, _) => Fin.Fail<Bound>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()));

    private static Case Toggle(string name, bool initial, string off = "No", string on = "Yes", Option<string> localName = default, bool varies = false) =>
        Ref(name: name, create: () => (ValidValue(value: off), ValidValue(value: on)).Apply((disabled, enabled) => new OptionToggle(initial, disabled, enabled)).As(), prompt: Option<string>.None, localName: localName, bind: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionToggle native, Option<string> _) => getter.AddOptionToggle(name, ref native), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => Bool(value: pair.Value, off: off, on: on).Map(value => Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Toggle, stringValue: Some(pair.Value)))), varies: varies);
    private static Case Number(string name, double initial, Option<double> lower = default, Option<double> upper = default, string? prompt = null, Option<string> localName = default, bool varies = false) =>
        Ref(name: name, create: () => BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (double value) => new OptionDouble(value), single: static (double value, bool isLower, double bound) => new OptionDouble(value, isLower, bound), bounded: static (double value, double lo, double hi) => new OptionDouble(value, lo, hi)), prompt: Optional(prompt), localName: localName, bind: Prompted(plain: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionDouble native) => getter.AddOptionDouble(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionDouble native, string label) => getter.AddOptionDouble(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => double.TryParse(s: pair.Value, style: System.Globalization.NumberStyles.Float, provider: System.Globalization.CultureInfo.InvariantCulture, result: out double parsed) ? ValidateBounds(initial: parsed, lower: lower, upper: upper).ToOption().Map(_ => Scripted(key: name, value: Some((object)parsed), listIndex: Option<int>.None, optionType: CommandLineOptionType.Number, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: varies);
    private static Case Integer(string name, int initial, Option<int> lower = default, Option<int> upper = default, string? prompt = null, Option<string> localName = default, bool varies = false) =>
        Ref(name: name, create: () => BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static (int value) => new OptionInteger(value), single: static (int value, bool isLower, int bound) => new OptionInteger(value, isLower, bound), bounded: static (int value, int lo, int hi) => new OptionInteger(value, lo, hi)), prompt: Optional(prompt), localName: localName, bind: Prompted(plain: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionInteger native) => getter.AddOptionInteger(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionInteger native, string label) => getter.AddOptionInteger(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => int.TryParse(s: pair.Value, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int parsed) ? ValidateBounds(initial: parsed, lower: lower, upper: upper).ToOption().Map(_ => Scripted(key: name, value: Some((object)parsed), listIndex: Option<int>.None, optionType: CommandLineOptionType.Number, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: varies);
    private static Case Text(string name, string initial = "", bool allowEmpty = false, string? prompt = null, Option<string> localName = default, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionString(initial, allowEmpty)), prompt: Optional(prompt), localName: localName, bind: Prompted(plain: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionString native) => getter.AddOptionString(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionString native, string label) => getter.AddOptionString(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => (!string.IsNullOrEmpty(value: pair.Value) || allowEmpty) ? Some(Scripted(key: name, value: Some((object)pair.Value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Simple, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: varies);
    private static Case Color(string name, Color initial, string? prompt = null, Option<string> localName = default, bool varies = false) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionColor(initial)), prompt: Optional(prompt), localName: localName, bind: Prompted(plain: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionColor native) => getter.AddOptionColor(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionColor native, string label) => getter.AddOptionColor(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => global::System.Drawing.Color.FromName(name: pair.Value) switch { global::System.Drawing.Color value when value.IsKnownColor || value.IsNamedColor => Some(Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Color, stringValue: Some(pair.Value))), _ => Option<CommandOptionValue>.None }), varies: varies);
    private static Case List(string name, Seq<string> values, int current = 0, Option<string> localName = default, bool varies = false) =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from valid in values.TraverseM(ValidValue).As()
            from nonEmpty in NonEmpty(values: valid)
            from index in ValidIndex(index: current, count: nonEmpty.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput())
            from bound in Added(getter: getter, index: getter.AddOptionList(OptionName(english: validName, localName: localName), nonEmpty.Map(static value => new global::Rhino.UI.LocalizeStringPair(english: value, local: value)).AsIterable(), index), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: nonEmpty), varies: varies)
            select bound,
            ScriptToken: token =>
                from pair in ScriptPair(name: name, token: token)
                from index in IndexOf(values: values, selected: pair.Value, same: StringComparer.Ordinal.Equals)
                select Scripted(key: name, value: Some((object)values[index]), listIndex: Some(index), optionType: CommandLineOptionType.List, stringValue: Some(pair.Value)));
    private static Case List<T>(string name, Seq<T> values, Func<T, string> label, int current = 0, Option<string> localName = default, bool varies = false) =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from items in values.TraverseM(value => Optional(value).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())).As()
            from labels in items.TraverseM(value => Optional(label(arg: value)).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput()).Bind(ValidValue)).As()
            from nonEmpty in NonEmpty(values: items)
            from index in ValidIndex(index: current, count: items.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput())
            from bound in Added(getter: getter, index: getter.AddOptionList(OptionName(english: validName, localName: localName), labels.Map(static value => new global::Rhino.UI.LocalizeStringPair(english: value, local: value)).AsIterable(), index), native: null, snapshot: g => SnapshotAt(name: validName, getter: g, values: nonEmpty), varies: varies)
            select bound,
            ScriptToken: token =>
                from pair in ScriptPair(name: name, token: token)
                from project in Optional(label)
                from index in toSeq(Enumerable.Range(start: 0, count: values.Count)).Find(index => string.Equals(a: project(arg: values[index]), b: pair.Value, comparisonType: StringComparison.Ordinal) || string.Equals(a: values[index]?.ToString(), b: pair.Value, comparisonType: StringComparison.Ordinal))
                select Scripted(key: name, value: Some((object)values[index]!), listIndex: Some(index), optionType: CommandLineOptionType.List, stringValue: Some(pair.Value)));
    private static Case EnumList<TEnum>(string name, TEnum initial, Seq<TEnum> values = default, bool varies = false) where TEnum : struct, Enum =>
        EnumOption(name: name, values: values, initial: Some(initial), current: Option<int>.None, varies: varies);
    private static Case EnumSelection<TEnum>(string name, Seq<TEnum> values, int current = 0, bool varies = false) where TEnum : struct, Enum =>
        EnumOption<TEnum>(name: name, values: values, initial: Option<TEnum>.None, current: Some(current), varies: varies);

    private static Case Ref<TNative, TValue>(string name, Func<Fin<TNative>> create, Option<string> prompt, Option<string> localName, RefOptionBinder<TNative> bind, Func<TNative, TValue> current, Func<string, Option<CommandOptionValue>> script, bool varies) where TNative : IDisposable =>
        new(Name: name, AddToGetter: (getter, validName) => create().Bind(native => {
            int index = bind(getter, OptionName(english: validName, localName: localName), ref native, prompt);
            return Added(getter: getter, index: index, native: native, snapshot: g => Fin.Succ(value: Snapshot(key: validName, name: validName, getter: g, value: Some((object)current(arg: native)!), listIndex: Option<int>.None)), varies: varies);
        }), ScriptToken: script);

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
            select bound,
            ScriptToken: token =>
                from pair in ScriptPair(name: name, token: token)
                from selected in ParseEnum<TEnum>(value: pair.Value)
                from index in EnumIndex(values: values.IsEmpty ? toSeq(global::System.Enum.GetValues<TEnum>()) : values, value: selected)
                select Scripted(key: name, value: Some((object)selected), listIndex: Some(index), optionType: CommandLineOptionType.List, stringValue: Some(pair.Value)));

    internal abstract Fin<Bound> Add(GetBaseClass getter);
    internal abstract Option<CommandOptionValue> Script(string token);

    internal static Option<CommandOptionValue> Script(Seq<CommandOption> options, string token) =>
        options.Choose(option => option.Script(token: token)).Find(static _ => true);

    private sealed record Case(string Name, Func<GetBaseClass, string, Fin<Bound>> AddToGetter, Func<string, Option<CommandOptionValue>>? ScriptToken = null) : CommandOption(name: Name) {
        internal override Fin<Bound> Add(GetBaseClass getter) =>
            from name in guard(CommandLineOption.IsValidOptionName(optionName: Name), Op.Of(name: nameof(CommandOption)).InvalidInput())
                .Bind(_ => Fin.Succ(value: Name))
            from bound in AddToGetter(arg1: getter, arg2: name)
            select bound;

        internal override Option<CommandOptionValue> Script(string token) =>
            Optional(ScriptToken).Bind(project => project(arg: token));
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
            .Bind(g => options.Map(static option => option.Name).Distinct().Count == options.Count ? options.TraverseM(option => option.Add(getter: g)).As() : Fin.Fail<Seq<Bound>>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()))
            .Map(static bounds => new Scope(bounds: bounds));

    private static Fin<TNative> BoundedOption<TValue, TNative>(
        TValue initial,
        Option<TValue> lower,
        Option<TValue> upper,
        Func<TValue, TNative> unconstrained,
        Func<TValue, bool, TValue, TNative> single,
        Func<TValue, TValue, TValue, TNative> bounded) where TValue : IComparable<TValue> =>
        from valid in ValidateBounds(initial: initial, lower: lower, upper: upper)
        select (valid.Lower, valid.Upper) switch {
            ( { IsSome: true } lo, { IsSome: true } hi) => bounded(arg1: initial, arg2: lo.IfNone(initial), arg3: hi.IfNone(initial)),
            ( { IsSome: true } lo, _) => single(arg1: initial, arg2: true, arg3: lo.IfNone(initial)),
            (_, { IsSome: true } hi) => single(arg1: initial, arg2: false, arg3: hi.IfNone(initial)),
            _ => unconstrained(arg: initial),
        };

    private static Option<(Option<int> Lower, Option<int> Upper)> IntegerBounds(Option<double> lower, Option<double> upper) =>
        (IntegerBound(value: lower), IntegerBound(value: upper)) switch {
            (Option<int> lo, Option<int> hi) when lower.IsSome == lo.IsSome && upper.IsSome == hi.IsSome => Some((Lower: lo, Upper: hi)),
            _ => Option<(Option<int> Lower, Option<int> Upper)>.None,
        };

    private static Option<int> IntegerBound(Option<double> value) =>
        value.Case switch {
            double bound when bound >= int.MinValue && bound <= int.MaxValue && Math.Truncate(d: bound) == bound => Some((int)bound),
            _ => Option<int>.None,
        };

    private static Fin<(Option<TValue> Lower, Option<TValue> Upper)> ValidateBounds<TValue>(TValue initial, Option<TValue> lower, Option<TValue> upper) where TValue : IComparable<TValue> =>
        (lower, upper) switch {
            (Option<TValue> lo, Option<TValue> hi) when lo.Case is TValue left && hi.Case is TValue right && left.CompareTo(other: right) > 0 => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
            (Option<TValue> lo, _) when lo.Case is TValue left && initial.CompareTo(other: left) < 0 => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
            (_, Option<TValue> hi) when hi.Case is TValue right && initial.CompareTo(other: right) > 0 => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: Op.Of(name: nameof(CommandOption)).InvalidInput()),
            _ => Fin.Succ(value: (Lower: lower, Upper: upper)),
        };

    private static RefOptionBinder<TNative> Prompted<TNative>(RefOptionPlainBinder<TNative> plain, RefOptionPromptBinder<TNative> prompted) where TNative : IDisposable =>
        (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native, Option<string> prompt) => prompt.Case switch {
            string label => prompted(getter, name, ref native, label),
            _ => plain(getter, name, ref native),
        };

    private static global::Rhino.UI.LocalizeStringPair OptionName(string english, Option<string> localName) =>
        localName.Case switch {
            string local => new(english: english, local: local),
            _ => new(english: english, local: english),
        };

    private static Fin<string> ValidValue(string value) =>
        guard(CommandLineOption.IsValidOptionValueName(optionValue: value), Op.Of(name: nameof(CommandOption)).InvalidInput())
            .Bind(_ => Fin.Succ(value: value));

    private static CommandOptionValue Scripted(string key, Option<object> value, Option<int> listIndex, CommandLineOptionType optionType, Option<string> stringValue) =>
        new(
            index: -1,
            key: key,
            name: key,
            value: value,
            listIndex: listIndex,
            optionType: optionType,
            englishName: Some(key),
            localName: Option<string>.None,
            stringValue: stringValue);

    private static Option<(string Key, string Value)> ScriptPair(string name, string token) =>
        Optional(token)
            .Map(static value => value.Trim())
            .Bind(value => ((Equal: value.IndexOf(value: '=', comparisonType: StringComparison.Ordinal), Colon: value.IndexOf(value: ':', comparisonType: StringComparison.Ordinal)) switch {
                (int equal and > 0, int colon and > 0) => equal < colon ? Some(equal) : Some(colon),
                (int equal and > 0, _) => Some(equal),
                (_, int colon and > 0) => Some(colon),
                _ => Option<int>.None,
            }).Bind(index => string.Equals(a: value[..index], b: name, comparisonType: StringComparison.Ordinal) ? Some((Key: name, Value: value[(index + 1)..].Trim())) : Option<(string Key, string Value)>.None));

    private static Option<string> ScriptName(string name, string token) =>
        Optional(token).Map(static value => value.Trim()).Bind(value => string.Equals(a: value, b: name, comparisonType: StringComparison.Ordinal) ? Some(name) : Option<string>.None);

    private static Option<bool> Bool(string value, string off, string on) =>
        value switch {
            string text when string.Equals(a: text, b: on, comparisonType: StringComparison.OrdinalIgnoreCase) => Some(true),
            string text when string.Equals(a: text, b: off, comparisonType: StringComparison.OrdinalIgnoreCase) => Some(false),
            string text when string.Equals(a: text, b: "1", comparisonType: StringComparison.Ordinal) || string.Equals(a: text, b: "true", comparisonType: StringComparison.OrdinalIgnoreCase) => Some(true),
            string text when string.Equals(a: text, b: "0", comparisonType: StringComparison.Ordinal) || string.Equals(a: text, b: "false", comparisonType: StringComparison.OrdinalIgnoreCase) => Some(false),
            _ => Option<bool>.None,
        };

    private static Option<TEnum> ParseEnum<TEnum>(string value) where TEnum : struct, Enum =>
        global::System.Enum.TryParse(value: value, ignoreCase: true, result: out TEnum selected) ? Some(selected) : Option<TEnum>.None;

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

    private static CommandOptionValue Snapshot(string key, string name, GetBaseClass getter, Option<object> value, Option<int> listIndex) {
        CommandLineOption option = getter.Option();
        return new(
            index: getter.OptionIndex(),
            key: key,
            name: name,
            value: value,
            listIndex: listIndex,
            optionType: option.OptionType,
            englishName: Optional(option.EnglishName),
            localName: Optional(option.LocalName),
            stringValue: Optional(option.StringOptionValue));
    }

    private static Fin<CommandOptionValue> SnapshotAt<T>(string name, GetBaseClass getter, Seq<T> values) =>
        from index in ValidIndex(index: getter.Option().CurrentListOptionIndex, count: values.Count, error: Op.Of(name: nameof(CommandOption)).InvalidResult())
        select Snapshot(key: name, name: name, getter: getter, value: Some((object)values[index]!), listIndex: Some(index));

    private static Fin<CommandOptionValue> SnapshotAt<TEnum>(string name, GetBaseClass getter, Seq<TEnum> values, bool selection) where TEnum : struct, Enum =>
        (selection switch {
            false => Try.lift<TEnum>(f: getter.GetSelectedEnumValue<TEnum>).Run(),
            true => Try.lift<TEnum>(f: () => getter.GetSelectedEnumValueFromSelectionList(values.AsIterable())).Run(),
        })
            .MapFail(static _ => Op.Of(name: nameof(CommandOption)).InvalidResult())
            .Map(selected => Snapshot(
                key: name,
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
        IndexOf(values: values, selected: value, same: EqualityComparer<TEnum>.Default.Equals);

    private static Option<int> IndexOf<T>(Seq<T> values, T selected, Func<T, T, bool> same) =>
        Optional(same).Bind(compare => toSeq(Enumerable.Range(start: 0, count: values.Count)).Find(index => compare(arg1: values[index], arg2: selected)));

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
