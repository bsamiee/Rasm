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

The error-correction constants are `ERROR_CORRECT_L`, `ERROR_CORRECT_M`, `ERROR_CORRECT_Q`, and `ERROR_CORRECT_H`; the SVG factory family covers `SvgImage`, `SvgPathImage`, `SvgFillImage`, `SvgPathFillImage`, and `SvgFragmentImage`.

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE]        | [CAPABILITY]                   |
| :-----: | :------------------- | :-------------------- | :----------------------------- |
|   [1]   | `QRCode`             | builder               | QR symbol builder              |
|   [2]   | `ERROR_CORRECT_*`    | error-correction axis | redundancy-level selector      |
|   [3]   | `image.svg.*`        | SVG factory           | no-dependency vector factories |
|   [4]   | `image.pil.PilImage` | raster factory        | Pillow-backed raster factory   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build, encode, and render
- rail: image

The constructor row carries version, error-correction, box-size, border, image-factory, and mask policy.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                 | [CAPABILITY]                                |
| :-----: | :------------------- | :--------------------------- | :------------------------------------------ |
|   [1]   | `QRCode`             | builder policy               | configure a QR builder                      |
|   [2]   | `QRCode.add_data`    | data plus optimize policy    | append data segments                        |
|   [3]   | `QRCode.make`        | fit flag                     | compute the symbol (auto-fit version)       |
|   [4]   | `QRCode.get_matrix`  | no-arg matrix read           | the boolean module matrix                   |
|   [5]   | `QRCode.make_image`  | image factory plus kwargs    | render to SVG (default raster needs Pillow) |
|   [6]   | `QRCode.print_ascii` | output stream plus TTY flags | render to ASCII                             |
|   [7]   | `make`               | data plus render kwargs      | one-shot build-and-render helper            |

## [4]-[IMPLEMENTATION_LAW]

[IMAGE_QR]:
- import: `import qrcode` at boundary scope only; module-level import is banned by the manifest import policy.
- builder axis: one `QRCode` owns the symbol; version/error-correction/box-size/border/mask are constructor rows, never a per-config builder type.
- render axis: the image factory is a row value — `image.svg.*` for the no-dependency vector path and `image.pil.PilImage` for the raster path; `get_matrix` is the raw-matrix path feeding a custom renderer; rendering is a factory row, never a parallel QR type.
- error-correction axis: `ERROR_CORRECT_L/M/Q/H` is the redundancy row; the owner selects it by the deployment surface (print vs screen).
- evidence: each symbol captures version, error-correction level, module count, image factory, and output byte length as an image receipt.
- boundary: qrcode owns QR generation; raster post-processing routes to `pillow`; the SVG path feeds the document/visuals owners directly; live UI stays outside this package.

[RAIL_LAW]:
- Package: `qrcode`
- Owns: QR symbol generation, boolean-matrix output, SVG/raster/ASCII rendering, version and error-correction control
- Accept: QR symbol generation feeding the image, document, and visuals owners
- Reject: wrapper-renames of `add_data`/`make_image`; a hand-rolled Reed-Solomon encoder; a forced raster path where the SVG factory needs no dependency; identity minting the runtime owns
