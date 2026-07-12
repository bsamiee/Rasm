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
- namespace: `Markdig.Renderers.Normalize` (canonical-markdown renderer)
- namespace: `Markdig.Renderers.Html` (`HtmlAttributes` attach surface + `TryGetAttributes`)
- namespace: `Markdig.Extensions.Tables` (pipe/grid table block AST)
- namespace: `Markdig.Extensions.TaskLists` (task-list checkbox inline)
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

| [INDEX] | [SYMBOL]                                           | [RAIL]                                            |
| :-----: | :------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `MarkdownObject`                                   | AST root base                                     |
|  [02]   | `MarkdownDocument`                                 | document root                                     |
|  [03]   | `Block`                                            | block base                                        |
|  [04]   | `ContainerBlock`                                   | child-bearing                                     |
|  [05]   | `LeafBlock`                                        | inline-bearing                                    |
|  [06]   | `ParagraphBlock`                                   | paragraph                                         |
|  [07]   | `HeadingBlock`                                     | heading                                           |
|  [08]   | `FencedCodeBlock`                                  | fenced code                                       |
|  [09]   | `CodeBlock`                                        | indented code                                     |
|  [10]   | `QuoteBlock`                                       | block quote                                       |
|  [11]   | `ListBlock`                                        | list                                              |
|  [12]   | `ListItemBlock`                                    | list item                                         |
|  [13]   | `ThematicBreakBlock`                               | rule                                              |
|  [14]   | `HtmlBlock`                                        | raw HTML                                          |
|  [15]   | `LinkReferenceDefinition`                          | link definition                                   |
|  [16]   | `Extensions.Tables.Table` (`: ContainerBlock`)     | pipe/grid table                                   |
|  [17]   | `Extensions.Tables.TableRow` (`: ContainerBlock`)  | table row (`IsHeader`)                            |
|  [18]   | `Extensions.Tables.TableCell` (`: ContainerBlock`) | table cell (`ColumnIndex`/`ColumnSpan`/`RowSpan`) |

[INLINE_TYPES]: inline AST family
- rail: markdown

| [INDEX] | [SYMBOL]                                         | [RAIL]                         |
| :-----: | :----------------------------------------------- | :----------------------------- |
|  [01]   | `Inline`                                         | inline base                    |
|  [02]   | `ContainerInline`                                | child-bearing                  |
|  [03]   | `LeafInline`                                     | terminal inline                |
|  [04]   | `LiteralInline`                                  | text run                       |
|  [05]   | `EmphasisInline`                                 | emphasis                       |
|  [06]   | `LinkInline`                                     | link / image                   |
|  [07]   | `CodeInline`                                     | code span                      |
|  [08]   | `AutolinkInline`                                 | autolink                       |
|  [09]   | `LineBreakInline`                                | line break                     |
|  [10]   | `HtmlInline`                                     | raw HTML inline                |
|  [11]   | `Extensions.TaskLists.TaskList` (`: LeafInline`) | task-list checkbox (`Checked`) |

## [03]-[ENTRYPOINTS]

[PARSE_ENTRYPOINTS]: parse and render operations
- rail: markdown
- surface-root: `Markdown`

| [INDEX] | [SURFACE]          | [RAIL]                                                                                                       |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `Parse`            | `Parse(string, pipeline?, context?)` / `Parse(string, bool trackTrivia)` to `MarkdownDocument`               |
|  [02]   | `ToHtml`           | `ToHtml(string, pipeline?, context?)` to string, plus `MarkdownDocument.ToHtml(writer, pipeline?)` extension |
|  [03]   | `ToPlainText`      | plain-text render to string or `TextWriter`                                                                  |
|  [04]   | `Normalize`        | canonical-markdown render (`NormalizeOptions`) to string or `TextWriter`                                     |
|  [05]   | `Convert`          | `Convert(string, IMarkdownRenderer, pipeline?, context?)` for a custom renderer                              |
|  [06]   | `Markdown.Version` | assembly file-version string (`"1.3.2"`)                                                                     |

[BUILDER_ENTRYPOINTS]: pipeline configuration
- rail: markdown

`MarkdownPipelineBuilder` owns pipeline construction and parser state.

| [INDEX] | [SURFACE]                               | [RAIL]                                      |
| :-----: | :-------------------------------------- | :------------------------------------------ |
|  [01]   | `Build`                                 | seal to immutable `MarkdownPipeline`        |
|  [02]   | `BlockParsers` / `InlineParsers`        | typed ordered parser lists                  |
|  [03]   | `Extensions`                            | `OrderedList<IMarkdownExtension>` mutations |
|  [04]   | `TrackTrivia` / `PreciseSourceLocation` | roundtrip trivia and span fidelity          |
|  [05]   | `MaximumNestingDepth` / `DebugLog`      | recursion guard and debug writer            |
|  [06]   | `DocumentProcessed`                     | `ProcessDocumentDelegate` post-parse event  |

`MarkdownExtensions` owns the pipeline configuration extensions.

| [INDEX] | [SURFACE]                                                   | [RAIL]                                |
| :-----: | :---------------------------------------------------------- | :------------------------------------ |
|  [01]   | `UseAdvancedExtensions`                                     | full extension bundle                 |
|  [02]   | `UsePipeTables` / `UseGridTables`                           | tables                                |
|  [03]   | `UseTaskLists` / `UseListExtras`                            | task lists and ordered-list extras    |
|  [04]   | `UseAutoIdentifiers` / `UseAutoLinks`                       | heading ids and bare-URL links        |
|  [05]   | `UseYamlFrontMatter`                                        | front matter                          |
|  [06]   | `UseEmphasisExtras` / `UseFootnotes` / `UseDefinitionLists` | inline forms and definition lists     |
|  [07]   | `UseMathematics` / `UseDiagrams`                            | math and diagram fences               |
|  [08]   | `UseCustomContainers` / `UseGenericAttributes`              | containers and generic attributes     |
|  [09]   | `UseAlertBlocks` / `UseCitations` / `UseAbbreviations`      | alerts, citations, and abbreviations  |
|  [10]   | `UseEmojiAndSmiley` / `UseSmartyPants` / `UseMediaLinks`    | emoji, typography, and embedded media |
|  [11]   | `UsePreciseSourceLocation` / `UsePragmaLines`               | precise spans and line-number pragmas |
|  [12]   | `Use<TExtension>` / `Configure(string)`                     | custom and string-named configuration |

[AST_ENTRYPOINTS]: AST traversal and node evidence for folding
- rail: markdown

| [INDEX] | [SURFACE]                                             | [SURFACE_ROOT]                                  |
| :-----: | :---------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Descendants()`                                       | `MarkdownObjectExtensions`                      |
|  [02]   | `Descendants<T>()`                                    | `MarkdownObjectExtensions`                      |
|  [03]   | `Span` / `ToPositionText()`                           | `MarkdownObject`                                |
|  [04]   | `Line` / `Column`                                     | `MarkdownObject`                                |
|  [05]   | `SetData` / `GetData` / `ContainsData` / `RemoveData` | `MarkdownObject`                                |
|  [06]   | `Inline`                                              | `LeafBlock`                                     |
|  [07]   | `Level` / `HeaderChar`                                | `HeadingBlock`                                  |
|  [08]   | `Info` / `Arguments`                                  | `FencedCodeBlock`                               |
|  [09]   | `IsOrdered` / `BulletType` / `Order`                  | `ListBlock`                                     |
|  [10]   | `Url` / `Title` / `IsImage`                           | `LinkInline`                                    |
|  [11]   | `LineCount` / `LineStartIndexes`                      | `MarkdownDocument`                              |
|  [12]   | `Lines`                                               | `LeafBlock`                                     |
|  [13]   | `Content`                                             | `CodeInline` / `LiteralInline`                  |
|  [14]   | `DelimiterCount` / `DelimiterChar`                    | `EmphasisInline`                                |
|  [15]   | `Parent`                                              | `Inline`                                        |
|  [16]   | `Checked`                                             | `Extensions.TaskLists.TaskList`                 |
|  [17]   | `TryGetAttributes() -> HtmlAttributes?`               | `HtmlAttributesExtensions` on `IMarkdownObject` |

[AST_EVIDENCE]:
- [01]-[WALK]: Full pre-order `IEnumerable<MarkdownObject>` walk.
- [02]-[TYPED_WALK]: Typed walk with start-node narrowing through the `ContainerBlock` and `ContainerInline` overloads.
- [03]-[SPAN]: `Span` is a `SourceSpan` field, and `ToPositionText()` renders its diagnostic position.
- [04]-[POSITION]: Zero-based source position.
- [05]-[DATA]: Per-node `object`-keyed data bag for fold annotation.
- [06]-[INLINE]: Nullable `ContainerInline` child run of a paragraph or heading.
- [07]-[HEADING]: Outline depth and the `#` or setext marker.
- [08]-[FENCE]: Code-fence language tag and raw arguments for the syntax-highlight key.
- [09]-[LIST]: List ordering and marker.
- [10]-[LINK]: Link target, title, and image-versus-link discriminant.
- [11]-[LINES]: Line count and per-line absolute offset mapping.
- [12]-[RAW_TEXT]: Raw accumulated line text through `StringLineGroup.ToString()` for code-fence or indented-code bodies.
- [13]-[CONTENT]: `string` code content or `StringSlice` literal text.
- [14]-[EMPHASIS]: Emphasis run depth, with `1` for italic and `>= 2` for bold, plus the marker character.
- [15]-[ANCESTRY]: Nullable doubly-linked parent with `PreviousSibling` and `NextSibling` ancestry hops.
- [16]-[TASK]: Boolean task-list checkbox state.
- [17]-[ATTRIBUTES]: Attached `HtmlAttributes` with `Id`, `Classes`, and `Properties` for the heading-anchor slug.

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
