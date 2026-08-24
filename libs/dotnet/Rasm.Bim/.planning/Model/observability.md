# [BIM_OBSERVABILITY]

Composition-scoped observability for the BIM-and-exchange engine: `BimPoint` closes the `rasm.bim.<domain>.<point>` roster onto the kernel `IHookRoster` floor and `BimHooks.Live` mints the ONE kernel `HookRail` over it, `BimTelemetry` projects typed facts onto `rasm.bim.<domain>.<measure>` instruments as a rail subscriber — domain code fires facts and observability projects them, zero emit calls inside a projector or codec arm — and `BimBenchReceipt` closes the evidence loop: every Rasm.Bim performance claim is a typed, corpus-gated receipt, never a prose number.

Wire posture: HOST-LOCAL, BCL-only. Point capsule, rail, mount custody, fault cell, instrument-spec, advice-bucket, contributor-port, package identity, tenancy frame, numeric fault identity, recovery, and trace-band machinery arrive settled from the kernel, so no OpenTelemetry package is reachable here; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots, which admit the `Rasm.Bim` meter by name and `BimPoint.Scopes` into their one `SpanBand`. Subscriber failure parks point-attributed on the rail's evidence cell, the emitter untouched.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: `BimPoint` closes the point vocabulary on its kernel `Modalities` set and derived `TraceScope` plane, `BimFact` closes the payload family and realizes the kernel `IHookFact` floor through its `Point` projection, `ProgressLane` closes the lanes sharing the progress case, `GlobalIdSet`/`ContentKeySet`/`BimIssueMutation` close the set and mutation vocabularies, `StageMark` carries stage evidence, and `BimHooks.Live` mints the one kernel `HookRail`.
- [03]-[TELEMETRY_TAP]: `BimInstrument` declares the `rasm.bim.<domain>.<measure>` roster as rows CARRYING their kernel `InstrumentSpec` and mints the contributor port; `BimTelemetry` rails the one fact-to-write projection, binds the rail's parked-fault depth, and owns the span and attribution law over the kernel `SpanBand`.
- [04]-[BENCH_RECEIPTS]: `BimBenchClaims` rosters the per-op kernel `BenchClaim` rows, `BimBenchReceipt` carries the run evidence, and the corpus-gate row admits a claim as standing.

## [02]-[HOOK_RAIL]

- Owner: `BimPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.bim.<domain>.<point>` realizing the kernel `IHookRoster<BimPoint>` floor with a `CapabilitySet<HookModality>` column and the kernel `TraceScope` plane derived off the id's own head; `BimFact` the closed payload family every point types over; `ProgressLane` the three-row lane vocabulary the shared progress case discriminates on; `GlobalIdSet`, `ContentKeySet`, and `BimIssueMutation` the set and mutation vocabularies its cases carry; `BimHooks` the composition entry minting the ONE kernel `HookRail<BimPoint, BimFact, TelemetrySource>`. The folder mints zero rail mechanism — seats, veto and observe capsules, scoped taps, detach custody, mount census, and the evidence cell are the kernel's.
- Cases: point roster rows — `rasm.bim.exchange.progress` (`BimFact.Progress` under `ProgressLane.Exchange` — the DWG/DXF decode lane's `StageMark` stream off ACadSharp `ICadReader.OnProgress`), `rasm.bim.exchange.imported` (`BimFact.Imported` — the `ModelLoad` receipt fact post-decode), `rasm.bim.exchange.exported` (`BimFact.Exported` — the export-rail artifact emit carrying the sealed content key), `rasm.bim.projection.lowered` (`BimFact.Lowered` — the seam `GraphDelta` magnitude off the semantic projector), `rasm.bim.projection.legality` (veto beside observe, `BimFact.Admission` — an app policy refuses a graph delta before it lands), `rasm.bim.projection.emit` (veto beside observe, `BimFact.Egress` — the `Projection/egress#IFC_EGRESS` `Emit` fold consults it against the elected format and target schema BEFORE authoring, so a deliverable policy refuses on the exchange coordinates rather than on a delta that already landed), `rasm.bim.review.committed` (`BimFact.Committed` — the `Review/versioning#VERSION_GRAPH` `BimRepository.Seal` funnel, the one point both `Commit` and `CommitMerge` reach), `rasm.bim.review.issue` (`BimFact.IssueMutated` — the `Review/issues#BCF_ARCHIVE` board mutation under its closed `BimIssueMutation` key), `rasm.bim.review.verdict` (replay, `BimFact.Verdict` — IDS-facet and template-audit outcomes, buffered so a late panel drains the recent window), `rasm.bim.energy.progress` (`ProgressLane.Energy` — the energy translate lane's `TranslateStage` rows projecting the mark off OpenStudio `ProgressBar.onPercentageUpdated`), `rasm.bim.planning.progress` (`ProgressLane.Planning` — the `Planning/schedule#SCHEDULE` CPM pass (`CpmStage`) and the `Planning/cost#ESTIMATE` decomposition rollup (`EstimateStage`) over long networks), `rasm.bim.energy.emitted` (`BimFact.Emitted` — the `EnergyReceipt` fact per artifact under its `ArtifactKey` address), `rasm.bim.exchange.textured` (`BimFact.Textured` — the appearance channel census `MaterialFinish.Author` and `AppearanceProjection.TexturesOf` fire once per authored surface style, so the one exchange leg that drops payload by design is counted rather than silent), `rasm.bim.exchange.degrade` (`BimFact.Degraded` — an exchange leg completing while shedding capability, its closed lane and reason keys banding the counter while the identifier-grade subject rides the fact alone).
- Law: `BimFact` is the branch's ONE domain-fact family and every announcement projects from it — `Exchange/events#EVENT_PROJECTION` subscribes the announced points as tap rows, so a second union over this fact space forks the address, the trace, and the vocabulary keys one fire already carries. Cases therefore hold the ADDRESS an announcement subjects on — the commit key, the artifact content key, the `ArtifactKey` text, the topic guid, the specification ordinal — because an address recomputed at a subscriber reads a second identity for the fact its emitter already named.
- Law: the roster and the fact family are ONE correspondence — `BimFact.Point` projects each case onto its owning row through the generated `Map`, so a case with no row, or a row no case reaches, breaks at compile time, and the kernel `IHookFact<BimPoint>` floor's `Seats` derives from that one map rather than a hand-mirrored case list. The three progress points share one case and `ProgressLane` carries the discriminant, so the lane a fire site names IS the point it fires and no site spells both.
- Entry: `BimHooks.Live(key, gates, taps, band, cell)` mints the rail once at composition; an emitting page fires its declared row (`rail.Fire(BimPoint.Imported, fact, key)`), and the guarded arity hands its body the ADMITTED fact so a veto transform governs the seam it brackets. `Points` is the census the app root hands `HookRegistry.Mount`, and `BimPoint.Scopes` enters that root's `SpanBand.Of(version, scopes)`.
- Auto: fire order, veto folding, tap shielding, replay retention, and scoped release are the kernel rail's law — retention first, the veto left-fold second (its first refusal is the emitter's verdict AND parks on the cell), forked shielded taps last, so `Fire` returns without waiting on any tap. `BimIo.ImportGeometry` and `EnergyTranslate.Run` take `Option<BimRail> rail = default` — the optional slot every later fire-site entry repeats — so a rail-less composition pays one `IsNone` test.
- Receipt: a hook fire is the evidence event itself — the emitter's typed receipt already carries the fact, so a point mints nothing; the rail's `FaultCell` is the one evidence surface — veto refusals and shielded tap faults, point-attributed and clock-stamped — bounded by the kernel `Ring`, so a tap storm reads as `Shed` and `Lost` counts rather than as process memory, and `[03]` binds its depth as a pulled level.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, Rasm.Element, BCL inbox.
- Growth: a new point is one `BimPoint` row and one `BimFact` case, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `HookGate`/`HookTap` row at the composition's `Live` call, narrowing through the tap's own `Scope` column rather than through a point probe inside its delegate; delivery semantics are the kernel modality rows; a new native lane keeps its OWN closed stage roster projecting `StageMark` — the carrier is this page's, the rows are the lane's, and a second mark shape or a free-text stage slot is the deleted form.
- Boundary: NAMED LOSS carried from the kernel rail collapse — the COMPILE-TIME shape of the per-point fact type. A `HookPoint<BimFact.*>` column typed each seat to one case, so an `Imported` subscriber never saw an exported fact; under one rail every point shares `BimFact` and subscribers discriminate on the case. What survives is the roster row's modality admission, the union's closure, and — through the kernel `IHookFact<BimPoint>` floor this page realizes — the case narrowing itself as a RUNTIME gate: `Seats` derives from the `Point` correspondence, and `Fire` reads it at entry AND on the veto fold's product, so a fact fired at a foreign seat and a gate rewriting to a sibling case both refuse before any tap runs. Only the compile break is lost; the guarantee is not. WITNESS: `BimHooks.Live` is one expression where the deleted seat record was a per-point constructor column, a per-point mint line, a per-point census entry, and a private `Seat<TFact>` factory.
- Boundary: point ids compose the kernel `HookId` grammar with the package segment pinned `bim`, so a Bim point joins any app-tier registry census unrenamed — Bim declares its points here and the composing app subscribes direct; ids, planes, and modalities derive from the roster row alone, so an inline `HookId.Create` at a fire site does not compile; the payload closes at declaration, so a stringly payload cannot enter the rail; cases carry the CLOSED vocabulary key a sibling owner published (`InterchangeFormat.Key`, `IdsOutcome.Key`, `RuleSeverity.Key`, `ArtifactKey.Value`) rather than that owner's type, so this S0 Model stratum consumes no Exchange, Energy, or Review sibling type and each announcement re-admits the key through its owning gate; telemetry is a tap, never a producer — `[03]-[TELEMETRY_TAP]` subscribes one tap row here and `Exchange/events#EVENT_PROJECTION` subscribes beside it.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
// The kernel rail closed over this folder's roster/fact/owner triple — one alias set, so every signature reads the
// domain name and never the three-parameter spelling.
using BimGate = Rasm.Domain.HookGate<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimObserver = Rasm.Domain.HookTap<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimRail = Rasm.Domain.HookRail<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Point roster keyed rasm.bim.<domain>.<point> — the kernel HookId four-segment grammar, realizing the
// IHookRoster<BimPoint> floor so the ONE kernel rail takes this roster as its type parameter and seats mint from
// Items alone. Modalities is the kernel capability set: the two admission points carry Veto BESIDE Observe, since a
// veto-only set refuses every tap and the instrument projection subscribes unscoped. Each <domain> segment doubles
// as its `Exchange/events#EVENT_PROJECTION` EventType subject, so a point and the announcement projected off it
// join one vocabulary rather than two rosters a rename can separate.
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

    // One materialized index answers the roster floor's Id and Plane reads — the id and its rasm.<pkg>.<domain>
    // head derive from the key ONCE, so a bracket pays a lookup and a scope can never fork from the point it
    // brackets. Accessor-backed: the generator fills Items from its own static constructor.
    static readonly Lazy<FrozenDictionary<BimPoint, (HookId Id, TraceScope Plane)>> Index = new(
        static () => Items.ToFrozenDictionary(static row => row, static row =>
            (HookId.Create(value: row.Key), TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3])))),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Index.Value[this].Id;

    public Option<TraceScope> Plane => Some(Index.Value[this].Plane);

    // Composing roots admit this roster into SpanBand.Of; points share planes per domain head, so the projection
    // deduplicates, and an unadmitted scope refuses on the kernel rail rather than dropping every span silently.
    public static Seq<TraceScope> Scopes => toSeq(Index.Value.Values).Map(static entry => entry.Plane).Distinct().Strict();
}

// The three long-running lanes that share ONE progress case. The lane CARRIES its seat through a deferred
// accessor, so a fire site names the lane alone and the point it fires derives — where a free-text domain string
// let a caller spell "exchange" beside a point that was not the exchange one.
[SmartEnum<string>]
public sealed partial class ProgressLane {
    public static readonly ProgressLane Exchange = new("exchange", static () => BimPoint.ExchangeProgress);
    public static readonly ProgressLane Energy = new("energy", static () => BimPoint.EnergyProgress);
    public static readonly ProgressLane Planning = new("planning", static () => BimPoint.PlanningProgress);

    [UseDelegateFromConstructor]
    public partial BimPoint At();
}

// GlobalIdSet closes the IFC GlobalId SET: the lexical law (22 glyphs over the buildingSMART base64 alphabet
// 0-9, A-Z, a-z, `_`, `$`) AND the set law (sorted, distinct) in ONE owner, so no site re-spells the alphabet
// beside a length check that drifts from it and no site re-spells the ordering probe beside a distinct probe
// that disagrees about the comparer. `Of` NORMALIZES for the fire side — a rail holding a bag of ids gets a
// canonical set — while `Admit` REFUSES for the wire side, because the wire contract IS sorted-distinct and
// silently sorting a producer's malformed array hides a producer defect behind a well-formed announcement.
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

    public static GlobalIdSet Of(Seq<string> ids) => Create(ids);

    // Wire admission proves ordered-distinct rather than imposing it, and the glyph law rides the same
    // construction — through the TRY factory, because the throwing `Create` carries a producer's malformed
    // array out of the typed rail as an exception at the exact boundary the rail exists to answer.
    public static Fin<GlobalIdSet> Admit(ImmutableArray<string> values, Op key) =>
        WireSet.Ordered(values) && TryCreate(toSeq(values), out GlobalIdSet? admitted) && admitted is { } set
            ? Fin.Succ(set)
            : Fin.Fail<GlobalIdSet>(new BimFault.Refused(key, BimScope.Events, BimReason.Codec, string.Join(':', new object?[] { "event-set-malformed", "globalIds" })));

    static bool Admits(string? value) =>
        value is { Length: Glyphs } && value.All(static glyph =>
            glyph is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '$');
}

// ContentKeySet holds that same law over the content-key space: sorted-distinct at construction, with the wire
// form the fixed-width 32-hex rendering `Rasm/Domain/identity#CONTENT_KEY` `ContentHash` owns — hex-text ordering
// agrees with the numeric ordering only because that rendering is fixed-width.
[ValueObject<Seq<UInt128>>]
public sealed partial class ContentKeySet {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<UInt128> value) =>
        value = toSeq(value.Distinct().OrderBy(static k => k));

    public static ContentKeySet Of(Seq<UInt128> keys) => Create(keys);
}

// Both admissions share ONE wire-side probe: distinct under the ordinal comparer AND already in ordinal order,
// read on the ARRAY the producer sent — a set the producer sorted differently fails here rather than
// re-sorting quietly into a shape the sender never emitted.
static class WireSet {
    public static bool Ordered(ImmutableArray<string> values) =>
        values.Distinct(StringComparer.Ordinal).Count() == values.Length
        && values.SequenceEqual(values.OrderBy(static value => value, StringComparer.Ordinal), StringComparer.Ordinal);
}

// Issue mutation keys live on one generated owner, so neither a fire site nor an announcement admission can
// invent a sixth mutation.
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

// --- [MODELS] -----------------------------------------------------------------------------
// Every native lane's stage roster projects ONE stage-evidence carrier: Done the published fraction of the
// whole lane at the mark, Witness the lane-owned stage token. Rosters stay PLURAL per lane (the kernel
// ArrangeStage is internal to Rasm.Meshing, so a cross-package roster owner would invert strata) — a discrete
// ladder projects its declared rows, a continuous native callback mints marks with a live Done under one
// witness. Each mark REQUIRES its fraction, so a free-text stage with no fraction is unrepresentable.
public readonly record struct StageMark(double Done, string Witness);

// One closed payload family every hook point types over: one Op-keyed case per fact shape, so a point's fact is
// a case and the tap reads typed evidence. Format, codec, leg, tier, outcome, and artifact slots carry each
// CLOSED vocabulary KEY the firing page projects down — so the S0 Model stratum consumes no Exchange, Energy, or
// Review sibling type, and tag cardinality stays bounded because every key originates in a closed vocabulary at
// the fire site. Each case also carries the ADDRESS its announcement subjects on, so the events projection
// re-derives no identity its emitter already named.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFact : IHookFact<BimPoint> {
    private BimFact(Op key) => Key = key;

    public Op Key { get; }

    // The kernel floor's gate, DERIVED from the map below rather than a hand-mirrored per-point case list: every
    // case seats at exactly one row (the shared progress case at whichever lane row its own ProgressLane carries),
    // so `Fire` refuses an emitter pairing a fact with a foreign point, and refuses again on a veto's product.
    public bool Seats(BimPoint at) => Point.Equals(at);

    // The roster correspondence: the generated Map is total, so a case minted without a row does not compile.
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
    // Egress carries the admission the Emit fold consults BEFORE authoring: format and target schema are the
    // two facts an app policy refuses on, and Nodes is the scope magnitude a deliverable gate bounds — this
    // fact is PRE-artifact, disjoint from Exported, which reports the artifact a passed emit produced.
    public sealed record Egress(Op Key, string Format, string Schema, int Nodes) : BimFact(Key);
    public sealed record Committed(Op Key, UInt128 CommitKey, ContentKeySet Parents, string Branch, int Elements) : BimFact(Key);
    public sealed record IssueMutated(Op Key, string Topic, BimIssueMutation Mutation, Option<string> Comment, GlobalIdSet GlobalIds) : BimFact(Key);
    // Specification and Spec ride together because IDS v1.0 spec names are NOT unique: the ordinal
    // disambiguates two same-named specifications, and a
    // name-keyed verdict silently merges their findings into one.
    public sealed record Verdict(
        Op Key, string Specification, int Spec, ContentAddress Model, string Tier, string Outcome, string Severity,
        int Findings, GlobalIdSet GlobalIds) : BimFact(Key);
    public sealed record Emitted(Op Key, string Artifact, string Leg, string Format, int Warnings) : BimFact(Key);
    // Texture binding is the one exchange leg that drops payload BY DESIGN. The three counts are disjoint by
    // construction — Bound is what reached the artifact, Dropped is what a target refused, Unresolved is what
    // never resolved to bytes — so the sum is the authored channel census and a missing texture is attributable
    // to its cause rather than merely absent.
    public sealed record Textured(Op Key, string Format, int Bound, int Dropped, int Unresolved) : BimFact(Key);
    // Degraded names a leg that COMPLETED while shedding capability — a codec falling back, a feature the target
    // format cannot carry, a substituted approximation. Lane and Reason are closed vocabulary keys the firing leg
    // projects down; Subject names the identifier-grade element the degradation landed on and rides the fact for
    // a reader, never a metric dimension.
    public sealed record Degraded(Op Key, string Lane, string Reason, string Subject) : BimFact(Key);
}

// --- [COMPOSITION] --------------------------------------------------------------------------
// The composition entry over the KERNEL rail: the folder keeps its roster and fact union and mints ZERO rail
// mechanism. Live's one domain move is the band lowering onto the rail's IHookSpan floor plus the re-rail — a
// kernel composition refusal (a gate on an observe-only point, a mid-mount attach failure already rolled back)
// reads its origin from the Bim band like every other seam entry rather than as a bare kernel fault.
public static class BimHooks {
    public static Fin<BimRail> Live(
        Op key, Seq<BimGate> gates = default, Seq<BimObserver> taps = default,
        Option<SpanBand> band = default, Option<FaultCell> cell = default) =>
        BimRail.Of(key, gates, taps, band.Map(static span => (IHookSpan)span), cell)
            ;
}
```

## [03]-[TELEMETRY_TAP]

- Owner: `BimInstrument` the closed `rasm.bim.*` roster — a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) so `Rows` derives from `Items` and construction proves each row's name against its key — beside the one dotted slot block both the metric rows and the `[02]` span marks spell, with the contributor-port mint under the kernel `TelemetrySource.Bim` scope; `BimTelemetry` the fact-to-write projection over the `InstrumentSet` the composing root materializes. Receipts stay billing truth; instruments are the lossy dashboard channel projected from them.
- Cases: projection map — `rasm.bim.exchange.import.duration`/`import.size`/`instancing` off `BimFact.Imported` (duration, payload size, and instance placements sharing one evidence read), `rasm.bim.exchange.export.duration` off `BimFact.Exported`, `rasm.bim.projection.nodes`/`edges` off `BimFact.Lowered`, `rasm.bim.review.commits` off `BimFact.Committed`, `rasm.bim.review.mutations` off `BimFact.IssueMutated` banded by the closed mutation key, `rasm.bim.review.verdicts` off `BimFact.Verdict`, `rasm.bim.energy.exchanges`/`warnings` off `BimFact.Emitted`, `rasm.bim.exchange.texture.drops` off `BimFact.Textured` banded by format and drop cause (the two loss causes write, the bound count does not — it is the artifact's own evidence), `rasm.bim.exchange.degrades` off `BimFact.Degraded` banded by lane and reason, and the pulled `rasm.bim.observe.tap.faults` bound per point off the rail's own evidence cell.
- Entry: `BimInstrument.Telemetry(version)` is the contributor port the composing root materializes — the semconv coordinate is the kernel pin so all three signals bump together — and a root outside that fan binds `InstrumentSet.Of(cells, (meter, BimInstrument.Rows))` directly against its own minted meter; either path, never both. `BimTelemetry.Tap(set)` returns the ONE tap row passed to `BimHooks.Live`; `BimTelemetry.Depth(set, rail, key)` registers the rail's parked-fault read per point and returns the scopes that retire them, so the root arms it AFTER the mint the tap fed. `BimTelemetry.Traced<T>(band, at, op, model, body, marks)` is the span wrapper every long-running entry composes over the composing root's kernel band, model identity a required argument so no Bim span exists unattributed and the band nullable so a composition admitting no scope runs the identical rail untraced.
- Auto: every advised row ships its explicit-bucket bounds through the kernel's `InstrumentKind` x `MeasureForm` derivation, so this page names a bound row and never a create call; instrument identity de-duplicates by name inside the meter, and the write plane addresses by ROW, so the two-map-one-key pair a string name forced has no spelling here; tag values ride the typed fact's own vocabulary keys, never free text.
- Law: a subscriber failure is captured by the kernel shield and never fires a fact, so the refusal series has NO pushed write site — `Depth` binds the cell's own parked read per point instead, one registration per roster row under its own `PointSlot` tag. Fault identity is evidence, not a string tag: `FaultCell.Parked` holds `IsolatedFault(Point, Cause, At)`, so a health panel reads the generated numeric code and locally derived recovery from the preserved `Cause` while the point axis stays a live series, and `Shed`/`Lost` report what the ring dropped.
- Receipt: none — the tap projects receipts and hook facts; a metric minted beside it is a second truth. Five points project NO series by design: the three `Progress` streams are operator feedback whose live fraction is span-and-panel material, not a bounded series, and the two veto points reach the cell through their parked refusals — so an un-projected point is a stated posture, never an oversight. The projection's typed refusal rides straight out of the tap arm, so the capsule shield parks an unmounted row or a form mismatch as tap-attributed evidence rather than dropping a measurement silently.
- Packages: LanguageExt.Core, NodaTime, Rasm (the kernel instrument mechanism, scope identity roster, numeric fault identity and recovery, tenancy frame, and trace band), BCL inbox.
- Growth: a new projected fact is one `BimInstrument` row with its `Dimensions` and one arm on the total `Project` fold, which the generated `Switch` breaks at compile time; every row declares the kernel tenant slot beside its fact dimensions, while a new span dimension is one slot row here and one `marks` pair at the composing entry.
- Boundary: library altitude holds zero OpenTelemetry reference and zero span custody — the meter reaches the process only through the composing root's mint, so provider disposal owns instrument lifetime, and the kernel `SpanBand` owns the one `ActivitySource` per admitted scope, its listener gate, its `using` close, and its typed fail-leg status, so this page declares `BimPoint.Scopes` and holds no source, no wrapper, and no disposable; instrument custody is one-per-composition — either the app fan materializes the `Telemetry` port or a root binds `InstrumentSet.Of` locally, never both; subscription law — the tap mounts at `Live` ahead of the first fire, because the rail fans a `Replay` point's held window to each fresh subscriber and a late attach therefore re-counts that window onto the verdict counter; span law — the span name IS the kernel `Op` and the plane the point's own id head, so bracket and fact never name two scopes, and the typed verdict, not a tag, carries the error fact; attribution law — dimension slots carry this package's dotted `rasm.bim.<dimension>` namespace so a concept a sibling package also tags never collides, fault identity stays on the preserved Error as generated numeric code plus locally derived recovery rather than a string owner/category tag, tenancy is the kernel `TenantContext` projection every metric write folds so this page holds no tenant key and no baggage read, and model identity is identifier-grade: it rides the span alone as `Traced`'s own required argument, because one metric series per model is unbounded cardinality no view cap recovers and a slot left to caller discipline is a slot no caller stamps; the span fold never re-stamps the tenant partition the app root's baggage promotion already carries; SDK composition, exporters, exemplars, views, and cardinality caps stay at the app roots.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Diagnostics;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;
using BimObserver = Rasm.Domain.HookTap<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using BimRail = Rasm.Domain.HookRail<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Bim.Model;

// --- [TABLES] -------------------------------------------------------------------------------
// Closed roster on the kernel KernelInstrument form: each row CARRIES its InstrumentSpec and Rows derives from
// Items, so the const-name roster and a hand-listed sequence mirroring it are one declaration, the write plane
// addresses by ROW, and construction proves the row's name against its key. Kind and MeasureForm are the spec's
// own columns, so the kernel derives every create body and this page spells none.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BimInstrument {
    // Model identity stays SPAN-only: identifier-grade cardinality is free on a sampler-thinned span and
    // unbounded on a series. Traced stamps it from its own required argument, so this key has one write site.
    public const string ModelSlot = "rasm.bim.model";

    public const string CodecSlot = "rasm.bim.codec";
    public const string FormatSlot = "rasm.bim.format";
    public const string LegSlot = "rasm.bim.energy.leg";
    public const string OutcomeSlot = "rasm.bim.review.outcome";
    public const string PointSlot = "rasm.bim.point";
    public const string ProjectorSlot = "rasm.bim.projector";
    public const string TierSlot = "rasm.bim.review.tier";
    // Board mutation is a five-row closed key, so it bands a counter safely; the topic guid it arrives beside
    // stays identifier-grade and rides the fact alone for the same reason the model identity does.
    public const string MutationSlot = "rasm.bim.review.mutation";
    // Drop cause is the whole point of that counter: a refused target and an unresolvable image are different
    // exchange defects, so one counter banded by cause replaces two counters that would drift.
    public const string ChannelSlot = "rasm.bim.exchange.texture.cause";
    // Degradation bands on two BOUNDED axes. Its Subject stays off the series for the ModelSlot reason.
    public const string LaneSlot = "rasm.bim.exchange.lane";
    public const string ReasonSlot = "rasm.bim.exchange.reason";

    public static readonly BimInstrument ImportDuration = new(
        "rasm.bim.exchange.import.duration",
        InstrumentSpec.Create("rasm.bim.exchange.import.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "foreign-bytes decode wall duration per format and codec",
            Seq(TenantContext.TenantSlot, FormatSlot, CodecSlot), Some(Buckets.DecodeSeconds), None, None));

    // Size, never bytes: the estate name grammar carries no unit suffix and the UCUM By unit states the measure.
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
            "seam delta node magnitude per projector",
            Seq(TenantContext.TenantSlot, ProjectorSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly BimInstrument ProjectionEdges = new(
        "rasm.bim.projection.edges",
        InstrumentSpec.Create("rasm.bim.projection.edges", InstrumentKind.Distribution, MeasureForm.Whole, "{edge}",
            "seam delta edge magnitude per projector",
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

    // The rail's parked-fault depth — the ONE pulled row here, bound per point because a shielded refusal fires
    // no fact and therefore has no pushed write site at all.
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

// --- [OPERATIONS] -------------------------------------------------------------------------
// Fact-to-write projection over the composition's InstrumentSet — no minted state, so provider disposal owns
// instrument lifetime and this owner holds nothing to dispose.
public static class BimTelemetry {
    // ONE hook-rail subscription passed into BimHooks.Live beside the app's own taps. Unscoped by construction:
    // Project owns a TOTAL Switch, so it wants every point and a scope row here would restate that totality.
    public static BimObserver Tap(InstrumentSet set) => new(Op.Of(name: "rasm.bim.instruments"), fact => Project(set, fact));

    // Registration is a SET per row on the kernel cell store, so one probe per point publishes its own depth
    // under its own tag and the roster's banding survives with no write site. Tenant resolves ONCE at bind, which
    // is correct where the per-fact read is not: a rail is composition-scoped, not request-scoped.
    public static Fin<Seq<IDisposable>> Depth(InstrumentSet set, BimRail rail, Op key) =>
        toSeq(BimPoint.Items).Traverse(row => set.Bind(
            BimInstrument.TapFaults.Row,
            () => (double)rail.Faults.Parked.Count(fault => fault.Point.Equals(row.Id)),
            key,
            InstrumentSet.Tags(TenantContext.Current, (BimInstrument.PointSlot, (object?)row.Key)))).As();

    // Total generated dispatch — a new BimFact case breaks this projection at compile time, so an unprojected
    // fact is a build error. Project resolves the ambient partition ONCE per fact and threads it as state:
    // TenantContext.Current is the kernel's AsyncLocal slot, so a per-write read lets two writes of ONE fact land
    // under two partitions when a flow re-enters mid-projection. An arm whose writes share a tag set binds it
    // once at the head, so the shared partition is folded per fact, never per write.
    static Fin<Unit> Project(InstrumentSet set, BimFact fact) =>
        fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
            state: (set, TenantContext.Current),
            // The three long-run streams and the two admission points are span-and-cell material by declared
            // posture: a live fraction is not a bounded series and a refusal parks rather than pushes.
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
            // Only the LOSSES write. A bound channel is the artifact's own evidence; the two loss causes write
            // under one instrument banded by cause, so the drop total and its attribution are one series.
            textured: static (state, f) =>
                from refused in state.Rows.Write(BimInstrument.TextureDrops.Row, f.Dropped,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format), (BimInstrument.ChannelSlot, "target-refused")))
                from missing in state.Rows.Write(BimInstrument.TextureDrops.Row, f.Unresolved,
                    InstrumentSet.Tags(state.Tenant, (BimInstrument.FormatSlot, (object?)f.Format), (BimInstrument.ChannelSlot, "image-unresolved")))
                select unit,
            degraded: static (state, f) => state.Rows.Write(BimInstrument.ExchangeDegrades.Row, 1d,
                InstrumentSet.Tags(state.Tenant, (BimInstrument.LaneSlot, (object?)f.Lane), (BimInstrument.ReasonSlot, f.Reason))));

    // Span wrapper every long-running Bim entry composes over the composing root's kernel band, which owns the
    // source, the listener gate, the ActivityKind.Internal open, and the fail-leg status verdict — this page
    // adds attribution alone. The band arrives NULLABLE for the same reason the rail slot arrives optional: a
    // headless or plugin composition admits no scope, and a null receiver runs the identical rail untraced where
    // a required band would force that composition to mint an ActivitySource its root never disposes. Model
    // identity is a REQUIRED parameter rather than a caller-chosen mark row: every Bim entry runs against exactly
    // one model, and a slot published for a caller to remember is a slot a caller forgets. Further marks stay the
    // caller's identifier-grade rows; each stamps post-start, so no mark reaches the sampling verdict.
    // Exemption: a params span cannot cross a lambda, so materializing it is the one statement seam here.
    public static Fin<T> Traced<T>(
        SpanBand? band, BimPoint at, Op op, string model, Func<Fin<T>> body, params ReadOnlySpan<(string Slot, object? Value)> marks) {
        Seq<(string Slot, object? Value)> stamps = (BimInstrument.ModelSlot, (object?)model).Cons(toSeq(marks.ToArray()));
        return band is null
            ? body()
            : at.Plane.Match(
                Some: plane => band.Traced(plane, op, span => (Stamped(span, stamps), body()).Item2),
                None: body);
    }

    // Span attribution takes marks alone: the app root's baggage promotion already stamps rasm.tenant on every
    // span, so folding the metric plane's Tagged here would double-stamp the partition.
    static Unit Stamped(Activity? span, Seq<(string Slot, object? Value)> marks) =>
        ignore(marks.Iter(mark => ignore(span?.SetTag(mark.Slot, mark.Value))));
}
```

## [04]-[BENCH_RECEIPTS]

- Owner: `BimBenchClaims` the folder claim roster — `static readonly` kernel `BenchClaim` rows per the kernel law that claim rows live BESIDE the lanes they gate on their owning pages; every Rasm.Bim performance claim names its row and a folder-local claim type is the deleted form. `BimBenchReceipt` carries the typed run evidence a bench run mints per claim.
- Cases: claim rows — `ImportGlb`, `ImportIfc`, `ImportDwg`, `ImportPly`, `ImportScene`, `ImportUsd`, `ImportDotbim` (foreign-bytes decode per `BimIo` codec arm), `EgressReauthor` (IFC re-author over an admitted graph), `QueryMedium`/`QueryLarge` (element-set predicate folds at the two corpus graph scales), `GeoVectorRead`/`GeoRasterRead` (geospatial-seam ingest), `TessellationRoundTrip` (tessellation-bridge companion round trip) — each row carrying its `Corpus` slug, the estate corpus artifact whose content fingerprint the receipt stamps. Row keys never equal their lane owner's type name, so the `nameof` derivation resolves the type rather than the field beside it.
- Entry: the bench project constructs `BimBenchReceipt` rows at its edge — one per claim per run — and the corpus-gate admission row below is the ONE path a receipt becomes a standing claim.
- Auto: `CorpusFingerprint` derives through the one kernel content hasher over the corpus artifact bytes, so a claim binds to the exact input it measured and a corpus revision invalidates every dependent claim structurally, never by prose; a corpus-bound claim discharges `BenchLedger.Unproven` only through a proof pair whose fingerprint is present.
- Receipt: `BimBenchReceipt` — claim, corpus fingerprint, median / p95 / interquartile wall duration, allocated bytes, operation count, instant; the spans fold through the app-tier `BenchMeasurement.Of` into one exact distribution, so a receipt lands whole rather than defaulting a spread the gate reads. Distribution truth, no verdict field — judging is the gate fold's, not the receipt's.
- Packages: LanguageExt.Core, NodaTime, Rasm, BCL inbox.
- Growth: a new measured operation is one `BenchClaim` row on `BimBenchClaims`; a new measured axis is one field on the receipt breaking the gate mapping at compile time.
- Boundary: corpus-gate admission — a speed or allocation claim on any Rasm.Bim page resolves to a `BimBenchReceipt` the estate BenchmarkDotNet corpus gate stamped: the branch bench project folds each receipt into the app-tier benchmark envelope (suite `rasm.bim`, case the claim key) and the AppHost `BenchmarkGate.Judge` fold owns pass-or-regress under the host-evidence and budget law; BenchmarkDotNet binds in the branch test and benchmark projects per the Test Stack manifest tier, never `Rasm.Bim.csproj`, so no benchmark type crosses into this package; a hand-rolled kernel is admitted only after its receipt defeats the library route under that gate.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// Folder claim roster: kernel BenchClaim rows, one per measured operation, per the kernel law that claim rows
// live beside the lanes they gate. Every row is a corpus-REGRESSION claim — the measured lane is judged against
// its own prior stamped receipt on the same corpus — so Regression folds the kernel row's two lane columns onto
// ONE spelling at the no-regression 1.0 floor, and the spelling is nameof-DERIVED at the measured member so a
// rename breaks this roster at compile time where a literal strands the gate against a lane it can no longer
// bind. The Corpus column DECLARES the fixture roster — the tests-estate benchmark corpus manifest realizes each
// slug as its CorpusEntry.RelativePath row under the corpus BENCHMARK_CLAIM contract, and the receipt stamps the
// MEASURED CorpusEntry.Key at run — so no fingerprint pins here, a divergent realization fails the corpus-gate
// admission rather than this page, and the declaration is the authority the manifest transcribes.
public static class BimBenchClaims {
    private static readonly string Decode = $"{nameof(BimIo)}.{nameof(BimIo.ImportGeometry)}";
    private static readonly string DecodeIfc = $"{nameof(BimIo)}.{nameof(BimIo.ImportIfc)}";
    private static readonly string Reauthor = $"{nameof(BimExport)}.{nameof(BimExport.ExportIfc)}";
    private static readonly string Select = $"{nameof(ElementQuery)}.{nameof(ElementQuery.Query)}";
    private static readonly string Vector = $"{nameof(GeoVector)}.{nameof(GeoVector.Read)}";
    private static readonly string Raster = $"{nameof(GeoRaster)}.{nameof(GeoRaster.Read)}";
    private static readonly string Tessellate = $"{nameof(TessellationRequest)}.{nameof(TessellationRequest.Plan)}";

    public static readonly BenchClaim ImportGlb = Regression("import-glb", Decode, "corpus-scene-glb");
    public static readonly BenchClaim ImportIfc = Regression("import-ifc", DecodeIfc, "corpus-model-ifc");
    public static readonly BenchClaim ImportDwg = Regression("import-dwg", Decode, "corpus-drawing-dwg");
    public static readonly BenchClaim ImportPly = Regression("import-ply", Decode, "corpus-mesh-ply");
    public static readonly BenchClaim ImportScene = Regression("import-scene", Decode, "corpus-scene-fbx");
    public static readonly BenchClaim ImportUsd = Regression("import-usd", Decode, "corpus-stage-usd");
    public static readonly BenchClaim ImportDotbim = Regression("import-dotbim", Decode, "corpus-model-bim");
    public static readonly BenchClaim EgressReauthor = Regression("egress-reauthor", Reauthor, "corpus-model-ifc");
    public static readonly BenchClaim QueryMedium = Regression("query-medium", Select, "corpus-graph-100k");
    public static readonly BenchClaim QueryLarge = Regression("query-large", Select, "corpus-graph-1m");
    public static readonly BenchClaim GeoVectorRead = Regression("geo-vector", Vector, "corpus-geo-gpkg");
    public static readonly BenchClaim GeoRasterRead = Regression("geo-raster", Raster, "corpus-geo-cog");
    public static readonly BenchClaim TessellationRoundTrip = Regression("tessellation-roundtrip", Tessellate, "corpus-model-ifc");

    private static BenchClaim Regression(string claim, string lane, string corpus) =>
        new(Op.Of(name: claim), lane, lane, 1.0, Some(corpus));
}

// --- [MODELS] -----------------------------------------------------------------------------
// Distribution truth per claim per run: the verdict lives on the app-tier gate fold, never here; the corpus
// fingerprint binds the claim to the exact measured input through the one kernel content hasher and is the
// presence witness the kernel BenchLedger.Unproven proof pair reads.
public sealed record BimBenchReceipt(
    BenchClaim Claim,
    UInt128 CorpusFingerprint,
    Duration Median,
    Duration P95,
    Duration Iqr,
    long AllocatedBytes,
    long Operations,
    Instant At);
```

## [05]-[RESEARCH]

(none)
