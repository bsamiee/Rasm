# [HARNESSES]

Host proof harnesses run an original script's own bytes against case files in one host call per batch, then compare a rebuilt module with the host results. Case files, result files, and facts are data under `docs/research`, tools read every path from arguments.

## [01]-[LAYOUT]

| [INDEX] | [FILE]        | [HOLDS]                                                                                        |
| :-----: | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `cases.ts`    | Case and batch schemas, preconditions as rows each host decodes                                |
|  [02]   | `results.ts`  | Result schema over a host's engine literals, `crashed` and `hung` beside the completion        |
|  [03]   | `values.ts`   | Host value codec: `{"$": ...}` for `undefined`, `NaN`, `±Infinity`, `-0`, `date:<ISO 8601>`    |
|  [04]   | `compare.ts`  | Rebuilt module run over case arguments, each difference with its path                          |
|  [05]   | `facts.ts`    | Facts index load, lookup, crashed or hung lookup, and append, text facts search                |
|  [06]   | `instance.ts` | `watch-apps` launch and watch, `shlock` instance lock, `.ips` crash decode, `sample` of a hang |
|  [07]   | `error.ts`    | `HarnessError` every module raises                                                             |

Host folders (`tools/harnesses/<host>/`) supply a precondition schema, engine literals, transport, and engines, and reuse every root module.

## [02]-[CASES]

Case files live under each product's `proof/host/` as `<name>.cases.json`, paths relative to the product folder:

```json
{
    "script": "unpacked/<product>/<script>.jsx",
    "module": "rebuilt/<module>.ts",
    "precondition": { "<row>": "<value>" },
    "read": { "pages[*].top": "doc.pages[*].marginPreferences.top" },
    "vector": { "file": "rebuilt/vectors/<vector>.json", "script": "recovered/<script>.jsx", "lines": [[40, 72]], "calls": { "<fn>": "<original function>" } },
    "cases": [
        { "name": "<case>", "dialogs": [{ "fields": { "checkboxControls#2": true } }], "windows": [{ "fields": { "edittext#0": "12" }, "presses": ["OK"] }], "confirms": [false], "prompts": ["12"] },
        { "name": "<call>", "fn": "<fn>", "call": "<original function>", "args": [792, 13], "script": "recovered/<script>.jsx", "lines": [[40, 72]] },
        { "name": "<probe>", "evaluate": "doc.pages[0].bounds" }
    ]
}
```

- Fields decide the kind: `evaluate` probes, `call` calls an original function with `args`, any other case runs the script whole
- Batch `script`, `precondition`, and `read` are defaults, a case's own fields replace or merge over them, call cases name their own `script` and `lines`
- `vector` turns each vector case whose `fn` names a `calls` row into a call case with the vector's `args`
- Expressions read `doc`, `app`, and `made`, a `[*]` in a key and its expression spreads over the collection, a `[*]` in the expression alone reads an array
- Spread reads ending in a member chain read `everyItem()` once, members a bulk read refuses read per item
- Values written into the host take `{"$": "enum:<Enumeration>.<MEMBER>"}` for an enumerator, host reads qualify every enumerator one enumeration holds by name and code
- `dialogs` answer `app.dialogs` in order (`fields` keyed `<kind>#<n>`, `answer` false cancels), `windows` answer ScriptUI windows (`fields` by name, text, or `<type>#<n>`, `presses` by label), `files` answer pickers
- Reads run in points, before the script as `before` and after it as `read`, each failure under `readFailed`

## [03]-[INSTANCES]

Each batch reaches the host instance its installation runs, hidden through `watch-apps launch <path> --hides` when none runs, under a `shlock` lock keyed by bundle identifier and pid that a second run fails on. A `watch-apps watch` of the bundle and its crash reporter runs for the whole call, keeping rows of the instance pid, reporters inside the installation, and system dialogs naming the installation's display name. Dialogs requiring an answer are pressed through a case's `presses`, else cancel, default, or only button, system dialogs through Ignore or cancel alone, each recorded on the started case as `answered`. Engines write `<index>.started` before each case and `<index>.json` after it, the harness reads outcomes from those records.

- Exits settle on a status, crash report, or report failure, record `crashed` with `.ips` frames, and relaunch once the crash reporter exits
- Cases past `--limit` with no new record record `hung` with a `sample` of the instance, which stays running, and the batch ends when its records folder goes
- Faults before any case started fail the batch's first case and record neither `crashed` nor `hung`
- Launches write `always_never_send` 2 to `com.adobe.crashreporter` after `launch` returns, which the build resets at its own launch
- Results mark `shared` when another process launched the instance, which stays as it was

## [04]-[RESULTS]

Results hold `application`, `identifier`, `version`, `engine`, and per case `ran`, `thrown` (error with line and call chain), `exited`, `failed` (precondition, harness, or host failure), `crashed`, `hung`, `value` (call and probe), `before`, `read`, `alerts`, `confirms`, `prompts`, `windows` (control tree shown and filled), `dialogs` (controls with their prefilled values), `files`, `menus`, `activations` (activating members recorded and never sent), `leaked` (application state the case changed that the engine could not restore), and `answered`. ExtendScript prints 15 significant digits, so a number `String` does not round-trip arrives as `{"$": "<mantissa>p<exponent>"}`, the exact double.

## [05]-[FACTS]

`run --facts <index>.json` answers each `evaluate` case whose name, expression, precondition, bundle identifier, version, engine, and host state the index holds before any host call, prints the `--stated` text facts lines matching each remaining question, skips any case whose expression holds a crashed or hung fact, and appends every new answer and fault in one write. A batch whose cases the index answers makes no host call. Questions name the host behavior asked (`ExtendScript toPrecision(17) round-trips a double`), and questions about a class read its object model whole through one `evaluate` of its `reflect` or dictionary.

## [06]-[NATIVE_PLUGINS]

Native plugin proofs follow `designersbookshop-grid-calculator-pe`: `ghidra/scripts/Emu.java` holds one headless Ghidra p-code emulator over FIFOs (ops `begin`, `resume`, `read`, `write`, `signature`, `symbols`, `coverage`), `proof/emu.ts` opens a session, `proof/machine.ts` answers hooked calls with fake objects, `proof/call.ts` places arguments by Ghidra's parameter storage, and `proof/imports.ts` models libc and `PMString`. `Emu.java`, `machine.ts`, and `call.ts` name no product and copy unchanged, `emu.ts` changes only its `spawn` line to the product's `ghidra/scripts/run.sh` arguments (`adobe-indesign-right-to-left` copies both this way), and `imports.ts`, `families/`, and `tables.ts` stay per product.

## [07]-[INDESIGN]

```bash
nx run @rasm/harnesses:indesign -- run <product> <name>.cases.json [--release <prefix>]... [--only <case>]... [--limit <duration>] [--facts <index>.json] [--stated <facts>.txt]...
nx run @rasm/harnesses:indesign -- compare <product> <name>.cases.json
```

- Installations are the apps Launch Services opens the `uxpscript` bundle with, `--release` keeping versions it prefixes, else all
- `compare` checks every `<name>.<version>.results.json` beside the case file
- Batches reach the instance through Atelier's `do script` by pid, `.idjs` scripts in `language uxpscript` with the built bundle, others in `language javascript` with `extendscript.jsxinc`
- Facts hold the survey (enumerations, profiles, languages, locale, feature set, DOM version) per bundle and version, else a watched batch asks it
- Results hold the instance `state` at batch start: `modal`, working spaces, application preference objects whole, and the members the snapshot could not read
- Scripts holding an `@JSXBIN@` string literal run that literal, `#include` lines expand, `#target` lines drop
- Case folders sit under `<name>.<version>.captures/<index>/` beside the results, holding document copies, files the case wrote, IDML exports, and hang samples

Preconditions take one of the `document`, `open`, and `application` rows, then the others in table order:

| [INDEX] | [ROW]         | [VALUE]                                                                                       |
| :-----: | :------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `document`    | `intent`, `width`, `height`, `profiles` (`cmyk`, `rgb`), `language`, and further `properties` |
|  [02]   | `open`        | Document path, its copy opened with a window                                                  |
|  [03]   | `application` | `true` for a case reading application state, no document opened                               |
|  [04]   | `units`       | One `MeasurementUnits` member, or `script`, `horizontal`, and `vertical` ones                 |
|  [05]   | `engine`      | `extendscript` or `uxpscript` for a batch of evaluate cases                                   |
|  [06]   | `fonts`       | PostScript names `app.fonts` must report `INSTALLED`                                          |
|  [07]   | `redraw`      | `scriptPreferences.enableRedraw` the case runs under                                          |
|  [08]   | `idml`        | `true` exports `<index>.before.idml` and `.after.idml` inside the rolled-back step            |
|  [09]   | `labels`      | `insertLabel` rows                                                                            |
|  [10]   | `set`         | Member path and value pairs                                                                   |
|  [11]   | `add`         | `{ "as", "to", "kind", "properties" }`, read later as `made.<as>`                             |
|  [12]   | `steps`       | Statements                                                                                    |
|  [13]   | `select`      | Expressions `app.select` selects, an insertion point included                                 |
|  [14]   | `menus`       | `true` captures script menu actions and invokes each `onInvoke`                               |

- `intent`, `units`, profiles, languages, and `{"$": "enum:..."}` values decode against the survey before any host call, failing their case alone
- Engines read each `documents.add` property back and fail the case on a difference, `opened` holding each document's preference objects whole
- Page sizes read in the script unit, set before the document opens with its window
- `userInteractionLevel` is `NEVER_INTERACT` for the whole batch, and a script's own writes to it do not reach the host
- Originals run through `scoped(Window, alert, confirm, prompt, app, File, Folder, exit, $)`, UXPScript shadowing `require("indesign")` and `require("uxp")`, preconditions and reads evaluating against the same `app`
- ScriptUI windows are real `Window` objects laid out but never shown, `show` answering from `windows`, each closed after the case
- `app.documents` lists case documents, `activeDocument`, `activeWindow`, `layoutWindows`, and `selection` read the host, and a script's own documents open with a window
- `app.doScript` of a string or file in the engine's own language runs through the same shadow, a function runs as the host runs it, another language fails as `thrown`
- `app.activate`, and in UXPScript every `show`, `revealInSystem`, and `revealInBridge`, record under `activations`
- Case documents hold the batch label, and a batch closes labelled documents a dead session left open
- Cases close their documents, restore preferences, units, redraw, and the active document, and remove application swatches and styles they added
- Results add `undo` (nested `doScript` steps), `written` (an IDML hashed by entry without its XMP packet), and `logs` (UXPLogs lines `lsof` finds)
- UXPScript text holding `async`, `await`, `then`, `catch`, or `finally`, or failing to parse, and ExtendScript text naming `revealIn*`, fail unsent

Limits:
- InDesign keeps one instance per release, so batches share it with other runs, and the lock orders harness runs alone
- Atelier must be built from the current tree, an older build's instance states fail to decode
- `app.modalState` true holds every case document open, each `close` taking seconds, and results record it in `state.modal`
- ExtendScript `app` reads `activeDocument`, `activeWindow`, `layoutWindows`, and `selection` at start and after each `app` method, not after a document `select()`
- ExtendScript errors carry the throwing line alone, `$.stack` naming the catching frames, and a JSXBIN literal is opaque to the source checks
- Writes a script makes count as results only through `read` expressions and IDML exports, `settle`-style folding of rebuilt writes stays in each product's `rebuilt/check.ts`

## [08]-[BLENDER]

```bash
node tools/harnesses/blender/main.ts run <product> <name>.cases.json [--app <Blender.app>] [--facts <index>.json] [--only <case>]...
node tools/harnesses/blender/main.ts probe --facts <index>.json [--app <Blender.app>] [--precondition '<json>'] '<question>' '<expression>'
node tools/harnesses/blender/main.ts compare <product> <name>.cases.json [--app <Blender.app>]
```

- Installs are the bundles Spotlight lists for `org.blenderfoundation.blender`, `--app` picks one, and facts key on `Blender --version`
- Batches launch their own hidden instance with `TMPDIR` and an empty `BLENDER_USER_RESOURCES` in a run folder, quitting it from inside
- `engine.py` starts the installed Lab bridge on a free port, one request binding `check_is_finished` to run a case per timer pass
- Crashes record signal and `*.crash.txt` on the started case and press the alert's Ignore, and a new instance runs the remaining cases
- `run --facts` skips a case whose expression holds a crashed fact for the build and appends each new crash
- Script folders import as packages registered for the batch and files as modules registered for the case, `unregister` running when its scope closes
- Expressions and statements read `bpy`, `context`, and `scene` under the case override, call expressions also the script's names
- Values add `int:<digits>` past 2^53, `id:<type>:<name>`, and `repr:<text>` to the `{"$": ...}` forms, RNA float members store float32
- Cases end with file browsers cancelled, each window's workspace rewritten to close popups, and modal operators still running recorded as `modal`
- `compare` calls the batch `module` (Python) in a second batch on each `fn` case's arguments and compares the JSON values exactly

Preconditions apply in table order:

| [INDEX] | [ROW]                              | [VALUE]                                                                     |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `document`                         | `{ "open", "load_ui" }`, `"none"` for application state, else an empty file |
|  [02]   | `scene`, `view_layer`, `workspace` | Window members by name                                                      |
|  [03]   | `area`, `region`                   | Override types, the largest area taking an absent type, region `WINDOW`     |
|  [04]   | `set`                              | Member path and value pairs, unit `system` before `length_unit`             |
|  [05]   | `steps`                            | Statements                                                                  |
|  [06]   | `edited`                           | `true` pushes an undo step                                                  |
|  [07]   | `jobs`                             | `bpy.app.is_job_running` types a case waits on before its reads             |

Limits:
- Blender draws its popups, so neither watch-apps nor Python sees one and results record none
- Results hold `stdout`, `stderr`, `reports`, `modal`, and `crashed` in place of the InDesign dialog fields

## [09]-[RHINO]

```bash
node tools/harnesses/rhino/main.ts run <product> <name>.cases.json [--host <bundle id|name|path|version>]... [--limit <duration>] [--facts <index>.json] [--stated <facts>.txt]... [--only <case>]...
node tools/harnesses/rhino/main.ts compare <product> <name>.cases.json --release <version> [--limit <duration>]
```

- Installs are `com.mcneel.rhinoceros.<major>` bundles Spotlight lists, every install by default, each writing `<name>.<version>.results.json`
- `compare` runs on the install whose bundle identifier and version the results name, failing with the installed list when none reports both
- Runs write case rows to `batch.json` and compile expressions alone into `Cases.cs` of `Rasm.Harnesses.Rhino.csproj` in the batch's artifacts folder
- Builds read the target framework from the install's `dotnetstart` runtime configuration and Grasshopper 2 from the bundle, else the registry
- Cases whose expressions fail to compile record each compiler diagnostic as `Failed`, the rest build again and run
- Sessions spawn `Contents/MacOS/<CFBundleExecutable>` with `-nosplash` and `-runscript=_-ScriptEditor _Run <loader>.cs`, the engine hiding the app first
- Session release sends SIGTERM to the instance's process group and its companion apps, then SIGKILL past `--limit`
- Loaders run the engine in a collectible `AssemblyLoadContext` on the first idle and quit on the next, the journal recording every outcome
- `watch-apps watch` covers the bundle, companion apps the instance started, and system dialogs naming the install
- Dialogs shown after the engine journals its session take a case's `windows` or `presses` label, else the cancel or sole button
- System crash alerts take Ignore (`action-button-2`), and dialogs holding neither button between cases fail the batch
- Journal waits past `--limit` on a prompt write `<journal>.cancel`, and the engine posts `RhinoApp.PostCancelEvent` with a wake event
- Other waits past `--limit` record `hung` with a `sample` and a `dotnet-stack` report
- Exits record `crashed` with `.ips` exception, subtype, termination, frames, RhinoCode process log, and last swallowed exception
- New instances resume the cases a crash left
- `run --facts` answers each probe the index holds for its question, build, engine, expression, and precondition, then records each new answer
- `run --facts` skips a case holding a crashed or hung fact under the same key and appends each new one
- Expressions are C# over `doc`, `view`, `made`, `canvas` (Grasshopper 2 document), `editor` (its canvas control), and `scope` (`Capture`, `Solve`)
- `call` names a method group run on `args`, `commands` run one macro per `RunScript`, each stating its prompt answers
- Scripts run in place through `RhinoCode.TryCreateCode`, the language read from the extension or `#!` header
- Batch `plugin` names a binary the build references with its internal and private members publicized
- Values encode `CommonObject` as its openNURBS archive (`ToJSON`) and `Type` and members as declarations with XML summaries
- Values of other reference types fail to encode
- Case fields `dialogs`, `confirms`, `prompts`, `files`, and batch `vector` fail decoding

Preconditions take one of the `template`, `open`, and `application` rows, a case document row replacing the batch's with its `units` and `tolerance`:

| [INDEX] | [ROW]         | [VALUE]                                                                               |
| :-----: | :------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `template`    | `TemplateFolder` file of the running language, with `units` and `tolerance` required  |
|  [02]   | `open`        | `.3dm` keeping its own units and tolerances unless `units` or `tolerance` states them |
|  [03]   | `application` | `true` for application state, no document, batch `read`, commands, or script          |
|  [04]   | `language`    | RhCore localization `Localization.CurrentLanguageId` must report                      |
|  [05]   | `view`        | `projection`, `display` English name, camera `location`, `target`, `lens`             |
|  [06]   | `layout`      | Page view `name`, `width`, `height`, page `units`                                     |
|  [07]   | `layers`      | `{ "path", "color", "locked" }` by full path, parents made first                      |
|  [08]   | `canvas`      | Grasshopper 2 document the hidden editor canvas shows, `open` naming a `.ghz`         |
|  [09]   | `add`         | `{ "as", "geometry", "layer" }`, the id read as `made["<as>"]`                        |
|  [10]   | `select`      | `made` names selected, a locked or hidden one failing the case                        |
|  [11]   | `steps`       | C# statements in one undo record                                                      |
|  [12]   | `batch`       | `true` turns on `BatchMode` for the case                                              |

- Case documents open through `_-New` and write `<index>.3dm` and `<index>.after.3dm` under the captures folder
- Documents a case opened close once the next case's document is active, the last ones at quit
- Commands and scripts fail before running on a document Rhino does not report active
- Application settings classes a case changed restore after it under `restored`, those keeping the change under `leaked`
- Results add `document`, `macros`, `commands`, `prompts`, `printed`, `console`, `reported`, `logged` (RhinoCode logger), `writes`, and `diagnostics`
- `writes` lists object adds, deletes, replacements, attribute changes, and layer and material events while macros, script, and body ran
- `compare` checks `value`, `read` keys, or `writes` per case `expect`

Limits:
- Post-effect `WriteState` runs only under RDK native frames, so a plug-in throwing there records `crashed`, not `Thrown`
- `Localization.CurrentLanguageId` reads the stored `LanguageIdentifier`, which no launch argument sets, so a case naming another language fails
- `ZebraAnalysisSettings.GetCurrentState` aborts Rhino in `CRhinoZebraAnalysisSettings_Bool`, so analysis settings classes record no leak
