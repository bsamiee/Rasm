# [DOUBLES]

## [01]-[SEAMS]

Doubles enter where code under test reads its dependency, first matching row winning:

| [INDEX] | [CODE_READS]              | [DOUBLE_ENTERS_THROUGH]                                                          |
| :-----: | :------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | Effect service            | `Effect.provideService(Tag, double)`, a test layer for a block                   |
|  [02]   | `Config`                  | `ConfigProvider.fromUnknown(record)` provided as `ConfigProvider.ConfigProvider` |
|  [03]   | `process.env` itself      | `vi.stubEnv(name, value)`, `undefined` deleting the variable                     |
|  [04]   | Module export             | `vi.mock(import('<module>'), factory)`                                           |
|  [05]   | Member of a global object | `vi.spyOn(object, member)`                                                       |
|  [06]   | Callback parameter        | `vi.fn<Signature>()`                                                             |

Effect `Config` reads ignore `vi.stubEnv` after a file's first read: the default `ConfigProvider` is `fromEnv()`, built once per module instance, and `fromEnv` copies `process.env` at its first lookup. `graph.test.ts` (`tests/libs/typescript/developer/nx/`) stubs each Nx variable for Nx's own `workspaceDataDirectoryForWorkspace`, the reference oracle reading `process.env`, and passes the same record to the code under test through `ConfigProvider.fromUnknown`.

## [02]-[PLATFORM]

Platform services take their own constructor, and members a double leaves out take the constructor's default:

| [INDEX] | [SERVICE]             | [CONSTRUCTOR]                         | [LEFT_OUT]                                                   |
| :-----: | :-------------------- | :------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `FileSystem`          | `FileSystem.make` over `makeNoop({})` | Derived from given primitives, others fail `NotFound`        |
|  [02]   | `FileSystem`, no read | `FileSystem.layerNoop({})`            | Fail `NotFound`, temporary paths and `makeDirectory` die     |
|  [03]   | `Stdio`               | `Stdio.layerTest(fields)`             | Empty arguments and input, draining sinks, no terminal       |
|  [04]   | `Terminal`            | `Terminal.make(members)`              | None, each member given                                      |
|  [05]   | `ChildProcessSpawner` | `ChildProcessSpawner.make(spawn)`     | `exitCode`, `string`, `lines`, `streamString`, `streamLines` |
|  [06]   | `Path`                | `Path.layer`, real POSIX paths        | None                                                         |

`makeNoop` alone derives nothing: beside a given `readFile` its `readFileString` fails `NotFound`, and beside a given `access` its `exists` answers `false`. Spread under the given members into `FileSystem.make`, it fills every member the run never reaches, and `make` replaces `exists`, `glob`, `readFileString`, `stream`, `sink`, and `writeFileString` with forms reading the given primitives. `tasks` of `@rasm/nx/graph` reads Nx's cached graph through both services:

```ts
const served = (files: ReadonlyMap<string, Uint8Array>) =>
    FileSystem.make({
        ...FileSystem.makeNoop({}),
        readFile: (path) => Effect.fromNullishOr(files.get(path)).pipe(Effect.mapError(() => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'readFile', pathOrDescriptor: path }))),
    });

const read = (files: ReadonlyMap<string, Uint8Array>, environment: Record.ReadonlyRecord<string, string>) =>
    tasks(ROOT).pipe(Effect.provideService(FileSystem.FileSystem, served(files)), Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromUnknown(environment)), Effect.provide(Path.layer));
```

- Missing entries fail with the `PlatformError` reason the Node layer raises, as derived `exists` answers `false` for `NotFound` alone and fails for every other reason
- Members a run never reaches take `Effect.die('<member>')`, so a reach the test did not plan names that member in its defect
- `Stdio.layerTest({ args, stdout: () => Sink.forEach(...) })` overrides the fields a case reads, a sink writing each chunk to a `Ref` the test reads after the run
- `Terminal.make` takes `readInput` as a scoped queue of `UserInput` (`{ input, key: { name, ctrl, meta, shift } }`) for a prompt, and a prompt reading past its end fails with `QuitError`

Spawner doubles record each command and answer from the request. `unspawned` (`extendscript/node.test.ts`) records each argument vector and fails with the error the test then asserts by identity, so the test proves both the attempt and the propagation:

```ts
const unspawned = (requested: Ref.Ref<ReadonlyArray<string>>, refused: PlatformError.PlatformError) =>
    ChildProcessSpawner.make((command) => Effect.andThen(Ref.update(requested, Array.appendAll(ChildProcess.isStandardCommand(command) ? [[command.command, ...command.args.filter(Predicate.isString)].join(' ')] : [])), refused));
```

- `ChildProcess.isStandardCommand` narrows a command to `command` and `args`, and `Predicate.isString` drops `Redacted` arguments from a recorded line
- Scripted runs return `ChildProcessSpawner.makeHandle` with `stdout`, `stderr`, and `all` holding the bytes a real child writes, as derived `exitCode` drains `all` and derived reads drain `stderr`

## [03]-[COMMANDS]

`Command.runWith(command, { version })(argv)` parses `argv` where `Command.run` reads `Stdio` arguments, and requires `Command.Environment`: `FileSystem`, `Path`, `Terminal`, `ChildProcessSpawner`, and `Stdio`. Runs reaching no file, prompt, or process take the layer the `Command` module documents:

```ts
const Cli = Layer.mergeAll(
    FileSystem.layerNoop({}),
    Path.layer,
    Stdio.layerTest({}),
    Layer.succeed(Terminal.Terminal, Terminal.make({ columns: Effect.succeed(80), rows: Effect.succeed(24), size: Effect.die('size'), readInput: Effect.die('readInput'), readLine: Effect.die('readLine'), display: () => Effect.void })),
    Layer.succeed(ChildProcessSpawner.ChildProcessSpawner, ChildProcessSpawner.make(() => Effect.die('spawn'))),
);
```

Runs report through `Console` and fail with what they rendered:
- `CliOutput.defaultPresenter` writes help and version through `Console.log` and parse errors through `Console.error`, so `TestConsole.logLines` and `errorLines` read them and a `Stdio` sink reads none
- `--help` and `--version` succeed after printing, and a parse failure prints help, then fails with `CliError.ShowHelp` holding `commandPath` and `errors`
- Handler failures carrying a user message render, then fail with the handler's own error, and `renderErrors: false` skips rendering alone
- `Terminal.QuitError` from a prompt interrupts the run, so a quit case asserts an interruption through `Effect.exit`
- Global settings resolve their fallback `Config` through the provided `ConfigProvider`, so a setting case provides `ConfigProvider.fromUnknown` beside `argv`

## [04]-[MODELS]

`LanguageModel.make({ generateText, streamText })` returns an effect building the service from provider hooks, a double standing in for the provider alone. Prompt preparation, tool resolution, response decoding, and structured output run as a provider adapter runs them:

```ts
const model = (answer: (options: LanguageModel.ProviderOptions) => ReadonlyArray<Response.PartEncoded>) =>
    Layer.effect(LanguageModel.LanguageModel, LanguageModel.make({ generateText: (options) => Effect.sync(() => [...answer(options)]), streamText: () => Stream.die('streamText') }));
```

- Hooks read `options.prompt`, `options.tools`, and `options.responseFormat` (`{ type: 'json', objectName, schema }` under `generateObject`) and answer by rule from them
- Answers are encoded parts (`{ type: 'text', text }`, `{ type: 'tool-call', id, name, params }`), and `streamText` answers stream parts (`text-start`, `text-delta`, `text-end`, `finish`)
- Parts failing their schema fail `generateText`, `generateObject`, and `streamText` with an `AiError` whose `reason` is `InvalidOutputError`
- Tool calls naming a tool outside the toolkit fail with `ToolNotFoundError`, and `toolCallValidation: 'deferred'` returns them unchanged

## [05]-[MODULES]

Module mocks replace a module export no service carries: a value computed at import (`workspaceDataDirectory` of `@nx/devkit/internal`), a host module absent outside its host (`adobe:uxp`), or `node:path` under another platform's separators (`windows` of `@rasm/testing/path`):

```ts
vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));
vi.mock(import('@nx/devkit'), async (actual) => ({ ...(await actual()), readCachedProjectGraph: vi.fn<typeof readCachedProjectGraph>() }));
```

- `vi.mock(import('<module>'), factory)` types `actual` and the returned `Partial` against the module, so a renamed or retyped export fails type checking
- Vitest hoists `vi.mock` above imports, so a factory reads file-scope variables through `vi.hoisted` alone and imports modules inside itself (`await import('node:path')`)
- `vi.mocked(member).mockReturnValue(value)` sets one test's answer on a `vi.fn` the factory placed, a case's own value beside its arrangement
- Vitest defaults clear call records before each test and keep implementations, spies, and stubbed variables in place (`mockReset`, `restoreMocks`, `unstubEnvs` off)
- `using spy = vi.spyOn(console, 'warn').mockReturnValue(undefined)` restores at block end through the mock's `Symbol.dispose`, and a fixture restores through `onCleanup`
- `vi.fn<Signature>()` types a callback by the signature it replaces, and `toHaveBeenCalledOnce()` and `toHaveBeenCalledExactlyOnceWith(...args)` assert one call

Forms with a typed or seam-level counterpart take it:

| [INDEX] | [FORM]                                 | [IN_PLACE_OF]                                     |
| :-----: | :------------------------------------- | :------------------------------------------------ |
|  [01]   | `vi.mock(import('<module>'), factory)` | `vi.mock('<module>', factory)`                    |
|  [02]   | `Effect.provideService(Tag, double)`   | `vi.mock` of a module an Effect service wraps     |
|  [03]   | `ConfigProvider.fromUnknown(record)`   | `vi.stubEnv` for an Effect `Config` read          |
|  [04]   | `FileSystem.make` over `makeNoop`      | `makeNoop` alone beside a given primitive, a cast |
|  [05]   | `vi.fn<Signature>()`                   | Untyped `vi.fn()`                                 |
|  [06]   | `toHaveBeenCalledOnce()`               | `toHaveBeenCalledTimes(1)`                        |
|  [07]   | `using` or `onCleanup` restoring a spy | Spy left installed for later tests of its file    |
