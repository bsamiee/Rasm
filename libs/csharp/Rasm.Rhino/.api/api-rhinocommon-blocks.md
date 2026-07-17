# [RASM_RHINO_API_RHINOCOMMON_BLOCKS]

This catalog owns the instance-definition boundary: `InstanceDefinitionTable` identity, geometry authoring, linked-source transitions, lifecycle, and export; `InstanceDefinition` reference topology, dependency queries, and preview generation; `InstanceObject` explosion and component resolution; `ObjectTable` instance placement, replacement, and baking; and `TextFields` block-attribute composition and extraction. Definition geometry crosses through the geometry catalog, `File3dm` carries the standalone-archive definition graph, and document-bound objects resolve inside the owning session before detached values cross the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon block surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino`, `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.Runtime`, `Rhino.FileIO`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: block-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: definition and instance
- rail: block-boundary

| [INDEX] | [SYMBOL]                              | [KIND]          | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------ | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `InstanceDefinitionTable`             | document table  | block-definition identity, geometry, linked-source, lifecycle, and export    |
|  [02]   | `InstanceDefinition`                  | definition      | member objects, reference topology, usage queries, and preview bitmaps       |
|  [03]   | `InstanceObject`                      | placed instance | explode, sub-object resolution, and definition-use checks                    |
|  [04]   | `ObjectTable`                         | document table  | instance placement, replacement, exploded-piece insertion, history transform |
|  [05]   | `InstanceDefinitionUpdateType`        | enum            | static, obsolete embedded, linked-and-embedded, or linked                    |
|  [06]   | `InstanceDefinitionArchiveFileStatus` | enum            | linked-source archive availability state                                     |
|  [07]   | `LinkedInstanceDefinitionUpdateStyle` | enum            | prompt, always-update, or never-update policy                                |

[PUBLIC_TYPE_SCOPE]: attribute fields and archive reads
- rail: block-boundary

| [INDEX] | [SYMBOL]                            | [KIND]         | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `TextFields`                        | static surface | block-attribute composition and extraction          |
|  [02]   | `TextFields.InstanceAttributeField` | nested carrier | immutable key, prompt, and default-value descriptor |
|  [03]   | `File3dmInstanceDefinitionTable`    | archive table  | standalone-archive instance-definition roster       |
|  [04]   | `InstanceDefinitionGeometry`        | geometry       | archive-side definition geometry carrier            |

[ENUM_ROSTERS]:
- `public enum Rhino.DocObjects.InstanceDefinitionUpdateType` — `Static = 0`, obsolete `Embedded = 1`, `LinkedAndEmbedded = 2`, `Linked = 3`.
- `public enum Rhino.DocObjects.InstanceDefinitionArchiveFileStatus` — `NotALinkedInstanceDefinition = -3`, `LinkedFileNotReadable = -2`, `LinkedFileNotFound = -1`, `LinkedFileIsUpToDate = 0`, `LinkedFileIsNewer = 1`, `LinkedFileIsOlder = 2`, `LinkedFileIsDifferent = 3`.
- `public enum Rhino.DocObjects.InstanceDefinitionLayerStyle` — `None = 0`, `Active = 1`, `Reference = 2`; the setter is effective only for linked definitions.
- `public enum Rhino.LinkedInstanceDefinitionUpdateStyle` — `Prompt = 1`, `AlwaysUpdate = 2`, `NeverUpdate = 3`.

## [03]-[ENTRYPOINTS]

[DEFINITION_IDENTITY]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Find(string instanceDefinitionName) : InstanceDefinition` — resolves a definition by name; the `(string, bool ignoreDeleted)` overload is obsolete.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable[int index] : InstanceDefinition` — the indexer read; a purged slot returns null after the range check.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Find(Guid instanceId, bool ignoreDeletedInstanceDefinitions) : InstanceDefinition` — resolves a definition by id.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.GetList(bool ignoreDeleted) : InstanceDefinition[]` — returns a non-null, possibly empty roster whose zero-pointer slots contain null.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.GetUnusedInstanceDefinitionName(string root) : string` — mints an unused definition name; the `(string, uint defaultSuffix)` overload is obsolete.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.GetUnusedInstanceDefinitionName() : string` — mints an unused definition name from the host default root.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Count : int` — slot count, including deleted definitions.

[DEFINITION_AUTHORING]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Add(string name, string description, string url, string urlTag, Point3d basePoint, IEnumerable<GeometryBase> geometry, IEnumerable<ObjectAttributes> attributes) : int` — adds a definition with hyperlink metadata.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Add(string name, string description, Point3d basePoint, IEnumerable<GeometryBase> geometry, IEnumerable<ObjectAttributes> attributes) : int` — adds a definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Modify(int idefIndex, string newName, string newDescription, string newUrl, string newUrlTag, bool quiet) : bool` — modifies definition metadata.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.ModifyGeometry(int idefIndex, IEnumerable<GeometryBase> newGeometry, IEnumerable<ObjectAttributes> newAttributes) : bool` — replaces definition geometry; the public member accepts independent enumerables and does not prove equal cardinality.

[LINKED_SOURCE]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.ModifySourceArchive(int idefIndex, FileReference sourceArchive, InstanceDefinitionUpdateType updateType, bool quiet) : bool` — binds or rebinds a linked source archive.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.DestroySourceArchive(InstanceDefinition definition, bool quiet) : bool` — severs the linked source.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.RefreshLinkedBlock(InstanceDefinition definition) : bool` — reloads a linked definition from its source.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.UpdateLinkedInstanceDefinition(int idefIndex, string filename, bool updateNestedLinks, bool quiet) : bool` — updates a linked definition and optionally its nested links.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.MakeSourcePathRelative(InstanceDefinition idef, bool relative, bool quiet) : bool` — obsolete and inert; unconditionally returns `false`.
- `Rhino.DocObjects.InstanceDefinition.SourceArchive : string` — the linked source path, shadowed from `InstanceDefinitionGeometry`.
- `Rhino.FileIO.FileReference.CreateFromFullPath(string fullPath) : FileReference` / `CreateFromFullAndRelativePaths(string fullPath, string relativePath) : FileReference` — the linked-source reference mints; `FileReference : IDisposable`, so every mint rides a lease or using scope.
- `Rhino.LinkedInstanceDefinitionUpdateStyle` — `Prompt = 1` / `AlwaysUpdate` / `NeverUpdate`, the linked-update policy roster.

[DEFINITION_LIFECYCLE]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Delete(int idefIndex, bool deleteReferences, bool quiet) : bool` — deletes a definition and optionally its references.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Undelete(int idefIndex) : bool` — restores a deleted definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Purge(int idefIndex) : bool` — permanently removes a definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.PurgeUnused() : int` — removes all unreferenced definitions.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Compact(bool ignoreUndoReferences) : void` — reclaims deleted-definition slots.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Export(int idefIndex, string filename) : bool` — exports a definition to a standalone archive.

[REFERENCE_TOPOLOGY]:
- `Rhino.DocObjects.InstanceDefinition.Id : Guid` / `Index : int` / `Name : string` / `Description : string` / `IsDeleted : bool` — live component identity and deletion state.
- `Rhino.DocObjects.InstanceDefinition.ObjectCount : int` — member-object count.
- `Rhino.DocObjects.InstanceDefinition.UpdateType : InstanceDefinitionUpdateType` — static/linked/embedded mode.
- `Rhino.DocObjects.InstanceDefinition.ArchiveFileStatus : InstanceDefinitionArchiveFileStatus` — linked-source availability.
- `Rhino.DocObjects.InstanceDefinition.LayerStyle { get; set; } : InstanceDefinitionLayerStyle` — linked-definition layer policy; writes are no-ops unless the definition is linked.
- `Rhino.DocObjects.InstanceDefinition.IsTenuous : bool` — linked source is missing or unloadable.
- `Rhino.DocObjects.InstanceDefinition.SkipNestedLinkedDefinitions : bool` — nested linked definitions are excluded from load.
- `Rhino.DocObjects.InstanceDefinition.Object(int index) : RhinoObject` — a member object by index.
- `Rhino.DocObjects.InstanceDefinition.GetObjects() : RhinoObject[]` — the member-object roster.
- `Rhino.DocObjects.InstanceDefinition.GetReferences(int wheretoLook) : InstanceObject[]` — the placed references of this definition.
- `Rhino.DocObjects.InstanceDefinition.GetContainers() : InstanceDefinition[]` — the definitions nesting this one.
- `Rhino.DocObjects.InstanceDefinition.UsesDefinition(int otherIdefIndex) : int` — nesting depth into another definition.

[USAGE_QUERIES]:
- `Rhino.DocObjects.InstanceDefinition.UseCount() : int` — total reference count.
- `Rhino.DocObjects.InstanceDefinition.UseCount(out int topLevelReferenceCount, out int nestedReferenceCount) : int` — split top-level and nested reference counts.
- `Rhino.DocObjects.InstanceDefinition.InUse(int wheretoLook) : bool` — whether the definition is referenced.
- `Rhino.DocObjects.InstanceDefinition.UsesLayer(int layerIndex) : bool` — layer dependency probe.
- `Rhino.DocObjects.InstanceDefinition.UsesLinetype(int linetypeIndex) : bool` — linetype dependency probe.

[PREVIEW]:
- `Rhino.DocObjects.InstanceDefinition.CreatePreviewBitmap(DefinedViewportProjection definedViewportProjection, DisplayMode displayMode, Size bitmapSize, bool applyDpiScaling) : Bitmap` — renders a definition preview.
- `Rhino.DocObjects.InstanceDefinition.CreatePreviewBitmap(Guid definitionObjectId, DefinedViewportProjection viewportProjection, DisplayMode displayMode, Size bitmapSize, bool applyDpiScaling) : Bitmap` — renders a preview with one definition member selected.
- `Rhino.DocObjects.InstanceDefinition.CreatePreviewBitmap(Guid displayModeId, DefinedViewportProjection viewportProjection, IsometricCamera isometricCamera, bool drawDecorations, Size bitmapSize, bool applyDpiScaling) : Bitmap` — renders a definition preview with an isometric camera and display-mode id.

[INSTANCE_RESOLUTION]:
- `Rhino.DocObjects.InstanceObject.InstanceDefinition : InstanceDefinition` — the live definition this placement references; the member-object count reads through `InstanceDefinition.ObjectCount`, and the archive-only `GetObjectIds()` is not exposed on the live definition.
- `Rhino.DocObjects.InstanceObject.UsesDefinition(int definitionIndex, out int nestingLevel) : bool` — whether this instance uses a definition, with nesting level.
- `Rhino.DocObjects.InstanceObject.Explode(bool explodeNestedInstances, out RhinoObject[] pieces, out ObjectAttributes[] pieceAttributes, out Transform[] pieceTransforms) : void` — explodes the instance through the viewport overload.
- `Rhino.DocObjects.InstanceObject.Explode(bool skipHiddenPieces, Guid viewportId, bool explodeNestedInstances, out RhinoObject[] pieces, out ObjectAttributes[] pieceAttributes, out Transform[] pieceTransforms) : void` — allocates all three non-null arrays from one native piece count, so their lengths are equal, including zero.
- `Rhino.DocObjects.InstanceObject.SubObjectFromComponentIndex(ComponentIndex ci) : RhinoObject` — resolves a sub-object by component index.

[INSTANCE_OPERATIONS]:
- `Rhino.DocObjects.Tables.ObjectTable.AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform) : Guid` — places an instance with a transform.
- `Rhino.DocObjects.Tables.ObjectTable.AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform, ObjectAttributes attributes) : Guid` — places an attributed instance.
- `Rhino.DocObjects.Tables.ObjectTable.AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid` — places an instance with a transform and history.
- `Rhino.DocObjects.Tables.ObjectTable.ReplaceInstanceObject(Guid objectId, int instanceDefinitionIndex) : bool` — repoints an instance to another definition.
- `Rhino.DocObjects.Tables.ObjectTable.AddExplodedInstancePieces(InstanceObject instance, bool explodeNestedInstances = true, bool deleteInstance = false) : Guid[]` — bakes exploded pieces into the document; the implementation returns null when no non-empty ids are produced despite the non-nullable public signature.
- `Rhino.DocObjects.Tables.ObjectTable.TransformWithHistory(Guid objectId, Transform xform) : Guid` — transforms an object under a history record.

[ATTRIBUTE_FIELDS]:
- `public class Rhino.Runtime.TextFields.InstanceAttributeField` — the public nested descriptor with a public `(string key, string prompt, string defaultValue)` constructor.
- `Rhino.Runtime.TextFields.InstanceAttributeField.Key : string` / `Prompt : string` / `DefaultValue : string` — public get-only descriptor values.
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(string str) : TextFields.InstanceAttributeField[]` — returns a non-null, possibly empty descriptor array from a text-field string.
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(TextObject text) : TextFields.InstanceAttributeField[]` — returns a non-null, possibly empty descriptor array from a text object; null input yields empty.
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(InstanceDefinition idef) : TextFields.InstanceAttributeField[]` — returns a non-null, possibly empty descriptor array from a definition; null input yields empty.
- `Rhino.Runtime.TextFields.BlockAttributeText(string key, string prompt, string defaultValue) : string` — composes a block-attribute text-field token.

[ARCHIVE_READS]:
- `Rhino.FileIO.File3dm.AllInstanceDefinitions : File3dmInstanceDefinitionTable` — the standalone-archive definition table.
- `Rhino.FileIO.File3dm.InstanceDefinitions : IList<InstanceDefinitionGeometry>` — the archive-side definition geometry roster.
- `Rhino.DocObjects.InstanceDefinitionGeometry.Id : Guid` / `Name : string` / `SourceArchive : string` — archive definition identity and stored linked-source path.
- `Rhino.DocObjects.InstanceDefinitionGeometry.GetObjectIds() : Guid[]` — archive member-object ids; linked definitions return an empty roster because their members are not persisted in the containing archive.
- `Rhino.Geometry.InstanceReferenceGeometry.ParentIdefId : Guid` — referenced definition identity carried by an archive or live instance-reference geometry.

## [04]-[IMPLEMENTATION_LAW]

[BLOCK_TOPOLOGY]:
- `InstanceDefinitionTable` owns the definition graph (identity, geometry, linked source, lifecycle, export); `InstanceDefinition` is the shared geometry authored once and referenced by many placements; `InstanceObject` is one placement carrying a transform; `ObjectTable` places, replaces, explodes, and bakes instances. A block is N placements over one definition, never N geometry copies.
- linked-block state is a triple — `UpdateType`, `ArchiveFileStatus`, and the source `FileReference` — mutated only through the table's linked-source members; refresh reloads from the source, and relative-path intent enters through `FileReference.CreateFromFullAndRelativePaths` because `MakeSourcePathRelative` is obsolete and inert.
- usage is queried, never assumed: `UseCount`, `InUse`, `GetReferences`, `GetContainers`, `UsesDefinition`, `UsesLayer`, and `UsesLinetype` resolve the dependency topology before any delete or purge.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): table `bool` and index outcomes project to `Fin<Unit>` and `Fin<int>`; nullable definition reads lift to `Option<InstanceDefinition>`; roster and reference arrays land as `Seq<A>`/`Arr<A>`; explode outputs prove equal cardinality before indexing and fold into detached placed-piece records with failure-branch release.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): closed mutation and read vocabularies use generated unions and smart enums, which own total dispatch and closed policy rows.
- `Rasm` kernel: placement transforms, base points, and unit scaling for exploded pieces and previews compose the kernel numeric owners; host `InstanceAttributeField` values project into the detached key/prompt/default-value record before leaving the document grant.

[LOCAL_ADMISSION]:
- a definition enters through `Add` or `ModifyGeometry` after each geometry source is paired with exactly one `ObjectAttributes`; the independent host enumerables derive from that paired row sequence and therefore retain equal cardinality. A placement enters through `AddInstanceObject`, and an explode returns detached piece records keyed to the instance id.
- live `InstanceDefinition`, `InstanceObject`, and `TextObject` values remain inside the document grant; downstream code receives definition references, detached field values, projected receipts, or explicitly owned geometry and bitmap leases.

[RAIL_LAW]:
- Surface: `Rhino` + `Rhino.DocObjects` + `Rhino.DocObjects.Tables` + `Rhino.Runtime.TextFields` + `Rhino.FileIO` block reads
- Owns: the block-definition graph, linked-source archive state, instance placement and explosion, definition previews, and block-attribute composition and extraction.
- Accept: definition authoring and lifecycle, linked-source transitions, usage/dependency queries, instance placement/replacement/explosion, and field composition/extraction projected onto `Fin`/`Option` rails.
- Reject: copied-geometry blocks, assumed dependency state, exception-style table outcomes, and live document-bound block objects crossing the session boundary.
