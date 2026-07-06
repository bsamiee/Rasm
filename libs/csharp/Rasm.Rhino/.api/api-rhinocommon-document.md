# [API_RHINOCOMMON_DOCUMENT]

Catalog scope: the `RhinoDoc` lifecycle, table, event, and undo surface the package composes — document state, object-table mutation, attributes, and the full document/view event families.

[NAMESPACES]:
- `Rhino` — `RhinoDoc` (tables, `RuntimeSerialNumber`/`FromRuntimeSerialNumber`, readiness flags, `BeginUndoRecord`/`EndUndoRecord`/`AddCustomUndoEvent`, `ModelUnitSystem`, `Path`, `Open`/`CreateHeadless`/`OpenHeadless`, `Save`/`WriteFile`/`Export`/`Import`), `RhinoApp` (`WriteLine`, `RunScript`, `Idle`, `InvokeOnUiThread`, `IsOnMainThread`, `MainLoop`, `ToolbarFiles`), `RhinoMath`, `UnitSystem`.
- `Rhino.DocObjects` — `RhinoObject`, `ObjectAttributes`, `ObjRef`, `GripObject`, `ObjectEnumeratorSettings`, `ObjectType`, `HistoryRecord`, `Layer`, `Material`, `Group`, `Linetype`, `DimensionStyle`, `HatchPattern`, `ConstructionPlane`, `ViewInfo`, `ModelComponent`/`ModelComponentType`, `EarthAnchorPoint`, `PageViewGroup`, `SelectionMethod`, `PickContext`/`PickStyle`/`PickMode`, `ComponentIndex`, `SectionStyle`.
- `Rhino.DocObjects.Tables` — `ObjectTable` (add/delete/replace/transform/select/hide/lock/purge/undelete families), `NamedViewTable`, `NamedPositionTable`, `LayerTableEventType`.
- `Rhino.DocObjects.Custom` — `UserDictionary`.
- `Rhino.Collections` — `ArchivableDictionary`.
- `Rhino.Runtime` — `TextFields` instance-attribute field surface, `HostUtils.RunningInDarkMode`.

[EVENT_FAMILIES]:
- `RhinoDoc` object events — `AddRhinoObject`, `DeleteRhinoObject`, `ReplaceRhinoObject`, `ModifyObjectAttributes`, `UndeleteRhinoObject`, `PurgeRhinoObject`, `SelectObjects`, `DeselectObjects`, `DeselectAllObjects`.
- `RhinoDoc` table events — `LayerTableEvent`, `MaterialTableEvent`, `GroupTableEvent`, `LinetypeTableEvent`, `LightTableEvent`, `DimensionStyleTableEvent`, `InstanceDefinitionTableEvent`.
- `RhinoDoc` lifecycle events — `BeginOpenDocument`/`EndOpenDocument`, `BeginSaveDocument`/`EndSaveDocument`, `CloseDocument`, `NewDocument`, `ActiveDocumentChanged`, `DocumentPropertiesChanged`, `UnitsChangedWithScaling`, `UserStringChanged`.
