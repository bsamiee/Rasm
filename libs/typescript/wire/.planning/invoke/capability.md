# [WIRE_CAPABILITY]

`invoke/capability.ts` binds the capability SDK: the C# `SdkTarget.TypeScript` generator emits the `DescService` descriptors (`Rasm.AppHost/Agent`), the `CapabilityDescriptor` names each capability with its content-keyed command shape, and this module derives the Effect-typed SDK from the descriptor alone — unary methods become `Effect<O, FaultDetail>` calls, server-streaming methods become `Stream<O, FaultDetail>` feeds, per-method retry budgets attach where the method's idempotency admits them — with zero hand-written client methods. Admission is by content key: a descriptor whose key diverges from the pinned emit refuses before any call exists, so the SDK cannot drift from the service it was generated against.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                  |
| :-----: | :----------------- | :------------------------------------------------------------------------------ |
|   [1]   | `DESCRIPTOR_ADMIT` | the `CapabilityDescriptor` decode and the content-key admission gate              |
|   [2]   | `SDK_BIND`         | the descriptor-derived Effect SDK: mapped signatures, kind dispatch, budgets      |

## [2]-[DESCRIPTOR_ADMIT]

- Owner: `CapabilityDescriptor` — the decoded capability identity: name, the emitted service's qualified name, the content key of the command shape, the mint instant; and the admission static that compares the runtime-shipped descriptor key against the build-pinned one.
- Entry: `CapabilityDescriptor.admit(octets, pinned)` — decode plus key gate: a diverging key refuses with `parity` evidence carrying both keys, because a capability whose command shape moved is a different capability, not a version of the same one.
- Growth: a new capability is a new emitted descriptor plus its generated service — this module's shapes are capability-agnostic and never enumerate capabilities.
- Law: content-keyed admission (AH:52) — the key covers the command shape's canonical bytes; equality proves the TS SDK and the C# host hold one contract, and the check runs once at bind time, never per call.
- Boundary: the emitted `_pb.ts` service consts are build artifacts imported by the app's capability modules; the descriptor wire family is census row `CapabilityDescriptorWire`.

```typescript
import { ContentKey } from "@rasm/ts/kernel"
import { Effect, Option, type ParseResult, Schema } from "effect"
import { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "../codec/proto.ts"

class CapabilityDescriptor extends Schema.Class<CapabilityDescriptor>("CapabilityDescriptor")({
  name: Schema.NonEmptyString,
  service: Schema.NonEmptyString,
  key: ContentKey.FromCell,
  minted: Schema.DateTimeUtc,
}) {
  static readonly FromBytes: Schema.Schema<CapabilityDescriptor, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.CapabilityDescriptorWire, CapabilityDescriptor)
  static readonly admit = (octets: Uint8Array, pinned: ContentKey): Effect.Effect<CapabilityDescriptor, ParseResult.ParseError | WireFault> =>
    Effect.gen(function* () {
      const descriptor = yield* Schema.decodeUnknown(CapabilityDescriptor.FromBytes)(octets)
      return ContentKey.same(descriptor.key, pinned)
        ? descriptor
        : yield* new WireFault({
            family: "CapabilityDescriptorWire",
            reason: "parity",
            detail: `<capability-shape-moved:${descriptor.name}>`,
            evidence: Option.some({ actual: descriptor.key, expected: pinned }),
          })
    })
}
```

## [3]-[SDK_BIND]

- Owner: `Capability` — the SDK derivation: the `Sdk<T>` mapped type computing every method's Effect signature from the promise `Client<T>`, and the bind fold that walks `service.methods` by `methodKind`, wrapping each through `Invoke`'s lifts; per-method budgets arrive as an optional row table keyed by the method's `localName`.
- Entry: `Capability.bind(service, budgets?)` — `Effect<Sdk<T>, never, Invoke>`: one call derives the whole typed SDK; a method with a budget row composes `Invoke.retrying` around its lift, a method without one never retries (the safe default for non-idempotent verbs).
- Receipt: the SDK is the capability's whole callable surface — every method typed by the descriptor, every failure a `FaultDetail`; a hand-authored client method beside it is the drift defect the emit exists to kill.
- Growth: a new method on the emitted service appears in the SDK at regeneration with zero edits here; a method gaining idempotency is one budget row at the caller.
- Law: the derivation is kind-total over the shipped axis — `unary` and `server_streaming` bind; `client_streaming` and `bidi_streaming` refuse at bind time as drift evidence because the C# emitter does not mint them, and silence over an unbindable method would strand its caller at runtime.
- Law: signatures are computed, never restated — `Sdk<T>` maps `Client<T>`'s own member types into Effect carriers, so the SDK's type surface derives from the same descriptor the runtime binds; a parallel interface for a capability is the second-truth defect.
- Law: budgets attach at bind, visible in one table — retry policy per method is a value the binding call states; policy woven into call sites is unrecoverable and rejected.
- Boundary: the lifts, the fault fold, and the schedule compiler are `invoke/client.ts`'s; the `Client<T>` member shapes are `@connectrpc/connect`'s derivation; budget rows are `kernel/fault/budget` vocabulary.

```typescript
import type { Client } from "@connectrpc/connect"
import type { DescService } from "@bufbuild/protobuf"
import { Budget } from "@rasm/ts/kernel"
import { Effect, Stream } from "effect"
import type { FaultDetail } from "../fault/detail.ts"
import { Invoke } from "./client.ts"

declare namespace Capability {
  type Sdk<T extends DescService> = {
    readonly [K in keyof Client<T>]: Client<T>[K] extends (input: infer I, options?: infer _O) => Promise<infer O>
      ? (input: I) => Effect.Effect<O, FaultDetail>
      : Client<T>[K] extends (input: infer I, options?: infer _O) => AsyncIterable<infer O>
        ? (input: I) => Stream.Stream<O, FaultDetail>
        : never
  }
  type Budgets = Readonly<Record<string, Budget>>
}

const Capability: {
  readonly Descriptor: typeof CapabilityDescriptor
  readonly bind: <T extends DescService>(service: T, budgets?: Capability.Budgets) => Effect.Effect<Capability.Sdk<T>, WireFault, Invoke>
} = {
  Descriptor: CapabilityDescriptor,
  bind: <T extends DescService>(service: T, budgets?: Capability.Budgets) =>
    Effect.gen(function* () {
      const invoke = yield* Invoke
      const client = invoke.bind(service)
      const rows = yield* Effect.forEach(service.methods, (method) =>
        method.methodKind === "unary"
          ? Effect.succeed([
              method.localName,
              (input: unknown) => {
                const call = invoke.unary((signal, headers) => client[method.localName](input, { signal, headers }))
                const budget = budgets?.[method.localName]
                return budget === undefined ? call : Invoke.retrying(call, budget)
              },
            ] as const)
          : method.methodKind === "server_streaming"
            ? Effect.succeed([
                method.localName,
                (input: unknown) => invoke.stream((headers) => client[method.localName](input, { headers })),
              ] as const)
            : Effect.fail(
                new WireFault({
                  family: "CapabilityDescriptorWire",
                  reason: "drift",
                  detail: `<unbindable-kind:${method.methodKind}:${method.localName}>`,
                  evidence: Option.none(),
                }),
              ),
      )
      return Object.fromEntries(rows) as Capability.Sdk<T>
    }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Capability }
```

The terminal `Object.fromEntries` assembly is the page's one sanctioned assertion site: the mapped `Sdk<T>` type is computed from `Client<T>` while the runtime record is built from the descriptor's own `methods` walk — the two derive from one descriptor, and the assertion states that correspondence where the checker cannot carry it. The implementer confines it to this construction with the `// BOUNDARY ADAPTER` mark.
