# [DRAFTING]

Drawings, sheets, annotation, sections, and printed pages come from `make2d`, `sheet`, `add`, and `pdf` in `run_python`, and read back through `mutool` and `Read`.

## [01]-[SHEETS]

Each sheet runs in order on a new or an existing document:
1. `describe(doc)` gives `units`, `page_units`, `style`, and `views`, each layout row listing its details with their scales
2. `find(doc, layer_path="<A>")` rows give the model box, `min` and `max` in model units
3. Scale page length per model length fits the box in the detail frame (a 144 x 96 ft plan fills an ARCH D page at 1/4 in = 1 ft)
4. `sheet(doc, "<name>", (0.25, 1), (36, 24), frame=(<left>, <bottom>, <right>, <top>), zoom=<ids>)` adds the page and a locked detail
5. `sheet(doc, "<name>", <scale>, frame=..., zoom=..., projection=DefinedViewportProjection.Front)` adds a detail to a page the document holds
6. `add(doc, <geometry>, "<A>", Properties(print_width=<mm>), page="<name>")` places the border, title text, and page dimensions in page units
7. `pdf(doc, "drafting/<file>.pdf", ["<name>"])` prints the pages, `mutool` draws them for `Read`
