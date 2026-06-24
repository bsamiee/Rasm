# [RASM_FABRICATION_API_RECTPACKSHARP]

`RectpackSharp` is a pure-managed, MIT-licensed, AnyCPU IL-only axis-aligned-rectangle bin-packing library (multi-targeted `netstandard2.0`/`net5.0`) exposing one static `RectanglePacker.Pack` entrypoint and a `[Flags]`-attributed `PackingHints` heuristic enum; the fabrication folder consumes it as the maxrects/guillotine fast-path placement arm on the `Nesting/nfp#NESTING` `Stock` fold for the planar `Sheet`/`Plate`/`Billet` cases only, the NFP true-shape kernel remaining the irregular owner. The packer is in-place: `Pack` mutates each `PackingRectangle`'s `X`/`Y` to its assigned position and writes the enclosing `bounds` to the `out` parameter, the `Id` field round-tripping the caller's part index so the packed origin folds back to a `PartTransform`. The `Pack` surface is TFM-multiplexed: on `net5.0` (the asset NuGet resolves for the `net10.0` consumer) the sole overload takes `Span<PackingRectangle>`; on the `netstandard2.0` fallback it takes `PackingRectangle[]` — the modern build binds the `Span` form, so the consumer holds a real `PackingRectangle[]` and passes it as a span (implicit `T[] -> Span<T>`), re-reading its mutated elements after the call. No native asset and no RID burden — the package is managed IL, ALC-safe, centrally pinned at `RectpackSharp 1.2.0`. It carries two facade dependencies (`Microsoft.Bcl.HashCode 1.1.1`, `System.Memory 4.5.4`) that the `net10.0` BCL already supplies, so the resolved closure adds no runtime asset.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RectpackSharp`
- package: `RectpackSharp`
- version: `1.2.0`
- license: `MIT`
- assembly: `RectpackSharp` (`lib/net5.0` binds for the `net10.0` consumer; `lib/netstandard2.0` is the fallback asset)
- namespace: `RectpackSharp`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe)
- dependencies: `Microsoft.Bcl.HashCode 1.1.1`, `System.Memory 4.5.4` (BCL-shipped facades on `net10.0`; no runtime asset crosses)
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: packer surface and rectangle struct
- rail: fabrication

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :------------------ | :----------------- | :----------------------------------------------------------- |
|  [01]   | `RectanglePacker`   | static facade      | the one-shot in-place maxrects/guillotine `Pack`, plus the public layout helpers `CalculateTotalArea`/`FindBounds`/`AnyIntersects` over a `ReadOnlySpan<PackingRectangle>` |
|  [02]   | `PackingRectangle`  | mutable struct     | the per-part rectangle carrying its id, size, and assigned position (`IEquatable`/`IComparable`, implicit `System.Drawing.Rectangle` conversions) |
|  [03]   | `PackingHints`      | `[Flags]` enum     | the heuristic-selection flags the packer tries, OR-combinable and decomposed by `PackingHintExtensions.GetFlagsFrom` |
|  [04]   | `PackingHintExtensions` | static extensions | flag decomposition (`GetFlagsFrom`) and in-place heuristic sort (`SortByPackingHint`) over a `Span<PackingRectangle>` — the packer-internal sort surface, public but not consumer state |
|  [05]   | `PackingException`  | `Exception`        | the package's typed failure type (3 ctors); note `Pack` itself throws plain `Exception`/`ArgumentException` rather than `PackingException` |

[PUBLIC_TYPE_SCOPE]: `PackingRectangle` members
- rail: fabrication

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `PackingRectangle(uint x, uint y, uint width, uint height, int id = 0)` | constructor | construct a rectangle to pack, `id` round-tripping the part index |
|  [02]   | `PackingRectangle(Rectangle rectangle, int id = 0)` | constructor | construct from a `System.Drawing.Rectangle` (interop convenience) |
|  [03]   | `uint X` / `uint Y`            | mutable field   | the assigned position (written by `Pack`)              |
|  [04]   | `uint Width` / `uint Height`  | mutable field   | the rectangle dimensions                               |
|  [05]   | `int Id`                      | mutable field   | the caller-owned identifier (the part index)           |
|  [06]   | `uint SortKey`                | mutable field   | the packer-internal sort key (set during `Pack`; not consumer state) |
|  [07]   | `uint Right` / `uint Bottom`  | computed property | `X + Width` / `Y + Height` — the far edges            |
|  [08]   | `uint Area`                   | computed property | `Width * Height`                                       |
|  [09]   | `uint Perimeter`              | computed property | `2 * (Width + Height)`                                 |
|  [10]   | `uint BiggerSide`             | computed property | `Max(Width, Height)`                                   |
|  [11]   | `uint PathologicalMultiplier` | computed property | `(max/min) * area` integer skew metric the hint sorts by |
|  [12]   | `bool Contains(in PackingRectangle other)` | predicate | true when this rectangle fully encloses `other` (post-pack containment check) |
|  [13]   | `bool Intersects(in PackingRectangle other)` | predicate | AABB overlap test — the consumer's post-pack no-overlap validation |
|  [14]   | `PackingRectangle Intersection(in PackingRectangle other)` | projection | the overlap rectangle of the two AABBs (`default` when disjoint) — measures collision extent when `Intersects` flags a packer violation |

[PUBLIC_TYPE_SCOPE]: `PackingHints` flags
- rail: fabrication

| [INDEX] | [SYMBOL]                        | [VALUE] | [CAPABILITY]                                       |
| :-----: | :------------------------------ | :------ | :------------------------------------------------- |
|  [01]   | `TryByArea`                     | 1       | sort candidates by descending area                 |
|  [02]   | `TryByPerimeter`                | 2       | sort by descending perimeter                       |
|  [03]   | `TryByBiggerSide`               | 4       | sort by descending bigger side                     |
|  [04]   | `TryByWidth`                    | 8       | sort by descending width                           |
|  [05]   | `TryByHeight`                   | 16      | sort by descending height                          |
|  [06]   | `TryByPathologicalMultiplier`   | 32      | sort by the skew multiplier                         |
|  [07]   | `FindBest`                      | 63      | try every heuristic and keep the densest result    |
|  [08]   | `UnusualSizes`                  | 38      | the perimeter/bigger-side/pathological subset       |
|  [09]   | `MostlySquared`                 | 29      | the area/bigger-side/width/height subset            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: in-place pack — `RectanglePacker` facade
- rail: fabrication

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `RectanglePacker.Pack(Span<PackingRectangle> rectangles, out PackingRectangle bounds, PackingHints packingHint = PackingHints.FindBest, double acceptableDensity = 1.0, uint stepSize = 1, uint? maxBoundsWidth = null, uint? maxBoundsHeight = null)` | static pack | the sole `net5.0` overload (binds for `net10.0`) — pack the span in place, write the enclosing `bounds`, honour the optional max-bounds cap; the caller passes its `PackingRectangle[]` as a span and re-reads the mutated elements after the call. The `netstandard2.0` fallback exposes the identical signature with `PackingRectangle[]` in place of `Span<PackingRectangle>`. Throws plain `Exception` on solver failure, `ArgumentNullException`/`ArgumentOutOfRangeException`/`ArgumentException` on bad inputs (`stepSize == 0`, non-real `acceptableDensity`, zero `maxBounds*`, no valid hint) |
|  [02]   | `RectanglePacker.CalculateTotalArea(ReadOnlySpan<PackingRectangle> rectangles)` | static metric | the summed `Area` of the parts — the utilization numerator, read directly instead of re-summing `Width*Height` |
|  [03]   | `RectanglePacker.FindBounds(ReadOnlySpan<PackingRectangle> rectangles)` | static metric | the smallest enclosing rectangle of an already-positioned span (min `X`/`Y`, max `Right`/`Bottom`); the re-derivation of `bounds` after the consumer offsets packed positions |
|  [04]   | `RectanglePacker.AnyIntersects(ReadOnlySpan<PackingRectangle> rectangles)` | static predicate | true if any two parts in the span overlap — the package-owned O(n²) no-overlap validator the consumer asserts instead of hand-rolling a pairwise `Intersects` loop |

## [04]-[IMPLEMENTATION_LAW]

[IN_PLACE_PACK]:
- `Pack` MUTATES each element's `X`/`Y` to its assigned position; the `Id` field is untouched and round-trips the part index (the XML contract states it is "never touched"), so the consumer keys the packed origin back to its `PartTransform` by `Id`. On the `net5.0`-binding `net10.0` build the parameter is `Span<PackingRectangle>`; the consumer holds a real `PackingRectangle[]`, passes it as a span via the implicit `T[] -> Span<T>` conversion, and re-reads the same array after the call. (`Pack` internally copies the input into working arrays before mutating, then copies the winning layout back into the caller's storage, so the elements are repositioned but not reordered.)
- `bounds` (the `out`) carries the enclosing rectangle of the packed result, always anchored at `X=0`/`Y=0`, so its `Width`/`Height` are the consumed sheet extent the utilization metric reads directly (`Area = Width * Height` against the sum of packed-part areas)
- `maxBoundsWidth`/`maxBoundsHeight` bound the packer to the planar `Stock` extent (the sheet `Width`/`Height` floored to `uint`, each `> 0` or `Pack` throws); `null` maps internally to `uint.MaxValue` (no limit). When no layout fits the bounded area `Pack` does NOT silently drop or leave parts in place — it throws plain `Exception` ("Failed to find a solution"), so the consumer wraps the call in a `Fin` rail and treats the throw as the infeasibility signal rather than relying on a post-filter
- `acceptableDensity` (the early-exit density target, a `double`, must be a real number) and `stepSize` (the bin-shrink granularity, a `uint`, must be `> 0`) tune the search; the defaults (`1.0`, `1`) run the exhaustive `FindBest` search to maximal density
- coordinates are `uint`: the consumer ceils each part's `BoundingBox` diagonal to `uint` on the way in and offsets the packed `X`/`Y` by the part's `Bound().Min` on the way back so the `PartTransform` translation lands the local origin
- the packed result is validated with the package-owned `RectanglePacker.AnyIntersects(rectangles)` (the O(n²) pairwise `Intersects` scan — no two packed rectangles may overlap, `PackingRectangle.Intersection` yielding the offending overlap extent for the diagnostic), and every packed rectangle must `Contains`-fit the `bounds`; the consumer asserts a clean layout before folding to `PartTransform` instead of hand-rolling the pairwise loop

[RAIL_LAW]:
- Package: `RectpackSharp`
- Owns: axis-aligned-rectangle maxrects/guillotine bin packing over a caller-owned `Span<PackingRectangle>` (the `net5.0` asset that binds for `net10.0`; the `netstandard2.0` fallback packs a `PackingRectangle[]`), plus the `CalculateTotalArea`/`FindBounds`/`AnyIntersects` span metrics
- Accept: the planar `Sheet`/`Plate`/`Billet` `Stock` fast-path placement keyed off the `NestPolicy.Mode` `rect-fastpath` discriminant, the part bounding boxes ceiled to `uint`, the `PackingHints` heuristic and the `maxBoundsWidth`/`maxBoundsHeight` sheet cap
- Reject: a `BarStock`/`TubeStock`/`FromRemnant` true-shape placement (those route the NFP owner — `RectpackSharp` is AABB-only and cannot express irregular feasibility), a `PackingRectangle`/`PackingHints` type escaping the `rect-fastpath` arm into a sibling-kernel signature (the int-rect domain crosses to `PartTransform` at the one arm), and the dropped `MaxRect`/`BinPack.NET` packers owning the same AABB concern with no maintained edge
