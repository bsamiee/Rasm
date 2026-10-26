---
name: uxp-coding
description: "Use when writing or reviewing code that runs in an Adobe UXP host, covering plugin runtime and host boundaries"
---

# [UXP_CODING]

UXP runs each plugin and script in one V8 context inside an Adobe host, with host calls at a boundary that decodes values and translates each `Exit`.

[REFERENCES]:
- [01]-[INDESIGN](references/indesign.md): InDesign's synchronous DOM, from its object model and transactions to GREP, export, and plugin load
- [02]-[PHOTOSHOP](references/photoshop.md): Photoshop's modal scopes, history, descriptors, DOM members, events, export, fonts, and plugin load
- [03]-[PREMIERE](references/premiere.md): Premiere Pro's members, reads, transactions, events, projects, export, and Media Encoder
- [04]-[ILLUSTRATOR](references/illustrator.md): Illustrator's ExtendScript calls, object model, undo steps, export, and plugin load
- [05]-[TRANSPORT](references/transport.md): Typed calls a plugin serves over a socket it dials

## [01]-[CONTEXTS]

Host modules differ in call form, and code is written for one host's form:

| [INDEX] | [HOST]       | [MODULE]      | [DOM_CALLS]                            |
| :-----: | :----------- | :------------ | :------------------------------------- |
|  [01]   | InDesign     | `indesign`    | Synchronous, storage getters async     |
|  [02]   | Photoshop    | `photoshop`   | Promises inside a modal scope          |
|  [03]   | Premiere Pro | `premierepro` | Async getters, action objects          |
|  [04]   | Illustrator  | None          | ExtendScript through `do javascript`   |
|  [05]   | Acrobat      | None          | Acrobat JavaScript through `do script` |

- `require` resolves the running host's DOM module alone, `uxp.host.name` names that host, and cross-host code takes host calls as arguments
- Illustrator and Acrobat run supplied source through one Apple event each, sent in raw form with no terminology read:

```bash
osascript -e 'on run argv' -e 'tell application id "<bundle id>"' \
    -e '«event miscDjxM» (item 1 of argv) given «class JArg»:(rest of argv)' \
    -e 'end tell' -e 'end run' "$(cat <file>.jsx)" <argument>...
osascript -e 'tell application id "com.adobe.Acrobat.Pro"' \
    -e '«event miscdosc» "try { encodeURIComponent(JSON.stringify(<expression>)) } catch (error) { encodeURIComponent(JSON.stringify({ failed: error.message })) }"' \
    -e 'end tell'
```

- Acrobat source runs privileged with `this` naming the front document, and `global` members persist between calls
- Acrobat JavaScript holds `JSON`, `util`, `eval`, `new Function`, and arrow functions, and no `let`, `$`, `File`, or `reflect`
- Acrobat answers an uncaught throw or a syntax error with -1708 and no text, and source catches inside itself
- Acrobat replies in Mac Roman with `.` for a character it lacks, and `decodeURIComponent` restores the `encodeURIComponent` JSON source replies

Context decides what code can hold across time:

| [INDEX] | [CONTEXT] | [LIFETIME]   | [PERMISSIONS]                  | [HOLDS]                                                     |
| :-----: | :-------- | :----------- | :----------------------------- | :---------------------------------------------------------- |
|  [01]   | Plugin    | Host session | Manifest `requiredPermissions` | Listeners, sockets, timers, `localStorage`, `secureStorage` |
|  [02]   | Script    | One run      | Fixed by host                  | Values of that run, `sessionStorage` that ends with it      |

- Work that finishes after its call returns belongs to a plugin
- State a script needs in a later run is host state: a document, a file, or a stored value

## [02]-[RUNTIME]

Each difference from a browser or Node holds in InDesign, Photoshop, and Premiere Pro, and has one owner:

| [INDEX] | [DIFFERENCE]                                  | [OWNER]   | [FORM]                                   |
| :-----: | :-------------------------------------------- | :-------- | :--------------------------------------- |
|  [01]   | Global constructed at load (`TextEncoder`)    | Bundle    | Package implementation injected per read |
|  [02]   | Member of another type (`hrtime.bigint`)      | Runtime   | `Clock` layer over `performance.now()`   |
|  [03]   | Absent global or module (`Worker`, `node:*`)  | Bundle    | Dependencies reading one stay out        |
|  [04]   | Module typed as Node's (`fs`, `path`)         | Read site | Value decoded where read                 |
|  [05]   | Feature with a missing part (`Response.body`) | Call site | Whole-body read, flag on whole pattern   |

Effect's encoding and socket modules construct `TextEncoder` during module load, and bundles replace each encoding-global read with an `@exodus/bytes` import. Effect's default clock expects bigint nanoseconds, and UXP's `process.hrtime.bigint` returns a number. The host `Clock` layer derives monotonic nanoseconds from `performance.now()` and wall nanoseconds from each `currentTimeMillis` read, keeping native sleep and wall time. Offsets anchored once drop the re-anchoring Effect's wall clock performs on skew. Host callbacks running effects receive caller services through `run*With`.

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

- Imports name host modules under `adobe:`, a prefix that separates `fs`, `os`, and `path` from Node's and that bundles strip to the `require` name
- `types` lists `node` beside each `with-protocol` entry, and `node` types `require`, `process`, and `adobe:fs` members
- Diagnostics on a call the object model states name a typings gap, and every other diagnostic names a defect in the call
- Typings can state another enumeration value than the host holds, and code reads enumeration members from the module

Each gap closes where its value is read, with types the package or a schema states:

| [INDEX] | [GAP]                                              | [FORM]                                                     |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Member, value, or failure typings omit or misstate | Host value decoded through a declared `Schema` at its read |
|  [02]   | Module object typed with no module                 | Module passed as an argument of the package's type         |

Typings for `fs` declare bytes as `Buffer` and a failure's `code` as a string, where the host returns an `ArrayBuffer` and a numeric `code`:

```ts
import { readFileSync } from 'adobe:fs';

const Missing = Schema.Struct({ code: Schema.Literal(-2) });
const bytes = (path: string): Effect.Effect<Option.Option<Uint8Array>, HostError> =>
    Effect.try({
        try: () => Option.some(new Uint8Array(readFileSync(path))),
        catch: (cause) => new HostError({ step: path, cause }),
    }).pipe(Effect.catchIf(({ cause }) => Schema.is(Missing)(cause), () => Effect.succeedNone));
```

`new Uint8Array` accepts both representations. `Missing` decodes the numeric `code` a missing file throws into absence, and every other failure stays on the error channel as the package's `HostError`.

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
- Typings can predate the installed build and rename parameters a host names by reserved words, and renamed parameters call correctly by position
- Rejected calls complete each model: host errors name a parameter with its expected type and range, and whether a member applies now
- Probes run as undoable units, return failures as data, and leave no host dialog, test plugin, registry row, or test document behind
- `OSACopyScriptingDefinitionFromURL` returns XML that can contain XInclude references

## [05]-[HOST_ERRORS]

Hosts throw `Error` values with text in the user's locale, return some failures as values, and return an invalid reference for an unmatched lookup.

Each package declares one `Schema.TaggedError` with the refused step and the host's thrown value or returned failure as `cause`:

```ts
class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', {
    step: Schema.String,
    cause: Schema.Defect(),
}) {}
const written: (
    target: object,
    values: Record.ReadonlyRecord<string, unknown>,
) => Effect.Effect<readonly string[], HostError> = Effect.fnUntraced(function* (target, values) {
    return yield* Effect.validate(
        Object.entries(values),
        Effect.fnUntraced(function* ([field, value]: readonly [string, unknown]) {
            const refused = (cause: unknown) => new HostError({ step: field, cause });
            const inner: unknown = Predicate.isObject(value) ? yield* Effect.try({ try: () => Reflect.get(target, field), catch: refused }) : undefined;
            if (Predicate.isObject(value) && Predicate.isObject(inner)) {
                return Array.map(yield* written(inner, value), (name) => `${field}.${name}`);
            }
            yield* Effect.try({ try: () => Object.assign(target, { [field]: value }), catch: refused });
            return [field];
        }, Effect.sandbox),
    ).pipe(
        Effect.map(Array.flatten),
        Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
    );
});
```

`written` writes every field and fails with one reason per refused field, a nested field named by its path. `Effect.validate` partitions each element through `Effect.result`, nested write reasons survive under `Effect.sandbox` alone, and `Cause.fromReasons` merges sandboxed causes. `Object.assign` throws for every refused write, where `Reflect.set` returns `false` for a getter-only member and `true` for a key the object lacks.

- `catch` names the refused step from arguments of the failed call
- Lookups fail with their candidates, and independent writes accumulate every refusal in one cause
- Consumers decode a host code from `cause` where they branch on it
- Plugins log a cause to the UXP console through `Cause.pretty`, MCP servers render one through `Cause.prettyErrors` with `Formatter.format`, once at their boundary

| [INDEX] | [REJECTED]                                                | [REQUIRED]                                | [REASON]                 |
| :-----: | :-------------------------------------------------------- | :---------------------------------------- | :----------------------- |
|  [01]   | Host call built as a string template                      | Typed source, arguments passed as data    | Template skips compiler  |
|  [02]   | `Effect.result` run in a host callback, then `fromResult` | One `Exit` returned through host callback | Failure translated twice |
|  [03]   | `Cause.UnknownError`, or an error class per refused step  | One package error with step and cause     | Step lost or restated    |

## [06]-[SCOPES]

Host state that outlasts a call (listener, preference outside undo, temporary file, socket, runtime) pairs its acquisition with a release in one `Scope`. Hosts call a listener synchronously on their own thread, and removal takes the function registration received:

```ts
const listening = Effect.fnUntraced(function* <V, E>(
    add: (handler: (event: V) => void) => void,
    remove: (handler: (event: V) => void) => void,
    work: (event: V) => Effect.Effect<void, E>,
) {
    const fork = yield* FiberSet.makeRuntime<never, void, E>();
    yield* Effect.acquireRelease(
        Effect.sync(() => {
            const handler = (event: V): void => {
                fork(work(event));
            };
            add(handler);
            return handler;
        }),
        (handler) => Effect.sync(() => remove(handler)),
    );
});
```

`listening` returns `Effect<void, never, Scope>`, and its handler forks `work` and returns to its host at once. Closing the scope removes the handler and interrupts its fibers. `add` and `remove` take host calls of any shape (`addEventListener`, `action.addNotificationListener`). Host rollback restores document state, and a scope restores every state a rollback leaves.

## [07]-[UNITS_OF_WORK]

Each host groups changes into one undo step through one primitive, and every state change runs inside it:

| [INDEX] | [HOST]       | [PRIMITIVE]                                       | [BODY]                  | [UNDO_STEP]                    |
| :-----: | :----------- | :------------------------------------------------ | :---------------------- | :----------------------------- |
|  [01]   | InDesign     | `app.doScript` under `UndoModes.ENTIRE_SCRIPT`    | Synchronous function    | One step named by its argument |
|  [02]   | Photoshop    | `core.executeAsModal` with `suspendHistory`       | Async function, awaited | One state named at suspension  |
|  [03]   | Premiere Pro | `project.lockedAccess`, then `executeTransaction` | Synchronous callbacks   | One entry named by transaction |

- One primitive call holds one user-visible change, named in the undo menu
- Bodies hold host calls alone, and network, file, and decode work finishes before a body starts and enters as values
- Values and typed failures leave a body as data, and a throw rolls back an InDesign body
- Photoshop rolls back a throwing body under an open history suspension alone, and Premiere Pro executes the actions added before the throw
- Awaited work inside a synchronous body runs after its unit closed, where a later rejection rolls back no change
- Reads that need no undo step run outside every primitive where their host allows

## [08]-[ENTRY_POINTS]

Each host entry translates one `Exit` into that entry's host form:

| [INDEX] | [ENTRY]                        | [HOST_FORM]                                                   |
| :-----: | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | Plugin `main` module           | Forks `Layer.launch` at load or builds one `ManagedRuntime`   |
|  [02]   | `entrypoints.setup`            | One call per plugin with `plugin`, `panels`, and `commands`   |
|  [03]   | Panel `create`, `show`, `hide` | Forks its work and returns inside a 300 ms default timeout    |
|  [04]   | Command `run`                  | Promise resolved on success, rejected with the squashed cause |

Hosts show nothing for a rejected command, and Photoshop logs nothing for it. One registration takes every command and panel and runs each through one runtime that logs its failure:

```ts
import fs from 'adobe:fs';
import { entrypoints } from 'adobe:uxp';

const plugin = <R, E, F, C extends string, P extends string>(
    layer: Layer.Layer<R, E, Uxp | Plugin>,
    entries: {
        readonly commands: Record.ReadonlyRecord<C, Effect.Effect<void, F, R | Uxp | Plugin>>;
        readonly panels: Record.ReadonlyRecord<P, Record.ReadonlyRecord<'show' | 'hide', Effect.Effect<void, F, R | Uxp | Plugin>>>;
    },
): void => {
    const logged = flow(Cause.pretty, Console.error);
    const runtime = ManagedRuntime.make(
        layer.pipe(Layer.provideMerge(Effect.try(() => fs.readFileSync('plugin:/manifest.json', 'utf-8')).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json)), Layer.effect(Plugin), Layer.merge(hosted))), Layer.tapCause(logged)),
    );
    const run = (work: Effect.Effect<void, F, R | Uxp | Plugin>) => (): Promise<void> => runtime.runPromise(Effect.tapCause(work, logged));
    const handlers = {
        plugin: { create: (): Promise<void> => runtime.runPromise(Effect.void), destroy: (): Promise<void> => runtime.dispose() },
        commands: Record.map(entries.commands, run),
        panels: Record.map(entries.panels, Record.map(run)),
    } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands' | 'panels'>;
    Effect.runFork(
        Schema.decodeUnknownEffect(Schema.Struct({ setup: Schema.declare(Predicate.isFunction) }))(entrypoints).pipe(
            Effect.flatMap(({ setup }) => Effect.sync(() => setup.call(entrypoints, handlers))),
            Effect.tapCause(logged),
        ),
    );
};
```

`hosted` merges the host `Clock` layer with `Uxp`, the service holding `uxp` and `fs`, and `Plugin` holds the manifest `Manifest.json` decodes from the plugin's own folder. `Layer.provideMerge` keeps every service in the runtime's context, where `Layer.provide` feeds the layer alone and every `runPromise` reads Effect's default clock. `@adobe-uxp-types/uxp` merges the `setup` parameter with `entrypoints`, which a handler record fails for lacking `getCommand`, `getPanel`, `setup`, and `_pluginInfo`. `satisfies` checks each handler against the declared `plugin`, `commands`, and `panels` members, and `setup` decoded through a declared `Schema` runs under `call` with `entrypoints` as receiver and logs a throw as a defect. `ManagedRuntime` builds its layer before any entry's work runs, and `Layer.tapCause` logs a failed build and `Effect.tapCause` a failed entry to the UXP developer console before `runPromise` rejects with the squashed cause. `create` builds every layer at load, a linked socket included, and `dispose` closes every scope they opened.

- `setup` throws for a `plugin` record without `create`, for a command or panel key its manifest lacks, and after one successful call
- Panel handlers receive their root node: `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, `destroy(rootNode)`
- Hosts can run `main` at launch with no panel shown, and `main` builds every state a panel reads
- `show` renders from state `main` built, and panel visibility reaches `document` as a `uxpcommand` event

## [09]-[MANIFESTS]

One manifest serves one host application: `host.app` takes `ID`, `PS`, `premierepro`, `ame`, or `XD` with a `minVersion`, `entrypoints` rows name a `panel` or `command` `id` with `label.default`, and each `requiredPermissions` key is a boundary its host enforces:

| [INDEX] | [KEY]                             | [GRANTS]                                | [WITHOUT]                        |
| :-----: | :-------------------------------- | :-------------------------------------- | :------------------------------- |
|  [01]   | `network.domains`                 | Dial and fetch to each listed origin    | Dial throws a manifest denial    |
|  [02]   | `localFileSystem: "fullAccess"`   | Access outside plugin storage           | Defaults to plugin storage       |
|  [03]   | `allowCodeGenerationFromStrings`  | `eval`, `new Function`, inline handlers | InDesign v5 runs `new Function`  |
|  [04]   | `featureFlags.unhandledRejection` | `unhandledrejection` events             | Rejection logged, no caller told |
|  [05]   | `launchProcess`                   | `shell.openPath`, `shell.openExternal`  | Both calls throw a denial        |

- `plugin-temp:` reads and writes the folder `getTemporaryFolder()` returns under every permission level, and `plugin:` is read-only
- Storage providers hold no `unlinkSync`, and a temporary file's native storage entry deletes it through `delete()`
- Storage entries preserve native `url` schemes for file reads and expose `nativePath` for host-native writes
- `domains` entries name a host with a top-level domain or `localhost`, and an IP literal entry is discarded
- `shell.openPath` and `shell.openExternal` open a path or URL behind a consent dialog, with no arguments or process output
- Plugin data sits under `UXP/PluginsStorage/<product>/<major>/External/<pluginId>/PluginData`
- Native addons (`.uxpaddon`, manifest 6 `addon` with `enableAddon`) load in Photoshop and Premiere alone

Use `manage-repo` for packaging and installing plugins.
