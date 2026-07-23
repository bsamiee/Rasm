# [RASM_COMPUTE_API_EC3]

`EC3` (Building Transparency) is the embodied-carbon boundary owner of the EN 15978 LCA Assessment lane: the openEPD interchange graph of Environmental Product Declarations and a category-scoped GWP statistics engine, consumed hand-thin over `HttpClient` and a source-generated `System.Text.Json` context. Its closed-form A1-A3/A4/A5/B/C/D fold reads per-EPD `gwp` `Measurement` values (kgCO2e per declared unit) or category `StatisticsDto` percentiles, then writes an `Assessment.Result` node back content-keyed on the (OMF query, route) pair. Compute binds the GET read surface only.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `EC3` / openEPD REST
- package: none (REST integration)
- base url: `https://openepd.buildingtransparency.org/api` — the versioned declaration-graph surface
- auth: bearer token — `Authorization: Bearer <token>` at READ (consumer) scope
- versioning: `/v2/...` path prefix on search/statistics; every declaration self-describes via `doctype` (`"openEPD"`) and `openepd_version`
- metering: each endpoint deducts a token cost from the account budget; cached by content key, never polled
- rate limit: `429` carries `Retry-After` (seconds)
- transport: JSON request/response, `application/json`, UTF-8
- asset: external service; no runtime library, no native dependency
- rail: embodied-carbon

## [02]-[ENDPOINTS]

[ENDPOINT_SCOPE]: embodied-carbon read lane
- note: search and statistics take the Open Material Filter (OMF) string, a single-EPD read the openXPD UUID; search/statistics return `{ payload, meta }`, the by-UUID read a bare `Epd`. Pagination rides `page_number`/`page_size`, and `meta.paging` reports `total_count`/`total_pages`/`page_size` for the streaming pager.

| [INDEX] | [METHOD_PATH]             | [QUERY_BODY]                      | [RESPONSE]                                            |
| :-----: | :------------------------ | :-------------------------------- | :---------------------------------------------------- |
|  [01]   | `GET /v2/epds/search`     | `omf`, `page_number`, `page_size` | `{ payload: Epd[], meta: EpdSearchMeta }`             |
|  [02]   | `GET /v2/epds/statistics` | `omf`                             | `{ payload: StatisticsDto, meta: EpdStatisticsMeta }` |
|  [03]   | `GET /epds/{uuid}`        | path `uuid`; `fields` (CSV mask)  | `Epd`                                                 |
|  [04]   | `GET /v2/categories/tree` | —                                 | `Category` (recursive `subcategories`)                |

- [01]-[/v2/epds/search]: per-product EPD query; iterate pages until `meta.paging.total_pages`, each `Epd.impacts[method].gwp.A1A2A3.mean` a kgCO2e-per-declared-unit datum.
- [02]-[/v2/epds/statistics]: category-scoped GWP distribution; `achievable_target`, `conservative_estimate`, `average`, `pct50_gwp` are the LCA reference lines.
- [03]-[/epds/{uuid}]: full single declaration; `fields` trims the response to the consumed projection (`id,declared_unit,impacts`).
- [04]-[/v2/categories/tree]: `id` (`unique_name`), `openepd_hierarchical_name`, `masterformat`, `declared_unit` map a Rasm `Classification(system,code)` to an OMF category leaf.

[ENDPOINT_SCOPE]: supporting reads

| [INDEX] | [METHOD_PATH]                   | [RESPONSE]                                    | [NOTE]                                               |
| :-----: | :------------------------------ | :-------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `GET /pcrs/{uuid}`              | `Pcr`                                         | Product Category Rule a declaration was issued under |
|  [02]   | `GET /generic_estimates/{uuid}` | `GenericEstimate`                             | average-dataset fallback when no product EPD exists  |
|  [03]   | `GET /generic_estimates`        | `{ payload: GenericEstimatePreview[], meta }` | paged list; streaming pager source                   |
|  [04]   | `GET /industry_epds/{uuid}`     | `IndustryEpd`                                 | industry-wide EPD; weighted ~20 product EPDs         |
|  [05]   | `GET /industry_epds`            | `{ payload: IndustryEpdPreview[], meta }`     | paged list                                           |

## [03]-[WIRE_TYPES]

[WIRE_TYPE_SCOPE]: response envelope and paging
- note: search/statistics/list reads wrap the result in a generic envelope — the decoder reads `payload` for data and `meta.paging` for the streaming cursor; the by-UUID reads return the bare document.

| [INDEX] | [JSON_SHAPE]               | [FIELDS]                                              | [CONSUMER_NOTE]                                 |
| :-----: | :------------------------- | :---------------------------------------------------- | :---------------------------------------------- |
|  [01]   | envelope                   | `payload`, `meta`                                     | the data field; `meta` drives paging + warnings |
|  [02]   | `meta.paging`              | `total_count:int`; `total_pages:int`; `page_size:int` | stop when `page_number > total_pages`           |
|  [03]   | `meta.performance`         | `execution_time_ms:int`                               | server-side timing for the receipt              |
|  [04]   | `meta.warnings[]`          | `message:str`; `code:str`; `field:str?`               | soft-degradation folded into the receipt        |
|  [05]   | `meta.mf` / `meta.mf_hash` | resolved filter string + hash                         | `mf_hash` is a cache + content-key component    |

[WIRE_TYPE_SCOPE]: `StatisticsDto` (the LCA reference-line payload)
- note: every GWP field is `float`, kgCO2e over the `declared_unit` basis; `achievable_target` is the 20th percentile, `conservative_estimate` the 80th, and the `*_no_bod` variants exclude the burden-of-doubt uplift.

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
- note: the consumed declaration shape; `impacts` is the embodied-carbon source, and `specs` is the sector functional-spec dict (`concrete.strength_28d`) authoring OMF filters, not decoded structurally.

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
- note: `Impacts` keys LCIA method name → `ImpactSet`, keyed by impact-category name → `ScopeSet`, keyed by EN 15978 life-cycle module → `Measurement`; the decoder selects `impacts["EN 15978:2011"].gwp` (or `["IPCC AR6"].gwp` / `["TRACI 2.1"].gwp`) then folds the module measurements.

| [INDEX] | [LEVEL]                | [SHAPE]                           |
| :-----: | :--------------------- | :-------------------------------- |
|  [01]   | `Impacts` (method map) | `LCIAMethod` → `ImpactSet`        |
|  [02]   | `ImpactSet` (category) | impact-category name → `ScopeSet` |
|  [03]   | `ScopeSet` (modules)   | EN 15978 module → `Measurement`   |
|  [04]   | `EolScenario`          | named end-of-life pathway         |

- [01]-[IMPACTS]: pick the method matching the Assessment route; `Unknown LCIA` is the fallback bucket.
- [02]-[IMPACTSET]: `gwp`; `gwp-fossil`; `gwp-biogenic`; `gwp-luluc`; `gwp-nonCO2`; `GWP-GHG`; `odp`; `ap`; `ep`/`ep-marine`/`ep-fresh`/`ep-terr`; `pocp`; `WDP`; `PM`; `IRP`; `ETP-fw`; `HTP-c`; `HTP-nc`; `SQP`; `ADP-mineral`; `ADP-fossil` — `gwp` is the headline carbon, and the `gwp-*` split carries a decomposed GWP.
- [03]-[SCOPESET]: `A1A2A3`; `A1`; `A2`; `A3`; `A4`; `A5`; `B1`-`B7` (+ `Bn_years`); `C1`-`C4`; `D`; `C_scenarios:EolScenario[]` — the EN 15978 stages the closed-form fold sums; `A1A2A3` is the product-stage cradle-to-gate total, and `Bn_years` scopes each use-stage value.
- [04]-[EOLSCENARIO]: `name`; `likelihood:float?`; `C1`-`C4`; `D` (each `Measurement`) — named end-of-life pathway with probability weighting.

[WIRE_TYPE_SCOPE]: leaf value types

| [INDEX] | [TYPE]         | [FIELDS]                                                       | [CONSUMER_NOTE]                                     |
| :-----: | :------------- | :------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Measurement`  | `mean:float`; `unit:str?`; `rsd:float?`; `dist:Distribution?`  | scope leaf; `mean` kgCO2e; `rsd`/`dist` uncertainty |
|  [02]   | `Amount`       | `qty:float?` (≥0); `unit:str?` (SI preferred)                  | unit-bearing quantity; UnitsNet-canonicalized once  |
|  [03]   | `Distribution` | `log-normal` \| `normal` \| `uniform` \| `triangular` \| `max` | error model; `max` = upper bound, not a mean        |

Vocabulary and enum leaf types carry closed value rosters:

- [01]-[LCIAMETHOD]: `"EN 15978:2011"`; `"TRACI 2.2"`/`2.1`/`2.0`/`1.0`; `"IPCC AR6"`/`AR5`; `"EF 3.1"`/`3.0`/`2.0`; `"ReCiPe 2016"`/`2008`; `"CML 2016"`…`1992`; `"USEtox 2.12"`; `"LIME2"`; `"GWP-GHG"`; `"Unknown LCIA"` — the impact-assessment method key; route selects which one the Assessment trusts.
- [02]-[OPENEPDUNIT]: `kg`; `m2`; `m`; `m2 * RSI`; `MJ`; `t * km`; `MPa`; `item`; `W`; `use`; `°C`; `kgCO2e`; `hour` — the allowed declared/measurement unit vocabulary; `kgCO2e` is the GWP unit.
- [03]-[CATEGORY]: `id` (`unique_name`); `display_name`; `short_name`; `openepd_hierarchical_name`; `masterformat`; `description`; `declared_unit:Amount?`; `subcategories:Category[]` — recursive tree; MasterFormat + hierarchical name bridge a Rasm `Classification` to an OMF category leaf.
- [04]-[OMF_QUERY]: `!EC3 search("ReadyMix") valid_until: >"2024-03-08" and specs.concrete.strength_28d: >"30 MPa" !pragma oMF("1.0/1")` — the `omf` string for search/statistics; category + field predicates + a closing `!pragma oMF(version)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One typed `HttpClient` (openEPD `BaseAddress`, bearer injected by a `DelegatingHandler`) issues GET only; every call returns `Fin<T>` and decodes once at the boundary into typed domain scalars, so the publisher verbs are unrepresentable on the typed surface.

[STACKING]:
- `api-languageext`(`.api/api-languageext.md`): every call returns `Fin<T>` — a non-2xx status, a decode failure, or a missing `impacts[method].gwp` mints a `ComputeFault` on the carbon band; the streaming pager is a `Seq`/`IAsyncEnumerable` over `meta.paging`, `Traverse`/`Sequence` folding per-EPD GWP across pages in one applicative pass; category→OMF resolution and method-selection are total `Option` folds.
- `api-unitsnet`(`.api/api-unitsnet.md`): `Amount` (`declared_unit`, `kg_per_declared_unit`, `kg_C_per_declared_unit`) canonicalizes once at the boundary — `unit` via `UnitParser`, `qty` via the `QuantityValue` implicit operator — and only the raw SI scalar crosses interior signatures; `kgCO2e` is a domain basis, not a UnitsNet dimension, so GWP `Measurement.mean` stays a typed `EmbodiedCarbon` scalar and never enters `Mass`.
- `api-nodatime`(`.api/api-nodatime.md`): `date_of_issue`/`valid_until` decode to `Instant`; `valid_until < now` demotes a stale EPD in LCA selection, and the proto-serialized `Assessment` receipt carries the resolved `Instant`.
- `api-hybrid-cache`(`.api/api-hybrid-cache.md`) + `api-hashing`(`.api/api-hashing.md`): search/statistics responses cache in `HybridCache` keyed by `XxHash128(omf || route || page)` — `meta.mf_hash` is only the OMF component, route and page still participate — and the `Assessment.Result` write-back is content-keyed on the (OMF, LCIA method, route) tuple via the one `XxHash128` seed, so a repeated query is a cache hit and a token-metered endpoint is never re-hit.
- `System.Text.Json` source-gen: a `JsonSerializerContext` over the consumed projection decodes reflection-free and AOT-safe; hyphenated LCIA scope keys (`ep-marine`, `gwp-fossil`, `ADP-mineral`, `ETP-fw`, `HTP-c`/`HTP-nc`) and the method-map string keys carry `[JsonPropertyName]` aliases, `impacts`/`*_set` deserialize as `Dictionary<string, …>` then project to the typed scope union, and `fields` masking trims the response to the decoded subset.
- `Analysis/lifecycle`: sums the EN 15978 module `Measurement` values through the closed-form A1-A3/A4/A5/B/C/D fold, `product_service_life_years`/`Bn_years` feeding the B-stage replacement-count fold; the AppHost-owned `Microsoft.Extensions.Http.Resilience` handler honors `429`/`Retry-After` as the backoff floor, retries transient `5xx`, and fails `4xx` fast onto the typed fault rail.

[LOCAL_ADMISSION]:
- A category + spec OMF query enters at the EN 15978 LCA boundary and returns kgCO2e-per-declared-unit measurements decoded once into typed domain scalars; `meta.warnings[]` fold into the `Assessment` receipt as soft notes, never faults.

[RAIL_LAW]:
- Package: `EC3` / openEPD REST (Building Transparency)
- Owns: the embodied-carbon EPD graph (per-product `Epd.impacts` GWP) and category-scoped GWP statistics (`StatisticsDto` percentiles)
- Accept: a category + spec OMF query at the EN 15978 LCA boundary, returning kgCO2e-per-declared-unit measurements decoded once into typed domain scalars
- Reject: the publisher write verbs; forcing kgCO2e through `UnitsNet.Mass`; re-hitting a token-metered endpoint for an already-cached content key
