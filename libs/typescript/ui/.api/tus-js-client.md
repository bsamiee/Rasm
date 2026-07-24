# [TS_UI_API_TUS_JS_CLIENT]

`tus-js-client` drives the browser leg of the tus resumable-upload protocol: one `Upload` instance owns POST creation, HEAD offset discovery, PATCH chunk transfer, retry policy, and URL-storage resume against a tus server. Proven server offsets gate every transfer, and the client owns no bytes past finalization — content-address folding stays server-side.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tus-js-client`
- package: `tus-js-client` (MIT)
- module: CJS + ESM; the package `browser` field remaps the `main`/`module` node builds to browser builds for the ui bundle
- runtime: browser — the upload boundary for the ui bundle
- rail: upload client for the `object/stream` resumable-upload lane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: upload lifecycle, resume state, and protocol callbacks

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                   |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `Upload`           | upload client     | one browser upload session per `File`/`Blob`/`Buffer`/reader   |
|  [02]   | `UploadOptions`    | option shape      | endpoint, metadata, chunk, retry, hook, and storage policy     |
|  [03]   | `PreviousUpload`   | resume fact       | stored URL, parallel URLs, metadata, size, storage key         |
|  [04]   | `UrlStorage`       | resume store port | `findAllUploads`/`findUploadsByFingerprint`/`addUpload`/remove |
|  [05]   | `OnSuccessPayload` | success receipt   | wraps the final `HttpResponse` for `onSuccess`                 |
|  [06]   | `DetailedError`    | protocol fault    | `originalRequest`/`originalResponse`/`causingError` classing   |

[PUBLIC_TYPE_SCOPE]: request, chunk source, and transport seams

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]    | [CAPABILITY]                                        |
| :-----: | :------------------------------------------ | :--------------- | :-------------------------------------------------- |
|  [01]   | `HttpStack` / `DefaultHttpStack`            | transport port   | `createRequest(method, url)`/`getName`; default     |
|  [02]   | `HttpRequest`                               | request port     | method, URL, header, progress, `send`, abort access |
|  [03]   | `HttpResponse`                              | response receipt | status, header, body, underlying-object access      |
|  [04]   | `FileReader` / `FileSource` / `SliceResult` | chunk source     | `openFile` -> source with `size`/`slice`/`close`    |
|  [05]   | `defaultOptions`                            | policy seed      | `httpStack`/`fileReader`/`urlStorage`/`fingerprint` |
|  [06]   | `isSupported` / `canStoreURLs`              | capability facts | gate upload support and persistent resume storage   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: start, resume, retry, and terminate

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new Upload(file, options)`                                                         | ctor     | new session or `uploadUrl` resume |
|  [02]   | `upload.start()`                                                                    | instance | drives POST then PATCH chunk flow |
|  [03]   | `upload.abort(shouldTerminate?)`                                                    | instance | cancel or escalate to terminate   |
|  [04]   | `Upload.terminate(url, options?)`                                                   | static   | terminate a known URL             |
|  [05]   | `upload.findPreviousUploads()` / `upload.resumeFromPreviousUpload(previousUpload)`  | instance | resolve candidates and bind one   |
|  [06]   | `onBeforeRequest` / `onAfterResponse` / `onShouldRetry`                             | property | signing and retry-refusal policy  |
|  [07]   | `onProgress` / `onChunkComplete` / `onUploadUrlAvailable` / `onSuccess` / `onError` | property | progress, URL persist, and fault  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every transfer op proves the server offset before sending bytes; resume rebinds a stored `uploadUrl` rather than restarting.
- `Upload` drives protocol only; content-address finalization and object-store writes stay server-side.

[STACKING]:
- `@tus/server`(`../../data/.api/tus-server.md`): the client's POST/HEAD/PATCH/DELETE requests land on the one `Server` instance whose hooks own admission and finalization.
- `@tus/s3-store`(`../../data/.api/tus-s3-store.md`): the server datastore folds the client's ordered chunks and verified offsets into multipart parts.
- `effect`(`../../.api/effect.md`): an `Upload` binds as a scoped resource, callbacks enqueue progress, receipt, and typed-fault facts, and `abort` is the scope release arm.
- within-lib: `ui` binds URL storage, request headers, and progress callbacks as policy rows fed from decoded configuration and action payloads.

[LOCAL_ADMISSION]:
- Mint one `Upload` per selected source and one policy row per endpoint.
- Resume through `findPreviousUploads` then `resumeFromPreviousUpload`, binding the server offset before transfer.
- Persist `uploadUrl` through the `UrlStorage` port; completion or termination clears it.
- Classify retry on `DetailedError.originalResponse.getStatus()` through `onShouldRetry`.

[RAIL_LAW]:
- Package: `tus-js-client`
- Owns: browser tus upload sessions — `Upload`, resume discovery and URL storage, chunking, progress and request/response hooks, retry policy, custom `HttpStack`, file reader, and termination
- Accept: one session per source, verified-offset resume, callback-to-effect progress and receipt projection, custom request policy through `HttpStack`/hooks, server-owned finalization
- Reject: whole-file retry after a resumable URL exists, a second resume store beside the port, secret literals in headers, client-side content-address finalization, a shared `Upload` across independent files
