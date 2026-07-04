# [@effect/cli] — the Command/Args/Options/Prompt vocabulary behind the cli/verb contribution families; the app assembles exactly one Command root

`@effect/cli` is the terminal entry family under the one edge assembly law. Each verb family in `cli/verb.ts` is a `Command<Name, R, E, A>` VALUE — itself an `Effect` yielding its parsed config — exported as data; the APP folds selected families through `Command.withSubcommands` into exactly one root and runs it via `Command.run({name, version})` under the platform `Environment` (`FileSystem | Path | Terminal`). The god-CLI is structurally impossible because the root has no lib-side existence. doctor/replay/inspect ship as the lib ops family, their handlers executing over `host/exec`. `cli/render.ts` renders output through `@effect/printer`(-ansi), and the package's own `HelpDoc` lowers onto that same rail. This is the terminal peer of the `@effect/rpc` `RpcGroup` contribution family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/cli`
- package: `@effect/cli` `0.75.2` — license `MIT`
- module: dual ESM/CJS; types `dist/dts/index.d.ts`; per-module subpaths `Command`, `Args`, `Options`, `Prompt`, `CliApp`, `CliConfig`, `ConfigFile`, `HelpDoc` (+ `HelpDoc/Span`), `Primitive`, `Usage`, `ValidationError`, `CommandDescriptor`, `CommandDirective`, `BuiltInOptions`, `AutoCorrect`
- peers: `effect` `^3.21.2`, `@effect/platform` `^0.96.1`, `@effect/printer` + `@effect/printer-ansi` `^0.49.0` (catalog-resolved to `3.21.4` / `0.96.2` / `0.49.0`); the `Environment` is satisfied by `@effect/platform-node` `NodeContext.layer` or `@effect/platform-bun`
- bound asset: TSDECL `node_modules/@effect/cli/dist/dts/*.d.ts` (`assay api resolve @effect/cli` → `0.75.2`)
- admission: folder-local `# edge` catalog group; version centralized in `pnpm-workspace.yaml`
- role: `cli/verb.ts` (Command contribution families + the one-root assembly), `cli/render.ts` (Doc-composed output); the ops family executes over `host/exec`
- rail: `edge/cli`

## [02]-[COMMAND_OWNER]

A `Command` is an `Effect` that yields its parsed config and requires its own tag; the handler is the effectful action. Constructors build a leaf; combinators nest and inject; `run` is the argv boundary.

```ts
// @effect/cli/Command — Command<Name, R, E, A> extends Effect<A, never, Command.Context<Name>>, Pipeable
declare const make: {
  <Name extends string>(name: Name): Command<Name, never, never, {}>
  <Name extends string, const C extends Command.Config>(name: Name, config: C): Command<Name, never, never, ParseConfig<C>>
  <Name extends string, const C extends Command.Config, R, E>(name: Name, config: C, handler: (_: ParseConfig<C>) => Effect<void, E, R>): Command<Name, R, E, ParseConfig<C>>
}
declare const prompt: <Name extends string, A, R, E>(name: Name, prompt: Prompt<A>, handler: (_: A) => Effect<void, E, R>) => Command<string, R, E, A>
declare const fromDescriptor // wrap a raw CommandDescriptor (+ optional handler)

// combinators (all dual data-first / data-last)
declare const withHandler       // attach/replace the effectful action
declare const withDescription    // string | HelpDoc
declare const withSubcommands     // nest children; A gains `subcommand: Option<…>`, R/E union the children's
declare const provide             // Layer<LA,LE,LR> | ((A) => Layer) scoped to this command + subtree
declare const provideEffect / provideEffectDiscard / provideSync   // Tag-scoped service injection
declare const transformHandler    // wrap the handler effect with (effect, config) => effect

// accessors + the argv boundary
declare const getHelp / getUsage / getNames / getSubcommands
declare const getBashCompletions / getFishCompletions / getZshCompletions   // Effect<Array<string>>
declare const wizard              // interactive arg-builder → Effect<Array<string>, …, Environment>
declare const run: {
  (config: Omit<CliApp.ConstructorArgs<never>, "command">): <Name, R, E, A>(self: Command<Name, R, E, A>) => (args: ReadonlyArray<string>) => Effect<void, E | ValidationError, R | CliApp.Environment>
  <Name, R, E, A>(self: Command<Name, R, E, A>, config: Omit<CliApp.ConstructorArgs<never>, "command">): (args: ReadonlyArray<string>) => Effect<void, E | ValidationError, R | CliApp.Environment>
}
```

Consumer note: each verb family is a `Command` value; the app folds selected families through `withSubcommands([verbA, verbB, …])` into ONE root, then `Command.run({ name, version })` turns it into `argv => Effect<void, E | ValidationError, R | Environment>`. `provide` scopes a Layer to a verb subtree so the ops family's `host/exec` runtime is injected per-command, never at the whole tree.

## [03]-[OPTIONS_ARGS_ALGEBRA]

`Options<A>` (named flags) and `Args<A>` (positionals) are one combinator algebra over a variance carrier — arity, cardinality, fallback, and decode live in the combinators, never in parallel per-type constructors. Constructors differ only by the leading `name` (Options) vs an optional config (Args).

```ts
// constructors (shared): boolean · text · integer · float · date · choice · redacted · file · directory · fileSchema · none · all · isOptions/isArgs
// Options-only: choiceWithValue · fileContent · fileText · fileParse · keyValueMap        Args-only: path · getMinSize · getMaxSize
declare const boolean: (name: string, config?: Options.BooleanOptionsConfig) => Options<boolean>
declare const choice:  <A extends string, C extends ReadonlyArray<A>>(name: string, choices: C) => Options<C[number]>
declare const all:     <Arg extends Iterable<Options<any>> | Record<string, Options<any>>>(arg: Arg) => Options</* composite */>

// combinators (shared, dual signatures): map · mapEffect · mapTryCatch · optional · repeated · atLeast · atMost · between · withDefault · withFallbackConfig · withSchema · withDescription
// Options adds: filterMap · orElse · orElseEither · withAlias · withPseudoName · withFallbackPrompt
declare const withFallbackConfig: <B>(config: Config<B>) => <A>(self: Options<A>) => Options<B | A>   // fall back to an effect/Config value
declare const withSchema:         <A, I extends A, B>(schema: Schema<B, I, FileSystem | Path | Terminal>) => (self: Options<A>) => Options<B>
declare const withFallbackPrompt: <B>(prompt: Prompt<B>) => <A>(self: Options<A>) => Options<B | A>   // prompt when the flag is absent
```

Consumer note: `withFallbackConfig(Config)` is the bridge that unifies a CLI flag with an `effect/Config` value (env / config provider) in one declaration — the canonical way a verb resolves a flag from `host/config`. `withSchema(Schema)` decodes a flag straight into a kernel-branded value, so the terminal boundary decodes-once exactly like the wire and route boundaries. The one asymmetry: `Args.mapEffect` fails with `HelpDoc` while `Options.mapEffect` fails with `ValidationError`; `Args.validate` returns `[leftover, A]` and `Options.processCommandLine` returns `[Option<ValidationError>, leftover, A]`.

## [04]-[PROMPT]

`Prompt<Output>` is both a variance carrier and an `Effect<Output, QuitException, Terminal>`, so a prompt is yieldable directly in `Effect.gen`. The constructor roster is a fixed set of interaction atoms; `custom` owns any bespoke render/process loop.

```ts
// @effect/cli/Prompt — constructors: text · hidden · password · integer · float · confirm · toggle · date · list · file · select · multiSelect · succeed · custom · all
declare const select: <const A>(options: { message: string; choices: ReadonlyArray<{ title: string; value: A; description?: string; disabled?: boolean }>; maxPerPage?: number }) => Prompt<A>
declare const custom: <State, Output>(initialState: State | Effect<State, never, Prompt.Environment>, handlers: Prompt.Handlers<State, Output>) => Prompt<Output>
declare const map / flatMap / run   // Prompt<A> combinators; run → Effect<Output, QuitException, FileSystem | Path | Terminal>
```

Consumer note: a prompt plugs into a command two ways — `Command.prompt(name, prompt, handler)` for a prompt-driven leaf, or `Options.withFallbackPrompt(prompt)` to prompt only when a flag is absent (the doctor/replay ops family uses the latter for missing targets).

## [05]-[CONFIG_ERRORS_DOCS]

The app description, parser policy, the closed parse-error rail, the help algebra, and the config-file provider.

```ts
// @effect/cli/CliApp
declare namespace CliApp { type Environment = FileSystem | Path | Terminal; interface ConstructorArgs<A> { name; version; command; executable?; summary?; footer? } }
declare const make: <A>(config: CliApp.ConstructorArgs<A>) => CliApp<A>   // Command.run constructs one internally

// @effect/cli/CliConfig — Context.Tag service (isCaseSensitive · autoCorrectLimit · finalCheckBuiltIn · showAllNames · showBuiltIns · showTypes)
declare const layer: (config?: Partial<CliConfig>) => Layer<CliConfig>    // override parser policy at the root
declare const defaultConfig: CliConfig; declare const defaultLayer: Layer<CliConfig>

// @effect/cli/ValidationError — closed _tag union; each carries `error: HelpDoc`
type ValidationError = CommandMismatch | CorrectedFlag | HelpRequested | InvalidArgument | InvalidValue | MissingValue | MissingFlag | MultipleValuesDetected | MissingSubcommand | NoBuiltInMatch | UnclusteredFlag
declare const isHelpRequested: (self: ValidationError) => self is HelpRequested   // the clean-exit discriminant
declare const isValidationError / isMissingFlag / isMissingValue / …             // one refinement per member + one constructor per member

// @effect/cli/HelpDoc — Empty | Header | Paragraph | DescriptionList | Enumeration | Sequence; render via toAnsiText / toAnsiDoc; Span is the inline leaf, Usage the generated usage
// @effect/cli/ConfigFile — load a file as an effect/Config ConfigProvider so withFallbackConfig flags resolve from disk
declare const layer: (fileName: string, options?: { formats?: ReadonlyArray<"json" | "yaml" | "ini" | "toml">; searchPaths?: ReadonlyArray<string> }) => Layer<never, ConfigFileError, Path | FileSystem>
```

Consumer note: `isHelpRequested` is matched in the run rail to treat `--help`/`--version` as a clean exit rather than a failure; `CliConfig.layer({ isCaseSensitive })` overrides parser policy at the app root; `ConfigFile.layer(name)` layers a disk `ConfigProvider` so `withFallbackConfig` flags read from a config file. `CommandDescriptor`/`CommandDirective`/`BuiltInOptions`/`AutoCorrect`/`Primitive` are the internal parse machinery the typed surfaces above subsume.

## [06]-[STACKING]

- `@effect/platform` env: `Command.run` yields `Effect<…, R | Environment>` where `Environment = FileSystem | Path | Terminal`; satisfy it with `@effect/platform-node` `NodeContext.layer` (node) or `@effect/platform-bun` — the same serve-row selection `host/exec` makes for the HTTP family, so one runtime choice covers CLI and server.
- sibling `@effect/printer`(-ansi): `cli/render.ts` composes `Doc` (`text`/`vcat`/`hsep`/`nest`/`annotate`) and renders through `@effect/printer-ansi` `Ansi`/`AnsiDoc`; `HelpDoc.toAnsiDoc` IS a `@effect/printer-ansi` `AnsiDoc`, so parse-error help and app output share one render rail — no second styling path.
- sibling `@effect/rpc`: `RpcGroup` and `Command` are the two contribution families under the one assembly law — the app assembles one `HttpApi`, selects `RpcServer` protocol rows, and folds one `Command` root; the verb and rpc families compose identically, so a capability shipped as an rpc method and a verb reuses one handler `Effect`.
- `effect` `Config` + `host/config`: `withFallbackConfig`/`ConfigFile.layer` fold CLI flags into the `host/config` provider chain (env → doppler → file → remote) in one declaration, so a flag and its env var are never two sources.
- kernel `Schema`: `Options.withSchema`/`Args.withSchema`/`fileSchema(schema)` decode a flag/arg/file into a kernel brand at the terminal boundary — decode-once, matching the wire and route boundaries.
- `host/exec` ops family: doctor/replay/inspect are `Command` values whose handlers run over `host/exec` runtime rows; `Command.provide` scopes their Layers so the ops subtree carries its process/exec capability without leaking it to app verbs.

## [07]-[RAIL_LAW]

- Owns: the terminal entry family — `Command` contribution values, the shared `Options`/`Args` combinator algebra, `Prompt` interaction, and the argv boundary.
- Accept: verb families folded into one root via `withSubcommands`; `withFallbackConfig`/`withSchema` boundary decode; `HelpDoc` rendered through the `@effect/printer-ansi` rail.
- Reject: a lib-side assembled root `Command` (app-only, named defect); parallel per-type flag methods where one combinator owns the axis; hand-rolled argv parsing; a second CLI entry outside the one `Command.run`.
