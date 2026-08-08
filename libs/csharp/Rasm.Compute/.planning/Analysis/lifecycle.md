# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner owns the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment rail — the EN 15978 embodied-carbon takeoff and the supply/install/lifecycle cost rollup. Each folds the `Analysis/aggregator` (`AggregateEnvironmental`/`AggregateCost`) over the seam `MaterialComposition`, distributing each ply's per-module GWP and per-unit cost by the element's baked `Qto_*BaseQuantities` takeoff; where a ply carries no baked EN 15978 declaration, the async `EnrichCarbon` ingress resolves one from the EC3 / openEPD REST service through the fallback ladder, applied as a `GraphDelta` before the pure-sync `RunCarbon`.

One hand-thin EC3 client rides a typed `HttpClient` under the success-only content-key cache, and each runner returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the governing ratio the whole-life carbon (or in-place cost) against a target.

## [01]-[INDEX]

- [02]-[EC3_BOUNDARY]: `EpdQuery`→`EpdDeclaration` the resolver contract and `Ec3Service` the openEPD adapter satisfying it under a success-only content-key cache and the raw-kgCO2e GWP discipline.
- [03]-[CARBON_RUNNER]: `RunCarbon` the pure-sync EN 15978 takeoff and `EnrichCarbon` the async EC3 ingress resolving each undeclared ply through the fallback ladder.
- [04]-[COST_RUNNER]: `RunCost` the supply/install/lifecycle rollup over the composition, guarded to the requested `Currency`.

## [02]-[EC3_BOUNDARY]

- Owner: `EpdQuery` the closed request `[Union]` (`Products`/`Document`/`Generic`) and `EpdDeclaration`/`DeclaredAmount` the provider-neutral answer, together the `Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>>` resolver contract every carbon fold takes; `Ec3Service` the openEPD ADAPTER satisfying it; `Ec3Wire` the Mapperly wire→neutral mapper (the generated `Candidate` partial under `RequiredMappingStrategy.Both` beside the `[UserMapping]` `Substitution` fan-out) with `EpdCodec` its `[NamedMapping]` converter roster registered whole through `[UseStaticMapper]`; the openEPD wire-type family (`Epd`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`/`Amount`); the success-only `XxHash128` content-key cache; the `LciaMethod` `[SmartEnum<string>]` impact-method selector with its citation `Key` and wire `WireKey` columns; the `CarbonQuery` request input the `AssessmentRequest.Carbon` case carries.
- Entry: `Ec3Service.Resolve(EpdQuery query)` → `Task<Fin<Seq<EpdDeclaration>>>` is the adapter's ONE read, its generated total `Switch` binding the category page search, the by-identity document, and the category statistic onto three GET-only legs. `408`/`429`/`5xx` responses classify as transient `FailureKind.Timeout`; deterministic client responses classify as `FailureKind.Input`; transport and cancellation exceptions lower onto typed endpoint/timeout faults.
- Auto: the three legs share ONE polymorphic `Cached<T>` fold parameterized by the decode shape (`Unwrap<T>` for the `{payload, meta}` envelope, `Bare<T>` for the by-identity document) — no parallel `GetEnvelope`/`GetBare` pair; the cache stores the SUCCESS DTO ONLY (`Epd[]`/`StatisticsDto`/`Epd`, never a `Fin` or a `Seq`), the factory throwing the boundary fault so `HybridCache.GetOrCreateAsync` writes nothing on a failure and a transient `429`/`5xx` never poisons a content-key; the cache slot is `XxHash128.HashToUInt128` over the `(kind, omf, page|method|uuid)` string, every entry held under one `HybridCacheEntryOptions` policy (a days-scale distributed `Expiration` matching the provider's EPD revision cadence, an hour-scale `LocalCacheExpiration` re-validating L1 across redeploys) and tagged `ec3` + `ec3:<kind>` so a category recall is one tag eviction; the AppHost-owned resilience handler honors `429` + `Retry-After` as the backoff floor. Every module a declaration carries bands onto the seam `LifecycleStage` roster through ONE generated projection keyed by stage row, so the wire's fifteen `[JsonPropertyName]` members map by data rather than by a hand-summed fixed-slot literal.
- Packages: `System.Net.Http` (typed client + `ReadFromJsonAsync(Type, JsonSerializerContext)`), `System.Text.Json` (source-generated context, AOT-safe), `System.IO.Hashing` (`XxHash128.HashToUInt128`), Microsoft.Extensions.Caching.Hybrid (`HybridCache.GetOrCreateAsync` stateful overload), Riok.Mapperly (`[Mapper]`, `[MapProperty(Use = …)]`, `[MapValue]`, `[UserMapping]`, `[NamedMapping]`, `[UseStaticMapper]`, `[MapperIgnoreSource]` — the reader-free wire lowering), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime (`Instant`), Rasm.Element (project — `LifecycleStage` the banding projection is generated over), BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row carrying its citation and wire spellings; a new decoded openEPD member is one source-gen context property and one banding entry; a new lifecycle module is one seam `LifecycleStage` row with one banding entry here; a SECOND carbon provider is one type satisfying the resolver contract with zero edit to the folds — the boundary widens by row and by adapter, never by a second HTTP client and never a per-endpoint cache path.
- Boundary: the carbon folds take the RESOLVER, never this class — an assessment that names its provider cannot be run against a second catalogue, a fixture, or a cached corpus without editing the fold, and the concrete client is exactly what a test then has to fake through HTTP. Only the GET read surface is consumed (Rasm is a carbon consumer, never a publisher), and the openEPD wire family stays adapter-local: `Epd`/`ScopeSet`/`Amount` never cross the resolver contract, `EpdDeclaration` carrying the per-stage vector, the two basis witnesses, the expiry, and the citation the folds read. GWP `Measurement.Mean` is kgCO2e per declared unit and is not a `UnitsNet` quantity — it crosses interior signatures as a raw `double` and lands as a dimensionless `MeasureValue` labeled `kgCO2e` through `DomainMeasure`, never `UnitsNet.Mass` and never the abbreviation-resolving `MeasureValue.Of` (which rejects `kgCO2e`). `LciaMethod` carries its wire spelling as its OWN column (the `Model/providers#EP_AXIS` `WireKey` precedent): the citation a report renders and the token `impacts[method]` and `lcia_method=` are keyed by are two facts, and one string serving both makes a renamed citation silently miss every impact lookup. `LciaMethod` stays CLOSED and absence rides `Option` at the read — an `Unknown` sentinel row is a member of a closed family that names no method, and it resolves against no wire key while type-checking everywhere. Hyphenated LCIA scope keys (`A1A2A3`, `B1`…`B7`, `C1`…`C4`) require `[JsonPropertyName]` aliases; the `fields` query mask trims each leg to its own projection, so a category page carries candidate identity and basis alone and the winner's impacts are fetched once by identity rather than for every row the page returned; a failed read is the explicit `Fin.Fail` the caller surfaces, never a cached failure re-served as success.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Key carries the CITATION a report renders; WireKey the token `impacts[method]` and `lcia_method=` are keyed by. One
// string serving both makes a renamed citation silently miss every impact lookup, so each row declares its own crossing
// spelling exactly as the EP axis does. The family is CLOSED — absence rides Option at the read site, never a sentinel
// row that names no method, resolves against no wire key, and type-checks everywhere.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LciaMethod {
    public static readonly LciaMethod En15978 = new("EN 15978:2011", wireKey: "EN 15978");
    public static readonly LciaMethod IpccAr6 = new("IPCC AR6", wireKey: "IPCC AR6");
    public static readonly LciaMethod Traci21 = new("TRACI 2.1", wireKey: "TRACI 2.1");
    public static readonly LciaMethod Ef31    = new("EF 3.1", wireKey: "EF 3.1");

    public string WireKey { get; }
}

// ONE carbon-resolution request: the case is the rung of the fallback ladder, its payload the coordinates. Three legs on
// one contract means a second catalogue, a fixture, or a cached corpus substitutes by binding one delegate.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EpdQuery {
    private EpdQuery() { }

    // Products reads a category page of CANDIDATES — identity, expiry, and basis alone, no impacts.
    public sealed record Products(string Omf, LciaMethod Method, int Page) : EpdQuery;
    // Document fetches the winning candidate's full declaration once by identity rather than per page row.
    public sealed record Document(string Uuid, LciaMethod Method) : EpdQuery;
    // Generic reads the category substitution line a ply with no fresh product declaration falls back to.
    public sealed record Generic(string Omf, LciaMethod Method) : EpdQuery;
}

// --- [MODELS] ------------------------------------------------------------------------------
// Magnitude plus its unit abbreviation — the two facts a basis resolves from, which the seam MeasureValue.Of coerces to
// an SI dimension and scalar once.
public readonly record struct DeclaredAmount(double Qty, string Unit);

// Provider-neutral environmental declaration: the per-stage GWP vector in the declaration's OWN basis, the two basis
// witnesses Normalize reads, the expiry a freshness filter reads, and the citation pair a PropertyEvidence carries.
// StageGwp is None on a candidate row, whose page projection carries no impacts — the fold then resolves the winner's
// document, and a declaration that reaches normalization without a vector rails rather than defaulting a zero one.
public sealed record EpdDeclaration(
    string Source, string Reference, Option<Instant> ValidUntil,
    Option<DeclaredAmount> DeclaredUnit, Option<DeclaredAmount> KgPerDeclaredUnit, Option<double[]> StageGwp);

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

// EN 15978 life-cycle modules band onto the seam LifecycleStage roster. The fifteen [JsonPropertyName] members ARE the
// wire and stay verbatim; the BANDING is one generated projection keyed by seam stage row, so the whole-life GWP folds
// every module the EPD declares and a new stage row is one entry rather than a re-cut fixed-slot literal whose every
// later slot shifts. A1A2A3 is the cradle-to-gate product total, D the beyond-system-boundary credit.
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

    // ONE banding owner: each seam stage row names the wire members that sum onto it. A stage the wire declares nothing
    // for reads zero through the projection rather than shifting every slot after it.
    static readonly FrozenDictionary<LifecycleStage, Func<ScopeSet, double>> Banding =
        new KeyValuePair<LifecycleStage, Func<ScopeSet, double>>[] {
            new(LifecycleStage.A1A3, static s => Mean(s.A1A2A3)),
            new(LifecycleStage.A4,   static s => Mean(s.A4)),
            new(LifecycleStage.A5,   static s => Mean(s.A5)),
            new(LifecycleStage.B,    static s => Mean(s.B1) + Mean(s.B2) + Mean(s.B3) + Mean(s.B4) + Mean(s.B5) + Mean(s.B6) + Mean(s.B7)),
            new(LifecycleStage.C,    static s => Mean(s.C1) + Mean(s.C2) + Mean(s.C3) + Mean(s.C4)),
            new(LifecycleStage.D,    static s => Mean(s.D)),
        }.ToFrozenDictionary();

    // Projection generated over the seam roster, written at each row's OWN Index, so the vector is arity-correct by
    // construction against LifecycleStage.Count rather than by the order a literal happened to be typed in.
    public double[] ToStageVector() {
        double[] vector = new double[LifecycleStage.Count];
        foreach (LifecycleStage stage in LifecycleStage.Items) {
            vector[stage.Index] = Banding.TryGetValue(stage, out Func<ScopeSet, double>? read) ? read(this) : 0.0;
        }
        return vector;
    }

    static double Mean(Measurement? m) => m?.Mean ?? 0.0;
}

public sealed record Epd(string? Id, [property: JsonPropertyName("valid_until")] Instant? ValidUntil,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit,
    [property: JsonPropertyName("kg_per_declared_unit")] Amount? KgPerDeclaredUnit,
    Dictionary<string, Dictionary<string, ScopeSet>> Impacts) {
    // Per-module carbon reads impacts[<wire key>]["gwp"] and returns None when either key is absent — the method's WIRE
    // spelling, never its citation, so a re-worded citation cannot silently miss the lookup.
    public Option<ScopeSet> Gwp(LciaMethod method) =>
        Impacts.TryGetValue(method.WireKey, out Dictionary<string, ScopeSet> set) && set.TryGetValue("gwp", out ScopeSet gwp) ? Some(gwp) : None;
}

// Category-scoped GWP substitution line carries EC3 conservative_estimate (80th-percentile) in kgCO2e per declared unit
// is the generic value a ply with no fresh product EPD falls back to; declared_unit carries the basis the per-m³
// normalization reads (a category statistic carries no kg_per_declared_unit — a mass-based category needs the ply
// density). The response's other percentile lines are ignored.
public sealed record StatisticsDto(
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);

// --- [SERVICES] ----------------------------------------------------------------------------
// Ec3Service is the openEPD ADAPTER: it satisfies the resolver contract and owns every wire spelling behind it, so the
// carbon folds name a delegate and this class is one binding at the composition root.
public sealed class Ec3Service(HttpClient http, HybridCache cache, JsonSerializerContext json) {
    // Candidate search reads one page wide: a single token charge surfaces enough category rows for the freshness pick,
    // never a per-ply multi-page crawl.
    const int SearchPageSize = 100;

    // Candidate rows carry identity and basis ONLY — the winner's impacts are fetched once by identity, so a hundred-row
    // page never pays for ninety-nine impact matrices the fold discards.
    const string CandidateFields = "id,valid_until,declared_unit,kg_per_declared_unit";
    const string DocumentFields = "id,valid_until,declared_unit,kg_per_declared_unit,impacts";

    // ONE read over the generated total Switch: a new rung is a case the compiler demands an arm for. LCIA method is
    // behavior-bearing on the wire — lcia_method selects which method's statistics line the service computes — so the
    // cache identity and the remote request agree and no leg can label one method's estimate as another's GWP.
    public async Task<Fin<Seq<EpdDeclaration>>> Resolve(EpdQuery query) => await query.Switch(
        products: async p => (await Cached<Epd[]>($"search:{p.Omf}:{p.Method.WireKey}:{p.Page}",
                $"/v2/epds/search?omf={Uri.EscapeDataString(p.Omf)}&page_number={p.Page}&page_size={SearchPageSize}&fields={CandidateFields}",
                Unwrap<Epd[]>))
            .Map(static rows => toSeq(rows).Map(static row => Ec3Wire.Candidate(row))),
        document: async d => (await Cached<Epd>($"epd:{d.Uuid}:{d.Method.WireKey}",
                $"/epds/{Uri.EscapeDataString(d.Uuid)}?fields={DocumentFields}", Bare<Epd>))
            .Map(row => Declared(row, d.Method)),
        generic: async g => (await Cached<StatisticsDto>($"stat:{g.Omf}:{g.Method.WireKey}",
                $"/v2/epds/statistics?omf={Uri.EscapeDataString(g.Omf)}&lcia_method={Uri.EscapeDataString(g.Method.WireKey)}",
                Unwrap<StatisticsDto>))
            .Map(static stats => Ec3Wire.Substitution(stats)));

    // Document lowering keeps the sanctioned post-`with` for the method-selected vector — never a
    // [MapPropertyFromSource] whole-source reader, whose presence would suppress RMG020 for the whole mapping.
    static Seq<EpdDeclaration> Declared(Epd row, LciaMethod method) =>
        Seq(Ec3Wire.Candidate(row) with { StageGwp = row.Gwp(method).Map(static scope => scope.ToStageVector()) });

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

    // Source-generated decode rides the (Type, JsonSerializerContext) pair — one of the three contract-bound forms every
    // serializer verb admits — so the closed Envelope<Epd[]>/Envelope<StatisticsDto>/Epd contracts the context registers
    // resolve without a reflection fallback and without hand-casting a JsonTypeInfo out of the context per call.
    static async ValueTask<Option<T>> Unwrap<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional(((Envelope<T>?)await content.ReadFromJsonAsync(typeof(Envelope<T>), json))?.Payload);

    static async ValueTask<Option<T>> Bare<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((T?)await content.ReadFromJsonAsync(typeof(T), json));

    // Boundary-crossing carrier lifts ComputeFault across the HybridCache factory throw and converts back to
    // Fin.Fail at the one catch so a failed read never caches and never escapes as a raw exception.
    sealed class Ec3Boundary(Error fault) : Exception { public Error Fault { get; } = fault; }
}

// Wire -> neutral lowering is COMPILER-PROOF: `Candidate` generates member-by-member under
// RequiredMappingStrategy.Both, so an EpdDeclaration column added later FAILS THE BUILD instead of silently
// carrying a default — the hand initializer this replaces compiled clean with a new column absent. The mapping
// is READER-FREE, so RMG020 keeps its source-side force and the [MapperIgnoreSource] roster below is compiler
// inventory, not authored prose; LOC stays ~flat — the gain is the compiler, not the line count.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(EpdCodec))]
public static partial class Ec3Wire {
    // Candidate rows carry identity and basis ONLY — the provider tag and the absent vector are [MapValue]
    // constants, and the Impacts matrix is the one source member the candidate projection deliberately drops.
    [MapProperty(nameof(Epd.Id), nameof(EpdDeclaration.Reference), Use = nameof(EpdCodec.Reference))]
    [MapValue(nameof(EpdDeclaration.Source), "epd")]
    [MapValue(nameof(EpdDeclaration.StageGwp), Use = nameof(EpdCodec.NoVector))]
    [MapperIgnoreSource(nameof(Epd.Impacts))]
    public static partial EpdDeclaration Candidate(Epd row);

    // Category statistics resolve A1-A3 alone (the conservative estimate is a cradle-to-gate substitution
    // value) and carry no kg_per_declared_unit — a stage-vector fan-out no member mapping spells, so this leg
    // stays the admitted [UserMapping] hand fold beside the generated sibling.
    [UserMapping]
    public static Seq<EpdDeclaration> Substitution(StatisticsDto stats) {
        double[] product = new double[LifecycleStage.Count];
        product[LifecycleStage.A1A3.Index] = stats.ConservativeEstimate;
        return Seq(new EpdDeclaration("ec3-statistics", "conservative", None, EpdCodec.Declared(stats.DeclaredUnit), None, Some(product)));
    }
}

// [NamedMapping] converters own every wire-shape lift, registered whole through [UseStaticMapper] so the
// nullable Instant and Amount members cross with no per-member configuration.
public static class EpdCodec {
    [NamedMapping("reference")]
    public static string Reference(string? id) => id ?? "";

    public static Option<Instant> Lifted(Instant? at) => Optional(at);

    public static Option<DeclaredAmount> Declared(Amount? amount) =>
        amount is { Qty: { } qty } ? Some(new DeclaredAmount(qty, amount.Unit ?? "")) : None;

    public static Option<double[]> NoVector() => None;
}
```

## [03]-[CARBON_RUNNER]

- Owner: `LifecycleAssessment.RunCarbon` the pure-sync EN 15978 embodied-carbon assessment (a graph read, no network); `LifecycleAssessment.EnrichCarbon` the async ingress that decodes resolved declarations onto the seam `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; `Resolve`/`Fallback`/`Freshest`/`ToEnvironmental`/`Normalize` the per-ply fallback ladder; the Compute-owned `LifecycleGraphReads.TakeoffOf` base-quantity read; the `CarbonQuery` request input.
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock)` folds `AssemblyAggregator.AggregateEnvironmental` over each target's `MaterialComposition` and baked `TakeoffOf`; `EnrichCarbon(ElementGraph graph, Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>> epds, AssessmentRequest.Carbon request, IClock clock, Op key)` resolves undeclared plies through the ladder and returns a typed `GraphDelta` rail.
- Auto: `RunCarbon` resolves each ply's seam properties through one `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` resolver keyed on the composition's native `MaterialId` (never a graph `NodeId`), and the per-element area + volume through `TakeoffOf`, so a baked and a catalogue-resolved declaration fold identically. `EnrichCarbon` enumerates the undeclared ply materials (the `MaterialId` set lacking the `Environmental` case, not the element's directly-associated material), resolves each through the three-rung ladder — the category page's freshest non-expired candidate, that winner's own document, then the category substitution line — `Normalize`s the declaration's stage vector to per-one-unit of its native basis and tags that `MeasurementBasis`, embeds the carbon-only per-stage GwpTotal row into the full seam `(ImpactCategory × LifecycleStage)` matrix through `CarbonMatrix` (un-declared indicator rows zeroed, the partial-EPD invariant), and accumulates one monoid `GraphDelta` the composition root applies (an unresolvable-basis ply is skipped, not mis-scaled). Assessment stays a pure-sync graph read because every network call lives behind the explicit `EnrichCarbon` resolver, never inside the fold.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`), Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet`/`OfEnvironmental`/`PropertyEvidence`, `MaterialPropertyAccess.Environmental`, `ImpactCategory`/`LifecycleStage`, `MeasurementBasis`, `MaterialId`, `NodeId`, `Node.Material`/`Node.QuantitySet`, `Relationship.Assign`/`AssignKind`, `GraphDelta.Put`, `MeasureValue.Of`/`MeasureValue.Si`, `Dimension.VolumeDim`/`Dimension.AreaDim`/`Dimension.MassDim`, `Provenance`), UnitsNet (via `MeasureValue.Of` — the declared-unit abbreviation -> SI dimension/scalar coercion the basis tagging rides), Rasm (kernel `Op`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, NodaTime (`Instant`), BCL inbox (`ImmutableArray<double>` the seam impact-matrix store the ingress builds).
- Growth: a new lifecycle module is one seam `LifecycleStage` row (the `StageGwp` vector, the `ScopeSet` banding entry, and the aggregator fold widen by data); a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner; a richer selection (lowest-GWP, spec-matched) is one refinement of `Freshest`; a second carbon catalogue is one resolver binding.
- Boundary: the fold takes the RESOLVER — `Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>>` — not a named service, so the ladder is provider-neutral by construction and a fixture, a second catalogue, or a cached corpus substitutes at the composition root; naming the concrete client here made the ladder untestable without an HTTP fake and unusable against any other provider. `AggregateEnvironmental` over each ply's baked `Environmental` mints the PRIMARY GWP — the catalogue is the FALLBACK the async `EnrichCarbon` resolves, applied as a `GraphDelta` before the sync `RunCarbon`, so a fully-declared model needs no network call; the takeoff reads the baked `Qto_*BaseQuantities` (`TakeoffOf`) so a target with no base quantity rails `AssessmentInputMissing` rather than a silent zero takeoff; the GWP/intensity stay raw kgCO2e through `DomainMeasure` (dimensionless `MeasureValue` + label), never `UnitsNet.Mass`; `EnrichCarbon` splits failure by kind — a DETERMINISTIC data absence (no fresh declaration, a declared unit with no resolvable dimension such as a bare-count row lacking its kg-per-unit witness, a missing method GWP) skips the ply so `RunCarbon` rails the still-undeclared ply at its own fold, never defaulting a sentinel carbon or admitting a mis-scaled figure (a per-m² or per-kg declaration folds correctly under its tagged `MeasurementBasis` rather than being dropped), while a TRANSPORT/timeout fault aborts the enrichment rail (a partial delta masks the outage a retry still resolves); the runner reads the CONCRETE graph (above the seam), the write-back the `Analysis/assessment` spine's content-keyed `Node.Assessment`.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // Pure synchronous assessment folds the aggregator over each target's baked or EC3-enriched
    // composition + base-quantity takeoff; the governing ratio is the whole-life carbon against the design target, or
    // double.NaN -> NotApplicable with no target (never a misleading 0.0-ratio Satisfied) — the energy-runner convention.
    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock) =>
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
                new Provenance("LifecycleAssessment", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant())));

    // Async ingress resolves each undeclared ply through the three-rung ladder, decodes the declaration to the carbon
    // GwpTotal row in the seam matrix, and accumulates the enriching delta the composition root applies before the sync
    // RunCarbon, so a fully-declared model needs no network call. The resolver is a PARAMETER: the ladder is domain
    // logic over a contract, and the catalogue behind it is a composition-root binding.
    public static async Task<Fin<GraphDelta>> EnrichCarbon(
        ElementGraph graph, Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>> epds, AssessmentRequest.Carbon request, IClock clock, Op key) {
        Fin<GraphDelta> delta = Fin.Succ(GraphDelta.Empty);
        // Ingress boundary: a serial, rate-limited fetch over a token-metered endpoint, each resolved ply accumulated
        // onto the monoid delta. Failure splits by kind: DETERMINISTIC data absence (AssessmentInputMissing — no fresh
        // declaration, an unresolvable declared-unit basis, a missing method GWP) SKIPS the ply, because RunCarbon then
        // rails the still-undeclared ply with its own precise fault at the fold — the right surfacing point — while a
        // mis-scaled default would be silent; a TRANSPORT/timeout fault ABORTS the rail, because a partial delta would
        // erase the outage and mask the plies a retry could still resolve.
        foreach (Node.Material material in MissingDeclarations(graph, request.Targets)) {
            Fin<MaterialPropertySet> resolved = await Resolve(epds, request.Query, material, clock.GetCurrentInstant(), key);
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

    // Per-ply ladder: the freshest non-expired candidate in the ply's category, that winner's own full declaration, else
    // the category substitution line; a per-material OMF override resolves a multi-material assembly. Each rung tags the
    // declaration's native MeasurementBasis without a density — a per-kg basis resolves to mass at aggregation.
    static async Task<Fin<MaterialPropertySet>> Resolve(
        Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>> epds, CarbonQuery query, Node.Material material, Instant now, Op key) {
        string omf = query.OmfByMaterial.Find(material.MaterialKey.Value).IfNone(query.Omf);
        Fin<Seq<EpdDeclaration>> candidates = await epds(new EpdQuery.Products(omf, query.Method, Page: 1));
        return await candidates.Match(
            Succ: rows => Freshest(rows, now).Match(
                Some: winner => Document(epds, winner, query.Method, omf, key),
                None: () => Fallback(epds, omf, query.Method, key)),
            // Only deterministic DATA ABSENCE degrades to the substitution line; a transport or timeout fault is a
            // retryable outage that aborts the enrichment rail typed — masking it behind a statistics fallback
            // would return a partial delta after a transient failure, the deleted form.
            Fail: fault => Retryable(fault)
                ? Task.FromResult(Fin.Fail<MaterialPropertySet>(fault))
                : Fallback(epds, omf, query.Method, key));
    }

    // Winner's document carries the impacts its candidate row omitted; a document that still resolves no vector for
    // this method is deterministic absence, so it descends to the substitution line rather than railing the ply.
    static async Task<Fin<MaterialPropertySet>> Document(
        Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>> epds, EpdDeclaration winner, LciaMethod method, string omf, Op key) {
        Fin<Seq<EpdDeclaration>> document = await epds(new EpdQuery.Document(winner.Reference, method));
        return await document.Match(
            Succ: rows => rows.Head.Filter(static row => row.StageGwp.IsSome).Match(
                Some: declaration => Task.FromResult(ToEnvironmental(declaration, key)),
                None: () => Fallback(epds, omf, method, key)),
            Fail: fault => Retryable(fault)
                ? Task.FromResult(Fin.Fail<MaterialPropertySet>(fault))
                : Fallback(epds, omf, method, key));
    }

    static async Task<Fin<MaterialPropertySet>> Fallback(
        Func<EpdQuery, Task<Fin<Seq<EpdDeclaration>>>> epds, string omf, LciaMethod method, Op key) =>
        (await epds(new EpdQuery.Generic(omf, method)))
            .Bind(rows => rows.Head.ToFin((Error)new ComputeFault.AssessmentInputMissing($"<epd-substitution-absent:{omf}>")))
            .Bind(declaration => ToEnvironmental(declaration, key));

    // One transient discriminant both descent points read, so a retryable outage can never be classified two ways.
    static bool Retryable(Error fault) =>
        fault is ComputeFault.EndpointUnreachable || fault is ComputeFault.AnalysisFailed { Kind: var kind } && kind == FailureKind.Timeout;

    // Freshest non-stale candidate: an absent expiry is non-expiring, a dated one requires expiry >= now, and the latest
    // expiry wins within the category. Candidate rows carry no impacts, so the GWP filter belongs one rung lower.
    static Option<EpdDeclaration> Freshest(Seq<EpdDeclaration> rows, Instant now) =>
        toSeq(rows.Filter(row => row.ValidUntil.Match(Some: valid => valid >= now, None: static () => true))
            .OrderByDescending(static row => row.ValidUntil.IfNone(Instant.MaxValue))).Head;

    // ONE decode for every rung — a per-rung decode re-spelled the basis chain, the matrix embed, and the evidence stamp
    // three times. Normalize the declaration's stage vector to per-one-unit of its native basis (tagged with the
    // MeasurementBasis the fold scales by), embed the carbon GwpTotal row into the full seam (ImpactCategory ×
    // LifecycleStage) matrix through CarbonMatrix, and admit it through OfEnvironmental — the un-declared indicator rows
    // stay zero (the partial-EPD invariant). OfEnvironmental takes the matrix, no gwpKgCo2e param and no
    // GlobalWarmingPotential field; the carbon rides the GwpTotal-row cells (Gwp => IndicatorAt(GwpTotal, A1A3) is the
    // seam's derived cradle-to-gate read). The citation pair + LocalDate expiry ride the PropertyEvidence arg (the
    // Instant lowers via InUtc().Date, never a coarse int year).
    static Fin<MaterialPropertySet> ToEnvironmental(EpdDeclaration declaration, Op key) =>
        declaration.StageGwp.Match(
            Some: vector => Normalize(vector, declaration.DeclaredUnit, declaration.KgPerDeclaredUnit, key).Match(
                Some: norm => MaterialPropertySet.OfEnvironmental(
                    norm.Basis, MaterialPropertySet.Environmental.CarbonMatrix(norm.PerUnit), recycledContent: 0.0, endOfLifeRecovery: 0.0,
                    key, new PropertyEvidence(declaration.Source, declaration.Reference, declaration.ValidUntil.Map(static v => v.InUtc().Date))),
                None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<epd-basis-unresolved:{declaration.Reference}>"))),
            None: () => Fin.Fail<MaterialPropertySet>((Error)new ComputeFault.AssessmentInputMissing($"<epd-missing-gwp:{declaration.Reference}>")));

    // Normalize a per-declared-unit stage vector to per-one-unit of its native basis and tag the MeasurementBasis the
    // fold scales by — the strongest-dimension route winning through the Option `|` choice: a volume declared unit ->
    // PerM3, an area -> PerM2, a mass -> PerKg, else the kg-per-unit chain -> PerKg. Density is not read here — a per-kg
    // basis resolves to mass at aggregation (volume × the ply Mechanical.Density). None whenever no basis evidence
    // resolves — a MISSING declared unit included: only the kg-per-unit chain can still ground it, and absent both the
    // ply is skipped, because a defaulted PerM3 over an unknown basis mis-scales the declaration as volume material.
    static Option<(MeasurementBasis Basis, double[] PerUnit)> Normalize(
        double[] perDeclaredUnit, Option<DeclaredAmount> declaredUnit, Option<DeclaredAmount> kgPerDeclaredUnit, Op key) {
        Option<double> kgPerUnit = kgPerDeclaredUnit.Bind(k => MeasureValue.Of(k.Qty, k.Unit, key).ToOption().Map(static m => m.Si)).Filter(static kg => kg > 0.0);
        if (declaredUnit.Case is not DeclaredAmount unit) {
            return kgPerUnit.Map(kg => (MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / kg)));
        }
        Option<MeasureValue> declared = MeasureValue.Of(unit.Qty, unit.Unit, key).ToOption();
        Option<(MeasurementBasis, double[])> byVolume  = declared.Filter(static d => d.Dimension == Dimension.VolumeDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerM3, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byArea    = declared.Filter(static d => d.Dimension == Dimension.AreaDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerM2, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byMass    = declared.Filter(static d => d.Dimension == Dimension.MassDim && d.Si > 0.0).Map(d => (MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / d.Si)));
        Option<(MeasurementBasis, double[])> byKgChain = kgPerUnit.Map(kg => (MeasurementBasis.PerKg, Scale(perDeclaredUnit, 1.0 / kg)));
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
        toSeq(LifecycleStage.Items).TraverseM(stage => DomainMeasure($"{id.Value}/gwp-{stage.Module}", stageGwp[stage.Index], "kgCO2e")).As();

    // GWP and in-place cost are domain-basis scalars (kgCO2e, kgCO2e/m², a currency code), not UnitsNet quantities — a
    // dimensionless MeasureValue carrying the domain label, never the abbreviation-resolving MeasureValue.Of (which
    // rejects kgCO2e). The mint is the seam's labeled registry-less OfSi (the record ctor is private): the Scalar
    // QuantityType keeps the value dimensionless while the label rides CanonicalUnit, Fin so a NaN rails at the finite gate.
    static Fin<AssessmentFact> DomainMeasure(string name, double si, string unit) =>
        MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, si, unit).Map(value => AssessmentFact.Measure(name, value));
}

// Element geometric takeoff distributes GWP and cost per ply, reading the baked Qto_*BaseQuantities through the one
// Analysis/assessment AnalysisReads owner over the QuantityRows net-over-gross chains — every Qto set scanned, so a
// wall/slab/beam reads without a per-type accessor. A target with no baked base quantity rails the missing input.
public static class LifecycleGraphReads {
    extension(ElementGraph graph) {
        // Every row keys through a Rasm.Element-declared static and every read composes the one
        // Analysis/assessment AnalysisReads owner, so the net-over-gross preference is stated once on the declarer
        // and this reader shares one spelling with the non-referencing Bim and Fabrication writers.
        public Fin<ElementQuantity> TakeoffOf(NodeId element) {
            Option<double> volume = graph.Magnitude(element, QuantityRows.Volume);
            Option<double> area = graph.Magnitude(element, QuantityRows.SurfaceArea);
            return volume.IsNone && area.IsNone
                ? Fin.Fail<ElementQuantity>((Error)new ComputeFault.AssessmentInputMissing($"<element-base-quantities-absent:{element.Value}>"))
                // Fabrication NestYield.WasteAreaMm2 seam quantity contributes when the graph carries a nest-yield
                // row for this element — joins as the decode-side WasteAreaM2 column, so off-cut waste rolls
                // into the same AggregateEnvironmental/AggregateCost folds (the circulation ingress row).
                : Fin.Succ(new ElementQuantity(area.IfNone(0.0), volume.IfNone(0.0),
                    graph.Magnitude(element, QuantityRows.NestWasteArea).IfNone(0.0)));
        }
    }
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup runner; `CostBudget` the acceptance derivation over the request's two budget columns.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock)` folds `AssemblyAggregator.AggregateCost` over each target's `MaterialComposition` and baked `TakeoffOf`, guards currency, emits `supply-total`/`install-total`/`in-place-total` facts, and ratios the in-place total against the resolved budget.
- Packages: LanguageExt.Core, Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet.Cost`, `Currency`, `MaterialId`, `NodeId`, `MeasureValue`, `Dimension`, `Provenance`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementQuantity`/`PlyQuantity`, the Compute-owned `TakeoffOf`, BCL inbox.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost rail spans all composition cases (a single material or a profile member has a unit supply/install cost); a new acceptance modality is one `AssessmentRequest.Cost` budget column with one `CostBudget` arm.
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the `request.Currency` is load-bearing — the aggregated cost is guarded to it (a material priced in a different `Currency` rails, since the fold carries no exchange rate), so the request currency is a real validation target, never a decorative field; the per-ply quantity derives from the seam `Cost.Basis` against the baked `TakeoffOf` (or a `PlyQuantity` override); a material with no `Cost` case rails `AssessmentInputMissing`. Where the caller states a budget the governing ratio is REAL and the verdict a genuine `Satisfied`/`Marginal`/`Exceeded` band: `BudgetTotal` is the absolute cap on the target set's in-place cost, `BudgetPerArea` the rate against the same takeoff area the aggregator distributes cost by, the absolute column winning where both ride; only a request carrying NEITHER column projects `double.NaN` → `NotApplicable` (the informational rating, never a `0.0`-ratio `Satisfied` falsely asserting a budget pass) — the same no-target convention the energy and carbon runners hold, now a stated absence rather than a permanent one. Budgets are `decimal` because money is exact and a binary double silently re-rounds every currency figure it touches; the ratio widens once at the divide, where the operands are already a measured total.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // RunCost threads the in-place total and the takeoff area beside the facts, because both budget columns ratio
    // against a set-wide quantity a per-element fact stream cannot recover after the fact.
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock) =>
        request.Targets.Fold(
            Fin.Succ((Facts: Seq<AssessmentFact>(), InPlace: 0.0, AreaM2: 0.0)),
            (acc, id) => acc.Bind(state =>
                from composition in graph.CompositionOf(id).ToFin((Error)new ComputeFault.AssessmentInputMissing($"<cost-element-missing-composition:{id.Value}>"))
                from geometry in graph.TakeoffOf(id)
                from cost in AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), geometry)
                from _ in cost.Currency.Key == request.Currency
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>((Error)new ComputeFault.AssessmentInputMissing($"<cost-currency-mismatch:{cost.Currency.Key}<>{request.Currency}>"))
                from supply in DomainMeasure($"{id.Value}/supply-total", cost.SupplyTotal, cost.Currency.Key)
                from install in DomainMeasure($"{id.Value}/install-total", cost.InstallTotal, cost.Currency.Key)
                from inPlace in DomainMeasure($"{id.Value}/in-place-total", cost.TotalInPlace, cost.Currency.Key)
                select (Facts: state.Facts.Add(supply).Add(install).Add(inPlace),
                    InPlace: state.InPlace + cost.TotalInPlace,
                    AreaM2: state.AreaM2 + geometry.AreaM2)))
            .Map(state => AssessmentResult.Of(request.Route, state.Facts,
                CostBudget(request, state.AreaM2).Map(budget => state.InPlace / budget).IfNone(double.NaN),
                new Provenance("LifecycleAssessment", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant())));

    // Budget resolution is a two-rung choice, absolute first: BudgetTotal caps the whole target set, BudgetPerArea rates
    // against the SAME takeoff area the aggregator distributes cost by. A non-positive budget resolves to None rather
    // than an infinite ratio, so a zero column reports NotApplicable instead of asserting an instant exceedance.
    static Option<double> CostBudget(AssessmentRequest.Cost request, double areaM2) =>
        request.BudgetTotal.Map(static total => (double)total)
        | request.BudgetPerArea.Map(rate => (double)rate * areaM2)
        is { IsSome: true, Case: double budget } && budget > 0.0
            ? Some(budget)
            : None;
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
