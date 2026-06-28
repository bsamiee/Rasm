# [RASM_COMPUTE_API_FEALITE2D_PLOTTING]

`FEALiTE2D.Plotting` is the structural-result DIAGRAM emitter for the 2D `FEALiTE2D` solver
(`api-fealite2d`): one `Plotter` (over a solved `FEALiTE2D.Structure` + a `PlottingOption`) renders the
normal-force (NFD), shear-force (SFD), bending-moment (BMD), and deflected-shape diagrams for any
`LoadCase`/`LoadCombination` into a DXF drawing, laying the four diagram panels out side by side per
load case through the `PlotNFD`/`PlotSFD`/`PlotBMD`/`PlotDisplacement` fold over each element's
`Structure.Results.GetElementInternalForces` mesh segments, with the `ExtensionMethods`
`PointLocationOnLine`/`PointPerpendicularToLine`/`PointForDeflection` projecting force/deflection
ordinates onto the member axis. It carries `netDxf.netstandard` as its rendering backend — the full
managed DXF read/write library (`DxfDocument` over the `Vector2`/`Vector3` geometry, the `Line`/`MText`/
`Polyline2D`/`Hatch`/`Dimension`/`Insert` entity set, the `Layer`/`Linetype`/`TextStyle`/`DimensionStyle`
tables, `AciColor`/`Lineweight`, the `Group`/`Layout`/`Block` objects, and the AutoCad2000-2018 read/write
IO). The LOAD-BEARING BOUNDARY: netDxf is admitted ONLY as this plotter's transitive rendering backend
for structural diagrams; it is NOT the canonical CAD authority — `ACadSharp` (`api-acadsharp`, in
Rasm.Bim) owns DWG/DXF across the model, so a structural-diagram `.dxf` is a FEALiTE2D-specific artifact
that crosses to Persistence/AppUi as bytes (or round-trips through the file into ACadSharp), never a
second in-process DXF authority beside ACadSharp. It is pure-managed under MIT (both packages), the
result-visualization leg of `Solver/contract#SOLVE_CONTRACT` for the 2D structural lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FEALiTE2D.Plotting` (+ rendering backend `netDxf.netstandard`)
- package: `FEALiTE2D.Plotting` (the diagram emitter) — the csproj direct pin under "Structural Solvers";
  `netDxf.netstandard` (the DXF backend) is its transitive dependency, also centrally pinned
- version: `FEALiTE2D.Plotting` `1.1.2` (tracking the `FEALiTE2D` `1.1.2` core); `netDxf.netstandard` `3.0.1`
- license: MIT for both (`license type="expression"`; `FEALiTE/FEALiTE2D` and the netDxf netstandard fork)
  — reference the unmodified NuGet binaries
- assembly: `FEALiTE2D.Plotting` → the `net10.0` consumer binds `lib/net8.0` (ships
  `net8.0`/`netstandard2.0`/`net45`); `netDxf.netstandard` → binds `lib/net8.0` (ships
  `net8.0`/`net6.0`/`netstandard2.0`/`net471`/`net48`); both `net8.0` bind forward under `net10.0`;
  pure-managed AnyCPU IL, ALC-safe, no native asset
- namespace: `FEALiTE2D.Plotting.Dxf` (`Plotter`/`PlottingOption`/`ExtensionMethods`); `netDxf` (the
  `DxfDocument`/`Vector2`/`Vector3`/`AciColor`/`EntityObject`/`BoundingRectangle` root), `netDxf.Entities`
  (`Line`/`MText`/`Polyline2D`/`Hatch`/`Dimension`/…), `netDxf.Tables` (`Layer`/`Linetype`/`TextStyle`/…),
  `netDxf.Objects` (`Group`/`Layout`/…), `netDxf.Header` (`DxfVersion`/`HeaderVariables`),
  `netDxf.Blocks` (`Block`), `netDxf.IO`, `netDxf.Collections`, `netDxf.Units`
- transitive: `FEALiTE2D` (`api-fealite2d`, the 2D solver supplying `Structure`/`IElement`/`Node2D`/
  `LoadCase`/`LoadCombination`/`LinearMeshSegment`/`Structure.Results` the plotter reads — NOT redocumented
  here); `netDxf.netstandard` carries NO dependencies of its own (a standalone DXF library)
- scope: 2D structural-result diagram authoring (NFD/SFD/BMD/deflection) to DXF, and the DXF read/write
  library that backs it; NOT a solver (`FEALiTE2D` solves, this renders), NOT the canonical CAD authority
  (`ACadSharp` owns DWG/DXF), NOT a 3D-result visualizer (the 3D `BriefFiniteElement.Net` result feeds the
  AppUi/export rails directly)
- rail: `Solver/contract#SOLVE_CONTRACT` (the 2D structural-result-visualization leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the diagram emitter (`FEALiTE2D.Plotting.Dxf`)
- rail: solve
- note: `Plotter` requires a SOLVED `Structure` (its ctor throws if `AnalysisStatus` is non-zero) and a
  `LoadCase` that is in `Structure.LoadCasesToRun`; `PlottingOption` is the layer/scale/offset policy, and
  `ExtensionMethods` projects ordinates onto the member axis.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Plotter`                      | diagram emitter    | over a solved `Structure` + `PlottingOption`; the `Plot`/`PlotNFD`/`PlotSFD`/`PlotBMD`/`PlotDisplacement` surface |
|  [02]   | `PlottingOption`               | plot policy        | `DxfVersion`, the per-diagram `Layer` set, the per-diagram scale factors, and the inter-diagram offsets |
|  [03]   | `ExtensionMethods` (static)    | axis projection    | `PointLocationOnLine`/`PointPerpendicularToLine`/`PointForDeflection` over a `FEALiTE2D.Elements.IElement` → `Vector2` |

[PUBLIC_TYPE_SCOPE]: netDxf document, geometry, and color (`netDxf`)
- rail: solve
- note: `DxfDocument` is the drawing root (entities, tables, blocks, layouts, header vars); the geometry
  value types and color/weight are the entity coordinate/appearance primitives.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `DxfDocument` (`: DxfObject`)   | drawing root       | `Entities` (`DrawingEntities`)/`Layers`/`Blocks` (`BlockRecords`)/`Layouts`/`DrawingVariables`; `Load`/`Save`; netDxf 3.x routes entity add/remove through `Entities.Add`/`Entities.Remove`, not document-level `AddEntity` |
|  [02]   | `Vector2` / `Vector3` (struct) | geometry value     | the 2D/3D coordinate the entity start/end/position carries |
|  [03]   | `AciColor`                     | color value        | the ACI color (`Cyan`/`Yellow`/`Blue`/`Red`/`Green`/…) a layer/entity carries |
|  [04]   | `Lineweight` (enum)            | weight value       | the line weight a layer/entity carries |
|  [05]   | `BoundingRectangle`            | extents value      | `Width`/`Height` from a point set (the per-diagram panel extents the plotter lays out by) |
|  [06]   | `EntityObject` (abstract) / `DxfObject` (abstract) / `EntityType` (enum) | entity base | the entity/object hierarchy root and the entity-kind discriminant |
|  [07]   | `DxfVersion` (`netDxf.Header`) | version selector   | the AutoCAD release the document writes (`AutoCad2000`…`AutoCad2018`; writable releases are 2000+) |
|  [08]   | `HeaderVariables` (`netDxf.Header`) | header vars   | the DXF `$`-header drawing settings (units, limits, current layer) |

[PUBLIC_TYPE_SCOPE]: netDxf entities, tables, and objects (`netDxf.Entities` / `.Tables` / `.Objects` / `.Blocks`)
- rail: solve
- note: the full DXF entity/table/object surface — the plotter uses `Line`/`MText`/`Layer`, but the
  library backs the entire DXF model so a richer structural drawing (dimensioned, hatched, blocked) composes
  on the same `DxfDocument`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Line` / `Ray` / `XLine` / `Arc` / `Circle` / `Ellipse` / `Point` | curve entities | the diagram-segment `Line` (the plotter's primitive) and the conic/ray entities |
|  [02]   | `Polyline2D` / `Polyline3D` / `Spline` | polyline entities | the polyline/spline entities a continuous diagram envelope composes on |
|  [03]   | `MText` / `Text` (+ `MTextAttachmentPoint` / `MTextFormattingOptions`) | text entities | the diagram label (`MText`, the plotter's title primitive) and single-line text |
|  [04]   | `Hatch` (+ `HatchPattern` / `HatchGradientPattern` / `HatchBoundaryPath`) | fill entity | a filled diagram region (a stress/area shading) |
|  [05]   | `Dimension` (abstract) + `AlignedDimension` / `LinearDimension` / `Angular2LineDimension` / `Angular3PointDimension` / `ArcLengthDimension` / `DiametricDimension` / `RadialDimension` / `OrdinateDimension` | dimension entities | the annotated-dimension family for a dimensioned structural drawing |
|  [06]   | `Insert` / `Block` (`netDxf.Blocks`) | block reference | a reusable detail block and its placement (a support/load symbol) |
|  [07]   | `Solid` / `Face3D` / `Mesh` / `PolygonMesh` / `PolyfaceMesh` / `MLine` / `Leader` / `Image` / `Shape` / `Tolerance` | surface/annotation entities | the remaining DXF entity set the library backs |
|  [08]   | `Layer` (`netDxf.Tables`)      | layer table        | `Color` (`AciColor`) + `Lineweight` + `Linetype`; the per-diagram layer the plotter assigns |
|  [09]   | `Linetype` / `TextStyle` / `DimensionStyle` / `UCS` / `View` / `VPort` / `ApplicationRegistry` (`netDxf.Tables`) | table objects | the linetype/text/dimension styles and the coordinate-system/view tables |
|  [10]   | `Group` / `Layout` / `MLineStyle` / `ImageDefinition` / `LayerState` (`netDxf.Objects`) + `XData` (`netDxf`) | document objects | the entity group, the paper/model layout, the layer-state snapshot, and the extended-entity-data record |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plot a solved structure to DXF (the canonical leg)
- rail: solve
- note: the canonical call constructs a `Plotter` over the solved `FEALiTE2D.Structure`, then `Plot`s the
  four-panel diagram set for a load case/combination to a `.dxf`; the load case must be in
  `Structure.LoadCasesToRun` or `Plot` throws.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new Plotter(Structure structure, PlottingOption option)`        | construct       | bind a SOLVED structure + plot policy (throws if the structure is unsolved) |
|  [02]   | `plotter.Plot(string filePath, LoadCase loadCase)`               | plot            | write the NFD+SFD+BMD+deflection four-panel set for one load case |
|  [03]   | `plotter.Plot(string filePath, IEnumerable<LoadCase> loadCases)` | plot            | stack one row of panels per load case |
|  [04]   | `plotter.Plot(string filePath, LoadCombination loadCombination)` / `(IEnumerable<LoadCombination>)` | plot | the same for factored load combinations |
|  [05]   | `plotter.PlotNFD(DxfDocument, LoadCase \| LoadCombination, Vector2 startPosition)` | plot panel | author the normal-force diagram into an existing document at an offset |
|  [06]   | `plotter.PlotSFD(...)` / `PlotBMD(...)` / `PlotDisplacement(...)` | plot panel | the shear-force / bending-moment / deflected-shape panels |
|  [07]   | `plotter.PlotStructure(DxfDocument, Vector2 startPosition)`       | plot panel      | the bare statical-system line drawing |

[ENTRYPOINT_SCOPE]: plot policy and axis projection
- rail: solve
- note: `PlottingOption` defaults a per-diagram layer set (cyan structure, yellow NFD, blue SFD, red BMD,
  green deflection) and per-diagram scale factors; `ExtensionMethods` is the member-axis ordinate projection.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new PlottingOption()`                                            | construct       | the default plot policy (default layers/colors/scales/offsets) |
|  [02]   | `option.DxfVersion` / `option.LayerOfNFD` / `LayerOfSFD` / `LayerOfBMD` / `LayerOfDisplacement` / `LayerOfStructure` | policy | the output AutoCAD release and the per-diagram `Layer` |
|  [03]   | `option.NFDScaleFactor` / `SFDScaleFactor` / `BMDScaleFactor` / `DisplacmentScaleFactor` / `StructureScaleFactor` | policy | the per-diagram ordinate scale |
|  [04]   | `option.DiagramsHorizontalOffsets` / `DiagramsVerticalOffsets`    | policy          | the inter-panel layout spacing |
|  [05]   | `element.PointLocationOnLine(double x)` / `PointPerpendicularToLine(double x, double perDistance)` / `PointForDeflection(double x, double dx, double dy)` → `Vector2` | projection | project a force/deflection ordinate onto the member axis |

[ENTRYPOINT_SCOPE]: netDxf document IO (the backend, for a hand-composed drawing or re-read)
- rail: solve
- note: the plotter writes the document internally, but the same `DxfDocument` surface composes a richer
  structural drawing or re-reads a `.dxf` (the cross-pipeline round-trip with ACadSharp is through the file).

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `new DxfDocument(DxfVersion)` / `new DxfDocument()`               | construct       | an empty drawing at an AutoCAD release |
|  [02]   | `doc.Entities.Add(EntityObject)` / `doc.Entities.Remove(EntityObject)` (also `Add(IEnumerable<EntityObject>)`) | author          | add/remove a `Line`/`MText`/`Hatch`/… entity through the `DrawingEntities` collection |
|  [03]   | `doc.Save(string file)` / `Save(string file, bool isBinary)` / `Save(Stream)` / `Save(Stream, bool isBinary)` | save        | write the `.dxf` (ASCII, or binary DXF with `isBinary`) |
|  [04]   | `DxfDocument.Load(string file, IEnumerable<string> supportFolders)` / `Load(Stream, …)` | open | re-read a `.dxf` document |
|  [05]   | `doc.Layers` / `doc.Blocks` / `doc.Layouts` / `doc.DrawingVariables` | document tables | the layer/block/layout/header collections |

## [04]-[IMPLEMENTATION_LAW]

[PLOT_TOPOLOGY]:
- a `Plotter` binds a SOLVED `FEALiTE2D.Structure` (its ctor throws `InvalidOperationException` when
  `Structure.AnalysisStatus` is non-zero) and a `PlottingOption`; `Plot` validates the load case is in
  `Structure.LoadCasesToRun` (throws `ArgumentException` otherwise) — these are upstream-contract checks, so
  the plot path runs only on a solved structure and an admitted load case
- each panel folds `Structure.Elements` → `Structure.Results.GetElementInternalForces(element, loadCase)`
  (a `List<LinearMeshSegment>`), reading `Internalforces1/2.Fx` (NFD), `.Fy` (SFD), `.Mz` (BMD), and
  `Displacement1/2.Ux/Uy` (deflection), projecting each ordinate onto the member axis through
  `PointPerpendicularToLine`/`PointForDeflection`, and emitting `Line` segments + an `MText` title onto the
  panel's `Layer`; the four panels lay out side by side by the `BoundingRectangle.Width` + the horizontal
  offset, stacked by `Height` + the vertical offset for multi-case plots
- `PlottingOption` defaults a distinct `Layer` per diagram (cyan structure, yellow NFD, blue SFD, red BMD,
  green deflection) so the diagrams separate by layer in the CAD consumer, and per-diagram scale factors so
  the force/deflection ordinates render at a legible size

[BOUNDARY] — netDxf is the backend, ACadSharp the authority:
- `netDxf.netstandard` is admitted ONLY as `FEALiTE2D.Plotting`'s transitive DXF rendering backend; the
  canonical DWG/DXF authority across the model is `ACadSharp` (`api-acadsharp`, in Rasm.Bim) — Rasm.Compute
  does not reference ACadSharp, so within Compute netDxf is the sole DXF library and is used ONLY by the
  plotter
- a structural-diagram `.dxf` is a FEALiTE2D-specific artifact: it crosses to Persistence as content-keyed
  bytes / to AppUi as a drawing view, and any cross-pipeline composition with the canonical CAD model
  round-trips THROUGH the file (netDxf writes the `.dxf`, ACadSharp re-reads it) — holding a netDxf
  `DxfDocument` and an ACadSharp `CadDocument` as two live authorities over the same drawing is the rejected
  form
- the netDxf advanced surface (the `Dimension`/`Hatch`/`Insert`/`Block`/`Layout` set, the table objects)
  composes a richer structural drawing on the same `DxfDocument` when the plotter's bare-`Line` output needs
  dimensioning/blocking, but it never becomes a general CAD-authoring rail — that is ACadSharp's

[STACKING] — the 2D structural-result-visualization leg:
- `FEALiTE2D.Plotting` is the result-visualization leg of `Solver/contract#SOLVE_CONTRACT` for the 2D
  structural lane: the `FEALiTE2D` solver (`api-fealite2d`) solves the planar frame, and this plotter renders
  its `Structure.Results` internal-force/deflection diagrams to DXF — the solve and the render are disjoint,
  the plotter consuming the solved `Structure` by reference
- with `FEALiTE2D` (`api-fealite2d`): the upstream 2D solver — `Structure`/`IElement`/`Node2D`/`LoadCase`/
  `LoadCombination`/`LinearMeshSegment`/`Structure.Results` are its types the plotter reads, never
  redocumented or re-typed here
- with `BriefFiniteElement.Net` (`api-brief-finite-element`): the 3D structural twin — both 2D and 3D
  solvers ride CSparse; the 2D result renders through this DXF plotter, while the 3D `StaticLinearAnalysisResult`
  feeds the AppUi viewport/export rails directly by content key (there is no 3D DXF-diagram plotter)
- with the Bim `Model/structural#ANALYSIS_MODEL`: the upstream idealized graph the 2D solver is assembled
  from (planar members/supports/loads); the DXF diagram is the terminal report of that graph's solve
- with Persistence/AppUi: the emitted `.dxf` is a content-keyed artifact the Persistence blob lane stores
  and the AppUi drawing view renders — the same content-addressing the other solver receipts use

[LOCAL_ADMISSION]:
- the 2D structural-result diagram is `FEALiTE2D.Plotting`, rendered from a SOLVED `FEALiTE2D.Structure`;
  a hand-rolled DXF diagram writer or a re-derived internal-force projection beside the plotter is the
  rejected form
- the DXF backend is `netDxf.netstandard`, used ONLY by the plotter; reaching for netDxf as a general
  DWG/DXF authoring rail (ACadSharp's role) is the named boundary violation
- the diagram `.dxf` crosses to Persistence/AppUi as a content-keyed artifact and round-trips into the
  canonical CAD model through the file via ACadSharp; two live DXF authorities over one drawing is rejected
- the plot path runs only on a solved structure and an admitted load case (the ctor/`Plot` contract checks);
  plotting an unsolved structure or an unrun load case is the upstream-contract violation the package throws on

[RAIL_LAW]:
- Package: `FEALiTE2D.Plotting` (1.1.2) + `netDxf.netstandard` (3.0.1), both MIT, pure-managed `lib/net8.0`
  AnyCPU IL binding forward under net10, no native asset; transitive `FEALiTE2D` (the 2D solver) the plotter
  reads
- Owns: 2D structural-result DIAGRAM authoring — the `Plotter` rendering the NFD/SFD/BMD/deflection panels
  for any `LoadCase`/`LoadCombination` of a solved `FEALiTE2D.Structure` to DXF, the `PlottingOption`
  layer/scale/offset policy, the `ExtensionMethods` member-axis projection, and (via netDxf) the full DXF
  read/write document, entity, table, and object surface that backs it
- Accept: the result-visualization leg of `Solver/contract#SOLVE_CONTRACT` for the 2D lane — render a solved
  `FEALiTE2D.Structure`'s `Structure.Results` to a content-keyed `.dxf` artifact crossing to Persistence/AppUi,
  composing a richer drawing on the netDxf `DxfDocument` surface when dimensioning/blocking is needed
- Reject: using netDxf as a general DWG/DXF authoring rail (ACadSharp `api-acadsharp` owns CAD authority);
  holding a netDxf `DxfDocument` and an ACadSharp `CadDocument` as two live authorities over one drawing
  (cross-pipeline composition round-trips through the file); a hand-rolled DXF diagram writer or re-derived
  internal-force projection beside the plotter; plotting an unsolved structure or an unrun load case (the
  contract checks throw); a 3D-result DXF plotter (the 3D `BriefFiniteElement.Net` result feeds AppUi/export
  directly); binding the `net45`/`netstandard2.0` asset (net10 binds `net8.0`)
