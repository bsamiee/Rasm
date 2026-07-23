# [RASM_APPUI_API_TEXTMATESHARP]

`TextMateSharp` ports the VS Code TextMate tokenizer to .NET: a `Registry` drives an `IGrammar` over a line into scope-tagged `IToken` runs (or a binary-packed `int[]`), a `Theme` resolves each scope stack to a color and `FontStyle`, and `TMModel` re-tokenizes a line-list off the UI thread. `TextMateSharp.Grammars` ships the bundled grammar and theme corpus behind `RegistryOptions`, the reference `IRegistryOptions` locator, and the VS Code grammar-extension model. Native regex binds `Onigwrap` (Oniguruma); no managed path exists. Every tokenization flows from one `IRegistryOptions` locator.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TextMateSharp`
- package: `TextMateSharp` (MIT)
- assembly: `TextMateSharp`
- target: `netstandard2.0`
- namespace: `TextMateSharp.Registry` locator+engine, `TextMateSharp.Grammars` tokenize surface, `TextMateSharp.Themes` color resolution, `TextMateSharp.Model` background tokenizer
- rail: tokenizer
- depends: `Onigwrap` — Oniguruma native regex; the `IGrammar` match loop is native, and the native binary ships with the app.

[PACKAGE_SURFACE]: `TextMateSharp.Grammars`
- package: `TextMateSharp.Grammars` (MIT)
- assembly: `TextMateSharp.Grammars`
- target: `netstandard2.0`
- namespace: `TextMateSharp.Grammars`
- asset: embedded grammar and theme JSON resources
- rail: tokenizer
- depends: `TextMateSharp` — supplies `IRegistryOptions`, `IRawTheme`, and the tokenize and model surface.

## [02]-[PUBLIC_TYPES]

[REGISTRY_TYPE_SCOPE]: the locator contract, its bundled reference implementation, and the tokenizer engine

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :----------------- | :------------ | :------------------------------- |
|  [01]   | `IRegistryOptions` | interface     | four-member locator contract     |
|  [02]   | `RegistryOptions`  | class         | bundled-corpus reference locator |
|  [03]   | `Registry`         | class         | standalone tokenizer engine      |
|  [04]   | `ThemeName`        | enum          | bundled theme key                |

`IRegistryOptions` is the entire seam a host answers: `GetGrammar` returns the scope's grammar, `GetTheme`/`GetDefaultTheme` a theme, `GetInjections` its injections. `RegistryOptions` implements it over the embedded corpus; a Rasm-DSL host implements the four members itself or composes over a `RegistryOptions` and overrides `GetGrammar` for its own `source.rasm` scopes.

[GRAMMAR_TYPE_SCOPE]: the scope-tagged tokenize surface `Registry.LoadGrammar(scope)` returns

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                   |
| :-----: | :--------------------- | :-------------- | :----------------------------- |
|  [01]   | `IGrammar`             | interface       | grammar handle, line tokenizer |
|  [02]   | `IToken`               | interface       | scope-tagged token run         |
|  [03]   | `ITokenizeLineResult`  | interface       | object token result            |
|  [04]   | `ITokenizeLineResult2` | interface       | packed token result            |
|  [05]   | `IStateStack`          | interface       | line-continuation state        |
|  [06]   | `LineText`             | readonly struct | zero-copy line input           |
|  [07]   | `IGrammarRepository`   | interface       | grammar-resolution wiring      |

`IToken.Scopes` is the full scope stack at a span (`["source.cs","keyword.control.cs"]`); `TokenizeLine` returns `IToken[]`, `TokenizeLine2` the VS Code binary encoding packing foreground, background, and `FontStyle` per `int`. Feeding line N's `result.RuleStack` as line N+1's `prevState` carries block comments and string interpolation across lines. `LineText` converts implicitly from `string` or `ReadOnlyMemory<char>`, so the `ReadOnlyMemory<char>` overload tokenizes a rope slice without a substring copy.

[THEME_TYPE_SCOPE]: scope-to-color resolution (`TextMateSharp.Themes`)

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]              |
| :-----: | :----------------- | :------------- | :------------------------ |
|  [01]   | `IRawTheme`        | interface      | parsed theme document     |
|  [02]   | `Theme`            | class          | compiled scope-trie theme |
|  [03]   | `IRawThemeSetting` | interface      | raw theme rule            |
|  [04]   | `IThemeSetting`    | interface      | compiled theme rule       |
|  [05]   | `FontStyle`        | `[Flags]` enum | font style mask           |
|  [06]   | `IThemeProvider`   | interface      | theme match port          |

`RegistryOptions.LoadTheme(ThemeName)` returns the uncompiled `IRawTheme`; `Theme.CreateFromRawTheme` compiles it against the locator into a scope-trie, `Theme.Match(scopeStack)` returns the winning rule (color id and `FontStyle`), and `GetColor(id)` resolves the id to a hex string. `FontStyle` masks `NotSet` (-1), `None` (0), `Italic`, `Bold`, `Underline`, `Strikethrough`. `GetGuiColorDictionary` exposes editor-chrome colors (`"editor.background"`) so a host paints surrounding UI to match the grammar theme.

[CORPUS_MODEL_TYPE_SCOPE]: the VS Code grammar-extension model `RegistryOptions` decodes

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :---------------------- | :------------ | :--------------------------- |
|  [01]   | `GrammarDefinition`     | class         | grammar-extension manifest   |
|  [02]   | `Contributes`           | class         | language/grammar/snippet set |
|  [03]   | `Language`              | class         | language row                 |
|  [04]   | `Grammar`               | class         | grammar row                  |
|  [05]   | `LanguageConfiguration` | class         | editor-behavior model        |
|  [06]   | `Comments`              | class         | comment markers              |
|  [07]   | `Folding`               | class         | fold markers                 |
|  [08]   | `Indentation`           | class         | indentation rules            |
|  [09]   | `EnterRules`            | class         | on-enter rules               |
|  [10]   | `AutoClosingPairs`      | class         | auto-close pairs             |
|  [11]   | `Markers`               | class         | fold-marker regexes          |
|  [12]   | `LanguageSnippets`      | class         | snippet corpus               |
|  [13]   | `LanguageSnippet`       | class         | snippet row                  |
|  [14]   | `Region`                | class         | snippet body region          |

`LanguageConfiguration` drives editor behavior beyond color: `Comments.LineComment`/`BlockComment` feed a comment-toggle, `Brackets`/`AutoClosingPairs`/`SurroundingPairs` feed bracket matching and auto-close, `Folding.Markers` feeds marker-based folding. `RegistryOptions.GetLanguageByExtension(".cs").Configuration` reaches it for any bundled language without re-parsing JSON; `GrammarDefinition.Parse` and `LanguageConfiguration.Load`/`Parse` ingest a raw extension.

[MODEL_TYPE_SCOPE]: the background incremental tokenizer (`TextMateSharp.Model`)

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :---------------------------- | :------------ | :------------------- |
|  [01]   | `ITMModel`                    | interface     | model contract       |
|  [02]   | `TMModel`                     | class         | background tokenizer |
|  [03]   | `IModelLines`                 | interface     | line source          |
|  [04]   | `IModelTokensChangedListener` | interface     | delta listener       |
|  [05]   | `ITokenizationSupport`        | interface     | tokenize port        |

`TMModel` runs a background `TokenizerThread` that revalidates invalidated lines and emits `ModelTokensChangedEvent` ranges to listeners; `GetLineTokens(lineIndex)` reads cached tokens, `InvalidateLineRange` re-queues a span after an edit. A host wanting incremental tokenization without an editor drives `TMModel` directly over its own `IModelLines`.

## [03]-[ENTRYPOINTS]

[REGISTRY_ENTRY_SCOPE]: `RegistryOptions` corpus query, theme load, and custom-grammar loading

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `RegistryOptions(ThemeName)`                                         | ctor     | corpus locator      |
|  [02]   | `GetScopeByExtension(string) -> string`                              | instance | ext → scope         |
|  [03]   | `GetScopeByLanguageId(string) -> string`                             | instance | id → scope          |
|  [04]   | `GetLanguageByExtension(string) -> Language`                         | instance | ext → language      |
|  [05]   | `GetAvailableLanguages() -> List<Language>`                          | instance | corpus roster       |
|  [06]   | `GetAvailableGrammarDefinitions() -> IEnumerable<GrammarDefinition>` | instance | corpus roster       |
|  [07]   | `LoadTheme(ThemeName) -> IRawTheme`                                  | instance | theme load          |
|  [08]   | `GetGrammar(string) -> IRawGrammar`                                  | instance | locator contract    |
|  [09]   | `GetTheme(string) -> IRawTheme`                                      | instance | locator contract    |
|  [10]   | `GetDefaultTheme() -> IRawTheme`                                     | instance | locator contract    |
|  [11]   | `GetInjections(string) -> ICollection<string>`                       | instance | locator contract    |
|  [12]   | `LoadFromLocalDir(string, bool)`                                     | instance | custom grammar dir  |
|  [13]   | `LoadFromLocalFile(string, string)`                                  | instance | custom grammar file |

`new RegistryOptions(ThemeName.DarkPlus)` is the construction the editor stack passes to `InstallTextMate`; `GetScopeByExtension(".cs")` yields the scope `SetGrammar` selects. `LoadFromLocalDir`/`LoadFromLocalFile` ingest a VS Code grammar-extension folder, so a file-backed Rasm-DSL grammar registers from disk without a custom `IRegistryOptions`.

[REGISTRY_ENGINE_ENTRY_SCOPE]: standalone `Registry` tokenization, no editor

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Registry(IRegistryOptions)`                                   | ctor     | engine init                  |
|  [02]   | `Registry()`                                                   | ctor     | engine over `DefaultLocator` |
|  [03]   | `LoadGrammar(string) -> IGrammar`                              | instance | resolve + compile + inject   |
|  [04]   | `GrammarForScopeName(string) -> IGrammar`                      | instance | embedded grammar             |
|  [05]   | `LoadGrammarFromPathSync(string, int, Dictionary<string,int>)` | instance | file grammar                 |
|  [06]   | `SetTheme(IRawTheme)`                                          | instance | theme state                  |
|  [07]   | `GetTheme() -> Theme`                                          | instance | theme state                  |
|  [08]   | `GetColorMap() -> ICollection<string>`                         | instance | theme state                  |
|  [09]   | `GetLocator() -> IRegistryOptions`                             | instance | locator state                |

`new Registry(registryOptions).LoadGrammar("source.cs").TokenizeLine(line)` is the complete non-editor tokenization rail — scope-tagged `IToken` runs over a string with no editor control. `GrammarForScopeName` carries an overload taking `initialLanguage` and an embedded-language map for grammar embedding. A virtualized log or inspector surface maps `IToken.Scopes` through `GetTheme().Match(scopes)` → `GetColor(id)` to reuse the editor palette.

[GRAMMAR_TOKENIZE_ENTRY_SCOPE]: `IGrammar` line tokenization and state carry-forward

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]      |
| :-----: | :--------------------------------------------------------------------- | :------- | :---------------- |
|  [01]   | `TokenizeLine(LineText) -> ITokenizeLineResult`                        | instance | object tokens     |
|  [02]   | `TokenizeLine(LineText, IStateStack, TimeSpan) -> ITokenizeLineResult` | instance | multi-line tokens |
|  [03]   | `TokenizeLine2(LineText) -> ITokenizeLineResult2`                      | instance | packed tokens     |
|  [04]   | `GetScopeName() -> string`                                             | instance | grammar metadata  |
|  [05]   | `GetFileTypes() -> ICollection<string>`                                | instance | grammar metadata  |
|  [06]   | `IsCompiling`                                                          | property | compile state     |

Feeding line N's `result.RuleStack` as line N+1's `prevState` continues multi-line constructs; `TimeSpan timeLimit` bounds a pathological line, and `TokenizeLine2` carries the same state overload. `TokenizeLine2` packs color metadata per the VS Code binary scheme for hosts resolving color from the packed `int` rather than re-matching `Scopes`.

[THEME_NAMES]: the bundled `ThemeName` cases in declaration order (`TextMateSharp.Grammars`)

`Abbys` `Dark` `DarkPlus` `DimmedMonokai` `KimbieDark` `Light` `LightPlus` `OneDark` `Monokai` `QuietLight` `Red` `SolarizedDark` `SolarizedLight` `TomorrowNightBlue` `HighContrastLight` `HighContrastDark` `Dracula` `AtomOneLight` `AtomOneDark` `VisualStudioLight` `VisualStudioDark`

`Abbys` is the genuine spelling and `OneDark` precedes `Monokai`; `DarkPlus`/`LightPlus` are the VS Code default dark and light themes. `LoadTheme(ThemeName.X)` maps each to its embedded JSON, and an unmapped case returns `null`.

[BUNDLED_GRAMMARS]: the grammars `RegistryOptions` pre-registers (`GrammarNames`, `TextMateSharp.Grammars`)

`Asciidoc` `Bat` `Clojure` `CoffeeScript` `Cpp` `CSharp` `CSS` `Dart` `Diff` `Docker` `FSharp` `Git` `Go` `Groovy` `HandleBars` `HLSL` `HTML` `Ini` `Java` `Javascript` `Json` `Julia` `Latex` `Less` `Log` `Lua` `Make` `MarkdownBasics` `MarkdownMath` `ObjectiveC` `Pascal` `Perl` `PHP` `PowerShell` `Pug` `Python` `R` `Razor` `Ruby` `Rust` `SCSS` `ShaderLab` `ShellScript` `SQL` `Swift` `TypescriptBasics` `Typst` `VB` `XML` `YAML`

Workspace-relevant scopes cover `CSharp` (`source.cs`), `Cpp`/`HLSL`/`ShaderLab` (shader-adjacent), `Json`, `Python`/`Rust`/`FSharp`, `Log` (host/build-output coloring in livedata), and `MarkdownBasics`/`MarkdownMath`. A scope absent from this list — the Rasm-DSL `source.rasm`/`source.rasm-expression` — registers through a custom `IRegistryOptions.GetGrammar` or `LoadFromLocalFile`.

[MODEL_ENTRY_SCOPE]: standalone `TMModel` incremental tokenization (`TextMateSharp.Model`)

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------- | :------- | :----------------- |
|  [01]   | `TMModel(IModelLines)`                                       | ctor     | model init         |
|  [02]   | `SetGrammar(IGrammar)`                                       | instance | model init         |
|  [03]   | `AddModelTokensChangedListener(IModelTokensChangedListener)` | instance | delta subscribe    |
|  [04]   | `GetLineTokens(int) -> List<TMToken>`                        | instance | cached tokens      |
|  [05]   | `InvalidateLine(int)`                                        | instance | re-queue edit      |
|  [06]   | `InvalidateLineRange(int, int)`                              | instance | re-queue span      |
|  [07]   | `ForceTokenization(int)`                                     | instance | sync tokenize      |
|  [08]   | `IsLineInvalid(int) -> bool`                                 | instance | invalidation state |
|  [09]   | `Dispose()`                                                  | instance | teardown           |

Registering a listener, `SetGrammar`, then reading `GetLineTokens` as `ModelTokensChanged` ranges arrive is the off-UI-thread loop; after an edit `InvalidateLineRange` re-queues the span, and `ForceTokenization` carries a start/end range overload.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `IRegistryOptions` owns every scope the app tokenizes; each grammar and theme handle flows from that single locator, scope strings are corpus scopes (`"source.cs"`) or registered custom scopes (`"source.rasm"`), and themes are `ThemeName` cases resolved through `LoadTheme`.

[STACKING]:
- `api-avaloniaedit`(`.api/api-avaloniaedit.md`): `AvaloniaEdit.TextMate.InstallTextMate(IRegistryOptions)` consumes this catalog's `IRegistryOptions`, scope strings, and `IRawTheme` handles unchanged; `RegistryOptions`/`GetScopeByExtension`/`LoadTheme`/`ThemeName` are `TextMateSharp` types the adapter only forwards, and its `TextEditorModel`/`DocumentSnapshot` adapt `TMModel` over the editor `TextDocument`.
- editor rail: an editable pane feeds one locator to `editor.InstallTextMate(registryOptions)` → `SetGrammar(GetScopeByExtension(ext))` → `SetTheme(LoadTheme(ThemeName.DarkPlus))`; the installation owns its `TMModel`, so the host supplies only the `IRegistryOptions` and reacts to the `AppliedTheme` event for chrome alignment through `GetGuiColorDictionary`.
- standalone rail: a read-only surface (virtualized log, inspector preview) drives `new Registry(registryOptions).LoadGrammar(scope).TokenizeLine(line)` per line carrying `RuleStack` forward, or a `TMModel` over its own `IModelLines` for an incremental large source, mapping `IToken.Scopes` through `GetTheme().Match(scopes)` → `GetColor(id)` and the `FontStyle` flags onto the shared palette.

[LOCAL_ADMISSION]:
- A custom scope (`source.rasm`, `source.rasm-expression`) registers on the same locator the app installs: implement the four `IRegistryOptions` members, or `LoadFromLocalFile` a file-backed grammar extension.

[RAIL_LAW]:
- Package: `TextMateSharp`, `TextMateSharp.Grammars`
- Owns: TextMate tokenization — grammar resolution, scope-tagged token runs, scope-to-color theming, and off-thread incremental re-tokenization.
- Accept: every grammar and theme handle from one `IRegistryOptions`; multi-line state via `IStateStack`; the native `Onigwrap` binary shipped with the app.
- Reject: a second locator per scope; a hand-rolled regex tokenizer where a bundled or custom TextMate grammar exists; hardcoded color literals where `Theme.Match`/`GetColor` resolves the scope; a separate `Registry`/`TMModel` alongside an `InstallTextMate` editor, which already owns one.
