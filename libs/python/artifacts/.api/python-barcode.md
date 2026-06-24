# [PY_ARTIFACTS_API_PYTHON_BARCODE]

`python-barcode` (dist `python-barcode`, import `barcode`) supplies the pure-Python linear (1D) barcode surface for the artifacts imaging rail beside the segno QR arm: a name-keyed symbology registry (`get`/`generate`/`get_barcode_class`) that builds a `Barcode` from a code string and renders it through a dependency-free `SVGWriter` or a Pillow-backed `ImageWriter`. The package owner composes `generate`, the symbology registry, and the writer pair into the linear-barcode path; it never re-implements the 1D encoding tables python-barcode already owns, and it routes QR and 2D-matrix symbologies elsewhere because this package is strictly linear.

## [01]-[PACKAGE_SURFACE]

- package: `python-barcode`
- import: `barcode` (dist name `python-barcode`, import name `barcode`)
- owner: `artifacts`
- rail: imaging
- license: `MIT`; pure-Python `py3-none-any` wheel, no native link
- asset: the SVG path needs no dependency; the PNG/raster path needs Pillow (`python-barcode[images]`), resolved through the manifest, never a per-package pin
- installed: `0.16.1` reflected via assay api on cp315
- entry points: console script `python-barcode` (CLI: `create`/`list`); library use is import-only
- capability: linear (1D) barcode generation for the Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution, dependency-free SVG serialization, and Pillow-backed raster (PNG) serialization with configurable module geometry, quiet zone, and human-readable text

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode base and writer roots
- rail: imaging

`Barcode.default_writer` is `SVGWriter`; `ImageWriter` is exported only when Pillow is importable. The package re-exports the concrete symbology classes at top level (`Code128`, `Code39`, `EAN13`, `EAN13_GUARD`, `EAN8`, `EAN8_GUARD`, `EAN14`, `JAN`, `UPCA`, `ITF`, `CODABAR`, `ISBN10`, `ISBN13`, `ISSN`, `PZN`, `Gs1_128`); each subclasses `barcode.base.Barcode` and is resolved by name through the registry rather than imported individually. `SVGWriter` builds via `create_svg_object` (`xml.dom.minidom`); `BaseWriter.__init__(initialize, paint_module, paint_text, finish)` takes the four render callbacks the concrete writers bind.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                   |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `barcode.base.Barcode`       | symbology base | abstract 1D barcode; class attributes `name`, `digits`, `default_writer = SVGWriter`, `default_writer_options` |
|  [02]   | `barcode.writer.BaseWriter`  | writer base    | abstract renderer; `__init__(initialize, paint_module, paint_text, finish)` binds the four render callbacks; `set_options`/`calculate_size`/`render`/`save`/`write` drive the render loop |
|  [03]   | `barcode.writer.SVGWriter`   | SVG writer     | default, dependency-free SVG (`xml.dom.minidom`); `SVGWriter()` zero-arg construction                          |
|  [04]   | `barcode.writer.ImageWriter` | raster writer  | Pillow-backed PNG/raster; `ImageWriter(format='PNG', mode='RGB', dpi=300)`; exported only when Pillow imports  |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: imaging

`barcode.errors` is the typed failure rail; `BarcodeError` is the base, and the registry/encode validators raise the specific subtypes. `BarcodeNotFoundError` is raised by `get`/`get_class` for a name not in `PROVIDED_BARCODES`.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]    | [CAPABILITY]                                                  |
| :-----: | :------------------------------------ | :--------------- | :----------------------------------------------------------- |
|  [01]   | `errors.BarcodeError`                 | fault base       | base of every python-barcode failure                         |
|  [02]   | `errors.BarcodeNotFoundError`         | resolution fault | requested symbology name is not a `PROVIDED_BARCODES` key     |
|  [03]   | `errors.IllegalCharacterError`        | encode fault     | the code string contains a character the symbology forbids    |
|  [04]   | `errors.NumberOfDigitsError`          | arity fault      | wrong digit count for a fixed-length symbology (EAN/UPC/…)    |
|  [05]   | `errors.WrongCountryCodeError`        | EAN fault        | invalid country/prefix code for an EAN symbology              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbology registry and factory
- rail: imaging

`PROVIDED_BARCODES` is the sorted `list[str]` of accepted name keys: `codabar`, `code128`, `code39`, `ean`, `ean13`, `ean13-guard`, `ean14`, `ean8`, `ean8-guard`, `gs1`, `gs1_128`, `gtin`, `isbn`, `isbn10`, `isbn13`, `issn`, `itf`, `jan`, `nw-7`, `pzn`, `upc`, `upca`. `get` resolves a name to a class and, when `code` is supplied, builds the symbology in one call; `generate` builds and writes in one call. `get_barcode` is the same object as `get`; `get_barcode_class` is the same object as `get_class`.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                                                    | [CAPABILITY]                                                 |
| :-----: | :------------------ | :---------------------------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `get`               | `get(name, code=None, writer=None, options=None) -> Barcode \| type[Barcode]`                   | resolve the class, or build the symbology when `code` is set |
|  [02]   | `generate`          | `generate(name, code, writer=None, output=None, writer_options=None, text=None) -> str \| None` | build and write in one call (returns filename or `None`)     |
|  [03]   | `get_barcode_class` | `get_barcode_class(name) -> type[Barcode]`                                                      | resolve name to symbology class (same object as `get_class`) |
|  [04]   | `PROVIDED_BARCODES` | sorted `list[str]`                                                                              | accepted symbology name keys                                 |

[ENTRYPOINT_SCOPE]: `Barcode` render and serialize
- rail: imaging

`Barcode.__init__(code, writer=None, **options)` builds a symbology; `render` flows the renderer geometry through `writer_options`. Human-readable text is set by the `text` argument and the `write_text`/`text` writer options. `save` appends the writer's extension and returns the full filename; `write` streams to an open binary file; `render` returns the in-memory rendering. `build` returns the single-element 1s/0s encoding list, `get_fullcode` returns the human-readable code, and `to_ascii` renders the encoding as `X`/space characters.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                     | [CAPABILITY]                              |
| :-----: | :--------------------- | :----------------------------------------------- | :---------------------------------------- |
|  [01]   | `Barcode.save`         | `save(filename, options=None, text=None) -> str` | serialize to a file (extension by writer) |
|  [02]   | `Barcode.write`        | `write(fp, options=None, text=None) -> None`     | serialize to an open binary stream        |
|  [03]   | `Barcode.render`       | `render(writer_options=None, text=None)`         | in-memory rendering (SVG string / image)  |
|  [04]   | `Barcode.build`        | `build() -> list[str]`                           | single-element 1s/0s encoding list        |
|  [05]   | `Barcode.get_fullcode` | `get_fullcode()`                                 | full human-readable code string           |
|  [06]   | `Barcode.to_ascii`     | `to_ascii() -> str`                              | encoding rendered as `X`/space ASCII      |
|  [07]   | `Barcode.name`         | class attribute                                  | symbology name                            |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_BARCODE]:
- import: `import barcode` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `python-barcode`; the import name is `barcode`.
- registry axis: one name-keyed registry owns symbology selection — `get`/`generate`/`get_barcode_class` resolve a `PROVIDED_BARCODES` key to the concrete class; never import and branch over the individual symbology classes, and never hardcode a name not in `PROVIDED_BARCODES`; an unknown key raises `errors.BarcodeNotFoundError`, mapped at the boundary, never a bare `KeyError`/`ValueError`.
- writer axis: `SVGWriter` (default, dependency-free) and `ImageWriter` (Pillow PNG) are the two render rows on the same `Barcode`; the output format is the `writer` row, never a parallel barcode type per format — `generate(..., writer=ImageWriter())` selects raster, the default emits SVG. `Barcode.render(writer_options=)` returns the in-memory artifact (SVG `str` / PIL `Image`), `save(filename)` appends the writer extension and returns the path, `write(fp)` streams to an open binary handle — the same `Barcode` discriminates on writer + sink.
- option axis: `writer_options` carries `module_width`/`module_height`/`quiet_zone`/`font_size`/`font_path`/`text_distance`/`text_line_distance`/`background`/`foreground`/`center_text`/`guard_height_factor`/`margin_top`/`margin_bottom` as one option dict, plus `compress`/`with_doctype` on `SVGWriter` and `format`/`mode`/`dpi` (`'PNG'`/`'RGB'`/`300`) on `ImageWriter`; human-readable text is controlled by the `text` argument and the `write_text`/`text` options (`save` renders text only when `text` is truthy). `BaseWriter.set_options(dict)` is the single option-merge entry the concrete writers reuse — never a per-option setter family.
- fault axis: `errors.BarcodeError` is the rail base; `IllegalCharacterError`/`NumberOfDigitsError`/`WrongCountryCodeError` are raised by the per-symbology encode validators (e.g. a 12-digit EAN-13 input) and `BarcodeNotFoundError` by the registry — map the typed subtype at the boundary, never swallow into a generic string.
- dimensionality axis: this package is strictly 1D/linear — Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 only. It owns no 2D matrix or stacked symbology; QR/Micro-QR route to `segno`, and any DataMatrix/PDF417 requirement is a separate owner, never a python-barcode call.
- evidence: each symbol captures symbology name, code value, writer kind, module geometry, and output byte length as an imaging receipt.
- stacking: the `SVGWriter` string output composes directly into the document/figure owners (an inline `<svg>` fragment) with no rasterization; the `ImageWriter` PNG path stacks Pillow's encoders so a barcode raster lands in the same image pipeline as a `segno` QR PNG and a `pymupdf` `Pixmap` — one imaging owner discriminates on `(symbology-arm, writer)` rather than minting a per-format type. The `segno` QR arm and this linear arm share the imaging-receipt shape so a mixed QR+barcode label sheet folds into one receipt stream.
- boundary: python-barcode owns linear-barcode generation and SVG/PNG serialization; segno owns the QR/Micro-QR arm beside it; the SVG output feeds the document and figure owners directly; the PNG path routes through Pillow; live UI stays outside this package.

[RAIL_LAW]:
- Package: `python-barcode`
- Owns: linear (1D) barcode generation for the Code39/Code128/EAN/UPC/ITF/Codabar/ISBN/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution, the `errors.*` typed fault rail, and dependency-free SVG plus Pillow-backed PNG serialization
- Accept: `get`/`generate` over a `PROVIDED_BARCODES` name key, `SVGWriter` default and `ImageWriter` raster as writer rows, `writer_options` geometry control, the `errors.*` faults mapped at the boundary, SVG output feeding the document and figure owners, PNG sharing the Pillow/`segno` imaging pipeline
- Reject: wrapper-renames of `get`/`generate`; importing and branching over individual symbology classes where the registry resolves by name; a parallel barcode type per output format; swallowing `errors.*` into a generic string; a claimed 2D/matrix symbology (DataMatrix/PDF417/QR) this package does not provide — QR routes to `segno` and 2D-matrix needs are a separate owner
