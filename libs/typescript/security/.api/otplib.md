# [TS_SECURITY_API_OTPLIB]

`otplib` owns the RFC-4226 HOTP and RFC-6238 TOTP second factor on one strategy-discriminated async rail: `strategy` picks the algorithm as a value, `verify` returns a verdict rather than throwing, and crypto and base32 arrive as injected ports, so `authn/otp` binds the HMAC primitive `sign/crypto` already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `otplib`
- package: `otplib` (MIT)
- module: dual ESM and CJS; subpaths `.` (whole surface) · `./functional` · `./class`
- runtime: `runtime:neutral` — pure JS; the bundled `NobleCryptoPlugin` runs in browser and node, and a port swap drops that dependency
- rail: authn/otp — the second-factor mint and verify owner, taking its HMAC from `sign/crypto` through the `CryptoPlugin` port

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the verdict union and the option algebra both rails share.

`VerifyResult` narrows on `.valid`; `"timeStep" in result` then selects the TOTP arm carrying the RFC-6238 step number and the matched period start. Functional entries take the `*FunctionalOptions` pair carrying `strategy`, `crypto`, and `base32`; class entries take the class pair, whose plugin fields the constructor already bound. `guardrails?` threads every option shape, and `@otplib/core` (`.api/otplib-core.md`) owns its caps and construction.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `OTPStrategy`                | union         | `'totp' \| 'hotp'`, the one dispatch key        |
|  [02]   | `VerifyResult`               | union         | verdict plus `delta`, `timeStep`, `epoch`       |
|  [03]   | `OTPFunctionalOptions`       | interface     | functional generate shape                       |
|  [04]   | `OTPVerifyFunctionalOptions` | interface     | functional verify shape, adds token and window  |
|  [05]   | `OTPGenerateOptions`         | interface     | class-rail generate shape                       |
|  [06]   | `OTPVerifyOptions`           | interface     | class-rail verify shape                         |
|  [07]   | `OTPClassOptions`            | interface     | `OTP` construction shape                        |
|  [08]   | `OTPURIGenerateOptions`      | interface     | `OTP.generateURI` shape                         |
|  [09]   | `TOTPOptions`                | interface     | `TOTP` construction and per-call shape          |
|  [10]   | `OTPAuthOptions`             | interface     | the `crypto` and `base32` port slot             |
|  [11]   | `HashAlgorithm`              | union         | `sha1`, `sha256`, `sha512` as a value           |
|  [12]   | `OTPResult`                  | union         | ok-or-error carrier the `wrapResult` pair mints |

[PUBLIC_TYPE_SCOPE]: the plugin ports — the swap point keeping crypto shared rather than siloed.

`CryptoPlugin` and `Base32Plugin` are structural, so a plain object literal satisfies either and `sign/crypto` crosses in as `{ crypto }` with no factory import. `CryptoPlugin.hmac` returns bytes or a promise of bytes, which is why the primary entries are async and the `*Sync` mirror admits only a synchronous implementation. `@otplib/core` owns the named factories and the `OTPHooks` variant shapes reached through `hooks?`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `CryptoPlugin`      | interface     | `hmac`, `randomBytes`, `constantTimeEqual` port |
|  [02]   | `Base32Plugin`      | interface     | `encode`, `decode` port                         |
|  [03]   | `NobleCryptoPlugin` | class         | bundled `@noble/hashes` crypto default          |
|  [04]   | `ScureBase32Plugin` | class         | bundled `@scure/base32` base32 default          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the functional rail — one strategy-discriminated entry with a sync mirror.

HOTP requires `counter`; TOTP reads `period`, `epoch`, and `t0`. `generateURI` requires `issuer`, `label`, and `secret`, taking `strategy`, `algorithm`, `digits`, `period`, and `counter` as options.

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------ | :------ | :------------------------------------------ |
|  [01]   | `generate(OTPFunctionalOptions) -> Promise<string>`           | static  | mint a token on the selected strategy       |
|  [02]   | `generateSync(OTPFunctionalOptions) -> string`                | static  | sync mint; throws `HMACError` without it    |
|  [03]   | `verify(OTPVerifyFunctionalOptions) -> Promise<VerifyResult>` | static  | constant-time check across the window       |
|  [04]   | `verifySync(OTPVerifyFunctionalOptions) -> VerifyResult`      | static  | sync mirror of the check                    |
|  [05]   | `generateSecret({ length?, crypto?, base32? }) -> string`     | static  | Base32 enrollment secret, 20 bytes default  |
|  [06]   | `generateURI(options) -> string`                              | static  | `otpauth://` URI for QR enrollment          |
|  [07]   | `stringToBytes(string) -> Uint8Array`                         | static  | raw string to plugin input bytes            |
|  [08]   | `wrapResult(fn) -> (...Args) => OTPResult`                    | static  | lift a sync entry to a never-throw result   |
|  [09]   | `wrapResultAsync(fn) -> (...Args) => Promise<OTPResult>`      | static  | lift an async entry to a never-throw result |

[ENTRYPOINT_SCOPE]: the class rail — plugins and guardrails bound once per subject.

`OTP` keeps `strategy` dynamic; `TOTP` and `HOTP` fix it and tighten each signature to that algorithm, so HOTP takes its counter positionally and TOTP takes its token positionally. `HOTPOptions` and `TOTPVerifyOptions` reach only through the `@otplib/hotp` and `@otplib/totp` sub-imports.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `new OTP(OTPClassOptions)`                                                       | ctor     | bind strategy, plugins, guardrails once |
|  [02]   | `OTP.getStrategy() -> OTPStrategy`                                               | instance | read the bound discriminant             |
|  [03]   | `OTP.generateSecret(number) -> string`                                           | instance | Base32 secret at the bound plugins      |
|  [04]   | `OTP.generate(OTPGenerateOptions) -> Promise<string>`                            | instance | mint on the bound strategy              |
|  [05]   | `OTP.generateSync(OTPGenerateOptions) -> string`                                 | instance | sync mint                               |
|  [06]   | `OTP.verify(OTPVerifyOptions) -> Promise<VerifyResult>`                          | instance | check on the bound strategy             |
|  [07]   | `OTP.verifySync(OTPVerifyOptions) -> VerifyResult`                               | instance | sync check                              |
|  [08]   | `OTP.generateURI(OTPURIGenerateOptions) -> string`                               | instance | provisioning URI on the bound strategy  |
|  [09]   | `new TOTP(TOTPOptions)`                                                          | ctor     | fix the time-based algorithm            |
|  [10]   | `TOTP.generateSecret() -> string`                                                | instance | Base32 secret at the bound plugins      |
|  [11]   | `TOTP.generate(Partial<TOTPOptions>) -> Promise<string>`                         | instance | mint against the bound defaults         |
|  [12]   | `TOTP.verify(string, Partial<TOTPVerifyOptions>) -> Promise<VerifyResult>`       | instance | token first, overrides second           |
|  [13]   | `TOTP.toURI({ label?, issuer?, secret? }) -> string`                             | instance | override the bound URI fields           |
|  [14]   | `new HOTP(HOTPOptions)`                                                          | ctor     | fix the counter-based algorithm         |
|  [15]   | `HOTP.generateSecret() -> string`                                                | instance | Base32 secret at the bound plugins      |
|  [16]   | `HOTP.generate(number, Partial<HOTPOptions>) -> Promise<string>`                 | instance | counter is the first argument           |
|  [17]   | `HOTP.verify({ token, counter }, Partial<HOTPOptions>) -> Promise<VerifyResult>` | instance | tolerance rides the second argument     |
|  [18]   | `HOTP.toURI(number) -> string`                                                   | instance | provisioning URI at a counter           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `strategy` rides the options as a value, so `generate` and `verify` each stay one entry and `TOTP`/`HOTP` are its fixed specializations rather than a name fork.
- Tolerance is data: `epochTolerance` counts seconds and `counterTolerance` counts steps, each a `number` for a symmetric window or `[past, future]` for an asymmetric one — RFC-compliant TOTP is `[N, 0]` and HOTP look-ahead is `[0, N]`.
- `verify` returns a verdict and compares in constant time; `delta` on the valid arm resyncs an HOTP counter to `counter + delta + 1` and measures TOTP clock drift.
- `afterTimeStep` rejects a TOTP token whose `timeStep` sits at or below the persisted floor, so replay defence is a verify option rather than a caller-side re-check.

[STACKING]:
- `@oslojs/crypto`(`.api/oslojs-crypto.md`): a plain `{ name, hmac, randomBytes, constantTimeEqual }` object over `hmac`, `constantTimeEqual`, and the WebCrypto-filled `RandomReader` satisfies `CryptoPlugin`, and `authn/otp` passes it as `{ crypto }` — one HMAC primitive for the folder, the bundled `@noble/hashes` path bypassed.
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): `encodeBase32UpperCaseNoPadding` and `decodeBase32` satisfy `Base32Plugin.encode` and `.decode`, holding every base32 rendering on one owner.
- `@otplib/core`(`.api/otplib-core.md`): sub-import reaches `createCryptoPlugin` and `createBase32Plugin` for named port construction, `OTPHooks` for a non-numeric alphabet through `hooks?`, and the guardrail caps behind `guardrails?`.
- `@node-rs/argon2`(`.api/node-rs-argon2.md`): recovery codes sit outside this package — `generateSecret` or `generateRandomString` mints them and argon2 `hash` digests them at rest.
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts the async entries and `Effect.try` the `*Sync` pair; `Match.value(result)` with `Match.when({ valid: true }, …)` and `Match.exhaustive` folds the verdict, and `secret` stays `Redacted<string>` until `Redacted.value` feeds the call.
- `authn/credential` (in-folder owner): the OTP rows share the mint-and-resolve digest idiom with machine keys, and `authn/otp` reaches HMAC only through the `CryptoPlugin` object `sign/crypto` constructs.

[LOCAL_ADMISSION]:
- `authn/` subpaths import this package; the `tests/typescript/_architecture` audit catches a `sign`, `session`, or `secret` rail reaching it.
- `authn/otp` binds `{ crypto }` to the `sign/crypto`-backed plugin at every entry, keeping one HMAC owner for the folder.
- Functional `generate` and `verify` carry the default; a class earns its place where plugin and guardrail config is fixed per subject and reused across calls.

[RAIL_LAW]:
- Package: `otplib`
- Owns: RFC-4226 HOTP and RFC-6238 TOTP mint and verify, the `VerifyResult` verdict with `delta` resync, `epoch` and `counter` tolerance windows, `generateSecret`/`generateURI` enrollment, and the `CryptoPlugin`/`Base32Plugin` ports
- Accept: `strategy` as the one discriminant, the async entries under `Effect.tryPromise`, `delta` for HOTP resync, tolerance as `number | [past, future]`, `afterTimeStep` as the replay floor, `{ crypto }` bound to a `sign/crypto` plugin, `hooks` for a non-standard alphabet
- Reject: a `generateTotp`/`generateHotp` name fork, an enumerated tolerance-variant family, a caller-side replay re-check, a second HMAC stack beside the `CryptoPlugin` port, recovery codes claimed as an otplib row, an import outside `authn/`
