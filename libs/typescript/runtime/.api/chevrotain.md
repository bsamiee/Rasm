# [TS_RUNTIME_API_CHEVROTAIN]

`chevrotain` owns lexing and LL(k) parsing for the CESQL subscription filter dialect — the one filter shape whose expression grammar no attribute-comparison row can express. Token vocabulary and rule set declare the grammar as data, the library self-analyzes them into lookahead tables at construction, and a visitor lowers the resulting concrete syntax tree into the branch's own operator, function, and cast rows.

Grammar declaration is the library's one statement-bearing surface: rules are `protected` methods recorded by calling them, so the grammar owner is a class constructed once at module initialization and every surface above it takes token vocabulary, operator table, and error rows as values. Each parser instance carries `input` and `errors` as mutable state, so a filter evaluation binds one instance per fiber rather than sharing one across concurrent admissions.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the token vocabulary, the two parser bases, the syntax tree, and the error families

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `TokenType`                                        | vocabulary row | `PATTERN`/`CATEGORIES`/`LONGER_ALT`/`GROUP`/`LINE_BREAKS`        |
|  [02]   | `ITokenConfig`                                     | mint record    | the `createToken` input; `categories` builds the token hierarchy |
|  [03]   | `IToken`                                           | lexed token    | `image`, `startOffset`, optional line and column, `tokenType`    |
|  [04]   | `TokenVocabulary`                                  | parser input   | `TokenType[]`, a name-keyed dictionary, or a multi-mode set      |
|  [05]   | `ILexerConfig`                                     | lexer posture  | `positionTracking`, `safeMode`, `ensureOptimizations`            |
|  [06]   | `ILexingResult` / `ILexingError`                   | lex outcome    | `tokens`/`groups`/`errors`; errors carry offset, line, length    |
|  [07]   | `CstParser` / `EmbeddedActionsParser`              | parser bases   | tree-producing versus action-producing; both extend `BaseParser` |
|  [08]   | `IParserConfig`                                    | parser posture | `maxLookahead`, `nodeLocationTracking`, `recoveryEnabled`        |
|  [09]   | `CstNode` / `CstElement` / `CstChildrenDictionary` | syntax tree    | `name`, `children` keyed by rule or token, optional `location`   |
|  [10]   | `ICstVisitor<IN, OUT>`                             | lowering       | `visit(node, param?)` beside `validateVisitor()`                 |
|  [11]   | `IRecognitionException`                            | parse fault    | `token`, `resyncedTokens`, `context`; extends `Error`            |
|  [12]   | `IOrAlt<T>` / `IOrAltWithGate<T>`                  | alternative    | `{ ALT }` and the `GATE` predicate the lookahead consults        |
|  [13]   | `IRuleConfig<T>`                                   | rule posture   | per-rule resync and recovery value                               |
|  [14]   | `Rule` / `IProduction` / `GAstVisitor`             | grammar AST    | the recorded grammar as data, walkable without parsing           |

- `LexerDefinitionErrorType` is a TypeScript `enum` the branch never declares; it reaches this surface as a value read off a definition error alone.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: vocabulary minting, lexing, grammar declaration, parsing, and lowering

| [INDEX] | [SURFACE]                                        | [SHAPE]   | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------------- | :-------- | :------------------------------------------------------------------ |
|  [01]   | `createToken(config)`                            | function  | one vocabulary row; `categories` and `longer_alt` build hierarchies |
|  [02]   | `createTokenInstance(...)`                       | function  | a synthetic token, for a caller-built recovery frame                |
|  [03]   | `new Lexer(definition, config?)`                 | ctor      | compiles the vocabulary once; reusable and input-free               |
|  [04]   | `lexer.tokenize(text, initialMode?)`             | instance  | total — errors land in `ILexingResult.errors`, never a throw        |
|  [05]   | `lexer.lexerDefinitionErrors`                    | property  | definition faults when `deferDefinitionErrorsHandling` is set       |
|  [06]   | `new CstParser(vocabulary, config?)`             | ctor      | records the grammar; `performSelfAnalysis()` closes the ctor        |
|  [07]   | `this.RULE(name, impl, config?)`                 | protected | declares one production and returns its callable method             |
|  [08]   | `this.CONSUME(tokType, options?) -> IToken`      | protected | terminal; the numeric suffix family disambiguates occurrences       |
|  [09]   | `this.SUBRULE(rule, options?) -> CstNode`        | protected | non-terminal; mandatory for every rule-to-rule call                 |
|  [10]   | `this.OR(alts)` / `this.OR({ DEF, ... })`        | protected | alternation; `IOrAltWithGate.GATE` narrows an ambiguous choice      |
|  [11]   | `this.MANY` / `AT_LEAST_ONE` / `OPTION`          | protected | repetition and optionality, each with a separator sibling           |
|  [12]   | `this.ACTION(impl)` / `this.BACKTRACK(rule)`     | protected | escape from the recording phase; speculative parse predicate        |
|  [13]   | `parser.input = tokens`                          | property  | binds one token array; the instance is reusable, not reentrant      |
|  [14]   | `parser.errors`                                  | property  | accumulated `IRecognitionException[]` after a rule invocation       |
|  [15]   | `parser.reset()`                                 | instance  | clears the carried state between admissions                         |
|  [16]   | `parser.getBaseCstVisitorConstructor<IN, OUT>()` | instance  | the visitor base whose `visitXYZ` set `validateVisitor` proves      |
|  [17]   | `parser.getGAstProductions()`                    | instance  | the recorded grammar keyed by rule name                             |
|  [18]   | `isRecognitionException(error)`                  | function  | narrows a caught value to the parse-fault family                    |
|  [19]   | `EOF` / `EMPTY_ALT(value?)`                      | const/fn  | the end terminal and the empty alternative                          |
|  [20]   | `serializeGrammar` / `createSyntaxDiagramsCode`  | function  | grammar as data and as a rendered railroad diagram                  |
|  [21]   | `generateCstDts(productions, options?)`          | function  | derives concrete-syntax-tree declarations from the grammar          |
|  [22]   | `clearCache()` / `VERSION`                       | fn/const  | resets the definition cache; the installed version literal          |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Grammar declaration is a RECORDING phase: the constructor calls every rule implementation with no arguments to record it, so a rule body must tolerate absent parameters and must not compute.
- `performSelfAnalysis()` runs the ambiguity, left-recursion, and lookahead analysis and must be the constructor's last statement, so a grammar defect surfaces at module initialization rather than at the first filter.
- `tokenize` is TOTAL and returns `errors` as data, while a rule invocation both returns a partial tree AND accumulates into `parser.errors`, so a parse result is read as the pair rather than as the return value alone.
- `parser.input` and `parser.errors` are mutable instance state, so one instance serves one admission at a time and a shared instance across concurrent admissions interleaves their errors.
- Token ORDER in the lexer definition decides the match: the first matching pattern wins, so a keyword that prefixes an identifier needs `longer_alt` rather than reordering.
- `categories` builds a token hierarchy, so an operator class is one category token every member declares and the grammar consumes the category rather than enumerating members.
- `maxLookahead` defaults low and raising it costs analysis time at construction alone, so an ambiguous alternation resolves by raising it or by an `IOrAltWithGate.GATE`, never by reordering alternatives and hoping.
- `nodeLocationTracking` decides whether `CstNode.location` exists at all and `"full"` alone fills line and column, so a diagnostic quoting a source span declares the posture that produces it.
- `recoveryEnabled` synthesizes tokens and re-syncs, marking `CstNode.recoveredNode`, so a subscription admission that must refuse an unparseable filter leaves recovery off.
- `ensureOptimizations` turns a lexer optimization miss into a definition error rather than a silent slow path, which is how a custom pattern that defeats the start-character index gets caught.
- `getBaseCstVisitorConstructor` yields a base whose `visitXYZ` methods `validateVisitor()` proves complete, so a grammar rule added without its lowering arm fails at initialization.

[STACKING]:
- `core/interchange/carrier`(`core/.planning/interchange/carrier.md`): supplies the attribute grammar and the extension roster a CESQL identifier resolves against, so a missing-attribute error names a rostered attribute rather than an arbitrary key.
- `core/value/fault`(`core/.planning/value/fault.md`): `ILexingError` and `IRecognitionException` both land as `Fault.Class` evidence at the one lowering boundary; a parse failure refuses the SUBSCRIPTION at admission, never a delivery.
- `effect` `Schema`(`.api/effect.md`): the visitor lowers a `CstNode` into an owned expression owner rather than yielding the tree, so no consumer above reads `children` or a `tokenType` name.
- `effect` `Match`(`.api/effect.md`): the operator, function, and implicit-cast rows dispatch off the owned expression tags; the concrete syntax tree never reaches a dispatch surface.
- `effect` `Layer`(`.api/effect.md`): the compiled `Lexer` and grammar owner are constructed once and reach consumers through the requirement channel, so no module-level live instance is imported.
- `work/filter`(`runtime/.planning/work/filter.md`): the `sql` dialect row composes the compiled grammar behind one predicate shape; the remaining six dialects are attribute-comparison rows and reach no parser.

[LOCAL_ADMISSION]:
- Declare the token vocabulary as one contract-checked table and derive the lexer definition array and the parser vocabulary from it, so no roster is spelled twice.
- Construct the `Lexer` and the grammar owner once and provide them through a `Layer`; a per-filter construction repeats the whole self-analysis.
- Bind one parser instance per admission, read `errors` after every rule invocation, and `reset()` between admissions.
- Leave `recoveryEnabled` off and `skipValidations` off, since an unparseable filter refuses at admission and a grammar defect must fail at initialization.
- Set `nodeLocationTracking` to the posture the diagnostic reads, and never quote a span the posture did not fill.
- Accumulate lex errors, parse errors, and lowering errors onto one typed result — CESQL evaluation is total and reports a list, so no arm throws.
- Lower through a `validateVisitor()`-proven visitor, and let no `CstNode`, `IToken`, or `TokenType` escape that boundary.
- Keep `createSyntaxDiagramsCode`, `generateCstDts`, and `EmbeddedActionsParser` out of the branch: the first two are authoring tools and the third trades the provable visitor for inline actions.
