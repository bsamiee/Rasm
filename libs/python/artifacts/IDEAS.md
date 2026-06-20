# [PY_ARTIFACTS_IDEAS]

The folder's forward pool of higher-order concepts, each grounded in artifact production and the host-free companion charter. Open ideas are cards in `[1]-[OPEN]`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Each idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[ENCODED_MARK_OWNER]-[QUEUED]: one encoded-mark axis covers QR/Micro-QR and linear 1D marks.
- Capability: machine-readable mark production joins the preview owner as one encoded-mark family, not a QR-only special case or per-symbology class set.
- Shape: `figures/preview#PREVIEW` carries one `PreviewOp.MARK` row and one `Symbology` axis over `segno` QR/Micro-QR plus `python-barcode` linear registry rows.
- Unlocks: dependency-free SVG mark output for QR, Micro-QR, and linear 1D marks without phantom DataMatrix/PDF417 membership.
- Anchors: `figures/preview#PREVIEW`, `PreviewOp.MARK`, `Symbology`, `segno`, `python-barcode` `get_barcode_class`, `SVGWriter`, `PROVIDED_BARCODES`, and `.api/python-barcode.md`.
- Tension: the linear rows stay blocked on uv-sync plus `assay api` reflection; DataMatrix/PDF417 belong to a separate 2D-matrix owner.

[CONTENT_KEYED_REUSE_AND_SIGNAL]-[QUEUED]: artifact receipts become the reuse and measurement edge.
- Capability: artifact production contributes cache-hit and measured-output facts through the existing receipt fold instead of minting an artifacts cache or metrics owner.
- Shape: every producer threads `(ContentKey, Work)` into runtime lane admission and reports duration, bytes, compression ratio, and hit/miss through `receipt/receipt#RECEIPT` `contribute`.
- Unlocks: expensive PDFs, notebooks, publication tables, bundles, and offscreen scenes become content-keyed elision targets and observable render outputs with no new artifacts surface.
- Anchors: `ContentIdentity.of(...)`, `ArtifactReceipt`, `receipt/receipt#RECEIPT`, branch `CONTENT_ADDRESSED_REUSE_FABRIC`, branch `ONE_MEASURED_SIGNAL_STREAM`, runtime `execution/lanes`, and runtime `observability/metrics`.
- Tension: runtime lane-admission elision and the `MeterProvider` instrument set must land before artifacts can consume them.

[OUTWARD_FIGURE_HANDOFF_AND_DRIFT]-[QUEUED]: figures leave through one handoff axis and one drift guard.
- Capability: outward artifact figures share the Python graduation rail and structural drift detector rather than creating an artifacts-specific handoff or canonical-name guard.
- Shape: figure, table, chart, and scene outputs cross outward only as the `compute/graduation` `HandoffAxis` `model-asset` case keyed by `ContentIdentity`.
- Unlocks: artifacts can export sibling-consumable visual assets while proving zero private handoff rails and zero canonical-concept re-mints.
- Anchors: `ContentIdentity`, `RuntimeRail`, `Receipt`, `ReceiptContributor`, `ArtifactReceipt`, `compute/graduation#GRADUATION`, branch `ONE_GRADUATION_RAIL_OUTWARD`, and branch `CROSS_PACKAGE_DRIFT_GUARD`.
- Tension: upstream `compute/graduation` `HandoffAxis` and runtime `Structural.drift` must land before artifacts can verify the live drift query.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
