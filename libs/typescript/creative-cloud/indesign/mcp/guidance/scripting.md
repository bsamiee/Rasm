# [SCRIPTING]

## [01]-[OBJECTS]

Keep native objects through dependent operations. Obtain documents, preferences, and collections from their native owners; use returned specifiers to recover objects across separate calls. Native assignments take objects and enumerators directly. A name inside a string remains text.

Preserve owner identity when names repeat across groups or documents. Compare native objects and enumerators with `equals`; JavaScript identity and matching names do not establish native equality. After text edits before a stored range, obtain fresh references from native results. Character indices belong to their text range, including cell and note text.

Load host modules through UXP `require`. Node modules and ExtendScript's `File` constructor belong to different runtimes. Await promised storage getters before reading their properties; document DOM operations and file entries have different lifetimes.

## [02]-[COLLECTIONS]

Choose native access by the relationship needed:

| [INDEX] | [NEED]                    | [NATIVE_ACCESS]                         |
| :-----: | :------------------------ | :-------------------------------------- |
|  [01]   | Stable identified object  | `itemByID` or returned object reference  |
|  [02]   | Named member in one owner | `itemByName` within that collection      |
|  [03]   | Positional member or span | `item` or native range method            |
|  [04]   | Resolved member array     | `everyItem().getElements()`              |
|  [05]   | One field across members  | Native collective field access          |

Use the collection's supported methods; collection brackets, spread syntax, and array methods do not substitute for native collection access. Resolve members before changing collection membership when later work must retain their identities. Index specifiers follow positions.

## [03]-[CREATION]

Create through the owning collection's native `add` signature. Required arguments establish relationships; `withProperties` initializes writable values. Preserve `undefined` positions when supplying a later optional argument. `Groups.add` takes existing page items, while `Hyperlinks.add` connects a native source and destination.

Obtain nonconstructible settings through their owner. Set child preferences on the returned child object. Use native methods when an operation manages relationships: moving, transforming, fitting, applying styles, grouping, or removing objects can differ from assigning visible fields.

Keep the object returned by operations that replace native content. Path operations, conversion, and duplication can return a different owner. Continue from that result instead of recovering an object by position or matching bounds.

## [04]-[EFFECTS]

Complete asynchronous I/O before document changes that must share one undo step. Keep an `app.doScript` editing function synchronous when grouping later changes. Document undo excludes file writes and script preferences; restore temporary preference changes with `try/finally` and release temporary storage entries through their native API.

Recompose only when subsequent work depends on line positions or pagination after edits. Request native completion for operations that return a background task or process before consuming their output. A returned destination path does not establish completion.

Correct a failed native argument or member at its owner. Preserve reported errors and unavailable values instead of replacing them with defaults. Read only data needed for the next decision; returning a broad preference record can invoke unrelated native getters.

## [05]-[INTERFACE]

Choose a native object member for document state and a menu action for an interface command. Invoke a discovered action through `app.menuActions.itemByID(id).invoke()`. Resolve native commands by their actual identifiers rather than translating visible labels into method names.

Use `Panel.visible` to show or hide panels. Panel visibility does not expose individual controls. When a control has no native member, operate the application interface. An open dialog can hold a native call until the dialog closes.

Distinguish composed artwork from application state when inspecting a result. Native page or item export shows document content; an application window shows panels, dialogs, and controls.
