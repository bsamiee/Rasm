# [RASM_RHINO_API_RHINOCOMMON_DOCUMENT]

`RhinoDoc` owns document identity across the live and headless runtimes — the component-table roster, the lifecycle and mutation event families, undo records, the model and page unit and tolerance regimes, and object-table mutation. Every table is a typed component collection reached from the one document handle keyed by `RuntimeSerialNumber`, and every structural change surfaces as an event the package binds rather than polls. Headless documents expose the identical table, event, and undo surface, which makes the document handle the single seam the exchange and block catalogs open geometry through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.DocObjects.Custom`, `Rhino.Collections`
- asset: `RhinoCommon.dll` — the in-process managed host assembly, verified by direct decompile
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and identity
- rail: host

| [INDEX] | [SYMBOL]               | [KIND] | [CAPABILITY]                                                               |
| :-----: | :--------------------- | :----- | :------------------------------------------------------------------------- |
|  [01]   | `RhinoDoc`             | class  | live/headless document root owning tables, units, tolerances, undo, events |
|  [02]   | `ModelComponent`       | class  | base of every table component; carries the component identity              |
|  [03]   | `ModelComponentType`   | enum   | component-kind discriminant shared across every table                      |
|  [04]   | `UnitSystem`           | enum   | model and page unit-system vocabulary                                      |
|  [05]   | `LengthUnit`           | struct | known + custom length-unit carrier behind `ModelUnits`/`PageUnits`         |
|  [06]   | `RhinoMath`            | static | tolerance and numeric constant surface                                     |
|  [07]   | `ArchivableDictionary` | class  | option payload for headless open and custom document state                 |

[PUBLIC_TYPE_SCOPE]: the component-table roster
- rail: host

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
- rail: host

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
- rail: host

- `DocumentOpenEventArgs` / `DocumentSaveEventArgs` / `DocumentEventArgs` — lifecycle open, save, and document-scope payloads
- `UnitsChangedWithScalingEventArgs` / `WorksessionFileChangedEventArgs` — unit-scale and worksession-file-change payloads
- `RhinoObjectEventArgs` / `RhinoReplaceObjectEventArgs` / `RhinoModifyObjectAttributesEventArgs` — object add, delete, replace, and attribute payloads
- `RhinoTransformObjectsEventArgs` / `RhinoAfterTransformObjectsEventArgs` — before and after transform payloads
- `LayerTableEventArgs` / `MaterialTableEventArgs` / `GroupTableEventArgs` — core table-mutation payloads
- `InstanceDefinitionTableEventArgs` / `SectionStyleTableEventArgs` / `MarkupTableEventArgs` / `PageViewGroupTableEventArgs` — definition, section-style, markup, and page-group table payloads
- `CustomUndoEventArgs` — custom non-geometry undo payload folded into an undo record

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: identity and lifecycle
- rail: host

- `RhinoDoc.RuntimeSerialNumber : uint` — the stable document key
- `RhinoDoc.FromRuntimeSerialNumber(uint serialNumber) : RhinoDoc` — re-resolve the handle across a callback boundary
- `RhinoDoc.OpenDocuments(bool includeHeadless) : RhinoDoc[]` — enumerate live and headless documents
- `RhinoDoc.Open(string filePath, out bool wasAlreadyOpen) : RhinoDoc` — open into the live runtime
- `RhinoDoc.CreateHeadless(string file3dmTemplatePath) : RhinoDoc` — construct a headless document from a template
- `RhinoDoc.OpenHeadless(string file3dmPath) : RhinoDoc` / `OpenHeadless(string filePath, ArchivableDictionary options) : RhinoDoc` — open headless with optional payload
- `RhinoDoc.Dispose() : void` — release the headless document
- `RhinoDoc.IsHeadless` / `IsAvailable` / `IsOpening` / `IsClosing` / `IsInitializing` / `IsCreating` / `IsReadOnly` / `IsLocked : bool` — readiness discriminants
- `RhinoDoc.ActiveDoc : RhinoDoc` — the ambient live document; `Modified : bool` — the dirty-save discriminant
- `RhinoDoc.InCommand(bool bIgnoreScriptRunnerCommands) : int` / `ActiveCommandId : Guid` / `InGetPoint : bool` — command and acquisition state
- `RhinoDoc.TimeoutActiveGet() : void` — cancel an active interactive get on the document

[ENTRYPOINT_SCOPE]: component-table accessors
- rail: host

- `RhinoDoc.Objects : ObjectTable` / `Layers : LayerTable` / `Materials : MaterialTable` / `Groups : GroupTable`
- `RhinoDoc.Views : ViewTable` / `NamedViews : NamedViewTable` / `InstanceDefinitions : InstanceDefinitionTable`
- `RhinoDoc.NamedConstructionPlanes : NamedConstructionPlaneTable` / `NamedPositions : NamedPositionTable` / `NamedLayerStates : NamedLayerStateTable`
- `RhinoDoc.Snapshots : SnapshotTable` / `SectionStyles : SectionStyleTable` / `Markups : MarkupTable` / `PageViewGroups : PageViewGroupTable`
- `RhinoDoc.RenderMaterials : RenderMaterialTable` / `RenderEnvironments : RenderEnvironmentTable` / `RenderTextures : RenderTextureTable`
- `RhinoDoc.Linetypes : LinetypeTable` / `Lights : LightTable` / `DimStyles : DimStyleTable` / `HatchPatterns : HatchPatternTable`
- `RhinoDoc.Bitmaps : BitmapTable` / `Strings : StringTable` / `Manifest : ManifestTable`
- `LinetypeTable.PurgeUnused() : int` / `DimStyleTable.PurgeUnused() : int` / `HatchPatternTable.PurgeUnused() : int` / `InstanceDefinitionTable.PurgeUnused() : int`

[ENTRYPOINT_SCOPE]: units and tolerances
- rail: host

- `RhinoDoc.ModelAbsoluteTolerance` / `ModelRelativeTolerance` / `ModelAngleToleranceRadians` / `ModelAngleToleranceDegrees : double`
- `RhinoDoc.PageAbsoluteTolerance` / `PageRelativeTolerance` / `PageAngleToleranceRadians` / `PageAngleToleranceDegrees : double`
- `RhinoDoc.ModelUnits` / `PageUnits : LengthUnit`; `ModelUnitSystem` / `PageUnitSystem : UnitSystem`
- `LengthUnit.FromKnownUnitSystem(UnitSystem knownUnitSystem)` / `FromCustomUnitSystem(string name, double customUnitSize, UnitSystem knownUnitSystem)` : `LengthUnit`
- `LengthUnit.ToUnitSystem(out double metersPerUnit) : UnitSystem` / `IsUnset(in LengthUnit)` / `IsNone(in LengthUnit)` / `IsCustom(in LengthUnit) : bool` / `Name : string`
- `RhinoDoc.ModelDistanceDisplayPrecision` / `PageDistanceDisplayPrecision : int`
- `RhinoDoc.AdjustLengthUnits(bool modelUnits, LengthUnit units, bool scale) : bool` — set the length unit with optional geometry scaling
- `RhinoDoc.AdjustModelUnitSystem(UnitSystem newUnitSystem, bool scale) : void` / `AdjustPageUnitSystem(UnitSystem newUnitSystem, bool scale) : void`
- `RhinoDoc.GetCustomUnitSystem(bool modelUnits, out string customUnitName, out double metersPerCustomUnit) : bool`
- `RhinoDoc.SetCustomUnitSystem(bool modelUnits, string customUnitName, double metersPerCustomUnit, bool scale) : bool`

[ENTRYPOINT_SCOPE]: undo records
- rail: host

- `RhinoDoc.BeginUndoRecord(string description) : uint` / `EndUndoRecord(uint undoRecordSerialNumber) : bool` — bracket a mutation into one record
- `RhinoDoc.AddCustomUndoEvent(string description, EventHandler<CustomUndoEventArgs> handler) : bool` / `AddCustomUndoEvent(string description, EventHandler<CustomUndoEventArgs> handler, object tag) : bool`
- `RhinoDoc.ClearUndoRecords(bool purgeDeletedObjects) : void` / `ClearUndoRecords(uint undoSerialNumber, bool purgeDeletedObjects) : void` / `ClearRedoRecords() : void`
- `RhinoDoc.Undo() : bool` / `Redo() : bool`; `UndoActive` / `RedoActive : bool` — replay discriminants
- `RhinoDoc.UndoRecordingEnabled : bool` (get/set) / `UndoRecordingIsActive : bool` / `CurrentUndoRecordSerialNumber : uint` — record enlistment state

[ENTRYPOINT_SCOPE]: object-table mutation
- rail: host

- `ObjectTable.Add(GeometryBase geometry, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid`
- `ObjectTable.AddOrderedPointCloud(int xCt, int yCt, int zCt, Point3d[] box, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid`
- `ObjectTable.Replace(Guid objectId, GeometryBase geometry, bool ignoreModes) : bool`
- `ObjectTable.Delete(RhinoObject obj, bool quiet, bool ignoreModes) : bool` / `Delete(IEnumerable<Guid> objectIds, bool quiet) : int`
- `ObjectTable.Transform(Guid objectId, Transform xform, bool deleteOriginal) : Guid` / `TransformWithHistory(Guid objectId, Transform xform) : Guid`
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
- rail: host

- `ViewTable.ImportPageView(string filename, Guid mainViewportId, string pageName) : bool` — import a page view from an archive
- `ViewTable.Redraw(bool deferred) : void` / `ViewTable.PageViewCount : int`
- `ViewTable.EnableRedraw(bool enable, bool redrawDocument, bool redrawLayers) : void` / `RedrawEnabled : bool` — suppression bracket state
- `ViewTable.FlashObjects(IEnumerable<RhinoObject> list, bool useSelectionColor) : void`
- `NamedViewTable.RestoreWithAspectRatio(int index, RhinoViewport viewport) : bool`
- `NamedViewTable.RestoreAnimatedConstantTime(int index, RhinoViewport viewport, int frames, int ms_delay) : bool`
- `NamedViewTable.Add(string name, Guid viewportId) : int` / `FindByName(string name) : int`
- `ViewTable.AddPageView(string title) : RhinoPageView` / `AddPageView(string title, double pageWidth, double pageHeight) : RhinoPageView`
- `ViewTable.Delete(RhinoView view) : bool` / `ReorderPageViews(IEnumerable<RhinoPageView> orderedPages) : bool` / `ActiveView : RhinoView` (get/set) / `Redraw() : void`
- `PageViewGroupTable.FindName(string name) : PageViewGroup` / `FindIndex(int index) : PageViewGroup` / `Add(PageViewGroup group, IEnumerable<RhinoPageView> pages) : int`; `PageViewGroup.Name` / `Index`

[ENTRYPOINT_SCOPE]: named layer states and named positions
- rail: host

- `NamedLayerStateTable.Save(string name, Guid viewportId) : int` / `Restore(string name, RestoreLayerProperties properties, Guid viewportId) : bool`
- `NamedLayerStateTable.Rename(string oldName, string newName) : bool` / `Delete(string name) : bool` / `Import(string filename) : int` / `Names : string[]`
- `NamedPositionTable.Save(string name, IEnumerable<Guid> objectIds) : Guid` / `Restore(string name) : bool` / `Update(string name) : bool`
- `NamedPositionTable.Delete(string name) : bool` / `Rename(string oldName, string name) : bool` / `Append(string name, IEnumerable<Guid> objectIds) : bool` / `Names : string[]`
- `RestoreLayerProperties` — the host restore-scope flags carrier consumed by `NamedLayerStateTable.Restore`

[ENTRYPOINT_SCOPE]: per-viewport layer overrides and clipping planes
- rail: host

- `Layer.SetPerViewportColor(Guid viewportId, Color color)` / `DeletePerViewportColor(Guid viewportId)`
- `Layer.SetPerViewportVisible(Guid viewportId, bool visible)` / `DeletePerViewportVisible(Guid viewportId)`
- `Layer.SetPerViewportPersistentVisibility(Guid viewportId, bool persistentVisibility)` / `UnsetPerViewportPersistentVisibility(Guid viewportId)`
- `Layer.SetPerViewportPlotColor(Guid viewportId, Color color)` / `DeletePerViewportPlotColor(Guid viewportId)`
- `Layer.SetPerViewportPlotWeight(Guid viewportId, double plotWeight)` / `DeletePerViewportPlotWeight(Guid viewportId)` / `DeletePerViewportSettings(Guid viewportId)`
- `LayerTable.FindByFullPath(string layerPath, int notFoundReturnValue) : int` / indexer `this[int index] : Layer`
- `ObjectTable.AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds, ObjectAttributes attributes) : Guid`
- `ObjectTable.FindClippingPlanesForViewport(RhinoViewport viewport) : ClippingPlaneObject[]` / `FindId(Guid objectId) : RhinoObject`
- `ObjectTable.ModifyAttributes(Guid objectId, ObjectAttributes newAttributes, bool quiet) : bool` / `Transform(Guid objectId, Transform xform, bool deleteOriginal) : Guid` / `Delete(Guid objectId, bool quiet) : bool`
- `ClippingPlaneObject.AddClipViewport(RhinoViewport viewport, bool commit) : bool` / `RemoveClipViewport(RhinoViewport viewport, bool commit) : bool` / `ClippingPlaneGeometry`
- `ClippingPlaneSurface.ViewportIds() : Guid[]` / `SetClipParticipation(IEnumerable<Guid> objectIds, IEnumerable<int> layerIndices, bool isExclusionList)`

[ENTRYPOINT_SCOPE]: earth anchor and document state
- rail: host

- `RhinoDoc.EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable) / `Modified : bool` / `Name : string` / `Path : string` / `RenderSettings.Sun : Sun`
- `EarthAnchorPoint.EarthBasepointLatitude` / `EarthBasepointLongitude` / `EarthBasepointElevation : double` / `EarthBasepointElevationCoordinateSystem : int`
- `EarthAnchorPoint.ModelBasePoint : Point3d` / `ModelNorth` / `ModelEast : Vector3d` / `Name` / `Description : string`
- `EarthAnchorPoint.EarthLocationIsSet() : bool` / `ModelLocationIsSet() : bool`
- `EarthAnchorPoint.GetEarthAnchorPlane(out Vector3d anchorNorth) : Plane` / `GetModelCompass() : Plane` / `GetModelToEarthTransform(LengthUnit modelUnits) : Transform`
- `Sun.Latitude` / `Longitude` / `North : double` (get/set)

[ENTRYPOINT_SCOPE]: lifecycle events
- rail: host

- `RhinoDoc.BeginOpenDocument` / `EndOpenDocument` / `EndOpenDocumentInitialViewUpdate : EventHandler<DocumentOpenEventArgs>`
- `RhinoDoc.BeginSaveDocument` / `EndSaveDocument : EventHandler<DocumentSaveEventArgs>`
- `RhinoDoc.CloseDocument` / `NewDocument` / `ActiveDocumentChanged` / `DocumentPropertiesChanged : EventHandler<DocumentEventArgs>`
- `RhinoDoc.UnitsChangedWithScaling : EventHandler<UnitsChangedWithScalingEventArgs>` / `WorksessionFileChanged : EventHandler<WorksessionFileChangedEventArgs>`

[ENTRYPOINT_SCOPE]: object and table events
- rail: host

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
- structural change is delivered through the object, table, and lifecycle event families; a `BeginTransformObjects`/`AfterTransformObjects` pair brackets a transform the same way a table event brackets a table edit
- undo is a bracketed record between `BeginUndoRecord` and `EndUndoRecord`, and `AddCustomUndoEvent` folds non-geometry state into that same record

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): the out-`bool` and out-parameter table lookups fold to `Fin<A>`/`Option<A>`, and a multi-object mutation batch accumulates failures through `Validation<Error, A>` where a single edit short-circuits on `Fin`
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `UnitSystem`, `ObjectType`, and `ModelComponentType` wrap as keyed `SmartEnum` policy owners, and `RuntimeSerialNumber` wraps as a `ValueObject` document key so the host `uint` never leaks
- `Hashing`(`libs/csharp/.api/api-hashing.md`): document and table content keys derive from `XxHash128` for the persistence artifact index

[LOCAL_ADMISSION]:
- the document handle is admitted at the host-boundary tier as the single owner of live and headless document identity
- table mutation enters through the owning table accessor, never a raw component write
- event subscription enters through one watcher composition that binds the lifecycle, object, and table families together, never a scattered per-event handler

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: document identity, the component-table roster, lifecycle and mutation events, undo records, unit and tolerance regimes, object-table mutation
- Accept: live and headless document state, structural mutation, table query and selection
- Reject: standalone archive I/O, block-definition graph depth, interactive command acquisition
