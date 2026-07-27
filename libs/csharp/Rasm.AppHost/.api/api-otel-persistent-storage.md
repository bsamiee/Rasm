# [RASM_APPHOST_API_OTEL_PERSISTENT_STORAGE]

`OpenTelemetry.PersistentStorage.FileSystem` persists failed OTLP export batches as on-disk `FileBlob` files under a bounded `FileBlobProvider`, holding signal tails across network loss, crash, and ALC unload until the collector returns. Its `PersistentBlobProvider`/`PersistentBlob` base contract restores transitively from `OpenTelemetry.PersistentStorage.Abstractions` and owns the create, lease, read, write, and delete verbs an offline drain replays.

Both public verb families run span-first: `TryCreateBlob(ReadOnlySpan<byte>, …)` and `TryWrite(ReadOnlySpan<byte>, int)` are the live overloads, and each `byte[]` twin carries `[Obsolete]` naming its span replacement and a removal-in-next-major notice. Compositions reaching a `byte[]` overload take a build warning today and a break at the next major.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.PersistentStorage.FileSystem`
- package: `OpenTelemetry.PersistentStorage.FileSystem`
- assembly: `OpenTelemetry.PersistentStorage.FileSystem`
- namespace: `OpenTelemetry.PersistentStorage.FileSystem`
- asset: runtime library
- rail: telemetry

[PACKAGE_SURFACE]: `OpenTelemetry.PersistentStorage.Abstractions`
- package: `OpenTelemetry.PersistentStorage.Abstractions`
- assembly: `OpenTelemetry.PersistentStorage.Abstractions`
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- asset: transitive base contract restored with the file-system provider
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file-system provider family
- namespace: `OpenTelemetry.PersistentStorage.FileSystem`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `FileBlobProvider` | blob provider | bounded on-disk queue over a directory, `IDisposable`             |
|  [02]   | `FileBlob`         | blob          | one stored batch; `FullPath` names the live extension-tagged file |

[PUBLIC_TYPE_SCOPE]: abstraction base contract
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                       |
| :-----: | :----------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `PersistentBlobProvider` | abstract owner | public create/get/enumerate wrappers over `protected On*` virtuals |
|  [02]   | `PersistentBlob`         | abstract blob  | public lease/read/write/delete wrappers over `protected On*`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction and lifetime
- namespace: `OpenTelemetry.PersistentStorage.FileSystem`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `FileBlobProvider(string, long, int, long, int)` | ctor     | bounded on-disk queue rooted at path      |
|  [02]   | `FileBlobProvider.Dispose()`                     | instance | stops maintenance, releases the directory |
|  [03]   | `FileBlob(string)`                               | ctor     | binds a handle to an existing file        |
|  [04]   | `FileBlob.FullPath`                              | property | absolute path of the backing file         |

- `FileBlobProvider(string path, long maxSizeInBytes = 52428800, int maintenancePeriodInMilliseconds = 120000, long retentionPeriodInMilliseconds = 172800000, int writeTimeoutInMilliseconds = 60000)` — `path` is guarded non-null and `Directory.CreateDirectory` runs at construction, so the queue directory materializes eagerly and an unwritable path faults at composition rather than at first drop.
- `FileBlob(string fullPath)` binds a path with no size tracker, so a blob minted this way never decrements the provider's free-space accounting; provider-minted blobs carry the tracker.

[ENTRYPOINT_SCOPE]: blob-provider operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlobProvider`

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `TryCreateBlob(ReadOnlySpan<byte>, out PersistentBlob?)`      | instance | allocation-free create over a span        |
|  [02]   | `TryCreateBlob(ReadOnlySpan<byte>, int, out PersistentBlob?)` | instance | span create with lease                    |
|  [03]   | `TryGetBlob(out PersistentBlob?)`                             | instance | selects one unleased `.blob` handle       |
|  [04]   | `GetBlobs()`                                                  | instance | unleased `PersistentBlob` handle sequence |
|  [05]   | `TryCreateBlob(byte[], out PersistentBlob?)`                  | obsolete | array twin of row [01]                    |
|  [06]   | `TryCreateBlob(byte[], int, out PersistentBlob?)`             | obsolete | array twin of row [02]                    |

- Every public verb wraps its `protected On*` counterpart in a catch-all that logs to the `OpenTelemetry-PersistentStorage-Abstractions` EventSource and returns `false`, so a store fault surfaces as a refusal boolean and never as an exception at the call site.
- Span `OnTryCreateBlob` virtuals carry a base implementation; `FileBlobProvider` overrides all four, so span creates take the file path with no array copy.

[ENTRYPOINT_SCOPE]: blob operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlob`

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `TryLease(int)`                         | instance | atomically renames `.blob` to `.lock`       |
|  [02]   | `TryRead(out byte[]?)`                  | instance | reads bytes after a caller-acquired lease   |
|  [03]   | `TryWrite(ReadOnlySpan<byte>, int = 0)` | instance | allocation-free write with optional lease   |
|  [04]   | `TryDelete()`                           | instance | removes the selected or leased backing file |
|  [05]   | `TryWrite(byte[], int = 0)`             | obsolete | array twin of row [03]                      |

- `TryRead` returns the whole batch as one `byte[]`; no span read exists, so a drain pays one array per replayed blob and bounds its own concurrency accordingly.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `FileBlobProvider` extends `PersistentBlobProvider` and implements `IDisposable`; `FileBlob` extends `PersistentBlob`.
- Blob lifecycle rides the file extension: `.tmp` during write, `.blob` when durable, `.lock` while leased with `@yyyy-MM-ddTHHmmss.fffffffZ` appended as the lease deadline.
- Stored file name is `yyyy-MM-ddTHHmmss.fffffffZ-<guid:N>.blob`; the timestamp parses back out of the name, so retention and lease expiry read the clock off the path with no index.
- One background `Timer` at `maintenancePeriodInMilliseconds` reclaims expired `.blob` files, clears timed-out `.tmp` files, promotes expired `.lock` back to `.blob`, and recomputes free space against `maxSizeInBytes`.
- `GetBlobs` enumerates `*.blob` at the top directory alone, newest name first, filtering anything older than the retention deadline; nested directories are invisible to the drain.
- New blobs drop silently once folder size reaches `maxSizeInBytes`, reporting through the `OpenTelemetry-PersistentStorage-FileSystem` EventSource.
- Neither tier publishes a depth or size accessor: `FileBlobProvider` exposes construction, `Dispose`, and the four `protected override On*` verbs alone, its `DirectoryPath` field is `internal`, and the base contract adds only the create/get/enumerate wrappers — so queued blob count and folder bytes derive from an O(n) `GetBlobs` walk or from a consumer's own disposition accounting, never from the package.
- Emitted bytes are redacted before storage, so the store applies no encryption and directory access control is a deployment responsibility.

[STACKING]:
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`libs/csharp/.api/api-opentelemetry-exporter-otlp.md`): the exporter carries its OWN embedded copy of this package's types and drives them from `internal sealed OtlpExporterPersistentStorageTransmissionHandler`, which constructs `new FileBlobProvider(storagePath, 52428800, 120000, 172800000)` itself and retries on a background thread at a hardcoded 60 s interval. No public seam accepts a provider — the provider-taking constructor is `internal` — so the exporter's disk retry is armed only by `OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY=disk` beside `OTEL_DOTNET_EXPERIMENTAL_OTLP_DISK_RETRY_DIRECTORY_PATH`, and selecting `disk` without the path throws `NotSupportedException` at exporter construction.
- Offline custody therefore reaches the transport, never the exporter's persistence: `OtlpExporterOptions.HttpClientFactory` installs a `DelegatingHandler` over a directly-constructed `FileBlobProvider`, so queue policy, replay cadence, and refusal receipts stay branch-typed. Exactly one persistence owner arms per exporter — the exporter env pair stays unset wherever the handler leg is selected. That handler overrides the SYNCHRONOUS `Send` beside `SendAsync`, because the http/protobuf export client sends synchronously, and it owns no provider lifetime, because the exporter never disposes the client it was handed.
- `Rasm.AppHost/Observability/telemetry#SIGNAL_GOVERNANCE`: `OtlpOfflinePolicy.Open` mints ONE provider per exported-signal directory off `ProfileRoots.QueueRoot` as one frozen set at composition — early enough that an unwritable root faults at admission and late enough that nothing opens twice — `OtlpOfflineQueue` owns the accept and the lease-gated drain, `PersistentOtlpHandler` installs through `SignalGovernance.Durable`, and `TelemetryComposition.Dispose` releases every provider at the telemetry drain band; `Rasm.AppHost/Observability/instruments#PROVIDER_LIFETIME` `PluginTelemetryHost.Open` opens no queue, so an ALC capsule's failed batches die with the capsule rather than outliving their only replay provider.
- Stored blobs carry the request BODY alone: the drain rebuilds each replay off the live request that just succeeded, so endpoint and credential headers never reach disk and a rotated token applies to the whole tail.

[LOCAL_ADMISSION]:
- Construct one `FileBlobProvider` per OTLP transmission owner at the composition root, rooted at a per-owner writable queue directory, disposed with the owner.
- Drive every constructor policy value from the governance table.
- Reach the span overloads alone; the `byte[]` twins are obsolete and removed at the next major.

[RAIL_LAW]:
- Package: `OpenTelemetry.PersistentStorage.FileSystem`
- Owns: on-disk durability for OTLP export batches — offline queue, replay, retention
- Accept: composition-root `FileBlobProvider` construction; span `TryCreateBlob`/`TryWrite`; drain order `GetBlobs`/`TryGetBlob` → `TryLease` → `TryRead` → successful export → `TryDelete`
- Reject: hardcoded queue policy at call sites; reading a blob without a lease; storing unredacted payload bytes; obsolete `byte[]` overloads; arming the exporter's own disk retry beside a branch-owned queue
