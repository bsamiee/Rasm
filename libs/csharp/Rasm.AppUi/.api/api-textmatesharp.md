# [RASM_APPUI_API_TEXTMATESHARP]

`TextMateSharp` is a managed port of the VS Code TextMate tokenizer: a `Registry` drives an `IGrammar` over a `LineText` line and emits scope-tagged `IToken` runs (or a binary-packed `int[]` via `TokenizeLine2`), a `Theme` resolves those scopes to colors and `FontStyle`, and an `ITMModel`/`TMModel` background engine incrementally re-tokenizes a line-list off the UI thread and pushes `ModelTokensChangedEvent` deltas to listeners. `TextMateSharp.Grammars` ships the bundled corpus — 50 language grammars and 21 themes — behind one `RegistryOptions(ThemeName)` reference implementation of the `IRegistryOptions` locator, plus the `GrammarDefinition`/`Language`/`LanguageConfiguration` model that decodes a VS Code `package.json` grammar extension (comment markers, bracket pairs, folding markers, indentation rules). The critical integration fact: `AvaloniaEdit.TextMate.InstallTextMate(IRegistryOptions)` (catalogued in `api-avaloniaedit.md`) consumes the `IRegistryOptions`, scope strings, and `IRawTheme` handles defined HERE — `RegistryOptions`/`GetScopeByExtension`/`LoadTheme`/`ThemeName` are TextMateSharp types the AvaloniaEdit adapter only forwards. The inspector design page registers Rasm-DSL scopes (`source.rasm`, `source.rasm-expression`) by implementing the four-member `IRegistryOptions` contract directly, and the standalone `IGrammar.TokenizeLine` token rail serves any non-editor tokenization (livedata/log syntax coloring) without an editor control at all. Native regex matching is `Onigwrap` (Oniguruma); no managed regex fallback exists.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TextMateSharp`
- package: `TextMateSharp`
- version: `2.0.3`
- license: `MIT`
- assembly: `TextMateSharp`
- build-floor: `netstandard2.0` (single TFM; the `net10.0` consumer binds this asset directly — no TFM-precedence ambiguity)
- namespace: `TextMateSharp.Registry` (locator + engine), `.Grammars` (`IGrammar`/`IToken`/`LineText` tokenize surface), `.Themes` (`IRawTheme`/`Theme`/`FontStyle`), `.Model` (`ITMModel`/`TMModel` background tokenizer)
- asset: runtime library
- rail: tokenizer
- depends: `Onigwrap` (`1.0.10`, transitive) — Oniguruma native regex engine; the `IGrammar` match loop is native, not managed. No pure-managed regex path; the native binary must ship with the app.

[PACKAGE_SURFACE]: `TextMateSharp.Grammars`
- package: `TextMateSharp.Grammars`
- version: `2.0.3`
- license: `MIT`
- assembly: `TextMateSharp.Grammars`
- build-floor: `netstandard2.0` (single TFM)
- namespace: `TextMateSharp.Grammars` (`RegistryOptions`, `ThemeName`, `GrammarDefinition`, `Language`, `LanguageConfiguration`)
- asset: runtime library (embedded grammar/theme JSON resources)
- rail: tokenizer
- depends: `TextMateSharp` (`2.0.3`, transitive) — supplies `IRegistryOptions`, `IRawTheme`, the tokenize/model surface. Both arrive transitively through `AvaloniaEdit.TextMate 12.0.0`; neither is a direct `Directory.Packages.props` entry, so `api resolve` keys on neither — decompile from the consumer-restored `lib/netstandard2.0` asset.

## [02]-[PUBLIC_TYPES]

[REGISTRY_TYPES]: the locator contract, its bundled reference implementation, and the tokenizer engine (`TextMateSharp.Registry`, `TextMateSharp.Grammars`)
- rail: tokenizer

| [INDEX] | [SYMBOL]          | [SIGNATURE]                                                                  | [RAIL]              |
| :-----: | :---------------- | :-------------------------------------------------------------------------- | :------------------ |
|  [01]   | `IRegistryOptions`| `interface` — `GetTheme`/`GetGrammar`/`GetInjections`/`GetDefaultTheme` (4 members) | locator contract  |
|  [02]   | `RegistryOptions` | `class : IRegistryOptions` (`TextMateSharp.Grammars`) — bundled-corpus locator + custom-dir loader | reference locator |
|  [03]   | `Registry`        | `class` (`LoadGrammar`/`GrammarForScopeName`/`SetTheme`/`GetColorMap`/`GetTheme`/`GetLocator`) | tokenizer engine |
|  [04]   | `ThemeName`       | `enum` (21 cases) — keys the bundled theme corpus                            | theme key           |

`IRegistryOptions` is the entire seam `AvaloniaEdit.TextMate.InstallTextMate` requires: four members the host must answer. `RegistryOptions` is the ready-made implementation backed by the 50 embedded grammars and 21 themes; a Rasm-DSL host that adds custom scopes implements the four members itself (or composes over a `RegistryOptions` and overrides `GetGrammar` for its own scopes).

[GRAMMAR_TYPES]: the scope-tagged tokenize surface — what `Registry.LoadGrammar(scope)` returns (`TextMateSharp.Grammars`)
- rail: tokenizer

| [INDEX] | [SYMBOL]                | [SIGNATURE]                                                                 | [RAIL]             |
| :-----: | :---------------------- | :------------------------------------------------------------------------- | :----------------- |
|  [01]   | `IGrammar`              | `interface` (`TokenizeLine`/`TokenizeLine2`/`GetScopeName`/`GetFileTypes`/`IsCompiling`) | grammar handle |
|  [02]   | `IToken`                | `interface` (`StartIndex`/`EndIndex`/`Length`/`Scopes` — `List<string>` scope stack) | token run     |
|  [03]   | `ITokenizeLineResult`   | `interface` (`IToken[] Tokens`, `IStateStack RuleStack`)                    | object token result |
|  [04]   | `ITokenizeLineResult2`  | `interface` (`int[] Tokens` — binary-packed metadata, `IStateStack RuleStack`) | packed token result |
|  [05]   | `IStateStack`           | `interface` (`Depth`/`RuleId`/`EndRule`) — carry-forward tokenizer state    | line continuation  |
|  [06]   | `LineText`              | `readonly struct : IEquatable<LineText>` — `ReadOnlyMemory<char>` wrapper, implicit from `string`/`ReadOnlyMemory<char>` | zero-copy line input |
|  [07]   | `IGrammarRepository` / `IToken` collaborators | grammar-resolution + injection lookup contracts                | grammar wiring     |

`TokenizeLine(LineText)` returns `IToken[]` where each `IToken.Scopes` is the full TextMate scope stack at that span (`["source.cs","keyword.control.cs"]`); `TokenizeLine2` returns the VS Code binary encoding (foreground/background/fontStyle packed into each `int`) for tight color application. Pass the prior line's `IStateStack` (`result.RuleStack`) to the next line's `TokenizeLine(line, prevState, timeLimit)` so multi-line constructs (block comments, string interpolation) carry across lines. `LineText`'s implicit `string` conversion means `grammar.TokenizeLine("…")` is the call site; the `ReadOnlyMemory<char>` overload avoids the substring copy for a rope/slice source.

[THEME_TYPES]: scope-to-color resolution (`TextMateSharp.Themes`)
- rail: tokenizer

| [INDEX] | [SYMBOL]          | [SIGNATURE]                                                                 | [RAIL]            |
| :-----: | :---------------- | :------------------------------------------------------------------------- | :---------------- |
|  [01]   | `IRawTheme`       | `interface` (`GetName`/`GetInclude`/`GetSettings`/`GetTokenColors`/`GetGuiColors`) | raw theme doc |
|  [02]   | `Theme`           | `class` (`CreateFromRawTheme(IRawTheme, IRegistryOptions)`, `Match(IList<string>)`, `GetColorMap`/`GetColorId`/`GetColor`/`GetGuiColorDictionary`) | compiled theme |
|  [03]   | `IRawThemeSetting`/`IThemeSetting` | `interface` — one theme rule (scope selector + settings)            | theme rule        |
|  [04]   | `FontStyle`       | `[Flags] enum` (`NotSet=-1`, `None=0`, `Italic`, `Bold`, `Underline`, `Strikethrough`) | font style mask |
|  [05]   | `IThemeProvider`  | `interface` (`ThemeMatch(IList<string>)`, `GetDefaults`)                    | theme match port  |

`IRawTheme` is the parsed-but-uncompiled theme JSON (what `RegistryOptions.LoadTheme(ThemeName)` returns and `InstallTextMate`'s `SetTheme` consumes). `Theme.CreateFromRawTheme` compiles it against the locator into a scope-trie; `Theme.Match(scopeStack)` returns the winning `ThemeTrieElementRule` (color id + `FontStyle`) for a token's scope stack, and `GetColor(id)` resolves the id to the hex string. `GetGuiColorDictionary()` exposes editor-chrome colors (`"editor.background"`, `"editorLineNumber.foreground"`) so a host can paint the surrounding UI to match the grammar theme — this is the data behind AvaloniaEdit.TextMate's `TryGetThemeColor`.

[CORPUS_MODEL_TYPES]: the VS Code grammar-extension model `RegistryOptions` decodes (`TextMateSharp.Grammars`)
- rail: tokenizer

| [INDEX] | [SYMBOL]                | [SIGNATURE]                                                                 | [RAIL]             |
| :-----: | :---------------------- | :------------------------------------------------------------------------- | :----------------- |
|  [01]   | `GrammarDefinition`     | `class` (`Name`/`DisplayName`/`Version`/`Contributes`/`LanguageSnippets`; static `Parse(Stream)`/`Parse(string)`) | extension manifest |
|  [02]   | `Contributes`           | `class` (`Languages`/`Grammars`/`Snippets` lists)                          | contribution set   |
|  [03]   | `Language`              | `class` (`Id`/`Extensions`/`Aliases`/`MimeTypes`/`ConfigurationFile`/`Configuration`) | language row |
|  [04]   | `Grammar`               | `class` (`Language`/`ScopeName`/`Path`)                                     | grammar row        |
|  [05]   | `LanguageConfiguration` | `class` (`Comments`/`Brackets`/`AutoClosingPairs`/`SurroundingPairs`/`Folding`/`IndentationRules`/`EnterRules`; static `Load`/`LoadFromLocal`/`Parse`) | editor behavior |
|  [06]   | `Comments`/`Folding`/`Markers`/`Indentation`/`EnterRules`/`AutoClosingPairs` | `class` — `LineComment`/`BlockComment`, fold `Markers`, indent rules | behavior sub-model |
|  [07]   | `LanguageSnippets` / `LanguageSnippet` / `Region` | `class` (`Snippets` dict; `Prefix`/`Body`/`Description`) | snippet corpus     |
|  [08]   | `ThemeName`             | `enum` — 21 bundled themes (see `[ENTRYPOINTS]`)                            | theme key          |

`LanguageConfiguration` is the load-bearing model for editor behavior beyond color: `Comments.LineComment`/`BlockComment` feed a comment-toggle command, `Brackets`/`AutoClosingPairs`/`SurroundingPairs` feed bracket matching and auto-close, and `Folding.Markers` feeds a marker-based folding strategy. `RegistryOptions.GetLanguageByExtension(".cs").Configuration` reaches it for any bundled language without re-parsing JSON.

[MODEL_TYPES]: the background incremental tokenizer (`TextMateSharp.Model`) — what AvaloniaEdit.TextMate's `TextEditorModel`/`DocumentSnapshot` adapt
- rail: tokenizer

| [INDEX] | [SYMBOL]                      | [SIGNATURE]                                                                 | [RAIL]             |
| :-----: | :---------------------------- | :------------------------------------------------------------------------- | :----------------- |
|  [01]   | `ITMModel`                    | `interface` (`SetGrammar`/`AddModelTokensChangedListener`/`RemoveModelTokensChangedListener`/`ForceTokenization`/`Dispose`) | model contract |
|  [02]   | `TMModel`                     | `class : ITMModel` (`.ctor(IModelLines)`; `GetLineTokens`/`InvalidateLine`/`InvalidateLineRange`/`IsLineInvalid`/`ForceTokenization`) | bg tokenizer |
|  [03]   | `IModelLines`                 | `interface` — line-list source the model tokenizes                          | line source        |
|  [04]   | `IModelTokensChangedListener` | `interface` (`ModelTokensChanged(ModelTokensChangedEvent)`)                 | delta listener     |
|  [05]   | `ITokenizationSupport`        | `interface` (`GetInitialState`, `Tokenize(LineText, TMState, …)`)           | tokenize port      |

`TMModel(IModelLines)` runs a background `TokenizerThread` that revalidates invalidated lines and emits `ModelTokensChangedEvent` ranges to registered listeners; `GetLineTokens(lineIndex)` reads the cached `TMToken` list for a line, `InvalidateLineRange` re-queues a span after an edit. This is the engine AvaloniaEdit.TextMate wraps — a host that wants incremental tokenization WITHOUT the AvaloniaEdit editor (e.g. a virtualized log view) drives `TMModel` directly over its own `IModelLines`/`IModelTokensChangedListener`.

## [03]-[ENTRYPOINTS]

[REGISTRY_ENTRYPOINTS]: `RegistryOptions` corpus query, theme load, and custom-grammar loading
- rail: tokenizer

| [INDEX] | [SURFACE]                       | [SIGNATURE]                                                                 | [RAIL]            |
| :-----: | :------------------------------ | :------------------------------------------------------------------------- | :---------------- |
|  [01]   | `.ctor`                         | `new RegistryOptions(ThemeName defaultTheme)`                              | corpus locator    |
|  [02]   | `GetScopeByExtension`           | `string GetScopeByExtension(string extension)` (e.g. `".cs"` → `"source.cs"`) | ext → scope     |
|  [03]   | `GetScopeByLanguageId`          | `string GetScopeByLanguageId(string languageId)`                          | id → scope        |
|  [04]   | `GetLanguageByExtension`        | `Language GetLanguageByExtension(string extension)` (→ `.Configuration`)   | ext → language    |
|  [05]   | `GetAvailableLanguages` / `GetAvailableGrammarDefinitions` | `List<Language>` / `IEnumerable<GrammarDefinition>` | corpus roster |
|  [06]   | `LoadTheme`                     | `IRawTheme LoadTheme(ThemeName name)`                                      | theme load        |
|  [07]   | `GetTheme` / `GetGrammar` / `GetDefaultTheme` / `GetInjections` | the 4 `IRegistryOptions` members — `IRawTheme`/`IRawGrammar`/`IRawTheme`/`ICollection<string>` | locator contract |
|  [08]   | `LoadFromLocalDir` / `LoadFromLocalFile` | `void LoadFromLocalDir(string dirPath, bool overwrite = false)` / `void LoadFromLocalFile(string grammarName, string packageJson, bool overwrite = false)` | custom grammar load |

`new RegistryOptions(ThemeName.DarkPlus)` is the single construction the editor stack passes to `InstallTextMate`; `GetScopeByExtension(".cs")` yields the scope the editor's `SetGrammar` then selects. `LoadFromLocalDir`/`LoadFromLocalFile` ingest a VS Code grammar extension folder (a `package.json` + `.tmLanguage.json` + language-configuration) so the Rasm-DSL grammar registers from disk WITHOUT a custom `IRegistryOptions` — the simpler path than reimplementing the four members when the grammar is file-backed.

[REGISTRY_ENGINE_ENTRYPOINTS]: standalone `Registry` tokenization (no editor)
- rail: tokenizer

| [INDEX] | [SURFACE]                | [SIGNATURE]                                                                 | [RAIL]            |
| :-----: | :----------------------- | :------------------------------------------------------------------------- | :---------------- |
|  [01]   | `.ctor`                  | `new Registry(IRegistryOptions locator)` (or `new Registry()` → `DefaultLocator`) | engine init |
|  [02]   | `LoadGrammar`            | `IGrammar LoadGrammar(string initialScopeName)` (resolves + compiles, follows injections) | grammar resolve |
|  [03]   | `GrammarForScopeName`    | `IGrammar GrammarForScopeName(string scopeName)` / `(scopeName, int initialLanguage, Dictionary<string,int> embeddedLanguages)` | embedded-language grammar |
|  [04]   | `LoadGrammarFromPathSync`| `IGrammar LoadGrammarFromPathSync(string path, int initialLanguage, Dictionary<string,int> embeddedLanguages)` | file grammar |
|  [05]   | `SetTheme` / `GetTheme` / `GetColorMap` / `GetLocator` | `void SetTheme(IRawTheme)` / `Theme GetTheme()` / `ICollection<string> GetColorMap()` / `IRegistryOptions GetLocator()` | theme + state |

`new Registry(registryOptions).LoadGrammar("source.cs").TokenizeLine(line)` is the complete non-editor tokenization rail: no `TextEditor`, no `TextView` — just scope-tagged `IToken` runs over a string. The livedata/log/inspector pages that color text in a virtualized list (not an editable surface) drive THIS, then map `IToken.Scopes` + `Registry.GetTheme().Match(scopes)` to a brush, instead of mounting an editor.

[GRAMMAR_TOKENIZE_ENTRYPOINTS]: `IGrammar` line tokenization and state carry-forward
- rail: tokenizer

| [INDEX] | [SURFACE]            | [SIGNATURE]                                                                              | [RAIL]            |
| :-----: | :------------------- | :-------------------------------------------------------------------------------------- | :---------------- |
|  [01]   | `TokenizeLine`       | `ITokenizeLineResult TokenizeLine(LineText lineText)`                                    | object tokens     |
|  [02]   | `TokenizeLine` (continued) | `ITokenizeLineResult TokenizeLine(LineText lineText, IStateStack prevState, TimeSpan timeLimit)` | multi-line tokens |
|  [03]   | `TokenizeLine2`      | `ITokenizeLineResult2 TokenizeLine2(LineText lineText)` / `(lineText, IStateStack, TimeSpan)` | packed tokens    |
|  [04]   | `GetScopeName` / `GetFileTypes` / `IsCompiling` | `string` / `ICollection<string>` / `bool`                                  | grammar metadata  |
|  [05]   | `IToken` projection  | `token.StartIndex`/`token.EndIndex`/`token.Scopes` over `result.Tokens`                  | token → span      |

Feed line N's `result.RuleStack` (an `IStateStack`) as line N+1's `prevState` to tokenize a document line-by-line with correct multi-line construct continuation; the `TimeSpan timeLimit` bounds a pathological line. `TokenizeLine2`'s `int[]` packs color metadata per the VS Code binary scheme for hosts that resolve color from the packed value rather than re-matching `Scopes` against the theme.

[THEME_NAMES]: the 21 bundled `ThemeName` cases (`TextMateSharp.Grammars`)
- rail: tokenizer

`Abbys` (sic), `Dark`, `DarkPlus`, `DimmedMonokai`, `KimbieDark`, `Light`, `LightPlus`, `OneDark`, `Monokai`, `QuietLight`, `Red`, `SolarizedDark`, `SolarizedLight`, `TomorrowNightBlue`, `HighContrastLight`, `HighContrastDark`, `Dracula`, `AtomOneLight`, `AtomOneDark`, `VisualStudioLight`, `VisualStudioDark`.

Enum order is the load-bearing declaration order (note `Abbys` is the genuine spelling and `OneDark` precedes `Monokai`, not alphabetical); `DarkPlus`/`LightPlus` are the VS Code default dark/light themes. `LoadTheme(ThemeName.X)` maps each to its embedded JSON; an unmapped case returns `null`.

[BUNDLED_GRAMMARS]: the 50 grammars `RegistryOptions` pre-registers (`TextMateSharp.Grammars`)
- rail: tokenizer

`Asciidoc`, `Bat`, `Clojure`, `CoffeeScript`, `Cpp`, `CSharp`, `CSS`, `Dart`, `Diff`, `Docker`, `FSharp`, `Git`, `Go`, `Groovy`, `HandleBars`, `HLSL`, `HTML`, `Ini`, `Java`, `Javascript`, `Json`, `Julia`, `Latex`, `Less`, `Log`, `Lua`, `Make`, `MarkdownBasics`, `MarkdownMath`, `ObjectiveC`, `Pascal`, `Perl`, `PHP`, `PowerShell`, `Pug`, `Python`, `R`, `Razor`, `Ruby`, `Rust`, `SCSS`, `ShaderLab`, `ShellScript`, `SQL`, `Swift`, `TypescriptBasics`, `Typst`, `VB`, `XML`, `YAML`.

The corpus that matters for this workspace: `CSharp` (`source.cs`), `Cpp`/`HLSL`/`ShaderLab` (shader/GLSL-adjacent scopes), `Json` (`source.json`), `Python`/`Rust`/`FSharp`, `Log` (`Log` grammar → log-line scopes, the basis for colorizing host/build output in livedata), and `MarkdownBasics`/`MarkdownMath`. A scope absent from this list (the Rasm-DSL `source.rasm`/`source.rasm-expression`) must be added by a custom `IRegistryOptions.GetGrammar` or `LoadFromLocalFile`.

[MODEL_ENTRYPOINTS]: standalone `TMModel` incremental tokenization (`TextMateSharp.Model`)
- rail: tokenizer

| [INDEX] | [SURFACE]                          | [SIGNATURE]                                                                 | [RAIL]            |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------- | :---------------- |
|  [01]   | `.ctor` + `SetGrammar`             | `new TMModel(IModelLines lines)`; `void SetGrammar(IGrammar grammar)`       | model init        |
|  [02]   | `AddModelTokensChangedListener`    | `void AddModelTokensChangedListener(IModelTokensChangedListener listener)`  | delta subscribe   |
|  [03]   | `GetLineTokens`                    | `List<TMToken> GetLineTokens(int lineIndex)`                               | cached line tokens |
|  [04]   | `InvalidateLine` / `InvalidateLineRange` | `void InvalidateLine(int)` / `void InvalidateLineRange(int ini, int end)` | re-queue after edit |
|  [05]   | `ForceTokenization`                | `void ForceTokenization(int lineIndex)` / `(int start, int end)`           | synchronous tokenize |
|  [06]   | `IsLineInvalid` / `Dispose`        | `bool IsLineInvalid(int)` / `void Dispose()`                              | state + teardown  |

`TMModel` is the off-UI-thread engine: register a listener, call `SetGrammar`, then read `GetLineTokens` as `ModelTokensChanged` ranges arrive; after an edit call `InvalidateLineRange` to re-queue the affected span. The AvaloniaEdit.TextMate `TextEditorModel`/`DocumentSnapshot` (catalogued in `api-avaloniaedit.md`) is precisely an `IModelLines`/`IModelTokensChangedListener` adapter over the editor's `TextDocument` — drive `TMModel` directly only for a non-editor virtualized surface.

## [04]-[INTEGRATION_LAW]

[LOCATOR_RAIL_LAW]:
- Stack: tokenization config is one `IRegistryOptions`. The bundled path is `new RegistryOptions(ThemeName.DarkPlus)` (50 grammars + 21 themes ready); the Rasm-DSL path either (a) implements the four members — `GetGrammar(scope)` returns the raw `source.rasm`/`source.rasm-expression` grammar, `GetTheme`/`GetDefaultTheme` return a `RegistryOptions`-loaded `IRawTheme`, `GetInjections` returns `null` — or (b) calls `RegistryOptions.LoadFromLocalFile` to ingest a file-backed grammar extension. One locator instance owns all scopes the app tokenizes.
- Accept: every grammar/theme handle flows from one `IRegistryOptions`; scope strings are corpus scopes (`"source.cs"`, `"source.json"`) or registered custom scopes (`"source.rasm"`); themes are `ThemeName` cases resolved through `LoadTheme`.
- Reject: a second locator per scope; hardcoded color literals where `Theme.Match`/`GetColor` resolves the scope; treating `IRegistryOptions`/`ThemeName`/`IRawTheme` as AvaloniaEdit types (they are `TextMateSharp` — `api-avaloniaedit.md`'s `InstallTextMate`/`SetTheme`/`SetGrammar` only forward them).

[EDITOR_STACK_LAW]:
- Stack: when the surface IS an editable code pane, the locator feeds `editor.InstallTextMate(registryOptions)` (catalogued in `api-avaloniaedit.md`) -> `installation.SetGrammar(registryOptions.GetScopeByExtension(".cs"))` -> `installation.SetTheme(registryOptions.LoadTheme(ThemeName.DarkPlus))`. AvaloniaEdit.TextMate's `TextEditorModel` wraps `TMModel` over the editor's `TextDocument`, so the host never touches `TMModel`/`IModelLines` directly — it only supplies the `IRegistryOptions` and reacts to the `AppliedTheme` event for chrome alignment via `TryGetThemeColor` (which reads `Theme.GetGuiColorDictionary`).
- Accept: editor tokenization rides one `TextMate.Installation` over an `IRegistryOptions`; custom scopes register on the SAME locator the editor installs.
- Reject: instantiating a separate `Registry`/`TMModel` alongside an `InstallTextMate` editor (the installation already owns one) — the standalone `Registry`/`TMModel` rail is for NON-editor surfaces only.

[STANDALONE_TOKEN_RAIL_LAW]:
- Stack: when the surface is read-only and NOT an `AvaloniaEdit` editor (a virtualized log/livedata list, an inspector value preview), tokenize through `new Registry(registryOptions).LoadGrammar(scope)` then either `IGrammar.TokenizeLine(line)` per line (carrying `RuleStack` forward) for a one-shot pass, or a `TMModel` over an `IModelLines` for an incremental large source. Map `IToken.Scopes` to a brush via `registry.GetTheme().Match(scopes)` -> `GetColor(id)` (+ the `FontStyle` flags), so the SAME `ThemeName` palette the editor uses paints the non-editor surface — no second color table.
- Accept: non-editor syntax coloring derives from the shared locator + `Theme.Match`; multi-line state carries via `IStateStack`; the native `Onigwrap` engine ships with the app (no managed fallback).
- Reject: a hand-rolled regex tokenizer for log/DSL coloring where a bundled or custom TextMate grammar exists; re-deriving color from raw scope strings instead of `Theme.Match`; assuming a managed regex path when `Onigwrap`'s native binary is absent (tokenization fails without it).
