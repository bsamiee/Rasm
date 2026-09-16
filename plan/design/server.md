# [SERVER]

One stdio MCP server, `creative-cloud`, drives Illustrator, Photoshop, InDesign, and Acrobat. Each capability is one tool named `<host>_<verb>`, plus the cross-host `health`. No panel, dialog, menu item, shortcut, scripts-folder copy, or Action binding is a unit of work: agents call tools. This file owns the project graph, the code bar, the host table, the error and value vocabulary, the MCP boundary, the job model, both transports, `health`, and the extension recipe. Tool tables, DOM rules, plugin manifests, and preference catalogues belong to the host files beside it.

Rules every section holds to: no wrapper around an owning API, no fallback chain, one result type, data as JSON beside the code decoded once through `Schema`, every host value read from the host table, every host path a key of that table, closed unions read by tag.

Effect is `effect@4.0.0-rc.115` with `@effect/platform-node@4.0.0-rc.115`. Declaration paths cited as `<module>.d.ts:<line>` are read from that release; `@effect/platform`, `@effect/ai`, and `@effect/cli` no longer exist as packages and their modules live under `effect/unstable/*`.

## [01]-[PROJECTS]

Every edge is a `workspace:*` row in the importing project's `package.json` and a `references` entry in its `tsconfig.json`. Acyclic order: `typography` → `server` → both plugins; `illustrator-scripts` and `window-list` stand alone. The server reaches a built `.jsx` through its `.artifacts/` path, never through an import, because a Node file cannot typecheck under the ES3 typings the scripts project sets.

| [INDEX] | [PATH]                                     | [NAME]                        | [LANGUAGE] |
| :-----: | :----------------------------------------- | :---------------------------- | :--------- |
|  [01]   | `libs/typescript/typography/`        | `@rasm/typography`      | TypeScript |
|  [02]   | `apps/creative-cloud/server/`              | `@rasm/creative-cloud-server` | TypeScript |
|  [03]   | `apps/creative-cloud/indesign-plugin/`     | `@rasm/indesign-plugin`       | TypeScript |
|  [04]   | `apps/creative-cloud/photoshop-plugin/`    | `@rasm/photoshop-plugin`      | TypeScript |
|  [05]   | `apps/creative-cloud/illustrator-scripts/` | `@rasm/illustrator-scripts`   | TypeScript |
|  [06]   | `apps/creative-cloud/window-list/`         | `WindowList`                  | Swift      |

| [INDEX] | [NAME]                        | [OWNS]                                                                                   |
| :-----: | :---------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `@rasm/typography`      | Grid mathematics per mode, preset parse, `fontkit` metrics, size catalogue rows          |
|  [02]   | `@rasm/creative-cloud-server` | Host table, vocabulary, MCP boundary, job model, both transports, one directory per host |
|  [03]   | `@rasm/indesign-plugin`       | Socket client, tick, typed job handlers per kind, `execute` with autocorrect             |
|  [04]   | `@rasm/photoshop-plugin`      | Socket client, tick, typed job handlers per kind, modal scope and history suspension     |
|  [05]   | `@rasm/illustrator-scripts`   | One typed entry per Illustrator tool over `types-for-adobe`, prelude, emitted `.jsx`     |
|  [06]   | `WindowList`                  | Accessibility window-list reader the validators run beside screenshots                   |

| [INDEX] | [NAME]                        | [DEPENDENCIES]                                                                                |
| :-----: | :---------------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `@rasm/typography`      | `effect`, `fontkit`, `@types/fontkit`                                                         |
|  [02]   | `@rasm/creative-cloud-server` | `@rasm/typography`, `effect`, `@effect/platform-node`                                   |
|  [03]   | `@rasm/indesign-plugin`       | `@rasm/creative-cloud-server`, `effect`, `@adobe-uxp-types/uxp`, `vite`, `vite-uxp-plugin`    |
|  [04]   | `@rasm/photoshop-plugin`      | Row [03] plus `@adobe-uxp-types/photoshop`                                                    |
|  [05]   | `@rasm/illustrator-scripts`   | `types-for-adobe`, `@swc/core`, `@swc/cli`                                                    |
|  [06]   | `WindowList`                  | None                                                                                          |

`tools/nx/workspace.ts` infers each project from its manifest: a `tsconfig.json` gives tag `language:typescript` with targets `typecheck` and `check` (lines 27, 44), a `project.pbxproj` gives tags `language:swift` and `host:macos` with targets `build`, `install`, `lint`, `format`, `check` and names the project from the `.xcodeproj` basename (lines 28–33). Targets beyond those are `nx.targets` rows of the project's `package.json`, one tool per target with its arguments on the command.

| [INDEX] | [PROJECT]                     | [TARGET]                                      | [BODY]                                            |
| :-----: | :---------------------------- | :-------------------------------------------- | :------------------------------------------------ |
|  [01]   | `@rasm/typography`      | `grid`, `oracle`, `typecheck`, `check`        | `node automation.ts grid`; `node automation.ts oracle` |
|  [02]   | `@rasm/creative-cloud-server` | `deploy`, `typecheck`, `check`                | `node automation.ts deploy`; no `build`, no `serve` |
|  [03]   | `@rasm/indesign-plugin`       | `build`, `deploy`, `typecheck`, `check`       | `vite build`; `node automation.ts deploy`         |
|  [04]   | `@rasm/photoshop-plugin`      | `build`, `deploy`, `typecheck`, `check`       | `vite build`; `node automation.ts deploy`         |
|  [05]   | `@rasm/illustrator-scripts`   | `build`, `typecheck`, `check`                 | `swc` to `.artifacts/creative-cloud/illustrator/` |
|  [06]   | `WindowList`                  | `build`, `install`, `lint`, `format`, `check` | Inferred from `project.pbxproj`                   |

A plugin's `deploy` runs with `parallelism false` and `dependsOn ["build"]`: it copies the built folder to `~/Library/Application Support/Adobe/UXP/Plugins/External/<id>_<version>/`, decodes `PluginsInfo/v1/{PS,ID}.json` through `Schema`, replaces the row with the same `pluginId`, and writes it back at the file's own mode. The server's `deploy` copies `acrobat/rasm-trusted.js` and `acrobat/actions/*.sequ` into Acrobat's `DC/JavaScripts/` and `DC/Sequences/` folders with Acrobat quit. `WindowList` builds to `.cache/xcode/apps/creative-cloud/window-list/Build/Products/Debug/WindowList`; its `install` target is inferred and never run, because the product is a command-line tool.

`.mcp.json` row `creative-cloud` runs the server from source as `mise exec -- node apps/creative-cloud/server/main.ts`, a project-relative path, and every relative import carries `.ts`. Files sit at the project root:

| [INDEX] | [FILE]                             | [HOLDS]                                                                      |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `main.ts`                          | One `Layer`: `McpServer.layerStdio` over the merge of each tool file's layer and one rpc listener per socket host, provided with the `Hosts`, `Links`, and `Jobs` layers, `NodeServices.layer`, `NodeRuntime.runMain` with the exit-code teardown |
|  [02]   | `hosts.ts`                         | Host table as data, the start reads as one record keyed by host, the resolved row `Schema`, the `Hosts` service key |
|  [03]   | `errors.ts`                        | `BridgeError` variants, the `HostRejection` union, `classify`                |
|  [04]   | `values.ts`                        | Domain values, one `Schema` each, the `DPI` bounds, the `TIMEOUT_DEFAULT` and `PROBE_MS` figures |
|  [05]   | `frames.ts`                        | The `Frames` rpc group with its payload, job, and exit schemas, the `Link` schema, the file both plugins import |
|  [06]   | `osascript.ts`                     | Spawner boundary `reply`, `read` assembling the one template, the osascript `probe` |
|  [07]   | `socket.ts`                        | The `Links` service key, the rpc handlers over the link `Ref`, `serve` as the rpc server layer per socket host, `dispatch` |
|  [08]   | `jobs.ts`                          | The `Jobs` service key, in-flight `Ref`, deadline, `run` and `probe` claims, result spill |
|  [09]   | `images.ts`                        | Image budget table and formula, the `Raster` result `Schema`                 |
|  [10]   | `health.ts`                        | The `health` tool with its handler, the `probe` and `link` table keyed by `channel`, the one `Layer` main merges, requiring `Hosts`, `Links`, and `Jobs` |
|  [11]   | `<host>/tools.ts`, `<host>/*.json` | One host's tool table, data files, and job assembly for osascript hosts, exported as one `Layer` |

`tsconfig.json` lists `package.json` under `files`: `main.ts` and `health.ts` import it as a JSON module for `name` and `version`, so no file read, no manifest `Schema`, and no server service exist. No host directory exists until its unit lands the tool table: the probe body of a channel is the channel's, `execute` of `'1'` over a socket and `get version` over osascript, so `health.ts` reads both from the transport files and no per-host seed file forwards them.

## [02]-[CODE_BAR]

Every line of the bar is a form the tree already holds, with the file that holds it.

| [INDEX] | [LINE]                                                                       | [FILE]                                                   |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Policy is a pure function from a parsed value to a tagged decision           | `hooks/composition.ts` (`Policy`, `Decision`)            |
|  [02]   | Independent checks fold in order and the first refusal answers               | `hooks/composition.ts` (`fold`)                          |
|  [03]   | Dependent steps bind and short-circuit inside one `Effect.gen`               | `tools/yak/rhino-mcp-platform.ts` (entry generator)      |
|  [04]   | Independent steps accumulate every error                                     | `tools/yak/rhino-mcp-platform.ts` (`Effect.partition`)   |
|  [05]   | One registration point per concern, state created there and passed down      | `hooks/register.ts` (`register`, `claims`, `opening`)    |
|  [06]   | Closed tagged unions read by `kind` or `_tag`, never by message text         | `hooks/composition.ts`, `hooks/register.ts`              |
|  [07]   | Non-zero exit is a value a total function classifies, exceptions are defects | `hooks/register.ts` (`_run`)                             |
|  [08]   | Data is a table of rows beside the code, decoded once through `Schema`       | `tools/nx/workspace.ts` (`configurations`)               |
|  [09]   | Boundary provides services and translates the result, domain code stays pure | `tools/yak/rhino-mcp-platform.ts` (entry pipe)           |
|  [10]   | Module banners `[IMPORTS]` to `[EXPORTS]`, `_`-prefixed private bindings     | Every file cited in this table                           |
|  [11]   | Exports at the end in one `export {}` and one `export type {}`               | `hooks/composition.ts`, `tools/nx/workspace.ts`          |
|  [12]   | One tool per target, arguments on the command, configuration in the tool's own file, no wrapper target | `341884d02` (`package.json` `nx`, `biome.json`)         |
|  [13]   | Rules sit at their documented defaults, an override holds one concern under its own `includes` | `341884d02` (`biome.json` `overrides`)                   |
|  [14]   | Setting that holds tree-wide sits once at the tool's top level, a per-file toggle covers one file's fact | `biome.json` (`linter.domains.qwik`)                     |
|  [15]   | Inputs name the files a tool reads once through a named input and the tool version as `runtime`   | `341884d02` (`nx.json` `namedInputs`, README [03])       |
|  [16]   | Host access goes through Effect services, `NodeServices.layer` is provided once at the entry boundary | `341884d02` (`tools/nx/workspace.ts`)                    |
|  [17]   | Library client replaces a hand-rolled service the library constructs                              | `CLAUDE.md` [DIRECTNESS], `tests/typescript/support/resources.ts` |
|  [18]   | Helper earns its name by a second call or a checker (`noNestedPromises`, nesting depth, `useTopLevelRegex`), a single-use helper inlines | Rebuild mandate, `hooks/register.ts`, `hooks/text/command.ts` |
|  [19]   | Literal stays inline unless a checker names a constant (`noMagicNumbers`) or a second use exists  | Rebuild mandate, `hooks/observation/delivery.ts`         |
|  [20]   | Type alias exists for a second reference or a checker, one interface carries a shape its users share | Rebuild mandate, `hooks/observation/row.ts` (`Columns['trims'][number]`) |
|  [21]   | Catalog row is a first-party package with a 4.x line and a role the tree holds, one override row per peer the catalog crosses | `CLAUDE.md` [DEPENDENCY_SOURCES], `pnpm-workspace.yaml` |
|  [22]   | Loader the runtime supplies leaves the catalog                                                     | `pnpm-workspace.yaml` (`tsx`, `@swc-node/register`)      |
|  [23]   | Domain module imports its services from `effect` alone and names them in `R`, the Node constructor stays at the edge | `apps/creative-cloud/server/socket.ts` (`serve` requiring `SocketServer` from `unstable/socket/SocketServer.d.ts:27`, provided by `main.ts`) |
|  [24]   | Entry point is the one importer of `@effect/platform-node`, `Effect.provide(NodeServices.layer)` once before `NodeRuntime.runMain` | `apps/creative-cloud/server/main.ts`, `infra/automation.ts` (`NodeServices.d.ts:33`, `NodeRuntime.d.ts:27`, `NodeSocketServer.d.ts:78`) |
|  [25]   | Process output runs through the `Stdio` sink, no `process` or `Buffer` global in a module that imports `effect` | `infra/automation.ts` (`_operation` over `Stdio.d.ts:75`–`:77`, `Stream.d.ts:567`, `Queue.d.ts:611`, `:874`) |
|  [26]   | `types` carries `node` only where `import.meta.dirname` or a `node:` module is read, `[]` elsewhere | `tests/typescript/support/tsconfig.json`, both plugin `tsconfig.json` (`@types/node/web-globals/importmeta.d.ts:6`–`:12`) |
|  [27]   | Test file provides the platform once through `@effect/vitest`'s `layer`, each case stays an `Effect` | `libs/typescript/typography/*.test.ts` (`@effect/vitest/dist/index.d.ts:187`) |
|  [28]   | Exported schema is annotated `Schema.Codec<T, unknown>`, the type alias stands for `T` alone and the encoded side is read from the schema at run time | `apps/creative-cloud/server/errors.ts`, `hosts.ts`, `jobs.ts` (`Schema.d.ts:816`; `isolatedDeclarations` TS9010 with TS9013 refuses `typeof` of an unannotated local, re-proven 2026-09-15 on `Schema.Codec<typeof _X.Type, unknown>`, `Schema.Schema.Type<typeof _X>`, and an exported alias `typeof _X.Type`; `type X = typeof X.Type` over an annotated export alone compiles and states the type twice) |
|  [29]   | Tool file exports one `Layer` built from `McpServer.toolkit` over `toolkit.toLayer`, the tool and its handler stay local and inferred | `apps/creative-cloud/server/health.ts` (`McpServer.d.ts:300`, `Toolkit.d.ts:64`) |
|  [30]   | Rows read by key: `Effect.all` over a record keyed by the closed set, `Match.discriminatorsExhaustive` over the discriminant, no array filtered by membership | `apps/creative-cloud/server/hosts.ts` (`resolve`), `health.ts` (`Effect.d.ts:387`, `Match.d.ts:924`) |
|  [31]   | Generator-bodied function is `Effect.fnUntraced`, the body sits at nesting depth zero and the export annotation sits on the const; a generic body keeps the row's literals (`function* <H extends Row>`) | `apps/creative-cloud/server/osascript.ts` (`reply`), `hosts.ts` (`_resolved`), `socket.ts` (`_attach`, `_settle`, `dispatch`), `health.ts` (`_row`, `_answer`) (`Effect.d.ts:17708`–`:17722`) |
|  [34]   | One owner per fact: the in-flight `Ref` answers `hostBusy` for every host and the probe claims it unrecorded, the link holds the pending job alone and a `settle` is matched to it by `jobId` | `apps/creative-cloud/server/jobs.ts` (`run`), `socket.ts` (`dispatch` awaiting the pending `Deferred`, `Deferred.d.ts:146`, `:147`) |
|  [35]   | Independent reads combine in one `Effect.all`, a dependent read binds; a start-time key read never chains after a read it does not consume | `apps/creative-cloud/server/hosts.ts` (`resolve`, `Effect.d.ts:387`) |
|  [32]   | Client-initiated end of a stdio server is an interruption the entry point maps to exit 0 through `runMain`'s `teardown` | `apps/creative-cloud/server/main.ts` (`Runtime.d.ts:46`, `:93`, `Cause.d.ts:596`, `RpcServer.js:881`) |
|  [33]   | Logger output of a stdio server goes to stderr through `Layer.succeed(Logger.LogToStderr, true)` at the entry, because the default logger writes every level through `console.log` and stdout is the protocol | `apps/creative-cloud/server/main.ts` (`Logger.d.ts:160`, `internal/effect.js:3327`) |
|  [36]   | Every tool's `success` is one `Schema.Struct` at the top level; a tool with more than one result shape carries them as one closed `Schema.Union` read by `kind` under one field of that struct, the `{kind: 'error', error: BridgeError}` member included, because the MCP layer publishes `outputSchema` only when the derived JSON Schema has `type: "object"`, which a top-level union (`anyOf`, no `type`) lacks | `apps/creative-cloud/server/health.ts` (`McpServer.js:1121`, `Tool.d.ts:1191`; measured 2026-09-15: a `kind` union derives `{anyOf}` alone, a struct `type: "object"`, a struct with the union under one field `type: "object"`) |
|  [37]   | A schema in an invariant position (`RpcGroup<in out R>`) is annotated with its exact schema type (`Schema.Struct<{…}>`, `Schema.Union<readonly […]>`, `Schema.TaggedStruct<Tag, Record<never, never>>`) and the TypeScript type derives as `(typeof X)['Type']`, so the shape is stated once; `Schema.Codec<T, unknown>` (row [28]) stays where the schema stands alone | `apps/creative-cloud/server/frames.ts` (`RpcGroup.d.ts:31`, `Schema.d.ts:2758`, `:3852`, `:4819`, `:127`) |
|  [38]   | A service is a function-style `Context.Service<Shape>('Key')` key annotated `Context.Service<Shape, Shape>`, the form `Socket.Socket` and `KeyValueStore.KeyValueStore` take, because an exported class with a call in its heritage is TS9021 under `isolatedDeclarations`; its layer is `Layer.effect(Key, build)` at the entry and a consumer reads it with `Key.useSync` or by yielding the key | `apps/creative-cloud/server/hosts.ts`, `jobs.ts`, `socket.ts`, `main.ts` (`Context.d.ts:57`, `:94`, `:239`, `Socket.d.ts:49`, `KeyValueStore.d.ts:174`, `Layer.d.ts:1209`) |
|  [39]   | A layer constant built under two different dependencies takes `Layer.fresh`, because a layer is memoized by identity across one build and `Layer.provide` builds `self` in the shared memo map | `apps/creative-cloud/server/socket.ts` (`serve` over `RpcServer.layerProtocolSocketServer`, `Layer.d.ts:3411`, `Layer.js:130`, `:914`, `:1475`) |
|  [40]   | The transport protocol is the library's: framing, request ids, acks, ping, reconnect, and disconnect belong to `RpcServer` and `RpcClient`; the code owns the contract as one `RpcGroup` and the state machine over it | `apps/creative-cloud/server/frames.ts`, `socket.ts` (`RpcServer.d.ts:71`, `:200`, `RpcClient.d.ts:116`, `:237`) |

Each replacement below was read from the installed declaration its last column names (`effect@4.0.0-rc.115` and its platform packages).

| [INDEX] | [BEFORE]                                                                         | [AFTER]                                                                                                                  | [DECLARATION]                                        |
| :-----: | :------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Schema.transformOrFail(from, to, { decode: ParseResult.fromOption(...) })`      | `from.pipe(Schema.decodeTo(to, { decode: SchemaGetter.transformOptional(Option.flatMap(f)), encode: SchemaGetter.forbidden(...) }))` | `Schema.d.ts:4404`, `SchemaGetter.d.ts:521`, `:150`  |
|  [02]   | `Schema.parseJson(schema)`                                                       | `Schema.fromJsonString(schema)`                                                                                          | `Schema.d.ts:6729`                                   |
|  [03]   | `Schema.decode(s)`, `Schema.decodeUnknown(s)`, `Schema.encode(s)`               | `Schema.decodeEffect(s)`, `Schema.decodeUnknownEffect(s)`, `Schema.encodeEffect(s)`                                      | `Schema.d.ts:1192`, `:1541`                          |
|  [04]   | `Schema.filter`, `Schema.pattern`, `Schema.int`, `Schema.endsWith`               | `Schema.check(Schema.isPattern(re))`, `Schema.check(Schema.isInt())`, `Schema.check(Schema.isEndsWith(s))`               | `Schema.d.ts:5285`, `:5796`, `:5474`                 |
|  [05]   | `Schema.Literal(...Struct.keys(record))`                                         | `Schema.Literals(Struct.keys(record))`                                                                                   | `Schema.d.ts:3960`                                   |
|  [06]   | `Config.boolean('CI')`, `Config.string('HOME')`                                  | `Config.Boolean('CI')`, `Config.String('HOME')`                                                                          | `Config.d.ts:1011`, `:780`                           |
|  [07]   | `NodeContext.layer` from `@effect/platform-node`                                 | `NodeServices.layer`                                                                                                     | `NodeServices.d.ts:33`                               |
|  [08]   | `(x) => Effect.gen(...)` plus `Effect.runPromise(Effect.provide(f(x), layer))`   | `Effect.fnUntraced(function* (x) {...}, Effect.orDie, Effect.provide(NodeServices.layer))` and `flow(f, Effect.runPromise)` | `Effect.d.ts:17722`, `:6013`                         |
|  [09]   | `Command.run(cmd, { name, version })(process.argv)`                              | `Command.run(cmd, { version })`, argv from the `Stdio` service                                                           | `unstable/cli/Command.d.ts:2171`                     |
|  [10]   | `Options.boolean('import')`                                                      | `Flag.Boolean('import').pipe(Flag.withDescription(text), Flag.withDefault(false))`                                       | `unstable/cli/Flag.d.ts:66`, `:492`, `:722`          |
|  [11]   | `@effect/platform` `Command.make`, `CommandExecutor.string`                     | `ChildProcess.make(tool, args, { stderr: 'inherit' })`, `spawner.spawn`, `handle.exitCode`, `Stream.mkString(Stream.decodeText(handle.stdout))` | `unstable/process/ChildProcess.d.ts:456`, `ChildProcessSpawner.d.ts:81`, `:107`, `Stream.d.ts:14976` |
|  [12]   | `Effect.validateAll(items, f)`                                                   | `Effect.partition(items, f)`, then `Array.match(failures, { onEmpty, onNonEmpty: Effect.fail })`                         | `Effect.d.ts:419`, `Array.d.ts:484`                  |
|  [13]   | `TestDatabase` service over `PGlite.create`, `exec`, `query`, `listen`           | `PgliteClient.layer({ dataDir: 'memory://', relaxedDurability: true })`, seeds through `SqlClient.SqlClient.use((sql) => Effect.forEach(seed, (s) => sql.unsafe(s), { discard: true }))` under `Layer.effectDiscard` and `Layer.provideMerge` | `PgliteClient.d.ts:167`, `:60`, `SqlClient.d.ts:60`, `Context.d.ts:93`, `Layer.d.ts:1279`, `:2116` |
|  [14]   | `ObjectStore` service and `memoryObjectStore` (`Ref` over `HashMap`, byte order) | `KeyValueStore.layerMemory` from `effect/unstable/persistence`                                                           | `unstable/persistence/KeyValueStore.d.ts:230`        |
|  [15]   | `HttpApp.Default<E>`                                                             | `Effect.Effect<HttpServerResponse, E, HttpServerRequest \| Scope>` served by `HttpServer.serve` over `NodeHttpServer.layerTest` | `unstable/http/HttpServer.d.ts:69`, `NodeHttpServer.d.ts:119` |
|  [16]   | `MetricKey`, `MetricPair`, `MetricState`                                         | `Metric.Metric.Snapshot` matched by `type` through `Match.discriminatorsExhaustive('type')`                              | `Metric.d.ts:1478`, `Match.d.ts:924`                 |
|  [17]   | `FastCheck` from `effect`                                                        | `Arbitrary` from `effect/unstable/arbitrary` for `it.effect.prop`, `fc` from `fast-check` for schedulers and model runs  | `unstable/arbitrary/Arbitrary.d.ts:581`, `:598`, `@effect/vitest/dist/index.d.ts:38` |
|  [18]   | `Effect.forkDaemon`, `Effect.catchAll`, `Option.fromNullable`                    | `Effect.forkDetach`, `Effect.catch`, `Option.fromNullishOr`                                                              | `Effect.d.ts:16394`, `Option.d.ts:999`               |
|  [19]   | `domains: { qwik: "none" }` on two `biome.json` overrides                        | `linter.domains.qwik: "none"` once                                                                                       | `configuration_schema.json` `RuleDomains`, `RuleDomainValue` |
|  [20]   | Hooks `composition/{option,result,decision}.ts`, `text/path.ts`, `Values` alias, `bind`, `when` | `hooks/composition.ts`, `basename` in `hooks/text/command.ts`, `Values` inlined into `all`, `bind` and `when` inlined at their one call | `.claude/types/claude-code.d.ts` header (no npm import in a hooks module) |
|  [21]   | `Trim` alias, `_READ`, `_RESPONSE`, `_FILE` string constants                     | `Columns['trims'][number]`, inline `'Read'`, `['tool_response']`, `'file'`                                               | `hooks/observation/row.ts`                           |
|  [22]   | `Schema.Codec<T, TEncoded>` with a hand-written `*Encoded` alias per exported schema, fifteen in the server | `Schema.Codec<T, unknown>`, `E` covariant so the schema assigns, no `*Encoded` alias                                    | `Schema.d.ts:816`                                    |
|  [23]   | `_REPORT` regex, `_Report` struct over `exec().groups`, `Schema.decodeUnknownOption` of the struct | `Schema.TemplateLiteralParser([Schema.String, ': ', Schema.Literals(['syntax', 'execution']), ' error: ', Schema.String, ' (', Schema.NumberFromString, ')'])` decoded by `Schema.decodeUnknownOption`, the tuple's positions 0, 2, 4, 6 destructured | `Schema.d.ts:2316`, `:1281`                          |
|  [24]   | `Probe` hand union `{ok: true} \| {ok: false, error}`, `probed` over `Effect.match` | `Result.Result<Schema.Json, BridgeError>` from `Effect.result`, carrying the host's answer, encoded in the health row through `Schema.toCodecJson(Schema.Result(Schema.Json, BridgeError))`; `Schema.Result` alone encodes to a `Result` instance, which the MCP layer's `Schema.is(Schema.Json)` refuses (`McpServer.js:59`) | `Effect.d.ts:3420`, `Schema.d.ts:9213`, `:10202`     |
|  [25]   | `classify(host)(reply)` returning `Option<BridgeError>`, `_answer` lifting stdout on `None` | `classify(host, reply)` returning `Result.Result<string, BridgeError>`, lifted once by `Effect.fromResult`               | `Effect.d.ts:2353`                                   |
|  [28]   | `_probe` and `_link`, each a four-arm `Match.when({ id })` importing the host files | One `Match.discriminatorsExhaustive('id')` table yielding `{ probe, link }` per host                                     | `Match.d.ts:924`                                     |
|  [29]   | `fs.readFileString(path.join(import.meta.dirname, 'package.json'))` through `Schema.fromJsonString(Schema.Struct({ name, version }))` | `import manifest from './package.json' with { type: 'json' }`, `package.json` listed in `tsconfig.json` `files`         | `tsconfig.json` (`module: preserve` implies `resolveJsonModule`) |
|  [30]   | `health: Tool.Tool<'health', { parameters; success; failure; failureMode }, Services>` and `handler` exported, `Row`, `Health`, `Selection` and their `*Encoded` aliases | `layer(context)` exported alone, the `Tool.make` and its handler local under `toolkit.toLayer`, every row type inferred | `Toolkit.d.ts:64`, `McpServer.d.ts:300`              |
|  [31]   | `Layer.launch(Layer.mergeAll(health, McpServer.layerStdio(...), sockets))`, the process outliving its client | `Layer.provideMerge(McpServer.layerStdio(...), Layer.mergeAll(health, sockets))`, so the stdio protocol's end-of-input interrupt reaches the main fiber, and `runMain({ teardown })` answering exit 0 to an interrupts-only exit | `Layer.d.ts:2116`, `RpcServer.js:881`, `Runtime.d.ts:46`, `:93` |
|  [32]   | `Effect.flatMap(FileSystem.FileSystem, (fs) => ...)`, `Effect.gen` yielding `Crypto.Crypto` and `SocketServer.SocketServer` for one call | `FileSystem.FileSystem.use(...)`, `Crypto.Crypto.use(...)`, `SocketServer.SocketServer.use(...)`                        | `Context.d.ts:93`                                    |
|  [33]   | `Effect.flatMap(output(command), (reply) => reply.exitCode === 0 ? Effect.succeed(...) : Effect.fail(...))` | `Effect.filterOrFail((reply) => reply.exitCode === 0, hostKeyExit)` then `Effect.map`                                    | `Effect.d.ts:9090`                                   |
|  [34]   | `_illustrator`, `_photoshop`, `_indesign`, `_acrobat` readers and `readonly Resolved[]` | `Effect.all({ illustrator, photoshop, indesign, acrobat })` inside `resolve`, `Readonly<Record<HostId, Resolved>>`      | `Effect.d.ts:387`                                    |
|  [35]   | `Hosts` interface restating the table's channels and ports, `as const satisfies` on the export | `HOSTS` as `as const`, `resolve` reads `id`, `channel`, and `port` from the rows and `Resolved`'s literals check them    | `isolatedDeclarations` TS9010, TS9017                |
|  [36]   | `_DETAIL`, `_PREVIEW`, and the dpi bounds stated in `images.ts` beside `values.ts` | `_BUDGET` table keyed by `PixelBudget`, `DPI` bounds once in `values.ts` shared by the `Dpi` schema and `Number.clamp`  | `Number.d.ts:868`                                    |
|  [37]   | `_UUID_VERSION`, `_TIMEOUT`, `_ARTBOARD`, `_FRACTION` constants, `_ON_DEMAND`, `_PHOTOSHOP_STORAGE` segment arrays, `_BEAT_EVERY` | Literals inline at their one use; `_UUID_VERSION`, `_LIVE_READ_MS`, `_SPILL_CHARACTERS`, and `_CLOSE_NORMAL` stay because `noMagicNumbers` names them | `biome.json` (`style.noMagicNumbers`)                |
|  [38]   | Link `busy{identity, job, startedAt, lastBeat}` beside the in-flight `Ref`, `_claimed` and `_settled` transitions, `probe` dispatching past the `Ref` | Link `listening \| attached`, `dispatch` reads the attached link and awaits its pending `Deferred`, every probe runs through `probe` of `jobs.ts` with the `identity` settle | `Ref.d.ts:397`, `Deferred.d.ts:146`, `:147` |
|  [39]   | `<host>/health.ts` per host, four identical `probe` and `link` pairs folded by `Match.discriminatorsExhaustive('id')` | One `Match.discriminatorsExhaustive('channel')` table of two arms in `health.ts`, the row's `id` keying `jobs` and `endpoints` | `Match.d.ts:924`                                     |
|  [40]   | `Match.whenOr({phase: 'syntax'}, {code: -2740}, {code: -1728})` answering `scriptNotCompiled` to an execution-phase `-1728` | `Match.when({phase: 'syntax'})` alone, every execution error reads its code, `-1728` at execution is `hostRejected` | `Match.d.ts:671`, [07] code table                    |
|  [41]   | `_common` returning `{base, installFolder}` with `id` widened to `HostId`, each host re-stating `id`, `channel`, and `port`, `_live` chained after `_common` | Generic `_resolved<H extends Row>` returning the row minus `prefs` and `support` plus the five read keys, `Effect.all` over `_resolved` and the independent reads | `Struct.d.ts:324`, `Effect.d.ts:387`                 |
|  [42]   | `_settled` clearing `lastError` on success, the deadline unrecorded, `deadline` and `freshJobId` exported for the probe | `_recorded` keeping `lastError` until the next failure as Sidekick's `recordCommandSuccess` does, the deadline recorded as `deadlineExceeded` with its count, both local to `run` | `Exit.d.ts:722`, `Cause.d.ts:903`, `multi-client-bridge.js:69`–`:81` |
|  [43]   | `Run.record: boolean` selecting the path inside `_recorded`, `_record` then a second `Ref.update` clearing `inFlight`, `Effect.result(run(..., {record: false}))` in each transport | `_job(host, jobs, timeoutMs, settle, work)` with `settle: typeof _recorded` as the transition value, one `Ref.update` of `flow(settle(exit, at), _released)` at the exit and of `settle(Exit.fail(deadline), at)` at the deadline, `run` passing `_recorded` and `probe` passing `() => identity` under `Effect.result` | `Ref.d.ts:587`, `Function.d.ts:314`, `Effect.d.ts:3420` |
|  [44]   | `Reply` interface, `output` exported, `_run` in `osascript.ts`, `_stdout` in `hosts.ts` failing `hostKeyExit` | `reply(host, command)`: the spawn under `Effect.orDie` and `classify` returned as a `Result` the pipe lifts through `Effect.flatMap(Effect.fromResult)`, called by `read`, the `mdfind` read, and the `PlistBuddy` read | `Effect.d.ts:2353`, `:2523`, `:17722` |
|  [45]   | `read` spawning `running of application id` and then the statement, answering `Option<string>` under `Effect.when`; the probe lifting `None` to `hostNotRunning`; `_live` transposing the `Option` | One script whose first line is `if not running of application id "<id>" then error number -600`, classified `hostNotRunning` by the existing `-600` arm; `_live` recovering with `Effect.catchTag('hostNotRunning', () => Effect.succeedNone)` after `Effect.asSome` | `Effect.d.ts:4219`, `:3804`, `:1430` |
|  [46]   | `_absolute` and `_entry` over `fs.exists`, `Effect.filterOrFail(identity)`, and `hostKeyMissing{path}` | `_existing = Schema.decodeEffect(AbsolutePath.pipe(Schema.decodeTo(AbsolutePath, {decode: SchemaGetter.checkEffect(fs.exists), encode: SchemaGetter.passthroughSupertype()})))`, the issue carrying the path as its actual value | `Schema.d.ts:4404`, `SchemaGetter.d.ts:415`, `:249` |
|  [47]   | `Array.findFirst(String.linesIterator(found), String.isNonEmpty)` under `Option.match` naming `hostKeyMissing` | `_bundles = Schema.decodeEffect(Schema.String.pipe(Schema.decodeTo(Schema.NonEmptyArray(AbsolutePath), {decode: SchemaGetter.transform(String.split('\n')), encode: SchemaGetter.transform(Array.join('\n'))})))` then `Array.headNonEmpty`; `Schema.decodeEffect(Schema.NonEmptyString)` for the version | `Schema.d.ts:3735`, `:6335`, `SchemaGetter.d.ts:453`, `String.d.ts:454`, `Array.d.ts:1824`, `:8508` |
|  [48]   | Three-variant `HostKeyError` with the key restated inside `_absolute`, `_entry`, `_stdout`, and `_live`; `resolve` restating each host's reads | One `unresolved{host, key, cause: BridgeError \| SchemaError}` attached by `_key` alone; `_resolved(host, home, keys)` reading the five common keys and the host's extra rows as one `Effect.all` the host arm passes; `Effect.all` in rc.115 offers `mode: 'default' \| 'result'` alone, so no `validate` mode accumulates the keys | `Effect.d.ts:387`, `:5767` |
|  [51]   | `doJavascript` in `osascript.ts` with no caller | Deleted; the Illustrator unit assembles its statement beside its job files (`illustrator.md` [03]) | — |
|  [52]   | Hand `Frame` union of six tagged structs, `FrameText` through `Schema.fromJsonString`, `Socket.readerString` batches, the first batch decoded as `hello`, `Match` over each frame | One `RpcGroup.make(Rpc.make('attach', {payload, success: Job, error: AlreadyAttached, stream: true}), Rpc.make('settle', {payload}), Rpc.make('state', {payload}))`, served by `RpcServer.layer(Frames)` under `RpcServer.layerProtocolSocketServer` and `RpcSerialization.layerJson`, the handlers from `Frames.toLayer(Links.useSync(...))` over `Frames.of` | `Rpc.d.ts:465`, `RpcGroup.d.ts:66`, `:67`, `:160`, `RpcServer.d.ts:71`, `:200`, `RpcSerialization.d.ts:147` |
|  [53]   | `serve` calling `SocketServer.run` with `Effect.scoped(_attach(endpoint, socket))` per connection, the `Writer` held on the link | The socket server protocol of the rpc server, one client id per socket, `disconnects` offered when its scope closes, the request fibers of that client interrupted; `Layer.fresh` around the protocol constant so each listener builds its own | `RpcServer.js:969`–`:985`, `:404`–`:411`, `:75`–`:83`, `Layer.d.ts:3411` |
|  [54]   | Beat loop `Effect.repeat(writer.write(beat), Schedule.spaced(HEARTBEAT.beatMs))`, `Stream.timeout(HEARTBEAT.expiryMs)` expiry, `lastBeat` on the link, the `HEARTBEAT` figures in `frames.ts` | The client protocol's `Ping` every 5 s answered by the server's `Pong`, a missed `Pong` failing the client connection as `SocketOpenError{kind: 'Timeout'}`, which `retryTransientErrors` reconnects on `Schedule.min(exponential(500, 1.5), spaced(5000))`; no server timer, no figure, no `lastBeat` | `RpcClient.js:601`, `:667`–`:672`, `:682`, `:712`, `:727`, `RpcServer.js:526`–`:528`, `RpcMessage.d.ts:147`, `:353` |
|  [55]   | Reply `Queue<done \| failed, BridgeError>` on the connection, `dispatch` taking `until` the `jobId` matches, `Queue.fail` with `transportClosed` on close, `failed` mapped to `hostThrew` in `dispatch` | `pending: Option<{jobId, settled: Deferred<Done, BridgeError>}>` on the attached link, set by `dispatch` in the same `Ref.modify` that reads the link, `settle` completing the matching `Deferred` through `Deferred.done` of `Result.match(exit, {onSuccess: Exit.succeed, onFailure: hostThrew})` and otherwise a no-op, the `attach` release failing a pending `Deferred` with `transportClosed{code: 1000}` | `Deferred.d.ts:146`, `:147`, `:446`, `:551`, `Ref.d.ts:240`, `:397`, `Result.d.ts:1095`, `Exit.d.ts:207`, `:265` |
|  [56]   | `Effect.acquireRelease` around the link claim and the frame loop, a `CloseEvent` written when the loop ends | `Effect.acquireRelease` around the link claim alone, returning `Queue.make<Job>()` as the streaming result the server drains to the client as `Chunk` messages until the request ends | `Effect.d.ts:12118`, `Queue.d.ts:399`, `Rpc.d.ts:443`, `RpcServer.js:279`–`:294` |
|  [57]   | `Context` interface `{server, hosts, endpoints, jobs}`, `layer(context)` in `health.ts`, `Layer.unwrap(Effect.gen(...))` in `main.ts` building the state and passing it down | `Hosts`, `Links`, and `Jobs` keys owned by `hosts.ts`, `socket.ts`, and `jobs.ts`, `Layer.effect(Key, …)` for each at the entry under `Layer.provide`, `health.ts` exporting one `Layer` requiring them, `startedAt` and the three services read once in `toolkit.toLayer(Effect.map(Effect.all([Clock.currentTimeMillis, Hosts, Links, Jobs]), …))` | `Context.d.ts:239`, `:94`, `Layer.d.ts:1209`, `:1704`, `Toolkit.d.ts:54`, `:64` |
|  [58]   | `lastError.reason: string`, the `_tag` or `'defect'` | `lastError.error: BridgeError` through `Cause.findErrorOption`, `count` climbing while `error._tag` repeats, a defect or interrupt leaving `lastError` unchanged; `health` encodes `{error: BridgeError, count, secondsAgo}` | `Cause.d.ts:927` |
|  [59]   | `HostKeyError.unresolved.key: string` | `ResolvedKey = Exclude<Keys<Resolved>, Keys<Row>>` with the distributive `Keys<T> = T extends unknown ? keyof T : never`, so a key outside the read keys fails the typecheck | `hosts.ts` (`_key`) |
|  [60]   | `interface Identity`, `State`, `Job` and the `Frame`, `Link` aliases restating the schemas beside `Schema.Codec<T, unknown>` annotations | Exact schema-type annotations and `(typeof X)['Type']` derivations (row [37] of the bar) | `Schema.d.ts:127`, `:2758`, `:3852`, `:4819` |
|  [61]   | A tool `success` as a top-level `Schema.Union` on `kind` | One `Schema.Struct` with the closed `kind` union under one field, so `outputSchema` ships (row [36] of the bar) | `McpServer.js:1121`, `Tool.d.ts:1191` |

## [03]-[HOSTS]

`hosts.ts` holds the table as one `as const` value. Rows are a union by `channel`, so `port` exists on a socket host alone and the transport follows the same tag. Bundle ids are literals of the table, so a bad bundle id cannot reach osascript, where it would arrive as a compile-time `syntax error` with code `-1728`.

| [INDEX] | [HOST_ID]     | [BUNDLE_ID]                 | [PROCESS_NAME]               | [CHANNEL]   | [PORT] |
| :-----: | :------------ | :-------------------------- | :--------------------------- | :---------- | -----: |
|  [01]   | `illustrator` | `com.adobe.illustratorBeta` | `Adobe Illustrator`          | `osascript` |      — |
|  [02]   | `photoshop`   | `com.adobe.Photoshop`       | `Adobe Photoshop 2026`       | `socket`    |  39217 |
|  [03]   | `indesign`    | `com.adobe.InDesign`        | `Adobe InDesign 2026 (Beta)` | `socket`    |  39218 |
|  [04]   | `acrobat`     | `com.adobe.Acrobat.Pro`     | `AdobeAcrobat`               | `osascript` |      — |

Process names are the System Events spellings read on 2026-09-11; Illustrator's `Adobe Illustrator` also matches `pgrep -x`. Versions read the same day: Illustrator 30.9.0 build `72R`, Photoshop 27.11.0, InDesign 21.6.0.58, Acrobat 26.002.21901 with `app.viewerVersion` 26.00221901.

`main` resolves each host's keys before it serves, and `health` answers with the resolved table, so no artifact file records paths and every design file names a key instead of a literal.

| [INDEX] | [KEY]                  | [READ]                                                             |
| :-----: | :--------------------- | :----------------------------------------------------------------- |
|  [01]   | `<host>.bundlePath`    | `mdfind` on `kMDItemCFBundleIdentifier` through the spawner        |
|  [02]   | `<host>.version`       | `CFBundleShortVersionString` of `<bundlePath>/Contents/Info.plist` |
|  [03]   | `<host>.prefsFolder`   | `~/Library/Preferences` entry the host owns                        |
|  [04]   | `<host>.supportFolder` | `~/Library/Application Support/Adobe/` folder the host owns        |

Version reads are the plists read on 2026-09-11, and both preference folders were read the same day. `External/` under the support folder is proven for ten plugins. Resolved rows decode through one `Schema.Union` with a `Schema.Struct` per host, each carrying `id`, `bundleId`, `processName`, `channel`, `bundlePath`, `version`, `prefsFolder`, `supportFolder`, `installFolder`, `port` on the socket members, and the keys one host alone carries. Acrobat's bundle sits at `/Applications/Adobe Acrobat DC/Adobe Acrobat.app`.

Readbacks of 2026-09-15 through `hosts.ts`, each answered by the server's own `health` row: `mdfind` on `kMDItemCFBundleIdentifier` answers one line per bundle id, `/Applications/Adobe Illustrator (Beta)/Adobe Illustrator.app`, `/Applications/Adobe Photoshop (Beta)/Adobe Photoshop (Beta).app`, `/Applications/Adobe InDesign 2026 (Beta)/Adobe InDesign 2026 (Beta).app`, and `/Applications/Adobe Acrobat DC/Adobe Acrobat.app`; `CFBundleShortVersionString` reads `30.9.0`, `27.11.0`, `21.6.0.58`, and `26.002.21901`. The preference entries are `~/Library/Preferences/Adobe Illustrator 30.9.0 Beta Settings`, `Adobe Photoshop (Beta) Settings`, `Adobe InDesign (Beta)`, and the file `com.adobe.Acrobat.Pro.plist`. The support folders are `~/Library/Application Support/Adobe/Adobe Illustrator 30`, `Adobe Photoshop (Beta)`, and `Acrobat` (holding `DC` and `Distiller DC`); InDesign owns no folder under `~/Library/Application Support/Adobe/`, and its support tree is `~/Library/Preferences/Adobe InDesign (Beta)/Version 21.0-ME`, holding `InDesign Defaults`, `Scripts`, `StylePackPresets`, and `en_US`. `illustrator.onDemandModulesFolder` is `/Library/Application Support/Adobe/Adobe Illustrator (Beta)/OnDemandModules`, `photoshop.pluginData` is `UXP/PluginsStorage/PHSPBETA/27/External`, `indesign.featureSet` reads `righttoleft`, the lowercase spelling AppleScript gives the enumerator, and `acrobat.viewerVersion` reads `26.00221901`. The two live keys hold `Option.none()` when the read answers `hostNotRunning`; any other classified error on a read, and a path that does not exist, fails the start as `unresolved{host, key, cause}`.

| [INDEX] | [KEY]                                | [READ]                                                                                |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `indesign.installFolder`             | Parent folder of `<indesign.bundlePath>`                                              |
|  [02]   | `indesign.featureSet`                | `app.featureSet` through `do script … language javascript`, decoded as `Schema.Literal('righttoleft')` |
|  [03]   | `illustrator.installFolder`          | Parent folder of `<illustrator.bundlePath>`, holding `Presets.localized/en_US/`       |
|  [04]   | `illustrator.onDemandModulesFolder`  | `/Library/Application Support/Adobe/<basename of installFolder>/OnDemandModules/`     |
|  [05]   | `photoshop.installFolder`            | Parent folder of `<photoshop.bundlePath>`                                             |
|  [06]   | `photoshop.pluginsFolder`            | `~/Library/Application Support/Adobe/UXP/Plugins/External`                            |
|  [07]   | `photoshop.registry`                 | `~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/PS.json`                      |
|  [08]   | `photoshop.pluginData`               | `~/Library/Application Support/Adobe/UXP/PluginsStorage/PHSPBETA/27/External`         |
|  [09]   | `acrobat.viewerVersion`              | `app.viewerVersion` through one `do script`                                           |
|  [10]   | `acrobat.installFolder`              | Parent folder of `<acrobat.bundlePath>`, `/Applications/Adobe Acrobat DC`, the same common read every host takes |

## [04]-[VOCABULARY]

`errors.ts` holds one error family. Each variant is a member of one `Data.TaggedEnum` (`Data.d.ts:124`), whose constructor carries the explicit `Data.TaggedEnum.Constructor` annotation (`Data.d.ts:353`) `isolatedDeclarations` demands, beside one `Schema.Union` of `Schema.TaggedStruct` members (`Schema.d.ts:3921`, `:4869`) annotated `Schema.Codec<BridgeError, unknown>`, the encoded side read from the schema rather than restated; a `Schema.TaggedError` class cannot be exported under `isolatedDeclarations` (`TS9021`, `repo.md` [06]). Tags are the camelCase spellings of the rows below (`hostNotRunning`), because Biome's `useNamingConvention` governs the enum keys. A text field is named `reason`. `BridgeError` is the union of the eleven variants; `classify(host, reply)` returns `Result.Result<string, BridgeError>`, the trimmed stdout on exit 0 and the classified error otherwise, which `Effect.fromResult` (`Effect.d.ts:2353`) lifts once in `osascript.ts`. `Probe` is `Result.Result<Schema.Json, BridgeError>`, the value `Effect.result` (`Effect.d.ts:3420`) gives a live call, whose success carries the host's answer: the `get version` text of an osascript host, the `done.value` of a socket host. `host` is a `HostId` from the host table.

| [INDEX] | [VARIANT]            | [FIELDS]                           | [RAISED_BY]          |
| :-----: | :------------------- | :--------------------------------- | :------------------- |
|  [01]   | `HostNotRunning`     | `host`                             | Illustrator, Acrobat |
|  [02]   | `AutomationDenied`   | `host`                             | Illustrator, Acrobat |
|  [03]   | `HostUnresponsive`   | `host`, `code`                     | Illustrator, Acrobat |
|  [04]   | `ScriptNotCompiled`  | `host`, `code`, `line`             | Illustrator, Acrobat |
|  [05]   | `HostRejected`       | `host`, `code`, `reason`           | Illustrator, Acrobat |
|  [06]   | `DeadlineExceeded`   | `host`, `jobId`                    | Every host           |
|  [07]   | `HostNotAttached`    | `host`                             | Photoshop, InDesign  |
|  [08]   | `HostBusy`           | `host`, `jobId`, `startedAt`       | Every host           |
|  [09]   | `TransportClosed`    | `host`, `code`, `reason`           | Photoshop, InDesign  |
|  [10]   | `ResultNotDecodable` | `host`, `text`, `reason`           | Every host           |
|  [11]   | `HostThrew`          | `host`, `rejection: HostRejection` | Every host           |

| [INDEX] | [VARIANT]            | [TRIGGER]                                                         | [EVIDENCE]                        |
| :-----: | :------------------- | :---------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `HostNotRunning`     | `running of application id` answers `false`, or code `-600`       | `-600` documented                 |
|  [02]   | `AutomationDenied`   | `execution error` with code `-1743` or `-1744`                    | Documented, unreproduced          |
|  [03]   | `HostUnresponsive`   | Code `-1712`, the host answers the next call                      | Measured on InDesign              |
|  [04]   | `ScriptNotCompiled`  | `syntax error:` line, `-2740` absent term, `-1728` bad bundle id  | Measured twice and three ways     |
|  [05]   | `HostRejected`       | Any other `execution error`: `-1708`, `-1728`, Photoshop `8800`   | Measured, `8800` on an alias      |
|  [06]   | `HostRejected`       | Illustrator `Error <n>: <message>.Line: <line>-> <source> (5001)` | Measured, no modal raised         |
|  [07]   | `DeadlineExceeded`   | Join of the detached fiber passes `timeoutMs`                     | This design                       |
|  [08]   | `HostNotAttached`    | Job arrives while the link reads `listening`                      | Link state machine                |
|  [09]   | `HostBusy`           | Second job arrives while the in-flight `Option` is `Some`         | In-flight `Ref`                   |
|  [10]   | `TransportClosed`    | Socket closes while a job is in flight                            | Measured on both UXP hosts        |
|  [11]   | `ResultNotDecodable` | Decode failure over stdout, a response file, or a job value       | Sidekick truncates a stack at 500 |
|  [12]   | `HostThrew`          | Host ran the job and reported a failure in band                   | Every in-band shape               |

No variant exists that no consumer tells apart. A Photoshop modal refusal arrives as `HostThrew`; InDesign's `modalState` is a field of the `state` rpc and of `health`, a read rather than a refusal; a cancellation interrupts the handler fiber and answers nothing.

`HostRejection` is data a host produces, so it is one `Schema.Union` (`Schema.d.ts:3921`) of `Schema.TaggedStruct` members (`Schema.d.ts:4869`) with `type HostRejection = typeof HostRejection.Type`. The same schema decodes the failure side of a plugin's `settle` exit and an Illustrator response file. An empty field record is `Record<never, never>`, because Biome refuses `{}`.

| [INDEX] | [MEMBER]            | [FIELDS]                                           | [HOST]                         |
| :-----: | :------------------ | :------------------------------------------------- | :----------------------------- |
|  [01]   | `ScriptThrew`       | `name`, `message`, `line?`, `fileName?`, `number?` | Illustrator, InDesign, Acrobat |
|  [02]   | `DescriptorFailed`  | `index`, `result`, `message`                       | Photoshop                      |
|  [03]   | `UserCancelled`     | None                                               | Photoshop                      |
|  [04]   | `PreferenceLocked`  | `section`, `key`                                   | Photoshop                      |
|  [05]   | `ModalDenied`       | `holder: Schema.OptionFromNullOr(Schema.String)`   | Photoshop                      |
|  [06]   | `NotAllowed`        | `method`                                           | Acrobat                        |
|  [07]   | `MenuItemNotListed` | `name`                                             | Acrobat                        |
|  [08]   | `NoActiveDocument`  | None                                               | Acrobat, Photoshop, InDesign   |
|  [09]   | `UnknownMethod`     | `method`                                           | Photoshop, InDesign            |
|  [10]   | `MalformedParams`   | `reason`                                           | Photoshop, InDesign            |

| [INDEX] | [MEMBER]            | [TRIGGER]                                                                           |
| :-----: | :------------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ScriptThrew`       | Response file or `settle` failure carries `{name, message, stack}`, Acrobat adds `extMessage` |
|  [02]   | `DescriptorFailed`  | Fulfilled `batchPlay` array holds `{_obj: "error", message, result}` at `index`     |
|  [03]   | `UserCancelled`     | `result === -128`, or the value thrown after Escape ends a modal scope              |
|  [04]   | `PreferenceLocked`  | `notifications` setter throws while `quietMode` is true                             |
|  [05]   | `ModalDenied`       | `executeAsModal` rejects with `e.number === 9`, the holder parsed from the message  |
|  [06]   | `NotAllowed`        | `NotAllowedError`, or `GeneralError: Operation failed.` from `app.getPath`          |
|  [07]   | `MenuItemNotListed` | Name is absent from `app.listMenuItems()`, which answered 6 top-level menus         |
|  [08]   | `NoActiveDocument`  | `app.activeDocs.length === 0`, `app.documents.length === 0`, or no `activeDocument` |
|  [09]   | `UnknownMethod`     | Plugin executor received a `kind` outside its closed set                            |
|  [10]   | `MalformedParams`   | Plugin's job body decode failed                                                     |

`values.ts` holds the domain values, each one `Schema`, built once at the MCP boundary and never revalidated. Every constructor is `effect/Schema`.

| [INDEX] | [VALUE]            | [SCHEMA]                                                                             | [DECLARATION]             |
| :-----: | :----------------- | :----------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | `HostId`           | `Schema.Literals(['illustrator','photoshop','indesign','acrobat'])`                  | `Schema.d.ts:3960`        |
|  [02]   | `JobId`            | `Schema.String` checked `isUUID(4)`, branded `JobId`                                 | `:5366`, `:4081`, `:4151` |
|  [03]   | `TimeoutMs`        | `Schema.Int` checked `isBetween({minimum: 1, maximum: 300000})`, branded `TimeoutMs` | `:5812`, `:5754`          |
|  [04]   | `PageIndex`        | `Schema.Int` checked `isGreaterThanOrEqualTo(0)`                                     | `:5694`                   |
|  [05]   | `ArtboardIndex`    | `Schema.Int` checked `isBetween({minimum: 0, maximum: 99})`                          | `:5754`                   |
|  [06]   | `Dpi`              | `Schema.Int` checked `isBetween({minimum: 48, maximum: 600})`, an output value       | `:5754`                   |
|  [07]   | `NormalizedRegion` | `Schema.Struct` of `left`, `top`, `width`, `height`, each `Number` within 0 and 1    | `:2843`, `:5754`          |
|  [08]   | `PixelBudget`      | `Schema.Literals(['preview','detail'])`                                              | `:3960`                   |
|  [09]   | `AbsolutePath`     | `Schema.String` checked `isStartsWith('/')`, branded                                 | `:5462`, `:4151`          |
|  [10]   | `Undo`             | `Schema.Literals(['single','none'])`, the field every mutating tool's output carries  | `:3960`                   |

`NormalizedRegion` carries a struct-level check built with `Schema.makeFilter` (`Schema.d.ts:5191`) asserting `left + width ≤ 1` and `top + height ≤ 1`; a filter returning a string reports that string as the message. `Schema.positive` and its siblings no longer exist, so a lower bound reads `isGreaterThanOrEqualTo`. Bundle ids and ports come from the host table, so no `BundleId` and no `Port` schema exists. `AbsolutePath` matches `POSIX file`, which takes a POSIX path.

## [05]-[MCP_BOUNDARY]

Tools are declared over Effect `Schema` and served by `effect/unstable/ai`, so no `@modelcontextprotocol/sdk` import exists in these projects.

| [INDEX] | [STEP]                                                                                  | [DECLARATION]                               |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `Tool.make(name, {description, parameters, success, failure})`, `parameters` a `Schema` | `unstable/ai/Tool.d.ts:812`                 |
|  [02]   | `Toolkit.make(...tools)` folds a host's rows, `toolkit.toLayer` attaches handlers       | `unstable/ai/Toolkit.d.ts:230`, `:64`       |
|  [03]   | `McpServer.toolkit(kit)` registers the toolkit as a `Layer`                             | `unstable/ai/McpServer.d.ts:300`            |
|  [04]   | `McpServer.layerStdio({name, version, protocols})` requires the `Stdio` service         | `unstable/ai/McpServer.d.ts:239`            |
|  [05]   | `protocols` is non-empty, `[McpProtocol.v2025_11_25]`                                   | `unstable/ai/McpProtocol.d.ts:94`           |
|  [06]   | `Stdio` holds `stdin` as a `Stream` and `stdout()` as a `Sink`                          | `Stdio.d.ts:81`, `:75`                      |
|  [07]   | `NodeStdio.layer` supplies `Stdio`, and `NodeServices.layer` includes it                | `NodeStdio.d.ts:10`, `NodeServices.d.ts:25` |
|  [08]   | `NodeRuntime.runMain` starts the program                                                | `NodeRuntime.d.ts:27`                       |

`name` and `version` come from the server's `package.json`, the one place that states them, through a typed JSON import in `main.ts`. Each tool file builds its own `Toolkit.make` and exports one `Layer` from `McpServer.toolkit` over `toolkit.toLayer`, so the tool, its schemas, and its handler stay local and inferred; `main.ts` merges those layers under `McpServer.layerStdio` through `Layer.provideMerge` (`Layer.d.ts:2116`) and provides the `Hosts`, `Links`, and `Jobs` layers to the merge through `Layer.provide` (`Layer.d.ts:1704`), each built once with `Layer.effect` (`Layer.d.ts:1209`): `Hosts` from `resolve`, `Links` from one `Ref<Link>` at `listening` per socket host, `Jobs` from one `Ref<Activity>` per host. The layer derives each tool's published JSON Schema from its Effect `Schema` and decodes every argument before the handler runs, so a handler receives constructed branded values and validates nothing again. Strict tools reject excess properties, and the layer supplies `McpSchema.McpRequestContext` to each handler.

The stdio protocol ends when the client closes the server's input: `RpcServer.js:881` interrupts the fiber that built the protocol, which is the main fiber because the stdio layer is the member `Layer.provideMerge` builds last. An interrupts-only exit is the client's shutdown, so `NodeRuntime.runMain` takes a `teardown` (`Runtime.d.ts:46`) that answers exit 0 to it and defers every other exit to `Runtime.defaultTeardown` (`Runtime.d.ts:93`, `Cause.hasInterruptsOnly` at `Cause.d.ts:596`).

Output is one `Schema.Struct` per tool at the top level. A tool with more than one result shape carries them as one closed `Schema.Union` read by `kind` under one field of that struct, the `{kind: 'error', error: BridgeError}` member included, never as the top-level schema: `McpServer.js:1121` publishes a tool's `outputSchema` only when `Tool.getJsonSchemaFromSchema(tool.successSchema).type === "object"` (`Tool.d.ts:1191`), and a `Schema.Union` of `kind` structs derives `{anyOf: [...]}` with no top-level `type`, so no output contract ships, while a `Schema.Struct` derives `type: "object"` and a closed union nested under one field of a struct derives `type: "object"` with zero optionals (measured 2026-09-15 through that function; the running server's `tools/list` shows `health` with `outputSchema.type "object"` and keys `server`, `hosts`). The layer renders a success as structured content beside its JSON text, and a declared failure as `isError: true` with the error's `message` as plain text and no fields (`McpServer.js:1146`–`1160` at rc.115), which is why a `BridgeError` travels in the success value and never in the failure channel. `health` carries no error member: each of its failures is a row value.

`jobs.ts` measures a success value's JSON text before the toolkit hands it over. Above 200 000 characters it writes `.artifacts/creative-cloud/<host>/results/<jobId>.json` through `effect/FileSystem` (`FileSystem.d.ts:363`) and answers `{kind: 'file', path, bytes, sha256}`. The threshold is one constant of that file. `KeyValueStore.layerFileSystem(directory)` (`unstable/persistence/KeyValueStore.d.ts:246`) was judged against `makeDirectory` and `writeFile` on 2026-09-15 and refused: `set(key, bytes)` names its file by percent-encoding the key inside a directory the layer owns, so the `path` the result carries would restate the store's private naming, one directory per host would need one layer per host or a flattened key, and `bytes` and `sha256` are computed beside either form, so the store saves no line and adds a layer.

Cancellation interrupts the handler fiber and answers nothing. The host job runs to completion and the in-flight `Option` stays `Some` until it reports: AppleScript's `with timeout` stops waiting rather than cancelling, a synchronous InDesign DOM call cannot be interrupted, and a Photoshop modal scope ends on a user cancel alone.

## [06]-[JOBS]

| [INDEX] | [ASPECT]         | [DECISION]                                                                                       |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | One job per host | `Ref<Option<InFlight{jobId, startedAt}>>` per host, created in `main` and passed to handlers     |
|  [02]   | Second call      | Answers `HostBusy{host, jobId, startedAt}`, no queue and no plugin-side refusal                  |
|  [03]   | Deadline         | `Effect.forkDetach` of the run, `Fiber.join` under `Effect.timeoutOrElse`                        |
|  [04]   | After a deadline | In-flight `Option` stays `Some` until the host reports, a late reply clears it and is dropped    |
|  [05]   | Default          | `timeoutMs` defaults to 30 000, the per-request figure Sidekick proves                           |
|  [06]   | Done per host    | Illustrator and Acrobat: exit `0` with the reply decoded; Photoshop and InDesign: a `settle` rpc whose exit is `Success` |

`Effect.forkDetach` (`Effect.d.ts:16394`) replaces the daemon fork of Effect 3 and `Effect.timeoutOrElse` (`Effect.d.ts:7984`) replaces `timeoutFail`; its `orElse` returns `Effect.fail(new DeadlineExceeded({host, jobId}))`. `Fiber.join` (`Fiber.d.ts:282`) is interrupted by the deadline while the detached fiber keeps waiting on the child, so the process is released when osascript exits. The `job` frame carries no deadline and the plugin compares no clock.

Serialization is the server's, carried from Sidekick, whose bridge holds one request in flight and recurses after each settle while the plugin executes whatever arrives. Here the in-flight `Ref` alone decides for every host and every caller, the `health` probe included, which claims it through `probe` of `jobs.ts`, whose settle transition is `identity`, so the probe leaves `lastSuccessAt` and `lastError` to real jobs as Sidekick's `{record: false}` does; the server sends no second job while the `Option` is `Some`, and the plugin holds no busy branch. A deadline is recorded as `lastError {reason: 'deadlineExceeded'}` when it fires, the host's own report is recorded when it arrives, and a success never clears `lastError`, so `health` reads what has been failing beside when a job last worked. A measured consequence stands behind row 04: InDesign finished a 2.5 second script issued under a one second timeout and answered the next call, and Sidekick's own late reply logged `No pending request for id` after its pending entry was deleted.

Undo is a per-host rule.

| [INDEX] | [HOST]      | [UNDO]                                                                                           |
| :-----: | :---------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | Photoshop   | `core.executeAsModal(fn, {commandName, timeOut})` around `suspendHistory` and `resumeHistory`    |
|  [02]   | Photoshop   | Exception before `resumeHistory` rolls back at scope end, so one job is one history state        |
|  [03]   | InDesign    | `doScript(fn, ScriptLanguage.UXPSCRIPT, [], UndoModes.ENTIRE_SCRIPT, name)` named after the tool |
|  [04]   | InDesign    | One undo step covers a synchronous `fn` alone, an async body ignores the undo mode               |
|  [05]   | InDesign    | Tool that exports or reads a file splits into a synchronous mutation job and an asynchronous job |
|  [06]   | InDesign    | Every mutation is get-or-create by name plus a `rasm_` label, so a rerun converges               |
|  [07]   | Illustrator | ExtendScript groups nothing, a script is N undo steps, and `app.undo()` is never called          |
|  [08]   | Acrobat     | No undo API, a mutating job reports `dirty: true` and saves through the trusted `saveAs`         |

Output of a mutating tool carries `undo: 'single' | 'none'`. Photoshop reads such as `snapshot`, `get_document`, `get_preferences`, and `list_presets` carry `None` for `suspendHistory` and `commandName` and run outside a modal scope.

Preferences are saved and restored per job, the practice Sidekick proves. The plugin executor pairs acquisition and release around every job, so a release runs on the error path.

| [INDEX] | [HOST]   | [SAVED_AND_RESTORED]                                                                       |
| :-----: | :------- | :----------------------------------------------------------------------------------------- |
|  [01]   | InDesign | `app.scriptPreferences.measurementUnit` set to `MeasurementUnits.POINTS`                   |
|  [02]   | InDesign | `app.scriptPreferences.userInteractionLevel` set to `UserInteractionLevels.NEVER_INTERACT` |
|  [03]   | InDesign | `app.jpegExportPreferences` fields an export touches                                       |

Saved export fields are `jpegQuality`, `exportResolution`, `pageString`, `jpegExportRange`, and `exportingSpread`. Document units corrupt geometry reads and a modal alert during a capture blocks the bridge, which is why rows 01 and 02 wrap every job rather than every capture.

Images carry one budget. `images.ts` computes `dpi = clamp(round(min(1568 / longEdgeIn, sqrt(1150000 / areaIn))), 48, 600)` for `detail`; `preview` is 768 px on the long edge within 300 000 px and is ignored for a region capture and an object capture. Every image result carries `{widthPx, heightPx, dpi}`, the `Raster` schema of `images.ts`, and the host renders at that size, so no downscaler exists. A preview of a sheet longer than 16 in on its long edge computes below 48 dpi, so the `Dpi` bounds of `values.ts` hold for `detail` alone.

| [INDEX] | [HOST]      | [IMAGE_RESULT]                                                                                             |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | Photoshop   | Base64 JPEG over the socket from `encodeImageData({base64: true})`, the plugin has no disk access          |
|  [02]   | InDesign    | Plugin exports to `.artifacts/creative-cloud/indesign/<jobId>/<name>.<ext>` and the `settle` value carries the path |
|  [03]   | Illustrator | `imageCapture(File)` writes into the job directory                                                         |
|  [04]   | Acrobat     | None                                                                                                       |

## [07]-[OSASCRIPT]

Illustrator and Acrobat run through one child process. `osascript -` reads the script from stdin and passes later arguments to `on run argv`, proven with `printf 'on run argv…' | osascript - hello` answering `got hello`.

| [INDEX] | [STEP]                                                                                           | [DECLARATION]                        |
| :-----: | :----------------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `ChildProcess.make('osascript', ['-'], {stdin})` with the script as a `Stream`                   | `ChildProcess.d.ts:456`, `:343`      |
|  [02]   | `CommandInput` accepts a `Stream<Uint8Array, PlatformError>`, so `Command.feed` has no successor | `ChildProcess.d.ts:112`              |
|  [03]   | `spawner.spawn(command)` yields a scoped handle                                                  | `ChildProcessSpawner.d.ts:209`       |
|  [04]   | Handle holds `exitCode`, `stdout`, `stderr`, `kill`, `pid`                                       | `ChildProcessSpawner.d.ts:71`–`:120` |
|  [05]   | `ChildProcessSpawner` is the service, `NodeChildProcessSpawner.layer` the implementation         | `ChildProcessSpawner.d.ts:247`       |
|  [06]   | `NodeServices.layer` supplies the spawner beside `FileSystem`, `Path`, and `Stdio`               | `NodeServices.d.ts:25`               |

`osascript.ts` exports `reply(host, command)`, the spawner boundary every child process of the server runs through, and `read(host, bundleId, timeoutMs, statement, file)`, which assembles the one template as one child process: its first line raises `-600` when `running of application id` answers `false`, before the `tell` that would launch the host, and the reply classifies as `hostNotRunning` (measured 2026-09-15: `execution error: Application isn't running. (-600)` at exit 1, and `30.9.0` from the running Illustrator). `file` is the `Option` of a path the `set f` line binds, `Option.none()` for every statement that names no file. The timeout ceiling is `Math.ceil(Duration.toSeconds(Duration.millis(timeoutMs)))`. Bindings sit outside the `tell` block, because a `POSIX file` specifier inside it compiles as an object of the target and answers `-1728`:

```text
if not running of application id "<bundleId>" then error number -600
set f to POSIX file "<path>"
with timeout of <seconds> seconds
tell application id "<bundleId>"
<statement>
end tell
end timeout
```

Interpolated values are AppleScript string literals alone, escaped `\` to `\\` and `"` to `\"`. No shell is involved, so `quoted form of` never appears, and `with arguments {…}` is a list literal of such strings that reaches the script as `arguments[i]`. Statements per host: Illustrator runs `do javascript f with arguments {"<request>", "<response>"} show debugger never`, Acrobat runs `do script "<js>"` with the JavaScript an immediately invoked function returning `JSON.stringify`.

Illustrator takes its arguments through a request file and a response file under `.artifacts/creative-cloud/illustrator/jobs/<jobId>/`, and the script returns `String(File.length)` of the response so the reader asserts the byte count. Measured 2026-09-11 on 30.9.0: `POSIX file`, `file "<hfs>"`, and `alias` all ran in 0.14 to 0.17 seconds with the same result, `with arguments` landed on the script-level `arguments` array as strings, a UTF-8 request round-tripped, and the returned `646` equalled the response byte count. A function scope shadows `arguments`, so an entry captures it at top level before any function. The ExtendScript engine has no `JSON`, so a script parses its request with `eval("(" + text + ")")`.

Classification is total over `{exitCode, stdout, stderr}`. A non-zero exit is a value, never an exception. One `Schema.TemplateLiteralParser` (`Schema.d.ts:2316`) reads the phase before the code, because osascript prints `<start>:<end>: <syntax|execution> error: <message> (<code>)` on stderr and exits 1, the first pair being the character range of the failing statement (`61:78` on a one-line script, measured 2026-09-15), which `scriptNotCompiled.line` keeps as one string; `Array.findFirst` over `String.linesIterator(stderr)` takes the first line the parser decodes, since the parser's `Schema.String` parts read across line breaks and whole-stderr decoding would swallow a preceding line into `line`:

```text
Schema.TemplateLiteralParser([Schema.String, ': ', Schema.Literals(['syntax', 'execution']), ' error: ', Schema.String, ' (', Schema.NumberFromString, ')'])
```

| [INDEX] | [CODE] | [NAME]                              | [MEANING]                                | [EVIDENCE]                         |
| :-----: | -----: | :---------------------------------- | :--------------------------------------- | :--------------------------------- |
|  [01]   |  -1712 | `errAETimeout`                      | Apple event timed out                    | Measured on InDesign               |
|  [02]   |  -1708 | `errAEEventNotHandled`              | Event was not handled                    | Measured on Acrobat                |
|  [03]   |  -1743 | `errAEEventNotPermitted`            | Target refuses this sender               | Documented, unreproduced           |
|  [04]   |  -1744 | `errAEEventWouldRequireUserConsent` | Consent undetermined and no prompt       | Documented, unreproduced           |
|  [05]   |   -600 | `procNotFound`                      | No eligible process                      | Documented, unreproduced           |
|  [06]   |  -1728 | `errAENoSuchObject`                 | Referenced object does not exist         | Measured three ways                |
|  [07]   |  -2740 | Syntax error                        | Identifier cannot follow this identifier | Measured twice as a compile error  |
|  [08]   |   8800 | Photoshop internal                  | General Photoshop error                  | Measured on an `as alias` argument |

`syntax error` line is a defect in the template and answers `ScriptNotCompiled`, never a host state. Acrobat's `app.getPath("user","javascript")` raises `GeneralError: Operation failed.` from the `do script` context rather than `NotAllowedError`, so the classifier matches name and message for that method. An ExtendScript syntax error inside an Illustrator file run returns on stderr as `execution error: Adobe Illustrator got an error: Error 9: …Line: 50-> …` with no modal dialog and no `-1712`.

Automation rows belong to the terminal at the root of the launch chain, and each call was answered from a stored grant with no prompt. `tccutil reset AppleEvents <bundleId>` makes a host prompt again.

## [08]-[SOCKET]

Photoshop and InDesign attach over a WebSocket the server listens on. UXP cannot listen, so the server keeps the listener and is the `RpcServer`, and each plugin is a pure `RpcClient`; jobs flow server → plugin through the one streaming rpc the plugin subscribes to.

| [INDEX] | [STEP]                                                                        | [DECLARATION]                                |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `NodeSocketServer.layerWebSocket({host, port})` provides `SocketServer`, one per socket host in `main.ts` | `NodeSocketServer.d.ts:78`                   |
|  [02]   | `RpcServer.layerProtocolSocketServer` turns that `SocketServer` into the rpc `Protocol`: one client id per accepted socket, `disconnects` offered when a socket's scope closes, `Ping` answered with `Pong` | `unstable/rpc/RpcServer.d.ts:200`, `RpcServer.js:969`–`:985`, `:526`–`:528` |
|  [03]   | `Layer.fresh` around the protocol constant, because a layer is memoized by identity within one build and two listeners need two protocols | `Layer.d.ts:3411`, `Layer.js:130`, `:914`, `:1475` |
|  [04]   | `RpcSerialization.layerJson`, the WebSocket frames each message              | `unstable/rpc/RpcSerialization.d.ts:147`     |
|  [05]   | `RpcServer.layer(group)` runs the handlers `group.toLayer` provides, forked in the layer scope | `unstable/rpc/RpcServer.d.ts:71`, `RpcGroup.d.ts:66`, `RpcServer.js:562` |
|  [06]   | `Rpc.make(tag, {payload, success, error, stream})` and `RpcGroup.make` declare the contract both sides read | `unstable/rpc/Rpc.d.ts:465`, `RpcGroup.d.ts:160` |
|  [07]   | A streaming handler returns an `Effect` of a `Queue.Dequeue`; the server drains it to the client as `Chunk` messages, interrupts the request on the client's disconnect, and closes the request scope, so the handler's release runs | `unstable/rpc/Rpc.d.ts:443`, `RpcServer.js:279`–`:294`, `:75`–`:83`, `:182`–`:212`, `:404`–`:411` |
|  [08]   | Client: `RpcClient.make(group)` over `RpcClient.layerProtocolSocket({retryTransientErrors: true})`, `Socket.layerWebSocket(url)`, `Socket.layerWebSocketConstructorGlobal` | `unstable/rpc/RpcClient.d.ts:116`, `:237`, `unstable/socket/Socket.d.ts:596`, `:542` |
|  [09]   | Liveness is the client protocol's: a `Ping` every 5 s, the connection failed as `SocketOpenError{kind: 'Timeout'}` when no `Pong` answered the previous one, reconnected under `retryTransientErrors` on `Schedule.min(exponential(500, 1.5), spaced(5000))`; `ConnectionHooks` reports connect and disconnect | `RpcClient.js:601`, `:667`–`:672`, `:682`, `:712`, `:727`, `RpcClient.d.ts:299`, `RpcMessage.d.ts:147`, `:353` |

Bind host is the IPv4 literal, because `localhost` binds `::1` alone and refuses IPv4 loopback, while the plugin dials `ws://localhost:<port>`, the domain its manifest declares. Messages are the rpc protocol's JSON envelopes (`RpcMessage.d.ts:60`, `:206`, `:254`) with no compression and no payload-size option; the only size limit is the MCP result spill. No heartbeat, handshake, or reconnect figure exists in the tree: the client protocol owns them.

| [INDEX] | [SIDE] | [STATE]                         | [TRANSITION]                                                                   |
| :-----: | :----- | :------------------------------ | :----------------------------------------------------------------------------- |
|  [01]   | Server | `listening`                     | `attach{identity}` claims the link in one `Ref.modify` and gives `attached{identity, state: None, jobs: Queue<Job>, pending: None}`, the queue returned as the stream the plugin reads |
|  [02]   | Server | `attached`                      | A second `attach` fails its stream with `alreadyAttached` and the link stays; `state` sets `state`; `dispatch` sets `pending{jobId, settled: Deferred}` in the `Ref.modify` that reads the link and offers the job to `jobs`; `settle{jobId, exit}` completes the pending `Deferred` whose `jobId` matches, `Success` with the value and `Failure` as `hostThrew{rejection}`, and is otherwise a no-op, as Sidekick drops `No pending request for id` |
|  [03]   | Server | `attached`                      | The plugin's disconnect ends the `attach` request; its release fails a pending `Deferred` with `transportClosed{code: 1000}` and resets `listening` |
|  [04]   | Plugin | —                               | `RpcClient` over the socket protocol: dial, retry, ping, reconnect, and the `attach` stream restart are the library's; the plugin holds no state machine |

The server's link value carries the job queue and the pending job in the `attached` member, so one `Ref` per endpoint holds state and transport handles together and `linkState` strips them for `health` (`Struct.omit`, `Struct.d.ts:324`). The link holds no `busy` member: the in-flight `Ref` of [06] is the one owner of the running job and of `hostBusy{startedAt}`, so one job is pending per link at most and a completed `Deferred` answers a late `settle` with `false` (`Deferred.d.ts:446`). `dispatch` reads the link, sets the pending job, offers the job, and awaits the `Deferred` (`Deferred.d.ts:147`); a `listening` link answers `hostNotAttached`. The close code is the normal-closure `1000` the server states; the peer's own close code is not read. `frames.ts` exports `Link` as both the schema and its type, the Effect convention. The `attach` handler is `Effect.acquireRelease` (`Effect.d.ts:12118`) around the claim, returning `Queue.make<Job>()` (`Queue.d.ts:399`) as the streaming result (`Rpc.d.ts:443`), so the server's own request scope pairs the claim with its release.

| [INDEX] | [RPC]    | [DIRECTION]              | [CONTRACT]                                                          |
| :-----: | :------- | :----------------------- | :------------------------------------------------------------------ |
|  [01]   | `attach` | Plugin → server, stream  | Payload `{plugin, version, host: {name, version}, uxp, app?, dom?}` (`Schema.OptionFromOptionalKey`, `Schema.d.ts:9072`); each element a `Job {jobId, kind, body, suspendHistory: {documentId, name}?, commandName?}`; stream error `alreadyAttached` |
|  [02]   | `settle` | Plugin → server          | `{jobId, exit: Schema.Result({value, autocorrections?}, HostRejection)}` (`Schema.d.ts:9213`), on the wire `{_tag: 'Success', success}` or `{_tag: 'Failure', failure}` (`ResultIso`, `:9200`), closed where `Schema.Exit` would carry `Die` and `Interrupt` members |
|  [03]   | `state`  | Plugin → server          | `{modalState, activeDocumentId, modified}`                          |

`host.version` comes from `require('uxp').host` and `uxp` from `require('uxp').versions.uxp`, because the Photoshop UXP DOM exposes no `app.version`; InDesign adds `app.version` and `app.scriptPreferences.version`. `value` holds the body's JSON-serialisable return or an export path under `.artifacts/`, and a DOM object is never returned. Sidekick's reply codes map onto the rejection members: a missing parameter is `MalformedParams`, an unknown method is `UnknownMethod`, and a script error is `ScriptThrew`. Readback of 2026-09-15 with a fake `RpcClient` plugin on the InDesign port: `attach` accepted, the `health` probe's `execute` of `'1'` settled `{_tag: 'Success', success: 1}` with `link {_tag: 'attached', identity: {plugin: 'first', …}, state: null}`, a second client's stream failed `{_tag: 'alreadyAttached'}` and its socket closed, and killing the first returned the link to `listening` with the probe answering `hostNotAttached`; the process exited 0 once the stdio input closed.

Each plugin holds the socket client, the tick, the typed job handlers keyed by kind, and `execute` as the one text escape hatch with autocorrect in front. Use `plan/design/indesign.md` and `plan/design/photoshop.md` for those handlers, the manifests, the deploy proofs, and the DOM rules.

## [09]-[HEALTH]

`health` takes an optional `HostId`, decoded once through `Schema.OptionFromOptionalKey` (`Schema.d.ts:9058`), and answers one row per host: the resolved table row, the link state, a live probe, and the in-flight job. It is one boundary Effect per host and the one tool that answers while a job is in flight. `health.ts` reads `probe` and `link` through one `Match.discriminatorsExhaustive('channel')` table (`Match.d.ts:924`) of two arms, `osascript.probe` with `Effect.succeedNone` and `socket.probe` with `socket.linkState`, the row's `id` keying `jobs` and `endpoints`; the selected hosts are the record's values, or the one row read by key.

| [INDEX] | [CHANNEL]   | [PROBE]                                                                        |
| :-----: | :---------- | :----------------------------------------------------------------------------- |
|  [01]   | `socket`    | One unrecorded `execute` job whose body is `1`, under a 5 000 ms race          |
|  [02]   | `osascript` | `if not running of application id "<bundleId>" then error number -600`, a line that does not launch the host |
|  [03]   | `osascript` | Then, in the same script, `tell application id "<bundleId>" to get version`, proven on three hosts |

The `running of` line raises `-600` before any event is sent, so a host that is not running answers `HostNotRunning` and is not launched, and `-1743` answers `AutomationDenied`. Probe shape is Sidekick's, whose `get_health` runs a real `execute {code:"1"}` end to end under a 5 000 ms race. Every probe runs through `probe` of `jobs.ts`, which claims the in-flight `Ref` for `PROBE_MS` with an `identity` settle transition, so it answers `hostBusy` from the in-flight `Ref` without dispatching, a socket probe answers `hostNotAttached` while the link reads `listening`, and `deadlineExceeded` when the 5 000 ms race passes; a success carries the host's answer, the `get version` text or the job's `done.value`. Readback of 2026-09-15 after the rebuild, with every host running and no plugin deployed: Illustrator and Acrobat `probe {_tag: 'Success', success: '30.9.0'}` and `{_tag: 'Success', success: '26.002.21901'}`, Photoshop and InDesign `probe {_tag: 'Failure', failure: {_tag: 'hostNotAttached'}}` with `link {_tag: 'listening'}`, InDesign `featureSet 'righttoleft'`, Acrobat `viewerVersion 26.00221901`, `server.uptimeSeconds 0.549`, and the `initialize` result on stdio names `@rasm/creative-cloud-server` `0.1.0`, the manifest's `name` and `version`; the process exits 0 once the client closes its input. The depth pass readback of the same day, Acrobat quit, answers Acrobat `probe {_tag: 'Failure', failure: {_tag: 'hostNotRunning'}}` with `viewerVersion null` from the one-script read and the same rows otherwise. A request still in flight when the input closes is interrupted with the main fiber and answers a JSON-RPC `InternalError`, so a readback holds the input open until its reply arrives, as an MCP client does. The third depth pass's readback of the same day, with a fake `RpcClient` plugin attached on the InDesign port, answers InDesign `link {_tag: 'attached', identity: {plugin: 'first', version: '0.0.0', host: {name: 'ID', version: '21.6.0.58'}, uxp: '8.1.0', app: '21.6.0.58'}, state: null}` and `probe {_tag: 'Success', success: 1}`, Photoshop `listening` with `hostNotAttached`, Illustrator `'30.9.0'`, Acrobat `hostNotRunning`, and after the plugin is killed InDesign `listening` with `hostNotAttached`; every row's `lastError` stays `null`, since a probe records nothing.

| [INDEX] | [FIELD]         | [VALUE]                                                                         |
| :-----: | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `server`        | `{name, version, uptimeSeconds}` with name and version from `package.json`; no `pid`, since Biome refuses both the `process` global and `node:process` here |
|  [02]   | `host`          | Resolved row with ids, paths, version, channel, and port on a socket host       |
|  [03]   | `link`          | `{_tag: 'listening'}` or `{_tag: 'attached', identity, state}` of the link, `null` on an osascript host |
|  [04]   | `probe`         | `Result<Json, BridgeError>` from the live call, on the wire `{_tag: 'Success', success}` or `{_tag: 'Failure', failure}` through `Schema.toCodecJson` |
|  [05]   | `inFlight`      | `Option<{jobId, startedAt}>` read from the job `Ref`                            |
|  [06]   | `lastSuccessAt` | Timestamp of the last recorded success, held in the same `Ref`                  |
|  [07]   | `lastError`     | `{error: BridgeError, count, secondsAgo}` of the last recorded failure, held in the same `Ref`, `count` climbing while `error._tag` repeats, never cleared by a success, unchanged by a defect |

## [10]-[EXTENSION]

Each change names the files it touches and adds no pattern the tree lacks.

| [INDEX] | [CHANGE]               | [FILES]                                                                                         |
| :-----: | :--------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | Add a host             | `hosts.ts`: one table row with its channel tag, its `Resolved` member, and its `resolve` entry; the literal in `values.ts` `HostId` |
|  [02]   | Add a host             | `errors.ts`: the `HostRejection` members that host's in-band failures need                      |
|  [03]   | Add a host             | `<host>/tools.ts`: the tool table, its handlers, and the host's map into `HostRejection`        |
|  [04]   | Add a host             | `main.ts`: one more tool layer in the merge and, for a socket host, one more `_listener` and one more `_endpoint` row of the `Links` layer; `health.ts` changes only for a host on a third channel, which adds one arm to its `Match.discriminatorsExhaustive('channel')` table |
|  [05]   | Add a host             | `apps/creative-cloud/README.md`: the host row; no `.mcp.json` row and no allow row change       |
|  [06]   | Add a tool             | `<host>/tools.ts`: `name`, `description`, `parameters`, `success` by `kind`, `handler`, in that file's `Toolkit.make` and `toLayer` |
|  [07]   | Add a tool             | Job body beside it, a text module for a socket host or an entry under the scripts project       |
|  [08]   | Add a tool             | Host design file gains a proof row naming the calls and the document rows behind them           |
|  [09]   | Add a preference class | Photoshop: one entry in the class table and one member of its section `Schema.Literals`         |
|  [10]   | Add a preference class | Illustrator: `{key, kind}` rows in `illustrator/preferences.json`                               |
|  [11]   | Add a preference class | Acrobat: `{path, typecode, value}` rows in `acrobat/preferences.json`                           |
|  [12]   | Add a preference class | InDesign: keys in the `set_text_defaults` input `Schema`                                        |
|  [13]   | Add a job kind         | `frames.ts`: one member of the `kind` literals, which both plugins import                       |
|  [14]   | Add a job kind         | Plugin executor gains one branch of its `Match.exhaustive`, which the union typechecks          |
|  [15]   | Add an error variant   | `errors.ts`: one member of the `Data.TaggedEnum`, one `Schema.TaggedStruct` member of `BridgeError`, one classifier branch |
|  [16]   | Add an rpc             | `frames.ts`: one `Rpc.make` row in `Frames` with its member of the group annotation; `socket.ts`: one handler in `Frames.of`, which the group typechecks |

## [11]-[UNVERIFIED]

Each row names the unit whose research step reads it. Nothing is built on an unread fact.

| [INDEX] | [FACT]                                                                                 | [UNIT]              |
| :-----: | :------------------------------------------------------------------------------------- | :------------------ |
|  [01]   | Bundle-path read per host and its answer for each bundle id: read in [03] on 2026-09-15 | Server core         |
|  [02]   | UXP plugin residency and load with the panel closed                                    | InDesign plugin     |
|  [03]   | `ws://localhost:<port>` from UXP reaching the IPv4 listener                            | InDesign plugin     |
|  [04]   | Effect-carrying Vite bundle under UXP V8, and the UXP version read at run time         | InDesign plugin     |
|  [05]   | Whether a relaunch reloads a replaced folder at an unchanged version                   | InDesign plugin     |
|  [06]   | InDesign WebSocket message-size ceiling                                                | InDesign plugin     |
|  [07]   | InDesign enum member casing, camelCase against `UPPER_SNAKE`                           | InDesign plugin     |
|  [08]   | Photoshop `_obj` casing, `result === -128`, `e.number === 9` holder, modal-time report | Photoshop plugin    |
|  [09]   | Acrobat support folder and the load of the trusted file after a relaunch               | Acrobat tools       |
|  [10]   | Which Acrobat methods raise `NotAllowedError` against `GeneralError`                   | Acrobat tools       |
|  [11]   | Illustrator error shapes beyond those measured, over a mutating job                    | Illustrator scripts |
|  [12]   | Photoshop `app.systemInformation` while the link reads `busy`                          | Photoshop plugin    |
