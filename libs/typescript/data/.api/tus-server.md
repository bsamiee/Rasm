# [TS_DATA_API_TUS_SERVER]

`@tus/server` implements tus catalog-bound — the deployed resumable-upload protocol — as one `Server` class over a pluggable `DataStore`: POST creates an upload, HEAD answers `Upload-Offset`, PATCH appends from a verified offset, DELETE terminates, and the client never re-trusts a byte because resume is offset arithmetic against the store. The `object/stream` rail composes it with `@tus/s3-store` (`.api/tus-s3-store.md`) so tus offsets map onto S3 multipart parts, and the lifecycle hooks (`onUploadCreate`/`onUploadFinish`) are the rail's admission and finalize seams — metadata validation at create, the content-address finalize fold at finish. The server is request-shape dual: `handle(req, res)` serves node, `handleWeb(request): Promise<Response>` serves any fetch-shaped runtime, so the runtime binding is a serving-plane row, never a fork here. The IETF RUFH draft (`draft-ietf-httpbis-resumable-upload`) absorbs tus core with the same offset/complete semantics — the protocol row swaps on RFC with the store and hooks unchanged.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/server`
- package: `@tus/server`
- license: `MIT`
- engine: `node >= catalog`; `handleWeb` runs on any fetch-shaped runtime (Bun, Workers)
- backing: `srvx` (the cross-runtime `ServerRequest` shape), `@tus/utils` (re-exported whole — `Upload`, `DataStore`, `Locker`, `KvStore`, `EVENTS`, `CancellationContext`)
- module format: ESM (`dist/index.js`, `dist/*.d.ts`); one root export
- runtime: server plane only — the browser leg is `tus-js-client`, a ui-branch concern
- rail: the `object/stream` resume rail — the server is Effect-wrapped at the seam (`Effect.tryPromise` per dispatch), never leaked into domain code

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the server, its options, and the hook seams
- rail: object/stream
- `ServerOptions` is one options record: routing (`path`, `generateUrl`, `getFileIdFromRequest`), naming (`namingFunction`), bounds (`maxSize`), lifecycle hooks, and the `locker` guarding concurrent PATCHes on one upload. The constructor takes `WithOptional<ServerOptions, "locker"> & { datastore }` — the locker defaults, the datastore is mandatory.
- Every option row below is a `ServerOptions` field. `onUploadCreate(req, upload)` returns `{ metadata? }`, `onUploadFinish(req, upload)` returns `{ status_code?, headers?, body? }`. `Upload` carries `id`/`size?`/`offset`/`metadata?`/`storage?`/`creation_date?`/`sizeIsDeferred`; `DataStore` is abstract over `create`/`write`/`getUpload`/`remove`/`declareUploadLength`/`deleteExpired`; `EVENTS` is `POST_CREATE`/`POST_RECEIVE`/`POST_FINISH`/`POST_TERMINATE`.

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                        |
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serving and lifecycle under Effect
- rail: object/stream
- The server is a scoped resource whose dispatch members lift per call; the hooks are where the rail's own folds attach, so tus internals stay invisible past this seam. `handle(req: http.IncomingMessage, res: http.ServerResponse)` returns `Promise<void>`, `handleWeb(req: Request)` and `cleanUpExpiredUploads()` return `Promise<Response>`/`Promise<number>`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                        |
| :-----: | :--------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `new Server({ datastore, ...options })`                    | construct      | one staging-band server; hooks close over the rail's folds |
|  [02]   | `server.handle(req, res)`                                  | node dispatch  | the node serving row mounts this under its route           |
|  [03]   | `server.handleWeb(req)`                                    | fetch dispatch | Bun/Workers/`toWebHandler` — one server, both shapes       |
|  [04]   | `server.cleanUpExpiredUploads()`                           | maintenance    | scheduled sweep of expired staging uploads                 |
|  [05]   | `server.on(EVENTS.POST_FINISH, (req, res, upload) => ...)` | event tap      | observability beside the finish hook, not the seam         |

## [04]-[IMPLEMENTATION_LAW]

[TUS_TOPOLOGY]:
- offset-verified resume is the whole protocol: HEAD answers `Upload-Offset`, PATCH appends only at that offset, and a mismatched offset is a protocol refusal — no byte is ever re-trusted, so resume after any failure is a HEAD plus a PATCH from the verified offset.
- the store is a port: `DataStore` is the abstract contract and `@tus/s3-store` the composed implementation; the server owns protocol conformance, the store owns bytes — the rail never subclasses either.
- hooks are the composition seams: `onUploadCreate` validates and enriches metadata before creation, `onUploadFinish` runs after the final byte lands and before the client reply — the content-address finalize fold (chunk, digest, conditional re-put) attaches there and its refusal aborts the reply typed.
- dual dispatch, one server: `handle` (node req/res) and `handleWeb` (fetch Request/Response) serve the same instance, so the runtime lane is a serving-plane selection and the tus surface exists once.
- RUFH is a row swap: the IETF draft carries the same offset/complete semantics; on RFC the protocol row swaps under unchanged store and hooks.

[INTEGRATION_LAW]:
- Stack with `@tus/s3-store` (`.api/tus-s3-store.md`): the `datastore` slot takes `new S3Store({ s3ClientConfig: { bucket, ...clientConfig }, partSize })` — tus offsets map onto S3 multipart parts, and the staging band lives under the object plane's own endpoint/credential `Config` facts.
- Stack with `effect`: each dispatch lifts through `Effect.tryPromise`; the server constructs inside `Effect.acquireRelease` at the owning service; hook bodies run the rail's Effect folds through the owning `ManagedRuntime` handle — a hook is a boundary adapter, and its thrown refusal is the tus-conformant abort.
- Stack with `object/store`: the finish hook's finalize fold reads the staged bytes and lands the content-addressed conditional put through the object plane's one client; the staging upload is then removed — staging and content bands never share keys.
- Stack with the ui branch: `tus-js-client` is the browser leg driving POST/PATCH/HEAD against this server; the server catalog owes it nothing — the wire is the tus protocol itself.

[LOCAL_ADMISSION]:
- construct one `Server` per staging band inside a scoped service; never per request.
- attach admission and finalize logic only through `onUploadCreate`/`onUploadFinish`; never fork the handler classes or read store internals.
- run `cleanUpExpiredUploads` on the maintenance cadence; never let the staging band grow unbounded.
- serve through `handle`/`handleWeb` under the serving plane's route; never `server.listen()` — the process owns its boot edge.

[RAIL_LAW]:
- Package: `@tus/server`
- Owns: tus catalog-bound protocol conformance — creation, offset-verified PATCH resume, HEAD/DELETE, expiration sweep, the `ServerOptions` policy record, the hook seams, the `Locker` contract, and the re-exported `@tus/utils` model (`Upload`, `DataStore`, `EVENTS`)
- Accept: one scoped `Server` per staging band, hooks as the admission/finalize seams, `handleWeb` for fetch-shaped runtimes, `maxSize` as the admission ceiling, `MemoryLocker` on a single node
- Reject: per-request server construction, handler subclassing, `listen()` inside library code, finalize logic outside `onUploadFinish`, a staging band without an expiration sweep
