# Objective Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/objective.md`

The moves are cumulative and ordered. Fenced-LOC deltas count nonblank C# lines in the shown scopes; symbol deltas count authored declarations. The queue totals **-27 fenced LOC**, **-1 type**, and **-11 member declarations** in the target. It adds no helper, wrapper, roster, or generated owner.

Protected non-moves: keep `AlertSeverity.Stride` as the semantic `Tolerance` surface over private lazy storage; exposing `Lazy<Tolerance>.Value` at its three consumers would relocate initialization mechanics. Keep `BurnVerdict.At` computed by each case; constructor-threading it through the base would add a second stored `BurnRow` reference per verdict. Keep `AlertSpec.Severity`/`Hold` as deploy-plane columns, and keep both `PanelSpec.Of` overloads because callers use both default-widget and explicit-widget call shapes. Do not replace `BenchLedger`'s census with `RosterFold.Collisions` in this refinement: the latter returns each collided key once, while the current traversal emits one fault per colliding input row, so that substitution changes observable error cardinality.

## Move 1 — Fold the one-to-one posture roster into `AlertSeverity`

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:124`, anchor `public sealed partial class AlertPosture`; `:138`, anchors `AlertSeverity Ticket` / `Page`; consumer ripple `libs/dotnet/Rasm.AppUi/.planning/Charts/telemetry.md:242`, anchor `row.Severity.Posture.Key`.

**From:**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlertPosture {
    public static readonly AlertPosture Warning = new("warning", urgency: "queue");
    public static readonly AlertPosture Critical = new("critical", urgency: "interrupt");
    public string Urgency { get; }
}
```

```csharp
public static readonly AlertSeverity Ticket = new("ticket", rank: 0, holdMinutes: 30, posture: AlertPosture.Warning);
public static readonly AlertSeverity Page = new("page", rank: 1, holdMinutes: 0, posture: AlertPosture.Critical);
public AlertPosture Posture { get; }
```

```csharp
new KeyValuePair<string, string>(PostureSlot, row.Severity.Posture.Key),
new KeyValuePair<string, string>(UrgencySlot, row.Severity.Posture.Urgency)
```

```csharp
toSeq(Severity.Items).Find(rank => rank.Key == row.Severity.Posture.Key)
    .ToFin(Fail: (Error)new ChartFault.SpecRejected(
        $"burn/{row.Key}: posture {row.Severity.Posture.Key} names no severity"));
```

**To:**

```csharp
public static readonly AlertSeverity Ticket = new("ticket", rank: 0, holdMinutes: 30, posture: "warning", urgency: "queue");
public static readonly AlertSeverity Page = new("page", rank: 1, holdMinutes: 0, posture: "critical", urgency: "interrupt");
public string Posture { get; }
public string Urgency { get; }
```

```csharp
new KeyValuePair<string, string>(PostureSlot, row.Severity.Posture),
new KeyValuePair<string, string>(UrgencySlot, row.Severity.Urgency)
```

```csharp
toSeq(Severity.Items).Find(rank => rank.Key == row.Severity.Posture)
    .ToFin(Fail: (Error)new ChartFault.SpecRejected(
        $"burn/{row.Key}: posture {row.Severity.Posture} names no severity"));
```

**Effect:** target **-7 fenced LOC**, **-1 type**, **-2 net members**. Consumer LOC is unchanged.

**API / consumer proof:** `Warning` is paired only with `Ticket`, and `Critical` only with `Page`; no repository consumer accepts or selects an `AlertPosture`. The sole external code read immediately projects the posture key into AppUi `Severity`. Thinktecture smart-enum columns carry both strings without a second generated roster. The `warning|critical` and `queue|interrupt` wire values survive exactly.

**Ripples:** remove `AlertPosture` from the page index and rewrite the burn ownership/growth prose at lines 104, 108, 112, and 113 around one severity row carrying posture and urgency. Change only the two shown AppUi `.Posture.Key` reads.

## Move 2 — Store `Duration` columns, not minute scalars plus projections

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:138`, anchors `AlertSeverity Ticket` / `Page`; `:147`, anchor `HoldMinutes`; `:172`, anchors the four `BurnRow` rows; `:179`, anchors `LongMinutes` / `ShortMinutes`.

**From (after Move 1):**

```csharp
public static readonly AlertSeverity Ticket = new("ticket", rank: 0, holdMinutes: 30, posture: "warning", urgency: "queue");
public static readonly AlertSeverity Page = new("page", rank: 1, holdMinutes: 0, posture: "critical", urgency: "interrupt");
public int HoldMinutes { get; }
public Duration Hold => Duration.FromMinutes(HoldMinutes);
```

```csharp
public static readonly BurnRow PageFast = new("page-fast", factor: 14.4d, longMinutes: 60, shortMinutes: 5, severity: AlertSeverity.Page);
public static readonly BurnRow PageSlow = new("page-slow", factor: 6d, longMinutes: 360, shortMinutes: 30, severity: AlertSeverity.Page);
public static readonly BurnRow TicketFast = new("ticket-fast", factor: 3d, longMinutes: 1_440, shortMinutes: 120, severity: AlertSeverity.Ticket);
public static readonly BurnRow TicketSlow = new("ticket-slow", factor: 1d, longMinutes: 4_320, shortMinutes: 360, severity: AlertSeverity.Ticket);
```

```csharp
public int LongMinutes { get; }
public int ShortMinutes { get; }
public Duration Long => Duration.FromMinutes(LongMinutes);
public Duration Short => Duration.FromMinutes(ShortMinutes);
projection: static row => row.LongMinutes,
```

**To:**

```csharp
public static readonly AlertSeverity Ticket = new("ticket", rank: 0, hold: Duration.FromMinutes(30), posture: "warning", urgency: "queue");
public static readonly AlertSeverity Page = new("page", rank: 1, hold: Duration.Zero, posture: "critical", urgency: "interrupt");
public Duration Hold { get; }
```

```csharp
public static readonly BurnRow PageFast = new("page-fast", factor: 14.4d, @long: Duration.FromMinutes(60), @short: Duration.FromMinutes(5), severity: AlertSeverity.Page);
public static readonly BurnRow PageSlow = new("page-slow", factor: 6d, @long: Duration.FromMinutes(360), @short: Duration.FromMinutes(30), severity: AlertSeverity.Page);
public static readonly BurnRow TicketFast = new("ticket-fast", factor: 3d, @long: Duration.FromMinutes(1_440), @short: Duration.FromMinutes(120), severity: AlertSeverity.Ticket);
public static readonly BurnRow TicketSlow = new("ticket-slow", factor: 1d, @long: Duration.FromMinutes(4_320), @short: Duration.FromMinutes(360), severity: AlertSeverity.Ticket);
```

```csharp
public Duration Long { get; }
public Duration Short { get; }
projection: static row => row.Long.TotalSeconds,
```

**Effect:** **-3 fenced LOC**, **-3 members**.

**API / consumer proof:** NodaTime owns elapsed-span semantics through `Duration.FromMinutes`, `Duration.Zero`, and `TotalSeconds`; Thinktecture accepts `Duration` as an ordinary constructor column. C# requires escaped named arguments for generated parameters named `long` and `short`, hence `@long:` / `@short:`. Every repository consumer already reads `Hold`, `Long`, or `Short`; the minute members have no consumer outside their projections.

**Ripples:** none outside the target. AppHost health and AppUi keep `Severity.Hold`; SLO tiles keep `BurnRow.Short`.

## Move 3 — Delete the unconsumed saturation convenience member

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:60`, anchor `public sealed record Saturation`.

**From:**

```csharp
public sealed record Saturation(string Metric, double Bound, LevelBreach Breach) : Sli {
    public bool Breached(double reading) => Breach.Breaches(reading, Bound);
}
```

**To:**

```csharp
public sealed record Saturation(string Metric, double Bound, LevelBreach Breach) : Sli;
```

**Effect:** **-1 fenced LOC**, **-1 public member**.

**API / consumer proof:** the repository has no call to `Sli.Saturation.Breached`; the case's admitted `Bound` and `Breach` columns are the actual data contract. Removing the convenience method changes neither union payload, generated dispatch, nor serialization shape.

**Ripples:** none.

## Move 4 — Make firing extraction exhaustive through generated dispatch

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:265`, anchor `rows.Choose(static row => row is BurnVerdict.Firing ...)`.

**From:**

```csharp
Severity: AlertSeverity.Dominant(
    rows.Choose(static row => row is BurnVerdict.Firing fired ? Some(fired.At.Severity) : None).Strict()));
```

**To:**

```csharp
Severity: AlertSeverity.Dominant(
    rows.Choose(static row => row.Switch(
        firing: static fired => Some(fired.At.Severity),
        quiet: static _ => Option<AlertSeverity>.None,
        unread: static _ => Option<AlertSeverity>.None)).Strict()));
```

**Effect:** **+3 fenced LOC**, no symbol delta.

**API / consumer proof:** the local Thinktecture catalogue guarantees total generated `Switch` dispatch for regular unions. The current negative type test silently treats any future verdict as non-firing; total dispatch makes that new semantic decision a compile break. `Choose` receives the same `Some` only for `Firing` and the same `None` for both existing non-firing cases.

**Ripples:** none.

## Move 5 — Inline the single-use `Slo.Share` expression

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:258`, anchor `public static double Share`; `:277`, anchor `Spend: Share(row, objective)`.

**From:**

```csharp
public static double Share(BurnRow row, Objective objective) =>
    row.Factor * row.Long.TotalSeconds / objective.Window.TotalSeconds;
Spend: Share(row, objective),
```

**To:**

```csharp
Spend: row.Factor * row.Long.TotalSeconds / objective.Window.TotalSeconds,
```

**Effect:** **-2 fenced LOC**, **-1 public member**.

**API / consumer proof:** `Share` has exactly one repository consumer and adds no admission or policy beyond the `AlertSpec.Spend` expression. `Slo.Burn` stays because AppUi and the verdict fold both consume it.

**Ripples:** none.

## Move 6 — Remove the generator-redundant reference guard

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:207`, anchor `ValidateFactoryArguments`; `:213`, anchor `&& sli is not null`.

**From:**

```csharp
&& sli is not null
&& target is > 0d and < 1d
```

**To:**

```csharp
&& target is > 0d and < 1d
```

**Effect:** **-1 fenced LOC**.

**API / consumer proof:** the local Thinktecture catalogue fixes the generated complex-value-object order: non-nullable reference guards run before `ValidateFactoryArguments`. The hook still owns name grammar, target, default-window normalization, and the burn floor.

**Ripples:** none.

## Move 7 — Delete `Sli.Breaks`; the union case already carries the discriminant

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:79`, anchor `public Seq<string> Breaks`; `:290`, anchor `sli.Breaks.ForAll`.

**From:**

```csharp
public Seq<string> Breaks => Switch(
    ratio: static _ => Seq<string>(),
    partition: static row => Seq(row.By),
    latency: static _ => Seq<string>(),
    saturation: static _ => Seq<string>(),
    freshness: static _ => Seq<string>());
```

```csharp
&& sli.Breaks.ForAll(key => row.Dimensions.Exists(declared => declared == key))
```

**To:**

```csharp
&& (sli is not Sli.Partition partition
    || row.Dimensions.Exists(declared => declared == partition.By))
```

**Effect:** **-5 fenced LOC**, **-1 public member**.

**API / consumer proof:** only `Partition` has a break key and it carries exactly one `By`; all other arms allocate an empty `Seq` solely so `ForAll` returns true. `Slo.Admit` is the only consumer. The case pattern preserves the exact truth table.

**Ripples:** remove only `Breaks`; `Series`, `Admits`, and `Wellformed` retain distinct case-owned projections.

## Move 8 — Replace the optional-row eliminator with `Option.Exists`

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:285`, anchor `public static Validation<Error, Objective> Admit`, after Move 7.

**From:**

```csharp
&& sli.Series.ForAll(name => roster.Find(name).Match(
    Some: row => sli.Admits.Exists(kind => kind.Equals(row.Kind))
        && (sli is not Sli.Partition partition
            || row.Dimensions.Exists(declared => declared == partition.By))
        && (sli is not Sli.Latency latency
            || (string.Equals(row.Unit, Buckets.Seconds, StringComparison.Ordinal)
                && row.Bounds.Exists(buckets => buckets.Bounds.Contains(latency.Ceiling.TotalSeconds)))),
    None: static () => false))
```

```csharp
    ? Validation<Error, Objective>.Success(objective)
    : Validation<Error, Objective>.Fail(new KernelFault.InvalidValue(
        Label: objective.Name,
        Requirement: $"a wellformed indicator whose series declare as {string.Join(" or ", sli.Admits.Map(static kind => kind.Key))}, name every partition key, and pin a latency ceiling ON the declared seconds bucket ladder"));
```

**To:**

```csharp
&& sli.Series.ForAll(name => roster.Find(name).Exists(row =>
    sli.Admits.Exists(kind => kind.Equals(row.Kind))
    && (sli is not Sli.Partition partition
        || row.Dimensions.Exists(declared => declared == partition.By))
    && (sli is not Sli.Latency latency
        || (string.Equals(row.Unit, Buckets.Seconds, StringComparison.Ordinal)
            && row.Bounds.Exists(buckets => buckets.Bounds.Contains(latency.Ceiling.TotalSeconds))))))
```

```csharp
    ? objective
    : new KernelFault.InvalidValue(
        Label: objective.Name,
        Requirement: $"a wellformed indicator whose series declare as {string.Join(" or ", sli.Admits.Map(static kind => kind.Key))}, name every partition key, and pin a latency ceiling ON the declared seconds bucket ladder");
```

**Effect:** **-1 fenced LOC**.

**API / consumer proof:** `HashMap.Find` returns `Option<T>` and the local LanguageExt catalogue defines `Option.Exists` as false for `None` and the predicate result for `Some`. The declared `Validation<Error,Objective>` return target owns the value/error lifts. Pack accumulation remains unchanged.

**Ripples:** none.

## Move 9 — Inline the single-use pack-name slot and remove `self`

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:371`, anchor `public Fin<BoardPack> Admit`; `:379`, anchor `private static Validation<Error, Unit> Named`.

**From:**

```csharp
public Fin<BoardPack> Admit(HashMap<string, InstrumentSpec> roster) {
    BoardPack self = this;
    return (Panels.Traverse(panel => panel.Admit(roster)).As(),
            Objectives.Traverse(objective => Slo.Admit(roster, objective)).As(),
            Named(Objectives))
        .Apply((_, _, _) => self).As().ToFin();
}
```

```csharp
private static Validation<Error, Unit> Named(Seq<Objective> objectives) =>
    objectives.Collisions(static row => row.Name) is { IsEmpty: false } collided
        ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
            Label: string.Join(", ", collided), Requirement: "one objective per alert-namespace name"))
        : Validation<Error, Unit>.Success(unit);
```

**To:**

```csharp
public Fin<BoardPack> Admit(HashMap<string, InstrumentSpec> roster) =>
    (Panels.Traverse(panel => panel.Admit(roster)).As(),
     Objectives.Traverse(objective => Slo.Admit(roster, objective)).As(),
     Objectives.Collisions(static row => row.Name) is { IsEmpty: false } collided
        ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
            Label: string.Join(", ", collided), Requirement: "one objective per alert-namespace name"))
        : Validation<Error, Unit>.Success(unit))
    .Apply((_, _, _) => this).As().ToFin();
```

**Effect:** **-4 fenced LOC**, **-1 private member**; one local alias disappears.

**API / consumer proof:** `Named` has one call and exists only as the third independent slot of this applicative product. The same `Collisions` result, error, three-way `Apply`, and `ToFin` boundary remain; no claim is reordered or short-circuited. `TelemetryContributorPort.Admit` in `Domain/telemetry.md:557` consumes the unchanged `BoardPack.Admit` surface.

**Ripples:** none.

## Move 10 — Inline the single-use duplicate-row gate without changing cardinality

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:425`, anchor `rows.Traverse(row => Distinct(row, census))`; `:447`, anchor `private static Validation<Error, Unit> Distinct`.

**From:**

```csharp
rows.Traverse(row => Distinct(row, census)).As()
```

```csharp
private static Validation<Error, Unit> Distinct(BenchClaim row, HashMap<Op, int> census) =>
    census.Find(row.Claim).IfNone(0) > 1
        ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
            Label: row.Claim.ToString(), Requirement: "one ledger row per claim key"))
        : Validation<Error, Unit>.Success(unit);
```

**To:**

```csharp
rows.Traverse(row => census.Find(row.Claim).IfNone(0) > 1
    ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
        Label: row.Claim.ToString(), Requirement: "one ledger row per claim key"))
    : Validation<Error, Unit>.Success(unit)).As()
```

**Effect:** **-2 fenced LOC**, **-1 private member**.

**API / consumer proof:** `Distinct` has one lambda call and adds no rule beyond the already-built census lookup. Inlining preserves the same per-input-row traversal, `IfNone(0)` behavior, error payload, applicative accumulation, and therefore the exact number and order of duplicate faults.

**Ripples:** none.

## Move 11 — Inline the single-use bench-claim admission gate

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:425`, anchor `rows.Traverse(Admitted)`; `:441`, anchor `private static Validation<Error, BenchClaim> Admitted`.

**From:**

```csharp
rows.Traverse(Admitted).As()
```

```csharp
private static Validation<Error, BenchClaim> Admitted(BenchClaim row) =>
    row.IsValid
        ? Validation<Error, BenchClaim>.Success(row)
        : Validation<Error, BenchClaim>.Fail(new KernelFault.InvalidValue(
            Label: row.Claim.ToString(), Requirement: "a positive speedup floor and non-blank lane spellings"));
```

**To:**

```csharp
rows.Traverse(row => row.IsValid
    ? Validation<Error, BenchClaim>.Success(row)
    : Validation<Error, BenchClaim>.Fail(new KernelFault.InvalidValue(
        Label: row.Claim.ToString(), Requirement: "a positive speedup floor and non-blank lane spellings"))).As()
```

**Effect:** **-2 fenced LOC**, **-1 private member**.

**API / consumer proof:** `Admitted` has one method-group use and owns no rule beyond `BenchClaim.IsValid`; inlining keeps the same per-row `Validation`, accumulated `Traverse`, and error payload.

**Ripples:** none.

## Move 12 — State proof presence with `Option.Exists`

**Location:** `libs/dotnet/Rasm/.planning/Domain/objective.md:436`, anchor `return Rows.Filter` inside `BenchLedger.Unproven`.

**From:**

```csharp
return Rows.Filter(row => index.Find(row.Claim).Match(
    Some: corpus => row.Corpus.IsSome && corpus.IsNone,
    None: static () => true));
```

**To:**

```csharp
return Rows.Filter(row => !index.Find(row.Claim).Exists(corpus => row.Corpus.IsNone || corpus.IsSome));
```

**Effect:** **-2 fenced LOC**.

**API / consumer proof:** a claim is proven exactly when its key exists and either it is not corpus-bound or the proof carries a fingerprint. `Option.Exists` states that positive law and is false on a missing key; the outer negation preserves missing-key and missing-required-fingerprint failures. The keyed index and first-present-fingerprint merge remain unchanged.

**Ripples:** none.
