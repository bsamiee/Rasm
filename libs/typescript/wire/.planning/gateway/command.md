# [WIRE_COMMAND]

`gateway/command.ts` dispatches decoded `CommandPayloadWire` verbs from `Rasm.AppUi/Shell` through the availability gate: the payload decodes once (verb, body, tenant, stamp), the gate port — typed against `state/evidence/availability`'s `CommandAvailability`/`DegradationLevel` vocabulary — answers whether the verb's floor is met under the current degradation level, and the verb row's handler runs only on a grant. Handlers arrive as an app-supplied row table through the Layer factory, each row carrying its availability floor as a policy column, so the gateway is one generic dispatch surface and an app's verb set is data. Refusal is an outcome value with the level as evidence, never a fault; an unknown verb is drift evidence, because the shell only emits verbs it was built against.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                     |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|   [1]   | `PAYLOAD_DISPATCH`  | the decoded payload, the handler row contract, the `Gateway` dispatch service   |
|   [2]   | `AVAILABILITY_GATE` | the gate port typed against `state` vocabulary and the grant/refuse fold        |

## [2]-[PAYLOAD_DISPATCH]

- Owner: `CommandPayload` — the decoded command class (verb, body carriage, tenant, `Hlc` stamp) — and `Gateway`, the `Effect.Service` whose Layer factory takes the verb row table: `{ [verb]: { floor, handle } }`, the handler and its availability floor one row, contract-checked at the factory call.
- Entry: `Gateway.submit(octets)` — decode, gate, dispatch, receipt in one rail; the return is the `Dispatched` outcome family: `Granted` carrying the handler's receipt, `Refused` carrying the level evidence.
- Receipt: `Dispatched` is process-local outcome vocabulary — `Granted { verb, receipt }`, `Refused { verb, level, floor }` — the shell's optimistic UI resolves against it and telemetry counts refusals by level.
- Growth: a new verb is one row in the app's table — floor and handler land together, the mapped contract makes a missing handler a compile error at the factory call, and this module never changes; a new outcome kind is one tagged case every exhaustive consumer breaks on.
- Law: the row table is the dispatch — one generic indexed lookup, the mapped annotation keeping per-verb payload flow cast-free; a `switch` over verbs, a second dispatch path, or a handler reached around the gate is rejected.
- Law: an unknown verb is `drift` evidence — the shell and the app were built against one verb set, so a miss means generational skew; the fault carries the verb and the known set's size, and the quarantine policy row routes it.
- Law: handlers receive the decoded payload and the tenant — never raw octets, never a re-decode; the body's per-verb shape is the handler's own admission (each row decodes its body with its own schema, the second-admission law for nested unknown bands).
- Boundary: the availability vocabulary and its evidence folds are `state/evidence/availability`; the gate Layer is provided at the app root from state-fed evidence; support-plane capture rides `gateway/support.ts`.

```typescript
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import { CommandAvailability, DegradationLevel } from "@rasm/ts/state"
import { Data, Effect, Option, type ParseResult, Schema } from "effect"
import { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

class CommandPayload extends Schema.Class<CommandPayload>("CommandPayload")({
  verb: Schema.NonEmptyString,
  body: Schema.Unknown,
  tenant: TenantContext,
  stamp: Hlc.FromStamp,
}) {
  static readonly FromBytes: Schema.Schema<CommandPayload, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.CommandPayloadWire, CommandPayload)
}

type Dispatched = Data.TaggedEnum<{
  Granted: { readonly verb: string; readonly receipt: unknown }
  Refused: { readonly verb: string; readonly level: DegradationLevel; readonly floor: DegradationLevel }
}>
const Dispatched: Data.TaggedEnum.Constructor<Dispatched> = Data.taggedEnum<Dispatched>()

declare namespace Gateway {
  type Row = {
    readonly floor: DegradationLevel
    readonly handle: (payload: CommandPayload) => Effect.Effect<unknown, WireFault>
  }
  type Handlers = Readonly<Record<string, Row>>
}

class Gateway extends Effect.Service<Gateway>()("wire/Gateway", {
  effect: (handlers: Gateway.Handlers) =>
    Effect.gen(function* () {
      const gate = yield* AvailabilityGate
      return {
        submit: (octets: Uint8Array): Effect.Effect<Dispatched, ParseResult.ParseError | WireFault> =>
          Effect.gen(function* () {
            const payload = yield* Schema.decodeUnknown(CommandPayload.FromBytes)(octets)
            const row = yield* Option.match(Option.fromNullable(handlers[payload.verb]), {
              onNone: () =>
                Effect.fail(
                  new WireFault({
                    family: "CommandPayloadWire",
                    reason: "drift",
                    detail: `<unknown-verb:${payload.verb}>`,
                    evidence: Option.none(),
                  }),
                ),
              onSome: Effect.succeed,
            })
            const level = yield* gate.current
            return yield* CommandAvailability.meets(level, row.floor)
              ? Effect.map(row.handle(payload), (receipt) => Dispatched.Granted({ verb: payload.verb, receipt }))
              : Effect.succeed(Dispatched.Refused({ verb: payload.verb, level, floor: row.floor }))
          }),
      }
    }),
  accessors: true,
}) {
  static readonly Payload: typeof CommandPayload = CommandPayload
  static readonly Outcome: Data.TaggedEnum.Constructor<Dispatched> = Dispatched
}
```

## [3]-[AVAILABILITY_GATE]

- Owner: `AvailabilityGate` — the `Context.Tag` port this module declares and the app root satisfies from `state`-fed evidence; the port's one member reads the current `DegradationLevel`.
- Entry: `yield* AvailabilityGate` inside the gateway construction; the app root provides `Layer.succeed(AvailabilityGate, ...)` projected from its availability evidence fold, or a live layer over `state/query/live`'s subscription.
- Growth: a richer gate (per-tenant levels, verb-class overrides) widens the port's member signature once — the gateway's grant fold reads the same vocabulary and the row's `floor` column is unchanged.
- Law: the gate types against `state` vocabulary, never re-declares it — `DegradationLevel` and the `meets` comparison are `state/evidence/availability`'s owners; the port is declared HERE because `wire` is the consumer and the ledger points `wire -> state`, while the LIVE evidence arrives at composition.
- Law: gating is read-then-dispatch, not subscribe-then-cache — each submit reads the gate; staleness policy belongs to the gate's providing Layer, so a cached level is the provider's stated decision, never this module's accident.
- Boundary: the shell's availability display consumes the same state vocabulary through its own path; this port carries exactly the gateway's read.

```typescript
import { Context } from "effect"

class AvailabilityGate extends Context.Tag("wire/AvailabilityGate")<AvailabilityGate, {
  readonly current: Effect.Effect<DegradationLevel>
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AvailabilityGate, Gateway }
```
