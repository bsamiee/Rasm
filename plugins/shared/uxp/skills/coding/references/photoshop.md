# [PHOTOSHOP]

Photoshop's UXP DOM uses Action Manager descriptors. `action.batchPlay` plays changes inside one modal scope and reads anywhere, and replies with one descriptor per command. Apple event `do javascript` runs ExtendScript, where `new Function` of bad syntax answers `undefined` where `eval` throws, and an `undefined` last value answers as text.

## [01]-[MODAL_SCOPES]

`core.executeAsModal` holds Photoshop's one modal scope for a body, and a refused command resolves as an error descriptor in the reply array:

```ts
import { type ActionDescriptor, action, app, core, type ExecutionContext } from 'adobe:photoshop';

const REJECTIONS = { Cancelled: 2, Unavailable: 9 } as const;
const Rejected = Schema.Struct({ number: Schema.Union(Record.collect(REJECTIONS, (tag, code) => Schema.Literal(code).transform(tag))) });

const played = (commands: ActionDescriptor[]) =>
    Effect.flatMap(Effect.tryPromise({ try: () => action.batchPlay(commands, {}), catch: HostError.refused(Array.map(commands, Struct.get('_obj')).join(', ')) }), (replies) =>
        Option.match(
            Array.findFirst(Array.zip(commands, replies), ([{ _obj: command }, reply]) => Option.map(Schema.decodeUnknownOption(Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Int }))(reply), HostError.refused(command))),
            { onNone: () => Effect.succeed(replies), onSome: Effect.fail },
        ),
    );
const cancelled = (context: ExecutionContext) =>
    Effect.callback<never>((resume) => {
        Object.assign(context, { onCancel: () => resume(Effect.interrupt) });
        return Effect.sync(() => Object.assign(context, { onCancel: undefined }));
    });
const modal = <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>) =>
    Effect.flatMap(Effect.context<R>(), (services) =>
        Effect.flatten(
            Effect.tryPromise({
                try: (signal) => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(Effect.raceFirst(body(context), cancelled(context)), { signal }), { commandName }),
                catch: (cause) => Option.match(Schema.decodeUnknownOption(Rejected)(cause), { onNone: () => HostError.refused(commandName)(cause), onSome: ({ number: _tag }) => HostError.make({ reason: { _tag }, subject: commandName, cause }) }),
            }),
        ),
    );
const committed = <A, E, R>(name: string, body: Effect.Effect<A, E | HostError, R>) =>
    modal(name, ({ hostControl }) =>
        app.documents.reduce<Effect.Effect<A, E | HostError, R>>(
            (inner, { id }) =>
                Effect.acquireUseRelease(
                    Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID: id, name }), catch: HostError.refused(`suspendHistory of document ${id}`) }),
                    () => inner,
                    (suspension, exit) => Effect.tryPromise({ try: () => hostControl.resumeHistory(suspension, Exit.isSuccess(exit)), catch: HostError.refused(`resumeHistory of document ${id}`) }),
                ),
            body,
        ),
    );
```

`played` fails with the first refused command's error descriptor. `committed` runs `body` under one suspended history state per open document, or unsuspended with none open. Each resume commits on success and drops every change on failure or interruption, and a refused resume fails `HostError`. Suspensions left open commit when their body resolves. `modal` runs its body on the caller's services and passes its `signal`, and an interrupted caller interrupts the body. `Effect.raceFirst` resumes with the first exit, and a progress-window cancel interrupts the body through `onCancel`. `Rejected` decodes documented rejection codes into `Cancelled` and `Unavailable`, and every other rejection stays `Refused`. `Effect.flatten` unwraps the `Exit` the body returns as a value.

| [INDEX] | [CALL]                                                    | [REPLY]                                                         |
| :-----: | :-------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Command, DOM setter, or method outside a scope            | Throws `number` 1000 naming the first event it plays            |
|  [02]   | Unknown command, or `get`, `set`, `select` of no element  | `result` -25920, reply array ends at that command               |
|  [03]   | `hide` of no element                                      | `result` -25922                                                 |
|  [04]   | `multiGet` of a closed document or a missing property     | `result` -25923                                                 |
|  [05]   | Dialog cancelled, or refusal alert closed under `display` | `result` -128 with an empty `message`                           |
|  [06]   | Modal scope another plugin holds                          | Rejects `number` 9 after `timeOut` (1 s default) unless freed   |
|  [07]   | `executeAsModal` inside a body                            | Runs nested in its outer scope with a context of its own        |
|  [08]   | Cancel on the progress window                             | `onCancel` runs, awaited plays throw, rejects `number` 2        |
|  [09]   | Body rejecting with an `Error`                            | `executeAsModal` rejects with its string, other values as given |

- `resumeHistory({ ...suspension, finalName }, true)` renames the state at commit, and a suspension holding no change records no state
- Plays outside a suspension record one state each under Photoshop's name (`New Layer`, `Master Opacity Change`) and stay when their body throws
- `Document.suspendHistory(callback, name)` wraps both calls in `executeAsModal`, passes `document` for `hostControl`, and resolves `undefined`
- `interactive: true` scopes open `display` dialogs and no progress window, which other scopes open after a delay and fill from `reportProgress`
- `executeAsModal` resolves with its target's value, `@adobe-uxp-types/photoshop` types that value, and `@types/photoshop` types `Promise<void>`

## [02]-[DESCRIPTORS]

Descriptors are plain records in Action JSON, and one `multiGet` returns named properties of every element under a target:

```ts
const layers = (documentID: number) =>
    played([
        {
            _obj: 'multiGet',
            _target: { _ref: 'document', _id: documentID },
            extendedReference: [['layerID', 'name', 'visible'], { _obj: 'layer', index: 1, count: -1 }],
            options: { failOnMissingProperty: false, failOnMissingElement: false },
        },
    ]).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ list: Schema.Array(Schema.Struct({ layerID: Schema.Int, name: Schema.String, visible: Schema.Boolean })) })]))),
        Effect.map(([{ list }]) => list),
    );
```

`layers` runs outside every modal scope and fails with an error descriptor as cause for a closed document. `index: 1` starts above a background layer, `count: -1` runs to the last element, and `failOnMissingProperty: false` omits an absent property.

| [INDEX] | [VALUE]     | [FORM]                                                             |
| :-----: | :---------- | :----------------------------------------------------------------- |
|  [01]   | Object      | `{ _obj: <class>, <key>: <value> }`                                |
|  [02]   | Enumeration | `{ _enum: <type>, _value: <member> }`                              |
|  [03]   | Unit number | `{ _unit: <unit>, _value: <number> }`                              |
|  [04]   | Reference   | `{ _ref: <class>, _id: <id> }`, outermost container last           |
|  [05]   | Property    | `[{ _property: <key> }, { _ref: <class>, _id: <id> }]` under `get` |
|  [06]   | Target      | `{ _ref: <class>, _enum: 'ordinal', _value: 'targetEnum' }`        |
|  [07]   | Dialog      | `_options: { dialogOptions: 'display' }`, `silent` when absent     |

- `_id` names an element across edits, where `_index` and `_name` change as a user edits
- Units are `pixelsUnit`, `pointsUnit`, `percentUnit`, `angleUnit`, `densityUnit`, `distanceUnit`, and `millimetersUnit`
- Keys are string IDs, and `get` returns keys the Actions panel never records (`targetLayers`, `layerKind`, `documentID`)
- Each `set` holds only the keys it changes, and keys one preference couples (a color enum and its custom color) go in one descriptor
- `batchPlay(commands, { synchronousExecution: true })` returns the reply array in place of a promise
- DOM members play with `app.displayDialogs` as `dialogOptions`, `DialogModes.ERROR` (`'silent'`) by default

## [03]-[DOM]

DOM members are JavaScript over descriptors, and a getter plays a synchronous `get`. Setters and methods play `saveDocumentSelection`, a `select` of their target, and their command in one batch that leaves the selection unchanged.

| [INDEX] | [NEED]                        | [FORM]                                       | [REASON]                                           |
| :-----: | :---------------------------- | :------------------------------------------- | :------------------------------------------------- |
|  [01]   | Many properties, many targets | One `multiGet`, getters for one read         | One call reads every target                        |
|  [02]   | Enumeration comparison        | `constants` member under `===`               | Strings, numbers in `SaveOptions` and `SampleSize` |
|  [03]   | Preference read or write      | `app.preferences` member, writes in a scope  | Members play an application `get` or `set`         |
|  [04]   | Tool selection                | `select` of `{ _ref: <tool ID> }` in a scope | `app.currentTool` is a getter of a `Tool`          |
|  [05]   | Element by name               | `getByName`, then a null check               | Returns `null` for no match                        |
|  [06]   | Element held across edits     | Its DOM object or `id`                       | One object per id, members of a deleted one throw  |

- DOM objects hold `typename` typed as their class literal (`'Layer'`, `'TextFont'`, `'Layers'`), the discriminant of a union of DOM classes
- DOM objects name their container through `document` or `parent` (`null` at top level), and collections extend `Array` with `parent` naming their owner
- Returned objects derive a specifier: `String.uncapitalize(typename)` names the kind, and `id`, then name or PostScript name, then index the selector
- `document.layers` and a group's `layers` hold the ids one `getLayerTreeSync` or `getLayerGroupContentsSync` call read
- `getByName` on a layer list gets each layer's `name`
- `app.documents` and `app.fonts` play a get at each `length` and index read, and an array method on either plays one list read
- Preference members reach a subset of application keys, and a `get` of the application property reaches the rest (`guidesPrefs`, `interfacePrefs`)
- Unlocking a background layer gives it a new `id`, which its DOM object takes

Members answer failures in their own form:

| [INDEX] | [CASE]                                                                      | [ANSWER]                                                  |
| :-----: | :-------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Argument of wrong type or constant, number out of a checked range           | Throws `RangeError` or `Error` naming it, before any play |
|  [02]   | Number no member checks (`Layer.opacity = 500`)                             | Photoshop clamps it, reading 98.4                         |
|  [03]   | Error descriptor in a setter reply (`characterStyle.font` of a family name) | Ignored with the value unchanged                          |
|  [04]   | Error descriptor in an `app.preferences` reply                              | Throws its `message`                                      |
|  [05]   | Refused `createLayer`, `createTextLayer`, `createLayerGroup`, `groupLayers` | Resolves `null`                                           |
|  [06]   | Refused `createDocument`                                                    | Throws a fixed message in place of Photoshop's            |
|  [07]   | `document.paste`                                                            | Resolves `null` without awaiting its play                 |

## [04]-[EVENTS]

`action.addNotificationListener(events, handler)` calls `handler(event, descriptor)` for each listed event. `removeNotificationListener` with the same names and function removes it:
- User commands arrive with `_isCommand: true` in the descriptor
- Plays from a plugin's own modal scope, interactive included, and plays from ExtendScript reach no listener of that plugin
- `core.addNotificationListener('UI', events, handler)` delivers interface events, and group `'OS'` system ones (`displayConfigurationChanged`)
- `panelVisibilityChanged` holds `panelID` and `visible` for every panel

## [05]-[EXPORT]

`document.saveAs` holds `psd`, `psb`, `jpg`, `gif`, `png`, and `bmp`, each taking `(file, options, asCopy)` in a modal scope with a UXP `File` entry:
- `localFileSystem.getEntryWithUrl('file:<folder>')`, then `createFile(<name>, { overwrite: true })`, gives the entry under `fullAccess`
- `asCopy` true writes the file and leaves `title` and `path` unchanged
- `close()` opens a save dialog under default `SaveOptions.PROMPTTOSAVECHANGES`, and `closeWithoutSaving()` in a scope discards changes synchronously
- `hostControl.registerAutoCloseDocument(documentID)` closes a working document unsaved when its scope ends
- `save` descriptors take `in: { _path: localFileSystem.createSessionToken(file), _kind: 'local' }`, and plain paths reject "invalid file token used"
- `exportmanager` serves any plugin outside a modal scope, and neither typing declares it
- `exportImageAsync({ documentID, exportPath, exportOptions, formatOptions, exportTaskID })` writes the file
- `exportOptions` hold `scaleHorizontal` and `scaleVertical` in percent, and `formatOptions` `{ name: 'png', colorType: 'png24' }`
- `imaging` calls other than `createImageDataFromBuffer` throw `number` 1000 outside a modal scope, and `putPixels` records one history state
- `imaging.getPixels` under `targetSize` reads the smallest cache level at least that size, `level` n at 1/2ⁿ scale with `sourceBounds` in its pixels
- `imaging.encodeImageData` encodes RGB or Grayscale data as JPEG into a `Uint8Array` typed as `number[]`, or base64 text under `base64: true`

## [06]-[FONTS]

One `get` of the application property `fontList` returns parallel arrays `fontName`, `fontPostScriptName`, `fontFamilyName`, and `fontStyleName`:
- `app.fonts.getByName(<PostScript name>)` plays one `findFontByName` and returns a `TextFont` or `null`
- `characterStyle` and `paragraphStyle` getters read the first style run of `textKey`, then its `baseParentStyle`
- `characterStyle.font` reads and writes a PostScript name
- DOM text sizes are pixels and `textKey` sizes points, `textStyle.size` holding DOM `size` times 72 over resolution

## [07]-[PLUGINS]

Manifest host data decides when Photoshop loads a plugin and which calls need a scope:

| [INDEX] | [KEY]                     | [EFFECT]                                                                                          |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `host.data.apiVersion: 2` | Changes to documents, interface, or preferences need a modal scope, default at every `minVersion` |
|  [02]   | `host.data.loadEvent`     | `main` runs at launch under `"startup"`, at first use under `"use"`                               |

- `host.data.enableMenuRecording` records `command` entrypoints as Action steps
- Load runs panel `create` and `show`, and quit runs panel `hide`, panel `destroy`, then plugin `destroy`, whether the panel shows or not
- Showing or hiding a panel calls no handler, and its `uxpcommand` event holds `commandId` `uxpshowpanel` or `uxphidepanel`
- `.psjs` scripts run once users confirm a JavaScript load warning, write absolute `fs` paths, and throw `EvalError` for `eval` and `new Function`
- `require('uxp').script.executionContext` of a `.psjs` script holds `hostControl`, `reportProgress`, and `onCancel`
- `Object.keys(require('uxp').script)` reads empty
- `~/Library/Logs/Adobe/<app name>/UXPLogs_*.log` records each plugin load and each rejection no `unhandledRejection` flag delivers
