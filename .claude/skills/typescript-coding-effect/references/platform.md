# [PLATFORM]

## [01]-[PROCESSES]

`ChildProcess.make` describes a command as an effect of a scoped handle, and closing the scope stops the process:
- `ChildProcess.make(command, args, options)` and the template ``ChildProcess.make`git log -n ${count}` `` spell each argument once, an array value adding one argument per element
- Templates reject shell syntax, and a shell script builds through `CommandLine.join` or `CommandLine.quote` for a named shell
- `env` replaces the inherited environment unless `extendEnv: true`, and values can be `Redacted`
- `stdin` takes a `Stream`, and `stdout` and `stderr` take `'inherit'`, `'ignore'`, `'pipe'`, or a `Sink`
- `killSignal` defaults to `SIGTERM`, and `forceKillAfter` sends `SIGKILL` after a delay
- `ChildProcess.pipeTo(next, { from })` builds a pipeline, and `prefix`, `setCwd`, and `setEnv` transform a whole one
- `ChildProcessSpawner` members `string`, `lines`, `streamLines`, and `exitCode` own the handle and drain unread output, and code failing on a non-zero exit reads `handle.exitCode` beside the output
- `CommandLine.display(words)` renders a command for logs and errors, each `Redacted` word as `<redacted>`

## [02]-[COMMANDS]

`Command.make(name, params, handler)` derives the handler input from its flags and arguments, and `Command.run` parses `--help`, `--version`, `--completions`, `--wizard`, and `--log-level` itself:

```ts
Command.make(
    'format',
    {
        mode: Flag.Literals('mode', Mode.literals).pipe(Flag.withDefault(Mode.literals[0])),
        jobs: Flag.Int('jobs').pipe(Flag.withSchema(Schema.Int.check(Schema.isGreaterThan(0))), Flag.withFallbackConfig(Config.Int('FORMAT_JOBS')), Flag.withDefault(4)),
        roots: Argument.Directory('root', { mustExist: true }).pipe(Argument.atLeast(1)),
    },
    ({ mode, jobs, roots }) => ChildProcessSpawner.ChildProcessSpawner.use((spawner) => Effect.forEach(roots, (root) => spawner.exitCode(ChildProcess.make('biome', [mode, root])), { concurrency: jobs })),
).pipe(Command.run({ version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
```

`Mode` is a `Schema.Literals` of formatter modes and `version` the package version. `--log-level` sets `References.MinimumLogLevel` for the handler:
- `Command.withSubcommands([...])` composes commands, and a subcommand reads its parent's input through `yield* parent`
- `Command.withSharedFlags({...})` makes flags readable by descendants on either side of the subcommand name
- `Flag.Literals`, `Flag.Redacted`, `Flag.KeyValuePair`, `Flag.FileSchema(name, schema)`, and `Argument.Directory` are typed constructors, and `Flag.withSchema(codec)` decodes any value
- `Flag.withFallbackConfig(Config...)` reads a missing flag from configuration, `Flag.withFallbackPrompt(prompt)` asks for it, and `Flag.withDefault` closes the chain
- `Command.provide(layer | (input) => layer)` provides services chosen from the parsed input
- Handler errors with a `[CliError.userMessage]` getter render through the CLI formatter with their tag kept
- `Prompt.Text`, `Select`, and `Confirm` are effects reading `Terminal`, and `Prompt.all` collects a struct of them

## [03]-[FILES]

`FileSystem` and `Path` are services a platform layer provides, and `Glob` matches paths without the disk:
- `fs.glob(pattern, { root, exclude, dot })` returns sorted paths, and `Glob.make(pattern)` with `Glob.match` tests a path
- `fs.makeTempDirectoryScoped()` owns scratch space for a scope, `fs.stream` and `fs.sink` stream bytes, and `fs.watch(path)` streams changes
- Bundle resources resolve through `Url.fromString('../Resources/<file>', base)`, `base` the `URL` that `path.toFileUrl(executable)` returns, in place of a `dirname` chain
- `FileSystem.layerNoop({ ...overrides })` stubs the service for a test

## [04]-[CRYPTO]

`effect/crypto` holds one service per primitive family over WebCrypto, each taking an algorithm value:
- `Digest`, `Mac`, `Aead`, `Cipher`, `Kdf`, `Signature`, `KeyAgreement`, `KeyWrap`, and `SecureRandom` are the services, with algorithms (`Digest.sha256`, `Aead.aes256Gcm`)
- `digest.digestHex(Digest.sha256, bytes)` returns lowercase hex
- `Mac.key`, `Aead.key`, and `Signature.privateKey` import keys typed by algorithm, and `KeyFormat.decodePublic` reads `pem`, `jwk`, `spki`, or `pkcs8`
- `mac.verify(key, tag, data)` compares in constant time, and `Mac.timingSafeEqual` compares held bytes
- `Jwt.verifyWithKeySet(S, token, { issuer, audience, algorithms })` over `Jwks.layerRemote` decodes claims, needing `Signature` and `HttpClient`

## [05]-[ENCODING]

`effect/encoding` holds byte and text codecs and streaming channels for line, event, and archive formats:
- `Hex.decode`, `Base64.decode`, and `Base64Url.decode` return a `Result`, and schema boundaries use `Schema.Uint8ArrayFromHex` and the other `Uint8ArrayFrom*` schemas
- `Ndjson.decodeSchema(S)()`, `Csv.decodeSchema(S)`, and `Sse.decodeDataSchema(S)` are channels `Stream.pipeThroughChannel` applies, with matching encoders
- `Yaml.parse`, `Toml.parse`, `Ini.parse`, and `Xml.decodeString` read configuration formats for a schema to decode, and `Tar` and `Zip` stream archive entries
- `NodeStream.fromReadable` enters a Node readable stream as a `Stream`

## [06]-[DURABLE]

`effect/graphql`, `effect/workflow`, and `effect/cluster` hold generated clients, durable execution, and sharded entities:
- `graphqlgen` from `@effect/graphql-generator` writes typed operations per `.graphql` document, and `GraphQLClient.make(group)` gives one method per operation
- `GraphQLClient.items` pages through a cursor, and `GraphQLResolver.findById` batches lookups into one request
- `Workflow.make` declares a workflow with schemas and an idempotency key, and `Activity.make` wraps an effect the engine stores and replays
- `Entity.make(name, rpcs)` addresses an RPC protocol by entity id across runners, and `SingleRunner.layer` and `TestRunner.layer` run entities
