# [RASM_RHINO_API_RHINOCOMMON_APPSETTINGS]

`Rhino.ApplicationSettings` owns the process-wide preference tree behind every command and viewport: appearance colors, model-aid and osnap policy, file and autosave paths, general modeling defaults, view and OpenGL behavior, gumball and smart-track interaction, the eight visual-analysis presets, and the command-alias, never-repeat, and shortcut-key registries. Every settings owner is a static class paired with a serializable `*State` snapshot and a `GetCurrentState`/`GetDefaultState`/`UpdateFromState`/`RestoreDefaults` round-trip, so a setting is read, defaulted, and re-applied as one immutable value rather than a scatter of mutable statics. Storage under every owner is a `PersistentSettings` node whose typed writer surface `api-rhinocommon-persistence.md` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.ApplicationSettings`, `Rhino` (`PersistentSettings` custody, cross-referenced)
- asset: `RhinoCommon.dll` — the in-process managed host assembly, verified by direct decompile
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings owners and their state snapshots
- rail: host

Each owner is a static class; its `*State` sibling is the serializable snapshot `GetCurrentState`/`UpdateFromState` round-trips.

| [INDEX] | [OWNER]                   | [STATE]                        | [CAPABILITY]                                                  |
| :-----: | :------------------------ | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | `AppearanceSettings`      | `AppearanceSettingsState`      | UI/viewport colors, fonts, dark/light mode, window position   |
|  [02]   | `ModelAidSettings`        | `ModelAidSettingsState`        | ortho, grid snap, osnap modes, nudge, planar, gumball toggles |
|  [03]   | `FileSettings`            | `FileSettingsState`            | template/autosave/backup paths, search paths, recent files    |
|  [04]   | `GeneralSettings`         | `GeneralSettingsState`         | extrusion default, creased-surface split, general modeling    |
|  [05]   | `ViewSettings`            | `ViewSettingsState`            | lens lengths, rotation style, defined-view restore scope      |
|  [06]   | `OpenGLSettings`          | `OpenGLSettingsState`          | GPU-pipeline and hardware-acceleration policy                 |
|  [07]   | `CursorTooltipSettings`   | `CursorTooltipSettingsState`   | cursor-tooltip visibility and placement                       |
|  [08]   | `SmartTrackSettings`      | `SmartTrackSettingsState`      | smart-track guide, tracking-point, and line behavior          |
|  [09]   | `GumballSettings`         | `GumballSettingsState`         | gumball size, snapping, and drag-strength                     |
|  [10]   | `SelectionFilterSettings` | `SelectionFilterSettingsState` | per-object-kind selection filter policy                       |
|  [11]   | `SoftTransformSettings`   | —                              | soft-editing falloff policy                                   |
|  [12]   | `ChooseOneObjectSettings` | `ChooseOneObjectSettingsState` | ambiguous-pick disambiguation policy                          |
|  [13]   | `PackageManagerSettings`  | —                              | package-manager source and update policy                      |

[PUBLIC_TYPE_SCOPE]: visual-analysis presets
- rail: host

Eight analysis owners share the round-trip shape and feed the `VisualAnalysisMode` overlays of `api-rhinocommon-display.md`; each pairs with a `*State` snapshot.

| [INDEX] | [OWNER]                      | [STATE]                           | [CAPABILITY]                            |
| :-----: | :--------------------------- | :-------------------------------- | :-------------------------------------- |
|  [01]   | `CurvatureAnalysisSettings`  | `CurvatureAnalysisSettingsState`  | Gaussian/mean curvature style and range |
|  [02]   | `CurvatureGraphSettings`     | `CurvatureGraphSettingsState`     | curvature-hair scale and density        |
|  [03]   | `DraftAngleAnalysisSettings` | `DraftAngleAnalysisSettingsState` | draft-angle direction and range         |
|  [04]   | `EdgeAnalysisSettings`       | `EdgeAnalysisSettingsState`       | naked/non-manifold edge display color   |
|  [05]   | `EndAnalysisSettings`        | `EndAnalysisSettingsState`        | curve-end continuity display            |
|  [06]   | `DirectionAnalysisSettings`  | `DirectionAnalysisSettingsState`  | surface/curve direction overlay         |
|  [07]   | `EmapAnalysisSettings`       | `EmapAnalysisSettingsState`       | environment-map reflection analysis     |
|  [08]   | `ZebraAnalysisSettings`      | `ZebraAnalysisSettingsState`      | zebra-stripe direction and thickness    |
|  [09]   | `ThicknessAnalysisSettings`  | `ThicknessAnalysisSettingsState`  | wall-thickness range analysis           |

[PUBLIC_TYPE_SCOPE]: registries and carriers
- rail: host

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                                                        |
| :-----: | :-------------------- | :----- | :------------------------------------------------------------------ |
|  [01]   | `CommandAliasList`    | static | command-alias registry — add, delete, macro lookup, defaults roster |
|  [02]   | `CommandAlias`        | class  | one alias→macro binding with an instant flag                        |
|  [03]   | `ShortcutKeySettings` | static | keyboard-shortcut registry keyed by `ShortcutKey`/`KeyboardKey`     |
|  [04]   | `KeyboardShortcut`    | class  | one modifier+key→macro binding                                      |
|  [05]   | `NeverRepeatList`     | static | commands excluded from the repeat-last-command roster               |

[PUBLIC_TYPE_SCOPE]: policy vocabularies
- rail: host

- `PaintColor` — UI paint slot (`NormalStart`/`HotBorder`/`PressedEnd`/`TextEnabled`/`ActiveViewportTitle`/`PanelBackground`/`EditBoxBackground`/… twenty-four slots) keying `AppearanceSettings.GetPaintColor`
- `WidgetColor` — construction-axis widget color (`UAxisColor`/`VAxisColor`/`WAxisColor`)
- `OsnapModes` — flags osnap set (`Near`/`Focus`/`Center`/`Vertex`/`Knot`/`Quadrant`/`Midpoint`/`Intersection`/`End`/`Perpendicular`/`Tangent`/`Point`)
- `CursorMode` (`None`/`BlackOnWhite`/`WhiteOnBlack`), `PointDisplayMode` (`WorldPoint`/`CplanePoint`), `MouseSelectMode` (`Crossing`/`Window`/`Combo`), `MiddleMouseMode` (`PopupMenu`/`PopupToolbar`/`RunMacro`), `CommandPromptPosition` (`Top`/`Bottom`/`Floating`/`Hidden`)
- `Installation` — install class (`Commercial`/`Educational`/`NotForResale`/`Beta`/`Evaluation`/`Corporate`/…), `ViewSettings.ViewRotationStyle`, `CurvatureAnalysisSettings.CurvatureStyle`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the state round-trip
- rail: host

Every owner repeats this shape; `AppearanceSettings` stands for the family.

- `AppearanceSettings.GetCurrentState() : AppearanceSettingsState` / `GetDefaultState() : AppearanceSettingsState` / `GetDefaultState(bool darkMode) : AppearanceSettingsState` — snapshot the live or default preferences
- `AppearanceSettings.UpdateFromState(AppearanceSettingsState state) : void` / `RestoreDefaults() : void` — re-apply a snapshot or reset
- `ModelAidSettings.GetCurrentState()` / `GetDefaultState()` / `UpdateFromState(ModelAidSettingsState state)` and the identical quartet on `FileSettings`, `GeneralSettings`, `ViewSettings`, `OpenGLSettings`, `CursorTooltipSettings`, `SmartTrackSettings`, `GumballSettings`, and every `*AnalysisSettings` owner

[ENTRYPOINT_SCOPE]: appearance colors
- rail: host

- `AppearanceSettings.GetPaintColor(PaintColor whichColor) : Color` / `GetPaintColor(PaintColor whichColor, bool compute) : Color` / `SetPaintColor(PaintColor whichColor, Color c[, bool forceUiUpdate]) : void` — the whole UI-paint surface is keyed by the `PaintColor` slot, never per-slot named properties
- `AppearanceSettings.DefaultPaintColor(PaintColor whichColor[, bool darkMode]) : Color`; `GetWidgetColor(WidgetColor whichColor) : Color` / `SetWidgetColor(WidgetColor whichColor, Color c[, bool forceUiUpdate]) : void` / `DefaultWidgetColor(WidgetColor whichColor) : Color`
- `AppearanceSettings.SetToDarkMode() : bool` / `SetToLightMode() : bool` / `UsingDefaultDarkModeColors() : bool` / `UsingDefaultLightModeColors() : bool` / `InitialMainWindowPosition(out Rectangle bounds) : bool`

[ENTRYPOINT_SCOPE]: model-aid and osnap state
- rail: host

`ModelAidSettingsState` carries the mutable knobs the owner round-trips (all get/set).

- snap and construction: `GridSnap` / `Ortho` / `Planar` / `Osnap` / `OsnapModes : OsnapModes` / `OsnapCursorMode : CursorMode` / `ProjectSnapToCPlane` / `SnapToLocked` / `SnapToOccluded` / `SnapToFiltered` / `OnlySnapToSelected` / `PointDisplay : PointDisplayMode` : bool/enum
- cplane: `UniversalConstructionPlaneMode` / `AutoAlignCPlane` / `AutoCPlaneAlignment : int` / `StickyAutoCPlane` / `OrientAutoCPlaneToView`
- nudge and pickbox: `NudgeMode : int` / `NudgeKeyStep` / `CtrlNudgeKeyStep` / `ShiftNudgeKeyStep : double` / `OsnapPickboxRadius` / `MousePickboxRadius` / `DigitizerOsnapPickboxRadius`
- gumball and control-polygon: `AutoGumballEnabled` / `SnappyGumballEnabled` / `GumballExtrudeMergeFaces` / `DisplayControlPolygon` / `HighlightControlPolygon` / `ControlPolygonDisplayDensity : int` / `OrthoAngle : double`

[ENTRYPOINT_SCOPE]: file paths and autosave
- rail: host

- `FileSettings.GetSearchPaths() : string[]` / `AddSearchPath(string folder, int index) : int` / `DeleteSearchPath(string folder) : bool` / `FindFile(string fileName) : string` / `SearchPathCount : int`
- `FileSettings.AutoSaveBeforeCommands() : string[]` / `SetAutoSaveBeforeCommands(string[] commands) : void` / `RecentlyOpenedFiles() : string[]`
- `FileSettings.GetDataFolder(bool currentUser) : string` / `DefaultTemplateFolderForLanguageID(int languageID) : string`

[ENTRYPOINT_SCOPE]: general, view, and analysis
- rail: host

- `GeneralSettings.UseExtrusions : bool` / `SplitCreasedSurfaces : bool` — the two modeling-default statics beside the state round-trip
- `ViewSettings.ThreePointPerspectiveLensLength : double` / `TwoPointPerspectiveLensLength : double`; the defined-view restore-scope flags `DefinedViewSetCPlane`/`DefinedViewSetProjection`/`DefinedViewSetClippingPlanes`/`DefinedViewSetDisplayMode` are owned by `api-rhinocommon-display.md`
- `CurvatureAnalysisSettings.CalculateCurvatureAutoRange(IEnumerable<Mesh> meshes, ref CurvatureAnalysisSettingsState settings) : bool` — the analysis owner computes an auto-range into its own state, the extra verb beyond the shared round-trip

[ENTRYPOINT_SCOPE]: alias, shortcut, and never-repeat registries
- rail: host

- `CommandAliasList.Add(string alias, string macro) : bool` / `Delete(string alias) : bool` / `SetMacro(string alias, string macro) : bool` / `GetMacro(string alias) : string` / `FindAlias(string alias) : CommandAlias` / `IsAlias(string alias) : bool` / `GetNames() : string[]` / `Count : int` / `ToDictionary() : Dictionary<string, string>` / `GetDefaults() : Dictionary<string, string>` / `Update(IEnumerable<CommandAlias> aliases, bool replaceAll) : void`
- `new CommandAlias(string alias, string macro, bool instant)` — the alias binding minted before `Update`
- `ShortcutKeySettings.GetShortcuts() : KeyboardShortcut[]` / `GetDefaults() : KeyboardShortcut[]` / `Update(IEnumerable<KeyboardShortcut> shortcuts, bool replaceAll) : void` / `GetMacro(ShortcutKey key) : string` / `SetMacro(KeyboardKey key, ModifierKey modifier, string macro) : void` / `IsAcceptableKeyCombo(KeyboardKey key, ModifierKey modifier) : bool`; `KeyboardShortcut.Modifier : ModifierKey` / `Key : KeyboardKey` / `Macro : string`
- `NeverRepeatList.UseNeverRepeatList : bool` / `SetList(string[] commandNames) : int` / `CommandNames() : string[]`

[ENTRYPOINT_SCOPE]: persistent-settings storage
- rail: host

- storage under every owner is a `Rhino.PersistentSettings` node; the typed writer/reader surface (`SetString`/`SetColor`/`SetEnumValue<T>`/`GetValue<T>`, change tracking, section-scoped keys) is owned by `api-rhinocommon-persistence.md` §[03] and never re-documented here
- a plugin reads its own tree through `Rhino.PlugIns.PlugIn.GetPluginSettings(Guid plugInId, bool load) : PersistentSettings` (owned by `api-rhinocommon-plugins.md`), which this family shares the `PersistentSettings` node type with

## [04]-[IMPLEMENTATION_LAW]

[SETTINGS_TOPOLOGY]:
- one owner projects one concern: the static class exposes the read/write verbs, the `*State` sibling is the immutable snapshot, and `GetCurrentState`→mutate→`UpdateFromState` is the atomic edit, never a scatter of mutable static setters observed mid-flight
- color is keyed, not named: `AppearanceSettings` reaches every UI color through `GetPaintColor(PaintColor)`/`GetWidgetColor(WidgetColor)`, so a new color slot is an enum row rather than a new property
- osnap and selection policy are flag sets (`OsnapModes`, selection filter), combined by bitwise union rather than per-mode booleans
- the analysis owners are one family: eight presets repeat the round-trip and differ only in the `*State` payload, so an analysis mode is a row in `api-rhinocommon-display.md`'s `VisualAnalysisMode` roster reading its matching settings owner

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the `FindFile`/`FindAlias`/`GetMacro` lookups that can miss fold to `Option<A>`, `SetMacro`/`AddSearchPath`/`DeleteSearchPath` bool/index outcomes fold to `Fin<A>`, and a `*State` edit accumulates through an immutable `with`-style projection rather than in-place static mutation
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `PaintColor`, `WidgetColor`, `CursorMode`, `PointDisplayMode`, `MouseSelectMode`, `MiddleMouseMode`, `CommandPromptPosition`, and `Installation` map to keyed `SmartEnum` owners, `OsnapModes` wraps as a flag `SmartEnum` policy set, and each `*State` snapshot wraps as a `ComplexValueObject` so the settings value crosses domain code as one validated record
- `api-rhinocommon-persistence.md`: the `PersistentSettings` node under every owner is the custody spine, and its typed writer/reader surface composes there rather than being re-minted per settings family
- `api-rhinocommon-display.md`: the eight `*AnalysisSettings` owners feed the `VisualAnalysisMode` false-color overlays, and `ViewSettings` restore-scope flags compose the defined-view restore there

[LOCAL_ADMISSION]:
- a preference edit enters through the owning static class's state round-trip, never a raw `PersistentSettings` write at this tier
- color access enters through the keyed `GetPaintColor`/`GetWidgetColor` accessors, never a per-slot named field
- alias, shortcut, and never-repeat mutation enters through the registry's `Update(..., replaceAll)`, and `PersistentSettings` typed I/O routes to `api-rhinocommon-persistence.md`

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the process-wide preference tree — appearance, model-aid, file, general, view, OpenGL, interaction, and analysis settings, plus the alias/shortcut/never-repeat registries
- Accept: the state round-trip, keyed color access, flag-set osnap/selection policy, registry CRUD
- Reject: raw `PersistentSettings` node writes at this tier, per-slot named color duplication, mid-edit mutable-static observation, and re-documentation of the persistence typed-writer surface

[NAMESPACE_CENSUS]:
- covered public surface: `Rhino.ApplicationSettings` full family — settings owners, `*State` snapshots, the analysis presets, the alias/shortcut/never-repeat registries, and the policy enums
- cross-referenced, never duplicated: `Rhino.PersistentSettings` typed writers and change tracking (owned by `api-rhinocommon-persistence.md`), `ViewSettings.DefinedViewSet*` restore-scope flags (owned by `api-rhinocommon-display.md`), `PlugIn.GetPluginSettings` (owned by `api-rhinocommon-plugins.md`)
- access-excluded (internal, never lands): `NamespaceDoc`, `LicenseNode`, `Installation` service helpers, and the `*State` internal backing fields the host does not expose
