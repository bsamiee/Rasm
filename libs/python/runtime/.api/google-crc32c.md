# [PY_RUNTIME_API_GOOGLE_CRC32C]

`google-crc32c` binds Google's CRC32C (Castagnoli) C library at one runtime seam: the admission cloud-arm integrity fence. Secret Manager's client never self-verifies `SecretPayload.data_crc32c`, so the `cloud_read` fence compares it against `google_crc32c.value(payload.data)` before trusting the payload — a mismatch raises the retried transport `OSError` on the `RetryClass.SECRET` row. Streaming `Checksum` and `extend` stay unconsumed: the payload arrives as one buffer, so the one-shot digest is the whole slice.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `google-crc32c`
- package: `google-crc32c` (Apache-2.0)
- module: `google_crc32c`
- abi: C-extension over Google's CRC32C library; pure-Python fallback where the extension is absent
- rail: settings-secrets

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: digest surface

| [INDEX] | [SURFACE]                  | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `value(data) -> int`       | static  | one-shot CRC32C digest; fence compares against `data_crc32c`       |
|  [02]   | `extend(crc, data) -> int` | static  | resumable digest; `extend(value(a), b) == value(a + b)`            |
|  [03]   | `Checksum(data=b"")`       | ctor    | incremental digest: `update` `digest` `hexdigest` `copy` `consume` |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `value(payload.data) != payload.data_crc32c` is corrupted transport — a retryable transient on the `RetryClass.SECRET` `OSError` target, never a MISS and never a trusted payload; the check runs inside the one `guarded` tier envelope, never a second verification surface.

[STACKING]:
- `google-cloud-secret-manager`(`.api/google-cloud-secret-manager.md`): `value(payload.data)` compares against `SecretPayload.data_crc32c` inside the `cloud_read` fence between `access_secret_version` and a trusted payload; a mismatch is the retried transport fault, never a trusted read.
- within-lib: a module-scope `lazy` import beside the Secret Manager client in the `admission.md` `[03]-[SETTINGS]` prelude, so the digest costs nothing until the gated cloud arm first fires.

[LOCAL_ADMISSION]:
- `value(data) -> int` is the sole admitted entry; `extend` and the streaming `Checksum` stay unconsumed, and admitting either needs a live fence rather than a speculative re-catalog.

[RAIL_LAW]:
- Package: `google-crc32c`
- Owns: the `SecretPayload.data_crc32c` integrity comparison on the admission cloud arm — the one digest between `access_secret_version` and a trusted payload
- Accept: `value(data) -> int` compared against `data_crc32c` inside the `cloud_read` fence
- Reject: a trusted `payload.data` without the digest, a hand-rolled CRC32C, a `Checksum` admission ahead of a live fence, a second integrity owner beside the one cloud-arm check
