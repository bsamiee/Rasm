# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

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

[T1]-[BLOCKED]: `ElementSet` query algebra reaches the AppUi viewport as a declared `[PROJECTION]` seam.
- Capability: model-query results (`Model/query` `ElementSet`) rendered as AppUi viewport/inspector selections.
- Shape: one `Model/query -> csharp:Rasm.AppUi/<owner> # [PROJECTION]` seam row with the consuming AppUi page fence.
- Unlocks: saved-query overlays, selection-driven dashboards, query-scoped exports.
- Anchors: `Model/query` `ElementSet`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi page names a consumer today — the seam row re-enters `ARCHITECTURE.md` `[02]-[SEAMS]` only when one does; deferred pressure never rides the ledger.

[T2]-[BLOCKED]: `ScheduleNetwork` CPM/4D projection reaches the AppUi Charts plane as a declared `[PROJECTION]` seam.
- Capability: 4D construction-sequencing and critical-path dashboards over the `Planning/schedule` domain.
- Shape: one `Planning/schedule -> csharp:Rasm.AppUi/Charts # [PROJECTION]` seam row with the consuming dashboards fence.
- Unlocks: 4D playback tiles, earned-value overlays beside the existing `Planning/cost` receipt row.
- Anchors: `Planning/schedule` `ScheduleNetwork`; the AppUi `[V9]` growth register carries the reciprocal record.
- Tension: no AppUi consuming fence exists today — same re-entry law as `[T1]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
