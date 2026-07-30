# [RASM_COMPUTE_API_POLLINATION_SDK]

`Rasm.Persistence` owns the `PollinationSDK` surface for this branch at `libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md` — the OpenAPI `*Api` REST clients, the `Client.Configuration` token-auth surface, the recipe interface model, and the whole `PollinationSDK.Wrapper` job/run/asset roster — so Compute registers that catalogue rather than re-tabling it. This partition holds the `EnergyRoute.Cloud` dispatch POLICY alone: which recipe, which `ElementGraph`-derived inputs, and whether the cloud arm or the local `EnergyToolchain` subprocess runs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: Compute dispatch partition of `PollinationSDK`
- package: `PollinationSDK` (MIT, direct `PackageReference`)
- assembly: `PollinationSDK`; Compute reaches `PollinationSDK.Wrapper` alone
- asset: pure-managed netstandard2.0; the `net10.0` consumer binds `lib/netstandard2.0`
- rail: cloud-run (Compute dispatch policy)

- Registers the SDK transport and wrapper surface(`libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md`): `JobInfo`, `ScheduledJobInfo`, `RunInfo`, `AssetBase`/`RunAssetBase`, the recipe interface model, and every submit, watch, upload, delete, and download member resolve there beside the REST and auth surface — a member verified against that catalogue is verified for this dispatch, and re-tabling one here forks the branch's SDK truth.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Analysis/energy` owns the recipe-run dispatch policy — which recipe, which inputs, and whether `EnergyRoute.Cloud` or the local `EnergyToolchain` subprocess runs — while auth, REST, and artifact upload stay Persistence's; the dispatch threads build `JobInfo` → `RunJobAsync` → `WatchJobStatusAsync` → `RunInfo.GetOutputAssets(platform)` → `DownloadRunAssetsAsync(assets, saveAsDir:)`, and the Persistence catalog owns the measured signatures.
- No shared async tail exists across that thread, so the dispatch calls each entrypoint POSITIONALLY or under its own delegate spelling: `RunJobAsync` names `progressReporting`/`token`, `WatchJobStatusAsync` names `progressAction`/`cancelToken`, and `DownloadRunAssetsAsync` interposes `string saveAsDir = null` before `reportingAction` and `bool useCached = false` before `cancelToken` while REQUIRING its asset list — a call site assuming one tail spelling fails to compile at whichever leg it guessed wrong.
- Every cloud-route failure surfaces as the typed `ComputeFault.AnalysisFailed` row (`(Solve, Foreign)` / `(Admission, Timeout)`) with the HTTP status on `Diagnostic.Code`, never a stringly interpolated arm.
- `LBT.RestSharp` and `LBT.Newtonsoft.Json` — the vendored SDK fork closure — load OUTSIDE-RHINO on the sidecar, never in the in-Rhino plugin assembly.

[STACKING]:
- `PollinationSDK`(`libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md`): the durable half lands Persistence-side — artifact bytes at `Store/blobstore`, lineage at `Version/provenance`, the completed run at `Query/cache#ARTIFACT_BLOB_INDEX`.
- `api-sqlite`(`api-sqlite.md`): the pulled `SqlFile` folds through the same `Microsoft.Data.Sqlite` read-only extraction over the bracketed scratch artifact the local subprocess route drives.
- within-lib: `EnergyRoute.Cloud` builds `JobInfo` from recipe plus `ElementGraph`-derived OSM/IDF inputs behind the `Analysis/energy` runner entry point, so a cloud simulation and a local one share one result-extraction seam.

[LOCAL_ADMISSION]:
- `EnergyRoute.Cloud` is one dispatch arm of `Analysis/energy`, selected against the local `EnergyToolchain` subprocess arm; the SDK and its fork closure bind only on the sidecar.
- Token auth is app-root connection input handed to `Configuration`/`TokenRepo`, never a Compute fence member.

[RAIL_LAW]:
- Package: `PollinationSDK`
- Owns: the `EnergyRoute.Cloud` recipe-run dispatch policy — build the job descriptor, submit and watch through the registered `Wrapper` surface, hand the result to the Persistence durable landing
- Accept: a cloud-routed energy simulation whose result folds through the shared `SqlFile` extraction and lands content-keyed Persistence-side
- Reject: a member roster for any part of the SDK here, loading the SDK or its RestSharp/Newtonsoft forks in the in-Rhino assembly, and a stringly cloud-arm fault where the typed `AnalysisFailed` row belongs
