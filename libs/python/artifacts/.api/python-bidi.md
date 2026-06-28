# [PY_ARTIFACTS_API_PYTHON_BIDI]

`python-bidi` (dist `python-bidi`, import `bidi`) supplies the UAX#9 Unicode Bidirectional Algorithm reorder surface for the artifacts text-shaping rail ahead of HarfBuzz shaping: a top-level `get_display(str_or_bytes, encoding='utf-8', base_dir=None, debug=False)` that resolves a logical-order string into its visual display order over the Rust `unicode-bidi` crate, and a `get_base_level(text) -> int` paragraph-direction probe (`0` LTR / `1` RTL). The package owner composes `get_display` and `get_base_level` into the `typography/shape#SHAPE` `Shaping` owner's `ShapeOp.Bidi` arm — the mixed-direction reorder pass that runs over a logical-order run BEFORE `uharfbuzz` itemises and shapes it, so an Arabic/Hebrew + Latin run reaches `Buffer.shape` already in visual order with the per-paragraph base level resolved. It never re-implements the bidi resolution rules (explicit embeddings/overrides, weak/neutral/implicit type resolution, level-run reordering, mirroring) the `unicode-bidi` Rust core owns, and on the runtime floor the arm dispatches onto the gated `runtime` `anyio.to_process.run_sync` subprocess seam because the native binding has no package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-bidi`
- package: `python-bidi`
- import: `bidi` (dist name `python-bidi`, import name `bidi`); the public names re-export from `bidi.wrapper`, which calls the native `bidi.bidi` PyO3 extension module (`get_display_inner`/`get_base_level_inner`)
- owner: `artifacts`
- rail: text-shaping
- version: `0.6.10`
- license: `LGPL-3.0-or-later` (OSI; dist-info ships `COPYING.LESSER`/`COPYING`, classifier `License :: OSI Approved :: GNU Library or Lesser General Public License (LGPL)`) — a PyO3 binding over the Rust `unicode-bidi` crate; the LGPL obligation is dynamic-link runtime use, never source vendoring of the algorithm
- depends-on: none (self-contained; the `unicode-bidi` Rust core owns the UAX#9 resolution, so no Python-side runtime dependency)
- entry points: console script `pybidi` (`bidi:main` CLI: stdin/argv reorder with `-e`/`-b`/`-d`/`-u`/`-r`); library use is import-only. The surface is the module-level `get_display`/`get_base_level` functions plus the lower-level `bidi.algorithm` introspection family
- capability: UAX#9 Unicode Bidirectional Algorithm reorder of a logical-order `str` (or `encoding`-decoded `bytes`) into visual display order, with `base_dir` paragraph-direction override (`'L'`/`'R'`), a `debug` mode returning the calculated embedding levels, paragraph base-level resolution (`get_base_level` → `0` LTR / `1` RTL), and a pure-Python `bidi.algorithm` fallback exposing the per-stage storage (`get_embedding_levels`/`debug_storage`) with the `upper_is_rtl` debug knob

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reorder value and direction probe
- rail: text-shaping

`python-bidi` is a function surface, not a type surface: `get_display` returns the same kind it was given (`str` in → `str` out, `bytes` in → `bytes` out re-encoded with `encoding`), so there is no wrapper value object to allocate. The only carried value is the paragraph base level as a plain `int` (`0` LTR / `1` RTL). The `StrOrBytes = str | bytes` alias is the input/output union; `base_dir` is a 2-member string vocabulary (`'L'`/`'R'`) the boundary lifts into a closed direction value rather than threading a raw character.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]       | [CAPABILITY]                                                                                                                          |
| :-----: | :-------------------- | :------------------ | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `StrOrBytes`          | input/output union  | `str \| bytes` — the `get_display` argument and return; `bytes` is decoded with `encoding` on entry and re-encoded on exit, `str` passes through unencoded |
|  [02]   | base level (`int`)    | direction value     | `get_base_level` return — `0` LTR / `1` RTL for the first paragraph; the boundary lifts it into the closed paragraph-direction value the `Bidi` arm keys on |
|  [03]   | `base_dir` (`'L'`/`'R'`) | direction override | the optional override that pins the resolved base level instead of computing it from the first strong character; a 2-member string vocabulary, never a free string |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reorder and base-direction probe
- rail: text-shaping

`get_display` is the one reorder entry — it accepts `str` or `bytes`, runs the UAX#9 algorithm over the Rust core, and returns the visually-reordered text in the same kind. `base_dir='L'`/`'R'` overrides the auto-computed paragraph direction (the override the `Bidi` arm sets when the document layout already fixes the column direction), and `debug=True` returns the calculated embedding levels instead of the reordered string for a diagnostic pass. `get_base_level` probes the first paragraph's resolved direction without reordering, so the arm can decide direction once and pass it down to shaping. The lower-level `bidi.algorithm` module is the pure-Python reference implementation of the same surface plus the per-stage storage hooks; it is NOT the gated-binding path and is used only for the `upper_is_rtl` debug behaviour and storage introspection.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                                       |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `get_display`                   | `get_display(str_or_bytes: StrOrBytes, encoding: str = 'utf-8', base_dir: str \| None = None, debug: bool = False) -> StrOrBytes` | UAX#9 reorder logical → visual order (the gated `Bidi` arm; native Rust path) |
|  [02]   | `get_base_level`                | `get_base_level(text: str) -> int`                                                                   | first-paragraph base direction (`0` LTR / `1` RTL) without reordering |
|  [03]   | `bidi.algorithm.get_display`    | `get_display(str_or_bytes, encoding='utf-8', upper_is_rtl=False, base_dir=None, debug=False) -> StrOrBytes` | pure-Python reference reorder; adds the `upper_is_rtl` debug knob (uppercase Latin treated as strong RTL) |
|  [04]   | `bidi.algorithm.get_base_level` | `get_base_level(text, upper_is_rtl=False) -> int`                                                     | pure-Python first-paragraph base level                            |
|  [05]   | `bidi.algorithm.get_embedding_levels` | `get_embedding_levels(text, storage, upper_is_rtl=False, debug=False)`                          | the per-character resolved embedding levels (the reference algorithm's level array) |
|  [06]   | `bidi.algorithm.debug_storage`  | `debug_storage(storage, base_info=False, chars=True, runs=False)`                                     | dump the internal bidi storage (characters / level runs) for a diagnostic trace |

## [04]-[IMPLEMENTATION_LAW]

[TEXT_SHAPING_BIDI]:
- ordering axis: the `Bidi` reorder runs BEFORE `uharfbuzz` shaping, never after — `get_display` produces the visual-order run that `uharfbuzz` `Buffer.add_str` + `shape` then itemises and positions; reordering after shaping would scramble the glyph cluster mapping. The arm is a pre-pass on a mixed-direction logical run, so on a pure-LTR run with no strong-RTL character the arm is a no-op the owner elides rather than a mandatory hop. The `PositionedGlyphRun` the `Shaping` owner emits therefore carries glyphs already in visual order with the per-paragraph base level threaded from `get_base_level`.
- direction axis: `get_base_level(text)` resolves the first paragraph's base direction (`0` LTR / `1` RTL) from the first strong character; `get_display(..., base_dir='L'|'R')` overrides that computation when the document column direction is already fixed by the layout. Compute the base level once at the `Bidi` arm and thread it into the shaping `direction`/`script` decision rather than re-deriving it per fragment — `base_dir` is a 2-member `'L'`/`'R'` vocabulary the boundary lifts into the closed paragraph-direction value, never a free string passed verbatim.
- bytes axis: `get_display` accepts `str` or `bytes`; on `bytes` it decodes with `encoding` (default `'utf-8'`), reorders, and RE-ENCODES the result with the same `encoding`, so a `bytes` in yields `bytes` out and a `str` in yields `str` out — the arm passes a decoded `str` and keeps the byte path off the shaping seam, since `uharfbuzz` shapes a `str`. Never mix the kinds at the boundary (a `str` reorder must not be re-encoded), and never pass the `bytes` form when the upstream is already a decoded run.
- debug axis: `debug=True` makes `get_display` return the calculated embedding levels instead of the reordered string — a diagnostic pass, never the production reorder; the `Bidi` arm runs `debug=False` and routes the level array through `bidi.algorithm.get_embedding_levels`/`debug_storage` only for a trace. The pure-Python `bidi.algorithm.get_display` additionally exposes `upper_is_rtl=True` (treat uppercase Latin as strong RTL) — a reference-test affordance the native binding ignores, never a production knob.
- evidence: each reorder pass captures the resolved paragraph base level (`0`/`1`), whether a `base_dir` override was applied, the presence of a strong-RTL character (whether the arm was a no-op or an active reorder), and the run length as a text-shaping receipt fact folded into the `Shaping` owner's `ArtifactReceipt.Document`/`Preview` contribution; map a Rust panic at the subprocess seam into the `runtime` `RuntimeRail` fault rather than surfacing it bare across the interpreter boundary.
- boundary: python-bidi owns the UAX#9 logical→visual reorder and the paragraph base-level probe — the mixed-direction pre-pass `uharfbuzz` does NOT perform (HarfBuzz shapes a run in a single given direction; it does not run the bidi resolution that splits a mixed run into directional level runs). `uharfbuzz` owns the OpenType itemisation/shaping AFTER reorder, `fonttools` the face/variation selection, `blackrenderer` the COLRv1 color-glyph raster of the shaped run; the `Bidi` reorder feeds the shaped run that `documents/emit#DOCUMENT` places and `figures/compose#COMPOSE` annotates. The reorder is the gated Rust hop; live UI stays outside this package.

[RAIL_LAW]:
- Package: `python-bidi`
- Owns: the UAX#9 Unicode Bidirectional Algorithm reorder of a logical-order `str`/`bytes` into visual display order via `get_display` (over the Rust `unicode-bidi` core), the `base_dir='L'|'R'` paragraph-direction override, the `debug` embedding-level diagnostic mode, the `get_base_level` first-paragraph direction probe, and the pure-Python `bidi.algorithm` reference surface (`get_embedding_levels`/`debug_storage`/`upper_is_rtl`)
