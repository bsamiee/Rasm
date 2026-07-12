# [RASM_FABRICATION_API_RECTANGLEBINPACK_CSHARP]

`RectangleBinPack.CSharp` is a pure-managed, MIT-licensed, AnyCPU IL-only C# port of Jukka Jylanki's RectangleBinPack — the canonical academic axis-aligned 2D bin-packing suite — exposing four independent stateful packer classes (`MaxRectsBinPack`, `SkylineBinPack`, `GuillotineBinPack`, `ShelfBinPack`) plus the mass-cut `SingleBinPack`, each over an explicit per-algorithm heuristic enum, all returning the one mutable `Rect` value struct. The assembly id is `RectangleBinPacking` (NOT the package id `RectangleBinPack.CSharp`); the single namespace is `RectangleBinPacking`. The `Nesting/stock` design owner consumes it as the 2D rectangle bin-packing / cutting-stock placement engine for sheet/stock-material arrangement and material-utilization. The two admitted packers own DISJOINT concerns inside the package: this suite is the academic MaxRects/Skyline/Guillotine/Shelf family with the panel-saw `Guillotine` straight-cut constraint and the `SingleBinPack` mass-cut sheet-yield case (the material-planning YIELD the `StockNest.Pack` fold drives), while the sibling `Nesting/nfp` rect-fastpath arm of the true-shape CAM nest rides this same suite — a heuristic sweep over one `MaxRectsBinPack.Insert` fold; neither concern re-packs the other, and a `Rect`/heuristic-enum type in a signature outside `Nesting/stock` is the seam violation.

The packers are STATEFUL and INCREMENTAL: each is constructed (or `Init`-reset) to one bin extent, then fed parts one `Insert` at a time, each call mutating the packer's internal free/used lists and returning the placed `Rect` (a `Height == 0` rectangle is the placement-failure sentinel — the part did not fit). The consumer drives the part stream, reads each returned `Rect`'s assigned `X`/`Y`, and folds it back to a scalar placement, treating `Height == 0` as the infeasibility signal (no exception is thrown — the suite never throws a typed packing failure). There is NO built-in utilization/occupancy metric: the consumer derives material-utilization itself from the summed `Rect.Area` of placements against the bin `width * height`, reading `MaxRectsBinPack.UsedRectangles` (the one packer exposing its placement list) or accumulating returned `Rect`s for the others.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RectangleBinPack.CSharp`
- package: `RectangleBinPack.CSharp`
- license: `MIT`
- assembly: `RectangleBinPacking` (`lib/netstandard2.0/RectangleBinPacking.dll` — SINGLE TFM, the only asset; the assembly name diverges from the package id)
- namespace: `RectangleBinPacking`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe)
- dependencies: NONE (`.NETStandard2.0` empty dependency group — zero transitive closure)
- consumer-bind note: single-TFM `netstandard2.0`, so the `net10.0` consumer binds the one asset directly; the `[API_TFM_RESOLUTION]` multi-target hazard does NOT apply — there is no fallback TFM to misresolve
- rail: fabrication (`Nesting/stock` 2D cutting-stock placement)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: packer classes and the rectangle struct
- rail: fabrication

`MaxRectsBinPack` maximizes general placement density through five heuristics and optional 90-degree rotation, and it exposes `UsedRectangles` and `FreeRectangles`. `SkylineBinPack` streams small uniform parts through bottom-left or minimum-waste placement and an optional Guillotine waste map. `GuillotineBinPack` enforces panel-saw straight cuts spanning the stock edge to edge through six free-rectangle choices, four split heuristics, and optional free-rectangle merging. `ShelfBinPack` owns the simplest row-stacked layout through an optional Guillotine waste map. `SingleBinPack.Insert(partWidth, partHeight, quantity)` returns the as-many-as-fit `List<Rect>` for a homogeneous cut list. `Rect` carries mutable placement fields, computed extents and area, containment and intersection predicates, and the universal `Height == 0` failure sentinel.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                |
| :-----: | :------------------ | :-------------- | :-------------------------- |
|  [01]   | `MaxRectsBinPack`   | stateful packer | maximal-rectangle placement |
|  [02]   | `SkylineBinPack`    | stateful packer | skyline placement           |
|  [03]   | `GuillotineBinPack` | stateful packer | straight-cut placement      |
|  [04]   | `ShelfBinPack`      | stateful packer | row-band placement          |
|  [05]   | `SingleBinPack`     | mass-cut packer | homogeneous sheet yield     |
|  [06]   | `Rect`              | mutable struct  | placement value carrier     |

[PUBLIC_TYPE_SCOPE]: `Rect` members
- rail: fabrication

The packers return positioned `Rect` instances, so the consumer rarely calls the constructor. `Insert` writes the mutable bottom-left position and extent fields, and `Height == 0` marks a part that did not fit. `Contains` supplies the post-pack within-bin check. `Intersects` supplies the pairwise post-pack no-overlap check because the suite has no batch `AnyIntersects` surface.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]     | [CAPABILITY]         |
| :-----: | :------------------------------------------ | :---------------- | :------------------- |
|  [01]   | `Rect(int x, int y, int width, int height)` | constructor       | positioned rectangle |
|  [02]   | `int X`                                     | mutable field     | horizontal position  |
|  [03]   | `int Y`                                     | mutable field     | vertical position    |
|  [04]   | `int Width`                                 | mutable field     | horizontal extent    |
|  [05]   | `int Height`                                | mutable field     | vertical extent      |
|  [06]   | `int Right`                                 | computed property | `X + Width`          |
|  [07]   | `int Bottom`                                | computed property | `Y + Height`         |
|  [08]   | `int Area`                                  | computed property | `Width * Height`     |
|  [09]   | `bool Contains(Rect other)`                 | predicate         | full-enclosure test  |
|  [10]   | `bool Intersects(Rect other)`               | predicate         | AABB overlap test    |

[PUBLIC_TYPE_SCOPE]: per-packer state and lifecycle members
- rail: fabrication
- note: every packer follows the same lifecycle — ctor sets the bin extent, `Init` re-resets it (clears the free/used lists), `Insert` places one part and mutates internal state. `MaxRectsBinPack` and `GuillotineBinPack` surface their placement lists publicly (`UsedRectangles`/`FreeRectangles`); `SkylineBinPack`/`ShelfBinPack` keep theirs private, so the consumer accumulates returned `Rect`s for those.

| [INDEX] | [SYMBOL]                                                             | [PACKER]            | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------- | :------------------ | :--------------------------- |
|  [01]   | `MaxRectsBinPack(int width, int height, bool allowRotations = true)` | `MaxRectsBinPack`   | construct with rotation      |
|  [02]   | `void Init(int width, int height, bool allowRotations = true)`       | `MaxRectsBinPack`   | reset extent and state       |
|  [03]   | `int BinWidth`                                                       | `MaxRectsBinPack`   | bin width                    |
|  [04]   | `int BinHeight`                                                      | `MaxRectsBinPack`   | bin height                   |
|  [05]   | `bool AllowRotations`                                                | `MaxRectsBinPack`   | rotation policy              |
|  [06]   | `List<Rect> UsedRectangles`                                          | `MaxRectsBinPack`   | utilization placement list   |
|  [07]   | `List<Rect> FreeRectangles`                                          | `MaxRectsBinPack`   | cutting-stock remnant list   |
|  [08]   | `SkylineBinPack(int width, int height, bool useWasteMap)`            | `SkylineBinPack`    | construct with gap recycling |
|  [09]   | `GuillotineBinPack(int width, int height)`                           | `GuillotineBinPack` | construct axis-fixed state   |
|  [10]   | `void MergeFreeRectangles()`                                         | `GuillotineBinPack` | coalesce adjacent free space |
|  [11]   | `List<Rect> UsedRectangles`                                          | `GuillotineBinPack` | placement list               |
|  [12]   | `List<Rect> FreeRectangles`                                          | `GuillotineBinPack` | remnant geometry             |
|  [13]   | `ShelfBinPack(int width, int height, bool useWasteMap)`              | `ShelfBinPack`      | construct with gap recycling |
|  [14]   | `SingleBinPack(int width, int height)`                               | `SingleBinPack`     | construct mass-cut state     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: per-algorithm incremental `Insert` — each takes that packer's own heuristic enum
- rail: fabrication
- note: every `Insert` returns the placed `Rect`; a `Rect` with `Height == 0` is the placement-failure sentinel (the part did not fit the remaining free area). No method throws on infeasibility.

`MaxRectsBinPack.Insert` mutates `UsedRectangles` and `FreeRectangles` through the selected maximal-rectangles heuristic. `SkylineBinPack.Insert` selects bottom-left or minimum-waste skyline placement. `GuillotineBinPack.Insert` enforces the panel-saw straight-cut constraint; `merge` coalesces free rectangles inline, `rectChoice` selects the free rectangle, and `splitMethod` selects the cut axis. Its `FreeRectChoiceHeuristic` is the distinct nested `GuillotineBinPack.FreeRectChoiceHeuristic`, not the top-level MaxRects enum. `ShelfBinPack.Insert` selects a next-fit, first-fit, best-area, best-height, or best-width shelf. `SingleBinPack.Insert` returns the positioned `List<Rect>` containing up to `quantity` identical parts that fit on one sheet.

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY]   | [CAPABILITY]           |
| :-----: | :------------------------- | :--------------- | :--------------------- |
|  [01]   | `MaxRectsBinPack.Insert`   | maxrects place   | density heuristic      |
|  [02]   | `SkylineBinPack.Insert`    | skyline place    | level heuristic        |
|  [03]   | `GuillotineBinPack.Insert` | guillotine place | panel-saw split        |
|  [04]   | `ShelfBinPack.Insert`      | shelf place      | shelf heuristic        |
|  [05]   | `SingleBinPack.Insert`     | mass-cut yield   | homogeneous placements |

[INSERT_SIGNATURES]:
- `MaxRectsBinPack.Insert`: `(int width, int height, FreeRectChoiceHeuristic method)`
- `SkylineBinPack.Insert`: `(int width, int height, LevelChoiceHeuristic method)`
- `GuillotineBinPack.Insert`: `(int width, int height, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)`
- `ShelfBinPack.Insert`: `(int width, int height, ShelfChoiceHeuristic method)`
- `SingleBinPack.Insert`: `(int partWidth, int partHeight, int quantity)`

## [04]-[HEURISTIC_VOCABULARY]

[HEURISTIC_SCOPE]: `RectangleBinPacking.FreeRectChoiceHeuristic` 'TOP-LEVEL — the `MaxRectsBinPack.Insert` axis'
- rail: fabrication

| [INDEX] | [SYMBOL]               | [ORDINAL] | [CAPABILITY]                                                          |
| :-----: | :--------------------- | :-------- | :-------------------------------------------------------------------- |
|  [01]   | `RectBestShortSideFit` | 0         | minimize the shorter leftover side — the classic high-density default |
|  [02]   | `RectBestLongSideFit`  | 1         | minimize the longer leftover side                                     |
|  [03]   | `RectBestAreaFit`      | 2         | minimize leftover area                                                |
|  [04]   | `RectBottomLeftRule`   | 3         | Tetris bottom-left placement                                          |
|  [05]   | `RectContactPointRule` | 4         | maximize shared edge contact — densest interlocking, slowest          |

[HEURISTIC_SCOPE]: `GuillotineBinPack.FreeRectChoiceHeuristic` 'NESTED — DISTINCT from the top-level enum above'
- rail: fabrication
- note: this nested enum is NOT interchangeable with the top-level `FreeRectChoiceHeuristic`; the consumer must reference `GuillotineBinPack.FreeRectChoiceHeuristic.*` explicitly for the guillotine `Insert`.

| [INDEX] | [SYMBOL]                | [ORDINAL] | [CAPABILITY]                          |
| :-----: | :---------------------- | :-------- | :------------------------------------ |
|  [01]   | `RectBestAreaFit`       | 0         | minimize leftover area                |
|  [02]   | `RectBestShortSideFit`  | 1         | minimize shorter leftover side        |
|  [03]   | `RectBestLongSideFit`   | 2         | minimize longer leftover side         |
|  [04]   | `RectWorstAreaFit`      | 3         | maximize leftover area (spread parts) |
|  [05]   | `RectWorstShortSideFit` | 4         | maximize shorter leftover side        |
|  [06]   | `RectWorstLongSideFit`  | 5         | maximize longer leftover side         |

[HEURISTIC_SCOPE]: `GuillotineBinPack.GuillotineSplitHeuristic` 'the cut-axis selector — the panel-saw kerf-direction policy'
- rail: fabrication

| [INDEX] | [SYMBOL]                   | [ORDINAL] | [CAPABILITY]                                       |
| :-----: | :------------------------- | :-------- | :------------------------------------------------- |
|  [01]   | `SplitShorterLeftoverAxis` | 0         | cut across the shorter leftover dimension          |
|  [02]   | `SplitLongerLeftoverAxis`  | 1         | cut across the longer leftover dimension           |
|  [03]   | `SplitMinimizeArea`        | 2         | choose the split that minimizes the larger remnant |
|  [04]   | `SplitMaximizeArea`        | 3         | choose the split that maximizes the larger remnant |
|  [05]   | `SplitShorterAxis`         | 4         | cut across the shorter bin axis                    |
|  [06]   | `SplitLongerAxis`          | 5         | cut across the longer bin axis                     |

[HEURISTIC_SCOPE]: `SkylineBinPack.LevelChoiceHeuristic` and `ShelfBinPack.ShelfChoiceHeuristic`
- rail: fabrication

`LevelBottomLeft` selects the lowest skyline landing, and `LevelMinWasteFit` selects the minimum skyline gap. `ShelfChoiceHeuristic` selects the row-band shelf policy.

| [INDEX] | [ENUM]                 | [SYMBOL]             |
| :-----: | :--------------------- | :------------------- |
|  [01]   | `LevelChoiceHeuristic` | `LevelBottomLeft`    |
|  [02]   | `LevelChoiceHeuristic` | `LevelMinWasteFit`   |
|  [03]   | `ShelfChoiceHeuristic` | `ShelfNextFit`       |
|  [04]   | `ShelfChoiceHeuristic` | `ShelfFirstFit`      |
|  [05]   | `ShelfChoiceHeuristic` | `ShelfBestAreaFit`   |
|  [06]   | `ShelfChoiceHeuristic` | `ShelfWorstAreaFit`  |
|  [07]   | `ShelfChoiceHeuristic` | `ShelfBestHeightFit` |
|  [08]   | `ShelfChoiceHeuristic` | `ShelfBestWidthFit`  |
|  [09]   | `ShelfChoiceHeuristic` | `ShelfWorstWidthFit` |

## [05]-[IMPLEMENTATION_LAW]

[STATEFUL_INCREMENTAL]:
- Each packer is a MUTABLE, single-bin state machine: construct or `Init` to one bin extent, then call `Insert` per part. `Insert` mutates the packer's free/used geometry and returns the placed `Rect`. The consumer pre-sorts parts descending by area or longer side because placement quality is order-sensitive, then folds each returned `Rect.X` and `Rect.Y` into a scalar placement transform.
- To pack across MULTIPLE sheets, the consumer opens a fresh packer through `Init` after a `Height == 0` failure and re-feeds the unplaced part because the suite has no built-in multi-bin driver.
- The `Height == 0` sentinel is the ONLY infeasibility signal: no `Insert` throws a typed packing exception (the suite ships no exception type), so the consumer wraps the placement loop in a `Fin` rail and maps the zero-height return to the typed `FabricationFault.Nest` placement-failure case rather than catching.
- Coordinates are `int`: the `Nesting/stock` owner ceils each part's footprint and the sheet extent to `int` on the way in, and offsets the returned `Rect.X`/`Rect.Y` back into the layout coordinate frame on the way out. There is no `uint` domain — the suite is `int` end to end.
- `MaxRectsBinPack` alone exposes `UsedRectangles`/`FreeRectangles`. `UsedRectangles` is the utilization source — material-utilization = `sum(used.Area) / (BinWidth * BinHeight)`. `FreeRectangles` after a batch is the REMNANT geometry the cutting-stock leftover ledger reads. For the other three packers the consumer accumulates the returned `Rect`s itself to compute the same ratio. `GuillotineBinPack.MergeFreeRectangles()` (or per-insert `merge: true`) coalesces fragmented free area before the remnant read.
- `SingleBinPack.Insert(w, h, quantity)` is the homogeneous mass-cut yield: it returns the `List<Rect>` of as-many-of-the-identical-part-as-fit on one sheet, the sheet-yield count being `list.Count`. The internal nesting machinery it uses (the private `Vector2d`/`MockPart`/`NestingSolution`/`OuterContourPoint` types) does NOT escape the API — only `List<Rect>` is the surface, so the consumer reads positions and count, nothing finer.

[RAIL_LAW]:
- Package: `RectangleBinPack.CSharp` (assembly `RectangleBinPacking`)
- Owns: the academic axis-aligned 2D bin-packing suite — MaxRects (densest general), Skyline (fast streaming), Guillotine (panel-saw straight-cut), Shelf (row-band), and SingleBinPack (homogeneous mass-cut), each a stateful per-bin `Insert` stream over its own heuristic enum, all returning the `Rect` value struct
- Accept: the `Nesting/stock` sheet/stock-material rectangle placement and material-utilization fold — part footprints and sheet extents ceiled to `int`, the algorithm chosen by the layout intent (`Guillotine` for saw-cut sheet goods, `MaxRects` for nest density, `SingleBinPack` for identical-part yield), utilization derived from `Rect.Area` sums, remnants read from `MaxRectsBinPack.FreeRectangles`/`GuillotineBinPack` post-merge
- Reject: any true-shape / irregular-polygon nesting (the suite is AABB-only — irregular feasibility has no owner here); a second rectangular packer beside `StockNest.Pack` (the sibling `Nesting/nfp` rect-fastpath arm is the from-scratch CAM fast-path over the same `MaxRectsBinPack` fold, never a yield engine); a `Rect`/heuristic-enum type escaping the `Nesting/stock` placement fold into an unrelated sibling signature (the int-rect domain crosses to the scalar placement at the one fold); and treating the two `FreeRectChoiceHeuristic` enums as one type (the top-level and `GuillotineBinPack`-nested enums are distinct and non-interchangeable)
