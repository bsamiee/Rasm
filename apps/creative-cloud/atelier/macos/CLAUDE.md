# [ATELIER_MACOS]

`Atelier.app` drives installed Adobe applications by Apple Events from each scripting dictionary and observes their windows through Accessibility.

## [01]-[SCOPE]

- Atelier holds Adobe bundle selection, its request methods, and the tool each refused interference routes to, `AdobeScripting` holds the refusal rows and script commands, `AppleEvents` matches the rows, and `EffectRPC` serves the wire
- Capabilities extend code every Adobe application shares, with no per-application branch or file
- Calls raise no application and open no dialog or panel: launches run in the background, Apple Events send with never-interact
- Never-interact is advisory, `execute` refuses a dictionary command a refusal row marks before it is sent
- Refusal rows hold host facts keyed by bundle identifier stem, the resolved event code in any spelling, and a parameter or property value, a stem covers every channel and version
- InDesign `show` (`UIUIshow`) is the one row proven live, every other row rests on its dictionary text
- Window ordering inside an application (`bring to front`, `make` a document or window) passes, InDesign keeps it in its frame window
- `execute` refuses a script command toward `execute_script` and a menu-action command toward the Menu form or `execute_script`, as `Adobe.routes` names them
- `script` sends source as direct parameter of the script command `AdobeScripting` names for the installation and replies with its runtime
- InDesign names no script command: its `do script` runs at INTERACT_WITH_ALL whatever the send mode, and its scripts run through the linked extension

## [02]-[PROTOCOL]

Stdin and stdout carry one JSON-RPC 2.0 message per line in the form Effect's `RpcSerialization.layerNdJsonRpc` reads:
- `Requests` in `@rasm/automation` declares each method and schema, a method change edits it and `Request` together
- Lines end at `\n` alone, U+2028, U+2029, and U+0085 inside a request stay in it
- Calls run concurrently and replies carry their call's `id`
- `@effect/rpc/Interrupt` cancels the call its `requestId` names
- `@effect/rpc/Ack` is read and ignored, no ping or pong crosses the wire
- Lines other than an interrupt or a call with a readable `id` get no reply
- Closing stdin ends the process and every call
- Failures reply an encoded Effect `Cause` in `error.data`, one `Fail` per refusal, a `Die` for an unreadable request or unencodable reply
- Refusals an `Rpc` error union declares encode that schema's fields
- `installations` chunks are `Result` values, a discovery failure fills one chunk and the next listing follows it
- `instances` lists every running instance of a scriptable Adobe installation with its state, at subscription and after each change of one
- Instance calls address a running process by `pid`, bundle calls address an installation by `url`
- Instance calls refuse a `pid` that no running Adobe application bundle holds
- Targets silent past a call's timeout answer `unanswered`

## [03]-[VALUES]

`AppleEventValue` crosses as the plain JSON its `@rasm/automation` schema reads, with an Effect tagged struct for each Apple Event type JSON lacks:
- Text coerces to the enumerator, type, date, or file URL a parameter's dictionary type takes, and to an ordinal as an `index` selector
- Codes as the scripting definition writes them pass wherever a command, term, or enumerator name does
- Codes holding a control byte cross in the `0x` hexadecimal form sdef(5) states, commands as 16 hex digits
- Reply values pass back unchanged as arguments

## [04]-[HOST]

- Atelier disclaims responsibility from its spawning process, TCC grants (Automation, Screen Recording, Accessibility) attach to `Atelier.app`
