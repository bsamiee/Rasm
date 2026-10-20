# [ATELIER_MACOS]

`Atelier.app` drives installed Adobe applications through Apple Events built from each scripting dictionary at run time, and observes their windows through Accessibility.

## [01]-[SCOPE]

- Adobe bundle selection and the protocol stay here, dictionary modeling, Apple Event encoding, and every other macOS capability come from `RasmMacOS`
- New capabilities extend the code every application shares, with no per-application branch or file
- Installed dictionaries, XInclude expanded, supply every term except AppleScript's own `activate`, `get`, and `set`
- Each application's parsed dictionary stays held until its process identifier changes
- Changes reach callers after `nx run Atelier:install` and `mise daemons restart atelier`, the daemon spawns the installed executable

## [02]-[PROTOCOL]

Stdin and stdout carry one JSON-RPC 2.0 message per line in the form Effect's `RpcSerialization.layerNdJsonRpc` reads:
- `Request` and the `Requests` group in `@rasm/automation` declare each method, a change edits both
- Calls run concurrently, a reply carries its call's `id`, and a line with no readable `id` other than a ping gets no reply
- Each `@effect/rpc/Ping` gets `@effect/rpc/Pong`, and `@effect/rpc/Interrupt` ends the stream its `requestId` names
- Closing stdin ends every stream, and the process exits once every reply is written
- Failures reply an encoded Effect `Cause`, one `Fail` per refusal, a `Die` for an unreadable request or unencodable reply
- Failure `name` is the innermost failure case, a refusal an `Rpc` error union declares encodes that schema's fields (`Unmatched`)
- Applications are addressed by bundle URL as `installations` lists them, each version and channel apart
- Element ids are CoreGraphics window numbers, the ids `press` and `screenshot` take

## [03]-[VALUES]

`Native` encodes values as plain JSON, with a `_tag` object for each Apple Event type JSON lacks:
- Text coerces to the enumerator, type, date, or file URL a parameter's dictionary type takes, and to an ordinal as an `index` selector
- Four-character codes cross as the scripting definition writes them and pass wherever a command, term, or enumerator name does
- Commands resolve among those whose parameters take every argument, language commands first, and more than one match fails
- Reply values pass back unchanged as arguments, `opaque` keeps a descriptor the dictionary does not model

## [04]-[HOST]

- `Atelier.app` holds Automation, Screen Recording, and Accessibility grants, start disclaims responsibility from its spawning process
- Element reads that find the process untrusted open the Accessibility prompt
- Start sets a one-second Accessibility messaging timeout, an application silent past it reads `responding` false
- `screenshot` activates the application before capture and fails when macOS keeps another application in front
