# [ATELIER_MACOS]

`Atelier.app` drives installed Adobe applications by Apple Events from each scripting dictionary and observes their windows through Accessibility.

## [01]-[SCOPE]

- Adobe bundle selection and the protocol stay here, every macOS capability comes from `RasmMacOS`
- New capabilities extend the code every application shares, with no per-application branch or file
- Changes reach callers after `nx run Atelier:install` and `mise daemons restart atelier`

## [02]-[PROTOCOL]

Stdin and stdout carry one JSON-RPC 2.0 message per line in the form Effect's `RpcSerialization.layerNdJsonRpc` reads:
- `Request` and the `Requests` group in `@rasm/automation` declare each method, a change edits both
- Calls run concurrently and replies carry their call's `id`
- Each `@effect/rpc/Ping` gets `@effect/rpc/Pong`, and `@effect/rpc/Interrupt` cancels the call its `requestId` names
- Lines other than a ping or a call with a readable `id` get no reply
- Closing stdin cancels every call, and the process exits once every reply is written
- Failures reply an encoded Effect `Cause`, one `Fail` per refusal, a `Die` for an unreadable request or unencodable reply
- Refusals an `Rpc` error union declares encode that schema's fields (`Unmatched`)
- Applications are addressed by bundle URL as `installations` lists them, each version and channel apart
- `status`, `watch`, `press`, `execute`, and `screenshot` address a running instance by an optional `pid`, and refuse a bundle running several instances without one as `Unmatched` listing the pids
- `execute` and `screenshot` launch a bundle with no running instance when no `pid` is given
- Element ids are CoreGraphics window numbers, the ids `press` and `screenshot` take

## [03]-[VALUES]

`AppleEventValue` crosses as the plain JSON its `@rasm/automation` schema reads, with a `_tag` object for each Apple Event type JSON lacks:
- Text coerces to the enumerator, type, date, or file URL a parameter's dictionary type takes, and to an ordinal as an `index` selector
- Codes cross as the scripting definition writes them and pass wherever a command, term, or enumerator name does
- Commands resolve among those taking every argument as a parameter, AppleScript commands first, and more than one match fails
- Records cross as JSON objects, a record of another descriptor type (range, comparison, logical) as a `record` tag holding that type
- Reply values pass back unchanged as arguments, and `opaque` keeps each non-record descriptor no other tag or JSON value holds exactly

## [04]-[HOST]

- `Atelier.app` holds Automation, Screen Recording, and Accessibility grants once start disclaims responsibility from its spawning process
- Element reads that find the process untrusted open the Accessibility prompt
- Start sets a one-second Accessibility messaging timeout, an application silent past it reads `unresponsive`
- `screenshot` activates the application before capture and fails when macOS keeps another application in front
- ScreenCaptureKit returns a window or `application` capture as one `sdrImage` for every range, and fills `hdrImage` for a whole-display capture alone, which no target takes
- `hdr` images hold 16-bit extended sRGB values above 1.0, which `public.tiff` keeps and `public.png`, `public.heic`, and `public.jpeg` clip
