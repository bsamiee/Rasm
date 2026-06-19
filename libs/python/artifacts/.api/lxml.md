# [PY_ARTIFACTS_API_LXML]

`lxml` supplies the libxml2/libxslt-backed XML and HTML surface for the artifacts structured-documents rail through the `lxml.etree` module: parse/serialize functions, an element factory, a tunable parser, and the XPath/XSLT compilers that drive document building, query, transform, schema validation, and namespace cleanup against the native libxml2 core. The package owner composes `etree.parse`, `etree.Element`, `etree.XPath`, and `etree.XSLT` into the structured-documents owner; it never re-implements XML parsing the native core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lxml`
- package: `lxml`
- import: `from lxml import etree`
- owner: `artifacts`
- rail: structured documents
- installed: `6.1.1` reflected via `python -c "from lxml import etree"` on the gated `python_version<'3.15'` band (no CPython 3.15 wheel; dispatched onto the runtime subprocess lane, never the cp315 core)
- entry points: none (library only)
- capability: libxml2/libxslt XML and HTML parse/serialize, element building, XPath, XSLT, XML Schema/RelaxNG/Schematron validation, namespace cleanup, incremental parsing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: element, parser, and query roots (`lxml.etree`)
- rail: structured documents

| [INDEX] | [SYMBOL]                                                 | [PACKAGE_ROLE]     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `etree._Element`                                         | element node       | tree node with children/attrib/text/tail access         |
|  [02]   | `etree._ElementTree`                                     | document tree      | a rooted document with write/getroot                    |
|  [03]   | `etree.XMLParser`                                        | XML parser         | tunable parser (recovery, schema, huge_tree, resolvers) |
|  [04]   | `etree.HTMLParser`                                       | HTML parser        | lenient HTML parser                                     |
|  [05]   | `etree.XPath`                                            | compiled query     | a reusable XPath expression                             |
|  [06]   | `etree.XSLT`                                             | compiled transform | a reusable XSLT stylesheet                              |
|  [07]   | `etree.XMLSchema` / `etree.RelaxNG` / `etree.Schematron` | validator          | schema validation engines                               |
|  [08]   | `etree.QName`                                            | qualified name     | namespace-aware name value object                       |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]   | [CAPABILITY]                      |
| :-----: | :---------------------- | :--------------- | :-------------------------------- |
|  [01]   | `etree.XMLSyntaxError`  | parse fault      | malformed XML source              |
|  [02]   | `etree.XPathEvalError`  | query fault      | XPath evaluation failed           |
|  [03]   | `etree.XSLTApplyError`  | transform fault  | XSLT application failed           |
|  [04]   | `etree.DocumentInvalid` | validation fault | document failed schema validation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, build, and serialize
- rail: structured documents

Parser and serializer rows share source, parser, base-url, element, namespace, encoding, XML/HTML method, declaration, pretty-print, and comment policy.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                     | [CAPABILITY]                    |
| :-----: | :------------------------- | :------------------------------- | :------------------------------ |
|  [01]   | `etree.parse`              | source plus parser/base-url      | parse a file/stream into a tree |
|  [02]   | `etree.fromstring`         | text plus parser/base-url        | parse a string into an element  |
|  [03]   | `etree.XML`                | XML text plus parser/base-url    | parse an XML string literal     |
|  [04]   | `etree.HTML`               | HTML text plus parser/base-url   | parse an HTML string            |
|  [05]   | `etree.Element`            | tag plus attributes/namespaces   | construct an element            |
|  [06]   | `etree.SubElement`         | parent plus child element policy | construct and append a child    |
|  [07]   | `etree.tostring`           | tree plus serializer policy      | serialize a tree                |
|  [08]   | `etree.XMLParser`          | parser construction policy       | tunable parser                  |
|  [09]   | `etree.cleanup_namespaces` | tree plus namespace policy       | prune unused namespaces         |

[ENTRYPOINT_SCOPE]: query, transform, and validate
- rail: structured documents

Query rows share namespace, extension, regexp, smart-string, access-control, and schema-source policy.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                | [CAPABILITY]                        |
| :-----: | :---------------------- | :-------------------------- | :---------------------------------- |
|  [01]   | `etree.XPath`           | path plus query policy      | compile a reusable XPath            |
|  [02]   | `_Element.xpath`        | path plus one-shot policy   | evaluate XPath against a node       |
|  [03]   | `etree.XSLT`            | stylesheet plus XSLT policy | compile a stylesheet                |
|  [04]   | `etree.XMLSchema`       | tree or file schema source  | build a schema validator            |
|  [05]   | `XMLSchema.assertValid` | tree validation input       | validate or raise `DocumentInvalid` |

## [04]-[IMPLEMENTATION_LAW]

[XML_LIBXML2]:
- import: `from lxml import etree` at boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `XMLParser` (or `HTMLParser`) carries every parse knob (recover/huge_tree/schema/resolvers); a parse modality is a parser row, never a parallel parse function family.
- query axis: `etree.XPath` (compiled, reused) is the preferred query surface; `_Element.xpath` is the one-shot row.
- transform axis: `etree.XSLT` compiled once and applied is the transform surface; results feed the document owner.
- validation axis: `XMLSchema`/`RelaxNG`/`Schematron` are the validator row set; a validation failure raises `DocumentInvalid` on the structured-documents fault rail.
- evidence: each parse/transform captures source size, parser flags, validation result, and output byte length as a structured-documents receipt.
- boundary: lxml owns XML/HTML/XSLT; YAML routes to `ruamel.yaml`, TOML to `tomlkit`; the Office owners consume lxml internally for OOXML parts; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `lxml`
- Owns: XML/HTML parse/serialize, element building, XPath, XSLT, schema validation, namespace cleanup
- Accept: native XML/HTML processing feeding the structured-documents and Office owners
- Reject: wrapper-renames of `parse`/`tostring`/`xpath`; a stdlib `xml.etree` fallback where lxml is admitted; identity minting the runtime owns
