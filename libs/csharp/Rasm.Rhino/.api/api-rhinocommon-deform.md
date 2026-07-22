# [RASM_RHINO_API_RHINOCOMMON_DEFORM]

This catalog owns the host-fidelity nonlinear deformation and flattening boundary: the `SpaceMorph` engine, its `Rhino.Geometry.Morphs` implementations, the `MorphControl` deformer, and the `Unroller`/`Squisher`/`MeshUnwrapper` flatteners. A morph deforms any `GeometryBase` in place along a type-agnostic point map; the flatteners are disposable native resources. Every engine P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's commands; the boundary never re-derives kernel-altitude DEC UV-flattening or linear motion.

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

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
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

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Unroller`         | flattener      | unroll developable surfaces/breps with following geometry        |
|  [02]   | `Squisher`         | disposable     | stress-relaxed flatten of surfaces and meshes with mark tracking |
|  [03]   | `SquishParameters` | config carrier | spring constants, deformation mode, topology and mapping policy  |
|  [04]   | `MeshUnwrapper`    | disposable     | uv-unwrap a mesh set by an unwrap algorithm                      |

[PUBLIC_TYPE_SCOPE]: closed flattening vocabularies

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :-------------------------- | :------------ | :----------------------------------- |
|  [01]   | `MeshUnwrapMethod`          | enum          | uv-unwrap algorithm selector         |
|  [02]   | `SquishFlatteningAlgorithm` | enum          | geometric or physical-stress flatten |
|  [03]   | `SquishDeformation`         | enum          | stretch/compress deformation mode    |

- `[MeshUnwrapMethod]`: `LSCM` `ABFPP` `ARAP`
- `[SquishFlatteningAlgorithm]`: `Geometric` `PhysicalStress`
- `[SquishDeformation]`: `Free` `StretchMostly` `StretchOnly` `CompressMostly` `CompressOnly` `Custom`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: space-morph engine

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `SpaceMorph.Morph(GeometryBase) : bool`       | instance | deform any geometry in place         |
|  [02]   | `SpaceMorph.Morph(ref Plane) : bool`          | instance | morph a plane frame by origin/axes   |
|  [03]   | `SpaceMorph.MorphPoint(Point3d) : Point3d`    | instance | abstract per-point deformation map   |
|  [04]   | `SpaceMorph.IsMorphable(GeometryBase) : bool` | static   | gate a geometry kind before morphing |

- `[SPACE_MORPH_KNOBS]`: `Tolerance` `QuickPreview` `PreserveStructure`
- `SpaceMorph.Morph`: a `false` return marks a geometry the morph rejected.

[ENTRYPOINT_SCOPE]: morph constructors

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `BendSpaceMorph(Point3d, Point3d, Point3d, double, bool, bool)` | ctor    | bend about the start-end spine        |
|  [02]   | `FlowSpaceMorph(Curve, Curve, bool, bool, bool)`                | ctor    | reflow base curve onto target curve   |
|  [03]   | `MaelstromSpaceMorph(Plane, double, double, double)`            | ctor    | spiral between two radii              |
|  [04]   | `SplopSpaceMorph(Plane, Surface, Point2d, double, double)`      | ctor    | map a plane region onto surface uv    |
|  [05]   | `SporphSpaceMorph(Surface, Surface, Point2d, Point2d)`          | ctor    | reflow one surface onto another       |
|  [06]   | `StretchSpaceMorph(Point3d, Point3d, double)`                   | ctor    | axial stretch to a length             |
|  [07]   | `TaperSpaceMorph(Point3d, Point3d, double, double, bool, bool)` | ctor    | radial taper between two radii        |
|  [08]   | `TwistSpaceMorph()`                                             | ctor    | rotate about an axis per length       |
|  [09]   | `MeshCageMorph(Mesh, Mesh)`                                     | ctor    | cage-deform via reference/target mesh |

- `[SPORPH_KNOBS]`: `ConstrainNormal`
- `[TWIST_KNOBS]`: `TwistAxis` `TwistAngleRadians` `InfiniteTwist`
- `BendSpaceMorph(Point3d, Point3d, Point3d, bool, bool)`: no-angle overload bends toward the third point.
- `FlowSpaceMorph(Curve, Curve, bool)`: overload drops the two reversal flags.
- `SplopSpaceMorph`: `(Plane, Surface, Point2d)` and `(…, double scale)` overloads drop rotation and scale.
- `StretchSpaceMorph(Point3d, Point3d, Point3d)`: overload stretches to a point.
- `SporphSpaceMorph(Surface, Surface)`: overload auto-aligns the uv params.

[ENTRYPOINT_SCOPE]: interactive morph control

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------- | :------- | :---------------------------- |
|  [01]   | `MorphControl(NurbsCurve, NurbsCurve)`    | ctor     | origin-to-target curve driver |
|  [02]   | `MorphControl.Morph(GeometryBase) : bool` | instance | apply the control deformation |

- `[MORPH_CONTROL_KNOBS]`: `SpaceMorphTolerance` `QuickPreview` `PreserveStructure` `Curve` `Surface`

[ENTRYPOINT_SCOPE]: unroll flattener

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Unroller(Brep)`                                                             | ctor     | unroller over a developable brep     |
|  [02]   | `Unroller(Surface)`                                                          | ctor     | unroller over a developable surface  |
|  [03]   | `Unroller.AddFollowingGeometry(...)`                                         | instance | carry geometry into the flat frame   |
|  [04]   | `Unroller.PerformUnroll(out Curve[], out Point3d[], out TextDot[]) : Brep[]` | instance | unroll to flattened breps + geometry |
|  [05]   | `Unroller.FollowingGeometryIndex(Curve) : int`                               | instance | index of a carried curve or dot      |

- `[UNROLL_KNOBS]`: `ExplodeOutput` `ExplodeSpacing` `AbsoluteTolerance` `RelativeTolerance`
- `Unroller.AddFollowingGeometry`: overloads cover `Curve`, `Point3d`, `Point`, `TextDot`, their `IEnumerable` forms, and `(Point3d, string)` dot pairs, so a dot row needs no minted `TextDot`.
- `Unroller.PerformUnroll(List<Brep>)`: overload returns a follow-geometry count.
- `Unroller.FollowingGeometryIndex`: a `TextDot` overload indexes carried dots.

[ENTRYPOINT_SCOPE]: squish flattener

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Squisher()`                                                          | ctor     | disposable squisher                   |
|  [02]   | `Squisher.SquishSurface(SquishParameters, Surface) : Brep`            | instance | stress-relax a surface to a flat brep |
|  [03]   | `Squisher.SquishMesh(SquishParameters, Mesh) : Mesh`                  | instance | flatten a mesh with mark tracking     |
|  [04]   | `Squisher.SquishCurve(Curve) : PolylineCurve`                         | instance | map a curve into the flat frame       |
|  [05]   | `Squisher.SquishTextDot(TextDot) : TextDot`                           | instance | map a dot into the flat frame         |
|  [06]   | `Squisher.SquishPoint(Point3d, out Point3d) : bool`                   | instance | map a point into the flat frame       |
|  [07]   | `Squisher.Is2dPatternSquished(GeometryBase) : bool`                   | static   | probe a squished pattern              |
|  [08]   | `Squisher.SquishBack2dMarks(GeometryBase, IEnumerable<GeometryBase>)` | static   | inverse-map flattened marks to 3d     |

- `[SQUISH_READS]`: `Get2dMesh` `Get3dMesh` `GetLengthConstrained2dLines` `GetLengthConstrained3dLines` `GetAreaConstrainedTrianglesIndices`
- `Squisher.SquishSurface`/`SquishMesh`: a `(…, IEnumerable<GeometryBase>, List<GeometryBase>)` overload maps marks into a caller-owned list.
- `Squisher.GetMesh2dEdges`/`GetMesh3dEdges`: alias the length-constrained line reads and return identical data, so a consumer reads one pair.

[ENTRYPOINT_SCOPE]: squish policy carrier

| [INDEX] | [SURFACE]                                                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `SquishParameters.Default : SquishParameters`                                              | factory  | the standing preset              |
|  [02]   | `SquishParameters.SetDeformation(SquishDeformation, bool, double, double, double, double)` | instance | set deformation mode + constants |
|  [03]   | `SquishParameters.SetSpringConstants(double, double)`                                      | instance | set boundary/deformation bias    |
|  [04]   | `SquishParameters.GetSpringConstants(out double, out double) : bool`                       | instance | read the spring biases           |

- `[SQUISH_KNOBS]`: `PreserveTopology` `SaveMapping` `BoundaryStretchConstant` `BoundaryCompressConstant` `InteriorStretchConstant` `InteriorCompressConstant` `AbsoluteLimit` `Algorithm`
- `SquishParameters.SetDeformation`: set-only, no read-back property; the carrier is `IDisposable`.

[ENTRYPOINT_SCOPE]: mesh uv unwrap

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `MeshUnwrapper(Mesh)`                           | ctor     | unwrapper over one mesh        |
|  [02]   | `MeshUnwrapper(IEnumerable<Mesh>)`              | ctor     | unwrapper over a mesh set      |
|  [03]   | `MeshUnwrapper.Unwrap(MeshUnwrapMethod) : bool` | instance | compute uv onto texture coords |

- `[UNWRAP_KNOBS]`: `SymmetryPlane` (set-only, pins a symmetry constraint the unwrap consumes)
- `MeshUnwrapper`: disposable native resource.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SpaceMorph` is a type-agnostic engine: `Morph(GeometryBase)` deforms any geometry along the concrete `MorphPoint` map in place, so the morphs discriminate on deformation kind rather than geometry type, and `IsMorphable` gates a geometry kind before the call. Every concrete morph is `IDisposable` carrying `Dispose()`, `IsValid`, and the `MorphPoint(Point3d)` override, while the `SpaceMorph` base is not `IDisposable`; `MeshMorphMesh` is `internal`, leaving `MeshCageMorph` the sole public mesh-driven deformer.
- `MorphControl` holds a persistent origin-to-target driver and morphs against it repeatedly, distinct from the one-shot morph subclasses; it is a `GeometryBase` and follows the geometry catalog's custody rules.
- flattening is a distinct host-fidelity altitude from the kernel DEC UV-flatten: `Unroller` unrolls developable surfaces carrying following geometry, `Squisher` stress-relaxes non-developable surfaces and meshes under a `SquishParameters` spring model with forward and inverse mark mapping, and `MeshUnwrapper` computes uv by an unwrap algorithm; `Squisher` and `MeshUnwrapper` are native `IDisposable` resources.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a `bool` morph or unwrap folds into a `Fin<Unit>` keyed to the mutated geometry, a nullable squish or unroll result lifts to `Option<Brep>`/`Option<Mesh>`, the unroll `Brep[]` and carried-geometry arrays land as `Seq<A>`, and the caller-owned squish mark lists and the `PerformUnroll` parallel `out` arrays fold into one detached flatten receipt.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed flattening vocabularies — `MeshUnwrapMethod`, `SquishFlatteningAlgorithm`, and `SquishDeformation` — wrap as `[SmartEnum<TKey>]` owners; the morph kind models as a `[Union]` over the morph-kind arms, each binding its defining-parameter carrier.
- `Rasm` kernel: host-neutral DEC UV-flattening and linear-motion transforms stand at the kernel altitude and the boundary re-derives none of them; radii, angles, lengths, spring constants, and tolerances compose the kernel numeric and unit owners before the native call.

[LOCAL_ADMISSION]:
- deformation enters through the morph union or the `MorphControl` driver: each arm binds its native morph, applies it to a duplicated geometry, and projects the `bool` outcome onto the rail; flattening enters through the `Unroller`, `Squisher`, or `MeshUnwrapper` owner, disposing every native flattener through a using scope or lease and draining the caller-owned mark lists into detached records.
- native morph, `MorphControl`, and flattener types stay inside the deformation grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed flatten receipt, or an explicitly owned geometry lease.

[RAIL_LAW]:
- Surface: `Rhino.Geometry` + `Rhino.Geometry.Morphs` host-fidelity deformation and flattening
- Owns: the space-morph engine and its deformation family, the interactive morph-control deformer, and the unroll, squish, and mesh-unwrap flatteners.
- Accept: native morph and flatten outcomes projected onto `Fin`/`Option`/`Seq` rails, parallel unroll `out` arrays and caller-owned squish mark lists folded into typed receipts, and disposable flatteners leased.
- Reject: re-deriving kernel-altitude DEC flattening or linear motion, exception-style handling of `false` morph or unwrap results, retaining an undisposed native flattener, and leaking host morph/flattener types past the boundary.
