# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner owns the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment pipeline — the EN 15978 embodied-carbon takeoff and the supply/install/lifecycle cost rollup. Both are ONE rollup over the `Analysis/aggregator` fold, discriminated by the aggregation each request names: the fold reads the contract `MaterialComposition`, distributes each ply by the element's baked `Qto_*BaseQuantities` takeoff, and projects a result into the uniform fact stream. Where a ply carries no baked EN 15978 declaration, the async `EnrichCarbon` ingress resolves one from the EC3 / openEPD REST catalogue through the fallback ladder, applied as a `GraphDelta` before the pure-sync carbon rollup.

One typed `HttpClient` rides the shared `CacheLane` under a framed content key, and each runner returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the governing ratio the whole-life carbon (or in-place cost) against the acceptance target the request carries — ABSENT where it carries none.

## [01]-[INDEX]

- [02]-[EC3_BOUNDARY]: `EpdQuery`→`EpdDeclaration` the resolver contract and `Ec3Service` the openEPD adapter satisfying it over the shared cache lane, the closed unit and indicator rosters, and the raw-kgCO2e GWP discipline.
- [03]-[CARBON_RUNNER]: the EN 15978 takeoff and `EnrichCarbon` the async EC3 ingress resolving each undeclared ply down one descent ladder.
- [04]-[COST_RUNNER]: the supply/install/lifecycle rollup over the same fold skeleton, guarded to the requested `Currency`.

## [02]-[EC3_BOUNDARY]

- Owner: `EpdQuery` the closed request `[Union]` whose cases ARE the rungs of one descent ladder and `EpdDeclaration`/`DeclaredAmount` the provider-neutral answer, together the `Func<EpdQuery, Task<Fin<EpdAnswer>>>` resolver contract every carbon fold takes; `Ec3Service` the openEPD ADAPTER satisfying it; `Ec3Wire` the Mapperly wire→neutral mapper with `EpdCodec` its `[NamedMapping]` converter roster registered whole through `[UseStaticMapper]`; the openEPD wire-type family (`Epd`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`/`Meta`/`Paging`/`Warning`/`Amount`/`Org`); `DeclaredUnit` the closed openEPD unit roster carrying each token's contract `MeasurementBasis`; `ImpactIndicator` the closed transcription from the openEPD `ImpactSet` indicator names onto the contract `ImpactCategory` rows; the `LciaMethod` `[SmartEnum<string>]` impact-method roster with its citation `Key` and wire `WireKey` columns; the `CarbonQuery` request input the `AssessmentRequest.Carbon` case carries.
- Entry: `Ec3Service.Resolve(EpdQuery query)` → `Task<Fin<EpdAnswer>>` is the adapter's ONE read, its generated total `Switch` binding the category page search, the by-identity document, the industry-wide EPD, the generic estimate, and the category statistic onto five GET-only legs. A `429` carrying a delta `Retry-After` lands `ComputeFault.EndpointThrottled` publishing the server's own window; a window-less rate limit, a request timeout, and a `5xx` land `ComputeFault.EndpointUnreachable` (`Transient`); every deterministic `4xx` and every decode refusal land `AnalysisFailed` inheriting the kernel `Terminal` posture. Retriability is PUBLISHED at the fault and executed at the root-bound handler, so no arm here spells a predicate over exception types.
- Auto: the five legs share ONE polymorphic `Cached<T>` fold parameterized by the decode shape (`Unwrap<T>` for the `{payload, meta}` message envelope, `Bare<T>` for the by-identity documents); the cache stores a `Cached<Fin<T>>` envelope so a SUCCESS holds for the provider's revision cadence while a DETERMINISTIC refusal holds only for the lane's negative window and a transient one is never written at all — the boundary-crossing exception carrier this replaces existed solely to make `HybridCache` skip a write. The slot derives through the kernel `ContentHash.Of<TState>` over the leg's own framed fields beside the response `meta.mf_hash` the server resolved the filter to, so two OMF strings the server normalizes to one filter share a slot and a length-framed key cannot collide two legs whose colon-joined concatenation once did. Every module a declaration carries bands onto the contract `LifecycleStage` roster and every indicator onto `ImpactCategory` through generated projections keyed by row, so the wire's members map by data rather than by a hand-summed fixed-slot literal, and a row the contract does not carry lands a NAMED degrade rather than a silent zero.
- Law: KEY PRESENCE is the coverage census. An openEPD scope member the wire never declared is UNDECLARED ABSENCE, never a zero — the `?? 0.0` collapse this replaces erased the census before the sum, so a partial EPD and a genuinely zero-impact module published the same number and the contract matrix then zero-filled every undeclared indicator with nothing left to say which. Every declared cell rides `EpdDeclaration.Impacts` as an `(indicator, stage)` KEY, and the coverage a consumer reads DERIVES from that key set.
- Law: the reference study period is the B-stage's own scale. `product_service_life_years` and each scope's `Bn_years` are the two columns EN 15978 B1–B7 arithmetic needs — a use-stage value declared over one year and summed straight into a sixty-year study reports a building's whole maintenance and operational carbon as a single year of it, and a product whose service life is shorter than the study period is REPLACED, its product stage re-incurred at B4. Absent either column the B stages carry their declared magnitude and the result states the unscaled basis rather than fabricating a period.
- Packages: `System.Net.Http` (typed client + `ReadFromJsonAsync(Type, JsonSerializerContext)`), `System.Text.Json` (source-generated context, AOT-safe), Microsoft.Extensions.Caching.Hybrid (reached through the `Rasm.AppHost` `Runtime/resources#CACHE_LANES` `CacheSurface`, never a cache instance), Riok.Mapperly (`[Mapper]`, `[MapProperty(Use = …)]`, `[MapValue]`, `[UserMapping]`, `[NamedMapping]`, `[UseStaticMapper]`, `[MapperIgnoreSource]` — the reader-free wire lowering), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[ValueObject<string>]`, `[Union]`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`HashMap`/`Validation`), Generator.Equals (`[Equatable]` — the impact-map and stage-vector reference-equality repair), NodaTime (`Instant`/`Duration`), Rasm (kernel — `ContentHash.Of<TState>`/`CanonicalWriter` the framed slot key, `Retriability` the published posture), Rasm.Element (project — `LifecycleStage`/`ImpactCategory` the banding projections are generated over, `MeasurementBasis`, `PropertyEvidence`/`EvidenceGrade`), Rasm.AppHost (project — the `CacheLane` descriptor and its `CacheSurface`), BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row carrying its citation and wire spellings; a new decoded openEPD member is one source-gen context property and one banding entry; a new lifecycle module is one contract `LifecycleStage` row with one banding entry here, a new EN 15804 indicator one contract `ImpactCategory` row with one `ImpactIndicator` row; a new ladder rung is one `EpdQuery` case the descent's own roster orders; a SECOND carbon provider is one type satisfying the resolver contract with zero edit to the folds — the boundary widens by row and by adapter, never by a second HTTP client and never a per-endpoint cache path.
- Boundary: the carbon folds take the RESOLVER, never this class — an assessment that names its provider cannot be run against a second catalogue, a fixture, or a cached corpus without editing the fold. Only the GET read surface is consumed (Rasm is a carbon consumer, never a publisher), and the openEPD wire family stays adapter-local: `Epd`/`ScopeSet`/`Amount` never cross the resolver contract, `EpdDeclaration` carrying the declared indicator cells, the two basis witnesses, the service life, and the `PropertyEvidence` the folds read. Cache tags reach `HybridCache` only through `CacheLane.Tag` and entry lifetime only through the lane's own `Entry` — this lane names an owner key and the lane frames it, where a page-local `HybridCacheEntryOptions` beside raw string tags was a THIRD cache authority against the folder's one-owner ruling and a tag space no `Invalidate` could reach. GWP `Measurement.Mean` is kgCO2e per declared unit and is not a `UnitsNet` quantity — it crosses interior signatures as a raw `double` and lands as a dimensionless `MeasureValue` labeled `kgCO2e` through `DomainMeasure`, never `UnitsNet.Mass` and never the abbreviation-resolving `MeasureValue.Of` (which rejects `kgCO2e`). `LciaMethod` carries its wire spelling as its OWN column (the `Model/providers#EP_AXIS` `WireKey` precedent): the citation a report renders and the token `impacts[method]` and `lcia_method=` are keyed by are two facts, and one string serving both makes a renamed citation silently miss every impact lookup. `LciaMethod` stays CLOSED and absence rides `Option` at the read — the wire's own `Unknown LCIA` bucket is a REFUSAL here, because a declaration whose method nothing named cannot be compared against one whose method the route pinned. `doctype`/`openepd_version` gate the decoder before any impact read, so a re-shaped future document refuses rather than decoding half. Provenance is the declaration's own orgs — `manufacturer`, `program_operator`, `third_party_verifier` and the `compliance` standards — never the `"epd"` literal a `[MapValue]` constant once stamped on every row alike. Hyphenated LCIA scope and indicator keys (`A1A2A3`, `B1`…`B7`, `C1`…`C4`, `gwp-fossil`, `ADP-mineral`, `ETP-fw`, `HTP-c`) require `[JsonPropertyName]` aliases; the `fields` query mask trims each leg to its own projection, so a category page carries candidate identity and basis alone and the winner's impacts are fetched once by identity rather than for every row the page returned; `meta.warnings[]` fold into the result as soft notes and `meta.paging` states whether the one probed page WAS the whole candidate set, because a freshest-of-100 pick silently presented as a freshest-of-all is the selection defect no downstream number reveals.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LciaMethod {
    public static readonly LciaMethod En15978 = new("EN 15978:2011", wireKey: "EN 15978:2011");
    public static readonly LciaMethod Traci22 = new("TRACI 2.2", wireKey: "TRACI 2.2");
    public static readonly LciaMethod Traci21 = new("TRACI 2.1", wireKey: "TRACI 2.1");
    public static readonly LciaMethod Traci20 = new("TRACI 2.0", wireKey: "TRACI 2.0");
    public static readonly LciaMethod Traci10 = new("TRACI 1.0", wireKey: "TRACI 1.0");
    public static readonly LciaMethod IpccAr6 = new("IPCC AR6", wireKey: "IPCC AR6");
    public static readonly LciaMethod IpccAr5 = new("IPCC AR5", wireKey: "IPCC AR5");
    public static readonly LciaMethod Ef31 = new("EF 3.1", wireKey: "EF 3.1");
    public static readonly LciaMethod Ef30 = new("EF 3.0", wireKey: "EF 3.0");
    public static readonly LciaMethod Ef20 = new("EF 2.0", wireKey: "EF 2.0");
    public static readonly LciaMethod ReCiPe2016 = new("ReCiPe 2016", wireKey: "ReCiPe 2016");
    public static readonly LciaMethod ReCiPe2008 = new("ReCiPe 2008", wireKey: "ReCiPe 2008");
    public static readonly LciaMethod Cml2016 = new("CML 2016", wireKey: "CML 2016");
    public static readonly LciaMethod Cml2012 = new("CML 2012", wireKey: "CML 2012");
    public static readonly LciaMethod Cml2007 = new("CML 2007", wireKey: "CML 2007");
    public static readonly LciaMethod Cml2001 = new("CML 2001", wireKey: "CML 2001");
    public static readonly LciaMethod Cml1992 = new("CML 1992", wireKey: "CML 1992");
    public static readonly LciaMethod UseTox212 = new("USEtox 2.12", wireKey: "USEtox 2.12");
    public static readonly LciaMethod Lime2 = new("LIME2", wireKey: "LIME2");
    public static readonly LciaMethod GwpGhg = new("GWP-GHG", wireKey: "GWP-GHG");

    public string WireKey { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImpactIndicator {
    public static readonly ImpactIndicator Gwp = new("gwp", ImpactCategory.GwpTotal);
    public static readonly ImpactIndicator GwpFossil = new("gwp-fossil", ImpactCategory.GwpFossil);
    public static readonly ImpactIndicator GwpBiogenic = new("gwp-biogenic", ImpactCategory.GwpBiogenic);
    public static readonly ImpactIndicator GwpLuluc = new("gwp-luluc", ImpactCategory.GwpLuluc);
    public static readonly ImpactIndicator Odp = new("odp", ImpactCategory.Odp);
    public static readonly ImpactIndicator Ap = new("ap", ImpactCategory.Ap);
    public static readonly ImpactIndicator EpFresh = new("ep-fresh", ImpactCategory.EpFreshwater);
    public static readonly ImpactIndicator EpMarine = new("ep-marine", ImpactCategory.EpMarine);
    public static readonly ImpactIndicator EpTerrestrial = new("ep-terr", ImpactCategory.EpTerrestrial);
    public static readonly ImpactIndicator Pocp = new("pocp", ImpactCategory.Pocp);
    public static readonly ImpactIndicator AdpMinerals = new("ADP-mineral", ImpactCategory.AdpMinerals);
    public static readonly ImpactIndicator AdpFossil = new("ADP-fossil", ImpactCategory.AdpFossil);

    public ImpactCategory Category { get; }

    public static Option<ImpactIndicator> Of(string wireKey) => toSeq(Items).Find(row => row.Key == wireKey);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DeclaredUnit {
    public static readonly DeclaredUnit Kilogram = new("kg", Some(MeasurementBasis.PerKg));
    public static readonly DeclaredUnit SquareMetre = new("m2", Some(MeasurementBasis.PerM2));
    public static readonly DeclaredUnit CubicMetre = new("m3", Some(MeasurementBasis.PerM3));
    public static readonly DeclaredUnit Item = new("item", Some(MeasurementBasis.PerItem));
    public static readonly DeclaredUnit Use = new("use", Some(MeasurementBasis.PerItem));
    public static readonly DeclaredUnit Metre = new("m", None);
    public static readonly DeclaredUnit ThermalResistance = new("m2 * RSI", None);
    public static readonly DeclaredUnit Megajoule = new("MJ", None);
    public static readonly DeclaredUnit TonneKilometre = new("t * km", None);
    public static readonly DeclaredUnit Megapascal = new("MPa", None);
    public static readonly DeclaredUnit Watt = new("W", None);
    public static readonly DeclaredUnit Celsius = new("°C", None);
    public static readonly DeclaredUnit KgCo2e = new("kgCO2e", None);
    public static readonly DeclaredUnit Hour = new("hour", None);

    public Option<MeasurementBasis> Basis { get; }

    public static Option<DeclaredUnit> Of(string token) => toSeq(Items).Find(row => row.Key == token);
}

[ValueObject<string>]
public sealed partial class Omf {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value is { Length: > 0 } filter && filter.StartsWith(OmfPrefix, StringComparison.Ordinal) && filter.Contains(OmfPragma, StringComparison.Ordinal)
            ? null
            : new ValidationError(message: $"<omf-grammar:{value}>");

    const string OmfPrefix = "!EC3 ";
    const string OmfPragma = "!pragma oMF(";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EpdQuery {
    private EpdQuery() { }

    public sealed record Products(Omf Omf, LciaMethod Method) : EpdQuery;
    public sealed record Document(string Uuid, LciaMethod Method) : EpdQuery;
    public sealed record Industry(string Uuid, LciaMethod Method) : EpdQuery;
    public sealed record Generic(string Uuid, LciaMethod Method) : EpdQuery;
    public sealed record Statistic(Omf Omf, LciaMethod Method) : EpdQuery;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DeclaredAmount(double Qty, DeclaredUnit Unit);

public readonly record struct IndicatorCell(ImpactCategory Category, LifecycleStage Stage, double PerDeclaredUnit);

[Equatable]
public sealed partial record EpdDeclaration(
    PropertyEvidence Evidence,
    Option<DeclaredAmount> DeclaredUnit,
    Option<DeclaredAmount> KgPerDeclaredUnit,
    Option<double> ServiceLifeYears,
    Option<double> BnYears,
    [property: OrderedEquality] Seq<IndicatorCell> Impacts) {
    public Seq<ImpactCategory> Coverage => Impacts.Map(static cell => cell.Category).Distinct();

    public bool Declares(ImpactCategory category) => Impacts.Exists(cell => cell.Category == category);
}

public sealed record EpdAnswer(Seq<EpdDeclaration> Rows, Seq<string> Warnings, bool Complete) {
    public static readonly EpdAnswer Empty = new(Seq<EpdDeclaration>(), Seq<string>(), Complete: true);
}

public sealed record CarbonQuery(
    Omf Omf, Map<string, Omf> OmfByMaterial, LciaMethod Method,
    double ReferencePeriodYears, Option<double> TargetKgCo2e) {
    public static CarbonQuery Of(Omf omf, LciaMethod method, double referencePeriodYears) =>
        new(omf, Map<string, Omf>(), method, referencePeriodYears, None);
}

public sealed record Envelope<T>(T Payload, Meta? Meta);

public sealed record Meta(Paging? Paging, Warning[]? Warnings, [property: JsonPropertyName("mf_hash")] string? MfHash);

public sealed record Paging(
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("total_pages")] int TotalPages,
    [property: JsonPropertyName("page_size")] int PageSize);

public sealed record Warning(string? Message, string? Code, string? Field);

public sealed record Measurement(double Mean, string? Unit,
    double? Rsd, [property: JsonPropertyName("dist")] string? Distribution);

public sealed record Amount(double? Qty, string? Unit);

public sealed record Org(string? Name, [property: JsonPropertyName("web_domain")] string? WebDomain);

public sealed record Standard([property: JsonPropertyName("short_name")] string? ShortName, string? Issuer);

public sealed record ScopeSet(
    [property: JsonPropertyName("A1A2A3")] Measurement? A1A2A3,
    [property: JsonPropertyName("A4")] Measurement? A4, [property: JsonPropertyName("A5")] Measurement? A5,
    [property: JsonPropertyName("B1")] Measurement? B1, [property: JsonPropertyName("B2")] Measurement? B2,
    [property: JsonPropertyName("B3")] Measurement? B3, [property: JsonPropertyName("B4")] Measurement? B4,
    [property: JsonPropertyName("B5")] Measurement? B5, [property: JsonPropertyName("B6")] Measurement? B6,
    [property: JsonPropertyName("B7")] Measurement? B7,
    [property: JsonPropertyName("C1")] Measurement? C1, [property: JsonPropertyName("C2")] Measurement? C2,
    [property: JsonPropertyName("C3")] Measurement? C3, [property: JsonPropertyName("C4")] Measurement? C4,
    [property: JsonPropertyName("D")] Measurement? D,
    [property: JsonPropertyName("Bn_years")] double? BnYears) {

    static readonly FrozenDictionary<LifecycleStage, Func<ScopeSet, Option<double>>> Banding =
        new KeyValuePair<LifecycleStage, Func<ScopeSet, Option<double>>>[] {
            new(LifecycleStage.A1A3, static s => Mean(s.A1A2A3)),
            new(LifecycleStage.A4,   static s => Mean(s.A4)),
            new(LifecycleStage.A5,   static s => Mean(s.A5)),
            new(LifecycleStage.B,    static s => Band(Mean(s.B1), Mean(s.B2), Mean(s.B3), Mean(s.B4), Mean(s.B5), Mean(s.B6), Mean(s.B7))),
            new(LifecycleStage.C,    static s => Band(Mean(s.C1), Mean(s.C2), Mean(s.C3), Mean(s.C4))),
            new(LifecycleStage.D,    static s => Mean(s.D)),
        }.ToFrozenDictionary();

    public Seq<IndicatorCell> Cells(ImpactCategory category) =>
        toSeq(LifecycleStage.Items).Choose(stage =>
            Banding[stage](this).Map(magnitude => new IndicatorCell(category, stage, magnitude)));

    static Option<double> Band(params ReadOnlySpan<Option<double>> modules) =>
        Seq(modules).Somes() is { IsEmpty: false } declared ? Some(declared.Sum()) : None;

    static Option<double> Mean(Measurement? m) => Optional(m).Map(static value => value.Mean);
}

public sealed record Epd(
    string? Id,
    string? Doctype,
    [property: JsonPropertyName("openepd_version")] string? OpenEpdVersion,
    [property: JsonPropertyName("valid_until")] Instant? ValidUntil,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit,
    [property: JsonPropertyName("kg_per_declared_unit")] Amount? KgPerDeclaredUnit,
    [property: JsonPropertyName("product_service_life_years")] double? ServiceLifeYears,
    Org? Manufacturer,
    [property: JsonPropertyName("program_operator")] Org? ProgramOperator,
    [property: JsonPropertyName("third_party_verifier")] Org? Verifier,
    Standard[]? Compliance,
    Dictionary<string, Dictionary<string, ScopeSet>> Impacts) {

    public (Seq<IndicatorCell> Cells, Seq<string> Unmapped) Indicators(LciaMethod method) =>
        Optional(Impacts.TryGetValue(method.WireKey, out Dictionary<string, ScopeSet> set) ? set : null)
            .Map(static indicators => toSeq(indicators).Fold(
                (Cells: Seq<IndicatorCell>(), Unmapped: Seq<string>()),
                static (state, row) => ImpactIndicator.Of(row.Key).Match(
                    Some: indicator => state with { Cells = state.Cells + row.Value.Cells(indicator.Category) },
                    None: () => state with { Unmapped = state.Unmapped.Add(row.Key) })))
            .IfNone((Seq<IndicatorCell>(), Seq<string>()));

    public Option<double> BnYears(LciaMethod method) =>
        Optional(Impacts.TryGetValue(method.WireKey, out Dictionary<string, ScopeSet> set) ? set : null)
            .Bind(static set => Optional(set.TryGetValue(ImpactIndicator.Gwp.Key, out ScopeSet gwp) ? gwp : null))
            .Bind(static gwp => Optional(gwp.BnYears));
}

public sealed record StatisticsDto(
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    [property: JsonPropertyName("standard_deviation")] double? StandardDeviation,
    [property: JsonPropertyName("epds_count")] int EpdsCount,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Ec3Service(HttpClient http, CacheRuntime cache, JsonSerializerContext json) {
    const string Ec3Owner = "ec3-epd";

    const int SearchPageSize = 100;

    const string GuardFields = "doctype,openepd_version";
    const string CandidateFields = $"{GuardFields},id,valid_until,declared_unit,kg_per_declared_unit";
    const string DocumentFields = $"{CandidateFields},product_service_life_years,manufacturer,program_operator,third_party_verifier,compliance,impacts";

    public async Task<Fin<EpdAnswer>> Resolve(EpdQuery query) => await query.Switch(
        products: async p => (await Cached<Epd[]>(query,
                $"/v2/epds/search?omf={Uri.EscapeDataString(p.Omf.ToValue())}&page_number=1&page_size={SearchPageSize}&fields={CandidateFields}",
                Unwrap<Epd[]>))
            .Map(page => Ec3Wire.Candidates(page, p.Method)),
        document: async d => (await Cached<Epd>(query, Identity("/epds", d.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, d.Method)),
        industry: async i => (await Cached<Epd>(query, Identity("/industry_epds", i.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, i.Method)),
        generic: async g => (await Cached<Epd>(query, Identity("/generic_estimates", g.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, g.Method)),
        statistic: async s => (await Cached<StatisticsDto>(query,
                $"/v2/epds/statistics?omf={Uri.EscapeDataString(s.Omf.ToValue())}&lcia_method={Uri.EscapeDataString(s.Method.WireKey)}",
                Unwrap<StatisticsDto>))
            .Map(page => Ec3Wire.Substitution(page)));

    static string Identity(string route, string uuid, string fields) =>
        $"{route}/{Uri.EscapeDataString(uuid)}?fields={fields}";

    async Task<Fin<(T Payload, Option<Meta> Meta)>> Cached<T>(
        EpdQuery query, string path,
        Func<HttpContent, JsonSerializerContext, ValueTask<Option<(T Payload, Option<Meta> Meta)>>> decode) where T : notnull {
        UInt128 slot = Slot(query);
        Cached<Fin<(T, Option<Meta>)>> held = await cache.Read(
            CacheLane.ModelResult,
            $"epd:{slot:x32}",
            (Http: http, Json: json, Path: path, Decode: decode, Slot: slot),
            static async (state, token) => new Cached<Fin<(T, Option<Meta>)>>(state.Slot, await Fetch(state.Http, state.Json, state.Path, state.Decode, token)),
            owners: Seq(Ec3Owner));
        return held.Echo == slot
            ? held.Value
            : Fin.Fail<(T, Option<Meta>)>(new ComputeFault.CacheCorrupt($"<ec3-slot-echo:{slot:x32}>"));
    }

    static async Task<Fin<(T Payload, Option<Meta> Meta)>> Fetch<T>(
        HttpClient http, JsonSerializerContext json, string path,
        Func<HttpContent, JsonSerializerContext, ValueTask<Option<(T Payload, Option<Meta> Meta)>>> decode,
        CancellationToken token) where T : notnull {
        using HttpResponseMessage response = await http.GetAsync(path, token);
        return response.IsSuccessStatusCode
            ? (await decode(response.Content, json)).ToFin((Error)new ComputeFault.AnalysisFailed(
                SolvePhase.Extraction, FailureKind.Foreign, $"<ec3-decode:{path}>"))
            : Fin.Fail<(T, Option<Meta>)>(Refusal(path, response.StatusCode, Optional(response.Headers.RetryAfter?.Delta)));
    }

    static Error Refusal(string path, HttpStatusCode status, Option<TimeSpan> retryAfter) =>
        status is HttpStatusCode.TooManyRequests && retryAfter.Case is TimeSpan window
            ? new ComputeFault.EndpointThrottled($"<ec3:429:{path}>", Duration.FromTimeSpan(window))
            : status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError
                ? new ComputeFault.EndpointUnreachable($"<ec3:{(int)status}:{path}>")
                : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<ec3:{(int)status}:{path}>", Some((int)status));

    static UInt128 Slot(EpdQuery query) =>
        ContentHash.Of(query, static (q, w) => q.Switch(
            products:  p => w.Ordinal(0).String(p.Omf.ToValue()).String(p.Method.WireKey),
            document:  d => w.Ordinal(1).String(d.Uuid).String(d.Method.WireKey),
            industry:  i => w.Ordinal(2).String(i.Uuid).String(i.Method.WireKey),
            generic:   g => w.Ordinal(3).String(g.Uuid).String(g.Method.WireKey),
            statistic: s => w.Ordinal(4).String(s.Omf.ToValue()).String(s.Method.WireKey)));

    static async ValueTask<Option<(T Payload, Option<Meta> Meta)>> Unwrap<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((Envelope<T>?)await content.ReadFromJsonAsync(typeof(Envelope<T>), json))
            .Bind(static envelope => Optional(envelope.Payload).Map(payload => (payload, Optional(envelope.Meta))));

    static async ValueTask<Option<(T Payload, Option<Meta> Meta)>> Bare<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((T?)await content.ReadFromJsonAsync(typeof(T), json)).Map(static payload => (payload, Option<Meta>.None));
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(EpdCodec))]
public static partial class Ec3Wire {
    const string Doctype = "openEPD";

    [MapProperty(nameof(Epd.Id), nameof(EpdDeclaration.Evidence), Use = nameof(EpdCodec.Evidence))]
    [MapValue(nameof(EpdDeclaration.Impacts), Use = nameof(EpdCodec.NoCells))]
    [MapValue(nameof(EpdDeclaration.BnYears), Use = nameof(EpdCodec.NoScale))]
    [MapperIgnoreSource(nameof(Epd.Impacts))]
    [MapperIgnoreSource(nameof(Epd.Doctype))]
    [MapperIgnoreSource(nameof(Epd.OpenEpdVersion))]
    public static partial EpdDeclaration Candidate(Epd row);

    [UserMapping]
    public static EpdAnswer Candidates((Epd[] Payload, Option<Meta> Meta) page, LciaMethod method) =>
        Guarded(toSeq(page.Payload), page.Meta, static (row, _) => Candidate(row), method);

    [UserMapping]
    public static EpdAnswer Declared((Epd Payload, Option<Meta> Meta) document, LciaMethod method) =>
        Guarded(Seq(document.Payload), document.Meta, static (row, m) => {
            (Seq<IndicatorCell> cells, Seq<string> unmapped) = row.Indicators(m);
            return Candidate(row) with { Impacts = cells, BnYears = row.BnYears(m) };
        }, method);

    [UserMapping]
    public static EpdAnswer Substitution((StatisticsDto Payload, Option<Meta> Meta) line) =>
        new(Seq(new EpdDeclaration(
                EpdCodec.Statistic(line.Payload),
                EpdCodec.Declared(line.Payload.DeclaredUnit), None, None, None,
                Seq(new IndicatorCell(ImpactCategory.GwpTotal, LifecycleStage.A1A3, line.Payload.ConservativeEstimate)))),
            EpdCodec.Warnings(line.Meta), Complete: true);

    static EpdAnswer Guarded(Seq<Epd> rows, Option<Meta> meta, Func<Epd, LciaMethod, EpdDeclaration> lower, LciaMethod method) =>
        new(rows.Filter(static row => StringComparer.Ordinal.Equals(row.Doctype, Doctype)).Map(row => lower(row, method)),
            EpdCodec.Warnings(meta),
            meta.Bind(static m => Optional(m.Paging)).Match(Some: static p => p.TotalPages <= 1, None: static () => true));
}

public static class EpdCodec {
    [NamedMapping(nameof(Evidence))]
    public static PropertyEvidence Evidence(Epd row) =>
        PropertyEvidence.Of(
            Issuer(row),
            row.Verifier is not null ? EvidenceGrade.Measured : EvidenceGrade.Import,
            reference: Optional(row.Id),
            validUntil: Optional(row.ValidUntil).Map(static v => v.InUtc().Date));

    public static PropertyEvidence Statistic(StatisticsDto line) =>
        PropertyEvidence.Of("ec3-statistics", EvidenceGrade.Catalogue,
            reference: Some($"conservative:n={line.EpdsCount}"));

    static string Issuer(Epd row) =>
        Optional(row.ProgramOperator?.WebDomain)
        | Optional(row.Manufacturer?.WebDomain)
        | Optional(row.Compliance).Bind(static rows => toSeq(rows).Choose(static s => Optional(s.Issuer)).Head)
        is { IsSome: true, Case: string domain } ? domain : "epd";

    [NamedMapping(nameof(Lifted))]
    public static Option<Instant> Lifted(Instant? at) => Optional(at);

    [NamedMapping(nameof(Declared))]
    public static Option<DeclaredAmount> Declared(Amount? amount) =>
        from qty in Optional(amount?.Qty)
        from token in Optional(amount?.Unit)
        from unit in DeclaredUnit.Of(token)
        select new DeclaredAmount(qty, unit);

    [NamedMapping(nameof(NoCells))]
    public static Seq<IndicatorCell> NoCells() => Seq<IndicatorCell>();

    [NamedMapping(nameof(NoScale))]
    public static Option<double> NoScale() => None;

    public static Seq<string> Warnings(Option<Meta> meta) =>
        meta.Bind(static m => Optional(m.Warnings)).Map(static rows =>
            toSeq(rows).Map(static w => $"{w.Code}:{w.Field}:{w.Message}")).IfNone(Seq<string>());
}
```

## [03]-[CARBON_RUNNER]

- Owner: `LifecycleAssessment.Rollup` the ONE aggregation fold both disciplines instantiate; `RunCarbon`/`RunCost` the two entries naming their aggregation, facts, and acceptance target; `LifecycleAssessment.EnrichCarbon` the async ingress that decodes resolved declarations onto the contract `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; `EpdLadder` the descent roster and `Descend`/`Freshest`/`ToEnvironmental`/`Normalize`/`ServiceLife` the per-ply resolution; `LifecycleGraphReads.TakeoffOf` the baked-quantity read; the `CarbonQuery` request input.
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock)` and its cost sibling both call `Rollup`, which folds one `AssemblyAggregator` arm over each target's `MaterialComposition` and baked `ElementTakeoff`; `EnrichCarbon(ElementGraph graph, Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, AssessmentRequest.Carbon request, IClock clock)` resolves undeclared plies down the ladder and returns a typed `(GraphDelta, PlyGaps)` result.
- Auto: `Rollup` resolves each ply's contract properties through one `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` keyed on the composition's native `MaterialId` (never a graph `NodeId`), and the per-element takeoff through `TakeoffOf`, so a baked and a catalogue-resolved declaration fold identically. `EnrichCarbon` enumerates the undeclared ply materials (the `MaterialId` set lacking the `Environmental` case, not the element's directly-associated material), resolves each down the `EpdQuery` descent — the category page's freshest non-expired candidate, that winner's own document, the industry-wide EPD, the generic estimate, then the category substitution line — `Normalize`s the declaration's cells to per-one-unit of its native basis and tags that `MeasurementBasis`, `ServiceLife`-scales the B stages against the request's reference study period, embeds every DECLARED indicator into the contract `(ImpactCategory × LifecycleStage)` matrix, and accumulates one monoid `GraphDelta` beside the `PlyGaps` ledger naming every ply the ingress could not resolve and why. Assessment stays a pure-sync graph read because every network call lives behind the explicit `EnrichCarbon` resolver, never inside the fold.
- Law: a SKIPPED ply is a counted fact, never silence. The ingress splits failure by posture — a TERMINAL refusal (no fresh declaration, an unresolvable declared-unit basis, a missing method indicator) skips the ply and records its `PlyGap`, so `RunCarbon` fails the still-undeclared ply at its own fold with the ledger already naming which plies the catalogue could not answer for; a TRANSIENT or THROTTLED refusal ABORTS the pipeline, because a partial delta erases the outage and masks the plies a re-drive would still resolve. The posture is READ off the fault the transport published, never re-derived by a predicate over exception types — one authority for one fact.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`/`HashMap`/`WriterT`/`TraverseM`/`PartitionFallible`), Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet`/`OfEnvironmental`/`PropertyEvidence`/`EvidenceGrade`, `MaterialPropertyAccess.Environmental`, `ImpactCategory`/`LifecycleStage`, `MeasurementBasis`, `MaterialId`, `NodeId`, `Node.Material`, `GraphDelta.Put`, `MeasureValue.OfSi`, `QuantityType`, `UnitProvenance`, `Dimension`), UnitsNet (via `MeasureValue.Of` — the declared-unit abbreviation → SI coercion the basis tagging rides), Rasm (kernel — `MeasureBundle`/`MassKind` the takeoff carrier, `Retriability` the published posture the descent reads), the `Analysis/aggregator` `AssemblyAggregator`/`ElementTakeoff`/`PlyQuantity`/`Plies`/`PlyGap`/`PlyGaps`/`PlyDiscipline`, the `Analysis/assessment` `AnalysisReads` bag-read owner, the `Runtime/admission#DISPATCH_SPINE` `ComputeFault`/`AssessmentInputReason`, NodaTime (`Instant`), BCL inbox (`ImmutableArray<double>` the contract impact-matrix store).
- Growth: a new lifecycle module is one contract `LifecycleStage` row (the cell set, the `ScopeSet` banding entry, and the aggregator fold widen by data); a new indicator is one contract `ImpactCategory` row with one `ImpactIndicator` transcription row; a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner; a richer selection (lowest-GWP, spec-matched) is one refinement of `Freshest`; a second carbon catalogue is one resolver binding.
- Boundary: the fold takes the RESOLVER — `Func<EpdQuery, Task<Fin<EpdAnswer>>>` — not a named service, so the ladder is provider-neutral by construction and a fixture, a second catalogue, or a cached corpus substitutes at the composition root. `AggregateEnvironmental` over each ply's baked `Environmental` mints the PRIMARY figure — the catalogue is the FALLBACK the async `EnrichCarbon` resolves, applied as a `GraphDelta` before the sync rollup, so a fully-declared model needs no network call; the takeoff reads the baked `Qto_*BaseQuantities` into the kernel `MeasureBundle`, whose `MassKind` discriminant survives on every row and whose absent domain answers `Option`, so a target with no base quantity fails rather than folding a zero takeoff into a zero carbon figure. The recycled-content and end-of-life-recovery fractions are the declaration's own scenario data and ride `Option` — the two `0.0` literals this replaces stated a MEASURED zero recovery on every catalogue-resolved ply, which is a circularity claim the EPD never made. The GWP/intensity stay raw kgCO2e through `DomainMeasure`, never `UnitsNet.Mass`; the runner reads the CONCRETE graph (above the contract), the write-back the `Analysis/assessment` spine's content-keyed `Node.Assessment`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EpdLadder {
    public static readonly EpdLadder Product = new("product", rank: 0);
    public static readonly EpdLadder Industry = new("industry", rank: 1);
    public static readonly EpdLadder Generic = new("generic", rank: 2);
    public static readonly EpdLadder Statistic = new("statistic", rank: 3);

    public int Rank { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class LifecycleAssessment {

    static Fin<AssessmentResult> Rollup<TResult>(
        ElementGraph graph, AssessmentRoute route, Seq<NodeId> targets, IClock clock,
        Func<MaterialComposition, ElementTakeoff, Fin<TResult>> aggregate,
        Func<NodeId, TResult, Fin<Seq<AssessmentFact>>> project,
        Func<TResult, Fin<Unit>> admit,
        Func<TResult, double> measure,
        Func<double, Option<double>, Option<double>> acceptance) =>
        targets
            .TraverseM(id =>
                from composition in graph.CompositionOf(id).ToFin(Missing(AssessmentInputReason.CompositionShape, id.ToValue()))
                from takeoff in graph.TakeoffOf(id)
                from result in aggregate(composition, takeoff)
                from _ in admit(result)
                from facts in project(id, result)
                select (Facts: facts, Total: measure(result), takeoff.Area))
            .As()
            .Bind(rows => AssessmentResult.Of(route,
                rows.Bind(static row => row.Facts),
                acceptance(rows.Sum(static row => row.Total),
                    rows.Map(static row => row.Area).Somes() is { IsEmpty: false } areas ? Some(areas.Sum()) : None),
                clock.GetCurrentInstant()));

    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock) =>
        Rollup(graph, request.Route, request.Targets, CarbonKey, clock,
            aggregate: (composition, takeoff) => AssemblyAggregator.AggregateEnvironmental(composition, Resolver(graph), Seq<PlyQuantity>(), takeoff),
            project: static (id, lifecycle) =>
                from whole in DomainMeasure($"{id.ToValue()}/whole-life-gwp", lifecycle.WholeLifeGwpKgCo2e, Kilograms)
                from intensity in Optional(lifecycle.EmbodiedCarbonIntensityKgCo2eM2)
                    .Map(value => DomainMeasure($"{id.ToValue()}/embodied-carbon-intensity", value, KilogramsPerSquareMetre).Map(static fact => Seq(fact)))
                    .IfNone(Fin.Succ(Seq<AssessmentFact>()))
                from recycled in Optional(lifecycle.RecycledContentFraction)
                    .Map(value => AssessmentFact.Ratio($"{id.ToValue()}/recycled-content", value).Map(static fact => Seq(fact)))
                    .IfNone(Fin.Succ(Seq<AssessmentFact>()))
                from stages in StageFacts(id, lifecycle.StageGwp)
                let gaps = lifecycle.Gaps.Map(gap => AssessmentFact.Text($"{id.ToValue()}/ply-gap", $"{gap.Material.ToValue()}:{gap.Discipline.Key}"))
                select Seq(whole) + intensity + recycled + stages + gaps,
            admit: static _ => Fin.Succ(unit),
            measure: static lifecycle => lifecycle.WholeLifeGwpKgCo2e,
            acceptance: (total, _) => request.Query.TargetKgCo2e.Filter(static target => target > 0.0).Map(target => total / target));

    public static async Task<Fin<(GraphDelta Delta, PlyGaps Gaps)>> EnrichCarbon(
        ElementGraph graph, Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, AssessmentRequest.Carbon request, IClock clock) {
        Instant now = clock.GetCurrentInstant();
        Seq<(Node.Material Material, Fin<MaterialPropertySet> Resolved)> resolved = Seq<(Node.Material, Fin<MaterialPropertySet>)>();
        foreach (Node.Material material in MissingDeclarations(graph, request.Targets)) {
            resolved = resolved.Add((material, await Descend(epds, request.Query, material, now, key)));
        }
        return resolved.Find(static row => Aborts(row.Resolved)).Match(
            Some: aborted => Fin.Fail<(GraphDelta, PlyGaps)>(aborted.Resolved.Match(Succ: static _ => Error.Empty, Fail: static error => error)),
            None: () => Fin.Succ(resolved.Fold(
                (Delta: GraphDelta.Empty, Gaps: PlyGaps.Empty),
                static (state, row) => row.Resolved.Match(
                    Succ: environmental => state with { Delta = state.Delta.Put(row.Material with { Properties = row.Material.Properties.Add(environmental) }) },
                    Fail: _ => state with { Gaps = state.Gaps.Combine(PlyGaps.Of(row.Material.MaterialKey, PlyDiscipline.Environmental)) }))));
    }

    static bool Aborts(Fin<MaterialPropertySet> resolved) =>
        resolved.Match(Succ: static _ => false, Fail: static error => error is Fault { Retriability: not Retriability.TerminalCase });

    static async Task<Fin<MaterialPropertySet>> Descend(
        Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, CarbonQuery query, Node.Material material, Instant now) {
        Omf omf = query.OmfByMaterial.Find(material.MaterialKey.ToValue()).IfNone(query.Omf);
        Fin<EpdAnswer> page = await epds(new EpdQuery.Products(omf, query.Method));
        if (Aborts(page)) { return Fin.Fail<MaterialPropertySet>(page.Match(Succ: static _ => Error.Empty, Fail: static e => e)); }
        Option<string> winner = page.ToOption().Bind(answer => Freshest(answer.Rows, now).Bind(static row => row.Evidence.Reference));
        foreach (EpdQuery rung in Rungs(winner, omf, query.Method)) {
            Fin<EpdAnswer> answer = await epds(rung);
            if (Aborts(answer)) { return Fin.Fail<MaterialPropertySet>(answer.Match(Succ: static _ => Error.Empty, Fail: static e => e)); }
            Option<MaterialPropertySet> admitted = answer.ToOption()
                .Bind(rows => rows.Rows.Find(row => row.Declares(ImpactCategory.GwpTotal)))
                .Bind(row => ToEnvironmental(row, query, key).ToOption());
            if (admitted.Case is MaterialPropertySet resolved) { return Fin.Succ(resolved); }
        }
        return Fin.Fail<MaterialPropertySet>(Missing(AssessmentInputReason.PlyPropertyAbsent, material.MaterialKey.ToValue()));
    }

    static Seq<EpdQuery> Rungs(Option<string> winner, Omf omf, LciaMethod method) =>
        winner.Map(uuid => Seq<EpdQuery>(
                new EpdQuery.Document(uuid, method),
                new EpdQuery.Industry(uuid, method),
                new EpdQuery.Generic(uuid, method)))
            .IfNone(Seq<EpdQuery>())
            .Add(new EpdQuery.Statistic(omf, method));

    static Seq<Node.Material> MissingDeclarations(ElementGraph graph, Seq<NodeId> targets) =>
        targets.Choose(graph.CompositionOf)
            .Bind(static c => c.Materials)
            .Choose(mid => graph.Material(mid))
            .Filter(static m => m.Properties.Environmental.IsNone)
            .Distinct();

    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, mid.ToValue()));

    static Option<EpdDeclaration> Freshest(Seq<EpdDeclaration> rows, Instant now) {
        LocalDate today = now.InUtc().Date;
        Seq<EpdDeclaration> live = rows.Filter(row => row.Evidence.ValidUntil.ForAll(until => today <= until));
        return toSeq(live.Filter(static row => row.Evidence.ValidUntil.IsSome)
                .OrderByDescending(static row => row.Evidence.ValidUntil.IfNone(default(LocalDate)))).Head
            | live.Head;
    }

    static Fin<MaterialPropertySet> ToEnvironmental(EpdDeclaration declaration, CarbonQuery query) =>
        from basis in Normalize(declaration)
        let scaled = ServiceLife(declaration, query.ReferencePeriodYears)
        from admitted in MaterialPropertySet.OfEnvironmental(
            basis.Basis,
            Matrix(scaled.Map(cell => cell with { PerDeclaredUnit = cell.PerDeclaredUnit / basis.PerUnit })),
            recycledContent: None, endOfLifeRecovery: None,
            declaration.Evidence)
        select admitted;

    static ImmutableArray<double> Matrix(Seq<IndicatorCell> cells) {
        double[] matrix = new double[MaterialPropertySet.Environmental.MatrixArity];
        cells.Iter(cell => matrix[(cell.Category.Key * LifecycleStage.Items.Count) + cell.Stage.Key] = cell.PerDeclaredUnit);
        return [.. matrix];
    }

    static Seq<IndicatorCell> ServiceLife(EpdDeclaration declaration, double referencePeriodYears) {
        double useScale = declaration.BnYears.Filter(static years => years > 0.0)
            .Map(years => referencePeriodYears / years).IfNone(1.0);
        double replacements = declaration.ServiceLifeYears.Filter(static years => years > 0.0)
            .Map(life => Math.Max(0.0, Math.Ceiling(referencePeriodYears / life) - 1.0)).IfNone(0.0);
        Seq<IndicatorCell> scaled = declaration.Impacts.Map(cell =>
            cell.Stage == LifecycleStage.B ? cell with { PerDeclaredUnit = cell.PerDeclaredUnit * useScale } : cell);
        return replacements > 0.0
            ? scaled + declaration.Impacts
                .Filter(static cell => cell.Stage == LifecycleStage.A1A3)
                .Map(cell => new IndicatorCell(cell.Category, LifecycleStage.B, cell.PerDeclaredUnit * replacements))
            : scaled;
    }

    static Fin<(MeasurementBasis Basis, double PerUnit)> Normalize(EpdDeclaration declaration) =>
        declaration.DeclaredUnit
            .Bind(static declared => declared.Unit.Basis.Map(basis => (Basis: basis, Amount: declared)))
            .Filter(static row => row.Amount.Qty > 0.0)
            .Map(static row => (row.Basis, PerUnit: row.Amount.Qty))
            | declaration.KgPerDeclaredUnit.Filter(static kg => kg.Qty > 0.0)
                .Map(static kg => (MeasurementBasis.PerKg, PerUnit: kg.Qty))
        is { IsSome: true, Case: (MeasurementBasis, double) resolved }
            ? Fin.Succ(resolved)
            : Fin.Fail<(MeasurementBasis, double)>(Missing(AssessmentInputReason.DeclaredUnitBasis,
                declaration.DeclaredUnit.Map(static d => d.Unit.Key).IfNone(string.Empty)));

    static Fin<Seq<AssessmentFact>> StageFacts(NodeId id, ImmutableArray<double> stageGwp) =>
        toSeq(LifecycleStage.Items).TraverseM(stage => DomainMeasure($"{id.ToValue()}/gwp-{stage.Module}", stageGwp[stage.Key], Kilograms)).As();

    static Fin<AssessmentFact> DomainMeasure(string name, double si, string unit) =>
        MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, si, Some(UnitProvenance.Label(unit))).Map(value => AssessmentFact.Measure(name, value));

    static ComputeFault Missing(AssessmentInputReason reason, string witness) => new ComputeFault.AssessmentInputMissing(reason, witness);

    const string Kilograms = "kgCO2e";
    const string KilogramsPerSquareMetre = "kgCO2e/m²";
}

public static class LifecycleGraphReads {
    extension(ElementGraph graph) {
        public Fin<ElementTakeoff> TakeoffOf(NodeId element) =>
            Seq((MassKind.Area, graph.Magnitude(element, QuantityRows.SurfaceArea)),
                    (MassKind.Volume, graph.Magnitude(element, QuantityRows.Volume)))
                .Choose(static row => row.Item2.Map(magnitude => (row.Item1, magnitude)))
                is { IsEmpty: false } held
                ? MeasureBundle.Of(held, TakeoffKey).ToFin()
                    .Map(measures => new ElementTakeoff(measures, graph.Magnitude(element, QuantityRows.NestWasteArea)))
                : Fin.Fail<ElementTakeoff>(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.MeasureAbsent, element.ToValue()));
    }
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup entry over the shared `Rollup` fold; `CostBudget` the acceptance derivation over the request's two budget columns.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock)` names the `AggregateCost` arm, the currency guard, the three cost facts, and the budget acceptance; every other step is the one `Rollup` skeleton the carbon runner also instantiates.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost pipeline spans all composition cases (a single material or a profile member has a unit supply/install cost); a new acceptance modality is one `AssessmentRequest.Cost` budget column with one `CostBudget` arm.
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the `request.Currency` is load-bearing — the aggregated cost is guarded to it (a material priced in a different `Currency` fails, since the fold carries no exchange rate), so the request currency is a real validation target, never a decorative field; the per-ply quantity derives from the contract `Cost.Basis` against the baked takeoff (or a `PlyQuantity` override); a material with no `Cost` case fails. Where the caller states a budget the governing ratio is REAL and the verdict a genuine `Satisfied`/`Marginal`/`Exceeded` band: `BudgetTotal` is the absolute cap on the target set's in-place cost, `BudgetPerArea` the rate against the same takeoff area the aggregator distributes cost by, the absolute column winning where both ride; a request carrying NEITHER column reports an ABSENT ratio, the same no-target spelling the carbon runner now holds. The request's budget columns are `decimal` because money is exact and a binary double re-rounds every currency figure it touches; the aggregator's own totals are `double` and the ratio widens once at the divide — so the exactness claim covers the ACCEPTANCE columns alone, and no exact-money accumulator exists or is claimed on this page.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class LifecycleAssessment {
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock) =>
        Rollup(graph, request.Route, request.Targets, CostKey, clock,
            aggregate: (composition, takeoff) => AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), takeoff),
            project: static (id, cost) => AssessmentFact.Rows(
                DomainMeasure($"{id.ToValue()}/supply-total", cost.SupplyTotal, cost.Currency.Key),
                DomainMeasure($"{id.ToValue()}/install-total", cost.InstallTotal, cost.Currency.Key),
                DomainMeasure($"{id.ToValue()}/in-place-total", cost.TotalInPlace, cost.Currency.Key)),
            admit: cost => cost.Currency.Key == request.Currency
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(Missing(AssessmentInputReason.CurrencyMismatch, $"{cost.Currency.Key}<>{request.Currency}")),
            measure: static cost => cost.TotalInPlace,
            acceptance: (total, areaM2) => CostBudget(request, areaM2).Map(budget => total / budget));

    static Option<double> CostBudget(AssessmentRequest.Cost request, Option<double> areaM2) =>
        (request.BudgetTotal.Map(static total => (double)total)
         | (from rate in request.BudgetPerArea from area in areaM2 select (double)rate * area))
            .Filter(static budget => double.IsFinite(budget) && budget > 0.0);
}
```
