# [PY_DATA_API_BW2IO]

`bw2io` is the import/export and database-ingestion layer of the Brightway 2.5 LCA stack: it extracts life-cycle-inventory databases from external formats (ecoinvent ecospold1/2, SimaPro CSV, Excel/CSV, ExioBase, US-EEIO/JSON-LD, US LCI), runs them through a composable strategy pipeline that links exchanges to a biosphere and to other databases, and writes the linked graph into a `bw2data` `Database` that `bw2calc` then computes against. It owns the biosphere/LCIA/migration bootstrap (`bw2setup`), the `randonneur`-backed data-migration registry, GEXF graph export, and the `BW2Package` interchange format. It is the INGESTION leg of the data EPD/LCA owner — it never re-implements the matrix solver (`bw2calc`/`bw_processing`) or the project/storage layer (`bw2data`); it produces the database those own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw2io`
- package: `bw2io`
- version: `0.9.17`
- license: BSD-3-Clause
- module: `bw2io`
- owner: `data`
- rail: epd-lca (LCI/LCIA ingestion)
- depends: `bw2data>=4.6.2` (project/`Database`/`config`/`databases`/`parameters`), `bw2calc>=2.0` (LCA engine, downstream consumer), `bw_processing>=1.0` (matrix datapackage substrate), `bw2parameters`, `bw_migrations`, `randonneur>=0.6` + `randonneur_data>=0.5.4` (migration verbs/registry), `pyecospold`, `stats_arrays`, `lxml`, `openpyxl`/`xlrd`/`xlsxwriter`, `voluptuous`, `SPARQLWrapper`, `numpy<3`, `scipy`, `tqdm`, `requests`; optional `ecoinvent_interface` (gates `import_ecoinvent_release`), `multifunctional`+`bw_simapro_csv` (gate multifunctional DBs + `SimaProBlockCSVImporter`)
- evidence: assay-reflected — `bw2io 0.9.17` (`api resolve bw2io`), installed in the active env; `__all__` (46 names) carries the importer/bootstrap/export surface and `bw2io.importers.base_lci.LCIImporter` carries the pipeline contract
- capability: format extractors → strategy-pipeline linking → `bw2data.Database` write, for ecospold1/2, SimaPro CSV, Excel/CSV, ExioBase 3 (monetary/hybrid), US-EEIO + JSON-LD; LCIA-method import (Excel/CSV/ecospold1/SimaPro); biosphere/LCIA/migration bootstrap; `randonneur` migrations; GEXF export; `BW2Package` round-trip; `activity_hash` identity

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: LCI importers (subclass `LCIImporter`)
- rail: epd-lca

| [INDEX] | [SYMBOL]                                                         | [SOURCE_FORMAT]           | [ROLE]                                                                 |
| :-----: | :--------------------------------------------------------------- | :------------------------ | :--------------------------------------------------------------------- |
|  [01]   | `SingleOutputEcospold2Importer`                                  | ecospold2 (ecoinvent 3.x) | single-output unit-process ecospold2 XML                               |
|  [02]   | `SingleOutputEcospold1Importer` / `MultiOutputEcospold1Importer` | ecospold1                 | single- / multi-output ecospold1 XML                                   |
|  [03]   | `SimaProCSVImporter`                                             | SimaPro CSV               | SimaPro inventory CSV export                                           |
|  [04]   | `SimaProBlockCSVImporter`                                        | SimaPro CSV (block)       | block-parser variant (present only when `bw_simapro_csv` is installed) |
|  [05]   | `CSVImporter` / `ExcelImporter`                                  | generic CSV / `.xlsx`     | the Brightway tabular template format                                  |
|  [06]   | `Exiobase3MonetaryImporter` / `Exiobase3HybridImporter`          | ExioBase 3                | monetary / hybrid environmentally-extended IO tables                   |

[PUBLIC_TYPE_SCOPE]: LCIA importers (subclass `LCIAImporter`)
- rail: epd-lca

| [INDEX] | [SYMBOL]                                | [SOURCE_FORMAT]  | [ROLE]                                |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `ExcelLCIAImporter` / `CSVLCIAImporter` | `.xlsx` / CSV    | characterization-factor method tables |
|  [02]   | `Ecospold1LCIAImporter`                 | ecospold1 LCIA   | ecospold1 impact-method XML           |
|  [03]   | `SimaProLCIACSVImporter`                | SimaPro LCIA CSV | SimaPro method CSV                    |

[PUBLIC_TYPE_SCOPE]: pipeline base + interchange types
- rail: epd-lca

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]     | [ROLE]                                                                                                                                                                   |
| :-----: | :----------------------------------------------------------- | :---------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `bw2io.importers.base_lci.LCIImporter`                       | importer base     | the shared LCI pipeline contract every LCI importer inherits (`apply_strategies`, `statistics`, `match_database`, `write_database`, …); the integration spine — see [03] |
|  [02]   | `bw2io.importers.base_lcia.LCIAImporter`                     | importer base     | LCIA pipeline base — `apply_strategies`, `write_methods`, `add_missing_cfs`, `drop_unlinked`, `migrate`, `statistics`/`all_linked`                                       |
|  [03]   | `BW2Package`                                                 | package codec     | Brightway-native `.bw2package` export/import (`export_obj`/`export_objs`/`load_file`/`import_file`) for portable database/method interchange                             |
|  [04]   | `Migration` / `migrations`                                   | migration store   | a named data-migration (field remaps) and the registry registered into `bw2data.config.metadata`                                                                         |
|  [05]   | `UnlinkedData` / `unlinked_data`                             | diagnostics store | persisted unlinked-flow records and the registry for cross-session unlinked inspection                                                                                   |
|  [06]   | `ChemIDPlus`                                                 | enrichment        | CAS/chemical-identifier resolver for biosphere-flow enrichment                                                                                                           |
|  [07]   | `bw2io.errors.{StrategyError, NonuniqueCode, WrongDatabase}` | error rail        | strategy/linking failure, duplicate `code`, and wrong-target-database guards raised by the pipeline                                                                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: project bootstrap
- rail: epd-lca

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY]  | [RAIL]                                                                                                              |
| :-----: | :-------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `bw2setup()`                                                                                  | one-shot setup  | create biosphere3 + default LCIA methods + core migrations in the active project (idempotent on existing biosphere) |
|  [02]   | `create_default_biosphere3(overwrite=False)`                                                  | biosphere       | write the `biosphere3` elementary-flow database via `Ecospold2BiosphereImporter`                                    |
|  [03]   | `create_default_lcia_methods(overwrite=False, rationalize_method_names=False, shortcut=True)` | LCIA            | install the bundled ecoinvent 3.9 LCIA method pack (`shortcut`) or import fresh                                     |
|  [04]   | `create_core_migrations()`                                                                    | migrations      | register the built-in field-remap migrations                                                                        |
|  [05]   | `add_ecoinvent_3{3..9}_biosphere_flows(...)`                                                  | biosphere patch | add version-specific ecoinvent biosphere flows for cross-version linking                                            |
|  [06]   | `add_example_database()` / `get_csv_example_filepath()` / `get_xlsx_example_filepath()`       | fixtures        | example DB + template paths for tests/onboarding                                                                    |

[ENTRYPOINT_SCOPE]: importer pipeline (LCIImporter contract — bound methods on every LCI importer)
- rail: epd-lca

| [INDEX] | [SURFACE]                                                                                                                                                              | [ENTRY_FAMILY]   | [RAIL]                                                                                                                                                                                           |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `imp = ImporterClass(filepath, db_name, ...)` then `imp.data`                                                                                                          | construct        | extract the source into `self.data` (a `list[dict]` of nodes each carrying `exchanges`); `self.strategies` is the default strategy list                                                          |
|  [02]   | `imp.apply_strategies(strategies=None, verbose=True)` / `imp.apply_strategy(fn)`                                                                                       | link             | run the strategy pipeline (or one strategy) that normalizes units, drops subcategories, assigns production, and links exchanges                                                                  |
|  [03]   | `imp.statistics(print_stats=True) -> (num_nodes, num_edges, num_unlinked, num_multifunctional)`                                                                        | receipt          | the linking-quality tuple; `imp.all_linked` is `num_unlinked == 0`                                                                                                                               |
|  [04]   | `imp.match_database(db_name=None, fields=None, ignore_categories=False, relink=False, edge_kinds=None, ...)`                                                           | match            | link exchanges against self or another `Database` by field tuple; `match_database_against_top_level_context(...)` and `..._only_available_in_given_context_tree(...)` for context-tree fallbacks |
|  [05]   | `imp.write_database(data=None, delete_existing=True, backend=None, activate_parameters=False, db_name=None, searchable=True, check_typos=False) -> ProcessedDataStore` | write            | persist the linked graph as a `bw2data.Database` (guards duplicate `code` → `NonuniqueCode`, wrong target → `WrongDatabase`); auto-selects `MultifunctionalDatabase` when needed                 |
|  [06]   | `imp.write_excel(only_unlinked=False, only_names=False) -> Path` / `imp.create_randonneur_excel_template_for_unlinked(...) -> Path`                                    | matching IO      | export a matching/diagnostic spreadsheet or a `randonneur` remap template for the unlinked edges                                                                                                 |
|  [07]   | `imp.create_new_biosphere(name)` / `imp.add_unlinked_flows_to_biosphere_database(...)` / `imp.add_unlinked_activities()`                                               | unlinked resolve | promote unlinked biosphere flows / technosphere activities into a (new) database and relink                                                                                                      |
|  [08]   | `imp.migrate(migration_name)` / `imp.randonneur(label=None, datapackage=None, verbs=rn.utils.SAFE_VERBS, migrate_edges=True, migrate_nodes=False, ...)`                | migrate          | apply a registered `bw2io` migration or a `randonneur`/`randonneur_data` transformation to `self.data`                                                                                           |
|  [09]   | `imp.drop_unlinked(i_am_reckless=False)`                                                                                                                               | prune            | delete every still-unlinked exchange (guarded; requires the keyword)                                                                                                                             |
|  [10]   | `imp.write_project_parameters(...)` / `imp.write_database_parameters(...)`                                                                                             | parameters       | persist project/database `bw2parameters` parameter sets                                                                                                                                          |

LCIA importers mirror the shape: `apply_strategies()` → `write_methods(overwrite=False, verbose=True)`, with `add_missing_cfs()` to fill missing characterization factors, `drop_unlinked(verbose=True)` to prune, `migrate(migration_name)` for field remaps, and `statistics()`/`all_linked` as the linking receipt.

[ENTRYPOINT_SCOPE]: one-shot full-system imports
- rail: epd-lca

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                                                                                                 |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `import_ecoinvent_release(version, system_model, ...)`                          | ecoinvent      | download + import an ecoinvent release end-to-end (requires `ecoinvent_interface` credentials; a stub warns if absent) |
|  [02]   | `exiobase_monetary(version=(3,8,1), year=2017, products=False, name=None, ...)` | ExioBase       | download (Zenodo) + import an EXIOBASE monetary IO table as a database                                                 |
|  [03]   | `useeio20(name="USEEIO-2.0", collapse_products=False, prune=False)`             | US-EEIO        | download + JSON-LD import the US EPA USEEIO 2.0 model + its LCIA methods                                               |
|  [04]   | `install_project(...)`                                                          | remote project | install a packaged remote Brightway project                                                                            |

[ENTRYPOINT_SCOPE]: export, backup, and utilities
- rail: epd-lca

| [INDEX] | [SURFACE]                                                                                         | [ENTRY_FAMILY]   | [RAIL]                                                                                                                                                                                                                                                                      |
| :-----: | :------------------------------------------------------------------------------------------------ | :--------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DatabaseToGEXF(database, ...)` / `DatabaseSelectionToGEXF(...)` / `keyword_to_gephi_graph(...)`  | graph export     | export a database (or selection) to a GEXF graph for Gephi/`networkx`                                                                                                                                                                                                       |
|  [02]   | `lci_matrices_to_excel(database)` / `lci_matrices_to_matlab(database)`                            | matrix export    | dump the technosphere/biosphere matrices to `.xlsx` / `.mat`                                                                                                                                                                                                                |
|  [03]   | `backup_project_directory(...)` / `restore_project_directory(...)` / `backup_data_directory(...)` | backup           | tar-archive and restore a project / the data directory                                                                                                                                                                                                                      |
|  [04]   | `activity_hash(dataset, fields=...)` / `es2_activity_hash(...)`                                   | identity         | deterministic node hash used as the `code` for unlinked/biosphere nodes                                                                                                                                                                                                     |
|  [05]   | `normalize_units(data)` / `load_json_data_file(name)`                                             | utility          | unit normalization and bundled JSON data loading                                                                                                                                                                                                                            |
|  [06]   | `bw2io.strategies.*`                                                                              | strategy library | the composable linking functions (`link_iterable_by_fields`, `assign_only_product_as_production`, `drop_unspecified_subcategories`, `strip_biosphere_exc_locations`, `normalize_units`, `drop_unlinked`, `match_against_top_level_context`, …) that `apply_strategies` runs |

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_TOPOLOGY]:
- the canonical flow is `extract → apply_strategies → statistics → match/resolve → write_database`: construct an importer (extraction fills `self.data` as `list[dict]` nodes carrying `exchanges`), run the strategy pipeline to link edges, read `statistics()`/`all_linked` to gauge linking, resolve the residual unlinked set (match against another DB, add to biosphere, drop, or emit a `randonneur` remap template), then write to a `bw2data.Database`
- `self.data` is plain Python data (a list of dicts), NOT a frame — strategies are pure `list[dict] -> list[dict]` functions, so a custom strategy is just a function passed to `apply_strategy`; this is the extension point for project-specific linking (EPD/material remaps) rather than subclassing
- `write_database` is the boundary that mints the durable `Database`: it guards non-unique `code` (`NonuniqueCode`), enforces the single-target-database invariant (`WrongDatabase`), and auto-promotes to a `MultifunctionalDatabase` when any node is `multifunctional`
- migrations are field-remap transforms: `migrate(name)` applies a registered `bw2io` migration; `randonneur(...)` applies the richer `randonneur`/`randonneur_data` verb set (`replace`/`update`/`disaggregate`, plus `create`/`delete` when explicitly enabled) to edges and/or nodes

[INTEGRATION]:
- bw2data/bw2calc seam (the spine): `bw2io` writes the `Database` and the LCIA methods into the active `bw2data` project; `bw2calc` (the LCA engine) and `bw_processing` (the matrix datapackage) consume that written database — the EPD/LCA owner runs `bw2io` ingestion once, then computes via `bw2calc`, never re-extracting at calculation time
- premise seam (consumer): `premise` calls `bw2io` strategies to import its `additional_inventories` and normalize units before applying the prospective sector transforms; `bw2io` is the extra-LCI front door for premise's prospective builds, whose output `bw2data` databases `bw2calc` then scores (install `premise[bw25]` to bind this `bw2io 0.9.x` / `bw2data 4.x` / `bw2calc 2.5` line) — see `premise.md`
- openepd/epdx seam: `openepd` (OpenEPD payloads) and `epdx` (parsed ILCD+EPD impacts) supply EPD material-impact rows; map them through a custom strategy + `ExcelImporter`/`CSVImporter` (or a synthesized `self.data`) into a Brightway database so EPD-declared impacts join the computed inventory
- olca-ipc seam (cross-tool): openLCA exports JSON-LD; the JSON-LD importer ingests it into Brightway, and `olca-ipc` consumes the same model graph live — `bw2io` is the Brightway-side bridge of the two-engine LCA interchange
- tabular/contract seam: `self.data` (nodes + exchanges) and the unlinked-edge set flatten into a `pandas`/`polars` frame for the data contract gate (`pandera`/`dataframely`) and the `statistics()` tuple becomes a `QualityProfile` row in the data profile rail — linking quality is a first-class receipt, not a print
- graph seam: `DatabaseToGEXF`/`keyword_to_gephi_graph` hand the technosphere graph to the data graph owner (`rustworkx`/`networkx`) for topology analysis
- stamina/observability seam: the remote one-shot imports (`import_ecoinvent_release`, `exiobase_monetary`, `useeio20`) hit network/Zenodo/credentialed endpoints — wrap them in a `stamina` retry; wrap `apply_strategies` (a multi-pass transform) in a `structlog`/`opentelemetry` span keyed by `db_name` so the linking pipeline is observable
- ContentIdentity seam: key the imported database (and the `.bw2package` export) by the runtime `ContentIdentity` over the source file + strategy set so re-ingestion is deduped against the persistence reuse ledger

[EXCEPTIONS]:
- `bw2io.errors.StrategyError` — a strategy/match references an absent external database or an invalid linking configuration
- `bw2io.errors.NonuniqueCode` — two nodes resolve to the same `code` at `write_database`
- `bw2io.errors.WrongDatabase` — a node's `database` field disagrees with the write target
- `import_ecoinvent_release` degrades to a warning when `ecoinvent_interface` is absent; `SimaProBlockCSVImporter` is present only with `bw_simapro_csv`; `drop_unlinked` raises unless `i_am_reckless=True`

[RAIL_LAW]:
- Package: `bw2io`
- Owns: extraction of external LCI/LCIA formats, the strategy-pipeline linking model, `bw2data.Database`/method writing, the biosphere/LCIA/migration bootstrap, `randonneur` migrations, GEXF/matrix export, and `BW2Package` interchange
- Accept: the `extract → apply_strategies → statistics → match → write_database` pipeline; custom linking as a `list[dict] -> list[dict]` strategy passed to `apply_strategy`; `statistics()`/`all_linked` as the linking receipt; `randonneur`/`migrate` for field remaps; the one-shot `import_*`/`useeio20`/`exiobase_monetary` system imports under a retry; `.bw2package` for portable interchange
- Reject: hand-rolled ecospold/SimaPro/Excel parsing when an importer owns the format; hand-rolled exchange linking when a `bw2io.strategies` function or `match_database` covers it; re-implementing the matrix build or LCA solve (that is `bw_processing`/`bw2calc`); treating `statistics()` output as a print instead of a persisted quality receipt
