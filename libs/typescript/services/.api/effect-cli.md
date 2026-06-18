# [API_CATALOGUE] effect-cli

Grounded from installed `node_modules` type declarations (`@effect/cli` 0.75.2; peers `@effect/platform` 0.96.1, `effect` 3.21.3, `@effect/printer-ansi` 0.49.0). Covers the typed-command binding surface the TS planning pages consume — `AutomationDriver` (the `provisioning/contract#PROVISIONING` cluster) is the owner-symbol that internalizes this package: a `Command` tree whose handlers are `Effect`, executed via `Command.run` / `CliApp.run` under a host `Environment` (`FileSystem | Path | Terminal`). Module set re-exported from the package index: `Args`, `AutoCorrect`, `BuiltInOptions`, `CliApp`, `CliConfig`, `Command`, `CommandDescriptor`, `CommandDirective`, `ConfigFile`, `HelpDoc`, `Options`, `Primitive`, `Prompt`, `Usage`, `ValidationError`, `Span`.

---

## [1] — Command

The typed-command owner. A `Command` is itself an `Effect` yielding its parsed config and requiring its own `Command.Context<Name>` tag; the handler is the effectful action.

```ts
// @effect/cli/Command
export declare const TypeId: unique symbol
export type TypeId = typeof TypeId

export interface Command<Name extends string, R, E, A>
  extends Pipeable, Effect<A, never, Command.Context<Name>> {
  readonly [TypeId]: TypeId
  readonly descriptor: Descriptor.Command<A>
  readonly handler: (_: A) => Effect<void, E, R>
  readonly tag: Tag<Command.Context<Name>, A>
  readonly transform: Command.Transform<R, E, A>
}

export declare namespace Command {
  interface Context<Name extends string> { readonly _: unique symbol; readonly name: Name }
  // config tree: keys map to Args / Options / nested Config / arrays thereof
  interface Config {
    readonly [key: string]: Args<any> | Options<any> | ReadonlyArray<Args<any> | Options<any> | Config> | Config
  }
  type ParseConfig<A extends Config> = Types.Simplify<{ readonly [K in keyof A]: ParseConfigValue<A[K]> }>
  type Transform<R, E, A> = (effect: Effect<void, any, any>, config: A) => Effect<void, E, R>
}
```

Constructors:

```ts
// the canonical entry: name + structured config tree + handler
export declare const make: {
  <Name extends string>(name: Name): Command<Name, never, never, {}>
  <Name extends string, const Config extends Command.Config>(
    name: Name, config: Config
  ): Command<Name, never, never, Types.Simplify<Command.ParseConfig<Config>>>
  <Name extends string, const Config extends Command.Config, R, E>(
    name: Name, config: Config,
    handler: (_: Types.Simplify<Command.ParseConfig<Config>>) => Effect<void, E, R>
  ): Command<Name, R, E, Types.Simplify<Command.ParseConfig<Config>>>
}

// prompt-driven leaf command
export declare const prompt: <Name extends string, A, R, E>(
  name: Name, prompt: Prompt<A>, handler: (_: A) => Effect<void, E, R>
) => Command<string, R, E, A>

// build from a raw CommandDescriptor (+ optional handler)
export declare const fromDescriptor: {
  (): <A extends { readonly name: string }>(command: Descriptor.Command<A>) => Command<A["name"], never, never, A>
  <A extends { readonly name: string }, R, E>(handler: (_: A) => Effect<void, E, R>): (command: Descriptor.Command<A>) => Command<A["name"], R, E, A>
  <A extends { readonly name: string }>(descriptor: Descriptor.Command<A>): Command<A["name"], never, never, A>
  <A extends { readonly name: string }, R, E>(descriptor: Descriptor.Command<A>, handler: (_: A) => Effect<void, E, R>): Command<A["name"], R, E, A>
}
```

Combinators (dual data-first / data-last signatures throughout):

```ts
export declare const withHandler: {
  <A, R, E>(handler: (_: A) => Effect<void, E, R>): <Name extends string, XR, XE>(self: Command<Name, XR, XE, A>) => Command<Name, R, E, A>
  <Name extends string, XR, XE, A, R, E>(self: Command<Name, XR, XE, A>, handler: (_: A) => Effect<void, E, R>): Command<Name, R, E, A>
}
export declare const withDescription: {
  (help: string | HelpDoc): <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => Command<Name, R, E, A>
  <Name extends string, R, E, A>(self: Command<Name, R, E, A>, help: string | HelpDoc): Command<Name, R, E, A>
}
// nests child commands; result A gains a `subcommand: Option<...>` discriminant and the union of child R/E
export declare const withSubcommands: {
  <Subcommand extends readonly [Command<any, any, any, any>, ...Array<Command<any, any, any, any>>]>(
    subcommands: Subcommand
  ): <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => Command<
    Name,
    R | Exclude<Effect.Context<ReturnType<Subcommand[number]["handler"]>>, Command.Context<Name>>,
    E | Effect.Error<ReturnType<Subcommand[number]["handler"]>>,
    Descriptor.Command.ComputeParsedType<A & Readonly<{ subcommand: Option<Descriptor.Command.GetParsedType<Subcommand[number]["descriptor"]>> }>>
  >
  // + data-first overload
}
// layer / service injection scoped to a command and its subtree
export declare const provide: {
  <A, LR, LE, LA>(layer: Layer<LA, LE, LR> | ((_: A) => Layer<LA, LE, LR>)): <Name extends string, R, E>(self: Command<Name, R, E, A>) => Command<Name, LR | Exclude<R, LA>, LE | E, A>
  <Name extends string, R, E, A, LR, LE, LA>(self: Command<Name, R, E, A>, layer: Layer<LA, LE, LR> | ((_: A) => Layer<LA, LE, LR>)): Command<Name, LR | Exclude<R, LA>, E | LE, A>
}
export declare const provideEffect: {
  <I, S, A, R2, E2>(tag: Tag<I, S>, effect: Effect<S, E2, R2> | ((_: A) => Effect<S, E2, R2>)): <Name extends string, R, E>(self: Command<Name, R, E, A>) => Command<Name, R2 | Exclude<R, I>, E2 | E, A>
  // + data-first overload
}
export declare const provideEffectDiscard: { /* same shape, discards the effect result */ }
export declare const provideSync: {
  <I, S, A>(tag: Tag<I, S>, service: S | ((_: A) => S)): <Name extends string, R, E>(self: Command<Name, R, E, A>) => Command<Name, Exclude<R, I>, E, A>
  // + data-first overload
}
export declare const transformHandler: {
  <R, E, A, R2, E2>(f: (effect: Effect<void, E, R>, config: A) => Effect<void, E2, R2>): <Name extends string>(self: Command<Name, R, E, A>) => Command<Name, R | R2, E | E2, A>
  // + data-first overload
}
```

Accessors and execution:

```ts
export declare const getHelp: <Name extends string, R, E, A>(self: Command<Name, R, E, A>, config: CliConfig) => HelpDoc
export declare const getNames: <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => HashSet<string>
export declare const getSubcommands: <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => HashMap<string, Descriptor.Command<unknown>>
export declare const getUsage: <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => Usage
export declare const getBashCompletions: <Name extends string, R, E, A>(self: Command<Name, R, E, A>, programName: string) => Effect<Array<string>>
export declare const getFishCompletions: <Name extends string, R, E, A>(self: Command<Name, R, E, A>, programName: string) => Effect<Array<string>>
export declare const getZshCompletions: <Name extends string, R, E, A>(self: Command<Name, R, E, A>, programName: string) => Effect<Array<string>>
export declare const wizard: {
  (prefix: ReadonlyArray<string>, config: CliConfig): <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => Effect<Array<string>, QuitException | ValidationError, FileSystem | Path | Terminal>
  // + data-first overload
}

// the app boundary: turns a Command into argv -> Effect
export declare const run: {
  (config: Omit<CliApp.ConstructorArgs<never>, "command">): <Name extends string, R, E, A>(self: Command<Name, R, E, A>) => (args: ReadonlyArray<string>) => Effect<void, E | ValidationError, R | CliApp.Environment>
  <Name extends string, R, E, A>(self: Command<Name, R, E, A>, config: Omit<CliApp.ConstructorArgs<never>, "command">): (args: ReadonlyArray<string>) => Effect<void, E | ValidationError, R | CliApp.Environment>
}
```

`Command.run` is the single entry point: supply `{ name, version, ... }` (everything in `CliApp.ConstructorArgs` except `command`), get back `argv => Effect<void, E | ValidationError, R | Environment>`. This is the value `AutomationDriver` wires into the runtime under the platform `Environment` layer.

---

## [2] — CliApp

Lower-level application description; `Command.run` constructs and runs one internally. Direct use is the alternative to `Command.run` when the command is already a raw `CommandDescriptor`.

```ts
// @effect/cli/CliApp
export interface CliApp<A> extends Pipeable {
  readonly name: string
  readonly version: string
  readonly executable: string
  readonly command: CommandDescriptor.Command<A>
  readonly summary: Span
  readonly footer: HelpDoc
}

export declare namespace CliApp {
  type Environment = FileSystem | Path | Terminal
  interface ConstructorArgs<A> {
    readonly name: string
    readonly version: string
    readonly command: CommandDescriptor.Command<A>
    readonly executable?: string | undefined
    readonly summary?: Span | undefined
    readonly footer?: HelpDoc | undefined
  }
}

export declare const make: <A>(config: CliApp.ConstructorArgs<A>) => CliApp<A>
export declare const run: {
  <R, E, A>(args: ReadonlyArray<string>, execute: (a: A) => Effect<void, E, R>): (self: CliApp<A>) => Effect<void | E, CliApp.Environment | R, ValidationError>
  <R, E, A>(self: CliApp<A>, args: ReadonlyArray<string>, execute: (a: A) => Effect<void, E, R>): Effect<void | E, CliApp.Environment | R, ValidationError>
}
```

`CliApp.Environment = FileSystem | Path | Terminal` is the platform requirement every CLI execution carries; satisfy it with `@effect/platform-node`'s `NodeContext.layer` (or `NodeTerminal` + `NodeFileSystem` + `NodePath`).

---

## [3] — Options

Named flags (`--name`). Parameterized by their decoded value `A`. `Options<A>` is a `Pipeable` variance carrier (not an `Effect`).

```ts
// @effect/cli/Options
export declare const OptionsTypeId: unique symbol
export interface Options<A> extends Options.Variance<A>, Pipeable {}

export declare namespace Options {
  interface BooleanOptionsConfig { readonly ifPresent?: boolean; readonly negationNames?: ReadonlyArray<string>; readonly aliases?: ReadonlyArray<string> }
  interface PathOptionsConfig { readonly exists?: Primitive.PathExists }
}
```

Primitive constructors (each takes the flag `name` first):

```ts
export declare const boolean: (name: string, options?: Options.BooleanOptionsConfig) => Options<boolean>
export declare const text: (name: string) => Options<string>
export declare const integer: (name: string) => Options<number>
export declare const float: (name: string) => Options<number>
export declare const date: (name: string) => Options<globalThis.Date>
export declare const choice: <A extends string, C extends ReadonlyArray<A>>(name: string, choices: C) => Options<C[number]>
export declare const choiceWithValue: <C extends ReadonlyArray<[string, any]>>(name: string, choices: C) => Options<C[number][1]>
export declare const redacted: (name: string) => Options<Redacted>
export declare const secret: (name: string) => Options<Secret>          // @deprecated — use redacted
export declare const keyValueMap: (option: string | Options<string>) => Options<HashMap<string, string>>
export declare const none: Options<void>
// path / file family
export declare const directory: (name: string, config?: Options.PathOptionsConfig) => Options<string>
export declare const file: (name: string, config?: Options.PathOptionsConfig) => Options<string>
export declare const fileContent: (name: string) => Options<readonly [path: string, content: Uint8Array]>
export declare const fileText: (name: string) => Options<readonly [path: string, content: string]>
export declare const fileParse: (name: string, format?: "json" | "yaml" | "ini" | "toml") => Options<unknown>
export declare const fileSchema: <I, A>(name: string, schema: Schema<A, I, FileSystem | Path | Terminal>, format?: "json" | "yaml" | "ini" | "toml") => Options<A>
// aggregate tuple / iterable / struct of Options into one Options of the composite
export declare const all: <const Arg extends Iterable<Options<any>> | Record<string, Options<any>>>(arg: Arg) => All.Return<Arg>
export declare const isOptions: (u: unknown) => u is Options<unknown>
```

Combinators (dual signatures; arity/cardinality and fallbacks live here, not in parallel constructors):

```ts
export declare const map: { <A, B>(f: (a: A) => B): (self: Options<A>) => Options<B>; <A, B>(self: Options<A>, f: (a: A) => B): Options<B> }
export declare const mapEffect: { <A, B>(f: (a: A) => Effect<B, ValidationError, FileSystem | Path | Terminal>): (self: Options<A>) => Options<B>; /* + data-first */ }
export declare const mapTryCatch: { <A, B>(f: (a: A) => B, onError: (e: unknown) => HelpDoc): (self: Options<A>) => Options<B>; /* + data-first */ }
export declare const filterMap: { <A, B>(f: (a: A) => Option<B>, message: string): (self: Options<A>) => Options<B>; /* + data-first */ }
export declare const optional: <A>(self: Options<A>) => Options<Option<A>>
export declare const repeated: <A>(self: Options<A>) => Options<Array<A>>
export declare const atLeast: { (times: 0): <A>(self: Options<A>) => Options<Array<A>>; (times: number): <A>(self: Options<A>) => Options<NonEmptyArray<A>>; /* + data-first */ }
export declare const atMost: { (times: number): <A>(self: Options<A>) => Options<Array<A>>; /* + data-first */ }
export declare const between: { (min: 0, max: number): <A>(self: Options<A>) => Options<Array<A>>; (min: number, max: number): <A>(self: Options<A>) => Options<NonEmptyArray<A>>; /* + data-first */ }
export declare const orElse: { <A>(that: Options<A>): <B>(self: Options<B>) => Options<A | B>; /* + data-first */ }
export declare const orElseEither: { <A>(that: Options<A>): <B>(self: Options<B>) => Options<Either<A, B>>; /* + data-first */ }
export declare const withDefault: { <const B>(fallback: B): <A>(self: Options<A>) => Options<B | A>; /* + data-first */ }
export declare const withFallbackConfig: { <B>(config: Config<B>): <A>(self: Options<A>) => Options<B | A>; /* + data-first */ }
export declare const withFallbackPrompt: { <B>(prompt: Prompt<B>): <A>(self: Options<A>) => Options<B | A>; /* + data-first */ }
export declare const withAlias: { (alias: string): <A>(self: Options<A>) => Options<A>; /* + data-first */ }
export declare const withDescription: { (description: string): <A>(self: Options<A>) => Options<A>; /* + data-first */ }
export declare const withPseudoName: { (pseudoName: string): <A>(self: Options<A>) => Options<A>; /* + data-first */ }
export declare const withSchema: { <A, I extends A, B>(schema: Schema<B, I, FileSystem | Path | Terminal>): (self: Options<A>) => Options<B>; /* + data-first */ }
// introspection / manual parse
export declare const getHelp: <A>(self: Options<A>) => HelpDoc
export declare const getIdentifier: <A>(self: Options<A>) => Option<string>
export declare const getUsage: <A>(self: Options<A>) => Usage
export declare const isBool: <A>(self: Options<A>) => boolean
export declare const parse: { (args: HashMap<string, ReadonlyArray<string>>, config: CliConfig): <A>(self: Options<A>) => Effect<A, ValidationError, FileSystem>; /* + data-first */ }
export declare const processCommandLine: { (args: ReadonlyArray<string>, config: CliConfig): <A>(self: Options<A>) => Effect<[Option<ValidationError>, Array<string>, A], ValidationError, FileSystem | Path | Terminal>; /* + data-first */ }
export declare const wizard: { (config: CliConfig): <A>(self: Options<A>) => Effect<Array<string>, QuitException | ValidationError, FileSystem | Path | Terminal>; /* + data-first */ }
```

`withFallbackConfig` is the bridge that lets a flag fall back to an `effect/Config` value (env var / config provider) — the canonical way `AutomationDriver` unifies CLI flags and environment configuration in one declaration.

---

## [4] — Args

Positional arguments. Same variance/combinator algebra as `Options`, but constructors take an optional `config` (custom name) instead of a leading `name`.

```ts
// @effect/cli/Args
export declare const ArgsTypeId: unique symbol
export interface Args<A> extends Args.Variance<A>, Pipeable {}

export declare namespace Args {
  interface BaseArgsConfig { readonly name?: string }
  interface PathArgsConfig extends BaseArgsConfig { readonly exists?: Primitive.PathExists }
  interface FormatArgsConfig extends BaseArgsConfig { readonly format?: "json" | "yaml" | "ini" | "toml" }
}
```

Constructors:

```ts
export declare const text: (config?: Args.BaseArgsConfig) => Args<string>
export declare const boolean: (options?: Args.BaseArgsConfig) => Args<boolean>
export declare const integer: (config?: Args.BaseArgsConfig) => Args<number>
export declare const float: (config?: Args.BaseArgsConfig) => Args<number>
export declare const date: (config?: Args.BaseArgsConfig) => Args<globalThis.Date>
export declare const choice: <A>(choices: ReadonlyArray<[string, A]>, config?: Args.BaseArgsConfig) => Args<A>
export declare const redacted: (config?: Args.BaseArgsConfig) => Args<Redacted>
export declare const secret: (config?: Args.BaseArgsConfig) => Args<Secret>
export declare const none: Args<void>
// path / file family
export declare const path: (config?: Args.PathArgsConfig) => Args<string>
export declare const directory: (config?: Args.PathArgsConfig) => Args<string>
export declare const file: (config?: Args.PathArgsConfig) => Args<string>
export declare const fileContent: (config?: Args.BaseArgsConfig) => Args<readonly [path: string, content: Uint8Array]>
export declare const fileText: (config?: Args.BaseArgsConfig) => Args<readonly [path: string, content: string]>
export declare const fileParse: (config?: Args.FormatArgsConfig) => Args<unknown>
export declare const fileSchema: <I, A>(schema: Schema<A, I, FileSystem | Path | Terminal>, config?: Args.FormatArgsConfig) => Args<A>
export declare const all: <const Arg extends Iterable<Args<any>> | Record<string, Args<any>>>(arg: Arg) => All.Return<Arg>
export declare const isArgs: (u: unknown) => u is Args<unknown>
```

Combinators (dual signatures):

```ts
export declare const map: { <A, B>(f: (a: A) => B): (self: Args<A>) => Args<B>; /* + data-first */ }
export declare const mapEffect: { <A, B>(f: (a: A) => Effect<B, HelpDoc, FileSystem | Path | Terminal>): (self: Args<A>) => Args<B>; /* + data-first */ }
export declare const mapTryCatch: { <A, B>(f: (a: A) => B, onError: (e: unknown) => HelpDoc): (self: Args<A>) => Args<B>; /* + data-first */ }
export declare const optional: <A>(self: Args<A>) => Args<Option<A>>
export declare const repeated: <A>(self: Args<A>) => Args<Array<A>>
export declare const atLeast: { (times: 0): <A>(self: Args<A>) => Args<Array<A>>; (times: number): <A>(self: Args<A>) => Args<NonEmptyArray<A>>; /* + data-first */ }
export declare const atMost: { (times: number): <A>(self: Args<A>) => Args<Array<A>>; /* + data-first */ }
export declare const between: { (min: 0, max: number): <A>(self: Args<A>) => Args<Array<A>>; (min: number, max: number): <A>(self: Args<A>) => Args<NonEmptyArray<A>>; /* + data-first */ }
export declare const withDefault: { <const B>(fallback: B): <A>(self: Args<A>) => Args<B | A>; /* + data-first */ }
export declare const withFallbackConfig: { <B>(config: Config<B>): <A>(self: Args<A>) => Args<B | A>; /* + data-first */ }
export declare const withDescription: { (description: string): <A>(self: Args<A>) => Args<A>; /* + data-first */ }
export declare const withSchema: { <A, I extends A, B>(schema: Schema<B, I, FileSystem | Path | Terminal>): (self: Args<A>) => Args<B>; /* + data-first */ }
// introspection / manual validate
export declare const getHelp: <A>(self: Args<A>) => HelpDoc
export declare const getIdentifier: <A>(self: Args<A>) => Option<string>
export declare const getMinSize: <A>(self: Args<A>) => number
export declare const getMaxSize: <A>(self: Args<A>) => number
export declare const getUsage: <A>(self: Args<A>) => Usage
export declare const validate: { (args: ReadonlyArray<string>, config: CliConfig): <A>(self: Args<A>) => Effect<[Array<string>, A], ValidationError, FileSystem | Path | Terminal>; /* + data-first */ }
export declare const wizard: { (config: CliConfig): <A>(self: Args<A>) => Effect<Array<string>, ValidationError | QuitException, FileSystem | Path | Terminal>; /* + data-first */ }
```

Note the asymmetry vs `Options`: `Args.mapEffect` fails with `HelpDoc` (not `ValidationError`), and `Args.validate` returns `[leftover, A]` while `Options.processCommandLine` returns `[Option<ValidationError>, leftover, A]`.

---

## [5] — Prompt

Interactive terminal prompts. `Prompt<Output>` is both a variance carrier and an `Effect<Output, QuitException, Terminal>`, so a prompt can be yielded directly inside an `Effect.gen`.

```ts
// @effect/cli/Prompt
export declare const PromptTypeId: unique symbol
export interface Prompt<Output>
  extends Prompt.Variance<Output>, Pipeable, Effect<Output, QuitException, Terminal> {}

export declare namespace Prompt {
  type Environment = FileSystem | Path | Terminal
  // tagged render-loop action
  type Action<State, Output> = TaggedEnum<{
    readonly Beep: {}
    readonly NextFrame: { readonly state: State }
    readonly Submit: { readonly value: Output }
  }>
  interface ActionDefinition extends TaggedEnum.WithGenerics<2> { readonly taggedEnum: Action<this["A"], this["B"]> }
  interface Handlers<State, Output> {
    readonly render: (state: State, action: Action<State, Output>) => Effect<string, never, Environment>
    readonly process: (input: UserInput, state: State) => Effect<Action<State, Output>, never, Environment>
    readonly clear: (state: State, action: Action<State, Output>) => Effect<string, never, Environment>
  }
  // option records (selected fields)
  interface TextOptions { readonly message: string; readonly default?: string; readonly validate?: (value: string) => Effect<string, string> }
  interface IntegerOptions { readonly message: string; readonly min?: number; readonly max?: number; readonly incrementBy?: number; readonly decrementBy?: number; readonly validate?: (value: number) => Effect<number, string> }
  interface FloatOptions extends IntegerOptions { readonly precision?: number }
  interface ListOptions extends TextOptions { readonly delimiter?: string }
  interface ConfirmOptions { readonly message: string; readonly initial?: boolean; readonly label?: { readonly confirm: string; readonly deny: string }; readonly placeholder?: { readonly defaultConfirm?: string; readonly defaultDeny?: string } }
  interface ToggleOptions { readonly message: string; readonly initial?: boolean; readonly active?: string; readonly inactive?: string }
  interface DateOptions { readonly message: string; readonly initial?: globalThis.Date; readonly dateMask?: string; readonly validate?: (value: globalThis.Date) => Effect<globalThis.Date, string>; readonly locales?: { /* months / monthsShort / weekdays / weekdaysShort tuples */ } }
  interface FileOptions { readonly type?: Primitive.PathType; readonly message?: string; readonly startingPath?: string; readonly maxPerPage?: number; readonly filter?: (file: string) => boolean | Effect<boolean, never, Environment> }
  interface SelectChoice<A> { readonly title: string; readonly value: A; readonly description?: string; readonly disabled?: boolean; readonly selected?: boolean }
  interface SelectOptions<A> { readonly message: string; readonly choices: ReadonlyArray<SelectChoice<A>>; readonly maxPerPage?: number }
  interface MultiSelectOptions { readonly selectAll?: string; readonly selectNone?: string; readonly inverseSelection?: string; readonly min?: number; readonly max?: number }
}
```

Constructors and combinators:

```ts
export declare const text: (options: Prompt.TextOptions) => Prompt<string>
export declare const hidden: (options: Prompt.TextOptions) => Prompt<Redacted>
export declare const password: (options: Prompt.TextOptions) => Prompt<Redacted>
export declare const integer: (options: Prompt.IntegerOptions) => Prompt<number>
export declare const float: (options: Prompt.FloatOptions) => Prompt<number>
export declare const confirm: (options: Prompt.ConfirmOptions) => Prompt<boolean>
export declare const toggle: (options: Prompt.ToggleOptions) => Prompt<boolean>
export declare const date: (options: Prompt.DateOptions) => Prompt<Date>
export declare const list: (options: Prompt.ListOptions) => Prompt<Array<string>>
export declare const file: (options?: Prompt.FileOptions) => Prompt<string>
export declare const select: <const A>(options: Prompt.SelectOptions<A>) => Prompt<A>
export declare const multiSelect: <const A>(options: Prompt.SelectOptions<A> & Prompt.MultiSelectOptions) => Prompt<Array<A>>
export declare const succeed: <A>(value: A) => Prompt<A>
export declare const custom: <State, Output>(initialState: State | Effect<State, never, Prompt.Environment>, handlers: Prompt.Handlers<State, Output>) => Prompt<Output>
export declare const all: <const Arg extends Iterable<Prompt<any>> | Record<string, Prompt<any>>>(arg: Arg) => All.Return<Arg>
export declare const map: { <Output, Output2>(f: (output: Output) => Output2): (self: Prompt<Output>) => Prompt<Output2>; /* + data-first */ }
export declare const flatMap: { <Output, Output2>(f: (output: Output) => Prompt<Output2>): (self: Prompt<Output>) => Prompt<Output2>; /* + data-first */ }
export declare const run: <Output>(self: Prompt<Output>) => Effect<Output, QuitException, Prompt.Environment>
```

`Prompt` plugs into a command two ways: `Command.prompt(name, prompt, handler)` for a prompt-driven leaf, or `Options.withFallbackPrompt(prompt)` to prompt when a flag is absent.

---

## [6] — CliConfig

Parser policy. A `Context.Tag`-backed service with a default and layer constructors.

```ts
// @effect/cli/CliConfig
export interface CliConfig {
  readonly isCaseSensitive: boolean      // default false
  readonly autoCorrectLimit: number      // Levenstein threshold, default 2
  readonly finalCheckBuiltIn: boolean    // default false
  readonly showAllNames: boolean         // default true
  readonly showBuiltIns: boolean         // default true
  readonly showTypes: boolean            // default true
}

export declare const CliConfig: Context.Tag<CliConfig, CliConfig>
export declare const defaultConfig: CliConfig
export declare const defaultLayer: Layer.Layer<CliConfig>
export declare const layer: (config?: Partial<CliConfig>) => Layer.Layer<CliConfig>
export declare const make: (params: Partial<CliConfig>) => CliConfig
export declare const normalizeCase: { (text: string): (self: CliConfig) => string; (self: CliConfig, text: string): string }
```

`CliConfig.layer(partial)` is how `AutomationDriver` overrides parser policy (e.g. `isCaseSensitive`) at the composition root; absent that, `Command.run` falls back to `defaultConfig`.

---

## [7] — ValidationError

The closed error rail surfaced by parsing (`Command.run` adds it to the error channel as `E | ValidationError`). A discriminated `_tag` union over a `Proto` brand.

```ts
// @effect/cli/ValidationError
export declare const ValidationErrorTypeId: unique symbol
export type ValidationError =
  | CommandMismatch | CorrectedFlag | HelpRequested | InvalidArgument | InvalidValue
  | MissingValue | MissingFlag | MultipleValuesDetected | MissingSubcommand | NoBuiltInMatch | UnclusteredFlag

export declare namespace ValidationError { interface Proto { readonly [ValidationErrorTypeId]: ValidationErrorTypeId } }

// each member: ValidationError.Proto + readonly _tag + readonly error: HelpDoc, plus:
//   HelpRequested            -> readonly command: Command<unknown>
//   MultipleValuesDetected   -> readonly values: ReadonlyArray<string>
//   UnclusteredFlag          -> readonly unclustered / rest: ReadonlyArray<string>

// refinements (one per member, all `(self|u) => self is <Member>`)
export declare const isValidationError: (u: unknown) => u is ValidationError
export declare const isCommandMismatch / isCorrectedFlag / isHelpRequested / isInvalidArgument / isInvalidValue:
  (self: ValidationError) => self is /* member */
export declare const isMissingValue / isMissingFlag / isMissingSubcommand / isMultipleValuesDetected / isNoBuiltInMatch / isUnclusteredFlag:
  (self: ValidationError) => self is /* member */

// constructors (each returns the wide ValidationError)
export declare const commandMismatch: (error: HelpDoc) => ValidationError
export declare const correctedFlag: (error: HelpDoc) => ValidationError
export declare const helpRequested: <A>(command: Command<A>) => ValidationError
export declare const invalidArgument: (error: HelpDoc) => ValidationError
export declare const invalidValue: (error: HelpDoc) => ValidationError
export declare const keyValuesDetected: (error: HelpDoc, keyValues: ReadonlyArray<string>) => ValidationError
export declare const missingFlag: (error: HelpDoc) => ValidationError
export declare const missingValue: (error: HelpDoc) => ValidationError
export declare const missingSubcommand: (error: HelpDoc) => ValidationError
export declare const noBuiltInMatch: (error: HelpDoc) => ValidationError
export declare const unclusteredFlag: (error: HelpDoc, unclustered: ReadonlyArray<string>, rest: ReadonlyArray<string>) => ValidationError
```

`isHelpRequested` is the discriminant `AutomationDriver` matches to treat help/version as a clean exit rather than a failure.

---

## [8] — Primitive

The string-parse atoms underlying `Options` / `Args` constructors. `Primitive<A>` carries parse + validate for one scalar type; rarely constructed directly but its two string-literal models flow through the higher surfaces.

```ts
// @effect/cli/Primitive
export declare const PrimitiveTypeId: unique symbol
export interface Primitive<A> extends Primitive.Variance<A> {}

export declare namespace Primitive {
  type PathExists = "yes" | "no" | "either"
  type PathType = "file" | "directory" | "either"
  type ValueType<P> = /* extracts A from a Primitive<A> */
}

export declare const boolean: (defaultValue: Option<boolean>) => Primitive<boolean>
export declare const choice: <A>(alternatives: ReadonlyArray<[string, A]>) => Primitive<A>
export declare const date: Primitive<globalThis.Date>
export declare const float: Primitive<number>
export declare const isBool: <A>(self: Primitive<A>) => boolean
export declare const getChoices: <A>(self: Primitive<A>) => Option<string>
// + integer / text / path constructors and getHelp / getTypeName / validate accessors
```

`PathExists` is the value passed as `Options.PathOptionsConfig.exists` / `Args.PathArgsConfig.exists`; `PathType` is `Prompt.FileOptions.type`.

---

## [9] — HelpDoc / Span / Usage

The documentation algebra. `HelpDoc` is the rendered-help model carried by every `ValidationError`, accepted by `Command.withDescription`, and returned by every `getHelp`.

```ts
// @effect/cli/HelpDoc
export type HelpDoc = Empty | Header | Paragraph | DescriptionList | Enumeration | Sequence
export interface Empty { readonly _tag: "Empty" }
export interface Header { readonly _tag: "Header"; readonly value: Span; readonly level: number }
export interface Paragraph { readonly _tag: "Paragraph"; readonly value: Span }
export interface DescriptionList { readonly _tag: "DescriptionList"; readonly definitions: NonEmptyReadonlyArray<readonly [Span, HelpDoc]> }
export interface Enumeration { readonly _tag: "Enumeration"; readonly elements: NonEmptyReadonlyArray<HelpDoc> }
export interface Sequence { readonly _tag: "Sequence"; readonly left: HelpDoc; readonly right: HelpDoc }
// refinements isEmpty / isHeader / isParagraph / isDescriptionList / isEnumeration / isSequence;
// constructors blocks / descriptionList / enumeration / h1..h3 / p; render toAnsiDoc / toAnsiText
// (Span is the inline-text leaf model under HelpDoc/Span; Usage is the generated usage model from getUsage)
```

---

## [10] — ConfigFile

Loads a config file as an `effect/Config` `ConfigProvider`, layered into the runtime so `withFallbackConfig` flags resolve from disk.

```ts
// @effect/cli/ConfigFile
export type Kind = "json" | "yaml" | "ini" | "toml"
export declare const ConfigErrorTypeId: unique symbol
export interface ConfigFileError extends YieldableError {
  readonly [ConfigErrorTypeId]: ConfigErrorTypeId
  readonly _tag: "ConfigFileError"
  readonly message: string
}
export declare const ConfigFileError: (message: string) => ConfigFileError
export declare const makeProvider: (fileName: string, options?: { readonly formats?: ReadonlyArray<Kind>; readonly searchPaths?: ReadonlyArray<string> }) => Effect<ConfigProvider, ConfigFileError, Path | FileSystem>
export declare const layer: (fileName: string, options?: { readonly formats?: ReadonlyArray<Kind>; readonly searchPaths?: ReadonlyArray<string> }) => Layer<never, ConfigFileError, Path | FileSystem>
```

---

## [11] — Secondary modules

`CommandDescriptor` (the untyped `Command<A>` the typed `Command` wraps via `.descriptor`; `Command.run` ultimately drives it), `CommandDirective` (the parse-result directive — `UserDefined` vs `BuiltIn` — produced internally), `BuiltInOptions` (the `--help` / `--version` / `--wizard` / `--completions` built-in flag set), and `AutoCorrect` (Levenstein flag-suggestion utility honouring `CliConfig.autoCorrectLimit`) are present but not load-bearing for `AutomationDriver`: the typed `Command` + `Options` + `Args` + `Prompt` + `Command.run` surfaces above subsume them.
