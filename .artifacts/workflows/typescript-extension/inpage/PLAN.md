# [INPAGE_TYPESETTING_PLAN]

Goal: universal Perso-Arabic typesetting over any installed font as real InDesign and typography logic in `libs/typescript`, general at its level for any app to compose and shaped by neither Atelier nor MCP. `apps/creative-cloud/atelier` composes it as MCP tools built on `libs/typescript/ai/mcp` at that package's full capability: each tool does more so tools collapse rather than multiply, and state a client reads is a resource rather than a call. Indented paths under each row name the files and functions in `docs/research/applications/indesign/` holding its logic, InPage's first: evidence of principles rebuilt as general library capability, never scripts to copy or a source of Urdu or Desi forms or one font's or product's values. Capability the distills, rows and research hold that the libraries lack or narrow joins as new general logic or deepens existing logic, and growth adding it stays, held to `.claude/workflows/BAR.md` like every other line, with adversaries refining new code as hard as old. Every package `docs/research/typescript-extension/packages.md` chooses serves at full capability: each reader and adversary mines package source at its catalog release against the code, hand logic a member states goes, and capability a member makes cheap joins where it belongs. Each section after the host mechanisms is one unit of rows owning one package's files, in dependency order. `node proof/ab.ts` in `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/` is the behavior oracle.

## [01]-[DECISIONS]

Rules every package of the run follows:
- InPage faces, data files, widgets, license and Windows calls supply no name, size, style, step or table to library code
- InPage tuned values (kerning rule rows, thresholds, kashida ranks, speed flags) enter `proof/ab` adapters alone, libraries take caller rows
- InDesign capability is Effect operations over host objects any UXP program calls, the `Typesetting` RPC group their transport
- `@rasm/indesign` and `@rasm/automation` hold no MCP shape
- Each InDesign write uses the member InDesign states for its capability, in place of a find-and-change pass or InPage attribute `0x50f3e` or `0x1bca`
- Pair kerning derives from outline geometry, each kern the negated minimum clearance between band ink profiles of adjacent shaped runs in font units:
  - Preceding run takes a line slanted from `foot` to `top` slid to touch its ink, clearance capped at its advance, plus `correction`, floored at zero
  - Rules match ICU `JoiningGroup` names, `following` testing the following run's first letter pair, `preceding` the preceding run's last letter
  - Lone punctuation pairs as a preceding run alone, and runs pair across at most one space separator or ZWNJ
  - Bitmaps, byte-shift quantization, thresholds, penalties and bonuses are InPage artifacts
- Nuqta and aerab adjustment is one em offset per axis on glyphs HarfBuzz classes `MARK`
- Letter adjustment is kerning, tracking and baseline-shift deltas, kerning at the insertion point after the last base letter before its marks
- Kashida is InDesign's own justification over any installed face: `None`, `Tatweel` (U+0640 inserted) or `Stretched` (`jalt`, then tatweels)
  - Elongation per place stays within caller bounds `lower..upper` in nuqta widths, `fractional` allowing part of a tatweel
  - Nuqta width is the narrowest dot contour of the face's Beh
  - `Stretched` probes `jalt` at each joining place and keeps an alternate bringing the line closer to its width
  - HarfBuzz flags no `SAFE_TO_INSERT_TATWEEL` place on `jalt` faces, and tatweels go at flagged clusters alone
- Text carving is stylistic sets `ss01`..`ss20` of the face's GSUB, the styles InPage's vendor page names
- Quran glossary and symbol sets decode from caller CSV and TSV text with caller-named header columns, digits in any decimal numbering system
- Quran ayahs key by number, sorted per surah with one per number, InPage's file order, repeated numbers and texts cut at NUL being artifacts
- Symbol cells are 1 to 6 hex digits after an optional `0x` or `0X` naming a Unicode scalar value, InPage's other parses being artifacts
- Symbol sets filter by face coverage, and Atelier tools return text that `change` inserts

## [02]-[HOST_MECHANISMS]

InDesign reads and writes go through members:

| [INDEX] | [CAPABILITY]    | [MECHANISM]                                                                                                   |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | Targets         | Story and frame take `texts.item(0)`, `/` takes `app.textDefaults`                                            |
|  [02]   | Text defaults   | No `kerningValue` or `opentypeFeatures`, kerns and kerning deltas on `/` fail `Unmatched`                     |
|  [03]   | Pair kern       | `InsertionPoint.kerningValue` in thousandths of an em, negative tightens                                      |
|  [04]   | Manual kerns    | Nonzero value under `kerningMethod` `app.translateKeyString('$ID/Manual')` stays                              |
|  [05]   | Metrics kerning | `kerningMethod` written as `app.translateKeyString('$ID/Metrics')`                                            |
|  [06]   | Tracking        | `tracking` in thousandths of an em                                                                            |
|  [07]   | Baseline shift  | `baselineShift` in points, up positive                                                                        |
|  [08]   | Mark offsets    | `xOffsetDiacritic`, `yOffsetDiacritic` in thousandths of an em at any size, positive y away from base         |
|  [09]   | Kashida         | `kashidas`, `paragraphJustification` `NASKH_*` with `_FRAC` fractional, `paragraphKashidaWidth` w as `[w, w]` |
|  [10]   | Stylistic sets  | `otfStylisticSets` bitmask, bit n − 1 for `ss<n>`                                                             |
|  [11]   | Feature flags   | `otf<Feature>` booleans for `titl`, `calt`, `dlig`, `salt`, `jalt`                                            |
|  [12]   | Small caps      | `capitalization` `CAP_TO_SMALL_CAP` for `c2sc` with `smcp`, `SMALL_CAPS` for `smcp`                           |
|  [13]   | Other features  | `opentypeFeatures` tag and value pairs                                                                        |
|  [14]   | Digits          | `digitsType` to an `Intl` numbering system id                                                                 |
|  [15]   | Direction       | `paragraphDirection` always reads a member, no direction writes `characterDirection` `DEFAULT_DIRECTION`      |
|  [16]   | Spacing         | `minimum`, `desired`, `maximum` × `WordSpacing`, `LetterSpacing`, `GlyphScaling` in percent                   |
|  [17]   | Text and face   | `change` on `contents` and `appliedFont`                                                                      |
|  [18]   | Diacritic mode  | `change` on `diacriticPosition`                                                                               |
|  [19]   | Steps           | `app.textPreferences` `kerningKeyIncrement`, `baselineShiftKeyIncrement`, Command or Ctrl multiplying by 5    |
|  [20]   | Undo            | `commit` under `UndoModes.ENTIRE_SCRIPT`, label from request                                                  |

## [03]-[TYPOGRAPHY_SHAPING]

Open capability of `@rasm/typography` in `shaped-text.ts`, `tables.ts`, `layout.ts` and `face.ts`, general level:
- `ShapedText.of` moves each mark by `marks.y` away from its base as `yOffsetDiacritic` does, below-base marks down
  - `distilled/typesetting/inpage-typesetting-tool/position/adjuster.ts` `adjustDiacriticPosition`, `diacriticStep`
  - `distilled/internals/adobe-indesign-composers/WorldReady/diacritics.ts` `offset`, `store`
  - `distilled/internals/adobe-indesign-right-to-left/attr-menu-observers.ts` `offsetUpdate`
- `Tables` decodes GSUB alternate sets behind `aalt`, `salt` and `cvNN` for the shaped glyph of the joined form
  - `Layout` gains `cvNN` `FeatureParams` characters and labels, and a choice writes as `tag=n` over one character
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/glyph-runs-2/features.ts` `substitution`
  - `distilled/internals/adobe-indesign-right-to-left/actions.ts` `features`, `openTypeActionName`
  - `facts/adobe-indesign.txt` `FONTS`
- Stylistic sets substituting a shared glyph form rival groups from GSUB coverage `Tables` decodes, a set write unsetting its rivals
  - `distilled/typesetting/inpage-typesetting-tool/panels/features.ts` `exclusive`, `clearConflictingFeatureButtons`
  - `distilled/typesetting/inpage-typesetting-tool/suites/selection.ts` `stylisticSetStates`, `featureTag`

## [04]-[TYPOGRAPHY_JUSTIFICATION]

Open capability of `@rasm/typography` in `justification.ts` and `settings.ts`, general level:
- Tatweel width reads the advance of `Face.extenders` (JSTF `arab`) first, else U+0640, where `Justification.of` reads U+0640 alone
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/kashida.ts` `kashidaWidth`
  - `distilled/typesetting/winsoft-scribedoor/composer-justification.ts` `updateWidth`
- Kashida count, per-place length and slack form one relation solving for whichever member a caller leaves open
  - `distilled/typesetting/hasaninscripts-kashida/scopes.ts` `width`, `applyScopes`
  - `distilled/typesetting/hasaninscripts-kashida/controls.ts` `widthSteps`, `stepWidth`
  - `distilled/internals/adobe-indesign-right-to-left/attr-menu-observers.ts` `kashidaApply`
- Kashida places rank by caller rows per script style (Naskh, Nastaliq), present rows one set:
  - Rows apply before line breaking (text-level, lines reflow) or after it (fit to slack)
  - Eligibility reads tatweel advance, `SAFE_TO_INSERT_TATWEEL` flags and `jalt` coverage
  - `distilled/typesetting/hasaninscripts-kashida/justification.ts` `justifications`
  - `distilled/internals/adobe-indesign-composers/WorldReady/justification.ts` `methods`, `create`
  - `distilled/typesetting/winsoft-scribedoor/composer-justification.ts` `applyMEComposers`
  - `facts/indesign-host.json` `enumerations` `ParagraphJustificationOptions`, `KashidasOptions`
- Justification mechanisms take caller ranks (word space by adjacent script, each kashida class, `jalt`, letter space, glyph scaling)
  - Equal ranks share slack by capacity
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/justification.ts` `distribute`, `contribute`, `spacingLimits`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-hl-composer/demerits.ts` `breakDemerits`
  - `distilled/internals/adobe-indesign-composers/Justification-Panel/panel-observers.ts` `spacings`
  - `distilled/typesetting/id-extras-v-justify/spacing.ts` `limits`, `distribution`
- `Justification.of` composes a line of runs, each with its own face and `Settings`, slack shared by rank
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/justification.ts` `adjustToWidth`, `walk`, `letterSpace`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-hl-composer/widths.ts` `measureLine`
- Glyph scaling maps `spacing.glyph` to a `wdth` value inside `Face.axes` and reshapes, synthetic scale serving faces without the axis
  - Glyphs keep their horizontal scale for renderers and `Profile.of`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/glyph-runs-2/wax.ts` `scale`, `rescaled`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/justification.ts` `adjustSpacing`
  - `facts/adobe-indesign.txt` `FONTS`
- `Settings.alignment` holds start, end, center and justified with natural or forced last line, `Justification.of` keeping a natural last line
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/justification.ts` `align`, `alignedOffset`, `lastLine`
  - `distilled/internals/adobe-indesign-composers/Justification-Panel/panel-observers.ts` `alignments`, `singleWordToTarget`
  - `facts/indesign-host.json` `enumerations` `Justification`, `SingleWordJustification`
- `Settings.leading` sets line height as shaped extents times a caller ratio, paragraph space dropping at column and frame edges
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-2/line-states.ts` `leadingAnchors`, `nextLine`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/paragraph-composer-hl-composer/metrics.ts` `firstLineOffsetMetric`
  - `distilled/typesetting/id-extras-v-justify/column-justify.ts` `restoreColumn`
  - `facts/adobe-indesign.txt` `FONTS`
- Verse sets each hemistich to width w, the widest hemistich at minimum stretch, in centered columns or staggered indents `(a, M − w − a)`
  - Parallel text cuts at segment boundaries, each row as tall as its tallest column
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/styles.ts` `verseStyleSpecs`, `cycleStyle`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/conversion.ts` `convert`, `apply`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/settings.ts` `constrain`

## [05]-[TYPOGRAPHY_METRICS]

Open capability of `@rasm/typography` in `metrics.ts`, `kerning.ts` and `profile.ts`, general level:
- Size-relative choices derive from the face:
  - Underlines sit from `Metrics.underline` against the run's deepest ink
  - Character boxes take shaped extents plus caller padding
  - Footnote references take `Metrics.superscript` size and offset
  - Latin size matches alef height to cap height or x-height per face pair
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/drop-caps-context-dropcap.ts` `metricsOf`, `fontMetric`, `inkBounds`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-hl-composer/metrics.ts` `metric`
  - `facts/adobe-indesign.txt` `FONTS`, `GREP`
- `Kerning` takes a vertical shift bound and returns the `(dx, dy)` of smallest advance keeping clearance, for headline nesting
  - `distilled/typesetting/inpage-typesetting-tool/kerning/pairs.ts` `getPairKerning`, `seek`
  - `distilled/typesetting/inpage-typesetting-tool/kerning/bitmaps.ts` `bitmapsOverlap`
- `Kerning.kerns` `following` rules test the next word's first letter pair past a non-joining letter, catching gaf or keheh after و, د, ؤ or ز
  - `distilled/typesetting/inpage-typesetting-tool/spacing/tuner.ts` `nextWord`, `applyKerningToRange`
  - `distilled/typesetting/inpage-typesetting-tool/spacing/characters.ts` `isWordEndingLetter`, `isLoneSeparator`
  - `distilled/typesetting/inpage-typesetting-tool/kerning/pairs.ts` `masks`

## [06]-[TYPOGRAPHY_QURAN_SYMBOLS]

Open capability of `@rasm/typography` in `quran.ts` and `symbols.ts`, general level:
- `Surah.text` appends U+06DD then ayah digits `Digits` formats in `Settings.numberingSystem`
  - Faces drawing no enclosing mark take U+FD3E and U+FD3F around the digits
  - `distilled/typesetting/inpage-typesetting-tool/quran/glossary.ts` `insertAyahs`
  - `distilled/typesetting/inpage-typesetting-tool/symbols/panel.ts` `getSupportedUnicodeCharacter`
  - `distilled/typesetting/winsoft-scribedoor/number-conversion.ts` `numberingSystems`, `numberToString`
- `Quran` selects ayahs by surah and ayah range, juz, hizb, ruku or page through `quran-meta` per riwayah, text from `@quran.ws/text` or caller CSV
  - `distilled/typesetting/inpage-typesetting-tool/quran/glossary.ts` `parseAyahs`, `parseSurahs`, `byKey`, `findSurahNumber`
  - `distilled/typesetting/inpage-typesetting-tool/data/text.ts` `dataRows`, `cString`
- `Symbols` cells take a `0x` or `0X` prefix before their hex digits
  - `distilled/typesetting/inpage-typesetting-tool/data/text.ts` `parseHexCodePoint`
  - `distilled/typesetting/inpage-typesetting-tool/symbols/panel.ts` `loadSymbolData`

## [07]-[SEARCH]

Open capability of `@rasm/search` in `unmatched.ts`, `rank.ts` and `digits.ts`, general level:
- Ordering compares keys up to a caller delimiter through `Intl.Collator(locale, { sensitivity })`, `base` or `accent` deciding whether harakat count
  - `distilled/internals/adobe-indesign-composers/AdobeLinguistic/diacritics.ts` `strip`
  - `distilled/typesetting/winsoft-scribedoor/spelling.ts` `compare`
  - `distilled/typesetting/inpage-typesetting-tool/quran/glossary.ts` `byKey`
- Ordering and `Unmatched` matching share yeh, kaf and digit folds, `Digits.fold` holding digits alone
  - `distilled/typesetting/winsoft-scribedoor/spelling.ts` `digit`, `digits`
  - `distilled/typesetting/inpage-typesetting-tool/data/text.ts` `digitZeros`, `getAsNumber`
  - `facts/adobe-indesign.txt` `GREP`

## [08]-[AUTOMATION]

Open capability of `@rasm/automation` in `typesetting.ts`, Creative Cloud level:
- `Adjust.deltas.baselineShift` takes em, InDesign writing em × point size per style run
  - Panel divides `baselineShiftKeyIncrement` by the selection's point size
  - `distilled/typesetting/inpage-typesetting-tool/spacing/tuner.ts` `steps`, `adjustStep`
  - `distilled/typesetting/inpage-typesetting-tool/suites/selection.ts` `incrementText`, `adjustText`
  - `distilled/typesetting/inpage-typesetting-tool/services/preferences.ts` `atSpeed`
  - `facts/adobe-indesign.txt` `DOM`
- `Kerns` restores metrics kerning over its target
  - `distilled/typesetting/inpage-typesetting-tool/spacing/tuner.ts` `clearKerningInRange`

## [09]-[GRID]

Open capability of `@rasm/grid` (`graphics/grid`, no package yet), general level, with its `@rasm/indesign` reads and writes:
- Tracks solve `E = near + far + n·s + (n − 1)·g` for any member
  - `facts/adobe-indesign.txt` `GUIDES`, `LAYOUT`
- Formats derive inside, outside, start and end from `facingPages`, `pageBinding` and `page.side`, with bleed and slug offsets
  - Placement anchors and ruler sign follow reading direction
  - `distilled/typesetting/winsoft-scribedoor/reverse-layout-invert.ts` `setPageBinding`, `bindPages`, `margins`, `pageGeometry`, `mirror`
  - `distilled/typesetting/winsoft-scribedoor/direction.ts` `pageSetupReads`, `mirror`
  - `distilled/internals/adobe-indesign-right-to-left/invert-dialog.ts` `leftToRight`, `pageRange`
  - `facts/adobe-indesign.txt` `DOM`, `LAYOUT`
- Proportions and canons derive margins from a ratio and a solve target
- Baselines form a leading lattice from a start, `Settings.leading` snapping to it and field tops at a first-baseline kind
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-2/line-states.ts` `gridExtent`, `alignmentAnchors`
  - `distilled/typesetting/id-extras-v-justify/column-justify.ts` `baselineOf`, `targetBaselines`
  - `facts/indesign-host.json` `enumerations` `FirstBaseline`, `BaselineGridRelativeOption`
- Units relate point, pica, Didot and Cicero, and measurement text decodes into them
  - `distilled/typesetting/id-extras-v-justify/measurement.ts` `units`, `scales`, `parseMeasurement`, `formatMeasurement`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/fields.ts` `pointsPerMillimetre`, `measure`
  - `facts/adobe-indesign.txt` `DOM`
- Anchors align through `Document.align` and `anchoredObjectSettings`
  - `facts/adobe-indesign.txt` `LAYOUT`
  - `facts/indesign-host.json` `enumerations` `AlignOptions`, `AnchorPosition`, `AnchoredRelativeTo`
- Line widths are usable intervals per line band, the frame chord minus wrap outlines grown by their offsets
  - Host reads: `textWrapPreferences` (`textWrapMode`, `textWrapOffset`, `textWrapSide`, `contourOptions`), `ignoreWrap`, frame paths
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-hl-globals/line-state.ts` `findParcelTiles`, `parcelTiles`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/line-breaking-hl-composer/widths.ts` `availableWidth`
  - `facts/indesign-host.json` `enumerations` `TextWrapModes`, `TextWrapSideOptions`
- Story containers read `textColumnCount`, `textColumnGutter`, `insetSpacing`, `ignoreWrap` and path kind, measure `M = (W − insets − (n − 1)·g) ÷ n`
  - `distilled/typesetting/id-extras-v-justify/scope-action.ts` `columnsOf`, `frameColumns`
  - `distilled/typesetting/id-extras-v-justify/column-justify.ts` `roomOf`
  - `facts/adobe-indesign.txt` `GUIDES`
- Running heads are `TextVariable` of `MATCH_PARAGRAPH_STYLE_TYPE` with `searchStrategy` `FIRST_ON_PAGE` or `LAST_ON_PAGE` over TOC styles
  - InDesign drops ZWNJ from variable output, and Persian heads restore it
  - `facts/indesign-host.json` `enumerations` `VariableTypes`, `SearchStrategies`
- Tab stops (start, end, center, character) map to physical sides by direction, written to `tabList` through `change`
  - Decimal tab character comes from the run's numbering system through `Intl.NumberFormat` `formatToParts`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/justification.ts` `justifyAfterTab`, `tabSpacing`
  - `distilled/internals/adobe-indesign-composers/Paragraph-Composer/wax-builder-idte-wax-builder/builder.ts` `flushTabs`
  - `facts/indesign-host.json` `enumerations` `TabStopAlignment`

## [10]-[INDESIGN_SETTINGS]

Open capability of `@rasm/indesign` in `dom/settings.ts`, app level:
- `HostSettings` maps `Settings.alignment` to `justification` by direction, `RIGHT_JUSTIFIED` for right-to-left start, `FULLY_JUSTIFIED` for forced
  - `distilled/typesetting/winsoft-scribedoor/direction.ts` `textAlign`, `mirrored`, `realign`
  - `distilled/right-to-left/phinney-r2l/styles.ts` `ensureStyle`
  - `distilled/internals/adobe-indesign-composers/WorldReady/phase2-conversion.ts` `applyLeftToRight`
  - `facts/indesign-host.json` `enumerations` `Justification`
- `HostSettings` maps `Settings.leading` to `leading` and `autoLeading`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/styles.ts` `defaults`, `verseStyleSpecs`
  - `facts/adobe-indesign.txt` `DOM`, `FONTS`

## [11]-[INDESIGN_DOCUMENT]

Open capability of `@rasm/indesign` in `requests.ts`, `contract.ts` and `dom/model.ts`, app level:
- Page, spread and document targets resolve to the text of each story they hold
  - `distilled/typesetting/hasaninscripts-kashida/scopes.ts` `scopes`, `applyScopes`
  - `distilled/typesetting/hasaninscripts-kashida/page-range.ts` `parsePageRange`, `resolvePageIndex`
  - `distilled/typesetting/id-extras-v-justify/scope-action.ts` `scopeRows`, `overScope`
- Pattern style rules write `nestedGrepStyles`, `nestedStyles`, `nestedLineStyles` and `footnoteOptions`
  - `facts/adobe-indesign.txt` `GREP`
- Styles resolve by name through `itemByName`, else `add` with declared properties
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/conversion.ts` `ensure`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/styles.ts` `addStyle`, `overwriteStyle`
  - `distilled/right-to-left/phinney-r2l/styles.ts` `ensureStyle`
  - `facts/adobe-indesign.txt` `DOM`, and `facts/indesign-host.json` on bracketed style names
- Numbering reads and writes numbered lists, `footnoteOptions` and sections
  - `distilled/right-to-left/hasaninscripts-convert-numbering-and-bullets-lists-to-text/convert.ts` `scopes`, `convertedLists`
  - `distilled/right-to-left/hasaninscripts-add-prefix-word-to-selected-numbers/prefix.ts` `prefixSelected`
  - `facts/indesign-host.json` `enumerations` `ListType`, `NumberingStyle`, `FootnoteNumberingStyle`
- Notes convert between footnotes, endnotes, threaded stories and anchored frames
  - `facts/indesign-host.json` `enumerations` `EndnoteScope`, `EndnoteFrameCreate`, `FootnoteRestarting`
- Composition constraints read and write keep options and `autoSizingType`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/styles.ts` `verseStyleSpecs`
  - `facts/indesign-host.json` `enumerations` `AutoSizingTypeEnum`, `AutoSizingReferenceEnum`
- Tables walk and size through `Cell.width`, `rows.minimumHeight` and `convertToText`
  - `distilled/internals/adobe-indesign-composers/WorldReady/phase2-conversion.ts` `convertTables`, `convertTable`, `swapCell`
  - `distilled/typesetting/winsoft-scribedoor/reverse-layout-invert.ts` `storyTables`, `invertTables`
- Document settings persist through `insertLabel` and `documentPresets`
  - `distilled/typesetting/id-extras-v-justify/scope-action.ts` `storedOptions`, `insertLabels`, `persist`
  - `distilled/right-to-left/hasaninscripts-convert-line-to-verse/settings.ts` `load`, `encode`
  - `facts/adobe-indesign.txt` `DOM`, `RUNTIME`

## [12]-[ATELIER]

Open capability of `apps/creative-cloud/atelier` in `features/typesetting.ts` and `plugins/indesign/typesetting.ts`:
- Panel feature controls list the selection face's features (`ssNN` with `Layout.features` names, `titl`, `jalt`) in place of every `ss01`..`ss20`
  - `distilled/typesetting/inpage-typesetting-tool/suites/selection.ts` `stylisticSetStates`
  - `distilled/typesetting/inpage-typesetting-tool/panels/features.ts` `featureBase`
  - `distilled/internals/adobe-indesign-right-to-left/actions.ts` `featureActions`, `actionStates`
  - `facts/adobe-indesign.txt` `MENUS`, `FONTS`
- Kashida writes refuse a face with no positive U+0640 advance and no `jalt`, naming the face, where InDesign draws broken joins
  - `distilled/internals/adobe-indesign-right-to-left/attr-menu-observers.ts` `kashidaApply`
  - `distilled/internals/adobe-indesign-right-to-left/actions.ts` `features`
  - `distilled/typesetting/hasaninscripts-kashida/scopes.ts` `rejections`
- `kerning` tool offers metrics restore through `Kerns`
  - `distilled/typesetting/inpage-typesetting-tool/spacing/tuner.ts` `clearKerningInRange`
- `kerning` over a text selection restores metrics kerning across the widened words before writing, so a rerun replaces its earlier kerns
  - `distilled/typesetting/inpage-typesetting-tool/spacing/tuner.ts` `expandSelectionToWords`, `processKerningSelection`
- Panel lists the selection's characters and selects the one picked, so arrows move a single letter or mark
  - `distilled/typesetting/inpage-typesetting-tool/panels/cells.ts` `setTextSelection`, `cellTextIterator`, `hitTestCell`
  - `distilled/typesetting/inpage-typesetting-tool/services/text.ts` `selectTextCharacter`
  - `distilled/typesetting/winsoft-scribedoor/selection-manager.ts` `selectCells`, `selectTextFromData`
  - `distilled/typesetting/winsoft-scribedoor/cell-table.ts` `selectedCells`, `cursiveFormAt`
