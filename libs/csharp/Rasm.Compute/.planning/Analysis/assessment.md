# [COMPUTE_ASSESSMENT]

Rasm.Compute assessment algebra: the C#-first discipline-analysis vocabulary that reads the concrete `Rasm.Element` `ElementGraph` directly — above the seam, no `IElementProjection`, Compute being app-platform consuming the AEC-domain seam upward. One polymorphic `AssessmentRequest` routes over the seam `Discipline` to a discipline runner that folds its discipline-specific input into ONE uniform `AssessmentResult` fact stream; `AnalysisReads` is the one baked-bag, edge-attribute, and analytical-footprint read owner every runner composes. The `Analysis/dispatch` sibling owns what happens to that stream — the content key, the lifecycle-aware cache dispatch, the write-back, the supersede close-out, and the sweep — so this page owns the ALGEBRA and dispatch owns the SPINE; `Analysis.Commission` stays here because the commissioning gate ladder is a fold over this page's own fact and verdict vocabulary rather than a dispatch modality.

Seam vocabulary arrives settled from `Rasm.Element` — the `Discipline`, the typed `PropertyValue` family, `AssessmentPayload` with its ONE `Open` admission and ONE `Land` outcome landing, `PayloadContent`, `EvidenceRun`, `AnalysisRoute.Of`, `BlobKey`, `GraphDelta`, the `Assign` edge, `CanonicalWriter`, and `ContentAddress`; Compute decodes and writes, never re-mints them. Absence is `Option` on every result column — a governing ratio a runner could not compute is `None`, not a `0.0` the verdict band reads as a clean pass.

## [01]-[INDEX]

- [02]-[ROUTE_AXIS]: `AssessmentRoute` rows the standard-code axis and `AssessmentVerdict` bands the governing ratio over one `Option`-shaped projection.
- [03]-[REQUEST_FAMILY]: `AssessmentRequest` cases carry discipline input and fold the content key; `AssessmentResult` carries every runner's facts, its optional ratio, and its gated `EvidenceRun` as one stream.
- [04]-[ANALYSIS_READS]: `AnalysisReads` is the ONE baked-bag, edge-attribute, and analytical-footprint read owner every discipline runner composes.
- [05]-[COMMISSIONING]: `Analysis.Commission` gates the metered-against-predicted comparison, folds the band-propagating residual, and lands the verdict as an `Assessment` node under a derived commissioning `AnalysisRoute`.

## [02]-[ROUTE_AXIS]

- Owner: `AssessmentRoute` `[SmartEnum<string>]` the standard-code axis, each row carrying the seam `Discipline` it serves, the human `Standard` citation, and the machine `SolverVersion` revision token; `AssessmentVerdict` `[SmartEnum<string>]` the ratio-banded outcome with a `Critical` column and the ONE `FromRatio` projection over an optional ratio.
- Cases: structural/thermal/acoustic/fire/energy/environmental/cost/seismic/circulation/daylight routes, each a row carrying its `Discipline`, citation, and `SolverVersion` (the seismic rows over the condensed modal pencil, the circulation/daylight rows the once-runnerless seam rows now served) — never a parallel per-discipline enum; `AssessmentVerdict` rows `satisfied`/`marginal`/`exceeded`/`not-applicable`.
- Entry: the route is a value the `AssessmentRequest` case carries and the content-key folds; `AssessmentVerdict.FromRatio(Option<double>)` bands a governing utilization/criticality ratio (`>1.0` exceeded, `≥0.95` marginal, finite-below satisfied, ABSENT or non-finite not-applicable), so a verdict derives from the ratio, never a stored flag that drifts and never a caller-supplied band two mints disagree on.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`Option`/`Seq`/`Fin`), Rasm.Element (project — `Discipline`), the `Analysis/capacity` `DesignCode` (the bijection counterpart `Probe` proves), the `Runtime/admission#DISPATCH_SPINE` `ComputeFault`/`AssessmentInputReason`, BCL inbox.
- Growth: a new design code is one `AssessmentRoute` row carrying its `Discipline`, citation, and `SolverVersion`, with its `DesignCode` counterpart where the row serves `Discipline.Structural` (`Probe` names the miss at composition); a closed-form or solver revision is one bumped `SolverVersion` on the existing row; a new discipline is one seam `Discipline` row with its routes, the dispatch `Switch` breaking until the runner arm exists; zero new surface.
- Boundary: the route `Discipline` is the seam vocabulary, never re-declared — a Compute-local discipline enum is the deleted form; the structural route↔`DesignCode` join spans two non-referencing `SmartEnum` owners on a shared `Key`, a correspondence no type system holds, so `Probe` proves the bijection at composition and a rename faults there rather than unrouting silently until a solve rails; the route `Key` AND `SolverVersion` are load-bearing content-key components folded by `Analysis/dispatch#DISPATCH_WRITEBACK` `ContentKey`, never free strings. The `SolverVersion` is the ONE version axis on this rail — the machine revision token distinct from the human `Standard` citation, realizing the seam's "the `AnalysisRoute` token OR the `InputKey` MUST fold the solver tool+version" obligation for every route, so a closed-form edition or an EnergyPlus/EC3 solver change re-keys to a fresh node rather than false-hitting a prior `Computed` result; every downstream expected-version pin DERIVES from this one, so a second fold of a derived spelling keys one fact twice and lets the two halves desynchronize. `AssessmentVerdict` derives from the governing ratio at projection so the receipt verdict and the fact stream cannot disagree, and its absent arm is the honest answer for a route that computed no ratio at all.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessmentRoute {
    public static readonly AssessmentRoute Aisc360   = new("aisc360",     Discipline.Structural,    "AISC 360-22",            solver: "aisc360-22");
    public static readonly AssessmentRoute En1993    = new("en1993",      Discipline.Structural,    "EN 1993-1-1:2005",       solver: "en1993-1-1:2005");
    public static readonly AssessmentRoute En1994    = new("en1994",      Discipline.Structural,    "EN 1994-1-1:2004",       solver: "en1994-1-1:2004");
    public static readonly AssessmentRoute En1992    = new("en1992",      Discipline.Structural,    "EN 1992-1-1:2004",       solver: "en1992-1-1:2004");
    public static readonly AssessmentRoute Nds       = new("nds",         Discipline.Structural,    "NDS 2018",               solver: "nds-2018");
    public static readonly AssessmentRoute En1995    = new("en1995",      Discipline.Structural,    "EN 1995-1-1:2004",       solver: "en1995-1-1:2004");
    public static readonly AssessmentRoute Aci318    = new("aci318",      Discipline.Structural,    "ACI 318-19",             solver: "aci318-19");
    public static readonly AssessmentRoute Tms402    = new("tms402",      Discipline.Structural,    "TMS 402-22",             solver: "tms402-22");
    public static readonly AssessmentRoute En1996    = new("en1996",      Discipline.Structural,    "EN 1996-1-1:2005",       solver: "en1996-1-1:2005");
    public static readonly AssessmentRoute AisiS100  = new("aisi-s100",   Discipline.Structural,    "AISI S100-16",           solver: "aisi-s100-16");
    public static readonly AssessmentRoute Sdpws     = new("sdpws",       Discipline.Structural,    "AWC SDPWS-2021",         solver: "sdpws-2021");
    public static readonly AssessmentRoute Iso6946   = new("iso6946",     Discipline.Thermal,       "ISO 6946:2017",          solver: "iso6946:2017");
    public static readonly AssessmentRoute En13788   = new("en13788",     Discipline.Thermal,       "EN ISO 13788:2012",      solver: "en-iso-13788:2012");
    public static readonly AssessmentRoute Iso12354  = new("iso12354",    Discipline.Acoustic,      "ISO 12354-1:2017",       solver: "iso12354-1:2017");
    public static readonly AssessmentRoute Iso3382   = new("iso3382",     Discipline.Acoustic,      "ISO 3382-1 / EN 12354-6", solver: "en12354-6:2003");
    public static readonly AssessmentRoute En1993Fire = new("en1993-1-2", Discipline.Fire,          "EN 1993-1-2:2005",       solver: "en1993-1-2:2005");
    public static readonly AssessmentRoute En1992Fire = new("en1992-1-2", Discipline.Fire,          "EN 1992-1-2:2004",       solver: "en1992-1-2:2004");
    public static readonly AssessmentRoute EnergyPlus = new("energyplus", Discipline.Energy,        "EnergyPlus 25.2 / ISO 52016", solver: "energyplus-25.2.0");
    public static readonly AssessmentRoute En15978   = new("en15978",     Discipline.Environmental, "EN 15978:2011",          solver: "en15978:2011+ec3");
    public static readonly AssessmentRoute CostInPlace = new("cost-in-place", Discipline.Cost,      "in-place unit cost",     solver: "cost-in-place-1");
    public static readonly AssessmentRoute En1998   = new("en1998",      Discipline.Seismic,       "EN 1998-1:2004",         solver: "en1998-1:2004");
    public static readonly AssessmentRoute Asce7    = new("asce7",       Discipline.Seismic,       "ASCE 7-22",              solver: "asce7-22");
    public static readonly AssessmentRoute IbcEgress = new("ibc-egress", Discipline.Circulation,   "IBC 2024 Ch.10",         solver: "ibc-2024-ch10");
    public static readonly AssessmentRoute EnEgress = new("en-egress",   Discipline.Circulation,   "EN egress / national annexes", solver: "en-egress-1");
    public static readonly AssessmentRoute En17037  = new("en17037",     Discipline.Daylight,      "EN 17037:2018",          solver: "en17037:2018");

    public Discipline Discipline { get; }
    public string Standard { get; }
    // Machine solver/standard-revision token, separate from the human Standard citation and the ONE version axis the
    // content key folds: a closed-form edition bump, an EnergyPlus solver bump, or an EC3 method change increments
    // THIS token so a re-assessment re-keys rather than false-hitting a prior version's Computed result. Every
    // downstream expected-version pin (the energy toolchain gate) DERIVES from this row; a second fold of the derived
    // spelling keys one fact twice and lets a policy re-spelling desynchronize two halves of one key.
    public string SolverVersion { get; }

    // Route-to-code bijection probe, the ReceiptSurface.Probe precedent applied to a second cross-registry
    // correspondence no type system holds: every Structural row MUST resolve an Analysis/capacity DesignCode row,
    // because the static design check joins two non-referencing SmartEnum owners on their shared Key. A rename on
    // either side otherwise unroutes silently — the join rails at solve time on a route the corpus believed served —
    // so the composition root runs this and faults where the drift is, not where it surfaces. Seismic rows are
    // excluded BY CONSTRUCTION: their route names the seismic ACTION standard while the capacity code rides
    // SeismicSpec, so no en1998/asce7 capacity row exists for the join to find.
    public static Fin<Unit> Probe() {
        Seq<AssessmentRoute> unrouted = toSeq(Items)
            .Filter(static route => route.Discipline == Discipline.Structural && DesignCode.For(route).IsFail);
        return unrouted.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing(
                AssessmentInputReason.RouteUnrouted,
                string.Join(',', unrouted.Map(static route => route.Key))));
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AssessmentVerdict {
    public static readonly AssessmentVerdict Satisfied     = new("satisfied",      critical: false);
    public static readonly AssessmentVerdict Marginal      = new("marginal",       critical: false);
    public static readonly AssessmentVerdict Exceeded      = new("exceeded",       critical: true);
    public static readonly AssessmentVerdict NotApplicable = new("not-applicable", critical: false);

    public bool Critical { get; }

    // MarginBand 0.95 is STATED LAW, not a knob: every mint reads this one projection, no policy owner carries a
    // band, and a parameter no call site ever passes lets two verdicts over one ratio disagree by argument.
    const double MarginBand = 0.95;

    // ONE projection over an OPTIONAL ratio: a route that computed no ratio and a route whose ratio is non-finite
    // both band not-applicable, and the absent case can no longer reach the ladder as a 0.0 reading Satisfied.
    public static AssessmentVerdict FromRatio(Option<double> ratio) =>
        ratio.Filter(double.IsFinite).Match(
            Some: static value => value > 1.0 ? Exceeded : value >= MarginBand ? Marginal : Satisfied,
            None: () => NotApplicable);
}
```

## [03]-[REQUEST_FAMILY]

- Owner: `AssessmentRequest` `[Union]` the discipline-input axis — one case per discipline carrying its target `NodeId` set, its `AssessmentRoute`, and the discipline policy; the shared `Targets`/`Route` reads are abstract overrides each case satisfies positionally (the seam `Node.Id` idiom), `Discipline` derived from the route, and `CanonicalBytes` contributes the discipline input to the content key; `AssessmentResult` the one uniform outcome carrier (its `Discipline`/`Verdict`/`At` all derived, an optional `GoverningRatio`, an optional `BlobKey` keying the heavy artifact, and the gated seam `EvidenceRun` audit); `AssessmentFact` the typed neutral `(PropertyName, PropertyValue)` fact, its factory family total over the seam `PropertyValue` cases so any discipline emits a scalar, a demand/capacity interval, a per-band list, or a classified rating without hand-building a `PropertyValue`.
- Cases: one case per discipline carrying its specific input — `Structural`, `Seismic` (the `SeismicSpec` response-spectrum payload), `Thermal`, `Acoustic`, `Fire`, `Energy`, `Carbon`, `Cost`, `Circulation`, `Daylight`; the RESULT is the uniform `AssessmentResult` fact stream every runner returns, so a `StructuralResult`/`ThermalResult` parallel family is the rejected form collapsed onto one fact stream with `(PropertyName, PropertyValue)` slot/kind metadata.
- Entry: a runner consumes one `AssessmentRequest` case and returns `Fin<AssessmentResult>`; `AssessmentResult.Of(route, facts, governingRatio, at, key, elapsed, correlation, attempt, resultBlob)` is the ONE mint — it derives the `Verdict` from the optional ratio and mints the seam `EvidenceRun` ONCE from its pieces, so no runner constructs the audit record positionally and no runner spells the author/tool/version triple; `resultBlob` defaults `None` (a closed-form route stores no artifact) and a subprocess/solver route stores its EnergyPlus SQLite or FEA result set through the `Analysis/dispatch#DISPATCH_WRITEBACK` `AssessmentSink` egress port the dispatch threads.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `NodeId`, `PropertyName`, `PropertyValue`, `Interpolation`, `MeasureValue`, `Dimension`, `BlobKey`, `EvidenceRun`/`EvidenceRun.Of`, `Discipline`, `CanonicalWriter`), Rasm (kernel — `Op` the audit-mint key), NodaTime, BCL inbox.
- Growth: a new discipline is one `AssessmentRequest` case with one dispatch arm — the generated `Switch` breaks until it exists; a new fact on any discipline is one `AssessmentFact` row in the runner's fold (the factory family already total over the seam `PropertyValue` cases), never a new result type or a structured value flattened to a string; zero new surface.
- Boundary: arity discriminates on the case payload shape, never a name suffix or mode flag; `Targets` is a seam `NodeId` set so a runner reads only the reachable subgraph and never invents identities; the discipline policy (combinations, climate, weather, query) is the case payload, never an ambient global; `AssessmentFact.Value` is the seam `PropertyValue` union (a `Measure` carries the SI scalar and unit) so a fact is typed and unit-bearing, never a bare double; a utilization/criticality ratio is a dimensionless `Measure`, NEVER a `Bounded` (the seam `Bounded` is the lower/upper/setpoint interval, not a scalar). `GoverningRatio` is `Option<double>` because a route with no acceptance target computed no ratio at all — the structural `0.0` a `double` column forced there banded `Satisfied` and published a pass no check ever ran, and that false pass is what the option forecloses; `Discipline`/`Verdict`/`At` are derived (a stored `At` beside the audit's own instant is the deleted duplicate); the heavy artifact is referenced by the optional seam `BlobKey`, never a raw `UInt128` re-stating the seed invariant and never an inlined payload. `EvidenceRun` is the seam's GATED audit mint (the retired `Provenance` name), so `Of` returns `Fin<AssessmentResult>` and the eight discipline runners change SIGNATURE rather than body — one mint site for the author/tool/version triple instead of eight positional constructions that drift. The uniform `AssessmentResult` IS the discipline-specific result — specificity lives in the FACTS, not parallel carriers.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Typed neutral fact pairs PropertyName with PropertyValue for write-back into the seam Results bag. The factory
// family covers the seam PropertyValue union (Measure/Ratio/Text/Flag/Reference/Bounded/Enumerated/List/Table) —
// never a hand-built PropertyValue nor a structured result flattened to a string. Factories over an admitted value
// are TOTAL; raw-scalar mints are Fin through the seam MeasureValue.OfSi finite gate. A ratio is a dimensionless
// Measure, never a Bounded interval.
public readonly record struct AssessmentFact(PropertyName Name, PropertyValue Value) {
    public static AssessmentFact Measure(string name, MeasureValue value)     => new(PropertyName.Create(name), new PropertyValue.Measure(value));
    // Railed raw-scalar mints reject NaN/∞ through the seam OfSi finite gate before a fact exists, so every discipline
    // threads `Fin` at its own fold; `Rows` lifts independent mints through `Validation` and reports every rejected scalar.
    public static Fin<AssessmentFact> Measure(string name, Dimension dimension, double si) => MeasureValue.OfSi(dimension, si).Map(value => Measure(name, value));
    public static Fin<AssessmentFact> Ratio(string name, double value)        => Measure(name, Dimension.Dimensionless, value);
    public static Fin<Seq<AssessmentFact>> Rows(params ReadOnlySpan<Fin<AssessmentFact>> facts) => Seq(facts).Traverse(static fact => fact.ToValidation()).As().ToFin();
    public static AssessmentFact Text(string name, string value)             => new(PropertyName.Create(name), new PropertyValue.Text(value));
    public static AssessmentFact Flag(string name, bool value)               => new(PropertyName.Create(name), new PropertyValue.Boolean(value));
    public static AssessmentFact Reference(string name, NodeId target)        => new(PropertyName.Create(name), new PropertyValue.Reference(target));
    public static AssessmentFact Bounded(string name, Option<MeasureValue> lower, Option<MeasureValue> upper, Option<MeasureValue> setpoint) => new(PropertyName.Create(name), new PropertyValue.Bounded(lower, upper, setpoint));
    // Seam Enumerated carries typed PropertyValue members on both slots — chosen and allowed sets lift through
    // PropertyValue.Text here, never a Seq<string> flattening the write-back verdict entry would then disagree with.
    public static AssessmentFact Enumerated(string name, string chosen, Seq<string> allowed) => new(PropertyName.Create(name), new PropertyValue.Enumerated(Seq<PropertyValue>(new PropertyValue.Text(chosen)), allowed.Map(static a => (PropertyValue)new PropertyValue.Text(a))));
    public static AssessmentFact List(string name, Seq<PropertyValue> values) => new(PropertyName.Create(name), new PropertyValue.List(values));
    public static AssessmentFact Table(string name, Seq<(PropertyValue Defining, PropertyValue Defined)> rows) => new(PropertyName.Create(name), new PropertyValue.Table(rows, Interpolation.NotDefined));
}

// Discipline/Verdict/At are DERIVED, never stored: Discipline from Route, Verdict from the optional governing ratio
// at mint, At from the EvidenceRun audit. GoverningRatio is OPTIONAL because a request carrying no acceptance target
// (an informational carbon rollup, a budget-less cost takeoff) computed no ratio — the `0.0` a double column forced
// there banded Satisfied and published a pass no check ever ran.
public sealed record AssessmentResult(
    AssessmentRoute Route,
    Seq<AssessmentFact> Facts,
    AssessmentVerdict Verdict,
    Option<double> GoverningRatio,
    Option<BlobKey> ResultBlob,
    EvidenceRun Provenance) {
    public Discipline Discipline => Route.Discipline;
    public Instant At => Provenance.At;

    // THE one mint. The seam EvidenceRun.Of is GATED (blank author/tool/version, negative elapsed, negative attempt
    // all refuse), so the audit record is minted here ONCE from its pieces and every discipline runner hands the
    // pieces instead of constructing the record — one site spelling the "rasm.compute" author and the route's own
    // tool/version columns, rather than eight positional constructions a rename drifts apart.
    public static Fin<AssessmentResult> Of(
        AssessmentRoute route, Seq<AssessmentFact> facts, Option<double> governingRatio, Instant at, Op key,
        Duration elapsed = default, Option<CorrelationId> correlation = default, int attempt = 0,
        Option<BlobKey> resultBlob = default) =>
        EvidenceRun.Of("rasm.compute", route.Key, route.SolverVersion, at, key, elapsed, correlation: correlation, attempt: attempt)
            .Map(run => new AssessmentResult(route, facts, AssessmentVerdict.FromRatio(governingRatio), governingRatio, resultBlob, run));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssessmentRequest {
    private AssessmentRequest() { }

    // Every case carries Targets and the AssessmentRoute as its first two positionals, so the shared reads are the union's own
    // abstract overrides (the seam Node.Id idiom), never a per-case Switch. Discipline derives from the route.
    public abstract Seq<NodeId> Targets { get; }
    public abstract AssessmentRoute Route { get; }

    public sealed record Structural(Seq<NodeId> Targets, AssessmentRoute Route, Seq<LoadCombinationSpec> Combinations, StructuralPolicy Policy, Option<SiteActionPolicy> Site = default) : AssessmentRequest;
    // Seismic is its own case, not an Option on Structural: the route DISCIPLINE differs (Seismic, not Structural), so
    // one case carrying both would make AdmitRoute answer two disciplines from one arm and leave the seismic routes
    // structurally unreachable. The route names the seismic ACTION standard (EN 1998 / ASCE 7) while the member checks
    // run under a MATERIAL code, so the capacity DesignCode rides SeismicSpec rather than joining off the route.
    public sealed record Seismic(Seq<NodeId> Targets, AssessmentRoute Route, SeismicSpec Spec, StructuralPolicy Policy, Option<SiteActionPolicy> Site = default) : AssessmentRequest;
    public sealed record Thermal(Seq<NodeId> Targets, AssessmentRoute Route, BoundaryClimate Climate) : AssessmentRequest;
    public sealed record Acoustic(Seq<NodeId> Targets, AssessmentRoute Route, double RequiredRw, Option<double> TargetReverberationS = default) : AssessmentRequest;
    public sealed record Fire(Seq<NodeId> Targets, AssessmentRoute Route, FireExposure Exposure, double RequiredMinutes, double Utilization) : AssessmentRequest;
    public sealed record Energy(Seq<NodeId> Targets, AssessmentRoute Route, WeatherRef Weather, EnergyPolicy Policy) : AssessmentRequest;
    public sealed record Carbon(Seq<NodeId> Targets, AssessmentRoute Route, CarbonQuery Query) : AssessmentRequest;
    // Budgets are the cost arm's ACCEPTANCE axis: BudgetTotal caps the target set's in-place cost, BudgetPerArea rates
    // against its takeoff area. Both are decimal because money is exact; both are optional because an informational
    // rollup is a real request, and their joint absence is what an ABSENT governing ratio then honestly reports.
    public sealed record Cost(Seq<NodeId> Targets, AssessmentRoute Route, string Currency, Option<decimal> BudgetTotal = default, Option<decimal> BudgetPerArea = default) : AssessmentRequest;
    public sealed record Circulation(Seq<NodeId> Targets, AssessmentRoute Route, EgressPolicy Policy, Map<NodeId, OccupancyClass> Occupancies) : AssessmentRequest;
    public sealed record Daylight(Seq<NodeId> Targets, AssessmentRoute Route, Option<WeatherSource> Weather, double RequiredSunHours, Seq<LocalDate> DesignDays, ObstructionScene Scene, DaylightPolicy Policy, Option<SolarSite> Site = default) : AssessmentRequest;

    public Discipline Discipline => Route.Discipline;

    // Case-to-route invariant: the union case and Route.Discipline are ONE classification — the dispatch proves the
    // correspondence BEFORE any content-key derivation, so runner dispatch (the case Switch), persisted discipline,
    // and receipt identity (Route.Discipline) can never split-brain an assessment.
    public Fin<Unit> AdmitRoute() {
        Discipline expected = Switch(
            structural: static _ => Discipline.Structural, seismic: static _ => Discipline.Seismic,
            thermal: static _ => Discipline.Thermal,
            acoustic: static _ => Discipline.Acoustic, fire: static _ => Discipline.Fire,
            energy: static _ => Discipline.Energy, carbon: static _ => Discipline.Carbon,
            cost: static _ => Discipline.Cost, circulation: static _ => Discipline.Circulation,
            daylight: static _ => Discipline.Daylight);
        return expected == Route.Discipline
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<assessment-route-discipline:{expected.Key}!={Route.Discipline.Key}>"));
    }

    // Discipline-specific policy folds into the content key so a changed load combination, spectrum row, excitation
    // axis, capacity code, climate, fire requirement, weather file, execution route, setpoint/internal-load, LCIA
    // method + per-material OMF, or currency re-keys rather than returning a stale hit. Every variable-length sequence
    // is COUNT-PREFIXED and sorted by key, so the projection is self-delimiting and order-stable, and every optional
    // rides the writer's own `Optional` member — which writes the presence tag BEFORE the value it delimits, so no arm
    // can invert the framing order the way a hand `IfSome` beside a separate `Bool` tag could.
    // Exemption: the per-row writer emission is the canonical byte-boundary statement seam.
    public void CanonicalBytes(CanonicalWriter w) => Switch(
        structural: r => {
            Frame(w, r.Policy, r.Site);
            w.Ordinal(r.Combinations.Count);
            foreach (LoadCombinationSpec combo in r.Combinations.OrderBy(static c => c.Label, StringComparer.Ordinal)) {
                w.String(combo.Label).Ordinal(combo.Factors.Count);
                foreach ((StructuralCase kase, double factor) in combo.Factors.OrderBy(static f => f.Key.Key, StringComparer.Ordinal)) { w.String(kase.Key).Double(factor); }
            }
            return w;
        },
        // Every seismic column is result-determining: the spectrum ROW and the ground-motion policy set the demand, the
        // excitation axis selects which participation factors scale it, the combination row decides how the modal
        // responses add, the floor decides admission, and the CAPACITY code decides every member check — so all six
        // fold. The ground-type S/TB/TC/TD are the spectrum row's own code table addressed by Key + SiteClass, so
        // folding those two addresses them without re-writing the table on every key.
        seismic: r => {
            Frame(w, r.Policy, r.Site);
            return w.String(r.Spec.Spectrum.Key).String(r.Spec.Direction.Key).String(r.Spec.Combination.Key)
                .String(r.Spec.Capacity.Key).Double(r.Spec.ParticipationFloor)
                .String(r.Spec.Policy.SiteClass).Double(r.Spec.Policy.Pga).Double(r.Spec.Policy.Behavior).Double(r.Spec.Policy.DampingRatio)
                .Double(r.Spec.Policy.Sds).Double(r.Spec.Policy.Sd1).Double(r.Spec.Policy.T1).Double(r.Spec.Policy.TLong);
        },
        thermal:  r => w.Double(r.Climate.InteriorTempC).Double(r.Climate.InteriorRh).Double(r.Climate.ExteriorTempC).Double(r.Climate.ExteriorRh).Double(r.Climate.TargetUValueWM2K),
        acoustic: r => w.Double(r.RequiredRw).Optional(r.TargetReverberationS, static (target, k) => k.Double(target)),
        fire:     r => w.String(r.Exposure.Key).Double(r.RequiredMinutes).Double(r.Utilization),
        // The energy EXECUTION ROUTE re-keys (a local in-process build and a cloud recipe are different derivations,
        // so the provider discriminant + cloud owner/project/platform/job-descriptor fold — a cloud result never
        // false-hits a local one; the descriptor folds VERBATIM, content-keyed input refs only, no local
        // path/URL/timestamp/auth token that would over-key and silently re-run a metered job); NEVER the discovery
        // paths, which are provisioning rather than identity. The toolchain's expected version does NOT fold: it
        // DERIVES from the route SolverVersion this key already carries, and folding a derived spelling beside its
        // own source keys one fact twice.
        energy:   r => r.Policy.Route.Switch(
                        subprocess: _ => w.String("local"),
                        cloud:      c => w.String("cloud").String(c.Owner).String(c.Project).String(c.Platform).String(c.JobDescriptor))
                        .String(r.Weather.EpwPath).String(r.Weather.Station)
                        .Optional(r.Policy.TargetEui, static (m, k) => k.Double(m)).Double(r.Policy.HeatingSetpointC).Double(r.Policy.CoolingSetpointC).Double(r.Policy.LightingPowerWM2).Double(r.Policy.EquipmentPowerWM2),
        carbon:   r => {
            w.String(r.Query.Omf).String(r.Query.Method.Key).Optional(r.Query.TargetKgCo2e, static (m, k) => k.Double(m)).Double(r.Query.ReferencePeriodYears).Ordinal(r.Query.OmfByMaterial.Count);
            foreach ((string material, string omf) in r.Query.OmfByMaterial.OrderBy(static p => p.Key, StringComparer.Ordinal)) { w.String(material).String(omf); }
            return w;
        },
        // Budgets are result-determining (they band the verdict), so each folds through Optional with its exact decimal
        // rendering — an invariant-culture string, never a double, because the binary widening re-rounds the very figure
        // the key is meant to distinguish.
        cost:     r => w.String(r.Currency)
                        .Optional(r.BudgetTotal, static (total, k) => k.String(total.ToString(CultureInfo.InvariantCulture)))
                        .Optional(r.BudgetPerArea, static (rate, k) => k.String(rate.ToString(CultureInfo.InvariantCulture))),
        circulation: r => {
            w.Double(r.Policy.AllowableTravelM).Double(r.Policy.AllowableDeadEndM).Double(r.Policy.AllowableCommonPathM)
                .Double(r.Policy.MinimumClearWidthM).Double(r.Policy.CapacityPerMetreWidth)
                .Double(r.Policy.UnimpededSpeedMPerS).Double(r.Policy.SpecificFlowPersonsPerMS).Optional(r.Policy.AllowableRsetMinutes, static (m, k) => k.Double(m))
                .Ordinal(r.Occupancies.Count);
            foreach ((NodeId space, OccupancyClass occupancy) in r.Occupancies.OrderBy(static p => p.Key.Value, StringComparer.Ordinal)) { w.String(space.Value).String(occupancy.Key); }
            return w;
        },
        daylight: r => {
            // The EPW row folds its file identity; the GRIDDED row folds the corpus CONTENT KEY beside its declared
            // cell and year span, NEVER the path — one shared corpus serves many sites, so a path-keyed fold collides
            // re-published corpora and misses a cell move. The obstruction scene folds by ITS key because a new
            // neighbour re-shades the target, and an explicit geometry-only site folds its four scalars for the same
            // reason. Every DaylightPolicy column is result-determining: the sun step and sample count set the sun-hour
            // quantum, the hemisphere fan the sky-view resolution, and the occlusion cadence how many hours the
            // circumsolar term was actually measured at — a re-cadenced sweep reading the old cached answer reports a
            // fidelity it never ran.
            w.Optional(r.Weather, static (source, k) => source.Switch(
                    epw: row => k.String(row.Weather.EpwPath).String(row.Weather.Station),
                    gridded: row => k.String($"{row.CorpusKey:x32}").Ordinal(row.LatIndex).Ordinal(row.LonIndex).Ordinal(row.Years)))
                .Optional(r.Site, static (site, k) => k.Double(site.LatitudeDeg).Double(site.LongitudeDeg).Double(site.TimezoneHours).Double(site.ElevationM))
                .Ordinal(r.Policy.SunSamplesPerDay).Double(r.Policy.SunStepHours).Ordinal(r.Policy.HemisphereAzimuths)
                .Ordinal(r.Policy.HemisphereAltitudes).Ordinal(r.Policy.OcclusionCadenceHours)
                .Double(r.RequiredSunHours).String($"{r.Scene.Key:x32}").Ordinal(r.DesignDays.Count);
            foreach (LocalDate day in r.DesignDays) { w.String(LocalDatePattern.Iso.Format(day)); }
            return w;
        });

    // Frame idealization columns BOTH structural cases share: the formulation/serviceability/sampling policy and the
    // site-derived actions, so a changed wind speed, snow load, live-load category, tributary strip, or roof band
    // re-keys the static and the seismic assessment through ONE projection rather than two that drift apart.
    static void Frame(CanonicalWriter w, StructuralPolicy policy, Option<SiteActionPolicy> site) =>
        w.String(policy.Formulation.Key).Double(policy.DeflectionLimitRatio).Ordinal(policy.StationCount)
            .Double(policy.StirrupSpacing).Double(policy.CotTheta)
            .Optional(site, static (row, k) => k.Double(row.BasicWindSpeedMPerS).String(row.Exposure.Key).Double(row.Kzt).Double(row.Kd).Double(row.GcpNet)
                .Double(row.GroundSnowPa).Double(row.Ce).Double(row.Ct).Double(row.SnowImportance).Double(row.RoofSlopeFactor)
                .String(row.LiveLoad.Key).Double(row.TributaryWidthM).Double(row.RoofBandM));
}
```

## [04]-[ANALYSIS_READS]

- Owner: `AnalysisReads` the ONE Analysis-rail read owner every discipline runner composes — three `extension` blocks over the three carriers a runner reads through: `ElementGraph` (baked quantity and property bags), `Relationship.Generic` (projected edge attributes), and `FootprintPolygon` (the analytical planar boundary).
- Entry: `graph.Bags<T>(subject, set)` resolves the carrier bags an object's `Assign.PropertyDefinition` edges reach; `Quantity`/`Magnitude`/`Property`/`Scalar` read one row or an ORDERED CHAIN whose first hit wins; `edge.Attribute`/`Magnitude`/`Si`/`Flag`/`Text` read a `Generic` edge's attribute map; `footprint.Planar()` projects the seam analytical boundary onto the NTS `Polygon` every planar runner measures over.
- Packages: LanguageExt.Core (`Option`/`Seq`), Rasm.Element (project — `ElementGraph`, `Node`/`Node.PropertySet`/`Node.QuantitySet`, `NodeId`, `Relationship.Assign`/`AssignKind`/`Relationship.Generic`, `PropertyName`, `PropertyValue`, `MeasureValue`, `FootprintPolygon`, `Vector3`), NetTopologySuite (`GeometryFactory.CreateLinearRing`/`CreatePolygon(LinearRing, LinearRing[])`, `Coordinate`, `Polygon`), BCL inbox.
- Growth: a new bag-read modality is one member on the `ElementGraph` block every runner inherits; a new edge-attribute case one member on the edge block; a new analytical-shape projection one member on its own carrier block — never a per-page copy.
- Boundary: the baked-bag, edge-attribute, and analytical-footprint reads are `AnalysisReads`' alone — a per-page `Quantity`/`Property`/`Named`/`Si`/`Polygon` copy is the forked form four runners maintained in parallel, and every row those reads key is a `Rasm.Element`-declared static so the projector and the non-referencing runner share one spelling. Each read is a SHAPE, not a discipline, and the optional set filter scopes a read to one named bag where the discipline needs it and scans every bound bag otherwise. `Planar` carries the footprint's INTERIOR RINGS: a `FootprintPolygon` declares a shell plus its hole run, and a shell-only projection reports a courtyard, a shaft, or an atrium void as occupiable floor — the area, the egress catchment, and the daylight target all inflate by exactly the holes the projection dropped. Presence is preserved on every read: an absent attribute reads `None` and a present `0.0` reads `Some`, so a truthiness collapse that turns a real start-joint station into a defaulted midspan cannot form.

```csharp signature
// --- [BOUNDARIES] --------------------------------------------------------------------------
// THE Analysis-rail read owner every discipline runner composes. A baked quantity, a baked property, a projected
// edge attribute, and an analytical footprint are four wires with one access shape — resolve the carrier, key it by
// an owner-declared row, discriminate the case — so ONE polymorphic surface serves them and the per-page copies
// (a physics Quantity/Property pair, an energy set-scoped pair, a lifecycle Named, a structural Si/Opt pair, a
// circulation ring-to-polygon fold) collapse here. Every row a caller keys is a Rasm.Element-declared static — a
// call-site PropertyName.Create forks the key space between the non-referencing projector and reader on the first rename.
public static class AnalysisReads {
    extension(ElementGraph graph) {
        // Bag nodes follow an object's Assign.PropertyDefinition edges the Bim DefinesProperties fold projected.
        public Seq<T> Bags<T>(NodeId subject, Option<string> set = default) where T : Node =>
            toSeq(graph.EdgesAt(subject))
                .Choose(e => e is Relationship.Assign { SubKind: AssignKind.PropertyDefinition } a && a.Subject == subject
                    ? graph.Find<T>(a.Definition) : None)
                .Filter(node => set.Match(Some: name => node is Node.PropertySet p ? p.Bag.SetName == name
                    : node is Node.QuantitySet q && q.Bag.SetName == name, None: static () => true));

        public Option<MeasureValue> Quantity(NodeId subject, PropertyName row, Option<string> set = default) =>
            graph.Bags<Node.QuantitySet>(subject, set).Choose(q => q.Bag.Values.Find(row)).Head;

        public Option<double> Magnitude(NodeId subject, PropertyName row, Option<string> set = default) =>
            graph.Quantity(subject, row, set).Map(static m => m.Si);

        // Ordered chain: the first row the graph carries wins, so a net-over-gross takeoff preference is stated ONCE
        // on the Element declarer and every discipline reads the same order.
        public Option<double> Magnitude(NodeId subject, Seq<PropertyName> chain, Option<string> set = default) =>
            chain.Choose(row => graph.Magnitude(subject, row, set)).Head;

        public Option<PropertyValue> Property(NodeId subject, PropertyName row, Option<string> set = default) =>
            graph.Bags<Node.PropertySet>(subject, set).Choose(p => p.Bag.Values.Find(row)).Head;

        // Scalar lifts a Measure-cased property row to its SI magnitude; a present non-Measure row reads absent,
        // because a thermal-bridge Ψ stamped as text is a projector defect, never a number to coerce.
        public Option<double> Scalar(NodeId subject, PropertyName row, Option<string> set = default) =>
            graph.Property(subject, row, set).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None);

        // THE assessment-row projection every reconciliation read shares. The supersede close-out, the stale-marking
        // fold, the staleness closure, and the drifted-row seed each asked the same question of the whole node map
        // and each spelled its own `node is Node.Assessment` scan — three of them inside one sweep, two inside a
        // fixpoint that re-walked every node per recursion level. One projection, one place a future node-map index
        // lands, and the case test stops being a per-caller idiom.
        public Seq<Node.Assessment> Assessments() =>
            toSeq(graph.Nodes.Values).Choose(static node => node is Node.Assessment row ? Some(row) : None);
    }

    extension(Relationship.Generic edge) {
        public Option<PropertyValue> Attribute(PropertyName row) => edge.Attributes.Find(row);

        // Presence-preserving: an absent attribute reads None and a present 0.0 reads Some, so a truthiness collapse
        // that turns a real start-joint station into a defaulted midspan cannot form.
        public Option<double> Magnitude(PropertyName row) =>
            edge.Attribute(row).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None);

        // Si is the ZERO-DEFAULTING read a component vector wants, where an unstamped ordinate genuinely contributes
        // nothing; a read that must distinguish absence takes Magnitude instead.
        public double Si(PropertyName row) => edge.Magnitude(row).IfNone(0.0);

        public bool Flag(PropertyName row) =>
            edge.Attribute(row).Map(static v => v is PropertyValue.Boolean b && b.Value).IfNone(false);

        public Option<string> Text(PropertyName row) =>
            edge.Attribute(row).Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None);
    }

    extension(FootprintPolygon footprint) {
        // THE planar projection every area, egress-catchment, and daylight-target read shares — and it carries the
        // INTERIOR RINGS. A shell-only CreatePolygon reports a courtyard, a shaft, or an atrium void as occupiable
        // floor, so the area, the occupant load derived from it, and the sky-view fraction over it all inflate by
        // exactly the holes the projection dropped. NTS closes nothing implicitly, so each ring closes explicitly.
        public NetTopologySuite.Geometries.Polygon Planar() =>
            Ground.CreatePolygon(Ring(footprint.Ring), [.. footprint.Holes.Map(Ring)]);
    }

    // ONE factory under the default floating precision model and SRID 0: the analytical footprints are model-space
    // rings the seam already quantized to Header.Tolerance, so a second precision model here would re-round a
    // coordinate the content key was minted from.
    static readonly NetTopologySuite.Geometries.GeometryFactory Ground = new();

    static NetTopologySuite.Geometries.LinearRing Ring(Seq<Vector3> ring) =>
        Ground.CreateLinearRing([.. ring.Map(static p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)),
            new NetTopologySuite.Geometries.Coordinate(ring[0].X, ring[0].Y)]);
}
```

## [05]-[COMMISSIONING]

- Owner: `CommissioningPolicy` the `[ComplexValueObject]` acceptance policy (consumable-coverage floor, residual tolerance fraction) admitted at CONSTRUCTION through `ValidateFactoryArguments`, the same admission form the sibling `Analysis/circulation` `EgressPolicy` holds; `CommissioningAsk` the one commissioning request the sweep carries beside its assessment requests; `Commissioned` the one-pass outcome carrier (the `GraphDelta`, the `ComputeReceipt.Assessment`, the three typed magnitudes, the coverage the verdict was drawn over, the banded `AssessmentVerdict`); `Analysis.Commission` the commissioning rail entry, extending the `Analysis/dispatch#DISPATCH_WRITEBACK` owner through its own partial so the content key, node-id mint, supersede close-out, and receipt case stay one owner's.
- Law: the commissioning read is the `Rasm.Element` `AssessmentPayload.ResultMeasure` MEMBER — the dimensioned flat read off a receipt's typed `Results` bag. `Rasm.Bim`'s `Energy/results#RESULTS_ADMISSION` `ResultMeasure` is a `[SmartEnum]` naming an energy quantity axis — a same-spelled, unrelated concept in a different namespace, and no line on this page reaches it.
- Entry: `Commission(graph, element, ask, correlation, clock)` returns `Fin<Commissioned>` — the baked `Element` carries both evidence kinds flat so the comparison reads two values off ONE `Bake`, and the graph rides beside it for the header tolerance, the predecessor rows, and the supersede close-out. ONE `PropertyName` selects both sides: the series' observed `Aspect` and the assessment's `ResultMeasure` name are the same key, so a comparison can never pair a metered aspect against an unrelated result entry. `Analysis/dispatch#SWEEP` `Sweep` folds every `SweepContext.Commissionings` ask through this entry in the same reconciliation pass whose staleness closure flips the verdicts, so the third rail entry is reachable from the same driver the other two ride and a commissioning verdict is re-derived exactly when either upstream drifts.
- Auto: the gates run in ORDER and each is a typed refusal carrying its `AssessmentInputReason` row — policy admission is gone (the `[ComplexValueObject]` cannot construct invalid), window boundedness (NodaTime `Interval.Start`/`End` THROW on an unbounded side, so this crosses before any endpoint read), the consumable prediction, the covering series, the quantity-triple agreement, the completeness floor, then the representative figure and the residual. Verdicts band onto the ONE `AssessmentVerdict.FromRatio` axis rather than a parallel commissioning vocabulary: the ratio is the residual's worst-case banded magnitude over the policy tolerance of the predicted magnitude, so `>1.0` is `exceeded`, `≥0.95` `marginal`, finite-below `satisfied`, and an ABSENT ratio — the honest answer where a prediction carries no scale to band against — `not-applicable`.
- Receipt: the verdict rides the ONE `ComputeReceipt.Assessment` case under the derived commissioning route with the deviation ratio on `GoverningRatio` and the measured `Elapsed`, so a commissioning run reads on the same telemetry rail every assessment does and the `Runtime/receipts` `AssessmentWire` mirror is unperturbed; the three magnitudes and the coverage ride the payload `Results` bag AND the `Commissioned` carrier off one computation, never a second fold.
- Packages: Thinktecture.Runtime.Extensions (`[ComplexValueObject]` + `ValidateFactoryArguments`), LanguageExt.Core (`Seq`/`Option`/`Fin` and the `Fin` query-expression sequencing the gate ladder rides), NodaTime (`Instant`/`Duration`/`Interval`), Rasm.Element (project — `Element` with its flat `Observations`/`Assessments`, `ObservationSeries`/`SeriesStatistics`/`SamplingKind`, `Node.Observation`, `AssessmentPayload`/`.ResultMeasure`/`.Open`, `PayloadContent.Results`, `EvidenceRun`, `OutcomeCapability`, `AnalysisRoute.Of`, `MeasureValue`/`MeasureBand`/`Dimension`, `GraphDelta`/`Relationship.Assign`/`AssignKind`, `ContentAddress.Of<TState>`, `NodeId.Of`/`NodeSeed.Content`), Rasm (kernel — the `Op` op-key), BCL inbox.
- Growth: a new acceptance stance is one `CommissioningPolicy` value; a new commissioned discipline is FREE — the route derives from the assessed row, so no `AssessmentRoute` row, no `Discipline` edit, and no `Probe` change follows; a new commissioning fact is one entry in the `Land` bag; a verdict-rule or residual-rule revision is one bumped `CommissioningRevision`; never a parallel commissioning verdict vocabulary, never a per-discipline commissioning entry.
- Boundary: the commissioning `AnalysisRoute` DERIVES from `(assessed route, element id, aspect)` and occupies no `AssessmentRoute` row — commissioning is not a discipline, so a single row has no `Discipline` column to carry and a row-per-discipline family forks every future route in two; folding the element and the aspect into the token is what makes the seam's one-usable-node law hold PER MEASURED STREAM, since `Supersede` keys on `(discipline, route)` and a route shared across aspects or elements flips a neighbour's verdict `Superseded`. Comparisons never COERCE: a quantity-triple disagreement is a binding defect, never a unit conversion away from meaning something, and a predicted measure whose `CanonicalUnit` the registry cannot name refuses too, because agreement the seam cannot state is not agreement. Completeness screening refuses an UNANSWERABLE denominator (an event-driven stream carries no cadence, an unbounded window no duration) rather than reading a silent sensor as full coverage. Commissioning refusals stay RAIL-ONLY and never mint a `Failed` node: the fold is closed-form and deterministic over evidence a binding repair or a fresh flush changes, so a cached failure outlives the repair that fixes it — the failure-caching path is the dispatch's, for a foreign solver whose failure is expensive to reproduce. Measured-evidence node ids are a PURE FUNCTION of the series, the same projection `Runtime/observation#OBSERVATION_LANE` mints them from, so the `DependsOn` entry derives with no graph lookup and the two legs cannot disagree about which node the verdict depends on; recording the predicted node beside it puts the commissioning verdict on the recorded analysis DAG as a seam `Set<NodeId>` whose distinctness is the TYPE's, so the sweep's staleness closure flips it the moment either upstream drifts. Sampling algebra, statistics, and uncertainty propagation stay seam-owned — a lane-local downsample, a re-derived completeness, or a call-site `measured.Si - predicted.Si` is the deleted form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Stated acceptance law per deployment, never a per-call argument: two commissioning verdicts over one element must
// not disagree by argument, the same reason AssessmentVerdict's margin band is a const rather than a knob. Admission
// is at CONSTRUCTION — the `bool Invalid` property this replaces guarded at each use and disagreed with the sibling
// Analysis/circulation EgressPolicy, which already admits this way; an invalid policy is now unrepresentable, so the
// gate ladder below opens on the window rather than on a re-test of a value it already holds.
[ComplexValueObject]
public sealed partial class CommissioningPolicy {
    public double CompletenessFloor { get; }
    public double ToleranceFraction { get; }

    public static readonly CommissioningPolicy Canonical = Create(completenessFloor: 0.9, toleranceFraction: 0.1);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double completenessFloor, ref double toleranceFraction) =>
        validationError = double.IsFinite(completenessFloor) && completenessFloor is > 0.0 and <= 1.0
            && double.IsFinite(toleranceFraction) && toleranceFraction > 0.0
                ? null
                : new ValidationError(message: "<commissioning-policy-invalid>");
}

// --- [MODELS] ------------------------------------------------------------------------------
// ONE commissioning request: the element under test, the assessed route the prediction came from, the aspect naming
// BOTH sides, the query window, and the acceptance policy. The sweep carries a roster of these beside its assessment
// requests, so the five positionals cross one boundary as one value rather than five parallel sequences.
public readonly record struct CommissioningAsk(
    AssessmentRoute Assessed, PropertyName Aspect, Interval Window, CommissioningPolicy Policy);

// The dispatch Assessed counterpart for the commissioning rail: the delta the caller applies, the receipt the
// telemetry rail emits, and the comparison itself typed — so a consumer reads the three magnitudes and the coverage
// the verdict was drawn over without re-decoding the payload this pass just wrote. The Verdict stays the TYPED row
// and the receipt's string projects from it at mint, so verdict and receipt read one source.
public sealed record Commissioned(
    GraphDelta Delta, ComputeReceipt.Assessment Receipt,
    MeasureValue Measured, MeasureValue Predicted, MeasureValue Residual,
    double Completeness, AssessmentVerdict Verdict);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class Analysis {
    // The fold IS the solver on this route, so the seam's "the AnalysisRoute token OR the InputKey MUST fold the
    // solver tool+version" obligation discharges by folding this token into the content key AND stamping it as the
    // EvidenceRun version — a revised residual or banding rule re-keys to a fresh node instead of false-hitting a
    // prior revision's Computed verdict.
    public const string CommissioningRevision = "commissioning-1";

    // Commissioning fact names cross into the payload every consumer reads, so they earn consts on this owner under
    // the same law the two seismic names ride.
    public const string MeasuredFact = "measured";
    public const string PredictedFact = "predicted";
    public const string ResidualFact = "residual";
    public const string CoverageFact = "observed-completeness";

    const string CommissionedSuffix = "+commissioned";

    static readonly Op CommissioningKey = Op.Of(name: nameof(Commission));

    // The derived route: the assessed row's key, the element, and the aspect. Element and aspect fold because
    // Supersede keys on (discipline, route) and a token shared across two aspects of one wall — or across two walls
    // — would flip a neighbour's live verdict Superseded. AnalysisRoute.Of normalizes and lower-cases at admission
    // and RAILS a blank token, so the token is opaque by construction and the element id carries the discriminating
    // entropy. The aspect egresses through the declared key CONVERSION, never a `.Value` read: PropertyName leaves
    // KeyMemberName generated, so its key member is the private field and only the conversion operator reaches it.
    public static Fin<AnalysisRoute> CommissioningRoute(AssessmentRoute assessed, NodeId element, PropertyName aspect) =>
        AnalysisRoute.Of($"{assessed.Key}{CommissionedSuffix}:{element.Value}:{(string)aspect}", CommissioningKey);

    // The commissioning rail entry. The gate ladder is a QUERY EXPRESSION rather than a nested Bind tower: each step
    // depends on the last, so sequencing IS the dependence and the order on the page is the order in the code —
    // window boundedness, prediction, series, quantity triple, coverage, figure, residual, land. Policy admission is
    // absent by construction, the [ComplexValueObject] having gated it at the caller's own Create.
    public static Fin<Commissioned> Commission(
        ElementGraph graph, Element element, CommissioningAsk ask, CorrelationId correlation, IClock clock) {
        Instant started = clock.GetCurrentInstant();
        return from _window in Windowed(ask.Window)
               from prediction in Predicted(element, ask)
               from series in Metered(element, ask)
               from _triple in Agreed(series, prediction.Measure)
               from coverage in Covered(series, ask)
               from measured in series.Statistics.Representative(series.Sampling, CommissioningKey)
               from residual in Residual(measured, prediction.Measure)
               from landed in Land(
                   graph,
                   new CommissioningFold(element, ask, prediction.Payload, series, measured, prediction.Measure, residual, coverage),
                   correlation, clock, started)
               select landed;
    }

    // Every gate input the Land fold reads, as ONE value: the eleven positionals threaded through four private
    // members are the parameter ladder a carrier deletes, and the carrier also makes the content key and the
    // write-back read the SAME evidence set rather than two hand-matched argument lists. The ask absorbs four of
    // the former columns, so the carrier states only what the gates DERIVED.
    readonly record struct CommissioningFold(
        Element Element, CommissioningAsk Ask, AssessmentPayload Predicted, ObservationSeries Series,
        MeasureValue Measured, MeasureValue Prediction, MeasureValue Residual, double Coverage);

    // NodaTime Interval.Start/End THROW on an unbounded side, so the caller's query window — the one un-admitted
    // input on this path — crosses the bar BEFORE any read touches an endpoint; a gate that read an endpoint to
    // prove boundedness would throw PAST the rail rather than onto it.
    static Fin<Unit> Windowed(Interval window) =>
        window.HasStart && window.HasEnd
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.WindowUnbounded, string.Empty));

    // The Consumable capability is the WHOLE readability gate — Computed and the readable-but-drifted Stale carry it,
    // Superseded and every in-flight row do not — so the comparison draws against exactly the one prediction the
    // one-usable-node law leaves standing, and the blanket node-exists read is the same defect the dispatch's
    // lifecycle switch deleted.
    static Fin<(AssessmentPayload Payload, MeasureValue Measure)> Predicted(Element element, CommissioningAsk ask) =>
        AnalysisRoute.Of(ask.Assessed.Key, CommissioningKey)
            .Bind(route => element.Assessments
                .Find(row => row.Discipline == ask.Assessed.Discipline && row.Route == route
                    && row.Outcome.Capabilities.Admits(OutcomeCapability.Consumable))
                .ToFin(new ComputeFault.AssessmentInputMissing(
                    AssessmentInputReason.AssessmentUnusable, $"{ask.Assessed.Key}:{element.Id.Value}")))
            .Bind(payload => payload.ResultMeasure(ask.Aspect)
                .ToFin(new ComputeFault.AssessmentInputMissing(
                    AssessmentInputReason.MeasureAbsent, $"{(string)ask.Aspect}:{ask.Assessed.Key}"))
                .Map(measure => (payload, measure)));

    // A re-mounted instrument OPENS A FRESH SERIES (SensorId is deployment-scoped and Window.Start folds into the
    // stream identity), so one aspect legitimately carries several series on one element. The comparison selects the
    // deployment that actually covers the asked window and, among those, the LATEST — the current mounting — because
    // a first-match read would silently answer from a decommissioned sensor's record. The latest is a MAX fold over
    // the covering set, never a full ordering for a single-element question.
    static Fin<ObservationSeries> Metered(Element element, CommissioningAsk ask) =>
        element.Observations
            .Filter(series => series.Aspect == ask.Aspect
                && series.Window.Start < ask.Window.End && ask.Window.Start < series.Window.End)
            .Fold(Option<ObservationSeries>.None, static (latest, series) =>
                latest.Filter(held => held.Window.Start >= series.Window.Start).IfNone(series))
            .ToFin(new ComputeFault.AssessmentInputMissing(
                AssessmentInputReason.SeriesAbsent, $"{(string)ask.Aspect}:{element.Id.Value}"));

    // The triple REFUSES and never coerces (the cluster Boundary owns why). The predicted side's CanonicalUnit is
    // Option<string> — absent exactly where the registry could name no unit — so an absent unit refuses here rather
    // than passing an agreement the seam never stated.
    static Fin<Unit> Agreed(ObservationSeries series, MeasureValue predicted) =>
        series.Observed == predicted.Type
        && series.Signature == predicted.Dimension
        && predicted.CanonicalUnit.Match(
            Some: token => string.Equals(series.CanonicalUnit, token, StringComparison.Ordinal),
            None: static () => false)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing(
                AssessmentInputReason.QuantityDisagreement, $"{series.Observed.Value}:{predicted.Type.Value}"));

    // Expected answers None for an event-driven stream and for an unbounded window; Completeness answers None over a
    // zero denominator. Either absence REFUSES — the fabricated full-coverage read over an unanswerable denominator
    // is exactly the shape a commissioning verdict must never publish.
    static Fin<double> Covered(ObservationSeries series, CommissioningAsk ask) =>
        series.Statistics.Completeness(series.Expected(ask.Window))
            .ToFin(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.CoverageUnanswerable, series.Sensor.Value))
            .Bind(share => share >= ask.Policy.CompletenessFloor
                ? Fin.Succ(share)
                : Fin.Fail<double>(new ComputeFault.AssessmentInputMissing(
                    AssessmentInputReason.UnderCovered, $"{share:R}:{ask.Policy.CompletenessFloor:R}")));

    // The seam exposes NO subtraction: Sum is the one same-type addition entry (its Type guard the one cross-type
    // gate) and Scale the one magnitude scaling, both propagating the MeasureBand through the seam's own algebra.
    // The residual is therefore measured + (-1 × predicted) composed from those two — a call-site Si difference
    // strands both the instrument band and the solver band exactly where the verdict reads them.
    static Fin<MeasureValue> Residual(MeasureValue measured, MeasureValue predicted) =>
        predicted.Scale(-1.0).Bind(negated => MeasureValue.Sum(Seq(measured, negated), CommissioningKey));

    // BAND-AWARE deviation onto the ONE verdict axis: the band is the worst-case residual magnitude the evidence
    // admits, so a residual whose uncertainty reaches past the tolerance bands as the exceedance it may be rather
    // than the pass its nominal alone would claim. EXACTNESS IS BAND ABSENCE at the seam, so an unbanded residual
    // falls back to its own magnitude. A prediction of ZERO magnitude carries no scale to band against and the
    // deviation is honestly ABSENT — the Option is what the verdict's not-applicable arm reads, where the former
    // divide-to-non-finite left the intent in prose alone.
    static Option<double> Deviation(MeasureValue residual, MeasureValue predicted, CommissioningPolicy policy) =>
        Some(policy.ToleranceFraction * Math.Abs(predicted.Si))
            .Filter(static scale => scale > 0.0)
            .Map(scale => residual.Uncertainty.Match(
                Some: static band => Math.Max(Math.Abs(band.LowerSi), Math.Abs(band.UpperSi)),
                None: () => Math.Abs(residual.Si)) / scale)
            .Filter(double.IsFinite);

    // Write-back under the derived route: the same content-keyed node-id mint, supersede close-out, and Assign edge
    // every dispatch write-back takes, so a commissioning verdict is an ordinary Assessment node the sweep, the cache
    // dispatch, and the retention class already know how to carry. DependsOn names BOTH upstreams as a seam
    // Set<NodeId>, so the staleness closure flips this verdict when either the series or the prediction drifts.
    static Fin<Commissioned> Land(
        ElementGraph graph, CommissioningFold fold, CorrelationId correlation, IClock clock, Instant started) {
        double tolerance = graph.Header.Tolerance;
        Option<double> ratio = Deviation(fold.Residual, fold.Prediction, fold.Ask.Policy);
        AssessmentVerdict verdict = AssessmentVerdict.FromRatio(ratio);
        // The three magnitude facts mint TOTAL off admitted MeasureValues while the coverage share crosses the raw
        // scalar gate, so only the fallible one threads Fin. The governing ratio rides the bag only when PRESENT: an
        // absent deviation is the legitimate not-applicable verdict here (a prediction of zero magnitude carries no
        // scale to band against), where the dispatch write-back rails a non-finite ratio because a runner's is a
        // solver defect — same figure, opposite meaning, so the two paths differ by construction rather than by luck.
        return from route in CommissioningRoute(fold.Ask.Assessed, fold.Element.Id, fold.Ask.Aspect)
               let seriesId = ObservationNodeId(fold.Series, tolerance)
               let key = CommissioningKeyOf(graph, fold, route, seriesId)
               from nodeId in AssessmentNodeId(fold.Ask.Assessed.Discipline, route, key.Value, tolerance)
               from predictedId in AssessmentNodeId(fold.Predicted.Discipline, fold.Predicted.Route, fold.Predicted.InputKey, tolerance)
               from coverage in AssessmentFact.Ratio(CoverageFact, fold.Coverage)
               from bag in Banded(
                   Seq(AssessmentFact.Measure(MeasuredFact, fold.Measured),
                       AssessmentFact.Measure(PredictedFact, fold.Prediction),
                       AssessmentFact.Measure(ResidualFact, fold.Residual),
                       coverage)
                   .Fold(Map<PropertyName, PropertyValue>(), static (held, fact) => held.AddOrUpdate(fact.Name, fact.Value))
                   .AddOrUpdate(VerdictKey, Chosen(verdict)),
                   ratio)
               from content in PayloadContent.Results(bag, None, CommissioningKey)
               from provenance in EvidenceRun.Of(
                   "rasm.compute", route.Value, CommissioningRevision, clock.GetCurrentInstant(), CommissioningKey,
                   elapsed: clock.GetCurrentInstant() - started, correlation: Some(correlation))
               from payload in AssessmentPayload.Open(
                   fold.Ask.Assessed.Discipline, route, key.Value, AssessmentOutcome.Computed, content, provenance,
                   CommissioningKey, Set(seriesId, predictedId))
               from delta in Supersede(graph, fold.Ask.Assessed.Discipline, route, nodeId,
                   GraphDelta.Empty.Put(new Node.Assessment(nodeId, payload)))
               select new Commissioned(
                   delta.Link(new Relationship.Assign(fold.Element.Id, nodeId, AssignKind.Assessment)),
                   Receipt(fold.Ask.Assessed.Discipline, route.Value, key, verdict, ratio, correlation, provenance.Elapsed),
                   fold.Measured, fold.Prediction, fold.Residual, fold.Coverage, verdict);
    }

    // The measured evidence's node id is a PURE FUNCTION of the series — Runtime/observation#OBSERVATION_LANE mints
    // it from this same projection — so the DependsOn entry derives with no graph lookup and the producer and the
    // consumer cannot disagree about which node the verdict depends on.
    static NodeId ObservationNodeId(ObservationSeries series, double tolerance) =>
        NodeId.Of(new NodeSeed.Content(new Node.Observation(NodeId.Of(new NodeSeed.Placement()), series), tolerance));

    // The commissioning InputKey folds the measured evidence's own EXTENT, not just its identity: the series node id
    // is the STREAM's content self-hash and holds constant across every append, so a key folding it alone would
    // false-hit the prior verdict the moment new data landed — exactly the re-run commissioning exists to serve. The
    // advancing Window.End and the census total are what move per flush, so both fold; the predicted side folds its
    // InputKey so a re-solved prediction re-keys too; the window and the acceptance policy fold because both band
    // the verdict; and the revision token discharges the seam's solver-version obligation. The fold STREAMS through
    // the seam's one tolerance-bound entry — no writer is constructed here and no preimage is materialized.
    // Exemption: the writer emission is the canonical byte-boundary statement seam.
    static ContentAddress CommissioningKeyOf(
        ElementGraph graph, CommissioningFold fold, AnalysisRoute route, NodeId seriesId) =>
        ContentAddress.Of(
            (fold, route, seriesId, graph.Header.Tolerance), graph.Header.Tolerance,
            static (state, w) => w.Double(state.Tolerance).String(state.route.Value).String(CommissioningRevision)
                .String(state.seriesId.Value)
                .I64(state.fold.Series.Window.End.ToUnixTimeTicks()).Ordinal(state.fold.Series.Statistics.Observed)
                .U128(state.fold.Predicted.InputKey)
                .I64(state.fold.Ask.Window.Start.ToUnixTimeTicks()).I64(state.fold.Ask.Window.End.ToUnixTimeTicks())
                .Double(state.fold.Ask.Policy.CompletenessFloor).Double(state.fold.Ask.Policy.ToleranceFraction));
}
```

## [06]-[RESEARCH]

(none)
