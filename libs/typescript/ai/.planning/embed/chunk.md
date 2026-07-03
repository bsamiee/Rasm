# [AI_CHUNK]

Chunking is a policy row, not a loop: one `Chunker.cut` entry normalizes source text once and dispatches on the policy's lane — `fixed` sliding windows, `sentence` boundary packing, `markdown` structure-aware splitting — through one handler record, emitting `Piece` values that carry sequence, body, character span, and a token estimate. Every decision that shapes an embedding corpus lives on the policy value: window, overlap, lane, locale. Normalization runs before any cut so equal source yields byte-equal pieces — the stability that makes the embedder's input cache and the store's fingerprint keys deterministic — and spans index the normalized text so every piece is re-locatable evidence, never a floating string.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                              |
| :-----: | :---------- | :------------------------------------------------------------------- |
|  [01]   | [NORMALIZE] | the one scrub fold — unicode form, control strip, whitespace collapse |
|  [02]   | [CUT]       | the lane record, the `Piece` receipt, and the one `cut` entry         |

## [2]-[NORMALIZE]

[NORMALIZE]:
- Owner: the interior `_scrub` fold — NFC unicode normalization, control-character strip, horizontal-whitespace collapse, blank-run flattening, and edge trim, applied once to the whole source before any lane runs.
- Law: normalization is the determinism anchor — the embedder's built-in cache keys on the input string and the store's vector-row fingerprints key on chunk content, so byte-stable pieces are what make re-embedding idempotent; a lane cutting un-normalized text would mint distinct cache keys for visually identical sources.
- Law: spans index the NORMALIZED text — `_scrub` output is the coordinate system every `Piece.start`/`Piece.end` addresses; a consumer that must map back to raw source re-runs the scrub, because the raw-offset mapping is deliberately not carried.
- Law: scrubbing is lossy only below semantics — control characters and whitespace runs carry no embedding signal; case, punctuation, and unicode content survive, so the scrub never erases retrievable meaning.
- Growth: a new normalization step is one line in the fold, and every lane inherits it at once.
- Packages: `effect` (none beyond the platform string surface — the fold is pure).

## [3]-[CUT]

[CUT]:
- Owner: `Chunker` — the policy shape, the lane handler record, and the one entry. `Chunker.Policy` is `{ lane, window, overlap, locale }` with `window`/`overlap` in estimated tokens; `_lanes` is the mapped handler record dispatched by one indexed call; `Chunker.cut(text, policy)` scrubs, cuts, filters empties, and mints `Piece` rows.
- Law: the estimator is a sizing heuristic, stated — four characters per token calibrates window arithmetic and the `estimate` field; exact counting is `model/token`'s metered read, and a consumer needing exactness gauges pieces through `Budget.gauge` after cutting, never by trusting `estimate` past sizing tolerance.
- Law: `fixed` is the only overlapping lane — windows slide by `window - overlap` with a floor of one character, so pathological policies degrade to dense coverage instead of an infinite unfold; boundary lanes (`sentence`, `markdown`) carry no overlap because their boundaries are semantic, and duplicating semantic units would double-count retrieval evidence.
- Law: `sentence` packs `Intl.Segmenter` sentence segments greedily up to the window — a segment never splits mid-sentence, and the segmenter's locale is the policy's; the segmenter construction is the platform seam this lane names.
- Law: `markdown` splits at heading marks that fall OUTSIDE code-fence spans — fence spans pair off fence marks in one fold, guarded heads bound the blocks, and an oversize block re-packs through the `fixed` lane with its offsets shifted into place, so structure wins where it exists and coverage wins where it does not.
- Law: `Piece` is the folder's chunk receipt — `seq` orders the corpus, `start`/`end` locate it, `estimate` sizes it, and the class's derived surfaces (arbitrary, equivalence) come free for the test estate; construction happens only inside `cut` over already-proven bodies.
- Boundary: embedding the pieces is `embed/embedder`'s; fingerprint keys per vector row are the store's vector-row law — this page emits re-locatable normalized content, the store keys it.
- Entry: `Chunker.cut(text, policy)`.
- Receipt: `ReadonlyArray<Piece>`.
- Growth: a new lane is one `_lanes` row plus its `Lane` literal — the policy type, the dispatch, and the admission all read one anchor.
- Packages: `@effect/ai` (none — pure), `effect` (`Array`, `Iterable`, `Option`, `Schema`).

```typescript
import { Array, Iterable, Option, Schema } from "effect"

const _GAUGE = 4

const _scrub = (text: string): string =>
  text
    .normalize("NFC")
    .replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]/g, "")
    .replace(/[ \t]+/g, " ")
    .replace(/\n{3,}/g, "\n\n")
    .trim()

const _gauge = (body: string): number => Math.ceil(body.length / _GAUGE)

class Piece extends Schema.Class<Piece>("Piece")({
  seq: Schema.Int.pipe(Schema.nonNegative()),
  body: Schema.NonEmptyString,
  start: Schema.Int.pipe(Schema.nonNegative()),
  end: Schema.Int.pipe(Schema.positive()),
  estimate: Schema.Int.pipe(Schema.positive()),
}) {}

declare namespace Chunker {
  type Lane = keyof typeof _lanes
  type Policy = {
    readonly lane: Lane
    readonly window: number
    readonly overlap: number
    readonly locale: string
  }
  type Cut = { readonly body: string; readonly start: number }
  type Shape = {
    readonly Piece: typeof Piece
    readonly cut: (text: string, policy: Policy) => ReadonlyArray<Piece>
  }
}

const _fixed = (clean: string, policy: Chunker.Policy): ReadonlyArray<Chunker.Cut> => {
  const chars = policy.window * _GAUGE
  const step = Math.max(1, chars - policy.overlap * _GAUGE)
  return Array.fromIterable(
    Iterable.unfold(0, (off) =>
      off < clean.length
        ? Option.some([{ body: clean.slice(off, off + chars), start: off }, off + step])
        : Option.none()),
  )
}

const _sentence = (clean: string, policy: Chunker.Policy): ReadonlyArray<Chunker.Cut> => {
  const chars = policy.window * _GAUGE
  const segments = Array.fromIterable(new Intl.Segmenter(policy.locale, { granularity: "sentence" }).segment(clean))
  const packed = Array.reduce(
    segments,
    { body: "", start: 0, rows: Array.empty<Chunker.Cut>() },
    (acc, seg) =>
      acc.body.length === 0
        ? { body: seg.segment, start: seg.index, rows: acc.rows }
        : acc.body.length + seg.segment.length <= chars
          ? { body: acc.body + seg.segment, start: acc.start, rows: acc.rows }
          : { body: seg.segment, start: seg.index, rows: Array.append(acc.rows, { body: acc.body, start: acc.start }) },
  )
  return packed.body.length === 0 ? packed.rows : Array.append(packed.rows, { body: packed.body, start: packed.start })
}

const _markdown = (clean: string, policy: Chunker.Policy): ReadonlyArray<Chunker.Cut> => {
  const chars = policy.window * _GAUGE
  const fences = Array.reduce(
    Array.fromIterable(clean.matchAll(/^```/gm)),
    { open: Option.none<number>(), rows: Array.empty<readonly [number, number]>() },
    (acc, mark) =>
      Option.match(acc.open, {
        onNone: () => ({ open: Option.some(mark.index), rows: acc.rows }),
        onSome: (start) => ({ open: Option.none<number>(), rows: Array.append(acc.rows, [start, mark.index] as const) }),
      }),
  )
  const heads = Array.filterMap(Array.fromIterable(clean.matchAll(/^#{1,6}\s/gm)), (mark) =>
    Array.some(fences.rows, ([a, b]) => mark.index > a && mark.index < b) ? Option.none() : Option.some(mark.index))
  const bounds = Array.dedupe([0, ...heads, clean.length])
  const blocks = Array.zipWith(bounds, Array.drop(bounds, 1), (a, b) => ({ body: clean.slice(a, b), start: a }))
  return Array.flatMap(blocks, (block) =>
    block.body.length <= chars
      ? [block]
      : Array.map(_fixed(block.body, { ...policy, overlap: 0 }), (piece) => ({ body: piece.body, start: block.start + piece.start })))
}

const _lanes: { readonly [K in "fixed" | "markdown" | "sentence"]: (clean: string, policy: Chunker.Policy) => ReadonlyArray<Chunker.Cut> } = {
  fixed: _fixed,
  markdown: _markdown,
  sentence: _sentence,
}

const Chunker: Chunker.Shape = {
  Piece,
  cut: (text, policy) => {
    const clean = _scrub(text)
    return Array.map(
      Array.filter(_lanes[policy.lane](clean, policy), (row) => row.body.trim().length > 0),
      (row, seq) =>
        new Piece({
          seq,
          body: row.body,
          start: row.start,
          end: row.start + row.body.length,
          estimate: _gauge(row.body),
        }),
    )
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Chunker, Piece }
```
