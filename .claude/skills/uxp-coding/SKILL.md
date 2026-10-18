---
name: uxp-coding
description: "Use when writing or reviewing code that runs in an Adobe UXP host, covering contexts, runtime, typings, object models, host errors, scopes, units of work, entry points, and manifests."
---

# [UXP_CODING]

UXP runs each plug-in and script in one V8 context inside an Adobe host, with host calls at a boundary that decodes values and translates each `Exit`.

[REFERENCES]:
- [01]-[INDESIGN](references/indesign.md): InDesign's synchronous DOM, from its object model and transactions to GREP, export, and plug-in load
- [02]-[PHOTOSHOP](references/photoshop.md): Photoshop's modal scopes, history, descriptors, DOM members, events, export, fonts, and plug-in load
- [03]-[PREMIERE](references/premiere.md): Premiere Pro's members, reads, transactions, events, projects, export, and Media Encoder
- [04]-[ILLUSTRATOR](references/illustrator.md): Illustrator's ExtendScript calls, object model, undo steps, export, and plug-in load
- [05]-[REWRITING](references/rewriting.md): Supplied source compiled through positional splices, with thrown lines mapped back
- [06]-[TRANSPORT](references/transport.md): Typed calls a plug-in serves over a socket it dials

## [01]-[CONTEXTS]

Host modules differ in call form, and code is written for one host's form:

| [INDEX] | [HOST]       | [MODULE]      | [DOM_CALLS]                            | [PLUG_INS]  |
| :-----: | :----------- | :------------ | :------------------------------------- | :---------- |
|  [01]   | InDesign     | `indesign`    | Synchronous                            | Third-party |
|  [02]   | Photoshop    | `photoshop`   | Promises inside a modal scope          | Third-party |
|  [03]   | Premiere Pro | `premierepro` | Async getters, action objects          | Third-party |
|  [04]   | Illustrator  | None          | ExtendScript through `do javascript`   | Adobe's own |
|  [05]   | Acrobat      | None          | Acrobat JavaScript through `do script` | None        |

- `require` resolves the running host's DOM module alone, `uxp.host.name` names that host, and code shared between hosts takes host calls as arguments
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
|  [01]   | Plug-in   | Host session | Manifest `requiredPermissions` | Listeners, sockets, timers, `localStorage`, `secureStorage` |
|  [02]   | Script    | One run      | Fixed by host                  | Values of that run, `sessionStorage` that ends with it      |

- Work that finishes after its call returns belongs to a plug-in
- State a script needs in a later run is host state: a document, a file, or a stored value

## [02]-[RUNTIME]

Each difference from a browser or Node holds in InDesign, Photoshop, and Premiere Pro, and has one owner:

| [INDEX] | [DIFFERENCE]                                  | [OWNER]             | [FORM]                                   |
| :-----: | :-------------------------------------------- | :------------------ | :--------------------------------------- |
|  [01]   | Global constructed at load (`TextEncoder`)    | Bundle              | Package implementation injected per read |
|  [02]   | Member of another type (`hrtime.bigint`)      | `@rasm/uxp/runtime` | `Clock` layer over `performance.now()`   |
|  [03]   | Absent global or module (`Worker`, `node:*`)  | Bundle              | Dependencies reading one stay out        |
|  [04]   | Module typed as Node's (`fs`, `path`)         | Read site           | Value decoded where read                 |
|  [05]   | Feature with a missing part (`Response.body`) | Call site           | Whole-body read, flag on whole pattern   |

Effect's `encoding/Base64`, `socket/Socket`, and `encoding/SchemaBinary` modules construct a `TextEncoder` while they load, before any module body of the plug-in runs. Root `uxp()` in `vite.config.ts` gives rolldown `transform.inject` rows that turn each read of an encoding global into an import of its `@exodus/bytes` class, with no global assigned and no import order to keep. Effect's default clock reads `process.hrtime.bigint`, a `number` in UXP, at each span start, span end, and log inside a span, and throws "Cannot mix BigInt and other types". Every runtime and `run*` call provides a `Clock` layer over `performance.now()`, which `launch` of `@rasm/uxp/runtime` provides to a panel, and a `run*With` inside a host callback passes the caller's services on.

- Effect decodes with `{ stream: true }`, which an encoding package must implement
- `structuredClone`, `Buffer`, `MessageChannel`, and `Worker` are absent in every host, and `require` of `util`, `buffer`, or a `node:` name throws
- `fs` holds synchronous reads, writes, and `lstatSync` without `existsSync`, `appendFileSync`, `mkdirSync`, or `statSync`, and `writeFileSync` reads its receiver, called on the module's default import
- `path` is a function with `resolve` and `basename`

Bundles keep one form:
- Output is one CommonJS script with each host module left as a `require` call by module name, and an unresolved dependency throws at its first read
- Resolution uses neutral conditions: browser conditions select builds that construct absent globals at load, Node conditions emit `node:` requires
- Plug-in `main` is a script that renders into `document.body`, and UXP never runs `<script type="module">`

## [03]-[TYPINGS]

Typings declare a host module under its `require` name, under an `adobe:` scheme, or as a type with no module:

| [INDEX] | [MODULES]                 | [PACKAGE]                    | [DECLARES]                                                        |
| :-----: | :------------------------ | :--------------------------- | :---------------------------------------------------------------- |
|  [01]   | `uxp`, `fs`, `os`, `path` | `@adobe-uxp-types/uxp`       | `uxp` by name, each module under `adobe:` from `with-protocol`    |
|  [02]   | `photoshop`               | `@adobe-uxp-types/photoshop` | `photoshop` by name, `adobe:photoshop` from `with-protocol`       |
|  [03]   | `photoshop`               | `@types/photoshop`           | Adobe's declarations, `photoshop` and `photoshop/dom/*` by name   |
|  [04]   | `indesign`                | `indesign-uxp-types`         | `indesign` by name from its root entry, a module file from `flat` |
|  [05]   | `premierepro`             | `@adobe/premierepro`         | Type `premierepro` of the module object, no module                |
|  [06]   | Illustrator ExtendScript  | `types-for-adobe`            | Global classes per host folder, no module                         |

- Imports name host modules under `adobe:`, a prefix that separates `fs`, `os`, and `path` from Node's and that bundles strip to the `require` name
- `types` lists `node` beside each `with-protocol` entry, and `node` types `require`, `process`, and `adobe:fs` members
- Diagnostics on a call the object model states name a typings gap, and every other diagnostic names a defect in the call
- Typings can state another enumeration value than the host holds, and code reads enumeration members from the module

Each gap closes where its value is read, with types the package or a schema states:

| [INDEX] | [GAP]                                     | [FORM]                                                         |
| :-----: | :---------------------------------------- | :------------------------------------------------------------- |
|  [01]   | Member a host has and typings omit        | `Schema.decodeUnknownOption(schema)(Reflect.get(owner, name))` |
|  [02]   | Value or failure with a misstated shape   | `Schema` decode at its read                                    |
|  [03]   | Parameter type no host argument satisfies | `Reflect.apply(member, owner, [argument])`, under `satisfies`  |
|  [04]   | Module object typed with no module        | Module passed as an argument of the package's type             |

Typings for `fs` declare bytes as `Buffer` and a failure's `code` as a string, where the host returns an `ArrayBuffer` and a numeric `code`:

```ts
import { readFileSync } from 'adobe:fs';

class FileError extends Schema.TaggedError<FileError, { readonly brand: unique symbol }>()('FileError', { path: Schema.String, code: Schema.Int }) {}

const Coded = Schema.Struct({ code: FileError.fields.code });
const bytes = (path: string): Effect.Effect<Uint8Array, FileError> =>
    Effect.sync(() => new Uint8Array(readFileSync(path))).pipe(
        Effect.catchDefect((cause) =>
            Option.match(Schema.decodeUnknownOption(Coded)(cause), {
                onNone: () => Effect.die(cause),
                onSome: ({ code }) => Effect.fail(FileError.make({ path, code })),
            }),
        ),
    );
```

`new Uint8Array` accepts both representations, `Coded` takes its field from `FileError`, a thrown value without that shape reaches `Cause` unchanged as a defect, and a missing file throws `code` -2.

## [04]-[OBJECT_MODELS]

Class, property, method, parameter, and enumeration facts come from the owner that states them for the installed build, and source holds no copy:

| [INDEX] | [HOST]       | [OWNER]                                             | [READ]                                          |
| :-----: | :----------- | :-------------------------------------------------- | :---------------------------------------------- |
|  [01]   | InDesign     | `$.dictionary` of the running host                  | `getClass(<name>).toXML()` rows                 |
|  [02]   | Photoshop    | `app.js` of the installed build, `@types/photoshop` | Syntax-tree read joined with its declaration    |
|  [03]   | Premiere Pro | `ScriptingAPIManifest.json` and `api_config.json`   | `jq` read of the classes the allow-list exposes |
|  [04]   | Illustrator  | `reflect` of the running host, `types-for-adobe`    | `reflect.properties` rows, methods by name      |
|  [05]   | Acrobat      | `acrohelp` argument to a native method              | `HelpError.extMessage` per parameter            |

- Acrobat's `extMessage` lists parameters as `name: type`, optional ones in brackets
- AppleScript dictionaries hold AppleScript names and forms with method parameters missing or changed, and serve Apple event encoding alone
- Typings can predate the installed build and rename parameters a host names by reserved words, and renamed parameters call correctly by position
- Rejected calls complete each model: host errors name a parameter with its expected type and range, and whether a member applies now
- Probes run as a preview or undoable unit, return failures as data, and leave no host dialog, test plug-in, registry row, or test document behind
- Use `Skill(search-code)` for a declaration in `types-for-adobe` or `@types/photoshop`

## [05]-[HOST_ERRORS]

Hosts throw `Error` values with text in the user's locale, return some failures as values, and return an invalid reference for an unmatched lookup. Each package declares one `Schema.TaggedError` over a `Schema.TaggedUnion` of reasons, and the operation that raised a failure builds its reason:

```ts
const Reason = Schema.TaggedUnion({
    Absent: { subject: Schema.String, candidates: Schema.Array(Schema.String) },
    Rejected: { field: Schema.String, cause: Schema.Defect() },
});
class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', { reason: Reason }) {
    override get message(): string {
        return Reason.match(this.reason, {
            Absent: ({ subject, candidates }) => [`No ${subject}`, ...candidates].join(', '),
            Rejected: ({ field, cause }) => `${field}: ${Formatter.format(cause)}`,
        });
    }
}

const Change = Schema.Struct({ name: Schema.String, values: Schema.Record(Schema.String, Schema.Json) });
const changed = Effect.fn('changed')(function* (find: (name: string) => Option.Option<object>, names: () => readonly string[], input: unknown) {
    const { name, values } = yield* Schema.decodeUnknownEffect(Change)(input);
    const target = yield* Effect.fromOption(find(name), () => HostError.make({ reason: { _tag: 'Absent', subject: name, candidates: names() } }));
    yield* Effect.validate(
        Object.entries(values),
        ([field, value]) =>
            Effect.try({
                try: () => Object.assign(target, { [field]: value }),
                catch: (cause) => HostError.make({ reason: { _tag: 'Rejected', field, cause } }),
            }),
        { discard: true },
    );
    return name;
});
const lenient = flow(
    changed,
    Effect.catchReason('HostError', 'Absent', ({ subject }) => Effect.succeed(subject)),
);
```

Decode, lookup, and write stop at their first failure, while field writes run to their last element and fail with `NonEmptyArray<HostError>`, one `Rejected` per refused field. `names` runs on the failure path alone. `Object.assign` throws for every refused write, where `Reflect.set` returns `false` for a getter-only member and `true` for a key the object lacks. `catchReason` in `lenient` recovers `Absent` alone, and `SchemaError` and `Rejected` stay on the error channel.

- `catch` builds its reason from arguments of the failed call, and `Schema.Defect()` keeps the thrown value typed until a view renders it
- `message` is a getter over the reason, a stored `message` field replaces the rendered text and drops every other field from it

| [INDEX] | [REJECTED]                                                | [REQUIRED]                                | [REASON]                         |
| :-----: | :-------------------------------------------------------- | :---------------------------------------- | :------------------------------- |
|  [01]   | Reason chosen by matching host message text               | Reason built by operation that threw      | Text changes per locale, release |
|  [02]   | Pre-check copying a host rule                             | Host answer mapped to its reason          | Copy drifts from host rule       |
|  [03]   | Literal table of facts a host states                      | One bulk host read where consumed         | Table drifts per install         |
|  [04]   | Host call built as a string template                      | Typed source, arguments passed as data    | Template skips compiler          |
|  [05]   | `Effect.result` run in a host callback, then `fromResult` | One `Exit` returned through host callback | Failure translated twice         |

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

- One primitive call holds one user-visible change, and its name is the text a user reads in the undo menu
- Bodies hold host calls alone, and network, file, and decode work finishes before a body starts and enters as values
- Values and typed failures leave a body as data, and a throw rolls back an InDesign body
- Photoshop rolls back a throwing body under an open history suspension alone, and Premiere Pro executes the actions added before the throw
- Awaited work inside a synchronous body runs after its unit closed, where a later rejection rolls back no change
- Reads that need no undo step run outside every primitive where their host allows

## [08]-[ENTRY_POINTS]

Each host entry translates one `Exit` into that entry's host form:

| [INDEX] | [ENTRY]                        | [HOST_FORM]                                                   |
| :-----: | :----------------------------- | :------------------------------------------------------------ |
|  [01]   | Plug-in `main` module          | Forks `Layer.launch` at load or builds one `ManagedRuntime`   |
|  [02]   | `entrypoints.setup`            | One call per plug-in with `plugin`, `panels`, and `commands`  |
|  [03]   | Panel `create`, `show`, `hide` | Forks its work and returns inside a 300 ms default timeout    |
|  [04]   | Command `run`                  | Promise resolved on success, rejected with the squashed cause |

Hosts show nothing for a rejected command, and Photoshop logs nothing for it. One registration runs every command through one runtime and renders each outcome:

```ts
import { entrypoints } from 'adobe:uxp';

const registered = <R, ER, E>(
    layer: Layer.Layer<R, ER>,
    status: HTMLElement,
    commands: Record.ReadonlyRecord<string, Effect.Effect<string, E, R>>,
): void => {
    const runtime = ManagedRuntime.make(Layer.provideMerge(layer, clock));
    const rendered = Effect.onExit((exit: Exit.Exit<string, E>) =>
        Effect.sync(() => {
            status.setAttribute('data-state', exit._tag);
            status.replaceChildren(Exit.match(exit, { onSuccess: identity, onFailure: Cause.pretty }));
        }),
    );
    const handlers = {
        plugin: { create: (): Promise<void> => runtime.runPromise(Effect.void), destroy: (): Promise<void> => runtime.dispose() },
        commands: Record.map(commands, (work) => (): Promise<void> => runtime.runPromise(Effect.asVoid(rendered(work)))),
    } satisfies Pick<Parameters<typeof entrypoints.setup>[0], 'plugin' | 'commands'>;
    Reflect.apply(entrypoints.setup, entrypoints, [handlers]);
};
```

`clock` is the host `Clock` layer. `@adobe-uxp-types/uxp` merges the `setup` parameter with `entrypoints`, which a handler record fails for lacking `getCommand`, `getPanel`, `setup`, and `_pluginInfo`. `satisfies` checks each handler against the declared `plugin` and `commands` members, and `Reflect.apply` calls the untyped overload with `entrypoints` as receiver. `Effect.onExit` renders every failed `Exit` through `Cause.pretty` before `runPromise` rejects with the squashed cause. `create` builds every layer at load, and `dispose` closes every scope they opened.

- Panels run `launch(<layer>, { peer, origin })` of `@rasm/uxp/runtime` at load, which renders link and call status into `document.body`
- `setup` throws for a `plugin` record without `create`, for a command or panel key its manifest lacks, and after one successful call
- Panel handlers receive their root node: `create(rootNode)`, `show(rootNode, data)`, `hide(rootNode, data)`, `destroy(rootNode)`
- `main` builds every state a panel reads, since a host can run `main` at launch with no panel shown
- `show` renders from state `main` built, and panel visibility reaches `document` as a `uxpcommand` event

## [09]-[MANIFESTS]

One manifest serves one host application: `host.app` takes `ID`, `PS`, `premierepro`, or `ame` with a `minVersion`, `entrypoints` rows name a `panel` or `command` `id` with `label.default`, and each `requiredPermissions` key is a boundary its host enforces:

| [INDEX] | [KEY]                             | [GRANTS]                                | [WITHOUT]                                      |
| :-----: | :-------------------------------- | :-------------------------------------- | :--------------------------------------------- |
|  [01]   | `network.domains`                 | Dial and fetch to each listed origin    | Dial throws a manifest denial                  |
|  [02]   | `localFileSystem: "fullAccess"`   | `fs` on absolute paths and `file:` URLs | `fs` throws "Route not found"                  |
|  [03]   | `allowCodeGenerationFromStrings`  | `eval` and `new Function` in Photoshop  | `EvalError`, InDesign and Premiere Pro compile |
|  [04]   | `featureFlags.unhandledRejection` | `unhandledrejection` events             | Rejection logged, no caller told               |

- `plugin-temp:` reads and writes the folder `getTemporaryFolder()` returns under every permission level, and `plugin:` is read-only
- Storage providers hold no `unlinkSync`, and code releases a temporary file by overwriting it
- Host-native writes take a folder's `nativePath`, and `fs` reads that file back under its scheme
- `domains` entries name a host with a top-level domain or `localhost`, and an IP literal entry is discarded

Packaged plug-ins are `.ccx` zips, one per host, with `manifest.json` at their root. `UnifiedPluginInstallerAgent --install <file>.ccx` installs into the release hosts Adobe's installer serves, and a Beta host loads a plug-in folder through a registry row written beside it:
- Row `path` reads `$localPlugins/External/<folder>`, a folder under `UXP/Plugins/External`
- Plug-in data sits under `UXP/PluginsStorage/<product>/<major>/External/<pluginId>/PluginData`
