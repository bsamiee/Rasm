# [RASM_APPHOST_API_OTEL_PERSISTENT_STORAGE]

`OpenTelemetry.PersistentStorage.FileSystem` supplies a bounded file-system `FileBlobProvider` that persists failed OTLP export batches as on-disk `FileBlob` files, so a host losing its collector keeps signal tails across network loss, crash, and ALC unload. Its `PersistentBlob`/`PersistentBlobProvider` base contract ships in the transitively-restored `OpenTelemetry.PersistentStorage.Abstractions` and owns the create, lease, read, write, and delete verbs the offline drain replays through.

[APP_ROOT_RESERVED]: `[TELEMETRY_OFFLINE_SPINE]` — a persistent blob provider is a composition-root instantiation only, constructed once per OTLP transmission owner (`SignalGovernance.Govern` for hosted roots, `PluginTelemetryHost.Open` for per-ALC capsules) and disposed with it. Emitted bytes are already redacted, so stored payload is never unclassified.

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
- rail: telemetry

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :----------------- | :------------ | :---------------------------------------- |
|  [01]   | `FileBlobProvider` | blob provider | `PersistentBlobProvider`, `IDisposable`   |
|  [02]   | `FileBlob`         | blob          | `PersistentBlob`; `FullPath` on-disk file |

[PUBLIC_TYPE_SCOPE]: abstraction base contract
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- rail: telemetry

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                                    |
| :-----: | :----------------------- | :------------- | :---------------------------------------- |
|  [01]   | `PersistentBlobProvider` | abstract owner | create, get, enumerate over any backing   |
|  [02]   | `PersistentBlob`         | abstract blob  | lease, read, write, delete a stored batch |

## [03]-[ENTRYPOINTS]

Signatures are type-only overload shapes; namespace and owning type come from the scope. Constructor policy defaults are the [DEFAULTS] rows.

[ENTRYPOINT_SCOPE]: provider construction and lifetime
- namespace: `OpenTelemetry.PersistentStorage.FileSystem`
- rail: telemetry

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :----------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `FileBlobProvider(string, long, int, long, int)` | ctor           | bounded on-disk queue rooted at path      |
|  [02]   | `FileBlobProvider.Dispose()`                     | disposal       | stops maintenance, releases the directory |
|  [03]   | `FileBlob(string)`                               | ctor           | binds a blob handle to an existing file   |
|  [04]   | `FileBlob.FullPath`                              | string prop    | absolute path of the backing file         |

[ENTRYPOINT_SCOPE]: blob-provider operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlobProvider`
- rail: telemetry

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]    | [RAIL]                                    |
| :-----: | :------------------------------------------------------------ | :---------------- | :---------------------------------------- |
|  [01]   | `TryCreateBlob(ReadOnlySpan<byte>, out PersistentBlob?)`      | span create       | allocation-free create over a span        |
|  [02]   | `TryCreateBlob(ReadOnlySpan<byte>, int, out PersistentBlob?)` | span create+lease | span create with lease                    |
|  [03]   | `TryGetBlob(out PersistentBlob?)`                             | select            | selects one unleased `.blob` handle       |
|  [04]   | `GetBlobs()`                                                  | enumerate         | unleased `PersistentBlob` handle sequence |

[ENTRYPOINT_SCOPE]: blob operations
- namespace: `OpenTelemetry.PersistentStorage.Abstractions`
- owner: `PersistentBlob`
- rail: telemetry

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :---------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `TryLease(int)`                     | lease          | atomically renames `.blob` to `.lock`       |
|  [02]   | `TryRead(out byte[]?)`              | read           | reads bytes after caller-acquired lease     |
|  [03]   | `TryWrite(ReadOnlySpan<byte>, int)` | span write     | allocation-free write with optional lease   |
|  [04]   | `TryDelete()`                       | delete         | removes the selected or leased backing file |

## [04]-[IMPLEMENTATION_LAW]

[DEFAULTS]: `FileBlobProvider` constructor policy
- `path` — queue directory, required, no default
- `maxSizeInBytes` — folder cap, default `52428800`
- `maintenancePeriodInMilliseconds` — maintenance interval, default `120000`
- `retentionPeriodInMilliseconds` — blob retention, default `172800000`
- `writeTimeoutInMilliseconds` — write timeout, default `60000`

[STORAGE_TOPOLOGY]:
- public namespace `OpenTelemetry.PersistentStorage.FileSystem` — `FileBlobProvider`, `FileBlob`
- base namespace `OpenTelemetry.PersistentStorage.Abstractions` — `PersistentBlobProvider`, `PersistentBlob`, restored transitively with the file-system package
- `FileBlobProvider` extends `PersistentBlobProvider` and implements `IDisposable`; `FileBlob` extends `PersistentBlob` and exposes `FullPath`
- file extension names the blob lifecycle: `.tmp` during write, `.blob` when durable, `.lock` while leased with the lease-expiry timestamp appended
- stored file name is `datetimestamp(ISO 8601)-GUID`; retention, maintenance, and lease expiry run on a background pass at `maintenancePeriodInMilliseconds`
- maintenance reclaims expired `.blob` files, clears timed-out `.tmp` files, promotes expired `.lock` back to `.blob`, and recomputes free space against `maxSizeInBytes`
- new blobs drop silently once folder size reaches `maxSizeInBytes`; a drop reports through the `OpenTelemetry-PersistentStorage-FileSystem` EventSource
- stored bytes are unencrypted and unprocessed; directory access control is a deployment responsibility

[LOCAL_ADMISSION]:
- Construct one `FileBlobProvider` per OTLP transmission owner at the composition root, rooted at a per-owner writable queue directory, disposed with the owner.
- Bind the provider to the OTLP exporter transmission-retry path so a failed batch persists as a blob and replays on reconnection; the drain band flushes what the wire takes and the store holds the rest.
- Drive every constructor policy value from the governance table, never hardcoded at the call site.
- Select through `GetBlobs()` or `TryGetBlob`, require `TryLease(leasePeriodInMilliseconds)` success, then call `TryRead`; call `TryDelete` only after the leased batch drains successfully.

[RAIL_LAW]:
- Package: `OpenTelemetry.PersistentStorage.FileSystem`
- Owns: on-disk durability for OTLP export batches — offline queue, replay, retention
- Accept: `FileBlobProvider` construction at the composition root; span-based `TryCreateBlob`; drain order `TryGetBlob` or `GetBlobs` → `TryLease` → `TryRead` → successful export → `TryDelete`
- Reject: hardcoded queue policy at call sites; reading a blob without a lease; storing unredacted payload bytes
