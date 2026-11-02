# [HARNESSES]

Host proof harnesses run an original script's own bytes against case files in one host call per batch, then compare a rebuilt module with the host results. Case files, result files, and facts are data under `docs/research`, tools read every path from arguments.

## [01]-[LAYOUT]

| [INDEX] | [FILE]                            | [HOLDS]                                                                  |
| :-----: | :-------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `cases.ts`                        | Case and batch schemas over a host's precondition schema                 |
|  [02]   | `results.ts`                      | Result schema over a host's engine literals                              |
|  [03]   | `values.ts`                       | Host value codec: `{"$": ...}` for `undefined`, `NaN`, `±Infinity`, `-0` |
|  [04]   | `compare.ts`                      | Rebuilt module run over case arguments, each difference with its path    |
|  [05]   | `facts.ts`                        | Facts index load, lookup, and append                                     |
|  [06]   | `indesign/host.ts`                | Application table, case expansion, `osascript` transport                 |
|  [07]   | `indesign/precondition.ts`        | InDesign precondition schema                                             |
|  [08]   | `indesign/engine.jsxinc`, `.idjs` | ExtendScript and UXPScript engines running inside InDesign               |
|  [09]   | `indesign/main.ts`                | `run`, `probe`, and `compare` subcommands                                |

Host folders (`tools/harnesses/<host>/`) supply a precondition schema, engine literals, transport, and engines, and reuse every root module.

## [02]-[RUN]

```bash
node tools/harnesses/indesign/main.ts run <product> <name>.cases.json [--host 2027|2026] [--only <case>]...
node tools/harnesses/indesign/main.ts compare <product> <name>.cases.json
node tools/harnesses/indesign/main.ts probe --facts docs/research/applications/adobe/facts/indesign-host.json [--host 2026] [--engine uxpscript] [--precondition '<json>'] '<question>' '<expression>'
```

- `run` writes `<name>.results.json` beside the case file and prints one line per case
- `--host` defaults to `2027` (InDesign 2027 Beta), `2026` takes InDesign 2026 where the Beta refuses a call
- Engine follows the script extension, `.idjs` runs UXPScript through `app.doScript` with `ScriptLanguage.UXPSCRIPT`, others run ExtendScript through `do script`
- `compare` imports the batch `module`, runs each case naming `fn` on its arguments, and exits 1 on a difference
- Numbers compare under `Object.is`, a rebuilt module exporting `tolerance` (one number, or one per function) compares within that relative tolerance

## [03]-[CASES]

Case files live under each product's `proof/host/` as `<name>.cases.json`, paths relative to the product folder:

```json
{
    "script": "unpacked/<product>/<script>.jsx",
    "module": "rebuilt/<module>.ts",
    "precondition": { "document": { "documentPreferences": { "facingPages": false, "pagesPerDocument": 4 } }, "units": "POINTS" },
    "read": { "pages[*].top": "doc.pages[*].marginPreferences.top" },
    "vector": { "file": "rebuilt/vectors/<vector>.json", "script": "recovered/<script>.jsx", "lines": [[40, 72]], "calls": { "<fn>": "<original function>" } },
    "cases": [
        { "name": "<case>", "dialogs": [{ "fields": { "checkboxControls#2": true } }], "windows": [{ "presses": ["OK"] }], "confirms": [false], "prompts": ["12"] },
        { "name": "<call>", "fn": "<fn>", "call": "<original function>", "args": [792, 13], "script": "recovered/<script>.jsx", "lines": [[40, 72]] },
        { "name": "<probe>", "evaluate": "doc.pages[0].bounds" }
    ]
}
```

- Fields decide the kind: `evaluate` probes, `call` calls an original function with `args`, any other case runs the script whole
- Batch `script`, `precondition`, and `read` are defaults, a case's own fields replace or merge over them, call cases name their own `script` and `lines`
- `vector` turns each vector case whose `fn` names a `calls` row into a call case with the vector's `args`
- Scripts holding an `@JSXBIN@` string literal run that literal, `#include` lines expand, `#target` lines drop
- Expressions read `doc`, `app`, and `made`, a `[*]` in a key and its expression spreads over the collection, a `[*]` in the expression alone reads an array
- Values written into the host take `{"$": "enum:<Enumeration>.<MEMBER>"}` for an enumerator
- `dialogs` answer `app.dialogs` in order (`fields` keyed `<kind>#<n>`, `answer` false cancels), `windows` answer ScriptUI windows (`fields` by name, text, or `<type>#<n>`, `presses` by label), `files` answer pickers
- Reads run in points, before the script as `before` and after it as `read`, each failure under `readFailed`

Precondition rows run in table order:

| [INDEX] | [ROW]      | [VALUE]                                                                      |
| :-----: | :--------- | :--------------------------------------------------------------------------- |
|  [01]   | `document` | `documents.add` properties, or `open` with a document path                   |
|  [02]   | `labels`   | `insertLabel` rows                                                           |
|  [03]   | `units`    | One `MeasurementUnits` name, or `script`, `horizontal`, and `vertical` names |
|  [04]   | `set`      | Member path and value pairs                                                  |
|  [05]   | `add`      | `{ "as", "to", "kind", "properties" }`, read later as `made.<as>`            |
|  [06]   | `steps`    | Statements                                                                   |
|  [07]   | `select`   | Expressions `app.selection` returns, an insertion point included             |
|  [08]   | `menus`    | `true` captures script menu actions and invokes each `onInvoke`              |

## [04]-[RESULTS]

Results hold `application`, `version`, `engine`, and per case `ran`, `thrown` (error with line), `exited`, `failed` (precondition or harness failure), `value` (call and probe), `before`, `read`, `alerts`, `alertTitles`, `confirms`, `prompts`, `windows` (control tree shown and filled), `dialogs` (controls with their prefilled values), `files`, and `menus`. ExtendScript prints 15 significant digits, so a number `String` does not round-trip arrives as `{"$": "<mantissa>p<exponent>"}`, the exact double.

## [05]-[FACTS]

`probe` answers one question in one host call and appends it to the facts index with application, version, engine, expression, precondition, answer, and time. Before any host call it looks the question up by text, application, bundle version, and engine, and prints a known answer without calling the host. Questions name the host behavior asked (`ExtendScript toPrecision(17) round-trips a double`).

## [06]-[RULES]

Engines hold every case to the same host contract:
- Every case runs inside `try` and `catch` and writes each failure to its result, the UXPScript boot catches whatever escapes, so no host modal shows
- Originals run through `scoped(Window, alert, confirm, prompt, app, File, Folder, exit)` with recording fakes, UXPScript shadowing `require("indesign")`
- ExtendScript `app.doScript` of a string or file runs that source through the same shadow, a function runs as the host runs it
- Fake `Window` builds the declared control tree, fills fields from the case, and returns the case answer from `show`, palettes answer after the script returns
- `userInteractionLevel` is `NEVER_INTERACT` from before the first document opens until the case ends, then the prior level returns, and a script's own writes to it do not reach the host
- Documents open with `documents.add(false)` or `open(file, false)`, and a script's own `documents.add` and `open` take no window
- `app.activeDocument` is the case document, `app.documents` lists case documents alone, `app.activeWindow` is undefined, and `app.selection` holds the `select` rows
- Cases close every document opened after the case started, without saving, and restore script units and the active document

## [07]-[NATIVE_PLUGINS]

Native plugin proofs follow `designersbookshop-grid-calculator-pe`: `ghidra/scripts/Emu.java` holds one headless Ghidra p-code emulator over FIFOs (ops `begin`, `resume`, `read`, `write`, `signature`, `symbols`, `coverage`), `proof/emu.ts` opens a session, `proof/machine.ts` answers hooked calls with fake objects, `proof/call.ts` places arguments by Ghidra's parameter storage, and `proof/imports.ts` models libc and `PMString`. `Emu.java`, `machine.ts`, and `call.ts` name no product and copy unchanged, `emu.ts` changes only its `spawn` line to the product's `ghidra/scripts/run.sh` arguments (`adobe-indesign-right-to-left` copies both this way), and `imports.ts`, `families/`, and `tables.ts` stay per product.

## [08]-[LIMITS]

- Windowless documents hold no selection (`select` throws `No document windows are open.`), so `select` rows reach the script through the fake `app.selection` alone
- Scripts reading `app.activeWindow`, layout windows, `app.documents.everyItem()`, or another open document fail as `thrown`
- ExtendScript `app.doScript` in UXPScript or AppleScript fails as `thrown`, its source would run outside the shadow
- UXPScript sources with top-level `await` fail to parse inside `scoped`
- Writes a script makes count as results only through `read` expressions, `settle`-style folding of rebuilt writes stays in each product's `rebuilt/check.ts`
