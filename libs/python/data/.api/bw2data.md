# [PY_DATA_API_BW2DATA]

`bw2data` is the project and graph store of the Brightway cluster: it owns the on-disk project directory (`platformdirs`-resolved), the SQLite/`peewee` database of nodes (activities/products) and edges (exchanges), the LCIA metadata stores (`databases`/`methods`/`geomapping`/`normalizations`/`weightings` registries), the `bw2parameters` parameter system, the `whoosh` full-text search index, and — critically — the serialization of that graph into `bw_processing` datapackages plus the bridge functions that feed `bw2calc`. It is the system of record; `bw2calc` is stateless compute over what `bw2data.process()` emits. It never solves an LCA (`bw2calc` owns that), never authors the datapackage format (`bw_processing` owns that), and never re-implements the SQLite ORM, the unit registry, or the search index its dependencies provide.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2data`
- package: `bw2data`
- import: `import bw2data as bd`
- owner: `data`
- rail: lca-store (EPD/LCA cluster)
- version: `4.7`
- license: `BSD-3-Clause` (`LICENSE` ships in `dist-info/licenses/`; Copyright Chris Mutel)
- asset: pure Python (zero compiled extensions); SQLite via `peewee`, units via `pint`, search via `whoosh`, config via `pydantic-settings`. `Requires-Python >=3.9`
- depends-on: `peewee>=4.0.1` (the SQLite ORM behind nodes/edges), `bw_processing>=0.9.5` (the `process()` output format), `bw2parameters` (the parameter evaluator), `pydantic-settings` (the `config` BaseSettings), `platformdirs` (project directory resolution), `pint` (unit handling), `rapidfuzz` (fuzzy search), `deepdiff~=7.0.1` (change detection), `blinker` (the `signal` lifecycle hooks), `fsspec`, `lxml`, `numpy<3`, `scipy`, `deprecated`
- marker: COMPANION-GATED. Pinned `bw2data; python_version<'3.15'`. Pure-Python package; the gate is TRANSITIVE — `numpy<3`, `scipy`, `lxml`, `peewee`, `rapidfuzz` lack `cp315` wheels at admission, so the cluster pins `<3.15`. `assay api resolve bw2data` cannot reflect on the active `cp315` interpreter; this surface is verified against the real `bw2data 4.7` wheel on an isolated `cp313` install.
- entry points: library-only; no console script
- capability: project isolation and lifecycle, a node/edge product-system graph in SQLite with dict-proxy mutation, LCIA method/weighting/normalization stores, scoped (project/database/activity) parameters with recalculation, fuzzy + full-text search, graph->datapackage serialization (`process()`), dataframe export of nodes/edges, and the typed bridge that hands functional units and datapackages to `bw2calc`

## [02]-[CAPTURE]

[PUBLIC_TYPES]: singletons and graph proxies
- `bd.projects` — the `ProjectManager` singleton; the active-project switch that scopes every other object.
- `bd.Database(name, backend='sqlite') -> ProcessedDataStore` — the `DatabaseChooser` factory; returns a backend instance (the default `SQLiteBackend`) for the named database. Polymorphic on `backend`.
- `bd.Node` / `bd.get_node(**kwargs)` / `bd.get_activity(key=None, **kwargs)` / `bd.get_id(key)` — node lookup; `Node` is the activity proxy (`MutableMapping` over the row fields) with `.id` and `.key`.
- `bd.Edge` — the exchange proxy (`MutableMapping`) with `.amount`, `.input`, `.output`, `.unit`, `.uncertainty`, `.uncertainty_type`.
- `bd.Method(name)` / `bd.methods` — the LCIA characterization store (`Methods` registry); `bd.Weighting`/`bd.weightings`, `bd.Normalization`/`bd.normalizations` — the parallel weighting/normalization stores.
- `bd.databases` (the `Databases` registry), `bd.geomapping`, `bd.mapping` — serialized id/metadata registries.
- `bd.parameters` — the `ParameterManager` singleton (project/database/activity parameter scopes).
- `bd.config` — the `pydantic-settings` settings object (`backends`, `biosphere`, `cache`, `global_location`, `sqlite3_databases`, `version`); `bd.preferences`, `bd.labels` — runtime configuration and the exchange-kind / field label vocabulary.
- `bd.DataStore` / `bd.ProcessedDataStore` — the store base classes; a `ProcessedDataStore` is anything that serializes itself to a `bw_processing` datapackage via `.process()`.
- `bd.Searcher` / `bd.IndexManager` — the `whoosh` full-text search index manager.
- `bd.calculation_setups` / `bd.dynamic_calculation_setups` — named `{name: {'inv': [...], 'ia': [...]}}` batch specs consumed by `MultiLCA`.
- `bd.JsonWrapper`, `bd.convert_backend(database_name, backend)`, `bd.set_data_dir(dirpath, permanent=True)`, `bd.extract_brightway_databases(...)`.

[PROJECT_LIFECYCLE]: `bd.projects` (`ProjectManager`)
- `create_project(name)`, `set_current(name)`, `current`, `delete_project(name=None, delete_dir=False)`, `copy_project(new_name)`, `rename_project(...)`, `report()`.
- directories: `dir`, `output_dir`, `logs_dir`, `request_directory(name)` — the project-scoped paths; `purge_deleted_directories()`.
- `migrate_project_25()` (legacy->2.5 layout), `use_short_hash`/`use_full_hash` (activity-hash width), `read_only`, `dataset`, `db`.

[GRAPH_STORE]: the `SQLiteBackend` returned by `Database(name)`
- write/read: `write(data, process=True, searchable=True, check_typos=True, signal=None)` (a `{(db, code): activity_dict}` bulk write), `load()`, `get(code=None, **kwargs)`, `random(filters=True, true_random=False)`, `search(string, **kwargs)`, `query(*queries)`.
- construct: `new_node(code=None, **kwargs)` / `new_activity(code, **kwargs)` -> `Node`; `register(write_empty=True, **kwargs)` / `deregister()`.
- serialize: `process(csv=False)` -> emits the `bw_processing` datapackage; `datapackage()` returns it; `filepath_processed()` / `dirpath_processed()`.
- export: `nodes_to_dataframe(columns=None, return_sorted=True) -> pandas.DataFrame`, `edges_to_dataframe(categorical=True, formatters=None) -> pandas.DataFrame`, `graph_technosphere(filename=None, **kwargs)`.
- maintain: `copy(name)`, `rename(name)`, `delete(keep_params=False, vacuum=True, signal=True)`, `copy_activities(activities, target_database, signal=True)`, `find_dependents(...)`, `find_graph_dependents()`, `delete_duplicate_exchanges(fields=['amount','type'])`, `set_geocollections()`, `make_searchable(reset=False)`/`make_unsearchable()`, `relabel_data(data, old_name, new_name)`; `.metadata`, `.filters`, `.order_by`.

[NODE_EDGE]: the dict-proxy graph surface
- `Node` (activity): `exchanges()`, `technosphere()`, `biosphere()`, `production()`, `substitution()`, `consumers(kinds=[...])`, `upstream(kinds=[...])`, `producers()`, `edges()` — the typed exchange iterators; `new_edge(**kwargs)` / `new_exchange(**kwargs)`, `rp_exchange()` (reference-product exchange); `save(signal=True)`, `copy(code=None, **kwargs)`, `delete(signal=True)`, `create_aggregated_process(database=None, **kwargs)`; `lca(method=None, amount=1.0)` (one-activity convenience LCA); `valid(why=False)`, `as_dict()`; dict-like field access (`MutableMapping`) over name/unit/location/etc.
- `Edge` (exchange): `.amount`, `.input`, `.output`, `.unit`, `.uncertainty`, `.uncertainty_type`; `lca(method=None, amount=None)`, `random_sample(n=100)` (sample the uncertainty distribution), `save()`, `delete()`, `valid(why=False)`, `as_dict()`.

[LCIA_STORES]: `Method` / `Weighting` / `Normalization` (`ProcessedDataStore` subclasses)
- `Method(name)`: `register(**kwargs)`, `write(data, process=True)` (the `[(flow_key, cf), ...]` characterization rows), `load()`, `process(**extra_metadata)` -> the LCIA `bw_processing` datapackage, `datapackage()`, `metadata`, `validate(data)`, `copy(name=None)`, `make_searchable()`, `get_abbreviation()`, `add_geomappings(data)`, `delete(...)`, `backup()`.
- `bd.methods` / `bd.weightings` / `bd.normalizations` (the registries): `get`, `keys`, `items`, `values`, `flush`, `clean`, `random`, `list`, `version`, `pack`/`unpack`, `serialize`/`deserialize`, `backup`.

[PARAMETERS]: `bd.parameters` (`ParameterManager`)
- `new_project_parameters(data)`, `new_database_parameters(data, database)`, `new_activity_parameters(data, group)` — the three scopes; `add_to_group(group, activity)`, `add_exchanges_to_group(group, activity)`, `recalculate()` (re-evaluate the `bw2parameters` formula graph), `remove_from_group(...)`, `remove_exchanges_from_group(...)`, `rename_project_parameter`/`rename_database_parameter`/`rename_activity_parameter`.

[BRIDGE]: the `bw2calc` seam (the only supported hand-off)
- `prepare_lca_inputs(demand, method=None, weighting=None, normalization=None, demands=None, remapping=True, demand_database_last=True) -> (functional_unit, data_objs, remapping_dicts)` — resolves the demand keys to integer ids, collects the technosphere/biosphere/LCIA datapackages, and returns the tuple to splat into `bc.LCA(demand=fu, data_objs=data_objs, remapping_dicts=[remapping])`.
- `get_multilca_data_objs(functional_units, method_config) -> list[DatapackageBase]` — the `MultiLCA` input builder, keyed by a `bw2calc.MethodConfig`-shaped dict.
- `get_node(**kwargs)` / `get_activity(key=None, **kwargs)` / `get_id(key)` — resolve a node by `database`/`code`/`name`/`id` kwargs or a `(database, code)` key tuple.

[IMPLEMENTATION_LAW]:
- The project is the unit of isolation: `projects.set_current(name)` switches the active SQLite database, datapackage directory, parameters, and registries. Every store object reads the *current* project; switch first, then act.
- `Database()` is polymorphic on backend and RETURNS a backend instance — it is a factory, not a class. `db = Database('ei'); db.write({...})` persists the graph; `db.process()` serializes it to the `bw_processing` datapackage `bw2calc` reads. Mutation is bulk via `write` or per-node via `new_node(...).save()`.
- `get_node`/`get_activity`/`get_id` are the polymorphic lookups — discriminate by kwargs, never proliferate `get_by_code`/`get_by_name` variants. `Node`/`Edge` are dict-like proxies over `peewee` rows: mutate keys then `.save()`; `.technosphere()`/`.biosphere()`/`.production()` filter exchanges by kind from the `labels` vocabulary.
- `process()` is the serialization seam: a `ProcessedDataStore` (`Database`, `Method`, `Weighting`, `Normalization`) writes its COO contribution into a `bw_processing` `Datapackage` via `add_persistent_vector`. Never hand-author that datapackage; call `process()`.
- Parameters are a formula graph (`bw2parameters`): set project/database/activity parameters, then `parameters.recalculate()` propagates the dependent exchange amounts. Lifecycle hooks fire through `blinker` signals (`signal=True`).

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `pydantic-settings`: `bd.config` IS a `pydantic-settings` BaseSettings — read/override it through the settings rail, never mutate global module state.
- `pandas` -> `narwhals`/`polars`: `nodes_to_dataframe()` / `edges_to_dataframe()` are the tabular hand-off; pull the graph into the columnar rail for analytics instead of iterating proxies.
- `structlog` / `opentelemetry`: `bw2data` emits structured log events through `structlog`; wrap a database `write`/`process` in a span so ingestion of a large background database is observable.
- `universal-pathlib` / `fsspec`: project directories resolve through `platformdirs`, and the emitted datapackages are `fsspec`-backed (`bw_processing`); route the datapackage output to the artifact store via the `fsspec` filesystem.
- `msgspec`: an activity/exchange is a plain JSON-able dict — type the node payload with a `msgspec.Struct` at the ingestion boundary so the graph write is schema-checked, then hand the dict to `Database.write`.

[SIBLING_STACK]: stacking with the EPD/LCA folder cluster
- `bw2calc`: `prepare_lca_inputs` / `get_multilca_data_objs` are the bridge; `bw2data` is the store, `bw2calc` the stateless engine over its `process()` output.
- `bw_processing`: `process()` emits its `Datapackage`; `reindex` rebuilds matrix ids against the `bd.mapping` store.
- `bw2io`: the importer layer (its own catalog) that POPULATES `bw2data` from ecoinvent/SimaPro/Excel/ecospold sources; `bw2data` is the destination store.
- `premise`: reads a `bw2data` background database (`source_type='brightway'`) and writes prospective-scenario `bw2data` databases via `write_db_to_brightway` (future electricity mixes, etc.); install `premise[bw25]` so it binds this `bw2data 4.x` store, not the legacy bw2 line.
- `openepd` / `epdx`: EPD declarations ingested as `bw2data` activities (or reconciled against `bw2calc` results computed over this graph).
- `olca-ipc`: the remote openLCA alternative store+engine, chosen when the inventory lives in an openLCA server rather than a Brightway project.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bw2data`
- Owns: project isolation and lifecycle, the SQLite/`peewee` node+edge product-system graph with dict-proxy mutation, the LCIA method/weighting/normalization stores, the scoped parameter system with recalculation, fuzzy + full-text search, graph->datapackage serialization (`process()`), dataframe export, and the `bw2calc` bridge
- Accept: `projects.set_current` as the isolation switch; `Database(name)` as the graph factory and `db.write`/`new_node(...).save()` as the mutation surface; `get_node`/`get_activity`/`get_id` as the polymorphic lookup; `Node`/`Edge` dict-proxy mutate-then-`save`; `.technosphere()`/`.biosphere()`/`.production()` as the typed exchange iterators; `prepare_lca_inputs`/`get_multilca_data_objs` as the ONLY `bw2calc` hand-off; `nodes_to_dataframe`/`edges_to_dataframe` as the tabular export; `process()` as the serialization seam; `parameters.recalculate()` for the formula graph
- Reject: hand-assembling `data_objs` instead of `prepare_lca_inputs`; per-key getter proliferation when `get_node` discriminates by kwargs; raw `peewee`/SQLite access when the `Node`/`Edge` proxies and backend own it; re-implementing the datapackage serialization `bw_processing` owns; a parallel project/config store when `projects` + `config` own it; per-row Python iteration for analytics when `nodes_to_dataframe`/`edges_to_dataframe` exist
