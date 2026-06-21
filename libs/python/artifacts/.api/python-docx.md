# [PY_ARTIFACTS_API_PYTHON_DOCX]

`python-docx` supplies the Word `.docx` document surface for the artifacts office rail: a polymorphic `Document` factory that opens an existing file/stream or creates from the default template, returning a `document.Document` whose `add_*` content family (paragraph, heading, table, picture, section, page-break, comment), block-level `Run`/`Paragraph`/`Table`/`Section` owners, named-style catalog, and `CoreProperties` metadata drive OOXML word-processing construction and editing. The package owner composes `Document`, the `add_*` family, the `Run.add_break`/`add_picture`/`add_tab` inline surface, the `Section` header/footer/page-geometry surface, the `Styles.add_style` catalog, and the `Inches`/`Pt`/`Cm`/`Emu`/`Twips` value objects into the office owner; it removes any raw-EMU integer math because the `Length` value objects own unit conversion, and it never re-implements the Office Open XML part graph, the lxml-backed element serialization, or the relationship/content-type bookkeeping python-docx already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-docx`
- package: `python-docx`
- import: `docx`
- owner: `artifacts`
- rail: office
- installed: `1.2.0` (uv.lock pin, ungated — runs on cp315-core; reflected on cp313 with matching version)
- license: MIT (Steve Canny) — permissive, no copyleft gate; aligns with the MIT/BSD sibling office owners
- abi: pure Python; single runtime dependency `lxml` (BSD) carries the compiled XML extension and is the only ABI surface; `typing_extensions` on older interpreters. Installs clean on cp315
- entry points: none (library only)
- capability: `.docx` construction and editing — paragraphs with runs and named styles, headings, tables (rows/columns/cells, nested tables, cell merge, vertical alignment), inline pictures (document- and run-level), page/line/column breaks and tabs, sections (header/footer triad, margins, orientation, page geometry), the named paragraph/character/table/list style catalog with latent styles, comments, core document properties, and ordered block iteration via `iter_inner_content`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and block content types
- rail: office

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]  | [CAPABILITY]                                              |
| :-----: | :----------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `document.Document`            | document object | root object: `add_*` family, block accessors, save        |
|  [02]   | `text.paragraph.Paragraph`     | paragraph       | runs, hyperlinks, alignment, paragraph_format, style      |
|  [03]   | `text.run.Run`                 | run             | styled text plus inline break/picture/tab/text/font       |
|  [04]   | `text.hyperlink.Hyperlink`     | hyperlink       | run-bearing hyperlink inside a paragraph                  |
|  [05]   | `table.Table`                  | table           | rows/columns, cell access, autofit, alignment             |
|  [06]   | `table._Cell`                  | cell            | paragraphs, nested tables, merge, vertical alignment      |
|  [07]   | `section.Section`              | section         | page geometry, margins, orientation, header/footer triad  |
|  [08]   | `shape.InlineShape`            | inline shape    | the picture/drawing returned by `add_picture`             |

[PUBLIC_TYPE_SCOPE]: style, metadata, and unit types
- rail: office

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]                                          |
| :-----: | :-------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `styles.styles.Styles`            | style catalog  | named-style add/lookup/default, latent styles          |
|  [02]   | `styles.style._ParagraphStyle`    | paragraph style| named paragraph style with font/format                |
|  [03]   | `styles.style._CharacterStyle`    | character style| named run style                                       |
|  [04]   | `opc.coreprops.CoreProperties`    | metadata       | author/title/created/modified/keywords/category set    |
|  [05]   | `shared.Length`                   | unit base      | EMU value object (`Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips`)|
|  [06]   | `shared.RGBColor`                 | color          | run-font color value object                           |
|  [07]   | `enum.text.WD_ALIGN_PARAGRAPH`    | alignment enum | paragraph alignment (also `WD_BREAK`/`WD_LINE_SPACING`)|
|  [08]   | `enum.section.WD_SECTION_START`   | section enum   | section start kind (also `WD_ORIENTATION`)            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document factory, content, save
- rail: office

`Document(docx=None)` is the single open-or-create factory; the `add_*` rows mint block content and return the created owner. Picture/section sizing takes `Length` value objects, never ints.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                                                              | [CAPABILITY]                                  |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Document`                  | `Document(docx: str | IO[bytes] | None = None) -> document.Document`                      | open a file/stream or create from default      |
|  [02]   | `Document.add_paragraph`    | `add_paragraph(text='', style: str | ParagraphStyle | None = None) -> Paragraph`          | add a styled paragraph                        |
|  [03]   | `Document.add_heading`      | `add_heading(text='', level=1) -> Paragraph`                                              | add a heading (level 0 = title)               |
|  [04]   | `Document.add_table`        | `add_table(rows, cols, style: str | _TableStyle | None = None) -> Table`                  | add a table                                   |
|  [05]   | `Document.add_picture`      | `add_picture(image_path_or_stream, width: Length | None = None, height: Length | None = None) -> InlineShape` | add a document-level inline picture |
|  [06]   | `Document.add_section`      | `add_section(start_type: WD_SECTION = NEW_PAGE) -> Section`                                | add a section                                 |
|  [07]   | `Document.add_page_break`   | `add_page_break() -> Paragraph`                                                           | add a page break                              |
|  [08]   | `Document.add_comment`      | `add_comment(runs: Run | Sequence[Run], text='', author='', initials=None) -> Comment`    | anchor a comment over runs (since 1.1)        |
|  [09]   | `Document.save`             | `save(path_or_stream)`                                                                    | serialize to a path or stream                 |
|  [10]   | `Document.iter_inner_content`| `iter_inner_content() -> Iterator[Paragraph | Table]`                                    | walk body blocks in document order            |

[ENTRYPOINT_SCOPE]: inline run, table, and section authoring
- rail: office

The `Run` inline surface is where text/break/picture/tab live; `Table`/`_Cell` own grid growth and merging; `Section` owns page geometry and the header/footer triad.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                  | [CAPABILITY]                                          |
| :-----: | :------------------------- | :---------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Paragraph.add_run`        | `add_run(text=None, style: str | CharacterStyle | None = None) -> Run`        | add a styled run                                      |
|  [02]   | `Paragraph.insert_paragraph_before`| `insert_paragraph_before(text=None, style=None) -> Paragraph`        | insert before this paragraph                          |
|  [03]   | `Run.add_break`            | `add_break(break_type: WD_BREAK = LINE)`                                      | inline line/page/column break                         |
|  [04]   | `Run.add_picture`          | `add_picture(image_path_or_stream, width=None, height=None) -> InlineShape`   | run-level inline picture (flows with text)            |
|  [05]   | `Run.add_tab` / `add_text` | `add_tab()` / `add_text(text)`                                               | inline tab / append run text                          |
|  [06]   | `Table.add_row` / `add_column`| `add_row() -> _Row` / `add_column(width: Length) -> _Column`              | grow the grid                                         |
|  [07]   | `Table.cell`               | `cell(row_idx, col_idx) -> _Cell`                                            | address a cell (also `row_cells`/`column_cells`)      |
|  [08]   | `_Cell.merge` / `add_table`| `merge(other_cell) -> _Cell` / `add_table(rows, cols) -> Table`              | span/merge cells; nest a table                        |
|  [09]   | `Section.header` / `footer`| `header -> _Header` (also `first_page_header`/`even_page_header`)            | the header/footer triad                               |
|  [10]   | `Styles.add_style`         | `add_style(name, style_type: WD_STYLE_TYPE, builtin=False) -> BaseStyle`     | mint a reusable named style (also `default`/`get_by_id`)|

[ENTRYPOINT_SCOPE]: accessors and metadata
- rail: office

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                            | [CAPABILITY]                                   |
| :-----: | :------------------------- | :-------------------------------------- | :--------------------------------------------- |
|  [01]   | `Document.paragraphs`      | `paragraphs -> list[Paragraph]`         | body paragraphs                                |
|  [02]   | `Document.tables`          | `tables -> list[Table]`                 | body tables                                    |
|  [03]   | `Document.sections`        | `sections -> Sections`                  | section collection                             |
|  [04]   | `Document.styles`          | `styles -> Styles`                      | the named-style catalog                        |
|  [05]   | `Document.inline_shapes`   | `inline_shapes -> InlineShapes`         | all inline pictures/drawings                   |
|  [06]   | `Document.core_properties` | `core_properties -> CoreProperties`     | author/title/created/modified/keywords/category|
|  [07]   | `Section.orientation`      | `orientation` / `page_width` / `page_height` | page geometry (`WD_ORIENTATION`)          |
|  [08]   | `Run.font`                 | `font -> Font` (`bold`/`italic`/`underline`/`color`) | run typography value                |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_DOCX]:
- import: `import docx` at boundary scope only; module-level import is banned by the manifest import policy. The distribution is `python-docx`; the import name is `docx`.
- document axis: `docx.Document(docx=None)` is the single polymorphic factory for both open and create (path/stream opens, `None` creates from the default template); it returns the `document.Document` object, never a parallel reader/writer split.
- content axis: `add_paragraph`/`add_heading`/`add_table`/`add_picture`/`add_section`/`add_page_break`/`add_comment` is one content-add surface on the document object; each content kind is a row, never a parallel document type. The inline run surface (`Run.add_break`/`add_picture`/`add_tab`/`add_text`) is the within-paragraph mirror — a run-level picture flows with text where the document-level `add_picture` is block-anchored.
- table axis: `Table.add_row`/`add_column`/`cell`/`row_cells`/`column_cells` own grid growth and addressing; `_Cell.merge` spans cells and `_Cell.add_table` nests — never a parallel merged-cell or nested-table type. A table cell is authored through the same `_Cell.add_paragraph` surface as the body.
- styling axis: named styles come from `Document.styles`; `Styles.add_style(name, WD_STYLE_TYPE, builtin=)` mints a reusable row and `default(style_type)` reads the catalog default — styles are named rows attached to paragraphs/runs/tables, never per-run duplication. `Run.font` carries `RGBColor`/`WD_UNDERLINE` directly only for ad-hoc emphasis the catalog does not name.
- section axis: `Section` owns page geometry (`page_width`/`page_height`/`orientation`/margins) and the header/footer triad (`header`/`first_page_header`/`even_page_header` and footer mirrors); a multi-layout document is a sequence of section rows, never per-page objects.
- unit axis: all measurements use `Inches`/`Pt`/`Cm`/`Mm`/`Emu`/`Twips` `Length` value objects, never raw EMU ints; alignment/break/section-start are `WD_ALIGN_PARAGRAPH`/`WD_BREAK`/`WD_SECTION_START` enum rows, never magic strings.
- evidence: each document op captures paragraph count, table count, image count, section count, comment count, and output byte length as an office receipt.
- boundary: python-docx owns `.docx`. Integration: `docxtpl` wraps a `Document` as a jinja2 template — prefer `docxtpl.DocxTemplate.render` for styled fill-from-context and reserve raw `python-docx` for programmatic block construction. `msoffcrypto-tool` decrypts an encrypted container to a plaintext stream `Document(stream)` reads, and re-seals the saved bytes; `python-magic` gates which reader runs at admission. Excel routes to `openpyxl`, PowerPoint to `python-pptx`, ODF to `odfpy`, PDF render to `pymupdf`/`weasyprint`. Live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-docx`
- Owns: `.docx` construction and editing — paragraphs/runs/headings, tables with merge and nesting, document- and run-level inline pictures, breaks/tabs, sections with header/footer and page geometry, the named-style catalog, comments, core properties, ordered block iteration
- Accept: word-processing authoring feeding the office and export-bundle owners, downstream of the `docxtpl` template path and the `msoffcrypto-tool` confidentiality edge
- Reject: wrapper-renames of `add_paragraph`/`save`; a per-run style rebuild where `Styles.add_style` and named styles exist; raw-EMU integers where `Length` value objects exist; magic alignment/break strings where the `WD_*` enums exist; ad-hoc paragraph-stitching where a `docxtpl` template carries the layout; identity minting the runtime owns
