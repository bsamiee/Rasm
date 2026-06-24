# [RASM_FABRICATION_API_GEOMETRY3SHARP]

`geometry3Sharp` (gradientspace, Boost-1.0/BSL-1.0, pure-managed `netstandard2.0`) is a HEAVY 2D/3D geometric-computation and mesh library already admitted centrally (the `Rasm.Bim` mesh-text importer owner); the fabrication folder reuses ONLY its `g3.BiArcFit2` biarc-fitting surface plus the `Arc2d`/`Segment2d`/`Vector2d` curve primitives it produces, at the `Posting/program#CUT_PROGRAM` `Biarc` projection, to fit a linear `Move`/contour chord run into two `G1`-tangent-continuous circular arcs the dialect-neutral `CutProgram` emits as `G2`/`G3` `ArcCw`/`ArcCcw` words. The admission is FIREWALLED to the biarc/curve-fit rail — the `DMesh3`/mesh-boolean/`MeshSignedDistanceGrid` half of the library never crosses into this folder or the kernel `Rasm.Geometry` strata owner. `BiArcFit2` ports the Ryan Juckett biarc interpolation: given two points and two tangents it fits two `Arc2d` (or degenerate `Segment2d`) meeting `G1`-tangent-continuous at a computed junction. No native asset and no RID burden — the package is managed IL, ALC-safe, zero native dependency. The namespace is `g3`. The NewWheelTech `geometry4Sharp` fork is REJECTED as a duplicate: this same package owns the identical surface, so no second fork is admitted.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp`
- version: `1.0.324` (the newest release; the `geometry4Sharp` fork stalls at `1.0.0` and is rejected)
- license: `Boost-1.0` (BSL-1.0)
- assembly: `geometry3Sharp`
- namespace: `g3`
- asset: pure-managed AnyCPU IL `netstandard2.0`/`net45` (no native asset, no RID burden)
- rail: fabrication (shared central pin with the `Rasm.Bim` mesh-text importer)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: biarc fit and curve primitives (the SOLE admitted surface)
- rail: fabrication

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :------------ | :-------------- | :------------------------------------------------------- |
|  [01]   | `BiArcFit2`   | fitter class    | fit two `G1`-continuous arcs to a point/tangent pair     |
|  [02]   | `Arc2d`       | curve class     | a 2D circular arc (`Center`/`Radius`/`AngleStartDeg`/`AngleEndDeg`/`IsReversed`, `SampleT`/`ArcLength`) implementing `IParametricCurve2d` |
|  [03]   | `Segment2d`   | curve struct    | a 2D line segment (`Center`/`Direction`/`Extent` form, `P0`/`P1`/`Length`) implementing `IParametricCurve2d` |
|  [04]   | `Vector2d`    | vector struct   | the 2D point/tangent carrier (`.x`/`.y` fields, `.Normalized`/`Normalize`/`Dot`/`Distance`) |

[PUBLIC_TYPE_SCOPE]: `BiArcFit2` members
- rail: fabrication

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2)` | constructor | fit the biarc to the endpoint/tangent pair                   |
|  [02]   | `BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2, double d1)` | constructor | the explicit-fit-parameter overload                          |
|  [03]   | `Arc2d Arc1` / `Arc2d Arc2`                                                       | field           | the two fitted arcs (valid when the segment flag is false)   |
|  [04]   | `bool Arc1IsSegment` / `bool Arc2IsSegment`                                       | field           | true when the fitted span degenerated to a straight segment  |
|  [05]   | `Segment2d Segment1` / `Segment2d Segment2`                                       | field           | the fitted segment (valid when the matching `*IsSegment` flag is true) |
|  [06]   | `Vector2d Point1` / `Point2`                                                      | field           | the input endpoints                                          |
|  [07]   | `Vector2d Tangent1` / `Tangent2`                                                  | field           | the input tangents                                          |
|  [08]   | `double FitD1` / `FitD2`                                                          | field           | the solved fit parameters                                    |
|  [09]   | `double Epsilon`                                                                  | field           | the fit tolerance (default `1E-08`)                          |
|  [10]   | `List<IParametricCurve2d> Curves` / `IParametricCurve2d Curve1` / `Curve2`        | property        | the polymorphic curve list (arc-or-segment per span)         |
|  [11]   | `double Distance(Vector2d p)` / `Vector2d NearestPoint(Vector2d p)`               | method          | the query-point deviation from / closest point on the fitted biarc — the emit-quality gate |

[PUBLIC_TYPE_SCOPE]: `Arc2d` / `Segment2d` members
- rail: fabrication

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `Arc2d(Vector2d center, double radius, double startDeg, double endDeg)` | constructor | construct an arc from center/radius/sweep-degrees        |
|  [02]   | `Arc2d(Vector2d vCenter, Vector2d vStart, Vector2d vEnd)`      | constructor     | the center+start+end-point overload (`SetFromCenterAndPoints`) |
|  [03]   | `Vector2d Arc2d.Center` / `double Radius`                      | field           | the arc center and radius                                |
|  [04]   | `double Arc2d.AngleStartDeg` / `AngleEndDeg`                   | field           | the arc sweep angles (degrees)                           |
|  [05]   | `bool Arc2d.IsReversed`                                        | field           | the sweep direction (CW when true → `G2`, CCW → `G3`)    |
|  [06]   | `Vector2d Arc2d.SampleT(double t)`                             | method          | the arc point at parameter `t∈[0,1]` (`SampleT(0)`=start, `SampleT(1)`=end) |
|  [07]   | `Vector2d Arc2d.P0` / `P1`                                     | property        | start/end endpoints, defined as `SampleT(0.0)`/`SampleT(1.0)` |
|  [08]   | `double Arc2d.ArcLength`                                       | property        | the swept arc length `(AngleEndDeg−AngleStartDeg)·(π/180)·Radius` — the feedrate/tab-spacing length |
|  [09]   | `Segment2d(Vector2d p0, Vector2d p1)`                          | constructor     | the endpoint-pair overload `set_output` builds the degenerate span from |
|  [10]   | `Vector2d Segment2d.P0` / `P1`                                 | property        | the segment endpoints (`Center ∓ Extent·Direction`)     |
|  [11]   | `Vector2d Segment2d.Center` / `Direction` / `double Extent`    | field           | the segment center-direction-extent form                 |
|  [12]   | `double Segment2d.Length`                                      | property        | the segment length `2·Extent` — the straight-span feed length |

[PUBLIC_TYPE_SCOPE]: `Vector2d` members (the point/tangent carrier)
- rail: fabrication

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `Vector2d(double x, double y)`                                 | constructor     | the primary point/tangent builder off raw `Point3d.X`/`Y` |
|  [02]   | `double Vector2d.x` / `y`                                      | field           | the raw component fields (lowercase) the `I`/`J` word reads |
|  [03]   | `Vector2d Vector2d.Normalized`                                 | property        | the unit-copy used to build the input tangents (`Zero` when degenerate) |
|  [04]   | `double Vector2d.Normalize(double epsilon = 2.22E-16)`         | method          | in-place unit-scale RETURNING the prior length — a `0.0` return flags a zero (degenerate) tangent |
|  [05]   | `double Vector2d.Length` / `LengthSquared`                     | property        | the magnitude, the chord-length the fit/gate reads       |
|  [06]   | `double Vector2d.Dot(Vector2d)` / `AngleD(Vector2d)`           | method          | the tangent dot / inter-tangent angle (degrees) the corner test reads |
|  [07]   | `double Vector2d.Distance(Vector2d)` / `DistanceSquared(Vector2d)` | method      | the point deviation the worst-vertex fit-error gate reads |
|  [08]   | `Vector2d Vector2d.Perp` / `UnitPerp`                          | property        | the perpendicular / unit-perpendicular (the I/J offset lies along the tangent perp) |
|  [09]   | `static Vector2d Vector2d.Zero` / `AxisX` / `AxisY`            | field           | the readonly origin/axis constants                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: biarc fit — `BiArcFit2` constructor + arc/segment read
- rail: fabrication

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new Vector2d(p.X, p.Y)` / `new Vector2d(t.X, t.Y).Normalized`                   | tangent build  | the run-endpoint point and the UNIT tangent (the `.Normalized` carries the `G1` continuity input) the fit consumes |
|  [02]   | `new BiArcFit2(point1, tangent1, point2, tangent2)`                              | construct-fit  | fit the biarc; read `Arc1`/`Arc2` (or `Segment1`/`Segment2` per the `Arc1IsSegment`/`Arc2IsSegment` flag) |
|  [03]   | `fit.Distance(probe)`                                                            | error gate     | the worst original-chord-vertex deviation from the fitted biarc — gate `G2`/`G3` emission against `BiarcPolicy.FitTolerance`, else fall back to a chorded `Feed` run |
|  [04]   | `arc.SampleT(0.0)` / `SampleT(1.0)` / `arc.Center` / `arc.IsReversed`            | arc read       | the fitted arc start/end, center, and sweep direction — the `I`/`J` word is `Center − SampleT(0.0)`, `IsReversed` selects `ArcCw` (`G2`) vs `ArcCcw` (`G3`) |
|  [05]   | `fit.Segment1.P1` / `fit.Segment2.P1`                                            | segment read   | the straight-span end the degenerate (`*IsSegment == true`) run emits as a `Feed` block |

## [04]-[IMPLEMENTATION_LAW]

[BIARC_FIT]:
- `BiArcFit2(p1, t1, p2, t2)` fits two arcs `G1`-tangent-continuous at a solved junction; each fitted span is an `Arc2d` UNLESS the geometry degenerates to a straight run, in which case `Arc1IsSegment`/`Arc2IsSegment` is true and the `Segment1`/`Segment2` carries the straight span — the consumer reads the flag per span and emits an `ArcCw`/`ArcCcw` `GWord` for an arc or a `Feed` `GWord` for a segment
- the input tangents must be unit-length: the consumer builds each from `new Vector2d(t.X, t.Y).Normalized` (the property returns `Vector2d.Zero` below `2.22E-16`), or normalizes in place with `Vector2d.Normalize()` whose RETURNED prior length is the zero-tangent signal — a `0.0` length is a degenerate (collinear-collapsed) chord run the consumer skips below `MinRunLength` rather than fitting
- `Arc2d` endpoints read through `SampleT(0.0)` (start) and `SampleT(1.0)` (end); `Arc2d.P0`/`P1` are convenience properties defined as exactly those samples, so the consumer uses `SampleT` uniformly. The `I`/`J` center offset the G-code word carries is `Center - SampleT(0.0)` (the start-to-center vector)
- `IsReversed` selects the sweep direction: `IsReversed == true` maps to a clockwise `G2` (`ArcCw`), `false` to a counter-clockwise `G3` (`ArcCcw`)
- the fit error is read through `BiArcFit2.Distance(p)` (a query point's deviation from the fitted biarc) and `NearestPoint(p)`: the consumer gates emission on the worst original-chord-vertex deviation staying within the cut tolerance, falling back to a chorded `Feed` run when the biarc over-deviates rather than emitting an out-of-tolerance `G2`/`G3` block

[RAIL_LAW]:
- Package: `geometry3Sharp` (the central pin, shared with the `Rasm.Bim` mesh-text importer)
- Owns: 2D biarc fitting (`g3.BiArcFit2`) and the `Arc2d`/`Segment2d`/`Vector2d` curve primitives it produces
- Accept: the `Posting/program#CUT_PROGRAM` `Biarc` projection fitting a conditioned-contour chord run (or the `adaptive` `Move` chord run carrying its `Option<ArcCenter>`) into `G2`/`G3` arc blocks, the imported DXF bulge/arc span recovered as native arcs
- Reject: the `DMesh3`/`DMeshAABBTree3`/mesh-boolean/`MeshSignedDistanceGrid`/`Remesher` half of the library crossing into this folder or the kernel `Rasm.Geometry` strata (the admission is firewalled to the `BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` curve surface), a `g3` mesh type in a sibling-kernel signature, the NewWheelTech `geometry4Sharp` fork as a duplicate admission (this package owns the identical surface), and the dropped CavalierContours arc-offset engine that would add a second offset call site the one-Clipper2 law forbids (the biarc refit recovers the arc identity at emission over the single Clipper2 chord owner, never a parallel offset)
