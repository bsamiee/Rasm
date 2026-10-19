# [PREMIERE_PRO]

Premiere Pro's module is a record of native classes and members its own JavaScript adds, and its async reads complete before any lock opens.

## [01]-[MEMBERS]

Installed builds bundle a manifest of native classes and an allow-list naming what the module exposes, and one `jq` read prints each class:

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
- `class` rows are static members and constants, typed `number` or `string` with no value, and `async` rows return promises
- Members module JavaScript adds (`lockedAccess`, `executeTransaction`, `cast`, `EventManager`, `Constants`) sit in typings and not the manifest
- `strings -n 3000 "<app>/Contents/MacOS/<executable>" | rg -F mediaCoreBackend` prints module JavaScript

## [02]-[READS]

Getters return base classes, and each class's static `cast` narrows one or returns `null` for another kind:

```ts
import type { Project, premierepro } from '@adobe/premierepro';

const folders = (host: premierepro, project: Project) =>
    Effect.tryPromise(() => project.getRootItem()).pipe(
        Effect.flatMap((root) => Effect.tryPromise(() => root.getItems())),
        Effect.map(Array.flatMap((item) => Array.fromNullishOr(host.FolderItem.cast(item)))),
    );
```

`folders` returns the bins in a project's root through `Array.fromNullishOr` over `cast`, which typings declare without `null`, and a rejected read fails `Cause.UnknownError` with the rejection as cause. `host` is the module object `require` returns.

| [INDEX] | [NEED]         | [FORM]                                                 | [DECIDING_FACT]                                 |
| :-----: | :------------- | :----------------------------------------------------- | :---------------------------------------------- |
|  [01]   | Active project | `await Project.getActiveProject()`                     | `null` with no project open, one object each    |
|  [02]   | Bin contents   | `FolderItem.cast(item)`, then `getItems()`             | Items are `ProjectItem` with `name` and `type`  |
|  [03]   | Clip members   | `ClipProjectItem.cast(item)`                           | Returns `null` for a bin                        |
|  [04]   | Item kind      | `item.type` against `ProjectItem.TYPE_BIN`             | Sequences read `TYPE_CLIP`                      |
|  [05]   | Selection      | `ProjectUtils.getSelection(project)`, `getItems()`     | Holds imported items, misses a bin action's bin |
|  [06]   | Track items    | `track.getTrackItems(TrackItemType.CLIP, false)`       | Synchronous, concrete clip track item classes   |
|  [07]   | Markers        | `await Markers.getMarkers(owner)`, then `getMarkers()` | `null` for an owner other than sequence or clip |

- Work that names new items reads them from their folder
- Handles read live state, and a track item or `Markers` read before an edit returns the edited value
- `TrackItemType` is a `Constants` group, and `Marker` getters are synchronous

## [03]-[TRANSACTIONS]

Edits are actions built and added inside one `lockedAccess` callback, and `addAction` alone tells an executed action from a refused one:

```ts
import type { Project, ProjectItem } from '@adobe/premierepro';

const renamed = (project: Project, items: readonly ProjectItem[], undo: string, name: (item: ProjectItem) => string): Effect.Effect<number, Cause.UnknownError> =>
    Effect.try({
        try: () => {
            const refused: string[] = [];
            project.lockedAccess(() => {
                const actions = items.map((item) => [item.name, item.createSetNameAction(name(item))] as const);
                project.executeTransaction((compound) => {
                    refused.push(...actions.flatMap(([label, action]) => (compound.addAction(action) ? [] : [label])));
                }, undo);
            });
            return refused;
        },
        catch: (cause) => new Cause.UnknownError(cause, undo),
    }).pipe(
        Effect.flatMap(
            Array.match({
                onEmpty: () => Effect.succeed(items.length),
                onNonEmpty: (refused) => Effect.fail(new Cause.UnknownError({ refused }, undo)),
            }),
        ),
    );
```

`renamed` returns its count over items its caller read before the lock, or fails with `refused` naming each item `addAction` refused beside a transaction that executed every other action. Actions build before the first `addAction`, where a throwing `name` adds none, and a throw from inside the transaction callback follows the actions added before it, which still execute. `refused` leaves through an outer array, since `lockedAccess` returns `true` and drops its callback's value.

| [INDEX] | [CALL]                                      | [RESULT]                                                    |
| :-----: | :------------------------------------------ | :---------------------------------------------------------- |
|  [01]   | `create*Action` outside `lockedAccess`      | Throws, inside a transaction callback too                   |
|  [02]   | `executeTransaction(callback, undo)`        | `true`, one History entry named `undo`, whatever executed   |
|  [03]   | `executeTransaction` with no `undo`         | History entry "Edit Project"                                |
|  [04]   | Action built in another `lockedAccess`      | `addAction` returns `false`                                 |
|  [05]   | `addAction(null)`, an edit the host refuses | `false`                                                     |
|  [06]   | `addAction` of a non-action                 | Throws "Illegal Parameter type"                             |
|  [07]   | Throw inside a transaction callback         | Reaches its caller, added actions still execute             |
|  [08]   | Async getter inside a callback              | Pending promise no callback can await                       |
|  [09]   | Transaction inside a transaction callback   | Own History entry, its actions execute before the outer one |

- `create*Action` members return synchronously and change nothing until their transaction executes

## [04]-[EVENTS]

`EventManager.addEventListener(target, name, handler)` calls `subscribeToEvent` for track and render events, then the target's own `addEventListener`, and `addGlobalEventListener` listens on the event root:

```ts
import type { Project, premierepro } from '@adobe/premierepro';

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

`dirtied` holds a project's `DIRTY` registration in a scope through `listening`. Handlers receive an event whose `type` equals its name, with no other field.

| [INDEX] | [EVENT]                              | [TARGET]                      | [RAISED_BY]                                         |
| :-----: | :----------------------------------- | :---------------------------- | :-------------------------------------------------- |
|  [01]   | `ProjectEvent.DIRTY`                 | `Project`                     | Each executed transaction, repeatedly, after return |
|  [02]   | `ProjectEvent.OPENED`, `CLOSED`      | Global listener               | `createProject`, `open`, and `close`                |
|  [03]   | `VideoTrackEvent`, `AudioTrackEvent` | `VideoTrack`, `AudioTrack`    | Edits to items on that track                        |
|  [04]   | `EncoderEvent`                       | `EncoderManager.getManager()` | Nothing for an `IMMEDIATELY` export                 |

- `Constants` groups hold host strings (`"ProjectEvent.Dirty"`) where typings declare numeric enums

## [05]-[PROJECTS]

Projects open, return their root, save, and close through static and instance calls:

| [INDEX] | [STEP]        | [CALL]                                                    | [RULE]                                       |
| :-----: | :------------ | :-------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Create        | `await Project.createProject(path)`                       | New project becomes active                   |
|  [02]   | Root          | `await project.getRootItem()`                             | Returns a `FolderItem`                       |
|  [03]   | Import        | `await project.importFiles(paths, true)`                  | Items land in the root, selected             |
|  [04]   | Sequence      | `await project.createSequenceFromMedia(name, clips, bin)` | New sequence becomes active                  |
|  [05]   | Save          | `await project.save()`                                    | `true` once written                          |
|  [06]   | Close         | `await project.close()`                                   | Discards unsaved changes with no prompt      |
|  [07]   | After close   | `await Project.getActiveProject()`                        | Returns the project open before, same object |
|  [08]   | Closed handle | Reads and transactions on a closed project                | Still answer, `true` from each transaction   |

- `CloseProjectOptions()` builds without `new`, starts with `promptIfDirty` false, and each setter returns the same object
- Save and export show a progress dialog that closes when their promise resolves

## [06]-[EXPORT]

Files export through the encoder `EncoderManager.getManager()` returns, and frames through `Exporter`:

| [INDEX] | [OUTPUT]       | [CALL]                                                                           | [RESULT]                  |
| :-----: | :------------- | :------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | Sequence file  | `exportSequence(sequence, Constants.ExportType.IMMEDIATELY, path, preset, true)` | `true` with file complete |
|  [02]   | File extension | `EncoderManager.getExportFileExtension(sequence, preset)`                        | Extension preset writes   |
|  [03]   | One frame      | `Exporter.exportSequenceFrame(sequence, time, name, folder, width, height)`      | Writes `<folder><name>`   |

- Presets are `.epr` files under `<app>/Contents/Settings/EncoderPresets` and `Settings/Frame.io/EncoderPresets`
- `TickTime.createWithSeconds` builds times, and `ticks` reads 254016000000 per second as a string

## [07]-[MEDIA_ENCODER]

Media Encoder loads plug-ins under manifest host `ame` from an `ame.json` registry row, `uxp.host.name` reads `ame`, and `require` resolves `mediaencoder` with no `premierepro`:
- Media Encoder's `api_config.json` allows no class, and `Object.keys(require('mediaencoder'))` lists its module (`RenderQueue`, `PresetManager`)
- Module JavaScript defines `Constants()` as a method returning `RenderQueueStatus`, `Result`, and `Workarea`
