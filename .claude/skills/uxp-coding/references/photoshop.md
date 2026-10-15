# [PHOTOSHOP]

Photoshop's DOM is a layer over Action Manager descriptors: `action.batchPlay` plays each change as a command inside one modal scope and returns each read as a descriptor outside one.

## [01]-[MODAL_SCOPES]

`core.executeAsModal` holds Photoshop's one modal scope for a body, and a refused command resolves as an error descriptor in the reply array:

```ts
import { type ActionDescriptor, action, core, type ExecutionContext } from 'adobe:photoshop';

const PlayReason = Schema.TaggedUnion({
    Refused: { command: Schema.String, message: Schema.String, result: Schema.Int },
    Cancelled: { command: Schema.String },
    Busy: { message: Schema.String },
    Thrown: { cause: Schema.Defect() },
});
class PlayError extends Schema.TaggedError<PlayError>()('PlayError', { reason: PlayReason }) {}

const { message, result } = PlayReason.cases.Refused.fields;
const Failed = Schema.Struct({ _obj: Schema.Literal('error'), message, result });
const thrown = (cause: unknown): PlayError => new PlayError({ reason: { _tag: 'Thrown', cause } });
const played = (commands: ActionDescriptor[]): Effect.Effect<ActionDescriptor[], PlayError> =>
    Effect.flatMap(Effect.tryPromise({ try: () => action.batchPlay(commands, {}), catch: thrown }), (replies) => {
        const userCancelled = -128;
        const refused = Array.findFirst(Array.zip(commands, replies), ([{ _obj: command }, reply]) =>
            Option.map(Schema.decodeUnknownOption(Failed)(reply), (failed) =>
                failed.result === userCancelled
                    ? { _tag: 'Cancelled' as const, command }
                    : { _tag: 'Refused' as const, command, message: failed.message, result: failed.result },
            ),
        );
        return Option.match(refused, { onNone: () => Effect.succeed(replies), onSome: (reason) => Effect.fail(new PlayError({ reason })) });
    });
const modal = <A, E, R>(commandName: string, body: (context: ExecutionContext) => Effect.Effect<A, E, R>): Effect.Effect<A, E | PlayError, R> => {
    const busyCode = 9;
    const Busy = Schema.Struct({ number: Schema.Literal(busyCode), message });
    return Effect.flatMap(Effect.context<R>(), (services) =>
        Effect.flatten(
            Effect.tryPromise({
                try: (signal) => core.executeAsModal((context) => Effect.runPromiseExitWith(services)(body(context), { signal }), { commandName }),
                catch: (cause) =>
                    Option.match(Schema.decodeUnknownOption(Busy)(cause), {
                        onNone: () => thrown(cause),
                        onSome: (busy) => new PlayError({ reason: { _tag: 'Busy', message: busy.message } }),
                    }),
            }),
        ),
    );
};
const committed = (documentID: number, name: string, commands: ActionDescriptor[]): Effect.Effect<ActionDescriptor[], PlayError> =>
    modal(name, ({ hostControl }) =>
        Effect.scoped(
            Effect.andThen(
                Effect.acquireRelease(
                    Effect.tryPromise({ try: () => hostControl.suspendHistory({ documentID, name }), catch: thrown }),
                    (suspension, exit) => Effect.promise(() => hostControl.resumeHistory(suspension, Exit.isSuccess(exit))),
                ),
                played(commands),
            ),
        ),
    );
```

`committed` plays a command list as one history state named `name`, or fails with the first refused command and leaves no change: `resumeHistory` commits on the body's success and drops every change on failure or interruption. `Failed` takes `message` and `result` from the `Refused` case, and `Busy` takes `message`. `modal` runs its body on the caller's services and passes its `signal`, and an interrupted caller interrupts the body. The body returns its `Exit` as a value that `Effect.flatten` unwraps, since a rejected body reaches its caller with no `message` or `number`.

| [INDEX] | [CALL]                                         | [REPLY]                                                   |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Command, DOM setter, or method outside a scope | Throws `number` 1000 naming the event it plays            |
|  [02]   | Unknown command                                | Error descriptor, `result` -25920, later commands skipped |
|  [03]   | Reference that resolves nothing                | Error descriptor, `result` -25923                         |
|  [04]   | User cancel                                    | Error descriptor, `result` -128                           |
|  [05]   | Modal scope another plug-in holds              | `executeAsModal` rejects with `number` 9                  |
|  [06]   | `executeAsModal` inside a body                 | Runs nested in its outer scope                            |
|  [07]   | Long body                                      | `isCancelled` reads true at an awaited host call          |

## [02]-[DESCRIPTORS]

Descriptors are plain records in Action JSON, and one `multiGet` returns named properties of every element under a target:

```ts
const Listed = Schema.Tuple([
    Schema.Struct({ list: Schema.Array(Schema.Struct({ layerID: Schema.Int, name: Schema.String, visible: Schema.Boolean })) }),
]);
const layers = (documentID: number): Effect.Effect<(typeof Listed.Type)[0]['list'], PlayError | Schema.SchemaError> =>
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

`layers` runs outside every modal scope and fails `Refused` for a closed document. `index: 1` starts above a background layer, `count: -1` runs to the last element, and `failOnMissingProperty: false` leaves an absent property out of its record.

| [INDEX] | [VALUE]     | [FORM]                                                             |
| :-----: | :---------- | :----------------------------------------------------------------- |
|  [01]   | Object      | `{ _obj: <class>, <key>: <value> }`                                |
|  [02]   | Enumeration | `{ _enum: <type>, _value: <member> }`                              |
|  [03]   | Unit number | `{ _unit: <unit>, _value: <number> }`                              |
|  [04]   | Reference   | `{ _ref: <class>, _id: <id> }`, outermost container last           |
|  [05]   | Property    | `[{ _property: <key> }, { _ref: <class>, _id: <id> }]` under `get` |
|  [06]   | Target      | `{ _ref: <class>, _enum: 'ordinal', _value: 'targetEnum' }`        |
|  [07]   | Silent run  | `_options: { dialogOptions: 'silent' }` on a command with a dialog |

- `_id` names an element across edits, where `_index` and `_name` change as a user edits
- Units are `pixelsUnit`, `pointsUnit`, `percentUnit`, `angleUnit`, `densityUnit`, `distanceUnit`, and `millimetersUnit`
- Keys are string IDs, and `get` returns keys the Actions panel never records (`targetLayers`, `layerKind`, `documentID`)
- Each `set` holds only the keys it changes, and keys one preference couples (a color enum and its custom color) go in one descriptor
- Commands a plug-in plays inside a modal scope reach no notification listener of that plug-in

## [03]-[DOM]

DOM members are JavaScript over descriptors: a getter plays a synchronous `get`, and a setter or method plays its command and throws on an error descriptor.

| [INDEX] | [NEED]                        | [FORM]                                  | [DECIDING_FACT]                        |
| :-----: | :---------------------------- | :-------------------------------------- | :------------------------------------- |
|  [01]   | One read of one object        | DOM getter                              | One synchronous play per read          |
|  [02]   | Many properties, many targets | One `multiGet`                          | One call reads every target            |
|  [03]   | Enumeration comparison        | `constants` member under `===`          | Members are strings                    |
|  [04]   | One named history state       | `suspendHistory` scope around its plays | Each play is its own state otherwise   |
|  [05]   | Recovery inside a body        | `Effect.catchReason` on one reason      | Cancelled bodies stop at `Cancelled`   |
|  [06]   | Preference read or write      | `app.preferences` member inside a scope | Each member plays an application `set` |
|  [07]   | Tool selection                | `app.currentTool` with a tool class ID  | Writes a string ID, reads it back      |

- `Document.suspendHistory(callback, name)` is the DOM's own `executeAsModal` around `hostControl.suspendHistory` and `resumeHistory`
- `@adobe-uxp-types/photoshop` types `executeAsModal` as its target's value, and `@types/photoshop` as `Promise<void>`
- Preference members reach a subset of application keys, and a `get` of the application property reaches the rest (`guidesPrefs`, `interfacePrefs`)

## [04]-[PLUG_INS]

Manifest host data decides when Photoshop loads a plug-in and which calls need a scope:

| [INDEX] | [KEY]                            | [EFFECT]                                                                 |
| :-----: | :------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `host.data.apiVersion: 2`        | Every change to documents, interface, or preferences needs a modal scope |
|  [02]   | `host.data.loadEvent: "startup"` | `main` runs at launch                                                    |
|  [03]   | `host.data.loadEvent: "use"`     | `main` runs at the plug-in's first use                                   |
|  [04]   | `host.app: "PS"`                 | Registry file `PS.json` lists the plug-in                                |

- Registry rows hold `hostMinVersion`, `name`, `path`, `pluginId`, `status`, `type`, and `versionString`, and no other key
- Row `path` reads `$localPlugins/External/<pluginId>_<versionString>`, a folder under `UXP/Plugins/External`
- Adobe's plug-in installer serves release hosts, and a Beta host loads a folder and registry row written beside it
- Plug-in data sits under `UXP/PluginsStorage/<product>/<major>/External/<pluginId>/PluginData`

## [05]-[MEMBERS]

Installed builds bundle their DOM as JavaScript that `@types/photoshop` declares, and one syntax-tree read joined with each declaration prints a class:

```bash
rule="id: members
language: javascript
rule: {kind: method_definition, inside: {kind: class_body, inside: {kind: class_declaration, has: {field: name, regex: ^<Class>$}}}}"
ast-grep scan --custom-languages allow --inline-rules "$rule" --json=stream "<app>/Contents/Required/UXP/app.js" |
    jq -rs --rawfile declared node_modules/@types/photoshop/dom/<Class>.d.ts '
    ($declared | [scan("(?m)^ {4}(?:static |readonly )*(?:(get) )?(\\w+)\\??(\\(.*\\))?: (.+);$")]
        | map({key: .[1], value: (if .[0] or (.[2] | not) then .[3] else "\(.[2]): \(.[3])" end)}) | from_entries) as $declared
    | map(.text | capture("^(?<kind>(?:static |async |get |set )*)\\*?(?<name>[\\w$]+)(?<params>\\([^{]*\\))")) | group_by(.name)[]
    | [.[0].name, (map(.kind) | if any(test("set ")) then "readwrite" elif any(test("get ")) then "readonly" else "method" end),
        $declared[.[0].name] // "undeclared \(.[0].params)"]
    | @tsv'
```

```text
blendMode	readwrite	Constants.BlendMode
moveAbove	method	undeclared (e)
name	readwrite	string
```

- Rows list what the installed build holds: a member newer than the declarations reads `undeclared` with its parameter list
- Undeclared members' bodies in `app.js` name the descriptors they play
- `rg -o 'class \w+' "<app>/Contents/Required/UXP/app.js"` lists every class, and `constants.js` beside it holds each `constants` group's values
- `strings -n 6 "<app>/Contents/MacOS/<executable>" | rg -o '/<event> \[[^]]*\]'` prints a command's terminology keys, `/list` marking a list
- Terminology keys are the Actions panel's vocabulary, typed as scalar, class, enumerated, or unit
