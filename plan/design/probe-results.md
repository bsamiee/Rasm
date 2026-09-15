# [PROBE_RESULTS]

Probe date 2026-09-11, 21:10–21:40. Every script and the node project sit under
`<P>` = `<Sources>/design/probe`, where `<Sources>` =
`plan/research/sources` and
`<Inputs>` = `plan/inputs`.
No Adobe document was opened, saved, exported, or captured; no preference was set; no app was
launched or brought to the front. Every osascript call ran inside `with timeout of 60 seconds`, one at
a time, with a System Events window-list read before and after; no window list changed and no dialog
appeared. Illustrator was not running for the whole session (`pgrep -fli illustrator` empty, no
`Adobe Illustrator` row in `ps`), so experiments 1, 2, 3, 9 (Illustrator), and 10 (Illustrator
`app.version`) are recorded as `not running` with their scripts ready to rerun.

Process state at start: InDesign at 15.6% CPU (its idle baseline, it returned
to 14–16% after every call), Photoshop (Beta) at 1.4%, Acrobat at 1.3%.

## [01]-[R-C3]

Illustrator `do javascript (POSIX file …) with arguments {…}` path form.

Command: `<P>/run-illustrator.sh` (idle check `pgrep -x "Adobe Illustrator"` + `ps -o %cpu` under 5%,
window-list diff around each call, then three forms in turn: `POSIX file`, `file "<hfs>"`,
`alias "<hfs>"`, then `<P>/r-c4.jsx`).

Output:

```text
$ pgrep -fli illustrator
(empty)
```

Verdict: not run, Illustrator not running. `<P>/r-c3.jsx` reads `arguments[0]` (request JSON,
parsed through `eval("(" + text + ")")` because the ExtendScript engine has no `JSON`; the InDesign
run below confirmed `typeof JSON === "undefined"` on ExtendScript 4.5.6), writes `arguments[1]` with
`app.version`, `app.documents.length`, `$.version`, `app.buildNumber`, and returns
`String(File.length)`. Rerun: `zsh <P>/run-illustrator.sh` once Illustrator is up and idle.

## [02]-[R-C4]

`Compatibility.ILLUSTRATOR24`, the `Compatibility`, `DocumentType`, `SymbolRegistrationPoint`,
`ElementPlacement` tables, and `app.preferences.get*Preference` on twelve candidate key spellings.

Command: `<P>/run-illustrator.sh` last call, `do javascript (POSIX file "<P>/r-c4.jsx")`.

Verdict: not run, Illustrator not running. `r-c4.jsx` returns one JSON object with every member of
the four enums as `name: "String|Number"`, and for each key the result or error of
`getIntegerPreference`, `getBooleanPreference`, `getRealPreference`, `getStringPreference`. The key
list is `showToolTips`, `rulerType`, `uiBrightness`, `GeneralPrefs/showToolTips`,
`GeneralPrefs/rulerType`, `uiBrightness/uiBrightnessValue`, `aiPrefs/showToolTips`,
`Prefs/showToolTips`, `showTips`, `text/units`, `rulerType_5`, `Ruler/Units` (the last three come
from the prefs dump `<Inputs>/illustrator/prefs-flat.txt`, which records `rulerType_5`
as a real row).

## [03]-[R-I4]

`Brushes` member names by reflection.

Command: same `r-c4.jsx` run; it reads `Brushes.prototype` members, `typeof Brushes`, and, only when
`app.documents.length > 0`, `app.activeDocument.brushes` member names with `typeof add` and
`typeof addAndLoad`.

Verdict: not run, Illustrator not running. Expectation to check on rerun: ExtendScript DOM classes
expose no members on `.prototype` (InDesign returned `typeof Document.prototype.changeComposer ===
"undefined"` below while the class exists), so only the instance read decides it.

## [04]-[R-C2]

Does the Illustrator `tsconfig` need `types-for-adobe/shared/JavaScript`; what does esbuild → swc
emit.

Project: `<P>/node/package.json` (`typescript` 7.1.0-dev.20260906.1 from the Rasm catalog,
`types-for-adobe` 7.2.6, `extendscript-es5-shim` 0.3.1, `@swc/core` 1.16.2, `esbuild` 0.28.2,
`@modelcontextprotocol/sdk` 1.30.0, `effect` 3.22.1, `@crowecawcaw/xa11y` 0.14.0). Sample:
`<P>/node/sample.ts` (30 lines: `app.activeDocument.swatches`, `File`, top-level `arguments`,
`$.writeln`, `for…of`, template string, arrows, object spread, `JSON`).

Commands (no tsconfig; the function hook refuses a second config beside an owner, so `--types` went
on the command line):

```text
tsc --noEmit --strict --target es2018 --module esnext --moduleResolution bundler <lib> --typeRoots ./node_modules --types <types> sample.ts
```

| `<lib>` | `<types>` | Result |
| :-- | :-- | :-- |
| `--lib es5` | `Illustrator/2022` | 82 lines: 41 `lib.es5.d.ts` errors (`TS2451 Cannot redeclare block-scoped variable 'Object'/'Function'/'String'/'Boolean'/'Number'/'Math'/'Date'/'RegExp'`, `TS2374`, `TS2687`) plus 41 mirror errors in `types-for-adobe/shared/JavaScript.d.ts` |
| `--lib es5` | `Illustrator/2022,shared/JavaScript` | identical 82 lines |
| `--noLib` | `Illustrator/2022` | 5 errors, all in `sample.ts` (below) |
| `--noLib` | `Illustrator/2022,shared/JavaScript` | identical 5 errors |

The five `sample.ts` errors under `--noLib`:

```text
sample.ts(7,62): error TS2339: Property 'typename' does not exist on type 'Color'.
sample.ts(17,28): error TS2304: Cannot find name 'JSON'.
sample.ts(21,47): error TS2304: Cannot find name 'arguments'.
sample.ts(21,69): error TS2304: Cannot find name 'arguments'.
sample.ts(27,13): error TS2304: Cannot find name 'JSON'.
```

Facts: `Illustrator/2022/index.d.ts` opens with `/// <reference path="../../shared/global.d.ts" />`
and `ScriptUI.d.ts`, and `global.d.ts` pulls `JavaScript.d.ts` (the ES3 globals) in, so the
`shared/JavaScript` row changes nothing; both `types-for-adobe/shared/tsconfig.json` and
`Illustrator/2022/tsconfig.json` set `"noLib": true, "module": "none", "typeRoots": []`.
`declare class Swatches extends Array<Swatch>` (line 4553), which is why `for…of` typechecks.

Verdict: `types: ["types-for-adobe/Illustrator/2022"]` alone, with `noLib: true` (never
`lib: ["es5"]`); three declarations the project must add: `JSON` (json2), top-level
`arguments: string[]`, and `Color.typename` (missing from the 2022 typings).

esbuild: `esbuild sample.ts --bundle --format=esm --target=es2018 --platform=neutral` → 835 B,
`<P>/node/out-esbuild.js`, arrows, `const`, `for…of`, template strings, and spread intact.

swc: `transformSync(out-esbuild.js, {jsc:{target:"es3", loose:true}, isModule:false})` →
`<P>/node/out-swc-script.js`, 2960 B. Scan of the emit: `Symbol` **present** (guarded:
`typeof Symbol !== "undefined" && o[Symbol.iterator]`), `Object.defineProperty` absent,
`class` absent, `for…of` lowered, template literals gone, arrows gone, spread → `_extends`
(`Object.assign || function assign…` fallback). Unquoted reserved-word keys: none. ES5 calls in the
emit: `Array.isArray`, `.bind`, `Array.from` (Map/Set branch only) — `Array.isArray` runs on every
`for…of`, so `extendscript-es5-shim` is required at runtime. With `module.type: "commonjs"` the emit
adds `"use strict"` only (2974 B), still no `defineProperty`. With
`jsc.assumptions: {iterableIsArray: true, setSpreadProperties: true}` the `for…of` helper is
unchanged and `Object.defineProperty` appears (3823 B), so those assumptions are wrong for this
target.

Runtime hazard in the emit (decides code form): `_create_for_of_iterator_helper_loose(o)` returns an
index iterator only when `Array.isArray(o)` or `o` is a string/Arguments/typed array; an Illustrator
collection is `[object Swatches]` and not an `Array`, so `for (const s of doc.swatches)` throws
`TypeError: Invalid attempt to iterate non-iterable instance` at runtime. `for…of` over a DOM
collection is unusable; an index loop is the form (arrays built in script iterate fine).

Emitted output (`out-swc-script.js`, the sample part, helpers elided):

```javascript
var _arguments = arguments;
// sample.ts
var swatchRows = function swatchRows(doc) {
    var rows = [];
    for(var _iterator = _create_for_of_iterator_helper_loose(doc.swatches), _step; !(_step = _iterator()).done;){
        var swatch = _step.value;
        rows.push({ name: swatch.name, kind: String(swatch.color.typename) });
    }
    return rows;
};
var readRequest = function readRequest(path) {
    var file = new File(path);
    file.open("r");
    var text = file.read();
    file.close();
    return _extends({ job: "read" }, JSON.parse(text));
};
var main = function main() {
    var _ref = [ String(_arguments[0]), String(_arguments[1]) ], requestPath = _ref[0], responsePath = _ref[1];
    var request = readRequest(requestPath);
    var rows = app.documents.length > 0 ? swatchRows(app.activeDocument) : [];
    $.writeln("job " + request.job + ": " + rows.length + " swatches");
    var out = new File(responsePath);
    out.open("w");
    out.write(JSON.stringify(_extends({}, request, { rows: rows, version: app.version })));
    out.close();
    return "" + out.length;
};
main();
```

## [05]-[R-C1]

Does `@modelcontextprotocol/sdk` 1.30.0 `registerTool` accept `Schema.standardSchemaV1(…)`.

Command: `node <P>/node/r-c1.mjs` (McpServer + Client over `InMemoryTransport.createLinkedPair()`,
one tool `echo` with `inputSchema: Schema.standardSchemaV1(Schema.Struct({name: String, count:
Number}))`).

Output:

```text
standard schema vendor: effect version: 1
registerTool(inputSchema=standardSchemaV1): FAIL Error: inputSchema must be a Zod schema or raw shape, received an unrecognized object
listTools: FAIL MCP error -32601: Method not found
callTool valid: FAIL MCP error -32601: Method not found
callTool invalid: FAIL MCP error -32601: Method not found
```

The guard, `dist/esm/server/mcp.js:862-871`:

```javascript
function getZodSchemaObject(schema) {
    if (!schema) { return undefined; }
    if (isZodRawShapeCompat(schema)) { return objectFromShape(schema); }
    if (!isZodSchemaInstance(schema)) {
        throw new Error('inputSchema must be a Zod schema or raw shape, received an unrecognized object');
    }
    return schema;
}
```

`rg -i 'standard.?schema|~standard' dist/esm/server/*.js` finds nothing; `zod` is a non-optional
`peerDependencies` row (`"zod": "^3.25 || ^4.0"`, `peerDependenciesMeta.zod.optional: false`) and
was not installed by the probe (`require("zod")` fails), so `listTools` failed with `-32601` because
registration never happened, not because of a missing zod at call time.

Verdict: no. 1.30.0 accepts a Zod schema instance or a raw shape of Zod types only, no Standard
Schema path exists, `zod` is a required peer; a Zod shape is written at the MCP boundary and Effect
`Schema` stays behind it.

## [06]-[R-C5]

`PluginsInfo/v1/` row shape and the UPIA binary.

Command: `cat ~/Library/Application Support/Adobe/UXP/PluginsInfo/v1/{ID,PS}.json`.

`ID.json` (185 B, mode 0644, 2026-09-10 23:30):

```json
{"plugins":[{"hostMinVersion":"19.0","name":"Sidekick","path":"$localPlugins/External/8ebe7f95_1.0.22","pluginId":"8ebe7f95","status":"enabled","type":"uxp","versionString":"1.0.22"}]}
```

`PS.json` (1774 B, mode 0600, 2026-09-11 00:16): nine rows of the same shape, all
`"hostMinVersion":"23.3.0"`, `"type":"uxp"`, `"status":"enabled"`, paths
`$localPlugins/External/<pluginId>_<version>`: `TK9 Multi-Mask` (`com.tk.multimask` 4.0.0),
`TK9 Combo` (`com.tk.comboV8`), `TK9 Cx` (`com.tk.cxV8`), `TK9 Export` (`com.tk.export`),
`TK9 My Actions-Tab 1..4` (`com.tk.myactionstab1..4`), `TK9 My Actions` (`com.tk.myactionsV8`).

Row shape: `{hostMinVersion, name, path, pluginId, status, type, versionString}`, `path` rooted at
the `$localPlugins` token. The InDesign file is `ID.json` (answering the first half of R-C5); the
`Sidekick` plugin id `8ebe7f95` is a hash, not a reverse-DNS id.

UPIA directory `/Library/Application Support/Adobe/Adobe Desktop Common/RemoteComponents/UPI/UnifiedPluginInstallerAgent/`
holds one entry, `UnifiedPluginInstallerAgent.app` (root:wheel, 2025-08-11), binary at
`Contents/MacOS/UnifiedPluginInstallerAgent`. `--version` → `Unified Plugin Installer Agent Tool` /
`Version : 8.5.0.13`. No arguments → prints nothing, exits. `--help` lists `--help`, `--install <extension-file-path>`,
`--remove <extension-name>`, `--list <all || product display name>`, `--version`. `--install` and
`--list` were not run.

Verdict: file name and row shape recorded; the unsigned-`.ccx` half stays open for job P1/I1.

## [07]-[INDESIGN]

sdef and read-only `do script`.

`sdef` needs Xcode (`xcode-select: error: tool 'sdef' requires Xcode`), so `<P>/sdef-dump.py`
calls `OSACopyScriptingDefinitionFromURL` through ctypes: 3,633,545 B to `<P>/indesign.sdef`.
The `do script` command (`code="K2  dosc"`): direct parameter `any` (the script), `language`
(`doLg`, type `ScLg`, optional, "If not specified, uses the language used to call this method"),
`with arguments` (`wArg`, any), `undo mode` (`pSUM`, `eSUM`: script request / entire script / auto
undo / fast entire script), `undo name` (`unnm`, text, default `"Script"`), result `any`.
Enumeration `ScLg`: `unknown` [Unkn], `javascript` [JSLg], `uxpscript` [USLg] "The UxpScript
language", `applescript language` [ASLg].

Command: `osascript <P>/indesign-probe.applescript` (reads `<P>/indesign-probe.jsx` as UTF-8 and
runs `do script js language javascript`). Elapsed 1.68 s, window list unchanged, CPU back to
baseline.

Output (one JSON string):

```json
{"jsonShim":true,"appVersion":"21.6.0.58","scriptingVersion":"21.5","engineVersion":"4.5.6",
 "modalState":false,"documentsLength":0,"composer":"Adobe Paragraph Composer",
 "digitsType":"DEFAULT_DIGITS","harbuzz":true,"hasFindKeyStrings":"function",
 "findKeyStringsOptyca":[],"findKeyStringsAdobeWorldReady":["$ID/HL Composer Optyca"],
 "translateKeyStringArabic":"Arabic","hasChangeComposer":"undefined",
 "userInteractionLevel":"NEVER_INTERACT","locale":"ENGLISH_LOCALE","featureSet":"ROMAN",
 "digitsTypeEnum":["DEFAULT_DIGITS=1684628581","ARABIC_DIGITS=1684627826","HINDI_DIGITS=1684629609",
  "FARSI_DIGITS=1684629089","NATIVE_DIGITS=1684631137","FULL_FARSI_DIGITS=1684629094",
  "THAI_DIGITS=1684632680","LAO_DIGITS=1684630625","DEVANAGARI_DIGITS=1684628598", "…20 members"],
 "composerNames":{"error":"Error: Object does not support the property or method 'composerNames'"}}
```

Facts: the ExtendScript engine has no `JSON` (`jsonShim: true`); `app.findKeyStrings` takes the
translated UI string and returns key strings, so `findKeyStrings("HL Composer Optyca")` is `[]` and
`findKeyStrings("Adobe World-Ready Paragraph Composer")` is `["$ID/HL Composer Optyca"]`, which is
the composer key the plan's `apply_rtl_defaults` sets; `translateKeyString("$ID/Arabic")` resolves
to `Arabic`; `FARSI_DIGITS` and `FULL_FARSI_DIGITS` are both members (R-C7 stays a rendering
question); `Document.prototype.changeComposer` is `undefined` (DOM classes expose no prototype
members, so R-C6 still needs the instance read in job I4); `app.scriptPreferences.userInteractionLevel`
is already `NEVER_INTERACT` in this session.

uxpscript: `osascript <P>/indesign-probe-uxp.applescript` (`do script js language uxpscript`, the
script `require("indesign")` and returns `JSON.stringify(...)`) → empty result, no error, 1.57 s,
InDesign CPU spiked to 84% for the read and settled to 15.8%. Control
`<P>/indesign-uxp-minimal.applescript` (`do script "\"uxp-ok\"" language uxpscript`) → empty,
0.39 s; the same shape with `language javascript` → `js-ok`.

Verdict: `do script … language javascript` returns values and is the InDesign transport;
`language uxpscript` executes (no error, no dialog) but returns nothing through Apple Events, so a
UXP-script job must write its result to a file.

## [08]-[ACROBAT]

Command: `osascript <P>/acrobat-probe.applescript` (reads `<P>/acrobat-probe.js`, the
`(function(){try{…}catch(e){…}})()` wrapper, runs `do script js`). Elapsed 0.20 s, window list
`sample.pdf` unchanged, no dialog.

Output:

```json
{"ok":true,"viewerVersion":26.00221901,"viewerType":"Exchange-Pro","menuItems":6,
 "trustedFunction":"function","getPathError":"GeneralError: Operation failed.",
 "getPathErrorName":"GeneralError","getPathErrorMessage":"Operation failed.",
 "fromPDFConverters":["com.adobe.acrobat.eps","com.adobe.acrobat.xlsx","com.adobe.acrobat.html",
  "com.adobe.acrobat.jpeg","com.adobe.acrobat.jp2k","com.callas.preflight.pdfa",
  "com.callas.preflight.pdfe","com.callas.preflight.pdfx","com.adobe.acrobat.png",
  "com.adobe.acrobat.ps","com.adobe.acrobat.pptx","com.adobe.acrobat.rtf",
  "com.adobe.acrobat.accesstext","com.adobe.acrobat.plain-text","com.adobe.acrobat.tiff",
  "com.adobe.acrobat.doc","com.adobe.acrobat.docx","com.adobe.acrobat.xml-1-00",
  "com.adobe.acrobat.spreadsheet"],
 "typeofGlobal":"object","numDocs":1,"runtimeHighlight":true,"language":"ENU","platform":"MAC"}
```

Verdict: `app.getPath("user","javascript")` from the `do script` context raises
`GeneralError: Operation failed.` (name `GeneralError`), not `NotAllowedError`; the driver's
`not-allowed{method}` classifier must match `GeneralError` + `Operation failed.` for `getPath`.
`app.listMenuItems()` returns the 6 top-level menus only; `global` is an object; 19 export
converters.

## [09]-[R-C9]

`xa11y` and the earlier Swift dumps.

The npm package `xa11y@0.0.1` is an empty placeholder (`package.json` only, no code). The project
is `github.com/xa11y/xa11y` (MIT, v0.14.0 released 2026-09-09); its Node binding is
`@crowecawcaw/xa11y` 0.14.0 (`README`: `npm install @crowecawcaw/xa11y`, Python `pip install
xa11y`), installed into `<P>/node`. `App.byPid(pid, {timeout: 0})` is a lookup with no activation;
`app.tree()` returns `{role, name, children}` nodes only (no position, size, path, or enabled).

Command: `node <P>/node/ax-dump.mjs <pid>`.

| Process | xa11y nodes | lookup ms | tree ms | Swift `tree` nodes (`<Inputs>/ax/<app>.json`) | Swift `menus` nodes |
| :-- | --: | --: | --: | --: | --: |
| InDesign 33256 | 42 (32 button, 5 static_text, 1 text_field, 1 group, 1 unknown, app, window) | 69 | 25 | 47 | 3670 |
| Photoshop 33823 | 9 (3 combo_box, 2 scroll_bar, 2 text_field, app, window) | 55 | 10 | 13 | 1151 |
| Acrobat 34321 | 2 (app, window `sample.pdf`) | 53 | 5 | 6 | 382 |
| Illustrator | not running | — | — | 346 (window), 4036 (menus) | — |

Verdict: xa11y reproduces the window trees within the ±3–5 node noise the Swift README records
(it drops the traffic-light buttons and the empty group Acrobat and Photoshop expose) at 5–25 ms per
tree, but its `tree()` has no menu bar (top level is the window alone) and no geometry, so
inspection's read path stays the Swift walker for menus and positions; xa11y suits a fast
window-state read only. The Illustrator comparison waits for the process.

## [10]-[VERSIONS]

| Fact | Value | Source |
| :-- | :-- | :-- |
| Illustrator app | `<illustrator.bundlePath>`, `com.adobe.illustratorBeta`, 30.9.0 | `plutil -p …/Info.plist` |
| Illustrator prefs folder | `<illustrator.prefsFolder>`, plus `com.adobe.illustrator.plist`, `com.adobe.illustratorBeta.plist` | `ls ~/Library/Preferences \| rg -i illustrator` |
| Illustrator `app.version` | not running | — |
| Photoshop | `<photoshop.bundlePath>`, `com.adobe.Photoshop`, plist 27.11.0 | plist |
| Photoshop `app.version` | `27.11.0`, 0 documents, ExtendScript `$.version` 4.5.12, 0.13 s, no dialog | `osascript -e 'with timeout of 60 seconds / tell application id "com.adobe.Photoshop" to do javascript "app.version + …" / end timeout'` |
| InDesign | `<indesign.bundlePath>`, `com.adobe.InDesign`, 21.6.0.58 | plist, `app.version` |
| InDesign Version folder | `<indesign.prefsFolder>` (and `Adobe InDesign/Version <major>.0`) | `ls` |
| Acrobat | `/Applications/Adobe Acrobat DC/Adobe Acrobat.app`, `com.adobe.Acrobat.Pro`, 26.002.21901, `app.viewerVersion` 26.00221901 | plist, probe |

## [11]-[R-C3_LIVE]

Illustrator Beta 30.9.0 (build `72R`, ExtendScript 4.5.6, locale `en_US`) running
since 22:10 with one document open (`default-template.ai @ 8.98 % (RGB/Preview)`, one
`AXStandardWindow`); nothing was opened or saved. Every call ran through `<P>/run-illustrator.sh
<step>` (idle gate `ps -o %cpu` under 5%, System Events window-list diff, `with timeout of 60
seconds`, result to `<P>/last-call.txt`, log in `<P>/run-illustrator.log`). CPU before each call:
0.4, 0.2, 0.5, 0.2, 0.1, 0.4%; the window list never changed; no dialog.

Command per form (`<req>` = `<P>/request.json` holding `{"job":"r-c3","n":1,"text":"héllo"}`,
`<res>` = `<P>/response.json`, `<hfs>` = the HFS form of `<P>/r-c3.jsx`):

```text
tell application id "com.adobe.illustratorBeta"
do javascript (POSIX file "<P>/r-c3.jsx") with arguments {"<req>", "<res>"}
do javascript (file "<hfs>") with arguments {"<req>", "<res>"}
do javascript (alias "<hfs>") with arguments {"<req>", "<res>"}
```

Output, identical for all three forms (return value, then `response.json`):

```text
646                       # osascript stdout, elapsed 0.142 s / 0.168 s / 0.161 s
response.json bytes: 646
{"argc":2,"reqPath":"<req>","resPath":"<res>","request":{"job":"r-c3","n":1,"text":"héllo"},
 "readError":null,"appVersion":"30.9.0","documentsLength":1,"engineVersion":"4.5.6",
 "buildNumber":"72R","locale":"en_US","scriptFile":"<P>/r-c3.jsx"}
```

Facts: `with arguments` lands on the script-level `arguments` array as strings (`argc: 2`); a
function scope shadows it, so the script captures `var scriptArguments = arguments` at top level
before any function (the first run returned `argc: 0` for that reason, a script bug, not the
transport); the UTF-8 request round-trips (`héllo` → `é`); the returned `String(File.length)`
equals `wc -c` of the response; no `JSON` object exists in the engine, so the request is parsed with
`eval("(" + text + ")")` until json2 is bundled.

Verdict: all three path forms run on 30.9.0 with the same result in 0.14–0.17 s; the 2020
`JavaScript code was missing` bug on the `file` form is gone; the driver keeps `POSIX file` (no HFS
conversion step) and the request/response file protocol with the byte-count assertion holds.

## [12]-[R-C4_LIVE]

Command: `zsh <P>/run-illustrator.sh c4` (`<P>/r-c4.jsx`) and `… c4b` (`<P>/r-c4b.jsx`), results
in `<P>/r-c4-result.json` (2120 B) and `<P>/r-c4b-result.json` (3212 B), 0.15 s and 0.16 s.

Enum reflection: `for (var k in Compatibility)` enumerates only members already touched (0 on a
fresh engine, 1 after reading `ILLUSTRATOR24`, 6 after reading six), `E.reflect.properties` throws
`Error: Invalid enumeration value`, and reading a nonexistent member throws
`Error 1320: Invalid enumeration value` (the first `c4b` run died on `Compatibility.ILLUSTRATOR99`
with `execution error: Adobe Illustrator got an error: Error 1320: Invalid enumeration valueLine:
25-> var m = E[name]; (5001)`, no dialog). A member is an object whose `String()` and `valueOf()`
are its qualified name and whose `Number()` is `NaN`; `m.reflect` reports `Object props=4
methods=7`. The tables therefore come from access by name:

| Enum | Resolves on 30.9.0 | Throws `1320` |
| :-- | :-- | :-- |
| `Compatibility` | `ILLUSTRATOR3`, `ILLUSTRATOR8`, `ILLUSTRATOR10`, `ILLUSTRATOR17`, **`ILLUSTRATOR24`**, `JAPANESEVERSION3` (9, 11–16 untested, listed in the typings) | `ILLUSTRATOR18`–`23`, `ILLUSTRATOR25`–`30`, `ILLUSTRATOR99` |
| `DocumentType` | `ILLUSTRATOR`, `EPS`, `PDF`, `FXG` | `SVG`, `NOPE` |
| `SymbolRegistrationPoint` | `SYMBOLTOPLEFTPOINT`, `SYMBOLCENTERPOINT`, `SYMBOLBOTTOMRIGHTPOINT` (the nine typed members' shape holds) | `NOPE` |
| `ElementPlacement` | `INSIDE`, `PLACEATBEGINNING`, `PLACEATEND`, `PLACEBEFORE`, `PLACEAFTER` | `NOPE` |

Preference reads (`app.preferences.get{Integer,Boolean,Real,String}Preference(key)`, none throws;
an unknown key returns integer `207192349`, boolean `true`, real `2.22436241268732E-314`, string
`""`):

| Key | integer | boolean | real | Reading |
| :-- | --: | :-- | --: | :-- |
| `showToolTips` | 1 | true | denormal | resolves, integer 1 |
| `rulerType` | 6 | true | denormal | resolves, integer 6 (pixels) |
| `uiBrightness` | 207192349 | true | **0** | resolves as a real (`uiBrightness = 0.0` in the Cloud Prefs dump), not an integer |
| `text/units` | 6 | true | denormal | resolves |
| `rulerType_5` | 6 | true | denormal | resolves (row in `prefs-flat.txt`) |
| `plugin/AIMCPServer/ServerEnabled` | **1** | true | denormal | resolves, integer 1 |
| `plugin/AgenticUI/ShowAgenticUIPanelPreference2` | **0** | **false** | denormal | resolves, integer 0 |
| `AIMCPServer/ServerEnabled`, `AgenticUI/ShowAgenticUIPanelPreference2` | 207192349 | true | denormal | unknown: the `plugin/` prefix is part of the key |
| `GeneralPrefs/showToolTips`, `GeneralPrefs/rulerType`, `uiBrightness/uiBrightnessValue`, `aiPrefs/showToolTips`, `Prefs/showToolTips`, `showTips`, `Ruler/Units` | 207192349 | true | denormal | unknown spellings |

Verdict: `Compatibility.ILLUSTRATOR24` resolves and is the newest member on 30.9.0 (25–30 throw),
so `build_template` save options name `ILLUSTRATOR24`; the preference keys are bare
(`showToolTips`, `rulerType`) or `plugin/<section>/<key>` for plug-in sections, `uiBrightness` is a
real, and `207192349` from `getIntegerPreference` is the unknown-key sentinel the reader must treat
as absence. Also for the driver: an ExtendScript syntax error in a file run (first `c4` run,
`Error 9: Illegal use of reserved word 'int'`) came back on osascript stderr as
`execution error: Adobe Illustrator got an error: Error 9: …Line: 50-> …` with no modal dialog and
no `-1712`, which contradicts the plan's [06] claim that a syntax error raises a modal and recycles
the process; and an unquoted reserved-word key (`int`, `boolean`) is a hard parse error, which fixes
the R-C2 emit check's "unquoted reserved words" row as load-bearing.

## [13]-[R-I4_LIVE]

Command: the `c4` run, `app.activeDocument.brushes` on the open document (read only).

Output:

```json
{"count":7,"memberNames":["length","parent","length","typename"],"add":"function",
 "addAndLoad":"function","protoAddAndLoad":"undefined"}
```

`Brushes.prototype` enumerates nothing, `typeof Brushes` is `function`, and `brushes.reflect`
throws `No such element`.

Verdict: `Brushes.addAndLoad` is declared on the instance (`typeof === "function"`) on 30.9.0
with `add` beside it; the runtime call on one source (the write half of R-I4) stays for job time.

## [14]-[VERSIONS_LIVE]

`app.version` = `30.9.0`, `app.buildNumber` = `72R`, `$.version` = `4.5.6`, `$.locale` =
`en_US`; prefs folder `<illustrator.prefsFolder>`; bundle
`com.adobe.illustratorBeta`, process name `Adobe Illustrator` (`pgrep -x` matches).

## [15]-[VERDICTS]

| Experiment | Verdict | Decides in the plan |
| :-- | :-- | :-- |
| R-C3 | `POSIX file`, `file "<hfs>"`, and `alias` forms all run on 30.9.0 in 0.14–0.17 s; `with arguments` lands on script-level `arguments`; file protocol round-trips UTF-8; return `646` equals the response byte count | driver keeps `POSIX file` + request/response files + byte assertion |
| R-C4 | `Compatibility.ILLUSTRATOR24` resolves and is the newest (25–30 throw `1320`); enums are not enumerable (`for…in` and `reflect` fail), tables come from access by name; keys `showToolTips`=1, `rulerType`=6, `uiBrightness` real 0.0, `plugin/AIMCPServer/ServerEnabled`=1, `plugin/AgenticUI/ShowAgenticUIPanelPreference2`=0; unknown key sentinel `207192349`; a syntax error returns on stderr with no modal | `build_template` save options; preference key spellings and the absence sentinel; error classification |
| R-I4 | `brushes.addAndLoad` and `brushes.add` are `function` on the instance (7 brushes in the open document) | declared status closed; runtime call stays for job time |
| R-C2 | `noLib: true` + `types: ["types-for-adobe/Illustrator/2022"]` only; `shared/JavaScript` is transitive; declare `JSON`, `arguments`, `Color.typename`; swc es3 loose emits no `class`/`defineProperty`, `Symbol` guarded, needs `Array.isArray` (es5-shim); `for…of` over a DOM collection throws at runtime | the `types` row, the shim, and an index-loop rule for collections |
| R-C1 | no: 1.30.0 accepts Zod instances or raw Zod shapes only, `zod` is a required peer | a Zod shape at the MCP boundary |
| R-C5 | `ID.json`/`PS.json` rows `{hostMinVersion, name, path, pluginId, status, type, versionString}`, UPIA 8.5.0.13 with `--install/--remove/--list`; unsigned `.ccx` untested | install route half-decided |
| InDesign | `language javascript` returns values (1.7 s); `language uxpscript` runs but returns nothing; World-Ready composer key `$ID/HL Composer Optyca`; `changeComposer` undefined on the prototype; no `JSON` in ExtendScript | InDesign transport is `javascript`; UXP jobs write files; composer key for `apply_rtl_defaults` |
| Acrobat | `getPath` raises `GeneralError: Operation failed.`; `trustedFunction` present; `global` object; 6 top-level menus | `not-allowed` classifier string |
| R-C9 | `@crowecawcaw/xa11y` 0.14.0 matches Swift window counts within noise at 5–25 ms but has no menu bar and no geometry | inspection keeps the Swift walker; xa11y for window-state reads |
| Versions | Illustrator `app.version` 30.9.0 build `72R` (prefs `illustrator.prefsFolder`), Photoshop 27.11.0, InDesign 21.6.0.58 (`indesign.prefsFolder`), Acrobat 26.002.21901 | version facts |
