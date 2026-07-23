# [RASM_FABRICATION_API_GEOMETRY3SHARP]

`geometry3Sharp` enters the fabrication rail on one surface — `g3.BiArcFit2`, the line-sourced biarc fitter. It folds a line-only chord run (a kernel mesh-section chain or a densified line offset) into two `G1`-continuous `Arc2d` or `Segment2d` spans that emit as `G2`/`G3` arc moves, reading the arc frame straight from the fitted span with no downstream refit. Arc-native kerf, lead, and adaptive-clearing toolpaths carry their bulge through `CavalierContours` and never reach this fitter, which owns only the genuinely line-sourced residual.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp` (Boost-1.0)
- assembly: `geometry3Sharp`
- namespace: `g3`
- asset: pure-managed AnyCPU IL, multi-target `netstandard2.0` / `net45` (no native asset, no RID burden); the `net10.0` consumer binds `lib/netstandard2.0/geometry3Sharp.dll`
- rail: fabrication — line-sourced biarc fit

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: biarc fit and curve primitives.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :---------- | :------------ | :----------------------------- |
|  [01]   | `BiArcFit2` | fitter class  | point/tangent biarc fitting    |
|  [02]   | `Arc2d`     | curve class   | circular span carrier          |
|  [03]   | `Segment2d` | curve struct  | degenerate straight span       |
|  [04]   | `Vector2d`  | vector struct | point and unit tangent carrier |

[PUBLIC_TYPE_SCOPE]: `BiArcFit2` members.

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d)`                        | constructor   | endpoint/tangent fit  |
|  [02]   | `BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d, double)`                | constructor   | explicit fit distance |
|  [03]   | `Arc2d Arc1` / `Arc2`                                                      | field         | fitted arc spans      |
|  [04]   | `bool Arc1IsSegment` / `Arc2IsSegment`                                     | field         | degenerate span flags |
|  [05]   | `Segment2d Segment1` / `Segment2`                                          | field         | fitted segment spans  |
|  [06]   | `Vector2d Point1` / `Point2`                                               | field         | input endpoints       |
|  [07]   | `Vector2d Tangent1` / `Tangent2`                                           | field         | input tangents        |
|  [08]   | `double FitD1` / `FitD2`                                                   | field         | solved fit distances  |
|  [09]   | `double Epsilon`                                                           | field         | fit tolerance         |
|  [10]   | `List<IParametricCurve2d> Curves` / `IParametricCurve2d Curve1` / `Curve2` | property      | polymorphic span list |
|  [11]   | `Distance(Vector2d) -> double` / `NearestPoint(Vector2d) -> Vector2d`      | method        | fit-error query       |

[PUBLIC_TYPE_SCOPE]: `Arc2d`, `Segment2d`, and `Vector2d` members.

| [INDEX] | [SYMBOL]                                                                          | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `Arc2d(Vector2d, double, double, double)`                                         | constructor   | center/radius arc     |
|  [02]   | `Arc2d(Vector2d, Vector2d, Vector2d)`                                             | constructor   | center/endpoints arc  |
|  [03]   | `Vector2d Arc2d.Center` / `double Radius`                                         | field         | center and radius     |
|  [04]   | `double Arc2d.AngleStartDeg` / `AngleEndDeg`                                      | field         | sweep angles          |
|  [05]   | `bool Arc2d.IsReversed`                                                           | field         | direction selection   |
|  [06]   | `Arc2d.SampleT(double) -> Vector2d`                                               | method        | parameter sample      |
|  [07]   | `Vector2d Arc2d.P0` / `P1`                                                        | property      | endpoints             |
|  [08]   | `double Arc2d.ArcLength`                                                          | property      | arc length            |
|  [09]   | `Arc2d.SampleArcLength(double)` / `Segment2d.SampleArcLength(double) -> Vector2d` | method        | distance sample       |
|  [10]   | `Segment2d(Vector2d, Vector2d)`                                                   | constructor   | endpoint segment      |
|  [11]   | `Vector2d Segment2d.P0` / `P1`                                                    | property      | segment endpoints     |
|  [12]   | `Vector2d Segment2d.Center` / `Direction` / `double Extent`                       | field         | center-direction span |
|  [13]   | `double Segment2d.Length`                                                         | property      | segment length        |
|  [14]   | `Vector2d(double, double)`                                                        | constructor   | point/tangent builder |
|  [15]   | `double Vector2d.x` / `y`                                                         | field         | component fields      |
|  [16]   | `Vector2d Vector2d.Normalized`                                                    | property      | unit-copy tangent     |
|  [17]   | `Vector2d.Normalize(double) -> double`                                            | method        | in-place unit scale   |
|  [18]   | `double Vector2d.Length` / `LengthSquared`                                        | property      | magnitude             |
|  [19]   | `Vector2d.Dot(Vector2d)` / `AngleD(Vector2d) -> double`                           | method        | tangent comparison    |
|  [20]   | `Vector2d.Distance(Vector2d)` / `DistanceSquared(Vector2d) -> double`             | method        | point deviation       |
|  [21]   | `Vector2d Vector2d.Perp` / `UnitPerp`                                             | property      | perpendicular vectors |
|  [22]   | `static Vector2d Vector2d.Zero` / `AxisX` / `AxisY`                               | field         | origin and axes       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: line-sourced biarc fit.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------ | :------- | :----------------- |
|  [01]   | `new Vector2d(double, double).Normalized`                     | ctor     | unit tangent input |
|  [02]   | `new BiArcFit2(Vector2d, Vector2d, Vector2d, Vector2d)`       | ctor     | biarc solve        |
|  [03]   | `BiArcFit2.Distance(Vector2d) -> double`                      | instance | tolerance gate     |
|  [04]   | `Arc2d.SampleT(double)` / `Arc2d.Center` / `Arc2d.IsReversed` | instance | `G2`/`G3` mapping  |
|  [05]   | `Segment2d.P1`                                                | property | `G1` fallback      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BiArcFit2(p1, t1, p2, t2)` fits two spans meeting at a `G1`-continuous junction; `Arc1IsSegment` and `Arc2IsSegment` select the `Segment2d` fallback per span, exposed uniformly through `Curves` / `Curve1` / `Curve2` as `IParametricCurve2d`.
- Input tangents are unit-length via `Vector2d.Normalized`; a zero tangent routes to the straight-span path before fitting.
- Arc endpoints read through `SampleT(0.0)` and `SampleT(1.0)`; the G-code `I`/`J` offset is `Center - SampleT(0.0)`, `IsReversed == true` maps to clockwise `G2`, `false` to counter-clockwise `G3`.
- `SampleArcLength(a)` samples by distance along the fitted span, so tab and micro-bridge spacing follows arc length rather than parameter position.

[STACKING]:
- `CavalierContours`(`.api/api-cavaliercontours.md`): its `PlineVertex.Bulge` maps directly to a `G2`/`G3` move, so a bulge-carrying offset, lead-arc, or adaptive-spiral loop emits arcs without a refit; `g3.BiArcFit2` fires only on a genuinely line-sourced chord run — a line-only path densified through `ArcsToApproxLines` then clipped, or a kernel mesh-section chain — and stays the sole biarc owner for that residual.
- `Posting/program`, `Toolpath/motion`: the fitted `Arc2d`/`Segment2d` span drives arc emit — `Arc2d.Center - SampleT(0.0)` is the `Move.ArcCenter` `I`/`J` offset, `IsReversed` selects `G2` vs `G3`, `Segment2d.P1` the `G1` fallback, `SampleArcLength` the tab and lead-point spacing.

[LOCAL_ADMISSION]:
- Feed `BiArcFit2` a `Vector2d` point/tangent pair with tangents pre-normalized through `Normalized`; the two-arg constructor solves the symmetric fit, the explicit-`d1` constructor pins the first arc distance.
- Gate emission with `BiArcFit2.Distance(p)` against `BiarcPolicy.FitTolerance`; an over-tolerance fit falls back to chorded `G1` output.
- Reach `g3.BiArcFit2` only for a line-sourced kernel mesh-section chain; an arc-native loop reads `Geometry2D/arcs` through `CavalierContours` directly.

[RAIL_LAW]:
- Package: `geometry3Sharp` (Boost-1.0)
- Owns: line-sourced biarc fitting — two `G1`-continuous `Arc2d`/`Segment2d` spans from a point/tangent pair, with `SampleT`/`SampleArcLength` arc reads and `Distance` fit-error query.
- Accept: a unit-normalized `Vector2d` point/tangent pair from a genuinely line-only chord run; the symmetric or explicit-fit-distance constructor; the `Arc2d`/`Segment2d` frame read straight for `G2`/`G3` emit.
- Reject: fitting a path already carrying bulge through the `CavalierContours` offset (the retired post-hoc refit); admitting `DMesh3`, `DMeshAABBTree3`, `MeshSignedDistanceGrid`, `Remesher`, mesh-Boolean surfaces, or the `geometry4Sharp` fork into the fabrication rail; a hand-rolled biarc solver beside `BiArcFit2`.
