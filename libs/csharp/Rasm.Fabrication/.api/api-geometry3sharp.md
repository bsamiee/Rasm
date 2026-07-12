# [RASM_FABRICATION_API_GEOMETRY3SHARP]

The fabrication rail admits `geometry3Sharp` only for line-sourced biarc fitting. `g3.BiArcFit2` converts kernel mesh-section chord runs into two `G1`-continuous `Arc2d` or `Segment2d` spans for `G2`/`G3` emission; arc-native kerf, lead, and adaptive work stay on `Geometry2D/arcs` through CavalierContours. The admission excludes mesh, distance-grid, remeshing, and Boolean surfaces, and the `geometry4Sharp` fork is a duplicate surface rather than a second dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`

- Package: `geometry3Sharp`
- License: `Boost-1.0`
- Assembly: `geometry3Sharp`
- Namespace: `g3`
- Asset: managed IL
- Rail: fabrication line-sourced biarc fitting

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: biarc fit and curve primitives.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :---------- | :------------ | :----------------------------- |
|  [01]   | `BiArcFit2` | fitter class  | point/tangent biarc fitting    |
|  [02]   | `Arc2d`     | curve class   | circular span carrier          |
|  [03]   | `Segment2d` | curve struct  | degenerate straight span       |
|  [04]   | `Vector2d`  | vector struct | point and unit tangent carrier |

[PUBLIC_TYPE_SCOPE]: `BiArcFit2` members.

| [INDEX] | [SYMBOL]                                                                                       | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2)`            | constructor   | endpoint fit          |
|  [02]   | `BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2, double d1)` | constructor   | explicit fit distance |
|  [03]   | `Arc2d Arc1` / `Arc2d Arc2`                                                                    | field         | fitted arc spans      |
|  [04]   | `bool Arc1IsSegment` / `bool Arc2IsSegment`                                                    | field         | degenerate span flags |
|  [05]   | `Segment2d Segment1` / `Segment2d Segment2`                                                    | field         | fitted segment spans  |
|  [06]   | `Vector2d Point1` / `Point2`                                                                   | field         | input endpoints       |
|  [07]   | `Vector2d Tangent1` / `Tangent2`                                                               | field         | input tangents        |
|  [08]   | `double FitD1` / `FitD2`                                                                       | field         | solved fit distances  |
|  [09]   | `double Epsilon`                                                                               | field         | fit tolerance         |
|  [10]   | `List<IParametricCurve2d> Curves` / `IParametricCurve2d Curve1` / `Curve2`                     | property      | polymorphic span list |
|  [11]   | `double Distance(Vector2d p)` / `Vector2d NearestPoint(Vector2d p)`                            | method        | fit-error query       |

[PUBLIC_TYPE_SCOPE]: `Arc2d`, `Segment2d`, and `Vector2d` members.

| [INDEX] | [SYMBOL]                                                                           | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `Arc2d(Vector2d center, double radius, double startDeg, double endDeg)`            | constructor   | center/radius arc     |
|  [02]   | `Arc2d(Vector2d vCenter, Vector2d vStart, Vector2d vEnd)`                          | constructor   | center/endpoints arc  |
|  [03]   | `Vector2d Arc2d.Center` / `double Radius`                                          | field         | center and radius     |
|  [04]   | `double Arc2d.AngleStartDeg` / `AngleEndDeg`                                       | field         | sweep angles          |
|  [05]   | `bool Arc2d.IsReversed`                                                            | field         | clockwise selection   |
|  [06]   | `Vector2d Arc2d.SampleT(double t)`                                                 | method        | parameter sample      |
|  [07]   | `Vector2d Arc2d.P0` / `P1`                                                         | property      | endpoints             |
|  [08]   | `double Arc2d.ArcLength`                                                           | property      | arc length            |
|  [09]   | `Vector2d Arc2d.SampleArcLength(double a)` / `Segment2d.SampleArcLength(double a)` | method        | distance sample       |
|  [10]   | `Segment2d(Vector2d p0, Vector2d p1)`                                              | constructor   | endpoint segment      |
|  [11]   | `Vector2d Segment2d.P0` / `P1`                                                     | property      | segment endpoints     |
|  [12]   | `Vector2d Segment2d.Center` / `Direction` / `double Extent`                        | field         | center-direction span |
|  [13]   | `double Segment2d.Length`                                                          | property      | segment length        |
|  [14]   | `Vector2d(double x, double y)`                                                     | constructor   | point/tangent builder |
|  [15]   | `double Vector2d.x` / `y`                                                          | field         | component fields      |
|  [16]   | `Vector2d Vector2d.Normalized`                                                     | property      | unit-copy tangent     |
|  [17]   | `double Vector2d.Normalize(double epsilon = 2.22E-16)`                             | method        | in-place unit scale   |
|  [18]   | `double Vector2d.Length` / `LengthSquared`                                         | property      | magnitude             |
|  [19]   | `double Vector2d.Dot(Vector2d)` / `AngleD(Vector2d)`                               | method        | tangent comparison    |
|  [20]   | `double Vector2d.Distance(Vector2d)` / `DistanceSquared(Vector2d)`                 | method        | point deviation       |
|  [21]   | `Vector2d Vector2d.Perp` / `UnitPerp`                                              | property      | perpendicular vectors |
|  [22]   | `static Vector2d Vector2d.Zero` / `AxisX` / `AxisY`                                | field         | origin and axes       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: line-sourced biarc fit.

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CAPABILITY]       |
| :-----: | :-------------------------------------------------------------------- | :------------- | :----------------- |
|  [01]   | `new Vector2d(p.X, p.Y)` / `new Vector2d(t.X, t.Y).Normalized`        | tangent build  | unit tangent input |
|  [02]   | `new BiArcFit2(point1, tangent1, point2, tangent2)`                   | construct-fit  | biarc solve        |
|  [03]   | `fit.Distance(probe)`                                                 | error gate     | tolerance check    |
|  [04]   | `arc.SampleT(0.0)` / `SampleT(1.0)` / `arc.Center` / `arc.IsReversed` | arc read       | `G2`/`G3` mapping  |
|  [05]   | `fit.Segment1.P1` / `fit.Segment2.P1`                                 | segment read   | `G1` fallback      |

## [04]-[IMPLEMENTATION_LAW]

- `BiArcFit2(p1, t1, p2, t2)` fits two spans meeting at a `G1`-continuous junction; `Arc1IsSegment` and `Arc2IsSegment` select `Segment2d` fallback per span.
- Input tangents are unit-length through `new Vector2d(t.X, t.Y).Normalized`; a zero tangent routes to the straight-span path before fitting.
- Arc endpoints read through `SampleT(0.0)` and `SampleT(1.0)`; the G-code `I`/`J` offset is `Center - SampleT(0.0)`.
- Tab and micro-bridge insertion read `SampleArcLength(a)` so spacing follows distance along the fitted span rather than parameter position.
- `IsReversed == true` maps to clockwise `G2`; `false` maps to counter-clockwise `G3`.
- `BiArcFit2.Distance(p)` gates emission against `BiarcPolicy.FitTolerance`; an over-tolerance fit falls back to chorded `G1` output.
- `g3.BiArcFit2` applies only to genuinely line-sourced kernel mesh-section chains; arc-native loops read `Geometry2D/arcs` directly.
- `DMesh3`, `DMeshAABBTree3`, `MeshSignedDistanceGrid`, `Remesher`, mesh Boolean surfaces, and `geometry4Sharp` stay outside the fabrication rail.
