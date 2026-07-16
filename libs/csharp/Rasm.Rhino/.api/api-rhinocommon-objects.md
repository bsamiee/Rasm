# [RASM_RHINO_API_RHINOCOMMON_OBJECTS]

This catalog owns the live document-object surface: `RhinoObject` identity, state, subobject selection and highlight, mesh and render-mesh caches, texture and material resolution, on-object visual analysis, object frame, and section extraction; `ObjectAttributes` as the typed per-object display, source-vocabulary, per-viewport override, section-style, hatch, group, and frame program; and the parametric-history triad `HistoryRecord` (slot authoring), `ReplayHistoryData` (slot reads), and `ReplayHistoryResult` (geometry-update dispatch). Pick projection off `ObjRef` stays with the commands catalog, id-addressed table mutation and event binding stay with the document catalog, geometry custody stays with the geometry catalog, custom-object subclassing and grip editing move to the custom-objects catalog, and user strings and user data cross into the persistence catalog. Every live `RhinoObject` and `ObjectAttributes` value resolves inside the owning document grant and never leases outward.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon live-object surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon.dll` — verified by direct decompile
- namespaces: `Rhino.DocObjects`, `Rhino.Geometry`, `Rhino.Render`, `Rhino.Render.CustomRenderMeshes`, `Rhino.Commands`, `Rhino.UI.Gumball`, `Rhino.FileIO`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: object-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: live object and attributes
- rail: object-boundary

| [INDEX] | [SYMBOL]           | [KIND]     | [CAPABILITY]                                                                   |
| :-----: | :----------------- | :--------- | :----------------------------------------------------------------------------- |
|  [01]   | `RhinoObject`      | class      | live document-object read/mutate: state, subobjects, meshes, material, frame   |
|  [02]   | `ObjectAttributes` | class      | per-object display, source vocabularies, viewport overrides, groups, and frame |
|  [03]   | `ObjectFrameFlags` | flags enum | object-frame read policy on `RhinoObject` and `ObjectAttributes`               |

[PUBLIC_TYPE_SCOPE]: parametric-history triad
- rail: object-boundary

| [INDEX] | [SYMBOL]              | [KIND]               | [CAPABILITY]                                                         |
| :-----: | :-------------------- | :------------------- | :------------------------------------------------------------------- |
|  [01]   | `HistoryRecord`       | class, `IDisposable` | slot-keyed history authoring threaded into an add/transform mutation |
|  [02]   | `ReplayHistoryData`   | class, `IDisposable` | slot readers plus object-ref recovery during a replay callback       |
|  [03]   | `ReplayHistoryResult` | class                | existing-object geometry-update dispatch across all update kinds     |

[ENUM_ROSTERS]:
- `public enum Rhino.DocObjects.RhinoObject.ObjectFrameFlags` (`[Flags]`) — `Standard = 0`, `IncludeScaleTransforms = 1`, `ReturnUnset = 2`.
- `public enum Rhino.DocObjects.ObjectMode` — `Normal = 0`, `Hidden = 1`, `Locked = 2`, `InstanceDefinitionObject = 3`.
- `public enum Rhino.DocObjects.ActiveSpace` — `None = 0`, `ModelSpace = 1`, `PageSpace = 2`, `UVEditorSpace = 3`, `BlockEditorSpace = 4`.
- `public enum Rhino.DocObjects.ObjectDecoration` (`[Flags]`) — `None = 0`, `StartArrowhead = 8`, `EndArrowhead = 16`, `BothArrowhead = 24`.
- `public enum Rhino.DocObjects.ObjectLinetypeSource` — `LinetypeFromLayer = 0`, `LinetypeFromObject = 1`, `LinetypeFromParent = 3`.
- `public enum Rhino.DocObjects.ObjectColorSource` — `ColorFromLayer = 0`, `ColorFromObject = 1`, `ColorFromMaterial = 2`, `ColorFromParent = 3`.
- `public enum Rhino.DocObjects.ObjectPlotColorSource` — `PlotColorFromLayer = 0`, `PlotColorFromObject = 1`, `PlotColorFromDisplay = 2`, `PlotColorFromParent = 3`.
- `public enum Rhino.DocObjects.ObjectPlotWeightSource` — `PlotWeightFromLayer = 0`, `PlotWeightFromObject = 1`, `PlotWeightFromParent = 3`.
- `public enum Rhino.DocObjects.ObjectMaterialSource` — `MaterialFromLayer = 0`, `MaterialFromObject = 1`, `MaterialFromParent = 3`.
- `public enum Rhino.DocObjects.ObjectSectionAttributesSource` — `FromLayer = 0`, `FromObject = 1`, `FromParent = 2`, `FromSectioner = 3`.
- `public enum Rhino.DocObjects.ItemColorSource` — `ColorFromLayer = 0`, `ColorFromObject = 1`, `ColorFromParent = 3`, `ColorCustom = 4`.
- `public enum Rhino.DocObjects.SectionLabelStyle` — `None = 0`, `TextDotFromName = 1`, `TextFromName = 2`.
- `public enum Rhino.Geometry.MeshType` — `Default = 0`, `Render = 1`, `Analysis = 2`, `Preview = 3`, `Any = 4`.
- `public enum Rhino.Render.DecalMapping` — `None = -1`, `Planar = 0`, `Cylindrical = 1`, `Spherical = 2`, `UV = 3`.
- `public enum Rhino.Render.DecalProjection` — `None = -1`, `Forward = 0`, `Backward = 1`, `Both = 2`.
- `public enum Rhino.Render.CustomRenderMeshes.RenderMeshProvider.Flags` (`[Flags]`) — `None = 0`, `Canceled = 1`, `DisableCaching = 2`, `Recursive = 4`, `IsDocumentObject = 8`, `AlwaysCopyDocumentContent = 0x10`, `ReturnNullForStandardMaterial = 0x20`, `Incomplete = 0x40`; `Canceled` is the sole return-signalled bit.

## [03]-[ENTRYPOINTS]

[OBJECT_IDENTITY_STATE]:
- `Rhino.DocObjects.RhinoObject.ObjectType : ObjectType` — geometry-kind discriminant for query and dispatch.
- `Rhino.DocObjects.RhinoObject.RuntimeSerialNumber : uint` — process-stable object key; `FromRuntimeSerialNumber(uint serialNumber) : RhinoObject` re-resolves the handle across a callback boundary.
- `Rhino.DocObjects.RhinoObject.Id : Guid` / `Name : string` / `Document : RhinoDoc` — persistent identity and owning-document reads.
- `Rhino.DocObjects.RhinoObject.IsNormal` / `IsLocked` / `IsHidden` / `IsDeleted` / `IsReference` / `Visible : bool` — mode and lifecycle discriminants.
- `Rhino.DocObjects.RhinoObject.IsSolid` / `IsDeletable` / `IsPictureFrame` / `IsInstanceDefinitionGeometry : bool` — structural discriminants.
- `Rhino.DocObjects.RhinoObject.WorksessionReferenceSerialNumber` / `ReferenceModelSerialNumber` / `InstanceDefinitionModelSerialNumber : uint` — reference-provenance serials.
- `Rhino.DocObjects.RhinoObject.Geometry : GeometryBase` / `Attributes : ObjectAttributes` — the geometry-plus-attributes pair every object composes; attribute set duplicates the incoming value and stamps the object id.
- `Rhino.DocObjects.RhinoObject.CommitChanges() : bool` — flushes edited geometry and attributes back through the table; an edited attribute set never persists without it, and `true` means changes actually moved into the document — `false` on a clean working copy is a no-op, not a failure.
- `Rhino.DocObjects.RhinoObject.DuplicateGeometry() : GeometryBase` / `MemoryEstimate() : uint` — detached geometry copy and byte-footprint probe; `DuplicateGeometry` is public and non-virtual, so only `OnDuplicate` is the copy hook.
- `Rhino.DocObjects.RhinoObject.ShortDescription(bool plural) : string` / `ShortDescriptionWithClosedStatus(bool prepend, bool plural, out int status) : string` / `Description(TextLog textLog) : void` — human-facing kind labels.
- `Rhino.DocObjects.RhinoObject.NextRuntimeSerialNumber : uint` (static) — peeks the next serial the object table will mint.
- `Rhino.DocObjects.RhinoObject.IsActiveInViewport(RhinoViewport viewport) : bool` (public virtual) — per-viewport activeness resolving the active-in-viewport override family.
- `Rhino.DocObjects.RhinoObject.GetTightBoundingBox(IEnumerable<RhinoObject> rhinoObjects, out BoundingBox boundingBox) : bool` / `GetTightBoundingBox(IEnumerable<RhinoObject> rhinoObjects, Plane plane, out BoundingBox boundingBox) : bool` (static) — batch tight extent over an object set, world or plane-framed.

[OBJECT_SUBCLASSES]:
- concrete kind classes each carry a live-cast accessor plus a detached copy: `BrepObject.BrepGeometry : Brep` / `DuplicateBrepGeometry() : Brep`, `CurveObject.CurveGeometry : Curve` / `DuplicateCurveGeometry() : Curve`, `MeshObject.MeshGeometry : Mesh` / `DuplicateMeshGeometry() : Mesh`, `PointObject.PointGeometry : Point` / `DuplicatePointGeometry() : Point`, `SurfaceObject.SurfaceGeometry : Surface` / `DuplicateSurfaceGeometry() : Surface`, `ExtrusionObject.ExtrusionGeometry : Extrusion` / `DuplicateExtrusionGeometry() : Extrusion`, `PointCloudObject.PointCloudGeometry : PointCloud` / `DuplicatePointCloudGeometry() : PointCloud`.
- `Rhino.DocObjects.SubDObject` carries no typed geometry accessor — SubD reads go through the base `Geometry` member.
- `Rhino.DocObjects.InstanceObject` — `InstanceXform : Transform` / `InsertionPoint : Point3d` / `InstanceDefinition : InstanceDefinition` / `UsesDefinition(int definitionIndex, out int nestingLevel) : bool` / `SubObjectFromComponentIndex(ComponentIndex ci) : RhinoObject`; the `Explode` overloads live in the blocks catalog's operation rail.

[SUBOBJECT_SELECTION]:
- `Rhino.DocObjects.RhinoObject.Select(bool on) : int` / `Select(bool on, bool syncHighlight) : int` / `Select(bool on, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility) : int` — whole-object selection returning the resulting select count.
- `Rhino.DocObjects.RhinoObject.IsSelected(bool checkSubObjects) : int` / `IsSelectable() : bool` / `IsSelectable(bool ignoreSelectionState, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility) : bool` — selection-state and eligibility reads.
- `Rhino.DocObjects.RhinoObject.SelectSubObject(ComponentIndex componentIndex, bool select, bool syncHighlight) : int` / `SelectSubObject(ComponentIndex componentIndex, bool select, bool syncHighlight, bool persistentSelect) : int` — component-scoped selection.
- `Rhino.DocObjects.RhinoObject.IsSubObjectSelected(ComponentIndex componentIndex) : bool` / `IsSubObjectSelectable(ComponentIndex componentIndex, bool ignoreSelectionState) : bool` / `GetSelectedSubObjects() : ComponentIndex[]` / `UnselectAllSubObjects() : int` — component selection queries.
- `Rhino.DocObjects.RhinoObject.Highlight(bool enable) : bool` / `IsHighlighted(bool checkSubObjects) : int` — whole-object highlight.
- `Rhino.DocObjects.RhinoObject.HighlightSubObject(ComponentIndex componentIndex, bool highlight) : bool` / `IsSubObjectHighlighted(ComponentIndex componentIndex) : bool` / `GetHighlightedSubObjects() : ComponentIndex[]` / `UnhighlightAllSubObjects() : int` — component highlight.
- `Rhino.DocObjects.RhinoObject.GetSubObjects() : RhinoObject[]` — explodes the object into detached member objects the caller owns; the members are not live document state, and landing them is the caller's table add.
- `Rhino.DocObjects.RhinoObject.Select`/`IsSelected` grade contract — `0` unselected, `1` selected, `2` selected persistently; locked or hidden objects (and objects on locked or hidden layers) cannot select.

[GRIP_ENABLE_SEAM]:
- `Rhino.DocObjects.RhinoObject.GripsOn : bool` (get/set) / `GripsSelected : bool` — grip-editing enablement and selection state.
- `Rhino.DocObjects.RhinoObject.GetGrips() : GripObject[]` — enabled grips; the grip edit surface lives in the custom-objects catalog.
- `Rhino.DocObjects.RhinoObject.EnableCustomGrips(CustomObjectGrips customGrips) : bool` — installs a custom grip set authored in the custom-objects catalog.

[MESH_RENDER_CACHES]:
- `Rhino.DocObjects.RhinoObject.GetMeshes(MeshType meshType) : Mesh[]` / `MeshCount(MeshType meshType, MeshingParameters parameters) : int` / `CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters) : int` / `DestroyMeshes(MeshType meshType) : void` — the per-object render/analysis mesh cache lifecycle.
- `GetMeshes` custody — the returned meshes are non-owning const wrappers parented to the object (built through `ObjRef(parent, pGeometry)` over the cached pointer, never a duplicate), so mutation does not persist, the wrapper dies with the object's cache, and an owning consumer duplicates before the parent changes.
- `Rhino.DocObjects.RhinoObject.IsMeshable(MeshType meshType) : bool` / `GetRenderMeshParameters() : MeshingParameters` / `GetRenderMeshParameters(bool returnDocumentParametersIfUnset) : MeshingParameters` / `SetRenderMeshParameters(MeshingParameters mp) : bool` — meshability and per-object meshing policy; both `GetRenderMeshParameters` overloads mint a fresh caller-owned disposable `MeshingParameters`, never a borrowed carrier.
- `Rhino.DocObjects.RhinoObject.MeshObjects(IEnumerable<RhinoObject> rhinoObjects, MeshingParameters parameters, out Mesh[] meshes, out ObjectAttributes[] attributes) : Result` — batch meshing; the output mesh and attribute arrays are caller-owned, and three further overloads add a worker-thread flag, a simple-dialog `ref`, and a UI-style/transform pair.
- `Rhino.DocObjects.RhinoObject.RenderMeshes(MeshType mt, ViewportInfo vp, List<InstanceObject> ancestry, ref RenderMeshProvider.Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) : RenderMeshes` — the live custom-render-mesh accessor; static `GetRenderMeshes`/`GetRenderMeshesWithUpdatedTCs` are obsolete against it.
- `Rhino.DocObjects.RhinoObject.HasCustomRenderMeshes(MeshType mt, ViewportInfo vp, ref RenderMeshProvider.Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs) : bool` / `CustomRenderMeshesBoundingBox(MeshType mt, ViewportInfo vp, ref RenderMeshProvider.Flags flags, PlugIn plugin, DisplayPipelineAttributes attrs, out BoundingBox boundingBox) : bool` — custom-render-mesh presence and extent; the `RenderPrimitiveList` accessors are obsolete against these.
- `Rhino.DocObjects.RhinoObject.GetCustomRenderMeshParameter(Guid providerId, string parameterName) : IConvertible` / `SetCustomRenderMeshParameter(Guid providerId, string parameterName, object value) : void` — provider-keyed render-mesh parameters.
- `Rhino.DocObjects.RhinoObject.GetFillSurfaces(RhinoObject rhinoObject, IEnumerable<ClippingPlaneObject> clippingPlaneObjects, bool unclippedFills) : Brep[]` — section-fill surfaces against clipping planes; two lighter overloads drop the fill flag and the plane collection.

[TEXTURE_MATERIAL]:
- `Rhino.DocObjects.RhinoObject.GetTextureMapping(int channel) : TextureMapping` / `GetTextureMapping(int channel, out Transform objectTransform) : TextureMapping` / `SetTextureMapping(int channel, TextureMapping tm) : int` / `SetTextureMapping(int channel, TextureMapping tm, Transform objectTransform) : int` — per-channel texture-mapping read and mutate.
- `Rhino.DocObjects.RhinoObject.HasTextureMapping() : bool` / `GetTextureChannels() : int[]` — texture-channel presence and roster.
- `Rhino.DocObjects.RhinoObject.GetMaterial(bool frontMaterial) : Material` / `GetMaterial(ComponentIndex componentIndex) : Material` / `GetMaterial(ComponentIndex componentIndex, Guid plugInId) : Material` / `GetMaterial(ComponentIndex componentIndex, Guid plugInId, ObjectAttributes attributes) : Material` — resolved material for object or component.
- `Rhino.DocObjects.RhinoObject.GetRenderMaterial(bool frontMaterial) : RenderMaterial` / `GetRenderMaterial(ComponentIndex componentIndex) : RenderMaterial` / `GetRenderMaterial(ComponentIndex componentIndex, Guid plugInId) : RenderMaterial` / `GetRenderMaterial(ComponentIndex componentIndex, Guid plugInId, ObjectAttributes attributes) : RenderMaterial` — resolved render material for object or component.
- `Rhino.DocObjects.RhinoObject.RenderMaterial : RenderMaterial` / `HasSubobjectMaterials : bool` / `SubobjectMaterialComponents : ComponentIndex[]` — object render material and per-component material carriers.

[COMPONENT_MATERIAL_CARRIERS]:
- `Rhino.DocObjects.MaterialRefs : IDictionary<Guid, MaterialRef>` — per-plug-in material references keyed by plug-in id: `this[Guid key] : MaterialRef` (get throws `KeyNotFoundException`, set is add-or-replace) / `Count : int` / `Add(Guid key, MaterialRef value) : void` / `Remove(Guid key) : bool` / `ContainsKey(Guid key) : bool` / `TryGetValue(Guid key, out MaterialRef value) : bool` / `Clear() : void` / `Create(MaterialRefCreateParams createParams) : MaterialRef` (the sole constructor path; caller disposes the temp). Populating the dictionary slows every rendering-material query on the object — prefer `MaterialIndex` when one material suffices.
- `Rhino.DocObjects.MaterialRef : IDisposable` — `MaterialSource : ObjectMaterialSource` / `PlugInId : Guid` / `FrontFaceMaterialId : Guid` / `BackFaceMaterialId : Guid` / `FrontFaceMaterialIndex : int` / `BackFaceMaterialIndex : int` — the front/back per-face material carrier `HasSubobjectMaterials` fans out to; all reads, both constructors internal, and a `Create` product is a temp instance the caller disposes after `Add` copies it.
- `Rhino.DocObjects.MaterialRefCreateParams` — plain get/set carrier with an implicit default constructor: `PlugInId : Guid` / `MaterialSource : ObjectMaterialSource` / `FrontFaceMaterialId : Guid` / `FrontFaceMaterialIndex : int` / `BackFaceMaterialId : Guid` / `BackFaceMaterialIndex : int`.
- `MaterialRefs.Create` trap — the native marshalling pushes the front-face values through the back-face slot and back-face through front, so front and back swap across the boundary; consume the factory verbatim so a host repair never double-swaps.

[RENDER_MESHES_CARRIER]:
- `Rhino.Render.CustomRenderMeshes.RenderMeshes : IEnumerable<Instance>` — `InstanceCount : int` / `ObjectId : Guid` / `Document : RhinoDoc` / `Hash : uint` / `AddInstance(Instance instance) : void` — the return carrier of `RhinoObject.RenderMeshes`.

[GUMBALL_FRAME]:
- `Rhino.UI.Gumball.GumballFrame` (struct) — `Plane : Plane` / `ScaleGripDistance : Vector3d` / `ScaleMode : GumballScaleMode` (all get/set) — the out-type of `TryGetGumballFrame`/`TryGetGumballFrameForCurrentAlignment`.

[SECTION_STYLE]:
- `Rhino.DocObjects.SectionStyle : ModelComponent` — `BackgroundFillMode : SectionBackgroundFillMode` / `BackgroundFillColor : Color` / `BackgroundFillPrintColor : Color` / `BoundaryVisible : bool` / `BoundaryWidthScale : double` / `BoundaryPlotWeightMillimeters : double` / `BoundaryColor : Color` / `BoundaryPrintColor : Color` / `SectionFillRule : ObjectSectionFillRule` / `HatchIndex : int` / `HatchScale : double` / `HatchRotationRadians : double` / `HatchPatternColor : Color` / `HatchPatternPrintColor : Color` / `BoundaryLinetypeIndex : int` / `InUse : bool` / `GetBoundaryLinetype() : Linetype` / `SetBoundaryLinetype(Linetype) : void` / `RemoveBoundaryLinetype() : void` / `ReadFromFile(string filename, out SectionStyle[] sectionStyles, out HatchPattern[] hatchPatterns) : bool` (static) — the carrier `ObjectAttributes.Get/SetCustomSectionStyle` and `ComputedSectionStyle` exchange.

[VISUAL_ANALYSIS]:
- `Rhino.DocObjects.RhinoObject.EnableVisualAnalysisMode(VisualAnalysisMode mode, bool enable) : bool` — toggles an analysis mode registered through the display catalog on this object.
- `Rhino.DocObjects.RhinoObject.InVisualAnalysisMode() : bool` / `InVisualAnalysisMode(VisualAnalysisMode mode) : bool` / `GetActiveVisualAnalysisModes() : VisualAnalysisMode[]` — active-mode queries.
- `Rhino.DocObjects.RhinoObject.AnalysisModeChanged : EventHandler<RhinoObjectAnalysisModeChangedEventArgs>` (static event) — the notification seam paired with `EnableVisualAnalysisMode`.

[OBJECT_FRAME_SECTIONS]:
- `Rhino.DocObjects.RhinoObject.ObjectFrame() : Plane` / `ObjectFrame(ObjectFrameFlags flags) : Plane` — object-frame reads; the `RhinoObject.SetObjectFrame` overloads are obsolete, so frame writes route to `ObjectAttributes.SetObjectFrame`.
- `Rhino.DocObjects.RhinoObject.TryGetGumballFrame(out GumballFrame frame) : bool` / `TryGetGumballFrameForCurrentAlignment(out GumballFrame frame) : bool` — gumball-alignment frame reads.
- `Rhino.DocObjects.RhinoObject.HasDynamicTransform : bool` / `GetDynamicTransform(out Transform transform) : bool` — in-flight drag-transform probe.
- `Rhino.DocObjects.RhinoObject.CreateSections(Plane plane, string name, double tolerance, out ObjectAttributes[] objectAttributes) : GeometryBase[]` / `CreateSlices(Plane centerPlane, string name, double thickness, double tolerance, out ObjectAttributes[] objectAttributes) : GeometryBase[]` — section and slice extraction, each returning geometry paired with per-piece attributes.

[HISTORY_LINKAGE]:
- `Rhino.DocObjects.RhinoObject.HasHistoryRecord() : bool` / `SetHistory(HistoryRecord history) : bool` / `DeleteHistoryRecord() : bool` — per-object history attachment.
- `Rhino.DocObjects.RhinoObject.HistoryParents() : Guid[]` / `HistoryChildren() : Guid[]` — the object-history dependency edges feeding a parent/child topology.
- `Rhino.DocObjects.RhinoObject.CopyHistoryOnReplace() : bool` / `SetCopyHistoryOnReplace(bool bCopy) : void` — history-survival policy across a geometry replace.

[ATTRIBUTE_SOURCES_DISPLAY]:
- `Rhino.DocObjects.ObjectAttributes.Mode : ObjectMode` / `Space : ActiveSpace` / `Visible : bool` / `CastsShadows` / `ReceivesShadows : bool` — object mode, space, and shadow participation.
- `Rhino.DocObjects.ObjectAttributes.LinetypeSource : ObjectLinetypeSource` / `ColorSource : ObjectColorSource` / `PlotColorSource : ObjectPlotColorSource` / `PlotWeightSource : ObjectPlotWeightSource` / `MaterialSource : ObjectMaterialSource` / `SectionAttributesSource : ObjectSectionAttributesSource` — the six source-resolution vocabularies selecting layer, object, parent, or sectioner.
- `Rhino.DocObjects.ObjectAttributes.ObjectColor : Color` / `PlotColor : Color` / `PlotWeight : double` / `LinetypePatternScale : double` / `WireDensity : int` / `DisplayOrder : int` / `ObjectDecoration : ObjectDecoration` — display values applied under their source vocabulary.
- `Rhino.DocObjects.ObjectAttributes.ObjectId : Guid` / `Name : string` / `Url : string` / `LayerIndex : int` / `LinetypeIndex : int` / `MaterialIndex : int` / `ViewportId : Guid` — identity and table-index anchors.
- `Rhino.DocObjects.ObjectAttributes.DrawColor(RhinoDoc document) : Color` / `DrawColor(RhinoDoc document, Guid viewportId) : Color` / `ComputedPlotColor(RhinoDoc document) : Color` / `ComputedPlotColor(RhinoDoc document, Guid viewportId) : Color` / `ComputedPlotWeight(RhinoDoc document) : double` / `ComputedPlotWeight(RhinoDoc document, Guid viewportId) : double` — source-resolved effective values against a document and optional viewport.
- `Rhino.DocObjects.ObjectAttributes.Duplicate() : ObjectAttributes` / `Transform(Transform xform) : bool` / `ClearRenderingAttributes() : void` — detached copy, frame transform, and render-attribute reset.
- `Rhino.DocObjects.ObjectAttributes.IsInstanceDefinitionObject : bool` / `HasMapping : bool` — definition-membership and plug-in-mapping presence probes; `HasMapping` is the cheap gate before iterating channels.
- `Rhino.DocObjects.ObjectAttributes.OCSMappingChannelId : int` (static, `= 100000`) — the reserved channel id addressing the object's OCS mapping through `MappingChannel`.
- `Rhino.DocObjects.ObjectAttributes.SetUserString(string key, string value) : bool` / `GetUserString(string key) : string` / `GetUserStrings() : NameValueCollection` / `DeleteUserString(string key) : bool` / `DeleteAllUserStrings() : void` / `UserStringCount : int` — the attribute-set user-string store, physically here while the durable user-data law stays with the persistence catalog.

[ATTRIBUTE_VIEWPORT_OVERRIDES]:
- `Rhino.DocObjects.ObjectAttributes.HasDisplayModeOverride(Guid viewportId) : bool` / `GetDisplayModeOverride(Guid viewportId) : Guid` / `SetDisplayModeOverride(DisplayModeDescription mode) : bool` / `SetDisplayModeOverride(DisplayModeDescription mode, Guid rhinoViewportId) : bool` / `RemoveDisplayModeOverride() : void` / `RemoveDisplayModeOverride(Guid rhinoViewportId) : void` — per-viewport display-mode override query, set, and clear.
- `Rhino.DocObjects.ObjectAttributes.AddHideInDetailOverride(Guid detailId) : bool` / `RemoveHideInDetailOverride(Guid detailId) : bool` / `HasHideInDetailOverrideSet(Guid detailId) : bool` / `GetHideInDetailOverrides() : Guid[]` — per-detail hide overrides; `DetailBackgroundVisible : bool` carries the detail background flag.
- `Rhino.DocObjects.ObjectAttributes.GetActiveInViewportOverrides(out Guid[] viewportIds, out bool active) : bool` / `SetActiveInViewportOverrides(Guid[] viewportIds, bool active) : bool` / `HasActiveInViewportOverride(Guid viewportId, out bool active) : bool` / `AddActiveInViewportOverride(Guid viewportId, bool active) : bool` / `RemoveActiveInViewportOverride(Guid viewportId, bool active) : bool` — per-viewport active-state overrides.

[ATTRIBUTE_SECTION_HATCH]:
- `Rhino.DocObjects.ObjectAttributes.ComputedSectionStyle(RhinoDoc doc, ObjectAttributes sectionerAttributes, bool computeColors, Guid viewport_id) : SectionStyle` / `GetCustomSectionStyle() : SectionStyle` / `SetCustomSectionStyle(SectionStyle sectionStyle) : void` / `RemoveCustomSectionStyle() : void` / `SectionStyleIndex : int` / `ClippingPlaneLabelStyle : SectionLabelStyle` — resolved and custom section styling.
- `Rhino.DocObjects.ObjectAttributes.GetCustomLinetype() : Linetype` / `SetCustomLinetype(Linetype linetype) : void` / `RemoveCustomLinetype() : void` — per-object custom linetype overriding the linetype index.
- `Rhino.DocObjects.ObjectAttributes.HatchBackgroundFillColor : Color` / `HatchBackgroundFillPrintColor : Color` / `HatchBoundaryVisible : bool` / `HatchBoundaryColor : Color` / `HatchBoundaryPlotColor : Color` / `HatchBoundaryColorSource : ItemColorSource` / `HatchBoundaryPlotColorSource : ItemColorSource` / `HatchBoundaryPlotWeightMillimeters : double` — hatch fill and boundary display attributes.

[ATTRIBUTE_GROUPS_FRAME_RENDER]:
- `Rhino.DocObjects.ObjectAttributes.GetGroupList() : int[]` / `GroupCount : int` / `IsInGroup(int groupIndex) : bool` / `AddToGroup(int groupIndex) : void` / `RemoveFromGroup(int groupIndex) : void` / `RemoveFromAllGroups() : void` — group membership as an index set.
- `Rhino.DocObjects.ObjectAttributes.ObjectFrame() : Plane` / `SetObjectFrame(Transform xform) : void` / `SetObjectFrame(Plane plane) : void` — the live object-frame home replacing the obsolete `RhinoObject.SetObjectFrame`.
- `Rhino.DocObjects.ObjectAttributes.RenderMaterial : RenderMaterial` / `Decals : Decals` / `MaterialRefs : MaterialRefs` / `File3dmMeshModifiers : File3dmMeshModifiers` — render-content carriers on the attribute set.
- `Rhino.DocObjects.ObjectAttributes.CustomMeshingParameters : MeshingParameters` / `EnableCustomMeshingParameters : bool` — per-object meshing-parameter override.
- `Rhino.Render.Decals : IEnumerable<Decal>` — `Add(Decal decal) : uint` (returns the decal `CRC`, `0` implies failure; throws `DecalReadOnlyException` on a read-only attribute set) / `Remove(Decal decal) : bool` / `RemoveAllDecals() : void`; `Clear()` is obsolete against `RemoveAllDecals`, and the collection carries no indexer and no `Count`.
- `Rhino.Render.Decal.Create(DecalCreateParams createParams) : Decal` (static, null on native failure) — the decal mint; instance surface: `CRC : int` / `Mapping : DecalMapping` / `Projection : DecalProjection` / `Origin : Point3d` / `VectorUp : Vector3d` / `VectorAcross : Vector3d` / `Transparency : double` / `MapToInside : bool` / `IsVisible : bool` / `Height : double` / `Radius : double` / `StartLatitude` / `EndLatitude` / `StartLongitude` / `EndLongitude : double` (get-only) / `TextureInstanceId : Guid` / `GetTextureMapping() : TextureMapping` / `TryGetColor(Point3d point, Vector3d normal, ref Color4f colInOut, ref Point2d uvOut) : bool` / `HorzSweep`/`SetHorzSweep`, `VertSweep`/`SetVertSweep`, `GetUVBounds`/`SetUVBounds` over `double` bounds; `DecalMapping`/`DecalProjection` duplicate properties are obsolete aliases of `Mapping`/`Projection`.
- `Rhino.Render.DecalCreateParams` — all-get/set carrier: `TextureInstanceId : Guid` / `DecalMapping : DecalMapping` / `DecalProjection : DecalProjection` / `MapToInside : bool` / `Transparency : double` / `Origin : Point3d` / `VectorUp : Vector3d` / `VectorAcross : Vector3d` / `Height : double` / `Radius : double` / `StartLatitude` / `EndLatitude` / `StartLongitude` / `EndLongitude : double` / `MinU` / `MinV` / `MaxU` / `MaxV : double`.
- decal traps — `StartLatitude`/`EndLatitude` carry the HORIZONTAL sweep and `StartLongitude`/`EndLongitude` the VERTICAL (the host inverts the names; prefer `HorzSweep`/`VertSweep`); several live `Decal` setters call through the const pointer so mutating an enumerated instance does not persist — author decals through `DecalCreateParams` + `Decal.Create` + `Decals.Add`.
- `Rhino.FileIO.File3dmMeshModifiers` — five nullable render-modifier accessors, null when absent: `Displacement : File3dmDisplacement` / `EdgeSoftening : File3dmEdgeSoftening` / `Thickening : File3dmThickening` / `CurvePiping : File3dmCurvePiping` / `ShutLining : File3dmShutLining`.

[HISTORY_RECORD_SLOTS]:
- `Rhino.DocObjects.HistoryRecord.HistoryRecord(Command command, int version)` — mints a single-use leased native record keyed to a command and history version; `SetHistoryVersion(int historyVersion) : bool` re-stamps the version and `CopyOnReplaceObject : bool` sets replace-survival.
- `Rhino.DocObjects.HistoryRecord.SetBool(int id, bool value) : bool` / `SetInt(int id, int value) : bool` / `SetDouble(int id, double value) : bool` / `SetPoint3d(int id, Point3d value) : bool` / `SetVector3d(int id, Vector3d value) : bool` / `SetTransorm(int id, Transform value) : bool` / `SetColor(int id, Color value) : bool` / `SetGuid(int id, Guid value) : bool` / `SetString(int id, string value) : bool` — scalar slot setters keyed by int id; the host spells the transform setter `SetTransorm`.
- `Rhino.DocObjects.HistoryRecord.SetCurve(int id, Curve value) : bool` / `SetSurface(int id, Surface value) : bool` / `SetBrep(int id, Brep value) : bool` / `SetMesh(int id, Mesh value) : bool` — geometry slot setters.
- `Rhino.DocObjects.HistoryRecord.SetObjRef(int id, ObjRef value) : bool` / `SetPoint3dOnObject(int id, ObjRef objref, Point3d value) : bool` — object-reference slots; both register a history dependency that arms replay when the referenced object changes.
- `Rhino.DocObjects.HistoryRecord.SetBools(int id, IEnumerable<bool> values) : bool` / `SetInts(int id, IEnumerable<int> values) : bool` / `SetDoubles(int id, IEnumerable<double> values) : bool` / `SetPoint3ds(int id, IEnumerable<Point3d> values) : bool` / `SetVector3ds(int id, IEnumerable<Vector3d> values) : bool` / `SetColors(int id, IEnumerable<Color> values) : bool` / `SetGuids(int id, IEnumerable<Guid> values) : bool` / `SetStrings(int id, IEnumerable<string> values) : bool` — plural slot setters keyed by the same int id space.
- `Rhino.DocObjects.HistoryRecord.Handle : nint` — the wrapped native record pointer, read-only interop access beside the `Dispose` lifecycle.
- `Rhino.DocObjects.HistoryRecord.Dispose() : void` — releases the native record; a record threads into one `ObjectTable.Add`/`TransformWithHistory` and is never reused.

[HISTORY_REPLAY_READS]:
- `Rhino.DocObjects.ReplayHistoryData.Document : RhinoDoc` / `HistoryVersion : int` / `RecordId : Guid` / `Results : ReplayHistoryResult[]` — replay-callback context and the existing-result roster.
- `Rhino.DocObjects.ReplayHistoryData.GetRhinoObjRef(int id) : ObjRef` — recovers the live object reference stored by `SetObjRef`/`SetPoint3dOnObject`.
- `Rhino.DocObjects.ReplayHistoryData.TryGetBool(int id, out bool value) : bool` / `TryGetInt(int id, out int value) : bool` / `TryGetInts(int id, out int[] values) : bool` / `TryGetDouble(int id, out double value) : bool` / `TryGetDoubles(int id, out double[] values) : bool` / `TryGetPoint3d(int id, out Point3d value) : bool` / `TryGetPoint3dOnObject(int id, out Point3d value) : bool` / `TryGetVector3d(int id, out Vector3d value) : bool` / `TryGetTransform(int id, out Transform value) : bool` / `TryGetColor(int id, out Color value) : bool` / `TryGetGuid(int id, out Guid value) : bool` / `TryGetGuids(int id, out Guid[] values) : bool` / `TryGetString(int id, out string value) : bool` — slot readers mirroring the record setters, each false on a missing or mistyped slot.
- `Rhino.DocObjects.ReplayHistoryData.AppendHistoryResult() : ReplayHistoryResult` / `UpdateResultArray(IEnumerable<ReplayHistoryResult> newResults) : void` — grow or replace the result roster during replay; `Dispose() : void` releases the native reader.
- host lifecycle: the wrapper constructs `ReplayHistoryData` in a `using` scope and disposes it the moment `Command.ReplayHistory` returns, so no reader, result, or recovered `ObjRef` survives the callback; an exception in the override is swallowed into the exception report and the native callback answers `0`.

[HISTORY_RESULT_DISPATCH]:
- `Rhino.DocObjects.ReplayHistoryResult.ExistingObject : RhinoObject` — the object a replay updates in place; a replay drives `UpdateTo*` and never `ObjectTable.Add`/`Replace`.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToPoint(Point3d point, ObjectAttributes attributes) : bool` / `UpdateToTextDot(TextDot dot, ObjectAttributes attributes) : bool` / `UpdateToLine(Point3d from, Point3d to, ObjectAttributes attributes) : bool` / `UpdateToPolyline(IEnumerable<Point3d> points, ObjectAttributes attributes) : bool` — point, text-dot, and open-form updates.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToArc(Arc arc, ObjectAttributes attributes) : bool` / `UpdateToCircle(Circle circle, ObjectAttributes attributes) : bool` / `UpdateToEllipse(Ellipse ellipse, ObjectAttributes attributes) : bool` / `UpdateToSphere(Sphere sphere, ObjectAttributes attributes) : bool` / `UpdateToCurve(Curve curve, ObjectAttributes attributes) : bool` / `UpdateToSurface(Surface surface, ObjectAttributes attributes) : bool` — analytic-primitive and free-form curve/surface updates.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToExtrusion(Extrusion extrusion, ObjectAttributes attributes) : bool` / `UpdateToMesh(Mesh mesh, ObjectAttributes attributes) : bool` / `UpdateToSubD(SubD subD, ObjectAttributes attributes) : bool` / `UpdateToBrep(Brep brep, ObjectAttributes attributes) : bool` — solid and mesh-family updates.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToPointCloud(PointCloud cloud, ObjectAttributes attributes) : bool` / `UpdateToPointCloud(IEnumerable<Point3d> points, ObjectAttributes attributes) : bool` — point-cloud update from a cloud or raw points.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToClippingPlane(Plane plane, double uMagnitude, double vMagnitude, Guid clippedViewportId, ObjectAttributes attributes) : bool` / `UpdateToClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds, ObjectAttributes attributes) : bool` — clipping-plane update against one or many viewports.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToLinearDimension(LinearDimension dimension, ObjectAttributes attributes) : bool` / `UpdateToRadialDimension(RadialDimension dimension, ObjectAttributes attributes) : bool` / `UpdateToAngularDimension(AngularDimension dimension, ObjectAttributes attributes) : bool` / `UpdateToLeader(Leader leader, ObjectAttributes attributes) : bool` / `UpdateToHatch(Hatch hatch, ObjectAttributes attributes) : bool` — annotation and hatch updates.
- `Rhino.DocObjects.ReplayHistoryResult.UpdateToText(TextEntity text, ObjectAttributes attributes) : bool` / `UpdateToText(string text, Plane plane, double height, string fontName, bool bold, bool italic, TextJustification justification, ObjectAttributes attributes) : bool` / `UpdateToInstanceReferenceGeometry(InstanceReferenceGeometry instanceReference, ObjectAttributes attributes) : bool` — text-entity, raw-text, and block-reference updates.

## [04]-[IMPLEMENTATION_LAW]

[OBJECT_TOPOLOGY]:
- one `RhinoObject` is geometry plus one `ObjectAttributes` set, both resolved from the document by id inside a session grant; `RuntimeSerialNumber` keys the object and `FromRuntimeSerialNumber` re-resolves it where a host callback drops the reference.
- attribute edits stage on a duplicated set and never persist until `CommitChanges`, so a read-modify-write pass mutates a working copy and flushes once; the id-addressed `ObjectTable.ModifyAttributes` mutation in the document catalog is the alternate write path.
- source vocabularies decide resolution: each display value reads from layer, object, parent, material, display, or sectioner per its `*Source` enum, and the `Draw*`/`Computed*` members return the effective value that resolution yields against a document and viewport.
- per-viewport state is three override families — display mode, hide-in-detail, and active-in-viewport — each a query/add/remove set keyed by viewport or detail id, never a single scalar.
- history is a directed object graph: `HistoryParents`/`HistoryChildren` are the edges, a `HistoryRecord` is the per-object construction receipt, and replay re-runs a command against the recorded slots.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): `bool` and out-parameter object reads fold to `Fin<A>`/`Option<A>`; roster arrays land as `Seq<A>`; a multi-object selection or attribute batch accumulates failures through `Validation<Error, A>` where a single edit short-circuits on `Fin`; the disposable `HistoryRecord` and `ReplayHistoryData` ride a leased using-scope with failure-branch release.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the source, mode, space, decoration, and section-label vocabularies wrap as keyed `SmartEnum` policy owners; the `UpdateTo*` result kinds and the `HistoryRecord` slot kinds collapse into generated unions owning total dispatch over one geometry-plus-attributes payload.
- `Rasm` kernel: object-frame planes, gumball frames, section and slice geometry, and mesh parameters compose the kernel numeric owners; effective colors and plot weights project into detached receipts before leaving the document grant.

[LOCAL_ADMISSION]:
- a live `RhinoObject` and its `ObjectAttributes` remain inside the document grant; downstream code receives detached geometry copies, projected attribute receipts, or explicit leases, never the live handle.
- history authoring enters through a leased `HistoryRecord` populated by slot id inside a command, threaded once into `ObjectTable.Add`/`TransformWithHistory`; replay enters through the sealed `ReplayHistoryData` callback and drives `ReplayHistoryResult.UpdateTo*` on the existing object.
- grip editing crosses to the custom-objects catalog through `GetGrips`/`EnableCustomGrips`; visual-analysis registration crosses to the display catalog; pick projection and id-set selection cross to the commands and document catalogs.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects` live-object, attribute, and history reads
- Owns: object state and subobject selection, mesh/render/material/texture caches, on-object visual analysis, object frame and section extraction, the typed attribute program, and the record/replay/result history triad.
- Accept: object read and staged mutate, attribute display and override programs, section/slice extraction, and history authoring and replay projected onto `Fin`/`Option`/`Validation` rails.
- Reject: live document-bound objects or attribute sets crossing the session boundary, exception-style history outcomes, replay that adds or replaces table objects instead of updating existing results, and a cancel channel on the strict-`bool` replay contract.
