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
- `@effect/rpc/Interrupt` cancels the call its `requestId` names, an interrupted launch stops waiting
- `@effect/rpc/Ack` is read and ignored, the client sends no ping and gets no pong
- Lines other than an interrupt or a call with a readable `id` get no reply
- Closing stdin ends the process and every call with it
- Failures reply an encoded Effect `Cause` in `error.data`, one `Fail` per refusal, a `Die` for an unreadable request or unencodable reply
- Refusals an `Rpc` error union declares encode that schema's fields (`Unmatched`)
- `installations` streams the full row list once Spotlight finishes gathering, then again after each Spotlight update or running-application change that alters a row
- `installations` lists what Spotlight indexes in the application folders, a bundle outside its index is absent
- `statuses` streams `{pid, status}` for every Adobe instance that creates windows: its status when observation starts and after each change of its windows, dialogs, sheets, or menus, with an instance's refusal riding inside its status event as a `Failure` result
- Neither stream fails on one instance, a listing failure ends `installations` with its `Cause`
- Applications are addressed by `url` as `installations` lists them, each version and channel apart, and a call resolves its `url` from the bundle and running processes with no Spotlight query
- `activate`, `status`, `press`, `execute`, and `screenshot` address a running instance by an optional `pid`, and refuse a bundle running several instances without one as `Unmatched` listing the pids
- `activate`, `execute`, and `screenshot` launch a bundle with no running instance when no `pid` is given
- `launch` replies the `pid` of the instance once it finished launching
- `press` performs its actions in order and stops at the first refusal
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
- Start opens the Accessibility prompt when the process is untrusted and sets a one-second Accessibility messaging timeout, an application silent past it reads `unresponsive`
- `activate` and `screenshot` bring the application forward through a Launch Services open request, which macOS honors from a background process where an activation request is declined, and wait until it is active
- ScreenCaptureKit returns a window or `application` capture as one `sdrImage` for every range, and fills `hdrImage` for a whole-display capture alone, which no target takes
- `hdr` images hold 16-bit extended sRGB values above 1.0, which `public.tiff` keeps and `public.png`, `public.heic`, and `public.jpeg` clip
