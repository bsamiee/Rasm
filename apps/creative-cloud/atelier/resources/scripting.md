# [ADOBE_SCRIPTING]

## [01]-[APPLICATION]

Select an installation from `applications` and pass its `url` as `application`. Omission requires one application with a linked extension. Commands open a stopped application at its selected URL; use `activate` to bring it forward.

Before constructing a native command, read the returned dictionary URI for parameter types, list declarations, optionality, inheritance, and property access. Dictionary XML describes Apple Events, not a complete document API or JavaScript runtime. For operations supplied by a linked extension, use its contract and resources.

## [02]-[COMMANDS]

Pass `execute` a `run` with `_tag: "Command"`, `command`, and `arguments`. Use dictionary spelling for command and parameter names, with `direct` for a direct parameter. Omit optional arguments to retain native defaults; `null` supplies native null.

Use `get` to read `direct`, `set` to write `to` into `direct`, and `activate` without arguments. Language commands remain available when omitted from dictionary XML. Check property access before writing.

For an application declaring a `name` property, read it with:

```json
{
    "_tag": "Command",
    "command": "get",
    "arguments": {
        "direct": { "want": "'prop'", "form": "'prop'", "seld": "name", "from": null }
    }
}
```

## [03]-[REFERENCES]

Retain returned object specifiers for later commands; names and collection positions can change after edits. References belong to their application and document, and closing a document or removing an object can invalidate them. Keep Apple Event specifiers separate from a linked extension's references.

| [INDEX] | [FIELD] | [MEANING]                                                          |
| :-----: | :------ | :----------------------------------------------------------------- |
|  [01]   | `want`  | Dictionary class name or quoted native type code                   |
|  [02]   | `form`  | Quoted native selector code                                        |
|  [03]   | `seld`  | Selector value: property, position, name, ID, or native descriptor |
|  [04]   | `from`  | Containing specifier; null or omission selects application scope   |

Keep surrounding single quotes and spaces in native codes. Use `form: "'prop'"` for a property named by `seld`, `"'indx'"` for a one-based position or native ordinal, `"'name'"` for a name, and `"'ID  '"` for a native ID. Selectors must be supported by the application and class.

For an application declaring `document` elements, request all documents with:

```json
{
    "_tag": "Command",
    "command": "get",
    "arguments": {
        "direct": { "want": "document", "form": "'indx'", "seld": "'all '", "from": null }
    }
}
```

To read a returned document's name, use its specifier as `from` in a property specifier with `want: "'prop'"`, `form: "'prop'"`, and `seld: "name"`. For a writable property, pass that specifier as `set`'s `direct` and the replacement as `to`. Derive child elements and properties from class definitions, not plural names.

## [04]-[VALUES]

Use JSON primitives, arrays, and records for native values. Record keys resolve through dictionary properties; retain a returned record's typed `class` member to preserve class context. Declared parameter types select enum names, type names, ISO 8601 dates, and file URLs. Through an `any` parameter, strings remain text.

When a reply contains `type` and `data`, pass the complete object unchanged to preserve native type through any parameter, including enum and class values inside records. `type` is a quoted four-character descriptor code; `data` holds Base64 native descriptor bytes. Optional `value` supplies a text projection; input ignores it. Preserve dates, file references, large integers, and specialized descriptors in that form.

When a dictionary name is ambiguous, use a code supplied by the dictionary or native result.

## [05]-[SCRIPT_LANGUAGES]

Select a script command by its dictionary description and set an explicit language when accepted. Match syntax, APIs, and execution context to ExtendScript, UXP JavaScript, or AppleScript. Command names alone do not identify runtimes: Illustrator's `do script` plays an Actions-panel action, while `do javascript` runs ExtendScript source or a script file.

`Command` passes arguments unchanged and adds no undo transaction. Group edits through native undo parameters or runtime history APIs. Use a linked `Script` form's guidance for its execution and undo rules. Within a script, retain native handles for dependent operations and return values or references needed by later calls.

## [06]-[RESULTS]

Use `reply` as the native result. If a script returns JSON text, parse it according to that script's contract. Native errors retain host status and message. Undelivered events did not run; for an unanswered mutation, inspect application state before repeating it.

List files written by a command in `artifacts` as file URLs. Attachments read those files after completion; inspect `unread` for failures while retaining `reply`. Artifact URLs do not create or export files. When starting asynchronous output, use the application's native completion mechanism before returning.

Use `screenshot` for dialogs and panels: it brings an application forward and captures its focused window or a returned window ID. PNG dimensions describe image pixels. Use document export or a linked render operation for page or object artwork.
