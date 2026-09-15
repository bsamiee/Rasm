# InDesign 2026 Beta probe

Read-only probe run 2026-09-11. Nothing was saved, no preference was written, no Adobe folder was
touched, no second Sidekick primary was started. Every document was opened with
`app.open(File(path), false)` and closed with `SaveOptions.NO`; `app.documents.length` was 0 before
and after each pass.

Host confirmed as the Beta: `app.version` = **21.6.0.58**, `app.fullName` =
`/Applications/Adobe InDesign 2026 (Beta)/Adobe InDesign 2026 (Beta).app`, `app.locale` =
`ENGLISH_LOCALE`, PID 33256. `app.scriptPreferences.userInteractionLevel` was set to
`NEVER_INTERACT` at the head of every script.

Companion files in this directory:

| File | Contents |
| :--- | :--- |
| `app-preferences.json` | Every readable `app.*Preferences` object, document presets, PDF presets, preflight profiles with rule data |
| `app-preferences-2.json` | Text defaults, colour settings, transparency, swatches, installed font families |
| `doc-default-template.json` | Full dump of `Default Template.indt` |
| `doc-school-document-template.json` | Full dump of `School Document Template.indd` |
| `doc-kelman-us-letter-portrait-grid.json` | Full dump of `US Letter Portrait Grid System.indt` |
| `doc-kelman-digital-presentation-landscape-grid-v2.json` | Full dump of `Digital Presentation Landscape Grid System v2.indd` |
| `toc-and-swatches.txt` | TOC style entries, colour-group membership, Source Sans 3 style list |
| `indesign-defaults.strings.txt` | `strings -n 5` over the binary `InDesign Defaults` |
| `probe-app.jsx`, `probe-app2.jsx`, `probe-doc.jsx`, `probe-toc.jsx`, `run.sh` | The probes themselves |

---

## 01 Preferences and workspace files on disk

`~/Library/Preferences/Adobe InDesign (Beta)/Version 21.0/en_US/`

| Entry | Kind | Size / state |
| :--- | :--- | :--- |
| `InDesign Defaults` | binary | 6,414,336 bytes, written 16:11 |
| `Color Settings` | binary | 3,465,896 bytes |
| `AppPrefs.xml` | XML | 216 bytes, holds only `Workspace/Switcher = ""` |
| `Workspaces/Essentials_CurrentWorkspace.xml` | XML | 46,479 bytes |
| `Workspaces/Start_CurrentWorkspace.xml` | XML | 48,215 bytes |
| `Find-Change Queries/GREP/` | dir | 2 custom queries (below) |
| `Menu Sets/` | dir | **empty** — no custom menu set |
| `Scripts/Scripts Panel/` | dir | **empty** — no user scripts |
| `StylePackPresets/Previews/` | dir | **empty** |
| `CompositeFont/` | dir | **empty** |
| `DVADialogPrefs/DVADialogPrefs.xml` | XML | dialog geometry |
| `UXP/PluginsStorage/Internal/` | dir | 19 Adobe first-party UXP panels |
| `AdobeFntComposite.lst`, `FontsTemp.lst`, `cooltypeTemp.lst`, `CMapsTemp.lst` | font caches | regenerated on launch |
| `Debug Database.txt`, `Trace Database.txt` | debug flags | `UseAdobeCleanFont false`, beta feature toggles off |

There is **no `InDesign Shortcut Sets/` directory** — the shortcut set is `Default` (read from
`app.generalPreferences.keyboardShortcutSet`), so InDesign has never had to write a custom set.
There is likewise **no `Glyph Sets/`, `Autocorrect`, or `Swatch Libraries` directory** under this
profile; those directories only appear once a custom glyph set, an autocorrect entry, or an imported
`.ase` is saved. Autocorrect word pairs live inside the binary `InDesign Defaults`.

### Find-Change GREP queries

Both custom queries, verbatim (headers `Version 5.1`, `QueryType Grep`, options identical between
them: locked layers/stories excluded, master pages excluded, hidden layers excluded, footnotes
included, kana- and width-sensitive).

**`Collapse Ordinary Spaces.xml`**

```
FindExpression:    [ ]{2,}
ReplaceExpression: (single space)
```

**`Trim Trailing Spaces and Tabs.xml`**

```
FindExpression:    [ \t]+$
ReplaceExpression: (empty)
```

Both carry a `ReplaceFormatSettings` block with `TextAttribute type="changecondmode" value="0"`.

### Where the other preset classes actually live

| Preset class | Location | State |
| :--- | :--- | :--- |
| Document presets | Inside the binary `InDesign Defaults`, not a separate file | Two: `[Default]` and `Default Template` |
| Preflight profiles | Inside `InDesign Defaults`; embedded copies travel in each document | `[Basic]` at app level; `Default Preflight` embedded in the template |
| PDF export presets | `/Library/Application Support/Adobe/Adobe PDF/Settings/` (shared, root-owned) | 10 `.joboptions`, all Adobe stock |
| User PDF presets | `~/Library/Application Support/Adobe/Adobe PDF/Settings/` | **empty** — no custom job options |
| Colour settings | `Color Settings` in the profile directory | See §2 |

Shared `.joboptions` (all dated 2026-09-06, i.e. installed with the app):
`High Quality Print`, `Oversized Pages`, `PDFA1b 2005 CMYK`, `PDFA1b 2005 RGB`, `PDFX1a 2001`,
`PDFX3 2002`, `Press Quality`, `Smallest File Size`, `Standard-Classic`, `Standard`.

`app.pdfExportPresets` reports six: `[PDF/X-3:2002]`, `[PDF/X-1a:2001]`, `[High Quality Print]`,
`[PDF/X-4:2008]`, `[Smallest File Size]`, `[Press Quality]`. `app.printerPresets` reports one,
`[Default]`. No custom preset of either kind exists.

### Workspace XML

`Essentials_CurrentWorkspace.xml` is a `<user-workspace application-version="21.6"
build-number="58">` wrapper holding one `<PaletteWorkspace FeatureSet="256" version="2.0">` whose
`OWLWorkspaceBookmark` carries a 46,023-character CDATA payload, plus a `<menu-set name="InDesign
Defaults" is-preset="true">`. The CDATA is the real workspace.

Docks:

| Anchor | Content | Closed | Notes |
| :--- | :--- | :--- | :--- |
| top | control-bar | false | one leaf, `Control Strip` id 70913, **collapsed = true**, origin `-15 -2`, size `1903 57`; all 48 control-strip option groups enabled |
| bottom | control-bar | false | empty |
| left | palette toolbar | false | one `<toolbar id="2" size-variant="vertical-narrow">`, leaf id 4353 (the Tools panel), not closed |
| right | palette toolbar | false | the panel column, below |
| none ×44 | palette toolbar | 43 closed, 1 open | floating-window memory, mostly at origin `617 335` / `796 484` |

Right-hand dock, one column, 12 palette groups. Every one is open (`is-closed=false`,
`is-minimized=false`); `current-state=1` throughout except palette 8, which is `3`
(icon-collapsed). Named leaves in dock order:

| Panel (internal name) | Leaf id | Collapsed | detailLevel |
| :--- | ---: | :--- | ---: |
| `HistoryPanelName` (History) | 149248 | false | 1 |
| Character Styles | 8465 | false | 0 |
| Paragraph Styles | 8466 | false | 0 |
| (unnamed leaf) | 18464 | false | 0 |
| (unnamed leaf) | 132609 | false | – |
| Stroke | 11035 | false | 1 |
| Color | 29241 | false | 1 |
| Swatches | 16385 | false | 0 |
| PDF Comments | 134502 | false | 0 |
| `PropertiesPanelName` (Properties) | 71040 | false | – |
| Pages | 6199 | false | 1 |
| CC Libraries | 134508 | false | 1 |

Palette preferred sizes, in dock order: `240×200`, `250×466` (constrained `240×180`), `250×466`,
`252×244`, `240×368` (constrained `240×231`), `240×312`, `240×138`, `240×153`, `252×342`,
`252×1004`, `300×1004`, `300×1004`. No front-tab selector (`selected=`) is recorded, so each group
holds a single leaf rather than a tab stack.

`Start_CurrentWorkspace.xml` has the same top/left/right docks and 48 more floating-dock memories;
it is the workspace InDesign writes for the Start screen, and the Start screen is disabled
(`showStartWorkspace = false`).

### `InDesign Defaults` (binary, `strings -n 5`)

47,524 strings. What is recognisable:

- The active workspace name `@Essentials` and `@Book` (the two workspace records InDesign keeps).
- The document preset `@Default Template` together with the full path it points at:
  `…/03.Digital Asset Database/05.Software Related Assets/99.Default Profiles/InDesign/Default Template.indt`,
  plus its sandbox bookmark blob.
- The preflight profile name `@Default Preflight`.
- The recent-document list: four `editorial-examples` files under
  `OneDrive-Personal/05.School/Fall 2026/Masters Preperation/assets/` (`board.indd`,
  `landscape.indd`, `portrait.indd`) and four `/private/tmp/editorial-tooling/commissioning/*.indd`.
- The font menu cache — every installed family, including the whole Neue Haas Grotesk Display Pro
  and Text Pro range, Avenir LT Pro, Bodoni 72, FrutigerNeue, BookmanJFPro.
- Only one composer-related string, `@Paragraph Composer.InDesignPlugin` (the plug-in id, not a
  chosen composer name).
- Colour-profile strings are all inside embedded `.joboptions` blobs (`sRGB IEC61966-2.1`,
  `U.S. Web Coated (SWOP) v2`, `Coated FOGRA27`); the live working spaces are in `Color Settings`
  and are reported through the API in §2.
- Paragraph and character style names do **not** appear — the app-level style list is bare
  (`[No Paragraph Style]`, `[Basic Paragraph]`, `[None]`), so all named styles live in documents.
  `@Running Header` appears, but as the built-in text variable, not a style.

---

## 02a Application preferences (ExtendScript)

Units are **inches** at app level, so the raw numbers below are inches unless marked. Full dump in
`app-preferences.json`.

### `app.viewPreferences`

| Property | Value |
| :--- | :--- |
| horizontal / vertical measurement units | `INCHES` / `INCHES` |
| typographic / text size units | `POINTS` / `POINTS` |
| stroke units | `POINTS` |
| print dialog units | `INCHES` |
| ruler origin | `SPREAD_ORIGIN` |
| points per inch | 72 |
| cursor key increment | 0.0138889 in = **1 pt** |
| guide snap-to zone | 4 px |
| show rulers / frame edges / text threads / notes | all true |

### `app.generalPreferences` (selected)

`toolTips` normal, `showContentGrabber` true, `showLiveCorners` true, `showMasterPageOverlay`
false, `objectsMoveWithPage` false, `preventSelectingLockedItems` true, `mainMonitorPpi`
151.500002, `useCustomMonitorResolution` false, `uiBrightnessPreference` 0 (darkest),
`pasteboardColorPreference` 1, `showWhatsNewOnStartup` false, `showStartWorkspace` false,
`useApplicationFrame` true, `openDocumentsAsTabs` true, `openRecentLength` 20,
`autoCollapseIconPanels` true, `showLegacyNewDocumentDialog` false,
`setActiveWorkspace` **Essentials**, `keyboardShortcutSet` **Default**,
`autoGenerateAltText` false, `enableContentAwareFit` false, `ungroupRemembersLayers` true,
`temporaryFolder` `~/Library/Caches/Adobe InDesign (Beta)/Version 21.0/en_US/InDesign Recovery`.

### `app.documentPreferences` (app-level new-document defaults)

Page 8.5 × 11 in `Letter` portrait, **facing pages true**, 1 page, bleed 0 on all four sides, slug 0,
`intent` `PRINT_INTENT`, `createPrimaryTextFrame` false, `startPageNumber` 1,
`columnGuideLocked` true, `overprintBlack` true.

### `app.marginPreferences`

top/bottom/left/right 0.5 in (36 pt), 1 column, gutter 0.1667 in (12 pt).

Note the divergence: the app defaults are *stock InDesign*, while the `Default Template` document
preset (below) carries the real values. New documents made from **File ▸ New** without picking the
preset get 0.5 in margins, facing pages, and no bleed.

### `app.gridPreferences` (baseline grid defaults for new documents)

| Property | Value |
| :--- | :--- |
| `baselineStart` | 0.5 in = **36 pt** |
| `baselineDivision` | 0.1666667 in = **12 pt** |
| `baselineGridRelativeOption` | `TOP_OF_PAGE` |
| `baselineGridShown` | false |
| `baselineViewThreshold` | 75 % |
| document grid | 1 in gridline, 8 subdivisions, hidden, snap off, grids in back |

This is stock. It is **not** 15 pt, so a document created outside the template starts on a 12 pt
grid measured from the page top.

### `app.storyPreferences`

`opticalMarginAlignment` **false**, `opticalMarginSize` 12, `storyDirection` `LEFT_TO_RIGHT`.

`app.storyPreferences` **has no `composer` property** — the API raises
`Object does not support the property or method 'composer'`. Composer lives on
`app.textDefaults.composer` and on each paragraph style. See §4.

### `app.textDefaults`

`composer` **Adobe Paragraph Composer**, font Minion Pro Regular 12 pt / AUTO leading,
`justification` `LEFT_ALIGN`, language *English: USA*, `kerningMethod` Metrics, hyphenation on
(after-first 2, before-last 2, words longer than 5, ladder 3, capitalised words true, zone 0.5 in,
weight 5), word spacing 80/100/133, letter spacing 0/0/0, glyph scaling 100/100/100,
`alignToBaseline` **false**, `paragraphDirection` `LEFT_TO_RIGHT`, `digitsType` `DEFAULT_DIGITS`.

### `app.textPreferences` (selected)

`typographersQuotes` true, `useOpticalSize` true, `useParagraphLeading` true,
`highlightSubstitutedFonts` true, `highlightSubstitutedGlyphs` true,
`highlightHjViolations` **false**, `highlightKeeps` **false**, `highlightCustomSpacing` false,
superscript 58.3 % / 33.3 %, subscript 58.3 % / 33.3 %, small cap 70 %,
leading key increment 2 pt, baseline-shift increment 2 pt, kerning increment 20/1000 em,
`smartTextReflow` true with `limitToMasterTextFrames` true and `deleteEmptyPages` false,
`shapeIndicAndLatinWithHarbuzz` **true**, `linkTextFilesWhenImporting` false,
`showInvisibles` false.

### `app.textEditingPreferences`, `app.clipboardPreferences`, `app.guidePreferences`, `app.transformPreferences`, `app.dictionaryPreferences`, `app.linkingPreferences`, `app.imagePreferences`, `app.pasteboardPreferences`, `app.displayPerformancePreferences`, `app.spellPreferences`, `app.autoCorrectPreferences`, `app.scriptPreferences`, `app.textFramePreferences`

All present and dumped in full in `app-preferences.json`. Nothing in them is non-stock apart from
what is already called out.

### `app.colorSettings`

| Property | Value |
| :--- | :--- |
| `enableColorManagement` | true |
| `engine` | Adobe (ACE) |
| `cmykPolicy` | `PRESERVE_EMBEDDED_PROFILES` |
| working CMYK list | 24 entries, includes `GRACoL2013_CRPC6.icc` |
| working RGB list | 31 entries |
| CMS settings list | 16 named settings |

### `app.transparencyPreferences`

`blendingSpace` **CMYK**, global light angle 120°, altitude 30°.

### `app.documentPresets`

| Name | Facing | Page | Margins (in) | Bleed (in) | Intent |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `[Default]` | true | 8.5 × 11 Letter | 0.5 / 0.5 / 0.5 / 0.5 | 0 | Print |
| **`Default Template`** | **false** | 8.5 × 11 Letter | 0.625 top, **0.69144444167614** bottom, 0.625 left, 0.625 right | 0.125 uniform | Print |

Both are 1 column with a 0.1667 in (12 pt) gutter and `createPrimaryTextFrame` false.

### `app.preflightProfiles`

One app-level profile, `[Basic]`, with 18 rules, all `RULE_IS_DISABLED` except the stock set.
Rule data is dumped in full (`ADBE_BlankPages`, `ADBE_BleedSlug` with 9 pt bleed / 18 pt slug
thresholds, and the rest).

### Counts and absences

| Query | Result |
| :--- | :--- |
| `app.menuActions.length` | **5011** |
| `app.menus.length` | 151 |
| `app.scriptMenuActions.length` | 2 |
| `app.fonts.length` | **2115** faces across **446** families |
| `app.userDictionaries.length` | 60 |
| `app.textVariables` | 10 built-ins |
| `app.paragraphStyles` | `[No Paragraph Style]`, `[Basic Paragraph]` |
| `app.characterStyles` | `[None]` |
| `app.objectStyles` | `[None]`, `[Basic Graphics Frame]`, `[Basic Text Frame]`, `[Basic Grid]` |
| `app.inks` | Process Cyan / Magenta / Yellow / Black |

**Properties the API does not have** (each raised
`Object does not support the property or method …`): `app.keyboardShortcutSets`,
`app.findChangeQueries`, `app.activeWorkspace`, `app.workspaces`, `app.typePreferences`,
`app.printPreferences`, `app.documentPreset`, `app.layoutAdjustmentPreferences`,
`app.storyWindowPreferences`, `app.tocStyles`, `app.contentPlacerPreferences`,
`app.toolsPreferences`, `app.panelPreferences`, `app.objectStyleOptions`, `app.textDefault`
(singular), `app.metadataPreferences`, `app.glyphSets`, `app.autoCorrectTables[i].language`,
`app.dashedStrokeOptions` / `dottedStrokeOptions` / `stripedStrokeOptions`, `app.kinsokuTable`,
`app.trapPreset`, `app.mojikumiUiPreferences`, `app.gridDataInformation`,
`app.htmlFXLExportPreferences`, `app.ePubFixedLayoutExportPreferences`,
`app.pdfExportPreferences.exportWhich`, `app.swatches[i].model` for the `None` swatch,
`app.fontLockingPreferences`. Keyboard-shortcut sets and saved Find/Change queries are reachable
only through the filesystem, which is why §1 matters.

`app.interactivePDFExportPreferences` and `app.ePubExportPreferences` were not expanded (skipped per
brief); `app.pdfExportPreferences` is dumped whole in `app-preferences.json`.

---

## 02b `Default Template.indt`

Opened as `Untitled-1`, `saved = false`, `converted = false`. The `.indt` on disk was not touched.

### Document geometry

| Property | Value (in) | Value (pt) |
| :--- | ---: | ---: |
| page width × height | 8.5 × 11 | 612 × 792 |
| orientation | portrait | |
| facing pages | **false** | |
| pages | 1 | |
| intent | `PRINT_INTENT` | |
| margin top | 0.625 | **45** |
| margin bottom | 0.69144444167614 | **49.784** |
| margin left / right | 0.625 / 0.625 | 45 / 45 |
| columns | 1, gutter 0.1666667 | 12 |
| bleed (uniform) | 0.125 | **9** |
| slug | 0 | 0 |
| start page number | 1, Arabic, single section | |
| ruler origin | `PAGE_ORIGIN` | |
| measurement units | inches / inches, type in points | |

### Colour and transparency

| Property | Value |
| :--- | :--- |
| CMYK profile | **GRACoL2013_CRPC6.icc** |
| RGB profile | **Adobe RGB (1998)** |
| CMYK / RGB policy | `PRESERVE_EMBEDDED_PROFILES` |
| solid / image / after-blending intent | `USE_COLOR_SETTINGS` |
| transparency blend space | **CMYK** |
| overprint black | true |

### Baseline grid

| Property | Value (in) | Value (pt) |
| :--- | ---: | ---: |
| `baselineStart` | 0.10022222499053 | **7.216** |
| `baselineDivision` | 0.20833333333333 | **15** |
| `baselineGridRelativeOption` | `TOP_OF_MARGIN` | |
| `baselineGridShown` | **false** | |
| `baselineViewThreshold` | 75 % | |
| document grid | hidden, 1 in / 8 subdivisions, in back | |

The arithmetic is exact. Text height = 792 − 45 − 49.784 = **697.216 pt**, and
7.216 + 46 × 15 = 697.216, so there are 47 baselines and the 47th lands precisely on the bottom
margin. The odd bottom margin exists to make that true.

7.216 pt = 656/1000 em at 11 pt. The real cap height of the installed Source Sans 3 variable font is
**660/1000** (`OS/2.sCapHeight` = 660, and the `H` glyph bbox tops out at 660), which is 7.26 pt at
11 pt. The grid's first line and the `Text Frame` object style's `CAP_HEIGHT` first-baseline offset
are therefore **0.044 pt apart**. Harmless while `alignToBaseline` is off; wrong the moment it is
turned on and a frame is expected to start on the grid without snapping.

### Layers, masters, pages

One layer, `Layout` (visible, unlocked, printable, light blue, 0 items).

| Master | Base | Pages | Spread guides | Page items |
| :--- | :--- | ---: | ---: | :--- |
| `A-Blank` | — | 1 | **24** | 0 |
| `R-Running` | `A-Blank` | 1 | 0 | 2 |

`A-Blank`'s 24 guides are a real modular grid, all on the `Layout` layer, unlocked, threshold 5 %:

- **12 vertical** at 45, 122, 134, 211, 223, 300, 312, 389, 401, 478, 490, 567 pt.
  That is **6 columns of 77 pt with 12 pt gutters**; 6 × 77 + 5 × 12 = 522 pt = the 7.25 in
  text width.
- **12 horizontal** at 45, 142.216, 165, 262.216, 285, 382.216, 405, 502.216, 525, 622.216, 645,
  742.216 pt. That is **6 rows of 97.216 pt with 22.784 pt gutters**, i.e. a **row pitch of exactly
  120 pt = 8 baselines**, and 6 × 120 − 22.784 = 697.216 = the text height. Each row is
  7.216 + 6 × 15, so a row holds 7 baselines.

`R-Running` adds two frames, both on `Layout`, both using the `Text Frame` object style:

| Name | Label | Bounds (in, T L B R) | Bounds (pt) |
| :--- | :--- | :--- | :--- |
| Automatic Page Number | `sidekick_automatic_page_number` | 10.5599, 0.625, 10.7252, 7.875 | 760.31, 45, 772.22, 567 |
| Running Section Header | `sidekick_running_section_header` | 0.293, 0.625, 0.4583, 7.875 | 21.1, 45, 33, 567 |

Both labels are `sidekick_*`. **This template was generated by Sidekick**, not drawn by hand. Both
frames sit outside the text block (header above the 45 pt top margin, folio below the 742.216 pt
bottom margin), which is correct.

The document's single page applies **`A-Blank`**, not `R-Running`. Running head and folio are
therefore present in the file but not on the page a new document opens with.

Two stories exist, each one character long, each holding `` — the auto page-number and
section-marker placeholders in those two frames. `allPageItems` = 2, `links` = 0. There is no body
text anywhere, which matches "should be none".

### Paragraph style graph

24 styles, no style groups (`allParagraphStyleGroups` is not a property of `Document` in this build;
`doc.paragraphStyleGroups.length` is 0 in the dump). Spacing shown in points.

| Style | Based on | Next | Font | Size/Lead | Align | Before | After | First | Left | Baseline | Hyph |
| :--- | :--- | :--- | :--- | ---: | :--- | ---: | ---: | ---: | ---: | :--- | :--- |
| `[No Paragraph Style]` | root | — | Minion Pro Regular | 12/auto | left | 0 | 0 | 0 | 0 | off | on |
| `[Basic Paragraph]` | — | self | Minion Pro Regular | 12/auto | left | 0 | 0 | 0 | 0 | off | on |
| **Text Base** | — | Body | Source Sans 3 Regular | **11/15** | left | 0 | 0 | 0 | 0 | off | **off** |
| Body | Text Base | Body | Source Sans 3 Regular | 11/15 | left | 0 | **6** | 0 | 0 | off | on |
| Heading 1 | Text Base | Body | Source Sans 3 Bold | 18/21 | left | **18** | 6 | 0 | 0 | off | off |
| Heading 2 | Heading 1 | Body | Source Sans 3 Bold | 14/18 | left | 12 | 3 | 0 | 0 | off | off |
| Heading 3 | Heading 2 | Body | Source Sans 3 Bold | 11/15 | left | 9 | 3 | 0 | 0 | off | off |
| Title | Heading 1 | Subtitle | Source Sans 3 Bold | 30/33 | left | 0 | 6 | 0 | 0 | off | off |
| Subtitle | Text Base | Body | Source Sans 3 Regular | 14/18 | left | 0 | 15 | 0 | 0 | off | off |
| Caption | Text Base | Caption | Source Sans 3 Regular | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Note | Caption | Note | Source Sans 3 Regular | 9/12 | left | 0 | 3 | −18 | 18 | off | off |
| Running Header | Caption | self | Source Sans 3 Regular | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Folio | Running Header | self | Source Sans 3 Regular | 9/12 | **right** | 0 | 0 | 0 | 0 | off | off |
| Bullets | Body | self | Source Sans 3 Regular | 11/15 | left | 0 | 6 | −12 | 12 | off | on |
| Numbered | Body | self | Source Sans 3 Regular | 11/15 | left | 0 | 6 | −6 | 30 | off | on |
| Numbered First | Numbered | Numbered | Source Sans 3 Regular | 11/15 | left | 0 | 6 | −6 | 30 | off | on |
| Figure Caption | Caption | Caption | Source Sans 3 Regular | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Table Caption | Caption | Body | Source Sans 3 Regular | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Table Body | Caption | self | Source Sans 3 Regular | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Table Header | Table Body | Table Body | Source Sans 3 **Bold** | 9/12 | left | 0 | 0 | 0 | 0 | off | off |
| Table Numeric | Table Body | self | Source Sans 3 Regular | 9/12 | **right** | 0 | 0 | 0 | 0 | off | off |
| Contents Title | Heading 1 | Contents 1 | Source Sans 3 Bold | 18/21 | left | 18 | 6 | 0 | 0 | off | off |
| Contents 1 | Body | self | Source Sans 3 Regular | 11/15 | left | 0 | 0 | 0 | 0 | off | off |
| Contents 2 | Contents 1 | self | Source Sans 3 Regular | 11/15 | left | 0 | 0 | 0 | 12 | off | off |

Shared across every style (inherited from `Text Base`): language *English: USA*, fill `Black` at
100 %, tracking 0, Metrics kerning, composer **Adobe Paragraph Composer**, `spanColumnType`
`SINGLE_COLUMN`, no drop cap, no rule above/below, no paragraph shading, no border, no nested style,
no GREP style, no line style, `leadingModel` `AKI_BELOW`, `characterAlignment` `ALIGN_EM_CENTER`,
`otfFigureStyle` `PROPORTIONAL_LINING`, `otfContextualAlternate` true, ligatures true,
`otfStylisticSets` 0, `balanceRaggedLines` off, `gridAlignFirstLineOnly` **false** everywhere.

Justification settings, uniform: word spacing 90/100/110 %, letter spacing −2/0/2 %, glyph scaling
98/100/102 %, single-word `FULLY_JUSTIFIED`. Hyphenation, where on: words longer than 5, after
first 2, before last 2, ladder limit 3, capitalised words **false**, last word **false**, across
columns **false**, zone 0.5 in.

Keep options: every heading (`Heading 1/2/3`, `Title`, `Subtitle`, `Contents Title`, `Table Caption`)
has `keepWithNext = 2` and `keepAllLinesTogether = true`; `Body`, `Bullets`, `Numbered`, `Note`
have `keepLinesTogether = true` with first/last 2 (widow and orphan control); `Table Body/Header/
Numeric` have keeps off. `startParagraph` is `ANYWHERE` throughout — no style forces a page or
column break.

Tab stops: `Note` one left tab at 18 pt; `Bullets` one at 12 pt; `Numbered` and `Numbered First`
one at 30 pt. Every other style has none.

Lists: `Bullets` is a `BULLET_LIST`; `Numbered` and `Numbered First` are `NUMBERED_LIST` on the
`Body List` numbering list, numbering expression `^#.^t`, format `1, 2, 3, 4…`, alignment
**right**, marker character style **Tabular Figures**. `Numbered First` differs from `Numbered`
only in `numberingContinue = false` — it restarts a list. Numbering lists in the document:
`[Default]`, `Body List`, `Figures`, `Tables`; only `Body List` is referenced by a style.

### Character styles

Seven, all based on `[None]`, each setting exactly one attribute and leaving ~130 others at
`NothingEnum.NOTHING`. This is the right way to build them.

| Style | The one attribute |
| :--- | :--- |
| `Emphasis` | `fontStyle = Italic` |
| `Strong` | `fontStyle = Bold` |
| `Strong Emphasis` | `fontStyle = Bold Italic` |
| `No Break` | `noBreak = true` |
| `Tabular Figures` | `otfFigureStyle = TABULAR_LINING` |
| `Note Reference` | `position = OT_SUPERSCRIPT` |

### Object styles

| Style | Based on | Enabled categories | Notable |
| :--- | :--- | :--- | :--- |
| `Text Frame` | — | fill, stroke, text-frame general/baseline/auto-size, story, para style | `firstBaselineOffset = CAP_HEIGHT`, inset 0, 1 column, `verticalJustification TOP`, auto-size **off** |
| `Figure` | — | fill, stroke, frame fitting | `fittingOnEmptyFrame = PROPORTIONALLY`, `autoFit` false |
| `Caption` | `Text Frame` | + paragraph style | applies para style `Caption`; **`autoSizingType = HEIGHT_ONLY`** from `TOP_CENTER_POINT`; `firstBaselineOffset = CAP_HEIGHT` |

Plus the four built-ins. No object style enables transparency, drop shadow, or any effect; none
enables anchored-object options, so anchored frames fall back to `INLINE_POSITION`. No object style
turns on a custom baseline frame grid (`useCustomBaselineFrameGrid` false everywhere,
`baselineFrameGridIncrement` left at the 12 pt default).

### Table and cell styles

| Table style | Based on | Header | Body | Footer | Left col | Right col | Space before/after | Borders |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | ---: | ---: |
| `Table` | — | `Header` | `Body` | `Body` | `Body` | `Body` | 0 / 0 | 0 pt all four |

| Cell style | Based on | Para style | Insets (T B L R, pt) | Vertical | Strokes |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Body` | — | `Table Body` | 3, 3, 5, 5 | `TOP_ALIGN` | 0 pt all edges, fill `None`, `firstBaselineOffset CAP_HEIGHT` |
| `Header` | `Body` | `Table Header` | inherit | inherit | **bottom edge 0.5 pt** |
| `Numeric` | `Body` | `Table Numeric` | inherit | inherit | inherit |

`Header` and `Numeric` override only what they must — same discipline as the character styles.

### TOC style

| TOC style | Title | Title style | Entries |
| :--- | :--- | :--- | :--- |
| `[Default]` | Contents | `[No Paragraph Style]` | none |
| **`Contents`** | Contents | `Contents Title` | `Heading 1` → level 1, format `Contents 1`; `Heading 2` → level 2, format `Contents 2` |

Both entries use `pageNumberPosition = AFTER_ENTRY`, separator `^y` (right-indent tab),
`separatorStyle` `[No character style]`, `sortAlphabet` false. `Contents` has
`numberedParagraphs = EXCLUDE_NUMBERS`, `createBookmarks` true, `runIn` false,
`includeBookDocuments` false.

### Swatches and colour groups

| Group | Members |
| :--- | ---: |
| `[Root Color Group]` | 4 — `None`, `Registration`, `Paper`, `Black` (all CMYK process) |
| `Default Palette / Base` | 13 — RGB White, RGB Black, True Gray, Red, Green, Blue, RGB Yellow, RGB Cyan, RGB Magenta, Orange, Purple, Brown, Base Beige |
| `Default Palette / Expanded` | 79 — a named hue system (Persian Plum, Carmine, Turkish Red, … Gunmetal, Davy's Gray, Platinum, Seasalt, Snow, White Smoke) |

96 swatches total. **Every named swatch is RGB in a CMYK-blend, GRACoL-profiled print document.**
Their component values are also non-integral — `True Gray` is
`126.99867005107 / 126.985226167315 / 127.007204523346`, `Beige` is
`244.392783317121 / 243.62969661965 / 220.124589615759`. They were authored elsewhere (an OKLCH or
Lab source, most likely) and round-tripped through a float conversion instead of being written as
integers.

### Everything else in the template

| Item | State |
| :--- | :--- |
| Sections | 1, unnamed, starts at page 1, Arabic, no prefix |
| Conditions / condition sets | **none** |
| Hyperlinks / destinations / bookmarks | **none** |
| Cross-reference formats | 9, all stock |
| Text variables | 10, all stock; `Running Header` matches `[Basic Paragraph]` (not `Heading 1`) |
| XML tags | `Root` only |
| Embedded preflight profile | **`Default Preflight`** (1 profile) |
| Document-level PDF presets | none |
| Footnote options | Arabic, start 1, no restart, superscript marker, separator tab, `[Basic Paragraph]` as footnote text style |
| Links | 0 |
| Fonts used | **Source Sans 3 Regular**, `OPENTYPE_TT`, `INSTALLED`, from `~/Library/Application Support/design-tools/fonts/google/source-sans-3/source-sans-3-regular-variable.ttf` |
| Document preset reference | InDesign records none on a document — the link is one-way, preset → file path |

All 16 Source Sans 3 instances (ExtraLight → Black, plus italics) report `INSTALLED`, served from
two variable TTFs in the `design-tools` font store. **No missing font in any of the four files.**

---

## 02c The three comparison documents

### `School Document Template.indd` — the user's old default

Found at `…/98.Templates/02.Documentation Templates/99.School & Personal Templates/School Document
Template.indd`, not the path in the brief.

| Property | Value |
| :--- | :--- |
| Page | 8.5 × 11 in Letter portrait, 1 page, facing **false** |
| Margins | top 51 pt, bottom 66 pt, left/right 36 pt, 1 column, 12 pt gutter |
| Bleed | 9 pt uniform |
| Baseline grid | start **0**, division **15 pt**, relative to **top of margin**, **shown = true** |
| Colour | CMYK `U.S. Web Coated (SWOP) v2`, RGB `sRGB IEC61966-2.1`, blend space **RGB** |
| Text defaults | Neue Haas Grotesk Text Pro 55 Roman **12.75/15**, `alignToBaseline` **true** |
| Layers | 5: `Grid Lines`, `Hanglines`, `Notation Layer`, `Graphics Layer`, `Main Layer` |
| Masters | `A-Text Body`, `B-Cover Page`, `C-Table of Contents` (45 horizontal guides each, 15 pt pitch from 56.438 pt), `D-MLA Bibliography`, `E-APA Bibliography` (no guides) |
| Styles | 37 paragraph, 14 character, 4 object (built-ins only), 3 table, 4 cell |
| Swatches | 40 in `[Root Color Group]` (7) + `My Colors` (33) |
| Fonts | Neue Haas Grotesk Display Pro 75 Bold, Text Pro 55 Roman, Text Pro 75 Bold — all `INSTALLED` from Adobe Fonts |
| Content | 24 page items, 20 stories |

Its size ladder is the Kelman one: 51/60, 42/60, 38.25/45, 25.5/30, 19.125/22.5, 12.75/15,
9.5625/11.25. **Size is 0.85 × leading in every case**, and every leading is a rational multiple of
15 (0.75×, 1×, 1.5×, 2×, 3×, 4×). Space-after values are 15 or 30 pt. `alignToBaseline` is true on
every body, quote, caption, TOC and bibliography style; it is false only on the display headers and
the watermark. Character styles are colour and weight roles (`Grey/White/Red/Blue Text`, `Bold`,
`Italic`, `Bold & Italic`, `Highlight - Body Text`, `Hyperlink`, `Footnote Superscript`,
`TOC Number Style`). No style has a `basedOn` parent — 37 flat styles.

### `US Letter Portrait Grid System.indt` (Kelman)

| Property | Value |
| :--- | :--- |
| Page | 792 × 612 pt — **landscape**, despite the file name; facing pages **true**, 18 pages, 10 spreads |
| Margins | 36 pt all round, 12 pt gutter |
| Bleed | 8.50393700787402 pt (3 mm) |
| Baseline grid | start 5 pt, division **10 pt**, relative to **top of page**, shown |
| Units | **points**, ruler `SPREAD_ORIGIN` |
| Colour | CMYK `Coated FOGRA39`, RGB `sRGB`, blend **CMYK** |
| Text defaults | Neue Haas Grotesk Text Pro 55 Roman **8.5/10**, `alignToBaseline` **true**, language *English: UK* |
| Master | one, `A-Master`, 2 pages, 28 spread guides (verticals at 25 / 574.5 / 649.5 / 1199 pt; horizontals in 110 pt bands with 10 pt gaps) |
| Styles | 24 paragraph, 0 character, 0 custom object/table/cell |
| Swatches | 5; one custom, `R=26 G=26 B=26` |
| Content | 98 page items, 71 stories |

Style names encode their own metrics: `34/40 Style A White`, `25.5/30 Style B`, `17/20 Style C`,
`12.75/15 Style D`, `8.5/10 Style E/F`, `8.5/10 Body`, `8.5/10 Caption`, `6.375/8.5 Small`, each in
a White and a Black variant. Again **size = 0.85 × leading** throughout, and on this document the
module is 10 pt rather than 15.

### `Digital Presentation Landscape Grid System v2.indd` (Kelman)

| Property | Value |
| :--- | :--- |
| Page | 1280 × 720 px, `Android (1280 x 720)`, landscape, 13 pages, facing **false** |
| Intent | **`WEB_INTENT`**, units **pixels**, bleed 0 |
| Margins | 36 px all round, 12 px gutter |
| Baseline grid | start 0, division **15 px**, relative to **top of page**, hidden |
| Colour | CMYK `Coated FOGRA39`, RGB `sRGB`, blend **RGB** |
| Text defaults | Neue Haas Grotesk Text Pro 55 Roman 9.5625/11.25, `alignToBaseline` **true** |
| Layers | `f-height`, `Layer 1` |
| Master | one, `A-Master`, 67 page guides; verticals give **4 columns of 286.936 px with 20.45 px gutters**; horizontals run on a 15 px pitch with extra f-height pairs |
| Styles | 24 paragraph in Black / White / Grey variants, 0 character |
| Swatches | 6; `Accent Colour` and `R=26 G=26 B=26` |
| Content | 116 page items, 88 stories |

Ladder: 68/80, 59.5/70, 51/60, 38.25/45, 25.5/30, 19.125/22.5, 12.75/15, 9.5625/11.25 — **0.85 ×
leading** again, leadings on the 15 px module except 70 and 80.

---

## 03 What the Default Template gets right and wrong against a 15 pt baseline

### Right

1. **The page arithmetic closes.** 45 pt top, 49.784 pt bottom, 7.216 pt first baseline and 15 pt
   division give exactly 47 lines with the last on the bottom margin. The bottom margin is odd on
   purpose and it is correct.
2. **The column grid closes.** 6 × 77 pt + 5 × 12 pt = 522 pt = the full 7.25 in measure.
3. **The row grid is a baseline multiple.** 120 pt pitch = 8 baselines, 6 rows spanning the full
   697.216 pt text height.
4. **One root style.** Everything descends from `Text Base`, which owns the font, language,
   justification and hyphenation settings. Changing the family is a one-field edit.
5. **Character styles are single-attribute and based on `[None]`.** Seven styles, each setting one
   thing. They compose with any paragraph style without dragging in a font or a size.
6. **Cell styles override only what they must.** `Header` sets one stroke, `Numeric` sets nothing
   but its paragraph style; both inherit insets and vertical alignment from `Body`.
7. **Keep options are real.** Headings keep with next and keep all lines together; body, list and
   note styles have 2-line widow/orphan control. Nothing forces a break.
8. **Headings do not hyphenate.** `Text Base`, all three headings, `Title`, `Subtitle`, captions and
   table styles have hyphenation off; only running body text has it on.
9. **Numbering is set up properly.** A named `Body List`, right-aligned numbers, a `Tabular Figures`
   character style on the marker, and a `Numbered First` that restarts.
10. **Bleed is 9 pt, uniform, with the document in GRACoL 2013 CRPC6 and CMYK blending.** That is a
    coherent print setup.
11. **The TOC style is wired to the heading styles** and formats through `Contents 1` / `Contents 2`,
    with a right-indent-tab separator.
12. **An embedded preflight profile** travels with the file.

### Wrong

1. **`alignToBaseline` is `false` on all 24 paragraph styles, and `baselineGridShown` is `false`.**
   The entire grid described above is decorative. Nothing snaps to it, nothing displays it. Both of
   the user's own references (`School Document Template`, both Kelman grids) set `alignToBaseline`
   true on every text style. This is the single biggest defect.

2. **Only 4 of 22 named styles resolve to a 15 pt multiple.** Taking leading + spaceBefore +
   spaceAfter for a one-line paragraph:

   | Style | leading + before + after | pt | On grid |
   | :--- | :--- | ---: | :--- |
   | Text Base | 15 + 0 + 0 | 15 | ✅ |
   | Body | 15 + 0 + 6 | **21** | ❌ |
   | Heading 1 | 21 + 18 + 6 | 45 | ✅ (3 × 15) |
   | Heading 2 | 18 + 12 + 3 | **33** | ❌ |
   | Heading 3 | 15 + 9 + 3 | **27** | ❌ |
   | Title | 33 + 0 + 6 | **39** | ❌ |
   | Subtitle | 18 + 0 + 15 | **33** | ❌ |
   | Caption | 12 + 0 + 0 | **12** | ❌ |
   | Note | 12 + 0 + 3 | 15 | ✅ |
   | Bullets | 15 + 0 + 6 | **21** | ❌ |
   | Numbered / Numbered First | 15 + 0 + 6 | **21** | ❌ |
   | Figure Caption | 12 + 0 + 0 | **12** | ❌ |
   | Table Caption | 12 + 0 + 0 | **12** | ❌ |
   | Contents Title | 21 + 18 + 6 | 45 | ✅ |
   | Contents 1 / 2 | 15 + 0 + 0 | 15 | ✅ |

   `Body` is the style that matters most and it is 6 pt off every paragraph. After four paragraphs
   the text has drifted 24 pt — more than a line and a half.

3. **The 6 pt space-after is the culprit and it has no defence.** 6 pt is 0.4 of the module. There
   is no combination of paragraphs for which it resolves. Either it goes to 0 with
   `alignToBaseline` on (the Kelman answer), or it goes to 15 pt (the School template's answer).

4. **Leadings 12, 18, 21 and 33 are not on the module.** Only 15 is. 12 pt captions, 18 pt
   H2/Subtitle, 21 pt H1, 33 pt Title each need their own correction. Against a 15 pt module the
   available leadings are 7.5, 11.25, 15, 22.5, 30, 45, 60.

5. **The size-to-leading ratio is inconsistent**, which is why the ladder feels arbitrary:

   | Style | Size/Lead | Ratio |
   | :--- | ---: | ---: |
   | Body, Heading 3, Bullets, Numbered, Contents | 11/15 | 0.733 |
   | Caption, Note, Folio, Table styles | 9/12 | 0.750 |
   | Heading 2, Subtitle | 14/18 | 0.778 |
   | Heading 1, Contents Title | 18/21 | 0.857 |
   | Title | 30/33 | 0.909 |

   Every reference file the user owns uses **0.85 exactly**. The 15 pt ladder that satisfies both
   the module and the 0.85 rule is: 6.375/7.5, 9.5625/11.25, **12.75/15**, 19.125/22.5, 25.5/30,
   38.25/45, 51/60.

6. **The first baseline is 0.044 pt off the font.** `baselineStart` 7.216 pt encodes a 656/1000 cap
   height; Source Sans 3's is 660/1000 = 7.26 pt at 11 pt. The `Text Frame` object style uses
   `firstBaselineOffset = CAP_HEIGHT`, so an unsnapped frame starts 0.044 pt below the grid. The
   value should be 7.26, or the offset should be `LEADING`.

7. **The template's own page does not use the master that has the running head and folio.**
   Page 1 applies `A-Blank`; `R-Running` is orphaned.

8. **`R-Running` inherits `A-Blank` but carries none of its guides** (0 spread guides against 24) —
   the dump shows master-guide inheritance is not happening, so any page on `R-Running` loses the
   modular grid.

9. **96 RGB swatches in a CMYK-blended, GRACoL-profiled print document.** They will all convert at
   output, and none of them is a spot. Their components are floats
   (`126.99867005107` rather than `127`), so round-tripping through any other tool will not
   reproduce them exactly.

10. **No object style enables a custom baseline frame grid.** With the document grid relative to
    `TOP_OF_MARGIN` and no frame-level grid, any frame that does not start at the top margin has no
    correct grid to snap to.

11. **`Caption`'s object style auto-sizes height from `TOP_CENTER_POINT`.** An auto-growing frame and
    a fixed baseline grid are in tension; the frame will grow in 12 pt caption lines against a 15 pt
    page grid.

12. **`opticalMarginAlignment` is off** at app and document level, and no style sets a GREP style, a
    nested style, or a stylistic set. For a template whose whole point is typographic defaults, the
    optical margin is a free win that is not taken.

13. **Facing pages is false while margins are symmetric.** For a report template that is fine; but
    the app-level default is facing = true, so a document created without the preset diverges.

14. **The `Running Header` text variable matches `[Basic Paragraph]`, not `Heading 1`.** The running
    head frame exists but the variable that would fill it points at the wrong style.

15. **No conditions, no hyperlink destinations, no bookmarks, no document-level PDF preset.** For a
    report template that ships a TOC and cross-reference formats, a PDF preset with bookmarks on is
    the missing half.

### Grid summary in one line

The geometry is exact to 0.001 pt, the type is 6 pt out of step on every paragraph, and the switch
that would reconcile them (`alignToBaseline`) is off.

---

## 04 World-Ready composer in this non-ME build

The probe is read-only; nothing was set.

**What the API exposes.** `composer` is a plain `String` property, not an enumeration. There is no
`ComposerType` enum to read, and `app.storyPreferences.composer` does not exist in this build
(`Object does not support the property or method 'composer'`). The readable values:

| Where | Value | Type |
| :--- | :--- | :--- |
| `app.textDefaults.composer` | `Adobe Paragraph Composer` | string |
| `app.paragraphStyles[0].composer` (`[No Paragraph Style]`) | `Adobe Paragraph Composer` | string |
| `app.paragraphStyles.itemByName("[Basic Paragraph]").composer` | `Adobe Paragraph Composer` | string |
| Every style in all four documents | `Adobe Paragraph Composer` | string |

Because the property is a string, the DOM cannot enumerate the accepted values. The composers the
build actually registers must come from the plug-in that owns them.

**What the build ships.** Two pieces of evidence, both read-only:

1. `/Applications/Adobe InDesign 2026 (Beta)/…/Contents/MacOS/Required/` contains
   **`WorldReady.InDesignPlugin`** alongside `Paragraph Composer.InDesignPlugin`. The WorldReady
   binary carries the localised menu string `Apply Adobe World-Ready Composers`.
2. `Paragraph Composer.InDesignPlugin` contains the composer name table, and it holds all six
   entries:

   ```
   Adobe Paragraph Composer
   Adobe Single-line Composer
   Adobe World-Ready Paragraph Composer
   Adobe World-Ready Single-line Composer
   Adobe Japanese Paragraph Composer
   Adobe Japanese Single-line Composer
   ```

   (with localised variants: `Adobe World-Ready Styckedisposition`, `Adobe World-Ready
   Enradsdisposition`, `Adoben World-Ready Paragraph -koostaja`, and so on.)

**Corroborating DOM evidence.** The World-Ready attribute set is present on every paragraph style
and on `app.textDefaults` in this build: `paragraphDirection` (`LEFT_TO_RIGHT_DIRECTION`),
`characterDirection` (`DEFAULT_DIRECTION`), `digitsType` (`DEFAULT_DIGITS`), `kashidas`
(`DEFAULT_KASHIDAS`), `diacriticPosition` (`OPENTYPE_POSITION_FROM_BASELINE`). Those properties only
exist when the World-Ready plug-in is loaded. `app.textPreferences.shapeIndicAndLatinWithHarbuzz` is
also present and **true**.

**Conclusion.** `"Adobe World-Ready Paragraph Composer"` is a registered composer name in this
non-ME 21.6.0.58 build and assigning it to `paragraphStyle.composer` or
`app.textDefaults.composer` will be accepted. What is *not* present is the UI affordance: the menu
action `Apply Adobe World-Ready Composers` is not in `app.menuActions` (looked up by name,
`isValid` false), so the composer is reachable by script and by the Paragraph panel menu, not by
that command. Nothing was assigned during this probe, so this is inference from the plug-in's own
string table plus the presence of the World-Ready property set — not an executed round trip.

The practical note: switching to the World-Ready composer changes line breaking for Latin text too
(it uses a different justification engine), so it is a deliberate typographic choice, not a free
upgrade. It is worth it only if the template must set Arabic, Hebrew, or Indic.

---

## 05 Sidekick

### What is installed

| Item | Value |
| :--- | :--- |
| CLI package | `@indesign-mcp/server` **1.0.27**, private, ESM |
| Install root | `/usr/local/lib/indesign-sidekick/` (root-owned, oclif macOS `.pkg`) |
| Symlink | `/usr/local/bin/indesign-sidekick` → `…/bin/indesign-sidekick` |
| Bundled Node | `bin/node`, 89.8 MB — the package ships its own runtime |
| Binaries declared | `indesign-mcp-server` → `./dist/index.js`; `indesign-sidekick` → `./bin/run.js` |
| oclif identity | bin `indesign-sidekick`, macOS identifier `nl.eastpole.indesign-sidekick`, signed `Developer ID Installer: Wilfred Springer (47YAXF5AQS)` |
| Update channel | S3, `https://releases.sidekick.eastpole.nl`, bucket `indesign-sidekick-releases` |
| Targets | `darwin-arm64`, `darwin-x64`, `win32-x64` |
| Hot-loaded implementation | `~/Library/Application Support/indesign-sidekick/1.0.27/` — `impl.js` (8.55 MB, minified), `vips-node.mjs`, `vips.wasm`, `version.txt` |
| User config | `~/Library/Application Support/indesign-sidekick/config.json` → `{"telemetry": false}` |
| `version.txt` | `1.0.27` |
| UXP panel | `~/Library/Application Support/Adobe/UXP/Plugins/External/8ebe7f95_1.0.22/` |
| Running? | **No.** `lsof -iTCP:6001 -sTCP:LISTEN` returns nothing; no `indesign-sidekick` process in `ps`. |

`dist/` layout: `commands/` (`start`, `register`, `unregister`), `tools/` (8 modules),
`lib/` (claude-desktop, code-transforms, config, font-metrics, health-schema, launcher-update,
paths, posthog, telemetry, tool-schema, update-notice, update-state, vips-embedded, vips-init),
`resources/` (enum-lookup, snapshot-view, snapshot-view-html), `data/enum-mappings.json`,
plus `launcher.js`, `multi-client-bridge.js`, `protocol.js`, `impl.js`, `index.js`, `tool-types.js`,
`types.js`.
`scripts/`: `postinstall.sh`, `pack-mcpb.js`, `pack-impl.js`, `patch-oclif-win-installer.js`,
`generate-vips-embedded.mjs`, `preview-snapshot-view.ts`, `silent-server.mjs`, three smoke tests,
`render-nsis-path-smoke.js`.

Runtime dependencies: `@modelcontextprotocol/sdk ^1.24.3`, `@oclif/core ^4.8.0`, `acorn` +
`acorn-walk` + `astring` (it parses and rewrites the JS you send), `detect-port`, `opentype.js`,
`semver`, `uuid`, `wasm-vips`, `ws`, `zod ^3.25.0`. Dev deps include
`@modelcontextprotocol/ext-apps ^1.7.4` (MCP Apps) and `javascript-obfuscator` (which is why
`impl.js` is minified).

### The UXP panel manifest

```json
{
  "id": "8ebe7f95",
  "name": "Sidekick",
  "version": "1.0.22",
  "main": "index.html",
  "manifestVersion": 5,
  "host": { "app": "ID", "minVersion": "19.0" },
  "requiredPermissions": {
    "localFileSystem": "fullAccess",
    "network": { "domains": ["ws://localhost:6001", "wss://localhost:6001"] },
    "launchProcess": { "schemes": ["https"] }
  },
  "entrypoints": [{ "type": "panel", "id": "mcpPanel", "label": { "default": "Sidekick" },
                    "minimumSize": { "width": 200, "height": 100 } }]
}
```

InDesign 19.0 or newer. The panel holds **no server**; it is a WebSocket *client* that dials
`ws://localhost:6001`.

### How it actually runs — read from `launcher.js` and the log

The transport to the MCP client is **stdio** (`StdioServerTransport` from
`@modelcontextprotocol/sdk/server/stdio.js`, `launcher.js:20` and `:534`). Port 6001 is *internal*.

The architecture is a primary/secondary bridge, and `~/Library/Logs/design-tools/sidekick.log`
shows every step of it:

```
Starting with implementation v1.0.27
Port 6001 in use, starting as SECONDARY
Registered 9 tools
Telemetry: disabled
MCP server started
Connected to primary MCP server
…
Disconnected from primary
Promoting to PRIMARY
[handshake] First message … {"type":"plugin-hello","version":"1.0.22"}
[handshake] Checking version compatibility: server=1.0.27, plugin=1.0.22
[handshake] Plugin … accepted, version=1.0.22
[handshake] First message … {"type":"secondary-hello","clientId":"…","version":"1.0.27"}
[handshake] Secondary MCP server connected: …
handleSecondaryMessage: forwarding request method=execute, payloadId=…
sendToPluginDirect: method=execute, readyState=1
```

- Each MCP client spawns its own `indesign-sidekick start` process and talks to it over **stdio**.
- The first such process to get port 6001 becomes **primary** and owns the WebSocket server.
- The UXP panel connects to the primary with `plugin-hello` and stays connected; the primary
  version-checks it (`server=1.0.27, plugin=1.0.22` → accepted).
- Every later process becomes **secondary**, connects to the primary as a WebSocket client with
  `secondary-hello`, and forwards `method=execute` payloads through it.
- When the primary dies a secondary promotes itself (`Disconnected from primary` →
  `Promoting to PRIMARY`) and the panel reconnects.

So **one InDesign panel serves any number of MCP clients**, and the brief's instruction not to start
a second primary is exactly right: starting one by hand while a client owns 6001 would just create a
secondary, but killing the wrong one would drop the panel's link.

`launcher.js` also self-updates (`lib/launcher-update.js`, `lib/update-state.js`) and hot-loads the
implementation from `~/Library/Application Support/indesign-sidekick/<version>/impl.js`, appending an
`[Update: …]` notice to successful tool results.

### The nine tools

Six come from the implementation's `getTools(bridge)` and three from the launcher itself
(`launcher.js:507`, `Registered ${initialTools.length + 3} tools` → 6 + 3 = 9):

| # | Tool | Source | Purpose |
| ---: | :--- | :--- | :--- |
| 1 | `execute` | impl | Run JavaScript in InDesign **via UXP, not ExtendScript**. `app` is not global — `const { app } = require('indesign')`. Async/await supported, promises awaited, return values must be JSON-serialisable. Sandboxed FS, no `app.system()`, no BridgeTalk. It rewrites your code with acorn/astring, and prefixes results with `[Autocorrected]` when it does. |
| 2 | `snapshot` | impl | Page or spread of the active document as a base64 JPEG at a vision-model resolution (dpi is not a parameter). Optional `region` `[x0,y0,x1,y1]` normalised 0..1 to zoom. |
| 3 | `snapshot_object` | impl | One page item by id, as it appears on the page (including overlaps and clipping) or isolated. |
| 4 | `show_snapshot` | impl | Same JPEG as `snapshot`, but flagged for MCP Apps hosts so it renders inline with fit-to-width and zoom. Backed by the resource `ui://sidekick/snapshot-view`. |
| 5 | `get_layout` | impl | Page dimensions, margins and a pre-computed `contentArea` per page (max 20), recto/verso aware, `geometricBounds` as `[top,left,bottom,right]`; with `includeItems` it returns each item's `id`, `type`, `bounds`, `hasGraphic` for feeding `snapshot_object`. |
| 6 | `get_font_metrics` | impl | x-height, cap-height, ascender, descender, line gap, underline/strikeout, sub/superscript, bbox, `unitsPerEm`, from system fonts and locally cached Adobe Fonts. `refresh=true` rebuilds the cache. Powered by `opentype.js`. |
| 7 | `get_health` | launcher | Server/plugin version and connection state. |
| 8 | `get_configuration` | launcher | Read the server config. |
| 9 | `set_configuration` | launcher | Write the server config. |

The MCP server also ships a long `instructions` block telling the model InDesign's quirks — units
must be strings (`"11pt"`, not `11`), facing-page margin mirroring, recto/verso frame maths,
`collection.everyItem().getElements()` for iteration, and type-dependent property access. That
instruction text is embedded in `launcher.js`.

`image-crop.js` and `snapshot-render.js` in `dist/tools/` are helper modules, not registered tools.

### The `.mcp.json` entry for Claude Code

The vendor's own registration code (`dist/lib/claude-desktop.js`) writes exactly this server block:

```js
const SERVER_NAME = "indesign-sidekick";
const SERVER_CONFIG = { command: "indesign-sidekick", args: ["start"] };
```

into `~/Library/Application Support/Claude/claude_desktop_config.json` under `mcpServers`, which
`indesign-sidekick register` does and `indesign-sidekick unregister` undoes. The same shape is what
Claude Code needs. For this repo, where `.mcp.json` runs every stdio server under `mise exec`, the
entry would be:

```json
{
  "mcpServers": {
    "indesign-sidekick": {
      "type": "stdio",
      "command": "indesign-sidekick",
      "args": ["start"]
    }
  }
}
```

`indesign-sidekick` is already on `PATH` via `/usr/local/bin`, and the package ships its own Node,
so it needs no `mise exec` wrapper and no `node` on `PATH`. If an absolute path is preferred:
`"command": "/usr/local/lib/indesign-sidekick/bin/indesign-sidekick"`.

Nothing else is required: no port, no env var, no secret. The one runtime precondition is that
InDesign is running with the **Sidekick panel open** (Window ▸ Extensions ▸ Sidekick) — without the
panel there is no `plugin-hello` and every tool call fails at the bridge.

Optional environment overrides read by the launcher: `SIDEKICK_VERSION` (pin the hot-loaded
implementation), `SIDEKICK_VERSIONS_URL`, `SIDEKICK_DATA_DIR`, `SIDEKICK_DEV`,
`SIDEKICK_LAUNCHER_VERSION`.

The vendor documents only *"Point it to `indesign-sidekick start`"* ([sidekick.eastpole.nl/install]).
It publishes no Claude Code snippet; the block above is the stdio contract applied to `.mcp.json`.

### Vendor facts (from sidekick.eastpole.nl)

| Fact | Value |
| :--- | :--- |
| Vendor | East Pole B.V. / Wilfred Springer (NL) |
| Licence | **Proprietary**, closed source, obfuscated launcher |
| Price | **$9.95 / month per person**, 30-day free trial |
| Sold through | **Adobe Exchange**, billed by **FastSpring** — does *not* appear under Adobe Plans & Products |
| Activation | Adobe Exchange entitlement tied to an individual Adobe ID, not a key in the MCP server |
| Team licensing | **None.** No multi-seat, no volume, no annual — the vendor states the marketplace does not support it |
| Refunds | 14 days, through Adobe support with an `ADO…` order id |
| Requirements | InDesign **2024 or later** (UXP), macOS or Windows; Claude Desktop ≥ 0.10.0 for the MCPB route |
| Signing | macOS installer signed `Developer ID Installer: Wilfred Springer (47YAXF5AQS)`; **Windows installers are unsigned** |
| Telemetry | PostHog, on by default since 1.0.22 — already **disabled** in this install's `config.json` |
| npm | `@indesign-mcp/server` is **not on the public registry**; it exists only inside the installer |
| Latest version | **1.0.27**, 2026-09-07 — the installed version |
| MCPB bundle | `indesign-sidekick-1.0.27.mcpb`, one-click into Claude Desktop; the bundle is a ~250 KB launcher that downloads the implementation from `releases.sidekick.eastpole.nl/versions.json` |
| Claude Code | Named explicitly in the vendor's help (*"Claude Desktop for conversation, Claude Code for automation"*), but absent from the `/mcp` clients page and given no install snippet — use the `.pkg` and the stdio entry above |

Changelog points that matter here:

- **1.0.25** fixed a real outage: in August 2026 MCP client SDKs began rejecting draft-07
  `outputSchema`, which silently dropped `get_health`, `get_layout` and `get_font_metrics`. The fix
  moved to JSON Schema 2020-12 and **required a manual reinstall** — auto-update could not deliver
  it. This install is 1.0.27, so it is past that.
- **1.0.26** added a once-a-day notice when a reinstall-only fix is pending.
- **1.0.27** fixed a Windows installer PATH bug; irrelevant on macOS.
- 1.0.15–1.0.21 built up the vision path: higher-resolution snapshots, region zoom, per-object
  capture, MCP Apps inline preview, and a move from native image libraries to `wasm-vips`.

One caveat worth knowing: the implementation is **downloaded from the vendor's S3 at run time** and
cannot be pinned except through `SIDEKICK_VERSION`. For a repo whose standard is that a lock file
states a version once, that is a live exception.

---

## 06 Is Sidekick the right InDesign controller for Claude Code?

### Does Adobe ship an official alternative? Not for the desktop app.

`developer.adobe.com/indesign/uxp/*` has no MCP surface — UXP plugins, UXP scripts, the DOM API and
dev tools, nothing else. Adobe does ship three official MCP servers, and one of them names InDesign,
but none of them drives the application on this machine:

| Adobe server | Shipped | Transport | What it does |
| :--- | :--- | :--- | :--- |
| **Adobe for creativity** | 2026-04-28 | Remote, Streamable HTTP + OAuth, `https://adobe-creativity.adobe.io/mcp` | 50+ tools across Photoshop, Lightroom, Illustrator, Firefly, Premiere, Express, InDesign, Stock. The InDesign part is **cloud data/template merge through Firefly Services** — it cannot see or touch an open document. |
| **Adobe Express Developer MCP** | 2026-03 | — | Documentation server for add-on developers. Not automation. |
| **Firefly Creative Production Run-Workflow MCP** | enterprise | — | Runs published Firefly workflows whose nodes include "Merge InDesign data". |

Adobe's in-app AI (the InDesign Beta AI Assistant, and the Firefly AI Assistant which reached
InDesign public beta 2026-06-18) is not MCP at all and is not scriptable from Claude Code.

So: **there is no official Adobe way to drive local InDesign from Claude Code.**

### The third-party field (GitHub, 2026-09-11)

| Project | ★ | Licence | Last push | Bridge | Surface |
| :--- | ---: | :--- | :--- | :--- | ---: |
| `nutriandrea/adobe-indesign-mcp` | 13 | MIT | 2026-08-30 | stdio, InDesign 2022+ | 194 tools, 747 tests, CI, on npm as `indesign-nutria-mcp` |
| `lucdesign/indesign-mcp-server` | 38 | MIT | 2026-09-01 | **AppleScript — macOS only** | 51+ tools, clone-and-build |
| `Rinellasky/indesign-mcp` | 1 | MIT | 2026-07-28 | **UXP panel → proxy on :3001** — the closest analogue to Sidekick | 99 tools, `.dxt` + `.ccx` |
| `matrayu/adobe-mcp` | 11 | MIT | 2026-01-18 | multi-app CC suite | fork of `mikechambers/adb-mcp` |
| `MoebiusSt/indesign-scripting-mcp` | 2 | MIT | 2026-09-10 | **COM/OLE — Windows only** | Python, DOM lookup + exec |
| `zachshallbetter/indesign-mcp-server` | 19 | MIT | **2025-07-27, stale** | stdio | claims 135+ tools |
| `chris-enea/indesign-mcp` | 19 | **no licence** | 2025-06-03, stale | — | unusable |

npm has `indesign-mcp@1.0.15` (stale since 2025-06), `mcp-indesign@0.2.0` (Windows only) and
`indesign-nutria-mcp`. None has meaningful download volume.

### Verdict

**Keep Sidekick.** For this machine and this task it is the right controller, on four grounds:

1. **It is the only one whose bridge is an official UXP panel.** Every macOS alternative of
   comparable maturity drives InDesign through AppleScript/`osascript` — the same channel this probe
   used, which is fine for reading but is a separate, slower, ExtendScript-shaped API. Sidekick runs
   **UXP JavaScript**, which is the API Adobe is actually developing.
2. **It is the only cross-platform one.** AppleScript bridges are macOS-only, COM/OLE bridges are
   Windows-only. The repo's standard is portable tooling.
3. **Vision is the differentiator, not tool count.** `snapshot`, `snapshot_object` and
   `show_snapshot` render the page at a resolution a vision model can read, with region zoom and
   per-object isolation. For typography work — *is this heading landing on the grid?* — being able to
   look at the page is worth more than 194 named tools. The MIT alternatives are almost all
   write-only.
4. **`get_font_metrics` closes the loop this probe just opened.** The 0.044 pt cap-height error in
   the template is exactly the class of defect that tool exists to prevent.

The real costs, stated plainly:

- **Proprietary, $9.95/month per seat, no team licensing.** Under the licensing policy on file
  (copyleft, gated and noncommercial licences accepted, cost is the only blocker), this is the one
  thing to weigh.
- **The implementation is fetched from the vendor's S3 at run time** and is obfuscated. It can only
  be pinned through `SIDEKICK_VERSION`.
- **A nine-tool surface with one `execute` escape hatch.** That is a deliberate design — the model
  writes InDesign JavaScript rather than picking from a menu — and it suits Claude Code, but it means
  every operation's correctness rests on the model knowing the DOM. The server's embedded
  `instructions` block mitigates this (units as strings, recto/verso maths, collection iteration).
- **It needs InDesign open with the panel visible.** No headless path.

If the subscription is the blocker, the only credible free substitute is
`nutriandrea/adobe-indesign-mcp` (MIT, 194 tools, tested, InDesign 2022+), and the trade is losing
the vision tools and the UXP panel bridge.

### One operational note for this repo

Adding the entry to `.mcp.json` starts a `indesign-sidekick start` process per Claude Code session.
The first to claim port 6001 becomes primary and owns the panel; the rest become secondaries and
queue through it. That is by design and needs no configuration — but it does mean the InDesign panel
is shared serial state across every session, which is worth knowing before two agents work on
documents at once.

