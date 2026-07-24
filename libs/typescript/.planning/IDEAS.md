# [TYPESCRIPT_BRANCH_IDEAS]

Branch-level cross-package concert — higher-order ideas coupling two or more TS folders, distilled from the folder registers, never folder-local concepts. A cross-language idea lives in `libs/.planning/IDEAS.md`; `[1]-[OPEN]` holds the live concert and `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[BRANCH_SCHEMA_VARIANTS]-[QUEUED]: One schema declaration yields every variant — `VariantSchema` derivation collapses parallel wire/domain/patch spellings branch-wide.
- Capability: multi-variant schema construction derives wire, domain, insert/update, and json forms from one field-level declaration, so a family spelling each variant by hand — read-side relations, wire rows and their domain twins, config admission shapes — declares once and projects each variant totally.
- Shape: a derivation law beside core's vocabulary-table owner, with adoption rows where parallel spellings exist — data read models and journal wire rows first, config admission second.
- Unlocks: variant drift becomes impossible by construction; a new projection form is one variant key, never a re-spelled schema.
- Anchors: `VariantSchema.*` (`libs/typescript/.api/effect-experimental.md`), the unexploited substrate member this concert exploits; core `value/schema.md` vocabulary-table owner (carded); data `read/query.md` `Model.Class` relations; the derivation bound at `libs/typescript/.planning/RULINGS.md` `[02]-[COLLAPSE]`.

[CONFIG_FAMILY_SCOPE]-[QUEUED]: One config-contract law binds the branch — the `Setting`-shaped family claim is decided and aligned.
- Capability: environment custody reads one ruled scope — every folder and app resolves its variables through a boot-proven `Setting`-shaped contract, or the family claim narrows to the runtime and app planes; scattered per-site `Config` reads stop being an undecided middle.
- Shape: the scope decision graduates to `libs/typescript/.planning/RULINGS.md`; alignment rows follow on the security ceremony pages (inline `Config.duration`/`Config.string` reads folding into a folder contract) and `libs/typescript/iac/.planning/program/spec.md`/`libs/typescript/iac/.planning/operate/secret.md` — or the claim narrows in place at `libs/typescript/runtime/.planning/proc/config.md` `[04]`.
- Unlocks: a malformed environment fails one boot proof per folder instead of surfacing at first read mid-request; config custody has one law branch-wide.
- Anchors: `libs/typescript/runtime/.planning/proc/config.md` `[04]` family claim; `libs/typescript/security/.planning/crypt/sign.md` inline `Config.duration` reads; the `authn/oauth.md`/`authn/webauthn.md` ceremony config rows.
- Tension: per-folder contracts buy boot-time proof at the cost of a `Setting`-shaped owner per folder; narrowing the claim leaves ceremony config ad-hoc — the ruling decides which cost the branch prices.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[BRANCH_SIGNAL_PLANE]-[COMPLETE]: five legs settled — `core/observe/convention.md` wire rows, `core/observe/board.md` packs, `runtime/otel/emit.md` lanes, `runtime/otel/meter.md` bridge, `iac/operate/observe.md` compile leg; the census feed landed as `Pulse.Board` filling `DashboardModel` payloads at the app deploy-feed seam and reaching iac as the `runtime.pulse` pack row — `BoardPack` never existed as an owner.
[BRANCH_HOOK_RAIL]-[COMPLETE]: core `observe/tap.md` owns brand and modality vocabulary, runtime `otel/emit.md#HOOKS` runs the one dispatch engine, data `journal/append.md#HOOK_POINTS` conformed to `Tap.point` rows over a publisher port; tap seams mirror as `[SHAPE]: Tap.Registry` (data/ui/security↔runtime) with the ruled iac substrate boundary as `[SHAPE]: Tap.Point` (core↔iac).
[BRANCH_CONTEXT_CARRIAGE]-[COMPLETE]: core `Carrier.Current` retains admitted tracestate and scrubbed baggage under the live span, runtime `Propagation` continues each dialect-decoded ingress through `OtelBridge.makeExternalSpan`/`withSpanContext`, data `Journal.envelope`/`Journal.carrier` round-trip the CloudEvents context with `rasmtenant`→baggage reconstruction, and fanout and MQTT inject their exact NATS/Kafka/MQTT rows.
[CONTENT_IDENTITY]-[COMPLETE]: one seed-zero `XxHash128` mint at `core/value/contentKey`, every verifying and keying site delegating; law settled at `ARCHITECTURE.md` `[04]-[INTERNAL]`.
[INTERCHANGE_DECODE_ONCE]-[COMPLETE]: one keyed codec census at `core/interchange/codec` decodes every C#-minted family exactly once; law settled at `ARCHITECTURE.md` `[04]-[INTERNAL]`.
[JOURNAL_SPINE]-[COMPLETE]: `data/journal/append` owns the one atomic write with ledger and outbox in-commit, the read side folding through `data/read/fold`; law settled at `ARCHITECTURE.md` `[04]-[INTERNAL]`.
[TENANCY_SCOPE]-[COMPLETE]: `Tenant.within` is the single scoped write path over `AppIdentity`, isolation a scope value never a fork; law settled at `ARCHITECTURE.md` `[04]-[INTERNAL]`.
[CROSS_LANGUAGE_INVARIANTS]-[COMPLETE]: wire ownership, content identity, clock, quantity, and receipt-family invariants frozen under `tests/contracts` corpus assertion; law settled at `ARCHITECTURE.md` `[03]-[SEAMS]`/`[04]-[INTERNAL]`.
