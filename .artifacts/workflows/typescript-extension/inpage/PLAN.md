# [INPAGE_TYPESETTING_PLAN]

Rebuild of the InPage Typesetting Tool capability into `libs/typescript`, from base `5e8dc3f1e`. Logic source: `docs/research/applications/indesign/distilled/typesetting/inpage-typesetting-tool/` (27 files), its study `docs/research/typescript-extension/studies/inpage-typesetting-tool.md`, and the A/B harness `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/proof/ab/`. Rules: `TODO.md` [02], `.claude/workflows/TYPESCRIPT.md`, Skill(typescript-coding), Skill(typescript-coding-effect), Skill(uxp-coding), Skill(clean-prose). Where this plan and `docs/research/typescript-extension/ARCHITECTURE.md` differ, this plan wins for the files it names. The landing graph is [13] itself: package ids are its indexes, `section` is the exact heading, `files` are repository paths, and the workflow's Plan phase does not run.

## [01]-[DECISIONS]

1. Capability is rebuilt at four levels, each file one concept under one export, and no file holds a name, width, height, style, step or table an InPage face, data file, widget, licence or Windows API supplied
2. General level (`@rasm/typography`, `@rasm/search`) holds no host type: Unicode through ICU4X, shaping and outlines through HarfBuzz, tables through fontkit, CSV through Effect `Csv`, digits through `Intl`
3. Creative Cloud level (`@rasm/automation/typesetting`) holds the request contracts every Adobe text host can serve, in em and points, naming no host member, and imports `@rasm/typography/settings`, an entry that imports `effect` alone
4. InDesign level (`@rasm/indesign`) holds one bidirectional codec between host members and `Settings`, and handlers writing through `commit`, one undo step each, inside the UXP plugin; every write uses the host's own mechanism for that capability ([03])
5. Atelier composes: tool prose, the spacing pass over a story (host reads, Node shaping and kerning, host writes, MCP progress), and the UXP panel; InPage-derived tuning rows and data layouts are caller arguments, never defaults in code
6. Related quantities sit in one parameterized relation that solves for the member a caller leaves open: ink profiles and the pair gap (`Kerning`), slack across mechanisms with kashida count, grade and width (`Justification`), digit value and glyph per numbering system (`Digits`), step times multiplier from host increments (panel)
7. Kashida is the host's own justification over any installed face: `insertion` none, tatweel (U+0640 inserted) or stretched (connections elongated), `length` none, short, medium or long, `fractional`, `alternates` (`jalt`); InPage's private attributes `0x50f3e` and `0x1bca` have no counterpart and stay out
8. Nuqta and aerab adjustment is one mark offset in em per axis applied to glyphs of GDEF class `MARK`, written to `xOffsetDiacritic` and `yOffsetDiacritic`; letter adjustment is kerning, tracking and baseline-shift deltas, kerning placed at the insertion point after the base letter and before its marks
9. Pair kerning computes from outline geometry: per-band ink profiles of adjacent shaped runs in font units, a slanted wall slid to contact and folded into the following run's profile, kern = minimum band gap capped at the following run's advance; bitmaps, byte-shift quantization, thresholds, penalties and bonuses are InPage artifacts and go
10. Text carving is stylistic sets `ss01`..`ss20` of the face's GSUB applied through the host's `otfStylisticSets`; the vendor page's "twenty styles" names this set
11. Quran glossary and symbol sets decode from caller-supplied CSV and TSV text with caller-named columns, through Effect `Csv`, and filter by face coverage
12. Hand code no package states is declared in `.claude/workflows/TYPESCRIPT.md` [01]: outline flattening and band profiles (harfbuzzjs family), kashida priority rows (icu family), the feature-tag table (InDesign family)
13. Modules instantiating wasm at import (`harfbuzzjs`, `icu`) load in Node alone; `@rasm/indesign` imports from `@rasm/typography` only `./settings` and `./source`
14. No test files are added; `node proof/ab.ts` stays the oracle and gains a library column where the library keeps InPage behavior
15. Galley has no files and gains none; the UXP panel in `apps/creative-cloud/atelier/plugins/indesign` is the human interface, its step sizes read from `app.textPreferences`
16. ARCHITECTURE [03.6] progress joins as `@rasm/mcp/progress` for the spacing pass; state sealing, proxy, paging and persisted caches stay out of this landing
17. `papaparse` and `@types/papaparse` leave the catalog: no source imports them

## [02]-[CAPABILITIES]

Distilled products read together reveal capabilities no single product states. Level: U is universal TypeScript, CC Creative Cloud, ID InDesign. Rows this plan builds name their file; the others land with their own rows (grids, footnotes, tables) and are listed so InPage functions are placed as uses, not rebuilt apart.

### [02.1]-[BUILT_HERE]

Lattice stepping (U relation, ID step source): `v' = v + k·s`, `s` from the host's keyboard increments, `k` a multiplier
- Host: `app.textPreferences.kerningKeyIncrement`, `baselineShiftKeyIncrement`, `leadingKeyIncrement`, `viewPreferences.cursorKeyIncrement`
- Uses: InPage `spacing/tuner.ts` `adjustStep`, `position/adjuster.ts` `diacriticStep`, `adjustDiacriticPosition`, `suites/selection.ts` `adjustText`, `incrementText`, `adjustFrames`, `services/preferences.ts` `atSpeed`, `trackingSpeed`
- Uses: grids `bncrrthrs` `baseline.ts`, `rolanddreger` `baseline.ts` `keydownStep`, `designersbookshop` `units.ts` `keyDown`, `keyUp`
- Here: `plugins/indesign/typesetting.ts` computes `k·s`, `requests/adjust.ts` applies the delta

Text segmentation (U): words, joining runs, marks, graphemes from Unicode properties
- Here: `text/words.ts` `Word`
- Uses: InPage `spacing/characters.ts` (all four), `spacing/tuner.ts` `nextWord`, `expandSelectionToWords`, `scripts/extendscript.ts` `updateKerning`, `services/text.ts` `tokenizeString`
- Uses: footnotes `brackets-pro/scan.ts`, `footnotes-cleaner/scan.ts`, `endnotes-number-generator/caret.ts`, `swap-footnote-position/punctuation.ts`, right-to-left `add-prefix-word/prefix.ts`

Pair spacing pass (U geometry, ID write): adjacent units, a delta from glyph geometry or a rule, one write per pair in one commit
- Here: `composition/profile.ts` `Profile`, `composition/kerning.ts` `Kerning`, `requests/kerns.ts`
- Host: `InsertionPoint.kerningValue`, `kerningMethod` read for `Manual` and `Metrics`
- Uses: InPage `kerning/pairs.ts`, `kerning/bitmaps.ts`, `kerning/render.ts`, `kerning/cache.ts`, `kerning/json.ts`, `spacing/tuner.ts` `applyKerningToRange`, `clearKerningInRange`, `processKerningSelection`, `services/transfers.ts` `downloadFontKerningData`
- Uses: footnotes `brackets-pro/remove.ts` `spacingPass`, `normalizeSpaceAfterMarker`

Range scoping (U page lists, ID resolution): selection, story, frame, table cell, pages, document resolve to text ranges
- Here: `Word.expand` widens a selection to words; existing `Specifier`, `FindChange.scope`
- Uses: InPage `spacing/tuner.ts` `processKerningSelection`, `expandSelectionToWords`, `suites/selection.ts` `walkFrames`, `reached`, `hasSelectedText`, `selectedCellText`, `hasCellText`
- Uses: footnotes `footnote-manager/run.ts`, grids `fit-text-frames-to-margins-pro/scopes.ts`, styles `apply-object-style/page-scope.ts`, tables `table-direction/pages.ts`

Insertion in a face with coverage (U coverage, ID write)
- Here: `Face.has`, `Symbols.supported`; the write is `change` on `contents` and `appliedFont`
- Uses: InPage `suites/selection.ts` `insertText`, `applyFont`, `symbols/panel.ts` `insertSelectedSymbol`, `getSupportedUnicodeCharacter`, `quran/glossary.ts` `insertAyahs`, `services/text.ts` `selectTextCharacter`
- Uses: internals `adobe-indesign-right-to-left/actions.ts` `insertCharacter`, footnotes `footnote-manager/import.ts`

Feature state per face (U face side, ID selection side): features a face holds, intersected with the selection's state
- Here: existing `Layout.features` (tags with `ssNN` names), `Settings.features`, `requests/typeset.ts`
- Host: `otfStylisticSets`, `otfTitling`, `otfContextualAlternate`, `otfDiscretionaryLigature`, `otfStylisticAlternate`, `otfJustificationAlternate`, `capitalization`, `opentypeFeatures`
- Uses: InPage `suites/selection.ts` `featureTag`, `stylisticSetStates`, `panels/features.ts` (all), `scripts/extendscript.ts` `smallCapsState`, `toggleSmallCaps`, `services/text.ts` `getBooleanTextAttributeState`, `setBooleanTextAttribute`
- Uses: internals `adobe-indesign-right-to-left/actions.ts` `openTypeActionName`, `actionStates`, right-to-left `phinney-r2l/styles.ts`

Mark positioning (U offsets, ID members): one em offset per axis on mark glyphs
- Here: `Settings.marks`, `Shaped.offsetMarks`, `requests/adjust.ts`
- Host: `xOffsetDiacritic`, `yOffsetDiacritic`, `diacriticPosition` through `change`
- Uses: InPage `position/adjuster.ts` (all), internals `attr-list-observers.ts`, `attr-menu-observers.ts` `offsetConstruct`, `offsetUpdate`

Digits per numbering system (U): value and glyph per system from `Intl`
- Here: `data/search/digits.ts` `Digits`
- Uses: InPage `data/text.ts` `getAsNumber`, `parseHexCodePoint`, `digitZeros`, `services/text.ts` `runScriptAsUnsigned16`; right-to-left `add-prefix-word/prefix.ts`; footnotes `page-range.ts` `digitsOnly` (both products)

Direction, digits and kashida as settings (U settings, ID codec)
- Here: `composition/settings.ts` `Settings`, `documents/settings.ts` `HostSettings`
- Host: `paragraphDirection`, `characterDirection`, `digitsType`, `kashidas`, `paragraphJustification`, `paragraphKashidaWidth`, `otfJustificationAlternate`, spacing members
- Uses: InPage `scripts/extendscript.ts` `applyLanguage` (becomes `change appliedLanguage` on root styles), right-to-left `phinney-r2l/direction.ts`, tables `table-direction/writes.ts`

Data rows from files (U): CSV and TSV through Effect `Csv` with header-named columns
- Here: `text/quran.ts` `Quran`, `text/symbols.ts` `Symbols`
- Uses: InPage `data/text.ts` `textLines`, `dataRows`, `cString`, `quran/glossary.ts` `loadQuranData`, `symbols/panel.ts` `loadSymbolData`, `services/transfers.ts` (all); footnotes `footnote-manager/import.ts`

Long host passes with progress (CC): one request reporting progress to its caller
- Here: `ai/mcp/progress.ts` `progress`
- Uses: InPage `spacing/tuner.ts` `RangeProgressBar` calls, grids `designersbookshop` `updates.ts`

### [02.2]-[OTHER_ROWS]

Listed with level and host mechanism so no later row rebuilds an InPage use apart:
- Track relation (U, `@rasm/grid`): `E = near + far + n·s + (n−1)·g`, solved for any member; grids products, footnotes `footnotes-columns/add.ts`, tables `resize.ts`
- Anchor alignment (U relation, ID `Document.align`, `anchoredObjectSettings`): grids `fit-text-frames`, `move-and-resize`, footnotes `sidenotes/stack.ts`
- Box algebra and binding side (U `Format`, ID `Page.side`, bleed and slug offsets): grids `pchiorean-indentz`, `gitbruno-novelty`, `id-extras`
- Size, leading and baseline (U `Baseline`, `Metrics` gaining cap height and x-height): grids `ajabon`, `kahrel-page-setup`, `rolanddreger`
- Proportions, canons, formats (U `@rasm/grid`): grids `fitbaseline`, `thisischrisswift`, `caroljpeg`, `designersbookshop` `page-sizes.ts`
- Live style rules by pattern (ID `nestedGrepStyles`, `nestedStyles`, `nestedLineStyles`, `footnoteOptions`): footnotes `brackets-pro`, `apply-styles-to-footnotes`, `footnote-breakline`
- Styles by name with declared properties (ID `itemByName`, `add`): footnotes, tables `table-dynamic-numbering/numbering.ts`, grids `style-writes.ts`
- Numbering and counters (ID numbered lists, `footnoteOptions`, sections): footnotes `kahrel-footnotes-columns`, `kahrel-sidenotes`, tables numbering, InPage `quran/glossary.ts` `populateAyahNumbers`, `updateAyahCount` (chrome)
- Units and measurement text (ID `MeasurementUnits` under `preserve`): every hand unit table in grids, tables and `kahrel-sidenotes/units.ts`
- One-undo write batch (ID `commit`, `preserve`): every `UndoModes.ENTIRE_SCRIPT` row and preference save-restore
- Note representation conversion (ID footnotes, threaded stories, anchored frames, endnotes): footnotes products
- Composition constraints (U breaking penalties, ID `autoSizingType`, keep members): footnotes `footnote-space`, `split-footnotes`, `footnotes-columns`
- Table walk and sizing (ID `Cell.width`, `rows.minimumHeight`, `convertToText`): tables products
- Document-persisted settings (ID `insertLabel`, `documentPresets`): grids presets, InPage `services/preferences.ts`, `kerning/cache.ts` (chrome)

## [03]-[HOST_MECHANISMS]

Each InDesign-level write uses the member InDesign states for that capability, never a find and change pass or a private attribute:

| [INDEX] | [CAPABILITY]             | [MECHANISM]                                                                                 |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | Pair kern                | `InsertionPoint.kerningValue`, thousandths of an em, negative tightens                      |
|  [02]   | Restore font kerning     | `kerningMethod` written as `$ID/Metrics`                                                    |
|  [03]   | Tracking, baseline shift | `tracking` thousandths of an em, `baselineShift` points                                     |
|  [04]   | Mark offsets             | `xOffsetDiacritic`, `yOffsetDiacritic`, unit per the dictionary row                         |
|  [05]   | Kashida                  | `kashidas`, `paragraphJustification`, `paragraphKashidaWidth`, `otfJustificationAlternate`  |
|  [06]   | Stylistic sets           | `otfStylisticSets` bitmask, bit n−1 for `ss<n>`                                             |
|  [07]   | Other features           | `otfTitling`, `otfContextualAlternate`, `otfDiscretionaryLigature`, `otfStylisticAlternate` |
|  [08]   | Small caps               | `capitalization` `CAP_TO_SMALL_CAP` for `c2sc` with `smcp`, `SMALL_CAPS` for `smcp`         |
|  [09]   | Remaining tags           | `opentypeFeatures` tag and value pairs                                                      |
|  [10]   | Digits                   | `digitsType`                                                                                |
|  [11]   | Direction                | `paragraphDirection`, `characterDirection`                                                  |
|  [12]   | Spacing ratios           | `minimum`, `desired`, `maximum` × `WordSpacing`, `LetterSpacing`, `GlyphScaling`            |
|  [13]   | Text and face insertion  | `change` on `contents` and `appliedFont`                                                    |
|  [14]   | Diacritic position mode  | `change` on `diacriticPosition`                                                             |
|  [15]   | Step sizes               | `app.textPreferences.kerningKeyIncrement`, `baselineShiftKeyIncrement`                      |
|  [16]   | Undo                     | `commit` under `UndoModes.ENTIRE_SCRIPT`, label from the request                            |

## [04]-[LEVELS]

```text
libs/typescript/graphics/typography/            @rasm/typography, general
├── error.ts           TypographyError                  new
├── face.ts            Face                             rebuilt (has, markClass, extenders, outline)
├── shaped.ts          Shaped                           rebuilt (class, advance, offsetMarks)
├── source.ts, metrics.ts, layout.ts, color.ts, names.ts   kept
├── tables.ts          Tables                           rebuilt (GDEF, JSTF)
├── tags.ts            Tags                             rebuilt (rename from Settings)
├── text/
│   ├── unicode.ts     Unicode                          new, ICU4X service
│   ├── words.ts       Word                             new, words and joining runs
│   ├── quran.ts       Quran                            new, glossary
│   └── symbols.ts     Symbols                          new, symbol sets by coverage
└── composition/
    ├── settings.ts    Settings                         new, effect only
    ├── profile.ts     Profile                          new, band ink profiles
    ├── kerning.ts     Kerning                          new, pair kern and pass
    └── justification.ts   Justification                new, kashida and one-line fit
libs/typescript/data/search/digits.ts           Digits, general, new
libs/typescript/ai/mcp/progress.ts              progress, Creative Cloud composition support, new
libs/typescript/creative-cloud/automation/typesetting.ts   Typesetting, Adjust, Kerns, Typeset, new
libs/typescript/creative-cloud/indesign/
├── documents/settings.ts   HostSettings                new, host members ↔ Settings
├── documents/text.ts       ParagraphAttributes, Run    rebuilt in the members this plan owns
├── requests/adjust.ts      adjust                      new
├── requests/kerns.ts       kerns                       new
├── requests/typeset.ts     typeset                     new
└── host.ts                 Contract, host              rebuilt (merge, local)
libs/typescript/creative-cloud/uxp/entrypoints.ts   plugin panels receive their root node, rebuilt
apps/creative-cloud/atelier/
├── features/typesetting.ts   spacing, glossary, symbols tools and prose   new
├── features/fonts.ts         font tool on the new Shaped and Justification   rebuilt
├── features/plugins.ts       registerRpcGroup rows adjust, kerns, typeset   rebuilt
├── main.ts                   registers typesetting                         rebuilt
└── plugins/indesign/
    ├── main.ts               panel entry                                   rebuilt
    ├── typesetting.ts        panel controls                                new
    └── uxp.config.ts         panel entrypoint row                          rebuilt
```

## [05]-[PACKAGES]

Catalog rows in `pnpm-workspace.yaml` and root `package.json`, newest releases on 2026-11-02:

| [INDEX] | [ROW]              | [VERSION] | [ACTION]                                                       |
| :-----: | :----------------- | :-------- | :------------------------------------------------------------- |
|  [01]   | `icu`              | 2.4.0     | Add under new `# --- [TYPOGRAPHY]` after `[SPATIAL_RENDERING]` |
|  [02]   | `harfbuzzjs`       | 1.6.4     | Move from `[ADOBE_HOSTS_FONTS]` to `[TYPOGRAPHY]`              |
|  [03]   | `fontkit`          | 2.0.4     | Move to `[TYPOGRAPHY]`                                         |
|  [04]   | `@types/fontkit`   | 2.0.9     | Move to `[TYPOGRAPHY]`                                         |
|  [05]   | `papaparse`        | 5.7.0     | Remove from catalog and root `package.json`                    |
|  [06]   | `@types/papaparse` | 5.5.2     | Remove from catalog and root `package.json`                    |

Members used, confirmed from installed declarations or, for `icu`, the published 2.4.0 sources:
- `icu` 2.4.0 (ESM, top-level `await` instantiating `icu_capi.wasm`): `JoiningType.forChar`, `JoiningGroup.forChar`, `GeneralCategory.forChar`, `BidiClass.forChar`, `Script`, `ScriptWithExtensions` (`hasScript`, `asBorrowed.getScriptExtensionsVal`), each value's `value` name
- `harfbuzzjs` 1.6.4: `Buffer.addText`, `setDirection`, `setScript`, `setLanguage`, `setFlags`, `setClusterLevel`, `getGlyphInfos`, `getGlyphPositions`, `shape(font, buffer, features)`, `Font.glyphToJson`, `glyphHAdvance`, `nominalGlyph` (`number | undefined`), `glyphExtents`, `Face.getGlyphClass`, `GlyphClass`, `GlyphFlag.SAFE_TO_INSERT_TATWEEL`, `BufferFlag.PRODUCE_SAFE_TO_INSERT_TATWEEL`, `Feature`, `Variation`
- `fontkit` 2.0.4 through `create(Buffer.from(...))` as today (Node entry): lazy getters `GDEF` (`markAttachClassDef` formats 1 and 2), `JSTF` (`scriptList` rows with `extenderGlyphs`), decoded through `Tables`
- `effect` 4.1.1: `effect/encoding` `Csv.decodeString({ delimiter, headers: true })` with `Stream.pipeThroughChannel`, `CsvError`; `Context.Service`, `Schema.Class`, `Schema.TaggedError`, `Schema.TaggedUnion`, `Schema.withConstructorDefault`, `SchemaTransformation.transform`, `Match`, `Array.window`, `Array.groupBy`, `Effect.validate`, `Cache`, `ManagedRuntime`
- `effect/ai`: `McpRequestContext` (`progressToken`), `McpSchema.ProgressNotification`, the `McpServer` notification broadcaster
- `effect/rpc`: `RpcGroup.merge`, `Rpc.make`, `RpcTest.makeClient`
- `Intl.supportedValuesOf('numberingSystem')`, `Intl.NumberFormat(locale, { numberingSystem }).formatToParts`
- `indesign-uxp-types` 1.0.0 members of [03], enumerations `ParagraphJustificationOptions` (`NASKH_TATWEEL_JUSTIFICATION`, `_FRAC`, `NASKH_KASHIDA_JUSTIFICATION`, `_FRAC`), `KashidasOptions` (`DEFAULT_KASHIDAS`, `KASHIDAS_OFF`), `DigitsTypeOptions`, `DiacriticPositionOptions`, `Capitalization`

Rejected:

| [INDEX] | [OPTION]                                                                                               | [REASON]                                                                     |
| :-----: | :----------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `@resvg/resvg-js`, `skia-canvas`, `@napi-rs/canvas`                                                    | Native binaries, results depend on scan conversion                           |
|  [02]   | `canvaskit-wasm` 0.42.0                                                                                | 7 MB wasm and async init for path operations a profile pass answers          |
|  [03]   | `pureimage`, `@thi.ng/rasterize`                                                                       | Even-odd fill alone, one simple polygon alone                                |
|  [04]   | `clipper2-ts` 2.0.1-18                                                                                 | Needs flattened input; the gap question needs no boolean operation           |
|  [05]   | `polygon-clipping`, `martinez-polygon-clipping`, `@flatten-js/core`, `bezier-js`, `svg-path-commander` | Stale, untyped, or without curves and fill rules                             |
|  [06]   | `csv-parse`, `d3-dsv`, `papaparse`                                                                     | Node `Buffer` at load, stale, UMD; Effect `Csv` decodes with a tab delimiter |
|  [07]   | `bekesh`, `ar-justify`, `@nagwa-limited/kashida-engine`                                                | DOM measurement, hand Unicode 17 tables                                      |
|  [08]   | InPage `kern.dat` codec, server JSON, embedded fonts                                                   | Vendor file, endpoint and GDI loading; pair tables cache in the consumer     |

## [06]-[TYPOGRAPHY]

`@rasm/typography` `package.json` (edited in [13.1]): `exports` become `./face`, `./shaped`, `./source`, `./settings`, `./unicode`, `./words`, `./kerning`, `./justification`, `./quran`, `./symbols`; `dependencies` gain `icu`. Entries other than `./settings` and `./source` load in Node alone. Each file names its concept, exports, the distilled and existing symbols it replaces, and what derives.

### [06.1]-[ERROR_AND_FACE]

`error.ts`, export `TypographyError` (`Schema.TaggedError`): `subject`, `cause`, raised for unreadable font bytes alone; replaces `Cause.UnknownError` at `face.ts` L55. WOFF and WOFF2 keep `Cause.IllegalArgumentError` (TYPESCRIPT.md rule 7).

`face.ts`, export `Face`:
- `Face.open(source, instance)` keeps its selection logic and `create(Buffer.from(...))`
- `Face.has(codePoint): boolean` is `font.nominalGlyph(codePoint) !== undefined`; replaces distilled `symbols/panel.ts` `getSupportedUnicodeCharacter` coverage test
- `Face.markClass(glyph): Option<number>` from decoded `GDEF.markAttachClassDef` (format 1 `startGlyph` and `classValueArray`, format 2 `classRangeRecord`)
- `Face.extenders: readonly number[]` from the `JSTF` script row tagged `arab`, else empty
- `Face.outline(glyph)` returns commands in font units (today's division by `unitsPerEm` goes), consumed by `Profile`
- `markClass`, `extenders` and `outline` read values built in `open` and omitted on encode like `font`
- Derives: coverage from the cmap through HarfBuzz, mark classes and extenders from tables, no character set or glyph list is copied

### [06.2]-[TABLES_SHAPED_TAGS]

`tables.ts`, export `Tables`: gains optional `GDEF` (`glyphClassDef`, `markAttachClassDef` both formats) and `JSTF` (`scriptList` rows with `tag`, `extenderGlyphs`), decoded from fontkit's untyped getters through `Schema`.

`tags.ts`, export `Tags`: `Tags.Tag` (today's `Tag`) and `Tags.list(host, description)` (today's `Settings` factory); `face.ts`, `shaped.ts` and `settings.ts` import it; harfbuzzjs stays `import type`.

`shaped.ts`, export `Shaped`:
- `Shaped.Run` and `Shaped.of(face, run)` keep their signatures
- Each glyph gains `class: Schema.Literals(Struct.keys(Struct.omit(GlyphClass, [])))` from `face.font.face.getGlyphClass`
- `Shaped.advance` getter sums `xAdvance`
- `Shaped.offsetMarks(delta: { x: number; y: number }): Shaped` adds em offsets to glyphs with `class` `MARK`; replaces distilled `position/adjuster.ts` general half
- Replaces distilled `kerning/render.ts` `renderKerningBitmap` (advance sum, cluster offsets); Uniscribe and GDI calls go

### [06.3]-[UNICODE]

`text/unicode.ts`, export `Unicode`: `class Unicode extends Context.Service<Unicode, typeof import('icu')>()('@rasm/typography/text/unicode')` with `static layer = Layer.effect(Unicode, Effect.promise(() => import('icu')))`. Consumers read `JoiningType`, `JoiningGroup`, `GeneralCategory`, `BidiClass`, `Script`, `ScriptWithExtensions` directly. Replaces the hand classes of distilled `spacing/characters.ts`, the 44-mark list behind `scripts/extendscript.ts` `updateKerning`, and `data/text.ts` `digitZeros`.

### [06.4]-[WORD]

`text/words.ts`, export `Word` (`Data.Class`): `start`, `end` (UTF-16), `arabic: boolean`, `runs: readonly { start: number; end: number }[]`, `spacesBefore`, `breakBefore`
- `Word.of(text: string): Effect<readonly Word[], never, Unicode>`
- Code points are Arabic when `ScriptWithExtensions.hasScript(cp, Script.Arab)`; a word spans Arabic code points between spaces and separators
- Runs end after an Arabic code point of `JoiningType` `RightJoining` or `NonJoining` plus its following `Transparent` code points; `JoinCausing` continues; ZWNJ ends a run
- Separators: U+0020, U+000D, U+000A, U+2029; `breakBefore` marks a paragraph separator before the word
- `Word.expand(text, first, last): [number, number]` widens a range to word boundaries at the same separators, pure, no service
- Replaces distilled `spacing/tuner.ts` `nextWord`, `expandSelectionToWords`, eligibility tests, `spacing/characters.ts` (all), `services/text.ts` `tokenizeString`
- Derives: classes from ICU4X; `uint16` truncation, `runCapacity`, Urdu-only letters and the Latin digit exceptions go

### [06.5]-[QURAN]

`text/quran.ts`, export `Quran` (`Schema.Class`): `surahs: Array({ number: Int, name: String, ayahs: Array({ number: Int, text: String }) })` sorted by number
- `Quran.Columns` (`Schema.Struct`): `ayahs: { surah, number, text }`, `surahs: { number, name }`, each a header name the caller states
- `Quran.decode(files: { ayahs: string; surahs: string }, columns): Effect<Quran, CsvError | Schema.SchemaError | Unmatched>`
- Pipeline per file: `Stream.make(text).pipe(Stream.pipeThroughChannel(Csv.decodeString({ delimiter: ',', headers: true })), Stream.runCollect)`, rows as records keyed by header; a header the columns name but the file lacks is `Unmatched` over the header row
- Numbers decode through `Digits.fold` then `Schema.NumberFromString`; rows group by surah with `Array.groupBy`; the first name per surah number wins
- `Quran.surah(query: string): Effect<Surah, Unmatched>` through `Unmatched.single` over `[name, String(number)]`
- `Quran.text(surah, ayah: number | 'all', separator: string): Effect<string, Unmatched>`, `Unmatched` on an ayah number the surah lacks
- Replaces distilled `quran/glossary.ts` `loadQuranData`, `findSurahNumber`, the join of `insertAyahs`; `data/text.ts` `textLines`, `dataRows`, `cString`, `getAsNumber`
- Derives: columns from the header row and the caller's names; NUL cuts, `PMString`, panel strings and the `\r` separator go

### [06.6]-[SYMBOLS]

`text/symbols.ts`, export `Symbols` (`Schema.Class`): `sets: Schema.Record(Schema.String, Schema.Array(CodePoint))` keyed by header name
- `CodePoint` = `Schema.String.pipe(Schema.decodeTo(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 0x10ffff })), SchemaTransformation.transform({ decode: (cell) => Number.parseInt(cell, 16), encode: (point) => point.toString(16) })))`
- `Symbols.decode(file: string): Effect<Symbols, CsvError | Schema.SchemaError>` through `Csv.decodeString({ delimiter: '\t', headers: true })`, empty cells dropped before decoding
- `Symbols.supported(face: Face): Symbols` keeps code points `face.has`
- `Symbols.text(codePoint): string` through `String.fromCodePoint`
- Replaces distilled `symbols/panel.ts` `loadSymbolData`, `getSupportedUnicodeCharacter`; `data/text.ts` `parseHexCodePoint`; `panels/cells.ts` symbol grid goes (chrome)

### [06.7]-[SETTINGS]

`composition/settings.ts`, export `Settings` (`Schema.Class`, imports `effect` and `#/tags`, loads in UXP), every field under `Schema.withConstructorDefault` so `Settings.make({})` is the default:
- `direction: Option<'rtl' | 'ltr'>`
- `numberingSystem: Option<String>`, an `Intl` numbering system id
- `kashida: { insertion: 'none' | 'tatweel' | 'stretched', length: 'none' | 'short' | 'medium' | 'long', fractional: Boolean, alternates: Boolean }`
- `marks: { x: Finite, y: Finite }` in em
- `features: Array(Tags.Tag)` enabled OpenType features
- `spacing: { word, letter, glyph }` each `{ min, desired, max }`: word as ratios of the space advance (default 1), letter as em offsets (default 0), glyph as ratios (default 1)
- Host-only facts (`diacriticPosition`, `kerningMethod`, composer) stay in `@rasm/indesign` and reach the host through `change`
- Replaces distilled `position/adjuster.ts` axis facts, `services/preferences.ts` speed words (deleted: steps are caller values)

### [06.8]-[PROFILE]

`composition/profile.ts`, export `Profile` (`Data.Class`): `unitsPerEm`, `left: readonly Option<number>[]`, `right: readonly Option<number>[]` per band, `advance: number`, all in font units
- Band k covers `[k, k + 1)` font units from the face's `descender` to `ascender`, the same edges for every run of one face
- `Profile.of(face, shaped)`: places `face.outline` of each glyph at its position, flattens `Q` and `C` commands by de Casteljau subdivision to a half-unit tolerance, takes per band the minimum and maximum x of segments clipped to the band
- `Profile.wall(profile, wall: { x: number; top: number; foot: number })`: line `w(y) = s + x·(y − foot)/(top − foot)`, `s` the minimum over inked bands in `[foot, top]` of `left(y) − x·(y − foot)/(top − foot)` minus one unit; `left` becomes `min(left, w)`
- `Profile.gap(before, after): Option<number>`: `after` placed at `before.advance`, gap the minimum over bands inked on both sides of `before.advance + after.left − before.right`, none when no band has both
- Hand code declared under TYPESCRIPT.md [01] harfbuzzjs family
- Replaces distilled `kerning/bitmaps.ts` (all) and the slide loops of `kerning/pairs.ts` `getPairKerning`; GDI stride, byte-shift granularity and the fixed canvas go

### [06.9]-[KERNING]

`composition/kerning.ts`, export `Kerning` (`Data.Class`): `index: number` (UTF-16 insertion index), `before: string`, `after: string`, `em: number` signed, negative tightens
- `Kerning.Rule` (`Schema.Struct`): `side: 'before' | 'after'`, `groups: Array(String)` of ICU4X `JoiningGroup` value names, `wall: { x, top, foot }` in em, `correction: Finite`
- `Kerning.Rules` (`Schema.Struct`): `rows: Array(Rule)`, `otherwise: Rule.omit('side', 'groups')`; `Kerning.Rules.make({})` is a vertical wall at the following run's left bound with zero correction
- `Kerning.rule(rules, before, after): Effect<Rule, never, Unicode>`: first row whose `groups` holds the `JoiningGroup` of the preceding run's first or second base letter (`side` `before`) or the following run's last base letter (`side` `after`), else `otherwise`
- `Kerning.pair(face, rules): (before: string, after: string) => Effect<Option<number>, never, Unicode>`: shapes both runs with `Shaped.of`, builds profiles, applies the rule's wall, takes `Profile.gap`, caps at the following run's advance, adds `correction`, clamps negatives to zero, returns em
- `Kerning.of(text, pair): Effect<readonly Kerning[], never, Unicode | R>`: words through `Word.of`; for each adjacent pair of Arabic runs across at most one space, `pair(before, after)`; `index` is the following run's start; `em` is the negated gap
- Punctuation words of one code point count as a preceding run
- Replaces distilled `kerning/pairs.ts` `getPairKerning`, `masks`, `tuning`; `kerning/cache.ts` lookups; `spacing/tuner.ts` `applyKerningToRange` segmentation, eligibility and position
- Derives: letters from `JoiningGroup`; the InPage rows, thresholds, penalty, bonus and cap factors are quantization artifacts and exist only in the harness adapter; face-name tests, `automaticKerning`, the progress bar and `kern.dat` go; the manual-kern skip is the host's

### [06.10]-[JUSTIFICATION]

`composition/justification.ts`, export `Justification` (`Data.Class`): `run: typeof Shaped.Run.Type` after insertion, `shaped: Shaped`, `slack: number`, `mechanisms: readonly { kind: 'word' | 'kashida' | 'letter' | 'glyph'; amount: number }[]`
- `Justification.Place` (`Data.Class`): `cluster`, `priority`, `group`
- `Justification.places(text, shaped): Effect<readonly Place[], never, Unicode>`: clusters flagged `SAFE_TO_INSERT_TATWEEL` ranked by joining-group priority rows (user kashida; after initial or medial Seen or Sad; before final TehMarbuta, Heh or Dal; before final Alef, Tah, Lam, Kaf or Gaf; before medial Beh preceding Reh, Yeh or Alef Maqsurah; before final Waw, Ain, Qaf or Feh; any other final join; no final Yeh except after Seen), ties toward word end, one place per word
- `Justification.width(face): Option<number>`: advance of `face.extenders[0]`, else of `nominalGlyph(0x640)`
- `Justification.fit({ slack, width, grade, fractional, count? })` is one relation: `slack = count × grade × width`, solving `count` when absent (floor unless `fractional`) or `slack` when `count` is given; `grade` is the ordinal of `length` (short 1, medium 2, long 3)
- `Justification.justify(face, run, width, settings): Effect<Justification, never, Unicode>`: shapes with `PRODUCE_SAFE_TO_INSERT_TATWEEL`, spreads slack by priority: word space within `spacing.word`, kashida through `fit` and `places` (tatweel inserts U+0640 and reshapes; stretched enables `jalt` over the place's cluster; `alternates` adds `jalt` everywhere), letter space within `spacing.letter`, glyph scaling within `spacing.glyph`; negative slack shrinks to the `min` ratios; never fails, `slack` reports the remainder
- Hand code declared under TYPESCRIPT.md [01] icu family: the priority rows
- Replaces nothing distilled: InPage's faces held this in their tables; the capability is InDesign's kashida justification over any face

## [07]-[SEARCH]

`data/search/digits.ts`, export `Digits` (loads in UXP; `package.json` `exports` gains `./digits` in [13.1]):
- `Digits.systems: readonly string[]`: `Intl.supportedValuesOf('numberingSystem')` filtered to systems whose ten `formatToParts` digits are one `\p{Nd}` code point each and consecutive
- `Digits.table(system): readonly string[]` builds its formatter per call; `Digits.format(text, system)` maps ASCII digits to the system; `Digits.fold(text)` maps every system's digits to ASCII
- Replaces distilled `data/text.ts` `getAsNumber`, `digitZeros`; the int32 clamp and `wcstol` go; parsing is `Digits.fold` then `Schema.NumberFromString`
- `fold.ts` of ARCHITECTURE decision 17 reads `Digits.table` when it lands

## [08]-[MCP]

`ai/mcp/progress.ts`, export `progress` (`package.json` `exports` gains `./progress` in [13.1]): `progress<A>(work: Stream<{ progress: number; total: Option<number>; message: Option<string> }>)` sends one `McpSchema.ProgressNotification` per element for the request's `progressToken` from `McpRequestContext`, drains silently without a token, through the `McpServer` notification broadcaster. Consumer: the `spacing` tool per story range.

## [09]-[AUTOMATION]

`creative-cloud/automation/typesetting.ts`, exports `Typesetting`, `Adjust`, `Kerns`, `Typeset` (`package.json` `exports` gains `./typesetting`, `dependencies` gain `@rasm/typography`, in [13.1]):
- `Adjust` (`Schema.Class`): `target: Schema.String`, `deltas: Array(TaggedUnion({ Kerning: { em }, Tracking: { em }, BaselineShift: { points }, Marks: { x, y } }))`; `Adjust.Result = Reference.mapFields(Struct.assign({ applied: Array(String) }))`; getter `label`
- `Kerns` (`Schema.Class`): `target`, `kerns: Union([Literal('clear'), Array(Struct({ index: Int, em: Finite }))])`; `Kerns.Result = Reference + { written: Int }`
- `Typeset` (`Schema.Class`): `target`, `settings: Option<Settings>`; `Typeset.Result = Settings`; a read when absent, a write of the given fields followed by a read when present
- `Typesetting = RpcGroup.make(Rpc.make('adjust', { payload: Adjust, success: Adjust.Result, error }), Rpc.make('kerns', ...), Rpc.make('typeset', ...))` with `error` from `requests.ts`
- Replaces distilled `suites/selection.ts` `adjustText`, `incrementText`, `adjustFrames`, `insertText`, `applyFont`, `stylisticSetStates`; `panels/features.ts` `toggleExclusiveTextAttribute` (exclusivity is the caller's `features` set); `spacing/tuner.ts` `adjustStep`, `clearKerningInRange`, `processKerningSelection`; `scripts/extendscript.ts` `toggleSmallCaps`, `smallCapsState`
- Derives: the 10000 reset bound, speed flags and frame walking go; a frame specifier resolves to its story's text

## [10]-[INDESIGN]

Handlers run in UXP; each decodes `target` through `Schema.decodeUnknownEffect(Specifier.Path)`, resolves `Specifier.object`, reads text through `Model` `text` and `frame` predicates (a frame taking `texts.item(0)`, `/` taking `app.textDefaults`) and writes inside `commit`.

`documents/settings.ts`, export `HostSettings`: one bidirectional codec between host members and `Settings`, declared once and used for every read and write:
- `direction`: `paragraphDirection` members, `Match.exhaustive` over the enumeration
- `numberingSystem` from `digitsType`: `ARABIC_DIGITS` `latn`, `HINDI_DIGITS` `arab`, `FARSI_DIGITS` and `FULL_FARSI_DIGITS` `arabext`, `DEFAULT_DIGITS` and `NATIVE_DIGITS` absent, `THAI_DIGITS` `thai`, `LAO_DIGITS` `laoo`, `DEVANAGARI_DIGITS` `deva`, `BENGALI_DIGITS` `beng`, `GURMUKHI_DIGITS` `guru`, `GUJARATI_DIGITS` `gujr`, `ORIYA_DIGITS` `orya`, `TAMIL_DIGITS` `tamldec`, `TELUGU_DIGITS` `telu`, `KANNADA_DIGITS` `knda`, `MALAYALAM_DIGITS` `mlym`, `TIBETAN_DIGITS` `tibt`, `KHMER_DIGITS` `khmr`, `BURMESE_DIGITS` `mymr`; writes invert, `arabext` writing `FARSI_DIGITS`
- `kashida`: `kashidas` `KASHIDAS_OFF` reads `insertion` none; `NASKH_TATWEEL_JUSTIFICATION` and `_FRAC` read `tatweel`, `NASKH_KASHIDA_JUSTIFICATION` and `_FRAC` read `stretched`, other members with kashidas on read `stretched`; `_FRAC` reads `fractional`; `otfJustificationAlternate` reads `alternates`; `paragraphKashidaWidth` 0 to 3 reads `length` none to long; writes invert, `stretched` writing `NASKH_KASHIDA_JUSTIFICATION` or its `_FRAC`
- `marks`: `xOffsetDiacritic`, `yOffsetDiacritic` divided by the unit factor the dictionary row states for the member
- `features`: `otfStylisticSets` bits to `ss<n>`; `otfTitling` `titl`; `otfContextualAlternate` `calt`; `otfDiscretionaryLigature` `dlig`; `otfStylisticAlternate` `salt`; `otfJustificationAlternate` `jalt`; `capitalization` `CAP_TO_SMALL_CAP` `c2sc` and `smcp`, `SMALL_CAPS` `smcp`; every other tag through `opentypeFeatures`; writes build the bitmask as `Number.sumAll(sets.map((n) => 2 ** (n − 1)))`
- `spacing`: `minimum`, `desired`, `maximum` × `WordSpacing`, `LetterSpacing`, `GlyphScaling` divided by 100
- Hand table declared under TYPESCRIPT.md [01] InDesign family

`documents/text.ts` (members this plan owns): `ParagraphAttributes` gains `settings: Settings` for paragraph members (direction, digits, kashida, spacing) and `Run` gains `settings` for character members (marks, features) through `HostSettings`; `Run.opentypeFeatures` and `kerningValue` stay.

`requests/adjust.ts`, export `adjust`: `Kerning` adds `em × 1000` to `kerningValue` of `insertionPoints.item(b + 1)`, `b` the last character at or before the target's last insertion point whose `contents` fails `/^\p{Mn}+$/v`; `kerningMethod` is never written. `Tracking` adds `em × 1000` to `tracking`; `BaselineShift` adds points to `baselineShift`; `Marks` adds offsets through `HostSettings`. Replaces distilled `position/adjuster.ts` host half, `scripts/extendscript.ts` `updateKerning`, `suites/selection.ts` `adjustText`, `incrementText`, `adjustFrames`.

`requests/kerns.ts`, export `kerns`: `clear` writes `kerningMethod` `$ID/Metrics` over the range; otherwise writes `kerningValue = em × 1000` at each `insertionPoints.item(index)` unless that point's `kerningMethod` reads the localized `Manual` (`app.translateKeyString('$ID/Manual')`) with a nonzero value, and reports `written`. Replaces `spacing/tuner.ts` `applyKerningToRange` writes, `clearKerningInRange`, `processKerningSelection`.

`requests/typeset.ts`, export `typeset`: reads the target's members through `HostSettings` into `Settings`; with `settings` present, writes the given fields through the same codec first. Replaces `suites/selection.ts` `stylisticSetStates`, `panels/features.ts` `toggleExclusiveTextAttribute`, `scripts/extendscript.ts` `toggleSmallCaps`, `smallCapsState`.

`host.ts`: `Contract = Requests.add(...).merge(Typesetting).middleware(ScriptPreferences)`; `toLayer` gains `adjust`, `kerns`, `typeset`; `host(indesign)` returns `local: <A, E, R>(effect) => Effect<A, E, Exclude<R, Model>>` providing `Model` under the same `preserve(app.scriptPreferences, ...)` as the middleware, for the panel.

## [11]-[ATELIER]

`features/typesetting.ts`, export `typesetting(routed)`, registered in `main.ts` beside `font(routed)`:
- Tool `spacing`: `target` (story, text or frame specifier), `rules: Kerning.Rules` (default `Kerning.Rules.make({})`), `clear: Boolean`; reads every page of runs through `client('inspect', ...)`, widens a text selection with `Word.expand`, opens each run's face through `client('font', ...)` and `Face.open`, runs `Kerning.of` once over the joined text with `pair` wrapped in a `Cache.make` keyed by `Data.struct({ face: postscriptName, variations, rules, before, after })`, raises `progress` per story range, then `client('kerns', ...)`
- Tool `glossary`: `ayahs` and `surahs` file paths, `columns: Quran.Columns`, `surah`, `ayah: number | 'all'`, `separator`; returns the text, which `change` inserts
- Tool `symbols`: `file`, `face` name; returns `Symbols.supported(face).sets` as strings
- Prose for `registerRpcGroup` rows in `features/plugins.ts`: `adjust` "Adds kerning, tracking, baseline shift or mark offsets to selected text", `kerns` "Writes pair kerns at insertion indexes or restores metrics kerning", `typeset` "Reads or sets direction, digits, kashida, mark offsets, features and spacing of selected text"
- `features/fonts.ts`: `font` tool gains `width: Option<Finite>` and `settings: Settings`; with `width` the specimen is `Justification.justify`, otherwise `Shaped.of` followed by `offsetMarks(settings.marks)`; names follow the rebuild
- `plugins/indesign/typesetting.ts`, export `typesetting` panel `{ show: (root) => Effect, hide }`: arrow buttons for marks x and y, kerning and baseline by mode, a kashida insertion and length control, stylistic set checkboxes from the `typeset` read of `app.selection[0]`; each control runs `local(handler(payload))` with `app.selection[0].toSpecifier()`; steps are `kerningKeyIncrement` (kerning, tracking, mark offsets) and `baselineShiftKeyIncrement` (baseline) read from `app.textPreferences`, a modifier key multiplying by the same factor the host's own shortcuts use
- `plugins/indesign/main.ts`: `plugin(dial, { commands: {}, panels: { typesetting } })`; `uxp.config.ts` `entrypoints` gains the `typesetting` panel row
- `libs/typescript/creative-cloud/uxp/entrypoints.ts`: panel `show` and `hide` receive the root node the host passes

## [12]-[REMOVALS_AND_MAP]

| [INDEX] | [FILE_OR_SYMBOL]                                          | [ACTION] | [REPLACEMENT]                                 |
| :-----: | :-------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `typography/face.ts` `Cause.UnknownError` raise           | Rebuilt  | `TypographyError`                             |
|  [02]   | `typography/face.ts` `outline` em division                | Rebuilt  | Font units                                    |
|  [03]   | `typography/tags.ts` `Settings`, `Tag`                    | Rebuilt  | `Tags.list`, `Tags.Tag`                       |
|  [04]   | `typography/shaped.ts` glyph record                       | Rebuilt  | Gains `class`; `advance`, `offsetMarks`       |
|  [05]   | `typography/tables.ts`                                    | Rebuilt  | Gains `GDEF`, `JSTF`                          |
|  [06]   | `indesign/documents/text.ts` `ParagraphAttributes`, `Run` | Rebuilt  | Gain `settings`                               |
|  [07]   | `indesign/host.ts` `Contract`, `host`                     | Rebuilt  | Merges `Typesetting`, returns `local`         |
|  [08]   | `uxp/entrypoints.ts` `plugin` panels                      | Rebuilt  | `show(root)`                                  |
|  [09]   | `atelier/features/fonts.ts` `font`                        | Rebuilt  | `width`, `settings`                           |
|  [10]   | `atelier/features/plugins.ts` `registerRpcGroup` rows     | Rebuilt  | Rows for the new tools                        |
|  [11]   | `atelier/plugins/indesign/main.ts`, `uxp.config.ts`       | Rebuilt  | Panel entry                                   |
|  [12]   | Catalog `papaparse`, `@types/papaparse`                   | Deleted  | None                                          |
|  [13]   | `.claude/workflows/TYPESCRIPT.md`                         | Rebuilt  | Rows of decision 12, [02] rule 15, [03] cells |

Consumers that move: `apps/creative-cloud/atelier/features/fonts.ts` (`Face`, `Shaped`, `Justification`, `Settings`), `apps/creative-cloud/atelier/plugins/indesign/main.ts` (`plugin` entries), `.claude/workflows/TYPESCRIPT.md` [03] rows 01, 04, 05 (`Digits`; `[AFTER]` gains `graphics/typography`; `[CONSUMERS]` gains `@rasm/automation`).

Distilled exports with no target above stay out: `panels/actions.ts`, `panels/cells.ts`, `panels/grid.ts`, `position/adjuster.ts` widgets and icons, `suites/selection.ts` `callAllSuites`, `callSuitesUntilTrue`, `hasCellText`, `walkFrames`, `panels/features.ts` `featureBase`, `clearFeatureButton`, `kerning/cache.ts` file codec, `kerning/json.ts`, `kerning/render.ts` host calls, `scripts/extendscript.ts` `applyLanguage` (`change appliedLanguage` serves it), `services/*`, `fonts/embedded.ts`, `host/*`. Selection Assistance (`panels/cells.ts` `setTextSelection`) is host chrome: `call select` on a character serves it.

## [13]-[LANDING]

Packages land in parallel over disjoint files, each once the packages it `needs` land. Every path is a repository path.

### [13.1]-[CATALOG]

Files: `pnpm-workspace.yaml`, `package.json`, `libs/typescript/graphics/typography/package.json`, `libs/typescript/data/search/package.json`, `libs/typescript/creative-cloud/automation/package.json`, `libs/typescript/ai/mcp/package.json`, `.claude/workflows/TYPESCRIPT.md`. Needs: none. Rows of [05]; `exports` and `dependencies` of [06] to [09]; TYPESCRIPT.md [01] row 11 gains "outline flattening and band profiles", a new `icu` row holds "kashida priority rows", the Adobe row gains "the InDesign feature-tag and enumeration table", [02] gains rule 15 (wasm-at-import modules load in Node alone), [03] cells of [12].

### [13.2]-[DIGITS]

Files: `libs/typescript/data/search/digits.ts`. Needs: `13.1`. Section [07].

### [13.3]-[TYPOGRAPHY_CORE]

Files: `libs/typescript/graphics/typography/error.ts`, `libs/typescript/graphics/typography/face.ts`, `libs/typescript/graphics/typography/shaped.ts`, `libs/typescript/graphics/typography/tables.ts`, `libs/typescript/graphics/typography/tags.ts`, `libs/typescript/graphics/typography/text/unicode.ts`, `libs/typescript/graphics/typography/composition/settings.ts`. Needs: `13.1`. Sections [06.1] to [06.3], [06.7].

### [13.4]-[TYPOGRAPHY_TEXT]

Files: `libs/typescript/graphics/typography/text/words.ts`, `libs/typescript/graphics/typography/text/quran.ts`, `libs/typescript/graphics/typography/text/symbols.ts`. Needs: `13.2`, `13.3`. Sections [06.4] to [06.6].

### [13.5]-[TYPOGRAPHY_KERNING]

Files: `libs/typescript/graphics/typography/composition/profile.ts`, `libs/typescript/graphics/typography/composition/kerning.ts`. Needs: `13.3`, `13.4`. Sections [06.8], [06.9].

### [13.6]-[TYPOGRAPHY_JUSTIFICATION]

Files: `libs/typescript/graphics/typography/composition/justification.ts`. Needs: `13.3`, `13.4`. Section [06.10].

### [13.7]-[PROGRESS]

Files: `libs/typescript/ai/mcp/progress.ts`. Needs: `13.1`. Section [08].

### [13.8]-[AUTOMATION]

Files: `libs/typescript/creative-cloud/automation/typesetting.ts`. Needs: `13.1`, `13.3`. Section [09].

### [13.9]-[INDESIGN]

Files: `libs/typescript/creative-cloud/indesign/documents/settings.ts`, `libs/typescript/creative-cloud/indesign/documents/text.ts`, `libs/typescript/creative-cloud/indesign/requests/adjust.ts`, `libs/typescript/creative-cloud/indesign/requests/kerns.ts`, `libs/typescript/creative-cloud/indesign/requests/typeset.ts`, `libs/typescript/creative-cloud/indesign/host.ts`. Needs: `13.3`, `13.8`. Section [10]. The mark offset unit factor is read from the dictionary `Field` row of `xOffsetDiacritic` through `Model.rows`, and a probe through `tools/harnesses/indesign` writing one known value confirms it before the handler lands.

### [13.10]-[ATELIER]

Files: `apps/creative-cloud/atelier/features/typesetting.ts`, `apps/creative-cloud/atelier/features/fonts.ts`, `apps/creative-cloud/atelier/features/plugins.ts`, `apps/creative-cloud/atelier/main.ts`, `apps/creative-cloud/atelier/plugins/indesign/main.ts`, `apps/creative-cloud/atelier/plugins/indesign/typesetting.ts`, `apps/creative-cloud/atelier/plugins/indesign/uxp.config.ts`, `libs/typescript/creative-cloud/uxp/entrypoints.ts`. Needs: `13.3`, `13.4`, `13.5`, `13.6`, `13.7`, `13.8`, `13.9`. Section [11].

### [13.11]-[HARNESS]

Files: `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/proof/ab/spacing.ts`, `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/proof/ab/quran.ts`, `docs/research/applications/indesign/products/typesetting/inpage-typesetting-tool/proof/ab/symbols.ts`. Needs: `13.2`, `13.3`, `13.4`, `13.5`. Section [14].

## [14]-[HARNESS]

`node proof/ab.ts` from the product folder stays the oracle and keeps exiting 0. Distilled-against-rebuilt pairs stay untouched. Families in the table gain a `library` column: `library?: Fn` with `scope?: { reason: string; holds: (args) => boolean }`, run by the same `attempt` and `agree` after the distilled column, reported as `library N agreeing, M outside scope, K mismatching`, failing the family on a mismatch inside scope. Adapters import library files by relative path with `.ts` extensions, build `const runtime = ManagedRuntime.make(Unicode.layer)` and `await runtime.context()` at module top, run through `runtime.runSync`, and project results into the rebuilt return shape. The InPage rule rows (`Gaf` before at x 65/128 em, `Reh` after at 65/128, `Dal`, `Waw`, `TehMarbuta`, `Heh` after at 60/128 with correction −0.06, `Alef` after at 60/128, otherwise 60/128, top 200/128, foot 0 or −50/128) exist only in the spacing adapter.

| [INDEX] | [FAMILY]  | [PAIRS]                                                                             | [LIBRARY]                                    | [SCOPE]                                                                                                               |
| :-----: | :-------- | :---------------------------------------------------------------------------------- | :------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `spacing` | `expandSelectionToWords`                                                            | `Word.expand`                                | Finite bounds inside the text, no U+0009, U+000A, U+2028, U+2029                                                      |
|  [02]   | `spacing` | `isWordEndingLetter`, `isCombiningMark`, `isArabicSpacingCharacter`                 | `JoiningType`, `Transparent`, `hasScript`    | Units in U+0600..U+06FF outside the study's listed mistakes, no digits                                                |
|  [03]   | `spacing` | `applyKerningToRange` `applied` positions                                           | `Kerning.of` with `pair` over the case table | One face, `kerns` true, every key present, finite values, kerning in {0, 1e8, null}, `first` 0, runs under 1000 units |
|  [04]   | `quran`   | `loadQuranData`, `getSurahNames`, `getAyahs`, `findSurahNumber`, `insertAyahs` text | `Quran.decode`, `surah`, `text`              | ASCII or Arabic-Indic digits, cells without `"` or NUL, non-null files                                                |
|  [05]   | `quran`   | `getAsNumber`                                                                       | `Digits.fold` then `Number`                  | Digits of one decimal system, values inside int32, no sign-only text                                                  |
|  [06]   | `symbols` | `parseHexCodePoint`, `appendUtf32`                                                  | `Symbols` cell codec, `Symbols.text`         | No sign, tab, line break, quote or NUL; values at most 0x10FFFF                                                       |

Families actions, fonts, host, services, tables, position, suites, scripts and kerning gain no library column: their logic is out, host-only, or shares no intermediate with the outline profile (bitmap slide against geometric gap).

## [15]-[CHECK]

One pass after every package lands: `pnpm install --prefer-offline`, then `nx run <project>:check` for `@rasm/typography`, `@rasm/search`, `@rasm/mcp`, `@rasm/automation`, `@rasm/indesign`, `@rasm/uxp`, `@rasm/atelier`, `@rasm/atelier-indesign-plugin`, then `nx run rasm:lint:biome -- <files>` and `nx run rasm:lint:ast-grep -- <files>` over every edited file outside `docs/research`, then `node proof/ab.ts` from the product folder. Named `const` values under `[CONSTANTS]` hold the thousandths factor and the percent divisor. Findings fix at their cause once, never through a suppression or lowered severity, then the same commands run once more.

## [16]-[LANDING_NOTES]

Points where the code or a package's real source contradicted a statement above, with what landed:

| [INDEX] | [SECTION]   | [PLAN_STATEMENT]                                                      | [REAL_FACT_AND_LANDING]                                                                                                                                |
| :-----: | :---------- | :-------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | [05], [06.4] | `Script.Arab`, `hasScript(cp, Script)`, `asBorrowed.getScriptExtensionsVal` | icu 2.4.0 names `Script.Arabic`; `hasScript(cp, script: number)` takes `Script.Arabic.toIntegerValue()`; `asBorrowed` is a getter                     |
|  [02]   | [06.10]     | Priority row "Reh, Yeh or Alef Maqsurah"                              | `JoiningGroup` has no `AlefMaqsurah`; U+0649 is group `Yeh`, the row reads "Reh or Yeh"                                                               |
|  [03]   | [08]        | `progressToken` from `McpRequestContext`                              | Context has no such field; token reads from `requestMetadata.progressToken` through `McpSchema.ProgressToken`; `progress` takes `E`, `R` alone        |
|  [04]   | [06.2], [12] | `Tags.Tag`, `Tags.list`                                              | ast-grep `no-namespace-object` and Biome's static-only class rule refuse the grouping; `tags.ts` exports `Tag` and `TagSettings`                       |
|  [05]   | [07]        | `Digits` class with statics                                          | Biome refuses a static-only class; `Digits` is one `as const` object; `fold` derives digit values from `\p{Nd}` runs of ten, no zero table              |
|  [06]   | [09]        | `settings: Option<Settings>` on `Typeset`                             | A partial write needs optional keys; `direction` and `numberingSystem` take `OptionFromNullOr` (absent key unchanged, `null` default), `{}` is refused   |
|  [07]   | [05]        | `[TYPOGRAPHY]` after `[SPATIAL_RENDERING]`, rows moved from `[ADOBE_HOSTS_FONTS]` | Landed; the remaining Adobe rows keep the group under `[ADOBE_HOSTS]`                                                                                   |
|  [08]   | [05]        | `effect` 4.1.1                                                        | Registry newest is 4.1.2; the catalog stays at 4.1.1 across every `@effect/*` row, a move for `rasm:upgrade`                                            |
|  [09]   | [10]        | `/` target reads `app.textDefaults` for every member                  | `TextDefault` typings lack `kerningValue` and `opentypeFeatures`; `kerningMethod` is a string with no enumeration                                       |
|  [10]   | [06.1]      | `markClass`, `extenders` read values built in `open`                  | Encode-omitted fields `markClasses: ReadonlyMap<number, number>` and `extenders` are public members of `Face`                                           |
|  [11]   | [09], [10]  | Requests as the InDesign capability                                   | Owner rule: capability is plain Effect operations over host objects (`HostSettings.read/write`, kern and adjust writes) callable from any UXP program; `handle` and the `Typesetting` RPC group are transport bindings; no MCP shape in `@rasm/indesign` or `@rasm/automation` |
|  [12]   | [06.4]      | Runs end after a RightJoining or NonJoining code point                  | A run also ends before a code point that joins no backward neighbour (`شيء` splits before the hamza); non-Arabic code points sit in no run, `arabic` holds when `runs` is non-empty |
|  [13]   | [06.5]      | Rows as records keyed by header                                       | One pass without `headers`, first row as the header row, named columns resolved to indexes through `Unmatched`; ayahs naming no surah fail `Unmatched` over `surah` |
|  [14]   | [06.9]      | `side` `before` reads the preceding run, wall on the following run    | InPage tests `prefix.slice(0, 2)` of the following run and walls the preceding run; `before` reads the following run's first two base letters, `after` the preceding run's last, `Profile.gap(following, walled preceding)` |
|  [15]   | [06.8]      | `Profile` fields `unitsPerEm`, `left`, `right`, `advance`             | `descender` joins so `wall` maps bands to heights; the flattening tolerance is local to its one reader (ast-grep `no-single-reader-tsx`)                 |
|  [16]   | [06.9]      | Punctuation words of one code point count as a preceding run           | Decided by ICU `GeneralCategoryGroup.punctuation`, inside a word too; never an `after` run; ZWNJ between runs keeps them adjacent                        |
|  [17]   | [14] row 03 | Scope: one face, `kerns` true, finite values                           | InPage omits positions at U+0623, U+0625, U+0629, starts runs at a mark after hamza, U+061F or U+06D4, and spaces at digits and `:`; the scope excludes these |
|  [18]   | [06.10]     | `stretched` enables `jalt` over a `SAFE_TO_INSERT_TATWEEL` place      | HarfBuzz flags no tatweel places on the `jalt` faces checked (Aref Ruqaa, Gulzar); `stretched` ranks every join and keeps a `jalt` place when the measured advance changes and fits, `fit` drives tatweel alone |
|  [19]   | [06.10]     | `Justification.fit` with `grade` from `length`                        | `Justification.grade(length)` is public so callers derive the ordinal once; priority rows hold `side`, `following`, `position`; `FarsiYeh` joins the Yeh rows (U+06CC has its own group) |
|  [20]   | [06.7], [10] | `spacing.letter` as em offsets                                        | InDesign letter spacing is a percent of the space width (100% adds one space between letters); `Settings.spacing.letter` is ratios of the space advance, the host codec divides by 100, `Justification` scales by the space advance |
|  [21]   | [10]        | `direction` absent reads `None`                                       | `paragraphDirection` has no default member and always reads `Some`; `None` writes `characterDirection` `DEFAULT_DIRECTION`                               |
|  [22]   | [10]        | `local: Effect<A, E, Exclude<R, Model>>`                              | `preserve` can refuse, so `local` fails `E | HostError`; `HostSettings.write` fails `Unmatched | SchemaError` too, inherited from `Change.write`           |
|  [23]   | [10]        | Story targets read through `Model` `text`                            | `Model.text` holds for no `Story`; story and frame targets resolve to `texts.item(0)`; `kerns` and `Kerning` deltas on `/` fail `Unmatched`              |
|  [24]   | [13.9]      | Host probe confirms the mark offset unit before the handler lands      | Probe batches did not run while instance 11207 read unresponsive; marks use the thousandths factor until the batch at `scratchpad/settings-probe` runs    |
|  [25]   | [14]        | `library?: Fn` with `scope?` per pair                                 | Pairs also take `reference?` (projection of the rebuilt result) and `within?` (in-scope generators on a separate random stream so existing cases stay identical); `Kerning.rule` gains a row against `getPairKerning().path` |
|  [26]   | [14] rows   | Stated scopes                                                         | Narrowed per [16] rows 12, 16, 17 and the harness run: `spacing` 11117 agreeing, `quran` 4063, `symbols` 5499, 0 mismatching inside scope; `biome.json` ignores `docs/research` |
|  [27]   | [10], [13.9] | Unit factor read from the dictionary `Field` row of `xOffsetDiacritic` | The row states `number` and no unit; the probe (build 22.0.0.101, Noto Naskh Arabic, `OPENTYPE_POSITION_FROM_BASELINE`) measured 100 = 0.1 em at 12 and 24 pt, so both offsets are thousandths of an em declared once as the thousandths `const`; the host clamps nothing and converts no measurement unit |
|  [28]   | [06.7]      | `marks.y` in em, y up                                                 | InDesign's positive `yOffsetDiacritic` moves each mark away from its base (above-base marks up, below-base marks down); the codec passes the value through, the sign for below-base marks differs from `Shaped.offsetMarks` |
|  [29]   | [11]        | `Cache.make` keyed by `Data.struct({ face, variations, rules, before, after })` | `Data.struct` is absent in effect 4.1.1; one `Cache` per opened face keyed `{ before, after }` lives for one call; `Kerning.of` runs per consecutive same-face span with `progress` per span |
|  [30]   | [11]        | A modifier key multiplies the step by the host's shortcut factor      | InDesign's five-times shortcuts take Command (macOS) or Ctrl (Windows), not Shift; the panel multiplies on `metaKey || ctrlKey`                           |
|  [31]   | [11], [12]  | Panel `show(root)`                                                    | `@adobe-uxp-types/uxp` gives `show` no parameters; the handler takes rest arguments and decodes the root with `instanceof HTMLElement`; `@rasm/indesign` exports `./adjust`, `./model`, `./settings`, `./transactions` for the panel, `STYLISTIC_TAGS` derived from the twenty-set count |
|  [32]   | [11]        | `spacing` widens a selection with `Word.expand` then `client('kerns')` | `clear` writes the given target unwidened (no specifier names a widened range); computed kerns target the owning story or cell text at absolute indexes; `glossary` and `symbols` register through `registerTool` and return `{ text }` and `{ sets }` |
|  [33]   | [13.10]     | Atelier lands with the other packages                                 | Another session rebuilds `apps/creative-cloud/atelier`, `tools/harnesses` and `tools/watch-apps`; [13.10] stays open for a later landing, and the lander's drafts under `apps/creative-cloud/atelier` (features/typesetting.ts, features/fonts.ts, features/plugins.ts, main.ts, plugins/indesign/{main,typesetting,uxp.config}.ts, plugins/indesign/package.json) stand as uncommitted drafts, `git checkout` of them refused by the edit hook; `@rasm/atelier` and `@rasm/atelier-indesign-plugin` `check` pass with them |

## [17]-[DESKTOP_MANUAL]

Capability the desktop InPage 4.1 manual holds that the landed code lacks and a later row builds, each stated as the relation it derives from; InPage's own values are its choices and enter a harness adapter alone. Landed beside this section: `Settings.features` as tag to value with the InDesign codec keeping `opentypeFeatures` values, `ShapedText.of` taking `Settings` features and direction, letter space at joining-run boundaries alone, tatweel from a positive U+0640 advance, OS/2 superscript and subscript in `Metrics`.

| [INDEX] | [CAPABILITY]              | [LEVEL] | [RELATION_AND_MECHANISM]                                                                                                                                                                                                                              |
| :-----: | :------------------------ | :------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Logical alignment         | U, ID   | `Settings.alignment` start, end, centre, justified with last line natural or forced; `Justification.justify` keeps a natural last line; codec picks `justification` from `direction` (`RIGHT_JUSTIFIED` for RTL start, `FULLY_JUSTIFIED` for forced) |
|  [02]   | Baseline shift in em      | CC, ID  | `Adjust.deltas.baselineShift` in em, `adjustText` writing `em × pointSize` per style run, the panel dividing `baselineShiftKeyIncrement` by the selection's size; values taken from InPage negate, its negative shift moving up                       |
|  [03]   | Glyph scale through width | U       | `spacing.glyph` ratio maps to a `wdth` value inside `Face.axes` and reshapes; synthetic scale serves faces without the axis and `ShapedText` glyphs carry the horizontal scale a renderer and `Profile.of` apply                                      |
|  [04]   | Kashida style             | U       | Pattern set per style (Raqim `arabic-naskh`, `arabic-nastaliq`, caller rows) as `kashidaCandidates` data, phase before breaking (text-level, lines reflow) or after (fit to slack), eligibility from tatweel advance, safe flags and `jalt` coverage   |
|  [05]   | Kashida eligibility write | ID      | `HostSettings.write` refuses `kashidas` and tatweel `paragraphJustification` for a face with no positive tatweel advance and no `jalt`, the failure InDesign and Word showed on contextual joins                                                       |
|  [06]   | Justification priority    | U       | One rank per mechanism (word space by script of the adjacent text, each kashida class, `jalt`, letter, glyph), equal ranks sharing slack by capacity, order caller data; InPage's 10, 10 and 12 to 22 are one adapter row                              |
|  [07]   | Per-glyph alternates      | U, ID   | GSUB alternate sets behind `aalt`, `salt`, `cvNN` decoded through `Tables` for the shaped glyph of the joined form, `cvNN` `FeatureParams` characters and labels in `Layout`, applied as `tag=n` over one character through `opentypeFeatures`         |
|  [08]   | Per-line widths           | U, ID   | Usable intervals per line band from the frame chord minus wrapping outlines grown by offsets; `PageItem.textWrapPreferences` (`textWrapMode`, `textWrapOffset`, `textWrapSide`, `contourOptions`), `TextFramePreference.ignoreWrap`, frame paths     |
|  [09]   | Story containers          | ID      | `StoryView` frames report `textColumnGutter`, `insetSpacing`, `ignoreWrap` and path kind beside `textColumnCount`, so `M = (W − insets − (n − 1)·g) ÷ n` derives in place of a caller width                                                           |
|  [10]   | Leading                   | U, ID   | Line height from shaped extents times a caller ratio or snapped to the baseline lattice, `Settings.leading`, codec over `leading`, `autoLeading`; paragraph space dropped at column and frame edges                                                    |
|  [11]   | Verse and parallel text   | U       | Hemistich width `w` as the widest at minimum stretch, forced fit per hemistich, centred columns with a gutter or staggered indents `(a, M − w − a)`; parallel columns cut at segment boundaries, rows at the tallest column                           |
|  [12]   | Running head              | ID      | `TextVariable` of `MATCH_PARAGRAPH_STYLE_TYPE` with `searchStrategy` `FIRST_ON_PAGE` or `LAST_ON_PAGE` over the TOC style list; InDesign drops ZWNJ from variable output, so Persian heads need the ZWNJ restored                                        |
|  [13]   | Ordering                  | U       | `Intl.Collator(locale, { sensitivity })` over keys ending at a caller delimiter, `base` against `accent` deciding whether harakat count, sharing the yeh, kaf and digit folds with matching                                                            |
|  [14]   | Tabs                      | U, ID   | Stops start, end, centre and character made physical by direction, the decimal character from the run's numbering system (`Intl.NumberFormat` `formatToParts` decimal), `tabList` written through `change`                                            |
|  [15]   | Size-relative choices     | U       | Underline from `post` or CFF stroke against the run's deepest ink, character box from shaped extents plus caller padding, footnote reference from `Metrics.superscript`, Latin size from alef height against cap or x-height per face pair             |
|  [16]   | Binding side derivation   | ID      | `PageView` and `DocumentView` derive inside, outside, start and end from `facingPages`, `pageBinding` and `page.side`; placement anchor and ruler sign from reading direction                                                                            |
|  [17]   | Ayah mark                 | U       | `@quran.ws/text` `ayahMark` (U+06DD then digits of `Settings.numberingSystem`) appended by `Quran.text`, with U+FD3E and U+FD3F where a shape test shows the face draws no enclosing mark                                                               |
|  [18]   | Mixed-run justification   | U       | `Justification.justify` over a line of runs with their own faces and `Settings`, slack shared by the priority of [06]                                                                                                                                   |
|  [19]   | Headline nesting          | U, ID   | `Kerning.pair` with a vertical shift bound, the `(dx, dy)` of smallest advance keeping clearance written as `kerningValue` and `baselineShift`                                                                                                           |
