# [PY_ARTIFACTS_API_PYICU]

`PyICU` (import `icu`) binds ICU4C as the artifacts locale-aware text layer — CLDR-tailored dictionary-backed segmentation, the complete UAX#9 bidirectional engine, CLDR collation with sort-key and alphabetic-index bucketing, UAX#15 normalization, script itemisation, and transliteration — over the `Locale`/`UnicodeString`/`UnicodeSet` substrate. It stacks UNDER `uharfbuzz` shaping as the locale/CLDR upgrade of the locale-free default `uniseg` (UAX#14) and `python-bidi` (UAX#9 reorder) own, and never re-implements the UCA, UAX#9/#14/#15, or CLDR-segmentation cores the ICU4C C++ owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PyICU`
- package: `PyICU`
- module: `icu` (the `icu` package re-exports the `_icu` C++ extension: `from _icu import *`)
- owner: `artifacts`
- rail: text-locale
- license: `MIT`; the dynamically-linked ICU4C libraries carry the permissive ICU/Unicode license, no copyleft obligation
- build: sdist-only C++ extension over ICU4C (`icu-i18n`/`icu-uc`/`icu-io` via `pkg-config`, C++11), absent on the current interpreter so the ICU arm dispatches on the gated subprocess seam
- entry points: none; the `_icu` extension is import-only

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: locale, string, and set substrate

`Locale` keys every operation; text crosses as a Python `str` (ICU's `UnicodeString` auto-converts), and `UnicodeSet` is the character-class value the break, collator, and transliterator surfaces accept.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]         | [CAPABILITY]                               |
| :-----: | :------------------------ | :-------------------- | :----------------------------------------- |
|  [01]   | `Locale`                  | locale value          | CLDR keys and language-tag transforms      |
|  [02]   | `UnicodeString`           | mutable UTF-16 string | boundary conversion and non-UTF-8 decoding |
|  [03]   | `UnicodeSet`              | character class       | ICU-pattern algebra and frozen membership  |
|  [04]   | `StringCharacterIterator` | text cursor           | codepoint iteration for script itemisation |
|  [05]   | `CharacterIterator`       | cursor base           | shared text-iteration contract             |
|  [06]   | `ICUError`                | error rail            | raw ICU failure-code access                |
|  [07]   | `ICU_VERSION`             | version probe         | linked ICU library version                 |
|  [08]   | `UNICODE_VERSION`         | version probe         | linked Unicode data version                |
|  [09]   | `ICU_MAX_MAJOR_VERSION`   | version probe         | ICU major-version ceiling                  |

[PUBLIC_TYPE_SCOPE]: segmentation, bidi, and collation engine objects

Segmentation, bidi, and collation engine objects, each constructed from a `Locale`.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                  |
| :-----: | :----------------------- | :------------------- | :-------------------------------------------- |
|  [01]   | `BreakIterator`          | segmentation factory | locale-tailored dictionary iterator           |
|  [02]   | `RuleBasedBreakIterator` | concrete iterator    | custom and precompiled rule execution         |
|  [03]   | `Bidi`                   | UAX#9 bidi engine    | paragraph and line reordering with index maps |
|  [04]   | `BidiTransform`          | bidi reshape+reorder | one-shot reorder and Arabic shaping           |
|  [05]   | `Collator`               | collation factory    | locale-tailored comparator construction       |
|  [06]   | `RuleBasedCollator`      | CLDR collation       | custom and binary collation rules             |
|  [07]   | `CollationKey`           | comparable sort key  | stored ordering key for repeated comparison   |
|  [08]   | `AlphabeticIndex`        | index bucketer       | mutable locale-script record buckets          |
|  [09]   | `ImmutableIndex`         | immutable bucketer   | frozen locale-script bucket lookup            |

[PUBLIC_TYPE_SCOPE]: normalization, script, transliteration, and case objects

Normalization, script, transliteration, case, property, search, and confusable-detection objects.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]        | [CAPABILITY]                                  |
| :-----: | :-------------------- | :------------------- | :-------------------------------------------- |
|  [01]   | `Normalizer2`         | UAX#15 normalizer    | standard and custom normalization modes       |
|  [02]   | `FilteredNormalizer2` | filtered normalizer  | UnicodeSet-scoped normalization               |
|  [03]   | `Script`              | script itemisation   | codepoint script and direction queries        |
|  [04]   | `UScriptCode`         | script enum          | script identity vocabulary                    |
|  [05]   | `Transliterator`      | rule-based transform | built-in and Python-subclass conversion rules |
|  [06]   | `CaseMap`             | locale case mapping  | locale-correct lower, upper, title, and fold  |
|  [07]   | `Edits`               | change-span map      | fine and coarse casing-change runs            |
|  [08]   | `Char`                | character properties | ICU character-property queries                |
|  [09]   | `StringSearch`        | locale search        | collation-aware substring matching            |
|  [10]   | `SearchIterator`      | search cursor        | grapheme-boundary match traversal             |
|  [11]   | `SpoofChecker`        | confusable detection | mixed-script and restriction checks           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: locale-tailored line/word segmentation

`BreakIterator.createLineInstance(locale)` returns a `RuleBasedBreakIterator` carrying the locale tailoring and the bundled Thai/Lao/Khmer/Burmese/Japanese/Chinese dictionary, so a spaceless script breaks at lexical boundaries `uniseg`'s default table cannot reach; walk `first`/`next` (or `following`/`nextBoundary`) collecting offsets until `DONE`, and `getRuleStatus()` tags each break so the Knuth-Plass `Penalty` weights a CJK break apart from a Latin space.

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :---------------------------------- |
|  [01]   | `BreakIterator.createLineInstance(locale)`   | tailored dictionary line boundaries |
|  [02]   | `BreakIterator.createWordInstance(locale)`   | locale-tailored word boundaries     |
|  [03]   | `BreakIterator.createCharacterInstance(loc)` | grapheme-cluster boundaries         |
|  [04]   | `BreakIterator.createSentenceInstance(loc)`  | locale-tailored sentence boundaries |
|  [05]   | `BreakIterator.createTitleInstance(locale)`  | title-case boundaries               |
|  [06]   | `bi.setText(text)`                           | load the segmented run              |
|  [07]   | `bi.getText()`                               | return the retained run             |
|  [08]   | `bi.getRuleStatus()`                         | `UWordBreak` or line-rule status    |
|  [09]   | `bi.getBinaryRules()`                        | serialize precompiled rules         |
|  [10]   | `RuleBasedBreakIterator(binary_rules)`       | reload precompiled rules            |
|  [11]   | `RuleBasedBreakIterator(rule_string)`        | custom RBBI grammar                 |

- `bi` walk: `first()` `next()` `previous()` `last()` `current()` `following(offset)` `preceding(offset)` `isBoundary(offset)` `nextBoundary()` — boundary offsets; `next()` returns `BreakIterator.DONE` past the end.

[ENTRYPOINT_SCOPE]: bidirectional reordering and reshaping

`Bidi.setPara` runs the full UAX#9 resolution — explicit embeddings/overrides/isolates, weak/neutral/implicit types, the per-character level array, and the visual-run decomposition; `getVisualRun(i) -> (logical_start, length, level)` is the single-direction partition the shaper itemises by, and `setLine(start, limit)` re-runs reordering line-relative after the paragraph levels are known, which `python-bidi`'s whole-string `get_display` cannot give. `BidiTransform().transform` is the one-shot reorder + Arabic-join + digit-shape across the `(level, order)` endpoints.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `Bidi()`                                                                           | default bidi object               |
|  [02]   | `Bidi(max_length, max_run_count)`                                                  | pre-sized paragraph storage       |
|  [03]   | `Bidi.setPara(text, para_level=DEFAULT_LTR, levels=None)`                          | UAX#9 paragraph resolution        |
|  [04]   | `Bidi.getDirection()`                                                              | `LTR`/`RTL`/`MIXED`/`NEUTRAL`     |
|  [05]   | `Bidi.countRuns()`                                                                 | directional-run count             |
|  [06]   | `Bidi.getVisualRun(i)`                                                             | `(logical_start, length, level)`  |
|  [07]   | `Bidi.setLine(start, limit)`                                                       | post-break line-relative ordering |
|  [08]   | `Bidi.writeReordered(options)`                                                     | visual text with reorder controls |
|  [09]   | `BidiTransform().transform(text, in_lvl, in_ord, out_lvl, out_ord, mirror, shape)` | reorder and reshape               |

- `Bidi` levels and maps: `getParaLevel()` `getLevelAt(i)` `getLevels()` `getVisualIndex(logical)` `getLogicalIndex(visual)` `getLogicalMap()` `getVisualMap()` `invertMap()` `reorderVisual(levels)` `reorderLogical(levels)` `countParagraphs()` `getParagraph()` `getParagraphByIndex()` `orderParagraphsLTR()` `setReorderingMode(mode)` `setReorderingOptions(options)`.

[ENTRYPOINT_SCOPE]: collation, indexing, and normalization

`Collator.createInstance(locale)` is the CLDR collation entry; `compare(a, b)` is the one-shot comparator and `getSortKey(s) -> bytes` the stable key a stored sort index holds — sort once with the key, use `compare`/`CollationKey` only when one string matches many. `setAttribute(UCollAttribute.*, UCollAttributeValue.*)` tailors strength (`PRIMARY`..`IDENTICAL`), case-first, normalization, and alternate-handling. `Normalizer2.getNFCInstance().normalize(s)` is the canonical-composition pre-pass before `uharfbuzz`.

| [INDEX] | [SURFACE]                           | [CAPABILITY]                           |
| :-----: | :---------------------------------- | :------------------------------------- |
|  [01]   | `Collator.createInstance(locale)`   | CLDR-tailored locale collator          |
|  [02]   | `collator.compare(a, b)`            | locale-correct three-way ordering      |
|  [03]   | `collator.getSortKey(s)`            | stable binary corpus-sort key          |
|  [04]   | `collator.getCollationKey(s)`       | `CollationKey` for repeated comparison |
|  [05]   | `collator.setAttribute(attr, val)`  | UCA attribute-matrix selection         |
|  [06]   | `collator.setStrength(level)`       | collation-strength selection           |
|  [07]   | `collator.getRules()`               | recover custom collation rules         |
|  [08]   | `collator.cloneBinary()`            | serialize compiled collation rules     |
|  [09]   | `RuleBasedCollator(binary, root)`   | load precompiled collation rules       |
|  [10]   | `RuleBasedCollator(rules)`          | compile custom collation rules         |
|  [11]   | `AlphabeticIndex(locale)`           | locale-script glossary buckets         |
|  [12]   | `Normalizer2.getNFCInstance()`      | five standard normalization singletons |
|  [13]   | `norm2.normalize(s)`                | normalization and quick-check path     |
|  [14]   | `Script.getScript(cp)`              | font-fallback codepoint script         |
|  [15]   | `Transliterator.createInstance(id)` | built-in or custom script conversion   |

[ENTRYPOINT_SCOPE]: substrate construction, casing, properties, search, and safety
- `Locale`: `Locale(name)` `getEnglish()` `getUS()` `getRoot()` `getJapanese()` `getName()` `getLanguage()` `getScript()` `getDisplayName()` `forLanguageTag(tag)` `toLanguageTag()` `addLikelySubtags()` `minimizeSubtags()` `setKeywordValue()` `createKeywords()` `canonicalize()`.
- `UnicodeString`: `UnicodeString(str)` `UnicodeString(bytes, encoding)` `countChar32()` `char32At(i)`; `[]`/`[:]`/`+=` in-place mutation.
- `UnicodeSet`: `UnicodeSet(pattern)` `contains()` `add()` `addAll()` `retainAll()` `complement()` `freeze()` `UnicodeSetIterator`.
- `StringCharacterIterator`: `first()` `next()` `previous()` `next32PostInc()` `hasNext()`.
- `AlphabeticIndex` / `ImmutableIndex`: `addLabels(locale)` `addRecord(name, data)` `getBucketLabel()` `nextBucket()` `nextRecord()` `recordData()` `buildImmutableIndex()`.
- `Normalizer2`: `getNFDInstance()` `getNFKCInstance()` `getNFKDInstance()` `getNFKCCasefoldInstance()` `getInstance(data, name, mode)` `isNormalized()` `quickCheck()` `spanQuickCheckYes()` `hasBoundaryBefore()` `hasBoundaryAfter()` `isInert()` `append()` `normalizeSecondAndAppend()`.
- `FilteredNormalizer2`: `FilteredNormalizer2(norm2, unicode_set)`.
- `Script`: `getScriptExtensions(cp)` `getShortName()` `getScriptCode()` `isRightToLeft()` `getSampleString()`.
- `Transliterator`: `createFromRules(id, rules, dir)` `transliterate(s)` `createInverse()` `toRules(escaped)` `getAvailableIDs()` `registerInstance(t)` `handleTransliterate(text, pos, incremental)`.
- `CaseMap` / `Edits`: `CaseMap.toLower(locale, s, edits)` `toUpper()` `toTitle()` `fold()`; `Edits.getFineIterator()` `getCoarseIterator()` `getFineChangesIterator()` `hasChanges()` `numberOfChanges()` `lengthDelta()`.
- `Char`: `charType()` `charDirection()` `getIntPropertyValue(cp, prop)` `isUAlphabetic()` `getPropertyValueName()` `getPropertyValueEnum()` `charName()`.
- `StringSearch`: `StringSearch(pattern, text, collator_or_locale, break_iterator)` `first()` `next()` `getMatchedLength()` `setAttribute()`.
- `SpoofChecker`: `SpoofChecker()` `setChecks(checks)` `setRestrictionLevel(level)` `check(s)` `areConfusable(a, b)` `getSkeleton(s)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- translation: `icu` is a hand-written C++ extension mirroring the ICU4C C++ API class-for-class — a `UErrorCode &`/`ParseError &` out-arg drops and raises `icu.ICUError`, a `UnicodeString &` result returns as `str` (or passes in for in-place mutation), and `UDate` is milliseconds since epoch (Python `time()` × 1000 at the boundary).
- string boundary: an ICU API taking `UnicodeString` also accepts `str` — pass `str` in, read `str` out; allocate an explicit `UnicodeString` only for in-place mutation (`name[12:18] = 'x'`, `+=`), 16-bit `UChar` indexing, or a non-UTF-8 `bytes` decode (`UnicodeString(data, 'latin-1')`), with `countChar32()`/`char32At(i)` for codepoint access over the 16-bit store.
- gated seam: the extension is absent on the current interpreter, so the ICU arm dispatches on the `runtime` `anyio.to_process.run_sync` subprocess seam `python-bidi`'s `Bidi` arm uses — the ICU calls are GIL-holding C++ that must not stall the event loop, and a Python-subclass `Transliterator` raise maps into the `runtime` `RuntimeRail` fault across the boundary, never bare.
- data version: the linked `ICU_VERSION`/`UNICODE_VERSION` stamp the segmentation/collation/normalization receipt as the CLDR/UCA data version, so a data bump that shifts a sort key or a break is attributable.

[STACKING]:
- `expression`/`runtime` (`.api/expression.md`, substrate): the owner lifts each ICU call through `expression` `Result` / the `runtime` `RuntimeRail` (mapping `icu.ICUError`/`InvalidArgsError` into the typed fault at the gated seam), keys every output by the `runtime` `ContentKey`, and folds the segmentation/collation/normalization fact onto the shared `ArtifactReceipt`.
- `msgspec`/`beartype`/`anyio`/`structlog` (`.api/`, substrate): the engine choice is a `msgspec` `Struct` + `StrEnum` row (`SegmentEngine.{ICU, UNISEG}` / `BidiEngine.{ICU, PYTHON_BIDI}`, not parallel owners); `beartype` validates the `Locale`/strength/normalization-mode inputs at the boundary; `anyio.to_process.run_sync` runs the native call bounded by the shaping `CapacityLimiter`; `structlog` + OpenTelemetry span the rail.
- `uharfbuzz`/`fonttools`/`blackrenderer` (`.api/uharfbuzz.md`, folder): ICU stacks UNDER shaping as the locale-aware pre-layer — `Normalizer2.getNFCInstance().normalize` composes text before `uharfbuzz.Buffer.add_str`, `Bidi.getVisualRun` partitions the run into single-direction spans `uharfbuzz.shape` consumes, `Script.getScript` itemises the script runs `typography/font#FONT` fallback selects a face for; it supersedes neither `uharfbuzz` OpenType shaping, `fonttools` binary face model, nor `blackrenderer` COLRv1 raster.
- `typography/shape#SHAPE` `BIDI` arm: the ICU `Bidi`/`BidiTransform` path is the locale/explicit-level upgrade of the `python-bidi` `get_display` arm — one `BidiEngine` row on the existing `ShapeOp.BIDI` step (`Bidi.getVisualRun` itemisation feeding the shaper, not a parallel owner), with `Script.getScript` added as the font-fallback input threaded to `FONT`.
- `typography/layout#LAYOUT` `BREAK` arm: the ICU `BreakIterator.createLineInstance(locale)` path is the locale/dictionary upgrade of the `uniseg.linebreak` arm — one `SegmentEngine` row on the existing `LayoutOp.BREAK` step, projecting the ICU break offsets + `getRuleStatus()` tag into the same `BreakClass`/`Penalty` algebra the Knuth-Plass program threads; the dictionary segmentation closes the spaceless-script gap `uniseg`'s default table leaves open.
- collation seam: `Collator.getSortKey`/`AlphabeticIndex` is the locale-correct sort `drawing/schedule`, `specification/classify`, and `visualization/table` read for sorted, bucketed output — the one collation source.

[LOCAL_ADMISSION]:
- segmentation: `BreakIterator.createLineInstance(locale)` -> `setText` -> `first`/`next`-until-`DONE` with `getRuleStatus()` tagging each break for the `Penalty` cost; the locale-free `uniseg.linebreak.line_break` stays the default, ICU taken where the run carries a CLDR-tailored or dictionary-segmented script (Thai/Lao/Khmer/Burmese/Japanese/Chinese).
- bidi: `Bidi.setPara` -> `getVisualRun` is the directional-run itemisation and `setLine(start, limit)` the line-relative reorder; `python-bidi.get_display` stays the default whole-string reorder, ICU taken for explicit-level control, multi-paragraph per-paragraph levels, the logical/visual index map, or the `BidiTransform` reshape one-shot.
- collation: `Collator.createInstance(locale)` + `getSortKey`/`compare` is the locale-correct sort no other admitted package owns and `AlphabeticIndex` the bucketing; `RuleBasedCollator(rules)`/`cloneBinary()` are the custom and precompiled paths; sort once with `getSortKey`, use `compare`/`CollationKey` only when one string matches many.
- normalization: `Normalizer2.getNFCInstance().normalize(s)` is the pre-shaping canonical composition, `getNFKCCasefoldInstance` the case-fold-and-compatibility key, `FilteredNormalizer2` the `UnicodeSet`-scoped restriction.
- itemisation: `Script.getScript(cp).getShortName()` over a `StringCharacterIterator` is the font-fallback script split, `Transliterator.createInstance(id)`/`transliterate` the rule-based conversion, `CaseMap.toUpper(locale, s, Edits())` locale-correct casing with the change-span map.

[RAIL_LAW]:
- Package: `PyICU`
- Owns: the locale/CLDR-tailored Unicode text layer — dictionary-backed `BreakIterator` segmentation, the complete UAX#9 `Bidi`/`BidiTransform` engine, CLDR `Collator`/`RuleBasedCollator` collation with `AlphabeticIndex` bucketing, `Normalizer2`/`FilteredNormalizer2` UAX#15 normalization, `Script.getScript` itemisation, `Transliterator` conversion, `CaseMap`/`Edits` casing, `Char` properties, `UnicodeSet` membership, `StringSearch` search, and `SpoofChecker` confusable detection, over the `Locale`/`UnicodeString` substrate
- Accept: `createLineInstance(locale)` + dictionary segmentation where the script needs CLDR tailoring or has no spaces; `Bidi.setPara`/`getVisualRun`/`setLine` for explicit-level, line-relative, or index-mapped bidi; `Collator.getSortKey`/`AlphabeticIndex` for locale-correct sort and bucketing; `Normalizer2` NFC pre-shaping; `Script.getScript` font-fallback itemisation — each one engine row on the existing shape/layout step, dispatched on the gated `to_process` seam through `RuntimeRail`
- Reject: re-implementing the UCA weights, the UAX#9 resolution, the UAX#14 break rules, the UAX#15 tables, or the CLDR dictionary segmentation ICU4C owns; a second shaping/layout owner where the ICU engine is one `BidiEngine`/`SegmentEngine` row; the locale-free `uniseg`/`python-bidi` path on a plain-LTR run; a bare `icu.ICUError` across the interpreter boundary where `RuntimeRail` owns the fault
