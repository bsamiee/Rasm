# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner: the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment rail — the EN 15978 embodied-carbon takeoff and the supply/install/lifecycle cost rollup. Each folds the relocated `Analysis/aggregator` (`AggregateEnvironmental`/`AggregateCost`) over the seam `MaterialComposition` read DIRECTLY from the concrete `Rasm.Element` `ElementGraph` (above the seam, no `IElementProjection`), distributing each ply's per-module GWP and per-unit cost by the element's baked `Qto_*BaseQuantities` takeoff. Where a ply material carries no baked EN 15978 declaration, the async `EnrichCarbon` ingress resolves one from the EC3 / openEPD REST service (Building Transparency's embodied-carbon graph) through a fallback ladder — the freshest valid product `Epd` in the ply's own category, then the category `StatisticsDto` conservative-estimate substitution line — decoded and basis-tagged from the EPD `declared_unit` (its native per-m³/per-m²/per-kg/per-item basis, the SAME `MeasurementBasis` the cost case carries) once onto the seam `MaterialPropertySet.Environmental` and applied as a `GraphDelta` BEFORE the pure sync `RunCarbon`, so a fully-declared model needs no network call and the takeoff stays a pure graph read. The EC3 client is hand-thin over a typed `HttpClient` + a source-generated `System.Text.Json` context (no SDK to pin): only the GET read surface is exposed, every read returns `Fin<T>` (a non-2xx, a decode failure, or a missing `impacts[method].gwp` mints a `ComputeFault`, never an exception in domain flow), and a SUCCESS payload is cached in `HybridCache` keyed by `XxHash128(kind, omf, page|method)` so a token-metered endpoint is never re-hit for an unchanged query — a transient fault is NEVER cached. The GWP stays a raw kgCO2e domain scalar — CO2-equivalence is a domain basis, not an SI dimension, so it is carried as a dimensionless `MeasureValue` with the `kgCO2e` unit label and is NEVER forced through `UnitsNet.Mass` nor through the UnitsNet-resolving `MeasureValue.Of`. Each runner returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the governing ratio the whole-life carbon (or in-place cost) against a target. Construction SCHEDULING and 4D cost-loading stay in `Rasm.Bim` (MPXJ); this is the embodied material takeoff only, the cost arm bracketed (`§1`) to the aggregator fold plus the fact emit.

## [01]-[INDEX]

- [01]-[EC3_BOUNDARY]: the `Ec3Service` typed-`HttpClient` read client (search/statistics/by-uuid), the openEPD wire types (`Epd`/`ScopeSet`/`Measurement`/`Amount`/`StatisticsDto`/`Envelope<T>`), the success-only `XxHash128` content-key cache, the `LciaMethod` selector, and the GWP raw-kgCO2e discipline.
- [02]-[CARBON_RUNNER]: `LifecycleAssessment.RunCarbon` — the pure-sync EN 15978 takeoff folding `AggregateEnvironmental` over each target's composition and baked takeoff; `LifecycleAssessment.EnrichCarbon` — the async EC3 ingress resolving each undeclared ply through the product-EPD-then-category-statistic fallback ladder, basis-tagging each declaration from its `declared_unit` to its native `MeasurementBasis` (`Normalize`), and returning the enriching `GraphDelta`; the Compute-owned `TakeoffOf` base-quantity read.
- [03]-[COST_RUNNER]: `LifecycleAssessment.RunCost` — the supply/install/lifecycle rollup folding `AggregateCost` over the graph, guarded to the requested `Currency`, emitting the in-place-cost fact stream.

## [02]-[EC3_BOUNDARY]

- Owner: `Ec3Service` the typed-`HttpClient` embodied-carbon read client; the openEPD wire-type family (`Epd`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`); the success-only `XxHash128` content-key cache; the `LciaMethod` `[SmartEnum<string>]` impact-method selector; the `CarbonQuery` request input the `AssessmentRequest.Carbon` case carries.
- Entry: `public Task<Fin<StatisticsDto>> Statistics(string omf, LciaMethod method)` · `public Task<Fin<Seq<Epd>>> Search(string omf, int page)` · `public Task<Fin<Epd>> ByUuid(string uuid)` — every read GET-only (the POST/PUT/PATCH publisher verbs are absent from the typed surface, so a write cannot be emitted with a READ-scope token), each returning `Fin<T>` where a non-2xx mints `ComputeFault.EndpointUnreachable` (2208) and a decode failure `ComputeFault.AnalysisRunFailed` (2219) on the band rather than throwing into domain flow.
- Auto: the three reads share ONE polymorphic `Cached<T>` fold parameterized by the decode shape (`Unwrap<T>` for the `{payload, meta}` envelope, `Bare<T>` for the by-UUID document) — no parallel `GetEnvelope`/`GetBare` pair; the cache stores the SUCCESS DTO ONLY (`Epd[]`/`StatisticsDto`/`Epd`, never a `Fin` or a `Seq`), the factory throwing the boundary fault so `HybridCache.GetOrCreateAsync` writes nothing on a failure and a transient `429`/`5xx` never poisons a content-key; the cache slot is `XxHash128.HashToUInt128` over the `(kind, omf, page|method)` string; the AppHost-owned resilience handler honors `429` + `Retry-After` as the backoff floor.
- Packages: `System.Net.Http` (typed client + `ReadFromJsonAsync(JsonSerializerContext)`), `System.Text.Json` (source-generated context, AOT-safe), `System.IO.Hashing` (`XxHash128.HashToUInt128`), Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync` stateful overload), LanguageExt.Core, NodaTime (`Instant`), BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row; a new decoded openEPD member is one source-gen context property; a new read endpoint is one method composing the same `Cached<T>` fold — the client widens by row/method, never a second HTTP client and never a per-endpoint cache path.
- Boundary: only the GET read surface is consumed (Rasm is a carbon consumer, never a declaration publisher); the GWP `Measurement.Mean` is kgCO2e per declared unit and is NOT a `UnitsNet` quantity — it crosses interior signatures as a raw `double` and lands as a dimensionless `MeasureValue` labeled `kgCO2e` through `DomainMeasure`, never `UnitsNet.Mass` and never the abbreviation-resolving `MeasureValue.Of(value, unit, key)` (which would reject `kgCO2e`); the EC3 `Measurement` is per the EPD `declared_unit`, so the ingress `Normalize`s the per-module vector to per-ONE-unit of its NATIVE basis and TAGS that `MeasurementBasis` the `AggregateEnvironmental` fold scales by — a volume `declared_unit` -> PerM3, an area -> PerM2 (the per-m² membrane/board EPD the old per-m³-only normalization SKIPPED), a mass -> PerKg, else the `kg_per_declared_unit` -> PerKg chain — density NOT read at ingress (a per-kg basis resolves to mass at AGGREGATION, against the ply `Mechanical.Density` the aggregator reads there), an unresolvable bare-count declaration railed (the ply skipped), so the migration source's blind per-m³ assumption that mis-scaled a per-kg EPD by ~1000× is the deleted form; hyphenated LCIA scope keys (`A1A2A3`, `B1`…`B7`, `C1`…`C4`) require `[JsonPropertyName]` aliases; the `fields` query mask trims the response to the decoded projection so the source-gen context carries only the consumed members and the token cost stays minimal; a failed read is the explicit `Fin.Fail` rail the caller surfaces, never a cached failure re-served as success.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LciaMethod {
    public static readonly LciaMethod En15978 = new("EN 15978:2011");
    public static readonly LciaMethod IpccAr6 = new("IPCC AR6");
    public static readonly LciaMethod Traci21 = new("TRACI 2.1");
    public static readonly LciaMethod Ef31    = new("EF 3.1");
    public static readonly LciaMethod Unknown = new("Unknown LCIA");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The carbon request input the AssessmentRequest.Carbon case carries: the category OMF (the default scope), an
// optional per-material OMF override (a multi-material assembly resolves each ply from its OWN EC3 category — concrete,
// insulation, and gypsum never share one EPD), the LCIA method, and the design target the verdict ratios against.
public sealed record CarbonQuery(string Omf, Map<string, string> OmfByMaterial, LciaMethod Method, double TargetKgCo2e) {
    public static CarbonQuery Of(string omf, LciaMethod method, double target) => new(omf, Map<string, string>(), method, target);
}

// The source-generated System.Text.Json wire projection — only the consumed members; the `fields` query mask trims the
// rest server-side. The search/statistics reads wrap the result in { payload, meta }; the decoder reads `payload` only,
// the unconsumed `meta` object skipped by the deserializer (no pager/warning surface the runner does not act on).
public sealed record Envelope<T>(T Payload);

public sealed record Measurement(double Mean);

// The openEPD unit-bearing quantity (declared_unit / kg_per_declared_unit): a magnitude plus a UnitsNet-resolvable unit
// abbreviation the basis normalization coerces to SI once. The qty/unit keys decode under the context camelCase policy
// (no alias); the uncertainty-free Amount carries no rsd. This is the field the per-declared-unit -> per-m³ chain reads.
public sealed record Amount(double? Qty, string? Unit);

// The EN 15978 life-cycle module set the decoder bands onto the seam LifecycleStage 6-vector. ToStageVector sums the
// FULL use-stage B1..B7 and end-of-life C1..C4 — never only B1/C1 — so the whole-life GWP folds every module the EPD
// declares; A1A2A3 is the cradle-to-gate product total, D the beyond-system-boundary credit.
public sealed record ScopeSet(
    [property: JsonPropertyName("A1A2A3")] Measurement? A1A2A3,
    [property: JsonPropertyName("A4")] Measurement? A4, [property: JsonPropertyName("A5")] Measurement? A5,
    [property: JsonPropertyName("B1")] Measurement? B1, [property: JsonPropertyName("B2")] Measurement? B2,
    [property: JsonPropertyName("B3")] Measurement? B3, [property: JsonPropertyName("B4")] Measurement? B4,
    [property: JsonPropertyName("B5")] Measurement? B5, [property: JsonPropertyName("B6")] Measurement? B6,
    [property: JsonPropertyName("B7")] Measurement? B7,
    [property: JsonPropertyName("C1")] Measurement? C1, [property: JsonPropertyName("C2")] Measurement? C2,
    [property: JsonPropertyName("C3")] Measurement? C3, [property: JsonPropertyName("C4")] Measurement? C4,
    [property: JsonPropertyName("D")] Measurement? D) {
    public double[] ToStageVector() => [
        Mean(A1A2A3), Mean(A4), Mean(A5),
        Mean(B1) + Mean(B2) + Mean(B3) + Mean(B4) + Mean(B5) + Mean(B6) + Mean(B7),
        Mean(C1) + Mean(C2) + Mean(C3) + Mean(C4),
        Mean(D)];
    static double Mean(Measurement? m) => m?.Mean ?? 0.0;
}

public sealed record Epd(string? Id, [property: JsonPropertyName("valid_until")] Instant? ValidUntil,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit,
    [property: JsonPropertyName("kg_per_declared_unit")] Amount? KgPerDeclaredUnit,
    Dictionary<string, Dictionary<string, ScopeSet>> Impacts) {
    // The per-module carbon for the chosen method: impacts[method]["gwp"] — None when the EPD omits the method or gwp.
    public Option<ScopeSet> Gwp(LciaMethod method) =>
        Impacts.TryGetValue(method.Key, out var set) && set.TryGetValue("gwp", out var gwp) ? Some(gwp) : None;
}

// The category-scoped GWP substitution line (kgCO2e per declared unit): the EC3 conservative_estimate (80th-percentile)
// is the generic value a ply with no fresh product EPD falls back to; declared_unit carries the basis the per-m³
// normalization reads (a category statistic carries no kg_per_declared_unit — a mass-based category needs the ply
// density). The response's other percentile lines are ignored.
public sealed record StatisticsDto(
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class Ec3Service(HttpClient http, HybridCache cache, JsonSerializerContext json) {
    // The freshest-EPD search reads one page WIDE rather than many pages DEEP: a single token charge surfaces enough
    // category candidates for Freshest to pick the latest valid_until, never a per-ply multi-page crawl.
    const int SearchPageSize = 100;

    public Task<Fin<StatisticsDto>> Statistics(string omf, LciaMethod method) =>
        Cached<StatisticsDto>($"stat:{omf}:{method.Key}", $"/v2/epds/statistics?omf={Uri.EscapeDataString(omf)}", Unwrap<StatisticsDto>);

    public async Task<Fin<Seq<Epd>>> Search(string omf, int page) =>
        (await Cached<Epd[]>($"search:{omf}:{page}",
            $"/v2/epds/search?omf={Uri.EscapeDataString(omf)}&page_number={page}&page_size={SearchPageSize}&fields=id,valid_until,declared_unit,kg_per_declared_unit,impacts", Unwrap<Epd[]>))
            .Map(static e => e.ToSeq());

    public Task<Fin<Epd>> ByUuid(string uuid) =>
        Cached<Epd>($"epd:{uuid}", $"/epds/{Uri.EscapeDataString(uuid)}?fields=id,declared_unit,kg_per_declared_unit,impacts", Bare<Epd>);

    // ONE polymorphic fetch+cache fold over the decode shape — no parallel GetEnvelope/GetBare. Cache a SUCCESS ONLY:
    // the factory throws the boundary fault on a non-2xx or a decode miss, so HybridCache writes nothing on failure and a
    // transient 429/5xx never poisons a token-metered content-key; the cached value is the source-gen DTO (never a Fin or
    // a Seq), keyed by the XxHash128 of the (kind, omf, page|method) string. Exemption: the HybridCache + HTTP boundary —
    // the throw converts back onto the Fin rail exactly once at this seam (the §[03] Auto async-network boundary).
    async Task<Fin<T>> Cached<T>(string key, string path, Func<HttpContent, JsonSerializerContext, ValueTask<Option<T>>> decode) where T : notnull {
        string slot = XxHash128.HashToUInt128(MemoryMarshal.AsBytes(key.AsSpan())).ToString();
        try {
            return Fin.Succ(await cache.GetOrCreateAsync(slot, (http, json, path, decode),
                static async (state, ct) => {
                    using HttpResponseMessage response = await state.http.GetAsync(state.path, ct);
                    return !response.IsSuccessStatusCode
                        ? throw new Ec3Boundary(new ComputeFault.EndpointUnreachable($"<ec3:{(int)response.StatusCode}:{state.path}>"))
                        : (await state.decode(response.Content, state.json)).IfNone(() => throw new Ec3Boundary(new ComputeFault.AnalysisRunFailed($"<ec3-decode:{state.path}>")));
                }));
        }
        catch (Ec3Boundary boundary) { return Fin.Fail<T>(boundary.Fault); }
    }

    // The source-gen decode rides the NON-generic ReadFromJsonAsync(Type, JsonSerializerContext) overload — the generic
    // ReadFromJsonAsync<T> only binds JsonSerializerOptions/JsonTypeInfo<T>/CancellationToken, never a JsonSerializerContext,
    // so the closed Envelope<Epd[]>/Envelope<StatisticsDto>/Epd the context registers resolve through the Type form.
    static async ValueTask<Option<T>> Unwrap<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional(((Envelope<T>?)await content.ReadFromJsonAsync(typeof(Envelope<T>), json))?.Payload);

    static async ValueTask<Option<T>> Bare<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((T?)await content.ReadFromJsonAsync(typeof(T), json));

    // The boundary-crossing carrier — a ComputeFault lifted across the HybridCache factory throw, converted back to
    // Fin.Fail at the one catch so a failed read never caches and never escapes as a raw exception.
    sealed class Ec3Boundary(Error fault) : Exception { public Error Fault { get; } = fault; }
}
```

## [03]-[CARBON_RUNNER]

- Owner: `LifecycleAssessment.RunCarbon` the pure-sync EN 15978 embodied-carbon assessment (a graph read, no network); `LifecycleAssessment.EnrichCarbon` the async EC3 ingress that decodes EC3 declarations onto the seam `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; the Compute-owned `LifecycleGraphReads.TakeoffOf` base-quantity read; the `CarbonQuery` request input.
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks)` folds `AssemblyAggregator.AggregateEnvironmental` over each target's seam `MaterialComposition` and baked `TakeoffOf` geometry, emitting the `whole-life-gwp`/`embodied-carbon-intensity`/per-stage-`A1-A3`…`D`/`recycled-content` facts, the governing ratio the whole-life carbon against the policy target; `public static Task<Fin<GraphDelta>> EnrichCarbon(ElementGraph graph, Ec3Service ec3, AssessmentRequest.Carbon request, ClockPolicy clocks, Op key)` resolves each undeclared ply through the EC3 fallback ladder and returns the enriching delta the composition root applies before `RunCarbon`.
- Auto: `RunCarbon` resolves each ply's seam properties through ONE `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` resolver keyed on the composition's NATIVE `MaterialId` (`graph.Material(MaterialId)`, never a graph `NodeId`), and the per-element area + volume through `TakeoffOf` (the baked `Qto_*BaseQuantities`), so a baked declaration and an EC3-resolved declaration fold identically; `EnrichCarbon` enumerates the undeclared PLY materials (the composition's `MaterialId` set lacking the `Environmental` case, NOT the element's directly-associated material), resolves each from EC3 — the freshest `valid_until` product `Epd` in the ply's own `OmfByMaterial` category (falling back to the category `Omf`), then the `StatisticsDto.ConservativeEstimate` substitution line when no fresh product EPD matches — `Normalize`s the per-module `ScopeSet` from its `declared_unit` to per-ONE-unit of its native basis and TAGS that `MeasurementBasis` (no density read at ingress — a per-kg basis resolves to mass at aggregation), embeds the carbon-only per-stage GwpTotal row into the FULL seam `(ImpactCategory × LifecycleStage)` impact matrix through `CarbonMatrix` (the GwpTotal indicator row at its matrix offset, the un-declared indicator rows zeroed — the seam's partial-EPD invariant) and admits it through `MaterialPropertySet.OfEnvironmental`, and accumulates one monoid `GraphDelta` the composition root applies (a ply whose declared basis is unresolvable is skipped, not mis-scaled); the assessment stays a pure sync graph read because every network call lives in the explicit `EnrichCarbon` ingress, never inside the fold.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet`/`OfEnvironmental`, `MaterialPropertyAccess.Environmental`, `ImpactCategory`/`LifecycleStage`, `MeasurementBasis`, `MaterialId`, `NodeId`, `Node.Material`/`Node.QuantitySet`, `Relationship.Assign`/`AssignKind`, `GraphDelta.Put`, `MeasureValue.Of`/`MeasureValue.Si`, `Dimension.VolumeDim`/`Dimension.AreaDim`/`Dimension.MassDim`, `Provenance`), UnitsNet (via `MeasureValue.Of` — the `declared_unit` abbreviation -> SI dimension/scalar coercion the basis tagging rides), Rasm (kernel `Op`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, the `Ec3Service`, NodaTime (`Instant`), BCL inbox (`ImmutableArray<double>` the seam impact-matrix store the ingress builds).
- Growth: a new lifecycle module is one seam `LifecycleStage` row (the `StageGwp` vector, the `ScopeSet` banding, and the aggregator fold widen by data); a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner; a richer EC3 selection (lowest-GWP, spec-matched) is one refinement of `Freshest`.
- Boundary: the PRIMARY GWP is the local `AggregateEnvironmental` over each ply's baked `Environmental` — EC3 is the FALLBACK the async `EnrichCarbon` resolves, cached through `HybridCache`, applied as a `GraphDelta` before the sync `RunCarbon`, so a fully-declared model needs no network call; the takeoff reads the baked `Qto_*BaseQuantities` (`TakeoffOf`) so a target with no base quantity rails `AssessmentInputMissing` rather than a silent zero takeoff; the GWP/intensity stay raw kgCO2e through `DomainMeasure` (dimensionless `MeasureValue` + label), never `UnitsNet.Mass`; `EnrichCarbon` is best-effort — a ply EC3 resolve that fails OR whose `declared_unit` carries no resolvable dimension (a bare-count/item declaration with no `kg_per_declared_unit`) is skipped, leaving `RunCarbon` to rail a still-undeclared ply rather than defaulting a sentinel carbon or admitting a mis-scaled figure (a per-m² or per-kg EPD now folds correctly under its tagged `MeasurementBasis` rather than being dropped); the runner reads the CONCRETE graph (above the seam), the write-back the `Analysis/assessment` spine's content-keyed `Node.Assessment`.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // The pure sync assessment — a graph read folding the aggregator over each target's baked (or EC3-enriched)
    // composition + base-quantity takeoff; the governing ratio is the whole-life carbon against the design target, or
    // double.NaN -> NotApplicable with no target (never a misleading 0.0-ratio Satisfied) — the energy-runner convention.
    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Total: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<carbon-element-missing-composition:{id.Value}>"))
                from geometry in graph.TakeoffOf(id)
                from lifecycle in AssemblyAggregator.AggregateEnvironmental(composition, Resolver(graph), Seq<PlyQuantity>(), geometry)
                select (Facts: state.Facts
                        .Add(DomainMeasure($"{id.Value}/whole-life-gwp", lifecycle.WholeLifeGwpKgCo2e, "kgCO2e"))
                        .Add(DomainMeasure($"{id.Value}/embodied-carbon-intensity", lifecycle.EmbodiedCarbonIntensityKgCo2eM2, "kgCO2e/m²"))
                        .Add(AssessmentFact.Ratio($"{id.Value}/recycled-content", lifecycle.RecycledContentFraction))
                        + StageFacts(id, lifecycle.StageGwp),
                    Total: state.Total + lifecycle.WholeLifeGwpKgCo2e)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts,
                request.Query.TargetKgCo2e > 0.0 ? state.Total / request.Query.TargetKgCo2e : double.NaN,
                new Provenance("LifecycleAssessment", request.Route.Standard, "EC3/openEPD", clocks.Now)));

    // The async EC3 INGRESS — resolves each undeclared PLY material through the product-EPD -> category-statistic
    // fallback ladder, decodes the per-module ScopeSet to the carbon GwpTotal row embedded in the seam (ImpactCategory ×
    // LifecycleStage) impact matrix (CarbonMatrix), and accumulates the enriching delta the composition root applies
    // before the sync RunCarbon, so a fully-declared model needs no network call.
    public static async Task<Fin<GraphDelta>> EnrichCarbon(ElementGraph graph, Ec3Service ec3, AssessmentRequest.Carbon request, ClockPolicy clocks, Op key) {
        GraphDelta delta = GraphDelta.Empty;
        // EC3 ingress boundary: a serial, rate-limited fetch over the token-metered endpoint, each resolved ply accumulated
        // onto the monoid delta (Exemption: the explicit async network boundary — a failed resolve is skipped, not railed,
        // so one stale category never aborts the whole enrichment; RunCarbon rails a ply still undeclared after the pass).
        foreach (Node.Material material in MissingDeclarations(graph, request.Targets))
            delta = (await Resolve(ec3, request.Query, material, clocks.Now, key)).Match(
                Succ: env => delta.Put(material with { Properties = material.Properties.Add(env) }),
                Fail: _   => delta);
        return Fin.Succ(delta);
    }

    // The undeclared PLY materials reachable from the targets: each composition's native MaterialId set, resolved to its
    // material node, filtered to those lacking the seam Environmental case — the plies the aggregator folds, NOT the
    // element's directly-associated container material (a LayerSet's plies, not the layer-set node).
    static Seq<Node.Material> MissingDeclarations(ElementGraph graph, Seq<NodeId> targets) =>
        targets.Choose(graph.CompositionOf)
            .Bind(static c => c.Materials)
            .Distinct()
            .Choose(mid => graph.Material(mid))
            .Filter(static m => m.Properties.Environmental.IsNone)
            .Distinct();

    // The seam-keyed resolver the aggregator folds: a ply MaterialId -> its material node's property set, railing the
    // missing-input fault on an absent material so the aggregator reads the composition's OWN plies by native key.
    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<material-absent:{mid.Value}>"));

    // The fallback ladder for ONE ply: the freshest valid product EPD in the ply's own category, else the category
    // conservative-estimate substitution line; a per-material OMF override resolves a multi-material assembly correctly.
    // Each rung TAGS the EPD's native MeasurementBasis (Normalize) WITHOUT a density — a per-kg basis resolves to mass at
    // AGGREGATION (the aggregator reads the ply density there), so the ingress never needs, nor skips on, a missing density.
    static async Task<Fin<MaterialPropertySet>> Resolve(Ec3Service ec3, CarbonQuery query, Node.Material material, Instant now, Op key) {
        string omf = query.OmfByMaterial.Find(material.MaterialKey.Value).IfNone(query.Omf);
        return await (await ec3.Search(omf, page: 1)).Map(epds => Freshest(epds, query.Method, now)).Match(
            Succ: fresh => fresh.Match(
                Some: epd => Task.FromResult(ToEnvironmental(epd, query.Method, key)),
                None: ()  => Fallback(ec3, omf, query.Method, key)),
            Fail: e => Task.FromResult(Fin.Fail<MaterialPropertySet>(e)));
    }

    static async Task<Fin<MaterialPropertySet>> Fallback(Ec3Service ec3, string omf, LciaMethod method, Op key) =>
        (await ec3.Statistics(omf, method)).Bind(stats => FromStatistics(stats, key));

    // The freshest non-stale EPD carrying the method's gwp: a null valid_until is non-expiring, else valid_until >= now;
    // the latest expiry wins so a category's most-current declaration is chosen.
    static Option<Epd> Freshest(Seq<Epd> epds, LciaMethod method, Instant now) =>
        epds.Filter(e => e.Gwp(method).IsSome && (e.ValidUntil is not { } valid || valid >= now))
            .OrderByDescending(static e => e.ValidUntil ?? Instant.MaxValue)
            .ToSeq().Head;

    // Normalize the EPD's per-module ScopeSet to per-ONE-unit of its NATIVE basis (TAGGED with that MeasurementBasis the
    // AggregateEnvironmental fold scales by, the SAME DeclaredQuantity owner the cost fold uses, so a per-kg/per-m²/per-m³
    // EPD all fold correctly and the migration source's ~1000× per-declared-unit-as-per-m³ error is gone), then EMBED the
    // carbon GwpTotal row into the FULL seam (ImpactCategory × LifecycleStage) impact matrix through the seam-owned
    // MaterialPropertySet.Environmental.CarbonMatrix carbon-row → matrix builder and admit
    // it through OfEnvironmental — the matrix is the seam's ONE impact store, an EC3 declaration carbon-only so the
    // un-declared indicator rows stay zero (the seam's partial-EPD invariant). Normalize rails an unresolvable basis to
    // None so EnrichCarbon skips the ply (RunCarbon then rails it). The cradle-to-gate A1-A3 GWP is the seam's DERIVED read
    // Gwp => IndicatorAt(GwpTotal, A1A3) over the matrix — NOT a headline arg (OfEnvironmental takes the (ImpactCategory ×
    // LifecycleStage) matrix, no gwpKgCo2e param, no GlobalWarmingPotential field; the carbon rides the GwpTotal-row cells).
    // Density is NO LONGER read at ingress — per-kg resolves at aggregation.
    static Fin<MaterialPropertySet> ToEnvironmental(Epd epd, LciaMethod method, Op key) =>
        epd.Gwp(method).Match(
            Some: scope => Normalize(scope.ToStageVector(), epd.DeclaredUnit, epd.KgPerDeclaredUnit, key).Match(
                Some: norm => MaterialPropertySet.OfEnvironmental(
                    norm.Basis, MaterialPropertySet.Environmental.CarbonMatrix(norm.PerUnit), recycledContent: 0.0, endOfLifeRecovery: 0.0,
                    epd: epd.Id ?? "", validUntilYear: epd.ValidUntil?.InUtc().Year ?? 0, key),
                None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<ec3-epd-basis-unresolved:{epd.Id}>"))),
            None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<ec3-epd-missing-gwp:{epd.Id}>")));

    // The category conservative estimate (EC3 80th-percentile substitution value) lands on the A1-A3 product module —
    // the only stage a category statistic resolves; downstream modules stay zero until a product EPD supplies them. The
    // category declared_unit drives the SAME basis Normalize (no kg_per_declared_unit at category scope, so a mass/area/
    // volume declared_unit tags PerKg/PerM2/PerM3 the aggregator scales by — a bare-count category with no unit rails).
    static Fin<MaterialPropertySet> FromStatistics(StatisticsDto stats, Op key) {
        double[] product = new double[LifecycleStage.Count];
        product[LifecycleStage.A1A3.Index] = stats.ConservativeEstimate;
        return Normalize(product, stats.DeclaredUnit, null, key).Match(
            Some: norm => MaterialPropertySet.OfEnvironmental(norm.Basis, MaterialPropertySet.Environmental.CarbonMatrix(norm.PerUnit),
                recycledContent: 0.0, endOfLifeRecovery: 0.0, epd: "ec3-statistics-conservative", validUntilYear: 0, key),
            None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing("<ec3-statistics-basis-unresolved>")));
    }

    // Normalize a per-DECLARED-unit StageGwp vector to per-ONE-unit of its NATIVE basis and TAG the MeasurementBasis the
    // AggregateEnvironmental fold scales by (the SAME basis-aware DeclaredQuantity owner the cost fold uses) — the
    // strongest-dimension route winning through the Option `|` choice: a VOLUME declared_unit -> PerM3 / its declared
    // volume, an AREA -> PerM2 / its declared area (the per-m² membrane/board EPD the prior per-m³ normalization SKIPPED
    // outright), a MASS -> PerKg / its declared mass, else the kg_per_declared_unit chain -> PerKg / kg-per-unit (the EC3
    // "kg_per_declared_unit converts a per-unit GWP to a per-kg GWP" rule for a count/item declaration carrying a mass).
    // Density is NOT read here — a per-kg basis resolves to mass at AGGREGATION (the aggregator scales by volume × the ply
    // Mechanical.Density), so the ingress never needs, nor skips on, a missing density. None only when no dimension
    // resolves (a bare-count/item declaration with no kg_per_declared_unit) so EnrichCarbon skips the ply; a declaration
    // carrying NO unit keeps the PerM3 default. MeasureValue.Of resolves the declared unit's dimension + SI scalar through UnitsNet.
    static Option<(MeasurementBasis Basis, double[] PerUnit)> Normalize(double[] perDeclaredUnit, Amount? declaredUnit, Amount? kgPerDeclaredUnit, Op key) {
        if (declaredUnit is not { } unit) { return Some((MeasurementBasis.PerM3, perDeclaredUnit)); }
        Option<MeasureValue> declared = MeasureValue.Of(unit.Qty ?? 1.0, unit.Unit ?? "", key).ToOption();
        Option<double> kgPerUnit = kgPerDeclaredUnit is { } k ? MeasureValue.Of(k.Qty ?? 0.0, k.Unit ?? "", key).ToOption().Map(static m => m.Si) : None;
        Option<(MeasurementBasis, double[])> byVolume  = declared.Filter(static d => d.Dimension == Dimension.VolumeDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerM3, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byArea    = declared.Filter(static d => d.Dimension == Dimension.AreaDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerM2, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byMass    = declared.Filter(static d => d.Dimension == Dimension.MassDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byKgChain = kgPerUnit.Filter(static kg => kg > 0.0).Map(kg => (MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / kg)));
        return byVolume | byArea | byMass | byKgChain;
    }

    static double[] Scale(double[] vector, double factor) {
        double[] scaled = new double[vector.Length];
        for (int i = 0; i < vector.Length; i++) { scaled[i] = vector[i] * factor; }
        return scaled;
    }

    // The EC3-resolved CARBON-only per-stage GwpTotal row embeds into the FULL seam (ImpactCategory × LifecycleStage)
    // matrix OfEnvironmental admits through the seam's ONE carbon-row → matrix owner MaterialPropertySet.Environmental
    // .CarbonMatrix (the WRITE dual of Environmental.StageGwp) — the GwpTotal indicator row at its stable offset, every
    // un-declared indicator row zeroed (an EC3/openEPD declaration is carbon-only, the seam's partial-EPD invariant), so
    // this ingress never re-spells the offset arithmetic the seam owns; a future EC3 method exposing the full EN 15804+A2
    // indicator set passes its matrix to OfEnvironmental directly, bypassing the carbon-row convenience.
    static Seq<AssessmentFact> StageFacts(NodeId id, ImmutableArray<double> stageGwp) =>
        LifecycleStage.Items.ToSeq().Map(stage => DomainMeasure($"{id.Value}/gwp-{stage.Module}", stageGwp[stage.Index], "kgCO2e"));

    // GWP and in-place cost are DOMAIN-BASIS scalars (kgCO2e, kgCO2e/m², a currency code), NOT UnitsNet quantities — a
    // dimensionless MeasureValue carrying the domain unit label, never the abbreviation-resolving MeasureValue.Of (which
    // would reject kgCO2e). The fact name carries the semantic; the label carries the basis the wire consumer reads flat.
    // The seam MeasureValue is the 4-arg (QuantityType, Dimension, Si, CanonicalUnit) record: a domain scalar carries the
    // Scalar dimension-anonymous QuantityType (the MeasureValue.Zero identity) so the kgCO2e label rides CanonicalUnit
    // while the Type/Dimension keep it untyped-but-dimensionless — never a QTO accessor false-match, never the phantom 3-arg ctor.
    static AssessmentFact DomainMeasure(string name, double si, string unit) =>
        AssessmentFact.Measure(name, new MeasureValue(QuantityType.Scalar, Dimension.Dimensionless, si, unit));
}

// The element geometric takeoff the GWP/cost folds distribute per ply — a Compute-owned ElementGraph extension reading
// the baked Qto_*BaseQuantities (the Bim base-quantity projection), net preferred over gross, scanned across the
// element-type Qto set so a wall/slab/beam reads without a per-type accessor; the seam owns the material/composition
// reads, this discipline takeoff lives in Compute (the §4E above-the-seam pattern). A target with no baked base quantity
// rails the missing input rather than a silent zero takeoff that would zero a volumetric ply's carbon.
public static class LifecycleGraphReads {
    const string NetVolume = "NetVolume";
    const string GrossVolume = "GrossVolume";
    const string NetSideArea = "NetSideArea";
    const string NetArea = "NetArea";
    const string GrossSideArea = "GrossSideArea";

    extension(ElementGraph graph) {
        public Fin<ElementQuantity> TakeoffOf(NodeId element) {
            Seq<Node.QuantitySet> bags = graph.EdgesAt(element)
                .Choose(e => e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == element
                    ? graph.Find<Node.QuantitySet>(a.Definition) : None);
            Option<double> volume = Named(bags, NetVolume) | Named(bags, GrossVolume);
            Option<double> area = Named(bags, NetSideArea) | Named(bags, NetArea) | Named(bags, GrossSideArea);
            return volume.IsNone && area.IsNone
                ? Fin.Fail<ElementQuantity>((Error)new ComputeFault.AssessmentInputMissing($"<element-base-quantities-absent:{element.Value}>"))
                : Fin.Succ(new ElementQuantity(area.IfNone(0.0), volume.IfNone(0.0)));
        }
    }

    static Option<double> Named(Seq<Node.QuantitySet> bags, string name) =>
        bags.Choose(qs => qs.Bag.Values.Find(PropertyName.Create(name))).Head.Map(static m => m.Si);
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup runner.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks)` — folds `AssemblyAggregator.AggregateCost` over each target's seam `MaterialComposition` and baked `TakeoffOf` geometry, summing the per-ply supply/install/lifecycle cost, guarding the aggregated currency against the requested one, and emitting the `supply-total`/`install-total`/`in-place-total` facts.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet.Cost`, `Currency`, `MaterialId`, `NodeId`, `MeasureValue`, `Dimension`, `Provenance`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, the Compute-owned `TakeoffOf`, BCL inbox.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost rail spans all composition cases (a single material or a profile member has a unit supply/install cost); the cost arm is bracketed (`§1`) — the runner stays proportionate (the aggregator fold + the fact emit), the depth reserved for carbon and the physical disciplines.
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the `request.Currency` is load-bearing — the aggregated cost is guarded to it (a material priced in a different `Currency` rails, since the fold carries no exchange rate), so the request currency is a real validation target, never a decorative field; the per-ply quantity derives from the seam `Cost.Basis` against the baked `TakeoffOf` (or a `PlyQuantity` override); a material with no `Cost` case rails `AssessmentInputMissing`; the bracketed rollup carries no acceptance budget, so the governing ratio is `double.NaN` → `NotApplicable` (the informational rating, never a `0.0`-ratio `Satisfied` falsely asserting a budget pass) — the same no-target convention the energy and carbon runners hold.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), InPlace: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<cost-element-missing-composition:{id.Value}>"))
                from geometry in graph.TakeoffOf(id)
                from cost in AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), geometry)
                from _ in cost.Currency.Key == request.Currency
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>((Error)new ComputeFault.AssessmentInputMissing($"<cost-currency-mismatch:{cost.Currency.Key}<>{request.Currency}>"))
                select (Facts: state.Facts
                        .Add(DomainMeasure($"{id.Value}/supply-total", cost.SupplyTotal, cost.Currency.Key))
                        .Add(DomainMeasure($"{id.Value}/install-total", cost.InstallTotal, cost.Currency.Key))
                        .Add(DomainMeasure($"{id.Value}/in-place-total", cost.TotalInPlace, cost.Currency.Key)),
                    InPlace: state.InPlace + cost.TotalInPlace)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, double.NaN,
                new Provenance("LifecycleAssessment", request.Route.Standard, "n/a", clocks.Now)));
}
```

## [05]-[RESEARCH]

- [EC3_OPENEPD_READ]: the embodied-carbon boundary is the EC3 / openEPD REST service (`https://openepd.buildingtransparency.org/api`, bearer auth, token-metered, `429`/`Retry-After` rate-limited) consumed hand-thin over a typed `HttpClient` + a source-generated `System.Text.Json` context — `/v2/epds/search` (OMF → `Epd[]`, page-parameterized), `/v2/epds/statistics` (OMF → `StatisticsDto` reference lines), `/epds/{uuid}` (single declaration). The decoder selects `Epd.impacts["EN 15978:2011"].gwp` then `ScopeSet.ToStageVector` bands the FULL module set (A1A2A3, A4, A5, Σ B1..B7, Σ C1..C4, D) into the per-stage carbon GwpTotal row `CarbonMatrix` embeds in the seam `MaterialPropertySet.Environmental` `(ImpactCategory × LifecycleStage)` impact matrix (the GwpTotal indicator row, the un-declared indicator rows zeroed) — never only B1/C1, so the whole-life GWP folds every declared module; the GWP `Measurement.Mean` is kgCO2e per the EPD `declared_unit`, `Normalize`d once at ingest to per-ONE-unit of its native basis and TAGGED with that `MeasurementBasis` the aggregator scales by (a VOLUME/AREA/MASS `declared_unit` tags PerM3/PerM2/PerKg, else the `kg_per_declared_unit` -> PerKg chain; the EC3 `declared_unit`/`kg_per_declared_unit` fields the search/by-uuid `fields` mask carries) and kept a raw domain scalar thereafter — never forced through `UnitsNet.Mass`. A bare-count declaration with no resolvable dimension is railed; a per-kg basis resolves to mass at AGGREGATION (volume × the ply density), not at ingest, so a per-m² or per-kg EPD now folds correctly instead of being skipped. Only the GET read surface is consumed. Ripple counterpart: `Rasm.Compute/.api/api-ec3` (the openEPD wire contract).
- [FALLBACK_LADDER]: an undeclared ply resolves through a two-rung ladder — the freshest non-stale product `Epd` in the ply's own category (`CarbonQuery.OmfByMaterial`, defaulting to the category `Omf`), then the category `StatisticsDto.ConservativeEstimate` (the EC3 80th-percentile substitution value) when no fresh product EPD matches; `valid_until` gates staleness (a null expiry is non-expiring, else `valid_until >= now`), so a multi-material assembly resolves concrete, insulation, and gypsum each from its own EPD rather than one shared declaration, and a category with no current product EPD still yields a defensible generic value. `EnrichCarbon` is the explicit async ingress applied as a `GraphDelta` before the pure sync `RunCarbon`.
- [CONTENT_KEY_CACHE]: EC3 SUCCESS payloads cache in `HybridCache` keyed by `XxHash128(kind, omf, page|method)` so a token-metered endpoint is never re-hit for an unchanged query; a transient failure is NEVER cached — the `Cached<T>` factory throws the boundary fault so `GetOrCreateAsync` writes nothing on failure and the throw converts back onto the `Fin` rail at the one catch (the same throw-at-boundary discipline `Runtime/channels` uses for the bSDD REST read), the cached value the source-gen DTO (`Epd[]`/`StatisticsDto`/`Epd`), never a `Fin` or a `Seq` the default serializer cannot round-trip. The downstream `Node.Assessment` node is content-keyed on the `(input subgraph, route, discipline policy)` triple by the `Analysis/assessment` spine (the kernel `XxHash128` seed-zero rail, the carbon policy's `Omf`/`OmfByMaterial`/`Method`/`TargetKgCo2e` folded through `AssessmentRequest.CanonicalBytes`), distinct from this ephemeral query cache.
- [EN_15978_TAKEOFF]: the whole-building embodied-carbon takeoff folds `AssemblyAggregator.AggregateEnvironmental` over each element's seam `MaterialComposition` (the relocated engine owns the per-module `Σ(StageGwp_i · quantity_i)` fold), so a baked declaration and an EC3-resolved declaration fold identically onto the `LifecycleStage` band; the per-ply geometric quantity comes from the Compute-owned `TakeoffOf` reading the element's baked `Qto_*BaseQuantities` (net volume + net side area, dimension-true), the cradle-to-gate `A1-A3` and the whole-life total projections over the per-module vector, the `EmbodiedCarbonIntensity` the GWP per element area. The seam `StageGwp` carries its `MeasurementBasis` — the EC3 ingress `Normalize`s every resolved declaration to per-ONE-unit of its native `declared_unit` basis and TAGS it (PerKg/PerM2/PerM3/PerItem) before it lands on the `Environmental` case, so the aggregator scales each ply by the basis-matching element quantity through the SAME `DeclaredQuantity` owner the cost fold uses, and a baked native-basis declaration and an EC3-resolved native-basis declaration both fold correctly under one basis-aware scale (the `Rasm.Materials` `SustainabilityCatalogue` authors each baked `Environmental` row at the EPD's NATIVE `declared_unit` basis — per-kg metals/glass/gypsum/insulation, per-m³ concrete/timber/masonry/stone, per-m² membranes — never a forced curated PerM3, the SAME basis axis the EC3 ingress tags). Ripple counterpart: `Rasm.Materials` `Properties/sustainability` (RETIRE the aggregator half; KEEP single-material `Environmental` authoring lowered onto the seam at the EPD's native `MeasurementBasis`), `Rasm.Element/Composition/material` (the seam `Environmental` case + `LifecycleStage` band + `OfEnvironmental` admission), and `Rasm.Compute` `Analysis/aggregator` (`AggregateEnvironmental` + `ElementQuantity`/`PlyQuantity`).
- [COST_SCOPE]: the cost runner folds `AssemblyAggregator.AggregateCost` for the embodied supply/install/lifecycle rollup over a single `Currency` guarded against the request — construction SCHEDULING, CPM, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ via the `Planning` owner), so the Compute cost arm is the material takeoff and the Bim planning owner is the schedule, aligned by the seam graph never coupled. The cost discipline is bracketed in the campaign (`§1`): the runner is proportionate, the depth reserved for carbon and the physical disciplines.
- [GRAPH_READ_ACCESSORS]: the runners read the concrete graph through the seam accessors `graph.CompositionOf(NodeId) → Option<MaterialComposition>` (lifted to `Fin` at the call) and `graph.Material(MaterialId) → Option<Node.Material>` (the aggregator resolver's native-key lookup, whose `.Properties` the fold reads), plus the Compute-owned `graph.TakeoffOf(NodeId) → Fin<ElementQuantity>` reading the baked base quantities — the element area + volume the per-m² intensity and the per-ply quantity distribution need; the OMF query, the per-material OMF map, the LCIA method, and the carbon target ride `CarbonQuery`. The base-quantity takeoff is COMPUTE-owned (the §4E above-the-seam discipline read), composing the seam quantity-bag primitives, NOT a seam member. Ripple counterpart: `Rasm.Element/Graph/element` (the seam `CompositionOf`/`Material(MaterialId)` reads + the `Node.QuantitySet` bag the takeoff scans) and `Rasm.Bim` (the `Qto_*BaseQuantities` base-quantity projection the takeoff reads).
