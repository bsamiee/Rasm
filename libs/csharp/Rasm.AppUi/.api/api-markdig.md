# [RASM_APPUI_API_MARKDIG]

`Markdig` supplies the Markdown pipeline builder, parse/render entrypoints, and the block/inline AST with typed descendant traversal for document folding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Markdig`
- package: `Markdig`
- version: `1.3.1`
- assembly: `Markdig`
- namespace: `Markdig`
- namespace: `Markdig.Syntax`
- namespace: `Markdig.Syntax.Inlines`
- namespace: `Markdig.Parsers`
- namespace: `Markdig.Renderers`
- namespace: `Markdig.Renderers.Normalize` (canonical-markdown renderer)
- asset: runtime library
- build-floor: ships `lib/net10.0`; the `net10.0` consumer binds it directly
- rail: markdown

## [02]-[PUBLIC_TYPES]

[PIPELINE_TYPES]: pipeline and parser surfaces
- rail: markdown

| [INDEX] | [SYMBOL]                  | [RAIL]               |
| :-----: | :------------------------ | :------------------- |
|  [01]   | `Markdown`                | static entrypoint    |
|  [02]   | `MarkdownPipeline`        | built pipeline       |
|  [03]   | `MarkdownPipelineBuilder` | pipeline builder     |
|  [04]   | `MarkdownExtensions`      | `Use*` configuration |
|  [05]   | `MarkdownParserContext`   | parse context        |
|  [06]   | `MarkdownParser`          | parser core          |
|  [07]   | `HtmlRenderer`            | HTML renderer        |
|  [08]   | `RendererBase`            | renderer base        |

[BLOCK_TYPES]: block AST family
- rail: markdown

| [INDEX] | [SYMBOL]                  | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|  [01]   | `MarkdownObject`          | AST root base   |
|  [02]   | `MarkdownDocument`        | document root   |
|  [03]   | `Block`                   | block base      |
|  [04]   | `ContainerBlock`          | child-bearing   |
|  [05]   | `LeafBlock`               | inline-bearing  |
|  [06]   | `ParagraphBlock`          | paragraph       |
|  [07]   | `HeadingBlock`            | heading         |
|  [08]   | `FencedCodeBlock`         | fenced code     |
|  [09]   | `CodeBlock`               | indented code   |
|  [10]   | `QuoteBlock`              | block quote     |
|  [11]   | `ListBlock`               | list            |
|  [12]   | `ListItemBlock`           | list item       |
|  [13]   | `ThematicBreakBlock`      | rule            |
|  [14]   | `HtmlBlock`               | raw HTML        |
|  [15]   | `LinkReferenceDefinition` | link definition |

[INLINE_TYPES]: inline AST family
- rail: markdown

| [INDEX] | [SYMBOL]          | [RAIL]          |
| :-----: | :---------------- | :-------------- |
|  [01]   | `Inline`          | inline base     |
|  [02]   | `ContainerInline` | child-bearing   |
|  [03]   | `LeafInline`      | terminal inline |
|  [04]   | `LiteralInline`   | text run        |
|  [05]   | `EmphasisInline`  | emphasis        |
|  [06]   | `LinkInline`      | link / image    |
|  [07]   | `CodeInline`      | code span       |
|  [08]   | `AutolinkInline`  | autolink        |
|  [09]   | `LineBreakInline` | line break      |
|  [10]   | `HtmlInline`      | raw HTML inline |

## [03]-[ENTRYPOINTS]

[PARSE_ENTRYPOINTS]: parse and render operations
- rail: markdown
- surface-root: `Markdown`

| [INDEX] | [SURFACE]     | [RAIL]                                                                          |
| :-----: | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `Parse`       | `Parse(string, pipeline?, context?)` / `Parse(string, bool trackTrivia)` to `MarkdownDocument` |
|  [02]   | `ToHtml`      | `ToHtml(string, pipeline?, context?)` to string, plus `MarkdownDocument.ToHtml(writer, pipeline?)` extension |
|  [03]   | `ToPlainText` | plain-text render to string or `TextWriter`                                     |
|  [04]   | `Normalize`   | canonical-markdown render (`NormalizeOptions`) to string or `TextWriter`        |
|  [05]   | `Convert`     | `Convert(string, IMarkdownRenderer, pipeline?, context?)` for a custom renderer |
|  [06]   | `Markdown.Version` | assembly file-version string (`"1.3.1"`)                                   |

[BUILDER_ENTRYPOINTS]: pipeline configuration
- rail: markdown

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]            | [RAIL]                          |
| :-----: | :----------------------------------------------- | :------------------------ | :------------------------------ |
|  [01]   | `Build`                                          | `MarkdownPipelineBuilder` | seal to immutable `MarkdownPipeline` |
|  [02]   | `BlockParsers` / `InlineParsers`                 | `MarkdownPipelineBuilder` | `OrderedList<BlockParser>` / `OrderedList<InlineParser>` |
|  [03]   | `Extensions`                                     | `MarkdownPipelineBuilder` | `OrderedList<IMarkdownExtension>` direct manipulation |
|  [04]   | `TrackTrivia` / `PreciseSourceLocation`          | `MarkdownPipelineBuilder` | roundtrip trivia + span fidelity (bool props) |
|  [05]   | `MaximumNestingDepth` / `DebugLog`               | `MarkdownPipelineBuilder` | recursion guard + debug `TextWriter` |
|  [06]   | `DocumentProcessed`                              | `MarkdownPipelineBuilder` | `event ProcessDocumentDelegate` post-parse hook |
|  [07]   | `UseAdvancedExtensions`                          | `MarkdownExtensions`      | full extension bundle           |
|  [08]   | `UsePipeTables` / `UseGridTables`                | `MarkdownExtensions`      | tables                          |
|  [09]   | `UseTaskLists` / `UseListExtras`                 | `MarkdownExtensions`      | task lists, ordered-list extras |
|  [10]   | `UseAutoIdentifiers` / `UseAutoLinks`            | `MarkdownExtensions`      | heading ids, bare-URL links     |
|  [11]   | `UseYamlFrontMatter`                             | `MarkdownExtensions`      | front matter                    |
|  [12]   | `UseEmphasisExtras` / `UseFootnotes` / `UseDefinitionLists` | `MarkdownExtensions` | rich inline + definition lists |
|  [13]   | `UseMathematics` / `UseDiagrams`                 | `MarkdownExtensions`      | math + mermaid/diagram fences   |
|  [14]   | `UseCustomContainers` / `UseGenericAttributes`   | `MarkdownExtensions`      | `:::` containers + `{#id .class}` attribute syntax |
|  [15]   | `UseAlertBlocks` / `UseCitations` / `UseAbbreviations` | `MarkdownExtensions` | GitHub alerts, citations, abbreviations |
|  [16]   | `UseEmojiAndSmiley` / `UseSmartyPants` / `UseMediaLinks` | `MarkdownExtensions` | emoji, typographic, media-embed inline |
|  [17]   | `UsePreciseSourceLocation` / `UsePragmaLines`    | `MarkdownExtensions`      | precise spans, line-number pragmas |
|  [18]   | `Use<TExtension>` / `Configure(string)`          | `MarkdownExtensions`      | custom extension, string-named pipeline config |

[AST_ENTRYPOINTS]: AST traversal and node evidence for folding
- rail: markdown

| [INDEX] | [SURFACE]                          | [SURFACE_ROOT]             | [RAIL]                                                |
| :-----: | :--------------------------------- | :------------------------- | :--------------------------------------------------- |
|  [01]   | `Descendants()`                    | `MarkdownObjectExtensions` | full pre-order `IEnumerable<MarkdownObject>` walk     |
|  [02]   | `Descendants<T>()`                 | `MarkdownObjectExtensions` | typed walk; `ContainerBlock`/`ContainerInline` overloads narrow the start node |
|  [03]   | `Span` (`SourceSpan` field) / `ToPositionText()` | `MarkdownObject`           | source span + diagnostic position string             |
|  [04]   | `Line` / `Column`                  | `MarkdownObject`           | zero-based source position                            |
|  [05]   | `SetData` / `GetData` / `ContainsData` / `RemoveData` | `MarkdownObject` | per-node `object`-keyed data bag for fold annotation  |
|  [06]   | `Inline`                           | `LeafBlock`                | `ContainerInline?` child run of a paragraph/heading   |
|  [07]   | `Level` / `HeaderChar`             | `HeadingBlock`             | outline depth + `#`/setext marker                     |
|  [08]   | `Info` / `Arguments`               | `FencedCodeBlock`          | code-fence language tag + raw args (syntax-highlight key) |
|  [09]   | `IsOrdered` / `BulletType` / `Order` | `ListBlock`              | list ordering and marker                              |
|  [10]   | `Url` / `Title` / `IsImage`        | `LinkInline`              | link target, title, image-vs-link discriminant        |
|  [11]   | `LineCount` / `LineStartIndexes`   | `MarkdownDocument`         | line count + per-line absolute offset mapping         |

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_LAW]:
- Package: `Markdig`
- Owns: Markdown parsing, extension configuration, and rendering through one immutable pipeline
- Accept: a `MarkdownPipeline` is built once and reused across parses
- Reject: regex or line-split Markdown handling beside the pipeline

[AST_LAW]:
- Package: `Markdig`
- Owns: the block/inline AST as the only Markdown document model
- Accept: document folds drive off `Descendants<T>` projections with `Span` evidence; the fold reads node-typed evidence (`HeadingBlock.Level`, `FencedCodeBlock.Info`, `LinkInline.Url`/`IsImage`) and annotates via `SetData`/`GetData`
- Reject: parallel Markdown node models duplicating the syntax tree; regex extraction of headings/links the AST already exposes

[STACKING]:
- Outline into the live-data rail: `document.Descendants<HeadingBlock>()` projects to `(Level, text, Span)` rows that seed a `DynamicData` `SourceCache` keyed by `Span.Start`; `TransformToTree` (using `Level` as the parent-depth key) folds the flat heading stream into a collapsible document-outline `Node` tree bound to a `TreeDataGrid` — the outline shares the one change-set rail with every other panel.
- Editor integration: `MarkdownObject.Span`/`Line`/`Column` map fold nodes back to source offsets in the `Avalonia.AvaloniaEdit` document, so an outline-row click scrolls the editor and an editor caret resolves to the enclosing block via the span; `FencedCodeBlock.Info` selects the `AvaloniaEdit.TextMate` grammar for the fence body.
- One built pipeline, reused: a single `MarkdownPipelineBuilder.UseAdvancedExtensions()....Build()` is constructed once at composition and threaded as the `pipeline` argument to every `Parse`/`ToHtml`; `MarkdownParserContext.Properties` carries cross-document state (link-reference resolution, asset rewrites) without a second parse pass, and `DocumentProcessed` hooks post-parse AST rewrites onto the same pipeline.
