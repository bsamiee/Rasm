# [DRAFTING]

Drawings and sheets come from `document.py` entry points and RhinoCommon inside `run_python`, with no command prompt.

## [01]-[MAKE2D]

`make2d(doc, view, layer_path, ids, offset)` projects solids, SubD, meshes, curves, and block instances through a view with `HiddenLineDrawing` and returns records of the flat curves it added:
- Visible curves go on `<layer_path>::Visible` and hidden curves on `<layer_path>::Hidden` in the Hidden linetype
- Curves lie on World XY with their lower corner at `offset`
- Block instances project through their definition's geometry in place, the instance stays as it is
- `ids` naming text or points return a `RhinoObject` fault for each, runs over every visible object skip them
- Shapes `HiddenLineDrawing` refuses return a `HiddenLineDrawing` fault each

## [02]-[LAYOUTS]

`sheet(doc, name, size, scale, projection, zoom=())` adds a page with one locked detail over the whole page, `scale` as page units per model units:
- Details center on `zoom` ids or box, `()` every visible object
- `size` is in `doc.PageUnitSystem`
- ARCH D at 1/4 in = 1 ft in a feet document is `sheet(doc, "<name>", (36, 24), (0.25, 1), DefinedViewportProjection.Top)`, `ViewRecord.scale` 48
- `capture(doc, "<name>", view="<page name>", zoom=None)` draws an inactive page at the page view's pixel size
- Page names in use return a `RhinoPageView` fault listing the pages
- Detail cameras move through `detail.Viewport.SetCameraTarget(<point>, True)` and hold after `detail.CommitViewportChanges()`
- `add(doc, geometry, "<A::B>", page="<page name>")` places title text and borders in page units

`pdf(doc, path, pages)` prints the named pages, or every page in page order, into one vector PDF at paper size in print colors:
- Pages print from a windowed document, a headless document's page returns a `RhinoDoc` fault
- Relative paths resolve under `.artifacts/rhino/`
- Unknown page names return a `RhinoPageView` fault listing the pages
- `mutool draw -r 60 -o <root>/.artifacts/rhino/<name>-%d.png <path>` draws the PDF's pages for `Read`
- Print widths override linetype widths in pixels and show under `_PrintDisplay` alone, `Layer.PlotWeight` is millimeters with 0 as default

## [03]-[DIMENSIONS]

```python
# Aligned dimension in the current style
from Rhino.Geometry import AnnotationType, LinearDimension, Plane, Point3d, Vector3d
import document
doc = __rhino_doc__
dimension = LinearDimension.Create(AnnotationType.Aligned, doc.DimStyles.Current, Plane.WorldXY, Vector3d.XAxis, Point3d(0, 0, 0), Point3d(10, 0, 0), Point3d(5, -3, 0), 0.0)
print(document.add(doc, dimension, "<A::B>"))
```

- Style lengths take the drawing space's units, model text is `TextHeight` times `DimensionScale` model units, page text `TextHeight` page units
- Dimension and leader commands add to the document's dimension layer while one is set, other annotation goes on its call's layer
- Layout-space scaling draws annotation in every detail and page at the style's text height at any detail scale
- Style edits change a copy of `doc.DimStyles.Current` and commit through `doc.DimStyles.Modify(style, style.Id, False)`
- `TextHeight` is the style face's cap height
- `LengthResolution` and display precision `p` round to 1/2^p inch, `GetDistanceDisplayText(UnitSystem.Feet, style)` marks stacked fractions `[[ ]]`
- `doc.ModelDistanceDisplayMode` takes `Rhino.UI.DistanceDisplayMode`, the same-named `Rhino.DocObjects` enum lacks `FeetInches`
- `AlternateDimensionLengthDisplay` takes `LengthDisplay.Millmeters`, the API's spelling

## [04]-[SECTIONS_AND_HATCHES]

Documents hold hatch patterns, linetypes, and section styles their template holds, `RhinoDoc.CreateHeadless(None)` and Rhino's own templates hold none past the Continuous linetype:
- `HatchPattern.GetDefaultHatchPatterns()` supplies the built-in patterns by `Name`, `ANSI31` absent
- `doc.HatchPatterns.Add` returns the index a hatch takes and gives each pattern its id
- `doc.Linetypes.LoadDefaultLinetypes()` adds Hidden, Dashed, DashDot, Center, Border, and Dots
- Layers a script or `layer` adds start at `SectionStyleIndex` -1, and clipped solids on them draw hollow with a boundary 3 times the line width
- `doc.SectionStyles.Find("<name>")` or `Add(style)` gives the index a layer's `SectionStyleIndex` takes
- `style.HatchIndex` names a pattern the document's table holds
- `SectionStyle.ReadFromFile(path)` reads a `.3dm`'s styles as `(ok, styles, hatchPatterns)`, `File3dm` holds no section style table
- Cut fills follow the mode's `ClippingSurfaceUsage`, 3 with `ClippingSurfaceColor` for a solid poche, cut edges follow object color under 2
