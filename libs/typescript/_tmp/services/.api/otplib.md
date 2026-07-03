# [API_CATALOGUE] otplib

`otplib` at v13 is a ground-up rewrite — the v12 `authenticator`/`totp`/`hotp` singletons, their `.check()`/`.keyuri()` methods, and the `HashAlgorithms`/`KeyEncodings` enum objects are **gone**. The surface is now an options-object, async-first, result-typed rail with one strategy discriminant (`OTPStrategy = 'totp' | 'hotp'`) over a plugin substrate: crypto and Base32 are **ports** (`CryptoPlugin`/`Base32Plugin`), so HMAC and secret encoding are injectable rather than a closed silo. `generate`/`verify` mint and check tokens, `generateSecret`/`generateURI` drive enrolment, and the discriminated `VerifyResult` narrows on `valid` — its TOTP arm carrying `timeStep`/`epoch`/`delta` and its HOTP arm carrying `delta`, so `delta` resyncs an HOTP counter (`counter + delta + 1`) or detects TOTP clock drift while `timeStep` is the replay floor. In `services` it is the TOTP/HOTP arm of `security/auth.md`'s one `Authn` `Effect.Service`, sitting beside the `@simplewebauthn/server` WebAuthn arm: the async functional entries are lifted through one `Effect.tryPromise` (rejection → `AuthFault`) inside the `EnrolTotp`/`VerifyTotp` cases of the `AuthCommand.$match` dispatch, the per-user `MfaSecret` Base32 secret is the `@effect/sql-pg` stored row (`Model.Sensitive`), and `verify`'s `result.timeStep` persists as the `afterTimeStep` replay floor so a reused time-step is rejected. A browser-side `otplib` verifier is the named defect — the browser collects the code, this server owner verifies it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `otplib`
- package: `otplib` (13.4.1, MIT) — metapackage re-exporting `@otplib/core`/`@otplib/totp`/`@otplib/hotp`/`@otplib/uri` plus the default `@otplib/plugin-crypto-noble`/`@otplib/plugin-base32-scure`
- module format: dual ESM (`import` → `dist/index.js`) + CJS (`require` → `dist/index.cjs`); subpaths `.` (barrel — functional + class + substrate re-exports) · `./functional` · `./class`; self-typed `.d.ts`/`.d.cts` beside each `.js`, no `@types`
- reflected: TSDECL — `node_modules/otplib/dist/{index,functional,class,types-BBT_82HF}.d.ts` + `node_modules/.pnpm/@otplib+{core,totp,hotp,plugin-crypto-noble,plugin-base32-scure}@13.4.1/…/dist/*.d.ts`
- runtime target: runtime-neutral / isomorphic — pure JS; the default `NobleCryptoPlugin` (`@noble/hashes`, **synchronous** HMAC) + `ScureBase32Plugin` (`@scure/base`) run on Node, Deno, Bun, and Workers; swapping the port drops even those dependencies
- ABI: `secret` is a Base32 `string | Uint8Array` (a string is Base32-decoded through the `Base32Plugin`); HMAC rides the `CryptoPlugin` port (sync or `Promise`-returning); `VerifyResult` is a `valid`-discriminated union (TOTP arm `{valid:true; delta; epoch; timeStep}`, HOTP arm `{valid:true; delta}`, both `{valid:false}`); `HashAlgorithm` is the `'sha1'|'sha256'|'sha512'` union (not an enum object); tolerance is `number | [past, future]`
- consumer: `security/auth.md#VERIFIER` — the `EnrolTotp`/`VerifyTotp` `AuthCommand` arms under the one `Authn` `Effect.Service`, `MfaSecret` the `@effect/sql-pg` stored row (Base32 secret as `Model.Sensitive`), `afterTimeStep` the persisted TOTP replay floor read from `result.timeStep`
- rail: auth / totp+hotp
- LEGACY_BAN: no v12 `authenticator`/`totp`/`hotp` *singletons*, no `.check()`, no `.keyuri()` (the v13 name is `generateURI`), no `HashAlgorithms`/`KeyEncodings` enum objects — a design page citing the v12 API is a phantom

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the strategy discriminant, the option algebra, the discriminated result
- rail: auth
- `OTPStrategy` is the one dispatch key; options are additive (verify extends generate with `token` + tolerance + `afterTimeStep`); `VerifyResult` is a `valid`-tagged union whose extra fields exist only on the `valid: true` arm — narrow with `if (result.valid)` and, for the TOTP-only fields, `"timeStep" in result`.

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY]       | [CAPABILITY]                                                                     |
| :-----: | :------------------------------------------------------------------------- | :------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `OTPStrategy = 'totp' \| 'hotp'`                                            | strategy value      | the one discriminant; `generate`/`verify`/`generateURI`/`OTP` route on it as a value, never a name fork |
|  [02]   | `VerifyResult`                                                              | discriminated union | `{valid:true; delta; epoch; timeStep}` (TOTP) \| `{valid:true; delta}` (HOTP) \| `{valid:false}`; `delta` → HOTP resync (`counter+delta+1`) / TOTP drift, `timeStep`/`epoch` → TOTP replay floor + matched period start (TOTP arm only) |
|  [03]   | `OTPFunctionalOptions` / `OTPVerifyFunctionalOptions`                       | option algebra      | root functional-entry option shapes: `secret`, `strategy`, `algorithm`, `digits`, `period`/`epoch`/`t0` (TOTP), `counter` (HOTP), `crypto`/`base32`/`guardrails`/`hooks`; verify adds `token`, `epochTolerance`/`counterTolerance`, `afterTimeStep` |
|  [04]   | `OTPAuthOptions` `{ crypto?: CryptoPlugin; base32?: Base32Plugin }`         | plugin override     | the readonly port-injection slot carried on every entry                          |
|  [05]   | `OTPClassOptions` / `OTPGenerateOptions` / `OTPVerifyOptions` / `OTPURIGenerateOptions` | class option algebra | `OTP`-class shapes (strategy fixed on the instance, absent from the per-call options); `OTPClassOptions` carries `{strategy?, crypto?, base32?, guardrails?}` |
|  [06]   | `TOTPOptions`                                                               | type alias          | the fully-optional `TOTP`-class config (`secret`/`epoch`/`t0`/`period`/`algorithm`/`digits`/`crypto`/`base32`/`issuer`/`label`/`guardrails`/`hooks`) |
|  [07]   | `HashAlgorithm = 'sha1' \| 'sha256' \| 'sha512'` · `Digits = number` (`@otplib/core`) | primitive params    | authenticator-compat defaults (`sha1`, 6 digits); raise per policy — `Digits` is unbranded, bounded by guardrails at runtime |

[PUBLIC_TYPE_SCOPE]: the plugin ports, the bundled defaults, the `@otplib/core` construction substrate
- rail: auth
- `CryptoPlugin`/`Base32Plugin` are structural ports exported from the `otplib` root — satisfy one with a plain object literal, no factory needed. The `create*Plugin` factories, the branded/guard utility types, `OTPHooks`, and `SecretOptions` live in `@otplib/core` (the transitively-present substrate `otplib` re-exports selectively); reach for them by `@otplib/core` subimport only when named construction beats a literal or a shared crypto owner is admitted.

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY]        | [CAPABILITY]                                                                     |
| :-----: | :------------------------------------------------------------------------------- | :------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `CryptoPlugin { name; hmac(alg,key,data); randomBytes(len); constantTimeEqual(a,b) }` | structural port (root) | the crypto swap point; `hmac` returns `Uint8Array \| Promise<Uint8Array>` (sync or WebCrypto); satisfy with a literal to bind HMAC to a shared owner |
|  [02]   | `Base32Plugin { name; encode(data,opts?); decode(str) }`                          | structural port (root) | the Base32 swap point; `encode` takes `{ padding? }` (default unpadded, Google-Authenticator compatible) |
|  [03]   | `NobleCryptoPlugin` / `ScureBase32Plugin`                                         | class (root)         | the bundled defaults (`@noble/hashes` / `@scure/base`); `new NobleCryptoPlugin()` HMAC is **synchronous**, so `generateSync`/`verifySync` are admitted with it |
|  [04]   | `createCryptoPlugin(opts)` / `createBase32Plugin(opts)` (`@otplib/core`)          | factory (subimport)  | named port construction from `CreateCryptoPluginOptions {name?, hmac, randomBytes, constantTimeEqual?}` / `CreateBase32PluginOptions {name?, encode, decode}`; optional — a plain object satisfies the port |
|  [05]   | `OTPHooks { encodeToken?; validateToken?; truncateDigest? }` (`@otplib/core`)     | hook slot (subimport) | non-standard OTP (Steam Guard alphabet, custom truncation) via the `hooks?` option field; the variant is a hook value, never a package fork |
|  [06]   | `OTPGuardrails` / `OTPGuardrailsConfig` / `createGuardrails(cfg)` (root)          | validation caps      | bound secret bytes / period / digits / verification window per policy; caps evidenced by `@otplib/core` `MIN_SECRET_BYTES`/`MAX_SECRET_BYTES`/`MIN_PERIOD`/`MAX_PERIOD`/`MAX_WINDOW`/`MAX_COUNTER`/`RECOMMENDED_SECRET_BYTES` |
|  [07]   | `OTPResult<T,E>` / `wrapResult` / `wrapResultAsync` (root) · `stringToBytes` (root) | never-throw wrap     | optional tagged-result (`{ok:true; value} \| {ok:false; error}`) wrapping of an entry; `stringToBytes` converts a raw passphrase to `Uint8Array` when the secret is not Base32 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the functional rail — one strategy-discriminated entry (async + sync mirror)
- rail: auth
- `strategy` selects TOTP vs HOTP as a *value*; the same call shape serves both (HOTP requires `counter`, TOTP uses `period`/`epoch`/`t0`). Prefer `generate`/`verify` (async, plugin-agnostic); the `*Sync` mirror needs a sync-HMAC plugin (the default `NobleCryptoPlugin` is one).

| [INDEX] | [SURFACE]                                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `generate(options: OTPFunctionalOptions): Promise<string>` / `generateSync(...): string`     | mint token     | `authn/otp` challenge issuance; `Effect.tryPromise` (async) / `Effect.try` (sync, throws `HMACError` without a sync plugin) |
|  [02]   | `verify(options: OTPVerifyFunctionalOptions): Promise<VerifyResult>` / `verifySync(...): VerifyResult` | check token    | second-factor verify; constant-time internally, returns `{valid:false}` (never throws) on a wrong code; narrow `.valid` then read `delta`/`timeStep` |
|  [03]   | `generateSecret(options?: { length?; crypto?; base32? }): string`                            | mint secret    | Base32 shared secret at enrolment (default 20 bytes / 160-bit); the `MfaSecret.secret` row value |
|  [04]   | `generateURI({ strategy?, issuer, label, secret, algorithm?, digits?, period?, counter? }): string` | provisioning   | `otpauth://` URI for QR enrolment at the `ui` edge; the v13 replacement for v12 `.keyuri()` |

[ENTRYPOINT_SCOPE]: the class rail — strategy-fixed specializations over the same options
- rail: auth
- `OTP` is the strategy-dynamic wrapper (constructed once with `{strategy, crypto, base32, guardrails}`); `TOTP`/`HOTP` are the fixed-strategy classes with tighter per-strategy signatures. Use a class when the plugin/guardrail config is fixed for a subject and reused across calls; prefer the functional entries otherwise.

| [INDEX] | [SURFACE]                                                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `new OTP(OTPClassOptions)` → `.getStrategy()` · `.generateSecret(length?)` · `.generate`/`.generateSync` · `.verify`/`.verifySync` · `.generateURI` | unified class  | pre-bound plugin/guardrail config reused per subject; the per-call options omit `strategy` (fixed on the instance) |
|  [02]   | `new TOTP(TOTPOptions)` → `.generate(opts?)` · `.verify(token, opts?)` · `.generateSecret()` · `.toURI(opts?)` | TOTP class     | time-based codes; `@otplib/totp` module fns `getRemainingTime()`/`getTimeStepUsed()` (subpath, not root) drive the seconds-left UI |
|  [03]   | `new HOTP(HOTPOptions)` → `.generate(counter, opts?)` · `.verify({token, counter}, opts?)` · `.generateSecret()` · `.toURI(counter?)` | HOTP class     | counter-based codes; `counter` is `number \| bigint`; `delta` resync                |

## [04]-[IMPLEMENTATION_LAW]

[OTP_TOPOLOGY]:
- one strategy discriminant: `'totp' | 'hotp'` is a value on the options/constructor, not two APIs. `generate({ strategy })`/`verify({ strategy })` is the polymorphic entry; `TOTP`/`HOTP` are its strategy-fixed specializations. Never fork a `generateTotp`/`generateHotp` pair.
- `VerifyResult` narrowing: `delta` exists on both valid arms; `timeStep`/`epoch` exist only on the TOTP valid arm. Guard `if (result.valid)`, then discriminate the TOTP fields with `"timeStep" in result` before reading them — the `services/auth.md` `VerifyTotp` arm does exactly this to persist the replay floor.
- parameterized window: `epochTolerance` (TOTP) and `counterTolerance` (HOTP) are `number` (symmetric) or `[past, future]` (asymmetric). RFC-compliant past-only TOTP is `[N, 0]`; HOTP look-ahead is `[0, N]`. The window is data, never an enumerated set of verify variants.
- result-typed, constant-time: `verify` returns `VerifyResult`, never throws on a wrong code; comparison is constant-time internally. `delta` on an HOTP match is the resync signal — persist `counter + delta + 1`; a nonzero TOTP `delta` is clock drift; `timeStep` is the replay floor.
- async-first: `generate`/`verify` return `Promise` because `CryptoPlugin.hmac` may be async (WebCrypto). The `*Sync` mirror throws `HMACError` unless the bound crypto plugin exposes synchronous HMAC — the default `NobleCryptoPlugin` does, so both mirrors are usable node-side.

[STACKING_LAW]:
- Each async entry is one `Effect.tryPromise({ try: () => verify/generate/generateSecret/generateURI(opts), catch: (cause) => new AuthFault({ reason: "invalid_totp", detail: String(cause) }) })` — a `Data.TaggedError`/`Schema.TaggedError` rail (`AuthFault`), never a thrown exception in domain code; the `EnrolTotp`/`VerifyTotp` lifts are two arms of `security/auth.md`'s `AuthCommand.$match`, beside the `@simplewebauthn/server` `Verify/BeginWebauthn*` arms under the one `Authn` `Effect.Service` (`.api/simplewebauthn-server.md` is the paired WebAuthn catalogue completing the pair).
- `verify` result flows straight into the replay guard: narrow `result.valid && "timeStep" in result`, compare `result.timeStep` against the persisted `MfaSecret.afterTimeStep` floor (`timeStep <= floor` → `AuthFault{reason:"replayed"}`), and write the new `timeStep` back through the same `@effect/sql-pg` `SqlClient` UPDATE inside the verify effect.
- `MfaSecret` is the stored row over `persistence/store#STORE_BOUNDARY`: the Base32 `secret` is `Model.Sensitive` (`Redacted<string>`) at rest, unwrapped only at the `verify`/`generateURI` call boundary. Enrolment (`EnrolTotp`) mints the secret through `generateSecret` and the QR URI through `generateURI({ strategy: "totp", issuer, label, secret })`, both kept `Redacted` until the QR render at the `ui` edge.
- plugin binding: the folder default is `NobleCryptoPlugin`/`ScureBase32Plugin` (applied internally when `crypto`/`base32` are omitted). The `CryptoPlugin`/`Base32Plugin` ports are the swap point — pass `{ crypto }`/`{ base32 }` (a plain object literal or a `createCryptoPlugin`/`createBase32Plugin` result) to bind HMAC/Base32 to a shared crypto owner instead of the bundled default, keeping one HMAC owner per folder when one is admitted.
- variants without a fork: `OTPHooks.encodeToken`/`truncateDigest` implement Steam Guard and other non-numeric alphabets through the `hooks?` option field — a hook value, not a new package or a sibling entry.
- recovery/backup codes are *not* an otplib feature: mint them from `generateSecret` (or a CSPRNG), digest at rest through the folder's password-hash owner, and constant-time-compare — otplib owns only the TOTP/HOTP second-factor rows.

[RAIL_LAW]:
- Package: `otplib` (+ `@otplib/core` subimport for the plugin factories, hooks, and utility types)
- Owns: RFC-4226 HOTP + RFC-6238 TOTP mint/verify, the `valid`-discriminated `VerifyResult` (`delta` resync + TOTP `timeStep`/`epoch`), parameterized `epochTolerance`/`counterTolerance` windows, `afterTimeStep` replay floor, `generateSecret`/`generateURI` enrolment, `guardrails`/`hooks`, and the `CryptoPlugin`/`Base32Plugin` ports with their `NobleCryptoPlugin`/`ScureBase32Plugin` defaults
- Accept: `strategy` as the one discriminant, `generate`/`verify` async entries under `Effect.tryPromise` → `AuthFault`, `VerifyResult` narrowed on `valid` then `"timeStep" in result`, `delta` for HOTP resync, tolerance as `number | [past, future]`, `MfaSecret` `Model.Sensitive` secret unwrapped only at the call, `hooks` for non-standard variants, the port swap to a shared crypto owner
- Reject: the v12 `authenticator`/`totp`/`hotp` singletons, `.check()`, `.keyuri()`, and the `HashAlgorithms`/`KeyEncodings` enum objects (all phantoms); a `generateTotp`/`generateHotp` name fork; throwing on a wrong code (it returns `{valid:false}`); reading `delta`/`timeStep` without the `valid`/`"timeStep" in result` guard; an enumerated tolerance-variant family; a verification that skips the `afterTimeStep` replay advance; a browser-side verifier (this is the server concern the ceremony forwards to); treating recovery codes as an otplib feature; a sibling verifier method per credential where `AuthCommand.$match` discriminates
