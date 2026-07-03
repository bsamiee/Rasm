# [SERVICES_AUTH]

The server-side credential-verification owner — `Authn`, the one `Effect.Service` that verifies every second-factor and primary credential the browser collects but never trusts itself: the TOTP/HOTP one-time-password through `otplib`, the WebAuthn registration attestation and authentication assertion through `@simplewebauthn/server`, the OAuth authorization-code exchange, and the API-key presentation. The browser owns only the ceremony (the navigator-credential call and the collected code) and forwards the result over the `interchange` `CommandGateway`; the attestation and secret verification run HERE and never in the browser — a browser-side `@simplewebauthn/server`/`otplib` verifier is the named defect the `platform/Session/session` boundary already names. Every verifier is one arm on one `AuthCommand` request `Data.TaggedEnum` under one `$match`, never a sibling method per credential. The verifier rides the one `persistence/store#STORE_BOUNDARY` `PgClient` over the `MfaSecret`/`WebauthnCredential`/`OauthAccount`/`ApiKey`/`Session` entities the `EntityRegistry` already carries, decodes no `interchange`-owned wire union, and crosses no .NET wire. This is the `authn/` sub-domain's one page.

## [01]-[INDEX]

- [01]-[VERIFIER]: owns the `AuthCommand` request axis, the `CredentialKind` vocabulary, the `Authn` verifier service, the replay-defeating counter/time-step advance, and the `AuthFault` policy-projected fault.

## [02]-[VERIFIER]

- Owner: `Authn`, the one `Effect.Service` verifying every credential the browser forwards; `AuthCommand`, the closed request `Data.TaggedEnum` over the verb axis (`EnrolTotp`/`VerifyTotp`/`BeginWebauthnRegistration`/`VerifyWebauthnRegistration`/`BeginWebauthnAuthentication`/`VerifyWebauthnAuthentication`/`ExchangeOauthCode`/`VerifyApiKey`) folded by one `$match`; `CredentialKind`, the `as const satisfies Record` vocabulary whose rows carry the per-credential replay and lifetime policy; `AuthFault`, the one `Data.TaggedError` whose `reason` projects through a policy table.
- Cases: the verifier is one entrypoint over the closed `AuthCommand` family, dispatched by one `AuthCommand.$match`, so a new credential verb breaks every dispatch site at compile time rather than growing a sibling verifier service. The TOTP arm verifies the browser-collected code against the per-user `MfaSecret` Base32 secret through `otplib` `verify` (the async functional surface, `strategy: "totp"`), narrowing the discriminated `VerifyResult` on `valid === true` and persisting `timeStep` as the `afterTimeStep` replay floor so a reused step is rejected — `otplib` binds `NobleCryptoPlugin`/`ScureBase32Plugin` by default, and enrolment mints the secret through `generateSecret` and the provisioning URI through `generateURI` (`strategy: "totp"`, the `issuer`/`label`/`secret`) for the QR the browser renders. The WebAuthn arms drive the four `@simplewebauthn/server` primitives: `generateRegistrationOptions`/`generateAuthenticationOptions` build the JSON challenge options the browser feeds `navigator.credentials.create()`/`.get()`, and `verifyRegistrationResponse`/`verifyAuthenticationResponse` verify the signed attestation/assertion against the stored `WebauthnCredential` row, the registration verify guarding the `VerifiedRegistrationResponse` discriminated union on `verified === true` before reading `registrationInfo`, and the authentication verify advancing the stored `counter` to `authenticationInfo.newCounter` so a cloned authenticator replay is rejected. The OAuth arm exchanges the authorization code for the provider token and upserts the `OauthAccount` row; the API-key arm constant-time-compares the presented key against the `ApiKey` row's stored hash. Every arm resolves a verified credential into one `Session` row through the one `SqlClient`, so a successful verification IS a session mint, never a side channel. The `expectedChallenge` and `expectedOrigin`/`expectedRPID` the WebAuthn verifiers demand arrive from the per-session enrolment challenge persisted at the `Begin*` arm, never a client-supplied value.
- Auto: `CredentialKind` is the one `as const satisfies Record` vocabulary keyed by credential — `totp`/`webauthn`/`oauth`/`apiKey` — each row carrying the replay-advance policy (`timeStep`/`counter`/`none`), the session lifetime, and the `AuthFault` reason it rejects with, read by `keyof typeof` indexed access so a `Match` chain re-deriving the per-credential policy is the deleted form; `R.keys(CredentialKind)` spreads into the `Schema.Literal(...)` the `MfaSecret`/`WebauthnCredential` rows tag their `kind` column with, so the wire vocabulary derives from the same anchor.
- Entry: the verifier rides the one `persistence/store#STORE_BOUNDARY` `PgClient` over the `MfaSecret`/`WebauthnCredential`/`OauthAccount`/`ApiKey`/`Session` entities the `EntityRegistry` already declares (no sibling schema — a read projection is `Model.fields`/`Schema.pick` off the one class), and the verification runs inside the `persistence/tenancy#TENANCY` `withTenant` GUC scope so every credential lookup reads the RLS-scoped row set; the `Begin*` challenge persists through the same client keyed to the in-flight `Session`; a verified credential mints a `Session` row and the `platform/Session/session#AUTH_SESSION` browser owner reads the resulting bearer over the `interchange` `CommandGateway`, never a browser-side verifier; the API-key and OAuth secrets resolve at the deploy boundary through the `provisioning/contract#PROVISIONING` `SecretResolver`, never a hand-set literal.
- Wire: the verifier decodes no `interchange`-owned wire union — the `interchange` `CommandGateway` is the generic outbound transport the browser forwards the collected ceremony result over, and `interchange` owns no auth-verify command shape; the `AuthCommand` request shapes are this folder's own, the `RegistrationResponseJSON`/`AuthenticationResponseJSON` payloads are the W3C WebAuthn browser shapes `@simplewebauthn/server` defines (not a .NET wire), and this surface carries no .NET wire type and crosses no .NET wire.
- Packages: `otplib` for the TOTP/HOTP verification (`generate`/`verify`/`generateSecret`/`generateURI`, the discriminated `VerifyResult`, the default `NobleCryptoPlugin`/`ScureBase32Plugin`); `@simplewebauthn/server` for the WebAuthn flow (`generateRegistrationOptions`/`verifyRegistrationResponse`/`generateAuthenticationOptions`/`verifyAuthenticationResponse`, the `WebAuthnCredential` stored-credential shape, `MetadataService`/`SettingsService` for the attestation-root policy); `@effect/sql`/`@effect/sql-pg` for the credential and session rows through `persistence/store#STORE_BOUNDARY`; `effect` for the `AuthCommand` `Data.TaggedEnum` dispatch, the `CredentialKind` vocabulary, the `AuthFault` policy projection, and the `Redacted` secret wrapping; `@effect/platform-node` for the driver host.
- Growth: a new credential verb lands as one `AuthCommand` `Data.TaggedEnum` variant breaking the `$match` at compile time, never a sibling verifier service; a new credential kind lands as one `CredentialKind` vocabulary row carrying its replay/lifetime policy; a new attestation format lands as one `SettingsService.setRootCertificates` row; a new auth-failure mode lands as one `AuthFault` reason row on the policy table, never a parallel error class; a new TOTP/HOTP variant lands as one `otplib` `strategy` selection on the same arm.
- Boundary: the named defects — a browser-side `@simplewebauthn/server`/`otplib` verifier instead of the server concern; a sibling verifier method or service per credential instead of the one `AuthCommand` `$match`; a parallel auth-failure error class instead of the one `AuthFault` reason row; a hand-rolled CBOR/attestation parse or manual signature check instead of the `@simplewebauthn/server` verifiers; a re-minted credential schema beside the `EntityRegistry` `Model.Class`; a verification that skips the `counter`/`timeStep` replay advance and so re-admits a replayed assertion; a `Match` chain re-deriving the per-credential policy the `CredentialKind` vocabulary owns; an `expectedChallenge` read from the client instead of the persisted per-session enrolment challenge. This is a node-only surface, never browser-reachable.

```ts contract
import type { SqlError } from "@effect/sql"
import type {
  AuthenticationResponseJSON,
  PublicKeyCredentialCreationOptionsJSON,
  PublicKeyCredentialRequestOptionsJSON,
  RegistrationResponseJSON,
  VerifiedAuthenticationResponse,
  VerifiedRegistrationResponse,
  WebAuthnCredential,
} from "@simplewebauthn/server"
import {
  generateAuthenticationOptions,
  generateRegistrationOptions,
  verifyAuthenticationResponse,
  verifyRegistrationResponse,
} from "@simplewebauthn/server"
import { generateSecret, generateURI, verify, type VerifyResult } from "otplib"
import { Model, SqlClient } from "@effect/sql"
import { Data, Duration, Effect, Match, Option, Record as R, Redacted, Schema as S } from "effect"

const _Credential = {
  totp:     { replay: "timeStep", lifetime: Duration.hours(12), reason: "invalid_totp"     },
  webauthn: { replay: "counter",  lifetime: Duration.hours(24), reason: "invalid_assertion" },
  oauth:    { replay: "none",     lifetime: Duration.hours(8),  reason: "oauth_exchange"    },
  apiKey:   { replay: "none",     lifetime: Duration.days(30),  reason: "invalid_api_key"   },
} as const satisfies Record<string, { replay: "timeStep" | "counter" | "none"; lifetime: Duration.Duration; reason: keyof typeof _AuthPolicy }>

type CredentialKind = keyof typeof _Credential

const _AuthPolicy = {
  invalid_totp:      { status: 401, retryable: true,  ord: 0 },
  invalid_assertion: { status: 401, retryable: true,  ord: 1 },
  invalid_api_key:   { status: 401, retryable: false, ord: 2 },
  oauth_exchange:    { status: 502, retryable: true,  ord: 3 },
  challenge_expired: { status: 410, retryable: false, ord: 4 },
  replayed:          { status: 409, retryable: false, ord: 5 },
} as const satisfies Record<string, { status: number; retryable: boolean; ord: number }>

class MfaSecret extends Model.Class<MfaSecret>("MfaSecret")({
  id: Model.Generated(S.UUID),
  userId: S.UUID,
  kind: S.Literal(...(R.keys(_Credential) as [CredentialKind, ...Array<CredentialKind>])),
  secret: Model.Sensitive(S.String),
  afterTimeStep: S.optionalWith(S.Number, { as: "Option" }),
  createdAt: Model.DateTimeInsert,
}) {}

class WebauthnCredential extends Model.Class<WebauthnCredential>("WebauthnCredential")({
  id: S.String,
  userId: S.UUID,
  publicKey: S.Uint8ArrayFromSelf,
  counter: S.Number,
  transports: S.optionalWith(S.Array(S.String), { as: "Option" }),
  deviceType: S.Literal("singleDevice", "multiDevice"),
  backedUp: S.Boolean,
  createdAt: Model.DateTimeInsert,
}) {}

class AuthFault extends S.TaggedError<AuthFault>()("AuthFault", {
  reason: S.Literal(...(R.keys(_AuthPolicy) as [keyof typeof _AuthPolicy, ...Array<keyof typeof _AuthPolicy>])),
  detail: S.String,
}) {
  get status()    { return _AuthPolicy[this.reason].status }
  get retryable() { return _AuthPolicy[this.reason].retryable }
}

class Verified extends S.Class<Verified>("Verified")({
  userId: S.UUID,
  kind: S.Literal(...(R.keys(_Credential) as [CredentialKind, ...Array<CredentialKind>])),
  sessionId: S.UUID,
}) {}

type AuthCommand = Data.TaggedEnum<{
  readonly EnrolTotp:                    { readonly userId: string; readonly issuer: string; readonly label: string }
  readonly VerifyTotp:                   { readonly userId: string; readonly token: string }
  readonly BeginWebauthnRegistration:    { readonly userId: string; readonly userName: string; readonly rpName: string; readonly rpID: string }
  readonly VerifyWebauthnRegistration:   { readonly userId: string; readonly response: RegistrationResponseJSON; readonly expectedChallenge: string; readonly expectedOrigin: string; readonly expectedRPID: string }
  readonly BeginWebauthnAuthentication:  { readonly userId: string; readonly rpID: string }
  readonly VerifyWebauthnAuthentication: { readonly userId: string; readonly response: AuthenticationResponseJSON; readonly expectedChallenge: string; readonly expectedOrigin: string; readonly expectedRPID: string }
  readonly ExchangeOauthCode:            { readonly userId: string; readonly code: string; readonly provider: string }
  readonly VerifyApiKey:                 { readonly userId: string; readonly presented: Redacted.Redacted }
}>
const AuthCommand = Data.taggedEnum<AuthCommand>()

type EnrolReceipt =
  | { readonly _tag: "TotpSecret"; readonly secret: string; readonly uri: string }
  | { readonly _tag: "RegistrationOptions"; readonly options: PublicKeyCredentialCreationOptionsJSON }
  | { readonly _tag: "AuthenticationOptions"; readonly options: PublicKeyCredentialRequestOptionsJSON }

interface Authn {
  readonly run: (command: AuthCommand) => Effect.Effect<Verified, AuthFault | SqlError.SqlError>
  readonly begin: (command: AuthCommand) => Effect.Effect<EnrolReceipt, AuthFault | SqlError.SqlError>
}

const verifyTotp = (sql: SqlClient.SqlClient, userId: string, token: string): Effect.Effect<number, AuthFault | SqlError.SqlError> =>
  sql<MfaSecret>`SELECT user_id AS "userId", kind, secret, after_time_step AS "afterTimeStep" FROM mfa_secret WHERE user_id = ${userId} AND kind = 'totp'`.pipe(
    Effect.flatMap((rows) =>
      Option.match(Option.fromNullable(rows[0]), {
        onNone: () => Effect.fail(new AuthFault({ reason: "invalid_totp", detail: "no secret" })),
        onSome: (row) =>
          Effect.tryPromise({
            try: () => verify({ strategy: "totp", secret: row.secret, token, afterTimeStep: Option.getOrUndefined(row.afterTimeStep) }),
            catch: (cause) => new AuthFault({ reason: "invalid_totp", detail: String(cause) }),
          }).pipe(
            Effect.flatMap((result: VerifyResult) =>
              result.valid && "timeStep" in result
                ? Option.match(Option.fromNullable(Option.getOrUndefined(row.afterTimeStep)), {
                    onNone: () => Effect.succeed(result.timeStep),
                    onSome: (floor) => result.timeStep <= floor
                      ? Effect.fail(new AuthFault({ reason: "replayed", detail: "reused step" }))
                      : Effect.succeed(result.timeStep),
                  })
                : Effect.fail(new AuthFault({ reason: "invalid_totp", detail: "rejected" })),
            ),
          ),
      }),
    ),
  )

const verifyWebauthnAuthentication = (
  sql: SqlClient.SqlClient,
  command: Extract<AuthCommand, { readonly _tag: "VerifyWebauthnAuthentication" }>,
): Effect.Effect<number, AuthFault | SqlError.SqlError> =>
  sql<WebauthnCredential>`SELECT id, user_id AS "userId", public_key AS "publicKey", counter, transports, device_type AS "deviceType", backed_up AS "backedUp" FROM webauthn_credential WHERE id = ${command.response.id}`.pipe(
    Effect.flatMap((rows) =>
      Option.match(Option.fromNullable(rows[0]), {
        onNone: () => Effect.fail(new AuthFault({ reason: "invalid_assertion", detail: "no credential" })),
        onSome: (row) =>
          Effect.tryPromise({
            try: () =>
              verifyAuthenticationResponse({
                response: command.response,
                expectedChallenge: command.expectedChallenge,
                expectedOrigin: command.expectedOrigin,
                expectedRPID: command.expectedRPID,
                credential: { id: row.id, publicKey: row.publicKey, counter: row.counter, transports: Option.getOrUndefined(row.transports) } satisfies WebAuthnCredential,
              }),
            catch: (cause) => new AuthFault({ reason: "invalid_assertion", detail: String(cause) }),
          }).pipe(
            Effect.flatMap((result: VerifiedAuthenticationResponse) =>
              result.verified
                ? Effect.succeed(result.authenticationInfo.newCounter)
                : Effect.fail(new AuthFault({ reason: "invalid_assertion", detail: "unverified" })),
            ),
          ),
      }),
    ),
  )

const beginRegistration = (sql: SqlClient.SqlClient, command: Extract<AuthCommand, { readonly _tag: "BeginWebauthnRegistration" }>): Effect.Effect<EnrolReceipt, AuthFault | SqlError.SqlError> =>
  Effect.tryPromise({
    try: () => generateRegistrationOptions({ rpName: command.rpName, rpID: command.rpID, userName: command.userName }),
    catch: (cause) => new AuthFault({ reason: "invalid_assertion", detail: String(cause) }),
  }).pipe(
    Effect.tap((options) =>
      sql`INSERT INTO webauthn_challenge (user_id, challenge) VALUES (${command.userId}, ${options.challenge})
          ON CONFLICT (user_id) DO UPDATE SET challenge = ${options.challenge}`,
    ),
    Effect.map((options): EnrolReceipt => ({ _tag: "RegistrationOptions", options })),
  )

const verifyRegistration = (command: Extract<AuthCommand, { readonly _tag: "VerifyWebauthnRegistration" }>): Effect.Effect<VerifiedRegistrationResponse, AuthFault> =>
  Effect.tryPromise({
    try: () =>
      verifyRegistrationResponse({
        response: command.response,
        expectedChallenge: command.expectedChallenge,
        expectedOrigin: command.expectedOrigin,
        expectedRPID: command.expectedRPID,
      }),
    catch: (cause) => new AuthFault({ reason: "invalid_assertion", detail: String(cause) }),
  }).pipe(
    Effect.filterOrFail(
      (result) => result.verified,
      () => new AuthFault({ reason: "invalid_assertion", detail: "unverified" }),
    ),
  )
```
