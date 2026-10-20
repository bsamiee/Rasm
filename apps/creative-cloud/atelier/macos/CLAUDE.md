# [ATELIER_MACOS]

`Atelier.app` drives installed Adobe applications by Apple Events from each scripting dictionary and observes their windows through Accessibility.

## [01]-[SCOPE]

- Adobe bundle selection and the protocol stay here, every macOS capability comes from `RasmMacOS`
- New capabilities extend the code every application shares, with no per-application branch or file
- Changes reach callers after `nx run Atelier:install` and `mise daemons restart atelier`, the daemon spawns the installed executable

## [02]-[PROTOCOL]

Stdin and stdout carry one JSON-RPC 2.0 message per line in the form Effect's `RpcSerialization.layerNdJsonRpc` reads:
- `Request` and the `Requests` group in `@rasm/automation` declare each method, a change edits both
- Calls run concurrently, a reply carries its call's `id`, and a line holding neither an `id` nor an `@effect/rpc/` control method gets no reply
- Each `@effect/rpc/Ping` gets `@effect/rpc/Pong`, and `@effect/rpc/Interrupt` cancels the call its `requestId` names
- Closing stdin cancels every call, and the process exits once every reply is written
- Failures reply an encoded Effect `Cause`, one `Fail` per refusal, a `Die` for an unreadable request or unencodable reply
- A refusal an `Rpc` error union declares encodes that schema's fields (`Unmatched`)
- Applications are addressed by bundle URL as `installations` lists them, each version and channel apart
- Element ids are CoreGraphics window numbers, the ids `press` and `screenshot` take

## [03]-[VALUES]

`AppleEventValue` crosses as plain JSON, with a `_tag` object for each Apple Event type JSON lacks:
- Text coerces to the enumerator, type, date, or file URL a parameter's dictionary type takes
- Text naming an ordinal (`first`, `middle`, `last`, `any`, `every`) is an `index` selector
- Four-character codes cross as the scripting definition writes them and pass wherever a command, term, or enumerator name does
- Commands resolve among those whose parameters take every argument, language commands first, and more than one match fails
- Reply values pass back unchanged as arguments
- `opaque` keeps each descriptor the dictionary does not model, a 64-bit integer no Double holds exactly among them

## [04]-[HOST]

- `Atelier.app` holds Automation, Screen Recording, and Accessibility grants, start disclaims responsibility from its spawning process
- Element reads that find the process untrusted open the Accessibility prompt
- Start sets a one-second Accessibility messaging timeout, an application silent past it reads `responding` false
- `screenshot` activates the application before capture and fails when macOS keeps another application in front
