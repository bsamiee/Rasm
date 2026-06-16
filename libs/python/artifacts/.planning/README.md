# [PY_ARTIFACTS_PLANNING]

`artifacts` owns artifact production: one polymorphic document/PDF/Office/structured-text plan, one VisualSpec to ExportPlan axis spanning 2D charts and 3D scientific visualization, one report-templating composition owner, one preview owner, and one compression owner. It has zero consumers today; implementation is full-capability. Content identity and the bundle spine are consumed from runtime `ContentIdentity`, never re-minted. It produces files and receipts for downstream owners without owning UI.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                            | [OWNS]                                                                            | [STATE]   |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------- | :-------- |
|   [1]   | [documents](documents.md)         | the document/PDF/Office/structured-text plan, report templating, artifact receipt | finalized |
|   [2]   | [visual-export](visual-export.md) | the VisualSpec/ExportPlan axis (2D + 3D), preview, compression                    | finalized |

## [2]-[CATALOGUE_PENDING]

- `pillow` has no cp315 binary wheel and a blocked source build; it transitively blocks `pikepdf`, `reportlab`, `weasyprint`, `python-pptx`, `matplotlib`. These six finalize once `pillow` installs (a cp315 wheel publishes or the host provisions libjpeg/zlib + an archiver) — suite TASKLOG `PY_API_004`.
- `pyvista`/`vtk` ride the `python_version<'3.13'` native floor (no cp315 wheel); their fence members verify on the gated sub-3.13 interpreter.
- 18 distributions (pypdf, pymupdf, pypdfium2, lxml, ruamel-yaml, tomlkit, python-docx, openpyxl, qrcode, python-magic, altair, plotly, vl-convert-python, kaleido, zstandard, lz4, brotli, py7zr) plus jinja2 are cp315-reflected.

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis. `DocumentPlan` is ONE dispatch axis with backend-per-mode policy rows collapsing 6 PDF + 3 Office + 3 structured-text backends; `VisualSpec` collapses 2D chart engines and 3D scientific viz into one visual-to-export axis with export backends as rows; `ArtifactReceipt` is one kind-discriminated receipt family replacing scattered per-type receipts. `[STATE]` carries `SPIKE` on the pillow-toolchain or native-floor probe.

| [INDEX] | [AXIS/CONCERN]    | [OWNER]           | [KIND]                      | [CASES]                                         |  [STATE]  |
| :-----: | :---------------- | :---------------- | :-------------------------- | :---------------------------------------------- | :-------: |
|   [1]   | Document plan     | `DocumentPlan`    | tagged union + backend rows | pdf×6 / office×3 / structured-text×3            |   SPIKE   |
|   [2]   | Report templating | `ReportPlan`      | frozen owner                | sections/data-figure bind/TOC over jinja2       | FINALIZED |
|   [3]   | Visual spec       | `VisualSpec`      | tagged union                | 2D chart engines + 3D pyvista scene             |   SPIKE   |
|   [4]   | Export plan       | `ExportPlan`      | frozen owner + backend rows | vl-convert/kaleido/pillow                       |   SPIKE   |
|   [5]   | Preview           | `Preview`         | static surface              | image/preview/media-detection                   |   SPIKE   |
|   [6]   | Compression       | `Compression`     | StrEnum + algo rows         | zstd/lz4/brotli/7z                              | FINALIZED |
|   [7]   | Artifact receipt  | `ArtifactReceipt` | tagged union                | document/pdf/office/report/preview/export modes | FINALIZED |

## [4]-[BUILD_ORDER]

| [INDEX] | [FILE]             | [TRANSCRIBES]                                         | [GATE]         |
| :-----: | :----------------- | :---------------------------------------------------- | :------------- |
|   [1]   | `documents.py`     | documents#DOCUMENT, #REPORT, #RECEIPT                 | static + floor |
|   [2]   | `visual_export.py` | visual-export#VISUAL, #EXPORT, #PREVIEW, #COMPRESSION | static + floor |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]            | [EVIDENCE]                                              |
| :----: | :---------------- | :------------------------------------------------------ |
|  [G1]  | `uv lock --check` | artifacts pins resolve against the root manifest        |
|  [G2]  | `.api` catalogue  | every fence member resolves to an `.api` row            |
|  [G3]  | toolchain floor   | pillow + the native VTK floor install before re-reflect |

## [6]-[PROHIBITIONS]

- [NEVER] re-mint content identity; the artifact bundle key is one runtime `ContentIdentity` key — the former `ContentDigest`/`ArtifactBundle` owners are collapsed away.
- [NEVER] own live UI controls, dashboard runtime, browser state, product artifact stores, or AppUi evidence timelines.
- [NEVER] own IFC tessellation/GLB (geometry) or geospatial/mesh-file/columnar interchange (data).
- [NEVER] author parallel PDF/Office/visual rails; `DocumentPlan` and `VisualSpec` are single dispatch axes with backend rows.
- [NEVER] scatter per-type receipts; `ArtifactReceipt` is one kind-discriminated family.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                                                      | [PAGE]        | [CATALOGUE]                | [STATUS]          |
| :-----: | :------------------------------------------------------------- | :------------ | :------------------------- | :---------------- |
|   [1]   | pymupdf, pypdf, pikepdf, pypdfium2, reportlab, weasyprint      | documents     | api-pymupdf.md ...         | catalogue-pending |
|   [2]   | python-docx, python-pptx, openpyxl, lxml, ruamel-yaml, tomlkit | documents     | api-python-docx.md ...     | catalogue-pending |
|   [3]   | jinja2                                                         | documents     | api-jinja2.md              | admitted          |
|   [4]   | altair, plotly, matplotlib, vl-convert-python, kaleido         | visual-export | api-altair.md ...          | catalogue-pending |
|   [5]   | pyvista, vtk                                                   | visual-export | api-pyvista.md, api-vtk.md | catalogue-pending |
|   [6]   | pillow, qrcode, python-magic                                   | visual-export | api-pillow.md ...          | catalogue-pending |
|   [7]   | zstandard, lz4, brotli, py7zr                                  | visual-export | api-zstandard.md ...       | admitted          |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. The pillow-blocked six finalize once the image toolchain installs; the `pyvista`/`vtk` 3D scene path finalizes on the native floor. The report-templating composition (jinja2 binding VisualSpec outputs into a DocumentPlan tree) and the strided GLB-class deterministic byte layout for content-addressing deepen against the real backends. The bar: any artifact a flagship app emits — a templated multi-page PDF report, a 3D scientific scene render, a content-addressed compressed bundle — is buildable from these pages alone, keyed by one runtime owner.
