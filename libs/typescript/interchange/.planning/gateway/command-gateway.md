# [INTERCHANGE_COMMAND_GATEWAY]

The outbound command-dial face: `CommandGateway` over the control verbs reading the `projection` `AvailabilityStore` fold as a dial-time gate, plus `IntentRegistry`, the deep-link key vocabulary the gateway resolves. The gateway is generated-client unary calls plus a receipt fold — wire-consumption, not state — co-located with the transport-owning domain so `@connectrpc/*` never leaks into the fold tier and the transport-free interior holds. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page binds only those fences and authors no shape.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                       |
| :-----: | :-------------- | :---------------------------------------------------------- |
|   [1]   | COMMAND_GATEWAY | the outbound verb gateway, the dial-time gate, the intent registry |
|   [2]   | TS_PROJECTION   | the command, availability, and receipt wire shapes the gateway binds |

## [2]-[COMMAND_GATEWAY]

- Owner: `CommandGateway`, the single gateway over the control verbs as the outbound-dial face of `transport/transport.md` `WireClients`, and `IntentRegistry`, the deep-link key vocabulary the gateway resolves. The gateway is generated-client unary calls and a receipt fold, co-located with the transport-owning domain because dialing dispatch is its outbound-effect surface even though it reads the availability fold, never folded into the host-free `projection` interior.
- Cases: `CommandGateway` turns a UI intent into a unary call and folds the receipt back toward the `projection` `AvailabilityStore` fold (an intra-package module edge, the store provided through the `invoke` `R` channel); the three verbs — capture a support bundle, set a degradation level, reload options — dial the control-service verbs on `csharp:Rasm.Compute/remote/remote#TS_PROJECTION` and the support capture verb against `csharp:Rasm.AppHost/observability/support-bundles#TS_PROJECTION`, payloads and outcomes carried by the command wire shapes against `csharp:Rasm.AppUi/commands/commands-availability#TS_PROJECTION`; each dial runs through `Effect.tryPromise` whose `catch` is the infallible `faults/fault-family.md` `fromConnect` fold, so the dial surfaces the typed `FaultDetail` the rail reconstructs rather than swallowing the `ConnectError` as an opaque defect, and a cause carrying no trailer lands the typed `FaultDetail.Quarantine` case inside the same fold rather than a downstream re-catch. `IntentRegistry` addresses command intents by stable string keys against `csharp:Rasm.AppUi/commands/commands-availability#TS_PROJECTION` so deep links survive a reload; a resolved intent dispatches through `CommandGateway` carrying its payload.
- Entry: `CommandGateway` reads the `AvailabilityStore` fold as a gate so a disabled command never fires and holds no domain state of its own — the `AvailabilityStore` requirement rides the `invoke` `R` channel explicitly so the capability is never silently dropped, the fold provided at the SPA composition root, not closed over here; intents resolve from the `ui` `DeepLinkBinding` query string, the key the stable identifier never a re-derived display string.
- Packages: `@connectrpc/connect-web` for the unary dial, `@bufbuild/protobuf` for the request message construction, and `effect` for the `Match` primitive and the gateway-and-registry `Effect.Service` composition.
- Growth: a new control verb lands as one `CommandGateway` method, never a sibling gateway; a new addressable intent lands as one `IntentRegistry` key bound to one gateway verb.
- Boundary: a second gateway beside `CommandGateway` is the named defect, and the gateway reads availability as a gate but never holds it — `AvailabilityStore` lives in `projection`; a transport interceptor reading `AuthSession` is the only credential path and `interchange` owns no session state; the branch stamps no tenancy onto an outbound call — tenancy is a server-minted receipt dimension the TS side only reads.

```ts contract
type ControlVerb = "captureSupport" | "setDegradation" | "reloadOptions";

interface CommandGateway {
  readonly invoke: (verb: ControlVerb, payload: CommandPayloadWire) => Effect.Effect<CommandReceiptWire, FaultDetail, AvailabilityStore>;
}

interface IntentRegistry {
  readonly resolve: (key: string) => Option.Option<{ readonly verb: ControlVerb; readonly payload: CommandPayloadWire }>;
}

class CommandGatewayLive extends Effect.Service<CommandGatewayLive>()("@rasm/ts/interchange/CommandGateway", {
  effect: Effect.gen(function* () {
    const clients = yield* WireClients;
    const dial = (call: () => Promise<CommandReceiptWire>) =>
      Effect.tryPromise({ try: call, catch: faultDetailRail.fromConnect });
    const invoke = (verb: ControlVerb, payload: CommandPayloadWire) =>
      Effect.flatMap(AvailabilityStore, (availability) =>
        availability.isEnabled(verb).pipe(
          Effect.filterOrFail((ok) => ok, () => FaultDetail.HopFault({ code: "command-disabled", evidence: { verb } })),
          Effect.flatMap(() =>
            Match.value(verb).pipe(
              Match.when("captureSupport", () => dial(() => clients.control.captureSupport(payload))),
              Match.when("setDegradation", () => dial(() => clients.control.setDegradation(payload))),
              Match.when("reloadOptions", () => dial(() => clients.control.reloadOptions(payload))),
              Match.exhaustive,
            )),
        ));
    return { invoke } satisfies CommandGateway;
  }),
}) {}
```

## [3]-[TS_PROJECTION]

- Owner: the command, availability, and receipt wire shapes the gateway binds — transcribed from `csharp:Rasm.AppUi/commands/commands-availability#TS_PROJECTION` and `csharp:Rasm.AppHost/ports/runtime-ports#TS_PROJECTION`; the diagnostics-evidence and support-bundle shapes ride their owning consumers.
- Entry: `CommandPayloadWire` is the 4-case payload union, `CommandReceiptWire` binds as the `TPayload` on the `projection` `ReceiptEnvelopeCarrier`, and `CommandAvailabilityWire` is the gate row the `AvailabilityStore` folds.
- Packages: `effect` `Schema` for the codec surface.
- Growth: a new command payload case lands as one union arm; a new availability row lands as one field; the branch authors no shape absent from the C# fence.
- Boundary: every shape transcribes a C# `#TS_PROJECTION` fence verbatim.

```ts contract
const CommandPayloadWire = Schema.Union(
  Schema.Struct({ _tag: Schema.Literal("none") }),
  Schema.Struct({ _tag: Schema.Literal("single"), value: Schema.String }),
  Schema.Struct({ _tag: Schema.Literal("many"), values: Schema.Array(Schema.String) }),
  Schema.Struct({ _tag: Schema.Literal("text"), text: Schema.String }),
);
type CommandPayloadWire = Schema.Schema.Type<typeof CommandPayloadWire>;

const CommandOutcomeWire = Schema.Literal("completed", "cancelled", "rejected", "faulted");

const CommandAvailabilityWire = Schema.Struct({
  key: Schema.String,
  available: Schema.Boolean,
  level: Schema.String,
});
type CommandAvailabilityWire = Schema.Schema.Type<typeof CommandAvailabilityWire>;

const CommandReceiptWire = Schema.Struct({
  intentKey: Schema.String,
  outcome: CommandOutcomeWire,
  payload: CommandPayloadWire,
});
type CommandReceiptWire = Schema.Schema.Type<typeof CommandReceiptWire>;
```
