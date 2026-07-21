# [RASM_GRASSHOPPER_API_RHINO_COMMON]

`RhinoCommon` carries three Rhino-side seams the Grasshopper host composes: `RhinoDoc` is the active-model handoff the editor getter yields focus to, `Rhino.UI.Dialogs` is the native edit/number prompt fast lane, and `Rhino.Geometry` supplies the value and geometry carriers the component ports and Garden data transfer type against. Carriers split value structs (`Point3d`, `Line`, `Transform`) from `GeometryBase` reference geometry (`Curve`, `Brep`, `Mesh`). Every member is catalog-verified against the installed RhinoWIP `RhinoCommon.dll`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `RhinoCommon`
- package: `RhinoCommon` (Rhino 9 WIP host managed API; not a NuGet pin — the in-process `RhinoCommon.dll` shipped in the Rhino application bundle is the resolved asset)
- assembly: `RhinoCommon`
- namespace: `Rhino` (`RhinoDoc`)
- namespace: `Rhino.UI` (`Dialogs`)
- namespace: `Rhino.Geometry` (value and geometry port carriers)
- asset: host assembly; `/Applications/RhinoWIP.app/Contents/Frameworks/RhCore.framework/Versions/Current/Resources/RhinoCommon.dll` resolved from the installed RhinoWIP bundle
- rail: host-rhino

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: active-model handoff
- namespace: `Rhino`
- rail: host-rhino

`RhinoDoc` is the live model the editor getter arbitrates; it transports as an opaque payload, its document semantics owned by `Rasm.Rhino`, never adjudicated in this folder.

| [INDEX] | [SYMBOL]  | [KIND] | [CAPABILITY]                                  |
| :-----: | :-------- | :----- | :-------------------------------------------- |
|  [01]   | `RhinoDoc` | class  | active model, getter target, identity carrier |

[PUBLIC_TYPE_SCOPE]: Rhino.Geometry value carriers
- namespace: `Rhino.Geometry`
- rail: host-rhino

Value structs the `Ports` `PortRow` vocabulary types against and the `GardenData` shape records carry by value; each is an immutable geometric value the folder transports without mutation.

| [INDEX] | [SYMBOL]      | [KIND] | [CAPABILITY]                       |
| :-----: | :------------ | :----- | :--------------------------------- |
|  [01]   | `Point3d`     | struct | 3D model-space point               |
|  [02]   | `Vector3d`    | struct | 3D direction and displacement      |
|  [03]   | `Point2d`     | struct | 2D parameter and screen-plane point |
|  [04]   | `Line`        | struct | finite from/to segment             |
|  [05]   | `Arc`         | struct | circular arc                       |
|  [06]   | `Circle`      | struct | planar circle                      |
|  [07]   | `Rectangle3d` | struct | oriented planar rectangle          |
|  [08]   | `Box`         | struct | oriented bounding box              |
|  [09]   | `Sphere`      | struct | analytic sphere                    |
|  [10]   | `Plane`       | struct | origin and axis-frame plane        |
|  [11]   | `Transform`   | struct | 4×4 affine transform matrix        |
|  [12]   | `Quaternion`  | struct | rotation quaternion                |
|  [13]   | `Interval`    | struct | 1D numeric domain                  |
|  [14]   | `MeshFace`    | struct | triangle or quad face index quad   |

[PUBLIC_TYPE_SCOPE]: Rhino.Geometry reference geometry
- namespace: `Rhino.Geometry`
- rail: host-rhino

`GeometryBase`-derived reference types the surface, curve, and mesh port carriers pin; `Polyline` derives from `Point3dList` as an open or closed vertex chain.

| [INDEX] | [SYMBOL]   | [KIND]                 | [CAPABILITY]                          |
| :-----: | :--------- | :--------------------- | :------------------------------------ |
|  [01]   | `Curve`    | class (`GeometryBase`) | abstract curve base                   |
|  [02]   | `Surface`  | class (`GeometryBase`) | abstract surface base                 |
|  [03]   | `Brep`     | class (`GeometryBase`) | boundary-representation poly-surface  |
|  [04]   | `SubD`     | class (`GeometryBase`) | subdivision surface                   |
|  [05]   | `Mesh`     | class (`GeometryBase`) | polygon mesh                          |
|  [06]   | `TextDot`  | class (`GeometryBase`) | annotation dot                        |
|  [07]   | `Polyline` | class (`Point3dList`)  | open or closed vertex chain           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: RhinoDoc access and identity
- namespace: `Rhino`
- rail: host-rhino

`ActiveDoc` is the getter's fallback when the editor case carries `None`; the identity and state reads let a getter-arbitration consumer key and inspect the handed-off document.

| [INDEX] | [SURFACE]                          | [SHAPE]           | [CAPABILITY]                        |
| :-----: | :--------------------------------- | :---------------- | :---------------------------------- |
|  [01]   | `RhinoDoc.ActiveDoc`               | static property   | the active document, `null` if none |
|  [02]   | `RuntimeSerialNumber` / `DocumentId` | property          | session document identity           |
|  [03]   | `Name` / `Path`                    | property          | document name and file path         |
|  [04]   | `Modified`                         | property          | dirty-state read                    |
|  [05]   | `ModelUnitSystem`                  | `UnitSystem`      | model unit vocabulary               |
|  [06]   | `ActiveSpace`                      | `ActiveSpace`     | model or layout space               |
|  [07]   | `IsHeadless`                       | property          | headless-document flag              |

[ENTRYPOINT_SCOPE]: Rhino.UI.Dialogs native prompts
- namespace: `Rhino.UI`
- rail: host-rhino

Two prompt members settle a text or numeric value through the Rhino-native fast lane; each returns `bool` for accepted-versus-dismissed and emits its value through an `out`/`ref` channel. `ShowEditBox(string title, string message, string defaultText, bool multiline, out string text)` and `ShowNumberBox(string title, string message, ref double number[, double minimum, double maximum])` are the exact typed forms.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                     | [CAPABILITY]            |
| :-----: | :--------------------- | :----------------------------------------------- | :---------------------- |
|  [01]   | `Dialogs.ShowEditBox`  | `(title, message, defaultText, multiline, out text)` → `bool` | native edit-text prompt |
|  [02]   | `Dialogs.ShowNumberBox` | `(title, message, ref number)` → `bool`          | unbounded number prompt |
|  [03]   | `Dialogs.ShowNumberBox` | `(title, message, ref number, minimum, maximum)` → `bool` | bounded number prompt   |

## [04]-[IMPLEMENTATION_LAW]

[RHINO_COMMON_TOPOLOGY]:
- `RhinoDoc` crosses one seam only — `Editor.BeginRhinoGetter(RhinoDoc)` on `Shell/editor.md`'s `GetterCase` — and a getter-arbitration consumer reads `ActiveDoc` as the default target when the case carries `None`; a direct `RhinoDoc` getter beside the editor bypasses arbitration and is the deleted form
- geometry carriers enter as `typeof(Rhino.Geometry.T)` in `Components/ports.md`'s `PortRow` vocabulary and as record payloads in `Components/data.md`'s shape unions; the folder transports each by value or reference without ever mutating the geometry
- value structs and `GeometryBase` classes split the carrier roster: a struct carrier copies through the port pin, a reference carrier pins the live host object
- `ShowEditBox`/`ShowNumberBox` are the Rhino-styled fast lane behind `Eto/windows.md`'s `PickerSpec.EditCase`/`NumberCase`, settling a value where a full Eto dialog is unwarranted

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): `RhinoDoc.ActiveDoc` null-gates through `Optional(RhinoDoc.ActiveDoc)` into `Option<RhinoDoc>`; a `ShowEditBox`/`ShowNumberBox` `bool` return with its `out`/`ref` channel lifts to `Fin<string>`/`Fin<double>` where `false` maps to `Fault.InvalidResult`; a carrier's `IsValid` gate folds to `Validation<Error, T>` before the port admits it
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the carrier set is owned as `Components/ports.md`'s `PortRow` `[SmartEnum]` vocabulary so a port declares by row, not by hand-typed `typeof`; host discriminants `UnitSystem` and `ActiveSpace` project onto `[SmartEnum<TKey>]` owners where the folder attaches a unit or space behaviour

[LOCAL_ADMISSION]:
- `RhinoDoc` is admitted only as the editor getter payload; Rhino document semantics are `Rasm.Rhino`'s concern entirely and no owner in this folder adjudicates the document
- geometry carriers are opaque port and data payloads; the folder never re-implements a `Rhino.Geometry` operation, deferring all geometric logic to the Rasm kernel and the host
- native input is `Dialogs.ShowEditBox`/`ShowNumberBox` through the `PickerSpec` fast lane; a hand-rolled edit or number dialog is the deleted form

[RAIL_LAW]:
- Package: `RhinoCommon` (Rhino document and geometry carriers)
- Owns: the `RhinoDoc` getter-handoff payload, the `Rhino.UI.Dialogs` edit and number prompts, and the `Rhino.Geometry` value and reference port carriers
- Accept: active-document access at the editor getter seam, native edit and number input prompts, geometry-carrier typing at ports and data transfer
- Reject: Rhino document mutation and semantics (`Rasm.Rhino`), Eto and Rhino UI styling (`api-rhino-ui`), the GH2 document graph (`api-gh2-document`)
