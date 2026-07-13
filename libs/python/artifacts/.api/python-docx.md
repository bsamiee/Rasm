# [PY_ARTIFACTS_API_PYTHON_DOCX]

`python-docx` supplies the Word `.docx` document surface for the artifacts office rail: a polymorphic `Document` factory that opens an existing file/stream or creates from the default template, returning a `document.Document` whose `add_*` content family (paragraph, heading, table, picture, section, page-break, comment), block-level `Run`/`Paragraph`/`Table`/`Section` owners, the full `Font`/`ParagraphFormat` direct-formatting surface, the named-style catalog with latent styles, the `Comments` review collection, and the 15-field `CoreProperties` metadata record drive OOXML word-processing construction and editing. It is the `document/emit#DOCUMENT` Office lowering arm for `DocumentMode.DOCX` — every `add_*` call is a fold FROM a `document/model#NODE` `DocumentNode` subtree, never an opaque payload — and `document/lens#LENS` walks `iter_inner_content`/`runs`/`hyperlinks` back TO the tree, so authoring and extraction are inverses over the one node algebra. The owner composes `Document`, the `add_*` family, the `Run.add_break`/`add_picture`/`add_tab`/`add_text` inline surface, the `Font` typography axis, the `ParagraphFormat` block-geometry axis, the `Section` header/footer/page-geometry surface, the `Styles.add_style` catalog, and the `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips` value objects; it removes raw-EMU integer math because the `Length` value objects own unit conversion, it routes alignment/break/underline through the `WD_*` enums rather than magic strings, and it never re-implements the Office Open XML part graph, the lxml-backed element serialization, or the relationship/content-type bookkeeping python-docx already owns. It runs in-process (cp315 wheel present), not behind the `anyio.to_process` worker seam the sub-3.15 office siblings cross.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-docx`
- package: `python-docx`
- import: `docx`
- owner: `artifacts`
- rail: office
- installed: `1.2.0`
- license: MIT (Steve Canny) — permissive, no copyleft gate; aligns with the MIT/BSD sibling office owners (`python-pptx`, `openpyxl`, `xlsxwriter`)
- build floor: pure-Python wheel; cp315-resident, in-process (no `; python_version` gate, no worker offload — distinct from `python-pptx`/`openpyxl` which the office rail can push to `anyio.to_process`)
- runtime deps internalized: `lxml` (element serialization — the artifacts `lxml.md` owner; never re-author the part XML), `Pillow` (image part sizing for `add_picture`), `typing-extensions`
- entry points: none (library only)
- capability: `.docx` construction and editing — paragraphs with runs and named styles, headings, tables (rows/columns/cells, nested tables, horizontal+vertical cell merge via `grid_span`, vertical alignment, per-row height rules, table direction), inline pictures (document- and run-level), page/line/column breaks and tabs, sections (header/footer triad gated by `different_first_page_header_footer` + the document odd/even flag, the four margins, gutter, header/footer distance, orientation, page geometry), the named paragraph/character/table/list style catalog with latent styles and base-style inheritance, the comment collection (`add_comment`/`comments`/`mark_comment_range`), document settings, the 15-field core-properties record, the full `Font` direct-formatting surface (sub/superscript, caps variants, strike/double-strike, highlight, complex-script/RTL, color), the full `ParagraphFormat` surface (indents, spacing, tab stops, keep/widow/page-break-before controls, line-spacing rules), and ordered block iteration via `iter_inner_content` (with rendered-page-break detection in `Run.iter_inner_content`/`Paragraph.rendered_page_breaks`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and block content types
- rail: office

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]  | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `document.Document`           | document object | root object: `add_*` family, block accessors, save          |
|  [02]   | `text.paragraph.Paragraph`    | paragraph       | runs, hyperlinks, alignment, `paragraph_format`, style      |
|  [03]   | `text.run.Run`                | run             | styled text plus inline break/picture/tab/text/font         |
|  [04]   | `text.parfmt.ParagraphFormat` | block geometry  | indents, spacing, tab stops, keep/widow/page-break controls |
|  [05]   | `text.font.Font`              | run typography  | size/bold/italic/underline/color/caps/sub-super/strike      |
|  [06]   | `text.hyperlink.Hyperlink`    | hyperlink       | run-bearing hyperlink inside a paragraph (read axis)        |
|  [07]   | `table.Table`                 | table           | rows/columns, cell access, autofit, alignment, direction    |
|  [08]   | `table._Cell`                 | cell            | paragraphs, nested tables, merge, grid_span, v-align        |
|  [09]   | `table._Row`                  | row             | cells, height/height_rule, grid_cols_before/after           |
|  [10]   | `section.Section`             | section         | page geometry, margins, gutter, orientation, header/footer  |
|  [11]   | `shape.InlineShape`           | inline shape    | the picture/drawing returned by `add_picture` (type/w/h)    |

[PUBLIC_TYPE_SCOPE]: style, review, metadata, and unit types
- rail: office

The `WD_*` enums sit under `enum.<module>`; beyond the representative rows below, `enum.text` also carries `WD_BREAK`/`WD_LINE_SPACING`/`WD_UNDERLINE`/`WD_TAB_ALIGNMENT`/`WD_TAB_LEADER`/`WD_COLOR_INDEX`, `enum.section` also `WD_ORIENTATION`/`WD_HEADER_FOOTER`, `enum.style` also `WD_BUILTIN_STYLE`, `enum.table` also `WD_TABLE_ALIGNMENT`/`WD_ROW_HEIGHT_RULE`/`WD_TABLE_DIRECTION`.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `styles.styles.Styles`                  | style catalog     | add/lookup/default/by-id, `latent_styles`                         |
|  [02]   | `styles.style.BaseStyle`                | style base        | name/style_id/type/builtin/priority/hidden/locked/quick           |
|  [03]   | `styles.style._ParagraphStyle`          | paragraph style   | + font/`paragraph_format`/base_style/next_paragraph_style         |
|  [04]   | `styles.style._CharacterStyle`          | character style   | named run style with font                                         |
|  [05]   | `styles.latent.LatentStyles`            | latent catalog    | built-in latent style defaults + `add_latent_style`               |
|  [06]   | `comments.Comments`                     | comment catalog   | `add_comment`/`get` over the comments part                        |
|  [07]   | `comments.Comment`                      | comment           | author/initials/timestamp/comment_id + block content              |
|  [08]   | `settings.Settings`                     | document settings | `odd_and_even_pages_header_footer` toggle                         |
|  [09]   | `opc.coreprops.CoreProperties`          | metadata          | the 15 OOXML core-property fields                                 |
|  [10]   | `shared.Length`                         | unit base         | EMU value object (`Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips`)          |
|  [11]   | `shared.RGBColor`                       | color             | run-font color value object (`from_string` parse)                 |
|  [12]   | `enum.text.WD_ALIGN_PARAGRAPH`          | alignment enum    | paragraph alignment                                               |
|  [13]   | `enum.section.WD_SECTION_START`         | section enum      | section start kind                                                |
|  [14]   | `enum.style.WD_STYLE_TYPE`              | style-kind enum   | `add_style` discriminant (`PARAGRAPH`/`CHARACTER`/`TABLE`/`LIST`) |
|  [15]   | `enum.table.WD_CELL_VERTICAL_ALIGNMENT` | cell-align enum   | `_Cell.vertical_alignment`                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document factory, content, save
- rail: office

`Document(docx=None)` is the single open-or-create factory; the `add_*` rows mint block content and return the created owner. Picture/section sizing takes `Length` value objects (ints are tolerated but the owner forbids them). The `SURFACE` names drop the `Document.` prefix; `add_picture` and `add_comment` carry the full typed signatures `add_picture(image_path_or_stream: str \| IO[bytes], width: int \| Length \| None = None, height: int \| Length \| None = None) -> InlineShape` and `add_comment(runs: Run \| Sequence[Run], text: str = '', author: str = '', initials: str \| None = '') -> Comment`.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                       |
| :-----: | :------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Document`           | `Document(docx: str \| IO[bytes] \| None = None) -> document.Document`             |
|  [02]   | `add_paragraph`      | `add_paragraph(text='', style: str \| ParagraphStyle \| None = None) -> Paragraph` |
|  [03]   | `add_heading`        | `add_heading(text='', level=1) -> Paragraph` (level 0 = title)                     |
|  [04]   | `add_table`          | `add_table(rows, cols, style: str \| _TableStyle \| None = None) -> Table`         |
|  [05]   | `add_picture`        | `add_picture(image_path_or_stream, width=None, height=None) -> InlineShape`        |
|  [06]   | `add_section`        | `add_section(start_type: WD_SECTION = WD_SECTION.NEW_PAGE) -> Section`             |
|  [07]   | `add_page_break`     | `add_page_break() -> Paragraph`                                                    |
|  [08]   | `add_comment`        | `add_comment(runs, text='', author='', initials='') -> Comment`                    |
|  [09]   | `save`               | `save(path_or_stream: str \| IO[bytes])`                                           |
|  [10]   | `iter_inner_content` | `iter_inner_content() -> Iterator[Paragraph \| Table]`                             |

[ENTRYPOINT_SCOPE]: inline run, table, and section authoring
- rail: office

The `Run` inline surface is where text/break/picture/tab live; `Table`/`_Cell`/`_Row` own grid growth, merging, and geometry; `Section` owns page geometry and the header/footer triad.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                                                |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `Paragraph.add_run`                   | `add_run(text: str \| None = None, style: str \| CharacterStyle \| None = None) -> Run`     |
|  [02]   | `Paragraph.insert_paragraph_before`   | `insert_paragraph_before(text=None, style=None) -> Paragraph`                               |
|  [03]   | `Run.add_break`                       | `add_break(break_type: WD_BREAK = WD_BREAK.LINE)`                                           |
|  [04]   | `Run.add_picture`                     | `add_picture(image_path_or_stream, width=None, height=None) -> InlineShape`                 |
|  [05]   | `Run.add_tab` / `add_text`            | `add_tab()` / `add_text(text: str) -> _Text`                                                |
|  [06]   | `Run.mark_comment_range`              | `mark_comment_range(last_run: Run, comment_id: int) -> None`                                |
|  [07]   | `Table.add_row` / `add_column`        | `add_row() -> _Row` / `add_column(width: Length) -> _Column`                                |
|  [08]   | `Table.cell`                          | `cell(row_idx, col_idx) -> _Cell`                                                           |
|  [09]   | `_Cell.merge` / `add_table`           | `merge(other_cell: _Cell) -> _Cell` / `add_table(rows, cols) -> Table`                      |
|  [10]   | `Section.header` / `footer`           | `header -> _Header` (also `first_page_header`/`even_page_header` + footer mirrors)          |
|  [11]   | `_Header.add_paragraph` / `add_table` | `add_paragraph(text='', style=None) -> Paragraph` / `add_table(rows, cols, width) -> Table` |
|  [12]   | `Styles.add_style`                    | `add_style(name: str, style_type: WD_STYLE_TYPE, builtin: bool = False) -> BaseStyle`       |
|  [13]   | `LatentStyles.add_latent_style`       | `add_latent_style(name: str) -> _LatentStyle`                                               |

[ENTRYPOINT_SCOPE]: typography and block-geometry direct formatting
- rail: office

`Run.font` (a `Font`) carries character-level direct formatting the named-style catalog does not name; `Paragraph.paragraph_format` (a `ParagraphFormat`) carries block-level geometry. Both are tri-state (`True`/`False`/`None=inherit`). `ParagraphFormat.tab_stops -> TabStops` grows a tab via `add_tab_stop(position: Length, alignment: WD_TAB_ALIGNMENT, leader: WD_TAB_LEADER)`.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                                          |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Font.size` / `name` / `color` | `size -> Length` / `name -> str` / `color -> ColorFormat (rgb / theme_color / type)`                  |
|  [02]   | `Font` weight/slope            | `bold` / `italic` / `underline (bool \| WD_UNDERLINE)` / `strike` / `double_strike`                   |
|  [03]   | `Font` caps + position         | `all_caps` / `small_caps` / `subscript` / `superscript`                                               |
|  [04]   | `Font` decorative              | `highlight_color (WD_COLOR_INDEX)` / `shadow` / `outline` / `emboss` / `imprint` / `hidden`           |
|  [05]   | `Font` complex-script          | `rtl` / `complex_script` / `cs_bold` / `cs_italic` / `math` / `snap_to_grid` / `no_proof`             |
|  [06]   | `ParagraphFormat` indents      | `left_indent` / `right_indent` / `first_line_indent` (all `Length`)                                   |
|  [07]   | `ParagraphFormat` spacing      | `space_before` / `space_after` (`Length`) / `line_spacing` / `line_spacing_rule (WD_LINE_SPACING)`    |
|  [08]   | `ParagraphFormat` flow         | `alignment` / `keep_together` / `keep_with_next` / `page_break_before` / `widow_control` (align enum) |
|  [09]   | `ParagraphFormat.tab_stops`    | `tab_stops -> TabStops`                                                                               |

[ENTRYPOINT_SCOPE]: accessors, grid geometry, review, and metadata
- rail: office

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                            |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `Document` collections     | `paragraphs -> list[Paragraph]` / `tables -> list[Table]` / `sections -> Sections`                      |
|  [02]   | `Document` catalogs        | `styles -> Styles` / `inline_shapes -> InlineShapes` / `comments -> Comments` / `settings -> Settings`  |
|  [03]   | `Document.core_properties` | `core_properties -> CoreProperties` (15 OOXML fields; see metadata axis)                                |
|  [04]   | `Section` page size        | `orientation (WD_ORIENTATION)` / `page_width` / `page_height`                                           |
|  [05]   | `Section` margins          | `left_margin` / `right_margin` / `top_margin` / `bottom_margin` (all `Length`)                          |
|  [06]   | `Section` offsets          | `gutter` / `header_distance` / `footer_distance` (all `Length`)                                         |
|  [07]   | `Section` toggles          | `different_first_page_header_footer -> bool` (+ `Settings.odd_and_even_pages_header_footer`)            |
|  [08]   | `Section` iter + start     | `iter_inner_content() -> Iterator[Paragraph \| Table]` / `start_type -> WD_SECTION`                     |
|  [09]   | `Table` grid controls      | `autofit -> bool` / `alignment (WD_TABLE_ALIGNMENT)` / `table_direction (WD_TABLE_DIRECTION)` / `style` |
|  [10]   | `_Cell` geometry           | `grid_span -> int` / `vertical_alignment (WD_CELL_VERTICAL_ALIGNMENT)` / `width (Length)`               |
|  [11]   | `_Row` geometry            | `height (Length)` / `height_rule (WD_ROW_HEIGHT_RULE)` / `grid_cols_before` / `grid_cols_after`         |
|  [12]   | `Run` page-break           | `contains_page_break -> bool` / `iter_inner_content() -> Iterator[str \| Drawing \| RenderedPageBreak]` |
|  [13]   | `Paragraph` read axis      | `rendered_page_breaks -> list[RenderedPageBreak]` / `hyperlinks -> list[Hyperlink]`                     |
|  [14]   | `BaseStyle` identity       | `name` / `style_id` / `type` / `builtin` / `delete()`                                                   |
|  [15]   | `BaseStyle` flags          | `priority` / `hidden` / `locked` / `quick_style` / `unhide_when_used`                                   |
|  [16]   | `_ParagraphStyle` adds     | `font -> Font` / `paragraph_format -> ParagraphFormat` / `base_style` / `next_paragraph_style`          |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_DOCX]:
- import: `import docx` at boundary scope only; module-level import is banned by the manifest import policy. The distribution is `python-docx`; the import name is `docx`. Runs in-process — there is no `anyio.to_process` worker seam here (the cp315 wheel is native), unlike the `python-pptx`/`openpyxl` arms the office rail may push to a sub-3.15 worker.
- document axis: `docx.Document(docx=None)` is the single polymorphic factory for both open and create (path/stream opens, `None` creates from the default template); it returns the `document.Document` object, never a parallel reader/writer split.
- emission axis: every `add_*` call is a LOWERING fold from a `document/model#NODE` `DocumentNode` subtree — the `document/emit#DOCUMENT` `DocumentPlan` `DocumentMode.DOCX` arm walks the tree and emits paragraphs/headings/tables/pictures/sections, threading one `EmitFact` evidence carrier; the docx owner never accepts an opaque `dict` payload. `document/lens#LENS` is the strict inverse — it walks `Document.iter_inner_content` / `Paragraph.runs` / `Paragraph.hyperlinks` / `_Cell.iter_inner_content` back to `DocumentNode`, so authoring and extraction are inverses over the one node algebra.
- content axis: `add_paragraph`/`add_heading`/`add_table`/`add_picture`/`add_section`/`add_page_break`/`add_comment` is one content-add surface on the document object; each content kind is a row, never a parallel document type. The inline run surface (`Run.add_break`/`add_picture`/`add_tab`/`add_text`) is the within-paragraph mirror — a run-level picture flows with text where the document-level `add_picture` is block-anchored.
- table axis: `Table.add_row`/`add_column`/`cell`/`row_cells`/`column_cells` own grid growth and addressing; `_Cell.merge(other_cell)` spans the rectangular region the two cells bound (returning the merged `_Cell`), `_Cell.grid_span` reads the resulting horizontal span, `_Cell.add_table` nests, and `_Row.height`/`height_rule`/`grid_cols_before`/`grid_cols_after` own row geometry — never a parallel merged-cell or nested-table type. A table cell is authored through the same `_Cell.add_paragraph` surface as the body. `add_column` requires an explicit `Length` width; `add_row` does not.
- typography axis: `Run.font` is one `Font` value carrying the full direct-formatting surface (`size`/`name`/`color`, the `bold`/`italic`/`underline`/`strike`/`double_strike` weight-slope set, the `all_caps`/`small_caps`/`subscript`/`superscript` caps-position set, `highlight_color`, the `shadow`/`outline`/`emboss`/`imprint` effect set, and the `rtl`/`complex_script`/`cs_bold`/`cs_italic`/`math` script set) — all tri-state (`True`/`False`/`None`=inherit-from-style). `Font.color` is a `ColorFormat` (`rgb` `RGBColor` or `theme_color`), parsed via `RGBColor.from_string`. Direct font formatting is for ad-hoc emphasis the named-style catalog does not name; recurring formatting belongs in a style.
- block-geometry axis: `Paragraph.paragraph_format` is one `ParagraphFormat` owning indentation (`left_indent`/`right_indent`/`first_line_indent`), spacing (`space_before`/`space_after`/`line_spacing`/`line_spacing_rule`), pagination (`keep_together`/`keep_with_next`/`page_break_before`/`widow_control`), `alignment`, and the `tab_stops` grid (`add_tab_stop(position, alignment, leader)`) — never per-paragraph XML poking.
- styling axis: named styles come from `Document.styles`; `Styles.add_style(name, WD_STYLE_TYPE, builtin=)` mints a reusable row, `default(style_type)` reads the catalog default, `get_by_id(style_id, style_type)` / `get_style_id(style_or_name, style_type)` resolve identity, and `latent_styles` exposes the `LatentStyles` defaults (`default_priority`/`default_to_hidden`/...) plus `add_latent_style`. A `BaseStyle` carries `priority`/`hidden`/`locked`/`quick_style`/`unhide_when_used`/`delete()`, and a `_ParagraphStyle` adds `base_style`/`next_paragraph_style` for inheritance chains — styles are named rows attached to paragraphs/runs/tables, never per-run duplication.
- section axis: `Section` owns page geometry (`page_width`/`page_height`/`orientation`/the four margins/`gutter`/`header_distance`/`footer_distance`) and the header/footer triad (`header`/`first_page_header`/`even_page_header` and footer mirrors, each a `_Header`/`_Footer` with its own `add_paragraph`/`add_table` story); the first-page header is inert until `different_first_page_header_footer = True` and the even-page header is inert until `Settings.odd_and_even_pages_header_footer = True` — set the toggle, never assume the triad applies. `Section.iter_inner_content` walks that section's blocks; a multi-layout document is a sequence of section rows, never per-page objects.
- review axis: `Document.add_comment(runs, text, author, initials)` is the high-level anchor-over-runs surface (it computes the range and writes the comment); `Document.comments` is the `Comments` collection (`add_comment(text, author, initials)` + `get(comment_id)`), each `Comment` carrying `author`/`initials`/`timestamp`/`comment_id` plus `add_paragraph`/`add_table` body content. `Run.mark_comment_range(last_run, comment_id)` is the low-level range marker when authoring the comment part directly — never a parallel annotation type.
- metadata axis: `Document.core_properties` is the single `CoreProperties` record exposing all 15 OOXML core fields (`author`/`title`/`subject`/`keywords`/`category`/`comments`/`content_status`/`created`/`modified`/`last_modified_by`/`last_printed`/`identifier`/`language`/`revision`/`version`) — the provenance/authorship seal, never a sidecar dict.
- unit axis: all measurements use `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips` `Length` value objects, never raw EMU ints; alignment/break/section-start/underline/tab/highlight/cell-align/row-height are `WD_ALIGN_PARAGRAPH`/`WD_BREAK`/`WD_SECTION_START`/`WD_UNDERLINE`/`WD_TAB_ALIGNMENT`/`WD_TAB_LEADER`/`WD_COLOR_INDEX`/`WD_CELL_VERTICAL_ALIGNMENT`/`WD_ROW_HEIGHT_RULE` enum rows, never magic strings.
- rail stacking: the office-receipt model is one `msgspec.Struct`/`pydantic` model (`libs/python/.api/msgspec.md`, `libs/python/.api/pydantic.md`) capturing paragraph count, table count, image count, section count, comment count, style count, and output byte length.
- receipt seam: the model contributes the `document/emit#DOCUMENT` `EmitFact` case; the lowering op is `@beartype`-guarded, emits one `structlog` event inside an `opentelemetry` span, and retries transient save I/O under `stamina` (`libs/python/runtime/.api/stamina.md`).
- raster seam: an RGBA overlay/QR/chart raster arrives as a `numpy` array (`libs/python/.api/numpy.md`) -> PNG and anchors through `add_picture`; fallible opens return through the `expression` `Result` rail.
- boundary: python-docx owns `.docx` as the `document/emit#DOCUMENT` `DocumentMode.DOCX` arm and the `document/lens#LENS` `.docx` extraction inverse. `docxtpl` (`libs/python/artifacts/.api/docxtpl.md`) wraps a `Document` as a jinja2 template — the `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.DOCX_TEMPLATE` fan derives the docxtpl context from the same `DocumentNode` tree and lowers through `document/emit#DOCUMENT`, so prefer `docxtpl.DocxTemplate.render` for styled fill-from-context and reserve raw `python-docx` for programmatic block construction. `msoffcrypto-tool` (`libs/python/artifacts/.api/msoffcrypto-tool.md`) decrypts an encrypted container to a plaintext stream `Document(stream)` reads, and re-seals the saved bytes; `puremagic`/`python-magic` gate which reader runs at admission through `exchange/detect#DETECT` and `document/emit#DOCUMENT` `DocumentPlan.bound`. Excel routes to `openpyxl`, PowerPoint to `python-pptx`, ODF to `odfpy`, PDF render to `pymupdf`/`weasyprint`. Live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-docx`
- Owns: `.docx` construction and editing — paragraphs/runs/headings, tables with horizontal+vertical merge/grid-span and nesting and per-row geometry, document- and run-level inline pictures, breaks/tabs, sections with the header/footer triad and full page geometry (margins/gutter/header-footer distance), the named-style catalog with latent styles and base-style inheritance, the full `Font` direct-formatting surface, the full `ParagraphFormat` block-geometry surface, comments with range marking, the 15-field core properties, ordered block iteration. It is the `document/emit#DOCUMENT` `DocumentMode.DOCX` lowering arm and the `document/lens#LENS` `.docx` extraction inverse.
- Accept: word-processing authoring lowering from the `document/model#NODE` `DocumentNode` tree feeding the office and export-bundle owners, downstream of the `docxtpl` template path and the `msoffcrypto-tool` confidentiality edge, gated by the `exchange/detect#DETECT` sniffer
- Reject: wrapper-renames of `add_paragraph`/`save`; a per-run style rebuild where `Styles.add_style` and named styles exist; per-run direct font duplication where a `_CharacterStyle` carries the recurring formatting; raw-EMU integers where `Length` value objects exist; magic alignment/break/underline strings where the `WD_*` enums exist; per-page section objects where a section sequence suffices; ad-hoc paragraph-stitching where a `docxtpl` template carries the layout; opaque-payload emission where the `DocumentNode` lowering fold belongs; raised exceptions crossing the boundary where the `expression` `Result` rail belongs; identity minting the runtime owns
