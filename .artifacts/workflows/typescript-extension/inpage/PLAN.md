# [INPAGE_TYPESETTING_PLAN]

Goal: universal Perso-Arabic typesetting over any installed font, rebuilt as general logic at its level in `libs/typescript` and composed in `apps/creative-cloud/atelier`. Typesetting distills, InPage's first, are evidence of their principles, never of Urdu or Desi forms or one font's or product's values. Capability the distills, these rows and the research hold that the libraries lack or narrow joins as new general logic or deepens existing logic. Code growth that adds that capability stays, held to `.claude/workflows/BAR.md` like every other line, and adversaries refine new code as hard as old. Every package `docs/research/typescript-extension/packages.md` chooses serves at full capability: each reader and adversary mines package source at its catalog release against the code, hand logic a member states goes, and capability a member makes cheap joins where it belongs. `node proof/ab.ts` in `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/` is the behavior oracle.

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

## [03]-[TYPOGRAPHY]

Open capability of `@rasm/typography`, general level:
- `ShapedText.of` moves each mark by `marks.y` away from its base as `yOffsetDiacritic` does, below-base marks down
- Tatweel width reads the advance of `Face.extenders` (JSTF `arab`) first, else U+0640, where `Justification.of` reads U+0640 alone
- Kashida count, per-place length and slack form one relation solving for whichever member a caller leaves open
- Kashida places rank by caller rows per script style (Naskh, Nastaliq), present rows one set:
  - Rows apply before line breaking (text-level, lines reflow) or after it (fit to slack)
  - Eligibility reads tatweel advance, `SAFE_TO_INSERT_TATWEEL` flags and `jalt` coverage
- Justification mechanisms take caller ranks (word space by adjacent script, each kashida class, `jalt`, letter space, glyph scaling)
  - Equal ranks share slack by capacity
- `Justification.of` composes a line of runs, each with its own face and `Settings`, slack shared by rank
- Glyph scaling maps `spacing.glyph` to a `wdth` value inside `Face.axes` and reshapes, synthetic scale serving faces without the axis
  - Glyphs keep their horizontal scale for renderers and `Profile.of`
- `Tables` decodes GSUB alternate sets behind `aalt`, `salt` and `cvNN` for the shaped glyph of the joined form
  - `Layout` gains `cvNN` `FeatureParams` characters and labels, and a choice writes as `tag=n` over one character
- Stylistic sets substituting a shared glyph form rival groups from GSUB coverage `Tables` decodes, a set write unsetting its rivals
- `Settings.alignment` holds start, end, center and justified with natural or forced last line, `Justification.of` keeping a natural last line
- `Settings.leading` sets line height as shaped extents times a caller ratio, paragraph space dropping at column and frame edges
- Verse sets each hemistich to width w, the widest hemistich at minimum stretch, in centered columns or staggered indents `(a, M − w − a)`
  - Parallel text cuts at segment boundaries, each row as tall as its tallest column
- Size-relative choices derive from the face:
  - Underlines sit from `Metrics.underline` against the run's deepest ink
  - Character boxes take shaped extents plus caller padding
  - Footnote references take `Metrics.superscript` size and offset
  - Latin size matches alef height to cap height or x-height per face pair
- `Surah.text` appends U+06DD then ayah digits `Digits` formats in `Settings.numberingSystem`
  - Faces drawing no enclosing mark take U+FD3E and U+FD3F around the digits
- `Quran` selects ayahs by surah and ayah range, juz, hizb, ruku or page through `quran-meta` per riwayah, text from `@quran.ws/text` or caller CSV
- `Kerning` takes a vertical shift bound and returns the `(dx, dy)` of smallest advance keeping clearance, for headline nesting
- `Kerning.kerns` `following` rules test the next word's first letter pair past a non-joining letter, catching gaf or keheh after و, د, ؤ or ز
- `Symbols` cells take a `0x` or `0X` prefix before their hex digits

## [04]-[SEARCH]

Open capability of `@rasm/search`, general level:
- Ordering compares keys up to a caller delimiter through `Intl.Collator(locale, { sensitivity })`, `base` or `accent` deciding whether harakat count
- Ordering and `Unmatched` matching share yeh, kaf and digit folds, `Digits.fold` holding digits alone

## [05]-[AUTOMATION]

Open capability of `@rasm/automation`, Creative Cloud level:
- `Adjust.deltas.baselineShift` takes em, InDesign writing em × point size per style run
  - Panel divides `baselineShiftKeyIncrement` by the selection's point size
- `Kerns` restores metrics kerning over its target

## [06]-[INDESIGN]

Open capability of `@rasm/indesign`, app level:
- `HostSettings` maps `Settings.alignment` to `justification` by direction, `RIGHT_JUSTIFIED` for right-to-left start, `FULLY_JUSTIFIED` for forced
- `HostSettings` maps `Settings.leading` to `leading` and `autoLeading`
- Page, spread and document targets resolve to the text of each story they hold
- Pattern style rules write `nestedGrepStyles`, `nestedStyles`, `nestedLineStyles` and `footnoteOptions`
- Styles resolve by name through `itemByName`, else `add` with declared properties
- Numbering reads and writes numbered lists, `footnoteOptions` and sections
- Notes convert between footnotes, endnotes, threaded stories and anchored frames
- Composition constraints read and write keep options and `autoSizingType`
- Tables walk and size through `Cell.width`, `rows.minimumHeight` and `convertToText`
- Document settings persist through `insertLabel` and `documentPresets`

## [07]-[ATELIER]

Open capability of `apps/creative-cloud/atelier`:
- Panel feature controls list features the selection's face holds (`ssNN` with `Layout.features` names, `titl`, `jalt`) in place of every `ss01`..`ss20`
- Kashida writes refuse a face with no positive U+0640 advance and no `jalt`, naming the face, where InDesign draws broken joins
- `kerning` tool offers metrics restore through `Kerns`
- `kerning` over a text selection restores metrics kerning across the widened words before writing, so a rerun replaces its earlier kerns
- Panel lists the selection's characters and selects the one picked, so arrows move a single letter or mark

## [08]-[GRID]

Open capability of `@rasm/grid` (`graphics/grid`, no package yet), general level, with its `@rasm/indesign` reads and writes:
- Tracks solve `E = near + far + n·s + (n − 1)·g` for any member
- Formats derive inside, outside, start and end from `facingPages`, `pageBinding` and `page.side`, with bleed and slug offsets
  - Placement anchors and ruler sign follow reading direction
- Proportions and canons derive margins from a ratio and a solve target
- Baselines form a leading lattice from a start, `Settings.leading` snapping to it and field tops at a first-baseline kind
- Units relate point, pica, Didot and Cicero, and measurement text decodes into them
- Anchors align through `Document.align` and `anchoredObjectSettings`
- Line widths are usable intervals per line band, the frame chord minus wrap outlines grown by their offsets
  - Host reads: `textWrapPreferences` (`textWrapMode`, `textWrapOffset`, `textWrapSide`, `contourOptions`), `ignoreWrap`, frame paths
- Story containers read `textColumnCount`, `textColumnGutter`, `insetSpacing`, `ignoreWrap` and path kind, measure `M = (W − insets − (n − 1)·g) ÷ n`
- Running heads are `TextVariable` of `MATCH_PARAGRAPH_STYLE_TYPE` with `searchStrategy` `FIRST_ON_PAGE` or `LAST_ON_PAGE` over TOC styles
  - InDesign drops ZWNJ from variable output, and Persian heads restore it
- Tab stops (start, end, center, character) map to physical sides by direction, written to `tabList` through `change`
  - Decimal tab character comes from the run's numbering system through `Intl.NumberFormat` `formatToParts`
