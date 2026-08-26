# [PY_RUNTIME_API_GOOGLE_CRC32C]

`google-crc32c` binds Google's CRC32C (Castagnoli) C library at one runtime boundary: the admission cloud-arm integrity fence. Secret Manager's client never self-verifies `SecretPayload.data_crc32c`, so the `CloudVault.read` fence compares it against `google_crc32c.value(payload.data)` before trusting the payload — a mismatch raises `IntegrityError` (the admission-owned `OSError` subclass the `RetryClass.SECRET` target still catches while the fault detail names itself). Streaming `Checksum` and `extend` stay unconsumed: the payload arrives as one buffer, so the one-shot digest is the whole slice.

## [01]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: digest surface

| [INDEX] | [SURFACE]                  | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `value(data) -> int`       | static  | one-shot CRC32C digest; fence compares against `data_crc32c`       |
|  [02]   | `extend(crc, data) -> int` | static  | resumable digest; `extend(value(a), b) == value(a + b)`            |
|  [03]   | `Checksum(data=b"")`       | ctor    | incremental digest: `update` `digest` `hexdigest` `copy` `consume` |

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `value(payload.data) != payload.data_crc32c` is corrupted transport — the admission `IntegrityError(OSError)` the `RetryClass.SECRET` target retries, never a MISS and never a trusted payload; the check runs inside the one `guarded` tier envelope, never a second verification surface.

[STACKING]:
- `google-cloud-secret-manager`(`.api/google-cloud-secret-manager.md`): `value(payload.data)` compares against `SecretPayload.data_crc32c` inside the `CloudVault.read` fence between `access_secret_version` and a trusted payload; a mismatch is the retried `IntegrityError`, never a trusted read.
- within-lib: a module-scope `lazy` import beside the Secret Manager client in the `execution/admission#SETTINGS` prelude, so the digest costs nothing until the gated cloud arm first fires.

[LOCAL_ADMISSION]:
- `value(data) -> int` is the sole admitted entry; `extend` and the streaming `Checksum` stay unconsumed, and admitting either needs a live fence rather than a speculative re-catalog.
