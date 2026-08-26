# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimPoint` closes the `rasm.bim.<domain>.<point>` roster onto the kernel `IHookRoster` floor and `BimHooks.Live` mints the ONE kernel `HookSet` over it, `BimTelemetry` projects typed facts onto `rasm.bim.<domain>.<measure>` instruments as a hook subscriber — domain code fires facts and observability projects them, zero emit calls inside a projector or codec arm — and `BimBenchClaims` fixes typed, corpus-bound claim identity for every measured BIM lane.

Wire posture: HOST-LOCAL, BCL-only. Point capsule, dispatcher, mount custody, fault cell, instrument-spec, advice-bucket, contributor-port, package identity, tenancy frame, numeric fault identity, recovery, and trace-band machinery arrive settled from the kernel, so no OpenTelemetry package is reachable here; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots, which admit the `Rasm.Bim` meter by name and `BimPoint.Scopes` into their one `SpanBand`. Subscriber failure parks point-attributed on the hooks' evidence cell, the emitter untouched.

## [01]-[INDEX]

- [02]-[HOOKS]: `BimPoint` closes the point vocabulary on its kernel `Modalities` set and derived `TraceScope` plane, `BimFact` closes the payload family and realizes the kernel `IHookFact` floor through its `Point` projection, `ProgressLane` closes the lanes sharing the progress case, `GlobalIdSet`/`ContentKeySet`/`BimIssueMutation` close the set and mutation vocabularies, `StageMark` carries stage evidence, and `BimHooks.Live` mints the one kernel `HookSet`.
- [03]-[TELEMETRY_TAP]: `BimInstrument` declares the `rasm.bim.<domain>.<measure>` roster as rows CARRYING their kernel `InstrumentSpec` and mints the contributor port; `BimTelemetry` routes the one fact-to-write projection, binds the hooks' parked-fault depth, and owns the span and attribution law over the kernel `SpanBand`.
- [04]-[BENCH_CLAIMS]: `BimBenchClaims` rosters the per-op kernel `BenchClaim` rows consumed directly by the AppHost benchmark composition.

## [02]-[HOOKS]

- Owner: `BimPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.bim.<domain>.<point>` realizing the kernel `IHookRoster<BimPoint>` floor with a `CapabilitySet<HookModality>` column and the kernel `TraceScope` plane derived off the id's own head; `BimFact` the closed payload family every point types over; `ProgressLane` the three-row lane vocabulary the shared progress case discriminates on; `GlobalIdSet`, `ContentKeySet`, and `BimIssueMutation` the set and mutation vocabularies its cases carry; `BimHooks` the composition entry minting the ONE kernel `HookSet<BimPoint, BimFact, TelemetrySource>`. The folder mints zero hook mechanism — seats, veto and observe capsules, scoped taps, detach custody, mount census, and the evidence cell are the kernel's.
- Cases: point roster rows — `rasm.bim.exchange.progress` (`BimFact.Progress` under `ProgressLane.Exchange` — the DWG/DXF decode lane's `StageMark` stream off ACadSharp `ICadReader.OnProgress`), `rasm.bim.exchange.imported` (`BimFact.Imported` — the decoded-source fact post-decode), `rasm.bim.exchange.exported` (`BimFact.Exported` — the export-path artifact emit carrying the sealed content key), `rasm.bim.projection.lowered` (`BimFact.Lowered` — the shared `GraphDelta` magnitude off the semantic projector), `rasm.bim.projection.legality` (veto beside observe, `BimFact.Admission` — an app policy refuses a graph delta before it lands), `rasm.bim.projection.emit` (veto beside observe, `BimFact.Egress` — the `Projection/egress#IFC_EGRESS` `Emit` fold consults it against the elected format and target schema BEFORE authoring, so a deliverable policy refuses on the exchange coordinates rather than on a delta that already landed), `rasm.bim.review.committed` (`BimFact.Committed` — the `Review/versioning#VERSION_GRAPH` `BimRepository.Seal` funnel, the one point both `Commit` and `CommitMerge` reach), `rasm.bim.review.issue` (`BimFact.IssueMutated` — the `Review/issues#BCF_ARCHIVE` board mutation under its closed `BimIssueMutation` key), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — IDS-facet and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (`ProgressLane.Energy` — the energy translate lane's `TranslateStage` rows projecting the mark off OpenStudio `ProgressBar.onPercentageUpdated`), `rasm.bim.planning.progress` (`ProgressLane.Planning` — the `Planning/schedule#SCHEDULE` CPM pass (`CpmStage`) and the `Planning/cost#ESTIMATE` decomposition rollup (`EstimateStage`) over long networks), `rasm.bim.energy.emitted` (`BimFact.Emitted` — the `EnergyCensus` fact per artifact under its `ArtifactKey` address), `rasm.bim.exchange.textured` (`BimFact.Textured` — the appearance channel census `MaterialFinish.Author` and `AppearanceProjection.TexturesOf` fire once per authored surface style, so the one exchange leg that drops payload by design is counted rather than silent), `rasm.bim.exchange.degrade` (`BimFact.Degraded` — an exchange leg completing while shedding capability, its closed lane and reason keys banding the counter while the identifier-grade subject rides the fact alone).
- Law: `BimFact` is the branch's ONE domain-fact family and every announcement projects from it — `Exchange/events#EVENT_PROJECTION` subscribes the announced points as tap rows, so a second union over this fact space forks the address, the trace, and the vocabulary keys one fire already carries. Cases therefore hold the ADDRESS an announcement subjects on — the commit key, the artifact content key, the `ArtifactKey` text, the topic guid, the specification ordinal — because an address recomputed at a subscriber reads a second identity for the fact its emitter already named.
- Law: the roster and the fact family are ONE correspondence — `BimFact.Point` projects each case onto its owning row through the generated `Map`, so a case with no row, or a row no case reaches, breaks at compile time, and the kernel `IHookFact<BimPoint>` floor's `Seats` derives from that one map rather than a hand-mirrored case list. The three progress points share one case and `ProgressLane` carries the discriminant, so the lane a fire site names IS the point it fires and no site spells both.
- Entry: `BimHooks.Live(key, gates, taps, band, cell)` mints the dispatcher once at composition; an emitting page fires its declared row (`hooks.Fire(BimPoint.Imported, fact, key)`), and the guarded arity hands its body the ADMITTED fact so a veto transform governs the contract it brackets. `Points` is the census the app root hands `HookRegistry.Mount`, and `BimPoint.Scopes` enters that root's `SpanBand.Of(version, scopes)`.
- Auto: fire order, veto folding, tap shielding, replay retention, and scoped release are the kernel hooks's law — retention first, the veto left-fold second (its first refusal is the emitter's verdict AND parks on the cell), forked shielded taps last, so `Fire` returns without waiting on any tap. `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimHooks> hooks = default` — the optional slot every later fire-site entry repeats — so a hook-less composition pays one `IsNone` test.
- Output: the fired `BimFact` carries the fact whole — the emitter's typed result already holds it, so a point mints nothing; the hooks' `FaultCell` is the one evidence surface — veto refusals and shielded tap faults, point-attributed and clock-stamped — bounded by the kernel `Ring`, so a tap storm reads as `Shed` and `Lost` counts rather than as process memory, and `[03]` binds its depth as a pulled level.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, Rasm.Element, BCL inbox.
- Growth: a new point is one `BimPoint` row and one `BimFact` case, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `HookGate`/`HookTap` row at the composition's `Live` call, narrowing through the tap's own `Scope` column rather than through a point probe inside its delegate; delivery semantics are the kernel modality rows; a new native lane keeps its OWN closed stage roster projecting `StageMark` — the carrier is this page's, the rows are the lane's, and a second mark shape or a free-text stage slot is the deleted form.
- Boundary: NAMED LOSS carried from the kernel hook collapse — the COMPILE-TIME shape of the per-point fact type. A `HookPoint<BimFact.*>` column typed each seat to one case, so an `Imported` subscriber never saw an exported fact; under one dispatcher every point shares `BimFact` and subscribers discriminate on the case. What survives is the roster row's modality admission, the union's closure, and — through the kernel `IHookFact<BimPoint>` floor this page realizes — the case narrowing itself as a RUNTIME gate: `Seats` derives from the `Point` correspondence, and `Fire` reads it at entry AND on the veto fold's product, so a fact fired at a foreign seat and a gate rewriting to a sibling case both refuse before any tap runs. Only the compile break is lost; the guarantee is not. WITNESS: `BimHooks.Live` is one expression where the deleted seat record was a per-point constructor column, a per-point mint line, a per-point census entry, and a private `Seat<TFact>` factory.
- Boundary: point ids compose the kernel `HookId` grammar with the package segment pinned `bim`, so a Bim point joins any app-tier registry census unrenamed — Bim declares its points here and the composing app subscribes direct; ids, planes, and modalities derive from the roster row alone, so an inline `HookId.Create` at a fire site does not compile; the payload closes at declaration, so a stringly payload cannot enter the dispatcher; cases carry the CLOSED vocabulary key a sibling owner published (`InterchangeFormat.Key`, `IdsOutcome.Key`, `RuleSeverity.Key`, `ArtifactKey.Value`) rather than that owner's type, so this S0 Model stratum consumes no Exchange, Energy, or Review sibling type and each announcement re-admits the key through its owning gate; telemetry is a tap, never a producer — `[03]-[TELEMETRY_TAP]` subscribes one tap row here and `Exchange/events#EVENT_PROJECTION` subscribes beside it.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using BimGate = Rasm.Domain.HookGate<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimObserver = Rasm.Domain.HookTap<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class BimPoint : IHookRoster<BimPoint> {
    public static readonly BimPoint ExchangeProgress = new("rasm.bim.exchange.progress", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Imported = new("rasm.bim.exchange.imported", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Exported = new("rasm.bim.exchange.exported", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Lowered = new("rasm.bim.projection.lowered", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Legality = new("rasm.bim.projection.legality", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly BimPoint Egress = new("rasm.bim.projection.emit", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly BimPoint Committed = new("rasm.bim.review.committed", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint IssueMutated = new("rasm.bim.review.issue", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Verdict = new("rasm.bim.review.verdict", CapabilitySet<HookModality>.Of(HookModality.Replay));
    public static readonly BimPoint EnergyProgress = new("rasm.bim.energy.progress", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint PlanningProgress = new("rasm.bim.planning.progress", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Emitted = new("rasm.bim.energy.emitted", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint Textured = new("rasm.bim.exchange.textured", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly BimPoint ExchangeDegrade = new("rasm.bim.exchange.degrade", CapabilitySet<HookModality>.Of(HookModality.Observe));

    static readonly Lazy<FrozenDictionary<BimPoint, (HookId Id, TraceScope Plane)>> Index = new(
        static () => Items.ToFrozenDictionary(static row => row, static row =>
            (HookId.Create(value: row.Key), TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3])))),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Index.Value[this].Id;

    public Option<TraceScope> Plane => Some(Index.Value[this].Plane);

    public static Seq<TraceScope> Scopes => toSeq(Index.Value.Values).Map(static entry => entry.Plane).Distinct().Strict();
}

[SmartEnum<string>]
public sealed partial class ProgressLane {
    public static readonly ProgressLane Exchange = new("exchange", static () => BimPoint.ExchangeProgress);
    public static readonly ProgressLane Energy = new("energy", static () => BimPoint.EnergyProgress);
    public static readonly ProgressLane Planning = new("planning", static () => BimPoint.PlanningProgress);

    [UseDelegateFromConstructor]
    public partial BimPoint At();
}

[ValueObject<Seq<string>>]
public sealed partial class GlobalIdSet {
    const int Glyphs = 22;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<string> value) {
        if (!value.ForAll(Admits)) {
            validationError = new ValidationError("global-id-glyphs");
            return;
        }
        value = toSeq(value.Distinct(StringComparer.Ordinal).OrderBy(static id => id, StringComparer.Ordinal));
    }

    public static Fin<GlobalIdSet> Admit(ImmutableArray<string> values, Op key) =>
        WireSet.Ordered(values) && TryCreate(toSeq(values), out GlobalIdSet? admitted) && admitted is { } set
            ? Fin.Succ(set)
            : Fin.Fail<GlobalIdSet>(new BimFault.Refused(key, BimScope.Events, BimReason.Codec, string.Join(':', new object?[] { "event-set-malformed", "globalIds" })));

    static bool Admits(string? value) =>
        value is { Length: Glyphs } && value.All(static glyph =>
            glyph is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '$');
}

[ValueObject<Seq<UInt128>>]
public sealed partial class ContentKeySet {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<UInt128> value) =>
        value = toSeq(value.Distinct().OrderBy(static k => k));

}

static class WireSet {
    public static bool Ordered(ImmutableArray<string> values) =>
        values.Distinct(StringComparer.Ordinal).Count() == values.Length
        && values.SequenceEqual(values.OrderBy(static value => value, StringComparer.Ordinal), StringComparer.Ordinal);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimIssueMutation {
    public static readonly BimIssueMutation TopicOpened = new("topic-opened");
    public static readonly BimIssueMutation TopicRevised = new("topic-revised");
    public static readonly BimIssueMutation CommentAdded = new("comment-added");
    public static readonly BimIssueMutation ViewpointAdded = new("viewpoint-added");
    public static readonly BimIssueMutation StatusAdvanced = new("status-advanced");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StageMark(double Done, string Witness);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFact : IHookFact<BimPoint> {
    private BimFact(Op key) => Key = key;

    public Op Key { get; }

    public bool Seats(BimPoint at) => Point.Equals(at);

    public BimPoint Point => this.Map(
        progress: static f => f.Lane.At(),
        imported: static _ => BimPoint.Imported,
        exported: static _ => BimPoint.Exported,
        lowered: static _ => BimPoint.Lowered,
        admission: static _ => BimPoint.Legality,
        egress: static _ => BimPoint.Egress,
        committed: static _ => BimPoint.Committed,
        issueMutated: static _ => BimPoint.IssueMutated,
        verdict: static _ => BimPoint.Verdict,
        emitted: static _ => BimPoint.Emitted,
        textured: static _ => BimPoint.Textured,
        degraded: static _ => BimPoint.ExchangeDegrade);

    public sealed record Progress(Op Key, ProgressLane Lane, StageMark Stage) : BimFact(Key);
    public sealed record Imported(Op Key, string Format, string Codec, long Bytes, int Blocks, int Instances, Duration Elapsed) : BimFact(Key);
    public sealed record Exported(Op Key, UInt128 ContentKey, string Format, long Bytes, Duration Elapsed) : BimFact(Key);
    public sealed record Lowered(Op Key, string Projector, int Nodes, int Edges) : BimFact(Key);
    public sealed record Admission(Op Key, GraphDelta Delta) : BimFact(Key);
    public sealed record Egress(Op Key, string Format, string Schema, int Nodes) : BimFact(Key);
    public sealed record Committed(Op Key, UInt128 CommitKey, ContentKeySet Parents, string Branch, int Elements) : BimFact(Key);
    public sealed record IssueMutated(Op Key, string Topic, BimIssueMutation Mutation, Option<string> Comment, GlobalIdSet GlobalIds) : BimFact(Key);
    public sealed record Verdict(
        Op Key, string Specification, int Spec, ContentAddress Model, string Tier, string Outcome, string Severity,
        int Findings, GlobalIdSet GlobalIds) : BimFact(Key);
    public sealed record Emitted(Op Key, string Artifact, string Leg, string Format, int Warnings) : BimFact(Key);
    public sealed record Textured(Op Key, string Format, int Bound, int Dropped, int Unresolved) : BimFact(Key);
    public sealed record Degraded(Op Key, string Lane, string Reason, string Subject) : BimFact(Key);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class BimHooks {
    public static Fin<BimHooks> Live(
        Op key, Seq<BimGate> gates = default, Seq<BimObserver> taps = default,
        Option<SpanBand> band = default, Option<FaultCell> cell = default) =>
        BimHooks.Of(key, gates, taps, band.Map(static span => (IHookSpan)span), cell)
            ;
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimInstrument` the closed `rasm.bim.*` roster — a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) so `Rows` derives from `Items` and construction proves each row's name against its key — beside the one dotted slot block both the metric rows and the `[02]` span marks spell, with the contributor-port mint under the kernel `TelemetrySource.Bim` scope; `BimTelemetry` the fact-to-write projection over the `InstrumentSet` the composing root materializes. Results stay the truth; instruments are the lossy dashboard channel projected from the facts they fire.
- Cases: projection map — `rasm.bim.exchange.import.duration`/`import.size`/`instancing` off `BimFact.Imported` (duration, payload size, and instance placements sharing one evidence read), `rasm.bim.exchange.export.duration` off `BimFact.Exported`, `rasm.bim.projection.nodes`/`edges` off `BimFact.Lowered`, `rasm.bim.review.commits` off `BimFact.Committed`, `rasm.bim.review.mutations` off `BimFact.IssueMutated` banded by the closed mutation key, `rasm.bim.review.verdicts` off `BimFact.Verdict`, `rasm.bim.energy.exchanges`/`warnings` off `BimFact.Emitted`, `rasm.bim.exchange.texture.drops` off `BimFact.Textured` banded by format and drop cause (the two loss causes write, the bound count does not — it is the artifact's own evidence), `rasm.bim.exchange.degrades` off `BimFact.Degraded` banded by lane and reason, and the pulled `rasm.bim.observe.tap.faults` bound per point off the hooks' own evidence cell.
- Entry: `BimInstrument.Telemetry(version)` is the contributor port the composing root materializes — the semconv coordinate is the kernel pin so all three signals bump together — and a root outside that fan binds `InstrumentSet.Of(cells, (meter, BimInstrument.Rows))` directly against its own minted meter; either path, never both. `BimTelemetry.Tap(set)` returns the ONE tap row passed to `BimHooks.Live`; `BimTelemetry.Depth(set, hooks, key)` registers the hooks' parked-fault read per point and returns the scopes that retire them, so the root arms it AFTER the mint the tap fed. `BimTelemetry.Traced<T>(band, at, op, model, body, marks)` is the span wrapper every long-running entry composes over the composing root's kernel band, model identity a required argument so no Bim span exists unattributed and the band nullable so a composition admitting no scope runs the identical dispatcher untraced.
- Auto: every advised row ships its explicit-bucket bounds through the kernel's `InstrumentKind` x `MeasureForm` derivation, so this page names a bound row and never a create call; instrument identity de-duplicates by name inside the meter, and the write plane addresses by ROW, so the two-map-one-key pair a string name forced has no spelling here; tag values ride the typed fact's own vocabulary keys, never free text.
- Law: a subscriber failure is captured by the kernel shield and never fires a fact, so the refusal series has NO pushed write site — `Depth` binds the cell's own parked read per point instead, one registration per roster row under its own `PointSlot` tag. Fault identity is evidence, not a string tag: `FaultCell.Parked` holds `IsolatedFault(Point, Cause, At)`, so a health panel reads the generated numeric code and locally derived recovery from the preserved `Cause` while the point axis stays a live series, and `Shed`/`Lost` report what the ring dropped.
- Output: `Fin<Unit>` per fact — the tap projects hook facts alone, and a metric minted beside it is a second truth. Five points project NO series by design: the three `Progress` streams are operator feedback whose live fraction is span-and-panel material, not a bounded series, and the two veto points reach the cell through their parked refusals — so an un-projected point is a stated posture, never an oversight. The projection's typed refusal rides straight out of the tap arm, so the capsule shield parks an unmounted row or a form mismatch as tap-attributed evidence rather than dropping a measurement silently.
- Packages: LanguageExt.Core, NodaTime, Rasm (the kernel instrument mechanism, scope identity roster, numeric fault identity and recovery, tenancy frame, and trace band), BCL inbox.
- Growth: a new projected fact is one `BimInstrument` row with its `Dimensions` and one arm on the total `Project` fold, which the generated `Switch` breaks at compile time; every row declares the kernel tenant slot beside its fact dimensions, while a new span dimension is one slot row here and one `marks` pair at the composing entry.
- Boundary: library altitude holds zero OpenTelemetry reference and zero span custody — the meter reaches the process only through the composing root's mint, so provider disposal owns instrument lifetime, and the kernel `SpanBand` owns the one `ActivitySource` per admitted scope, its listener gate, its `using` close, and its typed fail-leg status, so this page declares `BimPoint.Scopes` and holds no source, no wrapper, and no disposable; instrument custody is one-per-composition — either the app fan materializes the `Telemetry` port or a root binds `InstrumentSet.Of` locally, never both; subscription law — the tap mounts at `Live` ahead of the first fire, because the dispatcher fans a `Replay` point's held window to each fresh subscriber and a late attach therefore re-counts that window onto the verdict counter; span law — the span name IS the kernel `Op` and the plane the point's own id head, so bracket and fact never name two scopes, and the typed verdict, not a tag, carries the error fact; attribution law — dimension slots carry this package's dotted `rasm.bim.<dimension>` namespace so a concept a sibling package also tags never collides, fault identity stays on the preserved Error as generated numeric code plus locally derived recovery rather than a string owner/category tag, tenancy is the kernel `TenantContext` projection every metric write folds so this page holds no tenant key and no baggage read, and model identity is identifier-grade: it rides the span alone as `Traced`'s own required argument, because one metric series per model is unbounded cardinality no view cap recovers and a slot left to caller discipline is a slot no caller stamps; the span fold never re-stamps the tenant partition the app root's baggage promotion already carries; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;
using BimObserver = Rasm.Domain.HookTap<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Model;

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimInstrument {
    public const string ModelSlot = "rasm.bim.model";

    public const string CodecSlot = "rasm.bim.codec";
    public const string FormatSlot = "rasm.bim.format";
    public const string LegSlot = "rasm.bim.energy.leg";
    public const string OutcomeSlot = "rasm.bim.review.outcome";
    public const string PointSlot = "rasm.bim.point";
    public const string ProjectorSlot = "rasm.bim.projector";
    public const string TierSlot = "rasm.bim.review.tier";
    public const string MutationSlot = "rasm.bim.review.mutation";
    public const string ChannelSlot = "rasm.bim.exchange.texture.cause";
    public const string LaneSlot = "rasm.bim.exchange.lane";
    public const string ReasonSlot = "rasm.bim.exchange.reason";

    public static readonly BimInstrument ImportDuration = new(
        "rasm.bim.exchange.import.duration",
        InstrumentSpec.Create("rasm.bim.exchange.import.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "foreign-bytes decode wall duration per format and codec",
            Seq(TenantContext.TenantSlot, FormatSlot, CodecSlot), Some(Buckets.DecodeSeconds), None, None));

    public static readonly BimInstrument ImportSize = new(
        "rasm.bim.exchange.import.size",
        InstrumentSpec.Create("rasm.bim.exchange.import.size", InstrumentKind.Distribution, MeasureForm.Whole, "By",
            "decoded source byte count per format",
            Seq(TenantContext.TenantSlot, FormatSlot), Some(Buckets.ByteSizes), None, None));

    public static readonly BimInstrument Instancing = new(
        "rasm.bim.exchange.instancing",
        InstrumentSpec.Create("rasm.bim.exchange.instancing", InstrumentKind.Distribution, MeasureForm.Whole, "{instance}",
            "instance placements per decoded pool",
            Seq(TenantContext.TenantSlot, FormatSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly BimInstrument ExportDuration = new(
        "rasm.bim.exchange.export.duration",
        InstrumentSpec.Create("rasm.bim.exchange.export.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "artifact emit wall duration per format",
            Seq(TenantContext.TenantSlot, FormatSlot), Some(Buckets.DecodeSeconds), None, None));

    public static readonly BimInstrument ProjectionNodes = new(
        "rasm.bim.projection.nodes",
        InstrumentSpec.Create("rasm.bim.projection.nodes", InstrumentKind.Distribution, MeasureForm.Whole, "{node}",
            "graph delta node magnitude per projector",
            Seq(TenantContext.TenantSlot, ProjectorSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly BimInstrument ProjectionEdges = new(
        "rasm.bim.projection.edges",
        InstrumentSpec.Create("rasm.bim.projection.edges", InstrumentKind.Distribution, MeasureForm.Whole, "{edge}",
            "graph delta edge magnitude per projector",
            Seq(TenantContext.TenantSlot, ProjectorSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly BimInstrument ReviewCommits = new(
        "rasm.bim.review.commits",
        InstrumentSpec.Create("rasm.bim.review.commits", InstrumentKind.Count, MeasureForm.Whole, "{commit}",
            "model commits sealed through the one repository funnel",
            Seq(TenantContext.TenantSlot), None, None, None));

    public static readonly BimInstrument ReviewMutations = new(
        "rasm.bim.review.mutations",
        InstrumentSpec.Create("rasm.bim.review.mutations", InstrumentKind.Count, MeasureForm.Whole, "{mutation}",
            "issue-board mutations by closed mutation key",
            Seq(TenantContext.TenantSlot, MutationSlot), None, None, None));

    public static readonly BimInstrument ReviewVerdicts = new(
        "rasm.bim.review.verdicts",
        InstrumentSpec.Create("rasm.bim.review.verdicts", InstrumentKind.Count, MeasureForm.Whole, "{verdict}",
            "review verdicts by tier and outcome",
            Seq(TenantContext.TenantSlot, TierSlot, OutcomeSlot), None, None, None));

    public static readonly BimInstrument EnergyExchanges = new(
        "rasm.bim.energy.exchanges",
        InstrumentSpec.Create("rasm.bim.energy.exchanges", InstrumentKind.Count, MeasureForm.Whole, "{exchange}",
            "energy artifacts by leg and format",
            Seq(TenantContext.TenantSlot, LegSlot, FormatSlot), None, None, None));

    public static readonly BimInstrument EnergyWarnings = new(
        "rasm.bim.energy.warnings",
        InstrumentSpec.Create("rasm.bim.energy.warnings", InstrumentKind.Count, MeasureForm.Whole, "{warning}",
            "energy exchange warning tallies per format",
            Seq(TenantContext.TenantSlot, FormatSlot), None, None, None));

    public static readonly BimInstrument TextureDrops = new(
        "rasm.bim.exchange.texture.drops",
        InstrumentSpec.Create("rasm.bim.exchange.texture.drops", InstrumentKind.Count, MeasureForm.Whole, "{channel}",
            "appearance texture channels lost at binding, banded by cause",
            Seq(TenantContext.TenantSlot, FormatSlot, ChannelSlot), None, None, None));

    public static readonly BimInstrument ExchangeDegrades = new(
        "rasm.bim.exchange.degrades",
        InstrumentSpec.Create("rasm.bim.exchange.degrades", InstrumentKind.Count, MeasureForm.Whole, "{degradation}",
            "exchange legs completing with shed capability, banded by lane and reason",
            Seq(TenantContext.TenantSlot, LaneSlot, ReasonSlot), None, None, None));

    public static readonly BimInstrument TapFaults = new(
        "rasm.bim.observe.tap.faults",
        InstrumentSpec.Create("rasm.bim.observe.tap.faults", InstrumentKind.Level, MeasureForm.Whole, "{fault}",
            "veto refusals and shielded tap faults held on the composition's evidence cell",
            Seq(TenantContext.TenantSlot, PointSlot), None, None, None));

    public InstrumentSpec Row { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Bim, Version: version, Instruments: Rows, Planes: BimPoint.Scopes);

    static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
        if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
            throw new ArgumentException($"<bim-instrument:{key}>", nameof(row));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BimTelemetry {
    public static BimObserver Tap(InstrumentSet set) => new(Op.Of(name: "rasm.bim.instruments"), fact => Project(set, fact));

    public static Fin<Seq<IDisposable>> Depth(InstrumentSet set, BimHooks hooks, Op key) =>
        toSeq(BimPoint.Items).Traverse(row => set.Bind(
            BimInstrument.TapFaults.Row,
            () => (double)hooks.Faults.Parked.Count(fault => fault.Point.Equals(row.Id)),
            key,
            InstrumentSet.Tags(TenantContext.Current, (BimInstrument.PointSlot, (object?)row.Key)))).As();

    static Fin<Unit> Project(InstrumentSet set, BimFact fact) =>
        fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
            state: (set, TenantContext.Current),
            progress: static (_, _) => Fin.Succ(unit),
            admission: static (_, _) => Fin.Succ(unit),
            egress: static (_, _) => Fin.Succ(unit),
            imported: static (state, f) =>
                from tags in Fin.Succ(InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format), (BimInstrument.CodecSlot, f.Codec)))
                from wall in state.Rows.Write(BimInstrument.ImportDuration.Row, f.Elapsed.TotalSeconds, tags)
                from size in state.Rows.Write(BimInstrument.ImportSize.Row, f.Bytes, tags)
                from placed in state.Rows.Write(BimInstrument.Instancing.Row, f.Instances, tags)
                select unit,
            exported: static (state, f) => state.Rows.Write(BimInstrument.ExportDuration.Row, f.Elapsed.TotalSeconds,
                InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format))),
            lowered: static (state, f) =>
                from tags in Fin.Succ(InstrumentSet.Tags(state.Tenant, (BimInstrument.ProjectorSlot, (object?)f.Projector)))
                from nodes in state.Rows.Write(BimInstrument.ProjectionNodes.Row, f.Nodes, tags)
                from edges in state.Rows.Write(BimInstrument.ProjectionEdges.Row, f.Edges, tags)
                select unit,
            committed: static (state, _) => state.Rows.Write(BimInstrument.ReviewCommits.Row, 1d, InstrumentSet.Tags(state.Tenant)),
            issueMutated: static (state, f) => state.Rows.Write(BimInstrument.ReviewMutations.Row, 1d,
                InstrumentSet.Tags(state.Tenant, (BimInstrument.MutationSlot, (object?)f.Mutation.Key))),
            verdict: static (state, f) => state.Rows.Write(BimInstrument.ReviewVerdicts.Row, 1d,
                InstrumentSet.Tags(state.Tenant, (BimInstrument.TierSlot, (object?)f.Tier), (BimInstrument.OutcomeSlot, f.Outcome))),
            emitted: static (state, f) =>
                from legs in state.Rows.Write(BimInstrument.EnergyExchanges.Row, 1d,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.LegSlot, (object?)f.Leg), (BimInstrument.FormatSlot, f.Format)))
                from warned in state.Rows.Write(BimInstrument.EnergyWarnings.Row, f.Warnings,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format)))
                select unit,
            textured: static (state, f) =>
                from refused in state.Rows.Write(BimInstrument.TextureDrops.Row, f.Dropped,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format), (BimInstrument.ChannelSlot, "target-refused")))
                from missing in state.Rows.Write(BimInstrument.TextureDrops.Row, f.Unresolved,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format), (BimInstrument.ChannelSlot, "image-unresolved")))
                select unit,
            degraded: static (state, f) => state.Rows.Write(BimInstrument.ExchangeDegrades.Row, 1d,
                InstrumentSet.Tags(state.Tenant, (BimInstrument.LaneSlot, (object?)f.Lane), (BimInstrument.ReasonSlot, f.Reason))));

    public static Fin<T> Traced<T>(
        SpanBand? band, BimPoint at, Op op, string model, Func<Fin<T>> body, params ReadOnlySpan<(string Slot, object? Value)> marks) {
        Seq<(string Slot, object? Value)> stamps = (BimInstrument.ModelSlot, (object?)model).Cons(toSeq(marks.ToArray()));
        return band is null
            ? body()
            : at.Plane.Match(
                Some: plane => band.Traced(plane, op, span => (Stamped(span, stamps), body()).Item2),
                None: body);
    }

    static Unit Stamped(Activity? span, Seq<(string Slot, object? Value)> marks) =>
        ignore(marks.Iter(mark => ignore(span?.SetTag(mark.Slot, mark.Value))));
}
```

## [04]-[BENCH_CLAIMS]

- Owner: `BimBenchClaims` the folder claim roster — `static readonly` kernel `BenchClaim` rows per the kernel law that claim rows live BESIDE the lanes they gate on their owning pages; every Rasm.Bim performance claim names its row and a folder-local claim type is the deleted form.
- Cases: claim rows — `ImportGlb`, `ImportIfc`, `ImportDwg`, `ImportPly`, `ImportFbx`, `ImportUsd`, `ImportDotbim` (`Import(format)` over the `Exchange/format#FORMAT_AXIS` row the claim decodes — the GeometryGym row measures `BimIo.ImportIfc` because its carrier is the live `DatabaseIfc`, every other import-capable row `BimIo.ImportGeometry`, and a row without the import capability is a static construction defect), `EgressReauthor` (IFC re-author over an admitted graph), `QueryL`/`QueryXL` (`Query(grade)` over the `Rasm.Element/Graph/corpus#CORPUS_ROSTER` grades `L` and `XL`, the two that page reserves for benchmark hosts while `S` and `M` stay unit and property lanes), `GeoVectorRead` (`Vector(source)` over the `Semantics/vector#VECTOR_SOURCE` row `GeoVector.Read` takes), `GeoRasterRead` (`Raster(format)` — the raster front sniffs bare bytes, so the GeoTIFF format row names the fixture alone), `TessellationRoundTrip` (tessellation-bridge companion round trip) — every row's claim key and `Corpus` binding derive from the roster row it binds and no slug literal survives here: `Slug` spells `corpus-<Key>` off the format or source row, the committed fixture the tests manifest discovers under that exact name, and `Query` spells `forge-<Key>` off the grade, the Element model the bench mints through `CorpusGate.Mint` at setup and fingerprints through `CorpusModel.Snapshot`; the prefix is the realization discriminant, so a graph scale binds a claim without a committed file and a decoded format never mints in memory. Row keys never equal their lane owner's type name, so the `nameof` derivation resolves the type rather than the field beside it.
- Entry: each public field exposes one `BenchClaim`; the private row constructors derive its lane and corpus identities from the owning vocabularies.
- Auto: `CorpusFingerprint` derives through the one kernel content hasher over the corpus artifact bytes, so a claim binds to the exact input it measured and a corpus revision invalidates every dependent claim structurally, never by prose; a corpus-bound claim discharges `BenchLedger.Unproven` only through a proof pair whose fingerprint is present.
- Output: `BenchClaim` rows carry the claim key, vectorized and reference lanes, speedup floor, and corpus slug.
- Packages: LanguageExt.Core, Rasm, BCL inbox.
- Growth: a new measured decode is one `Import(InterchangeFormat.<Row>)` field, a new vector source one `Vector(GeoVectorSource.<Row>)` field, a new graph scale one `Query(CorpusGrade.<Row>)` field — each spelling its claim key and corpus binding off the row, so the tests manifest's missing-fixture row is the whole landing signal; a lane outside those rosters is one `Regression` field over a `Slug` call.
- Boundary: the bench edge resolves the claim's corpus slug, admits the harness sample through `BenchMeasurement.Of`, and passes that measurement directly to `Benchmark.Of` with suite `rasm.bim`, the claim key, and the corpus fingerprint; `Rasm.Bim` exports only the `BenchClaim` rows.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Element.Graph;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
public static class BimBenchClaims {
    private static readonly string Decode = $"{nameof(BimIo)}.{nameof(BimIo.ImportGeometry)}";
    private static readonly string DecodeIfc = $"{nameof(BimIo)}.{nameof(BimIo.ImportIfc)}";
    private static readonly string Reauthor = $"{nameof(BimExport)}.{nameof(BimExport.ExportIfc)}";
    private static readonly string Select = $"{nameof(ElementQuery)}.{nameof(ElementQuery.Query)}";
    private static readonly string VectorRead = $"{nameof(GeoVector)}.{nameof(GeoVector.Read)}";
    private static readonly string RasterRead = $"{nameof(GeoRaster)}.{nameof(GeoRaster.Read)}";
    private static readonly string Tessellate = $"{nameof(TessellationRequest)}.{nameof(TessellationRequest.Plan)}";

    public static readonly BenchClaim ImportGlb = Import(InterchangeFormat.Glb);
    public static readonly BenchClaim ImportIfc = Import(InterchangeFormat.Ifc);
    public static readonly BenchClaim ImportDwg = Import(InterchangeFormat.Dwg);
    public static readonly BenchClaim ImportPly = Import(InterchangeFormat.Ply);
    public static readonly BenchClaim ImportFbx = Import(InterchangeFormat.Fbx);
    public static readonly BenchClaim ImportUsd = Import(InterchangeFormat.Usd);
    public static readonly BenchClaim ImportDotbim = Import(InterchangeFormat.DotBim);
    public static readonly BenchClaim EgressReauthor = Regression("egress-reauthor", Reauthor, Slug(InterchangeFormat.Ifc));
    public static readonly BenchClaim QueryL = Query(CorpusGrade.L);
    public static readonly BenchClaim QueryXL = Query(CorpusGrade.XL);
    public static readonly BenchClaim GeoVectorRead = Vector(GeoVectorSource.GeoPackage);
    public static readonly BenchClaim GeoRasterRead = Raster(InterchangeFormat.GeoTiff);
    public static readonly BenchClaim TessellationRoundTrip = Regression("tessellation-roundtrip", Tessellate, Slug(InterchangeFormat.Ifc));

    private static BenchClaim Regression(string claim, string lane, string corpus) =>
        new(Op.Of(name: claim), lane, lane, 1.0, Some(corpus));

    private static string Slug(InterchangeFormat format) => $"corpus-{format.Key}";

    private static string Slug(GeoVectorSource source) => $"corpus-{source.Key}";

    private static BenchClaim Import(InterchangeFormat format) =>
        format.Capabilities.Admits(InterchangeCapability.Import)
            ? Regression($"import-{format.Key}", format.Codec == InterchangeCodec.GeometryGym ? DecodeIfc : Decode, Slug(format))
            : throw new InvalidOperationException($"<bench-claim:{format.Key}:import-capability-absent>");

    private static BenchClaim Vector(GeoVectorSource source) => Regression($"geo-{source.Key}", VectorRead, Slug(source));

    private static BenchClaim Raster(InterchangeFormat format) => Regression($"geo-{format.Key}", RasterRead, Slug(format));

    private static BenchClaim Query(CorpusGrade grade) => Regression($"query-{grade.Key}", Select, $"forge-{grade.Key}");
}
```

## [05]-[RESEARCH]

(none)
