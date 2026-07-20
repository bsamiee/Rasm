# [TYPESCRIPT_BRANCH_TASKLOG]

Branch-level cross-package work — the wiring, guards, and seams no single TS folder owns, distilled from the branch concert; per-folder work stays in the owning folder ledger. `[1]-[OPEN]` holds live tasks and `[2]-[CLOSED]` compacts a finished or dropped task to a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[OTEL_SUBSTRATE_HOMING]-[COMPLETE]: `@effect/opentelemetry` demoted out of the branch substrate tier as a single-consumer package — branch registry row and branch catalogue removed; the runtime folder registry and `runtime/.api/` own the package and its catalogue.
[PLATFORM_BINDING_ROWS]-[COMPLETE]: branch substrate registry enumerates the `-node`/`-bun`/`-browser` binding rows, matching the branch catalogues and the folder registries.
[SEAM_REGISTRY_MERMAID]-[COMPLETE]: branch `[03]-[SEAMS]` renders as the kinded Mermaid seam registry, folder mirrors spelling the C# endpoint contracts verbatim.
