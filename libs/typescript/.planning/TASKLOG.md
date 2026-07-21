# [TYPESCRIPT_BRANCH_TASKLOG]

Branch-level cross-package work — the wiring, guards, and seams no single TS folder owns, distilled from the branch concert; per-folder work stays in the owning folder ledger. `[1]-[OPEN]` holds live tasks and `[2]-[CLOSED]` compacts a finished or dropped task to a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[VARIANT_FAMILY_CENSUS]-[QUEUED]: Parallel-spelling census pins the first `VariantSchema` adopters.
- Capability: a census of families spelling wire/domain/patch variants by hand, each pinned to its owning page by repo-relative path with the variant keys it derives; the first adoption rows land on the census verdict.
- Shape: census rows riding `[BRANCH_SCHEMA_VARIANTS]`'s adoption law; owners pinned before any derivation lands.
- Unlocks: IDEAS.md [BRANCH_SCHEMA_VARIANTS] — the first `VariantSchema` adopters land on named owners, parallel hand-spelled variants collapsing to one declaration.
- Anchors: `VariantSchema.*` (`libs/typescript/.api/effect-experimental.md`); data `read/query.md` relation models; core `interchange/codec.md` wire families.
- Atomic: census and first adoption pins.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[SIGNAL_PLANE_CLOSURE]-[COMPLETE]: spellings reconcile across the five legs — meter `Pulse.Board` projects through the app deploy-feed into iac's `runtime.pulse` pack row, the mirror-less direct `Pulse.Board` seam is absent, and tap labels align; `Convention` plus `AppIdentity` rule every leg, while uncataloged Foundation panel members remain explicit terminal research rather than confirmed claims.
[TAP_GRAMMAR_CONFORMANCE]-[COMPLETE]: every data point name re-proves the core `TapPoint` brand through `Tap.point` mints in `journal/append.md#HOOK_POINTS`; seam edges landed with identical `[KIND]` labels — `[SHAPE]: Tap.Registry` at data/ui/security↔runtime, `[SHAPE]: Tap.Point` at core↔data and core↔iac.
[EXTERNAL_SPAN_CONTINUATION]-[COMPLETE]: HTTP, NATS, Kafka, MQTT, Connect, and CloudEvents cross core `Carrier.extract`/`inject` rows; `emit#CONTINUATION` scopes scrubbed context through `Carrier.Current`, continues `Tracer.ExternalSpan`, and exposes the carried live context to every egress; `Journal.carrier` restores `rasmtenant` as `rasm.tenant` baggage.
[OTEL_SUBSTRATE_HOMING]-[COMPLETE]: `@effect/opentelemetry` demoted out of the branch substrate tier as a single-consumer package — branch registry row and branch catalogue removed; the runtime folder registry and `runtime/.api/` own the package and its catalogue.
[PLATFORM_BINDING_ROWS]-[COMPLETE]: branch substrate registry enumerates the `-node`/`-bun`/`-browser` binding rows, matching the branch catalogues and the folder registries.
[SEAM_REGISTRY_MERMAID]-[COMPLETE]: branch `[03]-[SEAMS]` renders as the kinded Mermaid seam registry, folder mirrors spelling the C# endpoint contracts verbatim.
