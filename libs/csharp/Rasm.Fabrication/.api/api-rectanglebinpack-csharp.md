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

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                  |
| :-----: | :------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `MaxRectsBinPack`   | stateful packer | the maximal-rectangles algorithm — best 2D density, the highest-yield general placement; EXPOSES `UsedRectangles`/`FreeRectangles` lists; optional 90-deg rotation; five placement heuristics |
|  [02]   | `SkylineBinPack`    | stateful packer | the skyline (bottom-left/min-waste) algorithm with an optional Guillotine waste-map; the fast streaming packer for many small uniform parts |
|  [03]   | `GuillotineBinPack` | stateful packer | the guillotine-cut algorithm — the PANEL-SAW straight-cut constraint (every cut spans the stock edge-to-edge); six free-rect-choice heuristics x four split heuristics + optional free-rect merge |
|  [04]   | `ShelfBinPack`      | stateful packer | the shelf (row-band) algorithm with an optional Guillotine waste-map; the simplest row-stacked layout |
|  [05]   | `SingleBinPack`     | mass-cut packer | the identical-part mass-cut case: `Insert(partWidth, partHeight, quantity)` returns the `List<Rect>` of as-many-as-fit placements on one sheet — the sheet-yield count for a homogeneous cut list |
|  [06]   | `Rect`              | mutable struct  | the one placement value carrier — `X`/`Y`/`Width`/`Height` fields, `Right`/`Bottom`/`Area` computed, `Contains`/`Intersects` predicates; `Height == 0` is the universal placement-failure sentinel |

[PUBLIC_TYPE_SCOPE]: `Rect` members
- rail: fabrication

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [CAPABILITY]                                            |
| :-----: | :-------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `Rect(int x, int y, int width, int height)` | constructor | construct a positioned rectangle (the packers return placed instances; the consumer rarely constructs one directly) |
|  [02]   | `int X` / `int Y`                 | mutable field     | the assigned bottom-left position written by `Insert`  |
|  [03]   | `int Width` / `int Height`        | mutable field     | the rectangle extent; `Height == 0` on a returned `Rect` means the part did not fit |
|  [04]   | `int Right`                       | computed property | `X + Width` — the far X edge                           |
|  [05]   | `int Bottom`                      | computed property | `Y + Height` — the far Y edge                          |
|  [06]   | `int Area`                        | computed property | `Width * Height` — the per-part utilization numerator the consumer sums |
|  [07]   | `bool Contains(Rect other)`       | predicate         | true when this rectangle fully encloses `other` — the post-pack within-bin containment check |
|  [08]   | `bool Intersects(Rect other)`     | predicate         | AABB overlap test — the consumer's post-pack no-overlap validation (the suite ships no batch `AnyIntersects`, so the pairwise scan is the consumer's to fold) |

[PUBLIC_TYPE_SCOPE]: per-packer state and lifecycle members
- rail: fabrication
- note: every packer follows the same lifecycle — ctor sets the bin extent, `Init` re-resets it (clears the free/used lists), `Insert` places one part and mutates internal state. `MaxRectsBinPack` and `GuillotineBinPack` surface their placement lists publicly (`UsedRectangles`/`FreeRectangles`); `SkylineBinPack`/`ShelfBinPack` keep theirs private, so the consumer accumulates returned `Rect`s for those.

| [INDEX] | [SYMBOL]                                                            | [PACKER]          | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `MaxRectsBinPack(int width, int height, bool allowRotations = true)` | `MaxRectsBinPack` | construct over a bin extent; `allowRotations` admits the 90-deg part flip that raises density |
|  [02]   | `void Init(int width, int height, bool allowRotations = true)`     | `MaxRectsBinPack` | re-bind the bin extent and clear all placement state — reuse one packer instance across sheets |
|  [03]   | `int BinWidth` / `int BinHeight` / `bool AllowRotations`           | `MaxRectsBinPack` | read-only bin-extent and rotation-policy properties     |
|  [04]   | `List<Rect> UsedRectangles` / `List<Rect> FreeRectangles`          | `MaxRectsBinPack` | the live placement and remaining-free lists — `UsedRectangles` is the utilization source (sum `Area`); `FreeRectangles` is the remnant geometry for a cutting-stock leftover ledger |
|  [05]   | `SkylineBinPack(int width, int height, bool useWasteMap)`          | `SkylineBinPack`  | construct; `useWasteMap` enables the internal Guillotine waste-map recycling of skyline gaps |
|  [06]   | `GuillotineBinPack(int width, int height)`                         | `GuillotineBinPack` | construct over a bin extent (no rotation flag — guillotine geometry is axis-fixed) |
|  [07]   | `void MergeFreeRectangles()`                                       | `GuillotineBinPack` | coalesce adjacent free rectangles after a batch of inserts — the post-pack defragmentation that recovers contiguous remnant area |
|  [08]  | `List<Rect> UsedRectangles` / `List<Rect> FreeRectangles`         | `GuillotineBinPack` | placement and free lists for remnant geometry |
|  [09]   | `ShelfBinPack(int width, int height, bool useWasteMap)`            | `ShelfBinPack`    | construct; `useWasteMap` enables the internal Guillotine waste-map for the dead area under each shelf |
|  [10]   | `SingleBinPack(int width, int height)`                             | `SingleBinPack`   | construct over a sheet extent for the homogeneous mass-cut yield query |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: per-algorithm incremental `Insert` — each takes that packer's own heuristic enum
- rail: fabrication
- note: every `Insert` returns the placed `Rect`; a `Rect` with `Height == 0` is the placement-failure sentinel (the part did not fit the remaining free area). No method throws on infeasibility.

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `MaxRectsBinPack.Insert(int width, int height, FreeRectChoiceHeuristic method)`                                   | maxrects place | place one part by the chosen maximal-rectangles heuristic; mutates `UsedRectangles`/`FreeRectangles`; the densest general placement |
|  [02]   | `SkylineBinPack.Insert(int width, int height, LevelChoiceHeuristic method)`                                       | skyline place  | place one part along the skyline by bottom-left or min-waste level choice |
|  [03]   | `GuillotineBinPack.Insert(int width, int height, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)` | guillotine place | place one part under the panel-saw straight-cut constraint; `merge` coalesces the resulting free rects inline; `rectChoice` selects the free rect, `splitMethod` selects the cut axis — note `FreeRectChoiceHeuristic` here is the NESTED `GuillotineBinPack.FreeRectChoiceHeuristic`, a DIFFERENT enum from the top-level one MaxRects uses |
|  [04]   | `ShelfBinPack.Insert(int width, int height, ShelfChoiceHeuristic method)`                                         | shelf place    | place one part onto a next-fit/first-fit/best-area/best-height/best-width shelf |
|  [05]   | `SingleBinPack.Insert(int partWidth, int partHeight, int quantity)`                                               | mass-cut yield | place up to `quantity` identical parts on one sheet and return the `List<Rect>` of those that fit — the sheet-yield count for a homogeneous cut list (the placement positions, not just the count) |

## [04]-[HEURISTIC_VOCABULARY]

[HEURISTIC_SCOPE]: `RectangleBinPacking.FreeRectChoiceHeuristic` (TOP-LEVEL — the `MaxRectsBinPack.Insert` axis)
- rail: fabrication

| [INDEX] | [SYMBOL]                  | [ORDINAL] | [CAPABILITY]                                       |
| :-----: | :------------------------ | :-------- | :------------------------------------------------- |
|  [01]   | `RectBestShortSideFit`    | 0         | minimize the shorter leftover side — the classic high-density default |
|  [02]   | `RectBestLongSideFit`     | 1         | minimize the longer leftover side                  |
|  [03]   | `RectBestAreaFit`         | 2         | minimize leftover area                             |
|  [04]   | `RectBottomLeftRule`      | 3         | Tetris bottom-left placement                       |
|  [05]   | `RectContactPointRule`    | 4         | maximize shared edge contact — densest interlocking, slowest |

[HEURISTIC_SCOPE]: `GuillotineBinPack.FreeRectChoiceHeuristic` (NESTED — DISTINCT from the top-level enum above)
- rail: fabrication
- note: this nested enum is NOT interchangeable with the top-level `FreeRectChoiceHeuristic`; the consumer must reference `GuillotineBinPack.FreeRectChoiceHeuristic.*` explicitly for the guillotine `Insert`.

| [INDEX] | [SYMBOL]                  | [ORDINAL] | [CAPABILITY]                          |
| :-----: | :------------------------ | :-------- | :------------------------------------ |
|  [01]   | `RectBestAreaFit`         | 0         | minimize leftover area                |
|  [02]   | `RectBestShortSideFit`    | 1         | minimize shorter leftover side        |
|  [03]   | `RectBestLongSideFit`     | 2         | minimize longer leftover side         |
|  [04]   | `RectWorstAreaFit`        | 3         | maximize leftover area (spread parts) |
|  [05]   | `RectWorstShortSideFit`   | 4         | maximize shorter leftover side        |
|  [06]   | `RectWorstLongSideFit`    | 5         | maximize longer leftover side         |

[HEURISTIC_SCOPE]: `GuillotineBinPack.GuillotineSplitHeuristic` (the cut-axis selector — the panel-saw kerf-direction policy)
- rail: fabrication

| [INDEX] | [SYMBOL]                     | [ORDINAL] | [CAPABILITY]                                       |
| :-----: | :--------------------------- | :-------- | :------------------------------------------------- |
|  [01]   | `SplitShorterLeftoverAxis`   | 0         | cut across the shorter leftover dimension          |
|  [02]   | `SplitLongerLeftoverAxis`    | 1         | cut across the longer leftover dimension           |
|  [03]   | `SplitMinimizeArea`          | 2         | choose the split that minimizes the larger remnant |
|  [04]   | `SplitMaximizeArea`          | 3         | choose the split that maximizes the larger remnant |
|  [05]   | `SplitShorterAxis`           | 4         | cut across the shorter bin axis                     |
|  [06]   | `SplitLongerAxis`            | 5         | cut across the longer bin axis                      |

[HEURISTIC_SCOPE]: `SkylineBinPack.LevelChoiceHeuristic` and `ShelfBinPack.ShelfChoiceHeuristic`
- rail: fabrication

| [INDEX] | [ENUM]                  | [SYMBOLS]                                                                                   | [CAPABILITY]                                              |
| :-----: | :---------------------- | :----------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `LevelChoiceHeuristic`  | `LevelBottomLeft`, `LevelMinWasteFit`                                                       | skyline level selection — lowest landing vs minimal gap  |
|  [02]   | `ShelfChoiceHeuristic`  | `ShelfNextFit`, `ShelfFirstFit`, `ShelfBestAreaFit`, `ShelfWorstAreaFit`, `ShelfBestHeightFit`, `ShelfBestWidthFit`, `ShelfWorstWidthFit` | shelf (row-band) selection policy |

## [05]-[IMPLEMENTATION_LAW]

[STATEFUL_INCREMENTAL]:
- Each packer is a MUTABLE, single-bin state machine: construct (or `Init`) to one bin extent, then call `Insert` per part. `Insert` mutates the packer's free/used geometry and returns the placed `Rect`. The consumer drives the part ORDER (the placement quality is order-sensitive — pre-sort parts descending by area/longer-side before streaming for best yield), reads each returned `Rect.X`/`Rect.Y`, and folds it into a scalar placement transform. To pack across MULTIPLE sheets the consumer loops: on a `Height == 0` failure it opens a fresh packer (`Init`) for the next sheet and re-feeds the unplaced part — the suite has no built-in multi-bin driver.
- The `Height == 0` sentinel is the ONLY infeasibility signal: no `Insert` throws a typed packing exception (the suite ships no exception type), so the consumer wraps the placement loop in a `Fin` rail and maps the zero-height return to the typed `FabricationFault.Nest` placement-failure case rather than catching.
- Coordinates are `int`: the `Nesting/stock` owner ceils each part's footprint and the sheet extent to `int` on the way in, and offsets the returned `Rect.X`/`Rect.Y` back into the layout coordinate frame on the way out. There is no `uint` domain — the suite is `int` end to end.
- `MaxRectsBinPack` alone exposes `UsedRectangles`/`FreeRectangles`. `UsedRectangles` is the utilization source — material-utilization = `sum(used.Area) / (BinWidth * BinHeight)`. `FreeRectangles` after a batch is the REMNANT geometry the cutting-stock leftover ledger reads. For the other three packers the consumer accumulates the returned `Rect`s itself to compute the same ratio. `GuillotineBinPack.MergeFreeRectangles()` (or per-insert `merge: true`) coalesces fragmented free area before the remnant read.
- `SingleBinPack.Insert(w, h, quantity)` is the homogeneous mass-cut yield: it returns the `List<Rect>` of as-many-of-the-identical-part-as-fit on one sheet, the sheet-yield count being `list.Count`. The internal nesting machinery it uses (the private `Vector2d`/`MockPart`/`NestingSolution`/`OuterContourPoint` types) does NOT escape the API — only `List<Rect>` is the surface, so the consumer reads positions and count, nothing finer.

[RAIL_LAW]:
- Package: `RectangleBinPack.CSharp` (assembly `RectangleBinPacking`)
- Owns: the academic axis-aligned 2D bin-packing suite — MaxRects (densest general), Skyline (fast streaming), Guillotine (panel-saw straight-cut), Shelf (row-band), and SingleBinPack (homogeneous mass-cut), each a stateful per-bin `Insert` stream over its own heuristic enum, all returning the `Rect` value struct
- Accept: the `Nesting/stock` sheet/stock-material rectangle placement and material-utilization fold — part footprints and sheet extents ceiled to `int`, the algorithm chosen by the layout intent (`Guillotine` for saw-cut sheet goods, `MaxRects` for nest density, `SingleBinPack` for identical-part yield), utilization derived from `Rect.Area` sums, remnants read from `MaxRectsBinPack.FreeRectangles`/`GuillotineBinPack` post-merge
- Reject: any true-shape / irregular-polygon nesting (the suite is AABB-only — irregular feasibility has no owner here); a second rectangular packer beside `StockNest.Pack` (the sibling `Nesting/nfp` rect-fastpath arm is the from-scratch CAM fast-path over the same `MaxRectsBinPack` fold, never a yield engine); a `Rect`/heuristic-enum type escaping the `Nesting/stock` placement fold into an unrelated sibling signature (the int-rect domain crosses to the scalar placement at the one fold); and treating the two `FreeRectChoiceHeuristic` enums as one type (the top-level and `GuillotineBinPack`-nested enums are distinct and non-interchangeable)
