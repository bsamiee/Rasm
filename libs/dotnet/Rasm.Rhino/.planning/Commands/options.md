# [RASM_RHINO_OPTIONS]

`OptionSet.Bind` admits and binds one command-line vocabulary inside the getter window. `OptionLease` owns every native carrier from first construction through selected-value projection and deterministic release, including partial-bind failure.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `NameGrammar`/`OptionName`, `OptionKind`, `OptionTrait`, `NumericBand<T>`, `OptionChoices`, `OptionValue`, and the `IEnumBinding`/`TypedEnumBinding<TEnum>` existential pair.
- [03]-[CASE_ALGEBRA]: `OptionValue.Bind` and `Decode` — the native carrier transfer and the settings-token grammar.
- [04]-[LEASE]: `OptionRow`, `OptionMark`, `OptionEvidence`, `OptionSetting`/`OptionChoice`, `OptionSet`, and the `OptionLease` capsule.
- [05]-[BOUNDARY]: the published-surface, decode-ownership, and command-thread carves.
- [06]-[RESEARCH]: open verification rows.

## [02]-[VOCABULARY]

`OptionName` owns script-stable English identity and localized display under one grammar row: the host publishes two admission oracles — one for option names, one for option value names — and the row selecting between them is a required construction argument, so a value name in an option-name slot refuses at admission rather than by type. `OptionValue` closes bare, toggle, numeric, text, colour, list, and enum-backed modalities; each case carries the evidence its native binder and settings grammar require, and declares the traits it admits.

- Law: the three policy axes a bound option carries — hidden registration, varying value, empty-text admission — are COMBINABLE membership over one vocabulary, not three two-row classes with one bool getter each. `OptionValue.Admissible` is the per-case legal set, so `AllowsEmpty` on a colour row and `Hidden` on a toggle refuse at the row rather than being written into a host call that ignores them.
- Law: admission accumulates through `FactoryValidation`; a bad band and blank prompt produce one generated refusal naming both defects.
- Law: identity is CANONICAL at intake. The host matches option names case-insensitively, so `OptionName.Key` is the one uppercase form the slot vocabulary, the roster distinctness test, and every lookup index read; a per-probe `StringComparer.OrdinalIgnoreCase` argument is the deleted form.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[ComplexValueObject]`, `[Union]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `[MemberEqualityComparer<TAccessor, TMember>]`, `[KeyMemberEqualityComparer<TAccessor, TKey>]`, `ComparerAccessors`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`, `Map`, `Atom`, `Traverse`/`TraverseM`/`FoldM`); Generator.Equals (`api-generator-equals.md` — `[Equatable]`, `[OrderedEquality]`, `[IgnoreEquality]`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`), `Domain/results` (`HostEdge.Side`, `ValidityClaim`), `Numerics/atoms` (`PerceptualColor.OfArgb`); `Document/session` (`DraftFault`); RhinoCommon commands (`Rasm.Rhino/.api/api-rhinocommon-commands.md:155-202` — `AddOption*`, `AddOptionEnum*`, `GetSelectedEnumValue*`, `SetOptionVaries`, `IsValidOptionName`/`IsValidOptionValueName`, the `CommandLineOption` reads, `ToggleValues`, `ListOptions`); `Rhino.UI.LocalizeStringPair` (`api-rhino-ui.md`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class NameGrammar {
    public static readonly NameGrammar Option = new(admits: CommandLineOption.IsValidOptionName);
    public static readonly NameGrammar Value = new(admits: CommandLineOption.IsValidOptionValueName);

    [UseDelegateFromConstructor]
    internal partial bool Admits(string english);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class OptionName {
    public NameGrammar Grammar { get; }

    [MemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    public string English { get; }

    public Option<string> Local { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref NameGrammar grammar,
        ref string english,
        ref Option<string> local) {
        NameGrammar held = grammar;
        string candidate = english;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (held is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Grammar) }))),
                (string.IsNullOrWhiteSpace(candidate),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(English) }))),
                (held is not null && !string.IsNullOrWhiteSpace(candidate) && !held.Admits(candidate),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(NameGrammar.Admits), candidate }))),
                (local.Exists(string.IsNullOrWhiteSpace),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Local) })))));
    }

    internal string Key => English.ToUpperInvariant();

    internal global::Rhino.UI.LocalizeStringPair Native => new(english: English, local: Local.IfNone(English));
}

[SmartEnum<int>]
public sealed partial class OptionKind {
    public static readonly OptionKind Simple = new(key: (int)CommandLineOptionType.Simple, display: static _ => Seq<string>());
    public static readonly OptionKind Number = new(key: (int)CommandLineOptionType.Number, display: static _ => Seq<string>());
    public static readonly OptionKind Toggle = new(key: (int)CommandLineOptionType.Toggle, display: static native => {
        native.ToggleValues(english: false, offValue: out string off, onValue: out string on);
        return Seq(off, on);
    });
    public static readonly OptionKind Color = new(key: (int)CommandLineOptionType.Color, display: static _ => Seq<string>());
    public static readonly OptionKind List = new(key: (int)CommandLineOptionType.List, display: static native => toSeq(native.ListOptions(english: false)));
    public static readonly OptionKind Hidden = new(key: (int)CommandLineOptionType.Hidden, display: static _ => Seq<string>());

    internal CommandLineOptionType Native => (CommandLineOptionType)Key;

    [UseDelegateFromConstructor]
    internal partial Seq<string> Display(CommandLineOption native);

    internal static Fin<OptionKind> Of(CommandLineOptionType native) =>
        FactoryBridge.Row<int, OptionKind>((int)native);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OptionTrait : ICapability<OptionTrait> {
    public static readonly OptionTrait Hidden = new(key: "hidden");
    public static readonly OptionTrait Varies = new(key: "varies");
    public static readonly OptionTrait AllowsEmpty = new(key: "allows-empty");
}

[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct NumericBand<T> where T : struct, INumber<T> {
    public Option<T> Lower { get; }
    public Option<T> Upper { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<T> lower,
        ref Option<T> upper) {
        Option<T> low = lower;
        Option<T> high = upper;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (low.Exists(static bound => !T.IsFinite(bound)) || high.Exists(static bound => !T.IsFinite(bound)),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(NumericBand<T>), 0d, "finite bounds" }))),
                ((low.Case, high.Case) is (T minimum, T maximum) && minimum > maximum,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(NumericBand<T>), 0d, "a lower bound at or below the upper" })))));
    }

    public bool Contains(T value) => T.IsFinite(value)
        && Lower.ForAll(bound => value >= bound)
        && Upper.ForAll(bound => value <= bound);

    internal TNative Carrier<TNative>(
        T seed,
        Func<T, TNative> free,
        Func<T, bool, T, TNative> half,
        Func<T, T, T, TNative> closed) => (Lower.Case, Upper.Case) switch {
            (T lower, T upper) => closed(seed, lower, upper),
            (T lower, _) => half(seed, true, lower),
            (_, T upper) => half(seed, false, upper),
            _ => free(seed),
        };
}

[ComplexValueObject]
[ValidationError]
public sealed partial class OptionChoices {
    public Seq<OptionName> Values { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<OptionName> values) {
        Seq<OptionName> rows = values;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (rows.IsEmpty, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Values) }))),
                (rows.Exists(static value => value is null),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(OptionChoices) }))),
                (rows.ForAll(static value => value is not null)
                    && rows.Map(static value => value.Key).Distinct().Count != rows.Count,
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Values), "distinct option value names" })))));
    }

    internal int Count => Values.Count;

    internal Option<int> IndexOf(string token) => Values
        .Map(static (value, index) => (value.Key, Seat: index))
        .Find(entry => string.Equals(entry.Key, token.ToUpperInvariant(), StringComparison.Ordinal))
        .Map(static entry => entry.Seat);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionValue {
    private OptionValue() { }
    public sealed record Verb(Option<OptionName> Display) : OptionValue;
    public sealed record Toggle(bool Current, OptionName Off, OptionName On) : OptionValue;
    public sealed record Number(double Current, NumericBand<double> Band, Option<string> Prompt = default) : OptionValue;
    public sealed record Count(int Current, NumericBand<int> Band, Option<string> Prompt = default) : OptionValue;
    public sealed record Text(string Current, Option<string> Prompt = default) : OptionValue;
    public sealed record Paint(PerceptualColor Current, Option<string> Prompt = default) : OptionValue;
    public sealed record Pick(OptionChoices Values, int Current) : OptionValue;
    public sealed record EnumChoice(IEnumBinding Binding) : OptionValue;

    internal CapabilitySet<OptionTrait> Admissible => Switch(
        verb: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Hidden, OptionTrait.Varies),
        toggle: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies),
        number: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies),
        count: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies),
        text: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.AllowsEmpty, OptionTrait.Varies),
        paint: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies),
        pick: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies),
        enumChoice: static _ => CapabilitySet<OptionTrait>.Of(OptionTrait.Varies));

    internal Seq<ValidationClause> Clauses() => Switch(
        verb: static _ => Seq<ValidationClause>(),
        toggle: static (row) => FactoryValidation.Violated(
            (row.Off is null || row.On is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Toggle) }))),
            (row.Off is not null && row.On is not null && string.Equals(row.Off.Key, row.On.Key, StringComparison.Ordinal),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Toggle), "distinct off and on names" })))),
        number: static (row) => FactoryValidation.Violated(
            (!row.Band.Contains(row.Current), () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Number), row.Current, "a seed inside the band" }))),
            (row.Prompt.Exists(string.IsNullOrWhiteSpace),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Number.Prompt) })))),
        count: static (row) => FactoryValidation.Violated(
            (!row.Band.Contains(row.Current), () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Count), row.Current, "a seed inside the band" }))),
            (row.Prompt.Exists(string.IsNullOrWhiteSpace),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Count.Prompt) })))),
        text: static (row) => FactoryValidation.Violated(
            (row.Current is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Text) }))),
            (row.Prompt.Exists(string.IsNullOrWhiteSpace),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Text.Prompt) })))),
        paint: static (row) => FactoryValidation.Violated(
            (row.Current is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Paint) }))),
            (row.Prompt.Exists(string.IsNullOrWhiteSpace),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Paint.Prompt) })))),
        pick: static (row) => FactoryValidation.Violated(
            (row.Values is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Pick) }))),
            (row.Values is not null && (row.Current < 0 || row.Current >= row.Values.Count),
                () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Pick), row.Current, "a seat inside the roster" })))),
        enumChoice: static (row) => FactoryValidation.Violated(
            (row.Binding is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(EnumChoice) })))));

    internal Fin<Unit> Admit() => FactoryValidation.Admit(Clauses());

    internal Fin<OptionValue> Read(Option<CommandLineOption> native, Func<Fin<OptionValue>> carrier) =>
        native.Bind(selected => Published(selected))
            .IfNone(() => Try.lift(carrier).Run().Bind(static inner => inner))
            .Bind(candidate => candidate.Admit().Map(_ => candidate));

    private Option<Fin<OptionValue>> Published(CommandLineOption native) => Switch(
        state: native,
        verb: static (_, _) => Option<Fin<OptionValue>>.None,
        toggle: static (held, row) => Some(Optional(held.CurrentToggleValue)
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Map<OptionValue>(value => row with { Current = value })),
        number: static (held, row) => Some(Fin.Succ<OptionValue>(row with { Current = held.CurrentNumericValue })),
        count: static (held, row) => Some(held.CurrentNumericValue is var value
            && ValidityClaim.All(
                ValidityClaim.Finite(value), value >= int.MinValue, value <= int.MaxValue, value == Math.Truncate(value)).Holds
                ? Fin.Succ<OptionValue>(row with { Current = checked((int)value) })
                : Fin.Fail<OptionValue>(new KernelFault.InvalidResult())),
        text: static (held, row) => Some(Optional(held.StringOptionValue)
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Map<OptionValue>(value => row with { Current = value })),
        paint: static (_, _) => Option<Fin<OptionValue>>.None,
        pick: static (held, row) => Some(Fin.Succ<OptionValue>(row with { Current = held.CurrentListOptionIndex })),
        enumChoice: static (_, _) => Option<Fin<OptionValue>>.None);

    public static Fin<OptionValue> OfEnum<TEnum>(
        TEnum current,
        Option<Seq<TEnum>> selection = default)
        where TEnum : struct, Enum, IConvertible =>
        TypedEnumBinding<TEnum>.Of(current: current, selection: selection)
            .Map(static binding => (OptionValue)new EnumChoice(Binding: binding));
}

public interface IEnumBinding {
    string Current { get; }
    Fin<int> Seat(GetBaseClass getter, OptionName name);
    Fin<IEnumBinding> Read(GetBaseClass getter);
    Fin<IEnumBinding> Decode(string token);
}

public sealed class TypedEnumBinding<TEnum> : IEnumBinding where TEnum : struct, Enum, IConvertible {
    private readonly Option<Seq<TEnum>> selection;
    private readonly (TEnum Value, string Name) current;

    private TypedEnumBinding(Option<Seq<TEnum>> selection, (TEnum Value, string Name) current) =>
        (this.selection, this.current) = (selection, current);

    public string Current => current.Name;

    private Seq<TEnum> Roster => selection.IfNone(Declared);

    internal static Fin<IEnumBinding> Of(TEnum current, Option<Seq<TEnum>> selection) {
        Seq<TEnum> rows = selection.IfNone(Declared);
        return from _ in guard(
                   !rows.IsEmpty && rows.Distinct().Count == rows.Count,
                   new KernelFault.InvalidInput(Axis: Some(nameof(selection)))).ToFin()
               from named in rows.TraverseM(value => Named(value)).As()
               from seated in Named(current)
               from __ in guard(named.Exists(row => row.Value.Equals(seated.Value)), new KernelFault.InvalidInput())
               select (IEnumBinding)new TypedEnumBinding<TEnum>(selection: selection, current: seated);
    }

    public Fin<int> Seat(GetBaseClass getter, OptionName name) => selection.Match(
        Some: rows => rows.Map(static (item, index) => (item, index))
            .Find(entry => entry.item.Equals(current.Value))
            .Map(static entry => entry.index)
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Bind(index => Try.lift(() => Fin.Succ(value: getter.AddOptionEnumSelectionList(
                englishOptionName: name.English,
                enumSelection: rows,
                listCurrentIndex: index))).Run().Bind(static inner => inner)),
        None: () => Try.lift(() => Fin.Succ(value: getter.AddOptionEnumList(
            englishOptionName: name.English,
            defaultValue: current.Value))).Run().Bind(static inner => inner));

    public Fin<IEnumBinding> Read(GetBaseClass getter) =>
        Try.lift(() => Fin.Succ(value: selection.Match(
                Some: rows => getter.GetSelectedEnumValueFromSelectionList(selectionList: rows),
                None: getter.GetSelectedEnumValue<TEnum>))).Run().Bind(static inner => inner)
            .Bind(value => Seated(value: value));

    public Fin<IEnumBinding> Decode(string token) =>
        Enum.TryParse(value: token, ignoreCase: true, result: out TEnum parsed)
            ? Seated(value: parsed)
            : Fin.Fail<IEnumBinding>(error: new KernelFault.InvalidInput());

    private Fin<IEnumBinding> Seated(TEnum value) =>
        from named in Named(value)
        from _ in guard(Roster.Exists(item => item.Equals(named.Value)), new KernelFault.InvalidInput())
        select (IEnumBinding)new TypedEnumBinding<TEnum>(selection: selection, current: named);

    private static Fin<(TEnum Value, string Name)> Named(TEnum value) =>
        Enum.GetName(value) is string name
            && Enum.TryParse(name, ignoreCase: false, out TEnum roundTrip)
            && roundTrip.Equals(value)
                ? Fin.Succ((Value: value, Name: name))
                : Fin.Fail<(TEnum, string)>(new KernelFault.InvalidInput(Axis: Some(typeof(TEnum).Name)));

    private static Seq<TEnum> Declared => toSeq(Enum.GetValues<TEnum>()).Strict();
}
```

## [03]-[CASE_ALGEBRA]

`OptionValue.Bind` constructs one bound row and transfers each disposable carrier immediately into the supplied lease. `Decode` mirrors the same family for settings tokens; the colour grammar accepts invariant `RRGGBB` and `AARRGGBB` hexadecimal words.

- Law: the prompt arity is ONE decision. Four host carrier families each publish a bare and a prompted registration overload, and the choice between them is the presence of the row's prompt — so `Prompted` names it once and each carrier site is one expression instead of the same two-arm switch written four times.
- Law: the colour token is HEX TEXT and the kernel owner admits a packed ARGB word, so the six-digit branch fills opaque alpha and both lengths terminate in ONE `PerceptualColor.OfArgb` — no `System.Drawing.Color` is minted only to be re-read, which is the round trip a hand-rolled component fold used to sit inside.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public abstract partial record OptionValue {
    internal Fin<BoundOption> Bind(
        GetBaseClass getter,
        OptionName name,
        CapabilitySet<OptionTrait> traits,
        OptionLease lease) => Switch(
        state: (Getter: getter, Name: name, Traits: traits, Lease: lease),
        verb: static (held, row) => Try.lift(() => Fin.Succ(new BoundOption(
            Index: row.Display.Case switch {
                OptionName display => held.Getter.AddOption(
                    held.Name.Native, display.Native, held.Traits.Admits(capability: OptionTrait.Hidden)),
                _ => held.Getter.AddOption(held.Name.Native),
            },
            Current: () => Fin.Succ<OptionValue>(value: row)))).Run().Bind(static inner => inner),
        toggle: static (held, row) => Try.lift(() => {
            OptionToggle native = new(initialValue: row.Current, offValue: row.Off.Native, onValue: row.On.Native);
            held.Lease.Own(native);
            int index = held.Getter.AddOptionToggle(optionName: held.Name.Native, toggleValue: ref native);
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }).Run().Bind(static inner => inner),
        number: static (held, row) => Try.lift(() => {
            OptionDouble native = row.Band.Carrier(
                seed: row.Current,
                free: static seed => new OptionDouble(seed),
                half: static (seed, lower, bound) => new OptionDouble(seed, lower, bound),
                closed: static (seed, lower, upper) => new OptionDouble(seed, lower, upper));
            held.Lease.Own(native);
            int index = Prompted(
                row.Prompt,
                bare: () => held.Getter.AddOptionDouble(held.Name.Native, ref native),
                titled: prompt => held.Getter.AddOptionDouble(held.Name.Native, ref native, prompt));
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }).Run().Bind(static inner => inner),
        count: static (held, row) => Try.lift(() => {
            OptionInteger native = row.Band.Carrier(
                seed: row.Current,
                free: static seed => new OptionInteger(seed),
                half: static (seed, lower, bound) => new OptionInteger(seed, lower, bound),
                closed: static (seed, lower, upper) => new OptionInteger(seed, lower, upper));
            held.Lease.Own(native);
            int index = Prompted(
                row.Prompt,
                bare: () => held.Getter.AddOptionInteger(held.Name.Native, ref native),
                titled: prompt => held.Getter.AddOptionInteger(held.Name.Native, ref native, prompt));
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }).Run().Bind(static inner => inner),
        text: static (held, row) => Try.lift(() => {
            OptionString native = new(
                initialString: row.Current,
                allowEmptyString: held.Traits.Admits(capability: OptionTrait.AllowsEmpty));
            held.Lease.Own(native);
            int index = Prompted(
                row.Prompt,
                bare: () => held.Getter.AddOptionString(held.Name.Native, ref native),
                titled: prompt => held.Getter.AddOptionString(held.Name.Native, ref native, prompt));
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }).Run().Bind(static inner => inner),
        paint: static (held, row) =>
            from color in Slots.Rgb(shade: row.Current)
            from bound in Try.lift(() => {
                OptionColor native = new(initialValue: color);
                held.Lease.Own(native);
                int index = Prompted(
                    row.Prompt,
                    bare: () => held.Getter.AddOptionColor(held.Name.Native, ref native),
                    titled: prompt => held.Getter.AddOptionColor(held.Name.Native, ref native, prompt));
                return Fin.Succ(new BoundOption(
                    Index: index,
                    Current: () => Slots.Shade(color: native.CurrentValue)
                        .Map(shade => (OptionValue)(row with { Current = shade }))));
            }).Run().Bind(static inner => inner)
            select bound,
        pick: static (held, row) => Try.lift(() => Fin.Succ(new BoundOption(
            Index: held.Getter.AddOptionList(
                optionName: held.Name.Native,
                listValues: row.Values.Values.Map(static value => value.Native).AsIterable(),
                listCurrentIndex: row.Current),
            Current: () => Fin.Succ<OptionValue>(value: row)))).Run().Bind(static inner => inner),
        enumChoice: static (held, row) => row.Binding.Seat(getter: held.Getter, name: held.Name)
            .Map(index => new BoundOption(
                Index: index,
                Current: () => row.Binding.Read(getter: held.Getter)
                    .Map(binding => (OptionValue)(row with { Binding = binding })))));

    internal Fin<OptionValue> Decode(string token) => Switch(
        state: token,
        verb: static (_, row) => Fin.Succ<OptionValue>(row),
        toggle: static (held, row) => held.ToUpperInvariant() switch {
            var value when string.Equals(value, row.On.Key, StringComparison.Ordinal) =>
                Fin.Succ<OptionValue>(row with { Current = true }),
            var value when string.Equals(value, row.Off.Key, StringComparison.Ordinal) =>
                Fin.Succ<OptionValue>(row with { Current = false }),
            _ => Fin.Fail<OptionValue>(new KernelFault.InvalidInput()),
        },
        number: static (held, row) => double.TryParse(
            held, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && row.Band.Contains(value)
            ? Fin.Succ<OptionValue>(row with { Current = value })
            : Fin.Fail<OptionValue>(new KernelFault.InvalidInput()),
        count: static (held, row) => int.TryParse(
            held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && row.Band.Contains(value)
            ? Fin.Succ<OptionValue>(row with { Current = value })
            : Fin.Fail<OptionValue>(new KernelFault.InvalidInput()),
        text: static (held, row) => Fin.Succ<OptionValue>(row with { Current = held }),
        paint: static (held, row) => DecodeColor(token: held)
            .Map(value => (OptionValue)(row with { Current = value })),
        pick: static (held, row) => row.Values.IndexOf(held).Match(
            Some: index => Fin.Succ<OptionValue>(row with { Current = index }),
            None: () => int.TryParse(held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                && index >= 0 && index < row.Values.Count
                ? Fin.Succ<OptionValue>(row with { Current = index })
                : Fin.Fail<OptionValue>(new KernelFault.InvalidInput())),
        enumChoice: static (held, row) => row.Binding.Decode(token: held)
            .Map(binding => (OptionValue)(row with { Binding = binding })));

    private static int Prompted(Option<string> prompt, Func<int> bare, Func<string, int> titled) =>
        prompt.Match(Some: titled, None: bare);

    private static Fin<PerceptualColor> DecodeColor(string token) {
        ReadOnlySpan<char> digits = token.AsSpan().Trim().TrimStart('#');
        return digits.Length is 6 or 8
            && uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value)
                ? PerceptualColor.OfArgb(
                    packed: unchecked((int)(digits.Length is 6 ? value | 0xFF00_0000u : value)))
                : Fin.Fail<PerceptualColor>(error: new KernelFault.InvalidInput());
    }
}
```

## [04]-[LEASE]

- Law: the capsule is ONE atom. Bound rows, owned carriers, accumulated faults, and the lease state are columns of one immutable record swapped as a unit, so a release can never observe a half-appended carrier roster and drop the newest carrier — the exact leak the lease exists to prevent. Three disciplines guarding one lifetime — a bare field, an `Interlocked` flag, a second `Atom` — is the deleted form.
- Law: the bound roster is KEYED by the native option index, so the selected-option probe and the ordered snapshot read one authority — an ordered map answers both, where a sequence beside a linear scan answered neither well.
- Law: `AcquireOutcome` carries two DIFFERENT option facts. `OptionChoice` is the touch history — one entry per option cycle, in the order the user drove them, carrying the localized display the host published at that moment. `OptionSetting` is the settled state — every bound option's final value, read once at seal. A consumer folding the history to recover the settled state re-derives what the snapshot already answers.
- Boundary: `OptionSet.Seeded` is the SETTINGS re-seat, not a second scripted-token parser. The host's own getter parses macro tokens inside its loop and publishes them through `Result()`/`Option()` exactly as it does an interactive pick; `Seeded` re-seats a vocabulary from persisted text BEFORE any getter exists, which no host member answers.

`OptionLease` exists before the first bind and receives each carrier as it is created. Any failed row releases the partial lease; success returns the same capsule to acquisition. One `OptionValue.Read` projection admits pointer-backed values for both the selected-option answer and the settled snapshot before detached evidence leaves the getter window; one `OptionMark` threads the shared native identity through the evidence.

`Dispose` accumulates into `Faults` and returns. Acquisition consumes the lease under a `using` nested inside the getter's own, so a throwing cleanup unwinds past the acquisition result and replaces the in-flight answer with a release fault — the caller loses the value it obtained. `Release` remains the result-typed entry a caller reads when cleanup evidence matters.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record OptionRow(
    OptionName Name,
    OptionValue Value,
    CapabilitySet<OptionTrait> Traits) : ISlotted<string> {
    public string SlotKey => Name.Key;

    internal Fin<Unit> Admit() => Name is null || Value is null
        ? Fin.Fail<Unit>(new KernelFault.InvalidValue(nameof(OptionRow), string.Join(" | ", new object?[] { key, "an option row" })))
        : FactoryValidation.Admit(Value.Clauses()
            + FactoryValidation.Violated(
                (!Value.Admissible.AdmitsAll(Traits),
                    () => new ValidationClause(string.Join(" | ", new object?[] { key, nameof(Traits), $"traits this modality reads; unread <{Value.Admissible.Missing(Traits).Wire}>" }))),
                (Value is OptionValue.Verb { Display.IsNone: true } && Traits.Admits(capability: OptionTrait.Hidden),
                    () => new ValidationClause(string.Join(" | ", new object?[] { key, nameof(OptionTrait.Hidden), "a display token beside a hidden verb" }))),
                (Value is OptionValue.Text { Current.Length: 0 } && !Traits.Admits(capability: OptionTrait.AllowsEmpty),
                    () => new ValidationClause(string.Join(" | ", new object?[] { key, nameof(OptionValue.Text), "empty text only under the empty-text trait" })))));
}

public sealed record OptionMark(int NativeIndex, OptionKind Kind, string English, string Local);

[Equatable]
public sealed partial record OptionEvidence(
    OptionMark Mark,
    [property: OrderedEquality] Seq<string> Display);

public sealed record OptionSetting(OptionName Name, OptionValue Value);

public sealed record OptionChoice(OptionSetting Setting, OptionEvidence Evidence);

public sealed record OptionAssignment(string Name, string Token);

internal sealed record BoundOption(int Index, Func<Fin<OptionValue>> Current);

[Equatable]
public sealed partial record OptionSet {
    private OptionSet(RulePlan<OptionRow, string> plan, FrozenDictionary<string, OptionRow> index) =>
        (Plan, this.index) = (plan, index);

    [IgnoreEquality]
    private readonly FrozenDictionary<string, OptionRow> index;

    internal RulePlan<OptionRow, string> Plan { get; }

    public Seq<OptionRow> Rows => Plan.Rules;

    public static Fin<OptionSet> Of(Seq<OptionRow> rows) {
        return from _ in guard(!rows.IsEmpty, new KernelFault.InvalidInput(Axis: Some(nameof(rows)))).ToFin()
               from plan in RulePlan<OptionRow, string>.Of(
                   rules: rows, admit: static (row, k) => row.Admit(k))
               select new OptionSet(
                   plan: plan,
                   index: plan.Rules.ToFrozenDictionary(static row => row.SlotKey, StringComparer.Ordinal));
    }

    internal Option<OptionRow> Find(string name) =>
        index.TryGetValue(name.ToUpperInvariant(), out OptionRow? row) ? Some(row) : None;

    public Fin<OptionLease> Bind(GetBaseClass getter) {
        OptionLease lease = new();
        return guard(RhinoApp.IsOnMainThread && getter is not null, new KernelFault.InvalidContext()).ToFin()
            .Bind(_ => Plan.Apply(
                target: getter,
                apply: (row, target, k) =>
                    row.Value.Bind(getter: target, name: row.Name, traits: row.Traits, lease: lease, key: k)
                        .Bind(bound => {
                            lease.Attach(row: row, bound: bound);
                            return Try.lift(() => {
                                target.SetOptionVaries(
                                    optionIndex: bound.Index,
                                    varies: row.Traits.Admits(capability: OptionTrait.Varies));
                                return Fin.Succ(unit);
                            }).Run().Bind(static inner => inner);
                        })))
            .Map(_ => lease)
            .Rollback(release: () => lease.Release());
    }

    public Fin<OptionSet> Seeded(Seq<OptionAssignment> assignments) {
        return assignments
            .FoldM<Fin, Map<string, OptionValue>>(Map<string, OptionValue>(), (held, assignment) =>
                from name in Acceptance.Text(assignment.Name)
                from token in Admit.Need(assignment.Token)
                from row in Find(name).ToFin(Fail: new KernelFault.MissingContext())
                from value in row.Value.Decode(token: token)
                select held.AddOrUpdate(row.SlotKey, value))
            .As()
            .Bind(seeded => Of(
                rows: Rows.Map(row => seeded.Find(row.SlotKey).Match(
                    Some: value => row with { Value = value },
                    None: () => row))));
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LeaseState {
    private LeaseState() { }
    internal sealed record Open : LeaseState;
    internal sealed record Released : LeaseState;
}

public sealed class OptionLease : IDisposable {
    private readonly Atom<LeaseBooks> books = Atom(value: LeaseBooks.Empty);

    private sealed record LeaseBooks(
        Map<int, (OptionRow Row, BoundOption Bound)> Bound,
        Seq<IDisposable> Resources,
        Seq<IDisposable> Draining,
        Seq<Error> Faults,
        LeaseState State) {
        internal static readonly LeaseBooks Empty = new(
            Bound: Map<int, (OptionRow, BoundOption)>(),
            Resources: Seq<IDisposable>(),
            Draining: Seq<IDisposable>(),
            Faults: Seq<Error>(),
            State: new LeaseState.Open());
    }

    internal Unit Own(IDisposable resource) =>
        ignore(books.Swap(f: held => held with { Resources = held.Resources.Add(value: resource) }));

    internal OptionLease Attach(OptionRow row, BoundOption boundOption) {
        _ = books.Swap(f: held => held with {
            Bound = held.Bound.AddOrUpdate(boundOption.Index, (row, boundOption)),
        });
        return this;
    }

    public Fin<OptionChoice> Selected(GetBaseClass getter) {
        return from _ in AdmitLive()
               from choice in Try.lift(() =>
                   from __ in guard(getter is not null && getter.Result() is GetResult.Option, new KernelFault.InvalidResult()).ToFin()
                   from native in Optional(getter.Option()).ToFin(Fail: new KernelFault.InvalidResult())
                   from entry in books.Value.Bound.Find(getter.OptionIndex()).ToFin(Fail: new KernelFault.InvalidResult())
                   from current in entry.Row.Value.Read(native: Some(native), carrier: entry.Bound.Current)
                   from kind in OptionKind.Of(native: native.OptionType)
                   select new OptionChoice(
                       Setting: new OptionSetting(Name: entry.Row.Name, Value: current),
                       Evidence: new OptionEvidence(
                           Mark: new OptionMark(native.Index, kind, native.EnglishName, native.LocalName),
                           Display: kind.Display(native: native)))).Run().Bind(static inner => inner)
               select choice;
    }

    internal Fin<Seq<OptionSetting>> Snapshot() => Try.lift(() =>
        from _ in AdmitLive()
        from settled in toSeq(books.Value.Bound.Values)
            .TraverseM(entry => entry.Row.Value.Read(native: None, carrier: entry.Bound.Current)
                .Map(value => new OptionSetting(Name: entry.Row.Name, Value: value)))
            .As()
        select settled.Strict()).Run().Bind(static inner => inner);

    private Fin<Unit> AdmitLive() => guard(
        flag: RhinoApp.IsOnMainThread && books.Value.State is LeaseState.Open,
        False: new KernelFault.InvalidContext()).ToFin();

    internal Fin<Unit> Release() {
        LeaseBooks settled = books.Swap(f: static held => held.State is LeaseState.Released
            ? held with { Draining = Seq<IDisposable>() }
            : held with {
                Bound = Map<int, (OptionRow, BoundOption)>(),
                Resources = Seq<IDisposable>(),
                Draining = held.Resources,
                State = new LeaseState.Released(),
            });
        return Custody.Release(
            held: settled.Draining,
            release: resource => Try.lift(() => Fin.Succ(value: HostEdge.Side(resource.Dispose))).Run().Bind(static inner => inner));
    }

    public Seq<Error> Faults => books.Value.Faults;

    public void Dispose() => _ = Release().Match(
        Succ: static released => released,
        Fail: fault => ignore(books.Swap(f: held => held with { Faults = held.Faults.Add(value: fault) })));
}
```

## [05]-[BOUNDARY]

`OptionSet`, `OptionRow`, `OptionValue`, `OptionLease.Selected`, and `OptionSet.Seeded` are the PUBLISHED surface — a command body in the `apps/<app>/` plugin shell authors the vocabulary and hands it to `Acquire`, so a corpus-wide caller census answers zero for them exactly as it does for `Acquisition.Get`. Every INTERNAL member is reached from `Commands/acquisition`: `Bind` and `Release` from the drive's bracket ladder, `Selected` from its option cycle, and `Snapshot` from its seal.

`OptionLease` owns every `ref`-taking host carrier — `OptionToggle`, `OptionDouble`, `OptionInteger`, `OptionString`, `OptionColor` — from construction to release, and no carrier, `CommandLineOption`, or `GetBaseClass` handle leaves the getter window. `Rhino.UI.LocalizeStringPair` is a host localization carrier and stays here; the kernel interaction plane owns Eto control shapes and takes none of it.

`NumericBand<T>` stays folder-local by ruling: the kernel `Bound`/`Band` rows are `double` named-range admission vocabulary, and a generic host-option interval over `INumber<T>` is a different concept with no kernel counterpart.

The command-thread carve: `RhinoApp.IsOnMainThread` at `Bind` and at every live-lease read is Rhino's COMMAND-thread affinity — a different axis than the kernel marshal, whose `UiThread`/`UiDispatch` owner sits at S0 below this page, and the two are different threads by construction on Windows.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
