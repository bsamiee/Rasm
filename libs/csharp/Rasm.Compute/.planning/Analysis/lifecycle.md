# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner owns the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment rail — the EN 15978 embodied-carbon takeoff and the supply/install/lifecycle cost rollup. Each folds the `Analysis/aggregator` (`AggregateEnvironmental`/`AggregateCost`) over the seam `MaterialComposition` read directly from the concrete `Rasm.Element` `ElementGraph`, distributing each ply's per-module GWP and per-unit cost by the element's baked `Qto_*BaseQuantities` takeoff. Where a ply carries no baked EN 15978 declaration, the async `EnrichCarbon` ingress resolves one from the EC3 / openEPD REST service through a fallback ladder — the freshest valid product `Epd` in the ply's own category, then the category `StatisticsDto` conservative-estimate line — basis-tagged from the EPD `declared_unit` onto the seam `MaterialPropertySet.Environmental` and applied as a `GraphDelta` before the pure-sync `RunCarbon`, so a fully-declared model needs no network call.

A hand-thin EC3 client rides a typed `HttpClient` and a source-generated `System.Text.Json` context: only the GET read surface is exposed, every read returns `Fin<T>` (a non-2xx, decode failure, or missing `impacts[method].gwp` mints a `ComputeFault`, never an exception in domain flow), and a success payload caches in `HybridCache` keyed by `XxHash128(kind, omf, page|method)` so a token-metered endpoint is never re-hit — a transient fault is never cached. GWP stays a raw kgCO2e domain scalar — CO2-equivalence is a domain basis, not an SI dimension — carried as a dimensionless `MeasureValue` with the `kgCO2e` label, never forced through `UnitsNet.Mass` nor the abbreviation-resolving `MeasureValue.Of`. Each runner returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the governing ratio the whole-life carbon (or in-place cost) against a target. Construction SCHEDULING and 4D cost-loading stay in `Rasm.Bim` (MPXJ) — this is the embodied material takeoff only, the cost arm bracketed to the aggregator fold plus the fact emit.

## [01]-[INDEX]

- [01]-[EC3_BOUNDARY]: the `Ec3Service` read client, the openEPD wire types, the success-only content-key cache, and the raw-kgCO2e GWP discipline.
- [02]-[CARBON_RUNNER]: `RunCarbon` the pure-sync EN 15978 takeoff and `EnrichCarbon` the async EC3 ingress resolving each undeclared ply through the fallback ladder.
- [03]-[COST_RUNNER]: `RunCost` the supply/install/lifecycle rollup over the composition, guarded to the requested `Currency`.

## [02]-[EC3_BOUNDARY]

- Owner: `Ec3Service` the typed-`HttpClient` embodied-carbon read client; the openEPD wire-type family (`Epd`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`); the success-only `XxHash128` content-key cache; the `LciaMethod` `[SmartEnum<string>]` impact-method selector; the `CarbonQuery` request input the `AssessmentRequest.Carbon` case carries.
- Entry: `Statistics` · `Search` · `ByUuid` expose GET-only reads returning `Fin<T>`. `408`/`429`/`5xx` responses classify as transient `FailureKind.Timeout`; deterministic client responses classify as `FailureKind.Input`; transport and cancellation exceptions lower onto typed endpoint/timeout faults.
- Auto: the three reads share ONE polymorphic `Cached<T>` fold parameterized by the decode shape (`Unwrap<T>` for the `{payload, meta}` envelope, `Bare<T>` for the by-UUID document) — no parallel `GetEnvelope`/`GetBare` pair; the cache stores the SUCCESS DTO ONLY (`Epd[]`/`StatisticsDto`/`Epd`, never a `Fin` or a `Seq`), the factory throwing the boundary fault so `HybridCache.GetOrCreateAsync` writes nothing on a failure and a transient `429`/`5xx` never poisons a content-key; the cache slot is `XxHash128.HashToUInt128` over the `(kind, omf, page|method)` string, every entry held under one `HybridCacheEntryOptions` policy (a days-scale distributed `Expiration` matching the provider's EPD revision cadence, an hour-scale `LocalCacheExpiration` re-validating L1 across redeploys) and tagged `ec3` + `ec3:<kind>` so a category recall is one tag eviction; the AppHost-owned resilience handler honors `429` + `Retry-After` as the backoff floor.
- Packages: `System.Net.Http` (typed client + `ReadFromJsonAsync(JsonSerializerContext)`), `System.Text.Json` (source-generated context, AOT-safe), `System.IO.Hashing` (`XxHash128.HashToUInt128`), Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync` stateful overload), LanguageExt.Core, NodaTime (`Instant`), BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row; a new decoded openEPD member is one source-gen context property; a new read endpoint is one method composing the same `Cached<T>` fold — the client widens by row/method, never a second HTTP client and never a per-endpoint cache path.
- Boundary: only the GET read surface is consumed (Rasm is a carbon consumer, never a publisher). GWP `Measurement.Mean` is kgCO2e per declared unit and is not a `UnitsNet` quantity — it crosses interior signatures as a raw `double` and lands as a dimensionless `MeasureValue` labeled `kgCO2e` through `DomainMeasure`, never `UnitsNet.Mass` and never the abbreviation-resolving `MeasureValue.Of` (which rejects `kgCO2e`). Ingress `Normalize`s the per-module vector to per-one-unit of its native basis and tags that `MeasurementBasis` the `AggregateEnvironmental` fold scales by — a volume `declared_unit` → PerM3, an area → PerM2, a mass → PerKg, else the `kg_per_declared_unit` → PerKg chain; density is not read at ingress (a per-kg basis resolves to mass at aggregation against the ply `Mechanical.Density`), and an unresolvable bare-count declaration is railed and the ply skipped. Hyphenated LCIA scope keys (`A1A2A3`, `B1`…`B7`, `C1`…`C4`) require `[JsonPropertyName]` aliases; the `fields` query mask trims the response to the decoded projection so the token cost stays minimal; a failed read is the explicit `Fin.Fail` the caller surfaces, never a cached failure re-served as success.

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
// Carbon request input carries category OMF as default scope, an
// optional per-material OMF override (a multi-material assembly resolves each ply from its OWN EC3 category — concrete,
// insulation, and gypsum never share one EPD), the LCIA method, and the design target the verdict ratios against.
public sealed record CarbonQuery(string Omf, Map<string, string> OmfByMaterial, LciaMethod Method, double TargetKgCo2e) {
    public static CarbonQuery Of(string omf, LciaMethod method, double target) => new(omf, Map<string, string>(), method, target);
}

// Source-generated System.Text.Json wire projection admits only consumed members; the `fields` query mask trims the
// rest server-side. The search/statistics reads wrap the result in { payload, meta }; the decoder reads `payload` only,
// Unconsumed `meta` object stays skipped because the runner owns no pager or warning surface.
public sealed record Envelope<T>(T Payload);

public sealed record Measurement(double Mean);

// openEPD unit-bearing quantity carries declared_unit or kg_per_declared_unit as a magnitude plus UnitsNet-resolvable unit
// abbreviation the basis normalization coerces to SI once. The qty/unit keys decode under the context camelCase policy
// (no alias); the uncertainty-free Amount carries no rsd. This is the field the per-declared-unit -> per-m³ chain reads.
public sealed record Amount(double? Qty, string? Unit);

// EN 15978 life-cycle modules band onto the seam LifecycleStage 6-vector. ToStageVector sums the
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
    // Per-module carbon reads impacts[method]["gwp"] and returns None when either key is absent.
    public Option<ScopeSet> Gwp(LciaMethod method) =>
        Impacts.TryGetValue(method.Key, out Dictionary<string, ScopeSet> set) && set.TryGetValue("gwp", out ScopeSet gwp) ? Some(gwp) : None;
}

// Category-scoped GWP substitution line carries EC3 conservative_estimate (80th-percentile) in kgCO2e per declared unit
// is the generic value a ply with no fresh product EPD falls back to; declared_unit carries the basis the per-m³
// normalization reads (a category statistic carries no kg_per_declared_unit — a mass-based category needs the ply
// density). The response's other percentile lines are ignored.
public sealed record StatisticsDto(
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class Ec3Service(HttpClient http, HybridCache cache, JsonSerializerContext json) {
    // Freshest-EPD search reads one page wide: a single token charge surfaces enough
    // category candidates for Freshest to pick the latest valid_until, never a per-ply multi-page crawl.
    const int SearchPageSize = 100;

    // LCIA method is behavior-bearing on the wire: lcia_method selects
    // which method's statistics line the service computes, so the cache identity and the remote request agree and
    // Fallback therefore cannot label one method's conservative estimate as another's GWP.
    public Task<Fin<StatisticsDto>> Statistics(string omf, LciaMethod method) =>
        Cached<StatisticsDto>($"stat:{omf}:{method.Key}", $"/v2/epds/statistics?omf={Uri.EscapeDataString(omf)}&lcia_method={Uri.EscapeDataString(method.Key)}", Unwrap<StatisticsDto>);

    public async Task<Fin<Seq<Epd>>> Search(string omf, int page) =>
        (await Cached<Epd[]>($"search:{omf}:{page}",
            $"/v2/epds/search?omf={Uri.EscapeDataString(omf)}&page_number={page}&page_size={SearchPageSize}&fields=id,valid_until,declared_unit,kg_per_declared_unit,impacts", Unwrap<Epd[]>))
            .Map(static e => e.ToSeq());

    public Task<Fin<Epd>> ByUuid(string uuid) =>
        Cached<Epd>($"epd:{uuid}", $"/epds/{Uri.EscapeDataString(uuid)}?fields=id,declared_unit,kg_per_declared_unit,impacts", Bare<Epd>);

    // Entry lifetime follows EPD revision cadence rather than session duration, so the
    // distributed entry holds for days while the in-process L1 re-validates hourly against redeploys; the kind-and-omf
    // tags make a category recall (`ec3`, `ec3:search`, …) one tag eviction, never a key enumeration.
    static readonly HybridCacheEntryOptions CacheLife = new() { Expiration = TimeSpan.FromDays(14), LocalCacheExpiration = TimeSpan.FromHours(1) };

    // One polymorphic fetch+cache fold over the decode shape (no parallel GetEnvelope/GetBare). Cache a success only: the
    // factory throws the boundary fault on a non-2xx or decode miss, so HybridCache writes nothing on failure and a
    // transient 429/5xx never poisons a content-key; the cached value is the source-gen DTO, held under the CacheLife
    // policy and the (ec3, ec3:<kind>) tag pair. Exemption: the HybridCache + HTTP boundary — the throw converts back
    // onto the Fin rail exactly once at this seam.
    async Task<Fin<T>> Cached<T>(string key, string path, Func<HttpContent, JsonSerializerContext, ValueTask<Option<T>>> decode) where T : notnull {
        string slot = XxHash128.HashToUInt128(MemoryMarshal.AsBytes(key.AsSpan())).ToString();
        try {
            return Fin.Succ(await cache.GetOrCreateAsync(slot, (http, json, path, decode),
                static async (state, ct) => {
                    using HttpResponseMessage response = await state.http.GetAsync(state.path, ct);
                    return !response.IsSuccessStatusCode
                        ? throw new Ec3Boundary(HttpFailure(state.path, response.StatusCode))
                        : (await state.decode(response.Content, state.json)).IfNone(() => throw new Ec3Boundary(new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Foreign, $"<ec3-decode:{state.path}>")));
                },
                CacheLife,
                tags: ["ec3", $"ec3:{key[..key.IndexOf(':')]}"]));
        }
        catch (Ec3Boundary boundary) { return Fin.Fail<T>(boundary.Fault); }
        catch (HttpRequestException ex) { return Fin.Fail<T>(new ComputeFault.EndpointUnreachable($"<ec3-transport:{ex.HttpRequestError}:{ex.Message}>")); }
        catch (TaskCanceledException ex) { return Fin.Fail<T>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Timeout, $"<ec3-timeout:{ex.Message}>")); }
    }

    static ComputeFault.AnalysisFailed HttpFailure(string path, HttpStatusCode status) =>
        new(SolvePhase.Admission,
            status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status >= 500 ? FailureKind.Timeout : FailureKind.Input,
            $"<ec3:{path}>", Some((int)status));

    // Source-generated decode rides ReadFromJsonAsync(Type, JsonSerializerContext), because the generic
    // ReadFromJsonAsync<T> only binds JsonSerializerOptions/JsonTypeInfo<T>/CancellationToken, never a JsonSerializerContext,
    // so the closed Envelope<Epd[]>/Envelope<StatisticsDto>/Epd the context registers resolve through the Type form.
    static async ValueTask<Option<T>> Unwrap<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional(((Envelope<T>?)await content.ReadFromJsonAsync(typeof(Envelope<T>), json))?.Payload);

    static async ValueTask<Option<T>> Bare<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((T?)await content.ReadFromJsonAsync(typeof(T), json));

    // Boundary-crossing carrier lifts ComputeFault across the HybridCache factory throw and converts back to
    // Fin.Fail at the one catch so a failed read never caches and never escapes as a raw exception.
    sealed class Ec3Boundary(Error fault) : Exception { public Error Fault { get; } = fault; }
}
```

## [03]-[CARBON_RUNNER]

- Owner: `LifecycleAssessment.RunCarbon` the pure-sync EN 15978 embodied-carbon assessment (a graph read, no network); `LifecycleAssessment.EnrichCarbon` the async EC3 ingress that decodes EC3 declarations onto the seam `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; the Compute-owned `LifecycleGraphReads.TakeoffOf` base-quantity read; the `CarbonQuery` request input.
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks)` folds `AssemblyAggregator.AggregateEnvironmental` over each target's `MaterialComposition` and baked `TakeoffOf`; `EnrichCarbon` resolves undeclared plies through product search then category statistics and returns a typed `GraphDelta` rail.
- Auto: `RunCarbon` resolves each ply's seam properties through one `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` resolver keyed on the composition's native `MaterialId` (never a graph `NodeId`), and the per-element area + volume through `TakeoffOf`, so a baked and an EC3-resolved declaration fold identically. `EnrichCarbon` enumerates the undeclared ply materials (the `MaterialId` set lacking the `Environmental` case, not the element's directly-associated material), resolves each from EC3 through the fallback ladder, `Normalize`s the `ScopeSet` to per-one-unit of its native basis and tags that `MeasurementBasis`, embeds the carbon-only per-stage GwpTotal row into the full seam `(ImpactCategory × LifecycleStage)` matrix through `CarbonMatrix` (un-declared indicator rows zeroed, the partial-EPD invariant), and accumulates one monoid `GraphDelta` the composition root applies (an unresolvable-basis ply is skipped, not mis-scaled). Assessment stays a pure-sync graph read because every network call lives in the explicit `EnrichCarbon` ingress, never inside the fold.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet`/`OfEnvironmental`/`PropertyEvidence`, `MaterialPropertyAccess.Environmental`, `ImpactCategory`/`LifecycleStage`, `MeasurementBasis`, `MaterialId`, `NodeId`, `Node.Material`/`Node.QuantitySet`, `Relationship.Assign`/`AssignKind`, `GraphDelta.Put`, `MeasureValue.Of`/`MeasureValue.Si`, `Dimension.VolumeDim`/`Dimension.AreaDim`/`Dimension.MassDim`, `Provenance`), UnitsNet (via `MeasureValue.Of` — the `declared_unit` abbreviation -> SI dimension/scalar coercion the basis tagging rides), Rasm (kernel `Op`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, the `Ec3Service`, NodaTime (`Instant`), BCL inbox (`ImmutableArray<double>` the seam impact-matrix store the ingress builds).
- Growth: a new lifecycle module is one seam `LifecycleStage` row (the `StageGwp` vector, the `ScopeSet` banding, and the aggregator fold widen by data); a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner; a richer EC3 selection (lowest-GWP, spec-matched) is one refinement of `Freshest`.
- Boundary: the PRIMARY GWP is the local `AggregateEnvironmental` over each ply's baked `Environmental` — EC3 is the FALLBACK the async `EnrichCarbon` resolves, cached through `HybridCache`, applied as a `GraphDelta` before the sync `RunCarbon`, so a fully-declared model needs no network call; the takeoff reads the baked `Qto_*BaseQuantities` (`TakeoffOf`) so a target with no base quantity rails `AssessmentInputMissing` rather than a silent zero takeoff; the GWP/intensity stay raw kgCO2e through `DomainMeasure` (dimensionless `MeasureValue` + label), never `UnitsNet.Mass`; `EnrichCarbon` splits failure by kind — a DETERMINISTIC data absence (no fresh EPD, a `declared_unit` with no resolvable dimension such as a bare-count declaration lacking `kg_per_declared_unit`, a missing method GWP) skips the ply so `RunCarbon` rails the still-undeclared ply at its own fold, never defaulting a sentinel carbon or admitting a mis-scaled figure (a per-m² or per-kg EPD folds correctly under its tagged `MeasurementBasis` rather than being dropped), while a TRANSPORT/timeout fault aborts the enrichment rail (a partial delta would mask the outage a retry could still resolve); the runner reads the CONCRETE graph (above the seam), the write-back the `Analysis/assessment` spine's content-keyed `Node.Assessment`.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // Pure synchronous assessment folds the aggregator over each target's baked or EC3-enriched
    // composition + base-quantity takeoff; the governing ratio is the whole-life carbon against the design target, or
    // double.NaN -> NotApplicable with no target (never a misleading 0.0-ratio Satisfied) — the energy-runner convention.
    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), Total: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<carbon-element-missing-composition:{id.Value}>"))
                from geometry in graph.TakeoffOf(id)
                from lifecycle in AssemblyAggregator.AggregateEnvironmental(composition, Resolver(graph), Seq<PlyQuantity>(), geometry)
                from whole in DomainMeasure($"{id.Value}/whole-life-gwp", lifecycle.WholeLifeGwpKgCo2e, "kgCO2e")
                from intensity in DomainMeasure($"{id.Value}/embodied-carbon-intensity", lifecycle.EmbodiedCarbonIntensityKgCo2eM2, "kgCO2e/m²")
                from recycled in AssessmentFact.Ratio($"{id.Value}/recycled-content", lifecycle.RecycledContentFraction)
                from stages in StageFacts(id, lifecycle.StageGwp)
                select (Facts: state.Facts.Add(whole).Add(intensity).Add(recycled) + stages,
                    Total: state.Total + lifecycle.WholeLifeGwpKgCo2e)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts,
                request.Query.TargetKgCo2e > 0.0 ? state.Total / request.Query.TargetKgCo2e : double.NaN,
                new Provenance("LifecycleAssessment", request.Route.Standard, request.Route.SolverVersion, clocks.Now)));

    // Async EC3 ingress resolves each undeclared ply through product EPD then category-statistic fallback,
    // decodes the ScopeSet to the carbon GwpTotal row in the seam matrix, and accumulates the enriching delta the
    // composition root applies before the sync RunCarbon, so a fully-declared model needs no network call.
    public static async Task<Fin<GraphDelta>> EnrichCarbon(ElementGraph graph, Ec3Service ec3, AssessmentRequest.Carbon request, ClockPolicy clocks, Op key) {
        Fin<GraphDelta> delta = Fin.Succ(GraphDelta.Empty);
        // EC3 ingress boundary: a serial, rate-limited fetch over the token-metered endpoint, each resolved ply accumulated
        // onto the monoid delta. Failure splits by kind: DETERMINISTIC data absence (AssessmentInputMissing — no fresh EPD,
        // an unresolvable declared-unit basis, a missing method GWP) SKIPS the ply, because RunCarbon then rails the
        // still-undeclared ply with its own precise fault at the fold — the right surfacing point — while a mis-scaled
        // default would be silent; a TRANSPORT/timeout fault ABORTS the rail, because a partial delta would erase the
        // outage and mask the plies a retry could still resolve.
        foreach (Node.Material material in MissingDeclarations(graph, request.Targets)) {
            Fin<MaterialPropertySet> resolved = await Resolve(ec3, request.Query, material, clocks.Now, key);
            delta = resolved.Match(
                Succ: environmental => delta.Map(current => current.Put(material with { Properties = material.Properties.Add(environmental) })),
                Fail: error => error is ComputeFault.AssessmentInputMissing ? delta : delta.Bind(_ => Fin.Fail<GraphDelta>(error)));
        }
        return delta;
    }

    // Undeclared ply materials derive from each target composition's native MaterialId set, resolved to its
    // material node, filtered to those lacking the seam Environmental case — the plies the aggregator folds, NOT the
    // element's directly-associated container material (a LayerSet's plies, not the layer-set node).
    static Seq<Node.Material> MissingDeclarations(ElementGraph graph, Seq<NodeId> targets) =>
        targets.Choose(graph.CompositionOf)
            .Bind(static c => c.Materials)
            .Distinct()
            .Choose(mid => graph.Material(mid))
            .Filter(static m => m.Properties.Environmental.IsNone)
            .Distinct();

    // Seam-keyed resolver maps a ply MaterialId to its material node's property set, railing the
    // missing-input fault on an absent material so the aggregator reads the composition's OWN plies by native key.
    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<material-absent:{mid.Value}>"));

    // Per-ply fallback chooses the freshest valid product EPD in its category, else the category
    // conservative-estimate line; a per-material OMF override resolves a multi-material assembly. Each rung tags the EPD's
    // native MeasurementBasis without a density — a per-kg basis resolves to mass at aggregation, so no density is needed here.
    static async Task<Fin<MaterialPropertySet>> Resolve(Ec3Service ec3, CarbonQuery query, Node.Material material, Instant now, Op key) {
        string omf = query.OmfByMaterial.Find(material.MaterialKey.Value).IfNone(query.Omf);
        Fin<Seq<Epd>> searched = await ec3.Search(omf, page: 1);
        return await searched.Match(
            Succ: epds => Freshest(epds, query.Method, now).Match(
                Some: epd => Task.FromResult(ToEnvironmental(epd, query.Method, key)),
                None: () => Fallback(ec3, omf, query.Method, key)),
            Fail: _ => Fallback(ec3, omf, query.Method, key));
    }

    static async Task<Fin<MaterialPropertySet>> Fallback(Ec3Service ec3, string omf, LciaMethod method, Op key) =>
        (await ec3.Statistics(omf, method)).Bind(stats => FromStatistics(stats, key));

    // Freshest non-stale EPD carries method GWP; null valid_until is non-expiring, while dated entries require valid_until >= now.
    // Latest expiry wins within the category.
    static Option<Epd> Freshest(Seq<Epd> epds, LciaMethod method, Instant now) =>
        epds.Filter(e => e.Gwp(method).IsSome && (e.ValidUntil is not { } valid || valid >= now))
            .OrderByDescending(static e => e.ValidUntil ?? Instant.MaxValue)
            .ToSeq().Head;

    // Normalize the ScopeSet to per-one-unit of its native basis (tagged with the MeasurementBasis the fold scales by,
    // DeclaredQuantity remains shared with cost before carbon GwpTotal embeds into the full seam
    // (ImpactCategory × LifecycleStage) matrix through CarbonMatrix and admit it through OfEnvironmental — the un-declared
    // indicator rows stay zero (the partial-EPD invariant). An unresolvable basis rails None so EnrichCarbon skips the ply.
    // OfEnvironmental takes the matrix, no gwpKgCo2e param and no GlobalWarmingPotential field; the carbon rides the
    // GwpTotal-row cells (Gwp => IndicatorAt(GwpTotal, A1A3) is the seam's derived cradle-to-gate read). The EPD id +
    // LocalDate expiry ride the PropertyEvidence arg (the Instant lowers via InUtc().Date, never a coarse int year).
    static Fin<MaterialPropertySet> ToEnvironmental(Epd epd, LciaMethod method, Op key) =>
        epd.Gwp(method).Match(
            Some: scope => Normalize(scope.ToStageVector(), epd.DeclaredUnit, epd.KgPerDeclaredUnit, key).Match(
                Some: norm => MaterialPropertySet.OfEnvironmental(
                    norm.Basis, MaterialPropertySet.Environmental.CarbonMatrix(norm.PerUnit), recycledContent: 0.0, endOfLifeRecovery: 0.0,
                    key, new PropertyEvidence("epd", epd.Id ?? "", Optional(epd.ValidUntil).Map(static v => v.InUtc().Date))),
                None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<ec3-epd-basis-unresolved:{epd.Id}>"))),
            None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<ec3-epd-missing-gwp:{epd.Id}>")));

    // Category conservative estimate (EC3 80th-percentile substitution value) lands on A1-A3,
    // A1-A3 is the only stage a category statistic resolves; downstream modules stay zero until a product EPD supplies them. This
    // category declared_unit drives the SAME basis Normalize (no kg_per_declared_unit at category scope, so a mass/area/
    // volume declared_unit tags PerKg/PerM2/PerM3 the aggregator scales by — a bare-count category with no unit rails).
    static Fin<MaterialPropertySet> FromStatistics(StatisticsDto stats, Op key) {
        double[] product = new double[LifecycleStage.Count];
        product[LifecycleStage.A1A3.Index] = stats.ConservativeEstimate;
        return Normalize(product, stats.DeclaredUnit, null, key).Match(
            Some: norm => MaterialPropertySet.OfEnvironmental(norm.Basis, MaterialPropertySet.Environmental.CarbonMatrix(norm.PerUnit),
                recycledContent: 0.0, endOfLifeRecovery: 0.0, key, new PropertyEvidence("ec3-statistics", "conservative", None)),
            None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing("<ec3-statistics-basis-unresolved>")));
    }

    // Normalize a per-declared-unit StageGwp vector to per-one-unit of its native basis and tag the MeasurementBasis the
    // fold scales by — the strongest-dimension route winning through the Option `|` choice: a volume declared_unit ->
    // PerM3, an area -> PerM2, a mass -> PerKg, else the kg_per_declared_unit chain -> PerKg / kg-per-unit. Density is not
    // read here — a per-kg basis resolves to mass at aggregation (volume × the ply Mechanical.Density). None whenever no
    // basis evidence resolves — a MISSING declared_unit included: only the kg_per_declared_unit chain can still ground it,
    // and absent both the ply is skipped (a defaulted PerM3 over an unknown basis mis-scales the EPD as volume material —
    // Unknown basis stays distinguishable from per-m³, deleting the prior fabricated default.
    static Option<(MeasurementBasis Basis, double[] PerUnit)> Normalize(double[] perDeclaredUnit, Amount? declaredUnit, Amount? kgPerDeclaredUnit, Op key) {
        if (declaredUnit is not { } unit) {
            return kgPerDeclaredUnit is { } bare && MeasureValue.Of(bare.Qty ?? 0.0, bare.Unit ?? "", key).ToOption().Map(static m => m.Si).Filter(static kg => kg > 0.0) is { IsSome: true, Case: double kgOnly }
                ? Some((MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / kgOnly)))
                : None;
        }
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

    // Carbon-only per-stage GwpTotal embeds into the full seam matrix through CarbonMatrix, the write dual of
    // Environmental.StageGwp), every un-declared indicator row zeroed (the partial-EPD invariant), so the ingress never
    // re-spells the offset arithmetic the seam owns; a full EN 15804+A2 method passes its matrix to OfEnvironmental directly.
    static Fin<Seq<AssessmentFact>> StageFacts(NodeId id, ImmutableArray<double> stageGwp) =>
        LifecycleStage.Items.ToSeq().TraverseM(stage => DomainMeasure($"{id.Value}/gwp-{stage.Module}", stageGwp[stage.Index], "kgCO2e")).As();

    // GWP and in-place cost are domain-basis scalars (kgCO2e, kgCO2e/m², a currency code), not UnitsNet quantities — a
    // dimensionless MeasureValue carrying the domain label, never the abbreviation-resolving MeasureValue.Of (which
    // rejects kgCO2e). The mint is the seam's labeled registry-less OfSi (the record ctor is private): the Scalar
    // QuantityType keeps the value dimensionless while the label rides CanonicalUnit, Fin so a NaN rails at the finite gate.
    static Fin<AssessmentFact> DomainMeasure(string name, double si, string unit) =>
        MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, si, unit).Map(value => AssessmentFact.Measure(name, value));
}

// Element geometric takeoff distributes GWP and cost per ply through a Compute-owned ElementGraph extension reading
// baked Qto_*BaseQuantities, preferring net over gross and scanning every Qto set so wall/slab/beam reads without a
// per-type accessor. A target with no baked base quantity rails the missing input rather than a silent zero takeoff.
public static class LifecycleGraphReads {
    const string NetVolume = "NetVolume";
    const string GrossVolume = "GrossVolume";
    const string NetSideArea = "NetSideArea";
    const string NetArea = "NetArea";
    const string GrossSideArea = "GrossSideArea";

    extension(ElementGraph graph) {
        public Fin<ElementQuantity> TakeoffOf(NodeId element) {
            Seq<Node.QuantitySet> bags = graph.EdgesAt(element)
                .Choose(e => e is Relationship.Assign { SubKind: AssignKind.PropertyDefinition } a && a.Subject == element
                    ? graph.Find<Node.QuantitySet>(a.Definition) : None);
            Option<double> volume = Named(bags, NetVolume) | Named(bags, GrossVolume);
            Option<double> area = Named(bags, NetSideArea) | Named(bags, NetArea) | Named(bags, GrossSideArea);
            return volume.IsNone && area.IsNone
                ? Fin.Fail<ElementQuantity>((Error)new ComputeFault.AssessmentInputMissing($"<element-base-quantities-absent:{element.Value}>"))
                // Fabrication NestYield.WasteAreaMm2 seam quantity contributes when the graph carries a nest-yield
                // row for this element — joins as the decode-side WasteAreaM2 column, so off-cut waste rolls
                // into the same AggregateEnvironmental/AggregateCost folds (the circulation ingress row).
                : Fin.Succ(new ElementQuantity(area.IfNone(0.0), volume.IfNone(0.0), NestWasteM2(bags)));
        }
    }

    static Option<double> Named(Seq<Node.QuantitySet> bags, string name) =>
        bags.Choose(qs => qs.Bag.Values.Find(PropertyName.Create(name))).Head.Map(static m => m.Si);

    // Fabrication nest-yield decode reads the seam-baked waste-area quantity (NestYield.WasteAreaMm2 lowered
    // onto the element's quantity bag by the Fabrication projector) read as SI m² — absent rows contribute 0.
    static double NestWasteM2(Seq<Node.QuantitySet> bags) => Named(bags, "NestWasteArea").IfNone(0.0);
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup runner.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks)` folds `AssemblyAggregator.AggregateCost` over each target's `MaterialComposition` and baked `TakeoffOf`, guards currency, and emits `supply-total`/`install-total`/`in-place-total` facts.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet.Cost`, `Currency`, `MaterialId`, `NodeId`, `MeasureValue`, `Dimension`, `Provenance`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, the Compute-owned `TakeoffOf`, BCL inbox.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost rail spans all composition cases (a single material or a profile member has a unit supply/install cost); the cost arm is bracketed (`§1`) — the runner stays proportionate (the aggregator fold + the fact emit), the depth reserved for carbon and the physical disciplines.
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the `request.Currency` is load-bearing — the aggregated cost is guarded to it (a material priced in a different `Currency` rails, since the fold carries no exchange rate), so the request currency is a real validation target, never a decorative field; the per-ply quantity derives from the seam `Cost.Basis` against the baked `TakeoffOf` (or a `PlyQuantity` override); a material with no `Cost` case rails `AssessmentInputMissing`; the bracketed rollup carries no acceptance budget, so the governing ratio is `double.NaN` → `NotApplicable` (the informational rating, never a `0.0`-ratio `Satisfied` falsely asserting a budget pass) — the same no-target convention the energy and carbon runners hold.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, ClockPolicy clocks) =>
        request.Targets.Fold(
            Fin.Succ(Seq<AssessmentFact>()),
            (acc, id) => acc.Bind(facts =>
                from composition in graph.CompositionOf(id).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<cost-element-missing-composition:{id.Value}>"))
                from geometry in graph.TakeoffOf(id)
                from cost in AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), geometry)
                from _ in cost.Currency.Key == request.Currency
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>((Error)new ComputeFault.AssessmentInputMissing($"<cost-currency-mismatch:{cost.Currency.Key}<>{request.Currency}>"))
                from supply in DomainMeasure($"{id.Value}/supply-total", cost.SupplyTotal, cost.Currency.Key)
                from install in DomainMeasure($"{id.Value}/install-total", cost.InstallTotal, cost.Currency.Key)
                from inPlace in DomainMeasure($"{id.Value}/in-place-total", cost.TotalInPlace, cost.Currency.Key)
                select facts.Add(supply).Add(install).Add(inPlace)))
            .Map(facts => AssessmentResult.Of(request.Route, facts, double.NaN,
                new Provenance("LifecycleAssessment", request.Route.Standard, request.Route.SolverVersion, clocks.Now)));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
