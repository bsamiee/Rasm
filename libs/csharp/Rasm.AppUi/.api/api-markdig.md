# [RASM_APPUI_API_MARKDIG]

`Markdig` supplies the Markdown pipeline builder, parse/render entrypoints, and the block/inline AST with typed descendant traversal for document folding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Markdig`
- package: `Markdig`
- assembly: `Markdig`
- namespace: `Markdig`
- namespace: `Markdig.Syntax`
- namespace: `Markdig.Syntax.Inlines`
- namespace: `Markdig.Parsers`
- namespace: `Markdig.Renderers`
- asset: runtime library
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

| [INDEX] | [SURFACE]     | [RAIL]             |
| :-----: | :------------ | :----------------- |
|  [01]   | `Parse`       | string-to-AST      |
|  [02]   | `ToHtml`      | HTML render        |
|  [03]   | `ToPlainText` | plain-text render  |
|  [04]   | `Normalize`   | canonical markdown |
|  [05]   | `Convert`     | custom renderer    |

[BUILDER_ENTRYPOINTS]: pipeline configuration
- rail: markdown

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT]            | [RAIL]           |
| :-----: | :----------------------------------- | :------------------------ | :--------------- |
|  [01]   | `Build`                              | `MarkdownPipelineBuilder` | pipeline seal    |
|  [02]   | `BlockParsers` / `InlineParsers`     | `MarkdownPipelineBuilder` | parser lists     |
|  [03]   | `TrackTrivia`                        | `MarkdownPipelineBuilder` | roundtrip trivia |
|  [04]   | `PreciseSourceLocation`              | `MarkdownPipelineBuilder` | span fidelity    |
|  [05]   | `UseAdvancedExtensions`              | `MarkdownExtensions`      | extension bundle |
|  [06]   | `UsePipeTables` / `UseGridTables`    | `MarkdownExtensions`      | tables           |
|  [07]   | `UseTaskLists`                       | `MarkdownExtensions`      | task lists       |
|  [08]   | `UseAutoIdentifiers`                 | `MarkdownExtensions`      | heading ids      |
|  [09]   | `UseYamlFrontMatter`                 | `MarkdownExtensions`      | front matter     |
|  [10]   | `UseEmphasisExtras` / `UseFootnotes` | `MarkdownExtensions`      | rich inline      |
|  [11]   | `UseMathematics` / `UseDiagrams`     | `MarkdownExtensions`      | math + diagrams  |
|  [12]   | `Use<TExtension>`                    | `MarkdownExtensions`      | custom extension |

[AST_ENTRYPOINTS]: AST traversal for folding
- rail: markdown

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]             | [RAIL]          |
| :-----: | :----------------- | :------------------------- | :-------------- |
|  [01]   | `Descendants`      | `MarkdownObjectExtensions` | full traversal  |
|  [02]   | `Descendants<T>`   | `MarkdownObjectExtensions` | typed traversal |
|  [03]   | `Span`             | `MarkdownObject`           | source span     |
|  [04]   | `Line` / `Column`  | `MarkdownObject`           | source position |
|  [05]   | `LineStartIndexes` | `MarkdownDocument`         | offset mapping  |

## [04]-[IMPLEMENTATION_LAW]

[PIPELINE_LAW]:
- Package: `Markdig`
- Owns: Markdown parsing, extension configuration, and rendering through one immutable pipeline
- Accept: a `MarkdownPipeline` is built once and reused across parses
- Reject: regex or line-split Markdown handling beside the pipeline

[AST_LAW]:
- Package: `Markdig`
- Owns: the block/inline AST as the only Markdown document model
- Accept: document folds drive off `Descendants<T>` projections with `Span` evidence
- Reject: parallel Markdown node models duplicating the syntax tree
