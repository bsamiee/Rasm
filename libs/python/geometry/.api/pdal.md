# [PY_GEOMETRY_API_PDAL]

`pdal` supplies a pipeline-based point-cloud processing engine through `Pipeline`, `Stage`, `Reader`, `Filter`, `Writer`, and `PipelineIterator` for executing PDAL stage graphs, streaming large datasets, accessing output numpy arrays and mesh data, and querying available drivers and dimensions for the geometry scan-processing rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pdal`
- package: `pdal`
- module: `pdal`
- asset: compiled extension (libpdal wrapper) — requires native `libpdal` C++ library; pip install from sdist requires cmake + libpdal headers; conda-forge provides prebuilt wheel
- rail: scan-processing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: pipeline and stage family
- rail: scan-processing

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]      | [CAPABILITY]                                             |
| :-----: | :----------------- | :----------------- | :------------------------------------------------------- |
|   [1]   | `Pipeline`         | execution graph    | build, execute, stream, and query output of stage graph  |
|   [2]   | `Stage`            | base stage         | type, tag, inputs, options; pipe operator support        |
|   [3]   | `Reader`           | source stage       | infers reader driver from filename; accepts kwargs       |
|   [4]   | `Filter`           | transform stage    | requires explicit `type` string; accepts kwargs          |
|   [5]   | `Writer`           | sink stage         | infers writer driver from filename; accepts kwargs       |
|   [6]   | `PipelineIterator` | streaming iterator | chunk-by-chunk execution with log/schema/metadata access |

[PUBLIC_TYPE_SCOPE]: module-level symbols
- rail: scan-processing

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :----------- | :------------- | :------------------------------------- |
|   [1]   | `dimensions` | dimension list | all valid PDAL point dimension records |
|   [2]   | `info`       | version info   | version and configuration details      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Pipeline construction and execution
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `Pipeline(spec=None, arrays=(), loglevel=logging.ERROR, json=None, dataframes=(), stream_handlers=()) -> Pipeline` | construction   | build from JSON string or stage sequence     |
|   [2]   | `p.execute(allowed_dims=[]) -> int`                                                                                | execution      | run pipeline; returns output point count     |
|   [3]   | `p.execute_streaming(chunk_size=10000, allowed_dims=[]) -> int`                                                    | execution      | streaming execution for streamable pipelines |
|   [4]   | `p.iterator(chunk_size=10000, prefetch=0, allowed_dims=[]) -> PipelineIterator`                                    | execution      | lazy chunk iterator                          |
|   [5]   | `p.toJSON() -> str`                                                                                                | serialization  | serialize pipeline to PDAL JSON string       |

[ENTRYPOINT_SCOPE]: Pipeline output properties
- rail: scan-processing

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------- |
|   [1]   | `p.arrays -> list[np.ndarray]`              | output         | list of structured numpy arrays post-execute |
|   [2]   | `p.meshes -> list`                          | output         | mesh data post-execute                       |
|   [3]   | `p.metadata -> str`                         | output         | JSON metadata from last execution            |
|   [4]   | `p.log -> str`                              | output         | execution log from last run                  |
|   [5]   | `p.schema -> str`                           | output         | output dimension schema as JSON              |
|   [6]   | `p.srswkt2 -> str`                          | output         | spatial reference WKT2 string                |
|   [7]   | `p.quickinfo -> str`                        | output         | quick pipeline information                   |
|   [8]   | `p.stages -> list[Stage]`                   | query          | stages in this pipeline                      |
|   [9]   | `p.streamable -> bool`                      | query          | whether pipeline supports streaming          |
|  [10]   | `p.loglevel -> int` / `p.loglevel = value`  | config         | get/set logging level                        |
|  [11]   | `p.inputs = arrays`                         | config         | set input numpy arrays or stream handlers    |
|  [12]   | `p.get_meshio(idx) -> Mesh \| None`         | output         | output mesh at index as `meshio.Mesh`        |
|  [13]   | `p.get_dataframe(idx) -> DataFrame \| None` | output         | output as pandas DataFrame                   |

[ENTRYPOINT_SCOPE]: Stage construction and pipe operator
- rail: scan-processing

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------- |
|   [1]   | `Reader(filename=None, **options) -> Reader`                  | construction   | source stage; driver inferred from path  |
|   [2]   | `Filter(type: str, **options) -> Filter`                      | construction   | transform stage; explicit type required  |
|   [3]   | `Writer(filename=None, **options) -> Writer`                  | construction   | sink stage; driver inferred from path    |
|   [4]   | `stage \| other -> Pipeline`                                  | composition    | chain stages/pipelines via pipe operator |
|   [5]   | `pipeline \|= other -> Pipeline`                              | composition    | in-place append stage or pipeline        |
|   [6]   | `stage.pipeline(*arrays, loglevel=logging.ERROR) -> Pipeline` | construction   | wrap stage in Pipeline with input arrays |
|   [7]   | `stage.type -> str`                                           | query          | PDAL stage type string                   |
|   [8]   | `stage.tag -> str \| None`                                    | query          | stage tag for graph referencing          |
|   [9]   | `stage.inputs -> list[Stage \| str]`                          | query          | upstream stage inputs                    |
|  [10]   | `stage.options -> dict[str, Any]`                             | query          | stage option dict                        |

[ENTRYPOINT_SCOPE]: PipelineIterator and module-level functions
- rail: scan-processing

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------ | :------------- | :------------------------------------- |
|   [1]   | `iter.__next__() -> np.ndarray` | streaming      | next chunk as structured numpy array   |
|   [2]   | `iter.log -> str`               | output         | log for completed chunks               |
|   [3]   | `iter.schema -> str`            | output         | output schema JSON                     |
|   [4]   | `iter.metadata -> str`          | output         | metadata JSON                          |
|   [5]   | `pdal.info -> dict`             | query          | PDAL version and build configuration   |
|   [6]   | `pdal.dimensions -> list`       | query          | all registered point dimension records |

## [4]-[IMPLEMENTATION_LAW]

[PIPELINE_TOPOLOGY]:
- `Pipeline` accepts either a PDAL JSON string (`spec`) or a sequence of `Stage` instances; stage sequences are serialized to PDAL JSON internally.
- `p.execute()` populates `p.arrays`, `p.meshes`, `p.metadata`, `p.log`; these are undefined before execution.
- `p.execute_streaming()` and `p.iterator()` require a streamable pipeline; check `p.streamable` before calling.
- `p.arrays` is a list of structured `numpy.ndarray` objects, one per stage that produces output; dimension names are the structured array field names.
- `Reader` infers its driver from the file extension via `libpdalpython.infer_reader_driver`; an explicit `type` keyword overrides inference. `Filter` always requires an explicit `type`.
- The pipe operator `|` creates a new `Pipeline`; `|=` mutates the left operand in place.
- `p.inputs = [array]` supplies numpy arrays as in-memory point sources; `stream_handlers` pass callables for streaming sources.

[LOCAL_ADMISSION]:
- Point data enters as either a file path (via `Reader(filename)`) or a numpy structured array (via `p.inputs`).
- Output exits as `p.arrays` (structured numpy) or `p.get_dataframe(idx)` / `p.get_geodataframe(idx)` for pandas/geopandas consumers.
- PDAL JSON pipelines persist as strings; `toJSON()` normalizes the in-memory graph back to PDAL JSON for serialization.

[RAIL_LAW]:
- Package: `pdal`
- Owns: PDAL stage graph construction, point-cloud pipeline execution, streaming iteration, and driver introspection
- Accept: PDAL JSON strings, file-path-based Reader/Writer stages, numpy structured arrays as inputs
- Reject: hand-rolled point-cloud format parsing; direct libpdal C++ API calls from Python; installation without native libpdal
