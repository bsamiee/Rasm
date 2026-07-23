# [TS_RUNTIME_API_EFFECT_CLI]

`@effect/cli` is the terminal entry family under the one edge assembly law. Each verb family in `cli/verb.ts` is a `Command<Name, R, E, A>` VALUE — itself an `Effect` yielding its parsed config — exported as data; the APP folds selected families through `Command.withSubcommands` into exactly one root and runs it via `Command.run({name, version})` under the platform `Environment` (`FileSystem | Path | Terminal`). No root `Command` exists lib-side, so the god-CLI cannot be spelled. doctor/replay/inspect ship as the lib ops family, their handlers executing over `proc/exec`. `cli/render.ts` renders output through `@effect/printer`(-ansi), and the package's own `HelpDoc` lowers onto that same rail. This is the terminal peer of the `@effect/rpc` `RpcGroup` contribution family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/cli`
- package: `@effect/cli` (MIT)
- module: dual ESM/CJS; types `dist/dts/index.d.ts`; per-module subpaths `Command`, `Args`, `Options`, `Prompt`, `CliApp`, `CliConfig`, `ConfigFile`, `HelpDoc` (+ `HelpDoc/Span`), `Primitive`, `Usage`, `ValidationError`, `CommandDescriptor`, `CommandDirective`, `BuiltInOptions`, `AutoCorrect`
- peers: `effect`, `@effect/platform`, `@effect/printer`, `@effect/printer-ansi`; `Environment` is satisfied by `@effect/platform-node` `NodeContext.layer` or `@effect/platform-bun`
- asset: `.d.ts` declaration surface under `dist/dts/`
- admission: folder-local `# edge` catalog group; versions centralized in `pnpm-workspace.yaml`
- role: `cli/verb.ts` (Command contribution families + the one-root assembly), `cli/render.ts` (Doc-composed output); the ops family executes over `proc/exec`
- rail: `edge/cli`

## [02]-[COMMAND_OWNER]

A `Command` is an `Effect` that yields its parsed config and requires its own tag; the handler is the effectful action. Constructors build a leaf; combinators nest and inject; `run` is the argv boundary.

[SURFACES]: `make` `prompt` `fromDescriptor` `withHandler` `withDescription` `withSubcommands` `provide` `provideEffect` `transformHandler` `getHelp` `getBashCompletions` `wizard` `run` `provideEffectDiscard` `provideSync` `getUsage` `getNames` `getSubcommands` `getFishCompletions` `getZshCompletions`

Consumer note: each verb family is a `Command` value; the app folds selected families through `withSubcommands([verbA, verbB, …])` into ONE root, then `Command.run({ name, version })` turns it into `argv => Effect<void, E | ValidationError, R | Environment>`. `provide` scopes a Layer to a verb subtree so the ops family's `proc/exec` runtime is injected per-command, never at the whole tree.

## [03]-[OPTIONS_ARGS_ALGEBRA]

`Options<A>` (named flags) and `Args<A>` (positionals) are one combinator algebra over a variance carrier — arity, cardinality, recovery, and decode live in the combinators, never in parallel per-type constructors. Constructors differ only by the leading `name` (Options) vs an optional config (Args).

[SURFACES]: `boolean(string,Options.BooleanOptionsConfig?) -> Options<boolean>` `choice(string,C) -> Options<C[number]>` `all(Arg) -> Options</* composite */>` `withFallbackConfig(Config<B>) -> <A>(self:Options<A>)=>Options<B|A>` `withSchema(Schema<B,I,FileSystem|Path|Terminal>) -> (self:Options<A>)=>Options<B>` `withFallbackPrompt(Prompt<B>) -> <A>(self:Options<A>)=>Options<B|A>`

Consumer note: `withFallbackConfig(Config)` is the bridge that unifies a CLI flag with an `effect/Config` value (env / config provider) in one declaration — the canonical way a verb resolves a flag from `proc/config`. `withSchema(Schema)` decodes a flag straight into a kernel-branded value, so the terminal boundary decodes-once exactly like the wire and route boundaries. One asymmetry remains: `Args.mapEffect` fails with `HelpDoc` while `Options.mapEffect` fails with `ValidationError`; `Args.validate` returns `[leftover, A]` and `Options.processCommandLine` returns `[Option<ValidationError>, leftover, A]`.

## [04]-[PROMPT]

`Prompt<Output>` is both a variance carrier and an `Effect<Output, QuitException, Terminal>`, so a prompt is yieldable directly in `Effect.gen`. A fixed constructor roster covers the interaction atoms; `custom` owns any bespoke render/process loop.

[SURFACES]: `select` `custom` `map` `flatMap` `run`

Consumer note: a prompt plugs into a command two ways — `Command.prompt(name, prompt, handler)` for a prompt-driven leaf, or `Options.withFallbackPrompt(prompt)` to prompt only when a flag is absent (the doctor/replay ops family uses the latter for missing targets).

## [05]-[CONFIG_ERRORS_DOCS]

App description, parser policy, the closed parse-error rail, the help algebra, and the config-file provider.

[SURFACES]: `CliApp` `Environment` `ConstructorArgs` `make` `layer` `defaultConfig` `defaultLayer` `ValidationError` `isHelpRequested` `isValidationError` `isMissingFlag` `isMissingValue`

Consumer note: `isHelpRequested` is matched in the run rail to treat `--help`/`--version` as a clean exit rather than a failure; `CliConfig.layer({ isCaseSensitive })` overrides parser policy at the app root; `ConfigFile.layer(name)` layers a disk `ConfigProvider` so `withFallbackConfig` flags read from a config file. `CommandDescriptor`/`CommandDirective`/`BuiltInOptions`/`AutoCorrect`/`Primitive` are the internal parse machinery the typed surfaces above subsume.

## [06]-[STACKING]

- `@effect/platform` env: `Command.run` yields `Effect<…, R | Environment>` where `Environment = FileSystem | Path | Terminal`; satisfy it with `@effect/platform-node` `NodeContext.layer` (node) or `@effect/platform-bun` — the same serve-row selection `proc/exec` makes for the HTTP family, so one runtime choice covers CLI and server.
- sibling `@effect/printer`(-ansi): `cli/render.ts` composes `Doc` (`text`/`vcat`/`hsep`/`nest`/`annotate`) and renders through `@effect/printer-ansi` `Ansi`/`AnsiDoc`; `HelpDoc.toAnsiDoc` IS a `@effect/printer-ansi` `AnsiDoc`, so parse-error help and app output share one render rail — no second styling path.
- sibling `@effect/rpc`: `RpcGroup` and `Command` are the two contribution families under the one assembly law — the app assembles one `HttpApi`, selects `RpcServer` protocol rows, and folds one `Command` root; the verb and rpc families compose identically, so a capability shipped as an rpc method and a verb reuses one handler `Effect`.
- `effect` `Config` + `proc/config`: `withFallbackConfig`/`ConfigFile.layer` fold CLI flags into the `proc/config` provider chain (env → doppler → file → remote) in one declaration, so a flag and its env var are never two sources.
- kernel `Schema`: `Options.withSchema`/`Args.withSchema`/`fileSchema(schema)` decode a flag/arg/file into a kernel brand at the terminal boundary — decode-once, matching the wire and route boundaries.
- `proc/exec` ops family: doctor/replay/inspect are `Command` values whose handlers run over `proc/exec` runtime rows; `Command.provide` scopes their Layers so the ops subtree carries its process/exec capability without leaking it to app verbs.

## [07]-[RAIL_LAW]

- Owns: the terminal entry family — `Command` contribution values, the shared `Options`/`Args` combinator algebra, `Prompt` interaction, and the argv boundary.
- Accept: verb families folded into one root via `withSubcommands`; `withFallbackConfig`/`withSchema` boundary decode; `HelpDoc` rendered through the `@effect/printer-ansi` rail.
- Reject: a lib-side assembled root `Command` (app-only, named defect); parallel per-type flag methods where one combinator owns the axis; hand-rolled argv parsing; a second CLI entry outside the one `Command.run`.
