# [EXTENDSCRIPT]

Illustrator runs ExtendScript through `do javascript`, and After Effects `DoScript` replies with `app.exitCode` alone, reset to 0 per script. Illustrator answers `do javascript` with its last expression as Unicode text (`utxt`), and `do script` executes an Actions panel action.

## [01]-[CALLS]

Unlinked calls send the body and the reply encoder as one source text:

```ts
import { transformSync } from '@swc/wasm';

const Reply = Schema.Union([
    Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
    Unmatched,
    Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
    Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
]);
type Replied<Tag extends (typeof Reply.Encoded)['_tag']> = Extract<typeof Reply.Encoded, { readonly _tag: Tag }>;

const runtime = () => {
    const isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null;
    const finite = (value: unknown): number | undefined => (typeof value === 'number' && Math.abs(value) < Number.POSITIVE_INFINITY ? value : undefined);
    const unmatched = (subject: string, query: string, candidates: readonly string[]): Replied<'Unmatched'> => ({ _tag: 'Unmatched', subject, query, candidates });
    const illegal = (text: string): Replied<'IllegalArgument'> => ({ _tag: 'IllegalArgument', message: text });
    const reply = (answer: () => unknown, evaluated = (_thrown: unknown): boolean => true): string => {
        try {
            return encoded({ _tag: 'Value' satisfies Replied<'Value'>['_tag'], value: answer() });
        } catch (thrown) {
            if (isRecord(thrown) && (thrown._tag === 'Unmatched' || thrown._tag === 'IllegalArgument')) {
                return encoded(thrown);
            }
            const fields = isRecord(thrown) ? thrown : { message: thrown };
            return encoded({ _tag: 'Thrown' satisfies Replied<'Thrown'>['_tag'], message: String(fields.message), number: finite(fields.number), line: evaluated(thrown) ? finite(fields.line ?? fields.lineNumber) : undefined });
        }
    };
    return { unmatched, illegal, reply };
};
const encoder = transformSync(`var runtime = ${String(runtime)};`, { jsc: { target: 'es3', parser: { syntax: 'ecmascript' }, assumptions: { iterableIsArray: true } } }).code;
const sent = (body: string) => `(function (self) { var body = ${body}\n${encoder}\nreturn encodeURIComponent(runtime().reply(function () { return body.call(self); }));\n})(this)`;
const Answer = Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.fromJsonString(Reply)));
```

`encoded` is the ES3 JSON writer `runtime` declares beside `reply`, and `body` a function expression the same transform lowered. `send` passes `sent(body)` as the `do javascript` direct parameter through an Apple event sender, and `Answer` decodes its reply. Host code throws `unmatched` and `illegal` records, `reply` writes them as their own cases, and `evaluated` decides which throws keep `line`. One `answered(success, subject)` folds every reply on both paths: `Value` decodes through the call's success schema, `Unmatched` fails as itself, `IllegalArgument` fails `IllegalArgumentError`, and `Thrown` fails `HostError` `Thrown` when it holds a line and `Refused` without one, its subject joined with `Error <number>`. `encoded` writes non-finite numbers as `null` and plain records by own keys, skipping a key holding `undefined`, and an unset field decodes through `Schema.optionalKey` and `OptionFromOptionalKey` as absent. Host objects a host specifier names encode as `{type, specifier, name, valid}` with `type` from Illustrator `typename`, and every other host object as its `String` form (`[Document Untitled-1]`).

- `with arguments` (`«class JArg»`) passes text, numbers, booleans, and lists, `null` and records as `undefined`, and `osascript` `argv` as text
- ExtendScript reads U+2028 and U+2029 as line terminators, and a string literal holding one raw fails to compile, in source and in JSON alike
- Uncaught throws and syntax errors answer Apple event error 5001 with no dialog and text `Error <number>: <message>.Line: <line>->  <source line>`
- `alert`, `confirm`, and `prompt` activate the host, and `new Function` parameters of those names shadow them with `() => undefined`, `() => false`, and `() => null`
- Top-level `var` and `$.global` members persist between calls, and a source holds its state inside one function
- `do javascript` runs a file passed as direct parameter (`POSIX file "<path>"`) with the same `arguments`
- Events sent before Illustrator finishes launching fail -600 or get no reply, and a `launch` event to it returns once it accepts them
- `AIAgenticSystem.aip` serves MCP protocol `2025-03-26` at `/v1/mcp` on the first free port of `127.0.0.1:18412`-`18422`, by launch order across builds, with bearer token `plugin/AIMCPServer/AuthToken` of that build's `Adobe Illustrator Prefs`, refusing any `Origin` header

## [02]-[RUNTIME]

ExtendScript is ES3 with `const`, `toSource`, and `uneval`:
- `let`, arrow functions, getter literals, and ES3 reserved words as unquoted keys (`boolean`) are syntax errors
- `JSON`, `Symbol`, `Object.keys`, `Object.defineProperty`, `Array.isArray`, `Array.from`, `Function.prototype.bind`, and `String.prototype.trim` read `undefined`
- `Array.prototype` holds no `map`, `forEach`, `filter`, `reduce`, or `indexOf`
- ExtendScript reads `a || b && c` as `(a || b) && c`, and swc removes the parentheses protecting it, so `lower` restores them at `es3`
- `toSource()` writes JavaScript source (unquoted keys, `undefined`, `NaN`), never JSON
- `File.name` reads the URI-encoded name (`My%20File.ai`), and `displayName` the decoded name a reply holds
- `$.hiresTimer` readings disagree with wall time (negative values, minutes for a call returning in seconds), and timing reads `Date` differences

Host code and bodies are TypeScript that swc lowers under `jsc.target: 'es3'`, where `assumptions.iterableIsArray` turns `for...of` into an index loop alone. Other forms emit helpers calling members ExtendScript lacks, and host code holds plain functions over literal tuples:

| [INDEX] | [FORM]                                                              | [HELPER]               | [READS]                       |
| :-----: | :------------------------------------------------------------------ | :--------------------- | :---------------------------- |
|  [01]   | Array pattern over anything but an array literal (`const [a] = xs`) | `_sliced_to_array`     | `Array.isArray`               |
|  [02]   | Array spread                                                        | `_to_consumable_array` | `Array.isArray`, `Array.from` |
|  [03]   | Class with methods                                                  | `_create_class`        | `Object.defineProperty`       |
|  [04]   | Object spread                                                       | `_object_spread`       | `Object.keys`, `forEach`      |

- Array patterns over an array literal (`const [a, b] = [x, y]`) lower to index reads, declarations, parameters, and assignments alike

## [03]-[OBJECT_MODEL]

Illustrator values name their class and address through host members:

| [INDEX] | [NEED]                 | [FORM]                                        | [REASON]                                              |
| :-----: | :--------------------- | :-------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Class of a value       | `typename` (`Document`, `Layers`, `PathItem`) | `Object.prototype.toString` reads `[Document <name>]` |
|  [02]   | Member by name         | `getByName(name)`                             | Absent name throws, no `itemByName` or `isValid`      |
|  [03]   | Member by position     | `collection[index]`                           | Index past the end throws 1200 `MRAP`                 |
|  [04]   | Item held across calls | `uuid`, then `getPageItemFromUuid(uuid)`      | Repeated reads of one item compare `===` true         |
|  [05]   | Active document        | `app.activeDocument`                          | Throws with no document open                          |

Handlers return host objects, and one host `specifier` function builds every reply's specifier from the object:
- Illustrator names a document by `fullName`, unique where `name` repeats and `/Untitled-1` untitled, and a page item by `uuid` under its document
- Illustrator names artboards and layers by position in `document.artboards` or their parent's `layers`, the parent's specifier built through `parent`
- Geometry is in points under `app.coordinateSystem` `DOCUMENTCOORDINATESYSTEM`, y growing upward from the first artboard's bottom edge
- `pathItems.rectangle(top, left, width, height)` places an item with `position` `[left, top]` and `geometricBounds` `[left, top, right, bottom]`
- ExtendScript holds no listener API (`app.addEventListener` reads `undefined`), and a script reads state when it runs
- `app.preferences` getters of an unregistered key return a varying value and store the key, where `preferenceExists(key)` stores none
- Illustrator rewrites `Adobe Illustrator Prefs` whole at quit, and edits to the file while it runs are lost

## [04]-[UNDO_STEPS]

`app.undo()` in a later call reverts every change one Illustrator call made:
- Throws keep the changes made before them, and their step stays on the undo stack
- `app.undo()` inside a call reverts that call's changes and the previous call's step together, and code undoes through a call of its own
- `document.close(SaveOptions.DONOTSAVECHANGES)` closes a document with no dialog

## [05]-[HOST_ERRORS]

Caught `Error` values hold `number`, `message`, `line`, and `source`, the text of the script that threw, and code branches on `number`. Replies name `line` where `source` equals the evaluated body alone, and a host-script throw holds a host-script line:

| [INDEX] | [NUMBER] | [RAISED_BY]                                                               |
| :-----: | :------: | :------------------------------------------------------------------------ |
|  [01]   |   1302   | Absent name, active document with none open                               |
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
- Option object writes recurse into a plain value where the host holds a record, and one `IllegalArgument` names the class by its `Object.prototype.toString` tag with every refused dotted path

## [06]-[EXPORT]

| [INDEX] | [OUTPUT]                              | [CALL]                                                                        |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | PNG, SVG, others                      | `document.exportFile(file, ExportType.<TYPE>, new ExportOptions<TYPE>())`     |
|  [02]   | PDF                                   | `document.saveAs(file, new PDFSaveOptions())`                                 |
|  [03]   | Raster of a rect                      | `document.imageCapture(file, rect, new ImageCaptureOptions())`                |
|  [04]   | Artboards at scale                    | `document.exportForScreens(folder, ExportForScreensType.SE_PNG24, options)`   |
|  [05]   | Document, artboard, or item at a size | Active `artboards.add(rect)`, then PNG24 `exportFile` with `artBoardClipping` |

- Rects come from `artboardRect` or `visibleBounds`, and PNG24 scale percent is `longestSide / max(rect[2] - rect[0], rect[1] - rect[3]) * 100`
- Sized exports remove the added artboard and restore the active index read before it in `finally`
- Untitled documents read `fullName` `/Untitled-1` and an empty `path.fsName`
- `saveAs` renames the open document to the written file and marks it saved
- `exportForScreens` writes `<factor>x/<artboard name>.png`, into target's parent when target folder is missing
- Image capture, raster, and export resolutions take 72 to 2400, and PNG option ranges refuse a scale the host refuses

## [07]-[TEXT_AND_FONTS]

`app.textFonts` lists every installed face, and `getByName(<PostScript name>)` reads one. Each `TextFont` holds `name` (PostScript), `family`, and `style`:
- `textFrames.add()` takes text through `contents`, set in `MyriadPro-Regular` by default
- `textRange.characterAttributes.textFont` takes a `TextFont`, and `size` takes points
- `createOutline()` on a duplicate frame returns a `GroupItem` with glyph outline bounds as `geometricBounds`
