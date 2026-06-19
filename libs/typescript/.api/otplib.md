# [API_CATALOGUE] otplib

`otplib` is a TypeScript-first RFC 4226 (HOTP) and RFC 6238 (TOTP) one-time-password library with a pluggable crypto/base32 backend. The default entry (`otplib`) ships the functional API (`generate`, `generateSync`, `verify`, `verifySync`, `generateSecret`, `generateURI`), the unified `OTP` wrapper class that dispatches on a `strategy` of `'totp' | 'hotp'`, the strategy-specific `TOTP`/`HOTP` classes, the `NobleCryptoPlugin` and `ScureBase32Plugin` default plugins, the `OTPGuardrails` validation surface, and the `OTPResult` rail. Secrets are Base32-encoded for Google Authenticator compatibility; `verify` returns a discriminated `VerifyResult` carrying `delta`, `epoch`, and `timeStep` for clock-drift and replay handling. Subpath entries `otplib/functional` and `otplib/class` re-export the functional and class surfaces respectively.

---

## [01]-[PACKAGE_SURFACE]

The default `otplib` entry re-exports a curated set from the functional module, the class module, the type module, `@otplib/core`, `@otplib/totp`, `@otplib/hotp`, and the two default plugins.

```ts
// otplib — dist/index.d.ts (default entry)
export { OTPAuthOptions, OTPFunctionalOptions, OTPStrategy, OTPVerifyFunctionalOptions } from './types.js'
export { generate, generateSecret, generateSync, generateURI, verify, verifySync } from './functional.js'
export { OTP, OTPClassOptions, OTPGenerateOptions, OTPURIGenerateOptions, OTPVerifyOptions } from './class.js'
export {
  Base32Plugin, CryptoPlugin, HashAlgorithm, OTPGuardrails, OTPGuardrailsConfig,
  OTPResult, createGuardrails, stringToBytes, wrapResult, wrapResultAsync
} from '@otplib/core'
export { TOTP, TOTPOptions, VerifyResult } from '@otplib/totp'
export { HOTP } from '@otplib/hotp'
export { NobleCryptoPlugin } from '@otplib/plugin-crypto-noble'
export { ScureBase32Plugin } from '@otplib/plugin-base32-scure'
```

The subpath entries narrow the surface: `otplib/functional` exports `OTPStrategy`, `VerifyResult`, and the six functional functions; `otplib/class` exports `OTP` plus the class option types.

---

## [02]-[FUNCTIONAL_ENTRYPOINTS]

The functional API takes a single options object discriminated by `strategy`. `generate`/`verify` are async; `generateSync`/`verifySync` require a sync-capable crypto plugin and throw `HMACError` otherwise.

```ts
// otplib — functional surface
type OTPStrategy = "totp" | "hotp"
type VerifyResult = TOTPVerifyResult | HOTPVerifyResult

declare function generateSecret(options?: { length?: number; crypto?: CryptoPlugin; base32?: Base32Plugin }): string
declare function generateURI(options: {
  strategy?: OTPStrategy; issuer: string; label: string; secret: string;
  algorithm?: HashAlgorithm; digits?: Digits; period?: number; counter?: number
}): string
declare function generate(options: OTPGenerateOptions): Promise<string>
declare function generateSync(options: OTPGenerateOptions): string
declare function verify(options: OTPVerifyOptions): Promise<VerifyResult>
declare function verifySync(options: OTPVerifyOptions): VerifyResult
```

---

## [03]-[FUNCTIONAL_OPTIONS]

`OTPGenerateOptions` carries every field for both strategies; `OTPVerifyOptions` extends it with `token` and the tolerance/replay fields. TOTP fields (`epoch`, `t0`, `period`, `epochTolerance`, `afterTimeStep`) and HOTP fields (`counter`, `counterTolerance`) coexist on the shape and are selected by `strategy`.

```ts
// otplib — types.d.ts
type OTPAuthOptions = {
  readonly crypto?: CryptoPlugin
  readonly base32?: Base32Plugin
}
type OTPGenerateOptions = {
  secret: string | Uint8Array
  strategy?: OTPStrategy
  crypto?: CryptoPlugin
  base32?: Base32Plugin
  guardrails?: OTPGuardrails
  algorithm?: HashAlgorithm
  digits?: Digits
  period?: number
  epoch?: number
  t0?: number
  counter?: number
  hooks?: OTPHooks
}
type OTPVerifyOptions = OTPGenerateOptions & {
  token: string
  epochTolerance?: number | [number, number]
  counterTolerance?: number | [number, number]
  afterTimeStep?: number
}
```

The `OTPFunctionalOptions` and `OTPVerifyFunctionalOptions` aliases re-exported from the index resolve to `OTPGenerateOptions` and `OTPVerifyOptions`.

---

## [04]-[OTP_CLASS]

`OTP` is the unified wrapper that internally routes to TOTP or HOTP based on its constructed `strategy`. `getStrategy` reports the active strategy; `generateURI` builds an `otpauth://` URI for QR provisioning.

```ts
// otplib — class.d.ts
type OTPClassOptions = {
  strategy?: OTPStrategy
  crypto?: CryptoPlugin
  base32?: Base32Plugin
  guardrails?: OTPGuardrails
}
type OTPGenerateOptions = {
  secret: string | Uint8Array
  algorithm?: HashAlgorithm; digits?: Digits
  epoch?: number; t0?: number; period?: number; counter?: number
  guardrails?: OTPGuardrails; hooks?: OTPHooks
}
type OTPVerifyOptions = OTPGenerateOptions & {
  token: string
  epochTolerance?: number | [number, number]
  counterTolerance?: number | [number, number]
  afterTimeStep?: number
}
type OTPURIGenerateOptions = {
  issuer: string; label: string; secret: string
  algorithm?: HashAlgorithm; digits?: Digits; period?: number; counter?: number
}

declare class OTP {
  constructor(options?: OTPClassOptions)
  getStrategy(): OTPStrategy
  generateSecret(length?: number): string
  generate(options: OTPGenerateOptions): Promise<string>
  generateSync(options: OTPGenerateOptions): string
  verify(options: OTPVerifyOptions): Promise<VerifyResult>
  verifySync(options: OTPVerifyOptions): VerifyResult
  generateURI(options: OTPURIGenerateOptions): string
}
```

---

## [05]-[STRATEGY_CLASSES]

`TOTP` (`@otplib/totp`) and `HOTP` (`@otplib/hotp`) are the strategy-specific wrappers. Both store options at construction and accept per-call overrides. `TOTP.toURI` and `HOTP.toURI` emit provisioning URIs; the TOTP module also exports time-window helpers.

```ts
// @otplib/totp
type TOTPOptions = {
  readonly secret?: string | Uint8Array
  readonly epoch?: number; readonly t0?: number; readonly period?: number
  readonly algorithm?: HashAlgorithm; readonly digits?: Digits
  readonly crypto?: CryptoPlugin; readonly base32?: Base32Plugin
  readonly issuer?: string; readonly label?: string
  readonly guardrails?: OTPGuardrails; readonly hooks?: OTPHooks
}
type TOTPGenerateOptions = TOTPOptions & { readonly secret: string | Uint8Array; readonly crypto: CryptoPlugin }
type TOTPVerifyOptions = TOTPGenerateOptions & {
  readonly token: string
  readonly epochTolerance?: number | [number, number]
  readonly afterTimeStep?: number
}

declare class TOTP {
  constructor(options?: TOTPOptions)
  generateSecret(): string
  generate(options?: Partial<TOTPOptions>): Promise<string>
  verify(token: string, options?: Partial<Omit<TOTPVerifyOptions, "token">>): Promise<VerifyResult>
  toURI(options?: { label?: string; issuer?: string; secret?: string }): string
}

declare function generate(options: TOTPGenerateOptions): Promise<string>
declare function generateSync(options: TOTPGenerateOptions): string
declare function verify(options: TOTPVerifyOptions): Promise<VerifyResult>
declare function verifySync(options: TOTPVerifyOptions): VerifyResult
declare function getRemainingTime(time?: number, period?: number, t0?: number, guardrails?: OTPGuardrails): number
declare function getTimeStepUsed(time?: number, period?: number, t0?: number, guardrails?: OTPGuardrails): number

// @otplib/hotp
type HOTPOptions = {
  readonly secret?: string | Uint8Array
  readonly counter?: number | bigint
  readonly algorithm?: HashAlgorithm; readonly digits?: Digits
  readonly crypto?: CryptoPlugin; readonly base32?: Base32Plugin
  readonly issuer?: string; readonly label?: string
  readonly guardrails?: OTPGuardrails; readonly hooks?: OTPHooks
}
type HOTPGenerateOptions = HOTPOptions & { readonly secret: string | Uint8Array; readonly counter: number | bigint; readonly crypto: CryptoPlugin }
type HOTPVerifyOptions = HOTPGenerateOptions & { readonly token: string; readonly counterTolerance?: number | [number, number] }

declare class HOTP {
  constructor(options?: HOTPOptions)
  generateSecret(): string
  generate(counter: number, options?: Partial<HOTPOptions>): Promise<string>
  verify(params: { token: string; counter: number }, options?: Partial<HOTPOptions & { counterTolerance?: number | [number, number] }>): Promise<VerifyResult>
  toURI(counter?: number): string
}
```

---

## [06]-[VERIFY_RESULT]

Verification returns a discriminated union on `valid`. TOTP's valid result carries `delta`, `epoch`, and `timeStep`; HOTP's valid result carries only `delta`. The unified `VerifyResult` is the union of both.

```ts
// @otplib/totp — VerifyResult
type VerifyResultValid = {
  readonly valid: true
  readonly delta: number    // periods offset from current step (signed)
  readonly epoch: number    // unix seconds of matched period start
  readonly timeStep: number // RFC 6238 time step counter
}
type VerifyResultInvalid = { readonly valid: false }
type VerifyResult = VerifyResultValid | VerifyResultInvalid

// @otplib/hotp — VerifyResult
type HOTPVerifyResultValid = { readonly valid: true; readonly delta: number } // counter steps ahead
type HOTPVerifyResultInvalid = { readonly valid: false }
```

---

## [07]-[PLUGINS_AND_RAILS]

`@otplib/core` defines the `CryptoPlugin` and `Base32Plugin` contracts plus the `OTPResult` rail and value brands. `NobleCryptoPlugin` (`@noble/hashes`-backed, sync HMAC) and `ScureBase32Plugin` (`@scure/base`-backed) are the defaults bound when no plugin is supplied. `HashAlgorithm` is `"sha1" | "sha256" | "sha512"`; `Digits` is `number`.

```ts
// @otplib/core — plugin and rail contracts
type CryptoPlugin = {
  readonly name: string
  hmac(algorithm: HashAlgorithm, key: Uint8Array, data: Uint8Array): Promise<Uint8Array> | Uint8Array
  randomBytes(length: number): Uint8Array
  constantTimeEqual(a: string | Uint8Array, b: string | Uint8Array): boolean
}
type Base32Plugin = {
  readonly name: string
  encode(data: Uint8Array, options?: Base32EncodeOptions): string
  decode(str: string): Uint8Array
}
type OTPResultOk<T> = { readonly ok: true; readonly value: T }
type OTPResultError<E> = { readonly ok: false; readonly error: E }
type OTPResult<T, E = Error> = OTPResultOk<T> | OTPResultError<E>

type OTPHooks = {
  readonly encodeToken?: (truncatedValue: number, digits: number) => string
  readonly validateToken?: (token: string, digits: number) => void
  readonly truncateDigest?: (hmacResult: Uint8Array) => number
}

declare function createGuardrails(config: OTPGuardrailsConfig): OTPGuardrails
declare function stringToBytes(str: string): Uint8Array
declare function wrapResult<T>(fn: () => T): OTPResult<T>
declare function wrapResultAsync<T>(fn: () => Promise<T>): Promise<OTPResult<T>>

// Default plugins re-exported from `otplib`
declare class NobleCryptoPlugin implements CryptoPlugin { readonly name: "noble" }
declare class ScureBase32Plugin implements Base32Plugin { readonly name: "scure" }
```

---

## [08]-[IMPLEMENTATION_LAW]

[STRATEGY_DISPATCH]:
- One canonical options shape carries both TOTP and HOTP fields; `strategy` (`'totp'` default) selects the algorithm at the functional and `OTP`-class boundary. The strategy-specific `TOTP`/`HOTP` classes pre-bind the strategy and reject the foreign fields.
- `secret` is Base32-encoded when a string; pass `Uint8Array` for raw key material. String secrets require a `base32` plugin (defaults to `ScureBase32Plugin`).

[VERIFY_RAIL]:
- `verify` returns a discriminated `VerifyResult`, never throws on a wrong token. Narrow on `result.valid === true` before reading `delta`.
- TOTP `delta` is signed period offset; HOTP `delta` is counter steps ahead. After a successful HOTP verify, persist `counter + delta + 1` to block replay; after TOTP verify, persist `timeStep` and pass it as `afterTimeStep` to reject reused steps.
- `epochTolerance`/`counterTolerance` accept a symmetric number or a `[past, future]` tuple. RFC-compliant past-only TOTP uses `[5, 0]`.

[SYNC_PATH]:
- `generateSync`/`verifySync` require a crypto plugin whose `hmac` is synchronous (`NobleCryptoPlugin`). A WebCrypto-style async-only plugin throws `HMACError` on the sync path.

[PLUGIN_BOUNDARY]:
- `CryptoPlugin` and `Base32Plugin` are the runtime-abstraction seams; `createCryptoPlugin`/`createBase32Plugin` (from `@otplib/core`) build custom backends, but `otplib` binds `NobleCryptoPlugin` and `ScureBase32Plugin` by default.
- `OTPHooks` replaces the default RFC 4226 numeric encoding for non-standard variants (e.g., Steam Guard) without forking core.
