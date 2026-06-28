# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner: the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment rail — the EN 15978 embodied-carbon takeoff and the supply/install cost rollup. It folds the per-ply embodied carbon and cost through the relocated `Analysis/aggregator` (`AggregateEnvironmental`/`AggregateCost`) over the seam `MaterialComposition` read DIRECTLY from the `Rasm.Element` `ElementGraph`, and where a material carries no baked EN 15978 declaration it resolves one from the EC3 / openEPD REST service (Building Transparency's embodied-carbon graph) — the embodied-carbon BOUNDARY owner. The EC3 client is hand-thin over a typed `HttpClient` + a source-generated `System.Text.Json` context (no SDK to pin): a category+spec Open Material Filter query returns either per-product `Epd.impacts["EN 15978:2011"].gwp` module measurements (kgCO2e per declared unit) or category `StatisticsDto` percentile reference lines, decoded once into the seam `MaterialPropertySet.Environmental` per-module `StageGwp` vector, cached in `HybridCache` and content-keyed on the `(OMF query, LCIA method, route)` tuple so a token-metered endpoint is never re-hit for an unchanged query. The GWP stays a raw kgCO2e domain scalar — CO2-equivalence is a domain basis, not an SI dimension, so it is NEVER forced through `UnitsNet.Mass`. Each fold returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the verdict the whole-life carbon (or cost) against a target. Construction SCHEDULING and 4D cost-loading stay in `Rasm.Bim` (MPXJ); this is the embodied material takeoff only.

## [01]-[INDEX]

- [01]-[EC3_BOUNDARY]: the `Ec3Service` typed-HttpClient read client (search/statistics/by-uuid), the openEPD wire types (`Epd`/`Impacts`/`ScopeSet`/`Measurement`/`StatisticsDto`), the `(OMF, method, route)` content-key cache, and the GWP raw-kgCO2e discipline.
- [02]-[CARBON_RUNNER]: `LifecycleAssessment.RunCarbon` — the EN 15978 takeoff folding `AggregateEnvironmental` over the graph plus the EC3-resolved declarations and reference lines, emitting the whole-life-carbon fact stream.
- [03]-[COST_RUNNER]: `LifecycleAssessment.RunCost` — the supply/install/lifecycle rollup folding `AggregateCost` over the graph, emitting the in-place-cost fact stream.

## [02]-[EC3_BOUNDARY]

- Owner: `Ec3Service` the typed-HttpClient embodied-carbon read client; the openEPD wire-type family (`Epd`/`Impacts`/`ImpactSet`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`); the `Ec3KeyPolicy` `(OMF, LCIA method, route)` content-key; the `LciaMethod` selector.
- Entry: `public Task<Fin<StatisticsDto>> Statistics(string omf, LciaMethod method)` · `public Task<Fin<Seq<Epd>>> Search(string omf, int page)` · `public Task<Fin<Epd>> ByUuid(string uuid)` — every read GET-only (no publisher verb is exposed, so a write cannot be emitted with a READ-scope token), returning `Fin<T>` where a non-2xx, a deserialization failure, or a missing `impacts[method].gwp` mints a `ComputeFault` on the band rather than throwing.
- Auto: the response is cached in `HybridCache` keyed by `XxHash128(omf || method.Key || page)` so a token-metered endpoint is never re-hit for an unchanged query (`meta.mf_hash` is only the OMF component of that key, not the whole identity); the resilience pipeline (the AppHost-owned standard handler) honors `429` + `Retry-After` as the backoff floor; the streaming pager is a `Seq`/`IAsyncEnumerable` projection driven by `meta.paging.total_pages`.
- Packages: `System.Net.Http` (typed client), `System.Text.Json` (source-generated context, AOT-safe), Microsoft.Extensions.Caching.Hybrid, LanguageExt.Core, NodaTime, BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row; a new decoded openEPD member is one source-gen context property; a new read endpoint is one method — the client widens by row/method, never a second HTTP client.
- Boundary: only the GET read surface is consumed (Rasm is a carbon consumer, never a declaration publisher — the POST/PUT/PATCH verbs are absent from the typed surface); GWP `Measurement.mean` carries the unit `kgCO2e` which is NOT a UnitsNet quantity (CO2-equivalence is a domain basis, not an SI dimension) so it stays a raw `double` kgCO2e domain scalar and is NEVER forced through `UnitsNet.Mass` — mixing a kgCO2e value into a `UnitsNet.Mass` is the boundary error the decoder rejects; `Amount` (`declared_unit`/`kg_per_declared_unit`) canonicalizes through UnitsNet once at the boundary so only the raw SI scalar crosses; hyphenated LCIA scope keys (`gwp-fossil`, `ep-marine`, `ADP-mineral`) require `[JsonPropertyName]` aliases; `valid_until < now` demotes an EPD as stale; the `Assessment.Result` node is content-keyed on the `(OMF query, LCIA method, route)` tuple so an identical carbon query is a cache hit and a 412-noop on the object store.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComputeKeyPolicy, string>]
public sealed partial class LciaMethod {
    public static readonly LciaMethod En15978 = new("EN 15978:2011");
    public static readonly LciaMethod IpccAr6 = new("IPCC AR6");
    public static readonly LciaMethod Traci21 = new("TRACI 2.1");
    public static readonly LciaMethod Ef31    = new("EF 3.1");
    public static readonly LciaMethod Unknown = new("Unknown LCIA");
}

// --- [MODELS] ------------------------------------------------------------------------------
// The carbon request input the AssessmentRequest.Carbon case carries (the OMF category query + the design target).
public sealed record CarbonQuery(string Omf, LciaMethod Method, double TargetKgCo2e);

// The source-generated System.Text.Json wire projection — only the consumed members; `fields` masking trims the rest.
public sealed record Envelope<T>(T Payload, EnvelopeMeta Meta);
public sealed record EnvelopeMeta(Paging Paging, [property: JsonPropertyName("mf_hash")] string? MfHash);
public sealed record Paging([property: JsonPropertyName("total_pages")] int TotalPages, [property: JsonPropertyName("page_size")] int PageSize);

public sealed record Measurement(double Mean, string? Unit, double? Rsd);
public sealed record ScopeSet(
    [property: JsonPropertyName("A1A2A3")] Measurement? A1A2A3, [property: JsonPropertyName("A4")] Measurement? A4,
    [property: JsonPropertyName("A5")] Measurement? A5, [property: JsonPropertyName("B1")] Measurement? B1,
    [property: JsonPropertyName("C1")] Measurement? C1, [property: JsonPropertyName("D")] Measurement? D) {
    public double[] ToStageVector() => [Mean(A1A2A3), Mean(A4), Mean(A5), Mean(B1), Mean(C1), Mean(D)];
    static double Mean(Measurement? m) => m?.Mean ?? 0.0;
}
public sealed record Epd(string? Id, [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit,
    [property: JsonPropertyName("valid_until")] Instant? ValidUntil, Dictionary<string, Dictionary<string, ScopeSet>> Impacts) {
    public Option<ScopeSet> Gwp(LciaMethod method) =>
        Impacts.TryGetValue(method.Key, out var set) && set.TryGetValue("gwp", out var gwp) ? Some(gwp) : None;
}
public sealed record StatisticsDto(
    [property: JsonPropertyName("achievable_target")] double AchievableTarget,
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    double Average, [property: JsonPropertyName("pct50_gwp")] double Median,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);
public sealed record Amount(double? Qty, string? Unit);

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class Ec3Service(HttpClient http, HybridCache cache, JsonSerializerContext json) {
    public Task<Fin<StatisticsDto>> Statistics(string omf, LciaMethod method) =>
        Cached($"stat:{omf}:{method.Key}", () => GetEnvelope<StatisticsDto>($"/v2/epds/statistics?omf={Uri.EscapeDataString(omf)}"));

    public Task<Fin<Seq<Epd>>> Search(string omf, int page) =>
        Cached($"search:{omf}:{page}", () => GetEnvelope<Epd[]>($"/v2/epds/search?omf={Uri.EscapeDataString(omf)}&page_number={page}&fields=id,declared_unit,valid_until,impacts").Map(static e => e.ToSeq()));

    public Task<Fin<Epd>> ByUuid(string uuid) =>
        Cached($"epd:{uuid}", () => GetBare<Epd>($"/epds/{uuid}?fields=id,declared_unit,impacts"));

    async Task<Fin<T>> Cached<T>(string key, Func<Task<Fin<T>>> fetch) {
        UInt128 contentKey = XxHash128.HashToUInt128(MemoryMarshal.AsBytes(key.AsSpan()));
        return await cache.GetOrCreateAsync(contentKey.ToString(), async _ => await fetch());
    }

    async Task<Fin<T>> GetEnvelope<T>(string path) {
        using HttpResponseMessage response = await http.GetAsync(path);
        return !response.IsSuccessStatusCode
            ? Fin.Fail<T>(new ComputeFault.EndpointUnreachable($"<ec3:{(int)response.StatusCode}:{path}>"))
            : (await response.Content.ReadFromJsonAsync<Envelope<T>>(json)) is { Payload: { } payload }
                ? Fin.Succ(payload)
                : Fin.Fail<T>(new ComputeFault.AnalysisRunFailed($"<ec3-decode:{path}>"));
    }

    async Task<Fin<T>> GetBare<T>(string path) {
        using HttpResponseMessage response = await http.GetAsync(path);
        return !response.IsSuccessStatusCode
            ? Fin.Fail<T>(new ComputeFault.EndpointUnreachable($"<ec3:{(int)response.StatusCode}:{path}>"))
            : (await response.Content.ReadFromJsonAsync<T>(json)) is { } payload
                ? Fin.Succ(payload)
                : Fin.Fail<T>(new ComputeFault.AnalysisRunFailed($"<ec3-decode:{path}>"));
    }
}
```

## [03]-[CARBON_RUNNER]

- Owner: `LifecycleAssessment.RunCarbon` the sync EN 15978 embodied-carbon assessment (a pure graph read); `LifecycleAssessment.EnrichCarbon` the async EC3 ingress that decodes EC3 declarations onto the seam `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; the `CarbonQuery` request input (the OMF query, the LCIA method, the per-element area, the target).
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks)` folds `AssemblyAggregator.AggregateEnvironmental` over each target's seam `MaterialComposition` (the baked OR EC3-enriched `Environmental`) and emits the `whole-life-gwp`/`embodied-carbon-intensity`/per-stage-`A1A2A3`…`D`/`recycled-content` facts, the verdict the whole-life carbon against the policy target; `public static Task<Fin<GraphDelta>> EnrichCarbon(ElementGraph graph, Ec3Service ec3, AssessmentRequest.Carbon request, LciaMethod method)` resolves each material lacking a declaration from EC3 and returns the enriching delta the composition root applies before `RunCarbon`.
- Auto: a per-module EC3 `ScopeSet` decodes onto the seam `MaterialPropertySet.Environmental.StageGwp` vector over the `LifecycleStage` band (A1A2A3/A4/A5/B/C/D) exactly as the aggregator reads a baked declaration, so an EC3-resolved material and a baked material fold identically; `valid_until < now` demotes a stale EPD to the category statistics fallback; the assessment stays a pure sync graph read because the async EC3 work is the explicit `EnrichCarbon` ingress, never a network call inside the fold.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet.Environmental`, `LifecycleStage`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, the `Ec3Service`, NodaTime, BCL inbox.
- Growth: a new lifecycle module is one seam `LifecycleStage` row (the `StageGwp` vector and the EC3 `ScopeSet` decode widen by data); a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner.
- Boundary: the primary GWP is the local `AggregateEnvironmental` over the graph's baked `Environmental` declarations — EC3 is the FALLBACK resolved by the async `EnrichCarbon` ingress (cached through `HybridCache`, applied as a `GraphDelta` before the sync `RunCarbon`), so a fully-declared model needs no network call and the assessment stays a pure sync graph read; the GWP stays raw kgCO2e (never `UnitsNet.Mass`); the `Assessment.Result` is content-keyed on the `(OMF query, LCIA method, route)` tuple; an absent declaration with no EC3 match rails `AssessmentInputMissing` rather than defaulting a sentinel carbon.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // The sync assessment — a pure graph read folding the aggregator over the baked (or EC3-enriched) Environmental.
    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Total: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<carbon-element-missing-composition>"))
                from lifecycle in AssemblyAggregator.AggregateEnvironmental(composition, Resolver(graph), Seq<PlyQuantity>(), graph.ElementAreaM2(id))
                select (Facts: state.Facts
                        .Add(AssessmentFact.Measure($"{id.Value}/whole-life-gwp", MeasureValue.Of(lifecycle.WholeLifeGwpKgCo2e, "kgCO2e")))
                        .Add(AssessmentFact.Measure($"{id.Value}/embodied-carbon-intensity", MeasureValue.Of(lifecycle.EmbodiedCarbonIntensityKgCo2eM2, "kgCO2e/m²")))
                        .Add(AssessmentFact.Ratio($"{id.Value}/recycled-content", lifecycle.RecycledContentFraction))
                        + StageFacts(id, lifecycle.StageGwp),
                    Total: state.Total + lifecycle.WholeLifeGwpKgCo2e)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts,
                request.Query.TargetKgCo2e > 0.0 ? state.Total / request.Query.TargetKgCo2e : 0.0,
                new Provenance("LifecycleAssessment", request.Route.Standard, "EC3/openEPD", clocks.Now), clocks.Now));

    // The async EC3 INGRESS — resolves each target material lacking a baked Environmental declaration from the EC3
    // service, decodes the per-module ScopeSet onto the seam StageGwp vector, and returns a GraphDelta enriching the
    // graph; the composition root applies it before the sync RunCarbon, so a fully-declared model needs no network call.
    public static async Task<Fin<GraphDelta>> EnrichCarbon(ElementGraph graph, Ec3Service ec3, AssessmentRequest.Carbon request, LciaMethod method) {
        GraphDelta delta = GraphDelta.Empty;
        foreach (NodeId mid in MissingDeclarations(graph, request.Targets)) {
            (await ec3.Search(request.Query.Omf, page: 1))
                .Bind(static epds => epds.HeadOrNone().ToFin((Error)new ComputeFault.AssessmentInputMissing("<ec3-no-epd>")))
                .IfSucc(epd => delta = delta.WithProperty(mid, ToEnvironmental(epd, method)));
        }
        return Fin.Succ(delta);
    }

    static Seq<NodeId> MissingDeclarations(ElementGraph graph, Seq<NodeId> targets) =>
        targets.Bind(graph.MaterialsOf).Distinct()
            .Filter(mid => graph.Material(mid).Map(static m => m.Properties).Map(static props => !props.Exists(static p => p is MaterialPropertySet.Environmental)).IfFail(false));

    static Seq<AssessmentFact> StageFacts(NodeId id, ReadOnlyMemory<double> stageGwp) =>
        LifecycleStage.Items.ToSeq().Map(stage => AssessmentFact.Measure($"{id.Value}/gwp-{stage.Code}", MeasureValue.Of(stageGwp.Span[stage.Index], "kgCO2e")));

    static MaterialPropertySet.Environmental ToEnvironmental(Epd epd, LciaMethod method) =>
        epd.Gwp(method).Match(
            Some: scope => MaterialPropertySet.Environmental.Of(scope.ToStageVector().AsMemory(), epd.Id ?? "", epd.ValidUntil),
            None: () => MaterialPropertySet.Environmental.Empty);
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup runner.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks)` — folds `AssemblyAggregator.AggregateCost` over each target's seam `MaterialComposition`, summing the per-ply supply/install/lifecycle cost over a single currency, and emits the `supply-total`/`install-total`/`in-place-total` facts.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet.Cost`, `Currency`, `NodeId`), the `Analysis/aggregator` `AssemblyAggregator`, BCL inbox.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost rail spans all composition cases (a single material or a profile member has a unit supply/install cost).
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the rollup is over a single `Currency` (a mismatch rails because the fold carries no exchange rate); the per-ply quantity rides the request or derives from the layer thickness × area; a material with no cost case rails `AssessmentInputMissing`.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), InPlace: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).MapFail(static _ => (Error)new ComputeFault.AssessmentInputMissing("<cost-element-missing-composition>"))
                from cost in AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), graph.ElementAreaM2(id))
                select (Facts: state.Facts
                        .Add(AssessmentFact.Measure($"{id.Value}/supply-total", MeasureValue.Of(cost.SupplyTotal, cost.Currency.Key)))
                        .Add(AssessmentFact.Measure($"{id.Value}/install-total", MeasureValue.Of(cost.InstallTotal, cost.Currency.Key)))
                        .Add(AssessmentFact.Measure($"{id.Value}/in-place-total", MeasureValue.Of(cost.TotalInPlace, cost.Currency.Key))),
                    InPlace: state.InPlace + cost.TotalInPlace)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts, 0.0,
                new Provenance("LifecycleAssessment", request.Route.Standard, "n/a", clocks.Now), clocks.Now));

    static Func<NodeId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) => id => graph.Material(id).Map(static m => m.Properties);
}
```

## [05]-[RESEARCH]

- [EC3_OPENEPD_READ]: the embodied-carbon boundary is the EC3 / openEPD REST service (`https://openepd.buildingtransparency.org/api`, bearer auth, token-metered, `429`/`Retry-After` rate-limited) consumed hand-thin over a typed `HttpClient` + a source-generated `System.Text.Json` context — `/v2/epds/search` (OMF → `Epd[]`, paged), `/v2/epds/statistics` (OMF → `StatisticsDto` percentile reference lines), `/epds/{uuid}` (single declaration). The decoder selects `Epd.impacts["EN 15978:2011"].gwp` then folds the `ScopeSet` modules (`A1A2A3`/`A4`/`A5`/`B`/`C`/`D`) onto the seam `MaterialPropertySet.Environmental.StageGwp` vector; the GWP `Measurement.mean` is kgCO2e per declared unit and stays a raw domain scalar, never `UnitsNet.Mass`. Only the GET read surface is consumed (Rasm is a carbon consumer, never a publisher).
- [CONTENT_KEY_CACHE]: EC3 responses cache in `HybridCache` keyed by `XxHash128(omf || method || page)` and the `Assessment.Result` is content-keyed on the `(OMF query, LCIA method, route)` tuple via the one canonical `XxHash128` seed, so an identical carbon query is a cache hit and a 412-noop on the object store and a token-metered endpoint is never re-hit; `meta.mf_hash` is only the OMF component of that key (route and page still participate). The resilience pipeline (the AppHost standard handler) honors `429`/`Retry-After` as the backoff floor — Compute owns the typed client, AppHost owns the resilience policy.
- [EN_15978_TAKEOFF]: the whole-building embodied-carbon takeoff folds `AssemblyAggregator.AggregateEnvironmental` over each element's seam `MaterialComposition` (the relocated engine owns the per-module `Σ(StageGwp_i · quantity_i)` fold), so a baked declaration and an EC3-resolved declaration fold identically onto the `LifecycleStage` band; the cradle-to-gate `A1A2A3` and the cradle-to-grave whole-life total are projections over the per-module vector, the `EmbodiedCarbonIntensity` the GWP per element area. The takeoff composes the aggregator, never re-deriving the per-module sum. Ripple counterpart: `Rasm.Materials` `Properties/sustainability` (RETIRE the aggregator half; KEEP the single-material `Environmental` authoring lowered onto the seam) and `Rasm.Element/Composition/material` (the seam `Environmental` case + `LifecycleStage` band).
- [COST_SCOPE]: the cost runner folds `AssemblyAggregator.AggregateCost` for the embodied supply/install/lifecycle material-cost rollup over a single currency only — construction SCHEDULING, CPM, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ via the `Planning` owner), so the Compute cost arm is the material takeoff and the Bim planning owner is the schedule, aligned by the seam graph never coupled. The cost discipline is bracketed in the campaign (`§1`): the runner is proportionate (the aggregator fold + fact emit), the depth reserved for carbon and the physical disciplines.
- [GRAPH_READ_ACCESSORS]: the runners read the concrete graph through the seam accessors `graph.CompositionOf(NodeId)→Fin<MaterialComposition>`, `graph.Material(NodeId)→Fin<Node.Material>` (whose `.Properties` is the aggregator resolver), and `graph.ElementAreaM2(NodeId)` (the element area for the per-m² intensity and the per-ply quantity); the OMF query, the LCIA method, and the carbon target ride the `AssessmentRequest.Carbon.Query`. Ripple counterpart: `Rasm.Element/Graph/element` (the `CompositionOf`/`Material`/`ElementAreaM2` graph accessors over the baked material subgraph).
