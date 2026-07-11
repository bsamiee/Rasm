# [RASM_RHINO_API_RHINOCOMMON_BLOCKS]

This catalog owns the instance-definition (block) boundary: `InstanceDefinitionTable` identity, geometry authoring, linked-source archive transitions, deletion/undelete/purge/compact, and export; `InstanceDefinition` reference topology, usage and dependency queries, and preview-bitmap generation; `InstanceObject` explode and sub-object component resolution; and the `ObjectTable` instance-placement operations with `TextFields` block-attribute extraction. Definition geometry crosses at the boundary the geometry catalog owns, `File3dm` block-side reads carry the standalone-archive definition graph, and outcomes project onto the `LanguageExt` rails with the closed archive/update vocabularies wrapped as `Thinktecture` generated owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon block surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.Runtime`, `Rhino.FileIO`
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
|  [05]   | `InstanceDefinitionUpdateType`        | enum            | static, linked, or embedded update mode of a definition                      |
|  [06]   | `InstanceDefinitionArchiveFileStatus` | enum            | linked-source archive availability state                                     |

[PUBLIC_TYPE_SCOPE]: attribute fields and archive reads
- rail: block-boundary

| [INDEX] | [SYMBOL]                         | [KIND]         | [CAPABILITY]                                                         |
| :-----: | :------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `TextFields`                     | static surface | block attribute-field extraction from text, objects, and definitions |
|  [02]   | `InstanceAttributeField`         | carrier        | one extracted block attribute-field descriptor                       |
|  [03]   | `File3dmInstanceDefinitionTable` | archive table  | standalone-archive instance-definition roster                        |
|  [04]   | `InstanceDefinitionGeometry`     | geometry       | the archive-side definition geometry carrier                         |

## [03]-[ENTRYPOINTS]

[DEFINITION_IDENTITY]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Find(string instanceDefinitionName, bool ignoreDeletedInstanceDefinitions) : InstanceDefinition` — resolves a definition by name.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Find(Guid instanceId, bool ignoreDeletedInstanceDefinitions) : InstanceDefinition` — resolves a definition by id.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.GetList(bool ignoreDeleted) : InstanceDefinition[]` — the definition roster.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.GetUnusedInstanceDefinitionName(string root, uint defaultSuffix) : string` — mints an unused definition name.

[DEFINITION_AUTHORING]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Add(string name, string description, string url, string urlTag, Point3d basePoint, IEnumerable<GeometryBase> geometry, IEnumerable<ObjectAttributes> attributes) : int` — adds a definition with hyperlink metadata.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Add(string name, string description, Point3d basePoint, IEnumerable<GeometryBase> geometry, IEnumerable<ObjectAttributes> attributes) : int` — adds a definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Modify(int idefIndex, string newName, string newDescription, string newUrl, string newUrlTag, bool quiet) : bool` — modifies definition metadata.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.ModifyGeometry(int idefIndex, IEnumerable<GeometryBase> newGeometry, IEnumerable<ObjectAttributes> newAttributes) : bool` — replaces definition geometry.

[LINKED_SOURCE]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.ModifySourceArchive(int idefIndex, FileReference sourceArchive, InstanceDefinitionUpdateType updateType, bool quiet) : bool` — binds or rebinds a linked source archive.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.DestroySourceArchive(InstanceDefinition definition, bool quiet) : bool` — severs the linked source.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.RefreshLinkedBlock(InstanceDefinition definition) : bool` — reloads a linked definition from its source.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.UpdateLinkedInstanceDefinition(int idefIndex, string filename, bool updateNestedLinks, bool quiet) : bool` — updates a linked definition and optionally its nested links.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.MakeSourcePathRelative(InstanceDefinition idef, bool relative, bool quiet) : bool` — toggles the source-path relativity.

[DEFINITION_LIFECYCLE]:
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Delete(int idefIndex, bool deleteReferences, bool quiet) : bool` — deletes a definition and optionally its references.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Undelete(int idefIndex) : bool` — restores a deleted definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Purge(int idefIndex) : bool` — permanently removes a definition.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.PurgeUnused() : int` — removes all unreferenced definitions.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Compact(bool ignoreUndoReferences) : void` — reclaims deleted-definition slots.
- `Rhino.DocObjects.Tables.InstanceDefinitionTable.Export(int idefIndex, string filename) : bool` — exports a definition to a standalone archive.

[REFERENCE_TOPOLOGY]:
- `Rhino.DocObjects.InstanceDefinition.ObjectCount : int` — member-object count.
- `Rhino.DocObjects.InstanceDefinition.UpdateType : InstanceDefinitionUpdateType` — static/linked/embedded mode.
- `Rhino.DocObjects.InstanceDefinition.ArchiveFileStatus : InstanceDefinitionArchiveFileStatus` — linked-source availability.
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
- `Rhino.DocObjects.InstanceDefinition.CreatePreviewBitmap(Guid displayModeId, DefinedViewportProjection viewportProjection, IsometricCamera isometricCamera, bool drawDecorations, Size bitmapSize, bool applyDpiScaling) : Bitmap` — renders a definition preview with an isometric camera and display-mode id.

[INSTANCE_RESOLUTION]:
- `Rhino.DocObjects.InstanceObject.UsesDefinition(int definitionIndex, out int nestingLevel) : bool` — whether this instance uses a definition, with nesting level.
- `Rhino.DocObjects.InstanceObject.Explode(bool explodeNestedInstances, out RhinoObject[] pieces, out ObjectAttributes[] pieceAttributes, out Transform[] pieceTransforms) : void` — explodes the instance into placed pieces.
- `Rhino.DocObjects.InstanceObject.Explode(bool skipHiddenPieces, Guid viewportId, bool explodeNestedInstances, out RhinoObject[] pieces, out ObjectAttributes[] pieceAttributes, out Transform[] pieceTransforms) : void` — explodes with viewport-scoped hidden-piece skipping.
- `Rhino.DocObjects.InstanceObject.SubObjectFromComponentIndex(ComponentIndex ci) : RhinoObject` — resolves a sub-object by component index.

[INSTANCE_OPERATIONS]:
- `Rhino.DocObjects.Tables.ObjectTable.AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform, ObjectAttributes attributes, HistoryRecord history, bool reference) : Guid` — places an instance with a transform and history.
- `Rhino.DocObjects.Tables.ObjectTable.ReplaceInstanceObject(Guid objectId, int instanceDefinitionIndex) : bool` — repoints an instance to another definition.
- `Rhino.DocObjects.Tables.ObjectTable.AddExplodedInstancePieces(InstanceObject instance, bool explodeNestedInstances, bool deleteInstance) : Guid[]` — bakes exploded pieces into the document.
- `Rhino.DocObjects.Tables.ObjectTable.TransformWithHistory(Guid objectId, Transform xform) : Guid` — transforms an object under a history record.

[ATTRIBUTE_FIELDS]:
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(string str) : InstanceAttributeField[]` — extracts attribute fields from a text-field string.
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(TextObject text) : InstanceAttributeField[]` — extracts attribute fields from a text object.
- `Rhino.Runtime.TextFields.GetInstanceAttributeFields(InstanceDefinition idef) : InstanceAttributeField[]` — extracts attribute fields carried by a definition.
- `Rhino.Runtime.TextFields.BlockAttributeText(string key, string prompt, string defaultValue) : string` — composes a block-attribute text-field token.

[ARCHIVE_READS]:
- `Rhino.FileIO.File3dm.AllInstanceDefinitions : File3dmInstanceDefinitionTable` — the standalone-archive definition table.
- `Rhino.FileIO.File3dm.InstanceDefinitions : IList<InstanceDefinitionGeometry>` — the archive-side definition geometry roster.

## [04]-[IMPLEMENTATION_LAW]

[BLOCK_TOPOLOGY]:
- `InstanceDefinitionTable` owns the definition graph (identity, geometry, linked source, lifecycle, export); `InstanceDefinition` is the shared geometry authored once and referenced by many placements; `InstanceObject` is one placement carrying a transform; `ObjectTable` places, replaces, explodes, and bakes instances. A block is N placements over one definition, never N geometry copies.
- linked-block state is a triple — `UpdateType` (static/linked/embedded), `ArchiveFileStatus` (source availability), and the source `FileReference` — mutated only through the table's linked-source members; refresh reloads from the source, and relativity toggles the stored path.
- usage is queried, never assumed: `UseCount`, `InUse`, `GetReferences`, `GetContainers`, `UsesDefinition`, `UsesLayer`, and `UsesLinetype` resolve the dependency topology before any delete or purge.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a table `bool`/`int`-index authoring or lifecycle result projects to `Fin<int>`/`Fin<Unit>` keyed to the definition index; a `Find` returning null lifts to `Option<InstanceDefinition>`; roster, member-object, reference, and exploded-piece arrays land as `Seq<A>`/`Arr<A>`; an explode fans its parallel `pieces`/`pieceAttributes`/`pieceTransforms` outputs into one `Seq` of placed-piece records.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): `InstanceDefinitionUpdateType` and `InstanceDefinitionArchiveFileStatus` wrap as `[SmartEnum<TKey>]`; a definition `Guid` identity and an `InstanceAttributeField` descriptor wrap as `[ValueObject<T>]` / `[ComplexValueObject]`; the linked-source state models as a `[Union]` over static, linked-available, and linked-missing cases.
- `Rasm` kernel: placement transforms, base points, and unit scaling for exploded pieces and previews compose the kernel numeric owners; the boundary re-derives none of the geometry math.

[LOCAL_ADMISSION]:
- a definition enters through the table's `Add`/`ModifyGeometry` members over canonical geometry payloads and returns a projected index rail; a placement enters through `ObjectTable.AddInstanceObject` with a transform and history; an explode returns projected placed-piece records keyed to the instance id.
- host block types never leak past the boundary; downstream code holds the projected rail value, the definition index, and the canonical geometry the geometry catalog admits.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects` + `Rhino.DocObjects.Tables` + `Rhino.Runtime.TextFields` + `Rhino.FileIO` block reads
- Owns: the block-definition graph, linked-source archive state, instance placement and explode, definition previews, and block attribute-field extraction.
- Accept: definition authoring and lifecycle, linked-source transitions, usage/dependency queries, instance placement/replacement/explode, and attribute-field extraction projected onto `Fin`/`Option` rails.
- Reject: copied-geometry blocks (a definition is shared, placements reference it), assumed usage (the dependency queries resolve it), exception-style table outcomes, and leaking host block types past the boundary.
