# [RASM_GRASSHOPPER_API_RHINO_COMMON]

`RhinoCommon` carries the Rhino-side seams the Grasshopper host composes: `RhinoDoc` is the active-model handoff the editor getter arbitrates, and `Rhino.Geometry` mints the value structs and `GeometryBase` reference carriers the component ports and `GardenData` transfer type against.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon Grasshopper-boundary surface
- host: Rhino host runtime, in-process (proprietary McNeel SDK)
- assembly: `RhinoCommon`
- namespace: `Rhino`, `Rhino.Geometry`
- asset: in-process `RhinoCommon.dll` from the installed RhinoWIP bundle at `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/RhinoCommon.dll`
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Rhino` active-model handoff

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------- | :------------ | :-------------------------------------------- |
|  [01]   | `RhinoDoc` | class         | active model, getter target, identity carrier |

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry` port-carrier roster — the typing vocabulary a `PortRow` declares and `GardenData` transports

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------ | :------------ | :------------------------------------------- |
|  [01]   | `Point2d`     | struct        | 2D parameter and screen-plane point          |
|  [02]   | `Rectangle3d` | struct        | oriented planar rectangle                    |
|  [03]   | `Arc`         | struct        | circular arc                                 |
|  [04]   | `Circle`      | struct        | planar circle                                |
|  [05]   | `Sphere`      | struct        | analytic sphere                              |
|  [06]   | `Curve`       | class         | abstract `GeometryBase` curve base           |
|  [07]   | `Surface`     | class         | abstract `GeometryBase` surface base         |
|  [08]   | `Brep`        | class         | `GeometryBase` boundary-representation solid |
|  [09]   | `SubD`        | class         | `GeometryBase` subdivision surface           |
|  [10]   | `Mesh`        | class         | `GeometryBase` polygon mesh                  |
|  [11]   | `TextDot`     | class         | `GeometryBase` annotation dot                |
|  [12]   | `Polyline`    | class         | open or closed `Point3dList` vertex chain    |

- Registers `RhinoCommon` value substrate(`libs/csharp/.api/api-rhinocommon.md`): `Point3d`, `Vector3d`, `Plane`, `Line`, `BoundingBox`, `Transform`, `MeshFace`, `Quaternion`, `Interval`, `Box`, and `GeometryBase` carry their algebra there and type ports by that spelling; the rows above are the carriers this boundary adds beyond it.
- Every row is an opaque port payload here — the folder types against it and never operates on it, so a carrier's members are read at its owning catalogue alone.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Rhino` RhinoDoc access and identity

| [INDEX] | [SURFACE]                                | [SHAPE]         | [CAPABILITY]                    |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `RhinoDoc.ActiveDoc`                     | static property | active document, `null` if none |
|  [02]   | `RhinoDoc.RuntimeSerialNumber -> uint`   | property        | session serial identity         |
|  [03]   | `RhinoDoc.DocumentId -> int`             | property        | session document id             |
|  [04]   | `RhinoDoc.Name -> string`                | property        | document name                   |
|  [05]   | `RhinoDoc.Path -> string`                | property        | document file path              |
|  [06]   | `RhinoDoc.Modified -> bool`              | property        | dirty-state read                |
|  [07]   | `RhinoDoc.ModelUnitSystem -> UnitSystem` | property        | model unit vocabulary           |
|  [08]   | `RhinoDoc.ActiveSpace -> ActiveSpace`    | property        | model or layout space           |
|  [09]   | `RhinoDoc.IsHeadless -> bool`            | property        | headless-document flag          |

- Registers the `Rhino.UI` host-bridge seams (`libs/csharp/.api/api-rhino-ui.md`): the `Dialogs.ShowEditBox` and `Dialogs.ShowNumberBox` native value prompts carry their algebra there and this boundary calls them by that spelling; the rows above are the `RhinoDoc` handoff this partition adds beyond it.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RhinoDoc` crosses one seam — `Editor.BeginRhinoGetter(RhinoDoc)` on `Shell/editor.md`'s `GetterCase` — and a getter-arbitration consumer reads `ActiveDoc` as the default target when the case carries `None`; a direct `RhinoDoc` getter beside the editor is the deleted form
- geometry carriers enter as `typeof(Rhino.Geometry.T)` in `Components/ports.md`'s `PortRow` vocabulary and as record payloads in `Components/data.md`'s shape unions, transported by value or reference and never mutated
- value structs and `GeometryBase` classes split the carrier roster: a struct carrier copies through the port pin, a reference carrier pins the live host object
- the registered value prompts are the Rhino-styled fast lane behind `Eto/windows.md`'s `PickerSpec.EditCase`/`NumberCase`, settling a value where a full Eto dialog is unwarranted

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `RhinoDoc.ActiveDoc` null-gates through `Optional(...)` into `Option<RhinoDoc>`; a carrier's `IsValid` gate folds to `Validation<Error, T>` before the port admits it
- `api-rhino-ui`(`libs/csharp/.api/api-rhino-ui.md`): the registered value prompts settle the `PickerSpec` fast lane, their `bool` return with its `out`/`ref` channel lifting to `Fin<string>`/`Fin<double>` where `false` maps to `Fault.InvalidResult`
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the carrier set is owned as `Components/ports.md`'s `PortRow` `[SmartEnum]` vocabulary so a port declares by row, not hand-typed `typeof`; the `UnitSystem` and `ActiveSpace` host discriminants project onto `[SmartEnum<TKey>]` owners where the folder attaches a unit or space behaviour
- within-folder: `GardenData` records transport value structs by value and the `Editor` and `PickerSpec` owners front the `RhinoDoc` handoff and the `Dialogs` prompts, so every host surface enters through one folder owner per capability, never a second typing path

[LOCAL_ADMISSION]:
- `RhinoDoc` is admitted only as the editor getter payload; Rhino document semantics are `Rasm.Rhino`'s concern entirely and no owner in this folder adjudicates the document
- geometry carriers are opaque port and data payloads; the folder defers every geometric operation to the Rasm kernel and the host rather than re-implementing a `Rhino.Geometry` op
- native input is the registered value prompts through the `PickerSpec` fast lane; a hand-rolled edit or number dialog is the deleted form

[RAIL_LAW]:
- Partition: RhinoCommon Grasshopper boundary (`Rhino`, `Rhino.Geometry` port carriers)
- Owns: the `RhinoDoc` getter-handoff payload and the boundary-added `Rhino.Geometry` port carriers over the registered value substrate
- Accept: active-document access at the editor getter seam, geometry-carrier typing at ports and data transfer, native value input through the registered prompt fast lane
- Reject: Rhino document mutation and semantics (`Rasm.Rhino`), Eto and Rhino UI styling and prompts (`api-rhino-ui`), the GH2 document graph (`api-gh2-document`), a re-tabling of the substrate carrier algebra
