# [API_CATALOGUE] @aws-sdk/s3-request-presigner

`@aws-sdk/s3-request-presigner` is the SigV4 query-string presigner for S3: it turns an `S3Client` + `Command` (or a pre-built `HttpRequest`) into a credential-bearing URL a credential-less browser can `GET`/`PUT`/`DELETE`. In the `persistence/object#OBJECT_STORE` design this package is NOT the primary path for the two single-object verbs — `@effect-aws/client-s3` presign-overloads EXACTLY `S3Service.getObject(args, { presigned: true })` and `putObject(args, { presigned: true })` (the ONLY two operations its generator gives a `{ presigned: true }` overload; every other `S3Service` method is single-signature) as typed `Effect<string>` without any presigner import. Every OTHER presign is this package's job: the `PresignGrant.Multipart` per-part `UploadPartCommand` presign (`uploadPart` carries NO `{ presigned: true }` overload), POST-policy form uploads, `HeadObject`/`GetBucket*` presigns, a command from a non-`@effect-aws` client, and raw-`HttpRequest` signing where no command exists.

- package: `@aws-sdk/s3-request-presigner`
- version: `3.1078.0`
- license: `Apache-2.0`
- tier: `node` — SigV4 signing is WebCrypto-capable in both runtimes, but the design credentials this from the node-only `SecretStore`, so the binding lives in the node bundle; the browser only fetches the resulting URL.
- rail: object-store (escape hatch behind `@effect-aws/client-s3`)

## [01]-[PACKAGE_SURFACE]

`index.d.ts` re-exports exactly two owners — `./getSignedUrl` and `./presigner`. The `./constants` module (`UNSIGNED_PAYLOAD`, `SHA256_HEADER`, `X-Amz-*` query-param names, `ALGORITHM_IDENTIFIER`) is INTERNAL — not re-exported through the entry, so it is not a public member and must not be cited as one.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                                          |
| :-----: | :-------------------------- | :------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `getSignedUrl`              | async function | command-style presign: `S3Client` + `Command` → `Promise<string>`                                    |
|  [02]   | `S3RequestPresigner`        | class          | raw-`HttpRequest` presigner implementing `@smithy/types` `RequestPresigner`                            |
|  [03]   | `S3RequestPresignerOptions` | type alias     | `PartialBy<SignatureV4MultiRegionInit, "service" \| "uriEscapePath"> & { signingName?: string }`     |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: command-style presign — rail: object-store

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY]  | [SIGNATURE / RETURN]                                                                                                                                                                       |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `getSignedUrl(client, command, options?)`                            | presigned URL   | `<InputTypesUnion extends object, InputType extends InputTypesUnion, OutputType extends MetadataBearer>(client: Client<any, InputTypesUnion, MetadataBearer, any>, command: Command<InputType, OutputType, any, InputTypesUnion, MetadataBearer>, options?: RequestPresigningArguments) => Promise<string>` |
|  [02]   | `new S3RequestPresigner(options)`                                     | constructor     | `(options: S3RequestPresignerOptions)` — low-level presigner from an explicit SigV4-multi-region init                                                                                     |
|  [03]   | `presigner.presign(request, options?)`                               | sign            | `(requestToSign: IHttpRequest, options?: RequestPresigningArguments) => Promise<IHttpRequest>` — the `RequestPresigner` contract method                                                    |
|  [04]   | `presigner.presignWithCredentials(request, credentials, options?)`   | sign w/ creds   | `(requestToSign: IHttpRequest, credentials: AwsCredentialIdentity, options?: RequestPresigningArguments) => Promise<IHttpRequest>` — signs with an explicitly-supplied credential identity |

`getSignedUrl` is fully polymorphic over client/command — it accepts any Smithy `Client`+`Command` pair, so the same call presigns a `GetObjectCommand`, `PutObjectCommand`, `HeadObjectCommand`, or a `CreateMultipartUploadCommand`/`UploadPartCommand` from `@aws-sdk/client-s3`. `presign` moves all `x-amz-*` headers into the signed query string by default; `presignWithCredentials` is the arm for a caller that already holds a resolved `AwsCredentialIdentity` (the `SecretStore`-resolved access-key pair) and wants to sign a hand-built `HttpRequest` (e.g. a browser POST-policy form) without a credential provider on the client.

## [03]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presigner init and the `@smithy/types` signing-arguments carrier — rail: object-store

`RequestPresigningArguments` (from `@smithy/types`, `extends RequestSigningArguments extends SigningArguments`) is the one options object BOTH `getSignedUrl` and `presign*` accept. Beyond the TTL/region/service fields, the four header hoist-control sets are load-bearing — they make a presign correct for streaming or unsigned-payload uploads:

```ts
interface RequestPresigningArguments {          // the full accepted shape
  readonly expiresIn?: number                   // URL TTL in seconds (primary field; Duration.toSeconds(ttl))
  readonly signingDate?: number | string | Date // signature timestamp; defaults new Date()
  readonly signingService?: string              // overrides the SigV4 service signing name for this call
  readonly signingRegion?: string               // overrides the SigV4 signing region for this call
  readonly unsignableHeaders?: Set<string>      // headers excluded from the signature
  readonly signableHeaders?: Set<string>        // headers forced INTO the signature (overrides unsignableHeaders)
  readonly unhoistableHeaders?: Set<string>     // headers kept in the request header, NOT hoisted to the query string
  readonly hoistableHeaders?: Set<string>       // headers hoisted into the query string and signed (overrides unhoistableHeaders)
}
```

`S3RequestPresignerOptions` is the SigV4-multi-region signer init (`region: string | Provider<string>`, `credentials`, `sha256`) with `service` (defaults `"s3"`) and `uriEscapePath` (defaults `false` for S3 double-encoding correctness) made OPTIONAL by `PartialBy`, plus an optional `signingName` that overrides the SigV4 service signing name. Construct `S3RequestPresigner` directly only when signing a raw `HttpRequest`; the command path (`getSignedUrl`) derives this init from the passed `S3Client`.

## [04]-[IMPLEMENTATION_LAW]

[STACKING]: the escape-hatch presign as one Effect rail
- Single-object presign is NOT this package: `@effect-aws/client-s3` `S3Service.getObject(args, { presigned: true, expiresIn })` / `putObject(args, { presigned: true, expiresIn })` return `Effect<string, SdkError | S3ServiceError, S3Service>` and take the same `RequestPresigningArguments` overload payload. The `PresignGrant` `Data.TaggedEnum` `Match.tagsExhaustive` fold routes `Get`/`Put` to that overload; `Multipart` has NO overload (the generated `uploadPart` is single-signature) and folds through THIS package's `getSignedUrl` instead.
- This package owns every presign the overload cannot reach. Wrap the raw promise as one rail: `Effect.tryPromise({ try: () => getSignedUrl(s3Client, command, { expiresIn: Duration.toSeconds(ttl) }), catch: (cause) => new ObjectFault({ reason: "presign_denied", key, cause }) })`, so a presign failure lands on the same `ObjectFault` policy projection every S3 verb folds to — never a second error rail.
- `PresignGrant.Multipart` is the load-bearing case: after `S3Service.createMultipartUpload` yields the `UploadId`, map each part number through this presigner — `Effect.forEach(rangeOf(parts), (PartNumber) => Effect.tryPromise({ try: () => getSignedUrl(s3Client, new UploadPartCommand({ Bucket, Key, UploadId, PartNumber }), { expiresIn: Duration.toSeconds(ttl) }), catch: (cause) => new ObjectFault({ reason: "presign_denied", key, cause }) }))` — collecting the `ReadonlyArray<string>` of per-part URLs a browser uploads directly. `UploadPartCommand` comes from `@aws-sdk/client-s3` (`aws-sdk-client-s3.md`).
- Credentials never come from `process.env`. The `S3Client` (from `@aws-sdk/client-s3`) is built once at the composition root with the access-key pair `SecretStore.resolve(SecretRef.Doppler(...))` yielded (`security/secret#SECRET_STORE`, the `@dopplerhq/node-sdk` arm), and `presignWithCredentials` takes the resolved `AwsCredentialIdentity` directly for the client-less raw-request case.
- Every presign runs inside the `persistence/tenancy#TENANCY` `withTenant` GUC scope so the bucket/key prefix the URL grants is RLS-scoped to `app.current_tenant`; a presign that escapes that scope lets a tenant sign another tenant's prefix.
- The produced URL is a plain `string`; it crosses no .NET wire and reaches the browser as a credential-less fetch target. `expiresIn` is the TTL carried on each `PresignGrant` row; `unhoistableHeaders`/`signableHeaders` are the arms a streaming `Content-Type`-signed or `UNSIGNED-PAYLOAD` upload sets.

[SIBLING_STACK]:
- `@aws-sdk/client-s3` (`aws-sdk-client-s3.md`) owns the `S3Client` + `GetObjectCommand`/`PutObjectCommand`/`CreateMultipartUploadCommand` this package presigns; it is the only admitted client whose commands feed `getSignedUrl`.
- `@effect-aws/client-s3` (`effect-aws-client-s3.md`) presign-overloads ONLY `getObject`/`putObject` and owns the whole `S3Service` object surface; this package is the presign owner for every other verb (multipart per-part, `HeadObject`, `GetBucket*`, POST-policy) and for raw-`HttpRequest` signing — the two are complementary, not one-behind-the-other.
- `@smithy/types` owns `RequestPresigningArguments`/`RequestPresigner`/`AwsCredentialIdentity`/`HttpRequest`; `@aws-sdk/signature-v4-multi-region` owns the `SignatureV4MultiRegionInit` this package's options partially re-open.

[RAIL_LAW]:
- Package: `@aws-sdk/s3-request-presigner`
- Owns: SigV4 query-string presigning for every S3 command the `@effect-aws/client-s3` overload does NOT model — the `Multipart` per-part `UploadPartCommand` presign, `HeadObject`/`GetBucket*`, POST-policy, non-`@effect-aws` commands — plus raw-`HttpRequest` signing; the overload owns only single-object `getObject`/`putObject`
- Accept: `getSignedUrl(s3Client, command, { expiresIn })` wrapped in `Effect.tryPromise` → `ObjectFault`, under the tenancy GUC, credentials from `SecretStore`; `presignWithCredentials` for the client-less raw-request/POST-policy case
- Reject: `getSignedUrl` for single-object `getObject`/`putObject` where the `{ presigned: true }` overload already covers it; hand-rolled SigV4 URL assembly; a `new Date()`/env credential read instead of the resolved `SecretStore` identity; a presign outside the `withTenant` GUC scope
