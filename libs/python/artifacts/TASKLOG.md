# [PY_ARTIFACTS_TASKLOG]

The folder's open and closed work, distilled from `IDEAS.md`. Open tasks are cards in `[1]-[OPEN]` with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` leader; closed tasks move to `[2]-[CLOSED]` with `[COMPLETE]`/`[DROPPED]`. Each task names the exact sub-domain or file it lands in.

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

[ENCODED_MARK_PREVIEW]-[BLOCKED]: widen the imaging code arm to the full encoded-mark family on `figures/preview.md`.
- Capability: `figures/preview#PREVIEW` covers QR/Micro-QR and linear 1D encoded marks through one imaging operation family.
- Shape: `PreviewOp.MARK` replaces `PreviewOp.QR` and carries a `Symbology` `StrEnum` over settled `segno` QR rows plus pending `python-barcode` linear rows, all contributing the existing `ArtifactReceipt.Preview` case.
- Unlocks: machine-readable SVG preview output expands without a parallel code class, a Pillow-backed core leak, or phantom DataMatrix/PDF417 membership.
- Anchors: `Preview`, `PreviewOp.MARK`, `Symbology`, `ArtifactReceipt.Preview`, `segno.make`/`make_micro`/`QRCode.save(kind="svg")`, `python-barcode` `get_barcode_class`, `SVGWriter`, and `PROVIDED_BARCODES`.
- Tension: uv-sync plus `ApiPackage.reflect` must confirm the `python-barcode` registry and writer surface; DataMatrix/PDF417 stay routed to a separate future 2D-matrix owner.

[PYTHON_BARCODE_CATALOGUE]-[BLOCKED]: author the `python-barcode` `.api` catalogue on capture.
- Capability: the folder `.api/python-barcode.md` catalogue becomes the verified evidence source for the linear 1D barcode arm.
- Shape: the catalogue keeps the canonical-source-authored surface as `AUTHORED-PENDING-VERIFICATION` until uv-sync resolves `python-barcode` and reflection confirms `get_barcode_class`, `get`, `PROVIDED_BARCODES`, `SVGWriter`, `ImageWriter`, and the per-symbology classes.
- Unlocks: `figures/preview#PREVIEW` can settle the `_barcode` body and linear `Symbology` rows without adding a README admission row or guessing package members.
- Anchors: `.api/python-barcode.md`, catalogue header `RESEARCH-capture-pending-on-uv-sync`, repo-root `pyproject.toml` admission, `ApiPackage.reflect`, and the existing README domain-package row.
- Tension: the active venv still lacks the package, and the repo-root `pyproject.toml` comment still claims `linear/2D barcode symbologies`; the stale central-manifest comment is recorded for the orchestrator and stays outside this file.

[CONTRIBUTE_ARTIFACTS_MEASURED_SIGNALS_RUNTIME]-[BLOCKED]: contribute the artifacts measured signals into the one runtime metric stream — trickle-down from branch `ONE_MEASURED_SIGNAL_STREAM`.
- Capability: artifact production duration, byte-volume, and compression-ratio signals join the one runtime metric stream instead of local logging fields.
- Shape: `receipt/receipt#RECEIPT` `ArtifactReceipt.contribute` feeds the runtime `observability/metrics` `MeterProvider` instrument set at composition, consuming the branch stream without minting an artifacts metric owner.
- Unlocks: document emit, chart render, scene render, table build, and bundle compression become observable through render-duration histograms and output-byte gauges on the shared branch stream.
- Anchors: branch `ONE_MEASURED_SIGNAL_STREAM`, runtime `observability/metrics`, `MeterProvider`, `ArtifactReceipt.contribute`, `receipt/receipt#SIGNALS`, and production receipt facts.
- Tension: the runtime instrument set must land before artifacts can record against it; the artifacts consumer edge is already authored but blocked-aligned on that upstream task.

[KEY_ARTIFACTS_DOCUMENT_OUTPUT_CONTENT]-[BLOCKED]: key the artifacts document output into the content-addressed reuse fabric — trickle-down from branch `CONTENT_ADDRESSED_REUSE_FABRIC`.
- Capability: artifacts producers consume the runtime content-addressed reuse fabric so identical outputs short-circuit on a cached content key.
- Shape: each producer threads its existing `ContentIdentity.of(...)` key as a `(ContentKey, Work[T])` pair into runtime `execution/lanes` admission, while `ArtifactReceipt` carries the hit/miss distinction.
- Unlocks: expensive signed PDFs, rendered notebooks, charts, tables, bundles, and scenes become elision targets without a second artifacts cache or durable store.
- Anchors: branch `CONTENT_ADDRESSED_REUSE_FABRIC`, runtime `execution/lanes`, `LanePolicy.run`, `(ContentKey, Work[T])`, `ContentIdentity.of(...)`, `ArtifactReceipt`, and `receipt/receipt#SIGNALS`.
- Tension: runtime lane admission must land first; the artifacts side is verification-and-consume only because producers are already content-keyed.

[HOLD_OUTWARD_FIGURE_HANDOFF_HANDOFFAXIS]-[BLOCKED]: hold the outward figure handoff to the one HandoffAxis and clear the drift guard — trickle-down from branch `ONE_GRADUATION_RAIL_OUTWARD` + master `CROSS_PACKAGE_DRIFT_GUARD`.
- Capability: outward artifacts figure, table, chart, and scene handoff stays on the one graduation rail and verifies drift-clean source ownership.
- Shape: sibling-bound outputs cross only as the `compute/graduation` `HandoffAxis` `model-asset` case keyed by `ContentIdentity`, and runtime `evidence` `Structural.drift` confirms artifacts re-mint no content-identity seed, receipt rail, retry owner, or wire-projection name.
- Unlocks: sibling packages consume artifact outputs through one handoff axis while branch drift checks prove there is no parallel per-artifact handoff or private canonical concept.
- Anchors: branch `ONE_GRADUATION_RAIL_OUTWARD`, master `CROSS_PACKAGE_DRIFT_GUARD`, `compute/graduation#GRADUATION`, `HandoffAxis`, `ContentIdentity`, `ArtifactReceipt`, runtime `evidence/evidence`, `Structural.drift`, and `ARCHITECTURE.md` seams.
- Tension: upstream graduation `HandoffAxis` and runtime drift-detector tasks must land before artifacts can confirm against the live query; artifacts already assert the invariant as a pre-keyed consumer.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
