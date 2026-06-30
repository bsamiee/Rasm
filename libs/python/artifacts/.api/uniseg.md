# [PY_ARTIFACTS_API_UNISEG]

`uniseg` supplies the pure-Python Unicode text-segmentation engine for the artifacts typography rail: the four UAX boundary algorithms — UAX #29 grapheme-cluster / word / sentence segmentation and UAX #14 line-break-opportunity resolution — each exposed as a uniform four-function family (a `…_break(char)` per-code-point property read, a `…_breakables(s)` 0/1 opportunity bitstream, a `…_boundaries(s)` index iterator, and a `…s(s)`/`…_units(s)` token iterator), plus the `Run`/`Breakable`/`TailorFunction` breakable-table primitives those families fold over, plus the fixed-width east-asian wrapping owner (`tt_width`/`tt_text_extents`/`tt_wrap`/`TTFormatter`/`Wrapper`/`Formatter`), plus the UCD-property surface (`category`/`east_asian_width`/`bidirectional` re-exports and the UAX #44 derived-property + UTS #51 emoji predicates). It bundles its own Unicode 16.0.0 segmentation tables in `db_lookups.py` (a two-level trie behind `get_handle`/`get_value`) so every break algorithm is data-pinned independent of the interpreter, and owns no shaping, no layout optimization, and no font model. This is the categorical-best UAX-segmentation owner: the `typography/layout#LAYOUT` `LineLayout` owner reaches it as the `BREAK` `LayoutOp` arm — `line_break(char)`/`line_break_units(text)` resolve the UAX #14 mandatory/opportunity positions the hand-rolled Knuth-Plass Box/Glue/Penalty total-fit dynamic program threads — and the grapheme/east-asian-width surface reconciles shaped-run cluster counts and measures fixed-width column geometry. The owner never re-derives the UAX #29/#14 break rules, never hand-builds a break-class table, and never re-implements the tailorable breakable-table walk — uniseg owns the full segmentation fold; only the paragraph-optimization algebra on top is the design's own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uniseg`
- package: `uniseg`
- import: `uniseg` (submodules `uniseg.linebreak` / `uniseg.graphemecluster` / `uniseg.wordbreak` / `uniseg.sentencebreak` / `uniseg.wrap` / `uniseg.breaking` / `uniseg.unicodedatawrapper` / `uniseg.derived` / `uniseg.emoji` / `uniseg.unicodeproperty`)
- owner: `artifacts`
- rail: typography (line-layout)
- license: `MIT` (OSI; classifier `License :: OSI Approved :: MIT License`, dist-info ships `LICENSE`)
- version: `0.10.1`
- unicode-data: `16.0.0` (`uniseg.unidata_version`) — the segmentation property tables (`Line_Break`/`Grapheme_Cluster_Break`/`Word_Break`/`Sentence_Break`/`InCB`/emoji) are bundled in-package (`db_lookups.py`), so break results are pinned to UCD 16.0.0 regardless of the running interpreter's UCD
- build-floor: pure-Python, universal wheel `py3-none-any` (`Root-Is-Purelib: true`); `Requires-Python >=3.9`; no abi gate, no cp-gate, present on cp315
- depends-on: none at runtime (zero `Requires-Dist`). `unicodedata2` is an OPTIONAL accelerator `uniseg.unicodedatawrapper` imports when present and silently falls back to stdlib `unicodedata` when absent — it is NOT installed here, so the General_Category / East_Asian_Width / Bidi_Class re-exports ride the interpreter's bundled UCD (17.0.0 on this cp315 interpreter), a split from the 16.0.0-pinned segmentation tables documented in [04]
- entry points: none (library only; the `python -m uniseg.<module>` doctest hooks are not a public surface)
- capability: UAX #29 extended grapheme-cluster segmentation (Indic conjunct + emoji ZWJ + regional-indicator pair aware); UAX #29 word and sentence segmentation; UAX #14 line-break-opportunity resolution (full LB1–LB31 rule set incl. Brahmic/Aksara, emoji, regional-indicator, quotation East-Asian context); a per-algorithm tailoring hook (`TailorFunction`) for locale-specific break overrides without re-implementing the algorithm; the `Run` tailorable breakable-table walk and the `boundaries`/`break_units` table-to-token primitives; fixed-width (terminal/monospace) east-asian-aware wrapping with tab expansion and ambiguous-width policy; the UCD property surface (General_Category, East_Asian_Width, Bidi_Class, combining, numeric/decimal/digit, name/lookup/normalize re-exports) and the UAX #44 derived-property + UTS #51 emoji boolean predicates

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: break-property enums and the breakable-table primitives
- rail: typography

uniseg's break-property enums all subclass `EnumProperty(str, Enum)`, so every member IS its UCD short string (`LineBreak.BK == "BK"`, `GraphemeClusterBreak.OTHER == "Other"`) and `str(member)` is the member NAME while `member.value` is the UCD code. This `str`-enum identity is load-bearing for the `typography/layout#LAYOUT` owner, whose `_MANDATORY: frozenset[str] = frozenset({"BK", "CR", "LF", "NL"})` membership test `line_break(char) in _MANDATORY` works precisely because `line_break` returns a `str`-valued enum equal to its UCD code. The breakable table is the `Breakable` 0/1 `Enum`, `Breakables = Iterable[Literal[0, 1]]` is the bitstream alias, and `Run[_T]` is the generic tailorable walk every algorithm folds over.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------ | :----------- |
|  [01]   | `LineBreak` / `LB` | UAX #14 property enum | the 47-member `Line_Break` value set (`BK`/`CR`/`LF`/`NL` mandatory, `BA`/`HY`/`SP`/`ZW`/`GL`/`WJ`/`CB`/`CM`/`ZWJ` opportunity-control, `OP`/`CL`/`CP`/`QU`/`IS`/`NS`/`B2`/`IN` punctuation, `AL`/`HL`/`NU`/`PR`/`PO`/`SY` alphanumeric, `ID`/`EB`/`EM`/`CJ`/`H2`/`H3`/`JL`/`JV`/`JT`/`RI` CJK/emoji, `AK`/`AP`/`AS`/`VF`/`VI`/`SA` Brahmic, `AI`/`SG`/`XX` resolved-away); `LB` is the alias. `str`-enum: `LineBreak.BK == "BK"` |
|  [02]   | `GraphemeClusterBreak` / `GCB` | UAX #29 property enum | the 15-member `Grapheme_Cluster_Break` set (`OTHER`/`CR`/`LF`/`CONTROL`/`EXTEND`/`ZWJ`/`REGIONAL_INDICATOR`/`PREPEND`/`PACINGMARK` [=`SpacingMark`]/`L`/`V`/`T`/`LV`/`LVT`); `GCB` alias |
|  [03]   | `WordBreak` / `WB` | UAX #29 property enum | the 20-member `Word_Break` set (`OTHER`/`CR`/`LF`/`NEWLINE`/`EXTEND`/`ZWJ`/`REGIONAL_INDICATOR`/`FORMAT`/`KATAKANA`/`HEBREW_LETTER`/`ALETTER`/`SINGLE_QUOTE`/`DOUBLE_QUOTE`/`MIDNUMLET`/`MIDLETTER`/`MIDNUM`/`NUMERIC`/`EXTENDNUMLET`/`WSEGSPACE`); `WB` alias |
|  [04]   | `SentenceBreak` / `SB` | UAX #29 property enum | the `Sentence_Break` set (`OTHER`/`CR`/`LF`/`EXTEND`/`SEP`/`FORMAT`/`SP`/`LOWER`/`UPPER`/`OLETTER`/`NUMERIC`/`ATERM`/`SCONTINUE`/`STERM`/`CLOSE`); `SB` alias |
|  [05]   | `Breakable` | breakable cell enum | `Breakable.DoNotBreak` (0) / `Breakable.Break` (1); `__bool__` yields the 0/1 truthiness the table folds use |
|  [06]   | `Breakables` | bitstream alias | `Iterable[Literal[0, 1]]` — the 0/1 opportunity stream a `…_breakables(s)` yields (1 = break BEFORE this position, 0 = do not), same length as `s` |
|  [07]   | `TailorFunction` | tailoring hook alias | `Callable[[str, Breakables], Breakables]` — receives the source string and its default breakable stream, returns a customized one; the locale-override seam threaded by the `tailor=` keyword on every `…_boundaries`/`…s`/`…_units` entry |
|  [08]   | `Run[_T]` | tailorable table walk | `Run(text, func=lambda x: x)` — the generic per-code-point property walk (`walk`/`head`/`position`/`curr`/`prev`/`next`/`cc`/`pc`/`nc`/`attr`/`char`/`is_following`/`is_leading`/`is_continuing`/`break_here`/`do_not_break_here`/`set_skip_table`/`set_default`/`literal_breakables`) the rule engines fold over; the substrate for a fully custom break algorithm |
|  [09]   | `EnumProperty` | base property enum | `str, Enum` base; `__str__` -> member name, `__repr__` -> `Class.NAME`, `value` -> UCD code. Every break/category/width enum derives it, so all are `str`-comparable to their UCD codes |
|  [10]   | `PropertyFunction` | property-fn alias | `Callable[[str], _T]` — the per-code-point property-read signature; the `property=` keyword on every break entry accepts one (for a legacy/tailored property mapping) |

[PUBLIC_TYPE_SCOPE]: UCD General_Category and East_Asian_Width value enums
- rail: typography

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------ | :----------- |
|  [01]   | `Category` / `GC` | General_Category enum | the full `General_Category` set incl. the grouped aliases (`LU`/`LL`/`LT`/`LM`/`LO`/`L`, `MN`/`MC`/`ME`/`M`, `ND`/`NL`/`NO`/`N`, `PC`/`PD`/`PS`/`PE`/`PI`/`PF`/`PO`/`P`, `SM`/`SC`/`SK`/`SO`/`S`, `ZS`/`ZL`/`ZP`/`Z`, `CC`/`CF`/`CS`/`CO`/`CN`/`C`); `GC` alias. Returned by `category(chr)` |
|  [02]   | `EastAsianWidth` / `EA` | East_Asian_Width enum | the 6-member `East_Asian_Width` set (`A` ambiguous / `F` fullwidth / `H` halfwidth / `N` neutral / `NA` narrow / `W` wide); `EA` alias. Returned by `east_asian_width(chr)`; the width axis `tt_width` keys on |

[PUBLIC_TYPE_SCOPE]: UAX #44 derived-property and UTS #51 emoji enum
- rail: typography

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------ | :----------- |
|  [01]   | `IndicConjunctBreak` / `InCB` | derived-property enum | `NONE`/`LINKER`/`CONSONANT`/`EXTEND` — the `Indic_Conjunct_Break` derived property `grapheme_cluster_breakables` consumes for the GB9c conjunct-cluster rule; returned by `indic_conjunct_break(c)`; `InCB` alias |

[PUBLIC_TYPE_SCOPE]: fixed-width wrapping formatter surface
- rail: typography

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
| :-----: | :-------- | :------------ | :----------- |
|  [01]   | `Formatter` | formatter protocol | the `Protocol` a `Wrapper` drives: `wrap_width: int \| None` (`None` = no wrap, `0` = wrap as narrow as possible), `tab_width: int`, `text_extents(s) -> list[int]`, `handle_text(text, extents)`, `handle_new_line()` — implement it to wrap into any logical-width medium (e.g. shaped-advance widths) |
|  [02]   | `Wrapper` | wrapping engine | `Wrapper().wrap(formatter, s, cur=0, offset=0, *, iter_breakables=line_break_breakables) -> int` — folds `s` through the formatter, breaking at `iter_breakables` opportunities when the cumulative extent exceeds `wrap_width`, expanding tabs to `tab_width` stops; returns the line count. `iter_breakables` is swappable to `grapheme_cluster_breakables` for character-wrap |
|  [03]   | `TTFormatter` | fixed-width formatter | `TTFormatter(width, *, tab_width=8, tab_char=' ', ambiguous_as_wide=False)` — the monospace `Formatter` impl; `.lines()` yields wrapped strings after a `Wrapper.wrap`; `tab_char` validated narrow (`ValueError` on a wide fill char) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the four UAX boundary-algorithm families
- rail: typography

Each of the four segmentation algorithms is the SAME four-function shape: a per-code-point property read, a 0/1 breakable bitstream (length == `len(s)`), a boundary-index iterator, and a token iterator. Every boundary/token entry takes the `property=` keyword (swap the property-read for a legacy/tailored mapping) and the `tailor=` keyword (a `TailorFunction` applied to the bitstream for locale overrides). The `typography/layout#LAYOUT` `BREAK` arm composes the LINE family (`line_break` + `line_break_units`); the GRAPHEME family reconciles shaped-run clusters and feeds `tt_text_extents`; WORD/SENTENCE serve text-extract token granularity.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
|  [01]   | `uniseg.linebreak.line_break` | `line_break(c: str, /) -> LineBreak` | the UAX #14 `Line_Break` class of one code point (`XX` for unassigned); the per-boundary read the `BREAK` arm maps into `BreakClass` |
|  [02]   | `uniseg.linebreak.line_break_breakables` | `line_break_breakables(s, /, *, property=line_break) -> Breakables` | the full LB1–LB31 0/1 break-opportunity stream BEFORE each position; the algorithm core |
|  [03]   | `uniseg.linebreak.line_break_boundaries` | `line_break_boundaries(s, /, *, property=line_break, tailor=None) -> Iterator[int]` | the legal line-break indices (first > 0 through `len(s)`); count == number of line-break units |
|  [04]   | `uniseg.linebreak.line_break_units` | `line_break_units(s, /, *, property=line_break, tailor=None) -> Iterator[str]` | the line-break-unit token iterator (each token ends at a legal break, trailing spaces kept on the token); the `BREAK` arm's segment source |
|  [05]   | `uniseg.graphemecluster.grapheme_cluster_break` | `grapheme_cluster_break(c, /) -> GraphemeClusterBreak` | the UAX #29 `Grapheme_Cluster_Break` class (`Other` default) |
|  [06]   | `uniseg.graphemecluster.grapheme_cluster_breakables` | `grapheme_cluster_breakables(s, /, *, property=grapheme_cluster_break) -> Breakables` | the GB1–GB13 extended-cluster 0/1 stream (Indic conjunct via `indic_conjunct_break`, emoji ZWJ sequences via `extended_pictographic`, regional-indicator pairs) |
|  [07]   | `uniseg.graphemecluster.grapheme_cluster_boundaries` | `grapheme_cluster_boundaries(s, /, *, property=…, tailor=None) -> Iterator[int]` | the grapheme-cluster boundary indices (0 through `len(s)`) |
|  [08]   | `uniseg.graphemecluster.grapheme_clusters` | `grapheme_clusters(s, /, *, property=…, tailor=None) -> Iterator[str]` | the user-perceived-character token iterator; the unit `tt_text_extents` widths and the shaped-run cluster reconciliation count |
|  [09]   | `uniseg.wordbreak.word_break` | `word_break(c, /) -> WordBreak` | the UAX #29 `Word_Break` class (`Other` default) |
|  [10]   | `uniseg.wordbreak.word_breakables` | `word_breakables(s, /, *, property=word_break) -> Breakables` | the WB1–WB999 0/1 word-break stream |
|  [11]   | `uniseg.wordbreak.word_boundaries` | `word_boundaries(s, /, *, property=…, tailor=None) -> Iterator[int]` | the word-boundary indices |
|  [12]   | `uniseg.wordbreak.words` | `words(s, /, *, property=…, tailor=None) -> Iterator[str]` | the user-perceived-word token iterator (punctuation/space emitted as their own tokens) |
|  [13]   | `uniseg.sentencebreak.sentence_break` | `sentence_break(c, /) -> SentenceBreak` | the UAX #29 `Sentence_Break` class (`Other` default) |
|  [14]   | `uniseg.sentencebreak.sentence_breakables` | `sentence_breakables(s, /, *, property=sentence_break) -> Breakables` | the SB1–SB998 0/1 sentence-break stream |
|  [15]   | `uniseg.sentencebreak.sentence_boundaries` | `sentence_boundaries(s, /, *, property=…, tailor=None) -> Iterator[int]` | the sentence-boundary indices |
|  [16]   | `uniseg.sentencebreak.sentences` | `sentences(s, /, *, property=…, tailor=None) -> Iterator[str]` | the sentence token iterator |

[ENTRYPOINT_SCOPE]: breakable-table primitives and the `Run` walk
- rail: typography

`boundaries` and `break_units` are the table-to-output primitives every family's `…_boundaries`/`…_units` are built from — call them directly to convert a CUSTOM (tailored) breakable stream into indices or tokens without re-running an algorithm. `Run` is the tailorable per-code-point walk for authoring a wholly custom break algorithm (the substrate uniseg's own four engines use); a design composing uniseg should reach for `tailor=` first and `Run` only for a genuinely new algorithm.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
|  [01]   | `uniseg.breaking.boundaries` | `boundaries(breakables: Breakables, /) -> Iterator[int]` | fold a 0/1 stream into boundary indices (yields each 1-position, then the terminal `len`); the index source `document/emit`/`composition/compose` slice on |
|  [02]   | `uniseg.breaking.break_units` | `break_units(s: str, breakables: Breakables, /) -> Iterator[str]` | fold a string + its 0/1 stream into the token slices between breaks; `len(s) == len(breakables)` required |
|  [03]   | `uniseg.breaking.Run` | `Run(text, func=…)`; `.walk(offset=1)`/`.head()`/`.is_following(attrs, greedy=)`/`.is_leading(attrs)`/`.break_here()`/`.do_not_break_here()`/`.set_skip_table(it)`/`.set_default(Breakable)`/`.literal_breakables(default=Breakable.Break)` | the rule-engine substrate: per-position attribute/codepoint lookahead-behind with a skip table and a deferred-resolution breakable array; only for authoring a new algorithm, never for re-deriving a UAX rule uniseg already owns |

[ENTRYPOINT_SCOPE]: fixed-width east-asian wrapping and width
- rail: typography

`tt_wrap` is the one-call monospace wrap (terminal/code-listing/fixed-pitch column); `tt_width`/`tt_text_extents` are the standalone width probes (`1` halfwidth / `2` fullwidth, ambiguous resolved by policy). These ride the interpreter's UCD `east_asian_width` (see [04] data-split), not uniseg's 16.0.0 tables.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
|  [01]   | `uniseg.wrap.tt_wrap` | `tt_wrap(s, /, wrap_width, *, tab_width=8, tab_char=' ', ambiguous_as_wide=False, cur=0, offset=0, iter_breakables=line_break_breakables) -> Iterator[str]` | wrap to a fixed character-cell width using UAX #14 opportunities and east-asian width; tab expansion + ambiguous-width policy + first-line `cur` indent + per-line `offset` |
|  [02]   | `uniseg.wrap.tt_width` | `tt_width(s, /, index=0, *, ambiguous_as_wide=False) -> int` | the cell width (`1`/`2`) of the grapheme cluster at `s[index]`, keyed on its leading code point's `East_Asian_Width` |
|  [03]   | `uniseg.wrap.tt_text_extents` | `tt_text_extents(s, /, *, ambiguous_as_wide=False) -> list[int]` | the cumulative cell-width prefix array over `grapheme_clusters(s)` (one entry per code point); the monospace measure for column geometry |
|  [04]   | `uniseg.wrap.wrap` | `wrap(formatter, s, /, cur=0, offset=0, *, iter_breakables=line_break_breakables) -> int` | module-level `Wrapper.wrap` over a shared engine; drive a custom `Formatter` (e.g. shaped-advance widths) for proportional fixed-opportunity wrap |

[ENTRYPOINT_SCOPE]: UCD property re-exports and derived/emoji predicates
- rail: typography

`uniseg.unicodedatawrapper` re-exports the stdlib `unicodedata` surface plus the enum-typed `category`/`east_asian_width`; `uniseg.derived` and `uniseg.emoji` add the UAX #44 / UTS #51 boolean predicates the break algorithms consume internally and a design can read directly for character classification.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
|  [01]   | `uniseg.unicodedatawrapper.category` | `category(chr, /) -> Category` | enum-typed `General_Category` (vs the stdlib's raw 2-char string) |
|  [02]   | `uniseg.unicodedatawrapper.east_asian_width` | `east_asian_width(chr, /) -> EastAsianWidth` | enum-typed `East_Asian_Width` |
|  [03]   | `uniseg.unicodedatawrapper.{bidirectional,combining,mirrored,decimal,digit,numeric,decomposition,name,lookup,normalize,is_normalized,unidata_version,ucd_3_2_0}` | stdlib `unicodedata` signatures | the raw UCD re-export surface (Bidi_Class string, combining class, mirrored flag, numeric/decimal/digit, NFC/NFD `normalize`, name/lookup) — interpreter UCD, the same call `python-bidi` reorder context can read |
|  [04]   | `uniseg.derived.{alphabetic,math,lowercase,uppercase,cased,case_ignorable,id_start,id_continue,xid_start,xid_continue,default_ignorable_code_point,grapheme_extend,grapheme_base,changes_when_*}` | `(c: str, /) -> bool` | the UAX #44 derived boolean predicates (16.0.0-pinned via the bundled tables) |
|  [05]   | `uniseg.derived.indic_conjunct_break` | `indic_conjunct_break(c, /) -> IndicConjunctBreak` | the `Indic_Conjunct_Break` derived value the GB9c grapheme rule consumes |
|  [06]   | `uniseg.emoji.{emoji,emoji_presentation,emoji_modifier_base,emoji_component,extended_pictographic}` | `(c: str, /) -> bool` | the UTS #51 emoji predicates (`extended_pictographic` drives the GB11/WB3c emoji-ZWJ break suppression); 16.0.0-pinned |

## [04]-[IMPLEMENTATION_LAW]

[SEGMENTATION_FAMILY]:
- uniform-shape axis: every algorithm is the four-function `break`/`breakables`/`boundaries`/`units` shape; a design selects the GRANULARITY (grapheme < word < line-unit < sentence) by choosing the family, never by re-implementing a coarser segmentation on top of a finer one. The `breakables` 0/1 stream is the canonical intermediate — `boundaries`/`break_units` derive indices and tokens from it — so a design that needs BOTH the indices and the tokens computes `…_breakables(s)` once and folds it through both primitives rather than calling two algorithm passes.
- str-enum-identity axis: the break enums are `EnumProperty(str, Enum)`, so a break-class read is directly `str`-comparable to its UCD code and `in`-testable against a `frozenset[str]` of codes — the `typography/layout#LAYOUT` `_MANDATORY` membership test `line_break(char) in {"BK","CR","LF","NL"}` is correct ONLY because of this identity. A design must compare against the UCD-code strings (`member.value`), never against `str(member)` (which is the member NAME), and never re-wrap the enum in a parallel `StrEnum`.
- tailoring axis: locale-specific break overrides go through the `tailor: TailorFunction` keyword (receives `(s, default_breakables)`, returns a customized stream) or the `property: PropertyFunction` keyword (a legacy/remapped class read, e.g. the `line_break_legacy` AI->ID mapping in the upstream doctest) — NEVER a fork of the algorithm. A new break behavior is one `tailor`/`property` callable; a wholly new algorithm is a `Run`-based fold; in both cases uniseg's UAX rule engine stays the substrate.
- data-pin axis (LOAD-BEARING SPLIT): the four segmentation algorithms + the derived/emoji predicates read uniseg's BUNDLED Unicode 16.0.0 tables (`db_lookups.py` via `get_handle`/`get_value`), so break results are deterministic and interpreter-independent — `uniseg.unidata_version == "16.0.0"`. BUT `category`/`east_asian_width`/`bidirectional` (and the rest of `unicodedatawrapper`) delegate to stdlib `unicodedata` when the optional `unicodedata2` is absent (it is, here), so they follow the INTERPRETER's UCD (17.0.0 on this cp315 build). Consequence: `tt_width`/`tt_text_extents`/`tt_wrap` (which key on `east_asian_width`) ride 17.0.0 width data while `line_break_units` rides 16.0.0 break data. A design must treat the segmentation result as 16.0.0-pinned and the east-asian-width measure as interpreter-pinned, and must not assume a single UCD version spans both; admitting `unicodedata2` later would realign `category`/`east_asian_width` to its pinned version, removing the split.
- evidence axis: the `typography/layout#LAYOUT` `BREAK` arm folds the resolved `BreakClass` opportunity stream into an `ArtifactReceipt.Document` content-key derivation (the encoded break-opportunity byte count); the break-unit count, the mandatory-vs-opportunity split, and the bundled `unidata_version` stay interior evidence the arm folds into the content key, never new receipt fields. A segmentation pass is pure and offline (no IO, no native call, no event-loop hop) — it needs no `runtime` worker seam, unlike the `python-bidi` reorder it sits beside.

[STACKING]:
- layout-arm seam (primary): the `typography/layout#LAYOUT` `LineLayout` owner reaches uniseg as the `BREAK` `LayoutOp` arm — `line_break_units(text)` segments the source into break-unit tokens and `line_break(unit[-1])` reads the trailing code point's class, projecting each inter-token gap into a `BreakClass` row (`MANDATORY` when the class is in `_MANDATORY = {"BK","CR","LF","NL"}`, else `OPPORTUNITY`, `PROHIBITED` inside a no-break cluster). That `BreakClass` stream threads the hand-rolled Knuth-Plass `Item` Box/Glue/Penalty algebra (`.api/uniseg.md` is the design's cited admission for the `BREAK` arm) — uniseg owns the UAX #14 opportunity resolution; the total-fit demerit dynamic program on top is the design's own, NOT a uniseg re-export. The rejected duplicate is a hand-rolled break-class table beside `line_break`; the rejected lower-capability form is a greedy first-fit wrap (`tt_wrap`) where the paragraph owner needs the total-fit optimum — `tt_wrap` serves the monospace/terminal medium, the Knuth-Plass DP serves the proportional document page.
- shape-cluster seam: `typography/shape#SHAPE` shapes a run into a `PositionedGlyphRun` whose per-glyph `cluster` indices are byte/codepoint offsets HarfBuzz assigns under `BufferClusterLevel.MONOTONE_GRAPHEMES` (`.api/uharfbuzz.md`). `grapheme_clusters(text)`/`grapheme_cluster_boundaries(text)` provide the UAX #29 extended-cluster boundaries the design reconciles those HarfBuzz clusters against (a user-perceived character = one extended grapheme cluster = one or more glyphs), so cursor placement, selection, and per-cluster width attribution land on grapheme boundaries rather than mid-cluster code points. uniseg owns the extended-cluster definition (Indic conjunct, emoji ZWJ, regional-indicator pairs) HarfBuzz's monotone clustering approximates but does not canonicalize.
- table-measure seam: `visualization/table#TABLE` lowers schedule/QTO frames through `great-tables` (`.api/great-tables.md`); where a fixed-pitch or CJK-width-correct column measure is needed (a monospace code/data column, an east-asian glyph census), `tt_text_extents(cell)`/`tt_width(cell)` give the cell-width prefix the column geometry sums — `grapheme_clusters` as the iteration unit so a combining-mark sequence counts as one cell, not several. The east-asian-width axis is interpreter-UCD-pinned (data-pin axis above); the table owner reads it as a width measure, never as a segmentation authority.
- universal-tier rails: the design-side break-policy and layout-knob carriers are frozen `msgspec.Struct` rows (`libs/python/.api/msgspec.md`) — the `BreakClass` `StrEnum` and the `LayoutParams` field set spread over the uniseg knobs (`language`/break thresholds), the resolved opportunity stream encoded through one module-level `msgspec.msgpack.Encoder` into the content-key fact; `beartype` (`libs/python/.api/beartype.md`) validates the boundary `str`/measure inputs; every `BREAK`/`HYPHENATE`/`PARAGRAPH` arm returns the same `expression`-`Result`/`RuntimeRail[ContentKey]` rail (`libs/python/.api/expression.md`) the rest of the `LayoutOp` family returns, keyed by the runtime content key. Because a uniseg pass is pure and offline it rides no `anyio`/`runtime` worker seam (unlike `python-bidi`'s gated `to_process` reorder) — it folds inline in the `BREAK` acceptor and contributes one `ArtifactReceipt.Document`.

[LOCAL_ADMISSION]:
- the UAX #14 break-opportunity read is `uniseg.linebreak.line_break(char)` / `line_break_units(text)`, never a hand-rolled break-class table nor a regex word-split — uniseg owns the full LB1–LB31 rule set including the Brahmic/Aksara (`AK`/`AP`/`AS`/`VF`/`VI`), emoji (`EB`/`EM`), regional-indicator, and East-Asian-context quotation rules a hand-rolled table silently drops.
- the user-perceived-character unit is `uniseg.graphemecluster.grapheme_clusters(text)` (the UAX #29 EXTENDED cluster), never `list(text)` code-point iteration nor `str` indexing — a combining sequence, an Indic conjunct, an emoji ZWJ sequence, and a regional-indicator flag pair are each one cluster, and cursor/selection/width attribution must land on those boundaries.
- locale-specific break overrides are a `tailor: TailorFunction` or `property: PropertyFunction` callable on the existing entry, never a forked algorithm or a parallel break owner; a new algorithm (if ever needed) is a `Run`-based fold, never a re-derivation of a UAX rule uniseg already provides.
- the break enums are compared against their UCD-code strings (`member.value` / the `str`-enum identity), never against `str(member)` (the member name) — the `_MANDATORY` membership test depends on `LineBreak.BK == "BK"`, which holds because `EnumProperty` derives `str, Enum`.
- the fixed-width wrap is `tt_wrap`/`TTFormatter` for the monospace/terminal medium ONLY; the proportional document paragraph uses the `typography/layout#LAYOUT` Knuth-Plass total-fit DP fed by `line_break_units`, never `tt_wrap`'s greedy first-fit — the two are different media, not interchangeable.
- the segmentation result is treated as Unicode 16.0.0-pinned (the bundled tables) and the `east_asian_width`/`category` measure as interpreter-UCD-pinned (the stdlib fallback) — a design must not assume one UCD version spans both, and must not hand-vendor segmentation tables to "match" the interpreter when uniseg's pinned 16.0.0 is the deterministic contract.

[RAIL_LAW]:
- Package: `uniseg`
- Owns: UAX #29 extended grapheme-cluster / word / sentence segmentation and UAX #14 line-break-opportunity resolution (the four-function `break`/`breakables`/`boundaries`/`units` family per algorithm, the tailorable `Run`/`Breakable` breakable-table substrate, the `boundaries`/`break_units` table primitives), fixed-width east-asian-aware wrapping (`tt_wrap`/`tt_width`/`tt_text_extents`/`TTFormatter`/`Wrapper`/`Formatter`), and the UCD General_Category / East_Asian_Width / Bidi_Class re-exports plus the UAX #44 derived-property and UTS #51 emoji predicates — bundling Unicode 16.0.0 segmentation tables in-package
- Accept: the `BREAK` `LayoutOp` arm on `typography/layout#LAYOUT` `LineLayout` (`line_break`/`line_break_units` -> `BreakClass` stream feeding the Knuth-Plass DP); the `grapheme_clusters` extended-cluster boundaries reconciling `typography/shape#SHAPE` HarfBuzz cluster counts and feeding `tt_text_extents`; the `tt_width`/`tt_text_extents` monospace/CJK column measure on `visualization/table#TABLE`; the `tailor`/`property` keyword as the locale-override seam
- Reject: a hand-rolled UAX #14 break-class table where `line_break` owns the LB1–LB31 fold; `list(text)` code-point iteration where `grapheme_clusters` owns the extended cluster; a forked break algorithm where `tailor`/`property` (or a `Run` fold) suffices; `tt_wrap` greedy first-fit where the document paragraph needs the total-fit Knuth-Plass optimum; comparing a break enum against `str(member)` (the name) instead of its UCD code; assuming a single UCD version spans both the 16.0.0-pinned segmentation tables and the interpreter-pinned `east_asian_width`/`category` re-exports
