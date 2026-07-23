# [PY_ARTIFACTS_API_UNISEG]

`uniseg` owns the pure-Python UAX text-segmentation engine for the artifacts typography rail: UAX #29 grapheme-cluster / word / sentence segmentation and UAX #14 line-break-opportunity resolution over a tailorable breakable-table walk, each algorithm a uniform `break`/`breakables`/`boundaries`/token-iterator family; fixed-width east-asian wrapping and the UCD property surface ride the same engine. Segmentation tables ship in-package pinned to one Unicode release, so break results are interpreter-independent; the engine owns no shaping, layout optimization, or font model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uniseg`
- package: `uniseg` (MIT)
- import: `uniseg` (submodules `linebreak`, `graphemecluster`, `wordbreak`, `sentencebreak`, `wrap`, `breaking`, `unicodedatawrapper`, `derived`, `emoji`, `unicodeproperty`)
- owner: `artifacts`
- rail: typography (line-layout)
- unicode-data: `16.0.0` (`uniseg.unidata_version`) — segmentation, derived, and emoji tables bundled in `db_lookups.py` (two-level trie behind `get_handle`/`get_value`), so break results pin to this UCD release regardless of interpreter
- build-floor: pure-Python universal wheel `py3-none-any` (`Root-Is-Purelib: true`), `Requires-Python >=3.9`, no abi/cp gate
- depends-on: none at runtime. Optional `unicodedata2` accelerator loads when present, else stdlib `unicodedata`; absent here, so `category`/`east_asian_width`/`bidirectional` follow the interpreter UCD, split from the release-pinned segmentation tables ([04] data-pin)
- entry points: none (library only)
- capability: UAX #29 extended grapheme-cluster (Indic conjunct, emoji ZWJ, regional-indicator aware), word, and sentence segmentation; UAX #14 line-break over the full LB1–LB31 rule set; the `TailorFunction` locale-override hook and `Run` tailorable breakable-table walk; fixed-width east-asian-aware wrapping with tab expansion and ambiguous-width policy; the UCD General_Category / East_Asian_Width / Bidi_Class re-exports; the UAX #44 derived-property and UTS #51 emoji predicates

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: break-property enums and the breakable-table primitives

Break-property enums subclass `EnumProperty(str, Enum)`: every member equals its UCD short code (`LineBreak.BK == "BK"`), `member.value` is the code and `str(member)` the member name. This identity lets a consumer `in`-test a break class against a `frozenset[str]` of UCD codes directly — the layout owner's mandatory-break set `{"BK","CR","LF","NL"}` membership test depends on it. `Breakable` is the 0/1 cell enum, `Breakables` the bitstream, `Run[_T]` the generic tailorable walk every algorithm folds over.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]         | [CAPABILITY]                                                               |
| :-----: | :----------------------------- | :-------------------- | :------------------------------------------------------------------------- |
|  [01]   | `LineBreak` / `LB`             | UAX #14 property enum | the `Line_Break` class set; `LB` alias                                     |
|  [02]   | `GraphemeClusterBreak` / `GCB` | UAX #29 property enum | the `Grapheme_Cluster_Break` set; `GCB` alias (`PACINGMARK` = SpacingMark) |
|  [03]   | `WordBreak` / `WB`             | UAX #29 property enum | the `Word_Break` set; `WB` alias                                           |
|  [04]   | `SentenceBreak` / `SB`         | UAX #29 property enum | the `Sentence_Break` set; `SB` alias                                       |
|  [05]   | `Breakable`                    | breakable cell enum   | `DoNotBreak` (0) / `Break` (1); `bool(cell)` gives the 0/1 the folds use   |
|  [06]   | `Breakables`                   | bitstream alias       | `Iterable[Literal[0, 1]]`; 1 = break before, `len == len(s)`               |
|  [07]   | `TailorFunction`               | tailoring hook alias  | `Callable[[str, Breakables], Breakables]`; the `tailor=` override seam     |
|  [08]   | `Run[_T]`                      | tailorable table walk | `Run(text, func=…)`; the custom-algorithm walk the engines fold over       |
|  [09]   | `EnumProperty`                 | base property enum    | `str, Enum` base; `str(m)` name, `m.value` UCD code                        |
|  [10]   | `PropertyFunction`             | property-fn alias     | `Callable[[str], _T]`; the `property=` per-code-point read                 |

[PUBLIC_TYPE_SCOPE]: UCD value enums

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]         | [CAPABILITY]                                                                |
| :-----: | :---------------------------- | :-------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Category` / `GC`             | General_Category enum | grouped `General_Category` set; `GC` alias; `category(chr)` return          |
|  [02]   | `EastAsianWidth` / `EA`       | East_Asian_Width enum | `East_Asian_Width` set; `EA` alias; the width axis `tt_width` keys on       |
|  [03]   | `IndicConjunctBreak` / `InCB` | derived-property enum | `NONE`/`LINKER`/`CONSONANT`/`EXTEND`; the GB9c `Indic_Conjunct_Break` input |

`EastAsianWidth`: `A` ambiguous, `F` fullwidth, `H` halfwidth, `N` neutral, `NA` narrow, `W` wide.

[PUBLIC_TYPE_SCOPE]: fixed-width wrapping formatter surface

A `Wrapper` drives a `Formatter` (`TTFormatter` the monospace impl), folding `s` through it at `iter_breakables` breaks and yielding wrapped lines after `.wrap`. `TTFormatter` validates a narrow `tab_char` (raises `ValueError` on a wide fill char).

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]         | [CAPABILITY]                                                              |
| :-----: | :------------ | :-------------------- | :------------------------------------------------------------------------ |
|  [01]   | `Formatter`   | formatter protocol    | the `Protocol` a `Wrapper` drives; any width medium                       |
|  [02]   | `Wrapper`     | wrapping engine       | `.wrap` folds `s` through the formatter at `iter_breakables` breaks       |
|  [03]   | `TTFormatter` | fixed-width formatter | the monospace `Formatter`; `.lines()` yields wrapped strings post-`.wrap` |

[Formatter]: `wrap_width` `tab_width` `text_extents(s)` `handle_text(text, extents)` `handle_new_line()`; `None` disables wrapping and `0` selects the narrowest width.
[Wrapper]: `wrap(formatter, s, cur=0, offset=0, *, iter_breakables=line_break_breakables) -> int` breaks by formatter extent and returns line count.
[TTFormatter]: `TTFormatter(width, *, tab_width=8, tab_char=' ', ambiguous_as_wide=False)` rejects a wide `tab_char`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the four UAX boundary-algorithm families

Every algorithm is one four-function family — grapheme, word, sentence are UAX #29, line UAX #14. For family `X`: `X_break(c, /) -> <Enum>` (per-code-point read, `XX`/`Other` unassigned) and `X_breakables(s, /, *, property=X_break) -> Breakables` (the canonical 0/1 stream, `len == len(s)`), with `X_boundaries` and the token iterator completing it. `property=` swaps the class read for a tailored/legacy map; `tailor=` applies a `TailorFunction`. `X_breakables` is the intermediate `boundaries`/`break_units` derive from, so needing both indices and tokens folds one stream through both. Surfaces live in `uniseg.<family-module>`; `line_break_breakables` is the LB1–LB31 core.

| [INDEX] | [FAMILY]           | [PROPERTY_READ]          | [TOKEN_ITERATOR]    | [CAPABILITY]                                                    |
| :-----: | :----------------- | :----------------------- | :------------------ | :-------------------------------------------------------------- |
|  [01]   | `grapheme_cluster` | `grapheme_cluster_break` | `grapheme_clusters` | GB1–GB13 extended cluster; Indic conjunct, emoji ZWJ, RI pairs  |
|  [02]   | `word`             | `word_break`             | `words`             | WB1–WB999; punctuation and space are own tokens                 |
|  [03]   | `line`             | `line_break`             | `line_break_units`  | LB1–LB31 opportunity resolution; the `BREAK` arm segment source |
|  [04]   | `sentence`         | `sentence_break`         | `sentences`         | SB1–SB998 sentence stream                                       |

[ENTRYPOINT_SCOPE]: breakable-table primitives and the `Run` walk

`boundaries`/`break_units` are the table-to-output primitives every family's `…_boundaries`/`…_units` build on — call them directly to convert a CUSTOM (tailored) breakable stream into indices or tokens without re-running an algorithm. `Run` is the tailorable per-code-point walk for authoring a wholly new break algorithm, never for re-deriving a UAX rule uniseg already owns; a consumer reaches for `tailor=` first.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                 |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `uniseg.breaking.boundaries`  | `boundaries(breakables, /) -> Iterator[int]`; yields each 1-position then the terminal `len` |
|  [02]   | `uniseg.breaking.break_units` | `break_units(s, breakables, /) -> Iterator[str]`; `len(s) == len(breakables)` required       |
|  [03]   | `uniseg.breaking.Run`         | `Run(text, func=…)`; per-position lookahead/behind over a skip table and deferred array      |

- `Run` methods: `.walk` `.head` `.is_following` `.is_leading` `.break_here` `.do_not_break_here` `.set_skip_table` `.set_default` `.literal_breakables`

[ENTRYPOINT_SCOPE]: fixed-width east-asian wrapping and width

`tt_wrap` is the one-call monospace wrap (terminal / code-listing / fixed-pitch column); `tt_width`/`tt_text_extents` are the standalone width probes (`1` halfwidth / `2` fullwidth, ambiguous resolved by `ambiguous_as_wide`). `tt_wrap` and `wrap` share a `cur`, `offset`, `iter_breakables` tail (`iter_breakables=line_break_breakables` default, grapheme for char-wrap). These ride the interpreter's UCD `east_asian_width` ([04] data-pin), not the release tables.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                        |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `uniseg.wrap.tt_wrap`         | `tt_wrap(s, /, wrap_width, *, tab_width=8, tab_char=' ', ambiguous_as_wide=False) -> Iterator[str]` |
|  [02]   | `uniseg.wrap.tt_width`        | `tt_width(s, /, index=0, *, ambiguous_as_wide=False) -> int`; cell width `1`/`2` at `s[index]`      |
|  [03]   | `uniseg.wrap.tt_text_extents` | `tt_text_extents(s, /, *, ambiguous_as_wide=False) -> list[int]`; prefix over `grapheme_clusters`   |
|  [04]   | `uniseg.wrap.wrap`            | module-level `Wrapper.wrap`; drive a custom `Formatter` for proportional fixed-opportunity wrap     |

[ENTRYPOINT_SCOPE]: UCD property re-exports and derived/emoji predicates

`uniseg.unicodedatawrapper` re-exports stdlib `unicodedata` with the enum-typed `category`/`east_asian_width`; `uniseg.derived` and `uniseg.emoji` add the UAX #44 / UTS #51 boolean predicates the break algorithms consume and a consumer reads for classification. `category`/`east_asian_width`/`indic_conjunct_break` return the enum type; every `derived`/`emoji` predicate is `(c, /) -> bool`.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                                                                           |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `uniseg.unicodedatawrapper.category`         | enum-typed `General_Category`, `category(chr) -> Category`                             |
|  [02]   | `uniseg.unicodedatawrapper.east_asian_width` | enum-typed `East_Asian_Width`, `east_asian_width(chr) -> EastAsianWidth`               |
|  [03]   | `uniseg.unicodedatawrapper.*`                | raw stdlib `unicodedata` re-export incl. `bidirectional` (Bidi_Class); interpreter-UCD |
|  [04]   | `uniseg.derived.*`                           | UAX #44 derived boolean predicates; 16.0.0-pinned bundled tables                       |
|  [05]   | `uniseg.derived.indic_conjunct_break`        | `indic_conjunct_break(c) -> IndicConjunctBreak`; the GB9c value                        |
|  [06]   | `uniseg.emoji.*`                             | UTS #51 emoji predicates; `extended_pictographic` drives GB11/WB3c                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- uniform-shape: a consumer selects granularity (grapheme < word < line-unit < sentence) by choosing the family, never by re-segmenting a finer grain into a coarser one. `X_breakables(s)` is the canonical 0/1 intermediate; needing both indices and tokens folds one stream through `boundaries` and `break_units`, never two algorithm passes.
- str-enum-identity: break enums are `EnumProperty(str, Enum)`, so a class read is `str`-comparable to its UCD code and `in`-testable against a `frozenset[str]`; a consumer compares against `member.value` (the code), never `str(member)` (the name), and never re-wraps the enum in a parallel `StrEnum`.
- tailoring: a locale-specific override is a `tailor: TailorFunction` (bitstream rewrite) or `property: PropertyFunction` (remapped class read); a wholly new algorithm is a `Run` fold, with uniseg's UAX rule engine the substrate under both.
- data-pin (load-bearing split): segmentation and the derived/emoji predicates read the bundled release tables (`db_lookups.py`, `unidata_version`), so break results are interpreter-independent; `category`/`east_asian_width`/`bidirectional` follow stdlib `unicodedata` (absent `unicodedata2`), the interpreter UCD. A consumer treats segmentation as release-pinned and the width/category measure as interpreter-pinned, spanning no single UCD version; `unicodedata2` realigns the measure.
- evidence: a segmentation pass is pure and offline (no IO, native call, or event-loop hop), so it folds inline in the `BREAK` acceptor with no `runtime` worker seam; the resolved opportunity stream, break-unit count, mandatory/opportunity split, and `unidata_version` fold into the `ArtifactReceipt.Document` content key as interior evidence, never new receipt fields.

[STACKING]:
- `typography/layout#LAYOUT` (primary within-lib): the `LineLayout` owner reaches uniseg as the `BREAK` `LayoutOp` arm — `line_break_units(text)` segments into break-unit tokens, `line_break(unit[-1])` reads the trailing class, projecting each gap into a `BreakClass` row (`MANDATORY` for a class in `{"BK","CR","LF","NL"}`, else `OPPORTUNITY`, `PROHIBITED` inside a no-break cluster). uniseg owns the UAX #14 opportunity resolution; the Knuth-Plass total-fit Box/Glue/Penalty program threading that stream is the owner's own, and the proportional document paragraph takes that optimum, never `tt_wrap`'s greedy first-fit.
- `typography/shape#SHAPE`: `grapheme_clusters`/`grapheme_cluster_boundaries` give the UAX #29 extended-cluster boundaries reconciling the `PositionedGlyphRun` per-glyph `cluster` indices HarfBuzz assigns under `BufferClusterLevel.MONOTONE_GRAPHEMES` (`.api/uharfbuzz.md`), so cursor, selection, and per-cluster width attribution land on cluster boundaries. uniseg owns the extended-cluster definition HarfBuzz monotone clustering approximates but does not canonicalize.
- `visualization/table#TABLE`: `tt_text_extents(cell)`/`tt_width(cell)` give the fixed-pitch/CJK-correct cell-width prefix the `great-tables` (`.api/great-tables.md`) column geometry sums, `grapheme_clusters` the iteration unit so a combining sequence counts as one cell; the east-asian-width axis is interpreter-UCD-pinned, read as a width measure, never a segmentation authority.
- universal-tier rails: the layout break-policy/knob carriers are frozen `msgspec.Struct` rows (`.api/msgspec.md`) encoded through a module-level `msgspec.msgpack.Encoder` into the content key; `beartype` (`.api/beartype.md`) validates the `str`/measure inputs; every `BREAK`/`HYPHENATE`/`PARAGRAPH` arm returns the `expression` (`.api/expression.md`) `Result`/`RuntimeRail[ContentKey]` rail. A pure-offline pass folds inline with no `anyio`/`runtime` worker seam, unlike `python-bidi`'s gated `to_process` reorder.

[LOCAL_ADMISSION]:
- `line_break`/`line_break_units` reads the break and `grapheme_clusters` the user-perceived character (UAX #29 extended cluster); both own rule sets a hand-rolled break-class table or `list(text)` iteration drops — Brahmic/Aksara, emoji, regional-indicator, East-Asian quotation for the break; combining sequences, Indic conjuncts, emoji ZWJ, RI pairs for the cluster.
- a locale break override rides `tailor`/`property` on the existing entry; a genuinely new algorithm is a `Run` fold, never a forked algorithm or parallel break owner.
- `tt_wrap`/`TTFormatter` serve the monospace/terminal medium; the proportional document paragraph rides the `typography/layout#LAYOUT` Knuth-Plass DP fed by `line_break_units`.
- segmentation is treated as release-pinned (the bundled tables) and the `east_asian_width`/`category` measure as interpreter-pinned (the stdlib fallback); no hand-vendored segmentation tables to match the interpreter.

[RAIL_LAW]:
- Package: `uniseg`
- Owns: UAX #29 grapheme-cluster / word / sentence segmentation and UAX #14 line-break resolution (the four-function family per algorithm, the tailorable `Run`/`Breakable` walk, the `boundaries`/`break_units` primitives), fixed-width east-asian-aware wrapping (`tt_wrap`/`tt_width`/`tt_text_extents`/`TTFormatter`/`Wrapper`/`Formatter`), and the UCD General_Category / East_Asian_Width / Bidi_Class re-exports with the UAX #44 derived and UTS #51 emoji predicates, over bundled release tables
- Accept: the `BREAK` `LayoutOp` arm (`line_break`/`line_break_units` -> `BreakClass` stream feeding the Knuth-Plass DP); `grapheme_clusters` boundaries reconciling `typography/shape#SHAPE` HarfBuzz clusters and feeding `tt_text_extents`; the `tt_width`/`tt_text_extents` monospace/CJK column measure on `visualization/table#TABLE`; `tailor`/`property` as the locale-override seam
- Reject: a hand-rolled LB1–LB31 table where `line_break` folds it; `list(text)` iteration where `grapheme_clusters` owns the extended cluster; a forked algorithm where `tailor`/`property` or a `Run` fold suffices; `tt_wrap` greedy first-fit where the document paragraph needs the total-fit optimum; comparing a break enum against `str(member)` instead of `member.value`; assuming one UCD version spans both the release-pinned tables and the interpreter-pinned `east_asian_width`/`category`
