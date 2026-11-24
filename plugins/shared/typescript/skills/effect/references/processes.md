# [PROCESSES]

## [01]-[PLACEMENT]

Work leaving the current fiber selects its form by what runs and what a caller reads back:

| [INDEX] | [WORK]                          | [FORM]                                                  | [RELEASE]                                |
| :-----: | :------------------------------ | :------------------------------------------------------ | :--------------------------------------- |
|  [01]   | Program, output and exit code   | `ChildProcess.make` yielded in a scope                  | Scope close signals process group        |
|  [02]   | Program, output alone           | `ChildProcessSpawner` `string`, `lines`, `streamLines`  | Output end, exit code never read         |
|  [03]   | Program, exit code alone        | `ChildProcessSpawner` `exitCode`                        | Leader exit, descendant pipes ignored    |
|  [04]   | Own module, one reply           | `Worker` under `Effect.acquireDisposable`               | `terminate()` on scope close             |
|  [05]   | Own module, many typed requests | `RpcClient.layerProtocolWorker` over `NodeWorker.layer` | Close message, terminate after 5 seconds |

Worker threads share their process and have no `process.chdir`, signal listener, or parent IPC channel, and `process.exit` in a thread ends it alone. Work needing its own working directory, its own signals, or a crash boundary runs in a child process, and `NodeWorker.layer` drives a `child_process.fork` child as it drives a thread.

## [02]-[CHILD_PROCESSES]

`ChildProcess.make` describes a command, and yielding it spawns a process through `ChildProcessSpawner` into a handle the enclosing scope owns. Commands failing on a non-zero exit read every pipe and the exit code at once:

```ts
const run = Effect.fn('run')(function* (command: string, args: readonly string[], grace: Duration.Input) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', forceKillAfter: grace });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* ExitError.make({ argv: [command, ...args], code, stderr });
}, Effect.scoped);
```

`ExitError` is the package error. Pipes default to `'pipe'`, a child blocks before exit on a full pipe no fiber reads, and each piped stream is read or set to `'inherit'` or `'ignore'`:
- `ChildProcessSpawner` `string`, `lines`, `streamString`, and `streamLines` read stdout, drain stderr, and succeed whatever the exit code
- `includeStderr: true` interleaves stderr into a `ChildProcessSpawner` read
- Scope close sends `killSignal` (`SIGTERM`) to the process group, waits for its leader without limit, then up to one second for its group
- `forceKillAfter` adds `SIGKILL` to scope close
- Leaders exiting non-zero signal their group at exit, and `detached` (default `true` outside Windows) gives each command a group of its own
- `handle.unref` lets a parent exit first and leaves its child running at scope close, the `Reref` effect it returns restoring both
- `env` replaces the inherited environment, `PATH` included, and its values can be `Redacted`
- `extendEnv: true` extends the inherited environment and removes each variable set to `undefined`
- `ChildProcess.make(command, args)` and ``ChildProcess.make`git log -n ${count}` `` spell each argument once, arrays adding one per element
- Templates reject shell syntax, and `shell` with arguments fails with `BadArgument`
- Scripts quote their values through `CommandLine.quote` or `CommandLine.join`
- `CommandLine.display(words)` renders a command for logs and errors, each `Redacted` word as `<redacted>`
- `stdin` takes a `Stream`, and `stdout` and `stderr` take a `Sink`, the handle's stream emitting its output
- Commands hold no IPC channel
- `additionalFds: { fd3: { type: 'duplex' } }` opens a descriptor `getInputFd(3)` writes and `getOutputFd(3)` reads, Windows refusing its half-close
- `ChildProcess.pipeTo(next, { from, to })` pipes `stdout`, `stderr`, `all`, or `fdN` onward
- `prefix` edits a pipeline's leftmost command, and `setCwd` and `setEnv` edit every command

## [03]-[WORKERS]

One-shot workers reply once and end:

```ts
const reply = Effect.fn('reply')(function* (entry: URL, workerData: unknown) {
    const worker = yield* Effect.acquireDisposable(Effect.sync(() => new Worker(entry, { workerData })));
    const failed = (cause: unknown) => new Cause.UnknownError(cause, `${entry.pathname} ends before it replies`);
    const [message] = yield* Effect.raceFirst(Effect.tryPromise({ try: (signal) => once(worker, 'message', { signal }), catch: failed }), Effect.flatMap(Effect.tryPromise({ try: (signal) => once(worker, 'exit', { signal }), catch: failed }), failed));
    return yield* Schema.decodeUnknownEffect(Reply)(message);
}, Effect.scoped);
```

`Reply` is the worker's message schema and `once` comes from `node:events`. A throwing worker fails the message arm through `once` rejecting on `error`, and an exit arm fails a worker ending with no message:
- Worker threads receive their input through `workerData`
- `child_process.fork` children take the same pipe with an `Effect.tap` sending their request before `Effect.raceFirst`
- `Worker` disposes through `terminate()` and awaits its thread's end, `ChildProcess` through `kill()` and returns at the signal
- Steps reading a forked child's exit await `once(child, 'exit')` inside the scope

Long-lived pools answer typed requests over one `RpcGroup`:

```ts
class Hasher extends Context.Service<Hasher>()('@app/Hasher', { make: RpcClient.make(Jobs) }) {
    static readonly layer = Layer.effect(this, this.make).pipe(Layer.provide(RpcClient.layerProtocolWorker({ size: os.availableParallelism() })), Layer.provide(NodeWorker.layer(() => new Worker(entry))));
}

const serve = RpcServer.layer(Jobs).pipe(Layer.provide([JobsLive, RpcServer.layerProtocolWorkerRunner]), Layer.provide(NodeWorkerRunner.layer), Layer.launch, NodeRuntime.runMain);
```

`Jobs` is the `RpcGroup`, `JobsLive` its handler layer, and `entry` a worker module URL with a body running `serve`:
- `{ size }` holds a fixed pool, `{ minSize, maxSize, timeToLive }` resizes it, and `concurrency` bounds requests per worker
- Failed workers respawn every second, and requests open on a dead worker fail with `RpcClientError`
- `NodeWorker.layer` release ends a thread through `terminate()` and a forked child through `SIGKILL`
- Worker protocols post each schema's JSON encoding and read no `RpcSerialization` layer
- `RpcWorker.layerInitialMessage(schema, build)` sends each worker one value as it starts, `RpcWorker.initialMessage(schema)` decoding it inside
- Pools respawn a worker that ends, and work that must end with its reply takes the one-shot form

## [04]-[COMMANDS]

`Command.make(name, params, handler)` derives the handler input from its flags and arguments, and `Command.run` parses `--help`, `--version`, `--completions`, `--wizard`, and `--log-level` itself:

```ts
const cli = Command.make(
    'format',
    {
        mode: Flag.Literals('mode', Mode.literals).pipe(Flag.withDefault(Mode.literals[0])),
        jobs: Flag.Int('jobs').pipe(Flag.withSchema(Schema.Int.check(Schema.isGreaterThan(0))), Flag.withFallbackConfig(Config.Int('FORMAT_JOBS')), Flag.withDefault(Effect.sync(os.availableParallelism))),
        roots: Argument.Directory('root', { mustExist: true }).pipe(Argument.atLeast(1)),
    },
    ({ mode, jobs, roots }) => ChildProcessSpawner.ChildProcessSpawner.use((spawner) => Effect.forEach(roots, (root) => spawner.exitCode(ChildProcess.make(formatter, [mode, root])), { concurrency: jobs })),
).pipe(Command.run({ version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
```

`formatter` is an executable name, `Mode` a `Schema.Literals` of its modes, and `version` the package version. Flags resolve through one chain: `withSchema` decodes, `withFallbackConfig` reads a missing flag from `Config`, `withFallbackPrompt` asks for it, and `withDefault` closes it. Commands a flag reaches select where it is declared:

| [INDEX] | [REACH]                        | [DECLARATION]                                                 | [READ]                          |
| :-----: | :----------------------------- | :------------------------------------------------------------ | :------------------------------ |
|  [01]   | One command                    | `Command.make` params                                         | Handler input                   |
|  [02]   | Command and its subcommands    | `Command.withSharedFlags`                                     | `yield* parent` in a subcommand |
|  [03]   | Every descendant, as a service | `Command.withGlobalFlags([GlobalFlag.Setting(id)({ flag })])` | `yield*` of the setting         |

Shared flags parse before or after the subcommand name, and a run wraps its handler:
- `GlobalFlag.LogLevel` yields the parsed `--log-level` `Option`
- `Command.provide(layer | (input) => layer)` provides services chosen from the parsed input
- Handler errors with a `[CliError.userMessage]` getter render through the CLI formatter, their tag kept
- `Prompt` constructors are effects reading `FileSystem`, `Path`, and `Terminal`
- `Command.runWith(command, { version })(argv)` runs over a given argument list, a test passing its own
