# [ILLUSTRATOR_SCRIPTS]

Fourteen Illustrator scripts and eleven action sets sit on the Drive as read-only reference for the Illustrator tool surface. Nine scripts are ExtendScript bytecode and were decoded; five are plain ExtendScript and were read directly. This file records what each one does, the formulas it applies, the DOM calls it makes, the files it writes, and the defects reading it turned up.

Decoded sources are not copied into the repository; this file is the whole record of them.

Two path keys stand for the Drive locations: `<scripts>` is `<drive.root>/03.Digital Asset Database/05.Software Related Assets/00.Automation Assets/00.Adobe Illustrator/01.Scripts`, and `<actions>` is `<drive.root>/03.Digital Asset Database/05.Software Related Assets/00.Automation Assets/00.Adobe Illustrator/00.Actions`.

## [01]-[INVENTORY]

| [INDEX] | [PRODUCT]                 | [DRIVE PATH]                                            | [FORMAT]                    | [LICENSE]        |
| :-----: | :------------------------ | :------------------------------------------------------ | :-------------------------- | :--------------- |
|  [01]   | Horizontal Lockup Grid    | `<scripts>/01.Grids/Horizontal Lockup Grid.jsx`          | JSXBIN `@JSXBIN@ES@2.1@`, no wrapper comment | None stated |
|  [02]   | Vertical Lockup Guide     | `<scripts>/01.Grids/Vertical Lockup Guide.jsx`           | JSXBIN `@JSXBIN@ES@2.1@`, no wrapper comment | None stated |
|  [03]   | Condensed Lockup Grids    | `<scripts>/01.Grids/Condensed Lockup Grids.jsx`          | JSXBIN `@JSXBIN@ES@2.1@`, no wrapper comment | None stated |
|  [04]   | Instant Isometric Grids   | `<scripts>/01.Grids/Instant Isometric Grids.jsx`         | JSXBIN `@JSXBIN@ES@2.1@`, descriptive header | None stated |
|  [05]   | Add Object Guides         | `<scripts>/01.Grids/AddObjectGuides.jsx`                 | JSXBIN, JsxBlind obfuscated  | creold NOTICE    |
|  [06]   | Duplicate Artboards Pro   | `<scripts>/01.Artboard & Org/Duplicate_Artboards_Pro.jsx` | JSXBIN, JsxBlind obfuscated | creold NOTICE    |
|  [07]   | Bento Grid                | `<scripts>/01.Grids/BentoGrid.jsx`                       | JSXBIN, JsxBlind obfuscated  | creold NOTICE    |
|  [08]   | Highlight Text            | `<scripts>/04.Text Related/HighlightText/HighlightText.jsx` | JSXBIN, JsxBlind obfuscated | creold NOTICE  |
|  [09]   | Gradient Blender          | `<scripts>/05.Color Related/GradientBlender.jsx`          | JSXBIN, JsxBlind obfuscated | creold NOTICE    |
|  [10]   | Ai Command Palette        | `<scripts>/00.General/AiCommandPalette.jsx`              | Plain ExtendScript, 526 907 B | MIT            |
|  [11]   | Opacity Mask Clip         | `<scripts>/06.Clipping & Masks/Opacity Mask Clip.jsx`     | Plain ExtendScript, 5 997 B | MIT              |
|  [12]   | Trim Masks                | `<scripts>/06.Clipping & Masks/TrimMasks.jsx`            | Plain ExtendScript, 10 214 B | MIT             |
|  [13]   | Transfer Swatches         | `<scripts>/05.Color Related/02.Transfer Swatches.jsx`     | Plain ExtendScript, 11 794 B | None stated     |
|  [14]   | Sync Global Colors Names  | `<scripts>/05.Color Related/03.Sync Global Colors Names.jsx` | Plain ExtendScript, 5 046 B | MIT          |
|  [15]   | Action sets               | `<actions>/**/*.aia`                                     | Plain ASCII action files     | None stated      |

The creold NOTICE is the same block in each of the five paid wrappers: `This script is provided "as is" without warranty of any kind.` followed by `You are not allowed to:`, `- Sub-license, resell or rent it.`, `- Include it in any online or offline archive or database.` The three MIT wrappers carry `Released under the MIT license` with the opensource.org URL, plus `Free to use, not for sale`.

Gradient Blender carries a manual at `<scripts>/Guides/GradientBlender-User-Guide-En.pdf`, 8 pages, read in full and cross-checked below. Highlight Text ships `demo.ai` and `demo.jpg` beside it.

Authorship: Sergey Osokin (creold) wrote rows 05 through 09 and rows 11, 12, and 14; Josh Duncan wrote row 10; Alexander Ladygin wrote row 13. Rows 01 through 04 carry no author string.

Five decoded scripts persist settings to `Folder.myDocuments + "/Adobe Scripts/" + <script name with spaces replaced by underscores> + "_data.json"`, Highlight Text to `_presets.json`. Every one writes with a hand-rolled serializer and reads back through `new Function("return (" + text + ")")()`, so the content is ExtendScript source notation and not strict JSON despite the extension.

## [02]-[DECODER]

| [INDEX] | [FACT]          | [VALUE]                                                                       |
| :-----: | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | Name            | `jsxer`                                                                        |
|  [02]   | URL             | https://github.com/AngeloD2022/jsxer                                           |
|  [03]   | Version         | v1.7.4, released 2025-03-31, prebuilt `jsxer-v1.7.4-macOS-arm64.zip`           |
|  [04]   | License         | AGPL-3.0, C++ built with CMake                                                 |
|  [05]   | Coverage        | JSXBIN 2.x, which includes the `@JSXBIN@ES@2.1@` format Illustrator emits      |
|  [06]   | Deobfuscation   | `-b` / `--unblind` undoes JsxBlind identifier obfuscation                      |

Each wrapper holds a single `eval("@JSXBIN@ES@2.1@…")` literal, which the pipeline extracts and JSON-unescapes before decoding. Six of the nine were JsxBlind obfuscated with string tables of 240 to 501 entries; substituting every `symbol_N[k]` back to its literal or property name recovers the strings and property names.

One decoder artifact: in `--unblind` output the `<` operator inside a `for` condition is rewritten as a `symbol_N` token, 26 times in Add Object Guides and 15 times in Duplicate Artboards Pro. Those conditions read correctly in the plain decoded output.

No npm package decodes JSXBIN; the npm `jsxbin` family wraps the ExtendScript Toolkit encoder. `mendax47/jsxbin_decompiler` and `KabeerNayak/jsxbin_decompiler` are stale forks of jsxer, `codecopy/jsxbin-to-jsx-converter` and `fujiapeng/JsxbinDecompiler` are unmaintained, and `Sror/extendscript-toolkit-jsxbintojsx-plugin` requires the retired ExtendScript Toolkit.

## [03]-[LOCKUP_GRIDS]

Three scripts draw proportional guide grids around a selected logo. None has a dialog or a preferences file. Each finds or creates a layer named `Lockup Grid`, unlocks it, draws into it, and sets `locked = true`. Guide color is a fixed `RGB(232, 74, 255)`. Area text is normalized first through `convertAreaObjectToPointObject()`, and `app.coordinateSystem` is set to `ARTBOARDCOORDINATESYSTEM`.

Horizontal Lockup Grid, 64 decoded lines. From the selection it reads `left`, `top`, `width`, `height` and derives `bottom = top - height`. Twelve two-point paths are drawn. Horizontals span `left - width/2` to `left + width + width/2` at `top`, `bottom`, `top - height/2`, `top - height/4`, `top - 3*height/4`, `top + height/4`, `bottom - height/4`. Verticals span `top + height/2` to `bottom - height/2` at `left - height/4`, `left + width + height/4`, `left`, `left + width`. DOM sequence: `doc.layers.getByName` or `doc.layers.add`, then per line `doc.pathItems.add()`, `setEntirePath([[x1,y1],[x2,y2]])`, `strokeColor`, `strokeWidth = 1`. Output is stroked paths on the locked grid layer.

Vertical Lockup Guide, 112 decoded lines, with the derived values:

```text
third  = height / 3
ninth  = third / 3
wthird = width / 3
offset = left - width - 30
bottom = top - height
```

It duplicates the selection twice as side spacers at `[left - width, top]` and `[left + width, top]`, each moved into the grid layer with `ElementPlacement.PLACEATEND`. Horizontals span `left - width` to `left + 2*width` at `top`, `top - third`, `top - 2*third`; further horizontals span `offset` to `left + 2*width + 30` at `bottom`, `bottom - third`, `bottom - 2*third`, `bottom - height`. A ninths grid spans `left - width` to `left + 2*width` at `bottom - k*ninth` for k in {1, 2, 4, 5, 7, 8}. Verticals run from `top` to `bottom - height` at `left`, `left + wthird`, `left + width`, `left + width - wthird`. Numeric labels carry 1 to 3 on the thirds bands twice at `x = offset - 15`, and the repeating sequence 1,2,3,1,2,3,1,2,3 on the nine ninths at `x = left - width`. A label is a text frame with `contents = num`, font size `isSmall ? height/3 : height/2`, positioned at `[left, top - tf.height/2]`. DOM sequence adds `sel.duplicate()`, `.position`, `.move(layer, PLACEATEND)`, then `pathItems.add()` per guide with `strokeWidth = 2` and `fillColor` set, then `doc.textFrames.add()` per label with `textRange.characterAttributes.size` and `.fillColor`.

Condensed Lockup Grids, 113 decoded lines, with `third = height/3`, `wthird = width/3`, `quarter = width/4`. Horizontals span `left - wthird` to `left + width + wthird` at `top`, `top - third`, `top - 2*third`, `bottom`, `bottom - third`, `bottom - 2*third`, `bottom - height`, `top + third`, `top + 2*third`. Verticals run from `bottom - height + width + 2*wthird` down to `bottom - height` at `left`, `left + quarter`, `left + 2*quarter`, `left + 3*quarter`, `left + width`. Two duplicates of the selection rotate 90 degrees and sit at `[left - height, bottom - height + dup.height]` and `[left + width, bottom - height + dup.height]`. Labels carry 1 to 3 on the thirds at `x = left - wthird - 15` and 1 to 4 across the quarters. The label helper gains `isVert` and `width` parameters: a vertical label sits at `[left + width/2 - tf.width/2, top]`. DOM sequence matches Vertical Lockup Guide plus `dup.rotate(90)` before `.position` and `.move`.

## [04]-[ISOMETRIC_GRIDS]

Instant Isometric Grids, 165 decoded lines, no dialog and no preferences. Its wrapper header states a 72 pt grid size, a color taken from the active document color, a layer named `ISOGrids` with a sublayer named after the artboard, and that the grid can be locked, hidden, recolored, or removed from there.

Coordinates: the script saves `doc.rulerOrigin`, sets it to `[0, doc.height]` and then to `[abr[0], abr[1]]`, switches to `ARTBOARDCOORDINATESYSTEM` for the work, and restores both the coordinate system and the ruler origin at the end.

Algorithm, with `x2 = left + width/2`, step `i = 72`, ring counter `c = 1`:

1. Create three seed lines `l`, `k`, `m`, each running `x2, top - height` to `x2, bottom + height`; lines `l` and `k` additionally take `height += height` and `top += height/2`.
2. Per ring `c`: duplicate `m` at plus and minus `i*c` horizontally when `x2 + i*c <= right`; duplicate `m` at plus and minus `(i*c)/2` when `x2 + (i*c)/2 <= right`; duplicate `k` at `+i*c` and rotate −60 degrees about `Transformation.CENTER`, removing that copy and breaking the loop when its `top + height/2 <= artboard top`; duplicate `k` at `−i*c` and rotate −60 degrees; duplicate `l` at plus and minus `i*c` and rotate +60 degrees; increment `c`.
3. After the loop, rotate `l` by +60 degrees and `k` by −60 degrees and add `l`, `k`, `m` to the collection.

Clipping: `drawClip` adds a `groupItems.add()`, creates `pathItems.rectangle(abr[1], abr[0], abr[2]-abr[0], abr[1]-abr[3])` moved to the beginning of the group, moves every line to the end, sets `ab.hasSelectedArtwork = true`, and sets `group.clipped = true`. Output is one clipped group per artboard inside the sublayer `<artboard name> iso grid`.

A second variant `processArtboard` survives in the bytecode unused: it builds the grid by rotating whole arrays rather than incrementally and calls `drawClip` with one argument missing.

## [05]-[OBJECT_GUIDES]

Add Object Guides, titled "Add Object Guides v0.1", 1314 decoded lines and 1087 unblinded, 327-string table, Sergey Osokin, March 2025, Illustrator CS6 and later. It requires Illustrator version 16 or later and at least one selected object.

Guide color comes from Illustrator's own preferences rather than a constant: `255 * app.preferences.getRealPreference("Guide/Color/red" | "…green" | "…blue")`. An RGB document takes that as an `RGBColor` directly. A CMYK document converts RGB to HSV to CMYK after two corrections:

```text
if (s < 20 && v > 80) v = 50
if (s > 10)           s = 100
```

| [INDEX] | [PANEL]            | [CONTROL]                                                                    | [DEFAULT]              |
| :-----: | :----------------- | :--------------------------------------------------------------------------- | :--------------------- |
|  [01]   | Target Layer       | `Name:` edittext, 18 characters, case-insensitive match                       | `GUIDES`               |
|  [02]   | Target Layer       | `Clear Layer` checkbox                                                        | Off                    |
|  [03]   | Edges              | `Left`, `Right`, `Center X`, `Center Y`, `Top`, `Bottom`, `L Diagonal`, `R Diagonal` checkboxes | All on |
|  [04]   | Edges              | `Edges Preset:` dropdown, width 205                                           | Index 0, `Custom`      |
|  [05]   | Margins            | `Left:` `Right:` `Top:` `Bottom:` edittexts, width 62                         | `0 <units>`            |
|  [06]   | Margins            | Unlabeled make-margins-equal checkbox, disables Right, Top, Bottom            | On                     |
|  [07]   | Extend Guides To   | Radios `Selected Object`, `Active Artboard`, `Document Canvas`                | Selected Object        |
|  [08]   | Extend Ends Length | Radios `By Percentage`, `By Absolute Value`, panel disabled for Document Canvas | By Percentage        |
|  [09]   | Extend Ends Length | `Value:` edittext, width 80                                                   | `0`                    |
|  [10]   | Object Dimensions  | Radios `Geometric Bounds`, `Visible Bounds`                                   | Geometric Bounds       |
|  [11]   | Draw As            | Radios `Guidlines`, `Stroked Paths`                                           | Guidlines              |
|  [12]   | Bottom row         | `Preview` checkbox, `About` button, OK, Cancel                                | Preview off            |

Dropdown options are `Custom`, `All`, `Bounds`, `Bounds And Centers`, `Only Centers`, `Only Diagonals`. Arrow keys and `[` and `]` nudge a numeric field by 1, and by 10 with Shift held.

Preset masks in checkbox order L, T, R, B, CX, CY, LD, RD:

| [INDEX] | [PRESET]   | [MASK]                              |
| :-----: | :--------- | :---------------------------------- |
|  [01]   | `all`      | All true                            |
|  [02]   | `bounds`   | First four true                     |
|  [03]   | `center`   | CX and CY only                      |
|  [04]   | `diagonal` | LD and RD only                      |
|  [05]   | `ortho`    | First six true                      |
|  [06]   | `last`     | The user's previous checkbox state  |

Algorithm: margins convert to pixels and divide by `doc.scaleFactor`; the chosen bounds array expands by `b[0] -= mL; b[1] += mT; b[2] += mR; b[3] -= mB`; per selected object a group named `<objectName>_guides` is created in the target layer; three emitters run, each in its own try/catch, for horizontals at `top`, `bottom`, `centerY`, verticals at `left`, `right`, `centerX`, and diagonals `[left,top]→[right,bottom]` and `[right,top]→[left,bottom]`. Empty groups are removed at the end.

Each segment lengthens at both ends by distance `d`:

```text
dx  = p2x - p1x
dy  = p2y - p1y
len = sqrt(dx*dx + dy*dy)
ux  = dx / len
uy  = dy / len
p1' = [p1x - ux*d, p1y - uy*d]
p2' = [p2x + ux*d, p2y + uy*d]
```

Under `By Percentage`, `d = (dimension * pct) / 100`, where `dimension` is the object width for a horizontal, the object height for a vertical, or the diagonal's own length, and takes the artboard's corresponding dimension when the target is the artboard. Under `By Absolute Value`, `d` is the converted length.

For an artboard or canvas target, a containment test skips any segment already fully inside the rectangle, and a diagonal clips to the rectangle by line and rectangle intersection:

```text
k = (p2y - p1y) / (p2x - p1x)
b = p1y - k * p1x
candidates:
  [rect.left,           k*rect.left  + b]   kept when y is within [bottom, top]
  [rect.right,          k*rect.right + b]   kept when y is within [bottom, top]
  [(rect.top - b)/k,    rect.top]           kept when x is within [left, right]
  [(rect.bottom - b)/k, rect.bottom]        kept when x is within [left, right]
```

The segment draws only when exactly two candidates survive.

DOM sequence: `app.preferences.getRealPreference` → layer lookup or create → `layer.groupItems.add()` → per guide `pathItems.add()`, `setEntirePath`, `filled = false`, `stroked = true`, `strokeWidth = 0.25`, `strokeColor = guideColor`. Under `Guidlines` every produced item then takes `.guides = true`. The target layer rises with `zOrder(ZOrderMethod.BRINGTOFRONT)`. Output is groups named `<objectName>_guides` inside the named layer, holding either Illustrator guides or 0.25 pt stroked paths.

Preview redraws through `app.undo()` and cleans up a placeholder path named `Remove This Unused Path` in the dialog's `onClose`.

Preferences file `~/Documents/Adobe Scripts/Add_Object_Guides_data.json` holds `win_x`, `win_y`, `name`, `isClearLyr`, `left`, `centerX`, `right`, `top`, `centerY`, `bottom`, `leftDiag`, `rightDiag`, `equal`, `marginsL`, `marginsR`, `marginsT`, `marginsB`, `bounds`, `extend`, `deltaUnits`, `deltaValue`, `isGuide`.

The wrapper comment states only "Draws guidelines around selected objects with custom length", requirements of CS6 and later, and release notes "0.1 Initial version"; it documents none of the presets, margins, extension modes, bounds choice, or clipping, and no separate manual exists. The decoded behavior above is the specification.

## [06]-[ARTBOARD_DUPLICATION]

Duplicate Artboards Pro, titled "Duplicate Atboards Pro v.0.3.2", 578 decoded lines and 510 unblinded, 240-string table, Sergey Osokin, October 2020, modified April 2025, with bilingual English and Russian strings.

Configuration constants: `abNamePh: "%a"`, `autoNumPh: "%n"`, `cnvs: 16383`, `columns: 1`, `copies: 1`, `spacing: 20`, `minSpacing: 0`, `limit: 2500`, `tmpLyr: "FOR_AB_COORD"`, `lKey: "%isLocked"`, `hKey: "%isHidden"`, `uiOpacity: 0.97`, `abs: 10`.

| [INDEX] | [CONTROL]                                            | [DEFAULT]         |
| :-----: | :--------------------------------------------------- | :---------------- |
|  [01]   | Artboard dropdown, entries formatted `<n>: <name>`    | Active artboard   |
|  [02]   | `Copies` edittext, 60 by 30                           | `1`               |
|  [03]   | `Columns` edittext                                    | `1`               |
|  [04]   | `Spacing, <units>` edittext                           | `20`              |
|  [05]   | `Artboard name` edittext                              | `%a_%n`           |
|  [06]   | `Insert as last in Artboards list` checkbox           | Off               |
|  [07]   | `Copy Artwork with Artboard` checkbox                 | On                |
|  [08]   | Cancel and Ok buttons, plus a clickable GitHub credit | —                 |

A warning line appears when the document holds more than 2500 page items. Arrow keys nudge by 1 and by 10 with Shift. Copy count caps at `aiVers >= 22 ? 1000 : 100 - artboards.length`. Clicking either placeholder description line inserts `%a` or `%n` into the name field.

Canvas-fit precheck: the Illustrator canvas half-extent is 16383, and a document-relative box comes from a probe:

```text
shift = 1 + (((probe.position[0]*2 - 16384) - (probe.position[1]*2 + 16384)) / 2)
```

A 300 by 300 rectangle is placed at `[probe.pos[0] - shift, probe.pos[1] + shift]`, an artboard-sized rectangle beside it, and then:

```text
left   = floor(abRect.position[0] - probeRect.position[0])
top    = floor(probeRect.position[1] - abRect.position[1])
bottom = top + height
right  = left + width
```

Every probe object is removed afterwards. The width check computes `stepX = (right - left) + spacing` and `lastRight = right + stepX*(columns - 1)`, warning when `lastRight > 16383` and reporting how many columns fit. The height check computes `rowsUsed` as the count of `i` in `[0, copies)` where `(i+1) % columns == 0` and `stepY = (bottom - top) + spacing`, warning when `bottom + stepY*rowsUsed > 16383` and reporting `(columns - 1) + columns*rowsUsed` as the number of copies that fit.

Per-copy placement, with `n` the 1-based copy index, `wrap = (n % columns) == 0`, `row = n / columns`, `h = ab[3] - ab[1]`, `w = ab[2] - ab[0]`, and `prev` the previously created artboard rectangle:

| [INDEX] | [CASE]                | [NEW ARTBOARD RECT]                                                             |
| :-----: | :-------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `wrap`                | `[ab[0], ab[1] - spacing*row + h*row, ab[2], ab[3] - spacing*row + h*row]`       |
|  [02]   | First copy, no wrap   | `[ab[2] + spacing, ab[1], ab[2] + spacing + w, ab[3]]`                           |
|  [03]   | Later copy, no wrap   | `[prev[2] + spacing, prev[1], prev[2] + spacing + w, prev[3]]`                   |

Naming resolves `%a` to the source artboard name and `%n` to the copy index zero-padded to `String(copies).length` digits through `("000000000" + n).slice(-len)`. Creation calls `artboards.add(rect)` under insert-as-last and `artboards.insert(rect, sourceIndex + n)` otherwise.

Artwork copying, when enabled: unlock every layer recursively; strip `%isLocked` and `%isHidden` from every `pageItem.note`; unlock and unhide items recursively, tagging each item's `note` so the state restores; switch to `ScreenMode.FULLSCREEN` when copies exceed 10 for speed; call `doc.selectObjectsOnActiveArtboard()` and `duplicate()` each selected item; offset each duplicate by `position += [newRect[2] - ab[2], newRect[1] - ab[1]]`, converting through `doc.convertCoordinate(pos, DOCUMENTCOORDINATESYSTEM, ARTBOARDCOORDINATESYSTEM)` when the application is in document coordinates; restore locked and hidden state from the notes, clear the tags, and restore the screen mode. The temporary `FOR_AB_COORD` layer is removed in `onClose`.

Preferences file `~/Documents/Adobe Scripts/Duplicate_Atboards_Pro_data.json`, serialized with `Object.toSource()`, holds `copies`, `columns`, `spacing`, `name`, `isInsert`, `isArtwork`.

## [07]-[BENTO_GRID]

Bento Grid, titled "Bento Grid v0.2", 1448 decoded lines and 1270 unblinded, 326-string table, Sergey Osokin, December 2024, modified December 2025, Illustrator CS5 and later.

Configuration: `aiVers`, `isMac`, `mgns: [10,15,10,7]`, `param: 0`, `winOpacity: 0.98`, `units` from the document ruler, `lblHeight = aiVers < 17 ? 15 : 30`. The dialog is a tabbed panel of width 220.

| [INDEX] | [TAB]  | [CONTROL]                                                  | [DEFAULT]                     |
| :-----: | :----- | :---------------------------------------------------------- | :---------------------------- |
|  [01]   | GRID   | Grid Size `Width:` and `Height:`                             | Target width and height       |
|  [02]   | GRID   | Columns `Number:`, accepting `n`, `a-b`, or `a,b`            | `4-8`                         |
|  [03]   | GRID   | Columns `Gutter:`                                            | `10 <units>`                  |
|  [04]   | GRID   | `Split Cell Into Two Randomly` checkbox for columns          | Off                           |
|  [05]   | GRID   | `First Cell Height Range:`, enabled by that checkbox         | `50-70 %`                     |
|  [06]   | GRID   | Rows `Number:`                                               | `1-5`                         |
|  [07]   | GRID   | Rows `Gutter:`                                               | `10 <units>`                  |
|  [08]   | GRID   | `Split Cell Into Two Randomly` checkbox for rows             | Off                           |
|  [09]   | GRID   | `First Cell Width Range:`, enabled by that checkbox          | `20-40 %`                     |
|  [10]   | GRID   | `Corner Radius:`                                             | `0 <units>`                   |
|  [11]   | TOTAL  | Grid Size `Width:` and `Height:`, synced with GRID           | As above                      |
|  [12]   | TOTAL  | `Max Number:`, 3 characters, unitless                        | `15`                          |
|  [13]   | TOTAL  | `Gutter:`                                                    | `10 <units>`                  |
|  [14]   | TOTAL  | `Minimum Width And Height:`                                  | `10 <units>`                  |
|  [15]   | TOTAL  | `Add Hero Cell` checkbox                                     | Off                           |
|  [16]   | TOTAL  | `Center It` checkbox, disabled until Hero is on              | Off                           |
|  [17]   | TOTAL  | `Corner Radius:`, synced with GRID                           | `0 <units>`                   |

Buttons are Cancel on Esc, OK on Shift+Enter, Generate on Enter as the default, and `?` for About. Two status lines show `Estimated Num:` and `Last Num:`. With a selection present a note warns that the grid takes its size from the selected object and that the original will be deleted.

GRID algorithm:

```text
colCount = randomInt(colMin, colMax)
rowCount = randomInt(rowMin, rowMax)        per column, independent
colW     = (grid.w - (colCount - 1) * colGutter) / colCount
rowH     = (grid.h - (rowCount - 1) * rowGutter) / rowCount
```

A cell defaults to `SINGLE`. With row splitting enabled, `Math.random() > 0.5`, and a previous cell that was not itself a row split, the cell becomes `SPLIT_ROW`; the same test then applies for `SPLIT_COL`. The split ratio and geometry:

```text
ratio = min/100 + Math.random() * (max - min) / 100
SPLIT_ROW:  left = (colW - colGutter) * ratio,  right  = (colW - colGutter) - left
SPLIT_COL:  top  = (rowH - rowGutter) * ratio,  bottom = (rowH - rowGutter) - top
```

The walk advances `x += colW + colGutter` per column and `y -= rowH + rowGutter` per row, because Illustrator's y axis points up.

TOTAL algorithm, a largest-leaf-first guillotine packer capped at 1000 iterations:

```text
while (leaves.length < maxCount && iter < 1000) {
  rect       = the leaf with the largest area
  canH       = rect.w >= 2*minSize + gutter
  canV       = rect.h >= 2*minSize + gutter
  splitHoriz = rect.w > rect.h            forced when only one axis fits
  if (canH && canV && Math.random() < 0.3) splitHoriz = !splitHoriz
  avail = (splitHoriz ? rect.w : rect.h) - gutter
  size  = minSize + Math.random() * (avail - 2*minSize)
  ratio = size / avail                    gutter reapplied at render time
  split the leaf into two children at size with a gutter gap
}
```

The hero cell:

```text
scale = 0.45 + Math.random() * 0.2       45 to 65 percent
heroW = max(grid.w * scale, minSize)
heroH = max(grid.h * scale, minSize)
```

It centers under `Center It` and otherwise places randomly, snapping to an edge whenever the remaining gap falls below `minSize + gutter`. Up to four surrounding zones LEFT, RIGHT, TOP, BOTTOM are emitted, each only when its remaining strip reaches `minSize`, each carrying random `expand`, `expandLeft`, and `expandRight` flags that decide whether it reaches the container edge or only the hero's edge. The remaining `maxCount - 1` cells distribute across the zones, every zone taking at least one where there are enough, then leftovers landing on `Math.floor(Math.random() * zones.length)`, and each zone receives its own recursive split subtree.

DOM sequence:

1. Grid box: with a selection that is not a `TextRange`, its unioned `geometricBounds`, computed with special handling for `GroupItem`, clipped groups, and `CompoundPathItem`, which are duplicated into a temporary layer, flattened with `executeMenuCommand("noCompoundPath")` and `"group"`, measured, and the temporary layer removed; the selected item hides during preview and is deleted on OK. Without a selection, the active artboard's `artboardRect`.
2. Target layer: `activeLayer` when visible and unlocked, else the first visible and unlocked layer, else the first layer forced visible and unlocked.
3. `container.pathItems.roundedRectangle(top, left, width, height, rX, rY)` per cell with `rX = rY = cornerRadius`.
4. `layer.groupItems.add()`, then each rectangle `.move(group, ElementPlacement.PLACEATEND)` iterated back to front to preserve stacking order, then `app.selection = null`.
5. Live regeneration removes the previous group and forces a redraw by nudging `views[0].centerPoint` by 0.001 and back with `$.sleep(10)`.

Output: group names by mode are `BENTO`, `BENTO_TOTAL`, `BENTO_HERO`, `BENTO_HERO_CENTER`. Individual rectangles are unnamed and carry the default new-path appearance, with no explicit fill or stroke. No layer is created for the artwork; the only layer created is the transient one used for compound-path measurement.

Preferences file `~/Documents/Adobe Scripts/Bento_Grid_data.json` holds `win_x`, `win_y`, `tab`, `gridW`, `gridH`, `columns`, `columnsGap`, `isSplitColumns`, `columnsPercent`, `rows`, `rowsGap`, `isSplitRows`, `rowsPercent`, `rect`, `rectGap`, `rectMin`, `hero`, `isCentered`, `radius`.

## [08]-[TEXT_HIGHLIGHT]

Highlight Text, titled "Highlight Text 0.5", 1852 decoded lines and 1689 unblinded, 501-string table, Sergey Osokin, December 2022, modified December 2025, Illustrator CS6 and later.

Configuration: `cmyk: "0, 10, 100, 0"`, `rgb: "255, 235, 0"`, `icoSize: [24,24]`, `isDarkUI: app.preferences.getRealPreference("uiBrightness") <= 0.5`, `refPt: app.preferences.getIntegerPreference("plugin/Transform/AnchorPoint")`, `last: "Last Used"`, `isMac`, `units`, `isRgb` from the document color space, `sf = doc.scaleFactor || 1`. Window opacity is 0.97 and the position restores from a saved global.

| [INDEX] | [PANEL]              | [CONTROL]                                                           | [DEFAULT]                              |
| :-----: | :------------------- | :------------------------------------------------------------------ | :------------------------------------- |
|  [01]   | Highlight Dimensions | `Width:` slider 0 to 100 plus edittext, accepting the word `fit`     | `100 %`                                |
|  [02]   | Highlight Dimensions | `Height:` slider 0 to 100 plus edittext, accepting absolute units    | `100 %`                                |
|  [03]   | Overall Height       | Radios `x-height`, `Cap Height`, `Custom`                            | x-height                               |
|  [04]   | Overall Height       | Custom letter edittext, 6 characters, "Enter one letter"             | `x`                                    |
|  [05]   | Anchor Point         | 3 by 3 unlabeled radio grid, 30 px pitch, 14 px squares              | The application's transform anchor     |
|  [06]   | RGB/CMYK Color       | Comma-separated channel list, label following the document space     | `255, 235, 0` or `0, 10, 100, 0`       |
|  [07]   | RGB/CMYK Color       | Swatch button opening `app.showColorPicker` or `$.colorPicker`       | —                                      |
|  [08]   | Y Offset             | Edittext, range plus and minus 16383                                 | `0 <units>`                            |
|  [09]   | Dashed Line          | `Enable` checkbox                                                    | Off                                    |
|  [10]   | Dashed Line          | `Dash:` and `Gap:` edittexts, 0 to 1000                              | `0 <units>`                            |
|  [11]   | Zig Zag Effect       | `Enable` checkbox                                                    | Off                                    |
|  [12]   | Zig Zag Effect       | `Size:` edittext plus `Relative` and `Absolute` radios               | Relative, 0 to 100 %, absolute 0 to 7200 |
|  [13]   | Zig Zag Effect       | `Ridges:` edittext 0 to 100, plus `Smooth` and `Corner` radios       | `3`, Smooth                            |
|  [14]   | Preset row           | Dropdown plus add, save, delete, and reveal-presets-file icon buttons | `Last Used`                           |
|  [15]   | Bottom row           | `Preview` checkbox, `?` About, Cancel and OK, order swapped on Mac   | Preview off                            |

Typing a letter while the preset dropdown holds focus jump-searches entries. Arrow keys and `[` and `]` nudge a numeric field, with Shift giving a step of 10.

Measurement runs through outlines rather than font metrics, and is the script's distinguishing technique. In whole-textFrame mode, for each `line` in `textFrame.lines`: clone a one-line frame from `characters[0].duplicate(insertionPoint)` and set `contents = line.contents`, or `"x"` for a blank line; advance `top -= leading` per iteration and align horizontally per `Justification`, matching `CENTER` and `RIGHT` against the frame's `left` and `width` and otherwise left; duplicate that frame, call `.createOutline()`, and read `geometricBounds` and `width` for the box width; duplicate again, override `.contents` to the single measurement glyph, which is the x-height letter, the cap letter, or the custom letter, outline it, and read that outline's `height` and `geometricBounds[1]` as the line's measured height and baseline-relative top; remove both temporary outline objects.

In TextRange mode it finds the intersecting text frames, duplicates the frame, outlines it, groups the resulting path items, then counts whitespace runs matching `\s|\x03|\f|\r\n|\r|\n` before and inside the selection in order to skip the glyph paths that carry no mark. It slices `pageItems` for the selected character range into a temporary group to obtain `left` and `width`, and a second duplicate with `contents` swapped to the measurement glyph supplies `height` and `top`.

The highlight is a stroked two-point open path, not a filled rectangle: the stroke weight carries the highlight's height.

```text
left     = dest.left
top      = dest.top
boxW     = isAbsW ? rW : dest.width
boxH     = isAbsH ? rH : dest.height
lineEndX = boxW * (isAbsW ? 1 : rW) + left
```

The anchor-dependent adjustment, for the CENTER case:

```text
absolute:  left -= 0.5 * (boxW - dest.width)
relative:  left += 0.5 * boxW * (1 - rW)
absolute:  top  += 0.5 * (boxH - dest.height)
relative:  top  -= 0.5 * boxH * (1 - rH)
```

The other eight anchors, `TOP`, `TOPRIGHT`, `LEFT`, `RIGHT`, `BOTTOMLEFT`, `BOTTOM`, `BOTTOMRIGHT`, and the default top-left, apply the analogous half-delta or full-delta shifts. The Y offset applies last:

```text
top   -= (0.5 * boxH * (isAbsH ? 1 : rH) - offset)
right  = left + lineEndX
```

Path construction:

```text
pathItems.add()
setEntirePath([[left, top], [right, top]])
filled      = false
stroked     = true
strokeWidth = boxH * (isAbsH ? 1 : rH)
strokeColor = color
```

Dashes set `strokeDashes = [dash, gap, 0, 0, 0, 0]`. The zigzag applies Illustrator's native live effect through `applyEffect` with the XML `<LiveEffect name="Adobe Zigzag"><Dict data="R amount #1 R relAmount #2 R absoluteness #3 R ridges #4 R roundness #5 "/></LiveEffect>`, substituting size, relative amount, absoluteness, ridge count, and smoothness.

DOM sequence: reads `selection`, `textFrames`, `textRange.{start, end, justification, leading}`, `parent.textFrames`, `lines[i].contents`, `characters[0]`, `insertionPoints[0]`, `pageItems`, `groupItems`, `layer`, `geometricBounds`, and `width`, `height`, `top`, `left`; creates `textFrames.add()`, `.duplicate()`, `.createOutline()`, `layer.groupItems.add()`, and `layer.pathItems.add()`; converts units through `UnitValue(...).as(...)` and color through `app.convertSampleColor(ImageColorSpace.RGB | CMYK, ..., ColorConvertPurpose.defaultpurpose)`.

Output: each highlight is a bare open `PathItem` placed with `move(dest, ElementPlacement.PLACEAFTER)` on the same layer as, and immediately after, its source object. No wrapper layer or group is created and the highlight is not grouped with the text. Color is a solid `strokeColor`, RGB or CMYK per the document, or a duplicated spot color. No blend mode and no opacity are set, so both inherit Normal and 100 percent. On completion `app.selection` holds the new paths; on Cancel the original selection restores.

Preferences file `~/Documents/Adobe Scripts/Highlight_Text_presets.json` is a flat object keyed by preset name, written with a hand-rolled serializer at two-space indent and read with `JSON.parse` falling back to `new Function("return (" + text + ")")()`. A `Default` preset is hardcoded and a `Last Used` entry is rewritten on every OK, then reloaded as the initial selection on the next run.

The wrapper carries `//@target illustrator` and `//@targetengine 'hgltxt'` outside the `eval`, so neither appears in the bytecode and the body does not depend on engine persistence.

## [09]-[GRADIENT_BLENDER]

Gradient Blender, titled "Gradient Blender v.0.3", 1143 decoded lines and 1021 unblinded, 279-string table, Sergey Osokin, January 2022, modified November 2023, Illustrator CS6 and later, with bilingual English and Russian strings.

Configuration: `maxPrecision: 20`, `precision: 1`, `uiWidth: 160`, `uiOpacity: 0.99`, `uiMargins: 16`, `uiSpacing: 10`, `aiVers`, `isMac`.

| [INDEX] | [CONTROL]                                                                     | [DEFAULT]                              |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `Unique gradients: N of M` statictext with left and right navigation buttons   | Enabled above one gradient, disabled below Illustrator 17 |
|  [02]   | `Before` preview panel 15 px and `After` preview panel 50 px, drawn in `onDraw` | —                                    |
|  [03]   | `Precision (max 20)` edittext, 4 characters, arrow spinner 1 and 10, clamped 1 to 20 | `1`                              |
|  [04]   | `Remove intermediate stops` checkbox                                           | Off                                    |
|  [05]   | Color space radios `OKLAB`, `OKLCH`, `LCH`, `HSL`                              | LCH                                    |
|  [06]   | `Hue interpolation method` dropdown `Shorter`, `Longer`, `Decreasing`, `Increasing`, disabled under OKLAB | Index 0, `Shorter`   |
|  [07]   | Appearance attribute `Fill color` checkbox                                     | On                                     |
|  [08]   | Appearance attribute `Stroke color` checkbox                                   | Off                                    |
|  [09]   | Conditional CMYK-document notice and CS6 preview-unavailable notice            | —                                      |
|  [10]   | `?` About, Cancel, Ok, Mac ordering Cancel first                               | —                                      |

Algorithm:

1. Collect gradients by walking `selection` recursively: a `GroupItem` recurses into `pageItems`; a `PathItem` or `CompoundPathItem` contributes when `fillColor` or `strokeColor` has `typename === "GradientColor"` and the gradient exposes `gradientStops`. Results deduplicate by object identity, so a gradient shared by several objects is processed once.
2. Optionally delete every intermediate stop, keeping the two endpoints.
3. Snapshot every remaining stop as `{pos: rampPoint, col: color, opa: opacity}`.
4. Add `(precision - 1) * (stopCount - 1)` stops through `gradientStops.add()`.
5. Pin `gradientStops[last].rampPoint = 100` for the duration of the rewrite.
6. Walk the stops. For an index `i` that is not a multiple of `precision`:

   ```text
   rampPoint = previous.rampPoint + (next.pos - current.pos) / precision
   opacity   = previous.opacity   + (next.opa - current.opa) / precision
   color     = mix(from, to, (i % precision) / precision, mode, hueMethod)
   ```

   For an index that is a multiple of `precision`, the original stop's position, color, and opacity restore verbatim, and `from` and `to` advance to the next original pair.
7. Restore `gradientStops[last].rampPoint` to the original last position and call `redraw()`.

Per-gradient work sits in a try/catch, so one failure does not abort the batch.

Mixing is componentwise linear interpolation in the chosen space, `out[i] = a[i] + (b[i] - a[i]) * t`. Endpoint resolution runs first: a `SpotColor` at tint 100 resolves to `spot.color` and a lower tint mixes toward white by `1 - tint/100`; a `GrayColor` converts through `convertSampleColor(GrayScale, RGB, …)`. When both endpoints are pure-K CMYK, meaning `cyan == magenta == yellow == 0`, the color-space math is skipped and only the `black` channel interpolates, rebuilding a `CMYKColor(0, 0, 0, K)`. In a CMYK document every RGB result converts back with `convertSampleColor(RGB, CMYK, …)`.

Hue arc adjustment takes `from`, `to`, and `diff = to - from` and shifts the endpoints before the interpolation so it travels the intended way around the wheel.

Gradient meshes are unsupported; the script touches `GradientColor.gradientStops` alone. The native stop `midPoint` slider is never touched, and smoothing comes entirely from inserted discrete stops.

DOM sequence: selection walk → per gradient optional `gradientStops[i].remove()` for all but the endpoints → `gradientStops.add()` N times → mutate `rampPoint`, `opacity`, and `color` through `new RGBColor()` or `new CMYKColor()` with `colorType = ColorModel.PROCESS` → `redraw()`. The preview panels draw with `graphics.newPath`, `rectPath`, `ellipsePath`, `newBrush`, `newPen`, `fillPath`, and `strokePath` and never touch the document.

Preferences file `~/Documents/Adobe Scripts/Gradient_Blender_data.json`, written as `obj.toSource()`, holds `precision`, `isFill`, `isStroke`, `rmvStops`, `mode` as the selected radio's `.text`, and `hKey` as the hue dropdown's `.selection.index`. It reads back through a single `readln()` plus `new Function("return " + text)()`.

The manual supplies what the code does not. Graphic applications interpolate gradients linearly per channel in RGB, which produces grey muddy midpoints with no correction for hue, saturation, or brightness; Photoshop CC 2022 added perceptual gradient interpolation and Illustrator has none. On color spaces it states that OKLAB gives a perceptual transition crediting Björn Ottosson, OKLCH works from hue, chroma, and luminance and keeps lightness stable as hue changes, LCH is sometimes more vivid with less predictable lightness, and HSL is sometimes more vivid than LCH. It carries one behavioral warning absent from the code: a gradient on duplicated objects, or one applied with the Eyedropper, is a single global document gradient, so the script changes every copy of it in the document, and the only remedy is to reverse the gradient or nudge any stop in the Gradient panel first to force Illustrator to fork it. Precision and interpolation mode apply to every selected gradient, not only the one in the preview, and not every gradient can reach a smooth transition without also changing the original colors.

## [10]-[MASK_SCRIPTS]

Opacity Mask Clip v0.3, MIT, Sergey Osokin, April 2019, modified March 2024, tested on Illustrator CC 2019 to 2024. It enables the Transparency panel Clip checkbox on every selected object carrying an opacity mask, and its header warns against placing it in an action slot because that freezes Illustrator.

It requires an open document, Illustrator 16 or later, and a selection that is not a `TextRange`, then asks for confirmation that the selection holds opacity masks alone. Configuration holds `actionSet: "OpacityMaskClip" + "v.0.3"`, `actionName: "ActivateClip"`, `actionPath: Folder.myDocuments + "/Adobe Scripts/"`, `lay: "Remove This Layer"`, and `limit: 10`.

Algorithm: clear the selection and add a temporary layer named `Remove This Layer`; build an action source string carrying one event with `internalName (ai_plugin_transparency)` and a single boolean parameter with `key 1668049264` set to 1, hex-encoding the set and action names through `charCodeAt(0).toString(16)`; write that string to `<actionPath>/<set>.aia`, call `app.loadAction(f)`, and remove the file; switch to `ScreenMode.FULLSCREEN` when the selection exceeds 10 items; set `app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS`; walk the selection back to front, and per item add a placeholder `pathItem` before it, move the item into the temporary layer, select it, run `app.doScript(name, set)`, move the result back before the placeholder, and remove the placeholder, recursing into a `GroupItem` with page items; restore the interaction level, remove the temporary layer, call `app.unloadAction(set, '')`, and restore the screen mode.

Trim Masks v0.3, MIT, Sergey Osokin, March 2020, tested on Illustrator CC 2019 to 2025, with the same action-slot warning. It trims every clipping group in a document through Pathfinder Crop. Configuration holds `actionSet` and `actionName` of `Trim-Mask`, the same `actionPath`, `isSaveMask: true`, and `over: 10`. Strings are localized English and Russian through `$.localize = true`.

Algorithm: build and load an action whose one event carries `internalName (ai_plugin_pathfinder)` with an enumerated parameter `key 1851878757`, name `Crop`, value 9; select all when the selection is empty; collect groups and count clipping groups recursively, switching to full screen above 10; then per clipping group set `evenodd = false` on every child, because an even-odd fill rule gives Pathfinder Crop the wrong result; outline live text inside the group, restoring each path's fill from `textRange.fillColor`; normalize compound paths through `noCompoundPath`, ungroup, and `compoundPath`; duplicate the filled clipping path when `isSaveMask` holds; save the group's `opacity` and `blendingMode`, run `app.doScript`, and restore both onto the resulting path. It unloads the action, deselects, and restores the screen mode and interaction level at the end.

## [11]-[SWATCH_SCRIPTS]

Transfer Swatches, Alexander Ladygin, copyright 2018, www.ladyginpro.ru, Illustrator CC and later, no license statement. The dialog holds a panel titled `Please select a document!` with a dropdown of every open document except the active one, a `Replace the same by name` checkbox, and Cancel and OK buttons.

Algorithm: read the source document's swatch groups through `getSwatchesSaveStructure`, where group index 0 holds the ungrouped swatches and every later index is a named group. For an ungrouped swatch other than `[Registration]` and `[None]`, look the name up in the target document: under replace, overwrite `swatchOriginal.color`; otherwise add a new swatch named `name + '_' + swatches.length + i`; a name absent from the target adds the swatch under its own name. For groups, the non-replace branch adds each group whole through `addSwatches('group', …)`, and the replace branch looks the group up by name, overwrites each matching swatch color, adds a missing swatch through `doc.swatches.add()` and `group.addSwatch(s)`, and falls back to adding the whole group when the group name is absent. `addSwatches` creates a global swatch through `doc.spots.add()` when `isGlobal` holds and a plain swatch through `doc.swatches.add()` otherwise, so spot colors carry across.

Sync Global Colors Names v0.1, MIT, Sergey Osokin, April 2021, tested on Illustrator CC 2018 to 2021. The dialog holds a panel titled `Source for sync` with a dropdown of every open document, Cancel and Ok buttons, and a clickable copyright line. Strings are localized English and Russian.

Algorithm: `collectSpotsNames` reads every `doc.spots` entry other than `[Registration]` into `{name, red, green, blue}` taken from `spot.color`; `replaceSpotsNames` walks each other open document and renames a spot whose `color.red`, `color.green`, and `color.blue` all match a collected entry. Each modified document is activated and saved when it has a path, then the original active document restores and an alert reports completion.

## [12]-[COMMAND_PALETTE]

Ai Command Palette 0.11.6, Josh Duncan, 14 008 lines in one IIFE with `//@target illustrator` at line 11. The header states `Copyright 2024 Josh Duncan` and `This script is distributed under the MIT License.` followed by `See the LICENSE file for details.`; the file holds no license text and no `LICENSE` file sits beside it on the Drive. It is the origin of `plan/inputs/menu_commands.csv` and `plan/inputs/tool_commands.csv`.

The palette window (`commandPalette()`, lines 11688 to 11807) is a ScriptUI dialog of width 600 and height 211 on Windows or 201 on macOS, sized so nine list rows show, holding three children: a query edittext, a multi-column listbox with headers whose default columns are Name at width 450 and Type at width 100, and a row with OK and Cancel buttons of width 100 each. `visibleListItems` is 9 and `mostRecentCommandsCount` is 25 (lines 10723 to 10724). `q.onChanging` recomputes `matcher(this.text, commands)` against a per-dialog memo and rebuilds the listbox on every keystroke (lines 11732 to 11742, 11489 to 11493).

Keyboard behavior: Escape in the query field closes the window (11765 to 11768); Up and Down typed in the query field are re-dispatched into the listbox as a synthetic `KeyboardEvent` carrying `fromQuery` and `fromQueryShiftKey` (11762, 11769 to 11783); `scrollListBoxWithArrows` wraps in both directions and hand-maintains a `frameStart` scroll position with `revealItem()` because ScriptUI exposes no visible window of a listbox (11596 to 11684); a double-click accepts the row, or appends it as a step inside the workflow and startup builders (11543 to 11573).

The command table `commandsData` spans lines 1347 to 10676 and holds 653 entries of uniform shape `{id, action, type, docRequired, selRequired, name{en,de,ru,zh-cn}, hidden}`: 529 of type `menu`, 91 of type `tool`, 24 of type `builtin`, 9 of type `config`. Optional `minVersion` and `maxVersion` fields appear 179 times and are checked by `commandVersionCheck()` (334 to 341). The localization table `strings` spans lines 585 to 1346 with 149 entries in the same four locales. Line 583 records that both tables are generated by an external Python script.

| [INDEX] | [CALL]                                        | [LINE]        | [PURPOSE]                                  |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `app.executeMenuCommand(command.action)`       | 12485         | Runs every `menu` command                  |
|  [02]   | `app.selectTool(command.action)`               | 12489         | Runs every `tool` command                  |
|  [03]   | `app.doScript(command.name, command.set)`      | 12493         | Runs a recorded Illustrator action         |
|  [04]   | `app.executeMenuCommand("fitin")`              | 13530         | Zoom to fit inside `goToArtboard`          |
|  [05]   | `app.open(f)`                                  | 12503, 13924  | Bookmark open and recent-document open     |
|  [06]   | `app.redraw()`                                 | 13934         | `redrawWindows` builtin                    |
|  [07]   | `app.activeDocument.imageCapture(f)`           | 13447         | PNG export                                 |
|  [08]   | `app.activeDocument.exportVariables(f)`        | 13468         | Dataset variable export                    |
|  [09]   | `app.preferences.getStringPreference(...)`     | 11156, 11167, 13877 | Reads action sets and recent files   |
|  [10]   | `app.preferences.getIntegerPreference(...)`    | 11161, 13874  | Action count and recent-file count         |
|  [11]   | `$.evalFile(f)`                                | 12560          | Runs a user `.js` or `.jsx`                |

Recorded actions are discovered at run time rather than stored: `userActions.load()` walks `app.preferences` under `"plugin/Action/SavedSets/set-" + i + "/"` for `i` of 1 to 100, reading `name`, `actionCount`, and `action-N/name`, and writes `{action:"action", type:"action", set, name}` records straight into `commandsData` (11147 to 11182). Illustrator must restart before a newly recorded action appears (13171 to 13172).

Dispatch runs through `switch (command.type.toLowerCase())` in `executeAction` (12445 to 12475): `config` and `builtin` to `internalAction`, `menu` to `menuAction`, `tool` to `toolAction`, `action` to `actionAction`, `bookmark`, `file`, and `folder` to `bookmarkAction`, `picker` to `runCustomPicker`, `script` to `scriptAction`. A workflow is handled one level up in `processCommand`, which validates every step then recurses over `command.actions` (12397 to 12413). `executeAction` gates on `docRequired` and `selRequired` with a confirm-to-override prompt (12421 to 12440). Command kinds created at run time or from preferences are `action`, `workflow`, `script`, `bookmark` with subtype `file` or `folder`, `picker`, and the ad-hoc `Option` pseudo-type minted inside `runCustomPicker` (12518).

Matching picks one of two functions once at startup: `var matcher = prefs["fuzzy"] ? fuzzy : scoreMatches;` (13973), with `prefs.fuzzy` defaulting to true (10799). `fuzzy(q, commands)` (11183 to 11240) strips regex-special characters and lowercases both sides, strips a trailing `...` from the command name, splits the query on spaces into chunks, and runs `findMatches(chunks, commandName)` (11270 to 11324), a greedy longest-prefix scanner that grows a substring while it still matches the remainder, commits the last successful span, and restarts past it; a single character that matches nowhere wipes the span list and aborts, so a command survives only when every chunk is fully coverable. `calculateScore(command, spans)` (11242 to 11268) weights each span:

```text
whole word    (starts at 0 or after a space, ends at end or before a space) : (e - s) * 3
starts a word                                                              : (e - s) * 2
otherwise                                                                  : (e - s) * 1
plus 0.5 when the span begins at or after the last " > " separator in the name
plus 1   when the exact query is a known latch
plus 0.5 when the command is in recentCommands
```

Results sort descending by score. The legacy `scoreMatches` (11331 to 11418) scores by word-boundary regular expressions, gives a latched query a flat 1000, adds each matching word's length against the name and the `" > "`-flattened `strippedName`, adds recent-command run counts, and sorts with an in-place bubble sort.

Latching learns shortcuts from history: `userHistory.load()` (11077 to 11125) replays `History.json` and maps each distinct query string to its most frequently chosen command in `latches`, alongside `recentCommands` run counts and the last 25 distinct entries in `mostRecentCommands`. Every accepted selection with a non-empty query appends `{query, command, timestamp}` and saves immediately (11745 to 11758), truncated to the last 500 entries (11132).

| [INDEX] | [PURPOSE]                    | [EXPRESSION]                                                      | [LINE]        |
| :-----: | :--------------------------- | :---------------------------------------------------------------- | :------------ |
|  [01]   | Legacy settings folder        | `setupFolderObject(Folder.userData + "/" + settingsFolderName)` with `settingsFolderName = "JBD"` | 10782-10783 |
|  [02]   | Legacy settings file          | `AiCommandPaletteSettings.json`                                    | 10784         |
|  [03]   | Preferences folder            | `setupFolderObject(Folder.userData + "/JBD/AiCommandPalette")`      | 10788         |
|  [04]   | Preferences file              | `Preferences.json`                                                 | 10789         |
|  [05]   | History file                  | `History.json` in the same folder                                  | 11059-11060   |
|  [06]   | Development dumps             | `prefs.json`, `commands.json`, `log_<timestamp>.txt`               | 10740-10753   |
|  [07]   | Windows keypress shim         | `setupFileObject(settingsFolder, "SimulateKeypress.vbs")`          | 470           |
|  [08]   | URL-open shim                 | `new File(Folder.temp.absoluteURI + "/aisLink.html")`              | 493           |

`setupFolderObject` creates a missing folder (510 to 514), so launching the script creates both `JBD` and `JBD/AiCommandPalette` under `Folder.userData` before any user action. Persistence is `obj.toSource()` on write (572) and bare `eval(json)` on read (562), so neither file is JSON. The preferences model holds `startupCommands`, `hiddenCommands`, `workflows`, `bookmarks`, `scripts`, `pickers`, `fuzzy`, `latches`, `version`, `os`, `locale`, `aiVersion`, `timestamp`, of which the last five are deliberately not restored from disk (10792 to 10805, 10853).

## [13]-[ACTION_SETS]

An `.aia` file is a flat tab-indented `/key value` tree opening with `/version 3`, the set's hex-encoded `/name`, and `/actionCount N`, under which `N` sibling `/action-N { … }` blocks each carry a hex `/name`, `/keyIndex`, `/colorIndex`, and `/eventCount M`, and each of those holds `M` ordered `/event-N { … }` blocks naming the Illustrator command by an unquoted literal `/internalName (ai_plugin_…)` beside a hex `/localizedName` and the `/isOn` and `/hasDialog` playback flags, each event closing over `/parameterCount` and its `/parameter-N { /key <FourCC as int> /type (ustring|boolean|real|unit real|enumerated) /value … }` entries whose string values are length-prefixed `[ N <hex bytes> ]` blocks decoding as UTF-8. No file carries `/eventSelectorName`; version 3 identifies an event by `/internalName`.

| [INDEX] | [FILE UNDER `<actions>`]                                              | [SET NAME]              | [ACTIONS] |
| :-----: | :-------------------------------------------------------------------- | :---------------------- | :-------: |
|  [01]   | `Site Analysis Actions.aia`                                            | Site Analysis Actions   |     5     |
|  [02]   | `Human Scaling Actions.aia`                                            | Human Scaling Actions   |    14     |
|  [03]   | `New/Human_Scales_in_a_Second/Human_Scales_in_a_Second.aia`             | Human Scales in a Second |   14     |
|  [04]   | `Human Shadow Actions .aia`                                            | `Human Shadow Actions ` |     4     |
|  [05]   | `New/Illustrator Actions - Personal Shadow/SA_Personal Shadow.aia`      | `Shadows `              |     4     |
|  [06]   | `New/SA_Plan_Shadows/SA_Plan_Shadows.aia`                              | `Plan Shadows `         |     1     |
|  [07]   | `Outline Actions - B&W.aia`                                            | Outline Actions - B&W   |     2     |
|  [08]   | `New/SA_Black and White Actions (1)/Actions/SA_Black and White.aia`     | Black and White         |     1     |
|  [09]   | `LPE 3.aia`                                                            | LPE 3                   |     9     |
|  [10]   | `Utillity Actions.aia`                                                 | Utillity Actions        |    11     |
|  [11]   | `New/ILLUSTRATOR ACTIONS VOL.1/HBD Actions Vol.1.aia`                   | HBD Actions Vol.1       |    11     |

Action names by set: the two human-scale sets carry the same 14, `1:250 (1″=20′-0″)` through `1:10 (1 1/2″=1′-0″)` in Standing then Sitting order. The two shadow sets carry the same four, `Personal Shadow Top Right`, `Personal Shadow Top Left ` with a trailing space, `Personal Shadow Bottom Right`, `Personal Shadow Bottom Left`. `Plan Shadows ` carries `Long Shadow`. `Outline Actions - B&W` carries `Single Selection` and `Entire Document`, 11 events each. `Black and White` carries one action of the same name with 11 events. `LPE 3` carries `Exit Isolation Mode`, `Remove Unused Swatches`, `Add Selected Colors`, `Saturate +`, `Saturate -`, `Add Colors`, `rgb`, `cmyk`, `Add Used Colors`. `Utillity Actions` carries `Opacity 75`, `Opacity 50`, `Opacity 25`, `Reflect Vertical`, `Reflect Horizontal`, `Unite (Pathfinder)`, `Intersect (Pathfinder)`, `Exclude (Pathfinder)`, `Minus Front (Pathfinder)`, `Expand Shape`, `Expand + Unite Shape`. `HBD Actions Vol.1` carries `Align + Distribute Vertical`, `Align + Distribute Horizontal`, `Blend`, `Expand (Appearance Only)`, `Expand Shape`, `Expand + Unite Shape`, `Offset Path`, `Centre Objects`, `Roughen`, `Zig Zag`, `Drop Shadow`.

Site Analysis Actions is the set the utility styles come from. `Water Line Action`, `Power Line Action`, `Gas Line Action`, and `Sewer Line Action` are one template of ten events parameterized by a pair of named graphic styles, and `Noise Line Action` is a three-event variant.

| [INDEX] | [EVENT]                                  | [PARAMETER]                                          |
| :-----: | :--------------------------------------- | :--------------------------------------------------- |
|  [01]   | `ai_plugin_styles`, Graphic Styles        | ustring `<Utility> - Dotted Line`                    |
|  [02]   | `ai_plugin_transparency`, Transparency    | enum `Exclusion`, value 11                           |
|  [03]   | `ai_plugin_offset`, Offset Path           | 15.0 units, miter limit 4.0, join `Miter`            |
|  [04]   | `ai_plugin_styles`, Graphic Styles        | ustring `<Utility> - Fill Style`                     |
|  [05]   | `ai_plugin_transparency`, Transparency    | enum `Exclusion`                                     |
|  [06]   | `ai_plugin_find`, `Select: `              | enum `Same Blending Mode`                            |
|  [07]   | `ai_plugin_transparency`, Transparency    | enum `Normal`                                        |
|  [08]   | `adobe_group`, Group                      | —                                                    |
|  [09]   | `adobe_deselectAll`, Deselect All         | —                                                    |
|  [10]   | `ai_plugin_setColor`, Set color           | enum `Default fill and stroke`                       |

`<Utility>` is `Water`, `Power`, `Gas`, or `Sewer`. `Noise Line Action` runs `ai_plugin_styles` with ustring `Noise - Line Style`, then `adobe_deselectAll`, then `ai_plugin_setColor` with `Default fill and stroke`. Every event carries `/isOn 1`; only Offset Path carries `/hasDialog 1`, paired with `/showDialog 0`, so its dialog stays suppressed at playback. All five actions reference their graphic styles by name string rather than embedding them, so the styles must already exist in the target document.

## [14]-[DEFECTS]

| [INDEX] | [PRODUCT]                | [DEFECT]                                                                                                          |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | Horizontal Lockup Grid   | The `left - height/4` vertical is emitted twice                                                                     |
|  [02]   | Vertical Lockup Guide    | Decoded line 56 reads `doc.defaultFillColor - defColor;`, a subtraction whose result is discarded where an assignment was intended |
|  [03]   | Instant Isometric Grids  | The unused `processArtboard` variant calls `drawClip` with one argument missing                                     |
|  [04]   | Add Object Guides        | The `Draw As` radio label is misspelled `Guidlines`                                                                 |
|  [05]   | Duplicate Artboards Pro  | The script name is misspelled `Atboards` in the title bar, the About text, and the preferences filename             |
|  [06]   | Duplicate Artboards Pro  | Lock and hide state is stored in `pageItem.note`, a user-visible field, and an interrupted run leaves `%isLocked` or `%isHidden` tags behind |
|  [07]   | Bento Grid               | `size = minSize + Math.random() * (avail - 2*minSize)` falls below `minSize` when `avail < 2*minSize`; the `canH` and `canV` guards test `2*minSize + gutter` against the pre-gutter dimension while `avail` is already gutter-reduced |
|  [08]   | Highlight Text           | No right-to-left or vertical-text handling exists; the geometry assumes left-to-right horizontal lines throughout   |
|  [09]   | Highlight Text           | Highlights are neither grouped nor layered, so a document with many of them is hard to manage                       |
|  [10]   | Gradient Blender         | The color-space labels are wired to the wrong mathematics                                                           |
|  [11]   | Gradient Blender         | LCH is the effective default through ScriptUI radio exclusivity                                                     |
|  [12]   | Gradient Blender         | Selecting HSL produces no blend at all                                                                              |
|  [13]   | Gradient Blender         | The hue-arc dropdown is shifted by one and the correct longer-arc branch is unreachable                             |
|  [14]   | Transfer Swatches        | The script adds `extend`, `getSwatches`, `getSwatchGroups`, `addSwatches`, `transferSwatches`, and four more members to `Object.prototype` as enumerable properties, so every `for...in` in the same session sees them |
|  [15]   | Transfer Swatches        | `getSwatchesSaveStructure(onlySelected)` is only ever called with no argument, so the selected-swatches path is dead |
|  [16]   | Transfer Swatches        | The duplicate-name suffix `name + '_' + swatches.length + i` concatenates two numbers as text instead of counting    |
|  [17]   | Sync Global Colors Names | Matching reads `spot.color.red`, `.green`, and `.blue` alone, so a CMYK document compares undefined channel values   |
|  [18]   | Sync Global Colors Names | Every other open document with a path is saved without asking                                                       |
|  [19]   | Ai Command Palette       | `oldCommandIdsLUT` is read at lines 11023 and 11025 and defined nowhere, so migrating a legacy workflow with a stale step id throws |
|  [20]   | Ai Command Palette       | `imageCapture()` inverts its guard at line 13440: it aborts when a document is open and throws when none is         |
|  [21]   | Ai Command Palette       | `scoreMatches` reads `word` at line 11365 and `strippedName` at 11362, both assigned later at 11376 and 11369       |
|  [22]   | Ai Command Palette       | The `fuzzy` latch bonus at lines 11219 to 11222 never compares `latches[q]` to the current id, so every candidate takes the same +1 |
|  [23]   | Ai Command Palette       | The `default` branch of `executeAction` alerts and falls through to `func(command)` with `func` undefined (12473 to 12478) |
|  [24]   | Ai Command Palette       | The `default` branch of `internalAction` reads an undefined `action` instead of `command.action` (12699)            |
|  [25]   | Ai Command Palette       | `buildWorkflow` pushes to `prefs.workflows` alone (13407), so the new workflow is unrunnable until the next launch  |
|  [26]   | Ai Command Palette       | `buildPicker` declares `picker` inside the `else` branch (12786) and returns it from outside (12803)                |
|  [27]   | Ai Command Palette       | `buildWorkflow(id, badActions)` at line 12404 passes an argument `buildWorkflow(editWorkflowId)` at 13357 does not take |
|  [28]   | Ai Command Palette       | `userActions.load` computes a sanitized id at line 11168 and overwrites it at 11169 with `set + "_" + name`, which collides across sets sharing a name |
|  [29]   | Ai Command Palette       | `Deleted commands will longer work in any workflows` is missing a word at lines 701 and 705                         |
|  [30]   | Ai Command Palette       | `Startup commands will displayed in order from top to bottom.` is missing a word in all four locales (1189 to 1192) |
|  [31]   | Ai Command Palette       | The key `ruler_units_title_case` leaked into the German and Russian translations at lines 1092 to 1093              |
|  [32]   | Ai Command Palette       | `readJSONData` runs `eval()` on the file contents at line 562, so `Preferences.json` and `History.json` are executable |
|  [33]   | Ai Command Palette       | A corrupt file renames to a single `.bak` slot at lines 10860 and 11120, so a second corruption destroys the first backup |
|  [34]   | Ai Command Palette       | `update()` destroys and rebuilds the whole listbox on every keystroke against 653 commands (11489 to 11493)         |
|  [35]   | Ai Command Palette       | Named arguments assigned in the call, as `filterCommands((commands = null), (types = null), …)`, make every parameter an implicit global mutated per call |
|  [36]   | Action sets              | `Human Scaling Actions.aia` and `Human_Scales_in_a_Second.aia` hold identical actions under different set names, as do the two shadow sets |
|  [37]   | Action sets              | Trailing spaces sit inside the set names `Human Shadow Actions `, `Shadows `, `Plan Shadows `, the action name `Personal Shadow Top Left `, and the event name `Select: `, and `app.doScript(name, set)` matches on the exact string |
|  [38]   | Action sets              | `Utillity Actions` misspells the word in both the filename and the recorded set name                                |

Gradient Blender's four defects materially change what its options do, so the rebuild follows the manual's stated semantics rather than this build's label-to-branch wiring.

Defect 10, the color-space wiring. Each `case` string calls a converter that does not match its label:

| [INDEX] | [RADIO LABEL] | [CASE MATCHED] | [WHAT IT COMPUTES]                                             |
| :-----: | :------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | OKLAB         | `"OKLAB"`      | OKLab, correct                                                  |
|  [02]   | OKLCH         | `"OKLCH"`      | Classic HSL, min and max, `L = (max+min)/2`, hue sextants       |
|  [03]   | LCH           | `"LCH"`        | OKLCH, sRGB to OKLab, then `C = √(a²+b²)`, `H = atan2(b, a)`    |
|  [04]   | HSL           | `"HSL"`        | Nothing, an empty case body                                     |
|  [05]   | Unreachable   | `"HCL"`        | True CIE LCH, `convertSampleColor(RGB→LAB)` then polar          |

The `"OKLCH"` branch reads hue from index `[0]` while `"LCH"` and `"HCL"` read index `[2]`; index 0 is where H sits in `[H,S,L]` and index 2 where it sits in `[L,C,H]`. The LCH button therefore yields OKLCH, the OKLCH button yields HSL, and genuine CIE LCH is unreachable. The mismatch is consistent across the whole file including the preferences loader, so it is a defect of the build rather than a decoder artifact, and it reads as a rename that never reached the `case` strings.

Defect 11. Both the OKLCH and the LCH radiobutton are constructed with `value = true`; setting the second silently clears the first because sibling radiobuttons under one parent auto-exclude, and no code resets it. Combined with defect 10, the out-of-box behavior is OKLCH interpolation, which matches what the author intended even though the selected button reads LCH.

Defect 12. The `"HSL"` case has an empty body, leaving the color array empty. In a CMYK document that reaches `convertSampleColor` with an empty sample, throws, and is swallowed by the caller's try/catch, so the stop keeps whatever color `gradientStops.add()` gave it. In an RGB document it reaches `new RGBColor()` with `Math.round(undefined)` and produces NaN channels.

Defect 13. The hue index is always the dropdown's `selection.index`, so it is always one of 0 to 3, which makes the switch's `default` branch dead code, and `default` holds the only correct longer-arc implementation. Measured against the CSS definitions the manual's page 5 cites:

| [INDEX] | [LABEL]    | [BRANCH TAKEN]                                                      | [BEHAVIOR IMPLEMENTED]     |
| :-----: | :--------- | :------------------------------------------------------------------ | :------------------------- |
|  [01]   | Shorter    | `case 0`, `diff > 180 → from += 360`, `diff < -180 → to += 360`       | Shorter, correct           |
|  [02]   | Longer     | `case 1`, `diff > 0 → from += 360`                                    | Decreasing                 |
|  [03]   | Decreasing | `case 2`, `diff < 0 → to += 360`                                      | Increasing                 |
|  [04]   | Increasing | `case 3`, empty                                                       | No-op, raw hue values       |
|  [05]   | Unreachable | `default`, `abs(diff) < 180 → diff > 0 ? to += 360 : from += 360`      | Longer, never reached       |

Only `Shorter` does what its label says, and four of the nine option combinations are misrouted.

## [15]-[NOT_RECOVERED]

| [INDEX] | [MISSING]                                                                    | [CONSEQUENCE]                                              |
| :-----: | :--------------------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Original identifier names in the six JsxBlind-obfuscated scripts              | Variables read as `symbol_N`; property names, string literals, and dialog text were recovered, so semantics are complete and naming alone is lost |
|  [02]   | Original comments and source formatting                                       | JSXBIN never stores them                                    |
|  [03]   | The `<` operator inside `--unblind` loop conditions                           | Read those conditions from the plain decoded output         |
|  [04]   | Unit conversion factors, `doc.scaleFactor`, and preference-derived guide colors | They depend on the live document and the user's preferences |
|  [05]   | Certainty that an empty case body was empty in the source                     | `case "HSL"` and `case 3` in Gradient Blender both print empty; the downstream NaN path and the consistent off-by-one support reading them as genuinely empty |
|  [06]   | Empirical confirmation in a running Illustrator                               | No script was executed, so the geometry and the four Gradient Blender defects rest on reading alone |
