using Color = System.Drawing.Color;

namespace Rasm.Rhino.Commands;

// --- [MODELS] -----------------------------------------------------------------------------
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
    public bool Is(string key) => string.Equals(a: Key, b: key, comparisonType: StringComparison.OrdinalIgnoreCase);
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
    bool AllowEmpty = false,
    int Current = 0,
    Option<double> Lower = default,
    Option<double> Upper = default);

public abstract record CommandOption {
    private CommandOption(string name) => Name = name;

    private delegate int RefOptionBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native, Option<string> prompt) where TNative : IDisposable;
    private delegate int RefOptionPlainBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native) where TNative : IDisposable;
    private delegate int RefOptionPromptBinder<TNative>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref TNative native, string prompt) where TNative : IDisposable;

    public string Name { get; }

    public static CommandOption Of<T>(string name, T value, CommandOptionPolicy policy = default) =>
        value switch {
            bool toggle => Toggle(name: name, initial: toggle, policy: policy),
            double number => Number(name: name, initial: number, policy: policy),
            int integer => Integer(name: name, initial: integer, policy: policy),
            string text => Text(name: name, initial: text, policy: policy),
            Color color => Color(name: name, initial: color, policy: policy),
            IEnumerable<string> values => Choice(name: name, values: values, label: static value => value, policy: policy),
            _ => Invalid(name: name),
        };

    public static CommandOption Named(string name, CommandOptionPolicy policy = default) =>
        NamedOption(name: name, value: policy.ValueName, hidden: policy.Hidden, varies: policy.Varies, localName: Optional(policy.LocalName), localValue: Optional(policy.LocalValueName));

    public static CommandOption Choice<T>(
        string name,
        IEnumerable<T> values,
        Func<T, string> label,
        Option<T> selected = default,
        CommandOptionPolicy policy = default) {
        Seq<T> source = Optional(values).Map(static items => toSeq(items)).IfNone(Seq<T>());
        Option<Func<T, string>> project = Optional(label);
        Option<int> current =
            selected.Bind(value => toSeq(Enumerable.Range(start: 0, count: source.Count))
                .Find(index => EqualityComparer<T>.Default.Equals(source[index], value))) |
            Some(policy.Current);
        return (project.Case, current.Case) switch {
            (Func<T, string> labels, int index) => ListOption(name: name, values: source, label: labels, current: index, localName: typeof(T).IsEnum ? Option<string>.None : Optional(policy.LocalName), varies: policy.Varies, enumerated: typeof(T).IsEnum),
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
    private static Case Toggle(string name, bool initial, CommandOptionPolicy policy) =>
        Ref(name: name, create: () => (ValidValue(value: policy.Off), ValidValue(value: policy.On)).Apply((disabled, enabled) => new OptionToggle(initial, disabled, enabled)).As(), prompt: Option<string>.None, localName: Optional(policy.LocalName), bind: static (getter, name, ref native, _) => getter.AddOptionToggle(name, ref native), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => BoolValue(value: pair.Value, off: policy.Off, on: policy.On).Map(value => Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Toggle, stringValue: Some(pair.Value)))), varies: policy.Varies);
    private static Case Number(string name, double initial, CommandOptionPolicy policy) =>
        Ref(name: name, create: () => BoundedOption(initial: initial, lower: policy.Lower, upper: policy.Upper, unconstrained: static value => new OptionDouble(value), single: static (value, isLower, bound) => new OptionDouble(value, isLower, bound), bounded: static (value, lo, hi) => new OptionDouble(value, lo, hi)), prompt: Optional(policy.Prompt), localName: Optional(policy.LocalName), bind: Prompted(plain: static (getter, name, ref native) => getter.AddOptionDouble(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionDouble native, string label) => getter.AddOptionDouble(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => double.TryParse(s: pair.Value, style: System.Globalization.NumberStyles.Float, provider: System.Globalization.CultureInfo.InvariantCulture, result: out double parsed) ? Limit(lower: policy.Lower, upper: policy.Upper).Accept(value: parsed).Map(value => Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Number, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: policy.Varies);
    private static Case Integer(string name, int initial, CommandOptionPolicy policy) =>
        IntegerBounds(lower: policy.Lower, upper: policy.Upper).Case switch {
            (Option<int> lower, Option<int> upper) =>
                Ref(name: name, create: () => BoundedOption(initial: initial, lower: lower, upper: upper, unconstrained: static value => new OptionInteger(value), single: static (value, isLower, bound) => new OptionInteger(value, isLower, bound), bounded: static (value, lo, hi) => new OptionInteger(value, lo, hi)), prompt: Optional(policy.Prompt), localName: Optional(policy.LocalName), bind: Prompted(plain: static (getter, name, ref native) => getter.AddOptionInteger(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionInteger native, string label) => getter.AddOptionInteger(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => int.TryParse(s: pair.Value, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int parsed) ? Limit(lower: lower, upper: upper).Accept(value: parsed).Map(value => Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Number, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: policy.Varies),
            _ => Invalid(name: name),
        };
    private static Case Text(string name, string initial, CommandOptionPolicy policy) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionString(initial, policy.AllowEmpty)), prompt: Optional(policy.Prompt), localName: Optional(policy.LocalName), bind: Prompted(plain: static (getter, name, ref native) => getter.AddOptionString(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionString native, string label) => getter.AddOptionString(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => (!string.IsNullOrEmpty(value: pair.Value) || policy.AllowEmpty) ? Some(Scripted(key: name, value: Some((object)pair.Value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Simple, stringValue: Some(pair.Value))) : Option<CommandOptionValue>.None), varies: policy.Varies);
    private static Case Color(string name, Color initial, CommandOptionPolicy policy) =>
        Ref(name: name, create: () => Fin.Succ(value: new OptionColor(initial)), prompt: Optional(policy.Prompt), localName: Optional(policy.LocalName), bind: Prompted(plain: static (getter, name, ref native) => getter.AddOptionColor(name, ref native), prompted: static (GetBaseClass getter, global::Rhino.UI.LocalizeStringPair name, ref OptionColor native, string label) => getter.AddOptionColor(name, ref native, label)), current: static native => native.CurrentValue, script: token => ScriptPair(name: name, token: token).Bind(pair => ColorValue(text: pair.Value).Map(value => Scripted(key: name, value: Some((object)value), listIndex: Option<int>.None, optionType: CommandLineOptionType.Color, stringValue: Some(pair.Value)))), varies: policy.Varies);
    private static Case ListOption<T>(string name, Seq<T> values, Func<T, string> label, int current, Option<string> localName, bool varies, bool enumerated) {
        Func<GetBaseClass, global::Rhino.UI.LocalizeStringPair, Seq<T>, Seq<string>, int, int> bind = enumerated ? EnumBind : PlainBind;
        return ListCore(name: name, values: values, label: label, current: current, localName: localName, varies: varies, bind: bind, capture: SnapshotList);
    }

    private static int PlainBind<T>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair localized, Seq<T> _, Seq<string> labels, int index) =>
        getter.AddOptionList(localized, labels.Map(static value => new global::Rhino.UI.LocalizeStringPair(english: value, local: value)).AsIterable(), index);

    private static int EnumBind<T>(GetBaseClass getter, global::Rhino.UI.LocalizeStringPair localized, Seq<T> items, Seq<string> _, int index) {
        T[] include = [.. items];
        // BOUNDARY ADAPTER — native AddOptionEnumList<T> is constrained `where T : struct, IConvertible`, but Choice<T> is unconstrained (it also serves string); the enum write crosses the constraint via dynamic. Runs once per options bind, never on a per-event path.
        return index >= 0 && index < items.Count ? getter.AddOptionEnumList(englishOptionName: localized.English, defaultValue: (dynamic)items[index]!, include: include) : -1;
    }

    private static Case ListCore<T>(string name, Seq<T> values, Func<T, string> label, int current, Option<string> localName, bool varies, Func<GetBaseClass, global::Rhino.UI.LocalizeStringPair, Seq<T>, Seq<string>, int, int> bind, Func<string, GetBaseClass, Seq<T>, Fin<CommandOptionValue>> capture) =>
        new(Name: name, AddToGetter: (getter, validName) =>
            from items in values.TraverseM(value => Optional(value).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())).As()
            from labels in items.TraverseM(value => Optional(label(arg: value)).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput()).Bind(ValidValue)).As()
            from nonEmpty in NonEmpty(values: items)
            from index in ValidIndex(index: current, count: nonEmpty.Count, error: Op.Of(name: nameof(CommandOption)).InvalidInput())
            from bound in Added(getter: getter, index: bind(arg1: getter, arg2: OptionName(english: validName, localName: localName), arg3: nonEmpty, arg4: labels, arg5: index), native: null, snapshot: g => capture(arg1: validName, arg2: g, arg3: nonEmpty), varies: varies)
            select bound,
            ScriptToken: token =>
                from pair in ScriptPair(name: name, token: token)
                from index in toSeq(Enumerable.Range(start: 0, count: values.Count)).Find(i => string.Equals(a: label(arg: values[i]), b: pair.Value, comparisonType: StringComparison.Ordinal) || string.Equals(a: values[i]?.ToString(), b: pair.Value, comparisonType: StringComparison.Ordinal))
                select Scripted(key: name, value: Some((object)values[index]!), listIndex: Some(index), optionType: CommandLineOptionType.List, stringValue: Some(pair.Value)));
    private static Case Ref<TNative, TValue>(string name, Func<Fin<TNative>> create, Option<string> prompt, Option<string> localName, RefOptionBinder<TNative> bind, Func<TNative, TValue> current, Func<string, Option<CommandOptionValue>> script, bool varies) where TNative : IDisposable =>
        new(Name: name, AddToGetter: (getter, validName) => create().Bind(native => {
            int index = bind(getter, OptionName(english: validName, localName: localName), ref native, prompt);
            return Added(getter: getter, index: index, native: native, snapshot: g => Fin.Succ(value: Snapshot(key: validName, name: validName, getter: g, value: Some((object)current(arg: native)!), listIndex: Option<int>.None)), varies: varies);
        }), ScriptToken: script);

    internal abstract Fin<Bound> Add(GetBaseClass getter);
    internal abstract Option<CommandOptionValue> Script(string token);
    internal static Option<CommandOptionValue> Script(Seq<CommandOption> options, string token) =>
        options.Choose(option => option.Script(token: token)).Head;
    internal static Fin<Seq<CommandOption>> Validate(Seq<CommandOption> options, Op op) {
        CommandOption[] rows = [.. options];
        return rows.All(static option => option is not null && CommandLineOption.IsValidOptionName(optionName: option.Name))
            && rows.Select(static option => option.Name).Distinct(comparer: StringComparer.OrdinalIgnoreCase).Count() == rows.Length
            ? Fin.Succ(value: options)
            : Fin.Fail<Seq<CommandOption>>(error: op.InvalidInput());
    }
    internal static Option<Color> ColorValue(string text) =>
        Optional(text)
            .Map(static value => value.Trim())
            .Bind(static value => value switch {
                string hex when HexColor(text: hex).Case is Color color => Some(color),
                string csv when CsvColor(text: csv).Case is Color color => Some(color),
                string named when named.Length > 0 && char.IsLetter(c: named[0]) && Enum.TryParse(value: named, ignoreCase: true, result: out System.Drawing.KnownColor known) => Some(System.Drawing.Color.FromKnownColor(color: known)),
                _ => Option<Color>.None,
            });

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
            .Bind(g => Validate(options: options, op: Op.Of(name: nameof(CommandOption)))
                .Bind(active => active.Fold(Fin.Succ(value: Seq<Bound>()), (state, option) =>
                    state.Bind(bounds => option.Add(getter: g).Match(
                        Succ: bound => Fin.Succ(value: bounds + Seq(bound)),
                        Fail: error => {
                            _ = bounds.Iter(static bound => bound.Release());
                            return Fin.Fail<Seq<Bound>>(error: error);
                        })))))
            .Map(static bounds => new Scope(bounds: bounds));

    private static Fin<TNative> BoundedOption<TValue, TNative>(
        TValue initial,
        Option<TValue> lower,
        Option<TValue> upper,
        Func<TValue, TNative> unconstrained,
        Func<TValue, bool, TValue, TNative> single,
        Func<TValue, TValue, TValue, TNative> bounded) where TValue : IComparable<TValue> =>
        from valid in Limit(lower: lower, upper: upper).Project<TValue>(op: Op.Of(name: nameof(CommandOption)))
        from accepted in Limit(lower: lower, upper: upper).Accept(value: initial).ToFin(Fail: Op.Of(name: nameof(CommandOption)).InvalidInput())
        select (valid.Lower, valid.Upper) switch {
            ( { IsSome: true } lo, { IsSome: true } hi) => bounded(arg1: accepted, arg2: lo.IfNone(accepted), arg3: hi.IfNone(accepted)),
            ( { IsSome: true } lo, _) => single(arg1: accepted, arg2: true, arg3: lo.IfNone(accepted)),
            (_, { IsSome: true } hi) => single(arg1: accepted, arg2: false, arg3: hi.IfNone(accepted)),
            _ => unconstrained(arg: accepted),
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

    private static CommandInputPolicy.LimitSpec Limit<TValue>(Option<TValue> lower, Option<TValue> upper) where TValue : IComparable<TValue> =>
        new(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: false, StrictlyUpper: false);

    private static RefOptionBinder<TNative> Prompted<TNative>(RefOptionPlainBinder<TNative> plain, RefOptionPromptBinder<TNative> prompted) where TNative : IDisposable =>
        (getter, name, ref native, prompt) => prompt.Case switch {
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
            }).Bind(index => string.Equals(a: value[..index], b: name, comparisonType: StringComparison.OrdinalIgnoreCase) ? Some((Key: name, Value: value[(index + 1)..].Trim())) : Option<(string Key, string Value)>.None));

    private static Option<string> ScriptName(string name, string token) =>
        Optional(token).Map(static value => value.Trim()).Bind(value => string.Equals(a: value, b: name, comparisonType: StringComparison.OrdinalIgnoreCase) ? Some(name) : Option<string>.None);

    internal static Option<bool> BoolValue(string value, string off = "No", string on = "Yes") =>
        value switch {
            string text when string.Equals(a: text, b: on, comparisonType: StringComparison.OrdinalIgnoreCase) => Some(value: true),
            string text when string.Equals(a: text, b: off, comparisonType: StringComparison.OrdinalIgnoreCase) => Some(value: false),
            string text when string.Equals(a: text, b: "1", comparisonType: StringComparison.Ordinal) || string.Equals(a: text, b: "true", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: text, b: "yes", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: text, b: "on", comparisonType: StringComparison.OrdinalIgnoreCase) => Some(value: true),
            string text when string.Equals(a: text, b: "0", comparisonType: StringComparison.Ordinal) || string.Equals(a: text, b: "false", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: text, b: "no", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: text, b: "off", comparisonType: StringComparison.OrdinalIgnoreCase) => Some(value: false),
            _ => Option<bool>.None,
        };

    private static Option<Color> HexColor(string text) =>
        Optional(text)
            .Map(static value => value.Trim())
            .Map(static value => value.StartsWith(value: "0x", comparisonType: StringComparison.OrdinalIgnoreCase) ? value[2..] : value.TrimStart(trimChar: '#'))
            .Bind(static value => value switch {
                string hex and { Length: 6 } when int.TryParse(s: hex, style: System.Globalization.NumberStyles.HexNumber, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int rgb) => Some(System.Drawing.Color.FromArgb(red: (rgb >> 16) & 255, green: (rgb >> 8) & 255, blue: rgb & 255)),
                string hex and { Length: 8 } when int.TryParse(s: hex, style: System.Globalization.NumberStyles.HexNumber, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int argb) => Some(System.Drawing.Color.FromArgb(alpha: (argb >> 24) & 255, red: (argb >> 16) & 255, green: (argb >> 8) & 255, blue: argb & 255)),
                _ => Option<Color>.None,
            });

    private static Option<Color> CsvColor(string text) =>
        text.Split(separator: ',', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) switch {
            [string r, string g, string b] when byte.TryParse(s: r, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte red) && byte.TryParse(s: g, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte green) && byte.TryParse(s: b, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte blue) => Some(System.Drawing.Color.FromArgb(red: red, green: green, blue: blue)),
            [string a, string r, string g, string b] when byte.TryParse(s: a, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte alpha) && byte.TryParse(s: r, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte red) && byte.TryParse(s: g, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte green) && byte.TryParse(s: b, style: System.Globalization.NumberStyles.Integer, provider: System.Globalization.CultureInfo.InvariantCulture, result: out byte blue) => Some(System.Drawing.Color.FromArgb(alpha: alpha, red: red, green: green, blue: blue)),
            _ => Option<Color>.None,
        };

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

    private static Fin<CommandOptionValue> SnapshotList<T>(string name, GetBaseClass getter, Seq<T> values) =>
        from index in ValidIndex(index: getter.Option().CurrentListOptionIndex, count: values.Count, error: Op.Of(name: nameof(CommandOption)).InvalidResult())
        select Snapshot(key: name, name: name, getter: getter, value: Some((object)values[index]!), listIndex: Some(index));

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

    internal sealed class Scope : IDisposable {
        private bool disposed;

        internal Scope(Seq<Bound> bounds) => Bounds = bounds;

        internal Seq<Bound> Bounds { get; }

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
