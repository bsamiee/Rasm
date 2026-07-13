# [PY_ARTIFACTS_API_ODFPY]

`odfpy` supplies the pure-Python OpenDocument read+write DOM for the artifacts office rail: the `OpenDocument*` factory family mints typed OASIS document trees, the camelCase `Element` write API (`addElement`/`addText`/`addCDATA`/`setAttribute`/`setAttrNS`) authors the namespaced graph against a per-module `allowed_attributes` grammar, the `odf.text`/`odf.table`/`odf.style`/`odf.number` element-factory modules supply the body/cell/style/data-style nodes, the `odf.dc`/`odf.meta` factories author the descriptive-metadata part, `odf.userfield.UserFields` programs the field-merge surface, `save`/`write`/`xml` serialize the ZIP-plus-XML container, and on the read side `load` parses an `.ods`/`.odt`/`.odp` container (from a path OR an open binary stream, `defusedxml`-hardened) back into the same tree that `getElementsByType`/`getAttribute`/`isInstanceOf`/`teletype.extractText` walk. The package owner composes the factory roots, the `Element` write API, the data-style + metadata + field families, and `save` into the `document/emit#DOCUMENT` `DocumentMode.ODT`/`ODS` authoring arm, and the `load` -> `getElementsByType` -> `getAttribute`/`extractText` read path into the `document/lens#LENS` `ODS_READ`/`ODT_READ` recovery arm; it removes any LibreOffice/UNO bridge because the entire OASIS parse-and-serialize is in-package, and never re-implements the ODF ZIP-plus-XML container, the namespaced allowed-attribute grammar, the data-style number formats, or the whitespace-correct text serialization odfpy already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odfpy`
- package: `odfpy`
- import: `odf`
- owner: `artifacts`
- rail: office
- installed: `1.4.1`
- floor: pure-Python (no compiled extension); abi-agnostic, runs on cp315
- runtime dep: `defusedxml` (the SAX parser `load` constructs through `defusedxml.sax.make_parser`, entity-expansion + billion-laughs safe on the read path)
- license: triple-classified Apache-2.0 / GPL / LGPL (GPLv2+ with LGPL option) — copyleft; gate any redistribution-sensitive bundling against this, distinct from the MIT/BSD sibling office owners (`xlsxwriter`/`openpyxl`/`python-docx`/`python-pptx`)
- entry points: console scripts `csv2ods`, `mailodf`, `odf2mht`, `odf2xhtml`, `odf2xml`, `odfimgimport`, `odflint`, `odfmeta`, `odfoutline`, `odfuserfield`, `xml2odf`; library use is import-only (no CLI subprocess — the in-process `OpenDocument`/`load` surface composes directly)
- capability: ODF document authoring via the `OpenDocument*` factory family (spreadsheet/text/presentation/drawing/chart/image/text-master), namespaced `Element` DOM write API (`addElement`/`addText`/`addCDATA`/`setAttribute`/`setAttrNS` plus W3C-DOM `appendChild`/`insertBefore`/`removeChild`), grammar-checked allowed-attribute validation per module, full data-style family (`odf.number` date/time/currency/percentage/number/boolean styles), descriptive metadata authoring (`odf.dc` Dublin-Core + `odf.meta` ODF-meta factories), programmable field-merge (`odf.userfield.UserFields`), list-style builders (`odf.easyliststyle`), embedded-object/picture/thumbnail attachment (`addPicture`/`addObject`/`addThumbnail`), the package manifest + AES encryption-data DOM (`odf.manifest`), container serialization (`save`/`write`/`xml`/`contentxml`/`stylesxml`/`metaxml`/`settingsxml`/`toXml`), `load` parse of any OASIS document from a path or readable stream, cache-lifecycle traversal (`build_caches`/`rebuild_caches`/`clear_caches`/`getElementsByType`/`getAttribute`/`getAttrNS`/`isInstanceOf`), and whitespace-correct text extract/inject (`teletype.extractText`/`addTextToElement`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document factory roots
- rail: office — `odf.opendocument`

The `OpenDocument*` factories are the authoring entry: each returns an `OpenDocument` pre-seeded with the correct mimetype, the flavor `body`/content root, `automaticstyles`, `styles`, `masterstyles`, `meta`, `settings`, `scripts`, and `fontfacedecls` containers for that ODF flavor. There is one document type discriminated by factory, never a parallel writer per flavor; `load` reconstructs the same `OpenDocument` from disk and re-attaches the flavor-matched body alias (`doc.spreadsheet`/`doc.text`/`doc.presentation`/`doc.drawing`-via-`graphics`/`doc.chart`/`doc.image`/`doc.formula`). Every `[SYMBOL]` is `odf.opendocument.*`; the `OpenDocument` `save`/`write`/`xml`/`getElementsByType`/`addPicture` surface is the entrypoints tables below.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                                                 |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `OpenDocument`             | document root | `mimetype`/`body`/content/styles/meta/settings + `Pictures` map + `element_dict` cache |
|  [02]   | `OpenDocumentSpreadsheet`  | factory       | `.ods` root seeded with a `spreadsheet` body (`doc.spreadsheet` alias)                 |
|  [03]   | `OpenDocumentText`         | factory       | `.odt` root seeded with a `text` body (`doc.text` alias)                               |
|  [04]   | `OpenDocumentPresentation` | factory       | `.odp` root seeded with a `presentation` body                                          |
|  [05]   | `OpenDocumentDrawing`      | factory       | `.odg` root seeded with a `drawing` body                                               |
|  [06]   | `OpenDocumentChart`        | factory       | `.odc` chart-document root                                                             |
|  [07]   | `OpenDocumentImage`        | factory       | `.odi` image-document root                                                             |
|  [08]   | `OpenDocumentTextMaster`   | factory       | `.odm` master-document root                                                            |

[PUBLIC_TYPE_SCOPE]: DOM node API and grammar reject rail
- rail: office — `odf.element`

Every node in the tree (factory-built or parsed) is one `odf.element.Element`; element factories (`Table`, `P`, `Style`, ...) are namespaced constructors keyed by qname, not parallel node classes. `Element` carries `qname`, the W3C-DOM node-type constants (via `Node`), the namespace-prefix resolver (`get_nsprefix`/`get_knownns`), and the `allowed_attributes` grammar that `check_grammar=True` validates writes against. Grammar rejection is a closed two-error rail. Every `[SYMBOL]` is `odf.*`; the tree-link mutators and `firstChild`/`lastChild` traversal are the entrypoints table below.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                                                                                   |
| :-----: | :------------------------ | :-------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `element.Element`         | DOM node        | `qname`/`childNodes`/`parentNode` + attribute map + `allowed_attributes()` grammar       |
|  [02]   | `element.Node`            | node base       | W3C node-type constants (`ELEMENT_NODE`=1/`TEXT_NODE`=3/…/`NOTATION_NODE`=12)            |
|  [03]   | `element.Text`            | text leaf       | `#text` leaf carrying `data`; serialized whitespace-correct by `toXml`                   |
|  [04]   | `element.CDATASection`    | cdata leaf      | CDATA leaf for raw/script/formula payloads                                               |
|  [05]   | `element.Childless`       | leaf mixin      | base for elements that reject children (`appendChild`/`addElement` raise `IllegalChild`) |
|  [06]   | `element.IllegalChild`    | error           | grammar reject when a child qname is not allowed under the parent qname                  |
|  [07]   | `element.IllegalText`     | error           | grammar reject when text is added to a non-text element                                  |
|  [08]   | `teletype.WhitespaceText` | text serializer | whitespace accumulator backing `extractText`/`addTextToElement`                          |

[PUBLIC_TYPE_SCOPE]: data-style, metadata, field, and manifest families
- rail: office — `odf.number` / `odf.dc` / `odf.meta` / `odf.userfield` / `odf.manifest`

The data-style, metadata, and field modules are the depth the office plane grades against as a publication/AEC-documentation engine: `odf.number` owns the data-style number formats bound to cells by `style:name`; `odf.dc` + `odf.meta` author the descriptive `meta.xml` part (the metadata/provenance seal); `odf.userfield.UserFields` is the programmable field-merge engine over an existing document; `odf.manifest` is the package manifest + AES encryption-data DOM. Every `[SYMBOL]` is `odf.*`; the full `odf.number` format tokens, `dc`/`meta` leaves, and `UserFields` methods are the entrypoints tables below.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [RAIL]                                                                                  |
| :-----: | :------------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `number` style factories  | data-style family  | `{Number,Date,Time,Currency,Percentage,Boolean,Text}Style` + format-token children      |
|  [02]   | `dc` factories            | Dublin-Core meta   | `dc:*` leaves (`Title`/`Creator`/`Subject`/`Date`/`Language`) into `doc.meta`           |
|  [03]   | `meta` factories          | ODF meta           | `meta:*` (`CreationDate`/`Generator`/`Keyword`/`UserDefined`/`DocumentStatistic`/…)     |
|  [04]   | `userfield.UserFields`    | field-merge engine | `UserFields(src, dest)` user-field substitution over a source/dest document             |
|  [05]   | `userfield.UserFieldDecl` | field declaration  | `text:user-field-decl` node declaring a named typed field the body references           |
|  [06]   | `manifest.Manifest`       | package manifest   | `META-INF/manifest.xml` DOM; `FileEntry` + `EncryptionData`/`Algorithm`/`KeyDerivation` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document author, embed, and serialize
- rail: office — `odf.opendocument`

A factory mints the `OpenDocument`; authored nodes attach to a content container — `addElement` lives on `Element`, so nodes go through `doc.body.addElement(...)` (or the flavor body alias `doc.spreadsheet`/`doc.text` and the part containers `doc.automaticstyles`/`doc.styles`/`doc.meta`), never `OpenDocument.addElement`; styles register through `automaticstyles`/`styles`; embedded pictures/objects attach through the `addPicture*`/`addObject`/`addThumbnail` family; `save`/`write`/`xml` serialize; the `*xml` accessors expose individual parts as bytes without writing a file. `createElement`/`createTextNode`/`createCDATASection` are the DOM-factory mirror, `getMediaType` returns the document mimetype, and the cache-lifecycle ops keep `getElementsByType` correct after mutation. Surfaces are `OpenDocument.*` (nodes attach through `doc.body.addElement`, never `OpenDocument.addElement`); `addPicture` with `content=None` registers a file by path (mediatype sniffed via `mimetypes`), else registers `content` bytes, returning the `Pictures/<uuid>.<ext>` manifest href.

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]                                                             |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `OpenDocument*` factory                                | `OpenDocumentSpreadsheet() -> OpenDocument`                              |
|  [02]   | `body.addElement`                                      | `doc.body.addElement(element, check_grammar=True)`                       |
|  [03]   | `save`                                                 | `save(outputfile, addsuffix=False)` — path or `"-"`/stdout               |
|  [04]   | `write`                                                | `write(outputfp)` — ZIP to an open binary stream                         |
|  [05]   | `xml`                                                  | `xml() -> bytes` — full packaged document, no disk                       |
|  [06]   | `contentxml` / `stylesxml` / `metaxml` / `settingsxml` | `<part>() -> bytes` per packaged part                                    |
|  [07]   | `toXml`                                                | `toXml(filename='') -> str` — unpackaged flat-XML (`.fodt`)              |
|  [08]   | `getStyleByName`                                       | `getStyleByName(name) -> Element \| None`                                |
|  [09]   | `addPicture`                                           | `addPicture(filename, mediatype=None, content=None) -> str`              |
|  [10]   | `addPictureFromFile`                                   | `addPictureFromFile(filename, mediatype=None) -> str`                    |
|  [11]   | `addPictureFromString`                                 | `addPictureFromString(content, mediatype) -> str`                        |
|  [12]   | `addObject`                                            | `addObject(document, objectname=None) -> str` — returns `Object N/` href |
|  [13]   | `addThumbnail`                                         | `addThumbnail(filecontent=None)` — `Thumbnails/thumbnail.png` part       |
|  [14]   | `create{Element,TextNode,CDATASection}`                | `create<X>(data) -> Element` DOM-factory mirror                          |
|  [15]   | `getMediaType`                                         | `getMediaType() -> str` — the document mimetype                          |
|  [16]   | `build_caches` / `rebuild_caches` / `clear_caches`     | `element_dict` cache lifecycle; rebuild after raw `appendChild`          |

[ENTRYPOINT_SCOPE]: Element write, traverse, and text round-trip
- rail: office — `odf.element` / `odf.teletype`

`Element` is one node that both authors (write API + W3C-DOM mutation) and reads (traverse + attribute get). `check_grammar=True` validates each write against the module's `allowed_attributes`; pass `check_grammar=False` only at a confirmed boundary. `teletype.addTextToElement` is the write-side mirror of `extractText`, splitting a string into `<text:s>`/`<text:tab>`/`<text:line-break>` runs odfpy emits via the `S`/`Tab`/`LineBreak` factories. Surfaces are `Element.*` unless `teletype.*`; write methods take `check_grammar=True`, `setAttribute`/`getAttribute` key on the lowercase dash-stripped local name, and the raw `appendChild`/`insertBefore`/`removeChild` mutators skip the grammar check and need a `rebuild_caches` (`replaceChild` on leaf nodes only).

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE]                                                             |
| :-----: | :---------------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `addElement`                                                | `addElement(element, check_grammar=True)`                                |
|  [02]   | `addText`                                                   | `addText(text)` — rejects on `Childless`/`IllegalText`                   |
|  [03]   | `addCDATA`                                                  | `addCDATA(cdata)` — raw script/formula payload                           |
|  [04]   | `setAttribute`                                              | `setAttribute(attr, value)`                                              |
|  [05]   | `setAttrNS`                                                 | `setAttrNS(namespace, localpart, value)`                                 |
|  [06]   | `appendChild`                                               | `appendChild(node)`                                                      |
|  [07]   | `insertBefore`                                              | `insertBefore(newChild, refChild)`                                       |
|  [08]   | `removeChild`                                               | `removeChild(oldChild)`                                                  |
|  [09]   | `getElementsByType`                                         | `getElementsByType(element) -> list[Element]`                            |
|  [10]   | `getAttribute`                                              | `getAttribute(attr) -> str \| None`                                      |
|  [11]   | `getAttrNS`                                                 | `getAttrNS(namespace, localpart) -> str \| None`                         |
|  [12]   | `removeAttribute` / `removeAttrNS`                          | `removeAttribute(attr)` / `removeAttrNS(namespace, localpart)`           |
|  [13]   | `isInstanceOf`                                              | `isInstanceOf(element) -> bool` — dispatch key for a `childNodes` walk   |
|  [14]   | `hasChildNodes` / `childNodes` / `firstChild` / `lastChild` | document-order traversal links                                           |
|  [15]   | `get_nsprefix` / `get_knownns`                              | `get_nsprefix(namespace)` / `get_knownns(prefix)`                        |
|  [16]   | `toXml`                                                     | `toXml(level, f)` — serialize subtree into writer `f`                    |
|  [17]   | `teletype.extractText`                                      | `extractText(odfElement) -> str` — flatten + unwrap runs                 |
|  [18]   | `teletype.addTextToElement`                                 | `addTextToElement(odfElement, s)` — inject `<text:s>`/`tab`/`line-break` |

[ENTRYPOINT_SCOPE]: namespaced body, table, style, and data-style factory modules
- rail: office — `odf.text` / `odf.table` / `odf.style` / `odf.number` / `odf.draw` / `odf.office`

Each ODF module exposes the element factories for one namespace; the factory is both the authoring constructor (`Table(name=...)`) and the type key for `getElementsByType`/`isInstanceOf`. Spreadsheet cells carry typed value attributes; the merge span is `numberrowsspanned`/`numbercolumnsspanned` while the run-length repeat is `numbercolumnsrepeated`/`numberrowsrepeated`; the `odf.number` style factories build the date/time/currency/percentage/number data-styles bound to cells through `setAttribute('stylename', ...)`. This is the full write member set the `document/emit#DOCUMENT` `ODT`/`ODS` arm composes. Modules are `odf.*`; each factory is `Name(**args) -> Element` (the `**args` are camelCase attribute names the grammar validates), and `easyliststyle` takes `styleFromString(name, specifiers, delim, spacing, showAllLevels)` / `styleFromList(styleName, specArray, spacing, showAllLevels)`.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                                                                     |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `table.Table`                                        | `table:table` sheet node                                                         |
|  [02]   | `table.TableRow` / `TableColumn` / `TableHeaderRows` | `table:table-row` / `-column` / `-header-rows`                                   |
|  [03]   | `table.TableCell`                                    | `table:table-cell` with typed value + run-length repeat + merge-span attributes  |
|  [04]   | `table.CoveredTableCell`                             | `table:covered-table-cell` (placeholder under a merge span)                      |
|  [05]   | `text.P` / `Span` / `H`                              | `text:p` paragraph / `text:span` run / `text:h` heading at `outlinelevel`        |
|  [06]   | `text.List` / `ListItem`                             | `text:list` / `text:list-item` nesting                                           |
|  [07]   | `text.A`                                             | `text:a` hyperlink anchor wrapping run content (`href`, `type='simple'`)         |
|  [08]   | `text.Note` / `NoteCitation` / `NoteBody`            | `text:note` footnote/endnote (`noteclass`) + citation mark + body                |
|  [09]   | `text.S` / `Tab` / `LineBreak` / `SoftPageBreak`     | whitespace + break run nodes `teletype` emits                                    |
|  [10]   | `office.Annotation`                                  | `office:annotation` comment node (author/date + body)                            |
|  [11]   | `style.Style`                                        | auto/named style; `family` `text`/`paragraph`/`table-cell`/`table`/`graphic`     |
|  [12]   | `style.TextProperties` / `ParagraphProperties`       | font/weight/color child + alignment/RTL (`writingmode`) child of a `Style`       |
|  [13]   | `style.{TableCell,Table}Properties`                  | cell-fill/border + table-level children                                          |
|  [14]   | `style.{TableColumn,TableRow,Graphic}Properties`     | column-width, row-height, frame-graphic children                                 |
|  [15]   | `style.PageLayout` / `MasterPage`                    | page geometry + master-page binding for printed egress                           |
|  [16]   | `number.<Kind>Style`                                 | data-style (`Currency`/`Date`/`Time`/… `Style`); tokens bound by `style:name`    |
|  [17]   | `draw.Frame` / `Image`                               | `draw:frame` image/object anchor; `draw:image` refs picture by `addPicture` href |
|  [18]   | `easyliststyle.styleFromString` / `styleFromList`    | build a multi-level `ListStyle` from a compact spec                              |

[ENTRYPOINT_SCOPE]: metadata, field-merge, and parse
- rail: office — `odf.dc` / `odf.meta` / `odf.userfield` / `odf.opendocument`

The metadata factories author the `meta.xml` part for the provenance seal the office plane grades against; `UserFields` is the programmable field-merge engine over a loaded document; `load` is the single parse entry (path or stream), `defusedxml`-hardened.

| [INDEX] | [SURFACE]                          | [CAPABILITY]                                                                                  |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `odf.dc.*` + `doc.meta.addElement` | Dublin-Core descriptive leaves into `meta.xml` via `doc.meta.addElement`                      |
|  [02]   | `odf.meta.*`                       | ODF document-metadata + statistics + custom-property leaves                                   |
|  [03]   | `odf.userfield.UserFields`         | template-merge: `UserFields(src, dest).loaddoc()` then `list_fields`/`get`/`update`/`savedoc` |
|  [04]   | `odf.opendocument.load`            | `load(odffile) -> OpenDocument` — parse `.ods`/`.odt`/`.odp` from path or `BytesIO`           |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_STACK]:
- `document/emit#DOCUMENT` ownership (WRITE): `odfpy` is the single OpenDocument provider on the `document/emit` arm, the pure-Python OASIS sibling beside the OOXML office rows (`python-docx`/`python-pptx`/`openpyxl`/`xlsxwriter`/`docxtpl`). The `DocumentMode.ODT`/`ODS` rows both bind `Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)` — `CORE` because the lowering is pure-Python (`defusedxml`, no lxml C-extension), so the arm crosses `anyio.to_thread.run_sync` under the shared `_OFFLOAD` `CapacityLimiter` (GIL-released, off the loop) rather than `to_process`. `_odf_emit` is a `match`-per-`DocumentNode` LOWERING fold FROM the one `document/model#NODE` tree, never an opaque payload: each `RunNode` authors a `text.Span` over a cached appearance-keyed `style.Style`+`TextProperties` (weight/italic/`fontname`/`fontsize`/`color`/`textposition` from `RunScript`, `fo:language`+`fo:country` from `NodeMeta.lang`); each `SectionNode`/heading lowers to `text.H` at its `outlinelevel`; each `BlockNode` to `text.P`; lists to `text.List`/`text.ListItem`; tables to `table.Table`+`TableHeaderRows`+`TableCell` with the `TableNode.spans` `(row, col, col_span, row_span)` quad onto `numberrowsspanned`/`numbercolumnsspanned` and `CoveredTableCell` placeholders; figures embed through `addPictureFromString` resolved from `EmitSpec.assets`; `AnnotationNode` LINK/NOTE/markup via `text.A`/`text.Note(noteclass='footnote')`/`office.Annotation`; RTL via `style.ParagraphProperties(writingmode='rl-tb')`; and `load(spec.template)` supplies template support. The `ODS` arm lowers the same tree's tabular nodes to `table.Table` with typed `TableCell(valuetype=, value=)` cells and `odf.number` data-styles bound by `style:name`. `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.ODT` fans to THIS arm, so the template pipeline NEVER mints its own `OpenDocument` — odfpy is double-owned by nothing.
- `document/lens#LENS` ownership (READ): `odfpy` is the `LensProvider.ODFPY` core on the recovery arm, the recover-TO inverse of the emit lowering. `LensOp.ODS_READ` (`_ods_arm`) folds `load` -> `getElementsByType(Table|TableRow|TableCell)` -> per-cell `teletype.extractText` with `getAttribute("numbercolumnsrepeated")` run-length expansion into a `TableNode` of `RunNode` cells per sheet. `LensOp.ODT_READ` (`_odt_arm`) folds `load(BytesIO)` then a document-order `body.childNodes` walk dispatching each node by `isInstanceOf(text.H)`/`isInstanceOf(text.P)` — a `text:h` to a `SectionNode` at its `getAttribute("outlinelevel")` level, a `text:p` to a `BlockNode` of `RunNode`, both content-flattened through `teletype.extractText`. The OOXML/binary-Excel formats odfpy's OASIS parser does NOT cover route to `python-calamine` (`XLSX_READ`) / `python-docx` (`DOCX_READ`) instead — odfpy owns only the OASIS container.
- `exchange/detect#DETECT` seam: the `...opendocument...` compound MIME (`...spreadsheet`/`...text`/`...presentation`) routes through the `_MEDIA_CLASS` longest-prefix fold to the document branch and `Container.ZIP`, so a detected ODF buffer reaches the odfpy `load` path with the flavor already resolved — odfpy mints no second sniff.
- universal-tier stacking (`libs/python/.api`): the odf fold rides the shared rails the page composes, never a bespoke odfpy aspect. `anyio.to_thread.run_sync(limiter=_OFFLOAD)` carries the synchronous `OpenDocument` author/`write` and the `load` parse off the event loop onto the `CORE` band; `expression` `Result`/`RuntimeRail` owns the typed-error fold and the `tagged_union` discriminant the emit/lens ops dispatch over; `msgspec` (`Struct`, `convert`) materializes the `EmitSpec`/`LensSpec` payload and the typed-evidence facts; `rasm.runtime.faults.async_boundary("document.emit"/"document.lens", ...)` wraps each op so the odfpy work inherits the `structlog` + OpenTelemetry span and converts an `odf.element.IllegalChild`/`IllegalText` grammar raise (and any `zipfile`/`defusedxml` parse raise) to the runtime `BoundaryFault` envelope without an odfpy-specific try/except; and `numpy` carries no odf payload (text/table nodes are scalar — the RGBA figure buffer is resolved upstream and embedded as bytes through `addPictureFromString`). The grammar `check_grammar=True` is the parse-not-validate boundary: admission is total at the factory, the interior never re-checks.
- receipt seam: each ODF production contributes the EXISTING `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, bytes_)` case via the runtime `ReceiptContributor` port — the same byte-only office case `python-docx`/`python-pptx`/`openpyxl`/`xlsxwriter` mint, keyed by the content key over the serialized `xml()` bytes, the `ReceiptKind.OFFICE` row resolving `lambda key, fact: ArtifactReceipt.Office(key, len(fact.data))` through the emit `_RECEIPT` table under the `@receipted(_REDACTION)` weave. The richer per-document evidence (mimetype, sheet/section names, row/column counts, per-cell value-type, expanded repeat count, embedded-picture hrefs) rides the `EmitFact` carrier the consumer reads off `self.fact` — never a parallel ODF-receipt rail, never the typography-rail `Document` case, and the receipt owner imports no `odf` handle.

[OFFICE_ODF]:
- import: `from odf.opendocument import OpenDocumentSpreadsheet, OpenDocumentText, load`, `from odf.table import Table, TableRow, TableCell, CoveredTableCell, TableHeaderRows`, `from odf.text import P, Span, H, List, ListItem, A, Note, NoteBody, NoteCitation`, `from odf.style import Style, TextProperties, ParagraphProperties, TableCellProperties`, `from odf.number import CurrencyStyle, DateStyle`, `from odf import dc, meta`, `from odf.office import Annotation`, `from odf.teletype import extractText, addTextToElement` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: one `OpenDocument` owns both author and serialize; the ODF flavor is the factory choice (`OpenDocumentSpreadsheet`/`OpenDocumentText`/`OpenDocumentPresentation`/...), never a parallel writer type per flavor; `load` reconstructs the same `OpenDocument` from a path or open binary stream and re-attaches the flavor-matched body alias, so read and write share one tree contract.
- authoring axis: `addElement` keyed by a namespaced factory is the single node-attach surface; `setAttribute`/`setAttrNS` are the attribute-write rows validated against the module's `allowed_attributes` grammar when `check_grammar=True`; styles register once through `automaticstyles`/`styles` and bind to cells/paragraphs/runs by `style:name`, never per-cell or per-run style duplication (the emit arm caches one appearance-keyed `Style` per distinct appearance).
- data-style axis: `odf.number` `DateStyle`/`TimeStyle`/`CurrencyStyle`/`PercentageStyle`/`NumberStyle`/`BooleanStyle` containers with their format-token children own the cell number formats, registered once in `automaticstyles` and bound by `style:name`; a literal pre-formatted string in a `valuetype="string"` cell is the fallback only when no data-style models the format, never a hand-built format string.
- metadata axis: `odf.dc` + `odf.meta` factories author the `meta.xml` part into `doc.meta` (`dc.Title`/`dc.Creator`/`dc.Subject`/`dc.Date`, `meta.Keyword`/`meta.UserDefined`/`meta.DocumentStatistic`), so the descriptive-metadata + provenance seal is in-package; a sidecar XMP write is never substituted where odfpy owns the OASIS meta part.
- field-merge axis: `odf.userfield.UserFields` bound to a loaded document owns programmable field substitution (`list_fields`/`get`/`update`/`savedoc`) — a template `.odt` with declared user-fields is merged in place, never a string-replace over serialized XML.
- text axis: `addTextToElement` owns string-to-node injection (`<text:s>`/`tab`/`line-break` runs) and `extractText` owns the inverse flatten; a manual `childNodes` string join is never substituted because odfpy owns the whitespace contract on both directions.
- traversal axis: on read, `getElementsByType` keyed by an element factory is the single subtree collector and `isInstanceOf` the per-node dispatch key for an ordered `childNodes` walk; sheet/row/cell enumeration are factory rows (`Table`, `TableRow`, `TableCell`), never a hand-written XML walk or a parallel `findRows`/`findCells` family; after a raw `appendChild`/`insertBefore` mutation, `rebuild_caches` restores the `element_dict` so `getElementsByType` stays complete.
- attribute axis: `getAttribute` by lowercase dash-stripped local name owns typed-cell reads; `valuetype`/`value`/`numbercolumnsrepeated`/`numberrowsrepeated` are the run-length attribute rows and `numberrowsspanned`/`numbercolumnsspanned` the merge-span rows, resolved against the cell's allowed-attribute table, with `getAttrNS((namespace, localpart))` the explicit-qname row when the local name is ambiguous; `numbercolumnsrepeated`/`numberrowsrepeated` are read as integer run-lengths and expanded into dense cell positions on ingest, never literal duplicated nodes.
- serialization axis: `save`/`write`/`xml` package the ZIP (`content.xml`/`styles.xml`/`meta.xml`/`settings.xml` + `META-INF/manifest.xml`) exactly once; `contentxml`/`stylesxml`/`metaxml`/`settingsxml` expose individual parts as bytes for an in-memory boundary handoff that never writes a file; `toXml` emits the unpackaged flat-XML form; `defusedxml.sax` backs the `load` parse so the read path is entity-expansion + billion-laughs safe.
- evidence: each authored or ingested document captures the source/target mimetype, sheet/section names, row count, column count, per-cell value-type and value, the expanded repeat count, the descriptive-metadata field tally, and embedded-picture/object hrefs as an office receipt carried on the `EmitFact`/`LensFact` carrier.
- boundary: odfpy owns the full ODF/ODS/ODT/ODP container author+parse, the `Element` DOM, namespaced element factories, the data-style number formats, descriptive-metadata authoring, programmable field-merge, typed-attribute reads, embedded-part attachment, and whitespace-correct text round-trip with no UNO/LibreOffice dependency; OOXML `.xlsx` authoring routes to `xlsxwriter` (streaming) or `openpyxl` (read+write), `.docx` to `python-docx`, `.pptx` to `python-pptx`, binary/OOXML spreadsheet READ to `python-calamine`; the in-memory `xml()`/`contentxml()` bytes feed the artifacts download and export owners directly; the copyleft license gates any redistribution-sensitive bundling distinct from the MIT/BSD sibling owners.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `odfpy`
- Owns: OpenDocument authoring via the `OpenDocument*` factory family, `Element` DOM write+read API, namespaced element factories per ODF module, the `odf.number` data-style family, `odf.dc`/`odf.meta` descriptive-metadata authoring, `odf.userfield.UserFields` field-merge, `odf.easyliststyle` list-style building, grammar-checked attribute validation, embedded picture/object attachment, the `odf.manifest` package + encryption-data DOM, container serialization, ODF/ODS/ODT/ODP parse (path or stream, `defusedxml`-hardened), and whitespace-correct text inject/extract
- Accept: ODF authoring as the `document/emit#DOCUMENT` `DocumentMode.ODT`/`ODS` `_odf_emit` LOWERING arm over the `document/model#NODE` tree (the `document/emit#DOCUMENT` `DocumentPlan.bound` `ODF` target delegating here), feeding the office/download/export owners; and ODF/ODS ingest as the `document/lens#LENS` `LensProvider.ODFPY` `ODS_READ`/`ODT_READ` recovery arm via `load` -> `getElementsByType`/`isInstanceOf` -> `getAttribute`/`extractText`; all under one `async_boundary` on the `CORE` band contributing the single `ArtifactReceipt.Office` case
- Reject: wrapper-renames of the `OpenDocument*` factories/`save`/`load`/`addElement`/`getElementsByType`; a hand-rolled ODF ZIP-plus-XML parser or serializer; a hand-built data-style format string where `odf.number` owns the data-style, or a serialized-XML string-replace where `odf.userfield.UserFields` owns field-merge, or a sidecar XMP write where `odf.dc`/`odf.meta` own the OASIS meta part; a UNO/LibreOffice/ffmpeg-CLI subprocess where the in-package round-trip needs no external process; a second `OpenDocument` mint in `document/emit#DOCUMENT` `DocumentPlan.bound` where the emit arm owns the ODF lowering; a parallel node class or writer type per element kind or document flavor; a manual `childNodes` text join where `extractText`/`addTextToElement` own whitespace unwrapping; per-cell or per-run style duplication where named styles register once; an odfpy-specific try/except where the `async_boundary` converts `IllegalChild`/`IllegalText` to `BoundaryFault`; a second ODF-receipt rail where the single `ArtifactReceipt.Office` case carries the evidence; an OOXML/binary-spreadsheet read where `python-calamine`/`python-docx` own the non-OASIS formats
