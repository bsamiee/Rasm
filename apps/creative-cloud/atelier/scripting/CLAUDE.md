# [ATELIER_SCRIPTING]

`Atelier` drives installed Adobe applications through Apple Events built from each application's scripting dictionary at run time, captures their windows, and lists installed font faces.

- Installed dictionaries own commands, parameters, classes, properties, and enumerations, a copy, table, or generated type of them drifts
- New capabilities extend the code every application shares, with no per-application branch or file
- JavaScript runs through the command a dictionary declares for it (`do javascript`, `do script`)
- AppleScript's own `activate`, `get`, and `set` events join every dictionary's commands, and a command whose direct parameter takes a specifier fits an object specifier
- Apple Events alone reach an application, with no socket, broker, queue, or health check
- Requests and replies share one JSON form, a reply value feeds a later request unchanged
- `Atelier.app` is a background-only application bundle with identifier `app.rasm.atelier`, the subject macOS Automation and Screen Recording consent and `tccutil` address
- `mise.toml` `[env]` `ATELIER_PATH` names the executable inside the installed bundle
- `apps/creative-cloud/atelier/scripting.ts` spawns `ATELIER_PATH` and keeps it running, one JSON-RPC 2.0 request per stdin line and one reply per stdout line
- Methods are `applications`, `application`, `definition`, `execute`, `contentType`, `windows`, `capture`, and `faces`
- Lines run concurrently, a reply carries its request's `id`, a line with no readable `id` gets no reply, and closing stdin exits after every reply is written
- Each running application's parsed dictionary stays in memory until its process identifier changes
- Dictionary reads wait for a launching application to finish, and read a stopped application only from a dictionary file its bundle declares
- `contentType` answers a file's media type, and the caller reads the file's bytes
- Listed applications are the Adobe bundles in the application folders, or with `running` the bundles of running instances, each bundle once with its identifier, and `application` reads the one bundle at a url
- `windows` lists an application's on-screen windows front to back, with bounds in points, level (`normal`, `modal`, `other`), and a title when the window server names one
- `capture` runs `screencapture` as a child of `Atelier` under its Screen Recording grant, scales the image down to the longest side it is given, never up, and answers its pixel size
- `faces` lists each face CoreText finds among the available fonts once, with the path of its file
- When another binary is responsible for it, `Atelier` re-executes in place with responsibility disclaimed, TCC consent names it alone
