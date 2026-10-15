# [ATELIER_SCRIPTING]

`Atelier` drives installed Adobe applications through Apple Events built from each scripting dictionary at run time.

- Installed dictionaries own commands, parameters, classes, properties, and enumerations, a copy, table, or generated type of them drifts
- New capabilities extend the code every application shares, with no per-application branch or file
- JavaScript runs through the command a dictionary declares for it (`do javascript`, `do script`)
- AppleScript's own `activate`, `get`, and `set` events join every dictionary's commands
- Commands with a specifier direct parameter take an object specifier
- Apple Events alone reach an application, with no socket, broker, queue, or health check
- Requests and replies share one JSON form, a reply value feeds a later request unchanged
- `Atelier.app` is a background-only bundle with identifier `app.rasm.atelier`, the subject of Automation and Screen Recording consent and `tccutil`
- `mise.toml` `[env]` `ATELIER_PATH` names the executable inside the installed bundle
- `apps/creative-cloud/atelier/scripting.ts` keeps `ATELIER_PATH` running, one JSON-RPC 2.0 request per stdin line, one reply per stdout line
- `Request` in `Scripting.swift` decodes each method, the `RpcGroup` in `scripting.ts` types each call
- A refusal replies with a JSON-RPC error carrying `_tag` `ScriptingError` and a `Refused` `reason`, the form `ScriptingError` decodes
- Lines run concurrently, a reply carries its request's `id`, a line with no readable `id` gets no reply
- Closing stdin exits after every reply is written
- Each running application's parsed dictionary stays in memory until its process identifier changes
- Dictionary reads wait for a launching application to finish, and read a stopped application only from a dictionary file its bundle declares
- `contentType` answers a file's media type, and the caller reads the file's bytes
- `applications` lists each Adobe bundle in the application folders once with its identifier, with `running` the bundles of running instances
- `application` reads the one bundle at a url
- `windows` lists on-screen windows front to back with bounds in points, level (`normal`, `modal`, `other`), and the title a window server names
- `capture` runs `screencapture` as a child of `Atelier` under its Screen Recording grant and answers the image's pixel size
- Captures scale down to the `longest` side a call names, never up
- `faces` lists each face CoreText finds among the available fonts once, with the path of its file
- When another binary is responsible for it, `Atelier` re-executes in place with responsibility disclaimed, TCC consent names it alone
