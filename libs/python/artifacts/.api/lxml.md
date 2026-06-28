# [PY_ARTIFACTS_API_LXML]

`lxml` supplies the libxml2/libxslt-backed XML and HTML surface for the artifacts structured-documents rail through `lxml.etree` (and the `lxml.html`/`lxml.objectify`/`lxml.builder` submodules): tree-building, tunable parsers, compiled XPath and XSLT with Python extension functions, XML Schema/RelaxNG/Schematron validation, C14N canonicalization, incremental/event parsing (`iterparse`/`iterwalk`/pull parsers), incremental serialization (`xmlfile`/`htmlfile`), and tree-mutation helpers (`indent`/`strip_*`/`cleanup_namespaces`) against the native libxml2 core. The package owner composes `etree.parse`, `etree.Element`/`SubElement`, `etree.XPath`, `etree.XSLT`, `etree.iterparse`, and `etree.xmlfile` into the structured-documents owner; it never re-implements XML parsing the native core already owns. lxml is consumed internally by the OOXML/ODF Office owners (`openpyxl`/`python-docx`/`python-pptx`/`odfpy`) for part parsing and is the XML/XSLT half of the rail where `ruamel.yaml` owns YAML and `tomlkit` owns TOML.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lxml`
- package: `lxml`
- import: `from lxml import etree`
- owner: `artifacts`
- rail: structured documents
- installed: `6.1.1`
- license: BSD-3-Clause (Python bindings); bundled native libxml2 (MIT) and libxslt (MIT)
- entry points: none (library only)
- capability: libxml2/libxslt XML and HTML parse/serialize, element building, compiled XPath, XSLT with Python extension functions and access control, XML Schema/RelaxNG/Schematron/DTD validation, C14N canonicalization, incremental event/pull parsing, incremental serialization, namespace cleanup, and tree-mutation helpers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element, parser, and query roots (`lxml.etree`)
- rail: structured documents

| [INDEX] | [SYMBOL]                                                 | [PACKAGE_ROLE]     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `etree._Element`                                         | element node       | tree node with `xpath`/`find`/`findall`/`iterfind`/`iter`/`getparent`/`getroottree`/`append`/`insert`/`addnext`/`makeelement`/`nsmap`/`sourceline` |
|  [02]   | `etree._ElementTree`                                     | document tree      | rooted document with `write`/`getroot`/`xpath`/`xslt`/`docinfo` |
|  [03]   | `etree.XMLParser` / `etree.HTMLParser`                   | parser             | tunable parser (recover, huge_tree, resolve_entities, schema, target, resolvers) |
|  [04]   | `etree.XMLPullParser` / `etree.HTMLPullParser`           | pull parser        | feed-driven event pull parser (`read_events`)           |
|  [05]   | `etree.XPath` / `etree.ETXPath`                          | compiled query     | reusable XPath expression (`ETXPath` accepts ElementTree-style `{ns}` paths) |
|  [06]   | `etree.XSLT`                                             | compiled transform | reusable XSLT stylesheet (`__call__`, `strparam`, `tostring`) |
|  [07]   | `etree.XSLTExtension` / `etree.XSLTAccessControl`        | XSLT extension     | Python XSLT extension element + read/write/network access policy |
|  [08]   | `etree.FunctionNamespace` / `etree.Extension`            | XPath extension    | register Python callables as XPath/XSLT functions       |
|  [09]   | `etree.XMLSchema` / `etree.RelaxNG` / `etree.Schematron` / `etree.DTD` | validator | schema validation engines (`assertValid`/`validate`/`error_log`) |
|  [10]   | `etree.Resolver` / `etree.PythonElementClassLookup` / `etree.ElementNamespaceClassLookup` | extensibility | custom URI resolver and element-class lookup hooks |
|  [11]   | `etree.QName`                                            | qualified name     | namespace-aware name value object                       |
|  [12]   | `etree.C14NWriterTarget` / `etree.TreeBuilder`           | serialize target   | C14N writer target / SAX-style tree builder target      |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

`LxmlError` is the root; `LxmlSyntaxError` subclasses both `LxmlError` and the stdlib `SyntaxError`.

| [INDEX] | [SYMBOL]                                                          | [PACKAGE_ROLE]   | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `etree.XMLSyntaxError`                                           | parse fault      | malformed XML source              |
|  [02]   | `etree.XPathEvalError` / `etree.XPathSyntaxError`               | query fault      | XPath evaluation/compile failed   |
|  [03]   | `etree.XSLTApplyError` / `etree.XSLTParseError`                 | transform fault  | XSLT application/parse failed     |
|  [04]   | `etree.DocumentInvalid`                                         | validation fault | document failed schema validation |
|  [05]   | `etree.SerialisationError` / `etree.C14NError`                  | serialize fault  | serialization/canonicalization failed |
|  [06]   | `etree.LxmlError`                                              | fault root       | base of the lxml fault hierarchy  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, build, and serialize
- rail: structured documents

Parser and serializer rows share source, parser, base-url, element, namespace, encoding, XML/HTML method, declaration, pretty-print, and comment policy.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                  | [CAPABILITY]                       |
| :-----: | :------------------------- | :--------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `etree.parse`              | `parse(source, parser=None, *, base_url=None)`                               | parse a file/stream into a tree    |
|  [02]   | `etree.fromstring` / `etree.XML` | `fromstring(text, parser=None, *, base_url=None)`                      | parse a string into an element     |
|  [03]   | `etree.HTML`               | `HTML(text, parser=None, *, base_url=None)`                                  | parse an HTML string               |
|  [04]   | `etree.Element` / `etree.SubElement` | tag plus attrib/nsmap (SubElement appends to a parent)             | construct an element / child       |
|  [05]   | `etree.tostring`           | `tostring(element_or_tree, *, encoding=None, method='xml', xml_declaration=None, pretty_print=False, with_tail=True, standalone=None, doctype=None, exclusive=False, inclusive_ns_prefixes=None, with_comments=True)` | serialize a tree |
|  [06]   | `etree.indent`             | `indent(tree, space='  ', *, level=0)`                                       | in-place pretty-print indentation  |
|  [07]   | `etree.canonicalize`       | `canonicalize(xml_data=None, *, out=None, from_file=None, **options)`        | C14N canonical serialization       |
|  [08]   | `etree.cleanup_namespaces` | `cleanup_namespaces(tree_or_element, top_nsmap=None, keep_ns_prefixes=None)` | prune unused namespaces            |
|  [09]   | `etree.strip_tags` / `etree.strip_elements` / `etree.strip_attributes` | tree plus name policy                            | remove tags/elements/attributes in place |
|  [10]   | `etree.XMLParser`          | parser construction policy (recover, huge_tree, resolve_entities, schema, target, no_network) | tunable parser |

[ENTRYPOINT_SCOPE]: incremental parse and serialize
- rail: structured documents

`iterparse`/`iterwalk` drive bounded-memory event streaming over a source or an existing tree; `xmlfile`/`htmlfile` are context-manager incremental writers (`element`/`write`/`flush`) for large output without a full in-memory tree.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                          | [CAPABILITY]                          |
| :-----: | :------------------- | :------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `etree.iterparse`    | `iterparse(source, events=('end',), tag=None, *, huge_tree=, recover=, ...)` | streaming `(event, element)` parse, clear-as-you-go |
|  [02]   | `etree.iterwalk`     | `iterwalk(element_or_tree, events=('end',), tag=None)`               | event walk over an existing tree      |
|  [03]   | `etree.XMLPullParser`| `XMLPullParser(events=, *, tag=, ...)` + `feed`/`read_events`        | feed-driven pull parser               |
|  [04]   | `etree.xmlfile` / `etree.htmlfile` | `xmlfile(output_file, encoding=, compression=, close=, buffered=)` | incremental serialization writer (CM) |

[ENTRYPOINT_SCOPE]: query, transform, and validate
- rail: structured documents

Query rows share namespace, extension-function, regexp, smart-string, access-control, and schema-source policy. `XPath`/`ETXPath` are compiled once and reused with bound variables; `XSLT.strparam` escapes a string XSLT parameter.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                              | [CAPABILITY]                        |
| :-----: | :--------------------------------- | :------------------------------------------------------- | :---------------------------------- |
|  [01]   | `etree.XPath`                      | `XPath(path, *, namespaces=, extensions=, regexp=, smart_strings=)` then `__call__(node, **vars)` | compile/evaluate a reusable XPath |
|  [02]   | `_Element.xpath`                   | `node.xpath(path, namespaces=, extensions=, **vars)`     | one-shot XPath against a node       |
|  [03]   | `etree.XSLT`                       | `XSLT(stylesheet, *, extensions=, access_control=)` then `__call__(tree, **params)` | compile/apply a stylesheet  |
|  [04]   | `etree.FunctionNamespace`          | `FunctionNamespace(ns_uri)[name] = fn`                   | register a Python XPath/XSLT function |
|  [05]   | `etree.XMLSchema` / `etree.RelaxNG` / `etree.Schematron` | schema tree/file source                | build a validator engine            |
|  [06]   | `XMLSchema.assertValid` / `.validate` | tree validation input                                 | validate (raise `DocumentInvalid`) or boolean check + `error_log` |

[ENTRYPOINT_SCOPE]: HTML and builder submodules
- rail: structured documents

`lxml.html` parses lenient HTML into `HtmlElement` trees (with `.text_content()`/`.cssselect()`/link rewriting); `lxml.builder.E`/`ElementMaker` is the functional element-construction factory; `lxml.objectify` exposes attribute-style tree access.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                       | [CAPABILITY]                          |
| :-----: | :--------------------------------- | :------------------------------------------------- | :------------------------------------ |
|  [01]   | `lxml.html.fromstring` / `.document_fromstring` / `.fragment_fromstring` | HTML text plus parser policy | parse HTML to `HtmlElement` (doc vs fragment) |
|  [02]   | `lxml.html.parse` / `.tostring`    | source/element plus serializer policy              | HTML parse/serialize                  |
|  [03]   | `lxml.builder.E` / `lxml.builder.ElementMaker` | `E.tag(attrib_or_children...)` / `ElementMaker(namespace=, nsmap=, typemap=)` | functional element construction |
|  [04]   | `lxml.objectify.fromstring` / `.parse` | XML source                                     | attribute-access objectified tree     |

## [04]-[IMPLEMENTATION_LAW]

[XML_LIBXML2]:
- import: `from lxml import etree` at boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `XMLParser` (or `HTMLParser`) carries every parse knob (recover/huge_tree/resolve_entities/no_network/schema/target/resolvers); a parse modality is a parser row, never a parallel parse function family — and untrusted input sets `resolve_entities=False`/`no_network=True`/`huge_tree=False` on the parser, not at the call site.
- streaming axis: `iterparse`/`iterwalk` (in, clear-as-you-go) and `xmlfile`/`htmlfile` (out, incremental) own the bounded-memory path for large documents; the pull parsers (`XMLPullParser`) own feed-driven event streaming — never a full-tree read where the document exceeds memory.
- query axis: `etree.XPath`/`ETXPath` (compiled, reused, with bound variables and `FunctionNamespace` extensions) is the preferred query surface; `_Element.xpath` is the one-shot row; `_Element.cssselect` requires the separate `cssselect` package.
- transform axis: `etree.XSLT` compiled once and applied is the transform surface, with Python `extensions`/`XSLTExtension`, `XSLTAccessControl`, and `strparam`-escaped parameters; results feed the document owner.
- validation axis: `XMLSchema`/`RelaxNG`/`Schematron`/`DTD` are the validator row set; `validate` returns a boolean with `error_log`, `assertValid` raises `DocumentInvalid` on the structured-documents fault rail.
- canonical axis: `canonicalize`/`C14NWriterTarget` own C14N output; pretty-print is `indent` + `tostring(pretty_print=True)`; namespace pruning is `cleanup_namespaces`; tree trimming is `strip_tags`/`strip_elements`/`strip_attributes` — never a hand-rolled serializer.
- boundary: lxml owns XML/HTML/XSLT/C14N/schema-validation; YAML routes to `ruamel.yaml`, TOML to `tomlkit`; the OOXML/ODF Office owners consume lxml internally for document parts; HTML sanitization/cleaning is NOT in lxml (the `lxml.html.clean` module was split into the separate `lxml_html_clean` / `cssselect` packages, neither admitted — do not cite a sanitize surface as lxml-owned); live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lxml`
- Owns: XML/HTML parse/serialize, element building, compiled XPath with Python extensions, XSLT with extensions/access-control, XML Schema/RelaxNG/Schematron/DTD validation, C14N canonicalization, incremental event/pull parsing, incremental serialization, namespace cleanup, and tree-mutation helpers
