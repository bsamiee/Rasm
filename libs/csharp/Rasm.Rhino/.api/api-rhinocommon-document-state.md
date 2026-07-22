# [RASM_RHINO_API_RHINOCOMMON_DOCUMENT_STATE]

This catalog owns the document-scoped saved-state presets — named construction planes, named positions, named layer states, the snapshot name roster, and the worksession reference-model roster — beside the `SnapShotsClient` seam through which a plugin captures and restores snapshots. `NamedViewTable` stays owned by Viewport/operations.md through `NamedViewOp`; the `ArchivableDictionary` and `PersistentSettings` custody spine lives in api-rhinocommon-persistence.md. `SnapshotTable` carries its name roster alone: capture and restore ride `RunScript` and `SnapShotsClient`, never a table method.

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
- `Rhino.DocObjects.Tables.NamedConstructionPlaneTable.Add(string name, Plane plane) : int` / `Add(ConstructionPlane constructionPlane) : int` — an empty name mints a unique one, an existing name REPLACES that cplane, `-1` signals rejection; Add/Find/Delete/indexer/Count is the whole roster, no `Modify` or `Rename`.
- `NamedConstructionPlaneTable.Find(string name) : int` — resolves the preset index by name, negative on miss.
- `NamedConstructionPlaneTable[int index] : ConstructionPlane` / `Count : int` — indexed read bounded by count; the table alone implements `IEnumerable<ConstructionPlane>`.
- `NamedConstructionPlaneTable.Delete(int index) : bool` / `Delete(string name) : bool` — preset removal by index or name.
- `NamedConstructionPlaneTable.Document : RhinoDoc` / `NamedPositionTable.Document : RhinoDoc` / `NamedLayerStateTable.Document : RhinoDoc` — every preset table re-resolves its owner.
- `RhinoDoc.NamedConstructionPlanes` / `NamedPositions` / `NamedLayerStates` / `Snapshots` / `Worksession` — the document accessor properties reaching these surfaces.
- `Rhino.DocObjects.ConstructionPlane.Plane : Plane` / `Name : string` — the base plane and preset name.
- `ConstructionPlane.GridSpacing : double` / `SnapSpacing : double` / `GridLineCount : int` / `ThickLineFrequency : int` — grid metrics and thick-line cadence.
- `ConstructionPlane.ShowGrid : bool` / `ShowAxes : bool` / `ShowZAxis : bool` / `DepthBuffered : bool` — grid, axis, and depth visibility toggles.
- `ConstructionPlane.ThinLineColor : Color` / `ThickLineColor : Color` / `GridXColor : Color` / `GridYColor : Color` / `GridZColor : Color` — the thin/thick grid-line and axis color vocabulary; each is a plain auto property with no has-custom-color flag.
- `Rhino.DocObjects.ConstructionPlaneGridDefaults.GridSpacing : double` / `SnapSpacing : double` / `GridLineCount : int` / `GridThickFrequency : int` / `ShowGrid : bool` / `ShowGridAxes : bool` / `ShowWorldAxes : bool` — the seven default fields seeding a new cplane grid.
- `ConstructionPlane()` seeds WorldXY with `GridSpacing` 1.0, `GridLineCount` 70, `ThickLineFrequency` 5, `DepthBuffered` true, `ShowGrid`/`ShowAxes` true, `ShowZAxis` false, and colors from `AppearanceSettings`.

[NAMED_POSITIONS]:
- `Rhino.DocObjects.Tables.NamedPositionTable.Save(string name, IEnumerable<RhinoObject> objects) : Guid` / `Save(string name, IEnumerable<Guid> objectIds) : Guid` — snapshots the objects' current transforms under a name, returning the preset id.
- `NamedPositionTable.Restore(Guid id) : bool` / `Restore(string name) : bool` — reapplies a stored transform set by id or name.
- `NamedPositionTable.Update(Guid id) : bool` / `Update(string name) : bool` — rewrites a preset from the objects' current transforms.
- `NamedPositionTable.Append(Guid id, IEnumerable<RhinoObject> objects) : bool` / `Append(Guid id, IEnumerable<Guid> objectIds) : bool` / `Append(string name, IEnumerable<RhinoObject> objects) : bool` / `Append(string name, IEnumerable<Guid> objectIds) : bool` — adds objects to an existing preset.
- `NamedPositionTable.ObjectXform(Guid id, RhinoObject obj, ref Transform xform) : bool` / `ObjectXform(Guid id, Guid objId, ref Transform xform) : bool` — reads one object's stored transform into `xform`.
- `NamedPositionTable.Objects(Guid id) : RhinoObject[]` / `Objects(string name) : RhinoObject[]` / `ObjectIds(Guid id) : Guid[]` / `ObjectIds(string name) : Guid[]` — the participating objects and ids of a preset.
- `NamedPositionTable.Rename(Guid id, string name) : bool` / `Rename(string oldName, string name) : bool` / `Delete(Guid id) : bool` / `Delete(string name) : bool` — preset rename and removal.
- `NamedPositionTable.Name(Guid id) : string` / `Id(string name) : Guid` / `Ids : Guid[]` / `Names : string[]` / `Count : int` — identity resolution and roster; the name overloads scan linearly and answer false or null on an empty-guid miss.

[NAMED_LAYER_STATES]:
- `Rhino.DocObjects.Tables.NamedLayerStateTable.Save(string name) : int` / `Save(string name, Guid viewportId) : int` — captures the current layer property set, optionally per viewport; an existing name UPDATES that state in place.
- `NamedLayerStateTable.Restore(string name, RestoreLayerProperties properties) : bool` / `Restore(string name, RestoreLayerProperties properties, Guid viewportId) : bool` — reapplies only the property groups the flag mask selects.
- `NamedLayerStateTable.Rename(string oldName, string newName) : bool` / `Delete(string name) : bool` / `FindName(string name) : int` — rename, removal, and index resolution.
- `NamedLayerStateTable.Import(string filename) : int` — imports layer states from a `.3dm` archive, returning the imported count.
- `NamedLayerStateTable.Names : string[]` / `Count : int` — the preset roster.

[SNAPSHOTS]:
- `Rhino.DocObjects.Tables.SnapshotTable.Names : string[]` — reads the snapshot names alone, backed by `ON_3dmSettings_GetSnapShots`; no capture, restore, rename, or delete rides the table.
- `SnapshotTable.Document : RhinoDoc` — re-resolves the owning document; capture and restore route through `RunScript` and `SnapShotsClient`.

[SNAPSHOT_PARTICIPATION]:
- `Rhino.DocObjects.SnapShots.SnapShotsClient.RegisterSnapShotClient(SnapShotsClient client) : bool` (static) — enlists a client into the snapshot pipeline; the parameterless constructor already auto-adds the instance to the internal client list, `abstract class SnapShotsClient : IDisposable` carries `CppPointer : nint`, `SerialNumber : int` (get/set), and `Dispose()`, and no removal member exists, so `Dispose` never takes the instance back out of the process client list.
- `SnapShotsClient.PlugInId() : Guid` / `ClientId() : Guid` / `Category() : string` / `Name() : string` — the abstract identity contract; `Category()` returns one of the static category strings `ApplicationCategory()` / `DocumentCategory()` / `RenderingCategory()` / `ViewsCategory()` / `ObjectsCategory()` / `LayersCategory()` / `LightsCategory()`.
- `SnapShotsClient.SupportsDocument() : bool` / `SaveDocument(RhinoDoc, BinaryArchiveWriter) : bool` / `RestoreDocument(RhinoDoc, BinaryArchiveReader) : bool` / `SnapshotRestored(RhinoDoc) : void` — the document-scoped capture and restore overrides; `SnapshotRestored` fires after ALL clients restore their data.
- `SnapShotsClient.SupportsObjects() : bool` / `SupportsObject(RhinoObject) : bool` / `SaveObject(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveWriter) : bool` / `RestoreObject(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader) : bool` — the per-object capture and restore overrides; the `ref Transform` is identity at first object association and updates on each subsequent object transform.
- `SnapShotsClient.SupportsAnimation() : bool` / `AnimationStart(RhinoDoc, int frames) : void` / `PrepareForDocumentAnimation(RhinoDoc, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimateDocument(RhinoDoc, double pos, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `PrepareForObjectAnimation(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimateObject(RhinoDoc, RhinoObject, ref Transform, double pos, BinaryArchiveReader start, BinaryArchiveReader stop) : bool` / `AnimationStop(RhinoDoc) : bool` — the interpolation overrides between two captured states.
- `SnapShotsClient.ExtendBoundingBoxForDocumentAnimation(RhinoDoc, BinaryArchiveReader start, BinaryArchiveReader stop, ref BoundingBox) : void` / `ExtendBoundingBoxForObjectAnimation(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader start, BinaryArchiveReader stop, ref BoundingBox) : void` — grow the animation bound so interpolated states stay inside the redraw frame.
- `SnapShotsClient.ObjectTransformNotification(RhinoDoc, RhinoObject, ref Transform, BinaryArchiveReader) : bool` — fires during capture on every per-object transform change.
- `SnapShotsClient.IsCurrentModelStateInAnySnapshot(RhinoDoc, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, TextLog = null) : bool` / `IsCurrentModelStateInAnySnapshot(RhinoDoc, RhinoObject, BinaryArchiveReader, SimpleArrayBinaryArchiveReader, TextLog = null) : bool` — fired before a restore to warn on unsaved model state, the `TextLog` collecting the missing items; every abstract member above is a mandatory override a client subclass implements.

[WORKSESSION]:
- `Rhino.DocObjects.Worksession.ModelCount : int` / `ModelPaths : string[]` — the reference-model roster count and paths; `ModelCount` counts the active model even unsaved while `ModelPaths` excludes an unsaved active model, so the two can disagree by one.
- `Worksession.FileName : string` / `Name : string` / `RuntimeSerialNumber : uint` / `Document : RhinoDoc` — worksession identity and owner; `FileName` is null and `Name` empty when no worksession or an unsaved one is active.
- `Worksession.ModelPathFromSerialNumber(uint modelSerialNumber) : string` — instance resolver against the owning worksession's document; `Worksession.FileNameFromRuntimeSerialNumber(uint runtimeSerialNumber) : string` (static) — the one static worksession-file resolver.

## [04]-[IMPLEMENTATION_LAW]

[PRESET_TOPOLOGY]:
- each named table is a document-scoped name-to-state store: `NamedConstructionPlaneTable` over one cplane's plane, grid metric, and axis-color vocabulary; `NamedPositionTable` over per-object transforms keyed by id and name and reapplied on restore; `NamedLayerStateTable` over a full layer property set restored through the `RestoreLayerProperties` mask.
- `SnapshotTable` carries the name roster alone; capture, restore, and animation live on `SnapShotsClient`, subclassed and registered once through `RegisterSnapShotClient` and driven over `BinaryArchiveWriter`/`BinaryArchiveReader`.
- `Worksession` reads the reference-model roster and never mutates it.

[STACKING]:
- `LanguageExt.Core`(`libs/csharp/.api/api-languageext.md`): table `bool` outcomes and negative-signalling `int` indices fold to `Fin<Unit>`/`Fin<int>`, `ObjectXform`'s `ref Transform` out projects to `Fin<Transform>`, and roster arrays land as `Seq<A>` with zero-pointer slots dropped.
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): `RestoreLayerProperties` wraps as a bounded flags vocabulary owning the property-group mask, and the preset kinds close as generated cases so the save/restore rail dispatches totally.
- `Rasm` kernel: cplane `Plane`, grid metrics, and per-object `Transform` values compose the kernel numeric owners and colors compose the kernel color owner, never re-derived inside the preset layer.

[LOCAL_ADMISSION]:
- a preset enters through the owning table's `Save` or `Add`; restore and update return `bool` outcomes the rail lifts to `Fin`; snapshot participation enters once through `RegisterSnapShotClient`.
- live `ConstructionPlane`, `Worksession`, and archive-reader handles stay inside the document grant; downstream code receives detached preset values, decoded transforms, or projected receipts.

[RAIL_LAW]:
- Surface: `Rhino.DocObjects.Tables` named-preset tables + `Rhino.DocObjects` cplane and worksession carriers + `Rhino.DocObjects.SnapShots` participation
- Owns: named construction planes, named positions, named layer states, the snapshot name roster, the worksession reference-model roster, and plugin snapshot participation.
- Accept: preset save, restore, update, rename, and delete, layer-state restore under a property mask, snapshot participation registration, and worksession roster reads projected onto `Fin`/`Option` rails.
- Reject: snapshot capture and restore through the table, named-view ownership, assumed preset existence, and exception-style table outcomes.
