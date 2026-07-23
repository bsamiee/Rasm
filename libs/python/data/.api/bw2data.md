# [PY_DATA_API_BW2DATA]

`bw2data` is the Brightway system of record: the `platformdirs`-resolved project directory, the SQLite/`peewee` node+edge product-system graph, the LCIA method/weighting/normalization stores, the `bw2parameters` scoped-parameter formula graph, the `whoosh` search index, and the graph→datapackage serialization (`process()`) that feeds `bw2calc`. `bw2calc` owns the solve, `bw_processing` the datapackage format, and `peewee`/`pint`/`whoosh` the ORM/units/search — `bw2data` composes them as the sole authoring surface, and `bw2calc` is stateless compute over its `process()` emission.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2data`
- package: `bw2data` (`BSD-3-Clause`, Chris Mutel)
- module: `import bw2data as bd`
- rail: lca-store (EPD/LCA cluster)
- asset: pure-Python `py3-none-any` purelib, ABI-agnostic; SQLite via `peewee`, units via `pint`, search via `whoosh`, config via `pydantic-settings`
- depends: `peewee` (node/edge ORM), `bw_processing` (`process()` output), `bw2parameters` (formula evaluator), `pydantic-settings` (`config`/`labels`), `platformdirs` (project dirs), `blinker` (`signal` hooks), `rapidfuzz` (fuzzy search); optional `multifunctional` self-registers an extra backend at import
- capability: project isolation and lifecycle, a node/edge SQLite graph with dict-proxy mutation, LCIA method/weighting/normalization stores, scoped parameters with recalculation, fuzzy + full-text search, graph→datapackage serialization, node/edge dataframe export, and the typed `bw2calc` bridge

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: singletons, graph proxies, stores, and registries

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `projects`                                          | singleton     | `ProjectManager` — active-project switch scoping every store |
|  [02]   | `Database`                                          | factory       | `DatabaseChooser(name, backend='sqlite')` backend instance   |
|  [03]   | `Node`                                              | proxy         | activity `MutableMapping` over row fields with `.id`/`.key`  |
|  [04]   | `Edge`                                              | proxy         | exchange `MutableMapping` with `.amount`/`.input`/`.output`  |
|  [05]   | `Method`/`Weighting`/`Normalization`                | store         | three LCIA characterization/weighting/normalization stores   |
|  [06]   | `DataStore`/`ProcessedDataStore`                    | base class    | store bases serializing via `.process()`                     |
|  [07]   | `parameters`                                        | singleton     | `ParameterManager` — project/database/activity param scopes  |
|  [08]   | `Searcher`/`IndexManager`                           | class         | `whoosh` full-text search-index manager                      |
|  [09]   | `databases`/`methods`/`weightings`/`normalizations` | registry      | serialized name→metadata registry stores                     |
|  [10]   | `geomapping`/`mapping`                              | registry      | id and geo metadata registries for the matrix id space       |
|  [11]   | `config`/`preferences`/`labels`                     | settings      | `pydantic-settings` config; exchange-kind + field labels     |
|  [12]   | `calculation_setups`/`dynamic_calculation_setups`   | registry      | named `{'inv', 'ia'}` batch specs `MultiLCA` consumes        |

## [03]-[ENTRYPOINTS]

[PROJECT_LIFECYCLE]: `bd.projects` (`ProjectManager`)
- lifecycle: `create_project(name=None, **kwargs)` `set_current(name, writable=True)` `delete_project(name=None, delete_dir=False)` `copy_project(new_name, switch=True)` `rename_project(new_name)`
- directories: `request_directory(name)` `migrate_project_25()` `purge_deleted_directories()` `report()`
- state: `current` `dir` `output_dir` `logs_dir` `read_only` `dataset` `db` `use_short_hash` `use_full_hash`

[GRAPH_STORE]: the `SQLiteBackend` returned by `Database(name)`
- write: `write(data, process=True, searchable=True, check_typos=True, signal=None)` `load()` `get(code=None, **kwargs)`
- build: `new_node(code=None, **kwargs)`/`new_activity(code, **kwargs)` `register(write_empty=True, **kwargs)`/`deregister()` `copy(name)`/`rename(name)`
- read: `random(filters=True, true_random=False)` `search(string, **kwargs)` `query(*queries)`
- serialize: `process(csv=False)` `datapackage()` `filepath_processed()`/`dirpath_processed()` `graph_technosphere(filename=None, **kwargs)`
- export: `nodes_to_dataframe(columns=None, return_sorted=True)` `edges_to_dataframe(categorical=True, formatters=None)`
- maintain: `delete(keep_params=False, warn=True, vacuum=True, signal=True)` `copy_activities(activities, target_database, signal=True)` `delete_duplicate_exchanges(fields=['amount', 'type'])`
- maintain: `find_dependents()`/`find_graph_dependents()` `make_searchable(reset=False)`/`make_unsearchable()` `set_geocollections()` `relabel_data(data, old_name, new_name)`
- state: `metadata` `filters` `order_by`

[NODE_EDGE]: the dict-proxy graph surface
- `Node` iterators: `exchanges()` `edges()` `technosphere()` `biosphere()` `production(include_substitution=False)` `substitution()` `producers()` `consumers(kinds=)` `upstream(kinds=)` `rp_exchange()`
- `Node` ops: `new_edge(**kwargs)`/`new_exchange(**kwargs)` `save(signal=True)` `copy(code=None, signal=True, **kwargs)` `delete(signal=True)` `create_aggregated_process(database=None, **kwargs)` `lca(method=None, amount=1.0)` `valid(why=False)` `as_dict()`
- `Edge`: `.amount` `.input` `.output` `.unit` `.uncertainty` `.uncertainty_type`; `lca(method=None, amount=None)` `random_sample(n=100)` `save()` `delete()` `valid(why=False)` `as_dict()`

[LCIA_STORES]: `Method`/`Weighting`/`Normalization` (`ProcessedDataStore` subclasses)
- store ops: `write(data, process=True)` `process(**extra_metadata)` `register(**kwargs)` `load()` `validate(data)` `copy(name)` `add_geomappings(data)` `make_searchable(reset=False)` `backup()`
- registries `methods`/`weightings`/`normalizations`: `keys` `values` `items` `random` `list` `flush` `clean` `pack`/`unpack` `serialize`/`deserialize` `backup`; `databases.version(database)`

[PARAMETERS]: `bd.parameters` (`ParameterManager`)
- scopes: `new_project_parameters(data, overwrite=True)` `new_database_parameters(data, database, overwrite=True)` `new_activity_parameters(data, group, overwrite=True)`
- group + eval: `add_to_group(group, activity)` `add_exchanges_to_group(group, activity)` `recalculate(signal=True)` `remove_from_group(group, activity, restore_amounts=True)` `remove_exchanges_from_group(group, activity, restore_original=True)`
- rename: `rename_project_parameter(parameter, new_name, update_dependencies=False)` and the `_database_`/`_activity_` peers

[BRIDGE]: the `bw2calc` seam and node lookups
- `prepare_lca_inputs(demand=None, method=None, weighting=None, normalization=None, demands=None, remapping=True, demand_database_last=True) -> (functional_unit, data_objs, remapping_dicts)`
- `get_multilca_data_objs(functional_units, method_config) -> list[DatapackageBase]` — builds the `MultiLCA` datapackage inputs keyed by a `bw2calc.MethodConfig`-shaped dict
- lookup: `get_node(**kwargs)` `get_activity(key=None, **kwargs)` `get_id(key)` — discriminate by `database`/`code`/`name`/`id` kwargs or a `(database, code)` key
- `prepare_lca_inputs`: raises `Brightway2Project` until `projects.migrate_project_25()` has run — migrate a legacy project before the first hand-off.

[MODULE]: `convert_backend(database_name, backend)` `set_data_dir(dirpath, permanent=True)` `extract_brightway_databases(...)` `JsonWrapper`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Project is the unit of isolation: `projects.set_current(name)` switches the active SQLite database, datapackage directory, parameters, and registries; every store reads the current project, so switch first then act.
- `Database` is a factory polymorphic on `backend` returning a backend instance, never a class; `db.write({...})` bulk-persists the graph and `db.process()` serializes it to the `bw_processing` datapackage `bw2calc` reads, mutating bulk via `write` or per-node via `new_node(...).save()`.
- `get_node`/`get_activity`/`get_id` discriminate by kwargs as one polymorphic lookup; `Node`/`Edge` are dict proxies over `peewee` rows — mutate keys then `.save()`, and `.technosphere()`/`.biosphere()`/`.production()` filter exchanges by the `labels` kind vocabulary.
- `process()` is the serialization seam: a `ProcessedDataStore` writes its COO contribution into a `bw_processing` `Datapackage` via `add_persistent_vector`.
- Parameters are a `bw2parameters` formula graph: set project/database/activity parameters then `parameters.recalculate()` propagates the dependent exchange amounts; lifecycle hooks fire through `blinker` signals under `signal=True`.

[STACKING]:
- `bw2calc`(`.api/bw2calc.md`): `prepare_lca_inputs`/`get_multilca_data_objs` are the sole hand-off, resolving demand keys to integer ids and returning `(functional_unit, data_objs, remapping_dicts)` to splat into `bc.LCA`; `bw2data` stores, `bw2calc` solves over `process()`.
- `bw-processing`(`.api/bw-processing.md`): `process()` emits its `Datapackage`; `reindex` rebuilds matrix ids against the `mapping` store.
- `bw2io`(`.api/bw2io.md`): the importer populating this store from ecoinvent/SimaPro/Excel/ecospold; `bw2data` is the destination.
- `premise`(`.api/premise.md`): reads a `bw2data` background (`source_type='brightway'`) and writes prospective-scenario databases via `write_db_to_brightway`; `premise[bw25]` binds this store.
- `openepd`(`.api/openepd.md`)/`epdx`(`.api/epdx.md`): EPD declarations ingest as `bw2data` activities or reconcile against `bw2calc` results over this graph.
- `olca-ipc`(`.api/olca-ipc.md`): the remote openLCA store+engine alternative when the inventory lives on an openLCA server.
- substrate rails (`libs/python/.api/`): `bd.config` IS a `pydantic-settings` BaseSettings — read/override through the settings rail; `nodes_to_dataframe`/`edges_to_dataframe` hand the graph to the `narwhals`/`polars` columnar rail; project dirs and datapackages resolve `fsspec`-backed via `platformdirs`/`universal-pathlib`; a `write`/`process` wraps in a `structlog`/`opentelemetry` span; a `msgspec.Struct` types the node payload at the ingestion boundary before `Database.write`.

[LOCAL_ADMISSION]:
- Admitted unpinned as the `impact` cluster's system of record; the project graph is the sole authoring surface and `bw2calc` the only compute consumer of its `process()` emission.

[RAIL_LAW]:
- Package: `bw2data`
- Owns: project isolation and lifecycle, the SQLite/`peewee` node+edge graph with dict-proxy mutation, the LCIA method/weighting/normalization stores, the scoped parameter system with recalculation, fuzzy + full-text search, graph→datapackage serialization, dataframe export, and the `bw2calc` bridge
- Accept: `projects.set_current` isolation switch; `Database(name)` factory with `db.write`/`new_node(...).save()` mutation; `get_node`/`get_activity`/`get_id` polymorphic lookup; `Node`/`Edge` mutate-then-`save`; `.technosphere()`/`.biosphere()`/`.production()` typed iterators; `prepare_lca_inputs`/`get_multilca_data_objs` as the sole `bw2calc` hand-off; `nodes_to_dataframe`/`edges_to_dataframe` export; `process()` serialization; `parameters.recalculate()`
- Reject: hand-assembling `data_objs` past `prepare_lca_inputs`; per-key getter proliferation where `get_node` discriminates by kwargs; raw `peewee`/SQLite where the `Node`/`Edge` proxies own it; re-implementing the `bw_processing` datapackage serialization; a parallel project/config store past `projects`+`config`; per-row iteration for analytics where the dataframe exports exist
