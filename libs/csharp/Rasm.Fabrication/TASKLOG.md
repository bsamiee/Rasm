# [FABRICATION_TASKLOG]

Open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

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

[SPECIALIZED_ENVELOPE_ADMISSION]-[QUEUED]: Enforce `SpecializedToolpathEnvelope` admission at its owner and route every consumer through the admitted rail.
- Capability: `SpecializedToolpathEnvelope` mints only through an admission factory that folds payload validity, so an invalid envelope is unrepresentable and the public primary constructor retires.
- Shape: a private primary constructor with a static `Fin<SpecializedToolpathEnvelope>` admission factory on the owner; Toolpath `wire`, `bevel`, `link`, `motion`, Posting `program`, `dialect`, and Verify `simulate` construct through the factory and drop their local `payload.IsValid` revalidation branches.
- Unlocks: one admission seam for every specialized-toolpath consumer, collapsing six duplicated advisory revalidations into the owner's single fold.
- Anchors: the value-object admission pattern the corpus already carries; the `SpecializedToolpathEnvelope` owner in `Process/owner.md`; the consumer pages holding direct `new SpecializedToolpathEnvelope(...)` construction.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` landed the fact union, instrument roster, contributor port, projection fan, and classification rows; `FabricationRuntime` carries the `FabricationTap` port and the AppHost seam is mirrored at `[03]-[SEAMS]`.
