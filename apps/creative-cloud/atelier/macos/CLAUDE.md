# [ATELIER_MACOS]

`Atelier.app` drives installed Adobe applications through Apple Events built from each scripting dictionary at run time, and observes their windows, dialogs, sheets, and menus through Accessibility.

- `RasmMacOS` owns the universal macOS capabilities: application discovery, launch without substitution, readiness, Accessibility observation, element description, button presses, responsiveness, activation, and window capture
- Atelier selects Adobe bundles (identifier prefix `com.adobe.`), reads their dictionaries, encodes Apple Events, and composes the library's capabilities per application
- Installed dictionaries with XInclude expanded own commands, parameters, classes, properties, and enumerations
- New capabilities extend the code every application shares, with no per-application branch or file
- Commands and language parameters select their native runtime; `do script` can name an action rather than JavaScript
- AppleScript's own `activate`, `get`, and `set` events join every dictionary's commands
- Commands with a specifier direct parameter take an object specifier
- `launch` opens the selected bundle URL without substitution and answers once the instance has finished launching; its launch event preserves an already-running application's windows
- Requests reuse full replies; opaque `type` and `data` preserve native identity, optional `value` supplies display text
- `Atelier.app`, identifier `app.rasm.atelier`, owns Automation, Screen Recording, and Accessibility consent; the Accessibility prompt appears at the first `status`, `watch`, or `press`
- `mise.toml` `atelier` daemon row `env` `ATELIER_PATH` names the executable inside the installed bundle
- `@rasm/automation` keeps `ATELIER_PATH` running, one JSON-RPC 2.0 request per stdin line, replies and stream chunks one per stdout line, in the wire form Effect's `RpcSerialization.layerNdJsonRpc` reads
- `Request` in `Scripting.swift` decodes each method, the `RpcGroup` in `helper.ts` types each call
- Refusals reply `error: {code: 1, message: "Refused", _tag: "AutomationError", reason: {_tag: "Refused", failures: [...]}}`
- `watch` streams `{chunk: true, id, result: [Status]}` lines on every element or focus change and ends with `{jsonrpc, id}` once the client sends `@effect/rpc/Interrupt` for its id; `@effect/rpc/Ack` lines are read and dropped
- Native changes reach callers after `nx run Atelier:install` and `pitchfork restart rasm/atelier`
- Lines run concurrently, a reply carries its request's `id`, a line with no readable `id` gets no reply
- Closing stdin exits after every reply is written
- Each running application's parsed dictionary stays in memory until its process identifier changes
- Dictionary reads wait for a launching application to finish, and read a stopped application only from a dictionary file its bundle declares
- `terms` takes a declaration kind and answers each declaration's `name` and `code` as the dictionary states them
- `contentType` answers a file's media type, `application/octet-stream` for a type with none, and the caller reads the file's bytes
- `applications` lists each Adobe bundle in the application folders once, every version and channel, with `running` the instance's pid and readiness and `channel` the bundle's `RELEASECHANNEL` or the parenthesized word of its `CFBundleName`
- `status` answers `responding`, `frontWindow`, and the open elements with id, kind, role, title, static text, and button labels; an application answering no Accessibility request within one second reads `responding` false
- Element ids are CoreGraphics window numbers, the ids `screenshot` takes
- `press` presses a button by label, `AXCloseButton` for a window's close button, `AXCancel` for a menu, and answers the status after the press
- `screenshot` returns the frontmost capturable or requested window image and available windows in one call
- `RasmMacOS` activates through native state observation and joins front-to-back CoreGraphics window IDs to ScreenCaptureKit
- Captures include child windows at native pixel size, base64 in the reply, with no crop or temporary file
- `faces` lists each face CoreText finds among the available fonts once, with the path of its file
- When another binary is responsible for it, `Atelier` re-executes in place with responsibility disclaimed, TCC consent names it alone
