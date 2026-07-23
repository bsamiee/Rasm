# [PY_GEOMETRY_API_PDAL]

`pdal` mints a pipeline-based point-cloud processing engine over the native `libpdal` C++ core: a `Pipeline` stage graph composed from injected `Reader`/`Filter`/`Writer` driver factories under `|`, run batch or streamed, its output read as structured `numpy` arrays, a `meshio.Mesh`, or a `pandas`/`geopandas` frame. Every LAS/LAZ/E57/COPC/EPT ingestion and the filter catalog route through this one scan-processing surface, never a hand-rolled format parser or a direct `libpdal` C++ call.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdal`
- package: `pdal`
- module: `pdal`; import runs `inject_pdal_drivers()`, binding the `Reader`/`Filter`/`Writer` driver factories and the module-level `dimensions`/`info`
- rail: scan-processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and stage family

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]      | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------- | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Pipeline`                          | execution graph    | `libpdalpython.Pipeline` subclass; build/stream/iterate, picklable           |
|  [02]   | `Stage`                             | base stage         | `type`/`tag`/`inputs`/`options`; `__or__`/`.pipeline()` composition          |
|  [03]   | `InferableTypeStage`                | filename-typed     | base for `Reader`/`Writer`; `type` inferred from `filename`                  |
|  [04]   | `Reader`                            | source stage       | driver from path (`infer_reader_driver`) or explicit `type`                  |
|  [05]   | `Filter`                            | transform stage    | requires explicit `type: str` positional                                     |
|  [06]   | `Writer`                            | sink stage         | driver from path (`infer_writer_driver`) or explicit `type`                  |
|  [07]   | `PipelineIterator`                  | streaming iterator | `libpdalpython` chunk iterator with per-chunk `log`/`schema`/`metadata`      |
|  [08]   | `drivers.Driver` / `drivers.Option` | registry record    | `Driver` (name/short_name/type/options), `Option` (name/default/description) |

[PUBLIC_TYPE_SCOPE]: module-level symbols

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                                                      |
| :-----: | :------------------------ | :--------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `pdal.dimensions`         | dimension list   | `getDimensions()`: every point dimension record (name/dtype/size)                 |
|  [02]   | `pdal.info`               | version info     | `getInfo()` namespace: PDAL version, `major`/`minor`/`patch`, `sha1`, plugin path |
|  [03]   | `drivers.StreamableTypes` | `frozenset[str]` | stage type strings whose driver advertises streaming; `Stage.streamable` reads it |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: injected driver factories (the primary construction surface)

Each factory pre-fills `type='<family>.<name>'` and threads `filename` (first option, readers/writers only) and option kwargs; a reader/writer `filename` admits a plain path str or a PDAL FileSpec dict `{"path": str, ...}` (streamable remote/authenticated sources), and every `<driver>.__doc__` carries the driver description and each `Option` repr (`name=default: description`).

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Reader.las(filename, **opts)` / `.e57` / `.copc` / `.ept` / ...  | static   | typed reader factory                           |
|  [02]   | `Filter.range(**opts)` / `.crop` / `.outlier` / `.smrf` / ...     | static   | typed filter factory                           |
|  [03]   | `Writer.las(filename, **opts)` / `.gltf` / `.ply` / `.copc` / ... | static   | typed writer factory                           |
|  [04]   | `<driver>.__doc__`                                                | property | docstring: driver description + `Option` reprs |

[ENTRYPOINT_SCOPE]: stage construction and composition

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------------------------------------ |
|  [01]   | `Reader(filename=None, **options) -> Reader`                  | ctor     | source stage; driver inferred from path when no `type`  |
|  [02]   | `Filter(type: str, **options) -> Filter`                      | ctor     | transform stage; explicit `type` required (positional)  |
|  [03]   | `Writer(filename=None, **options) -> Writer`                  | ctor     | sink stage; driver inferred from path when no `type`    |
|  [04]   | `stage \| other -> Pipeline`                                  | operator | `Stage.__or__` builds a fresh `Pipeline((self, other))` |
|  [05]   | `stage.pipeline(*arrays, loglevel=logging.ERROR) -> Pipeline` | instance | wrap one stage in a `Pipeline` with input arrays        |
|  [06]   | `stage.type`                                                  | property | PDAL driver type string                                 |
|  [07]   | `stage.tag`                                                   | property | tag for graph referencing                               |
|  [08]   | `stage.inputs`                                                | property | upstream `Stage \| str` list                            |
|  [09]   | `stage.options`                                               | property | option dict copy                                        |
|  [10]   | `stage.streamable -> bool`                                    | property | `stage.type in drivers.StreamableTypes`                 |

- `Reader(filename=...)` / `Writer(filename=...)`: `filename` accepts a path str or a FileSpec dict `{"path": str, ...}`; a str auto-wraps to `{"path": str}`, a dict without `"path"` raises `ValueError` (FileSpec passthrough needs the native `libpdal >= 2.9`, else the dict degrades to its `path` only).

[ENTRYPOINT_SCOPE]: Pipeline construction and execution

`Pipeline(spec=None, arrays=(), loglevel=logging.ERROR, json=None, dataframes=(), stream_handlers=())` builds from a PDAL JSON string, a `Stage` sequence, or `pandas`/`geopandas` frames (a `geometry` column drops and expands to `numpy` records); `__ior__` refuses to append a Pipeline after one already carrying inputs.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `Pipeline(...) -> Pipeline`                                    | ctor     | from JSON, `Stage` sequence, or frames (see lead)     |
|  [02]   | `p.execute() -> int`                                           | instance | run pipeline; returns output point count              |
|  [03]   | `p.execute_streaming(chunk_size=10000) -> int`                 | instance | streaming execution; requires `p.streamable`          |
|  [04]   | `p.iterator(chunk_size=10000, prefetch=0) -> PipelineIterator` | instance | lazy chunk iterator; requires `p.streamable`          |
|  [05]   | `p \| stage -> Pipeline` / `p \|= stage`                       | operator | `__or__` copies+appends; `__ior__` appends in place   |
|  [06]   | `p.toJSON() -> str` / `p.pipeline -> str`                      | instance | normalized JSON; appends `filters.merge` (all-reader) |
|  [07]   | `pickle.dumps(p)`                                              | instance | `__getstate__`/`__setstate__` round-trip the JSON     |
|  [08]   | `copy.copy(p)`                                                 | instance | `__copy__` clones inputs + stages                     |

[ENTRYPOINT_SCOPE]: Pipeline output and config

Output members are `libpdalpython.Pipeline` attributes undefined before `execute()`; each `get_*(idx)` returns `None` for an empty view, and metadata/schema/quickinfo are parsed JSON.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `p.arrays -> list[np.ndarray]`                     | property | one `numpy` structured array per view (fields = dimensions) |
|  [02]   | `p.meshes -> list[np.ndarray]`                     | property | per-view mesh face records (`A`/`B`/`C` vertex indices)     |
|  [03]   | `p.get_meshio(idx)`                                | instance | array XYZ + mesh A/B/C → `meshio.Mesh`                      |
|  [04]   | `p.get_dataframe(idx)`                             | instance | structured array as a `pandas` frame                        |
|  [05]   | `p.get_geodataframe(idx, xyz=False, crs=None)`     | instance | `points_from_xy` geometry (XY or XYZ), optional CRS         |
|  [06]   | `p.metadata -> dict`                               | property | last-run per-stage metadata                                 |
|  [07]   | `p.schema -> dict`                                 | property | dimension census `{"schema": {"dimensions": [...]}}`        |
|  [08]   | `p.quickinfo -> dict`                              | property | pre-execute quick info                                      |
|  [09]   | `p.srswkt2 -> str` / `p.log -> str`                | property | SRS WKT2 text / execution log text                          |
|  [10]   | `p.stages -> list[Stage]` / `p.streamable -> bool` | property | the Python `Stage` list / whether every stage streams       |
|  [11]   | `p.loglevel -> int` (get/set)                      | property | maps Python `logging` levels to PDAL log levels             |
|  [12]   | `p.inputs = [(array, handler), ...]`               | property | `numpy` point sources, each with an optional stream-handler |

[ENTRYPOINT_SCOPE]: PipelineIterator and module introspection

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `iter(p.iterator(...))` / `it.__next__() -> np.ndarray`           | operator | next chunk as a structured `numpy` array           |
|  [02]   | `it.log -> str` / `it.schema -> dict` / `it.metadata -> dict`     | property | log, parsed schema/metadata for completed chunks   |
|  [03]   | `infer_reader_driver(path)` / `infer_writer_driver(path)`         | static   | driver inferred from extension (`Reader`/`Writer`) |
|  [04]   | `getDrivers()` / `getOptions()` / `getDimensions()` / `getInfo()` | static   | registry rows behind injection/`dimensions`/`info` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import pdal` at boundary scope only; the `inject_pdal_drivers` side-effect binds the typed factories, so the import completes before any `Reader.las`/`Filter.range` reference.
- construction: build the stage graph through the injected factories under `|` (`Reader.las(path) | Filter.range(limits="Z[0:100]") | Writer.las(out)`); `Filter` alone needs an explicit `type`, readers and writers infer it from `filename` — a path str or a FileSpec dict `{"path": str, ...}` for remote/authenticated sources — and `Pipeline(spec=...)` also admits a raw PDAL JSON string or a `Stage` sequence.
- execution: `execute()` runs batch; `execute_streaming()`/`iterator()` are the streaming arms gated on `p.streamable`, a fold over `drivers.StreamableTypes`. Output members read once into the receipt.
- output: `arrays` is the canonical structured-`numpy` egress; `get_meshio` seams XYZ + `triangle` cells from mesh A/B/C (after `filters.poisson`/`filters.delaunay`) into a `meshio.Mesh`, and `get_dataframe`/`get_geodataframe` the tabular seams — a null `Mesh`/frame surfaces as `None`.
- offload: `Pipeline` pickles through its JSON state, so a multi-second solve hands to the runtime offload lane by JSON.
- evidence: each run captures output point count, schema dimension names, SRS WKT2, and stage tags as a scan-pipeline receipt; the metadata JSON carries per-stage diagnostics.

[STACKING]:
- `open3d`(`.api/open3d.md`): `p.arrays` XYZ feeds `geometry.PointCloud` (via `utility.Vector3dVector`) and `p.get_meshio` XYZ + A/B/C a `geometry.TriangleMesh` for `registration.registration_icp`.
- `small-gicp`(`.api/small-gicp.md`): the `p.arrays` `Nx3` block is the raw-array `align(...)` input or feeds `preprocess_points` for multi-threaded GICP/VGICP refinement.
- `trimesh`(`.api/trimesh.md`): `p.get_meshio` `meshio.Mesh` (or `p.meshes` A/B/C over `p.arrays` XYZ) rebuilds a `Trimesh` for CSG, repair, and export.
- geometry scan owner: folds the driver factories into one `Pipeline`, reads `p.arrays`/`get_meshio` into the scan-pipeline receipt, and hands conditioned clouds to the registration siblings.

[LOCAL_ADMISSION]:
- Construct stages through the injected `Reader`/`Filter`/`Writer.<driver>` factories under `|`; a raw `Filter(type=...)` where an injected factory exists, a per-format pipeline function family, hand-rolled point-cloud parsing, and a direct `libpdal` C++ call from Python are each rejected.

[RAIL_LAW]:
- Package: `pdal`
- Owns: PDAL stage-graph construction via injected factories, pipeline execution (batch/streaming/iterator), the `numpy`/`meshio`/`pandas`/`geopandas` IO seams, and driver/dimension/option introspection
- Accept: PDAL JSON strings, filename-typed `Reader`/`Writer` stages, explicit-type `Filter` stages, `numpy` structured arrays, and `pandas`/`geopandas` frames
- Reject: hand-rolled point-cloud parsing, a direct `libpdal` C++ call from Python, a per-format pipeline function family where a driver factory plus `|` composes it, and a raw `Filter(type=...)` shadowing an injected `Filter.<name>`
