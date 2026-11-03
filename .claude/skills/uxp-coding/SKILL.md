---
name: uxp-coding
description: "Use when writing or reviewing code that runs in an Adobe UXP host, covering plugin runtime and host boundaries"
---

# [UXP_CODING]

UXP runs each plugin and script in one V8 context inside an Adobe host.

Ranges, enumerations, and localized names come from the running host, extents from the measured item, unit ratios from their standard, names of created files from their target, and every other value from the caller.

[REFERENCES]:
- [01]-[INDESIGN](references/indesign.md): InDesign's synchronous DOM: object model, transactions, GREP, export, and plugin load
- [02]-[PHOTOSHOP](references/photoshop.md): Photoshop's modal scopes, history, descriptors, DOM members, events, export, fonts, and plugin load
- [03]-[PREMIERE](references/premiere.md): Premiere Pro's members, reads, transactions, events, projects, export, and Media Encoder
- [04]-[ILLUSTRATOR](references/illustrator.md): Illustrator's ExtendScript calls, object model, undo steps, export, and plugin load
- [05]-[TRANSPORT](references/transport.md): Typed calls a plugin serves over a socket it dials

## [01]-[CONTEXTS]

Host modules differ in DOM call form:

| [INDEX] | [HOST]       | [MODULE]      | [DOM_CALLS]                            |
| :-----: | :----------- | :------------ | :------------------------------------- |
|  [01]   | InDesign     | `indesign`    | Synchronous, storage getters async     |
|  [02]   | Photoshop    | `photoshop`   | Promises inside a modal scope          |
|  [03]   | Premiere Pro | `premierepro` | Async getters, action objects          |
|  [04]   | Illustrator  | None          | ExtendScript through `do javascript`   |
|  [05]   | Acrobat      | None          | Acrobat JavaScript through `do script` |

- `require` resolves the running host's DOM module alone, `uxp.host.name` names that host, and cross-host code takes host calls as arguments
- Illustrator and Acrobat without a link run source through Atelier `execute_script`, which sends their dictionary script command
- Acrobat source runs privileged with `this` naming the front document, and `global` members persist between calls
- Acrobat JavaScript holds `JSON`, `util`, `eval`, `new Function`, and arrow functions, and no `let`, `$`, `File`, or `reflect`
- Acrobat answers an uncaught throw or a syntax error with -1708 and no text, and source catches its own throws

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

Bundles keep one form:
- Output is one CommonJS script with each host module left as a `require` call by module name, and an unresolved dependency throws at its first read
- Resolution uses neutral conditions: browser conditions select builds that construct absent globals at load, Node conditions emit `node:` requires
- Plugin `main` is a script that renders into `document.body`, and UXP never runs `<script type="module">`

## [03]-[TYPINGS]

Typings declare a host module under its `require` name, under an `adobe:` scheme, or as a type with no module:

| [INDEX] | [MODULES]                 | [PACKAGE]                    | [DECLARES]                                                       |
| :-----: | :------------------------ | :--------------------------- | :--------------------------------------------------------------- |
|  [01]   | `uxp`, `fs`, `os`, `path` | `@adobe-uxp-types/uxp`       | `uxp` by name, each module under `adobe:` from `with-protocol`   |
|  [02]   | `photoshop`               | `@adobe-uxp-types/photoshop` | `photoshop` by name, `adobe:photoshop` from `with-protocol`      |
|  [03]   | `photoshop`               | `@types/photoshop`           | Adobe's declarations, `photoshop` and `photoshop/dom/*` by name  |
|  [04]   | `indesign`                | `indesign-uxp-types`         | `indesign` by name, `adobe:indesign` from `flat` through `paths` |
|  [05]   | `premierepro`             | `@adobe/premierepro`         | Type `premierepro` of the module object, no module               |
|  [06]   | Illustrator ExtendScript  | `types-for-adobe`            | Global classes per host folder, no module                        |

- Imports name host modules under `adobe:`, a prefix that separates `fs`, `os`, and `path` from Node's
- `types` lists `node` beside each `with-protocol` entry, and `node` types `require`, `process`, and `adobe:fs` members
- `paths` entries resolve against the declaring config's folder, and a shared config writes `${configDir}/node_modules/...` to resolve in each extending project
- Diagnostics on a call the object model states name a typings gap, and every other diagnostic names a defect in the call
- Typings can state another enumeration value than the host holds, and code reads enumeration members from the module

Each gap closes where its value is read, with types the package or a schema states:

| [INDEX] | [GAP]                                              | [FORM]                                             |
| :-----: | :------------------------------------------------- | :------------------------------------------------- |
|  [01]   | Member, value, or failure typings omit or misstate | Host value decoded through a `Schema` at its read  |
|  [02]   | Module object typed with no module                 | Module passed as an argument of the package's type |

Typings for `fs` declare bytes as `Buffer` and a failure's `code` as a string, where the host returns an `ArrayBuffer` and a numeric `code`:

```ts
import { readFileSync } from 'adobe:fs';

const bytes = (path: string) =>
    Effect.try({ try: () => Option.some(new Uint8Array(readFileSync(path))), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFile', pathOrDescriptor: path, cause }) }).pipe(Effect.catchIf(({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause), () => Effect.succeedNone));
```

`new Uint8Array` accepts both representations, and `PlatformError` holds the host's thrown value as `cause`.

## [04]-[OBJECT_MODELS]

Object model facts come from the owner that states them for the installed build:

| [INDEX] | [HOST]       | [OWNER]                                             | [READ]                                          |
| :-----: | :----------- | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | InDesign     | `$.dictionary` of the running host                  | `getClass(<name>).toXML()` rows                 |
|  [02]   | Photoshop    | `app.js` of the installed build, `@types/photoshop` | Syntax-tree read joined with its declaration    |
|  [03]   | Premiere Pro | `ScriptingAPIManifest.json` and `api_config.json`   | `jq` read of the classes the allow-list exposes |
|  [04]   | Illustrator  | `reflect` of the running host, `types-for-adobe`    | `reflect.properties` rows, methods by name      |
|  [05]   | Acrobat      | `acrohelp` argument to a native method              | `HelpError.extMessage` per parameter            |

- Acrobat's `extMessage` lists parameters as `name: type`, optional ones in brackets
- Native scripting dictionaries declare Apple event commands and parameter types, and a script command's parameters select its language and undo mode
- InDesign `show`, `reveal in system`, and `reveal in Bridge`, Acrobat `bring to front`, and Premiere Pro `editoriginal` dictionary commands activate their host
- Typings can predate the installed build and rename parameters a host names by reserved words, and renamed parameters call correctly by position
- Rejected calls complete each model: host errors name a parameter with its expected type and range, and whether a member applies now
- Probes run as undoable units, return failures as data, and leave no host dialog, test plugin, registry row, or test document behind
- `OSACopyScriptingDefinitionFromURL` returns XML that can contain XInclude references

## [05]-[HOST_ERRORS]

Hosts throw `Error` values with text in the user's locale, and return some failures as values.

`HostError` is the package error for failures no host error (`PlatformError`, `HttpClientError`) states, with a `Refused` reason, refused step as `subject`, and the host's thrown value or returned failure as `cause`. `HostError.refused(subject)` returns a host call's `catch` function:

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
- Consumers decode a host code from `cause` where they branch on it
- Throws in evaluated source read their `isEval()` frame's line from `.stack` under an `Error.prepareStackTrace` restored in `finally`
- Boundaries log a cause once, plugins through `Cause.pretty` to the UXP console, MCP servers through `Cause.prettyErrors` with `Formatter.format`
- Host calls are typed source the compiler checks, with arguments passed as data

## [06]-[SCOPES]

Host state outlasting a call (listener, preference outside undo, temporary file, socket, runtime) pairs acquisition with release in one `Scope`. Hosts call a listener synchronously on their own thread, and removal takes the function registration received:

```ts
const commands = Effect.flatMap(FiberSet.makeRuntime<never, void, HostError>(), (fork) => {
    const handler = (event: Event): void => fork(shown(event));
    return Effect.acquireRelease(Effect.sync(() => document.addEventListener('uxpcommand', handler)), () => Effect.sync(() => document.removeEventListener('uxpcommand', handler)));
});
```

`commands` is an `Effect<void, never, Scope>`, and its handler forks `shown(event)`, the effect a panel runs per visibility event, and returns to its host at once. Closing the scope removes the handler and interrupts its fibers. Host listener calls of every shape (`action.addNotificationListener`, `app.addEventListener`) take the same pair. Host rollback restores document state, and a scope restores every state a rollback leaves.

## [07]-[UNDO_STEPS]

Each host groups changes into one undo step through one call, and every state change runs inside it:

| [INDEX] | [HOST]       | [CALL]                                            | [BODY]                  | [UNDO_STEP]                    |
| :-----: | :----------- | :------------------------------------------------ | :---------------------- | :----------------------------- |
|  [01]   | InDesign     | `app.doScript` under `UndoModes.ENTIRE_SCRIPT`    | Synchronous function    | One step named by its argument |
|  [02]   | Photoshop    | `core.executeAsModal` with `suspendHistory`       | Async function, awaited | One state named at suspension  |
|  [03]   | Premiere Pro | `project.lockedAccess`, then `executeTransaction` | Synchronous callbacks   | One entry named by transaction |

- One call holds one user-visible change, named in the undo menu
- Bodies hold host calls alone, and network, file, and decode work finishes before a body starts and enters as values
- Evaluated source is `Effect<Effect<A, HostError>, HostError | IllegalArgumentError>`, compiled before a step and run inside by `yield* yield*`
- Values and typed failures leave a body as data
- Awaited work inside a synchronous body runs after its unit closed, where a later rejection rolls back no change
- Reads needing no undo step run outside these calls where their host allows

## [08]-[ENTRY_POINTS]

Each entry point translates one `Exit` into the form its host expects:

| [INDEX] | [ENTRY]                        | [BEHAVIOR]                                                    |
| :-----: | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | Plugin `main` module           | Forks `Layer.launch` at load or builds one `ManagedRuntime`   |
|  [02]   | `entrypoints.setup`            | One call per plugin with `plugin`, `panels`, and `commands`   |
|  [03]   | Panel `create`, `show`, `hide` | Forks its work and returns inside a 300 ms default timeout    |
|  [04]   | Command `run`                  | Promise resolved on success, rejected with the squashed cause |

Hosts show nothing for a rejected command, and Photoshop logs nothing for it. One registration takes every command and panel and runs each through one runtime that logs its failure:

```ts
import fs from 'adobe:fs';
import { entrypoints } from 'adobe:uxp';

const manifest = 'plugin:/manifest.json';
const logged = flow(Cause.pretty, Console.error);
const runtime = ManagedRuntime.make(
    MainLayer.pipe(
        Layer.provideMerge(
            Effect.try({ try: () => fs.readFileSync(manifest, 'utf-8'), catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'readFileString', pathOrDescriptor: manifest, cause }) }).pipe(
                Effect.flatMap(Schema.decodeEffect(Manifest.json)),
                Layer.effect(Plugin),
                Layer.merge(hosted),
            ),
        ),
        Layer.tapCause(logged),
    ),
);

Schema.decodeEffect(Schema.Struct({ setup: Schema.declare(Predicate.isFunction) }))(entrypoints).pipe(
    Effect.flatMap(({ setup }) =>
        Effect.sync(() =>
            setup.call(entrypoints, {
                plugin: { create: () => runtime.runPromise(Effect.void), destroy: () => runtime.dispose() },
                commands: Record.map({ relink, preflight }, (work) => () => runtime.runPromise(Effect.tapCause(work, logged))),
                panels: Record.map({ links }, (work) => {
                    const held = Effect.runSync(Scope.provide(ScopedRef.fromAcquire(Effect.void), runtime.scope));
                    return {
                        show: (root: unknown) => runtime.runPromise(Effect.flatMap(Schema.decodeUnknownEffect(Schema.instanceOf(HTMLElement))(root), (element) => ScopedRef.set(held, work(element).pipe(Effect.tapCauseIf(Predicate.not(Cause.hasInterruptsOnly), logged), Effect.forkScoped, Effect.asVoid)))),
                        hide: () => runtime.runPromise(ScopedRef.set(held, Effect.void)),
                    };
                }),
            } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>),
        ),
    ),
    Effect.tapCause(logged),
    Effect.runFork,
);
```

`MainLayer` is the plugin's layer reading `Uxp` and `Plugin`, `relink` and `preflight` the effects its manifest's commands name, and `links` the `(root: HTMLElement) => Effect<void, E, Scope | ...>` panel `links` runs while shown. `hosted` merges the host `Clock` layer with `Uxp`, the service holding `uxp` and `fs`. `Plugin` holds the manifest `Manifest.json` decodes from the plugin folder, and a failed read fails `PlatformError`, the error Effect's `FileSystem.readFileString` raises. `Layer.provideMerge` keeps services in runtime context, where `Layer.provide` feeds the layer alone and `runPromise` reads Effect's default clock. `@adobe-uxp-types/uxp` merges `setup`'s parameter with `entrypoints`, failing records without `getCommand`, `getPanel`, `setup`, and `_pluginInfo`. Decoded `setup` runs through `call` with `entrypoints` as receiver, and its throw logs as a defect. `runFork` calls `setup` before `main` returns. `ManagedRuntime` builds its layer before any entry's work runs. `Layer.tapCause` logs a failed build and `Effect.tapCause` a failed command to the UXP console before `runPromise` rejects. Each panel holds its running work's scope in one `ScopedRef` under `runtime.scope`. `show` replaces the scope and `hide` closes it, where `Effect.forkDetach` fibers outlive `dispose`. `create` builds every layer at load, a linked socket included, and `dispose` closes every scope they opened.

- `setup` throws for a `plugin` record without `create`, for a command or panel key its manifest lacks, and after one successful call
- Panel handlers receive their root node: `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, `destroy(rootNode)`
- Hosts can run `main` at launch with no panel shown, and `main` builds every state a panel reads
- `show` renders from state `main` built, and panel visibility reaches `document` as a `uxpcommand` event

## [09]-[MANIFESTS]

One manifest serves one host, its `id` and `version` derived at build, `host.app` taking `ID`, `PS`, `premierepro`, `ame`, or `XD` with a `minVersion`. Hosts enforce each `requiredPermissions` key:

| [INDEX] | [KEY]                                   | [GRANTS]                                                 | [WITHOUT]                        |
| :-----: | :-------------------------------------- | :------------------------------------------------------- | :------------------------------- |
|  [01]   | `network.domains`                       | Dial and fetch to each listed origin                     | Dial throws a manifest denial    |
|  [02]   | `localFileSystem: "fullAccess"`         | Access outside plugin storage                            | Defaults to plugin storage       |
|  [03]   | `allowCodeGenerationFromStrings`        | `eval`, `new Function`, inline handlers                  | InDesign v5 runs `new Function`  |
|  [04]   | `featureFlags.unhandledRejection`       | `unhandledrejection` events                              | Rejection logged, no caller told |
|  [05]   | `launchProcess`                         | `shell.openPath`, `shell.openExternal`                   | Both calls throw a denial        |
|  [06]   | `clipboard: "read"` or `"readAndWrite"` | System clipboard reads, or reads and writes              | No clipboard access              |
|  [07]   | `ipc.enablePluginCommunication`         | Calls to other installed plugins through `pluginManager` | Defaults to `false`              |
|  [08]   | `webview` `allow: "yes"` with `domains` | Modal dialog WebViews for listed domains or `"all"`      | No WebView                       |
|  [09]   | `enableUserInfo`                        | `userInfo.userId()` returns the Creative Cloud user GUID | No user GUID                     |

- `entrypoints` rows name a `panel` or `command` `id` with `label.default`, and hosts refuse a manifest naming neither `main` nor an entrypoint
- `plugin-temp:` reads and writes the folder `getTemporaryFolder()` returns under every permission level, and `plugin:` is read-only
- Storage providers hold no `unlinkSync`, and a temporary file's native storage entry deletes it through `delete()`
- Storage entries preserve native `url` schemes for file reads and expose `nativePath` for host-native writes
- `adobe:uxp` exports `Entry` as a type alone, and `Uxp.Entry` decodes a storage entry by its `isEntry: true` field, its `url` typed `string` and returned as a string or `URL`
- `domains` entries name a host with a top-level domain or `localhost`, and an IP literal entry is discarded
- `shell.openPath` and `shell.openExternal` open a path or URL behind a consent dialog, with no arguments or process output
- Plugin data sits under `UXP/PluginsStorage/<product>/<major>/External/<pluginId>/PluginData`
- Native addons (`.uxpaddon`, manifest 6 `addon` with `enableAddon`) load in Photoshop and Premiere alone
