# [RASM_GRASSHOPPER_API_RHINO_COMMON]

`RhinoCommon` carries the Rhino-side seams the Grasshopper host composes: `RhinoDoc` is the active-model handoff the editor getter arbitrates, `Rhino.UI.Dialogs` fast-lanes native edit and number prompts, and `Rhino.Geometry` mints the value structs and `GeometryBase` reference carriers the component ports and `GardenData` transfer type against.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon` (RhinoWIP host managed API)
- assembly: `RhinoCommon`
- namespace: `Rhino`, `Rhino.UI`, `Rhino.Geometry`
- asset: in-process `RhinoCommon.dll` from the installed RhinoWIP bundle at `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/RhinoCommon.dll`
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Rhino` active-model handoff

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------- | :------------ | :-------------------------------------------- |
|  [01]   | `RhinoDoc` | class         | active model, getter target, identity carrier |

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry` value carriers

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------ | :------------ | :---------------------------------- |
|  [01]   | `Point3d`     | struct        | 3D model-space point                |
|  [02]   | `Vector3d`    | struct        | 3D direction and displacement       |
|  [03]   | `Point2d`     | struct        | 2D parameter and screen-plane point |
|  [04]   | `Line`        | struct        | finite from/to segment              |
|  [05]   | `Arc`         | struct        | circular arc                        |
|  [06]   | `Circle`      | struct        | planar circle                       |
|  [07]   | `Rectangle3d` | struct        | oriented planar rectangle           |
|  [08]   | `Box`         | struct        | oriented bounding box               |
|  [09]   | `Sphere`      | struct        | analytic sphere                     |
|  [10]   | `Plane`       | struct        | origin and axis-frame plane         |
|  [11]   | `Transform`   | struct        | 4×4 affine transform matrix         |
|  [12]   | `Quaternion`  | struct        | rotation quaternion                 |
|  [13]   | `Interval`    | struct        | 1D numeric domain                   |
|  [14]   | `MeshFace`    | struct        | triangle or quad face index quad    |

[PUBLIC_TYPE_SCOPE]: `Rhino.Geometry` reference geometry

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------- | :------------ | :------------------------------------------- |
|  [01]   | `Curve`    | class         | abstract `GeometryBase` curve base           |
|  [02]   | `Surface`  | class         | abstract `GeometryBase` surface base         |
|  [03]   | `Brep`     | class         | `GeometryBase` boundary-representation solid |
|  [04]   | `SubD`     | class         | `GeometryBase` subdivision surface           |
|  [05]   | `Mesh`     | class         | `GeometryBase` polygon mesh                  |
|  [06]   | `TextDot`  | class         | `GeometryBase` annotation dot                |
|  [07]   | `Polyline` | class         | open or closed `Point3dList` vertex chain    |

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

[ENTRYPOINT_SCOPE]: `Rhino.UI` Dialogs native prompts

Each prompt settles a value through the Rhino-native fast lane, the accepted-versus-dismissed verdict on the `bool` return and the value on an `out`/`ref` channel.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------------- | :------ | :---------------------- |
|  [01]   | `Dialogs.ShowEditBox(string, string, string, bool, out string) -> bool`     | static  | native edit-text prompt |
|  [02]   | `Dialogs.ShowNumberBox(string, string, ref double) -> bool`                 | static  | unbounded number prompt |
|  [03]   | `Dialogs.ShowNumberBox(string, string, ref double, double, double) -> bool` | static  | bounded number prompt   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RhinoDoc` crosses one seam — `Editor.BeginRhinoGetter(RhinoDoc)` on `Shell/editor.md`'s `GetterCase` — and a getter-arbitration consumer reads `ActiveDoc` as the default target when the case carries `None`; a direct `RhinoDoc` getter beside the editor is the deleted form
- geometry carriers enter as `typeof(Rhino.Geometry.T)` in `Components/ports.md`'s `PortRow` vocabulary and as record payloads in `Components/data.md`'s shape unions, transported by value or reference and never mutated
- value structs and `GeometryBase` classes split the carrier roster: a struct carrier copies through the port pin, a reference carrier pins the live host object
- `ShowEditBox`/`ShowNumberBox` are the Rhino-styled fast lane behind `Eto/windows.md`'s `PickerSpec.EditCase`/`NumberCase`, settling a value where a full Eto dialog is unwarranted

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `RhinoDoc.ActiveDoc` null-gates through `Optional(...)` into `Option<RhinoDoc>`; a `ShowEditBox`/`ShowNumberBox` `bool` return with its `out`/`ref` channel lifts to `Fin<string>`/`Fin<double>` where `false` maps to `Fault.InvalidResult`; a carrier's `IsValid` gate folds to `Validation<Error, T>` before the port admits it
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the carrier set is owned as `Components/ports.md`'s `PortRow` `[SmartEnum]` vocabulary so a port declares by row, not hand-typed `typeof`; the `UnitSystem` and `ActiveSpace` host discriminants project onto `[SmartEnum<TKey>]` owners where the folder attaches a unit or space behaviour
- within-folder: `GardenData` records transport value structs by value and the `Editor` and `PickerSpec` owners front the `RhinoDoc` handoff and the `Dialogs` prompts, so every host surface enters through one folder owner per capability, never a second typing path

[LOCAL_ADMISSION]:
- `RhinoDoc` is admitted only as the editor getter payload; Rhino document semantics are `Rasm.Rhino`'s concern entirely and no owner in this folder adjudicates the document
- geometry carriers are opaque port and data payloads; the folder defers every geometric operation to the Rasm kernel and the host rather than re-implementing a `Rhino.Geometry` op
- native input is `Dialogs.ShowEditBox`/`ShowNumberBox` through the `PickerSpec` fast lane; a hand-rolled edit or number dialog is the deleted form

[RAIL_LAW]:
- Package: `RhinoCommon` (Rhino document and geometry carriers)
- Owns: the `RhinoDoc` getter-handoff payload, the `Rhino.UI.Dialogs` edit and number prompts, and the `Rhino.Geometry` value and reference port carriers
- Accept: active-document access at the editor getter seam, native edit and number input prompts, geometry-carrier typing at ports and data transfer
- Reject: Rhino document mutation and semantics (`Rasm.Rhino`), Eto and Rhino UI styling (`api-rhino-ui`), the GH2 document graph (`api-gh2-document`)
