# [RASM_COMPUTE_API_POLLINATION_SDK]

`PollinationSDK` routes the `EnergyRoute.Cloud` recipe-run dispatch policy — `Analysis/energy` builds a `JobInfo` from the recipe and `ElementGraph`-derived inputs, submits and watches it through `Wrapper`, and folds the pulled `SqlFile` through the shared eplusout.sql extraction. Persistence owns the SDK's full REST/auth/artifact transport surface and the durable result landing — artifact bytes, lineage, cloud-run index — this dispatch composes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PollinationSDK`
- package: `PollinationSDK`
- license: MIT (Pollination / Ladybug Tools)
- assembly: `PollinationSDK`; namespace `PollinationSDK.Wrapper`
- asset: pure-managed netstandard2.0; the `net10.0` consumer binds `lib/netstandard2.0`
- rail: cloud-run (Compute dispatch policy)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Wrapper` dispatch types the energy runner composes

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                               |
| :-----: | :----------------- | :------------- | :----------------------------------------- |
|  [01]   | `JobInfo`          | job descriptor | recipe-run descriptor the runner builds    |
|  [02]   | `ScheduledJobInfo` | submitted job  | cloud handle watched to a terminal status  |
|  [03]   | `RunInfo`          | run handle     | download handle over a completed run       |
|  [04]   | `RunAssetBase`     | asset base     | pulled result asset carrying the `SqlFile` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EnergyRoute.Cloud` dispatch chain — `PollinationSDK.Wrapper`

`Analysis/energy` threads build `JobInfo` -> `RunJobAsync` -> `WatchJobStatusAsync` -> `DownloadRunAssetsAsync`, every async call ending the shared `(Action<string> progress = null, CancellationToken = default)` tail.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `JobInfo(Job)`                                                                   | ctor     | job descriptor from a recipe job          |
|  [02]   | `JobInfo(RecipeInterface)`                                                       | ctor     | job descriptor from a recipe interface    |
|  [03]   | `JobInfo.RunJobAsync() -> Task<ScheduledJobInfo>`                                | instance | upload inputs and submit in one           |
|  [04]   | `ScheduledJobInfo.WatchJobStatusAsync() -> Task<string>`                         | instance | poll to a terminal run status             |
|  [05]   | `RunInfo.DownloadRunAssetsAsync(List<RunAssetBase>) -> Task<List<RunAssetBase>>` | instance | pull result assets carrying the `SqlFile` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Analysis/energy` owns the recipe-run dispatch policy — which recipe, which inputs, and whether `EnergyRoute.Cloud` or the local `EnergyToolchain` subprocess runs — while auth, REST, and artifact upload stay Persistence's.
- A cloud-route failure surfaces as the typed `ComputeFault.AnalysisFailed` row (`(Solve, Foreign)` / `(Admission, Timeout)`) with the HTTP status on `Diagnostic.Code`, never a stringly interpolated arm.
- `LBT.RestSharp` and `LBT.Newtonsoft.Json` — the vendored SDK fork closure — load OUTSIDE-RHINO on the sidecar, never in the in-Rhino plugin assembly.

[STACKING]:
- `PollinationSDK`(`libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md`): the durable half lands Persistence-side — artifact bytes at `Store/blobstore`, lineage at `Version/provenance`, the completed run at `Query/cache#ArtifactKind.CloudRun` — and that catalog owns the full REST/auth/artifact transport surface.
- `api-sqlite`(`api-sqlite.md`): the pulled `SqlFile` folds through the same `Microsoft.Data.Sqlite` read-only extraction over the bracketed scratch artifact the local subprocess route drives.
- within-lib: `EnergyRoute.Cloud` builds `JobInfo` from recipe + `ElementGraph`-derived OSM/IDF inputs behind the `Analysis/energy` runner entry point, so a cloud simulation and a local one share one result-extraction seam.

[LOCAL_ADMISSION]:
- `EnergyRoute.Cloud` is one dispatch arm of `Analysis/energy`, selected against the local `EnergyToolchain` subprocess arm; the SDK and its fork closure bind only on the sidecar.
- Token auth is app-root connection input handed to `Configuration`/`TokenRepo`, never a Compute fence member.

[RAIL_LAW]:
- Package: `PollinationSDK`
- Owns: the `EnergyRoute.Cloud` recipe-run dispatch policy — build `JobInfo`, submit and watch through `Wrapper`, hand the result to the Persistence durable landing
- Accept: a cloud-routed energy simulation whose result folds through the shared `SqlFile` extraction and lands content-keyed Persistence-side
- Reject: loading the SDK or its RestSharp/Newtonsoft forks in the in-Rhino assembly; re-documenting the transport/auth/artifact surface Persistence owns; a stringly cloud-arm fault where the typed `AnalysisFailed` row belongs
