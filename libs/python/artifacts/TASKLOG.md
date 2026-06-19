# [PY_ARTIFACTS_TASKLOG]

The folder's open and closed work, distilled from `IDEAS.md`. Open tasks are cards in `[1]-[OPEN]` with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` leader; closed tasks move to `[2]-[CLOSED]` with `[COMPLETE]`/`[DROPPED]`. Each task names the exact sub-domain or file it lands in.

## [1]-[OPEN]

[BLOCKED] Widen the imaging code arm to the full encoded-mark family on `figures/preview.md` — from `ENCODED_MARK_OWNER`.
- BLOCKED on the `python-barcode` catalogue resolving on uv-sync. The segno QR/Micro-QR half is REALIZED: `figures/preview#PREVIEW` collapses `PreviewOp.QR` into one `PreviewOp.MARK` row carrying a `Symbology` `StrEnum` sub-axis (QR/Micro-QR settled fence code over segno `make`/`make_micro`/`save(kind="svg")`, the `is_qr` discriminant routing `_qr` vs `_barcode`). The linear (1D) `python-barcode` rows (`get_barcode_class`-resolved Code128/Code39/EAN13/EAN8/UPCA/ITF/Codabar/ISBN13/ISSN/PZN/GS1-128 over `SVGWriter`) stay a marked RESEARCH seam in `[3]-[RESEARCH]`. DataMatrix/PDF417 DROPPED (python-barcode is strictly linear). Close-condition: uv-sync resolves `python-barcode` and `ApiPackage.reflect` confirms `get_barcode_class`/`SVGWriter`/`PROVIDED_BARCODES`, settling the `_barcode` body and the linear `Symbology` rows.
- The arm lands on the existing `Preview` owner as one symbology-row axis beside the raster ops, contributing the existing `ArtifactReceipt.Preview` case; the encoded-mark concern is one axis, never a parallel code class. The `SVGWriter` is the ONLY admitted writer (the `ImageWriter` PNG path needs Pillow and re-introduces the leak segno removed).

[BLOCKED] Author the `python-barcode` `.api` catalogue on capture — coverage move from `ENCODED_MARK_OWNER`.
- BLOCKED on the package resolving in the environment: `python-barcode` is present in the lockfile but absent from the active venv (catalogue header `RESEARCH-capture-pending-on-uv-sync`), so the `.api/python-barcode.md` catalogue is AUTHORED-PENDING-VERIFICATION (linear-only, canonical-source-authored) until `assay api` reflection confirms `get_barcode_class`/`get`/`PROVIDED_BARCODES`/`SVGWriter`/`ImageWriter`/the per-symbology classes on uv-sync. README already lists python-barcode (no README edit). Close-condition: uv-sync resolves the package and reflection confirms the canonical-authored surface.
- CENTRAL-MANIFEST: the repo-root `pyproject.toml` python-barcode comment is stale (`linear/2D barcode symbologies` contradicts the verified linear-only catalogue) — a stale-comment correction is owed to the central manifest, recorded for the orchestrator. No admission row to add (the row already exists).

[BLOCKED] Contribute the artifacts measured signals into the one runtime metric stream — trickle-down from branch `ONE_MEASURED_SIGNAL_STREAM`.
- Route every artifacts production duration/byte-volume signal (document emit, chart render, scene render, table build, bundle compression ratio) into the runtime `observability/metrics` `MeterProvider` rather than a local log field, so render-duration histograms and output-byte gauges become first-class observable metrics on the one branch stream.
- Integrate the runtime `observability/metrics` instrument set (consumed, never re-minted) through the `receipt/receipt#RECEIPT` `contribute` fold; no new artifacts package.
- The signal contribution lands on the `ArtifactReceipt.contribute` path — the receipt is the natural per-production fact carrier — feeding the runtime instruments at composition; this is the artifacts-side consumer of the branch metric stream, never a parallel artifacts metric owner.
- Blocked-aligned on the runtime `observability/metrics` task landing the instrument set; the artifacts contribution is the receipt-fold edge that consumes it. Close-condition: the runtime `MeterProvider` instrument set lands, then `receipt/receipt#SIGNALS` `contribute` records against it (the consumer edge is already authored).

[BLOCKED] Key the artifacts document output into the content-addressed reuse fabric — trickle-down from branch `CONTENT_ADDRESSED_REUSE_FABRIC`.
- Confirm every artifacts producer admits its `(ContentKey, Work)` pair into the runtime lane-admission elision so an identical document/chart/table/bundle at an identical spec short-circuits to the cached content key, and the `ArtifactReceipt` carries the hit/miss distinction — the artifacts owners already key every output by `ContentIdentity.of`, so the elision is inherited, not re-minted.
- Integrate the runtime `execution/lanes` `(ContentKey, Work[T])` admission and `frozendict` session cache (consumed); no new artifacts package.
- The change is verification-only on the artifacts side: every `_emit` already returns `ContentIdentity.of(...)`, so the producers thread the same key into the lane admission the runtime owns; the most expensive artifacts output (a signed archival PDF, a rendered notebook) is the natural elision target.
- Aligned on the runtime lane-admission task; the artifacts producers are pre-keyed consumers, never a second cache owner. Close-condition: the runtime `execution/lanes` `(ContentKey, Work)` admission lands, then the producers thread the existing `ContentIdentity.of` key into it (the consumer edge is authored at `receipt/receipt#SIGNALS`).

[BLOCKED] Hold the outward figure handoff to the one HandoffAxis and clear the drift guard — trickle-down from branch `ONE_GRADUATION_RAIL_OUTWARD` + master `CROSS_PACKAGE_DRIFT_GUARD`.
- Confirm every artifacts figure/table/chart/scene that crosses outward to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a parallel per-artifact handoff; and confirm the artifacts sources re-mint no canonical concept (content-identity seed, receipt rail, retry owner, wire-projection name) so the runtime `evidence/evidence` `Structural.drift` cross-language query finds zero re-mints in any artifacts module.
- Integrate the `compute/graduation` `HandoffAxis` model-asset case and the runtime `evidence` `Structural.drift` query (both consumed, never re-minted); no new artifacts package.
- The change is alignment-and-verification on the artifacts side: every `_emit` already returns `ContentIdentity.of(...)` and every producer contributes one `ArtifactReceipt` case consuming the runtime ports, so the outward edge is the single `HandoffAxis` model-asset case and the source surface is drift-clean by construction.
- Aligned on the `compute/graduation` HandoffAxis task and the runtime `evidence` drift-detector task; the artifacts contribution is the pre-keyed outward consumer and the drift-clean source, never a second owner. Close-condition: the upstream graduation `HandoffAxis` task and the runtime `Structural.drift` detector land, then the artifacts sources confirm against the live drift query (the `ARCHITECTURE.md` charter lines 25-26 already assert both invariants).

## [2]-[CLOSED]

(none)
