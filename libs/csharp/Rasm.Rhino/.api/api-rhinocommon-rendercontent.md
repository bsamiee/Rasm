# [RASM_RHINO_API_RHINOCOMMON_RENDERCONTENT]

`Rhino.Render` owns the RDK content object model: `RenderContent` is the abstract root every material, texture, and environment derives, carrying identity, a parent/child slot tree, a typed `FieldDictionary`, render-hash caching, XML/file IO, and UI/preview generation. `RenderMaterial`, `RenderTexture`, and `RenderEnvironment` specialize that root; `RenderContentType`, `ContentUuids`, `RenderContentSerializer`, and `CustomRenderContentAttribute` register and mint content by factory id; `RenderMaterialTable`/`RenderEnvironmentTable`/`RenderTextureTable` hold the document's content over `IRenderContentTable<T>`; eleven static events broadcast content mutation; and `TextureMapping` carries the mapping geometry a content graph reads. `api-rhinocommon-render.md` owns the disjoint renderer-lifecycle slice — `RenderTexture.CreateEvaluator`/`SimulateTexture` evaluation and `PostEffectExecutionControl` — so this catalog draws the seam at content authoring and configuration, never live evaluation. `api-rhinocommon-rendersettings.md` owns the document render-settings family the content graph binds into; `api-rhinocommon-document.md` owns `RhinoDoc.RenderMaterials`/`RenderEnvironments`/`RenderTextures` table accessors plus the `Rhino.DocObjects.Material`/`MaterialTable` document surface the material bridge targets; and `api-rhinocommon-geometry.md` owns the `Mesh`/`Plane`/`Sphere`/`Cylinder` carriers `TextureMapping` factories consume.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon RDK content surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render`, `Rhino.Render.Fields`
- kernel: `Rasm` (host-agnostic color, vector, and unit owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: render-content boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: content graph
- rail: render-content boundary

| [INDEX] | [SYMBOL]               | [KIND]           | [CAPABILITY]                                                              |
| :-----: | :--------------------- | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | `RenderContent`        | abstract root    | identity, child-slot tree, fields, render-hash, XML/file IO, UI, preview  |
|  [02]   | `RenderMaterial`       | content material | standard child slots, texture-usage reads, material/PBR simulation bridge |
|  [03]   | `RenderTexture`        | content texture  | projection, wrap, repeat/offset, mapping channel, local-mapping transform |
|  [04]   | `RenderEnvironment`    | content env      | environment simulation into `SimulatedEnvironment`                        |
|  [05]   | `SimulatedEnvironment` | bake carrier     | background color, image, and projection of a simulated environment        |
|  [06]   | `SimulatedTexture`     | bake carrier     | disposable baked-texture facsimile: file, UVW, channel, projection        |
|  [07]   | `TexturedValue<T>`     | textured slot    | typed value-plus-texture carrier read by `HandleTexturedValue<T>`         |

`RenderContent : IDisposable` — a minted-but-unattached content owns its native until table attach or disposal; `RenderContentType`, `SimulatedEnvironment`, and `SimulatedTexture` are likewise disposable leases.

[PUBLIC_TYPE_SCOPE]: typed field family
- rail: render-content boundary

`RenderContent.Fields` is one `FieldDictionary` whose polymorphic `Add`/`Set`/`TryGetValue` overloads discriminate on payload type; each add returns the matching typed `Field`, and `Field.FieldFromPointer` dispatches the concrete field on the native `Variant.Types`.

| [INDEX] | [SYMBOL]          | [KIND]              | [CAPABILITY]                                                        |
| :-----: | :---------------- | :------------------ | :------------------------------------------------------------------ |
|  [01]   | `FieldDictionary` | field owner         | typed add, set, and `TryGetValue` over one polymorphic surface      |
|  [02]   | `Field`           | abstract value      | `Name`, `Tag`, texture-amount bounds, `ValueAsObject`/`GetValue<T>` |
|  [03]   | typed `Field`     | 16 payload carriers | one concrete field per payload type, `Value` plus `ValueAsObject`   |

[PUBLIC_TYPE_SCOPE]: registry, collection, table, mapping
- rail: render-content boundary

`IRenderContentTable<T>` — internal contract all three document tables share: `Add`/`Remove`/`Find(Guid)`/`BeginChange`/`EndChange`, with count and indexer riding the internal `IRhinoTable<T>`. Archive-side render content (`Rhino.FileIO.File3dmRenderContent : ModelComponent`) is the file-IO catalog's projection, never this live-document surface.

| [INDEX] | [SYMBOL]                       | [KIND]             | [CAPABILITY]                                                         |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `RenderContentType`            | factory descriptor | type id, internal name, engine/plug-in id, `NewRenderContent`        |
|  [02]   | `ContentUuids`                 | static id vocab    | well-known material/environment/texture factory type ids             |
|  [03]   | `RenderContentSerializer`      | abstract IO        | custom file-format read/write registered by id                       |
|  [04]   | `CustomRenderContentAttribute` | registration attr  | flags a plug-in content class for `RegisterContent`                  |
|  [05]   | `RenderContentCollection`      | mutable set        | usage-filtered content set with iterator and tag cursor              |
|  [06]   | `RenderContentKindList`        | kind set           | additive `RenderContentKind` set with single-kind collapse           |
|  [07]   | `IRenderContentTable<T>`       | internal contract  | shared document-table member set the three tables implement          |
|  [08]   | `RenderMaterialTable`          | document table     | document render materials over `IRenderContentTable<RenderMaterial>` |
|  [09]   | `RenderEnvironmentTable`       | document table     | document render environments, same shape                             |
|  [10]   | `RenderTextureTable`           | document table     | document render textures, same shape                                 |
|  [11]   | `TextureMapping`               | `ModelComponent`   | mapping geometry, evaluation, and primitive recovery                 |
|  [12]   | `MappingTag`                   | mapping key        | mapping id, type, and mesh transform for a channel                   |
|  [13]   | `CachedTextureCoordinates`     | coord cache        | read-only per-vertex texture coordinates by mapping channel          |

[ENUM_ROSTERS]:
- `public enum Rhino.Render.RenderContentKind` — `None = 0`, `Material = 1`, `Environment = 2`, `Texture = 4`.
- `[Flags] public enum Rhino.Render.RenderContentStyles` — `None = 0`, `TextureSummary = 1`, `QuickPreview = 2`, `PreviewCache = 4`, `ProgressivePreview = 8`, `LocalTextureMapping = 0x10`, `GraphDisplay = 0x20`, `Adjustment = 0x80`, `Fields = 0x100`, `ModalEditing = 0x200`, `DynamicFields = 0x400`, `NameTypeSection = 0x800`.
- `public enum Rhino.Render.ProxyTypes` — `None`, `Single`, `Multi`, `Texture`.
- `public enum Rhino.Render.FilterContentByUsage` — `None`, `Used`, `Unused`, `UsedSelected`.
- `public enum Rhino.Render.RenderContent.ChangeContexts` — `UI`, `Drop`, `Program`, `Ignore`, `Tree`, `Undo`, `FieldInit`, `Serialize`, `RealTimeUI`, `Script`.
- `public enum Rhino.Render.RenderContent.MatchDataResult` — `None`, `Some`, `All`.
- `public enum Rhino.Render.RenderContent.ExtraRequirementsSetContexts` — `UI`, `Drop`, `Program`.
- `[Flags] public enum Rhino.Render.CrcRenderHashFlags : ulong` — `Normal = 0`, `ExcludeLinearWorkflow = 1`, `ExcludeLocalMapping = 2`, `ExcludeUnits = 4`, `ForSimulation = 1`, `ExcludeDocumentEffects = 0xD`.
- `public enum Rhino.Render.TextureMappingType` — `None`, `SurfaceParameters`, `PlaneMapping`, `CylinderMapping`, `SphereMapping`, `BoxMapping`, `MeshMappingPrimitive`, `SurfaceMappingPrimitive`, `BrepMappingPrimitive`, `OcsMapping`, `FalseColors`.
- `public enum Rhino.Render.TextureSpace` — `Single`, `Divided`.
- `public enum Rhino.Render.Projection` — `None`, `ClosestPoint`, `Ray`.
- `public enum Rhino.Render.RenderMaterial.StandardChildSlots` — `None = 0`, `Diffuse = 100`, `Transparency = 101`, `Bump = 102`, `Environment = 103`, `PbrBaseColor = 100` (aliases `Diffuse`), `PbrSubsurface = 104`, `PbrSubSurfaceScattering = 105`, `PbrSubsurfaceScatteringRadius = 106`, `PbrMetallic = 107`, `PbrSpecular = 108`, `PbrSpecularTint = 109`, `PbrRoughness = 110`, `PbrAnisotropic = 111`, `PbrAnisotropicRotation = 112`, `PbrSheen = 113`, `PbrSheenTint = 114`, `PbrClearcoat = 115`, `PbrClearcoatRoughness = 116`, `PbrOpacityIor = 117`, `PbrOpacity = 101` (aliases `Transparency`), `PbrOpacityRoughness = 118`, `PbrEmission = 119`, `PbrAmbientOcclusion = 120`, `PbrDisplacement = 121`, `PbrClearcoatBump = 122`, `PbrAlpha = 123`, `BlendAmount = 124`.
- `public enum Rhino.Render.RenderMaterial.AssignToSubFaceChoices` — `Keep`, `Remove`, `Ask`.
- `public enum Rhino.Render.RenderMaterial.AssignToBlockChoices` — `Always`, `Never`, `Ask`.
- `public enum Rhino.Render.RenderContentChangeReason` — `None`, `Attach`, `Detach`, `ChangeAttach`, `ChangeDetach`, `AttachUndo`, `DetachUndo`, `Open`, `Delete`; the `RenderContentEventArgs.Reason` payload.
- `public enum Rhino.Render.RenderContent.EmbedFilesChoice` — `NeverEmbed`, `AlwaysEmbed`, `AskUser`; the `SaveToFile` embed policy.
- `public enum Rhino.Render.DynamicIconUsage` — `TreeControl`, `SubnodeControl`, `ContentControl`, `General`.
- `public enum Rhino.Render.Utilities.PreviewQuality` — `None`, `Low`, `Medium`, `IntermediateProgressive`, `Full`; the `PreviewRenderedEventArgs.Quality` payload.
- `public enum Rhino.Render.RenderContent.ContentInstanceBrowserButtons` — `None`, `NewButton`, `EditButton`; the `ShowContentInstanceBrowser` button set.
- `public enum Rhino.Render.SimulatedTexture.ProjectionModes` / `SimulatedTexture.EnvironmentMappingModes` — the baked-texture projection and environment-mapping vocabularies.
- `public enum Rhino.Render.RenderMaterial.PreviewGeometryType` / `RenderMaterial.PreviewBackgroundType` — the default preview-pane geometry and background vocabularies.

## [03]-[ENTRYPOINTS]

[CONTENT_IDENTITY_TREE]:
- `Rhino.Render.RenderContent.TypeId : Guid` / `Id : Guid` / `GroupId : Guid` — factory type id shared across instances, unique instance id, and group id.
- `Rhino.Render.RenderContent.Name : string` / `Notes : string` / `Tags : string` / `Category : string` / `DisplayName : string` — content metadata; `SetName(string, bool renameEvents, bool ensureNameUnique)` renames with event and uniqueness control.
- `Rhino.Render.RenderContent.TypeName : string` / `TypeDescription : string` — abstract per-class identity the subclass supplies.
- `Rhino.Render.RenderContent.Styles : RenderContentStyles` / `ProxyType : ProxyTypes` / `ModelUnits : LengthUnit` — capability flags, proxy classification, and unit context.
- `Rhino.Render.RenderContent.TopLevel : bool` / `Hidden : bool` / `Private : bool` / `IsLocked : bool` / `CanBeEdited : bool` / `IsDefaultInstance : bool` / `IsHiddenByAutoDelete : bool` — content state predicates.
- `Rhino.Render.RenderContent.DocumentOwner : RhinoDoc` / `DocumentAssoc : RhinoDoc` — owning and associated document; `Document`/`DocumentRegistered` are obsolete.
- `Rhino.Render.RenderContent.Parent : RenderContent` / `FirstChild` / `NextSibling` / `TopLevelParent` / `ChildSlotName : string` / `ChildSlotDisplayName : string` — child-slot tree navigation and the localized slot name.
- `Rhino.Render.RenderContent.UseCount() : int` / `GetUnderlyingInstances(RenderContentCollection collection) : bool` — usage tally and underlying-instance census.
- `Rhino.Render.RenderContent.IsContentTypeAcceptableAsChild(Guid type, string childSlotName) : bool` / `IsFactoryProductAcceptableAsChild(DataSources.ContentFactory, string) : bool` / `IsFactoryProductAcceptableAsChild(Guid kindId, string factoryKind, string childSlotName) : bool` — virtual child-acceptance gates a subclass overrides.
- `Rhino.Render.RenderContent.ConvertUnits(LengthUnit from, LengthUnit to) : void` — virtual unit conversion; the `(UnitSystem, UnitSystem)` overload is obsolete.
- `Rhino.Render.RenderContent.FindChild(string childSlotName) : RenderContent` / `SetChild(RenderContent, string childSlotName) : bool` / `DeleteChild(string, ChangeContexts) : bool` / `DeleteAllChildren(ChangeContexts) : void` — child-slot graph mutation.
- `Rhino.Render.RenderContent.ChildSlotOn(string) : bool` / `SetChildSlotOn(string, bool, ChangeContexts)` / `ChildSlotAmount(string) : double` / `SetChildSlotAmount(string, double, ChangeContexts)` — per-slot enable and blend amount.
- `Rhino.Render.RenderContent.MakeGroupInstance() : RenderContent` / `Ungroup() : bool` / `UngroupRecursive() : bool` / `SmartUngroupRecursive() : bool` — content-group instancing and dissolution.

[CONTENT_FACTORY_IO]:
- `Rhino.Render.RenderContent.Create(RhinoDoc doc, Guid type) : RenderContent` / `Create(RhinoDoc, Type) : RenderContent` — construct content by factory type id or CLR type; parented overloads add `(RenderContent parent, string childSlotName)`, and the four `ShowContentChooserFlags` overloads are current interactive chooser variants carrying `doc` as the last parameter.
- `Rhino.Render.RenderContent.RegisterContent(PlugIn plugin) : Type[]` / `RegisterContent(Assembly, Guid pluginId) : Type[]` — registers a plug-in's `CustomRenderContentAttribute` content classes.
- `Rhino.Render.RenderContent.FromId(RhinoDoc document, Guid id) : RenderContent` / `FromXml(string xml, RhinoDoc doc) : RenderContent` — resolve content by instance id or XML; `FromXml(string)` and `AddPersistentRenderContent` are obsolete.
- `Rhino.Render.RenderContent.LoadFromFile(string filename) : RenderContent` / `SaveToFile(string filename, EmbedFilesChoice) : bool` — content file IO with embed policy.
- `Rhino.Render.RenderContent.MakeCopy() : RenderContent` / `Xml : string` / `Filename : string` / `GetEmbeddedFilesList() : string[]` / `FilesToEmbed : IEnumerable<string>` — copy, serialized form, and embedded-support-file roster.
- `Rhino.Render.RenderContent.Factory() : DataSources.ContentFactory` / `Replace(RenderContent newcontent) : bool` / `ForDisplay() : RenderContent` / `IsReference() : bool` — originating factory (`ContentFactory` lives in `Rhino.Render.DataSources`, not `Rhino.Render`), in-place replacement, display proxy, and reference probe.
- `Rhino.Render.RenderContent.ShowContentInstanceBrowser(RhinoDoc doc, ref Guid instance_id, RenderContentKind kinds, ContentInstanceBrowserButtons buttons) : bool` — content-instance picker dialog.
- Protected subclass hooks: `OnMakeCopy(ref RenderContent)`, `CalculateRenderHash2(CrcRenderHashFlags, string[]) : uint`, `OnAddUserInterfaceSections()`, `OnGetDefaultsInteractive() : bool`, `ModifyRenderContentStyles(RenderContentStyles add, RenderContentStyles remove)` — the copy, live-hash, UI-section, interactive-defaults, and style-mutation override surface a custom content class implements.

[CONTENT_PARAMS_FIELDS]:
- `Rhino.Render.RenderContent.Fields : FieldDictionary` — typed parameter dictionary; `BindParameterToField(string parameterName, Field field, ChangeContexts)` binds a named parameter to a field, and the four-argument `BindParameterToField(string parameterName, string childSlotName, Field field, ChangeContexts)` binds through a child slot.
- `Rhino.Render.RenderContent.GetParameter(string parameterName) : object` / `SetParameter(string parameterName, object value) : bool` — named-parameter read/write; the `ChangeContexts` overload of `SetParameter` is obsolete.
- `Rhino.Render.RenderContent.GetChildSlotParameter(string, string) : object` / `SetChildSlotParameter(string, string, object, ExtraRequirementsSetContexts) : bool` — child-slot extra-requirement parameters.
- `Rhino.Render.RenderContent.GetExtraRequirementParameter(string contentParameterName, string extraRequirementParameter) : object` / `SetExtraRequirementParameter(string, string, object, ExtraRequirementsSetContexts) : bool` — direct auto-UI extra-requirement read/write beside the child-slot pair.
- `Rhino.Render.RenderContent.BeginChange(ChangeContexts) : void` / `EndChange() : void` — the change-notification scope every field mutation rides.
- `Rhino.Render.RenderContent.BeginCreateDynamicFields(bool automatic)` / `CreateDynamicField(string internalName, string localName, string englishName, object value, object minValue, object maxValue, int sectionId) : bool` / `EndCreateDynamicFields()` — runtime dynamic-field construction.
- `Rhino.Render.RenderContent.ChildSlotNameFromParamName(string) : string` / `ParamNameFromChildSlotName(string) : string` — parameter-name to child-slot-name resolution.

[CONTENT_HASH]:
- `Rhino.Render.RenderContent.RenderHash : uint` — cached render hash; `SetIsRenderHashRecursive(bool)` sets recursive hashing.
- `Rhino.Render.RenderContent.RenderHashExclude(CrcRenderHashFlags flags, string excludeParameterNames) : uint` / `RenderHashExclude(CrcRenderHashFlags, string, LinearWorkflow) : uint` — hash with excluded axes; the `TextureRenderHashFlags` overload is obsolete.
- `Rhino.Render.RenderContent.MatchData(RenderContent oldContent) : MatchDataResult` / `IsCompatible(Guid renderEngineId) : bool` — content-migration match and render-engine compatibility.

[CONTENT_UI_PREVIEW]:
- `Rhino.Render.RenderContent.AddAutomaticUserInterfaceSection(string caption, int id) : bool` / `AddUserInterfaceSection(ICollapsibleSection section) : bool` / `GetUiHash() : ulong` — automatic and custom UI sections; the `(Type, string, bool, bool)` overload is obsolete.
- `Rhino.Render.RenderContent.OpenInEditor() : bool` / `Edit() : RenderContent` — modeless editor open and edit session; `OpenInModalEditor` is obsolete.
- `Rhino.Render.RenderContent.GenerateRenderContentPreview(LinearWorkflow lwf, RenderContent c, int width, int height, bool bSuppressLocalMapping, PreviewJobSignature pjs, PreviewAppearance pa, ref Utilities.PreviewRenderResult result) : Bitmap` — full content preview render.
- `Rhino.Render.RenderContent.GenerateQuickContentPreview(RenderContent c, int width, int height, PreviewSceneServer psc, bool bSuppressLocalMapping, int reason, ref Utilities.PreviewRenderResult result) : Bitmap` — quick preview; the `LinearWorkflow`-leading overload adds color correction, and `NewPreviewSceneServer(SceneServerData) : PreviewSceneServer` supplies the scene.
- `Rhino.Render.RenderContent.Icon(Size, out Bitmap) : bool` / `VirtualIcon(Size, out Bitmap) : bool` / `DynamicIcon(Size, out Bitmap, DynamicIconUsage) : bool` — content icon bitmaps.

[MATERIAL_BRIDGE]:
- `Rhino.Render.RenderMaterial.FromMaterial(Material material, RhinoDoc doc) : RenderMaterial` / `CreateBasicMaterial(Material, RhinoDoc) : RenderMaterial` / `CreateImportedMaterial(Material, RhinoDoc, bool reference) : RenderMaterial` — construct render material from a document `Material`.
- `Rhino.Render.RenderMaterial.ToMaterial(RenderTexture.TextureGeneration tg) : Material` / `SimulateMaterial(ref Material simulation, RenderTexture.TextureGeneration tg) : void` — bake to a document `Material`; the `bool isForDataOnly` simulate overloads are obsolete.
- `Rhino.Render.RenderMaterial.ConvertToPhysicallyBased(RenderTexture.TextureGeneration tg) : Rhino.DocObjects.PhysicallyBasedMaterial` — PBR document-material projection.
- `Rhino.Render.RenderMaterial.GetTextureFromUsage(StandardChildSlots slot) : RenderTexture` / `GetTextureOnFromUsage(StandardChildSlots) : bool` / `GetTextureAmountFromUsage(StandardChildSlots) : double` / `TextureChildSlotName(StandardChildSlots) : string` — standard texture-slot reads.
- `Rhino.Render.RenderMaterial.AssignTo(IEnumerable<ObjRef>, AssignToSubFaceChoices, AssignToBlockChoices, bool bInteractive) : bool` / `AssignTo(ObjRef) : bool` — object and sub-face material assignment.
- `Rhino.Render.RenderMaterial.SlotFromTextureType(TextureType) : StandardChildSlots` / `TextureTypeFromSlot(StandardChildSlots) : TextureType` — texture-type to standard-slot mapping.
- `Rhino.Render.RenderMaterial.PlasterMaterialGuid` / `PlasticMaterialGuid` / `PaintMaterialGuid` / `GlassMaterialGuid` / `GemMaterialGuid` / `MetalMaterialGuid` / `PictureMaterialGuid : Guid` — built-in material factory ids beside `ContentUuids`.
- `Rhino.Render.RenderMaterial.SmellsLikePlaster` / `SmellsLikePaint` / `SmellsLikeMetal` / `SmellsLikePlastic` / `SmellsLikeGem` / `SmellsLikeGlass : bool` and the six `SmellsLikeTextured*` twins — material-class heuristic predicates.
- `Rhino.Render.RenderMaterial.DefaultPreviewGeometryType : PreviewGeometryType` / `DefaultPreviewBackgroundType : PreviewBackgroundType` / `DefaultPreviewSize : double` — default preview-pane configuration (get/set).
- `Rhino.Render.RenderMaterial.ImportMaterialAndAssignToLayers(RhinoDoc doc, string file, IEnumerable<int> layer_indices) : bool` / `HandleTexturedValue<T>(string slotname, TexturedValue<T> tc) : bool` — RMTL import-and-assign and typed textured-slot read.

[TEXTURE_CONTENT]:
- `Rhino.Render.RenderTexture.GetProjectionMode() : TextureProjectionMode` / `SetProjectionMode(TextureProjectionMode, ChangeContexts)` / `GetWrapType() : TextureWrapType` / `SetWrapType(TextureWrapType, ChangeContexts)` — projection and wrap configuration.
- `Rhino.Render.RenderTexture.GetRepeat() : Vector3d` / `SetRepeat(Vector3d, ChangeContexts)` / `GetOffset() : Vector3d` / `SetOffset(Vector3d, ChangeContexts)` / `GetRotation() : Vector3d` / `SetRotation(Vector3d, ChangeContexts)` — the UVW repeat/offset/rotation triple.
- `Rhino.Render.RenderTexture.GetMappingChannel() : int` / `SetMappingChannel(int, ChangeContexts)` / `LocalMappingTransform : Transform` / `GetEnvironmentMappingMode() : TextureEnvironmentMappingMode` / `SetEnvironmentMappingMode(TextureEnvironmentMappingMode, ChangeContexts)` — mapping-channel binding and environment mapping.
- `Rhino.Render.RenderTexture.PixelSize(out int u, out int v, out int w) : void` / `PixelSize2 : (int width, int height, int depth)?` / `IsHdrCapable() : bool` / `IsLinear() : bool` / `IsNormalMap() : bool` / `IsImageBased() : bool` — texel dimensions and content-classification predicates.
- `Rhino.Render.RenderTexture.NewBitmapTexture(Bitmap bitmap, RhinoDoc doc) : RenderTexture` / `NewBitmapTexture(SimulatedTexture, RhinoDoc) : RenderTexture` / `SaveAsImage(string, int, int, int) : bool` — bitmap-backed texture construction and export.
- `Rhino.Render.RenderTexture.RenderHashWithoutLocalMapping : uint` / `RenderHashWithoutLocalMappingOrLinearWorkflow : uint` — pre-excluded hash reads beside `RenderHashExclude`.
- `Rhino.Render.RenderTexture.GetRepeatLocked() : bool` / `SetRepeatLocked(bool, ChangeContexts)` / `GetOffsetLocked() : bool` / `SetOffsetLocked(bool, ChangeContexts)` — UVW repeat/offset lock flags.
- `Rhino.Render.RenderTexture.GetPreviewIn3D() : bool` / `SetPreviewIn3D(bool, ChangeContexts)` / `GetPreviewLocalMapping() : bool` / `SetPreviewLocalMapping(bool, ChangeContexts)` / `GetDisplayInViewport() : bool` / `SetDisplayInViewport(bool, ChangeContexts)` — preview and viewport-display flags; each setter also carries a context-free overload.
- `Rhino.Render.RenderTexture.GetLocalMappingType() : eLocalMappingType` / `GetInternalEnvironmentMappingMode() : TextureEnvironmentMappingMode` / `GraphInfo(ref TextureGraphInfo)` / `SetGraphInfo(TextureGraphInfo)` — local-mapping classification and texture-graph info.
- `Rhino.Render.RenderTexture.GetEnvironmentMappingProjection(TextureEnvironmentMappingMode mode, Vector3d reflectionVector, out float u, out float v) : bool` / `GetWcsBoxMapping(Point3d worldXyz, Vector3d normal) : Point3d` — static environment-projection and WCS-box helpers.

[ENVIRONMENT_CONTENT]:
- `Rhino.Render.RenderEnvironment.NewBasicEnvironment(SimulatedEnvironment environment, RhinoDoc doc) : RenderEnvironment` / `TextureChildSlotName : string` — basic environment from a simulation and its texture slot.
- `Rhino.Render.RenderEnvironment.SimulateEnvironment(ref SimulatedEnvironment simulation, bool isForDataOnly) : void` / `SimulateEnvironment(bool isForDataOnly) : SimulatedEnvironment` — environment bake into a `SimulatedEnvironment`, by ref-fill or returning form.
- `Rhino.Render.SimulatedEnvironment.BackgroundColor : Color` / `BackgroundImage : SimulatedTexture` / `BackgroundProjection : SimulatedEnvironment.BackgroundProjections` — bake payload; `SimulatedEnvironment : IDisposable`, so every bake rides a lease; `ProjectionFromString(string)` / `StringFromProjection(BackgroundProjections)` round-trip the projection name.
- `Rhino.Render.SimulatedTexture(RhinoDoc doc)` / `SimulatedTexture(RhinoDoc, Texture)` — doc-aware constructors; the doc-free pair is obsolete.
- `Rhino.Render.SimulatedTexture.Filename : string` / `OriginalFilename : string` / `LocalMappingTransform : Transform` / `Repeat : Vector2d` / `Offset : Vector2d` / `Rotation : double` / `Repeating : bool` / `MappingChannel : int` / `ProjectionMode : ProjectionModes` — file, UVW, and projection payload.
- `Rhino.Render.SimulatedTexture.HasTransparentColor : bool` / `TransparentColor : Color4f` / `TransparentColorSensitivity : double` / `Filtered : bool` / `BitmapSize : int` (static get/set) — transparency and raster policy.
- `Rhino.Render.SimulatedTexture.SetMappingChannelAndProjectionMode(ProjectionModes, int mappingChannel, EnvironmentMappingModes)` / `Texture() : Texture` / `UnitsToMeters(RhinoDoc, double) : double` / `MetersToUnits(RhinoDoc, double) : double` — channel/projection write, `DocObjects.Texture` projection, and doc-aware unit bridges.

[FIELD_DICTIONARY]:
- `Rhino.Render.Fields.FieldDictionary.Add(string key, T value[, string prompt[, int sectionId]]) : <T>Field` — one polymorphic add per payload type, returning the matching typed field (`StringField`, `BoolField`, `IntField`, `FloatField`, `DoubleField`, `Color4fField`, `Vector2dField`, `Vector3dField`, `Point2dField`, `Point3dField`, `Point4dField`, `GuidField`, `TransformField`, `DateTimeField`, `ByteArrayField`, `NullField`).
- `Rhino.Render.Fields.FieldDictionary.AddTextured(string key, T value, string prompt, bool treatAsLinear, int sectionId) : <T>Field` / `AddFilename(string, string, string, int) : StringField` — textured and filename field variants.
- `Rhino.Render.Fields.FieldDictionary.Set(string key, T value) : void` / `TryGetValue(string key, out T value) : bool` / `TryGetValue<T>(string, out T) : bool` — payload-typed set and read; the `ChangeContexts` set overloads are obsolete.
- `Rhino.Render.Fields.FieldDictionary.ContainsField(string) : bool` / `GetField(string) : Field` / `RemoveField(string) : void` / `GetEnumerator() : IEnumerator<Field>` — field lookup, removal, and iteration.
- `Rhino.Render.Fields.Field.Name : string` / `Tag : object` / `ValueAsObject() : object` / `GetValue<T>() : T` — field identity and type-erased/typed value access.
- `Rhino.Render.Fields.Field.TextureAmountMin : double` / `TextureAmountMax : double` / `UseTextureOn : bool` / `UseTextureAmount : bool` / `IsHiddenInAutoUI : bool` — textured-field UI bounds and auto-UI visibility.

[REGISTRY]:
- `Rhino.Render.RenderContentType.GetAllAvailableTypes() : RenderContentType[]` / `NewRenderContent() : RenderContent` / `NewContentFromTypeId(Guid typeId) : RenderContent` / `NewContentFromTypeId(Guid typeId, RhinoDoc doc) : RenderContent` — factory enumeration and construction by type id; `RenderContentType(Guid typeId)` constructs a descriptor, and the type is `IDisposable`, so a census projects and disposes.
- `Rhino.Render.RenderContentType.Id : Guid` / `InternalName : string` / `RenderEngineId : Guid` / `PlugInId : Guid` — factory descriptor identity.
- `Rhino.Render.ContentUuids` — static well-known factory type ids. Materials: `BasicMaterialType`, `BlendMaterialType`, `V8BlendMaterialType`, `CompositeMaterialType`, `PlasterMaterialType`, `MetalMaterialType`, `PaintMaterialType`, `PlasticMaterialType`, `GemMaterialType`, `GlassMaterialType`, `PictureMaterialType`, `PhysicallyBasedMaterialType`, `DoubleSidedMaterialType`, `EmissionMaterialType`, `DisplayAttributeMaterialType`, `RealtimeDisplayMaterialType`, `DefaultMaterialInstance`. Environments: `BasicEnvironmentType`, `DefaultEnvironmentInstance`. Textures: `Texture2DCheckerTextureType`, `Texture3DCheckerTextureType`, `AdvancedDotTextureType`, `BitmapTextureType`, `SimpleBitmapTextureType`, `BlendTextureType`, `CubeMapTextureType`, `ExposureTextureType`, `FBmTextureType`, `GradientTextureType`, `GraniteTextureType`, `GridTextureType`, `HDRTextureType`, `EXRTextureType`, `MarbleTextureType`, `MaskTextureType`, `NoiseTextureType`, `PerlinMarbleTextureType`, `PerturbingTextureType`, `ProjectionChangerTextureType`, `ResampleTextureType`, `SingleColorTextureType`, `StuccoTextureType`, `TextureAdjustmentTextureType`, `TileTextureType`, `TurbulenceTextureType`, `WavesTextureType`, `WoodTextureType`, `AddTextureType`, `MultiplyTextureType`, `PhysicalSkyTextureType`. Bump textures: `HatchBumpTexture`, `CrossHatchBumpTexture`, `LeatherBumpTexture`, `WoodBumpTexture`, `SpeckleBumpTexture`, `GritBumpTexture`, `DotBumpTexture`. Collection instances: `BasicMaterialCCI`, `BlendMaterialCCI`, `CompositeMaterialCCI`, `BasicEnvironmentCCI`.
- `Rhino.Render.RenderContentSerializer(string fileExtension, RenderContentKind contentKind, bool canRead, bool canWrite)` — protected abstract constructor; `RegisterSerializer(Guid id) : bool` registers, and the host also discovers serializers through `RenderPlugIn.RenderContentSerializers()`.
- `Rhino.Render.RenderContentSerializer.Read(string pathToFile) : RenderContent` / `Write(string pathToFile, RenderContent, CreatePreviewEventArgs) : bool` — custom content-format read/write; `CanRead`/`CanWrite`/`FileExtension`/`ContentKind` describe the format and `EnglishDescription` (abstract) / `LocalDescription` name it.
- `Rhino.Render.RenderContentSerializer.CanLoadMultiple() : bool` / `LoadMultiple(RhinoDoc, IEnumerable<string>, RenderContentKind, LoadMultipleFlags) : bool` / `ReportContentAndFile(RenderContent, string, int)` / `ReportDeferredContentAndFile(RenderContent, string, int)` — the multi-file load surface over the nested `enum LoadMultipleFlags { Normal, Preload }`.
- `Rhino.Render.CustomRenderContentAttribute(string renderEngineGuid, bool imageBased, string category, bool is_elevated, bool is_built_in, bool is_private, bool is_linear, bool is_hdrcapable, bool is_normalmap)` — plug-in content registration attribute; a six-argument `(renderEngineGuid, imageBased, category, is_elevated, is_built_in, is_private)` constructor precedes it, and the named properties `RenderEngineId : Guid` (get), `ImageBased`, `IsLinear`, `IsHdrCapable`, `IsNormalMap`, `Category` (default `"General"`), `IsElevated`, `IsBuiltIn`, `IsPrivate` are settable at the attribute site.
- `Rhino.Render.ICurrentEnvironment` — `ForBackground` / `ForReflectionAndRefraction` / `ForLighting : RenderEnvironment` (get/set); the current-environment usage triple `RhinoDoc.CurrentEnvironment` returns — the accessor is `api-rhinocommon-document.md`'s.

[COLLECTION_KINDLIST]:
- `Rhino.Render.RenderContentCollection.Append(RenderContent content) : void` / `Set(RenderContentCollection) : void` / `Add(RenderContentCollection)` / `Remove(RenderContentCollection)` / `Clear() : void` — content-set composition; the collection is `IDisposable`.
- `Rhino.Render.RenderContentCollection.Count() : int` / `ContentAt(int) : RenderContent` / `Find_Sel(Guid) : RenderContent` / `Iterator() : ContentCollectionIterator` / `GetFilterContentByUsage() : FilterContentByUsage` — set access and usage filter; `ContentCollectionIterator` (`IDisposable`) cursors through `First()`/`Next() : RenderContent` and removes through `DeleteThis()`.
- `Rhino.Render.RenderContentCollection.GetForcedVaries() : bool` / `SetForcedVaries(bool)` / `GetSearchPattern() : string` / `SetSearchPattern(string)` / `ContentNeedsPreviewThumbnail(RenderContent) : bool` — editor selection-set state the content-editor seam reads.
- `Rhino.Render.RenderContentKindList.Add(RenderContentKind kind) : void` / `Contains(RenderContentKind) : bool` / `SingleKind() : RenderContentKind` / `Count() : int` — additive kind set with single-kind collapse; the list is `IDisposable`.

[CONTENT_TABLES]:
- `Rhino.Render.RenderMaterialTable.Count : int` / `this[int index] : RenderMaterial` — count and range-checked indexer that throws `IndexOutOfRangeException` out of range; both ride the internal `IRhinoTable<T>`.
- `Rhino.Render.RenderMaterialTable.Add(RenderMaterial) : bool` / `Remove(RenderMaterial) : bool` / `Find(Guid id) : RenderMaterial` — content table mutation and id lookup.
- `Rhino.Render.RenderMaterialTable.BeginChange(RenderContent.ChangeContexts) : void` / `EndChange() : void` / `GetEnumerator() : IEnumerator<RenderMaterial>` — table change scope and iteration; the three tables are `public sealed` over the internal `IRenderContentTable<T>` contract with byte-identical member sets, so `RenderEnvironmentTable` and `RenderTextureTable` repeat the shape over `RenderEnvironment`/`RenderTexture` with no per-table extras.

[CONTENT_EVENTS]:
- `Rhino.Render.RenderContent.ContentAdded` / `ContentRenamed` / `ContentDeleting` / `ContentDeleted` / `ContentReplacing` / `ContentReplaced` / `ContentUpdatePreview` / `CurrentEnvironmentChanged` — `EventHandler<RenderContentEventArgs>` lifecycle broadcasts; `CurrentEnvironmentChanged` carries no dedicated args type, its `EnvironmentUsageEx` field is the signal.
- `Rhino.Render.RenderContent.ContentChanged` — `EventHandler<RenderContentChangedEventArgs>`; the args derive `RenderContentEventArgs` and add `ChangeContext : RenderContent.ChangeContexts` and `OldContent : RenderContent`.
- `Rhino.Render.RenderContent.ContentFieldChanged` — `EventHandler<RenderContentFieldChangedEventArgs>`; the args derive `RenderContentChangedEventArgs` (so they also carry `ChangeContext`/`OldContent`) and add `FieldName : string`.
- `Rhino.Render.RenderContent.PreviewRendered` — `EventHandler<PreviewRenderedEventArgs>` carrying `Bitmap`, `PreviewJobSignature`, and `Quality`; eleven statics close the roster.
- `Rhino.Render.RenderContentEventArgs.Content : RenderContent` / `Document : RhinoDoc` / `Reason : RenderContentChangeReason` / `EnvironmentUsageEx : RenderSettings.EnvironmentUsage` — the shared event carrier; the `RenderEnvironment.Usage`-typed `EnvironmentUsage` twin is obsolete.

[TEXTURE_MAPPING]:
- `Rhino.Render.TextureMapping : ModelComponent` (`ComponentType => ModelComponentType.TextureMapping`); every constructor is internal, so a mapping is minted only through the statics or read off a `RhinoObject`.
- `Rhino.Render.TextureMapping.CreateSurfaceParameterMapping() : TextureMapping` / `CreatePlaneMapping(Plane, Interval dx, Interval dy, Interval dz) : TextureMapping` / `CreatePlaneMapping(Plane, Interval, Interval, Interval, bool capped) : TextureMapping` / `CreateOcsMapping(Plane) : TextureMapping` / `CreateCylinderMapping(Cylinder, bool capped) : TextureMapping` / `CreateSphereMapping(Sphere) : TextureMapping` / `CreateBoxMapping(Plane, Interval dx, Interval dy, Interval dz, bool capped) : TextureMapping` / `CreateCustomMeshMapping(Mesh) : TextureMapping` — the exact factory roster; an OCS mapping binds only on the `ObjectAttributes.OCSMappingChannelId` channel.
- `Rhino.Render.TextureMapping.MappingType : TextureMappingType` / `TextureSpace : TextureSpace` (get/set) / `Projection : Projection` (get/set) / `Capped : bool` (get/set) / `Id : Guid` — mapping classification and identity.
- `Rhino.Render.TextureMapping.UvwTransform : Transform` / `PrimitiveTransform : Transform` / `NormalTransform : Transform` — the mapping transform triple (get/set); `PrimativeTransform` is the obsolete misspelling.
- `Rhino.Render.TextureMapping.Evaluate(Point3d p, Vector3d n, out Point3d t) : int` / `Evaluate(Point3d, Vector3d, out Point3d, Transform pXform, Transform nXform) : int` — per-point evaluation returning the primitive side index.
- `Rhino.Render.TextureMapping.Decompose(Transform localTransform, out Vector3d xyz_position, out Vector3d xyz_scale, out Vector3d xyz_rotation, out Vector3d uvw_offset, out Vector3d uvw_repeat, out Vector3d uvw_rotation) : void` — UI-parity TRS/UVW decomposition of a local mapping transform.
- `Rhino.Render.TextureMapping.TryGetMappingBox(out Plane, out Interval, out Interval, out Interval) : bool` / `TryGetMappingPlane(out Plane, out Interval, out Interval, out Interval) : bool` — each with an `out bool capped` five-argument twin; `TryGetMappingSphere(out Sphere) : bool` / `TryGetMappingCylinder(out Cylinder) : bool` (+ `out bool capped` twin) / `TryGetMappingMesh(out Mesh) : bool` — primitive recovery from a mapping.
- `Rhino.DocObjects.RhinoObject.GetTextureMapping(int channel) : TextureMapping` / `GetTextureMapping(int channel, out Transform objectTransform) : TextureMapping` / `SetTextureMapping(int channel, TextureMapping) : int` / `SetTextureMapping(int, TextureMapping, Transform objectTransform) : int` / `HasTextureMapping() : bool` / `GetTextureChannels() : int[]` — the per-object channel binding; the channel is a raw `int`, `ObjectAttributes.OCSMappingChannelId` is the OCS constant, and no public document mapping table exists — the object carrier itself is `api-rhinocommon-objects.md`'s.
- `Rhino.Render.MappingTag.Id : Guid` / `MappingType : TextureMappingType` / `MappingCRC : uint` / `MeshTransform : Transform` (all get/set) / `CompareTo(MappingTag) : int` — per-channel mapping tag.
- `Rhino.Render.CachedTextureCoordinates : CommonObject, IList<Point3d>` — read-only (`IsReadOnly` true, mutators throw): `Dim : int` (2 = UV, 3 = UVW) / `MappingId : Guid` / `Count : int` / `this[int] : Point3d` / `TryGetAt(int, out double u, out double v, out double w) : bool` / `IndexOf(Point3d) : int` / `Contains(Point3d) : bool` / `CopyTo(Point3d[], int)`; retrieval rides the mesh seam the geometry catalog owns.

## [04]-[IMPLEMENTATION_LAW]

[CONTENT_TOPOLOGY]:
- `RenderContent` is the single abstract root; `RenderMaterial`, `RenderTexture`, and `RenderEnvironment` never carry a parallel identity, tree, or field surface — every specialization reads its base graph, and a fourth content kind is a `RenderContentType` factory row, never a new base.
- Content forms a child-slot tree: `Parent`/`FirstChild`/`NextSibling` walk it, `FindChild`/`SetChild`/`DeleteChild` mutate it by slot name, and every mutation names a `ChangeContexts` reason inside a `BeginChange`/`EndChange` scope — a raw field write outside that scope is the deleted form.
- Fields are the one parameter surface: `FieldDictionary` discriminates payload type at the `Add`/`Set`/`TryGetValue` overload, returns the matching typed `Field`, and `Field.FieldFromPointer` recovers the concrete field from the native `Variant.Types` — no per-type field accessor family exists beside it.
- Render-hash is derived and cached: `RenderHash` reads it, `RenderHashExclude` recomputes it over `CrcRenderHashFlags` axes, and content migration matches through `MatchData`, never through re-read equality.
- Content authoring and configuration are this catalog; live texture evaluation (`CreateEvaluator`, `SimulateTexture`) and post-effect execution are `api-rhinocommon-render.md` — one graph configures, the other evaluates, and the two never merge.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): content `bool` and index outcomes project to `Fin<Unit>`/`Fin<int>`; nullable content reads (`FromId`, `Find`, `FindChild`) lift to `Option<RenderContent>`; table and collection rosters land as `Seq<A>`; a `RenderContent`/`SimulatedEnvironment`/`TextureMapping` `IDisposable` rides a `using` bounded by `Eff`, and a failed create or refused assign crosses as `Fin<A>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `RenderContentKind`, `RenderContentStyles`, `ProxyTypes`, `ChangeContexts`, `TextureMappingType`, and `CrcRenderHashFlags` map at the edge to `[SmartEnum]`/flag owners; the polymorphic `Field` payload set collapses to one `[Union]` value owner keyed on the payload type the dictionary already discriminates.
- `api-rhinocommon-rendersettings.md`: `RenderSettings.RenderEnvironment(EnvironmentUsage, EnvironmentPurpose)` binds a `RenderEnvironment` into the document render environment, and the content event carrier reports `RenderSettings.EnvironmentUsage` — settings own the binding, this catalog owns the content.
- `api-rhinocommon-document.md`: `RhinoDoc.RenderMaterials`/`RenderEnvironments`/`RenderTextures` are the table accessors, and `RenderMaterial.ToMaterial`/`ConvertToPhysicallyBased` bridge to the `Rhino.DocObjects.Material`/`PhysicallyBasedMaterial` document surface the document catalog owns.
- `api-rhinocommon-geometry.md`: `TextureMapping` factories consume `Plane`/`Interval`/`Sphere`/`Cylinder`/`Mesh` and recovery returns them; the geometry catalog owns those carriers, this catalog owns the mapping.

[LOCAL_ADMISSION]:
- Content enters through `Create`, `FromId`, `FromXml`, `LoadFromFile`, or `<kind>.From*`; a live `RenderContent` stays inside the document grant, and downstream code receives a content reference, a detached field value, a projected receipt, or an owned bitmap/`Material` lease.
- `FieldDictionary` is the sole parameter owner; the render-hash rail is `RenderHashExclude` over bounded flags; a re-derived hash or a stringly parameter map beside the field dictionary is rejected.
- `RenderMaterialTable`/`RenderEnvironmentTable`/`RenderTextureTable` are the three document content owners over `IRenderContentTable<T>`; a per-kind bespoke table method family beside the shared contract is the collapsed form.

[RAIL_LAW]:
- Surface: `Rhino.Render` + `Rhino.Render.Fields` content graph
- Owns: the `RenderContent` object model (identity, child-slot tree, typed fields, render-hash, XML/file IO, UI/preview), the `RenderMaterial`/`RenderTexture`/`RenderEnvironment` specializations and material/PBR bridge, content registration and factory minting, the three content tables, the eleven content events, and `TextureMapping` geometry.
- Accept: content authoring and configuration through `Create`/`From*`/field mutation inside `BeginChange`/`EndChange`; polymorphic field access over `FieldDictionary`; content trapped through `Try.lift(...).Run()` with kind/style/context enums mapped to bounded owners; table and collection outcomes projected onto `Fin`/`Option`/`Seq` rails.
- Reject: a parallel identity/tree/field surface on a subclass, a per-payload field-accessor family beside `FieldDictionary`, a field write outside a change scope, a re-derived render hash, a live `RenderContent`/`TextureMapping` escaping into a domain signature, and the live-evaluation surface `api-rhinocommon-render.md` owns re-catalogued here.
