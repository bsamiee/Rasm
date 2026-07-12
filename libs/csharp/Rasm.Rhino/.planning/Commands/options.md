# [RASM_RHINO_OPTIONS]

The command-line option vocabulary (`Rasm.Rhino.Commands`). One `OptionValue` union carries every option modality the host command line admits — bare verb, toggle, bounded number, bounded integer, text, color, and list — and one `OptionRow` pairs a validated localized label with its value, so an option set is data: identity, localization, native binder, scripted decode, current-value projection, validation, and lifetime are all recoverable from the row, and the census-era CLR-type switch factory with its `dynamic` enum escape hatch is dead. An enum vocabulary is a `Pick` factory projection over its names, never a generic host binding. Native carriers are boundary-owned: `OptionLease` constructs every `Option*` carrier at bind, holds it for exactly the getter window, projects current values on demand, resolves a selection through the getter's own option index, and releases every carrier deterministically on dispose.

## [01]-[INDEX]

- [02]-[IDENTITY]: `OptionLabel` — validated english identity with its localization column.
- [03]-[VALUE_FAMILY]: the `OptionValue` union — one case per option modality with native bind, current-value projection, and scripted decode per case.
- [04]-[SET_AND_LEASE]: `OptionRow`, `OptionSet`, the `OptionLease` lifetime capsule, and `OptionChoice` the selection product.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[IDENTITY]

- Owner: `OptionLabel` `[ComplexValueObject]` — the english option name validated through the host's own `CommandLineOption.IsValidOptionName` at admission, with the localized display name as an optional column; `Pair()` projects the `LocalizeStringPair` the native binders consume.
- Law: identity validates once at the factory — an invalid option name is unconstructible, so no binder, decoder, or snapshot re-checks it; a toggle's value names validate through `IsValidOptionValueName` inside the toggle case's own admission.
- Law: localization is a column, never a sibling label type — the english name is the stable identity scripts and receipts key on, and the local name is display material the `Pair` projection carries to the host.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class OptionLabel {
    public string English { get; }
    public Option<string> Local { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string english, ref Option<string> local) {
        validationError = CommandLineOption.IsValidOptionName(optionName: english)
            ? validationError
            : new ValidationError("option name is invalid");
    }

    public global::Rhino.UI.LocalizeStringPair Pair() =>
        new(english: English, local: Local.IfNone(English));
}
```

## [03]-[VALUE_FAMILY]

- Owner: `OptionValue` `[Union]` — one case per command-line option modality: `Verb` the value-less selectable (with its optional displayed value name and hidden flag), `Toggle`, `Number` and `Count` with optional bounds as case payload, `Text` with its empty-admission grant, `Paint`, and `Pick` the list case that also absorbs every enum vocabulary through `Pick.OfEnum<TEnum>`; each case owns its native bind, its current-value projection, and its scripted token decode inside the one generated dispatch.
- Law: the bind is the case's own arm — a toggle constructs `OptionToggle` and calls `AddOptionToggle`, a bounded number selects the one-, two-, or three-argument `OptionDouble` construction from its bounds payload, a list projects its values onto `LocalizeStringPair` rows through `AddOptionList` — so a new modality is one case and no factory, binder, parser, or capture site reopens.
- Law: the enum escape hatch is dead — an enum-valued option is `Pick.OfEnum<TEnum>()` projecting `Enum.GetNames<TEnum>()` into the one list case, so the generic host list members are never reached through `dynamic` and the vocabulary stays a value.
- Law: scripted decode is per-case token grammar — toggle matches its off/on value names ordinally, number and count parse invariant and re-check the case bounds, text honors the empty grant, pick matches a value name or an ordinal — and a token no case grammar admits is a typed decode fault carrying the raw token.
- Law: bounds are case payload enforced at both ends — the native carrier receives them so the command line refuses out-of-band typing, and the scripted decode re-checks them because a script token bypasses the interactive band.
- Law: toggle value names default to `Off`/`On` — value names pass the host's own `IsValidOptionValueName` gate, which admits letters and numbers only, so a bracketed or punctuated default is unconstructible.
- RESEARCH: the host color-token grammar is unverified — the `Paint` scripted decode lands after catalog verification; until then a paint token decode refuses typed.
- Growth: a new option modality is one case with its three arms; `OptionRow`, `OptionSet`, the lease, and every consumer read it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionValue {
    private OptionValue() { }
    public sealed record Verb(Option<string> ValueName = default, bool Hidden = false) : OptionValue;
    public sealed record Toggle(bool Current, string Off = "Off", string On = "On") : OptionValue;
    public sealed record Number(double Current, Option<double> Lower = default, Option<double> Upper = default, Option<string> Prompt = default) : OptionValue;
    public sealed record Count(int Current, Option<int> Lower = default, Option<int> Upper = default, Option<string> Prompt = default) : OptionValue;
    public sealed record Text(string Current, bool AllowEmpty = false, Option<string> Prompt = default) : OptionValue;
    public sealed record Paint(System.Drawing.Color Current, Option<string> Prompt = default) : OptionValue;
    public sealed record Pick(Seq<string> Values, int Current) : OptionValue {
        public static Fin<Pick> OfEnum<TEnum>(TEnum current) where TEnum : struct, Enum {
            Seq<string> names = toSeq(Enum.GetNames<TEnum>());
            return names.Map(static (name, index) => (Name: name, Index: index))
                .Find(row => row.Name == current.ToString())
                .Map(row => new Pick(Values: names, Current: row.Index))
                .ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput());
        }
    }

    internal Fin<BoundOption> Bind(GetBaseClass getter, OptionLabel label, Op key) =>
        Switch(
            state: (Getter: getter, Label: label, Op: key),
            verb: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ(value: row.ValueName.Case switch {
                string value => BoundOption.Plain(
                    index: ctx.Getter.AddOption(ctx.Label.Pair(), new global::Rhino.UI.LocalizeStringPair(english: value, local: value), row.Hidden),
                    current: () => row),
                _ => BoundOption.Plain(index: ctx.Getter.AddOption(ctx.Label.Pair()), current: () => row),
            })),
            toggle: static (ctx, row) =>
                from off in guard(CommandLineOption.IsValidOptionValueName(row.Off), ctx.Op.InvalidInput())
                from on in guard(CommandLineOption.IsValidOptionValueName(row.On), ctx.Op.InvalidInput())
                from bound in ctx.Op.Catch(() => {
                    OptionToggle native = new(row.Current, row.Off, row.On);
                    int index = ctx.Getter.AddOptionToggle(ctx.Label.Pair(), ref native);
                    return Fin.Succ(value: BoundOption.Carried(index: index, carrier: native,
                        current: () => row with { Current = native.CurrentValue }));
                })
                select bound,
            number: static (ctx, row) => ctx.Op.Catch(() => {
                OptionDouble native = (row.Lower.Case, row.Upper.Case) switch {
                    (double lo, double hi) => new OptionDouble(row.Current, lo, hi),
                    (double lo, _) => new OptionDouble(row.Current, true, lo),
                    (_, double hi) => new OptionDouble(row.Current, false, hi),
                    _ => new OptionDouble(row.Current),
                };
                int index = row.Prompt.Case switch {
                    string prompt => ctx.Getter.AddOptionDouble(ctx.Label.Pair(), ref native, prompt),
                    _ => ctx.Getter.AddOptionDouble(ctx.Label.Pair(), ref native),
                };
                return Fin.Succ(value: BoundOption.Carried(index: index, carrier: native,
                    current: () => row with { Current = native.CurrentValue }));
            }),
            count: static (ctx, row) => ctx.Op.Catch(() => {
                OptionInteger native = (row.Lower.Case, row.Upper.Case) switch {
                    (int lo, int hi) => new OptionInteger(row.Current, lo, hi),
                    (int lo, _) => new OptionInteger(row.Current, true, lo),
                    (_, int hi) => new OptionInteger(row.Current, false, hi),
                    _ => new OptionInteger(row.Current),
                };
                int index = row.Prompt.Case switch {
                    string prompt => ctx.Getter.AddOptionInteger(ctx.Label.Pair(), ref native, prompt),
                    _ => ctx.Getter.AddOptionInteger(ctx.Label.Pair(), ref native),
                };
                return Fin.Succ(value: BoundOption.Carried(index: index, carrier: native,
                    current: () => row with { Current = native.CurrentValue }));
            }),
            text: static (ctx, row) => ctx.Op.Catch(() => {
                OptionString native = new(row.Current, row.AllowEmpty);
                int index = row.Prompt.Case switch {
                    string prompt => ctx.Getter.AddOptionString(ctx.Label.Pair(), ref native, prompt),
                    _ => ctx.Getter.AddOptionString(ctx.Label.Pair(), ref native),
                };
                return Fin.Succ(value: BoundOption.Carried(index: index, carrier: native,
                    current: () => row with { Current = native.CurrentValue }));
            }),
            paint: static (ctx, row) => ctx.Op.Catch(() => {
                OptionColor native = new(row.Current);
                int index = row.Prompt.Case switch {
                    string prompt => ctx.Getter.AddOptionColor(ctx.Label.Pair(), ref native, prompt),
                    _ => ctx.Getter.AddOptionColor(ctx.Label.Pair(), ref native),
                };
                return Fin.Succ(value: BoundOption.Carried(index: index, carrier: native,
                    current: () => row with { Current = native.CurrentValue }));
            }),
            pick: static (ctx, row) =>
                from _ in guard(!row.Values.IsEmpty && row.Current >= 0 && row.Current < row.Values.Count, ctx.Op.InvalidInput())
                from bound in ctx.Op.Catch(() => Fin.Succ(value: BoundOption.Plain(
                    index: ctx.Getter.AddOptionList(
                        ctx.Label.Pair(),
                        row.Values.Map(static value => new global::Rhino.UI.LocalizeStringPair(english: value, local: value)).AsIterable(),
                        row.Current),
                    current: () => row)))
                select bound);

    internal Fin<OptionValue> Decode(string token, Op key) =>
        Switch(
            state: (Token: token, Op: key),
            verb: static (ctx, row) => Fin.Succ<OptionValue>(value: row),
            toggle: static (ctx, row) => ctx.Token switch {
                var t when string.Equals(a: t, b: row.On, comparisonType: StringComparison.OrdinalIgnoreCase) =>
                    Fin.Succ<OptionValue>(value: row with { Current = true }),
                var t when string.Equals(a: t, b: row.Off, comparisonType: StringComparison.OrdinalIgnoreCase) =>
                    Fin.Succ<OptionValue>(value: row with { Current = false }),
                _ => Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput()),
            },
            number: static (ctx, row) =>
                double.TryParse(s: ctx.Token, provider: System.Globalization.CultureInfo.InvariantCulture, result: out double value)
                && row.Lower.Map(lo => value >= lo).IfNone(noneValue: true)
                && row.Upper.Map(hi => value <= hi).IfNone(noneValue: true)
                    ? Fin.Succ<OptionValue>(value: row with { Current = value })
                    : Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput()),
            count: static (ctx, row) =>
                int.TryParse(s: ctx.Token, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int value)
                && row.Lower.Map(lo => value >= lo).IfNone(noneValue: true)
                && row.Upper.Map(hi => value <= hi).IfNone(noneValue: true)
                    ? Fin.Succ<OptionValue>(value: row with { Current = value })
                    : Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput()),
            text: static (ctx, row) => ctx.Token.Length > 0 || row.AllowEmpty
                ? Fin.Succ<OptionValue>(value: row with { Current = ctx.Token })
                : Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput()),
            paint: static (ctx, _) => Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput()),
            pick: static (ctx, row) => row.Values
                .Map(static (value, index) => (Value: value, Index: index))
                .Find(entry => string.Equals(a: entry.Value, b: ctx.Token, comparisonType: StringComparison.OrdinalIgnoreCase))
                .Match(
                    Some: entry => Fin.Succ<OptionValue>(value: row with { Current = entry.Index }),
                    None: () => int.TryParse(s: ctx.Token, provider: System.Globalization.CultureInfo.InvariantCulture, result: out int ordinal)
                         && ordinal >= 0 && ordinal < row.Values.Count
                        ? Fin.Succ<OptionValue>(value: row with { Current = ordinal })
                        : Fin.Fail<OptionValue>(error: ctx.Op.InvalidInput())));
}
```

## [04]-[SET_AND_LEASE]

- Owner: `OptionRow` — label plus value plus the varies column; `OptionSet` — the validated row collection whose factory proves name distinctness once; `BoundOption` — the internal bind product: native index, optional disposable carrier, current-value projection; `OptionLease` — the lifetime capsule owning every bound carrier for exactly the getter window; `OptionChoice` — the selection product: the row's label, its post-selection value, the native option index, and the host's own `StringOptionValue` evidence.
- Entry: `OptionSet.Bind(GetBaseClass, Op) : Fin<OptionLease>` — binds every row in declaration order and stamps `SetOptionVaries` on each varies-marked index; `OptionLease.Selected(GetBaseClass, Op) : Fin<OptionChoice>` — resolves a `GetResult.Option` terminal through `getter.OptionIndex()` against the bound indices, projects the selected row's current value, and reads the list selection through the host option's `CurrentListOptionIndex`; `OptionLease.Snapshot() : Seq<(OptionLabel Label, OptionValue Current)>` — the whole set's live values; `Dispose` releases every carrier exactly once.
- Law: carrier lifetime is the lease, never scattered disposal — the native `Option*` carriers are constructed at bind, never escape the capsule, and release deterministically on dispose; a carrier surviving its getter reads stale native memory and is unreachable through this seam.
- Law: selection resolves by native index, never by label re-parse — the bind index the host returned is the join key, so localized display, hidden verbs, and duplicate-prefix names never mis-resolve.
- Law: a set admits only distinct english names — the factory refuses a duplicate at construction, so the acquisition drive never binds an ambiguous command line.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record OptionRow(OptionLabel Label, OptionValue Value, bool Varies = false);

public sealed record OptionSet(Seq<OptionRow> Rows) {
    public static Fin<OptionSet> Of(params ReadOnlySpan<OptionRow> rows) {
        Op op = Op.Of(name: nameof(OptionSet));
        Seq<OptionRow> table = toSeq(rows.ToArray());
        return from _ in guard(!table.IsEmpty, op.InvalidInput())
               from __ in guard(
                   table.Map(static row => row.Label.English).Distinct(StringComparer.OrdinalIgnoreCase).Count() == table.Count,
                   op.InvalidInput())
               select new OptionSet(Rows: table);
    }

    public Fin<OptionLease> Bind(GetBaseClass getter, Op key) =>
        Rows.TraverseM(row => row.Value.Bind(getter: getter, label: row.Label, key: key).Map(bound => {
                _ = Op.SideWhen(row.Varies, () => getter.SetOptionVaries(optionIndex: bound.Index, varies: true));
                return (Row: row, Bound: bound);
            })).As()
            .Map(bound => new OptionLease(bound: bound));

    public Fin<(OptionLabel Label, OptionValue Value)> Decode(string name, string token, Op key) =>
        Rows.Find(row => string.Equals(a: row.Label.English, b: name, comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToFin(Fail: key.MissingContext())
            .Bind(row => row.Value.Decode(token: token, key: key).Map(value => (row.Label, value)));
}

public sealed record OptionChoice(OptionLabel Label, OptionValue Value, int Index, Option<string> Raw);

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal sealed record BoundOption(int Index, Option<IDisposable> Carrier, Func<OptionValue> Current) {
    internal static BoundOption Plain(int index, Func<OptionValue> current) =>
        new(Index: index, Carrier: Option<IDisposable>.None, Current: current);

    internal static BoundOption Carried(int index, IDisposable carrier, Func<OptionValue> current) =>
        new(Index: index, Carrier: Some(carrier), Current: current);
}

public sealed class OptionLease : IDisposable {
    private readonly Seq<(OptionRow Row, BoundOption Bound)> bound;
    private int released;

    internal OptionLease(Seq<(OptionRow Row, BoundOption Bound)> bound) => this.bound = bound;

    public Fin<OptionChoice> Selected(GetBaseClass getter, Op key) {
        int index = getter.OptionIndex();
        CommandLineOption native = getter.Option();
        return bound.Find(entry => entry.Bound.Index == index)
            .ToFin(Fail: key.InvalidResult())
            .Map(entry => new OptionChoice(
                Label: entry.Row.Label,
                Value: entry.Row.Value is OptionValue.Pick pick
                    ? pick with { Current = native.CurrentListOptionIndex }
                    : entry.Bound.Current(),
                Index: index,
                Raw: Optional(native.StringOptionValue).Filter(static raw => raw.Length > 0)));
    }

    public Seq<(OptionLabel Label, OptionValue Current)> Snapshot() =>
        bound.Map(static entry => (entry.Row.Label, entry.Bound.Current()));

    public void Dispose() =>
        _ = Interlocked.Exchange(location1: ref released, value: 1) is 0
            ? bound.Iter(static entry => entry.Bound.Carrier.Iter(static carrier =>
                ignore(Op.Of(name: nameof(OptionLease)).Catch(() => { carrier.Dispose(); return Fin.Succ(value: unit); }))))
            : unit;
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]        | [FORM]                                           | [ENTRY]                             |
| :-----: | :------------------ | :------------- | :----------------------------------------------- | :---------------------------------- |
|  [01]   | option identity     | `OptionLabel`  | `[ComplexValueObject]`, host-validated name      | `Create` / `Pair`                   |
|  [02]   | modality vocabulary | `OptionValue`  | one union: bind, decode, projection per case     | `Bind` / `Decode`                   |
|  [03]   | option set          | `OptionSet`    | distinct-name validated row collection           | `Of` / `Bind` / `Decode`            |
|  [04]   | carrier lifetime    | `OptionLease`  | capsule over native carriers, idempotent release | `Selected` / `Snapshot` / `Dispose` |
|  [05]   | selection product   | `OptionChoice` | label + value + native index + raw evidence      | `OptionLease.Selected`              |
