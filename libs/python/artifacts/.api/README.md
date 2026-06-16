# [PY_ARTIFACTS_API_CATALOGUE]

`artifacts` API catalogue pages carry the external-package surface for each rail. A page is reflection-complete where its distribution installs and imports on the cp315 host: `jinja2`, `pypdf`, `pymupdf`, `pypdfium2`, `lxml`, `ruamel-yaml`, `tomlkit`, `python-docx`, `openpyxl`, `qrcode`, `python-magic`, `altair`, `plotly`, `vl-convert-python`, `kaleido`, `zstandard`, `lz4`, `brotli`, and `py7zr` carry fully reflected member tables and entrypoint signatures. The residual gaps are exactly eight distributions: `pillow` and its dependents `pikepdf`, `reportlab`, `weasyprint`, `python-pptx`, `matplotlib` (locked but not installable because `pillow` has no cp315 wheel and its source build needs absent libjpeg/zlib headers), plus `pyvista` and `vtk` (intentionally marker-gated `python_version<'3.13'` on the native VTK floor). Each gap page carries an explicit absence record naming its exact cause; its member surfaces stay a TASKLOG gap until the blocker clears. Source names a member only after its page captures the exact spelling.

## [1]-[PACKAGE_PAGES]

[IMAGE]:
- rail: image
- pages:
  - [api-pillow.md](api-pillow.md)
  - [api-qrcode.md](api-qrcode.md)

[PDF]:
- rail: pdf
- pages:
  - [api-pymupdf.md](api-pymupdf.md)
  - [api-pypdf.md](api-pypdf.md)
  - [api-pikepdf.md](api-pikepdf.md)
  - [api-pypdfium2.md](api-pypdfium2.md)
  - [api-reportlab.md](api-reportlab.md)
  - [api-weasyprint.md](api-weasyprint.md)

[OFFICE]:
- rail: office
- pages:
  - [api-python-docx.md](api-python-docx.md)
  - [api-python-pptx.md](api-python-pptx.md)
  - [api-openpyxl.md](api-openpyxl.md)

[STRUCTURED_DOCUMENTS]:
- rail: structured documents
- pages:
  - [api-lxml.md](api-lxml.md)
  - [api-ruamel-yaml.md](api-ruamel-yaml.md)
  - [api-tomlkit.md](api-tomlkit.md)

[REPORT_TEMPLATING]:
- rail: report-templating
- pages:
  - [api-jinja2.md](api-jinja2.md)

[VISUALS]:
- rail: visuals
- pages:
  - [api-altair.md](api-altair.md)
  - [api-vl-convert-python.md](api-vl-convert-python.md)
  - [api-plotly.md](api-plotly.md)
  - [api-kaleido.md](api-kaleido.md)
  - [api-matplotlib.md](api-matplotlib.md)

[COMPRESSION]:
- rail: compression
- pages:
  - [api-zstandard.md](api-zstandard.md)
  - [api-lz4.md](api-lz4.md)
  - [api-brotli.md](api-brotli.md)
  - [api-py7zr.md](api-py7zr.md)

[FILE_CONTROL]:
- rail: file control
- pages:
  - [api-python-magic.md](api-python-magic.md)

## [2]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- API pages carry external package API facts and package-rail admission records.
- Planning pages carry owner boundaries and source-transcription law.
- README pages route catalogues without duplicating member tables.
