# [STORE_UPCAST]

Schema evolution without migrations: every persisted payload carries the `eventVersion` its author stamped, and reads lift it to the current shape through a total per-tag step chain — version `n` to `n+1`, array-indexed so completeness is a construction fact — before one `Schema.decodeUnknown` through the live family proves the landing. The raw log is never rewritten; a new event shape is one step appended to its tag's chain plus the bumped `latest`, and every journal read, projection lane, and snapshot load inherits the lift through the one plan value. Totality is proven by the `tests/typescript/_testkit` law combinators — every historical version composed through its chain must decode — and a chain whose step roster disagrees with `latest` dies loudly at plan construction, never silently at read.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                       |
| :-----: | :---------------- | :----------------------------------------------------------------------------- |
|  [01]   | `CHAIN_VOCABULARY`| the raw envelope shape, the step chain, the construction-checked completeness   |
|  [02]   | `PLAN_FOLD`       | `Upcast.plan` for tagged families, `Upcast.chain` for single shapes, the decode |

## [2]-[CHAIN_VOCABULARY]

- Owner: `Upcast.Raw` — the `{tag, version, payload}` envelope every persisted row projects into before lifting; `Upcast.Chain` — `{latest, steps}` where `steps[i]` lifts version `i + 1` to `i + 2` over the encoded shape.
- Packages: `effect` (types only at this altitude).
- Growth: a new version of one event is one step pushed onto its chain and `latest` bumped by one — old steps never change, because the versions they lift are already in the log.
- Law: steps are total pure functions over encoded payloads — `(payload: unknown) => unknown` with no failure channel; partiality has nowhere to hide because the terminal decode re-proves every invariant the current schema states.
- Law: completeness is positional — a chain of `latest: 4` carries exactly three steps; `_sized` enforces `steps.length === latest - 1` at plan construction and a mismatch is a defect (`Effect.die`-grade, surfaced synchronously), never a read-time fault.
- Law: the step transforms the whole encoded member including its `_tag` — a rename across versions is a step that rewrites the tag, and the plan indexes chains by the tag AS WRITTEN, so renamed families keep their history reachable.
- Boundary: where `Raw` comes from — the journal row projection — is `journal/append.md`'s; where the current family is declared is app material arriving as a `Schema.Union` value.

```typescript
declare namespace Upcast {
  type Raw = {
    readonly tag: string
    readonly version: number
    readonly payload: unknown
  }
  type Step = (payload: unknown) => unknown
  type Chain = {
    readonly latest: number
    readonly steps: ReadonlyArray<Step>
  }
  type Roster = { readonly [tag: string]: Chain }
}

const _sized = (tag: string, chain: Upcast.Chain): Upcast.Chain => {
  if (chain.steps.length !== chain.latest - 1) {
    throw new Error(`<upcast-chain-incomplete:${tag}:${chain.steps.length}/${chain.latest - 1}>`)
  }
  return chain
}
```

## [3]-[PLAN_FOLD]

- Owner: `Upcast` — `plan(family, roster)` binds a tagged event family to its chains; `chain(shape, spec)` is the single-shape twin the snapshot store keys by `snapshot_schema_version`; both return decode folds that lift then prove.
- Packages: `effect` (`Effect`, `Array`, `Option`, `Record`, `Schema`, `ParseResult`).
- Entry: `plan(...).decode(raw)` is the ONLY road from a persisted payload to a live event value — `journal/append.md`'s read stream, `project/*` lanes, and `journal/retain.md`'s DSAR fold all compose it; `plan(...).latest(tag)` is what the append surface stamps on writes, so write-version and read-lift share one anchor and cannot drift.
- Law: `Upcast.body` is the one JSON-column projection — pg returns jsonb columns as live objects while the sqlite lanes return TEXT, so the string-parse ternary exists in exactly one spelling and every payload, snapshot-body, and state-cell read composes it; a malformed stored text is a defect at the projection, because the column was written by `Schema.encode` and cannot lawfully hold non-JSON.
- Receipt: the decode lands in the family type or fails as `ParseError` on the one admission rail — a lifted payload that fails the current schema is exactly a malformed-history finding, routed to quarantine by the consuming lane, never swallowed.
- Growth: a new tag is one roster entry (`latest: 1`, empty steps); a new version is one step; a family-wide reshape is still per-tag steps — the fold never widens.
- Law: an unknown tag in the log is a defect at read (`Option.none` from `latest`) only when the family truly dropped a tag — the sanctioned path for tag retirement is keeping a tombstone member in the union, so the plan stays total over everything ever written.
- Law: the lift is `Array.reduce` over `steps.slice(version - 1)` — versions already at `latest` fold through zero steps, so hot reads pay one array slice and one decode; no memo table exists because the fold is allocation-light and the decode dominates.
- Law: the testkit law combinators prove totality per chain — every `(tag, version)` pair present in the corpus composes to a decodable value; the proof lives in the tests estate, the page states the obligation as fact.
- Boundary: snapshot bodies ride `chain` with `snapshot_schema_version` as the version coordinate — `journal/snapshot.md` composes it; the C#-minted `SnapshotHeader`/`CrdtOpWire` arrive already decoded through `wire` and never re-enter this fold.

```typescript
import { Array, Effect, Option, Record, Schema, type ParseResult } from "effect"

declare namespace Upcast {
  type Plan<A> = {
    readonly latest: (tag: string) => Option.Option<number>
    readonly decode: (raw: Raw) => Effect.Effect<A, ParseResult.ParseError>
  }
  type Lift<A> = {
    readonly latest: number
    readonly decode: (version: number, payload: unknown) => Effect.Effect<A, ParseResult.ParseError>
  }
}

const _lift = (chain: Upcast.Chain, version: number, payload: unknown): unknown =>
  Array.reduce(chain.steps.slice(version - 1), payload, (held, step) => step(held))

const _body = (column: unknown): unknown =>
  typeof column === "string" ? JSON.parse(column) : column

const Upcast = {
  body: _body,
  plan: <A, I>(family: Schema.Schema<A, I>, roster: Upcast.Roster): Upcast.Plan<A> => {
    const chains = Record.map(roster, (chain, tag) => _sized(tag, chain))
    const admit = Schema.decodeUnknown(family)
    return {
      latest: (tag) => Option.map(Record.get(chains, tag), (chain) => chain.latest),
      decode: (raw) =>
        Option.match(Record.get(chains, raw.tag), {
          onNone: () => Effect.die(new Error(`<upcast-unknown-tag:${raw.tag}>`)),
          onSome: (chain) => admit(_lift(chain, raw.version, raw.payload)),
        }),
    }
  },
  chain: <A, I>(shape: Schema.Schema<A, I>, spec: Upcast.Chain): Upcast.Lift<A> => {
    const chain = _sized("<snapshot>", spec)
    const admit = Schema.decodeUnknown(shape)
    return {
      latest: chain.latest,
      decode: (version, payload) => admit(_lift(chain, version, payload)),
    }
  },
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Upcast }
```
