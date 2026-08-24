# [RASM_APPHOST_API_COMMANDLINE]

`System.CommandLine` owns the app-root verb boundary: a `RootCommand` symbol tree parses argv into a typed `ParseResult`, and each command binds a synchronous or asynchronous action dispatched off that result. Typed `Option<T>` and `Argument<T>` carry arity, aliases, and completions, and parse failures surface as `ParseResult.Errors` data rather than a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.CommandLine`
- package: `System.CommandLine`
- assembly: `System.CommandLine`
- namespace: `System.CommandLine`, `.Parsing`, `.Invocation`, `.Completions`
- asset: runtime library
- rail: configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: command and symbol family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                    |
| :-----: | :------------------------------ | :------------- | :------------------------------ |
|  [01]   | `Symbol`                        | abstract class | shared name and completion base |
|  [02]   | `Command`                       | class          | subcommand and symbol container |
|  [03]   | `RootCommand`                   | class          | application entry command       |
|  [04]   | `Option`                        | abstract class | named typed option symbol       |
|  [05]   | `Argument`                      | abstract class | positional value symbol         |
|  [06]   | `Directive`                     | class          | bracketed prefix directive      |
|  [07]   | `VersionOption`                 | sealed class   | built-in `--version` option     |
|  [08]   | `DiagramDirective`              | sealed class   | `[diagram]` parse-debug token   |
|  [09]   | `EnvironmentVariablesDirective` | sealed class   | `[env]` variable-setting token  |

[PUBLIC_TYPE_SCOPE]: parse and invocation family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                  |
| :-----: | :------------------------ | :-------------- | :---------------------------- |
|  [01]   | `ParseResult`             | class           | parsed value and error access |
|  [02]   | `ParserConfiguration`     | class           | parser behavior policy        |
|  [03]   | `InvocationConfiguration` | class           | invocation output and error   |
|  [04]   | `ArgumentArity`           | readonly struct | argument count bounds         |
|  [05]   | `CommandLineAction`       | abstract class  | sync or async handler base    |
|  [06]   | `CommandResult`           | sealed class    | per-command validation target |

[ArgumentArity]: `Zero` `ZeroOrOne` `ExactlyOne` `ZeroOrMore` `OneOrMore`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: symbol construction

| [INDEX] | [SURFACE]                         | [SHAPE] | [CAPABILITY]        |
| :-----: | :-------------------------------- | :------ | :------------------ |
|  [01]   | `Command(name, description?)`     | ctor    | named command node  |
|  [02]   | `RootCommand(description?)`       | ctor    | top-level entry     |
|  [03]   | `Option<T>(name, params aliases)` | ctor    | typed option symbol |
|  [04]   | `Argument<T>(name)`               | ctor    | typed positional    |

- `Option<T>`/`Argument<T>` set `Description` through the `Symbol` property after construction, never a ctor argument.

[ENTRYPOINT_SCOPE]: command composition

| [INDEX] | [SURFACE]                              | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Command.Add(Argument)`                | instance | adds a positional argument    |
|  [02]   | `Command.Add(Option)`                  | instance | adds an option                |
|  [03]   | `Command.Add(Command)`                 | instance | adds a subcommand             |
|  [04]   | `Command.Arguments`                    | property | `IList<Argument>` accessor    |
|  [05]   | `Command.Options`                      | property | `IList<Option>` accessor      |
|  [06]   | `Command.Subcommands`                  | property | `IList<Command>` accessor     |
|  [07]   | `Command.Aliases`                      | property | `ICollection<string>` aliases |
|  [08]   | `Command.TreatUnmatchedTokensAsErrors` | property | unmatched-token error policy  |

[ENTRYPOINT_SCOPE]: action binding

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Command.SetAction(Action<ParseResult>)`                             | instance | void synchronous handler      |
|  [02]   | `Command.SetAction(Func<ParseResult, int>)`                          | instance | exit-code synchronous handler |
|  [03]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task>)`      | instance | preferred async handler       |
|  [04]   | `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)` | instance | preferred exit-code async     |
|  [05]   | `Command.Action`                                                     | property | `CommandLineAction?` accessor |
|  [06]   | `Command.Validators`                                                 | property | `List<Action<CommandResult>>` |

[ENTRYPOINT_SCOPE]: parse and invoke

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :---------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Command.Parse(IReadOnlyList<string>, config?)` | instance | parses argv to `ParseResult`  |
|  [02]   | `Command.Parse(string, config?)`                | instance | parses a command line         |
|  [03]   | `ParseResult.GetValue<T>(Option<T>)`            | instance | typed option value            |
|  [04]   | `ParseResult.GetValue<T>(Argument<T>)`          | instance | typed argument value          |
|  [05]   | `ParseResult.Errors`                            | property | `ParseError` list             |
|  [06]   | `ParseResult.GetResult(Symbol)`                 | instance | symbol parse-result node      |
|  [07]   | `ParseResult.Invoke(config?) -> int`            | instance | runs the matched sync action  |
|  [08]   | `ParseResult.InvokeAsync(config?, ct) -> Task`  | instance | runs the matched async action |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RootCommand` roots the symbol tree; `Command` nodes nest subcommands, options, and arguments.
- `SetAction` stores a `CommandLineAction`; `Parse` builds the `ParseResult` and the matched command's action runs on `Invoke`.
- Every `Command` treats unmatched tokens as errors by default.
- `ParseResult.Errors` carries `ParseError` instances; a non-empty set blocks invocation.

[STACKING]:
- `Runtime/modules.md`: `AppRootVerbs.Mount(string, Seq<VerbRow>) -> RootCommand` folds a seed-DATA `VerbRow` table onto one `RootCommand`; each row binds `Command.SetAction(Func<ParseResult, CancellationToken, Task<int>>)`, reads arguments through `ParseResult.GetValue<T>`, and projects `ParseResult.Errors` to exit-code evidence, one boundary adapter onto composed owners rather than a second dispatcher.

[LOCAL_ADMISSION]:
- Build the `RootCommand` tree at composition; bind typed `Option<T>` and `Argument<T>` to consumers through `ParseResult.GetValue<T>` inside `SetAction` delegates.
- `SetAction(Func<ParseResult, CancellationToken, Task<int>>)` is the async entry point.
- `Command.Validators` run after parse and before invocation for cross-option constraints.

[RAIL_LAW]:
- Package: `System.CommandLine`
- Owns: CLI argument parsing and command dispatch
- Accept: `RootCommand`-rooted parse tree with typed `Option<T>` and `Argument<T>` surfaces
- Reject: hand-rolled `args` parsing or positional-index access without `Argument<T>` binding
