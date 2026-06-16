# [PY_ARTIFACTS_PLANNING]

`artifacts` owns document, PDF, Office, image, visualization, compression, content identity, preview, and export bundles. It produces files and receipts for downstream owners without owning UI.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] |
| :-----: | :----- | :----- |
|   [1]   | [artifact bundles](artifact-bundles.md) | content digests, file-set manifests, bundle identity |
|   [2]   | [documents PDF Office](documents-pdf-office.md) | document codecs, PDF extraction/render/repair, Office/tabular files |
|   [3]   | [visual preview compression](visual-preview-compression.md) | chart specs, static exports, previews, image output, archives |

## [2]-[OWNER_CLUSTERS]

[BUNDLE]:
- Owner symbols: `ContentDigest`, `ArtifactBundle`, `ArtifactManifest`.
- API routes: `.api/api-zstandard.md`, `.api/api-lz4.md`, `.api/api-brotli.md`, `.api/api-py7zr.md`, plus runtime resource evidence.
- Boundary: no product artifact store or support-capture root ownership.

[DOCUMENT]:
- Owner symbols: `DocumentPlan`, `DocumentReceipt`, `PdfReceipt`, `OfficeReceipt`.
- API routes: `.api/api-pymupdf.md`, `.api/api-pypdf.md`, `.api/api-pikepdf.md`, `.api/api-pypdfium2.md`, `.api/api-python-docx.md`, `.api/api-python-pptx.md`, `.api/api-openpyxl.md`, `.api/api-lxml.md`, `.api/api-ruamel-yaml.md`, `.api/api-tomlkit.md`, `.api/api-reportlab.md`, `.api/api-weasyprint.md`.
- Boundary: no durable document store and no hand-rolled parsers.

[VISUAL]:
- Owner symbols: `VisualSpec`, `ExportPlan`, `PreviewReceipt`.
- API routes: `.api/api-altair.md`, `.api/api-vl-convert-python.md`, `.api/api-plotly.md`, `.api/api-kaleido.md`, `.api/api-matplotlib.md`, `.api/api-pillow.md`, `.api/api-qrcode.md`, `.api/api-python-magic.md`.
- Boundary: no live dashboard, UI event state, AppUi render surface, or browser runtime.

## [3]-[TRANSCRIPTION_LAW]

- Future source lands directly under `libs/python/artifacts`.
- Artifacts consumes runtime rails and resources only after runtime source exists.
- Artifacts may consume data/compute outputs as immutable bundle inputs, not as package interiors.
- All emitted files carry content identity and receipt fields.
