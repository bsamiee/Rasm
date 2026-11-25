---
name: coding
description: "Use when writing or reviewing code that runs in an Adobe UXP host, covering plugin runtime and host boundaries"
---

# [UXP_CODING]

UXP runs each plugin and script in one V8 context inside an Adobe host.

Ranges and enumerations come from the host's declared object model, localized names from the running host, extents from the measured item, unit ratios from their standard, names of created files from their target, and every other value from the caller.

[REFERENCES]:
- [01]-[INDESIGN](references/indesign.md): InDesign behavior its typings leave unstated, for code calling its synchronous DOM
- [02]-[PHOTOSHOP](references/photoshop.md): Photoshop behavior its typings leave unstated, for descriptor and DOM calls
- [03]-[PREMIERE](references/premiere.md): Premiere Pro and Media Encoder behavior their typings and manifests leave unstated, for edits as actions
- [04]-[EXTENDSCRIPT](references/extendscript.md): Illustrator behavior its declared sources leave unstated, for ES3 source `do javascript` runs

## [01]-[CONTEXTS]

Each host runs one newest runtime, and a dictionary command runs source with no linked plugin:

| [INDEX] | [HOST]        | [RUNTIME]                           | [MODULE]       | [UNLINKED_SOURCE]                   |
| :-----: | :------------ | :---------------------------------- | :------------- | :---------------------------------- |
|  [01]   | InDesign      | UXP                                 | `indesign`     | `do script` in `uxpscript`          |
|  [02]   | Photoshop     | UXP                                 | `photoshop`    | `do javascript`, ExtendScript alone |
|  [03]   | Premiere Pro  | UXP                                 | `premierepro`  | None                                |
|  [04]   | Media Encoder | UXP                                 | `mediaencoder` | None                                |
|  [05]   | After Effects | UXP                                 | `aftereffects` | `DoScript`, ExtendScript alone      |
|  [06]   | Illustrator   | UXP behind `EnableThirdPartyUXP`    | `illustrator`  | `do javascript`, ExtendScript alone |
|  [07]   | Acrobat       | Acrobat JavaScript, SpiderMonkey 24 | None           | `do script`                         |

- CEP turns off by default in December 2028 and leaves new host releases in December 2029
- `require` resolves the running host's DOM module alone, `uxp.host.name` names that host, and cross-host code takes host calls as arguments
- Illustrator checks `EnableThirdPartyUXP` of `AIFeatures.cfg` beside its app (`enabled="1"`, `configurableInRelease="1"`) for plugins UXP developer mode loads
- Illustrator reads `UXPExtensionPath.json` beside its app, each string value naming a folder it scans as `<folder>/<plugin>/manifest.json`
- Illustrator parses a plugin that marker finds and runs no `main` of it at launch, and reads no `PluginsInfo` registry
- Illustrator runs a third-party plugin's `main` on a UXP developer mode load alone, and refuses a manifest naming no `panel` or `command` entry point
- Illustrator lists each command under Window > Extensions, one with `hostUIContext.hideFromMenu` included
- Illustrator's `uxp.host` throws at its first read and holds no member after, and `app.path` names the folder holding the bundle `app.name` names
- Illustrator returns a new object at each read of one document, layer, artboard, or page item, and code compares their keys, never the objects
- Illustrator page items hold `parentDocument` and a `parent` layer, layers and artboards hold no `parent`, and `getPageItemFromUuid` finds an item by `uuid`
- Illustrator `exportFile` takes a UXP storage `File` entry and refuses a path or URL string with error 1242 or 1220
- Illustrator serves `illustrator-dom.js` to plugins: `require('illustrator')` returns `{ app }`, the specifier inferred from the string beside its registration
- Illustrator enums and creatable classes sit on `globalThis`, every call is synchronous, and enum values are strings
- After Effects module holds `app` and DOM classes, calls are synchronous, collections run 1 to `length`, `constructor.name` names an object's class
- After Effects `saveFrameToPng(seconds, path)` returns a `DeferredCall` holding `wait()`, `onComplete`, and `onError` and no `then`, and the PNG exists when the call returns
- After Effects `resolutionFactor` takes an integer downsample `[x, y]` from 1 to 99, and `uxp.host.applicationPath` names the running bundle
- Acrobat source runs privileged with `this` naming the front document, or the `Root` object when none is open, and `global` members persist between calls
- Acrobat JavaScript holds `JSON`, `util`, `eval`, `new Function`, and arrow functions, and no `let`, `$`, `File`, or `reflect`
- Acrobat answers an uncaught throw or syntax error with -1708 and no text, and source catches its own throws with `lineNumber` as `line`
- Acrobat holds no global dialog switch, and a replaced `app.alert` still opens the host alert

Context decides what code holds across calls:

| [INDEX] | [CONTEXT] | [LIFETIME]   | [PERMISSIONS]                  | [HOLDS]                                                     |
| :-----: | :-------- | :----------- | :----------------------------- | :---------------------------------------------------------- |
|  [01]   | Plugin    | Host session | Manifest `requiredPermissions` | Listeners, sockets, timers, `localStorage`, `secureStorage` |
|  [02]   | Script    | One run      | Fixed by host                  | Values of that run, `sessionStorage` that ends with it      |

- Work that finishes after its call returns belongs to a plugin
- State a script needs in a later run is host state: a document, a file, or a stored value

## [02]-[RUNTIME]

Differences from a browser or Node hold in InDesign, Photoshop, and Premiere Pro, each with one owner:

| [INDEX] | [DIFFERENCE]                                  | [OWNER]   | [FORM]                                 |
| :-----: | :-------------------------------------------- | :-------- | :------------------------------------- |
|  [01]   | Member of another type (`hrtime.bigint`)      | Runtime   | `Clock` layer over `performance.now()` |
|  [02]   | Absent global or module (`Worker`, `node:*`)  | Bundle    | Dependencies reading one stay out      |
|  [03]   | Module typed as Node's (`fs`, `path`)         | Read site | Value decoded where read               |
|  [04]   | Feature with a missing part (`Response.body`) | Call site | Whole-body read, flag on whole pattern |

Effect's default clock expects bigint nanoseconds, and UXP's `process.hrtime.bigint` returns a number. Host `Clock` layer reads monotonic nanoseconds from `performance.now()`, wall nanoseconds from each `currentTimeMillis` read, and keeps native sleep. Offsets computed once skip the skew correction Effect's wall clock performs. Host callbacks running effects receive caller services through `run*With`.

- Hosts embed the V8 of Chrome 118, which holds `Object.groupBy` and regex flag `v` and lacks `Promise.withResolvers`, `Set` methods, `Array.fromAsync`, and `using`
- `structuredClone`, `Buffer`, `MessageChannel`, and `Worker` are absent in every host, and `require` of `util`, `buffer`, or a `node:` name throws
- `fs` holds synchronous reads, writes, and `lstatSync`, and no `existsSync`, `appendFileSync`, `mkdirSync`, or `statSync`
- `writeFileSync` reads its receiver, and code calls it on the module's default import
- `path` is a function with `resolve` and `basename`
- Effect's POSIX `Path` service converts paths and `file:` URLs, `toFileUrl` percent-encoding `%`, `#`, `?`, and space a `file:${path}` template breaks on
- Plugin `main` is a script that renders into `document.body`, and UXP never runs `<script type="module">`
- Plugin scripts hold no top-level `await`, and dependencies instantiating WebAssembly through one at import stay out

## [03]-[TYPINGS]

Typings declare a host module under its `require` name, under an `adobe:` scheme, or as a type with no module:

| [INDEX] | [MODULES]                 | [PACKAGE]              | [DECLARES]                                                       |
| :-----: | :------------------------ | :--------------------- | :--------------------------------------------------------------- |
|  [01]   | `uxp`, `fs`, `os`, `path` | `@adobe-uxp-types/uxp` | `uxp` by name, each module under `adobe:` from `with-protocol`   |
|  [02]   | `photoshop`               | `@types/photoshop`     | `photoshop` and `photoshop/dom/*` by path, no `adobe:` module    |
|  [03]   | `indesign`                | `indesign-uxp-types`   | `indesign` by name, `adobe:indesign` from `flat` through `paths` |
|  [04]   | `premierepro`             | `@adobe/premierepro`   | Type `premierepro` of the module object, no module               |

- Diagnostics on a call the object model states name a typings gap, and every other diagnostic names a defect in the call
- Typings can state another enumeration value than the host holds, and code reads enumeration members from the module

Each gap closes where its value is read, with types the package or a schema states:

| [INDEX] | [GAP]                                              | [FORM]                                                                   |
| :-----: | :------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | Member, value, or failure typings omit or misstate | Host value decoded through a `Schema` at its read                        |
|  [02]   | Module object typed with no module                 | Module passed as an argument of the package's type                       |
|  [03]   | Same gap in ExtendScript without `Schema`          | Assertion to the host's shape, suppression naming gap                    |
|  [04]   | Global no package declares (`ExportOptionsPNG24`)  | Interface, `globalThis` decoded through a struct of its function members |

Typings for `fs` declare bytes as `Buffer` and a failure's `code` as a string, where the host returns an `ArrayBuffer` and a numeric `code`:

```ts
import { readFileSync } from 'adobe:fs';

const bytes = (path: string) =>
    Effect.try({ try: () => Option.some(new Uint8Array(readFileSync(path))), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFile', pathOrDescriptor: path, cause }) }).pipe(Effect.catchIf(({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause), () => Effect.succeedNone));
```

`new Uint8Array` accepts both representations, and `PlatformError` holds the host's thrown value as `cause`.

## [04]-[OBJECT_MODELS]

Object models come from each host's declared source, never from reflection over live objects or a hand list:

| [INDEX] | [HOST]        | [SOURCE]                                                                                           |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | InDesign      | `indesign-uxp-types`                                                                               |
|  [02]   | Photoshop     | `@types/photoshop`                                                                                 |
|  [03]   | Premiere Pro  | `@adobe/premierepro`, installed `ScriptingAPIManifest.json` as cross-check                         |
|  [04]   | Media Encoder | Installed `ScriptingAPIManifest.json`, AdobeDocs `uxp-media-encoder`                               |
|  [05]   | After Effects | AdobeDocs `uxp-after-effects`, which states no enumerations                                        |
|  [06]   | Illustrator   | Newest `Illustrator*/omv.xml` under `/Library/Application Support/Adobe/Scripting Dictionaries CC` |
|  [07]   | Acrobat       | AdobeDocs `acrobat-pdfl-sdk` `jsapiref`, `acrohelp` parameter help                                 |

- InDesign's bundled assistant `indesign.d.ts` is internal, and After Effects' bundled `@dva/types` is confidential
- Illustrator's `$.dictionary` reads `null`
- Media Encoder's `api_config.json` is empty, and its manifest names each member with type, optional parameters, and async flag
- `acrohelp` as an argument to a native Acrobat method answers a `HelpError` with `extMessage` listing parameters as `name: type`, optional ones in brackets
- Native scripting dictionaries declare Apple event commands and parameter types, and a script command's parameters select its language and undo mode
- InDesign `show`, `reveal in system`, and `reveal in Bridge`, Acrobat `bring to front`, and Premiere Pro `editoriginal` dictionary commands activate their host
- Typings can predate the installed build and rename parameters a host names by reserved words, and renamed parameters call correctly by position
- Host errors name a refused parameter with its expected type and range, and whether a member applies now
- `OSACopyScriptingDefinitionFromURL` launches an app with a dynamic dictionary (InDesign, Illustrator), and its XML can contain XInclude references

## [05]-[HOST_ERRORS]

Hosts throw `Error` values with text in the user's locale, and return some failures as values.

`HostError` is the package error for failures no host error (`PlatformError`, `HttpClientError`) states, with refused step as `subject`, host's thrown value or returned failure as `cause`, and one reason per caller response:

| [INDEX] | [REASON]      | [HOST_OUTCOME]                                                     |
| :-----: | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Refused`     | Call refused or failure returned as a value                        |
|  [02]   | `Unavailable` | Host or bridge serves no call (modal scope held, no linked plugin) |
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
- Evaluated source ends `//# sourceURL=<name>`, one name in plugins and scripts, and an `Error.prepareStackTrace` held in a scope reads lines from frames naming `<name>` and leaves `.stack` text
- Line formatters install before a body runs, as a host reads the stack of a rejection its body throws before any `catch` sees it
- `HostError` and `Unmatched` state their `ErrorReporter.severity`, and a request failure answered to its caller logs at info, `Unavailable` at warn
- Host calls are typed source the compiler checks, with arguments passed as data
- Hosts evaluating text receive a typed function as `String(fn)` swc lowers to their target, and a schema's `Encoded` types the reply
- Caller bodies lower through swc with `sourceMaps: true`, `node:module` `SourceMap.findEntry` maps a `Thrown` line to its body line, and a transform diagnostic fails `Thrown` at its own line

## [06]-[SCOPES]

Host state outlasting a call (listener, preference outside undo, temporary file, socket, runtime) pairs acquisition with release in one `Scope`. Hosts call a listener synchronously on their own thread, and removal takes the function registration received:

```ts
const listen = (target: EventTarget, type: string, fork: (work: Effect.Effect<void, HostError>) => void, work: Effect.Effect<void, HostError>) => {
    const handler = (): void => fork(work);
    return Effect.acquireRelease(Effect.sync(() => target.addEventListener(type, handler)), () => Effect.sync(() => target.removeEventListener(type, handler)));
};
```

`listen` returns an `Effect<void, never, Scope>`, and its handler forks `work` through `fork`, the runner a panel receives, and returns to its host at once. Closing the scope removes the handler, and closing the panel's scope interrupts every run. Host listener calls of every shape (`action.addNotificationListener`, `theme.onUpdated.addListener`) take the same pair, and InDesign's `app.addEventListener` takes none, as its release deadlocks an unload. Host rollback restores document state, and a scope restores every state a rollback leaves.

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
|  [04]   | Illustrator   | One `do javascript` call                          | Synchronous source      | One step per call              |
|  [05]   | After Effects | `app.beginUndoGroup(label)`, `app.endUndoGroup()` | Synchronous source      | One step per script or render  |

- One call holds one user-visible change, named in the undo menu
- Bodies hold host calls alone, and network, file, and decode work finishes before a body starts and enters as values
- Evaluated source compiles before a step into `ran`, run inside it, and `settled`, which awaits its value after it, under one scope
- After Effects declares no undo or rollback member, and a failed body leaves its undo group as one step
- Compiles pass the scope as one parameter `new Function('{ <keys> }', 'return eval(arguments[1])')` destructures, binding no name source shadows
- Scope keys outside identifiers fail the compile as `Thrown` with the host's `SyntaxError`, and a compiled value other than a function fails `IllegalArgumentError` naming its `typeof`
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
    readonly link?: (plugin: string) => Layer.Layer<never, E, Uxp | Plugin | Path.Path | Socket.WebSocketConstructor>;
    readonly commands?: Record.ReadonlyRecord<string, Effect.Effect<void, F, Uxp | Plugin>>;
    readonly panels?: Record.ReadonlyRecord<string, (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Uxp | Plugin>) => void) => Effect.Effect<void, F, Scope.Scope | Uxp | Plugin>>;
}): void => {
    const hosted = Uxp.layer({ uxp, fs });
    const reported = <A, X, R>(effect: Effect.Effect<A, X, R>) => effect.pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly<X>), Effect.logError));
    Effect.gen(function* () {
        const [decoded, { setup }] = yield* Effect.all([
            Effect.try({ try: () => fs.readFileSync(manifest, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: manifest, cause }) }).pipe(Effect.flatMap(Schema.decodeEffect(UxpManifest.Identity))),
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
        yield* Effect.sync(() => setup.call(entrypoints, { plugin: { create: () => runtime.runPromise(Effect.void) }, commands: handled, panels: shown } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>));
    }).pipe(Effect.tapCause(Effect.logError), Effect.provide(hosted), Effect.runFork);
};
```

`Uxp` is the service holding `uxp` and `fs`, `Uxp.layer` merges it with the host `Clock` layer, `Logger.LogToStderr` set `true`, Effect's `Path.layer`, and `Socket.layerWebSocketConstructorGlobal`, the constructor a link dials through: hosts record `console.error` at error level in their log file where Effect's default logger writes `console.log`, and an entry's top-level `runFork` or `runSyncExit` logs under the same reference by providing the layer outside its `tapCause`. `Plugin` holds the plugin id and host app `UxpManifest.Identity` decodes from the plugin folder's manifest, the fields a running plugin reads, so a manifest key the model lacks never stops a load, and a failed read fails `PlatformError` as Effect's `FileSystem.readFileString` does. `link` takes the manifest `id` as `plugin`, and its launch forks inside the runtime's layer, where a failed or dropped link logs while commands and panels run. `Layer.provideMerge` keeps the host `Clock` in runtime context, where `runPromise` otherwise reads Effect's default clock. One `Effect.all` under `runtime.scope` allocates each command's `FiberHandle` and each panel's `ScopedRef`. Reruns interrupt the running command, and `cancel` clears it. `@adobe-uxp-types/uxp` declares `cancel` for future use. Panels receive `fork`, a `FiberSet.makeRuntime` runner in the shown scope for synchronous host callbacks that logs each run's failure once and ignores it. `show` replaces the shown scope and `hide` closes it, interrupting every fork. Plugin records declare no `destroy`, and the host ends every scope the layers opened with the plugin's context: InDesign calls panel handlers and plugin `destroy` during unload with its main thread waiting on them, where a handler or scope release reaching its DOM deadlocks it. `@adobe-uxp-types/uxp` merges `setup`'s parameter with `entrypoints`, failing records without `getCommand`, `getPanel`, `setup`, and `_pluginInfo`. Decoded `setup` runs through `call` with `entrypoints` as receiver, its throw logs as a defect, and `runFork` calls it before `main` returns.

- `setup` throws for a `plugin` record without `create`, for a command or panel key its manifest lacks, and after one successful call
- `serve` logs each `unhandledrejection` event at error, as `@rasm/uxp/vite` writes `featureFlags.unhandledRejection` into every built manifest
- Hosts log an exception leaving a callback at error under any flag, and `featureFlags.uncaughtException` delivers each as an `error` event too
- Panel handlers receive their root node: `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, `destroy(rootNode)`
- Hosts can run `main` at launch with no panel shown, and `main` builds every state a panel reads
- `show` renders from state `main` built, and panel visibility reaches `document` as a `uxpcommand` event
- Panels act through the served contract's handlers with payloads its request classes build, and each write names its undo step from the payload
- Panels hold host controls alone, and actions redraw from returned state and disable controls when host state offers nothing to act on
- `document.theme` holds `getCurrent` and `onUpdated` in every host, `Plugin.theme` streams it, and each panel root takes its host's theme fill of `UxpManifest.fills` as text color
- InDesign holds no `--uxp-host-*` value and no `matchMedia`, and paints text black
- Controls hold typed values as `[label, value]` tuples, reading `Array.get(options, element.selectedIndex)` and showing one by `selectedIndex`

## [09]-[MANIFESTS]

One manifest serves one host, `host.app` naming `ID`, `PS`, `premierepro`, `ame`, `aftereffects`, or `AI`, and `host.data.loadEvent` `startup` loads a plugin at host launch. Hosts enforce each `requiredPermissions` and `featureFlags` key:

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

- `entrypoints` rows name a `panel` or `command` `id` with `label.default`, `description`, and `hostUIContext`, and hosts refuse a manifest naming neither `main` nor an entrypoint
- Icon rows state a base `path` with `scale`, and hosts resolve `<base>@<scale>x.<extension>` for each factor
- Panel icons are 23-pixel `toolbar` PNGs and plugin icons 24-pixel `pluginList` PNGs, each at 1x and 2x
- Icons take one row per theme fill, `medium` in the dark group as Adobe's manifests place it
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

## [10]-[DEBUGGING]

Debugger routes need UXP developer mode, `{"developer": true}` in `/Library/Application Support/Adobe/UXP/Developer/settings.json`, and a running host:

| [INDEX] | [HOST]                  | [DEBUG]                                                | [TEST]                                 |
| :-----: | :---------------------- | :----------------------------------------------------- | :------------------------------------- |
|  [01]   | Photoshop               | UXP Debugger, `debugger` MCP, Atelier `execute_script` | `@bubblydoo/vitest-pool-uxp` `uxpPool` |
|  [02]   | InDesign                | UXP Debugger, `debugger` MCP, Atelier `execute_script` | Atelier `execute_script`               |
|  [03]   | Premiere, Media Encoder | UXP Debugger, `debugger` MCP, Atelier `diagnostics`    | None                                   |
|  [04]   | After Effects           | UXP Debugger, `debugger` MCP, Atelier `execute_script` | Atelier `execute_script`               |
|  [05]   | Illustrator             | UXP Debugger, `debugger` MCP, Atelier `execute_script` | Atelier `execute_script`               |

UXP Debugger (`jaroslavbereza.uxpdebugger`) serves UXP Developer Tool's broker from a VS Code window on `127.0.0.1:14001`, and a host launched while it listens connects to it. Hooks under `/__uxp_debugger__/hooks` act on a manifest registered in its panel, and built manifests sit at `.artifacts/vite/<project>/manifest.json`:

```bash
xh -b POST 127.0.0.1:14001/__uxp_debugger__/hooks/<load|refresh|reload|unload> manifestPath=<manifest>
xh -b 127.0.0.1:14001/__uxp_debugger__/hooks/watch manifestPath==<manifest>
```

- `load`, `refresh`, and `reload` answer `sessions`, `refresh` keeps a session and its debugger, and each runs new build output with no host relaunch
- `watch` answers 404 for an unregistered manifest, which registers once per machine through the panel's `Add plugin…` menu
- `AXPress` on the `UXP Devtools` activity-bar tab toggles its side bar, and the tab reads `value` 1 while the panel shows
- Menu item `Currently opened manifest.json` registers the active editor's file
- Panel title buttons `UXP: Stop UXP Devtools` and `UXP: Start UXP Devtools` close and open the broker's port
- UXP Debugger stays the broker, as Photoshop and InDesign stall in launch while `@adobe-fixed-uxp/uxp-devtools-cli` `uxp service start` listens on the port, and its `plugin reload`, `unload`, and `debug` reach no host
- `debugger` MCP attaches through `create_debug_session` with `language` `javascript`, then `attach_to_process` with `stopOnEntry` `false`
- `attach_to_process` `adapterConfig` holds `websocketAddress` `ws://127.0.0.1:14001/socket/cdt/<session>`, `sourceMaps` `true`, and `cwd` of the build folder
- `set_breakpoint` takes a TypeScript source path, as `@rasm/uxp/vite` writes `main.js.map` beside each bundle, and `detach_from_process` leaves its host running
- Hosts hold one debugger connection per session, and a second connection replaces the first, VS Code's own included
- Paused plugins block their host's JavaScript thread, and an Atelier call reaching a breakpoint answers after `continue_execution`
- Attach, a TypeScript breakpoint, a read of its locals, and detach hold in Photoshop, InDesign, After Effects, Premiere Pro, and Media Encoder
- Loads over an installed plugin of one id leave the installed instance running without its host module, its link open, and no `destroy` call before quit
- Debugger launches empty `plugins` of `PluginsInfo/v1/<host>.json` while their host is quit and restore the file after its quit
- Hosts with no `PluginsInfo/v1/<host>.json` scan `Plugins/External` and log every other host's folder at error
- Illustrator answers `Plugin/load` for a manifest with an entry point and leaves one with none unanswered past the broker's 5000 ms bound
- Attach, a `main.js` breakpoint in a command's `run`, a read of its locals, and detach hold in Illustrator, which has no row in UXP Debugger's host list
- Illustrator breakpoints in `do javascript` source belong to ExtendScript Debugger (`adobe.extendscript-debug`) under preference `AllowVSCodeDebuggerConnections`
- CDP `Debugger.setPauseOnExceptions` `uncaught` with `Runtime.exceptionThrown` and `Runtime.consoleAPICalled` lists a session's throws and console errors
- InDesign unload calls panel handlers on the plugin thread (`PluginRuntime::forEachPanel`) with its main thread waiting in `PluginRuntime::destroy`, and a DOM call there deadlocks both, which `sample <pid> 1` shows
- Plugin sessions answer CDP at `ws://127.0.0.1:14001/socket/cdt/<session>`: `Runtime.evaluate` takes the `contextId` of `Runtime.executionContextCreated`, and `Debugger.setBreakpointByUrl` a `main.js` line
- VS Code logs each connected host's UXP console to `logs/<run>/window1/exthost/JaroslavBereza.uxpdebugger/UXP – <host> <version>.log`

- Atelier `execute_script` reaches Photoshop and After Effects through linked Atelier plugins, and InDesign through `do script` with `language uxpscript`
- Atelier `execute_script` reaches Illustrator through its Atelier plugin while one is loaded and linked, else through `do javascript`
- Premiere and Media Encoder declare no script command
- Atelier `diagnostics` reads a host's reports and UXP logs whether the host runs or not
- `uxp-cli` and `vitest-pool-uxp` find Photoshop alone and throw `Photoshop is not open` without it
- `uxp-cli create-cdp-url` loads the folder as a plugin session and prints a `ws://` URL for any CDP client, `@bubblydoo/vitest-pool-cdp` included
- `uxp-cli open-devtools` brings Chrome forward through `open -a`, and agents take `create-cdp-url`
- `vitest-pool-uxp` runs with `maxWorkers: 1`, `isolate: false`, `fileParallelism: false`, the settings its README requires
- `nx install` writes the plugin folder and its `PluginsInfo` row, and a running host loads the new code at its next launch
