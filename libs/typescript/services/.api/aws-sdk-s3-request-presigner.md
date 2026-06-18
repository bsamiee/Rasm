# [API_CATALOGUE] @aws-sdk/s3-request-presigner

`@aws-sdk/s3-request-presigner` generates presigned S3 URLs from an `S3Client` instance and a command, and exposes `S3RequestPresigner` for low-level `HttpRequest` signing when the command-style API does not apply. The primary entrypoint is the `getSignedUrl` function; `S3RequestPresigner` covers advanced scenarios where a pre-built `HttpRequest` must be signed directly.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/s3-request-presigner`
- package: `@aws-sdk/s3-request-presigner`
- module: `@aws-sdk/s3-request-presigner`
- asset: `getSignedUrl` function, `S3RequestPresigner` class, `S3RequestPresignerOptions` type
- rail: object-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: presigner type family
- rail: object-store

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                                      |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | `getSignedUrl`              | async function | generates presigned URL from `S3Client` + `Command`                                               |
|   [2]   | `S3RequestPresigner`        | class          | low-level `HttpRequest` presigner implementing `RequestPresigner`                                 |
|   [3]   | `S3RequestPresignerOptions` | type alias     | extends `SignatureV4MultiRegionInit` minus `service`/`uriEscapePath`, with optional `signingName` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: URL generation
- rail: object-store

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY]  | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|   [1]   | `getSignedUrl(client, command, options?)`                             | presigned URL   | `Promise<string>`; `options` is `RequestPresigningArguments`          |
|   [2]   | `new S3RequestPresigner(options)`                                     | constructor     | low-level presigner from `S3RequestPresignerOptions`                  |
|   [3]   | `S3RequestPresigner.presign(request, options?)`                       | sign            | `Promise<IHttpRequest>` — signs an `HttpRequest`                      |
|   [4]   | `S3RequestPresigner.presignWithCredentials(request, creds, options?)` | sign with creds | `Promise<IHttpRequest>` — signs with explicit `AwsCredentialIdentity` |

## [4]-[IMPLEMENTATION_LAW]

[PRESIGNER_TOPOLOGY]:
- `getSignedUrl` accepts any `Client<any, InputTypesUnion, MetadataBearer, any>` and any matching `Command`; in practice always called with `S3Client` and an S3 command such as `GetObjectCommand` or `PutObjectCommand`.
- `RequestPresigningArguments` from `@smithy/types` carries `expiresIn` (seconds), `signingDate`, `signingRegion`, and `signingService` fields; `expiresIn` is the primary field for URL TTL.
- `S3RequestPresigner` is used when the caller already has a raw `HttpRequest` rather than a command; the `presign` method merges `unsignableHeaders`, `hoistableHeaders`, and `unhoistableHeaders` from the options before signing.

[LOCAL_ADMISSION]:
- Presigned URLs encode credentials and expiry in the query string; they do not require AWS credentials at fetch time.
- The presigned URL is a plain `string`; the consuming browser or service performs a plain HTTP GET/PUT/DELETE against it.
- `@effect-aws/client-s3` exposes `getObject(args, { presigned: true })` and `putObject(args, { presigned: true })` as typed Effect alternatives to calling `getSignedUrl` directly; prefer those in Effect-native code.

[RAIL_LAW]:
- Package: `@aws-sdk/s3-request-presigner`
- Owns: S3 presigned URL generation and low-level HttpRequest signing
- Accept: `getSignedUrl(client, command, { expiresIn })` as the primary path
- Reject: hand-rolled SigV4 URL construction when this package is admitted
