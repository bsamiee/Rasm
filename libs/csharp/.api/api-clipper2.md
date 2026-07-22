# [RASM_API_CLIPPER2]

`Clipper2Lib` owns planar polygon algebra on the float production plane: every operation resolves over integer-exact `int64` coordinates, and a double entry scales in at a caller-named decimal precision and descales its result. Precision rides the call as data, so one engine plane serves both coordinate families.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Clipper2`
- package: `Clipper2` (BSL-1.0, Angus Johnson)
- assembly: `Clipper2Lib`
- namespace: `Clipper2Lib`
- asset: pure-managed AnyCPU IL carrying no native payload, so a collectible ALC loads and unloads it with no RID burden
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: coordinate and path carriers

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :-------- | :------------ | :--------------------------------------------- |
|  [01]   | `Point64` | struct        | `long` ordinate pair, the engine's atom        |
|  [02]   | `PointD`  | struct        | `double` ordinate pair at the caller boundary  |
|  [03]   | `Rect64`  | struct        | int64 axis-aligned extent with rect predicates |
|  [04]   | `RectD`   | struct        | double axis-aligned extent                     |
|  [05]   | `Path64`  | class         | `List<Point64>` closed polygon or open run     |
|  [06]   | `Paths64` | class         | `List<Path64>` polygon set                     |
|  [07]   | `PathD`   | class         | `List<PointD>` closed polygon or open run      |
|  [08]   | `PathsD`  | class         | `List<PathD>` polygon set                      |

[PUBLIC_TYPE_SCOPE]: operation vocabularies and failure

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------- | :------------ | :------------------------------------- |
|  [01]   | `ClipType`             | enum          | Boolean arm selector                   |
|  [02]   | `FillRule`             | enum          | interior-fill winding policy           |
|  [03]   | `PathType`             | enum          | subject-or-clip role on an engine add  |
|  [04]   | `JoinType`             | enum          | offset corner join style               |
|  [05]   | `EndType`              | enum          | open-path end cap style                |
|  [06]   | `PointInPolygonResult` | enum          | `[Flags]` containment verdict          |
|  [07]   | `ClipperLibException`  | class         | precision-range failure off `ClipperD` |

- `ClipType`: `NoClip` returns the subject untouched; `Intersection`, `Union`, `Difference`, and `Xor` are the overlay arms.
- `FillRule`: `EvenOdd` fills by crossing parity, `NonZero` by non-zero winding, `Positive` and `Negative` by signed winding sense.
- `JoinType`: `Miter` holds the sharp corner to `MiterLimit`, `Square` caps at an angle, `Bevel` cuts flat, `Round` arcs.
- `EndType`: `Polygon` offsets a closed ring, `Joined` closes an open run before offsetting, `Butt` ends flat, `Square` extends, `Round` caps semicircular.
- `PathType`: `Subject` and `Clip` assign the role of every path handed to a stateful engine.
- `PointInPolygonResult`: `IsOn` is `0`, `IsInside` is `1`, `IsOutside` is `2`.

[PUBLIC_TYPE_SCOPE]: engines and result carriers

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `Clipper`                       | class         | static facade folding every one-shot operation           |
|  [02]   | `Minkowski`                     | class         | static facade carrying morphology at caller precision    |
|  [03]   | `ClipperBase`                   | class         | abstract engine base owning input state and result flags |
|  [04]   | `Clipper64`                     | class         | stateful int64 overlay engine                            |
|  [05]   | `ClipperD`                      | class         | stateful double overlay engine at a bound precision      |
|  [06]   | `ClipperOffset`                 | class         | offset engine over join, end, and per-vertex delta       |
|  [07]   | `ClipperOffset.DeltaCallback64` | delegate      | per-vertex signed offset magnitude                       |
|  [08]   | `ReuseableDataContainer64`      | class         | precomputed subject/clip vertex structure                |
|  [09]   | `PolyTree64`                    | class         | int64 nesting result root                                |
|  [10]   | `PolyPath64`                    | class         | int64 nesting node carrying its ring and children        |
|  [11]   | `PolyTreeD`                     | class         | double nesting result root carrying its scale            |
|  [12]   | `PolyPathD`                     | class         | double nesting node carrying its ring and children       |
|  [13]   | `PolyPathBase`                  | class         | abstract node base owning depth, hole sense, traversal   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Boolean overlay — `Clipper`

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------------- | :------ | :-------------------------- |
|  [01]   | `Clipper.Intersect(Paths64, Paths64, FillRule)`                         | static  | int64 intersection          |
|  [02]   | `Clipper.Intersect(PathsD, PathsD, FillRule, int)`                      | static  | double intersection         |
|  [03]   | `Clipper.Union(Paths64, FillRule)`                                      | static  | int64 self-union            |
|  [04]   | `Clipper.Union(Paths64, Paths64, FillRule)`                             | static  | int64 union                 |
|  [05]   | `Clipper.Union(PathsD, FillRule)`                                       | static  | double self-union           |
|  [06]   | `Clipper.Union(PathsD, PathsD, FillRule, int)`                          | static  | double union                |
|  [07]   | `Clipper.Difference(Paths64, Paths64, FillRule)`                        | static  | int64 difference            |
|  [08]   | `Clipper.Difference(PathsD, PathsD, FillRule, int)`                     | static  | double difference           |
|  [09]   | `Clipper.Xor(Paths64, Paths64, FillRule)`                               | static  | int64 exclusive-or          |
|  [10]   | `Clipper.Xor(PathsD, PathsD, FillRule, int)`                            | static  | double exclusive-or         |
|  [11]   | `Clipper.BooleanOp(ClipType, Paths64, Paths64, FillRule)`               | static  | int64 arm-selected overlay  |
|  [12]   | `Clipper.BooleanOp(ClipType, PathsD, PathsD, FillRule, int)`            | static  | double arm-selected overlay |
|  [13]   | `Clipper.BooleanOp(ClipType, Paths64, Paths64, PolyTree64, FillRule)`   | static  | int64 overlay into a tree   |
|  [14]   | `Clipper.BooleanOp(ClipType, PathsD, PathsD, PolyTreeD, FillRule, int)` | static  | double overlay into a tree  |

- `Clipper.BooleanOp`: a null subject yields an empty result and leaves a supplied tree untouched; a null clip runs the arm against the subject alone.

[ENTRYPOINT_SCOPE]: offset, window, morphology, construction — `Clipper` and `Minkowski`

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `Clipper.InflatePaths(Paths64, double, JoinType, EndType, double, double)`     | static  | int64 inflate                  |
|  [02]   | `Clipper.InflatePaths(PathsD, double, JoinType, EndType, double, int, double)` | static  | double inflate                 |
|  [03]   | `Clipper.RectClip(Rect64, Paths64)`                                            | static  | int64 set clipped to a rect    |
|  [04]   | `Clipper.RectClip(Rect64, Path64)`                                             | static  | int64 ring clipped to a rect   |
|  [05]   | `Clipper.RectClip(RectD, PathsD, int)`                                         | static  | double set clipped to a rect   |
|  [06]   | `Clipper.RectClip(RectD, PathD, int)`                                          | static  | double ring clipped to a rect  |
|  [07]   | `Clipper.RectClipLines(Rect64, Paths64)`                                       | static  | int64 open runs to a rect      |
|  [08]   | `Clipper.RectClipLines(Rect64, Path64)`                                        | static  | int64 open run to a rect       |
|  [09]   | `Clipper.RectClipLines(RectD, PathsD, int)`                                    | static  | double open runs to a rect     |
|  [10]   | `Clipper.RectClipLines(RectD, PathD, int)`                                     | static  | double open run to a rect      |
|  [11]   | `Clipper.MinkowskiSum(Path64, Path64, bool)`                                   | static  | int64 sum locus                |
|  [12]   | `Clipper.MinkowskiSum(PathD, PathD, bool)`                                     | static  | double sum at precision `2`    |
|  [13]   | `Clipper.MinkowskiDiff(Path64, Path64, bool)`                                  | static  | int64 difference locus         |
|  [14]   | `Clipper.MinkowskiDiff(PathD, PathD, bool)`                                    | static  | double diff at precision `2`   |
|  [15]   | `Minkowski.Sum(Path64, Path64, bool)`                                          | static  | int64 sum locus                |
|  [16]   | `Minkowski.Sum(PathD, PathD, bool, int)`                                       | static  | double sum at caller decimals  |
|  [17]   | `Minkowski.Diff(Path64, Path64, bool)`                                         | static  | int64 difference locus         |
|  [18]   | `Minkowski.Diff(PathD, PathD, bool, int)`                                      | static  | double diff at caller decimals |
|  [19]   | `Clipper.Ellipse(Point64, double, double, int)`                                | static  | int64 ellipse or circle ring   |
|  [20]   | `Clipper.Ellipse(PointD, double, double, int)`                                 | static  | double ellipse or circle ring  |

- `Minkowski`: every locus folds a `FillRule.NonZero` union before returning, so the result is already a simple polygon set.

[ENTRYPOINT_SCOPE]: measurement, predicates, hygiene — `Clipper`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Clipper.Area(Path64) -> double`                               | static  | signed ring area, positive is CCW |
|  [02]   | `Clipper.Area(Paths64) -> double`                              | static  | signed set area                   |
|  [03]   | `Clipper.Area(PathD) -> double`                                | static  | signed double ring area           |
|  [04]   | `Clipper.Area(PathsD) -> double`                               | static  | signed double set area            |
|  [05]   | `Clipper.IsPositive(Path64) -> bool`                           | static  | CCW winding test                  |
|  [06]   | `Clipper.IsPositive(PathD) -> bool`                            | static  | CCW winding test on doubles       |
|  [07]   | `Clipper.GetBounds(Path64) -> Rect64`                          | static  | ring extent                       |
|  [08]   | `Clipper.GetBounds(Paths64) -> Rect64`                         | static  | set extent                        |
|  [09]   | `Clipper.GetBounds(PathD) -> RectD`                            | static  | double ring extent                |
|  [10]   | `Clipper.GetBounds(PathsD) -> RectD`                           | static  | double set extent                 |
|  [11]   | `Clipper.PointInPolygon(Point64, Path64)`                      | static  | inside, on, or outside verdict    |
|  [12]   | `Clipper.PointInPolygon(PointD, PathD, int)`                   | static  | double containment verdict        |
|  [13]   | `Clipper.PointsNearEqual(PointD, PointD, double) -> bool`      | static  | squared-distance coincidence test |
|  [14]   | `Clipper.DistanceSqr(Point64, Point64) -> double`              | static  | squared point distance            |
|  [15]   | `Clipper.PerpendicDistFromLineSqrd(Point64, Point64, Point64)` | static  | squared point-to-line distance    |
|  [16]   | `Clipper.PerpendicDistFromLineSqrd(PointD, PointD, PointD)`    | static  | squared double point-to-line      |
|  [17]   | `Clipper.MidPoint(Point64, Point64) -> Point64`                | static  | segment midpoint                  |
|  [18]   | `Clipper.MidPoint(PointD, PointD) -> PointD`                   | static  | double segment midpoint           |
|  [19]   | `Clipper.RamerDouglasPeucker(Path64, double)`                  | static  | tolerance-bounded decimation      |
|  [20]   | `Clipper.RamerDouglasPeucker(Paths64, double)`                 | static  | set decimation                    |
|  [21]   | `Clipper.RamerDouglasPeucker(PathD, double)`                   | static  | double decimation                 |
|  [22]   | `Clipper.RamerDouglasPeucker(PathsD, double)`                  | static  | double set decimation             |
|  [23]   | `Clipper.SimplifyPath(Path64, double, bool)`                   | static  | epsilon collinear simplification  |
|  [24]   | `Clipper.SimplifyPath(PathD, double, bool)`                    | static  | double collinear simplification   |
|  [25]   | `Clipper.SimplifyPaths(Paths64, double, bool)`                 | static  | set simplification                |
|  [26]   | `Clipper.SimplifyPaths(PathsD, double, bool)`                  | static  | double set simplification         |
|  [27]   | `Clipper.TrimCollinear(Path64, bool)`                          | static  | collinear vertex removal          |
|  [28]   | `Clipper.TrimCollinear(PathD, int, bool)`                      | static  | double collinear vertex removal   |
|  [29]   | `Clipper.StripDuplicates(Path64, bool)`                        | static  | consecutive duplicate removal     |
|  [30]   | `Clipper.StripNearDuplicates(PathD, double, bool)`             | static  | short-edge collapse under a bound |

[ENTRYPOINT_SCOPE]: scaling, conversion, transform — `Clipper`

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Clipper.ScalePath(Path64, double)`                | static   | int64 ring rescale                     |
|  [02]   | `Clipper.ScalePath(PathD, double)`                 | static   | double ring rescale                    |
|  [03]   | `Clipper.ScalePaths(Paths64, double)`              | static   | int64 set rescale                      |
|  [04]   | `Clipper.ScalePaths(PathsD, double)`               | static   | double set rescale                     |
|  [05]   | `Clipper.ScalePath64(PathD, double) -> Path64`     | static   | scale-in to the engine plane           |
|  [06]   | `Clipper.ScalePaths64(PathsD, double) -> Paths64`  | static   | set scale-in to the engine plane       |
|  [07]   | `Clipper.ScalePathD(Path64, double) -> PathD`      | static   | descale out of the engine plane        |
|  [08]   | `Clipper.ScalePathsD(Paths64, double) -> PathsD`   | static   | set descale out of the engine plane    |
|  [09]   | `Clipper.ScalePoint64(Point64, double) -> Point64` | static   | point rescale in the engine plane      |
|  [10]   | `Clipper.ScalePointD(Point64, double) -> PointD`   | static   | point descale                          |
|  [11]   | `Clipper.ScaleRect(RectD, double) -> Rect64`       | static   | extent scale-in for a rect clip        |
|  [12]   | `Clipper.Path64(PathD) -> Path64`                  | static   | unscaled ring narrowing                |
|  [13]   | `Clipper.Paths64(PathsD) -> Paths64`               | static   | unscaled set narrowing                 |
|  [14]   | `Clipper.PathD(Path64) -> PathD`                   | static   | unscaled ring widening                 |
|  [15]   | `Clipper.PathsD(Paths64) -> PathsD`                | static   | unscaled set widening                  |
|  [16]   | `Clipper.OffsetPath(Path64, long, long)`           | static   | int64 ring shift                       |
|  [17]   | `Clipper.TranslatePath(Path64, long, long)`        | static   | int64 ring translation                 |
|  [18]   | `Clipper.TranslatePath(PathD, double, double)`     | static   | double ring translation                |
|  [19]   | `Clipper.TranslatePaths(Paths64, long, long)`      | static   | int64 set translation                  |
|  [20]   | `Clipper.TranslatePaths(PathsD, double, double)`   | static   | double set translation                 |
|  [21]   | `Clipper.ReversePath(Path64)`                      | static   | int64 winding reversal                 |
|  [22]   | `Clipper.ReversePath(PathD)`                       | static   | double winding reversal                |
|  [23]   | `Clipper.ReversePaths(Paths64)`                    | static   | int64 set winding reversal             |
|  [24]   | `Clipper.ReversePaths(PathsD)`                     | static   | double set winding reversal            |
|  [25]   | `Clipper.InflateRect(ref Rect64, int, int)`        | static   | in-place int64 extent grow             |
|  [26]   | `Clipper.InflateRect(ref RectD, double, double)`   | static   | in-place double extent grow            |
|  [27]   | `Clipper.MakePath(int[]) -> Path64`                | factory  | interleaved `int` pairs to a ring      |
|  [28]   | `Clipper.MakePath(long[]) -> Path64`               | factory  | interleaved `long` pairs to a ring     |
|  [29]   | `Clipper.MakePath(double[]) -> PathD`              | factory  | interleaved `double` pairs to a ring   |
|  [30]   | `Clipper.PolyTreeToPaths64(PolyTree64) -> Paths64` | static   | int64 tree flattened to a set          |
|  [31]   | `Clipper.PolyTreeToPathsD(PolyTreeD) -> PathsD`    | static   | double tree flattened to a set         |
|  [32]   | `Clipper.InvalidRect64`                            | property | inverted-extent seed for a bounds fold |
|  [33]   | `Clipper.InvalidRectD`                             | property | inverted double-extent seed            |

[ENTRYPOINT_SCOPE]: stateful int64 overlay — `Clipper64` and `ClipperBase`

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Clipper64.AddSubject(Paths64)`                                      | instance | closed subject set              |
|  [02]   | `Clipper64.AddOpenSubject(Paths64)`                                  | instance | open subject set                |
|  [03]   | `Clipper64.AddClip(Paths64)`                                         | instance | clip set                        |
|  [04]   | `ClipperBase.AddSubject(Path64)`                                     | instance | one closed subject              |
|  [05]   | `ClipperBase.AddOpenSubject(Path64)`                                 | instance | one open subject                |
|  [06]   | `ClipperBase.AddClip(Path64)`                                        | instance | one clip ring                   |
|  [07]   | `Clipper64.AddReuseableData(ReuseableDataContainer64)`               | instance | precomputed input replay        |
|  [08]   | `Clipper64.Execute(ClipType, FillRule, Paths64) -> bool`             | instance | closed result                   |
|  [09]   | `Clipper64.Execute(ClipType, FillRule, Paths64, Paths64) -> bool`    | instance | closed and open results         |
|  [10]   | `Clipper64.Execute(ClipType, FillRule, PolyTree64) -> bool`          | instance | nesting-tree result             |
|  [11]   | `Clipper64.Execute(ClipType, FillRule, PolyTree64, Paths64) -> bool` | instance | tree and open results           |
|  [12]   | `ClipperBase.Clear()`                                                | instance | drops every added path          |
|  [13]   | `ClipperBase.GetBounds() -> Rect64`                                  | instance | extent of the added vertices    |
|  [14]   | `ClipperBase.PreserveCollinear`                                      | property | keeps collinear result vertices |
|  [15]   | `ClipperBase.ReverseSolution`                                        | property | flips result winding            |

[ENTRYPOINT_SCOPE]: stateful double overlay — `ClipperD`

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `ClipperD(int)`                                                   | ctor     | binds the decimal precision |
|  [02]   | `ClipperD.AddPath(PathD, PathType, bool)`                         | instance | role-tagged single path     |
|  [03]   | `ClipperD.AddPaths(PathsD, PathType, bool)`                       | instance | role-tagged path set        |
|  [04]   | `ClipperD.AddSubject(PathD)`                                      | instance | one closed subject          |
|  [05]   | `ClipperD.AddSubject(PathsD)`                                     | instance | closed subject set          |
|  [06]   | `ClipperD.AddOpenSubject(PathD)`                                  | instance | one open subject            |
|  [07]   | `ClipperD.AddOpenSubject(PathsD)`                                 | instance | open subject set            |
|  [08]   | `ClipperD.AddClip(PathD)`                                         | instance | one clip ring               |
|  [09]   | `ClipperD.AddClip(PathsD)`                                        | instance | clip set                    |
|  [10]   | `ClipperD.Execute(ClipType, FillRule, PathsD) -> bool`            | instance | closed result               |
|  [11]   | `ClipperD.Execute(ClipType, FillRule, PathsD, PathsD) -> bool`    | instance | closed and open results     |
|  [12]   | `ClipperD.Execute(ClipType, FillRule, PolyTreeD) -> bool`         | instance | nesting-tree result         |
|  [13]   | `ClipperD.Execute(ClipType, FillRule, PolyTreeD, PathsD) -> bool` | instance | tree and open results       |

[ENTRYPOINT_SCOPE]: offset engine — `ClipperOffset`

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `ClipperOffset(double, double, bool, bool)`          | ctor     | miter limit, arc tolerance, result flags  |
|  [02]   | `ClipperOffset.AddPath(Path64, JoinType, EndType)`   | instance | one path in its own join and end group    |
|  [03]   | `ClipperOffset.AddPaths(Paths64, JoinType, EndType)` | instance | a path group under one join and end style |
|  [04]   | `ClipperOffset.Execute(double, Paths64)`             | instance | constant-delta inflate to a flat set      |
|  [05]   | `ClipperOffset.Execute(double, PolyTree64)`          | instance | constant-delta inflate to a nesting tree  |
|  [06]   | `ClipperOffset.Execute(DeltaCallback64, Paths64)`    | instance | per-vertex variable-delta inflate         |
|  [07]   | `ClipperOffset.Clear()`                              | instance | drops every registered group              |
|  [08]   | `ClipperOffset.ArcTolerance`                         | property | arc-approximation deviation bound         |
|  [09]   | `ClipperOffset.MiterLimit`                           | property | miter extension bound before squaring     |
|  [10]   | `ClipperOffset.MergeGroups`                          | property | unions groups before offsetting           |
|  [11]   | `ClipperOffset.PreserveCollinear`                    | property | keeps collinear result vertices           |
|  [12]   | `ClipperOffset.ReverseSolution`                      | property | flips result winding                      |
|  [13]   | `ClipperOffset.DeltaCallback`                        | property | the per-vertex delta source               |

[ENTRYPOINT_SCOPE]: nesting topology — `PolyPath64`, `PolyPathD`, `PolyPathBase`, `PolyTreeD`

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `PolyPath64.Polygon`                  | property | this node's int64 ring                    |
|  [02]   | `PolyPathD.Polygon`                   | property | this node's double ring                   |
|  [03]   | `PolyPath64.Child(int) -> PolyPath64` | instance | typed child by ordinal                    |
|  [04]   | `PolyPath64[int] -> PolyPath64`       | property | typed child indexer                       |
|  [05]   | `PolyPathD[int] -> PolyPathD`         | property | typed child indexer                       |
|  [06]   | `PolyPath64.Area() -> double`         | instance | subtree area with holes netted out        |
|  [07]   | `PolyPathD.Area() -> double`          | instance | double subtree area with holes netted out |
|  [08]   | `PolyPathBase.Level`                  | property | root-relative depth                       |
|  [09]   | `PolyPathBase.IsHole`                 | property | outer-or-hole sense from depth parity     |
|  [10]   | `PolyPathBase.Count`                  | property | direct child count                        |
|  [11]   | `PolyPathBase.GetEnumerator()`        | instance | ordered direct-child walk                 |
|  [12]   | `PolyPathBase.Clear()`                | instance | drops the child subtree                   |
|  [13]   | `PolyTreeD.Scale`                     | property | the scale the producing `ClipperD` bound  |

[ENTRYPOINT_SCOPE]: reusable subject precompute — `ReuseableDataContainer64`

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `ReuseableDataContainer64()`                                 | ctor     | opens an empty precompute cache |
|  [02]   | `ReuseableDataContainer64.AddPaths(Paths64, PathType, bool)` | instance | precomputes a role-tagged set   |
|  [03]   | `ReuseableDataContainer64.Clear()`                           | instance | drops the precomputed input     |

[ENTRYPOINT_SCOPE]: extent and point algebra — `Rect64`, `RectD`, `Point64`, `PointD`

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Rect64(long, long, long, long)`        | ctor     | extent from its four edges        |
|  [02]   | `Rect64.Contains(Point64) -> bool`      | instance | point-in-extent test              |
|  [03]   | `Rect64.Contains(Rect64) -> bool`       | instance | extent containment test           |
|  [04]   | `Rect64.Intersects(Rect64) -> bool`     | instance | broad-phase overlap test          |
|  [05]   | `Rect64.IsEmpty() -> bool`              | instance | degenerate-extent test            |
|  [06]   | `Rect64.IsValid() -> bool`              | instance | seeded-sentinel test              |
|  [07]   | `Rect64.MidPoint() -> Point64`          | instance | extent centre                     |
|  [08]   | `Rect64.AsPath() -> Path64`             | instance | extent as a clippable ring        |
|  [09]   | `Rect64.Width`                          | property | span, settable through `right`    |
|  [10]   | `Rect64.Height`                         | property | span, settable through `bottom`   |
|  [11]   | `RectD(double, double, double, double)` | ctor     | double extent from its four edges |
|  [12]   | `RectD.Contains(PointD) -> bool`        | instance | point-in-extent test              |
|  [13]   | `RectD.Contains(RectD) -> bool`         | instance | extent containment test           |
|  [14]   | `RectD.Intersects(RectD) -> bool`       | instance | broad-phase overlap test          |
|  [15]   | `RectD.IsEmpty() -> bool`               | instance | degenerate-extent test            |
|  [16]   | `RectD.MidPoint() -> PointD`            | instance | extent centre                     |
|  [17]   | `RectD.AsPath() -> PathD`               | instance | extent as a clippable ring        |
|  [18]   | `RectD.Width`                           | property | span, settable through `right`    |
|  [19]   | `RectD.Height`                          | property | span, settable through `bottom`   |
|  [20]   | `Point64(long, long)`                   | ctor     | ordinate pair                     |
|  [21]   | `Point64(PointD, double)`               | ctor     | scale-in from a double point      |
|  [22]   | `PointD(Point64, double)`               | ctor     | descale out of the engine plane   |
|  [23]   | `Point64 + Point64 -> Point64`          | operator | vector translation                |
|  [24]   | `Point64 - Point64 -> Point64`          | operator | vector difference                 |
|  [25]   | `Point64 == Point64 -> bool`            | operator | ordinate equality                 |
|  [26]   | `PointD == PointD -> bool`              | operator | ordinate equality                 |
|  [27]   | `PointD.Negate()`                       | instance | in-place ordinate negation        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Point64.X` and `Point64.Y` are `long`, so a double entry scales in at `Math.Pow(10, precision)` and descales at the reciprocal; `ClipperD` and the `Clipper` double statics fold that pair around an int64 core, and a hand-spelled `Clipper.ScalePaths64`-clip-`ScalePathsD` sequence reproduces them exactly.
- Precision admits integers `-8` through `8`: `ClipperD`'s constructor rails an out-of-range value as `ClipperLibException`, while the `Clipper` double statics rail it as a bare `Exception` from the internal precision check.
- Every `Execute` clears its caller-supplied solution container before filling it; the overlay engines return `bool`, where `false` marks an internal fault the engine caught rather than a successful empty result, and `ClipperOffset.Execute` returns void.
- `ClipperOffset.Execute(DeltaCallback64, Paths64)` binds the callback and runs at delta `1.0`, so the callback returns each vertex's whole signed magnitude.
- `ClipperBase.Clear()` drops the added path set rather than the last solution alone, and `ClipperBase.GetBounds()` measures the added vertices in engine int64 space.
- `Rect64` and `RectD` expose mutable `left`, `top`, `right`, and `bottom` fields whose settable `Width` and `Height` move `right` and `bottom`; `IsEmpty()` reads true at `bottom <= top` or `right <= left`.
- `Path64`, `Paths64`, `PathD`, and `PathsD` derive `List<T>`, so capacity presizing, collection initializers, and LINQ compose on every input and result without a copy.
- `PolyPath64.Area()` and `PolyPathD.Area()` sum a node's signed area with every descendant's, so a hole ring's negative area nets out and one call yields a subtree's true filled area.

[STACKING]:
- `CavalierContours`(`Rasm.Fabrication/.api/api-cavaliercontours.md`): a bulge-carrying profile crosses in through `ArcsToApproxLines(errorDistance)` to a `PathD`, and a pure-polygon clip or a Minkowski no-fit-polygon construction resolves here while a constant-radius arc result stays on the arc-native peer.
- `DSTV.Net`(`Rasm.Fabrication/.api/api-dstv-net.md`): parsed `DstvHole`, `DstvSlot`, and `Contour` flange `(XCoord, YCoord)` pairs become `Path64` subject and clip rings for the true-shape nest and the projection window.
- `SharpVoronoiLib`(`Rasm.Fabrication/.api/api-sharpvoronoilib.md`): each cell's `ClockwisePoints` ring scales to a `Path64` at the fabrication precision, so a relaxed partition machines through `ClipperOffset` pocketing.
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): NTS owns predicate topology, buffering, and spatial indexing on the double plane; a ring crosses either way through the `Coordinate`-to-`PointD` ordinate pair, and the int64 overlay, offset, and Minkowski locus resolve here.
- Within-library: one `ClipperD` engine folds Boolean, rectangle window, containment, and `PolyTreeD` topology at one bound precision; one `ClipperOffset` engine folds both the constant-delta and the `DeltaCallback64` arities so `MergeGroups`, `PreserveCollinear`, and `ReverseSolution` bind identically on either; `ReuseableDataContainer64` precomputes a recurring part set once and every per-position `Clipper64.Execute` folds over it; `Rect64.Intersects` and `Rect64.Contains` reject a non-overlapping pair before any overlay runs.

[LOCAL_ADMISSION]:
- A one-shot operation folds through the `Clipper` facade; subject or clip state reused across more than one `Execute` selects `Clipper64` or `ClipperD`, and a subject set recurring across a placement scan selects `ReuseableDataContainer64` beside it.
- Double-coordinate work names its precision once — at the `ClipperD` constructor or the static's `precision` argument — and that one value governs every scale-in on the path.
- A hole-aware or nesting result takes the `PolyTree64` or `PolyTreeD` `Execute` arm and reads `Area()`, `IsHole`, and `Level` off the tree; `Clipper.PolyTreeToPaths64` and `Clipper.PolyTreeToPathsD` flatten only where nesting decides nothing downstream.
- Double morphology enters through `Minkowski.Sum` and `Minkowski.Diff` carrying the owner's `decimalPlaces`, since the `Clipper.MinkowskiSum` and `Clipper.MinkowskiDiff` double pair pins precision at `2`.
- `DeltaCallback64` receives the source path, its per-vertex normals, and the current and previous vertex indices, and returns that vertex's signed delta.

[RAIL_LAW]:
- Package: `Clipper2`
- Owns: planar polygon Boolean overlay, path offsetting, rectangle windowing, Minkowski morphology, containment, and path hygiene over an int64 coordinate plane
- Accept: `Path64` and `Paths64` int64 geometry, and `PathD` and `PathsD` double geometry carrying its decimal precision
- Reject: hand-rolled polygon clipping, coordinate scaling spelled beside `Clipper.ScalePaths64` and `Clipper.ScalePathsD`, `ClipperBase` internals reached past `Clipper64` and `ClipperD`, triangulation the kernel triangulation owner holds, and the exact-geometry concern the kernel `Rasm` owns
