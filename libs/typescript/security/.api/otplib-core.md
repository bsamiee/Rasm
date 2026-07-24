# [TS_SECURITY_API_OTPLIB_CORE]

`@otplib/core` owns the OTP substrate every strategy call folds through: the `OTPHooks` variant seam, the structural `CryptoPlugin`/`Base32Plugin` ports with their factories and context wrappers, the guardrail cap algebra, the RFC-4226 primitive kernel, and the `OTPError` class tree.

Every capability is bytes-in/bytes-out — this package binds no crypto implementation, so the ports carry the algorithm and the entropy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@otplib/core`
- package: `@otplib/core` (MIT)
- module: dual ESM + CJS; subpaths `.` · `./errors` · `./utils` · `./types`
- runtime: `runtime:neutral` — no host API touched, every primitive pure
- rail: authn/otp substrate, pinned directly beneath the `otplib` metapackage

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: hook seam, plugin ports, and the option vocabulary a strategy call types against

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :-------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `OTPHooks`            | variant hooks   | non-numeric dialects without forking the rail    |
|  [02]   | `CryptoPlugin`        | structural port | HMAC, entropy, constant-time compare             |
|  [03]   | `Base32Plugin`        | structural port | secret codec, padding optional                   |
|  [04]   | `CryptoContext`       | port wrapper    | `hmac`/`hmacSync`/`randomBytes` bound once       |
|  [05]   | `Base32Context`       | port wrapper    | `encode`/`decode` bound once                     |
|  [06]   | `OTPGuardrails`       | cap record      | resolved caps carrying an override mark          |
|  [07]   | `OTPGuardrailsConfig` | cap vocabulary  | overridable secret, period, counter, window keys |
|  [08]   | `SecretOptions`       | option record   | port pair and byte length for a secret mint      |
|  [09]   | `HashAlgorithm`       | algorithm value | `sha1`/`sha256`/`sha512` dispatch key            |
|  [10]   | `Digits`              | primitive alias | token length carried as data                     |
|  [11]   | `OTPResult<T, E>`     | tagged result   | `ok`-discriminated value-or-error carrier        |
|  [12]   | `OTPResultOk<T>`      | result arm      | `value` payload                                  |
|  [13]   | `OTPResultError<E>`   | result arm      | `error` payload                                  |

Type operators brand a raw string and narrow an option record once its ports are proven present.

[TYPE_OPERATORS]: `Brand` `Base32Secret` `OTPToken` `RequireKeys` `OptionalKeys` `NarrowBy` `PluginConfig` `WithRequiredPlugins` `GenerationReady`

[PUBLIC_TYPE_SCOPE]: the `OTPError` class tree on `./errors` — every family narrows the root and every leaf narrows its family, so a catch arm discriminates at whatever depth the handler needs

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `OTPError`              | root class    | `Error` subclass every failure extends          |
|  [02]   | `SecretError`           | family        | secret byte length and type faults              |
|  [03]   | `CounterError`          | family        | HOTP counter sign, integrality, overflow        |
|  [04]   | `TimeError`             | family        | TOTP clock sign and finiteness                  |
|  [05]   | `PeriodError`           | family        | period bound faults                             |
|  [06]   | `TokenError`            | family        | token length and format faults                  |
|  [07]   | `CryptoError`           | family        | HMAC and entropy faults from the port           |
|  [08]   | `Base32Error`           | family        | secret encode and decode faults                 |
|  [09]   | `PluginError`           | family        | absent crypto or base32 port                    |
|  [10]   | `ConfigurationError`    | family        | absent secret, label, issuer; wrong secret type |
|  [11]   | `CounterToleranceError` | family        | HOTP window sign and width                      |
|  [12]   | `EpochToleranceError`   | family        | TOTP window sign and width                      |
|  [13]   | `AfterTimeStepError`    | family        | replay-floor sign, integrality, range           |
|  [14]   | `AlgorithmError`        | leaf          | unsupported algorithm value                     |
|  [15]   | `DigitsError`           | leaf          | digit count outside the admitted band           |

Constant caps are the guardrail vocabulary; a policy value overrides a cap through `createGuardrails`, never a re-derived literal.

[CONSTANT_CAPS]: `MIN_SECRET_BYTES` `MAX_SECRET_BYTES` `RECOMMENDED_SECRET_BYTES` `MIN_PERIOD` `MAX_PERIOD` `DEFAULT_PERIOD` `MAX_COUNTER` `MAX_WINDOW`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: port and guardrail construction — the named alternative to a bare object literal

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `createCryptoPlugin(CreateCryptoPluginOptions) -> CryptoPlugin`   | factory | name an hmac/entropy/compare triple    |
|  [02]   | `createBase32Plugin(CreateBase32PluginOptions) -> Base32Plugin`   | factory | name an encode/decode pair             |
|  [03]   | `createCryptoContext(CryptoPlugin) -> CryptoContext`              | factory | bind a crypto port for repeated calls  |
|  [04]   | `createBase32Context(Base32Plugin) -> Base32Context`              | factory | bind a base32 port for repeated calls  |
|  [05]   | `createGuardrails(Partial<OTPGuardrailsConfig>) -> OTPGuardrails` | factory | fold policy caps over the constant set |
|  [06]   | `hasGuardrailOverrides(OTPGuardrails) -> boolean`                 | static  | separate a policy cap from a default   |

[ENTRYPOINT_SCOPE]: the RFC-4226 kernel and secret codecs — reached inside an `OTPHooks` implementation

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `dynamicTruncate(Uint8Array) -> number`                              | static  | RFC-4226 truncation to a 31-bit integer |
|  [02]   | `truncateDigits(number, number) -> string`                           | static  | render a truncated integer as digits    |
|  [03]   | `counterToBytes(number \| bigint) -> Uint8Array`                     | static  | 8-byte big-endian counter block         |
|  [04]   | `getDigestSize(HashAlgorithm) -> number`                             | static  | digest width per algorithm value        |
|  [05]   | `constantTimeEqual(string \| Uint8Array, ...) -> boolean`            | static  | timing-safe token comparison            |
|  [06]   | `validateByteLengthEqual(Uint8Array, Uint8Array) -> boolean`         | static  | length equality ahead of a compare      |
|  [07]   | `generateSecret(SecretOptions) -> string`                            | static  | mint a Base32 enrollment secret         |
|  [08]   | `normalizeSecret(string \| Uint8Array, Base32Plugin?) -> Uint8Array` | static  | admit either secret form to bytes       |
|  [09]   | `stringToBytes(string \| Uint8Array) -> Uint8Array`                  | static  | UTF-8 lift for hand-built plugin input  |
|  [10]   | `bytesToString(Uint8Array) -> string`                                | static  | inverse render of the byte form         |

[ENTRYPOINT_SCOPE]: narrowing guards, tolerance normalization, and the result rail

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------ | :------------------------------------------ |
|  [01]   | `hasPlugins(T) -> T is WithRequiredPlugins<T>`                      | static  | prove both ports before a generate call     |
|  [02]   | `hasCrypto(T) -> T is GenerationReady<T>`                           | static  | prove the crypto port alone                 |
|  [03]   | `hasBase32(T) -> T is T & { base32: Base32Plugin }`                 | static  | prove the base32 port alone                 |
|  [04]   | `normalizeEpochTolerance(number \| [number, number])`               | static  | resolve a TOTP window to a past/future pair |
|  [05]   | `normalizeCounterTolerance(number \| [number, number])`             | static  | resolve a HOTP window to a past/future pair |
|  [06]   | `wrapResult((...Args) => T) -> (...Args) => OTPResult<T, OTPError>` | fold    | convert a throwing kernel call to a result  |
|  [07]   | `wrapResultAsync((...Args) => Promise<T>)`                          | fold    | the async mirror of the same conversion     |

Validators throw a typed family error; asserters narrow an optional field to present.

[VALIDATORS]: `validateSecret` `validateCounter` `validateTime` `validatePeriod` `validateToken` `validateCounterTolerance` `validateEpochTolerance`
[ASSERTERS]: `requireCryptoPlugin` `requireBase32Plugin` `requireSecret` `requireLabel` `requireIssuer` `requireBase32String`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Ports carry every environment fact: `CryptoPlugin` supplies HMAC, entropy, and constant-time compare while `Base32Plugin` supplies the secret codec, so one plugin object re-targets the whole rail.
- `HashAlgorithm` is an argument, never a call family — `getDigestSize` and every HMAC path take the algorithm as a value.
- `OTPHooks` intercepts truncation and encoding, so a non-numeric dialect is three optional functions on the option record.
- `createGuardrails` folds an override map over the constant caps and marks the result, so `hasGuardrailOverrides` separates a policy cap from a default at runtime.

[STACKING]:
- `otplib`(`.api/otplib.md`): `OTPHooks` rides the `hooks?` option field and a `createGuardrails` record rides `guardrails` on `generate`/`verify`; that root owns the strategy discriminant and the `VerifyResult` algebra.
- `effect`(`.api/effect.md`): `OTPError` and its families are the `Effect.try` catch discriminants a `Match.value(...)` fold routes onto the credential fault rail, and secret bytes stay `Redacted` until `normalizeSecret`.
- `@oslojs/crypto`(`.api/oslojs-crypto.md`): `hmac` and `constantTimeEqual` are the values `createCryptoPlugin({ hmac, randomBytes, constantTimeEqual })` names, so OTP HMAC rides the primitive `sign/crypto` owns.
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): `encodeBase32UpperCaseNoPadding` and `decodeBase32` are the values `createBase32Plugin({ encode, decode })` names, keeping base32 on one owner.
- within-lib: `CryptoContext`/`Base32Context` bind a port once per subject so a per-call plugin lookup disappears, and `wrapResult`/`wrapResultAsync` convert a throwing kernel call into `OTPResult` ahead of the Effect boundary.

[LOCAL_ADMISSION]:
- `authn/` subpaths import it alone: type-only for hook and port shapes, value imports for the factories, guardrail constants, and error classes.
- Kernel primitives serve an `OTPHooks` implementation; a strategy call routes through the `otplib` root.

[RAIL_LAW]:
- Package: `@otplib/core`
- Owns: the hook seam, the plugin port types with their factories and context wrappers, the guardrail cap algebra, the RFC-4226 primitive kernel, and the `OTPError` class tree
- Accept: type-only hook and port imports, `createCryptoPlugin`/`createBase32Plugin` for named construction, `createGuardrails` for policy caps, error classes as `Effect.try` catch discriminants
- Reject: a truncation, digit render, or secret normalization re-implemented beside the kernel row; a second guardrail constant set; a direct `@otplib/totp`/`@otplib/hotp` sub-import
