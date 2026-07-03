# [WIRE_COMMAND]

`gateway/command.ts` dispatches decoded `CommandPayloadWire` verbs from `Rasm.AppUi/Shell` through the availability gate: the payload decodes once (verb, body, tenant, stamp), the gate port — typed against `state/evidence/availability`'s `Availability.Verdict` vocabulary — answers the verb's verdict under the current degradation evidence, and the verb's handler runs only on an `Available` verdict. Handlers arrive as an app-supplied row table through the Layer factory, so the gateway is one generic dispatch surface and an app's verb set is data. Refusal is an outcome value carrying the state verdict as evidence, never a fault; an unknown verb is drift evidence, because the shell only emits verbs it was built against.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                     |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|   [1]   | `PAYLOAD_DISPATCH`  | the decoded payload, the handler row contract, the `Gateway` dispatch service   |
|   [2]   | `AVAILABILITY_GATE` | the gate port typed against `state` vocabulary and the grant/refuse fold        |

## [2]-[PAYLOAD_DISPATCH]

- Owner: `CommandPayload` — the decoded command class (verb, body carriage, tenant, `Hlc` stamp) — and `Gateway`, the `Effect.Service` whose Layer factory takes the verb row table: `{ [verb]: handle }`, the app's verb set as data, supplied once at the factory call.
- Entry: `Gateway.submit(octets)` — decode, gate, dispatch, receipt in one rail; the return is the `Dispatched` outcome family: `Granted` carrying the handler's receipt, `Refused` carrying the state verdict as evidence.
- Receipt: `Dispatched` is process-local outcome vocabulary — `Granted { verb, receipt }`, `Refused { verb, verdict }` — the shell's optimistic UI resolves against it and telemetry counts refusals by the verdict's own reason rows.
- Growth: a new verb is one row in the app's table — the table is data, so the gateway never changes; a new outcome kind is one tagged case every exhaustive consumer breaks on.
- Law: the row table is the dispatch — one indexed lookup over the app-supplied record; a `switch` over verbs, a second dispatch path, or a handler reached around the gate is rejected.
- Law: an unknown verb is `drift` evidence — the shell and the app were built against one verb set, so a miss means generational skew; the fault names the verb, and the quarantine policy row routes it.
- Law: handlers receive the decoded payload — never raw octets, never a re-decode; the body's per-verb shape is the handler's own admission (each row decodes its body with its own schema, the second-admission law for nested unknown bands).
- Boundary: the availability vocabulary, the per-command verdict map, and the total `admits` fallback are `state/evidence/availability`'s owners; the gate Layer is provided at the app root from state-fed evidence; support-plane capture rides `gateway/support.ts`.

```typescript
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import type { Availability } from "@rasm/ts/state"
import { Context, Data, Effect, Option, type ParseResult, Schema } from "effect"
import { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

class CommandPayload extends Schema.Class<CommandPayload>("CommandPayload")({
  verb: Schema.NonEmptyString,
  body: Schema.Unknown,
  tenant: TenantContext,
  stamp: Hlc,
}) {
  static readonly FromBytes: Schema.Schema<CommandPayload, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.CommandPayloadWire, CommandPayload)
}

type Dispatched = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: unknown }
  Refused: { readonly verb: string; readonly verdict: Availability.Verdict }
}>
const Dispatched: Data.TaggedEnum.Constructor<Dispatched> = Data.taggedEnum<Dispatched>()

declare namespace Gateway {
  type Handlers = Readonly<Record<string, (payload: CommandPayload) => Effect.Effect<unknown, WireFault>>>
}

class Gateway extends Effect.Service<Gateway>()("wire/Gateway", {
  effect: (handlers: Gateway.Handlers) =>
    Effect.map(AvailabilityGate, (gate) => ({
      submit: (octets: Uint8Array): Effect.Effect<Dispatched, ParseResult.ParseError | WireFault> =>
        Effect.gen(function* () {
          const payload = yield* Schema.decodeUnknown(CommandPayload.FromBytes)(octets)
          const handle = yield* Option.match(Option.fromNullable(handlers[payload.verb]), {
            onNone: () =>
              new WireFault({
                family: "CommandPayloadWire",
                reason: "drift",
                detail: `<unknown-verb:${payload.verb}>`,
                evidence: Option.none(),
              }),
            onSome: Effect.succeed,
          })
          const verdict = yield* gate.admits(payload.verb)
          return yield* verdict._tag === "Available"
            ? Effect.map(handle(payload), (receipt) => Dispatched.Granted({ verb: payload.verb, receipt }))
            : Effect.succeed(Dispatched.Refused({ verb: payload.verb, verdict }))
        }),
    })),
  accessors: true,
}) {
  static readonly Payload: typeof CommandPayload = CommandPayload
  static readonly Outcome: Data.TaggedEnum.Constructor<Dispatched> = Dispatched
}
```

## [3]-[AVAILABILITY_GATE]

- Owner: `AvailabilityGate` — the `Context.Tag` port this module declares and the app root satisfies from `state`-fed evidence; the port's one member answers a verb's `Availability.Verdict`.
- Entry: `yield* AvailabilityGate` inside the gateway construction; the app root provides the Layer projected from its availability evidence fold — the projection composes `state`'s total `Availability.admits` read over the live folded snapshot, minting the command coordinate from the wire verb at the composition seam where both vocabularies are in scope.
- Growth: a richer gate (per-tenant snapshots, verb-class overrides) widens the port's member signature once — the gateway's grant fold reads the same verdict vocabulary unchanged.
- Law: the gate types against `state` vocabulary, never re-declares it — `Availability.Verdict` and the level-fallback posture are `state/evidence/availability`'s owners; the port is declared HERE because `wire` is the consumer and the ledger points `wire -> state`, while the LIVE evidence arrives at composition.
- Law: gating is read-then-dispatch, not subscribe-then-cache — each submit reads the gate; staleness policy belongs to the gate's providing Layer, so a cached verdict is the provider's stated decision, never this module's accident.
- Law: refusal keeps the verdict whole — `Gated` carries its reason and optional `until`, `Withheld` its level; the outcome family transports state evidence untouched, so telemetry and the shell read one vocabulary.
- Boundary: the shell's availability display consumes the same state vocabulary through its own path; this port carries exactly the gateway's read.

```typescript
class AvailabilityGate extends Context.Tag("wire/AvailabilityGate")<AvailabilityGate, {
  readonly admits: (verb: string) => Effect.Effect<Availability.Verdict>
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AvailabilityGate, Gateway }
```
