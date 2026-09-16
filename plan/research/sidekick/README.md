# [SIDEKICK]

Sidekick is an MCP server that drives Adobe InDesign through a UXP panel over a local WebSocket. This folder holds the server build output, the impl bundle, and the panel, decomposed below as the reference for the InDesign half of the Creative Cloud work.

Paths are relative to this folder. The prettified panel source is `plan/research/sources/controllers/uxp-index.pretty.js`, cited as `plugin`. Citations are `file:line`.

## [01]-[CONTENTS]

```text
sidekick/
├── README.md
├── app-support/
│   ├── version.txt                     1.0.27
│   ├── config.json                     {"telemetry": false}
│   └── 1.0.27/impl.js                  esbuild bundle of the impl half, 51 789 lines
├── lib/
│   ├── package.json                    @indesign-mcp/server 1.0.27, dependency rows
│   ├── npm-shrinkwrap.json             resolved dependency tree
│   ├── bin/                            oclif entry points: run.js, dev.js, indesign-sidekick, uninstall
│   ├── scripts/                        packaging and smoke-test scripts of the product build
│   └── dist/                           unminified commented TypeScript build output, the source of truth
└── uxp/8ebe7f95_1.0.22/
    ├── manifest.json                   UXP manifest
    ├── index.html                      panel markup
    ├── dist/index.js                   panel bundle, prettified copy under plan/research/sources/controllers/
    └── icons/                          panel and plugin icons
```

| [INDEX] | [PART]                    | [WHAT IT IS]                                                                     |
| :-----: | :------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `lib/dist/`               | Commented build output for every module, with `.d.ts` and source maps beside each |
|  [02]   | `lib/dist/data/`          | `enum-mappings.json`, 720 868 bytes over 19 117 lines                             |
|  [03]   | `app-support/1.0.27/`     | Bundle the launcher downloads and runs, all modules inlined into one file         |
|  [04]   | `uxp/8ebe7f95_1.0.22/`    | Installed panel at plugin version 1.0.22, one version behind the server           |
|  [05]   | `lib/bin/`, `lib/scripts/`| Product packaging, not part of the decomposition                                  |

## [02]-[MODULE_MAP]

`app-support/1.0.27/impl.js` inlines every module; the ranges locate each one inside the bundle, and `lib/dist/` holds the same code with comments.

| [INDEX] | [IMPL RANGE]  | [FILE]                                    | [PURPOSE]                                   |
| :-----: | :------------ | :---------------------------------------- | :------------------------------------------ |
|  [01]   | 14301–18834   | `lib/dist/lib/vips-embedded.js`           | Base64 wasm and worker for wasm-vips        |
|  [02]   | 18935–18948   | `lib/dist/lib/paths.js`                   | Data and cache directories                  |
|  [03]   | 18949–18978   | `lib/dist/lib/posthog.js`                 | Telemetry envelope                          |
|  [04]   | 18979–30191   | `lib/dist/lib/update-notice.js`           | Launcher update notice                      |
|  [05]   | 30192–49310   | `lib/dist/data/enum-mappings.json`        | Enum name, constant, and FourCC tables      |
|  [06]   | 49311–49779   | `lib/dist/lib/code-transforms.js`         | Acorn autocorrect over model-written code   |
|  [07]   | 49780–49942   | `lib/dist/tools/execute.js`               | `execute` tool                              |
|  [08]   | 49943–50310   | `lib/dist/lib/font-metrics/*.js`          | Font index, scanner, opentype parse         |
|  [09]   | 50311–50404   | `lib/dist/tools/get-font-metrics.js`      | `get_font_metrics` tool                     |
|  [10]   | 50405–50637   | `lib/dist/tools/get-layout.js`            | `get_layout` tool                           |
|  [11]   | 50638–50862   | `lib/dist/resources/snapshot-view*.js`    | MCP Apps snapshot viewer                    |
|  [12]   | 50863–50932   | `lib/dist/lib/vips-init.js`               | vips extraction                             |
|  [13]   | 50933–51010   | `lib/dist/tools/image-crop.js`            | Server-side crop and recompress             |
|  [14]   | 51011–51437   | `lib/dist/tools/snapshot-render.js`       | Capture script builder                      |
|  [15]   | 51438–51687   | `lib/dist/tools/snapshot*.js`             | `snapshot`, `snapshot_object`, `show_snapshot` |
|  [16]   | 51688–51714   | `lib/dist/tools/index.js`                 | Tool registry                               |
|  [17]   | 51715–51789   | `lib/dist/resources/{enum-lookup,index}.js` | `enum://{enumName}` resource              |

Packages the build vendors, from `lib/package.json` dependency rows: `@modelcontextprotocol/sdk` (low-level Server over stdio), `@oclif/core` (launcher commands), `acorn` with `acorn-walk` and `astring` (transforms), `detect-port` (port election), `opentype.js` (font tables), `semver`, `uuid` (request ids), `wasm-vips` 0.0.18 (JPEG crop and recompress), `ws` (bridge), `zod` (tool schemas).

## [03]-[PROTOCOL]

Plain JSON text frames over `ws://localhost:6001`. The server constructs `new WebSocketServer({ port })` with no `perMessageDeflate` and no `maxPayload` override, so ws defaults hold: compression off, 100 MB frame cap. No size cap sits on the wire; the cap sits at the MCP result.

| [INDEX] | [STEP]           | [FRAME]                                                     | [SOURCE]                          |
| :-----: | :--------------- | :---------------------------------------------------------- | :-------------------------------- |
|  [01]   | Plugin hello     | `{"type":"plugin-hello","version"}`                          | `plugin:117-122`                  |
|  [02]   | Server accept    | `{"type":"hello-ack","serverVersion"}`                       | `lib/dist/multi-client-bridge.js:378-390` |
|  [03]   | Server reject    | `{"type":"hello-nack",code,message,description}`, `close(1008)` | `lib/dist/protocol.d.ts:28`    |
|  [04]   | Heartbeat        | `{"type":"ping"}` every 5000 ms after ack, no pong           | `lib/dist/multi-client-bridge.js:392-396` |
|  [05]   | Request          | `{"jsonrpc":"2.0",id:<uuidv4>,method,params}`                | `lib/dist/multi-client-bridge.js:261-298` |
|  [06]   | Reply            | `{"jsonrpc":"2.0",id,result}` or `{...,error:{code,message,data:{type,stack}}}` | `plugin:490,514,564` |
|  [07]   | Secondary in     | `{"type":"forward",clientId,payload}`                        | `lib/dist/multi-client-bridge.js:223-252` |
|  [08]   | Secondary out    | `{"type":"response",clientId,payload}`                       | `lib/dist/multi-client-bridge.js:612-621` |

Handshake rules: the first frame must identify. Reject codes are `VERSION_MISSING`, `SERVER_UPDATE_REQUIRED`, `SERVER_DOWNGRADE_REQUIRED`, `VERSION_INVALID` (`lib/dist/protocol.d.ts:28`). A second plugin connection closes with `close(1008,"Plugin already connected")` (`lib/dist/multi-client-bridge.js:323-327`). Non-JSON frames and unknown types close with 1008 (`lib/dist/multi-client-bridge.js:314,415`).

Heartbeat on the panel side: a 10000 ms timeout resets on the ack and on every ping, and expiry calls `closeAndReconnect("heartbeat-miss")` (`plugin:222-227`). The panel waits 5000 ms for the ack and retries after 2000 ms (`plugin:54-56`).

Methods the panel exposes: `execute{code,description}` and the legacy `snapshot{target,index}` (`plugin:483,524`). Every 1.0.27 tool rides `execute`. Error codes are `-32602` missing parameter, `-32601` unknown method, `-32000` script error, with `stack` truncated to 500 characters.

Queue: the primary holds `requestQueue` and `isProcessing`, keeps exactly one request in flight, and recurses after each settle (`lib/dist/multi-client-bridge.js:136-162`); `sendToPlugin` always enqueues (`lib/dist/multi-client-bridge.js:451-464`). The server serializes execution, the panel does not.

Timeouts: `REQUEST_TIMEOUT = 30000` ms per request (`lib/dist/multi-client-bridge.js:28,275,623`). On expiry the pending entry is deleted and the promise rejects; a reply arriving after that logs `No pending request for id`. A panel disconnect rejects every pending request with `"Plugin disconnected"` and clears the ping interval (`lib/dist/multi-client-bridge.js:427-438`).

Election: `detect-port(6001)` free makes the process primary, taken makes it secondary (`lib/dist/multi-client-bridge.js:108-118`). A secondary forwards to the primary and the primary answers; forwarded errors flatten to `{code:-32000,message}` (`lib/dist/multi-client-bridge.js:249`). Election serves several MCP clients sharing one InDesign.

## [04]-[JOB_EXECUTION]

The panel runs code through the async function constructor and awaits it (`plugin:423-568`):

```js
const AsyncFunction = Object.getPrototypeOf(async () => {}).constructor;
const result = await new AsyncFunction(code)();
```

No `doScript`, no undo mode, no undo grouping. The result frame is `{success:true,result}` or `{success:false,error,errorType,stack}` (`plugin:429-434`).

Preference save and restore lives inside the generated script, not around the call. `lib/dist/tools/snapshot-render.js:311-315,458-461` set `app.scriptPreferences.measurementUnit = MeasurementUnits.POINTS` and `userInteractionLevel = UserInteractionLevels.NEVER_INTERACT` and restore both in `finally`. `exportBase64` snapshots the five `app.jpegExportPreferences` fields `jpegQuality`, `exportResolution`, `pageString`, `jpegExportRange`, `exportingSpread` and restores them in `finally` (`lib/dist/tools/snapshot-render.js:66-97`).

Serialization happens at the server: `JSON.stringify(result, null, 2)`, with `undefined` rendered as the string `"undefined"` (`lib/dist/tools/execute.js:133`). Generated scripts return plain objects built from reads. An error reaches the model as `` `${notesBlock}Error: ${message}` `` with `isError: true` (`lib/dist/tools/execute.js:170-178`).

## [05]-[CODE_TRANSFORMS]

`transformCode` runs two passes, collection then enum, and returns the code untouched when the parse fails (`lib/dist/lib/code-transforms.js:687-695`). Notes are generated before mutation (`lib/dist/lib/code-transforms.js:511-515`), deduped and capped at 10 followed by an `…and N more` line (`lib/dist/tools/execute.js:12-23`), and emitted as a leading `[Autocorrected before execution]` block on success and on error.

Collection bracket indexing rewrites `collection[i]` to `__sk_item(collection, i, "Pages")` with the helper declared as `const __sk_item = (c, i, n) => c?.constructor?.name === n ? c.item(i) : c[i];` (`lib/dist/lib/code-transforms.js:408`). `COLLECTION_CLASS_NAMES` holds 58 entries verified over 62 owner and property pairs: every collection reports the capitalized property name, with `xmlElements` reporting `XMLElements` and `xmlTags` reporting `XMLTags` (`lib/dist/lib/code-transforms.js:297-355`). Guards: a dataflow classifier marks variables assigned from `.getElements()`, `.everyItem()`, an `/^all[A-Z]/` property, or an array literal as arrays and never rewrites them (`lib/dist/lib/code-transforms.js:363-388`); ambiguous reassignment drops the variable (`lib/dist/lib/code-transforms.js:425-441`); string and template indices are skipped; write contexts `c[i]=x`, `c[i]++`, `delete c[i]` are skipped (`lib/dist/lib/code-transforms.js:490-502`); the base expression is evaluated once.

Enum values rewrite `obj.prop = <literal>` to `__sk_enum(obj,"prop",Enum.CONST,<original>,<dual>)` with the helper `const __sk_enum = (o, k, e, f, d) => { const c = o[k]; return (o[k] = c?.constructor?.name === "Enumerator" || (d && (typeof c === "number" || Array.isArray(c))) ? e : f); };` (`lib/dist/lib/code-transforms.js:144`). Models emit FourCC integers such as `autoSizingType = 1634494067` or constant-name strings such as `"CENTER_ALIGN"`, `"points"`, `"allCaps"`. The guard tests the property's current value because every enum value reports `constructor.name === "Enumerator"`. `DUAL_TYPED_ENUMS = {Leading, UIColors}` also accept a numeric or array current value (`lib/dist/lib/code-transforms.js:142`). The FourCC heuristic accepts an integer in the open interval (1 000 000 000, 2 200 000 000) (`lib/dist/lib/code-transforms.js:21-27`). Candidates come from every `*.propName` key in the mappings, and a value matching none is left alone (`lib/dist/lib/code-transforms.js:47-68`). Name matching runs exact, then normalized by uppercasing and stripping `_`, and accepts only a single matching constant (`lib/dist/lib/code-transforms.js:83-106`). Only `=` is rewritten.

Missing imports: an `EnumName.CONST` for a known enum that is not destructured from `require('indesign')` is added to that destructuring or to a new `const {…} = require('indesign');` line prepended to the code, sorted and deduped (`lib/dist/lib/code-transforms.js:218-285`); a bare `app.*` with no `app` import adds `app` (`lib/dist/lib/code-transforms.js:175-213`).

## [06]-[TOOLS]

`lib/dist/tools/index.js` registers `execute`, `snapshot`, `snapshot_object`, `show_snapshot`, `get_layout`, `get_font_metrics`; `lib/dist/launcher.js` adds `get_health`, `get_configuration`, `set_configuration`. Output schemas exist on `get_layout`, `get_font_metrics`, and `get_health` alone.

| [INDEX] | [TOOL]              | [INPUT]                                          | [OUTPUT]                              |
| :-----: | :------------------ | :----------------------------------------------- | :------------------------------------ |
|  [01]   | `execute`           | `{code, description?}`                            | Serialized result text                |
|  [02]   | `snapshot`          | `{target, index, region?, preview?}`              | One base64 JPEG image block           |
|  [03]   | `snapshot_object`   | `{pageItemId, isolate?}`                          | Image block plus geometry meta        |
|  [04]   | `get_layout`        | `{includeItems?}`                                 | Page bounds, margins, content area    |
|  [05]   | `get_font_metrics`  | `{fontIdentifier, style?, refresh?}`              | Font table metrics in font units      |
|  [06]   | `get_health`        | none                                              | Server, plugin, probe, connection rows |

`execute` runs `transformCode` and then `sendToPlugin("execute",{code,description})`. Its tool description is a DOM cheat sheet of roughly 100 lines, the source of the quirk rows below.

`snapshot` takes `target` of `"page"` or `"spread"`, an integer `index` at 0 or above, an optional `region` of `[x0,y0,x1,y1]` normalized 0 to 1 from a top-left origin for a page target, validated as `0 ≤ n ≤ 1`, `x1 > x0`, `y1 > y0` (`lib/dist/tools/snapshot.js:63-65`), and an optional `preview` flag. Resolution comes from a pixel budget (`lib/dist/tools/snapshot-render.js:36-43`):

```text
dpi = clamp(round(min(maxLongEdgePx / longEdgeIn, sqrt(maxPixels / areaIn))), 48, 600)
budget         = {1568 px long edge, 1 150 000 px total}
preview budget = {768 px long edge, 300 000 px total}
```

`preview` is ignored for a region capture and for an object capture. A page capture reads `page.bounds` for width and height, derives dpi, and calls `exportBase64(doc, "+" + (index + 1), false, dpi)`. A spread capture sums page widths, takes the maximum page height, sets `pageString` to `"+first"` or `"+first-+last"`, and sets `exportingSpread = true` (`lib/dist/tools/snapshot-render.js:330-349`).

Region and object isolation builds a temporary document (`lib/dist/tools/snapshot-render.js:109-302`): `app.documents.add(false)`, delete the extra pages, set `facingPages = false`, zero the margins before resizing, set `pageWidth` and `pageHeight`, set view units to points. The page floors at `MIN_PAGE_PT = 216` and dpi derives from the floored size. Geometry comes from `pageSpaceBounds`, the minimum and maximum of the four corners returned by `item.resolve([AnchorPoint.*, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS], CoordinateSpaces.SPREAD_COORDINATES)`. Z-order comes from `container.allPageItems` reversed and filtered to direct children of a `Spread` or `MasterSpread`, with applied masters placed first. Items whose bounding box is disjoint from the crop are skipped; the rest are duplicated into the temporary spread and moved by a relative `move(undefined, [dx+"pt", dy+"pt"])`. Padding bands are masked with `rectangles.add()` filled `colors.item("Paper")` and stroked `swatches.item("None")`. The temporary document closes with `tmp.close(SaveOptions.NO)` in `finally`.

`snapshot_object` looks up `spread.pageItems.itemByID(id).isValid` and takes `getElements()[0]` for the concrete class, falling back to a scan of `spread.allPageItems` (`lib/dist/tools/snapshot-render.js:370-387`). It computes `overlaps[]` holding the id, name, and type of every sibling whose spread bounding box intersects, reads `effectivePpi` from the contained graphic first, and applies the dpi cap only when the capture is single-material, meaning isolated or with zero overlaps (`lib/dist/tools/snapshot-render.js:427-432`). Meta carries `{type,widthPx,heightPx,dpi,contentWidthPx,contentHeightPx,effectivePpi,isolated,overlaps,note}`.

Server-side image post-processing runs in `lib/dist/tools/image-crop.js`: `cropToContent` applies the content-to-full ratio through wasm-vips `extractArea` and re-encodes at `Q:90` with `optimize_coding` and `trellis_quant`; `compressToFit` targets 400 000 base64 characters over the ladder Q 80, 70, 60, then multiplies `scale` by 0.8 with Q reset, stopping at `scale ≤ 0.2`.

`get_layout` reports the first 20 pages (`lib/dist/tools/get-layout.js:129-130`). Per page it normalizes `bounds` to the origin, reads margins through `parseFloat(String(mb.*))`, and derives the content area by page side (`lib/dist/tools/get-layout.js:149-165`):

```text
LEFT_HAND           : left = mb.right,  right = pageWidth - mb.left
RIGHT_HAND, single  : left = mb.left,   right = pageWidth - mb.right
both                : top  = mb.top,    bottom = pageHeight - mb.bottom
```

`includeItems` walks `pageItems.everyItem().getElements()`, recurses into groups, caps at 200 items (`lib/dist/tools/get-layout.js:89`), and emits `{id,type,name,bounds,hasGraphic}` with `type = String(it.constructor.name)` and `hasGraphic = it.graphics.length > 0`. Enum values render through `String(...)`.

`get_font_metrics` parses a self-read file with `opentype.parse(buffer.buffer)` and returns names, `unitsPerEm`, `ascender`, `descender`, `lineGap = hhea.lineGap ?? os2.sTypoLineGap ?? 0`, `xHeight` and `capHeight` from OS/2 `sxHeight` and `sCapHeight` with a fallback to the bounding-box `y2` of glyph `x` or `H` recorded as `xHeightSource` and `capHeightSource` of `"os2"` or `"measured"`, the `head` bounding box, `post.underlinePosition` and `underlineThickness`, OS/2 strikeout, subscript and superscript metrics, `isMonospace`, and `filePath`, all in font units. Scan roots on macOS are `/Library/Fonts`, `/System/Library/Fonts`, `/System/Library/Fonts/Supplemental`, `~/Library/Fonts`, and `~/Library/Application Support/Adobe/CoreSync/plugins/livetype/.r`, walked to depth 3, where an extension-less file matching `/^[a-f0-9-]{8,}$/i` is an Adobe Fonts file. The index caches at `~/.cache/indesign-sidekick/font-index.json` with a 24 hour time to live. Match order is exact PostScript name, exact full name without style, exact family, partial, style exact then style substring, `"regular"`, first.

`get_health` returns `{server{version,role,updateStatus,devMode,pid,uptimeSeconds}, plugin{version,connected,disconnectionReason?}, probe{ok,error?}, connections{...}, lastSuccessfulCommandSecondsAgo, lastError{message,count,secondsAgo}}` (`lib/dist/launcher.js:194-288`). The probe is a real end-to-end `execute` of `{code:"1"}` recorded as `{record:false}` under a 5000 ms race.

## [07]-[ENUM_MAPPINGS]

`lib/dist/data/enum-mappings.json` holds four top-level objects:

```text
enums        : { <EnumName>: { <CONSTANT>: { value: <fourcc int>, description? } } }   425 enums
properties   : { "<OwnerType>.<propName>": "<EnumName>" }                              2 344 rows
fourccToEnum : { "<int>": { enum, constant } }                                         1 769 rows
methods      : { "Document.exportFile": {"0":"ExportFormat"}, … }                      3 rows
```

`findEnumsForProperty` matches the `.propName` suffix and ignores the owner; `findEnumConstantForValue`, `findEnumConstantForName`, and `isKnownEnum` read `enums`; `lib/dist/resources/enum-lookup.js` serves the `enum://{enumName}` resource. `fourccToEnum` and `methods` have no reader.

Regeneration: `require('indesign')` exposes every enum object, so `Object.entries(Enum)` yields the names and FourCC integers in-process at panel start. The `properties` owner map is not derivable from the DOM, and the guard works without it: a slot whose current value reports `constructor.name === "Enumerator"` is an enum slot, and the constant is identified by a unique name lookup across every enum. `fourccToEnum`, `methods`, and the `description` text carry no reader and are dropped.

## [08]-[DOM_QUIRKS]

| [INDEX] | [QUIRK]                                                                    | [WORKAROUND]                                                       | [SOURCE]                                     |
| :-----: | :------------------------------------------------------------------------- | :----------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `collection[i]` returns `undefined`                                         | `.item(i)`, `.firstItem()`, `.everyItem().getElements()`, `__sk_item` | `lib/dist/lib/code-transforms.js:389-408`    |
|  [02]   | `===` and `==` are always false on DOM objects and enumerators              | `.equals()`, or `String(value)` for an enum name                    | `lib/dist/tools/execute.js:54-58`; `lib/dist/tools/get-layout.js:117-126` |
|  [03]   | Every enumerator reports `constructor.name === "Enumerator"`                | Basis of `__sk_enum`                                                | `lib/dist/lib/code-transforms.js:107-127`    |
|  [04]   | Collections report the capitalized property name; `xmlElements` → `XMLElements` | 58-entry name map                                                | `lib/dist/lib/code-transforms.js:290-296`    |
|  [05]   | `Leading` and `UIColors` properties read back as a number or a 3-array      | `DUAL_TYPED_ENUMS`                                                  | `lib/dist/lib/code-transforms.js:128-142`    |
|  [06]   | Type-dependent properties throw "The property is not applicable in the current state" | Check the type, or try/catch                              | `lib/dist/tools/execute.js:48`               |
|  [07]   | An integer FourCC or constant string assigned to an enum property           | Autocorrect, or let InDesign reject it                              | `lib/dist/tools/execute.js:59-66`            |
|  [08]   | `\n` renders without word spacing                                           | `\r` for a paragraph break                                          | `lib/dist/tools/execute.js:68-70`            |
|  [09]   | Bare numbers take the document units                                        | Unit strings such as `"11pt"`                                       | `lib/dist/tools/execute.js:34-38`            |
|  [10]   | Some properties return Promises, `doc.fullName` among them                  | Await                                                               | `lib/dist/tools/execute.js:73`               |
|  [11]   | `doc.masterSpreads[0].pages` is undefined right after `documents.add()`     | Pass `marginPreferences` to `documents.add()`                       | `lib/dist/tools/execute.js:74`               |
|  [12]   | Facing-page `geometricBounds` are not mirrored, margins are                 | Compute recto and verso from `page.side` and `page.marginPreferences` | `lib/dist/tools/execute.js:82-87`; `lib/dist/tools/get-layout.js:149-165` |
|  [13]   | A temporary page below about 216 pt throws "Data is out of range"           | Floor to `MIN_PAGE_PT`, mask padding with Paper, crop server-side   | `lib/dist/tools/snapshot-render.js:57-60,245-257` |
|  [14]   | Zeroing margins after resizing, or on a small page, throws                  | Zero margins first, then resize                                     | `lib/dist/tools/snapshot-render.js:106-118`  |
|  [15]   | `geometricBounds` is local and pre-transform, wrong for a rotated item      | `resolve([anchor, GEOMETRIC_PATH_BOUNDS], SPREAD_COORDINATES)`      | `lib/dist/tools/snapshot-render.js:126-154`  |
|  [16]   | The spread origin is the spread center                                      | Anchor to `pageSpaceBounds(page)`                                   | `lib/dist/tools/snapshot-render.js:190-194`  |
|  [17]   | `pageItems.everyItem()` is not z-ordered, `allPageItems` is front to back   | Walk reversed, keep direct children                                 | `lib/dist/tools/snapshot-render.js:156-173`  |
|  [18]   | `move()` far outside a small pasteboard throws and leaves the duplicate     | Pre-filter disjoint items                                           | `lib/dist/tools/snapshot-render.js:200-214`  |
|  [19]   | Setting `geometricBounds` moves the frame and not the placed content        | Use `move()`                                                        | `lib/dist/tools/snapshot-render.js:175-179`  |
|  [20]   | Master content must render behind page items                                | Place masters first per `page.appliedMaster`                        | `lib/dist/tools/snapshot-render.js:229-242`  |
|  [21]   | `itemByID` yields a generic PageItem, `pageItems` is top level only         | `.getElements()[0]`, and `allPageItems` for the scan                | `lib/dist/tools/snapshot-render.js:370-385`  |
|  [22]   | Document units corrupt geometry reads                                       | `scriptPreferences.measurementUnit = POINTS`, restored in `finally` | `lib/dist/tools/snapshot-render.js:304-315,458-461` |
|  [23]   | A modal alert during capture blocks the bridge                              | `userInteractionLevel = NEVER_INTERACT`, restored in `finally`      | `lib/dist/tools/snapshot-render.js:308-315`  |
|  [24]   | `app.jpegExportPreferences` hold the user's own settings                    | Save and restore all five in `finally`                              | `lib/dist/tools/snapshot-render.js:66-97`    |
|  [25]   | `app.documents.add(true)` steals focus                                      | `add(false)`                                                        | `lib/dist/tools/snapshot-render.js:111-112`  |
|  [26]   | A refused UXP connection arrives as `onclose` instead of a throw            | `socketOpened` flag                                                 | `plugin:45-51,151-155`                       |
|  [27]   | UXP sockets deliver queued events late                                      | Null all four handlers before abandoning a socket                   | `plugin:75-86`                               |
|  [28]   | `close()` can throw                                                         | Wrap the call                                                       | `plugin:137-148`                             |
|  [29]   | No shell, no BridgeTalk, sandboxed file system, UTF-8 only                  | —                                                                   | `lib/dist/tools/execute.js:40-45`            |

## [09]-[MANIFEST]

`uxp/8ebe7f95_1.0.22/manifest.json` declares `manifestVersion: 5`, `id: "8ebe7f95"`, `name: "Sidekick"`, `version: "1.0.22"`, `main: "index.html"`, and `host: {app: "ID", minVersion: "19.0"}`. Required permissions are `localFileSystem: "fullAccess"`, `network.domains: ["ws://localhost:6001","wss://localhost:6001"]`, and `launchProcess.schemes: ["https"]`. One `panel` entrypoint with id `mcpPanel` carries `minimumSize` 200 by 100 and themed 23 px icons; the plugin icon is 48 px at scales 1 and 2.

## [10]-[PRODUCT_PLUMBING]

| [INDEX] | [PART]                             | [SOURCE]                                     | [REASON IT IS NOT CARRIED]                       |
| :-----: | :--------------------------------- | :------------------------------------------- | :----------------------------------------------- |
|  [01]   | Port election and forwarding        | `lib/dist/multi-client-bridge.js:108-118,223-252` | One MCP client per host                     |
|  [02]   | Auto-update and update notice       | `lib/dist/lib/update-notice.js`, `lib/dist/lib/launcher-update.js` | Version comes from the repository |
|  [03]   | Telemetry envelope                  | `lib/dist/lib/posthog.js`, `lib/dist/lib/telemetry.js` | No telemetry                            |
|  [04]   | `get_configuration`, `set_configuration` | `lib/dist/launcher.js`                  | One boolean, `telemetry`                         |
|  [05]   | `show_snapshot` and the MCP Apps viewer | `lib/dist/resources/snapshot-view*.js`   | Viewer surface of the product                    |
|  [06]   | oclif commands and packaging scripts | `lib/bin/`, `lib/scripts/`                  | Product build and installer                      |
|  [07]   | Claude Desktop registration         | `lib/dist/lib/claude-desktop.js`             | Registration belongs to the repository           |

The probe inside `get_health` is carried: it settles whether the panel answers rather than whether the socket is open.

## [11]-[DEFECTS]

| [INDEX] | [DEFECT]                                                                     | [SOURCE]                                   |
| :-----: | :--------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | The panel writes `description` into the activity element through `innerHTML`  | `plugin:411`                               |
|  [02]   | The legacy `snapshot` method fixes `exportResolution` at 72 and restores none of the five JPEG preferences | `plugin:452-456`     |
|  [03]   | `get_layout` truncates at 20 pages and 200 items with no cursor and no flag in the result | `lib/dist/tools/get-layout.js:89,129-130` |
|  [04]   | A reply arriving after the 30 s timeout is logged and discarded, the model receives the timeout alone | `lib/dist/multi-client-bridge.js:28,275,623` |
|  [05]   | `fourccToEnum` (1 769 rows) and `methods` are built into the mappings file with no reader | `lib/dist/data/enum-mappings.json` |
|  [06]   | A parse failure returns the code untouched with an empty transformation list, so the model sees no note | `lib/dist/lib/code-transforms.js:419-420,558-560` |
