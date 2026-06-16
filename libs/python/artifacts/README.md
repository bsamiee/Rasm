# [PY_ARTIFACTS]

`artifacts` owns content digests, artifact bundles, document plans, visual specs, export plans, previews, and compression receipts. This package currently contains planning and API placeholders only; future source lands directly in this folder after the planning and API pages are filled.

## [OWNER]

[PLANNING]:
- Path: `.planning/README.md`
- Owns: artifact bundles, PDF/document/Office codecs, visual specs, previews, and compression planning.

[API]:
- Path: `.api/README.md`
- Owns: flat `api-*.md` placeholders for artifact, document, visual, image, and compression dependencies.

[BOUNDARY]:
- Artifacts emits portable files, manifests, and receipts.
- `Rasm.AppUi` and TypeScript app roots own live UI, dashboards, interaction state, and evidence timelines.
