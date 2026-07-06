# [PY_GEOMETRY_API_PDAL]

`pdal` supplies a pipeline-based point-cloud processing engine over the native `libpdal` C++ core: a `Pipeline` (the executable stage graph), a `Stage` family (`Reader`/`Filter`/`Writer`) whose concrete drivers are injected as named staticmethods from the runtime driver registry, a `PipelineIterator` for chunked streaming, and module-level `dimensions`/`info` introspection. The scan-processing owner composes typed driver factories (`Reader.las`, `Filter.range`, `Writer.gltf`, ...) under the `|` operator into one `Pipeline`, executes it, and reads output as structured `numpy` arrays, a `meshio.Mesh`, or a `pandas`/`geopandas` frame — it never hand-rolls a point-cloud format parser or calls the libpdal C++ API directly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdal`
- package: `pdal`
- import: `import pdal` then `pdal.Reader`/`pdal.Filter`/`pdal.Writer`/`pdal.Pipeline`; `import` time runs `inject_pdal_drivers()` and binds module-level `dimensions`/`info`
- owner: `geometry`
- rail: scan-processing
- installed: `3.5.3`
- entry points: none (library only)
- capability: PDAL JSON or stage-sequence pipeline construction, driver inference from filename, execute / streaming-execute / chunked-iterate, in-memory `numpy` structured-array inputs and outputs, mesh output to `meshio`, frame output to `pandas`/`geopandas`, pickle round-trip via the pipeline JSON, and full driver/dimension/option introspection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and stage family
- rail: scan-processing

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                       |
| :-----: | :------------------------ | :----------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Pipeline`                | execution graph    | `libpdalpython.Pipeline` subclass: build/execute/stream/iterate; picklable via pipeline JSON |
|  [02]   | `Stage`                   | base stage         | `type`/`tag`/`inputs`/`options`; `__or__` and `.pipeline(*arrays)` composition      |
|  [03]   | `InferableTypeStage`      | filename-typed     | base for `Reader`/`Writer`; `type` falls back to driver inferred from `filename`    |
|  [04]   | `Reader`                  | source stage       | `_infer_type = libpdalpython.infer_reader_driver`; driver from path or explicit `type` |
|  [05]   | `Filter`                  | transform stage    | requires explicit `type: str` positional                                            |
|  [06]   | `Writer`                  | sink stage         | `_infer_type = libpdalpython.infer_writer_driver`; driver from path or explicit `type` |
|  [07]   | `PipelineIterator`        | streaming iterator | `libpdalpython` chunk iterator with per-chunk `log`/`schema`/`metadata`             |
|  [08]   | `drivers.Driver` / `drivers.Option` | registry record | dataclass describing one PDAL driver (name/type/description) and one option (name/default/description) |

[PUBLIC_TYPE_SCOPE]: module-level symbols
- rail: scan-processing

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                            |
| :-----: | :------------------------ | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `pdal.dimensions`         | dimension list  | `libpdalpython.getDimensions()`: every registered point dimension record (name/dtype/size) |
|  [02]   | `pdal.info`               | version info    | `libpdalpython.getInfo()`: version, GDAL/GEOS/sqlite versions, plugin path |
|  [03]   | `drivers.StreamableTypes` | `frozenset[str]`| stage type strings whose driver advertises streaming; `Stage.streamable` reads it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: injected driver factories (the primary construction surface)
- rail: scan-processing

`inject_pdal_drivers()` runs at import: for each `libpdalpython.getDrivers()` row it binds a `staticmethod` named by the driver short-name onto `Reader`/`Filter`/`Writer`, pre-filling `type=<driver>` and threading `filename` (first option, when present) plus option kwargs. Construct stages through these, not raw `Filter(type=...)`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `Reader.las(filename, **opts)` / `.e57` / `.copc` / `.ept` / `.ply` / ...  | construction | typed reader factory; `type='readers.<name>'` pre-filled, `filename` first positional |
|  [02]   | `Filter.range(**opts)` / `.crop` / `.outlier` / `.smrf` / `.poisson` / `.icp` / `.reprojection` / ... | construction | typed filter factory; `type='filters.<name>'` pre-filled |
|  [03]   | `Writer.las(filename, **opts)` / `.gltf` / `.ply` / `.copc` / `.gdal` / ... | construction | typed writer factory; `type='writers.<name>'` pre-filled |
|  [04]   | `<driver>.__doc__`                                         | introspection  | each factory's docstring is the driver description plus the `repr` of every `Option` (name=default: description) |

[ENTRYPOINT_SCOPE]: stage construction and composition
- rail: scan-processing

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `Reader(filename=None, **options) -> Reader`                  | construction   | source stage; driver inferred from path when no `type` |
|  [02]   | `Filter(type: str, **options) -> Filter`                      | construction   | transform stage; explicit `type` required (positional) |
|  [03]   | `Writer(filename=None, **options) -> Writer`                  | construction   | sink stage; driver inferred from path when no `type`   |
|  [04]   | `stage \| other -> Pipeline`                                  | composition    | `Stage.__or__` builds a fresh `Pipeline((self, other))` |
|  [05]   | `stage.pipeline(*arrays, loglevel=logging.ERROR) -> Pipeline` | construction   | wrap one stage in a `Pipeline` with input arrays       |
|  [06]   | `stage.type / .tag / .inputs / .options`                      | query          | PDAL type string; tag for graph referencing; upstream `Stage\|str` list; option dict copy |
|  [07]   | `stage.streamable -> bool`                                    | query          | `stage.type in drivers.StreamableTypes`                |

[ENTRYPOINT_SCOPE]: Pipeline construction and execution
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `Pipeline(spec=None, arrays=(), loglevel=logging.ERROR, json=None, dataframes=(), stream_handlers=()) -> Pipeline` | construction   | from PDAL JSON string, stage sequence, or `pandas`/`geopandas` frames (geometry column dropped/expanded to `numpy` records) |
|  [02]   | `p.execute(allowed_dims=[]) -> int`                                                                                | execution      | run pipeline; returns output point count      |
|  [03]   | `p.execute_streaming(chunk_size=10000, allowed_dims=[]) -> int`                                                    | execution      | streaming execution; requires `p.streamable`  |
|  [04]   | `p.iterator(chunk_size=10000, prefetch=0, allowed_dims=[]) -> PipelineIterator`                                    | execution      | lazy chunk iterator; requires `p.streamable`  |
|  [05]   | `p \| stage -> Pipeline` / `p \|= stage`                                                                          | composition    | `__or__` copies then appends; `__ior__` mutates in place (rejects input-bearing pipeline after another) |
|  [06]   | `p.toJSON() -> str` / `p.pipeline -> str`                                                                          | serialization  | normalized PDAL JSON (auto-appends `filters.merge` when all stages are readers) |
|  [07]   | `pickle.dumps(p)` / `copy.copy(p)`                                                                                 | serialization  | `__getstate__`/`__setstate__` round-trip the pipeline JSON; `__copy__` clones inputs+stages |

[ENTRYPOINT_SCOPE]: Pipeline output and config
- rail: scan-processing

Output members are `libpdalpython.Pipeline` C-extension attributes, undefined before `execute()`.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :----------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `p.arrays -> list[np.ndarray]`                         | output         | one structured `numpy` array per output view; dimension names are the field names |
|  [02]   | `p.meshes -> list[np.ndarray]`                         | output         | per-view mesh face records (`A`/`B`/`C` vertex indices) |
|  [03]   | `p.get_meshio(idx) -> meshio.Mesh \| None`             | output         | fold array XYZ + mesh A/B/C into a `meshio.Mesh` with one `triangle` `CellBlock`; `None` when empty |
|  [04]   | `p.get_dataframe(idx) -> pandas.DataFrame \| None`     | output         | structured array as a `pandas` frame                |
|  [05]   | `p.get_geodataframe(idx, xyz=False, crs=None) -> geopandas.GeoDataFrame \| None` | output | frame with `points_from_xy` geometry (XY or XYZ) and optional CRS |
|  [06]   | `p.metadata -> dict` / `p.schema -> dict` / `p.quickinfo -> dict` | output | parsed JSON (`json.loads` in the binding): last-run per-stage metadata / dimension census `{"schema": {"dimensions": [...]}}` / pre-execute quick info |
|  [07]   | `p.srswkt2 -> str` / `p.log -> str`                    | output         | SRS WKT2 text / execution log text                  |
|  [08]   | `p.stages -> list[Stage]` / `p.streamable -> bool`     | query          | the Python `Stage` list / whether every stage streams |
|  [09]   | `p.loglevel -> int` (get/set)                          | config         | maps Python `logging` levels to PDAL log levels      |
|  [10]   | `p.inputs = [(array, handler), ...]`                   | config         | `numpy` structured arrays as in-memory point sources, each optionally paired with a stream-handler callable |

[ENTRYPOINT_SCOPE]: PipelineIterator and module introspection
- rail: scan-processing

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :----------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `iter(p.iterator(...))` / `it.__next__() -> np.ndarray` | streaming | next chunk as a structured `numpy` array          |
|  [02]   | `it.log -> str` / `it.schema -> dict` / `it.metadata -> dict` | output | log text / parsed schema / parsed metadata for completed chunks |
|  [03]   | `libpdalpython.infer_reader_driver(path)` / `infer_writer_driver(path)` | query | driver string inferred from extension (`Reader`/`Writer` use these) |
|  [04]   | `libpdalpython.getDrivers()` / `getOptions()` / `getDimensions()` / `getInfo()` | query | raw registry rows the driver injection and module-level `dimensions`/`info` consume |

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_TOPOLOGY]:
- import: `import pdal` at boundary scope only; module-level import is banned by the manifest import policy. The import side-effect (`inject_pdal_drivers`) is what binds the typed driver factories, so the boundary import must complete before any `Reader.las`/`Filter.range` reference.
- construction axis: build the stage graph through the injected driver factories under `|` — `Reader.las(path) | Filter.range(limits="Z[0:100]") | Writer.las(out)` — never as parallel format-specific pipeline functions. `Filter` is the only stage requiring an explicit `type`; readers/writers infer it from the filename. `Pipeline(spec=...)` also accepts a raw PDAL JSON string or a `Stage` sequence, and `dataframes=`/`stream_handlers=` supply `pandas`/`geopandas` frames or streaming-source callables.
- execution axis: `execute()` is the polymorphic run; `execute_streaming()`/`iterator()` are the streaming arms gated on `p.streamable` (a fold over `drivers.StreamableTypes`). Output members (`arrays`/`meshes`/`metadata`/`schema`/`log`) are undefined before execute and are read once into the receipt.
- output axis: `arrays` is the canonical structured-`numpy` egress; `get_meshio` is the direct `meshio.Mesh` seam (XYZ points + `triangle` CellBlock from mesh A/B/C, e.g. after `filters.poisson`/`filters.delaunay`), `get_dataframe`/`get_geodataframe` the tabular seams. A null `Mesh`/frame surfaces as `None`, not an exception.
- offload axis: `Pipeline` pickles through its JSON state (`__getstate__`/`__setstate__`), so a multi-second pipeline solve hands to the runtime offload lane by JSON, surviving even the `to_process` fallback the in-memory companion kernels cannot.
- evidence: each run captures output point count, the schema dimension names, the SRS WKT2, and the stage tags as a scan-pipeline receipt; the metadata JSON carries per-stage diagnostics.
- boundary: pdal owns LAS/LAZ/E57/COPC/EPT/PLY/text pipeline ingestion and the broad filter catalog (crop/range/outlier/SMRF/PMF/reprojection/poisson/ICP). The conditioned `numpy` block or `meshio.Mesh` hands to `open3d`/`small-gicp` registration, `trimesh`/`meshio` mesh exchange, and `laspy` for LAS-specific extra-dimension work; pdal never calls the libpdal C++ API directly from domain code.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pdal`
- Owns: PDAL stage-graph construction via injected driver factories, pipeline execution (batch/streaming/iterator), `numpy`/`meshio`/`pandas`/`geopandas` IO seams, driver/dimension/option introspection
- Accept: PDAL JSON strings, filename-typed `Reader`/`Writer` stages, explicit-type `Filter` stages, `numpy` structured arrays and `pandas`/`geopandas` frames as inputs
- Reject: hand-rolled point-cloud format parsing; direct libpdal C++ API calls from Python; a per-format pipeline function family where the injected driver factory plus `|` composes it; raw `Filter(type=...)` where an injected `Filter.<name>` factory exists; installation without native `libpdal`

[CAPTURE_GAP]:
- members: introspected against the cached sdist source (`src/pdal/__init__.py`, `pipeline.py`, `drivers.py`); every documented type, entrypoint, property, and the `inject_pdal_drivers` factory mechanism resolves — no phantom. The concrete injected driver short-names (`Reader.las`, `Filter.range`, ...) are registry-runtime values from `libpdalpython.getDrivers()`, enumerated here as representative of the live driver set, confirmed against a `pdal.info`/`getDrivers()` call on the installed companion distribution.
