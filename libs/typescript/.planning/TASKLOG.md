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
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[FOLDER_INTERNAL_SPINE_DIAGRAMS]-[QUEUED]: Every folder `ARCHITECTURE.md` renders its interior flow, so the crossing law reads as a graph beside its prose.
- Capability: each package's `[04]-[INTERNAL]` carries the archetype flowchart its own subsystem spine earns — entry, transform, egress with edge labels naming the carried fact — so a cold reader derives interior order from the map instead of reconstructing it from paragraph clauses; the branch tier already renders both of its spines and the folder tier alone lags.
- Shape: one `[04]-[INTERNAL]` flowchart per genuine spine on each of the six `libs/typescript/<pkg>/ARCHITECTURE.md` files, seated above the existing crossing-law prose.
- Unlocks: the folder tier reaches the shape owner's full `[04]` contract, and every interior seating decision has a diagram to contradict.
- Anchors: `.claude/skills/docgen/templates/architecture.template.md` INTERNAL archetype and its one-fence-per-spine rule; `libs/typescript/.planning/ARCHITECTURE.md` `[04]-[INTERNAL]`, the branch-grain exemplar; each folder's landed `[02]-[STRATA]` fence supplying the owner roster.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[VARIANT_FAMILY_CENSUS]-[COMPLETE]: the census resolves the adoption question rather than opening it — `@effect/sql` `Model.Class` IS `VariantSchema.make` over the `select`/`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate` variant set, so every relation family already derives (`data/read/query.md`, `data/journal/append.md`, `data/journal/retain.md`) and a bespoke `VariantSchema.make` earns a seat only where the variant axis is NOT the SQL one; the branch catalogue now carries the full constructor surface so the next adopter reads it there.
[SIGNAL_PLANE_CLOSURE]-[COMPLETE]: spellings reconcile across the five legs — meter `Pulse.Board` projects through the app deploy-feed into iac's `runtime.pulse` pack row, the mirror-less direct `Pulse.Board` seam is absent, and tap labels align; `Convention` and `AppIdentity` rule every leg, while uncataloged Foundation panel members remain explicit terminal research rather than confirmed claims.
[TAP_GRAMMAR_CONFORMANCE]-[COMPLETE]: every data point name re-proves the core `TapPoint` brand through `Tap.point` mints in `data/journal/append#HOOK_POINTS`; seam edges landed with identical `[KIND]` labels — `[SHAPE]: Tap.Registry` at data/ui/security↔runtime, `[SHAPE]: Tap.Point` at core↔data and core↔iac.
[EXTERNAL_SPAN_CONTINUATION]-[COMPLETE]: HTTP, NATS, Kafka, MQTT, Connect, and CloudEvents cross core `Carrier.extract`/`inject` rows; `runtime/otel/emit#CONTINUATION` scopes scrubbed context through `Carrier.Current`, continues `Tracer.ExternalSpan`, and exposes the carried live context to every egress; `Journal.carrier` restores `rasmtenant` as `rasm.tenant` baggage.
[OTEL_SUBSTRATE_HOMING]-[COMPLETE]: `@effect/opentelemetry` demoted out of the branch substrate tier as a single-consumer package — branch registry row and branch catalogue removed; the runtime folder registry and `runtime/.api/` own the package and its catalogue.
[PLATFORM_BINDING_ROWS]-[COMPLETE]: branch substrate registry enumerates the `-node`/`-bun`/`-browser` binding rows, matching the branch catalogues and the folder registries.
[SEAM_REGISTRY_MERMAID]-[COMPLETE]: branch `[03]-[SEAMS]` renders as the kinded Mermaid seam registry, folder mirrors spelling the C# endpoint contracts verbatim.
