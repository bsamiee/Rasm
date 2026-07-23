# [PY_ARTIFACTS_API_SIMPLEIDML]

`simpleidml` (import `simple_idml`) owns in-process Adobe InDesign IDML template mutation for the artifacts `export/indesign` rail: one `IDMLPackage` — a `zipfile.ZipFile` subclass over the unzipped `.idml` archive — exposes the whole InDesign structure through a lazy lxml introspection surface and rewrites it through a `@use_working_copy` mutation algebra. This owner authors the editable `.idml` by mutating a designer template along its named XML tag tree, feeding placed content in rather than synthesizing page geometry. Rejected for the host-free engine: the InDesign-Server SOAP path and the native print-package inspector.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `simpleidml`
- package: `simpleidml` (BSD-3-Clause)
- module: `simple_idml`
- owner: `artifacts`
- rail: `export/indesign`
- depends: `lxml` owns the `xml_structure`/`_Element` tag-tree algebra and every designmap/spread/story rewrite, shared with the `document/emit#DOCUMENT` arms; `suds-py3` binds only the rejected InDesign-Server SOAP path, never the in-process mutation rail

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the IDML document root and the lxml tag tree it exposes

`IDMLPackage` is the document root, a `zipfile.ZipFile` subclass constructed over a real archive path; a stream-backed handle leaves `self.filename` `None` and crashes the `@use_working_copy` `os.unlink`. Introspection is lazy — each property memoizes on first read and resets after a mutation — and `xml_structure` is the central authority: an lxml `_Element` reconstructed from `XML/BackingStory.xml`, against which every `at`/`only`/`under` anchor resolves through `_Element.xpath`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                          |
| :-----: | :------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `IDMLPackage`        | document root    | `zipfile.ZipFile` over the unzipped `.idml`; mutation + introspection |
|  [02]   | `xml_structure`      | lxml tag tree    | `@property -> _Element`; every `at`/`only` XPath resolves against it  |
|  [03]   | `xml_structure_tree` | lxml ElementTree | `@property -> ElementTree`; wrapper for absolute-XPath/XSLT use       |
|  [04]   | `designmap`          | designmap node   | `@property`; `designmap.xml` root with `layer_nodes`/`spread_nodes`   |
|  [05]   | `backing_story`      | backing story    | `@property -> BackingStory`; `XML/BackingStory.xml`, structure root   |

[PUBLIC_TYPE_SCOPE]: the import-XML content-control vocabulary (closed string constants)

`import_xml` reads reserved markup-tag names off the source XML to decide per-element content handling, named on the source tree rather than passed as method arguments; `BACKINGSTORY` and `IdPkgNS` are the backing-story path and IDML packaging namespace the structure walk keys on.

| [INDEX] | [SYMBOL]            | [VALUE]                                                  | [EFFECT]                                                |
| :-----: | :------------------ | :------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `SETCONTENT_TAG`    | `"simpleidml-setcontent"`                                | replace the anchor's content with the imported node's   |
|  [02]   | `IGNORECONTENT_TAG` | `"simpleidml-ignorecontent"`                             | import structurally, leave destination content as-is    |
|  [03]   | `FORCECONTENT_TAG`  | `"simpleidml-forcecontent"`                              | force import where the default merge skips a child node |
|  [04]   | `BACKINGSTORY`      | `"XML/BackingStory.xml"`                                 | in-archive backing-story path (`xml_structure` root)    |
|  [05]   | `IdPkgNS`           | `"http://ns.adobe.com/AdobeInDesign/idml/1.0/packaging"` | the IDML packaging XML namespace URI                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the non-mutating reads

`IDMLPackage(filename, mode)` is the single constructor — `mode="r"` opens a designer template, `mode="w"` runs only inside the decorator's repackage.

| [INDEX] | [SURFACE]                                                   | [SHAPE]   | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------------------- | :-------- | :--------------------------------------------------- |
|  [01]   | `IDMLPackage(filename, mode="r", debug=False)`              | ctor      | open the unzipped `.idml` archive over a real path   |
|  [02]   | `@use_working_copy`                                         | decorator | extract, mutate, repackage, swap, reopen fresh       |
|  [03]   | `export_xml(from_tag=None, encoding=None) -> str`           | instance  | serialize the tagged XML tree (inverse `import_xml`) |
|  [04]   | `export_as_tree() -> dict`                                  | instance  | nested `{tag, attrs, content}` dict                  |
|  [05]   | `is_prefixed(prefix) -> bool`                               | instance  | whether already namespace-prefixed (idempotent)      |
|  [06]   | `extras.create_idml_package_from_dir(src_dir, destination)` | factory   | build an `.idml` from a directory tree               |

[ENTRYPOINT_SCOPE]: the `@use_working_copy` mutation algebra (the `IdmlStep`-eligible surface)

Each method returns a fresh `IDMLPackage` (return elided below). Fourteen are `IdmlStep`-eligible; `prefix` applies once on the base and `add_page_from_idml` is the singular the batch `add_pages_from_idml` folds. XPath `at`/source `only`/`under` resolve against `xml_structure`; `remove_layer`/`remove_guides_on_layer`/`add_story_with_content` key the designmap/story files by id.

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | `prefix(prefix)`                                                    | namespace `Self`/ref ids, `^\w+$` (collision-free)     |
|  [02]   | `insert_idml(idml_package, at, only)`                               | merge a sub-template's tagged content at the XPath     |
|  [03]   | `add_pages_from_idml(idml_packages)`                                | batch-fold `(package, page_number, at, only)` tuples   |
|  [04]   | `add_page_from_idml(idml_package, page_number, at, only)`           | append one source page (batch singular)                |
|  [05]   | `import_xml(xml, at)`                                               | populate tagged structure via control tags             |
|  [06]   | `import_pdf(pdf_path, at, crop=…, page_number=1)`                   | place a linked, re-croppable PDF page                  |
|  [07]   | `set_attributes(xpath, items, element_id=None)`                     | write page-item attrs; `href` relinks the image        |
|  [08]   | `add_note(note, author, at, when=None)`                             | attach an InDesign collaboration note                  |
|  [09]   | `merge_layers(with_name=None)`                                      | collapse every designmap layer into one, re-point refs |
|  [10]   | `suffix_layers(suffix)`                                             | append `suffix` to every designmap layer id            |
|  [11]   | `remove_layer(layer_id)`                                            | drop the designmap layer and, transitively, its guides |
|  [12]   | `remove_orphan_layers()`                                            | drop every designmap layer no spread item references   |
|  [13]   | `remove_guides_on_layer(layer_id)`                                  | strip a layer's guides, the layer stays                |
|  [14]   | `remove_content(under)`                                             | recursively clear tagged content under the anchor      |
|  [15]   | `add_story_with_content(story_id, xml_element_id, xml_element_tag)` | mint a new story bound to an XML element               |
|  [16]   | `xml_element_leaf_to_node(xpath, xml_content_ref)`                  | promote a `Rectangle` leaf to a `TextFrame` node       |

[ENTRYPOINT_SCOPE]: the lazy introspection inventory (the `IdmlFact` evidence source)

Lazy memoized properties are the structural inventory the consumer reads off the final instance after the fold; each resets on mutation. A `get_*` query family resolves spread/story objects and page-item ids by xpath/name/id, ridden internally by `set_attributes`/`import_pdf`, never an `isinstance` ladder over the InDesign node classes.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------ | :----------------------------------------- |
|  [01]   | `stories_for_node(node_path) -> list[str]`                    | story files beneath an XML-structure XPath |
|  [02]   | `story_ids_for_node(node_path) -> list[str]`                  | story ids beneath the XPath                |
|  [03]   | `get_spread_object_by_{xpath,name,id}(key) -> Spread \| None` | the `Spread` owning an anchored element    |
|  [04]   | `get_story_object_by_xpath(xpath) -> Story`                   | the `Story` owning an anchored element     |
|  [05]   | `get_element_content_id_by_xpath(xpath) -> str`               | a page-item content id at the XPath        |
|  [06]   | `get_active_layer_name() -> str`                              | the active designmap layer name            |
|  [07]   | `get_layer_id_by_name(name) -> str`                           | a designmap layer id by name               |

| [INDEX] | [SURFACE]                         | [CAPABILITY]                                        |
| :-----: | :-------------------------------- | :-------------------------------------------------- |
|  [01]   | `spreads -> list[str]`            | `Spreads/` member names                             |
|  [02]   | `spreads_objects -> list[Spread]` | Spread objects                                      |
|  [03]   | `last_spread -> Spread`           | the last spread                                     |
|  [04]   | `pages -> list[Page]`             | every page across all spreads (the page-count fact) |
|  [05]   | `stories -> list[str]`            | `Stories/` member names                             |
|  [06]   | `story_ids -> list[str]`          | extracted `Story_<id>` ids                          |
|  [07]   | `tags -> list[_Element]`          | markup elements (deep-copied); `len()` is the fact  |
|  [08]   | `font_families -> list[_Element]` | font elements                                       |
|  [09]   | `style_groups -> list[_Element]`  | style-group elements                                |
|  [10]   | `style -> Style`                  | Resources style object (writable)                   |
|  [11]   | `style_mapping -> StyleMapping`   | Resources style-mapping object (writable)           |
|  [12]   | `graphic -> Graphic`              | Resources graphic object (writable)                 |
|  [13]   | `referenced_layers -> list[str]`  | designmap layer ids a spread item references        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `IDMLPackage` opened over a real filesystem path is the whole surface; every `@use_working_copy` verb returns a fresh instance (extract, rewrite, repackage, `os.unlink(self.filename)`, `shutil.move`, reopen), so the fold threads the returned instance forward and a dropped reference leaks an unclosed backing file the platform cannot unlink. Base open and `prefix` run once; the step fold runs over the one threaded package, never a per-op re-open.
- Every destination `at`/source `only`/`under` is an XPath resolved against the live `xml_structure` `_Element` before the mutation — the `at` against the destination tree, the `only` against the source tree — an empty result being the missing-anchor signal; a layer/story/content id keys the designmap/story files directly and is non-empty-checked, never routed through the XPath resolver that rejects a valid designmap id.
- `prefix` enforces `^\w+$` and namespaces every `Self`/reference id so a merged sub-template cannot collide, applied once on the base and once per source; `is_prefixed` is the idempotence guard and `suffix_layers` the layer-id analogue.
- `import_xml` reads the reserved control tags off the source XML (named on the tree, never method arguments); `import_pdf` writes `pdf_path` verbatim into the page-item `href` and `<Link>` element as a placed linked resource, its `crop` one `PDFCrop` member (default `CropContentVisibleLayers`), the rail supplying a `PdfCrop` `StrEnum` member's `.value` and a `file:` URI off `Path.as_uri()`; `set_attributes` writes page-item attributes and an `href` key relinks the image resource or, empty, removes the page item.

[STACKING]:
- `lxml`(`.api/lxml.md`): `xml_structure` is an lxml `_Element` and every anchor an `_Element.xpath` query — the same lxml the `document/emit#DOCUMENT` arms own, so the IDML mutation fold and the document XML egress share one lxml owner and cross the same `anyio.to_process` worker band, never two parallel XML stacks.
- `expression`(`.api/expression.md`): `IdmlStep` is a fourteen-case `tagged_union` dispatched by one total `match`+`assert_never`, `IdmlFault` (`payload`/`empty_data`/`bad_prefix`/`empty_anchor`/`empty_ref`) the closed `of`-time admission union; the lane maps the worker outcome to `RuntimeRail[ArtifactReceipt]`, the lxml/zipfile `try/except` living only at the boundary as `BoundaryFault`, never the domain pipeline.
- universal rail (`libs/python/.api/`): `Idml`/`IdmlSource`/`IdmlFact` are frozen `msgspec.Struct`s and only the picklable `IdmlFact` (spread/page/story/font/style/layer/tag/node/step counts, byte length) crosses back, never a live `IDMLPackage`/`_Element`; the untrusted template is admitted once at `Idml.of` through an `IndesignPayload` `TypedDict` and a `pydantic` `TypeAdapter` (`ValidationError` maps to `IdmlFault.payload`); `beartype` guards the `PdfCrop` `StrEnum`/`IdmlSource`/`IdmlStep` authoring contract; one `Idml` runs per `anyio.to_process.run_sync` worker under a small `CapacityLimiter`, a process worker (not thread) mandatory for the filesystem-mutating extract/repackage; per-op counts and byte length ride the `structlog`/`opentelemetry` span and `simple_idml.VERSION` rides the receipt as a deployment fact.
- within-lib: `export/indesign#INDESIGN` is the sole consumer — the fourteen step-eligible `@use_working_copy` methods are its `IdmlStep` cases (the base `prefix` and singular `add_page_from_idml` excluded), `composition/compose#COMPOSE` produces the placed SVG/PDF layout `import_xml`/`import_pdf` feed into the template's named structure, and the `pdf_oxide`/`pymupdf` PDF is the page `import_pdf` places as a linked resource; `IdmlSource(data, prefix, at, only)` is the layered `Layer(name, source, bbox)` counterpart, never a shared erased source bag.

[LOCAL_ADMISSION]:
- Pure-Python over `zipfile`+`lxml` with no compiled extension, so the dist binds on cp315 with no `python_version` gate and no source build; `BSD-3-Clause` permissive and commercial-safe, admitted for the `export/indesign` rail.
- Import lazily at boundary scope (`from simple_idml import idml`, then `idml.IDMLPackage(str(path))`); the `simple_idml.indesign`/`commands` SOAP modules and the transitive `suds-py3` are never imported on the rail — a `suds` import signals the rejected server arm leaked in, and a missing import here is a packaging fault, never a host-capability gate.

[RAIL_LAW]:
- Package: `simpleidml`
- Owns: in-process IDML open, lazy introspection, and the `@use_working_copy` mutation algebra over one threaded `IDMLPackage` — namespace prefix/suffix merge, sub-template and page composition, tagged-content population, linked-PDF placement, page-item attribute and `href` relink, editorial notes, designmap-layer surgery, story creation, subtree clear, `Rectangle`-to-`TextFrame` promotion, and tagged-content egress; the entrypoint tables own the member roster.
- Accept: the editable `.idml` authored by mutating a designer template; the `composition/compose#COMPOSE` placed layout fed in via `import_xml`/`import_pdf`; a `pdf_oxide`/`pymupdf` page placed as a linked, re-croppable resource; the `IndesignPayload` admission at `of`; the `PdfCrop`/`IdmlSource` typed rows; the `ArtifactReceipt.Office` byte-count case with the rich `IdmlFact` structural evidence.
- Reject: a `BytesIO`/stream-backed package where the path-backed constructor and the `os.unlink(self.filename)` decorator require a real temp path; a one-handle reassignment dropping the fresh instance each verb returns; a per-op re-open where one threaded package folds the sequence; an unresolved XPath anchor, or a layer/story/content id routed through the XPath resolver where it keys the designmap/story files; a hand-typed crop literal where a `PdfCrop` member's `.value` is the token; raw template bytes into `IdmlSource.data` past the `IndesignPayload` admission; a parallel IDML receipt case where `ArtifactReceipt.Office` is the structured-document-Zip arity; an `import_xml`/`import_pdf` graft onto the layered `ExportTarget` (or a layered arm onto `IdmlStep`); importing the InDesign-Server SOAP `save_as`/`export_package_as` path the host-free engine rejects; re-implementing the IDML Zip/XML serializer, designmap model, or spread/story translation `simple_idml` owns, the placed geometry `composition/compose` owns, the PDF `pdf_oxide`/`pymupdf` own, or the IFC/semantic model `Rasm.Bim` owns.
