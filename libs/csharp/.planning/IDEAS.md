# [CSHARP_BRANCH_IDEAS]

Cross-package C# concert ideas — concepts coupling two or more C# packages into one capability, distilled from the folder pools. A concept living inside one folder stays in that folder's pool; a concept spanning C#, Python, and TypeScript at once lives at the cross-libs tier and is referenced as a wire seam, never restated. Each idea is a card: a bracketed slug leader with the capability, what it unlocks, and the gap or technique it draws on.

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

[SIGNAL_CONCERT]-[ACTIVE]: One receipt-projected signal fabric across the C# strata.
- Capability: typed receipts and hook facts stay the single truth; instruments, logs, and spans project from them, never a parallel telemetry model.
- Shape: AppHost's Observability spine owns projection, Health, bundles, the hook rail, and benchmark receipts; Persistence's receipt-slot registry feeds the `store.<domain>.<verb>` series; Compute contributes progress rows onto the same fan; every root pushes OTLP.
- Unlocks: metric→trace→profile click-through through trace-based exemplars and the profile-id stamp, incident bundles replaying buffered logs, and one dashboard vocabulary the TypeScript iac stratum compiles.
- Anchors: branch observability catalogs under `libs/csharp/.api/`; the diagnostics doctrine; AppHost Observability pages; Persistence store observability.
- Ripple: `libs` `[UNIFIED_SIGNAL_FABRIC]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
