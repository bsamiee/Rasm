# [PHOTOSHOP]

Photoshop's UXP DOM uses Action Manager descriptors: `action.batchPlay` plays changes inside one modal scope and reads anywhere, replying with one descriptor per command. Native Apple Event `do javascript` runs ExtendScript.

## [01]-[MODAL_SCOPES]

`core.executeAsModal` holds Photoshop's one modal scope for a body, and a refused command resolves as an error descriptor in the reply array:

```ts
import { type ActionDescriptor, action, core, type ExecutionContext } from 'adobe:photoshop';

const Failed = Schema.Struct({ _obj: Schema.Literal('error'), message: Schema.String, result: Schema.Int });
const played = (commands: ActionDescriptor[]): Effect.Effect<ActionDescriptor[], Cause.UnknownError> =>
    Effect.flatMap(Effect.tryPromise(() => action.batchPlay(commands, {})), (replies) =>
        Option.match(
            Array.findFirst(Array.zip(commands, replies), ([{ _obj: command }, reply]) => Option.map(Schema.decodeUnknownOption(Failed)(reply), (failed) => new Cause.UnknownError(failed, command))),
            { onNone: () => Effect.succeed(replies), onSome: Effect.fail },
        ),
    );
const modal = <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>): Effect.Effect<A, E | Cause.UnknownError, R> =>
    Effect.flatMap(Effect.context<R>(), (services) =>
        Effect.flatten(
            Effect.tryPromise({
                try: (signal) => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(body(context), { signal }), { commandName }),
                catch: (cause) => new Cause.UnknownError(cause, commandName),
            }),
        ),
    );
const committed = (documentID: number, name: string, commands: ActionDescriptor[]): Effect.Effect<ActionDescriptor[], Cause.UnknownError> =>
    modal(name, ({ hostControl }) =>
        Effect.scoped(
            Effect.andThen(
                Effect.acquireRelease(Effect.tryPromise(() => hostControl.suspendHistory({ documentID, name })), (suspension, exit) =>
                    Effect.promise(() => hostControl.resumeHistory(suspension, Exit.isSuccess(exit))),
                ),
                played(commands),
            ),
        ),
    );
```

`committed` plays a command list as one history state named `name`, or fails with the first refused command's error descriptor as cause and leaves no change. `resumeHistory` commits on the body's success and drops every change on failure or interruption, since a suspension left open commits when its body resolves. `modal` runs its body on the caller's services and passes its `signal`, and an interrupted caller interrupts the body. The body returns its `Exit` as a value that `Effect.flatten` unwraps.

| [INDEX] | [CALL]                                                    | [REPLY]                                                         |
| :-----: | :-------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Command, DOM setter, or method outside a scope            | Throws `number` 1000 naming the first event it plays            |
|  [02]   | Unknown command, or `get`, `set`, `select` of no element  | `result` -25920, reply array ends at that command               |
|  [03]   | `hide` of no element                                      | `result` -25922                                                 |
|  [04]   | `multiGet` of a closed document or a missing property     | `result` -25923                                                 |
|  [05]   | Dialog cancelled, or refusal alert closed under `display` | `result` -128 with an empty `message`                           |
|  [06]   | Modal scope another plug-in holds                         | Rejects `number` 9 after `timeOut` (1 s default) unless freed   |
|  [07]   | `executeAsModal` inside a body                            | Runs nested in its outer scope with a context of its own        |
|  [08]   | Cancel on the progress window                             | `onCancel` runs, awaited plays throw, rejects `number` 2        |
|  [09]   | Body rejecting with an `Error`                            | `executeAsModal` rejects with its string, other values as given |

- `resumeHistory({ ...suspension, finalName }, true)` renames the state at commit, and a suspension holding no change records no state
- Plays outside a suspension record one state each under Photoshop's name (`New Layer`, `Master Opacity Change`) and stay when their body throws
- `Document.suspendHistory(callback, name)` wraps both calls in `executeAsModal`, passes `document` for `hostControl`, and resolves `undefined`
- `interactive: true` scopes open `display` dialogs and no progress window, which other scopes open after about 2 s and fill from `reportProgress`
- `@adobe-uxp-types/photoshop` types `executeAsModal` as its target's value, which it resolves with, and `@types/photoshop` as `Promise<void>`

## [02]-[DESCRIPTORS]

Descriptors are plain records in Action JSON, and one `multiGet` returns named properties of every element under a target:

```ts
const Listed = Schema.Tuple([
    Schema.Struct({ list: Schema.Array(Schema.Struct({ layerID: Schema.Int, name: Schema.String, visible: Schema.Boolean })) }),
]);
const layers = (documentID: number) =>
    played([
        {
            _obj: 'multiGet',
            _target: { _ref: 'document', _id: documentID },
            extendedReference: [['layerID', 'name', 'visible'], { _obj: 'layer', index: 1, count: -1 }],
            options: { failOnMissingProperty: false, failOnMissingElement: false },
        },
    ]).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Listed)),
        Effect.map(([{ list }]) => list),
    );
```

`layers` runs outside every modal scope and fails with an error descriptor as cause for a closed document. `index: 1` starts above a background layer, `count: -1` runs to the last element, and `failOnMissingProperty: false` leaves an absent property out of its record.

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
- `batchPlay(commands, { synchronousExecution: true })` returns the reply array in place of a promise, the form DOM getters play
- DOM members play with `app.displayDialogs` as `dialogOptions`, `DialogModes.ERROR` (`'silent'`) by default

## [03]-[DOM]

DOM members are JavaScript over descriptors: a getter plays a synchronous `get`, and a setter or method plays `saveDocumentSelection`, a `select` of its target, and its command in one batch, which leaves the selection as it was.

| [INDEX] | [NEED]                        | [FORM]                                       | [DECIDING_FACT]                                    |
| :-----: | :---------------------------- | :------------------------------------------- | :------------------------------------------------- |
|  [01]   | Many properties, many targets | One `multiGet`, getters for one read         | One call reads every target                        |
|  [02]   | Enumeration comparison        | `constants` member under `===`               | Strings, numbers in `SaveOptions` and `SampleSize` |
|  [03]   | Preference read or write      | `app.preferences` member, writes in a scope  | Members play an application `get` or `set`         |
|  [04]   | Tool selection                | `select` of `{ _ref: <tool ID> }` in a scope | `app.currentTool` is a getter of a `Tool`          |
|  [05]   | Element by name               | `getByName`, then a null check               | Returns `null` for no match                        |
|  [06]   | Element held across edits     | Its DOM object or `id`                       | One object per id, members of a deleted one throw  |

- `document.layers` and a group's `layers` hold the ids one `getLayerTreeSync` or `getLayerGroupContentsSync` call read
- `getByName` on a layer list gets each layer's `name`
- `app.documents` and `app.fonts` play a get at each `length` and index read, and an array method on either plays one list read
- Preference members reach a subset of application keys, and a `get` of the application property reaches the rest (`guidesPrefs`, `interfacePrefs`)
- Unlocking a background layer gives it a new `id`, which its DOM object takes

Failures leave each member in its own form:

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

`action.addNotificationListener(events, handler)` calls `handler(event, descriptor)` for each listed event name, and `removeNotificationListener` with the same names and function removes it:
- User commands arrive with `_isCommand: true` in the descriptor
- Plays from a plug-in's own modal scope, interactive included, and plays from ExtendScript reach no listener of that plug-in
- `core.addNotificationListener('UI', events, handler)` delivers interface events, and group `'OS'` system ones (`displayConfigurationChanged`)
- `panelVisibilityChanged` holds `panelID` and `visible` for every panel

## [05]-[EXPORT]

`document.saveAs` holds `psd`, `psb`, `jpg`, `gif`, `png`, and `bmp`, each taking `(file, options, asCopy)` inside a modal scope with a UXP `File` entry:
- `localFileSystem.getEntryWithUrl('file:<folder>')`, then `createFile(<name>, { overwrite: true })`, gives the entry under `fullAccess`
- `asCopy` true writes the file and leaves `title` and `path` unchanged
- `close()` opens a save dialog under default `SaveOptions.PROMPTTOSAVECHANGES`, and `closeWithoutSaving()` in a scope discards changes synchronously
- `hostControl.registerAutoCloseDocument(documentID)` closes a working document unsaved when its scope ends
- `save` descriptors take `in: { _path: localFileSystem.createSessionToken(file), _kind: 'local' }`, and plain paths reject "invalid file token used"
- `exportmanager`, which neither typing declares, serves any plug-in outside a modal scope
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

## [07]-[PLUG_INS]

Manifest host data decides when Photoshop loads a plug-in and which calls need a scope:

| [INDEX] | [KEY]                     | [EFFECT]                                                                                          |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `host.data.apiVersion: 2` | Changes to documents, interface, or preferences need a modal scope, default at every `minVersion` |
|  [02]   | `host.data.loadEvent`     | `main` runs at launch under `"startup"`, at first use under `"use"`                               |

- Load runs panel `create` and `show`, and quit runs panel `hide`, panel `destroy`, then plugin `destroy`, whether the panel shows or not
- Showing or hiding a panel calls no handler, and its `uxpcommand` event holds `commandId` `uxpshowpanel` or `uxphidepanel`
- `.psjs` scripts run once users confirm a JavaScript load warning, write absolute `fs` paths, and throw `EvalError` for `eval` and `new Function`
- `require('uxp').script.executionContext` of a `.psjs` script holds `hostControl`, `reportProgress`, and `onCancel`
- `Object.keys(require('uxp').script)` reads empty
- `~/Library/Logs/Adobe/<app name>/UXPLogs_*.log` records each plug-in load and each rejection no `unhandledRejection` flag delivers

## [08]-[MEMBERS]

Installed builds bundle their DOM as JavaScript that `@types/photoshop` declares, and one syntax-tree read of both prints a class:

```bash
rule="id: built
language: javascript
rule: {kind: method_definition, inside: {kind: class_body, inside: {kind: class_declaration, has: {field: name, regex: ^<Class>$}}}}
---
id: declared
language: tsx
rule: {any: [{kind: method_signature}, {kind: public_field_definition}], inside: {kind: class_body, inside: {kind: class_declaration, has: {field: name, regex: ^<Class>$}}}}"
ast-grep scan --custom-languages allow --inline-rules "$rule" --json=stream "<app>/Contents/Required/UXP/app.js" node_modules/@types/photoshop/dom/<Class>.d.ts |
    jq -rs 'map(.text |= gsub("\\s+"; " ") | . + (.text | capture("^(?<kind>(?:(?:private|static|async|get|set) )*)\\*?(?<name>[\\w$]+)(?<params>\\([^{]*\\))?")))
    | group_by(.name)[] | (map(select(.ruleId == "built")) | select(length > 0)) as $built
    | [.[0].name, ($built | map(.kind) | if any(test("set ")) then "readwrite" elif any(test("get ")) then "readonly" else "method" end),
        (map(select(.ruleId == "declared"))[0].text // "undeclared \($built[0].params)")]
    | @tsv'
```

```text
blendMode	readwrite	get blendMode(): Constants.BlendMode
moveAbove	method	undeclared (e)
name	readwrite	get name(): string
```

- Rows list what the installed build holds, and a member the declarations lack reads `undeclared` with its parameters
- Members' `app.js` bodies name their plays
- `rg -o 'class \w+' "<app>/Contents/Required/UXP/app.js"` lists every class, and `constants.js` beside it holds each `constants` group's values
- `strings -n 6 "<app>/Contents/MacOS/<executable>" | rg -o -m1 '/<event> \[\(.*?\)[^<\]]*(<<.*?>>)?\]'` prints a command's terminology keys
- Terminology keys are the Actions panel's vocabulary, typed as scalar, class, enumerated, or unit, and `/list` marks a list
