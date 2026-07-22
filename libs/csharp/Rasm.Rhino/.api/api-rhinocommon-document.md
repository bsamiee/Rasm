# [RASM_RHINO_API_RHINOCOMMON_DOCUMENT]

`RhinoDoc` owns document identity across the live and headless runtimes: one handle keyed by `RuntimeSerialNumber` projects every typed component table, and every structural change surfaces as a bound event rather than a polled state. Headless documents expose the identical table, event, and undo surface behind the `IsHeadless` discriminant, making the handle the single host-boundary seam for document geometry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon document surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon.dll` — verified by direct decompile
- namespaces: `Rhino`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.DocObjects.Custom`, `Rhino.Collections`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and identity

| [INDEX] | [SYMBOL]               | [KIND] | [CAPABILITY]                                                                     |
| :-----: | :--------------------- | :----- | :------------------------------------------------------------------------------- |
|  [01]   | `RhinoDoc`             | class  | live/headless document root owning tables, units, tolerances, undo, events       |
|  [02]   | `ModelComponent`       | class  | base of every table component; carries the component identity                    |
|  [03]   | `ModelComponentType`   | enum   | component-kind discriminant shared across every table                            |
|  [04]   | `UnitSystem`           | enum   | model and page unit-system vocabulary                                            |
|  [05]   | `LengthUnit`           | struct | known + custom length-unit carrier behind `ModelUnits`/`PageUnits`               |
|  [06]   | `RhinoMath`            | static | tolerance and numeric constant surface                                           |
|  [07]   | `ArchivableDictionary` | class  | option payload for headless open and custom document state                       |
|  [08]   | `LengthValue`          | class  | disposable locale-aware length text — parse, format, and unit conversion         |
|  [09]   | `ScaleValue`           | class  | disposable scale text ("1:100") over a left/right `LengthValue` pair             |
|  [10]   | `StringParserSettings` | class  | disposable parse-grammar carrier with shared preset statics                      |
|  [11]   | `AnimationProperties`  | class  | disposable document animation-capture spec behind `RhinoDoc.AnimationProperties` |

[PUBLIC_TYPE_SCOPE]: the component-table roster

| [INDEX] | [SYMBOL]                      | [KIND] | [CAPABILITY]                                                              |
| :-----: | :---------------------------- | :----- | :------------------------------------------------------------------------ |
|  [01]   | `ObjectTable`                 | table  | document object collection; add, delete, replace, transform, select, pick |
|  [02]   | `LayerTable`                  | table  | layer tree; `PurgeUnused` reclamation                                     |
|  [03]   | `MaterialTable`               | table  | document material collection                                              |
|  [04]   | `GroupTable`                  | table  | group collection; `PurgeUnused` reclamation                               |
|  [05]   | `ViewTable`                   | table  | model and page views; page-view import, redraw, count                     |
|  [06]   | `NamedViewTable`              | table  | saved named views with aspect-ratio and animated restore                  |
|  [07]   | `InstanceDefinitionTable`     | table  | block definitions; the block catalog owns the deep surface                |
|  [08]   | `NamedConstructionPlaneTable` | table  | saved construction planes                                                 |
|  [09]   | `NamedPositionTable`          | table  | saved named positions                                                     |
|  [10]   | `NamedLayerStateTable`        | table  | saved layer states                                                        |
|  [11]   | `SnapshotTable`               | table  | document snapshots                                                        |
|  [12]   | `SectionStyleTable`           | table  | section-style definitions                                                 |
|  [13]   | `MarkupTable`                 | table  | markup and annotation-review entities                                     |
|  [14]   | `PageViewGroupTable`          | table  | page-view groupings                                                       |
|  [15]   | `RenderMaterialTable`         | table  | render-content materials                                                  |
|  [16]   | `RenderEnvironmentTable`      | table  | render environments                                                       |
|  [17]   | `RenderTextureTable`          | table  | render textures                                                           |
|  [18]   | `LinetypeTable`               | table  | linetype definitions; `PurgeUnused` reclamation                           |
|  [19]   | `LightTable`                  | table  | render lights; components are `LightObject` document objects              |
|  [20]   | `DimStyleTable`               | table  | dimension styles; `PurgeUnused` reclamation                               |
|  [21]   | `HatchPatternTable`           | table  | hatch patterns; `PurgeUnused` reclamation                                 |
|  [22]   | `BitmapTable`                 | table  | embedded bitmap assets                                                    |
|  [23]   | `StringTable`                 | table  | document user text                                                        |
|  [24]   | `ManifestTable`               | table  | cross-table component manifest keyed by `ModelComponentType`              |

[PUBLIC_TYPE_SCOPE]: object carriers and query

| [INDEX] | [SYMBOL]                   | [KIND]     | [CAPABILITY]                                             |
| :-----: | :------------------------- | :--------- | :------------------------------------------------------- |
|  [01]   | `RhinoObject`              | class      | document object handle enumerated from the object table  |
|  [02]   | `ObjectAttributes`         | class      | per-object display, layer, and organization attributes   |
|  [03]   | `ObjRef`                   | class      | object reference carrying a subobject component index    |
|  [04]   | `ObjectType`               | flags enum | geometry-kind filter vocabulary for query and selection  |
|  [05]   | `ObjectEnumeratorSettings` | class      | object-table query filter for `GetObjectList`            |
|  [06]   | `HistoryRecord`            | class      | history-enabled command record threaded through mutation |
|  [07]   | `ComponentIndex`           | struct     | subobject addressing within a parent object              |
|  [08]   | `PickContext`              | class      | pick projection consumed by `PickObjects`                |

[PUBLIC_TYPE_SCOPE]: event and undo carriers
- `DocumentOpenEventArgs` / `DocumentSaveEventArgs` / `DocumentEventArgs` — lifecycle open, save, and document-scope payloads
- `UnitsChangedWithScalingEventArgs` / `WorksessionFileChangedEventArgs` — unit-scale and worksession-file-change payloads
- `RhinoObjectEventArgs` / `RhinoReplaceObjectEventArgs` / `RhinoModifyObjectAttributesEventArgs` — object add, delete, replace, and attribute payloads
- `RhinoTransformObjectsEventArgs` / `RhinoAfterTransformObjectsEventArgs` — before and after transform payloads
- `LayerTableEventArgs` / `MaterialTableEventArgs` / `GroupTableEventArgs` — core table-mutation payloads
- `InstanceDefinitionTableEventArgs` / `SectionStyleTableEventArgs` / `MarkupTableEventArgs` / `PageViewGroupTableEventArgs` — definition, section-style, markup, and page-group table payloads
- `LightTableEventArgs` — `Document : RhinoDoc` / `EventType : LightTableEventType` / `LightIndex : int` / `OldState : Light` / `NewState : LightObject`; the prior state is a bare `Light` while the current lazily resolves `Document.Lights[LightIndex]` as a `LightObject`, so the two sides never share a `ModelComponent` shape
- `CustomUndoEventArgs` — custom non-geometry undo payload folded into an undo record

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: identity and lifecycle
- `RhinoDoc.RuntimeSerialNumber : uint` — carries the stable document key
- `RhinoDoc.FromRuntimeSerialNumber(uint serialNumber) : RhinoDoc` — re-resolve the handle across a callback boundary
- `RhinoDoc.OpenDocuments(bool includeHeadless) : RhinoDoc[]` — enumerate live and headless documents
- `RhinoDoc.Open(string filePath, out bool wasAlreadyOpen) : RhinoDoc` — open into the live runtime
- `RhinoDoc.CreateHeadless(string file3dmTemplatePath) : RhinoDoc` — construct a headless document from a template
- `RhinoDoc.OpenHeadless(string file3dmPath) : RhinoDoc` / `OpenHeadless(string filePath, ArchivableDictionary options) : RhinoDoc` — open headless with optional payload
- `RhinoDoc.Dispose() : void` — release the headless document
- `RhinoDoc.IsHeadless` / `IsAvailable` / `IsOpening` / `IsClosing` / `IsInitializing` / `IsCreating` / `IsReadOnly` / `IsLocked : bool` — readiness discriminants
- `RhinoDoc.ActiveDoc : RhinoDoc` — reads the ambient live document; `Modified : bool` — carries the dirty-save discriminant
- `RhinoDoc.InCommand(bool bIgnoreScriptRunnerCommands) : int` / `ActiveCommandId : Guid` / `InGetPoint : bool` — command and acquisition state
- `RhinoDoc.TimeoutActiveGet() : void` — cancel an active interactive get on the document

[ENTRYPOINT_SCOPE]: component-table accessors
- `RhinoDoc.Objects : ObjectTable` / `Layers : LayerTable` / `Materials : MaterialTable` / `Groups : GroupTable`
- `RhinoDoc.Views : ViewTable` / `NamedViews : NamedViewTable` / `InstanceDefinitions : InstanceDefinitionTable`
- `RhinoDoc.NamedConstructionPlanes : NamedConstructionPlaneTable` / `NamedPositions : NamedPositionTable` / `NamedLayerStates : NamedLayerStateTable`
- `RhinoDoc.Snapshots : SnapshotTable` / `SectionStyles : SectionStyleTable` / `Markups : MarkupTable` / `PageViewGroups : PageViewGroupTable`
- `RhinoDoc.RenderMaterials : RenderMaterialTable` / `RenderEnvironments : RenderEnvironmentTable` / `RenderTextures : RenderTextureTable`
- `RhinoDoc.Linetypes : LinetypeTable` / `Lights : LightTable` / `DimStyles : DimStyleTable` / `HatchPatterns : HatchPatternTable`
- `RhinoDoc.Bitmaps : BitmapTable` / `Strings : StringTable` / `Manifest : ManifestTable`
- face binding routes through `DimStyles` rows — `DimensionStyle.Font` get/set, `Font.FromQuartetProperties(quartetName, bold, italic) : Font` null on an unresolvable quartet, landed by `DimStyleTable.Add(dimstyle, reference) : int`
- `LinetypeTable.PurgeUnused() : int` / `DimStyleTable.PurgeUnused() : int` / `HatchPatternTable.PurgeUnused() : int` / `InstanceDefinitionTable.PurgeUnused() : int`

[ENTRYPOINT_SCOPE]: units and tolerances
- `RhinoDoc.ModelAbsoluteTolerance` / `ModelRelativeTolerance` / `ModelAngleToleranceRadians` / `ModelAngleToleranceDegrees : double`
- `RhinoDoc.PageAbsoluteTolerance` / `PageRelativeTolerance` / `PageAngleToleranceRadians` / `PageAngleToleranceDegrees : double`
- `RhinoDoc.ModelUnits` / `PageUnits : LengthUnit`; `ModelUnitSystem` / `PageUnitSystem : UnitSystem`
- `LengthUnit.FromKnownUnitSystem(UnitSystem knownUnitSystem)` / `FromCustomUnitSystem(string name, double customUnitSize, UnitSystem knownUnitSystem)` : `LengthUnit`
- `LengthUnit.ToUnitSystem(out double metersPerUnit) : UnitSystem` / `IsUnset(in LengthUnit)` / `IsNone(in LengthUnit)` / `IsCustom(in LengthUnit) : bool` / `Name : string`
- `RhinoDoc.ModelDistanceDisplayPrecision` / `PageDistanceDisplayPrecision : int`
- `RhinoMath.ToRadians(double degrees) : double` / `ToDegrees(double radians) : double` — the angular-unit conversion pair
- `RhinoDoc.AdjustLengthUnits(bool modelUnits, LengthUnit units, bool scale) : bool` — set the length unit with optional geometry scaling
- `RhinoDoc.AdjustModelUnitSystem(UnitSystem newUnitSystem, bool scale) : void` / `AdjustPageUnitSystem(UnitSystem newUnitSystem, bool scale) : void`
- `RhinoDoc.GetCustomUnitSystem(bool modelUnits, out string customUnitName, out double metersPerCustomUnit) : bool`
- `RhinoDoc.SetCustomUnitSystem(bool modelUnits, string customUnitName, double metersPerCustomUnit, bool scale) : bool`

[ENTRYPOINT_SCOPE]: unit and scale text
- `Rhino.LengthValue.Create(string s, StringParserSettings ps, out bool parsedAll) : LengthValue` — locale-aware length parse; `parsedAll` proves whole-string consumption, `IsUnset()` marks failure
- `Rhino.LengthValue.Create(double length, UnitSystem us, StringFormat format[, uint localeId])` / `Create(double length, LengthUnit units, StringFormat format[, uint localeId]) : LengthValue` — format a value into length text
- `LengthValue.LengthString : string` / `Units : LengthUnit` / `UnitSystem : UnitSystem` / `ParseSettings : StringParserSettings` / `ContextLocaleId : uint` / `ContextAngleUnitSystem : AngleUnitSystem`
- `LengthValue.Length() : double` / `Length(LengthUnit units)` / `Length(UnitSystem units) : double` — unit-converted egress; `ChangeLength(double)` / `ChangeUnits(LengthUnit)` / `ChangeUnitSystem(UnitSystem) : LengthValue`; `IsUnset() : bool`
- `LengthValue.StringFormat` — nested `byte` enum: `ExactDecimal` / `ExactProperFraction` / `ExactImproperFraction` / `CleanDecimal` / `CleanProperFraction` / `CleanImproperFraction`
- `Rhino.ScaleValue.Create(string s, StringParserSettings ps)` / `Create(LengthValue left, LengthValue right, ScaleStringFormat format) : ScaleValue`; `OneToOne() : ScaleValue`; `IsUnset() : bool`
- `ScaleValue.LeftToRightScale` / `RightToLeftScale : double`; `LeftLengthValue()` / `RightLengthValue() : LengthValue` — a unitless ratio parses to `LengthUnit.None` on both sides
- `ScaleValue.ScaleStringFormat` — nested `byte` enum: `None` / `RatioFormat` / `EquationFormat` / `FractionFormat` / `Unset`
- `Rhino.Input.StringParserSettings.DefaultParseSettings` / `ParseSettingsRadians` / `ParseSettingsDegrees` / `ParseSettingsIntegerNumber` / `ParseSettingsRationalNumber` / `ParseSettingsDoubleNumber` / `ParseSettingsRealNumber` / `ParseSettingsEmpty : StringParserSettings` — shared preset statics with inert `Dispose`; mutating a preset poisons every later parse process-wide
- `StringParserSettings.DefaultLengthUnitSystem : UnitSystem` / `DefaultAngleUnitSystem : AngleUnitSystem` / `PreferedLocaleId : uint` with the per-token `Parse*` grammar booleans (`ParseFeetInches`, `ParseSurveyorsNotation`, `ParseCommaAsDecimalPoint`, `ParseArithmeticExpression`, ...)
- `Rhino.Input.StringParser.ParseLengthExpession(string expression, StringParserSettings parse_settings_in, UnitSystem output_unit_system, out double value_out) : int` / `ParseAngleExpressionDegrees(string, out double)` / `ParseAngleExpressionRadians(string, out double) : bool` / `ParseNumber(string, int, StringParserSettings, ref StringParserSettings, out double) : int` — the host misspells `Expession` in the length/angle expression members

[ENTRYPOINT_SCOPE]: animation-capture spec
- `RhinoDoc.AnimationProperties : AnimationProperties` — GET mints a detached native copy, SET commits it; in-place mutation without the set-back is inert
- `new AnimationProperties()` / `new AnimationProperties(AnimationProperties source)` — fresh or copied spec; `Dispose()` releases the native carrier
- `AnimationProperties.CaptureTypes` — nested `uint` enum: `Path` / `Turntable` / `Flythrough` / `DaySunStudy` / `SeasonalSunStudy` / `None`; `CaptureType : CaptureTypes` selects the motion kind
- `AnimationProperties.CameraPoints` / `TargetPoints : Point3d[]` and `CameraPathId` / `TargetPathId : Guid` — point-row or path-curve motion tracks per slot
- `AnimationProperties.FrameCount` / `CurrentFrame : int`; `ViewportName : string`; `DisplayMode : Guid`
- `AnimationProperties.Latitude` / `Longitude` / `NorthAngle : double` — sun-study geography; `Start{Year,Month,Day,Hour,Minutes,Seconds}` / `End{Year,Month,Day,Hour,Minutes,Seconds} : int` — calendar window (years `1800..2199`)
- `AnimationProperties.DaysBetweenFrames` / `MinutesBetweenFrames : int` — seasonal versus one-day frame spacing
- `AnimationProperties.FolderName` / `FileExtension` / `AnimationName` / `HtmlFileName : string`; `HtmlFullPath : string` — derived output path
- `AnimationProperties.CaptureMethod : string` — `"preview"` or `"full"`; `RenderFull` / `RenderPreview : bool` — render-engine engagement per frame
- `AnimationProperties.Images` / `Dates : string[]` — host-written frame receipts; `LightIndex : int` is host preview state

[ENTRYPOINT_SCOPE]: undo records
- `RhinoDoc.BeginUndoRecord(string description) : uint` / `EndUndoRecord(uint undoRecordSerialNumber) : bool` — bracket a mutation into one record
- `RhinoDoc.AddCustomUndoEvent(string description, EventHandler<CustomUndoEventArgs> handler) : bool` / `AddCustomUndoEvent(string description, EventHandler<CustomUndoEventArgs> handler, object tag) : bool`
- `RhinoDoc.ClearUndoRecords(bool purgeDeletedObjects) : void` / `ClearUndoRecords(uint undoSerialNumber, bool purgeDeletedObjects) : void` / `ClearRedoRecords() : void`
- `RhinoDoc.Undo() : bool` / `Redo() : bool`; `UndoActive` / `RedoActive : bool` — replay discriminants
- `RhinoDoc.UndoRecordingEnabled : bool` (get/set) / `UndoRecordingIsActive : bool` / `CurrentUndoRecordSerialNumber : uint` — record enlistment state
- `Rhino.ApplicationSettings.HistorySettings` (static) — `RecordingEnabled : bool` / `RecordNextCommand : bool` (arms recording for one command only) / `UpdateEnabled : bool` / `ObjectLockingEnabled : bool` (history-bearing objects behave locked, editable only through inputs) / `BrokenRecordWarningEnabled : bool` — the process-wide history governance switches, all get/set

[ENTRYPOINT_SCOPE]: object-table mutation
- `ObjectTable.Add(GeometryBase geometry, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid`
- `ObjectTable.AddOrderedPointCloud(int xCt, int yCt, int zCt, Point3d[] box, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid`
- `ObjectTable.Replace(Guid objectId, GeometryBase geometry, bool ignoreModes) : bool`
- `ObjectTable.Delete(RhinoObject obj, bool quiet, bool ignoreModes) : bool` / `Delete(IEnumerable<Guid> objectIds, bool quiet) : int` / `Delete(Guid objectId, bool quiet) : bool`
- `ObjectTable.Transform(Guid objectId, Transform xform, bool deleteOriginal) : Guid` / `TransformWithHistory(Guid objectId, Transform xform) : Guid` / `TransformWithHistory(ObjRef objref, Transform xform) : Guid` / `TransformWithHistory(RhinoObject obj, Transform xform) : Guid`
- typed add family with history threading — every `Add<Kind>` mutation carries a `(..., ObjectAttributes attributes, HistoryRecord history, bool reference)` overload: `AddPoint` (Point3d and `Rhino.Geometry.Point`), `AddPointCloud` (PointCloud and IEnumerable<Point3d>), `AddClippingPlane` (single and plural viewport ids), `AddLine`, `AddPolyline`, `AddArc`, `AddCircle`, `AddEllipse`, `AddRectangle`, `AddBox`, `AddSphere`, `AddCurve`, `AddTextDot`, `AddText` (TextEntity and two raw-string forms), `AddLeader`, `AddLinearDimension`, `AddRadialDimension`, `AddAngularDimension`, `AddOrdinateDimension`, `AddCentermark`, `AddSurface`, `AddExtrusion`, `AddSubD`, `AddMesh` (one with `requireValidMesh`), `AddBrep` (one with `splitKinkySurfaces`), `AddClippingPlaneSurface`, `AddInstanceObject`, `AddHatch`, `AddMorphControl`; and `AddRhinoObject(Custom{Brep,Curve,Mesh,Point}Object, HistoryRecord history) : void` for the custom-object kinds.
- `ObjectTable.HistoryRecordCount : int` — document history-record census; no public history-record table exists, so this property is the only table-level history read
- `ObjectTable.ModifyAttributes(Guid objectId, ObjectAttributes newAttributes, bool quiet) : bool`
- `ObjectTable.Hide` / `Show` / `Lock` / `Unlock` `(Guid objectId, bool ignoreLayerMode) : bool` — object-mode transitions
- `ObjectTable.Select(IEnumerable<Guid> objectIds, bool select, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility) : int`
- `ObjectTable.SetSelectedObjects(IEnumerable<Guid> objectIds, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility) : int`
- `ObjectTable.UnselectAll(bool ignorePersistentSelections) : int` / `GetSelectedObjects(bool includeLights, bool includeGrips) : IEnumerable<RhinoObject>`
- `ObjectTable.Undelete(uint runtimeSerialNumber) : bool` / `Purge(uint runtimeSerialNumber) : bool` — deleted-object lifecycle by runtime serial
- `ObjectTable.ReplaceInstanceObject(Guid objectId, int instanceDefinitionIndex) : bool` / `FindId(Guid id) : RhinoObject`
- `ObjectTable.PickObjects(PickContext pickContext) : ObjRef[]` / `GetObjectList(ObjectEnumeratorSettings settings) : IEnumerable<RhinoObject>`
- `LayerTable.PurgeUnused() : int` / `GroupTable.PurgeUnused() : int`

[ENTRYPOINT_SCOPE]: view and named-view tables
- `ViewTable.ImportPageView(string filename, Guid mainViewportId, string pageName) : bool` — import a page view from an archive
- `ViewTable.Redraw(bool deferred) : void` / `ViewTable.PageViewCount : int`
- `ViewTable.EnableRedraw(bool enable, bool redrawDocument, bool redrawLayers) : void` / `RedrawEnabled : bool` — suppression bracket state
- `ViewTable.FlashObjects(IEnumerable<RhinoObject> list, bool useSelectionColor) : void`
- `NamedViewTable.Restore(int index, RhinoViewport viewport) : bool` — runs the non-animated direct restore
- `NamedViewTable.RestoreWithAspectRatio(int index, RhinoViewport viewport) : bool`
- `NamedViewTable.RestoreAnimatedConstantSpeed(int index, RhinoViewport viewport, double units_per_frame, int ms_delay) : bool`
- `NamedViewTable.RestoreAnimatedConstantTime(int index, RhinoViewport viewport, int frames, int ms_delay) : bool`
- `NamedViewTable.Add(string name, Guid viewportId) : int` / `FindByName(string name) : int`
- `NamedViewTable.Rename(int index, string newName) : bool` / `Delete(int index) : bool` — string-keyed overloads exist beside both
- `ViewTable.GetViewList(ViewTypeFilter filter) : RhinoView[]` — filtered census; `ViewTypeFilter` lives in `Rhino.Display`
- `ViewTable.Find(string mainViewportName, bool compareCase) : RhinoView` / `Find(Guid mainViewportId) : RhinoView` — main-viewport resolution only; detail ids silently miss
- `ViewTable.GetPageViews() : RhinoPageView[]`
- `ViewTable.AddPageView(string title) : RhinoPageView` / `AddPageView(string title, double pageWidth, double pageHeight) : RhinoPageView`
- `ViewTable.Delete(RhinoView view) : bool` / `ActiveView : RhinoView` (get/set) / `Redraw() : void` — no reorder member exists; page order is host-owned
- `PageViewGroupTable.FindName(string name) : PageViewGroup` / `FindIndex(int index) : PageViewGroup` / `Add(PageViewGroup group, IEnumerable<RhinoPageView> pages) : int`; `PageViewGroup.Name` / `Index`
- `PageViewGroup()` — public ctor with settable `Name`, minted before the table `Add`
- `RhinoPageView.Duplicate(bool duplicatePageGeometry) : RhinoPageView` — copies the page, optionally with its geometry
- `RhinoPageView.GetPageViewGroupList() : int[]` / `IsInPageViewGroup(int groupIndex) : bool` / `AddToPageViewGroup(int groupIndex) : bool` / `RemoveFromPageViewGroup(int groupIndex) : bool` — group membership per page

[ENTRYPOINT_SCOPE]: named layer states and named positions
- `NamedLayerStateTable.Save(string name, Guid viewportId) : int` / `Restore(string name, RestoreLayerProperties properties, Guid viewportId) : bool`
- `NamedLayerStateTable.Rename(string oldName, string newName) : bool` / `Delete(string name) : bool` / `Import(string filename) : int` / `Names : string[]`
- `NamedPositionTable.Save(string name, IEnumerable<Guid> objectIds) : Guid` / `Restore(string name) : bool` / `Update(string name) : bool`
- `NamedPositionTable.Delete(string name) : bool` / `Rename(string oldName, string name) : bool` / `Append(string name, IEnumerable<Guid> objectIds) : bool` / `Names : string[]`
- `RestoreLayerProperties` — carries the host restore-scope flags `NamedLayerStateTable.Restore` consumes

[ENTRYPOINT_SCOPE]: per-viewport layer overrides and clipping planes
- `Layer.SetPerViewportColor(Guid viewportId, Color color)` / `DeletePerViewportColor(Guid viewportId)`
- `Layer.SetPerViewportVisible(Guid viewportId, bool visible)` / `DeletePerViewportVisible(Guid viewportId)`
- `Layer.SetPerViewportPersistentVisibility(Guid viewportId, bool persistentVisibility)` / `UnsetPerViewportPersistentVisibility(Guid viewportId)`
- `Layer.SetPerViewportPlotColor(Guid viewportId, Color color)` / `DeletePerViewportPlotColor(Guid viewportId)`
- `Layer.SetPerViewportPlotWeight(Guid viewportId, double plotWeight)` / `DeletePerViewportPlotWeight(Guid viewportId)` / `DeletePerViewportSettings(Guid viewportId)`
- `LayerTable.FindByFullPath(string layerPath, int notFoundReturnValue) : int` / indexer `this[int index] : Layer`
- `ObjectTable.AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds, ObjectAttributes attributes) : Guid`
- `ObjectTable.FindClippingPlanesForViewport(RhinoViewport viewport) : ClippingPlaneObject[]`
- `ClippingPlaneObject.AddClipViewport(RhinoViewport viewport, bool commit) : bool` / `RemoveClipViewport(RhinoViewport viewport, bool commit) : bool` / `ClippingPlaneGeometry`
- `ClippingPlaneSurface.ViewportIds() : Guid[]` / `SetClipParticipation(IEnumerable<Guid> objectIds, IEnumerable<int> layerIndices, bool isExclusionList)`

[ENTRYPOINT_SCOPE]: light table
- `LightTable.Add(Light light) : int` / `Add(Light light, ObjectAttributes attributes) : int` — index of the added light, `-1` on failure
- `LightTable.Modify(Guid id, Light light) : bool` / `Modify(int index, Light light) : bool` — the id form resolves through `Find` with deleted lights included
- `LightTable.Delete(int index, bool quiet) : bool` / `Delete(LightObject item) : bool` / `Undelete(int index) : bool`
- `LightTable.Find(Guid id, bool ignoreDeleted) : int` / `FindName(string name) : LightObject` / `FindNameHash(NameHash nameHash) : LightObject` / `FindIndex(int index) : LightObject` — deleted lights have no name; the index form is host-discouraged in favor of ids
- `LightTable.Count : int` — excludes `Sun` and `Skylight`; indexer `this[int index] : LightObject` null on a purged slot
- `LightTable.Sun : Sun` / `Skylight : Skylight` — RDK-gated accessors throwing `RdkNotLoadedException` when the RDK is absent
- `LightTable.DefaultLight : LightObject` — fallback light `Rhino` renders with when no document light illuminates the scene
- `LightTable.ComponentType : ModelComponentType` — `ModelComponentType.RenderLight`; the table enumerates as `IEnumerable<LightObject>`

[ENTRYPOINT_SCOPE]: earth anchor and document state
- `RhinoDoc.EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable) / `Modified : bool` / `Name : string` / `Path : string` / `RenderSettings.Sun : Sun`
- `EarthAnchorPoint.EarthBasepointLatitude` / `EarthBasepointLongitude` / `EarthBasepointElevation : double` / `EarthBasepointElevationCoordinateSystem : int`
- `EarthAnchorPoint.ModelBasePoint : Point3d` / `ModelNorth` / `ModelEast : Vector3d` / `Name` / `Description : string`
- `EarthAnchorPoint.EarthLocationIsSet() : bool` / `ModelLocationIsSet() : bool`
- `EarthAnchorPoint.GetEarthAnchorPlane(out Vector3d anchorNorth) : Plane` / `GetModelCompass() : Plane` / `GetModelToEarthTransform(LengthUnit modelUnits) : Transform`
- `Sun.Latitude` / `Longitude` / `North : double` (get/set)

[ENTRYPOINT_SCOPE]: lifecycle events
- `RhinoDoc.BeginOpenDocument` / `EndOpenDocument` / `EndOpenDocumentInitialViewUpdate : EventHandler<DocumentOpenEventArgs>`
- `RhinoDoc.BeginSaveDocument` / `EndSaveDocument : EventHandler<DocumentSaveEventArgs>`
- `RhinoDoc.CloseDocument` / `NewDocument` / `ActiveDocumentChanged` / `DocumentPropertiesChanged : EventHandler<DocumentEventArgs>`
- `RhinoDoc.UnitsChangedWithScaling : EventHandler<UnitsChangedWithScalingEventArgs>` / `WorksessionFileChanged : EventHandler<WorksessionFileChangedEventArgs>`

[ENTRYPOINT_SCOPE]: object and table events
- `RhinoDoc.AddRhinoObject` / `DeleteRhinoObject : EventHandler<RhinoObjectEventArgs>` / `ReplaceRhinoObject : EventHandler<RhinoReplaceObjectEventArgs>`
- `RhinoDoc.ModifyObjectAttributes : EventHandler<RhinoModifyObjectAttributesEventArgs>`
- `RhinoDoc.BeforeTransformObjects : EventHandler<RhinoTransformObjectsEventArgs>` / `AfterTransformObjects : EventHandler<RhinoAfterTransformObjectsEventArgs>`
- `RhinoDoc.LayerTableEvent : EventHandler<LayerTableEventArgs>` / `MaterialTableEvent : EventHandler<MaterialTableEventArgs>` / `GroupTableEvent : EventHandler<GroupTableEventArgs>`
- `RhinoDoc.InstanceDefinitionTableEvent : EventHandler<InstanceDefinitionTableEventArgs>` / `SectionStyleTableEvent : EventHandler<SectionStyleTableEventArgs>`
- `RhinoDoc.MarkupTableEvent : EventHandler<MarkupTableEventArgs>` / `PageViewGroupTableEvent : EventHandler<PageViewGroupTableEventArgs>`
- `RhinoDoc.UndeleteRhinoObject` / `PurgeRhinoObject : EventHandler<RhinoObjectEventArgs>` — deleted-object lifecycle payloads
- `RhinoDoc.SelectObjects` / `DeselectObjects : EventHandler<RhinoObjectSelectionEventArgs>` / `DeselectAllObjects : EventHandler<RhinoDeselectAllObjectsEventArgs>`
- `RhinoDoc.LinetypeTableEvent : EventHandler<LinetypeTableEventArgs>` / `LightTableEvent : EventHandler<LightTableEventArgs>` / `DimensionStyleTableEvent : EventHandler<DimStyleTableEventArgs>`
- `RhinoDoc.HatchPatternTableEvent : EventHandler<HatchPatternTableEventArgs>` / `TextureMappingEvent : EventHandler<RhinoDoc.TextureMappingEventArgs>`
- `RhinoDoc.RenderMaterialsTableEvent` / `RenderEnvironmentTableEvent` / `RenderTextureTableEvent : EventHandler<RhinoDoc.RenderContentTableEventArgs>` — `RenderMaterialAssignmentChangedEventArgs` derives the assignment payload

## [04]-[IMPLEMENTATION_LAW]

[DOCUMENT_TOPOLOGY]:
- one `RhinoDoc` handle keyed by `RuntimeSerialNumber` projects every table; `FromRuntimeSerialNumber` re-resolves that handle inside a host callback where the reference is not carried
- live and headless documents expose one identical table, event, and undo surface; `IsHeadless` is the only discriminant a consumer branches on
- every table component is a `ModelComponent` keyed by `ModelComponentType`; the owning table mutates, and the component never writes itself back
- structural change is delivered through the object, table, and lifecycle event families; a `BeforeTransformObjects`/`AfterTransformObjects` pair brackets a transform the same way a table event brackets a table edit
- undo is a bracketed record between `BeginUndoRecord` and `EndUndoRecord`, and `AddCustomUndoEvent` folds non-geometry state into that same record

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the out-`bool` and out-parameter table lookups fold to `Fin<A>`/`Option<A>`, and a multi-object mutation batch accumulates failures through `Validation<Error, A>` where a single edit short-circuits on `Fin`
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `UnitSystem`, `ObjectType`, and `ModelComponentType` wrap as keyed `SmartEnum` policy owners, and `RuntimeSerialNumber` wraps as a `ValueObject` document key so the host `uint` never leaks
- `Hashing`(`libs/csharp/.api/api-hashing.md`): document and table content keys derive from `XxHash128` for the persistence artifact index

[LOCAL_ADMISSION]:
- Document-handle admission lives at the host-boundary tier as the single owner of live and headless document identity
- table mutation enters through the owning table accessor, never a raw component write
- event subscription enters through one watcher composition that binds the lifecycle, object, and table families together, never a scattered per-event handler

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: document identity, the component-table roster, lifecycle and mutation events, undo records, unit and tolerance regimes, object-table mutation
- Accept: live and headless document state, structural mutation, table query and selection
- Reject: standalone archive I/O, block-definition graph depth, interactive command acquisition
