# [RASM_PERSISTENCE_API_POLLINATION_SDK]

`PollinationSDK` supplies the Pollination cloud compute transport — the generated OpenAPI `*Api` REST clients (`JobsApi`/`RunsApi`/`ProjectsApi`/`ArtifactsApi`/`RecipesApi` + the account/org/team/registry/subscription leg), the `Client.Configuration` token-auth surface, and the high-level `Wrapper` orchestration (`JobInfo`/`JobRunner`/`ScheduledJobInfo`/`RunInfo` + the run-asset family). Its DURABLE half spans three Persistence owners: the presigned-S3 artifact plane binds `Store/blobstore` (`ArtifactsApi.CreateArtifactAsync` returns an `S3UploadRequest`, so the bytes ride the SAME S3/S3-compatible object plane the folder's `Store/blobstore` owner already drives — `AWSSDK.S3`, `Minio`, `api-objectstore.md`), a downloaded `RunAsset` lands as a content-keyed (`XxHash128`) blob whose lineage records at `Version/provenance`, and the completed-run result index lands at `Query/cache#ArtifactKind.CloudRun`. The job-SUBMISSION/watch orchestration half ROUTES to `Rasm.Compute` (the cloud-route counterpart owner): this catalog carries the SDK transport surface, the run-dispatch policy is a Compute concern. The SDK runs OUTSIDE-RHINO on the cloud-run sidecar: it carries its OWN vendored `LBT.RestSharp` (RestSharp 106) + `LBT.Newtonsoft.Json` fork closure plus a local `Microsoft.Data.Sqlite` job cache, and the in-Rhino plugin assembly never references it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PollinationSDK`
- package: `PollinationSDK`
- version: `1.10.0`
- license: MIT (`licenses.nuget.org/MIT` — Pollination / Ladybug Tools)
- assembly: `PollinationSDK`
- namespace: `PollinationSDK` (model DTOs), `.Api` (REST clients), `.Client` (config/auth/serialization infra), `.Wrapper` (high-level orchestration), `.Interface.*` (recipe/job/io interface model)
- dependencies (vendored-fork isolation): `LBT.RestSharp` `106.11.7.1` (RestSharp 106 fork — the SDK's HTTP transport), `LBT.Newtonsoft.Json` `13.0.3.2` (Newtonsoft fork — the SDK's JSON codec via `AnyOfJsonConverter`/`OpenAPIDateConverter`), `Microsoft.Data.Sqlite` `10.0.9` (the `Wrapper.LocalDatabase` job/asset cache, native `e_sqlite3`), `Pollination.Logger` `1.1.4`, `Microsoft.CSharp` `4.7.0` (dynamic), `System.ComponentModel.Annotations` `5.0.0`
- target frameworks: `netstandard2.0`
- asset: runtime library, pure-managed AnyCPU; the only native floor is the transitive `Microsoft.Data.Sqlite` `e_sqlite3`. The `net10.0` consumer binds `lib/netstandard2.0` (the sole TFM) — a netstandard2.0 floor, no `net8`+ surface.
- rail: cloud-run (cloud compute transport)
- ABI floor: the SDK's JSON + HTTP run through the DISTINCT-package-id Ladybug forks (`LBT.RestSharp`/`LBT.Newtonsoft.Json`), so they never collide with the folder's `Newtonsoft.Json` `13.0.4` or its System.Text.Json rails. `Microsoft.Data.Sqlite` `10.0.9` is touched ONLY by the `Wrapper.LocalDatabase` cache through the ADO.NET `SqliteConnection`/`SqliteCommand` surface; the `*Api` REST layer never references it.

The in-sidecar assembly composes the cloud-run transport for the Compute route; the in-Rhino assembly never loads `PollinationSDK`, `LBT.RestSharp`, or the SQLite cache — the entire RestSharp-106 + Newtonsoft-fork closure stays isolated to the sidecar.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Client` infrastructure
- rail: cloud-run

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                                                        |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Configuration`         | config/auth     | `Default` static, `BasePath`, `AccessToken => TokenRepo?.GetToken()`, `DefaultHeader`, `AddApiKey`, `AddDefaultHeader`, `CreateApiClient` |
|  [02]   | `GlobalConfiguration : Configuration` | config | the ambient `Instance` a parameterless `*Api` ctor binds                            |
|  [03]   | `ApiClient`             | transport       | `: IReadableConfiguration` consumer — the RestSharp-backed request executor         |
|  [04]   | `TokenRepo`             | auth            | holds + refreshes the Pollination access token (`GetToken()`)                       |
|  [05]   | `ApiResponse<T>`        | response        | `Data`/`StatusCode`/`Headers` — the `WithHttpInfoAsync` return carrier              |
|  [06]   | `ApiException : Exception` | fault         | `ErrorCode`/`ErrorContent` — thrown by the model-returning `*Async` overloads        |
|  [07]   | `IApiAccessor`          | contract        | the `*Api` marker (`Configuration`, `ExceptionFactory`)                             |
|  [08]   | `AnyOfJsonConverter` / `OpenAPIDateConverter` | Newtonsoft converter | the union + date codecs the SDK's `LBT.Newtonsoft.Json` serializer mounts |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Api` REST clients (each paired `*Async` + `*WithHttpInfoAsync`)
- rail: cloud-run

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                                          |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `JobsApi`       | REST client   | `CreateJob`/`GetJob`/`ListJobs`/`CancelJob`/`DeleteJob`/`DownloadJobArtifact` over a project |
|  [02]   | `RunsApi`       | REST client   | `GetRun`/`GetAllRunSteps`/`GetRunOutput`/`GetRunStepLogs`/`DownloadRunArtifact`/`CancelRun` |
|  [03]   | `ProjectsApi`   | REST client   | `CreateProject`/`GetProject`/`ListProjects`/recipe-filter + access-permission admin   |
|  [04]   | `ArtifactsApi`  | REST client   | `CreateArtifact` (-> `S3UploadRequest`) / `DownloadArtifact` / `ListArtifacts` / `DeleteArtifact` |
|  [05]   | `RecipesApi` / `RegistriesApi` / `PluginsApi` | REST client | recipe/registry/plugin catalog transport (the run definition supply side) |
|  [06]   | `AccountsApi` / `OrgsApi` / `TeamsApi` / `UsersApi` / `UserApi` / `APITokensApi` | REST client | identity/ownership/token legs |
|  [07]   | `SubscriptionsApi` / `SubscriptionPlansApi` / `LicensesApi` / `ApplicationsApi` | REST client | billing/license/application legs |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK.Wrapper` high-level orchestration
- rail: cloud-run

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CAPABILITY]                                                                      |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `JobInfo`             | job descriptor    | `new JobInfo(Job)` / `new JobInfo(RecipeInterface)`; `Job`/`ProjectSlug`/`RecipeOwner`/`LocalRunFolder`; `ToJson`/`FromJson`; `RunJobAsync`/`UploadJobAssetsAsync` |
|  [02]   | `JobRunner`           | job runner        | `new JobRunner(JobInfo)`; `RunOnCloudAsync`; static `UploadJobAssetsAsync`/`CheckLocalJobStatus`/`GetJobErrors`/`CheckRecipeInProject` |
|  [03]   | `ScheduledJobInfo`    | submitted job     | the cloud-scheduled job handle; `WatchJobStatusAsync` (poll to completion) / `DeleteAsync` |
|  [04]   | `RunInfo`             | run handle        | `new RunInfo(Project, runID|Run)` / `(JobInfo|ScheduledJobInfo)` / `(localRunFolder)`; `Run`; `DownloadRunAssetsAsync` |
|  [05]   | `AssetBase` / `RunAssetBase` / `RunInputAsset` / `RunOutputAsset` / `CloudReferenceAsset` | asset | the run input/output asset family the download/upload threads |
|  [06]   | `JobResultPackage`    | result            | the packaged job result the wrapper assembles                                     |
|  [07]   | `LocalDatabase` / `LocalRunArguments` / `InputArgumentValidator` | local cache | the `Microsoft.Data.Sqlite` job/asset cache + local-run argument validation |

[PUBLIC_TYPE_SCOPE]: `PollinationSDK` model DTOs (transport payloads)
- rail: cloud-run

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `Job` / `CloudJob` / `CloudJobList` / `CreatedContent` | DTO | the job submission body, the cloud job state, the list page, the create receipt |
|  [02]   | `Run` / `StepStatus` / `JobStatusEnum` / `RunStatusEnum` | DTO | run state, per-step status, the job/run status discriminants |
|  [03]   | `Project` / `ProjectCreate` / `ProjectRecipeFilter` / `ProjectAccessPolicyList` | DTO | project transport + recipe-filter + access policy |
|  [04]   | `S3UploadRequest` / `KeyRequest` / `FileMeta` / `FileMetaList` | DTO | the presigned-S3 artifact upload request, the key request, artifact metadata |
|  [05]   | `RecipeInterface` / `Inputs`/`Outputs` family (`.Interface.Io.*`) | DTO | the recipe interface model `JobInfo` is built from |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: auth + client construction — `Configuration`
- rail: cloud-run
- composition law: the access token is acquired by the app root and handed over (token lifecycle is NOT a Persistence fence member); a `*Api` binds the ambient `GlobalConfiguration.Instance` or an explicit `Configuration`.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                                       | [CAPABILITY]                                  |
| :-----: | :------------------------------------- | :--------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `Configuration.Default`                | `static Configuration Default`                                    | the ambient config the parameterless `*Api` binds |
|  [02]   | `config.AccessToken`                   | `virtual string AccessToken => TokenRepo?.GetToken()`            | the bearer token the request executor sends    |
|  [03]   | `config.AddDefaultHeader`              | `void AddDefaultHeader(string key, string value)`               | inject `Authorization`/`x-pollination-token`   |
|  [04]   | `new JobsApi(config)`                  | `JobsApi(Configuration configuration = null)` / `JobsApi(string basePath)` / `JobsApi()` | a REST client bound to a config (or the global) |

[ENTRYPOINT_SCOPE]: high-level cloud transport — `Wrapper`
- rail: cloud-run
- composition law: `JobInfo` wraps a `Job` + project slug + local folder; `RunJobAsync` uploads assets then submits, returning a `ScheduledJobInfo`; the run handle pulls assets back, which the `Version/provenance` owner lands content-keyed.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------------------------------------------ | :------------------------------------------- |
|  [01]   | `new JobInfo(job)`                         | `JobInfo(Job job)` / `JobInfo(RecipeInterface recipe)`                                                    | the job descriptor                            |
|  [02]   | `jobInfo.RunJobAsync`                      | `Task<ScheduledJobInfo> RunJobAsync(Action<string> progressReporting = null, CancellationToken = default)` | upload assets + submit in one (the convenience) |
|  [03]   | `jobInfo.UploadJobAssetsAsync`             | `Task<Job> UploadJobAssetsAsync(Action<string> progressReporting = null, CancellationToken = default)`    | upload input artifacts before submit          |
|  [04]   | `new JobRunner(jobInfo).RunOnCloudAsync`   | `Task<CloudJob> RunOnCloudAsync(Project project, Action<string> progressReporting, CancellationToken token)` | submit a job to a project on the cloud      |
|  [05]   | `scheduled.WatchJobStatusAsync`            | `Task<string> WatchJobStatusAsync(Action<string> progressAction = null, CancellationToken cancelToken = default)` | poll the scheduled job to a terminal status |
|  [06]   | `scheduled.DeleteAsync`                    | `Task<bool> DeleteAsync()`                                                                                | delete the scheduled job                      |
|  [07]   | `new RunInfo(project, runId).DownloadRunAssetsAsync` | `Task<List<RunAssetBase>> DownloadRunAssetsAsync(List<RunAssetBase> runAssets, string saveAsDir = null, Action<string> reportingAction = null, bool useCached = false, CancellationToken cancelToken = default)` | pull result assets (optionally cached) |

[ENTRYPOINT_SCOPE]: low-level REST — `JobsApi` / `RunsApi` / `ProjectsApi` / `ArtifactsApi`
- rail: cloud-run
- composition law: every operation has a model-returning `*Async` (throws `ApiException`) and a `*WithHttpInfoAsync` returning `ApiResponse<T>` (status + headers); all take `(string owner, string name, …, CancellationToken)`.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `JobsApi.CreateJobAsync`           | `Task<CreatedContent> CreateJobAsync(string owner, string name, Job job, string authorization = null, string xPollinationToken = null, CancellationToken = default)` | submit a job to a project          |
|  [02]   | `JobsApi.GetJobAsync` / `ListJobsAsync` | `Task<CloudJob> GetJobAsync(string owner, string name, string jobId, …)` / `Task<CloudJobList> ListJobsAsync(string owner, string name, List<string> ids = null, JobStatusEnum? status = null, DateTime? createdAfter = null, …, int? page = null, int? perPage = null, …)` | poll / page jobs |
|  [03]   | `RunsApi.GetRunAsync` / `GetRunOutputAsync` | `Task<Run> GetRunAsync(string owner, string name, string runId, …)` / `Task<object> GetRunOutputAsync(string owner, string name, string runId, string outputName, …)` | run state / named output |
|  [04]   | `RunsApi.DownloadRunArtifactAsync` | `Task<object> DownloadRunArtifactAsync(string owner, string name, string runId, string path = null, …)`  | fetch a run artifact by path                |
|  [05]   | `ProjectsApi.GetProjectAsync` / `CreateProjectAsync` | `Task<Project> GetProjectAsync(string owner, string name, …)` / `Task<CreatedContent> CreateProjectAsync(string owner, ProjectCreate projectCreate, …)` | project transport |
|  [06]   | `ArtifactsApi.CreateArtifactAsync` | `Task<S3UploadRequest> CreateArtifactAsync(string owner, string name, KeyRequest keyRequest, …)`         | request a presigned S3 upload (the object-store seam) |
|  [07]   | `ArtifactsApi.DownloadArtifactAsync` / `ListArtifactsAsync` | `Task<object> DownloadArtifactAsync(string owner, string name, string path = null, …)` / `Task<FileMetaList> ListArtifactsAsync(string owner, string name, List<string> path = null, …)` | fetch / list artifacts |

## [04]-[IMPLEMENTATION_LAW]

[SIDECAR_ISOLATION]:
- `PollinationSDK` runs OUTSIDE-RHINO on the cloud-run sidecar. Its HTTP is `LBT.RestSharp` (RestSharp 106) and its JSON is `LBT.Newtonsoft.Json` — DISTINCT package ids from the folder's `Newtonsoft.Json` and its System.Text.Json rails, so the fork closure is fully isolated and never collides. The in-Rhino plugin assembly never references `PollinationSDK` or its forks.
- Token auth is connection input handed over by the app root (`Configuration.Default.AddDefaultHeader` / `TokenRepo`), not a Persistence fence member — the token lifecycle boundary the app root owns.

[ARTIFACT_S3_SEAM]:
- The Pollination artifact plane IS S3: `ArtifactsApi.CreateArtifactAsync` returns an `S3UploadRequest` (the presigned PUT), and `DownloadArtifactAsync`/`DownloadRunArtifactAsync` resolve S3-backed assets. The actual byte transfer composes the folder's `Store/blobstore` object-store owner (`AWSSDK.S3`/`Minio`, `api-objectstore.md`) — the presigned URL is uploaded to/fetched from the SAME object plane, so a Pollination artifact and a Rasm content-keyed blob share one transfer rail rather than a second HTTP uploader.
- A downloaded `RunAsset` lands as a content-keyed (`XxHash128`) blob in the Persistence store; the run-asset bytes flow through the same `AsStream` body bridge (`api-highperformance.md`) the object-store rail uses.

[STACK]:
- compute-route seam: `PollinationSDK`'s job-submission/watch half ROUTES to `Rasm.Compute` (the cloud-run dispatch owner) — this catalog owns the SDK transport surface, the recipe-run policy is a Compute concern. The durable result half stays Persistence: artifact bytes at `Store/blobstore`, lineage at `Version/provenance`, the completed-run index at `Query/cache#ArtifactKind.CloudRun`.
- object-store seam: the `S3UploadRequest`/artifact download composes `api-objectstore.md` (`AWSSDK.S3`/`Minio`) for the byte transfer through the `Store/blobstore` owner; the content-key (`XxHash128`) and `AsStream` body bridge are the folder's, not Pollination's.
- result-landing seam: a `Run` result + its `RunOutputAsset`s map onto the `Version/provenance` lineage owner and the `Query/cache#ArtifactKind.CloudRun` index — a completed cloud run becomes a content-addressed, lineage-tracked artifact set, not a loose file download.

[RAIL_LAW]:
- Package: `PollinationSDK` `1.10.0` (MIT, pure-managed netstandard2.0, `net10.0` binds `netstandard2.0`, vendored RestSharp-106 + Newtonsoft-fork closure)
- Owns: the Pollination cloud compute transport — the `*Api` REST clients, the `Configuration`/`TokenRepo` auth, the `Wrapper` job/run/asset orchestration, and the model DTOs (`Job`/`CloudJob`/`Run`/`Project`/`S3UploadRequest`/`FileMetaList`)
- Accept: a recipe-run job submitted to a Pollination project, watched to completion, and its result assets pulled back — the durable half projected to `Store/blobstore` (artifact bytes), `Version/provenance` (lineage), and `Query/cache#ArtifactKind.CloudRun` (result index) while the job-dispatch half routes to `Rasm.Compute`; the artifact bytes transferred via the folder's object-store owner
- Reject: loading `PollinationSDK` or its RestSharp/Newtonsoft forks in the in-Rhino assembly; a second S3 uploader where `api-objectstore.md` owns the object plane; a hand-rolled token store where `Configuration`/`TokenRepo` carry auth; treating the netstandard2.0 floor as a net8+ surface

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- This page carries `PollinationSDK` API facts only; the `ContentKey` projection of downloaded assets, the lineage landing, and the cloud-run result index are owned by `Store/blobstore`, `Version/provenance`, and `Query/cache`, while the job-dispatch policy is owned by `Rasm.Compute`.
- Dependency isolation: the SDK's `LBT.RestSharp`/`LBT.Newtonsoft.Json`/`Microsoft.Data.Sqlite` closure is sidecar-only and never crosses into the in-Rhino assembly; the forks are distinct package ids that never collide with the folder's `Newtonsoft.Json`/STJ rails.
- Object-store lane: the artifact byte transfer is owned by `api-objectstore.md` (`AWSSDK.S3`/`Minio`) at the `Store/blobstore` owner; this page records only that `ArtifactsApi.CreateArtifactAsync` returns a presigned `S3UploadRequest`.
- Compute-route boundary: `Rasm.Compute` owns the recipe-run dispatch policy; this catalog owns only the SDK transport surface and its three durable Persistence landings (`Store/blobstore`, `Version/provenance`, `Query/cache`).
