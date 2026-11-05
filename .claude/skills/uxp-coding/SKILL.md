---
name: uxp-coding
description: "Use when writing or reviewing code that runs in an Adobe UXP host, covering plugin runtime and host boundaries"
---

# [UXP_CODING]

UXP runs each plugin and script in one V8 context inside an Adobe host.

Ranges, enumerations, and localized names come from the running host, extents from the measured item, unit ratios from their standard, names of created files from their target, and every other value from the caller.

[REFERENCES]:
- [01]-[INDESIGN](references/indesign.md): InDesign behavior its dictionary and typings leave unstated, for code calling its synchronous DOM
- [02]-[PHOTOSHOP](references/photoshop.md): Photoshop behavior its prototypes and typings leave unstated, for descriptor and DOM calls
- [03]-[PREMIERE](references/premiere.md): Premiere Pro and Media Encoder behavior their manifests and typings leave unstated, for edits as actions
- [04]-[EXTENDSCRIPT](references/extendscript.md): Illustrator and After Effects behavior `reflect` and typings leave unstated, for their ES3 source
- [05]-[TRANSPORT](references/transport.md): Typed calls a plugin serves over a socket it dials

## [01]-[CONTEXTS]

Host modules differ in DOM call form:

| [INDEX] | [HOST]        | [MODULE]      | [DOM_CALLS]                                             |
| :-----: | :------------ | :------------ | :------------------------------------------------------ |
|  [01]   | InDesign      | `indesign`    | Synchronous, storage getters async                      |
|  [02]   | Photoshop     | `photoshop`   | Promises inside a modal scope                           |
|  [03]   | Premiere Pro  | `premierepro` | Async getters, action objects                           |
|  [04]   | Illustrator   | None          | CEP ExtendScript until UXP public beta                  |
|  [05]   | After Effects | None          | CEP ExtendScript until its release host installs `.ccx` |
|  [06]   | Acrobat       | None          | Acrobat JavaScript through `do script`                  |

- CEP turns off by default in December 2028 and leaves After Effects and Illustrator in December 2029
- `require` resolves the running host's DOM module alone, `uxp.host.name` names that host, and cross-host code takes host calls as arguments
- Unlinked Illustrator, After Effects, and Acrobat run source through Atelier `execute_script` over the host's dictionary script command
- Acrobat source runs privileged with `this` naming the front document, and `global` members persist between calls
- Acrobat JavaScript holds `JSON`, `util`, `eval`, `new Function`, and arrow functions, and no `let`, `$`, `File`, or `reflect`
- Acrobat answers uncaught throws and syntax errors with -1708 and no text, and source catches its own throws with `lineNumber` as `line`

Context decides what code holds across calls:

| [INDEX] | [CONTEXT] | [LIFETIME]   | [PERMISSIONS]                  | [HOLDS]                                                     |
| :-----: | :-------- | :----------- | :----------------------------- | :---------------------------------------------------------- |
|  [01]   | Plugin    | Host session | Manifest `requiredPermissions` | Listeners, sockets, timers, `localStorage`, `secureStorage` |
|  [02]   | Script    | One run      | Fixed by host                  | Values of that run, `sessionStorage` that ends with it      |

- Work that finishes after its call returns belongs to a plugin
- State a script needs in a later run is host state: a document, a file, or a stored value

## [02]-[RUNTIME]

Differences from a browser or Node hold in InDesign, Photoshop, and Premiere Pro, each with one owner:

| [INDEX] | [DIFFERENCE]                                  | [OWNER]   | [FORM]                                   |
| :-----: | :-------------------------------------------- | :-------- | :--------------------------------------- |
|  [01]   | Global constructed at load (`TextEncoder`)    | Bundle    | Package implementation injected per read |
|  [02]   | Member of another type (`hrtime.bigint`)      | Runtime   | `Clock` layer over `performance.now()`   |
|  [03]   | Absent global or module (`Worker`, `node:*`)  | Bundle    | Dependencies reading one stay out        |
|  [04]   | Module typed as Node's (`fs`, `path`)         | Read site | Value decoded where read                 |
|  [05]   | Feature with a missing part (`Response.body`) | Call site | Whole-body read, flag on whole pattern   |

Effect's encoding and socket modules construct `TextEncoder` at load, and bundles replace each encoding-global read with an `@exodus/bytes` import. Effect's default clock expects bigint nanoseconds, and UXP's `process.hrtime.bigint` returns a number. Host `Clock` layer reads monotonic nanoseconds from `performance.now()`, wall nanoseconds from each `currentTimeMillis` read, and keeps native sleep. Offsets computed once skip the skew correction Effect's wall clock performs. Host callbacks running effects receive caller services through `run*With`.

- Encoding packages must implement the `{ stream: true }` option Effect decodes with
- `structuredClone`, `Buffer`, `MessageChannel`, and `Worker` are absent in every host, and `require` of `util`, `buffer`, or a `node:` name throws
- `fs` holds synchronous reads, writes, and `lstatSync`, and no `existsSync`, `appendFileSync`, `mkdirSync`, or `statSync`
- `writeFileSync` reads its receiver, and code calls it on the module's default import
- `path` is a function with `resolve` and `basename`
- Effect's POSIX `Path` service converts paths and `file:` URLs, `toFileUrl` percent-encoding `%`, `#`, `?`, and space a `file:${path}` template breaks on

Bundles keep one form:
- Output is one CommonJS script with each host module left as a `require` call by module name, and an unresolved dependency throws at its first read
- Resolution uses neutral conditions: browser conditions select builds that construct absent globals at load, Node conditions emit `node:` requires
- Plugin `main` is a script that renders into `document.body`, and UXP never runs `<script type="module">`
- CommonJS output holds no top-level `await`, and dependencies instantiating WebAssembly through one at import stay out

## [03]-[TYPINGS]

Typings declare a host module under its `require` name, under an `adobe:` scheme, or as a type with no module:

| [INDEX] | [MODULES]                 | [PACKAGE]                    | [DECLARES]                                                       |
| :-----: | :------------------------ | :--------------------------- | :--------------------------------------------------------------- |
|  [01]   | `uxp`, `fs`, `os`, `path` | `@adobe-uxp-types/uxp`       | `uxp` by name, each module under `adobe:` from `with-protocol`   |
|  [02]   | `photoshop`               | `@adobe-uxp-types/photoshop` | `photoshop` by name, `adobe:photoshop` from `with-protocol`      |
|  [03]   | `photoshop`               | `@types/photoshop`           | Adobe's declarations, `photoshop` and `photoshop/dom/*` by name  |
|  [04]   | `indesign`                | `indesign-uxp-types`         | `indesign` by name, `adobe:indesign` from `flat` through `paths` |
|  [05]   | `premierepro`             | `@adobe/premierepro`         | Type `premierepro` of the module object, no module               |
|  [06]   | ExtendScript              | `types-for-adobe`            | Global classes per host folder (`AfterEffects/26.0`), no module  |

- Imports name host modules under `adobe:`, a prefix that separates `fs`, `os`, and `path` from Node's
- `types-for-adobe` host folders load `shared/JavaScript.d.ts` beside the project `lib`, where ES5 built-ins ExtendScript lacks type-check
- `types` lists `node` beside each `with-protocol` entry, and `node` types `require`, `process`, and `adobe:fs` members
- `paths` entries resolve against the declaring config's folder, and a shared config writes `${configDir}/node_modules/...` to resolve in each extending project
- Diagnostics on a call the object model states name a typings gap, and every other diagnostic names a defect in the call
- Typings can state another enumeration value than the host holds, and code reads enumeration members from the module

Each gap closes where its value is read, with types the package or a schema states:

| [INDEX] | [GAP]                                              | [FORM]                                                                   |
| :-----: | :------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Member, value, or failure typings omit or misstate | Host value decoded through a `Schema` at its read                        |
|  [02]   | Module object typed with no module                 | Module passed as an argument of the package's type                       |
|  [03]   | Same gap in ExtendScript without `Schema`          | Assertion to the host's shape, suppression naming gap                    |
|  [04]   | Global no package declares (`__adobe_cep__`)       | Interface, `globalThis` decoded through a struct of its function members |

`types-for-adobe` declares `saveFrameToPng` `void` where hosts return a deferred call with `wait()`, and option-class fields (`ExportOptionsPNG24`) `static` where writes index each instance.

Typings for `fs` declare bytes as `Buffer` and a failure's `code` as a string, where the host returns an `ArrayBuffer` and a numeric `code`:

```ts
import { readFileSync } from 'adobe:fs';

const bytes = (path: string) =>
    Effect.try({ try: () => Option.some(new Uint8Array(readFileSync(path))), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFile', pathOrDescriptor: path, cause }) }).pipe(Effect.catchIf(({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause), () => Effect.succeedNone));
```

`new Uint8Array` accepts both representations, and `PlatformError` holds the host's thrown value as `cause`.

## [04]-[OBJECT_MODELS]

Object model facts come from the owner that states them for the installed build:

| [INDEX] | [HOST]       | [OWNER]                                            | [READ]                                          |
| :-----: | :----------- | :------------------------------------------------- | :---------------------------------------------- |
|  [01]   | InDesign     | `$.dictionary` of the running host                 | `getClass(<name>).toXML()` rows                 |
|  [02]   | Photoshop    | Prototypes of the running host, `@types/photoshop` | Prototype members, method source parameters     |
|  [03]   | Premiere Pro | `ScriptingAPIManifest.json` and `api_config.json`  | `jq` read of the classes the allow-list exposes |
|  [04]   | ExtendScript | `reflect` of the running host, `types-for-adobe`   | Fields, methods with arguments, enumerations    |
|  [05]   | Acrobat      | `acrohelp` argument to a native method             | `HelpError.extMessage` per parameter            |

- UXP prototype chains up to `Object.prototype` name each class by `constructor.name`, fields by getters (settable with a setter), methods by own functions
- Acrobat's `extMessage` lists parameters as `name: type`, optional ones in brackets
- Native scripting dictionaries declare Apple event commands and parameter types, and a script command's parameters select its language and undo mode
- InDesign `show`, `reveal in system`, and `reveal in Bridge`, Acrobat `bring to front`, and Premiere Pro `editoriginal` dictionary commands activate their host
- Typings can predate the installed build and rename parameters a host names by reserved words, and renamed parameters call correctly by position
- Rejected calls complete each model: host errors name a parameter with its expected type and range, and whether a member applies now
- Probes run as undoable units, return failures as data, and leave no host dialog, test plugin, registry row, or test document behind
- `OSACopyScriptingDefinitionFromURL` returns XML that can contain XInclude references

## [05]-[HOST_ERRORS]

Hosts throw `Error` values with text in the user's locale, and return some failures as values.

`HostError` is the package error for failures no host error (`PlatformError`, `HttpClientError`) states, with refused step as `subject`, host's thrown value or returned failure as `cause`, and one reason per caller response:

| [INDEX] | [REASON]      | [HOST_OUTCOME]                                                     |
| :-----: | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Refused`     | Call refused or failure returned as a value                        |
|  [02]   | `Unavailable` | Host or bridge serves no call (modal scope held, no CEP reply)     |
|  [03]   | `Cancelled`   | User cancelled the call's progress window                          |
|  [04]   | `Thrown`      | Evaluated source threw, `line` holding the body line a frame names |

Messages derive `<subject> <reason>: <cause message>`, or `line <n>: <subject>` for `Thrown`. Rejection codes a host documents decode at `catch` into their reason through a table from tag to code and `Schema.Literal(code).transform(tag)`, and consumers branch on `reason._tag`. `HostError.refused(subject)` returns a host call's `catch` function, and failures returned as values call it with no cause:

```ts
const written = (target: object, values: Record.ReadonlyRecord<string, unknown>): Effect.Effect<readonly string[], HostError> =>
    Effect.validate(
        Record.toEntries(values),
        Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
            const refused = HostError.refused(field);
            const inner: unknown = Predicate.isObject(value) && Predicate.hasProperty(target, field) ? yield* Effect.try({ try: () => target[field], catch: refused }) : undefined;
            if (Predicate.isObject(value) && Predicate.isObject(inner)) {
                return Array.map(yield* written(inner, value), (name) => `${field}.${name}`);
            }
            yield* Effect.try({ try: () => Object.assign(target, { [field]: value }), catch: refused });
            return [field];
        }, Effect.sandbox),
    ).pipe(Effect.map(Array.flatten), Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))));
```

`written` writes every field and fails with one reason per refused field, a nested field named by its path. `Effect.validate` partitions elements through `Effect.result`, `Effect.sandbox` alone keeps nested reasons, and `Cause.fromReasons` merges them. `Object.assign` throws for every refused write, where `Reflect.set` returns `false` for a getter-only member and `true` for a key the object lacks.

- `catch` names the refused step from arguments of the failed call
- Lookups fail with their candidates, and independent writes accumulate every refusal in one cause
- Writes layer caller options first and derived fields last, and no caller option contradicts a derived value
- Evaluated source ends `//# sourceURL=<name>`, and an `Error.prepareStackTrace` restored in `finally` reads lines from frames naming `<name>` and leaves `.stack` text
- Host calls are typed source the compiler checks, with arguments passed as data
- Hosts evaluating text receive a typed function as `String(fn)` swc lowers to their target, and a schema's `Encoded` types the reply
- Caller bodies lower through swc with `sourceMaps: true`, `node:module` `SourceMap.findEntry` maps a `Thrown` line to its body line, and a transform diagnostic fails `Thrown` at its own line

## [06]-[SCOPES]

Host state outlasting a call (listener, preference outside undo, temporary file, socket, runtime) pairs acquisition with release in one `Scope`. Hosts call a listener synchronously on their own thread, and removal takes the function registration received:

```ts
const listened = (target: EventTarget, type: string, fork: (work: Effect.Effect<void, HostError>) => void, work: Effect.Effect<void, HostError>) => {
    const handler = (): void => fork(work);
    return Effect.acquireRelease(Effect.sync(() => target.addEventListener(type, handler)), () => Effect.sync(() => target.removeEventListener(type, handler)));
};
```

`listened` is an `Effect<void, never, Scope>`, and its handler forks `work` through `fork`, the runner a panel receives, and returns to its host at once. Closing the scope removes the handler, and closing the panel's scope interrupts every run. Host listener calls of every shape (`action.addNotificationListener`, `app.addEventListener`) take the same pair. Host rollback restores document state, and a scope restores every state a rollback leaves.

Preferences outside undo take one save and restore pair:

```ts
const restore = (target: object, saved: Record.ReadonlyRecord<string, unknown>) =>
    Effect.validate(Record.toEntries(saved), ([key, value]) => Effect.try({ try: () => Object.assign(target, { [key]: value }), catch: HostError.refused(`restore of ${key}`) }), { discard: true }).pipe(Effect.orDie);
const preserve = <T extends object>(target: T, replacements: Partial<T>, held: readonly string[] = Struct.keys(replacements)) => {
    const refused = HostError.refused(`preferences ${held.join(', ')}`);
    return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(held, (key): readonly [string, unknown] => [key, Reflect.get(target, key)]), catch: refused }), (saved) => restore(target, saved)).pipe(
        Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })),
    );
};
```

`preserve` reads every `held` key through `Reflect.get` before writing any, where `Struct.pick` skips prototype accessors. `held` names keys a later write in the scope sets. `restore` accumulates every refusal under `Effect.validate` and dies through `Effect.orDie`, finalizers holding no error channel.

## [07]-[UNDO_STEPS]

Each host groups changes into one undo step through one call, and every state change runs inside it:

| [INDEX] | [HOST]        | [CALL]                                            | [BODY]                  | [UNDO_STEP]                    |
| :-----: | :------------ | :------------------------------------------------ | :---------------------- | :----------------------------- |
|  [01]   | InDesign      | `app.doScript` under `UndoModes.ENTIRE_SCRIPT`    | Synchronous function    | One step named by its argument |
|  [02]   | Photoshop     | `core.executeAsModal` with `suspendHistory`       | Async function, awaited | One state per open document    |
|  [03]   | Premiere Pro  | `project.lockedAccess`, then `executeTransaction` | Synchronous callbacks   | One entry named by transaction |
|  [04]   | Illustrator   | One `do javascript` or `evalScript` call          | Synchronous source      | One step per call              |
|  [05]   | After Effects | `app.beginUndoGroup(label)`, `app.endUndoGroup()` | Synchronous source      | One step per script or render  |

- One call holds one user-visible change, named in the undo menu
- Bodies hold host calls alone, and network, file, and decode work finishes before a body starts and enters as values
- Evaluated source is `Effect<Effect<A, HostError>, HostError | IllegalArgumentError>`, compiled before a step and run inside by `yield* yield*`
- Compiles pass the scope as one parameter `new Function('{ <keys> }', 'return eval(arguments[1])')` destructures, binding no name source shadows
- Scope keys outside identifiers stay out, and a compiled value other than a function fails `IllegalArgumentError` naming its `typeof`
- Values and typed failures leave a body as data
- Awaited work inside a synchronous body runs after its unit closed, where a later rejection rolls back no change
- Reads needing no undo step run outside these calls where their host allows

## [08]-[ENTRY_POINTS]

Each entry point translates one `Exit` into the form its host expects:

| [INDEX] | [ENTRY]                        | [BEHAVIOR]                                                                |
| :-----: | :----------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Plugin `main` module           | One `serve` call building one `ManagedRuntime`, the link forked inside it |
|  [02]   | `entrypoints.setup`            | One call per plugin with `plugin`, `panels`, and `commands`               |
|  [03]   | Panel `create`, `show`, `hide` | Forks its work and returns inside a 300 ms default timeout                |
|  [04]   | Command `run`, `cancel`        | `run` resolved on success, rejected with the squashed cause               |
|  [05]   | Script                         | One synchronous run answering once through `uxp.script.setResult`         |

Hosts show nothing for a rejected command, and Photoshop logs nothing for it. One registration takes every command and panel and runs each through one runtime that logs its failure:

```ts
import fs from 'adobe:fs';
import uxp, { entrypoints } from 'adobe:uxp';

const manifest = 'plugin:/manifest.json';
const serve = <E, F>({
    link,
    commands = {},
    panels = {},
}: {
    readonly link?: (extension: string) => Layer.Layer<never, E, Uxp | Plugin | Path.Path>;
    readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
    readonly panels?: Record.ReadonlyRecord<string, (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>>;
}): void => {
    const hosted = Uxp.layer({ uxp, fs });
    const reported = <A, X, R>(effect: Effect.Effect<A, X, R>) => effect.pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly<X>), Effect.logError));
    Effect.gen(function* () {
        const [decoded, { setup }] = yield* Effect.all([
            Effect.try({ try: () => fs.readFileSync(manifest, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: manifest, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json))),
            Schema.decodeEffect(Schema.Struct({ setup: Schema.declare(Predicate.isFunction) }))(entrypoints),
        ]);
        const linked = link === undefined ? Layer.empty : Layer.launch(link(decoded.id)).pipe(reported, Effect.forkScoped, Layer.effectDiscard);
        const runtime = ManagedRuntime.make(linked.pipe(Layer.provideMerge(Layer.succeed(Plugin)(decoded)), Layer.provideMerge(hosted)));
        const run = <X>(effect: Effect.Effect<unknown, X, Uxp | Plugin>): Promise<void> => runtime.runPromise(Effect.asVoid(reported(effect)));
        const [handled, shown] = yield* Effect.all([
            Effect.all(
                Record.map(
                    commands,
                    Effect.fnUntraced(function* (command) {
                        const running = yield* FiberHandle.make<void, F>();
                        return { run: () => runtime.runPromise(Effect.flatMap(FiberHandle.run(running, reported(command)), Fiber.join)), cancel: () => runtime.runPromise(FiberHandle.clear(running)) };
                    }),
                ),
            ),
            Effect.all(
                Record.map(
                    panels,
                    Effect.fnUntraced(function* (panel) {
                        const held = yield* ScopedRef.fromAcquire(Effect.void);
                        const fork = (element: HTMLElement) => FiberSet.makeRuntime<Uxp | Plugin, void, never>().pipe(Effect.flatMap((forked) => panel(element, flow(reported, Effect.ignore(), forked))), reported, Effect.forkScoped, Effect.asVoid);
                        return {
                            show: (root?: unknown): Promise<void> => run(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, fork(element)))),
                            hide: (): Promise<void> => run(ScopedRef.set(held, Effect.void)),
                        };
                    }),
                ),
            ),
        ]).pipe(Scope.provide(runtime.scope));
        yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void), destroy: () => runtime.dispose() }, commands: handled, panels: shown } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
    }).pipe(Effect.tapCause(Effect.logError), Effect.provide(hosted), Effect.runFork);
};
```

`Uxp` is the service holding `uxp` and `fs`, `Uxp.layer` merges it with the host `Clock` layer, `Logger.LogToStderr` set `true`, and Effect's `Path.layer`: hosts record `console.error` at error level in their log file where Effect's default logger writes `console.log`, and an entry's top-level `runFork` or `runSyncExit` logs under the same reference by providing the layer outside its `tapCause`. `Plugin` holds the manifest `Manifest.json` decodes from the plugin folder, and a failed read fails `PlatformError` as Effect's `FileSystem.readFileString` does. `link` takes the manifest `id` as its extension, and its launch forks inside the runtime's layer, where a failed or dropped link logs while commands and panels run. `Layer.provideMerge` keeps the host `Clock` in runtime context, where `runPromise` otherwise reads Effect's default clock. One `Effect.all` under `runtime.scope` allocates each command's `FiberHandle` and each panel's `ScopedRef`. Reruns interrupt the running command, and `cancel` clears it. `@adobe-uxp-types/uxp` declares `cancel` for future use. Panels receive `fork`, a `FiberSet.makeRuntime` runner in the shown scope for synchronous host callbacks that logs each run's failure once and ignores it. `show` replaces the shown scope and `hide` closes it, interrupting every fork, and `dispose` closes every scope the layers opened, where `Effect.forkDetach` fibers outlive it. `@adobe-uxp-types/uxp` merges `setup`'s parameter with `entrypoints`, failing records without `getCommand`, `getPanel`, `setup`, and `_pluginInfo`. Decoded `setup` runs through `call` with `entrypoints` as receiver, its throw logs as a defect, and `runFork` calls it before `main` returns.

- `setup` throws for a `plugin` record without `create`, for a command or panel key its manifest lacks, and after one successful call
- Panel handlers receive their root node: `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, `destroy(rootNode)`
- Hosts can run `main` at launch with no panel shown, and `main` builds every state a panel reads
- `show` renders from state `main` built, and panel visibility reaches `document` as a `uxpcommand` event
- Panels act through the served contract's handlers with payloads its request classes build, and each write names its undo step from the payload
- Panels hold host controls alone, and actions redraw from returned state and disable controls when host state offers nothing to act on
- Controls hold typed values as `[label, value]` tuples, reading `Array.get(options, element.selectedIndex)` and showing one by `selectedIndex`

## [09]-[MANIFESTS]

One manifest serves one host, and `host` arrays with any `app` decode for development and Adobe's bundled plugins. Host package contracts state `host` (`app` of `ID`, `PS`, `premierepro`, `ame`, or `aftereffects`, with `minVersion`) and `requiredPermissions` once, and builds derive every other key: `id`, `name`, and `version` from product identity, `manifestVersion` 6 where `addon` appears else 5, `main` from the entry, `host.data.loadEvent` `startup` to dial at launch, `network.domains` from the dial origin, and `icons` from emitted files. Build config parameters omit derived keys. Hosts enforce each `requiredPermissions` key:

| [INDEX] | [KEY]                                   | [GRANTS]                                                 | [WITHOUT]                        |
| :-----: | :-------------------------------------- | :------------------------------------------------------- | :------------------------------- |
|  [01]   | `network.domains`                       | Dial and fetch to each listed origin or `"all"`          | Dial throws a manifest denial    |
|  [02]   | `localFileSystem: "fullAccess"`         | Access outside plugin storage                            | Defaults to plugin storage       |
|  [03]   | `allowCodeGenerationFromStrings`        | `eval`, `new Function`, inline handlers                  | InDesign v5 runs `new Function`  |
|  [04]   | `featureFlags.unhandledRejection`       | `unhandledrejection` events                              | Rejection logged, no caller told |
|  [05]   | `launchProcess`                         | `shell.openPath`, `shell.openExternal`                   | Both calls throw a denial        |
|  [06]   | `clipboard: "read"` or `"readAndWrite"` | System clipboard reads, or reads and writes              | No clipboard access              |
|  [07]   | `ipc.enablePluginCommunication`         | Calls to other installed plugins through `pluginManager` | Defaults to `false`              |
|  [08]   | `webview` `allow: "yes"` with `domains` | Modal dialog WebViews for listed domains or `"all"`      | No WebView                       |
|  [09]   | `enableUserInfo`                        | `userInfo.userId()` returns the Creative Cloud user GUID | No user GUID                     |
|  [10]   | `enableAddon`                           | Native `.uxpaddon` loads                                 | No addon load                    |
|  [11]   | `featureFlags.enableAlerts`             | `alert`, `prompt`, and `confirm` dialogs                 | No dialogs                       |

- `entrypoints` rows name a `panel` or `command` `id` with `label.default`, `description`, `icon`, and `hostUIContext`, and hosts refuse a manifest naming neither `main` nor an entrypoint
- Entrypoint ids and labels sit in one `as const` table the build maps into `entrypoints`, and `main` checks handler records with `satisfies Record<keyof typeof <table>, unknown>`
- Icon rows state a base `path` with `scale`, and hosts resolve `<base>@<scale>x.<extension>` for each factor
- Panel icons are `toolbar` PNGs at 1x and 2x, plugin icons `pluginList` SVGs, each per theme group
- InDesign paints an inherited `<svg>` fill black, and plugin icons hold `fill` on each element
- `plugin-temp:` reads and writes the folder `getTemporaryFolder()` returns under every permission level, and `plugin:` is read-only
- Storage providers hold no `unlinkSync`, and a temporary file's native storage entry deletes it through `delete()`
- Storage entries preserve native `url` schemes for file reads and expose `nativePath` for host-native writes
- Typings declare the plugin `manifest` as `never`, and plugins decode `plugin:/manifest.json` through a schema
- `adobe:uxp` exports `Entry` as a type alone, and `Uxp.Entry` decodes a storage entry by its `isEntry: true` field, its `url` typed `string` and returned as a string or `URL`
- `domains` entries name a host with a top-level domain or `localhost` (`ws://localhost:<port>`), and an IP literal entry is discarded
- `shell.openPath` and `shell.openExternal` open a path or URL behind a consent dialog, with no arguments or process output
- Plugin data sits under `UXP/PluginsStorage/<product>/<major>/External/<pluginId>/PluginData`
- Native addons load in Photoshop and Premiere alone
