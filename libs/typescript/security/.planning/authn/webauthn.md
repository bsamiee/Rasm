# [SECURITY_WEBAUTHN] — the passkey ceremony: RP-side verification and the browser-safe invocation

`authn/webauthn` owns both halves of the passkey ceremony as two per-runtime subpath modules: the RP-side verifier over `@simplewebauthn/server` (node `./server`) mints ceremony options and verifies the signed response into a typed verdict, and the browser-safe invocation over `@simplewebauthn/browser` (`./browser`) wraps `navigator.credentials` into an `Effect` gated on a capability probe — the exports map keeps the node verifier physically unreachable from browser resolution. The whole surface is one options→verify pattern across registration and authentication; the attestation-format dispatch is internal, parameterized by policy, never a hand switch. The challenge is stateful across the two phases (minted at start, stashed in the `ChallengeStore` port, consumed single-use at finish), and the verified `Passkey` is public-key crypto — a typed boundary value, not a `Redacted` secret — persisted through the `WebAuthnStore` port with its signature counter, the non-increasing-counter check the clone/replay defense. A successful assertion establishes a session through `session/token`; the verdict is a discriminated rail, never a boolean-plus-throw.

## [1]-[CLUSTER_INDEX]

| [INDEX] | [CONCERN]                          | [OWNER]                             | [PACKAGES]                     | [RUNTIME]        |
| :-----: | :--------------------------------- | :---------------------------------- | :----------------------------- | :--------------- |
|  [01]   | credential + fault vocabulary      | `Passkey` / `WebAuthnFault`         | `@simplewebauthn/server`       | `./server` node  |
|  [02]   | RP-side options + verify           | `WebAuthn` service                  | `@simplewebauthn/server`, `session/token` | `./server` node |
|  [03]   | browser ceremony invocation        | `Passkeys` browser wrapper          | `@simplewebauthn/browser`      | `./browser`      |

## [2]-[RP_VERIFICATION]

[WEBAUTHN]:
- Owner: `WebAuthn.enrollStart`/`enrollFinish` register a passkey, `WebAuthn.assertStart`/`assertFinish` authenticate one; `Passkey` is the stored credential, `WebAuthnFault` the folder fault shape. `WebAuthnStore` holds credentials, `ChallengeStore` holds the single-use challenge.
- Packages: `@simplewebauthn/server` — `generateRegistrationOptions`/`verifyRegistrationResponse` and the authentication pair; the `verified` discriminant is matched so the credential is extracted only on the true arm, and `newCounter` is the replay defense. `session/token` establishes the session.
- Law: the challenge is minted server-side, stashed, and consumed single-use; the response is `Schema`-decoded before verify; a non-increasing counter is a cloned authenticator (`WebAuthnFault.counter`); attestation format dispatches inside the verifier, parameterized by `expectedRPID`/`expectedOrigin`, never a caller switch.
- Receipt: `Passkey` on registration, `TokenPair` on assertion — never a raw `VerifiedRegistrationResponse` past the seam.

```typescript
import { generateAuthenticationOptions, generateRegistrationOptions, verifyAuthenticationResponse, verifyRegistrationResponse, type AuthenticationResponseJSON, type PublicKeyCredentialCreationOptionsJSON, type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON, type VerifiedRegistrationResponse, type WebAuthnCredential } from "@simplewebauthn/server"
import { Config, Context, Effect, Option, Schema } from "effect"
import { CredentialRef, type SessionFault, type Subject, Token, type TokenPair } from "../session/token.ts"

// --- [CONSTANTS] ------------------------------------------------------------------------

const _reasons = ["ceremony", "challenge", "verification", "counter"] as const

const _transports = ["ble", "cable", "hybrid", "internal", "nfc", "smart-card", "usb"] as const

const WebAuthnFaultPolicy = {
  ceremony: { rank: 3, retry: false, status: 500 },
  challenge: { rank: 3, retry: false, status: 400 },
  verification: { rank: 4, retry: false, status: 401 },
  counter: { rank: 5, retry: false, status: 401 },
} as const

declare namespace WebAuthnFault {
  type Reason = keyof typeof WebAuthnFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly status: number }
  type _Rows<T extends Record<Reason, Row> = typeof WebAuthnFaultPolicy> = T
}

// --- [MODELS] ---------------------------------------------------------------------------

class Passkey extends Schema.Class<Passkey>("Passkey")({
  id: Schema.NonEmptyString,
  subject: Schema.UUID,
  publicKey: Schema.Uint8ArrayFromBase64,
  counter: Schema.Number,
  transports: Schema.optionalWith(Schema.Array(Schema.Literal(..._transports)), { as: "Option" }),
}) {}

// --- [ERRORS] ---------------------------------------------------------------------------

class WebAuthnFault extends Schema.TaggedError<WebAuthnFault>()("WebAuthnFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get policy(): WebAuthnFault.Row {
    return WebAuthnFaultPolicy[this.reason]
  }
  override get message(): string {
    return `<webauthn:${this.reason}> ${this.detail}`
  }
}

// --- [SERVICES] -------------------------------------------------------------------------

class WebAuthnStore extends Context.Tag("security/authn/WebAuthnStore")<WebAuthnStore, {
  readonly insert: (passkey: Passkey) => Effect.Effect<void, WebAuthnFault>
  readonly byId: (id: string) => Effect.Effect<Option.Option<Passkey>, WebAuthnFault>
  readonly bySubject: (subject: Subject["id"]) => Effect.Effect<ReadonlyArray<Passkey>, WebAuthnFault>
  readonly updateCounter: (id: string, counter: number) => Effect.Effect<void, WebAuthnFault>
}>() {}

class ChallengeStore extends Context.Tag("security/authn/ChallengeStore")<ChallengeStore, {
  readonly stash: (subject: string, challenge: string) => Effect.Effect<void, WebAuthnFault>
  readonly consume: (subject: string) => Effect.Effect<Option.Option<string>, WebAuthnFault>
}>() {}

class WebAuthn extends Effect.Service<WebAuthn>()("security/authn/WebAuthn", {
  effect: Effect.gen(function* () {
    const store = yield* WebAuthnStore
    const challenges = yield* ChallengeStore
    const token = yield* Token
    const rpID = yield* Config.string("WEBAUTHN_RP_ID")
    const rpName = yield* Config.string("WEBAUTHN_RP_NAME")
    const origin = yield* Config.string("WEBAUTHN_ORIGIN")
    const _challenge = (subject: string): Effect.Effect<string, WebAuthnFault> =>
      Effect.flatMap(challenges.consume(subject), Option.match({ onNone: () => Effect.fail(new WebAuthnFault({ reason: "challenge", detail: subject })), onSome: Effect.succeed }))
    const enrollStart = (subject: Subject["id"], userName: string): Effect.Effect<PublicKeyCredentialCreationOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const existing = yield* store.bySubject(subject)
        const options = yield* Effect.tryPromise({
          try: () => generateRegistrationOptions({ rpName, rpID, userName, userID: new TextEncoder().encode(subject), attestationType: "none", excludeCredentials: existing.map((passkey) => ({ id: passkey.id })) }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* challenges.stash(subject, options.challenge)
        return options
      })
    const enrollFinish = (subject: Subject["id"], response: RegistrationResponseJSON): Effect.Effect<Passkey, WebAuthnFault> =>
      Effect.gen(function* () {
        const expectedChallenge = yield* _challenge(subject)
        const verified = yield* Effect.tryPromise({
          try: () => verifyRegistrationResponse({ response, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
          catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
        }).pipe(Effect.filterOrFail(
          (outcome): outcome is Extract<VerifiedRegistrationResponse, { verified: true }> => outcome.verified,
          () => new WebAuthnFault({ reason: "verification", detail: "unverified registration" }),
        ))
        const passkey = new Passkey({ id: verified.registrationInfo.credential.id, subject, publicKey: verified.registrationInfo.credential.publicKey, counter: verified.registrationInfo.credential.counter, transports: Option.fromNullable(verified.registrationInfo.credential.transports) })
        yield* store.insert(passkey)
        return passkey
      })
    const assertStart = (subject: Subject["id"]): Effect.Effect<PublicKeyCredentialRequestOptionsJSON, WebAuthnFault> =>
      Effect.gen(function* () {
        const passkeys = yield* store.bySubject(subject)
        const options = yield* Effect.tryPromise({
          try: () => generateAuthenticationOptions({ rpID, allowCredentials: passkeys.map((passkey) => ({ id: passkey.id })), userVerification: "required" }),
          catch: (cause) => new WebAuthnFault({ reason: "ceremony", detail: String(cause) }),
        })
        yield* challenges.stash(subject, options.challenge)
        return options
      })
    const assertFinish = (subject: Subject["id"], response: AuthenticationResponseJSON): Effect.Effect<TokenPair, WebAuthnFault | SessionFault> =>
      Effect.gen(function* () {
        const passkey = yield* Effect.flatMap(store.byId(response.id), Option.match({ onNone: () => Effect.fail(new WebAuthnFault({ reason: "verification", detail: response.id })), onSome: Effect.succeed }))
        const expectedChallenge = yield* _challenge(subject)
        const credential: WebAuthnCredential = {
          id: passkey.id,
          publicKey: passkey.publicKey,
          counter: passkey.counter,
          ...(Option.isSome(passkey.transports) && { transports: [...passkey.transports.value] }),
        }
        const verified = yield* Effect.tryPromise({
          try: () => verifyAuthenticationResponse({ response, credential, expectedChallenge, expectedOrigin: origin, expectedRPID: rpID, requireUserVerification: true }),
          catch: (cause) => new WebAuthnFault({ reason: "verification", detail: String(cause) }),
        })
        yield* verified.verified ? Effect.void : Effect.fail(new WebAuthnFault({ reason: "verification", detail: "unverified assertion" }))
        const next = verified.authenticationInfo.newCounter
        yield* next > passkey.counter || (next === 0 && passkey.counter === 0)
          ? Effect.void
          : Effect.fail(new WebAuthnFault({ reason: "counter", detail: passkey.id }))
        yield* store.updateCounter(passkey.id, next)
        return yield* token.establish(new CredentialRef({ kind: "webauthn", key: passkey.id }), ["openid"], { tenant: Option.none(), verified: true })
      })
    return { enrollStart, enrollFinish, assertStart, assertFinish } as const
  }),
  dependencies: [Token.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ChallengeStore, Passkey, WebAuthn, WebAuthnFault, WebAuthnStore }
```

## [3]-[BROWSER_CEREMONY]

[PASSKEYS]:
- Owner: `Passkeys.register`/`Passkeys.authenticate` — the `./browser` runtime module wrapping `navigator.credentials` into an `Effect`, gated on a capability probe so an unsupported browser short-circuits to a typed fault before the call. `PasskeyFault` folds the pre-classified `WebAuthnError` `code`.
- Packages: `@simplewebauthn/browser` — `startRegistration`/`startAuthentication` take the v13 `{ optionsJSON }` object form (never the pre-12 positional form) and reject with an already-coded `WebAuthnError`; `browserSupportsWebAuthn` is the ceremony gate.
- Boundary: this module is `runtime:browser` and imports no node code — the RP verification is the `./server` module; a `Schema` per JSON shape decodes both the options in and the response out at the fetch seam the `browser`/`ui` folder owns.
- Receipt: the `RegistrationResponseJSON`/`AuthenticationResponseJSON` the caller POSTs back to `WebAuthn.*Finish`; the browser never verifies, only collects the signed response.

```typescript
import { browserSupportsWebAuthn, startAuthentication, startRegistration, WebAuthnError, type AuthenticationResponseJSON, type PublicKeyCredentialCreationOptionsJSON, type PublicKeyCredentialRequestOptionsJSON, type RegistrationResponseJSON } from "@simplewebauthn/browser"
import { Data, Effect } from "effect"

// --- [ERRORS] ---------------------------------------------------------------------------

class PasskeyFault extends Data.TaggedError("PasskeyFault")<{ readonly code: string; readonly detail: string }> {}

// --- [OPERATIONS] -----------------------------------------------------------------------

const _lift = <A>(run: () => Promise<A>): Effect.Effect<A, PasskeyFault> =>
  browserSupportsWebAuthn()
    ? Effect.tryPromise({ try: run, catch: (cause) => cause instanceof WebAuthnError ? new PasskeyFault({ code: cause.code, detail: cause.message }) : new PasskeyFault({ code: "ERROR_PASSTHROUGH_SEE_CAUSE_PROPERTY", detail: String(cause) }) })
    : Effect.fail(new PasskeyFault({ code: "ERROR_UNSUPPORTED", detail: "webauthn unsupported" }))

const Passkeys = {
  register: (optionsJSON: PublicKeyCredentialCreationOptionsJSON): Effect.Effect<RegistrationResponseJSON, PasskeyFault> =>
    _lift(() => startRegistration({ optionsJSON })),
  authenticate: (optionsJSON: PublicKeyCredentialRequestOptionsJSON): Effect.Effect<AuthenticationResponseJSON, PasskeyFault> =>
    _lift(() => startAuthentication({ optionsJSON })),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Passkeys, PasskeyFault }
```
