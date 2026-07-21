# [TS_SECURITY_IDEAS]

Forward pool of higher-order folder concepts grounded in the identity-and-custody domain and the monorepo purpose. `[1]-[OPEN]` carries the active ideas as cards; each card names the capability, what it unlocks, and the gap or technique it draws on. `[2]-[CLOSED]` carries the finished or dropped ideas with a one-line disposition so the same idea is never re-litigated. An idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[WORKLOAD_IDENTITY]-[QUEUED]: workload identity — machine-to-machine grants (client-credentials, token exchange) and DPoP sender-constrained proofs complete the authn plane for service, sidecar, and headless app shapes.
- Capability: a machine principal mints and refreshes its own access token through grant rows sharing the oauth row grammar; DPoP proofs mint and verify through `Jwt` with the `cnf.jkt` binding `Material.thumbprintUri` already supplies; the resolved machine principal projects into the per-call transport credential (gRPC metadata, NATS auth callout header) the runtime wave mounts.
- Shape: new page `libs/typescript/security/.planning/authn/workload.md` — grant rows, proof mint/verify, principal projection; `authn/oauth.md` keeps the browser authorization-code ceremony untouched.
- Unlocks: service-to-service auth over every transport axis at this folder's altitude; a fleet worker authenticates without a browser ceremony or a hand-carried static token.
- Anchors: `AccessClaims.cnf`, `Material.thumbprintUri`, `IssuerRef` remote verify, `arctic` ceremony rows, `openid-client` (admission-lane candidate for the certified grant/DPoP client).
- Tension: `openid-client` admission must survive the supersession review against `arctic` — split custody rules arctic the browser code ceremony and openid-client the machine grants and DPoP.

[SECURITY_BOARD_PACK]-[BLOCKED]: complete the core security board pack while preserving security's receipt-truth alert rail.
- Capability: core's `DashboardModel` security pack projects deny, rotation, reuse, JWKS, and KDF series beside authenticity rejects; tenant filtering rides the pack-owned `Convention.rasm.tenant` variable, while `Audit.alerts` remains the `Alert.of` latency fold and breached facts remain zero-tolerance receipt paging.
- Shape: extend `libs/typescript/core/.planning/observe/board.md` `_PACKS.security`; `access/audit.md` keeps one `DashboardModel.pack("security", identity, {})` call and mints no panel family.
- Unlocks: one complete security operations board reaches the existing iac `Boards` compiler without local query or alert-rule forks.
- Anchors: `access/audit.md` `[05]-[BOARD]`, core `board.md` `_securityRejects`/`_securityFacets` and `_PACKS.security`, iac `operate/observe.md` `Boards`.
- Arms: `libs/typescript/core/.planning/observe/board.md` `_PACKS.security` contains the five missing signal panels and a `Convention.rasm.tenant` pack variable; current disk contains only authenticity rejects and reject facets with no variables.
- Ripple: `core` `board#PACKS`.

[ADMITTED_SURFACE_COMPLETION]-[QUEUED]: admitted-surface completion — every verified-unexploited folder and branch catalog member lands as a row on its owning page, closing the census gap between catalogs and pages.
- Capability: `getTimeStepUsed` replaces the hand-rolled TOTP window math; `encodeBase64urlNoPadding` gives opaque tokens a base64url wire row; `preferredAuthenticatorType` and `verifyBrowserAutofillInput` complete the webauthn policy and autofill surfaces; `secrets.list` backs the partial-refresh planner with the full-object census.
- Shape: one row per member on its owner — `libs/typescript/security/.planning/authn/credential.md`, `libs/typescript/security/.planning/crypt/sign.md`, `libs/typescript/security/.planning/authn/webauthn.md`, `libs/typescript/security/.planning/crypt/secret.md`.
- Unlocks: admitted capability the estate already pays for stops idling; hand-rolled parallels to library members are deleted.
- Anchors: `.api/otplib.md`, `.api/oslojs-encoding.md`, `.api/simplewebauthn-server.md`, `.api/simplewebauthn-browser.md`, `.api/dopplerhq-node-sdk.md` — each member re-verified at its catalog anchor.

[COOKIE_SESSION_GUARD]-[QUEUED]: cookie-session guard — the guard family completes with the browser cookie scheme, so a cookie-authenticated browser app rides the same declarative middleware seam as bearer and api-key callers.
- Capability: a `SessionGuard` middleware decodes the access cookie through the platform security-decode seam, folds `Jwt.verify` and the CSRF double-submit check into one requirement provision, and lands refused presentations on the `bearer` reject row; cookie framing, CSRF mint, and guard verdict stay one lifecycle.
- Shape: guard row on `libs/typescript/security/.planning/authn/session.md` beside `BearerGuard` — same `HttpApiMiddleware.Tag` grammar, `provides: CurrentClaims`.
- Unlocks: a zero-JS-token browser app composes full session auth declaratively; the runtime serve wave mounts one more guard row, never a hand parser.
- Anchors: `HttpApiBuilder.securityDecode` (`libs/typescript/.api/effect-platform.md`), `HttpApiSecurity.apiKey` cookie variant, `Cookie.verify`, `CookieSpec` rows, `Reject.mark("bearer")`.

[AUTH_THROTTLE_ROWS]-[QUEUED]: One auth-throttle policy-row surface — five identical wirings collapse to rows.
- Capability: the store-backed auth-throttle idiom — limiter accessor, rate config rows, token-bucket policy, throttled-fault fold — is one folder-owned surface whose per-ceremony variation is a policy row, so a new throttled ceremony is a row, never a sixth hand-wiring.
- Shape: the policy-row owner lands in `libs/typescript/security/.planning/authn/credential.md` beside its `_throttled` fold (already the intra-family collapse); session refresh, otp verify, api-key resolve, webauthn assert-finish, and crypt verify key rows onto it.
- Unlocks: throttle posture, budgets, and fault classification stay one spelling across every credential-verify surface the corpus names a brute-force target.
- Anchors: the five wiring sites (`authn/session.md` refresh, `authn/credential.md` otp and api-key budgets, `authn/webauthn.md` assert-finish, `crypt/verify.md`); `RateLimiter.makeWithRateLimiter`; the limiter-posture ruling at `libs/typescript/.planning/RULINGS.md` `[02]-[COLLAPSE]` — the collapse stays inside the one auth posture that ruling protects.

[SECRET_STORAGE_DISCRIMINANT]-[QUEUED]: One entropy-keyed storage discriminant — every credential-at-rest form derives from the material's entropy class.
- Capability: secret-at-rest storage is one discriminant — low-entropy material digests through the KDF, high-entropy random material stores a fast constant-time fingerprint — so the machine key's per-candidate KDF scan collapses to the O(1) indexed compare the session rule already states, or the KDF defense-in-depth exception for machine keys is stated beside that rule; either way the discriminant is spelled once.
- Shape: an entropy-keyed storage row per credential surface on `libs/typescript/security/.planning/authn/credential.md`'s `Digest` idiom, reconciled against `authn/session.md`'s stated rule; recovery codes stay the stated gray zone.
- Unlocks: one storage law across every credential surface; `ApiKey.resolve` drops N argon2 verifies per resolve; the rule and its lone exception live at one seam.
- Anchors: `session.md`'s rule — argon2 for low-entropy credentials, a random high-entropy secret takes a fast constant-time compare, never a per-check KDF; `credential.md` `Digest.mint`/`Digest.resolve` argon2 candidate scan; `Crypto.fingerprint`'s O(1) indexed path; the `rk_<prefix>.<secret>` high-entropy mint through `Crypto.token`.
- Tension: argon2 on a high-entropy machine key defends only against fingerprint-table theft with reduced-entropy secrets — the bet is whether that defense prices N KDF verifies per resolve.

[API_KEY_CLAIM_BRIDGE]-[QUEUED]: Machine principals resolve into the one entitlement plane — the entitlement resolve discriminates every principal source.
- Capability: one polymorphic entitlement resolve — the api-key principal reaches the same `ClaimSet`/`Principal`/`TenantScope` fold user claims ride, discriminating on the presented shape or projecting the record to claims at the guard seam, so machine callers hit RBAC/ReBAC policy identically to token callers.
- Shape: one resolve arm on `libs/typescript/security/.planning/access/claim.md` (`Claim.resolve` input union over `AccessClaims | ApiKeyRecord`, roles via the `ClaimStore`) or a guard-seam projection beside `ApiKeyGuard` in `authn/credential.md`.
- Unlocks: `claim.md`'s own growth line — an API-key principal resolving into the same `ClaimSet` — lands; machine-keyed endpoints stop bypassing the entitlement fold.
- Anchors: `claim.md` `Claim.resolve`/`Claim.principal`/`Claim.bind`; `credential.md` `ApiKeyGuard` providing `CurrentApiKey: ApiKeyRecord`; the one-polymorphic-resolve law `credential.md` already states for `ApiKey.resolve`.

[LEASE_SPEC_CONTRACT]-[QUEUED]: LeaseSpec deploy-realization contract — the encoded lease shape publishes as the boundary value the deploy plane decodes into custody cells.
- Capability: `LeaseSpec` — scope, keys, TTL, renewal posture — gains its one encoded owner on the crypt plane, so lease semantics have a single spelling and the iac workload estate decodes each lease into a config-scoped Doppler `ServiceToken` and namespace-`Secret` custody cell as pure data; renewal posture and epoch-keyed rotation read from the spec, never from deploy-side convention.
- Shape: encoded `LeaseSpec` schema and boundary row on `libs/typescript/security/.planning/crypt/secret.md` beside the `_ACCESS` custody law; the `ARCHITECTURE.md` `[BOUNDARY]: LeaseSpec` seam keeps its spelling with the schema now real.
- Unlocks: a leased credential's blast radius is the lease by construction; per-app lease custody composes without collision; the deploy plane consumes lease semantics with zero security knowledge.
- Anchors: `crypt/secret.md` custody and rotation rows; `ARCHITECTURE.md` Crypt-to-Iac `[BOUNDARY]: LeaseSpec` edge; iac `kube/workload.md` lease realization (carded).
- Tension: iac realizes custody cells — this folder owns only the encoded shape and its semantics; a deploy concern leaking into the spec is a boundary breach.
- Ripple: `iac` `[LEASE_REALIZATION]`.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[WEBAUTHN_TRUST_ISOLATION]-[COMPLETE]: resolved on the refute arm — the catalog rules `SettingsService`/`MetadataService` module-singletons with no instance-scoped construction, and `authn/webauthn.md` states the single-policy-per-process law with the deployment-split remedy.
[REJECT_FACT_STREAM]-[COMPLETE]: one authenticity-reject fact stream — realized as the `Reject` owner on the crypt verify page; dialect/surface/reason are bounded facets, never divergent metric names.
[CONVENTION_INSTRUMENT_PLANE]-[COMPLETE]: Convention-owned instrument plane — every folder instrument (kdf timer, jwks quarantine/resolve/miss, shred reject, secret rotation, policy deny) mints from a core `Convention.instrument` row, and the tenant/reason tag keys read `Convention.rasm.*`.
[SECURITY_FACT_RAIL]-[COMPLETE]: security fact rail — `.planning/access/audit.md` owns `SecurityFact`, `Witness`, `AuditJournal`, class-routed lanes, and app-scoped `Audit.live`; every enumerated loud arm publishes, and keyed `Crypto.sign` behind `Pseudonym` supersedes the rejected unkeyed subject fingerprint.
[KDF_COST_CALIBRATION]-[COMPLETE]: KDF cost calibration — `Calibration` on `.planning/crypt/sign.md` `[07]-[CALIBRATION]` emits complete core `Claim.Band` receipts, passes every claim through `Claim.admit`, grades p99 against each `CryptoCost.targetMs`, and feeds those row-owned targets into `_argonMs` boundaries.
