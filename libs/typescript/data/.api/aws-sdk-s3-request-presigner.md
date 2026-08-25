# [TS_DATA_API_AWS_SDK_S3_REQUEST_PRESIGNER]

`@aws-sdk/s3-request-presigner` mints a SigV4 query-signed URL from a live `S3Client` and any S3 command through one polymorphic `getSignedUrl` — the command value discriminates upload, download, part, and probe, and the URL is a bounded-TTL bearer capability the browser consumes with no SDK. It inherits the client's resolved `credentials`/`region`/`endpoint`/`forcePathStyle`, so MinIO/R2/Tigris presign identically; under Effect one `tryPromise` returns the typed `{ url, expiresAt }` the `object/store` `[06]-[GRANT_MINT]` row hands the edge.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the presign entry and its argument policy

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `getSignedUrl(client, command, options?)`                   | presign entry | one mint over `S3Client` + command → `object/store` `[06]` |
|  [02]   | `RequestPresigningArguments.expiresIn` (`number`, seconds)  | TTL           | token lifetime, a `Config`-bounded fact                    |
|  [03]   | `.signableHeaders` / `.unsignableHeaders` (`Set<string>`)   | signed set    | pin SSE-C / content-type into the signature; drop volatile |
|  [04]   | `.hoistableHeaders` / `.unhoistableHeaders` (`Set<string>`) | query hoist   | hoist `Response*` overrides into the URL query             |

[PUBLIC_TYPE_SCOPE]: the lower-level presigner
- `getSignedUrl` composes `S3RequestPresigner`; reach the class to presign a hand-built `HttpRequest` or inject rotated/delegated credentials via `presignWithCredentials`. Both `presign*` return `Promise<IHttpRequest>`; `S3RequestPresignerOptions = PartialBy<SignatureV4MultiRegionInit, "service" | "uriEscapePath"> & { signingName? }`.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `S3RequestPresigner(S3RequestPresignerOptions)`                     | signer        | reusable signer; built per `getSignedUrl` call |
|  [02]   | `presign(HttpRequest, args?)`                                       | presign       | sign a hand-built request → URL                |
|  [03]   | `presignWithCredentials(HttpRequest, AwsCredentialIdentity, args?)` | presign       | explicit-credential presign (rotation / STS)   |
|  [04]   | `S3RequestPresignerOptions`                                         | options       | region/credentials/sha256 init from the client |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: minting a presigned capability token under Effect

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `getSignedUrl(client, command, { expiresIn })`                    | mint           | `object/store` `[06]` → `{ url, expiresAt }`    |
|  [02]   | `new PutObjectCommand({ Key, IfNoneMatch: "*", ChecksumSHA256 })` | upload URL     | browser-direct conditional-put upload token     |
|  [03]   | `new GetObjectCommand({ Key, ResponseContentDisposition })`       | download URL   | browser-direct download with response overrides |
|  [04]   | `new UploadPartCommand({ UploadId, PartNumber })`                 | part URL       | multipart browser-direct part upload token      |
|  [05]   | `Config.integer("PRESIGN_TTL_SECONDS")` → `expiresIn`             | TTL config     | composition-root `Config`; never a literal      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `getSignedUrl(client, command, options)` presigns any S3 operation, so a new presigned operation is a new command value, never a `presignUpload`/`presignDownload` method family.
- `signableHeaders` pins SSE-C and content-type into the signature; `hoistableHeaders` lifts `Response*` overrides into the query — both `Set<string>` policy on `RequestPresigningArguments`.

[STACKING]:
- `@aws-sdk/client-s3`(`.api/aws-sdk-client-s3.md`): `getSignedUrl` takes the SAME `S3Client` and command classes the object plane sends — `PutObjectCommand{ IfNoneMatch: "*", ChecksumSHA256 }` presigns to a conditional-put upload URL, carrying content-address idempotency into the browser-direct path; a SigV4 signer over the client config, never a second client.
- `effect`(`.api/effect.md`): the `Promise` is one `Effect.tryPromise`, `expiresIn` from `Config`, the row returning a typed `{ url, expiresAt: DateTime }`; `Schema` encodes the presign response at the edge boundary.
- `sharp`(`.api/sharp.md`): in the `object/store` `[06]-[GRANT_MINT]` fan-out `sharp` encodes each content-addressed derivative and `@aws-sdk/client-s3` conditional-puts it; this presigner mints one `getSignedUrl` `GetObject` URL per derivative row, each keyed by its own content-key and TTL-bounded like the source — the browser-direct delivery leg.
- `security`/`edge`: the presigned URL is a capability grant `security` bounds by TTL and audits at mint, `edge` returns as the object-access token; SSE-C keys pinned via `signableHeaders` are `Redacted` at rest.

[LOCAL_ADMISSION]:
- Mint through one `Effect.tryPromise` over `getSignedUrl` with `expiresIn` bound from `Config`, return the typed `{ url, expiresAt }`, and reuse the object plane's `S3Client` config for every provider.
