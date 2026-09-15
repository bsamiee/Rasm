# [PHOTOSHOP_BRUSHES_AND_SKILLS]

Research compiled 2026-09-11. Every claim below carries a live URL. Shipping Photoshop at time of writing is **27.10 (August 2026)**; the beta stream is 27.11-era. Adobe's release notes page was last updated 2026-08-28: <https://helpx.adobe.com/photoshop/desktop/whats-new/photoshop-on-desktop-release-notes.html>

Where a claim could not be confirmed against a primary source, it is marked **[UNVERIFIED]** rather than asserted.

---

## [PART_A] — PHOTOSHOP 27.x BRUSH AND PRESET MANAGEMENT

## [01] — WHERE PRESETS LIVE, AND WHAT LARGE SETS ACTUALLY COST

### [1.1] Storage locations (authoritative)

Adobe's preference-file reference, last updated 2024-03-01: <https://helpx.adobe.com/photoshop/kb/preference-file-names-locations-photoshop.html>

| Concern | File | macOS path |
| :-- | :-- | :-- |
| Every brush in the Brushes panel | `Brushes.psp` | `~/Library/Preferences/Adobe Photoshop [version] Settings/` |
| Recently used brushes | `MRUBrushes.psp` | same folder |
| Tool presets | `ToolPresets.psp` | same folder |
| Toolbar customization | `Toolbar Customization.psp`, `Toolbar Customization Primary.psp` | same folder |
| Workspaces | `[Workspace Name].psw` | `…Settings/WorkSpaces/` |
| Workspace modifications | (auto) | `…Settings/WorkSpaces (Modified)/` |
| Which workspaces are loaded | `Workspace Prefs.psp` | same folder |
| Keyboard shortcuts | `Keyboard Shortcuts.psp`, `…Primary.psp` | same folder |
| Menu customization | `Menu Customization.psp`, `…Primary.psp` | same folder |
| User-saved `.abr` files | `[name].abr` | `~/Library/Application Support/Adobe/Adobe Photoshop [version]/Presets/Brushes/` |
| Shipped presets | — | `/Applications/Adobe Photoshop [version]/Presets/Brushes/` |

Windows equivalent root: `%AppData%\Roaming\Adobe\Adobe Photoshop [version]\Adobe Photoshop [version] Settings\`.

The critical structural fact: **`Brushes.psp` is the live library.** Importing an `.abr` copies its contents into `Brushes.psp`; the original `.abr` becomes irrelevant and can be moved anywhere (<https://community.adobe.com/questions-712/can-i-move-brush-files-abr-after-they-have-been-installed-1157466>, 2023-09-04). Adobe itself warns on the current doc page (updated 2026-02-23): *"Preset brushes are stored in the Preferences file. If this file is deleted, reset, or damaged, your brushes will be lost. Saving them in a custom library ensures they are permanent."* — <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/create-a-new-preset-brush.html>

Preset Syncing to `assets.adobe.com` was **removed in Photoshop 25.0 (Sept 2023)**: <https://community.adobe.com/questions-700/preset-syncing-will-be-removed-from-photoshop-25-0-sept-2023-release-671405>. There is no cloud backstop. `Brushes.psp` is a single point of failure on local disk.

### [1.2] Real performance cost — measured from user reports

This is the part Adobe does not document. There is no Adobe page stating any brush limit. The evidence is entirely from the Adobe Community forums, and it is consistent across eight years and every Photoshop generation:

| Observed | Symptom | Source |
| :-- | :-- | :-- |
| **~2 GB `Brushes.psp`** | Hard ceiling. Above it, **only default brushes load, silently, with no error** | <https://community.adobe.com/questions-712/only-default-brushes-will-load-on-startup-cc-2019-osx-high-sierra-1072688> (2018-11-03); reproduced 2023-01-13 at 2.02 GB: <https://community.adobe.com/questions-712/my-brushes-psp-file-is-over-2-gb-so-they-are-not-loaded-into-the-photoshop-is-there-a-workaround-1148835> |
| **~4 GB `Brushes.psp`** | Photoshop takes *several minutes* to quit; spinner on exit | <https://community.adobe.com/questions-712/importing-third-party-brushes-drastically-slows-ps-on-exit-1150944> (2023-03-06) |
| **~500 MB `Brushes.psp`** | Photoshop 2023 **crashed at every launch**; user had to cut to ~200 MB | <https://community.adobe.com/questions-712/brushes-psp-file-size-makes-photoshop-2023-to-crash-at-startup-1152204> (2023-04-01) |
| **~1.5 GB single `.abr`** | `"The file size exceeds the limit allowed and cannot be saved"` on import | <https://community.adobe.com/questions-712/brush-abr-exceeds-limit-1151047> (2023-03-08) |
| **~1.39 GB across 57 groups (~22,800 brushes)** | `"Could not load all the brushes because the maximum number of brushes have been loaded"` — last 4-5 groups silently dropped | <https://community.adobe.com/questions-712/could-not-load-all-the-brushes-because-the-maximum-number-of-brushes-have-been-loaded-1165732> (2023-12-24) |
| **26.11 (Sept 2025)** | Freeze at launch on the *"Reading Brushes"* splash step. Deleting `Brushes.psp` fixed it until brushes were reinstalled. Secondary workaround: disable **GPU Compositing** | <https://community.adobe.com/bug-reports-711/p-photoshop-26-11-freeze-at-launch-on-reading-brushes-building-color-conversion-tables-658055/index9.html> (thread active through 2025-09-24) |
| **150-200 brushes on CC 2018** | Brushes vanished on restart, non-deterministically | <https://community.adobe.com/questions-712/photoshop-cc-2018-brushes-goen-after-every-restart-1063530> (2018-02-16) |

**Mechanism.** `Brushes.psp` is read whole at launch (hence the "Reading Brushes" splash step) and written whole at quit. Both are O(total bytes), not O(brushes used). The exit lag scales directly with `Brushes.psp` size and is independent of what you painted with. A third-party performance writeup states the same thing plainly: *"Photoshop loads every installed brush, font, pattern, and gradient into memory at startup"* — <https://blog.cgfrog.com/best-ways-make-photoshop-run-faster-even-1000-layers/> (2026-05-30).

### [1.3] Practical sizes

| Band | Verdict |
| :-- | :-- |
| **< 100 MB** | Safe. Launch and quit cost is imperceptible. |
| **100–250 MB** | Fine. The 2023 crash report settled at ~200 MB as stable. |
| **250–500 MB** | Noticeable launch/quit lag; one reported crash-at-launch at 500 MB. |
| **500 MB–2 GB** | Multi-minute quit times reported. Works, but hostile. |
| **> 2 GB** | **Silent total failure.** Only defaults load, no warning. |

**Target ≤ 150 MB resident.** For an entourage library that means roughly 300–800 curated brushes with tips in the 500–2500 px range, not the 5,000–20,000 brushes a "megapack" hoard produces.

### [1.4] Is importing a 3–4 GB ABR viable? **No.**

Three independent blockers, any one of which is fatal:

1. A single `.abr` at 1.5 GB already fails import with an explicit file-size error (2023-03-08 thread above). A 3–4 GB file will not import at all.
2. Even if it imported, the resulting `Brushes.psp` would exceed the ~2 GB ceiling and Photoshop would load *nothing but defaults* on the next launch — silently.
3. A brush-count cap exists independently of bytes ("maximum number of brushes have been loaded"), reported at ~22,800 brushes.

**The correct pattern** is one `.abr` per themed group, each well under 100 MB, imported and removed per project. Adobe's own community moderator recommends exactly this shape: *"re-organize & re-group brush presets in Photoshop's Brush presets panel and then export these group folders out of Photoshop from the brushes panel and keep these as a backup"* — 2023-09-04 thread above.

---

## [02] — EVERY LEGITIMATE WAY TO ORGANIZE BRUSHES

### [2.1] Brush groups in the Brushes panel — the only real organizer

Current Adobe doc, **last updated 2026-02-23**: <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/create-preset-brush-groups.html>

Procedure: `Window > Brushes` → **Create a new group** icon (folder, lower-right) → name → drag brushes in.

**Nesting works and is confirmed by Adobe's own bug surface.** A 2018 thread describes importing a group named ANIMALS and finding *"it has created a subgroup with the same name"* containing three sub-folders — i.e. groups nest at least two deep and the nesting round-trips through `.abr`: <https://community.adobe.com/questions-712/brushes-load-automatically-intermediate-folder-added-to-new-groups-1061321> (2018-01-09). That same thread documents the behaviour you can exploit: **importing an `.abr` automatically creates a group named after the set.** That is your programmatic route to group structure (see [05]).

Renaming: panel menu → **Rename Brush**, or double-click the tip. <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/rename-preset-brushes.html> (2026-02-23)

### [2.2] Brush presets vs Tool presets — Adobe's own distinction

From <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/get-started-with-brush-presets.html> (2026-02-23), quoted verbatim:

> **Brush presets**: These store only the brush tip characteristics (size, shape, hardness, and more). Tool presets can be converted to brush presets using the **Options** bar > **Tool Preset Picker** > settings icon > **Convert All to Brush Presets**, allowing you to integrate them into your brush library.
>
> **Tool presets**: These save the brush tip characteristics **and additional Brush tool settings from the Options bar**, such as opacity, flow, and blending modes. You can save tool presets for the Brush tool by choosing **Options** bar > settings icon > **Tool Preset Picker** > **New Tool Preset**.

And the trap, also verbatim from that page:

> The change is **temporary** if you modify a preset brush, such as changing its size or hardness. The next time you choose that preset, it returns to its original settings.

**Decision for an entourage library: use Brush presets, not Tool presets.** Tool presets live in a flat, ungroupable `ToolPresets.psp` list (the Tool Presets panel offers only *Show All / Sort by Tool / Show Current Tool Presets / Text Only / Small List / Large List* — <https://helpx.adobe.com/photoshop/desktop/get-started/set-up-toolbars-panels/create-tool-preset.html>, 2026-02-23). No folders, no export-selected, no hierarchy. Brush presets are the only preset class Photoshop gives a real tree to.

### [2.3] Export Selected Brushes

Confirmed on two current Adobe pages (both 2026-02-23):

> To share your custom brushes, **right-click the brush or group in the Brushes panel, select Export Selected Brushes**, and save the set as an .abr file.

— <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/import-brushes-brush-packs.html> and `.../create-brush-set-painting-options.html`

Selecting a **group** and exporting preserves its hierarchy — this is what makes ABR a viable interchange format for a curated library. Import is `Brushes` panel menu → **Import Brushes**, or double-click the `.abr` with Photoshop running.

### [2.4] Preset Manager — dead for brushes

`Edit > Presets > Preset Manager` still exists, but Adobe's own description scopes it to two types: *"The Preset Manager lets you save or load your presets for **contours and tools**"* — <https://helpx.adobe.com/photoshop/using/presets.html>. Brushes were removed from Preset Manager when the redesigned Brushes panel shipped in CC 2018. Do not build a workflow on it.

`Edit > Presets > Migrate Presets` still works (version-to-version on the same machine). **`Export/Import Presets` was removed** and Adobe's docs still reference it, which users have been complaining about since 2024 and which remained unresolved as of 2026-04-11:
- <https://community.adobe.com/questions-712/exporting-and-importing-presets-where-has-it-gone-1172751> (2024-09-13)
- <https://community.adobe.com/questions-712/has-there-been-any-resolution-to-issues-regarding-the-removal-of-import-export-presets-1557275> (2026-04-11)
- Adobe's own (stale) page: <https://helpx.adobe.com/photoshop/using/preset-migration.html>

**Machine-to-machine migration is a manual file copy of `Brushes.psp`.** That is the current state of the art.

### [2.5] Creative Cloud Libraries — do NOT rely on it for brushes

This is genuinely murky and the sources conflict. Report honestly:

| Source | Claim | Date |
| :-- | :-- | :-- |
| Adobe helpx, *CC Libraries in Photoshop* | *"You can also drag brush presets from Brush Presets panel … into the Libraries panel"* — <https://helpx.adobe.com/photoshop/using/cc-libraries-in-photoshop.html> | **last updated 2023-05-24** (stale) |
| Adobe CC Libraries developer docs | element type `application/vnd.adobe.element.brush+dcx` exists — <https://developer.adobe.com/creative-cloud-libraries/docs/integrate/guides/working-with-elements/supported-elements/> | live |
| Adobe *Libraries* PDF brief | Lists **"Pixel brushes"** and "Vector brushes" as supported, but only in specific apps — <https://www.adobe.com/content/dam/cc/us/en/pdfs/Libraries_brief.pdf> | undated |
| Adobe Community answer | *"CC Libraries can't do regular .abr brushes at the moment. … you **can** have brushes in a CC Library … if you create them with **Adobe Capture** on a mobile device. What you can't do with those is download them"* — <https://community.adobe.com/questions-712/how-do-i-add-a-brush-from-photoshop-to-an-adobe-library-1174791> | 2024-11-25 |

**Reading:** the "pixel brush" library element is a **Capture/Fresco-authored** asset type, not an arbitrary ABR preset. The Photoshop help page describing drag-and-drop from the Brushes panel has not been revised since 2023 and the most recent community evidence contradicts it for third-party ABR brushes.

**Verdict: do not architect a curated entourage library on CC Libraries.** No export path, no group hierarchy, no offline guarantee, contradictory documentation. Use `.abr` files in a versioned folder on disk. Test drag-and-drop yourself if you care — it is a 30-second check — but do not plan around it.

### [2.6] The ABR "group" record

The group hierarchy is a real block in the ABR container, keyed **`phry`** (0x70687279), alongside `samp` (sampled tips), `desc` (Action Descriptor with dynamics), and `patt` (patterns). The best machine-readable grammar in existence — a Kaitai Struct definition — enumerates all four keys and then reads the hierarchy block as opaque bytes:

```yaml
hierarchies_section_body:
  seq:
    - id: unknown_data
      size-eos: true
```

<https://github.com/jlai/brush-viewer/blob/main/shared/abr/ABR.ksy> (repo last pushed 2026-04-28). **Nobody outside Adobe has decoded `phry`.** See [05].

### [2.7] Scripting access — what actually exists in 2026

**There is no `app.brushes` in the UXP Photoshop DOM.** The complete class list on Adobe's own UXP reference is: `Action, ActionSet, Channel(s), ColorSampler(s), CountItem(s), Document(s), Guide(s), HistoryState(s), Layer, LayerComp(s), Layers, Paths, PathItem(s), PathPoint(s), SubPath*, Photoshop, Preferences*, Selection, Text, CharacterStyle, ParagraphStyle, TextFont(s), TextItem, WarpStyle`. No `Brush`, no `Brushes`, no `Preset`, no `ToolPreset`. Verified live at <https://adobedocs.github.io/uxp-photoshop/ps_reference/classes/preferences/preferencesnotifications/> (the class sidebar is the authoritative inventory).

Everything brush-related goes through `batchPlay`. What is proven to work:

| Operation | Descriptor | Evidence |
| :-- | :-- | :-- |
| **Enumerate all brushes** | `{_obj:"get", _target:[{_property:"brushes"},{_ref:"application",_enum:"ordinal",_value:"targetEnum"}]}` → `result[0].brush` | <https://forums.creativeclouddeveloper.com/t/how-to-get-all-brush-or-tool-presets/7168> (2023-11-27, answer by Jarda) |
| **Enumerate all presets** | same shape with `_property:"presetManager"` → `result[0].presetManager[0]` | same thread |
| **Select a brush by name** | `{_obj:"select", _target:[{_ref:"brush", _name:"Brush Name"}]}` | same thread |
| **Select a brush by index** (disambiguates duplicate names) | `r.putIndex(stringIDToTypeID("brush"), idx)` then `executeAction(stringIDToTypeID("select"), d, DialogModes.NO)` | <https://community.adobe.com/questions-712/script-to-get-brush-group-when-getting-brush-name-1151554> (2023-03-18) |
| **Define a brush from selection** | `Mk ` + class `Brsh` + `Usng` → `Prpr/fsel` of `Dcmn/Trgt`, plus `Nm ` string | <https://community.adobe.com/questions-712/script-to-make-brush-preset-from-png-file-1061377> (Chuck Uebele, 2017-12-15) |

What is **broken or unavailable**, as of Photoshop 2025 / UXP 6 (reported 2025-09-28, unanswered):

- Display names are unreliable. `currentToolOptions` returns internal names like `"brush 4"`. `presetManager` mixes brushes with tool presets, crop settings and text presets, and carries duplicates across loaded sets. The `brush` property has real names but *"only seems to contain custom ABR brushes, not default Photoshop brushes."*
- **Brush preview images cannot be accessed at all.**
- **A brush's group cannot be read.** The 2023-03-18 thread closed without an answer; a 2024-03-25 follow-up confirms name lookup *"ignores brush folder names"* (<https://community.adobe.com/questions-707/get-brush-via-script-1217239>).

Sources: <https://community.adobe.com/questions-712/how-to-get-actual-brush-display-names-not-internal-names-in-uxp-6-1182601> (2025-09-28), <https://forums.creativeclouddeveloper.com/t/need-help-finding-the-active-brush-presets-name/11437> (2025-09-19).

**The string IDs for group operations exist in Adobe's own SDK header**, `PIStringTerminology.h` (<https://github.com/AdobeDocs/photoshop-cpp-sdk/blob/main/pluginsdk/photoshopapi/photoshop/PIStringTerminology.h>, last commit 2024-12-20): `newBrushGroup` (line 3807), `brushFolderClass` and `brushGroup` (1118-1119), `defineBrush` (1896), and the panel-menu set `brushesDefine / brushesDelete / brushesExport / brushesImport / brushesLoad / brushesSave / brushesNew / brushesRename / brushesAppend` (1106-1116). The commands are reachable in principle; **no published recording shows `newBrushGroup`'s argument payload.** Recording it in Alchemist or ScriptListener is a 30-minute experiment and should be the first thing validated before committing to any automation design.

---

## [03] — BRUSH SETTINGS PANEL: WHAT A CURATED LIBRARY MUST NORMALIZE

### [3.1] A documentation warning you need before the table

**Adobe has deleted its own deep brush reference.** `helpx.adobe.com/photoshop/using/creating-modifying-brushes.html` — which contained the complete Brush Settings panel reference — now **301-redirects to a ~200-word task page** (`create-brush-tip-image.html`). The same happened to `brush-presets.html` → `select-a-preset-brush.html`. Verified live 2026-09-11.

The full reference survives only in the Wayback Machine:
- **<https://web.archive.org/web/20230728030137/https://helpx.adobe.com/photoshop/using/creating-modifying-brushes.html>** — Brush Settings panel overview, standard/bristle/erodible/airbrush tip options, Brush Pose, "Other brush options" (Noise, Wet Edges, Build-up, Smoothing, Protect Texture), Stroke Smoothing modes, Brush Scattering, Clear Brush Controls.

Two legacy pages *do* still resolve and hold real detail (both marked "Last updated on May 24, 2023"):
- **<https://helpx.adobe.com/photoshop/using/adding-dynamic-elements-brushes.html>** — Shape Dynamics, Color Dynamics, Transfer.
- **<https://helpx.adobe.com/photoshop/using/creating-textured-brushes.html>** — Texture, Dual Brush.

Current (2026-02-23) task pages, useful only for procedure:
- <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/display-brush-panel-brush-options.html>
- <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/create-brush-set-painting-options.html>
- <https://helpx.adobe.com/photoshop/desktop/apply-painting-techniques/brushes-presets/create-brush-tip-image.html>

**Archive the Wayback copy locally.** It is the only complete reference and Adobe has already shown it will drop it.

### [3.2] Every setting, and the normalization decision for an entourage library

| Group | Setting | What it does (Adobe's words, condensed) | Normalize to |
| :-- | :-- | :-- | :-- |
| **Brush Tip Shape** | **Size** | Brush diameter in **pixels** | Set deliberately per category ([04]). Resolution-dependent — this is the whole problem. |
| | **Use Sample Size** | Resets to the tip's original sampled diameter. Only available for sampled tips | Note each tip's native size; it is your quality ceiling |
| | Flip X / Flip Y | Mirrors the tip on its axis | Off in the preset; flip at paint time |
| | **Angle** | Rotation of the long axis from horizontal | **0°** for all entourage. Nonzero angle on a tree is a defect |
| | **Roundness** | Ratio of short to long axis. 100% = circular, 0% = linear | **100%** for all entourage |
| | **Hardness** | Size of the hard centre. **Cannot be changed on sampled brushes** | N/A for stamp brushes |
| | **Spacing** | Distance between marks as % of diameter. Deselected → cursor speed decides | **1000%+ for stamp brushes** (one mark per click). This is the single most important normalization. A people brush at 25% spacing smears a crowd. |
| **Shape Dynamics** | Size Jitter + Control (Off/Fade/Pen Pressure/Tilt/Wheel), Minimum Diameter, Tilt Scale | Varies mark size across a stroke | **Off** for placed entourage. On (20-40%) for scatter vegetation |
| | **Angle Jitter** + Control, incl. *Initial Direction* / *Direction* | Varies mark angle, as % of 360° | **0%** for people and trees (they must stay upright). 5-15% for ground cover |
| | **Roundness Jitter** + Control, Minimum Roundness | Varies the short/long axis ratio | **0%** for entourage |
| | Brush Projection | Tilt/rotation alters tip shape with a stylus | Off |
| **Scattering** | Scatter + Both Axes, Count, Count Jitter + Control | Number and placement of marks. Both Axes = radial; off = perpendicular to path | **Off** for people/trees. On for grass, foliage, atmosphere. Adobe warns: *"If you increase the count without increasing the spacing or scattering values, painting performance may decrease."* |
| **Texture** | Pattern, Invert, Scale, Texture Each Tip, Mode, Depth, Minimum Depth, Depth Jitter + Control | Makes strokes read as painted on textured stock | **Off** for entourage. Reserve for a separate "paper/canvas" group |
| **Dual Brush** | Mode, Diameter, Spacing, Scatter (Both Axes), Count | Second tip intersected with the primary | **Off** for entourage |
| **Color Dynamics** | **Apply per tip**, FG/BG Jitter + Control, Hue Jitter, Saturation Jitter, Brightness Jitter, Purity (−100…100) | Varies paint colour across a stroke | **Off** for silhouette entourage (you want flat, recolourable marks). *Apply per tip* + 5-15% Hue/Brightness Jitter is the correct setting for vegetation masses |
| **Transfer** | Opacity Jitter + Control, Flow Jitter + Control | Varies opacity/flow, capped by the Options-bar values | **Off** for entourage. Modest Opacity Jitter suits atmosphere |
| **Brush Pose** | Tilt X, Tilt Y, Rotation, Pressure, + **Override** checkboxes | Static stylus pose; Override pins it | Use Override to force a fixed pose so mouse and tablet produce identical marks |
| **Noise** | (checkbox) | *"Adds additional randomness to individual brush tips. Most effective on soft brush tips"* | Off |
| **Wet Edges** | (checkbox) | *"Causes paint to build up along the edges of the stroke, creating a watercolor effect"* | Off (unless a deliberate watercolour group) |
| **Build-up / Airbrush** | (checkbox) | Gradual tone accumulation while held | Off for stamps |
| **Smoothing** | (checkbox in panel) + **Stroke Smoothing 0-100 in Options bar** with *Pulled String Mode*, *Stroke Catch Up*, *Catch-Up On Stroke End*, *Adjust For Zoom* | Smoother curves; 0 = legacy behaviour | Off for stamps. 10-20% for hand-drawn line groups |
| **Protect Texture** | (checkbox) | *"Applies the same pattern and scale to all brush presets that have a texture … to simulate a consistent canvas texture"* | Off unless running a textured group |

Bulk reset: **Clear Brush Controls** in the Brush Settings panel menu clears every changed option except brush shape.

Tip-size ceilings worth knowing:
- **A sampled tip can be at most 2500 × 2500 px** (`Edit > Define Brush Preset`). Adobe states it on the current page.
- Photoshop converts colour to greyscale when defining a tip. Layer masks on the source do not affect the definition.
- The Brush tool size slider tops out at **5000 px**; Bristle, Airbrush and Erodible tips cap at **300 px**.
- The **brush preview window was discontinued in Photoshop 21.0.3** (January 2020).

### [3.3] What the New Brush dialog actually stores

The dialog has three checkboxes. **Adobe does not document them on any live page** — this is verified from multiple independent tutorials, which agree exactly:

| Checkbox | Effect |
| :-- | :-- |
| **Capture Brush Size in Preset** | Stores the current diameter with the preset. **Unchecked, the brush inherits whatever size the last-used brush had.** Blunt statement from an XP-Pen tutorial: *"Remember to check the 'Capture Brush Size in Preset,' or your brush size will change to the size of the last brush you used"* (<https://www.xp-pen.com/blog/beginners-using-photoshop-for-digital-drawing.html>) |
| **Include Tool Settings** | Stores the Options-bar state — **Opacity, Flow, Blending Mode, Airbrush, Smoothing** — and for the Mixer Brush the Wet/Load/Mix values |
| **Include Color** | Stores the current foreground colour with the preset |

Corroborating sources: <https://learnthatyourself.com/define-brush-preset-in-photoshop/> (2022-08-31, names all three), <https://eduardooroz.co/mixing-brush-photoshop/> (2026-04-07, Mixer Brush case), <https://sydspix.wordpress.com/2025/07/14/mixing-it-up-with-legacy-bristle-brushes-in-photoshop-mixer-refresher/> (2025-07-14), <https://www.ninakphotography.com/free-photoshop-actions-sharpen-actions/>.

**For a curated entourage library: Capture Brush Size ON, Include Tool Settings ON, Include Color OFF.** Size-on is non-negotiable — it is what makes a *library* rather than a pile. Color-off keeps every mark recolourable from the foreground swatch.

---

## [04] — CONSISTENT SIZING ACROSS ENTOURAGE LIBRARIES (3840 × 2160 px)

### [4.1] What vendors actually publish: nothing

Checked every vendor named, plus the obvious adjacent ones. **Not one publishes a default brush diameter, a target size, or a scale-consistency standard.**

| Vendor | Real domain | What they publish | Resolution stated | Sizing guidance |
| :-- | :-- | :-- | :-- | :-- |
| **Show It Better** | <https://www.showitbetter.co/format/photoshop/> | Item counts, format lists (`.ABR`, PNG, CAD, AI, SVG) | **None.** PNGs called "high-resolution" with no number | **None.** Only *"You might have to adjust the scale to your drawing"* (CAD) |
| **LayerGrid** | **layergrid.io** (not .co) — <https://www.layergrid.io/products/realistic-bundle> | *"475 Realistic 4K ABR Brushes … 106 Trees • 83 Bushes • 36 Grass • 150 People • 100 Vehicles"* | **"4K" only, never defined in px or dpi.** Their <https://www.layergrid.io/pages/faq> has no dimensions | **None.** Usage step 3 is literally *"Position and resize the item as needed"* |
| **Upstairs** | **learnupstairs.com** — <https://www.learnupstairs.com/packs/basic-tree-silhouettes-brush-set> | *"Number of Brushes 6 / Format .ABR / Size 2 MB"* | **None** | **None** |
| **Bracken** | **bracken.design** — <https://www.bracken.design/products/sketch-smudge> | *"72 Photoshop Brushes"*, tablet & mouse versions | **None** | **None.** Not entourage anyway — pencils/charcoal/smudge |
| **Arqui9** | <https://www.arqui9learn.com/productbrushes> | *"58 custom brushes"* | **None** | **None** |
| **Visualizing Architecture** (Alex Hogrefe) | <https://visualizingarchitecture.com/> | Doesn't sell brushes | **Yes — repeatedly** ([4.3]) | **Yes — but as a horizon rule, not pixels** |

The one place a Show It Better tip's actual size is published is a third-party mirror: PsFiles' [SANAA Style Human Figures](https://psfiles.com/sanaa-style-human-figures-brush/) lists **"Dimensions: 250 pixel"**, while PsFiles' own [Sanaa People Pack 2](https://psfiles.com/sanaa-people-pack-2-photoshop-brushes/) (2019-07-21) lists **"Dimensions: 1500 pixel"**. A 6× spread between two people-brush packs of the same visual style is the whole problem in one data point.

Two **non-archviz** vendors do publish a design canvas, proving it is possible and that the archviz field simply does not bother:
- Shiyoon Kim: *"Each of these brushes have been fine tuned to work within **1920 × 1080 document with a 200 dpi resolution**"* — <https://shiyoonk.gumroad.com/l/TSZuW>
- True Grit Texture Supply (RizzCraft): *"**Maximum recommended resolution = 5000 × 5000px @300ppi**"* — <https://www.truegrittexturesupply.com/products/rizzcraft>; their support article admits *"The default texture scale for most of our brushes **varies**"*

**Kyle T. Webster publishes no design resolution.** Neither <https://www.kylebrush.com/> nor <https://www.kyletwebster.com/portfolio/brushes/> states a px or ppi figure, and no Adobe page attributes one to him.

### [4.2] Photoshop has no scale-aware brush, and Adobe says nothing about it

| Question | Answer | Source |
| :-- | :-- | :-- |
| Is brush size in pixels? | Yes. Slider caps at 5000 px | <https://community.adobe.com/questions-712/brush-size-limits-at-5000px-1181701> (2025-07-29) |
| Any scale-aware feature? | **No.** Open feature request since at least 2020 | <https://feedback.photoshop.com/conversations/photoshop/scale-brush-texture-with-document-resolution/5f5f46094b561a3d426fc668> |
| Is there a "Use Legacy brush size" preference? | **No such preference could be found on any Adobe page.** There are *Legacy Brushes* (the pre-CC-2018 libraries, loadable from the Brushes panel menu) and *Legacy Compositing*, which are different things. **[UNVERIFIED — treat as not a real feature until seen in your own Preferences]** |

**Consequence:** a preset saved at 400 px paints 400 px on a 3840 px canvas and 400 px on a 7680 px canvas — half the relative area. Nothing compensates. Your defaults are valid for exactly one reference canvas, and you must record which.

### [4.3] The only published sizing rules in archviz are real-world, not pixel

| Rule | Published form | Source |
| :-- | :-- | :-- |
| Horizon alignment | *"place the people so that **their eyes more or less line up with the horizon line** (lower for short people, higher for tall) and this will ensure your people are in scale with each other and with the scene"* | Hogrefe, <https://visualizingarchitecture.com/tutorial-adding-people-via-photoshop/> (2010) |
| 3D proxy over eyeballing | *"I dropped in scale figures into the Sketchup model and exported an image to use as a guide in Photoshop"* | Hogrefe, <https://visualizingarchitecture.com/harbor-view-break-down/> (2016-08-07) |
| Drawing-scale conversion | *"figuring out the scale that you use the most i.e. **1:100** and convert roughly **170cm**. This means each 'person' will need to be about **1.7cm tall**"* | <https://www.toscaleblog.co.uk/blog/how-to-add-people-in-your-architectural-drawings> |
| Per-image scaling, no numbers | *"Adjust the size and position of the cutout to fit the scene's perspective and scale. Use the 'Free Transform' tool"* | <https://www.bm-3d.com/en/post/how-to-add-people-to-renders-archviz-human-cutouts-tutorial> (2024-04-09) |

Hogrefe does publish canvas resolutions, the closest thing to a standard anyone in archviz states:

| Statement | Post |
| :-- | :-- |
| *"For final renderings, I will bump up the resolution to somewhere between **4500 and 5000 px** though I often suggest students can get away with **3000 px** images"* | <https://visualizingarchitecture.com/v-ray-settings-overview/> (2015-05-17) |
| *"set the output resolution to **4500px × 2520px**"* | <https://visualizingarchitecture.com/wharf-design-foggy-morning-perspective-part-2/> (2014-07-13) |
| *"The final rendering had a resolution of **6,000px × 3,333px**"* | <https://visualizingarchitecture.com/cliff-retreat-finale-image/> (2016-03-06) |
| Site plan *"10,000 pixels tall"* | <https://visualizingarchitecture.com/high-res-site-plan/> |

The 3840 × 2160 master digital canvas (`gui-indesign.md` [07] row 21) sits squarely inside his published working range.

### [4.4] Derive the standard yourself — the arithmetic

**Perspective views.** `figure px = 2160 × (1.7 m ÷ visible scene height in metres at the figure's depth)`

| Visible frame height at figure | Figure height | Fraction of canvas |
| :-- | :-- | :-- |
| 10 m (tight courtyard) | **367 px** | 1/6 |
| 12 m | **306 px** | 1/7 |
| **20 m (typical street/plaza)** | **184 px** | **~1/12** |
| 30 m (wide urban view) | **122 px** | 1/18 |
| 50 m (aerial-ish) | **73 px** | 1/29 |

**The 20 m frame is the perspective frame (108 px/m) and 180 px the people default on 3840 × 2160**, band 90–360 px (`cloud-libraries.md` [03.4] rows [01] and the frame paragraph); a realistic default for a landscape presentation perspective.

**Orthographic plans and elevations.** `px per metre = 3840 ÷ site width in metres`

| Site width across 3840 px | px/m | 1.7 m figure | 8 m tree | 25 m building |
| :-- | :-- | :-- | :-- | :-- |
| 50 m (the plan frame, 76.8 px/m) | 76.8 | **131 px** | 614 px | 1920 px |
| 100 m | 38.4 | **65 px** | 307 px | 960 px |
| 200 m | 19.2 | **33 px** | 154 px | 480 px |

**Print check.** 3840 px on the long edge = **232 ppi at A3**, **164 ppi at A2**, **116 ppi at A1**. 16:9 is not ISO 1.414 either. 3840 × 2160 is a *panel inside a board*, not a full sheet.

### [4.5] Practical consequences

1. **Normalize tips, not just presets.** A 250 px tip enlarged to 400 px is already 1.6× past native and softens. A 1500 px tip at 200 px throws away 87% of its detail but stays crisp. Build the people set from tips whose *native* size sits at or above your default — always scale down, never up.
2. **Distrust "4K" claims.** Adobe's define-brush ceiling is 2500 × 2500 px, so a vendor's "4K" almost certainly describes the PNG, not the ABR tip. (Community reports do show >2500 px tips in circulation — <https://community.adobe.com/questions-716/max-brush-size-is-not-2500-x-2500-1203247> — so verify on import rather than trusting marketing.)
3. **Capture Brush Size on every preset**, or Photoshop hands each brush the last-used diameter and your library has no defaults at all.
4. **Record the reference canvas** in the library name or a read-me — exactly what Shiyoon Kim does and no archviz vendor does.

**Default diameters on the 3840 × 2160 reference canvas:** the eighteen category rows of `cloud-libraries.md` [03.4] (default, band, derivation, spacing, jitter), derived from the two frames above; that table owns every value.

---

## [05] — ABR AUTHORING OUTSIDE PHOTOSHOP

### [5.1] The library matrix (all verified live 2026-09-11)

| Project | Lang | Last commit | ★ | Read | Write | Versions | Sampled bitmap | Groups |
| :-- | :-- | :-- | --: | :-- | :-- | :-- | :-- | :-- |
| [psd-tools](https://github.com/psd-tools/psd-tools) 1.19.0 ([PyPI 2026-09-02](https://pypi.org/pypi/psd-tools/json)) | Python | 2026-09-10 | 1458 | **no ABR reader** | no | — | generic RLE codec only | no |
| [pytoshop](https://github.com/mdboom/pytoshop) 1.2.1 | Python | 2022-09-07 | 127 | no | PSD only | — | — | no |
| **[0xC0000054/pdn-photoshop-brush](https://github.com/0xC0000054/pdn-photoshop-brush)** | C# | **2023-07-02** | 18 | yes | **yes — v1/v2 only** | reads 1,2,6,10; **writes 1,2** | yes (`AbrSave.cs`, RLE + raw) | **no** |
| [Krita](https://invent.kde.org/graphics/krita/-/blob/master/libs/brush/kis_abr_brush_collection.cpp) | C++ | **2021-07-21** (file) | — | yes | no | 1, 2, 6.1, 6.2 — **not 10** | yes (`rle_decode()`) | no |
| [GIMP](https://github.com/GNOME/gimp/blob/master/app/core/gimpbrush-load.c) | C | **2025-06-13** (file) | 6407 | yes | no | **1, 2, 6.1/6.2, 10.1/10.2** | yes | no |
| [jlai/brush-viewer](https://github.com/jlai/brush-viewer) (Kaitai) | TS | 2026-04-28 | 26 | yes | no | 6.1, 6.2 | yes | **`phry` declared, body = `unknown_data`** |
| [scurest/abrupng](https://github.com/scurest/abrupng) | Rust (unpublished) | 2019-07-25 | 39 | yes | no | 1,2,6 | yes | no |
| [Brush-Converter](https://github.com/HanmiAsuka/Brush-Converter) (fork; original 404s) | Python | 2025-10-23 | 71 | yes | **"can not pack back yet"** | partial | yes | no |
| [MorrowShore/PSBrushExtract](https://github.com/MorrowShore/PSBrushExtract) | Python | 2025-10-14 | 8 | yes | no | claims "all" (brute rip) | yes | no |
| [@azphalt/importer-abr](https://www.npmjs.com/package/@azphalt/importer-abr) 0.1.5 | TS | published 2026-08-14, **12 dl/wk** | n/a | yes | no (→ `.azp`) | 1,2,6+ | yes, PackBits | no |
| [brushfactory.co](https://brushfactory.co/) (closed SaaS) | web | live | — | yes | claims `.abr` write | unstated | — | *"Exporting is paused"* on the page today |

**Registry sweeps, run live 2026-09-11:**

| Registry | Query | Result |
| :-- | :-- | :-- |
| PyPI | `abr` | [Auditory Brainstem Response analysis](https://pypi.org/pypi/abr/json) — unrelated. `abrtools`, `abrviewer`, `abr2gbr`, `photoshop-brushes` → **404** |
| crates.io | `abr` | [v0.0.1, 2020-06-22](https://crates.io/api/v1/crates/abr) — an Android screen-size CLI. No `photoshop-brush` crate; `abrupng` was never published |
| npm | `abr-parser`, `photoshop-abr`, `abr-reader`, `abr2png` | **all 404.** `abr` is an empty 0.0.0 placeholder (2022-04-11). Only real hit: `@azphalt/importer-abr` |

**There is no "abr-parser" npm package.** It does not exist.

### [5.2] Reference implementations

**Krita** — best-structured reader, narrowest range. `libs/brush/kis_abr_brush_collection.cpp`, 636 lines, file last touched 2021-07-21:

```c
static bool abr_supported_content(AbrInfo *abr_hdr) {
    switch (abr_hdr->version) {
    case 1: case 2: return true;
    case 6: if (abr_hdr->subversion == 1 || abr_hdr->subversion == 2) return true;
    }
    return false;                       // no case 10
}
```

It seeks only the `8BIM`/`samp` section, decodes raw or RLE tips, and skips computed brushes, brushes wider than 16384 px, and everything in `desc`. No `phry`, no groups, no dynamics. An open draft MR — [!2539 "Improved .abr bundle importing"](https://invent.kde.org/graphics/krita/-/merge_requests/2539), opened 2025-11-24, still draft — concedes Krita *"only imports brush tip … and discarded patterns."*

**GIMP** — widest version coverage. `app/core/gimpbrush-load.c`, 1055 lines, file last touched 2025-06-13:

```c
case 1: case 2:  return TRUE;
case 10: case 6: /* count contains format sub-version */
  if (abr_hdr->count == 1 || abr_hdr->count == 2) return TRUE;
```

Same shape as Krita (both descend from [Eric Lamarque's 2007 patch](https://code.obermui.de/eUgEntOptIc44/gimp/commit/8cb4d6070b50a5a5939c3228c4d2002b660d53cc.patch)), plus v10. Sampled brushes only. No groups.

**The best spec artifact in existence** is the Kaitai grammar at <https://github.com/jlai/brush-viewer/blob/main/shared/abr/ABR.ksy>. It enumerates all four block keys — `samp`, `desc`, `patt`, `phry` — and fully types the Action Descriptor. And it reads `phry` as opaque bytes.

### [5.3] Format documentation

| Source | URL | Covers |
| :-- | :-- | :-- |
| Archive Team file-format wiki | <http://fileformats.archiveteam.org/wiki/Photoshop_brush> | v1/v2 "documented in old Photoshop specs"; v6/v7 **"not publicly documented by Adobe"**; `samp` layout for 6.1 and 6.2 |
| brushfactory.co writeup | <https://brushfactory.co/formats/abr> | Header, `8BIM` framing, `samp` UUID+rect+depth+compression, `desc`/`Brsh`/`bVTy`. Notes that real encoders emit RLE rows that **overrun the row**, and Photoshop clips rather than failing |
| Krita ↔ Photoshop mapping table | <https://community.kde.org/Krita/Photoshop_Mapping_Table> | Maps `Brsh (VlLs)` descriptor keys to Krita dynamics |

### [5.4] Does Adobe publish an ABR spec? **No — verified byte-level.**

Fetched <https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/> — HTTP 200, 393,007 bytes, `Last-Modified: Sun, 24 Nov 2019 16:48:30 GMT`, titled *Adobe Photoshop File Formats Specification*.

| Token | Occurrences |
| :-- | --: |
| `brush` / `Brush` / `ABR` | **0 / 0 / 0** |
| `8BIM` | 8 |
| `Pattern` | 11 |
| `Duotone` | 23 |

The spec defines the `8BIM` and descriptor primitives ABR reuses but never mentions brushes. Adobe last documented v1/v2 in the Photoshop 6.0 spec, reachable only via archive.org. The spec has not changed since 2019-11-24.

### [5.5] Verdict — build it inside Photoshop

Four reasons, each independently sufficient:

1. **No writer exists for the format Photoshop uses.** The only open-source encoder, `AbrSave.cs`, emits `AbrFileVersion { Version1 = 1, Version2 }` — the pre-CS2 layout, no `desc` block, therefore no dynamics and no groups. Idle since 2023-07-02.
2. **Groups are undecoded everywhere.** The most rigorous artifact in existence names `phry` and reads it as opaque bytes. If nobody can *read* the hierarchy, nobody can round-trip it, let alone synthesize it.
3. **No spec to write against**, and the brushfactory writeup warns that Photoshop's own encoder emits malformed RLE — meaning "valid per spec" and "accepted by Photoshop" are different sets, and you cannot test the second from outside.
4. **The ecosystem is thinning.** Krita's ABR file is five years cold with its improvement MR in draft since Nov 2025. The one new npm package has 12 downloads/week, no repository, and no validation against a real Photoshop file.

**Recommended shape.** A UXP `.psjs` script that, per brush: opens the tip PNG → Select All → `defineBrush` with the target name → drives `newBrushGroup` for the folder structure → ends in `brushesExport` to emit a Photoshop-authored `.abr`.

**The one open risk is `newBrushGroup`'s descriptor payload.** Adobe's SDK header proves the command exists; no published recording shows its arguments. Record it in Alchemist or ScriptListener (record *New Brush Group*, then a drag of a brush into it) **before** committing to the design. If the drag proves unrecordable, the fallback is **one `.abr` per group** — Photoshop creates a group named after each imported set automatically, which is precisely the behaviour the [2018-01-09 thread](https://community.adobe.com/questions-712/brushes-load-automatically-intermediate-folder-added-to-new-groups-1061321) complains about and that you would be exploiting deliberately.

**Useful outside-Photoshop role for psd-tools.** It is not an ABR reader, but it *is* the right toolkit to build one for **validation**: `psd_tools.psd.descriptor` implements read **and write** for every OSType; `psd/patterns.py` has `VirtualMemoryArrayList` — the exact v6.2 sample container; `compression` exposes PackBits both ways. Issue [#671](https://github.com/psd-tools/psd-tools/issues/671) (filed 2026-06-19, fixed 2026-06-20) was raised by someone doing exactly this: *"extract channel bitmaps from Photoshop `.abr` brush presets (which internally reuse the same `VirtualMemoryArrayList` and ActionDescriptor structures as PSD pattern files)."* Use it to **inspect and verify** the ABRs Photoshop produces — never to author them.

---

## [06] — PHOTOSHOP 27.x WORKSPACE, TOOLBAR, QUIET MODE, UI CHANGES

### [6.1] Toolbar customization — `Edit > Toolbar`

Current Adobe doc, **2026-02-23**: <https://helpx.adobe.com/photoshop/desktop/get-started/set-up-toolbars-panels/customize-the-toolbar.html>

Two entry points: menu `Edit > Toolbar`, or the **`…` icon at the bottom of the toolbar → Edit Toolbar**.

Dialog anatomy, verbatim from Adobe:

- Left column = current Toolbar; right column = **Extra Tools**
- *"Drag and drop tools and groups to reorganize the toolbar"*
- *"Move less frequently used tools to Extra Tools"*
- **Toggle, which shows extra tools in the last toolbar slot** — this is the `…` overflow control
- **Save Preset** / **Load Preset** — a toolbar layout is a portable file (extension `.tbr`, per <https://www.file-extensions.org/extension/tbr>; Adobe does not name it)
- **Restore Defaults**
- **Clear Tools** — moves *every* tool to Extra Tools
- **Done** / **Cancel**

Grouping: tools nested under a parent can be reordered inside the dialog; dragging a sub-tool above its parent makes it the visible default for that slot.

**Can the Screen Mode / Quick Mask / Foreground-Background chips be hidden? Yes.**

Row of toggle icons at the **bottom of the Customize Toolbar dialog** controls four chrome elements independently:

| Chip | Hideable | Keyboard equivalent once hidden |
| :-- | :-- | :-- |
| The `…` overflow (Edit Toolbar button) | **Yes** — the first button | reachable via `Edit > Toolbar` |
| Foreground / Background colour swatches | **Yes** | `D` (defaults), `X` (swap) |
| Quick Mask Mode | **Yes** | `Q` |
| Change Screen Mode | **Yes** | `F` |

Sources: <https://www.psdvault.com/basics/photoshop-toolbar-customisation/> (*"You can hide the three dots slot which contains all the hidden tools, the foreground and background color box, Quick Mask mode, and screen mode"*), CreativePro <https://creativepro.com/customizing-photoshop-interface/>, and Rocky Mountain Training on removing the `…` button specifically: <https://www.rockymountaintraining.com/adobe-photoshop-cant-find-tool/>. A 2025 writeup notes the dialog now also toggles *"the generative AI workspace"* chip: <https://www.photoshoproadmap.com/customize-your-photoshop-toolbar-for-an-optimized-workflow/> (2025-07-12).

Recovery when a tool vanishes: `Edit > Toolbar > Restore Defaults`, or `Window > Workspace > Reset Essentials` — <https://helpx.adobe.com/photoshop/kb/bringing-back-tools.html> (2024-10-17).

### [6.2] Workspaces and the `.psw` file

`Window > Workspace > New Workspace`. Adobe's page, **2026-02-23**: <https://helpx.adobe.com/photoshop/desktop/get-started/learn-the-basics/save-custom-workspaces.html>

The **Capture** section offers exactly three checkboxes:

1. **Panel Locations**
2. **Keyboard Shortcuts**
3. **Menus**

**The toolbar is NOT in that list.** This is the important structural fact. Adobe's KB page on missing tools says toolbar settings *"are saved with your preferences and can also be saved as part of a workspace"* — which conflicts with the New Workspace dialog. The preference-file table settles it: toolbar state lives in its own `Toolbar Customization.psp` / `Toolbar Customization Primary.psp` pair, and the Customize Toolbar dialog has its own Save Preset / Load Preset. **Treat the toolbar as a separate artifact from the workspace and save both.**

File layout:

| Artifact | File | Path |
| :-- | :-- | :-- |
| A custom workspace | `[Workspace Name].psw` | `~/Library/Preferences/Adobe Photoshop [version] Settings/WorkSpaces/` |
| Modifications to any workspace | (auto) | `…/WorkSpaces (Modified)/` |
| Which workspaces are loaded | `Workspace Prefs.psp` | `…Settings/` |
| Toolbar layout | `Toolbar Customization.psp` + `.tbr` presets | `…Settings/` |
| Shortcuts | `Keyboard Shortcuts.psp` + `…Primary.psp` | `…Settings/` |
| Menus | `Menu Customization.psp` + `…Primary.psp` | `…Settings/` |

`Select and Mask.psw` and `Content-Aware Fill.psw` also live in `~/Library/Preferences/` — those workspaces are `.psw` files too.

**To move a full working setup to another machine, copy: the `WorkSpaces/` folder, `Workspace Prefs.psp`, `Toolbar Customization*.psp`, `Keyboard Shortcuts*.psp`, `Menu Customization*.psp`, and `Brushes.psp`.** There is no supported export tool ([2.4]).

### [6.3] Quiet Mode

Shipped in **Photoshop 26.11** (Sept 2025). It is the renamed and relocated former "focus mode" — confirmed by a critic who first reported it removed and then retracted: <https://unsung.aresluna.org/photoshops-challenges-with-focus-pt-2> (2026-04-29). Julieanne Kost's writeup: <https://jkost.com/blog/2025/09/5-quiet-photoshop-updates-you-may-have-missed.html> (2025-09-23) — *"Quiet Mode — Silence distracting pop-ups and notifications in Photoshop and Camera Raw."*

It is scriptable. UXP reference, **`PreferencesNotifications`**, min version **26.11**: <https://adobedocs.github.io/uxp-photoshop/ps_reference/classes/preferences/preferencesnotifications/>

| Property | Type | Access | Min | Note |
| :-- | :-- | :-- | :-- | :-- |
| `quietMode` | boolean | R/W | 26.11 | *"Enables or disables Quiet Mode, which limits in-app messages and notifications. When Quiet Mode is enabled, certain notification preferences become read-only"* |
| `showFeatureOnboarding` | boolean | R/W | 26.11 | Locked while Quiet Mode is on |
| `showWhatsNew` | boolean | R/W | 26.11 | Locked while Quiet Mode is on |
| `useRichToolTips` | boolean | R/W | 26.11 | Locked while Quiet Mode is on |

Reached as `app.preferences.notifications.quietMode`. Attempts to modify locked preferences **throw** while Quiet Mode is active — <https://adobedocs.github.io/uxp-photoshop/ps_reference/classes/preferences/>. Changelog entry: *"New Notifications group added to align with Preferences dialog. Included is Quiet mode with associated override for Rich Tooltips, What's new, and Feature Onboarding"* — <https://adobedocs.github.io/uxp-photoshop/ps_reference/changelog/>

### [6.4] Contextual Task Bar

Adobe doc, **last updated 2026-04-28**: <https://helpx.adobe.com/photoshop/desktop/get-started/learn-the-basics/boost-workflows-with-the-contextual-task-bar.html> — an on-canvas bar that changes with selection/layer/edit state.

It has become a primary surface in 27.x, not a convenience:
- **27.6 (April 2026)** — *"Access the right tools faster with an updated Contextual Task Bar"*
- **27.9.1 (July 2026)** — Remove tool options, including the Find distractions dropdown, Add to / Subtract from, and Remove after each stroke, moved into it
- **27.10 (August 2026)** — **Prompt to Edit** natural-language editing lives in it

Hide it: right-click the canvas edge (per <https://www.photoshoproadmap.com/before-and-after-view-plus-dynamic-layers-in-photoshop-beta/>, 2026-09-10).

### [6.5] The 2026 UI change, and what landed in the September 2026 beta

**The structural change is Spectrum.** A technology preview, **default On**, on <https://helpx.adobe.com/photoshop/desktop/whats-new/list-of-technology-preview-features.html> (2026-02-23):

> **Enable Modern User Interface** — *On.* This modifies the appearance of some control bars and dialogs to be more consistent with other Creative Cloud applications through the adoption of **Spectrum**, the multi-platform design system from Adobe. Restart Photoshop for the change to take effect.

**You can turn it off**, in Preferences → Technology Previews. Reception has been hostile in the professional press — <https://unsung.aresluna.org/photoshops-challenges-with-focus-pt-2> (2026-04-29) documents dialogs where the first field is not focused on open, and <https://frnt.com/story/16753/why-tech-critics-hate-adobes-new-photoshop-look> (2026-05-06) summarizes the criticism. If your workflow is keyboard-heavy, test both states.

**Shipping 27.10 (August 2026)** — <https://helpx.adobe.com/photoshop/desktop/whats-new/photoshop-on-desktop-release-notes.html>:
- **AI Assisted Editor (Beta)** — a separate prompt-driven editing mode, reached from a **toggle on the menu bar that can be hidden in Settings** (<https://9to5mac.com/2026/08/27/adobe-photoshop-adds-optional-ai-assisted-editor-mode-for-fully-prompt-based-image-editing/>)
- **Prompt to Edit** and **Markup** in the Contextual Task Bar
- **Light adjustment layer** — non-destructive exposure/contrast/highlights/shadows/whites/blacks
- **Customize the file formats in Save As** — reorder, show, hide formats in Save As and Save a Copy
- Adobe Stock search and place in-app
- **Dynamic Text** along shapes and paths
- AMD Zen 4/Zen 5 performance work
- LTS: Photoshop 2026 LTS is **26.11.7**

**September 2026 beta** (Release Notes - August 2026, posted 2026-09-04: <https://community.adobe.com/announcements-698/release-notes-august-2026-1640387>):
- **Enhance Edge** — AI colour-fringe decontamination on mask edges, from the Contextual Task Bar or by right-clicking a layer mask. Costs 20 generative credits; writes Content Credentials
- **Redesigned Stock panel**
- **Quick Compare** — modal before/after with Single View and Split View, zoom and pan (<https://community.adobe.com/announcements-698/quick-compare-is-now-in-photoshop-beta-1640291>, 2026-09-03)
- **Dynamic Layers** — scrub a slider to apply an AI transformation partially (<https://www.photoshoproadmap.com/before-and-after-view-plus-dynamic-layers-in-photoshop-beta/>, 2026-09-10)
- Relight: Original Lighting slider now defaults to 100% (was 50%); X/Y position sliders added

**Nothing in 27.x has touched brushes, the Brushes panel, or preset management.** The brush system is in maintenance. That is worth knowing before investing in it: it is stable, and it is not going to get better.

---

## [PART_B] — SKILLS AND KNOWLEDGE PACKS

## [07] — CLAUDE CODE SKILLS FOR ADOBE AND PRINT DESIGN

### [7.1] The honest headline

**Nothing meets a professional bar across the full brief.** There is no skill anywhere on GitHub that knows print typography as a craft — no optical margin alignment, no type-scale derivation, no Tschichold or Van de Graaf canon, no baseline-grid-to-leading arithmetic, no widow/orphan policy, no kerning judgment, no architectural sheet layout. Every "Swiss design" skill is a Tailwind theme. Every "editorial design" skill is a screen aesthetic. The word *editorial* has been captured entirely by web design.

Two hard numbers that settle it: searching all of GitHub for `CMYK prepress bleed filename:SKILL.md` returns **13 results**. Searching for `"GREP" indesign "paragraph style" filename:SKILL.md` returns **5**, two of which are a doc scrape and one hallucination.

### [7.2] Anthropic's own repo — nothing Adobe, nothing print

**[anthropics/skills](https://github.com/anthropics/skills)** — 175,843 ★, `pushed_at` **2026-09-10**. Full inventory at HEAD, 19 skills: `academy-guide`, `algorithmic-art`, `brand-guidelines`, `canvas-design`, `claude-api`, `discernment-nudge`, `doc-coauthoring`, `docx`, `frontend-design`, `internal-comms`, `mcp-builder`, `pdf`, `pptx`, `skill-creator`, `slack-gif-creator`, `theme-factory`, `web-artifacts-builder`, `webapp-testing`, `xlsx`.

All 501 paths in the repo grepped for `adobe|indesign|photoshop|illustrator|uxp|extendscript|typograph|grid|swiss|editorial` → **0 matches**.

| Design/document skill | Size | What it is | For your subjects |
| :-- | :-- | :-- | :-- |
| [`pptx`](https://github.com/anthropics/skills/tree/main/skills/pptx) | 20,796 B + `add_slide.py` (14 KB), OOXML XSDs, LibreOffice validators | Real OOXML engineering | Irrelevant, but a **model** of how a serious skill ships scripts |
| [`pdf`](https://github.com/anthropics/skills/tree/main/skills/pdf) | 8,072 B + `forms.md` (11,854 B), `reference.md` (16,692 B), 8 scripts | Form filling, bbox, page→image | PDF *forms* only. Zero prepress, zero PDF/X |
| [`canvas-design`](https://github.com/anthropics/skills/tree/main/skills/canvas-design) | 11,939 B + ~40 bundled OFL TTFs | Poster composition in HTML with real fonts | Closest to graphic design; still screen, no grid theory, no CMYK |
| [`brand-guidelines`](https://github.com/anthropics/skills/tree/main/skills/brand-guidelines) | 2,235 B | Anthropic's own colours and type | Thin by design |
| [`theme-factory`](https://github.com/anthropics/skills/tree/main/skills/theme-factory) | 3,124 B + 10 theme files (~520 B each) | Palette presets | **JUNK** for this purpose |

**[anthropics/claude-plugins-official](https://github.com/anthropics/claude-plugins-official)** — 36,145 ★, pushed 2026-09-11. Every plugin name dumped: **`adobe-for-creativity` is the only Adobe entry.** Design entries are all web/UI: `figma`, `canva`, `superdesign`, `frontend-design`, `ui-theme-designer`, `playground`. **No print, editorial, typography, or grid plugin exists in Anthropic's official marketplace.**

**[obra/superpowers](https://github.com/obra/superpowers)** — 285,352 ★, pushed 2026-09-11. All 14 skills are software-process (`brainstorming`, `test-driven-development`, `systematic-debugging`, `writing-plans`, `verification-before-completion`, …). **Nothing design, nothing Adobe.** Its value to you is `writing-skills/SKILL.md` (22,623 B) + `anthropic-best-practices.md` (45,820 B) as **authoring templates**. [obra/superpowers-marketplace](https://github.com/obra/superpowers-marketplace) — 1,256 ★, 2026-09-08 — carries no design or Adobe plugin.

### [7.3] Adobe's own skills repo — enterprise only

**[adobe/skills](https://github.com/adobe/skills)** — 182 ★, created 2026-02-05, pushed **2026-09-11**. Official Adobe org with a `.claude-plugin/marketplace.json` of 15 plugins: `adobe-analytics`, `adobe-cja`, five `aem-*`, `app-builder`, three `commerce-*`, `workfront`, `stardust`, `web`, **`adobe-for-creativity`**, `run-workflow`.

The only Creative Cloud content, at [`plugins/creative-cloud/`](https://github.com/adobe/skills/tree/main/plugins/creative-cloud) (last touched 2026-08-20):

| Skill | Bytes | Content |
| :-- | --: | :-- |
| `adobe-batch-edit-photos` | 40,615 | Lightroom/Photoshop **cloud** batch looks |
| `adobe-retouch-portraits` | 41,044 | Cloud retouch pipeline |
| `adobe-create-social-variations` | 30,915 | Express/PS crops |
| `adobe-create-pdfs-from-data` | 14,916 | **InDesign-powered** CSV→badges/certificates merge |
| `adobe` (router) | 8,947 | Onboarding tour, capability grid |

All of it routes through a hosted MCP at `https://adobe-creativity.adobe.io/mcp`. **Zero ExtendScript, zero UXP, zero batchPlay, zero local-app scripting.** The router SKILL.md is largely UI chrome instruction ("Adobe's red `#FA0F00` appears exactly once per card, as a 2px accent rule") — well-written marketing surface, not an automation capability.

**Verdict: MEDIOCRE.** Adobe's agent investment went to Experience Cloud. Creative Cloud got a consumer connector.

### [7.4] Photoshop / UXP / batchPlay

| Repo | ★ | Last push | What the SKILL.md is | Verdict |
| :-- | --: | :-- | :-- | :-- |
| **[kevinkiklee/adobe-plugin-skills](https://github.com/kevinkiklee/adobe-plugin-skills)** | **13** | **2026-07-10** | SKILL.md **30,774 B / ~3,900 words** + `ps-uxp-sdk.md` (66,753 B) + `lrc-sdk.md` (70,110 B) + CHANGELOG (11,880 B) + design spec (39,660 B). States its baseline: *"Photoshop API changelog 27.4 (Feb 2026) / UXP 9.2.0 / Manifest v5 … Last verified: 2026-07."* Organized pitfalls-first: *"`<canvas>` exists (v7.0.0+) but has **no** `drawImage`, `getImageData`, `putImageData`, `toDataURL`, `toBlob`"*; `executeAsModal` anti-patterns; *"DOM file APIs take Entries; batchPlay takes session tokens"*; capability matrix (36 Document props / 29 methods, 28 Layer props / 55 methods) | **EXCELLENT** — the single best artifact in this audit. Version-pinned, failure-mode-organized, 167 KB of real SDK reference |
| [dcc-mcp/dcc-mcp-photoshop](https://github.com/dcc-mcp/dcc-mcp-photoshop) | 13 | 2026-08-29 | MCP↔Photoshop UXP WebSocket bridge | Infrastructure, not knowledge. **MEDIOCRE** as a pack |
| [SFKislev/Flue](https://github.com/SFKislev/Flue) — [`skills/photoshop/SKILL.md`](https://github.com/SFKislev/Flue/blob/main/skills/photoshop/SKILL.md) | 82 | 2026-09-01 | **234 words.** A pointer to `photoshop_bridge.py`. Contains a **hardcoded path to the author's machine**: `C:\Users\fredd\.claude\skills\flue\SKILL.md` | **MEDIOCRE** — the bridge is novel, the skill file is a stub with a leaked dev path |
| [znznzna/photoshop-cli](https://github.com/znznzna/photoshop-cli) | **0** | 2026-03-11 | CLI via a UXP plugin | **JUNK** — zero stars, six months stale |
| [grain-tech/photoshop-auto-shadows](https://github.com/grain-tech/photoshop-auto-shadows) | 0 | 2026-04-09 | One batch-shadow skill | **JUNK** |

### [7.5] Illustrator

| Repo | ★ | Last change | Content | Verdict |
| :-- | --: | :-- | :-- | :-- |
| **[github/awesome-copilot — `adobe-illustrator-scripting`](https://github.com/github/awesome-copilot/tree/main/skills/adobe-illustrator-scripting)** | 38,907 (repo) | skill edited **2026-05-28** | SKILL.md **35,264 B / ~4,395 words** + `references/object-model-quick-reference.md` + three runnable JSX (`batch-export-png.jsx`, `create-color-grid.jsx`, `find-replace-text.jsx`). Covers Application/Document/Layer/PathItem/TextFrame, coordinate system, units, `#target illustrator`, `#targetengine "session"`, export workflows, data-driven variables/datasets, scripted print options | **EXCELLENT** — best-in-class for Illustrator. ES3 ExtendScript only, which per [08] is correct: Illustrator has no public UXP |
| [alexander-ladygin/illustrator-scripts](https://github.com/alexander-ladygin/illustrator-scripts) | 1,498 | **2023-09-04** | Script corpus, MIT | Unmaintained. Prefer creold |
| [crichalchemist/adobe-cli](https://github.com/crichalchemist/adobe-cli) | **2** | 2026-05-01 | CLI harness | **JUNK** — one day of commits |

That the only strong Illustrator skill lives in **GitHub's Copilot repo**, not a Claude one, is the finding. It is mirrored into ~25 downstream repos (`agent-skills-mirror`, `awesome-harness-primitives`, …) — copies, not improvements. Ignore them.

### [7.6] InDesign, GREP, typography automation

| Repo | ★ | Last push | Content | Verdict |
| :-- | --: | :-- | :-- | :-- |
| **[zhanglongxiao111/indesign-cli](https://github.com/zhanglongxiao111/indesign-cli)** | **46** | **2026-09-05** | **150 InDesign tools** over COM/`winax`, HTML↔InDesign conversion, JSON returns. SKILL.md (Chinese) routes to `references/html-authoring.md` (17,593 B), `direct-indesign-editing.md`, `template-filling.md`, `failure-handling.md` (five `error.category` values). Real layout modelling in source: `src/tools/masterSpread/`, `document/preferences.js` (38,571 B), `export/operations.js`. README names its targets: *"AI-generated design decks, **architecture presentations**, brand manuals, template-driven publishing"* | **EXCELLENT with hard caveats** — **Windows-only COM**, Chinese-first, and the shipped EXE **defaults telemetry to the vendor's NAS** (README tells external users to set `INDESIGN_CLI_TELEMETRY=off`). Read the source; don't install the EXE |
| **[MoebiusSt/indesign-scripting-mcp](https://github.com/MoebiusSt/indesign-scripting-mcp)** | **2** | **2026-09-10** | 4,580 B SKILL.md + 2,717 B reference. Two MCP servers (`indesign-dom` lookup, `indesign-exec` JSX). Enforces a real contract: `get_quick_reference()` → `get_gotchas(context)` → `search_dom`/`lookup_class` → `run_jsx(undo_mode="entire", undo_name="Agent: <task>")` → verify → `undo(steps=1)`. Script-label hygiene policy (`_tmp_` cleared at task end, `agentContext_` persisted) | **EXCELLENT process, MEDIOCRE reach** — 2 stars, Windows COM/OLE only. The best-designed *agent contract* for InDesign in existence, and nobody uses it |
| [LevyBytes/AI-SKILL-adobe-products](https://github.com/LevyBytes/AI-SKILL-adobe-products) | **5** | 2026-06-22 | **1,527 files.** Router (610 words) → 11 product subskills → **1,425 reference `.md`**: InDesign 368 (6.0 MB), Common Platform 222, Acrobat 193, After Effects 186, Premiere 152, Photoshop 139, Illustrator 74, InCopy 48, Firefly 46. Includes `uxp-find-grep-preference*.md`, `uxp-paragraph-style*.md`, `help-grids.md`, `help-composition-and-text-wrapping.md`, `help-chinese-japanese-and-korean*.md` (mojikumi/yakumono/warichu) | **GOOD-but-flawed.** It is a **rewritten scrape of helpx.adobe.com + the UXP DOM docs**, not craft knowledge. The UXP object pages are near-empty: the whole of `uxp-find-grep-preference.md` is *"Preferences used when finding grep text."* Duplicate suffixes (`-2-2`, `-3`) betray sloppy dedup. Derivative-copyright risk. 5 stars = unvetted. **Treat as a searchable mirror of helpx, nothing more** |
| [Aradotso/design-skills — `adobe-indesign-2026-workflow`](https://github.com/Aradotso/design-skills/blob/main/skills/adobe-indesign-2026-workflow/SKILL.md) | 4 | 2026-08-04 | 2,135 words, "Auto-generated … by ara.so". Describes a repo named *"Adobe-InDesign-2026"* it never links. Trigger list is keyword spam | **JUNK** — LLM-generated about a phantom source |

**On GREP specifically: nobody has written a real InDesign GREP skill.** The metacharacter set, `findGrepPreferences`/`changeGrepPreferences` state hygiene, nested styles, GREP styles inside paragraph styles — none of it exists as an agent skill anywhere.

### [7.7] Adobe automation in general

**[SFKislev/Flue](https://github.com/SFKislev/Flue)** — 82 ★, pushed 2026-09-01, created 2026-04-17. One `pip install`, no MCP servers; bridges 13 apps (Photoshop, Illustrator, InDesign, Premiere, After Effects, Audition, Blender, Houdini, 3ds Max, Unity, Office) via each app's native runtime (COM/AppleScript/CEP). Featured in [hesreallyhim/awesome-claude-code](https://github.com/hesreallyhim/awesome-claude-code) (53,898 ★, pushed 2026-09-11) with the claim *"InDesign alone exposes ~28k objects."*

**The architecture is the best idea in this space. The skills are not** — every per-app SKILL.md is 230–290 words of "read `FLUE.md`, then read `C:\Users\fredd\…`". **Verdict: EXCELLENT bridge, JUNK-tier skill files.**

For completeness: [aedev-tools/adobe-agent-skills](https://github.com/aedev-tools/adobe-agent-skills) (98 ★, **2026-03-02** — six months stale, After Effects only); [iart-ai/motion-design-skills](https://github.com/iart-ai/motion-design-skills) (29 ★, 2026-06-22, motion not print); [znznzna/lightroom-cli](https://github.com/znznzna/lightroom-cli) (22 ★, 2026-04-06, 107 LrC commands).

### [7.8] Graphic, editorial, print, Swiss, grid

| Repo | ★ | Last push | Content | Verdict |
| :-- | --: | :-- | :-- | :-- |
| [SkillMedev/skills — `print-layout`](https://github.com/SkillMedev/skills/blob/main/skills/print-layout/SKILL.md) | **12** | 2026-07-05 | ~1,130 words, no references, no scripts. Genuinely competent: 3 mm bleed, 3–5 mm safe zone, 300 DPI **at placed size**, ICC by press (SWOP / FOGRA39), *"Rich black C60 M40 Y40 K100 for large areas; pure K100 for small text and thin rules, so misregistration cannot blur them"*, ~300% ink limit, no hairlines under 0.25 pt, PDF/X. Opens with a real thesis: *"Print failures are paid for twice"* | **MEDIOCRE — best of the print lot.** Correct and well-written, but a one-page checklist with zero tooling |
| [heymoezy/porter — `print-designer`](https://github.com/heymoezy/porter/blob/main/skills/print-designer/SKILL.md) | **0** | 2026-09-10 | ~823 words; inputs to pin down (trim, folds, spine width, panel order, stock, coating, viewing distance, dieline) | **MEDIOCRE** — sensible scaffolding, zero adoption |
| [zeke/swiss-design-skill](https://github.com/zeke/swiss-design-skill) | **140** | 2026-06-29 | 12,998 B SKILL.md + `components.md` (11,265 B), `design-system.md`, `prompting.md`, **`tailwind-config.md`**. Six principles (12-col grid / 8 px base, opacity-not-hue hierarchy, one accent, `max-w-[60ch]`). Curated Müller-Brockmann / Hofmann / Ruder / Lohse / Odermatt corpus | **EXCELLENT of its kind — DISQUALIFIED for you.** IBM Plex + Tailwind utility classes end to end. `text-7xl font-light tracking-tight` is not a grid system; it is a web theme wearing Müller-Brockmann's name |
| [cathrynlavery/diagram-design](https://github.com/cathrynlavery/diagram-design) | **38,463** | **2026-09-10** | 40 editorial diagram types as self-contained HTML+SVG, v2.6. Brand-token onboarding gate, `references/profiles.md`, CI with visual-proof PNGs, 49 KB README. *"No shadows. No Mermaid slop."* | **EXCELLENT for diagrams** — the most professionally maintained design skill on GitHub. Diagrams only; no page layout, no type system |
| [bergside/awesome-design-skills — `editorial`](https://github.com/bergside/awesome-design-skills/blob/main/skills/editorial/SKILL.md) | 2,751 | 2026-06-28 | **479 words**; siblings named `power`, `gradient`, `colorful`, `agentic` | **JUNK** — 67 vibes files; "editorial" means a screen aesthetic |
| [HermeticOrmus/design-mastery-claude-code](https://github.com/HermeticOrmus/design-mastery-claude-code) | 15 | 2026-05-25 | `design-masters`, 1,392 words — Saul Bass and Dieter Rams as personas | **JUNK** — name-dropping, no method |
| [julianoczkowski/designer-skills](https://github.com/julianoczkowski/designer-skills) | 551 | 2026-07-06 | Web prototyping | Out of scope |
| [alchaincyf/huashu-design](https://github.com/alchaincyf/huashu-design) | **24,066** | 2026-08-25 | "HTML-native design skill, 20 design philosophies" | Out of scope — HTML slides |

**The awesome-lists, checked directly:**

| List | ★ | Pushed | Adobe/print/typography entries |
| :-- | --: | :-- | :-- |
| [travisvn/awesome-claude-skills](https://github.com/travisvn/awesome-claude-skills) | 15,037 | **2026-04-28 — abandoned 4½ months, 806 open issues** | Grepped for `adobe\|indesign\|photoshop\|illustrator\|typograph\|swiss\|editorial\|print\|grid\|extendscript\|uxp`: **one line**, Anthropic's `brand-guidelines` |
| [VoltAgent/awesome-agent-skills](https://github.com/VoltAgent/awesome-agent-skills) | 34,108 | 2026-09-07 | Same grep: **three lines** — `brand-guidelines`, `dembrandt` (UX), `talkstream/ru-text` (Russian typography rules). **Zero Adobe entries in a "1000+ skill" list** |
| [hesreallyhim/awesome-claude-code](https://github.com/hesreallyhim/awesome-claude-code) | 53,898 | 2026-09-11 | The only hand-written list with real editorial judgment: surfaces Flue, diagram-design, motion-skills. Carries **no** Adobe scripting or print-design skill |

### [7.9] What to actually take

1. **[kevinkiklee/adobe-plugin-skills](https://github.com/kevinkiklee/adobe-plugin-skills)** — for Photoshop UXP/batchPlay. Genuinely excellent. **Fork it**, and re-verify its 2026-07 baseline against [08].
2. **[github/awesome-copilot `adobe-illustrator-scripting`](https://github.com/github/awesome-copilot/tree/main/skills/adobe-illustrator-scripting)** — vendor the directory.
3. **[MoebiusSt/indesign-scripting-mcp](https://github.com/MoebiusSt/indesign-scripting-mcp)** — steal the *inspect → lookup → execute-with-`undo_name` → verify → rollback* contract and the script-label hygiene policy. That pattern is the correct shape for any InDesign agent, on any platform.
4. **[cathrynlavery/diagram-design](https://github.com/cathrynlavery/diagram-design)** — not your subject, but the only design skill maintained to a professional engineering standard. Copy its structure: brand-token gate, profile files, CI visual proofs.
5. **[obra/superpowers `writing-skills`](https://github.com/obra/superpowers)** — as an authoring template, which is exactly how indesign-cli uses it.

**Your macOS-first constraint eliminates zhanglongxiao111/indesign-cli and MoebiusSt/indesign-scripting-mcp as runnable tools** (both Windows COM). That leaves Photoshop UXP and Illustrator ExtendScript covered by two forks, and **InDesign, GREP, print typography, grid systems, and architectural presentation graphics uncovered by anything at all.** Those are yours to write.

---

## [08] — DOCUMENTATION SOURCES TO FEED AGENTS

Liveness and dates verified 2026-09-11 (HTTP status via `curl -L`, repo dates via the GitHub API, extension metadata via the VS Code Marketplace gallery API).

### [8.1] Four findings that change the shape of the answer

1. **Adobe shipped a consolidated UXP Hub** at **<https://developer.adobe.com/uxp/>** (HTTP 200). Its source repo `AdobeDocs/uxp-hub` was **pushed 2026-09-11**. Adobe staff confirmed the intent: *"The plan is to bring UXP-specific content into the UXP docs website, including UXP specific guides that apply across host apps, API references, changelog and version matrix"* — <https://forums.creativeclouddeveloper.com/t/announcement-uxp-changelog-and-product-support-matrix/12032> (2026-07-31). **This is the new canonical root. Start here, not at the per-app `/2022/` paths.**
2. **No OpenAPI, JSON, or `llms.txt` exists** for the UXP docs. `https://developer.adobe.com/uxp/llms.txt` → **404**. The machine-readable substitute is the **markdown source in `AdobeDocs/uxp-photoshop`** (`src/pages/ps-reference/classes/*.md`) plus **`@types/photoshop` on npm**.
3. **Two well-known community sites are gone.** `kahrel.plus.com` **does not resolve (NXDOMAIN)**; content moved to CreativePro. `tomaxxi.com` **has been taken over by a gambling spam site** — do not feed it to an agent.
4. **ExtendScript is not deprecated with a date.** Adobe's own words: *"There are currently no plans to remove ExtendScript support from Photoshop."*

### [8.2] Official Adobe — UXP core

| Source | URL | Covers | Currency | Verdict |
| :-- | :-- | :-- | :-- | :-- |
| **UXP Hub (new)** | <https://developer.adobe.com/uxp/> | Cross-host UXP: panels vs commands, manifest, guides, migration centre, host selector | repo pushed **2026-09-11** | **EXCELLENT — feed first** |
| Photoshop UXP root | <https://developer.adobe.com/photoshop/uxp/2022/> | Plugin + scripting quickstarts, packaging | repo pushed **2026-08-14** | **EXCELLENT** |
| **Photoshop DOM ref (`ps_reference`)** | <https://developer.adobe.com/photoshop/uxp/2022/ps_reference/> (301 → `/ps-reference/`; unversioned path also 200) | `app`, Document, Layer, Action, `executeAsModal`, MIN VERSION tags | 2026-08-14 | **EXCELLENT** |
| **batchPlay reference** | <https://developer.adobe.com/photoshop/uxp/2022/ps_reference/media/batchplay/> | actionJSON, `_obj`/`_target`/`_ref`/`_enum`, **Copy As JavaScript**, options | 2026-08-14 | **EXCELLENT** — densest page for agent codegen |
| UXP Scripting (`.psjs`) | <https://developer.adobe.com/photoshop/uxp/scripting/> | Single-file scripts, ES6 vs ES3 | 2026-05 | **EXCELLENT** |
| Manifest v5 | <https://developer.adobe.com/photoshop/uxp/2022/guides/uxp_guide/uxp-misc/manifest-v5/> | Schema, entrypoints, permissions | 2026 | **EXCELLENT** |
| **UXP source markdown** | <https://github.com/AdobeDocs/uxp-photoshop> → `src/pages/ps-reference/classes/*.md` | The same reference as raw `.md`, one file per class, plus `changelog/index.md` | pushed **2026-08-14**, 143 ★ | **EXCELLENT — best raw-corpus form** |
| Static mirror | <https://adobedocs.github.io/uxp-photoshop/> | GH Pages build | mirrors main | GOOD (fallback) |
| **UXP changelog + support matrix** | <https://blog.developer.adobe.com/en/publish/2026/07/uxp-changelog-and-support-matrix> | UXP 9.0→9.4, host ↔ UXP version matrix | **2026-07-29** | **EXCELLENT — the only current matrix** |
| Legacy versions table | <https://developer.adobe.com/xd/uxp/uxp/versions/> | UXP v5.5–v6.3 ↔ hosts | **stale, stops at UXP 6.3 / PS 24** | **SKIP** — actively misleading about Illustrator |
| **Photoshop extensibility landing** | <https://adobedocs.github.io/photoshop/> | Decision table: UXP Scripts / Plugins / Hybrid / Photoshop API / ExtendScript / CEP / C++ SDK / Generator, with min versions and file extensions | current | **EXCELLENT — best single orientation table** |

### [8.3] Official Adobe — InDesign

| Source | URL | Covers | Currency | Verdict |
| :-- | :-- | :-- | :-- | :-- |
| **UXP for InDesign** | <https://developer.adobe.com/indesign/uxp/> | Scripts vs plugins, recipes, references | 2026-05 | **EXCELLENT** |
| UXPScript (`.idjs`) | <https://developer.adobe.com/indesign/uxp/scripts/> | InDesign 18.0+ UXP scripts | current | **EXCELLENT** |
| **ExtendScript → UXP migration guide** | <https://developer.adobe.com/indesign/uxp/resources/migration-guides/extendscript/> | ES3→ES6 gaps, `==`/`instanceof` on DOM objects, `app.activeScript` | **2026-05-13** | **EXCELLENT** |
| **Object Model Viewer** | <https://developer.adobe.com/indesign/uxp/dom/api/> | Full ExtendScript DOM. Adobe: *"As of InDesign 18.0, ESTK no longer reliably connects to InDesign, so we have provided the OMV references here"* | live | **EXCELLENT but JS-rendered** — poor to scrape. Use indesignjs.de instead |
| Legacy scripting-guide path | `developer.adobe.com/indesign/1688653963277/guides/object-modal/` | — | **DEAD (404)** | **SKIP** |
| Adobe devnet PDFs | `adobe.com/devnet/indesign/documentation.html` | — | **DEAD (404)** | **SKIP** — the classic Scripting Guide / Tutorial PDFs are no longer published at any stable Adobe URL; only third-party rehosts remain |
| End-user scripting help | <https://helpx.adobe.com/indesign/using/scripting.html> (301 → `automate-workflows-with-scripts.html`) | Scripts panel, script locations | **2026-08-31** | GOOD (not an API ref) |

### [8.4] Official Adobe — Illustrator (ExtendScript only)

| Source | URL | Status | Verdict |
| :-- | :-- | :-- | :-- |
| Illustrator dev landing | <https://developer.adobe.com/illustrator/> | Stale marketing, **no UXP mention** | DATED-BUT-CANONICAL |
| `developer.adobe.com/illustrator/uxp/` | — | **404 — does not exist** | **SKIP** |
| `developer.adobe.com/illustrator/docs/` | — | **404** | **SKIP** |
| Official PDFs (*Scripting Guide*, *Scripting Reference: JavaScript*) | gated behind <https://developer.adobe.com/console/> | Newest publicly-cited Adobe edition is the **CC 2017 / v24 (CC 2020)** era | **DATED-BUT-CANONICAL** — do not chase; the community HTML port is strictly better |
| Third-party rehost | <https://illustratorscripts.com/wp-content/uploads/2023/01/Illustrator-Scripting-Guide.pdf> | Full guide, 2023 rehost | GOOD (fallback) |

**The de facto official Illustrator reference is the community port** ([8.7]).

### [8.5] Distribution, tooling, samples

| Source | URL | Covers | Currency | Verdict |
| :-- | :-- | :-- | :-- | :-- |
| `developer.adobe.com/distribution` | — | — | **404 — wrong URL** | **SKIP** |
| **Adobe Developer Distribution (correct)** | <https://developer.adobe.com/developer-distribution/creative-cloud/docs/guides/> | Self-service portal for UXP **and** ZXP/CEP/MXI → Creative Cloud Marketplace / Exchange; publisher profile, review, EU trader rules (deadline 2025-02-16) | current | **EXCELLENT** |
| Submission flow | <https://developer.adobe.com/developer-distribution/creative-cloud/docs/guides/submission/how-submit> | Preview and submit, auto-publish | current | GOOD |
| **UXP Developer Tool (UDT)** | <https://developer.adobe.com/photoshop/uxp/2022/guides/devtool/> · walkthrough <https://developer.adobe.com/photoshop/uxp/2022/guides/devtool/udt-walkthrough/> | Create/load/debug/package. Installed from the **Creative Cloud desktop app** — no standalone installer; needs elevated privileges | **2026-07-08** | **EXCELLENT** |
| **Alchemist** | <https://github.com/jardicc/alchemist> | ScriptListener for UXP: inspect and record action descriptors, generate batchPlay code | **last commit 2026-05-17**; last release 2.7.0 (2023-09-07); 248 ★ | **EXCELLENT** (tool). This is what you use to crack `newBrushGroup` in [5.5] |
| **Photoshop plugin samples** | <https://github.com/AdobeDocs/uxp-photoshop-plugin-samples> | Runnable UXP plugins, PS 22+ | pushed **2026-08-18**, 357 ★ | **EXCELLENT** |
| `adobe/uxp-photoshop-plugin-samples` | — | **404 — wrong org**, it is `AdobeDocs` | — | **SKIP** |
| InDesign UXP samples | <https://github.com/AdobeDocs/uxp-indesign-samples> | ID plugins/scripts | pushed 2023-10-09, 30 ★ | GOOD (stale but valid) |
| **CEP-Resources** | <https://github.com/Adobe-CEP/CEP-Resources> | CEP 8–12 cookbooks, Debugging Handbook, **and the Photoshop ExtendScript PDFs** | pushed **2026-02-20**, 1,839 ★ | **DATED-BUT-CANONICAL** |
| **Photoshop ExtendScript DOM reference — still published** | [`photoshop-javascript-ref-2020.pdf`](https://github.com/Adobe-CEP/CEP-Resources/blob/master/Documentation/Product%20specific%20Documentation/Photoshop%20Scripting/photoshop-javascript-ref-2020.pdf) (+ `photoshop-scripting-guide-2020.pdf`, `-vbs-ref-`, `-applescript-ref-`) | Complete PS ExtendScript object model | **frozen at 2020** — no newer edition exists anywhere | **DATED-BUT-CANONICAL.** This is the answer to "is the PS DOM reference still published": yes, in the CEP repo. Only Photoshop got PDFs preserved here, not AI or ID |
| **ExtendScript Debugger (VS Code)** | <https://marketplace.visualstudio.com/items?itemName=Adobe.extendscript-debug> | Breakpoints, eval, `#target` across AE/AU/AI/ID/PR/PS/IDS | publisher **Adobe**, **v2.1.0**, updated **2025-08-08**, 147,719 installs | **EXCELLENT — the ESTK replacement** |
| **ESTK status** | <https://adobedocs.github.io/uxp-photoshop/guides/legacy-extensibility/> | *"the ExtendScript Toolkit IDE is no longer maintained due to its reliance on 32-bit architectures"* | current | canonical citation |
| Adobe Developers Blog | <https://blog.developer.adobe.com/> | UXP-Hybrid for Premiere (2026-04-01), PS UI backend change (2026-06-25), UXP changelog (2026-07-29) | active, cadence up in 2026 | **EXCELLENT for deltas** |
| Photoshop cloud API (different thing) | <https://developer.adobe.com/firefly-services/docs/photoshop/> | REST PSD editing, Actions API — **not** in-app scripting | current | GOOD — don't confuse with UXP |

### [8.6] ExtendScript vs UXP — verified 2026 status

**Do not tell an agent ExtendScript is deprecated with an end date. It is not.**

| App | ExtendScript `.jsx` | CEP `.zxp` | UXP plugins | UXP scripts | Guidance |
| :-- | :-- | :-- | :-- | :-- | :-- |
| **Photoshop** | **Fully supported, no removal planned.** Adobe: *"The UXP Photoshop DOM API aims to be the evolution of the ExtendScript DOM. There are currently no plans to remove ExtendScript support from Photoshop"* — <https://adobedocs.github.io/uxp-photoshop/guides/legacy-extensibility/> | Supported but frozen; broken on ARM Macs without Rosetta | ✅ since 22.0 | ✅ `.psjs` since 23.5 | **Write UXP.** ExtendScript for legacy maintenance only |
| **InDesign / InCopy / ID Server** | **Fully supported**, and the ExtendScript DOM is still *richer* than UXP's | ✅ | ✅ since 17.0 | ✅ `.idjs` since 18.0 | **Split brain** — UXP for panels, ExtendScript where UXP DOM coverage is missing |
| **Illustrator** | **The only public option** | **The only public panel option** | ❌ **not public** — the runtime exists for Adobe's own features; no public API, no docs, no date. Adobe staff confirmation: <https://community.adobe.com/questions-652/clarification-needed-is-uxp-publicly-available-for-illustrator-in-2026-1548811> (2026-02-06) | ❌ | **Always ExtendScript** |
| **Premiere Pro** | ✅ | EOL | ✅ **GA since 25.6** (2025-12-01); UXP-Hybrid C++ since 26.2 (2026-04-01) | — | UXP |

- **CEP is end-of-life.** CEP 12 is the last release (Coppieters, Sept 2025); `Adobe-CEP/CEP-Resources` is maintenance-only.
- **Consequence for a knowledge base:** ExtendScript-only sources are **not obsolete** — they are the *only* coverage for Illustrator and for large parts of InDesign. Tag them `extendscript`, not `deprecated`.

### [8.7] Community references

| Person / site | URL | Covers | Last activity | Verdict |
| :-- | :-- | :-- | :-- | :-- |
| **Marc Autret — Indiscripts** | <https://www.indiscripts.com/> | Deepest ExtendScript writing anywhere: lexical scope/engines (2026-07-04), operator overloading (2026-02-17), Scripting Forum Roundup #14 (2026-04-21) | **2026-07-04** | **EXCELLENT** (ExtendScript only) |
| **IdExtenso** (Autret) | <https://github.com/indiscripts/IdExtenso> | Full ExtendScript framework for InDesign, v2.6 | pushed **2026-05-04**, 147 ★ | **EXCELLENT** |
| **Peter Kahrel — old site** | `kahrel.plus.com` | — | **DEAD — DNS NXDOMAIN** | **SKIP / BLOCKLIST** |
| **Kahrel scripts (new home)** | <https://creativepro.com/files/kahrel/indesignscripts.html> · <https://creativepro.com/indesignscripts/> | 100+ free ID scripts; per-script changelogs dated **2026-03-02**, 2025-10-18, 2025-05-27 | **2026-09-03** | **EXCELLENT** |
| CreativePro GREP topic | <https://creativepro.com/topic/grep/> | GREP tutorials | live | **EXCELLENT** — GREP is the best-documented InDesign niche |
| **Gregor Fellenz — ExtendScript API** | <https://www.indesignjs.de/extendscriptAPI/indesign-latest/> | **"Adobe InDesign 2026 (21.x) Object Model"** — searchable WebHelp generated from Adobe's own sources. He states he rebuilt it because jongware stopped at CS6 (<https://www.indesignjs.de/extendscriptAPI/indesign-latest/about.html>). Versioned: `/indesign20/` = ID 2025 | **ID 2026** | **EXCELLENT — best static, scrapeable ID DOM reference** |
| Generator behind it | <https://github.com/grefel/extendscriptApiDocTransformations> | DITA/XSLT pipeline | pushed **2026-09-11**, 48 ★ | GOOD |
| **grefel InDesign MCP server** | <https://github.com/grefel/indesign-scripting-mcp> | DOM lookup + ExtendScript execution via COM/OLE | pushed **2026-02-17** | **EXCELLENT for agents** (Windows-only) |
| **Kris Coppieters — "It's All JavaScript To Me"** | <https://coppieters.nz/its-all-javascript-to-me/> | The clearest ExtendScript/CEP/UXP/UXPScript taxonomy + per-app support matrix; `.jsx`/`.idjs`/`.psjs`; *"CEP is end-of-life, and the current release, CEP 12, will be the last release"*; CEP needs Rosetta on ARM Macs | stated accurate as of **Sept 2025** | **EXCELLENT — best orientation doc in the whole list** |
| Rorohiko | <https://rorohiko.com/> (403 to curl; renders fine) | Commercial ID tools (CRDT, APID ToolAssistant) | live | GOOD |
| **Josh Duncan** | <https://joshbduncan.com/> · [illustrator-scripts](https://github.com/joshbduncan/illustrator-scripts) (2026-04-09, 45 ★) · [AiCommandPalette](https://github.com/joshbduncan/AiCommandPalette) (2026-06-28, 64 ★) · [PsCommandPalette](https://github.com/joshbduncan/PsCommandPalette) (2026-05-14) | Idiomatic modern Illustrator ExtendScript + real UI patterns | **2026-06-28** | **EXCELLENT** |
| **Sergey Osokin (creold)** | <https://github.com/creold/illustrator-scripts> | 1,111 ★, the largest actively-maintained AI script corpus | pushed **2026-08-21** | **EXCELLENT — best few-shot corpus for Illustrator** |
| creold Photoshop scripts | <https://github.com/creold/photoshop-scripts> | PS JSX | 2025-05-01, 144 ★ | GOOD |
| **Alexander Ladygin** | <https://github.com/alexander-ladygin/illustrator-scripts> | 1,498 ★ AI scripts, MIT | **2023-09-04 — unmaintained** | GOOD (dated); prefer creold |
| **"Chris Bendtsen"** | — | **No Adobe-scripting figure by this name exists.** Likely a conflation with **Chris Cox** (ex-Adobe Photoshop senior computer scientist 1996–2016 — forum presence only, no docs site) | — | **SKIP** |
| **Davide Barranca** | <https://www.davidebarranca.com/> · <https://developer.adobe.com/developer-champion/davide-barranca> (lastmod 2026-09-01) | CEP-HTML5 archive + the definitive UXP series (BatchPlay with Alchemist, modal dialogs, flyout menus, Spectrum, React). Now a Sr. Developer Relations Engineer at Adobe; author of *Professional Photoshop UXP* | site ~2024-08; **now authoring on the Adobe blog (2026-04-01)** | **EXCELLENT** (CEP parts DATED-BUT-CANONICAL) |
| **Marijan Tompa (tomaxxi)** | `tomaxxi.com` | **Domain hijacked — now an Indonesian gambling site** | — | **SKIP — BLOCKLIST AS HOSTILE** |
| **jongware object model** | `jongware.mit.edu`, `jongware.com/idjshelp.html` | — | **DEAD** (author deceased) | **SKIP** — successors: indesignjs.de, lohriialo, autodtp |
| Ajar Productions ID object model | <https://ajarproductions.com/pages/products/indesign_object_model/> | Mirror | returns **406** to automated fetch (bot-blocked) | **SKIP for agents** |
| **lohriialo mirrors** | <https://lohriialo.github.io/Adobe-InDesign-Scripting-API-Reference-21.0/> (also 20.0 … 9.0, plus ID **Server** 21.0) | Per-version ID ExtendScript API, static HTML | pushed **2026-01-08** | **EXCELLENT — cleanest version-pinned static HTML for diffing the DOM across releases** |
| **docsforadobe directory** | <https://docsforadobe.dev/> | Index of all community ports | repo pushed 2026-06-09 | **EXCELLENT** |
| **Illustrator Scripting Guide (community port)** | <https://ai-scripting.docsforadobe.dev/> · repo <https://github.com/docsforadobe/illustrator-scripting-guide> | ~120 classes, `jsobjref/`, scripting constants, offline `/print_page/` | repo pushed **2025-03-20**, 76 ★; content tracks **Illustrator 24 (CC 2020)** | **EXCELLENT — the best Illustrator reference that exists**, despite the 2020 API cutoff |
| **JavaScript Tools Guide** | <https://extendscript.docsforadobe.dev/> · repo <https://github.com/docsforadobe/javascript-tools-guide> | ScriptUI, File/Folder, `$`, BridgeTalk — applies to **every** ExtendScript host | repo pushed **2026-01-23**, 51 ★ | **EXCELLENT** |
| `indesign-scripting.docsforadobe.dev` | — | **does not exist (no DNS)** | — | **SKIP** — use indesignjs.de / lohriialo |
| `ps-scripting.docsforadobe.dev` | — | **does not exist** | — | **SKIP** — use the 2020 PDF + `@types/photoshop` |
| **Types-for-Adobe** | <https://github.com/docsforadobe/Types-for-Adobe> · npm `types-for-adobe` **7.2.6** (2026-09-04) | TS typings for AE/AI/ID/PS/PR ExtendScript DOMs | pushed **2026-05-18**, 644 ★ | **EXCELLENT for agents — typed signatures beat prose** |
| **`@types/photoshop`** | <https://www.npmjs.com/package/@types/photoshop> | **UXP** Photoshop DOM typings, **v25.0.4, 2025-08-03** | current | **EXCELLENT for agents** |
| **Adobe Developer Community Resources** | <https://github.com/AdobeDeveloperCommunity/creative-cloud-developer-resources> | Adobe-blessed, Champion-curated link index — Adobe's explicit workaround for not linking community content from official docs | pushed **2026-08-24**, 13 ★ | **EXCELLENT — use as a crawl seed** |

### [8.8] Forums actually alive in 2026

| Forum | URL | Verified activity | Verdict |
| :-- | :-- | :-- | :-- |
| **Adobe Creative Cloud Developer Forums** (Discourse) | <https://forums.creativeclouddeveloper.com/> | Adobe staff answering; announcement thread **2026-07-31** | **EXCELLENT — the primary UXP venue** |
| Adobe Community — Photoshop | <https://community.adobe.com/photoshop-709> (old `/t5/` paths 301 here) | live | GOOD |
| Adobe Community — Illustrator | <https://community.adobe.com/p/illustrator> | live | GOOD |
| Adobe Community — InDesign | <https://community.adobe.com/indesign-669> | live | GOOD |
| **PS-Scripts.com** (phpBB) | <https://www.ps-scripts.com/> | live; newest real post **2026-08-03**; some spam; several boards dead since 2022/2024 | **DATED-BUT-CANONICAL** — great archive of ScriptListener-era tricks, low current traffic |
| "InDesign Talk" | `indesigntalk.com` | **no DNS response** | **SKIP** |

### [8.9] Recommended ingest set, ranked

1. `AdobeDocs/uxp-photoshop` raw markdown (`src/pages/ps-reference/**`) + the batchPlay page + Manifest v5 + <https://developer.adobe.com/uxp/>
2. `@types/photoshop` (25.0.4) and `types-for-adobe` (7.2.6) — typed signatures, highest token efficiency
3. `developer.adobe.com/indesign/uxp/**` + the ExtendScript→UXP migration guide + `indesignjs.de/extendscriptAPI/indesign-latest/` (or `lohriialo/…-21.0` for static HTML)
4. `ai-scripting.docsforadobe.dev` + `extendscript.docsforadobe.dev` — the mkdocs sources on GitHub are cleaner than the rendered sites
5. `photoshop-javascript-ref-2020.pdf` + `photoshop-scripting-guide-2020.pdf` from CEP-Resources
6. Few-shot corpora: `creold/illustrator-scripts`, `joshbduncan/illustrator-scripts`, `indiscripts/IdExtenso`, `creativepro.com/files/kahrel/`
7. Deltas: the Adobe Developers Blog UXP tag + `forums.creativeclouddeveloper.com` announcements
8. For the brush project specifically: `AdobeDocs/photoshop-cpp-sdk` → `PIStringTerminology.h`, and **Alchemist** to record what the header names but nobody has published

**Blocklist for any agent crawl:** `tomaxxi.com` (hijacked), `kahrel.plus.com` (NXDOMAIN), `jongware.*` (dead), `developer.adobe.com/distribution` (404), `adobe.com/devnet/*` (404), `ps-scripting.docsforadobe.dev` and `indesign-scripting.docsforadobe.dev` (never existed), `developer.adobe.com/xd/uxp/uxp/versions/` (stale and actively misleading about Illustrator).

---

## [09] — WHAT TO DO

1. **Cap `Brushes.psp` at ~150 MB.** Ship the entourage library as one `.abr` per group, imported per project. The 2 GB silent-failure cliff is real and undocumented.
2. **Brush presets, not tool presets.** Groups exist only for brush presets, and `Export Selected Brushes` on a group is the only interchange path Photoshop offers.
3. **Normalize on save: Spacing 1000%, Angle 0°, Roundness 100%, every dynamic off, Capture Brush Size ON, Include Tool Settings ON, Include Color OFF.**
4. **Adopt 180 px as the people default on the 3840 × 2160 reference canvas** (`cloud-libraries.md` [03.4] row [01]) and derive the rest from [4.4]. No vendor publishes a standard; you are writing one. Record the reference canvas in the library name.
5. **Build inside Photoshop.** No ABR writer exists for the modern format and the group block `phry` is undecoded by everyone. First experiment: record `newBrushGroup` in Alchemist. If it is unrecordable, fall back to one `.abr` per group and let Photoshop's auto-grouping on import do the work.
6. **Archive the Wayback copy of `creating-modifying-brushes.html`.** Adobe has already deleted the only complete Brush Settings reference from its live docs.
7. **Fork `kevinkiklee/adobe-plugin-skills`; vendor `awesome-copilot/adobe-illustrator-scripting`; copy MoebiusSt's inspect→execute→verify→rollback contract.** Everything else in the skills landscape is a stub, a scrape, or a Tailwind theme with a Swiss designer's name on it.

