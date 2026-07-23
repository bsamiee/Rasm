# [TS_UI_API_TUS_JS_CLIENT]

`tus-js-client` is the browser leg of the tus resumable-upload protocol: one `Upload` instance owns POST creation, HEAD resume discovery, PATCH chunk transfer, progress callbacks, retry policy, URL storage, and termination against the data folder's `@tus/server` object-stream rail. The client never owns bytes after finalization; it proves offsets against the server, resumes from a stored upload URL, and leaves content-address folding to the server finish hook.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tus-js-client`
- package: `tus-js-client` (MIT)
- module format: CJS + ESM; `main` and `module` point at node builds, and the package `browser` map swaps them to browser builds for the ui bundle
- types: bundled `lib/index.d.ts`
- deps: `buffer-from`, `combine-errors`, `is-stream`, `js-base6 catalog`, `lodash.throttle`, `proper-lockfile`, `url-parse`
- runtime: browser upload boundary; node build exists for non-ui execution, but this catalog admits the ui client row
- rail: upload client row for the `object/stream` resumable-upload lane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: upload lifecycle, resume state, and protocol callbacks
- rail: boundaries

The `Upload` lifecycle methods are catalogued in the [03] entrypoints table.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CONSUMER]                                                                       |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Upload`           | upload client     | one browser upload session over a `File`/`Blob`/`Buffer`/reader source           |
|  [02]   | `UploadOptions`    | option shape      | endpoint or `uploadUrl`, metadata, `chunkSize`, retries, hooks, storage, stack   |
|  [03]   | `PreviousUpload`   | resume fact       | stored URL, parallel URLs, metadata, size, creation time, storage key            |
|  [04]   | `UrlStorage`       | resume store port | `findAllUploads` / `findUploadsByFingerprint` / `addUpload` / `removeUpload`     |
|  [05]   | `OnSuccessPayload` | success receipt   | wraps the final `HttpResponse` emitted to `onSuccess`                            |
|  [06]   | `DetailedError`    | protocol fault    | `originalRequest`, optional `originalResponse`/`causingError` for retry classing |

[PUBLIC_TYPE_SCOPE]: request, chunk source, and transport seams
- rail: boundaries

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CONSUMER]                                                          |
| :-----: | :------------------------------------------ | :--------------- | :------------------------------------------------------------------ |
|  [01]   | `HttpStack` / `DefaultHttpStack`            | transport port   | `createRequest(method, url)` / `getName`; default runtime transport |
|  [02]   | `HttpRequest`                               | request port     | method, URL, headers, progress, `send(body)`, abort, runtime access |
|  [03]   | `HttpResponse`                              | response receipt | status, headers, body, underlying runtime-object access             |
|  [04]   | `FileReader` / `FileSource` / `SliceResult` | chunk source     | `openFile` → source with `size` / `slice(start, end)` / `close`     |
|  [05]   | `defaultOptions`                            | policy seed      | `httpStack` / `fileReader` / `urlStorage` / `fingerprint` defaults  |
|  [06]   | `isSupported` / `canStoreURLs`              | capability facts | gates for upload support and persistent resume storage              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: start, resume, retry, and terminate
- rail: boundaries

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [CONSUMER]                       |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `new Upload(file, options)`                                                         | construct      | new vs `uploadUrl` resume        |
|  [02]   | `upload.start()`                                                                    | transfer       | POST/PATCH flow, chunking, retry |
|  [03]   | `upload.abort(shouldTerminate?)`                                                    | stop           | cancel; escalate to terminate    |
|  [04]   | `Upload.terminate(url, options?)`                                                   | terminate      | termination for a known URL      |
|  [05]   | `upload.findPreviousUploads()` / `upload.resumeFromPreviousUpload(previousUpload)`  | resume         | resolve candidates, bind one     |
|  [06]   | `onBeforeRequest` / `onAfterResponse` / `onShouldRetry`                             | policy hooks   | signing, evidence, retry refusal |
|  [07]   | `onProgress` / `onChunkComplete` / `onUploadUrlAvailable` / `onSuccess` / `onError` | event hooks    | progress, URL persist, fault     |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Stack with `@tus/server` (`../../data/.api/tus-server.md`): the client drives POST, HEAD, PATCH, and DELETE against the server's one `Server` instance; server hooks own admission and finalization, and client callbacks only project progress and receipt state.
- Stack with `@tus/s3-store` (`../../data/.api/tus-s3-store.md`): multipart persistence lives under the server datastore; the browser client only supplies ordered chunks and verified offsets.
- Stack with `effect` (`../../.api/effect.md`): an upload is a scoped boundary resource; callbacks enqueue progress facts, completion receipts, and typed faults while `abort` is the release arm for interrupted UI scopes.
- Stack with the browser runtime: URL storage, request headers, and progress callbacks are UI policy rows; secrets and content metadata enter through decoded configuration and action payloads, never literals.

[LOCAL_ADMISSION]:
- Construct one `Upload` per selected file/source and one policy row per endpoint; never hide multiple concurrent files behind one upload instance.
- Resume through `findPreviousUploads` plus `resumeFromPreviousUpload`; never restart a stored upload without checking the server offset.
- Persist `uploadUrl` through the `UrlStorage` port and clear it through successful completion or termination; never duplicate a second resume store beside the port.
- Classify retry through `DetailedError.originalResponse.getStatus()` and `onShouldRetry`; never retry by string-matching error messages.
- Treat the client as a protocol driver only; content-address finalization and object-store writes remain server-side.

[RAIL_LAW]:
- Package: `tus-js-client`
- Owns: browser tus upload sessions - `Upload`, resume discovery and URL storage, chunking, progress hooks, request/response hooks, retry policy, custom HTTP stack, file reader, and termination
- Accept: one upload session per source, verified offset resume, callback-to-effect progress and receipt projection, custom request policy through `HttpStack`/hooks, server-owned finalization
- Reject: row-materialized whole-file retry after a resumable URL exists, duplicate resume stores, secret literals in headers, client-side content-address finalization, shared `Upload` instances across independent files
