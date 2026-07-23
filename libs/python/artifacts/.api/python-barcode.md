# [PY_ARTIFACTS_API_PYTHON_BARCODE]

`python-barcode` (import `barcode`) owns pure-Python linear (1D) barcode generation: a name-keyed symbology registry resolving a `PROVIDED_BARCODES` key to a `barcode.base.Barcode` subclass that serializes through a dependency-free `SVGWriter` or a Pillow-gated `ImageWriter`, and a `barcode.errors.BarcodeError` fault family the per-symbology encode validators raise. It is the `linear` arm of the one `Mark` owner spanning segno's `qr` arm and zxing-cpp's `matrix` arm; the core `graphic/marks/encode#MARK` path admits `SVGWriter` alone, keeping the Pillow raster path off-core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-barcode`
- package: `python-barcode` (MIT)
- import: `barcode`
- owner: `artifacts`
- rail: imaging — the `graphic/marks/encode#MARK` `linear` `EncodeArm` arm
- entry points: console script `python-barcode`; the codec owner is import-only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode base and writer roots

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                                        |
| :-----: | :--------------------------- | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `barcode.base.Barcode`       | symbology base | class attrs `name`/`digits`/`default_writer`/`default_writer_options`               |
|  [02]   | `barcode.writer.BaseWriter`  | writer base    | `set_options`/`calculate_size`/`register_callback`/`packed`/`render`/`save`/`write` |
|  [03]   | `barcode.writer.SVGWriter`   | SVG writer     | dependency-free `xml.dom.minidom` SVG; `SVGWriter()`; `compress`/`with_doctype`     |
|  [04]   | `barcode.writer.ImageWriter` | raster writer  | `ImageWriter(format='PNG', mode='RGB', dpi=300)`; `None` sans Pillow; OFF core      |

- `Barcode.default_writer` binds the `SVGWriter` class object, not an instance; the registry factories pass `writer=None`, resolved to `default_writer()`.

[PUBLIC_TYPE_SCOPE]: fault family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                               |
| :-----: | :----------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `errors.BarcodeError`          | fault base       | base of every python-barcode failure                       |
|  [02]   | `errors.BarcodeNotFoundError`  | resolution fault | requested symbology name is not a `PROVIDED_BARCODES` key  |
|  [03]   | `errors.IllegalCharacterError` | encode fault     | the code string contains a character the symbology forbids |
|  [04]   | `errors.NumberOfDigitsError`   | arity fault      | wrong digit count for a fixed-length symbology (EAN/UPC/…) |
|  [05]   | `errors.WrongCountryCodeError` | EAN fault        | invalid country/prefix code for an EAN symbology           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbology registry and factory

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                                                                                    |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `get` / `get_barcode`             | `get(name, code=None, writer=None, options=None) -> Barcode \| type[Barcode]`                   |
|  [02]   | `get_class` / `get_barcode_class` | `get_class(name) -> type[Barcode]`                                                              |
|  [03]   | `generate`                        | `generate(name, code, writer=None, output=None, writer_options=None, text=None) -> str \| None` |
|  [04]   | `PROVIDED_BARCODES`               | sorted `list[str]` (22 keys, 16 classes)                                                        |

- `get` lowercases `name`, raises `BarcodeNotFoundError` on a miss, builds the symbology when `code` is passed else returns the class; `get_barcode`/`get_barcode_class` are the same objects as `get`/`get_class`.
- `__BARCODE_MAP` aliases the 22 keys onto 16 classes with the upstream quirk `gs1`→`ISBN13` and `isbn`→`ISBN13`.

[ENTRYPOINT_SCOPE]: `Barcode` render and serialize

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                     | [CAPABILITY]                                         |
| :-----: | :------------------------ | :----------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Barcode.render`          | `render(writer_options=None, text=None)`         | in-memory SVG `str` or `PIL.Image`                   |
|  [02]   | `Barcode.write`           | `write(fp, options=None, text=None) -> None`     | open binary stream; the `encode#MARK` `BytesIO` sink |
|  [03]   | `Barcode.save`            | `save(filename, options=None, text=None) -> str` | file write; extension by writer; full filename       |
|  [04]   | `Barcode.build`           | `build() -> list[str]`                           | single-element 1s/0s module-extent list              |
|  [05]   | `Barcode.get_fullcode`    | `get_fullcode() -> str`                          | check-digit-completed code; `linear` score evidence  |
|  [06]   | `Barcode.to_ascii`        | `to_ascii() -> str`                              | encoding rendered as `X`/space ASCII                 |
|  [07]   | `Barcode.name` / `digits` | class attributes                                 | symbology name; fixed-length digit-count arity       |

- `Barcode.__init__(code, writer, **options)`: `writer` is positional with no default; the registry factories supply `None`, resolved to `default_writer()`.
- `render` stamps the human-readable line whenever `default_writer_options["write_text"]` (default `True`) holds or a `text` argument is passed, `text` overriding the `get_fullcode()` string.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registry: the `linear` `EncodeArm` resolves `barcode.get_barcode_class(symbology.value)` against a `PROVIDED_BARCODES` key and constructs the class directly; an unknown key raises `errors.BarcodeNotFoundError` mapped onto `MarkFault.unknown`.
- writer: `SVGWriter` is the sole core-path writer, its dependency-free SVG bytes read through `write(BytesIO(), …)`; the `writer` row owns output selection, so a raster barcode is one `ImageWriter` row rather than a per-format type.
- options: the geometry rides one nested closed `WriterOptions` `TypedDict` on `MarkPayload` — `module_width`/`module_height`/`quiet_zone`/`font_size`/`font_path`/`text_distance`/`text_line_distance`/`background`/`foreground`/`center_text`/`guard_height_factor`/`margin_top`/`margin_bottom` with the `SVGWriter` `compress`/`with_doctype` keys — admitted once through the band's `TypeAdapter`, folded to a hashable `frozendict`, and spread as the positional `options` into `write`; `BaseWriter.set_options` is the single merge entry.
- fault: the encode validators raise `IllegalCharacterError`, `NumberOfDigitsError`, and `WrongCountryCodeError` and the registry raises `BarcodeNotFoundError`, each mapped onto a distinct `MarkFault` case (`illegal`/`arity`/`arity`/`unknown`) at the `linear` arm.
- evidence: the arm folds into the shared `RasterFact` (SVG bytes, zero raster pixel dimensions, a `frozendict` score) and stamps the `get_fullcode` check-digit string and the resolved symbology name onto the score keyed by the `MarkFact` vocabulary, projected to `core/receipt#RECEIPT` `ArtifactReceipt.Preview` at the boundary.

[STACKING]:
- `expression`/`pydantic`/`msgspec`/`beartype`/`anyio`/`structlog` (`.api/` substrate): the SVG bytes from `write(BytesIO(), …)` cross the seam as a `Result[RasterFact, MarkFault]`, the `WriterOptions`/`MarkPayload` band is admitted once through a `TypeAdapter` and a `ValidationError` maps onto `MarkFault.options` as the `.errors()` `loc` `tuple[str, ...]`, the closed `Symbology`/`MarkFault`/`Content` vocabularies and `RasterFact` are `msgspec`/`tagged_union` structs, the `_encode` worker is `beartype`-contracted and offloaded under the one `_MARK_LANE` `CapacityLimiter` via `anyio.to_thread`, and `RasterFact.score` threads onto `ArtifactReceipt.Preview.scores` for the receipt.
- `svgelements` (`.api/svgelements.md`): the SVG markup parses through `SVG.parse(...)` for scale-to-fit / n-up / `bbox()` query before document egress, landing a barcode in the same figure-composition rail as a `segno` QR `svg_inline`, a `great-tables` table SVG, and a `vl-convert` chart SVG.
- `segno`(`.api/segno.md`) + `zxing-cpp`(`.api/zxing-cpp.md`): the `linear`, `qr`, and `matrix` arms share the `RasterFact` → `ArtifactReceipt.Preview` shape, so a mixed label sheet folds through one `Mark.over(MarkOp | Iterable[MarkOp])` into one `Block[ArtifactReceipt]` stream where the `get_fullcode` check digit, the segno `designator`/`version`, and the zxing resolved-format facts all land on one `Preview.scores` `frozendict[str, float | str]` band.
- `stream-zip` (`.api/stream-zip.md`): the `write(BytesIO(), …)` SVG stream feeds a `MemberFile` data iterable, so a batch of barcode SVGs streams into a ZIP label bundle without buffering whole files — identical to the segno `save(BytesIO, kind='svg')` bundle path.

[LOCAL_ADMISSION]:
- lazy `import barcode` (and `from barcode.errors import …`) at the `encode#MARK` boundary; module-level import violates the manifest import policy.
- python-barcode is strictly 1D — QR/Micro-QR route to the segno `qr` arm and DataMatrix/PDF417/Aztec/MaxiCode/rMQR to the zxing-cpp `matrix` arm; the SVG bytes feed the `svgelements`/document figure owners with no rasterization, and the `ImageWriter` raster path stays off-core so the Pillow dependency stays unengaged.

[RAIL_LAW]:
- Package: `python-barcode`
- Owns: linear (1D) barcode generation for the Code39/Code128/EAN-8/EAN-13/EAN-14/JAN/UPC-A/ITF/Codabar/ISBN-10/ISBN-13/ISSN/PZN/GS1-128 symbologies, name-keyed symbology resolution over `PROVIDED_BARCODES`, the 1s/0s `build`/`to_ascii` module model, the `get_fullcode` check-digit projection, the `errors.*` fault rail, and dependency-free `xml.dom.minidom` SVG with Pillow-gated raster serialization
- Accept: `get_barcode_class(symbology.value)` resolution over a `PROVIDED_BARCODES` key, `SVGWriter` as the sole core-path writer, `write(BytesIO(), options, text)` SVG bytes folding to `RasterFact`, a nested closed `WriterOptions` band on `MarkPayload`, the `errors.*` faults mapped onto distinct `MarkFault` cases, SVG bytes feeding the `svgelements`/document figure owners and the `stream-zip` bundle sink
- Reject: importing and branching over individual symbology classes where the registry resolves by name; the all-in-one `generate` (its `output` sink defaults `None` and raises `TypeError`, and it forces a `default_writer` the arm overrides); `ImageWriter` on the core path (it re-introduces the Pillow leak the segno arm removes); an erased `writer_options` `dict` re-validated in the arm; a parallel barcode type per output format; a bare `except Exception` flattening the `errors.*` causes; a claimed 2D/matrix symbology this package does not provide
