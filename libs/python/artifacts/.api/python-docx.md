# [PY_ARTIFACTS_API_PYTHON_DOCX]

`python-docx` supplies the Word `.docx` document surface for the artifacts office rail: a `Document` factory returning a document object whose content-add family (paragraphs, headings, tables, pictures, sections, page breaks) and style/section/property accessors drive OOXML word-processing document construction and editing. The package owner composes `Document`, the `add_*` family, and the style/section accessors into the office owner; it never re-implements OOXML parsing python-docx already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-docx`
- package: `python-docx`
- import: `docx`
- owner: `artifacts`
- rail: office
- installed: `1.2.0` reflected via `python -c "import docx"` on cp315
- entry points: none (library only)
- capability: `.docx` document construction and editing — paragraphs, runs, headings, tables, inline pictures, sections, styles, core properties, comments, page breaks

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and content types
- rail: office

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]  | [CAPABILITY]                  |
| :-----: | :------------------------- | :-------------- | :---------------------------- |
|   [1]   | `document.Document`        | document object | root document object          |
|   [2]   | `text.paragraph.Paragraph` | paragraph       | paragraph with runs and style |
|   [3]   | `text.run.Run`             | run             | styled text run               |
|   [4]   | `table.Table`              | table           | grid of cells                 |
|   [5]   | `section.Section`          | section         | page-layout section           |
|   [6]   | `ParagraphStyle`           | style           | named paragraph style         |
|   [7]   | `CharacterStyle`           | style           | named character style         |
|   [8]   | `shared.Inches`            | unit            | inch measurement value        |
|   [9]   | `Pt` / `Cm` / `Length`     | unit            | point/centimeter/EMU units    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document open and content authoring
- rail: office

Input rows use path-or-stream sources and optional style/unit policies; content rows return the created document object.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                   | [CAPABILITY]                                 |
| :-----: | :------------------------ | :----------------------------- | :------------------------------------------- |
|   [1]   | `Document`                | optional path or stream source | open or create a document (factory function) |
|   [2]   | `Document.add_paragraph`  | text plus optional style       | add a paragraph                              |
|   [3]   | `Document.add_heading`    | text plus heading level        | add a heading                                |
|   [4]   | `Document.add_table`      | row/column count plus style    | add a table                                  |
|   [5]   | `Document.add_picture`    | image source plus size policy  | add an inline picture                        |
|   [6]   | `Document.add_section`    | section-start policy           | add a section                                |
|   [7]   | `Document.add_page_break` | no-arg page-break insertion    | add a page break                             |
|   [8]   | `Document.save`           | path or stream target          | serialize the document                       |

[ENTRYPOINT_SCOPE]: accessors and properties
- rail: office

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                            | [CAPABILITY]                    |
| :-----: | :------------------------- | :-------------------------------------- | :------------------------------ |
|   [1]   | `Document.paragraphs`      | `paragraphs -> list[Paragraph]`         | document paragraphs             |
|   [2]   | `Document.tables`          | `tables -> list[Table]`                 | document tables                 |
|   [3]   | `Document.sections`        | `sections -> Sections`                  | document sections               |
|   [4]   | `Document.styles`          | `styles -> Styles`                      | the style catalog               |
|   [5]   | `Document.core_properties` | `core_properties -> CoreProperties`     | author/title/created metadata   |
|   [6]   | `Paragraph.add_run`        | `add_run(text=None, style=None) -> Run` | add a styled run to a paragraph |

## [4]-[IMPLEMENTATION_LAW]

[OFFICE_DOCX]:
- import: `import docx` at boundary scope only; module-level import is banned by the manifest import policy. The package distribution is `python-docx`; the import name is `docx`.
- document axis: `docx.Document(...)` is the single factory for both open and create (the `docx` argument is `None` to create); it returns the `document.Document` object, never a parallel reader/writer split.
- content axis: the `add_paragraph`/`add_heading`/`add_table`/`add_picture`/`add_section`/`add_page_break` family is one content-add surface on the document object; each content kind is a row, never a parallel document type.
- styling axis: named styles from `Document.styles` attach to paragraphs/runs; styles are reusable named rows, never per-run duplication; measurements use `Inches`/`Pt`/`Length` value objects, never raw EMU ints.
- evidence: each document op captures paragraph count, table count, image count, section count, and output byte length as an office receipt.
- boundary: python-docx owns `.docx`; Excel routes to `openpyxl`, PowerPoint to `python-pptx`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-docx`
- Owns: `.docx` construction and editing — paragraphs, headings, tables, pictures, sections, styles, core properties, page breaks
- Accept: word-processing document authoring feeding the office and export-bundle owners
- Reject: wrapper-renames of `add_paragraph`/`save`; a per-run style rebuild where named styles exist; raw-EMU measurements where unit value objects exist; identity minting the runtime owns
