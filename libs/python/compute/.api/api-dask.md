# [PY_COMPUTE_API_DASK]

`dask` supplies lazy, chunked, parallel array and dataframe computation with a deferred task graph for the compute study-orchestration rail. The package owner partitions large study payloads into chunked arrays and orchestrates experiment-run fan-out across the task scheduler; it never re-implements the parallel scheduler dask owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask`
- import: submodules `dask.array` (lint alias `da`), `dask.dataframe` (lint alias `dd`); `dask.delayed`, `dask.distributed`
- owner: `compute`
- rail: studies
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: lazy chunked computation — blocked NumPy-compatible arrays, partitioned dataframes, a deferred task graph, and local/distributed schedulers

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: lazy-computation owners
- rail: studies

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `dask.array.Array` | chunked array | blocked NumPy-compatible array |
| [2] | `dask.dataframe.DataFrame` | partitioned frame | blocked pandas-compatible frame |
| [3] | `dask.delayed` | deferred call | wraps a call into a task-graph node |
| [4] | `dask.distributed.Client` | scheduler client | submits the graph to a cluster |

[ENTRYPOINTS]:
- UN_REFLECTED: exact callable spellings (`dask.array.from_array`, `dask.compute`, `Array.compute`, `delayed.compute`, `Client.submit`) and verified signatures require a reflectable install to capture; type/submodule names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- chunking: a large study payload partitions into a `dask.array.Array` whose chunk facts join the array-admission record; the graph stays lazy until `compute`.
- orchestration: experiment-run fan-out composes through `dask.delayed` nodes submitted to a local or `distributed.Client` scheduler; the study receipt captures the partition count and scheduler class.
- boundary: dask orchestration is offline study evidence; production scheduling and substrate selection stay in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `dask`
- Owns: lazy chunked arrays/dataframes, the deferred task graph, and study/experiment-run orchestration for the studies rail
- Accept: a chunked study payload or experiment-run graph with a captured partition count and scheduler class
- Reject: eager full-materialization where chunking applies; hand-rolled parallel loops dask owns; product scheduling claims
