# [PY_CAD_API_CADQUERY_OCP]

`cadquery-ocp` binds the OpenCASCADE Technology (OCCT) B-rep modeling kernel to Python as flat `OCP.*` submodules: BREP topology, `gp`/`Geom` geometry, primitive/feature/Boolean/fillet/offset shape construction, mesh triangulation, STEP/IGES exchange, and the XCAF assembly/color/name/material document model. It is the sole `OCP` path this estate admits, and `CadService` composes its operations behind the generated boundary, so a peer reaches this kernel through typed requests rather than an import of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cadquery-ocp`
- package: `cadquery-ocp` (Apache-2.0)
- module: `OCP`
- namespaces: `OCP.{TopoDS,TopAbs,TopExp,TopTools,gp,Geom,GeomAPI,GC,BRep,BRepAlgoAPI,BRepBuilderAPI,BRepPrimAPI,BRepOffsetAPI,BRepFilletAPI,BRepGProp,BRepMesh,BRepCheck,BOPAlgo,ShapeFix,STEPControl,STEPCAFControl,IGESControl,IGESCAFControl,APIHeaderSection,StepBasic,StepData,IFSelect,Interface,RWGltf,XCAFApp,XCAFDoc,TDocStd,TDF,TCollection,TColStd,TColgp,Precision,Message}`
- abi: CPython C-extension over the OCCT C++ libraries; the Forge python-overlay `.pth` supplies `OCP` at the interpreter floor, and the import stays worker-side under the one-slot native lane
- rail: generated CadService provider kernel
- capability: OCCT BREP topology, parametric `gp`/`Geom` geometry, primitive/feature/Boolean/offset/fillet/chamfer shape construction, n-ary Boolean over `TopTools_ListOfShape`, mesh triangulation, STEP/IGES exchange with file-local STEP header access, and the XCAF document model carrying assembly, color, name, layer, and material

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: topology family — `OCP.TopoDS` / `OCP.TopAbs`

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

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `TColgp_HArray1OfPnt` | point array   | one-based handle array for interpolation constraints                    |
|  [02]   | `GeomAPI_Interpolate` | interpolator  | periodic closed B-spline through points; `Perform` / `IsDone` / `Curve` |

[PUBLIC_TYPE_SCOPE]: curve, face, and area construction — `OCP.GC` / `OCP.ShapeFix` / `OCP.BOPAlgo`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `GC_MakeArcOfCircle`  | curve builder | circular arc through start, interior, and end points        |
|  [02]   | `ShapeFix_Face`       | face repair   | boundary-orientation repair through `FixOrientation` only   |
|  [03]   | `BOPAlgo_BuilderFace` | area builder  | rebuild valid face areas from offset edges on one base face |

[PUBLIC_TYPE_SCOPE]: shape builders — `OCP.BRepBuilderAPI`

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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [CAPABILITY]                                                           |
| :-----: | :---------------------------- | :----------------- | :--------------------------------------------------------------------- |
|  [01]   | `BRepPrimAPI_MakeBox`         | box                | aligned or oriented box                                                |
|  [02]   | `BRepPrimAPI_MakeSphere`      | sphere             | full or partial sphere                                                 |
|  [03]   | `BRepPrimAPI_MakeCylinder`    | cylinder           | full or partial cylinder                                               |
|  [04]   | `BRepPrimAPI_MakeCone`        | cone               | full or partial cone                                                   |
|  [05]   | `BRepPrimAPI_MakeTorus`       | torus              | full or partial torus                                                  |
|  [06]   | `BRepPrimAPI_MakePrism`       | extrusion          | linear extrusion along vector                                          |
|  [07]   | `BRepPrimAPI_MakeRevol`       | revolution         | revolution around an axis                                              |
|  [08]   | `BRepAlgoAPI_Fuse`            | Boolean union      | unite two or more shapes                                               |
|  [09]   | `BRepAlgoAPI_Cut`             | Boolean difference | subtract tool from object                                              |
|  [10]   | `BRepAlgoAPI_Common`          | Boolean isect      | intersect two shapes                                                   |
|  [11]   | `BRepAlgoAPI_Section`         | Boolean section    | cross-section wire/edge result                                         |
|  [12]   | `BRepAlgoAPI_Splitter`        | shape splitter     | split shape by tool shapes                                             |
|  [13]   | `BRepOffsetAPI_MakeOffset`    | planar offset      | exact face-boundary offset with fixed arc join and no approximation    |
|  [14]   | `BRepOffsetAPI_ThruSections`  | loft               | surface through cross-sections                                         |
|  [15]   | `BRepOffsetAPI_MakePipeShell` | sweep              | evolving section along a spine                                         |
|  [16]   | `BRepFilletAPI_MakeFillet`    | fillet             | rolling-ball fillet on selected edges                                  |
|  [17]   | `BRepFilletAPI_MakeChamfer`   | chamfer            | chamfer on selected edges                                              |
|  [18]   | `TopTools_ListOfShape`        | shape collection   | `Append`/`Prepend`/`Extent`/`First` list for `SetArguments`/`SetTools` |
|  [19]   | `GeomAbs_JoinType`            | offset policy      | internal `GeomAbs_Arc` selection; not a public contract vocabulary     |

[PUBLIC_TYPE_SCOPE]: exchange and XCAF document — `OCP.STEPControl` / `OCP.IGESControl` / `OCP.STEPCAFControl` / `OCP.XCAFDoc` / `OCP.TDocStd`

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                         |
| :-----: | :---------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `STEPControl_Reader`          | STEP reader    | shape-only STEP read into topology                   |
|  [02]   | `STEPControl_Writer`          | STEP writer    | shape-only STEP write                                |
|  [03]   | `STEPControl_StepModelType`   | enum           | `AsIs`/`ManifoldSolidBrep`/… write schema            |
|  [04]   | `IGESControl_Reader`          | IGES reader    | shape-only IGES read                                 |
|  [05]   | `IGESControl_Writer`          | IGES writer    | shape-only IGES write                                |
|  [06]   | `STEPCAFControl_Reader`       | STEP CAF read  | assembly, color, name, layer, GD&T transfer          |
|  [07]   | `STEPCAFControl_Writer`       | STEP CAF write | assembly plus metadata write                         |
|  [08]   | `IGESCAFControl_Reader`       | IGES CAF read  | IGES read with color/name/layer modes                |
|  [09]   | `TDocStd_Document`            | XCAF document  | OCAF document root for the assembly tree             |
|  [10]   | `XCAFDoc_DocumentTool`        | XCAF tools     | `ShapeTool_s`/`ColorTool_s` label-tree access        |
|  [11]   | `APIHeaderSection_MakeHeader` | STEP header    | file-local schema read and canonical timestamp write |
|  [12]   | `StepBasic_Product`           | STEP product   | canonical product identity, name, and description    |
|  [13]   | `TCollection_HAsciiString`    | STEP string    | bound string carrier for header and product metadata |

[PUBLIC_TYPE_SCOPE]: triangulation and mesh export — `OCP.BRepMesh` / `OCP.StlAPI` / `OCP.RWGltf` / `OCP.BRepGProp` / `OCP.GProp`

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

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                                   |
| :-----: | :------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `BRep_Tool`          | topology tool   | static `Triangulation_s(face, loc) -> Poly_Triangulation \| None`              |
|  [02]   | `Poly_Triangulation` | mesh container  | one-based `NbNodes()`/`NbTriangles()`, `Node(i)`, `Triangle(i)`                |
|  [03]   | `Poly_Triangle`      | triangle record | `Value(i) -> int` one-based node index for `i` in 1..3                         |
|  [04]   | `TopLoc_Location`    | shape placement | default `TopLoc_Location()` is the out-location; `Transformation() -> gp_Trsf` |

[PUBLIC_TYPE_SCOPE]: supporting types — `OCP.XCAFApp` / `OCP.TDF` / `OCP.TCollection` / `OCP.Message` / `OCP.TColStd`

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]    | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `XCAFApp_Application`                  | OCAF application | static `GetApplication_s()`; `InitDocument(doc)` scaffolds the XCAF document |
|  [02]   | `TDF_LabelSequence`                    | label sequence   | one-based `Value(i)` / `Length()`; the `GetFreeShapes` out-parameter carrier |
|  [03]   | `TCollection_ExtendedString`           | UTF-16 string    | the required `TDocStd_Document` ctor storage-format arg                      |
|  [04]   | `TCollection_AsciiString`              | ASCII string     | the `RWGltf_CafWriter` file-path arg (a bare `str` also marshals)            |
|  [05]   | `Message_ProgressRange`                | progress range   | the writer/mesh progress argument                                            |
|  [06]   | `TColStd_IndexedDataMapOfStringString` | string map       | the `RWGltf_CafWriter.Perform` `fileInfo` glTF-metadata map                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: STEP exchange (shape-only) — no assembly metadata; read status is `IFSelect_ReturnStatus`

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

[ENTRYPOINT_SCOPE]: STEP XCAF exchange (assembly, color, name) — `Set*Mode(bool)` toggles select transferred channels

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
|  [11]   | `STEPCAFControl_Reader.Reader().StepModel()`                    | model          | parsed file model passed to the header accessor   |
|  [12]   | `XCAFDoc_ShapeTool.GetFreeShapes(sequence)`                     | accessor       | fill a label sequence with the document roots     |
|  [13]   | `XCAFDoc_ShapeTool.GetShape_s(label) -> TopoDS_Shape`           | accessor       | shape at a label, component locations applied     |
|  [14]   | `XCAFDoc_ShapeTool.GetReferredShape_s(label, referred)`         | accessor       | resolve an instance label to its prototype        |
|  [15]   | `XCAFDoc_ShapeTool.GetLocation_s(label) -> TopLoc_Location`     | accessor       | placement carried by one instance label           |
|  [16]   | `TDF_LabelSequence.Length()` / `.Value(i) -> TDF_Label`         | sequence       | one-based label roster filled as an out-parameter |
|  [17]   | `APIHeaderSection_MakeHeader(model).HasFs()`                    | header         | whether the parsed model carries `FILE_SCHEMA`    |
|  [18]   | `NbSchemaIdentifiers()` / `SchemaIdentifiersValue(i)`           | header         | one-based schema identifier strings               |
|  [19]   | `SetTimeStamp` / `SetAuthorValue` / `SetOrganizationValue`      | header         | overwrite emission-time and authorship identity   |
|  [20]   | `SetOriginatingSystem` / `SetPreprocessorVersion`               | header         | overwrite toolchain identity carried in the file  |
|  [21]   | `StepBasic_Product.SetId/SetName/SetDescription`                | product        | normalize process counters and mutable labels     |

[ENTRYPOINT_SCOPE]: process-global exchange configuration — set once per process before any reader, writer, or export

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `STEPControl_Controller.Init_s() -> bool`         | controller     | register the STEP protocol before any read |
|  [02]   | `IGESControl_Controller.Init_s() -> bool`         | controller     | register the IGES protocol before any read |
|  [03]   | `Interface_Static.SetCVal_s(name, value) -> bool` | config         | set one process-global string parameter    |
|  [04]   | `Interface_Static.CVal_s(name) -> str`            | config         | read one string parameter back for proof   |
|  [05]   | `Interface_Static.SetIVal_s(name, value) -> bool` | config         | set one process-global integer parameter   |
|  [06]   | `Interface_Static.IVal_s(name) -> int`            | config         | read one integer parameter back for proof  |

[ENTRYPOINT_SCOPE]: shape construction and Boolean modeling — construct, `Build()`, `IsDone()`, then `Shape()`/typed accessor

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

[ENTRYPOINT_SCOPE]: n-ary Boolean, profile offset, fillet/chamfer, and loft construction

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `BRepAlgoAPI_*.SetArguments` / `SetTools`                            | n-ary Boolean  | union/cut/common over shape lists            |
|  [02]   | `SetFuzzyValue` / `SetRunParallel` / `SetNonDestructive` / `SetGlue` | config         | tolerance, parallelism, non-destruct, glue   |
|  [03]   | `SectionEdges` / `History` / `HasModified` / `HasGenerated`          | history        | section edges and modification history       |
|  [04]   | `BRepOffsetAPI_MakeOffset(face, GeomAbs_Arc, false)`                 | profile offset | offset the oriented outer and holes together |
|  [05]   | `SetApprox(false)` / `Perform(distance, 0)`                          | profile offset | preserve line/circle/spline curve ownership  |
|  [06]   | `BOPAlgo_BuilderFace.SetFace` / `SetShapes` / `Areas`                | area rebuild   | partition returned offset edges into faces   |
|  [07]   | `BRepFilletAPI_MakeFillet.Add`                                       | fillet         | rolling-ball fillet on added edges           |
|  [08]   | `BRepFilletAPI_MakeChamfer.Add`                                      | chamfer        | chamfer on added edges                       |
|  [09]   | `BRepOffsetAPI_ThruSections.AddWire` / `GetStatus`                   | loft           | lofted solid and typed failure status        |

[ENTRYPOINT_SCOPE]: traversal, triangulation, and properties

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `TopExp_Explorer(shape, TopAbs_ShapeEnum)`    | explorer       | iterate sub-shapes by kind via `.More()`/`.Current()`/`.Next()` |
|  [02]   | `TopExp.MapShapes_s(shape, type, map)`        | map builder    | index all sub-shapes of a type                                  |
|  [03]   | `TopoDS.Edge_s(shape) -> TopoDS_Edge`         | downcast       | safe static downcast to edge                                    |
|  [04]   | `BRepMesh_IncrementalMesh`                    | mesher         | build the shape triangulation (5-arg overload; 2-arg is sugar)  |
|  [05]   | `StlAPI_Writer().Write(shape, path)`          | export         | write triangulated shape to STL                                 |
|  [06]   | `RWGltf_CafWriter.Perform`                    | export         | write XCAF document to glTF/GLB                                 |
|  [07]   | `BRepGProp.VolumeProperties_s(shape, gprops)` | property       | accumulate mass, centroid, moments                              |
|  [08]   | `BRepCheck_Analyzer(shape).IsValid()`         | validity       | exact OCCT topology and geometry validity                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- flat import: each module lands from the `OCP` namespace directly — `from OCP.TopoDS import TopoDS_Shape`, `from OCP.STEPCAFControl import STEPCAFControl_Reader` — with no intermediate `OCC.Core.*` layer.
- shape hierarchy: `TopoDS_Vertex` < `TopoDS_Edge` < `TopoDS_Wire` < `TopoDS_Face` < `TopoDS_Shell` < `TopoDS_Solid` < `TopoDS_CompSolid`/`TopoDS_Compound`; static `TopoDS.Edge_s`/`Face_s`/`Vertex_s`/`Wire_s` downcast a `TopoDS_Shape` to the concrete type before member access, a wrong-type cast raising.
- builder pattern: every `Make*` builder constructs, configures, calls `Build()`, checks `IsDone()`, then yields `Shape()` or a typed `Edge()`/`Face()`/`Solid()`; results carry `Generated`/`Modified`/`IsDeleted` for history queries.
- Boolean ops: `BRepAlgoAPI_*` are n-ary BOPAlgo operators — the binary `(S1, S2)` ctor is sugar over `SetArguments`/`SetTools` taking `TopTools_ListOfShape`; set `SetFuzzyValue`/`SetRunParallel`/`SetNonDestructive`/`SetGlue` before `Build()`, read `SectionEdges`/`History`/`HasModified`/`HasGenerated` after.
- profile offset and walls: `BRepOffsetAPI_MakeOffset(face, GeomAbs_Arc, false)` with `SetApprox(false)` preserves line, circle, and spline ownership while offsetting each oriented outer/hole set together. `BOPAlgo_BuilderFace` partitions the returned edges into valid areas on the original face. `ThickOp` extrudes the source and shifted areas and cuts by offset sign. Intersection/tangent joins and approximation are not total for the admitted curved profile and never become wire knobs. Fillet/chamfer add explicitly selected edges before `Build()`.
- XCAF document: construct `TDocStd_Document(TCollection_ExtendedString("MDTV-XCAF"))` then `XCAFApp_Application.GetApplication_s().InitDocument(doc)` before any `Transfer` — an `AsciiString` or bare `str` ctor arg raises `TypeError`. Label tree reads static `XCAFDoc_DocumentTool.ShapeTool_s(label)`/`ColorTool_s(label)`; `XCAFDoc_ShapeTool.GetFreeShapes(TDF_LabelSequence)`/`GetShape_s`/`GetReferredShape_s`/`GetLocation_s` walk instances and locations, the `TDF_LabelSequence` filled as an out-parameter and read one-based via `Value(i)`/`Length()`.
- exchange status: STEP/IGES read/write return `IFSelect_ReturnStatus` — gate `== IFSelect_RetDone` before consuming `OneShape()`/`Shape(n)`, and read the pre-transfer root count from `NbRootsForTransfer()`.
- unit initialization: `STEPControl_Controller.Init_s()` and `IGESControl_Controller.Init_s()` precede the sole process-wide `Interface_Static` owner. It sets and reads back `xstep.cascade.unit=M`, `write.step.unit=M`, and `write.step.schema=5` (AP242DIS) before any reader, writer, property fold, or GLB export. OCCT defaults units to `MM` and keeps writer schema process-global; leaving either implicit breaks metre receipts or idempotent artifact identity.
- exchange precision: STEP and IGES retain file-owned reader precision. User precision, forced maximums, post-transfer `ShapeFix_ShapeTolerance`, and forced `BRepLib.SameParameter_s` are rejected: live IGES proofs changed topology and tessellation non-monotonically or degraded same-parameter evidence. Future healing policy carries its own typed admission contract and receipt; it cannot alias tessellation deflection or IFC precision.
- STEP header: after a successful `STEPCAFControl_Reader.ReadFile`, build `APIHeaderSection_MakeHeader(reader.Reader().StepModel())`; require `HasFs()`, then read the one-based `SchemaIdentifiersValue(i).ToCString()` roster before `Transfer`. Verified `7.9.3.1.1` returns AP203 `CONFIG_CONTROL_DESIGN`, AP214 `AUTOMOTIVE_DESIGN` and `AUTOMOTIVE_DESIGN_CC2`, and AP242 `AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF`. `StepModel().Protocol().SchemaName(model)` is not file authority, and `FsValue()` is unusable because its `HeaderSection_FileSchema` return type is not registered.
- mesh then export: `BRepMesh_IncrementalMesh(shape, deflection, isRelative=False, angDeflection=0.5, isInParallel=False)` mutates the stored triangulation in place; `IsDone()` and zero `GetStatusFlags()` admit the result before export. Its per-call concurrency control is the boolean alone, not a thread-count parameter, so a bounded caller claims whole-lane custody before enabling it. Meshing precedes `StlAPI_Writer`/`RWGltf_CafWriter` export; minimal glTF is the 3-arg `RWGltf_CafWriter(path, binary).Perform(doc, fileInfo, progress)` with `fileInfo` an empty `TColStd_IndexedDataMapOfStringString`.
- properties: `BRepGProp.VolumeProperties_s`/`SurfaceProperties_s` fill a `GProp_GProps` exposing `Mass`/`CentreOfMass`/`MatrixOfInertia` as the transferred solid's geometric receipt.

[STACKING]:
- `trimesh`(`.api/trimesh.md`): a meshed `TopoDS_Face`'s `Poly_Triangulation`, read through `BRep_Tool.Triangulation_s` into one-based `Node(i)`/`Triangle(i)` values buffered as `numpy` arrays, becomes `Trimesh(vertices, faces)` for the mesh owner; OCP holds no mesh file handle of its own.
- within-lib `cad`/`step-bridge` owner: composes `STEPCAFControl_Reader` transfer, `XCAFDoc_DocumentTool` label-tree reads, `BRepMesh_IncrementalMesh` tessellation, and `RWGltf_CafWriter` into the CAD-STEP tessellation hop, meeting the C# `StepIso10303` codec at the wire through content identity, never an import.

[LOCAL_ADMISSION]:
- `cadquery-ocp` is admitted here as the one exact-modeling kernel behind `CadService`; `pythonocc-core`'s rival `OCC.Core.*` tree refuses, and geometry's OCCT reach rides `topologic_core`'s own binding.

[RAIL_LAW]:
- Package: `cadquery-ocp`
- Owns: OCCT BREP topology, parametric `gp`/`Geom` geometry, primitive/feature/Boolean/fillet/chamfer/thick-solid shape construction, mesh triangulation, STEP/IGES exchange and file-local STEP header access, and the XCAF assembly/color/name/material document model for the STEP bridge
- Accept: STEP/IGES source bytes for the B-rep hop; a parsed STEP model for file-local `FILE_SCHEMA`; `TopoDS_Shape` topology feeding the tessellation and mesh-export owners; `TopTools_ListOfShape` collections for the n-ary Boolean and thick-solid arms
- Reject: a hand-rolled STEP/IGES/header parser, `StepModel().Protocol().SchemaName(model)` as file-local AP evidence, `FsValue()` under the unregistered return type, B-rep topology, Boolean kernel, fillet/offset algebra, or triangulator where OCP is admitted; wrapper-renames of `STEPCAFControl_Reader`/`BRepMesh_IncrementalMesh`; shape-only `STEPControl_Reader` where assembly/color/name metadata is required; the `pythonocc-core` `OCC.Core.*` path; a bare-4-arg `MakeThickSolid(...)` ctor assumption where the operation is `MakeThickSolidByJoin`/`BySimple`
