# [PHOTOSHOP_BETA_27.11_STATE_PROBE]

Read-only probe, 2026-09-11. No document saved, no preference set, no preset imported, no write into any Adobe folder. Every write landed under `/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/photoshop/`.

## [00]-[HOST_IDENTITY]

Running process confirmed as the Beta build before any scripting call:

| [FACT] | [VALUE] |
| :----- | :------ |
| Process | PID 33823, `/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app/Contents/MacOS/Adobe Photoshop 2026` |
| `app.version` | `27.11.0` |
| `app.build` | `27.11.0 (20260909.m.3669 481f204)` |
| `app.path` | `/Applications/Adobe Photoshop (Beta)` |
| `app.preferencesFolder` | `/Users/bardiasamiee/Library/Preferences/Adobe Photoshop (Beta) Settings` |
| Bundle identifier | `com.adobe.Photoshop` (shared with the release build; only one is installed at a time in this tree) |
| Feature access level | `PublicBeta` |
| Launches | 11 |
| Machine | Apple M4 Max, 16 cores, 65536 MB, macOS 26.6.1, single 3600x2338 HDR display at scale 2 |
| Documents open | 0 |
| Fonts | 2041 faces, 465 families |
| Scripting bridge | `osascript` → `do javascript`, `app.displayDialogs = DialogModes.NO` |

Camera Raw plugin: `Camera Raw 18.6 (2698)` and `Camera Raw Filter 18.6 (2698)`, from `Camera Raw.plugin`.

## [01]-[PREFERENCES_STORE]

`~/Library/Preferences/Adobe Photoshop (Beta) Settings/`

| [SIZE] | [MTIME] | [FILE] |
| -----: | :------ | :----- |
| 106233 | 2026-09-11 16:11 | `Actions Palette.psp` |
| 25014 | 2026-09-10 22:12 | `Adjustments_Presets/presets.json` |
| 232522 | 2026-09-11 16:11 | `Adobe Photoshop (Beta) Prefs.psp` |
| 1236268 | 2026-09-10 22:55 | `Brushes.psp` |
| 3465468 | 2026-09-11 06:59 | `Color Settings.csf` |
| 2312463 | 2026-09-10 22:55 | `CustomShapes.psp` |
| 39367 | 2026-09-10 21:50 | `Default Type Styles.psp` |
| 3 | 2026-09-11 16:11 | `DialogPreferences.psp` (`{ }`) |
| 2865 | 2026-09-11 16:11 | `DVADialogPrefs/DVADialogPrefs.xml` |
| 64 | 2026-09-11 16:11 | `FavoriteFonts.psp` (empty list) |
| 1360 | 2026-09-11 18:15 | `FMCache.psp` (Floodgate feature flags) |
| 139245 | 2026-09-10 22:55 | `Gradients.psp` |
| 20 | 2026-09-11 18:15 | `LaunchEndFlag.psp` |
| 135755 | 2026-09-11 16:11 | `MachinePrefs.psp` |
| 1098 | 2026-09-11 16:11 | `MRU New Doc Sizes.json` |
| 193 | 2026-09-10 22:55 | `MRUSwatches.psp` |
| 104 | 2026-09-11 16:11 | `New Doc Sizes.json` |
| 8594532 | 2026-09-10 22:55 | `Patterns.psp` |
| 867142 | 2026-09-10 22:21 | `PluginCache.psp` |
| 24 | 2026-09-11 16:11 | `QuitEndFlag.psp` |
| 3273-3274 | 2026-09-11 01:23 → 18:15 | `sniffer-out.txt`, `sniffer-out1.txt` … `sniffer-out4.txt` |
| 10812911 | 2026-09-10 22:55 | `Styles.psp` |
| 10809 | 2026-09-10 22:55 | `Swatches.psp` |
| 6285 | 2026-09-11 06:59 | `UIPrefs.psp` |
| 43322 | 2026-09-11 16:11 | `Workspace Prefs.psp` |
| 43199 | 2026-09-11 01:14 | `WorkSpaces/Editorial Images.psw` |
| 43199 | 2026-09-11 06:59 | `WorkSpaces (Modified)/Editorial Images.psw` |
| 43199 | 2026-09-11 01:14 | `WorkSpaces (Modified)/Essentials.psw` |

Notes on the small files:

- `New Doc Sizes.json` holds `sections: [{section: "user", presets: []}]` — **zero user New Document presets**.
- `MRU New Doc Sizes.json` holds two entries: a `Custom` 6x4 in, 300 ppi, 16-bit, sRGB IEC61966-2.1, transparent fill; and the stock `Default Photoshop Size` 7x5 in, 300 ppi, 8-bit, white.
- `DVADialogPrefs.xml` records three remembered dialog positions (`com.adobe.photoshop.inAppMessaging_dialog-1`, `com.adobe.unifiedpanel_Template-Assisted-CCN-…`, `new_layer_dialog`) and one monitor at `(0, 39) → (1800, 1082)` points.
- `FavoriteFonts.psp` contains an empty `favoriteFontList` / `fontPostScriptNameVlLs`.

`strings` on the two large preference binaries yields key *names* only — the values are binary descriptor payloads. Dumps: `strings-Adobe_Photoshop_(Beta)_Prefs.txt` (12091 lines), `strings-Workspace_Prefs.txt` (175), `strings-UIPrefs.txt` (250), `strings-MachinePrefs.txt` (2083). The authoritative values come from the Action Manager read in section 02.

`UIPrefs.psp` key names of interest: `uiLanguageKey`, `eyeDropperSampleSheet`, `eyeDropperHUD`, `defaultCloudSave`, `autoShowHomeScreen`, `showHomeAtLaunch`, `extensionsOn`, `RlrU` (ruler units), `typeUnits`, `newDocPresetPrintResolution`, `newDocPresetPrintScale`, `newDocPresetScreenResolution`, `newDocPresetScreenScale`, `cursorShape`, `cursorBrushTipOutlineStrokeWidth`, `cursorCrosshair`, `cursorCrosshairWhileStroking`, `cursorHideStroke`, `cursorStrokeRope`, `cursorStrokeRopeColor`, `cameraRaw`, `preferACRForHDRToning`, `EXIF`, `ignoreRotationMetadata`, `askLayeredTIFF`, `clipboardTIFFTransparencyPref`, `disablePSDCompression`, `maximizeCompatibility`, `FileSaveToOriginalFolder`, `fileSaveInBackground`, `fileAutoSaveEnabled`, `fileAutoSaveInterval`, `fileLegacySaveAs`, `fileDoNotAppendCopy`, `canvasBackgroundColors`, `showHUD`, `overscrollEnabled`, `useRichToolTips`, `useRichToolTipsRestore`.

`MachinePrefs.psp` key names of interest: `quietMode`, `showAIAssistedButton`, `NumLaunches`, `PhotoshopCrashed`, `SnifferCrashes`, `gpuPrefs64`, `openglEnabled`, `openglAdvanced`, `openglVBLSyncEnabled`, `openglAdvAntiAliasEnabled`, `openglBilerpEnabled`, `CLCompute`, `gpuCompositingChecked`, `deepMonitor`, `npuEnabled`, `compCoreGPUEnabled2`, `compCoreThreadsEnabled`, `mtPsdReadEnabled`, `mtPsdReadPeakThreads`, `tileSize64`, `cloudWorkareaCustom`, `generatorEnabled`, `exportExportAsLegacy`, `expFeatureDeepUpscale`, `expFeatureContentAwareTracing`, `expFeatureNativeAvifJpegXl`, `expFeatureUniformDepth16Bit`, `expFeatureChalkboard2`, `expFeaturePropertyFeedback`, `FGCompositeCache`, `DroverUI`, `HasInteractedWithCoediting`, `dnsaLowScratch`.

`FMCache.psp` holds the live Floodgate flag set for `Key_27_11_0`: `ChalkboardEnableSidecarBackendFinal2`, `ChalkboardFeatureRenameToCreativeCollaboration`, `ChalkboardForceCanCreateNewDocumentsOff`, `ChalkboardGenericContainerResource`, `ChalkboardPref`, `FGCompositeCache`, `FloodgateFeatureFlagTest`, `ps_186213_bug_fix`, plus `firstLaunchAfterPrefsReset`.

`Color Settings.csf` (3.4 MB) carries the embedded description:

> Shared Adobe RGB and GRACoL 2013 configuration. Preserve embedded profiles; Adobe ACE, Relative Colorimetric and Black Point Compensation.

This is **not** the Photoshop factory default (factory is sRGB working RGB with U.S. Web Coated CMYK). It is a deliberately configured or CC-synced set.

### [WORKSPACES]

Three `.psw` files, all 43199 bytes:

| [PATH] | [MD5] |
| :----- | :---- |
| `WorkSpaces/Editorial Images.psw` | `51cbe68a0df7959e99b253bde523bb6d` |
| `WorkSpaces (Modified)/Essentials.psw` | `51cbe68a0df7959e99b253bde523bb6d` |
| `WorkSpaces (Modified)/Editorial Images.psw` | `90d8904562672c6c5505b1392c339ed8` |

The saved `Editorial Images` is byte-identical to the modified `Essentials`: the user saved the then-current Essentials layout under a new name at 01:14, then kept editing it (modified copy at 06:59). `WorkSpaces/` holds the saved baseline, `WorkSpaces (Modified)/` the live deltas. `Workspace Prefs.psp` holds the currently-applied layout (control bar width 1783, matching the modified Editorial Images).

`app.workspaceList` reports one user workspace and seven Adobe ones:

| [NAME] | [USER] |
| :----- | :----- |
| Core Tools | false |
| **Editorial Images** | **true** |
| Essentials | false |
| Motion | false |
| Painting | false |
| Photography | false |
| Graphic and Web | false |
| Start | false |

Factory `.psw` files sit at `/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app/Contents/Required/Workspaces/`: `Core Tools.psw`, `Core Tools_V2.psw`, `Graphic and Web.psw`, `Motion.psw`, `Painting.psw`, `Photography.psw`, `Assisted.psw`, `Start.psw`.

Decoded panel roster of `Editorial Images` (61 entries; full list in `workspace-panels.txt`, dock tree in `workspace-dock-tree.txt`). Docks:

- **top** — control bar (`panelid.static.options`).
- **left** — toolbar (`panelid.static.toolbar`), `vertical-narrow`.
- **bottom** — Timeline (`panelid.static.animation`), collapsed group with Measurement Log.
- **right, first tab-pane** — Actions + History + Version History + Navigator + Histogram (collapsed group), then a large collapsed group of Brush Settings, Brushes, Styles, Shapes, Clone Source, Character, Paragraph, Paragraph Styles, Character Styles, Glyphs, Layer Comps, Notes, Tool Presets; then open groups for `uxp:com.tk.multimask/tkmultimaskv9` (**TK9 Multi-Mask**), `uxp:com.tk.comboV8/tkcombocxv9` (**TK9 Combo**), minimized `uxp:com.tk.export/tkexportv9` (**TK9 Export**) and minimized `uxp:com.tk.myactionsV8/tkmyactions` (**TK9 My Actions**).
- **right, second tab-pane** — Color/Swatches/Gradients/Patterns group, Properties/Adjustments/Libraries group, Layers/Channels/Paths group.

Live `app.panelList` visibility at probe time: **visible** = Tools, Move Options, Color, Properties, Layers, TK9 Combo, TK9 Multi-Mask. Everything else hidden, including all four `TK9 My Actions-Tab 1..4` panels (which report as bare names `1`, `2`, `3`, `4`).

### [RESIDENT_PRESETS]

Group hierarchy parsed out of the `8BIMphry` block of each `.psp` (`phry.py`, output in `preset-groups.txt`). Counts cross-check exactly against the Action Manager preset-manager read.

| [STORE] | [SIZE] | [GROUPS] | [TOTAL] |
| :------ | -----: | :------- | ------: |
| `Brushes.psp` | 1236268 | General Brushes (8), Dry Media Brushes (6), Wet Media Brushes (6), Special Effects Brushes (8) | 28 |
| `Swatches.psp` | 10809 | RGB (6), CMYK (6), Grayscale (20), Pastel (16), Light (16), Pure (16), Dark (16), Darker (16), Pale (10) | 122 |
| `Gradients.psp` | 139245 | Basics (3), Blues (32), Purples (23), Pinks (17), Reds (7), Oranges (13), Greens (31), Grays (14), Cloud (8), Iridescent (22), Pastels (9), Neutrals (4) | 183 |
| `Styles.psp` | 10812911 | Basics (2), Natural (9), Fur (5), Fabric (5) | 21 |
| `Patterns.psp` | 8594532 | Trees (4), Grass (3), Water (3) | 10 |
| `CustomShapes.psp` | 2312463 | Wild Animals (15), Leaf Trees (12), Boats (15), Flowers (12) | 54 |
| Contours (in-memory) | — | ungrouped | 12 |
| Tool presets (in-memory) | — | ungrouped | 21 |

**Every one of these is an Adobe factory group.** The brush names confirm it: `Soft Round`, `Hard Round`, the four pressure variants, then the Kyle Webster set that ships as the factory default (`KYLE Ultimate Pencil Hard`, `Kyle's Drawing Box - Happy HB`, `KYLE Ultimate Charcoal Pencil 25px Med2`, `KYLE Bonus Chunky Charcoal`, `Kyle's Ultimate Pastel Palooza`, `Kyle's Eraser - Natural Edge`, `KYLE Ultimate Inking Thick 'n Thin`, `Kyle's Inkbox - Classic Cartoonist`, `Kyle's Paintbox - Wet Blender 50`, `Kyle's Real Oils - 01`, `Kyle's Real Oils Round Flex Wet`, `Kyle's Impressionist Blender 1`, three `Kyle's Spatter Brushes`, three `Kyle's Concept Brushes`, `Kyle's Screentones 35` and `38`). No user-authored brush, swatch, gradient, style, pattern, shape, or tool preset is resident.

Tool presets, all 21 factory: `Healing Brush 21 pixels`, `Magnetic Lasso 24 pixels`, five Crop presets (4x6, 5x3, 5x4, 5x7, 8x10 at 300 ppi), `Fill with Bubbles Pattern`, `Peanut Dash`, `Horizontal Type Tool Myriad Pro Regular 24 pt`, `Vertical Type Tool Myriad Pro Regular 24 pt`, `Coupon with Halftone Fill`, `Stamp Edge`, `Computer Eye`, `5 Point White Star`, `Red Dwarf`, `0.5 cm Black Arrow`, `Starburst Color Target`, `Stitched Patchwork`, `Circular Rainbow`, `Background Eraser 30 pixels`.

Contours, all 12 factory: `Linear`, `Cone`, `Cone - Inverted`, `Cove - Deep`, `Cove - Shallow`, `Gaussian`, `Half Round`, `Ring`, `Ring - Double`, `Rolling Slope - Descending`, `Rounded Steps`, `Sawtooth 1`.

`Adjustments_Presets/presets.json` (25014 bytes) holds the Adjustments panel's shipped preset groups: `default-presets`, `group-Black_and_White`, `group-Cinematic`, `group-Creative`, `group-Landscape`, `group-PhotoRepair`, `group-Portraits`, `group-Ungrouped`. Factory.

### [ACTIONS]

`Actions Palette.psp` (106233 bytes) holds 7 sets, 62 actions. Enumerated live through `executeActionGet` on `ASet` / `Actn`:

| [#] | [SET] | [N] | [ORIGIN] |
| --: | :---- | --: | :------- |
| 1 | **TK9 actions** | 26 | TK9 / user |
| 2 | Basic Adjustments | 5 | Adobe default |
| 3 | Subject & Background | 5 | Adobe default |
| 4 | Creative Effects | 6 | Adobe default |
| 5 | Guides | 8 | Adobe default |
| 6 | Resize | 6 | Adobe default |
| 7 | Export | 6 | Adobe default |

`TK9 actions`: B and C Landscape, B and C Subject, B and C General, Zone Colour Grading, Tight Landscape, Tight Subject, Intersect Foreground, Intersect Sky, Intersect Subject, Intersect Background, Intersect Selection, Vignette No Darks, Mask The Mask, Modify Fill, Midtone Lighten, Midtone Contrast, Shadows Lighten, Colour Dodge Burn, Multiply Curve, Screen Curve, Lift Warm, Drop Cool, Drop Saturated, Paint Out Saturation, Lift Unsaturated, Close Panels. The UTF-16 step names inside the store reference `com.tk.multimask`, `com.tk.cxV8`, `TK9 Multi-Mask`, `TK9 Cx`, `closePanels`, `makeMidtonesMask`, `chooseChannelMask` — these actions drive the TK9 UXP panels through Plugin Action steps.

Sets 2–7 are the modern Adobe default action sets introduced with the 2025 line. A full-text scan of `/Applications/Adobe Photoshop (Beta)/` found no file containing `Make subject pop` or `Resize for YouTube thumbnail`, so they are not shipped as `.atn` files in the bundle — they are seeded into `Actions Palette.psp` by the app. The legacy `.atn` sets in `Presets/Actions` (Commands, Frames, Image Effects, …) are **not** loaded.

Also recorded: `app.recentFilesAsStrings` holds one entry, `/private/tmp/editorial-tooling/tk9-v4-setup/material-section-tk9.psd`. Active tool is `moveTool`. `toolBarVisible` is true, `panelUILockIsEnabled` is false.

## [02]-[PREFERENCE_VALUES]

Two reads: the ExtendScript `app.preferences` object (`preferences.json`) and the full Action Manager application descriptor (`application-descriptor.json`, 238 KB, 130 top-level keys). The descriptor is the richer source; the ExtendScript object omits many modern keys and raises `The requested property does not exist` for `showAsianTextOptions`, `iconPreview`, `fullSizePreview`, `macOSThumbnail`.

### [GENERAL]

```
colorPickerPrefs.pickerKind = photoshopPicker
colorPickerHUDMode          = hueStripSmallHUDColorPicker
interpolationMethod         = bicubicAutomatic
historyLog                  = false
showToolTips                = true
resizeWindowsOnZoom         = false
dynamicColorSliders         = true
shiftKeyToolSwitch          = true
saveHistoryTo               = metadata
editLogItems                = session
exportClipboard             = true
beepWhenDone                = false
autoShowHomeScreen          = false
extensionsOn                = true
autoUpdateFiles             = false
resizePastePlace            = false
placeRasterSmartObject      = true
skipTransformSOFromLibrary  = false
legacyPathDrag              = false
modernFreeTransform         = false
legacyFreeTransform         = false
promoteBackgroundLayer      = true
nonDestructiveBrushTool     = false
vectorSelectionModifiesLayerSelection = false
useClassicFileNewDialog     = false
```

`homeScreenVisibility = false` and `autoShowHomeScreen = false` — **the Home screen is off**; the app opens straight to the canvas area.

### [INTERFACE]

```
kuiBrightnessLevel          = kPanelBrightnessMediumGray
highlightColorOption        = uiBlueHighlightColor
paletteEnhancedFontTypeKey  = preferSmallPaletteFontType
paletteUIScaledTypeKey      = false
uiLanguageKey               = en_US
showMenuColors              = true
colorChannels               = false        (Channels in Color, off)
```

Canvas backgrounds, all four screen modes: color `rgb(95.6, 143.4, 191.3)` held but `canvasColorMode = defaultGray` for Standard / Full Screen With Menubar / Artboard and `black` for Full Screen. Frames: `dropShadow`, `dropShadow`, `line`, `none`.

`layerThumbnailSize = medium`. `useRichToolTips = true`, `showToolTips = true`.

### [WORKSPACE]

```
enableNarrowOptionBar       = true
autoCollapseDrawers         = true
enableLargeTabs             = false
autoShowRevealStrips        = false
openNewDocsAsTabs           = true
enableFloatingDocDocking    = true
enableMacOSNativeFullScreen = false
useOSMenuAlignment          = false
```

### [TOOLS]

```
useRichToolTips             = true
enableGestures              = true
overscrollEnabled           = true
resizeWindowsOnZoom         = false
zoomWithScrollWheel         = false
animationKey                = true        (animated zoom)
verticalMovementsBrushHUD   = true
disableOpacityBrushHUD      = false
arrowKeysRotateBrushTip     = true
eraseUsingBrushTablet       = false
shiftKeyToolSwitch          = true
flick                       = true
transformsSnapToPixels      = true
showTransformReferencePoint = true
doubleClickLayerMaskLaunchSelectionLab = true
springLoadedTools           = true   (sensitivity 0.2)
showHUD                     = showHUDTopRight
```

### [UNITS_AND_RULERS]

```
rulerUnits                  = rulerPixels
typeUnits                   = rulerPixels
columnWidth                 = 180 (distanceUnit)
gutterWidth                 = 12.0003662109375
newDocPresetPrintResolution = 21600
newDocPresetScreenResolution= 5184
exactPoints                 = false        (PostScript points, 72/inch)
```

### [GUIDES_GRID_SLICES]

```
guidesColor                 = cyan          guidesStyle  = lens
activeArtboardGuides        = lightBlue / lens
nonActiveArtboardGuides     = lightBlue / dashedLines
smartGuidesColor            = magenta
gridColor                   = customEnum, rgb(128.9, 128.9, 128.9)
gridStyle                   = lens
gridMajor                   = 64            subdivisions = 4
gridUnits                   = rulerPixels
hoverBoundsColor            = lightBlue
sliceColor                  = lightBlue     showSliceNumbers = true
showInactiveArtboardGuides  = false
controlColor                = controlColorDefault
```

### [FILE_HANDLING]

```
previewsQuery               = queryAlways     (image previews: Always Save)
previewWinThumbnail         = true
extensionsQuery             = queryAlways     (append extension: Always)
lowerCase                   = true
defaultCloudSave            = false           (default to local, not cloud)
cameraRaw                   = true            (prefer ACR for supported raw)
preferACRForHDRToning       = false
EXIF                        = false           (ignore EXIF profile tag: off)
ignoreRotationMetadata      = false
FileSaveToOriginalFolder    = true
fileSaveInBackground        = true
fileAutoSaveEnabled         = true            interval 5 min
fileLegacySaveAs            = false
fileDoNotAppendCopy         = false
askLayeredTIFF              = true
clipboardTIFFTransparencyPref = true
disablePSDCompression       = false
maximizeCompatibility       = queryAlways     (Maximize PSD compatibility: Always)
cloudWorkareaCustom         = false
cloudWorkareaDirectory      = /Users/bardiasamiee/Documents/Adobe/Photoshop Cloud Associates
recentFiles                 = 20
```

### [PERFORMANCE_AND_GPU]

```
historyStates               = 50
numberOfCacheLevels         = 4        numberOfCacheLevels64 = 4
tileSize                    = 131072   tileSize64 = 1048576   (1024K)
memoryUsagePercent          = 70 %
openglEnabled               = true
npuEnabled                  = true
openglAdvanced.openglVBLSyncEnabled       = true
openglAdvanced.CLCompute                  = true
openglAdvanced.gpuCompositingChecked      = true
openglAdvanced.deepMonitor                = true
openglAdvanced.openglAdvAntiAliasEnabled  = true
openglAdvanced.openglBilerpEnabled        = false
compCoreGPUEnabled2         = true
compCoreThreadsEnabled      = true
mtPsdReadEnabled            = true     mtPsdReadPeakThreads = 8
expFeatureDynamicMaxPyramidLevel = false
scratchDisks                = ["Startup"]     (926.4 G volume, 717.1 G free, async I/O on)
```

`app.systemInformation` GPU block: `useGPU 1`, `useOpenCL 1`, `isGPUCapable 1`, `isGPUAllowed 1`, GPU `Apple M4 Max` / vendor `APPLE`, 55,662 MB GPU-accessible RAM against a 1,500 MB requirement, `UseGraphicsProcessorChecked 1`, `UseOpenCLChecked 1`, OpenCL 1.2, compute score 72,367.4, bandwidth 1,746 GB/s. Composite Core GPU on, Multithreaded Compositing on, Compositing Document Tab UI off, Comp Core Feature Prefs off.

### [CURSORS_AND_DISPLAY]

```
paintingCursors             = brushSize
otherCursors                = precise
cursorShape                 = normal
cursorBrushTipOutlineStrokeWidth = 1
cursorCrosshair             = true     (show crosshair in brush tip)
cursorCrosshairWhileStroking = false
cursorHideStroke            = false
cursorStrokeRope            = false    color rgb(255, 74, 255)
```

### [TRANSPARENCY_AND_GAMUT]

```
transparencyGamutPreferences = medium       (grid size)
transparencyGridColors       = light
gamutWarning                 = rgb(153, 153, 153)   opacity 100 %
```

### [TYPE]

```
smartQuotes                 = true
textComposerChoice          = defaultTextInterface
enableFontFallback          = true
enableGlyphAlternate        = true
enablePlaceHolderText       = false       (no lorem-ipsum fill)
enableAutoDefaultFontSize   = false
autoListDetection           = false
showEnglishFontNames        = true
showFontPreviews            = true        size = large
textToolTreatsESCAsCommit   = true
textToolRecentFontDisplayNumber = 10
```

### [HISTORY_AND_CONTENT_CREDENTIALS]

```
maximumStates               = 50
snapshotInitial             = true
nonLinear                   = false
historyLog                  = false      saveHistoryTo = metadata    editLogItems = session
contentCredentialsDocumentOptions = none    contentCredentialsDocumentAsk = true
contentCredentialsExportOptions   = none
contentCredentialsAvailable       = true
```

### [IMAGE_PROCESSING_AND_AI]

```
imageProcessingSelectSubjectPrefs                  = imageProcessingModeDevice   (Select Subject: on device)
imageProcessingSelectionsProcessingPrefsStr        = imageProcessingPerformantMode
imageProcessingRemoveToolProcessingPrefsStr        = imageProcessingPerformantMode
imageProcessingEnhanceResolutionProcessingPrefsStr = imageProcessingPerformantMode
```

`experimentalFeatures`:

```
enhancedControlsTouchBarPropertyFeedback = true
expFeatureDeepUpscale                    = true
expFeatureContentAwareTracing            = true
expFeaturePreciseHDRColorManagement      = true
FGCompositeCache                         = true
DroverUI                                 = true
FlowCompositing                          = false
expFeatureChalkboard2                    = false
FlowCanvas                               = false
HasInteractedWithCoediting                = false
```

`earlyAccessPrefs` and `privacyPrefs` are both empty descriptors.

### [EXPORT_AND_PLUGINS]

```
exportFileType              = PNG          exportFilePath = ""
exportAssetJPGQualityEnum   = 6
exportAssetsLocationSetting = 3            exportAsLocationSetting = 2
exportPNGTransparency       = true
exportMetaData              = 0
exportConvertToSRGB         = true
exportExportAsLegacy        = false
pluginPicker.showAllFilterGalleryEntries = false
pluginPicker.enablePluginDeveloperMode   = false
pluginPicker.generatorEnabled            = false
generatorStatus.generatorStatus          = 0   (Generator not running)
```

### [COLOR_SETTINGS]

`app.colorSettings` returns the name `Default Color Settings` (Photoshop's label for an unsaved custom set), with:

```
workingRGB      = Adobe RGB (1998)
workingCMYK     = GRACoL2013_CRPC6.icc
workingGray     = Gray Gamma 2.2
workingSpot     = Dot Gain 20%
policyRGB / policyCMYK / policyGray = preserve
askMismatchOpening = false   askMismatchPasting = true   askMissing = true
engine          = Adobe (ACE)
intent          = colorimetric   (relative colorimetric)
mapBlack        = true            (black point compensation)
dither          = true
renderSceneReferred = false
monitorCompression  = false
RGBBlendGamma   = false
TextBlendGamma  = 1.45
```

Matches the `Color Settings.csf` description verbatim. Non-default.

## [03]-[CAMERA_RAW]

Three locations, all present.

### [BINARY_PREFERENCES]

`~/Library/Preferences/Adobe Camera Raw Prefs` — 4340 bytes, mtime 2026-09-11 00:06, not a plist (`plutil` rejects it). It is the legacy four-char-code record format: tag (4 bytes, byte-reversed) + count (LE u32) + size (LE u32) + payload. Decoded to `camera-raw-prefs-decoded.txt`, 94 records. Workflow-relevant values:

| [TAG] | [VALUE] | [MEANING] |
| :---- | :------ | :-------- |
| `ClrS` | `DiP3` | workflow output space **Display P3** |
| `GryS` | `Gr22` | gray space Gray Gamma 2.2 |
| `BtDp` | 16 | **16 bits/channel** output |
| `Inte` | 1 | intent |
| `SmPI` | 0 | smart-object open off |
| `GPUi` | 0 | — |
| `MkLD` `MkCD` `MkED` `MkCG` `MkDD` `MkCu` `MkPe` `MkGd` `MkGw` `MscD` | 1,1,1,1,1,1,1,0,0,1 | per-panel masks/visibility toggles |
| `mlcb` / `nrla` | 50 | slider defaults |
| `WndS` / `MgWS` | window rects | UI state |

These are **not** the ACR factory workflow defaults (factory is sRGB, 8 bit). Display P3 at 16 bit is a deliberate setting and is consistent with the Photoshop-side Adobe RGB / GRACoL colour configuration.

### [XMP_PREFERENCES]

`~/Library/Application Support/Adobe/CameraRaw/Defaults/Preferences.xmp` (659 bytes) is the modern store:

```
crs:RawDefaultsElements          = "Adobe"
crs:DNGSidecarHandling           = "0"
crs:NegativeCachePath            = ""          (default location)
crs:NegativeCachePath2           = ""
crs:NegativeCacheMaximumSize     = "5.0"       (GB)
crs:NegativeCacheLargePreviewSize= "2048"
crs:JPEGHandling                 = "OpenIfHasSettings"
crs:TIFFHandling                 = "OpenIfHasSettings"
crs:AVIFHandling                 = "OpenIfHasSettings"
crs:JXLHandling                  = "OpenIfHasSettings"
```

`~/Library/Application Support/Adobe/CameraRaw/Defaults/RawDefaults.xmp` (458 bytes): `crs:Defaults="Adobe"`, `crs:MasterOnly="True"` — raw defaults are Adobe's, applied master-only. No camera-specific default override.

**Sidecar XMP**: `DNGSidecarHandling = 0` and no `Settings/*.xmp` beyond two index files — settings go to the Camera Raw database, not to sidecar `.xmp` files.

### [CACHE]

Cache path is unset, so the default applies: `~/Library/Caches/Adobe Camera Raw 2`. That directory exists and is **0 B / empty**. Maximum size 5.0 GB, large preview 2048 px.

### [GPU]

`~/Library/Application Support/Adobe/CameraRaw/GPU/Adobe Photoshop Camera Raw/Camera Raw GPU Config.txt` (632 bytes):

```
crs:gpu_preferred_system              = ""     (Auto)
crs:gpu_init_digest                   = 2AA37D91C42F14EC96C635B91DBB2D78
crs:gpu_compute_digest                = A1482CEDCF06AEF93A43AB6E706C9416
crs:gpu_compute_quick_self_test_passed= True
crs:gpu_hdr_display_scale             = 1
crs:gpu_noise_fudge                   = 0
crs:gpu_trimap_dilate_radius          = 0.01
crs:gpu_trimap_erode_radius           = 0.01
```

A sibling `Adobe Photoshop Lightroom Classic/Camera Raw GPU Config.txt` (574 bytes) carries the same values minus `gpu_init_digest`. `GPU/TimeEstimates.xmp` is 0 bytes.

### [PRESETS_AND_PROFILES]

| [FOLDER] | [SIZE] | [FILES] |
| :------- | -----: | ------: |
| `CameraProfiles/` | 0 B | 0 |
| `Curves/` | 0 B | 0 |
| `ImportedSettings/` | 0 B | 0 |
| `Settings/` | 1.2 M | 2 (`Index_8CE86435CBE10753.dat`, `Index_D274CEB126FE73ED.dat`) |
| `Defaults/` | 8 K | 2 (the two `.xmp` above) |
| `GPU/` | 8 K | 3 |
| `LensProfiles/1.0/` | 2.9 M | 1 (`pscache.dat`) |
| `ModelZoo/CloudDownload/` | 550 M | 21 across 10 model directories + `Index2.dat` |
| `Logs/` | 116 K | — |

**Zero user ACR presets, zero imported settings, zero custom camera profiles, zero custom curves.** The 550 MB `ModelZoo` is Adobe's downloaded AI model cache (Denoise, Enhance, masking), not user content.

## [04]-[UXP_AND_TK9]

### [INSTALLED_PLUGINS]

`~/Library/Application Support/Adobe/UXP/Plugins/External/` — ten entries, nine of them TK9:

| [ID] | [NAME] | [VERSION] | [HOST] | [MIN_VERSION] | [ENTRYPOINTS] |
| :--- | :----- | :-------- | :----- | :------------ | :------------ |
| `com.tk.comboV8` | TK9 Combo | 4.0.0 | PS | 23.3.0 | `tkcombocxv9`, `toolTips` |
| `com.tk.cxV8` | TK9 Cx | 4.0.0 | PS | 23.3.0 | `tkcombocxv9`, `toolTips` |
| `com.tk.export` | TK9 Export | 4.0.0 | PS | 23.3.0 | `tkexportv9`, `toolTips` |
| `com.tk.multimask` | TK9 Multi-Mask | 4.0.0 | PS | 23.3.0 | `tkmultimaskv9`, `toolTips` |
| `com.tk.myactionsV8` | TK9 My Actions | 4.0.0 | PS | 23.3.0 | `tkmyactions` |
| `com.tk.myactionstab1` | TK9 My Actions-Tab 1 | 4.0.0 | PS | 23.3.0 | `tkmyactions` |
| `com.tk.myactionstab2` | TK9 My Actions-Tab 2 | 4.0.0 | PS | 23.3.0 | `tkmyactions` |
| `com.tk.myactionstab3` | TK9 My Actions-Tab 3 | 4.0.0 | PS | 23.3.0 | `tkmyactions` |
| `com.tk.myactionstab4` | TK9 My Actions-Tab 4 | 4.0.0 | PS | 23.3.0 | `tkmyactions` |
| `8ebe7f95` | Sidekick | 1.0.22 | **ID** 19.0 | — | `mcpPanel` (InDesign, not Photoshop) |

`app.systemInformation` confirms all nine TK9 panels load "from Plugin Marketplace"; Multi-Mask, Combo, Export and My Actions reach **Loaded** at ~1176–1180 ms with ≤3 ms launch impact, while Cx and the four My Actions tabs stay **Prepared**.

### [CONFIGURED_VALUES]

`~/Library/Application Support/Adobe/UXP/PluginsStorage/PHSPBETA/27/External/<id>/PluginData/` — flat one-value `.ini` files, 26 in total. No secrets present.

| [MODULE] | [KEY] | [VALUE] |
| :------- | :---- | :------ |
| `com.tk.multimask` | `autoSetGraySpace` | **`true`** (Match Gray) |
| `com.tk.multimask` | `autoShowProperties` | **`false`** |
| `com.tk.multimask` | `autoHideSelection` | `false` |
| `com.tk.multimask` | `showSelectionIndicator` | `true` |
| `com.tk.multimask` | `FXOverlayColor` | `#ff00ff` |
| `com.tk.multimask` | `language` | `English` |
| `com.tk.comboV8` | `autoCloseTKActions` | `true` |
| `com.tk.comboV8` | `colorOpacityValue` | `1` |
| `com.tk.comboV8` | `overlayColor` | `#ff00ff` |
| `com.tk.comboV8` | `showSelectionIndicator` | `true` |
| `com.tk.comboV8` | `showSmartObjectIndicator` | `true` |
| `com.tk.comboV8` | `watermarkEdgeOffsetType` | `PX` |
| `com.tk.comboV8` | `watermarkPosition` | `CenterCenter` |
| `com.tk.comboV8` | `watermarkSmartObjectType` | `Embedded` |
| `com.tk.comboV8` | `webSharpenFileType` | `jpg10` |
| `com.tk.comboV8` | `language` | `English` |
| `com.tk.export` | `radioOutputLocation` | **`SameFolder`** |
| `com.tk.export` | `radioChooseSource` | `CurrentImage` |
| `com.tk.export` | `radioCropType` | `Centered` |
| `com.tk.export` | `radioBarType` | `ColorBars` |
| `com.tk.export` | `radioLogoPosition` | `CenterCenter` |
| `com.tk.export` | `radioEdgeOffsetType` | `PX` |
| `com.tk.export` | `saveFileType` | `jpg10` |
| `com.tk.export` | `loadLastPreset` | `false` |
| `com.tk.export` | `language` | `English` |
| `com.tk.myactionsV8` | `language` | `English` |

**Export folder**: `radioOutputLocation = SameFolder`; no explicit export path is stored anywhere in TK9's PluginData.

**My Actions / button assignments**: `com.tk.myactionsV8` stores nothing but `language.ini`, and `com.tk.myactionstab1..4` have **no PluginData directory at all** (only four `External/` module folders exist: `comboV8`, `cxV8` absent, `export`, `multimask`, `myactionsV8`). The TK9 My Actions panel and its four tabs are therefore **unconfigured** — no button assignments, no "My Actions" contents. This is a first-run state.

There is no `LocalStorage` or `SecureStorage` under any `External/` TK9 module, only `PluginData`. Adobe's own `Internal/` modules (`com.adobe.cclibrariespanel`, `pluginspanel`, `unifiedpanel`, `cai.uxp`, `nfp.gallery`, `ccx.start`, `adjustments-panel`, `inAppMessaging`, `inAppNotifications`, `uam`, `sharePanel`, `ccx.comments-webview`, `uxp.express-template-picker`) sit alongside. A stale `PHSPBETA/23/` tree also exists.

## [05]-[FACTORY_PRESETS_AND_CODE_SIGNING]

### [CODE_SIGNING]

```
codesign -dv "/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app"
Identifier        = com.adobe.Photoshop
Format            = app bundle with Mach-O universal (x86_64 arm64)
CodeDirectory     v=20500 size=1484511 flags=0x10000(runtime) hashes=46380+7
TeamIdentifier    = JQ525L2MZD
Runtime Version   = 15.2.0
Timestamp         = Sep 9, 2026 at 17:52:30
Sealed Resources  version=2 rules=13 files=6690
CDHash            = 95db8245a848966ddbb8209c4538ceb84d3898d2
```

**`Presets/` is NOT inside the signed bundle.** It sits beside the `.app`, at `/Applications/Adobe Photoshop (Beta)/Presets/` — a sibling of `/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app/`. `Contents/Presets` does not exist. So deleting a factory preset file would not break the signature.

It is nevertheless the wrong lever. Ownership is `root:admin`, mode `drwxr-xr-x`, so a write needs `sudo`; and more importantly **the files in `Presets/` are not what the panels show**. The panel contents live in the resident `.psp` stores under `~/Library/Preferences/Adobe Photoshop (Beta) Settings/` (section 01). The `.abr`, `.pat`, `.asl`, `.aco`, `.tpl`, `.atn` files in `Presets/` are load-on-demand archives reachable from the panel flyouts and from Edit ▸ Presets ▸ Preset Manager; none of the factory groups now in the panels (General Brushes, Dry Media Brushes, Blues, Iridescent, Wild Animals, Trees, …) corresponds to any file there.

**To remove factory presets from a panel, delete the groups in the panel (or in Preset Manager) and then save the workspace.** File deletion under `/Applications/.../Presets/` would change nothing in the panels, would need `sudo`, and would be undone by the next Beta update. The `.psp` stores are rewritten on quit, so the removal persists through them.

### [PRESETS_INVENTORY]

`/Applications/Adobe Photoshop (Beta)/Presets/` — 29 directories. Sizes: 3DLUTs 2.7M, Actions 252K, Black and White 48K, Brushes 11M, Channel Mixer 24K, Color Books 144K, Color Swatches 864K, Contours 8K, Curves 36K, Deco 2.1M, Duotones 548K, Exposure 16K, Guides 40K, HDR Toning 64K, Hue and Saturation 32K, Image Size 48K, Levels 32K, **Menu Customization 0 B (empty)**, Optimized Colors 16K, Optimized Output Settings 12K, Optimized Settings 48K, Patterns 9.3M, Scripts 1.6M, Styles 2.2M, Tools 196K, Video 3.1M, Widgets 884K.

Requested subfolders (full listing in `factory-presets.txt`):

| [FOLDER] | [FILES] |
| :------- | :------ |
| `Brushes/` | `Converted Legacy Tool Presets.abr` (5772265), `Legacy Brushes.abr` (6207521) |
| `Patterns/` | `Artists Brushes Canvas.pat` (2648630), `Color Paper.pat` (1543262), `Erodible Textures.pat` (3345174), `Nature Patterns.pat` (1065308), `Rock Patterns.pat` (1087089) |
| `Gradients/` | **absent** |
| `Styles/` | `Abstract Styles.asl` (2264336) |
| `Color Swatches/` | 21 `.aco` files + 2 `.pdf` readmes: ANPA Colors, DIC Color Guide, FOCOLTONE Colors, HKS E/E Process/K/K Process/N/N Process/Z/Z Process, Mac OS, Paint Color Swatches, Photo Filter Colors, TRUMATCH Colors, VisiBone, VisiBone2, Web Hues, Web Safe Colors, Web Spectrum, Windows |
| `Tools/` | `Crop and Marquee.tpl` (4440), `Text.tpl` (191564) |
| `Actions/` | `Commands.atn` (5273), `Frames.atn` (44724), `Image Effects.atn` (25833), `LAB - Black & White Technique.atn` (2523), `Production.atn` (9451), `Stars Trails.atn` (15746), `Text Effects.atn` (43623), `Textures.atn` (52468), `Video Actions.atn` (44718) |
| `Scripts/` | 26 top-level entries + `Event Scripts Only/` (8) + `Stack Scripts Only/` (40). **All `.jsx`; no `.psjs`.** |
| `Workspaces/` | **absent** — factory workspaces are at `…app/Contents/Required/Workspaces/` |
| `Menu Customization/` | present, **empty** |
| `Keyboard Shortcuts/` | **absent** |
| `Contours/` | `Contours.shc` (5149) |
| `Guides/` | 10 `.gds`: 12 Column, 16 Column, 2 x 3, 2 x 4, 24 Column, 3 x 2, 3 x 3 (Thirds), 4 x 2, 5 x 5 (Fifths), 8 Column |

### [PREFERENCE_KEY_NAMES_IN_THE_BINARY]

`strings -n 5` over `…/Contents/MacOS/Adobe Photoshop 2026` (389,879,904 bytes → 964,071 strings, saved as `ps-binary-strings.txt`). Exact identifiers found for each requested substring:

| [SUBSTRING] | [KEYS] |
| :---------- | :----- |
| Quiet | `quietMode`, `QuietMode`, `quiet`, `isServerQuiet` |
| ContextualTaskBar | `contextualTaskBars`, `RemoveToolContextualTaskBar` |
| Home | `autoShowHomeScreen`, `AutoShowHomeScreen`, `showHomeAtLaunch`, `showHomeScreen`, `hideHomeScreen`, `homeScreenVisibility`, `homeScreenVisibilityChanged`, `homeScreenReady`, `homeScreenDeepLink`, `GetHomeScreenDeeplink`, `HomeScreenAutoShow`, `HomeScreenManualShow`, `ManualShowHomeScreen`, `SwitchHomeScreen`, `showFromHomeScreen`, `fileOpenContextHomeScreenLRImport` |
| WhatsNew | `showWhatsNew`, `showWhatsNewRestore`, `WhatsNew` |
| Assistant | `showAIAssistant`, `AIAssistant`, `cmAIAssistant`, `hasAssistant`, `aiAssistantIcon`, `GenAiGenerativeRecomposeInAIAssistant`, `userFeedbackDialogDetailsAssistant` |
| Generative | `EnableGenerativeLayerEditing`, `actionGenerativeKey`, `assignGenerativeReferenceImage`, `ColligoGenerativeFill`, `ColligoGenerativeHarmonize`, `CxUI_GenerativeFill`, `Drover_GenerativeFillNew`, `cxui_avail_generative_layer`, `cxui_generative_layer_bar`, `gen_tech_generative_recompose`, `gen_tech_generative_upscale`, `GenAiGenerativeRecompose` |
| Onboarding | `enableOnboarding`, `showFeatureOnboarding`, `FeatureOnboarding`, `creativeOnboarding`, `CloudDocCDPOnboarding`, `disableCloudPickerOnboarding`, `ForceOnboardingToDoId`, `FirstCommentUserOnboardingPrefKey`, `ShareForReviewOnboardingPrefKey`, `onboarding_id` |
| Discover | `showEmbeddedDiscoverPanel`, `DisableEmbeddedDiscoverPanel`, `DiscoverPanelEarlyLoad`, `NF_DISCOVER_QUICK_ACTION` |
| AutoShowHome | `autoShowHomeScreen`, `AutoShowHomeScreen` |
| ToolTip / RichToolTip | `showToolTips`, `useRichToolTips`, `useRichToolTipsRestore`, `showRichTooltip`, `hideRichTooltip`, `RichToolTipsManifest`, `UXPRichTooltips`, `V3ExpandedRichToolTips` |
| Ruler | `ruler_units`, `rulerOriginH`, `rulerOriginV`, `RulerLocation`, `rulerInches`, `rulerCm`, `rulerMm`, `clearRuler`, `DoInvalidateRulers`, `origin_edits_honor_nonzero_display_ruler_origin` |
| StatusBar | `toggleStatusBar` |
| BrushPreview | `BrushPreview`, `BrushPreviewBackground`, `BrushPreviewBackgroundDisabled`, `gBrushPreviewSize`, `gBrushPreviewWidth`, `gBrushPreviewHeight` |
| SmallUI | `gSmallUIFont` |
| PanelThumb | `droverLayersPanelThumbnailSize`, `TMaskPanelThumbnailView` |
| LayerThumbnail | `layerThumbnailSize`, `dynamicLayerThumbnail`, `GetLayerThumbnail` |

A full-text scan of the preference store for `contextualTaskBar` and `quietMode` returns **nothing** — neither key has been written to disk, so both are at their compiled-in defaults. `quietMode` does appear as a key name in `MachinePrefs.psp`'s schema strings but with no persisted override.

## [06]-[SCRIPTS_FOLDER]

- `/Applications/Adobe Photoshop (Beta)/Presets/Scripts/` — 26 top-level files plus two subfolders, **all `.jsx`** (`Image Processor.jsx`, `Export Layers To Files.jsx`, `Layer Comps To Files.jsx`, `Load Files into Stack.jsx`, `Merge To HDR.jsx`, `Photomerge.jsx`, `Script Events Manager.jsx`, `Fit Image.jsx`, `Statistics.jsx`, `Conditional Mode Change.jsx`, `ContactSheetII.jsx`, `ArtBoards To Files.jsx`, `ArtBoards To PDF.jsx`, `Delete All Empty Layers.jsx`, `Flatten All Layer Effects.jsx`, `Flatten All Masks.jsx`, `ExportColorLookupTables.jsx`, `Lens Correct.jsx`, `Load DICOM.jsx`, `generate.jsx`, `ArtboardExport.inc`), plus `Event Scripts Only/` (8 `.jsx`) and `Stack Scripts Only/` (`.jsx`, `.exv`, `.png`).
- **No `.psjs` file exists anywhere in `/Applications/Adobe Photoshop (Beta)/`.** A full-text scan for the literal `psjs` across the entire installation — main binary, `dvauxphost.framework`, `ScriptingSupport.plugin`, every framework and plugin — returns zero hits. **File ▸ Scripts in this build does not enumerate `.psjs`.** The menu is fed by the ExtendScript `.jsx` scanner only.
- **`~/Library/Application Support/Adobe/Adobe Photoshop (Beta)/Presets/Scripts` does not exist.** The user Presets tree has 28 subdirectories and **no `Scripts` directory at all**:

  `Actions`, `Adjustments`, `Black and White`, `Brushes`, `Channel Mixer`, `Color Range`, `Color Swatches`, `Contours`, `Curves`, `Custom Shapes`, `Custom Toolbars`, `Duotones`, `Exposure`, `Gradients`, `Guides`, `HDR Toning`, `Hue and Saturation`, `Image Size`, `Keyboard Shortcuts`, `Levels`, `Menu Customization`, `Patterns`, `Select and Mask`, `Selective Color`, `Skies`, `Smart Sharpen`, `Styles`, `Tools`.

  **Every one of the 28 is empty — zero files.** The per-user script location is therefore not provisioned; a user script placed there would need the `Scripts` directory created first, and on the evidence of this build the reliable location remains the admin-owned `/Applications/Adobe Photoshop (Beta)/Presets/Scripts/` (requires `sudo`) or File ▸ Scripts ▸ Browse.

- **Script Events Manager store**: no `Script Events Manager.xml` and no equivalent file exists — not in `~/Library/Preferences/Adobe Photoshop (Beta) Settings/`, not under `~/Library/Application Support/Adobe/`. The script `Script Events Manager.jsx` (47426 bytes) ships in `Presets/Scripts/`, and the binary carries the string `Script Events Manager` plus a route pattern `(image-processor|delete-all-empty-layers|flatten-all-layer-effects|flatten-all-masks|script events-manager|load-dicom|load-files-into-stack)(-jsx)`, but **no events have been registered**, so no store has been created.

- `~/Library/Preferences/Adobe Photoshop 2026 Paths` (85 bytes) reads `/Applications/Adobe Photoshop (Beta)/ /Applications/Adobe Photoshop (Beta)/Plug-ins/` — the app and additional-plugins roots.

## [07]-[PRE_CODEX_BACKUP]

`~/Library/Application Support/design-tools/backups/photoshop-2026-2026-09-11/`

Sibling backups in the same tree: `illustrator-2026-2026-09-11/`, `indesign-2026-2026-09-11/`, `uxp/`.

Despite the directory name, every file inside carries an **Aug 19 00:48–00:52** mtime, and the paths say `Adobe Photoshop 2026`, not `(Beta)` — this is a snapshot of the **release** Photoshop 2026 install, taken at its first-run state.

```
    81  Adobe Photoshop 2026 Paths                 → "/Applications/Adobe Photoshop 2026/ /Applications/Adobe Photoshop 2026/Plug-ins/"
   228  com.adobe.Photoshop.plist
        Adobe Photoshop 2026 Settings/
   624    Actions Palette.psp
 25014    Adjustments_Presets/presets.json
227598    Adobe Photoshop 2026 Prefs.psp
1236268   Brushes.psp
2312463   CustomShapes.psp
     3    DialogPreferences.psp
   588    DVADialogPrefs/DVADialogPrefs.xml
    64    FavoriteFonts.psp
   440    FMCache.psp
139245    Gradients.psp
    19    LaunchEndFlag.psp
124421    MachinePrefs.psp
   557    MRU New Doc Sizes.json
   104    New Doc Sizes.json
8594532   Patterns.psp
868282    PluginCache.psp
    20    QuitEndFlag.psp
  3253    sniffer-out.txt
  3255    sniffer-out1.txt
10812911  Styles.psp
 10809    Swatches.psp
  6198    UIPrefs.psp
 32682    Workspace Prefs.psp
    64    WorkSpaces/                 (EMPTY)
    96    WorkSpaces (Modified)/
 32571      Essentials.psw
```

### [USER_PRESETS_TREE]

```
User Presets/
  Actions              0 files
  Adjustments          0 files
  Black and White      0 files
  Brushes              0 files
  Channel Mixer        0 files
  Color Range          0 files
  Color Swatches       0 files
  Contours             0 files
  Curves               0 files
  Custom Shapes        0 files
  Custom Toolbars      0 files
  Duotones             0 files
  Exposure             0 files
  Gradients            0 files
  Guides               0 files
  HDR Toning           0 files
  Hue and Saturation   0 files
  Image Size           0 files
  Keyboard Shortcuts   0 files
  Levels               0 files
  Menu Customization   0 files
  Patterns             0 files
  Select and Mask      0 files
  Selective Color      0 files
  Skies                0 files
  Smart Sharpen        0 files
  Styles               0 files
  Tools                0 files
```

**All 28 directories are empty (64 bytes each, zero files).** There was **no custom toolbar**, **no custom keyboard shortcut set**, **no menu customization**, and **no user-saved preset of any kind** before the Codex work. `WorkSpaces/` in the backup is likewise empty — **no user workspace existed**; only the modified `Essentials` layout.

The backup's `Actions Palette.psp` is **624 bytes** against the current **106233** — at snapshot time the Actions panel held essentially nothing (the store contains only the `setsVlLs` scaffold). Both the six Adobe default action sets and the `TK9 actions` set arrived afterwards.

`com.adobe.Photoshop.plist` in the backup:

```
butler.cxui_layer_next_variation  = 1
NSDisabledCharacterPaletteMenuItem = true
paletteEnhancedFontTypeKey 2026    = 1
uiLanguageKey 2026                 = "en_US"
VMMemoryUsagePercent642026         = 70
```

The current `~/Library/Preferences/com.adobe.Photoshop.plist` adds only Cocoa window/panel state (`NSNavPanelExpandedSizeForOpenMode`, `NSNavPanelExpandedSizeForSaveMode`, `NSOSPLastRootDirectory`, `NSWindow Frame GoToSheet`). The five Photoshop keys are unchanged.

## [08]-[FACTORY_VERSUS_USER]

| [ITEM] | [VERDICT] |
| :----- | :-------- |
| Brushes, Swatches, Gradients, Styles, Patterns, Custom Shapes, Contours, Tool Presets (all resident groups) | **Factory**, unmodified. Zero user presets. |
| Adjustments panel presets | **Factory** |
| Action sets 2–7 (Basic Adjustments, Subject & Background, Creative Effects, Guides, Resize, Export) | **Adobe defaults**, seeded by the app; absent from the backup |
| Action set 1 `TK9 actions` (26 actions) | **User / TK9** |
| Workspace `Editorial Images` | **User**, created 2026-09-11 01:14 from the then-current Essentials layout, edited through 06:59 |
| TK9 panel docking inside that workspace | **User** |
| Color settings (Adobe RGB 1998 / GRACoL2013_CRPC6 / Gray 2.2 / preserve / ACE / relative colorimetric / BPC / dither) | **User or CC-synced**, not Photoshop factory |
| Camera Raw workflow output (Display P3, 16 bit) | **User**, not ACR factory (sRGB, 8 bit) |
| Camera Raw presets, profiles, curves, imported settings | **None** — empty folders |
| Home screen off (`homeScreenVisibility = false`, `autoShowHomeScreen = false`) | **User** |
| Ruler and type units in pixels | **User** (factory is inches for rulers, points for type) |
| History states 50, cache levels 4, tile 1024K, memory 70 % | 50 states is above the factory 20; cache/tile/memory are at defaults |
| Auto-save every 5 minutes, save in background, save to original folder | Save-to-original-folder is a **user** change; the rest are defaults |
| Nine TK9 v4.0.0 UXP plugins | **User**, from the Plugin Marketplace |
| TK9 configuration | Mostly **defaults**; the one deliberate value is `autoSetGraySpace = true` (Match Gray). `autoShowProperties = false`. My Actions entirely unconfigured. |
| Custom toolbar, keyboard shortcuts, menu customization | **None**, now or in the backup |
| User Presets tree (both current and backup) | **Empty**, 28 empty directories each |
| New Document presets | **None** user-defined |
| `.psjs` scripts | **Unsupported by this build** |
| Script Events Manager | **No registered events, no store** |

## [09]-[DUMP_FILES]

All under `/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/photoshop/`:

| [FILE] | [CONTENT] |
| :----- | :-------- |
| `application-descriptor.json` | full Action Manager application descriptor, 130 keys (238 KB) |
| `preferences.json` | ExtendScript `app.preferences` + `app.*` scalars + action sets + font family count |
| `preset-manager.json` | preset-manager descriptor: brushes, swatches, gradients, styles, patterns, contours, tool presets |
| `preset-groups.txt` | `8BIMphry` group hierarchy and per-group counts for all six `.psp` stores |
| `system-information.txt` | `app.systemInformation`, 26797 bytes: GPU sniffer, frameworks, required plugins, UXP extensions |
| `camera-raw-prefs.txt` | raw bytes of `Adobe Camera Raw Prefs` |
| `camera-raw-prefs-decoded.txt` | 94 decoded four-char-code records |
| `workspace-panels.txt` | 61 decoded panel identifiers per `.psw`, both Editorial Images copies |
| `workspace-dock-tree.txt` | dock/tab-pane/tab-group tree of the live workspace |
| `factory-presets.txt` | file-by-file listing of the requested `Presets/` subfolders |
| `ps-binary-strings.txt` | 964,071 strings from the main binary |
| `strings-Adobe_Photoshop_(Beta)_Prefs.txt`, `strings-Workspace_Prefs.txt`, `strings-UIPrefs.txt`, `strings-MachinePrefs.txt` | `strings -n 4` of the four preference binaries |
| `phry.py` | the `8BIMphry` parser |
| `probe-prefs.jsx`, `probe-presets.jsx`, `probe-app.jsx`, `probe-sysinfo.jsx`, `run.applescript` | the read-only probe scripts |
