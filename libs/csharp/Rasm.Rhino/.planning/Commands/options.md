# [RASM_RHINO_OPTIONS]

`OptionSet.Bind` admits and binds one command-line vocabulary inside the getter window. `OptionLease` owns every native carrier from first construction through selected-value projection and deterministic release, including partial-bind failure.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `OptionLabel`/`OptionToken`, `OptionKind`, the policy rows, `NumericBand<T>`, `OptionValue`, and the `IEnumBinding`/`TypedEnumBinding<TEnum>` existential pair.
- [03]-[CASE_ALGEBRA]: `OptionValue.Bind` and `Decode` — the native carrier transfer and the scripted-token grammar.
- [04]-[LEASE]: `OptionRow`, `OptionMark`, `OptionEvidence`, `OptionChoice`, `OptionSet`, and the `OptionLease` capsule.
- [05]-[RESEARCH]: open verification rows.

## [02]-[VOCABULARY]

`OptionLabel` owns script-stable English identity and localized display. `OptionValue` closes bare, toggle, numeric, text, color, list, and enum-backed modalities; each case carries the evidence required by its native binder and scripted grammar.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [TYPES] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class OptionLabel {
    public string English { get; }
    public Option<string> Local { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string english,
        ref Option<string> local) {
        validationError = CommandLineOption.IsValidOptionName(optionName: english)
            ? validationError
            : new ValidationError(message: "option name is invalid");
    }

    internal global::Rhino.UI.LocalizeStringPair Native => new(english: English, local: Local.IfNone(English));
}

[ComplexValueObject]
public sealed partial class OptionToken {
    public string English { get; }
    public Option<string> Local { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string english,
        ref Option<string> local) {
        validationError = CommandLineOption.IsValidOptionValueName(optionValueName: english)
            ? validationError
            : new ValidationError(message: "option value is invalid");
    }

    internal global::Rhino.UI.LocalizeStringPair Native => new(english: English, local: Local.IfNone(English));
}

[SmartEnum<int>]
public sealed partial class OptionKind {
    public static readonly OptionKind Simple = new(key: (int)CommandLineOptionType.Simple);
    public static readonly OptionKind Number = new(key: (int)CommandLineOptionType.Number);
    public static readonly OptionKind Toggle = new(key: (int)CommandLineOptionType.Toggle);
    public static readonly OptionKind Color = new(key: (int)CommandLineOptionType.Color);
    public static readonly OptionKind List = new(key: (int)CommandLineOptionType.List);
    public static readonly OptionKind Hidden = new(key: (int)CommandLineOptionType.Hidden);

    internal CommandLineOptionType Native => (CommandLineOptionType)Key;

    internal static Fin<OptionKind> Of(CommandLineOptionType native, Op key) =>
        key.Row<int, OptionKind>((int)native);
}

[SmartEnum]
public sealed partial class OptionVisibility {
    public static readonly OptionVisibility Shown = new(isHidden: false);
    public static readonly OptionVisibility Hidden = new(isHidden: true);

    public bool IsHidden { get; }
}

[SmartEnum]
public sealed partial class OptionVariance {
    public static readonly OptionVariance Fixed = new(varies: false);
    public static readonly OptionVariance Variable = new(varies: true);

    public bool Varies { get; }
}

[SmartEnum]
public sealed partial class TextAdmission {
    public static readonly TextAdmission Required = new(allowEmpty: false);
    public static readonly TextAdmission Optional = new(allowEmpty: true);

    public bool AllowEmpty { get; }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct NumericBand<T> where T : struct, INumber<T> {
    public Option<T> Lower { get; }
    public Option<T> Upper { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<T> lower,
        ref Option<T> upper) {
        validationError = lower.Exists(static bound => !T.IsFinite(bound))
            || upper.Exists(static bound => !T.IsFinite(bound))
            ? new ValidationError(message: "numeric band contains a non-finite bound")
            : (lower.Case, upper.Case) is (T minimum, T maximum) && minimum > maximum
                ? new ValidationError(message: "numeric band is inverted")
                : validationError;
    }

    public bool Contains(T value) => T.IsFinite(value)
        && Lower.ForAll(bound => value >= bound)
        && Upper.ForAll(bound => value <= bound);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionValue {
    private OptionValue() { }
    public sealed record Verb(Option<OptionToken> Display, OptionVisibility Visibility) : OptionValue;
    public sealed record Toggle(bool Current, OptionToken Off, OptionToken On) : OptionValue;
    public sealed record Number(double Current, NumericBand<double> Band, Option<string> Prompt = default) : OptionValue;
    public sealed record Count(int Current, NumericBand<int> Band, Option<string> Prompt = default) : OptionValue;
    public sealed record Text(string Current, TextAdmission Admission, Option<string> Prompt = default) : OptionValue;
    public sealed record Paint(PerceptualColor Current, Option<string> Prompt = default) : OptionValue;
    public sealed record Pick(Seq<OptionToken> Values, int Current) : OptionValue;
    public sealed record EnumChoice(IEnumBinding Binding) : OptionValue;

    internal Fin<Unit> Admit(Op key) => Switch(
        verb: row => guard(row.Visibility is not null
            && (!row.Visibility.IsHidden || row.Display.IsSome), key.InvalidInput()).ToFin(),
        toggle: row => guard(row.Off is not null
            && row.On is not null
            && !string.Equals(row.Off.English, row.On.English, StringComparison.OrdinalIgnoreCase), key.InvalidInput()).ToFin(),
        number: row => from _ in guard(row.Band.Contains(row.Current), key.InvalidInput()).ToFin()
                       from __ in AdmitPrompt(row.Prompt, key)
                       select unit,
        count: row => from _ in guard(row.Band.Contains(row.Current), key.InvalidInput()).ToFin()
                      from __ in AdmitPrompt(row.Prompt, key)
                      select unit,
        text: row => from _ in guard(row.Current is not null
                          && row.Admission is not null
                          && (row.Current.Length > 0 || row.Admission.AllowEmpty), key.InvalidInput()).ToFin()
                     from __ in AdmitPrompt(row.Prompt, key)
                     select unit,
        paint: row => from _ in key.Need(row.Current)
                      from __ in AdmitPrompt(row.Prompt, key)
                      select unit,
        pick: row => guard(!row.Values.IsEmpty
            && row.Values.ForAll(static value => value is not null)
            && row.Values.Map(static value => value.English).Distinct(StringComparer.OrdinalIgnoreCase).Count == row.Values.Count
            && row.Current >= 0
            && row.Current < row.Values.Count, key.InvalidInput()).ToFin(),
        // The roster test, the seated value, and every operation moved INSIDE the binding, so admission here is
        // presence alone — a binding cannot exist carrying a value its own roster refuses.
        enumChoice: row => key.Need(row.Binding).Map(static _ => unit));

    // The fallback is RAILED because two of its producers can fail: an enum read-back must prove the host handed
    // a value the binding's roster admits, and a colour read-back must cross the kernel colour gate. A bare
    // `Func<OptionValue>` forced both to swallow or throw at exactly the point the option value is published.
    internal Fin<OptionValue> Read(Option<CommandLineOption> native, Func<Fin<OptionValue>> fallback, Op key) =>
        native.Match(
            Some: selected => Switch(
                (Native: selected, Fallback: fallback, Op: key),
                verb: static (held, _) => held.Op.Catch(held.Fallback),
                toggle: static (held, row) => Optional(held.Native.CurrentToggleValue)
                    .ToFin(Fail: held.Op.InvalidResult())
                    .Map<OptionValue>(value => row with { Current = value }),
                number: static (held, row) => Fin.Succ<OptionValue>(row with { Current = held.Native.CurrentNumericValue }),
                count: static (held, row) => held.Native.CurrentNumericValue is var value
                    && double.IsFinite(value)
                    && value >= int.MinValue
                    && value <= int.MaxValue
                    && value == Math.Truncate(value)
                        ? Fin.Succ<OptionValue>(row with { Current = checked((int)value) })
                        : Fin.Fail<OptionValue>(held.Op.InvalidResult()),
                text: static (held, row) => Optional(held.Native.StringOptionValue)
                    .ToFin(Fail: held.Op.InvalidResult())
                    .Map<OptionValue>(value => row with { Current = value }),
                paint: static (held, _) => held.Op.Catch(held.Fallback),
                pick: static (held, row) => Fin.Succ<OptionValue>(row with { Current = held.Native.CurrentListOptionIndex }),
                enumChoice: static (held, _) => held.Op.Catch(held.Fallback)),
            None: () => key.Catch(fallback))
        .Bind(candidate => candidate.Admit(key).Map(_ => candidate));

    private static Fin<Unit> AdmitPrompt(Option<string> prompt, Op key) => prompt.Match(
        Some: value => key.AcceptText(value).Map(static _ => unit),
        None: static () => Fin.Succ(unit));

    public static Fin<OptionValue> OfEnum<TEnum>(TEnum current, Option<Seq<TEnum>> include = default)
        where TEnum : struct, Enum, IConvertible =>
        TypedEnumBinding<TEnum>.Of(current: current, include: include, key: Op.Of(name: nameof(OfEnum)))
            .Map(static binding => (OptionValue)new EnumChoice(Binding: binding));
}

// The enum axis cannot be generic on `OptionValue` — the union is non-generic and one option set mixes enum types —
// so the binding is EXISTENTIAL: `IEnumBinding` is the erased face the union carries and `TypedEnumBinding<TEnum>`
// is its one implementation, keeping the roster, the seated value, the host bind, the read-back, and the token
// decode typed INSIDE itself. The erased form published five `object`-typed delegate columns whose contract no
// signature stated: a caller could hand `Admits` a foreign value, `Bind` a value of the wrong enum, or read back
// whatever `Read` returned without ever testing it against the roster. Nothing crosses this face untyped now.
public interface IEnumBinding {
    Type Type { get; }
    string Current { get; }
    Fin<int> Bind(GetBaseClass getter, OptionLabel label, Op key);
    Fin<IEnumBinding> Read(GetBaseClass getter, Op key);
    Fin<IEnumBinding> Decode(string token, Op key);
}

public sealed class TypedEnumBinding<TEnum> : IEnumBinding where TEnum : struct, Enum, IConvertible {
    private readonly Seq<TEnum> roster;
    private readonly bool restricted;
    private readonly TEnum current;

    private TypedEnumBinding(Seq<TEnum> roster, bool restricted, TEnum current) =>
        (this.roster, this.restricted, this.current) = (roster, restricted, current);

    public Type Type => typeof(TEnum);

    public string Current => Enum.GetName(current) ?? string.Empty;

    internal static Fin<IEnumBinding> Of(TEnum current, Option<Seq<TEnum>> include, Op key) {
        Seq<TEnum> rows = include.IfNone(toSeq(Enum.GetValues<TEnum>()).Strict());
        return guard(
                !rows.IsEmpty
                && rows.Distinct().Count == rows.Count
                && rows.ForAll(Declared)
                && rows.Exists(value => value.Equals(current)),
                key.InvalidInput()).ToFin()
            .Map(_ => (IEnumBinding)new TypedEnumBinding<TEnum>(
                roster: rows,
                restricted: include.IsSome,
                current: current));
    }

    public Fin<int> Bind(GetBaseClass getter, OptionLabel label, Op key) => restricted
        ? roster.Map(static (item, index) => (item, index))
            .Find(entry => entry.item.Equals(current))
            .Map(static entry => entry.index)
            .ToFin(Fail: key.InvalidResult())
            .Bind(index => key.Catch(() => Fin.Succ(value: getter.AddOptionEnumSelectionList(
                englishOptionName: label.English,
                enumSelection: roster,
                listCurrentIndex: index))))
        : key.Catch(() => Fin.Succ(value: getter.AddOptionEnumList(
            englishOptionName: label.English,
            defaultValue: current)));

    public Fin<IEnumBinding> Read(GetBaseClass getter, Op key) =>
        key.Catch(() => Fin.Succ(value: restricted
                ? getter.GetSelectedEnumValueFromSelectionList(selectionList: roster)
                : getter.GetSelectedEnumValue<TEnum>()))
            .Bind(value => Seated(value: value, key: key));

    public Fin<IEnumBinding> Decode(string token, Op key) =>
        Enum.TryParse(value: token, ignoreCase: true, result: out TEnum parsed)
            ? Seated(value: parsed, key: key)
            : Fin.Fail<IEnumBinding>(error: key.InvalidInput());

    // Every transition re-proves roster membership, so a host read-back outside a restricted selection list is a
    // typed refusal rather than a value the option silently publishes.
    private Fin<IEnumBinding> Seated(TEnum value, Op key) =>
        guard(roster.Exists(item => item.Equals(value)), key.InvalidInput()).ToFin()
            .Map(_ => (IEnumBinding)new TypedEnumBinding<TEnum>(
                roster: roster,
                restricted: restricted,
                current: value));

    private static bool Declared(TEnum value) =>
        Enum.GetName(value) is string name
            && Enum.TryParse(name, ignoreCase: false, out TEnum roundTrip)
            && roundTrip.Equals(value);
}

```

## [03]-[CASE_ALGEBRA]

`OptionValue.Bind` constructs one bound row and transfers each disposable carrier immediately into the supplied lease. `Decode` mirrors the same family for scripted tokens; color grammar accepts invariant `RRGGBB` and `AARRGGBB` hexadecimal tokens.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public abstract partial record OptionValue {
    internal Fin<BoundOption> Bind(GetBaseClass getter, OptionLabel label, OptionLease lease, Op key) => Switch(
        state: (Getter: getter, Label: label, Lease: lease, Op: key),
        verb: static (held, row) => held.Op.Catch(() => Fin.Succ(new BoundOption(
            Index: row.Display.Case switch {
                OptionToken display => held.Getter.AddOption(held.Label.Native, display.Native, row.Visibility.IsHidden),
                _ => held.Getter.AddOption(held.Label.Native),
            },
            Current: () => Fin.Succ<OptionValue>(value: row)))),
        toggle: static (held, row) => held.Op.Catch(() => {
            OptionToggle native = new(initialValue: row.Current, offValue: row.Off.Native, onValue: row.On.Native);
            held.Lease.Own(native);
            int index = held.Getter.AddOptionToggle(optionName: held.Label.Native, toggleValue: ref native);
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }),
        number: static (held, row) => held.Op.Catch(() => {
            OptionDouble native = (row.Band.Lower.Case, row.Band.Upper.Case) switch {
                (double lower, double upper) => new OptionDouble(row.Current, lower, upper),
                (double lower, _) => new OptionDouble(row.Current, true, lower),
                (_, double upper) => new OptionDouble(row.Current, false, upper),
                _ => new OptionDouble(row.Current),
            };
            held.Lease.Own(native);
            int index = row.Prompt.Case switch {
                string prompt => held.Getter.AddOptionDouble(held.Label.Native, ref native, prompt),
                _ => held.Getter.AddOptionDouble(held.Label.Native, ref native),
            };
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }),
        count: static (held, row) => held.Op.Catch(() => {
            OptionInteger native = (row.Band.Lower.Case, row.Band.Upper.Case) switch {
                (int lower, int upper) => new OptionInteger(row.Current, lower, upper),
                (int lower, _) => new OptionInteger(row.Current, true, lower),
                (_, int upper) => new OptionInteger(row.Current, false, upper),
                _ => new OptionInteger(row.Current),
            };
            held.Lease.Own(native);
            int index = row.Prompt.Case switch {
                string prompt => held.Getter.AddOptionInteger(held.Label.Native, ref native, prompt),
                _ => held.Getter.AddOptionInteger(held.Label.Native, ref native),
            };
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }),
        text: static (held, row) => held.Op.Catch(() => {
            OptionString native = new(initialString: row.Current, allowEmptyString: row.Admission.AllowEmpty);
            held.Lease.Own(native);
            int index = row.Prompt.Case switch {
                string prompt => held.Getter.AddOptionString(held.Label.Native, ref native, prompt),
                _ => held.Getter.AddOptionString(held.Label.Native, ref native),
            };
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Fin.Succ<OptionValue>(value: row with { Current = native.CurrentValue })));
        }),
        // `OptionColor` is the host's byte-quadruple carrier, so the kernel colour quantizes once on the way in
        // and re-admits once on the way out; no sRGB component arithmetic rides this page.
        paint: static (held, row) => held.Op.Catch(() => {
            OptionColor native = new(initialValue: Slots.Rgb(shade: row.Current));
            held.Lease.Own(native);
            int index = row.Prompt.Case switch {
                string prompt => held.Getter.AddOptionColor(held.Label.Native, ref native, prompt),
                _ => held.Getter.AddOptionColor(held.Label.Native, ref native),
            };
            return Fin.Succ(new BoundOption(
                Index: index,
                Current: () => Slots.Shade(color: native.CurrentValue, key: held.Op)
                    .Map(shade => (OptionValue)(row with { Current = shade }))));
        }),
        pick: static (held, row) =>
            from _ in guard(!row.Values.IsEmpty && row.Current >= 0 && row.Current < row.Values.Count, held.Op.InvalidInput())
            from bound in held.Op.Catch(() => Fin.Succ(new BoundOption(
                Index: held.Getter.AddOptionList(
                    optionName: held.Label.Native,
                    listValues: row.Values.Map(static value => value.Native).AsIterable(),
                    listCurrentIndex: row.Current),
                Current: () => Fin.Succ<OptionValue>(value: row))))
            select bound,
        enumChoice: static (held, row) => row.Binding.Bind(getter: held.Getter, label: held.Label, key: held.Op)
            .Map(index => new BoundOption(
                Index: index,
                Current: () => row.Binding.Read(getter: held.Getter, key: held.Op)
                    .Map(binding => (OptionValue)(row with { Binding = binding })))));

    internal Fin<OptionValue> Decode(string token, Op key) => Switch(
        state: (Token: token, Op: key),
        verb: static (_, row) => Fin.Succ<OptionValue>(row),
        toggle: static (held, row) => held.Token switch {
            var value when string.Equals(value, row.On.English, StringComparison.OrdinalIgnoreCase) => Fin.Succ<OptionValue>(row with { Current = true }),
            var value when string.Equals(value, row.Off.English, StringComparison.OrdinalIgnoreCase) => Fin.Succ<OptionValue>(row with { Current = false }),
            _ => Fin.Fail<OptionValue>(held.Op.InvalidInput()),
        },
        number: static (held, row) => double.TryParse(
            held.Token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && row.Band.Contains(value)
            ? Fin.Succ<OptionValue>(row with { Current = value })
            : Fin.Fail<OptionValue>(held.Op.InvalidInput()),
        count: static (held, row) => int.TryParse(
            held.Token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && row.Band.Contains(value)
            ? Fin.Succ<OptionValue>(row with { Current = value })
            : Fin.Fail<OptionValue>(held.Op.InvalidInput()),
        text: static (held, row) => held.Token.Length > 0 || row.Admission.AllowEmpty
            ? Fin.Succ<OptionValue>(row with { Current = held.Token })
            : Fin.Fail<OptionValue>(held.Op.InvalidInput()),
        paint: static (held, row) => DecodeColor(token: held.Token, key: held.Op)
            .Map(value => (OptionValue)(row with { Current = value })),
        pick: static (held, row) => row.Values.Map(static (value, index) => (value, index))
            .Find(entry => string.Equals(entry.value.English, held.Token, StringComparison.OrdinalIgnoreCase))
            .Match(
                Some: entry => Fin.Succ<OptionValue>(row with { Current = entry.index }),
                None: () => int.TryParse(held.Token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                    && index >= 0 && index < row.Values.Count
                    ? Fin.Succ<OptionValue>(row with { Current = index })
                    : Fin.Fail<OptionValue>(held.Op.InvalidInput())),
        enumChoice: static (held, row) => row.Binding.Decode(token: held.Token, key: held.Op)
            .Map(binding => (OptionValue)(row with { Binding = binding })));

    // The scripted token is HEX TEXT and the kernel owner admits a byte quadruple, so the decode packs the digits
    // and hands them to `PerceptualColor.OfRgb` directly — no `System.Drawing.Color` is minted only to be re-read,
    // which is the round trip that let a hand-rolled component fold sit between the token and the colour identity.
    private static Fin<PerceptualColor> DecodeColor(string token, Op key) {
        ReadOnlySpan<char> digits = token.AsSpan().Trim().TrimStart('#');
        return uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value) switch {
            true when digits.Length is 6 => PerceptualColor.OfRgb(
                (Red: (byte)(value >> 16), Green: (byte)(value >> 8), Blue: (byte)value, Alpha: byte.MaxValue),
                key: key),
            true when digits.Length is 8 => PerceptualColor.OfRgb(
                (Red: (byte)(value >> 16), Green: (byte)(value >> 8), Blue: (byte)value, Alpha: (byte)(value >> 24)),
                key: key),
            _ => Fin.Fail<PerceptualColor>(error: key.InvalidInput()),
        };
    }
}
```

## [04]-[LEASE]

- Law: the capsule is ONE atom. Bound rows, owned carriers, accumulated faults, and the released flag are columns of one immutable record swapped as a unit, so a release can never observe a half-appended carrier roster and drop the newest carrier — the exact leak the lease exists to prevent. A field plus an `Interlocked` flag plus a second `Atom` beside them is three disciplines guarding one lifetime and is the deleted form.

`OptionLease` exists before the first bind and receives each carrier as it is created. Any failed row releases the partial lease; success returns the same capsule to acquisition. One `OptionValue.Read` projection admits pointer-backed values for selection and snapshots before detached evidence leaves the getter window; one `OptionMark` threads the shared native identity through every evidence case as a base positional column.

`Dispose` accumulates into `Faults` and returns. The lease is consumed under a `using` nested inside the getter's own `using`, so a throwing cleanup would unwind past the acquisition result and replace the in-flight answer with a release fault — the caller then loses the value it actually obtained. `Release` remains the railed entry a caller reads when cleanup evidence matters.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record OptionRow(OptionLabel Label, OptionValue Value, OptionVariance Variance);

public sealed record OptionMark(int NativeIndex, OptionKind Kind, string English, string Local);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionEvidence {
    private OptionEvidence(OptionMark mark) => Mark = mark;

    public OptionMark Mark { get; }

    public sealed record Verb(OptionMark Mark) : OptionEvidence(Mark);
    public sealed record Toggle(OptionMark Mark, string Off, string On, bool Current) : OptionEvidence(Mark);
    public sealed record Number(OptionMark Mark, double Current) : OptionEvidence(Mark);
    public sealed record Count(OptionMark Mark, int Current) : OptionEvidence(Mark);
    public sealed record Text(OptionMark Mark, string Current) : OptionEvidence(Mark);
    public sealed record Paint(OptionMark Mark, PerceptualColor Current) : OptionEvidence(Mark);
    public sealed record Pick(OptionMark Mark, Seq<string> Values, int Current) : OptionEvidence(Mark);
    public sealed record EnumChoice(OptionMark Mark, Seq<string> Values, string Current) : OptionEvidence(Mark);
}

public sealed record OptionChoice(OptionLabel Label, OptionValue Value, OptionEvidence Evidence);

internal sealed record BoundOption(int Index, Func<Fin<OptionValue>> Current);

public sealed record OptionSet {
    private OptionSet(Seq<OptionRow> rows) => Rows = rows;

    public Seq<OptionRow> Rows { get; }

    public static Fin<OptionSet> Of(params ReadOnlySpan<OptionRow> rows) {
        Op op = Op.Of(name: nameof(OptionSet));
        Seq<OptionRow> table = toSeq(rows.ToArray());
        return from _ in guard(!table.IsEmpty, op.InvalidInput())
               from __ in guard(table.ForAll(static row => row is not null
                   && row.Label is not null
                   && row.Value is not null
                   && row.Variance is not null), op.InvalidInput())
               from ___ in table.TraverseM(row => row.Value.Admit(op)).As()
               from ____ in guard(table.Map(static row => row.Label.English)
                   .Distinct(StringComparer.OrdinalIgnoreCase).Count == table.Count, op.InvalidInput())
               select new OptionSet(rows: table);
    }

    public Fin<OptionLease> Bind(GetBaseClass getter, Op key) {
        OptionLease lease = new();
        return guard(RhinoApp.IsOnMainThread && getter is not null, key.InvalidContext()).ToFin()
            .Bind(_ => Rows.FoldM<Fin, OptionLease>(lease, (held, row) =>
                row.Value.Bind(getter: getter, label: row.Label, lease: held, key: key).Bind(bound => {
                    held.Attach(row: row, bound: bound);
                    return key.Catch(() => {
                        getter.SetOptionVaries(optionIndex: bound.Index, varies: row.Variance.Varies);
                        return Fin.Succ(held);
                    });
                })).As())
            .Match(
                Succ: static held => Fin.Succ(held),
                Fail: error => lease.Release().Match(
                    Succ: _ => Fin.Fail<OptionLease>(error),
                    Fail: cleanup => Fin.Fail<OptionLease>(error + cleanup)));
    }

    public Fin<(OptionLabel Label, OptionValue Value)> Decode(string name, string token, Op key) =>
        from admittedName in key.AcceptText(name)
        from admittedToken in key.Need(token)
        from row in Rows.Find(candidate => string.Equals(
            candidate.Label.English, admittedName, StringComparison.OrdinalIgnoreCase)).ToFin(Fail: key.MissingContext())
        from value in row.Value.Decode(token: admittedToken, key: key)
        select (row.Label, value);
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
public sealed class OptionLease : IDisposable {
    // ONE atom owns the whole capsule. The prior shape mixed three disciplines — an `Atom` for faults, bare
    // mutable fields for the bound rows and the carrier roster, and an `Interlocked`/`Volatile` int for the
    // released flag — so a release could observe a carrier roster mid-append and drop the newest carrier, which is
    // the one leak the lease exists to foreclose. `Draining` is the transient column the winning swap carries out:
    // `Swap` retries until its CAS lands, so exactly one caller sees a non-empty roster and every peer sees empty.
    private readonly Atom<LeaseBooks> books = Atom(value: LeaseBooks.Empty);

    private sealed record LeaseBooks(
        Seq<(OptionRow Row, BoundOption Bound)> Bound,
        Seq<IDisposable> Resources,
        Seq<IDisposable> Draining,
        Seq<Error> Faults,
        bool Released) {
        internal static readonly LeaseBooks Empty = new(
            Bound: Seq<(OptionRow, BoundOption)>(),
            Resources: Seq<IDisposable>(),
            Draining: Seq<IDisposable>(),
            Faults: Seq<Error>(),
            Released: false);
    }

    internal Unit Own(IDisposable resource) =>
        ignore(books.Swap(f: held => held with { Resources = held.Resources.Add(value: resource) }));

    internal OptionLease Attach(OptionRow row, BoundOption boundOption) {
        _ = books.Swap(f: held => held with { Bound = held.Bound.Add(value: (row, boundOption)) });
        return this;
    }

    public Fin<OptionChoice> Selected(GetBaseClass getter, Op key) =>
        from _ in AdmitLive(key)
        from choice in key.Catch(() =>
            from __ in guard(getter is not null && getter.Result() is GetResult.Option, key.InvalidResult())
            let index = getter.OptionIndex()
            from native in Optional(getter.Option()).ToFin(Fail: key.InvalidResult())
            from entry in books.Value.Bound.Find(candidate => candidate.Bound.Index == index).ToFin(Fail: key.InvalidResult())
            from current in entry.Row.Value.Read(native: Some(native), fallback: entry.Bound.Current, key: key)
            from kind in OptionKind.Of(native: native.OptionType, key: key)
            select new OptionChoice(
                Label: entry.Row.Label,
                Value: current,
                Evidence: Evidence(native, current, kind)))
        select choice;

    internal Fin<Seq<(OptionLabel Label, OptionValue Current)>> Snapshot(Op key) => key.Catch(() =>
        from _ in AdmitLive(key)
        from current in books.Value.Bound
            .TraverseM(entry => entry.Row.Value.Read(native: None, fallback: entry.Bound.Current, key: key)
                .Map(value => (entry.Row.Label, value)))
            .As()
        select current.Strict());

    private Fin<Unit> AdmitLive(Op key) => guard(
        flag: RhinoApp.IsOnMainThread && !books.Value.Released,
        False: key.InvalidContext()).ToFin();

    internal Fin<Unit> Release() {
        Op op = Op.Of(name: nameof(OptionLease));
        LeaseBooks settled = books.Swap(f: held => held.Released
            ? held with { Draining = Seq<IDisposable>() }
            : held with {
                Bound = Seq<(OptionRow, BoundOption)>(),
                Resources = Seq<IDisposable>(),
                Draining = held.Resources,
                Released = true,
            });
        return settled.Draining.Rev()
            .Traverse(resource => op.Catch(() => Fin.Succ(value: Op.Side(resource.Dispose))).ToValidation())
            .As()
            .Map(static _ => unit)
            .ToFin();
    }

    public Seq<Error> Faults => books.Value.Faults;

    public void Dispose() => _ = Release().Match(
        Succ: static released => released,
        Fail: fault => ignore(books.Swap(f: held => held with { Faults = held.Faults.Add(value: fault) })));

    private static OptionEvidence Evidence(CommandLineOption native, OptionValue current, OptionKind kind) {
        OptionMark mark = new(native.Index, kind, native.EnglishName, native.LocalName);
        return current.Switch(
            state: (Mark: mark, Native: native),
            verb: static (held, _) => (OptionEvidence)new OptionEvidence.Verb(held.Mark),
            toggle: static (held, row) => {
                held.Native.ToggleValues(english: false, offValue: out string off, onValue: out string on);
                return new OptionEvidence.Toggle(held.Mark, off, on, row.Current);
            },
            number: static (held, row) => new OptionEvidence.Number(held.Mark, row.Current),
            count: static (held, row) => new OptionEvidence.Count(held.Mark, row.Current),
            text: static (held, row) => new OptionEvidence.Text(held.Mark, row.Current),
            paint: static (held, row) => new OptionEvidence.Paint(held.Mark, row.Current),
            pick: static (held, row) => new OptionEvidence.Pick(
                held.Mark, toSeq(held.Native.ListOptions(english: false)), row.Current),
            enumChoice: static (held, row) => new OptionEvidence.EnumChoice(
                held.Mark,
                held.Native.OptionType is CommandLineOptionType.List ? toSeq(held.Native.ListOptions(english: false)) : [],
                Convert.ToString(row.Binding.Current, CultureInfo.InvariantCulture) ?? string.Empty));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
