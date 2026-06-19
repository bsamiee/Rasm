# [PLATFORM_FLAG_STREAM]

One page owns the streamed flag-delta ingress — `FlagStream`, the long-lived native `EventSource` SSE channel marshalled through the `runtime-composition/scoped-event-stream.md` bridge into a delta fold that patches the one `feature-flags/remote-config.md` `FlagSet` `SubscriptionRef` in place, decoding each frame once through `Schema.decodeUnknown`. The fixed-interval poll `Schedule` on `remote-config.md` demotes to the reconnect/backfill fallback under one retry policy, so SSE absence is graceful degradation, never a parallel flag source. A delta-frame flip propagates in seconds, never the next poll window; the `services`-owned bucket/variant vocabulary and the deterministic local bucket stay untouched. The page authors no flag vocabulary and crosses no command.

## [1]-[INDEX]

[FLAG_STREAM]: the SSE flag-delta ingress, the in-place cell patch, and the reconnect fold.

## [2]-[FLAG_STREAM]

- Owner: `FlagStream`, the streamed flag-delta owner — the native `EventSource` SSE ingress lifted through `scopedEventStream`, the `FlagDelta` `Schema.decodeUnknown` per-frame decode, and the in-place `SubscriptionRef.update` patch of the one `RemoteConfig` `FlagSet` cell. The `FlagSet` cell is owned by `remote-config.md` and a second flag cell patched here is the named parallel-source defect.
- Cases: `FlagStream` opens a long-lived `EventSource` against the `RuntimeConfig` `remoteConfigUrl` SSE endpoint, lifts its `message` events through `scopedEventStream` (the `EventSource` is an `EventTarget` absent from `WindowEventMap`, so the generic scoped bridge is the ingress, never `fromEventListenerWindow`), and folds each frame into the cell — `Schema.decodeUnknown(FlagDeltaSchema)` narrows the `MessageEvent.data` JSON once, and the decoded `FlagDelta` patches the `FlagSet` `ReadonlyMap` in place through `SubscriptionRef.update` (a `set` arm writes one flag's `FlagValue`, a `clear` arm removes one key), so a server-side flag flip swaps the one cell atomically and propagates without a reload; a decode failure folds to the same `FaultDetail.ConfigError` rail `remote-config.md` raises and retains the last-good `FlagSet`, never clearing flags.
- Auto: the `EventSource` is held as an `Effect.acquireRelease` resource closing the connection on scope exit, and the SSE ingress, the decode, and the patch fork `Effect.forkScoped` for the runtime lifetime; the reconnect/backfill rides one `Schedule` reusing the branch `projection` `StreamPolicy` reconnect shape rather than a bespoke retry, and the `remote-config.md` fixed-interval poll demotes to the backfill arm of that one policy — the SSE feed and the poll feed one fold, so an `EventSource` `error` reconnects through the policy and a prolonged outage backfills through the demoted poll, never two flag sources racing.
- Packages: `effect` `Schema.decodeUnknown` for the per-frame delta decode, `SubscriptionRef.update` for the in-place cell patch, `Schedule` for the reconnect/backfill, and `Effect.acquireRelease`/`Effect.forkScoped` for the scoped `EventSource`; `runtime-composition/scoped-event-stream.md` `scopedEventStream` for the SSE `message` ingress; the browser-native `EventSource` for the SSE channel; `RuntimeConfig` for the stream endpoint.
- Growth: a new delta operation lands as one `FlagDelta` `Data.TaggedEnum` arm and one patch fold arm, never a parallel ingress; a `WebTransport`-carried delta leg lands as one ingress source feeding the same patch fold, the cell unchanged; the reconnect cadence lands as one row on the reused `StreamPolicy` shape.
- Boundary: `FlagStream` patches the one `RemoteConfig.flags` cell `remote-config.md` owns — the `FlagSet` shape and the `services`-owned bucket/variant vocabulary stay settled, never re-authored — so a re-declared `FlagSet`/bucket axis is the named anti-spam defect; the deterministic local bucket evaluation in `remote-config.md` is untouched; the delta decode failure folds to `FaultDetail.ConfigError` and retains the last-good set; `FlagStream` emits no command, dials no transport beyond the SSE channel, and a flag value reaches a component through the `ui` `AtomBinding`, never a second state binding.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { FlagKey, FlagSet, FlagValue } from "./remote-config.ts";
import { Data, Effect, Schedule, Schema, Stream, SubscriptionRef } from "effect";
import { FlagKeySchema, FlagValueSchema, RemoteConfig } from "./remote-config.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";
import { scopedEventStream } from "../runtime-composition/scoped-event-stream.ts";

// --- [TYPES] ---------------------------------------------------------------------------
const FlagDeltaSchema = Schema.Union(
  Schema.Struct({ _tag: Schema.Literal("set"), key: FlagKeySchema, value: FlagValueSchema }),
  Schema.Struct({ _tag: Schema.Literal("clear"), key: FlagKeySchema }),
);
type FlagDelta = typeof FlagDeltaSchema.Type;

// --- [SERVICES] ------------------------------------------------------------------------
interface FlagStream {
  readonly run: Effect.Effect<void>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
const patchFlagSet = (set: FlagSet, delta: FlagDelta): FlagSet => {
  const flags = new Map<FlagKey, FlagValue>(set.flags);
  delta._tag === "set" ? flags.set(delta.key, delta.value) : flags.delete(delta.key);
  return { flags, fetchedAt: Date.now() };
};

const sseFrames = (endpoint: string): Stream.Stream<MessageEvent, never, never> =>
  Stream.unwrapScoped(
    Effect.acquireRelease(
      Effect.sync(() => new EventSource(endpoint)),
      (source) => Effect.sync(() => source.close()),
    ).pipe(
      Effect.map((source) =>
        scopedEventStream<MessageEvent, (event: MessageEvent) => void>(
          (emit) => {
            const fn = (event: MessageEvent): void => emit(event);
            source.addEventListener("message", fn);
            return fn;
          },
          (fn) => source.removeEventListener("message", fn),
        ),
      ),
    ),
  );

// --- [COMPOSITION] ---------------------------------------------------------------------
class FlagStreamLive extends Effect.Service<FlagStreamLive>()("@rasm/ts/platform/FlagStream", {
  scoped: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const remote = yield* RemoteConfig;
    const endpoint = yield* config.remoteConfigUrl;
    const run = sseFrames(endpoint).pipe(
      Stream.mapEffect((event) =>
        Schema.decodeUnknown(FlagDeltaSchema)(JSON.parse(event.data)).pipe(
          Effect.flatMap((delta) => SubscriptionRef.update(remote.flags, (set) => patchFlagSet(set, delta))),
          Effect.catchAll(() => Effect.void),
        ),
      ),
      Stream.runDrain,
      Effect.retry(Schedule.exponential("1 second").pipe(Schedule.jittered)),
      Effect.forkScoped,
    );
    return { run } satisfies FlagStream;
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type FlagStream, FlagStreamLive };
```
