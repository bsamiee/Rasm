# [ATELIER_SCRIPTING]

`Atelier` drives installed Adobe applications through Apple Events built from each scripting dictionary at run time.

- Installed dictionaries with XInclude expanded own commands, parameters, classes, properties, and enumerations
- New capabilities extend the code every application shares, with no per-application branch or file
- Commands and language parameters select their native runtime; `do script` can name an action rather than JavaScript
- AppleScript's own `activate`, `get`, and `set` events join every dictionary's commands
- Commands with a specifier direct parameter take an object specifier
- Scripting commands reach applications through native Apple Events
- Native launch targets the selected bundle URL without substitution; its launch event preserves an already-running application's windows
- Requests reuse full replies; opaque `type` and `data` preserve native identity, optional `value` supplies display text
- `Atelier.app`, identifier `app.rasm.atelier`, owns Automation and Screen Recording consent
- `mise.toml` `atelier` daemon row `env` `ATELIER_PATH` names the executable inside the installed bundle
- `apps/creative-cloud/atelier/scripting.ts` keeps `ATELIER_PATH` running, one JSON-RPC 2.0 request per stdin line, one reply per stdout line
- `Request` in `Scripting.swift` decodes each method, the `RpcGroup` in `scripting.ts` types each call
- Refusals reply with a JSON-RPC error carrying `_tag` `ScriptingError` and its `failures`, the form `ScriptingError` decodes
- Native changes reach callers after `nx run Atelier:install` and `pitchfork restart rasm/atelier`
- Lines run concurrently, a reply carries its request's `id`, a line with no readable `id` gets no reply
- Closing stdin exits after every reply is written
- Each running application's parsed dictionary stays in memory until its process identifier changes
- Dictionary reads wait for a launching application to finish, and read a stopped application only from a dictionary file its bundle declares
- `contentType` answers a file's media type, `application/octet-stream` for a type with none, and the caller reads the file's bytes
- `applications` lists each Adobe bundle in the application folders once with its identifier, with `running` the bundles of running instances
- `application` reads the one bundle at a url
- `screenshot` returns the frontmost capturable or requested window image and available windows in one call
- `RasmMacOS` activates through native state observation and joins front-to-back CoreGraphics window IDs to ScreenCaptureKit
- Atelier selects the Adobe application and encodes native screenshot output as PNG
- Captures include child windows at native pixel size, base64 in the reply, with no crop or temporary file
- `faces` lists each face CoreText finds among the available fonts once, with the path of its file
- When another binary is responsible for it, `Atelier` re-executes in place with responsibility disclaimed, TCC consent names it alone
