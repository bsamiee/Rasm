# [DRAFTING]

Drawings, sheets, annotation, and printed pages come from `make2d`, `sheet`, `add`, and `pdf` in `run_python`, and read back through `mutool` and `Read`.

## [01]-[DRAWINGS]

`make2d(doc, "<view>", "<A>", ids, offset=(<x>, <y>))` projects objects through a view onto World XY with the drawing's lower left at `offset`, curves on sublayers `<A>::Visible`, `<A>::Hidden`, and `<A>::SectionCut`:
1. `find(doc, layer_path="<model>", object_type=ObjectType.Brep | ObjectType.Extrusion)` rows give the ids and the model box
2. `find(doc, object_type=ObjectType.ClipPlane)` rows name the clipping planes an existing document holds
3. Model layers take a print width and a section style, layers a script adds holding neither
4. Plan and section cuts add a clipping plane `extent` wide on the drawing's view, a section's at `section_y`, its normal facing the part that stays
5. `make2d` with `offset` past the model box's `max` by `gap`, a section `below` its plan, keeps each drawing clear of the model and the others

```python
# Plan cut 4 ft above the floor and a section on Front, each drawn beside the model
from Rhino.DocObjects import ObjectAttributes, ObjectType
from Rhino.Geometry import Plane, Point3d, Vector3d
from System import Guid
from System.Collections.Generic import List
import document
from records import Properties

doc = __rhino_doc__
cut = doc.SectionStyles.Find("Cut")
for path in ("<model>::Walls", "<model>::Slab"):
    document.layer(doc, path, Properties(print_width=0.25))
    doc.Layers[document.layer_index(doc, path)].SectionStyleIndex = cut
attributes = ObjectAttributes()
attributes.LayerIndex = document.layer_index(doc, "<model>::Clips")
for view, plane in (("Top", Plane(Point3d(0, 0, 4), Vector3d(0, 0, -1))), ("Front", Plane(Point3d(0, section_y, 0), Vector3d.YAxis))):
    doc.Objects.AddClippingPlane(plane, extent, extent, List[Guid]([doc.Views.Find(view, compareCase=True).MainViewport.Id]), attributes)
model = document.find(doc, layer_path="<model>", object_type=ObjectType.Brep | ObjectType.Extrusion)
right = max(row.max[0] for row in model.rows)
print(document.make2d(doc, "Top", "<drawings>::Plan", [row.id for row in model.rows], offset=(right + gap, 0)))
print(document.make2d(doc, "Front", "<drawings>::Section", [row.id for row in model.rows], offset=(right + gap, below)))
```

- Visible and hidden curves print in their source's print color and width, hidden curves in linetype `Hidden`
- Cut outlines and fills print as each source's section style computes under the clipping plane, layer or object
- Styles fill a solid's cut, and any closed cut under `ObjectSectionFillRule.ClosedCurves`, with their pattern over their background color
- Template copies give layer `Default` style `Cut`, a solid fill in the section color with a 0.35 mm outline and no pattern
- Views no clipping plane cuts draw elevations, `Right` the east face

Hatched cuts take a style of their own, the pattern added once from Rhino's defaults:

```python
# Concrete hatch over the Cut fill for the cuts of <A>
from Rhino.DocObjects import HatchPattern, SectionStyle
import document

doc = __rhino_doc__
cut, held = doc.SectionStyles.FindIndex(doc.SectionStyles.Find("Cut")), doc.HatchPatterns.FindName("Concrete")
style = SectionStyle()
style.Name, style.HatchScale = "Concrete", 1.0
style.HatchIndex = held.Index if held is not None else doc.HatchPatterns.Add(next(entry for entry in HatchPattern.GetDefaultHatchPatterns() if entry.Name == "Concrete"))
style.BackgroundFillMode, style.BackgroundFillColor, style.BackgroundFillPrintColor = cut.BackgroundFillMode, cut.BackgroundFillColor, cut.BackgroundFillPrintColor
style.BoundaryPlotWeightMillimeters = cut.BoundaryPlotWeightMillimeters
doc.Layers[document.layer_index(doc, "<A>")].SectionStyleIndex = doc.SectionStyles.Add(style)
```

## [02]-[SHEETS]

`sheet(doc, "<page>", (<page length>, <model length>), (<width>, <height>), zoom=<ids>, frame=(<left>, <bottom>, <right>, <top>))` adds a Top detail locked at that scale, centered on `zoom` with an unprinted border on the current layer, and returns its row with `min` and `max` in page units:
1. `describe(doc).views` rows with `layout` true name each page and its first detail's `scale`
2. `find(doc, object_type=ObjectType.Detail)` rows give each detail's page and frame on an existing sheet
3. Model-space dimensions go on each drawing before its detail, their ids joining the drawing's in `zoom`
4. Frames sit between the title block rule at 3 in and the border, each the `zoom` box times scale plus 0.5 in, 1.5 in under it for a title
5. `sheet` with a size adds the page first, a page name the document holds taking no size
6. `page.PageName = "<sheet number>"` on the template's `Page 1` names it like every other sheet

- Feet documents with inch pages take scales `(0.125, 1)`, `(0.25, 1)`, `(0.375, 1)`, `(0.5, 1)`, `(1.5, 1)`, `(3, 1)`
- Details frame Make2D drawings, every plan, section, and elevation lying full size on World XY
- `sheet` without `frame` centers the detail on the whole page with one text height of margin

## [03]-[ANNOTATION]

Annotation goes on the dimension layer through `add(doc, <annotation>, page="<page>")` in page units and `add` without `page` in model units, and page curves name a text row's `layer`:
- Style `TextHeight` is the paper cap height, page text prints at it and model text seen through a detail prints at it at every detail scale
- `TextHeight` set on one `TextEntity` overrides the style for that text alone
- Make2D drawing dimensions measure model lengths, `AnnotationType.Rotated` at the measured direction's angle (0 along X, `math.pi / 2` along Y)
- Page dimensions over a detail show model lengths with `DetailMeasured` the detail's id and `DistanceScale` the model feet per page inch
- `Leader.Create("<text>", Plane.WorldXY, style, [<Point3d>, ...])` adds a leader in either space

Plans carry overall width and depth, sections and elevations their height, each dimension line 4 ft off the drawing:

```python
# Overall dimensions on the plan drawing and the height on the section drawing
import math
from Rhino.Geometry import AnnotationType, LinearDimension, Plane, Point3d, Vector3d
import document

doc = __rhino_doc__
style = doc.DimStyles.Current


def extent(path: str) -> tuple[float, float, float, float]:
    rows = document.find(doc, layer_path=path).rows
    return min(row.min[0] for row in rows), min(row.min[1] for row in rows), max(row.max[0] for row in rows), max(row.max[1] for row in rows)


def span(start: tuple[float, float], end: tuple[float, float], line: tuple[float, float], angle: float) -> LinearDimension:
    return LinearDimension.Create(AnnotationType.Rotated, style, Plane.WorldXY, Vector3d.XAxis, Point3d(*start, 0), Point3d(*end, 0), Point3d(*line, 0), angle)


left, bottom, right, top = extent("<drawings>::Plan")
print(document.add(doc, [span((left, bottom), (right, bottom), (left, bottom - 4), 0), span((left, bottom), (left, top), (left - 4, bottom), math.pi / 2)]))
left, bottom, _, top = extent("<drawings>::Section")
print(document.add(doc, span((left, bottom), (left, top), (left - 4, bottom), math.pi / 2)))
```

Every sheet takes one convention, each drawing's title and scale under its detail and the project, sheet title, and sheet number in the title block, `border` serving pages `sheet` adds, and `drawings` holding `(title, detail id, scale text)` per detail:

```python
# Drawing titles under the details and the title block of one sheet
from Rhino.Geometry import LineCurve, Plane, Point3d, Rectangle3d, TextEntity, Vector3d
import document
from records import Properties

doc = __rhino_doc__
style = doc.DimStyles.Current
page = "<sheet number>"


def text(value: str, x: float, y: float, height: float) -> TextEntity:
    entity = TextEntity.Create(value, Plane(Point3d(x, y, 0), Vector3d.ZAxis), style, False, 0, 0)
    entity.TextHeight = height
    return entity


texts = [text("<PROJECT>", 1, 1.6, 0.25), text("<SHEET TITLE>", 1, 1.05, 0.125), text(page, 32.5, 1.2, 0.375)]
for name, detail, scale in drawings:
    row = document.read_objects(doc, [detail]).rows[0]
    texts += [text(name, row.min[0], row.min[1] - 0.75, 0.1875), text(f"SCALE: {scale}", row.min[0], row.min[1] - 1.2, 0.09375)]
titles = document.add(doc, texts, page=page)
print(titles)
rule = LineCurve(Point3d(0.5, 3, 0), Point3d(35.5, 3, 0))
border = Rectangle3d(Plane.WorldXY, Point3d(0.5, 0.5, 0), Point3d(35.5, 23.5, 0)).ToNurbsCurve()
print(document.add(doc, [rule, border], titles.rows[0].layer, Properties(print_width=0.35), page=page))
```

## [04]-[PRINTING]

`pdf(doc, "drafting/<name>.pdf", ["<page>", ...])` prints layout pages of a windowed document at paper size into one vector PDF under `.artifacts/rhino`, every page in page order without `pages`, each layer a PDF layer:
1. `mutool draw -w 2000 -h 2000 -o <stem>-%d.png <stem>.pdf` draws each page whole at the largest size `Read` shows unscaled
2. `mutool draw -r 300 -o <stem>-<n>-300.png <stem>.pdf <n>` draws page `<n>`, its place in the record's `detail`, at print resolution with y running down
3. `magick <stem>-<n>-300.png -crop <w>x<h>+<x>+<y> +repage <stem>-<part>.png` cuts a frame with its title, sizes in inches times 300
4. `Read` of each PNG shows the page, then `rm <stem>-<n>-300.png`

Lines print in their print color and width:
- `Properties(print_width=<mm>, print_color="#<RRGGBB>")` sets a layer or object, 0 printing the thinnest line and -1 nothing
- Linetypes with a millimeter width print it over the print width, the template's `Hidden`, `Phantom`, and `Construction` 0.18 mm, `Center` 0.25 mm
- Template layers print 0.25 mm in document ink
