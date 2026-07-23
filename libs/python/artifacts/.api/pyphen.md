# [PY_ARTIFACTS_API_PYPHEN]

`pyphen` owns pure-Python soft-hyphenation for the artifacts typography rail: a `Pyphen` dictionary parses one Hunspell `hyph_<lang>.dic` TeX-pattern file and resolves every legal in-word break through the Liang competing odd/even pattern-weight fold, each break a `DataInt` whose integer value is the offset and whose `.data` carries the non-standard orthographic `(change, index, cut)` mutation.

`positions`/`iterate` feed the `typography/layout#LAYOUT` owner's flagged Knuth-Plass `Penalty` items; pyphen resolves in-word breaks alone and never re-derives the Liang algorithm, `.dic` grammar, or substitution rule.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyphen`
- package: `pyphen` (tri-license `GPL-2.0+ OR LGPL-2.1+ OR MPL-1.1`, libhyphen heritage; the LGPL/MPL arms make in-process linking commercial-safe)
- module: `pyphen` (single flat module; `pyphen.dictionaries` is the bundled `.dic` data package, not an import surface)
- owner: `artifacts`
- rail: typography
- asset: pure-Python universal wheel, stdlib-only (`re`, `importlib.resources`, `pathlib`), zero runtime dependencies
- dictionaries: bundles the LibreOffice `hyph_<lang>.dic` corpus indexed into `LANGUAGES` under each full and truncated code; `filename=` loads a non-bundled `.dic`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the hyphenation dictionary and its break-carrier types

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]                | [CAPABILITY]                                        |
| :-----: | :------------------------- | :--------------------------- | :-------------------------------------------------- |
|  [01]   | `pyphen.Pyphen`            | hyphenation dictionary       | configurable dictionary with cached split iteration |
|  [02]   | `pyphen.DataInt`           | break-offset carrier         | offset plus orthographic mutation metadata          |
|  [03]   | `pyphen.HyphDict`          | parsed pattern table         | competing-pattern table with per-word memo          |
|  [04]   | `pyphen.AlternativeParser` | non-standard-pattern factory | tags odd weights with substitution tuples           |

- `pyphen.DataInt`: `int` subclass; the value is the in-word break offset, `.data` is `None` for a standard break or a `(change, index, cut)` tuple — `change` an `'old=new'` substitution string (upper-cased when the word is all-caps), `index` the offset the substitution applies at, `cut` the source characters it replaces.
- `pyphen.HyphDict`: reached only as `Pyphen(...).hd`; its `positions(word)` is the untrimmed competing-pattern result `Pyphen.positions` affix-trims, memoized in `.cache`. `AlternativeParser` tags each odd (break) weight with the `(change, index, cut)` tuple, the mechanism behind every non-`None` `DataInt.data`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `Pyphen` construction and read surface
- knobs: `left`/`right` default `2` (minimum characters before/after any break, the orphan/widow guards), `cache` default `True`; `iterate` is aliased to `__call__`, so `dict(word)` == `dict.iterate(word)`

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `Pyphen(*, filename, lang, left, right, cache)`  | ctor     | resolve file/language, affix guards, and cache       |
|  [02]   | `positions(word) -> list[DataInt]`               | instance | trimmed in-word offsets with mutation data           |
|  [03]   | `iterate(word) -> Iterator[tuple[str, str]]`     | instance | longest-first substituted `(head, tail)` split pairs |
|  [04]   | `inserted(word, hyphen='-') -> str`              | instance | splice every break into one string                   |
|  [05]   | `wrap(word, width, hyphen='-') -> tuple \| None` | instance | the longest first split fitting `width`              |

- `positions`: lower-cases the word, trims to `self.left <= i <= len(word) - self.right`, memoizes per word. E.g. `positions('hyphenation') -> [2, 6]`; Hungarian `positions('asszonnyal') -> [2, 6]` with `.data` `('sz=', -1, 1)` / `('ny=ny', -1, 3)`; `iterate('hyphenation') -> [('hyphen','ation'), ('hy','phenation')]`.

[ENTRYPOINT_SCOPE]: the module-level language index and fallback
- [BINDING_ID]: `pyphen.VERSION` `pyphen.__version__`

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `pyphen.LANGUAGES`                           | static  | maps full and short codes to `.dic` paths  |
|  [02]   | `language_fallback(language) -> str \| None` | static  | truncate locale subtags to a bundled match |
|  [03]   | `pyphen.LANGUAGES_LOWERCASE`                 | static  | maps lowercase codes to canonical codes    |
|  [04]   | `pyphen.hdcache`                             | static  | shares parsed patterns by dictionary path  |

- `language_fallback`: normalizes `-` → `_`, lower-cases, then pops trailing subtags until a bundled code matches (`en_Latn_US` → `en`, `nl_BE` → `nl`), returning `None` when unbundled. `Pyphen(lang=)` resolves through it and raises `KeyError` on no match with no `filename=`, so a caller probes `LANGUAGES` first.
- `hdcache`: populated on first construction for a path and reused by every later instance — even `cache=False` re-parses but writes through — so repeated `Pyphen(lang=...)` in a hot layout loop is cheap.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Pyphen` dictionary folds every word through the Liang competing odd/even pattern-weight algorithm over parsed Hunspell `.dic` patterns; `positions` is the canonical surface, affix-trimmed and memoized in `HyphDict.cache`.
- Each break is a `DataInt`: the integer offset feeds the layout `Penalty` position while `.data` carries the `(change, index, cut)` orthographic mutation for non-standard hyphenation.

[STACKING]:
- `uniseg`(`.api/uniseg.md`): pyphen resolves only in-word soft-break offsets over each `words(text)` token uniseg segments; uniseg owns the UAX#14 hard/opportunity line breaks, both halves feeding the one `Item` stream.
- `expression`(`.api/expression.md`): each `positions` offset folds into the layout owner's closed `Item` `tagged_union` as `Item.of_penalty(hyphen_penalty, hyphen_advance, flagged=True)`, and the `KeyError`-raising `Pyphen(lang=)` construction lifts through `boundary(...)` onto a `RuntimeRail`, a missing dictionary surfacing as a typed fault.
- `msgspec`(`.api/msgspec.md`): the layout owner encodes each break as a `HyphenBreak` row through its msgpack `Encoder`; `DataInt` crosses as a plain `int` while `iterate`'s substituted split pairs carry the mutation into `HyphenBreak.head`/`tail`.
- `beartype`(`.api/beartype.md`): the `positions(word) -> list[int]` boundary type-checks cleanly because `DataInt` is an `int` subtype.
- `typography/layout#LAYOUT`: its `HYPHENATE` arm zips each token's `.positions(word)` against its reversed `.iterate(word)` split pairs into absolute grapheme-filtered `HyphenBreak` rows — the `HyphenSpec.left_min`/`right_min` fields ARE the `Pyphen` `left`/`right` knobs; its `PARAGRAPH` arm keys the rows by position into a `frozendict` the total-fit program charges `FitPolicy.flagged_demerit` for consecutive hyphenated lines. `DataInt.data` is the growth seam for orthography-correct emission.

[LOCAL_ADMISSION]:
- `import pyphen` and construct `Pyphen(lang=, left=, right=)` at boundary scope; the `KeyError` on an unbundled `lang` with no `filename=` is trapped by the owner's `boundary`, never raised across the seam.
- Probe `LANGUAGES` before construction, or pass `filename=` for a non-bundled `.dic`.

[RAIL_LAW]:
- Package: `pyphen`
- Owns: dictionary-driven soft-hyphenation over Hunspell `.dic` TeX patterns — Liang competing-pattern break resolution with `left`/`right` affix guards, non-standard orthographic substitution surfaced per break, and BCP-47 truncation fallback
- Accept: `Pyphen(lang=, left=, right=).positions(word)` for the raw offset surface and `.iterate(word)` for the substituted split pairs the layout owner threads into `Penalty` items
- Reject: `inserted` string-splicing or `wrap` per-word first-fit where the total-fit DP needs raw offsets; a hand-rolled Liang implementation, `.dic` parser, or substitution rule where pyphen owns the fold; `pyphen.version` where the module attributes are `VERSION`/`__version__`
