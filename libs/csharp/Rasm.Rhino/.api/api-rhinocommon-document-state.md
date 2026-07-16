# [RASM_RHINO_API_RHINOCOMMON_DOCUMENT_STATE]

This catalog owns the document-scoped saved-state presets: named construction planes with their grid vocabulary, named positions as per-object transform snapshots, named layer states restored under a property mask, the snapshot name roster, the worksession reference-model roster, and the `SnapShotsClient` plugin participation seam. `NamedViewTable` is owned by Viewport/operations.md through `NamedViewOp` and is never cataloged here; tables.md owns the `TableKind` classification and `TableOp.RestoreView`; the `ArchivableDictionary` and `PersistentSettings` custody spine lives in api-rhinocommon-persistence.md. `SnapshotTable` exposes only its name roster — capture and restore ride `RunScript` and the `SnapShotsClient` participation seam, never a table method.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon document-state surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.DocObjects`, `Rhino.DocObjects.Tables`, `Rhino.DocObjects.SnapShots`
- kernel: `Rasm` (host-agnostic vocabularies and numeric owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: document-state-boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: named-preset tables
- rail: document-state-boundary

| [INDEX] | [SYMBOL]                      | [KIND]       | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :----------- | :-------------------------------------------------------------- |
|  [01]   | `NamedConstructionPlaneTable` | sealed table | named cplane presets; add, find, delete, indexed read           |
|  [02]   | `NamedPositionTable`          | sealed table | per-object transform snapshots; save, restore, update, append   |
|  [03]   | `NamedLayerStateTable`        | sealed table | named layer states restored under a property mask               |
|  [04]   | `SnapshotTable`               | sealed table | snapshot name roster only; capture and restore ride `RunScript` |

[PUBLIC_TYPE_SCOPE]: preset value carriers and participation
- rail: document-state-boundary

| [INDEX] | [SYMBOL]                        | [KIND]         | [CAPABILITY]                                                    |
| :-----: | :------------------------------ | :------------- | :-------------------------------------------------------------- |
|  [01]   | `ConstructionPlane`             | class          | grid, plane, snap, and axis-color vocabulary of one cplane      |
|  [02]   | `ConstructionPlaneGridDefaults` | class          | seven-field default grid carrier for new cplanes                |
|  [03]   | `Worksession`                   | sealed class   | reference-model roster read surface; mutation is command-driven |
|  [04]   | `SnapShotsClient`               | abstract class | plugin capture, restore, and animation participation seam       |
|  [05]   | `RestoreLayerProperties`        | flags enum     | property mask consumed by `NamedLayerStateTable.Restore`        |

[ENUM_ROSTERS]:
- `[Flags] public enum Rhino.DocObjects.Tables.RestoreLayerProperties : uint` — `None = 0`, `Current = 1`, `Visible = 2`, `Locked = 4`, `Color = 8`, `Linetype = 0x10`, `PrintColor = 0x20`, `PrintWidth = 0x40`, `ViewportVisible = 0x80`, `ViewportColor = 0x100`, `ViewportPrintColor = 0x200`, `ViewportPrintWidth = 0x400`, `RenderMaterial = 0x800`, `SectionStyle = 0x1000`, `NewDetailOn = 0x2000`, `Expanded = 0x4000`, `All = uint.MaxValue`.

## [03]-[ENTRYPOINTS]

[CONSTRUCTION_PLANES]:
- `Rhino.DocObjects.Tables.NamedConstructionPlaneTable.Add(string name, Plane plane) : int` / `Add(ConstructionPlane constructionPlane) : int` — an empty name mints a unique one, an existing name REPLACES that cplane, and `-1` signals rejection; Add/Find/Delete/indexer/Count is the complete mutation roster — no `Modify` or `Rename` exists.
- `NamedConstructionPlaneTable.Find(string name) : int` — resolves the preset index by name, negative on miss.
- `NamedConstructionPlaneTable[int index] : ConstructionPlane` — the indexed read; `Count : int` bounds it; the table implements `IEnumerable<ConstructionPlane>` through `TableEnumerator<NamedConstructionPlaneTable, ConstructionPlane>`, the only enumerable preset table.
- `NamedConstructionPlaneTable.Delete(int index) : bool` / `Delete(string name) : bool` — preset removal by index or name.
- `NamedConstructionPlaneTable.Document : RhinoDoc` / `NamedPositionTable.Document : RhinoDoc` / `NamedLayerStateTable.Document : RhinoDoc` — every preset table re-resolves its owner.
- `RhinoDoc.NamedConstructionPlanes` / `NamedPositions` / `NamedLayerStates` / `Snapshots` / `Worksession` — the document accessor properties reaching these surfaces.
- `Rhino.DocObjects.ConstructionPlane.Plane : Plane` / `Name : string` — the base plane and preset name.
- `ConstructionPlane.GridSpacing : double` / `SnapSpacing : double` / `GridLineCount : int` / `ThickLineFrequency : int` — grid metrics and thick-line cadence.
- `ConstructionPlane.ShowGrid : bool` / `ShowAxes : bool` / `ShowZAxis : bool` / `DepthBuffered : bool` — grid, axis, and depth visibility toggles.
- `ConstructionPlane.ThinLineColor : Color` / `ThickLineColor : Color` / `GridXColor : Color` / `GridYColor : Color` / `GridZColor : Color` — the thin/thick grid-line and axis color vocabulary.
- `Rhino.DocObjects.ConstructionPlaneGridDefaults.GridSpacing : double` / `SnapSpacing : double` / `GridLineCount : int` / `GridThickFrequency : int` / `ShowGrid : bool` / `ShowGridAxes : bool` / `ShowWorldAxes : bool` — the seven default fields seeding a new cplane grid.
- `ConstructionPlane()` seeds WorldXY, `GridSpacing` 1.0, `GridLineCount` 70, `ThickLineFrequency` 5, `DepthBuffered` true, `ShowGrid`/`ShowAxes` true, `ShowZAxis` false, colors from `AppearanceSettings`; the five color members are plain auto properties — no has-custom-color flags exist.

[NAMED_POSITIONS]:
- `Rhino.DocObjects.Tables.NamedPositionTable.Save(string name, IEnumerable<RhinoObject> objects) : Guid` / `Save(string name, IEnumerable<Guid> objectIds) : Guid` — snapshots the current transforms of the given objects under a name, returning the preset id.
- `NamedPositionTable.Restore(Guid id) : bool` / `Restore(string name) : bool` — reapplies a stored transform set by id or name.
- `NamedPositionTable.Update(Guid id) : bool` / `Update(string name) : bool` — rewrites a preset from the objects' current transforms.
- `NamedPositionTable.Append(Guid id, IEnumerable<RhinoObject> objects) : bool` / `Append(Guid id, IEnumerable<Guid> objectIds) : bool` / `Append(string name, IEnumerable<RhinoObject> objects) : bool` / `Append(string name, IEnumerable<Guid> objectIds) : bool` — adds objects to an existing preset.
- `NamedPositionTable.ObjectXform(Guid id, RhinoObject obj, ref Transform xform) : bool` / `ObjectXform(Guid id, Guid objId, ref Transform xform) : bool` — reads one object's stored transform into `xform`.
- `NamedPositionTable.Objects(Guid id) : RhinoObject[]` / `Objects(string name) : RhinoObject[]` / `ObjectIds(Guid id) : Guid[]` / `ObjectIds(string name) : Guid[]` — the participating objects and ids of a preset.
- `NamedPositionTable.Rename(Guid id, string name) : bool` / `Rename(string oldName, string name) : bool` / `Delete(Guid id) : bool` / `Delete(string name) : bool` — preset rename and removal.
- `NamedPositionTable.Name(Guid id) : string` / `Id(string name) : Guid` / `Ids : Guid[]` / `Names : string[]` / `Count : int` — identity resolution and roster.

[NAMED_LAYER_STATES]:
- `Rhino.DocObjects.Tables.NamedLayerStateTable.Save(string name) : int` / `Save(string name, Guid viewportId) : int` — captures the current layer property set, optionally per viewport; an existing name UPDATES that state in place.
- `NamedLayerStateTable.Restore(string name, RestoreLayerProperties properties) : bool` / `Restore(string name, RestoreLayerProperties properties, Guid viewportId) : bool` — reapplies only the property groups the flag mask selects.
- `NamedLayerStateTable.Rename(string oldName, string newName) : bool` / `Delete(string name) : bool` / `FindName(string name) : int` — rename, removal, and index resolution.
- `NamedLayerStateTable.Import(string filename) : int` — imports layer states from a `.3dm` archive, returning the imported count.
- `NamedLayerStateTable.Names : string[]` / `Count : int` — the preset roster; `NamedPositionTable` name overloads resolve name-to-id by linear scan and answer false or null on an empty-guid miss.

[SNAPSHOTS]:
- `Rhino.DocObjects.Tables.SnapshotTable.Names : string[]` — the sole data reader; backed by `ON_3dmSettings_GetSnapShots`, with no capture, restore, rename, or delete on the table.
- `SnapshotTable.Document : RhinoDoc` — re-resolves the owning document; snapshot capture and restore route through `RunScript` and `SnapShotsClient`.

[SNAPSHOT_PARTICIPATION]:
- `Rhino.DocObjects.SnapShots.SnapShotsClient.RegisterSnapShotClient(SnapShotsClient client) : bool` (static) — enlists a plugin client into the snapshot pipeline; the public parameterless constructor auto-adds the instance to the internal client list, the class is `abstract class SnapShotsClient : IDisposable` carrying `CppPointer : nint`, `SerialNumber : int` (get/set), and `Dispose()`, and NO removal member exists — `Dispose` never takes the instance back out of the process client list.
- `SnapShotsClient.PlugInId() : Guid` / `ClientId() : Guid` / `Category() : string` / `Name() : string` — the abstract identity contract; `ApplicationCategory()`/`DocumentCategory()`/`RenderingCategory()`/`ViewsCategory()`/`ObjectsCategory()`/`LayersCategory()`/`LightsCategory()` are static METHODS returning the predefined category strings, and `Category()` returns one of them.
- `SnapShotsClient.SupportsDocument() : bool` / `SaveDocument(RhinoDoc, BinaryArchiveWriter) : bool` / `RestoreDocument(RhinoDoc, BinaryArchiveReader) : bool` / `SnapshotRestored(RhinoDoc) : void` — the document-scoped capture and restore overrides.
- `SnapShotsClient.SupportsObjects() : bool` / `SupportsObject(RhinoObject) : bool` / `SaveObject(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveWriter) : bool` / `RestoreObject(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader) : bool` — the per-object capture and restore overrides.
- `SnapShotsClient.SupportsAnimation() : bool` / `AnimationStart(RhinoDoc, int frames) : void` / `PrepareForDocumentAnimation(RhinoDoc, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimateDocument(RhinoDoc, double pos, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `PrepareForObjectAnimation(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimateObject(RhinoDoc, RhinoObject, ref Transform, double pos, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimationStop(RhinoDoc) : bool` — the interpolation overrides between two captured states.
- `SnapShotsClient.ExtendBoundingBoxForDocumentAnimation(RhinoDoc, BinaryArchiveReader start, BinaryArchiveReader stop, ref BoundingBox) : void` / `ExtendBoundingBoxForObjectAnimation(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader start, BinaryArchiveReader stop, ref BoundingBox) : void` — grow the animation bound so interpolated states stay inside the redraw frame.
- `SnapShotsClient.ObjectTransformNotification(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader) : bool` — the per-object transform-change callback the pipeline fires during capture.
- `SnapShotsClient.IsCurrentModelStateInAnySnapshot(RhinoDoc, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, TextLog = null) : bool` / `IsCurrentModelStateInAnySnapshot(RhinoDoc, RhinoObject, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, TextLog = null) : bool` — fired before a restore to warn on unsaved model state, the `TextLog` collecting the missing items; every abstract member above — 24 in total — is a mandatory override a client subclass implements.
- Restore-pipeline facts: `SnapshotRestored` fires after ALL clients restored their data, and the `ref Transform` on `SaveObject`/`RestoreObject` is identity at first object association, updated on each subsequent object transform.

[WORKSESSION]:
- `Rhino.DocObjects.Worksession.ModelCount : int` / `ModelPaths : string[]` — the reference-model roster count and paths; `ModelCount` includes the active model even unsaved while `ModelPaths` excludes an unsaved active model, so the two can disagree by one.
- `Worksession.FileName : string` / `Name : string` / `RuntimeSerialNumber : uint` / `Document : RhinoDoc` — worksession identity and owner; `FileName` is null and `Name` empty when no worksession or an unsaved one is active.
- `Worksession.ModelPathFromSerialNumber(uint modelSerialNumber) : string` — INSTANCE member resolving against the owning worksession's document; `Worksession.FileNameFromRuntimeSerialNumber(uint runtimeSerialNumber) : string` (static) — the one static worksession-file resolver.

## [04]-[IMPLEMENTATION_LAW]

[PRESET_TOPOLOGY]:
- each named table is a document-scoped name-to-state store: `NamedConstructionPlaneTable` saves the plane, grid metric, and axis-color vocabulary of one cplane; `NamedPositionTable` snapshots per-object transforms keyed by id and name and reapplies them on restore; `NamedLayerStateTable` saves a full layer property set and restores only the groups the `RestoreLayerProperties` mask selects.
- `SnapshotTable` exposes only its name roster; capture, restore, and animation live on `SnapShotsClient`, which a plugin subclasses, registers once through `RegisterSnapShotClient`, and drives through `BinaryArchiveWriter`/`BinaryArchiveReader` — the table never mutates snapshots.
- `Worksession` reads the reference-model roster and never mutates it, and `NamedViewTable` stays owned by the viewport catalog so named-view save and restore never split across two owners.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): table `bool` outcomes and negative-signalling `int` indices fold to `Fin<Unit>`/`Fin<int>`, `ObjectXform`'s `ref Transform` out projects to `Fin<Transform>`, and roster arrays land as `Seq<A>` with zero-pointer slots dropped.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `RestoreLayerProperties` wraps as a bounded flags vocabulary that owns the property-group mask, and the preset kinds close as generated cases so the save/restore rail dispatches totally.
- `Rasm` kernel: cplane `Plane`, grid metrics, and per-object `Transform` values compose the kernel numeric owners, and colors compose the kernel color owner, never re-derived inside the preset layer.

[LOCAL_ADMISSION]:
- a preset enters through the owning table's `Save` or `Add`; restore and update return `bool` outcomes the rail lifts to `Fin`; snapshot participation enters once through `RegisterSnapShotClient`.
- live `ConstructionPlane`, `Worksession`, and archive-reader handles remain inside the document grant; downstream code receives detached preset values, decoded transforms, or projected receipts.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects.Tables` named-preset tables + `Rhino.DocObjects` cplane and worksession carriers + `Rhino.DocObjects.SnapShots` participation
- Owns: named construction planes, named positions, named layer states, the snapshot name roster, the worksession reference-model roster, and plugin snapshot participation.
- Accept: preset save, restore, update, rename, and delete, layer-state restore under a property mask, snapshot participation registration, and worksession roster reads projected onto `Fin`/`Option` rails.
- Reject: snapshot capture and restore through the table, named-view ownership, assumed preset existence, and exception-style table outcomes.
