# [RASM_RHINO_API_RHINOCOMMON_SURFACING]

This catalog owns the host-fidelity freeform surface and curve construction boundary: the `NurbsSurface` build set, the `Surface`/`RevSurface`/`SumSurface`/`PlaneSurface` generation family, and the `Curve` host-op family — offset, refine, extend/trim/split, pull/project, and blend/fillet/tween/fit construction over `Curve` and `NurbsCurve`. Every member P/Invokes `rhcommon_c` and returns geometry bit-compatible with Rhino's commands; the boundary never re-derives the kernel-altitude host-neutral NURBS algebra owning evaluation, division, curvature, and tessellation, and routes intersection, iso/contour extraction, and native custody to their own catalogs. Native `bool`+`out` and nullable-or-array outcomes project onto the `LanguageExt` rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon surface-and-curve construction surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Geometry`
- kernel: `Rasm` (host-neutral NURBS evaluation and numeric owners composed by altitude, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: surface-construction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: surface owners

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [CAPABILITY]                                                             |
| :-----: | :------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `NurbsSurface` | native geometry | network, rail-revolve, through-points, curve-on-surface, ruled build     |
|  [02]   | `Surface`      | native base     | extrusion, periodic, soft-edit, rolling-ball fillet, tween-with-sampling |
|  [03]   | `RevSurface`   | native geometry | revolve of a curve or line about an axis with an angle sweep             |
|  [04]   | `SumSurface`   | native geometry | translational-sweep surface from two curves or a curve and direction     |
|  [05]   | `PlaneSurface` | native geometry | bounded plane through a box in a frame                                   |

[PUBLIC_TYPE_SCOPE]: curve owners and carriers

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                                |
| :-----: | :------------------------ | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Curve`                   | native base     | offset, fair/fit/rebuild/smooth/simplify, extend/trim/split, pull, project  |
|  [02]   | `NurbsCurve`              | native geometry | fit-point, spline, spiral, parabola, arc-bezier, subd-friendly construction |
|  [03]   | `CurveBooleanRegions`     | disposable      | planar-boolean region carrier: region/segment/point projections             |
|  [04]   | `RibbonOffsetParameters`  | config carrier  | ribbon-offset distance, blend, rebuild, and surface-method policy           |
|  [05]   | `NurbsCurveFitParameters` | config carrier  | advanced-fit tangent, kink, smoothing, and point-count policy               |

[PUBLIC_TYPE_SCOPE]: closed construction vocabularies

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :-------------------------- | :------------ | :------------------------------- |
|  [01]   | `CurveOffsetCornerStyle`    | enum          | offset corner-join policy        |
|  [02]   | `CurveOffsetEndStyle`       | enum          | offset end-cap policy            |
|  [03]   | `CurveExtensionStyle`       | enum          | extension geometry style         |
|  [04]   | `CurveEnd`                  | enum          | curve-end selector               |
|  [05]   | `SmoothingCoordinateSystem` | enum          | smoothing frame selector         |
|  [06]   | `CurveSimplifyOptions`      | flags enum    | simplify span-rebuild flags      |
|  [07]   | `CurveKnotStyle`            | enum          | interpolation knot spacing       |
|  [08]   | `PreserveEnd`               | enum          | match-curve end-preservation     |
|  [09]   | `RibbonOffsetSurfaceMethod` | enum          | ribbon surface-generation method |

- `[CurveOffsetCornerStyle]`: `None` `Sharp` `Round` `Smooth` `Chamfer`
- `[CurveOffsetEndStyle]`: `None` `Flat` `Round`
- `[CurveExtensionStyle]`: `Line` `Arc` `Smooth`
- `[CurveEnd]`: `None` `Start` `End` `Both`
- `[SmoothingCoordinateSystem]`: `World` `CPlane` `Object`
- `[CurveSimplifyOptions]`: `None` `SplitAtFullyMultipleKnots` `RebuildLines` `RebuildArcs` `RebuildRationals` `AdjustG1` `Merge` `All`
- `[CurveKnotStyle]`: `Uniform` `Chord` `ChordSquareRoot` `UniformPeriodic` `ChordPeriodic` `ChordSquareRootPeriodic`
- `[PreserveEnd]`: `None` `Position` `Tangency` `Curvature`
- `[RibbonOffsetSurfaceMethod]`: `None` `Sweep2` `Sweep2NetworkSrf`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: nurbs-surface build

Members dot off `NurbsSurface`.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `CreateRailRevolvedSurface(Curve, Curve, Line, bool)`                          | static   | revolve a profile along a rail         |
|  [02]   | `CreateFromPoints(IEnumerable<Point3d>, int, int, int, int)`                   | static   | interpolate a control-point grid       |
|  [03]   | `CreateFromCorners(Point3d, Point3d, Point3d, Point3d, double)`                | static   | four-corner bilinear surface           |
|  [04]   | `CreateRuledSurface(Curve, Curve)`                                             | static   | ruled surface between two curves       |
|  [05]   | `CreateSubDFriendly(Surface)`                                                  | static   | rebuild as subd-compatible bicubic     |
|  [06]   | `MakeCompatible(Surface, Surface, out NurbsSurface, out NurbsSurface) -> bool` | static   | reparameterize two to shared structure |
|  [07]   | `MatchToCurve(IsoStatus, Curve, double, double, double, int)`                  | instance | match one iso edge to a target curve   |

- `NurbsSurface.CreateNetworkSurface(IEnumerable<Curve>, int, int, IEnumerable<Curve>, int, int, double, double, double, out int)`: fits through a u/v curve network with per-boundary continuity; `out int error` carries the failure code, and the `(curves, continuity, …)` overload auto-sorts one curve set.
- `NurbsSurface.CreateThroughPoints(IEnumerable<Point3d>, int, int, int, int, bool, bool)`: fits through the points with u/v closure flags.
- `NurbsSurface.CreateCurveOnSurface(Surface, IEnumerable<Point2d>, double, bool) -> NurbsCurve`: geodesic-fit curve through surface uv points; `CreateCurveOnSurfacePoints(surface, fixedPoints, tolerance, periodic, initCount, levels) -> Point2d[]` returns the intermediate uv samples.
- `NurbsSurface.CreateFromPlane(Plane, Interval, Interval, int, int, int, int)`: nurbs form of a plane; `CreateFromCone/Cylinder/Sphere/Torus` build the analytic-primitive nurbs forms.

[ENTRYPOINT_SCOPE]: surface build

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Surface.CreateExtrusion(Curve, Vector3d)`            | static   | linear extrusion surface                   |
|  [02]   | `Surface.CreateExtrusionToPoint(Curve, Point3d)`      | static   | tapered-to-apex extrusion surface          |
|  [03]   | `Surface.CreatePeriodicSurface(Surface, int, bool)`   | static   | close a surface periodically               |
|  [04]   | `RevSurface.Create(Curve, Line, double, double)`      | static   | revolve a profile through an angle sweep   |
|  [05]   | `SumSurface.Create(Curve, Vector3d)`                  | static   | translational sweep of a curve             |
|  [06]   | `Surface.Fit(int, int, double)`                       | instance | fit to a lower knot count within tolerance |
|  [07]   | `Surface.Rebuild(int, int, int, int) -> NurbsSurface` | instance | rebuild to fixed point count and degree    |
|  [08]   | `PlaneSurface.CreateThroughBox(Plane, BoundingBox)`   | static   | bounded plane sized to span a box          |

- `Surface.CreateSoftEditSurface(Surface, Point2d, Vector3d, double, double, double, bool)`: falloff-weighted local push of a surface point.
- `Surface.CreateRollingBallFillet(Surface, bool, Surface, bool, double, double) -> Surface[]`: rolling-ball fillet between two surfaces; a `(surfaceA, uvA, surfaceB, uvB, …)` overload seeds by uv and a `(surfaceA, surfaceB, …)` overload picks sides automatically.
- `Surface.CreateTweenSurfacesWithSampling(Surface, Surface, int, int, double) -> Surface[]`: intermediate surfaces sampled between two inputs.
- `Surface.RebuildOneDirection(int, int, LoftType, double) -> NurbsSurface`: rebuild one direction under a loft type.
- `Surface.VariableOffset(double, double, double, double, double)`: corner-distance offset, `null` on failure; the `(…, IEnumerable<Point2d>, IEnumerable<double>, double)` interior overload refuses mismatched parameter/distance counts.
- `RevSurface.Create(Line, …)` and no-angle overloads cover the degenerate line-profile and full-revolution cases; `CreateFromCone/Cylinder/Sphere/Torus` build the analytic revolutions.
- `SumSurface.Create(Curve, Curve)`: sums two curves into the translational surface.
- `PlaneSurface.CreateThroughBox(Line, Vector3d, BoundingBox)`: frames the bounded plane by a line and in-plane vector.

[ENTRYPOINT_SCOPE]: curve offset and pull

Members dot off `Curve`.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `OffsetNormalToSurface(Surface, double) -> Curve`  | instance | lift a surface-lying curve by normal  |
|  [02]   | `OffsetTangentToSurface(Surface, double) -> Curve` | instance | lift a surface-lying curve by tangent |
|  [03]   | `PullToBrepFace(BrepFace, double) -> Curve[]`      | instance | project onto a face along its normals |
|  [04]   | `PullToMesh(Mesh, double) -> PolylineCurve`        | instance | pull onto a mesh as a polyline        |

- `Curve.Offset(Point3d, Vector3d, double, double, double, bool, CurveOffsetCornerStyle, CurveOffsetEndStyle) -> Curve[]`: planar offset with corner/end policy and a loose flag; the `(Plane, distance, tolerance, cornerStyle)` overload offsets in a plane.
- `Curve.OffsetOnSurface(BrepFace, double[], double[], double) -> Curve[]`: variable-distance offset across a face keyed by parameter/distance parallel arrays; constant-distance and through-point overloads accept `Surface` or `BrepFace`.
- `Curve.PullToBrepFace(Curve, BrepFace, double, bool) -> Curve[]`: static loose-fit form.
- `Curve.PullToMesh(Mesh, double, bool) -> Curve`: loose-fit smooth-curve overload.

[ENTRYPOINT_SCOPE]: curve refine

Members dot off `Curve`.

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Fair(double, double, int, int, int) -> Curve`                                      | instance | smooth within a deviation budget        |
|  [02]   | `Fit(int, double, double) -> Curve`                                                 | instance | refit to a lower knot count             |
|  [03]   | `Rebuild(int, int, bool) -> NurbsCurve`                                             | instance | rebuild to fixed point count and degree |
|  [04]   | `Smooth(double, bool, bool, bool, bool, SmoothingCoordinateSystem, Plane) -> Curve` | instance | per-axis smoothing in a frame           |
|  [05]   | `Simplify(CurveSimplifyOptions, double, double) -> Curve`                           | instance | replace spans with lines/arcs per flags |
|  [06]   | `RemoveShortSegments(double) -> bool`                                               | instance | in-place short-segment removal          |
|  [07]   | `MakeClosed(double) -> bool`                                                        | instance | in-place gap closure                    |

- `Curve.Smooth(double, bool, bool, bool, bool, SmoothingCoordinateSystem) -> Curve`: no-plane overload smooths in world or cplane.
- `Curve.SimplifyEnd(CurveEnd, …) -> Curve`: scopes the simplify to one end.

[ENTRYPOINT_SCOPE]: curve extend/trim/split

Members dot off `Curve`.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Extend(CurveEnd, double, CurveExtensionStyle) -> Curve` | instance | extend one end by length with a style |
|  [02]   | `ExtendOnSurface(CurveEnd, BrepFace) -> Curve`           | instance | extend across a trimmed face          |
|  [03]   | `ExtendOnSurface(CurveEnd, Surface) -> Curve`            | instance | extend across an untrimmed surface    |
|  [04]   | `Trim(Interval) -> Curve`                                | instance | sub-domain extraction                 |
|  [05]   | `Trim(CurveEnd, double) -> Curve`                        | instance | trim one end by length                |
|  [06]   | `TrimInterval(Interval) -> bool`                         | instance | in-place interval trim                |
|  [07]   | `Split(Brep, double, double) -> Curve[]`                 | instance | split at intersections with a cutter  |
|  [08]   | `Split(Plane, double, double) -> Curve[]`                | instance | split at a plane cutter               |

- `Curve.Extend(CurveEnd, CurveExtensionStyle, IEnumerable<GeometryBase>) -> Curve` and `Extend(CurveEnd, …, Point3d) -> Curve`: extend to bounding geometry or a point.
- `Curve.ExtendByLine(CurveEnd, IEnumerable<GeometryBase>) -> Curve` / `ExtendByArc(CurveEnd, IEnumerable<GeometryBase>) -> Curve`: line and arc extension to bounding geometry, `null` on failure.
- `Curve.Split(double) -> Curve[]` and `Split(IEnumerable<double>) -> Curve[]`: split at parameters; the `Brep` cutter form also accepts a surface, plane, or parameter set.

[ENTRYPOINT_SCOPE]: curve construct

Members dot off `Curve`.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `CreateMeanCurve(Curve, Curve, double) -> Curve`                        | static  | average two curves into a mean curve   |
|  [02]   | `CreateTweenCurves(Curve, Curve, int, double) -> Curve[]`               | static  | intermediate curves between two inputs |
|  [03]   | `CreateArcBlend(Point3d, Vector3d, Point3d, Vector3d, double) -> Curve` | static  | direction-seeded arc blend             |
|  [04]   | `MakeEndsMeet(Curve, bool, Curve, bool) -> bool`                        | static  | endpoint reconciliation of two curves  |
|  [05]   | `CreatePeriodicCurve(Curve, bool) -> Curve`                             | static  | periodic closure of a curve            |

- `Curve.CreateBlendCurve(Curve, double, bool, BlendContinuity, Curve, double, bool, BlendContinuity) -> Curve`: blend between two curves at stations with per-end continuity; a `(curveA, curveB, continuity, bulgeA, bulgeB)` overload blends end-to-end with bulge control; `CreateArcLineArcBlend(Point3d, Vector3d, Point3d, Vector3d, double) -> Curve` is the arc-line-arc form.
- `Curve.CreateMatchCurve(Curve, bool, BlendContinuity, Curve, bool, PreserveEnd, bool) -> Curve[]`: match one curve's end to another under continuity and end-preservation policy.
- `Curve.CreateFilletCurves(Curve, Point3d, Curve, Point3d, double, bool, bool, bool, double, double) -> Curve[]`: fillet arc between two curves picked near seed points; `CreateFillet(Curve, Curve, double, double, double) -> Arc` returns the raw arc and `CreateFilletCornersCurve(Curve, double, double, double) -> Curve` fillets every corner of one curve.
- `Curve.CreateTweenCurvesWithMatching(…)` reparameterizes first; `CreateTweenCurvesWithSampling(…, int, …)` samples for dissimilar inputs.
- `Curve.ProjectToBrep(IEnumerable<Curve>, IEnumerable<Brep>, Vector3d, double, out int[], out int[]) -> Curve[]`: project onto breps along a direction with source-index maps; single-curve/single-brep overloads drop the maps.
- `Curve.ProjectToMesh(IEnumerable<Curve>, IEnumerable<Mesh>, Vector3d, double) -> Curve[]` / `ProjectToPlane(Curve, Plane) -> Curve`: projection onto meshes and a plane.
- `Curve.FilletSurfaceToRail(BrepFace, BrepFace, double, double, int, int, IEnumerable<double>, int, bool, FilletSurfaceSplitType, double, List<Brep>, List<Brep>, List<Brep>, out double[]) -> bool`: an instance method on the rail `Curve` receiver whose fillet, trimmed-input, and fit outputs land in the caller-owned lists.

[ENTRYPOINT_SCOPE]: nurbs-curve fit

Members dot off `NurbsCurve`.

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `CreateHSpline(IEnumerable<Point3d>, Vector3d, Vector3d)` | static  | interpolating H-spline under tangents |
|  [02]   | `CreateParabolaFromVertex(Point3d, Point3d, Point3d)`     | static  | vertex-seeded parabola                |
|  [03]   | `CreateParabolaFromFocus(Point3d, Point3d, Point3d)`      | static  | focus-seeded parabola                 |
|  [04]   | `CreateFromArc(Arc, int, int)`                            | static  | arc-to-nurbs with span control        |
|  [05]   | `CreateFromLine(Line)`                                    | static  | line-to-nurbs                         |
|  [06]   | `Create(bool, int, IEnumerable<Point3d>)`                 | static  | control-point constructor             |

- `NurbsCurve.CreateFromFitPoints(IEnumerable<Point3d>, double, int, bool, Vector3d, Vector3d)`: fit through points within tolerance under tangent constraints; the `(points, tolerance, periodic)` overload drops the tangents.
- `NurbsCurve.CreateSpiral(Curve, double, double, Point3d, double, double, double, double, int)`: spiral along a rail; the `(Point3d, Vector3d, …)` overload spirals about an axis.
- `NurbsCurve.CreateNonRationalArcBezier(int, Point3d, Point3d, Point3d, double, double, double)`: non-rational arc-bezier construction.
- `NurbsCurve.CreateSubDFriendly(Curve)` / `CreateSubDFriendly(Curve, int, bool)`: subd-compatible rebuild; the bare form derives point count and closure from the source, the structured form fixes control-point count and periodic closure (`pointCount >= 6` when periodic, `>= 4` otherwise), and a `(IEnumerable<Point3d>, bool, bool)` overload builds from points.
- `NurbsCurve.MakeCompatible(IEnumerable<Curve>, Point3d, Point3d, int, int, double, double) -> NurbsCurve[]`: reparameterize a set to a shared structure for lofting.
- `NurbsCurve.CreateParabolaFromPoints(Point3d, Point3d, Point3d)` / `CreateFromCircle(Circle)` / `CreateFromEllipse(Ellipse)`: the three-point parabola and analytic-primitive constructors.

[ENTRYPOINT_SCOPE]: curve statics and carriers

Members dot off `Curve`.

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `CreateControlPointCurve(IEnumerable<Point3d>, int) -> Curve`         | static  | control-point fitting             |
|  [02]   | `CreateSoftEditCurve(Curve, double, Vector3d, double, bool) -> Curve` | static  | falloff-weighted local point push |
|  [03]   | `DoDirectionsMatch(Curve, Curve) -> bool`                             | static  | direction-agreement predicate     |
|  [04]   | `CreateBooleanIntersection(Curve, Curve, double) -> Curve[]`          | static  | planar curve intersection         |
|  [05]   | `CreateBooleanDifference(Curve, Curve, double) -> Curve[]`            | static  | planar curve difference           |

- `Curve.CreateInterpolatedCurve(IEnumerable<Point3d>, int[, CurveKnotStyle[, Vector3d, Vector3d]]) -> Curve`: interpolation is a `Curve` static, never a `NurbsCurve` member.
- `Curve.JoinCurves(IEnumerable<Curve>[, double[, bool[, bool], out int[]]]) -> Curve[]`: join with an optional result-to-source key map.
- `Curve.CreateBooleanUnion(IEnumerable<Curve>, double[, out int[]]) -> Curve[]` / `CreateBooleanDifference(Curve, IEnumerable<Curve>, double) -> Curve[]`: planar booleans keyed by tolerance, an optional result-to-source index map on union.
- `Curve.CreateBooleanRegions(IEnumerable<Curve>, Plane[, IEnumerable<Point3d>], bool, double) -> CurveBooleanRegions`: yields the disposable region carrier.
- `Curve.CreateCatenaryCurveFromApex(Point3d, Point3d, Vector3d, <shape arg>, bool, int, out Point3d, out double, out double, out double) -> Curve`: mints the hanging-curve family, one static per shape terminal (`ThroughPoint`/`FromLength`/`FromParameter`/`FromApex`) carrying apex, parameter, length, and deviation evidence.
- `Curve.CreateCurve2View(Curve, Curve, Vector3d, Vector3d, double, double) -> Curve[]`: two-view intersection construction; `CreateTextOutlines(string, string, double, int, bool, Plane, double, double) -> Curve[]` mints glyph outlines.
- `Curve.RibbonOffset(RibbonOffsetParameters, out Curve[], out Curve[], out Brep[]) -> Curve`: carrier-driven ribbon offset; scalar `(distance, blendRadius, directionPoint, normal, tolerance, …)` overloads predate it.
- `Curve.CreateNurbsCurveFit(Curve, Interval, NurbsCurveFitParameters, out Line, out double, out double) -> NurbsCurve`: advanced fit over its carrier.
- `[RIBBON_OFFSET_KNOBS]`: `OffsetDistance` `OffsetLocation` `OffsetTolerance` `OffsetPlaneVector3d` (`Vector3d.Unset` default) `BlendRadius` `RebuildPointCount` (0 disables) `RefitTolerance` (0 disables) `AlignCrossSections` `RibbonSurfaceGenerationMethod`
- `[NURBS_FIT_KNOBS]`: `TangentMatching` `Degree` `PointCount` `SubDFriendly` `KinkAngleRadians` `KinkAngleDegrees` `KinkSplitting` `SmoothingIntensity` `UniformityIntensity` `CurvatureBiasIntensity` `SmoothingCoefficient` `UniformityCoefficient` `CurvatureBiasCoefficient` `Closed` `ApplyTangentMatchingAtKinks` `OptimizeCurve` `PointCountRange`
- `[NURBS_FIT_ENUMS]`: `TangentMatch` (`None` `AtStart` `AtEnd` `AtStartAndEnd`) · `KinkSplit` (`None` `AtG1Changes` `AtLargeG2Changes` `AtMediumG2Changes` `AtSmallG2Changes`) · `Intensity` (`None` `Low` `Moderate` `Medium` `High` `Extreme` `Custom`)
- `NurbsCurveFitParameters` is `ICloneable`, `IDisposable`; `RibbonOffsetParameters` carries all get/set.
- `CurveBooleanRegions` reads: `RegionCount`/`PointCount`/`PlanarCurveCount -> int`, `RegionCurves(int) -> Curve[]`, `RegionPointIndex(int) -> int`, `BoundaryCount(int)`/`SegmentCount(int, int)`/`SegmentDetails(int, int, int, out Interval, out bool) -> int`, `PlanarCurve(int) -> Curve`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NurbsSurface` construction returns `null` on failure and carries diagnostics through `out int error` for the network and curve-on-surface builds; `MakeCompatible` and `MatchToCurve` reparameterize to shared or matched structure, and analytic-primitive nurbs forms feed the loft and network builders a compatible representation.
- `Curve` host ops split by intent: offset/pull produce parallel geometry, fair/fit/rebuild/smooth/simplify refine in place or return a refined copy, extend/trim/split change extent by parameter or bounding geometry, and pull/project map a curve onto a target; variable-distance offset and multi-input projection carry parameter/distance and source-index parallel arrays.
- freeform surface generation stays host-fidelity: `CreateNetworkSurface`, `CreateRailRevolvedSurface`, `CreateSoftEditSurface`, and `CreateRollingBallFillet` produce Rhino-command-identical geometry, distinct from the kernel's host-neutral surface algebra owning evaluation and division at a lower altitude.

[STACKING]:
- `LanguageExt.Core`(`api-languageext`): a nullable single build lifts to `Option<NurbsSurface>`/`Option<Curve>`, a possibly-null-or-empty `Curve[]`/`Surface[]` lands as `Seq<A>`, a `bool`-with-`out` op folds into a `Fin` keyed to the payload, the `out int error` network code and the projection source-index maps fold into a typed construction receipt, and the rolling-ball and tween arrays project as `Seq<Surface>`.
- `Thinktecture.Runtime.Extensions`(`api-thinktecture-runtime-extensions`): the closed policy vocabularies — `CurveOffsetCornerStyle`, `CurveOffsetEndStyle`, `CurveExtensionStyle`, `CurveEnd`, `SmoothingCoordinateSystem`, and the `[Flags]` `CurveSimplifyOptions` — wrap as `[SmartEnum<TKey>]`/`[Flags]`-backed owners; the curve host op models as a `[Union]` over the offset, refine, extend/trim/split, pull/project, and construct arms.
- `Rasm` kernel: host-neutral NURBS evaluation, division, curvature, and tessellation stand at the kernel altitude and the boundary re-derives none of them; degrees, tolerances, angles, and station parameters compose the kernel numeric owners before the native call.

[LOCAL_ADMISSION]:
- construction enters through the surface or curve op union: each arm binds its native member, projects the outcome onto the rail, and pairs parameter/distance or curve/brep parallel arrays into equal-cardinality rows before the native call; the caller-owned fillet output lists for `FilletSurfaceToRail` drain into detached brep records.
- native `Surface`, `NurbsSurface`, `Curve`, and `NurbsCurve` values stay inside the construction grant; downstream code receives duplicated canonical geometry keyed by content hash, the typed construction receipt, or an explicitly owned geometry lease.

[RAIL_LAW]:
- Package: `RhinoCommon` (`Rhino.Geometry` freeform surface and curve host-fidelity construction)
- Owns: nurbs-surface network/rail-revolve/through-points/curve-on-surface/ruled build, the surface/rev/sum/plane generation set, and the curve offset, refine, extend/trim/split, pull/project, and blend/fillet/tween/fit host ops.
- Accept: native surface and curve outcomes projected onto `Fin`/`Option`/`Seq` rails, parameter/distance and source-index parallel arrays paired into rows, `out`-error and index maps folded into typed receipts.
- Reject: re-deriving kernel-altitude NURBS evaluation and division, exception-style handling of null construction results, unpaired parallel-array inputs, and leaking host surface/curve types past the boundary.
