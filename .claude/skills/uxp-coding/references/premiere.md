# [PREMIERE_PRO]

Premiere Pro's module is a record of static classes, and its async reads complete before any lock opens.

## [01]-[TRANSACTIONS]

Actions are built and added inside synchronous callbacks, and each host boolean leaves as data that becomes a reason:

```ts
import type { ProjectItem, premierepro } from '@adobe/premierepro';

const EditReason = Schema.TaggedUnion({
    Unread: { cause: Schema.Defect() },
    Refused: { undo: Schema.String, names: Schema.NonEmptyArray(Schema.String) },
    Unexecuted: { undo: Schema.String },
    Thrown: { undo: Schema.String, cause: Schema.Defect() },
});
class EditError extends Schema.TaggedError<EditError, { readonly brand: unique symbol }>()('EditError', { reason: EditReason }) {}

const read = <A>(call: () => Promise<A>): Effect.Effect<A, EditError> =>
    Effect.tryPromise({ try: call, catch: (cause) => EditError.make({ reason: { _tag: 'Unread', cause } }) });
const renamed = Effect.fn('renamed')(function* (host: premierepro, undo: string, name: (item: ProjectItem) => string) {
    const project = yield* read(() => host.Project.getActiveProject());
    const selection = yield* read(() => host.ProjectUtils.getSelection(project));
    const items = yield* read(() => selection.getItems());
    const ran = yield* Effect.try({
        try: () => {
            let [refused, executed] = [Array.empty<string>(), false];
            project.lockedAccess(() => {
                executed = project.executeTransaction((compound) => {
                    const actions = Array.map(items, (item) => [item.name, item.createSetNameAction(name(item))] as const);
                    refused = Array.filterMap(actions, ([label, action]) => (compound.addAction(action) ? Result.failVoid : Result.succeed(label)));
                }, undo);
            });
            return { refused, executed };
        },
        catch: (cause) => EditError.make({ reason: { _tag: 'Thrown', undo, cause } }),
    });
    yield* Array.match(ran.refused, {
        onEmpty: () => Effect.void,
        onNonEmpty: (names) => Effect.fail(EditError.make({ reason: { _tag: 'Refused', undo, names } })),
    });
    return yield* ran.executed ? Effect.succeed(items.length) : Effect.fail(EditError.make({ reason: { _tag: 'Unexecuted', undo } }));
});
```

`renamed` returns its count of renamed items, and `host` is the module object `require` returns. Reads finish before `lockedAccess`, and actions are built before the first `addAction`, where a throwing `name` adds none.

| [INDEX] | [CALL]                                     | [RESULT]                                        |
| :-----: | :----------------------------------------- | :---------------------------------------------- |
|  [01]   | `executeTransaction` inside `lockedAccess` | `true` once its actions executed, or with none  |
|  [02]   | `executeTransaction` outside a lock        | Throws                                          |
|  [03]   | `compound.addAction(action)`               | `false` for an action it refuses                |
|  [04]   | Throw inside a transaction callback        | Reaches its caller, added actions still execute |
|  [05]   | Async getter inside a callback             | Pending promise no callback can await           |

- `Refused` names each item `addAction` refused beside a transaction that executed every other action
- `create*Action` members return synchronously, run inside the lock, and change nothing until their transaction executes

## [02]-[READS]

Getters return base types, and each class's static `cast` narrows one or returns `null` for another kind:

```ts
import type { FolderItem, Project, premierepro } from '@adobe/premierepro';

const folders = (host: premierepro, project: Project): Effect.Effect<FolderItem[], EditError> =>
    Effect.map(
        Effect.flatMap(
            read(() => project.getRootItem()),
            (root) => read(() => root.getItems()),
        ),
        (items) => Array.getSomes(Array.map(items, (item) => Option.fromNullishOr(host.FolderItem.cast(item)))),
    );
const dirtied = <E>(
    host: premierepro,
    project: Project,
    work: (event: object | undefined) => Effect.Effect<void, E>,
): Effect.Effect<void, never, Scope.Scope> =>
    listening(
        (handler: (event?: object) => void) => host.EventManager.addEventListener(project, host.Constants.ProjectEvent.DIRTY, handler),
        (handler) => host.EventManager.removeEventListener(project, host.Constants.ProjectEvent.DIRTY, handler),
        work,
    );
```

`folders` returns every bin under a project's root through `Option.fromNullishOr` over `cast`, which typings declare without `null`. `dirtied` holds a project's `DIRTY` registration in a scope through `listening`.

| [INDEX] | [NEED]         | [FORM]                                     | [DECIDING_FACT]                             |
| :-----: | :------------- | :----------------------------------------- | :------------------------------------------ |
|  [01]   | Active project | `await Project.getActiveProject()`         | Static async getter                         |
|  [02]   | Bin contents   | `FolderItem.cast(item)`, then `getItems()` | `ProjectItem` holds `name` and `type` alone |
|  [03]   | Clip members   | `ClipProjectItem.cast(item)`               | Returns `null` for a bin                    |
|  [04]   | Item kind      | `item.type` against `ProjectItem.TYPE_BIN` | Statics hold each kind's number             |
|  [05]   | Selection      | `ProjectUtils.getSelection(project)`       | Holds a new item once its panel updates     |

- Reads of independent objects combine through `Effect.all` over `read` calls
- Work that names new items reads them from their folder

## [03]-[EVENTS]

`EventManager` registers a handler on one target for one event name:
- Targets are `Project`, `Sequence`, `VideoTrack`, `AudioTrack`, and `EncoderManager`
- Events are string members of `Constants.<Group>Event`: `Project`, `Sequence`, `VideoTrack`, `AudioTrack`, `Encoder`, `Snap`, and `OperationComplete`
- One executed transaction can raise more than one `DIRTY` event, and handlers hold work that tolerates repeats

## [04]-[PROJECTS]

Projects open, return their root, and close through static and instance calls:

| [INDEX] | [STEP]      | [CALL]                                                         | [RULE]                              |
| :-----: | :---------- | :------------------------------------------------------------- | :---------------------------------- |
|  [01]   | Create      | `await Project.createProject(path)`                            | New project becomes active          |
|  [02]   | Root        | `await project.getRootItem()`                                  | Returns a `FolderItem`              |
|  [03]   | Close       | `project.close(CloseProjectOptions().setPromptIfDirty(false))` | Close opens a save prompt otherwise |
|  [04]   | After close | `await Project.getActiveProject()`                             | Returns the project open before     |

- Media Encoder loads the same module form under manifest host `ame`

## [05]-[MEMBERS]

Installed builds bundle a manifest of the module's classes and an allow-list naming what the module exposes, and one `jq` read prints each class:

```bash
jq -r --arg c <Class> --slurpfile allow "<app>/Contents/Resources/json/api_config.json" '
    ($allow[0].apis[] | select(has($c)) | .[$c]) as $names
    | .[] | select(.name == $c) | (.properties[], .methods[]) | select(.name | IN($names[]))
    | (if has("params") then "(\([.params[] | "\(.name)\(if .optional == "true" then "?" else "" end): \(.dataType)"] | join(", ")))" else "" end)
        as $params
    | ["\(.name)\($params): \(.dataType)", .propKind, .propType // (if .async == "true" then "async" else "sync" end)]
    | @tsv' "<app>/Contents/Resources/json/ScriptingAPIManifest.json"
```

```text
TYPE_BIN: number	class	readOnly
createSetNameAction(inName: string): Action	instance	sync
getProject(): Project	instance	async
```

- `jq -r '.apis[] | keys[] | select(. != "hash")' "<app>/Contents/Resources/json/api_config.json"` lists every exposed class
- `class` rows are static members and constants, and `async` rows return promises
- `lockedAccess`, `executeTransaction`, and the event-target members come from the module's own JavaScript and are typed by `@adobe/premierepro` alone
- Manifests type each enumeration member `number` or `string` with no value
