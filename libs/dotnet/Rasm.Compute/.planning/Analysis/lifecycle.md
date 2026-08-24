# [COMPUTE_LIFECYCLE]

Rasm.Compute lifecycle runner owns the `Discipline.Environmental` and `Discipline.Cost` arms of the assessment rail — the EN 15978 embodied-carbon takeoff and the supply/install/lifecycle cost rollup. Both are ONE rollup over the `Analysis/aggregator` fold, discriminated by the aggregation each request names: the fold reads the seam `MaterialComposition`, distributes each ply by the element's baked `Qto_*BaseQuantities` takeoff, and projects a receipt into the uniform fact stream. Where a ply carries no baked EN 15978 declaration, the async `EnrichCarbon` ingress resolves one from the EC3 / openEPD REST catalogue through the fallback ladder, applied as a `GraphDelta` before the pure-sync carbon rollup.

One typed `HttpClient` rides the shared `CacheLane` under a framed content key, and each runner returns one `AssessmentResult` fact stream the `Analysis/assessment` spine writes back, the governing ratio the whole-life carbon (or in-place cost) against the acceptance target the request carries — ABSENT where it carries none.

## [01]-[INDEX]

- [02]-[EC3_BOUNDARY]: `EpdQuery`→`EpdDeclaration` the resolver contract and `Ec3Service` the openEPD adapter satisfying it over the shared cache lane, the closed unit and indicator rosters, and the raw-kgCO2e GWP discipline.
- [03]-[CARBON_RUNNER]: the EN 15978 takeoff and `EnrichCarbon` the async EC3 ingress resolving each undeclared ply down one descent ladder.
- [04]-[COST_RUNNER]: the supply/install/lifecycle rollup over the same fold skeleton, guarded to the requested `Currency`.

## [02]-[EC3_BOUNDARY]

- Owner: `EpdQuery` the closed request `[Union]` whose cases ARE the rungs of one descent ladder and `EpdDeclaration`/`DeclaredAmount` the provider-neutral answer, together the `Func<EpdQuery, Task<Fin<EpdAnswer>>>` resolver contract every carbon fold takes; `Ec3Service` the openEPD ADAPTER satisfying it; `Ec3Wire` the Mapperly wire→neutral mapper with `EpdCodec` its `[NamedMapping]` converter roster registered whole through `[UseStaticMapper]`; the openEPD wire-type family (`Epd`/`ScopeSet`/`Measurement`/`StatisticsDto`/`Envelope<T>`/`Meta`/`Paging`/`Warning`/`Amount`/`Org`); `DeclaredUnit` the closed openEPD unit roster carrying each token's seam `MeasurementBasis`; `ImpactIndicator` the closed transcription from the openEPD `ImpactSet` indicator names onto the seam `ImpactCategory` rows; the `LciaMethod` `[SmartEnum<string>]` impact-method roster with its citation `Key` and wire `WireKey` columns; the `CarbonQuery` request input the `AssessmentRequest.Carbon` case carries.
- Entry: `Ec3Service.Resolve(EpdQuery query)` → `Task<Fin<EpdAnswer>>` is the adapter's ONE read, its generated total `Switch` binding the category page search, the by-identity document, the industry-wide EPD, the generic estimate, and the category statistic onto five GET-only legs. A `429` carrying a delta `Retry-After` lands `ComputeFault.EndpointThrottled` publishing the server's own window; a window-less rate limit, a request timeout, and a `5xx` land `ComputeFault.EndpointUnreachable` (`Transient`); every deterministic `4xx` and every decode refusal land `AnalysisFailed` inheriting the kernel `Terminal` posture. Retriability is PUBLISHED at the fault and executed at the root-bound handler, so no arm here spells a predicate over exception types.
- Auto: the five legs share ONE polymorphic `Cached<T>` fold parameterized by the decode shape (`Unwrap<T>` for the `{payload, meta}` message envelope, `Bare<T>` for the by-identity documents); the cache stores a `Cached<Fin<T>>` envelope so a SUCCESS holds for the provider's revision cadence while a DETERMINISTIC refusal holds only for the lane's negative window and a transient one is never written at all — the boundary-crossing exception carrier this replaces existed solely to make `HybridCache` skip a write. The slot derives through the kernel `ContentHash.Of<TState>` over the leg's own framed fields beside the response `meta.mf_hash` the server resolved the filter to, so two OMF strings the server normalizes to one filter share a slot and a length-framed key cannot collide two legs whose colon-joined concatenation once did. Every module a declaration carries bands onto the seam `LifecycleStage` roster and every indicator onto `ImpactCategory` through generated projections keyed by row, so the wire's members map by data rather than by a hand-summed fixed-slot literal, and a row the seam does not carry lands a NAMED degrade rather than a silent zero.
- Law: KEY PRESENCE is the coverage census. An openEPD scope member the wire never declared is UNDECLARED ABSENCE, never a zero — the `?? 0.0` collapse this replaces erased the census before the sum, so a partial EPD and a genuinely zero-impact module published the same number and the seam matrix then zero-filled every undeclared indicator with nothing left to say which. Every declared cell rides `EpdDeclaration.Impacts` as an `(indicator, stage)` KEY, and the coverage a consumer reads DERIVES from that key set.
- Law: the reference study period is the B-stage's own scale. `product_service_life_years` and each scope's `Bn_years` are the two columns EN 15978 B1–B7 arithmetic needs — a use-stage value declared over one year and summed straight into a sixty-year study reports a building's whole maintenance and operational carbon as a single year of it, and a product whose service life is shorter than the study period is REPLACED, its product stage re-incurred at B4. Absent either column the B stages carry their declared magnitude and the result states the unscaled basis rather than fabricating a period.
- Packages: `System.Net.Http` (typed client + `ReadFromJsonAsync(Type, JsonSerializerContext)`), `System.Text.Json` (source-generated context, AOT-safe), Microsoft.Extensions.Caching.Hybrid (reached through the `Rasm.AppHost` `Runtime/resources#CACHE_LANES` `CacheSurface`, never a cache instance), Riok.Mapperly (`[Mapper]`, `[MapProperty(Use = …)]`, `[MapValue]`, `[UserMapping]`, `[NamedMapping]`, `[UseStaticMapper]`, `[MapperIgnoreSource]` — the reader-free wire lowering), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[Union]`, `[ObjectFactory<string>]`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`HashMap`/`Validation`), Generator.Equals (`[Equatable]` — the impact-map and stage-vector reference-equality repair), NodaTime (`Instant`/`Duration`), Rasm (kernel — `ContentHash.Of<TState>`/`CanonicalWriter` the framed slot key, `Retriability` the published posture, `Op`), Rasm.Element (project — `LifecycleStage`/`ImpactCategory` the banding projections are generated over, `MeasurementBasis`, `PropertyEvidence`/`EvidenceGrade`), Rasm.AppHost (project — the `CacheLane` descriptor and its `CacheSurface`), BCL inbox; no NuGet SDK to pin (REST integration).
- Growth: a new LCIA method is one `LciaMethod` row carrying its citation and wire spellings; a new decoded openEPD member is one source-gen context property and one banding entry; a new lifecycle module is one seam `LifecycleStage` row with one banding entry here, a new EN 15804 indicator one seam `ImpactCategory` row with one `ImpactIndicator` row; a new ladder rung is one `EpdQuery` case the descent's own roster orders; a SECOND carbon provider is one type satisfying the resolver contract with zero edit to the folds — the boundary widens by row and by adapter, never by a second HTTP client and never a per-endpoint cache path.
- Boundary: the carbon folds take the RESOLVER, never this class — an assessment that names its provider cannot be run against a second catalogue, a fixture, or a cached corpus without editing the fold. Only the GET read surface is consumed (Rasm is a carbon consumer, never a publisher), and the openEPD wire family stays adapter-local: `Epd`/`ScopeSet`/`Amount` never cross the resolver contract, `EpdDeclaration` carrying the declared indicator cells, the two basis witnesses, the service life, and the `PropertyEvidence` the folds read. Cache tags reach `HybridCache` only through `CacheLane.Tag` and entry lifetime only through the lane's own `Entry` — this lane names an owner key and the lane frames it, where a page-local `HybridCacheEntryOptions` beside raw string tags was a THIRD cache authority against the folder's one-owner ruling and a tag space no `Invalidate` could reach. GWP `Measurement.Mean` is kgCO2e per declared unit and is not a `UnitsNet` quantity — it crosses interior signatures as a raw `double` and lands as a dimensionless `MeasureValue` labeled `kgCO2e` through `DomainMeasure`, never `UnitsNet.Mass` and never the abbreviation-resolving `MeasureValue.Of` (which rejects `kgCO2e`). `LciaMethod` carries its wire spelling as its OWN column (the `Model/providers#EP_AXIS` `WireKey` precedent): the citation a report renders and the token `impacts[method]` and `lcia_method=` are keyed by are two facts, and one string serving both makes a renamed citation silently miss every impact lookup. `LciaMethod` stays CLOSED and absence rides `Option` at the read — the wire's own `Unknown LCIA` bucket is a REFUSAL here, because a declaration whose method nothing named cannot be compared against one whose method the route pinned. `doctype`/`openepd_version` gate the decoder before any impact read, so a re-shaped future document refuses rather than decoding half. Provenance is the declaration's own orgs — `manufacturer`, `program_operator`, `third_party_verifier` and the `compliance` standards — never the `"epd"` literal a `[MapValue]` constant once stamped on every row alike. Hyphenated LCIA scope and indicator keys (`A1A2A3`, `B1`…`B7`, `C1`…`C4`, `gwp-fossil`, `ADP-mineral`, `ETP-fw`, `HTP-c`) require `[JsonPropertyName]` aliases; the `fields` query mask trims each leg to its own projection, so a category page carries candidate identity and basis alone and the winner's impacts are fetched once by identity rather than for every row the page returned; `meta.warnings[]` fold into the receipt as soft notes and `meta.paging` states whether the one probed page WAS the whole candidate set, because a freshest-of-100 pick silently presented as a freshest-of-all is the selection defect no downstream number reveals.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Key carries the CITATION a report renders; WireKey the token `impacts[method]` and `lcia_method=` are keyed by. One
// string serving both makes a renamed citation silently miss every impact lookup, so each row declares its own crossing
// spelling exactly as the EP axis does. The roster is the openEPD `LCIAMethod` vocabulary WHOLE under ONE stated
// admission rule — every published member except the `Unknown LCIA` bucket, which names no method to compare against
// and so refuses here. A hand subset is the derivation defect that rule forecloses: the carrier publishes five CML
// editions and a roster carrying one of them silently refuses four real declarations. The python data branch reads
// this vocabulary by importing it, so both ends state one truth and a carrier release lands as one row here.
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

// The openEPD ImpactSet indicator axis transcribed onto the seam ImpactCategory rows — the ONE correspondence, and
// the reason the environmental case stops being a GWP-only slice of a thirteen-indicator matrix. Both `gwp` and the
// hyphen-split components land, because EN 15804+A2 declares the total AND its fossil/biogenic/land-use parts as
// separate indicators the seam already carries as separate rows. A wire indicator the seam holds no row for degrades
// with its own magnitude named, never a silent drop and never a fabricated seam member.
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

// The closed openEPD declared/measurement unit vocabulary and the seam MeasurementBasis each token grounds a takeoff
// on. FOUR tokens carry a basis the seam declares; the rest are spec or intensity units a per-element quantity cannot
// be distributed over, so they REFUSE by name rather than defaulting to volume — a per-MPa or per-degree declaration
// scaled as if it were per-cubic-metre is a carbon figure wrong by whatever the element's volume happens to be.
// `use` grounds on the count basis exactly as `item` does: a functional use IS a countable occurrence.
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

    // None where the seam declares no basis for the token — the read is an honest absence the caller refuses on,
    // never a defaulted basis that mis-scales the whole declaration.
    public Option<MeasurementBasis> Basis { get; }

    public static Option<DeclaredUnit> Of(string token) => toSeq(Items).Find(row => row.Key == token);
}

// The Open Material Filter grammar admitted ONCE at the boundary. An OMF is a published query language — a category
// call, field predicates, and a closing `!pragma oMF(version)` — and a bare string in its place lets a malformed
// filter reach a token-metered endpoint, spend the call, and return an empty page indistinguishable from a category
// with no products. The pragma version is what makes a server-side grammar bump a REFUSAL rather than a silent
// re-interpretation of every predicate in the filter.
[ObjectFactory<string>]
[ValueObject<string>]
public sealed partial class Omf {
    static Validation<string> ValidateFactoryArguments(ref string value) =>
        value is { Length: > 0 } filter && filter.StartsWith(OmfPrefix, StringComparison.Ordinal) && filter.Contains(OmfPragma, StringComparison.Ordinal)
            ? Validation.Ok
            : new ValidationError(message: $"<omf-grammar:{value}>");

    const string OmfPrefix = "!EC3 ";
    const string OmfPragma = "!pragma oMF(";
}

// ONE carbon-resolution request: the case is the rung of the descent ladder, its payload the coordinates, and the
// ORDER is the ladder — each rung names the rung it falls to, so the descent is a fold over data rather than three
// hand functions whose shared failure arm was byte-identical in each. The catalogue publishes five reads and the
// ladder walks four of them: a product page, that winner's document, the industry-wide EPD averaging its sector, the
// generic average dataset, then the category statistic — from the most specific evidence to the least, and each step
// down is a stated loss of specificity a receipt names.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EpdQuery {
    private EpdQuery() { }

    // Products reads a category page of CANDIDATES — identity, expiry, and basis alone, no impacts.
    public sealed record Products(Omf Omf, LciaMethod Method) : EpdQuery;
    // Document fetches the winning candidate's full declaration once by identity rather than per page row.
    public sealed record Document(string Uuid, LciaMethod Method) : EpdQuery;
    // Industry-wide EPD weights roughly a score of product declarations across one sector.
    public sealed record Industry(string Uuid, LciaMethod Method) : EpdQuery;
    // Generic estimate is the average dataset a category with no product declaration falls to.
    public sealed record Generic(string Uuid, LciaMethod Method) : EpdQuery;
    // Statistic reads the category substitution line — the conservative percentile, the last rung.
    public sealed record Statistic(Omf Omf, LciaMethod Method) : EpdQuery;
}

// --- [MODELS] ------------------------------------------------------------------------------
// Magnitude plus its admitted unit — the two facts a basis resolves from. The unit is the CLOSED roster token, so a
// declaration whose unit the vocabulary does not carry refuses at decode rather than at the normalization three
// rungs later, where the ply had already been silently skipped with no evidence of why.
public readonly record struct DeclaredAmount(double Qty, DeclaredUnit Unit);

// One declared indicator cell: which EN 15804+A2 indicator, which life-cycle module, and the magnitude per declared
// unit. Rows exist ONLY where the wire declared them — presence IS the coverage census, so an absent cell is
// undeclared absence and a present zero is a measured zero, two states the `?? 0.0` collapse made one.
public readonly record struct IndicatorCell(ImpactCategory Category, LifecycleStage Stage, double PerDeclaredUnit);

// Provider-neutral environmental declaration. Evidence ABSORBS the source/reference/expiry triple onto the seam
// carrier that already models it — grade, attestation, and the audit link ride with it and the fold stops
// re-spelling three columns the seam owns. `Impacts` is the declared cell set and `Coverage` derives from it, never
// a hand-kept mirror. `ServiceLifeYears` and `BnYears` are the two columns the B-stage arithmetic needs; both are
// optional because a product declaration may state neither, and the fold then reports an unscaled basis.
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

// Every read answers a declaration set BESIDE the soft evidence the envelope carried: the server's warnings, and
// whether the probed page was the whole candidate set. A resolver contract returning declarations alone dropped
// both, so a truncated category page and a complete one were the same answer and the freshest-of-100 pick read as
// freshest-of-all.
public sealed record EpdAnswer(Seq<EpdDeclaration> Rows, Seq<string> Warnings, bool Complete) {
    public static readonly EpdAnswer Empty = new(Seq<EpdDeclaration>(), Seq<string>(), Complete: true);
}

// Carbon request input carries the category OMF as default scope, an optional per-material OMF override (a
// multi-material assembly resolves each ply from its OWN EC3 category — concrete, insulation, and gypsum never
// share one EPD), the LCIA method, the EN 15978 reference study period the B stages scale against, and the design
// target the verdict ratios against. The target is OPTIONAL because an informational rollup is a real request and
// its honest answer is an ABSENT ratio, never a `0.0` the verdict bands `Satisfied`.
public sealed record CarbonQuery(
    Omf Omf, Map<string, Omf> OmfByMaterial, LciaMethod Method,
    double ReferencePeriodYears, Option<double> TargetKgCo2e) {
    public static CarbonQuery Of(Omf omf, LciaMethod method, double referencePeriodYears) =>
        new(omf, Map<string, Omf>(), method, referencePeriodYears, None);
}

// Source-generated System.Text.Json wire projection admits only consumed members; the `fields` query mask trims the
// rest server-side. `meta` is DECODED: paging states whether one page was the whole set, warnings are the soft
// degradation the receipt carries, and `mf_hash` is the server's own resolved-filter identity the cache slot folds.
public sealed record Envelope<T>(T Payload, Meta? Meta);

public sealed record Meta(Paging? Paging, Warning[]? Warnings, [property: JsonPropertyName("mf_hash")] string? MfHash);

public sealed record Paging(
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("total_pages")] int TotalPages,
    [property: JsonPropertyName("page_size")] int PageSize);

public sealed record Warning(string? Message, string? Code, string? Field);

// Scope leaf. `rsd` and `dist` are the declaration's OWN uncertainty model and ride into the evidence grade rather
// than being discarded: a mean carrying a wide relative standard deviation is weaker evidence than one that does not.
public sealed record Measurement(double Mean, string? Unit,
    double? Rsd, [property: JsonPropertyName("dist")] string? Distribution);

// openEPD unit-bearing quantity carries declared_unit or kg_per_declared_unit as a magnitude plus its roster unit
// token. The qty/unit keys decode under the context camelCase policy (no alias).
public sealed record Amount(double? Qty, string? Unit);

// Provenance org: the natural key is the web domain, so two spellings of one manufacturer's display name resolve to
// one issuer.
public sealed record Org(string? Name, [property: JsonPropertyName("web_domain")] string? WebDomain);

public sealed record Standard([property: JsonPropertyName("short_name")] string? ShortName, string? Issuer);

// EN 15978 life-cycle modules band onto the seam LifecycleStage roster. The fifteen [JsonPropertyName] members ARE
// the wire and stay verbatim; the BANDING is one generated projection keyed by seam stage row, so a new stage row is
// one entry rather than a re-cut fixed-slot literal. `Bn_years` scopes every use-stage value in the set: B6 declared
// over one year is a per-year magnitude, and summing it into a sixty-year study without the scale reports one year
// of operational carbon as the whole life. A1A2A3 is the cradle-to-gate product total, D the beyond-system credit.
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

    // ONE banding owner: each seam stage row names the wire members that sum onto it, and each member answers an
    // Option so a module the wire never declared contributes NO cell. The `?? 0.0` this replaces made a partial
    // EPD and a zero-impact module the same number before the sum, erasing the presence census the peer branch's
    // decoder keeps and this leg's contract promises.
    static readonly FrozenDictionary<LifecycleStage, Func<ScopeSet, Option<double>>> Banding =
        new KeyValuePair<LifecycleStage, Func<ScopeSet, Option<double>>>[] {
            new(LifecycleStage.A1A3, static s => Mean(s.A1A2A3)),
            new(LifecycleStage.A4,   static s => Mean(s.A4)),
            new(LifecycleStage.A5,   static s => Mean(s.A5)),
            new(LifecycleStage.B,    static s => Band(Mean(s.B1), Mean(s.B2), Mean(s.B3), Mean(s.B4), Mean(s.B5), Mean(s.B6), Mean(s.B7))),
            new(LifecycleStage.C,    static s => Band(Mean(s.C1), Mean(s.C2), Mean(s.C3), Mean(s.C4))),
            new(LifecycleStage.D,    static s => Mean(s.D)),
        }.ToFrozenDictionary();

    // Cells for ONE indicator: every seam stage the wire declared something for, and nothing for the rest. The
    // lookup is CLOSED over the seam roster — a stage row the table has no entry for is a coding gap, not an
    // undeclared module, and the two are indistinguishable behind a `TryGetValue ... : 0.0` miss arm.
    public Seq<IndicatorCell> Cells(ImpactCategory category) =>
        toSeq(LifecycleStage.Items).Choose(stage =>
            Banding[stage](this).Map(magnitude => new IndicatorCell(category, stage, magnitude)));

    // A band is declared when ANY of its modules is: the sum runs over what the wire carried, and a band no module
    // declared stays absent rather than summing to a zero indistinguishable from a measured one.
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

    // Every declared indicator for the pinned method, transcribed onto the seam rows. The method's WIRE spelling
    // keys the outer map, never its citation, so a re-worded citation cannot silently miss the lookup; an indicator
    // outside the transcription is reported by name rather than dropped, because a catalogue that added a row is a
    // fact the operator must see and not a magnitude the fold may discard.
    public (Seq<IndicatorCell> Cells, Seq<string> Unmapped) Indicators(LciaMethod method) =>
        Optional(Impacts.TryGetValue(method.WireKey, out Dictionary<string, ScopeSet> set) ? set : null)
            .Map(static indicators => toSeq(indicators).Fold(
                (Cells: Seq<IndicatorCell>(), Unmapped: Seq<string>()),
                static (state, row) => ImpactIndicator.Of(row.Key).Match(
                    Some: indicator => state with { Cells = state.Cells + row.Value.Cells(indicator.Category) },
                    None: () => state with { Unmapped = state.Unmapped.Add(row.Key) })))
            .IfNone((Seq<IndicatorCell>(), Seq<string>()));

    // The use-stage scale rides the method's own gwp scope set, which is where the wire declares it.
    public Option<double> BnYears(LciaMethod method) =>
        Optional(Impacts.TryGetValue(method.WireKey, out Dictionary<string, ScopeSet> set) ? set : null)
            .Bind(static set => Optional(set.TryGetValue(ImpactIndicator.Gwp.Key, out ScopeSet gwp) ? gwp : null))
            .Bind(static gwp => Optional(gwp.BnYears));
}

// Category-scoped GWP substitution line: the EC3 conservative estimate is the 80th-percentile kgCO2e per declared
// unit a ply with no product declaration falls back to, and the sample composition beside it is what makes the
// substitution's WEAKNESS legible — a percentile drawn from four declarations is not the one drawn from four
// hundred. declared_unit carries the basis the normalization reads; a category statistic carries no
// kg_per_declared_unit, so a mass-based category needs the ply density.
public sealed record StatisticsDto(
    [property: JsonPropertyName("conservative_estimate")] double ConservativeEstimate,
    [property: JsonPropertyName("standard_deviation")] double? StandardDeviation,
    [property: JsonPropertyName("epds_count")] int EpdsCount,
    [property: JsonPropertyName("declared_unit")] Amount? DeclaredUnit);

// --- [SERVICES] ----------------------------------------------------------------------------
// Ec3Service is the openEPD ADAPTER: it satisfies the resolver contract and owns every wire spelling behind it, so
// the carbon folds name a delegate and this class is one binding at the composition root. `CacheSurface` is the one
// cache owner it reaches through; it holds no HybridCache of its own and mints no entry policy.
public sealed class Ec3Service(HttpClient http, CacheRuntime cache, JsonSerializerContext json) {
    // Owner keys cross the lane seam, never tags: the lane frames this owner into its own tag space, so one
    // Invalidate cuts every EPD entry and leaves the lane's model-result entries untouched.
    const string Ec3Owner = "ec3-epd";

    // Candidate search reads one page wide: a single token charge surfaces enough category rows for the freshness
    // pick, never a per-ply multi-page crawl. `meta.paging` then STATES whether that page was the whole set — the
    // pager the catalogue publishes is deliberately not walked, and the completeness flag is what keeps that
    // decision auditable instead of invisible.
    const int SearchPageSize = 100;

    // Candidate rows carry identity, basis, and the decoder guard ONLY — the winner's impacts are fetched once by
    // identity, so a hundred-row page never pays for ninety-nine impact matrices the fold discards.
    const string GuardFields = "doctype,openepd_version";
    const string CandidateFields = $"{GuardFields},id,valid_until,declared_unit,kg_per_declared_unit";
    const string DocumentFields = $"{CandidateFields},product_service_life_years,manufacturer,program_operator,third_party_verifier,compliance,impacts";

    // ONE read over the generated total Switch: a new rung is a case the compiler demands an arm for. LCIA method
    // is behavior-bearing on the wire — lcia_method selects which method's statistics line the service computes —
    // so the cache identity and the remote request agree and no leg can label one method's estimate as another's.
    public async Task<Fin<EpdAnswer>> Resolve(EpdQuery query) => await query.Switch(
        products: async p => (await Cached<Epd[]>(query,
                $"/v2/epds/search?omf={Uri.EscapeDataString(p.Omf.Value)}&page_number=1&page_size={SearchPageSize}&fields={CandidateFields}",
                Unwrap<Epd[]>))
            .Map(page => Ec3Wire.Candidates(page, p.Method)),
        document: async d => (await Cached<Epd>(query, Identity("/epds", d.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, d.Method)),
        industry: async i => (await Cached<Epd>(query, Identity("/industry_epds", i.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, i.Method)),
        generic: async g => (await Cached<Epd>(query, Identity("/generic_estimates", g.Uuid, DocumentFields), Bare<Epd>))
            .Map(row => Ec3Wire.Declared(row, g.Method)),
        statistic: async s => (await Cached<StatisticsDto>(query,
                $"/v2/epds/statistics?omf={Uri.EscapeDataString(s.Omf.Value)}&lcia_method={Uri.EscapeDataString(s.Method.WireKey)}",
                Unwrap<StatisticsDto>))
            .Map(page => Ec3Wire.Substitution(page)));

    static string Identity(string route, string uuid, string fields) =>
        $"{route}/{Uri.EscapeDataString(uuid)}?fields={fields}";

    // ONE polymorphic fetch+cache fold over the decode shape. The cached value is the `Cached<Fin<T>>` envelope the
    // folder's model lane already landed: a SUCCESS holds for the provider's revision cadence, a DETERMINISTIC
    // refusal holds under the lane's negative window so a `404` for a retired UUID is not re-bought on every ply,
    // and a TRANSIENT or THROTTLED refusal is never written — which is the entire job the boundary-crossing
    // exception carrier this replaces existed to do, spelled as a rail instead of a throw across a cache factory.
    // The slot derives through the kernel framed writer, so no two legs can collide the way a colon-joined
    // concatenation of unframed fields did, and the server's own resolved-filter hash folds beside the leg's fields.
    // The surface SCOPES the key against the lane and the ambient tenant and frames the owner into the lane's own
    // tag space, so this leg spells neither a scoped key nor a tag — the two things a page-local HybridCache
    // dependency once forced it to spell, and the reason its tags reached nothing any Invalidate could cut.
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
        // The echo catches a cross-key L2 corruption the content key alone cannot: a payload whose stored echo is
        // not this slot is discarded rather than served as this query's answer.
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

    // A status is a DISCRIMINANT, not a slug, and the posture it selects is PUBLISHED on the fault rather than
    // re-derived by a predicate at each descent point. A 429 carrying a delta Retry-After names the window the
    // re-drive rail must honour; a window-less rate limit, a request timeout, and a server fault recover on a later
    // attempt; a not-found, an unauthorized, and a malformed filter answer identically forever. The date form of
    // Retry-After deliberately degrades to the transient arm, because a clock-skewed absolute date forges a
    // negative wait.
    static Error Refusal(string path, HttpStatusCode status, Option<TimeSpan> retryAfter) =>
        status is HttpStatusCode.TooManyRequests && retryAfter.Case is TimeSpan window
            ? new ComputeFault.EndpointThrottled($"<ec3:429:{path}>", Duration.FromTimeSpan(window))
            : status is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout or >= HttpStatusCode.InternalServerError
                ? new ComputeFault.EndpointUnreachable($"<ec3:{(int)status}:{path}>")
                : new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<ec3:{(int)status}:{path}>", Some((int)status));

    // Length-framed, endianness-fixed, and self-delimiting: the case ordinal supplies the leg discriminant the
    // colon-joined string once smuggled into a prefix, and the raw XxHash128 over a UTF-16 machine-endian span it
    // replaces keyed one query two ways across architectures.
    static UInt128 Slot(EpdQuery query) =>
        ContentHash.Of(query, static (q, w) => q.Switch(
            products:  p => w.Ordinal(0).String(p.Omf.Value).String(p.Method.WireKey),
            document:  d => w.Ordinal(1).String(d.Uuid).String(d.Method.WireKey),
            industry:  i => w.Ordinal(2).String(i.Uuid).String(i.Method.WireKey),
            generic:   g => w.Ordinal(3).String(g.Uuid).String(g.Method.WireKey),
            statistic: s => w.Ordinal(4).String(s.Omf.Value).String(s.Method.WireKey)));

    // Source-generated decode rides the (Type, JsonSerializerContext) pair — one of the three contract-bound forms
    // every serializer verb admits — so the closed Envelope<Epd[]>/Envelope<StatisticsDto>/Epd contracts the context
    // registers resolve without a reflection fallback and without hand-casting a JsonTypeInfo per call.
    static async ValueTask<Option<(T Payload, Option<Meta> Meta)>> Unwrap<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((Envelope<T>?)await content.ReadFromJsonAsync(typeof(Envelope<T>), json))
            .Bind(static envelope => Optional(envelope.Payload).Map(payload => (payload, Optional(envelope.Meta))));

    static async ValueTask<Option<(T Payload, Option<Meta> Meta)>> Bare<T>(HttpContent content, JsonSerializerContext json) where T : notnull =>
        Optional((T?)await content.ReadFromJsonAsync(typeof(T), json)).Map(static payload => (payload, Option<Meta>.None));
}

// Wire -> neutral lowering is COMPILER-PROOF: `Candidate` generates member-by-member under
// RequiredMappingStrategy.Both, so an EpdDeclaration column added later FAILS THE BUILD instead of silently
// carrying a default. The mapping is READER-FREE, so RMG020 keeps its source-side force and the [MapperIgnoreSource]
// roster is compiler inventory, not authored prose.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(EpdCodec))]
public static partial class Ec3Wire {
    // The document decoder guard: a payload whose doctype is not openEPD, or whose version this decoder was not
    // written against, refuses BEFORE any impact read rather than decoding half a document into a carbon figure.
    const string Doctype = "openEPD";

    // Candidate rows carry identity and basis ONLY — the absent cell set is a [MapValue] constant, and the Impacts
    // matrix is the one source member the candidate projection deliberately drops. Provenance rides the
    // declaration's own orgs through the evidence converter, never a literal stamped on every row alike.
    [MapProperty(nameof(Epd.Id), nameof(EpdDeclaration.Evidence), Use = nameof(EpdCodec.Evidence))]
    [MapValue(nameof(EpdDeclaration.Impacts), Use = nameof(EpdCodec.NoCells))]
    [MapValue(nameof(EpdDeclaration.BnYears), Use = nameof(EpdCodec.NoScale))]
    [MapperIgnoreSource(nameof(Epd.Impacts))]
    [MapperIgnoreSource(nameof(Epd.Doctype))]
    [MapperIgnoreSource(nameof(Epd.OpenEpdVersion))]
    public static partial EpdDeclaration Candidate(Epd row);

    // One page of candidates beside the envelope's own soft evidence: the completeness flag states whether the
    // freshness pick ranged over the whole category or over the first hundred rows of it.
    [UserMapping]
    public static EpdAnswer Candidates((Epd[] Payload, Option<Meta> Meta) page, LciaMethod method) =>
        Guarded(toSeq(page.Payload), page.Meta, static (row, _) => Candidate(row), method);

    // Document lowering keeps the sanctioned post-`with` for the method-selected cells — never a
    // [MapPropertyFromSource] whole-source reader, whose presence would suppress RMG020 for the whole mapping. An
    // indicator the transcription has no seam row for rides out as a warning rather than vanishing.
    [UserMapping]
    public static EpdAnswer Declared((Epd Payload, Option<Meta> Meta) document, LciaMethod method) =>
        Guarded(Seq(document.Payload), document.Meta, static (row, m) => {
            (Seq<IndicatorCell> cells, Seq<string> unmapped) = row.Indicators(m);
            return Candidate(row) with { Impacts = cells, BnYears = row.BnYears(m) };
        }, method);

    // Category statistics resolve A1-A3 alone (the conservative estimate is a cradle-to-gate substitution value)
    // and carry no kg_per_declared_unit — a single-cell fan-out no member mapping spells, so this leg stays the
    // admitted [UserMapping] hand fold beside the generated sibling. The sample size and dispersion ride the
    // evidence grade, because a percentile over four declarations is weaker evidence than one over four hundred.
    [UserMapping]
    public static EpdAnswer Substitution((StatisticsDto Payload, Option<Meta> Meta) line) =>
        new(Seq(new EpdDeclaration(
                EpdCodec.Statistic(line.Payload),
                EpdCodec.Declared(line.Payload.DeclaredUnit), None, None, None,
                Seq(new IndicatorCell(ImpactCategory.GwpTotal, LifecycleStage.A1A3, line.Payload.ConservativeEstimate)))),
            EpdCodec.Warnings(line.Meta), Complete: true);

    // ONE guard for every document-bearing leg: a payload the decoder was not written against yields NO rows, so
    // the ladder descends rather than folding a half-read document into a carbon figure.
    static EpdAnswer Guarded(Seq<Epd> rows, Option<Meta> meta, Func<Epd, LciaMethod, EpdDeclaration> lower, LciaMethod method) =>
        new(rows.Filter(static row => StringComparer.Ordinal.Equals(row.Doctype, Doctype)).Map(row => lower(row, method)),
            EpdCodec.Warnings(meta),
            meta.Bind(static m => Optional(m.Paging)).Match(Some: static p => p.TotalPages <= 1, None: static () => true));
}

// [NamedMapping] converters own every wire-shape lift, registered whole through [UseStaticMapper] so the nullable
// Instant, Amount, and org members cross with no per-member configuration. Each attribute name is the spelling the
// `Use =` reference resolves — `nameof` yields the METHOD name, so the two must match exactly, and the lower-cased
// attribute this repairs bound nothing.
public static class EpdCodec {
    // Evidence ABSORBS the identity triple the seam already models. Grade DERIVES from what the declaration
    // proves: a third-party-verified declaration carries a measured LCA behind an independent check, an
    // unverified one is an import of the publisher's own word, and a category statistic is a catalogue average.
    // Source is the issuing org's own web domain — the natural key two spellings of one manufacturer resolve to —
    // never the `"epd"` literal a [MapValue] constant once stamped identically on every row.
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

    // The magnitude and the ROSTER unit admit together: an amount whose unit the closed vocabulary does not carry
    // is no declared amount at all, so the read is absent here rather than a string that fails to ground a basis
    // three rungs down with no evidence of which token was at fault.
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

- Owner: `LifecycleAssessment.Rollup` the ONE aggregation fold both disciplines instantiate; `RunCarbon`/`RunCost` the two entries naming their aggregation, facts, and acceptance target; `LifecycleAssessment.EnrichCarbon` the async ingress that decodes resolved declarations onto the seam `MaterialPropertySet.Environmental` and returns a graph-enriching `GraphDelta`; `EpdLadder` the descent roster and `Descend`/`Freshest`/`ToEnvironmental`/`Normalize`/`ServiceLife` the per-ply resolution; `LifecycleGraphReads.TakeoffOf` the baked-quantity read; the `CarbonQuery` request input.
- Entry: `public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock)` and its cost sibling both call `Rollup`, which folds one `AssemblyAggregator` arm over each target's `MaterialComposition` and baked `ElementTakeoff`; `EnrichCarbon(ElementGraph graph, Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, AssessmentRequest.Carbon request, IClock clock, Op key)` resolves undeclared plies down the ladder and returns a typed `(GraphDelta, PlyGaps)` rail.
- Auto: `Rollup` resolves each ply's seam properties through one `Func<MaterialId, Fin<Seq<MaterialPropertySet>>>` keyed on the composition's native `MaterialId` (never a graph `NodeId`), and the per-element takeoff through `TakeoffOf`, so a baked and a catalogue-resolved declaration fold identically. `EnrichCarbon` enumerates the undeclared ply materials (the `MaterialId` set lacking the `Environmental` case, not the element's directly-associated material), resolves each down the `EpdQuery` descent — the category page's freshest non-expired candidate, that winner's own document, the industry-wide EPD, the generic estimate, then the category substitution line — `Normalize`s the declaration's cells to per-one-unit of its native basis and tags that `MeasurementBasis`, `ServiceLife`-scales the B stages against the request's reference study period, embeds every DECLARED indicator into the seam `(ImpactCategory × LifecycleStage)` matrix, and accumulates one monoid `GraphDelta` beside the `PlyGaps` ledger naming every ply the ingress could not resolve and why. Assessment stays a pure-sync graph read because every network call lives behind the explicit `EnrichCarbon` resolver, never inside the fold.
- Law: a SKIPPED ply is a counted fact, never silence. The ingress splits failure by posture — a TERMINAL refusal (no fresh declaration, an unresolvable declared-unit basis, a missing method indicator) skips the ply and records its `PlyGap`, so `RunCarbon` rails the still-undeclared ply at its own fold with the ledger already naming which plies the catalogue could not answer for; a TRANSIENT or THROTTLED refusal ABORTS the rail, because a partial delta erases the outage and masks the plies a re-drive would still resolve. The posture is READ off the fault the transport published, never re-derived by a predicate over exception types — one authority for one fact.
- Packages: LanguageExt.Core (`Fin`/`Seq`/`Option`/`Map`/`HashMap`/`WriterT`/`TraverseM`/`PartitionFallible`), Rasm.Element (project — `ElementGraph`, `MaterialComposition`, `MaterialPropertySet`/`OfEnvironmental`/`PropertyEvidence`/`EvidenceGrade`, `MaterialPropertyAccess.Environmental`, `ImpactCategory`/`LifecycleStage`, `MeasurementBasis`, `MaterialId`, `NodeId`, `Node.Material`, `GraphDelta.Put`, `MeasureValue.OfSi`, `QuantityType`, `UnitProvenance`, `Dimension`), UnitsNet (via `MeasureValue.Of` — the declared-unit abbreviation → SI coercion the basis tagging rides), Rasm (kernel — `MeasureBundle`/`MassKind` the takeoff carrier, `Retriability` the published posture the descent reads, `Op`), the `Analysis/aggregator` `AssemblyAggregator`/`ElementTakeoff`/`PlyQuantity`/`Plies`/`PlyGap`/`PlyGaps`/`PlyDiscipline`, the `Analysis/assessment` `AnalysisReads` bag-read owner, the `Runtime/admission#DISPATCH_SPINE` `ComputeFault`/`AssessmentInputReason`, NodaTime (`Instant`), BCL inbox (`ImmutableArray<double>` the seam impact-matrix store).
- Growth: a new lifecycle module is one seam `LifecycleStage` row (the cell set, the `ScopeSet` banding entry, and the aggregator fold widen by data); a new indicator is one seam `ImpactCategory` row with one `ImpactIndicator` transcription row; a biogenic-carbon credit or a circularity index is one fact over the same aggregation, never a parallel carbon owner; a richer selection (lowest-GWP, spec-matched) is one refinement of `Freshest`; a second carbon catalogue is one resolver binding.
- Boundary: the fold takes the RESOLVER — `Func<EpdQuery, Task<Fin<EpdAnswer>>>` — not a named service, so the ladder is provider-neutral by construction and a fixture, a second catalogue, or a cached corpus substitutes at the composition root. `AggregateEnvironmental` over each ply's baked `Environmental` mints the PRIMARY figure — the catalogue is the FALLBACK the async `EnrichCarbon` resolves, applied as a `GraphDelta` before the sync rollup, so a fully-declared model needs no network call; the takeoff reads the baked `Qto_*BaseQuantities` into the kernel `MeasureBundle`, whose `MassKind` discriminant survives on every row and whose absent domain answers `Option`, so a target with no base quantity rails rather than folding a zero takeoff into a zero carbon figure. The recycled-content and end-of-life-recovery fractions are the declaration's own scenario data and ride `Option` — the two `0.0` literals this replaces stated a MEASURED zero recovery on every catalogue-resolved ply, which is a circularity claim the EPD never made. The GWP/intensity stay raw kgCO2e through `DomainMeasure`, never `UnitsNet.Mass`; the runner reads the CONCRETE graph (above the seam), the write-back the `Analysis/assessment` spine's content-keyed `Node.Assessment`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The descent order as DATA. Each rung names how to build its query from the ply's coordinates and what a
// successful answer means, so the ladder is one fold and the shared failure arm — byte-identical in each of the
// three hand functions this replaces — exists once.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EpdLadder {
    public static readonly EpdLadder Product = new("product", rank: 0);
    public static readonly EpdLadder Industry = new("industry", rank: 1);
    public static readonly EpdLadder Generic = new("generic", rank: 2);
    public static readonly EpdLadder Statistic = new("statistic", rank: 3);

    // Rank is the SPECIFICITY loss a receipt reports: a ply answered at rank 0 carries its own manufacturer's
    // declaration, one answered at rank 3 carries a category percentile, and a reviewer reads the difference.
    public int Rank { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    static readonly Op CarbonKey = Op.Of(name: nameof(RunCarbon));
    static readonly Op CostKey = Op.Of(name: nameof(RunCost));

    // THE aggregation fold both disciplines instantiate. RunCarbon and RunCost once carried a byte-identical
    // skeleton — fold the targets, resolve the composition, read the takeoff, aggregate, project facts, accumulate
    // a total — differing only in which aggregator arm ran, which facts it projected, and what the total was
    // ratioed against. Those three are parameters; the skeleton is one.
    static Fin<AssessmentResult> Rollup<TReceipt>(
        ElementGraph graph, AssessmentRoute route, Seq<NodeId> targets, Op key, IClock clock,
        Func<MaterialComposition, ElementTakeoff, Fin<TReceipt>> aggregate,
        Func<NodeId, TReceipt, Fin<Seq<AssessmentFact>>> project,
        Func<TReceipt, Fin<Unit>> admit,
        Func<TReceipt, double> measure,
        Func<double, Option<double>, Option<double>> acceptance) =>
        targets
            .TraverseM(id =>
                from composition in graph.CompositionOf(id).ToFin(Missing(AssessmentInputReason.CompositionShape, id.Value))
                from takeoff in graph.TakeoffOf(id)
                from receipt in aggregate(composition, takeoff)
                from _ in admit(receipt)
                from facts in project(id, receipt)
                select (Facts: facts, Total: measure(receipt), takeoff.Area))
            .As()
            .Bind(rows => AssessmentResult.Of(route,
                rows.Bind(static row => row.Facts),
                // The set-wide area sums the takeoffs that HELD one. A target whose bundle carries no area domain
                // contributes nothing rather than a zero, so a per-area budget rates against the area actually
                // taken off and an all-arealess set carries no rate to band against at all.
                acceptance(rows.Sum(static row => row.Total),
                    rows.Map(static row => row.Area).Somes() is { IsEmpty: false } areas ? Some(areas.Sum()) : None),
                clock.GetCurrentInstant(), key));

    // Pure synchronous carbon assessment over the baked or EC3-enriched composition. The governing ratio is the
    // whole-life carbon against the design target, ABSENT where the request carries none — never a `double.NaN`
    // standing in for an option the sibling runner already spelled correctly two hundred lines away.
    public static Fin<AssessmentResult> RunCarbon(ElementGraph graph, AssessmentRequest.Carbon request, IClock clock) =>
        Rollup(graph, request.Route, request.Targets, CarbonKey, clock,
            aggregate: (composition, takeoff) => AssemblyAggregator.AggregateEnvironmental(composition, Resolver(graph), Seq<PlyQuantity>(), takeoff),
            project: static (id, lifecycle) =>
                from whole in DomainMeasure($"{id.Value}/whole-life-gwp", lifecycle.WholeLifeGwpKgCo2e, Kilograms)
                from intensity in Optional(lifecycle.EmbodiedCarbonIntensityKgCo2eM2)
                    .Map(value => DomainMeasure($"{id.Value}/embodied-carbon-intensity", value, KilogramsPerSquareMetre).Map(static fact => Seq(fact)))
                    .IfNone(Fin.Succ(Seq<AssessmentFact>()))
                from recycled in Optional(lifecycle.RecycledContentFraction)
                    .Map(value => AssessmentFact.Ratio($"{id.Value}/recycled-content", value).Map(static fact => Seq(fact)))
                    .IfNone(Fin.Succ(Seq<AssessmentFact>()))
                from stages in StageFacts(id, lifecycle.StageGwp)
                // Every unresolved ply rides out as a NAMED row: which material lacked which discipline, the
                // question a carbon reviewer asks first and one that a partial total can never answer.
                let gaps = lifecycle.Gaps.Map(gap => AssessmentFact.Text($"{id.Value}/ply-gap", $"{gap.Material.Value}:{gap.Discipline.Key}"))
                select Seq(whole) + intensity + recycled + stages + gaps,
            admit: static _ => Fin.Succ(unit),
            measure: static lifecycle => lifecycle.WholeLifeGwpKgCo2e,
            acceptance: (total, _) => request.Query.TargetKgCo2e.Filter(static target => target > 0.0).Map(target => total / target));

    // Async ingress resolves each undeclared ply down the ladder, decodes the declaration into the seam matrix, and
    // accumulates the enriching delta the composition root applies before the sync RunCarbon, so a fully-declared
    // model needs no network call. The resolver is a PARAMETER: the ladder is domain logic over a contract.
    // The gap ledger returns BESIDE the delta because a caller that only learns "some plies were skipped" when the
    // sync fold later rails cannot tell an unpublished material from a catalogue outage that already passed.
    public static async Task<Fin<(GraphDelta Delta, PlyGaps Gaps)>> EnrichCarbon(
        ElementGraph graph, Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, AssessmentRequest.Carbon request, IClock clock, Op key) {
        Instant now = clock.GetCurrentInstant();
        // Ingress boundary: a serial, rate-limited walk over a token-metered endpoint. Serialization is deliberate
        // and the ORDER is the fold's, so a per-ply await is the shape rather than a defect.
        Seq<(Node.Material Material, Fin<MaterialPropertySet> Resolved)> resolved = Seq<(Node.Material, Fin<MaterialPropertySet>)>();
        foreach (Node.Material material in MissingDeclarations(graph, request.Targets)) {
            resolved = resolved.Add((material, await Descend(epds, request.Query, material, now, key)));
        }
        // PartitionFallible splits the walk once: an ABORTING posture on any ply fails the whole rail, every
        // terminal refusal becomes a counted gap, and the resolved plies fold into the delta. The mutable
        // accumulator with an inline Match reassignment this replaces dropped the skipped ply's fault entirely.
        return resolved.Find(static row => Aborts(row.Resolved)).Match(
            Some: aborted => Fin.Fail<(GraphDelta, PlyGaps)>(aborted.Resolved.Match(Succ: static _ => Error.Empty, Fail: static error => error)),
            None: () => Fin.Succ(resolved.Fold(
                (Delta: GraphDelta.Empty, Gaps: PlyGaps.Empty),
                static (state, row) => row.Resolved.Match(
                    Succ: environmental => state with { Delta = state.Delta.Put(row.Material with { Properties = row.Material.Properties.Add(environmental) }) },
                    Fail: _ => state with { Gaps = state.Gaps.Combine(PlyGaps.Of(row.Material.MaterialKey, PlyDiscipline.Environmental)) }))));
    }

    // ONE posture read for the whole ingress: the transport PUBLISHED whether its refusal is re-offerable, so the
    // `static bool Retryable(Error)` predicate over exception types this deletes had nothing left to spell — and
    // two authorities for one fact is exactly how a caller-cancelled read once classified as a retryable outage.
    static bool Aborts(Fin<MaterialPropertySet> resolved) =>
        resolved.Match(Succ: static _ => false, Fail: static error => error is Fault { Retriability: not Retriability.TerminalCase });

    // The descent, as a fold over the rung roster. Each rung either answers with a usable declaration or hands the
    // next rung its coordinates; a re-offerable outage exits the fold immediately, because descending past an
    // outage substitutes a category average for a product declaration the catalogue would have served.
    static async Task<Fin<MaterialPropertySet>> Descend(
        Func<EpdQuery, Task<Fin<EpdAnswer>>> epds, CarbonQuery query, Node.Material material, Instant now, Op key) {
        Omf omf = query.OmfByMaterial.Find(material.MaterialKey.Value).IfNone(query.Omf);
        Fin<EpdAnswer> page = await epds(new EpdQuery.Products(omf, query.Method));
        if (Aborts(page)) { return Fin.Fail<MaterialPropertySet>(page.Match(Succ: static _ => Error.Empty, Fail: static e => e)); }
        Option<string> winner = page.Match(Succ: answer => Freshest(answer.Rows, now).Bind(static row => row.Evidence.Reference), Fail: static _ => None);
        foreach (EpdQuery rung in Rungs(winner, omf, query.Method)) {
            Fin<EpdAnswer> answer = await epds(rung);
            if (Aborts(answer)) { return Fin.Fail<MaterialPropertySet>(answer.Match(Succ: static _ => Error.Empty, Fail: static e => e)); }
            Option<MaterialPropertySet> admitted = answer.ToOption()
                .Bind(rows => rows.Rows.Find(row => row.Declares(ImpactCategory.GwpTotal)))
                .Bind(row => ToEnvironmental(row, query, key).ToOption());
            if (admitted.Case is MaterialPropertySet resolved) { return Fin.Succ(resolved); }
        }
        return Fin.Fail<MaterialPropertySet>(Missing(AssessmentInputReason.PlyPropertyAbsent, material.MaterialKey.Value));
    }

    // The winner's own document first, then the two averaged declarations, then the category percentile. A rung
    // whose coordinate the page never surfaced is SKIPPED rather than queried with an empty identity.
    static Seq<EpdQuery> Rungs(Option<string> winner, Omf omf, LciaMethod method) =>
        winner.Map(uuid => Seq<EpdQuery>(
                new EpdQuery.Document(uuid, method),
                new EpdQuery.Industry(uuid, method),
                new EpdQuery.Generic(uuid, method)))
            .IfNone(Seq<EpdQuery>())
            .Add(new EpdQuery.Statistic(omf, method));

    // Undeclared ply materials derive from each target composition's native MaterialId set, resolved to its
    // material node, filtered to those lacking the seam Environmental case — the plies the aggregator folds, NOT
    // the element's directly-associated container material (a LayerSet's plies, not the layer-set node).
    static Seq<Node.Material> MissingDeclarations(ElementGraph graph, Seq<NodeId> targets) =>
        targets.Choose(graph.CompositionOf)
            .Bind(static c => c.Materials)
            .Choose(mid => graph.Material(mid))
            .Filter(static m => m.Properties.Environmental.IsNone)
            .Distinct();

    // Seam-keyed resolver maps a ply MaterialId to its material node's property set, railing the missing-input
    // fault on an absent material so the aggregator reads the composition's OWN plies by native key.
    static Func<MaterialId, Fin<Seq<MaterialPropertySet>>> Resolver(ElementGraph graph) =>
        mid => graph.Material(mid).Map(static m => m.Properties).ToFin(Missing(AssessmentInputReason.PlyPropertyAbsent, mid.Value));

    // Freshest non-stale candidate: a dated expiry must not have passed, and among the dated ones the latest wins.
    // An UNDATED declaration is admitted but ranks BELOW every dated one — the `IfNone(Instant.MaxValue)` this
    // replaces read one default two ways, admitting an undated row as non-expiring and then ranking it above every
    // dated row in the same expression, so a declaration that stated no expiry beat one that stated a valid future.
    static Option<EpdDeclaration> Freshest(Seq<EpdDeclaration> rows, Instant now) {
        LocalDate today = now.InUtc().Date;
        Seq<EpdDeclaration> live = rows.Filter(row => row.Evidence.ValidUntil.ForAll(until => today <= until));
        return toSeq(live.Filter(static row => row.Evidence.ValidUntil.IsSome)
                .OrderByDescending(static row => row.Evidence.ValidUntil.IfNone(default(LocalDate)))).Head
            | live.Head;
    }

    // ONE decode for every rung. Normalize the declaration's cells to per-one-unit of its native basis (tagged with
    // the MeasurementBasis the fold scales by), scale the use stages against the reference study period, embed
    // every DECLARED indicator into the full seam (ImpactCategory x LifecycleStage) matrix, and admit through
    // OfEnvironmental. The seam's arity invariant zero-fills whatever the declaration did not declare — that is the
    // seam's own law — so the COVERAGE rides beside it as evidence rather than being inferred from a cell that
    // reads zero for both an undeclared indicator and a measured one.
    static Fin<MaterialPropertySet> ToEnvironmental(EpdDeclaration declaration, CarbonQuery query, Op key) =>
        from basis in Normalize(declaration)
        let scaled = ServiceLife(declaration, query.ReferencePeriodYears)
        from admitted in MaterialPropertySet.OfEnvironmental(
            basis.Basis,
            Matrix(scaled.Map(cell => cell with { PerDeclaredUnit = cell.PerDeclaredUnit / basis.PerUnit })),
            recycledContent: None, endOfLifeRecovery: None,
            key,
            declaration.Evidence)
        select admitted;

    // Full-matrix embed over the seam's own row-major layout: every declared cell writes at its own
    // (indicator, stage) offset, so a declaration carrying acidification and ozone depletion beside carbon lands
    // all three. The carbon-only helper this replaces wrote one indicator row and zeroed twelve, which made the
    // seam's thirteen-indicator store a one-of-thirteen slice at every catalogue-resolved ply.
    static ImmutableArray<double> Matrix(Seq<IndicatorCell> cells) {
        double[] matrix = new double[MaterialPropertySet.Environmental.MatrixArity];
        cells.Iter(cell => matrix[(cell.Category.Key * LifecycleStage.Count) + cell.Stage.Index] = cell.PerDeclaredUnit);
        return [.. matrix];
    }

    // The EN 15978 B-stage scale, the catalogue's own stated obligation for this page. A use-stage value declared
    // over `Bn_years` scales to the study period, and a product whose service life is shorter than the period is
    // REPLACED — each replacement re-incurring the product stage at B4. Absent either column the stages carry their
    // declared magnitude unchanged, because a fabricated period is a worse answer than a stated unscaled one.
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

    // Normalize resolves the per-one-unit divisor and the MeasurementBasis the fold scales by — the declared unit's
    // own roster basis first, then the kg-per-unit chain. NONE of the fallback guesswork: a unit the roster carries
    // no basis for REFUSES by name, so a per-MPa or per-hour declaration reports the token that could not ground
    // rather than being silently skipped or defaulted to volume. Density is not read here — a per-kg basis resolves
    // to mass at aggregation (volume x the ply Mechanical.Density).
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
        toSeq(LifecycleStage.Items).TraverseM(stage => DomainMeasure($"{id.Value}/gwp-{stage.Module}", stageGwp[stage.Index], Kilograms)).As();

    // GWP and in-place cost are domain-basis scalars (kgCO2e, kgCO2e/m2, a currency code), not UnitsNet quantities
    // — a dimensionless MeasureValue carrying the domain label, never the abbreviation-resolving MeasureValue.Of
    // (which rejects kgCO2e). The mint is the seam's Label provenance case: Scalar carries no registry row, so the
    // labeled mint is admitted, the value stays dimensionless, the token rides CanonicalUnit, and Fin rails a NaN at
    // the finite gate. Label on a registry-named type refuses by construction — a per-site token cannot fork one.
    static Fin<AssessmentFact> DomainMeasure(string name, double si, string unit) =>
        MeasureValue.OfSi(QuantityType.Scalar, Dimension.Dimensionless, si, Some(UnitProvenance.Label(unit))).Map(value => AssessmentFact.Measure(name, value));

    static ComputeFault Missing(AssessmentInputReason reason, string witness) => new ComputeFault.AssessmentInputMissing(reason, witness);

    const string Kilograms = "kgCO2e";
    const string KilogramsPerSquareMetre = "kgCO2e/m²";
}

// Element geometric takeoff distributes GWP and cost per ply, reading the baked Qto_*BaseQuantities through the one
// Analysis/assessment AnalysisReads owner over the QuantityRows net-over-gross chains — every Qto set scanned, so a
// wall/slab/beam reads without a per-type accessor. A target with no baked base quantity rails the missing input.
public static class LifecycleGraphReads {
    extension(ElementGraph graph) {
        // Every row keys through a Rasm.Element-declared static and every read composes the one AnalysisReads
        // owner, so the net-over-gross preference is stated once on the declarer and this reader shares one
        // spelling with the non-referencing Bim and Fabrication writers. The takeoff is the KERNEL MeasureBundle:
        // the Kind discriminant survives on every row and an unheld domain answers Option, where the three
        // mutually-exclusive scalar columns it replaces forged a zero at every absent edge and re-derived the
        // discriminant the Kind already carried.
        public Fin<ElementTakeoff> TakeoffOf(NodeId element) =>
            Seq((MassKind.Area, graph.Magnitude(element, QuantityRows.SurfaceArea)),
                    (MassKind.Volume, graph.Magnitude(element, QuantityRows.Volume)))
                .Choose(static row => row.Item2.Map(magnitude => (row.Item1, magnitude)))
                is { IsEmpty: false } held
                ? MeasureBundle.Of(held, TakeoffKey).ToFin()
                    // Fabrication NestYield.WasteAreaMm2 contributes when the graph carries a nest-yield row for
                    // this element, joining as the decode-side WasteAreaM2 column so off-cut waste rolls into the
                    // same folds. Its ABSENCE is absent nesting evidence, not a measured zero off-cut.
                    .Map(measures => new ElementTakeoff(measures, graph.Magnitude(element, QuantityRows.NestWasteArea)))
                : Fin.Fail<ElementTakeoff>(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.MeasureAbsent, element.Value));
    }

    static readonly Op TakeoffKey = Op.Of(name: nameof(LifecycleGraphReads));
}
```

## [04]-[COST_RUNNER]

- Owner: `LifecycleAssessment.RunCost` the supply/install/lifecycle cost rollup entry over the shared `Rollup` fold; `CostBudget` the acceptance derivation over the request's two budget columns.
- Entry: `public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock)` names the `AggregateCost` arm, the currency guard, the three cost facts, and the budget acceptance; every other step is the one `Rollup` skeleton the carbon runner also instantiates.
- Growth: a maintenance-cost-over-service-life sum or a circularity-cost credit is one fold over the same composition; the cost rail spans all composition cases (a single material or a profile member has a unit supply/install cost); a new acceptance modality is one `AssessmentRequest.Cost` budget column with one `CostBudget` arm.
- Boundary: this is the embodied MATERIAL-cost takeoff only — construction SCHEDULING, resource-leveling, and 4D cost-loading stay in `Rasm.Bim` (MPXJ), never re-derived here; the `request.Currency` is load-bearing — the aggregated cost is guarded to it (a material priced in a different `Currency` rails, since the fold carries no exchange rate), so the request currency is a real validation target, never a decorative field; the per-ply quantity derives from the seam `Cost.Basis` against the baked takeoff (or a `PlyQuantity` override); a material with no `Cost` case rails. Where the caller states a budget the governing ratio is REAL and the verdict a genuine `Satisfied`/`Marginal`/`Exceeded` band: `BudgetTotal` is the absolute cap on the target set's in-place cost, `BudgetPerArea` the rate against the same takeoff area the aggregator distributes cost by, the absolute column winning where both ride; a request carrying NEITHER column reports an ABSENT ratio, the same no-target spelling the carbon runner now holds. The request's budget columns are `decimal` because money is exact and a binary double re-rounds every currency figure it touches; the aggregator's own totals are `double` and the ratio widens once at the divide — so the exactness claim covers the ACCEPTANCE columns alone, and no exact-money accumulator exists or is claimed on this page.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class LifecycleAssessment {
    // RunCost names its four parameters and inherits the skeleton. The takeoff area threads through the shared
    // measure leg because the per-area budget ratios against a set-wide quantity a per-element fact stream cannot
    // recover after the fact; the carbon runner's own measure leg contributes no area, and a budget arm reading it
    // would be reading a quantity that discipline never took.
    public static Fin<AssessmentResult> RunCost(ElementGraph graph, AssessmentRequest.Cost request, IClock clock) =>
        Rollup(graph, request.Route, request.Targets, CostKey, clock,
            aggregate: (composition, takeoff) => AssemblyAggregator.AggregateCost(composition, Resolver(graph), Seq<PlyQuantity>(), takeoff),
            project: static (id, cost) => AssessmentFact.Rows(
                DomainMeasure($"{id.Value}/supply-total", cost.SupplyTotal, cost.Currency.Key),
                DomainMeasure($"{id.Value}/install-total", cost.InstallTotal, cost.Currency.Key),
                DomainMeasure($"{id.Value}/in-place-total", cost.TotalInPlace, cost.Currency.Key)),
            admit: cost => cost.Currency.Key == request.Currency
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(Missing(AssessmentInputReason.CurrencyMismatch, $"{cost.Currency.Key}<>{request.Currency}")),
            measure: static cost => cost.TotalInPlace,
            acceptance: (total, areaM2) => CostBudget(request, areaM2).Map(budget => total / budget));

    // Budget resolution is a two-rung choice, absolute first: BudgetTotal caps the whole target set, BudgetPerArea
    // rates against the SAME takeoff area the aggregator distributes cost by — and where NO target took off an
    // area, the rate has nothing to rate against, so that rung is absent rather than multiplying by a zero the
    // fold would then read as an instantly exceeded budget of nothing. A non-positive budget resolves to None for
    // the same reason.
    static Option<double> CostBudget(AssessmentRequest.Cost request, Option<double> areaM2) =>
        (request.BudgetTotal.Map(static total => (double)total)
         | (from rate in request.BudgetPerArea from area in areaM2 select (double)rate * area))
            .Filter(static budget => double.IsFinite(budget) && budget > 0.0);
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
