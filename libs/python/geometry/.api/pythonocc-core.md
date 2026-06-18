# [PY_GEOMETRY_API_PYTHONOCC_CORE]

`pythonocc-core` supplies Python bindings to the OpenCASCADE Technology (OCCT) geometric modeling kernel via SWIG: it exposes the full BREP topology hierarchy (`TopoDS`), geometry primitives (`gp`, `Geom`), shape builders (`BRepBuilderAPI`, `BRepPrimAPI`, `BRepOffsetAPI`), Boolean/feature operations (`BRepAlgoAPI`), topology traversal (`TopExp`), and STEP/IGES exchange (`STEPControl`, `IGESControl`) as `OCC.Core.*` submodules.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pythonocc-core`
- package: `pythonocc-core`
- module: `OCC.Core`
- asset: conda-only runtime library (OpenCASCADE SWIG bindings)
- rail: geometry-algebra / cad-kernel

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: topology family — OCC.Core.TopoDS
- rail: cad-kernel topology

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [RAIL]                                      |
| :-----: | :----------------- | :--------------- | :------------------------------------------ |
|   [1]   | `TopoDS_Shape`     | base shape       | orientation, location, topology access      |
|   [2]   | `TopoDS_Vertex`    | 0D shape         | point in topology                           |
|   [3]   | `TopoDS_Edge`      | 1D shape         | bounded curve in topology                   |
|   [4]   | `TopoDS_Wire`      | 1D compound      | connected edge collection                   |
|   [5]   | `TopoDS_Face`      | 2D shape         | bounded surface element                     |
|   [6]   | `TopoDS_Shell`     | 2D compound      | connected face collection                   |
|   [7]   | `TopoDS_Solid`     | 3D shape         | closed volume                               |
|   [8]   | `TopoDS_CompSolid` | 3D compound      | connected solid collection                  |
|   [9]   | `TopoDS_Compound`  | generic compound | heterogeneous shape collection              |
|  [10]   | `TopoDS_Builder`   | shape builder    | `Add`/`Remove`/`Make*` topology mutations   |
|  [11]   | `TopoDS_Iterator`  | traversal        | iterate sub-shapes of a shape               |
|  [12]   | `TopoDS_HShape`    | transient handle | heap-managed shape reference                |
|  [13]   | `topods`           | downcast utility | `Edge`, `Face`, `Vertex`, `Wire`, ... casts |

[PUBLIC_TYPE_SCOPE]: geometry primitives — OCC.Core.gp
- rail: cad-kernel geometry

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]     | [RAIL]                               |
| :-----: | :------------ | :---------------- | :----------------------------------- |
|   [1]   | `gp_Pnt`      | 3D point          | Cartesian XYZ coordinate             |
|   [2]   | `gp_Pnt2d`    | 2D point          | UV parameter point                   |
|   [3]   | `gp_Vec`      | 3D vector         | direction + magnitude                |
|   [4]   | `gp_Vec2d`    | 2D vector         | 2D direction + magnitude             |
|   [5]   | `gp_Dir`      | 3D unit direction | normalized direction vector          |
|   [6]   | `gp_Dir2d`    | 2D unit direction | normalized 2D direction              |
|   [7]   | `gp_Ax1`      | axis              | point + direction                    |
|   [8]   | `gp_Ax2`      | coordinate frame  | point + normal + X direction         |
|   [9]   | `gp_Ax3`      | right-hand frame  | full 3-axis coordinate system        |
|  [10]   | `gp_Trsf`     | affine transform  | rotation, translation, scale, mirror |
|  [11]   | `gp_GTrsf`    | general transform | non-orthogonal affine transform      |
|  [12]   | `gp_Circ`     | 3D circle         | center + radius + normal             |
|  [13]   | `gp_Lin`      | infinite line     | point + direction                    |
|  [14]   | `gp_Cylinder` | cylinder surface  | axis + radius                        |
|  [15]   | `gp_Cone`     | cone surface      | axis + half-angle                    |

[PUBLIC_TYPE_SCOPE]: parametric geometry — OCC.Core.Geom
- rail: cad-kernel geometry

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                            |
| :-----: | :-------------------- | :----------------- | :-------------------------------- |
|   [1]   | `Geom_Curve`          | abstract curve     | parameter-space 3D curve          |
|   [2]   | `Geom_Line`           | line curve         | infinite line from `gp_Lin`       |
|   [3]   | `Geom_BSplineCurve`   | NURBS curve        | knot/pole/weight B-spline curve   |
|   [4]   | `Geom_BezierCurve`    | Bezier curve       | rational Bezier curve             |
|   [5]   | `Geom_OffsetCurve`    | offset curve       | offset from a base curve          |
|   [6]   | `Geom_Surface`        | abstract surface   | UV-parameter 3D surface           |
|   [7]   | `Geom_BSplineSurface` | NURBS surface      | knot/pole/weight B-spline surface |
|   [8]   | `Geom_BezierSurface`  | Bezier surface     | rational Bezier surface           |
|   [9]   | `Geom_OffsetSurface`  | offset surface     | offset from a base surface        |
|  [10]   | `Geom_Transformation` | geometry transform | moves/rotates geometry objects    |
|  [11]   | `Geom_CartesianPoint` | point geometry     | `gp_Pnt` in geometry hierarchy    |

[PUBLIC_TYPE_SCOPE]: shape builders — OCC.Core.BRepBuilderAPI
- rail: cad-kernel shape construction

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------- | :------------ | :------------------------------------- |
|   [1]   | `BRepBuilderAPI_MakeEdge`  | edge builder  | from line/circle/curve + vertices      |
|   [2]   | `BRepBuilderAPI_MakeWire`  | wire builder  | from edges or wires                    |
|   [3]   | `BRepBuilderAPI_MakeFace`  | face builder  | from surface/wire/plane                |
|   [4]   | `BRepBuilderAPI_MakeSolid` | solid builder | from shells                            |
|   [5]   | `BRepBuilderAPI_Sewing`    | sewing        | join open shells into closed solids    |
|   [6]   | `BRepBuilderAPI_Transform` | transform     | apply `gp_Trsf` to a shape             |
|   [7]   | `BRepBuilderAPI_Copy`      | copy          | deep copy shape with optional mesh     |
|   [8]   | `BRepBuilderAPI_MakeShape` | base builder  | `Shape()`, `Generated()`, `Modified()` |

[PUBLIC_TYPE_SCOPE]: primitive builders — OCC.Core.BRepPrimAPI
- rail: cad-kernel primitive construction

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------- | :------------ | :---------------------------- |
|   [1]   | `BRepPrimAPI_MakeBox`        | box           | aligned or axis-oriented box  |
|   [2]   | `BRepPrimAPI_MakeSphere`     | sphere        | full or partial sphere        |
|   [3]   | `BRepPrimAPI_MakeCylinder`   | cylinder      | full or partial cylinder      |
|   [4]   | `BRepPrimAPI_MakeCone`       | cone          | full or partial cone          |
|   [5]   | `BRepPrimAPI_MakeTorus`      | torus         | full or partial torus         |
|   [6]   | `BRepPrimAPI_MakeWedge`      | wedge         | box-like wedge shape          |
|   [7]   | `BRepPrimAPI_MakePrism`      | extrusion     | linear extrusion along vector |
|   [8]   | `BRepPrimAPI_MakeRevol`      | revolution    | revolution around an axis     |
|   [9]   | `BRepPrimAPI_MakeRevolution` | revolution    | curve revolution surface      |
|  [10]   | `BRepPrimAPI_MakeHalfSpace`  | half-space    | infinite half-space from face |

[PUBLIC_TYPE_SCOPE]: Boolean and feature operations — OCC.Core.BRepAlgoAPI
- rail: cad-kernel boolean operations

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]        | [RAIL]                           |
| :-----: | :----------------------------- | :------------------- | :------------------------------- |
|   [1]   | `BRepAlgoAPI_Fuse`             | Boolean union        | unite two or more shapes         |
|   [2]   | `BRepAlgoAPI_Cut`              | Boolean difference   | subtract tool from object        |
|   [3]   | `BRepAlgoAPI_Common`           | Boolean intersection | intersect two shapes             |
|   [4]   | `BRepAlgoAPI_Section`          | Boolean section      | cross-section wire/edge result   |
|   [5]   | `BRepAlgoAPI_Splitter`         | shape splitter       | split shape by tool shapes       |
|   [6]   | `BRepAlgoAPI_Defeaturing`      | defeaturing          | remove faces from a solid        |
|   [7]   | `BRepAlgoAPI_Check`            | validity check       | shape self-intersection analysis |
|   [8]   | `BRepAlgoAPI_BooleanOperation` | base Boolean         | shared `Build()`, `SetTools()`   |

[PUBLIC_TYPE_SCOPE]: offset and sweep operations — OCC.Core.BRepOffsetAPI
- rail: cad-kernel offset and sweep

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [RAIL]                               |
| :-----: | :------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `BRepOffsetAPI_MakePipe`         | pipe sweep      | section along spine                  |
|   [2]   | `BRepOffsetAPI_MakePipeShell`    | advanced pipe   | evolving section along spine         |
|   [3]   | `BRepOffsetAPI_ThruSections`     | loft            | surface through cross-sections       |
|   [4]   | `BRepOffsetAPI_MakeOffset`       | wire offset     | offset wire or face boundary         |
|   [5]   | `BRepOffsetAPI_MakeOffsetShape`  | solid offset    | offset a solid                       |
|   [6]   | `BRepOffsetAPI_MakeThickSolid`   | shell           | thick-walled solid from face removal |
|   [7]   | `BRepOffsetAPI_MakeFilling`      | surface filling | N-sided patch from boundary curves   |
|   [8]   | `BRepOffsetAPI_DraftAngle`       | draft           | apply draft angles to faces          |
|   [9]   | `BRepOffsetAPI_NormalProjection` | projection      | project shapes onto a surface        |

[PUBLIC_TYPE_SCOPE]: STEP/IGES exchange — OCC.Core.STEPControl / OCC.Core.IGESControl
- rail: cad-kernel data exchange

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                              |
| :-----: | :----------------------- | :------------ | :---------------------------------- |
|   [1]   | `STEPControl_Reader`     | STEP reader   | `ReadFile`, `TransferRoot`, `Shape` |
|   [2]   | `STEPControl_Writer`     | STEP writer   | `Transfer`, `Write`, `Model`        |
|   [3]   | `STEPControl_Controller` | STEP control  | reader/writer session coordination  |
|   [4]   | `STEPControl_ActorRead`  | STEP actor    | entity transfer into topology       |
|   [5]   | `STEPControl_ActorWrite` | STEP actor    | topology transfer into entities     |
|   [6]   | `IGESControl_Reader`     | IGES reader   | `ReadFile`, `TransferRoots`         |
|   [7]   | `IGESControl_Writer`     | IGES writer   | `AddShape`, `Write`, `ComputeModel` |
|   [8]   | `IGESControl_Controller` | IGES control  | reader/writer session coordination  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: topology construction
- rail: cad-kernel shape construction

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [RAIL]                    |
| :-----: | :------------------------------------------------------------------ | :-------------- | :------------------------ |
|   [1]   | `BRepBuilderAPI_MakeEdge(curve)` / `MakeEdge(p1, p2)`               | edge factory    | curve or point-pair edge  |
|   [2]   | `BRepBuilderAPI_MakeWire(e1, e2, ...)`                              | wire factory    | wire from edges           |
|   [3]   | `BRepBuilderAPI_MakeFace(wire)` / `MakeFace(surface)`               | face factory    | face from wire or surface |
|   [4]   | `BRepBuilderAPI_MakeSolid(shell)`                                   | solid factory   | solid from shell          |
|   [5]   | `BRepBuilderAPI_Sewing.Add(shape)` + `.Perform()` + `.SewedShape()` | sewing          | join open topology        |
|   [6]   | `BRepBuilderAPI_Transform(shape, trsf)`                             | transform       | apply geometric transform |
|   [7]   | `BRepBuilderAPI_MakeShape.Shape() -> TopoDS_Shape`                  | result accessor | produced shape result     |

[ENTRYPOINT_SCOPE]: primitive construction
- rail: cad-kernel primitive construction

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]            |
| :-----: | :---------------------------------------- | :------------- | :---------------- |
|   [1]   | `BRepPrimAPI_MakeBox(dx, dy, dz).Shape()` | box            | axis-aligned box  |
|   [2]   | `BRepPrimAPI_MakeBox(P1, P2).Shape()`     | box            | corner-point box  |
|   [3]   | `BRepPrimAPI_MakeSphere(R).Shape()`       | sphere         | full sphere       |
|   [4]   | `BRepPrimAPI_MakeCylinder(R, H).Shape()`  | cylinder       | full cylinder     |
|   [5]   | `BRepPrimAPI_MakeCone(R1, R2, H).Shape()` | cone           | full cone/frustum |
|   [6]   | `BRepPrimAPI_MakeTorus(R1, R2).Shape()`   | torus          | full torus        |
|   [7]   | `BRepPrimAPI_MakePrism(S, V).Shape()`     | extrusion      | linear extrusion  |
|   [8]   | `BRepPrimAPI_MakeRevol(S, A).Shape()`     | revolution     | full revolution   |

[ENTRYPOINT_SCOPE]: Boolean operations
- rail: cad-kernel boolean operations

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]      | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------------------- | :------------------ | :---------------------------- |
|   [1]   | `BRepAlgoAPI_Fuse(S1, S2).Shape()`                                                 | Boolean union       | union of two shapes           |
|   [2]   | `BRepAlgoAPI_Cut(S1, S2).Shape()`                                                  | Boolean diff        | difference of two shapes      |
|   [3]   | `BRepAlgoAPI_Common(S1, S2).Shape()`                                               | Boolean isect       | intersection of two shapes    |
|   [4]   | `BRepAlgoAPI_Section(S1, S2).Shape()`                                              | Boolean sect        | section curve/wire            |
|   [5]   | `BRepAlgoAPI_BooleanOperation.SetArguments(list)` + `.SetTools(list)` + `.Build()` | multi-shape Boolean | general Boolean               |
|   [6]   | `BRepAlgoAPI_Check(shape).IsValid()`                                               | validity            | check shape self-intersection |

[ENTRYPOINT_SCOPE]: topology traversal
- rail: cad-kernel topology traversal

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `TopExp_Explorer(shape, TopAbs_ShapeEnum)` + `.More()` + `.Current()` + `.Next()` | explorer       | iterate sub-shapes by type     |
|   [2]   | `topexp.MapShapes(shape, type, IndexedMapOfShape)`                                | map builder    | index all sub-shapes of a type |
|   [3]   | `topexp.MapShapesAndAncestors(shape, type1, type2, map)`                          | ancestor map   | sub-shapes to ancestor map     |
|   [4]   | `topexp.FirstVertex(edge) -> TopoDS_Vertex`                                       | vertex query   | first vertex of an edge        |
|   [5]   | `topexp.LastVertex(edge) -> TopoDS_Vertex`                                        | vertex query   | last vertex of an edge         |
|   [6]   | `topods.Edge(shape) -> TopoDS_Edge`                                               | downcast       | safe downcast to edge type     |
|   [7]   | `topods.Face(shape) -> TopoDS_Face`                                               | downcast       | safe downcast to face type     |

[ENTRYPOINT_SCOPE]: STEP/IGES exchange
- rail: cad-kernel data exchange

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------- |
|   [1]   | `STEPControl_Reader.ReadFile(path) -> int`        | reader         | read STEP file; return status    |
|   [2]   | `STEPControl_Reader.TransferRoot(n) -> bool`      | transfer       | transfer nth root to topology    |
|   [3]   | `STEPControl_Reader.Shape(n) -> TopoDS_Shape`     | result         | get nth transferred shape        |
|   [4]   | `STEPControl_Writer.Transfer(shape, mode) -> int` | writer         | convert shape to STEP entity     |
|   [5]   | `STEPControl_Writer.Write(path) -> int`           | writer         | write STEP file                  |
|   [6]   | `IGESControl_Reader.ReadFile(path) -> int`        | reader         | read IGES file                   |
|   [7]   | `IGESControl_Writer.AddShape(shape) -> None`      | writer         | add shape to IGES output         |
|   [8]   | `IGESControl_Writer.ComputeModel() -> None`       | writer         | finalize IGES model before write |
|   [9]   | `IGESControl_Writer.Write(path) -> bool`          | writer         | write IGES file                  |

## [4]-[IMPLEMENTATION_LAW]

[OCC_TOPOLOGY]:
- all modules live under `OCC.Core.*`; import as `from OCC.Core.TopoDS import TopoDS_Shape`
- shape hierarchy: `TopoDS_Vertex` < `TopoDS_Edge` < `TopoDS_Wire` < `TopoDS_Face` < `TopoDS_Shell` < `TopoDS_Solid` < `TopoDS_CompSolid` / `TopoDS_Compound`
- `TopAbs_ShapeEnum` values: `TopAbs_VERTEX`, `TopAbs_EDGE`, `TopAbs_WIRE`, `TopAbs_FACE`, `TopAbs_SHELL`, `TopAbs_SOLID`, `TopAbs_COMPSOLID`, `TopAbs_COMPOUND`
- downcast via `topods.Edge(shape)` etc. is required before accessing type-specific members; a cast to the wrong type raises an exception
- SWIG bindings are generated from `.i` files per OCCT module; ~250+ `OCC.Core.*` submodules are available
- distribution is conda-only (`conda install -c conda-forge pythonocc-core`); no PyPI wheel

[LOCAL_ADMISSION]:
- Import each module from `OCC.Core.<ModuleName>` — namespace is one class or group per module (e.g., `OCC.Core.BRep`, `OCC.Core.BRepAdaptor`, `OCC.Core.STEPControl`)
- Builders follow the OCCT command pattern: construct, configure, call `Build()` or equivalent, then call `Shape()` or error-check `IsDone()`
- `STEPControl_StepModelType` enum selects the STEP schema variant for writing: `AsIs`, `ManifoldSolidBrep`, etc.

[RAIL_LAW]:
- Package: `pythonocc-core`
- Owns: OCCT BREP topology, parametric geometry, Boolean operations, primitive construction, STEP/IGES exchange
- Accept: conda environment only; `OCC.Core.*` namespace imports
- Reject: PyPI installation attempts; hand-rolled BREP topology or exchange logic that duplicates OCCT capability
