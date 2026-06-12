# [RASM_APPUI_API_MARKDIG]

`Markdig` supplies the Markdown pipeline builder, parse/render entrypoints, and the block/inline AST with typed descendant traversal for document folding.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PIPELINE_TYPES]: pipeline and parser surfaces
- rail: markdown

| [INDEX] | [SYMBOL]                  | [RAIL]               |
| :-----: | :------------------------ | :------------------- |
|   [1]   | `Markdown`                | static entrypoint    |
|   [2]   | `MarkdownPipeline`        | built pipeline       |
|   [3]   | `MarkdownPipelineBuilder` | pipeline builder     |
|   [4]   | `MarkdownExtensions`      | `Use*` configuration |
|   [5]   | `MarkdownParserContext`   | parse context        |
|   [6]   | `MarkdownParser`          | parser core          |
|   [7]   | `HtmlRenderer`            | HTML renderer        |
|   [8]   | `RendererBase`            | renderer base        |

[BLOCK_TYPES]: block AST family
- rail: markdown

| [INDEX] | [SYMBOL]                  | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|   [1]   | `MarkdownObject`          | AST root base   |
|   [2]   | `MarkdownDocument`        | document root   |
|   [3]   | `Block`                   | block base      |
|   [4]   | `ContainerBlock`          | child-bearing   |
|   [5]   | `LeafBlock`               | inline-bearing  |
|   [6]   | `ParagraphBlock`          | paragraph       |
|   [7]   | `HeadingBlock`            | heading         |
|   [8]   | `FencedCodeBlock`         | fenced code     |
|   [9]   | `CodeBlock`               | indented code   |
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
|   [1]   | `Inline`          | inline base     |
|   [2]   | `ContainerInline` | child-bearing   |
|   [3]   | `LeafInline`      | terminal inline |
|   [4]   | `LiteralInline`   | text run        |
|   [5]   | `EmphasisInline`  | emphasis        |
|   [6]   | `LinkInline`      | link / image    |
|   [7]   | `CodeInline`      | code span       |
|   [8]   | `AutolinkInline`  | autolink        |
|   [9]   | `LineBreakInline` | line break      |
|  [10]   | `HtmlInline`      | raw HTML inline |

## [3]-[ENTRYPOINTS]

[PARSE_ENTRYPOINTS]: parse and render operations
- rail: markdown

| [INDEX] | [SURFACE]     | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :------------ | :------------- | :----------------- |
|   [1]   | `Parse`       | `Markdown`     | string-to-AST      |
|   [2]   | `ToHtml`      | `Markdown`     | HTML render        |
|   [3]   | `ToPlainText` | `Markdown`     | plain-text render  |
|   [4]   | `Normalize`   | `Markdown`     | canonical markdown |
|   [5]   | `Convert`     | `Markdown`     | custom renderer    |

[BUILDER_ENTRYPOINTS]: pipeline configuration
- rail: markdown

| [INDEX] | [SURFACE]                            | [SURFACE_ROOT]            | [RAIL]           |
| :-----: | :----------------------------------- | :------------------------ | :--------------- |
|   [1]   | `Build`                              | `MarkdownPipelineBuilder` | pipeline seal    |
|   [2]   | `BlockParsers` / `InlineParsers`     | `MarkdownPipelineBuilder` | parser lists     |
|   [3]   | `TrackTrivia`                        | `MarkdownPipelineBuilder` | roundtrip trivia |
|   [4]   | `PreciseSourceLocation`              | `MarkdownPipelineBuilder` | span fidelity    |
|   [5]   | `UseAdvancedExtensions`              | `MarkdownExtensions`      | extension bundle |
|   [6]   | `UsePipeTables` / `UseGridTables`    | `MarkdownExtensions`      | tables           |
|   [7]   | `UseTaskLists`                       | `MarkdownExtensions`      | task lists       |
|   [8]   | `UseAutoIdentifiers`                 | `MarkdownExtensions`      | heading ids      |
|   [9]   | `UseYamlFrontMatter`                 | `MarkdownExtensions`      | front matter     |
|  [10]   | `UseEmphasisExtras` / `UseFootnotes` | `MarkdownExtensions`      | rich inline      |
|  [11]   | `UseMathematics` / `UseDiagrams`     | `MarkdownExtensions`      | math + diagrams  |
|  [12]   | `Use<TExtension>`                    | `MarkdownExtensions`      | custom extension |

[AST_ENTRYPOINTS]: AST traversal for folding
- rail: markdown

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]             | [RAIL]          |
| :-----: | :----------------- | :------------------------- | :-------------- |
|   [1]   | `Descendants`      | `MarkdownObjectExtensions` | full traversal  |
|   [2]   | `Descendants<T>`   | `MarkdownObjectExtensions` | typed traversal |
|   [3]   | `Span`             | `MarkdownObject`           | source span     |
|   [4]   | `Line` / `Column`  | `MarkdownObject`           | source position |
|   [5]   | `LineStartIndexes` | `MarkdownDocument`         | offset mapping  |

## [4]-[IMPLEMENTATION_LAW]

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
