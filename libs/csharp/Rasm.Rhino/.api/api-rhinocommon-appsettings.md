# [RASM_RHINO_API_RHINOCOMMON_APPSETTINGS]

`Rhino.ApplicationSettings` owns the process-wide preference tree every command and viewport reads: appearance, model-aid, file, general, view, OpenGL, and interaction settings, the visual-analysis presets, and the alias, shortcut, and never-repeat registries. Every owner is a static class paired with a serializable `{Owner}State` snapshot, and `GetCurrentState` → mutate → `UpdateFromState` is the atomic edit rather than mutable statics observed mid-flight. Storage under every owner is a `Rhino.PersistentSettings` node whose typed writer/reader surface `api-rhinocommon-persistence.md` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (proprietary McNeel host SDK)
- assembly: `RhinoCommon.dll` — in-process managed host runtime, host-resolved
- namespace: `Rhino.ApplicationSettings`; `Rhino` (`PersistentSettings` custody, cross-referenced)
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings owners

Each owner is a static class; its serializable snapshot is `{Owner}State`, read and re-applied through `GetCurrentState`/`UpdateFromState`. `SoftTransformSettings` and `PackageManagerSettings` carry no snapshot.

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                  |
| :-----: | :------------------------ | :------------------------------------------------------------ |
|  [01]   | `AppearanceSettings`      | UI/viewport colors, fonts, dark/light mode, window position   |
|  [02]   | `ModelAidSettings`        | ortho, grid snap, osnap modes, nudge, planar, gumball toggles |
|  [03]   | `FileSettings`            | template/autosave/backup paths, search paths, recent files    |
|  [04]   | `GeneralSettings`         | extrusion default, creased-surface split, general modeling    |
|  [05]   | `ViewSettings`            | lens lengths, rotation style, defined-view restore scope      |
|  [06]   | `OpenGLSettings`          | GPU-pipeline and hardware-acceleration policy                 |
|  [07]   | `CursorTooltipSettings`   | cursor-tooltip visibility and placement                       |
|  [08]   | `SmartTrackSettings`      | smart-track guide, tracking-point, and line behavior          |
|  [09]   | `GumballSettings`         | gumball size, snapping, and drag-strength                     |
|  [10]   | `SelectionFilterSettings` | per-object-kind selection filter policy                       |
|  [11]   | `SoftTransformSettings`   | soft-editing falloff policy                                   |
|  [12]   | `ChooseOneObjectSettings` | ambiguous-pick disambiguation policy                          |
|  [13]   | `PackageManagerSettings`  | package-manager source and update policy                      |

[PUBLIC_TYPE_SCOPE]: visual-analysis presets

Analysis owners share the round-trip shape and feed the `VisualAnalysisMode` overlays of `api-rhinocommon-display.md`; each pairs a `{Owner}State` snapshot.

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                            |
| :-----: | :--------------------------- | :-------------------------------------- |
|  [01]   | `CurvatureAnalysisSettings`  | Gaussian/mean curvature style and range |
|  [02]   | `CurvatureGraphSettings`     | curvature-hair scale and density        |
|  [03]   | `DraftAngleAnalysisSettings` | draft-angle direction and range         |
|  [04]   | `EdgeAnalysisSettings`       | naked/non-manifold edge display color   |
|  [05]   | `EndAnalysisSettings`        | curve-end continuity display            |
|  [06]   | `DirectionAnalysisSettings`  | surface/curve direction overlay         |
|  [07]   | `EmapAnalysisSettings`       | environment-map reflection analysis     |
|  [08]   | `ZebraAnalysisSettings`      | zebra-stripe direction and thickness    |
|  [09]   | `ThicknessAnalysisSettings`  | wall-thickness range analysis           |

[PUBLIC_TYPE_SCOPE]: registries and carriers

| [INDEX] | [SYMBOL]              | [KIND] | [CAPABILITY]                                                        |
| :-----: | :-------------------- | :----- | :------------------------------------------------------------------ |
|  [01]   | `CommandAliasList`    | static | command-alias registry — add, delete, macro lookup, defaults roster |
|  [02]   | `CommandAlias`        | class  | one alias→macro binding with an instant flag                        |
|  [03]   | `ShortcutKeySettings` | static | keyboard-shortcut registry keyed by `ShortcutKey`/`KeyboardKey`     |
|  [04]   | `KeyboardShortcut`    | class  | one modifier+key→macro binding                                      |
|  [05]   | `NeverRepeatList`     | static | commands excluded from the repeat-last-command roster               |

[PUBLIC_TYPE_SCOPE]: policy enums

Each maps to a Thinktecture `SmartEnum` owner at the boundary; enum cases resolve from the assembly.

| [INDEX] | [SYMBOL]                | [KIND]     | [CAPABILITY]                         |
| :-----: | :---------------------- | :--------- | :----------------------------------- |
|  [01]   | `PaintColor`            | enum       | UI paint slot keying `GetPaintColor` |
|  [02]   | `WidgetColor`           | enum       | construction-axis widget color       |
|  [03]   | `OsnapModes`            | flags enum | osnap set combined by bitwise union  |
|  [04]   | `CursorMode`            | enum       | osnap-cursor rendering mode          |
|  [05]   | `PointDisplayMode`      | enum       | point display space                  |
|  [06]   | `MouseSelectMode`       | enum       | window/crossing selection mode       |
|  [07]   | `MiddleMouseMode`       | enum       | middle-button action                 |
|  [08]   | `CommandPromptPosition` | enum       | command-prompt placement             |
|  [09]   | `Installation`          | enum       | license/install class                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the state round-trip

Every owner repeats this shape; `AppearanceSettings` stands for the family.

- `AppearanceSettings.GetCurrentState() : AppearanceSettingsState` / `GetDefaultState() : AppearanceSettingsState` / `GetDefaultState(bool darkMode) : AppearanceSettingsState` — snapshot the live or default preferences.
- `AppearanceSettings.UpdateFromState(AppearanceSettingsState state) : void` / `RestoreDefaults() : void` — re-apply a snapshot or reset.
- `ModelAidSettings`, `FileSettings`, `GeneralSettings`, `ViewSettings`, `OpenGLSettings`, `CursorTooltipSettings`, `SmartTrackSettings`, `GumballSettings`, and every `*AnalysisSettings` owner carry the identical `GetCurrentState`/`GetDefaultState`/`UpdateFromState`/`RestoreDefaults` quartet over their own `*State`.

[ENTRYPOINT_SCOPE]: appearance colors
- `AppearanceSettings.GetPaintColor(PaintColor whichColor[, bool compute]) : Color` / `SetPaintColor(PaintColor whichColor, Color c[, bool forceUiUpdate]) : void` / `DefaultPaintColor(PaintColor whichColor[, bool darkMode]) : Color` — the UI-paint surface, keyed by the `PaintColor` slot.
- `AppearanceSettings.GetWidgetColor(WidgetColor whichColor) : Color` / `SetWidgetColor(WidgetColor whichColor, Color c[, bool forceUiUpdate]) : void` / `DefaultWidgetColor(WidgetColor whichColor) : Color` — construction-axis widget color.
- `AppearanceSettings.SetToDarkMode() : bool` / `SetToLightMode() : bool` / `UsingDefaultDarkModeColors() : bool` / `UsingDefaultLightModeColors() : bool` / `InitialMainWindowPosition(out Rectangle bounds) : bool` — theme switch, default-theme probe, and startup window bounds.

[ENTRYPOINT_SCOPE]: model-aid and osnap state

`ModelAidSettingsState` carries the round-tripped knobs (get/set); each is `bool` unless the token marks its type.

- snap: `GridSnap` `Ortho` `Planar` `Osnap` `OsnapModes`(OsnapModes) `OsnapCursorMode`(CursorMode) `ProjectSnapToCPlane` `SnapToLocked` `SnapToOccluded` `SnapToFiltered` `OnlySnapToSelected` `PointDisplay`(PointDisplayMode)
- cplane: `UniversalConstructionPlaneMode` `AutoAlignCPlane` `AutoCPlaneAlignment`(int) `StickyAutoCPlane` `OrientAutoCPlaneToView`
- nudge and pickbox: `NudgeMode`(int) `NudgeKeyStep`(double) `CtrlNudgeKeyStep`(double) `ShiftNudgeKeyStep`(double) `OsnapPickboxRadius`(int) `MousePickboxRadius`(int) `DigitizerOsnapPickboxRadius`(double)
- gumball and control-polygon: `AutoGumballEnabled` `SnappyGumballEnabled` `GumballExtrudeMergeFaces` `DisplayControlPolygon` `HighlightControlPolygon` `ControlPolygonDisplayDensity`(int) `OrthoAngle`(double)

[ENTRYPOINT_SCOPE]: file paths and autosave
- `FileSettings.GetSearchPaths() : string[]` / `AddSearchPath(string folder, int index) : int` / `DeleteSearchPath(string folder) : bool` / `FindFile(string fileName) : string` / `SearchPathCount : int` — search-path roster and resolution.
- `FileSettings.AutoSaveBeforeCommands() : string[]` / `SetAutoSaveBeforeCommands(string[] commands) : void` / `RecentlyOpenedFiles() : string[]` — autosave-trigger and recent-file lists.
- `FileSettings.GetDataFolder(bool currentUser) : string` / `DefaultTemplateFolderForLanguageID(int languageID) : string` — data and localized template folders.

[ENTRYPOINT_SCOPE]: general, view, and analysis
- `GeneralSettings.UseExtrusions : bool` / `SplitCreasedSurfaces : bool` — the modeling-default statics beside the state round-trip.
- `ViewSettings.ThreePointPerspectiveLensLength : double` / `TwoPointPerspectiveLensLength : double` — perspective lens lengths.
- `CurvatureAnalysisSettings.CalculateCurvatureAutoRange(IEnumerable<Mesh> meshes, ref CurvatureAnalysisSettingsState settings) : bool` — computes an auto-range into its own state, the verb beyond the shared round-trip.

[ENTRYPOINT_SCOPE]: alias, shortcut, and never-repeat registries
- `CommandAliasList.Add(string alias, string macro) : bool` / `Delete(string alias) : bool` / `SetMacro(string alias, string macro) : bool` / `GetMacro(string alias) : string` / `FindAlias(string alias) : CommandAlias` / `IsAlias(string alias) : bool` / `GetNames() : string[]` / `Count : int` / `ToDictionary() : Dictionary<string, string>` / `GetDefaults() : Dictionary<string, string>` / `Update(IEnumerable<CommandAlias> aliases, bool replaceAll) : void`; `new CommandAlias(string alias, string macro, bool instant)` mints the binding before `Update`.
- `ShortcutKeySettings.GetShortcuts() : KeyboardShortcut[]` / `GetDefaults() : KeyboardShortcut[]` / `Update(IEnumerable<KeyboardShortcut> shortcuts, bool replaceAll) : void` / `GetMacro(ShortcutKey key) : string` / `SetMacro(KeyboardKey key, ModifierKey modifier, string macro) : void` / `SetMacro(ShortcutKey key, string macro) : void` / `IsAcceptableKeyCombo(KeyboardKey key, ModifierKey modifier) : bool`; `KeyboardShortcut` carries `Modifier : ModifierKey` / `Key : KeyboardKey` / `Macro : string`.
- `NeverRepeatList.UseNeverRepeatList : bool` / `SetList(string[] commandNames) : int` / `CommandNames() : string[]`.

[ENTRYPOINT_SCOPE]: persistent-settings storage
- A plugin reads its own tree through `Rhino.PlugIns.PlugIn.GetPluginSettings(Guid plugInId, bool load) : PersistentSettings` (`api-rhinocommon-plugins.md`), sharing the `PersistentSettings` node type this family stores under.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one owner projects one concern: the static class exposes the read/write verbs, `{Owner}State` is the immutable snapshot, and `GetCurrentState` → mutate → `UpdateFromState` is the atomic edit, never a mutable static observed mid-flight.
- color is keyed, not named: `GetPaintColor(PaintColor)`/`GetWidgetColor(WidgetColor)` reach every UI color, so a new slot is an enum row rather than a new property.
- osnap and selection policy are flag sets combined by bitwise union rather than per-mode booleans.
- analysis owners are one family: each preset repeats the round-trip and differs only in its `*State` payload, so an analysis mode is a `VisualAnalysisMode` row of `api-rhinocommon-display.md` reading its settings owner.

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): `FindFile`/`FindAlias`/`GetMacro` misses fold to `Option<A>`, `SetMacro`/`AddSearchPath`/`DeleteSearchPath` outcomes fold to `Fin<A>`, and a `*State` edit accumulates through an immutable `with` projection rather than in-place static mutation.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): keyed policy enums map to `SmartEnum` owners, `OsnapModes` wraps as a flag `SmartEnum` policy set, and each `*State` snapshot wraps as a `ComplexValueObject` crossing domain code as one validated record.
- `api-rhinocommon-persistence.md`: `PersistentSettings` under every owner is the custody spine, and its typed writer/reader surface composes there rather than being re-minted per settings family.
- `api-rhinocommon-display.md`: `*AnalysisSettings` owners feed the `VisualAnalysisMode` false-color overlays, and `ViewSettings` restore-scope flags compose the defined-view restore there.

[LOCAL_ADMISSION]:
- a preference edit enters through the owning static class's state round-trip, never a raw `PersistentSettings` write at this tier.
- color access enters through the keyed `GetPaintColor`/`GetWidgetColor` accessors, never a per-slot named field.
- alias, shortcut, and never-repeat mutation enters through the registry's `Update(..., replaceAll)`; `PersistentSettings` typed I/O routes to `api-rhinocommon-persistence.md`.

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the process-wide preference tree — appearance, model-aid, file, general, view, OpenGL, interaction, and analysis settings, with the alias/shortcut/never-repeat registries
- Accept: the state round-trip, keyed color access, flag-set osnap/selection policy, registry CRUD
- Reject: raw `PersistentSettings` writes at this tier, per-slot named color duplication, mid-edit mutable-static observation, re-documented persistence typed-writer surface
