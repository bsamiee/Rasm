# [PY_DATA_API_PREMISE]

`premise` transforms a present-day ecoinvent LCI database into prospective databases consistent with an integrated-assessment-model (IAM) climate/energy scenario and future year. It reads a source ecoinvent database (Brightway project or ecospold), applies per-sector transformations toward the IAM pathway, and writes the scenario database(s) back to Brightway, SimaPro, openLCA, sparse matrices, or a shareable datapackage. It builds the forward-looking background the Brightway solver scores; it computes no LCIA itself.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `premise`
- package: `premise` (BSD-3-Clause)
- module: `premise`
- namespaces: `premise.{electricity, cement, steel, fuels, transport, heat, metals, battery, biomass, emissions}` — per-sector transformer modules dispatched by `NewDatabase.update()`
- rail: epd-lca (prospective LCA background)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the prospective-database builders

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `NewDatabase`         | class         | build one or many (model × pathway × year) scenario databases    |
|  [02]   | `IncrementalDatabase` | class         | stack sector transforms step-by-step for step sensitivity        |
|  [03]   | `PathwaysDataPackage` | class         | time-series builder spanning a year grid for the `pathways` tool |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder methods (bare `.` owned by `NewDatabase`) and module utilities
- ctor carry: `source_type` ∈ `brightway` (reads `source_db`) | `ecospold` (reads `source_file_path`); `system_model` ∈ `cutoff` | `consequential`; `key` decrypts the IAM scenario data
- writer carry: `name` is one string or a per-scenario list

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `NewDatabase(scenarios, *, source_type, key, system_model)`  | ctor     | admit scenarios and a source database                      |
|  [02]   | `.update(sectors=None)`                                      | instance | apply prospective transforms; `None` runs all sectors      |
|  [03]   | `.write_db_to_brightway(name)`                               | instance | register transformed database(s) into the open project     |
|  [04]   | `.write_superstructure_db_to_brightway(name)`                | instance | all scenarios in one scenario-difference-matrix DB         |
|  [05]   | `.write_datapackage(name)`                                   | instance | emit a `bw_processing` superstructure (`unfold`-shareable) |
|  [06]   | `.write_db_to_simapro(name)`                                 | instance | export to SimaPro CSV                                      |
|  [07]   | `.write_db_to_olca(name)`                                    | instance | export to openLCA (JSON-LD)                                |
|  [08]   | `.write_db_to_matrices(name)`                                | instance | export to sparse `numpy`/`scipy` matrices                  |
|  [09]   | `.generate_scenario_report()`                                | instance | per-scenario impact report                                 |
|  [10]   | `.generate_change_report()`                                  | instance | transformation change-log (`prettytable`/Excel)            |
|  [11]   | `IncrementalDatabase.update(sectors)`                        | instance | stack sector transforms; `sectors` is a dict               |
|  [12]   | `IncrementalDatabase.write_increment_db_to_brightway(name)`  | instance | write per-increment databases                              |
|  [13]   | `PathwaysDataPackage(scenarios, *, years, source_type, key)` | ctor     | admit scenarios across the year grid                       |
|  [14]   | `PathwaysDataPackage.create_datapackage(name)`               | instance | produce a `pathways`-tool datapackage over the grid        |
|  [15]   | `clear_cache()`                                              | static   | purge the cached transformed databases                     |
|  [16]   | `clear_inventory_cache()`                                    | static   | purge only the imported-inventory cache                    |
|  [17]   | `get_regions_definition(model)`                              | static   | geographic region definitions for an IAM model             |
|  [18]   | `scenario_downloader.download_csv(file_name, url)`           | static   | fetch an external scenario CSV                             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a scenario is `{"model", "pathway", "year"}`; `model` ∈ REMIND / IMAGE / TIAM-UCL / GCAM; `external_scenarios` injects a user IAM datapackage, `additional_inventories` imports extra LCI before transformation
- `update()` dispatches per sector to the transformer modules; `wurst` performs the underlying dataset search/copy/relink, `constructive_geometries` resolves the geographic linking
- premise self-parallelizes over scenarios and sectors with `tqdm` progress (`quiet=True` silences it); wrapping `update()` in an outer process pool contends with its own fan-out
- `use_cached_inventories`/`use_cached_database` reuse decrypted+imported intermediates across runs; `clear_inventory_cache()` after changing `additional_inventories` or the ecoinvent version, or stale inventories carry into the new build

[STACKING]:
- `numpy` / `xarray`(`.api/numpy.md`, `.api/xarray.md`): scenario data is `xarray`, the LCI is `numpy`/`scipy`/`sparse` matrices; `write_db_to_matrices` emits the same sparse substrate `bw2calc` consumes
- `structlog` + `opentelemetry`(`.api/structlog.md`, `.api/opentelemetry-api.md`): wrap `update()` and each `write_db_to_*` in one span carrying `(model, pathway, year, sectors)` — scenario identity, not per-dataset events
- `anyio`(`.api/anyio.md`): run blocking `update()` via `anyio.to_thread.run_sync`, keeping premise's own parallelism and adding no outer task-group fan-out
- `bw2data`(`.api/bw2data.md`): SOURCE and TARGET — `source_type='brightway'` reads `source_db` from the current project, `write_db_to_brightway` registers the prospective database(s) back; set the project via `bd.projects.set_current`, run `bw2io.bw2setup()` first
- `bw2calc`(`.api/bw2calc.md`): the consumer — score the prospective database with `LCA`/`MultiLCA`; the superstructure form feeds a scenario-difference `MultiLCA`
- `bw2io`(`.api/bw2io.md`): imports `additional_inventories` and normalizes units before transformation through `bw2io` strategies
- `bw-processing`(`.api/bw-processing.md`): `write_datapackage`/`write_db_to_matrices` emit `bw_processing` datapackages, and the scenario overlay rides `merge_datapackages_with_mask` splicing future coefficients onto the baseline background
- `pyarrow`(`.api/pyarrow.md`): the datapackage/superstructure export rides Arrow — route it through the data owner's Arrow rail for downstream tabular consumption
- `olca-ipc`(`.api/olca-ipc.md`): `write_db_to_olca` targets openLCA processes and flows
- `openepd` / `epdx`(`.api/openepd.md`, `.api/epdx.md`): foreground/background seam — the material-impact owner combines a current `Epd` (as-declared foreground) with a premise-shifted background for a forward-looking product footprint
- persistence reuse ledger: key a built database by `(model, pathway, year, ecoinvent version, system_model)` to dedup an identical scenario build; the decrypted-inventory cache is premise-internal and orthogonal to that ledger

[LOCAL_ADMISSION]:
- admit premise under the `[bw25]` extra so its Brightway store binds the `bw2data 4.x` line the admitted cluster requires, never the bw2-line default the bare install selects

[RAIL_LAW]:
- Package: `premise`
- Owns: prospective ecoinvent database construction from IAM scenarios — the builders, the per-sector transformations, and the Brightway/SimaPro/openLCA/matrix/datapackage exporters
- Accept: `NewDatabase(source_type='brightway', source_db=..., key=..., system_model=...).update().write_db_to_brightway(...)` as the canonical build→score path; `write_superstructure_db_to_brightway`/`write_datapackage` for multi-scenario/shareable forms; `IncrementalDatabase` for step sensitivity; `PathwaysDataPackage` for the year grid
- Reject: scoring inside premise (route to `bw2calc`); building without a `bw2data` source project and `bw2setup()` biosphere; running without a valid ecoinvent license and decryption `key`; an outer process pool around `update()`; a stale inventory cache across an ecoinvent-version change
