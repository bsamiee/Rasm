# [SECURITY_WORKLOAD]

Machine identity: the certified relying-party plane a service, sidecar, or headless worker authenticates through when no browser ceremony exists and the hand-carried static token is the thing being deleted. One `Configuration` per issuer holds the whole client state — discovered metadata, client id, the client-authentication strategy, the JWKS custody, the non-repudiation posture — and every grant, introspection, revocation, and protected-resource read takes it first, so a grant is a case in one closed request family dispatched on the value and a new grant type is a case plus an arm, never a second client type or a `getTokenByX` member family. Sender constraint is the plane's default posture, not an option: one DPoP key pair mints per principal, its handle rides `options.DPoP` into every grant and every resource call so each proof binds to that call's own method and URL, and its RFC 7638 thumbprint is the `cnf.jkt` a resource server recomputes from the presented key — a stolen access token is inert without the private key that proved it, and a first-party token this folder issues for the same workload carries the identical confirmation value through `AccessClaims.cnf`. JWKS custody is the folder's ONE ledger: the certified client seeds from `JwksLedger` and writes back through it exactly as the jose resolver does, so an issuer's key set is fetched once per estate rather than once per library — and because the two packages count `uat` in different units, the ledger owns an instant and each seam renders its own scalar. Every response is ingress: `TokenEndpointResponse` carries an untyped index band, so a grant lands through one `Schema` owner into `MachinePrincipal` and no field reaches a transport credential before it decodes. Every leg is bounded and class-gated, with one protocol-mandated nonce re-run that is an arm rather than a retry policy, and every credential is `Redacted` from admission into the header projection the runtime wave mounts. `WorkloadFault` instantiates the folder fault shape over the core `Fault.Class.family` seam.

## [01]-[INDEX]

- [02]-[CLIENT_BINDING]: the issuer spec, the client-auth vocabulary, the shared JWKS custody; `WorkloadFault`, `IssuerSpec`, `Bound`.
- [03]-[GRANT_ROSTER]: the request family, the RFC 8693 token-type URNs, the response owner, the resolved identity; `GrantRequest`, `TokenType`, `GrantWire`, `MachinePrincipal`.
- [04]-[PRINCIPAL_LIFECYCLE]: the scoped client, grant dispatch, rotation, introspection, revocation, and the proved resource call; `Workload`, `IssuerStore`.

## [02]-[CLIENT_BINDING]

[CLIENT_BINDING]:
- Owner: `IssuerSpec` — the per-issuer admission record (issuer URL, client id, the client-authentication row, the DPoP posture and proof algorithm, the non-repudiation posture, and the optional dynamic-registration metadata); `Bound` — what a bind RESOLVES: the `Configuration`, the DPoP handle with its thumbprint, and the issuer's advertised grant roster; `WorkloadFault` — the folder fault shape closed at the core family seam.
- Law: one `Configuration` holds the whole client state and every member takes it first, so client identity, authentication, transport, JWKS custody, and response-checking posture are decided once at bind and no per-call argument carries a secret; a client type or method family per grant is the shape this binding deletes.
- Law: client authentication is a bound strategy value, never a request field — `ClientSecretPost`/`ClientSecretBasic` for a shared secret, `PrivateKeyJwt` over a `crypt/secret`-supplied signing handle where the issuer demands asymmetric client auth, `None` for a public client relying on DPoP alone — so moving from a secret to a key is one row and no body ever assembles a `client_secret`.
- Law: discovery is the only endpoint source — `discovery` resolves the well-known document and `serverMetadata()` answers every endpoint, the advertised grant roster, and the DPoP signing algorithms; `dynamicClientRegistration` is the same resolution for a client that registers itself, `new Configuration(metadata, …)` the offline row an air-gapped deployment takes with an unchanged downstream surface, and a hand-built endpoint path is the form all three delete.
- Law: the advertised roster gates the plane, so an unsupported capability refuses before a request rather than as an opaque token-endpoint error — `dpop_signing_alg_values_supported` decides the proof algorithm at bind and an issuer advertising none against a DPoP-demanding spec is `unsupported`, while `grant_types_supported` rides `Bound` and is READ at dispatch against the request case's own advertised identifier, one `_GRANT_TYPES` row per case under the guard that forbids a rowless arm. Both gates take an issuer silent on their axis as no refusal — an unpublished roster is unstated capability, never denied capability — so the check bites exactly where the issuer made a claim.
- Law: JWKS custody is the folder's one ledger — `setJwksCache` seeds the client at bind and `getJwksCache` writes its snapshot back after each verifying leg, so this plane and the jose resolver share one stored key set per issuer and neither refetches what the other already paid for. The `uat` scalar is NEVER carried across: this package counts epoch seconds where jose counts epoch milliseconds, so the ledger owns an instant and each seam projects its own unit — a shared raw scalar reads as 1970 on one side, refetching on every call, or as the far future on the other, never refetching a rotated key. Neither is the type NAME: both packages export one spelled `ExportedJWKSCache` differing only in that unit, so importing it here hides the hazard at exactly the seam that has to see it — this projection takes its shape from `setJwksCache`'s own parameter, which cannot name the wrong package.
- Law: `enableNonRepudiationChecks` is a posture row, not a default — it demands a signed response wherever the issuer can produce one, so a deployment whose auditor requires provable issuer authorship flips one spec field and every JWT-secured response verifies against the discovered JWKS through that same custody.
- Growth: a new issuer is one `IssuerSpec` value; a new client-auth strategy is one `_AUTH` row or one `Authentication` case; a new posture is one spec field the bind reads.
- Boundary: `crypt/secret` supplies the client secret or the `PrivateKeyJwt` material; `crypt/sign` owns `JwksLedger`, `JwksSnapshot`, and `AccessClaims`; the runtime wave binds the instrumented fetch through `customFetch`, forwarding the whole `CustomFetchOptions` record — `duplex` included, so a `ReadableStream` protected-resource body streams instead of buffering; `authn/oauth` keeps the interactive browser ceremony and reaches none of these grants.

```typescript
import {
  ClientSecretBasic, ClientSecretPost, None, PrivateKeyJwt, ResponseBodyError, WWWAuthenticateChallengeError,
  clientCredentialsGrant, discovery, dynamicClientRegistration, enableNonRepudiationChecks, fetchProtectedResource,
  genericGrantRequest, getDPoPHandle, getJwksCache, initiateBackchannelAuthentication, initiateDeviceAuthorization,
  pollBackchannelAuthenticationGrant, pollDeviceAuthorizationGrant, randomDPoPKeyPair, refreshTokenGrant,
  setJwksCache, tokenIntrospection, tokenRevocation,
  type ClientAuth, type Configuration, type DPoPHandle, type IntrospectionResponse,
  type TokenEndpointResponse, type TokenEndpointResponseHelpers, type WWWAuthenticateChallenge,
} from "openid-client"
import { Fault } from "@rasm/core"
import { Array, Config, Context, Data, DateTime, Duration, Effect, Match, Option, Redacted, Schema } from "effect"
import { AccessClaims, JwksLedger, JwksSnapshot, type SingleUse } from "../crypt/sign.ts"
import { Reject } from "../crypt/verify.ts"

const _family = Fault.Class.family(
  ["transport", "grant", "nonce", "proof", "shape", "inactive", "unsupported", "expired"] as const,
  {
    transport: Fault.Class.row({
      class: "unavailable",
      leg: "transport",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `issuer leg did not complete: ${cause}`,
    }),
    grant: Fault.Class.row({
      class: "denied",
      leg: "token",
      detail: Schema.Struct({ code: Schema.String }),
      render: ({ code }) => `token endpoint denied the grant: ${code}`,
    }),
    nonce: Fault.Class.row({
      class: "invalid",
      leg: "dpop",
      detail: Schema.Struct({}),
      render: () => "issuer demands a fresh dpop nonce",
    }),
    proof: Fault.Class.row({
      class: "denied",
      leg: "dpop",
      detail: Schema.Struct({ challenge: Schema.String }),
      render: ({ challenge }) => `resource server rejected the dpop proof: ${challenge}`,
    }),
    shape: Fault.Class.row({
      class: "malformed",
      leg: "admission",
      detail: Schema.Struct({ cause: Schema.String }),
      render: ({ cause }) => `token response did not admit: ${cause}`,
    }),
    inactive: Fault.Class.row({
      class: "denied",
      leg: "introspect",
      detail: Schema.Struct({ client: Schema.String }),
      render: ({ client }) => `introspection reports client ${client} inactive`,
    }),
    unsupported: Fault.Class.row({
      class: "invalid",
      leg: "discovery",
      detail: Schema.Struct({ axis: Schema.Literal("dpop-alg", "grant"), value: Schema.String }),
      render: ({ axis, value }) => `issuer advertises no ${value} ${axis}`,
    }),
    expired: Fault.Class.row({
      class: "expired",
      leg: "lease",
      detail: Schema.Struct({ on: Schema.Literal("approval", "handoff"), coordinate: Schema.String }),
      render: ({ coordinate, on }) => `${on} window closed: ${coordinate}`,
    }),
  },
)

declare namespace WorkloadFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class WorkloadFault extends Schema.TaggedError<WorkloadFault>()("WorkloadFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const _NONCE = "use_dpop_nonce"

const _PRESENTED = {
  expired: false,
  grant: true,
  inactive: false,
  nonce: false,
  proof: true,
  shape: false,
  transport: false,
  unsupported: false,
} as const satisfies Record<WorkloadFault.Reason, boolean>

const _challenged = (cause: WWWAuthenticateChallengeError): Option.Option<string> =>
  Option.flatMap(Array.head(cause.cause), (challenge: WWWAuthenticateChallenge) => Option.fromNullable(challenge.parameters.error))

const _nonced = (cause: unknown): boolean =>
  cause instanceof ResponseBodyError
    ? cause.error === _NONCE
    : cause instanceof WWWAuthenticateChallengeError && Option.contains(_challenged(cause), _NONCE)

const _faultOf: (cause: unknown) => WorkloadFault = Match.type<unknown>().pipe(
  Match.when(_nonced, () => new WorkloadFault({ case: { reason: "nonce" } })),
  Match.when(Match.instanceOf(ResponseBodyError), (error) => new WorkloadFault({ case: { reason: "grant", code: error.error } })),
  Match.when(
    Match.instanceOf(WWWAuthenticateChallengeError),
    (error) => new WorkloadFault({ case: { reason: "proof", challenge: Option.getOrElse(_challenged(error), () => String(error.status)) } }),
  ),
  Match.orElse((error) => new WorkloadFault({ case: { reason: "transport", cause: String(error) } })),
)

const _AUTH = {
  post: (secret: Redacted.Redacted<string>) => ClientSecretPost(Redacted.value(secret)),
  basic: (secret: Redacted.Redacted<string>) => ClientSecretBasic(Redacted.value(secret)),
  none: () => None(),
} as const satisfies Record<string, (secret: Redacted.Redacted<string>) => ClientAuth>

type Authentication = Data.TaggedEnum<{
  Secret: { readonly scheme: keyof typeof _AUTH; readonly secret: Redacted.Redacted<string> }
  Key: { readonly key: Redacted.Redacted<CryptoKey> }
}>

const Authentication = Data.taggedEnum<Authentication>()

type IssuerSpec = {
  readonly issuer: string
  readonly clientId: string
  readonly authentication: Authentication
  readonly dpop: boolean
  readonly proofAlg: string
  readonly nonRepudiation: boolean
  readonly registration: Option.Option<Record<string, unknown>>
}

type Bound = {
  readonly spec: IssuerSpec
  readonly config: Configuration
  readonly proof: Option.Option<{ readonly handle: DPoPHandle; readonly jkt: string }>
  readonly grants: ReadonlyArray<string>
}

const _clientAuth: (authentication: Authentication) => ClientAuth = Authentication.$match({
  Key: ({ key }) => PrivateKeyJwt(Redacted.value(key)),
  Secret: ({ scheme, secret }) => _AUTH[scheme](secret),
})

const _seed = (snapshot: JwksSnapshot): Parameters<typeof setJwksCache>[1] => ({
  jwks: { keys: snapshot.keys },
  uat: Math.floor(DateTime.toEpochMillis(snapshot.observedAt) / 1000),
})
```

## [03]-[GRANT_ROSTER]

[GRANT_ROSTER]:
- Owner: `GrantRequest` — the closed request family every machine grant rides: `Client` (client credentials, the default machine grant), `Exchange` (RFC 8693 token exchange, delegation and impersonation both), `Refresh` (rotation of a held grant), `Device` (RFC 8628, the flow a headless enrollment takes), `Backchannel` (CIBA, out-of-band approval); `TokenType` is the RFC 8693 URN vocabulary; `GrantWire` the response owner; `MachinePrincipal` the resolved identity.
- Law: the grant is the input VALUE, never a member name — one entrypoint takes the request and `$match` dispatches its arm, so the five package legs are five arms of one surface and a sixth grant is one case plus one arm; a `grantClientCredentials`/`grantTokenExchange` family re-derives a discriminant the value already carries.
- Law: token exchange rides the generic request under the grant-type URN held as one folder constant, with `subject_token`, `subject_token_type`, and the optional `actor_token`/`requested_token_type`/`audience` as parameters — a hand-built token-endpoint body would duplicate a grant row and drop the client authentication the `Configuration` binds.
- Law: a two-leg flow keeps its handoff inside one arm — the device and CIBA starts answer a response a human must act on, so the arm carries the caller's `present` continuation and then polls; splitting start and poll into two public members would let a caller poll a response it never presented.
- Law: a poll is bounded by the FLOW's window, never by the per-leg issuer deadline — the certified client defaults each poll's abort to the start response's own `expires_in`, and running the poll as an ordinary leg instead put a ten-second deadline and a retryable budget around a window a human is still walking, so an unapproved device flow settled as a transport timeout and re-drove itself rather than expiring. That advertised `expires_in` bounds both sides: whatever `present` hands the user is enforced, the fiber's own `AbortSignal` carries it into the library so an interrupt reaches the pending poll, and a closed window settles `expired`.
- Law: a grant response is ingress — `TokenEndpointResponse` carries an untyped index band, so every response decodes through `GrantWire` before any field reaches a principal, the foreign spellings are renamed at the field record rather than by a mapping layer, and an issuer answering a `token_type` outside `bearer`/`dpop` refuses as `shape` instead of producing a credential no transport knows how to present.
- Law: absent evidence is absence, never a forged instant — `expires_in` is optional on the wire, so a response that states no lifetime lands the plane's configured floor and the receipt says when THIS plane will stop trusting the token, rather than a zero or a far-future expiry no issuer asserted.
- Law: `MachinePrincipal` carries the scheme its issuer chose, so the transport credential derives rather than being assembled — a DPoP-bound token presents under the `DPoP` scheme and a bearer token under `Bearer`, and a caller hand-prefixing `Bearer ` onto a sender-constrained token strips the binding the grant paid for.
- Law: a `MachinePrincipal`'s scopes are the issuer's granted delegation, and a first-party token this folder issues for the same workload states its `rasm:` delegation in `access/claim`'s `Scope` vocabulary the ceiling reads — so a workload authorized against Rasm's own plane caps at the bundle it was issued to spend exactly as a session or an api-key caller does, never a second delegation model per credential source.
- Law: the confirmation value is the BARE RFC 7638 thumbprint — RFC 9449 confirms a sender-constrained token by exactly what a resource server recomputes from the presented proof key, so `jkt` holds the thumbprint and never its `urn:ietf:params:oauth:jwk-thumbprint` URI form; the URI form is a subject spelling and lands in a `cnf.jkt` field as a value no verifier will match.
- Growth: a new grant is one `GrantRequest` case with its `_GRANT_TYPES` identifier row, its `_delegated` scope arm, and its dispatch arm — the row guard refuses a case reaching the advertised-roster gate unchecked, and the exhaustive `$match` refuses one rotating at an unstated breadth; a new exchange token type is one `_TOKEN_TYPES` row; a new response field is one `GrantWire` field the principal inherits.
- Boundary: what a resolved principal may DO is `access/claim`'s fold; where its credential is mounted per transport is the runtime wave's; this owner resolves the identity and its window.
- Packages: `openid-client` (`TokenEndpointResponse`, `TokenEndpointResponseHelpers`); `effect` (`Data`, `DateTime`, `Duration`, `Option`, `Redacted`, `Schema`).

```typescript
const _TOKEN_TYPES = {
  access: "urn:ietf:params:oauth:token-type:access_token",
  refresh: "urn:ietf:params:oauth:token-type:refresh_token",
  id: "urn:ietf:params:oauth:token-type:id_token",
  jwt: "urn:ietf:params:oauth:token-type:jwt",
  saml2: "urn:ietf:params:oauth:token-type:saml2",
} as const

const _EXCHANGE = "urn:ietf:params:oauth:grant-type:token-exchange"

const _GRANT_TYPES = {
  Backchannel: "urn:openid:params:grant-type:ciba",
  Client: "client_credentials",
  Device: "urn:ietf:params:oauth:grant-type:device_code",
  Exchange: _EXCHANGE,
  Refresh: "refresh_token",
} as const

declare namespace TokenType {
  type Kind = keyof typeof _TOKEN_TYPES
  type Urn = (typeof _TOKEN_TYPES)[Kind]
}

declare namespace GrantRequest {
  type Kind = GrantRequest["_tag"]
  type _Rows<T extends Record<Kind, string> = typeof _GRANT_TYPES> = T
}

type DeviceHandoff = { readonly userCode: string; readonly verificationUri: string; readonly expiresIn: number }

type GrantRequest = Data.TaggedEnum<{
  Client: { readonly scope: Option.Option<string> }
  Exchange: {
    readonly subject: Redacted.Redacted<string>
    readonly subjectType: TokenType.Kind
    readonly actor: Option.Option<Redacted.Redacted<string>>
    readonly requested: Option.Option<TokenType.Kind>
    readonly audience: Option.Option<string>
  }
  Refresh: { readonly presented: Redacted.Redacted<string>; readonly scope: Option.Option<string> }
  Device: { readonly scope: Option.Option<string>; readonly present: (handoff: DeviceHandoff) => Effect.Effect<void> }
  Backchannel: { readonly hint: string; readonly scope: Option.Option<string> }
}>

const GrantRequest = Data.taggedEnum<GrantRequest>()

const _delegated: (request: GrantRequest) => Option.Option<string> = GrantRequest.$match({
  Backchannel: ({ scope }) => scope,
  Client: ({ scope }) => scope,
  Device: ({ scope }) => scope,
  Exchange: () => Option.none(),
  Refresh: ({ scope }) => scope,
})

class GrantWire extends Schema.Class<GrantWire>("GrantWire")({
  token: Schema.propertySignature(Schema.Redacted(Schema.NonEmptyString)).pipe(Schema.fromKey("access_token")),
  scheme: Schema.propertySignature(Schema.Literal("bearer", "dpop")).pipe(Schema.fromKey("token_type")),
  lifetime: Schema.optionalWith(Schema.Positive, { as: "Option", exact: true }).pipe(Schema.fromKey("expires_in")),
  refresh: Schema.optionalWith(Schema.Redacted(Schema.NonEmptyString), { as: "Option", exact: true }).pipe(Schema.fromKey("refresh_token")),
  granted: Schema.optionalWith(Schema.NonEmptyString, { as: "Option", exact: true }).pipe(Schema.fromKey("scope")),
}) {}

const _admitted = Schema.decodeUnknown(GrantWire, { errors: "all" })

class MachinePrincipal extends Schema.Class<MachinePrincipal>("MachinePrincipal")({
  issuer: Schema.NonEmptyString,
  clientId: Schema.NonEmptyString,
  token: Schema.Redacted(Schema.NonEmptyString),
  scheme: Schema.Literal("bearer", "dpop"),
  scope: Schema.Array(Schema.NonEmptyString),
  expiresAt: Schema.DateTimeUtc,
  refresh: Schema.optionalWith(Schema.Redacted(Schema.String), { as: "Option" }),
  jkt: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {
  get credential(): Redacted.Redacted<string> {
    return Redacted.make(`${this.scheme === "dpop" ? "DPoP" : "Bearer"} ${Redacted.value(this.token)}`)
  }
  get lapsed(): Effect.Effect<boolean> {
    return Effect.map(DateTime.now, (now) => DateTime.greaterThanOrEqualTo(now, this.expiresAt))
  }
  claims(sid: string): AccessClaims {
    return new AccessClaims({
      sub: this.clientId,
      sid,
      scope: this.scope,
      tid: Option.none(),
      cnf: Option.map(this.jkt, (jkt) => ({ jkt })),
    })
  }
}

const _resolved = (bound: Bound, wire: GrantWire, floor: Duration.Duration): Effect.Effect<MachinePrincipal> =>
  Effect.map(DateTime.now, (now) =>
    new MachinePrincipal({
      issuer: bound.spec.issuer,
      clientId: bound.spec.clientId,
      token: wire.token,
      scheme: wire.scheme,
      scope: Option.match(wire.granted, { onNone: () => [], onSome: (granted) => granted.split(" ") }),
      expiresAt: DateTime.addDuration(
        now,
        Option.match(wire.lifetime, { onNone: () => floor, onSome: (seconds) => Duration.seconds(seconds) }),
      ),
      refresh: wire.refresh,
      jkt: Option.map(bound.proof, (held) => held.jkt),
    }))
```

## [04]-[PRINCIPAL_LIFECYCLE]

[PRINCIPAL_LIFECYCLE]:
- Owner: `Workload` — the scoped service over one `Bound` per issuer: `grant` dispatches the request family into a `MachinePrincipal`, `rotate` refreshes a held principal or falls back to its original request, `introspect` asks the issuer whether a presented token is still live, `retire` revokes on teardown, `call` is the DPoP-proved protected-resource read the plane exists to make possible, and `handoff`/`claim` are the two ends of the single-use crossing over the `IssuerStore` port a fleet hands a freshly minted principal across a process edge with.
- Law: sender constraint binds once per principal and travels with it — `randomDPoPKeyPair` mints the pair, `getDPoPHandle` wraps it against the bound `Configuration`, the handle rides `options.DPoP` into every grant and resource call, and the handle's own `calculateThumbprint` is the `jkt` the principal carries; the private key never leaves the handle, so the estate holds one proof custody per principal rather than one per call site.
- Law: a resource call is PROVED, never merely authorized — `fetchProtectedResource` carries the token, the method, the body, and the same handle, so the proof binds to this call's own method and URL and a proof replayed against another endpoint fails at the resource server; a hand-assembled `authorization` header omits the proof entirely and silently downgrades a sender-constrained credential to a bearer one.
- Law: DPoP nonce recovery is one arm keyed on the protocol CODE, never on the fault class — RFC 9449 carries `use_dpop_nonce` on two channels the certified client surfaces as two classes, the token endpoint answering with an error body and a resource server with a WWW-Authenticate challenge, so the triage reads the code across both and re-runs the same leg exactly once; keying the arm on the challenge class alone leaves every grant leg with no recovery and re-drives an `insufficient_scope` the issuer already decided, and folding the arm into the transport budget re-drives a genuine refusal. The handle records the server's nonce as the refusal lands, so the second attempt carries it and a third is a real answer.
- Law: a crossing carries a live credential or none — `handoff` stashes under the principal's own remaining window so a slot cannot outlive the token inside it, `claim` consumes once and re-reads `lapsed` because the hop itself spends time, and an empty slot and a spent principal both answer `expired`; the pair takes the store through the requirement channel per call, so a single-process composition binds no port and pays nothing for a crossing it never makes.
- Law: rotation is the principal's own lifetime, never a caller's timer — `rotate` reads the held refresh grant and re-drives the refresh arm, a bound refresh staying sender-constrained through the same handle, and a principal with no refresh grant re-runs the request that minted it; a caller comparing clocks against a stored expiry re-derives what `lapsed` already answers. Both arms rotate at the same BREADTH: `_delegated` projects the originating request's scope onto the refresh, because a scopeless refresh is answered at the issuer's default and narrows a workload silently, surfacing later as an authorization refusal nothing on this plane explains.
- Law: introspection answers liveness, never authorization — `active: false` is the `inactive` refusal and the response's scope and confirmation claims are evidence a caller may cross-check, while the decision stays `access/claim`'s fold; a plane authorizing on an introspection body forks the entitlement owner.
- Law: the plane carries its own denominator and guards what enters it — every resolved principal lands the `credential`-kinded admission and ceremony span through `Reject.measured` under the `workload` surface facet, while only a refusal a PRESENTED credential earned marks the refusal row: `_PRESENTED` states that per reason, so the token endpoint's denial and a resource server's proof rejection count while a capability the issuer never advertised, a leg that never arrived, a shape that refused, and a closed approval window do not. Counting a configuration fault as a credential guess makes a misconfigured spec read as a fleet under attack on the one series that separates credential failure from traffic change.
- Receipt: `MachinePrincipal` on grant, rotate, and claim, the introspection evidence on introspect, the raw `Response` on call (the caller decodes at its own seam), `void` on retire and handoff — never a raw `TokenEndpointResponse`.
- Boundary: the app root supplies the `IssuerSpec` and the `JwksLedger` binding; the runtime wave supplies the instrumented fetch and mounts `credential` as an HTTP header, gRPC call metadata, or a NATS auth-callout header; `access/claim` owns what the principal may do.

```typescript
class IssuerStore extends Context.Tag("security/authn/IssuerStore")<IssuerStore, SingleUse<MachinePrincipal, WorkloadFault>>() {}

const _policy = Config.unwrap({
  deadline: Config.duration("WORKLOAD_CALL_DEADLINE").pipe(
    Config.withDefault(Duration.seconds(10)),
    Config.withDescription("per-leg issuer deadline before the class-gated retry re-drives"),
  ),
  floor: Config.duration("WORKLOAD_LIFETIME_FLOOR").pipe(
    Config.withDefault(Duration.minutes(5)),
    Config.withDescription("trust window granted to a token whose response states no lifetime"),
  ),
})

class Workload extends Effect.Service<Workload>()("security/authn/Workload", {
  scoped: (spec: IssuerSpec) =>
    Effect.gen(function* () {
      const ledger = yield* JwksLedger
      const { deadline, floor } = yield* _policy
      const _legged = <A>(run: () => Promise<A>): Effect.Effect<A, WorkloadFault> =>
        Effect.tryPromise({ try: run, catch: _faultOf }).pipe(
          Effect.timeoutFail({ duration: deadline, onTimeout: () => new WorkloadFault({ case: { reason: "transport", cause: `${spec.issuer} did not answer inside the deadline` } }) }),
          Effect.retry(Fault.Budget.schedule("lease")),
        )
      const _proved = <A>(run: () => Promise<A>): Effect.Effect<A, WorkloadFault> =>
        _legged(run).pipe(Effect.catchIf((fault) => fault.case.reason === "nonce", () => _legged(run)))
      const config = yield* _legged(() =>
        Option.match(spec.registration, {
          onNone: () => discovery(new URL(spec.issuer), spec.clientId, undefined, _clientAuth(spec.authentication)),
          onSome: (metadata) => dynamicClientRegistration(new URL(spec.issuer), metadata, _clientAuth(spec.authentication)),
        }))
      yield* Effect.when(Effect.sync(() => enableNonRepudiationChecks(config)), () => spec.nonRepudiation)
      const _persist = Effect.flatMap(DateTime.now, (observedAt) =>
        Option.match(Option.fromNullable(getJwksCache(config)), {
          onNone: () => Effect.void,
          onSome: (cache) => ledger.save(spec.issuer, new JwksSnapshot({ keys: cache.jwks.keys, observedAt })),
        }))
      yield* Effect.flatMap(ledger.load(spec.issuer), (held) =>
        Effect.sync(() => Option.match(held, { onNone: () => undefined, onSome: (snapshot) => setJwksCache(config, _seed(snapshot)) })))
      const metadata = config.serverMetadata()
      const proof = yield* spec.dpop
        ? Effect.gen(function* () {
            yield* Effect.unless(
              Effect.fail(new WorkloadFault({ case: { reason: "unsupported", axis: "dpop-alg", value: spec.proofAlg } })),
              () =>
                Option.match(Option.fromNullable(metadata.dpop_signing_alg_values_supported), {
                  onNone: () => true,
                  onSome: (advertised) => Array.contains(advertised, spec.proofAlg),
                }),
            )
            const pair = yield* _legged(() => randomDPoPKeyPair(spec.proofAlg))
            const handle = getDPoPHandle(config, pair)
            return Option.some({ handle, jkt: yield* _legged(() => handle.calculateThumbprint()) })
          })
        : Effect.succeedNone
      const bound: Bound = { spec, config, proof, grants: metadata.grant_types_supported ?? [] }
      const _options = Option.match(proof, { onNone: () => undefined, onSome: ({ handle }) => ({ DPoP: handle }) })
      const _scoped = (scope: Option.Option<string>): Record<string, string> =>
        Option.match(scope, { onNone: () => ({}), onSome: (value) => ({ scope: value }) })
      const _polled = (
        window: number,
        run: (signal: AbortSignal) => Promise<TokenEndpointResponse & TokenEndpointResponseHelpers>,
      ): Effect.Effect<TokenEndpointResponse & TokenEndpointResponseHelpers, WorkloadFault> =>
        Effect.tryPromise({ try: run, catch: _faultOf }).pipe(
          Effect.timeoutFail({
            duration: Duration.seconds(window),
            onTimeout: () => new WorkloadFault({ case: { reason: "expired", on: "approval", coordinate: `${window}s` } }),
          }),
        )
      const _landed = (
        legged: Effect.Effect<TokenEndpointResponse & TokenEndpointResponseHelpers, WorkloadFault>,
      ): Effect.Effect<MachinePrincipal, WorkloadFault> =>
        legged.pipe(
          Effect.tap(() => _persist),
          Effect.flatMap((response) =>
            _admitted(response).pipe(Effect.mapError((cause) => new WorkloadFault({ case: { reason: "shape", cause: String(cause) } })))),
          Effect.flatMap((wire) => _resolved(bound, wire, floor)),
        )
      const _advertised = (request: GrantRequest): Effect.Effect<void, WorkloadFault> =>
        Effect.unless(
          Effect.fail(new WorkloadFault({ case: { reason: "unsupported", axis: "grant", value: _GRANT_TYPES[request._tag] } })),
          () => Array.isEmptyReadonlyArray(bound.grants) || Array.contains(bound.grants, _GRANT_TYPES[request._tag]),
        ).pipe(Effect.asVoid)
      const grant = (request: GrantRequest): Effect.Effect<MachinePrincipal, WorkloadFault> =>
        Effect.zipRight(_advertised(request), GrantRequest.$match(request, {
          Client: ({ scope }) => _landed(_proved(() => clientCredentialsGrant(config, _scoped(scope), _options))),
          Exchange: ({ actor, audience, requested, subject, subjectType }) =>
            _landed(_proved(() =>
              genericGrantRequest(config, _EXCHANGE, {
                subject_token: Redacted.value(subject),
                subject_token_type: _TOKEN_TYPES[subjectType],
                ...Option.match(actor, {
                  onNone: () => ({}),
                  onSome: (held) => ({ actor_token: Redacted.value(held), actor_token_type: _TOKEN_TYPES.access }),
                }),
                ...Option.match(requested, { onNone: () => ({}), onSome: (kind) => ({ requested_token_type: _TOKEN_TYPES[kind] }) }),
                ...Option.match(audience, { onNone: () => ({}), onSome: (value) => ({ audience: value }) }),
              }, _options))),
          Refresh: ({ presented, scope }) => _landed(_proved(() => refreshTokenGrant(config, Redacted.value(presented), _scoped(scope), _options))),
          Device: ({ present, scope }) =>
            Effect.gen(function* () {
              const started = yield* _legged(() => initiateDeviceAuthorization(config, _scoped(scope)))
              yield* present({ userCode: started.user_code, verificationUri: started.verification_uri, expiresIn: started.expires_in })
              return yield* _landed(_polled(started.expires_in, (signal) =>
                pollDeviceAuthorizationGrant(config, started, undefined, { ..._options, signal })))
            }),
          Backchannel: ({ hint, scope }) =>
            Effect.gen(function* () {
              const started = yield* _legged(() => initiateBackchannelAuthentication(config, { login_hint: hint, ..._scoped(scope) }))
              return yield* _landed(_polled(started.expires_in, (signal) =>
                pollBackchannelAuthenticationGrant(config, started, undefined, { ..._options, signal })))
            }),
        })).pipe(
          Effect.tapError((fault) =>
            Effect.when(Reject.mark("credential", { surface: "workload" }), () => _PRESENTED[fault.case.reason])),
          Reject.measured("credential", { surface: "workload" }),
          Effect.withSpan("security.workload.grant", { attributes: { issuer: spec.issuer } }),
        )
      return {
        bound,
        grant,
        handoff: (key: string, principal: MachinePrincipal): Effect.Effect<void, WorkloadFault, IssuerStore> =>
          Effect.gen(function* () {
            const now = yield* DateTime.now
            const remaining = DateTime.distance(now, principal.expiresAt)
            return yield* remaining > 0
              ? Effect.flatMap(IssuerStore, (store) => store.stash(key, principal, Duration.millis(remaining)))
              : Effect.fail(new WorkloadFault({ case: { reason: "expired", on: "handoff", coordinate: key } }))
          }),
        claim: (key: string): Effect.Effect<MachinePrincipal, WorkloadFault, IssuerStore> =>
          Effect.flatMap(IssuerStore, (store) =>
            Effect.flatMap(store.consume(key), Option.match({
              onNone: () => Effect.fail(new WorkloadFault({ case: { reason: "expired", on: "handoff", coordinate: key } })),
              onSome: (principal) =>
                Effect.flatMap(principal.lapsed, (spent) =>
                  spent ? Effect.fail(new WorkloadFault({ case: { reason: "expired", on: "handoff", coordinate: key } })) : Effect.succeed(principal)),
            }))),
        rotate: (principal: MachinePrincipal, origin: GrantRequest): Effect.Effect<MachinePrincipal, WorkloadFault> =>
          Option.match(principal.refresh, {
            onNone: () => grant(origin),
            onSome: (presented) => grant(GrantRequest.Refresh({ presented, scope: _delegated(origin) })),
          }),
        introspect: (principal: MachinePrincipal): Effect.Effect<IntrospectionResponse, WorkloadFault> =>
          _legged(() => tokenIntrospection(config, Redacted.value(principal.token))).pipe(
            Effect.filterOrFail(
              (response) => response.active,
              () => new WorkloadFault({ case: { reason: "inactive", client: principal.clientId } }),
            )),
        call: (
          principal: MachinePrincipal,
          target: { readonly url: URL; readonly method: string; readonly body: Option.Option<string> },
        ): Effect.Effect<Response, WorkloadFault> =>
          _proved(() =>
            fetchProtectedResource(
              config,
              Redacted.value(principal.token),
              target.url,
              target.method,
              Option.getOrUndefined(target.body),
              undefined,
              _options,
            )).pipe(Effect.withSpan("security.workload.call", { attributes: { method: target.method } })),
        retire: (principal: MachinePrincipal): Effect.Effect<void, WorkloadFault> =>
          _legged(() => tokenRevocation(config, Redacted.value(principal.token))),
      } as const
    }),
  accessors: true,
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Authentication, GrantRequest, IssuerStore, MachinePrincipal, Workload, WorkloadFault }
export type { Bound, DeviceHandoff, IssuerSpec, TokenType }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
