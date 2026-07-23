# [PY_ARTIFACTS_API_ODFPY]

`odfpy` owns the pure-Python OpenDocument read+write DOM on the artifacts office rail: the `OpenDocument*` factories mint typed OASIS trees, the camelCase `Element` write API authors the namespaced graph against a per-module `allowed_attributes` grammar, and `load` parses any `.ods`/`.odt`/`.odp` container back into the same tree the traversal API walks. `odfpy` keeps the whole OASIS parse-and-serialize in-package with no LibreOffice/UNO bridge, feeding the `document/emit#DOCUMENT` `DocumentMode.ODT`/`ODS` authoring arm and the `document/lens#LENS` `ODS_READ`/`ODT_READ` recovery arm.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odfpy`
- package: `odfpy` (Apache-2.0 OR GPL-2.0-or-later WITH LGPL-option)
- import: `odf`
- owner: `artifacts`
- rail: office
- build: pure-Python, abi-agnostic, no compiled extension
- runtime dep: `defusedxml` — `load` builds its SAX parser through `defusedxml.sax.make_parser`, entity-expansion + billion-laughs safe on the read path
- entry points: library import-only; the shipped console scripts stay unused on the in-process `OpenDocument`/`load` surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document factory roots

`OpenDocument*` factories are the authoring entry: each seeds an `OpenDocument` with the flavor's mimetype and its `body`/`automaticstyles`/`styles`/`masterstyles`/`meta`/`settings`/`scripts`/`fontfacedecls` containers, and `load` reconstructs the same root with its flavor-matched body alias.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                           |
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

Every node is one `odf.element.Element`; the element factories (`Table`, `P`, `Style`) are qname-keyed namespaced constructors, not parallel node classes, and grammar rejection is a closed `IllegalChild`/`IllegalText` rail.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                                             |
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

`odf.number`, `odf.dc`/`odf.meta`, `odf.userfield`, and `odf.manifest` supply the office plane's publication depth: `odf.number` owns cell number formats, `odf.dc`/`odf.meta` the descriptive `meta.xml` part, `odf.userfield.UserFields` field-merge, and `odf.manifest` the package + AES-encryption DOM.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]      | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :----------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `number` style factories  | data-style family  | `{Number,Date,Time,Currency,Percentage,Boolean,Text}Style` + format-token children      |
|  [02]   | `dc` factories            | Dublin-Core meta   | `dc:*` leaves (`Title`/`Creator`/`Subject`/`Date`/`Language`) into `doc.meta`           |
|  [03]   | `meta` factories          | ODF meta           | `meta:*` (`CreationDate`/`Generator`/`Keyword`/`UserDefined`/`DocumentStatistic`/…)     |
|  [04]   | `userfield.UserFields`    | field-merge engine | `UserFields(src, dest)` user-field substitution over a source/dest document             |
|  [05]   | `userfield.UserFieldDecl` | field declaration  | `text:user-field-decl` node declaring a named typed field the body references           |
|  [06]   | `manifest.Manifest`       | package manifest   | `META-INF/manifest.xml` DOM; `FileEntry` + `EncryptionData`/`Algorithm`/`KeyDerivation` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document author, embed, and serialize

A factory mints the `OpenDocument`; nodes attach through `doc.body.addElement` (or a flavor/part-container alias), never `OpenDocument.addElement`. `addPicture` with `content=None` registers a file by path (mediatype sniffed via `mimetypes`), else registers `content` bytes, returning the `Pictures/<uuid>.<ext>` href.

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

`Element` both authors (write API + W3C-DOM mutation) and reads (traverse + attribute get). `addTextToElement` is the write-side mirror of `extractText`, splitting a string into `<text:s>`/`<text:tab>`/`<text:line-break>` runs; the raw `appendChild`/`insertBefore`/`removeChild` mutators skip the grammar check and need a `rebuild_caches` (`replaceChild` on leaf nodes only).

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

Each ODF module exposes the element factories for one namespace; the factory is both the authoring constructor (`Table(name=...)`) and the type key for `getElementsByType`/`isInstanceOf`. Cells carry typed value attributes with `numbercolumnsrepeated`/`numberrowsrepeated` run-length and `numberrowsspanned`/`numbercolumnsspanned` merge spans; each factory is `Name(**args) -> Element` where `**args` are camelCase attribute names the grammar validates.

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

`odf.dc`/`odf.meta` author the `meta.xml` provenance part into `doc.meta`; `UserFields` programs field-merge over a loaded document; `load` is the single parse entry, path or stream, `defusedxml`-hardened.

| [INDEX] | [SURFACE]                          | [CAPABILITY]                                                                                  |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `odf.dc.*` + `doc.meta.addElement` | Dublin-Core descriptive leaves into `meta.xml` via `doc.meta.addElement`                      |
|  [02]   | `odf.meta.*`                       | ODF document-metadata + statistics + custom-property leaves                                   |
|  [03]   | `odf.userfield.UserFields`         | template-merge: `UserFields(src, dest).loaddoc()` then `list_fields`/`get`/`update`/`savedoc` |
|  [04]   | `odf.opendocument.load`            | `load(odffile) -> OpenDocument` — parse `.ods`/`.odt`/`.odp` from path or `BytesIO`           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `OpenDocument` owns author and serialize; the ODF flavor is the factory choice, never a parallel writer per flavor, and `load` reconstructs the same tree from a path or open binary stream so read and write share one contract.
- `check_grammar=True` is the parse-not-validate boundary: `addElement`/`setAttribute`/`setAttrNS` admit against the module's `allowed_attributes` at the factory, the interior never re-checks, and `check_grammar=False` passes only at a confirmed boundary.
- Styles register once through `automaticstyles`/`styles` and bind by `style:name`; the emit arm caches one appearance-keyed `Style` per distinct appearance. `odf.number` data-styles model every cell format, with a literal `valuetype="string"` cell the fallback only when no data-style models the format.
- `getElementsByType` is the single subtree collector and `isInstanceOf` the per-node dispatch key for an ordered `childNodes` walk; a raw `appendChild`/`insertBefore` mutation needs `rebuild_caches` to restore `element_dict`.
- `numbercolumnsrepeated`/`numberrowsrepeated` read as integer run-lengths expanded into dense positions on ingest, never duplicated nodes; `numberrowsspanned`/`numbercolumnsspanned` carry merge spans with `CoveredTableCell` placeholders.
- `extractText`/`addTextToElement` own the whitespace contract in both directions (`<text:s>`/`<text:tab>`/`<text:line-break>` runs).
- `save`/`write`/`xml` package the ZIP (`content.xml`/`styles.xml`/`meta.xml`/`settings.xml` + `META-INF/manifest.xml`) once; `contentxml`/`stylesxml`/`metaxml`/`settingsxml` expose parts as bytes for an in-memory handoff; `toXml` emits unpackaged flat-XML.

[STACKING]:
- `folder:document/emit#DOCUMENT` (WRITE): the single OpenDocument provider beside the OOXML office rows; `DocumentMode.ODT`/`ODS` bind `Backend(Band.CORE, _odf_emit, ReceiptKind.OFFICE)` — `CORE` for the pure-Python lowering crossing `anyio.to_thread.run_sync` under the shared `_OFFLOAD` `CapacityLimiter`. `_odf_emit` is a `match`-per-`DocumentNode` LOWERING fold from the one `document/model#NODE` tree: `RunNode` → `text.Span` over a cached `style.Style`+`TextProperties`, `SectionNode` → `text.H` at `outlinelevel`, `BlockNode` → `text.P`, lists → `text.List`/`ListItem`, tables → `table.Table`+`TableHeaderRows`+`TableCell` with the `TableNode.spans` quad, figures → `addPictureFromString` from `EmitSpec.assets`, `AnnotationNode` → `text.A`/`text.Note`/`office.Annotation`, RTL → `ParagraphProperties(writingmode='rl-tb')`, template → `load(spec.template)`; the `ODS` arm lowers tabular nodes to typed `TableCell(valuetype=, value=)` with `odf.number` styles. `DocumentPlan.bound` `ODT` fans here, so the pipeline mints no second `OpenDocument`.
- `folder:document/lens#LENS` (READ): the `LensProvider.ODFPY` recover-to inverse. `ODS_READ` folds `load` → `getElementsByType(Table|TableRow|TableCell)` → per-cell `extractText` with `numbercolumnsrepeated` run-length expansion into a `TableNode`. `ODT_READ` folds `load(BytesIO)` then a document-order `body.childNodes` walk dispatching `isInstanceOf(text.H)` → `SectionNode` at `outlinelevel` and `isInstanceOf(text.P)` → `BlockNode`, content-flattened through `extractText`; OOXML/binary formats outside the OASIS parser route to `python-calamine`(`XLSX_READ`)/`python-docx`(`DOCX_READ`).
- `folder:exchange/detect#DETECT`: `_MEDIA_CLASS` longest-prefix-folds the `...opendocument...` compound MIME to the document branch and `Container.ZIP`, so a detected ODF buffer reaches `load` with the flavor resolved and odfpy mints no second sniff.
- `folder:core/receipt#RECEIPT`: each production contributes the existing `ArtifactReceipt.Office(key, bytes_)` case via the `ReceiptContributor` port, keyed over the serialized `xml()` bytes through the `ReceiptKind.OFFICE` row under `@receipted(_REDACTION)`; the richer per-document evidence (mimetype, sheet/section names, row/column counts, per-cell value-type, expanded repeat count, embedded-picture hrefs) rides the `EmitFact`/`LensFact` carrier.
- universal tier (`libs/python/.api`): `anyio.to_thread.run_sync(limiter=_OFFLOAD)` carries the sync author/`write`/`load` off-loop; `expression` `Result`/`RuntimeRail` owns the typed-error fold and `tagged_union` discriminant; `msgspec` `Struct`/`convert` materializes the `EmitSpec`/`LensSpec` payload; `rasm.runtime.faults.async_boundary` wraps each op, inheriting the `structlog`/OpenTelemetry span and converting an `IllegalChild`/`IllegalText`/`zipfile`/`defusedxml` raise to `BoundaryFault`.

[LOCAL_ADMISSION]:
- ODF authoring enters as the `document/emit#DOCUMENT` `DocumentMode.ODT`/`ODS` `_odf_emit` LOWERING arm over the `document/model#NODE` tree; ODF/ODS ingest enters as the `document/lens#LENS` `LensProvider.ODFPY` `ODS_READ`/`ODT_READ` recovery arm — both under one `async_boundary` on the `CORE` band contributing the single `ArtifactReceipt.Office` case.

[RAIL_LAW]:
- Package: `odfpy`
- Owns: OpenDocument authoring via the `OpenDocument*` factories, the `Element` DOM write+read API, namespaced element factories per ODF module, the `odf.number` data-style family, `odf.dc`/`odf.meta` descriptive-metadata authoring, `odf.userfield.UserFields` field-merge, `odf.easyliststyle` list-style building, grammar-checked attribute validation, embedded picture/object attachment, the `odf.manifest` package + encryption-data DOM, container serialization, and OASIS parse (path or stream, `defusedxml`-hardened) with whitespace-correct text inject/extract
- Accept: the ODF write and read arms feeding the office/download/export owners, the whole OASIS container round-trip in-package, the in-memory `xml()`/`contentxml()` bytes handed to the download and export owners directly
- Reject: wrapper-renames of the `OpenDocument*` factories / `save`/`load`/`addElement`/`getElementsByType`; a hand-rolled ODF ZIP-plus-XML parser or serializer; a hand-built data-style format string where `odf.number` owns it; a serialized-XML string-replace where `odf.userfield.UserFields` owns field-merge; a sidecar XMP write where `odf.dc`/`odf.meta` own the OASIS meta part; a UNO/LibreOffice subprocess where the in-package round-trip needs no external process; a second `OpenDocument` mint where the emit arm owns the lowering; a parallel node class or writer per element kind or flavor; a manual `childNodes` text join where `extractText`/`addTextToElement` own whitespace; per-cell or per-run style duplication where named styles register once; an odfpy-specific try/except where `async_boundary` converts `IllegalChild`/`IllegalText` to `BoundaryFault`; a second ODF-receipt rail where `ArtifactReceipt.Office` carries the evidence; an OOXML/binary-spreadsheet read where `python-calamine`/`python-docx` own the non-OASIS formats
