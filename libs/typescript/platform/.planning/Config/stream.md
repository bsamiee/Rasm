# [PLATFORM_STREAM]

One page owns the streamed flag-delta ingress — `FlagStream`, the long-lived SSE channel read as the raw `text/event-stream` body of a browser `HttpClient` streaming request, decoded to text and fed chunk-by-chunk into the admitted `@effect/experimental` `Sse.makeParser` SSE-frame decoder (which owns the `data:`/`event:`/`id:`/`retry:` line framing the native `EventSource` would otherwise hide), whose emitted `Event` frames fold through `Schema.decodeUnknown` into a delta that patches the one `Config/flags.md` `FlagSet` `SubscriptionRef` in place. The fixed-interval poll `Schedule` on `Config/flags.md` demotes to the reconnect/backfill fallback and the `Sse.Retry.lastEventId` cursor seeds the `Last-Event-ID` reattach header on reconnect, so SSE absence is graceful degradation and a delta missed during an outage backfills by event id rather than only the demoted poll, never a parallel flag source. The raw-stream ingress (rather than `EventSource`) is what lets `Sse.makeParser` own the frame parsing and the app own reconnect — an `EventSource` already parses its own frames and owns its own reconnect, so it cannot host the admitted parser. A delta-frame flip propagates in seconds, never the next poll window; the `services`-owned bucket/variant vocabulary and the deterministic local bucket stay untouched. The page authors no flag vocabulary and crosses no command.

## [1]-[INDEX]

- [1]-[FLAG_STREAM]: the `HttpClient` raw SSE-body ingress over the `Sse.makeParser` decoder, the in-place cell patch, and the `Retry.lastEventId` reconnect fold.

## [2]-[FLAG_STREAM]

- Owner: `FlagStream`, the streamed flag-delta owner — the `HttpClient` raw SSE-body ingress, the `@effect/experimental` `Sse.makeParser` SSE-frame decode over the decoded text chunks, the `FlagDelta` `Schema.decodeUnknown` per-frame decode of the parsed `Event.data`, the `Sse.Retry.lastEventId` reattach cursor, and the in-place `SubscriptionRef.update` patch of the one `RemoteConfig` `FlagSet` cell. The `FlagSet` cell is owned by `Config/flags.md` and a second flag cell patched here is the named parallel-source defect.
- Cases: `FlagStream` opens a long-lived `HttpClient.execute` streaming request against the `RuntimeConfig` `remoteConfigUrl` SSE endpoint (the `accept: text/event-stream` header and, on reconnect, the `Last-Event-ID` reattach header set from the cursor through `HttpClientRequest.setHeader`), reads the raw response body through `HttpClientResponse.stream` as `Stream<Uint8Array>`, decodes it to text through `Stream.decodeText`, and feeds each text chunk into one `Sse.makeParser(onParse)` `Parser` instance held for the connection lifetime — the admitted SSE-frame decoder owns the `data:`/`event:`/`id:`/`retry:` line framing the native `EventSource` would hide, emitting one `Sse.AnyEvent` per complete frame into a `Queue` the fold drains (so the parser's imperative `onParse` callback meets the effect world at one queue seam); an emitted `Sse.Event` carries the `data`/`id` the fold reads (`Schema.decodeUnknown(FlagDeltaSchema)` narrows the `Event.data` JSON once and the decoded `FlagDelta` patches the `FlagSet` `ReadonlyMap` in place through `SubscriptionRef.update` — a `set` arm writes one flag's `FlagValue`, a `clear` arm removes one key — so a server-side flag flip swaps the one cell atomically and propagates without a reload), and an emitted `Sse.Retry` records its `lastEventId` into the reattach cell so the next reconnect resumes from that event id; a decode failure folds to `Effect.void` and retains the last-good `FlagSet`, never clearing flags.
- Auto: the streaming response is a scoped resource releasing the connection on scope exit, the `Sse.makeParser` `Parser` is one instance per connection fed each decoded text chunk, and the SSE pump, the parse, the decode, and the patch fork `Effect.forkScoped` for the runtime lifetime; the reconnect/backfill rides the reused branch `projection` `StreamPolicy` reconnect `Schedule` (`policy.reconnect`) rather than a bespoke retry, and on reconnect the `lastEventId` cursor seeds the `Last-Event-ID` request header so a missed delta backfills by id; the `Config/flags.md` fixed-interval poll demotes to the backfill arm — the SSE feed and the poll feed one fold, so a stream error reconnects through the policy and a prolonged outage backfills through the demoted poll, never two flag sources racing.
- Packages: `@effect/experimental` `Sse.makeParser(onParse)` for the SSE-frame decode over the decoded text, `Sse.Event`/`Sse.Retry`/`Sse.AnyEvent` for the parsed frame family, and `Sse.Retry.lastEventId` for the reconnect cursor (the same surface the `services` `agent/mcp` consumes — `platform` is the second consumer); `@effect/platform` `HttpClient.execute`/`HttpClientRequest.get`/`setHeader`/`HttpClientResponse.stream` for the raw SSE-body streaming request and the `Last-Event-ID` reattach header; `effect` `Schema.decodeUnknown` for the per-frame delta decode, `SubscriptionRef.update` for the in-place cell patch, `Ref` for the `lastEventId` reattach cursor, `Queue.unbounded` for the parser-to-effect seam, `Stream.decodeText`/`Stream.merge`/`Stream.runForEach`/`Stream.execute`/`Stream.fromQueue`/`Stream.unwrap` for the byte-to-frame pipeline, and `Effect.retry`/`Effect.forkScoped` for the retried scoped session; the `projection` `StreamPolicy` `streamPolicy("interactive")` reconnect row; `RuntimeConfig` for the stream endpoint.
- Growth: a new delta operation lands as one `FlagDelta` `Data.TaggedEnum` arm and one patch fold arm, never a parallel ingress; a `WebTransport`-carried delta leg lands as one ingress source feeding the same `Sse.makeParser`/patch fold, the cell unchanged; the reconnect cadence lands as one row on the reused `StreamPolicy` shape.
- Boundary: `FlagStream` patches the one `RemoteConfig.flags` cell `Config/flags.md` owns — the `FlagSet` shape and the `services`-owned bucket/variant vocabulary stay settled, never re-authored — so a re-declared `FlagSet`/bucket axis is the named anti-spam defect; the deterministic local bucket evaluation in `Config/flags.md` is untouched; the `Sse.makeParser` parser is a transport-decode mechanic, not a second flag source; a per-frame delta decode failure folds to `Effect.void` and retains the last-good set (the one-frame drop never tears the stream, exactly as the SSE/wire/worker decode-failure discipline), while a stream-level `HttpClientError` ends the session for the policy reconnect; `FlagStream` emits no command, dials no transport beyond the SSE channel, and a flag value reaches a component through the `ui` `AtomBinding`, never a second state binding.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { FlagKey, FlagSet, FlagValue } from "./remote-config.ts";
import { Effect, Option, Queue, Ref, Schema, Stream, SubscriptionRef } from "effect";
import { Sse } from "@effect/experimental";
import { HttpClient, HttpClientError, HttpClientRequest, HttpClientResponse } from "@effect/platform";
import { FlagKeySchema, FlagValueSchema, RemoteConfig } from "./remote-config.ts";
import { RuntimeConfig } from "../runtime-config/runtime-config.ts";
import { streamPolicy } from "../../projection/fold/policy.ts";

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

// The browser EventSource owns its own SSE frame parsing and reconnect, so it cannot host the
// admitted Sse.makeParser — the parser reads the RAW data:/event:/id:/retry: byte stream. The
// ingress is therefore the HttpClient streaming response (the same discipline services
// agent/mcp consumes), decoded to text and fed chunk-by-chunk into one Parser.
const sseFrames = (endpoint: string, lastEventId: Option.Option<string>): Stream.Stream<Sse.AnyEvent, HttpClientError.HttpClientError, HttpClient.HttpClient> =>
  Stream.unwrap(
    Effect.gen(function* () {
      const queue = yield* Queue.unbounded<Sse.AnyEvent>();
      const parser = Sse.makeParser((event) => { Queue.unsafeOffer(queue, event); });
      const request = HttpClientRequest.get(endpoint).pipe(
        HttpClientRequest.setHeader("accept", "text/event-stream"),
        Option.match(lastEventId, { onNone: () => (r: HttpClientRequest.HttpClientRequest) => r, onSome: (id) => HttpClientRequest.setHeader("Last-Event-ID", id) }),
      );
      const client = yield* HttpClient.HttpClient;
      const pump = HttpClientResponse.stream(client.execute(request)).pipe(
        Stream.decodeText(),
        Stream.runForEach((chunk) => Effect.sync(() => parser.feed(chunk))),
      );
      return Stream.fromQueue(queue).pipe(Stream.merge(Stream.execute(pump)));
    }),
  );

// --- [COMPOSITION] ---------------------------------------------------------------------
class FlagStreamLive extends Effect.Service<FlagStreamLive>()("@rasm/ts/platform/FlagStream", {
  scoped: Effect.gen(function* () {
    const config = yield* RuntimeConfig;
    const remote = yield* RemoteConfig;
    const client = yield* HttpClient.HttpClient;
    const endpoint = yield* config.remoteConfigUrl;
    const cursor = yield* Ref.make<Option.Option<string>>(Option.none());
    const policy = streamPolicy("interactive");

    const onEvent = (event: Sse.AnyEvent): Effect.Effect<void> =>
      Sse.Retry.is(event)
        ? Ref.set(cursor, Option.fromNullable(event.lastEventId))
        : Schema.decodeUnknown(Schema.parseJson(FlagDeltaSchema))(event.data).pipe(
            Effect.flatMap((delta) => SubscriptionRef.update(remote.flags, (set) => patchFlagSet(set, delta))),
            Effect.zipLeft(Option.match(Option.fromNullable(event.id), { onNone: () => Effect.void, onSome: (id) => Ref.set(cursor, Option.some(id)) })),
            Effect.catchAll(() => Effect.void),
          );

    const session: Effect.Effect<void, HttpClientError.HttpClientError, HttpClient.HttpClient> = Ref.get(cursor).pipe(
      Effect.flatMap((startId) =>
        sseFrames(endpoint, startId).pipe(Stream.runForEach(onEvent)),
      ),
    );

    const run = session.pipe(
      Effect.provideService(HttpClient.HttpClient, client),
      Effect.retry(policy.reconnect),
      Effect.forkScoped,
    );
    return { run } satisfies FlagStream;
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type FlagStream, FlagStreamLive };
```
