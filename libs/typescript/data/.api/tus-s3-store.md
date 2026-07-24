# [TS_DATA_API_TUS_S3_STORE]

`@tus/s3-store` binds the `@tus/utils` `DataStore` contract to S3 multipart uploads: the tus resumable offset IS the multipart high-water mark, so the store holds no cross-process state and resume re-derives from S3. It serves the object plane's STAGING band beneath `@tus/server` on the shared `@aws-sdk/client-s3` provider vocabulary; the finalize fold re-homes staged bytes to their content key and expiration grooms what it never re-homed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tus/s3-store`
- package: `@tus/s3-store` (MIT)
- module: ESM, single root export
- runtime: node/bun server plane
- rail: object-plane STAGING band beneath `@tus/server`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the store, its `Options` policy record, and the staged metadata record

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]                 | [CAPABILITY]                                            |
| :-----: | :--------------------------------------- | :---------------------------- | :------------------------------------------------------ |
|  [01]   | `S3Store`                                | class                         | `DataStore` over S3 multipart; the `datastore` slot     |
|  [02]   | `Options`                                | type                          | `S3Store` constructor input, the policy record below    |
|  [03]   | `Options.s3ClientConfig`                 | `S3ClientConfig & { bucket }` | endpoint, credentials, `forcePathStyle`, staging bucket |
|  [04]   | `Options.partSize`                       | number                        | preferred part bytes, 5 MiB to 5 GiB                    |
|  [05]   | `Options.minPartSize`                    | number                        | uniform-part floor                                      |
|  [06]   | `Options.maxMultipartParts`              | number                        | S3 part ceiling, default 10 000                         |
|  [07]   | `Options.maxConcurrentPartUploads`       | number                        | part-upload semaphore width per PATCH                   |
|  [08]   | `Options.useTags`                        | boolean                       | completion tags drive tag-based lifecycle               |
|  [09]   | `Options.expirationPeriodInMilliseconds` | number                        | staging TTL for the expiry sweep                        |
|  [10]   | `Options.cache`                          | `KvStore<MetadataValue>`      | fronts `${id}.info` `HeadObject`; in-memory default     |
|  [11]   | `MetadataValue`                          | object                        | staged record `{ file, "upload-id", "tus-version" }`    |
|  [12]   | `S3Store.maxUploadSize`                  | const                         | 5 TiB ceiling (`5497558138880`)                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `DataStore` members `@tus/server` drives, with the finalize and groom reads the rail owns

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------ | :------- | :----------------------------------------------------- |
|  [01]   | `new S3Store(Options)`                      | ctor     | one store per staging band beside the server           |
|  [02]   | `create(Upload) -> Upload`                  | instance | POST — opens the multipart upload, writes `${id}.info` |
|  [03]   | `write(Readable, string, number) -> number` | instance | PATCH — streams parts, returns the new offset          |
|  [04]   | `getUpload(string) -> Upload`               | instance | HEAD — offset from `ListParts` progress                |
|  [05]   | `read(string) -> Readable`                  | instance | the finalize fold's staged byte source                 |
|  [06]   | `remove(string)`                            | instance | staging removal after re-home                          |
|  [07]   | `deleteExpired() -> number`                 | instance | expiry sweep, returns the removed count                |
|  [08]   | `getExpiration() -> number`                 | instance | staging TTL window                                     |
|  [09]   | `declareUploadLength(string, number)`       | instance | tus `Upload-Defer-Length` — size set after creation    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- confirmed offset sums consecutive uploaded parts and any incomplete-part object, so resume never re-uploads past the high-water mark.
- `${id}.info` carries the `Upload` record and `UploadId` in S3 object `Metadata`, read by one `HeadObject` and fronted by `cache`, so the store holds no cross-process state.
- tus ids are random staging keys and the finalize fold mints content identity, never this store; `useTags` and expiration groom what finalize never re-homed.
- `partSize` targets, `minPartSize` floors, and `maxMultipartParts` caps; the store recomputes part size from declared length so a 5 TiB upload still fits 10 000 parts.

[STACKING]:
- `@tus/server`(`.api/tus-server.md`): the store fills the `datastore` slot, and `onUploadFinish` fires after `CompleteMultipartUpload` settles, so the finalize fold reads a whole durable object.
- `@aws-sdk/client-s3`(`.api/aws-sdk-client-s3.md`): `s3ClientConfig` is the object plane's provider vocabulary verbatim — one endpoint and credential set serves content client, presigner, and this store.
- `@aws-sdk/lib-storage`(`.api/aws-sdk-lib-storage.md`): `read(id)` feeds the finalize fold — chunk, digest fold, conditional re-put — and `remove(id)` closes staging once the 200-or-412 lands.
- `effect`: construction sits inside the owning service scope, `read`'s `Readable` lifts at the seam, and expiry sweeps run as scheduled effects, so the store's promises never cross into domain code.

[LOCAL_ADMISSION]:
- staging binds the object plane's provider `Config`, one endpoint and credential vocabulary shared with the content client.
- part arithmetic lives in the `partSize` and `minPartSize` policy fields, computed from declared length rather than hardcoded beside the store.

[RAIL_LAW]:
- Package: `@tus/s3-store`
- Owns: multipart-backed offset persistence, `${id}.info` metadata objects, incomplete-part carry, part-size arithmetic, staging expiry, and the `Options` policy record
- Accept: one store per staging band on the object plane's provider facts, `read` as the finalize byte source, expiry-swept staging, part policy as `Options` fields
- Reject: staging keys as content identity, a second provider vocabulary, an unswept band, driving the store outside the tus server except `read`, `remove`, and `deleteExpired`
