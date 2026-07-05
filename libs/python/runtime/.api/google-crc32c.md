# [PY_RUNTIME_API_GOOGLE_CRC32C]

`google-crc32c` binds the Google CRC32C (Castagnoli) C library at one runtime seam only: the `execution/admission#SETTINGS` cloud-arm integrity fence. The Secret Manager client does NOT self-verify `SecretPayload.data_crc32c`; the `_probe` `cloud_read` fence compares it against `google_crc32c.value(payload.data)` before trusting the payload, a mismatch raising the retried transport `OSError` under the `RetryClass.SECRET` row. The streaming `Checksum`/`extend` surface is unconsumed in runtime — the payload arrives as one buffer, so the one-shot digest is the whole slice.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `google-crc32c`
- package: `google-crc32c`
- import: `import google_crc32c`
- owner: `runtime`
- rail: settings-secrets
- version: `1.8.0`
- license: `Apache-2.0`
- asset: C-extension wrapper over the Google CRC32C library (`implementation` reports `"c"`; the pure-Python fallback engages only where the extension is absent)
- capability: one-shot CRC32C digest proving `SecretPayload.data_crc32c` transport integrity on the admission cloud arm

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot digest
- rail: settings-secrets

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                                                                    |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------------------------------------ |
|  [01]   | `value(data: bytes) -> int`       | digest         | CRC32C checksum of a buffer — the one member the cloud-arm fence compares against `data_crc32c` |
|  [02]   | `extend(crc: int, data: bytes) -> int` | digest    | continue a checksum over an appended buffer; `extend(value(a), b) == value(a + b)` |
|  [03]   | `Checksum(data=b"")`              | streaming      | hashlib-shaped incremental digest (`update`/`digest`/`hexdigest`/`copy`/`consume`) — unconsumed; the secret payload is one buffer |

## [03]-[IMPLEMENTATION_LAW]

[INTEGRITY_TOPOLOGY]:
- fence law: `google_crc32c.value(payload.data) != payload.data_crc32c` is corrupted transport — a retryable transient in the `RetryClass.SECRET` row's `OSError` target, never a MISS and never a silently-trusted payload; the check runs inside the one `guarded` tier envelope, never a second verification surface.
- lazy law: the import is a module-scope `lazy` binding beside the Secret Manager client imports (`admission.md` `[03]-[SETTINGS]` prelude) — the digest costs nothing until the gated cloud arm first fires.
- non-admission law: `extend` and the streaming `Checksum` are UNCONSUMED in runtime; admitting either requires a live fence, not a speculative re-catalog.

[RAIL_LAW]:
- Package: `google-crc32c`
- Owns: the `SecretPayload.data_crc32c` integrity comparison on the admission cloud arm — the one digest between `access_secret_version` and a trusted payload
- Accept: `value(data) -> int` compared against `data_crc32c` inside the `cloud_read` fence
- Reject: trusting `payload.data` without the digest, a hand-rolled CRC32C, a streaming `Checksum` admission ahead of a live fence, a second integrity owner beside the one cloud-arm check
