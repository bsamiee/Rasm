# [TS_SECURITY_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards whose leader holds a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — and three to four scoped bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. `[2]-[CLOSED]` carries `[COMPLETE]` and `[DROPPED]` items. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[TOTP_STEP_PROJECTION]-[BLOCKED]: `getRemainingTime`/`getTimeStepUsed` are `@otplib/totp` module exports the `otplib` root barrel does not re-export, and the folder's own rail law refuses a strategy sub-import; the swap unblocks only if the root barrel widens (idea `IDEAS.md` `[ADMITTED_SURFACE_COMPLETION]`).
- Capability: `Otp.remaining` derives from the library's step projection; the `_PERIOD` modulo arithmetic is deleted.
- Shape: one member swap on `libs/typescript/security/.planning/authn/credential.md` once a root-reachable spelling exists.
- Unlocks: IDEAS.md [ADMITTED_SURFACE_COMPLETION] — the hand-rolled TOTP window parallel is deleted, admitted otplib capability the estate already pays for stops idling.
- Arms: the period coupling the swap would have carried is landed independently — `_PERIOD` now rides `generateURI`, every TOTP `verify`, and `remaining` explicitly, so the three read one value instead of agreeing by shared library default.
- Anchors: `otplib` root barrel re-export set; `.api/otplib-core.md` `[RAIL_LAW]` sub-import refusal.
- Atomic: one member swap.

[BASE64URL_TOKEN_ROW]-[QUEUED]: add the base64url opaque-token wire row to `libs/typescript/security/.planning/crypt/sign.md` via `encodeBase64urlNoPadding` (idea `IDEAS.md` `[ADMITTED_SURFACE_COMPLETION]`).
- Capability: `Crypto.token` gains the URL-safe wire form for tokens riding paths and fragments; hex stays the fingerprint form.
- Shape: one codec row on `libs/typescript/security/.planning/crypt/sign.md` minting the base64url token form through `encodeBase64urlNoPadding`.
- Unlocks: IDEAS.md [ADMITTED_SURFACE_COMPLETION] — tokens ride paths and fragments in URL-safe form, closing the census gap between the encoding catalog and the sign page.
- Anchors: `.api/oslojs-encoding.md` `encodeBase64urlNoPadding` member anchor.
- Atomic: one codec row.

[WEBAUTHN_POLICY_AUTOFILL]-[QUEUED]: complete the webauthn policy and autofill surfaces in `libs/typescript/security/.planning/authn/webauthn.md` — `preferredAuthenticatorType` registration hint row, `verifyBrowserAutofillInput` gate on the autofill entry (idea `IDEAS.md` `[ADMITTED_SURFACE_COMPLETION]`).
- Capability: authenticator-type preference becomes a trust-row policy value at options mint; conditional-UI input verification gates `Passkeys.autofill` beside the existing capability probe.
- Shape: two option rows on `libs/typescript/security/.planning/authn/webauthn.md` — the `preferredAuthenticatorType` registration hint at options mint and the `verifyBrowserAutofillInput` gate on `Passkeys.autofill`.
- Unlocks: IDEAS.md [ADMITTED_SURFACE_COMPLETION] — authenticator-type preference and conditional-UI input verification complete the webauthn policy and autofill surfaces, admitted simplewebauthn members stop idling.
- Anchors: `.api/simplewebauthn-server.md` and `.api/simplewebauthn-browser.md` member anchors.
- Atomic: two option rows.

[SESSION_GUARD_ROW]-[QUEUED]: land the `SessionGuard` cookie-scheme row on `libs/typescript/security/.planning/authn/session.md` beside `BearerGuard` (idea `IDEAS.md` `[COOKIE_SESSION_GUARD]`).
- Capability: platform security-decode over the access cookie, `Jwt.verify` fold, CSRF double-submit composed into one requirement provision, refusals on the `bearer` reject row.
- Shape: same `HttpApiMiddleware.Tag` grammar, `provides: CurrentClaims`; the runtime serve wave mounts it as one more guard row.
- Unlocks: IDEAS.md [COOKIE_SESSION_GUARD] — a zero-JS-token browser app composes full session auth declaratively, the serve wave mounting one more guard row over no hand parser.
- Anchors: `HttpApiBuilder.securityDecode` (`libs/typescript/.api/effect-platform.md`), `CookieSpec`, `Cookie.verify`.

[APIKEY_SUBJECT_DIMENSION]-[QUEUED]: `ApiKeyStore` gains the subject dimension every peer store carries.
- Capability: machine keys enumerate and bulk-revoke by subject — the store port carries subject-keyed reads beside the prefix index, so rotation, offboarding, and breach response reach every key a principal holds.
- Shape: a subject-keyed read (and the revocation sweep it powers) on the `ApiKeyStore` port in `libs/typescript/security/.planning/authn/credential.md`, matching the subject dimension the session and identity ports carry.
- Unlocks: revoke-every-session gains its machine-key twin; DSAR and forensics cover machine principals.
- Anchors: `credential.md` `ApiKeyStore` (insert/byPrefix roster); `session.md` subject-family revocation precedent.
- Atomic: one port member and its consumers.

[TOKEN_ALPHABET_OWNER]-[QUEUED]: Token alphabets home at the crypto authority — two byte-identical literals collapse.
- Capability: the base62 token alphabet is one `crypt/sign` value every mint reads, so an alphabet change is one edit and the two spellings cannot drift.
- Shape: the alphabet value beside `Crypto.token` in `libs/typescript/security/.planning/crypt/sign.md`; `authn/credential.md` `_ALPHABET` and `authn/session.md` `_CSRF_ALPHABET` compose it.
- Unlocks: one entropy-alphabet spelling; a variant alphabet stays a caller value.
- Anchors: the byte-identical literals at `credential.md` and `session.md`; the `Crypto.token` owner.
- Atomic: one value hoist and two composition swaps.

[DOPPLER_COORDINATE_CONTRACT]-[QUEUED]: Doppler coordinates cross the custody seam as one typed contract.
- Capability: the token/project/config coordinate spellings the custodian reads and the deploy plane stamps derive from one owner, and the injection path for each coordinate is proven, never assumed.
- Shape: a typed coordinate contract at `libs/typescript/security/.planning/crypt/secret.md`'s config reads; iac `kube/workload.md`'s env assembly names its injection source per coordinate.
- Unlocks: custodian boot cannot fail on a coordinate the deploy plane never stamped.
- Route: prove whether `doppler run` injects `DOPPLER_PROJECT`/`DOPPLER_CONFIG` into the wrapped process env; on refute, the workload `_POLICY` rows gain the coordinates.
- Anchors: `secret.md` `Config.redacted("DOPPLER_TOKEN")`/`Config.string("DOPPLER_PROJECT")`/`Config.string("DOPPLER_CONFIG")`; iac `workload.md` `DOPPLER_TOKEN` secret and `doppler run --` entrypoint law.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[CSRF_HEADER_FIELD]-[COMPLETE]: the `CookieSpec.csrf` row carries `header: "x-csrf-token"` with the one-row-read-twice law; runtime `serve/route.md` `_csrfed` reads `CookieSpec.csrf.header` (route literal deleted) and `browser/route.md` `Vault.csrf` stamps the same field — all three ends verified spelling one row.
[WORKLOAD_PAGE]-[COMPLETE]: `authn/workload.md` authored — the closed `GrantRequest` family (client-credentials, RFC 8693 exchange, refresh, device, CIBA) over one discovered `Configuration` per issuer, DPoP bound once per principal with `cnf.jkt` from the handle's own thumbprint (RFC 9449 bare RFC 7638 value, never the URN form), `JwksLedger` bound as the certified client's JWKS custody through a per-package unit projection, and the transport-credential projection the runtime wave mounts; the card's original "client-credentials, token exchange, DPoP" scope widened to the full admitted surface.
[LEASE_SPEC_SCHEMA]-[COMPLETE]: `LeaseSpec` lands as a Schema struct beside `_ACCESS` with its exported type, so the `[BOUNDARY]: LeaseSpec` seam edge carries a real shape.
[SECRETS_LIST_CENSUS]-[COMPLETE]: `census` landed on `crypt/secret.md`'s custodian — one `secrets.list` read narrowed to the lease allowlist with dynamic leasing OFF (an inline lease surfaces no revocable handle), decoded through `Schema` and folded into the same `_publish` revision path `probe` and the rolling tick take, so custody's own equality check suppresses a rotation event where no byte moved; the catalog gained the trap that `SecretsListResponse.secrets` types four fixed example key names over an arbitrary name-keyed wire map.
[WEBAUTHN_TRUST_PROBE]-[COMPLETE]: probe resolved at the catalog leg — `.api/simplewebauthn-server.md` declares both trust services module-singletons (`SettingsService` a module const, no instance construction), and `authn/webauthn.md` already carries the refute-arm outcome: the single-policy-per-process composition law with the deployment-split remedy.
[REJECT_TELEMETRY_COLLAPSE]-[COMPLETE]: reject-telemetry collapse — `crypt/verify.md` minted `Reject`, the one Convention-named reject stream with a kind discriminant; session/oauth/webauthn/credential re-keyed their reject arms onto it, `BearerGuard` gained the `bearer` row, and webauthn challenge refusals gained the `ceremony` row.
[WIRE_LAW_INSTRUMENT_FOLD]-[COMPLETE]: wire-law instrument fold — seven page-local `security_*` mints (sign x5, secret, claim) converted to `Convention.instrument.*` rows; free-string `tenant`/`reason` tag keys re-keyed to `Convention.rasm.tenant`/`Convention.rasm.securityReason`; the tenant-cardinality obligation recorded as the `access -> runtime` `[PROJECTION]: rasm.tenant` seam.
[AUDIT_PAGE_AUTHORED]-[COMPLETE]: `access/audit.md` authored — `SecurityFact` vocabulary with the `_points` registry brand, `Witness` publish seam, `AuditJournal` port, and pseudonymized egress; the pseudonymizer landed KEYED (`Crypto.sign` behind `Pseudonym`), not this card's `Crypto.fingerprint`, which the egress law rejects for low-entropy subjects.
[LOUD_ARM_FACT_PUBLICATION]-[COMPLETE]: fact publication lands beside every loud arm — `session.md` revokes the reused subject before its backpressured `Reuse` fact, `secret.md` serializes full and targeted cell revisions and publishes custody before its rotation taps, and webauthn/claim/sign retain their typed verdict rails beside `Witness`.
[AUDIT_PACK_PRODUCER]-[COMPLETE]: the folder mints its own deploy-plane pack — `Audit.wire` carries the provenance key beside `Audit.pack`, which seals the core-encoded board and the `Alert.of` burn specs into one value, so the iac `_PACKS` tuple admits `security.audit` on a landed projection instead of holding a wire nobody mints.
[AUDIT_JOURNAL_MIRROR_ROUTE]-[COMPLETE]: the data-side mirror routes to its real owner — `data:journal/fact#RAIL` satisfies `AuditJournal` by projecting each record onto the one fact entrypoint, so the lead and the port boundary name the fact rail rather than the event-journal append owner.
[AUDIT_SNAPSHOT]-[COMPLETE]: `Audit.snapshot` landed on `access/audit.md` `[05]-[BOARD]` — `DashboardModel.snapshot` filtered to the `Convention.instrument.security*` rows and sealed into the typed `Snapshot`/`SnapshotRow` receipt.
[KDF_CALIBRATION_BENCH]-[COMPLETE]: KDF calibration on `crypt/sign.md` `[07]-[CALIBRATION]` runs bulkheaded `Crypto.digest`, emits the complete core `Claim.Band` shape, calls `Claim.admit`, and grades p99 against row-owned `CryptoCost.targetMs` values reused by `_argonMs`; `Jwt` throughput claims ride the same `bench` entrypoint.
[SECURITY_PACK_PANELS]-[COMPLETE]: core `observe/board.md` `_PACKS.security` landed the deny, replay, rotation, cold-JWKS, quarantine, and shredded-open trends beside the JWKS-resolve and key-derivation quantile panels, so every `Convention` security instrument carries a board consumer; the tenant half stayed refuted, since `DashboardModel.of` prepends the tenant variable to every pack and the security rows declare no tenant dimension.
