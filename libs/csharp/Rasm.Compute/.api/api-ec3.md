# [RASM_COMPUTE_API_EC3]

`EC3` (Building Transparency's Embodied Carbon in Construction Calculator) is a
REST service, not a NuGet assembly: it exposes the openEPD interchange graph of
Environmental Product Declarations plus a category-scoped embodied-carbon
statistics engine. Compute consumes it as a hand-thin client over `HttpClient` +
`System.Text.Json` source-generated contexts — there is no package to pin, so
this catalog is the integration contract, not a member list. EC3 is the
embodied-carbon boundary owner of the EN 15978 LCA Assessment lane: the closed-form
A1-A3/A4/A5/B/C/D fold reads per-EPD `gwp` `Measurement` values (kgCO2e per
declared unit) or category `StatisticsDto` percentiles, then writes an
`Assessment.Result` node back content-keyed on the (OMF query, route) pair. Only
the GET read surface is consumed; the POST/PUT/PATCH publisher surface is
documented for completeness and never called from Compute.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EC3` / openEPD REST
- package: none (REST integration; hand-thin over `System.Net.Http.HttpClient`)
- license: API governed by Building Transparency API Terms of Service; openEPD interchange schema is open (the reference Python lib `cchangelabs/openepd` is Apache-2.0); no vendored code, so no NuGet attribution obligation
- base url (openEPD API): `https://openepd.buildingtransparency.org/api` — the versioned, supported, declaration-graph surface (EPD read/search/statistics, categories, supporting entities)
- base url (legacy EC3 API): `https://buildingtransparency.org/api` — the older `materials` field-filter query surface; superseded by openEPD `/v2/epds/search` (OMF) and not consumed
- auth: bearer token — `Authorization: Bearer <token>`; tokens minted per-account at `buildingtransparency.org/ec3/manage-apps/keys` with READ (consumer) or READ WRITE (publisher) scope; API access mirrors the account's EC3 permissions
- versioning: `/v2/...` path prefix on the search/statistics endpoints; every declaration document also self-describes via `doctype` ("openEPD") + `openepd_version`
- metering: each endpoint deducts a token cost from the account usage budget (Material Filter Query / statistics ≈ 0.01, EPD Details ≈ 1.0, OpenEPD reads ≈ 1.0) — batch via the streaming pager, cache aggressively, never poll
- rate limit: `429` responses carry `Retry-After` (seconds); the resilience pipeline honors it as the backoff floor
- transport: JSON request/response; `application/json` content-type; UTF-8
- asset: external service (no runtime library, no native dependency)
- rail: embodied-carbon

## [02]-[ENDPOINTS]

[ENDPOINT_SCOPE]: embodied-carbon read lane (consumed by Compute)
- rail: embodied-carbon
- note: the three reads that feed the LCA Assessment lane. Search and statistics take the Open Material Filter (OMF) string; a single-EPD read takes the openXPD UUID. Search/statistics return the envelope `{ "payload": …, "meta": … }`; the by-UUID read returns a bare `Epd`. Pagination is `page_number`/`page_size` query params; `meta.paging` reports `total_count`/`total_pages`/`page_size` for the streaming pager.

| [INDEX] | [METHOD_PATH]             | [QUERY_BODY]                      | [RESPONSE]                                            |
| :-----: | :------------------------ | :-------------------------------- | :---------------------------------------------------- |
|  [01]   | `GET /v2/epds/search`     | `omf`, `page_number`, `page_size` | `{ payload: Epd[], meta: EpdSearchMeta }`             |
|  [02]   | `GET /v2/epds/statistics` | `omf`                             | `{ payload: StatisticsDto, meta: EpdStatisticsMeta }` |
|  [03]   | `GET /epds/{uuid}`        | path `uuid`; `fields` (CSV mask)  | `Epd`                                                 |
|  [04]   | `GET /v2/categories/tree` | —                                 | `Category` (recursive `subcategories`)                |

- [01]-[/v2/epds/search]: per-product EPD query; iterate pages until `meta.paging.total_pages`; each `Epd.impacts[method].gwp.A1A2A3.mean` is a kgCO2e-per-declared-unit datum.
- [02]-[/v2/epds/statistics]: category-scoped GWP distribution; `achievable_target` (20th pct), `conservative_estimate` (80th pct), `average`, `pct50_gwp` are the LCA reference lines.
- [03]-[/epds/{uuid}]: full single declaration; `fields` trims the response to the consumed projection (e.g. `id,declared_unit,impacts`) to cut token + payload cost.
- [04]-[/v2/categories/tree]: `id` (`unique_name`), `openepd_hierarchical_name`, `masterformat`, `declared_unit` map a Rasm `Classification(system,code)` to an OMF `!EC3 search(...)` category leaf.

[ENDPOINT_SCOPE]: supporting reads
- rail: embodied-carbon

| [INDEX] | [METHOD_PATH]                   | [RESPONSE]                                    | [NOTE]                                               |
| :-----: | :------------------------------ | :-------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `GET /pcrs/{uuid}`              | `Pcr`                                         | Product Category Rule a declaration was issued under |
|  [02]   | `GET /generic_estimates/{uuid}` | `GenericEstimate`                             | average-dataset fallback when no product EPD exists  |
|  [03]   | `GET /generic_estimates`        | `{ payload: GenericEstimatePreview[], meta }` | paged list; streaming pager source                   |
|  [04]   | `GET /industry_epds/{uuid}`     | `IndustryEpd`                                 | industry-wide EPD; weighted ~20 product EPDs         |
|  [05]   | `GET /industry_epds`            | `{ payload: IndustryEpdPreview[], meta }`     | paged list                                           |

[ENDPOINT_SCOPE]: publisher write surface (NOT consumed by Compute — documented for completeness)
- rail: embodied-carbon
- note: these require READ WRITE token scope and EPD-Creator permission. Rasm is a carbon consumer, never a declaration publisher, so the client exposes none of these.

| [INDEX] | [METHOD_PATH]                                             | [BODY]            | [NOTE]                                             |
| :-----: | :-------------------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `POST /epds`                                              | `Epd`             | create a new declaration                           |
|  [02]   | `PUT /epds/{id}`                                          | `Epd`             | replace an existing declaration                    |
|  [03]   | `PATCH /epds/post-with-refs`                              | `Epd`             | upsert with nested org/plant/pcr refs              |
|  [04]   | `POST /orgs` · `PUT /orgs/{id}`                           | `Org`             | manufacturer/operator (`web_domain` natural key)   |
|  [05]   | `POST /plants` · `PUT /plants/{id}`                       | `Plant`           | facility (OpenLocationCode + manufacturer key)     |
|  [06]   | `POST /pcrs` · `PUT /pcrs/{id}`                           | `Pcr`             | Product Category Rule                              |
|  [07]   | `POST /standards` · `PUT /standards/{id}`                 | `Standard`        | compliance standard reference                      |
|  [08]   | `POST /generic_estimates` · `PUT /generic_estimates/{id}` | `GenericEstimate` | avg-dataset publish; also `PATCH …/post_with_refs` |
|  [09]   | `POST /industry_epds` · `PUT /industry_epds/{id}`         | `IndustryEpd`     | industry-EPD publish                               |

## [03]-[WIRE_TYPES]

[WIRE_TYPE_SCOPE]: response envelope + paging
- rail: embodied-carbon
- note: search/statistics/list endpoints wrap the result in a generic envelope; the Compute decoder reads `payload` for data and `meta.paging` for the streaming cursor. The by-UUID reads skip the envelope and return the bare document. JSON keys carrying a `-` (LCIA scope names) require an explicit `[JsonPropertyName]` alias in the source-gen context.

| [INDEX] | [JSON_SHAPE]               | [FIELDS]                                              | [CONSUMER_NOTE]                                 |
| :-----: | :------------------------- | :---------------------------------------------------- | :---------------------------------------------- |
|  [01]   | envelope                   | `payload`, `meta`                                     | the data field; `meta` drives paging + warnings |
|  [02]   | `meta.paging`              | `total_count:int`; `total_pages:int`; `page_size:int` | stop when `page_number > total_pages`           |
|  [03]   | `meta.performance`         | `execution_time_ms:int`                               | server-side timing for the receipt              |
|  [04]   | `meta.warnings[]`          | `message:str`; `code:str`; `field:str?`               | soft-degradation folded into the receipt        |
|  [05]   | `meta.mf` / `meta.mf_hash` | resolved filter string + hash                         | `mf_hash` is a cache + content-key component    |

[WIRE_TYPE_SCOPE]: `StatisticsDto` (the LCA reference-line payload)
- rail: embodied-carbon
- note: every GWP field is `float`, in kgCO2e per declared unit, over the `declared_unit` basis. EC3 reference lines: `achievable_target` = 20th percentile, `conservative_estimate` = 80th percentile; the `*_no_bod` variants exclude the burden-of-doubt uplift.

| [INDEX] | [FIELD]                                                          | [TYPE]   | [MEANING]                                |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `achievable_target`                                              | `float`  | 20th-percentile GWP — aggressive target  |
|  [02]   | `conservative_estimate`                                          | `float`  | 80th-percentile GWP — conservative value |
|  [03]   | `average`                                                        | `float`  | mean GWP across the matched set          |
|  [04]   | `min` / `max`                                                    | `float?` | extrema of the matched set               |
|  [05]   | `standard_deviation`                                             | `float`  | dispersion of the matched set            |
|  [06]   | `epds_count` / `industry_epds_count` / `generic_estimates_count` | `int`    | sample composition (industry EPDs ~20×)  |
|  [07]   | `declared_unit`                                                  | `Amount` | the unit basis every percentile is per   |

- [PERCENTILES]: raw `pct10_gwp`/`pct30_gwp`/`pct40_gwp`/`pct50_gwp`/`pct60_gwp`/`pct70_gwp`/`pct90_gwp` (`float`) and burden-of-doubt-excluded `pct20_gwp_no_bod`/`pct40_gwp_no_bod`/`pct60_gwp_no_bod`/`pct80_gwp_no_bod`/`average_gwp_no_bod` (`float?`), all kgCO2e per declared unit.

[WIRE_TYPE_SCOPE]: `Epd` / `BaseDeclaration` (the declaration document)
- rail: embodied-carbon
- note: the consumed declaration shape. `declared_unit`/`kg_per_declared_unit` canonicalize through UnitsNet at the boundary; `impacts` is the embodied-carbon source. `specs` is the sector-specific functional spec dict (e.g. `concrete.strength_28d`) used to author OMF filters, not decoded structurally.

| [INDEX] | [FIELD]                                                      | [TYPE]                 | [CONSUMER_NOTE]                                |
| :-----: | :----------------------------------------------------------- | :--------------------- | :--------------------------------------------- |
|  [01]   | `id`                                                         | openXPD UUID `str?`    | EPD identity; by-UUID read + EC3 deep-link key |
|  [02]   | `doctype` / `openepd_version`                                | `str`                  | `"openEPD"` + version; guards the decoder      |
|  [03]   | `product_name` / `product_description`                       | `str?`                 | human label onto the Assessment node           |
|  [04]   | `product_classes`                                            | `dict<str,str\|str[]>` | classification map → `Classification`          |
|  [05]   | `declared_unit`                                              | `Amount?`              | functional unit (`1 m3`); UnitsNet basis       |
|  [06]   | `kg_per_declared_unit`                                       | `Amount` (kg)          | mass per declared unit — per-unit → per-kg     |
|  [07]   | `kg_C_per_declared_unit` / `kg_C_biogenic_per_declared_unit` | `Amount?` (kg)         | elemental + biogenic carbon (kgCO2e/kgC)       |
|  [08]   | `product_service_life_years`                                 | `float?`               | RSL feeding the B-stage replacement fold       |
|  [09]   | `date_of_issue` / `valid_until`                              | ISO-8601 datetime?     | NodaTime `Instant`; `valid_until` = staleness  |
|  [10]   | `version`                                                    | `int?`                 | declaration revision                           |
|  [11]   | `compliance`                                                 | `Standard[]`           | standards `short_name`/`name`/`link`/`issuer`  |
|  [12]   | `pcr`                                                        | `Pcr?`                 | issuing Product Category Rule                  |
|  [13]   | `impacts`                                                    | `Impacts`              | the LCIA map — embodied-carbon source          |
|  [14]   | `resource_uses` / `output_flows`                             | by-name scopesets      | energy/material in, waste out (EN 15804)       |
|  [15]   | `manufacturer` / `program_operator`                          | `Org?`                 | provenance orgs (`web_domain` natural key)     |
|  [16]   | `third_party_verifier` / `epd_developer`                     | `Org?`                 | verifier + developer provenance orgs           |
|  [17]   | `specs`                                                      | sector dict            | OMF filter vocabulary, not decoded             |
|  [18]   | `ext`                                                        | `dict?`                | `ext.gwp` + `ext.category`: EC3 GWP + category |

[WIRE_TYPE_SCOPE]: `Impacts` → `ImpactSet` → `ScopeSet` (the LCIA tree)
- rail: embodied-carbon
- note: `Impacts` is a JSON object keyed by LCIA method name → `ImpactSet`; `ImpactSet` is keyed by impact-category name → `ScopeSet`; `ScopeSet` is keyed by EN 15978 life-cycle module → `Measurement`. The decoder selects `impacts["EN 15978:2011"].gwp` (or `["IPCC AR6"].gwp` / `["TRACI 2.1"].gwp`) then folds the module measurements.

| [INDEX] | [LEVEL]                | [SHAPE]                           |
| :-----: | :--------------------- | :-------------------------------- |
|  [01]   | `Impacts` (method map) | `LCIAMethod` → `ImpactSet`        |
|  [02]   | `ImpactSet` (category) | impact-category name → `ScopeSet` |
|  [03]   | `ScopeSet` (modules)   | EN 15978 module → `Measurement`   |
|  [04]   | `EolScenario`          | named end-of-life pathway         |

- [01]-[IMPACTS]: pick the method matching the Assessment route; `Unknown LCIA` is the fallback bucket.
- [02]-[IMPACTSET]: `gwp`; `gwp-fossil`; `gwp-biogenic`; `gwp-luluc`; `gwp-nonCO2`; `GWP-GHG`; `odp`; `ap`; `ep`/`ep-marine`/`ep-fresh`/`ep-terr`; `pocp`; `WDP`; `PM`; `IRP`; `ETP-fw`; `HTP-c`; `HTP-nc`; `SQP`; `ADP-mineral`; `ADP-fossil` — `gwp` is the headline carbon; the `gwp-*` split is required when GWP is decomposed; hyphenated keys need JSON aliases.
- [03]-[SCOPESET]: `A1A2A3`; `A1`; `A2`; `A3`; `A4`; `A5`; `B1`-`B7` (+ `Bn_years`); `C1`-`C4`; `D`; `C_scenarios:EolScenario[]` — the EN 15978 stages the closed-form LCA fold sums; `A1A2A3` is the product-stage cradle-to-gate total; `Bn_years` scope each use-stage value.
- [04]-[EOLSCENARIO]: `name`; `likelihood:float?`; `C1`-`C4`; `D` (each `Measurement`) — named end-of-life pathway with probability weighting.

[WIRE_TYPE_SCOPE]: leaf value types
- rail: embodied-carbon

| [INDEX] | [TYPE]         | [FIELDS]                                                       | [CONSUMER_NOTE]                                     |
| :-----: | :------------- | :------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Measurement`  | `mean:float`; `unit:str?`; `rsd:float?`; `dist:Distribution?`  | scope leaf; `mean` kgCO2e; `rsd`/`dist` uncertainty |
|  [02]   | `Amount`       | `qty:float?` (≥0); `unit:str?` (SI preferred)                  | unit-bearing quantity; UnitsNet-canonicalized once  |
|  [03]   | `Distribution` | `log-normal` \| `normal` \| `uniform` \| `triangular` \| `max` | error model; `max` = upper bound, not a mean        |

The vocabulary/enum leaf types carry closed value rosters:

- [01]-[LCIAMETHOD]: `"EN 15978:2011"`; `"TRACI 2.2"`/`2.1`/`2.0`/`1.0`; `"IPCC AR6"`/`AR5`; `"EF 3.1"`/`3.0`/`2.0`; `"ReCiPe 2016"`/`2008`; `"CML 2016"`…`1992`; `"USEtox 2.12"`; `"LIME2"`; `"GWP-GHG"`; `"Unknown LCIA"` — the impact-assessment method key; route selects which one the Assessment trusts.
- [02]-[OPENEPDUNIT]: `kg`; `m2`; `m`; `m2 * RSI`; `MJ`; `t * km`; `MPa`; `item`; `W`; `use`; `°C`; `kgCO2e`; `hour` — the allowed declared/measurement unit vocabulary; `kgCO2e` is the GWP unit.
- [03]-[CATEGORY]: `id` (`unique_name`); `display_name`; `short_name`; `openepd_hierarchical_name`; `masterformat`; `description`; `declared_unit:Amount?`; `subcategories:Category[]` — recursive tree; MasterFormat + hierarchical name bridge a Rasm `Classification` to an OMF category leaf.
- [04]-[OMF_QUERY]: `!EC3 search("ReadyMix") valid_until: >"2024-03-08" and specs.concrete.strength_28d: >"30 MPa" !pragma oMF("1.0/1")` — the `omf` string for search/statistics; category + field predicates + a closing `!pragma oMF(version)`.

## [04]-[INTEGRATION_LAW]

[STACK_HTTP]:
- The client is one typed `HttpClient` (named/typed client registered in DI) with `BaseAddress = https://openepd.buildingtransparency.org/api` and a `DelegatingHandler` injecting `Authorization: Bearer <token>` from configuration — no per-call header assembly, no third-party SDK.
- The resilience pipeline (the AppHost-owned `Microsoft.Extensions.Http.Resilience` / Polly standard handler) honors `429` + `Retry-After` as the backoff floor and bounds total attempts; transient `5xx` retries, `4xx` (except 429) fail fast onto the typed fault rail.
- Reads are issued GET-only; the publisher verbs are absent from the typed surface so a write can never be emitted with a READ-scope token.

[STACK_JSON]:
- Decoding is `System.Text.Json` with a source-generated `JsonSerializerContext` over the consumed projection (`Epd`, `ImpactSet`, `ScopeSet`, `Measurement`, `Amount`, `StatisticsDto`, the envelope) — no reflection, AOT-safe.
- Hyphenated LCIA scope keys (`ep-marine`, `gwp-fossil`, `ADP-mineral`, `ETP-fw`, `HTP-c`/`HTP-nc`) and the method-map string keys require explicit `[JsonPropertyName]` aliases; the `impacts` and `*_set` objects deserialize as `Dictionary<string, …>` then project to the typed scope union.
- `fields` query masking trims the response to the decoded subset, so the source-gen context only needs the consumed members, not the full openEPD schema.

[STACK_UNITSNET]:
- `Amount` (`declared_unit`, `kg_per_declared_unit`, `kg_C_per_declared_unit`) canonicalizes through UnitsNet exactly once at the boundary: `unit` resolves via `UnitParser`, `qty` lifts via the `QuantityValue` implicit operator, and only the raw SI scalar crosses interior signatures.
- GWP carries the unit `kgCO2e`, which is NOT a UnitsNet quantity (CO2-equivalence is a domain basis, not an SI dimension). GWP `Measurement.mean` stays a typed domain `EmbodiedCarbon` scalar (raw double, kgCO2e) on the `Assessment` payload — it is never forced through `Mass`. Mixing a kgCO2e value into a `UnitsNet.Mass` is the boundary error the decoder rejects.
- `product_service_life_years` and `Bn_years` are bare year counts feeding the B-stage replacement-count fold; they convert to `Duration` only if a time computation needs them.

[STACK_LANGUAGEEXT]:
- Every call returns `Fin<T>` (LanguageExt.Core): a non-2xx status, a deserialization failure, or a missing `impacts[method].gwp` mints a `ComputeFault` on the carbon band rather than throwing; `meta.warnings[]` fold into the receipt as soft notes, not faults.
- The streaming pager is a `Seq`/`IAsyncEnumerable` projection driven by `meta.paging`; `Traverse`/`Sequence` accumulate per-EPD GWP folds across pages in one applicative pass.
- The category→OMF resolution and the method-selection are total `Option`/match folds, never imperative null checks.

[STACK_NODATIME]:
- `date_of_issue` / `valid_until` decode to `Instant`; `valid_until < now` marks an EPD stale and demotes it in the LCA selection. The proto-serialized Assessment receipt carries the resolved `Instant`, never the raw string.

[CACHE_CONTENT_KEY]:
- EPD search/statistics responses are cached in `Microsoft.Extensions.Caching.Hybrid` keyed by the full `XxHash128(omf || route || page)` tuple; `meta.mf_hash` is only the OMF/material-filter component of that key when present, NOT the whole identity (route and page still participate), so the token-metered endpoints must not be re-hit for an unchanged query.
- The `Assessment.Result` node written back is content-keyed on the (OMF query, LCIA method, route) tuple via the one canonical `XxHash128` seed, so an identical carbon query is a cache hit and a 412-noop on the object store.

[RAIL_LAW]:
- Service: `EC3` / openEPD REST (Building Transparency); base `https://openepd.buildingtransparency.org/api`, bearer auth, token-metered, `429`/`Retry-After` rate-limited
- Owns: the embodied-carbon EPD graph (per-product `Epd.impacts` GWP) and the category-scoped GWP statistics (`StatisticsDto` percentiles / reference lines)
- Accept: a category + spec OMF query at the EN 15978 LCA boundary, returning kgCO2e-per-declared-unit measurements decoded once into typed domain scalars
- Reject: the publisher write verbs (Rasm is a consumer); forcing kgCO2e through `UnitsNet.Mass`; the legacy `buildingtransparency.org/api/materials` field-filter surface (superseded by OMF `/v2/epds/search`); re-hitting a token-metered endpoint for a content-key that is already cached
