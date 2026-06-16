# [PY_ARTIFACTS_API_LXML]

`lxml` supplies the libxml2/libxslt-backed XML and HTML surface for the artifacts structured-documents rail through the `lxml.etree` module: parse/serialize functions, an element factory, a tunable parser, and the XPath/XSLT compilers that drive document building, query, transform, schema validation, and namespace cleanup against the native libxml2 core. The package owner composes `etree.parse`, `etree.Element`, `etree.XPath`, and `etree.XSLT` into the structured-documents owner; it never re-implements XML parsing the native core already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lxml`
- package: `lxml`
- import: `from lxml import etree`
- owner: `artifacts`
- rail: structured documents
- installed: `6.1.1` reflected via `python -c "from lxml import etree"` on cp315
- entry points: none (library only)
- capability: libxml2/libxslt XML and HTML parse/serialize, element building, XPath, XSLT, XML Schema/RelaxNG/Schematron validation, namespace cleanup, incremental parsing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element, parser, and query roots (`lxml.etree`)
- rail: structured documents

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `etree._Element` | element node | tree node with children/attrib/text/tail access |
| [2] | `etree._ElementTree` | document tree | a rooted document with write/getroot |
| [3] | `etree.XMLParser` | XML parser | tunable parser (recovery, schema, huge_tree, resolvers) |
| [4] | `etree.HTMLParser` | HTML parser | lenient HTML parser |
| [5] | `etree.XPath` | compiled query | a reusable XPath expression |
| [6] | `etree.XSLT` | compiled transform | a reusable XSLT stylesheet |
| [7] | `etree.XMLSchema` / `etree.RelaxNG` / `etree.Schematron` | validator | schema validation engines |
| [8] | `etree.QName` | qualified name | namespace-aware name value object |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `etree.XMLSyntaxError` | parse fault | malformed XML source |
| [2] | `etree.XPathEvalError` | query fault | XPath evaluation failed |
| [3] | `etree.XSLTApplyError` | transform fault | XSLT application failed |
| [4] | `etree.DocumentInvalid` | validation fault | document failed schema validation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, build, and serialize
- rail: structured documents

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `etree.parse` | `parse(source, parser=None, *, base_url=None) -> _ElementTree` | parse a file/stream into a tree |
| [2] | `etree.fromstring` | `fromstring(text, parser=None, *, base_url=None) -> _Element` | parse a string into an element |
| [3] | `etree.XML` | `XML(text, parser=None, *, base_url=None) -> _Element` | parse an XML string literal |
| [4] | `etree.HTML` | `HTML(text, parser=None, *, base_url=None) -> _Element` | parse an HTML string |
| [5] | `etree.Element` | `Element(_tag, attrib=None, nsmap=None, **_extra) -> _Element` | construct an element |
| [6] | `etree.SubElement` | `SubElement(_parent, _tag, attrib=None, nsmap=None, **_extra) -> _Element` | construct and append a child |
| [7] | `etree.tostring` | `tostring(element_or_tree, *, encoding=None, method='xml', xml_declaration=None, pretty_print=False, with_tail=True, standalone=None, doctype=None, exclusive=False, inclusive_ns_prefixes=None, with_comments=True, strip_text=False) -> bytes | str` | serialize a tree |
| [8] | `etree.XMLParser` | `XMLParser(*, encoding=None, recover=False, huge_tree=False, resolve_entities=True, no_network=True, schema=None, remove_blank_text=False, ...) -> XMLParser` | tunable parser |
| [9] | `etree.cleanup_namespaces` | `cleanup_namespaces(tree_or_element, top_nsmap=None, keep_ns_prefixes=None) -> None` | prune unused namespaces |

[ENTRYPOINT_SCOPE]: query, transform, and validate
- rail: structured documents

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `etree.XPath` | `XPath(path, *, namespaces=None, extensions=None, regexp=True, smart_strings=True) -> XPath` | compile a reusable XPath |
| [2] | `_Element.xpath` | `xpath(path, namespaces=None, extensions=None, smart_strings=True, **kwargs) -> list | str | float | bool` | evaluate XPath against a node |
| [3] | `etree.XSLT` | `XSLT(xslt_input, extensions=None, regexp=True, access_control=None) -> XSLT` | compile a stylesheet |
| [4] | `etree.XMLSchema` | `XMLSchema(etree=None, file=None) -> XMLSchema` | build a schema validator |
| [5] | `XMLSchema.assertValid` | `assertValid(etree_or_tree) -> None` | validate or raise `DocumentInvalid` |

## [4]-[IMPLEMENTATION_LAW]

[XML_LIBXML2]:
- import: `from lxml import etree` at boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `XMLParser` (or `HTMLParser`) carries every parse knob (recover/huge_tree/schema/resolvers); a parse modality is a parser row, never a parallel parse function family.
- query axis: `etree.XPath` (compiled, reused) is the preferred query surface; `_Element.xpath` is the one-shot row.
- transform axis: `etree.XSLT` compiled once and applied is the transform surface; results feed the document owner.
- validation axis: `XMLSchema`/`RelaxNG`/`Schematron` are the validator row set; a validation failure raises `DocumentInvalid` on the structured-documents fault rail.
- evidence: each parse/transform captures source size, parser flags, validation result, and output byte length as a structured-documents receipt.
- boundary: lxml owns XML/HTML/XSLT; YAML routes to `ruamel.yaml`, TOML to `tomlkit`; the Office owners consume lxml internally for OOXML parts; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lxml`
- Owns: XML/HTML parse/serialize, element building, XPath, XSLT, schema validation, namespace cleanup
- Accept: native XML/HTML processing feeding the structured-documents and Office owners
- Reject: wrapper-renames of `parse`/`tostring`/`xpath`; a stdlib `xml.etree` fallback where lxml is admitted; identity minting the runtime owns
