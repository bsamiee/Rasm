# [EDGE_ADMIT]

`hook/admit.ts` is webhook admission: one pipeline turns a verified inbound delivery into an enqueued `Hook` with a receipt, and every capability the edge ledger forbids importing arrives as a port — `ReplayLedger` (first-claim identity, memory Layer shipped, durable Layer at the app root), `QuotaGate` (intake quota typed against edge-declared verdicts, the `work` fenced-quota rows satisfy it at the root), and `HookIngress` (the enqueue seam a `work` queue or `store` journal Layer satisfies). The pipeline is fixed order — verify, identify, claim, quota, enqueue — because each gate is cheaper or more forgeable than the next: authenticity first (unsigned traffic never touches state), identity claim before quota (a replay must not burn budget), enqueue last (nothing durable happens for a refused delivery). The `held` octets thread verbatim from the verify seam into the enqueued hook — the byte-identity law end to end — and the 202 receipt is evidence, not the processed outcome: processing is `work`'s, behind the port.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                             | [PUBLIC]                                |
| :-----: | :------------- | :-------------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | [HOOK_MODEL]   | the admitted-hook owner and its receipt                                | `Hook`, `HookReceipt`                    |
|  [02]   | [PORT_TAGS]    | replay, quota, and ingress ports with the shipped memory ledger        | `ReplayLedger`, `QuotaGate`, `HookIngress` |
|  [03]   | [INTAKE_FOLD]  | the five-gate admission pipeline and its per-source spec               | `Intake`                                 |

## [2]-[HOOK_MODEL]

[HOOK_MODEL]:
- Owner: `Hook` — the admitted delivery as one `Schema.Class`: `source` (the intake's own name for the provider), `id` (the provider's delivery identity — the replay key), `at` (admission instant), `verified` (the `Verified` receipt from the signature fold, composed at full depth so freshness evidence travels), `held` (the verbatim octets, `Uint8ArrayFromBase64` on the encoded side so the interior stays byte-identical while the CLI capture file, the replay verb, and any enqueue Layer share the one derived wire twin), and `tenant` as `Option` for multi-tenant intakes.
- Law: `HookReceipt` is the 202 body — `id`, `source`, `at`, and the `lane` the ingress Layer reports (which queue or journal band accepted it) — enough for the caller to correlate a later outcome, nothing about processing, because admission is not execution.
- Law: no content key is minted here — content addressing has exactly one kernel mint and three delegating sites (invariant 2), and a webhook body is not one of them; the replay key is the PROVIDER's delivery identity, which is the deduplication contract providers themselves document.
- Packages: `effect` (`Schema`, `Option`); `hook/verify` (`Verified`).

```typescript
import type { Crypto } from "@rasm/ts/security"
import { Context, Data, DateTime, Duration, Effect, HashMap, Layer, Option, type Redacted, Ref, Schema } from "effect"
import { HookFault, Signature, Verified } from "./verify.ts"

class Hook extends Schema.Class<Hook>("Hook")({
  source: Schema.NonEmptyString,
  id: Schema.NonEmptyString.pipe(Schema.maxLength(256)),
  at: Schema.DateTimeUtc,
  verified: Verified,
  held: Schema.Uint8ArrayFromBase64,
  tenant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {}

class HookReceipt extends Schema.Class<HookReceipt>("HookReceipt")({
  id: Schema.NonEmptyString,
  source: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  lane: Schema.NonEmptyString,
}) {}
```

## [3]-[PORT_TAGS]

[PORT_TAGS]:
- Owner: the three ports, each a `Context.Tag` against edge-declared shapes — a port exists exactly where the ledger forbids the import, and the app root wires every satisfier. `ReplayLedger.claim(source, id, window)` answers first-claim atomically (`true` exactly once per identity per window); `QuotaGate.admit(source, tenant)` answers a `QuotaVerdict`; `HookIngress.enqueue(hook)` accepts the admitted hook and reports its lane.
- Law: `QuotaVerdict` is a process-local tagged family — `Granted` and `Exhausted` carrying the refusal window — so the quota seam speaks verdicts, never booleans, and the fenced-quota semantics (who counts what, per which fencing token) stay entirely behind the `work`-satisfied Layer.
- Law: `ReplayLedger.memory` is the shipped single-node satisfier — one `Ref`-held table where `claim` sweeps expired marks and claims in the same atomic `Ref.modify`, so the check and the set cannot tear under concurrent duplicates; a fleet replaces it with a durable Layer at the root (the same Tag, an `INSERT ... ON CONFLICT`-shaped store row), and the pipeline is untouched.
- Law: `QuotaGate.open` is the shipped unlimited satisfier — admission is a selection, so an archetype without quota wiring still composes the pipeline and the gate answers `Granted`; there is deliberately no default for `HookIngress`, because an intake with nowhere to enqueue is a composition error the root proof must surface, not absorb.
- Growth: a new admission capability (a payload-inspection gate, a source kill-switch) is one new port row here plus one pipeline arm — the pattern is settled.
- Packages: `effect` (`Context`, `Layer`, `Ref`, `HashMap`, `DateTime`, `Duration`, `Data`).

```typescript
type QuotaVerdict = Data.TaggedEnum<{
  Granted: {}
  Exhausted: { readonly retryAfter: Duration.Duration }
}>
const QuotaVerdict: Data.TaggedEnum.Constructor<QuotaVerdict> = Data.taggedEnum<QuotaVerdict>()

class ReplayLedger extends Context.Tag("edge/hook/ReplayLedger")<ReplayLedger, {
  readonly claim: (source: string, id: string, window: Duration.Duration) => Effect.Effect<boolean>
}>() {
  static readonly memory: Layer.Layer<ReplayLedger> = Layer.effect(
    ReplayLedger,
    Effect.map(Ref.make(HashMap.empty<string, DateTime.Utc>()), (cells) => ({
      claim: (source, id, window) =>
        Effect.flatMap(DateTime.now, (now) =>
          Ref.modify(cells, (table) => {
            const key = `${source}:${id}`
            const live = HashMap.filter(table, (seen) => DateTime.lessThan(now, DateTime.addDuration(seen, window)))
            return HashMap.has(live, key)
              ? ([false, live] as const)
              : ([true, HashMap.set(live, key, now)] as const)
          })),
    })),
  )
}

class QuotaGate extends Context.Tag("edge/hook/QuotaGate")<QuotaGate, {
  readonly admit: (source: string, tenant: Option.Option<string>) => Effect.Effect<QuotaVerdict>
}>() {
  static readonly open: Layer.Layer<QuotaGate> = Layer.succeed(QuotaGate, { admit: () => Effect.succeed(QuotaVerdict.Granted()) })
}

class HookIngress extends Context.Tag("edge/hook/HookIngress")<HookIngress, {
  readonly enqueue: (hook: Hook) => Effect.Effect<HookReceipt, HookFault>
}>() {}
```

## [4]-[INTAKE_FOLD]

[INTAKE_FOLD]:
- Owner: `Intake` — the per-source spec and the one admission entry: `Intake.Spec` fixes a source's dialect row, its secret, its replay window, its signature tolerance, and `identify` — the per-provider fold from headers and held octets to the delivery identity (providers spell it differently: a header for most, a body field for some; the fold owns the difference so the pipeline never branches on provider). `Intake.admit(spec, headers, held, tenant)` runs the five gates in fixed order and returns the receipt.
- Law: gate order is admission economics — verify (authenticity, cheapest to forge past nothing), identify (no identity, no admission), replay claim (`replayed` without burning quota), quota (`quota` carrying the verdict's own window as the fault's `retryAfter`), enqueue (the only durable step) — and any refusal leaves the process state exactly as it found it except the replay claim, which is deliberately sticky: a delivery refused AFTER its claim was taken is a provider retry the ledger already recognizes, and providers retry with the same identity by contract.
- Law: the endpoint seam above this fold reads raw body bytes under the serve row's admission ceiling and hands `held` verbatim — the pipeline is transport-free by signature, so the same admission serves an `HttpApiEndpoint` contribution, a raw router mount, or a replayed capture from the CLI ops family (`cli/verb.ts` replays a captured `Hook` straight into `HookIngress.enqueue`).
- Law: the requirement channel is the whole wiring story — `Crypto | ReplayLedger | QuotaGate | HookIngress` — so the composition root proves the intake by eliminating four Tags, and every archetype substitution (memory ledger in specs, open quota in a CLI tool) is Layer provision.
- Boundary: signed EGRESS webhooks are `work/deliver`'s and never appear here; what happens to an enqueued hook is the satisfier's; the quota fencing semantics live behind `QuotaGate`.
- Packages: `effect` (`Effect`, `DateTime`, `Option`, `Duration`, `Redacted`); `security/sign/crypto` (`Crypto` via the verify fold); `hook/verify` (`Signature`, `HookFault`).

```typescript
declare namespace Intake {
  type Spec = {
    readonly source: string
    readonly dialect: Signature.Dialect
    readonly secret: Redacted.Redacted<Uint8Array>
    readonly tolerance: Duration.Duration
    readonly window: Duration.Duration
    readonly identify: (headers: Readonly<Record<string, string | undefined>>, held: Uint8Array) => Option.Option<string>
  }
}

const _admit = (
  spec: Intake.Spec,
  headers: Readonly<Record<string, string | undefined>>,
  held: Uint8Array,
  tenant: Option.Option<string>,
): Effect.Effect<HookReceipt, HookFault, Crypto | ReplayLedger | QuotaGate | HookIngress> =>
  Effect.gen(function* () {
    const verified = yield* Signature.verify(spec.dialect, spec.secret, held, headers, spec.tolerance)
    const id = yield* Option.match(spec.identify(headers, held), {
      onNone: () => Effect.fail(new HookFault({ reason: "malformed", detail: "delivery identity absent", retryAfter: Option.none() })),
      onSome: Effect.succeed,
    })
    const ledger = yield* ReplayLedger
    const first = yield* ledger.claim(spec.source, id, spec.window)
    yield* Effect.when(
      Effect.fail(new HookFault({ reason: "replayed", detail: id, retryAfter: Option.none() })),
      () => !first,
    )
    const gate = yield* QuotaGate
    const verdict = yield* gate.admit(spec.source, tenant)
    yield* QuotaVerdict.$match(verdict, {
      Granted: () => Effect.void,
      Exhausted: ({ retryAfter }) =>
        Effect.fail(new HookFault({ reason: "quota", detail: spec.source, retryAfter: Option.some(retryAfter) })),
    })
    const now = yield* DateTime.now
    const ingress = yield* HookIngress
    return yield* ingress.enqueue(new Hook({ source: spec.source, id, at: now, verified, held, tenant }))
  })

const Intake: {
  readonly admit: typeof _admit
} = { admit: _admit }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hook, HookIngress, HookReceipt, Intake, QuotaGate, QuotaVerdict, ReplayLedger }
```
