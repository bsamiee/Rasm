# [PY_DATA_API_BW2IO]

`bw2io` is the ingestion leg of the Brightway 2.5 LCA stack: it extracts external LCI/LCIA inventory formats, links their exchanges through a composable strategy pipeline onto a biosphere and sibling databases, and writes the linked graph into a `bw2data` `Database` for `bw2calc` to solve. It owns the biosphere/LCIA/migration bootstrap, the `randonneur` migration registry, GEXF/matrix export, and the `BW2Package` interchange format; it never solves the matrix (`bw2calc`/`bw_processing`) nor owns the project/graph store (`bw2data`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2io`
- package: `bw2io` (BSD-3-Clause)
- module: `bw2io`
- namespaces: `bw2io.importers`, `bw2io.strategies`, `bw2io.errors`, `bw2io.export`
- owner: `data`
- rail: epd-lca (LCI/LCIA ingestion)
- depends: `bw2data` (project/`Database` store), `bw2calc` (downstream solver), `bw_processing` (matrix datapackage), `bw2parameters`, `randonneur` + `randonneur_data` (migration verbs/registry), `pyecospold`, `stats_arrays`, `lxml`, `openpyxl`/`xlrd`/`xlsxwriter`, `voluptuous`, `SPARQLWrapper`, `numpy`, `scipy`, `tqdm`, `requests`; optional `ecoinvent_interface` gates `import_ecoinvent_release`, `multifunctional` + `bw_simapro_csv` gate multifunctional DBs and `SimaProBlockCSVImporter`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `LCIImporter` subclasses — one source format each, written to a `bw2data` `Database`

| [INDEX] | [SYMBOL]                        | [CAPABILITY]                    |
| :-----: | :------------------------------ | :------------------------------ |
|  [01]   | `SingleOutputEcospold2Importer` | ecospold2 single-output process |
|  [02]   | `SingleOutputEcospold1Importer` | ecospold1 single-output         |
|  [03]   | `MultiOutputEcospold1Importer`  | ecospold1 multi-output          |
|  [04]   | `SimaProCSVImporter`            | SimaPro inventory CSV           |
|  [05]   | `SimaProBlockCSVImporter`       | SimaPro block CSV               |
|  [06]   | `CSVImporter`                   | Brightway CSV template          |
|  [07]   | `ExcelImporter`                 | Brightway `.xlsx` template      |
|  [08]   | `Exiobase3MonetaryImporter`     | ExioBase 3 monetary EE-IO       |
|  [09]   | `Exiobase3HybridImporter`       | ExioBase 3 hybrid EE-IO         |

- `SimaProBlockCSVImporter`: present only with `bw_simapro_csv` installed.

[PUBLIC_TYPE_SCOPE]: `LCIAImporter` subclasses — characterization-method import, written to `bw2data` methods

| [INDEX] | [SYMBOL]                 | [CAPABILITY]            |
| :-----: | :----------------------- | :---------------------- |
|  [01]   | `ExcelLCIAImporter`      | `.xlsx` CF method table |
|  [02]   | `CSVLCIAImporter`        | CSV CF method table     |
|  [03]   | `Ecospold1LCIAImporter`  | ecospold1 method XML    |
|  [04]   | `SimaProLCIACSVImporter` | SimaPro method CSV      |

[PUBLIC_TYPE_SCOPE]: pipeline base and interchange types
- [01]-[LCI_BASE]: `bw2io.importers.base_lci.LCIImporter` — the LCI pipeline contract every LCI importer inherits; the integration spine ([03]).
- [02]-[LCIA_BASE]: `bw2io.importers.base_lcia.LCIAImporter` — the LCIA pipeline base.
- [03]-[PACKAGE]: `BW2Package` — `.bw2package` export/import (`export_obj`/`export_objs`/`load_file`/`import_file`) for portable database/method interchange.
- [04]-[MIGRATION]: `Migration` / `migrations` — a named field-remap migration and the registry into `bw2data.config.metadata`.
- [05]-[UNLINKED]: `UnlinkedData` / `unlinked_data` — persisted unlinked-flow records and the cross-session registry.
- [06]-[ENRICHMENT]: `ChemIDPlus` — CAS/chemical-identifier resolver for biosphere-flow enrichment.
- [07]-[ERRORS]: `bw2io.errors.{StrategyError, NonuniqueCode, WrongDatabase}` — strategy/link failure against an absent external database or invalid linking config, duplicate `code`, and wrong-target-database guards raised by the pipeline.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: project bootstrap (module functions)

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `bw2setup()`                                                                                  | biosphere3 + LCIA + core migrations |
|  [02]   | `create_default_biosphere3(overwrite=False)`                                                  | write the `biosphere3` database     |
|  [03]   | `create_default_lcia_methods(overwrite=False, rationalize_method_names=False, shortcut=True)` | install bundled LCIA method pack    |
|  [04]   | `create_core_migrations()`                                                                    | register built-in field remaps      |
|  [05]   | `add_ecoinvent_3{3..9}_biosphere_flows(...)`                                                  | version-specific biosphere flows    |
|  [06]   | `add_example_database()` / `get_csv_example_filepath()` / `get_xlsx_example_filepath()`       | example DB and template paths       |

- `bw2setup`: idempotent on an existing biosphere.

[ENTRYPOINT_SCOPE]: `LCIImporter` contract — every method binds on the importer instance `imp`

| [INDEX] | [SURFACE]                                                                                   | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `imp = ImporterClass(filepath, db_name, ...)` then `imp.data`                               | extract source into `imp.data` |
|  [02]   | `imp.apply_strategies(strategies=None, verbose=True)` / `imp.apply_strategy(fn)`            | run the strategy pipeline      |
|  [03]   | `imp.statistics(print_stats=True)`                                                          | linking-quality receipt        |
|  [04]   | `imp.match_database(db_name=None, fields=None, relink=False, edge_kinds=None, ...)`         | link exchanges against a DB    |
|  [05]   | `imp.write_database(delete_existing=True, activate_parameters=False)`                       | persist the `bw2data.Database` |
|  [06]   | `imp.write_excel(...)` / `imp.create_randonneur_excel_template_for_unlinked(...)`           | matching/diagnostic export     |
|  [07]   | `imp.create_new_biosphere(name)` / `imp.add_unlinked_flows_to_biosphere_database(...)`      | promote unlinked flows         |
|  [08]   | `imp.migrate(migration_name)` / `imp.randonneur(verbs=SAFE_VERBS, migrate_edges=True, ...)` | apply migration / transform    |
|  [09]   | `imp.drop_unlinked(i_am_reckless=False)`                                                    | delete unlinked exchanges      |
|  [10]   | `imp.write_project_parameters(...)` / `imp.write_database_parameters(...)`                  | persist parameter sets         |

- `imp.data`: a `list[dict]` of nodes each carrying `exchanges`; `imp.strategies` is the default strategy list.
- `imp.statistics`: returns `(num_nodes, num_edges, num_unlinked, num_multifunctional)`; `imp.all_linked` is `num_unlinked == 0`.
- `imp.match_database`: `match_database_against_top_level_context(...)` and `match_database_against_only_available_in_given_context_tree(...)` are the context-tree fallbacks.
- `imp.write_database`: returns the `ProcessedDataStore`; guards duplicate `code` → `NonuniqueCode`, wrong target → `WrongDatabase`, and auto-selects `MultifunctionalDatabase`.
- `imp.add_unlinked_activities()`: promotes still-unlinked technosphere activities into a database and relinks.
- `imp.drop_unlinked`: raises unless `i_am_reckless=True`.
- LCIA importers mirror the shape: `apply_strategies()` → `write_methods(overwrite=False, verbose=True)`, with `add_missing_cfs()`, `drop_unlinked(verbose=True)`, `migrate(migration_name)`, and `statistics()`/`all_linked` as the receipt.

[ENTRYPOINT_SCOPE]: one-shot full-system imports (network/credentialed)

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------ | :--------------------------------- |
|  [01]   | `import_ecoinvent_release(version, system_model, ...)`                          | ecoinvent release end-to-end       |
|  [02]   | `exiobase_monetary(version=(3,8,1), year=2017, products=False, name=None, ...)` | EXIOBASE monetary IO table         |
|  [03]   | `useeio20(name="USEEIO-2.0", collapse_products=False, prune=False)`             | US EPA USEEIO 2.0 via JSON-LD      |
|  [04]   | `install_project(...)`                                                          | install a remote Brightway project |

- `import_ecoinvent_release`: needs `ecoinvent_interface` credentials; warns if absent.

[ENTRYPOINT_SCOPE]: export, backup, and utilities

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------------------ | :----------------------------- |
|  [01]   | `DatabaseToGEXF(database, ...)` / `DatabaseSelectionToGEXF(...)` / `keyword_to_gephi_graph(...)`  | GEXF graph export              |
|  [02]   | `lci_matrices_to_excel(database)` / `lci_matrices_to_matlab(database)`                            | matrix dump to `.xlsx`/`.mat`  |
|  [03]   | `backup_project_directory(...)` / `restore_project_directory(...)` / `backup_data_directory(...)` | tar-archive/restore project    |
|  [04]   | `activity_hash(dataset, fields=...)` / `es2_activity_hash(...)`                                   | deterministic node `code` hash |
|  [05]   | `normalize_units(data)` / `load_json_data_file(name)`                                             | unit normalization / JSON load |
|  [06]   | `bw2io.strategies.*`                                                                              | composable linking functions   |

- `bw2io.strategies`: `link_iterable_by_fields`, `assign_only_product_as_production`, `drop_unspecified_subcategories`, `strip_biosphere_exc_locations`, `normalize_units`, `drop_unlinked`, and `match_against_top_level_context` are the canonical linking functions `apply_strategies` runs.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Canonical flow `extract → apply_strategies → statistics → match/resolve → write_database`: construct an importer (extraction fills `imp.data` as `list[dict]` nodes carrying `exchanges`), run the strategy pipeline to link edges, read `statistics()`/`all_linked`, resolve the residual unlinked set (match another DB, add to biosphere, drop, or emit a `randonneur` template), then write to a `bw2data.Database`.
- `imp.data` is plain `list[dict]`, not a frame: strategies are pure `list[dict] -> list[dict]` functions, so a project-specific linker (EPD/material remap) is one function passed to `apply_strategy`, never a subclass.
- `write_database` mints the durable `Database`: it guards non-unique `code` (`NonuniqueCode`), enforces the single-target invariant (`WrongDatabase`), and auto-promotes to `MultifunctionalDatabase` when any node is multifunctional.
- Migrations are field-remap transforms: `migrate(name)` applies a registered `bw2io` migration; `randonneur(...)` applies the `randonneur`/`randonneur_data` verb set — `replace`/`update`/`disaggregate`, and `create`/`delete` when explicitly enabled — to edges and/or nodes.

[STACKING]:
- `bw2data`(`.api/bw2data.md`): `write_database` writes the `Database` and LCIA methods into the active project; `bw2io` is the ingestion front door, `bw2data` the store.
- `bw2calc`(`.api/bw2calc.md`): consumes the written database at solve time; ingestion runs once and computation never re-extracts.
- `bw_processing`(`.api/bw-processing.md`): the matrix-datapackage substrate `write_database`'s output serializes into.
- `premise`(`.api/premise.md`): calls `bw2io` strategies to import `additional_inventories` and normalize units before the prospective sector transforms; the `premise[bw25]` extra binds this Brightway-2.5 line, whose output databases `bw2calc` then scores.
- `openepd`/`epdx`(`.api/openepd.md`, `.api/epdx.md`): EPD material-impact rows map through a custom strategy + `ExcelImporter`/`CSVImporter` into a Brightway database so declared impacts join the computed inventory.
- `olca-ipc`(`.api/olca-ipc.md`): openLCA JSON-LD ingested via the JSON-LD importer, bridging the two-engine LCA interchange.
- `rustworkx`(`.api/rustworkx.md`): `DatabaseToGEXF`/`keyword_to_gephi_graph` hand the technosphere graph to the graph owner for topology analysis.
- `pandera`/`dataframely`(`.api/pandera.md`, `.api/dataframely.md`): `imp.data` and the unlinked set flatten to a frame for the contract gate, and `statistics()` becomes a `QualityProfile` row rather than a print.
- within-lib: `apply_strategies` folds an ordered `bw2io.strategies` list, a custom `list[dict] -> list[dict]` strategy composing into the same fold; wrap the one-shot imports in a `stamina` retry and `apply_strategies` in a `structlog`/`opentelemetry` span keyed by `db_name`, and key the imported database and `.bw2package` export by the runtime `ContentIdentity` for reuse dedup.

[LOCAL_ADMISSION]:
- `bw2io` is the sole ingestion path onto the EPD/LCA rail: a new source format is an `LCIImporter`/`LCIAImporter` subclass or a custom strategy, never a hand-rolled parser, and the written `bw2data.Database` is the only hand-off to `bw2calc`.

[RAIL_LAW]:
- Package: `bw2io`
- Owns: extraction of external LCI/LCIA formats, the strategy-pipeline linking model, `bw2data.Database`/method writing, the biosphere/LCIA/migration bootstrap, `randonneur` migrations, GEXF/matrix export, and `BW2Package` interchange
- Accept: the `extract → apply_strategies → statistics → match → write_database` pipeline; custom linking as a `list[dict] -> list[dict]` strategy passed to `apply_strategy`; `statistics()`/`all_linked` as the receipt; `randonneur`/`migrate` for field remaps; the one-shot `import_*`/`useeio20`/`exiobase_monetary` imports under a retry; `.bw2package` for portable interchange
- Reject: hand-rolled ecospold/SimaPro/Excel parsing when an importer owns the format; hand-rolled exchange linking when a `bw2io.strategies` function or `match_database` covers it; re-implementing the matrix build or LCA solve (`bw_processing`/`bw2calc`); treating `statistics()` as a print instead of a persisted quality receipt
