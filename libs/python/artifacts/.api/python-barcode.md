# [PY_ARTIFACTS_API_PYTHON_BARCODE]

`python-barcode` (dist `python-barcode`, import `barcode`) supplies the pure-Python linear (1D) barcode surface for the artifacts imaging rail beside the segno QR arm: a name-keyed symbology registry (`get`/`generate`/`get_barcode_class`) that builds a `Barcode` from a code string and renders it through a dependency-free `SVGWriter` or a Pillow-backed `ImageWriter`. The package owner composes `generate`, the symbology registry, and the writer pair into the linear-barcode path; it never re-implements the 1D encoding tables python-barcode already owns, and it routes QR and 2D-matrix symbologies elsewhere because this package is strictly linear.

## [1]-[PACKAGE_SURFACE]

- package: `python-barcode`
- import: `barcode` (dist name `python-barcode`, import name `barcode`)
- owner: `artifacts`
- rail: imaging
- asset: pure-Python runtime library (`py3-none-any` wheel; SVG path needs no dependency, the PNG path needs Pillow)
- installed: present in the lockfile but not yet synced into the active venv — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from the canonical source (`github.com/WhyNotHugo/python-barcode`) and official documentation, and reflection-verifies on uv sync (the package is pure-Python and imports on the cp315 core)
- entry points: console script `python-barcode` (CLI: `create`/`list`); library use is import-only
- capability: linear (1D) barcode generation for the Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution, dependency-free SVG serialization, and Pillow-backed raster (PNG) serialization with configurable module geometry, quiet zone, and human-readable text

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode base and writer roots
- rail: imaging

`Barcode.default_writer` is `SVGWriter`; the `ImageWriter` is defined only when Pillow is importable. Concrete symbology classes (`Code128`, `Code39`, `EAN13`, `EAN8`, `EAN14`, `JAN`, `UPCA`, `ITF`, `Codabar`, `ISBN10`, `ISBN13`, `ISSN`, `PZN`, `Gs1_128`) subclass `Barcode` and are resolved by name through the registry rather than imported individually.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------------------------- |
|   [1]   | `barcode.base.Barcode`    | symbology base  | abstract 1D barcode; class attribute `name`, `default_writer = SVGWriter` |
|   [2]   | `barcode.writer.BaseWriter`| writer base     | abstract renderer; owns the module-geometry/quiet-zone/text option set    |
|   [3]   | `barcode.writer.SVGWriter`| SVG writer      | default, dependency-free SVG (`xml.dom.minidom`)                          |
|   [4]   | `barcode.writer.ImageWriter`| raster writer | Pillow-backed PNG/raster; present only when Pillow imports                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbology registry and factory
- rail: imaging

`PROVIDED_BARCODES` is the sorted list of accepted name keys (`code128`, `code39`, `ean13`/`ean`, `ean8`, `ean14`/`gtin`, `jan`, `upc`/`upca`, `itf`, `codabar`/`nw-7`, `isbn13`/`isbn`/`gs1`, `isbn10`, `issn`, `pzn`, `gs1_128`, plus the `ean13-guard`/`ean8-guard` variants). `get`/`generate` resolve a name to a class and build the symbology in one call.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                | [CAPABILITY]                              |
| :-----: | :-------------------- | :------------------------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `get`                 | `get(name, code=None, writer=None, options=None)` -> `Barcode` or class    | resolve and build (or resolve the class)  |
|   [2]   | `generate`            | `generate(name, code, writer=None, output=None, writer_options=None, text=None)` -> `str` or `None` | build and write in one call |
|   [3]   | `get_barcode_class`   | `get_barcode_class(name)` -> `type[Barcode]`                               | resolve name to symbology class (alias of `get_class`) |
|   [4]   | `PROVIDED_BARCODES`   | sorted `list[str]`                                                          | accepted symbology name keys              |

[ENTRYPOINT_SCOPE]: `Barcode` render and serialize
- rail: imaging

`writer_options` flows the renderer geometry; human-readable text is suppressed by passing `text=""`, not a boolean flag. `save` appends the writer's extension; `write` streams to an open binary file; `render` returns the in-memory rendering.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                  | [CAPABILITY]                              |
| :-----: | :------------------- | :----------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `Barcode.save`       | `save(filename, options=None, text=None)` -> `str`           | serialize to a file (extension by writer) |
|   [2]   | `Barcode.write`      | `write(fp, options=None, text=None)`                         | serialize to an open binary stream        |
|   [3]   | `Barcode.render`     | `render(writer_options=None, text=None)`                     | in-memory rendering (SVG string / image)  |
|   [4]   | `Barcode.name`       | class attribute                                              | symbology name                            |

## [4]-[IMPLEMENTATION_LAW]

[IMAGING_BARCODE]:
- import: `import barcode` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `python-barcode`; the import name is `barcode`.
- registry axis: one name-keyed registry owns symbology selection — `get`/`generate`/`get_barcode_class` resolve a `PROVIDED_BARCODES` key to the concrete class; never import and branch over the individual symbology classes, and never hardcode a name not in `PROVIDED_BARCODES`.
- writer axis: `SVGWriter` (default, dependency-free) and `ImageWriter` (Pillow PNG) are the two render rows on the same `Barcode`; the output format is the `writer` row, never a parallel barcode type per format — `generate(..., writer=ImageWriter())` selects raster, the default emits SVG.
- option axis: `writer_options` carries `module_width`/`module_height`/`quiet_zone`/`font_size`/`text_distance`/`background`/`foreground`/`center_text` (with `dpi`/`format`/`mode` on `ImageWriter` only) as one option dict; human-readable text is suppressed by `text=""`, not a `write_text` flag.
- dimensionality axis: this package is strictly 1D/linear — Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 only. It owns no 2D matrix or stacked symbology; QR/Micro-QR route to `segno`, and any DataMatrix/PDF417 requirement is a separate owner, never a python-barcode call.
- evidence: each symbol captures symbology name, code value, writer kind, module geometry, and output byte length as an imaging receipt.
- boundary: python-barcode owns linear-barcode generation and SVG/PNG serialization; segno owns the QR/Micro-QR arm beside it; the SVG output feeds the document and figure owners directly; the PNG path routes through Pillow; live UI stays outside this package.

[RAIL_LAW]:
- Package: `python-barcode`
- Owns: linear (1D) barcode generation for the Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution, and dependency-free SVG plus Pillow-backed PNG serialization
- Accept: `get`/`generate` over a `PROVIDED_BARCODES` name key, `SVGWriter` default and `ImageWriter` raster as writer rows, `writer_options` geometry control, SVG output feeding the document and figure owners
- Reject: wrapper-renames of `get`/`generate`; importing and branching over individual symbology classes where the registry resolves by name; a parallel barcode type per output format; a claimed 2D/matrix symbology (DataMatrix/PDF417/QR) this package does not provide — QR routes to `segno` and 2D-matrix needs are a separate owner
