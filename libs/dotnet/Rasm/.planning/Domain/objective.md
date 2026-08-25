# [RASM_OBJECTIVE]

`Rasm.Domain` owns the branch's reliability algebra: the closed indicator family an objective binds, the multi-window multi-burn-rate table every alert derives from, the routing severity ladder, the board descriptor a sink publishes, and the speed-claim ledger a corpus gate ingests. Per-sink descriptors — viewport tiles, IaC rule rows, health rules, materials and fabrication rosters — compose these rows; a hand-typed window constant, a re-declared panel row, or a re-spelled admission fold beside them forks alerting and boards silently on any factor change.

Every admission here proves against ONE roster shape — the declaration keyset a contributor port or a mounted set publishes — so a panel over a self-minted row proves exactly as one over a mounted row does. Admission accumulates: panels, objectives, and name distinctness are independent claims and a pack with four broken panels reports four refusals, because the carrier, never a flag, decides whether a fold aborts or gathers.

## [01]-[INDEX]

- [02]-[INDICATOR]: `LevelBreach`, `Sli`, `SloSample` — the breach polarity column, the closed reliability-indicator family, and the window sample every rate divides.
- [03]-[BURN]: `AlertPosture`, `AlertSeverity`, `BurnRow`, `Objective`, `BurnReading`, `BurnVerdict`, `SloVerdict`, `AlertSpec`, `Slo` — the routing posture, the severity ladder, the burn table, the objective admission, and the evaluation fold.
- [04]-[BOARD]: `PanelKind`, `PanelSpec`, `BoardPack` — the board vocabulary and the one pack-wide admission.
- [05]-[BENCH]: `BenchClaim`, `BenchLedger` — the typed speed-claim row and the duplicate-refusing fold the corpus gate reads.

## [02]-[INDICATOR]

- Owner: `LevelBreach` is the polarity column a level indicator reads, so exhaustion measures and utilization measures share one shape; `Sli` is the closed reliability-indicator family every objective binds; `SloSample` is the window pair every rate divides.
- Cases: five indicator shapes — `Ratio` over a good and a total counter, `Partition` over ONE counter whose good half is a value set on a declared dimension, `Latency` over a distribution against a ceiling with its display quantile, `Saturation` over a level against a bound on either polarity, `Freshness` over a level against a staleness horizon; two breach polarities.
- Auto: level indicators read a scalar cell or one key of a mounted family with no arithmetic change, so both pulled kinds answer one shape and a per-key headroom target needs no second case. Each case's own field domain proves once through `Wellformed`, where the objective is still editable; the TypeScript reference form spells these as schema refinements, so both branches refuse identical policy values.
- Law: the sample proves `Breaching <= Total` at admission, so every rate the fold divides is bounded and no consumer re-checks it; a sampler folding its own evidence stream constructs the breaching count as a subset of the total by filter order, so the claim holds structurally at the seam that mints it.
- Law: `Partition` carries a success share over one tag-partitioned counter, never a second counter minted for the numerator — a good-half twin doubles the series a roster mounts, strands its own denominator on any arm edit, and re-mints per value the dimension already keys; `Ratio` stays the shape for genuinely independent counters. `Saturation` bounds a level in that level's OWN unit rather than a normalized share, so a rank, a depth, and a fraction each read one shape and the polarity row decides the side.
- Output: `Rate` answers absence rather than a quotient outside `[0,1]` — the shape is a boundary carrier a foreign series read fills and `default` mints, so an empty window and an invalid one both read `None` rather than handing a burn factor a fabricated quotient to read as a firing alert or a quiet stream.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Text.Json`).
- Growth: a sixth indicator shape is one `Sli` case breaking every dispatch at compile time; a third breach polarity is one `LevelBreach` row every consumer reads through its column.
- Boundary: series and partition keys stay `string` because the WIRE is the string key — a panel, an alert rule, and a query dialect all address a declared instrument by its published name, and the admission below resolves each against the roster rather than carrying a type no deploy plane can spell. Polymorphic metadata rides the family because every derived `AlertSpec` crosses to a deploy plane, where a base-typed write loses the case.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Text.Json.Serialization;
using NodaTime;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LevelBreach {
    public static readonly LevelBreach Ceiling = new("ceiling", static (reading, bound) => reading > bound);
    public static readonly LevelBreach Floor = new("floor", static (reading, bound) => reading < bound);

    [UseDelegateFromConstructor]
    public partial bool Breaches(double reading, double bound);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "sli")]
[JsonDerivedType(typeof(Ratio), "ratio")]
[JsonDerivedType(typeof(Partition), "partition")]
[JsonDerivedType(typeof(Latency), "latency")]
[JsonDerivedType(typeof(Saturation), "saturation")]
[JsonDerivedType(typeof(Freshness), "freshness")]
public abstract partial record Sli {
    private Sli() { }

    public sealed record Ratio(string Good, string Total) : Sli;
    public sealed record Partition(string Metric, string By, Seq<string> Good) : Sli;
    public sealed record Latency(string Metric, Duration Ceiling, double Quantile) : Sli;
    public sealed record Saturation(string Metric, double Bound, LevelBreach Breach) : Sli {
        public bool Breached(double reading) => Breach.Breaches(reading, Bound);
    }
    public sealed record Freshness(string Metric, Duration Horizon) : Sli;

    public Seq<InstrumentKind> Admits => Switch(
        ratio: static _ => Seq(InstrumentKind.Count),
        partition: static _ => Seq(InstrumentKind.Count),
        latency: static _ => Seq(InstrumentKind.Distribution),
        saturation: static _ => Seq(InstrumentKind.Level, InstrumentKind.Levels),
        freshness: static _ => Seq(InstrumentKind.Level, InstrumentKind.Levels));

    public Seq<string> Series => Switch(
        ratio: static row => Seq(row.Good, row.Total),
        partition: static row => Seq(row.Metric),
        latency: static row => Seq(row.Metric),
        saturation: static row => Seq(row.Metric),
        freshness: static row => Seq(row.Metric));

    public Seq<string> Breaks => Switch(
        ratio: static _ => Seq<string>(),
        partition: static row => Seq(row.By),
        latency: static _ => Seq<string>(),
        saturation: static _ => Seq<string>(),
        freshness: static _ => Seq<string>());

    public bool Wellformed => Switch(
        ratio: static row => row.Good != row.Total,
        partition: static row => !row.Good.IsEmpty && row.Good.Distinct().Count == row.Good.Count,
        latency: static row => row.Ceiling > Duration.Zero && row.Quantile is > 0d and < 1d,
        saturation: static row => double.IsFinite(row.Bound),
        freshness: static row => row.Horizon > Duration.Zero);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SloSample(long Breaching, long Total) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Breaching >= 0L, Breaching <= Total);

    public Option<double> Rate => Total == 0L || !IsValid ? None : Some(Breaching / (double)Total);
}
```

## [03]-[BURN]

- Owner: `AlertPosture` is the routing pair — the Alertmanager label a contact row matches and the urgency its receiver reads — carried as ONE row; `AlertSeverity` is the one routing vocabulary the deploy plane's contact rows key on and the one severity ORDER both the escalation walk and the pack verdict read; `BurnRow` is the multi-window multi-burn-rate table; `Objective` binds one indicator to a target ratio and a compliance window with the error budget deriving; `BurnReading` is the long-and-short sample pair; `BurnVerdict` is the per-row outcome; `SloVerdict` the pack outcome; `AlertSpec` the compilation-ready row each burn row derives; `Slo` the evaluation and admission fold.
- Cases: four burn rows — two paging pairs at 14.4× and 6×, two ticketing pairs at 3× and 1×; two severities; three verdict cases — `Firing`, `Quiet`, and `Unread`, where a window pair that carried no rate is a CASE rather than a state column beside two absent options.
- Entry: `Objective.Create(name, sli, target, window)` is the admission — a target outside the open unit interval, a blank or non-conforming name, and a window shorter than the longest burn row each refuse there, so a zero or negative budget has no construction path and no consumer guards one; `Slo.Evaluate(objective, readings)` folds one long-and-short sample pair per burn row into the verdict; `Slo.Specs(objective)` derives one spec per row; `Slo.Admit(roster, objective)` proves the indicator's own field domain, resolves every named series to a declared row of a kind the shape admits, and proves every partition key against that row's declared dimensions.
- Auto: a verdict fires only when BOTH windows exceed the row's factor — the long window proves sustained burn and the short window proves it still burns now, so a resolved incident resets without paging for its own tail; the budget-share figure derives from factor, long window, and the objective's own window at derivation time, so the headline an operator reads cannot disagree with the thresholds that fired it. Compliance floor derives from the longest burn row rather than a literal, so a tuned row moves it with no edit here.
- Law: every `AlertSeverity` column reaches the deploy plane through `Specs` — `Rank` orders the two extremum walks, `Hold` fills the spec's own dwell column, and the posture's key and urgency ride the annotation set — so a column the compile leg cannot read has no seat on this ladder.
- Law: the severity ORDER has one fold. `Stat.Extrema` is the branch's one banded extremum mint (`Rasm` RULINGS `[02]`), so escalation reads the least rank above a row and the pack verdict the greatest rank fired, and a row inserted into the ladder joins both walks with no edit. `Stride` is the band those folds carry: every ordinal this page folds is DECLARED with unit stride, so any band below one stride ties nothing and the fold reads exact; the residual lane's own floor is open, so a bare zero has no construction path.
- Output: `SloVerdict` carries per-row burn verdicts and the dominant severity as data a caller routes on; emission, delivery routing, and rule provisioning belong to the consuming plane.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a tuned discipline is one `BurnRow` value edit every consumer re-derives; a new routing posture is one `AlertPosture` row and one `AlertSeverity` row naming it, joining both extremum walks by rank alone and reaching the deploy plane through the annotations `Specs` already writes.
- Boundary: the severity roster is exactly `page` and `ticket` — the vocabulary the deploy plane's contact rows already key on — so the compile leg receives one dialect and a rank-ordered incident ladder rides the `Rank` and `Escalated` columns inside those two rows rather than a second severity type; delivery receivers, schedules, and escalation chains are deploy-plane configuration keyed by the severity row, never spec data. `AlertSpec` crosses a deploy plane whole, as data — annotation values are `string` because every one the derivation writes is a key or a name, and every declared severity column reaches that plane through them: the dwell rides `Hold`, the routing pair rides the posture annotations, so no column on the ladder is a policy nothing compiles.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using NodaTime;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlertPosture {
    public static readonly AlertPosture Warning = new("warning", urgency: "queue");
    public static readonly AlertPosture Critical = new("critical", urgency: "interrupt");

    public string Urgency { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlertSeverity {
    public static readonly AlertSeverity Ticket = new("ticket", rank: 0, holdMinutes: 30, posture: AlertPosture.Warning);
    public static readonly AlertSeverity Page = new("page", rank: 1, holdMinutes: 0, posture: AlertPosture.Critical);

    internal static Tolerance Stride => StrideBand.Value;
    private static readonly Lazy<Tolerance> StrideBand = new(static () =>
        Tolerance.Of(lane: ToleranceLane.Duplicate, value: EpsilonPolicy.SqrtEpsilon, key: Op.Of(name: "objective.stride")).ThrowIfFail());

    public int Rank { get; }

    public int HoldMinutes { get; }

    public AlertPosture Posture { get; }

    public Duration Hold => Duration.FromMinutes(HoldMinutes);

    public AlertSeverity Escalated =>
        Stat.Extrema(
            items: toSeq(Items).Filter(row => row.Rank > Rank),
            projection: static row => row.Rank,
            band: Stride,
            direction: ExtremumDirection.Minimum).Head.IfNone(this);

    public static Option<AlertSeverity> Dominant(Seq<AlertSeverity> fired) =>
        Stat.Extrema(
            items: fired,
            projection: static row => row.Rank,
            band: Stride,
            direction: ExtremumDirection.Maximum).Head;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BurnRow {
    public static readonly BurnRow PageFast = new("page-fast", factor: 14.4d, longMinutes: 60, shortMinutes: 5, severity: AlertSeverity.Page);
    public static readonly BurnRow PageSlow = new("page-slow", factor: 6d, longMinutes: 360, shortMinutes: 30, severity: AlertSeverity.Page);
    public static readonly BurnRow TicketFast = new("ticket-fast", factor: 3d, longMinutes: 1_440, shortMinutes: 120, severity: AlertSeverity.Ticket);
    public static readonly BurnRow TicketSlow = new("ticket-slow", factor: 1d, longMinutes: 4_320, shortMinutes: 360, severity: AlertSeverity.Ticket);

    public double Factor { get; }

    public int LongMinutes { get; }

    public int ShortMinutes { get; }

    public AlertSeverity Severity { get; }

    public Duration Long => Duration.FromMinutes(LongMinutes);

    public Duration Short => Duration.FromMinutes(ShortMinutes);

    public static Duration Floor =>
        Stat.Extrema(
            items: toSeq(Items),
            projection: static row => row.LongMinutes,
            band: AlertSeverity.Stride,
            direction: ExtremumDirection.Maximum).Head.Match(Some: static row => row.Long, None: static () => Duration.Zero);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Objective {
    public string Name { get; }
    public Sli Sli { get; }
    public double Target { get; }
    public Duration Window { get; }

    public double Budget => 1d - Target;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string name, ref Sli sli, ref double target, ref Duration window) {
        window = window == Duration.Zero ? Duration.FromDays(28) : window;
        validationError =
            !string.IsNullOrWhiteSpace(name)
            && name.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-')
            && sli is not null
            && target is > 0d and < 1d
            && window >= BurnRow.Floor
                ? null
                : new ValidationError(message:
                    $"Objective requires a dotted lowercase name, a target inside (0,1), and a window of at least {BurnRow.Floor}: {name}");
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct BurnReading(SloSample Long, SloSample Short);

[Union]
public abstract partial record BurnVerdict {
    private BurnVerdict() { }

    public abstract BurnRow At { get; }

    public sealed record Firing(BurnRow Row, double Long, double Short) : BurnVerdict { public override BurnRow At => Row; }
    public sealed record Quiet(BurnRow Row, double Long, double Short) : BurnVerdict { public override BurnRow At => Row; }
    public sealed record Unread(BurnRow Row) : BurnVerdict { public override BurnRow At => Row; }
}

public sealed record SloVerdict(Seq<BurnVerdict> Rows, Option<AlertSeverity> Severity);

public sealed record AlertSpec(
    string Slug,
    BurnRow Burn,
    AlertSeverity Severity,
    Duration Hold,
    Sli Sli,
    double Target,
    double Spend,
    Seq<KeyValuePair<string, string>> Annotations);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Slo {
    public const string ObjectiveSlot = "rasm.slo.objective";
    public const string SeveritySlot = "rasm.slo.severity";
    public const string BurnSlot = "rasm.slo.burn";
    public const string PostureSlot = "rasm.slo.posture";
    public const string UrgencySlot = "rasm.slo.urgency";

    public static double Burn(Objective objective, double errorRate) => errorRate / objective.Budget;

    public static double Share(BurnRow row, Objective objective) =>
        row.Factor * row.Long.TotalSeconds / objective.Window.TotalSeconds;

    public static SloVerdict Evaluate(Objective objective, Func<BurnRow, BurnReading> readings) {
        Seq<BurnVerdict> rows = toSeq(BurnRow.Items).Map(row => Verdict(objective, row, readings(row))).Strict();
        return new SloVerdict(
            Rows: rows,
            Severity: AlertSeverity.Dominant(
                rows.Choose(static row => row is BurnVerdict.Firing fired ? Some(fired.At.Severity) : None).Strict()));
    }

    public static Seq<AlertSpec> Specs(Objective objective) =>
        toSeq(BurnRow.Items).Map(row => new AlertSpec(
            Slug: $"{objective.Name}:{row.Key}",
            Burn: row,
            Severity: row.Severity,
            Hold: row.Severity.Hold,
            Sli: objective.Sli,
            Target: objective.Target,
            Spend: Share(row, objective),
            Annotations: Seq(
                new KeyValuePair<string, string>(ObjectiveSlot, objective.Name),
                new KeyValuePair<string, string>(SeveritySlot, row.Severity.Key),
                new KeyValuePair<string, string>(BurnSlot, row.Key),
                new KeyValuePair<string, string>(PostureSlot, row.Severity.Posture.Key),
                new KeyValuePair<string, string>(UrgencySlot, row.Severity.Posture.Urgency)))).Strict();

    public static Validation<Error, Objective> Admit(HashMap<string, InstrumentSpec> roster, Objective objective) =>
        objective.Sli is var sli
        && sli.Wellformed
        && sli.Series.ForAll(name => roster.Find(name).Match(
            Some: row => sli.Admits.Exists(kind => kind.Equals(row.Kind))
                && sli.Breaks.ForAll(key => row.Dimensions.Exists(declared => declared == key))
                && (sli is not Sli.Latency latency
                    || (string.Equals(row.Unit, Buckets.Seconds, StringComparison.Ordinal)
                        && row.Bounds.Exists(buckets => buckets.Bounds.Contains(latency.Ceiling.TotalSeconds)))),
            None: static () => false))
            ? Validation<Error, Objective>.Success(objective)
            : Validation<Error, Objective>.Fail(new KernelFault.InvalidValue(
                Label: objective.Name,
                Requirement: $"a wellformed indicator whose series declare as {string.Join(" or ", sli.Admits.Map(static kind => kind.Key))}, name every partition key, and pin a latency ceiling ON the declared seconds bucket ladder"));

    private static BurnVerdict Verdict(Objective objective, BurnRow row, BurnReading reading) =>
        reading.Long.Rate.Bind(slow => reading.Short.Rate.Map(fast => (Slow: Burn(objective, slow), Fast: Burn(objective, fast))))
            .Match(
                Some: burn => burn.Slow >= row.Factor && burn.Fast >= row.Factor
                    ? new BurnVerdict.Firing(Row: row, Long: burn.Slow, Short: burn.Fast)
                    : (BurnVerdict)new BurnVerdict.Quiet(Row: row, Long: burn.Slow, Short: burn.Fast),
                None: () => new BurnVerdict.Unread(Row: row));
}
```

## [04]-[BOARD]

- Owner: `PanelKind` is the closed board vocabulary a descriptor row names, its `For` projection carrying the canonical widget per measurement shape; `PanelSpec` the board descriptor over one declared instrument and the dimension keys it breaks on; `BoardPack` the per-sink pack carrying its provenance key beside panels and objectives under one admission.
- Cases: eight panel rows; a widget is `None` on a descriptor deferring to its row's own measurement shape.
- Entry: `PanelSpec.Of` is the ONE descriptor entry — one name, two overloads discriminated by the widget's presence in the value (`Of(title, instrument, params by)` derives the widget through `PanelKind.For` at admission; `Of(title, instrument, widget, params by)` pins it), each performing the params-to-`Seq` break-key lift so no call site spells `Seq`/`None` ceremony; the record constructor remains the general spelling a projection composes. `PanelSpec.Admit(roster)` resolves one panel's widget against the declared row's measurement shape after proving every break key against those same dimensions, and `BoardPack.Admit(roster)` is the one pack-wide proof.
- Auto: measurement shape carries a canonical reading, so a descriptor plane derives its default panel instead of re-deciding one per package; a board wanting a different widget overrides on its own row.
- Law: three INDEPENDENT claims close a pack — panels resolve widget and break keys, objectives resolve series and partition keys, and objective names stay distinct — so the fold ACCUMULATES rather than aborting: a pack with four broken panels and a collided objective name reports all five, each named. Name collision is the claim no per-row admission can make, because each row sees a single objective and a collided slug silently overwrites its twin's rules at the deploy plane rather than refusing anywhere.
- Law: `BoardPack` carries the provenance key the deploy plane admits it under as its FIRST column, so pack and key travel as one value and a key spelled only at the consuming tier has no construction path; the deploy tuple owns the closed vocabulary of admitted keys, so this column stays a plain `string` here and refuses at that boundary rather than forking a second roster in this branch.
- Output: `Admit` exits `ToFin` at the pack boundary, so a caller reads one rail while the accumulation stays inside the fold that earned it.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new visualization is one `PanelKind` row every pack reads through `For` without an edit; a new board row is one `PanelSpec` on the owning pack; a new pack-wide claim is one leg in the applicative product every sink inherits.
- Boundary: panel rows name visualization alone and carry no query dialect, provider field, or datasource binding, and a break key outside the declared row's own dimensions refuses at pack admission where the descriptor is still editable rather than at the first empty render.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PanelKind {
    public static readonly PanelKind Timeseries = new("timeseries");
    public static readonly PanelKind Stat = new("stat");
    public static readonly PanelKind Gauge = new("gauge");
    public static readonly PanelKind Heatmap = new("heatmap");
    public static readonly PanelKind Logs = new("logs");
    public static readonly PanelKind Table = new("table");
    public static readonly PanelKind Geomap = new("geomap");
    public static readonly PanelKind Nodes = new("nodes");

    public static PanelKind For(InstrumentKind measure) => measure.Switch(
        count: static () => Timeseries,
        delta: static () => Timeseries,
        distribution: static () => Heatmap,
        reading: static () => Stat,
        total: static () => Timeseries,
        balance: static () => Timeseries,
        level: static () => Gauge,
        levels: static () => Table);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PanelSpec(string Title, string Instrument, Seq<string> By, Option<PanelKind> Widget) {
    public static PanelSpec Of(string title, string instrument, params ReadOnlySpan<string> by) =>
        new(title, instrument, toSeq(by.ToArray()), None);
    public static PanelSpec Of(string title, string instrument, PanelKind widget, params ReadOnlySpan<string> by) =>
        new(title, instrument, toSeq(by.ToArray()), Some(widget));

    public Validation<Error, PanelKind> Admit(HashMap<string, InstrumentSpec> roster) =>
        roster.Find(Instrument).Filter(row => By.ForAll(key => row.Dimensions.Exists(declared => declared == key)))
            .Match(
                Some: row => Validation<Error, PanelKind>.Success(Widget.IfNone(PanelKind.For(row.Kind))),
                None: () => Validation<Error, PanelKind>.Fail(new KernelFault.InvalidValue(
                    Label: Title, Requirement: $"a declared {Instrument} row naming every break key")));
}

public sealed record BoardPack(string Wire, Seq<PanelSpec> Panels, Seq<Objective> Objectives) {
    public Seq<AlertSpec> Alerts => Objectives.Bind(Slo.Specs).Strict();

    public Fin<BoardPack> Admit(HashMap<string, InstrumentSpec> roster) {
        BoardPack self = this;
        return (Panels.Traverse(panel => panel.Admit(roster)).As(),
                Objectives.Traverse(objective => Slo.Admit(roster, objective)).As(),
                Named(Objectives))
            .Apply((_, _, _) => self).As().ToFin();
    }

    private static Validation<Error, Unit> Named(Seq<Objective> objectives) =>
        objectives.Collisions(static row => row.Name) is { IsEmpty: false } collided
            ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
                Label: string.Join(", ", collided), Requirement: "one objective per alert-namespace name"))
            : Validation<Error, Unit>.Success(unit);
}
```

## [05]-[BENCH]

- Owner: `BenchClaim` is the typed speed-claim row — the `Op` key naming the gated lane, the exact vectorized and reference member spellings under measurement, the `SpeedupFloor` the corpus gate enforces, and the `Corpus` slug naming the compile-time corpus a corpus-bound claim measures against; `BenchLedger` is the enumerable fold the corpus gate ingests.
- Entry: `Of` admits every row and proves key distinctness in ONE applicative product, so an eight-row ledger with three invalid rows and two collided keys reports five refusals each naming its own `Op`; `Rows` is the enumeration the corpus gate reads; `Unproven` returns every claim lacking a benchmark proof.
- Auto: `Unproven` folds the proof stream into one keyed index before it filters, so an N-row ledger against M proofs costs one pass each rather than the nested scan the prior membership probe ran per row.
- Law: claim rows live BESIDE the lanes they gate as `static readonly` rows on their owning pages, and the app composition root composes them into the ledger — the substrate floor never references an upper stratum, so the ledger cannot mint the rows itself. Folder-local claim `[SmartEnum]` rosters re-spelling this row shape are the deleted form: Bim's observability roster and Fabrication's guard roster are `static readonly BenchClaim` rows.
- Law: proof is corpus-aware — a claim carrying a `Corpus` slug is proven only by a benchmark proof whose corpus fingerprint is present, and a claim with `Corpus: None` is proven by the key alone, so a proof that measured no corpus never discharges a corpus-bound claim.
- Law: a claim is correctness-independent — the vectorized lane's result never depends on it; the claim gates only admission to the hot path, and a lane whose claim fails reverts to its reference row with zero behavior change.
- Packages: LanguageExt.Core.
- Growth: a new gated lane is one `BenchClaim` row beside it and one argument at the composing root.
- Boundary: `Rasm.AppHost`'s corpus gate reads `Rows` and resolves each claim to its benchmark verdict; judging, regression budgets, and host-evidence binding are the gate's — this ledger owns only the typed enumeration and the duplicate-refusal fold.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BenchClaim(
    Op Claim, string VectorizedLane, string ReferenceLane, double SpeedupFloor, Option<string> Corpus = default)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: SpeedupFloor),
        !string.IsNullOrWhiteSpace(value: VectorizedLane),
        !string.IsNullOrWhiteSpace(value: ReferenceLane),
        Corpus.Map(static slug => !string.IsNullOrWhiteSpace(value: slug)).IfNone(noneValue: true));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class BenchLedger {
    private BenchLedger(Seq<BenchClaim> rows) => Rows = rows;

    public Seq<BenchClaim> Rows { get; }

    public static Fin<BenchLedger> Of(params ReadOnlySpan<BenchClaim> claims) {
        Seq<BenchClaim> rows = toSeq(claims.ToArray());
        HashMap<Op, int> census = rows.Fold(
            HashMap<Op, int>(),
            static (held, row) => held.AddOrUpdate(row.Claim, static seats => seats + 1, static () => 1));
        return (rows.Traverse(Admitted).As(), rows.Traverse(row => Distinct(row, census)).As())
            .Apply(static (admitted, _) => new BenchLedger(rows: admitted)).As().ToFin();
    }

    public Seq<BenchClaim> Unproven(Seq<(Op Claim, Option<UInt128> Corpus)> proven) {
        HashMap<Op, Option<UInt128>> index = proven.Fold(
            HashMap<Op, Option<UInt128>>(),
            static (held, proof) => held.AddOrUpdate(
                proof.Claim,
                seated => seated.IsSome ? seated : proof.Corpus,
                () => proof.Corpus));
        return Rows.Filter(row => index.Find(row.Claim).Match(
            Some: corpus => row.Corpus.IsSome && corpus.IsNone,
            None: static () => true));
    }

    private static Validation<Error, BenchClaim> Admitted(BenchClaim row) =>
        row.IsValid
            ? Validation<Error, BenchClaim>.Success(row)
            : Validation<Error, BenchClaim>.Fail(new KernelFault.InvalidValue(
                Label: row.Claim.ToString(), Requirement: "a positive speedup floor and non-blank lane spellings"));

    private static Validation<Error, Unit> Distinct(BenchClaim row, HashMap<Op, int> census) =>
        census.Find(row.Claim).IfNone(0) > 1
            ? Validation<Error, Unit>.Fail(new KernelFault.InvalidValue(
                Label: row.Claim.ToString(), Requirement: "one ledger row per claim key"))
            : Validation<Error, Unit>.Success(unit);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
