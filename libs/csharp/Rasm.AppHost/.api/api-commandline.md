# [RASM_APPHOST_API_COMMANDLINE]

`System.CommandLine` supplies CLI parsing, command trees, typed options and arguments, tab completion, invocation actions, parse result inspection, and `ParseResult`-driven synchronous and asynchronous dispatch for AppHost entry-point consumers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.CommandLine`
- package: `System.CommandLine`
- assembly: `System.CommandLine`
- namespace: `System.CommandLine`
- asset: runtime library
- rail: configuration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: command and symbol family
- rail: configuration

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :------------------------------ | :--------------- | :------------------------------ |
|   [1]   | `Symbol`                        | base symbol      | name, description, completions  |
|   [2]   | `Command`                       | command node     | subcommands, options, arguments |
|   [3]   | `RootCommand`                   | root command     | application entry point         |
|   [4]   | `Option`                        | option symbol    | name, type, arity, aliases      |
|   [5]   | `Argument`                      | argument symbol  | positional value, arity         |
|   [6]   | `Directive`                     | directive symbol | `[directive]` prefix token      |
|   [7]   | `VersionOption`                 | version option   | `--version` built-in            |
|   [8]   | `DiagramDirective`              | parse diagram    | `[diagram]` debug directive     |
|   [9]   | `EnvironmentVariablesDirective` | env directive    | `[env]` env-variable directive  |

[PUBLIC_TYPE_SCOPE]: parse and invocation family
- rail: configuration

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                        |
| :-----: | :------------------------ | :---------------- | :---------------------------- |
|   [1]   | `ParseResult`             | parse outcome     | value access, errors, tokens  |
|   [2]   | `ParserConfiguration`     | parser options    | enable/disable behaviors      |
|   [3]   | `InvocationConfiguration` | invocation policy | exception handling, output    |
|   [4]   | `ArgumentArity`           | arity policy      | zero/one/many bounds          |
|   [5]   | `CommandLineAction`       | action base       | sync/async invocation handler |
|   [6]   | `CommandResult`           | command result    | per-command validation target |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbol construction
- rail: configuration

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :--------------------------------- | :------------- | :------------------------ |
|   [1]   | `Command(name, description?)`      | command ctor   | named command node        |
|   [2]   | `RootCommand(description?)`        | root ctor      | top-level entry point     |
|   [3]   | `Option<T>(name, description?)`    | option ctor    | typed option symbol       |
|   [4]   | `Argument<T>(name?, description?)` | argument ctor  | typed positional argument |

[ENTRYPOINT_SCOPE]: command composition
- rail: configuration

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------- | :------------- | :------------------------- |
|   [1]   | `Command.Add(Argument argument)`       | child add      | positional argument add    |
|   [2]   | `Command.Add(Option option)`           | child add      | option add                 |
|   [3]   | `Command.Add(Command command)`         | child add      | subcommand add             |
|   [4]   | `Command.Arguments`                    | child list     | `IList<Argument>` accessor |
|   [5]   | `Command.Options`                      | child list     | `IList<Option>` accessor   |
|   [6]   | `Command.Subcommands`                  | child list     | `IList<Command>` accessor  |
|   [7]   | `Command.Aliases`                      | alias set      | `ICollection<string>`      |
|   [8]   | `Command.TreatUnmatchedTokensAsErrors` | policy flag    | default `true`             |

[ENTRYPOINT_SCOPE]: action binding
- rail: configuration

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]  | [RAIL]                        |
| :-----: | :------------------------------------------------------------------- | :-------------- | :---------------------------- |
|   [1]   | `Command.SetAction(Action<ParseResult>)`                             | sync action     | void synchronous handler      |
|   [2]   | `Command.SetAction(Func<ParseResult, int>)`                          | sync action     | exit-code synchronous handler |
|   [3]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task>)`      | async action    | preferred async handler       |
|   [4]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` | async action    | preferred exit-code async     |
|   [5]   | `Command.Action`                                                     | action property | `CommandLineAction?` get/set  |
|   [6]   | `Command.Validators`                                                 | validator list  | `List<Action<CommandResult>>` |

[ENTRYPOINT_SCOPE]: parse and invoke
- rail: configuration

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `Command.Parse(IReadOnlyList<string>, config?)` | parse          | `ParseResult` from args         |
|   [2]   | `Command.Parse(string, config?)`                | parse          | `ParseResult` from command line |
|   [3]   | `ParseResult.GetValue<T>(Option<T>)`            | value access   | typed option value              |
|   [4]   | `ParseResult.GetValue<T>(Argument<T>)`          | value access   | typed argument value            |
|   [5]   | `ParseResult.Errors`                            | error list     | parse error collection          |
|   [6]   | `ParseResult.GetResult(Symbol)`                 | result lookup  | symbol parse result node        |

## [4]-[IMPLEMENTATION_LAW]

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
