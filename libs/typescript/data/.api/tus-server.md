# [TS_DATA_API_TUS_SERVER]

`@tus/server` owns tus resumable-upload protocol conformance as one `Server` over a pluggable `DataStore`: POST creates, HEAD answers `Upload-Offset`, PATCH appends from the verified offset, DELETE terminates, and resume is offset arithmetic against the store, never a re-trusted byte. `onUploadCreate` admits and `onUploadFinish` finalizes at the rail's seams; `handle` serves node and `handleWeb` serves any fetch-shaped runtime from one instance.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/server`
- package: `@tus/server` (MIT)
- module: ESM, one root export (`dist/index.js`, `dist/*.d.ts`)
- runtime: server plane — `handle` binds node, `handleWeb` binds any fetch-shaped runtime (Bun, Workers); the browser leg is `tus-js-client` on the ui branch
- backing: `srvx` mints the cross-runtime `ServerRequest` shape; `@tus/utils` re-exports whole — `Upload`, `DataStore`, `Locker`/`Lock`/`RequestRelease`, `KvStore` with its `Memory`/`File`/`Redis`/`IoRedis` rows, the `Metadata` parse/stringify/validate namespace, `StreamSplitter`/`StreamLimiter`, `Uid`, `EVENTS`, `ERRORS`, `CancellationContext`
- rail: the `object/stream` resume rail, Effect-wrapped at the seam (`Effect.tryPromise` per dispatch)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the server, its options record, and the hook seams

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Server` (`constructor({ datastore, path, ... })`)           | server         | one instance per staging band, held as a scoped service |
|  [02]   | `.path` / `.relativeLocation` / `.respectForwardedHeaders`   | routing        | the mount route; proxy-aware `Location` derivation      |
|  [03]   | `.allowedOrigins` / `.allowedHeaders` / `.exposedHeaders`    | cors           | origin roster or predicate widening the CORS headers    |
|  [04]   | `.maxSize` (`number \| (req, uploadId) => number`)           | bound          | admission ceiling — per-request fn reads caller quota   |
|  [05]   | `.namingFunction` / `.generateUrl` / `.getFileIdFromRequest` | identity       | id mint, `Location` derivation, id extraction           |
|  [06]   | `.onUploadCreate` / `.onUploadFinish`                        | hook           | admission seam; the finalize fold and receipt reply     |
|  [07]   | `.onIncomingRequest(req, uploadId)`                          | hook           | per-request gate — the auth handoff                     |
|  [08]   | `.onResponseError`                                           | error map      | the reply pair every fault path passes through          |
|  [09]   | `.locker` / `MemoryLocker` / `Locker`                        | lock           | exclusive PATCH access; `MemoryLocker` default          |
|  [10]   | `.lockDrainTimeout` / `.postReceiveInterval`                 | policy         | lock-cleanup budget; progress-event cadence             |
|  [11]   | `.disableTerminationForFinishedUploads`                      | policy         | DELETE posture on finished uploads                      |
|  [12]   | `Upload`                                                     | model          | the upload record every hook and event receives         |
|  [13]   | `DataStore`                                                  | store contract | the storage port `@tus/s3-store` implements             |
|  [14]   | `ERRORS`                                                     | error roster   | frozen `{ status_code, body }` rows the map returns     |
|  [15]   | `EVENTS`                                                     | events         | EventEmitter lifecycle taps beside the hook seams       |
|  [16]   | `RouteHandler`                                               | route          | `(req) => Response` mounted by `server.get`             |

- `Server(WithOptional<ServerOptions, "locker"> & { datastore })`: `locker` defaults, `datastore` mandatory; `.onUploadCreate` returns `{ metadata? }`, `.onUploadFinish` returns `{ status_code?, headers?, body? }`.
- `onResponseError(req, err: Error | { status_code, body })` returns `{ status_code, body } | undefined` sync or async: a returned pair REPLACES the reply, `undefined` keeps the pair derived from the throw.
- `locker` accepts a `Locker`, a `Promise<Locker>`, or `(req) => Locker | Promise<Locker>`, so a per-request lock backend needs no wrapper.
- `[Upload]`: `id` `size?` `offset` `metadata?` `storage?` `creation_date?` `sizeIsDeferred`.
- `[DataStore]`: `create` `write` `getUpload` `remove` `declareUploadLength` `deleteExpired` `getExpiration` `extensions` `hasExtension`.
- `[ERRORS]` by status: 400 `ABORTED` `INVALID_TERMINATION` `INVALID_LENGTH` `INVALID_METADATA`; 403 `MISSING_OFFSET` `INVALID_CONTENT_TYPE`; 404 `FILE_NOT_FOUND`; 409 `INVALID_OFFSET`; 410 `FILE_NO_LONGER_EXISTS`; 413 `ERR_SIZE_EXCEEDED` `ERR_MAX_SIZE_EXCEEDED`; 500 `ERR_LOCK_TIMEOUT` `UNKNOWN_ERROR` `FILE_WRITE_ERROR`; 501 `UNSUPPORTED_CONCATENATION_EXTENSION` `UNSUPPORTED_CREATION_DEFER_LENGTH_EXTENSION` `UNSUPPORTED_EXPIRATION_EXTENSION`.
- `[EVENTS]`: `POST_CREATE(req, upload, url)` `POST_RECEIVE(req, upload)` `POST_FINISH(req, res, upload)` `POST_TERMINATE(req, res, id)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serving and lifecycle under Effect

`handle(IncomingMessage, ServerResponse) -> Promise<void>` serves node, `handleWeb(Request) -> Promise<Response>` serves any fetch-shaped runtime, and `cleanUpExpiredUploads() -> Promise<number>` sweeps expired staging; the hooks anchor the rail's folds, so tus internals stay invisible past this seam.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `new Server({ datastore, ...options })`                    | ctor     | one staging-band server; hooks close over the rail's folds |
|  [02]   | `server.handle(req, res)`                                  | instance | the node serving row mounts this under its route           |
|  [03]   | `server.handleWeb(req)`                                    | instance | Bun/Workers/`toWebHandler` — one server, both shapes       |
|  [04]   | `server.get(path, RouteHandler)`                           | instance | a sibling GET route served off the same mount              |
|  [05]   | `server.cleanUpExpiredUploads()`                           | instance | scheduled sweep of expired staging uploads                 |
|  [06]   | `server.on(EVENTS.POST_FINISH, (req, res, upload) => ...)` | instance | observability beside the finish hook, not the seam         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- mismatched `Upload-Offset` refuses at the protocol: PATCH appends only at the verified offset and no byte re-trusts, so any failure resumes with one HEAD and one PATCH from that offset.
- `DataStore` is the abstract port and `@tus/s3-store` the composed implementation: the server owns protocol conformance, the store owns bytes, and the rail subclasses neither.
- `onUploadCreate` validates and enriches metadata before creation; `onUploadFinish` runs after the final byte and before the reply, where the content-address finalize fold (chunk, digest, conditional re-put) attaches.
- Both hooks abort the request on throw, and the server reads `status_code`/`body` OFF the thrown value, falling back to `ERRORS.UNKNOWN_ERROR` with the message appended — so a typed refusal throws a `{ status_code, body }` carrier while a bare `Error` degrades to 500.
- `onResponseError` closes every fault path last, taking the thrown value and returning a replacement `{ status_code, body }` or `undefined` to keep the derived pair: this is the error-MAPPING seam, and observation alone is a return of `undefined`.
- `draft-ietf-httpbis-resumable-upload` carries the same offset/complete semantics; on RFC the protocol row swaps under unchanged store and hooks.
- Built-in id extraction refuses a traversal before the store ever sees it: an id carrying `/`, `\`, or a NUL, or a percent-encoding that will not decode, answers no id at all, so a `DataStore` keyed on the raw id is safe without its own sanitizer.
- `.getFileIdFromRequest` owns extraction WHOLE when it is set, and a request whose URL misses the mount route reaches it with no path argument — so a custom extractor addressing uploads off a header or a signed token serves routes the built-in pattern never matches, and it owes the same traversal refusal the built-in makes.
- `.exposedHeaders` widens the protocol's own `Access-Control-Expose-Headers` roster; leaving it empty publishes that roster verbatim rather than a widened one.
- Creation-with-upload replies with the PATCH leg's headers over the create leg's, so an `onUploadFinish` header set on the same request wins the merge.

[STACKING]:
- `@tus/s3-store`(`.api/tus-s3-store.md`): `S3Store` fills the `datastore` slot via `new S3Store({ s3ClientConfig: { bucket, ...clientConfig }, partSize })`, mapping tus offsets onto S3 multipart parts under the object plane's endpoint/credential `Config`.
- `effect`: each dispatch lifts through `Effect.tryPromise`, the server constructs inside `Effect.acquireRelease` at the owning service, and hook bodies run the rail's folds through the owning `ManagedRuntime` — a hook is a boundary adapter whose thrown refusal is the tus-conformant abort, so the rail's tagged faults project onto `ERRORS` rows inside `onResponseError` and never leak a stack into the reply body.
- `object/store`: `onUploadFinish`'s finalize fold reads the staged bytes, lands the content-addressed conditional put through the object plane's one client, then removes the staging upload — staging and content bands never share keys.
- ui branch: `tus-js-client` drives POST/PATCH/HEAD against this server over the tus wire itself, so the server owes it no catalog surface.

[LOCAL_ADMISSION]:
- construct one `Server` per staging band inside a scoped service, held across requests.
- attach admission and finalize logic through `onUploadCreate`/`onUploadFinish` alone, never forking the handler classes or reading store internals.
- throw a `{ status_code, body }` carrier out of a hook and map every remaining rail fault onto an `ERRORS` row inside `onResponseError`, so no reply falls through to the 500 `UNKNOWN_ERROR` default.
- wire `cleanUpExpiredUploads` on the maintenance cadence and serve through `handle`/`handleWeb` under the serving plane's route, so the process keeps its own boot edge.

[RAIL_LAW]:
- Package: `@tus/server`
- Owns: tus protocol conformance — creation, offset-verified PATCH resume, HEAD/DELETE, expiration sweep, the `ServerOptions` policy record with its CORS admission rows, the hook seams, `onResponseError` as the fault-to-reply map, the `Locker` contract, and the re-exported `@tus/utils` model (`Upload`, `DataStore`, `ERRORS`, `EVENTS`, `Metadata`)
- Accept: one scoped `Server` per staging band, hooks as the admission/finalize seams, `{ status_code, body }` carriers as the refusal shape, `ERRORS` rows as the reply vocabulary, `handleWeb` for fetch-shaped runtimes, `maxSize` as the admission ceiling, `MemoryLocker` on a single node
- Reject: per-request server construction, handler subclassing, `listen()` inside library code, finalize logic outside `onUploadFinish`, a bare `Error` thrown from a hook, a hand-minted status/body pair where an `ERRORS` row exists, a staging band without an expiration sweep, an id sanitizer re-implemented over the built-in extraction, a `.getFileIdFromRequest` returning an id it never checked for traversal
