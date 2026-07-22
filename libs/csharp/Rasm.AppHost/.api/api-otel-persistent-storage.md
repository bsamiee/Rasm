# [RASM_APPHOST_API_OTEL_PERSISTENT_STORAGE]

`OpenTelemetry.PersistentStorage.FileSystem` persists failed OTLP export batches as on-disk `FileBlob` files under a bounded `FileBlobProvider`, holding signal tails across network loss, crash, and ALC unload until the collector returns. Its `PersistentBlobProvider`/`PersistentBlob` base contract restores transitively from `OpenTelemetry.PersistentStorage.Abstractions` and owns the create, lease, read, write, and delete verbs the offline drain replays.

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

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------- | :------------ | :------------------------------------ |
|  [01]   | `FileBlobProvider` | blob provider | bounded on-disk queue, disposable     |
|  [02]   | `FileBlob`         | blob          | one stored batch; `FullPath` names it |

[PUBLIC_TYPE_SCOPE]: abstraction base contract
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :----------------------- | :------------- | :------------------------------------ |
|  [01]   | `PersistentBlobProvider` | abstract owner | create, get, enumerate over any store |
|  [02]   | `PersistentBlob`         | abstract blob  | lease, read, write, delete one batch  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction and lifetime
- namespace: `OpenTelemetry.PersistentStorage.FileSystem`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `FileBlobProvider(string, long, int, long, int)` | ctor     | bounded on-disk queue rooted at path      |
|  [02]   | `FileBlobProvider.Dispose()`                     | instance | stops maintenance, releases the directory |
|  [03]   | `FileBlob(string)`                               | ctor     | binds a handle to an existing file        |
|  [04]   | `FileBlob.FullPath`                              | property | absolute path of the backing file         |

- `FileBlobProvider`: `path` required; `maxSizeInBytes` 52428800, `maintenancePeriodInMilliseconds` 120000, `retentionPeriodInMilliseconds` 172800000, `writeTimeoutInMilliseconds` 60000 default.

[ENTRYPOINT_SCOPE]: blob-provider operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlobProvider`

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `TryCreateBlob(ReadOnlySpan<byte>, out PersistentBlob?)`      | instance | allocation-free create over a span        |
|  [02]   | `TryCreateBlob(ReadOnlySpan<byte>, int, out PersistentBlob?)` | instance | span create with lease                    |
|  [03]   | `TryGetBlob(out PersistentBlob?)`                             | instance | selects one unleased `.blob` handle       |
|  [04]   | `GetBlobs()`                                                  | instance | unleased `PersistentBlob` handle sequence |

[ENTRYPOINT_SCOPE]: blob operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlob`

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `TryLease(int)`                     | instance | atomically renames `.blob` to `.lock`       |
|  [02]   | `TryRead(out byte[]?)`              | instance | reads bytes after a caller-acquired lease   |
|  [03]   | `TryWrite(ReadOnlySpan<byte>, int)` | instance | allocation-free write with optional lease   |
|  [04]   | `TryDelete()`                       | instance | removes the selected or leased backing file |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `FileBlobProvider` extends `PersistentBlobProvider` and implements `IDisposable`; `FileBlob` extends `PersistentBlob`.
- Blob lifecycle rides the file extension: `.tmp` during write, `.blob` when durable, `.lock` while leased with the lease-expiry timestamp appended.
- Stored file name is `datetimestamp(ISO 8601)-GUID`; a background pass at `maintenancePeriodInMilliseconds` runs retention, maintenance, and lease expiry.
- Maintenance reclaims expired `.blob` files, clears timed-out `.tmp` files, promotes expired `.lock` back to `.blob`, and recomputes free space against `maxSizeInBytes`.
- A new blob drops silently once folder size reaches `maxSizeInBytes`, reporting through the `OpenTelemetry-PersistentStorage-FileSystem` EventSource.
- Emitted bytes are redacted before storage, so the store applies no encryption and directory access control is a deployment responsibility.

[STACKING]:
- `OpenTelemetry.Exporter.OpenTelemetryProtocol`(`.api/api-otel-exporter.md`): a failed OTLP export batch persists through the exporter transmission-retry path as a `FileBlob` and replays on reconnection, the drain band flushing what the wire takes while the store holds the rest.
- `SignalGovernance.Govern` (hosted roots) and `PluginTelemetryHost.Open` (per-ALC capsules) construct one provider per transmission owner and drive its lease-gated drain.

[LOCAL_ADMISSION]:
- Construct one `FileBlobProvider` per OTLP transmission owner at the composition root, rooted at a per-owner writable queue directory, disposed with the owner.
- Drive every constructor policy value from the governance table.

[RAIL_LAW]:
- Package: `OpenTelemetry.PersistentStorage.FileSystem`
- Owns: on-disk durability for OTLP export batches — offline queue, replay, retention
- Accept: composition-root `FileBlobProvider` construction; span-based `TryCreateBlob`; drain order `GetBlobs`/`TryGetBlob` → `TryLease` → `TryRead` → successful export → `TryDelete`
- Reject: hardcoded queue policy at call sites; reading a blob without a lease; storing unredacted payload bytes
