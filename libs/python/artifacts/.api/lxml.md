# [PY_ARTIFACTS_API_LXML]

`lxml` supplies the libxml2/libxslt-backed XML and HTML surface for the artifacts structured-documents rail through `lxml.etree` (and the `lxml.html`/`lxml.objectify`/`lxml.builder`/`lxml.isoschematron`/`lxml.sax`/`lxml.ElementInclude` submodules): tree-building, tunable parsers, compiled XPath and XSLT with Python extension functions, XML Schema/RelaxNG/Schematron/DTD validation, C14N canonicalization, incremental/event parsing (`iterparse`/`iterwalk`/pull parsers), incremental serialization (`xmlfile`/`htmlfile`), the full forward/reverse iteration-axis family, structured `error_log` diagnostics, and tree-mutation helpers (`indent`/`strip_*`/`cleanup_namespaces`) against the native libxml2 core. The package owner composes `etree.parse`, `etree.Element`/`SubElement`, `etree.XPath`, `etree.XSLT`, `etree.iterparse`, and `etree.xmlfile` into the structured-documents owner; it never re-implements XML parsing the native core already owns. lxml is consumed internally by the OOXML/ODF Office owners (`openpyxl`/`python-docx`/`python-pptx`/`odfpy`) and SimpleIDML (the `IDMLPackage.xml_structure` tree) for part parsing and is the XML/XSLT third of the structured-text triad where `ruamel.yaml` owns YAML and `tomlkit` owns TOML.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lxml`
- package: `lxml`
- import: `from lxml import etree`
- owner: `artifacts`
- rail: structured documents
- installed: `6.1.1` (libxml2 `2.9.13`, libxslt `1.1.35` bundled; `etree.LIBXML_VERSION`/`etree.LIBXSLT_VERSION` carry the runtime versions, `etree.LIBXML_FEATURES` the compiled feature set)
- license: BSD-3-Clause (Python bindings); bundled native libxml2 (MIT) and libxslt (MIT)
- entry points: none (library only)
- capability: libxml2/libxslt XML and HTML parse/serialize, element building, compiled XPath, XSLT with Python extension functions and access control, XML Schema/RelaxNG/Schematron/DTD validation, C14N canonicalization, incremental event/pull parsing, incremental serialization, the forward/reverse iteration-axis family, DTD-ID-aware parsing, structured per-entry `error_log` diagnostics, attribute-style `objectify` access, namespace cleanup, and tree-mutation helpers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element, tree, parser, and query roots (`lxml.etree`)
- rail: structured documents

`_Element` is the node; `_ElementTree` is the rooted document carrying tree-level validate/transform/path convenience methods; an XPath text result is an `_ElementUnicodeResult` (a `str` subclass with `getparent()`/`is_text`/`is_tail`/`is_attribute` provenance), so a smart-string query result round-trips back to its source node.

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------------------------------ |
|  [01]   | `etree._Element`                                  | element node       | tree node; navigation, mutation, and iteration methods        |
|  [02]   | `etree._ElementTree`                              | document tree      | rooted document; write/validate/transform/path methods        |
|  [03]   | `etree._Attrib`                                   | attribute map      | live ordered attribute mapping on `_Element.attrib`           |
|  [04]   | `etree.DocInfo`                                   | document info      | `_ElementTree.docinfo` doctype/DTD/encoding/version fields    |
|  [05]   | `etree.XMLParser` / `etree.HTMLParser`            | parser             | tunable parser (recover/huge_tree/schema/target)              |
|  [06]   | `etree.XMLPullParser` / `etree.HTMLPullParser`    | pull parser        | feed-driven pull parser (`feed`/`read_events`/`close`)        |
|  [07]   | `etree.XPath` / `etree.ETXPath`                   | compiled query     | reusable compiled XPath (`ETXPath` accepts `{ns}` paths)      |
|  [08]   | `etree.XPathEvaluator`                            | compiled query     | node/document-shape-dispatching XPath factory                 |
|  [09]   | `etree.XSLT`                                      | compiled transform | reusable compiled XSLT stylesheet (`call`/`strparam`)         |
|  [10]   | `etree.XSLTExtension` / `etree.XSLTAccessControl` | XSLT extension     | Python XSLT extension element + access-control policy         |
|  [11]   | `etree.FunctionNamespace` / `etree.Extension`     | XPath extension    | register Python callables as XPath/XSLT functions             |
|  [12]   | `etree.XMLSchema` / `etree.RelaxNG` / `etree.DTD` | validator          | schema validators (`assertValid`/`validate`/`error_log`)      |
|  [13]   | `etree.Schematron` / `isoschematron.Schematron`   | validator          | libxml2-partial vs full ISO-Schematron (phases/patterns/SVRL) |
|  [14]   | `etree.Resolver`                                  | extensibility      | custom URI resolver hook                                      |
|  [15]   | `etree.PythonElementClassLookup` (+ family)       | extensibility      | element-class lookup hooks                                    |
|  [16]   | `etree.QName`                                     | qualified name     | namespace-aware name (`.text`/`.localname`/`.namespace`)      |
|  [17]   | `etree.CDATA` / `etree.Entity`                    | special node       | CDATA wrapper / entity-reference node factory                 |
|  [18]   | `etree.Comment` / `etree.ProcessingInstruction`   | special node       | comment / PI (`PI`) node factory                              |
|  [19]   | `etree.C14NWriterTarget` / `etree.TreeBuilder`    | serialize target   | C14N writer target / SAX-style tree builder target            |
|  [20]   | `etree._ElementUnicodeResult`                     | smart-string       | XPath text/attribute `str` subclass with source provenance    |
|  [21]   | `etree.PyErrorLog` / `etree._LogEntry`            | diagnostics        | per-entry diagnostics; `PyErrorLog` routes to `logging`       |
|  [22]   | `etree._ListErrorLog`                             | diagnostics        | the error-log list a parser/validator exposes                 |

- [01]-[ELEMENT_MEMBERS]: `_Element` carries `xpath`/`find`/`findall`/`iterfind`/`findtext`/the iteration-axis family/`getparent`/`getroottree`/`getnext`/`getprevious`/`append`/`insert`/`extend`/`replace`/`remove`/`addnext`/`addprevious`/`makeelement`/`nsmap`/`prefix`/`base`/`sourceline`.
- [02]-[TREE_MEMBERS]: `_ElementTree` carries `write`/`write_c14n`/`getroot`/`xpath`/`xslt`/`relaxng`/`xmlschema`/`xinclude`/`getpath`/`getelementpath`/`docinfo`.
- [03]-[ATTRIB_MEMBERS]: `_Attrib` carries `get`/`setitem`/`delitem`/`pop`/`update`/`clear`/`has_key`/`items`/`iteritems`; `_Element.set(name, value)` is the convenience setter.
- [04]-[DOCINFO_FIELDS]: `DocInfo` carries `doctype`/`public_id`/`system_url`/`internalDTD`/`externalDTD`/`encoding`/`standalone`/`xml_version`/`root_name`.
- [15]-[LOOKUP_FAMILY]: the element-class lookup hooks are `PythonElementClassLookup`/`ElementNamespaceClassLookup`/`AttributeBasedElementClassLookup`/`ElementDefaultClassLookup`/`ParserBasedElementClassLookup`/`CustomElementClassLookup`/`FallbackElementClassLookup`.
- [20]-[SMART_STRING]: `_ElementUnicodeResult` carries `getparent`/`is_text`/`is_tail`/`is_attribute` provenance; each `_LogEntry` carries `message`/`line`/`column`/`domain_name`/`type_name`/`level_name`/`path`/`filename`.

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

`LxmlError` is the root; `LxmlSyntaxError` subclasses both `LxmlError` and the stdlib `SyntaxError`. Validator faults split parse-time (`*ParseError`, building the schema) from validate-time (`*ValidateError`, applying it).

| [INDEX] | [SYMBOL]                                               | [PACKAGE_ROLE]     | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `etree.XMLSyntaxError`                                 | parse fault        | malformed XML (`.lineno`/`.offset`/`.msg`/`.error_log`) |
|  [02]   | `etree.XPathEvalError` / `etree.XPathSyntaxError`      | query fault        | XPath evaluation/compile failed                         |
|  [03]   | `etree.XSLTApplyError` / `etree.XSLTParseError`        | transform fault    | XSLT application/parse failed                           |
|  [04]   | `etree.DocumentInvalid`                                | validation fault   | document failed schema validation (`assertValid`)       |
|  [05]   | `etree.XMLSchemaParseError` (+ RelaxNG/Schematron/DTD) | schema-build fault | the schema/grammar is malformed (`*ParseError`)         |
|  [06]   | `etree.SerialisationError` / `etree.C14NError`         | serialize fault    | serialization/canonicalization failed                   |
|  [07]   | `etree.ParserError` / `etree.XIncludeError`            | structure fault    | parser-target / XInclude expansion failed               |
|  [08]   | `etree.LxmlError`                                      | fault root         | base of the lxml fault hierarchy                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, build, and serialize
- rail: structured documents

Parser and serializer rows share source, parser, base-url, element, namespace, encoding, XML/HTML method, declaration, pretty-print, and comment policy. The default-parser pair (`get_default_parser`/`set_default_parser`) installs a process-wide hardened parser so a bare `parse`/`fromstring` inherits the policy; `register_namespace` fixes a global prefix for serialization. The wide serializer signatures are `tostring(element_or_tree, *, encoding=None, method='xml', xml_declaration=None, pretty_print=False, with_tail=True, standalone=None, doctype=None, exclusive=False, inclusive_ns_prefixes=None, with_comments=True)`, `tounicode(element_or_tree, *, method='xml', pretty_print=False, with_tail=True, doctype=None)`, and the tree-trim trio `strip_tags(tree, *tag_names)` / `strip_elements(tree, *tag_names, with_tail=True)` / `strip_attributes(tree, *attribute_names)`.

| [INDEX] | [CALL_SHAPE]                                                                         | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `parse(source, parser=None, *, base_url=None)`                                       | parse a file/stream into a tree             |
|  [02]   | `fromstring(text, parser=None, *, base_url=None)` / `XML`                            | parse a string into an element              |
|  [03]   | `fromstringlist(strings, parser=None)`                                               | parse from an iterable of chunks            |
|  [04]   | `HTML(text, parser=None, *, base_url=None)`                                          | parse an HTML string                        |
|  [05]   | `Element(tag, attrib=None, nsmap=None, **extra)` / `SubElement`                      | construct an element / child                |
|  [06]   | `tostring(...)`                                                                      | serialize a tree                            |
|  [07]   | `tounicode(...)` / `tostringlist`                                                    | serialize to a list / to `str`              |
|  [08]   | `indent(tree, space='  ', *, level=0)`                                               | in-place pretty-print indentation           |
|  [09]   | `canonicalize(xml_data=None, *, out=None, from_file=None, ...)`                      | C14N canonical serialization                |
|  [10]   | `cleanup_namespaces(tree_or_element, top_nsmap=None, ...)`                           | prune unused namespaces                     |
|  [11]   | `strip_tags(...)` / `strip_elements(...)` / `strip_attributes(...)`                  | remove tags/elements/attributes in place    |
|  [12]   | `register_namespace(prefix, uri)` / `set_default_parser(...)` / `get_default_parser` | install global prefix / process-wide parser |
|  [13]   | `XMLParser(...)`                                                                     | tunable parser (all parse knobs)            |

[ENTRYPOINT_SCOPE]: DTD-aware parse and IDs
- rail: structured documents

`XMLID`/`XMLDTDID`/`parseid` parse while harvesting an `id`->element dictionary (`_IDDict`), the DTD-ID-keyed lookup path for `xml:id`/DTD-declared ID attributes — never an `xpath("//*[@id=...]")` scan when the source declares IDs.

| [INDEX] | [CALL_SHAPE]                                                        | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `XMLID(text, parser=None, *, base_url=None) -> (element, id_dict)`  | parse + collect an `{id: element}` map (also `XMLDTDID`)        |
|  [02]   | `parseid(source, parser=None, *, base_url=None) -> (tree, id_dict)` | parse a source + collect the ID map                             |
|  [03]   | `adopt_external_document(capsule, parser=None)`                     | adopt a foreign-binding PyCapsule libxml2 doc as `_ElementTree` |

[ENTRYPOINT_SCOPE]: iteration axes
- rail: structured documents

The iteration-axis family is the single traversal vocabulary: forward/reverse, child/descendant/ancestor/sibling, all tag-filterable and lazy — never a manual recursion or a `getchildren()` materialization. Tag filters accept `*`, a tag name, a `{ns}local` Clark name, or `etree.Element`/`Comment`/`PI` factories for node-type selection.

| [INDEX] | [CALL_SHAPE]                                                                    | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------------ | :---------------------------------------------- |
|  [01]   | `node.iter(*tags)`                                                              | depth-first descendants-or-self stream          |
|  [02]   | `node.iterchildren(*tags, reversed=False)` / `.iterdescendants(*tags)`          | direct children / strict descendants            |
|  [03]   | `node.iterancestors(*tags)` / `.itersiblings(*tags, preceding=False)`           | ancestor walk / sibling walk (either direction) |
|  [04]   | `node.iterfind(path, namespaces=None)` / `node.itertext(*tags, with_tail=True)` | lazy ElementPath find / text-content stream     |

[ENTRYPOINT_SCOPE]: incremental parse and serialize
- rail: structured documents

`iterparse`/`iterwalk` drive bounded-memory event streaming over a source or an existing tree; `xmlfile`/`htmlfile` are context-manager incremental writers (`element`/`write`/`flush`/`method`) for large output without a full in-memory tree. The wide signatures are `iterparse(source, events=('end',), *, tag=None, huge_tree=False, recover=False, remove_blank_text=False, schema=None, ...)` and `xmlfile(output_file, *, encoding=None, compression=None, close=False, buffered=True)` then `.element(tag)`/`.write(...)`/`.flush()`.

| [INDEX] | [CALL_SHAPE]                                                          | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `iterparse(source, events=('end',), *, tag=None, ...)`                | streaming `(event, element)` parse; `.clear()` to bound memory |
|  [02]   | `iterwalk(element_or_tree, events=('end',), tag=None)`                | event walk over an existing tree                               |
|  [03]   | `XMLPullParser(events=, *, tag=, ...)` + `feed`/`read_events`/`close` | feed-driven pull parser                                        |
|  [04]   | `xmlfile(output_file, ...)` / `htmlfile` (CM writer)                  | incremental serialization writer                               |

[ENTRYPOINT_SCOPE]: query, transform, and validate
- rail: structured documents

Query rows share namespace, extension-function, regexp, smart-string, access-control, and schema-source policy. `XPath`/`ETXPath` are compiled once and reused with bound variables; `XSLT.strparam` escapes a string XSLT parameter; the tree-level `_ElementTree.relaxng`/`.xmlschema`/`.xslt` are the one-shot convenience validators/transform on a parsed tree. The compiled forms are `XPath(path, *, namespaces=, extensions=, regexp=, smart_strings=)` then `call(node, **vars)` and `XSLT(stylesheet, *, extensions=, access_control=)` then `call(tree, profile_run=False, **params) -> _XSLTResultTree`; the validator engines `XMLSchema`/`RelaxNG`/`Schematron`/`DTD`/`isoschematron.Schematron(etree=, phase=, store_report=, error_finder=)` build from a schema tree/file source.

| [INDEX] | [CALL_SHAPE]                                                             | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `XPath(...)` then `call(node, **vars)`                                   | compile/evaluate a reusable XPath                        |
|  [02]   | `node.xpath(path, namespaces=, extensions=, smart_strings=True, **vars)` | one-shot XPath against a node                            |
|  [03]   | `XSLT(...)` then `call(tree, ...) -> _XSLTResultTree`                    | compile/apply a stylesheet                               |
|  [04]   | `FunctionNamespace(ns_uri)[name] = fn`                                   | register a Python XPath/XSLT function                    |
|  [05]   | `XMLSchema`/`RelaxNG`/`Schematron`/`DTD`/`isoschematron.Schematron(...)` | build a validator engine                                 |
|  [06]   | `_Validator.assertValid` / `.validate` / `.error_log`                    | validate (raise `DocumentInvalid`) or bool + `error_log` |
|  [07]   | `tree.xmlschema(schema_tree)` / `tree.relaxng` / `tree.xinclude()`       | one-shot tree-level validate / XInclude expansion        |

[ENTRYPOINT_SCOPE]: HTML, builder, and objectify submodules
- rail: structured documents

`lxml.html` parses lenient HTML into `HtmlElement` trees via `fromstring`/`document_fromstring`/`fragment_fromstring`/`fragments_fromstring`/`parse`/`tostring` with `.text_content()`/`.iterlinks()`/link rewriting (`.cssselect`/`find_class` need the un-admitted `cssselect`). `lxml.builder.ElementMaker(namespace=, nsmap=, typemap=, makeelement=)`/`E` (and `lxml.html.builder.E`) is the functional element-construction factory. `lxml.objectify` exposes typed attribute-style access via `fromstring`/`parse`/`E`/`DataElement(value, _pytype=...)`/`annotate`/`ObjectPath('a.b.c')`. `lxml.isoschematron.Schematron(etree=, phase=, store_report=, error_finder='//svrl:failed-assert')` then `.validate`/`.error_log`/`.validation_report` is the production ISO-Schematron engine, and `lxml.sax.saxify(element_or_tree, content_handler)`/`ElementTreeContentHandler` bridges a tree to SAX.

| [INDEX] | [SURFACE]                       | [CAPABILITY]                                                                  |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `lxml.html.fromstring`          | parse HTML to an `HtmlElement` (document vs fragment variants)                |
|  [02]   | `lxml.html.parse` / `.tostring` | HTML parse/serialize + `.text_content`/`.iterlinks` extraction                |
|  [03]   | `lxml.builder.ElementMaker`     | functional element construction (`E`, `lxml.html.builder.E`)                  |
|  [04]   | `lxml.objectify.fromstring`     | typed attribute-access tree + `xsi:type`/`py:pytype` annotation + dotted-path |
|  [05]   | `lxml.isoschematron.Schematron` | full ISO-Schematron (phases, abstract patterns, SVRL report)                  |
|  [06]   | `lxml.sax.saxify`               | drive an element tree into a SAX `ContentHandler` (egress bridge)             |

## [04]-[IMPLEMENTATION_LAW]

[XML_LIBXML2]:
- import: `from lxml import etree` (and `from lxml import html, objectify, builder, isoschematron` for the submodules) at boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `XMLParser` (or `HTMLParser`) carries every parse knob (recover/huge_tree/resolve_entities/no_network/load_dtd/dtd_validation/ns_clean/remove_blank_text/collect_ids/schema/target/resolvers); a parse modality is a parser row, never a parallel parse function family. Untrusted input sets `resolve_entities=False`/`no_network=True`/`huge_tree=False` on the parser (the XXE/billion-laughs guard), and a best-effort metadata blob adds `recover=True` so a malformed packet yields a partial tree the reader folds rather than a fault that sinks the whole read — both are parser policy, never a call-site flag. `set_default_parser(hardened)` installs that policy process-wide so a bare `parse`/`fromstring` inherits it.
- streaming axis: `iterparse`/`iterwalk` (in, `.clear()`-as-the caller-go) and `xmlfile`/`htmlfile` (out, incremental) own the bounded-memory path for large documents; the pull parsers (`XMLPullParser`) own feed-driven event streaming over a non-seekable source — never a full-tree read where the document exceeds memory.
- traversal axis: the iteration-axis family (`iter`/`iterchildren`/`iterdescendants`/`iterancestors`/`itersiblings`/`iterfind`/`itertext`) is the single lazy traversal vocabulary, tag-filterable by name/`{ns}local`/node-type factory; `getchildren()`/`getiterator()` are the deprecated materializing forms and are not used.
- query axis: `etree.XPath`/`ETXPath` (compiled, reused, with bound variables and `FunctionNamespace` extensions) is the preferred query surface; `_Element.xpath` is the one-shot row; a text/attribute result is an `_ElementUnicodeResult` whose `.getparent()` recovers the source node, so a smart-string query keeps provenance rather than losing it to a bare `str`. `_Element.cssselect`/`lxml.html` `.cssselect` require the separate un-admitted `cssselect` package — route selection through XPath unless `cssselect` is admitted.
- transform axis: `etree.XSLT` compiled once and applied is the transform surface, with Python `extensions`/`XSLTExtension`, `XSLTAccessControl` (read/write/network/PI policy), `set_global_max_depth` recursion bound, and `strparam`-escaped parameters; results are an `_XSLTResultTree` that feeds the document owner.
- validation axis: `XMLSchema`/`RelaxNG`/`DTD` plus `isoschematron.Schematron` (the full ISO engine with phases, abstract-pattern expansion, and the SVRL report — preferred over the partial libxml2 `etree.Schematron`) are the validator row set; `validate` returns a boolean with structured `error_log`, `assertValid` raises `DocumentInvalid` on the structured-documents fault rail; each `_LogEntry` carries `line`/`column`/`domain_name`/`type_name`/`level_name`/`message`/`path` for a positional diagnostic.
- canonical axis: `canonicalize`/`C14NWriterTarget` own C14N output (with `exclusive`/`inclusive_ns_prefixes`/`with_comments` policy), and `tostring(element, method="c14n2")` is the direct Canonical-XML-2.0 serialization of an in-memory element — the deterministic byte egress `document/model#NODE` `to_c14n` rides; pretty-print is `indent` + `tostring(pretty_print=True)`; namespace pruning is `cleanup_namespaces`; tree trimming is `strip_tags`/`strip_elements`/`strip_attributes` — never a hand-rolled serializer.
- diagnostic axis: `error_log`/`_LogEntry` (per-validator and per-parser) is the structured-diagnostic source; `PyErrorLog`/`use_global_python_log` route libxml2's global error stream into Python `logging`/`structlog` rather than swallowing it — the structured-documents owner reads `error_log` rows, never parses a flattened exception message.
- boundary: lxml owns XML/HTML/XSLT/C14N/schema-validation; YAML routes to `ruamel.yaml`, TOML to `tomlkit`; the OOXML/ODF Office owners and SimpleIDML consume lxml internally for document parts; HTML sanitization/cleaning is NOT in lxml (the `lxml.html.clean` module was split into the separate `lxml_html_clean`/`cssselect` packages, neither admitted — do not cite a sanitize or `.cssselect` surface as lxml-owned); live UI stays outside this package.

[STACKING]:
- structured-text triad: `lxml` (XML round-trip) joins `ruamel.yaml` (YAML round-trip, `libs/python/artifacts/.api/ruamel-yaml.md`) and `tomlkit` (TOML round-trip, `libs/python/artifacts/.api/tomlkit.md`) as the three fidelity parsers of the structured-documents rail — route by format, never cross-parse; each preserves source structure so a rewrite rail edits the loaded tree in place and re-emits.
- validate-then-decode rail: `etree.fromstring(src, parser=hardened)` -> `XMLSchema(schema).assertValid(tree)` (or `isoschematron.Schematron(...).validate(tree)`) gates the document, then a namespaced `tree.xpath(qname, namespaces=ns)` fold projects the typed fields into a `msgspec.Struct`/`pydantic` boundary model (`libs/python/.api/msgspec.md`, `libs/python/.api/pydantic.md`) — schema validation and value validation are two layers over one parse, the `_Element` tree never crossing the owner boundary.
- namespaced build/emit: a metadata/sidecar producer builds with `etree.Element(f"{{{ns}}}RDF", nsmap={...})` + `etree.SubElement(parent, f"{{{ns}}}local").text = value` (Clark-notation tags carrying the namespace) then `etree.tostring(root, xml_declaration=True, encoding="utf-8")` — the XMP-packet and OpenRaster-`stack.xml` authoring path in `exchange/metadata` and `export/layered`, where the emitted bytes feed `pyvips` `Image.set("xmp-data", ...)` or a `stream-zip` `MemberFile` data iterable directly, never an intermediate temp file.
- anchor-resolution: `_Element.xpath(selector)` over a live structure tree (SimpleIDML's `IDMLPackage.xml_structure` in `export/indesign`) is the pre-mutation existence gate — an empty xpath result is the missing-anchor signal raised as a typed `KeyError`/`BoundaryFault` before the heavy worker mutates, never a post-hoc failure mid-fold.
- rail-and-effects: parse/validate/transform compose under the `expression` `Result[T, E]` rail (`libs/python/.api/expression.md`); `@beartype` (`libs/python/.api/beartype.md`) guards boundary signatures.
- diagnostics: a structured-documents receipt records the `error_log` row count, element/namespace tally, and output byte length; `etree.PyErrorLog` routes libxml2 errors into `structlog` (`libs/python/.api/structlog.md`).
- retry: a network-touching parse (DTD/XInclude over a URL, never the untrusted default) wraps `stamina.retry` (`libs/python/runtime/.api/stamina.md`).
- incremental scale: `etree.iterparse(source, tag=qname)` with per-element `.clear()` is the bounded-memory ingest of a large OOXML/IDML part, and `etree.xmlfile(out)` the bounded-memory emit — both compose the `anyio`/streaming lanes (`libs/python/.api/anyio.md`) so a multi-hundred-MB document never materializes a full tree.

[RAIL_LAW]:
- Package: `lxml`
- Owns: XML/HTML parse/serialize, element building, the lazy iteration-axis family, compiled XPath with Python extensions and smart-string provenance, XSLT with extensions/access-control/recursion-bound, XML Schema/RelaxNG/Schematron (ISO + libxml2)/DTD validation with structured `error_log`, DTD-ID-aware parsing, C14N canonicalization, incremental event/pull parsing, incremental serialization, namespace cleanup, tree-mutation helpers, typed `objectify` access, and the SAX egress bridge
- Accept: a hardened-parser parse -> validate -> namespaced-xpath-fold -> `msgspec`/`pydantic` boundary-model decode rail; a Clark-notation `Element`/`SubElement` build -> `tostring`/`xmlfile` emit feeding a downstream byte sink; `_Element.xpath` anchor resolution against a live structure tree; `iterparse`+`.clear()`/`xmlfile` for bounded-memory large-document I/O; `isoschematron.Schematron` for phase/abstract-pattern Schematron; results carried on the `expression` `Result` rail with `error_log` rows as positional diagnostics
- Reject: a wrapper-rename of `parse`/`fromstring`/`tostring`; a call-site `resolve_entities`/`no_network`/`recover` flag where the parser owns the policy; a parallel parse-function family where one tunable parser discriminates; a manual `getchildren()` recursion where the iteration-axis family owns traversal; an `xpath("//*[@id=...]")` scan where `XMLID`/`parseid` collect the DTD-ID map; a flattened-message diagnostic where `error_log`/`_LogEntry` carry positional rows; a hand-rolled serializer/pretty-printer where `tostring`/`indent`/`canonicalize` own emission; a `.cssselect`/`lxml.html.clean` citation as lxml-owned (the split-out un-admitted packages own those); a full-tree read where the document exceeds memory; an `_Element`/`_ElementTree` node crossing the owner boundary uncollapsed
