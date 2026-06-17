# [PY_ARTIFACTS]

`artifacts` owns artifact production: one polymorphic document/PDF/Office/structured-text plan, one VisualSpec-to-ExportPlan axis spanning 2D charts and 3D scientific visualization, one report-templating composition owner, one preview owner, and one compression owner. It has zero consumers today and implementation is full-capability. Content identity and the bundle spine are consumed from the runtime `ContentIdentity` owner, never re-minted; the package produces files and receipts for downstream owners without owning UI. Owner state and the axis registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`. The design pages in `.planning/` are decision-complete blueprints an implementation agent transcribes; the package catalogues in `.api/` carry the external-surface evidence each page consumes.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                      | [OWNS]                                                                            |
| :-----: | :------------------------------------------ | :------------------------------------------------------------------------------- |
|   [1]   | [documents](.planning/documents.md)         | the document/PDF/Office/structured-text plan, report templating, artifact receipt |
|   [2]   | [visual-export](.planning/visual-export.md) | the VisualSpec/ExportPlan axis (2D + 3D), preview, compression                    |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in the root manifest; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`. The pillow-blocked PDF/Office/visual distributions and the native-floor `pyvista`/`vtk` carry `catalogue-pending` until the image toolchain and the sub-3.13 native floor install and re-reflect.

| [INDEX] | [PACKAGE]                                                      | [PAGE]        | [CATALOGUE]                                                                                  | [STATUS]          |
| :-----: | :------------------------------------------------------------ | :------------ | :------------------------------------------------------------------------------------------ | :---------------- |
|   [1]   | pymupdf, pypdf, pikepdf, pypdfium2, reportlab, weasyprint     | documents     | api-pymupdf.md, api-pypdf.md, api-pikepdf.md, api-pypdfium2.md, api-reportlab.md, api-weasyprint.md | catalogue-pending |
|   [2]   | python-docx, python-pptx, openpyxl, lxml, ruamel-yaml, tomlkit | documents     | api-python-docx.md, api-python-pptx.md, api-openpyxl.md, api-lxml.md, api-ruamel-yaml.md, api-tomlkit.md | catalogue-pending |
|   [3]   | jinja2                                                        | documents     | api-jinja2.md                                                                                | admitted          |
|   [4]   | altair, plotly, matplotlib, vl-convert-python, kaleido       | visual-export | api-altair.md, api-plotly.md, api-matplotlib.md, api-vl-convert-python.md, api-kaleido.md    | catalogue-pending |
|   [5]   | pyvista, vtk                                                 | visual-export | api-pyvista.md, api-vtk.md                                                                   | catalogue-pending |
|   [6]   | pillow, qrcode, python-magic                                | visual-export | api-pillow.md, api-qrcode.md, api-python-magic.md                                            | catalogue-pending |
|   [7]   | zstandard, lz4, brotli, py7zr                               | visual-export | api-zstandard.md, api-lz4.md, api-brotli.md, api-py7zr.md                                    | admitted          |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]      | [EVIDENCE]                                              |
| :-----: | :-------------------- | :---------- | :----------------------------------------------------- |
|  [G1]   | locked restore        | uv          | artifacts pins resolve against the root manifest        |
|  [G2]   | API catalogue resolve | assay api   | every fence member resolves to an `.api` row            |
|  [G3]   | type check            | ty          | typed-signature transcription resolves clean            |
|  [G4]   | lint and format       | ruff        | routed closure, zero diagnostics                        |
|  [G5]   | spec law-matrix       | pytest      | artifacts law-matrix specs pass                         |
|  [G6]   | toolchain floor       | uv          | pillow and the native VTK floor install before re-reflect |
|  [G7]   | page diagram render   | mermaid-cli | page diagrams render through the local renderer          |
