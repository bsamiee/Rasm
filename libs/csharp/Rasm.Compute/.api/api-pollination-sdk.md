# [RASM_COMPUTE_API_POLLINATION_SDK]

`PollinationSDK` is a cross-tier package: its DURABLE half (artifact bytes, lineage, cloud-run result index) is Persistence-owned and the canonical full-surface catalog is `libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md` (the `*Api` REST clients, `Configuration`/`TokenRepo` auth, `Wrapper` orchestration, model DTOs, and the `S3UploadRequest` object-store seam). Compute owns ONE slice: the `EnergyRoute.Cloud` run-dispatch policy — the `Analysis/energy` runner routes a recipe job to a Pollination project, watches it to completion, and hands the pulled result back to the Persistence durable landing. This catalog carries that dispatch delta only; read the Persistence catalog for the transport surface it composes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PollinationSDK` (Compute cloud-run-dispatch slice)
- package: `PollinationSDK`
- version: `1.10.0`
- license: MIT (Pollination / Ladybug Tools)
- canonical catalog: `libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md` (durable half + full SDK surface)
- assembly: `PollinationSDK`; namespace `PollinationSDK.Wrapper` (the dispatch surface Compute composes)
- asset: pure-managed netstandard2.0; the `net10.0` consumer binds `lib/netstandard2.0`. Sidecar-ISOLATED — the vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` fork closure runs OUTSIDE-RHINO on the cloud-run sidecar, never in the in-Rhino plugin assembly
- rail: cloud-run (Compute dispatch policy)

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `EnergyRoute.Cloud` dispatch — `PollinationSDK.Wrapper`
- rail: cloud-run
- composition law: `Analysis/energy`'s `EnergyRoute.Cloud` arm builds a `JobInfo` from the recipe + `ElementGraph`-derived OSM/IDF inputs, submits through `RunJobAsync`, watches to a terminal status, and pulls the result assets whose `SqlFile` (eplusout.sql) folds through the SAME extraction the subprocess route uses; the durable half (artifact bytes, lineage, result index) lands Persistence-side.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                 |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------------------------------ | :------------------------------------------- |
|  [01]   | `new JobInfo(job)`                 | `JobInfo(Job)` / `JobInfo(RecipeInterface)`                                                              | the job descriptor the energy runner builds  |
|  [02]   | `jobInfo.RunJobAsync`              | `Task<ScheduledJobInfo> RunJobAsync(Action<string> progress = null, CancellationToken = default)`        | upload inputs + submit in one                |
|  [03]   | `scheduled.WatchJobStatusAsync`    | `Task<string> WatchJobStatusAsync(Action<string> progress = null, CancellationToken = default)`          | poll to a terminal run status                |
|  [04]   | `new RunInfo(project, runId).DownloadRunAssetsAsync` | `Task<List<RunAssetBase>> DownloadRunAssetsAsync(List<RunAssetBase>, string saveAsDir = null, …)` | pull result assets (the `SqlFile` carrier)   |

## [03]-[IMPLEMENTATION_LAW]

[DISPATCH_BOUNDARY]:
- Compute owns the recipe-run dispatch POLICY (which recipe, which inputs, when to route `EnergyRoute.Cloud` vs the local `EnergyToolchain` subprocess); the transport surface (auth, REST, artifact upload) is the Persistence catalog's. A cloud-route failure surfaces as the typed `(Solve, Foreign)` / `(Admission, Timeout)` `ComputeFault.AnalysisFailed` row with the HTTP status riding `Diagnostic.Code`, per the energy runner's typed-failure classification — never a stringly interpolated cloud arm.
- The pulled `SqlFile` folds through the identical eplusout.sql extraction the subprocess route drives (`Microsoft.Data.Sqlite` read-only over the bracketed scratch artifact); the durable result lands content-keyed through the Persistence `Store/blobstore` + `Version/provenance` + `Query/cache#ArtifactKind.CloudRun` owners.

[RAIL_LAW]:
- Package: `PollinationSDK` `1.10.0` (Compute cloud-run-dispatch slice; durable half + full surface at the Persistence catalog)
- Owns: the `EnergyRoute.Cloud` recipe-run dispatch policy — build `JobInfo`, submit + watch through `Wrapper`, hand the result to the Persistence durable landing
- Accept: a cloud-routed energy simulation whose result folds through the shared `SqlFile` extraction and lands content-keyed Persistence-side
- Reject: loading the SDK or its RestSharp/Newtonsoft forks in the in-Rhino assembly; re-documenting the transport/auth/artifact surface the Persistence catalog owns; a stringly cloud-arm fault where the typed `AnalysisFailed` row belongs
