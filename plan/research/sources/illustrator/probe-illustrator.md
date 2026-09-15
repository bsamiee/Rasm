# Illustrator Beta 30.9.0 read-only probe

Probe date 2026-09-11. Nothing was written to any Illustrator folder, no document was saved, no
preference was set. Every artifact below sits under
`/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/5491787f-d5bf-4f29-95b6-7ed9e2976566/scratchpad/illustrator/`
(called `<S>` from here on).

## [00]-[DUMP_FILES]

| [FILE]                                     | [CONTENT]                                                         |
| :----------------------------------------- | :---------------------------------------------------------------- |
| `prefs-dump.txt`                           | `Adobe Illustrator Prefs` verbatim, CR translated to LF (4250 lines) |
| `prefs-flat.txt`                           | Same tree flattened to `section/key = value`, hex blobs decoded (2457 rows) |
| `cloud-prefs-dump.txt`, `cloud-prefs-flat.txt` | `Adobe Illustrator Cloud Prefs` raw and flattened (144 rows)   |
| `parse_prefs.py`                           | The flattener                                                     |
| `new-document-profiles.txt`                | All 141 rows of `PresetDocumentProfileDataV10.json`               |
| `Workspaces_Default_Workspace.txt` + `.OWLBookMark.xml` | Saved Default Workspace, and its decoded layout XML  |
| `Modified_Workspaces_*.txt` + `.OWLBookMark.xml` | Live Default Workspace, Essentials, Getting Started, Layout |
| `workspace-layout.txt`                     | All five workspace layouts as an indented tree                    |
| `Tools_Tools_Panel_Presets.txt`            | The user's custom toolbar, decoded                                |
| `factory-tools-panel-presets.txt`          | The app's stock toolbar presets, for comparison                   |
| `native-presets.txt`                       | Recursive listing of the nine factory preset folders              |
| `main-binary.strings`                      | 156 774 strings from the main executable                          |
| `binary-feature-keys.txt`                  | 142 `Enable*`/`Show*`/`Disable*` keys found in that binary        |
| `default-palette-ase.txt`                  | Parsed `Default Palette.ase` (92 colors, two groups)              |
| `mcp-tools.json`                           | Raw JSON-RPC `tools/list` response, 47 tools                      |
| `a-default-template.json`                  | Codex candidate template dump                                     |
| `b-my-default-profile-drive-root.json`     | Drive-root `My Default Profile.ai` dump                           |
| `c-my-default-profile-fork.json`           | `99.Default Profiles` fork dump                                   |
| `d-my-color-palette.json`                  | `My Color Palette.ai` dump                                        |
| `dump.jsx`, `job.txt`, `run-dumps.sh`, `run-dumps.log` | The ExtendScript harness and its run log            |

---

## [01]-[PREFERENCES]

### [01.1]-[TWO_FILES_NOT_ONE]

Preferences are split across two files in
`~/Library/Preferences/Adobe Illustrator 30.9.0 Beta Settings/en_US/`:

| [FILE]                        | [SIZE] | [FORMAT]                          | [HOLDS]                                                  |
| :---------------------------- | -----: | :-------------------------------- | :------------------------------------------------------- |
| `Adobe Illustrator Prefs`     | 92 226 B | ASCII, CR line endings, `/key value` tree with `{}` blocks and `[ len <hex> ]` blobs | Machine-local state: panels, actions, tool state, GenAI counters, MCP token, file lists |
| `Adobe Illustrator Cloud Prefs` | 3 342 B | same format                     | The settings that sync with the Adobe account: guides, grid, selection, type, units, clipboard, black preservation, UI brightness, tooltips |

**This split matters.** Almost every preference a person changes in `Illustrator > Settings` lives in
**Cloud Prefs**, not in the big Prefs file. Anything written into the big file for those keys is ignored.

Other state files in the same folder: `.runConfig` / `.runConfigRobin` (server feature flags, see
[04.4]), `PresetDocumentProfileDataV10.json`, `Workspaces/`, `Modified Workspaces/`, `Tools/`,
`WSMgrCfg/`, `DataRecovery/`, `AI Color Settings` (3.4 MB), `NotificationsUI.json`,
`3PModelDiscoveryCache.json`.

### [01.2]-[UNITS]

| [KEY]                          | [FILE]      | [VALUE] | [MEANING]                       |
| :----------------------------- | :---------- | ------: | :------------------------------ |
| `rulerType`                    | Cloud       | `6`     | General ruler unit = **Pixels** |
| `strokeUnits`                  | Cloud       | `6`     | Stroke = Pixels                 |
| `text/units`                   | Cloud       | `6`     | Type = Pixels                   |
| `text/asianunits`              | Cloud       | `2`     | Asian type = Points             |
| `numbersArePoints`             | Cloud       | `0`     | Numbers are not forced to points |
| `useGlobalRulers`              | Cloud       | `1`     | Global rulers, not per-artboard |
| `globalRulersVisible`          | Prefs       | `0`     | Rulers currently hidden         |
| `isRulerIn4thQuad`             | Prefs       | `1`     | Y increases downward            |
| `MeasureTool_Units`            | Prefs       | `0`     | Measure tool unit index         |
| `rulerType_5` / `_6` / `_7`    | Prefs       | `6`/`2`/`2` | Per-New-Document-dialog-tab remembered unit (Web=px, Print=pt) |

Unit codes seen: `2` = points, `6` = pixels. Every document dumped in [02] reports
`RulerUnits.Pixels`, which agrees.

### [01.3]-[GUIDES_GRID_SMART_GUIDES]

Guides and grid live entirely in **Cloud Prefs**:

```
Guide/Color/red = 0.29  Guide/Color/green = 1.0  Guide/Color/blue = 1.0   # cyan guides
Guide/Style = 0                                                          # lines, not dots
Guide/ShowPixelGrid = 1
Grid/Horizontal/Spacing = 64.0   Grid/Horizontal/Ticks = 4
Grid/Vertical/Spacing   = 64.0   Grid/Vertical/Ticks   = 4
Grid/Style = 0                                                           # lines
Grid/Posn = 0                                                            # grids in back
Grid/Color/Lite = 0.9,0.9,0.9    Grid/Color/Dark = 0.8,0.8,0.8
editableGuides = 1                                                       # (Prefs file)
```

Smart guides — note the same section name appears in **both** files with different keys:

| [KEY]                                   | [FILE] | [VALUE] |
| :-------------------------------------- | :----- | ------: |
| `smartGuides/isEnabled`                 | Prefs  | `1`     |
| `smartGuides/sensitivity`               | Prefs  | `1`     |
| `smartGuides/tolerance`                 | Prefs `0.0`, Cloud `2` | |
| `smartGuides/angularTolerance`          | Prefs  | `2`     |
| `smartGuides/rotationalSnapArcTolerance`| Prefs  | `4`     |
| `smartGuides/showRotationalGuides`      | Prefs  | `1`     |
| `smartGuides/invokeDistanceGuides`      | Prefs  | `1`     |
| `smartGuides/snapToIsolatedObjects`     | Prefs  | `1`     |
| `smartGuides/cursorSnapping`            | Prefs  | `0`     |
| `smartGuides/anglesCount` / `angles0..3`| Cloud  | `4` / 0, 45, 90, 135 |
| `smartGuides/customAngles0..3`          | Cloud  | 0, 45, 90, 135 |
| `smartGuides/showAlignmentGuides`       | Cloud  | `1`     |
| `smartGuides/showObjectHighlighting`    | Cloud  | `1`     |
| `smartGuides/showLabels`                | Cloud  | `1`     |
| `smartGuides/showReadouts`              | Cloud  | `1`     |
| `smartGuides/showConstructionGuides`    | Cloud  | `1`     |
| `smartGuides/showToolGuides`            | Cloud  | `1`     |
| `smartGuides/showSpacingGuides`         | Cloud  | `1`     |
| `smartGuides/snapToActiveArtboardContent` | Cloud | `1`    |

Snapping: `snappingTolerance` = `0.0` (Prefs) but `2` (Cloud); `showSnapping = 1`,
`forceSnapToGrid = 0`, `showVisualGuidesForSnapToGrid = 1`,
`snapToTangentPerpendicularParallel = 1`, `textLineSnapping = 1`, `snapToPoint = 1`,
`snapToGlyph = 1`, `showSnapToGlyphOpt = 1`, `textBaselineLineSnapping = 1`,
`textXHeightSnapping = 1`, `textImportantVisualLinesSnapping = 0`,
`alignmentGuides/{showSnapToGuide,showArtboardGuides,centerpoint,midpoint,endpoint}` all `1`.
Snap guide colors sit in `snapomatic/` (Prefs): construction guides RGB 0.18/0.19/0.57, glyph
guides 0.43/0.80/0.29.

### [01.4]-[SELECTION_AND_ANCHORS]

| [KEY]                        | [FILE] | [VALUE] |
| :--------------------------- | :----- | ------: |
| `selectionTolerance`         | Prefs `0.0`, Cloud `3` | |
| `anchorSizePref`             | Cloud  | `5`     |
| `selectedAnchorMarkType`     | Prefs  | `9`     |
| `unselectedAnchorMarkType`   | Prefs  | `1`     |
| `directionHandleMarkType`    | Prefs  | `10`    |
| `hideAnchorPointsInTools`    | Prefs  | `1`     |
| `showBoundingBox`            | Prefs  | `1`     |
| `showDirectionHandles`       | Cloud  | `1`     |
| `highlightAnchorOnMouseOver` | Cloud  | `1`     |
| `selectBehind`               | Cloud  | `1`     |
| `constrainPathDragging`      | Cloud  | `0`     |
| `doubleClickToIsolate`       | Cloud  | `1`     |
| `moveLockedAndHiddenArt`     | Cloud  | `1`     |
| `enableEnclosedMode`         | Cloud  | `0`     |
| `zoomToSelection`            | Cloud  | `1`     |
| `excludeEffectRegionsFromHitTest` | Cloud | `0` |
| `hitShapeOnPreview` / `hitTypeShapeOnPreview` | Cloud | `1` / `1` |
| `includeStrokeInBounds`      | Cloud  | `0`     |
| `scaleLineWeight`            | Cloud  | `1`     |
| `transformPatterns`          | Cloud  | `1`     |
| `onlyTransformPatterns`      | Prefs  | `0`     |
| `cursorKeyLength`            | Cloud  | `1.0`   |
| `maximumUndoDepth`           | Prefs  | `100`   |

### [01.5]-[CURSORS]

`usePreciseCursors = 1` (Cloud), `UIPreferences/scaleCursor = 1` (Prefs),
`TouchPreferenceUI/PreciseCursor = 1` (Prefs), `showColorSamplingRing = 1` (Cloud),
`liveCorners/cornerAngleLimit = 177.0`, `liveCorners/hideCornerWidgetBasedOnAngle = 1`,
`ovalRadius = 12.0`, `toolPen/rubberBandEnable = 1`, `pen/disableAutoAddDelete = 0`.

### [01.6]-[TYPE]

| [KEY]                             | [FILE] | [VALUE]              |
| :-------------------------------- | :----- | :------------------- |
| `text/faceSize`                   | Prefs  | `12.0`               |
| `text/defaultFaceNames/Roman/1`   | Prefs  | `MyriadPro-Regular`  |
| `text/greekingThreshold`          | Prefs  | `6.0`                |
| `Navigator/Greeking`              | Prefs  | `72.0`               |
| `text/sizeIncrement`              | Cloud  | `2.0`                |
| `text/riseIncrement`              | Cloud  | `1.0`                |
| `text/kernIncrement`              | Cloud  | `5`                  |
| `text/autoSizing`                 | Cloud  | `0`                  |
| `text/doFontLocking`              | Cloud  | `1`                  |
| `text/highlightMissingFonts`      | Cloud  | `1`                  |
| `AutoActivateMissingFont`         | Cloud  | `0`                  |
| `text/useEnglishFontNames`        | Cloud  | `0`                  |
| `text/recentFontMenu/showNEntries`| Cloud  | `10`                 |
| `RecentFont/RecentFontCount`      | Prefs  | `0`                  |
| `text/fontMenu/showInFace`        | Cloud  | `1`                  |
| `text/fontMenu/faceSizeMultiplier`| Cloud  | `1.0`                |
| `text/fontMenu/showSubMenusInFace`| Prefs  | `0`                  |
| `text/enableAlternateGlyph`       | Prefs  | `1`                  |
| `text/enableAutoFontDownloadForGenAI` | Prefs | `0`              |
| `text/enableListAutoDetection`    | Prefs  | `0`                  |
| `text/enablePreciseBBox`          | Prefs  | `0`                  |
| `text/showTextTouchUpButton`      | Prefs  | `0`                  |
| `dynamicspelling`                 | Prefs  | `0`                  |
| `showAsianTextOptions`            | Cloud  | `0`                  |
| `hyphenation/language`            | Cloud  | `0`                  |
| `fontHeightOption`                | Cloud  | `1`                  |
| `EnableActualPointTextSpaceAlign` / `EnableActualAreaTextSpaceAlign` | Cloud | `0` / `0` |

### [01.7]-[GPU_AND_PERFORMANCE]

```
Performance/EnableGPU_Ver19_2 = 1      Performance/GPUSupported      = 1
Performance/TurnOffGPUDueToCrash = 0   Performance/AutoSwitchEngine  = 1
Performance/AnimZoom = 1               Performance/ResponsiveZoom    = 1
Performance/FlickPan = 0
AIDisableAsyncRendering = 0            AIDisableAsyncRenderingForDX = 0
memory/physicalRAMSize = 65536  memory/memoryPercentage = 0  memory/usePhysicalRAMSize = 0
BIBCacheMaxSizeInBytes = 41943040  BIBCacheSizeInPercentOfPhysicalRam = 2.0
zoomWithMouseWheel = 1 (Cloud)   aiRotateWithTrackpad = 1   aiSwapAltControlWithScrollWheel = 0
DisableHoverScrollOnUnfocused = 1
```

### [01.8]-[FILE_HANDLING_AND_CLIPBOARD]

```
Ai_SaveBackupFiles = 1 (Cloud)         enableBackgroundSave = 1   enableBackgroundExport = 1
cloudAIEnableAutoSave = 1              cloudAIAutoSaveTimerInterval = 300
CrashRecovery/AutomaticallySave = 1    CrashRecovery/IdleLoopTimeInterval = 0
CrashRecovery/TurnOffForComplexDocument = 1
CrashRecovery/RecoveryFolderLocation = ~/Library/Preferences/Adobe Illustrator 30.9.0 Beta Settings/en_US/DataRecovery
aiFileFormat/PDFCompatibility = 1      aiFileFormat/enableContentRecovery = 0
RecentFileNumber = 20   maxBridgeMRUFiles = 30   DefaultSaveLocation = 0
useLowResProxy = 0      fileHandling/RenderLinksInLowRes = 0   showLinkInfo = 0
ReplacingLinks = 0      DontShowWarningAgain/applyAllLinkOption = 0
```

Clipboard (all in Cloud Prefs, `plugin/FileClipboard/`):

```
copyAsPDF = 1   copyAsAICB = 0   AICBOption = 1   flatten = 1
pasteWithoutFormatting = 1   linkoptions = 2
layers/pastePreserve = 0   layers/pastePreserveBackup = 0   (Prefs)
FileClipboard/copySVGCode = 1 (Prefs)
```

### [01.9]-[APPEARANCE_OF_BLACK]

```
blackPreservation/Onscreen = 0     # Display All Blacks Accurately
blackPreservation/Export   = 0     # Output All Blacks Accurately
```

Both off. Only these two keys exist; there is no third black-related key anywhere in either file.

### [01.10]-[UI_PANELS_TABS_TOOLTIPS_HOME_AI_COMMENTS]

The switches worth knowing, and their current state:

| [KEY]                                       | [FILE] | [VALUE] | [EFFECT]                                             |
| :------------------------------------------ | :----- | ------: | :--------------------------------------------------- |
| `Hello/ShowHomeScreenWS`                    | Prefs  | `0`     | **Home screen already disabled**                     |
| `ContextualTaskBarEnabled`                  | Prefs  | `1`     | Contextual task bar on                                |
| `ContextualTaskBar/Pinned`                  | Prefs  | `1`     | Pinned at H 329, V 873                                |
| `showToolTips`                              | Cloud  | `1`     | Tooltips on                                           |
| `showRichToolTips`                          | Cloud  | `1`     | Rich (animated) tooltips on                           |
| `showHelpBar`                               | Prefs  | `1`     | Help bar on                                           |
| `AgenticUI/ShowAgenticUIPanelPreference2`   | Prefs  | `0`     | AI Assistant panel hidden                             |
| `AgenticUI/AgenticPanelReopenNudgeApplied`  | Prefs  | `1`     | The nudge that reopens it has already fired once      |
| `AIMCPServer/ServerEnabled`                 | Prefs  | `1`     | **MCP server on** (see [03])                          |
| `AIMCPServer/ShowConnectionStatusOnHeader`  | Prefs  | `0`     | No MCP badge in the app bar                           |
| `AIAgenticSystem/ConsentSendDataToThirdParty` | Prefs | `1`    | Third-party model consent granted                     |
| `agentic/trustedFolders`                    | Prefs  | `()`    | Empty list                                            |
| `Workspaces/PrefCommentsPanelShowSetInWorkspace` | Prefs | `1` | Comments panel visibility is workspace-owned          |
| `Workspaces/PrefPropertiesPanelShowSetInWorkspace` | Prefs | `1` | Same for Properties                                 |
| `Workspaces/PrefDLPanelShowSetInWorkspace`  | Prefs  | `1`     | Same for Design Libraries                             |
| `uiBrightness`                              | Cloud  | `0.0`   | Darkest UI theme                                      |
| `uiCanvasIsWhite`                           | Cloud  | `0`     | Canvas matches UI brightness                          |
| `uiOpenDocumentsAsTabs`                     | Cloud  | `1`     | Documents as tabs                                     |
| `uiPersistDrawers`                          | Cloud  | `0`     | Panels do not auto-collapse-persist                   |
| `UIPreferences/scaleUI` / `snapUIScaleFactor` / `workspaceTabsSize` | Prefs | `1`/`1`/`1` | Large tabs               |
| `UIPreferences/appScaleFactor`              | Cloud  | `1.0`   |                                                       |
| `uiScrollButtonPosition`                    | Cloud  | `0`     |                                                       |
| `showArtboardLabelOnCanvas`                 | Cloud  | `1`     |                                                       |
| `showLockIcon`                              | Cloud  | `0`     |                                                       |
| `highContrastEnabled`                       | Prefs  | `0`     |                                                       |
| `betaWhatsNewShownVerBuildNum`              | Prefs  | `(30.9.0.66)` | What's New already shown for this build         |
| `aiShowSystemCompatibilityIssuesAtStartup`  | Prefs  | `1`     | Startup compatibility dialog on                       |
| `RecentColorsDiscoveryNotification`         | Prefs  | `1`     |                                                       |
| `FireflyBoardsNotificationShown`            | Prefs  | `0`     |                                                       |
| `generateView`                              | Prefs  | `1`     |                                                       |

The GenAI section (`GenAI/*`, 31 rows) is all usage counters at zero:
`ThirdPartyGenerationCountGPT4o`, `…Ideogram`, `…NanoBanana`, `…FireflyVector3/4`, `…GPT15`,
`ThirdPartyModelGenerativeCreditAccepted = 0`, `FireflyBoardsBlueDotShown = 0`,
`GenTraceOnboardingCompleted = 0`, `AutoSelectSeen = 0`.
`GenerativeText/DistinctActionsExecutedMask = 0`, `GenerativeTrace/ConceptToVectorUserDockState = 0`.

Onboarding counters, all already zero or dismissed: `S4RSharesheetOnboardingPref`,
`VectorEdgeOnboardingNudgePreference`, `ShouldShowDimensionOnboarding`,
`NumOfTimesSFROnboardingShownPref`, `PresetsOnboardingMsgPreference`, `dontShowAgainOnboarding`,
`Onboarding/CN/NumerOfTimesOnboardingShown`, `OnBoarding/PantoneLibOnBoardingShownCountAugust2023`,
`plugin/ToolIntroFTUEShown`, `kToolBarDrawerIntroShown`, `DesignLibraryFTUEShown`.

No `Comments` key exists in either preference file. Comments panel visibility is stored per
workspace (`ShowCommentsPanelBool 0` inside the workspace collection attributes, see [01.13]).

### [01.11]-[PLUGINS_AND_SCRIPTS_PATHS]

```
checkRequiredPlugins = 1
loadAdditionalPlugins = 0            # no additional plug-in folder configured
loadPluginsFromOutsideAppPackage = 1
pluginCacheCheckCreateDate = 2587878772   pluginCacheCheckModDate = 2587878772
```

**There is no scripts-path preference of any kind in either file.** See [05].

### [01.12]-[NEW_DOCUMENT_PROFILES]

`PresetDocumentProfileDataV10.json` — 150 204 B, a JSON **array of 141 entries**, all factory.
Full table in `<S>/new-document-profiles.txt`. Distribution:

| [presetSource] | [COUNT] | [MODE] | [PPI] | [rasterEffectSettings] | [settingsFile]      |
| :------------- | ------: | :----- | ----: | ---------------------: | :------------------- |
| print          | 24      | CMYK   | 72    | 300 (Print-Large 36)   | `Print.ai`           |
| web            | 14      | RGB    | 72    | 72                     | `Web.ai`             |
| film           | 16      | RGB    | 72    | 72                     | `Film & Video.ai`    |
| social         | 21      | RGB    | 72    | 72                     | `Social.ai`          |
| mobile         | 37      | RGB    | 72    | 72                     | `Mobile.ai`          |
| art            | 15      | RGB    | 72    | 72 (varies)            | `Art & Illustration.ai` |
| branding       | 14      | RGB/CMYK | 72  | 300                    | `Branding.ai`        |

Each entry carries `appSpecificKey` (`width`, `height`, `ppi`, `numArtboards`, `artboardSpacing`,
`bleedOffset`, `fill: "transparent"`, `transparencyGrid`, `gridColor1/2`, `previewMode`,
`mode`, `rasterEffectSettings`, `settingsFile`) plus `title`, `description` (the true geometry, e.g.
`A4` = `595.28 x 841.89 pt`, `A1` = `1683.78 x 2383.94 pt`, `4K UHD` = `3840 x 2160 px`), `units`,
`thumbnail_url`, `template_category`.

**Two anomalies.**

1. `appSpecificKey.width/height` is stale for the print family — every print row reads
   `612.0 x 792.0`. The real size is only in `description` and is re-derived at dialog time.
2. `settingsFile` for `print_*`, `web_*` (partially) and others points at the **release** install
   path `~/Library/Application Support/Adobe/Adobe Illustrator 30/en_US/New Document Profiles/…`,
   not the Beta path. The same mismatch shows in the root prefs: `startupFileType_5/_6/_7` reference
   `Adobe Illustrator 30/…` while `startupFileType`, `startupFileType_1`, `startupFileType_2`
   reference `Adobe Illustrator 30.9.0 Beta Settings/…`. The JSON is dated Aug 19 (the release
   install), the Beta never rewrote it.

`~/Library/Application Support/Adobe/Adobe Illustrator 30.9.0 Beta Settings/en_US/New Document Profiles/`
holds exactly the seven factory profiles (`Art & Illustration.ai`, `Branding.ai`, `Film & Video.ai`,
`Mobile.ai`, `Print.ai`, `Social.ai`, `Web.ai`), all dated 2026-09-10 21:49, byte-identical in size
to the release copies. **No user profile has been installed** — `My Default Profile.ai` is not there.

### [01.13]-[WORKSPACES]

Format: the same CR-terminated `/key value` tree as the prefs, **not** XML at the top level. The
actual dock layout is an XML document stored as a hex blob under `/collection1/attributes/OWLBookMark`.
Decoded copies are `<S>/*.OWLBookMark.xml`; the parsed trees are in `<S>/workspace-layout.txt`.

| [FILE]                                       | [SIZE] | [SHA-256 prefix] |
| :------------------------------------------- | -----: | :--------------- |
| `Workspaces/Default Workspace`               | 43 944 | `76d87b6c…`      |
| `Modified Workspaces/Default Workspace`      | 43 946 | `789e496a…`      |
| `Modified Workspaces/Essentials`             | 51 633 | —                |
| `Modified Workspaces/Getting Started`        | 51 130 | —                |
| `Modified Workspaces/Layout`                 | 50 094 | —                |

`Workspaces/` holds the saved definition; `Modified Workspaces/` holds the live, dirty copy of each
workspace the session has touched. The two Default Workspace copies differ by 2 bytes.

Default Workspace layout (`OWLBookMark`, 17 424 B of XML):

- `dock anchor=top content=control-bar` — one `control-bar` id 1, size `1783 31`, origin `11 40`, **closed**
- `dock anchor=bottom content=control-bar` — empty
- `dock anchor=left content="palette toolbar"` — four toolbars: `Getting Started` (closed),
  `Basic` (closed), `ToolBar` (closed), **`Default Toolbar` (open)** — the user's custom toolbar is
  the active one
- `dock anchor=right content="palette toolbar"` — two tab-panes of tab-groups holding 52 palettes

32 palettes open, 20 closed:

```
OPEN   CSXSExtension_com.adobe.DesignLibraries.angular_0, Transform, Character, Paragraph,
       AdobeOpenTypeFont, AdobeParaStyles, AdobeCharStyles, AdobeAltGlyphsPanel, AdobeBrush,
       NamedStyle, Symbols, PatternOptionsPanel, Vectorize, 3D and Materials, Align, Pathfinder,
       AdobePropertiesPanel, Appearance, Artboards, Links, AdobeSwatch_, Color,
       AdobeGradientEditor, Actions, History, AdobeLayerPalette, Export, Document Info,
       Navigator, Info, AdobeSeparationPreview, AdobeObjectAttributes

CLOSED AdobeBlendOptionsPanel, Stroke, Transparency, AI Assistant,
       Concept to Vector Unified Panel, BetaFeedbackPanel, Retype, Reflow Viewer,
       GeneratedVariations, CSS, Mockup, ccx-timeline, AdobeFlatteningPreview, SVGInteractivity,
       AdobeVariablesPanel, AdobeHorizontalTabPanel, AdobeVerticalTabPanel, Magic Wand,
       AdobeColorGuidePanel, CCX Commenting UXP Webview
```

`AI Assistant`, `Concept to Vector Unified Panel`, `GeneratedVariations`, `BetaFeedbackPanel` and
`CCX Commenting UXP Webview` are already closed in this workspace.

The `collection1/attributes` block also carries per-panel view state that is workspace-scoped, not
pref-scoped, including `AgentsUIPanelClosedByUser 1`, `ShowCommentsPanelBool 0`,
`ShowDLPanelBool 0`, `ShowPropertiesPanelBool 0`, `DockStarterToolbar 0`,
`Default Toolbar NumTearoffs 0`, and a long list of `ControlPanel OI*V` control-bar item switches.

### [01.14]-[TOOLS_PANEL_PRESETS]

Format: same `/key value` tree, CR-terminated, readable without `strings`.

`~/…/Tools/Tools Panel Presets` (6 315 B) holds **one** collection, `NumberOfCollections 1`,
`CatalogName (Adobe Custom Toolbar)`, collection name `Default Toolbar`, `canEdit 1`, `canDelete 1`.
It defines **30 top-level slots** (`CustomToolboxItem0..29`), several of which are flyout sets
(`CustomToolboxItemSet0..22`), plus `CustomToolboxFillStroke 1`, `CustomToolboxColorMode 0`,
`CustomToolboxDrawMode 1`, `CustomToolboxScreenMode 0`, `CustomToolboxGroupingMode 0`.

Slot order, top to bottom (flyout members in brackets):

1. [Direct Select, Direct Object Select, Direct Lasso, Magic Wand]
2. [Select, Crop]
3. [Scroll, Zoom, Rotate Canvas, Page]
4. [Type, Area Type, Path Type, Vertical Type, Vertical Area Type, Vertical Path Type, Touch Type]
5. [Dimension, Measure]
6. Eyedropper
7. [Slice, Slice Select]
8. [Eraser, Scissors, Knife]
9. [Freehand Smooth, Freehand Erase]
10. Corner Join
11. Reshape
12. [Free Transform, Puppet Warp]
13. [Rotate, Reflect]
14. [Scale, Shear]
15. [Shape Builder, Planar Paintbucket, Planar Face Select]
16. [Width, Warp, New Twirl, Pucker, Bloat, Scallop, Crystallize, Wrinkle]
17. [Blend, Constraints]
18. Mesh Editing
19. Gradient Vector
20. [Flare, Symbol Sprayer/Shifter/Scruncher/Sizer/Spinner/Stainer/Screener/Styler]
21. [Shaper, Freehand]
22. [Brush, Blob Brush]
23. [Curvature, Pen, Add Anchor Point, Delete Anchor Point, Anchor Point]
24. [Line, Rectangle, Ellipse, Rounded Rectangle]
25. [Regular Polygon, Star]
26. Arc
27. Spiral
28. [Column, Stacked Column, Bar, Stacked Bar, Line, Area, Scatter, Pie, Radar graph]
29. [Rectangular Grid, Polar Grid]
30. [Perspective Grid, Perspective Selection]

The factory file at `/Applications/Adobe Illustrator (Beta)/Presets.localized/en_US/Tools/Tools Panel Presets`
is a different object: 70 `CustomToolboxItem*` rows across several collections (`collection3` =
`Getting Started` with `ToolGroup0 (SELECT)` / `ToolGroup4 (SHAPES)` group headers, plus `Basic` and
`ToolBar`). The user's file has no `ToolGroup*` rows at all — it is a flat custom bar.

### [01.15]-[DRIVE_COPIES_COMPARED]

| [PAIR]                                                                                     | [RESULT] |
| :----------------------------------------------------------------------------------------- | :------- |
| Drive `99.Default Profiles/00.Adobe Illustrator/Default Toolbar` ↔ local `Tools/Tools Panel Presets` | **Identical**, `386fdd17cd9ac952fe4cc662bd625371c42418381b3acd3829c3d8c5af880e1b` |
| Drive `99.Default Profiles/00.Adobe Illustrator/Default Workspace` ↔ local `Workspaces/Default Workspace` | **Identical**, `76d87b6ce658079120f32da54ebdfd6a45c7249921148cd639e8f0b216fd76ce` |
| local `Workspaces/Default Workspace` ↔ local `Modified Workspaces/Default Workspace`        | Differ (`789e496a…`), 2 bytes larger — the live session copy |
| local `Tools/Tools Panel Presets` ↔ app factory `Tools/Tools Panel Presets`                | Differ (`07a85a4b…`) — factory has 70 items in 3 collections |

Both Drive files are dated 2026-09-11 16:11, which is after the Beta session started; they are a
current backup, not a stale one. The Drive folder also holds `AI28Settings_Nov 19, 2024_13 56`
(82 234 851 B), a full CC settings export from Illustrator 28, and a `rebuild/` directory.

---

## [02]-[DOCUMENT_DUMPS]

Method: one ExtendScript file (`<S>/dump.jsx`), driven through
`osascript -e 'tell application id "com.adobe.illustratorBeta" to do javascript (POSIX file …)'`,
reading its job parameters from `<S>/job.txt`. It sets
`app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS` before opening, and ends every
run with `doc.close(SaveOptions.DONOTSAVECHANGES)`. Every accessor is wrapped so one missing
property cannot abort a dump.

**No document was slow and none hung.** Open times: (a) 0.001 s, (b) 1.316 s, (c) 1.287 s,
(d) 0.893 s; total dump wall time for all three large files, 4 seconds. The whole run log is
`<S>/run-dumps.log`.

### [02.0]-[SUMMARY_TABLE]

| | (a) `default-template.ai` | (b) Drive root `My Default Profile.ai` | (c) `99.Default Profiles` fork | (d) `My Color Palette.ai` |
| :--- | ---: | ---: | ---: | ---: |
| Path | `~/Library/Application Support/design-tools/setup/illustrator/` | `…/03.Digital Asset Database/` | `…/05.Software Related Assets/99.Default Profiles/00.Adobe Illustrator/` | `…/05.Software Related Assets/02.Color Swatches/` |
| Size / date | 141 959 B, 2026-09-11 16:05 | 65 165 231 B, 2025-03-09 | 65 175 370 B, 2025-01-28 | 41 485 537 B, 2025-02-28 |
| Color space | RGB | RGB | RGB | RGB |
| Profile | **Adobe RGB (1998)** | sRGB IEC61966-2.1 | sRGB IEC61966-2.1 | sRGB IEC61966-2.1 |
| Ruler units | Pixels | Pixels | Pixels | Pixels |
| Artboards | 1 — `Artboard 1`, 3000 × 4000 pt | 1 — `Artboard 1`, 1224.57 × 790.87 | 1 — same | 1 — `v`, 1444.09 × 1289.89 |
| Raster effects | **72 ppi**, anti-alias **on**, padding **0** | **300 ppi**, anti-alias off, padding 36 | 300 ppi, off, 36 | 300 ppi, off, 36 |
| `outputResolution` | 800 | 800 | 800 | 800 |
| Swatches | 94 (3 groups) | 94 (3 groups) | 94 (3 groups) | **143 (5 groups)** |
| Spots (DOM) | 93 | 80 | 80 | 116 |
| Gradients | 0 | 0 | 0 | **8** |
| Patterns | 0 | 1 | 1 | **6** |
| Brushes | 7 | **502** | 502 | 302 |
| Symbols | **0** | 167 | 167 | **224** |
| Graphic styles | **1** (`[Default]`) | 16 | 33 | **38** |
| Character styles | 1 | 1 | 1 | 1 |
| Paragraph styles | **4** (`Body`, `Heading`, `Caption`) | 1 | 1 | 1 |
| Layers | 1 (`Layer 1`) | 2 (`Layer`, `Background` locked) | 2 (`Layer 1`, `Guides & Grids` locked) | 1 (`Layer 1`) |
| Page items | 0 | 1 | 0 | 312 |
| Text frames | 0 | 0 | 0 | 79 |

`document.stationery` is `false` on all four — none of them is marked as a template.

### [02.1]-[THE_CENTRAL_FINDING]

**The library the user is looking for is not in `My Default Profile.ai`. It is in
`My Color Palette.ai`.**

- Symbols: (b) and (c) hold **167** symbols, byte-for-byte the same list. (d) holds **224** and is a
  **strict superset** — every one of the 167 is present plus 57 more.
- Swatches: (b)'s 94 swatch names are a **strict subset** of (d)'s 143. (d) adds the `Pastel Colors`
  group (26) and `SIteplan Color Scheme 1` (9) plus the gradient and pattern swatches.
- Gradients and patterns exist only in (d).
- Graphic styles are the one place where neither file dominates. (b) ⊂ (c) exactly. Between (c) and
  (d):
  - (c) only (11): `Arrow Pointer Style`, `Digi Line Style`, and the whole `Tech Pen` family —
    `Fine`, `Blotting Paper`, `Rough Paper`, `Hand Drawn`, `Worn Nib`, `Worn Nib, Hand Drawn`,
    `Blotched`, `Ink Splatters`, `Paper Bleed Fill`
  - (d) only (16): `Primary Arrow Style`, `Site Boundary Style`,
    `Existing/Irrelevant Structures Fill`, `Green Space Fill Style`, `Noise - Line Style`,
    `Vehicle Line Path Style`, `Cyclist Line Path Style`, `Pedestrian Line Path Style`, and the
    utilities set `Water/Power/Gas/Sewer - Dotted Line` + `… - Fill Style`
  - **Union = 49 named graphic styles** (plus `[Default]`)
- Brushes: (b)/(c) 502, (d) 302, neither a subset. **Union = 582**.

The four categories the user named map cleanly onto (d)'s style list: annotation
(`Annotation - Open/Closed Circle/Square`), site (`Site Boundary`, `Green Space Fill`,
`Existing/Irrelevant Structures Fill`, `Topography Line`, `Noise - Line`), utilities
(water/power/gas/sewer), roads (`1 Lane` through `4 Lanes v5`, 14 styles).

The symbol families the user named are all in (d): number and letter variants
(`Legend - 1..15`, `Legend - A..O`, `Legend - R1..R15`, and the `Legend T - …` text variants),
tree markers (`White - Tree - 1..18` and `Transparent - Tree - 1..18`, 36 symbols),
site analysis (`North Arrow - S1..S5`, `Site Marker`, `Sight Line Eye`, `Sight Line View Angle`,
`Combined Sight Line`, `Noise Hot Spot`, `Flooding Indicator`, `Prevailing Winds Indicator`,
entry points, `Gate Symbol`, `Elevation Change Symbol`), utilities (`Fire Hydrant`,
`Electrical Substation`, `Water Meter/Connection`, `Manhole`), transport and POI
(stations, stops, `Parking Lot/Garage`, restaurants, schools, hospitals, `Charging Station (Car/Bike/Scooter)`, …),
pins (`Pin - Black` … `Pin - Purple`, 12), crosshairs (`Circle Crosshair`, `Crosshair - 1..15`),
layer icons (`Base Layer Icon`, `2nd/3rd/4th Layer Icon`), `Call-Out Box`.

Note four symbols named just `L`, `M`, `N`, `O` in all three files — these should be
`Legend T - L/M/N/O` and were renamed by accident. Same bug in (d).

### [02.2]-[COLOR_DRIFT_BETWEEN_COPIES]

Same swatch name, different RGB per file. The values in (d) are the canonical ones; (b) has drifted
and (a) has drifted further.

| [SWATCH]          | (d) `My Color Palette.ai` | (b) `My Default Profile.ai` | (a) Codex template + `Default Palette.ase` |
| :---------------- | :------------------------ | :-------------------------- | :----------------------------------------- |
| Aquamarine        | 127, 255, 212 (canonical) | 153, 212, 191               | 171, 211, 191                               |
| Amethyst          | 153, 102, 204 (canonical) | 142, 104, 173               | 131, 104, 169                               |
| Apple Green       | 141, 182, 0   (canonical) | 141, 183, 63                | 153, 182, 75                                |
| Aerospace Orange  | 255, 79, 0    (canonical) | 240, 82, 35                 | 209, 83, 43                                 |
| Avocado           | 86, 130, 3    (canonical) | 86, 131, 59                 | 101, 130, 66                                |
| Black Bean        | 61, 12, 2     (canonical) | 58, 20, 14                  | 54, 27, 22                                  |
| Beige             | 245, 245, 220 (canonical) | 245, 244, 220               | 244, 244, 220                               |

The pattern is a repeated profile conversion — saturated colors get pulled toward the middle each
round trip. (a) is one generation further from (d) than (b) is. **If the Codex template's palette
was built from anything other than `My Color Palette.ai`, it inherited the drift.**

Two further differences in the Codex template's palette:

- Group names were renamed: `Base Swatch` → `Default Palette / Base`,
  `Expanded Colors` → `Default Palette / Expanded`.
- Names were changed: `White`/`Black` → `RGB White`/`RGB Black`, `Beige` → `Base Beige`,
  and `Turqoise` (the original typo) → `Turquoise`. Also `Salmon Pink`/`Bittersweet` and
  `Desert Sand` sit in a different order.
- `Bone`, `Almond`, `Camel`, `Caramel`, `Canary`, `Caribbean Current`, `Byzantium` and others all
  moved a few units.

### [02.3]-[PER_LAYER_ARTWORK]

**(b) Drive-root `My Default Profile.ai`** — 65 MB and essentially empty of artwork:

| [LAYER]      | [ITEMS] | [LOCKED] | [VISIBLE] | [CONTENT] |
| :----------- | ------: | :------- | :-------- | :-------- |
| `Layer`      | 0       | no       | yes       | empty     |
| `Background` | 1       | **yes**  | yes       | one `PathItem` |

All 65 MB is the swatch, brush, symbol and style tables, not drawn artwork. The `Guides & Grids`
layer that (c) has was renamed to `Background` and its guide geometry replaced by one path.

**(c) `99.Default Profiles` fork** — `Layer 1` (0 items) and `Guides & Grids` (0 items, locked).
Zero page items in the whole document.

**(d) `My Color Palette.ai`** — one unlocked `Layer 1` with 87 top-level items:
75 `GroupItem`, 8 `PathItem`, 4 `TextFrame`; 312 page items and 79 text frames in total,
158 paths, 75 groups, no placed or raster items. The groups sit on a regular 110 × 160 pt grid from
about x = −490 to x = 930 and y = 490 down to y = −780 — a specimen sheet where each 100 × 150 cell
shows one swatch or symbol, with a text label beside it. No artwork is named; every `name` is empty.
Full per-item bounds are in `<S>/d-my-color-palette.json` under `layers[0].items`.

### [02.4]-[TEXT_STYLES]

Only (a) carries real paragraph styles:

```
[Normal Character Style]  SourceSans3-Regular  48 / 64
[Normal Paragraph Style]  SourceSans3-Regular  48 / 64
Body                      SourceSans3-Regular  48 / 64
Heading                   SourceSans3-Bold     96 / 112
Caption                   SourceSans3-Regular  36 / 48
```

(b), (c), (d) each hold only `[Normal Character Style]` (`MyriadPro-Regular`, 12 / 27) and a
`[Normal Paragraph Style]` whose attributes are undefined (`textFont` throws `null is not an
object`) — the style exists but was never given a font. That is worth knowing before copying these
documents forward.

### [02.5]-[PATTERN_CRASH]

**Not reproduced.** All six patterns in (d) (`Horizontal 3`, `New Pattern 20`, `Simple Grass 1`,
`Straight Lines - Diagonal`, `Unnamed Pattern 2`, `Water 9`) and the one in (b)/(c)
(`Unnamed Pattern 2`) enumerated cleanly through `document.patterns` and `document.swatches`, and
all three documents closed without incident. The dump did not open the Pattern Options panel or
enter pattern-edit mode, which is where the reported crash would live. The prefs do carry a full
`PatternOptionsPanel` view-state block inside the workspace attributes, so the panel has been used.
One run only, no retry, as instructed.

---

## [03]-[MCP_ENDPOINT]

### [03.1]-[CONFIG]

`~/.codex/config.toml` lines 207-210:

```toml
[mcp_servers.adobe-illustrator]
url = "http://localhost:18412/v1/mcp"
enabled = false
http_headers = { Authorization = "Bearer <token>" }
```

The block is **disabled** in Codex (`enabled = false`). The token there matches
`AIMCPServer/AuthToken` in the Illustrator prefs exactly.

### [03.2]-[WHERE_ILLUSTRATOR_STORES_THE_SETTING]

In `~/Library/Preferences/Adobe Illustrator 30.9.0 Beta Settings/en_US/Adobe Illustrator Prefs`,
flat rows 1529-1533 of `<S>/prefs-flat.txt`:

```
AIMCPServer/ServerEnabled                  = 1
AIMCPServer/AuthToken                      = [len=69] ilst_<64 hex chars>
AIMCPServer/ShowConnectionStatusOnHeader   = 0
AgenticUI/ShowAgenticUIPanelPreference2    = 0
AgenticUI/AgenticPanelReopenNudgeApplied   = 1
```

plus `AIAgenticSystem/ConsentSendDataToThirdParty = 1` and `agentic/trustedFolders = ()`.

**The port 18412 is not stored anywhere.** It does not appear in either preference file, nor in the
main executable's strings. It is compiled into the plug-in.

Implementation lives in
`/Applications/Adobe Illustrator (Beta)/Adobe Illustrator.app/Contents/Required/Plug-ins/Extensions/AIAgenticSystem.aip/Contents/MacOS/AIAgenticSystem`
— the only binary in the bundle containing the literal `v1/mcp`. The `AIMCPServer` pref key is read
by three binaries: that plug-in, `AppBarControls.aip`, and `IllustratorUI.aip` (the last two for the
header status badge).

### [03.3]-[HANDSHAKE]

`initialize` over plain HTTP POST with `Accept: application/json, text/event-stream` returned
`200 OK`, `Content-Type: application/json` (no SSE needed) and an `Mcp-Session-Id` header:

```json
{"protocolVersion":"2025-03-26",
 "serverInfo":{"name":"Adobe Illustrator","version":"1.0.0"},
 "capabilities":{"tools":{}},
 "instructions":"Bounds arrays: fields named bounds or canvas_bounds are [left, top, right, bottom]
 in canvas-global points (Y-down) … The bleedInsets field is [left, top, right, bottom] per-side
 margin insets in points—not canvas corner coordinates …"}
```

Only `tools` is advertised — no resources, no prompts, no sampling.

### [03.4]-[TOOLS]

`tools/list` returned **47 tools**, 108 718 B of JSON, 48 365 B of description text. Full schemas in
`<S>/mcp-tools.json`. **No tool was called.**

Read-only (16): `ListArtboards`, `GetActiveArtboard`, `GetArtboardProperties`, `GetCanvasStructure`,
`GetArtboardStructure`, `VisualizeSelection`, `ListDocuments`, `CapturePreview`, `FindObjects`,
`GetVisualAppearance`, `GetGeometry`, `GetTypographyMetrics`, `GetObjectStructure`, `GetBounds`,
`RunPreflightChecks`, `GetSwatches`.

Mutating (31): `AlignObjects`, `DistributeObjects`, `SetAppearance`, `CreateArtboard`,
`SetArtboardProperties`, `DuplicateArtboard`, `ScaleArtboards`, `FitArtboard`, `DeleteArtboard`,
`SetActiveArtboard`, `MoveArtboards`, `CreateDocument`, `OpenDocument`, `SwitchDocument`, `Export`,
`Vectorize`, `CreateLayer`, `CleanupPath`, `RenameObject`, `SelectObjects`, `DeleteObjects`,
`DuplicateObjects`, `CreateGroup`, `MoveObjectsToContainer`, `CreateClippingMask`, `ArrangeArt`,
`ReplaceText`, `ReplaceFont`, `MoveObjects`, `RotateObjects`, `ScaleObjects`.

Schema shape: every tool takes a flat object; object-addressing tools take `uuids` as an **array**
even for one object (`RenameObject` requires `uuids` + `newNames`, `SelectObjects` takes
`uuids` + `select`). Artboard tools key off `artboardIndex`, then `artboardName`, then the active
artboard. Coordinates are canvas-global points, Y-down, `[left, top, right, bottom]`.

Notable for this project: **`GetSwatches` reads any swatch library, not only the open document**
(«Read available swatches from the active document's Swatches panel *or any swatch library*»), and
`RunPreflightChecks` does a single-traversal production QA pass over the art tree. `Export` can write
a file, return a stream, **or upload to an Adobe-hosted URL**. `Vectorize` runs Image Trace.

The `.runConfig` feature flags gate two of these: `EnableVectorizeMCPTool: true` and
`EnableGenAIConceptToVectorMCPTool: false`.

---

## [04]-[NATIVE_RESOURCES]

### [04.1]-[FACTORY_LIBRARY_INVENTORY]

`/Applications/Adobe Illustrator (Beta)/Presets.localized/en_US/` — full recursive listing in
`<S>/native-presets.txt`.

| [FOLDER]            | [SUBDIRS] | [FILES] | [TOP-LEVEL CONTENT] |
| :------------------ | --------: | ------: | :------------------ |
| `Brushes`           | 10        | 38      | `Bristle.ai`, `Calligraphic.ai`, `Dry Media.ai`, `Engraving.ai`, `Images.ai`, `Ink.ai`, `Paint.ai`, `Pressure sensitive.ai`, `Stipple.ai`, `Watercolor.ai`, plus `Brushes (Classic)/` and `Ornamental/` |
| `Swatches`          | 15        | 118     | `Celebration.ase`, `Corporate.ase`, `Earthtone.ase`, `Kids Stuff.ase`, `Metal.ase`, `Neutral.ase`, `Skintones.ase`, `Textiles.ase`, `System (Macintosh).ai`, `System (Windows).ai`, `VisiBone2.ai`, `Web.ai`, plus `Art History/`, `Color Books/`, `Color Properties/`, `Default Swatches/`, `Foods/`, `Gradients/`, `Nature/`, `Patterns/`, `Scientific/` |
| `Symbols`           | 0         | 28      | `3D Symbols.ai`, `Arrows.ai`, `Artistic Textures.ai`, `Celebration.ai`, `Charts.ai`, `Communication.ai`, `Dot Pattern Vector Pack.ai`, `Fashion.ai`, `Florid Vector Pack.ai`, `Flowers.ai`, `Grime Vector Pack.ai`, `Hair and Fur.ai`, `Heirloom.ai`, `Illuminate Flow Charts.ai`, `Illuminate Org Charts.ai`, `Illuminate Ribbons.ai`, `Indigenous.ai`, `Logo Elements.ai`, `Mad Science.ai`, `Maps.ai`, `Mobile.ai`, `Nature.ai`, `Regal Vector Pack.ai`, `Retro.ai`, `Sushi.ai`, `Tiki.ai`, `Web Buttons and Bars.ai`, `Web Icons.ai` |
| `Graphic Styles`    | 1         | 21      | `Additive.ai`, `Additive for Blob Brush.ai`, `Bevel-and-emboss.ai`, `Dimension-and-depth.ai`, `Drop-shadow.ai`, `Light-and-glow.ai`, `Materials.ai`, `Outer-glow.ai`, `Special-effects.ai`, `Stickers.ai`, `Texture.ai`, plus `Graphic Styles (Classic)/` |
| `Actions`           | 0         | 1       | `Default_Actions.aia` |
| `Scripts`           | 0         | 3       | `ImageTracing.jsx`, `SaveDocsAsPDF.jsx`, `SaveDocsAsSVG.jsx` |
| `Workspaces`        | 0         | 12      | `Automation`, `Essentials`, `Essentials Classic`, `Essentials Classic@2x`, `Getting Started`, `Layout`, `Painting`, `Printing and Proofing`, `Start`, `Tracing`, `Typography`, `Web` |
| `Tools`             | 0         | 1       | `Tools Panel Presets` |
| `Keyboard Shortcuts`| 0         | 1       | `Illustrator Defaults.kys` |

Every factory library is currently non-persistent — the prefs hold 124 rows under
`Presets.localized/…` and **every `Persistent` value is `0`**. None of them is pinned to reopen.

### [04.2]-[USER_LIBRARIES_ARE_READ]

The folder-name string table in the main binary (and in `AILib.framework`) confirms Illustrator
resolves a **second, user-writable** preset root alongside the app one:

```
$$$/kAIFolderFolderNameStrings/UserWritablePresetBrushes/Win=Brushes
$$$/kAIFolderFolderNameStrings/UserWritablePresetStyles/Win=Graphic Styles
$$$/kAIFolderFolderNameStrings/UserWritablePresetSwatches/Win=Swatches
$$$/kAIFolderFolderNameStrings/UserWritablePresetSymbols/Win=Symbols
$$$/kAIFolderFolderNameStrings/UserWritablePresetColorTables/Win=Color Tables
$$$/kAIFolderFolderNameStrings/UserWritablePresetOptimize/Win=Optimize
$$$/kAIFolderFolderNameStrings/UserWritablePresetOutputSettings/Win=Output Settings
$$$/kAIFolderFolderNameStrings/UserWritablePresetSaveForWebSettings/Win=Save for Web Settings
$$$/kAIFolderFolderNameStrings/UserWritableUserMockup/Win=User Mockup Templates
```

That root is
`~/Library/Application Support/Adobe/Adobe Illustrator 30.9.0 Beta Settings/en_US/`, and the
installer created the four folders on 2026-09-10 21:56:

| [FOLDER]         | [CONTENT]                            |
| :--------------- | :----------------------------------- |
| `Swatches/`      | `Default Palette.ase`, 4 412 B, written **2026-09-11 16:23** |
| `Brushes/`       | empty                                |
| `Symbols/`       | empty                                |
| `Graphic Styles/`| empty                                |

**`Default Palette.ase`** parses (`<S>/default-palette-ase.txt`) as ASE version 1.0, 96 blocks,
**92 colors** in two groups — `Default Palette / Base` (13) and `Default Palette / Expanded` (79) —
all RGB, all flagged **global** (not spot). Its values match the Codex template exactly, which means
it carries the same drift described in [02.2]. It is the only user library currently installed, and
`Window > Swatch Libraries > User Defined` will show exactly one entry.

So: **dropping `.ai` files into those four folders is the supported way to add user libraries**, and
the three empty ones are free to fill.

### [04.3]-[WRITABILITY_AND_SIGNATURE]

```
drwxr-xr-x  root  admin  /Applications/Adobe Illustrator (Beta)/Presets.localized
drwxr-xr-x  root  admin  /Applications/Adobe Illustrator (Beta)/Presets.localized/en_US
drwxr-xr-x  root  admin  …/en_US/Symbols   …/en_US/Swatches   (all 0755 root:admin)
```

Not writable by the user. `sudo` would work; SIP is enabled but does not protect `/Applications`.

Signature:

```
Identifier      = com.adobe.illustratorBeta
Format          = app bundle with Mach-O thin (arm64)
Authority       = Developer ID Application: Adobe Inc. (JQ525L2MZD)
                  Developer ID Certification Authority / Apple Root CA
TeamIdentifier  = JQ525L2MZD
Timestamp       = Sep 8, 2026 13:48:05
Notarization    = stapled
CDHash          = 7b249d5284d524834a40b3ce148d090aa2fc5962
flags           = 0x10000 (runtime)     Sealed Resources version 2, rules=13, files=19836
```

**`Presets.localized` sits outside the signed bundle.** The signed object is
`/Applications/Adobe Illustrator (Beta)/Adobe Illustrator.app`; `Presets.localized`, `Support Files`,
`Plug-ins.localized`, `Cool Extras.localized` and `Scripting.localized` are siblings of it, not
inside it. The 19 836 sealed files are the `.app`'s own resources. So deleting factory libraries
would **not** break the code signature.

It would, however, be reverted. Every Beta build is a full reinstall into the same directory, and
these folders are all dated 2026-09-10 21:49 — the install timestamp. Any deletion survives until
the next Beta drop and no longer.

### [04.4]-[HIDDEN_FEATURE_SWITCHES]

142 `Enable*` / `Show*` / `Disable*` keys are in the main binary (`<S>/binary-feature-keys.txt`).
The ones matching the requested search terms, spelled exactly as they appear:

| [SEARCH TERM]      | [KEYS FOUND IN BINARY] |
| :----------------- | :--------------------- |
| Generative         | `GenerativeText`, `GenerativeTrace`, `GenerativeTraceOnboarding`, `GenerativeTraceFTUE`, `GenerativeTraceApproach2`, `GenerativeTraceControlBarPresetPicker`, `GenerativeExpand`, `GenerativeTextUI`, `GenAI/ThirdPartyModelGenerativeCreditAccepted`, `ImageGenerativeTraceButtonAction`, `Generative Remove Background` |
| Assistant          | `AI Assistant`, `AdobeIllustratorAssistantGenAIFeatures` |
| Home               | **`Hello/ShowHomeScreenWS`**, `Show HomeScreen Preference`, `Home screen visibility changed`, `Home screen file list changed` |
| Discover           | **`EnableDiscoverButtonInAppBar`**, `Discover Panel`, `DiscoverBtn`, `Discover_1`, `Discover_2`, `Show Discover Panel Template`, `Start Discover Panel Tour`, `LockDiscoveryNotification`, `ImageTraceDiscovery`, `BulkRenameDiscovery`, `ArtboardBackgroundColorDiscovery`, `LockArtboardsDiscovery` |
| Learn              | `Learn More`, `NewMarqueeLearnMoreButtonClicked`, `TextFormattingLearnMoreButtonClicked` (no pref switch) |
| Comments           | `allowComments`, `collectComments` (both PDF-export keys, not UI) |
| ContextualTaskBar  | Not in the main binary as a pref key — the live pref is `ContextualTaskBarEnabled` in the Prefs file; the class names are `UIContextualTaskBarContentView`, `ContextualDirectSelectionTaskBarUI`, `ContextualDirectSelectionTaskBarControlGroup`, `AI Contextual Taskbar Utilities Private Suite` |
| Tooltip            | **`DisableRichTooltips`**, `TurntableOnboardingBypassTooltipPrefs`, `AI Tool Tooltip Changed Notifier` (live prefs: `showToolTips`, `showRichToolTips` in Cloud Prefs) |
| SplashScreen       | Only C++ symbol names (`CAISplashScreenNotifier…`) — no pref key |
| WhatsNew           | `WhatsNew` (live pref: `betaWhatsNewShownVerBuildNum`) |
| StatusBar          | `StatusBarView`, `StatusBarCanvasRotation`, `TouchStatusBarUI` — no pref key |
| Ruler              | **`globalRulersVisible`**, **`isRulerIn4thQuad`**, `isRulerOriginTopLeft`, `AlternatePatternStyleTransformOnRulerOriginChange`, `ElixirInstantSaveArtboardRulerOriginTracking`, `Artboard Ruler visible`, `Ruler Guides`, `Ruler Menu`, `Ruler Inches/Millimeters/Centimeters/Meters/Feets` |

The agentic switches, which matter most here:

```
EnableAgentic                     EnableAgenticUX
EnableAgenticUXPPanel             EnableAgenticOnboardingExperience
ShowAgenticUIPanel                HideGenAIObjectAnnotations
EnableFireflyBoardsInterop        EnableFireflyWorkspaceExtension
EnableGenerateWithFireflyEntryPoints                EnableEditImageFireflyWorkspace
EnableDiscoverButtonInAppBar      ShowLayersPanelAddAssetFTUE
ShowSystemCompatibilityIssuesAtStartup
```

**These are not preferences — they are server-delivered feature flags.**
`~/…/Beta Settings/en_US/.runConfig` is a JSON cache keyed by version with an `ETag` and
`"ttl": 86400.0`. It currently sets `EnableAgentic: true`, `EnableAgenticImagePaste: true`,
`EnableAgenticIsolationRecovery: true`, `EnableVectorizeMCPTool: true`,
`EnableGenAIConceptToVectorMCPTool: false`, `GenerativeText: true`, `GenAICreditHub: true`,
`GenAIInstructEdit: true`, `EnableUAMPopupinAppBar: true`, plus a whitelist string
`GenAI3PModelWhitelist: "gpt-4o-image@default\ngemini-flash@nano-banana\ngemini-flash@nano-banana-2\ngpt-image@1.5"`.
Editing this file buys 24 hours at most; it is refetched on the TTL.

`/Applications/Adobe Illustrator (Beta)/AIFeatures.cfg` is a real local override point and is
currently an **empty** `<Features></Features>` document. That is the file to look at for a durable
switch, though it is root-owned and reinstalled with every Beta drop.

---

## [05]-[SCRIPTS_FOLDER]

**`File > Scripts` reads only the application folder, not a user folder.**

Evidence:

1. The folder-name string table has exactly one Scripts entry,
   `$$$/kAIFolderFolderNameStrings/Scripts/Win=Scripts`, and it is **not** in the
   `UserWritablePreset*` family. Every user-writable preset kind has a matching
   `UserWritablePreset…` entry ([04.2]); Scripts has none.
2. `rg -l --binary 'UserWritablePresetScripts'` across the entire
   `/Applications/Adobe Illustrator (Beta)` tree returns nothing.
3. `~/Library/Application Support/Adobe/Adobe Illustrator 30.9.0 Beta Settings/en_US/Scripts`
   **does not exist** — the installer created `Brushes`, `Symbols`, `Graphic Styles`, `Swatches`,
   `Composite Fonts`, `Kinsoku`, `MojiKumi`, `Mockup`, `Plug-ins`, `Save for Web Settings`,
   `New Document Profiles` and `Export Presets`, but not `Scripts`.
4. **Neither preference file holds any scripts-path key.** Greps for `script`, `scriptPath`,
   `folderPath`, `additionalPlug` return only `loadAdditionalPlugins = 0` and
   `loadPluginsFromOutsideAppPackage = 1`, which govern plug-ins, not scripts.

So the only menu-visible location is
`/Applications/Adobe Illustrator (Beta)/Presets.localized/en_US/Scripts/`, currently holding
`ImageTracing.jsx`, `SaveDocsAsPDF.jsx`, `SaveDocsAsSVG.jsx`. It is root:admin 0755, so adding a
script needs `sudo` and is wiped by the next Beta install. `File > Scripts > Other Script…` and
`do javascript` over Apple Events (what this probe used) both reach any path without that constraint.

---

## [06]-[WHAT_THIS_MEANS]

1. **`My Color Palette.ai` is the real asset library**, not `My Default Profile.ai`. It holds a
   strict superset of the profile's symbols (224 ⊃ 167) and swatches (143 ⊃ 94), all 8 gradients,
   all 6 patterns, and 16 graphic styles the profile lacks. Whatever gets built next should be seeded
   from it.
2. **Neither profile file is the whole story for graphic styles.** The union of the fork (c) and the
   palette (d) is 49 named styles; taking either alone loses 11 or 16. The Drive-root copy (b),
   despite being the *newer* file by date, holds the *fewest* styles of the three — it lost the
   roads, topography, easement and Digi Line styles that the January fork still has.
3. **The Codex template's palette carries color drift** and renamed swatches and groups. Every
   saturated color is measurably off the canonical value, one generation worse than the
   `My Default Profile.ai` copy. Rebuilding the palette from `My Color Palette.ai` directly would fix
   it.
4. **User libraries have a supported home** that is already wired up:
   `~/Library/Application Support/Adobe/Adobe Illustrator 30.9.0 Beta Settings/en_US/{Swatches,Brushes,Symbols,Graphic Styles}`.
   Three of the four are empty.
5. **Factory libraries cannot be hidden from the panel menus by preference.** They are enumerated
   from the app folder, which is root-owned, outside the code signature, and reinstalled every Beta
   drop. Deleting them is possible with `sudo` and does not break signing, but it is not durable.
   None of them is currently pinned (`Persistent = 0` for all 124 rows).
6. **Half the settings the user will want to change are in Cloud Prefs, not Prefs** — guides, grid,
   units, selection, type, clipboard, black preservation, tooltips, UI brightness. Writing them into
   the larger file has no effect.
7. **The MCP server is on and reachable** with 47 tools, while the Codex client block is
   `enabled = false`. The token is in `AIMCPServer/AuthToken`; the port is compiled into
   `AIAgenticSystem.aip` and configurable nowhere.
8. **The agentic and Firefly switches are server flags**, refetched daily through `.runConfig`. The
   only local override surface is the empty `AIFeatures.cfg` in the app folder.
9. **`My Default Profile.ai` was never installed as a New Document Profile.** The profile folder
   holds the seven factory files only, and all 141 rows of the New Document dialog are factory.
   `startupFileType*` still points at the release install's profile folder in three of six slots.
