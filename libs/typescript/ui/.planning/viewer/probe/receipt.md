# [UI_RECEIPT]

`viewer/probe/receipt.ts` produces and displays render evidence: the wire-owned `Envelope.Render` receipt (AU:60 — view id, `ContentKey`, matched flag, stamp) is the proof value this page renders, and its local half is the capture fold — read the settled framebuffer through the renderer's async readback, delegate the hash to the ONE kernel `ContentKey` mint (never a local hash), and compare structurally. A failed match is evidence rendered to the operator — which viewport, which key, when — never a fault on any channel; determinism of the captured frame (fixed size, settled scene, post-compile) is this page's capture discipline.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            |
| :-----: | :---------------- | :------------------------------------------------------------------ |
|   [1]   | `CAPTURE_FOLD`    | the deterministic framebuffer capture and the kernel hash delegate   |
|   [2]   | `EVIDENCE_RENDER` | the receipt display rows — match verdicts as operator evidence       |

## [2]-[CAPTURE_FOLD]

- Owner: `RenderProbe.capture` — the capture discipline as one fold: render into a fixed-extent target (capture never reads the live swap chain — DPR and resize would break determinism), await the settled frame (`compileAsync` before first capture; capture runs after the residency fold quiesces), read pixels through `renderer.readRenderTargetPixelsAsync(target, 0, 0, width, height, buffer)` (the WebGPU-safe async readback; a synchronous read stalls the pipeline and is the named defect), and delegate the octets to the kernel mint — `ContentKey` is minted in exactly one place in the branch, and this fold calls it, never re-implements it (`[R2]` gates the mint going load-bearing; this page inherits the gate transitively and carries no hash code to rewrite either way).
- Packages: `three` (`WebGLRenderTarget`-family targets, the async readback — members verified against the shipped runtime), `@rasm/ts/kernel` (`ContentKey` — the one mint), `@rasm/ts/wire/vocab` (`Envelope` — the receipt type derives as `Schema.Schema.Type<typeof Envelope.render>` off the `#vocab` schema value, never a parallel shape), `effect`.
- Law: capture parameters are a policy row — extent, target format, and the settle predicate live in one `as const` record; a capture with ad-hoc parameters produces an incomparable hash and is the named defect.
- Law: the comparison is structural — the local key and the receipt's key compare through `Equal.equals` on the brand; the verdict is `{ view, expected, actual, matched, at }` — a plain data row.
- Boundary: the wire receipt's decode is `wire/codec/envelope`'s; the renderer and scene are `viewer/scene/glb`'s (the capture fold takes them as parameters); the MRT/post chain that would feed a G-buffer capture is the same fold with a different target row.

```typescript
import type { ContentKey } from "@rasm/ts/kernel"
import type { Envelope } from "@rasm/ts/wire/vocab"
import { DateTime, Effect, Equal, type Schema } from "effect"

const _CAPTURE = { width: 1024, height: 1024 } as const

type _Render = Schema.Schema.Type<typeof Envelope.render>

declare namespace RenderProbe {
  type Readback = (width: number, height: number) => Effect.Effect<Uint8Array>
  type Verdict = {
    readonly view: string
    readonly expected: ContentKey
    readonly actual: ContentKey
    readonly matched: boolean
    readonly at: DateTime.Utc
  }
}

const _capture = (
  view: string,
  readback: RenderProbe.Readback,
  mint: (octets: Uint8Array) => Effect.Effect<ContentKey>,
  receipt: _Render,
): Effect.Effect<RenderProbe.Verdict> =>
  Effect.gen(function* () {
    const pixels = yield* readback(_CAPTURE.width, _CAPTURE.height)
    const actual = yield* mint(pixels)
    const at = yield* DateTime.now
    return {
      view,
      expected: receipt.key,
      actual,
      matched: Equal.equals(actual, receipt.key),
      at,
    }
  })
```

## [3]-[EVIDENCE_RENDER]

- Owner: `RenderProbe.tone` — the verdict presentation vocabulary: matched renders on the success tone, mismatched on the danger tone WITH both keys shown (the `:x32` spelling the kernel brand carries), and the wire receipt's own `matched`/`at` fields render beside the local verdict so the operator sees both proofs; stamps format through `intl/format`.
- Law: a mismatch is never a fault — no channel carries it, no retry fires; the verdict row IS the deliverable, and escalation is an operator decision outside this plane.
- Law: verdict history is a bounded fold — the last N verdicts per view ride a `Chunk`-backed atom (append, take-right), so evidence accumulates without unbounded memory; N is a policy row.
- Boundary: verdict transport to any journal is app egress; the badge/row primitives are `view/primitive` recipes.

```typescript
const _tone = {
  matched: { tone: "success" },
  mismatched: { tone: "danger" },
} as const

const RenderProbe: {
  readonly capture: typeof _capture
  readonly extent: typeof _CAPTURE
  readonly tone: typeof _tone
} = {
  capture: _capture,
  extent: _CAPTURE,
  tone: _tone,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { RenderProbe }
```
