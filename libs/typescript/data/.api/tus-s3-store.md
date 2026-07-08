# [TS_DATA_API_TUS_S3_STORE]

`@tus/s3-store` implements the `@tus/utils` `DataStore` contract over S3: `create` opens a multipart upload and writes a `${id}.info` metadata object, `write(src, id, offset)` streams the PATCH body into `UploadPart` calls sized by `partSize` under a `maxConcurrentPartUploads` semaphore, sub-part remainders persist as incomplete-part objects so a resumed PATCH re-reads them, and the final write triggers `CompleteMultipartUpload` — so the tus offset IS the multipart high-water mark and resume costs a `HeadObject` on the info object plus `ListParts`. The store rides the same `@aws-sdk/client-s3` config vocabulary as the object plane (`s3ClientConfig` embeds `S3ClientConfig` plus `bucket`), so endpoint, credentials, and `forcePathStyle` are the one set of provider `Config` facts. The rail treats this store as the STAGING band only: uploads land under tus-minted ids, the finish hook's finalize fold re-homes bytes to their `ContentKey`, and expiration (`expirationPeriodInMilliseconds` + `deleteExpired`) grooms abandoned staging uploads.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/s3-store`
- package: `@tus/s3-store`
- license: `MIT`
- backing: `@aws-sdk/client-s3` (the `S3` client it constructs from `s3ClientConfig`), `@shopify/semaphore` (part-upload concurrency), `@tus/utils` (`DataStore`, `Upload`, `KvStore`)
- runtime: server plane (node/bun) — streams PATCH bodies through `node:stream`/temp files
- module format: ESM; one root export (`S3Store`, `Options`, `MetadataValue`)
- rail: the `object/stream` staging band beneath `@tus/server` (`.api/tus-server.md`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the store, its options, and the metadata record
- rail: object/stream

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:-------------------------------------------------------------- |:------------- |:------------------------------------------------------------------------- |
| [01] | `S3Store` (`extends DataStore`, `constructor(options)`) | store | the `datastore` slot of `@tus/server`'s `Server` |
| [02] | `Options.s3ClientConfig` (`S3ClientConfig & { bucket }`) | provider config | the object plane's endpoint/credential/`forcePathStyle` facts, plus the staging bucket |
| [03] | `Options.partSize` / `.minPartSize` / `.maxMultipartParts` | part policy | preferred part bytes (≥ 5 MiB), uniform-part floor, 10 000-part ceiling — the server recomputes optimal size against declared length |
| [04] | `Options.maxConcurrentPartUploads` | concurrency | the part-upload semaphore width per PATCH |
| [05] | `Options.expirationPeriodInMilliseconds` / `.useTags` | expiry | staging TTL; completion tags on the info object drive tag-based lifecycle |
| [06] | `Options.cache` (`KvStore<MetadataValue>`) | metadata cache | fronts the `${id}.info` `HeadObject` reads; in-memory default |
| [07] | `MetadataValue` (`{ file: Upload, "upload-id", "tus-version" }`) | record | the staged upload's identity — the S3 `UploadId` rides here |
| [08] | `S3Store.maxUploadSize` (`5497558138880`) | bound | the 5 TiB S3 object ceiling the store enforces |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the DataStore members the server drives
- rail: object/stream
- The rail never calls these directly — `@tus/server` drives them per protocol verb; the rail's own reads are `read` (the staged bytes for the finalize fold) and `deleteExpired` (the groom).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `new S3Store({ s3ClientConfig: { bucket, endpoint, forcePathStyle, credentials, region }, partSize, maxConcurrentPartUploads, expirationPeriodInMilliseconds })` | construct | one store per staging band, built beside the server |
| [02] | `store.create(upload)` / `store.write(src, id, offset)` / `store.getUpload(id)` | protocol | driven by POST/PATCH/HEAD — offsets map onto `ListParts`-derived progress |
| [03] | `store.read(id): Promise<stream.Readable>` | staged read | the finalize fold's byte source — lifts through `Stream.fromReadableStream` after `Readable.toWeb` |
| [04] | `store.remove(id)` / `store.deleteExpired(): Promise<number>` / `store.getExpiration()` | groom | staging removal after re-home; the scheduled expiry sweep |
| [05] | `store.declareUploadLength(id, length)` | deferred size | tus `Upload-Defer-Length` — size declared after creation |

## [04]-[IMPLEMENTATION_LAW]

[STAGING_TOPOLOGY]:
- offsets are multipart arithmetic: a PATCH body streams into parts of `partSize`; the confirmed offset is the byte sum of consecutive uploaded parts plus any incomplete-part object, so resume is exact and no byte re-uploads past the high-water mark.
- metadata is an S3 object: `${id}.info` carries the `Upload` record and `UploadId` in object `Metadata`, read by one `HeadObject` and fronted by the `cache` KvStore — the store is stateless across processes by construction.
- the staging band is not the content band: tus ids are random staging keys; identity is minted by the rail's finalize fold, never by this store — `useTags`/expiration groom what finalize never re-homed.
- part policy is bounded both ways: `partSize` prefers, `minPartSize` floors, `maxMultipartParts` caps at the S3 limit, and the store recomputes the optimal size from the declared length so a 5 TiB upload still fits 10 000 parts.

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
