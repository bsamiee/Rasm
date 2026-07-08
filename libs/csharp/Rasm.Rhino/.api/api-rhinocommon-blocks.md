# [RASM_RHINO_API_RHINOCOMMON_BLOCKS]

Catalog scope: the instance-definition (block) surface — definition tables, instances, linked archives, previews, and the attribute text-field bridge.

[NAMESPACES]:
- `Rhino.DocObjects` — `InstanceDefinition`/`InstanceDefinitionGeometry` (find/add/modify/delete/undelete/purge/compact/export, linked-archive update/refresh/destroy, reference/container/usage queries, `CreatePreviewBitmap`), `InstanceObject` (`Explode`, `InstanceXform`, `InsertionPoint`, sub-object resolution), `InstanceDefinitionArchiveFileStatus`, `InstanceDefinitionLayerStyle`, `InstanceDefinitionUpdateType`, `LinkedInstanceDefinitionUpdateStyle`, `InstanceDefinitionTableEventArgs`/`EventType`, `ModelComponent.IsValidComponentName`, `ActiveSpace`, `DimStyle`.
- `Rhino.DocObjects.Tables` — `InstanceDefinitionTable` (full CRUD + linked-block families, `GetUnusedInstanceDefinitionName`), `ObjectTable` block members (`AddInstanceObject`, `ReplaceInstanceObject`, `AddExplodedInstancePieces`, `TransformWithHistory`).
- `Rhino.Geometry` — `InstanceReferenceGeometry`, `GeometryBase` (`Duplicate`/`Transform`/`GetBoundingBox`/`DataCRC`), `TextEntity.Create`.
- `Rhino.FileIO` — `File3dm` block-side reads (`AllInstanceDefinitions`, `Objects`, `ReadWithLog`, `ReadPreviewImage`, `Settings.ModelUnitSystem`).
- `Rhino.Runtime` — `TextFields.GetInstanceAttributeFields`/`InstanceAttributeField`.
- `Rhino` — `RhinoDoc.LinkedInstanceDefinitionUpdate`, `RhinoMath.UnitScale`, `DimStyles.Current`, distance display precision.
