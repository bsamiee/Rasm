# [PY_ARTIFACTS_API_RESVG_PY]

`resvg-py` supplies the SVG-to-PNG raster floor for the artifacts imaging rail: a single `svg_to_bytes` entrypoint that parses SVG markup or an `.svg`/`.svgz` file and renders it to PNG-encoded `bytes` through the embedded Rust `resvg 0.47.0` engine. The package owner composes `svg_to_bytes` into the figures and compose `ANNOTATE` path; it removes the Cairo/`cairosvg` and headless-browser rasterization stack because parse, font resolution, and PNG encoding are in-extension, and it never re-implements SVG path flattening, text shaping, or PNG encoding the resvg engine already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `resvg-py`
- package: `resvg-py`
- import: `resvg_py`
- owner: `artifacts`
- rail: imaging
- installed: `0.3.3` reflected via `assay api` on cp315
- entry points: library use is import-only; no console script
- capability: in-process SVG-to-PNG rasterization over the embedded Rust `resvg 0.47.0` engine — string or file (`.svg`/`.svgz`) input, explicit `width`/`height`/`zoom`/`dpi` sizing, CSS `background`, `style_sheet` injection, system/explicit/directory font resolution with generic-family overrides, and per-axis `shape`/`text`/`image` rendering policy, returning PNG-encoded `bytes`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: rasterizer entrypoint and version constants
- rail: imaging

The reflected `__all__` is `['__version__', '__author__', '__resvg_version__', 'svg_to_bytes']`: one rendering function plus three string constants. `svg_to_bytes` is a native `builtin_function_or_method` on the Rust extension `resvg_py.cpython-315-darwin.so`; `__resvg_version__` pins the embedded engine version (`0.47.0`). The function raises `ValueError` on empty/invalid SVG, bad option values, unparseable `background`, or render failure.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :------------------ | :--------------- | :---------------------------------------------- |
|  [01]   | `svg_to_bytes`      | render function  | native SVG-to-PNG rasterizer returning `bytes`  |
|  [02]   | `__version__`       | version constant | installed `resvg-py` package version (`0.3.3`)  |
|  [03]   | `__resvg_version__` | version constant | embedded Rust `resvg` engine version (`0.47.0`) |
|  [04]   | `__author__`        | version constant | package author string (`baseplate-admin`)       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SVG-to-PNG render
- rail: imaging

`svg_to_bytes` accepts `svg_string` or `svg_path` (`.svgz` gzip files supported on the path arm); sizing is `width`/`height`/`zoom`/`dpi` rows; font resolution is `skip_system_fonts` plus `font_files`/`font_dirs`/`*_family` rows; rendering quality is the `shape_rendering`/`text_rendering`/`image_rendering` `Literal` policy axis. It returns PNG-encoded `bytes`.

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                  | [CAPABILITY]                           |
| :-----: | :------------- | :---------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `svg_to_bytes` | `svg_to_bytes(svg_string=None, svg_path=None, background=None, ...) -> bytes` | render SVG markup or file to PNG bytes |

[ENTRYPOINT_SCOPE]: `svg_to_bytes` parameter axes
- rail: imaging

The reflected signature decomposes into source, sizing, parsing, font, and rendering-policy axes; every parameter below traces to the native `inspect.signature` and `.pyi` stub.

| [INDEX] | [AXIS]  | [PARAMETERS]                                                                                                                                                                                                                                                                                                    | [CAPABILITY]                                                                        |
| :-----: | :------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | source  | `svg_string: str \| None = None`, `svg_path: str \| None = None`                                                                                                                                                                                                                                                | UTF-8 SVG markup or `.svg`/`.svgz` file path (one required)                         |
|  [02]   | sizing  | `width: int \| None = None`, `height: int \| None = None`, `zoom: float \| None = None`, `dpi: float = 0.0`                                                                                                                                                                                                     | explicit pixel size, zoom multiplier, or DPI (`0` = SVG default)                    |
|  [03]   | parsing | `background: str \| None = None`, `style_sheet: str \| None = None`, `resources_dir: str \| None = None`, `languages: list[str] \| None = None`                                                                                                                                                                 | CSS canvas background, injected stylesheet, `xlink:href` root, `<switch>` languages |
|  [04]   | font    | `skip_system_fonts: bool = False`, `font_files: list[str] \| None = None`, `font_dirs: list[str] \| None = None`, `font_size: float = 16.0`, `font_family`/`serif_family`/`sans_serif_family`/`cursive_family`/`fantasy_family`/`monospace_family: str \| None = None`                                          | system/explicit/directory font loading with generic-family overrides                |
|  [05]   | policy  | `shape_rendering: Literal["optimize_speed","crisp_edges","geometric_precision"] = "geometric_precision"`, `text_rendering: Literal["optimize_speed","optimize_legibility","geometric_precision"] = "optimize_legibility"`, `image_rendering: Literal["optimize_quality","optimize_speed"] = "optimize_quality"` | per-axis shape/text/image rendering quality policy                                  |
|  [06]   | logging | `log_information: bool = False`                                                                                                                                                                                                                                                                                 | print resvg debug logs to stdout                                                    |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_RASTER]:
- import: `import resvg_py` at boundary scope only; module-level import is banned by the manifest import policy.
- source axis: one `svg_to_bytes` owns rasterization; `svg_string` and `svg_path` are input rows on the same surface (`.svgz` decompresses on the path arm), never a per-input render function.
- sizing axis: `width`/`height`/`zoom`/`dpi` are call rows selecting output resolution; `dpi=0.0` defers to the SVG-declared size, never a separate scale type.
- font axis: `skip_system_fonts` plus `font_files`/`font_dirs` and the six `*_family` overrides own deterministic font resolution as rows; when `skip_system_fonts` is set, explicit font files/dirs supply the faces, never a parallel font-loader surface.
- policy axis: `shape_rendering`/`text_rendering`/`image_rendering` are `Literal` rows on the render call; quality is a policy value per axis, never a parallel renderer type.
- evidence: each render captures package version, embedded `resvg` engine version (`__resvg_version__`), output PNG byte length, and resolved pixel dimensions as an imaging receipt.
- boundary: resvg-py owns SVG-to-PNG rasterization with no Cairo, headless-browser, or external-process dependency; the segno/document/visuals owners emit SVG and route it here for the PNG raster floor; downstream PNG post-processing routes to `pillow` only when explicitly required; live UI stays outside this package.

[RAIL_LAW]:
- Package: `resvg-py`
- Owns: in-process SVG-to-PNG rasterization over the embedded Rust `resvg 0.47.0` engine — string/file input, explicit sizing, font resolution, and per-axis rendering policy
- Accept: SVG markup or `.svg`/`.svgz` files from the imaging, figures, and compose `ANNOTATE` owners rendered to PNG `bytes`
- Reject: wrapper-renames of `svg_to_bytes`; a Cairo/`cairosvg` or headless-browser rasterization path where the in-extension engine needs no external dependency; a hand-rolled SVG parser, text shaper, or PNG encoder the resvg engine already owns; a parallel render function per input or output mode; identity minting the runtime owns
