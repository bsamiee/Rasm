# [RASM_RHINO_API_RHINOCOMMON_BLOCKS]

`InstanceDefinitionTable` owns the block-definition graph over shared `InstanceDefinition` geometry that many transform-carrying `InstanceObject` placements reference, so a block is N placements over one definition, never N geometry copies. `ObjectTable` places, replaces, explodes, and bakes instances; `TextFields` composes and extracts block-attribute fields; `File3dm` carries the standalone-archive definition graph. Every live `InstanceDefinition`, `InstanceObject`, and `TextObject` resolves inside its owning document session before a detached value crosses the boundary.

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
|  [05]   | `InstanceDefinitionUpdateType`        | enum            | static, linked-and-embedded, or linked source mode                           |
|  [06]   | `InstanceDefinitionArchiveFileStatus` | enum            | linked-source archive availability state                                     |
|  [07]   | `InstanceDefinitionLayerStyle`        | enum            | linked-definition layer policy                                               |
|  [08]   | `LinkedInstanceDefinitionUpdateStyle` | enum            | prompt, always-update, or never-update policy                                |

[ENUM_CASES]:
- `InstanceDefinitionUpdateType`: `Static` `LinkedAndEmbedded` `Linked`
- `InstanceDefinitionArchiveFileStatus`: `NotALinkedInstanceDefinition` `LinkedFileNotReadable` `LinkedFileNotFound` `LinkedFileIsUpToDate` `LinkedFileIsNewer` `LinkedFileIsOlder` `LinkedFileIsDifferent`
- `InstanceDefinitionLayerStyle`: `None` `Active` `Reference`
- `LinkedInstanceDefinitionUpdateStyle`: `Prompt` `AlwaysUpdate` `NeverUpdate`

[PUBLIC_TYPE_SCOPE]: attribute fields and archive reads
- rail: block-boundary

| [INDEX] | [SYMBOL]                            | [KIND]         | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `TextFields`                        | static surface | block-attribute composition and extraction          |
|  [02]   | `TextFields.InstanceAttributeField` | nested carrier | immutable key, prompt, and default-value descriptor |
|  [03]   | `File3dmInstanceDefinitionTable`    | archive table  | standalone-archive instance-definition roster       |
|  [04]   | `InstanceDefinitionGeometry`        | geometry       | archive-side definition geometry carrier            |

## [03]-[ENTRYPOINTS]

[DEFINITION_IDENTITY]:
- `InstanceDefinitionTable.Find(string) -> InstanceDefinition` — case-insensitive name resolve.
- `InstanceDefinitionTable[int] -> InstanceDefinition` — indexer; a purged slot returns null after the range check.
- `InstanceDefinitionTable.Find(Guid, bool) -> InstanceDefinition` — id resolve with deleted-inclusion switch.
- `InstanceDefinitionTable.GetList(bool) -> InstanceDefinition[]` — non-null roster whose zero-pointer slots hold null.
- `InstanceDefinitionTable.GetUnusedInstanceDefinitionName(string) -> string` / `() -> string` — mints an unused name from a root or the host default.
- `InstanceDefinitionTable.Count -> int` — slot count including deleted definitions.

[DEFINITION_AUTHORING]:
- `InstanceDefinitionTable.Add(string, string, string, string, Point3d, IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>) -> int` — hyperlink metadata.
- `InstanceDefinitionTable.Add(string, string, Point3d, IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>) -> int` — adds without hyperlink.
- `InstanceDefinitionTable.Modify(int, string, string, string, string, bool) -> bool` — writes definition metadata.
- `InstanceDefinitionTable.ModifyGeometry(int, IEnumerable<GeometryBase>, IEnumerable<ObjectAttributes>) -> bool` — replaces geometry; accepts independent enumerables and proves no equal cardinality, and fails by policy on a linked definition.

[LINKED_SOURCE]:
- `InstanceDefinitionTable.ModifySourceArchive(int, FileReference, InstanceDefinitionUpdateType, bool) -> bool` — repaths a linked source.
- `InstanceDefinitionTable.DestroySourceArchive(InstanceDefinition, bool) -> bool` — severs the link, converts source to `Static`, and retains geometry.
- `InstanceDefinitionTable.RefreshLinkedBlock(InstanceDefinition) -> bool` — reloads the same source, synchronous.
- `InstanceDefinitionTable.UpdateLinkedInstanceDefinition(int, string, bool, bool) -> bool` — loads a different importer-readable file, optionally nested links.
- `InstanceDefinition.SourceArchive -> string` — linked source path, shadowed from `InstanceDefinitionGeometry`.
- `FileReference.CreateFromFullPath(string) -> FileReference` / `CreateFromFullAndRelativePaths(string, string) -> FileReference` — mints the linked-source reference; `FileReference : IDisposable`, so each mint rides a lease.

[DEFINITION_LIFECYCLE]:
- `InstanceDefinitionTable.Delete(int, bool, bool) -> bool` — deletes a definition, optionally its references.
- `InstanceDefinitionTable.Undelete(int) -> bool` — restores a deleted definition.
- `InstanceDefinitionTable.Purge(int) -> bool` — permanently removes a definition.
- `InstanceDefinitionTable.PurgeUnused() -> int` — purges every unreferenced definition.
- `InstanceDefinitionTable.Compact(bool) -> void` — reclaims deleted-definition slots.
- `InstanceDefinitionTable.Export(int, string) -> bool` — writes a definition to a standalone archive, format inferred from extension.

[REFERENCE_TOPOLOGY]:
- `InstanceDefinition.Id -> Guid` / `Index -> int` / `Name -> string` / `Description -> string` / `IsDeleted -> bool` — live identity and deletion state; equality flows through `Id`.
- `InstanceDefinition.ObjectCount -> int` — member-object count.
- `InstanceDefinition.UpdateType -> InstanceDefinitionUpdateType` / `ArchiveFileStatus -> InstanceDefinitionArchiveFileStatus` — linked mode and source availability.
- `InstanceDefinition.LayerStyle { get; set; } -> InstanceDefinitionLayerStyle` — layer policy; a write no-ops unless the definition is linked.
- `InstanceDefinition.IsTenuous -> bool` — linked source missing or unloadable.
- `InstanceDefinition.SkipNestedLinkedDefinitions -> bool` — read-only; nested linked definitions excluded from load.
- `InstanceDefinition.Object(int) -> RhinoObject` / `GetObjects() -> RhinoObject[]` — a member object, or the roster.
- `InstanceDefinition.GetReferences(int) -> InstanceObject[]` — placed references; `0` top-level, `1` top and nested, `2` from other definitions; thread-affine.
- `InstanceDefinition.GetContainers() -> InstanceDefinition[]` — definitions nesting this one.
- `InstanceDefinition.UsesDefinition(int) -> int` — nesting depth into another definition.

[USAGE_QUERIES]:
- `InstanceDefinition.UseCount() -> int` / `UseCount(out int, out int) -> int` — total, or split top-level and nested counts.
- `InstanceDefinition.InUse(int) -> bool` — reference probe.
- `InstanceDefinition.UsesLayer(int) -> bool` / `UsesLinetype(int) -> bool` — layer and linetype dependency probes.

[PREVIEW]:
- `InstanceDefinition.CreatePreviewBitmap(DefinedViewportProjection, DisplayMode, Size, bool) -> Bitmap` — definition preview.
- `InstanceDefinition.CreatePreviewBitmap(Guid, DefinedViewportProjection, DisplayMode, Size, bool) -> Bitmap` — preview with one definition member selected.
- `InstanceDefinition.CreatePreviewBitmap(Guid, DefinedViewportProjection, IsometricCamera, bool, Size, bool) -> Bitmap` — preview with an isometric camera and display-mode id.
- every overload is UI-thread bound, returns null on failure, and hands the caller a bitmap it disposes.

[INSTANCE_RESOLUTION]:
- `InstanceObject.InstanceDefinition -> InstanceDefinition` — referenced live definition, null when the definition is in error; member count reads through `InstanceDefinition.ObjectCount`.
- `InstanceObject.UsesDefinition(int, out int) -> bool` — definition use with nesting level.
- `InstanceObject.Explode(bool, out RhinoObject[], out ObjectAttributes[], out Transform[]) -> void` — non-mutating explode; piece arrays returned, document unchanged.
- `InstanceObject.Explode(bool, Guid, bool, out RhinoObject[], out ObjectAttributes[], out Transform[]) -> void` — viewport overload; all three arrays share one native piece count, equal length including zero.
- `InstanceObject.SubObjectFromComponentIndex(ComponentIndex) -> RhinoObject` — a sub-object by component index.

[INSTANCE_OPERATIONS]:
- `ObjectTable.AddInstanceObject(int, Transform) -> Guid` / `(int, Transform, ObjectAttributes) -> Guid` / `(int, Transform, ObjectAttributes, HistoryRecord, bool) -> Guid` — places an instance, optionally attributed and history-wired.
- `ObjectTable.ReplaceInstanceObject(Guid, int) -> bool` — repoints an instance to another definition, the id preserved.
- `ObjectTable.AddExplodedInstancePieces(InstanceObject, bool, bool) -> Guid[]` — bakes exploded pieces; returns null on a no-op despite the non-nullable signature.
- `ObjectTable.TransformWithHistory(Guid, Transform) -> Guid` — transforms under a history record, always a copy with the original preserved.

[ATTRIBUTE_FIELDS]:
- every `GetInstanceAttributeFields` overload returns a non-null, possibly empty array, and null input yields empty.
- `TextFields.InstanceAttributeField(string, string, string)` — nested-descriptor ctor; `Key` / `Prompt` / `DefaultValue` are get-only.
- `TextFields.GetInstanceAttributeFields(string) -> InstanceAttributeField[]` / `(TextObject) -> InstanceAttributeField[]` / `(InstanceDefinition) -> InstanceAttributeField[]` — descriptors from a field string, text object, or definition.
- `TextFields.BlockAttributeText(string, string, string) -> string` — composes a block-attribute text-field token.

[ARCHIVE_READS]:
- `File3dm.AllInstanceDefinitions -> File3dmInstanceDefinitionTable` — standalone-archive definition table.
- `File3dm.InstanceDefinitions -> IList<InstanceDefinitionGeometry>` — archive-side geometry roster.
- `InstanceDefinitionGeometry.Id -> Guid` / `Name -> string` / `SourceArchive -> string` — archive identity and stored linked-source path.
- `InstanceDefinitionGeometry.GetObjectIds() -> Guid[]` — archive member ids; a linked definition returns empty because its members are not persisted in the containing archive.
- `InstanceReferenceGeometry.ParentIdefId -> Guid` — referenced definition identity on an archive or live instance-reference geometry.

## [04]-[IMPLEMENTATION_LAW]

[BLOCK_TOPOLOGY]:
- `InstanceDefinitionTable` owns the definition graph; `InstanceDefinition` is the geometry authored once and referenced by many placements; `InstanceObject` is one transform-carrying placement; `ObjectTable` places, replaces, explodes, and bakes.
- linked-block state is a triple — `UpdateType`, `ArchiveFileStatus`, and the source `FileReference` — mutated only through the table's linked-source members, and relative-path intent enters through `FileReference.CreateFromFullAndRelativePaths`.
- `UseCount`, `InUse`, `GetReferences`, `GetContainers`, `UsesDefinition`, `UsesLayer`, and `UsesLinetype` resolve the dependency topology before any delete or purge.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): table `bool` and index outcomes project to `Fin<Unit>` and `Fin<int>`; nullable definition reads lift to `Option<InstanceDefinition>`; roster and reference arrays land as `Seq<A>`/`Arr<A>`; explode outputs prove equal cardinality before indexing and fold into detached placed-piece records with failure-branch release.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed mutation and read vocabularies ride generated unions and smart enums that own total dispatch and closed policy rows.
- `Rasm` kernel: placement transforms, base points, and unit scaling for exploded pieces and previews compose the kernel numeric owners; a host `InstanceAttributeField` projects into a detached key/prompt/default-value record before leaving the document grant.

[LOCAL_ADMISSION]:
- a definition enters through `Add` or `ModifyGeometry` after each geometry source is paired with exactly one `ObjectAttributes`, so the independent host enumerables derive from that paired row sequence and retain equal cardinality; a placement enters through `AddInstanceObject`, and an explode returns detached piece records keyed to the instance id.
- live `InstanceDefinition`, `InstanceObject`, and `TextObject` values stay inside the document grant; downstream code receives definition references, detached field values, projected receipts, or explicitly owned geometry and bitmap leases.

[RAIL_LAW]:
- Package: `RhinoCommon` block surface
- Owns: the block-definition graph, linked-source archive state, instance placement and explosion, definition previews, and block-attribute composition and extraction.
- Accept: definition authoring and lifecycle, linked-source transitions, usage and dependency queries, instance placement, replacement, and explosion, and field composition and extraction projected onto `Fin`/`Option` rails.
- Reject: copied-geometry blocks, assumed dependency state, exception-style table outcomes, and live document-bound block objects crossing the session boundary.
