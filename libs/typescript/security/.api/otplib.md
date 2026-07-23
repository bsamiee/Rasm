# [TS_SECURITY_API_OTPLIB]

`otplib` at catalog-bound is a ground-up rewrite — the retired `authenticator`/`totp`/`hotp` singletons and their `.generate()`/`.check()` methods are gone. The surface is now an options-object, async-first, result-typed rail with one strategy discriminant (`'totp' | 'hotp'`) and a plugin substrate: crypto and base32 are ports (`CryptoPlugin`/`Base32Plugin`), so `authn/otp` binds HMAC to the *same* primitive `sign/crypto` owns instead of pulling a second crypto stack. It owns the second-factor rows (`generate`/`verify`/`generateSecret`/`generateURI`), the discriminated `VerifyResult` (`{ valid, delta }`) whose `delta` drives HOTP counter resync and TOTP drift detection, parameterized verification windows (`epochTolerance`/`counterTolerance` as `number | [past, future]`), validation `guardrails`, and `hooks` for non-standard variants (Steam Guard). Recovery/backup codes are *not* an otplib feature — they compose `generateSecret` (or the `@oslojs/crypto` RNG) with a `sign/crypto` digest-at-rest.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `otplib`
- package: `otplib` (MIT)
- module: dual ESM (`import` → `dist/index.js`) + CJS (`require` → `dist/index.cjs`); subpaths `.` (all) · `./functional` · `./class`
- effect-boundary: `generate`/`verify` return `Promise` → `Effect.tryPromise({ try, catch })`; `generateSync`/`verifySync` throw `HMACError` → `Effect.try`; `VerifyResult` narrows on `.valid`. `.api/effect.md`
- catalog-verdict: KEEP — the one RFC-4226/6238 TOTP/HOTP owner; the catalog-bound plugin port is what makes it composable with `sign/crypto` rather than a closed crypto silo
- runtime: `runtime:neutral` — pure JS; the default `NobleCryptoPlugin` (`@noble/hashes`) is browser+node, and swapping the port removes even that
- LEGACY_BAN: no `authenticator`, `totp`, `hotp` *singletons*; no `.check()`; a design page citing the catalog-bound API is a phantom

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the discriminated result and the option algebra
- rail: authn/otp
- `OTPStrategy` is the one discriminant. Options are additive: verify extends generate with `token` + tolerance. `VerifyResult` is a tagged-by-boolean union — `delta` exists on both `valid: true` arms; the TOTP arm additionally carries `timeStep`/`epoch`. Narrow `if (result.valid)`, then `"timeStep" in result` for the TOTP-only fields.

| [INDEX] | [SYMBOL]                                                                                               | [TYPE_FAMILY]    |
| :-----: | :----------------------------------------------------------------------------------------------------- | :--------------- |
|  [01]   | `OTPStrategy = 'totp' \| 'hotp'`                                                                       | strategy value   |
|  [02]   | `VerifyResult = { valid: true; delta; epoch; timeStep } \| { valid: true; delta } \| { valid: false }` | tagged result    |
|  [03]   | `OTPFunctionalOptions` / `OTPVerifyFunctionalOptions` · `OTPGenerateOptions` / `OTPVerifyOptions`      | option algebra   |
|  [04]   | `OTPAuthOptions` `{ crypto?: CryptoPlugin; base32?: Base32Plugin }`                                    | plugin override  |
|  [05]   | `HashAlgorithm = 'sha1' \| 'sha256' \| 'sha512'` · `digits` numeric field (default 6)                  | primitive params |

[CONSUMER_BOUNDARY] per member:
- [01]-[STRATEGY]: the one dispatch key; `generate`/`verify`/`OTP` route on it.
- [02]-[RESULT]: `delta` → HOTP counter resync (`counter + delta + 1`) / TOTP drift; TOTP-only `timeStep` → RFC-6238 step number (the `afterTimeStep` replay floor), `epoch` → matched period start.
- [03]-[OPTIONS]: `OTPFunctionalOptions`/`OTPVerifyFunctionalOptions` (functional rail) and `OTPGenerateOptions`/`OTPVerifyOptions` (class rail, NO `strategy`/`crypto`/`base32` fields, and never crossed by a functional `verify`). Fields: `secret`, `strategy`, `algorithm`, `digits`, `period`/`epoch`/`t0` (TOTP), `counter` (HOTP); verify adds `token`, `epochTolerance`/`counterTolerance`, `afterTimeStep` (the native TOTP replay floor rejecting `timeStep <= afterTimeStep`).
- [04]-[PLUGIN_OVERRIDE]: the port-injection slot on every entry.
- [05]-[PARAMS]: authenticator-compat defaults (`sha1`/6-digit); raise per policy — `HashAlgorithm` is root-exported, `digits` defaults to 6.

[PUBLIC_TYPE_SCOPE]: the plugin ports — the swap point that makes crypto shared, not siloed
- rail: authn/otp ← sign/crypto
- `CryptoPlugin`/`Base32Plugin` are structural ports exported from the `otplib` root — satisfy one with a plain object literal, no factory needed. The named `create*Plugin` factories, the `*Context` wrappers, and the `OTPHooks`/`Digits` types live in `@otplib/core` — a direct catalog pin with its own catalogue (`.api/otplib-core.md`); reach for them by sub-import only when named construction beats a literal. This is the seam by which otplib HMAC rides `@oslojs/crypto`.

| [INDEX] | [SYMBOL]                                                                              | [SOURCE]       |
| :-----: | :------------------------------------------------------------------------------------ | :------------- |
|  [01]   | `CryptoPlugin { name; hmac(alg,key,data); randomBytes(len); constantTimeEqual(a,b) }` | `otplib` root  |
|  [02]   | `Base32Plugin { name; encode(data,opts?); decode(str) }`                              | `otplib` root  |
|  [03]   | `NobleCryptoPlugin` / `ScureBase32Plugin`                                             | `otplib` root  |
|  [04]   | `createCryptoPlugin({ name?, hmac, randomBytes, constantTimeEqual? })`                | `@otplib/core` |
|  [05]   | `createBase32Plugin({ name?, encode, decode })`                                       | `@otplib/core` |
|  [06]   | `stringToBytes`                                                                       | `otplib` root  |
|  [07]   | `OTPHooks { encodeToken?; validateToken?; truncateDigest? }`                          | `@otplib/core` |
|  [08]   | `OTPGuardrails` / `OTPGuardrailsConfig` / `createGuardrails(cfg)`                     | `otplib` root  |
|  [09]   | `OTPResult<T,E>` / `wrapResult` / `wrapResultAsync`                                   | `otplib` root  |

[CONSUMER_BOUNDARY] per port:
- [01]-[CRYPTO_PORT]: structural crypto port; satisfied by a `sign/crypto` object over `@oslojs/crypto`.
- [02]-[BASE32_PORT]: structural base32 port; default `ScureBase32Plugin`, swap to `@oslojs/encoding` base32.
- [03]-[DEFAULTS]: the bundled defaults (`@noble/hashes` / `@scure/base32`).
- [04]-[CRYPTO_FACTORY]: named plugin construction — optional; a plain object satisfies the port.
- [05]-[BASE32_FACTORY]: named plugin construction — optional; a plain object satisfies the port.
- [06]-[STRING_BYTES]: the string→bytes helper for hand-built plugin input.
- [07]-[HOOKS]: non-standard OTP (Steam Guard alphabet, custom truncation); reached via the `hooks?` option field.
- [08]-[GUARDRAILS]: validation caps — `Partial<OTPGuardrailsConfig>` over `MIN_SECRET_BYTES`/`MAX_SECRET_BYTES`/`MIN_PERIOD`/`MAX_PERIOD`/`MAX_COUNTER`/`MAX_WINDOW` (UPPER_SNAKE keys; no digits guardrail — `digits` is an option field).
- [09]-[RESULT_WRAP]: optional never-throw wrapping of an entry into a tagged result.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the functional rail — the one strategy-discriminated entry (async + sync mirror)
- rail: authn/otp
- `strategy` selects TOTP vs HOTP as a *value*; the same call shape serves both (HOTP requires `counter`, TOTP uses `period`/`epoch`). Prefer `generate`/`verify` (async, plugin-agnostic); the `*Sync` mirror needs a sync-HMAC plugin.

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY] |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `generate(options: OTPGenerateOptions): Promise<string>` / `generateSync(...)`                      | mint token     |
|  [02]   | `verify(options: OTPVerifyOptions): Promise<VerifyResult>` / `verifySync(...)`                      | check token    |
|  [03]   | `generateSecret(options?: { length?; crypto?; base32? }): string`                                   | mint secret    |
|  [04]   | `generateURI({ strategy?, issuer, label, secret, algorithm?, digits?, period?, counter? }): string` | provisioning   |

[CONSUMER_BOUNDARY] per entry:
- [01]-[MINT_TOKEN]: `authn/otp` challenge issuance; `Effect.tryPromise`/`Effect.try`.
- [02]-[CHECK_TOKEN]: second-factor verify; constant-time internally; narrow `.valid`.
- [03]-[MINT_SECRET]: Base32 shared secret at enrollment (default 20 bytes/160-bit).
- [04]-[PROVISION]: `otpauth://` URI for QR enrollment at the `ui` edge.

[ENTRYPOINT_SCOPE]: the class rail — strategy-fixed specializations over the same options
- rail: authn/otp
- `OTP` is the strategy-dynamic wrapper (constructed once with `{ strategy, crypto, base32, guardrails }`); `TOTP`/`HOTP` are the fixed-strategy classes with the tighter per-strategy signatures. Use a class when the plugin/guardrail config is fixed for a subject and reused across calls.

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] |
| :-----: | :------------------------------------------------------ | :------------- |
|  [01]   | `new OTP({ strategy?, crypto?, base32?, guardrails? })` | unified class  |
|  [02]   | `new TOTP(options)`                                     | TOTP class     |
|  [03]   | `new HOTP(options)`                                     | HOTP class     |

[CONSUMER_BOUNDARY] per class:
- [01]-[UNIFIED]: `.generate`/`.verify`/`.generateSecret`/`.generateURI`/`.getStrategy`; pre-bound plugin/guardrail config reused per subject.
- [02]-[TOTP]: `.generate`/`.verify(token, opts?)` · `getRemainingTime()` / `getTimeStepUsed()`; time-window UI (seconds-left), `afterTimeStep` replay guard.
- [03]-[HOTP]: `.generate(counter, opts?)` / `.verify({ token, counter, counterTolerance? })` / `.toURI(counter?)`; counter-based codes, `delta` resync.

## [04]-[IMPLEMENTATION_LAW]

[OTP_TOPOLOGY]:
- one strategy discriminant: `'totp' | 'hotp'` is a value on the options/constructor, not two APIs. `generate({ strategy })` is the polymorphic entry; `TOTP`/`HOTP` are its strategy-fixed specializations. Never fork a `generateTotp`/`generateHotp` pair.
- parameterized window: `epochTolerance` (TOTP, in SECONDS — one prior 30s step is `[30, 0]`, never `[1, 0]`) and `counterTolerance` (HOTP, in counter steps) are `number` (symmetric) or `[past, future]` (asymmetric). RFC-compliant past-only TOTP is `[N, 0]`; HOTP look-ahead is `[0, N]`. The window is data, never an enumerated set of verify variants; the TOTP replay floor is the native `afterTimeStep` verify option, never a caller-side re-check.
- result-typed, constant-time: `verify` returns `VerifyResult`, never throws on a wrong code; comparison is constant-time internally. `delta` on the valid arm is the resync signal — persist `counter + delta + 1` after an HOTP match; nonzero TOTP `delta` is clock drift. The TOTP valid arm additionally carries `timeStep` (the RFC-6238 step number — the `afterTimeStep` replay floor) and `epoch` (matched period start).
- async-first: `generate`/`verify` are `Promise`-returning because the `CryptoPlugin.hmac` may be async (WebCrypto). The `*Sync` mirror is admitted only with a sync-HMAC plugin and throws `HMACError` otherwise.

[INTEGRATION_LAW]:
- Stack with `@oslojs/crypto` via the `CryptoPlugin` port (`.api/oslojs-crypto.md`): `sign/crypto` satisfies the structural `CryptoPlugin` with a plain object `{ name: 'rasm', hmac, randomBytes, constantTimeEqual }` over `@oslojs/crypto` `hmac`/`constantTimeEqual` (+ the WebCrypto-filled `RandomReader`) — no factory import needed — and `authn/otp` passes it as `{ crypto }`. TOTP/HOTP HMAC then rides the one primitive `sign` owns — the admission ban holds (oslojs stays in `sign/`; the plugin object crosses the in-folder `authn/otp → sign/crypto` delegation), and the default `@noble/hashes` dependency is bypassed. This mirrors `authn/apikey`'s digest delegation to `sign/crypto`.
- Stack with `.api/effect.md` rails: wrap `verify` in `Effect.tryPromise({ try: () => verify(opts), catch: OtpError })` and dispatch the `VerifyResult` with `Match.value(result).pipe(Match.when({ valid: true }, …), Match.when({ valid: false }, …), Match.exhaustive)` — or a plain `.valid` narrow. The `secret` is `Redacted<string>`; `Redacted.value` unwraps only at the `verify` call. Enrollment secret and `generateURI` output stay `Redacted` until the QR render at the edge.
- Stack with `@oslojs/encoding` base32 (`.api/oslojs-encoding.md`): the enrollment secret is Base32; swap the default `ScureBase32Plugin` for a plain `Base32Plugin` object `{ name: 'oslo', encode: encodeBase32UpperCaseNoPadding, decode: decodeBase32 }` over the encoding sibling to keep base32 on one owner, or keep scure — both satisfy the port. (The encoding import stays in `sign/`; the constructed plugin crosses to `authn/otp`, like the `CryptoPlugin`.)
- Stack with `authn/otp` recovery rows + `@node-rs/argon2`: recovery/backup codes are *not* otplib — they are `generateSecret`/`@oslojs/crypto` `generateRandomString` bytes, digested at rest by `sign/crypto`'s `@node-rs/argon2`, and compared with `constantTimeEqual`. otplib owns only the TOTP/HOTP second-factor rows.
- Stack with `hooks` for variants: `OTPHooks.encodeToken`/`truncateDigest` implement Steam Guard and other non-numeric alphabets without forking the rail — the variant is a hook value, not a new package.

[LOCAL_ADMISSION]:
- imported only inside `authn/` subpaths (the `authn/otp` row); a `sign`/`session`/`secret` rail importing it is the defect the `tests/typescript/_architecture` import audit catches. The crypto primitive still lives in `sign` — otplib receives it as an injected `CryptoPlugin`.
- default plugins are fine for a first cut; the `{ crypto }` swap to the `sign/crypto`-backed plugin is the standing target so the folder has one HMAC owner.
- prefer the functional `generate`/`verify` entries; reach for `OTP`/`TOTP`/`HOTP` only when a plugin/guardrail config is fixed per subject and reused.

[RAIL_LAW]:
- Package: `otplib`
- Owns: RFC-4226 HOTP + RFC-6238 TOTP mint/verify, the discriminated `VerifyResult` with `delta` resync, parameterized `epoch`/`counter` tolerance windows, `generateSecret`/`generateURI` enrollment, `guardrails`/`hooks`, and the `CryptoPlugin`/`Base32Plugin` ports
- Accept: `strategy` as the one discriminant, `generate`/`verify` async entries under `Effect.tryPromise`, `VerifyResult.delta` for HOTP resync, tolerance as `number | [past, future]`, the `{ crypto }` port bound to a `sign/crypto` plugin over `@oslojs/crypto`, `hooks` for non-standard variants
- Reject: the catalog-bound `authenticator`/`totp`/`hotp` singletons and `.check()` (a phantom), a `generateTotp`/`generateHotp` name fork, throwing on a wrong code (it returns `{ valid: false }`), an enumerated tolerance-variant family, a second HMAC/crypto stack when the `CryptoPlugin` port bridges `sign/crypto`, treating recovery codes as an otplib feature, any import outside `authn/`
