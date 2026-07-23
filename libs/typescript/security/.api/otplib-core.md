# [TS_SECURITY_API_OTPLIB_CORE]

`@otplib/core` is the interfaces-and-primitives package the `otplib` metapackage builds on, pinned directly so the credential owner types against it without riding the metapackage's re-export selection. It carries the pieces `otplib` root does NOT re-export in full: the `OTPHooks` variant surface (`encodeToken`/`validateToken`/`truncateDigest` — Steam Guard alphabets and custom truncation without forking the rail), the named plugin factories (`createCryptoPlugin`/`createBase32Plugin`) behind the structural `CryptoPlugin`/`Base32Plugin` ports, the guardrail constant caps and `createGuardrails`, the RFC-4226 primitive kernel (`dynamicTruncate`, `counterToBytes`, `truncateDigits`, `constantTimeEqual`), and the full typed error taxonomy under the `OTPError` root. The strategy rail itself — `generate`/`verify`, `VerifyResult`, the option algebra — is the `otplib` root's (`.api/otplib.md`); this catalogue covers only the substrate reached by direct sub-import.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@otplib/core`
- package: `@otplib/core` (MIT)
- deps: none — pure TS substrate, no crypto binding (crypto arrives through the `CryptoPlugin` port)
- module: dual ESM (`import` → `dist/index.js`) + CJS; subpaths `.` (all) · `./errors` · `./utils` · `./types`
- catalog-verdict: KEEP — the direct-pin substrate; `credential.md` imports `OTPHooks` type-only from the root
- runtime: `runtime:neutral` — no host API touched; every primitive is bytes-in/bytes-out
- rail: authn/otp — composed beside `otplib` (`.api/otplib.md` carries the strategy rail and the port-swap ruling)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the hook surface, the factories, and the guardrail algebra
- rail: authn/otp

| [INDEX] | [SYMBOL]                                                                                  | [TYPE_FAMILY]     |
| :-----: | :---------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `OTPHooks { encodeToken?; validateToken?; truncateDigest? }`                              | variant hooks     |
|  [02]   | `createCryptoPlugin({ name?, hmac, randomBytes, constantTimeEqual? })`                    | plugin factory    |
|  [03]   | `createBase32Plugin({ name?, encode, decode })`                                           | plugin factory    |
|  [04]   | `OTPGuardrails` / `OTPGuardrailsConfig` / `createGuardrails(cfg)`                         | guardrail algebra |
|  [05]   | `Digits` / `HashAlgorithm` / `SecretOptions` / `OTPResult`/`OTPResultOk`/`OTPResultError` | primitive types   |

[CONSUMER_BOUNDARY] per member:
- [01]-[HOOKS]: non-standard OTP dialects through the `hooks?` option field — the one variant seam.
- [02]-[CRYPTO_FACTORY]: named construction of the crypto port; a plain object literal satisfies it equally.
- [03]-[BASE32_FACTORY]: named construction of the base32 port.
- [04]-[GUARDRAILS]: validation caps over the constant floor/ceiling set.
- [05]-[PRIMITIVES]: option-field and result vocabulary shared with the `otplib` root.

[ROSTERS] reached only by direct sub-import:
- [06]-[CONSTANT_CAPS]: `MIN_SECRET_BYTES`/`MAX_SECRET_BYTES`/`RECOMMENDED_SECRET_BYTES`/`MIN_PERIOD`/`MAX_PERIOD`/`DEFAULT_PERIOD`/`MAX_COUNTER`/`MAX_WINDOW` — the guardrail vocabulary, policy values never re-derived literals.
- [07]-[RFC_KERNEL]: `dynamicTruncate`/`truncateDigits`/`counterToBytes`/`constantTimeEqual`/`generateSecret`/`normalizeSecret`/`stringToBytes`/`bytesToString` — the RFC-4226 primitive set, composed only when a hook implementation needs raw truncation.
- [08]-[ERROR_TAXONOMY]: `OTPError` root + `HMACError`/`SecretError`/`TokenError`/`CounterError`/`PeriodError`/`Base32Error`/`CryptoError`/`ConfigurationError` families (`./errors`) — boundary conversion targets, each lifting through `Effect.try` into the credential fault rail.

## [03]-[INTEGRATION]

- Owns: the substrate vocabulary beneath the OTP strategy rail — hooks, ports, factories, guardrail caps, RFC primitives, typed errors.
- Accept: type-only imports for hook and port shapes; value imports of guardrail constants and factories; error classes as `Effect.try` catch discriminants at the credential boundary.
- Reject: re-implementing truncation or secret normalization beside the kernel row; a second guardrail constant set; reaching for `@otplib/totp`/`@otplib/hotp` directly — the strategy rail stays the `otplib` root's.
