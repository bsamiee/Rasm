# [PY_ARTIFACTS_API_ODFPY]

`odfpy` supplies the pure-Python OpenDocument read+write DOM for the artifacts office rail: the `OpenDocument*` factory family mints typed OASIS document trees, the camelCase `Element` write API (`addElement`/`addText`/`setAttribute`) authors the namespaced graph, `save`/`write` serialize the ZIP-plus-XML container, and on the read side `load` parses an `.ods`/`.odt`/`.odp` container back into the same tree that `getElementsByType`/`getAttribute`/`teletype.extractText` walk. The package owner composes the factory roots, the `Element` write API, and `save` into the `OFFICE_ODF` authoring path and the `load` -> `getElementsByType` -> `getAttribute` read path; it removes any LibreOffice/UNO bridge because the entire OASIS parse-and-serialize is in-package, and never re-implements the ODF ZIP-plus-XML container, the namespaced allowed-attribute grammar, or the whitespace-correct text serialization odfpy already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odfpy`
- package: `odfpy`
- import: `odf`
- owner: `artifacts`
- rail: office
- installed: `1.4.1` reflected via reflection on cp315 (Python 3.15)
- license: triple-classified Apache-2.0 / GPL / LGPL (GPLv2+ with LGPL option) — copyleft; gate any redistribution-sensitive bundling against this, distinct from the MIT/BSD sibling office owners
- abi: pure Python, sdist-only (`odfpy-1.4.1.tar.gz`); single runtime dependency `defusedxml` (PSF-2.0) hardens the XML parse against entity-expansion attacks; no compiled extension, installs clean on cp315
- entry points: console scripts `csv2ods`, `mailodf`, `odf2mht`, `odf2xhtml`, `odf2xml`, `odfimgimport`, `odflint`, `odfmeta`, `odfoutline`, `odfuserfield`, `xml2odf`; library use is import-only
- capability: ODF document authoring via the `OpenDocument*` factory family (spreadsheet/text/presentation/drawing/chart/image/text-master), namespaced `Element` DOM write API (`addElement`/`addText`/`addCDATA`/`setAttribute`/`setAttrNS` plus W3C-DOM `appendChild`/`insertBefore`/`removeChild`), grammar-checked allowed-attribute validation per module, embedded-object/picture/thumbnail attachment, container serialization (`save`/`write`/`xml`/`contentxml`/`stylesxml`/`metaxml`/`settingsxml`), `load` parse of any OASIS document from a path or readable stream, DOM traversal (`getElementsByType`/`getAttribute`/`getAttrNS`/`isInstanceOf`), and whitespace-correct text extract/inject (`teletype.extractText`/`addTextToElement`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document factory roots
- rail: office — `odf.opendocument`

The `OpenDocument*` factories are the authoring entry: each returns an `OpenDocument` pre-seeded with the correct mimetype, `body`, content root, `automaticstyles`, `styles`, and `masterstyles` containers for that ODF flavor. There is one document type discriminated by factory, never a parallel writer per flavor; `load` reconstructs the same `OpenDocument` from disk.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]    | [RAIL]                                                            |
| :-----: | :------------------------------------ | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `odf.opendocument.OpenDocument`       | document root    | tree holding `mimetype`, `body`, content/styles/meta/settings parts; owns `save`/`write`/`xml` and `getElementsByType` |
|  [02]   | `odf.opendocument.OpenDocumentSpreadsheet` | factory     | `.ods` root seeded with `spreadsheet` body                       |
|  [03]   | `odf.opendocument.OpenDocumentText`   | factory          | `.odt` root seeded with `text` body                              |
|  [04]   | `odf.opendocument.OpenDocumentPresentation` | factory    | `.odp` root seeded with `presentation` body                      |
|  [05]   | `odf.opendocument.OpenDocumentDrawing` | factory         | `.odg` root seeded with `drawing` body                           |
|  [06]   | `odf.opendocument.OpenDocumentChart`  | factory          | `.odc` chart-document root                                       |
|  [07]   | `odf.opendocument.OpenDocumentImage`  | factory          | `.odi` image-document root                                       |
|  [08]   | `odf.opendocument.OpenDocumentTextMaster` | factory      | `.odm` master-document root                                      |

[PUBLIC_TYPE_SCOPE]: DOM node API
- rail: office — `odf.element`

Every node in the tree (factory-built or parsed) is one `odf.element.Element`; element factories (`Table`, `P`, `Style`, ...) are namespaced constructors keyed by qname, not parallel node classes. `Element` carries the W3C-DOM node-type constants and the `allowed_attributes` grammar that `check_grammar=True` validates writes against.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]   | [RAIL]                                                                 |
| :-----: | :---------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `odf.element.Element`         | DOM node        | base node: `qname`, `childNodes`, `parentNode`, attribute map, `allowed_attributes` grammar, write+read+traverse API |
|  [02]   | `odf.element.Node`            | node base       | W3C node-type constants (`ELEMENT_NODE`, `TEXT_NODE`, `CDATA_SECTION_NODE`, ...) and tree links |
|  [03]   | `odf.element.Text`            | text leaf       | `#text` leaf carrying `data`                                          |
|  [04]   | `odf.element.CDATASection`    | cdata leaf      | CDATA leaf for raw/script payloads                                    |
|  [05]   | `odf.element.Childless`       | leaf mixin      | base for elements that reject children (raises `IllegalChild`)        |
|  [06]   | `odf.element.IllegalChild`    | error           | grammar reject when a child is not allowed under the parent qname     |
|  [07]   | `odf.element.IllegalText`     | error           | grammar reject when text is added to a non-text element               |
|  [08]   | `odf.teletype.WhitespaceText` | text serializer | whitespace-aware accumulator backing `extractText`/`addTextToElement` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document author and serialize
- rail: office — `odf.opendocument`

A factory mints the `OpenDocument`; authored nodes attach to a content container — `addElement` lives on `Element`, so nodes go through `doc.body.addElement(...)` (or the flavor alias `doc.spreadsheet`/`doc.text` and the style containers `doc.automaticstyles`/`doc.styles`), never `OpenDocument.addElement`; styles register through `automaticstyles`/`styles`; `save`/`write` serialize; the `*xml` accessors expose individual parts without writing a file. `createElement`/`createTextNode`/`createCDATASection` are the DOM-factory mirror, and `getMediaType` returns the document mimetype.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                       | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `OpenDocument*` factory                | `OpenDocumentSpreadsheet()` -> `OpenDocument`      | mint a typed document tree (mimetype + body seeded)     |
|  [02]   | `OpenDocument.body` (`Element`) `.addElement` | `doc.body.addElement(element, check_grammar=True)` | attach an authored node to the content root (validates allowed-child grammar); `OpenDocument` itself has no `addElement` |
|  [03]   | `OpenDocument.save`                    | `save(outputfile, addsuffix=False)`                | serialize to a path (or `.stdout`); optional suffix add |
|  [04]   | `OpenDocument.write`                   | `write(outputfp)`                                  | serialize the ZIP to an open binary stream              |
|  [05]   | `OpenDocument.xml`                     | `xml()` -> `bytes`                                 | full packaged document as bytes without touching disk   |
|  [06]   | `OpenDocument.contentxml`              | `contentxml()` -> `bytes`                          | the `content.xml` part only                             |
|  [07]   | `OpenDocument.stylesxml`               | `stylesxml()` -> `bytes`                           | the `styles.xml` part only                              |
|  [08]   | `OpenDocument.getStyleByName`          | `getStyleByName(name)` -> `Element \| None`        | resolve a registered style element by `style:name`      |
|  [09]   | `OpenDocument.addPictureFromFile`      | `addPictureFromFile(filename, mediatype=None)` -> `str` | embed an image part, return its manifest href      |
|  [10]   | `OpenDocument.addPictureFromString`    | `addPictureFromString(content, mediatype)` -> `str` | embed image bytes, return its manifest href            |
|  [11]   | `OpenDocument.addObject`               | `addObject(document, objectname=None)` -> `str`    | embed a sub-`OpenDocument` (chart/object) part          |
|  [12]   | `OpenDocument.addThumbnail`            | `addThumbnail(filecontent=None)`                   | attach the `Thumbnails/thumbnail.png` preview           |

[ENTRYPOINT_SCOPE]: Element write and traverse
- rail: office — `odf.element`

`Element` is one node that both authors (write API + W3C-DOM mutation) and reads (traverse + attribute get). `check_grammar=True` validates each write against the module's `allowed_attributes`; pass `check_grammar=False` only at a confirmed boundary. `teletype.addTextToElement` is the write-side mirror of `extractText`, splitting a string into `<text:s>`/`<text:tab>`/`<text:line-break>` runs.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                         | [CAPABILITY]                                                       |
| :-----: | :----------------------------- | :--------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `Element.addElement`           | `addElement(element, check_grammar=True)`            | append a child node; grammar-validate the parent/child qname pair  |
|  [02]   | `Element.addText`              | `addText(text, check_grammar=True)`                  | append a `Text` leaf; rejects on `Childless`/`IllegalText`         |
|  [03]   | `Element.addCDATA`             | `addCDATA(cdata, check_grammar=True)`                | append a `CDATASection` (raw script/formula payload)               |
|  [04]   | `Element.setAttribute`         | `setAttribute(attr, value, check_grammar=True)`      | set an attribute by lowercase dash-stripped local name             |
|  [05]   | `Element.setAttrNS`            | `setAttrNS(namespace, localpart, value)`             | set an attribute by explicit `(namespace, localpart)` qname        |
|  [06]   | `Element.appendChild`          | `appendChild(node)`                                  | W3C-DOM append (no grammar check; raw tree mutation)               |
|  [07]   | `Element.insertBefore`         | `insertBefore(newChild, refChild)`                   | W3C-DOM positional insert                                          |
|  [08]   | `Element.removeChild`          | `removeChild(oldChild)`                              | W3C-DOM detach                                                     |
|  [09]   | `Element.getElementsByType`    | `getElementsByType(element)` -> `list[Element]`      | collect all nodes whose qname matches the factory's qname          |
|  [10]   | `Element.getAttribute`         | `getAttribute(attr)` -> `str \| None`                | read attribute by lowercase dash-stripped local name               |
|  [11]   | `Element.getAttrNS`            | `getAttrNS(namespace, localpart)` -> `str \| None`   | read attribute by explicit `(namespace, localpart)` qname          |
|  [12]   | `Element.removeAttribute`      | `removeAttribute(attr, check_grammar=True)` / `removeAttrNS(namespace, localpart)` | drop an attribute by local name or explicit qname    |
|  [13]   | `Element.isInstanceOf`         | `isInstanceOf(element)` -> `bool`                    | test a node's qname against a factory function                     |
|  [14]   | `Element.toXml`                | `toXml(level, f)`                                    | serialize this subtree into the writer at indent level             |
|  [15]   | `odf.teletype.extractText`     | `extractText(odfElement)` -> `str`                   | flatten a node to text, unwrapping `<text:s>`/`tab`/`line-break`   |
|  [16]   | `odf.teletype.addTextToElement`| `addTextToElement(odfElement, s)`                    | inject a string as proper `<text:s>`/`tab`/`line-break` runs       |

[ENTRYPOINT_SCOPE]: namespaced element factory modules
- rail: office — `odf.table` / `odf.text` / `odf.style` / `odf.number` / `odf.draw`

Each ODF module exposes the element factories for one namespace; the factory is both the authoring constructor (`Table(name=...)`) and the type key for `getElementsByType`. Spreadsheet cells carry typed value attributes; the `odf.number` style factories build the date/currency/percentage number formats applied through `setAttribute('stylename', ...)`.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                | [CAPABILITY]                                                  |
| :-----: | :--------------------------- | :------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `odf.table.Table`            | `Table(name=..., **args)` -> `Element`      | `table:table` sheet node                                     |
|  [02]   | `odf.table.TableRow`         | `TableRow(**args)` -> `Element`             | `table:table-row` node                                       |
|  [03]   | `odf.table.TableCell`        | `TableCell(valuetype=..., value=..., **args)` -> `Element` | `table:table-cell`; carries value-type and repeat count |
|  [04]   | `odf.table.CoveredTableCell` | `CoveredTableCell(**args)` -> `Element`     | `table:covered-table-cell` (merged-span placeholder)         |
|  [05]   | `odf.table.TableColumn`      | `TableColumn(**args)` -> `Element`          | `table:table-column` node                                    |
|  [06]   | `odf.table.TableHeaderRows`  | `TableHeaderRows(**args)` -> `Element`      | `table:table-header-rows` node                               |
|  [07]   | `odf.text.P`                 | `P(stylename=..., **args)` -> `Element`     | `text:p` paragraph node (cell/body text run)                 |
|  [08]   | `odf.text.Span`              | `Span(stylename=..., **args)` -> `Element`  | `text:span` inline run for mixed styling                     |
|  [09]   | `odf.text.H`                 | `H(outlinelevel=..., **args)` -> `Element`  | `text:h` heading node                                        |
|  [10]   | `odf.style.Style`            | `Style(name=..., family=..., **args)` -> `Element` | a registered named/automatic style                    |
|  [11]   | `odf.style.TextProperties`   | `TextProperties(**args)` -> `Element`       | `style:text-properties` (font/weight/color child)           |
|  [12]   | `odf.style.TableCellProperties` | `TableCellProperties(**args)` -> `Element` | `style:table-cell-properties` (fill/border/align child)   |
|  [13]   | `odf.number.CurrencyStyle`   | `CurrencyStyle(name=..., **args)` -> `Element` | data-style for currency cells                            |
|  [14]   | `odf.draw.Frame`             | `Frame(**args)` -> `Element`                | `draw:frame` anchor for images/objects/text-boxes            |
|  [15]   | `odf.draw.Image`             | `Image(href=..., **args)` -> `Element`      | `draw:image` referencing an embedded picture part            |

## [04]-[IMPLEMENTATION_LAW]

[OFFICE_ODF]:
- import: `from odf.opendocument import OpenDocumentSpreadsheet, load`, `from odf.table import Table, TableRow, TableCell`, `from odf.text import P`, `from odf.style import Style, TextProperties`, `from odf.teletype import extractText, addTextToElement` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: one `OpenDocument` owns both author and serialize; the ODF flavor is the factory choice (`OpenDocumentSpreadsheet`/`OpenDocumentText`/`OpenDocumentPresentation`/...), never a parallel writer type per flavor; `load` reconstructs the same `OpenDocument` from a path or open binary stream, so read and write share one tree contract.
- authoring axis: `addElement` keyed by a namespaced factory is the single node-attach surface; `setAttribute`/`setAttrNS` are the attribute-write rows validated against the module's `allowed_attributes` grammar when `check_grammar=True`; styles register once through `automaticstyles`/`styles` and bind to cells/paragraphs by `style:name`, never per-cell style duplication.
- text axis: `addTextToElement` owns string-to-node injection (`<text:s>`/`tab`/`line-break` runs) and `extractText` owns the inverse flatten; a manual `childNodes` string join is never substituted because odfpy owns the whitespace contract on both directions.
- traversal axis: on read, `getElementsByType` keyed by an element factory is the single subtree collector; sheet/row/cell enumeration are factory rows (`Table`, `TableRow`, `TableCell`), never a hand-written XML walk or a parallel `findRows`/`findCells` family.
- attribute axis: `getAttribute` by lowercase dash-stripped local name owns typed-cell reads; `valuetype`/`value`/`numbercolumnsrepeated`/`numberrowsrepeated` are attribute rows resolved against the cell's allowed-attribute table, with `getAttrNS((namespace, localpart))` the explicit-qname row when the local name is ambiguous; `numbercolumnsrepeated`/`numberrowsrepeated` are read as integer run-lengths and expanded into dense cell positions on ingest, never literal duplicated nodes.
- serialization axis: `save`/`write` package the ZIP (`content.xml`/`styles.xml`/`meta.xml`/`settings.xml` + `META-INF/manifest.xml`) exactly once; `xml`/`contentxml`/`stylesxml`/`metaxml`/`settingsxml` expose individual parts as bytes for an in-memory boundary handoff that never writes a file; `defusedxml` backs the parse so the read path is entity-expansion safe.
- evidence: each authored or ingested document captures the source/target mimetype, sheet/section names, row count, column count, per-cell value-type and value, the expanded repeat count, and embedded-picture/object hrefs as an office receipt.
- boundary: odfpy owns the full ODF/ODS/ODT/ODP container author+parse, the `Element` DOM, namespaced element factories, typed-attribute reads, embedded-part attachment, and whitespace-correct text round-trip with no UNO/LibreOffice dependency; OOXML `.xlsx` authoring routes to `xlsxwriter` (streaming) or `openpyxl` (read+write), `.docx` to `python-docx`, `.pptx` to `python-pptx`; the in-memory `xml()`/`contentxml()` bytes feed the artifacts download and export owners directly; the copyleft license gates any redistribution-sensitive bundling distinct from the MIT/BSD sibling owners.

[RAIL_LAW]:
- Package: `odfpy`
- Owns: OpenDocument authoring via the `OpenDocument*` factory family, `Element` DOM write+read API, namespaced element factories per ODF module, grammar-checked attribute validation, embedded picture/object attachment, container serialization, ODF/ODS/ODT/ODP parse, and whitespace-correct text inject/extract
- Accept: ODF authoring feeding the office/download/export owners, and ODF/ODS ingest feeding the tabular ingest owner via `load` -> `getElementsByType` -> `getAttribute`/`extractText`
- Reject: wrapper-renames of the `OpenDocument*` factories/`save`/`load`/`addElement`/`getElementsByType`; a hand-rolled ODF ZIP-plus-XML parser or serializer; a UNO/LibreOffice bridge where the in-package round-trip needs no external process; a parallel node class or writer type per element kind or document flavor; a manual `childNodes` text join where `extractText`/`addTextToElement` own whitespace unwrapping; per-cell style duplication where named styles register once
