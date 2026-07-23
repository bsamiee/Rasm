# [PY_ARTIFACTS_API_PYTHON_DOCX]

`python-docx` owns read+write `.docx` word-processing for the artifacts office rail: one polymorphic `Document` factory, the `add_*` content family, the block/style/review/metadata owners, and the `Font`/`ParagraphFormat` direct-formatting surface over full OOXML fidelity. It never re-implements the Office Open XML part graph or the lxml serialization it already owns. It is the `document/emit#DOCUMENT` `DocumentMode.DOCX` lowering arm and the `document/lens#LENS` `.docx` extraction inverse over one `DocumentNode` algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-docx`
- package: `python-docx` (`MIT`, Steve Canny)
- module: `docx`
- abi: pure-Python wheel, cp315-resident, in-process (no worker offload)
- rail: office — `.docx` word-processing authoring and editing feeding the document owners

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and block content types

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]  | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `document.Document`           | document object | root object: `add_*` family, block accessors, save          |
|  [02]   | `text.paragraph.Paragraph`    | paragraph       | runs, hyperlinks, alignment, `paragraph_format`, style      |
|  [03]   | `text.run.Run`                | run             | styled text with inline break/picture/tab/text/font         |
|  [04]   | `text.parfmt.ParagraphFormat` | block geometry  | indents, spacing, tab stops, keep/widow/page-break controls |
|  [05]   | `text.font.Font`              | run typography  | size/bold/italic/underline/color/caps/sub-super/strike      |
|  [06]   | `text.hyperlink.Hyperlink`    | hyperlink       | run-bearing hyperlink inside a paragraph (read axis)        |
|  [07]   | `table.Table`                 | table           | rows/columns, cell access, autofit, alignment, direction    |
|  [08]   | `table._Cell`                 | cell            | paragraphs, nested tables, merge, grid_span, v-align        |
|  [09]   | `table._Row`                  | row             | cells, height/height_rule, grid_cols_before/after           |
|  [10]   | `section.Section`             | section         | page geometry, margins, gutter, orientation, header/footer  |
|  [11]   | `shape.InlineShape`           | inline shape    | the picture/drawing returned by `add_picture` (type/w/h)    |

[PUBLIC_TYPE_SCOPE]: style, review, metadata, and unit types

`enum.<module>` carries the `WD_*` families beyond the representative rows below:
- `enum.text`: `WD_BREAK` `WD_LINE_SPACING` `WD_UNDERLINE` `WD_TAB_ALIGNMENT` `WD_TAB_LEADER` `WD_COLOR_INDEX`
- `enum.section`: `WD_ORIENTATION` `WD_HEADER_FOOTER`
- `enum.style`: `WD_BUILTIN_STYLE`
- `enum.table`: `WD_TABLE_ALIGNMENT` `WD_ROW_HEIGHT_RULE` `WD_TABLE_DIRECTION`

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

`Document(docx=None)` is the single open-or-create factory; the `add_*` rows mint block content and return the created owner. Picture and section sizing take `Length` value objects.

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

`Run` owns the within-paragraph text/break/picture/tab surface; `Table`/`_Cell`/`_Row` own grid growth, merging, and geometry; `Section` owns page geometry and the header/footer triad.

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

`Run.font` (a `Font`) carries character-level formatting the named-style catalog does not name; `Paragraph.paragraph_format` (a `ParagraphFormat`) carries block-level geometry. Both are tri-state (`True`/`False`/`None`=inherit). `ParagraphFormat.tab_stops -> TabStops` grows a tab via `add_tab_stop(position: Length, alignment: WD_TAB_ALIGNMENT, leader: WD_TAB_LEADER)`.

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

[TOPOLOGY]:
- document: `docx.Document(docx=None)` is the single polymorphic open-or-create factory — path/stream opens, `None` creates from the default template — returning `document.Document`.
- fold: every `add_*` is a lowering fold from a `document/model#NODE` `DocumentNode` subtree, and `document/lens#LENS` is the strict inverse over `Document.iter_inner_content`/`Paragraph.runs`/`Paragraph.hyperlinks`/`_Cell.iter_inner_content`, so authoring and extraction are inverses over one node algebra; lxml owns the OOXML part-graph serialization beneath every call.
- content: `add_paragraph`/`add_heading`/`add_table`/`add_picture`/`add_section`/`add_page_break`/`add_comment` is one content-add surface on the document; the `Run.add_break`/`add_picture`/`add_tab`/`add_text` inline surface is the within-paragraph mirror — a run picture flows with text where the document-level `add_picture` is block-anchored.
- table: `Table.add_row`/`add_column`/`cell`/`row_cells`/`column_cells` own grid growth and addressing; `_Cell.merge(other_cell)` spans a rectangular region and returns the merged `_Cell`, `_Cell.grid_span` reads the horizontal span, `_Cell.add_table` nests, and `_Row.height`/`height_rule`/`grid_cols_before`/`grid_cols_after` own row geometry. `add_column` requires an explicit `Length` width; `add_row` does not.
- typography: `Run.font` is one `Font` carrying the full direct-formatting surface across the weight-slope, caps-position, highlight, effect, and complex-script axes, all tri-state (`True`/`False`/`None`=inherit). `Font.color` is a `ColorFormat` (`rgb` `RGBColor` or `theme_color`) parsed via `RGBColor.from_string`; direct font formatting is ad-hoc emphasis, recurring formatting belongs in a style.
- block-geometry: `Paragraph.paragraph_format` is one `ParagraphFormat` owning indentation, spacing, pagination (`keep_together`/`keep_with_next`/`page_break_before`/`widow_control`), `alignment`, and the `tab_stops` grid (`add_tab_stop(position, alignment, leader)`).
- styling: named styles come from `Document.styles`; `Styles.add_style(name, WD_STYLE_TYPE, builtin=)` mints a reusable row, `default(style_type)` reads the catalog default, `get_by_id`/`get_style_id` resolve identity, and `latent_styles` exposes the `LatentStyles` defaults with `add_latent_style`; a `_ParagraphStyle` adds `base_style`/`next_paragraph_style` for inheritance chains.
- section: `Section` owns page geometry (margins/gutter/header-footer distance/orientation/size) and the header/footer triad (`header`/`first_page_header`/`even_page_header` + footer mirrors, each a `_Header`/`_Footer` with `add_paragraph`/`add_table`); the first-page header is inert until `different_first_page_header_footer = True`, the even-page header until `Settings.odd_and_even_pages_header_footer = True`. `Section.iter_inner_content` walks the section's blocks.
- review: `Document.add_comment(runs, text, author, initials)` is the high-level anchor-over-runs surface that computes the range; `Document.comments` is the `Comments` collection (`add_comment(text, author, initials)` + `get(comment_id)`), each `Comment` carrying `author`/`initials`/`timestamp`/`comment_id` with `add_paragraph`/`add_table` body content; `Run.mark_comment_range(last_run, comment_id)` is the low-level range marker.
- metadata: `Document.core_properties` is the single `CoreProperties` record exposing all 15 OOXML core fields, the provenance/authorship seal.
- unit: measurements use `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips` `Length` value objects, and alignment/break/section-start/underline/tab/highlight/cell-align/row-height ride the `WD_*` enum rows.

[STACKING]:
- `document/emit#DOCUMENT`(`emit.md`): `DocumentMode.DOCX` resolves to the docx arm, which walks the `document/model#NODE` `DocumentNode` tree emitting paragraphs/headings/tables/pictures/sections, threads one `EmitFact` evidence carrier, and mints the `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes)` case; the emit owner holds the engine, and `document/lens#LENS` is the `.docx` extraction inverse.
- `docxtpl`(`.api/docxtpl.md`): the `DocumentMode.DOCX_TEMPLATE` fan derives a docxtpl context from the same `DocumentNode` tree and mirrors the `RichText.add` appearance axis onto `Run.font`; `docxtpl.DocxTemplate.render` carries styled fill-from-context, raw `python-docx` carries programmatic block construction.
- `msoffcrypto-tool`(`.api/msoffcrypto-tool.md`): decrypts an encrypted container to a plaintext stream `Document(stream)` reads and re-seals saved bytes; `puremagic`(`.api/puremagic.md`)/`python-magic`(`.api/python-magic.md`) gate which reader runs at `exchange/detect#DETECT`.
- `lxml`(`.api/lxml.md`): owns the element serialization and relationship/content-type part graph every `add_*` writes through.
- `pillow`(`.api/pillow.md`)/`numpy`(`.api/numpy.md`): an RGBA overlay/QR/chart raster arrives as a numpy array -> PNG and anchors through `add_picture`, Pillow sizing the image part.
- `msgspec`(`.api/msgspec.md`)/`pydantic`(`.api/pydantic.md`): the office-receipt model is one `Struct` admitted through a `TypeAdapter`, capturing paragraph/table/image/section/comment/style counts and output byte length onto the `EmitFact`.
- rail: the lowering op is `@beartype`-guarded, emits one `structlog` event inside an `opentelemetry` span, retries transient save I/O under `stamina`(`.api/stamina.md`), and a provider raise converts to the `expression`(`.api/expression.md`) `Result` rail; Excel routes to `openpyxl`, PowerPoint to `python-pptx`, ODF to `odfpy`, PDF render to `pymupdf`/`weasyprint`.

[LOCAL_ADMISSION]:
- Import at boundary scope (`import docx`); the distribution is `python-docx`, the import name `docx`.
- Word-processing authoring lowering from the `document/model#NODE` `DocumentNode` tree feeding the office and export-bundle owners, downstream of the `docxtpl` template path and the `msoffcrypto-tool` confidentiality edge, gated by the `exchange/detect#DETECT` sniffer.

[RAIL_LAW]:
- Package: `python-docx`
- Owns: `.docx` construction and editing — paragraphs/runs/headings, tables with horizontal+vertical merge/grid-span/nesting and per-row geometry, document- and run-level inline pictures, breaks/tabs, sections with the header/footer triad and full page geometry, the named-style catalog with latent styles and base-style inheritance, the full `Font` direct-formatting surface, the full `ParagraphFormat` block-geometry surface, comments with range marking, the 15-field core properties, ordered block iteration; the `document/emit#DOCUMENT` `DocumentMode.DOCX` lowering arm and the `document/lens#LENS` `.docx` extraction inverse.
- Accept: word-processing authoring lowering from the `DocumentNode` tree feeding the office and export-bundle owners, downstream of the `docxtpl` template path and the `msoffcrypto-tool` confidentiality edge, gated by `exchange/detect#DETECT`.
- Reject: wrapper-renames of `add_paragraph`/`save`; a per-run style rebuild where `Styles.add_style` and named styles exist; per-run direct font duplication where a `_CharacterStyle` carries the recurring formatting; raw-EMU integers where `Length` value objects exist; magic alignment/break/underline strings where the `WD_*` enums exist; per-page section objects where a section sequence suffices; ad-hoc paragraph-stitching where a `docxtpl` template carries the layout; opaque-payload emission where the `DocumentNode` lowering fold belongs; raised exceptions crossing the boundary where the `expression` `Result` rail belongs; identity minting the runtime owns.
