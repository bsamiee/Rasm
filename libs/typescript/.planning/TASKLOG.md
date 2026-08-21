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

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[VARIANT_FAMILY_CENSUS]-[COMPLETE]: the census resolves the adoption question rather than opening it — `@effect/sql` `Model.Class` IS `VariantSchema.make` over the `select`/`insert`/`update`/`json`/`jsonCreate`/`jsonUpdate` variant set, so every relation family already derives (`data/read/query.md`, `data/journal/append.md`, `data/journal/retain.md`) and a bespoke `VariantSchema.make` earns a seat only where the variant axis is NOT the SQL one; the branch catalogue now carries the full constructor surface so the next adopter reads it there.
[FOLDER_INTERNAL_SPINE_DIAGRAMS]-[COMPLETE]: every folder architecture carries its owner flow from ingress through transformation to egress.
[SIGNAL_PLANE_CLOSURE]-[COMPLETE]: the five legs compose core `Convention` and `Identity.App`; runtime pulse feeds IaC boards without a mirror rail.
[TAP_GRAMMAR_CONFORMANCE]-[COMPLETE]: data sends its `Tap.Registry` to runtime; UI Hook and security Audit are registrars seating core's `Tap.Rail`; IaC stays outside Tap.
[EXTERNAL_SPAN_CONTINUATION]-[COMPLETE]: every transport composes `Carrier.extract`/`inject`; `Journal.carrier` restores `Identity.Tenant.scope` through `Convention.rasm.tenant`.
[OTEL_SUBSTRATE_HOMING]-[COMPLETE]: `@effect/opentelemetry` demoted out of the branch substrate tier as a single-consumer package — branch registry row and branch catalogue removed; the runtime folder registry and `runtime/.api/` own the package and its catalogue.
[PLATFORM_BINDING_ROWS]-[COMPLETE]: branch substrate registry enumerates the `-node`/`-bun`/`-browser` binding rows, matching the branch catalogues and the folder registries.
[SEAM_REGISTRY_MERMAID]-[COMPLETE]: branch `[03]-[SEAMS]` renders as the kinded Mermaid seam registry, folder mirrors spelling the C# endpoint contracts verbatim.
