# [PY_ARTIFACTS_DOCUMENTS_PDF_OFFICE]

Document ownership uses package codecs for structural inspection, extraction, rendering, repair, and export. It never hand-rolls PDF, OpenXML, XLSX, XML, YAML, or TOML parsers.

## [1]-[PDF_OWNER]

[PDF_PLAN]:
- Owns: PDF intent, page range, extraction mode, render mode, repair mode, encryption policy, and output bundle route.
- API routes: `.api/api-pymupdf.md`, `.api/api-pypdf.md`, `.api/api-pikepdf.md`, `.api/api-pypdfium2.md`, `.api/api-reportlab.md`, `.api/api-weasyprint.md`.
- Output: PDF receipt with extracted text, page images, repaired file, generated file, or failure facts.
- Boundary: no UI PDF viewer, no product document store, no raw object graph leakage.

[PDF_ROLE_SPLIT]:
- PyMuPDF: extraction, page rendering, pixmaps, annotations, and layout-oriented reads.
- pypdf: pure structural split/merge/metadata/page operations.
- pikepdf: qpdf-backed repair, object-level manipulation, encryption, and linearization.
- pypdfium2: PDFium-backed rendering and form-oriented probes.
- reportlab and WeasyPrint: generation from programmatic or HTML/CSS input.

## [2]-[OFFICE_OWNER]

[OFFICE_PLAN]:
- Owns: DOCX, PPTX, XLSX, and tabular file inspection or generation intent.
- API routes: `.api/api-python-docx.md`, `.api/api-python-pptx.md`, `.api/api-openpyxl.md`.
- Output: Office receipt with document structure, generated file, extracted media, or tabular summary.
- Boundary: no live editor, no UI document state, no hidden OpenXML mutation outside the owning plan.

[STRUCTURED_TEXT]:
- Owns: XML, YAML, and TOML round-trip preservation where comments, anchors, order, or formatting matter.
- API routes: `.api/api-lxml.md`, `.api/api-ruamel-yaml.md`, `.api/api-tomlkit.md`.
- Output: structured document receipt and bundle artifact.
- Boundary: no ad hoc string parsing for structured formats.

## [3]-[RED_TEAM]

- Reject PDF/Office code that exposes provider handles across package boundaries.
- Reject string-based XML/YAML/TOML mutation.
- Reject document generation without deterministic output and digest.
- Reject browser/UI ownership inside document export.
