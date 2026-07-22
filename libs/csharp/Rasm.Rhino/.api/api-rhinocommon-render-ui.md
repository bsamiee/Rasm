# [RASM_RHINO_API_RHINOCOMMON_RENDER_UI]

`Rhino.Render` owns the render-editor UI backing surface: `DataSources` carries the content-editor view settings (`Modes`/`Shapes`/`Sizes` thumbnail layout, `AssignBys` assignment policy, `MetaData` preview descriptor) and the `RhinoSettings` bridge that binds the editor to the document `RenderSettings`, active view, current renderer, and custom render sizes; `RenderPanels` and `RenderTabs` register and resolve a plug-in's custom render-window panels and side-pane tabs by session; `Rhino.Render.UI.WorldMapDayNight` renders the sun-editor day/night world map; and the `ParameterNames.PhysicallyBased`, `ChildSlotNames.PhysicallyBased`, and `RenderMaterial.BasicMaterialParameterNames` owners hold the named-parameter and child-slot vocabulary a content editor binds. `api-rhinocommon-rendercontent.md` owns the `RenderContent`/`RenderContentCollection`/`ContentFactory` object model these settings and the named parameters address; `api-rhinocommon-rendersettings.md` owns the `RenderSettings` aggregate the `RhinoSettings` bridge reads and writes; `api-rhinocommon-document.md` owns the `RhinoDoc`/`RhinoView`/`ViewInfo` carriers the bridge binds; and `api-rhino-ui.md` owns the `Rhino.UI.Controls.ICollapsibleSection` and `Rhino.UI.ObjectPropertiesPage` UI carriers a registered panel or tab body composes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon render-editor-UI surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render.DataSources`, `Rhino.Render.UI`, `Rhino.Render.ParameterNames`, `Rhino.Render.ChildSlotNames`, `Rhino.Render`
- kernel: `Rasm` (host-agnostic size and color owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: render-editor-UI boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: editor data sources and settings bridge
- rail: render-editor-UI boundary

`RhinoSettings` and `MetaData` are `sealed : IDisposable` native wrappers; the view/shape/size/assign vocabularies are bounded enums.

| [INDEX] | [SYMBOL]        | [KIND]             | [CAPABILITY]                                                       |
| :-----: | :-------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `RhinoSettings` | settings bridge    | render/preview settings, renderer id, active/rendering view, sizes |
|  [02]   | `MetaData`      | preview meta       | per-content geometry descriptor and content-instance id            |
|  [03]   | `Modes`         | view-mode enum     | grid/list/tree editor layout                                       |
|  [04]   | `Shapes`        | thumbnail enum     | square/wide thumbnail aspect                                       |
|  [05]   | `Sizes`         | thumbnail enum     | tiny-to-large thumbnail size                                       |
|  [06]   | `AssignBys`     | assign-policy enum | layer/parent/object/plug-in assignment policy                      |

[PUBLIC_TYPE_SCOPE]: custom panel/tab registration and sun-editor map
- rail: render-editor-UI boundary

`RenderPanels`/`RenderTabs` are `sealed` host-provided registries (internal constructor); `WorldMapDayNight` is a `sealed : IDisposable` map renderer. A registered panel or tab `Type` carries a `System.Runtime.InteropServices.GuidAttribute`.

| [INDEX] | [SYMBOL]           | [KIND]          | [CAPABILITY]                                                      |
| :-----: | :----------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `RenderPanels`     | panel registry  | register and resolve custom render-window panels by session       |
|  [02]   | `RenderTabs`       | tab registry    | register and resolve custom side-pane tabs, derive UI/session ids |
|  [03]   | `RenderPanelType`  | panel-kind enum | custom render-UI panel kind                                       |
|  [04]   | `WorldMapDayNight` | map renderer    | day/night world-map bitmap and lat-long-to-pixel mapping          |

[PUBLIC_TYPE_SCOPE]: named-parameter and child-slot constant owners
- rail: render-editor-UI boundary

Each owner is a static class of name constants a content editor and the `RenderContent.GetParameter`/`SetParameter` surface bind.

| [INDEX] | [SYMBOL]                                     | [KIND]            | [CAPABILITY]                                         |
| :-----: | :------------------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `ParameterNames.PhysicallyBased`             | PBR param names   | physically-based material named-parameter vocabulary |
|  [02]   | `ChildSlotNames.PhysicallyBased`             | PBR slot names    | physically-based texture child-slot vocabulary       |
|  [03]   | `RenderMaterial.BasicMaterialParameterNames` | basic param names | basic-material named-parameter vocabulary            |

[ENUM_ROSTERS]:
- `public enum Rhino.Render.DataSources.Modes` — `Unset = 0`, `Grid = 1`, `List = 2`, `Tree = 3`.
- `public enum Rhino.Render.DataSources.Shapes` — `Square = 0`, `Wide = 1`.
- `public enum Rhino.Render.DataSources.Sizes` — `Unset = 0`, `Tiny = 1`, `Small = 2`, `Medium = 3`, `Large = 4`.
- `public enum Rhino.Render.DataSources.AssignBys` — `Unset = 0`, `Layer = 1`, `Parent = 2`, `Object = 3`, `Varies = 4`, `PlugIn = 5`.
- `public enum Rhino.Render.RenderPanelType` — `RenderWindow = 0`; the custom-panel kind hosted in the render output window.
- `public enum Rhino.Render.RenderPanels.ExtraSidePanePosition` — `Left = 0`, `Top = 1`, `Right = 2`, `Bottom = 3`.

## [03]-[ENTRYPOINTS]

[SETTINGS_BRIDGE]:
- `Rhino.Render.DataSources.RhinoSettings.GetRenderSettings() : RenderSettings` / `SetRenderSettings(RenderSettings renderSettings) : void` — read and write the document render settings the editor binds; `RenderSettings` is `api-rhinocommon-rendersettings.md`'s.
- `Rhino.Render.DataSources.RhinoSettings.GetCurrentRenderer() : Guid` / `SetCurrentRenderer(Guid g) : void` — the current render-engine plug-in id.
- `Rhino.Render.DataSources.RhinoSettings.ActiveView() : RhinoView` / `RenderingView() : ViewInfo` — the active editor view and the view a render samples; `RhinoView`/`ViewInfo` are the document/display catalog's.
- `Rhino.Render.DataSources.RhinoSettings.GetCustomRenderSizes() : List<Size>` / `CustomImageSizeIsPreset : bool` — the custom render-size roster and preset flag.
- `Rhino.Render.DataSources.RhinoSettings.ViewSupportsShading(RhinoView view) : bool` / `GroundPlaneOnInViewDisplayMode(RhinoView view) : bool` / `SetGroundPlaneOnInViewDisplayMode(RhinoView view, bool bOn) : void` — per-view shading and ground-plane display predicates.
- `Rhino.Render.DataSources.RhinoSettings.CppPointer : nint` / `Dispose() : void` — native handle and disposal; `RhinoSettings : IDisposable`, so a live instance rides a `using`.
- `Rhino.Render.DataSources.MetaData.Geometry() : string` / `ContentInstanceId() : Guid` / `CppPointer : nint` / `Dispose() : void` — the preview meta-data descriptor and content-instance id; `MetaData : IDisposable`.

[PANEL_TAB_REGISTRATION]:
- `Rhino.Render.RenderPanels.RegisterPanel(PlugIn plugin, RenderPanelType renderPanelType, Type panelType, Guid renderEngineId, string caption, bool alwaysShow, bool initialShow) : void` / `RegisterPanel(PlugIn, RenderPanelType, Type, Guid renderEngineId, string caption, bool alwaysShow, bool initialShow, ExtraSidePanePosition pos) : void` — register a custom render-window panel; `panelType` carries a `GuidAttribute` or the call throws `ArgumentException`.
- `Rhino.Render.RenderPanels.FromRenderSessionId(PlugIn plugIn, Type panelType, Guid renderSessionId) : object` — resolve the registered panel instance for a render session.
- `Rhino.Render.RenderTabs.RegisterTab(PlugIn plugin, Type tabType, Guid renderEngineId, string caption, Icon icon) : void` — register a custom side-pane tab; `tabType` carries a `GuidAttribute`, and `Icon` is `System.Drawing.Icon`.
- `Rhino.Render.RenderTabs.FromRenderSessionId(PlugIn plugIn, Type tabType, Guid renderSessionId) : object` / `SidePaneUiIdFromTab(object tab) : Guid` / `SessionIdFromTab(object tab) : Guid` — resolve a registered tab and derive its side-pane UI and session ids.

[WORLD_MAP]:
- `Rhino.Render.UI.WorldMapDayNight.SetTimeInfo(DateTime dt, double timezone, int daylightSavingMinutes, bool bDaylightSavingsOn) : void` / `SetDayNightDisplay(bool bOn) : void` / `SetEnabled(bool bEnabled) : void` — configure the map time context and display toggles.
- `Rhino.Render.UI.WorldMapDayNight.MakeMapBitmap() : void` / `Map() : Image` / `HasMapForCurrentSettings() : bool` — render and read the world-map bitmap; `Image` is `System.Drawing.Image`.
- `Rhino.Render.UI.WorldMapDayNight.LocationToMap(Point2d latlong) : System.Drawing.Point` / `MapToLocation(System.Drawing.Point mapPoint) : Point2d` / `Dispose() : void` — the lat-long-to-pixel bijection and disposal; `WorldMapDayNight : IDisposable`.

[PARAMETER_NAMES]:
- `Rhino.Render.ParameterNames.PhysicallyBased` — get-only static `string` properties naming the PBR material parameters: `BaseColor`, `BRDF`, `Subsurface`, `SubsurfaceScatteringColor`, `SubsurfaceScatteringRadius`, `Specular`, `SpecularTint`, `Metallic`, `Roughness`, `Anisotropic`, `AnisotropicRotation`, `Sheen`, `SheenTint`, `Clearcoat`, `ClearcoatRoughness`, `ClearcoatBump`, `OpacityIor`, `Opacity`, `OpacityRoughness`, `Emission`, `Displacement`, `Bump`, `AmbientOcclusion`, `Alpha`.
- `Rhino.Render.ChildSlotNames.PhysicallyBased` — get-only static `string` properties naming the PBR texture child-slots, each resolving through `FromTextureType(TextureType textureType) : string`: `BaseColor`, `Subsurface`, `SubsurfaceScatteringColor`, `SubsurfaceScatteringRadius`, `Specular`, `SpecularTint`, `Metallic`, `Roughness`, `Anisotropic`, `AnisotropicRotation`, `Sheen`, `SheenTint`, `Clearcoat`, `ClearcoatRoughness`, `ClearcoatBump`, `OpacityIor`, `Opacity`, `OpacityRoughness`, `Emission`, `Displacement`, `Bump`, `AmbientOcclusion`, `Alpha`; `TextureType` is `api-rhinocommon-document.md`'s.
- `Rhino.Render.RenderMaterial.BasicMaterialParameterNames` — `public const string` basic-material parameter names: `Ambient = "ambient"`, `Emission = "emission"`, `FlamingoLibrary = "flamingo-library"`, `DisableLighting = "disable-lighting"`, `Diffuse = "diffuse"`, `Specular = "specular"`, `TransparencyColor = "transparency-color"`, `ReflectivityColor = "reflectivity-color"`, `Shine = "shine"`, `Transparency = "transparency"`, `Reflectivity = "reflectivity"`, `Ior = "ior"`.

## [04]-[IMPLEMENTATION_LAW]

[EDITOR_UI_TOPOLOGY]:
- `RhinoSettings` is the single editor-to-document bridge: it reads and writes the document `RenderSettings`, the current renderer id, the active/rendering view, and the custom render sizes — a domain owner binds through it rather than re-reading the document, and a parallel settings mirror beside it is the deleted form.
- View, thumbnail, and assign vocabularies are bounded: `Modes`/`Shapes`/`Sizes`/`AssignBys` enumerate the editor layout and assignment policy, and a stringly editor-mode or assign key beside them is rejected.
- Panel and tab registration is one owner each: `RenderPanels` registers custom render-window panels and `RenderTabs` custom side-pane tabs, both keyed by a `GuidAttribute`-decorated `Type` and resolved by render-session id — a panel or tab minted outside these registries never appears in the render editor.
- Named parameters are constant owners: `ParameterNames.PhysicallyBased`/`ChildSlotNames.PhysicallyBased`/`RenderMaterial.BasicMaterialParameterNames` name the parameters and child slots the `RenderContent.GetParameter`/`SetParameter` surface addresses — a literal parameter string beside these owners is the deleted form.
- Editor internals stay host-side: the `DataSources.PreviewSettings`/`ContentEditorSettings`/`NewContentControlAssignBy`/`RdkBackEnd`/`RdkRenderSettingsBackEnd`/`RdkContentUIs` types, the whole `Rhino.Render.UICommands` and `Rhino.Render.Controls.Definitions` namespaces, and the obsolete `Rhino.Render.UI.IUserInterfaceSection`/`UserInterfaceSection` interfaces are internal or obsolete and never cross the boundary.
- UI sections seat through the `Rhino.UI.Controls.ICollapsibleSection` family `api-rhino-ui.md` owns, never the obsolete `UserInterfaceSection` interface.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a `RhinoSettings`/`MetaData`/`WorldMapDayNight` `IDisposable` rides a `using` bounded by `Eff`; a settings read projects a detached value record, a `ViewSupportsShading` predicate crosses as `bool`, and a panel/tab registration outcome crosses as `Fin<Unit>`; `FromRenderSessionId` returning `object` lifts to `Option<A>` after an edge cast.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `Modes`, `Shapes`, `Sizes`, `AssignBys`, `RenderPanelType`, and `ExtraSidePanePosition` map at the edge to `[SmartEnum]` owners; the PBR/basic parameter-name sets collapse to one generated `[SmartEnum<string>]` name vocabulary keyed on the parameter the content surface addresses.
- `api-rhinocommon-rendersettings.md`: `RhinoSettings.GetRenderSettings`/`SetRenderSettings` bridge the document `RenderSettings` aggregate — settings own the configuration, this catalog owns the editor binding.
- `api-rhinocommon-rendercontent.md`: PBR/basic parameter names address `RenderContent.GetParameter`/`SetParameter`, and a registered panel or tab edits `RenderContent`; the content catalog owns the content, this catalog owns the UI vocabulary and registration.
- `api-rhinocommon-document.md` and `api-rhino-ui.md`: `RhinoDoc`/`RhinoView`/`ViewInfo`/`TextureType` bind the settings bridge and child-slot resolver, and a registered panel or tab body composes `Rhino.UI.Controls.ICollapsibleSection`/`Rhino.UI.ObjectPropertiesPage` — the document and UI catalogs own the carriers, this catalog owns the render-editor seam.

[LOCAL_ADMISSION]:
- Editor state enters through `RhinoSettings`/`MetaData`, mutates inside the `using` grant, and commits through the settings bridge; a detached value record leaves the boundary, never a live `RhinoSettings` or `MetaData`.
- A custom panel or tab enters through `RenderPanels.RegisterPanel`/`RenderTabs.RegisterTab` with a `GuidAttribute`-decorated `Type` and resolves by render-session id; a panel or tab minted beside the registry is rejected.
- Named parameters enter through the constant owners, never a literal parameter string; the editor layout enters through the bounded `Modes`/`Shapes`/`Sizes`/`AssignBys` vocabulary.

[RAIL_LAW]:
- Surface: `Rhino.Render.DataSources` + `Rhino.Render.UI` + `Rhino.Render.ParameterNames` + `Rhino.Render.ChildSlotNames` + the `Rhino.Render` panel/tab-registration and basic-parameter-name slice
- Owns: the `RhinoSettings` editor-to-document bridge, the `MetaData` preview descriptor, the `Modes`/`Shapes`/`Sizes`/`AssignBys` editor vocabulary, the `RenderPanels`/`RenderTabs` custom panel/tab registries, the `WorldMapDayNight` sun-editor map, and the PBR/basic named-parameter and child-slot constant owners.
- Accept: editor settings read and mutation through the `RhinoSettings` bridge with a `using` window; panel/tab registration over a `GuidAttribute`-keyed `Type` resolved by session; named parameters composed from the constant owners; editor vocabulary mapped to bounded owners at the edge.
- Reject: a parallel settings mirror beside `RhinoSettings`, a stringly editor-mode or assign key, a literal parameter string beside the name owners, a panel or tab minted outside the registries, the internal `DataSources`/`UICommands`/`Controls.Definitions` types re-surfaced, the obsolete `UserInterfaceSection` interfaces catalogued, and a live `RhinoSettings`/`MetaData` escaping into a domain signature.
