# [PLATFORM]

## [01]-[PROCESSES]

`ChildProcess.make` describes a command as an effect of a scoped handle, and closing the scope stops the process:
- `ChildProcess.make(command, args)` and template ``ChildProcess.make`git log -n ${count}` `` spell each argument once, arrays adding one per element
- Templates reject shell syntax, and a shell script builds through `CommandLine.join` or `CommandLine.quote` for a named shell
- `env` replaces the inherited environment unless `extendEnv: true`, and values can be `Redacted`
- `stdin` takes a `Stream`, and `stdout` and `stderr` take `'inherit'`, `'ignore'`, `'pipe'`, or a `Sink`
- `killSignal` defaults to `SIGTERM`, and `forceKillAfter` sends `SIGKILL` after a delay
- `ChildProcess.pipeTo(next, { from })` builds a pipeline, and `prefix`, `setCwd`, and `setEnv` transform a whole one
- `ChildProcessSpawner` members `string`, `lines`, `streamString`, and `streamLines` read stdout without exit code, `includeStderr: true` interleaving stderr
- `ChildProcessSpawner.exitCode` returns on process exit, ignoring pipes descendants hold
- `CommandLine.display(words)` renders a command for logs and errors, each `Redacted` word as `<redacted>`
- Commands run in their machine format (`--json`, `-z`) for a schema to decode, records split on its separator (`'\0'` for `git -z`)
- Text tables of commands without a machine format slice rows by column spans their ruler line marks, keying cells by heading

Commands failing on a non-zero exit read output and exit code in one scope:

```ts
const run = Effect.fn('run')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore' });
    const [output, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? output : yield* ExitError.make({ argv: [command, ...args], code, output });
}, Effect.scoped);
```

`ExitError` is the package error holding `argv`, `code`, and `output`, and a caller acting on a refusal reads `output` through `Effect.catchTag`.

## [02]-[COMMANDS]

`Command.make(name, params, handler)` derives the handler input from its flags and arguments, and `Command.run` parses `--help`, `--version`, `--completions`, `--wizard`, and `--log-level` itself:

```ts
Command.make(
    'format',
    {
        mode: Flag.Literals('mode', Mode.literals).pipe(Flag.withDefault(Mode.literals[0])),
        jobs: Flag.Int('jobs').pipe(Flag.withSchema(Schema.Int.check(Schema.isGreaterThan(0))), Flag.withFallbackConfig(Config.Int('FORMAT_JOBS')), Flag.withDefault(Effect.sync(os.availableParallelism))),
        roots: Argument.Directory('root', { mustExist: true }).pipe(Argument.atLeast(1)),
    },
    ({ mode, jobs, roots }) => ChildProcessSpawner.ChildProcessSpawner.use((spawner) => Effect.forEach(roots, (root) => spawner.exitCode(ChildProcess.make(formatter, [mode, root])), { concurrency: jobs })),
).pipe(Command.run({ version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
```

`formatter` names the formatter executable, `Mode` is a `Schema.Literals` of its modes, and `version` the package version. `--log-level` sets `References.MinimumLogLevel` for the handler:
- `Command.withSubcommands([...])` composes commands, and a subcommand reads its parent's input through `yield* parent`
- `Command.withSharedFlags({...})` makes flags readable by descendants on either side of the subcommand name
- `Flag.Redacted`, `Flag.KeyValuePair`, and `Flag.FileSchema(name, schema)` are typed constructors, and `Flag.withSchema(codec)` decodes any value
- `Flag.withFallbackConfig(config)` reads a missing flag from `Config`, `withFallbackPrompt(prompt)` asks for it, and `withDefault` closes the chain
- `Command.provide(layer | (input) => layer)` provides services chosen from the parsed input
- Handler errors with a `[CliError.userMessage]` getter render through the CLI formatter with their tag kept
- `Prompt` constructors are effects reading `Terminal`, and `Prompt.all` collects a struct of them

## [03]-[FILES]

`FileSystem` and `Path` are services a platform layer provides, and `Glob` matches paths without the disk:
- `fs.glob(pattern, { root, exclude, dot })` returns sorted paths, and `Glob.make(pattern)` with `Glob.match` tests a path
- `fs.makeTempFileScoped({ suffix })` owns one file a host writes and `makeTempDirectoryScoped()` a folder, each deleted when its scope closes
- `fs.stream` and `fs.sink` stream bytes, and `fs.watch(path)` streams changes
- `Url.fromString('../Resources/<file>', base)` resolves a bundle resource, `base` from `path.toFileUrl(executable)`, in place of `dirname` chains
- `import.meta.resolve('<pkg>/<file>')` locates a dependency file through `path.fromFileUrl` in an effect and `fileURLToPath` in a build config
- `FileSystem.layerNoop({ ...overrides })` stubs the service for a test

## [04]-[CRYPTO]

`effect/crypto` holds one service per primitive family over WebCrypto, each taking an algorithm value (`Digest.sha256`, `Aead.aes256Gcm`):
- `digest.digestHex(Digest.sha256, bytes)` returns lowercase hex
- `Mac.key`, `Aead.key`, and `Signature.privateKey` import algorithm-typed keys, and `KeyFormat.decodePublic` reads `pem`, `jwk`, `spki`, or `pkcs8`
- `mac.verify(key, tag, data)` compares in constant time, and `Mac.timingSafeEqual` compares held bytes
- `Jwt.verifyWithKeySet(S, token, { issuer, audience, algorithms })` over `Jwks.layerRemote` decodes claims, needing `Signature` and `HttpClient`

## [05]-[ENCODING]

`effect/encoding` holds byte and text codecs and streaming channels for line, event, and archive formats:
- `Hex.decode`, `Base64.decode`, and `Base64Url.decode` return a `Result`, and schema boundaries use `Schema.Uint8ArrayFrom*` schemas
- `Ndjson.decodeSchema(S)()` and `Csv.decodeSchema(S)()` read bytes, `decodeSchemaString` and `Sse.decodeDataSchema(S)` text, each with an encoder
- `Csv.decodeSchemaString(S)({ headers: true })` keys cells by header, an empty optional cell decoding as a missing key
- Caller-named CSV fields map to columns through `Schema.encodeKeys` on the row schema
- `Toml.parse`, `Jsonc.parse`, and `Json5.parse` return a `Result`, `Yaml.parse` throws `SyntaxError` into `Effect.try`, and `Ini.parse` is total
- `Xml.decodeString` piped through `Xml.elements(path)` reads XML trees for a schema, and `Xml.encodeString` writes an `Xml.Event` stream unindented
- Indented documents build from a tree through one module-scope `fast-xml-builder` `Builder({ ignoreAttributes: false, format: true })` reading `@_` keys
- `Tar` and `Zip` stream archive entries
- `NodeStream.fromReadable` enters a Node readable stream as a `Stream`

## [06]-[DURABLE]

`effect/graphql`, `effect/workflow`, and `effect/cluster` hold generated clients, durable execution, and sharded entities:
- `graphqlgen` (`@effect/graphql-generator`) writes typed operations per `.graphql` file, and `GraphQLClient.make(group)` gives a method per operation
- `GraphQLClient.items` pages through a cursor, and `GraphQLResolver.findById` batches lookups into one request
- `Workflow.make` declares a workflow with schemas and an idempotency key, and `Activity.make` wraps an effect the engine stores and replays
- `Entity.make(name, rpcs)` addresses an RPC protocol by entity id across runners, and `SingleRunner.layer` and `TestRunner.layer` run entities
