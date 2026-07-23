# [PY_ARTIFACTS_API_LXML]

`lxml` owns the libxml2/libxslt-backed XML and HTML surface for the artifacts structured-documents rail: tunable parsing, tree building and mutation, compiled XPath and XSLT with Python extensions, schema/RelaxNG/Schematron/DTD validation, C14N canonicalization, and incremental event/pull parsing over the native core it never re-implements. It is the XML third of the structured-text triad — `ruamel.yaml` owns YAML, `tomlkit` owns TOML — consumed by the OOXML/ODF Office owners and SimpleIDML for document-part parsing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lxml`
- package: `lxml` (BSD-3-Clause)
- module: `lxml.etree`
- namespaces: `lxml.etree`, `lxml.html`, `lxml.objectify`, `lxml.builder`, `lxml.isoschematron`, `lxml.sax`, `lxml.ElementInclude`
- owner: `artifacts`
- rail: structured documents

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element, tree, parser, and query roots (`lxml.etree`)

An XPath text or attribute result is an `_ElementUnicodeResult`, a `str` subclass whose provenance members recover the source node, so a smart-string result round-trips to its origin rather than decaying to bare text (`smart_strings=False` opts out).

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
- [15]-[LOOKUP_FAMILY]: `PythonElementClassLookup`/`ElementNamespaceClassLookup`/`AttributeBasedElementClassLookup`/`ElementDefaultClassLookup`/`ParserBasedElementClassLookup`/`CustomElementClassLookup`/`FallbackElementClassLookup` are the element-class lookup hooks.
- [20]-[SMART_STRING]: `_ElementUnicodeResult` carries `getparent`/`is_text`/`is_tail`/`is_attribute`; each `_LogEntry` carries `message`/`line`/`column`/`domain_name`/`type_name`/`level_name`/`path`/`filename`.

[PUBLIC_TYPE_SCOPE]: faults

`LxmlError` roots the hierarchy; `LxmlSyntaxError` subclasses both it and the stdlib `SyntaxError`. A validator splits faults parse-time (`*ParseError`, building the schema) from validate-time (`*ValidateError`, applying it).

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

`tostring`/`tounicode` carry `encoding`, `method`, `xml_declaration`, `pretty_print`, `with_tail`, `standalone`, `doctype`, `exclusive`, `inclusive_ns_prefixes`, `with_comments`. `set_default_parser` installs a process-wide hardened parser so a bare `parse`/`fromstring` inherits the policy; `register_namespace` fixes a global serialization prefix.

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

`XMLID`/`XMLDTDID`/`parseid` parse while harvesting an `id`->element dictionary (`_IDDict`) for `xml:id` and DTD-declared ID attributes — the declared-ID lookup path.

| [INDEX] | [CALL_SHAPE]                                                        | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `XMLID(text, parser=None, *, base_url=None) -> (element, id_dict)`  | parse + collect an `{id: element}` map (also `XMLDTDID`)        |
|  [02]   | `parseid(source, parser=None, *, base_url=None) -> (tree, id_dict)` | parse a source + collect the ID map                             |
|  [03]   | `adopt_external_document(capsule, parser=None)`                     | adopt a foreign-binding PyCapsule libxml2 doc as `_ElementTree` |

[ENTRYPOINT_SCOPE]: iteration axes

Tag filters accept `*`, a tag name, a `{ns}local` Clark name, or an `etree.Element`/`Comment`/`PI` factory for node-type selection; every axis is lazy.

| [INDEX] | [CALL_SHAPE]                                                                    | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------------ | :---------------------------------------------- |
|  [01]   | `node.iter(*tags)`                                                              | depth-first descendants-or-self stream          |
|  [02]   | `node.iterchildren(*tags, reversed=False)` / `.iterdescendants(*tags)`          | direct children / strict descendants            |
|  [03]   | `node.iterancestors(*tags)` / `.itersiblings(*tags, preceding=False)`           | ancestor walk / sibling walk (either direction) |
|  [04]   | `node.iterfind(path, namespaces=None)` / `node.itertext(*tags, with_tail=True)` | lazy ElementPath find / text-content stream     |

[ENTRYPOINT_SCOPE]: incremental parse and serialize

`iterparse`/`iterwalk` drive bounded-memory event streaming over a source or an existing tree; `xmlfile`/`htmlfile` are context-manager incremental writers (`.element`/`.write`/`.flush`) for large output without a full in-memory tree.

| [INDEX] | [CALL_SHAPE]                                                          | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `iterparse(source, events=('end',), *, tag=None, ...)`                | streaming `(event, element)` parse; `.clear()` to bound memory |
|  [02]   | `iterwalk(element_or_tree, events=('end',), tag=None)`                | event walk over an existing tree                               |
|  [03]   | `XMLPullParser(events=, *, tag=, ...)` + `feed`/`read_events`/`close` | feed-driven pull parser                                        |
|  [04]   | `xmlfile(output_file, ...)` / `htmlfile` (CM writer)                  | incremental serialization writer                               |

[ENTRYPOINT_SCOPE]: query, transform, and validate

`XPath`/`ETXPath` compile once and reuse with bound variables and `FunctionNamespace` extensions; `XSLT.strparam` escapes a string parameter; `_ElementTree.relaxng`/`.xmlschema`/`.xslt` are the one-shot tree-level convenience forms. A validator engine (`XMLSchema`/`RelaxNG`/`Schematron`/`DTD`/`isoschematron.Schematron`) builds from a schema tree or file.

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

`lxml.html` parses lenient HTML to `HtmlElement` trees; `.cssselect`/`find_class` need the separate un-admitted `cssselect` package, so route selection through XPath.

| [INDEX] | [SURFACE]                       | [CAPABILITY]                                                                  |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `lxml.html.fromstring`          | parse HTML to an `HtmlElement` (document vs fragment variants)                |
|  [02]   | `lxml.html.parse` / `.tostring` | HTML parse/serialize + `.text_content`/`.iterlinks` extraction                |
|  [03]   | `lxml.builder.ElementMaker`     | functional element construction (`E`, `lxml.html.builder.E`)                  |
|  [04]   | `lxml.objectify.fromstring`     | typed attribute-access tree + `xsi:type`/`py:pytype` annotation + dotted-path |
|  [05]   | `lxml.isoschematron.Schematron` | full ISO-Schematron (phases, abstract patterns, SVRL report)                  |
|  [06]   | `lxml.sax.saxify`               | drive an element tree into a SAX `ContentHandler` (egress bridge)             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `from lxml import etree` (and `html`/`objectify`/`builder`/`isoschematron`/`sax` for the submodules) at boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `XMLParser`/`HTMLParser` carries every parse knob (`recover`/`huge_tree`/`resolve_entities`/`no_network`/`load_dtd`/`dtd_validation`/`ns_clean`/`remove_blank_text`/`collect_ids`/`schema`/`target`/`resolvers`). Untrusted input hardens with `resolve_entities=False`/`no_network=True`/`huge_tree=False` (the XXE/billion-laughs guard); a best-effort read adds `recover=True` for a partial tree. `set_default_parser` installs the policy process-wide.
- streaming axis: `iterparse`/`iterwalk` (in, caller `.clear()`) and `xmlfile`/`htmlfile` (out) own the bounded-memory path; `XMLPullParser` owns feed-driven streaming over a non-seekable source, never a full-tree read past memory.
- traversal axis: the iteration-axis family (`iter`/`iterchildren`/`iterdescendants`/`iterancestors`/`itersiblings`/`iterfind`/`itertext`) is the single lazy traversal vocabulary, tag-filterable by name/`{ns}local`/node-type factory; the materializing `getchildren()`/`getiterator()` stay unused.
- query axis: `etree.XPath`/`ETXPath` (compiled, reused, bound variables, `FunctionNamespace` extensions) is the query surface and `_Element.xpath` the one-shot; a text/attribute result is an `_ElementUnicodeResult` whose `.getparent()` recovers the source node. `.cssselect` needs the un-admitted `cssselect` — route selection through XPath.
- transform axis: `etree.XSLT` compiled once and applied, with Python `extensions`/`XSLTExtension`, `XSLTAccessControl` (file/network access policy), `set_global_max_depth` recursion bound, and `strparam`-escaped parameters; results are an `_XSLTResultTree` that feeds the document owner.
- validation axis: `XMLSchema`/`RelaxNG`/`DTD` and `isoschematron.Schematron` (the full ISO engine — phases, abstract-pattern expansion, SVRL report — over the partial libxml2 `etree.Schematron`) are the validator set; `validate` returns a bool with structured `error_log`, `assertValid` raises `DocumentInvalid`; each `_LogEntry` carries `line`/`column`/`domain_name`/`type_name`/`level_name`/`message`/`path`.
- canonical axis: `canonicalize`/`C14NWriterTarget` own C14N output (`exclusive`/`inclusive_ns_prefixes`/`with_comments` policy) and `tostring(element, method="c14n2")` the direct Canonical-XML-2.0 byte egress a node's `to_c14n` rides; pretty-print is `indent` + `tostring(pretty_print=True)`, namespace pruning `cleanup_namespaces`, tree trimming `strip_tags`/`strip_elements`/`strip_attributes`.
- diagnostic axis: `error_log`/`_LogEntry` (per-validator, per-parser) is the structured-diagnostic source; `PyErrorLog`/`use_global_python_log` route libxml2's global error stream into Python `logging`/`structlog`.
- boundary: lxml owns XML/HTML/XSLT/C14N/schema validation; YAML routes to `ruamel.yaml`, TOML to `tomlkit`; HTML sanitization is not in lxml (split to the un-admitted `lxml_html_clean`/`cssselect`); the Office owners and SimpleIDML consume lxml for document parts; live UI stays outside.

[STACKING]:
- structured-text triad: `lxml` joins `ruamel.yaml` (`libs/python/artifacts/.api/ruamel-yaml.md`) and `tomlkit` (`libs/python/artifacts/.api/tomlkit.md`) as the three fidelity parsers — route by format, each preserving source structure for in-place edit-and-re-emit.
- validate-then-decode: `etree.fromstring(src, parser=hardened)` -> `XMLSchema(schema).assertValid(tree)` (or `isoschematron.Schematron(...).validate`) gates the document, then a namespaced `tree.xpath(qname, namespaces=ns)` fold projects the typed fields into a `msgspec.Struct`/`pydantic` boundary model (`libs/python/.api/msgspec.md`, `libs/python/.api/pydantic.md`) — the `_Element` tree never crosses the owner boundary.
- namespaced build/emit: `etree.Element(f"{{{ns}}}RDF", nsmap={...})` + `etree.SubElement(parent, f"{{{ns}}}local").text = value` (Clark-notation) then `etree.tostring(root, xml_declaration=True, encoding="utf-8")` feeds `pyvips` `Image.set("xmp-data", ...)` or a `stream-zip` `MemberFile` iterable directly — the XMP-packet and OpenRaster `stack.xml` path in `exchange/metadata` and `export/layered`, never a temp file.
- anchor-resolution: `_Element.xpath(selector)` over SimpleIDML's `IDMLPackage.xml_structure` (`export/indesign`) is the pre-mutation existence gate — an empty result is the missing-anchor signal raised as a typed `KeyError`/`BoundaryFault` before the worker mutates.
- rail-and-effects: parse/validate/transform compose under the `expression` `Result[T, E]` rail (`libs/python/.api/expression.md`); `@beartype` (`libs/python/.api/beartype.md`) guards boundary signatures.
- diagnostics: a structured-documents receipt records the `error_log` row count, element/namespace tally, and output byte length; `etree.PyErrorLog` routes libxml2 errors into `structlog` (`libs/python/.api/structlog.md`).
- retry: a network-touching parse (DTD/XInclude over a URL) wraps `stamina.retry` (`libs/python/runtime/.api/stamina.md`).
- incremental scale: `etree.iterparse(source, tag=qname)` with per-element `.clear()` is the bounded-memory ingest of a large OOXML/IDML part and `etree.xmlfile(out)` the emit — both over the `anyio` streaming lanes (`libs/python/.api/anyio.md`).

[RAIL_LAW]:
- Package: `lxml`
- Owns: XML/HTML parse/serialize, element building, the lazy iteration-axis family, compiled XPath with Python extensions and smart-string provenance, XSLT with extensions/access-control/recursion-bound, XML Schema/RelaxNG/Schematron (ISO + libxml2)/DTD validation with structured `error_log`, DTD-ID-aware parsing, C14N canonicalization, incremental event/pull parsing, incremental serialization, namespace cleanup, tree-mutation helpers, typed `objectify` access, and the SAX egress bridge
- Accept: a hardened-parser parse -> validate -> namespaced-xpath-fold -> `msgspec`/`pydantic` boundary-model decode; a Clark-notation `Element`/`SubElement` build -> `tostring`/`xmlfile` emit feeding a downstream byte sink; `_Element.xpath` anchor resolution against a live structure tree; `iterparse`+`.clear()`/`xmlfile` for bounded-memory large-document I/O; `isoschematron.Schematron` for phase/abstract-pattern Schematron; results on the `expression` `Result` rail with `error_log` rows as positional diagnostics
- Reject: a wrapper-rename of `parse`/`fromstring`/`tostring`; a call-site `resolve_entities`/`no_network`/`recover` flag where the parser owns the policy; a parallel parse-function family where one tunable parser discriminates; a manual `getchildren()` recursion where the iteration-axis family owns traversal; an `xpath("//*[@id=...]")` scan where `XMLID`/`parseid` collect the DTD-ID map; a flattened-message diagnostic where `error_log`/`_LogEntry` carry positional rows; a hand-rolled serializer/pretty-printer where `tostring`/`indent`/`canonicalize` own emission; a `.cssselect`/`lxml.html.clean` citation as lxml-owned; a full-tree read where the document exceeds memory; an `_Element`/`_ElementTree` node crossing the owner boundary uncollapsed
