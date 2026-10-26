# [ILLUSTRATOR]

Illustrator answers `do javascript` with its last expression as Unicode text (`utxt`), and `do script` executes an Actions panel action.

## [01]-[CALLS]

Calls needing structured results encode JSON in ExtendScript. Source holds the body as an `encodeURIComponent` string literal, compiles it through `eval` of a function expression, and returns the encoded value, or empty text for no value:

```js
(function (body) {
    function encoded(value) {
        var parts = [], key, at, kind;
        if (value === null || value === undefined || (typeof value === "number" && !isFinite(value))) return "null";
        if (typeof value === "number" || typeof value === "boolean") return String(value);
        if (typeof value === "string") return '"' + value.replace(/[\x00-\x1f"\\]/g, function (c) { return "\\u" + ("000" + c.charCodeAt(0).toString(16)).slice(-4); }) + '"';
        kind = Object.prototype.toString.call(value);
        if (kind === "[object Array]") { for (at = 0; at < value.length; at++) parts.push(encoded(value[at])); return "[" + parts.join(",") + "]"; }
        if (kind !== "[object Object]") return encoded(String(value));
        for (key in value) parts.push(encoded(key) + ":" + encoded(value[key]));
        return "{" + parts.join(",") + "}";
    }
    var value = eval("(function () {" + decodeURIComponent(body) + "\n})").call(this);
    return value === undefined ? "" : encodeURIComponent(encoded(value));
}).call(this, "<body>")
```

```ts
const ran = <E, R>(send: (source: string) => Effect.Effect<string, E, R>, source: string) =>
    Effect.flatMap(send(source), (reply) => (reply === '' ? Effect.succeedNone : Effect.asSome(Schema.decodeEffect(Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Schema.Json))))(reply))));
```

`send` passes `source` as the direct parameter of `do javascript` through an Apple event sender, and a throw or a syntax error fails as the Apple event error Illustrator answers. Host objects encode as their `String` form (`[Document Untitled-1]`), and bodies return JSON-shaped data.

- `with arguments` (`«class JArg»`) passes text, numbers, booleans, and lists, `null` and records as `undefined`, and `osascript` `argv` as text
- ExtendScript reads U+2028 and U+2029 as line terminators, and a string literal holding one raw fails to compile, in source and in JSON alike
- Uncaught throws and syntax errors answer Apple event error 5001 with no dialog and text `Error <number>: <message>.Line: <line>->  <source line>`
- Photoshop ExtendScript runs `new Function` of a source with a syntax error as a function answering `undefined`, where `eval` of it throws, and answers an `undefined` last value as the text `undefined`
- Top-level `var` and `$.global` members persist between calls, and a source holds its state inside one function
- `do javascript` runs a file passed as direct parameter (`POSIX file "<path>"`) with the same `arguments`
- Events sent before Illustrator finishes launching fail -600 or get no reply, and a `launch` event to it returns once it accepts them
- `AIAgenticSystem.aip` serves MCP at `http://127.0.0.1:18412/v1/mcp` with bearer token `plugin/AIMCPServer/AuthToken` of `Adobe Illustrator Prefs`

## [02]-[RUNTIME]

ExtendScript is ES3 with `const`, `toSource`, and `uneval`:
- `let`, arrow functions, getter literals, and ES3 reserved words as unquoted keys (`boolean`) are syntax errors
- `JSON`, `Symbol`, `Object.keys`, `Object.defineProperty`, `Array.isArray`, `Function.prototype.bind`, and `String.prototype.trim` read `undefined`
- `Array.prototype` holds no `map`, `forEach`, `filter`, `reduce`, or `indexOf`
- `toSource()` writes JavaScript source (unquoted keys, `undefined`, `NaN`), never JSON
- `$.hiresTimer` readings disagree with wall time (negative values, minutes for a call returning in seconds), and timing reads `Date` differences

## [03]-[OBJECT_MODEL]

`<object>.reflect.properties` rows hold `name`, `type` (`readonly`, `readwrite`), and `dataType`, `reflect.methods` names each method, `reflect.find(<property>)` reads one row and throws 1302 for a method name, and `$.dictionary` reads `null`:

| [INDEX] | [NEED]                 | [FORM]                                        | [DECIDING_FACT]                                       |
| :-----: | :--------------------- | :-------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Class of a value       | `typename` (`Document`, `Layers`, `PathItem`) | `Object.prototype.toString` reads `[Document <name>]` |
|  [02]   | Member by name         | `getByName(name)`                             | Absent name throws 1302, no `itemByName` or `isValid` |
|  [03]   | Member by position     | `collection[index]`                           | Index past the end throws 1200 `MRAP`                 |
|  [04]   | Item held across calls | `uuid`, then `getPageItemFromUuid(uuid)`      | Repeated reads of one item compare `===` true         |
|  [05]   | Active document        | `app.activeDocument`                          | Throws 1302 with no document open                     |

- Geometry is in points under `app.coordinateSystem` `DOCUMENTCOORDINATESYSTEM`, y growing upward from the first artboard's bottom edge
- `pathItems.rectangle(top, left, width, height)` places an item with `position` `[left, top]` and `geometricBounds` `[left, top, right, bottom]`
- ExtendScript holds no listener API (`app.addEventListener` reads `undefined`), and a script reads state when it runs
- `app.preferences` getters of an unregistered key return a varying value and store the key, where `preferenceExists(key)` stores none
- Illustrator rewrites `Adobe Illustrator Prefs` whole at quit, and edits to the file while it runs are lost

## [04]-[UNITS_OF_WORK]

Each `do javascript` call is one undo step, and `app.undo()` in a later call reverts every change the call made:
- Throws keep the changes made before them, and their step stays on the undo stack
- `app.undo()` inside a call reverts that call's changes and the previous call's step together, and code undoes through a call of its own
- `app.userInteractionLevel` outlasts the call, and a call writing `DONTDISPLAYALERTS` restores the value it read in `finally`
- `document.close(SaveOptions.DONOTSAVECHANGES)` closes a document with no dialog

## [05]-[HOST_ERRORS]

Caught `Error` values hold `number`, `message`, and `line`, and code branches on `number`:

| [INDEX] | [NUMBER] | [RAISED_BY]                                                               |
| :-----: | :------: | :------------------------------------------------------------------------ |
|  [01]   |   1302   | Absent name, active document with none open, `reflect.find` of a method   |
|  [02]   |   1239   | Number past a property's range                                            |
|  [03]   |   1226   | String written to an enumeration property                                 |
|  [04]   |   1224   | Non-number written to a number property                                   |
|  [05]   |   1200   | Internal refusal, message `an Illustrator error occurred: <n> ('<code>')` |
|  [06]   |    54    | Uncaught `Error`, message `Uncaught JavaScript exception: <message>`      |
|  [07]   |    45    | Read of a removed item                                                    |
|  [08]   |    25    | Syntax past ES3                                                           |
|  [09]   |    21    | Member read on `undefined` or `null`                                      |
|  [10]   |    9     | Reserved word as an unquoted name                                         |
|  [11]   |    4     | Unterminated string constant                                              |
|  [12]   |    2     | Undeclared name                                                           |
|  [13]   |    1     | `new Error(<message>)` from source                                        |

- Four-character codes in 1200 messages read reversed (`MRAP` is `PARM`, a bad parameter)

## [06]-[EXPORT]

| [INDEX] | [OUTPUT]           | [CALL]                                                                      |
| :-----: | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | PNG, SVG, others   | `document.exportFile(file, ExportType.<TYPE>, new ExportOptions<TYPE>())`   |
|  [02]   | PDF                | `document.saveAs(file, new PDFSaveOptions())`                               |
|  [03]   | Raster of a rect   | `document.imageCapture(file, rect, new ImageCaptureOptions())`              |
|  [04]   | Artboards at scale | `document.exportForScreens(folder, ExportForScreensType.SE_PNG24, options)` |

- Untitled documents read `fullName` `/Untitled-1` and an empty `path.fsName`
- `saveAs` renames the open document to the written file and marks it saved
- `exportForScreens` writes `<factor>x/<artboard name>.png`, into the target's parent when the target folder does not exist
- Image capture, raster, and export resolutions take 72 to 2400

## [07]-[TEXT_AND_FONTS]

`app.textFonts` lists every installed face, `getByName(<PostScript name>)` reads one and throws 1302 for an absent name, and a `TextFont` holds `name` (PostScript), `family`, and `style`:
- `textFrames.add()` takes text through `contents`, set in `MyriadPro-Regular` by default
- `textRange.characterAttributes.textFont` takes a `TextFont`, and `size` takes points
- `createOutline()` on a duplicate frame returns a `GroupItem` with glyph outline bounds as `geometricBounds`

## [08]-[PLUG_INS]

Illustrator loads UXP plugins from `Contents/Required/UXP/extensions` alone:
- UXP logs hold no `upic::` line, and a row in `UXP/PluginsInfo/v1/AI.json` loads nothing
- `XManConfig.xml` lists no `UxpHostCode` for `ILST`, and `UnifiedPluginInstallerAgent` installs no UXP plugin into Illustrator
- `AIUXPExtensionHostAPI` registers module `illustrator` (`{ app }`, each member forwarded to ExtendScript) for third-party plugins none can load
