# [RASM_APPUI_API_MARKDIG]

`Markdig` owns Markdown-to-AST parsing, extension-configured rendering, and the block/inline syntax tree with typed descendant traversal, all through one immutable pipeline. A single `MarkdownPipeline` builds once and drives every `Parse`, `ToHtml`, and `Normalize`; `Descendants<T>` projections carrying `Span` source-mapping evidence fold the tree onto the document-outline and editor-integration rails.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Markdig`
- package: `Markdig` (BSD-2-Clause, Alexandre Mutel)
- assembly: `Markdig`
- namespace: `Markdig`, `Markdig.Syntax`, `Markdig.Syntax.Inlines`, `Markdig.Parsers`, `Markdig.Renderers`, `Markdig.Renderers.Normalize`, `Markdig.Renderers.Html`, `Markdig.Extensions.Tables`, `Markdig.Extensions.TaskLists`
- asset: runtime library (managed)
- rail: markdown

## [02]-[PUBLIC_TYPES]

[PIPELINE_TYPES]: pipeline, parser, and renderer surfaces.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                    |
| :-----: | :------------------------ | :------------- | :------------------------------ |
|  [01]   | `Markdown`                | static class   | parse and render entrypoint     |
|  [02]   | `MarkdownPipeline`        | sealed class   | built immutable pipeline        |
|  [03]   | `MarkdownPipelineBuilder` | class          | pipeline and parser-state build |
|  [04]   | `MarkdownExtensions`      | static class   | `Use*` feature configuration    |
|  [05]   | `MarkdownParserContext`   | class          | cross-parse context state       |
|  [06]   | `MarkdownParser`          | static class   | parser core                     |
|  [07]   | `HtmlRenderer`            | class          | HTML renderer                   |
|  [08]   | `RendererBase`            | abstract class | renderer base                   |

[BLOCK_TYPES]: block AST family.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]            | [CAPABILITY]                 |
| :-----: | :------------------------ | :----------------------- | :--------------------------- |
|  [01]   | `MarkdownObject`          | abstract class           | AST root base                |
|  [02]   | `MarkdownDocument`        | class : `ContainerBlock` | document root                |
|  [03]   | `Block`                   | abstract class           | block base                   |
|  [04]   | `ContainerBlock`          | abstract class           | child-bearing block          |
|  [05]   | `LeafBlock`               | abstract class           | inline-bearing block         |
|  [06]   | `ParagraphBlock`          | class                    | paragraph                    |
|  [07]   | `HeadingBlock`            | class                    | heading                      |
|  [08]   | `FencedCodeBlock`         | class                    | fenced code                  |
|  [09]   | `CodeBlock`               | class                    | indented code                |
|  [10]   | `QuoteBlock`              | class                    | block quote                  |
|  [11]   | `ListBlock`               | class                    | list                         |
|  [12]   | `ListItemBlock`           | class                    | list item                    |
|  [13]   | `ThematicBreakBlock`      | class                    | rule                         |
|  [14]   | `HtmlBlock`               | class                    | raw HTML block               |
|  [15]   | `LinkReferenceDefinition` | class                    | link definition              |
|  [16]   | `Table`                   | class : `ContainerBlock` | pipe/grid table              |
|  [17]   | `TableRow`                | class : `ContainerBlock` | table row, `IsHeader`        |
|  [18]   | `TableCell`               | class : `ContainerBlock` | table cell, column/span geom |

[INLINE_TYPES]: inline AST family.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]             | [CAPABILITY]         |
| :-----: | :---------------- | :------------------------ | :------------------- |
|  [01]   | `Inline`          | abstract class            | inline base          |
|  [02]   | `ContainerInline` | class                     | child-bearing inline |
|  [03]   | `LeafInline`      | abstract class            | terminal inline      |
|  [04]   | `LiteralInline`   | class                     | text run             |
|  [05]   | `EmphasisInline`  | class : `ContainerInline` | emphasis             |
|  [06]   | `LinkInline`      | class                     | link / image         |
|  [07]   | `CodeInline`      | class                     | code span            |
|  [08]   | `AutolinkInline`  | class                     | autolink             |
|  [09]   | `LineBreakInline` | class                     | line break           |
|  [10]   | `HtmlInline`      | class                     | raw HTML inline      |
|  [11]   | `TaskList`        | class : `LeafInline`      | task-list checkbox   |

## [03]-[ENTRYPOINTS]

[PARSE_ENTRYPOINTS]: `Markdown` owns parse and render, each overload defaulting to the built-in pipeline.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Parse(string, pipeline?, context?)`              | static   | parse to `MarkdownDocument`           |
|  [02]   | `Parse(string, bool)`                             | static   | parse with trivia tracking            |
|  [03]   | `ToHtml(string, pipeline?, context?)`             | static   | markdown or document to HTML string   |
|  [04]   | `ToPlainText(string, pipeline?)`                  | static   | plain-text render to string or writer |
|  [05]   | `Normalize(string, NormalizeOptions?, pipeline?)` | static   | canonical-markdown render             |
|  [06]   | `Convert(string, IMarkdownRenderer, pipeline?)`   | static   | render through a custom renderer      |
|  [07]   | `Version`                                         | property | assembly file-version string          |

[BUILDER_ENTRYPOINTS]: `MarkdownPipelineBuilder` folds parser lists, extensions, and parse policy into one sealed pipeline.

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Build()`                               | instance | seal to immutable `MarkdownPipeline` |
|  [02]   | `BlockParsers` / `InlineParsers`        | property | typed ordered parser lists           |
|  [03]   | `Extensions`                            | property | `OrderedList<IMarkdownExtension>`    |
|  [04]   | `TrackTrivia` / `PreciseSourceLocation` | property | roundtrip trivia and span fidelity   |
|  [05]   | `MaximumNestingDepth` / `DebugLog`      | property | recursion guard and debug writer     |
|  [06]   | `DocumentProcessed`                     | event    | `ProcessDocumentDelegate` post-parse |

[EXTENSION_ENTRYPOINTS]: `MarkdownExtensions` folds each feature onto the builder as one chainable `Use*` call.

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `UseAdvancedExtensions`                                     | static  | full extension bundle                 |
|  [02]   | `UsePipeTables` / `UseGridTables`                           | static  | tables                                |
|  [03]   | `UseTaskLists` / `UseListExtras`                            | static  | task lists and ordered-list extras    |
|  [04]   | `UseAutoIdentifiers` / `UseAutoLinks`                       | static  | heading ids and bare-URL links        |
|  [05]   | `UseYamlFrontMatter`                                        | static  | front matter                          |
|  [06]   | `UseEmphasisExtras` / `UseFootnotes` / `UseDefinitionLists` | static  | inline forms and definition lists     |
|  [07]   | `UseMathematics` / `UseDiagrams`                            | static  | math and diagram fences               |
|  [08]   | `UseCustomContainers` / `UseGenericAttributes`              | static  | containers and generic attributes     |
|  [09]   | `UseAlertBlocks` / `UseCitations` / `UseAbbreviations`      | static  | alerts, citations, abbreviations      |
|  [10]   | `UseEmojiAndSmiley` / `UseSmartyPants` / `UseMediaLinks`    | static  | emoji, typography, embedded media     |
|  [11]   | `UsePreciseSourceLocation` / `UsePragmaLines`               | static  | precise spans and line-number pragmas |
|  [12]   | `Use<TExtension>` / `UseSelfPipeline` / `Configure(string)` | static  | custom, self-configuring, and named   |

[AST_ENTRYPOINTS]: `Descendants<T>` traversal and the node-typed evidence document folds read.

| [INDEX] | [SURFACE]                                         | [SHAPE]   | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------ | :-------- | :--------------------------------------------------------- |
|  [01]   | `MarkdownObjectExtensions.Descendants()`          | extension | pre-order `IEnumerable<MarkdownObject>` walk               |
|  [02]   | `MarkdownObjectExtensions.Descendants<T>()`       | extension | typed walk, start-node narrowing on container overloads    |
|  [03]   | `MarkdownObject.Span` / `ToPositionText()`        | member    | `SourceSpan` field and diagnostic position render          |
|  [04]   | `MarkdownObject.Line` / `Column`                  | property  | zero-based source position                                 |
|  [05]   | `MarkdownObject.SetData` / `GetData`              | method    | per-node keyed data bag, `ContainsData` / `RemoveData`     |
|  [06]   | `LeafBlock.Inline`                                | property  | nullable `ContainerInline` child run                       |
|  [07]   | `HeadingBlock.Level` / `HeaderChar`               | property  | outline depth and `#` or setext marker                     |
|  [08]   | `FencedCodeBlock.Info` / `Arguments`              | property  | fence language tag and raw args for the highlight key      |
|  [09]   | `ListBlock.IsOrdered` / `BulletType` / `Order`    | property  | list ordering and marker                                   |
|  [10]   | `LinkInline.Url` / `Title` / `IsImage`            | property  | link target, title, image-versus-link discriminant         |
|  [11]   | `MarkdownDocument.LineCount` / `LineStartIndexes` | property  | line count and per-line absolute offset mapping            |
|  [12]   | `LeafBlock.Lines`                                 | property  | raw accumulated line text via `StringLineGroup.ToString()` |
|  [13]   | `CodeInline.Content` / `LiteralInline.Content`    | property  | code `string` content or `StringSlice` literal text        |
|  [14]   | `EmphasisInline.DelimiterCount` / `DelimiterChar` | property  | run depth (`1` italic, `>= 2` bold) and marker char        |
|  [15]   | `Inline.Parent`                                   | property  | nullable parent with `PreviousSibling` / `NextSibling`     |
|  [16]   | `TaskList.Checked`                                | property  | boolean checkbox state                                     |
|  [17]   | `HtmlAttributesExtensions.TryGetAttributes()`     | extension | attached `HtmlAttributes` (`Id`, `Classes`, `Properties`)  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `MarkdownPipeline` builds once at composition and threads as the `pipeline` argument to every `Parse`, `ToHtml`, and `Normalize`; `MarkdownParserContext.Properties` carries cross-document state and `DocumentProcessed` hooks post-parse AST rewrites onto the same pipeline.
- Block and inline nodes form the sole Markdown document model; folds project through `Descendants<T>` with `Span` evidence and annotate via `SetData`/`GetData`, reading node-typed evidence (`HeadingBlock.Level`, `FencedCodeBlock.Info`, `LinkInline.Url`/`IsImage`) directly off each node.

[STACKING]:
- `api-dynamicdata`(`.api/api-dynamicdata.md`): `Descendants<HeadingBlock>()` projects `(Level, text, Span)` rows seeding a `SourceCache` keyed by `Span.Start`; `TransformToTree` on `Level` folds the flat heading stream into the collapsible outline `Node` tree bound to a `TreeDataGrid`, sharing the one change-set rail with every other panel.
- `api-avaloniaedit`(`.api/api-avaloniaedit.md`): `Span`/`Line`/`Column` map fold nodes to editor-document offsets, so an outline-row click scrolls the editor and a caret resolves to its enclosing block; `FencedCodeBlock.Info` selects the `api-textmatesharp` grammar for the fence body.
- Within-lib: one `MarkdownPipelineBuilder.UseAdvancedExtensions()…Build()` constructed at composition, threaded to every parse, resolving link references and asset rewrites through `MarkdownParserContext.Properties` without a second pass.

[LOCAL_ADMISSION]:
- `Markdig` is the branch's sole Markdown parser, renderer, and AST; one pipeline builds once and is reused across every parse.

[RAIL_LAW]:
- Package: `Markdig`
- Owns: Markdown parsing, extension configuration, rendering, and the block/inline AST as the only document model.
- Accept: one immutable pipeline reused across parses; document folds over `Descendants<T>` with `Span` evidence and `SetData`/`GetData` annotation, driven by node-typed evidence.
- Reject: regex or line-split Markdown handling beside the pipeline; a parallel node model duplicating the syntax tree; regex extraction of headings or links the AST already exposes.
