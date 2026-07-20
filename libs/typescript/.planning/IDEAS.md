# [TYPESCRIPT_BRANCH_IDEAS]

Branch-level cross-package concert — higher-order ideas coupling two or more TS folders, distilled from the folder registers, never folder-local concepts. A cross-language idea lives in `libs/.planning/IDEAS.md`; `[1]-[OPEN]` holds the live concert and `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[BRANCH_SIGNAL_PLANE]-[ACTIVE]: One signal plane spans the branch — every folder emits through the core `Convention` vocabulary, runtime alone bridges the OTLP wire, iac compiles the backend.
- Capability: Folder-blind observability — instruments, spans, and logs minted in owning folders correlate estate-wide, with raw `@opentelemetry/*` imports confined to `runtime` and the core semconv vocabulary; typed pack feeds now close the loop — runtime's `BoardPack` census and security's board/alert pack arrive at the compile leg as data, so boards derive from the rows the emitters write.
- Shape: `core/observe` wire rows (dotted `rasm.*` names, UCUM units, scope = package id, pinned semconv schema, `NoUTF8EscapingWithSuffixes` translation), the `runtime/otel` emit/crash/vital/meter bridge projecting work-plane Facts to `Convention`-keyed instruments, profile and bench vocabulary tiers joining through the core board pack dispatch, and `iac/operate/observe` realizing stores, dashboards, and alerts from the same `DashboardModel` rows and pack feeds.
- Unlocks: Dashboards and SLO burn alerts compiled from the vocabulary the emitters use, breaking at type-check instead of drifting; any app inherits the full signal plane by composing the export layer at its root.
- Anchors: `core/observe/convention.md`; `core/observe/board.md` pack dispatch; `runtime/otel/emit.md`; `runtime/otel/meter.md` census projection (carded); `iac/operate/observe.md` pack-ingest rows (carded); the `dataflow-system.md` `AppIdentity` resource law.
- Ripple: `libs` `[UNIFIED_SIGNAL_FABRIC]`.

[BRANCH_HOOK_RAIL]-[QUEUED]: One tap law spans the branch — core mints the hook vocabulary, runtime runs the one dispatch engine, every folder registry executes the same shape.
- Capability: `rasm.<pkg>.<domain>.<point>` point brand, closed veto/observe/replay modality vocabulary, `AppIdentity` scoping, and subscriber-fault isolation spell once in core; the data, ui, security, and iac registries mount their point sets on runtime's dispatch engine unchanged, so telemetry, audit, and app policy subscribe branch-wide with zero scattered emit sites and co-resident apps never contend over points.
- Shape: core `observe/tap.md` as the vocabulary owner, runtime's dispatch engine in the Hooks plane, folder point sets as registry rows — conformance holds when every folder registry names its points in the core brand grammar.
- Unlocks: one hook rail from browser ui to deploy plane; a new observation concern is a subscriber, never an owner edit in any folder.
- Anchors: core `observe/tap.md` (carded); runtime `otel/emit.md` dispatch engine (carded); data `[DATA_HOOK_TAP_REGISTRY]`; ui `[HOOK_RAIL]`; security `[0003]`; iac `[0004]`.
- Tension: iac's `rasm.iac.<tier>.<point>` rows bind pulumi `ResourceHook` lifecycles rather than the runtime engine — the brand grammar still governs, the execution substrate differs by ruled boundary.

[BRANCH_CONTEXT_CARRIAGE]-[QUEUED]: Causal context survives every hop — one carrier codec, one envelope projection, carriers binding both, foreign spans continued.
- Capability: core's W3C carrier codec owns every header dialect, data's CloudEvents envelope carries journal facts wire-neutral, runtime carriers bind codec and envelope per transport row, and a foreign `traceparent` entering the branch continues as an external span through `Tracer.externalSpan` — trace and tenant context cross Connect, NATS, MQTT, Kafka, and webhook boundaries with zero forks.
- Shape: the carrier, envelope, and binding cards already open in their owning folders, joined here by the external-span continuation rows landing where ingress decodes a foreign context — carrier ingress, webhook intake, EventLog sync.
- Unlocks: end-to-end causality across the C#, python, and TS peers over any transport; cost attribution rides tenant baggage through broker hops by construction.
- Anchors: core `interchange/carrier.md` dialect table (carded); data `[RELAY_CLOUDEVENTS_PROJECTION]`; runtime `[CARRIER_CODEC_BINDING]`/`[JOURNAL_ENVELOPE_CARRIAGE]`; `Tracer.externalSpan` (`libs/typescript/.api/effect.md`), the unexploited substrate member this concert exploits.

[BRANCH_SCHEMA_VARIANTS]-[QUEUED]: One schema declaration yields every variant — `VariantSchema` derivation collapses parallel wire/domain/patch spellings branch-wide.
- Capability: multi-variant schema construction derives wire, domain, insert/update, and json forms from one field-level declaration, so a family spelling each variant by hand — read-side relations, wire rows and their domain twins, config admission shapes — declares once and projects each variant totally.
- Shape: a derivation law beside core's vocabulary-table owner, with adoption rows where parallel spellings exist — data read models and journal wire rows first, config admission second.
- Unlocks: variant drift becomes impossible by construction; a new projection form is one variant key, never a re-spelled schema.
- Anchors: `VariantSchema.*` (`libs/typescript/.api/effect-experimental.md`), the unexploited substrate member this concert exploits; core `value/schema.md` vocabulary-table owner (carded); data `read/query.md` `Model.Class` relations.
- Tension: a variant is a projection of one declaration, never a second truth — families whose forms differ semantically rather than structurally stay separate declarations.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CONTENT_IDENTITY]-[COMPLETE]: one seed-zero `XxHash128` mint at `core/value/contentKey`, every verifying and keying site delegating; law settled at `dataflow-system.md`.
[INTERCHANGE_DECODE_ONCE]-[COMPLETE]: one keyed codec census at `core/interchange/codec` decodes every C#-minted family exactly once; law settled at `dataflow-system.md`.
[JOURNAL_SPINE]-[COMPLETE]: `data/journal/append` owns the one atomic write with ledger and outbox in-commit, the read side folding through `data/read/fold`; law settled at `dataflow-system.md`.
[TENANCY_SCOPE]-[COMPLETE]: `Tenant.within` is the single scoped write path over `AppIdentity`, isolation a scope value never a fork; law settled at `dataflow-system.md`.
[CROSS_LANGUAGE_INVARIANTS]-[COMPLETE]: wire ownership, content identity, clock, quantity, and receipt-family invariants frozen under `tests/contracts` corpus assertion; law settled at `dataflow-system.md`.
