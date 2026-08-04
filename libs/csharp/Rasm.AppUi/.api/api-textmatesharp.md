# [RASM_APPUI_API_TEXTMATESHARP]

`TextMateSharp` ports the VS Code TextMate tokenizer to .NET: a `Registry` drives an `IGrammar` over a line into scope-tagged `IToken` runs or a binary-packed `int[]`, a `Theme` resolves each scope stack to token paint and exposes its VS Code chrome key map, and `TMModel` re-tokenizes a line-list off the UI thread. `TextMateSharp.Grammars` ships the bundled grammar and theme corpus behind `RegistryOptions`, the reference `IRegistryOptions` locator, and the VS Code grammar-extension model.

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

`IRegistryOptions` is the entire seam a host answers, and it is exactly four members: `GetTheme(string scopeName)`, `GetGrammar(string scopeName) -> IRawGrammar`, `GetInjections(string scopeName) -> ICollection<string>`, and `GetDefaultTheme()`.

- `RegistryOptions` implements those four over the embedded corpus; a Rasm-DSL host composes over one and answers `GetGrammar` from its own rows for its `source.rasm` scopes, delegating the rest.
- Corpus-only members — `GetScopeByExtension`, `GetScopeByLanguageId`, `GetLanguageByExtension`, `GetAvailableLanguages`, `LoadTheme`, the local loaders — live on the concrete class, so a composing locator holds that instance rather than a bare interface.
- `IRawTheme` is likewise exactly five members — `GetName()`, `GetInclude()`, `GetSettings()`, `GetTokenColors()`, `GetGuiColors()` — so a host implements it as a DECORATOR over a corpus theme, forwarding the token rules and substituting `GetGuiColors()` to own every chrome key rather than inheriting the corpus's partial, per-theme-divergent coverage.

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

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [CAPABILITY]              |
| :-----: | :--------------------- | :------------- | :------------------------ |
|  [01]   | `IRawTheme`            | interface      | parsed theme document     |
|  [02]   | `Theme`                | class          | compiled scope-trie theme |
|  [03]   | `IRawThemeSetting`     | interface      | raw theme rule            |
|  [04]   | `IThemeSetting`        | interface      | compiled theme rule       |
|  [05]   | `ThemeTrieElementRule` | class          | one matched rule          |
|  [06]   | `FontStyle`            | `[Flags]` enum | font style mask           |
|  [07]   | `IThemeProvider`       | interface      | theme match port          |

`RegistryOptions.LoadTheme(ThemeName)` returns the uncompiled `IRawTheme`; `Theme.CreateFromRawTheme` compiles it into a scope-trie, `Theme.Match(scopeStack)` returns matching `ThemeTrieElementRule` rows — this theme's ahead of the included theme's, each block walking the scope stack most-specific first — and `GetColor(id)` resolves a color id to a hex string. Each row exposes `foreground`, `background`, `fontStyle`, `scopeDepth`, and `parentScopes`, folding to the first nonzero value per axis. `FontStyle` masks `NotSet` (-1), `None` (0), `Italic`, `Bold`, `Underline`, `Strikethrough`.

`Theme.GetGuiColorDictionary() -> ReadOnlyDictionary<string, string>` carries the whole VS Code chrome map, key to hex string, merging the theme's own `colors` block over that of the theme it includes. Coverage is theme-authored and diverges across the bundled corpus: `DarkPlus` inherits `dark_vs` and declares `"editor.background"` with no selection or line-number key, while `Monokai` spells the gutter `"editorLineNumber.foreground"` where `OneDark` spells the same pixel `"editor.lineNumber.foreground"`. Every read branches on the miss, and the consumer's fallback owns that pixel.

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

`LanguageConfiguration` drives editor behavior beyond color and carries eight settable members: `Comments`, `Brackets : IList<string>[]`, `AutoClosingPairs`, `SurroundingPairs : IList<char>[]`, `IndentationRules : Indentation`, `EnterRules`, `Folding`, and `AutoCloseBefore : string`.

`RegistryOptions.GetLanguageByExtension(".cs").Configuration` reaches it for any bundled language without re-parsing JSON; `GrammarDefinition.Parse`, `LanguageConfiguration.Load(grammarName, configurationFile)`, `LoadFromLocal(configurationFile)`, and `Parse(json)` ingest a raw extension, each returning null for an absent or unparsable source.

- `Comments`: `LineComment : string`, `BlockComment : IList<string>` (open at index 0, close at 1) — the whole comment-toggle input.
- `AutoClosingPairs`: `CharPairs : IList<char>[]` for the plain two-character rows and `AutoPairs : AutoPair[]` for the object rows, each `AutoPair` carrying `Open`, `Close`, and `NotIn : IList<string>` — the scope roster a pair must NOT close inside. `AutoCloseBefore` is the character set a pair may close before.
- `Folding`: `OffSide : bool`, `Markers`, and the derived `IsEmpty`; `Markers` carries `Start`/`End` regex strings feeding marker-based folding.
- `Indentation`: `Increase`, `Decrease`, `Unindent` regex strings (each defaulting to empty) and the derived `IsEmpty`; the JSON reader accepts a bare string or a `{ "pattern": … }` object at each slot.
- `EnterRules`: `Rules : IList<EnterRule>`, each `EnterRule` carrying `BeforeText`, `AfterText`, `ActionIndent` (`"indent"`, `"indentOutdent"`, `"outdent"`, `"none"`), and `AppendText`.
- `Language`: `Id`, `Extensions`, `Aliases`, `MimeTypes`, `ConfigurationFile`, and the parsed `Configuration` — so a language-id lookup and an extension lookup reach the same configuration.

[MODEL_TYPE_SCOPE]: the background incremental tokenizer (`TextMateSharp.Model`)

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :---------------------------- | :------------ | :------------------- |
|  [01]   | `ITMModel`                    | interface     | model contract       |
|  [02]   | `TMModel`                     | class         | background tokenizer |
|  [03]   | `IModelLines`                 | interface     | line source          |
|  [04]   | `IModelTokensChangedListener` | interface     | delta listener       |
|  [05]   | `ITokenizationSupport`        | interface     | tokenize port        |

`TMModel` runs a background `TokenizerThread` that revalidates invalidated lines and emits `ModelTokensChangedEvent` ranges to listeners; `GetLineTokens(lineIndex)` reads cached tokens, `InvalidateLineRange` re-queues a span after an edit. Incremental tokenization without an editor drives `TMModel` directly over the host's own `IModelLines`.

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
|  [12]   | `LoadFromLocalDir(string dirPath, bool overwrite = false)`           | instance | custom grammar dir  |
|  [13]   | `LoadFromLocalFile(string, string \| FileInfo, bool overwrite = false)` | instance | custom grammar file |

`new RegistryOptions(ThemeName.DarkPlus)` is the construction the editor stack passes to `InstallTextMate`; `GetScopeByExtension(".cs")` yields the scope `SetGrammar` selects, and `GetScopeByLanguageId("csharp")` answers the same question from a fence's declared language.

Both lookups return an empty or null string for an unknown token, so a scope resolution branches on the miss rather than handing `SetGrammar` a scope no grammar answers. `LoadFromLocalDir`/`LoadFromLocalFile` ingest a VS Code grammar-extension folder or `package.json`, so a file-backed Rasm-DSL grammar registers from disk without a custom `IRegistryOptions`; the `FileInfo` overload takes the manifest directly and `overwrite` replaces an already-registered grammar of the same name.

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

`new Registry(registryOptions).LoadGrammar("source.cs").TokenizeLine(line)` is the complete non-editor tokenization rail — scope-tagged `IToken` runs over a string with no editor control. `GrammarForScopeName` carries an overload taking `initialLanguage` and an embedded-language map for grammar embedding. Virtualized log and inspector surfaces resolve `IToken.Scopes` through `GetTheme().Match(scopes)` against one id-keyed brush cache seeded from `GetColorMap`, reusing the editor palette.

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

[THEME_ENTRY_SCOPE]: compiled-theme resolution and the chrome key map (`TextMateSharp.Themes`)

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]     |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------- |
|  [01]   | `Theme.CreateFromRawTheme(IRawTheme, IRegistryOptions) -> Theme`        | static   | theme compile    |
|  [02]   | `Theme.Match(IList<string>) -> List<ThemeTrieElementRule>`              | instance | scope resolution |
|  [03]   | `Theme.GetColor(int) -> string`                                         | instance | id to hex        |
|  [04]   | `Theme.GetColorId(string) -> int`                                       | instance | hex to id        |
|  [05]   | `Theme.GetColorMap() -> ICollection<string>`                            | instance | full palette     |
|  [06]   | `Theme.GetGuiColorDictionary() -> ReadOnlyDictionary<string, string>`   | instance | chrome key map   |
|  [07]   | `IRawTheme.GetGuiColors() -> ICollection<KeyValuePair<string, object>>` | instance | raw chrome block |
|  [08]   | `IRawTheme.GetTokenColors() -> ICollection<IRawThemeSetting>`           | instance | raw token rules  |
|  [09]   | `IRawTheme.GetSettings() -> ICollection<IRawThemeSetting>`              | instance | raw legacy rules |
|  [10]   | `IRawTheme.GetInclude() -> string`                                      | instance | base-theme path  |
|  [11]   | `IRawTheme.GetName() -> string`                                         | instance | theme metadata   |

- `GetGuiColorDictionary` is the chrome entrypoint every non-token pixel resolves through: `CreateFromRawTheme` merges the included theme's `colors` block first and the source theme's over it, so one lookup answers for the whole include chain. Each call wraps the same backing map in a fresh `ReadOnlyDictionary`, so a host hoists one reference per applied theme.
- `Theme.Match` returns rules, not colors: fold to the first nonzero `foreground`, `background`, and `fontStyle`, then push each color id through `GetColor` — a zero id means the rule set left that axis to the editor default.
- `GetColor` linear-scans the color table per call and `GetColorId` registers an unseen color as a new id, so a per-token call both costs O(colors) and grows the table; build one id-keyed brush cache per applied theme and read the cache thereafter.
- `GetColorMap` returns the distinct uppercase hex strings the theme registered — ids run 1-based in registration order with 0 reserved for unset — so one enumeration seeds that cache whole.

[THEME_NAMES]: the bundled `ThemeName` cases in declaration order (`TextMateSharp.Grammars`)

`Abbys` `Dark` `DarkPlus` `DimmedMonokai` `KimbieDark` `Light` `LightPlus` `OneDark` `Monokai` `QuietLight` `Red` `SolarizedDark` `SolarizedLight` `TomorrowNightBlue` `HighContrastLight` `HighContrastDark` `Dracula` `AtomOneLight` `AtomOneDark` `VisualStudioLight` `VisualStudioDark`

`Abbys` is the genuine spelling and `OneDark` precedes `Monokai`; `DarkPlus`/`LightPlus` are the VS Code default dark and light themes. `LoadTheme(ThemeName.X)` maps each to its embedded JSON, and an unmapped case returns `null`.

[BUNDLED_GRAMMARS]: the grammars `RegistryOptions` pre-registers (`GrammarNames`, `TextMateSharp.Grammars`)

`Asciidoc` `Bat` `Clojure` `CoffeeScript` `Cpp` `CSharp` `CSS` `Dart` `Diff` `Docker` `FSharp` `Git` `Go` `Groovy` `HandleBars` `HLSL` `HTML` `Ini` `Java` `Javascript` `Json` `Julia` `Latex` `Less` `Log` `Lua` `Make` `MarkdownBasics` `MarkdownMath` `ObjectiveC` `Pascal` `Perl` `PHP` `PowerShell` `Pug` `Python` `R` `Razor` `Ruby` `Rust` `SCSS` `ShaderLab` `ShellScript` `SQL` `Swift` `TypescriptBasics` `Typst` `VB` `XML` `YAML`

Workspace-relevant scopes cover `CSharp` (`source.cs`), `Cpp`/`HLSL`/`ShaderLab` (shader-adjacent), `Json`, `Python`/`Rust`/`FSharp`, `Log` (host/build-output coloring in livedata), and `MarkdownBasics`/`MarkdownMath`. Scopes absent from this list — the Rasm-DSL `source.rasm`/`source.rasm-expression` — register through a custom `IRegistryOptions.GetGrammar` or `LoadFromLocalFile`.

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
- One compiled `Theme` answers two independent planes: `Match`/`GetColor` resolve a scope stack to token paint, and `GetGuiColorDictionary` resolves a VS Code chrome key to a hex string. Themes author the chrome plane partially, so every consumer pairs each key read with its own fallback.

[STACKING]:
- `api-avaloniaedit`(`.api/api-avaloniaedit.md`): `AvaloniaEdit.TextMate.InstallTextMate(IRegistryOptions)` consumes this catalog's `IRegistryOptions`, scope strings, and `IRawTheme` handles unchanged; `RegistryOptions`/`GetScopeByExtension`/`LoadTheme`/`ThemeName` are `TextMateSharp` types the adapter only forwards, and its `TextEditorModel`/`DocumentSnapshot` adapt `TMModel` over the editor `TextDocument`.
- `api-avaloniaedit`(`.api/api-avaloniaedit.md`): `TextMate.Installation.TryGetThemeColor(key, out hex)` is the adapter's whole view of `Theme.GetGuiColorDictionary()`, refreshed on each `SetTheme`; a `true` return parses to a brush the consumer writes onto `TextView.CurrentLineBackground`, `TextArea.SelectionBrush`, `TextEditor.LineNumbersForeground`, and the rest of that styled-property set, because `TextMateColoringTransformer` paints token spans alone.
- editor rail: an editable pane feeds one locator to `editor.InstallTextMate(registryOptions)` → `SetGrammar(GetScopeByExtension(ext))` → `SetTheme(LoadTheme(ThemeName.DarkPlus))`; the installation owns its `TMModel`, so the host supplies only the `IRegistryOptions` and answers `AppliedTheme` by rewriting the chrome property set from `TryGetThemeColor`.
- standalone rail: a read-only surface (virtualized log, inspector preview) drives `new Registry(registryOptions).LoadGrammar(scope).TokenizeLine(line)` per line carrying `RuleStack` forward, or a `TMModel` over its own `IModelLines` for an incremental large source, resolving `IToken.Scopes` through `GetTheme().Match(scopes)` against one id-keyed brush cache and folding the `FontStyle` flags onto the same palette.

[LOCAL_ADMISSION]:
- Custom scopes (`source.rasm`, `source.rasm-expression`) register on the same locator the app installs: implement the four `IRegistryOptions` members, or `LoadFromLocalFile` a file-backed grammar extension.

[RAIL_LAW]:
- Package: `TextMateSharp`, `TextMateSharp.Grammars`
- Owns: TextMate tokenization — grammar resolution, scope-tagged token runs, scope-to-color theming, the theme's VS Code chrome key map, and off-thread incremental re-tokenization.
- Accept: every grammar and theme handle from one `IRegistryOptions`; multi-line state via `IStateStack`; one id-keyed brush cache per applied theme; every chrome key read as an optional lookup behind a consumer fallback; the native `Onigwrap` binary shipped with the app.
- Reject: a second locator per scope; a hand-rolled regex tokenizer where a bundled or custom TextMate grammar exists; hardcoded color literals where `Theme.Match`/`GetColor` resolves the scope; a `GetColor` call per token where the brush cache answers; a chrome key treated as guaranteed across themes; a separate `Registry`/`TMModel` alongside an `InstallTextMate` editor, which already owns one.
