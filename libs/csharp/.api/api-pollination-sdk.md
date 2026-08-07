# [RASM_API_POLLINATION_SDK]

`PollinationSDK` owns the Pollination cloud compute transport: the OpenAPI `*Api` REST clients, the `Client.Configuration` token-auth surface, and the `Wrapper` job/run/asset orchestration. Two folders split one run: `Rasm.Compute` holds the `EnergyRoute.Cloud` dispatch policy — which recipe, which `ElementGraph`-derived inputs, and whether the cloud arm or the local `EnergyToolchain` subprocess runs — while the durable result half lands across three `Rasm.Persistence` owners — artifact bytes at `Store/blobstore`, lineage at `Version/provenance`, the run index at `Query/cache#ARTIFACT_BLOB_INDEX`. Sidecar isolation binds it outside-Rhino behind the vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` fork closure and a local `Microsoft.Data.Sqlite` cache, never loaded by the plugin assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PollinationSDK`
- package: `PollinationSDK` (MIT)
- assembly: `PollinationSDK`
- namespace: `PollinationSDK` (model DTOs), `.Api` (REST clients), `.Client` (config/auth/serialization), `.Wrapper` (orchestration), `.Interface.*` (recipe/job/io model)
- depends: `LBT.RestSharp` (RestSharp-106 fork; HTTP transport), `LBT.Newtonsoft.Json` (Newtonsoft fork; JSON via `AnyOfJsonConverter`/`OpenAPIDateConverter`), `Microsoft.Data.Sqlite` (the `Wrapper.LocalDatabase` cache; native `e_sqlite3`)
- target: `netstandard2.0` (the sole TFM; a `net10.0` consumer binds `lib/netstandard2.0`)
- asset: runtime library, pure-managed AnyCPU; the only native floor is the transitive `Microsoft.Data.Sqlite` `e_sqlite3`
- rail: cloud-run

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Client` infrastructure

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------- |
|  [01]   | `Configuration`                       | config/auth   | token-auth config root                              |
|  [02]   | `GlobalConfiguration : Configuration` | config        | ambient `Instance` the parameterless `*Api` binds   |
|  [03]   | `ApiClient`                           | transport     | `: IReadableConfiguration`; RestSharp executor      |
|  [04]   | `TokenRepo`                           | auth          | holds and refreshes the access token (`GetToken`)   |
|  [05]   | `ApiResponse<T>`                      | response      | `Data`/`StatusCode`/`Headers` carrier               |
|  [06]   | `ApiException : Exception`            | fault         | `ErrorCode`/`ErrorContent`; thrown by `*Async`      |
|  [07]   | `IApiAccessor`                        | contract      | `*Api` marker (`Configuration`, `ExceptionFactory`) |
|  [08]   | `AnyOfJsonConverter`                  | converter     | the union codec on `LBT.Newtonsoft.Json`            |
|  [09]   | `OpenAPIDateConverter`                | converter     | the date codec on `LBT.Newtonsoft.Json`             |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Api` REST clients, each a generated `*Api` class pairing `*Async` and `*WithHttpInfoAsync`

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :---------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `JobsApi`                                 | run transport | job submit/poll/list/cancel/retry/delete, artifact download |
|  [02]   | `RunsApi`                                 | run transport | run state, step/output/log reads, artifact download, retry  |
|  [03]   | `ProjectsApi`                             | run transport | project CRUD, recipe-filter and access-policy admin         |
|  [04]   | `ArtifactsApi`                            | run transport | presigned-S3 artifact create/download/list/delete           |
|  [05]   | `RecipesApi` `RegistriesApi` `PluginsApi` | catalog       | recipe/registry/plugin definition supply                    |
|  [06]   | `AccountsApi` `OrgsApi` `TeamsApi`        | identity      | account/org/team ownership legs                             |
|  [07]   | `UsersApi` `UserApi` `APITokensApi`       | identity      | user and token legs                                         |
|  [08]   | `SubscriptionsApi` `SubscriptionPlansApi` | billing       | subscription and plan legs                                  |
|  [09]   | `LicensesApi` `ApplicationsApi`           | billing       | license and application legs                                |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Wrapper` high-level orchestration

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `JobInfo`                                                  | job descriptor | `Job`/`ProjectSlug`/`LocalRunFolder`; submit rail  |
|  [02]   | `JobRunner`                                                | job runner     | `RunOnCloudAsync` and static upload/status helpers |
|  [03]   | `ScheduledJobInfo`                                         | submitted job  | cloud handle; watch and delete                     |
|  [04]   | `RunInfo`                                                  | run handle     | `Run`; result-asset download                       |
|  [05]   | `AssetBase` / `RunAssetBase`                               | asset base     | the run-asset base contracts                       |
|  [06]   | `RunInputAsset` / `RunOutputAsset` / `CloudReferenceAsset` | asset          | input/output/cloud-reference asset kinds           |
|  [07]   | `JobResultPackage`                                         | result         | the packaged job result the wrapper assembles      |
|  [08]   | `LocalDatabase` / `LocalRunArguments`                      | local cache    | the `Microsoft.Data.Sqlite` job/asset cache        |
|  [09]   | `InputArgumentValidator`                                   | local cache    | local-run argument validation                      |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK` model DTOs (transport payloads)

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Job` `CloudJob` `CloudJobList` `CreatedContent`         | job           | job body, cloud state, list page, create receipt |
|  [02]   | `Run` `StepStatus` `JobStatusEnum` `RunStatusEnum`       | run           | run state, per-step status, status discriminants |
|  [03]   | `Project` `ProjectCreate`                                | project       | project transport and create body                |
|  [04]   | `ProjectRecipeFilter` `ProjectAccessPolicyList`          | access        | recipe-filter and access policy                  |
|  [05]   | `S3UploadRequest` `KeyRequest` `FileMeta` `FileMetaList` | artifact      | presigned-S3 upload, key request, metadata       |
|  [06]   | `RecipeInterface` `Inputs` `Outputs` (`.Interface.Io.*`) | recipe        | the recipe interface model `JobInfo` builds from |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: auth and client construction over `Configuration`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Configuration.Default`                          | static   | ambient config the parameterless `*Api` binds |
|  [02]   | `Configuration.AccessToken`                      | property | bearer token the request executor sends       |
|  [03]   | `Configuration.AddDefaultHeader(string, string)` | instance | inject `Authorization`/`x-pollination-token`  |
|  [04]   | `new JobsApi(Configuration)`                     | ctor     | REST client bound to an explicit config       |
|  [05]   | `new JobsApi(string)`                            | ctor     | REST client bound to a base path              |
|  [06]   | `new JobsApi()`                                  | ctor     | REST client bound to the global `Instance`    |

App-root code acquires and hands over the access token; a `*Api` binds the ambient `GlobalConfiguration.Instance` or an explicit `Configuration`, so token lifecycle is not a fence member.

[ENTRYPOINT_SCOPE]: high-level cloud transport over `Wrapper`

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new JobInfo(Job\|RecipeInterface)` / `JobInfo.FromJson(string)`               | ctor     | job descriptor; `ToJson()` round-trips JSON |
|  [02]   | `JobInfo.RunJobAsync(…) -> Task<ScheduledJobInfo>`                             | instance | upload assets then submit                   |
|  [03]   | `JobInfo.UploadJobAssetsAsync(…) -> Task<Job>`                                 | instance | upload input artifacts pre-submit           |
|  [04]   | `JobInfo.Set{LocalJob, CloudJob, Platform, JobSubFolderPath}` / `AddArgument`  | instance | descriptor mutation before submit           |
|  [05]   | `JobRunner.RunOnCloudAsync(Project, …) -> Task<CloudJob>`                      | instance | submit to a project — all three REQUIRED    |
|  [06]   | `JobRunner.UploadJobAssetsAsync(Project, Job, subfolderPath, …) -> Task<Job>`  | static   | pre-submit upload off a bare `Job`          |
|  [07]   | `JobRunner.RunOnLocalMachine(workFolder, workerNum, silentMode) -> string`     | instance | local queenbee execution arm                |
|  [08]   | `JobRunner.CheckLocalJobStatus -> RunStatusEnum` / `GetJobErrors -> string`    | static   | local run terminal state and error text     |
|  [09]   | `JobRunner.CheckRecipeInProject(string, string, Project, string) -> string`    | static   | recipe-filter admission probe               |
|  [10]   | `ScheduledJobInfo.From(Project, string)` / `(string ×3)` / `(string)`          | static   | cloud handle from ids or a run folder       |
|  [11]   | `ScheduledJobInfo.WatchJobStatusAsync(…) -> Task<string>`                      | instance | poll to a terminal status                   |
|  [12]   | `ScheduledJobInfo.{JobStatus, IsCloudJobDone(out string), SyncCloudJob()}`     | instance | non-polling status read and refresh         |
|  [13]   | `ScheduledJobInfo.CancelJob()` / `DeleteAsync() -> Task<bool>`                 | instance | cancel in flight, delete the scheduled job  |
|  [14]   | `new RunInfo(…)`                                                               | ctor     | run handle off ids or a run folder          |
|  [15]   | `RunInfo.GetOutputAssets(string) -> List<RunOutputAsset>` / `GetInputAssets()` | instance | roster `DownloadRunAssetsAsync` consumes    |
|  [16]   | `RunInfo.DownloadRunAssetsAsync(…) -> Task<List<RunAssetBase>>`                | instance | pull the named result assets                |
|  [17]   | `RunInfo.LoadLocalRunAssets(List<RunAssetBase>, string) -> List<RunAssetBase>` | instance | the local-run counterpart, no transfer      |
|  [18]   | `RunInfo.{IsLocalRun, IsCloudRunDone, CloudRunStatus, GetStatusMessage()}`     | instance | run-state reads off the held `Run`          |

- `RunInfo` ctors: `(Project, Run)`, `(Project, string)`, `(JobInfo)`, `(ScheduledJobInfo)`, `(string)`.

No shared async tail exists — the progress delegate is named per member and a named-argument call site binds the exact spelling: `RunJobAsync`/`UploadJobAssetsAsync` take `(Action<string> progressReporting = null, CancellationToken token = default)`, `WatchJobStatusAsync` takes `(Action<string> progressAction = null, CancellationToken cancelToken = default)`, the static `JobRunner.UploadJobAssetsAsync` takes `progressLogAction` with `Action actionWhenDone = null` AFTER the token, `RunOnCloudAsync` defaults NONE of its three, and `DeleteAsync()` carries neither delegate nor token. `DownloadRunAssetsAsync` decompiles as `(List<RunAssetBase> runAssets, string saveAsDir = null, Action<string> reportingAction = null, bool useCached = false, CancellationToken cancelToken = default)` — the asset list is REQUIRED, no zero-argument overload exists, and `useCached` consults the `Wrapper.LocalDatabase` cache; the run handle pulls assets back for the `Version/provenance` owner to land content-keyed.

[ENTRYPOINT_SCOPE]: low-level REST over `JobsApi`/`RunsApi`/`ProjectsApi`/`ArtifactsApi` — every member opens on `(owner, name)`

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `JobsApi.CreateJobAsync(Job, authorization, xPollinationToken) -> Task<CreatedContent>`  | instance | submit a job, headers included     |
|  [02]   | `JobsApi.GetJobAsync(jobId) -> Task<CloudJob>`                                           | instance | poll one job                       |
|  [03]   | `JobsApi.ListJobsAsync(…) -> Task<CloudJobList>`                                         | instance | page and filter jobs               |
|  [04]   | `JobsApi.CancelJobAsync` / `RetryJobAsync(…, RetryConfig)` / `DeleteJobAsync(jobId)`     | instance | job lifecycle past submit          |
|  [05]   | `JobsApi.SearchJobFolderAsync(jobId, path, page, perPage) -> Task<FileMetaList>`         | instance | enumerate a job's output folder    |
|  [06]   | `JobsApi.DownloadJobArtifactAsync(jobId, path) -> Task<object>`                          | instance | fetch a job artifact by path       |
|  [07]   | `RunsApi.GetRunAsync(runId) -> Task<Run>`                                                | instance | run state                          |
|  [08]   | `RunsApi.ListRunsAsync(jobId, status, page, perPage) -> Task<RunList>`                   | instance | page the runs of a job             |
|  [09]   | `RunsApi.GetRunOutputAsync(runId, outputName) -> Task<object>`                           | instance | named run output                   |
|  [10]   | `RunsApi.QueryResultsAsync(jobId, status, page, perPage) -> Task<RunResultList>`         | instance | result rows across a job's runs    |
|  [11]   | `RunsApi.GetAllRunStepsAsync` / `GetRunStepsAsync(…, StepStatusEnum?, stepId, …)`        | instance | per-step status, whole or filtered |
|  [12]   | `RunsApi.GetRunStepLogsAsync(runId, stepId) -> Task<string>`                             | instance | one step's log text                |
|  [13]   | `RunsApi.ListRunArtifactsAsync(runId, path, page, perPage) -> Task<FileMetaList>`        | instance | enumerate a run's artifacts        |
|  [14]   | `RunsApi.DownloadRunArtifactAsync(runId, path) -> Task<object>`                          | instance | fetch a run artifact by path       |
|  [15]   | `RunsApi.CancelRunAsync` / `RetryRunAsync(runId, RetryConfig)`                           | instance | run lifecycle past submit          |
|  [16]   | `ProjectsApi.GetProjectAsync() -> Task<Project>`                                         | instance | project transport                  |
|  [17]   | `ProjectsApi.CreateProjectAsync(ProjectCreate) -> Task<CreatedContent>`                  | instance | create a project; owner only       |
|  [18]   | `ProjectsApi.GetProjectRecipesAsync(search, page, perPage) -> Task<RecipeInterfaceList>` | instance | the recipe set a project admits    |
|  [19]   | `ProjectsApi.CreateProjectRecipeFilterAsync` / `GetProjectRecipeFiltersAsync`            | instance | recipe-filter admission policy     |
|  [20]   | `ArtifactsApi.CreateArtifactAsync(KeyRequest) -> Task<S3UploadRequest>`                  | instance | presigned S3 upload request        |
|  [21]   | `ArtifactsApi.DownloadArtifactAsync(path) -> Task<object>`                               | instance | fetch an artifact by path          |
|  [22]   | `ArtifactsApi.ListArtifactsAsync(path, page, perPage) -> Task<FileMetaList>`             | instance | list artifacts                     |
|  [23]   | `ArtifactsApi.DeleteArtifactAsync(owner, name, path, page, perPage) -> Task`             | instance | delete by path — returns bare `Task` |

- `ProjectsApi.CreateProjectAsync`: takes `owner` alone — no `name` argument.

Each op pairs a model-returning `*Async` (throws `ApiException`) with a `*WithHttpInfoAsync` returning `ApiResponse<T>`. Every op leads `(string owner, string name, …)` and closes on `CancellationToken cancellationToken = default` — `ProjectsApi.CreateProjectAsync` leads on `owner` ALONE and `ProjectsApi.ListProjectsAsync` leads on neither, so a positional call written against the two-leader shape binds the wrong argument. `JobsApi.CreateJobAsync` is the sole op carrying `string authorization = null, string xPollinationToken = null` before the token; reading that pair as the shared skeleton mis-positions the token on every other op. `ListJobsAsync` filters are `List<string> ids`, `JobStatusEnum? status`, `DateTime? createdAfter`, `DateTime? createdBefore`, `int? page`, `int? perPage`; the delete legs (`DeleteArtifactAsync`, `DeleteProjectAsync`) return bare `Task` and carry no body to inspect.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `LBT.RestSharp` (HTTP) and `LBT.Newtonsoft.Json` (JSON) carry distinct package ids from the Persistence folder's `Newtonsoft.Json` and its System.Text.Json rails, so the vendored RestSharp-106 + Newtonsoft-fork closure never collides.
- `Microsoft.Data.Sqlite` is touched only by the `Wrapper.LocalDatabase` cache through the ADO.NET `SqliteConnection`/`SqliteCommand` surface; the `*Api` REST layer never references it.
- Token auth is connection input the app root hands over (`Configuration.AddDefaultHeader`/`TokenRepo`), never a fence member.
- Every cloud-route failure surfaces as the typed `ComputeFault.AnalysisFailed` row (`(Solve, Foreign)` / `(Admission, Timeout)`) with the HTTP status on `Diagnostic.Code`, never a stringly interpolated arm.

[STACKING]:
- `api-objectstore.md`(`Rasm.Persistence/.api/api-objectstore.md`): `ArtifactsApi.CreateArtifactAsync` returns an `S3UploadRequest` presigned PUT and the `*Download*` ops resolve S3-backed assets, so the byte transfer rides the folder's object-store owner (`AWSSDK.S3`/`Minio`) on the same S3 plane; a downloaded `RunAsset` lands content-keyed (`XxHash128`) through that same body bridge, never a second HTTP uploader.
- `Microsoft.Data.Sqlite`(`api-sqlite.md`): the pulled `SqlFile` folds through the same read-only extraction over the bracketed scratch artifact the local subprocess route drives — one tabular reader serves both routes.
- Compute consumer anchor: `Analysis/energy` owns the recipe-run dispatch policy — `EnergyRoute.Cloud` builds `JobInfo` from recipe plus `ElementGraph`-derived OSM/IDF inputs behind the runner entry point, threading `JobInfo` → `RunJobAsync` → `WatchJobStatusAsync` → `RunInfo.GetOutputAssets(platform)` → `DownloadRunAssetsAsync(assets, saveAsDir:)`, so a cloud simulation and a local one share one result-extraction seam.
- Persistence consumer anchor: a `Run` result and its `RunOutputAsset`s land at `Version/provenance` (lineage) and `Query/cache#ARTIFACT_BLOB_INDEX` (result index), so a completed cloud run becomes a content-addressed, lineage-tracked artifact set.

[LOCAL_ADMISSION]:
- No in-Rhino plugin assembly admits `PollinationSDK` or its RestSharp-106/Newtonsoft-fork closure; the SDK and its SQLite cache load only on the cloud-run sidecar.
- `EnergyRoute.Cloud` is one dispatch arm of `Analysis/energy`, selected against the local `EnergyToolchain` subprocess arm; token auth is app-root connection input handed to `Configuration`/`TokenRepo`, never a Compute fence member.

[RAIL_LAW]:
- Package: `PollinationSDK` (MIT)
- Owns: the Pollination cloud compute transport — the `*Api` REST clients, `Configuration`/`TokenRepo` auth, `Wrapper` job/run/asset orchestration, and the model DTOs
- Accept: a recipe-run job submitted to a Pollination project, watched to completion, and its result assets pulled back — the dispatch half at `Rasm.Compute`, the durable half projected to `Store/blobstore`, `Version/provenance`, and `Query/cache#ARTIFACT_BLOB_INDEX`, artifact bytes transferred via the object-store owner
- Reject: loading the SDK or its forks in the in-Rhino assembly; a second S3 uploader where the object-store owner holds the plane; a hand-rolled token store where `Configuration`/`TokenRepo` carry auth; a stringly cloud-arm fault where the typed `AnalysisFailed` row belongs; treating the netstandard2.0 floor as a net8+ surface
