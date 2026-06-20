# [PERSISTENCE_TASKLOG]

The open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->



[PERSISTENCE_BIM_ARTIFACT_INDEX]-[QUEUED]: Persistence lands the content-keyed ArtifactIndexRow joining the Bim IFC semantic graph and its tessellated GLB as two projections of one content-addressed artifact, so the Bim TessellationOutcome cache-hit reads the prior GLB by ArtifactKey and the BimWire snapshot joins the same content key, never re-crossing the companion for an unchanged model.
- Capability: Gives the Bim tessellation cache and wire snapshot a durable content-addressed home: Persistence indexes the IFC artifact and the re-imported GLB by the Compute content-key, so a re-tessellation or re-fetch resolves by reference rather than re-computing.
- Shape: A Persistence ArtifactIndexRow keyed on the Compute InterchangeIdentity content-key carrying the IFC-semantic and GLB-tessellation projections, read by the Bim TessellationRequest.Resolve cache leg, the BimWire content-key join, and the INCREMENTAL_DELTA_REIMPORT prior-snapshot lookup.
- Unlocks: The Bim TESSELLATION_OUTCOME_RECEIPT cache-hit fact and the INCREMENTAL_DELTA_REIMPORT prior-model join resolve against a real durable index, and a federation client re-fetches only changed BimWire element rows by content-key.
- Anchors: Exchange/tessellation#TESSELLATION_BRIDGE ArtifactKey; Exchange/wire#WIRE_PROJECTION ContentKey; Exchange/import#IMPORT_RAIL Reimport prior-model join; Review/diff#MODEL_DIFF ElementFingerprint; csharp:Rasm.Persistence/Query ArtifactIndexRow.
- Tension: Bim depends strictly upward and mints no Persistence reference: the artifact index is the Persistence owner's concern read at the seam, the Bim side carrying only the ArtifactKey the index is keyed by.
- Ripple: counterpart of `Rasm.Bim` `[VERSIONED]` idea + Exchange/Review content-key owners (`wire`/`diff`/`tessellation`).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
