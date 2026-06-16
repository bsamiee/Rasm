# [PY_ARTIFACTS_VISUAL_PREVIEW_COMPRESSION]

Visual, preview, and compression ownership emits specs and files. It does not own dashboards, live chart interaction, AppUi layout, or browser state.

## [1]-[VISUAL_OWNER]

[VISUAL_SPEC]:
- Owns: chart grammar, data reference, encoding policy, theme hint, export target, and source artifact references.
- API routes: `.api/api-altair.md`, `.api/api-plotly.md`, `.api/api-matplotlib.md`.
- Output: visual spec record and export plan.
- Boundary: no live controls, viewport state, or UI event handling.

[STATIC_EXPORT]:
- Owns: SVG, PNG, PDF, HTML, and image-export routes.
- API routes: `.api/api-vl-convert-python.md`, `.api/api-kaleido.md`, `.api/api-matplotlib.md`, `.api/api-pillow.md`.
- Output: exported files with digest and preview receipt.
- Boundary: browser/Chrome dependencies are runtime capabilities reported as receipts, not hidden assumptions.

## [2]-[PREVIEW_OWNER]

[PREVIEW_RECEIPT]:
- Owns: thumbnail, page preview, chart preview, image dimensions, media type, and source digest.
- API routes: `.api/api-pillow.md`, `.api/api-pymupdf.md`, `.api/api-pypdfium2.md`, `.api/api-python-magic.md`, `.api/api-qrcode.md`.
- Output: preview file and metadata receipt.
- Boundary: AppUi owns retained render surfaces and accessibility adaptation.

[QR_CODE]:
- Owns: QR-code file generation as an artifact, not a UI control.
- API route: `.api/api-qrcode.md`.
- Output: image artifact with content digest.
- Boundary: no in-app interaction state.

## [3]-[COMPRESSION_OWNER]

[ARCHIVE_RECEIPT]:
- Owns: archive format, compression algorithm, member map, password/encryption policy, and deterministic extraction facts.
- API routes: `.api/api-zstandard.md`, `.api/api-lz4.md`, `.api/api-brotli.md`, `.api/api-py7zr.md`.
- Output: archive artifact and compression receipt.
- Boundary: package staging and release packaging remain caller-owned.

## [4]-[SPLIT_PRESSURE]

[VISUALS_PACKAGE]:
- Split trigger: visual grammar, scene rendering, export receipts, and visual QA outgrow artifact ownership and gain named consumers.
- Stay merged when: visuals remain static specs and exported files inside artifact bundles.

## [5]-[RED_TEAM]

- Reject chart code that becomes a dashboard runtime.
- Reject image preview without source digest and dimensions.
- Reject export code that hides browser or renderer capability requirements.
- Reject compression output without deterministic member ordering.
