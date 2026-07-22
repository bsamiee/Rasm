# [RASM_RHINO_API_RHINOCOMMON_RENDERCONTENT]

`Rhino.Render` owns the RDK content object model: `RenderContent` is the abstract root every material, texture, and environment derives, carrying identity, a child-slot tree, a typed `FieldDictionary`, render-hash caching, XML/file IO, and UI/preview generation. `RenderMaterial`/`RenderTexture`/`RenderEnvironment` specialize that root, factory registration mints content by id, three document tables hold it over `IRenderContentTable<T>`, and `TextureMapping` carries the mapping geometry a content graph reads. Content authoring and configuration draw the seam here; `api-rhinocommon-render.md` owns the disjoint evaluation slice — `RenderTexture.CreateEvaluator`/`SimulateTexture` and `PostEffectExecutionControl` — so live evaluation never enters this catalog.

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

`RenderContent.Fields` is one `FieldDictionary` whose polymorphic `Add`/`Set`/`TryGetValue` overloads discriminate on payload type, each add returning the matching typed `Field`; `Field.FieldFromPointer` dispatches the concrete field on the native `Variant.Types`.

| [INDEX] | [SYMBOL]          | [KIND]           | [CAPABILITY]                                                        |
| :-----: | :---------------- | :--------------- | :------------------------------------------------------------------ |
|  [01]   | `FieldDictionary` | field owner      | typed add, set, and `TryGetValue` over one polymorphic surface      |
|  [02]   | `Field`           | abstract value   | `Name`, `Tag`, texture-amount bounds, `ValueAsObject`/`GetValue<T>` |
|  [03]   | typed `Field`     | payload carriers | one concrete field per payload type, `Value` plus `ValueAsObject`   |

[PUBLIC_TYPE_SCOPE]: registry, collection, table, mapping

`IRenderContentTable<T>` is the internal contract all three document tables share — `Add`/`Remove`/`Find(Guid)`/`BeginChange`/`EndChange`, with count and indexer over the internal `IRhinoTable<T>`; archive-side `File3dmRenderContent : ModelComponent` is the file-IO catalog's projection, never this live-document surface.

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
- `RenderContentKind`: `None=0`, `Material=1`, `Environment=2`, `Texture=4`.
- `RenderContentStyles` (`[Flags]`): `None=0`, `TextureSummary=1`, `QuickPreview=2`, `PreviewCache=4`, `ProgressivePreview=8`, `LocalTextureMapping=0x10`, `GraphDisplay=0x20`, `Adjustment=0x80`, `Fields=0x100`, `ModalEditing=0x200`, `DynamicFields=0x400`, `NameTypeSection=0x800`.
- `ProxyTypes`: `None`, `Single`, `Multi`, `Texture`.
- `FilterContentByUsage`: `None`, `Used`, `Unused`, `UsedSelected`.
- `RenderContent.ChangeContexts`: `UI`, `Drop`, `Program`, `Ignore`, `Tree`, `Undo`, `FieldInit`, `Serialize`, `RealTimeUI`, `Script`.
- `RenderContent.MatchDataResult`: `None`, `Some`, `All`.
- `RenderContent.ExtraRequirementsSetContexts`: `UI`, `Drop`, `Program`.
- `CrcRenderHashFlags` (`[Flags] : ulong`): `Normal=0`, `ExcludeLinearWorkflow=1`, `ExcludeLocalMapping=2`, `ExcludeUnits=4`, `ForSimulation=1`, `ExcludeDocumentEffects=0xD`.
- `TextureMappingType`: `None`, `SurfaceParameters`, `PlaneMapping`, `CylinderMapping`, `SphereMapping`, `BoxMapping`, `MeshMappingPrimitive`, `SurfaceMappingPrimitive`, `BrepMappingPrimitive`, `OcsMapping`, `FalseColors`.
- `TextureSpace`: `Single`, `Divided`.
- `Projection`: `None`, `ClosestPoint`, `Ray`.
- `RenderMaterial.StandardChildSlots`: `None=0`, `Diffuse=100`, `Transparency=101`, `Bump=102`, `Environment=103`, `PbrBaseColor=100`, `PbrSubsurface=104`, `PbrSubSurfaceScattering=105`, `PbrSubsurfaceScatteringRadius=106`, `PbrMetallic=107`, `PbrSpecular=108`, `PbrSpecularTint=109`, `PbrRoughness=110`, `PbrAnisotropic=111`, `PbrAnisotropicRotation=112`, `PbrSheen=113`, `PbrSheenTint=114`, `PbrClearcoat=115`, `PbrClearcoatRoughness=116`, `PbrOpacityIor=117`, `PbrOpacity=101`, `PbrOpacityRoughness=118`, `PbrEmission=119`, `PbrAmbientOcclusion=120`, `PbrDisplacement=121`, `PbrClearcoatBump=122`, `PbrAlpha=123`, `BlendAmount=124` (`PbrBaseColor` aliases `Diffuse`, `PbrOpacity` aliases `Transparency`).
- `RenderMaterial.AssignToSubFaceChoices`: `Keep`, `Remove`, `Ask`.
- `RenderMaterial.AssignToBlockChoices`: `Always`, `Never`, `Ask`.
- `RenderContentChangeReason`: `None`, `Attach`, `Detach`, `ChangeAttach`, `ChangeDetach`, `AttachUndo`, `DetachUndo`, `Open`, `Delete`; the `RenderContentEventArgs.Reason` payload.
- `RenderContent.EmbedFilesChoice`: `NeverEmbed`, `AlwaysEmbed`, `AskUser`; the `SaveToFile` embed policy.
- `DynamicIconUsage`: `TreeControl`, `SubnodeControl`, `ContentControl`, `General`.
- `Utilities.PreviewQuality`: `None`, `Low`, `Medium`, `IntermediateProgressive`, `Full`; the `PreviewRenderedEventArgs.Quality` payload.
- `RenderContent.ContentInstanceBrowserButtons`: `None`, `NewButton`, `EditButton`; the `ShowContentInstanceBrowser` button set.
- `SimulatedTexture.ProjectionModes` / `SimulatedTexture.EnvironmentMappingModes` — baked-texture projection and environment-mapping vocabularies.
- `RenderMaterial.PreviewGeometryType` / `RenderMaterial.PreviewBackgroundType` — default preview-pane geometry and background vocabularies.

## [03]-[ENTRYPOINTS]

[CONTENT_IDENTITY_TREE]:
- `RenderContent.TypeId : Guid` / `Id : Guid` / `GroupId : Guid` — factory type id shared across instances, unique instance id, group id.
- `RenderContent.Name : string` / `Notes : string` / `Tags : string` / `Category : string` / `DisplayName : string` — content metadata; `SetName(string, bool, bool)` renames with event and uniqueness control.
- `RenderContent.TypeName : string` / `TypeDescription : string` — abstract per-class identity the subclass supplies.
- `RenderContent.Styles : RenderContentStyles` / `ProxyType : ProxyTypes` / `ModelUnits : LengthUnit` — capability flags, proxy classification, unit context.
- `RenderContent.TopLevel : bool` / `Hidden` / `Private` / `IsLocked` / `CanBeEdited` / `IsDefaultInstance` / `IsHiddenByAutoDelete : bool` — content state predicates.
- `RenderContent.DocumentOwner : RhinoDoc` / `DocumentAssoc : RhinoDoc` — owning and associated document.
- `RenderContent.Parent : RenderContent` / `FirstChild` / `NextSibling` / `TopLevelParent` / `ChildSlotName : string` / `ChildSlotDisplayName : string` — child-slot tree navigation and localized slot name.
- `RenderContent.UseCount() : int` / `GetUnderlyingInstances(RenderContentCollection) : bool` — usage tally and underlying-instance census.
- `RenderContent.IsContentTypeAcceptableAsChild(Guid, string) : bool` / `IsFactoryProductAcceptableAsChild(DataSources.ContentFactory, string) : bool` / `IsFactoryProductAcceptableAsChild(Guid, string, string) : bool` — virtual child-acceptance gates a subclass overrides.
- `RenderContent.ConvertUnits(LengthUnit, LengthUnit) : void` — virtual unit conversion.
- `RenderContent.FindChild(string) : RenderContent` / `SetChild(RenderContent, string) : bool` / `DeleteChild(string, ChangeContexts) : bool` / `DeleteAllChildren(ChangeContexts) : void` — child-slot graph mutation.
- `RenderContent.ChildSlotOn(string) : bool` / `SetChildSlotOn(string, bool, ChangeContexts)` / `ChildSlotAmount(string) : double` / `SetChildSlotAmount(string, double, ChangeContexts)` — per-slot enable and blend amount.
- `RenderContent.MakeGroupInstance() : RenderContent` / `Ungroup() : bool` / `UngroupRecursive() : bool` / `SmartUngroupRecursive() : bool` — content-group instancing and dissolution.

[CONTENT_FACTORY_IO]:
- `RenderContent.Create(RhinoDoc, Guid) : RenderContent` / `Create(RhinoDoc, Type) : RenderContent` — construct by factory type id or CLR type; parented overloads add `(RenderContent parent, string childSlotName)`, and the `ShowContentChooserFlags` overloads carry `doc` last as the interactive chooser variants.
- `RenderContent.RegisterContent(PlugIn) : Type[]` / `RegisterContent(Assembly, Guid) : Type[]` — registers a plug-in's `CustomRenderContentAttribute` content classes.
- `RenderContent.FromId(RhinoDoc, Guid) : RenderContent` / `FromXml(string, RhinoDoc) : RenderContent` — resolve content by instance id or XML.
- `RenderContent.LoadFromFile(string) : RenderContent` / `SaveToFile(string, EmbedFilesChoice) : bool` — content file IO with embed policy.
- `RenderContent.MakeCopy() : RenderContent` / `Xml : string` / `Filename : string` / `GetEmbeddedFilesList() : string[]` / `FilesToEmbed : IEnumerable<string>` — copy, serialized form, embedded-support-file roster.
- `RenderContent.Factory() : DataSources.ContentFactory` / `Replace(RenderContent) : bool` / `ForDisplay() : RenderContent` / `IsReference() : bool` — originating factory (`ContentFactory` lives in `Rhino.Render.DataSources`), in-place replacement, display proxy, reference probe.
- `RenderContent.ShowContentInstanceBrowser(RhinoDoc, ref Guid, RenderContentKind, ContentInstanceBrowserButtons) : bool` — content-instance picker dialog.
- Protected subclass hooks: `OnMakeCopy(ref RenderContent)`, `CalculateRenderHash2(CrcRenderHashFlags, string[]) : uint`, `OnAddUserInterfaceSections()`, `OnGetDefaultsInteractive() : bool`, `ModifyRenderContentStyles(RenderContentStyles, RenderContentStyles)` — the copy, live-hash, UI-section, interactive-defaults, and style-mutation override surface a custom content class implements.

[CONTENT_PARAMS_FIELDS]:
- `RenderContent.Fields : FieldDictionary` — typed parameter dictionary; `BindParameterToField(string, Field, ChangeContexts)` binds a named parameter to a field, and `BindParameterToField(string, string, Field, ChangeContexts)` binds through a child slot.
- `RenderContent.GetParameter(string) : object` / `SetParameter(string, object) : bool` — named-parameter read/write.
- `RenderContent.GetChildSlotParameter(string, string) : object` / `SetChildSlotParameter(string, string, object, ExtraRequirementsSetContexts) : bool` — child-slot extra-requirement parameters.
- `RenderContent.GetExtraRequirementParameter(string, string) : object` / `SetExtraRequirementParameter(string, string, object, ExtraRequirementsSetContexts) : bool` — direct auto-UI extra-requirement read/write beside the child-slot pair.
- `RenderContent.BeginChange(ChangeContexts) : void` / `EndChange() : void` — the change-notification scope every field mutation rides.
- `RenderContent.BeginCreateDynamicFields(bool)` / `CreateDynamicField(string, string, string, object, object, object, int) : bool` / `EndCreateDynamicFields()` — runtime dynamic-field construction.
- `RenderContent.ChildSlotNameFromParamName(string) : string` / `ParamNameFromChildSlotName(string) : string` — parameter-name to child-slot-name resolution.

[CONTENT_HASH]:
- `RenderContent.RenderHash : uint` — cached render hash; `SetIsRenderHashRecursive(bool)` sets recursive hashing.
- `RenderContent.RenderHashExclude(CrcRenderHashFlags, string) : uint` / `RenderHashExclude(CrcRenderHashFlags, string, LinearWorkflow) : uint` — hash with excluded axes.
- `RenderContent.MatchData(RenderContent) : MatchDataResult` / `IsCompatible(Guid) : bool` — content-migration match and render-engine compatibility.

[CONTENT_UI_PREVIEW]:
- `RenderContent.AddAutomaticUserInterfaceSection(string, int) : bool` / `AddUserInterfaceSection(ICollapsibleSection) : bool` / `GetUiHash() : ulong` — automatic and custom UI sections.
- `RenderContent.OpenInEditor() : bool` / `Edit() : RenderContent` — modeless editor open and edit session.
- `RenderContent.GenerateRenderContentPreview(LinearWorkflow, RenderContent, int, int, bool, PreviewJobSignature, PreviewAppearance, ref Utilities.PreviewRenderResult) : Bitmap` — full content preview render.
- `RenderContent.GenerateQuickContentPreview(RenderContent, int, int, PreviewSceneServer, bool, int, ref Utilities.PreviewRenderResult) : Bitmap` — quick preview; the `LinearWorkflow`-leading overload adds color correction, and `NewPreviewSceneServer(SceneServerData) : PreviewSceneServer` supplies the scene.
- `RenderContent.Icon(Size, out Bitmap) : bool` / `VirtualIcon(Size, out Bitmap) : bool` / `DynamicIcon(Size, out Bitmap, DynamicIconUsage) : bool` — content icon bitmaps.

[MATERIAL_BRIDGE]:
- `RenderMaterial.FromMaterial(Material, RhinoDoc) : RenderMaterial` / `CreateBasicMaterial(Material, RhinoDoc) : RenderMaterial` / `CreateImportedMaterial(Material, RhinoDoc, bool) : RenderMaterial` — construct render material from a document `Material`.
- `RenderMaterial.ToMaterial(RenderTexture.TextureGeneration) : Material` / `SimulateMaterial(ref Material, RenderTexture.TextureGeneration) : void` — bake to a document `Material`.
- `RenderMaterial.ConvertToPhysicallyBased(RenderTexture.TextureGeneration) : Rhino.DocObjects.PhysicallyBasedMaterial` — PBR document-material projection.
- `RenderMaterial.GetTextureFromUsage(StandardChildSlots) : RenderTexture` / `GetTextureOnFromUsage(StandardChildSlots) : bool` / `GetTextureAmountFromUsage(StandardChildSlots) : double` / `TextureChildSlotName(StandardChildSlots) : string` — standard texture-slot reads.
- `RenderMaterial.AssignTo(IEnumerable<ObjRef>, AssignToSubFaceChoices, AssignToBlockChoices, bool) : bool` / `AssignTo(ObjRef) : bool` — object and sub-face material assignment.
- `RenderMaterial.SlotFromTextureType(TextureType) : StandardChildSlots` / `TextureTypeFromSlot(StandardChildSlots) : TextureType` — texture-type to standard-slot mapping.
- `RenderMaterial.PlasterMaterialGuid` / `PlasticMaterialGuid` / `PaintMaterialGuid` / `GlassMaterialGuid` / `GemMaterialGuid` / `MetalMaterialGuid` / `PictureMaterialGuid : Guid` — built-in material factory ids beside `ContentUuids`.
- `RenderMaterial.SmellsLikePlaster` / `SmellsLikePaint` / `SmellsLikeMetal` / `SmellsLikePlastic` / `SmellsLikeGem` / `SmellsLikeGlass : bool` and the six `SmellsLikeTextured*` twins — material-class heuristic predicates.
- `RenderMaterial.DefaultPreviewGeometryType : PreviewGeometryType` / `DefaultPreviewBackgroundType : PreviewBackgroundType` / `DefaultPreviewSize : double` — default preview-pane configuration (get/set).
- `RenderMaterial.ImportMaterialAndAssignToLayers(RhinoDoc, string, IEnumerable<int>) : bool` / `HandleTexturedValue<T>(string, TexturedValue<T>) : bool` — RMTL import-and-assign and typed textured-slot read.

[TEXTURE_CONTENT]:
- `RenderTexture.GetProjectionMode() : TextureProjectionMode` / `SetProjectionMode(TextureProjectionMode, ChangeContexts)` / `GetWrapType() : TextureWrapType` / `SetWrapType(TextureWrapType, ChangeContexts)` — projection and wrap configuration.
- `RenderTexture.GetRepeat() : Vector3d` / `SetRepeat(Vector3d, ChangeContexts)` / `GetOffset() : Vector3d` / `SetOffset(Vector3d, ChangeContexts)` / `GetRotation() : Vector3d` / `SetRotation(Vector3d, ChangeContexts)` — the UVW repeat/offset/rotation triple.
- `RenderTexture.GetMappingChannel() : int` / `SetMappingChannel(int, ChangeContexts)` / `LocalMappingTransform : Transform` / `GetEnvironmentMappingMode() : TextureEnvironmentMappingMode` / `SetEnvironmentMappingMode(TextureEnvironmentMappingMode, ChangeContexts)` — mapping-channel binding and environment mapping.
- `RenderTexture.PixelSize(out int, out int, out int) : void` / `PixelSize2 : (int, int, int)?` / `IsHdrCapable() : bool` / `IsLinear() : bool` / `IsNormalMap() : bool` / `IsImageBased() : bool` — texel dimensions and content-classification predicates.
- `RenderTexture.NewBitmapTexture(Bitmap, RhinoDoc) : RenderTexture` / `NewBitmapTexture(SimulatedTexture, RhinoDoc) : RenderTexture` / `SaveAsImage(string, int, int, int) : bool` — bitmap-backed texture construction and export.
- `RenderTexture.RenderHashWithoutLocalMapping : uint` / `RenderHashWithoutLocalMappingOrLinearWorkflow : uint` — pre-excluded hash reads beside `RenderHashExclude`.
- `RenderTexture.GetRepeatLocked() : bool` / `SetRepeatLocked(bool, ChangeContexts)` / `GetOffsetLocked() : bool` / `SetOffsetLocked(bool, ChangeContexts)` — UVW repeat/offset lock flags.
- `RenderTexture.GetPreviewIn3D() : bool` / `SetPreviewIn3D(bool, ChangeContexts)` / `GetPreviewLocalMapping() : bool` / `SetPreviewLocalMapping(bool, ChangeContexts)` / `GetDisplayInViewport() : bool` / `SetDisplayInViewport(bool, ChangeContexts)` — preview and viewport-display flags; each setter also carries a context-free overload.
- `RenderTexture.GetLocalMappingType() : eLocalMappingType` / `GetInternalEnvironmentMappingMode() : TextureEnvironmentMappingMode` / `GraphInfo(ref TextureGraphInfo)` / `SetGraphInfo(TextureGraphInfo)` — local-mapping classification and texture-graph info.
- `RenderTexture.GetEnvironmentMappingProjection(TextureEnvironmentMappingMode, Vector3d, out float, out float) : bool` / `GetWcsBoxMapping(Point3d, Vector3d) : Point3d` — static environment-projection and WCS-box helpers.

[ENVIRONMENT_CONTENT]:
- `RenderEnvironment.NewBasicEnvironment(SimulatedEnvironment, RhinoDoc) : RenderEnvironment` / `TextureChildSlotName : string` — basic environment from a simulation and its texture slot.
- `RenderEnvironment.SimulateEnvironment(ref SimulatedEnvironment, bool) : void` / `SimulateEnvironment(bool) : SimulatedEnvironment` — environment bake into a `SimulatedEnvironment`, by ref-fill or returning form.
- `SimulatedEnvironment.BackgroundColor : Color` / `BackgroundImage : SimulatedTexture` / `BackgroundProjection : SimulatedEnvironment.BackgroundProjections` — bake payload; `SimulatedEnvironment : IDisposable`, so every bake rides a lease; `ProjectionFromString(string)` / `StringFromProjection(BackgroundProjections)` round-trip the projection name.
- `SimulatedTexture(RhinoDoc)` / `SimulatedTexture(RhinoDoc, Texture)` — doc-aware constructors.
- `SimulatedTexture.Filename : string` / `OriginalFilename : string` / `LocalMappingTransform : Transform` / `Repeat : Vector2d` / `Offset : Vector2d` / `Rotation : double` / `Repeating : bool` / `MappingChannel : int` / `ProjectionMode : ProjectionModes` — file, UVW, and projection payload.
- `SimulatedTexture.HasTransparentColor : bool` / `TransparentColor : Color4f` / `TransparentColorSensitivity : double` / `Filtered : bool` / `BitmapSize : int` (static get/set) — transparency and raster policy.
- `SimulatedTexture.SetMappingChannelAndProjectionMode(ProjectionModes, int, EnvironmentMappingModes)` / `Texture() : Texture` / `UnitsToMeters(RhinoDoc, double) : double` / `MetersToUnits(RhinoDoc, double) : double` — channel/projection write, `DocObjects.Texture` projection, doc-aware unit bridges.

[FIELD_DICTIONARY]:
- `FieldDictionary.Add(string key, T value[, string prompt[, int sectionId]]) : <T>Field` — one polymorphic add per payload type, returning the matching typed field (`StringField`, `BoolField`, `IntField`, `FloatField`, `DoubleField`, `Color4fField`, `Vector2dField`, `Vector3dField`, `Point2dField`, `Point3dField`, `Point4dField`, `GuidField`, `TransformField`, `DateTimeField`, `ByteArrayField`, `NullField`).
- `FieldDictionary.AddTextured(string, T, string, bool, int) : <T>Field` / `AddFilename(string, string, string, int) : StringField` — textured and filename field variants.
- `FieldDictionary.Set(string, T) : void` / `TryGetValue(string, out T) : bool` / `TryGetValue<T>(string, out T) : bool` — payload-typed set and read.
- `FieldDictionary.ContainsField(string) : bool` / `GetField(string) : Field` / `RemoveField(string) : void` / `GetEnumerator() : IEnumerator<Field>` — field lookup, removal, and iteration.
- `Field.Name : string` / `Tag : object` / `ValueAsObject() : object` / `GetValue<T>() : T` — field identity and type-erased/typed value access.
- `Field.TextureAmountMin : double` / `TextureAmountMax : double` / `UseTextureOn : bool` / `UseTextureAmount : bool` / `IsHiddenInAutoUI : bool` — textured-field UI bounds and auto-UI visibility.

[REGISTRY]:
- `RenderContentType.GetAllAvailableTypes() : RenderContentType[]` / `NewRenderContent() : RenderContent` / `NewContentFromTypeId(Guid) : RenderContent` / `NewContentFromTypeId(Guid, RhinoDoc) : RenderContent` — factory enumeration and construction by type id; `RenderContentType(Guid)` constructs a descriptor, and the type is `IDisposable`, so a census projects and disposes.
- `RenderContentType.Id : Guid` / `InternalName : string` / `RenderEngineId : Guid` / `PlugInId : Guid` — factory descriptor identity.
- `ContentUuids` — static well-known factory type ids. Materials: `BasicMaterialType`, `BlendMaterialType`, `V8BlendMaterialType`, `CompositeMaterialType`, `PlasterMaterialType`, `MetalMaterialType`, `PaintMaterialType`, `PlasticMaterialType`, `GemMaterialType`, `GlassMaterialType`, `PictureMaterialType`, `PhysicallyBasedMaterialType`, `DoubleSidedMaterialType`, `EmissionMaterialType`, `DisplayAttributeMaterialType`, `RealtimeDisplayMaterialType`, `DefaultMaterialInstance`. Environments: `BasicEnvironmentType`, `DefaultEnvironmentInstance`. Textures: `Texture2DCheckerTextureType`, `Texture3DCheckerTextureType`, `AdvancedDotTextureType`, `BitmapTextureType`, `SimpleBitmapTextureType`, `BlendTextureType`, `CubeMapTextureType`, `ExposureTextureType`, `FBmTextureType`, `GradientTextureType`, `GraniteTextureType`, `GridTextureType`, `HDRTextureType`, `EXRTextureType`, `MarbleTextureType`, `MaskTextureType`, `NoiseTextureType`, `PerlinMarbleTextureType`, `PerturbingTextureType`, `ProjectionChangerTextureType`, `ResampleTextureType`, `SingleColorTextureType`, `StuccoTextureType`, `TextureAdjustmentTextureType`, `TileTextureType`, `TurbulenceTextureType`, `WavesTextureType`, `WoodTextureType`, `AddTextureType`, `MultiplyTextureType`, `PhysicalSkyTextureType`. Bump textures: `HatchBumpTexture`, `CrossHatchBumpTexture`, `LeatherBumpTexture`, `WoodBumpTexture`, `SpeckleBumpTexture`, `GritBumpTexture`, `DotBumpTexture`. Collection instances: `BasicMaterialCCI`, `BlendMaterialCCI`, `CompositeMaterialCCI`, `BasicEnvironmentCCI`.
- `RenderContentSerializer(string, RenderContentKind, bool, bool)` — protected abstract constructor; `RegisterSerializer(Guid) : bool` registers, and the host also discovers serializers through `RenderPlugIn.RenderContentSerializers()`.
- `RenderContentSerializer.Read(string) : RenderContent` / `Write(string, RenderContent, CreatePreviewEventArgs) : bool` — custom content-format read/write; `CanRead`/`CanWrite`/`FileExtension`/`ContentKind` describe the format and `EnglishDescription` (abstract) / `LocalDescription` name it.
- `RenderContentSerializer.CanLoadMultiple() : bool` / `LoadMultiple(RhinoDoc, IEnumerable<string>, RenderContentKind, LoadMultipleFlags) : bool` / `ReportContentAndFile(RenderContent, string, int)` / `ReportDeferredContentAndFile(RenderContent, string, int)` — the multi-file load surface over the nested `enum LoadMultipleFlags { Normal, Preload }`.
- `CustomRenderContentAttribute(string, bool, string, bool, bool, bool, bool, bool, bool)` — plug-in content registration attribute; a six-argument constructor precedes it, and named properties `RenderEngineId : Guid` (get), `ImageBased`, `IsLinear`, `IsHdrCapable`, `IsNormalMap`, `Category` (default `"General"`), `IsElevated`, `IsBuiltIn`, `IsPrivate` are settable at the attribute site.
- `ICurrentEnvironment` — `ForBackground` / `ForReflectionAndRefraction` / `ForLighting : RenderEnvironment` (get/set); the current-environment usage triple `RhinoDoc.CurrentEnvironment` returns, and that accessor is `api-rhinocommon-document.md`'s.

[COLLECTION_KINDLIST]:
- `RenderContentCollection.Append(RenderContent) : void` / `Set(RenderContentCollection) : void` / `Add(RenderContentCollection)` / `Remove(RenderContentCollection)` / `Clear() : void` — content-set composition; the collection is `IDisposable`.
- `RenderContentCollection.Count() : int` / `ContentAt(int) : RenderContent` / `Find_Sel(Guid) : RenderContent` / `Iterator() : ContentCollectionIterator` / `GetFilterContentByUsage() : FilterContentByUsage` — set access and usage filter; `ContentCollectionIterator` (`IDisposable`) cursors through `First()`/`Next() : RenderContent` and removes through `DeleteThis()`.
- `RenderContentCollection.GetForcedVaries() : bool` / `SetForcedVaries(bool)` / `GetSearchPattern() : string` / `SetSearchPattern(string)` / `ContentNeedsPreviewThumbnail(RenderContent) : bool` — editor selection-set state the content-editor seam reads.
- `RenderContentKindList.Add(RenderContentKind) : void` / `Contains(RenderContentKind) : bool` / `SingleKind() : RenderContentKind` / `Count() : int` — additive kind set with single-kind collapse; the list is `IDisposable`.

[CONTENT_TABLES]:
- `RenderMaterialTable.Count : int` / `this[int] : RenderMaterial` — count and range-checked indexer that throws `IndexOutOfRangeException` out of range; both ride the internal `IRhinoTable<T>`.
- `RenderMaterialTable.Add(RenderMaterial) : bool` / `Remove(RenderMaterial) : bool` / `Find(Guid) : RenderMaterial` — content table mutation and id lookup.
- `RenderMaterialTable.BeginChange(RenderContent.ChangeContexts) : void` / `EndChange() : void` / `GetEnumerator() : IEnumerator<RenderMaterial>` — table change scope and iteration; the three `public sealed` tables carry byte-identical member sets over `IRenderContentTable<T>`, so `RenderEnvironmentTable` and `RenderTextureTable` repeat the shape over `RenderEnvironment`/`RenderTexture` with no per-table extras.

[CONTENT_EVENTS]:
- `RenderContent.ContentAdded` / `ContentRenamed` / `ContentDeleting` / `ContentDeleted` / `ContentReplacing` / `ContentReplaced` / `ContentUpdatePreview` / `CurrentEnvironmentChanged` — `EventHandler<RenderContentEventArgs>` lifecycle broadcasts; `CurrentEnvironmentChanged` carries no dedicated args type, its `EnvironmentUsageEx` field is the signal.
- `RenderContent.ContentChanged` — `EventHandler<RenderContentChangedEventArgs>`; the args derive `RenderContentEventArgs` and add `ChangeContext : RenderContent.ChangeContexts` and `OldContent : RenderContent`.
- `RenderContent.ContentFieldChanged` — `EventHandler<RenderContentFieldChangedEventArgs>`; the args derive `RenderContentChangedEventArgs` (also carrying `ChangeContext`/`OldContent`) and add `FieldName : string`.
- `RenderContent.PreviewRendered` — `EventHandler<PreviewRenderedEventArgs>` carrying `Bitmap`, `PreviewJobSignature`, and `Quality`.
- `RenderContentEventArgs.Content : RenderContent` / `Document : RhinoDoc` / `Reason : RenderContentChangeReason` / `EnvironmentUsageEx : RenderSettings.EnvironmentUsage` — the shared event carrier.

[TEXTURE_MAPPING]:
- `TextureMapping : ModelComponent` (`ComponentType => ModelComponentType.TextureMapping`); every constructor is internal, so a mapping is minted only through the statics or read off a `RhinoObject`.
- `TextureMapping.CreateSurfaceParameterMapping() : TextureMapping` / `CreatePlaneMapping(Plane, Interval, Interval, Interval) : TextureMapping` / `CreatePlaneMapping(Plane, Interval, Interval, Interval, bool) : TextureMapping` / `CreateOcsMapping(Plane) : TextureMapping` / `CreateCylinderMapping(Cylinder, bool) : TextureMapping` / `CreateSphereMapping(Sphere) : TextureMapping` / `CreateBoxMapping(Plane, Interval, Interval, Interval, bool) : TextureMapping` / `CreateCustomMeshMapping(Mesh) : TextureMapping` — the factory roster; an OCS mapping binds only on the `ObjectAttributes.OCSMappingChannelId` channel.
- `TextureMapping.MappingType : TextureMappingType` / `TextureSpace : TextureSpace` / `Projection : Projection` / `Capped : bool` (get/set) / `Id : Guid` — mapping classification and identity.
- `TextureMapping.UvwTransform : Transform` / `PrimitiveTransform : Transform` / `NormalTransform : Transform` (get/set) — the mapping transform triple.
- `TextureMapping.Evaluate(Point3d, Vector3d, out Point3d) : int` / `Evaluate(Point3d, Vector3d, out Point3d, Transform, Transform) : int` — per-point evaluation returning the primitive side index.
- `TextureMapping.Decompose(Transform, out Vector3d, out Vector3d, out Vector3d, out Vector3d, out Vector3d, out Vector3d) : void` — UI-parity TRS/UVW decomposition of a local mapping transform.
- `TextureMapping.TryGetMappingBox(out Plane, out Interval, out Interval, out Interval) : bool` / `TryGetMappingPlane(out Plane, out Interval, out Interval, out Interval) : bool` — each with an `out bool capped` five-argument twin; `TryGetMappingSphere(out Sphere) : bool` / `TryGetMappingCylinder(out Cylinder) : bool` (+ `out bool capped` twin) / `TryGetMappingMesh(out Mesh) : bool` — primitive recovery from a mapping.
- `Rhino.DocObjects.RhinoObject.GetTextureMapping(int) : TextureMapping` / `GetTextureMapping(int, out Transform) : TextureMapping` / `SetTextureMapping(int, TextureMapping) : int` / `SetTextureMapping(int, TextureMapping, Transform) : int` / `HasTextureMapping() : bool` / `GetTextureChannels() : int[]` — the per-object channel binding; the channel is a raw `int`, `ObjectAttributes.OCSMappingChannelId` is the OCS constant, and no public document mapping table exists — the object carrier is `api-rhinocommon-objects.md`'s.
- `MappingTag.Id : Guid` / `MappingType : TextureMappingType` / `MappingCRC : uint` / `MeshTransform : Transform` (get/set) / `CompareTo(MappingTag) : int` — per-channel mapping tag.
- `CachedTextureCoordinates : CommonObject, IList<Point3d>` — read-only (`IsReadOnly` true, mutators throw): `Dim : int` (2 = UV, 3 = UVW) / `MappingId : Guid` / `Count : int` / `this[int] : Point3d` / `TryGetAt(int, out double, out double, out double) : bool` / `IndexOf(Point3d) : int` / `Contains(Point3d) : bool` / `CopyTo(Point3d[], int)`; retrieval rides the mesh seam the geometry catalog owns.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RenderContent` is the single abstract root; `RenderMaterial`, `RenderTexture`, and `RenderEnvironment` never carry a parallel identity, tree, or field surface — every specialization reads its base graph, and a further content kind is a `RenderContentType` factory row, never a new base.
- Content forms a child-slot tree: `Parent`/`FirstChild`/`NextSibling` walk it, `FindChild`/`SetChild`/`DeleteChild` mutate it by slot name, and every mutation names a `ChangeContexts` reason inside a `BeginChange`/`EndChange` scope — a raw field write outside that scope is the deleted form.
- Fields are the one parameter surface: `FieldDictionary` discriminates payload type at the `Add`/`Set`/`TryGetValue` overload, returns the matching typed `Field`, and `Field.FieldFromPointer` recovers the concrete field from the native `Variant.Types` — no per-type field-accessor family exists beside it.
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
- Package: `RhinoCommon`
- Owns: the `RenderContent` object model (identity, child-slot tree, typed fields, render-hash, XML/file IO, UI/preview), the `RenderMaterial`/`RenderTexture`/`RenderEnvironment` specializations and material/PBR bridge, content registration and factory minting, the three content tables, the content events, and `TextureMapping` geometry.
- Accept: content authoring and configuration through `Create`/`From*`/field mutation inside `BeginChange`/`EndChange`; polymorphic field access over `FieldDictionary`; content trapped through `Try.lift(...).Run()` with kind/style/context enums mapped to bounded owners; table and collection outcomes projected onto `Fin`/`Option`/`Seq` rails.
- Reject: a parallel identity/tree/field surface on a subclass, a per-payload field-accessor family beside `FieldDictionary`, a field write outside a change scope, a re-derived render hash, a live `RenderContent`/`TextureMapping` escaping into a domain signature, and the live-evaluation surface `api-rhinocommon-render.md` owns re-catalogued here.
