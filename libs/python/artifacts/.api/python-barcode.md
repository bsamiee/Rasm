# [PY_ARTIFACTS_API_PYTHON_BARCODE]

`python-barcode` (dist `python-barcode`, import `barcode`) supplies the pure-Python linear (1D) barcode surface for the `graphic/marks/encode#MARK` codec, the `linear` arm of the one `Mark` owner that spans segno's QR/Micro-QR `qr` arm and zxing-cpp's 2D-matrix `matrix` arm. It is a name-keyed symbology registry (`get`/`get_class`/`generate`) resolving a `PROVIDED_BARCODES` key to a concrete `barcode.base.Barcode` subclass, a `Barcode` that encodes the code string into a module-extent list and serializes it through a dependency-free `SVGWriter` or a Pillow-gated `ImageWriter`, and a `barcode.errors.BarcodeError` family the per-symbology encode validators raise. The codec owner resolves the class through `get_barcode_class(symbology.value)`, constructs it against an explicit `SVGWriter()`, streams the SVG through `Barcode.write(fp, …)`, and maps the four `errors.*` causes onto distinct `MarkFault` cases; it never re-implements the 1D encoding tables python-barcode already owns, never admits `ImageWriter` on the core path (the PNG path re-introduces the Pillow dependency leak the segno arm exists to remove), and routes QR/Micro-QR to segno and DataMatrix/PDF417/Aztec/MaxiCode/rMQR to zxing-cpp because this package is strictly linear.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-barcode`
- package: `python-barcode`
- import: `barcode` (dist name `python-barcode`, import name `barcode`)
- owner: `artifacts`
- rail: imaging — the `graphic/marks/encode#MARK` `linear` `EncodeArm` arm
- license: `MIT` (`Classifier: License :: OSI Approved :: MIT License`; permissive, no copyleft obligation; the Pantone/proprietary rejection in `_REBUILD_BRIEF` [03] does not touch it)
- build-floor: `Requires-Python: >=3.9`; pure-Python wheel (`py3-none-any`), no compiled extension and no cp-gate — installs on the cp315 interpreter unconditionally
- installed: `0.16.1`
- asset: the `SVGWriter` path needs no dependency; the `ImageWriter` raster path needs Pillow (the `python-barcode[images]` extra), resolved through the root `pyproject` Pillow admission, never a per-package pin. The core `encode#MARK` path is `SVGWriter`-only, so the Pillow dependency is never engaged through this owner.
- entry points: console script `python-barcode` (CLI: `create`/`list`, `barcode.pybarcode`); the codec owner is import-only and never shells the CLI
- capability: linear (1D) barcode generation for the Code39/Code128/EAN-8/EAN-13/EAN-14/JAN/UPC-A/ITF/Codabar/ISBN-10/ISBN-13/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution over a 22-key registry, a 1s/0s module-extent `build`/`to_ascii` model, the `get_fullcode` human-readable check-digit projection, the `errors.*` typed fault rail, dependency-free `xml.dom.minidom` SVG serialization, and a Pillow-gated raster (PNG/any Pillow format) writer with configurable module geometry, quiet zone, guard-bar height, margins, and human-readable text

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode base and writer roots
- rail: imaging

`Barcode.default_writer` is the class `SVGWriter` (the class object, not an instance); `generate` instantiates it as `Barcode.default_writer()`. The package re-exports the concrete symbology classes at top level (`Code128`, `Code39`, `EAN8`, `EAN8_GUARD`, `EAN13`, `EAN13_GUARD`, `EAN14`, `JAN`, `UPCA`, `ITF`, `CODABAR`, `ISBN10`, `ISBN13`, `ISSN`, `PZN`, `Gs1_128`), each subclassing `barcode.base.Barcode`; the codec owner resolves them by registry key through `get_barcode_class`, never importing the classes individually. `SVGWriter`/`ImageWriter` both subclass `BaseWriter` and bind the four render callbacks (`_init`, `_paint_module`/`_create_module`, `_paint_text`/`_create_text`, `_finish`) into the shared render loop; `create_svg_object(with_doctype)` builds the `xml.dom.minidom` document root. The module guards Pillow at import time (`from PIL import Image …; except ImportError: Image = None`) and binds `ImageWriter = None` when Pillow is absent, so the raster writer is a name that exists only when Pillow imports — the encode owner never reaches that path regardless.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `barcode.base.Barcode` | symbology base | abstract 1D barcode; class attributes `name`, `digits` (fixed-length arity), `default_writer = SVGWriter` (the class), `default_writer_options`; `init(code, writer, **options)` (the registry passes `writer`, defaulted to `None` upstream, then resolved to `default_writer()`) |
| [02] | `barcode.writer.BaseWriter` | writer base | abstract renderer; `init(initialize, paint_module, paint_text, finish)` binds the four render callbacks; `set_options(options)` merges the option dict, `calculate_size(modules_per_line, number_of_lines)` derives the canvas, `register_callback(action, callback)` swaps one stage, `packed(line)`/`render(code)`/`save(filename, output)`/`write(content, fp)` drive the render loop |
| [03] | `barcode.writer.SVGWriter` | SVG writer | default, dependency-free SVG over `xml.dom.minidom` (`create_svg_object`); `SVGWriter()` zero-arg construction; `compress`/`with_doctype` options; the sole writer the `encode#MARK` core path admits |
| [04] | `barcode.writer.ImageWriter` | raster writer | Pillow-backed raster; `ImageWriter(format='PNG', mode='RGB', dpi=300)`; any Pillow-supported `format`; the module binds it to `None` when Pillow is absent; OFF the `encode#MARK` core path because it re-introduces the Pillow leak the segno arm removes |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: imaging

`barcode.errors` is the typed failure rail; `BarcodeError` is the base, and the registry/encode validators raise the specific subtypes. `BarcodeNotFoundError` is raised by `get`/`get_class` for a name not in `PROVIDED_BARCODES`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `errors.BarcodeError` | fault base | base of every python-barcode failure |
| [02] | `errors.BarcodeNotFoundError` | resolution fault | requested symbology name is not a `PROVIDED_BARCODES` key |
| [03] | `errors.IllegalCharacterError` | encode fault | the code string contains a character the symbology forbids |
| [04] | `errors.NumberOfDigitsError` | arity fault | wrong digit count for a fixed-length symbology (EAN/UPC/…) |
| [05] | `errors.WrongCountryCodeError` | EAN fault | invalid country/prefix code for an EAN symbology |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbology registry and factory
- rail: imaging

`PROVIDED_BARCODES` is the sorted `list[str]` of the 22 accepted name keys: `codabar`, `code128`, `code39`, `ean`, `ean13`, `ean13-guard`, `ean14`, `ean8`, `ean8-guard`, `gs1`, `gs1_128`, `gtin`, `isbn`, `isbn10`, `isbn13`, `issn`, `itf`, `jan`, `nw-7`, `pzn`, `upc`, `upca`. The keys are aliased onto 13 distinct classes — `ean`/`ean13`/`isbn`/`gs1`→`EAN13`-family (note the upstream quirk: `gs1`→`ISBN13`, `isbn`→`ISBN13`), `gtin`/`ean14`→`EAN14`, `upc`/`upca`→`UPCA`, `nw-7`/`codabar`→`CODABAR`, `jan`→`JAN`. The codec owner resolves a `Symbology.value` registry name through `get_barcode_class` and constructs the class against an explicit `SVGWriter()`; it does NOT use the all-in-one `generate`, which forces a non-`None` `output` sink and a `default_writer` it does have to override. `get` lowercases the name, raises `BarcodeNotFoundError` on a miss, and — when `code` is supplied — builds the symbology in one call (otherwise returns the class). `get_barcode` is the same object as `get`; `get_barcode_class` is the same object as `get_class` (a thin `get_class(name) -> get(name)` alias).

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `get` / `get_barcode` | `get(name, code=None, writer=None, options=None) -> Barcode \| type[Barcode]` (`@overload`: `code: str` → `Barcode`, `code=None` → `type[Barcode]`) | resolve the class, or build the symbology when `code` is set |
| [02] | `get_class` / `get_barcode_class` | `get_class(name) -> type[Barcode]` | resolve name to symbology class (the registry member the `encode#MARK` `linear` arm composes) |
| [03] | `generate` | `generate(name, code, writer=None, output, writer_options=None, text=None) -> str \| None` | build + write in one call; `output` is REQUIRED (`output=None` raises `TypeError`), accepts `str` path / `os.PathLike` / `BinaryIO`; returns the saved filename for a `str` path, else `None`; not on the core path |
| [04] | `PROVIDED_BARCODES` | sorted `list[str]` (22 keys, 13 classes) | accepted symbology name keys; the `linear` `Symbology.value` set resolves against it |

[ENTRYPOINT_SCOPE]: `Barcode` render and serialize
- rail: imaging

`Barcode.__init__(code, writer, **options)` builds a symbology — `writer` is positional with NO default on `Barcode` itself (the registry factories supply `None`, which `__init__` resolves to `default_writer()`). The render trio shares the positional `(options, text)` tail: `save(filename, options=None, text=None)` appends the writer extension and returns the full filename, `write(fp, options=None, text=None)` streams to an open binary handle, `render(writer_options=None, text=None)` returns the in-memory artifact (an SVG `str` from `SVGWriter`, a `PIL.Image` from `ImageWriter`). Human-readable text renders only when `text` is truthy. `build() -> list[str]` returns the single-element 1s/0s module-extent list, `get_fullcode()` returns the human-readable code (the EAN/UPC check-digit-completed string the `encode#MARK` score stamps as the `linear` evidence), and `to_ascii()` renders the encoding as an `X`/space ASCII preview. The `encode#MARK` core path constructs the class against `SVGWriter()` and calls `write(BytesIO(), writer_options, text)` so the SVG bytes land in the same in-memory sink as the segno arm, never touching disk.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Barcode.render` | `render(writer_options=None, text=None)` | in-memory rendering (SVG `str` / `PIL.Image`); the `encode#MARK` arm reads SVG bytes via `write` instead |
| [02] | `Barcode.write` | `write(fp, options=None, text=None) -> None` | serialize to an open binary stream (the `encode#MARK` `BytesIO` sink) |
| [03] | `Barcode.save` | `save(filename, options=None, text=None) -> str` | serialize to a file (extension by writer); returns the full filename |
| [04] | `Barcode.build` | `build() -> list[str]` | single-element 1s/0s module-extent list |
| [05] | `Barcode.get_fullcode` | `get_fullcode() -> str` | full human-readable code (check-digit completed); the `linear` arm's score evidence |
| [06] | `Barcode.to_ascii` | `to_ascii() -> str` | encoding rendered as `X`/space ASCII |
| [07] | `Barcode.name` / `digits` | class attributes | symbology name; fixed-length digit-count arity |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_BARCODE]:
- import: `lazy import barcode` (and `lazy from barcode.errors import BarcodeNotFoundError, IllegalCharacterError, NumberOfDigitsError, WrongCountryCodeError`) at `encode#MARK` boundary scope only; module-level import is banned by the manifest import policy. The dist name is `python-barcode`; the import name is `barcode`.
- registry axis: one name-keyed registry owns symbology selection — the `linear` `EncodeArm` arm resolves `barcode.get_barcode_class(symbology.value)` against a `PROVIDED_BARCODES` key and constructs the class directly; never import and branch over the individual symbology classes, never hardcode a name not in `PROVIDED_BARCODES`, never reach for the all-in-one `generate` (it forces an `output` sink and a `default_writer` the arm overrides). An unknown key raises `errors.BarcodeNotFoundError`, mapped onto `MarkFault.unknown`, never a bare `KeyError`/`ValueError`.
- writer axis: `SVGWriter` is the SOLE writer the `encode#MARK` core path admits — the arm constructs `get_barcode_class(name)(content, writer=barcode.writer.SVGWriter())` and reads the dependency-free SVG bytes through `write(BytesIO(), …)`. `ImageWriter` (Pillow raster) exists on the same `Barcode` as a second writer row but is deliberately OFF the core path: routing through it re-introduces the Pillow dependency leak the segno arm was admitted to remove, so a raster barcode is a documented out-of-core capability, never the default rail. The `writer` row, not a parallel barcode type per format, owns output selection.
- option axis: the python-barcode geometry rides one nested closed `WriterOptions` `TypedDict` on the `encode#MARK` `MarkPayload` band — `module_width`/`module_height`/`quiet_zone`/`font_size`/`font_path`/`text_distance`/`text_line_distance`/`background`/`foreground`/`center_text`/`guard_height_factor`/`margin_top`/`margin_bottom` plus the `SVGWriter` `compress`/`with_doctype` keys — admitted once through the band's `TypeAdapter`, deep-folded to a hashable `frozendict`, and spread as the positional `options` into `write`; human-readable text rides the `text` argument (`write` renders text only when `text` is truthy). `BaseWriter.set_options(options)` is the single option-merge entry the concrete writers reuse — never a per-option setter family, never an erased `dict[str, object]` re-validated in the arm.
- fault axis: `errors.BarcodeError` is the rail base; the per-symbology encode validators raise `IllegalCharacterError` (a forbidden character), `NumberOfDigitsError` (wrong digit count for a fixed-length symbology, e.g. a 12-digit EAN-13), and `WrongCountryCodeError` (an invalid EAN prefix), and the registry raises `BarcodeNotFoundError` — each named exactly at the `linear` arm and mapped onto a distinct `MarkFault` case (`illegal`/`arity`/`arity`/`unknown`), never a bare `except Exception` flattening the causes and never swallowed into a generic string.
- dimensionality axis: this package is strictly 1D/linear — Code39/Code128/EAN-8/EAN-13/EAN-14/JAN/UPC-A/ITF/Codabar/ISBN-10/ISBN-13/ISSN/PZN/GS1-128 only. It owns no 2D matrix or stacked symbology; QR/Micro-QR route to the segno `qr` arm, and DataMatrix/PDF417/Compact-PDF417/Aztec/MaxiCode/rMQR route to the zxing-cpp `matrix` arm — never a python-barcode call for either.
- evidence: the `linear` arm folds into the shared `RasterFact` (SVG bytes, the default zero pixel dimensions since the SVG path carries no raster, and a `frozendict` score) and stamps the `get_fullcode` check-digit-completed string plus the resolved symbology name onto the score keyed by the `MarkFact` vocabulary, projected to `core/receipt#RECEIPT` `ArtifactReceipt.Preview` at the boundary.
- boundary: python-barcode owns linear-barcode generation and `SVGWriter`/`ImageWriter` serialization; the `encode#MARK` `Mark` owner composes its `linear` arm beside the segno `qr` arm and the zxing-cpp `matrix` arm; the SVG bytes feed the `svgelements`/document figure owners directly with no rasterization; the `ImageWriter` raster path stays off-core (Pillow leak); live UI stays outside this package.

[STACKING]:
- The `linear` arm folds onto the SHARED `libs/python/.api` substrate exactly as the segno arm does: the SVG bytes from `write(BytesIO(), …)` cross the seam as a `Result[RasterFact, MarkFault]` (`expression`), the `WriterOptions`/`MarkPayload` band is admitted once through a `TypeAdapter` and a `ValidationError` maps onto `MarkFault.options` as the full `tuple[str, ...]` of `.errors()` `loc` paths (`pydantic`), the closed `Symbology`/`MarkFault`/`Content` vocabularies and the `RasterFact` value object are `msgspec`/`expression.tagged_union` structs, the `_encode` worker is `beartype`-contracted at definition time and offloaded off the event loop under the one `_MARK_LANE` `CapacityLimiter` (`anyio.to_thread`), and the `RasterFact.score` evidence threads onto `ArtifactReceipt.Preview.scores` for the `structlog`/OTel receipt — never a flat folder-only call.
- The dependency-free `SVGWriter` byte stream stacks straight into the folder-tier figure rail: the SVG markup parses through `svgelements` `SVG.parse(...)` for scale-to-fit / n-up / `bbox()` query before document egress, landing a barcode in the same figure-composition rail as a `segno` QR `svg_inline()`, a `great-tables` table SVG, and a `vl-convert` chart SVG. One imaging owner discriminates on `(EncodeArm, writer)`, never a per-format barcode type.
- The `linear` arm and the segno `qr` arm share the imaging-receipt shape (`RasterFact` → `ArtifactReceipt.Preview`), so a mixed QR + linear + 2D-matrix label sheet folds through one `Mark.over(MarkOp | Iterable[MarkOp])` entrypoint into one `Block[ArtifactReceipt]` stream — the `get_fullcode` linear check digit, the segno `designator`/`version`, and the zxing resolved-format facts all land on the same `Preview.scores` `frozendict[str, float | str]` band.
- The `get_barcode_class(name)(content, …).write(BytesIO(), …)` SVG byte stream writes into the same `io.BytesIO` sink that feeds a `stream-zip` `MemberFile` data iterable, so a batch of barcode SVGs streams into a ZIP label bundle without buffering whole files — identical to the segno `save(BytesIO, kind='svg')` bundle path.

[RAIL_LAW]:
- Package: `python-barcode`
- Owns: linear (1D) barcode generation for the Code39/Code128/EAN-8/EAN-13/EAN-14/JAN/UPC-A/ITF/Codabar/ISBN-10/ISBN-13/ISSN/PZN/GS1-128 symbologies, name-keyed 22-key symbology resolution, the 1s/0s `build`/`to_ascii` module model, the `get_fullcode` check-digit projection, the `errors.*` typed fault rail, and dependency-free `xml.dom.minidom` SVG plus Pillow-gated raster serialization
- Accept: `get_barcode_class(symbology.value)` registry resolution over a `PROVIDED_BARCODES` key, `SVGWriter` as the sole core-path writer, `write(BytesIO(), options, text)` SVG bytes folding to `RasterFact`, a nested closed `WriterOptions` geometry band on `MarkPayload`, the `errors.*` faults mapped onto distinct `MarkFault` cases, SVG bytes feeding the `svgelements`/document figure owners and the `stream-zip` bundle sink
- Reject: wrapper-renames of `get`/`generate`/`get_barcode_class`; importing and branching over individual symbology classes where the registry resolves by name; the all-in-one `generate` (it forces an `output` sink and a `default_writer` the arm overrides); `ImageWriter` on the core path (it re-introduces the Pillow leak segno removes); an erased `writer_options` `dict` re-validated in the arm; a parallel barcode type per output format; a bare `except Exception` flattening the `errors.*` causes; a claimed 2D/matrix symbology (DataMatrix/PDF417/QR) this package does not provide — QR routes to the segno `qr` arm and 2D-matrix to the zxing-cpp `matrix` arm
