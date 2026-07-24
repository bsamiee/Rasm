# [TS_RUNTIME_API_EFFECT_CLI]

`@effect/cli` owns the terminal entry family: each verb is a `Command<Name, R, E, A>` value — an `Effect` yielding its parsed config — exported as data, folded at the app root into one `Command.run` boundary over the platform `Environment`. It is the terminal peer of the `@effect/rpc` contribution family; the ops verbs execute over `proc/exec` and `cli/render.ts` lowers output onto the `@effect/printer` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/cli`
- package: `@effect/cli` (MIT)
- module: dual ESM/CJS; one subpath export per namespace, types under `dist/dts/`
- runtime: node or bun; peers `effect`, `@effect/platform`, `@effect/printer`(-ansi)
- rail: `edge/cli`

## [02]-[COMMAND]

[COMMAND_SCOPE]: a `Command` is an `Effect` yielding its parsed config and requiring its own tag; constructors mint a leaf, combinators nest and inject, `run` is the argv boundary that turns a root into `argv => Effect<void, E | ValidationError, R | Environment>`.

[SURFACES]: `make` `prompt` `fromDescriptor` `withHandler` `withDescription` `withSubcommands` `provide` `provideEffect` `provideEffectDiscard` `provideSync` `transformHandler` `getHelp` `getUsage` `getNames` `getSubcommands` `wizard` `run` `getBashCompletions` `getFishCompletions` `getZshCompletions`

## [03]-[OPTIONS_ARGS]

[OPTIONS_ARGS_SCOPE]: `Options<A>` (named flags) and `Args<A>` (positionals) share one combinator algebra over a variance carrier — arity, cardinality, recovery, and decode ride the combinators, not parallel per-type constructors, and constructors differ only by a leading `name` or optional config; `Args.mapEffect` fails with `HelpDoc` where `Options.mapEffect` fails with `ValidationError`.

[SURFACES]: `boolean(string, BooleanOptionsConfig?) -> Options<boolean>` `choice(string, C) -> Options<C[number]>` `all(Arg) -> Options<composite>` `withFallbackConfig(Config<B>) -> Options<B | A>` `withSchema(Schema<B, I, FileSystem | Path | Terminal>) -> Options<B>` `withFallbackPrompt(Prompt<B>) -> Options<B | A>`

## [04]-[PROMPT]

[PROMPT_SCOPE]: `Prompt<Output>` is both a variance carrier and an `Effect<Output, QuitException, Terminal>`, so it yields directly in `Effect.gen`; `custom` owns any bespoke render/process loop, and a prompt plugs into a command through `Command.prompt` (a prompt-driven leaf) or `Options.withFallbackPrompt` (prompting only when a flag is absent).

[SURFACES]: `select` `custom` `map` `flatMap` `run`

## [05]-[APP_CONFIG_DOCS]

[APP_SCOPE]: app description, parser policy, the closed parse-error rail, the help algebra, and the config-file provider; `isHelpRequested` marks `--help`/`--version` a clean exit, `CliConfig.layer({ isCaseSensitive })` overrides parser policy at the root, `ConfigFile.layer(name)` layers a disk `ConfigProvider` for `withFallbackConfig` flags, and `CommandDescriptor`/`CommandDirective`/`BuiltInOptions`/`AutoCorrect`/`Primitive` are the internal parse machinery these typed surfaces subsume.

[SURFACES]: `CliApp` `Environment` `ConstructorArgs` `make` `layer` `defaultConfig` `defaultLayer` `ValidationError` `isHelpRequested` `isValidationError` `isMissingFlag` `isMissingValue`

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Verb families export `Command` values as data; only the app root folds selected verbs through `withSubcommands` into one `Command.run` boundary, so no lib-side assembled root `Command` exists.

[STACKING]:
- `@effect/printer`(`.api/effect-printer.md`): `cli/render.ts` composes verb output as `Doc<Ansi>` through the combinator/layout surface (`vcat`/`hsep`/`nest`/`align`/`annotate`), never a pre-joined string.
- `@effect/printer-ansi`(`.api/effect-printer-ansi.md`): `HelpDoc.toAnsiDoc` yields an `AnsiDoc`, and both help and verb output lower to the terminal string through `AnsiDoc.render` — one render rail, no second styling path.
- `@effect/rpc`(`.api/effect-rpc.md`): `Command` and `RpcGroup` are the two contribution families under one edge assembly law, so a capability shipped as a verb and as an rpc method reuses one handler `Effect`.
- `@effect/platform`(`.api/effect-platform.md`): `Command.run` yields `Effect<…, R | Environment>` with `Environment = FileSystem | Path | Terminal`, satisfied by `@effect/platform-node` `NodeContext.layer` or `@effect/platform-bun` — one runtime choice covers CLI and server.
- `effect`(`.api/effect.md`): `Options.withSchema`/`Args.withSchema`/`fileSchema` decode a flag, arg, or file into a kernel brand at the terminal edge, and `withFallbackConfig`/`ConfigFile.layer` fold a flag into the `proc/config` provider chain so a flag and its env var are never two sources.
- `proc/exec` ops family: doctor/replay/inspect are `Command` values whose handlers run over `proc/exec` runtime rows, and `Command.provide` scopes their Layers to the ops subtree without leaking process capability to app verbs.

[RAIL_LAW]:
- Package: `@effect/cli`
- Owns: the terminal entry family — `Command` contribution values, the shared `Options`/`Args` combinator algebra, `Prompt` interaction, and the argv boundary.
- Accept: verb families folded into one root via `withSubcommands`; `withFallbackConfig`/`withSchema` boundary decode; `HelpDoc` rendered through `AnsiDoc.render`.
- Reject: a lib-side assembled root `Command`, parallel per-type flag methods where one combinator owns the axis, hand-rolled argv parsing, a second CLI entry outside the one `Command.run`.
