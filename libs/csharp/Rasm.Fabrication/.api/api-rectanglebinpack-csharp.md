# [RASM_FABRICATION_API_RECTANGLEBINPACK_CSHARP]

`RectangleBinPack.CSharp` is the pure-managed C# port of Jylanki's academic axis-aligned 2D bin-packing suite: four stateful per-bin packers (`MaxRectsBinPack`, `SkylineBinPack`, `GuillotineBinPack`, `ShelfBinPack`) and the mass-cut `SingleBinPack`, each an incremental `Insert` stream over its own heuristic enum, all returning the mutable `Rect` struct. `Nesting/stock` consumes it as the sheet-material rectangle placement and utilization engine, from panel-saw straight cuts to homogeneous sheet yield. Placement failure is the `Height == 0` sentinel, never a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RectangleBinPack.CSharp`
- package: `RectangleBinPack.CSharp` (`MIT`)
- assembly: `RectangleBinPacking` (`lib/netstandard2.0/RectangleBinPacking.dll`; single TFM, the sole asset — the assembly name diverges from the package id)
- namespace: `RectangleBinPacking`
- asset: pure-managed AnyCPU IL, zero package dependencies — no native asset, no RID burden, ALC-safe; the `net10.0` consumer binds the one `netstandard2.0` asset directly
- rail: fabrication (`Nesting/stock` 2D cutting-stock placement)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: packer classes and the rectangle struct

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                |
| :-----: | :------------------ | :-------------- | :-------------------------- |
|  [01]   | `MaxRectsBinPack`   | stateful packer | maximal-rectangle placement |
|  [02]   | `SkylineBinPack`    | stateful packer | skyline placement           |
|  [03]   | `GuillotineBinPack` | stateful packer | straight-cut placement      |
|  [04]   | `ShelfBinPack`      | stateful packer | row-band placement          |
|  [05]   | `SingleBinPack`     | mass-cut packer | homogeneous sheet yield     |
|  [06]   | `Rect`              | mutable struct  | placement value carrier     |

[PUBLIC_TYPE_SCOPE]: `Rect` value members

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [CAPABILITY]         |
| :-----: | :------------------------- | :---------------- | :------------------- |
|  [01]   | `Rect(int, int, int, int)` | constructor       | positioned rectangle |
|  [02]   | `X`                        | mutable field     | horizontal position  |
|  [03]   | `Y`                        | mutable field     | vertical position    |
|  [04]   | `Width`                    | mutable field     | horizontal extent    |
|  [05]   | `Height`                   | mutable field     | vertical extent      |
|  [06]   | `Right`                    | computed property | `X + Width`          |
|  [07]   | `Bottom`                   | computed property | `Y + Height`         |
|  [08]   | `Area`                     | computed property | `Width * Height`     |
|  [09]   | `Contains(Rect)`           | predicate         | full-enclosure test  |
|  [10]   | `Intersects(Rect)`         | predicate         | AABB overlap test    |

[PUBLIC_TYPE_SCOPE]: per-packer state and lifecycle members
- `MaxRectsBinPack` and `GuillotineBinPack` surface `UsedRectangles`/`FreeRectangles` publicly; `SkylineBinPack` and `ShelfBinPack` keep placements private, so their consumers accumulate the returned `Rect`s.

| [INDEX] | [SYMBOL]                          | [PACKER]            | [CAPABILITY]                 |
| :-----: | :-------------------------------- | :------------------ | :--------------------------- |
|  [01]   | `MaxRectsBinPack(int, int, bool)` | `MaxRectsBinPack`   | construct with rotation      |
|  [02]   | `Init(int, int, bool)`            | `MaxRectsBinPack`   | reset extent and state       |
|  [03]   | `BinWidth`                        | `MaxRectsBinPack`   | bin width                    |
|  [04]   | `BinHeight`                       | `MaxRectsBinPack`   | bin height                   |
|  [05]   | `AllowRotations`                  | `MaxRectsBinPack`   | rotation policy              |
|  [06]   | `UsedRectangles`                  | `MaxRectsBinPack`   | utilization placement list   |
|  [07]   | `FreeRectangles`                  | `MaxRectsBinPack`   | cutting-stock remnant list   |
|  [08]   | `SkylineBinPack(int, int, bool)`  | `SkylineBinPack`    | construct with gap recycling |
|  [09]   | `GuillotineBinPack(int, int)`     | `GuillotineBinPack` | construct axis-fixed state   |
|  [10]   | `MergeFreeRectangles()`           | `GuillotineBinPack` | coalesce adjacent free space |
|  [11]   | `UsedRectangles`                  | `GuillotineBinPack` | placement list               |
|  [12]   | `FreeRectangles`                  | `GuillotineBinPack` | remnant geometry             |
|  [13]   | `ShelfBinPack(int, int, bool)`    | `ShelfBinPack`      | construct with gap recycling |
|  [14]   | `SingleBinPack(int, int)`         | `SingleBinPack`     | construct mass-cut state     |

[PUBLIC_TYPE_SCOPE]: per-algorithm heuristic enums — each `Insert` takes its own packer's enum; the top-level and `GuillotineBinPack`-nested `FreeRectChoiceHeuristic` are distinct, non-interchangeable types
- `FreeRectChoiceHeuristic`: `RectBestShortSideFit` `RectBestLongSideFit` `RectBestAreaFit` `RectBottomLeftRule` `RectContactPointRule`
- `GuillotineBinPack.FreeRectChoiceHeuristic`: `RectBestAreaFit` `RectBestShortSideFit` `RectBestLongSideFit` `RectWorstAreaFit` `RectWorstShortSideFit` `RectWorstLongSideFit`
- `GuillotineBinPack.GuillotineSplitHeuristic`: `SplitShorterLeftoverAxis` `SplitLongerLeftoverAxis` `SplitMinimizeArea` `SplitMaximizeArea` `SplitShorterAxis` `SplitLongerAxis`
- `SkylineBinPack.LevelChoiceHeuristic`: `LevelBottomLeft` `LevelMinWasteFit`
- `ShelfBinPack.ShelfChoiceHeuristic`: `ShelfNextFit` `ShelfFirstFit` `ShelfBestAreaFit` `ShelfWorstAreaFit` `ShelfBestHeightFit` `ShelfBestWidthFit` `ShelfWorstWidthFit`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: per-algorithm incremental `Insert` — one part per call, returning the placed `Rect` (`Height == 0` marks the part that did not fit)

| [INDEX] | [SURFACE]                                                                                     | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `MaxRectsBinPack.Insert(int, int, FreeRectChoiceHeuristic)`                                   | instance | maximal-rectangle density |
|  [02]   | `SkylineBinPack.Insert(int, int, LevelChoiceHeuristic)`                                       | instance | skyline level fit         |
|  [03]   | `GuillotineBinPack.Insert(int, int, bool, FreeRectChoiceHeuristic, GuillotineSplitHeuristic)` | instance | panel-saw straight cut    |
|  [04]   | `ShelfBinPack.Insert(int, int, ShelfChoiceHeuristic)`                                         | instance | row-band shelf fit        |
|  [05]   | `SingleBinPack.Insert(int, int, int) -> List<Rect>`                                           | fold     | homogeneous sheet yield   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each packer is a mutable single-bin state machine: construct or `Init` to one bin extent, then `Insert` per part, each call mutating the free/used geometry and returning the placed `Rect`; the consumer pre-sorts parts descending by area or longer side (placement is order-sensitive) and folds each returned `Rect.X`/`Rect.Y` into a scalar placement transform.
- `Height == 0` is the sole infeasibility signal — no `Insert` throws, and the suite ships no exception type — so the placement loop wraps in a `Fin` rail mapping the zero-height return to the typed `FabricationFault.Nest` case.
- Coordinates are `int` end to end with no `uint` domain: the `Nesting/stock` owner ceils each part footprint and the sheet extent to `int` inbound and offsets the returned `Rect.X`/`Rect.Y` back into the layout frame outbound.
- Utilization is consumer-derived: `MaxRectsBinPack` alone exposes `UsedRectangles`/`FreeRectangles`, giving `sum(used.Area) / (BinWidth * BinHeight)` and the cutting-stock remnant ledger; the other packers accumulate returned `Rect`s. `GuillotineBinPack.MergeFreeRectangles()` (or per-insert `merge: true`) coalesces fragmented free area before the remnant read.
- Multi-sheet packing opens a fresh packer through `Init` after a `Height == 0` failure and re-feeds the unplaced part — the suite carries no built-in multi-bin driver.
- `SingleBinPack.Insert(w, h, quantity)` returns the `List<Rect>` of as-many-identical-parts-as-fit on one sheet, yield being `list.Count`; its private nesting machinery (`Vector2d`/`MockPart`/`NestingSolution`/`OuterContourPoint`) never escapes the API, so only `List<Rect>` is the surface.

[STACKING]:
- `Nesting/stock` (within-lib): `StockNest.Pack` selects the packer by layout intent — `GuillotineBinPack` for saw-cut sheet goods, `MaxRectsBinPack` for nest density, `SingleBinPack` for identical-part yield — folds each returned `Rect.X`/`Rect.Y` to the scalar placement, and reads `MaxRectsBinPack.FreeRectangles` or post-merge `GuillotineBinPack.FreeRectangles` as the remnant ledger.
- `Nesting/nfp` (within-lib): the rect-fastpath arm of the true-shape CAM nest runs one `MaxRectsBinPack.Insert` heuristic sweep over this same suite; neither concern re-packs the other, and a `Rect` or heuristic-enum type crossing into any signature outside `Nesting/stock` is the seam violation.
- cross-`.api`: none — the int-rect suite carries zero package dependencies and composes with no sibling catalogue.

[LOCAL_ADMISSION]:
- `Nesting/stock` is the sole consumer: build one packer per sheet, feed parts `Insert`-at-a-time in descending-area order, and map `Height == 0` to `FabricationFault.Nest`; the guillotine `Insert` references `GuillotineBinPack.FreeRectChoiceHeuristic.*` explicitly — its nested enum is a distinct type from the top-level `FreeRectChoiceHeuristic`.

[RAIL_LAW]:
- Package: `RectangleBinPack.CSharp` (assembly `RectangleBinPacking`)
- Owns: the academic axis-aligned 2D bin-packing suite — `MaxRectsBinPack` densest general, `SkylineBinPack` fast streaming, `GuillotineBinPack` panel-saw straight-cut, `ShelfBinPack` row-band, `SingleBinPack` homogeneous mass-cut — each a stateful per-bin `Insert` stream over its own heuristic enum, all returning `Rect`.
- Accept: the `Nesting/stock` sheet-placement and material-utilization fold — footprints and sheet extents ceiled to `int`, the algorithm chosen by layout intent, utilization and remnants derived from `Rect.Area` sums and the free-rectangle lists.
- Reject: true-shape or irregular-polygon nesting (the suite is AABB-only); a second rectangular packer beside `StockNest.Pack`; a `Rect` or heuristic-enum type escaping the `Nesting/stock` fold into an unrelated sibling signature; treating the two `FreeRectChoiceHeuristic` enums as one type.
