# [RASM_API_BCL_XML]

`System.Xml` owns every XML ingress and egress the branch crosses: an owned in-memory document, a streaming byte edge, XPath evaluation, XSD validation, XSLT transformation, and object-graph serialization. `XDocument` is the shape a consumer builds, queries, and mutates; `XmlReader`/`XmlWriter` hold the edge a payload too large or too async for a materialized tree crosses, and file or stream egress stays an explicit boundary act.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Xml`
- package: `System.Xml` (MIT)
- assembly: `System.Xml.ReaderWriter.dll`, `System.Xml.XDocument.dll`, `System.Xml.XPath.dll`, `System.Xml.XPath.XDocument.dll`, `System.Xml.XmlSerializer.dll`, `System.Runtime.Serialization.Xml.dll`
- namespace: `System.Xml`, `System.Xml.Linq`, `System.Xml.Schema`, `System.Xml.XPath`, `System.Xml.Xsl`, `System.Xml.Serialization`, `System.Runtime.Serialization`
- rail: document, stream, and object-graph XML codec behind every markup boundary

## [02]-[LINQ_XML]

[LINQ_XML_TYPE_SCOPE]: owned-document family a consumer builds from functional content and queries by axis

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------------- | :------------ | :------------------------------------- |
|  [01]   | `XObject`                    | class         | annotation and change-event root       |
|  [02]   | `XNode`                      | class         | orderable tree node                    |
|  [03]   | `XContainer`                 | class         | child-bearing node                     |
|  [04]   | `XDocument`                  | class         | document root carrying the prologue    |
|  [05]   | `XElement`                   | class         | the queried and mutated element        |
|  [06]   | `XAttribute`                 | class         | name-value pair on an element          |
|  [07]   | `XName`                      | class         | interned expanded name                 |
|  [08]   | `XNamespace`                 | class         | interned namespace composing names     |
|  [09]   | `XStreamingElement`          | class         | deferred-materialization projection    |
|  [10]   | `LoadOptions`                | enum          | whitespace, base-URI, line-info policy |
|  [11]   | `SaveOptions`                | enum          | formatting and duplicate-prefix policy |
|  [12]   | `ReaderOptions`              | enum          | duplicate-prefix policy on the bridge  |
|  [13]   | `XObjectChange`              | enum          | mutation kind on the change event      |
|  [14]   | `XObjectChangeEventArgs`     | class         | mutation payload                       |
|  [15]   | `XNodeEqualityComparer`      | class         | value equality over nodes              |
|  [16]   | `XNodeDocumentOrderComparer` | class         | document-order comparison              |

[LINQ_XML_LEAF_NODE]: `XText` `XCData` `XComment` `XProcessingInstruction` `XDocumentType` `XDeclaration`

[LINQ_XML_ENTRY_SCOPE]: construction, ingress and egress, mutation, the streaming bridge, and the annotation rail

`XElement` mirrors the whole `XDocument` `Load`/`LoadAsync`/`Parse`/`Save`/`SaveAsync` family rooted at a subtree.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `XDocument.Load(Stream, LoadOptions)`                         | static   | parse under load policy             |
|  [02]   | `XDocument.LoadAsync(Stream, LoadOptions, CancellationToken)` | static   | async document ingress              |
|  [03]   | `XDocument.Parse(string, LoadOptions)`                        | static   | parse in-memory markup text         |
|  [04]   | `XDocument.Save(Stream, SaveOptions)`                         | instance | write under formatting policy       |
|  [05]   | `XDocument.SaveAsync(Stream, SaveOptions, CancellationToken)` | instance | async document egress               |
|  [06]   | `XNode.WriteTo(XmlWriter)`                                    | instance | push a node into a writer           |
|  [07]   | `XNode.WriteToAsync(XmlWriter, CancellationToken)`            | instance | async push into a writer            |
|  [08]   | `new XElement(XName, params object?[])`                       | ctor     | build a tree from nested content    |
|  [09]   | `new XStreamingElement(XName, params object?[])`              | ctor     | stream a projection without a tree  |
|  [10]   | `XName.Get(string, string)`                                   | static   | mint a namespaced name              |
|  [11]   | `implicit operator XName(string)`                             | operator | admit an expanded-name literal      |
|  [12]   | `XNamespace.Get(string)`                                      | static   | intern a namespace                  |
|  [13]   | `operator +(XNamespace, string)`                              | operator | compose a namespaced name           |
|  [14]   | `XContainer.Element(XName) -> XElement?`                      | instance | first matching child element        |
|  [15]   | `XElement.Attribute(XName) -> XAttribute?`                    | instance | matching attribute or null          |
|  [16]   | `XElement.SetAttributeValue(XName, object?)`                  | instance | upsert an attribute; null removes   |
|  [17]   | `XElement.SetElementValue(XName, object?)`                    | instance | upsert a child; null removes        |
|  [18]   | `XNode.Remove()`                                              | instance | detach a node from its parent       |
|  [19]   | `Extensions.Remove(IEnumerable<XNode?>)`                      | static   | detach a whole axis result          |
|  [20]   | `Extensions.InDocumentOrder<T>(IEnumerable<T>)`               | static   | restore document order              |
|  [21]   | `XNode.CreateReader(ReaderOptions) -> XmlReader`              | instance | read an owned tree as a stream      |
|  [22]   | `XContainer.CreateWriter() -> XmlWriter`                      | instance | write into an owned tree            |
|  [23]   | `XNode.ReadFrom(XmlReader) -> XNode`                          | static   | build a node at a reader position   |
|  [24]   | `XNode.ReadFromAsync(XmlReader, CancellationToken)`           | static   | async node build                    |
|  [25]   | `XNode.DeepEquals(XNode?, XNode?) -> bool`                    | static   | structural equality                 |
|  [26]   | `XNode.CompareDocumentOrder(XNode?, XNode?) -> int`           | static   | relative document position          |
|  [27]   | `XObject.AddAnnotation(object)`                               | instance | attach out-of-band node state       |
|  [28]   | `XObject.Annotation<T>() -> T?`                               | instance | recover attached state              |
|  [29]   | `XObject.RemoveAnnotations<T>()`                              | instance | drop attached state                 |
|  [30]   | `XObject.Changing` / `XObject.Changed`                        | event    | pre- and post-mutation notification |
|  [31]   | `XElement.GetNamespaceOfPrefix(string) -> XNamespace?`        | instance | resolve a prefix on the live tree   |
|  [32]   | `XElement.GetPrefixOfNamespace(XNamespace) -> string?`        | instance | resolve the prefix inverse          |

[LINQ_XML_AXIS]: `Elements` `Descendants` `DescendantNodes` `Nodes` `Ancestors` `AncestorsAndSelf` `DescendantsAndSelf` `Attributes` `ElementsAfterSelf` `ElementsBeforeSelf` `NodesAfterSelf` `NodesBeforeSelf`

Every axis takes an optional `XName?` filter and carries an `IEnumerable<T>` sequence-level extension mirror on `Extensions`.

[LINQ_XML_MUTATION]: `XElement.SetValue` `XElement.ReplaceAll` `XElement.ReplaceAttributes` `XContainer.ReplaceNodes` `XNode.ReplaceWith`

[LINQ_XML_TYPED_READ]: `bool` `int` `long` `uint` `ulong` `float` `double` `decimal` `Guid` `DateTime` `DateTimeOffset` `TimeSpan` `string`

`explicit operator T(XElement)` parses element text in the invariant culture; each conversion carries a nullable and an `XAttribute` mirror.

[LINQ_XML_PROLOGUE]: `XDocument.Root` `XDocument.Declaration` `XDocument.DocumentType` `XObject.Document` `XObject.Parent` `XObject.BaseUri`

[LINQ_XML_RESERVED]: `XNamespace.None` `XNamespace.Xml` `XNamespace.Xmlns`

## [03]-[READER]

[READER_TYPE_SCOPE]: forward-only pull edge and the settings record fixing its conformance and entity posture

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------- | :------------ | :------------------------------------ |
|  [01]   | `XmlReader`           | class         | abstract forward-only pull parser     |
|  [02]   | `XmlReaderSettings`   | class         | conformance, entity, and quota policy |
|  [03]   | `XmlParserContext`    | class         | namespace and DTD context on ingress  |
|  [04]   | `XmlNameTable`        | class         | atomized-name store shared per parse  |
|  [05]   | `XmlNamespaceManager` | class         | prefix scope stack for query binding  |
|  [06]   | `XmlResolver`         | class         | external-entity resolution seam       |
|  [07]   | `XmlNodeType`         | enum          | positioned-node discriminant          |
|  [08]   | `ConformanceLevel`    | enum          | fragment or document conformance      |
|  [09]   | `DtdProcessing`       | enum          | DTD posture; `Prohibit` is the guard  |
|  [10]   | `XmlConvert`          | class         | invariant lexical-to-CLR conversion   |
|  [11]   | `XmlQualifiedName`    | class         | namespace-qualified name value        |
|  [12]   | `XmlException`        | class         | positioned well-formedness fault      |

[READER_ENTRY_SCOPE]: construction over each source, positioning, typed content reads, and subtree scoping

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `XmlReader.Create(Stream, XmlReaderSettings, XmlParserContext)` | static   | construct over bytes with context |
|  [02]   | `XmlReader.Create(TextReader, XmlReaderSettings)`               | static   | construct over decoded text       |
|  [03]   | `XmlReader.Create(string, XmlReaderSettings)`                   | static   | construct over a URI              |
|  [04]   | `XmlReader.Create(XmlReader, XmlReaderSettings)`                | static   | layer validation onto a reader    |
|  [05]   | `XmlReader.Read() -> bool`                                      | instance | advance one node                  |
|  [06]   | `XmlReader.ReadAsync() -> Task<bool>`                           | instance | async advance                     |
|  [07]   | `XmlReader.MoveToContent() -> XmlNodeType`                      | instance | skip to element or text           |
|  [08]   | `XmlReader.IsStartElement(string, string)`                      | instance | probe the positioned element      |
|  [09]   | `XmlReader.GetAttribute(string, string?)`                       | instance | read an attribute by name         |
|  [10]   | `XmlReader.ReadAttributeValue() -> bool`                        | instance | walk attribute value nodes        |
|  [11]   | `XmlReader.ReadSubtree() -> XmlReader`                          | instance | scope a reader to one element     |
|  [12]   | `XmlReader.ReadToFollowing(string, string)`                     | instance | seek the next named element       |
|  [13]   | `XmlReader.ReadToDescendant(string, string)`                    | instance | seek a named child                |
|  [14]   | `XmlReader.ReadToNextSibling(string, string)`                   | instance | seek a named sibling              |
|  [15]   | `XmlReader.Skip()`                                              | instance | discard the current subtree       |
|  [16]   | `XmlReader.ReadContentAs(Type, IXmlNamespaceResolver?)`         | instance | typed content read                |
|  [17]   | `XmlReader.ReadElementContentAs(Type, IXmlNamespaceResolver)`   | instance | typed element read                |
|  [18]   | `XmlReader.ReadContentAsBase64(byte[], int, int)`               | instance | stream inline binary              |
|  [19]   | `XmlReader.ReadValueChunk(char[], int, int)`                    | instance | stream a large text value         |
|  [20]   | `XmlReader.ReadInnerXml() -> string`                            | instance | markup of the current subtree     |
|  [21]   | `XmlReader.ReadOuterXml() -> string`                            | instance | markup including the current node |
|  [22]   | `XmlReader.Settings -> XmlReaderSettings?`                      | property | settings the reader was built on  |
|  [23]   | `XmlReaderSettings.Clone() -> XmlReaderSettings`                | instance | fork a settings baseline          |

[READER_TYPED_READ]: `ReadContentAsBoolean` `ReadContentAsInt` `ReadContentAsLong` `ReadContentAsFloat` `ReadContentAsDouble` `ReadContentAsDecimal` `ReadContentAsDateTime` `ReadContentAsDateTimeOffset` `ReadContentAsString` `ReadContentAsObject`

Each typed read carries a `ReadElementContentAs*` element form and an async mirror; `CanReadBinaryContent` and `CanReadValueChunk` gate the streaming pair.

[READER_POSITION]: `MoveToElement` `MoveToFirstAttribute` `MoveToNextAttribute` `MoveToAttribute`

[READER_SETTINGS]: `Async` `ConformanceLevel` `DtdProcessing` `XmlResolver` `IgnoreWhitespace` `IgnoreComments` `IgnoreProcessingInstructions` `CheckCharacters` `CloseInput` `MaxCharactersInDocument` `MaxCharactersFromEntities` `LineNumberOffset` `LinePositionOffset` `NameTable` `Schemas` `ValidationType` `ValidationFlags`

## [04]-[WRITER]

[WRITER_TYPE_SCOPE]: forward-only push edge and its formatting record

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------ | :------------ | :--------------------------------------- |
|  [01]   | `XmlWriter`         | class         | abstract push writer; `IAsyncDisposable` |
|  [02]   | `XmlWriterSettings` | class         | indent, newline, encoding policy         |
|  [03]   | `NewLineHandling`   | enum          | replace, entitize, or pass newlines      |
|  [04]   | `NamespaceHandling` | enum          | duplicate-declaration suppression        |
|  [05]   | `XmlOutputMethod`   | enum          | XML, HTML, or text serialization         |

[WRITER_ENTRY_SCOPE]: construction over each sink, the emit surface, and reader-to-writer transcode

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------ | :------- | :-------------------------------- |
|  [01]   | `XmlWriter.Create(Stream, XmlWriterSettings)`                       | static   | byte sink under encoding policy   |
|  [02]   | `XmlWriter.Create(TextWriter, XmlWriterSettings)`                   | static   | text sink                         |
|  [03]   | `XmlWriter.Create(StringBuilder, XmlWriterSettings)`                | static   | in-memory sink                    |
|  [04]   | `XmlWriter.Create(string, XmlWriterSettings)`                       | static   | file sink                         |
|  [05]   | `XmlWriter.Create(XmlWriter, XmlWriterSettings)`                    | static   | layer settings onto a writer      |
|  [06]   | `XmlWriter.WriteStartElement(string?, string, string?)`             | instance | open a prefixed element           |
|  [07]   | `XmlWriter.WriteEndElement()`                                       | instance | close with the shortest form      |
|  [08]   | `XmlWriter.WriteFullEndElement()`                                   | instance | force a non-empty close tag       |
|  [09]   | `XmlWriter.WriteElementString(string?, string, string?, string?)`   | instance | open, write text, close           |
|  [10]   | `XmlWriter.WriteAttributeString(string?, string, string?, string?)` | instance | emit one attribute                |
|  [11]   | `XmlWriter.WriteStartDocument(bool)`                                | instance | emit the declaration              |
|  [12]   | `XmlWriter.WriteEndDocument()`                                      | instance | close every open element          |
|  [13]   | `XmlWriter.WriteCData(string?)`                                     | instance | emit a CDATA section              |
|  [14]   | `XmlWriter.WriteComment(string?)`                                   | instance | emit a comment                    |
|  [15]   | `XmlWriter.WriteProcessingInstruction(string, string?)`             | instance | emit a processing instruction     |
|  [16]   | `XmlWriter.WriteDocType(string, string?, string?, string?)`         | instance | emit the DTD declaration          |
|  [17]   | `XmlWriter.WriteRaw(string)`                                        | instance | emit unescaped markup             |
|  [18]   | `XmlWriter.WriteBase64(byte[], int, int)`                           | instance | inline binary egress              |
|  [19]   | `XmlWriter.WriteValue(object)`                                      | instance | typed value egress                |
|  [20]   | `XmlWriter.WriteNode(XmlReader, bool)`                              | instance | copy a positioned reader subtree  |
|  [21]   | `XmlWriter.WriteNode(XPathNavigator, bool)`                         | instance | copy a navigator subtree          |
|  [22]   | `XmlWriter.WriteAttributes(XmlReader, bool)`                        | instance | copy the positioned attributes    |
|  [23]   | `XmlWriter.Flush()`                                                 | instance | drain buffered output             |
|  [24]   | `XmlWriterSettings.Clone() -> XmlWriterSettings`                    | instance | fork a settings baseline per sink |

Every emit member carries a `*Async` mirror, `WriteNodeAsync` included.

[WRITER_SETTINGS]: `Async` `Indent` `IndentChars` `NewLineChars` `NewLineHandling` `NewLineOnAttributes` `Encoding` `OmitXmlDeclaration` `ConformanceLevel` `NamespaceHandling` `CheckCharacters` `CloseOutput` `WriteEndDocumentOnClose` `DoNotEscapeUriAttributes` `OutputMethod`

## [05]-[XPATH]

[XPATH_TYPE_SCOPE]: expression evaluation over an owned tree and over the immutable navigator store

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------ | :------------ | :---------------------------------- |
|  [01]   | `XPathNavigator`    | class         | editable read cursor over a tree    |
|  [02]   | `XPathDocument`     | class         | immutable navigator-optimized store |
|  [03]   | `XPathExpression`   | class         | pre-compiled expression             |
|  [04]   | `XPathNodeIterator` | class         | lazy node-set cursor                |
|  [05]   | `IXPathNavigable`   | interface     | navigator-source seam               |
|  [06]   | `XPathNodeType`     | enum          | navigator node discriminant         |
|  [07]   | `XmlNodeOrder`      | enum          | relative-position verdict           |
|  [08]   | `XPathException`    | class         | expression compile or eval fault    |

[XPATH_ENTRY_SCOPE]: LINQ-to-XML extension family, the navigator bridge, and compiled evaluation

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `XNode.XPathSelectElement(string, IXmlNamespaceResolver?)`           | static   | one element by expression         |
|  [02]   | `XNode.XPathSelectElements(string, IXmlNamespaceResolver?)`          | static   | element sequence by expression    |
|  [03]   | `XNode.XPathEvaluate(string, IXmlNamespaceResolver?) -> object`      | static   | bool, double, string, or sequence |
|  [04]   | `XNode.CreateNavigator(XmlNameTable?) -> XPathNavigator`             | static   | navigator over an owned tree      |
|  [05]   | `XDocumentExtensions.ToXPathNavigable(XNode)`                        | static   | navigable adapter without a copy  |
|  [06]   | `new XPathDocument(XmlReader, XmlSpace)`                             | ctor     | build the immutable store         |
|  [07]   | `XPathDocument.CreateNavigator() -> XPathNavigator`                  | instance | cursor over the immutable store   |
|  [08]   | `XPathNavigator.Compile(string) -> XPathExpression`                  | instance | pre-compile a reused expression   |
|  [09]   | `XPathNavigator.Evaluate(XPathExpression, XPathNodeIterator?)`       | instance | evaluate a compiled expression    |
|  [10]   | `XPathNavigator.Select(XPathExpression) -> XPathNodeIterator`        | instance | iterate a node-set                |
|  [11]   | `XPathNavigator.SelectSingleNode(XPathExpression)`                   | instance | first node of a node-set          |
|  [12]   | `XPathNavigator.Matches(XPathExpression) -> bool`                    | instance | pattern test at the cursor        |
|  [13]   | `XPathNavigator.CheckValidity(XmlSchemaSet, ValidationEventHandler)` | instance | validate at the cursor            |
|  [14]   | `XPathNavigator.ReadSubtree() -> XmlReader`                          | instance | read the cursor subtree           |
|  [15]   | `XPathNavigator.WriteSubtree(XmlWriter)`                             | instance | copy the cursor subtree           |
|  [16]   | `XPathNavigator.UnderlyingObject -> object?`                         | property | recover the backing node          |

[XPATH_TYPED_VALUE]: `ValueAsBoolean` `ValueAsInt` `ValueAsLong` `ValueAsDouble` `ValueAsDateTime` `TypedValue` `ValueType` `XmlType`

[XPATH_SELECT_AXIS]: `SelectChildren` `SelectAncestors` `SelectDescendants`

## [06]-[SCHEMA]

[SCHEMA_TYPE_SCOPE]: XSD registration, compilation, and the validation-event rail

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :----------------------------- | :------------ | :--------------------------------- |
|  [01]   | `XmlSchemaSet`                 | class         | the compiled schema registry       |
|  [02]   | `XmlSchema`                    | class         | one parsed schema document         |
|  [03]   | `XmlSchemaObject`              | class         | partial-validation anchor          |
|  [04]   | `XmlSchemaObjectTable`         | class         | compiled global declaration table  |
|  [05]   | `XmlSchemaCompilationSettings` | class         | UPA-check posture                  |
|  [06]   | `XmlSchemaValidationFlags`     | enum          | inline schema and location posture |
|  [07]   | `ValidationType`               | enum          | validation mode on a reader        |
|  [08]   | `ValidationEventHandler`       | delegate      | the error and warning sink         |
|  [09]   | `ValidationEventArgs`          | class         | severity and the raised exception  |
|  [10]   | `XmlSeverityType`              | enum          | error or warning verdict           |
|  [11]   | `XmlSchemaException`           | class         | positioned schema fault            |
|  [12]   | `IXmlSchemaInfo`               | interface     | post-schema-validation annotation  |

[SCHEMA_ENTRY_SCOPE]: set management, streaming validation on a reader, and in-place validation of an owned tree

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `XmlSchemaSet.Add(string?, XmlReader) -> XmlSchema?`                                    | instance | register a schema from a reader |
|  [02]   | `XmlSchemaSet.Add(XmlSchemaSet)`                                                        | instance | merge another set               |
|  [03]   | `XmlSchemaSet.Compile()`                                                                | instance | compile the registered set      |
|  [04]   | `XmlSchemaSet.Reprocess(XmlSchema) -> XmlSchema`                                        | instance | recompile one member            |
|  [05]   | `XmlSchemaSet.RemoveRecursive(XmlSchema) -> bool`                                       | instance | drop a schema with its imports  |
|  [06]   | `XmlSchemaSet.Schemas(string?) -> ICollection`                                          | instance | enumerate by target namespace   |
|  [07]   | `XmlSchemaSet.CompilationSettings`                                                      | property | UPA-check posture               |
|  [08]   | `XmlSchemaSet.GlobalElements -> XmlSchemaObjectTable`                                   | property | compiled global element table   |
|  [09]   | `XmlSchemaSet.XmlResolver`                                                              | property | include and import resolution   |
|  [10]   | `XmlSchemaSet.ValidationEventHandler`                                                   | event    | load and validation errors      |
|  [11]   | `Extensions.Validate(XDocument, XmlSchemaSet, ValidationEventHandler?, bool)`           | static   | validate an owned tree in place |
|  [12]   | `Extensions.Validate(XElement, XmlSchemaObject, XmlSchemaSet, ValidationEventHandler?)` | static   | partial validation of a subtree |
|  [13]   | `Extensions.GetSchemaInfo(XElement) -> IXmlSchemaInfo?`                                 | static   | read post-validation annotation |

- `Extensions.Validate`: `addSchemaInfo` is what makes `GetSchemaInfo` return an annotation.

[SCHEMA_READER_BINDING]: `XmlReaderSettings.Schemas` `ValidationType.Schema` `XmlReaderSettings.ValidationFlags` `XmlReaderSettings.ValidationEventHandler`

[SCHEMA_VALIDATION_FLAG]: `ReportValidationWarnings` `ProcessSchemaLocation` `ProcessInlineSchema` `ProcessIdentityConstraints` `AllowXmlAttributes`

## [07]-[XSLT]

[XSLT_TYPE_SCOPE]: XSLT 1.0 compilation, argument binding, and the trusted-feature gate

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :-------------------------------- | :------------ | :------------------------------ |
|  [01]   | `XslCompiledTransform`            | class         | compiled stylesheet and its run |
|  [02]   | `XsltArgumentList`                | class         | parameter and extension binding |
|  [03]   | `XsltSettings`                    | class         | trusted-feature gate            |
|  [04]   | `XsltMessageEncounteredEventArgs` | class         | `xsl:message` payload           |
|  [05]   | `XsltCompileException`            | class         | stylesheet compile fault        |
|  [06]   | `XsltException`                   | class         | transform-time fault            |

[XSLT_ENTRY_SCOPE]: stylesheet compilation, transform invocation, and output-settings recovery

| [INDEX] | [SURFACE]                                                                                     | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `XslCompiledTransform.Load(XmlReader, XsltSettings?, XmlResolver?)`                           | instance | compile from a reader        |
|  [02]   | `XslCompiledTransform.Load(IXPathNavigable, XsltSettings?, XmlResolver?)`                     | instance | compile from a navigable     |
|  [03]   | `XslCompiledTransform.Load(string, XsltSettings?, XmlResolver?)`                              | instance | compile from a URI           |
|  [04]   | `XslCompiledTransform.Transform(XmlReader, XsltArgumentList?, XmlWriter)`                     | instance | run into a writer            |
|  [05]   | `XslCompiledTransform.Transform(IXPathNavigable, XsltArgumentList?, XmlWriter, XmlResolver?)` | instance | run with a resolver          |
|  [06]   | `XslCompiledTransform.OutputSettings -> XmlWriterSettings?`                                   | property | declared output settings     |
|  [07]   | `XsltArgumentList.AddParam(string, string, object)`                                           | instance | bind a stylesheet parameter  |
|  [08]   | `XsltArgumentList.AddExtensionObject(string, object)`                                         | instance | bind an extension object     |
|  [09]   | `XsltArgumentList.XsltMessageEncountered`                                                     | event    | capture `xsl:message` output |
|  [10]   | `XsltSettings.Default -> XsltSettings`                                                        | property | document function locked     |
|  [11]   | `XsltSettings.TrustedXslt -> XsltSettings`                                                    | property | trusted-stylesheet posture   |
|  [12]   | `XsltSettings.EnableDocumentFunction -> bool`                                                 | property | admit the document function  |

- `XslCompiledTransform.Transform`: build the result `XmlWriter` from `OutputSettings` so the stylesheet's own `xsl:output` binds.

## [08]-[SERIALIZATION]

[SERIALIZATION_TYPE_SCOPE]: object-graph boundaries — attribute-driven public members, opt-in contracts, and the binary-XML wire

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `XmlSerializer`                  | class         | attribute-driven public-member codec |
|  [02]   | `XmlSerializerNamespaces`        | class         | emitted prefix control               |
|  [03]   | `XmlAttributeOverrides`          | class         | out-of-band attribute remapping      |
|  [04]   | `XmlRootAttribute`               | attribute     | root name and namespace override     |
|  [05]   | `XmlDeserializationEvents`       | struct        | unknown-member callback set          |
|  [06]   | `DataContractSerializer`         | class         | opt-in contract codec                |
|  [07]   | `DataContractSerializerSettings` | class         | root, known types, graph caps        |
|  [08]   | `DataContractResolver`           | class         | polymorphic type resolution seam     |
|  [09]   | `XmlDictionaryWriter`            | class         | binary and text dictionary egress    |
|  [10]   | `XmlDictionaryReader`            | class         | binary and text dictionary ingress   |
|  [11]   | `XmlDictionaryReaderQuotas`      | class         | untrusted-input size caps            |
|  [12]   | `IXmlDictionary`                 | interface     | shared string table for the wire     |

[SERIALIZATION_ENTRY_SCOPE]: serializer construction, the read and write pair, and the compact binary wire

| [INDEX] | [SURFACE]                                                                                        | [SHAPE]  | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------------------------- | :------- | :----------------------- |
|  [01]   | `new XmlSerializer(Type, XmlRootAttribute?)`                                                     | ctor     | root-overridden codec    |
|  [02]   | `new XmlSerializer(Type, Type[]?)`                                                               | ctor     | admit polymorphic types  |
|  [03]   | `XmlSerializer.Serialize(XmlWriter, object?, XmlSerializerNamespaces?)`                          | instance | write with prefixes      |
|  [04]   | `XmlSerializer.Deserialize(XmlReader, XmlDeserializationEvents)`                                 | instance | read with callbacks      |
|  [05]   | `XmlSerializer.CanDeserialize(XmlReader) -> bool`                                                | instance | probe the root           |
|  [06]   | `new DataContractSerializer(Type, DataContractSerializerSettings?)`                              | ctor     | contract codec with caps |
|  [07]   | `DataContractSerializer.WriteObject(XmlDictionaryWriter, object?, DataContractResolver?)`        | instance | write with resolution    |
|  [08]   | `DataContractSerializer.ReadObject(XmlDictionaryReader, bool, DataContractResolver?)`            | instance | read with resolution     |
|  [09]   | `DataContractSerializer.IsStartObject(XmlReader) -> bool`                                        | instance | probe the contract root  |
|  [10]   | `XmlDictionaryWriter.CreateBinaryWriter(Stream, IXmlDictionary?, XmlBinaryWriterSession?, bool)` | static   | compact binary egress    |
|  [11]   | `XmlDictionaryReader.CreateBinaryReader(Stream, IXmlDictionary?, XmlDictionaryReaderQuotas)`     | static   | compact binary ingress   |
|  [12]   | `XmlDictionaryWriter.CreateTextWriter(Stream, Encoding, bool)`                                   | static   | text egress on the rail  |
|  [13]   | `XmlDictionaryReader.CreateDictionaryReader(XmlReader)`                                          | static   | lift onto the rail       |

- `new XmlSerializer(Type)`: construction emits an assembly, so one instance caches per `Type`.

[SERIALIZATION_QUOTA]: `MaxDepth` `MaxStringContentLength` `MaxArrayLength` `MaxBytesPerRead` `MaxNameTableCharCount`

## [09]-[DOM_INTEROP]

[DOM_INTEROP_TYPE_SCOPE]: mutable DOM a host API hands back

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :------------------------ | :------------ | :----------------------------- |
|  [01]   | `XmlDocument`             | class         | mutable DOM root               |
|  [02]   | `XmlNode`                 | class         | DOM node with XPath query      |
|  [03]   | `XmlElement`              | class         | DOM element                    |
|  [04]   | `XmlAttribute`            | class         | DOM attribute                  |
|  [05]   | `XmlNodeList`             | class         | DOM query result               |
|  [06]   | `XmlNodeReader`           | class         | pull reader over a DOM subtree |
|  [07]   | `XmlNodeChangedEventArgs` | class         | DOM mutation payload           |

[DOM_INTEROP_ENTRY_SCOPE]: egress, query, and bridges onto the owned and streaming surfaces

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `XmlDocument.Save(string)`                              | instance | write the DOM to a file          |
|  [02]   | `XmlDocument.Save(XmlWriter)`                           | instance | write the DOM into a writer      |
|  [03]   | `XmlDocument.Load(XmlReader)`                           | instance | build the DOM from a reader      |
|  [04]   | `XmlNode.SelectSingleNode(string, XmlNamespaceManager)` | instance | XPath query with prefix bindings |
|  [05]   | `XmlNode.SelectNodes(string, XmlNamespaceManager)`      | instance | XPath node-list query            |
|  [06]   | `XmlNode.CreateNavigator() -> XPathNavigator?`          | instance | navigator over a DOM node        |
|  [07]   | `XmlNode.WriteTo(XmlWriter)`                            | instance | push a DOM node into a writer    |
|  [08]   | `new XmlNodeReader(XmlNode)`                            | ctor     | read a DOM subtree as a stream   |

- `XmlDocument.Save(string)`: encoding carries from the first `XmlDeclaration`.

## [10]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- An owned document is an `XDocument`; `XmlDocument` enters only where a host API hands one back, and `XmlNodeReader` returns it to the owned surface.
- Untrusted foreign bytes parse under `XmlReaderSettings.DtdProcessing = Prohibit` with a null `XmlResolver`, so no external entity resolves.
- `XmlReaderSettings.Async` and `XmlWriterSettings.Async` admit the `*Async` member family; a reader or writer built without them carries the sync surface alone.
- File and stream egress is an explicit boundary act — an interior signature carries the owned tree, never a reader, writer, or file name.
- Attribute text crosses in the invariant culture: the typed `explicit operator` on the read side, `ToString("R", CultureInfo.InvariantCulture)` on the write side.

[STACKING]:
- `Xbim.InformationSpecifications`(`Rasm.Bim/.api/api-xbim-informationspecifications.md`): `Xids.LoadBuildingSmartIDS(XElement)` admits an owned tree with no byte round-trip, and `Xids.ExportBuildingSmartIDS` owns the inverse — `Rasm.Bim` `Review/validation` holds both ends of the IDS document lifecycle.
- `RhinoCommon`(`Rasm.Rhino/.api/api-rhinocommon-display.md`): `ViewCapture.CaptureToSvg(ViewCaptureSettings)` hands back an `XmlDocument`, which the vector `CaptureArtifact` carries until `Rasm.Rhino` `Exchange/publish` writes it through `XmlDocument.Save(string)` inside the atomic output-staging boundary.
- `Rasm.Materials` `Appearance/interchange`: renders `MtlxDocument` to `.mtlx` text through `System.Xml.Linq` at the host edge; the structured `MaterialWire` rides `WireCodec`, so no reader or writer reaches an interior signature.
- `Rasm.Persistence` `Ingest/issue`: reads each BCF `markup.bcf` and `*.bcfv` entry through `XDocument` over a `System.IO.Compression` `ZipArchive` container, the whole wire being zip and XML.
- `System.Xml.Linq` composes its own streaming half both directions: `XNode.CreateReader` and `XContainer.CreateWriter` bridge a tree onto the edge, `XNode.ReadFrom` builds a subtree at a reader position, `XmlWriter.WriteNode` transcodes reader to writer with no intermediate tree, and `XStreamingElement` emits a large projection straight to a sink.
- `XObject.AddAnnotation` carries out-of-band per-node state across a load-save round trip, and the `Changing`/`Changed` events fold a mutation stream over an owned tree without a diff pass.

[LOCAL_ADMISSION]:
- Every XML wire this branch defines admits through `System.Xml.Linq`; a foreign package enters only where it owns the format's own schema, as `Xbim.InformationSpecifications` owns IDS and the host `MaterialX` runtime owns `.mtlx` schema validation.

[RAIL_LAW]:
- Package: `System.Xml`
- Owns: every XML document, stream, and object-graph codec the branch crosses
- Accept: `XDocument` trees, `XmlReader`/`XmlWriter` edges under an explicit settings record, `XmlSchemaSet` validation, `XslCompiledTransform` transforms, attribute and contract serializers
- Reject: a hand-rolled XML tokenizer or emitter beside this surface
