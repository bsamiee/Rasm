# [PROCESSES]

## [01]-[PLACEMENT]

Work leaving the current fiber selects its form by what runs and what a caller reads back:

| [INDEX] | [WORK]                          | [FORM]                                                  | [RELEASE]                                 |
| :-----: | :------------------------------ | :------------------------------------------------------ | :---------------------------------------- |
|  [01]   | Program, output and exit code   | `ChildProcess.make` yielded in a scope                  | Scope close signals process group         |
|  [02]   | Program, output alone           | `ChildProcessSpawner` `string`, `lines`, `streamLines`  | Output end, exit code never read          |
|  [03]   | Program, exit code alone        | `ChildProcessSpawner` `exitCode`                        | Leader exit, descendant pipes ignored     |
|  [04]   | Program, typed requests         | RPC socket protocol over the handle's pipes (`rpc.md`)  | Scope close, as row 01                    |
|  [05]   | Own module, one reply           | `Worker` or `fork` under `Effect.acquireDisposable`     | `terminate()` or `SIGTERM` at scope close |
|  [06]   | Own module, many typed requests | `RpcClient.layerProtocolWorker` over `NodeWorker.layer` | Close message, terminate after 5 seconds  |

Worker threads share their process: `process.chdir` and signal listeners are absent, `process.env` is a copy, and `process.exit` ends the thread alone. Work needing its own working directory, its own signals, process-global state (`process.exitCode`), or a crash boundary runs in a `fork` child, and `NodeWorker.layer` drives a forked child as it drives a thread.

Node flags go where the process or thread needing them starts (its argv, `execArgv`, `sea.json`, a target's `NODE_OPTIONS`):
- Processes calling an experimental API (`node:ffi`, zlib ZIP, single executables) start with `--disable-warning=ExperimentalWarning`
- Worker `execArgv` scopes a per-thread flag to its thread and refuses a per-process one (`--process-timeout`)
- Code running inside a host process (an Nx executor) moves a call needing a flag into a Worker started with it
- `NODE_OPTIONS` refuses `--process-timeout` and `--config-file`, and `--process-timeout` refuses every inspector

## [02]-[CHILD_PROCESSES]

`ChildProcess.make` describes a command, and yielding it spawns a process through `ChildProcessSpawner` into a handle the enclosing scope owns. Programs answering through their exit code (`git check-ignore`, `rg`) return it beside their output, and a code outside the answer fails:

```ts
const ignored = Effect.fn('ignored')(function* (cwd: string, paths: readonly string[]) {
    const handle = yield* ChildProcess.make({ cwd, stdin: 'ignore', stdout: 'ignore' })`git check-ignore --no-index -q -- ${paths}`;
    const [said, code] = yield* Effect.all([handle.stderr.pipe(Stream.decodeText, Stream.mkString), handle.exitCode], { concurrency: 'unbounded' });
    return code <= 1 ? code === 0 : yield* new Cause.UnknownError(said, `git check-ignore exited ${code}`);
}, Effect.scoped);
```

Effect holds no member failing on a non-zero exit, and `run` of `@rasm/process` reads every pipe and fails with `ExitError` holding command, code, stdout, and stderr, or with `TimeoutError` past its `within` deadline. Pipes default to `'pipe'`, a child blocks before exit on a full pipe no fiber reads, and each piped stream is read or set to `'inherit'` or `'ignore'`:
- `ChildProcessSpawner` `string`, `lines`, `streamString`, and `streamLines` read stdout, drain stderr, and succeed whatever the exit code
- `includeStderr: true` interleaves stderr into a `ChildProcessSpawner` read
- `exitCode` fails with `PlatformError` for a child a signal ended, and a failed `stdin` stream fails it first
- `ChildProcess.pipeTo(next, { from, to })` pipes `stdout`, `stderr`, `all`, or `fdN` onward, answering the last command's `exitCode` alone
- `prefix` edits a pipeline's leftmost command, and `setCwd` and `setEnv` edit every command
- `env` replaces the inherited environment, `PATH` included, `setEnv` merging into it, and its values can be `Redacted`
- `extendEnv: true` extends the inherited environment and removes each variable set to `undefined`
- `cwd` resolves through `FileSystem`, a missing folder failing with `NotFound` before spawn
- `stdin` takes a `Stream`, and `stdout` and `stderr` take a `Sink`, the handle's stream emitting its output
- `additionalFds: { fd3: { type: 'duplex' } }` opens a descriptor `getInputFd(3)` writes and `getOutputFd(3)` reads, Windows refusing its half-close
- Commands hold no IPC channel, and a module answering over one runs under `fork` ([03])

Arguments are spelled once, and `make` parses a template when called:
- `ChildProcess.make(command, args)` and ``ChildProcess.make({ cwd })`git log ${ref}` `` take a `${}` value as one argument, an array one per element
- Template text holding shell syntax (`|`, `$HOME`, `*`, `~`) throws `CommandLineError` at `make`, a defect the first run shows
- `CommandLine.template` returns the words of text read at run time as a `Result`
- `shell` takes the whole script as the command, a `${}` value in a shell template throwing and `shell` with arguments failing with `BadArgument`
- Scripts holding values build through `Shell.script(dialect)` of `@rasm/shell`, quoting each hole by its place in the parsed script
- `Shell.script` refuses a hole inside code a program runs as a script (the operand of `sh -c`), read from its usage catalog
- `CommandLine.quote(word)` and `join(words)` quote one word or argument list as a `Result`, `{ format }` naming `posix`, `powershell`, or `windows`
- `CommandLine.display(words)` renders a command for logs and errors, each `Redacted` word as `<redacted>`

Release signals the process group a command leads, and each command leads one of its own while `detached` holds its default `true` outside Windows:
- Scope close sends `killSignal` (`SIGTERM`) to the group, waits for its leader without limit, then up to one second for the group
- `forceKillAfter` adds `SIGKILL` to scope close, and `run` sets it, so a leader ignoring `SIGTERM` holds no deadline open
- Leaders exiting non-zero signal their group at exit, and a zero exit signals it at scope close, ending descendants a program left running
- `detached: false` keeps the child in the parent's group: terminal interrupts reach it, release signals it alone, and its descendants outlive it
- `handle.unref` lets a parent exit first and leaves its child running at scope close, the `Reref` effect it returns restoring both
- Finalizers run on interruption and scope close, and `NodeRuntime.runMain` interrupts on `SIGINT` and `SIGTERM`
- Hosts ending their process through `process.exit` (an Nx plugin worker) run no finalizer, and a child there takes the host's exit hook
- `killChildOnHostExit` of `@nx/devkit/internal` is the Nx exit hook, ending the tracked child's tree as the plugin worker exits

Engines with a process call and no Node runtime build the spawner over that call:
- `ChildProcessSpawner.make(spawn)` derives `exitCode`, `string`, `lines`, `streamString`, and `streamLines` from one `spawn`
- `spawn` returns `ChildProcessSpawner.makeHandle({ pid, exitCode, isRunning, kill, stdin, stdout, stderr, all, getInputFd, getOutputFd, unref })`
- Commands the engine cannot run (a pipeline, `shell`, `additionalFds`, an `inherit` pipe) fail with `PlatformError.badArgument` before spawn

## [03]-[WORKERS]

One-shot workers reply once and end:

```ts
const resolved = Effect.fn('resolved')(function* (at: string) {
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${at} does not load as a Vite config`);
    const worker = yield* Effect.acquireDisposable(Effect.sync(() => new Worker(WORKER, { workerData: { at } })));
    const [message] = yield* Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed));
    return yield* Schema.decodeUnknownEffect(Resolved)(message);
}, Effect.scoped);
```

`WORKER` is the worker module's URL, `Resolved` its reply schema, and `once` comes from `node:events`. A throwing worker fails the message arm through `once` rejecting on `error`, and the exit arm fails a worker ending with no message:
- Worker threads receive their input through `workerData` and reply through `parentPort.postMessage`
- Replies cross structured clone, class instances arriving as plain objects the reply schema decodes
- `fork` children take the same race, an `Effect.tap` sending their request before `Effect.raceFirst`
- `fork(entry, { serialization: 'advanced' })` sends what JSON drops (`Uint8Array`, `Map`, `BigInt`)
- `Worker` disposes through `terminate()` and awaits its thread's end, a forked child through `kill()` returning at the signal
- Steps reading a forked child's exit await `once(child, 'exit')` inside the scope

Long-lived pools answer typed requests over one `RpcGroup`:

```ts
class Resolver extends Context.Service<Resolver>()('@app/Resolver', { make: RpcClient.make(Configs) }) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(RpcClient.layerProtocolWorker({ size: os.availableParallelism() })), Layer.provide(NodeWorker.layer(() => new Worker(WORKER))));
}

const serve = RpcServer.layer(Configs, { disableFatalDefects: true }).pipe(Layer.provide([ConfigsLive, RpcServer.layerProtocolWorkerRunner]), Layer.provide(NodeWorkerRunner.layer), Layer.launch, NodeRuntime.runMain);
```

`Configs` is the `RpcGroup`, `ConfigsLive` its handler layer, and `WORKER` a module running `serve`:
- `{ size }` holds a fixed pool, `{ minSize, maxSize, timeToLive }` resizes it, and `concurrency` bounds requests per worker
- Failed workers respawn every second, and requests open on a dead worker fail with `RpcClientError`
- Handler defects under default options fail the open requests of every worker in the pool, and `disableFatalDefects: true` keeps one to its call
- `NodeWorker.layer` release posts a close message, then after 5 seconds ends a thread through `terminate()` and a forked child through `SIGKILL`
- Worker protocols post each schema's JSON codec output by structured clone and read no `RpcSerialization` layer
- `Schema.Uint8Array` fields cross as base64 copies, and `Transferable.Uint8Array` moves the buffer to a thread with no copy
- `SharedMemory.schema(Int32Array)` shares one `SharedArrayBuffer` across threads and refuses the copy a forked child receives
- `RpcWorker.layerInitialMessage(schema, build)` sends each worker one value as it starts, `RpcWorker.initialMessage(schema)` decoding it inside
- Pools respawn a worker that ends, and work that must end with its reply takes the one-shot form

## [04]-[COMMANDS]

`Command.make(name, params, handler)` derives the handler input from its flags and arguments, and a setting every subcommand reads decodes its flag and its configuration fallback through one string codec:

```ts
const setting = <const Name extends string, A>(name: Name, schema: Schema.Codec<A, string>, fallback: A) =>
    GlobalFlag.Setting(name)({ flag: Flag.String(name).pipe(Flag.withSchema(schema), Flag.withFallbackConfig(Config.schema(schema, name.split('-'))), Flag.withDefault(fallback)) });
const Retention = setting('retention', Schema.DurationFromString, Duration.hours(1));

const call = Command.make('call', { tool: Argument.String('tool'), given: Argument.String('json').pipe(Argument.withSchema(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json))), Argument.withDefault({})) }, ({ tool, given }) => invoke(tool, given));

Command.make(name, {}, () => Effect.flatMap(Retention, serve)).pipe(Command.withSubcommands([call]), Command.withGlobalFlags([Retention]), Command.run({ version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
```

`invoke` calls one tool, `serve` serves with the retention it receives, and `name` and `version` come from the package manifest. Fallback combinators catch a missing flag or argument alone and apply in pipe order, `withFallbackConfig` reading `Config`, `withFallbackPrompt` asking, and `withDefault`, taking a value or an effect over the CLI environment, closing the chain. Commands a flag reaches select where it is declared:

| [INDEX] | [REACH]                        | [DECLARATION]                                                 | [READ]                          |
| :-----: | :----------------------------- | :------------------------------------------------------------ | :------------------------------ |
|  [01]   | One command                    | `Command.make` params                                         | Handler input                   |
|  [02]   | Command and its subcommands    | `Command.withSharedFlags`                                     | `yield* parent` in a subcommand |
|  [03]   | Every descendant, as a service | `Command.withGlobalFlags([GlobalFlag.Setting(id)({ flag })])` | `yield*` of the setting         |

Shared flags parse before or after the subcommand name, global flags outside their declaring command fail with help, and a run wraps its handler:
- `Command.run` reads `Stdio` args and parses `--help`, `--version`, `--completions`, `--wizard`, and `--log-level` from `CliConfig.builtIns`
- `CliConfig.layer({ builtIns })` replaces the built-in flag list
- `GlobalFlag.LogLevel` yields the parsed `--log-level` `Option`
- `Command.provide(layer | (input) => layer)` provides services chosen from the parsed input
- `CliError.UserError.make({ cause, userMessage })` and a package error with a `[CliError.userMessage]` getter render through `CliOutput.Formatter`
- Rendered errors keep their tag, and `runMain` logs them no second time
- `renderErrors: false` on `Command.run` leaves rendering to a host owning it
- `Prompt` values are effects reading `FileSystem`, `Path`, and `Terminal`, a quit interrupting the run `runMain` ends with code 130
