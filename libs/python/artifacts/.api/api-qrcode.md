# [PY_ARTIFACTS_API_QRCODE]

`qrcode` supplies the QR-code matrix and image surface for the artifacts image rail: a `QRCode` builder with data/error-correction/version control, an SVG image-factory family that renders without a raster dependency, and a one-shot `make` helper that drive QR symbol generation into a boolean matrix or an SVG/raster image. The package owner composes `QRCode`, the SVG image factories, and `make` into the image owner; it never re-implements Reed-Solomon QR encoding qrcode already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `qrcode`
- package: `qrcode`
- import: `qrcode`
- owner: `artifacts`
- rail: image
- installed: `8.2` reflected via `python -c "import qrcode"` on cp315
- entry points: console script `qr` (CLI); library use is import-only
- capability: QR symbol generation (versions 1-40, error-correction L/M/Q/H), boolean-matrix output, SVG rendering (no raster dependency), raster rendering via a Pillow image factory, ASCII/TTY rendering, mask-pattern control

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: builder and constants
- rail: image

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `QRCode` | builder | configure version/error-correction/box-size/border, add data, emit matrix or image |
| [2] | `ERROR_CORRECT_L` / `ERROR_CORRECT_M` / `ERROR_CORRECT_Q` / `ERROR_CORRECT_H` | error-correction axis | redundancy-level selector |
| [3] | `image.svg.SvgImage` / `SvgPathImage` / `SvgFillImage` / `SvgPathFillImage` / `SvgFragmentImage` | SVG factory | vector image factories (no raster dependency) |
| [4] | `image.pil.PilImage` | raster factory | Pillow-backed raster factory |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build, encode, and render
- rail: image

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `QRCode` | `QRCode(version=None, error_correction=ERROR_CORRECT_M, box_size=10, border=4, image_factory: type[GenericImage] | None = None, mask_pattern=None)` | configure a QR builder |
| [2] | `QRCode.add_data` | `add_data(data, optimize=20) -> None` | append data segments |
| [3] | `QRCode.make` | `make(fit=True) -> None` | compute the symbol (auto-fit version) |
| [4] | `QRCode.get_matrix` | `get_matrix() -> list[list[bool]]` | the boolean module matrix |
| [5] | `QRCode.make_image` | `make_image(image_factory=None, **kwargs) -> GenericImage` | render to SVG (default raster needs Pillow) |
| [6] | `QRCode.print_ascii` | `print_ascii(out=None, tty=False, invert=False) -> None` | render to ASCII |
| [7] | `make` | `make(data=None, **kwargs) -> GenericImage` | one-shot build-and-render helper |

## [4]-[IMPLEMENTATION_LAW]

[IMAGE_QR]:
- import: `import qrcode` at boundary scope only; module-level import is banned by the manifest import policy.
- builder axis: one `QRCode` owns the symbol; version/error-correction/box-size/border/mask are constructor rows, never a per-config builder type.
- render axis: the image factory is a row value — `image.svg.*` for the no-dependency vector path and `image.pil.PilImage` for the raster path; `get_matrix` is the raw-matrix path feeding a custom renderer; rendering is a factory row, never a parallel QR type.
- error-correction axis: `ERROR_CORRECT_L/M/Q/H` is the redundancy row; the owner selects it by the deployment surface (print vs screen).
- evidence: each symbol captures version, error-correction level, module count, image factory, and output byte length as an image receipt.
- boundary: qrcode owns QR generation; raster post-processing routes to `pillow`; the SVG path feeds the document/visuals owners directly; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `qrcode`
- Owns: QR symbol generation, boolean-matrix output, SVG/raster/ASCII rendering, version and error-correction control
- Accept: QR symbol generation feeding the image, document, and visuals owners
- Reject: wrapper-renames of `add_data`/`make_image`; a hand-rolled Reed-Solomon encoder; a forced raster path where the SVG factory needs no dependency; identity minting the runtime owns
