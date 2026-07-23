# [TS_DATA_API_TUS_S3_STORE]

`@tus/s3-store` implements the `@tus/utils` `DataStore` contract over S3: `create` opens a multipart upload and writes a `${id}.info` metadata object, `write(src, id, offset)` streams the PATCH body into `UploadPart` calls under a `maxConcurrentPartUploads` semaphore, and the final write triggers `CompleteMultipartUpload` — the tus offset IS the multipart high-water mark; resume costs `HeadObject` + `ListParts`. It rides the object plane's `@aws-sdk/client-s3` config vocabulary and serves the STAGING band only: the finalize fold re-homes bytes to their `ContentKey`, expiration grooms the rest.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/s3-store`
- package: `@tus/s3-store`
- license: `MIT`
- backing: `@aws-sdk/client-s3` (the `S3` client it constructs from `s3ClientConfig`), `@tus/utils` (`DataStore`, `Upload`, `KvStore`)
- runtime: server plane (node/bun) — streams PATCH bodies through `node:stream`/temp files
- module format: ESM; one root export (`S3Store`, `Options`, `MetadataValue`)
- rail: the `object/stream` staging band beneath `@tus/server` (`.api/tus-server.md`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the store, its options, and the metadata record
- rail: object/stream
- `S3Store` extends `DataStore` with `constructor(options)`; `MetadataValue` is `{ file: Upload, "upload-id", "tus-version" }`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                       |
| :-----: | :------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `S3Store`                                                | store           | the `datastore` slot of `@tus/server`'s `Server`          |
|  [02]   | `Options.s3ClientConfig` (`S3ClientConfig & { bucket }`) | provider config | endpoint/credential/`forcePathStyle`, staging bucket      |
|  [03]   | `Options.partSize`                                       | part policy     | preferred part bytes (≥ 5 MiB)                            |
|  [04]   | `Options.minPartSize`                                    | part policy     | uniform-part floor                                        |
|  [05]   | `Options.maxMultipartParts`                              | part policy     | 10 000-part S3 ceiling                                    |
|  [06]   | `Options.maxConcurrentPartUploads`                       | concurrency     | the part-upload semaphore width per PATCH                 |
|  [07]   | `Options.expirationPeriodInMilliseconds` / `.useTags`    | expiry          | staging TTL; completion tags drive tag-based lifecycle    |
|  [08]   | `Options.cache` (`KvStore<MetadataValue>`)               | metadata cache  | fronts `${id}.info` `HeadObject` reads; in-memory default |
|  [09]   | `MetadataValue`                                          | record          | the staged upload identity — S3 `UploadId` rides here     |
|  [10]   | `S3Store.maxUploadSize` (`5497558138880`)                | bound           | the 5 TiB S3 object ceiling the store enforces            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the DataStore members the server drives
- rail: object/stream
- `@tus/server` drives the store per protocol verb; the rail's own reads are `read` (staged bytes for the finalize fold) and `deleteExpired` (the groom). `new S3Store(options)` takes `{ s3ClientConfig: { bucket, endpoint, forcePathStyle, credentials, region }, partSize, maxConcurrentPartUploads, expirationPeriodInMilliseconds }`; `read` returns `Promise<stream.Readable>` lifted through `Stream.fromReadableStream` after `Readable.toWeb`, `deleteExpired` returns `Promise<number>`. Driven members are `create(upload)`/`write(src, id, offset)`/`getUpload(id)`/`read(id)`/`remove(id)`/`deleteExpired()`/`getExpiration()`/`declareUploadLength(id, length)`.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new S3Store(options)`                       | construct      | one store per staging band, built beside the server      |
|  [02]   | `create` / `write` / `getUpload`             | protocol       | POST/PATCH/HEAD — offsets from `ListParts` progress      |
|  [03]   | `read`                                       | staged read    | the finalize fold's byte source                          |
|  [04]   | `remove` / `deleteExpired` / `getExpiration` | groom          | staging removal after re-home; expiry sweep              |
|  [05]   | `declareUploadLength`                        | deferred size  | tus `Upload-Defer-Length` — size declared after creation |

## [04]-[IMPLEMENTATION_LAW]

[STAGING_TOPOLOGY]:
- offsets are multipart arithmetic: a PATCH body streams into parts of `partSize`; the confirmed offset is the byte sum of consecutive uploaded parts and any incomplete-part object, so resume is exact and no byte re-uploads past the high-water mark.
- metadata is an S3 object: `${id}.info` carries the `Upload` record and `UploadId` in object `Metadata`, read by one `HeadObject` and fronted by the `cache` KvStore — the store is stateless across processes by construction.
- staging band and content band stay disjoint: tus ids are random staging keys; the rail's finalize fold mints identity, never this store — `useTags`/expiration groom what finalize never re-homed.
- part policy is bounded both ways: `partSize` targets, `minPartSize` floors, `maxMultipartParts` caps at the S3 limit, and the store recomputes part size from the declared length so a 5 TiB upload still fits 10 000 parts.

[INTEGRATION_LAW]:
- Stack with `@tus/server` (`.api/tus-server.md`): the store is the `datastore` slot; the server's `onUploadFinish` fires after this store's `CompleteMultipartUpload` settles, so the finalize fold reads a whole, durable staged object.
- Stack with `@aws-sdk/client-s3` (`.api/aws-sdk-client-s3.md`): `s3ClientConfig` is the object plane's provider vocabulary verbatim — one set of endpoint/credential facts serves the content client, the presigner, and this staging store.
- Stack with `object/stream`: `read(id)` feeds the finalize fold (chunk stage → digest fold → conditional re-put via `@aws-sdk/lib-storage`, `.api/aws-sdk-lib-storage.md`); `remove(id)` closes staging after the 200-or-412 lands; `deleteExpired` rides the maintenance cadence beside the server's `cleanUpExpiredUploads`.
- Stack with `effect`: construction sits inside the owning service's scope; `read`'s `Readable` lifts at the seam; expiry sweeps run as scheduled effects — the store's promises never cross into domain code.

[LOCAL_ADMISSION]:
- build the store from the object plane's provider `Config` facts; never a second endpoint/credential vocabulary for staging.
- treat staged keys as opaque staging identity; never serve or persist a staging key as a content coordinate.
- set `expirationPeriodInMilliseconds` and run the expiry sweep; never an ungroomed staging band.
- size parts through `partSize`/`minPartSize` policy fields; never hardcode part arithmetic beside the store.

[RAIL_LAW]:
- Package: `@tus/s3-store`
- Owns: the S3 `DataStore` — multipart-backed offset persistence, `${id}.info` metadata objects, incomplete-part carry, part-size arithmetic, staging expiry, and the `Options` policy record
- Accept: one store per staging band under the object plane's provider facts, `read` as the finalize byte source, expiry-swept staging, part policy as options fields
- Reject: staging keys leaking as content identity, a second provider-config vocabulary, an unswept staging band, driving the store outside the tus server except `read`/`remove`/`deleteExpired`
