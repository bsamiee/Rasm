# [@aws-sdk/s3-request-presigner] — presigned-URL minting for the object presign rows

`@aws-sdk/s3-request-presigner` mints SigV4 query-signed URLs from a live `S3Client` and any S3 command — `getSignedUrl(client, command, { expiresIn })` is one polymorphic entry parameterized by the command value and a TTL, not a per-operation presign family. The presigned URL is a bounded-lifetime capability token: a `PutObjectCommand` yields a browser-direct upload URL (carrying the `IfNoneMatch`/checksum conditional-put headers), a `GetObjectCommand` yields a download URL (with `ResponseContentType`/`ResponseContentDisposition` overrides hoisted into the query), and `UploadPartCommand`/`HeadObjectCommand` yield part-upload and existence-probe URLs. It inherits the client's entire resolved config — `credentials`, `region`, `endpoint`, `forcePathStyle` — so presigning works unchanged against MinIO/R2/Tigris, and the store never re-declares provider facts. Under Effect the `Promise` is one `tryPromise` returning a typed `{ url, expiresAt }` the `object/presign` row hands to the edge.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/s3-request-presigner`
- package: `@aws-sdk/s3-request-presigner`
- version: `3.1078.0`
- license: `Apache-2.0`
- peer: none — pairs with `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`) by consuming its `S3Client` + command values
- backing: `@aws-sdk/signature-v4-multi-region` (the SigV4 / SigV4a multi-region query signer)
- runtime: `runtime:node` and browser — minting is server-side (holds credentials); the minted URL is consumed anywhere with no SDK
- module format: ESM/CJS dual, `sideEffects: false`; modules `getSignedUrl`, `presigner`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the presign entry and its argument policy
- rail: store/object
- `getSignedUrl` is the entry the store uses; `RequestPresigningArguments` parameterizes the TTL and the signed/hoisted header sets — every presign axis is a policy value, not a code path.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `getSignedUrl(client, command, options?): Promise<string>`    | presign entry   | `object/presign` — one mint over any `S3Client` + command                  |
|  [02]   | `RequestPresigningArguments.expiresIn` (`number`, seconds)    | TTL             | the capability-token lifetime; a `Config` fact, default-bounded            |
|  [03]   | `RequestPresigningArguments.signableHeaders`/`unsignableHeaders` (`Set<string>`) | signed set | pin SSE-C / content-type into the signature; exclude volatile headers      |
|  [04]   | `RequestPresigningArguments.hoistableHeaders`/`unhoistableHeaders` (`Set<string>`) | query hoist | hoist `Response*` overrides into the URL query for browser GET; signing scope (`region`/`service`) inherited from the client |

[PUBLIC_TYPE_SCOPE]: the lower-level presigner
- rail: store/object
- `getSignedUrl` composes `S3RequestPresigner`; the class is the reusable signer when the store presigns a hand-built `HttpRequest` or supplies credentials explicitly (rotation, delegated STS).

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------- |
|  [01]   | `S3RequestPresigner` (`constructor(options)`)                 | signer          | reusable signer; `getSignedUrl` builds one per call from the client config |
|  [02]   | `S3RequestPresigner.presign(request, args?): Promise<IHttpRequest>` | presign     | sign a built `HttpRequest`; the signed request → URL                       |
|  [03]   | `S3RequestPresigner.presignWithCredentials(request, credentials, args?): Promise<IHttpRequest>` | presign | explicit-credential presign (rotation / delegated STS)                     |
|  [04]   | `S3RequestPresignerOptions` (`PartialBy<SignatureV4MultiRegionInit, "service" \| "uriEscapePath"> & { signingName? }`) | options | region/credentials/sha256 init inherited from the client config            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: minting a presigned capability token under Effect
- rail: store/object
- One `getSignedUrl` mints any operation; the command value discriminates. The `Promise` becomes a typed `{ url, expiresAt }` so the edge never inspects a raw string.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Effect.tryPromise(() => getSignedUrl(client, command, { expiresIn }))` → `{ url, expiresAt }`      | mint           | `object/presign` — the typed presign row                 |
|  [02]   | `getSignedUrl(client, new PutObjectCommand({ Key, IfNoneMatch: "*", ChecksumSHA256 }), { expiresIn })` | upload URL | browser-direct conditional-put upload token              |
|  [03]   | `getSignedUrl(client, new GetObjectCommand({ Key, ResponseContentDisposition }), { expiresIn, hoistableHeaders })` | download URL | browser-direct download with hoisted response overrides |
|  [04]   | `getSignedUrl(client, new UploadPartCommand({ UploadId, PartNumber }), { expiresIn })`              | part URL       | multipart browser-direct part upload token               |
|  [05]   | `Config.integer("PRESIGN_TTL_SECONDS")` → `expiresIn`                                               | TTL config     | `host/config` — the token lifetime, never a literal      |

## [04]-[IMPLEMENTATION_LAW]

[PRESIGN_TOPOLOGY]:
- one mint, command discriminates: `getSignedUrl(client, command, options)` presigns ANY S3 operation — upload, download, part upload, head — so the presign surface is one entry over the command space, never a `presignUpload`/`presignDownload` family. A new presigned operation is a new command value.
- the URL is a bounded capability token: `expiresIn` is the whole grant lifetime; SigV4 query params carry the signature, so the holder needs no credentials. The store returns `{ url, expiresAt }` and the edge treats the URL as a bearer-equivalent secret with a hard TTL.
- config is inherited, never re-declared: the presigner reads the `S3Client`'s resolved `credentials`, `region`, `endpoint`, and `forcePathStyle`, so a presign against MinIO/R2/Tigris is the same call as against AWS — the S3-compatible provider facts live once on the client.
- signed vs. hoisted headers are policy: `signableHeaders` pins SSE-C and content-type into the signature (the upload must send them); `hoistableHeaders` lifts `Response*` overrides into the query (the browser GET needs no headers). Both are `Set<string>` policy values on `RequestPresigningArguments`.
- `S3RequestPresigner` is the reusable signer: `getSignedUrl` constructs one per call from the client config; the class is reached directly only to presign a hand-built `HttpRequest` or to inject rotated/delegated credentials via `presignWithCredentials`.

[INTEGRATION_LAW]:
- Stack with `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`): `getSignedUrl` takes the SAME `S3Client` and the SAME command classes the object plane sends — `PutObjectCommand{ IfNoneMatch: "*", ChecksumSHA256 }` presigns to a conditional-put upload URL, so the content-address idempotency and integrity contract survives into the browser-direct path. The presigner is a thin SigV4 signer over the client's config, not a second client.
- Stack with `effect` (`.api/effect.md`): the `Promise` is one `Effect.tryPromise`; `expiresIn` comes from `Config`; the row returns a typed `{ url, expiresAt: DateTime }` (never a bare string); `Schema` encodes the presign response at the edge boundary. No new rail — Effect owns the async lift and the typed carrier.
- Stack with `sharp` (`.api/sharp.md`): in the `object/presign` fan-out `sharp` encodes each content-addressed derivative (`toFormat(row.format, row.options)` per derivative-spec row) and `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`) conditional-puts it (`IfNoneMatch: "*"`); this presigner then mints one presigned `GetObject` URL per derivative row via `getSignedUrl`, each keyed by the derivative's own content-key and TTL-bounded like the source — the browser-direct delivery leg, never the server-side codec or the idempotent write.
- Stack with `security`/`edge`: the presigned URL is a capability grant — `security` bounds the TTL and audits the mint; `edge` returns it as the object-access token. SSE-C keys pinned via `signableHeaders` are `Redacted` at rest.

[LOCAL_ADMISSION]:
- mint with `getSignedUrl(client, command, { expiresIn })` and one `Effect.tryPromise`; never build a per-operation presign helper — the command value is the discriminant.
- bound `expiresIn` from `Config` and return `{ url, expiresAt }`; never presign with an unbounded or literal TTL, and never surface the raw URL untyped.
- reuse the object plane's `S3Client` config; never construct a second client or re-declare `credentials`/`endpoint`/`forcePathStyle` for presigning.
- pin SSE-C and content-type via `signableHeaders`, hoist `Response*` overrides via `hoistableHeaders`; treat the minted URL as a bearer secret.

[RAIL_LAW]:
- Package: `@aws-sdk/s3-request-presigner`
- Owns: `getSignedUrl` (the one presign entry over any `S3Client` + command), the `S3RequestPresigner` signer (`presign`/`presignWithCredentials`), and `RequestPresigningArguments` policy (TTL, signed/hoisted header sets, signing scope)
- Accept: one `Effect`-wrapped `getSignedUrl` discriminated by command value, `expiresIn` as a `Config`-bounded TTL, config inherited from the object plane's `S3Client`, `signableHeaders`/`hoistableHeaders` as policy, a typed `{ url, expiresAt }` carrier, the URL as a bounded capability token
- Reject: a per-operation presign family, an unbounded or literal TTL, a second client or re-declared provider facts for presigning, a raw untyped URL, an unaudited or non-expiring capability grant
