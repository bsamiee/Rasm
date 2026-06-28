# [PY_DATA_API_BRIGHTWAY2]

`brightway2` is the facade/umbrella of the Brightway LCA cluster and a zero-code package: its entire `__init__` is `from bw2data import *; from bw2calc import *; from bw2io import *` plus `__version__ = (2, 3)`, collapsing the three split packages into one namespace. It defines NO symbol of its own — even the first-run bootstrap helpers (`bw2setup`, the default biosphere/LCIA builders, ecoinvent/EEIO import) are `bw2io` functions surfaced through the `from bw2io import *` star, not brightway2 code. The real surfaces live in `bw2data` (project + graph store), `bw2calc` (the solver), `bw_processing` (the matrix datapackage), and `bw2io` (import/export + bootstrap). Admit `brightway2` only as the one-import convenience; compose new code directly against the owning catalogs (`bw2data.md`, `bw2calc.md`, `bw-processing.md`, `bw2io.md`), which carry the real, version-current surface — the bootstrap helpers included, which `bw2io` owns. The umbrella's own declared dependency floors (`bw2calc>=1.7.1`, `bw2data>=3.4.1`) are ancient and the cluster resolves far past them, so it is never the source of truth for a member or a version.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `brightway2`
- package: `brightway2`
- import: `import brightway2 as bw` (re-export umbrella) — prefer `import bw2data as bd; import bw2calc as bc` in new code
- owner: `data`
- rail: lca-facade (EPD/LCA cluster)
- version: `2.3`
- license: `BSD-3-Clause` (`Classifier: License :: OSI Approved :: BSD License`; Copyright Chris Mutel and ETH Zurich)
- asset: pure Python, `py3-none-any` purelib (zero compiled extensions, ABI-agnostic); a zero-code re-export shell — its only module body is the three star-imports and `__version__`
- depends-on: `bw2data`, `bw2calc`, `bw2io`, `bw2analyzer (>=0.9.4)`, `bw2parameters`, plus legacy `appdirs`, `asteval`, `docopt`, `eight`, `flask`, `future`, `lxml`, `numpy`, `peewee (>=3.0)`. NOTE: the cluster resolves to `bw2data 4.7` / `bw2calc 2.5.0` / `bw2io 0.9.x` / `bw-processing 1.5` regardless of the umbrella's stale floor pins.
- re-export scope: `from bw2data import *`, `from bw2calc import *`, `from bw2io import *` — `bw2analyzer` is a declared DEPENDENCY but is NOT re-exported into the `brightway2` namespace; reach contribution analysis via `import bw2analyzer` directly.
- marker: none — admitted unpinned; installed and `assay`-reflectable on the active interpreter
- entry points: library-only; no console script
- capability: one-import access to the merged `bw2data` + `bw2calc` + `bw2io` namespace, plus project bootstrap — create the default biosphere database and LCIA methods, run the core migrations, import an ecoinvent release or the US EEIO model, and back up / restore / install projects

## [02]-[CAPTURE]

[REEXPORT_MAP]: where each merged symbol is really owned
- from `bw2data` (-> `bw2data.md`): `projects`, `Database`, `Node`, `Edge`, `Method`, `Weighting`, `Normalization`, `get_node`, `get_activity`, `get_id`, `databases`, `methods`, `geomapping`, `mapping`, `parameters`, `config`, `preferences`, `labels`, `calculation_setups`, `dynamic_calculation_setups`, `prepare_lca_inputs`, `get_multilca_data_objs`, `DataStore`, `ProcessedDataStore`, `Searcher`, `convert_backend`, `set_data_dir`, `extract_brightway_databases`.
- from `bw2calc` (-> `bw2calc.md`): `LCA`, `MultiLCA`, `DenseLCA`, `IterativeLCA`, `LeastSquaresLCA`, `CachingLCA`, `JacobiGMRESLCA`, `PartitionedMonteCarloLCA`, `FastScoresOnlyMultiLCA`, `MethodConfig`.
- from `bw2io` (-> `bw2io.md`): `ExcelImporter`, `ExcelLCIAImporter`, `CSVImporter`, `CSVLCIAImporter`, `SimaProCSVImporter`, `SimaProLCIACSVImporter`, `SingleOutputEcospold1Importer`, `SingleOutputEcospold2Importer`, `MultiOutputEcospold1Importer`, `Ecospold1LCIAImporter`, `Exiobase3MonetaryImporter`, `ChemIDPlus`, `Migration`, `migrations`, `BW2Package`, `UnlinkedData`, `unlinked_data`, `activity_hash`, `es2_activity_hash`, `normalize_units`, `DatabaseToGEXF`, `lci_matrices_to_excel`.

[BOOTSTRAP]: the first-run helpers the namespace surfaces — all owned by `bw2io` (`bw2io.md`) and re-exported here through `from bw2io import *`, NOT brightway2 code; each mutates the current project
- `bw2setup()` — the one-call first-run: create `biosphere3`, the default LCIA methods, and the core migrations in the current project (idempotent — no-ops when `biosphere3` is already present).
- `create_default_biosphere3(overwrite=False)`, `create_default_lcia_methods(overwrite=False, rationalize_method_names=False, shortcut=True)`, `create_core_migrations()` — the individual steps `bw2setup` composes.
- `import_ecoinvent_release(...)` — import a licensed ecoinvent release as the background database; degrades to a warn-only stub unless the optional `ecoinvent_interface` package is installed. `useeio20(name='USEEIO-2.0', collapse_products=False, prune=False)` — install the US EEIO 2.0 input-output model; `add_example_database()` — a small teaching database.
- the biosphere-flow migration family `add_ecoinvent_33_biosphere_flows(...)` ... `add_ecoinvent_39_biosphere_flows(...)`, plus `exiobase_monetary(version=(3, 8, 1), year=2017, products=False, ...)`.
- project IO: `backup_project_directory(project)`, `restore_project_directory(filepath)`, `backup_data_directory()`, `install_project(...)`.
- example data: `get_csv_example_filepath()`, `get_xlsx_example_filepath()`.

[IMPLEMENTATION_LAW]:
- `brightway2` is `import *` over three packages; name collisions resolve to the last import (`bw2io`). It defines NO new types — every merged symbol is owned by `bw2data`/`bw2calc`/`bw2io`, and the version-current signature lives in those catalogs, never here.
- Prefer explicit imports (`import bw2data as bd; import bw2calc as bc; import bw2io as bi`) in new code: they avoid the `import *` namespace pollution, make the owner of each symbol obvious, and reach the same surfaces. The bootstrap helpers admitted "through brightway2" are `bw2io`'s — `import bw2io; bw2io.bw2setup()` is the equivalent with no umbrella — so `brightway2` earns admission only as the single-line convenience namespace, never as a capability owner.
- Every bootstrap helper mutates the CURRENT project — call `bd.projects.set_current(name)` first, then `bw2setup()`. The helpers are idempotent registries: re-running `bw2setup` on a set-up project is a no-op-ish refresh, not a duplicate.
- Contribution analysis (`bw2analyzer.ContributionAnalysis`, tagged-database traversal) is a dependency but NOT in this namespace; import `bw2analyzer` directly for it.

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- the facade adds nothing over its members; for the real substrate stacking (`pydantic`/`structlog`/`anyio`/`fsspec`/`numpy`/`scipy`/`msgspec`) see `bw2data.md`, `bw2calc.md`, and `bw-processing.md`.

[SIBLING_STACK]: stacking with the EPD/LCA folder cluster
- `bw2data` + `bw2calc` + `bw-processing`: the owned surfaces this facade merges; route every non-bootstrap call to those catalogs.
- `bw2io`: the import layer the facade re-exports; `bw2io.md` owns the importer surface (`ExcelImporter`, `SimaProCSVImporter`, ecospold/ecoinvent importers, `Migration`).
- `premise`, `openepd`, `epdx`, `olca-ipc`: the prospective-scenario, EPD-declaration, and remote-engine siblings — they integrate against `bw2data`/`bw2calc` directly, not through the umbrella.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `brightway2`
- Owns: ONLY the one-import convenience namespace merging `bw2data` + `bw2calc` + `bw2io` (zero own code); the first-run bootstrap helpers it surfaces (`bw2setup`, default biosphere/LCIA builders, ecoinvent/EEIO import, project backup/restore) are `bw2io`-owned re-exports, not brightway2 capability
- Accept: `bw2setup()` + `create_default_*` for first-run project setup; `import_ecoinvent_release` / `useeio20` / `add_example_database` for background-database ingestion; `backup_project_directory`/`restore_project_directory` for project IO — everything else routes to the owning catalog
- Reject: building new domain logic against the facade instead of `bw2data`/`bw2calc`/`bw2io` directly; relying on `import *` for member discovery (the owners carry the real signatures); treating `brightway2`'s stale dep floors as the cluster version (the cluster is `bw2data 4.7` / `bw2calc 2.5.0` / `bw-processing 1.5`); reaching for `bw2analyzer` symbols through this namespace (import `bw2analyzer` directly)
