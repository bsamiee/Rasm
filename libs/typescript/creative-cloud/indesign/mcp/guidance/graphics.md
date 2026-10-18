# [GRAPHICS]

## [01]-[SHAPES]

Group existing page items with `Groups.add` when children must remain separate objects. Use compound paths for a shared outline or Pathfinder operations to combine overlapping areas. Continue with returned page items after an operation replaces source geometry.

Set `Path.entirePath` to replace an outline, or edit individual `PathPoint` anchors and direction handles for local changes. Bounds describe an extent, not a path. Use stroke and corner settings to change line appearance without rebuilding geometry; corner effects, stroke joins, and curve handles produce different outlines.

## [02]-[FRAMES]

Edit a frame to change visible bounds; edit placed content to change position or scale inside those bounds. Follow graphics collections and native parent references to identify both objects. Page items can belong to groups, characters, cells, or states, so a parent reference alone does not identify a frame.

Choose a `fit` operation by the required visible result:

| [INDEX] | [RESULT]                     | [FIT OPTION]           | [CONSEQUENCE]                         |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------ |
|  [01]   | Whole content inside frame   | `PROPORTIONALLY`       | Empty space when proportions differ   |
|  [02]   | Content fills frame          | `FILL_PROPORTIONALLY`  | Frame edges conceal excess content    |
|  [03]   | Frame matches content        | `FRAME_TO_CONTENT`     | Frame dimensions change               |
|  [04]   | Content centered at its size | `CENTER_CONTENT`       | Position changes without scaling      |
|  [05]   | Content matches frame bounds | `CONTENT_TO_FRAME`     | Unequal scaling can distort content   |

Set `FrameFittingOption.fittingOnEmptyFrame` for initial placement and `autoFit` when resizing must reapply saved fitting. `APPLY_FRAME_FITTING_OPTIONS` uses current settings, including crop amounts changed by earlier placement. Text-frame auto-sizing changes a text container, independently of graphic fitting.

## [03]-[PLACEMENT]

Place a source document when artwork must retain source content and links. For an InDesign page, set `Application.importedPageAttributes` before placement. For a PDF page or PDF-compatible Illustrator artboard, use `Application.pdfPlacePreferences`. Choose source page and crop bounds before calling `place`; PDF preferences also control background transparency. Restore temporary application preferences after placement, including on failure.

Source-page cropping establishes imported bounds. Frame fitting then controls how imported content occupies a frame without choosing another source page. Retain returned graphics for content edits instead of treating containing frames as the imported artwork.

Use `GraphicLayerOption` to select source-layer visibility and behavior during link updates. Source layers remain part of placed content, separate from document layers. Use `ClippingPathSettings` when a source contour must conceal image background; frame edges and text-wrap contours remain independent boundaries.

Judge raster resolution at placed scale through `Image.effectivePpi`; `actualPpi` describes source resolution. Resizing a frame without scaling content does not change effective resolution.

## [04]-[SWATCHES]

Reuse document swatch references for shared color. Choose process or spot through `Color.model`, then choose component encoding through `Color.space`. Mixed color spaces can coexist in one document; component ranges follow the selected space.

Use a `Tint` to retain a base color with a percentage, or a `MixedInk` to combine native ink references and amounts. Apply swatches to fill or stroke before choosing tint and overprint behavior. Tint changes ink amount; opacity changes compositing.

Edit gradient stops for shared colors, locations, and midpoints. Set gradient origin, length, and angle on a page item when placement must differ between objects using one swatch. Organize existing swatches with `ColorGroup`; merging or removing swatches with native replacement arguments updates document references.

## [05]-[COLOR_MANAGEMENT]

Assign a document profile when existing component values need a different color interpretation. Select profiles from native lists on `Document`. Application `ColorSetting` controls working spaces, profile policies, color engine, and rendering intent; a placed `Image` has separate profile and intent settings.

Changing swatch components, assigning profiles, and converting output colors produce different results. RGB and CMYK equivalents depend on source and destination profiles, so numeric pairs from another document do not define a universal conversion.

Set document `TransparencyPreference.blendingSpace` for transparent compositing. Blend space affects transparent spreads without converting every swatch.

## [06]-[TRANSPARENCY]

Apply an effect to the component that must change: object, fill, stroke, or content. For a translucent text-frame background with opaque text, change fill transparency. Changing object transparency affects the composite. Placed graphics retain effects independently of containing frames.

Choose a component's transparency settings before changing nested blending or effect settings. For overlapping group children, isolate blending to confine interactions within a group; knockout controls how children obscure one another. Group and child settings both affect the result.

Use document global light for effects that must share direction. Effect settings with `useGlobalLight` enabled follow document `TransparencyPreference` light angle and altitude; independent lighting belongs to the effect.

## [07]-[TEXT_WRAP]

Set `TextWrapPreference` on the object that displaces text. Choose a bounding-box wrap for rectangular clearance, contour wrap for a silhouette, or column behavior to move text past artwork. Offset forms depend on wrap mode. Contour settings select native clipping paths, source alpha channels, source paths, or detected edges without changing visible image clipping.

When a wrap fails to move text, check receiving frames' `TextFramePreference.ignoreWrap` before changing offsets. Composition preferences can restrict wrapping to text beneath an object. Parent-page wrap scope controls whether wrap affects document pages; `Layer.ignoreWrap` suppresses a hidden layer's wrap on other layers.
