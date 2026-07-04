# [PY_DATA_API_PREMISE]

`premise` (PRospective EnvironMental Impact ASsEment) couples integrated-assessment-model (IAM) output to an ecoinvent LCI database, transforming a present-day ecoinvent database into a set of prospective databases consistent with a given climate/energy scenario and future year. It reads a source ecoinvent database from a Brightway project (or ecospold files), applies sector transformations (electricity mix, cement, steel, fuels, transport, heat, metals, …) toward the IAM pathway, and writes the resulting scenario database(s) back to Brightway, SimaPro, openLCA, sparse matrices, or a shareable datapackage. It is the PROSPECTIVE-SCENARIO leg of the data EPD/LCA owner — it produces forward-looking background databases that the Brightway solver then scores; it computes no LCIA itself.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `premise`
- package: `premise`
- import: `from premise import NewDatabase, IncrementalDatabase, PathwaysDataPackage, clear_cache, clear_inventory_cache, get_regions_definition`
- version: `2.4.6`
- license: BSD-3-Clause
- module: pure Python; the per-sector transformers live in `premise.{electricity,cement,steel,fuels,transport,heat,metals,battery,biomass,emissions,...}` and are dispatched by `NewDatabase.update()`
- owner: `data`
- rail: epd-lca (prospective LCA background)
- asset: pure Python over the scientific stack; the LCI math is `numpy`/`scipy`/`sparse` matrices and `xarray` IAM data
- depends: `bw2data`, `bw2io` (Brightway store/IO), `wurst>=0.4` (ecoinvent-editing engine), `constructive-geometries>=1.0.0` (geographic linking), `ecoinvent_interface` (release download), `premise_gwp` (extra GWP methods), `unfold` (scenario-datapackage fold/unfold), `numpy<2.0.0`, `scipy<1.14.0`, `xarray<=2024.2.0`, `pandas<3.0.0`, `sparse>=0.14.0`, `pyarrow`, `datapackage`, `openpyxl`, `pycountry`, `schema`, `cryptography`, `platformdirs`, `prettytable`, `tqdm`, `requests`, `pyYaml`
- bw25-extra: install as `premise[bw25]` to pull the modern Brightway 2.5 stack (`bw2calc>=2.0.1`, `bw2data>=4.3`, `bw2io>=0.9.4`) — REQUIRED to align with the admitted cluster (`bw2data 4.7` / `bw2calc 2.5.0`); the default deps leave `bw2calc` unpinned at the legacy bw2 line
- evidence: not installed in the active interpreter; members source-verified against `2.4.6`
- data-license: premise TRANSFORMS a LICENSED ecoinvent database (not bundled) and its IAM scenario data files are ENCRYPTED — a consumer needs a valid ecoinvent license AND a premise decryption `key` (or a self-supplied `external_scenarios`/`additional_inventories`) before any transform runs
- capability: build prospective ecoinvent databases for one or many (IAM model × pathway × year) scenarios, apply per-sector transformations, and export to Brightway / SimaPro / openLCA / matrices / datapackage, plus incremental and time-series (`pathways`) variants
- scope-law: premise BUILDS prospective background databases. It is not an LCIA calculator (`bw2calc`), not an EPD parser (`openepd`/`epdx`), and not the source-of-record store (`bw2data` owns the project graph)

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the database builders (`premise.new_database`, `premise.incremental`, `premise.pathways`)
- rail: epd-lca

| [INDEX] | [SURFACE] | [ROLE] |
| :-----: | :-------- | :----- |
|  [01]   | `NewDatabase(scenarios, source_version='3.12', source_type='brightway', key=None, source_db=None, source_file_path=None, additional_inventories=None, system_model='cutoff', system_args=None, use_cached_inventories=True, use_cached_database=True, external_scenarios=None, quiet=False, keep_imports_uncertainty=True, keep_source_db_uncertainty=False, gains_scenario='CLE', use_absolute_efficiency=False, biosphere_name='biosphere3', generate_reports=True)` | the builder: `scenarios` is a list of `{"model": "remind", "pathway": "SSP2-Base", "year": 2050}` dicts; `source_type` is `brightway` (reads `source_db` from the current project) or `ecospold` (reads `source_file_path`); `system_model` is `cutoff` or `consequential`; `key` decrypts the IAM data |
|  [02]   | `NewDatabase.update(sectors=None)` | apply the prospective transformations — `None` runs every sector, or pass a subset (`"electricity"`, `"cement"`, `"steel"`, `"fuels"`, `"transport"`, `"heat"`, `"metals"`, …) to scope the transform |
|  [03]   | `NewDatabase.write_db_to_brightway(name=None)` | register the transformed database(s) into the open Brightway project; `name` is one string or a list matching the scenario count |
|  [04]   | `NewDatabase.write_superstructure_db_to_brightway(...)` | write ONE superstructure database carrying all scenarios as a difference (scenario-difference-file) matrix — the Activity Browser / multi-scenario form |
|  [05]   | `NewDatabase.write_datapackage(name, ...)` | emit a `bw_processing`/`datapackage` superstructure (an `unfold`-shareable scenario package) |
|  [06]   | `NewDatabase.write_db_to_simapro(filepath=None)` / `.write_db_to_olca(filepath=None)` / `.write_db_to_matrices(filepath=None)` | export to SimaPro CSV, openLCA (JSON-LD), or sparse `numpy`/`scipy` matrices |
|  [07]   | `NewDatabase.generate_scenario_report(...)` / `.generate_change_report()` | a per-scenario impact report and a transformation change-log (`prettytable`/Excel) |
|  [08]   | `IncrementalDatabase(NewDatabase)` | incremental builder — `.update(sectors=dict)` stacks sector transformations cumulatively; `.write_increment_db_to_brightway(...)` writes the per-increment databases for sensitivity over transformation steps |
|  [09]   | `PathwaysDataPackage(scenarios, years=range(2005,2105,5), source_version='3.12', source_type='brightway', key=None, ...)` | the time-series builder; `.create_datapackage(name='pathways_<date>', contributors=None, transformations=None)` produces a `pathways`-tool datapackage spanning the year grid |

[UTILITY_SCOPE]: cache + region helpers (`premise.utils`, `premise.scenario_downloader`)
- `clear_cache()` — purge the cached transformed databases; `clear_inventory_cache()` — purge only the imported-inventory cache (after upgrading additional inventories)
- `get_regions_definition(model)` — the geographic region definitions for an IAM model (REMIND/IMAGE/…)
- `premise.scenario_downloader.download_csv(file_name, url, download_folder)` — fetch an external scenario CSV

## [03]-[TRANSFORMATION_LAW]

[SCENARIO_SHAPE]:
- a scenario is `{"model": <iam>, "pathway": <ssp/rcp pathway>, "year": <int>}`; `model` ∈ REMIND / IMAGE / TIAM-UCL / GCAM / others premise ships mappings for. `external_scenarios` injects a user IAM datapackage; `additional_inventories` (list of `{"filepath": ..., "ecoinvent version": ...}`) imports extra LCI before transformation.
- `update()` dispatches per sector to the transformer modules (`premise.electricity` rewires the electricity market mixes to the IAM pathway, `premise.steel`/`premise.cement`/`premise.fuels`/`premise.transport`/`premise.heat`/`premise.metals` decarbonize their sectors); `wurst` does the underlying dataset search/copy/relink, `constructive_geometries` resolves the geographic linking.
- premise parallelizes internally (multiprocessing over scenarios/sectors) and shows `tqdm` progress; `quiet=True` silences it. Do NOT wrap `update()` in an outer process pool — it manages its own fan-out.

[CACHING_LAW]: `use_cached_inventories`/`use_cached_database` reuse the decrypted+imported intermediates across runs; bump them off (or `clear_inventory_cache()`) after changing `additional_inventories` or the ecoinvent version, or stale cached inventories silently carry into the new build.

## [04]-[INTEGRATION]

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `numpy` / `xarray` — premise's IAM scenario data is `xarray`; the LCI is `numpy`/`scipy`/`sparse` matrices. The `write_db_to_matrices` output is the same sparse-matrix substrate `bw2calc` consumes.
- `structlog` + `opentelemetry` — wrap `update()` and each `write_db_to_*` in a span and log `(model, pathway, year, sectors)`; the transform is long (minutes) so the span carries the scenario identity, not per-dataset events.
- `anyio` — `update()` is blocking and CPU-bound with internal multiprocessing; call it via `anyio.to_thread.run_sync` so an async owner stays responsive, but keep premise's own parallelism (do not also fan out across an `anyio` task group).
- `pyarrow` — the `datapackage`/superstructure export rides Arrow; route it through the data owner's Arrow rail for downstream tabular consumption.

[SIBLING_STACK]: stacking with the data EPD/LCA folder cluster (`libs/python/data/.api/`)
- `bw2data` — the SOURCE and TARGET: `source_type='brightway'` reads `source_db` from the current `bw2data` project; `write_db_to_brightway(name)` registers the prospective database(s) back. Set the project with `bd.projects.set_current(...)` and run `bw2io.bw2setup()` before building. Install `premise[bw25]` so this is the modern `bw2data 4.x` store, not the legacy bw2 line.
- `bw2calc` — the consumer: after `write_db_to_brightway`, score the prospective database with `bw2calc.LCA`/`MultiLCA` exactly as a present-day database; the superstructure form feeds a scenario-difference `MultiLCA`.
- `bw2io` — premise uses `bw2io` strategies to import `additional_inventories` and normalize units before transformation; the importer surface is `bw2io.md`.
- `bw-processing` — `write_datapackage`/`write_db_to_matrices` emit `bw_processing` datapackages/matrices, and the prospective-scenario overlay rides `bw_processing.merge_datapackages_with_mask` (splice future-scenario coefficients onto the baseline background datapackage); that catalog owns the on-disk superstructure format.
- `olca-ipc` — `write_db_to_olca` targets openLCA; the prospective database becomes openLCA processes/flows.
- `openepd` / `epdx` — the foreground/background seam: `openepd`/`epdx` carry the as-declared product EPD (foreground), premise supplies the future-year background LCI. The material-impact owner combines a current `Epd` with a premise-shifted background for a forward-looking product footprint.

[CONTENT_IDENTITY_SEAM]: key a built database by `(model, pathway, year, ecoinvent version, system_model)` in the persistence reuse ledger so an identical scenario build is deduped; the decrypted-inventory cache is premise-internal and orthogonal to that ledger.

[RAIL_LAW]:
- Package: `premise`
- Owns: prospective ecoinvent database construction from IAM scenarios — `NewDatabase` (+ `IncrementalDatabase`, `PathwaysDataPackage`), the per-sector transformations, and the Brightway/SimaPro/openLCA/matrix/datapackage exporters
- Accept: `NewDatabase(scenarios, source_type='brightway', source_db=..., key=..., system_model=...).update().write_db_to_brightway(...)` as the canonical build→score path; `write_superstructure_db_to_brightway`/`write_datapackage` for multi-scenario/shareable forms; `IncrementalDatabase` for step sensitivity; `PathwaysDataPackage` for the year grid; `clear_inventory_cache()` after changing inventories
- Reject: treating premise as an LCIA calculator (route scoring to `bw2calc`); building without a `bw2data` source project + `bw2setup()` biosphere; running without a valid ecoinvent license / decryption `key`; the legacy default Brightway pins (admit `premise[bw25]` to match the cluster); wrapping `update()` in an outer process pool (it self-parallelizes); reusing a stale inventory cache across an ecoinvent-version change
