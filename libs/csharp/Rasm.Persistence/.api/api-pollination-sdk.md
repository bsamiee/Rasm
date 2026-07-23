# [RASM_PERSISTENCE_API_POLLINATION_SDK]

`PollinationSDK` owns the Pollination cloud compute transport: the OpenAPI `*Api` REST clients, the `Client.Configuration` token-auth surface, and the `Wrapper` job/run/asset orchestration. Its durable result half lands across three Persistence owners — artifact bytes at `Store/blobstore`, lineage at `Version/provenance`, the run index at `Query/cache#ArtifactKind.CloudRun` — while submission and watch route to `Rasm.Compute`. Sidecar isolation binds it outside-Rhino behind the vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` fork closure and a local `Microsoft.Data.Sqlite` cache, never loaded by the plugin assembly.

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

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new JobInfo(Job)` / `new JobInfo(RecipeInterface)`            | ctor     | the job descriptor                |
|  [02]   | `JobInfo.RunJobAsync() -> Task<ScheduledJobInfo>`              | instance | upload assets then submit         |
|  [03]   | `JobInfo.UploadJobAssetsAsync() -> Task<Job>`                  | instance | upload input artifacts pre-submit |
|  [04]   | `JobRunner.RunOnCloudAsync(Project) -> Task<CloudJob>`         | instance | submit a job to a project         |
|  [05]   | `ScheduledJobInfo.WatchJobStatusAsync() -> Task<string>`       | instance | poll to a terminal status         |
|  [06]   | `ScheduledJobInfo.DeleteAsync() -> Task<bool>`                 | instance | delete the scheduled job          |
|  [07]   | `RunInfo.DownloadRunAssetsAsync() -> Task<List<RunAssetBase>>` | instance | pull result assets, cache-aware   |

Every async method's tail is `(…, Action<string> progressReporting = null, CancellationToken = default)`; the run handle pulls assets back for the `Version/provenance` owner to land content-keyed.

[ENTRYPOINT_SCOPE]: low-level REST over `JobsApi`/`RunsApi`/`ProjectsApi`/`ArtifactsApi`

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `JobsApi.CreateJobAsync(Job) -> Task<CreatedContent>`                   | instance | submit a job to a project       |
|  [02]   | `JobsApi.GetJobAsync(string) -> Task<CloudJob>`                         | instance | poll one job                    |
|  [03]   | `JobsApi.ListJobsAsync(…) -> Task<CloudJobList>`                        | instance | page and filter jobs            |
|  [04]   | `RunsApi.GetRunAsync(string) -> Task<Run>`                              | instance | run state                       |
|  [05]   | `RunsApi.GetRunOutputAsync(string, string) -> Task<object>`             | instance | named run output                |
|  [06]   | `RunsApi.DownloadRunArtifactAsync(string, string) -> Task<object>`      | instance | fetch a run artifact by path    |
|  [07]   | `ProjectsApi.GetProjectAsync() -> Task<Project>`                        | instance | project transport               |
|  [08]   | `ProjectsApi.CreateProjectAsync(ProjectCreate) -> Task<CreatedContent>` | instance | create a project (drops `name`) |
|  [09]   | `ArtifactsApi.CreateArtifactAsync(KeyRequest) -> Task<S3UploadRequest>` | instance | presigned S3 upload request     |
|  [10]   | `ArtifactsApi.DownloadArtifactAsync(string) -> Task<object>`            | instance | fetch an artifact by path       |
|  [11]   | `ArtifactsApi.ListArtifactsAsync(List<string>) -> Task<FileMetaList>`   | instance | list artifacts                  |

Each op pairs a model-returning `*Async` (throws `ApiException`) with a `*WithHttpInfoAsync` returning `ApiResponse<T>`, over the shared skeleton `(string owner, string name, …, string authorization = null, string xPollinationToken = null, CancellationToken = default)`. `ListJobsAsync` filters are `List<string> ids`, `JobStatusEnum? status`, `DateTime? createdAfter`, `DateTime? createdBefore`, `int? page`, `int? perPage`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `LBT.RestSharp` (HTTP) and `LBT.Newtonsoft.Json` (JSON) carry distinct package ids from the folder's `Newtonsoft.Json` and its System.Text.Json rails, so the vendored RestSharp-106 + Newtonsoft-fork closure never collides.
- `Microsoft.Data.Sqlite` is touched only by the `Wrapper.LocalDatabase` cache through the ADO.NET `SqliteConnection`/`SqliteCommand` surface; the `*Api` REST layer never references it.
- Token auth is connection input the app root hands over (`Configuration.AddDefaultHeader`/`TokenRepo`), never a fence member.

[STACKING]:
- `api-objectstore.md`(`Store/blobstore`): `ArtifactsApi.CreateArtifactAsync` returns an `S3UploadRequest` presigned PUT and the `*Download*` ops resolve S3-backed assets, so the byte transfer rides the folder's object-store owner (`AWSSDK.S3`/`Minio`) on the same S3 plane; a downloaded `RunAsset` lands content-keyed (`XxHash128`) through that same body bridge, never a second HTTP uploader.
- `Rasm.Compute`: routes the job-submission and watch half to the cloud-run dispatch owner — this catalog owns the SDK transport surface, the recipe-run policy is a Compute concern.
- within-lib: a `Run` result and its `RunOutputAsset`s land at `Version/provenance` (lineage) and `Query/cache#ArtifactKind.CloudRun` (result index), so a completed cloud run becomes a content-addressed, lineage-tracked artifact set.

[LOCAL_ADMISSION]:
- No in-Rhino plugin assembly admits `PollinationSDK` or its RestSharp-106/Newtonsoft-fork closure; the SDK and its SQLite cache load only on the cloud-run sidecar.

[RAIL_LAW]:
- Package: `PollinationSDK` (MIT)
- Owns: the Pollination cloud compute transport — the `*Api` REST clients, `Configuration`/`TokenRepo` auth, `Wrapper` job/run/asset orchestration, and the model DTOs
- Accept: a recipe-run job submitted to a Pollination project, watched to completion, and its result assets pulled back — the durable half projected to `Store/blobstore`, `Version/provenance`, and `Query/cache#ArtifactKind.CloudRun` while the dispatch half routes to `Rasm.Compute`, artifact bytes transferred via the object-store owner
- Reject: loading the SDK or its forks in the in-Rhino assembly; a second S3 uploader where `api-objectstore.md` owns the object plane; a hand-rolled token store where `Configuration`/`TokenRepo` carry auth; treating the netstandard2.0 floor as a net8+ surface
