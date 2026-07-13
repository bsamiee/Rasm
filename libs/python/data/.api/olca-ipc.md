# [PY_DATA_API_OLCA_IPC]

`olca-ipc` is the Python client for a running openLCA server: it drives CRUD over the openLCA model graph (processes, flows, flow properties, unit groups, product systems, impact methods, parameters), builds product systems, runs calculations and Monte-Carlo simulations, and queries the full result surface — technosphere requirements, intervention (elementary) flows, LCIA impacts, costs, upstream contribution trees, and Sankey graphs. The wire model is the separate `olca_schema` package: pure dataclasses with `new_*` factories and `to_dict`/`from_dict`/`to_json` codecs, decoupled from transport. It is the LIVE openLCA COMPUTE/interchange leg of the data EPD/LCA owner (it requires an openLCA IPC or REST server); it never re-implements the LCA matrix solver openLCA owns or stores the model as the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `olca-ipc`
- package: `olca-ipc`
- version: `2.6.3`
- license: MPL-2.0
- module: `olca_ipc` (client/result) + `olca_schema` (`2.6.2`, CC0-1.0 — the wire model)
- owner: `data`
- rail: epd-lca (openLCA interchange + compute)
- depends: `olca-schema>=2.6.2`, `requests>=2.33.1`
- surface-law: the current surface is `import olca_ipc as ipc` + `import olca_schema as o` (model types live in `olca_schema`). The legacy single `olca` package (`import olca`; `olca.Client`; `client.dispose(result)`) shown on `greendelta.github.io/olca-ipc.py` is the OLD 0.x API and is superseded — do not use it; the `2.x` `Result` owns `dispose()` itself
- capability: a uniform IPC client over three transports (JSON-RPC, REST, protobuf) for model CRUD, product-system construction, calculation + simulation, and a complete result-query surface; the typed `olca_schema` graph with `new_*` factories and dict/JSON codecs; base64 source-file upload

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: clients and results (`olca_ipc`)
- rail: epd-lca

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]       | [ROLE]                                                                   |
| :-----: | :-------------------------------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `Client(endpoint: str \| int = 8080)`   | JSON-RPC client     | default JSON-RPC client; full `ProtoClient` contract                     |
|  [02]   | `RestClient(endpoint: str, **kwargs)`   | REST client         | same contract over openLCA REST; `**kwargs` → `requests.Session`         |
|  [03]   | `ProtoClient`                           | client ABC          | abstract transport-agnostic contract (CRUD + calculate/simulate)         |
|  [04]   | `Result` / `RestResult` / `ProtoResult` | result handle / ABC | lazy handle to a server-side result; `ProtoResult` the abstract contract |
|  [05]   | `FileData`                              | upload payload      | base64 payload for `put_source_file`; `FileData.from_file(path)`         |

[PUBLIC_TYPE_SCOPE]: model entities (`olca_schema as o` — every symbol below is `o.`-prefixed)
- rail: epd-lca
- setup: `CalculationSetup(target: Ref, impact_method, nw_set, amount, unit, with_costs, …)` is the calculation request; `ResultState(id, is_ready, is_scheduled, error)` is the server result state

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]    | [ROLE]                                          |
| :-----: | :----------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `RootEntity` / `Ref`                                               | base / reference | standalone base (`id`/`name`); typed `Ref`      |
|  [02]   | `Process`, `Flow`, `Exchange`                                      | inventory core   | a `Process` holds `Exchange`s of `Flow`s        |
|  [03]   | `FlowProperty`, `UnitGroup`, `Unit`                                | quantity model   | the quantity/unit typing of a `Flow`            |
|  [04]   | `ImpactMethod`, `ImpactCategory`, `ImpactFactor`                   | LCIA model       | impact method → categories → factors            |
|  [05]   | `ProductSystem`, `LinkingConfig`                                   | system           | linked product system + auto-linking config     |
|  [06]   | `CalculationSetup`, `ResultState`                                  | calculation      | calculation request + server result state       |
|  [07]   | `TechFlow`, `EnviFlow`                                             | result keys      | technosphere-flow and intervention-flow keys    |
|  [08]   | `TechFlowValue`, `EnviFlowValue`, `ImpactValue`, `CostValue`       | result values    | valued rows across inventory/impact/cost        |
|  [09]   | `UpstreamNode`, `GroupValue`, `SankeyRequest`, `SankeyGraph`       | contribution     | upstream node, grouped value, Sankey graph      |
|  [10]   | `{FlowType, ProcessType, ParameterScope, AllocationType, RefType}` | enums            | flow/process kind, param scope, allocation, ref |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model CRUD (`ProtoClient` — bound on `Client`/`RestClient` as `client`)
- rail: epd-lca
- reads: `get(model_type, uid=None, name=None) -> E | None`, `get_all(model_type) -> list[E]`, `get_descriptors(model_type) -> list[o.Ref]`/`get_descriptor(model_type, uid=None, name=None)`/`find(model_type, name)`, `get_providers(flow=None) -> list[o.TechFlow]`, `get_parameters(model_type, uid) -> list[o.Parameter | o.ParameterRedef]`
- writes: `put(model: o.RootEntity) -> o.Ref | None`/`put_all(*models)`, `put_source_file(source: o.Source | o.Ref, file_data: FileData) -> bool`, `delete(model)`/`delete_all(*models)`, `create_product_system(process, config: o.LinkingConfig | None = None) -> o.Ref | None`

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                                        |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `get`                                         | read one       | fetch a full entity by id or name (polymorphic on type + key) |
|  [02]   | `get_all`                                     | read all       | every instance of a type (eager — small types only)           |
|  [03]   | `get_descriptors` / `get_descriptor` / `find` | read refs      | lightweight `Ref` listing/lookup without full payloads        |
|  [04]   | `get_providers` / `get_parameters`            | read graph     | flow providers and an entity's parameters/redefinitions       |
|  [05]   | `put` / `put_all`                             | write          | insert/update one or many entities; returns the stored `Ref`  |
|  [06]   | `put_source_file`                             | write file     | upload a source document (base64 via `FileData`)              |
|  [07]   | `delete` / `delete_all`                       | delete         | remove one or many entities                                   |
|  [08]   | `create_product_system`                       | build          | auto-link a product system from a root process                |

[ENTRYPOINT_SCOPE]: calculation and result lifecycle
- rail: epd-lca
- call: `client.calculate(setup: o.CalculationSetup) -> Result`, `client.simulate(setup) -> Result`, `result.wait_until_ready() -> o.ResultState`/`get_state()`, `result.dispose()`, `result.get_demand() -> o.TechFlowValue | None`, `get_tech_flows()`/`get_envi_flows()`/`get_impact_categories()`

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------------- |
|  [01]   | `calculate`                                                   | calculate      | schedule a calc; returns a `Result` handle         |
|  [02]   | `simulate`                                                    | simulate       | Monte-Carlo handle; `result.simulate_next()`       |
|  [03]   | `wait_until_ready` / `get_state`                              | poll           | block until ready; scheduled/ready/error state     |
|  [04]   | `dispose`                                                     | release        | free the server result; dispose or leak            |
|  [05]   | `get_demand`                                                  | enumerate      | the result's reference demand row                  |
|  [06]   | `get_tech_flows` / `get_envi_flows` / `get_impact_categories` | enumerate      | tech-flow, intervention-flow, impact-category axes |

[ENTRYPOINT_SCOPE]: result queries (`ProtoResult` — total/direct/intensity/upstream/grouped families)
- rail: epd-lca
- technosphere: `get_total_requirements() -> list[o.TechFlowValue]`, `get_total_requirements_of(tech_flow)`, `get_scaling_factors()`, `get_scaled_tech_flows_of(...)`, `get_unscaled_tech_flows_of(...)`
- inventory: `get_total_flows() -> list[o.EnviFlowValue]`, `get_total_flow_value_of(envi_flow)`, `get_flow_contributions_of(...)`, `get_direct_interventions_of(tech_flow)`, `get_flow_intensities_of(...)`, `get_total_interventions_of(...)`, `get_upstream_interventions_of(envi_flow, path)`, `get_grouped_flow_results_of(...)`
- lcia: `get_total_impacts() -> list[o.ImpactValue]`, `get_normalized_impacts()`, `get_weighted_impacts()`, `get_total_impact_value_of(...)`, `get_impact_contributions_of(...)`, `get_direct_impacts_of(tech_flow)`, `get_impact_intensities_of(...)`, `get_total_impacts_of(...)`, `get_impact_factors_of(...)`, `get_flow_impacts_of(...)`, `get_upstream_impacts_of(impact, path)`, `get_grouped_impact_results_of(...)`
- lcc: `get_total_costs() -> o.CostValue`, `get_cost_contributions()`, `get_direct_costs_of(...)`, `get_cost_intensities_of(...)`, `get_total_costs_of(...)`, `get_upstream_costs_of(path)`, `get_grouped_cost_results()`
- sankey: `get_sankey_graph(config: o.SankeyRequest) -> o.SankeyGraph`

| [INDEX] | [ENTRY_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | technosphere   | scaled/unscaled requirement vectors and per-flow breakdowns                 |
|  [02]   | inventory      | total/direct/intensity/upstream/grouped intervention-flow results           |
|  [03]   | LCIA           | total/normalized/weighted impacts + contribution/intensity/upstream/grouped |
|  [04]   | LCC            | life-cycle-cost totals, contributions, intensities, upstream costs          |
|  [05]   | sankey         | the contribution Sankey graph for visualization                             |

[ENTRYPOINT_SCOPE]: model factories and codecs (`olca_schema as o` — every `new_*` is `o.`-prefixed)
- rail: epd-lca
- call: `o.new_unit_group(name, ref_unit)`, `o.new_unit(name, factor=1.0)`, `o.new_flow_property(name, unit_group)`, `o.new_flow(name, flow_type, flow_property)`/`new_product`/`new_waste`/`new_elementary_flow`, `o.new_process(name)`, `o.new_exchange(process, flow, amount=1.0, unit=None)`/`new_input`/`new_output`
- call: `o.new_parameter(name, value, scope=o.ParameterScope.GLOBAL_SCOPE)` (`GLOBAL_SCOPE`/`PROCESS_SCOPE`/`IMPACT_SCOPE`, default `GLOBAL_SCOPE`), `o.new_location(name, code=None)`, `o.new_{physical,economic,causal}_allocation_factor(...)`, `o.new_impact_category(name)`, `o.new_impact_factor(indicator, flow, value=1.0, unit=None)`, `o.new_impact_method(name, *indicators)`
- ref: `o.as_ref(entity) -> o.Ref`, `entity.to_ref() -> o.Ref`, `o.Ref(ref_type=o.RefType.X, id=...)` (the form `CalculationSetup.target` uses) — `olca_schema 2.6.2` has no module-level `ref()` helper (the legacy `olca` 0.x package); `entity.to_dict()`/`Type.from_dict(d)`/`entity.to_json()`/`Type.from_json(s)` are the wire codecs

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]     | [RAIL]                                            |
| :-----: | :------------------------------------------------------------ | :----------------- | :------------------------------------------------ |
|  [01]   | `new_unit_group`/`new_unit`/`new_flow_property`               | build quantities   | unit group, unit, flow-property factories         |
|  [02]   | `new_flow`/`new_product`/`new_waste`/`new_elementary_flow`    | build flows        | typed flow factories                              |
|  [03]   | `new_process`/`new_exchange`/`new_input`/`new_output`         | build processes    | process + exchange factories (auto `internal_id`) |
|  [04]   | `new_parameter`/`new_location`/`new_*_allocation_factor`      | build params/alloc | parameter, location, allocation-factor factories  |
|  [05]   | `new_impact_category`/`new_impact_factor`/`new_impact_method` | build LCIA         | impact category/factor/method factories           |
|  [06]   | `as_ref`/`entity.to_ref`/`o.Ref(...)`                         | reference          | entity → `Ref`, or construct by `RefType` + id    |
|  [07]   | `to_dict`/`from_dict`/`to_json`/`from_json`                   | codec              | the dataclass wire codecs at the boundary         |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- `ProtoClient` is the abstract contract; `Client` (JSON-RPC over a `requests.Session`), `RestClient` (REST), and the proto client all satisfy it, so calculation/CRUD code is transport-agnostic — pin a parameter to `ProtoClient` and pick the transport at the boundary
- `Result` is a HANDLE to a server-side result, not a materialized table: every `get_*` is a lazy IPC round trip against the live result id; this is why the result must be `dispose()`d and why large enumerations cost network — query only the axes needed
- the calculation lifecycle is `setup → calculate → wait_until_ready → query → dispose`: `CalculationSetup(target=o.Ref(ref_type=RefType.ProductSystem|Process, id=...), impact_method=Ref, nw_set=Ref, amount, unit, with_costs)` → `calculate` returns immediately → `wait_until_ready()` polls `ResultState` → query totals/contributions → `dispose()` in a `finally`
- `olca_schema` is decoupled data: the entity dataclasses carry no transport; build with `new_*` factories, link with `as_ref`/`to_ref` or a direct `o.Ref(ref_type=..., id=...)`, push with `put`/`put_all`, and the `to_dict`/`from_dict` (and `to_json`/`from_json`) codecs are the only wire contract — so a model is authored, serialized, and diffed entirely offline

[INTEGRATION]:
- TransportResource seam: the openLCA server endpoint (host/port) is supplied by the runtime `TransportResource`; construct `Client(endpoint)`/`RestClient(url)` from it at the boundary and never re-mint the connection
- tabular seam: the result rows (`list[o.ImpactValue]`/`list[o.EnviFlowValue]`/`list[o.TechFlowValue]`) project straight into a `pandas`/`polars` frame — the canonical pattern is `pd.DataFrame([(v.envi_flow.flow.name, v.amount, ...) for v in result.get_total_flows()])` — feeding the data tabular/contract/profile rails; `to_dict` gives the same as a record stream for `msgspec`
- bw2io seam (cross-tool): openLCA exports JSON-LD that `bw2io`'s JSON-LD importer ingests into Brightway, and `olca-ipc` drives the openLCA side live — together they are the two-engine LCA interchange the EPD/LCA owner spans
- bw2calc/bw2data seam (cross-engine alternative): `olca-ipc` is the REMOTE openLCA compute+store; `bw2calc` (in-process solver) over a `bw2data` project is the in-process Brightway alternative for the SAME characterized result — the EPD/LCA owner routes to `olca-ipc` when the inventory lives in an openLCA server and to the Brightway cluster when it lives in a `bw2data` project, and a `Result.get_total_impacts()` row is the openLCA-side counterpart of a `bw2calc` `score`
- epdx/openepd seam: openLCA 2.x models EPDs as a first-class entity (`o.RefType.Epd`), so an `epdx`-parsed ILCD+EPD or an `openepd` payload maps onto a native openLCA EPD/process/flow + `o.ImpactFactor` set pushed via `put_all`; conversely a calculation's `get_total_impacts()` yields EPD-comparable indicator values
- premise seam (prospective export): `premise.NewDatabase(...).write_db_to_olca(filepath)` exports a prospective (future-year) ecoinvent database into openLCA as processes/flows, so the forward-looking scenarios `olca-ipc` then calculates are the same ones the Brightway cluster scores in-process via `bw2calc` — `olca-ipc` is the openLCA sink of premise's `write_db_to_olca`
- msgspec/pydantic seam: the `olca_schema` `to_dict`/`from_dict` forms decode into `msgspec.Struct`/`pydantic` models at the wire when a typed internal carrier is wanted beyond the dataclasses
- stamina/anyio/observability seam: wrap the `requests`-backed IPC calls in a `stamina` retry for transient server failures; run the `wait_until_ready()` poll under an `anyio` deadline rather than an unbounded block; wrap `calculate`→query in a `structlog`/`opentelemetry` span keyed by `CalculationSetup.target`/`ResultState.id`; treat the `Result` as a resource released in a `finally`/`async with`
- ContentIdentity seam: key a calculation by the runtime `ContentIdentity` over the `CalculationSetup` (target + method + amount) so identical setups reuse the persistence ledger rather than recomputing

[EXCEPTIONS]:
- transport failures (connection refused, HTTP error) surface from `requests`; the client logs and returns `None`/empty on RPC-level errors rather than raising, so a consumer checks the return and the `ResultState.error` field
- `calculate` returns a `Result` whose state carries `error` when scheduling failed; `wait_until_ready()` returns a `ResultState(error=...)` on timeout/failure — inspect state, do not assume readiness
- a non-`dispose()`d result leaks server memory; this is a resource-rail obligation, not an exception

[RAIL_LAW]:
- Package: `olca-ipc` (+ `olca-schema`)
- Owns: the openLCA IPC/REST/proto client (model CRUD, product-system construction, calculate/simulate, the full result-query surface) and, via `olca_schema`, the typed model graph with `new_*` factories and dict/JSON codecs
- Accept: `import olca_ipc as ipc` + `import olca_schema as o`; `ProtoClient`-typed transport-agnostic code; the `setup → calculate → wait_until_ready → query → dispose` lifecycle with the result disposed in a `finally`; result rows projected into the tabular rail; models authored offline via factories and pushed with `put_all`
- Reject: the legacy single `olca` package / `import olca` 0.x surface; hand-rolled JSON-RPC against the openLCA socket when a client method exists; treating `Result` as a materialized table (it is a lazy server handle); skipping `dispose()`; re-implementing the LCA solver (openLCA owns it) or holding the model as the system of record
