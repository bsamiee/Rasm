# [PY_ARTIFACTS_API_PYTHON_BIDI]

`python-bidi` (import `bidi`) owns the UAX#9 bidirectional reorder for the artifacts text-shaping domain: `get_display` resolves a logical-order `str`/`bytes` into visual order over the Rust `unicode-bidi` core, and `get_base_level` probes the first paragraph's base direction (`0` LTR / `1` RTL). Both names re-export from `bidi.wrapper` onto the native `bidi.bidi` PyO3 extension; pure-Python `bidi.algorithm` is the reference/diagnostic path. `typography/shape#SHAPE` drives it as the `ShapeOp.BIDI` pre-pass ahead of `uharfbuzz`, gated onto the `runtime` `to_process` subprocess boundary.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reorder value and direction probe
- concern: text-shaping

`python-bidi` is a function surface: `get_display` returns the kind it was given (`str`->`str`, `bytes`->`bytes` re-encoded with `encoding`), so no wrapper value object allocates; the only carried value is the paragraph base level as a plain `int`. `StrOrBytes = str | bytes` (declared in both `bidi.wrapper` and `bidi.algorithm`) is the input/output union; `base_dir` is a 2-member `'L'`/`'R'` vocabulary the boundary lifts into a closed direction value, never a raw character.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]          | [CAPABILITY]                                                                         |
| :-----: | :----------------------- | :--------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `StrOrBytes`             | input/output union     | `str \| bytes`; `get_display` arg/return, `bytes` decoded/re-encoded with `encoding` |
|  [02]   | base level (`int`)       | direction value        | `get_base_level` return; `0` LTR / `1` RTL first paragraph                           |
|  [03]   | `base_dir` (`'L'`/`'R'`) | direction override     | override pinning the base level vs first-strong compute; 2-member vocabulary         |
|  [04]   | storage (`dict`)         | reference engine state | `get_empty_storage()` mutable `{'base_level','base_dir','chars','runs'}`             |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: reorder and base-direction probe (native path)
- concern: text-shaping

`get_display(str_or_bytes, encoding='utf-8', base_dir=None, debug=False) -> StrOrBytes` reorders `str`/`bytes` over the Rust core (`get_display_inner(text, base_dir, debug) -> str`) and returns the same kind; `base_dir='L'|'R'` overrides the auto-computed direction, `debug=True` returns the resolved-levels dump instead of the reordered string. `get_base_level(text) -> int` probes the first paragraph's direction without reordering, so the arm decides direction once and threads it into shaping.

| [INDEX] | [SURFACE]                        | [CAPABILITY]                                                                          |
| :-----: | :------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `get_display`                    | UAX#9 reorder logical -> visual order (the gated `BIDI` arm; native Rust path)        |
|  [02]   | `get_base_level`                 | first-paragraph base direction (`0` LTR / `1` RTL) without reordering                 |
|  [03]   | `bidi.bidi.get_display_inner`    | native PyO3 reorder `get_display` wraps; `str`-in/`str`-out, wrapper owns bytes codec |
|  [04]   | `bidi.bidi.get_base_level_inner` | native PyO3 base-level probe `get_base_level` wraps                                   |

[ENTRYPOINT_SCOPE]: pure-Python reference engine and stage pipeline (`bidi.algorithm`)
- concern: text-shaping

`bidi.algorithm` is the pure-Python reference of the same public surface (its `get_display` adds the `upper_is_rtl` debug knob), exposing the UAX#9 stages as separately callable functions over a `get_empty_storage()` dict. Stages run in fixed UAX#9 order, each taking `(storage, debug=False)`; `get_embedding_levels` fills `chars`/`runs`, and `debug_storage` dumps them. Owner code reaches this module only to inspect or verify the native reorder.

| [INDEX] | [SURFACE]                      | [CAPABILITY]                                                                                           |
| :-----: | :----------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `explicit_embed_and_overrides` | X1-X9: resolve explicit embeddings/overrides, push the directional-status stack, drop removed controls |
|  [02]   | `resolve_weak_types`           | W1-W7: resolve NSM/EN/ES/ET/AN/CS weak types within each level run                                     |
|  [03]   | `resolve_neutral_types`        | N0-N2: resolve neutral/isolate-formatting types to L/R by surrounding strong context                   |
|  [04]   | `resolve_implicit_levels`      | I1-I2: raise embedding levels by resolved type (the per-character level array)                         |
|  [05]   | `reorder_resolved_levels`      | L1-L2: reverse contiguous level runs highest-to-lowest into visual order                               |
|  [06]   | `apply_mirroring`              | L4: swap mirrored characters (brackets/quotes) in RTL runs via the `MIRRORED` table                    |
|  [07]   | `calc_level_runs`              | partition the resolved char stream into same-level runs (the directional-run decomposition)            |

Driver, storage, and diagnostic callables carry distinct signatures:

| [INDEX] | [SURFACE]                     | [SHAPE]                                                                                     |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `get_empty_storage`           | `get_empty_storage() -> dict`                                                               |
|  [02]   | `get_embedding_levels`        | `get_embedding_levels(text, storage, upper_is_rtl=False, debug=False)`                      |
|  [03]   | `reverse_contiguous_sequence` | `reverse_contiguous_sequence(chars, line_start, line_end, highest_level, lowest_odd_level)` |
|  [04]   | `debug_storage`               | `debug_storage(storage, base_info=False, chars=True, runs=False)`                           |

[ENTRYPOINT_SCOPE]: UCD data tables and the mirroring pairing
- concern: text-shaping

These module-level data are what the reference engine folds over — the native path carries its own copies inside the Rust crate, so the owner reads them only as reference constants (the explicit-level ceiling as a validation bound, a mirrored-bracket pair without re-running the algorithm). `MIRRORED` lives in `bidi.mirror` (re-exported from `bidi.algorithm`); the rest sit in `bidi.algorithm`.

| [INDEX] | [SURFACE]              | [SHAPE]                     | [CAPABILITY]                                                      |
| :-----: | :--------------------- | :-------------------------- | :---------------------------------------------------------------- |
|  [01]   | `MIRRORED`             | `dict[str, str]`, 362       | UAX BidiMirroring pairs (`'(' -> ')'`); L4 single-pair lookup     |
|  [02]   | `PARAGRAPH_LEVELS`     | `{'L': 0, 'AL': 1, 'R': 1}` | strong-type -> base-level map; `base_dir`/first-strong -> `0`/`1` |
|  [03]   | `EXPLICIT_LEVEL_LIMIT` | `62`                        | UAX#9 max explicit embedding depth; validation ceiling            |
|  [04]   | `X2_X5_MAPPINGS`       | `dict`                      | X2-X5: RLE/LRE/RLO/LRO embed/override push                        |
|  [05]   | `X6_IGNORED`           | `list`                      | X6: ignored control types                                         |
|  [06]   | `X9_REMOVED`           | `list`                      | X9: removed types (RLE/LRE/RLO/LRO/PDF, BN)                       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- ordering: bidi resolves BEFORE `uharfbuzz` shaping, and the shaping feed is the directional-run decomposition, never the whole-paragraph `get_display` string — the owner splits the paragraph into same-level runs, shapes each run's LOGICAL-order text through `Buffer.add_str`+`shape` with the run-level direction, then orders the shaped runs per UAX#9 L2.
- `get_display` is the whole-paragraph reorder only where reordered TEXT itself is the deliverable; a pure-LTR run is a no-op the owner elides (`get_display('hello') == 'hello'`; `get_display('אבג abc') == 'abc גבא'`).
- direction: `get_base_level(text)` resolves the first paragraph's base direction (`0`/`1`) from the first strong character, `get_display(..., base_dir='L'|'R')` overrides it when the layout fixes the column direction; compute the base level once at the `BIDI` arm and thread it into the shaping `direction`/`script` decision. `base_dir` is the 2-member `'L'`/`'R'` value the boundary lifts, passed as the second positional to `get_display_inner`.
- bytes: `get_display` on `bytes` decodes with `encoding` (default `'utf-8'`), reorders the decoded `str`, and re-encodes with the same `encoding`, so kind is preserved end to end (`get_display('אבג abc'.encode()) == b'abc \xd7\x92\xd7\x91\xd7\x90'`); the arm passes a decoded `str` and keeps the byte path off the `Buffer.add_str` shaping boundary.
- debug: native `get_display(debug=True)` returns the resolved-levels/classes dump instead of the reordered string, a diagnostic the `BIDI` arm never runs; a structured per-character level array routes through `bidi.algorithm.get_embedding_levels`/`calc_level_runs` over `get_empty_storage()` with `debug_storage` for the trace, and the pure-Python `get_display` honours `upper_is_rtl` (strong-RTL uppercase Latin), a reference-test affordance the native binding ignores.
- mirroring: the L4 character-mirror swap is INSIDE the native reorder (`get_display` returns mirrored output); `bidi.mirror.MIRRORED` is the same pairing table as data for a single mirrored-pair lookup without re-running the algorithm.

[STACKING]:
- `typography/shape#SHAPE` (within-lib primary): the `Shaping` owner reaches bidi via `lazy from bidi import get_base_level, get_display`, dispatching the `bidi` case onto the worker with `lane.offload(Kernel.of(_bidi_runs, KernelTrait.HOSTILE), request)`; the acceptor resolves the paragraph on the worker — `get_base_level` for the base level and `bidi.algorithm.get_embedding_levels`+`calc_level_runs` over `get_empty_storage()` for the same-level run decomposition (the one production slice the native binding does not expose), `base_dir=_BIDI_BASE[direction]` lifting the override — and crosses `(base_level, ((start, stop, level), ...))` back. `ShapeOp.SHAPE` shapes each run's logical-order slice with the run-level direction and assembles the positioned runs in L2 visual order into the `PositionedGlyphRun`; `get_display` crosses only when reordered TEXT is the deliverable.
- universal-tier transport (`.api/`): the reorder rides the `anyio` (`.api/anyio.md`) `to_process.run_sync(func, *args, cancellable=False, limiter=...)` subprocess boundary under the shaping-wide `CapacityLimiter` — the SUBPROCESS lane, not the `to_thread` lane `uharfbuzz`/`blackrenderer` use — worker death surfaces as `anyio.BrokenWorkerProcess`; every `BIDI` arm returns the `expression` (`.api/expression.md`) `Result`, a Rust panic folding in via `async_boundary`.
- universal-tier substrate: `beartype` (`.api/beartype.md`) validates the `base_dir`/`text` boundary inputs; reorder params ride a frozen `msgspec.Struct` (`.api/msgspec.md`) encoded into the content key; spans record through `structlog` + OpenTelemetry (`.api/structlog.md`, `.api/opentelemetry-api.md`) on the worker hop; a surfaced level array lands as a `numpy` (`.api/numpy.md`) `int8` embedding-level array keyed by character index.

[LOCAL_ADMISSION]:
- `python-bidi` is the locale-free DEFAULT bidi owner and the operative path on this interpreter: `get_display`/`get_base_level` for the whole-paragraph reorder and base-direction probe, owning the mixed-direction pre-pass `uharfbuzz` does not (HarfBuzz shapes a single given direction, never the resolution that splits a mixed run). `uharfbuzz` owns OpenType shaping after reorder, `fonttools` the font model + `SVGPathPen`, `blackrenderer` the COLRv1 raster, `uniseg` UAX#14/#29 segmentation.
- escalate to `pyicu` `Bidi` only for the surface `python-bidi` cannot give — line-relative reorder (`setLine` after line-break), the explicit visual-run decomposition (`getVisualRun` itemising a mixed run into single-direction spans), or combined reorder+Arabic-join+digit-shape (`BidiTransform.transform`); `python-bidi`'s whole-string `get_display` yields neither a line-scoped nor a visual-run-itemised result.
- `uniseg` is orthogonal, never redundant: UAX#14/#29 SEGMENTATION beside UAX#9 REORDER, side by side in the pipeline, its `unicodedatawrapper.bidirectional` Bidi_Class read a per-codepoint property, not a reorder. Unlike a `uniseg` pass (pure, offline, inline), the bidi reorder is the one shaping step that REQUIRES the `runtime` worker boundary because of the native binding.
