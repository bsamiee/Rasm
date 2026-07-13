# [PY_ARTIFACTS_API_PYICU]

`PyICU` (dist `PyICU`, import `icu`) supplies the ICU4C International-Components-for-Unicode binding for the artifacts text pipeline: the CLDR-tailored, dictionary-backed segmentation engine (`BreakIterator.createLineInstance`/`createWordInstance`/`createCharacterInstance`/`createSentenceInstance` -> `RuleBasedBreakIterator`), the complete UAX#9 bidirectional engine (`Bidi.setPara`/`getLevels`/`getVisualRun`/`setLine` + the `BidiTransform` reorder-reshape one-shot), the CLDR `Collator`/`RuleBasedCollator` collation owner with sort-key + `AlphabeticIndex` bucketing, the `Normalizer2` NFC/NFD/NFKC normalization family, `Script.getScript` script itemisation, `Transliterator` rule-based script conversion, and the `Locale`/`UnicodeString`/`UnicodeSet` substrate the whole binding rides. The package owner composes these into the locale-aware text layer that STACKS UNDER the `uharfbuzz` shaping and the Knuth-Plass paragraph fold — ICU owns the locale/CLDR-tailored segmentation, bidi, collation, and normalization, where `uniseg` (default-table UAX#14) and `python-bidi` (UAX#9 reorder only) own the locale-free default path. It never re-implements the UCA collation weights, the UAX#9 resolution, the UAX#14 break rules, the UAX#15 normalization tables, or the CLDR dictionary segmentation the ICU4C C++ core owns.

[SOURCE]: cp-gated absent on this interpreter (`PyICU; python_version<'3.15'` in the root manifest; the live interpreter is cp315 so the dist is not installed and `assay api resolve PyICU` returns `unsupported` — `no 'PyICU' source`). The member surface here is verified against the PyICU C++ wrapper sources, the `icu/__init__.py` re-export (`from _icu import *`), the `CHANGES` wrapper log, and the `test/` usage corpus (`test_BreakIterator.py`/`test_Bidi.py`/`test_Collator.py`/`test_Normalizer.py`/`test_Script.py`/`test_Transliterator.py`), with the ICU4C C++ API as the upstream contract PyICU faithfully mirrors method-for-method (`UErrorCode`/`ParseError` out-args dropped, the result `UnicodeString &` returned as `str`). Re-verify through `assay api resolve PyICU` once a cp315 wheel/sdist lands and the gate is removed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PyICU`
- package: `PyICU` (dist name `PyICU`, import name `icu`)
- import: `icu` (the `icu` package re-exports the `_icu` C++ extension module: `from _icu import *`; the deprecated alias `PyICU` re-imports `icu`); `icu.ICUError` is the one exception rail wrapping every `UErrorCode` failure, carrying `getErrorCode()`
- owner: `artifacts`
- rail: text-locale
- license: MIT (verbatim MIT text in `icu/__init__.py` and `LICENSE`; the dynamically-linked ICU4C libraries are ICU/Unicode license — both permissive, no copyleft obligation)
- version: `2.x` (latest stable line; tracks the linked ICU4C release — `icu.ICU_VERSION`/`icu.UNICODE_VERSION`/`icu.ICU_MAX_MAJOR_VERSION` report the runtime library, not the binding)
- build floor: sdist-only C++ extension; needs ICU4C headers+libs discoverable through `pkg-config` (`icu-i18n`/`icu-uc`/`icu-io`) and a C++11 compiler matching the CPython toolchain. No cp315 wheel exists, so on this interpreter the gate `python_version<'3.15'` excludes it; native ICU4C provisioning (Homebrew `icu4c`, Debian `libicu-dev`, or a Forge build) is a provisioning follow-up, never a design-page blocker
- entry points: none (library only; the `_icu` extension is import-only)
- capability: CLDR-tailored + dictionary-backed Unicode segmentation (line/word/character/sentence/title `BreakIterator`); the complete UAX#9 bidirectional algorithm (`Bidi` paragraph + line reorder with explicit-level control, level/visual-run extraction, reordering-map and the `BidiTransform` reshape+reorder one-shot); CLDR collation (`Collator`/`RuleBasedCollator` compare/sort-key with the full UCA attribute matrix, binary clone, and `AlphabeticIndex` bucketing); UAX#15 normalization (`Normalizer2` NFC/NFD/NFKC/NFKD/NFKC-casefold + `FilteredNormalizer2`); script itemisation (`Script.getScript`/`getScriptExtensions`/`isRightToLeft`); rule-based transliteration (`Transliterator` create/inverse/register + Python subclass); locale-aware case mapping (`CaseMap` + `Edits`); Unicode character properties (`Char`); `UnicodeSet` pattern membership; `StringSearch` locale-aware search; `SpoofChecker` confusable/restriction detection; UTS#46 IDNA; the `Locale`/`UnicodeString`/`UnicodeSet`/`StringCharacterIterator` substrate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: locale, string, and set substrate
- rail: text-locale
- call: `Locale` — `Locale(name)` / `Locale.getEnglish()`/`getUS()`/`getRoot()`/`getJapanese()`/...; `getName`/`getLanguage`/`getScript`/`getDisplayName`; `forLanguageTag(tag)`/`toLanguageTag()`; `addLikelySubtags`/`minimizeSubtags` (in place); `setKeywordValue`/`createKeywords`; `canonicalize` — the locale every CLDR-tailored operation keys on
- call: `UnicodeString` — `UnicodeString(str)` / `UnicodeString(bytes, encoding)`; mutable sequence of 16-bit `UChar` with `[]`/`[:]`/`+=`; `countChar32()`/`char32At(i)` codepoint access; auto-converts to/from Python `str` at every API boundary — needed explicitly only for in-place result args or non-UTF-8 decode
- call: `UnicodeSet` — `UnicodeSet(pattern)` (ICU `[...]` set syntax incl. `p{...}` property classes); `contains`/`add`/`addAll`/`retainAll`/`complement`/`freeze`; `UnicodeSetIterator` enumeration — the set the `BreakIterator`/`Collator`/`Transliterator`/`StringSearch` surfaces accept
- call: `StringCharacterIterator` / `CharacterIterator` — `first`/`next`/`previous`/`next32PostInc`/`hasNext` codepoint-aware cursor over a `UnicodeString` — the iteration substrate `Script.getScript` itemisation walks
- call: `ICUError` — the one exception every `UErrorCode` failure raises (`from icu import ICUError`); `getErrorCode()` returns the raw `UErrorCode`; `InvalidArgsError` for overload-resolution failures — the rail the owner maps into `runtime` `RuntimeRail` at the gated seam
- call: `ICU_VERSION` / `UNICODE_VERSION` / `ICU_MAX_MAJOR_VERSION` — linked-library version strings (e.g. `'74.2'` / `'15.1'`); the owner reads these to gate version-conditional members and to stamp the segmentation/collation/normalization receipt with the data version

Every ICU operation is parameterised by a `Locale` and consumes/produces text that crosses the boundary as a Python `str` (ICU's `UnicodeString` is auto-converted; pass a `str` in, get a `str` out — the explicit `UnicodeString` value object is needed only for in-place mutation or to carry a non-UTF-8 `bytes` decode). `UnicodeSet` is the closed character-class value the break/collator/transliterator surfaces accept.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]        | [CAPABILITY]                               |
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
- rail: text-locale
- call: `BreakIterator` — abstract base; `createLineInstance(Locale)`/`createWordInstance`/`createCharacterInstance`/`createSentenceInstance`/`createTitleInstance` factory family returning a concrete `RuleBasedBreakIterator`; `DONE` sentinel (`-1`); the CLDR-tailored, dictionary-backed iterator the locale-aware line-break needs
- call: `RuleBasedBreakIterator` — `setText(str)`/`getText()`; `first`/`next`/`previous`/`last`/`current`/`following(off)`/`preceding(off)`/`isBoundary(off)`; `nextBoundary()` (fast boundary bypass); `getRuleStatus()` (the break-tag — the `UWordBreak` rule status distinguishing word/number/CJK breaks); `getBinaryRules()` -> `bytes` and `RuleBasedBreakIterator(binary_rules)` precompiled-rule round-trip; `RuleBasedBreakIterator(rule_str)` custom RBBI grammar
- call: `Bidi` — `Bidi()` / `Bidi(maxLength, maxRunCount)`; `setPara(text, paraLevel=DEFAULT_LTR, embeddingLevels=None)`; `setLine(start, limit)` -> a line-scoped `Bidi`; `getParaLevel`/`getLevelAt`/`getLevels`; `getDirection()` -> `UBiDiDirection`; `getBaseDirection`; `countRuns`/`getVisualRun(i)` -> `(logicalStart, length, level)`; `getLogicalRun`/`getVisualIndex`/`getLogicalIndex`
- call: `Bidi` — `getLogicalMap`/`getVisualMap`/`invertMap`; `writeReordered(options)`; `reorderVisual(levels)`/`reorderLogical(levels)`; `countParagraphs`/`getParagraph`/`getParagraphByIndex`; `orderParagraphsLTR`/`setReorderingMode(UBiDiReorderingMode)`/`setReorderingOptions(UBiDiReorderingOption)` — the complete bidi surface `python-bidi` does not expose
- call: `BidiTransform` — `BidiTransform()`; `transform(text, inParaLevel, inOrder, outParaLevel, outOrder, mirroring, shapingOptions)` -> `str` — the one-shot UAX#9 reorder + Arabic shaping + digit-shaping transform across the four (level, `UBiDiOrder`) endpoints
- call: `Collator` / `RuleBasedCollator` — `Collator.createInstance(Locale)` -> a `RuleBasedCollator`; `compare(a, b)` -> `-1/0/1`; `getSortKey(s)` -> `bytes` (stable binary sort key); `getCollationKey(s)` -> `CollationKey`; `setAttribute(UCollAttribute.*, UCollAttributeValue.*)` / `setStrength`; `getRules()`; `cloneBinary()` -> `bytes` and `RuleBasedCollator(binary, root)` / `RuleBasedCollator(rule_str)`; `CollationElementIterator` per-element walk — the unique admitted locale-correct sort
- call: `CollationKey` — the opaque comparison key `getCollationKey` returns; orderable (`<`/`==`) and `toByteArray()` for storage; the precomputed form when one string is compared against many
- call: `AlphabeticIndex` / `ImmutableIndex` — `AlphabeticIndex(Locale)`; `addLabels(Locale)`/`addRecord(name, data)`/`getBucketLabel`/`nextBucket`/`nextRecord`/`recordData`; iterates `(label, type)` buckets; `buildImmutableIndex()` -> `ImmutableIndex` — the A-Z (locale-script) bucketing a schedule/glossary index needs

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]       | [CAPABILITY]                                  |
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
- rail: text-locale
- call: `Normalizer2` — `getNFCInstance`/`getNFDInstance`/`getNFKCInstance`/`getNFKDInstance`/`getNFKCCasefoldInstance` singletons; `getInstance(data, name, UNormalizationMode2.{COMPOSE,DECOMPOSE,...})` custom; `normalize(s)` -> `str`; `isNormalized`/`quickCheck`/`spanQuickCheckYes`; `hasBoundaryBefore`/`hasBoundaryAfter`/`isInert`; `append`/`normalizeSecondAndAppend` incremental compose — the pre-shaping NFC normalization the shape owner runs before `uharfbuzz`
- call: `FilteredNormalizer2` — `FilteredNormalizer2(norm2, unicodeSet)` — normalization restricted to a `UnicodeSet` (normalise only the marks in a script range), composing a `Normalizer2` with a `UnicodeSet` filter
- call: `Script` / `UScriptCode` — `Script.getScript(codepoint)` -> a script value; `.getShortName()` (`'Latn'`/`'Arab'`/`'Hani'`)/`.getScriptCode()`; `Script.getScriptExtensions(cp)`; `script.isRightToLeft()`/`script.getSampleString()`; the `UScriptCode` enum — the script-run itemisation the font-fallback boundary keys on
- call: `Transliterator` — `Transliterator.createInstance(id)` (built-in IDs `'Any-Latin'`/`'Accents-Any'`/`'NumericPinyin-Latin'`/...); `createFromRules(id, rules, dir)`; `transliterate(str)` -> `str`; `createInverse()`; `toRules(escaped)`; `getAvailableIDs`; `registerInstance(t)` and a Python subclass overriding `handleTransliterate(text, UTransPosition, incremental)` — script conversion and custom text rewriting
- call: `CaseMap` / `Edits` — `CaseMap.toLower(Locale, s, Edits?)`/`toUpper`/`toTitle`/`fold` (all `classmethod`s); `Edits` records the change-spans — `Edits.getFineIterator()`/`getCoarseIterator()`/`getFineChangesIterator()` walk the change runs, `hasChanges`/`numberOfChanges`/`lengthDelta` summarise them — locale-correct casing (Turkish dotless-i, Greek final-sigma) and the edit map a downstream re-index needs
- call: `Char` — `Char.charType`/`charDirection`/`getIntPropertyValue(cp, UProperty.*)`/`isUAlphabetic`/`getPropertyValueName`/`getPropertyValueEnum`/`charName`; the full `uchar.h` property surface — the property query a custom break/itemisation rule reads
- call: `StringSearch` / `SearchIterator` — `StringSearch(pattern, text, Collator \| Locale, BreakIterator?)`; `first`/`next`/`getMatchedLength`/`setAttribute` — collation-aware (accent/case-insensitive) substring search honouring grapheme boundaries
- call: `SpoofChecker` — `SpoofChecker()`; `setChecks(USpoofChecks.*)`/`setRestrictionLevel(URestrictionLevel)`; `check(s)`/`areConfusable(a, b)`/`getSkeleton(s)` — confusable/mixed-script/restriction-level detection for an identifier-safety gate

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]       | [CAPABILITY]                                  |
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
- rail: text-locale
- call: `BreakIterator.createLineInstance(locale: Locale)` -> `RuleBasedBreakIterator` — CLDR-tailored + dictionary-backed line-break-opportunity iterator
- call: `BreakIterator.createWordInstance` / `createCharacterInstance` / `createSentenceInstance` / `createTitleInstance` — word / grapheme-cluster / sentence / title-case break iterators
- call: `bi.setText(text: str)` / `bi.getText()` — load the run to segment; the iterator keeps a reference to the text
- call: `bi.first()` / `bi.next()` / `bi.previous()` / `bi.last()` / `bi.current()` — walk boundary offsets; `next()` returns the next offset or `BreakIterator.DONE`
- call: `bi.following(offset)` / `bi.preceding(offset)` / `bi.isBoundary(offset)` / `bi.nextBoundary()` — break at/around an offset; `nextBoundary()` is the offset-only fast path
- call: `bi.getRuleStatus()` — the `UWordBreak`/line rule status distinguishing the break kind
- call: `bi.getBinaryRules()` -> `bytes` / `RuleBasedBreakIterator(binary_rules)` — precompiled-rule round-trip (build once, reload the table)
- call: `RuleBasedBreakIterator(rule_string)` — a custom RBBI rule grammar (an AEC dimension-string or code-token break)

`BreakIterator.createLineInstance(Locale)` is the CLDR-tailored line-break entry: it returns a `RuleBasedBreakIterator` that knows the locale's tailoring AND carries the bundled Thai/Lao/Khmer/Burmese/Japanese/Chinese dictionary so a spaceless script breaks at lexical boundaries — the capability `uniseg`'s default-table UAX#14 cannot reach. `setText(str)` loads the run; iterate `first`/`next` (or `following(off)`/`nextBoundary()`) collecting offsets until `DONE`; `getRuleStatus()` at each break tags it (number vs word vs CJK-ideograph) so the Knuth-Plass `Penalty` cost can weight a CJK break differently from a Latin space.

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `BreakIterator.createLineInstance(…)`      | line break      | tailored dictionary boundaries      |
|  [02]   | `BreakIterator.createWordInstance(…)`      | segment factory | locale-tailored word boundaries     |
|  [03]   | `BreakIterator.createCharacterInstance(…)` | segment factory | grapheme-cluster boundaries         |
|  [04]   | `BreakIterator.createSentenceInstance(…)`  | segment factory | locale-tailored sentence boundaries |
|  [05]   | `BreakIterator.createTitleInstance(…)`     | segment factory | title-case boundaries               |
|  [06]   | `bi.setText(…)`                            | text bind       | load the segmented run              |
|  [07]   | `bi.getText()`                             | text bind       | return the retained run             |
|  [08]   | `bi.first()`                               | iterate         | first boundary offset               |
|  [09]   | `bi.next()`                                | iterate         | next offset or `BreakIterator.DONE` |
|  [10]   | `bi.previous()`                            | iterate         | previous boundary offset            |
|  [11]   | `bi.last()`                                | iterate         | final boundary offset               |
|  [12]   | `bi.current()`                             | iterate         | current boundary offset             |
|  [13]   | `bi.following(…)`                          | random access   | boundary after an offset            |
|  [14]   | `bi.preceding(…)`                          | random access   | boundary before an offset           |
|  [15]   | `bi.isBoundary(…)`                         | random access   | boundary predicate                  |
|  [16]   | `bi.nextBoundary()`                        | random access   | offset-only fast path               |
|  [17]   | `bi.getRuleStatus()`                       | break tag       | `UWordBreak` or line-rule status    |
|  [18]   | `bi.getBinaryRules()`                      | rule cache      | serialize precompiled rules         |
|  [19]   | `RuleBasedBreakIterator(binary_rules)`     | rule cache      | reload precompiled rules            |
|  [20]   | `RuleBasedBreakIterator(rule_string)`      | custom grammar  | AEC and code-token break rules      |

[ENTRYPOINT_SCOPE]: bidirectional reordering and reshaping
- rail: text-locale
- call: `Bidi()` / `Bidi(max_length, max_run_count)` — allocate the bidi object (pre-sized for the longest paragraph)
- call: `Bidi.setPara(text, para_level=UBiDiLevel.DEFAULT_LTR, embedding_levels=None)` — run UAX#9 over the paragraph; `para_level` `0`=LTR/`1`=RTL/`DEFAULT_*`=auto
- call: `Bidi.getParaLevel()` / `getLevelAt(i)` / `getLevels()` — the resolved paragraph base level and the per-character embedding-level tuple
- call: `Bidi.getDirection()` -> `UBiDiDirection` (`LTR`/`RTL`/`MIXED`/`NEUTRAL`) — whole-run direction; `MIXED` triggers the visual-run itemisation
- call: `Bidi.countRuns()` / `getVisualRun(i)` -> `(logicalStart, length, level)` — the single-direction run partition the shaper itemises by
- call: `Bidi.setLine(start, limit)` -> `Bidi` — line-relative reordering over the resolved paragraph (post-line-break)
- call: `Bidi.writeReordered(options)` — the visual-order string with `UBiDiReorderingOption` controls (`KEEP_BASE_COMBINING`/`REMOVE_BIDI_CONTROLS`/...)
- call: `Bidi.getVisualIndex(logical)` / `getLogicalIndex(visual)` / `getLogicalMap()` / `getVisualMap()` — per-character logical<->visual position map (for cursor/selection)
- call: `BidiTransform().transform(text, in_level, in_order, out_level, out_order, mirroring, shaping)` — reorder + Arabic-shape + digit-shape across (level, `UBiDiOrder`) endpoints

`Bidi.setPara` runs the full UAX#9 resolution: it resolves explicit embeddings/overrides/isolates, weak/neutral/implicit types, and produces the per-character level array AND the visual-run decomposition. `getVisualRun(i)` -> `(logicalStart, length, level)` is the directional-run partition the shaping owner itemises by (each run is a single-direction span `uharfbuzz` shapes in one direction); `setLine(start, limit)` re-runs reordering line-relative AFTER the paragraph levels are known, which a line-broken run needs and `python-bidi`'s whole-string `get_display` cannot give. `BidiTransform.transform` is the one-shot reorder + Arabic-join + digit-shape across (level, order) endpoints.

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY]     | [CAPABILITY]                         |
| :-----: | :-------------------------------- | :----------------- | :----------------------------------- |
|  [01]   | `Bidi()`                          | engine init        | default bidi object                  |
|  [02]   | `Bidi(max_length, max_run_count)` | engine init        | pre-sized paragraph storage          |
|  [03]   | `Bidi.setPara(…)`                 | resolve            | UAX#9 paragraph resolution           |
|  [04]   | `Bidi.getParaLevel()`             | level read         | resolved paragraph base level        |
|  [05]   | `getLevelAt(…)`                   | level read         | one character embedding level        |
|  [06]   | `getLevels()`                     | level read         | complete embedding-level tuple       |
|  [07]   | `Bidi.getDirection()`             | direction probe    | whole-run direction vocabulary       |
|  [08]   | `Bidi.countRuns()`                | visual runs        | directional-run count                |
|  [09]   | `getVisualRun(…)`                 | visual runs        | shaper-ready directional partition   |
|  [10]   | `Bidi.setLine(…)`                 | line reorder       | post-break line-relative ordering    |
|  [11]   | `Bidi.writeReordered(…)`          | reorder text       | visual text with reordering controls |
|  [12]   | `Bidi.getVisualIndex(…)`          | index map          | logical-to-visual position           |
|  [13]   | `getLogicalIndex(…)`              | index map          | visual-to-logical position           |
|  [14]   | `getLogicalMap()`                 | index map          | complete logical-to-visual map       |
|  [15]   | `getVisualMap()`                  | index map          | complete visual-to-logical map       |
|  [16]   | `BidiTransform().transform(…)`    | one-shot transform | reordered Arabic and digit shaping   |

[ENTRYPOINT_SCOPE]: collation, indexing, and normalization
- rail: text-locale
- call: `Collator.createInstance(locale: Locale)` -> `RuleBasedCollator` — the CLDR-tailored collator for the locale
- call: `collator.compare(a: str, b: str)` -> `int` — `-1/0/1` locale-correct comparison (use as a `sort(key=cmp_to_key(...))`)
- call: `collator.getSortKey(s: str)` -> `bytes` — the stable binary key — `sorted(items, key=collator.getSortKey)` the canonical sort
- call: `collator.setAttribute(UCollAttribute.STRENGTH, UCollAttributeValue.TERTIARY)` / `setStrength(level)` — the UCA attribute matrix (strength/case-first/normalization/alternate-handling/numeric)
- call: `collator.getRules()` / `collator.cloneBinary()` -> `bytes` / `RuleBasedCollator(binary, root)` / `RuleBasedCollator(rules)` — custom-rule and precompiled-binary collator construction
- call: `AlphabeticIndex(locale)` + `addRecord(name, data)` / `nextBucket` / `nextRecord` / `recordData` / `buildImmutableIndex()` — locale-script A-Z bucketing for a glossary/schedule index
- call: `Normalizer2.getNFCInstance()` / `getNFDInstance` / `getNFKCInstance` / `getNFKDInstance` / `getNFKCCasefoldInstance` — the five standard normalization singletons
- call: `norm2.normalize(s: str)` -> `str` / `norm2.isNormalized(s)` / `norm2.quickCheck(s)` — canonical/compatibility normalization + the quick-check fast path
- call: `Script.getScript(codepoint: int)` -> `UScriptCode` (`.getShortName()`) — per-codepoint script for the font-fallback run itemisation
- call: `Transliterator.createInstance(id)` / `transliterate(s)` / `createInverse()` — rule-based script conversion (`'Any-Latin'`, `'Latin-ASCII'`, custom rules)

`Collator.createInstance(Locale)` is the CLDR collation entry. `compare(a, b)` is the one-shot comparator; `getSortKey(s)` -> `bytes` is the stable binary key the owner sorts by when one corpus is sorted once (the key, not the comparator, is what a stored sort index holds). The `setAttribute(UCollAttribute.*, UCollAttributeValue.*)` matrix tailors strength (`PRIMARY`..`IDENTICAL`), case-first, normalization, and alternate-handling. `Normalizer2.getNFCInstance().normalize(s)` is the canonical-composition pre-pass the shaping owner runs before `uharfbuzz` so combining marks compose to their precomposed form.

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]  | [CAPABILITY]                           |
| :-----: | :--------------------------------- | :-------------- | :------------------------------------- |
|  [01]   | `Collator.createInstance(…)`       | collator init   | CLDR-tailored locale collator          |
|  [02]   | `collator.compare(…)`              | compare         | locale-correct three-way ordering      |
|  [03]   | `collator.getSortKey(…)`           | sort key        | stable binary corpus-sort key          |
|  [04]   | `collator.setAttribute(…)`         | tailor          | UCA attribute-matrix selection         |
|  [05]   | `setStrength(…)`                   | tailor          | collation-strength selection           |
|  [06]   | `collator.getRules()`              | rule round-trip | recover custom collation rules         |
|  [07]   | `collator.cloneBinary()`           | rule round-trip | serialize compiled collation rules     |
|  [08]   | `RuleBasedCollator(binary, root)`  | rule round-trip | load precompiled collation rules       |
|  [09]   | `RuleBasedCollator(rules)`         | rule round-trip | compile custom collation rules         |
|  [10]   | `AlphabeticIndex(…)`               | index bucket    | locale-script glossary buckets         |
|  [11]   | `Normalizer2.getNFCInstance(…)`    | normalizer      | five standard normalization singletons |
|  [12]   | `norm2.normalize(…)`               | normalize       | normalization and quick-check path     |
|  [13]   | `Script.getScript(…)`              | script itemise  | font-fallback codepoint script         |
|  [14]   | `Transliterator.createInstance(…)` | transliterate   | built-in or custom script conversion   |

## [04]-[IMPLEMENTATION_LAW]

[TEXT_LOCALE_TOPOLOGY]:
- namespace: `icu`; a hand-written C++ extension (not SWIG) faithfully mirroring the ICU4C C++ API class-for-class and method-for-method. Translation law: an ICU C++ `UErrorCode &` out-arg is dropped and the failure raised as `icu.ICUError`; a `ParseError &` is likewise dropped; a `UnicodeString &` result arg is omitted and the value returned as a Python `str` (or, the ICU way, passed in to be mutated in place and returned). `UDate` is ms-since-epoch (Python `time()` seconds × 1000 at the boundary).
- string boundary: ICU APIs taking `UnicodeString` are overloaded to also accept `str`; pass `str` in and read `str` out. Allocate an explicit `UnicodeString` only for in-place mutation (`name[12:18] = 'x'`, `+=`), 16-bit `UChar` indexing, or to decode a non-UTF-8 `bytes` (`UnicodeString(data, 'latin-1')`). `countChar32()`/`char32At(i)` give codepoint-correct access over the 16-bit store.
- gated runtime: the binding is cp-gated (`python_version<'3.15'`) and native (links ICU4C). On the runtime floor the owner dispatches the ICU arm onto the `runtime` `anyio.to_process.run_sync` subprocess seam (the same gated seam `python-bidi`'s `Bidi` arm uses), because the extension is absent on the cp315 interpreter and the ICU calls are GIL-holding C++ that must not stall the event loop. A Python-subclass `Transliterator` that raises propagates the exception across the boundary — map it into the `runtime` `RuntimeRail` fault, never surface it bare.
- version conditioning: read `icu.ICU_VERSION`/`icu.ICU_MAX_MAJOR_VERSION` before calling a version-conditional member (`getBinaryRules` needs ICU >= 4.8, `BidiTransform` needs ICU >= 58, `getNFKCCasefoldInstance` needs ICU >= 49); stamp the linked `ICU_VERSION`/`UNICODE_VERSION` onto the segmentation/collation/normalization receipt as the data version so a CLDR/UCA bump that shifts a sort key or break is attributable.

[LOCAL_ADMISSION]:
- segmentation: `BreakIterator.createLineInstance(Locale)` -> `setText` -> `first`/`next`-until-`DONE` is the locale-aware break path; `getRuleStatus()` tags each break for the `Penalty` cost. The default-table `uniseg.linebreak.line_break` path stays the locale-free fast path; ICU is taken when the run carries a CLDR-tailored or dictionary-segmented script (Thai/Lao/Khmer/Burmese/Japanese/Chinese) where `uniseg` cannot find lexical boundaries.
- bidi: `Bidi.setPara` -> `getVisualRun` is the directional-run itemisation; `setLine(start, limit)` is the line-relative reorder over the resolved paragraph. The whole-string `python-bidi.get_display` stays the simple LTR/RTL reorder path; ICU is taken when the run needs explicit-level control, per-paragraph levels in a multi-paragraph block, the logical<->visual index map (cursor placement), or the `BidiTransform` reshape+reorder one-shot.
- collation: `Collator.createInstance(Locale)` + `getSortKey`/`compare` is the locale-correct sort no other admitted package provides; `AlphabeticIndex` is the bucketing. `RuleBasedCollator(rules)`/`cloneBinary()` are the custom-tailoring and precompiled-binary paths. Sort once with `getSortKey`; use `compare`/`CollationKey` only when one string is matched against many ad hoc.
- normalization: `Normalizer2.getNFCInstance().normalize(s)` is the canonical-composition pre-pass before shaping; `getNFKCCasefoldInstance` is the case-fold-and-compatibility-fold path for a search/match key. `FilteredNormalizer2` restricts normalization to a `UnicodeSet`.
- itemisation: `Script.getScript(cp).getShortName()` over a `StringCharacterIterator` is the script-run split for font fallback. `Transliterator.createInstance(id)`/`transliterate` is rule-based script conversion; `CaseMap.toUpper(Locale, s, Edits())` is locale-correct casing with the change-span `Edits` map.

[STACK_INTEGRATION]:
- shared substrate tier (`libs/python/.api`): the owner lifts each ICU call through `expression` `Result` / the `runtime` `RuntimeRail` (mapping `icu.ICUError`/`InvalidArgsError` into the typed fault at the gated seam, never a bare raise), keys every output by the `runtime` `ContentKey`, models the engine choice as a `msgspec` `Struct` + `StrEnum` step (a `SegmentEngine.{ICU, UNISEG}` / `BidiEngine.{ICU, PYTHON_BIDI}` row, not parallel owners), validates the `Locale`/strength/normalization-mode inputs with `beartype` at the boundary, runs the GIL-holding native call under `anyio.to_process.run_sync` bounded by the shaping `CapacityLimiter`, and folds the segmentation/collation/normalization fact (locale, ICU/Unicode version, break count, sort-key length) onto the shared `ArtifactReceipt` via the `structlog` + OpenTelemetry span the rail wraps.
- folder tier (`libs/python/artifacts/.api`): ICU stacks UNDER `uharfbuzz` shaping as the locale-aware pre-layer — `Normalizer2.getNFCInstance().normalize` composes the text before `uharfbuzz.Buffer.add_str`, `Bidi.getVisualRun` partitions the run into the single-direction spans `uharfbuzz.shape` consumes, and `Script.getScript` itemises the script runs the `typography/font#FONT` fallback selects a face for. It supersedes neither `uharfbuzz` (OpenType shaping), `fonttools` (the binary face model), nor `blackrenderer` (COLRv1 raster).
- `typography/shape#SHAPE` `BIDI` arm: the ICU `Bidi`/`BidiTransform` path is the locale/explicit-level upgrade of the `python-bidi` `get_display` arm — one `BidiEngine` row on the existing `ShapeOp.BIDI` step (the ICU `Bidi.getVisualRun` itemisation feeding the shaper, not a parallel shaping owner), with `Script.getScript` itemisation added as the font-fallback input the shape owner threads to `FONT`.
- `typography/layout#LAYOUT` `BREAK` arm: the ICU `BreakIterator.createLineInstance(Locale)` path is the locale/dictionary upgrade of the `uniseg.linebreak` arm — one `SegmentEngine` row on the existing `LayoutOp.BREAK` step, projecting the ICU break offsets + `getRuleStatus()` tag into the SAME `BreakClass`/`Penalty` algebra the Knuth-Plass dynamic program threads, never a second layout owner. The dictionary segmentation closes the spaceless-script gap `uniseg`'s default table leaves open.
- collation seam: `Collator.getSortKey`/`AlphabeticIndex` is the locale-correct sort the schedule/index/classification owners (`drawing/schedule`, `specification/classify`, `visualization/table`) read for sorted, bucketed output — the one collation source, never a hand-rolled locale comparator.

[RAIL_LAW]:
- Package: `PyICU`
- Owns: the locale/CLDR-tailored Unicode text layer — dictionary-backed line/word/character/sentence/title `BreakIterator` segmentation, the complete UAX#9 `Bidi`/`BidiTransform` engine (paragraph + line reorder, level/visual-run/index-map extraction, reshape+reorder), CLDR `Collator`/`RuleBasedCollator` collation (compare/sort-key/binary-clone) with `AlphabeticIndex` bucketing, `Normalizer2`/`FilteredNormalizer2` UAX#15 normalization, `Script.getScript` itemisation, `Transliterator` rule-based conversion, `CaseMap`/`Edits` locale casing, `Char` properties, `UnicodeSet` membership, `StringSearch` locale search, `SpoofChecker` confusable detection, and the `Locale`/`UnicodeString` substrate
- Accept: `createLineInstance(Locale)`+dictionary segmentation where the script needs CLDR tailoring or has no spaces; `Bidi.setPara`/`getVisualRun`/`setLine` for explicit-level/line-relative/index-mapped bidi; `Collator.getSortKey`/`AlphabeticIndex` for locale-correct sort and bucketing; `Normalizer2` NFC pre-shaping; `Script.getScript` font-fallback itemisation — each as one engine row on the existing shape/layout step, dispatched on the gated `to_process` seam, mapped through `RuntimeRail`
- Reject: re-implementing the UCA collation weights, the UAX#9 resolution, the UAX#14 break rules, the UAX#15 normalization tables, or the CLDR dictionary segmentation ICU4C owns; a second shaping/layout owner where the ICU engine is one `BidiEngine`/`SegmentEngine` row on the existing arm; the locale-free `uniseg`/`python-bidi` path where the run is plain-LTR (ICU is the locale/dictionary upgrade, not the default); a bare `icu.ICUError` across the interpreter boundary where `RuntimeRail` owns the fault
