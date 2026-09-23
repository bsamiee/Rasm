# [DRAFTING]

Drawings and sheets come from `document.py` entry points and RhinoCommon inside `run_python`, with no command prompt.

## [01]-[MAKE2D]

`make2d(doc, view, layer_path, ids, offset)` projects solids, meshes, curves, and block instances through a view with `HiddenLineDrawing` and returns the records of the flat curves it added:
- Visible and hidden curves land on `<layer_path>::Visible` and `<layer_path>::Hidden`
- Curves land on World XY with their lower corner at `offset`
- Block instances project through their definition's geometry in place, the instance stays as it is
- `ids` naming text or points return a `RhinoObject` fault for each, runs over every visible object skip them

## [02]-[LAYOUTS]

`sheet(doc, name, size, scale, projection)` adds a page with one locked detail over the whole page, `scale` as page units per model units:
- `size` is in `doc.PageUnitSystem`, inches in the template
- 1 in = 20 ft is `sheet(doc, "<name>", (17, 11), (1, 20), DefinedViewportProjection.Top)`, the page's `ViewRecord.scale` reads 240 back
- Pages need not be the active view, `capture(doc, "<name>", view="<page name>", zoom=None)` captures the sheet at the page view's pixel size
- Page names in use return a `RhinoPageView` fault listing the pages

`pdf(doc, path, pages)` prints the named pages, or every page in page order, into one vector PDF at paper size in print colors:
- Unknown page names return a `RhinoPageView` fault listing the pages
- `mutool draw -r 60 -o <page>.png <path>` draws the PDF's pages for `Read`

## [03]-[DIMENSIONS]

```python
# Aligned dimension on an annotation layer
from Rhino.DocObjects import ObjectAttributes
from Rhino.Geometry import AnnotationType, LinearDimension, Plane, Point3d, Vector3d
import document
doc = __rhino_doc__
dimension = LinearDimension.Create(AnnotationType.Aligned, doc.DimStyles.Current, Plane.WorldXY, Vector3d.XAxis, Point3d(0, 0, 0), Point3d(10, 0, 0), Point3d(5, -3, 0), 0.0)
attributes = ObjectAttributes()
attributes.LayerIndex = document.layer_index(doc, "Annotation::Dimensions")
print(doc.Objects.AddLinearDimension(dimension, attributes))
```

- Template's current style is architectural foot-inch with `DimensionScale` 96, model-space text draws at 96 times its 1/8 in height
