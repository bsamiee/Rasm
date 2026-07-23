# [PY_DATA_API_OLCA_IPC]

`olca-ipc` drives a running openLCA server through one transport-agnostic client: model CRUD, product-system construction, calculation and Monte-Carlo simulation, and the full result-query surface across inventory, impact, cost, and contribution axes. `olca_schema` carries the typed model graph as a separate package — transport-free dataclasses with `new_*` factories and dict/JSON codecs. It owns the live openLCA compute/interchange leg of the data EPD/LCA rail, never re-implementing the matrix solver openLCA owns nor holding the model as the system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `olca-ipc`
- package: `olca-ipc` (MPL-2.0)
- module: `olca_ipc` (client + result) + `olca_schema` (CC0-1.0, the transport-free model graph)
- owner: `data`
- rail: epd-lca (openLCA interchange + compute)
- depends: `olca-schema`, `requests`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: clients and results (`olca_ipc`)

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Client(endpoint: str \| int = 8080)`   | class         | default JSON-RPC client; full `ProtoClient` contract             |
|  [02]   | `RestClient(endpoint: str, **kwargs)`   | class         | same contract over openLCA REST; `**kwargs` → `requests.Session` |
|  [03]   | `ProtoClient`                           | abc           | transport-agnostic contract: CRUD + calculate/simulate           |
|  [04]   | `Result` / `RestResult` / `ProtoResult` | class / abc   | lazy handle to a server-side result; `ProtoResult` the contract  |
|  [05]   | `FileData`                              | class         | base64 payload for `put_source_file`; `FileData.from_file(path)` |

[PUBLIC_TYPE_SCOPE]: model entities (`olca_schema as o` — every symbol is `o.`-prefixed)

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY]    | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `RootEntity` / `Ref`                                               | base / reference | standalone base (`id`/`name`); typed `Ref`      |
|  [02]   | `Process`, `Flow`, `Exchange`                                      | class            | a `Process` holds `Exchange`s of `Flow`s        |
|  [03]   | `FlowProperty`, `UnitGroup`, `Unit`                                | class            | the quantity/unit typing of a `Flow`            |
|  [04]   | `ImpactMethod`, `ImpactCategory`, `ImpactFactor`                   | class            | impact method → categories → factors            |
|  [05]   | `ProductSystem`, `LinkingConfig`                                   | class            | linked product system + auto-linking config     |
|  [06]   | `CalculationSetup`, `ResultState`                                  | class            | calculation request + server result state       |
|  [07]   | `TechFlow`, `EnviFlow`                                             | class            | technosphere-flow and intervention-flow keys    |
|  [08]   | `TechFlowValue`, `EnviFlowValue`, `ImpactValue`, `CostValue`       | class            | valued rows across inventory/impact/cost        |
|  [09]   | `UpstreamNode`, `GroupValue`, `SankeyRequest`, `SankeyGraph`       | class            | upstream node, grouped value, Sankey graph      |
|  [10]   | `{FlowType, ProcessType, ParameterScope, AllocationType, RefType}` | enum             | flow/process kind, param scope, allocation, ref |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model CRUD — `ProtoClient` instance methods, bound on `Client`/`RestClient`

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `get(model_type, uid=None, name=None) -> E \| None` | fetch one full entity by id or name             |
|  [02]   | `get_all(model_type) -> list[E]`                    | every instance of a type (eager, small types)   |
|  [03]   | `get_descriptors(model_type) -> list[o.Ref]`        | lightweight `Ref` listing without full payloads |
|  [04]   | `get_descriptor(model_type, uid=None, name=None)`   | one `Ref` by id or name                         |
|  [05]   | `find(model_type, name)`                            | `Ref` by name                                   |
|  [06]   | `get_providers(flow=None) -> list[o.TechFlow]`      | flow providers                                  |
|  [07]   | `get_parameters(model_type, uid)`                   | entity parameters and redefinitions             |
|  [08]   | `put(model) -> o.Ref \| None`                       | insert/update one; returns the stored `Ref`     |
|  [09]   | `put_all(*models)`                                  | insert/update many                              |
|  [10]   | `put_source_file(source, file_data) -> bool`        | upload a base64 source document                 |
|  [11]   | `delete(model)` / `delete_all(*models)`             | remove one or many entities                     |
|  [12]   | `create_product_system(process, config=None)`       | auto-link a product system from a root process  |

[ENTRYPOINT_SCOPE]: calculation and result lifecycle — client schedules, `ProtoResult` polls and enumerates

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `calculate(setup) -> Result`                                        | schedule a calc; returns a `Result` handle |
|  [02]   | `simulate(setup) -> Result`                                         | Monte-Carlo handle                         |
|  [03]   | `wait_until_ready() -> o.ResultState`                               | block until ready                          |
|  [04]   | `get_state() -> o.ResultState`                                      | scheduled/ready/error state                |
|  [05]   | `simulate_next()`                                                   | advance one Monte-Carlo iteration          |
|  [06]   | `dispose()`                                                         | free the server result                     |
|  [07]   | `get_demand() -> o.TechFlowValue \| None`                           | the reference demand row                   |
|  [08]   | `get_tech_flows()` / `get_envi_flows()` / `get_impact_categories()` | tech-flow, intervention-flow, impact axes  |

[ENTRYPOINT_SCOPE]: result queries — `ProtoResult` instance methods; `_of` variants scope a total to one key, `path` walks the upstream tree

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `get_total_requirements()` / `get_total_requirements_of(tech_flow)`     | requirement vector, whole or per flow          |
|  [02]   | `get_scaling_factors()`                                                 | scaling-factor vector                          |
|  [03]   | `get_scaled_tech_flows_of(...)` / `get_unscaled_tech_flows_of(...)`     | scaled/unscaled per-flow requirement breakdown |
|  [04]   | `get_total_flows()` / `get_total_flow_value_of(envi_flow)`              | intervention totals, whole or per flow         |
|  [05]   | `get_direct_interventions_of(...)` / `get_total_interventions_of(...)`  | direct and total interventions per tech-flow   |
|  [06]   | `get_flow_contributions_of(...)` / `get_flow_intensities_of(...)`       | intervention contribution and intensity        |
|  [07]   | `get_upstream_interventions_of(envi_flow, path)`                        | upstream intervention tree                     |
|  [08]   | `get_grouped_flow_results_of(...)`                                      | grouped intervention results                   |
|  [09]   | `get_total_impacts()` / `get_total_impacts_of(...)`                     | impact totals, whole or per key                |
|  [10]   | `get_total_impact_value_of(...)`                                        | one impact value by key                        |
|  [11]   | `get_normalized_impacts()` / `get_weighted_impacts()`                   | normalized and weighted impacts                |
|  [12]   | `get_direct_impacts_of(tech_flow)` / `get_impact_contributions_of(...)` | direct and contribution impacts                |
|  [13]   | `get_impact_intensities_of(...)` / `get_impact_factors_of(...)`         | impact intensity and factors                   |
|  [14]   | `get_flow_impacts_of(...)`                                              | per-flow impact values                         |
|  [15]   | `get_upstream_impacts_of(impact, path)`                                 | upstream impact tree                           |
|  [16]   | `get_grouped_impact_results_of(...)`                                    | grouped impact results                         |
|  [17]   | `get_total_costs() -> o.CostValue` / `get_total_costs_of(...)`          | LCC totals, whole or per key                   |
|  [18]   | `get_cost_contributions()` / `get_direct_costs_of(...)`                 | cost contribution and direct                   |
|  [19]   | `get_cost_intensities_of(...)`                                          | cost intensity per key                         |
|  [20]   | `get_upstream_costs_of(path)`                                           | upstream cost tree                             |
|  [21]   | `get_grouped_cost_results()`                                            | grouped cost results                           |
|  [22]   | `get_sankey_graph(config) -> o.SankeyGraph`                             | contribution Sankey graph for visualization    |

[ENTRYPOINT_SCOPE]: model factories and codecs (`olca_schema as o` — every `new_*` is `o.`-prefixed)

| [INDEX] | [SURFACE]                                                                                | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `new_unit_group(name, ref_unit)` / `new_unit(name, factor=1.0)`                          | unit group and unit                           |
|  [02]   | `new_flow_property(name, unit_group)`                                                    | flow property                                 |
|  [03]   | `new_flow(name, flow_type, flow_property)`                                               | generic typed flow                            |
|  [04]   | `new_product` / `new_waste` / `new_elementary_flow`                                      | product, waste, elementary shortcuts          |
|  [05]   | `new_process(name)` / `new_exchange(process, flow, amount=1.0, unit=None)`               | process and exchange (auto `internal_id`)     |
|  [06]   | `new_input` / `new_output`                                                               | typed input/output exchanges                  |
|  [07]   | `new_parameter(name, value, scope=o.ParameterScope.GLOBAL_SCOPE)`                        | parameter (`GLOBAL`/`PROCESS`/`IMPACT` scope) |
|  [08]   | `new_location(name, code=None)`                                                          | location                                      |
|  [09]   | `new_{physical,economic,causal}_allocation_factor(...)`                                  | allocation-factor factories                   |
|  [10]   | `new_impact_category(name)` / `new_impact_factor(indicator, flow, value=1.0, unit=None)` | impact category and factor                    |
|  [11]   | `new_impact_method(name, *indicators)`                                                   | impact method over indicators                 |
|  [12]   | `as_ref(entity)` / `entity.to_ref()` / `o.Ref(ref_type=o.RefType.X, id=...)`             | entity → `Ref`, or construct by ref type + id |
|  [13]   | `entity.to_dict()` / `Type.from_dict(d)` / `entity.to_json()` / `Type.from_json(s)`      | dataclass wire codecs at the boundary         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ProtoClient` is the abstract contract that `Client` (JSON-RPC over a `requests.Session`), `RestClient` (REST), and the proto client all satisfy; pin a parameter to `ProtoClient` and pick the transport at the boundary, so CRUD and calculation code stays transport-agnostic.
- `Result` is a handle to a server-side result, not a materialized table: every `get_*` is a lazy IPC round trip against the live result id, so the result is `dispose()`d and large enumerations cost network — query only the axes needed.
- Calculation runs `setup → calculate → wait_until_ready → query → dispose`: `CalculationSetup(target=o.Ref(ref_type=RefType.ProductSystem|Process, id=...), impact_method, nw_set, amount, unit, with_costs)`, `calculate` returns immediately, `wait_until_ready()` polls `ResultState`, and `dispose()` runs in a `finally`.
- `ProtoClient` returns `None`/empty on RPC-level errors rather than raising, and transport failures surface from `requests`; a consumer checks the return and the `ResultState.error` field, never assuming readiness.
- `olca_schema` is decoupled data: the entity dataclasses carry no transport, so a model is built with `new_*` factories, linked with `as_ref`/`to_ref` or a direct `o.Ref(ref_type=..., id=...)`, pushed with `put`/`put_all`, and the `to_dict`/`from_dict`/`to_json`/`from_json` codecs are the only wire contract — authored, serialized, and diffed offline.

[STACKING]:
- `bw2io`(`.api/bw2io.md`): openLCA exports JSON-LD that `bw2io`'s JSON-LD importer ingests into Brightway while `olca-ipc` drives the openLCA side live — the two-engine LCA interchange the EPD/LCA owner spans.
- `bw2calc`(`.api/bw2calc.md`) / `bw2data`(`.api/bw2data.md`): `olca-ipc` is the remote openLCA compute+store, `bw2calc` over a `bw2data` project the in-process alternative for the same characterized result — a `get_total_impacts()` row is the openLCA counterpart of a `bw2calc` `score`.
- `epdx`(`.api/epdx.md`) / `openepd`(`.api/openepd.md`): openLCA 2.x models EPDs first-class (`o.RefType.Epd`), so an `epdx`-parsed ILCD+EPD or `openepd` payload maps onto a native EPD/process/flow + `o.ImpactFactor` set pushed via `put_all`, and conversely `get_total_impacts()` yields EPD-comparable indicators.
- `premise`(`.api/premise.md`): `premise.NewDatabase(...).write_db_to_olca(filepath)` exports a prospective ecoinvent database into openLCA — `olca-ipc` is the openLCA sink of premise's `write_db_to_olca` and calculates the forward-looking scenarios.
- `pandas`(`.api/pandas.md`) / `polars`(`.api/polars.md`): result rows (`list[o.ImpactValue]`/`list[o.EnviFlowValue]`/`list[o.TechFlowValue]`) project straight into a frame feeding the tabular/contract/profile rails; `to_dict` gives the same as a record stream.
- `msgspec`(`../../.api/msgspec.md`) / `pydantic`(`../../.api/pydantic.md`): the `olca_schema` `to_dict`/`from_dict` forms decode into `msgspec.Struct`/`pydantic` models at the wire for a typed internal carrier.
- within-lib: construct `Client(endpoint)`/`RestClient(url)` from the runtime `TransportResource` at the boundary; wrap the `requests`-backed calls in a `stamina` retry, run `wait_until_ready()` under an `anyio` deadline, span `calculate`→query via `structlog`/`opentelemetry` keyed by `CalculationSetup.target`/`ResultState.id`, key a calculation by the runtime `ContentIdentity` over the setup for reuse-ledger dedup, and release the `Result` in a `finally`/`async with`.

[LOCAL_ADMISSION]:
- `olca-ipc` is the sole live-openLCA client on the epd-lca rail; a folder composing it registers `olca-ipc` and `olca-schema` in the branch manifest and this catalog.

[RAIL_LAW]:
- Package: `olca-ipc` (+ `olca-schema`)
- Owns: the openLCA IPC/REST/proto client — model CRUD, product-system construction, calculate/simulate, and the full result-query surface — and, via `olca_schema`, the typed model graph with `new_*` factories and dict/JSON codecs.
- Accept: `import olca_ipc as ipc` + `import olca_schema as o`; `ProtoClient`-typed transport-agnostic code; the `setup → calculate → wait_until_ready → query → dispose` lifecycle with the result disposed in a `finally`; result rows projected into the tabular rail; models authored offline via factories and pushed with `put_all`.
- Reject: hand-rolled JSON-RPC against the openLCA socket when a client method exists; treating `Result` as a materialized table when it is a lazy server handle; skipping `dispose()`; re-implementing the LCA solver openLCA owns or holding the model as the system of record.
</content>
