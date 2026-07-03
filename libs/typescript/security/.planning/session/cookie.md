# [SECURITY_COOKIE] — the cookie-attribute policy vocabulary and the CSRF double-submit law

`session/cookie` owns the cookie-attribute policy as one `as const` `CookieSpec` table and the CSRF double-submit law. It frames a `session/token` `TokenPair` into `FramedCookie` values the edge writes with `HttpServerResponse.setCookie` — the access token as a `__Host-` session cookie, the refresh as a path-scoped `__Secure-` cookie confined to the refresh route so it never rides an ordinary request, and the CSRF token as a readable `__Host-` cookie the client echoes into a header. Every attribute is a policy row, not a call-site literal: `httpOnly`, `secure`, `sameSite`, and `path` live in the table, and a new cookie role is one row. The CSRF fold mints a high-entropy token through `sign/crypto`, and verification is a constant-time `same` compare of the cookie value against the presented header — a mismatch or an absent pair is `CsrfFault` at 403. This page holds no HTTP transport: it produces framed values and consumes `sign/crypto` only, so the edge owns the response write and `security` stays runtime-portable.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                       | [OWNER]                                | [PACKAGES]              | [REJECTED_FORM]                            |
| :-----: | :------------------------------ | :------------------------------------- | :---------------------- | :----------------------------------------- |
|  [01]   | cookie-attribute policy rows    | `CookieSpec` `as const` table          | derived — `keyof typeof`| a `Set-Cookie` string built per call site  |
|  [02]   | frame a token pair / clear      | `Cookie.frame` / `Cookie.clear`        | `session/token`         | an `httpOnly` flag chosen at the handler   |
|  [03]   | CSRF double-submit              | `Cookie.csrf` / `Cookie.verify`        | `sign/crypto`           | a hand `===` on the token, a hidden re-check |

## [2]-[COOKIE_VOCABULARY]

[SPEC_AND_FAULT]:
- Owner: `CookieSpec` is the attribute table — one row per cookie role carrying `{ name, options }`; `FramedCookie` is the framed value the edge writes; `CsrfFault` is the folder fault shape at 403. Cookie attributes are the web standard, so `CookieOptions` is a local shape, not a package member.
- Packages: `session/token` supplies the `TokenPair`/`Session`; `sign/crypto` mints and compares the CSRF token; the edge applies each `FramedCookie` through `HttpServerResponse.setCookie`.
- Boundary: `session/token` owns the token values; the edge owns the response write and the request read (extracting the CSRF cookie and header); this page owns only the attribute policy and the CSRF compare.
- Growth: a new cookie role is one `CookieSpec` row; a `sameSite`/`path` change is a row edit that reframes every write with zero handler change.

```typescript
import { DateTime, Duration, Effect, Redacted, Schema } from "effect"
import { Crypto } from "../sign/crypto.ts"
import type { TokenPair } from "./token.ts"

// --- [TYPES] ----------------------------------------------------------------------------

type CookieOptions = {
  readonly httpOnly: boolean
  readonly secure: boolean
  readonly sameSite: "strict" | "lax" | "none"
  readonly path: string
  readonly maxAge?: number
}

type FramedCookie = { readonly name: string; readonly value: Redacted.Redacted<string>; readonly options: CookieOptions }

// --- [CONSTANTS] ------------------------------------------------------------------------

const CookieSpec = {
  access: { name: "__Host-access", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/" } },
  refresh: { name: "__Secure-refresh", options: { httpOnly: true, secure: true, sameSite: "strict", path: "/auth/refresh" } },
  csrf: { name: "__Host-csrf", options: { httpOnly: false, secure: true, sameSite: "strict", path: "/" } },
} as const satisfies Record<string, { readonly name: string; readonly options: CookieOptions }>

const _CSRF_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

const _reasons = ["absent", "mismatch"] as const

const CsrfFaultPolicy = {
  absent: { rank: 3, retry: false, status: 403 },
  mismatch: { rank: 4, retry: false, status: 403 },
} as const

declare namespace CookieSpec {
  type Role = keyof typeof CookieSpec
}

declare namespace CsrfFault {
  type Reason = keyof typeof CsrfFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof CsrfFaultPolicy> = T
}

// --- [ERRORS] ---------------------------------------------------------------------------

class CsrfFault extends Schema.TaggedError<CsrfFault>()("CsrfFault", {
  reason: Schema.Literal(..._reasons),
}) {
  get policy(): CsrfFault.Row {
    return CsrfFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<csrf:${this.reason}>`
  }
}
```

## [3]-[FRAME_AND_CSRF]

[COOKIE]:
- Owner: `Cookie.frame` maps a `TokenPair` onto the access and refresh specs with `maxAge` derived from the session window; `Cookie.clear` expires every role (`maxAge: 0`) on logout; `Cookie.csrf` mints the readable CSRF cookie, `Cookie.verify` compares it to the header.
- Packages: `sign/crypto` `Crypto.token` mints the CSRF value and `Crypto.same` compares it constant-time; `session/token` `Session.expiresAt` sets the refresh cookie lifetime.
- Law: the refresh cookie's `maxAge` tracks the session's remaining window, the access cookie is a session cookie (its JWT carries the real expiry), and the CSRF token is per-session; verification is constant-time double-submit, so a timing oracle and a stripped header both fail at 403.
- Receipt: `FramedCookie[]` — every value `Redacted`, so the framed set never leaks a token into a log; `Cookie.verify` returns `void` or `CsrfFault`, never a boolean the caller can forget to check.

```typescript
// --- [SERVICES] -------------------------------------------------------------------------

class Cookie extends Effect.Service<Cookie>()("security/session/Cookie", {
  effect: Effect.gen(function* () {
    const cipher = yield* Crypto
    const _framed = (role: CookieSpec.Role, value: Redacted.Redacted<string>, maxAge?: number): FramedCookie =>
      ({ name: CookieSpec[role].name, value, options: maxAge === undefined ? CookieSpec[role].options : { ...CookieSpec[role].options, maxAge } })
    const frame = (pair: TokenPair): Effect.Effect<ReadonlyArray<FramedCookie>> =>
      Effect.map(DateTime.now, (now) => {
        const remaining = Math.max(0, Math.floor(Duration.toSeconds(DateTime.distanceDuration(now, pair.session.expiresAt))))
        return [_framed("access", pair.access), _framed("refresh", pair.refresh, remaining)]
      })
    const clear = (): ReadonlyArray<FramedCookie> =>
      [_framed("access", Redacted.make(""), 0), _framed("refresh", Redacted.make(""), 0), _framed("csrf", Redacted.make(""), 0)]
    const csrf = (): Effect.Effect<FramedCookie, CsrfFault> =>
      cipher.token(_CSRF_ALPHABET, 32).pipe(
        Effect.mapError(() => new CsrfFault({ reason: "absent" })),
        Effect.map((token) => _framed("csrf", token)),
      )
    const verify = (cookieToken: string | undefined, headerToken: string | undefined): Effect.Effect<void, CsrfFault> =>
      cookieToken === undefined || headerToken === undefined
        ? Effect.fail(new CsrfFault({ reason: "absent" }))
        : cipher.same(Redacted.make(cookieToken), headerToken)
          ? Effect.void
          : Effect.fail(new CsrfFault({ reason: "mismatch" }))
    return { frame, clear, csrf, verify } as const
  }),
  dependencies: [Crypto.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cookie, CookieSpec, CsrfFault }
export type { CookieOptions, FramedCookie }
```
