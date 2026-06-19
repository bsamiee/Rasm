# [RASM_APPHOST_API_COMMANDLINE]

`System.CommandLine` supplies CLI parsing, command trees, typed options and arguments, tab completion, invocation actions, parse result inspection, and `ParseResult`-driven synchronous and asynchronous dispatch for AppHost entry-point consumers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.CommandLine`
- package: `System.CommandLine`
- assembly: `System.CommandLine`
- namespace: `System.CommandLine`
- asset: runtime library
- rail: configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: command and symbol family
- rail: configuration

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :------------------------------ | :--------------- | :------------------------------ |
|  [01]   | `Symbol`                        | base symbol      | name, description, completions  |
|  [02]   | `Command`                       | command node     | subcommands, options, arguments |
|  [03]   | `RootCommand`                   | root command     | application entry point         |
|  [04]   | `Option`                        | option symbol    | name, type, arity, aliases      |
|  [05]   | `Argument`                      | argument symbol  | positional value, arity         |
|  [06]   | `Directive`                     | directive symbol | `[directive]` prefix token      |
|  [07]   | `VersionOption`                 | version option   | `--version` built-in            |
|  [08]   | `DiagramDirective`              | parse diagram    | `[diagram]` debug directive     |
|  [09]   | `EnvironmentVariablesDirective` | env directive    | `[env]` env-variable directive  |

[PUBLIC_TYPE_SCOPE]: parse and invocation family
- rail: configuration

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                        |
| :-----: | :------------------------ | :---------------- | :---------------------------- |
|  [01]   | `ParseResult`             | parse outcome     | value access, errors, tokens  |
|  [02]   | `ParserConfiguration`     | parser options    | enable/disable behaviors      |
|  [03]   | `InvocationConfiguration` | invocation policy | exception handling, output    |
|  [04]   | `ArgumentArity`           | arity policy      | zero/one/many bounds          |
|  [05]   | `CommandLineAction`       | action base       | sync/async invocation handler |
|  [06]   | `CommandResult`           | command result    | per-command validation target |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbol construction
- rail: configuration

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :--------------------------------- | :------------- | :------------------------ |
|  [01]   | `Command(name, description?)`      | command ctor   | named command node        |
|  [02]   | `RootCommand(description?)`        | root ctor      | top-level entry point     |
|  [03]   | `Option<T>(name, description?)`    | option ctor    | typed option symbol       |
|  [04]   | `Argument<T>(name?, description?)` | argument ctor  | typed positional argument |

[ENTRYPOINT_SCOPE]: command composition
- rail: configuration

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------- | :------------- | :------------------------- |
|  [01]   | `Command.Add(Argument argument)`       | child add      | positional argument add    |
|  [02]   | `Command.Add(Option option)`           | child add      | option add                 |
|  [03]   | `Command.Add(Command command)`         | child add      | subcommand add             |
|  [04]   | `Command.Arguments`                    | child list     | `IList<Argument>` accessor |
|  [05]   | `Command.Options`                      | child list     | `IList<Option>` accessor   |
|  [06]   | `Command.Subcommands`                  | child list     | `IList<Command>` accessor  |
|  [07]   | `Command.Aliases`                      | alias set      | `ICollection<string>`      |
|  [08]   | `Command.TreatUnmatchedTokensAsErrors` | policy flag    | default `true`             |

[ENTRYPOINT_SCOPE]: action binding
- rail: configuration

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :------------------------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `Command.SetAction(Action<ParseResult>)`                             | sync action     | void synchronous handler      |
|  [02]   | `Command.SetAction(Func<ParseResult, int>)`                          | sync action     | exit-code synchronous handler |
|  [03]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task>)`      | async action    | preferred async handler       |
|  [04]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` | async action    | preferred exit-code async     |
|  [05]   | `Command.Action`                                                     | action property | `CommandLineAction?` get/set  |
|  [06]   | `Command.Validators`                                                 | validator list  | `List<Action<CommandResult>>` |

[ENTRYPOINT_SCOPE]: parse and invoke
- rail: configuration

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Command.Parse(IReadOnlyList<string>, config?)` | parse          | `ParseResult` from args         |
|  [02]   | `Command.Parse(string, config?)`                | parse          | `ParseResult` from command line |
|  [03]   | `ParseResult.GetValue<T>(Option<T>)`            | value access   | typed option value              |
|  [04]   | `ParseResult.GetValue<T>(Argument<T>)`          | value access   | typed argument value            |
|  [05]   | `ParseResult.Errors`                            | error list     | parse error collection          |
|  [06]   | `ParseResult.GetResult(Symbol)`                 | result lookup  | symbol parse result node        |

## [04]-[IMPLEMENTATION_LAW]

[COMMANDLINE_TOPOLOGY]:
- public namespaces: `System.CommandLine`, `.Parsing`, `.Invocation`, `.Completions`
- symbol hierarchy: `RootCommand` → `Command` → `Option`/`Argument`/`Command` trees
- dispatch: `SetAction` stores a `CommandLineAction`; parse then invokes the matched command action
- arity: `ArgumentArity.Zero`, `ZeroOrOne`, `ExactlyOne`, `ZeroOrMore`, `OneOrMore`
- `TreatUnmatchedTokensAsErrors` defaults to `true` on every `Command`
- `ParseResult.Errors` carries `ParseError` instances; non-empty errors prevent successful invocation by default

[LOCAL_ADMISSION]:
- Build the `RootCommand` tree at composition time; bind typed `Option<T>` and `Argument<T>` to configuration consumers through `ParseResult.GetValue<T>` inside `SetAction` delegates.
- Use `SetAction(Func<ParseResult, CancellationToken, Task<int>>)` as the preferred async entry point.
- Validators on `Command.Validators` run after parsing and before invocation; use them for cross-option constraints.

[RAIL_LAW]:
- Package: `System.CommandLine`
- Owns: CLI argument parsing and command dispatch
- Accept: `RootCommand`-rooted parse tree; typed `Option<T>` and `Argument<T>` surfaces
- Reject: hand-rolled `args` parsing or positional-index argument access without `Argument<T>` binding
