# [PY_ARTIFACTS_ARCHITECTURE]

`artifacts` is one artifact-production surface: every concern is an axis owner with closed cases and backend rows, every artifact keys by the runtime `ContentIdentity` owner, and every receipt is one kind-discriminated family. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the implementation source tree, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, the package boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The planned module layout IS the build order: each file is one transcription unit, dispatch axes before their backend rows, the report-templating composition after the document and visual axes it binds. Each leaf is annotated with the owners it transcribes and the owning page#cluster.

```text codemap
artifacts/
├── documents.py         # DocumentPlan, ReportPlan, ArtifactReceipt — documents#DOCUMENT, #REPORT, #RECEIPT
└── visual_export.py     # VisualSpec, ExportPlan, Preview, Compression — visual-export#VISUAL, #EXPORT, #PREVIEW, #COMPRESSION
```

`documents.py` lands first: `DocumentPlan` is one dispatch axis with backend-per-mode policy rows collapsing six PDF backends, three Office backends, and three structured-text backends; `ReportPlan` is the report-templating composition over jinja2 binding visual outputs into a document tree; `ArtifactReceipt` is the one kind-discriminated receipt family keyed by runtime `ContentIdentity` and wired through `ReceiptContributor`. `visual_export.py` follows: `VisualSpec` is one visual-to-export axis collapsing 2D chart engines and the 3D scientific scene, `ExportPlan` carries the export-backend rows, `Preview` owns image/preview/media-detection, and `Compression` is one algorithm-row owner over zstd/lz4/brotli/7z.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual toolchain/floor probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]       | [OWNER]           | [KIND]                      | [CASES]                                         | [PAGE#CLUSTER]              |  [STATE]  |
| :-----: | :---------------- | :---------------- | :-------------------------- | :---------------------------------------------- | :------------------------- | :-------: |
|   [1]   | document plan     | `DocumentPlan`    | tagged union + backend rows | pdf×6 / office×3 / structured-text×3            | documents#DOCUMENT         |   SPIKE   |
|   [2]   | report templating | `ReportPlan`      | frozen owner                | sections/data-figure bind/TOC over jinja2       | documents#REPORT           | FINALIZED |
|   [3]   | artifact receipt  | `ArtifactReceipt` | tagged union                | document/pdf/office/report/preview/export modes | documents#RECEIPT          | FINALIZED |
|   [4]   | visual spec       | `VisualSpec`      | tagged union                | 2D chart engines + 3D pyvista scene             | visual-export#VISUAL       |   SPIKE   |
|   [5]   | export plan       | `ExportPlan`      | frozen owner + backend rows | vl-convert/kaleido/pillow                       | visual-export#EXPORT       |   SPIKE   |
|   [6]   | preview           | `Preview`         | static surface              | image/preview/media-detection                   | visual-export#PREVIEW      |   SPIKE   |
|   [7]   | compression       | `Compression`     | StrEnum + algo rows         | zstd/lz4/brotli/7z                              | visual-export#COMPRESSION  | FINALIZED |

Every emitted artifact keys by one runtime `ContentIdentity` key; the receipt family stays one kind-discriminated owner, never scattered per-type receipts. `DocumentPlan` and `VisualSpec` are single dispatch axes with backend rows, never parallel rails.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PACKAGE]   | [MAY_REFERENCE_ARTIFACTS] | [ARTIFACTS_MAY_REFERENCE] | [BOUNDARY]                                       |
| :-----: | :---------- | :-----------------------: | :-----------------------: | :----------------------------------------------- |
|   [1]   | `runtime`   |            no             |            yes            | content key and receipt port consumed inward     |
|   [2]   | `data`      |            no             |            no             | columnar/mesh interchange stays at `data`        |
|   [3]   | `compute`   |            no             |            no             | numeric study evidence stays at `compute`        |
|   [4]   | `geometry`  |            no             |            no             | IFC tessellation/GLB stays at `geometry`         |

`artifacts` consumes runtime `ContentIdentity` and `ReceiptContributor` and never re-mints them. It composes data, compute, and geometry outputs as inputs but owns none of their interchange concerns. Cross-language consumer projections of the emitted artifacts ride the Tier-0 `region-map/seam-splits.md`.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named artifacts cluster, consequences land at the consumer. Intra-Python seams ride `pkg/page#CLUSTER`; cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as a Tier-0 seam, never restated here.

| [INDEX] | [SEAM]            | [MECHANICS_AT]            | [CONSEQUENCE_AT]                                                           |
| :-----: | :---------------- | :------------------------ | :------------------------------------------------------------------------ |
|   [1]   | content identity  | runtime/content-identity#IDENTITY | documents#RECEIPT and visual-export#EXPORT key artifacts by one `ContentIdentity` |
|   [2]   | artifact receipt  | documents#RECEIPT         | runtime/observability#RECEIPT wires `ArtifactReceipt` through `ReceiptContributor` |
|   [3]   | report figure bind | documents#REPORT          | visual-export#VISUAL outputs bind into the report-templating document tree |
|   [4]   | compression       | visual-export#COMPRESSION | content-addressed bundle bytes fold under the runtime content key          |

## [5]-[BOUNDARIES]

- `artifacts` is not a UI package, dashboard runtime, browser-state owner, product artifact store, or evidence-timeline owner.
- `artifacts` produces files and receipts for downstream owners and owns no live UI controls.
- IFC tessellation and GLB belong to `geometry`; geospatial, mesh-file, and columnar interchange belong to `data`; numeric study evidence belongs to `compute`.
- `pyvista`/`vtk` are 3D scientific visualization feeding the visual surface, reassigned here from the former data catalogue.
- Statement carve-outs are named per fence: the backend-dispatch acceptors on `DocumentPlan` and `VisualSpec` and the figure-bind fold on `ReportPlan` are the boundary capsules; every other member stays expression-shaped.
- Every emitted file carries one runtime `ContentIdentity` key; content identity is never re-minted.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER re-mint content identity; the artifact bundle key is one runtime `ContentIdentity` key — the former `ContentDigest`/`ArtifactBundle` owners are collapsed away.
- NEVER own live UI controls, dashboard runtime, browser state, product artifact stores, or evidence timelines.
- NEVER own IFC tessellation/GLB (geometry) or geospatial/mesh-file/columnar interchange (data).
- NEVER author parallel PDF/Office/visual rails; `DocumentPlan` and `VisualSpec` are single dispatch axes with backend rows.
- NEVER scatter per-type receipts; `ArtifactReceipt` is one kind-discriminated family.
