# [PY_ARTIFACTS_API_ODFPY]

`odfpy` supplies the pure-Python OpenDocument read surface for the artifacts ingest rail: `load` parses an `.ods`/`.odf` ZIP container into an `OpenDocument` tree, the `odf.element.Element` node API walks the tree with `getElementsByType` and `getAttribute`, the `odf.table` element factories name the spreadsheet shapes (`Table`, `TableRow`, `TableCell`), and `odf.teletype.extractText` flattens a cell's text run into a string with whitespace correctly unwrapped. The package owner composes `load`, `getElementsByType`, `getAttribute`, and `extractText` into the `OFFICE_INGEST` ODS read path; it removes any LibreOffice/UNO bridge because the entire OASIS parse is in-package and dependency-free, and it never re-implements the ODF ZIP-plus-XML container odfpy already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odfpy`
- package: `odfpy`
- import: `odf`
- owner: `artifacts`
- rail: ingest
- installed: `1.4.1` reflected via assay api on cp315
- entry points: console scripts `csv2ods`, `mailodf`, `odf2mht`, `odf2xhtml`, `odf2xml`, `odfimgimport`, `odflint`, `odfmeta`, `odfoutline`, `odfuserfield`, `xml2odf`; library use is import-only
- capability: ODF container parse (`load`) of any OASIS document (spreadsheet/text/presentation) from a path or readable stream into an `OpenDocument` tree; DOM-style traversal via `getElementsByType`/`getAttribute`/`getAttrNS`; namespaced element factories per ODF module (`odf.table`, `odf.text`, `odf.draw`, ...); whitespace-correct text extraction via `extractText`; round-trip serialization back to an `.ods`/`.odt` ZIP

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and node API
- rail: ingest

`load` returns an `OpenDocument`; `OpenDocument.spreadsheet`/`.body`/`.text` expose the content roots, and every node in the parsed tree is an `odf.element.Element` carrying `qname`, `childNodes`, and the attribute map. Element factories such as `Table`/`TableRow`/`TableCell` are namespaced constructors keyed by `getElementsByType` and `isInstanceOf`, not parallel node classes.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [RAIL]                                                       |
| :-----: | :------------------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `odf.opendocument.OpenDocument` | document root   | parsed ODF tree holding mimetype, body, and content parts    |
|  [02]   | `odf.element.Element`           | DOM node        | base node with `qname`/`childNodes`/attributes and traversal |
|  [03]   | `odf.element.Node`              | node base       | node-type constants (`ELEMENT_NODE`, `TEXT_NODE`) and tree   |
|  [04]   | `odf.element.Text`              | text node       | leaf text node carrying `data`                               |
|  [05]   | `odf.teletype.WhitespaceText`   | text serializer | whitespace-aware text accumulator backing `extractText`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container parse
- rail: ingest

`load` opens the ODF ZIP, reads `META-INF/manifest.xml`, and parses the `content.xml`/`styles.xml`/`meta.xml`/`settings.xml` parts into one `OpenDocument`; it accepts a filename or any open readable binary stream.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                      | [CAPABILITY]                                       |
| :-----: | :---------------------- | :-------------------------------- | :------------------------------------------------- |
|  [01]   | `odf.opendocument.load` | `load(odffile)` -> `OpenDocument` | parse an ODF path or readable stream into the tree |

[ENTRYPOINT_SCOPE]: tree traversal and attribute read
- rail: ingest

`getElementsByType` takes an element factory function (`Table`, `TableRow`, `TableCell`, `P`) and returns every matching node in subtree order; `getAttribute` reads a value by its lowercase, dash-stripped local name while `getAttrNS` reads by explicit `(namespace, localpart)`; `extractText` flattens a node's text run.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                    | [CAPABILITY]                                                     |
| :-----: | :-------------------------- | :---------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `Element.getElementsByType` | `getElementsByType(element)` -> `list[Element]` | collect all nodes whose qname matches the factory's qname        |
|  [02]   | `Element.getAttribute`      | `getAttribute(attr)` -> `str                    | None`                                                            | read attribute by lowercase dash-stripped local name      |
|  [03]   | `Element.getAttrNS`         | `getAttrNS(namespace, localpart)` -> `str       | None`                                                            | read attribute by explicit `(namespace, localpart)` qname |
|  [04]   | `Element.isInstanceOf`      | `isInstanceOf(element)` -> `bool`               | test a node's qname against a factory function                   |
|  [05]   | `odf.teletype.extractText`  | `extractText(odfElement)` -> `str`              | flatten a node to text, unwrapping `<text:s>`/`tab`/`line-break` |

[ENTRYPOINT_SCOPE]: spreadsheet element factories
- rail: ingest

The `odf.table` factories construct namespaced `Element` nodes (qname under `TABLENS`); the ingest path uses them as the type key for `getElementsByType`. `TableCell.getAttribute('valuetype')`/`('value')` reads the typed cell value; `getAttribute('numbercolumnsrepeated')`/`('numberrowsrepeated')` reads the run-length repeat count.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                            | [CAPABILITY]                                            |
| :-----: | :--------------------------- | :-------------------------------------- | :------------------------------------------------------ |
|  [01]   | `odf.table.Table`            | `Table(**args)` -> `Element`            | `table:table` sheet node                                |
|  [02]   | `odf.table.TableRow`         | `TableRow(**args)` -> `Element`         | `table:table-row` node                                  |
|  [03]   | `odf.table.TableCell`        | `TableCell(**args)` -> `Element`        | `table:table-cell` node (carries value-type and repeat) |
|  [04]   | `odf.table.CoveredTableCell` | `CoveredTableCell(**args)` -> `Element` | `table:covered-table-cell` (merged-span placeholder)    |
|  [05]   | `odf.table.TableColumn`      | `TableColumn(**args)` -> `Element`      | `table:table-column` node                               |
|  [06]   | `odf.table.TableHeaderRows`  | `TableHeaderRows(**args)` -> `Element`  | `table:table-header-rows` node                          |
|  [07]   | `odf.text.P`                 | `P(**args)` -> `Element`                | `text:p` paragraph node (cell text run)                 |

## [04]-[IMPLEMENTATION_LAW]

[INGEST_ODF]:
- import: `from odf.opendocument import load`, `from odf.table import Table, TableRow, TableCell`, `from odf.text import P`, `from odf.teletype import extractText` at boundary scope only; module-level import is banned by the manifest import policy.
- parse axis: one `load` owns container parse; path versus open binary stream is the argument, never a per-source loader type; the returned `OpenDocument` is the single tree root for every downstream walk.
- traversal axis: `getElementsByType` keyed by an element factory is the single subtree collector; sheet, row, and cell enumeration are factory rows (`Table`, `TableRow`, `TableCell`), never a hand-written XML walk or a parallel `findRows`/`findCells` family.
- attribute axis: `getAttribute` by lowercase dash-stripped local name owns typed-cell reads; `valuetype`/`value`/`numbercolumnsrepeated`/`numberrowsrepeated` are attribute rows resolved against the cell's allowed-attribute table, with `getAttrNS((namespace, localpart))` the explicit-qname row when the local name is ambiguous.
- text axis: `extractText` owns cell text flattening; `<text:s>`/`<text:tab>`/`<text:line-break>` unwrapping is in-library, never a manual `childNodes` string join.
- repeat axis: `numbercolumnsrepeated` and `numberrowsrepeated` are read as integer run-lengths and expanded into dense cell positions on ingest; a repeated cell is a count row, never a literal duplicated node.
- evidence: each ingested sheet captures the source mimetype, sheet name, row count, column count, per-cell value-type and value, and the expanded repeat count as an ingest receipt.
- boundary: odfpy owns ODF/ODS container parse, DOM traversal, and text extraction with no UNO/LibreOffice dependency; OOXML `.xlsx` routes to its own owner; the parsed cell stream feeds the tabular ingest owner directly; document authoring/serialization stays outside this read path.

[RAIL_LAW]:
- Package: `odfpy`
- Owns: ODF/ODS container parse, `Element` DOM traversal, namespaced element factories, typed-attribute reads, and whitespace-correct text extraction
- Accept: ODF/ODS read feeding the tabular ingest owner via `load` -> `getElementsByType` -> `getAttribute`/`extractText`
- Reject: wrapper-renames of `load`/`getElementsByType`/`getAttribute`; a hand-rolled ODF ZIP-plus-XML parser; a UNO/LibreOffice bridge where the in-package parse needs no external process; a parallel node class per element kind; a manual `childNodes` text join where `extractText` owns whitespace unwrapping
