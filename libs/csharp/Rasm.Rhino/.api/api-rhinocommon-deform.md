# [RASM_RHINO_API_RHINOCOMMON_DEFORM]

This catalog owns the host-fidelity nonlinear deformation and flattening boundary: the `SpaceMorph` abstract engine and its concrete `Rhino.Geometry.Morphs` implementations (bend, flow, maelstrom, splop, sporph, stretch, taper, twist, mesh-cage), the `MorphControl` interactive deformer, and the `Unroller`/`Squisher`/`MeshUnwrapper` flattening set. A morph deforms any `GeometryBase` in place along a type-agnostic point map, standing beside the geometry catalog's linear `Transform` mutation and the kernel's host-neutral DEC UV-flattening in `Rasm/Processing/flatten`, which own different altitudes and are never re-derived here. Every engine P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's own commands; the flatteners are disposable native resources, and every outcome projects onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon deformation-and-flattening surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`, `Rhino.Geometry.Morphs`
- kernel: `Rasm` (host-neutral DEC flattening and linear-motion owners composed by altitude, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: deformation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: morph engines
- rail: deformation

| [INDEX] | [SYMBOL]              | [KIND]          | [CAPABILITY]                                                  |
| :-----: | :-------------------- | :-------------- | :------------------------------------------------------------ |
|  [01]   | `SpaceMorph`          | abstract base   | type-agnostic point map over any `GeometryBase`, tuning knobs |
|  [02]   | `BendSpaceMorph`      | morph           | bend about a spine between start and end points               |
|  [03]   | `FlowSpaceMorph`      | morph           | reflow geometry from a base curve onto a target curve         |
|  [04]   | `MaelstromSpaceMorph` | morph           | spiral twist between two radii about a plane                  |
|  [05]   | `SplopSpaceMorph`     | morph           | map a plane region onto a surface uv patch                    |
|  [06]   | `SporphSpaceMorph`    | morph           | reflow from one surface onto another                          |
|  [07]   | `StretchSpaceMorph`   | morph           | axial stretch between two points to a length or point         |
|  [08]   | `TaperSpaceMorph`     | morph           | radial taper between two radii along an axis                  |
|  [09]   | `TwistSpaceMorph`     | morph           | rotate about an axis by a per-length angle                    |
|  [10]   | `MeshCageMorph`       | morph           | cage-deform geometry from a reference mesh to a target mesh   |
|  [11]   | `MorphControl`        | native geometry | interactive curve-to-curve or surface deformer                |

[PUBLIC_TYPE_SCOPE]: flattening engines
- rail: deformation

| [INDEX] | [SYMBOL]           | [KIND]         | [CAPABILITY]                                                     |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Unroller`         | flattener      | unroll developable surfaces/breps with following geometry        |
|  [02]   | `Squisher`         | disposable     | stress-relaxed flatten of surfaces and meshes with mark tracking |
|  [03]   | `SquishParameters` | config carrier | spring constants, deformation mode, topology and mapping policy  |
|  [04]   | `MeshUnwrapper`    | disposable     | uv-unwrap a mesh set by an unwrap algorithm                      |

[ENUM_ROSTERS]:
- `public enum Rhino.Geometry.MeshUnwrapMethod` — `LSCM`, `ABFPP`, `ARAP`.
- `public enum Rhino.Geometry.SquishFlatteningAlgorithm` — `Geometric`, `PhysicalStress`.
- `public enum Rhino.Geometry.SquishDeformation` — `Free`, `StretchMostly`, `StretchOnly`, `CompressMostly`, `CompressOnly`, `Custom`.

## [03]-[ENTRYPOINTS]

[MORPH_BASE]:
- `Rhino.Geometry.SpaceMorph.Morph(GeometryBase geometry) : bool` — deforms any geometry in place along the concrete point map; a `false` return signals the geometry could not morph.
- `Rhino.Geometry.SpaceMorph.Morph(ref Plane plane) : bool` — morphs a plane frame by its origin and axes.
- `Rhino.Geometry.SpaceMorph.MorphPoint(Point3d point) : Point3d` — the abstract per-point map each concrete morph defines.
- `Rhino.Geometry.SpaceMorph.IsMorphable(GeometryBase geometry) : bool` — whether a geometry kind supports morphing.
- `Rhino.Geometry.SpaceMorph.Tolerance : double` / `QuickPreview : bool` / `PreserveStructure : bool` — the deformation tolerance and the preview and control-point-preservation knobs shared by every morph.

[MORPH_KINDS]:
- `Rhino.Geometry.Morphs.BendSpaceMorph(Point3d start, Point3d end, Point3d point, double angle, bool straight, bool symmetric)` — bends about the start-end spine to a target angle; the no-angle overload bends toward the third point.
- `Rhino.Geometry.Morphs.FlowSpaceMorph(Curve curve0, Curve curve1, bool reverseCurve0, bool reverseCurve1, bool preventStretching)` — reflows geometry from a base curve onto a target curve with reversal and stretch control; the `(curve0, curve1, preventStretching)` overload drops the reversals.
- `Rhino.Geometry.Morphs.MaelstromSpaceMorph(Plane plane, double radius0, double radius1, double angle)` — spirals geometry between two radii about a plane by an angle.
- `Rhino.Geometry.Morphs.SplopSpaceMorph(Plane plane, Surface surface, Point2d surfaceParam, double scale, double angle)` — maps a plane region onto a surface uv location with scale and rotation.
- `Rhino.Geometry.Morphs.SporphSpaceMorph(Surface surface0, Surface surface1, Point2d surface0Param, Point2d surface1Param)` — reflows from a base surface onto a target surface aligned at uv params; the two-surface overload auto-aligns, and `ConstrainNormal : Vector3d` (`get/set`) pins the morph normal.
- `Rhino.Geometry.Morphs.StretchSpaceMorph(Point3d start, Point3d end, double length)` — stretches along the axis to a length; the `(start, end, Point3d point)` overload stretches to a point.
- `Rhino.Geometry.Morphs.TaperSpaceMorph(Point3d start, Point3d end, double startRadius, double endRadius, bool bFlat, bool infiniteTaper)` — radial taper along an axis between two radii.
- `Rhino.Geometry.Morphs.TwistSpaceMorph()` — twists about `TwistAxis : Line` by `TwistAngleRadians : double`, with `InfiniteTwist : bool` extending the twist past the axis ends.
- `Rhino.Geometry.Morphs.MeshCageMorph(Mesh referenceMesh, Mesh targetMesh)` — cage deformation of arbitrary geometry through a reference/target mesh pair; every concrete morph subclass carries `Dispose()` (the `SpaceMorph` base is not `IDisposable`), `IsValid`, and the abstract `MorphPoint(Point3d)` override, and `IsMorphable(GeometryBase)` is a `SpaceMorph` static.
- `Rhino.Geometry.Morphs.MeshMorphMesh` is `internal` (host-reserved), so vertex-position mapping through a reference/adjusted mesh pair has no accessible surface; `MeshCageMorph` is the sole public mesh-driven deformer.

[MORPH_CONTROL]:
- `Rhino.Geometry.MorphControl(NurbsCurve originCurve, NurbsCurve targetCurve)` — an interactive deformer driven by an origin-to-target curve pair.
- `Rhino.Geometry.MorphControl.Morph(GeometryBase geometry) : bool` — applies the control deformation to geometry.
- `Rhino.Geometry.MorphControl.SpaceMorphTolerance : double` / `QuickPreview : bool` / `PreserveStructure : bool` / `Curve : NurbsCurve` / `Surface : NurbsSurface` — the tuning knobs and the driving curve/surface reads.

[UNROLL]:
- `Rhino.Geometry.Unroller(Brep brep)` / `Unroller(Surface surface)` — constructs an unroller over a developable brep or surface.
- `Rhino.Geometry.Unroller.AddFollowingGeometry(IEnumerable<Curve> curves) : void` — registers geometry to carry into the flattened frame; the full overload roster covers `Curve`, `IEnumerable<Point3d>`, `Point3d`, `Point`, `IEnumerable<TextDot>`, `TextDot`, `(IEnumerable<Point3d> dotLocations, IEnumerable<string> dotText)`, and `(Point3d dotLocation, string dotText)`, so dot rows never require a minted `TextDot`.
- `Rhino.Geometry.Unroller.PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots) : Brep[]` — unrolls and returns the flattened breps beside the carried geometry; the `(List<Brep> flatbreps)` overload returns a follow-geometry count.
- `Rhino.Geometry.Unroller.FollowingGeometryIndex(Curve curve) : int` — the index of a carried curve or dot in the unroll output.

[SQUISH]:
- `Rhino.Geometry.Squisher()` — constructs a squisher; the instance is `IDisposable` and rides a using scope or lease.
- `Rhino.Geometry.Squisher.SquishSurface(SquishParameters sp, Surface surface, IEnumerable<GeometryBase> marks, List<GeometryBase> squished_marks_out) : Brep` — stress-relaxes a surface to a flat brep, mapping marks into the caller-owned output list; the no-marks overload drops them.
- `Rhino.Geometry.Squisher.SquishMesh(SquishParameters sp, Mesh mesh3d, IEnumerable<GeometryBase> marks, List<GeometryBase> squished_marks_out) : Mesh` — flattens a mesh with mark tracking.
- `Rhino.Geometry.Squisher.SquishCurve(Curve curve) : PolylineCurve` / `SquishTextDot(TextDot textDot) : TextDot` / `SquishPoint(Point3d point, out Point3d squishedPoint) : bool` — per-object mapping into the flattened frame after a squish.
- `Rhino.Geometry.Squisher.Get2dMesh() : Mesh` / `Get3dMesh() : Mesh` / `GetLengthConstrained2dLines() : Line[]` / `GetLengthConstrained3dLines() : Line[]` / `GetAreaConstrainedTrianglesIndices() : MeshFace[]` — the flattened and source triangulations and their length/area constraint diagnostics; `GetMesh2dEdges()`/`GetMesh3dEdges()` alias `GetLengthConstrained2dLines()`/`GetLengthConstrained3dLines()` and return identical data, so a consumer reads one pair, never both.
- `Rhino.Geometry.Squisher.Is2dPatternSquished(GeometryBase geometry) : bool` / `SquishBack2dMarks(GeometryBase squishedGeometry, IEnumerable<GeometryBase> marks) : IEnumerable<GeometryBase>` — the static probe and inverse-mapping of flattened marks back to 3d.
- `Rhino.Geometry.SquishParameters.Default : SquishParameters` — the standing preset; `PreserveTopology : bool`, `SaveMapping : bool`, `BoundaryStretchConstant`/`BoundaryCompressConstant`/`InteriorStretchConstant`/`InteriorCompressConstant : double`, `AbsoluteLimit : double`, and `Algorithm : SquishFlatteningAlgorithm` are the policy knobs.
- `Rhino.Geometry.SquishParameters.SetDeformation(SquishDeformation deformation, bool bPreserveBoundary, double boundaryStretchConstant, double boundaryCompressConstant, double interiorStretchConstant, double interiorCompressConstant) : void` / `SetSpringConstants(double boundaryBias, double deformationBias) : void` / `GetSpringConstants(out double boundaryBias, out double deformationBias) : bool` — the deformation-mode and spring-bias seams; deformation is set-only with no read-back property, and the carrier is `IDisposable`.

[UNWRAP]:
- `Rhino.Geometry.MeshUnwrapper(Mesh mesh)` / `MeshUnwrapper(IEnumerable<Mesh> meshes)` — constructs an unwrapper over one or a set of meshes; the instance is `IDisposable` and `SymmetryPlane : Plane` is set-only, pinning a symmetry constraint the unwrap consumes.
- `Rhino.Geometry.MeshUnwrapper.Unwrap(MeshUnwrapMethod method) : bool` — computes uv coordinates by the chosen unwrap algorithm and writes them onto the mesh texture coordinates.

## [04]-[IMPLEMENTATION_LAW]

[DEFORM_TOPOLOGY]:
- `SpaceMorph` is a type-agnostic engine: `Morph(GeometryBase)` deforms any geometry along the concrete `MorphPoint` map in place, so the ten morphs discriminate on deformation kind rather than geometry type, and each carries the shared `Tolerance`, `QuickPreview`, and `PreserveStructure` knobs beside its own defining parameters; `IsMorphable` gates a geometry kind before the call.
- `MorphControl` holds a persistent origin-to-target driver and morphs against it repeatedly, distinct from the one-shot `SpaceMorph` subclasses; it is itself a `GeometryBase` and follows the geometry catalog's custody rules.
- flattening is a distinct host-fidelity altitude from the kernel's DEC UV-flatten: `Unroller` unrolls developable surfaces carrying following geometry, `Squisher` stress-relaxes non-developable surfaces and meshes under a `SquishParameters` spring model with forward and inverse mark mapping, and `MeshUnwrapper` computes uv by an unwrap algorithm; `Squisher` and `MeshUnwrapper` are native `IDisposable` resources.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a `bool` morph or unwrap folds into a `Fin<Unit>` keyed to the mutated geometry, a nullable squish or unroll result lifts to `Option<Brep>`/`Option<Mesh>`, the unroll `Brep[]` and carried-geometry arrays land as `Seq<A>`, and the caller-owned squish mark lists and the `PerformUnroll` parallel `out` arrays fold into one detached flatten receipt.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed flattening vocabularies — `MeshUnwrapMethod`, `SquishFlatteningAlgorithm`, and `SquishDeformation` — wrap as `[SmartEnum<TKey>]` owners; the morph kind models as a `[Union]` over the bend, flow, maelstrom, splop, sporph, stretch, taper, twist, mesh-cage, and mesh-morph arms, each binding its defining-parameter carrier.
- `Rasm` kernel: host-neutral DEC UV-flattening and linear-motion transforms stand at the kernel altitude and the boundary re-derives none of them; radii, angles, lengths, spring constants, and tolerances compose the kernel numeric and unit owners before the native call.

[LOCAL_ADMISSION]:
- deformation enters through the morph union or the `MorphControl` driver: each arm binds its native morph, applies it to a duplicated geometry, and projects the `bool` outcome onto the rail; flattening enters through the `Unroller`, `Squisher`, or `MeshUnwrapper` owner, disposing every native flattener through a using scope or lease and draining the caller-owned mark lists into detached records.
- native morph, `MorphControl`, and flattener types stay inside the deformation grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed flatten receipt, or an explicitly owned geometry lease.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` + `Rhino.Geometry.Morphs` host-fidelity deformation and flattening
- Owns: the space-morph engine and its ten deformations, the interactive morph-control deformer, and the unroll, squish, and mesh-unwrap flatteners.
- Accept: native morph and flatten outcomes projected onto `Fin`/`Option`/`Seq` rails, parallel unroll `out` arrays and caller-owned squish mark lists folded into typed receipts, and disposable flatteners leased.
- Reject: re-deriving kernel-altitude DEC flattening or linear motion, exception-style handling of `false` morph or unwrap results, retaining an undisposed native flattener, and leaking host morph/flattener types past the boundary.
