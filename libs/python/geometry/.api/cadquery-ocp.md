# [PY_GEOMETRY_API_CADQUERY_OCP]

`cadquery-ocp` supplies Python bindings to the OpenCASCADE Technology (OCCT) geometric modeling kernel: it exposes the full BREP topology hierarchy (`TopoDS`), geometry primitives (`gp`, `Geom`), shape builders (`BRepBuilderAPI`, `BRepPrimAPI`, `BRepOffsetAPI`), Boolean/feature operations (`BRepAlgoAPI`), topology traversal (`TopExp`), triangulation (`BRepMesh`), STEP/IGES exchange (`STEPControl`, `IGESControl`), and the XCAF assembly/color/name path (`STEPCAFControl`, `XCAFDoc`, `TDocStd`) as flat `OCP.*` submodules. It is the sole PyPI OCP path for the STEP/IGES B-rep hop; the package owner composes `STEPCAFControl_Reader`/`XCAFDoc_DocumentTool`, `TopExp_Explorer` traversal, and `BRepMesh_IncrementalMesh` tessellation into the `step-bridge` owner, never re-implementing the kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cadquery-ocp`
- package: `cadquery-ocp`
- module: `OCP`
- owner: `geometry`
- rail: step-bridge / cad-kernel
- installed: `7.9.3.1.1`
- entry points: none (library only)
- capability: OCCT BREP topology, parametric `gp`/`Geom` geometry, primitive and feature shape construction, n-ary Boolean and offset operations over `TopTools_ListOfShape`, fillet/chamfer local operations, mesh triangulation, STEP/IGES exchange, and the XCAF document model carrying assembly structure, colors, names, layers, and materials

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: topology family — `OCP.TopoDS` / `OCP.TopAbs`
- rail: cad-kernel topology

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :----------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `TopoDS_Shape`     | base shape       | orientation, location, topology access             |
|  [02]   | `TopoDS_Vertex`    | 0D shape         | point in topology                                  |
|  [03]   | `TopoDS_Edge`      | 1D shape         | bounded curve in topology                          |
|  [04]   | `TopoDS_Wire`      | 1D compound      | connected edge collection                          |
|  [05]   | `TopoDS_Face`      | 2D shape         | bounded surface element                            |
|  [06]   | `TopoDS_Shell`     | 2D compound      | connected face collection                          |
|  [07]   | `TopoDS_Solid`     | 3D shape         | closed volume                                      |
|  [08]   | `TopoDS_CompSolid` | 3D compound      | connected solid collection                         |
|  [09]   | `TopoDS_Compound`  | generic compound | heterogeneous shape collection                     |
|  [10]   | `TopoDS_Builder`   | shape builder    | `Add`/`Remove`/`Make*` topology mutations          |
|  [11]   | `TopoDS_Iterator`  | traversal        | iterate direct sub-shapes of a shape               |
|  [12]   | `TopoDS`           | downcast utility | `Edge_s`/`Face_s`/`Vertex_s`/`Wire_s` static casts |
|  [13]   | `TopAbs_ShapeEnum` | enum             | `TopAbs_VERTEX` … `TopAbs_COMPOUND` shape kinds    |

[PUBLIC_TYPE_SCOPE]: geometry primitives — `OCP.gp`
- rail: cad-kernel geometry

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [CAPABILITY]                         |
| :-----: | :------------ | :---------------- | :----------------------------------- |
|  [01]   | `gp_Pnt`      | 3D point          | Cartesian XYZ coordinate             |
|  [02]   | `gp_Pnt2d`    | 2D point          | UV parameter point                   |
|  [03]   | `gp_Vec`      | 3D vector         | direction plus magnitude             |
|  [04]   | `gp_Dir`      | 3D unit direction | normalized direction vector          |
|  [05]   | `gp_Ax1`      | axis              | point plus direction                 |
|  [06]   | `gp_Ax2`      | coordinate frame  | point plus normal plus X direction   |
|  [07]   | `gp_Ax3`      | right-hand frame  | full 3-axis coordinate system        |
|  [08]   | `gp_Trsf`     | affine transform  | rotation, translation, scale, mirror |
|  [09]   | `gp_GTrsf`    | general transform | non-orthogonal affine transform      |
|  [10]   | `gp_Pln`      | plane             | infinite plane from axis frame       |
|  [11]   | `gp_Lin`      | infinite line     | point plus direction                 |
|  [12]   | `gp_Circ`     | 3D circle         | center plus radius plus normal       |
|  [13]   | `gp_Sphere`   | sphere surface    | center plus radius                   |
|  [14]   | `gp_Cylinder` | cylinder surface  | axis plus radius                     |
|  [15]   | `gp_Cone`     | cone surface      | axis plus half-angle                 |

[PUBLIC_TYPE_SCOPE]: parametric geometry — `OCP.Geom`
- rail: cad-kernel geometry

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [CAPABILITY]                      |
| :-----: | :-------------------- | :--------------- | :-------------------------------- |
|  [01]   | `Geom_Curve`          | abstract curve   | parameter-space 3D curve          |
|  [02]   | `Geom_Line`           | line curve       | infinite line from `gp_Lin`       |
|  [03]   | `Geom_BSplineCurve`   | NURBS curve      | knot/pole/weight B-spline curve   |
|  [04]   | `Geom_BezierCurve`    | Bezier curve     | rational Bezier curve             |
|  [05]   | `Geom_Surface`        | abstract surface | UV-parameter 3D surface           |
|  [06]   | `Geom_BSplineSurface` | NURBS surface    | knot/pole/weight B-spline surface |
|  [07]   | `Geom_BezierSurface`  | Bezier surface   | rational Bezier surface           |
|  [08]   | `Geom_Plane`          | plane surface    | infinite plane geometry           |
|  [09]   | `Geom_CartesianPoint` | point geometry   | `gp_Pnt` in geometry hierarchy    |

[PUBLIC_TYPE_SCOPE]: curve fitting — `OCP.GeomAPI` / `OCP.TColgp`
- rail: cad-kernel geometry construction

`GeomAPI_PointsToBSpline((points: TColgp_Array1OfPnt)).Curve()` yields a `Geom_BSplineCurve` fed to `BRepBuilderAPI_MakeEdge(curve).Edge()`; `TColgp_Array1OfPnt` is the one-based point array.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :------------------------ | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `TColgp_Array1OfPnt`      | point array   | one-based `(lower, upper)` ctor; `SetValue(i, gp_Pnt)` / `Value(i)` accessors |
|  [02]   | `GeomAPI_PointsToBSpline` | curve fitter  | B-spline fit from a point array; `.Curve() -> Geom_BSplineCurve`              |

[PUBLIC_TYPE_SCOPE]: shape builders — `OCP.BRepBuilderAPI`
- rail: cad-kernel shape construction

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :---------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `BRepBuilderAPI_MakeVertex`   | vertex builder  | from `gp_Pnt`                        |
|  [02]   | `BRepBuilderAPI_MakeEdge`     | edge builder    | from line/circle/curve plus vertices |
|  [03]   | `BRepBuilderAPI_MakeWire`     | wire builder    | from edges or wires                  |
|  [04]   | `BRepBuilderAPI_MakePolygon`  | polygon builder | closed polyline wire from points     |
|  [05]   | `BRepBuilderAPI_MakeFace`     | face builder    | from surface/wire/plane              |
|  [06]   | `BRepBuilderAPI_MakeSolid`    | solid builder   | from shells                          |
|  [07]   | `BRepBuilderAPI_Sewing`       | sewing          | join open shells into closed solids  |
|  [08]   | `BRepBuilderAPI_Transform`    | transform       | apply `gp_Trsf` to a shape           |
|  [09]   | `BRepBuilderAPI_NurbsConvert` | conversion      | convert shape geometry to NURBS      |
|  [10]   | `BRepBuilderAPI_Copy`         | copy            | deep copy shape with optional mesh   |

[PUBLIC_TYPE_SCOPE]: primitive and Boolean operations — `OCP.BRepPrimAPI` / `OCP.BRepAlgoAPI` / `OCP.BRepOffsetAPI`
- rail: cad-kernel modeling

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]      | [CAPABILITY]                                                           |
| :-----: | :------------------------------------- | :----------------- | :--------------------------------------------------------------------- |
|  [01]   | `BRepPrimAPI_MakeBox`                  | box                | aligned or oriented box                                                |
|  [02]   | `BRepPrimAPI_MakeSphere`               | sphere             | full or partial sphere                                                 |
|  [03]   | `BRepPrimAPI_MakeCylinder`             | cylinder           | full or partial cylinder                                               |
|  [04]   | `BRepPrimAPI_MakeCone`                 | cone               | full or partial cone                                                   |
|  [05]   | `BRepPrimAPI_MakeTorus`                | torus              | full or partial torus                                                  |
|  [06]   | `BRepPrimAPI_MakePrism`                | extrusion          | linear extrusion along vector                                          |
|  [07]   | `BRepPrimAPI_MakeRevol`                | revolution         | revolution around an axis                                              |
|  [08]   | `BRepAlgoAPI_Fuse`                     | Boolean union      | unite two or more shapes                                               |
|  [09]   | `BRepAlgoAPI_Cut`                      | Boolean difference | subtract tool from object                                              |
|  [10]   | `BRepAlgoAPI_Common`                   | Boolean isect      | intersect two shapes                                                   |
|  [11]   | `BRepAlgoAPI_Section`                  | Boolean section    | cross-section wire/edge result                                         |
|  [12]   | `BRepAlgoAPI_Splitter`                 | shape splitter     | split shape by tool shapes                                             |
|  [13]   | `BRepOffsetAPI_MakeThickSolid`         | shell              | hollow solid via `MakeThickSolidByJoin`/`BySimple` (no-arg ctor)       |
|  [14]   | `BRepOffsetAPI_ThruSections`           | loft               | surface through cross-sections                                         |
|  [15]   | `BRepOffsetAPI_MakePipeShell`          | sweep              | evolving section along a spine                                         |
|  [16]   | `BRepFilletAPI_MakeFillet`             | fillet             | rolling-ball fillet on selected edges                                  |
|  [17]   | `BRepFilletAPI_MakeChamfer`            | chamfer            | chamfer on selected edges                                              |
|  [18]   | `TopTools_ListOfShape`                 | shape collection   | `Append`/`Prepend`/`Extent`/`First` list for `SetArguments`/`SetTools` |
|  [19]   | `BRepOffset_Mode` / `GeomAbs_JoinType` | enums              | offset-construction mode and join type for thick-solid/offset ops      |

[PUBLIC_TYPE_SCOPE]: exchange and XCAF document — `OCP.STEPControl` / `OCP.IGESControl` / `OCP.STEPCAFControl` / `OCP.XCAFDoc` / `OCP.TDocStd`
- rail: cad-kernel data exchange

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :-------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `STEPControl_Reader`        | STEP reader    | shape-only STEP read into topology            |
|  [02]   | `STEPControl_Writer`        | STEP writer    | shape-only STEP write                         |
|  [03]   | `STEPControl_StepModelType` | enum           | `AsIs`/`ManifoldSolidBrep`/… write schema     |
|  [04]   | `IGESControl_Reader`        | IGES reader    | shape-only IGES read                          |
|  [05]   | `IGESControl_Writer`        | IGES writer    | shape-only IGES write                         |
|  [06]   | `STEPCAFControl_Reader`     | STEP CAF read  | assembly, color, name, layer, GD&T transfer   |
|  [07]   | `STEPCAFControl_Writer`     | STEP CAF write | assembly plus metadata write                  |
|  [08]   | `IGESCAFControl_Reader`     | IGES CAF read  | IGES read with color/name/layer modes         |
|  [09]   | `TDocStd_Document`          | XCAF document  | OCAF document root for the assembly tree      |
|  [10]   | `XCAFDoc_DocumentTool`      | XCAF tools     | `ShapeTool_s`/`ColorTool_s` label-tree access |

[PUBLIC_TYPE_SCOPE]: triangulation and mesh export — `OCP.BRepMesh` / `OCP.StlAPI` / `OCP.RWGltf` / `OCP.BRepGProp` / `OCP.GProp`
- rail: cad-kernel tessellation and properties

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                               |
| :-----: | :------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `BRepMesh_IncrementalMesh` | mesher         | deflection-bounded shape triangulation     |
|  [02]   | `StlAPI_Writer`            | STL writer     | write triangulated shape to STL            |
|  [03]   | `StlAPI_Reader`            | STL reader     | read STL into a shape                      |
|  [04]   | `RWGltf_CafWriter`         | glTF writer    | write XCAF document to glTF/GLB            |
|  [05]   | `RWGltf_CafReader`         | glTF reader    | read glTF/GLB into an XCAF document        |
|  [06]   | `BRepGProp`                | property tool  | `VolumeProperties_s`/`SurfaceProperties_s` |
|  [07]   | `GProp_GProps`             | property accum | mass, centroid, and moments accumulator    |

[PUBLIC_TYPE_SCOPE]: triangulation read-back — `OCP.BRep` / `OCP.Poly` / `OCP.TopLoc`
- rail: cad-kernel tessellation read

After `BRepMesh_IncrementalMesh` stores a shape's triangulation, the per-face mesh reads back into arrays through `BRep_Tool.Triangulation_s` for the `numpy`/`trimesh` handoff; a face carrying no stored triangulation returns `None`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                                   |
| :-----: | :------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `BRep_Tool`          | topology tool   | static `Triangulation_s(face, loc) -> Poly_Triangulation \| None`              |
|  [02]   | `Poly_Triangulation` | mesh container  | one-based `NbNodes()`/`NbTriangles()`, `Node(i)`, `Triangle(i)`                |
|  [03]   | `Poly_Triangle`      | triangle record | `Value(i) -> int` one-based node index for `i` in 1..3                         |
|  [04]   | `TopLoc_Location`    | shape placement | default `TopLoc_Location()` is the out-location; `Transformation() -> gp_Trsf` |

[PUBLIC_TYPE_SCOPE]: supporting types — `OCP.XCAFApp` / `OCP.TDF` / `OCP.TCollection` / `OCP.Message` / `OCP.TColStd`
- rail: cad-kernel exchange and document scaffolding

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]    | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `XCAFApp_Application`                  | OCAF application | static `GetApplication_s()`; `InitDocument(doc)` scaffolds the XCAF document |
|  [02]   | `TDF_LabelSequence`                    | label sequence   | one-based `Value(i)` / `Length()`; the `GetFreeShapes` out-parameter carrier |
|  [03]   | `TCollection_ExtendedString`           | UTF-16 string    | the required `TDocStd_Document` ctor storage-format arg                      |
|  [04]   | `TCollection_AsciiString`              | ASCII string     | the `RWGltf_CafWriter` file-path arg (a bare `str` also marshals)            |
|  [05]   | `Message_ProgressRange`                | progress range   | the writer/mesh progress argument                                            |
|  [06]   | `TColStd_IndexedDataMapOfStringString` | string map       | the `RWGltf_CafWriter.Perform` `fileInfo` glTF-metadata map                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: STEP exchange (shape-only)
- rail: cad-kernel data exchange

`STEPControl_Reader` reads geometry without assembly metadata; the read status is an `IFSelect_ReturnStatus` enum and roots transfer one-by-one or in bulk.

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------ |
|  [01]   | `STEPControl_Reader.ReadFile(path) -> IFSelect_ReturnStatus`        | reader         | load STEP file into the model         |
|  [02]   | `STEPControl_Reader.TransferRoots() -> int`                         | transfer       | transfer all roots to topology        |
|  [03]   | `STEPControl_Reader.TransferRoot(n) -> bool`                        | transfer       | transfer the nth root                 |
|  [04]   | `STEPControl_Reader.NbShapes() -> int`                              | query          | transferred shape count               |
|  [05]   | `STEPControl_Reader.OneShape() -> TopoDS_Shape`                     | result         | all transferred roots as one compound |
|  [06]   | `STEPControl_Reader.Shape(n) -> TopoDS_Shape`                       | result         | the nth transferred shape             |
|  [07]   | `STEPControl_Writer.Transfer(shape, mode) -> IFSelect_ReturnStatus` | writer         | convert shape under a `StepModelType` |
|  [08]   | `STEPControl_Writer.Write(path) -> IFSelect_ReturnStatus`           | writer         | flush STEP file to disk               |

[ENTRYPOINT_SCOPE]: STEP XCAF exchange (assembly, color, name)
- rail: cad-kernel data exchange

`STEPCAFControl_Reader` populates a `TDocStd_Document`; the label tree is read through `XCAFDoc_DocumentTool` static accessors. Mode toggles select which metadata transfers. The document itself is constructed with a `TCollection_ExtendedString` storage format — `TDocStd_Document(TCollection_ExtendedString("MDTV-XCAF"))` — and scaffolded by `XCAFApp_Application.GetApplication_s().InitDocument(doc)` before any transfer (an `AsciiString` or bare `str` ctor arg raises `TypeError`).

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `STEPCAFControl_Reader.ReadFile(path) -> IFSelect_ReturnStatus` | reader         | load STEP into the CAF reader                     |
|  [02]   | `SetColorMode` / `SetNameMode` / `SetLayerMode` / `SetGDTMode`  | config         | color/name/layer/GD&T channels (`Set*Mode(bool)`) |
|  [03]   | `SetMatMode` / `SetViewMode` / `SetPropsMode`                   | config         | material/view/validation-props channels           |
|  [04]   | `STEPCAFControl_Reader.Transfer(doc) -> bool`                   | transfer       | populate the XCAF document                        |
|  [05]   | `STEPCAFControl_Reader.Perform(path, doc) -> bool`              | transfer       | read plus transfer in one call                    |
|  [06]   | `XCAFApp_Application.GetApplication_s().InitDocument(doc)`      | document init  | scaffold the OCAF document before transfer        |
|  [07]   | `XCAFDoc_DocumentTool.ShapeTool_s(label) -> XCAFDoc_ShapeTool`  | accessor       | assembly/instance label tree                      |
|  [08]   | `XCAFDoc_DocumentTool.ColorTool_s(label) -> XCAFDoc_ColorTool`  | accessor       | per-shape color labels                            |
|  [09]   | `STEPCAFControl_Writer.Transfer(doc, mode) -> bool`             | writer         | stage XCAF document for write                     |
|  [10]   | `STEPCAFControl_Writer.Write(path) -> IFSelect_ReturnStatus`    | writer         | write STEP with assembly metadata                 |

[ENTRYPOINT_SCOPE]: shape construction and Boolean modeling
- rail: cad-kernel modeling

Every `Make*` builder follows the OCCT command pattern: construct, optionally configure, call `Build()`, check `IsDone()`, then read `Shape()`.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :---------------------------------------- | :------------- | :------------------------- |
|  [01]   | `BRepPrimAPI_MakeBox(dx, dy, dz).Shape()` | box factory    | axis-aligned box solid     |
|  [02]   | `BRepPrimAPI_MakeBox(P1, P2).Shape()`     | box factory    | corner-point box solid     |
|  [03]   | `BRepPrimAPI_MakeSphere(R).Shape()`       | sphere factory | full sphere solid          |
|  [04]   | `BRepPrimAPI_MakeCylinder(R, H).Shape()`  | cylinder       | full cylinder solid        |
|  [05]   | `BRepBuilderAPI_MakeEdge(curve).Edge()`   | edge factory   | edge from a `Geom_Curve`   |
|  [06]   | `BRepBuilderAPI_MakeFace(wire).Face()`    | face factory   | face from a closed wire    |
|  [07]   | `BRepAlgoAPI_Fuse(S1, S2).Shape()`        | Boolean union  | union of two shapes        |
|  [08]   | `BRepAlgoAPI_Cut(S1, S2).Shape()`         | Boolean diff   | difference of two shapes   |
|  [09]   | `BRepAlgoAPI_Common(S1, S2).Shape()`      | Boolean isect  | intersection of two shapes |

[ENTRYPOINT_SCOPE]: n-ary Boolean, fillet/chamfer, and thick-solid construction
- rail: cad-kernel modeling

The Boolean operators expose the full BOPAlgo surface beyond the binary `(S1, S2)` ctor: feed `TopTools_ListOfShape` collections to `SetArguments`/`SetTools` for n-ary operations, set `SetFuzzyValue`/`SetRunParallel`/`SetNonDestructive`/`SetGlue` before `Build()`, and read `SectionEdges()`/`History()`/`HasModified`/`HasGenerated`/`HasDeleted` after. `MakeThickSolid` is a no-arg-constructed builder driven by a method, not a 4-arg ctor; the faces-to-remove argument is a real `TopTools_ListOfShape` (an empty list yields a uniform offset shell). Fillet/chamfer add edges before `Build()`.

- call: n-ary Boolean — `op.SetArguments(TopTools_ListOfShape)` / `op.SetTools(...)` then `op.Build()` / `op.Shape()`; config via `op.SetFuzzyValue(tol)` / `SetRunParallel(flag)` / `SetNonDestructive(flag)` / `SetGlue(mode)`; history via `op.SectionEdges()` / `op.History()` / `HasModified()` / `HasGenerated()`
- call: thick-solid — `mts = BRepOffsetAPI_MakeThickSolid(); mts.MakeThickSolidByJoin(S, ClosingFaces: TopTools_ListOfShape, Offset, Tol, Mode=BRepOffset_Skin, ...)` then `mts.Shape()`; `mts.MakeThickSolidBySimple(S, Offset)`
- call: fillet/chamfer/loft — `BRepFilletAPI_MakeFillet(shape).Add(radius, edge)`; `BRepFilletAPI_MakeChamfer(shape).Add(dist, edge)`; `BRepOffsetAPI_ThruSections(isSolid).AddWire(wire)` then `.Build()` / `.Shape()`

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `BRepAlgoAPI_*.SetArguments` / `SetTools`                            | n-ary Boolean  | union/cut/common over shape lists          |
|  [02]   | `SetFuzzyValue` / `SetRunParallel` / `SetNonDestructive` / `SetGlue` | config         | tolerance, parallelism, non-destruct, glue |
|  [03]   | `SectionEdges` / `History` / `HasModified` / `HasGenerated`          | history        | section edges and modification history     |
|  [04]   | `BRepOffsetAPI_MakeThickSolid.MakeThickSolidByJoin`                  | shell          | hollow solid; empty list = uniform shell   |
|  [05]   | `MakeThickSolidBySimple`                                             | shell          | simple uniform-offset shell                |
|  [06]   | `BRepFilletAPI_MakeFillet.Add`                                       | fillet         | rolling-ball fillet on added edges         |
|  [07]   | `BRepFilletAPI_MakeChamfer.Add`                                      | chamfer        | chamfer on added edges                     |
|  [08]   | `BRepOffsetAPI_ThruSections.AddWire`                                 | loft           | lofted surface/solid through wires         |

[ENTRYPOINT_SCOPE]: traversal, triangulation, and properties
- rail: cad-kernel topology and tessellation

`TopExp_Explorer` is the polymorphic sub-shape iterator; `BRepMesh_IncrementalMesh` mutates a shape's triangulation in place before mesh export or glTF write. The mesher's load-bearing overload is the 5-arg `(shape, linDeflection, isRelative=False, angDeflection=0.5, isInParallel=False)`; the 2-arg `(shape, deflection)` is positional sugar over the same overload. `RWGltf_CafWriter.Perform` has NO `(doc, progress)` 2-arg overload — the minimal call is the 3-arg `Perform(doc, fileInfo, progress)` where `fileInfo` is a `TColStd_IndexedDataMapOfStringString` (an empty map returns `True`).

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `TopExp_Explorer(shape, TopAbs_ShapeEnum)`    | explorer       | iterate sub-shapes by kind via `.More()`/`.Current()`/`.Next()` |
|  [02]   | `TopExp.MapShapes_s(shape, type, map)`        | map builder    | index all sub-shapes of a type                                  |
|  [03]   | `TopoDS.Edge_s(shape) -> TopoDS_Edge`         | downcast       | safe static downcast to edge                                    |
|  [04]   | `BRepMesh_IncrementalMesh`                    | mesher         | build the shape triangulation (5-arg overload; 2-arg is sugar)  |
|  [05]   | `StlAPI_Writer().Write(shape, path)`          | export         | write triangulated shape to STL                                 |
|  [06]   | `RWGltf_CafWriter.Perform`                    | export         | write XCAF document to glTF/GLB                                 |
|  [07]   | `BRepGProp.VolumeProperties_s(shape, gprops)` | property       | accumulate mass, centroid, moments                              |

## [04]-[IMPLEMENTATION_LAW]

[OCP_TOPOLOGY]:
- import each module from the flat `OCP` namespace: `from OCP.TopoDS import TopoDS_Shape`, `from OCP.STEPCAFControl import STEPCAFControl_Reader`; there is no intermediate `Core` admission as in the SWIG `OCC.Core.*` layout, and module-level import stays at boundary scope per the manifest import policy.
- shape hierarchy: `TopoDS_Vertex` < `TopoDS_Edge` < `TopoDS_Wire` < `TopoDS_Face` < `TopoDS_Shell` < `TopoDS_Solid` < `TopoDS_CompSolid`/`TopoDS_Compound`; `TopAbs_ShapeEnum` enumerates the same kinds for `TopExp_Explorer`.
- downcast: static `TopoDS.Edge_s`/`Face_s`/`Vertex_s`/`Wire_s` cast a `TopoDS_Shape` to the concrete type before type-specific member access; a wrong-type cast raises.
- builder pattern: every `Make*` builder constructs, configures, calls `Build()`, checks `IsDone()`, and yields `Shape()` or a typed accessor such as `Edge()`/`Face()`/`Solid()`; results carry `Generated`/`Modified`/`IsDeleted` for downstream history queries.
- Boolean/offset detail: `BRepAlgoAPI_*` are n-ary BOPAlgo operators — the binary `(S1, S2)` ctor is sugar over `SetArguments`/`SetTools` taking `TopTools_ListOfShape`; `BRepOffsetAPI_MakeThickSolid` has a no-arg ctor and is driven by `MakeThickSolidByJoin(S, ClosingFaces: TopTools_ListOfShape, Offset, Tol, Mode=BRepOffset_Skin, ...)` or `MakeThickSolidBySimple(S, Offset)`, never a 4-positional-arg ctor; pass an empty `TopTools_ListOfShape` (or empty Python list, which pybind11 marshals) for a uniform offset shell. Fillet/chamfer (`BRepFilletAPI_MakeFillet`/`MakeChamfer`) add edges via `Add(...)` before `Build()`.

[STEP_BRIDGE]:
- assembly path: the `STEPCAFControl_Reader` plus `TDocStd_Document` plus `XCAFDoc_DocumentTool` triad is the load-bearing STEP entry; it preserves the assembly instance tree, per-shape colors, names, layers, GD&T, materials, and validation props that the shape-only `STEPControl_Reader` discards, and the `SetColorMode`/`SetNameMode`/`SetLayerMode`/`SetGDTMode`/`SetMatMode`/`SetViewMode`/`SetPropsMode` toggles select which metadata channels transfer. Construct the document as `TDocStd_Document(TCollection_ExtendedString("MDTV-XCAF"))` then scaffold it with `XCAFApp_Application.GetApplication_s().InitDocument(doc)` before `Transfer` — the `ExtendedString` storage-format arg is required (`AsciiString` and bare `str` raise `TypeError`). The label tree is read through static `XCAFDoc_DocumentTool.ShapeTool_s(label)`/`ColorTool_s(label)`; `XCAFDoc_ShapeTool.GetFreeShapes(TDF_LabelSequence)`/`GetShapes`/`GetShape_s`/`GetReferredShape_s`/`GetLocation_s` walk the assembly instances and locations, the `TDF_LabelSequence` filled as an out-parameter and read one-based via `Value(i)`/`Length()`.
- exchange status: STEP/IGES read and write return `IFSelect_ReturnStatus` (`IFSelect_RetVoid`/`RetDone`/`RetError`/`RetFail`/`RetStop`); check the status equals `IFSelect_RetDone` before consuming `OneShape()` or `Shape(n)`, and read the pre-transfer root count from `NbRootsForTransfer()`.
- tessellation: `BRepMesh_IncrementalMesh(shape, deflection, isRelative=False, angle=0.5, parallel=False)` mutates the shape's stored triangulation in place; `RWGltf_CafWriter` and `StlAPI_Writer` consume that triangulation, so meshing precedes glTF/STL export. The minimal glTF write is `RWGltf_CafWriter(path, binary).Perform(doc, fileInfo, progress)` with `fileInfo` an empty `TColStd_IndexedDataMapOfStringString` (the 3-arg overload; there is no 2-arg `Perform(doc, progress)`).
- evidence: `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` populate a `GProp_GProps` accumulator exposing `Mass`/`CentreOfMass`/`MatrixOfInertia` as the geometric receipt for a transferred solid.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `cadquery-ocp`
- Owns: OCCT BREP topology, parametric `gp`/`Geom` geometry, primitive/feature/Boolean/fillet/chamfer/thick-solid shape construction, mesh triangulation, STEP/IGES exchange, and the XCAF assembly/color/name/material document model for the STEP bridge
- Accept: STEP/IGES source bytes for the B-rep hop; `TopoDS_Shape` topology feeding the tessellation and mesh-export owners; `TopTools_ListOfShape` collections for the n-ary Boolean and thick-solid arms
- Reject: a hand-rolled STEP/IGES parser, B-rep topology, Boolean kernel, fillet/offset algebra, or triangulator where OCP is admitted; wrapper-renames of `STEPCAFControl_Reader`/`BRepMesh_IncrementalMesh`; shape-only `STEPControl_Reader` where the assembly/color/name metadata is required; the retired conda-only `pythonocc-core` `OCC.Core.*` path; a bare-4-arg `MakeThickSolid(...)` ctor assumption (the operation is `MakeThickSolidByJoin`/`BySimple`)

[CAPTURE_GAP]:
