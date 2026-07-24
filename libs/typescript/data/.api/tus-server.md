# [TS_DATA_API_TUS_SERVER]

`@tus/server` owns tus resumable-upload protocol conformance as one `Server` over a pluggable `DataStore`: POST creates, HEAD answers `Upload-Offset`, PATCH appends from the verified offset, DELETE terminates, and resume is offset arithmetic against the store, never a re-trusted byte. `onUploadCreate` admits and `onUploadFinish` finalizes at the rail's seams; `handle` serves node and `handleWeb` serves any fetch-shaped runtime from one instance.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/server`
- package: `@tus/server` (MIT)
- module: ESM, one root export (`dist/index.js`, `dist/*.d.ts`)
- runtime: server plane — `handle` binds node, `handleWeb` binds any fetch-shaped runtime (Bun, Workers); the browser leg is `tus-js-client` on the ui branch
- backing: `srvx` mints the cross-runtime `ServerRequest` shape; `@tus/utils` re-exports whole (`Upload`, `DataStore`, `Locker`, `KvStore`, `EVENTS`, `CancellationContext`)
- rail: the `object/stream` resume rail, Effect-wrapped at the seam (`Effect.tryPromise` per dispatch)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the server, its options record, and the hook seams

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `Server` (`constructor({ datastore, path, ... })`)         | server         | one instance per staging band, held as a scoped service    |
|  [02]   | `.path` / `.relativeLocation` / `.respectForwardedHeaders` | routing        | the mount route; proxy-aware `Location` derivation         |
|  [03]   | `.maxSize` (`number \| (req, uploadId) => number`)         | bound          | admission ceiling — per-request fn reads caller quota      |
|  [04]   | `.namingFunction`                                          | identity       | upload-id mint — the rail names uploads into staging       |
|  [05]   | `.generateUrl` / `.getFileIdFromRequest`                   | identity       | `Location` URL derivation, id extraction from the request  |
|  [06]   | `.onUploadCreate`                                          | hook           | admission seam — metadata validation before create         |
|  [07]   | `.onUploadFinish`                                          | hook           | finalize seam — the content-address fold + receipt reply   |
|  [08]   | `.onIncomingRequest(req, uploadId)` / `.onResponseError`   | hook           | per-request gate (auth handoff), error-mapping observation |
|  [09]   | `.locker` / `MemoryLocker` / `Locker`                      | lock           | exclusive PATCH access; `MemoryLocker` default             |
|  [10]   | `.lockDrainTimeout`                                        | policy         | lock-cleanup budget                                        |
|  [11]   | `.disableTerminationForFinishedUploads`                    | policy         | DELETE posture on finished uploads                         |
|  [12]   | `.postReceiveInterval`                                     | policy         | progress-event cadence                                     |
|  [13]   | `Upload`                                                   | model          | the upload record every hook and event receives            |
|  [14]   | `DataStore`                                                | store contract | the storage port `@tus/s3-store` implements                |
|  [15]   | `EVENTS`                                                   | events         | EventEmitter lifecycle taps beside the hook seams          |

- `Server(WithOptional<ServerOptions, "locker"> & { datastore })`: `locker` defaults, `datastore` mandatory; `.onUploadCreate` returns `{ metadata? }`, `.onUploadFinish` returns `{ status_code?, headers?, body? }`.
- `[Upload]`: `id` `size?` `offset` `metadata?` `storage?` `creation_date?` `sizeIsDeferred`.
- `[DataStore]`: `create` `write` `getUpload` `remove` `declareUploadLength` `deleteExpired` `getExpiration`.
- `[EVENTS]`: `POST_CREATE` `POST_RECEIVE` `POST_FINISH` `POST_TERMINATE`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serving and lifecycle under Effect

`handle(IncomingMessage, ServerResponse) -> Promise<void>` serves node, `handleWeb(Request) -> Promise<Response>` serves any fetch-shaped runtime, and `cleanUpExpiredUploads() -> Promise<number>` sweeps expired staging; the hooks anchor the rail's folds, so tus internals stay invisible past this seam.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `new Server({ datastore, ...options })`                    | ctor     | one staging-band server; hooks close over the rail's folds |
|  [02]   | `server.handle(req, res)`                                  | instance | the node serving row mounts this under its route           |
|  [03]   | `server.handleWeb(req)`                                    | instance | Bun/Workers/`toWebHandler` — one server, both shapes       |
|  [04]   | `server.cleanUpExpiredUploads()`                           | instance | scheduled sweep of expired staging uploads                 |
|  [05]   | `server.on(EVENTS.POST_FINISH, (req, res, upload) => ...)` | instance | observability beside the finish hook, not the seam         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a mismatched `Upload-Offset` is a protocol refusal: PATCH appends only at the verified offset and no byte re-trusts, so any failure resumes with one HEAD and one PATCH from that offset.
- `DataStore` is the abstract port and `@tus/s3-store` the composed implementation: the server owns protocol conformance, the store owns bytes, and the rail subclasses neither.
- `onUploadCreate` validates and enriches metadata before creation; `onUploadFinish` runs after the final byte and before the reply, where the content-address finalize fold (chunk, digest, conditional re-put) attaches and its refusal aborts the reply typed.
- `draft-ietf-httpbis-resumable-upload` carries the same offset/complete semantics; on RFC the protocol row swaps under unchanged store and hooks.

[STACKING]:
- `@tus/s3-store`(`.api/tus-s3-store.md`): `S3Store` fills the `datastore` slot via `new S3Store({ s3ClientConfig: { bucket, ...clientConfig }, partSize })`, mapping tus offsets onto S3 multipart parts under the object plane's endpoint/credential `Config`.
- `effect`: each dispatch lifts through `Effect.tryPromise`, the server constructs inside `Effect.acquireRelease` at the owning service, and hook bodies run the rail's folds through the owning `ManagedRuntime` — a hook is a boundary adapter whose thrown refusal is the tus-conformant abort.
- `object/store`: `onUploadFinish`'s finalize fold reads the staged bytes, lands the content-addressed conditional put through the object plane's one client, then removes the staging upload — staging and content bands never share keys.
- ui branch: `tus-js-client` drives POST/PATCH/HEAD against this server over the tus wire itself, so the server owes it no catalog surface.

[LOCAL_ADMISSION]:
- construct one `Server` per staging band inside a scoped service, held across requests.
- attach admission and finalize logic through `onUploadCreate`/`onUploadFinish` alone, never forking the handler classes or reading store internals.
- wire `cleanUpExpiredUploads` on the maintenance cadence and serve through `handle`/`handleWeb` under the serving plane's route, so the process keeps its own boot edge.

[RAIL_LAW]:
- Package: `@tus/server`
- Owns: tus protocol conformance — creation, offset-verified PATCH resume, HEAD/DELETE, expiration sweep, the `ServerOptions` policy record, the hook seams, the `Locker` contract, and the re-exported `@tus/utils` model (`Upload`, `DataStore`, `EVENTS`)
- Accept: one scoped `Server` per staging band, hooks as the admission/finalize seams, `handleWeb` for fetch-shaped runtimes, `maxSize` as the admission ceiling, `MemoryLocker` on a single node
- Reject: per-request server construction, handler subclassing, `listen()` inside library code, finalize logic outside `onUploadFinish`, a staging band without an expiration sweep
